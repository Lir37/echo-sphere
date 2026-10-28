#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Native views depend on a backdrop provider, not a DOM/document owner.
internal sealed class LegacyUiBackdropRegistry : INativeUiBackdrops,IDisposable
    {
        private sealed class Binding : IDisposable
        {
            internal LegacyUiBackdropRegistry Owner;internal VisualElement Element,Prefix;internal SourceUiBackdrop Backdrop;internal NativeCaptureBackdrop Capture;internal float Sigma,Scale;internal ulong Version,Revision;internal bool Prepared,RequestedExplicit,RetainedCompatibility;
            internal void Retire(){Backdrop?.Dispose();Backdrop=null;if(Capture!=null){Owner._renderOrder.Remove(Capture);Capture.Dispose();Capture=null;}}
            public void Dispose(){if(Owner==null)return;if(Owner._bindings.Remove(Element)){Owner._revision++;Retire();}Owner=null;}
        }
        private readonly Dictionary<VisualElement,Binding> _bindings=new Dictionary<VisualElement,Binding>();
        private readonly Func<VisualElement,Action<MeshGenerationContext>> _paint;
        private readonly SourceUiReplicaSnapshotCache _snapshots;
        private bool _disposed;
        private readonly bool _retainLayout,_boundReplica;private ulong _revision;
        private readonly bool _explicitLayers;private readonly Transform _captureOwner;
        private readonly List<NativeCaptureBackdrop> _renderOrder=new List<NativeCaptureBackdrop>();private bool _rendering;
        internal int LayoutPrepareCount {get;private set;}
        internal int DynamicRepaintCount {get;private set;}
        internal int CapturePrepareCount {get;private set;}
        internal int RetainedFallbackCount {get;private set;}
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
        internal bool DisableLayoutCacheForValidation=false;
        internal bool DisableExplicitLayersForValidation=false;
#endif
        internal LegacyUiBackdropRegistry(Func<VisualElement,Action<MeshGenerationContext>> paint,SourceUiReplicaSnapshotCache snapshots,bool retainLayout=false)
            :this(paint,snapshots,retainLayout,false){}
        internal LegacyUiBackdropRegistry(Func<VisualElement,Action<MeshGenerationContext>> paint,SourceUiReplicaSnapshotCache snapshots,bool retainLayout,bool boundReplica)
            :this(paint,snapshots,retainLayout,boundReplica,false,null){}
        internal LegacyUiBackdropRegistry(Func<VisualElement,Action<MeshGenerationContext>> paint,SourceUiReplicaSnapshotCache snapshots,bool retainLayout,bool boundReplica,bool explicitLayers,Transform captureOwner)
        {_paint=paint;_snapshots=snapshots;_retainLayout=retainLayout;_boundReplica=boundReplica;_explicitLayers=explicitLayers;_captureOwner=captureOwner;if(explicitLayers)Application.onBeforeRender+=RenderLayers;}
        public IDisposable RegisterNativeBackdrop(VisualElement element,float sigma)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(LegacyUiBackdropRegistry));if(_bindings.ContainsKey(element))throw new InvalidOperationException("Native backdrop already registered");
            var binding=new Binding {Owner=this,Element=element,Sigma=sigma};SelectBackend(binding,_explicitLayers);_bindings.Add(element,binding);_revision++;return binding;
        }
        private void SelectBackend(Binding binding,bool explicitLayers)
        {binding.Retire();if(explicitLayers)binding.Capture=new NativeCaptureBackdrop(binding.Element,_paint,_snapshots,_captureOwner);else binding.Backdrop=new SourceUiBackdrop(binding.Element,context=>{},_paint,_snapshots,_boundReplica);binding.Prepared=false;}
        internal void BeginFrame(){foreach(var layer in _renderOrder)layer.Scheduled=false;_renderOrder.Clear();}
        private void RenderLayers(){if(_disposed||_rendering)return;_rendering=true;try{foreach(var layer in _renderOrder)layer.Render();}finally{_rendering=false;}}
        internal bool Contains(VisualElement element)=>_bindings.ContainsKey(element);
        internal bool UsesCapture(VisualElement element)=>_bindings.TryGetValue(element,out var binding)&&binding.Capture!=null;
        internal void Prepare(VisualElement element,VisualElement prefixRoot,float scale=1)
        {
            if(!_bindings.TryGetValue(element,out var binding))return;
            bool explicitLayers=_explicitLayers;
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
            if(DisableExplicitLayersForValidation)explicitLayers=false;
#endif
            bool retain=_retainLayout;
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
            if(DisableLayoutCacheForValidation)retain=false;
#endif
            ulong version=retain?NativeMenuPaintVersion.ReadLayoutPrefix(prefixRoot,element,binding.Sigma):0;
            bool stable=retain&&binding.Prepared&&ReferenceEquals(binding.Prefix,prefixRoot)&&binding.Scale==scale&&binding.Version==version&&binding.Revision==_revision;
            bool requested=explicitLayers;
            if(requested){
                explicitLayers=!binding.RetainedCompatibility&&(stable&&binding.RequestedExplicit?binding.Capture!=null:NativeCaptureBackdrop.CanCapture(prefixRoot,element,binding.Sigma));
                // A moving clip must not repeatedly retire/recreate filter IDs.
                // Once compatibility is needed, retain that backend for its owner.
                if(!explicitLayers&&!binding.RetainedCompatibility){binding.RetainedCompatibility=true;
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS) || UNITY_INCLUDE_INSTRUMENTATION
                    if(NativeDeviceReplay.Active)Debug.Log("[EchoSphere.Replay] effect-mode=retained element="+element.name);
#endif
                }
            }
            binding.RequestedExplicit=requested;
            if((binding.Capture!=null)!=explicitLayers){SelectBackend(binding,explicitLayers);_revision++;}
            if(binding.Capture!=null){binding.Capture.Scheduled=true;_renderOrder.Add(binding.Capture);CapturePrepareCount++;}else if(requested)RetainedFallbackCount++;
            if(retain&&binding.Prepared&&ReferenceEquals(binding.Prefix,prefixRoot)&&binding.Scale==scale&&binding.Version==version&&binding.Revision==_revision){if(binding.Capture!=null)binding.Capture.RepaintDynamic();else binding.Backdrop.RepaintDynamic();DynamicRepaintCount++;return;}
            if(binding.Capture!=null)binding.Capture.Prepare(prefixRoot,binding.Sigma,scale);else binding.Backdrop.Prepare(prefixRoot,binding.Sigma);LayoutPrepareCount++;binding.Prepared=true;binding.Prefix=prefixRoot;binding.Scale=scale;binding.Version=version;binding.Revision=_revision;
        }
        public void Dispose()
        {if(_disposed)return;_disposed=true;if(_explicitLayers)Application.onBeforeRender-=RenderLayers;foreach(var binding in _bindings.Values){binding.Retire();binding.Owner=null;}_bindings.Clear();_renderOrder.Clear();}
    }
}

#endif
