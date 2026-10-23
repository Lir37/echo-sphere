using EchoSphere.Presentation;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeMobileLayout _nativeMobileLayout;
        private VisualElement _nativeRunField;
        private void InvalidateNativeUiAnchors(){if(_sourceUi!=null)_sourceUi.Dirty=true;}
    }
}
