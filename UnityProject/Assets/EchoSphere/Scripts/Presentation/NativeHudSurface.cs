using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Typed GPU fill/shadow for retained HUD elements. No CSS, DOM or raster parser.
    internal sealed class NativeHudSurface : IDisposable
    {
        private readonly VisualElement _host;
        private readonly Image _image;
        private readonly NativeMenuBrush _brush=new NativeMenuBrush();
        private NativeMenuBrush.Paint _paint;
        private float _width=-1,_height=-1,_scale=-1;
        private bool _dirty=true,_visible=true;
        internal NativeHudSurface(VisualElement host,NativeMenuBrush.Paint paint)
        {
            _host=host;_paint=paint;
            _image=new Image {name="native-hud-paint",pickingMode=PickingMode.Ignore,scaleMode=ScaleMode.StretchToFill};
            _image.style.position=Position.Absolute;host.hierarchy.Insert(0,_image);
        }
        internal void SetPaint(NativeMenuBrush.Paint paint){if(ReferenceEquals(_paint,paint))return;_paint=paint;_dirty=true;}
        internal void SetVisible(bool visible){_visible=visible;_image.style.display=visible?DisplayStyle.Flex:DisplayStyle.None;}
        internal void Prepare(float scale)
        {
            var w=_host.layout.width;var h=_host.layout.height;
            if(!_visible||w<=0||h<=0||!float.IsFinite(w+h)||_host.resolvedStyle.display==DisplayStyle.None){_image.style.display=DisplayStyle.None;return;}
            _image.style.display=DisplayStyle.Flex;
            if(!_dirty&&w==_width&&h==_height&&scale==_scale)return;
            _brush.Render(w,h,scale,_paint);_width=w;_height=h;_scale=scale;_dirty=false;
            _image.image=_brush.Texture;var b=_brush.Bounds;
            // An absolute child uses its parent's padding box origin (inside border).
            _image.style.left=b.x-_host.resolvedStyle.borderLeftWidth;_image.style.top=b.y-_host.resolvedStyle.borderTopWidth;
            _image.style.width=b.width;_image.style.height=b.height;_image.style.display=DisplayStyle.Flex;
        }
        public void Dispose(){_image.RemoveFromHierarchy();_brush.Dispose();}
    }
}
