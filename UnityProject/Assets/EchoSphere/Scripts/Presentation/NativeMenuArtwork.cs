using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Authored flat energy core. One retained GPU target; animated shader parts
    // never rebuild geometry or allocate per-orbit blur/depth resources.
    public sealed class NativeMenuArtwork : IDisposable
    {
        private readonly VisualElement _parent;
        private readonly NativeMainMenuBackground _surface;
        private int _animationFrame = -1;
        private readonly double _animationStarted=Time.realtimeSinceStartupAsDouble;
        public Texture Texture => _surface.Texture;
        public Rect Bounds { get; private set; }
        public int PaintCount => _surface.PaintCount;
        public int AnimationFrameCount { get; private set; }
        public NativeMenuArtwork(VisualElement parent)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _surface = new NativeMainMenuBackground(parent, "EchoSphere/NativeMenuCore");
            parent.Q<Image>("native-menu-background-image").name = "native-menu-core-image";
        }
        private static Color C(string hex) { ColorUtility.TryParseHtmlString(hex, out var color); return color; }
        public static NativeMenuBrush.Paint AvatarPaint(Color color)=>new NativeMenuBrush.Paint {Ellipse=true,Origin=new Vector2(.35f,.30f),Colors=new[]{Color.white,Color.white,color,C("#163a5c"),C("#02060d")},Stops=new[]{0f,.06f,.20f,.52f,.76f},OuterA=new NativeMenuBrush.Shadow(13,Vector2.zero,new Color(color.r,color.g,color.b,.3f))};
        public bool Prepare(float scale)
        {
            var box = _parent.layout;
            if (!float.IsFinite(box.width + box.height) || box.width <= 0 || box.height <= 0) return false;
            _surface.SetViewport(box.width, box.height, scale);
            if (_animationFrame != Time.frameCount)
            {
                _surface.Animate((float)(Time.realtimeSinceStartupAsDouble-_animationStarted));
                _animationFrame = Time.frameCount;
                AnimationFrameCount++;
            }
            Bounds = new Rect(0, 0, box.width, box.height);
            return true;
        }
        public void Dispose() => _surface.Dispose();
    }
}
