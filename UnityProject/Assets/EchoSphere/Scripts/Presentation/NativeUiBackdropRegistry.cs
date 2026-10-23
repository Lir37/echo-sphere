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
            internal NativeUiBackdropRegistry Owner;internal VisualElement Element;internal SourceUiBackdrop Backdrop;internal float Sigma;
            public void Dispose(){if(Owner==null)return;if(Owner._bindings.Remove(Element))Backdrop.Dispose();Owner=null;}
        }
        private readonly Dictionary<VisualElement,Binding> _bindings=new Dictionary<VisualElement,Binding>();
        private readonly Func<VisualElement,Action<MeshGenerationContext>> _paint;
        private readonly SourceUiReplicaSnapshotCache _snapshots;
        private bool _disposed;
        internal NativeUiBackdropRegistry(Func<VisualElement,Action<MeshGenerationContext>> paint,SourceUiReplicaSnapshotCache snapshots)
        {_paint=paint;_snapshots=snapshots;}
        public IDisposable RegisterNativeBackdrop(VisualElement element,float sigma)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeUiBackdropRegistry));if(_bindings.ContainsKey(element))throw new InvalidOperationException("Native backdrop already registered");
            var binding=new Binding {Owner=this,Element=element,Sigma=sigma,Backdrop=new SourceUiBackdrop(element,context=>{},_paint,_snapshots)};_bindings.Add(element,binding);return binding;
        }
        internal bool Contains(VisualElement element)=>_bindings.ContainsKey(element);
        internal void Prepare(VisualElement element,VisualElement prefixRoot)
        {if(_bindings.TryGetValue(element,out var binding))binding.Backdrop.Prepare(prefixRoot,binding.Sigma);}
        public void Dispose()
        {if(_disposed)return;_disposed=true;foreach(var binding in _bindings.Values){binding.Backdrop.Dispose();binding.Owner=null;}_bindings.Clear();}
    }
}
