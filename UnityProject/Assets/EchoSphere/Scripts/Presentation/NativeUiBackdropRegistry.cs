using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace EchoSphere.Presentation
{
    public interface INativeUiBackdrops {IDisposable RegisterNativeBackdrop(VisualElement element,float sigma);}
    internal sealed class NativeUiBackdropRegistry:INativeUiBackdrops,IDisposable
    {
        private sealed class Binding:IDisposable
        {
            internal NativeUiBackdropRegistry Owner;internal VisualElement Element;internal NativeEngineBackdrop Effect;
            public void Dispose(){if(Owner==null)return;Owner._bindings.Remove(Element);Owner=null;Effect.Dispose();}
        }
        private readonly Dictionary<VisualElement,Binding> _bindings=new Dictionary<VisualElement,Binding>();
        private bool _disposed;
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
        private LegacyUiBackdropRegistry _legacy;
        internal bool DisableLayoutCacheForValidation=false,DisableExplicitLayersForValidation=false;
        internal NativeUiBackdropRegistry(Func<VisualElement,Action<MeshGenerationContext>> paint,SourceUiReplicaSnapshotCache snapshots,bool retainLayout=false):this(paint,snapshots,retainLayout,false,false,null){}
        internal NativeUiBackdropRegistry(Func<VisualElement,Action<MeshGenerationContext>> paint,SourceUiReplicaSnapshotCache snapshots,bool retainLayout,bool bounded):this(paint,snapshots,retainLayout,bounded,false,null){}
        internal NativeUiBackdropRegistry(Func<VisualElement,Action<MeshGenerationContext>> paint,SourceUiReplicaSnapshotCache snapshots,bool retainLayout,bool bounded,bool explicitLayers,Transform owner):this(){if(NativeBackdropValidation.UseLegacy){_legacy?.Dispose();_legacy=new LegacyUiBackdropRegistry(paint,snapshots,retainLayout,bounded,false,null);}}
#endif
        internal NativeUiBackdropRegistry()
        {
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
            if(NativeBackdropValidation.UseLegacy)_legacy=new LegacyUiBackdropRegistry(e=>e is Image||e is TextElement?null:e.generateVisualContent,null,true,true,false,null);
#endif
        }
        public IDisposable RegisterNativeBackdrop(VisualElement element,float sigma)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeUiBackdropRegistry));
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
            if(_legacy!=null)return _legacy.RegisterNativeBackdrop(element,sigma);
#endif
            if(_bindings.ContainsKey(element))throw new InvalidOperationException("Native backdrop already registered");
            var binding=new Binding{Owner=this,Element=element,Effect=new NativeEngineBackdrop(element,sigma)};_bindings.Add(element,binding);
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS) || UNITY_INCLUDE_INSTRUMENTATION
            if(NativeDeviceReplay.Active)Debug.Log("[EchoSphere.Replay] effect-mode=unity-backdrop-filter element="+element.name);
#endif
            return binding;
        }
        internal int LayoutPrepareCount=>0;
        internal int DynamicRepaintCount=>0;
        internal int CapturePrepareCount=>0;
        internal int RetainedFallbackCount=>0;
        internal bool Contains(VisualElement e){
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
            if(_legacy!=null)return _legacy.Contains(e);
#endif
            return _bindings.ContainsKey(e);
        }
        internal bool UsesCapture(VisualElement e)=>false;
        internal void BeginFrame(){
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
            _legacy?.BeginFrame();
#endif
        }
        internal void Prepare(VisualElement e,VisualElement root,float scale=1)
        {
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
            _legacy?.Prepare(e,root,scale);
#endif
        }
        public void Dispose()
        {
            if(_disposed)return;_disposed=true;
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
            _legacy?.Dispose();
#endif
            foreach(var binding in _bindings.Values){binding.Owner=null;binding.Effect.Dispose();}_bindings.Clear();
        }
    }
}
