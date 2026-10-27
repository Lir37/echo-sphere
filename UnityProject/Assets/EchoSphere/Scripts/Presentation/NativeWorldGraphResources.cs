using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldSurface
    {
        private enum GraphOperation {Clear,Geometry,Mask,Horizontal,Vertical,CacheCopy,Composite,Present,AtlasMask,AtlasHorizontal,AtlasVertical}
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
        private readonly List<ShadowPlacement> _graphShadows=new List<ShadowPlacement>();
        internal int GraphResourceNodeCount {get;private set;}
        internal int GraphTransientShadowTextures {get;private set;}
        private void GraphNode(RenderGraph graph,GraphOperation operation,TextureHandle input,TextureHandle output,int first,int end,RectInt area,TextureHandle cameraColor,Matrix4x4 view,Matrix4x4 projection,Rect viewport)
        {
            string name=operation switch {GraphOperation.AtlasMask=>"ECHO shadow atlas masks",GraphOperation.AtlasHorizontal=>"ECHO shadow atlas horizontal",GraphOperation.AtlasVertical=>"ECHO shadow atlas vertical",GraphOperation.Clear=>"ECHO world clear",GraphOperation.Geometry=>"ECHO ordered geometry",GraphOperation.Mask=>"ECHO shadow mask",GraphOperation.Horizontal=>"ECHO shadow horizontal",GraphOperation.Vertical=>"ECHO shadow vertical",GraphOperation.CacheCopy=>"ECHO retained shadow write",GraphOperation.Composite=>"ECHO shadow composite",_=>"ECHO world UI copy"};
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
            _shadowCache.Begin();_graphAreas.Clear();_graphShadows.Clear();
            var target=graph.ImportTexture(GraphTarget);var ui=graph.ImportTexture(GraphUi);var none=default(TextureHandle);
            GraphNode(graph,GraphOperation.Clear,none,target,0,0,default,none,default,default,default);
            PrepareShadowPlans(graph);
            int first=0;
            for(int i=0;i<_batches.Count;i++){
                var batch=_batches[i];var plan=_shadowPlans[i];if(!plan.Active)continue;
                var placement=plan.Placement;var area=placement.Area;
                if(first<i)GraphNode(graph,GraphOperation.Geometry,none,target,first,i,default,none,default,default,default);
                first=i;var retainedHandle=plan.Retained;var cacheResult=plan.Cache;
                TextureHandle shadow=retainedHandle;
                if(plan.Page>=0){var page=_shadowPages[plan.Page];if(cacheResult==NativeWorldShadowCache.Result.Write)GraphNode(graph,GraphOperation.CacheCopy,page.Blur,retainedHandle,i,i+1,plan.Packed,none,default,default,default);shadow=retainedHandle.IsValid()?retainedHandle:page.Blur;}
                else if(cacheResult!=NativeWorldShadowCache.Result.Hit){
                    var desc=new TextureDesc(area.width,area.height){colorFormat=_target.graphicsFormat,depthBufferBits=DepthBits.None,msaaSamples=MSAASamples.MSAA4x,bindTextureMS=false,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,name="ECHO transient shadow mask"};
                    var mask=graph.CreateTexture(desc);desc.msaaSamples=MSAASamples.None;desc.name="ECHO transient shadow horizontal";var ping=graph.CreateTexture(desc);desc.name="ECHO transient shadow vertical";var blur=graph.CreateTexture(desc);
                    GraphTransientShadowTextures+=3;GraphShadowBlurPasses+=2;GraphShadowBlurPixels+=(long)area.width*area.height*2;ShadowPixelCount+=(long)area.width*area.height;ShadowPassCount++;
                    GraphNode(graph,GraphOperation.Mask,none,mask,i,i+1,area,none,default,default,default);
                    GraphNode(graph,GraphOperation.Horizontal,mask,ping,i,i+1,area,none,default,default,default);
                    GraphNode(graph,GraphOperation.Vertical,ping,blur,i,i+1,area,none,default,default,default);
                    if(cacheResult==NativeWorldShadowCache.Result.Write)GraphNode(graph,GraphOperation.CacheCopy,blur,retainedHandle,i,i+1,default,none,default,default,default);
                    shadow=retainedHandle.IsValid()?retainedHandle:blur;
                }
                GraphNode(graph,GraphOperation.Composite,shadow,target,i,i+1,area,none,default,default,default);
            }
            if(first<_batches.Count)GraphNode(graph,GraphOperation.Geometry,none,target,first,_batches.Count,default,none,default,default,default);
            GraphNode(graph,GraphOperation.Present,target,ui,0,0,default,cameraColor,view,projection,viewport);
        }
        private void ExecuteGraphNode(GraphData data,CommandBuffer commands)
        {
            if(data.Operation==GraphOperation.AtlasMask||data.Operation==GraphOperation.AtlasHorizontal||data.Operation==GraphOperation.AtlasVertical){ExecuteShadowAtlas(data,commands);return;}
            commands.SetGlobalFloat(ShadowAtlasId,0);
            var output=((RTHandle)data.Output).nameID;
            if(data.Operation==GraphOperation.Present){
                commands.SetGlobalInt(ShadowWeightCountId,0);commands.Blit(((RTHandle)data.Input).nameID,output,_presentMaterial,2);
                commands.SetRenderTarget(((RTHandle)data.CameraColor).nameID);commands.SetViewport(data.Viewport);commands.SetViewProjectionMatrices(data.View,data.Projection);
                GraphPending=false;GraphPassCount++;LastGraphFrame=Time.frameCount;return;
            }
            if(data.Operation==GraphOperation.CacheCopy){if(data.Area.width>0)commands.CopyTexture(((RTHandle)data.Input).nameID,0,0,data.Area.x,data.Area.y,data.Area.width,data.Area.height,output,0,0,0,0);else commands.CopyTexture(((RTHandle)data.Input).nameID,output);return;}
            if(data.Operation==GraphOperation.Horizontal||data.Operation==GraphOperation.Vertical){
                SetShadowKernel(commands,(float)VisualCanvasShadowViewport.Sigma(_batches[data.First].Blur));
                commands.SetGlobalVector(ShadowDirectionId,data.Operation==GraphOperation.Horizontal?new Vector4(1f/data.Area.width,0,0,0):new Vector4(0,1f/data.Area.height,0,0));
                commands.Blit(((RTHandle)data.Input).nameID,output,_blurMaterial,0);return;
            }
            commands.SetRenderTarget(output);
            if(data.Operation==GraphOperation.Composite){
                var batch=_batches[data.First];var area=data.Area;var placement=_graphShadows[data.First];
                commands.SetGlobalColor(ShadowColorId,batch.Shadow);commands.SetGlobalVector(ShadowOffsetId,new Vector4(batch.X/area.width,-batch.Y/area.height,0,0));commands.SetGlobalVector(ShadowRectId,new Vector4(-(area.x+placement.Translation.x)/area.width,-(area.y+placement.Translation.y)/area.height,(float)_target.width/area.width,(float)_target.height/area.height));
                var plan=_shadowPlans[data.First];
                if(plan.Page>=0&&!plan.Retained.IsValid()){
                    var page=_shadowPages[plan.Page];var packed=plan.Packed;
                    commands.SetGlobalFloat(ShadowAtlasId,1);commands.SetGlobalVector(ShadowSampleRectId,new Vector4((float)packed.x/page.Width,(float)packed.y/page.Height,(float)packed.xMax/page.Width,(float)packed.yMax/page.Height));
                    commands.SetGlobalVector(ShadowOffsetId,new Vector4(batch.X/page.Width,-batch.Y/page.Height,0,0));
                    commands.SetGlobalVector(ShadowRectId,new Vector4((packed.x-area.x-placement.Translation.x)/page.Width,(packed.y-area.y-placement.Translation.y)/page.Height,(float)_target.width/page.Width,(float)_target.height/page.Height));
                }
                var rect=ShadowCompositeRect(batch,placement);
#if UNITY_EDITOR
                if(FullShadowCompositeForValidation)rect=new Rect(0,0,_target.width,_target.height);
#endif
                if(rect.width>0&&rect.height>0){commands.EnableScissorRect(rect);commands.Blit(((RTHandle)data.Input).nameID,output,_blurMaterial,batch.Blend=="lighter"?2:batch.Blend=="screen"?3:1);commands.DisableScissorRect();ShadowCompositeCount++;ShadowCompositePixelCount+=(long)rect.width*(long)rect.height;}return;
            }
            var projection=GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(_viewport.xMin,_viewport.xMax,_viewport.yMin,_viewport.yMax,-1,1),false);
            if(data.Operation==GraphOperation.Mask){
                var area=data.Area;float px=_viewport.width/_target.width,py=_viewport.height/_target.height;
                projection=GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(_viewport.xMin+area.xMin*px,_viewport.xMin+area.xMax*px,_viewport.yMin+area.yMin*py,_viewport.yMin+area.yMax*py,-1,1),false);
                commands.ClearRenderTarget(false,true,Color.clear);commands.SetViewProjectionMatrices(Matrix4x4.identity,projection);var batch=_batches[data.First];commands.DrawMesh(batch.NativeMesh,_graphShadows[data.First].Matrix,batch.NativeMaterial,0,0,batch.NativeProperties);return;
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
