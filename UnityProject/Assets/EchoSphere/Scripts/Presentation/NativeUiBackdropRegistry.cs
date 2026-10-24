using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Native views depend on a backdrop provider, not a DOM/document owner.
    public interface INativeUiBackdrops
    {
        IDisposable RegisterNativeBackdrop(VisualElement element,float sigma);
    }
    internal sealed class NativeUiBackdropRegistry : INativeUiBackdrops,IDisposable
    {
        private sealed class Binding : IDisposable
        {
            internal NativeUiBackdropRegistry Owner;internal VisualElement Element,Prefix;internal SourceUiBackdrop Backdrop;internal float Sigma,Scale;internal ulong Version,Revision;internal bool Prepared;
            public void Dispose(){if(Owner==null)return;if(Owner._bindings.Remove(Element)){Owner._revision++;Backdrop.Dispose();}Owner=null;}
        }
        private readonly Dictionary<VisualElement,Binding> _bindings=new Dictionary<VisualElement,Binding>();
        private readonly Func<VisualElement,Action<MeshGenerationContext>> _paint;
        private readonly SourceUiReplicaSnapshotCache _snapshots;
        private bool _disposed;
        private readonly bool _retainLayout;private ulong _revision;
        internal int LayoutPrepareCount {get;private set;}
        internal int DynamicRepaintCount {get;private set;}
#if UNITY_EDITOR
        internal bool DisableLayoutCacheForValidation=false;
#endif
        internal NativeUiBackdropRegistry(Func<VisualElement,Action<MeshGenerationContext>> paint,SourceUiReplicaSnapshotCache snapshots,bool retainLayout=false)
        {_paint=paint;_snapshots=snapshots;_retainLayout=retainLayout;}
        public IDisposable RegisterNativeBackdrop(VisualElement element,float sigma)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeUiBackdropRegistry));if(_bindings.ContainsKey(element))throw new InvalidOperationException("Native backdrop already registered");
            var binding=new Binding {Owner=this,Element=element,Sigma=sigma,Backdrop=new SourceUiBackdrop(element,context=>{},_paint,_snapshots)};_bindings.Add(element,binding);_revision++;return binding;
        }
        internal bool Contains(VisualElement element)=>_bindings.ContainsKey(element);
        internal void Prepare(VisualElement element,VisualElement prefixRoot,float scale=1)
        {
            if(!_bindings.TryGetValue(element,out var binding))return;
            bool retain=_retainLayout;
#if UNITY_EDITOR
            if(DisableLayoutCacheForValidation)retain=false;
#endif
            ulong version=retain?NativeMenuPaintVersion.ReadLayoutPrefix(prefixRoot,element,binding.Sigma):0;
            if(retain&&binding.Prepared&&ReferenceEquals(binding.Prefix,prefixRoot)&&binding.Scale==scale&&binding.Version==version&&binding.Revision==_revision){binding.Backdrop.RepaintDynamic();DynamicRepaintCount++;return;}
            binding.Backdrop.Prepare(prefixRoot,binding.Sigma);LayoutPrepareCount++;binding.Prepared=true;binding.Prefix=prefixRoot;binding.Scale=scale;binding.Version=version;binding.Revision=_revision;
        }
        public void Dispose()
        {if(_disposed)return;_disposed=true;foreach(var binding in _bindings.Values){binding.Backdrop.Dispose();binding.Owner=null;}_bindings.Clear();}
    }
}
