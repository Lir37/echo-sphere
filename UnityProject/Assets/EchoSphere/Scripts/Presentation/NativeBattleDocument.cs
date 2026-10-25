using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Parallel scene route: authored hierarchy, existing verified presenters and
    // read models. No simulation state belongs to the document.
    public sealed class NativeBattleDocument : NativeRunDocument
    {
        [SerializeField] private bool _explicitBackdropLayers=false;
        protected override VisualTreeAsset SceneLayout =>
            Resources.Load<VisualTreeAsset>("EchoSphere/NativeBattle/NativeBattle")
            ??throw new InvalidOperationException("Native battle UXML is missing");
        protected override bool BoundBackdropGeometry => true;
        protected override bool ExplicitBackdropLayers => _explicitBackdropLayers;
    }
}
