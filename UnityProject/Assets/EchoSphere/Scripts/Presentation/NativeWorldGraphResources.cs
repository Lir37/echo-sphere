using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldSurface
    {
        private enum GraphOperation {Clear,Geometry,Mask,Horizontal,Vertical,CacheCopy,Composite,Present}
        private sealed class GraphData
        {
            internal NativeWorldSurface World;
            internal GraphOperation Operation;
            internal TextureHandle Input,Output,CameraColor;
            internal int First,End;
            internal RectInt Area;
            internal Matrix4x4 View,Projection;
            internal Rect Viewport;
        }
        private readonly List<RectInt> _graphAreas=new List<RectInt>();
        internal int GraphResourceNodeCount {get;private set;}
        internal int GraphTransientShadowTextures {get;private set;}
        private void GraphNode(RenderGraph graph,GraphOperation operation,TextureHandle input,TextureHandle output,int first,int end,RectInt area,TextureHandle cameraColor,Matrix4x4 view,Matrix4x4 projection,Rect viewport)
        {
            string name=operation switch {GraphOperation.Clear=>"ECHO world clear",GraphOperation.Geometry=>"ECHO ordered geometry",GraphOperation.Mask=>"ECHO shadow mask",GraphOperation.Horizontal=>"ECHO shadow horizontal",GraphOperation.Vertical=>"ECHO shadow vertical",GraphOperation.CacheCopy=>"ECHO retained shadow write",GraphOperation.Composite=>"ECHO shadow composite",_=>"ECHO world UI copy"};
            using(var builder=graph.AddUnsafePass<GraphData>(name,out var data)){
                data.World=this;data.Operation=operation;data.Input=input;data.Output=output;data.First=first;data.End=end;data.Area=area;data.CameraColor=cameraColor;data.View=view;data.Projection=projection;data.Viewport=viewport;
                if(input.IsValid())builder.UseTexture(input,AccessFlags.Read);
                builder.UseTexture(output,operation==GraphOperation.Geometry||operation==GraphOperation.Composite?AccessFlags.ReadWrite:AccessFlags.WriteAll);
                if(cameraColor.IsValid())builder.UseTexture(cameraColor,AccessFlags.ReadWrite);
                // Exact ordered blending and global shader bindings remain
                // explicit barriers. Texture lifetimes are now graph resources.
                builder.AllowGlobalStateModification(true);builder.AllowPassCulling(false);
                builder.SetRenderFunc(static (GraphData pass,UnsafeGraphContext context)=>pass.World.ExecuteGraphNode(pass,CommandBufferHelpers.GetNativeCommandBuffer(context.cmd)));
            }
            GraphResourceNodeCount++;
        }
        internal void RecordResourceGraph(RenderGraph graph,TextureHandle cameraColor,Matrix4x4 view,Matrix4x4 projection,Rect viewport)
        {
            using var frame=FrameMarker.Auto();
            PrepareFrameGlyphs();
            ShadowPixelCount=0;ShadowPassCount=0;ShadowCompositePixelCount=0;ShadowCompositeCount=0;CulledBatchCount=0;ShadowCacheHits=0;GeometryDrawCalls=0;InstancedPrimitiveCount=0;GraphResourceNodeCount=0;GraphTransientShadowTextures=0;
            _shadowCache.Begin();_graphAreas.Clear();
            var target=graph.ImportTexture(GraphTarget);var ui=graph.ImportTexture(GraphUi);var none=default(TextureHandle);
            GraphNode(graph,GraphOperation.Clear,none,target,0,0,default,none,default,default,default);
            int first=0;
            for(int i=0;i<_batches.Count;i++){
                var batch=_batches[i];var area=batch.NativeMesh==null?default:ShadowArea(batch);bool culled=area.width<=0||area.height<=0;
#if UNITY_EDITOR
                if(DisableBatchCullingForValidation&&batch.NativeMesh!=null)culled=false;
#endif
                _graphAreas.Add(culled?default:area);if(culled){CulledBatchCount++;continue;}
                if(batch.Shadow.a<=0||batch.Blur<=0&&batch.X==0&&batch.Y==0||area.width<=0||area.height<=0)continue;
                if(first<i)GraphNode(graph,GraphOperation.Geometry,none,target,first,i,default,none,default,default,default);
                first=i;
                RenderTexture retained=null;var cacheResult=NativeWorldShadowCache.Result.Miss;
                bool cacheAllowed=batch.NativeMaterial.shader==_solidShader&&NativeWorldShapes.GeometryVersion(batch.NativeMesh)!=0&&batch.NativeProperties.HasVector(NativeColorId);
#if UNITY_EDITOR
                if(FullShadowViewportForValidation||DisableShadowCacheForValidation)cacheAllowed=false;
#endif
                if(cacheAllowed){var props=batch.NativeProperties;var key=new NativeWorldShadowCache.Key(batch.NativeMesh,NativeWorldShapes.GeometryVersion(batch.NativeMesh),batch.NativeMatrix,_viewport,area,props.GetVector(NativeColorId).w,props.GetFloat(NativeOpacityId),batch.Blur);cacheResult=_shadowCache.Probe(key,area.width,area.height,out retained);}
                var retainedHandle=retained!=null?graph.ImportTexture(_shadowCache.GraphHandle(retained)):none;
                TextureHandle shadow=retainedHandle;
                if(cacheResult!=NativeWorldShadowCache.Result.Hit){
                    var desc=new TextureDesc(area.width,area.height){colorFormat=_target.graphicsFormat,depthBufferBits=DepthBits.None,msaaSamples=MSAASamples.MSAA4x,bindTextureMS=false,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,name="ECHO transient shadow mask"};
                    var mask=graph.CreateTexture(desc);desc.msaaSamples=MSAASamples.None;desc.name="ECHO transient shadow horizontal";var ping=graph.CreateTexture(desc);desc.name="ECHO transient shadow vertical";var blur=graph.CreateTexture(desc);
                    GraphTransientShadowTextures+=3;ShadowPixelCount+=(long)area.width*area.height;ShadowPassCount++;
                    GraphNode(graph,GraphOperation.Mask,none,mask,i,i+1,area,none,default,default,default);
                    GraphNode(graph,GraphOperation.Horizontal,mask,ping,i,i+1,area,none,default,default,default);
                    GraphNode(graph,GraphOperation.Vertical,ping,blur,i,i+1,area,none,default,default,default);
                    if(cacheResult==NativeWorldShadowCache.Result.Write)GraphNode(graph,GraphOperation.CacheCopy,blur,retainedHandle,i,i+1,area,none,default,default,default);
                    shadow=retainedHandle.IsValid()?retainedHandle:blur;
                }else ShadowCacheHits++;
                GraphNode(graph,GraphOperation.Composite,shadow,target,i,i+1,area,none,default,default,default);
            }
            if(first<_batches.Count)GraphNode(graph,GraphOperation.Geometry,none,target,first,_batches.Count,default,none,default,default,default);
            GraphNode(graph,GraphOperation.Present,target,ui,0,0,default,cameraColor,view,projection,viewport);
        }
        private void ExecuteGraphNode(GraphData data,CommandBuffer commands)
        {
            var output=((RTHandle)data.Output).nameID;
            if(data.Operation==GraphOperation.Present){
                commands.SetGlobalInt(ShadowWeightCountId,0);commands.Blit(((RTHandle)data.Input).nameID,output,_presentMaterial,2);
                commands.SetRenderTarget(((RTHandle)data.CameraColor).nameID);commands.SetViewport(data.Viewport);commands.SetViewProjectionMatrices(data.View,data.Projection);
                GraphPending=false;GraphPassCount++;LastGraphFrame=Time.frameCount;return;
            }
            if(data.Operation==GraphOperation.CacheCopy){commands.CopyTexture(((RTHandle)data.Input).nameID,output);return;}
            if(data.Operation==GraphOperation.Horizontal||data.Operation==GraphOperation.Vertical){
                SetShadowKernel(commands,(float)VisualCanvasShadowViewport.Sigma(_batches[data.First].Blur));
                commands.SetGlobalVector(ShadowDirectionId,data.Operation==GraphOperation.Horizontal?new Vector4(1f/data.Area.width,0,0,0):new Vector4(0,1f/data.Area.height,0,0));
                commands.Blit(((RTHandle)data.Input).nameID,output,_blurMaterial,0);return;
            }
            commands.SetRenderTarget(output);
            if(data.Operation==GraphOperation.Composite){
                var batch=_batches[data.First];var area=data.Area;
                commands.SetGlobalColor(ShadowColorId,batch.Shadow);commands.SetGlobalVector(ShadowOffsetId,new Vector4(batch.X/area.width,-batch.Y/area.height,0,0));commands.SetGlobalVector(ShadowRectId,new Vector4(-(float)area.x/area.width,-(float)area.y/area.height,(float)_target.width/area.width,(float)_target.height/area.height));
                int mx=Mathf.CeilToInt(Mathf.Abs(batch.X))+2,my=Mathf.CeilToInt(Mathf.Abs(batch.Y))+2;
                int left=Mathf.Clamp(area.xMin-mx,0,_target.width),bottom=Mathf.Clamp(area.yMin-my,0,_target.height),right=Mathf.Clamp(area.xMax+mx,0,_target.width),top=Mathf.Clamp(area.yMax+my,0,_target.height);
                var rect=new Rect(left,bottom,right-left,top-bottom);
#if UNITY_EDITOR
                if(FullShadowCompositeForValidation)rect=new Rect(0,0,_target.width,_target.height);
#endif
                if(rect.width>0&&rect.height>0){commands.EnableScissorRect(rect);commands.Blit(((RTHandle)data.Input).nameID,output,_blurMaterial,batch.Blend=="lighter"?2:batch.Blend=="screen"?3:1);commands.DisableScissorRect();ShadowCompositeCount++;ShadowCompositePixelCount+=(long)rect.width*(long)rect.height;}return;
            }
            var projection=GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(_viewport.xMin,_viewport.xMax,_viewport.yMin,_viewport.yMax,-1,1),false);
            if(data.Operation==GraphOperation.Mask){
                var area=data.Area;float px=_viewport.width/_target.width,py=_viewport.height/_target.height;
                projection=GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(_viewport.xMin+area.xMin*px,_viewport.xMin+area.xMax*px,_viewport.yMin+area.yMin*py,_viewport.yMin+area.yMax*py,-1,1),false);
                commands.ClearRenderTarget(false,true,Color.clear);commands.SetViewProjectionMatrices(Matrix4x4.identity,projection);var batch=_batches[data.First];commands.DrawMesh(batch.NativeMesh,batch.NativeMatrix,batch.NativeMaterial,0,0,batch.NativeProperties);return;
            }
            commands.SetViewProjectionMatrices(Matrix4x4.identity,projection);
            if(data.Operation==GraphOperation.Clear){commands.ClearRenderTarget(false,true,Color.clear);return;}
            for(int i=data.First;i<data.End;i++){
                var batch=_batches[i];if(batch.NativeMesh==null||_graphAreas[i].width<=0||_graphAreas[i].height<=0)continue;int pass=batch.Blend=="lighter"?1:batch.Blend=="screen"?2:0;
                int instances=DrawInstanceRun(commands,i,pass,data.End);if(instances>0)i+=instances-1;else{commands.DrawMesh(batch.NativeMesh,batch.NativeMatrix,batch.NativeMaterial,0,pass,batch.NativeProperties);GeometryDrawCalls++;}
            }
        }
    }
}
