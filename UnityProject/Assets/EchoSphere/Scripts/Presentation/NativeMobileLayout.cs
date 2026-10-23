using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Authored mobile host geometry from the original mobile component. No DOM,
    // CSS resolution or transient placeholder nodes are needed for these slots.
    internal sealed class NativeMobileLayout : IDisposable
    {
        internal VisualElement Root { get; }
        private readonly VisualElement _dock, _joystick, _placement, _ghost, _memory, _movement;
        private readonly NativeHudSurface _movementPaint;
        internal NativeMobileLayout(VisualElement parent)
        {
            Root = Box(parent, "native-mobile-layout"); Fill(Root);
            Root.pickingMode = PickingMode.Position; Root.style.overflow = Overflow.Hidden;
            _movement = Box(Root, "native-mobile-movement-target");
            _movement.style.position = Position.Absolute;
            _movement.style.width = _movement.style.height = 116; _movement.style.bottom = 44;
            _movementPaint = new NativeHudSurface(_movement, new NativeMenuBrush.Paint {
                Ellipse = true, Border = 1, BorderColor = new Color(134/255f,228/255f,1,.34f)
            });
            // Source z-index ordering: memory 9, ghost 10, feedback/dock 0,
            // tutorial movement target 30. All children ignore picking except
            // the native controls subsequently mounted into the dock.
            _placement = Box(Root, "native-mobile-placement-host");
            _joystick = Box(Root, "native-mobile-joystick-host");
            _dock = Box(Root, "native-mobile-dock-host");
            _dock.style.position = Position.Absolute; _dock.style.bottom = 16;
            _dock.style.flexDirection = FlexDirection.Column;
            _memory = Box(Root, "native-mobile-memory-host"); Fill(_memory);
            _ghost = Box(Root, "native-mobile-ghost-host"); Fill(_ghost);
            _movement.BringToFront();
        }
        internal void Bind(NativeRunUiAnchors anchors, bool leftHanded, float interfaceScale,
            float viewportWidth, bool movement, Vector2? joystick, Vector2? placement,
            bool ghost, bool memory, float dpr)
        {
            _dock.style.left = leftHanded ? new StyleLength(StyleKeyword.Auto) : new StyleLength(12);
            _dock.style.right = leftHanded ? new StyleLength(12) : new StyleLength(StyleKeyword.Auto);
            _dock.style.alignItems = leftHanded ? Align.FlexEnd : Align.FlexStart;
            _dock.style.transformOrigin = new TransformOrigin(Length.Percent(leftHanded ? 100 : 0), Length.Percent(100));
            _dock.style.scale = new Scale(new Vector3(interfaceScale, interfaceScale, 1));
            Feedback(_joystick, joystick, 116, interfaceScale);
            Feedback(_placement, placement, 52, interfaceScale);
            _ghost.style.display = ghost ? DisplayStyle.Flex : DisplayStyle.None;
            _memory.style.display = memory ? DisplayStyle.Flex : DisplayStyle.None;
            _movement.style.display = movement ? DisplayStyle.Flex : DisplayStyle.None;
            _movement.style.left = viewportWidth * (leftHanded ? .25f : .75f) - 58;
            if (movement) _movementPaint.Prepare(dpr);
            Bind(anchors.Mobile, Root); Bind(anchors.Dock, _dock);
            if (joystick.HasValue) Bind(anchors.Joystick, _joystick);
            if (placement.HasValue) Bind(anchors.Placement, _placement);
            if (ghost) Bind(anchors.Ghost, _ghost);
            if (memory) Bind(anchors.Memory, _memory);
        }
        internal Rect MovementBounds => _movement.worldBound;
        private static void Bind(NativeRunUiAnchors.Slot slot, VisualElement element) => slot.Bind(element, element, null);
        private static void Feedback(VisualElement host, Vector2? center, float size, float scale)
        {
            host.style.display = center.HasValue ? DisplayStyle.Flex : DisplayStyle.None;
            host.style.position = Position.Absolute; host.style.width = host.style.height = size;
            if (center.HasValue) { host.style.left = center.Value.x - size / 2; host.style.top = center.Value.y - size / 2; }
            host.style.transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50));
            host.style.scale = new Scale(new Vector3(scale, scale, 1));
        }
        private static VisualElement Box(VisualElement parent, string name)
        { var e = new VisualElement { name = name, pickingMode = PickingMode.Ignore }; parent.Add(e); return e; }
        private static void Fill(VisualElement e)
        { e.style.position = Position.Absolute; e.style.left = e.style.right = e.style.top = e.style.bottom = 0; }
        public void Dispose() { _movementPaint.Dispose(); Root.RemoveFromHierarchy(); }
    }
}
