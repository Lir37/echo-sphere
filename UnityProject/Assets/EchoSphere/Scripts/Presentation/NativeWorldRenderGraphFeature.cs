using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace EchoSphere.Presentation
{
    // Scene-local runtime feature. Renderer data is changed only in memory;
    // exact owned instances are removed at teardown. No quality asset is saved.
    public sealed class NativeWorldRenderGraphFeature : ScriptableRendererFeature
    {
        internal NativeWorldSurface World;
        internal Camera OwnerCamera;
        internal bool Ready;
        private WorldPass _pass;
        public override void Create(){_pass=new WorldPass();Ready=true;}
        public override void AddRenderPasses(ScriptableRenderer renderer,ref RenderingData renderingData)
        {
            if(World==null||!World.GraphPending||renderingData.cameraData.camera!=OwnerCamera)return;
            _pass.World=World;renderer.EnqueuePass(_pass);
        }
        protected override void Dispose(bool disposing){Ready=false;}
        private sealed class WorldPass:ScriptableRenderPass
        {
            internal NativeWorldSurface World;
            internal WorldPass(){renderPassEvent=RenderPassEvent.BeforeRenderingPostProcessing;}
            public override void RecordRenderGraph(RenderGraph graph,ContextContainer frameData)
            {
                if(World==null||!World.GraphPending)return;
                var resources=frameData.Get<UniversalResourceData>();var camera=frameData.Get<UniversalCameraData>();
                World.RecordResourceGraph(graph,resources.activeColorTexture,camera.GetViewMatrix(),GL.GetGPUProjectionMatrix(camera.GetProjectionMatrix(),true),new Rect(0,0,camera.cameraTargetDescriptor.width,camera.cameraTargetDescriptor.height));
            }
        }
    }
    internal sealed class NativeWorldRenderGraphLease:IDisposable
    {
        private readonly Camera _camera;private readonly RenderPipelineAsset _pipeline;
        private readonly List<(ScriptableRendererData Data,NativeWorldRenderGraphFeature Feature)> _features=new List<(ScriptableRendererData,NativeWorldRenderGraphFeature)>();
        internal NativeWorldRenderGraphLease(NativeWorldSurface world,Camera camera)
        {
            _camera=camera;_pipeline=GraphicsSettings.currentRenderPipeline;
            if(!Application.isPlaying||!(_pipeline is UniversalRenderPipelineAsset urp))return;
            foreach(var data in urp.rendererDataList){
                if(data==null)continue;
                bool attached=false;foreach(var existing in _features)if(existing.Data==data){attached=true;break;}if(attached)continue;
                var feature=ScriptableObject.CreateInstance<NativeWorldRenderGraphFeature>();feature.name="ECHO SPHERE scene world";feature.hideFlags=HideFlags.HideAndDontSave;feature.World=world;feature.OwnerCamera=camera;
                data.rendererFeatures.Add(feature);data.SetDirty();_features.Add((data,feature));
            }
        }
        internal bool IsCurrent(Camera camera)=>camera==_camera&&_pipeline==GraphicsSettings.currentRenderPipeline;
        internal bool CanSubmit {get{if(!Application.isPlaying||_camera==null||!_camera.isActiveAndEnabled)return false;foreach(var item in _features)if(item.Feature!=null&&item.Feature.Ready)return true;return false;}}
        public void Dispose()
        {
            foreach(var item in _features){if(item.Data!=null){item.Data.rendererFeatures.Remove(item.Feature);item.Data.SetDirty();}if(item.Feature!=null){if(Application.isPlaying)UnityEngine.Object.Destroy(item.Feature);else UnityEngine.Object.DestroyImmediate(item.Feature);}}
            _features.Clear();
        }
    }
}
