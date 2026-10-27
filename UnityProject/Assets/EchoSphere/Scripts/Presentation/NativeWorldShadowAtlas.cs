using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldSurface
    {
        private sealed class ShadowPlan
        {
            internal bool Active;internal ShadowPlacement Placement;
            internal NativeWorldShadowCache.Result Cache;
            internal TextureHandle Retained;internal int Page=-1;internal RectInt Packed;
        }
        private sealed class ShadowPage
        {
            internal readonly List<int> Items=new List<int>();
            internal float Sigma;internal int X,Y,Row,Width,Height;
            internal TextureHandle Blur;
            internal void Reset(float sigma){Items.Clear();Sigma=sigma;X=Y=Row=Width=Height=0;}
            internal bool Pack(int width,int height,out RectInt rect)
            {
                int x=X,y=Y,row=Row;if(x+width>512){x=0;y+=row;row=0;}
                if(width>512||y+height>512){rect=default;return false;}
                rect=new RectInt(x,y,width,height);X=x+width;Y=y;Row=Mathf.Max(row,height);Width=Mathf.Max(Width,X);Height=Mathf.Max(Height,Y+Row);return true;
            }
        }
        private readonly List<ShadowPlan> _shadowPlans=new List<ShadowPlan>();
        private readonly List<ShadowPage> _shadowPages=new List<ShadowPage>();
        private int _shadowPageUsed;
        private static readonly int ShadowAtlasId=Shader.PropertyToID("_SourceShadowAtlas");
        private static readonly int ShadowSampleRectId=Shader.PropertyToID("_SourceShadowSampleRect");
        internal int GraphShadowAtlasPages=>_shadowPageUsed;
        internal int GraphShadowBlurPasses {get;private set;}
        internal long GraphShadowBlurPixels {get;private set;}
#if UNITY_EDITOR
        internal bool DisableShadowAtlasForValidation=false;
#endif
        private void PrepareShadowPlans(RenderGraph graph)
        {
            _shadowPageUsed=0;GraphShadowBlurPasses=0;GraphShadowBlurPixels=0;
            for(int i=0;i<_batches.Count;i++){
                if(i==_shadowPlans.Count)_shadowPlans.Add(new ShadowPlan());var plan=_shadowPlans[i];plan.Active=false;plan.Page=-1;plan.Retained=default;
                var batch=_batches[i];var area=batch.NativeMesh==null?default:ShadowArea(batch);bool culled=area.width<=0||area.height<=0;
#if UNITY_EDITOR
                if(DisableBatchCullingForValidation&&batch.NativeMesh!=null)culled=false;
#endif
                _graphAreas.Add(culled?default:area);_graphShadows.Add(default);if(culled){CulledBatchCount++;continue;}
                if(batch.Shadow.a<=0||batch.Blur<=0&&batch.X==0&&batch.Y==0||area.width<=0||area.height<=0)continue;
                plan.Active=true;plan.Placement=PlaceShadow(batch,area);_graphShadows[i]=plan.Placement;area=plan.Placement.Area;
                plan.Cache=NativeWorldShadowCache.Result.Miss;RenderTexture retained=null;
                bool cacheAllowed=batch.NativeMaterial.shader==_solidShader&&NativeWorldShapes.GeometryVersion(batch.NativeMesh)!=0&&batch.NativeProperties.HasVector(NativeColorId);
#if UNITY_EDITOR
                if(FullShadowViewportForValidation||DisableShadowCacheForValidation)cacheAllowed=false;
#endif
                if(cacheAllowed){var props=batch.NativeProperties;var key=new NativeWorldShadowCache.Key(batch.NativeMesh,NativeWorldShapes.GeometryVersion(batch.NativeMesh),plan.Placement.Matrix,_viewport,area,props.GetVector(NativeColorId).w,props.GetFloat(NativeOpacityId),batch.Blur);plan.Cache=_shadowCache.Probe(key,area.width,area.height,out retained);}
                if(retained!=null)plan.Retained=graph.ImportTexture(_shadowCache.GraphHandle(retained));
                if(plan.Cache==NativeWorldShadowCache.Result.Hit){ShadowCacheHits++;continue;}
                bool atlas=plan.Placement.Local&&area.width<=512&&area.height<=512;
#if UNITY_EDITOR
                if(DisableShadowAtlasForValidation||FullShadowViewportForValidation)atlas=false;
#endif
                if(!atlas)continue;
                float sigma=(float)VisualCanvasShadowViewport.Sigma(batch.Blur);
                for(int page=0;page<_shadowPageUsed;page++)if(_shadowPages[page].Sigma==sigma&&_shadowPages[page].Pack(area.width,area.height,out plan.Packed)){plan.Page=page;break;}
                if(plan.Page<0&&_shadowPageUsed<8){int page=_shadowPageUsed++;if(page==_shadowPages.Count)_shadowPages.Add(new ShadowPage());_shadowPages[page].Reset(sigma);if(!_shadowPages[page].Pack(area.width,area.height,out plan.Packed))throw new System.InvalidOperationException("Invalid shadow page allocation");plan.Page=page;}
                if(plan.Page>=0)_shadowPages[plan.Page].Items.Add(i);
            }
            for(int i=0;i<_shadowPageUsed;i++){
                var page=_shadowPages[i];var desc=new TextureDesc(page.Width,page.Height){colorFormat=_target.graphicsFormat,depthBufferBits=DepthBits.None,msaaSamples=MSAASamples.MSAA4x,bindTextureMS=false,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,name="ECHO shadow atlas mask"};
                var mask=graph.CreateTexture(desc);desc.msaaSamples=MSAASamples.None;desc.name="ECHO shadow atlas horizontal";var ping=graph.CreateTexture(desc);desc.name="ECHO shadow atlas vertical";page.Blur=graph.CreateTexture(desc);
                GraphTransientShadowTextures+=3;GraphShadowBlurPasses+=2;GraphShadowBlurPixels+=(long)page.Width*page.Height*2;
                GraphNode(graph,GraphOperation.AtlasMask,default,mask,i,0,default,default,default,default,default);
                GraphNode(graph,GraphOperation.AtlasHorizontal,mask,ping,i,0,default,default,default,default,default);
                GraphNode(graph,GraphOperation.AtlasVertical,ping,page.Blur,i,0,default,default,default,default,default);
            }
        }
        private void ExecuteShadowAtlas(GraphData data,CommandBuffer commands)
        {
            var page=_shadowPages[data.First];var output=((RTHandle)data.Output).nameID;
            commands.SetGlobalFloat(ShadowAtlasId,0);
            if(data.Operation!=GraphOperation.AtlasMask){
                SetShadowKernel(commands,page.Sigma);commands.SetGlobalVector(ShadowDirectionId,data.Operation==GraphOperation.AtlasHorizontal?new Vector4(1f/page.Width,0,0,0):new Vector4(0,1f/page.Height,0,0));commands.Blit(((RTHandle)data.Input).nameID,output,_blurMaterial,0);return;
            }
            commands.SetRenderTarget(output);commands.ClearRenderTarget(false,true,Color.clear);
            float px=_viewport.width/_target.width,py=_viewport.height/_target.height;
            foreach(int index in page.Items){var plan=_shadowPlans[index];var area=plan.Placement.Area;var packed=plan.Packed;var batch=_batches[index];
                commands.SetViewport(new Rect(packed.x,packed.y,packed.width,packed.height));commands.EnableScissorRect(new Rect(packed.x,packed.y,packed.width,packed.height));
                var projection=GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(_viewport.xMin+area.xMin*px,_viewport.xMin+area.xMax*px,_viewport.yMin+area.yMin*py,_viewport.yMin+area.yMax*py,-1,1),false);
                commands.SetViewProjectionMatrices(Matrix4x4.identity,projection);commands.DrawMesh(batch.NativeMesh,plan.Placement.Matrix,batch.NativeMaterial,0,0,batch.NativeProperties);
                commands.DisableScissorRect();ShadowPixelCount+=(long)area.width*area.height;ShadowPassCount++;
            }
            commands.SetViewport(new Rect(0,0,page.Width,page.Height));
        }
    }
}
