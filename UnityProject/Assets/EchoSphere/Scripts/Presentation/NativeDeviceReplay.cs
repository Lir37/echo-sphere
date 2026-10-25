#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Explicit development-only replay of existing UI events. No ordinary
    // launch, release player, saved run or tutorial-completion flag is changed.
    public sealed class NativeDeviceReplay : MonoBehaviour
    {
        private int _phase,_startFrame;private double _started,_stageStarted;
        private long _gcTotal,_gcPeak;private int _samples;
        private ProfilerRecorder _gc,_textures,_textureBytes,_draws;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StartRequestedReplay()
        {
            if(Application.isEditor||!Debug.isDebugBuild||Application.platform!=RuntimePlatform.Android)return;
            using var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity=player.GetStatic<AndroidJavaObject>("currentActivity");
            using var intent=activity.Call<AndroidJavaObject>("getIntent");
            if(intent.Call<string>("getStringExtra","echo_diagnostic_replay")!="tutorial")return;
            using(var consumed=intent.Call<AndroidJavaObject>("removeExtra","echo_diagnostic_replay")){}
            if(PlayerPrefs.HasKey("echosphere_active_run_v1")){Debug.Log("[EchoSphere.Replay] blocked: existing saved run");return;}
            if(PlayerPrefs.GetInt("echosphere_tutorial_completed_v1",0)!=0){Debug.Log("[EchoSphere.Replay] blocked: tutorial already completed");return;}
            var owner=new GameObject("Explicit native device replay");DontDestroyOnLoad(owner);owner.AddComponent<NativeDeviceReplay>();
        }
        private void Awake()
        {
            _started=Time.realtimeSinceStartupAsDouble;
            var handles=new List<ProfilerRecorderHandle>();ProfilerRecorderHandle.GetAvailable(handles);
            _gc=Counter(handles,"GC Allocated In Frame");_textures=Counter(handles,"Render Textures Count");
            _textureBytes=Counter(handles,"Render Textures Bytes");_draws=Counter(handles,"Draw Calls Count");
            Debug.Log("[EchoSphere.Replay] start api="+SystemInfo.graphicsDeviceType+" version="+SystemInfo.graphicsDeviceVersion);
        }
        private static ProfilerRecorder Counter(List<ProfilerRecorderHandle> handles,string name)
        {
            foreach(var handle in handles){var description=ProfilerRecorderHandle.GetDescription(handle);if(description.Name==name)return ProfilerRecorder.StartNew(description.Category,name,1);}
            return default;
        }
        private void LateUpdate()
        {
            double now=Time.realtimeSinceStartupAsDouble;
            if(now-_started>240){Finish("timeout");return;}
            if(_phase==0){if(Submit("open-map")){_phase=1;DumpTextures("menu");}return;}
            if(_phase==1){if(PlayerPrefs.HasKey("echosphere_active_run_v1")){Finish("blocked: existing saved run");return;}if(Submit("start-resonance_basin"))_phase=2;return;}
            if(_phase==2){if(Find("tutorial-next")==null)return;BeginStage(now);_phase=3;return;}
            if(_gc.Valid){long bytes=_gc.LastValue;_gcTotal+=bytes;_gcPeak=Math.Max(_gcPeak,bytes);}_samples++;
            if(now-_stageStarted<20)return;
            Report(_phase==3?"tutorial-0":"tutorial-1",now);
            if(_phase==3&&Submit("tutorial-next")){BeginStage(now);_phase=4;return;}
            Finish("complete");
        }
        private void BeginStage(double now){_stageStarted=now;_startFrame=Time.frameCount;_gcTotal=0;_gcPeak=0;_samples=0;}
        private void Report(string stage,double now)
        {
            double elapsed=now-_stageStarted;
            Debug.Log("[EchoSphere.Replay] stage="+stage+" frames="+(Time.frameCount-_startFrame)+" elapsed="+elapsed.ToString("F3",CultureInfo.InvariantCulture)+
                " gcCounter="+_gc.Valid+" gcMean="+(_samples==0?0:_gcTotal/_samples)+" gcPeak="+_gcPeak+
                " rtCounter="+_textures.Valid+" rtCount="+(_textures.Valid?_textures.LastValue:-1)+" rtBytes="+(_textureBytes.Valid?_textureBytes.LastValue:-1)+" draws="+(_draws.Valid?_draws.LastValue:-1));
            DumpTextures(stage);
        }
        private static void DumpTextures(string stage)
        {
            var textures=Resources.FindObjectsOfTypeAll<RenderTexture>().Where(t=>t.IsCreated()).Select(t=>new {t.name,t.width,t.height,bytes=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t)}).OrderByDescending(t=>t.bytes).ToArray();
            Debug.Log("[EchoSphere.Replay] textures="+stage+" count="+textures.Length+" bytes="+textures.Sum(t=>t.bytes)+" largest=["+string.Join(";",textures.Take(12).Select(t=>t.name+":"+t.width+"x"+t.height+":"+t.bytes))+"]");
        }
        private static Button Find(string name)
        {
            Button found=null;
            void Visit(VisualElement e)
            {
                if(e.style.visibility==Visibility.Hidden||e.resolvedStyle.display==DisplayStyle.None||(e.style.opacity.keyword==StyleKeyword.Undefined&&e.style.opacity.value<=0))return;
                if(e is Button b&&b.name==name&&b.enabledInHierarchy&&b.layout.width>0&&b.layout.height>0)found=b;
                for(int i=0;i<e.hierarchy.childCount;i++)Visit(e.hierarchy[i]);
            }
            foreach(var document in UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None))Visit(document.rootVisualElement);
            return found;
        }
        private static bool Submit(string name)
        {
            var button=Find(name);if(button==null)return false;
            using(var submit=NavigationSubmitEvent.GetPooled()){submit.target=button;button.SendEvent(submit);}
            Debug.Log("[EchoSphere.Replay] submit="+name+" frame="+Time.frameCount);return true;
        }
        private void Finish(string reason){Debug.Log("[EchoSphere.Replay] "+reason);Destroy(gameObject);}
        private void OnDestroy(){_gc.Dispose();_textures.Dispose();_textureBytes.Dispose();_draws.Dispose();}
    }
}
#endif
