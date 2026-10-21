using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Preserve conceptStyle.css button { backdrop-filter: blur(8px) } while
    // controls/layout move to native UI. Shares the existing ordered filter
    // preparation and snapshot cache; full native pause composition is separate.
    internal sealed class NativePauseChromeComposition : IDisposable
    {
        private readonly NativePauseChromeView _view;
        private readonly VisualElement[] _fills = new VisualElement[7];
        private readonly VisualElement[] _hosts = new VisualElement[7], _parents = new VisualElement[7];
        private readonly int[] _indices = new int[7];
        private readonly ButtonLayout[] _layouts = new ButtonLayout[7];
        private readonly IDisposable[] _backdrops = new IDisposable[7];
        private readonly NativeMenuBrush[] _footerPaint = new NativeMenuBrush[2];
        private readonly Vector3[] _footerGeometry = new Vector3[2];
        private bool _disposed;
        internal int ButtonCount => _backdrops.Length;
        internal const float Sigma = 8;
        private struct ButtonLayout
        {
            internal StyleLength Left, Right, Width, Basis;
            internal StyleFloat Grow, Shrink;
            internal ButtonLayout(Button button)
            { var s=button.style;Left=s.marginLeft;Right=s.marginRight;Width=s.width;Basis=s.flexBasis;Grow=s.flexGrow;Shrink=s.flexShrink; }
            internal void Restore(Button button)
            { var s=button.style;s.marginLeft=Left;s.marginRight=Right;s.width=Width;s.flexBasis=Basis;s.flexGrow=Grow;s.flexShrink=Shrink; }
        }
        internal NativePauseChromeComposition(NativePauseChromeView view, SourceUiDocument document)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            if (document == null) throw new ArgumentNullException(nameof(document));
            view.SetCompositionOwnership(true);
            for (int i = 0; i < view.Buttons.Length; i++)
            {
                var button = view.Buttons[i];
                _layouts[i]=new ButtonLayout(button);var parent=_parents[i]=button.parent;int index=0;
                while(parent.hierarchy[index]!=button)index++;_indices[i]=index;
                var host=_hosts[i]=new VisualElement {name="pause-chrome-button-effect-host",pickingMode=PickingMode.Ignore};
                host.style.flexDirection=FlexDirection.Column;host.style.flexShrink=0;host.style.minWidth=0;
                host.style.marginLeft=button.style.marginLeft;host.style.marginRight=button.style.marginRight;
                if(i>=1&&i<=4){host.style.flexGrow=1;host.style.flexShrink=1;host.style.flexBasis=0;button.style.width=Length.Percent(100);}
                if(i==0){host.style.width=host.style.height=36;}
                button.style.marginLeft=button.style.marginRight=0;button.style.flexGrow=0;button.style.flexShrink=0;button.style.flexBasis=StyleKeyword.Auto;
                button.RemoveFromHierarchy();parent.hierarchy.Insert(index,host);host.Add(button);
                var fill = _fills[i] = i >= 5 ? (VisualElement)new Image { name = "pause-chrome-button-fill", pickingMode = PickingMode.Ignore, scaleMode = ScaleMode.StretchToFill } : new VisualElement { name = "pause-chrome-button-fill", pickingMode = PickingMode.Ignore };
                if(i>=5)_footerPaint[i-5]=new NativeMenuBrush();
                fill.style.position = Position.Absolute;
                fill.style.borderLeftWidth = fill.style.borderRightWidth = fill.style.borderTopWidth = fill.style.borderBottomWidth = 1;
                host.hierarchy.Insert(0, fill);
                // Registration inserts the blur prefix before this native fill,
                // so color/border are painted after blur, then native icon/text.
                _backdrops[i] = document.RegisterNativeBackdrop(host, Sigma);
            }
        }
        internal void Prepare()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(NativePauseChromeComposition));
            for (int i = 0; i < _fills.Length; i++)
            {
                var button = _view.Buttons[i]; var fill = _fills[i]; var host=_hosts[i];
                if (!float.IsFinite(button.layout.width + button.layout.height) || button.layout.width <= 0 || button.layout.height <= 0) continue;
                var paint = _view.PaintFor(button);
                if(i>=5){
                    var scale=button.panel?.scaledPixelsPerPoint??1;
                    var geometry=new Vector3(button.layout.width,button.layout.height,scale);var brush=_footerPaint[i-5];
                    if(_footerGeometry[i-5]!=geometry){
                        brush.Render(geometry.x,geometry.y,scale,new NativeMenuBrush.Paint {
                            Radius=paint.Radius,Border=1,Fill=paint.Fill,BorderColor=paint.Border,
                            Inset=new NativeMenuBrush.Shadow(0,new Vector2(0,1),new Color(1,1,1,.018f)),
                            OuterA=new NativeMenuBrush.Shadow(24,new Vector2(0,10),new Color(0,0,0,.14f))
                        });_footerGeometry[i-5]=geometry;
                    }
                    fill.style.borderLeftWidth=fill.style.borderRightWidth=fill.style.borderTopWidth=fill.style.borderBottomWidth=0;
                    fill.style.left=brush.Bounds.x;fill.style.top=brush.Bounds.y;fill.style.width=brush.Bounds.width;fill.style.height=brush.Bounds.height;
                    ((Image)fill).image=brush.Texture;
                    host.style.borderTopLeftRadius=host.style.borderTopRightRadius=host.style.borderBottomLeftRadius=host.style.borderBottomRightRadius=paint.Radius;
                    continue;
                }
                fill.style.left = fill.style.top = 0;
                fill.style.width = button.layout.width; fill.style.height = button.layout.height;
                fill.style.backgroundColor = paint.Fill;
                fill.style.borderLeftColor = fill.style.borderRightColor = fill.style.borderTopColor = fill.style.borderBottomColor = paint.Border;
                fill.style.borderTopLeftRadius = fill.style.borderTopRightRadius = fill.style.borderBottomLeftRadius = fill.style.borderBottomRightRadius = paint.Radius;
                host.style.borderTopLeftRadius=host.style.borderTopRightRadius=host.style.borderBottomLeftRadius=host.style.borderBottomRightRadius=paint.Radius;
            }
        }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true;
            foreach (var backdrop in _backdrops) backdrop?.Dispose();
            foreach (var fill in _fills) fill?.RemoveFromHierarchy();
            foreach(var brush in _footerPaint)brush?.Dispose();
            for(int i=0;i<_hosts.Length;i++){var button=_view.Buttons[i];button.RemoveFromHierarchy();_hosts[i].RemoveFromHierarchy();_parents[i].hierarchy.Insert(Mathf.Min(_indices[i],_parents[i].hierarchy.childCount),button);_layouts[i].Restore(button);}
            _view.SetCompositionOwnership(false);
        }
    }
}
