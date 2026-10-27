using UnityEngine.SceneManagement;
using EchoSphere;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // A bounded event journal, intentionally outside the saved runtime schema.
    internal sealed class NativeRouteJournal
    {
        private readonly Queue<string> _events=new Queue<string>(64);
        private bool _seen,_menu,_region,_paused;private int _page,_tutorial;private SceneHandle _scene;
        internal bool RegionLaunchPending;
        internal string[] Events=>_events.ToArray();
        internal void Observe(int runtime,SceneHandle scene,bool menu,bool region,int page,int tutorial,bool paused,string reason)
        {
            bool changed=!_seen||scene!=_scene||menu!=_menu||region!=_region||page!=_page||tutorial!=_tutorial||paused!=_paused;
            if(!changed&&reason=="state")return;
            _seen=true;_scene=scene;_menu=menu;_region=region;_page=page;_tutorial=tutorial;_paused=paused;
            var documents=new System.Text.StringBuilder();
            bool HasContent(VisualElement element){
                if(element.style.visibility==Visibility.Hidden||element.style.display==DisplayStyle.None||(element.style.opacity.keyword==StyleKeyword.Undefined&&element.style.opacity.value<=0))return false;
                if(element is TextElement text&&!string.IsNullOrEmpty(text.text)||element is Image image&&(image.image!=null||image.sprite!=null||image.vectorImage!=null)||element.style.backgroundColor.value.a>0)return true;
                for(int i=0;i<element.hierarchy.childCount;i++)if(HasContent(element.hierarchy[i]))return true;return false;
            }
            foreach(var document in UnityEngine.Object.FindObjectsByType<UIDocument>()){
                var root=document.rootVisualElement;if(root==null||!HasContent(root))continue;
                if(documents.Length>0)documents.Append(',');documents.Append(document.gameObject.name).Append('#').Append(document.ExistingObjectId());
            }
            string entry=$"[EchoSphere.Route] frame={Time.frameCount} runtime={runtime} scene={scene} menu={menu} region={region} page={page} tutorial={tutorial} paused={paused} documents=[{documents}] reason={reason}";
            if(_events.Count==64)_events.Dequeue();_events.Enqueue(entry);Debug.Log(entry);
        }
    }
#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
    // Development-only startup/resize observations. No replay, save or render
    // override: record the actual texture-to-panel geometry, not assumed sizes.
    internal sealed class NativeBattleFrameJournal
    {
        private Camera _camera;private NativeWorldSurface _world;private float _density;
        private bool _seen;private int _emitted,_initialFrames,_lastFrame=-1;
        private (int width,int height,float density,Rect root,Rect field,int textureWidth,int textureHeight,Vector2 logical,int cameraMask,float effectiveDensity) _last;
        internal void Submitted(VisualElement root,Image field,PanelSettings panel,Camera camera,NativeWorldSurface world,float density)
        {_camera=camera;_world=world;_density=density;Observe("submitted",root,field,panel);}
        internal void Layout(VisualElement root,Image field,PanelSettings panel)=>Observe("layout",root,field,panel);
        private void Observe(string stage,VisualElement root,Image field,PanelSettings panel)
        {
            if(_emitted>=64||_camera==null||_world==null||root==null||field==null)return;
            var texture=field.image;
            var state=(Screen.width,Screen.height,_density,root.worldBound,field.worldBound,texture!=null?texture.width:0,texture!=null?texture.height:0,_world.LogicalSize,_camera.cullingMask,root.scaledPixelsPerPoint);
            bool changed=!_seen||!state.Equals(_last);
            bool initial=stage=="submitted"&&_initialFrames<4&&_lastFrame!=Time.frameCount;
            if(!changed&&!initial)return;
            _seen=true;_last=state;_emitted++;if(initial){_initialFrames++;_lastFrame=Time.frameCount;}
            var cameras=Object.FindObjectsByType<Camera>();
            var worlds=Object.FindObjectsByType<NativeWorldSurface>();
            int documents=0;foreach(var document in Object.FindObjectsByType<UIDocument>())if(document.isActiveAndEnabled)documents++;
            string RectText(Rect r)=>System.FormattableString.Invariant($"{r.x:R},{r.y:R},{r.width:R},{r.height:R}");
            Debug.Log(System.FormattableString.Invariant($"[EchoSphere.Frame] stage={stage} frame={Time.frameCount} screen={state.Item1}x{state.Item2} density={_density:R} panelScale={panel.scale:R} effectiveDensity={state.Item10:R} root=[{RectText(state.Item4)}] field=[{RectText(state.Item5)}] texture={state.Item6}x{state.Item7} logical={state.Item8.x:R}x{state.Item8.y:R} cameraRect=[{RectText(_camera.rect)}] cameraPixels=[{RectText(_camera.pixelRect)}] cameraMask={_camera.cullingMask} cameras={cameras.Length} worlds={worlds.Length} documents={documents} worldDraws={_world.GeometryDrawCalls} shadowPasses={_world.ShadowPassCount} shadowPixels={_world.ShadowPixelCount} shadowComposites={_world.ShadowCompositeCount} compositePixels={_world.ShadowCompositePixelCount} culledBatches={_world.CulledBatchCount} graphPending={_world.GraphPending} graphPasses={_world.GraphPassCount} lastGraphFrame={_world.LastGraphFrame}"));
        }
    }
#endif
}
