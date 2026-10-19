using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // RadarHud's CSS border triangle is a native vector, not a bitmap/SDF substitute.
    internal sealed class NativeHudPoiArrow : VisualElement
    {
        private Color _color;
        internal NativeHudPoiArrow()
        {
            pickingMode=PickingMode.Ignore;style.position=Position.Absolute;style.left=style.right=style.top=style.bottom=0;
            style.display=DisplayStyle.None;generateVisualContent+=Draw;
        }
        internal void Set(Color color,List<FilterFunction> filter)
        {if(_color!=color){_color=color;MarkDirtyRepaint();}style.filter=filter;}
        private void Draw(MeshGenerationContext context)
        {
            // Authored 5px side borders / 10px bottom border define a 10px triangle.
            // Do not scale those vertices to Yoga's device-pixel-snapped host size.
            const float width=10,height=10;var p=context.painter2D;p.fillColor=_color;
            p.BeginPath();p.MoveTo(new Vector2(width*.5f,0));p.LineTo(new Vector2(width,height));p.LineTo(new Vector2(0,height));p.ClosePath();p.Fill();
        }
    }
}
