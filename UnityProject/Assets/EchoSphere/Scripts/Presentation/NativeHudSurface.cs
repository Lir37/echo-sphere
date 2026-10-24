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
        private NativeMenuBrushCache.Entry _skin;
        private NativeMenuBrush.Paint _paint;
        private float _width=-1,_height=-1,_scale=-1;
        private bool _dirty=true,_visible=true;
        internal static uint PaintVersion {get;private set;}
        internal VisualElement Host=>_host;
        internal static bool InViewport(VisualElement element,float margin=64)
        {
            var area=element.worldBound;area.xMin-=margin;area.yMin-=margin;area.xMax+=margin;area.yMax+=margin;
            for(var parent=element;parent!=null;parent=parent.parent){
                if(parent.resolvedStyle.display==DisplayStyle.None||parent.resolvedStyle.visibility==Visibility.Hidden)return false;
                if(parent!=element&&(parent.style.overflow==Overflow.Hidden||parent.ClassListContains("unity-scroll-view__content-viewport"))&&!area.Overlaps(parent.worldBound))return false;
            }
            return element.panel!=null&&area.Overlaps(element.panel.visualTree.worldBound);
        }
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
            float margin=Mathf.Max(ShadowMargin(_paint.OuterA),ShadowMargin(_paint.OuterB));
            if(!_visible||w<=0||h<=0||!float.IsFinite(w+h)||!InViewport(_host,margin)){_image.style.visibility=Visibility.Hidden;return;}
            _image.style.visibility=Visibility.Visible;
            _image.style.display=DisplayStyle.Flex;
            if(!_dirty&&w==_width&&h==_height&&scale==_scale)return;
            var old=_skin;_skin=NativeMenuBrushCache.Acquire(w,h,scale,_paint);_width=w;_height=h;_scale=scale;_dirty=false;
            PaintVersion++;
            _image.image=_skin.Brush.Texture;var b=_skin.Brush.Bounds;NativeMenuBrushCache.Release(old);
            // An absolute child uses its parent's padding box origin (inside border).
            _image.style.left=b.x-_host.resolvedStyle.borderLeftWidth;_image.style.top=b.y-_host.resolvedStyle.borderTopWidth;
            _image.style.width=b.width;_image.style.height=b.height;_image.style.display=DisplayStyle.Flex;
        }
        private static float ShadowMargin(NativeMenuBrush.Shadow? shadow)=>shadow.HasValue?shadow.Value.Blur*2+Mathf.Max(Mathf.Abs(shadow.Value.Offset.x),Mathf.Abs(shadow.Value.Offset.y))+Mathf.Max(0,shadow.Value.Spread):0;
        public void Dispose(){_image.image=null;_image.RemoveFromHierarchy();NativeMenuBrushCache.Release(_skin);_skin=null;}
    }
}
