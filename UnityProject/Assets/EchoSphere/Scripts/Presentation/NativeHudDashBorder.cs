using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Unity owns rounded path tessellation and dash generation.
    internal sealed class NativeHudDashBorder : VisualElement
    {
        private readonly float[] _pattern={3,2};
        internal NativeHudDashBorder()
        {
            pickingMode=PickingMode.Ignore;style.position=Position.Absolute;
            style.left=style.right=style.top=style.bottom=-1;
            generateVisualContent+=Draw;
        }
        private void Draw(MeshGenerationContext context)
        {
            var w=contentRect.width;var h=contentRect.height;if(w<=1||h<=1)return;
            const float edge=.5f;float radius=Mathf.Min(2.5f,Mathf.Min(w,h)*.5f-edge);
            var painter=context.painter2D;painter.lineWidth=1;painter.strokeColor=new Color(1,207/255f,122/255f,.82f);
            // Close a complete dash cycle, retaining a regular pattern at corners.
            var perimeter=2*(w+h-4*edge-4*radius)+2*Mathf.PI*radius;
            var count=Mathf.Max(1,Mathf.RoundToInt(perimeter/5));_pattern[0]=3;_pattern[1]=Mathf.Max(.01f,perimeter/count-3);
            painter.SetDashPattern(_pattern);painter.dashOffset=0;painter.BeginPath();painter.MoveTo(new Vector2(edge+radius,edge));
            painter.LineTo(new Vector2(w-edge-radius,edge));painter.ArcTo(new Vector2(w-edge,edge),new Vector2(w-edge,edge+radius),radius);
            painter.LineTo(new Vector2(w-edge,h-edge-radius));painter.ArcTo(new Vector2(w-edge,h-edge),new Vector2(w-edge-radius,h-edge),radius);
            painter.LineTo(new Vector2(edge+radius,h-edge));painter.ArcTo(new Vector2(edge,h-edge),new Vector2(edge,h-edge-radius),radius);
            painter.LineTo(new Vector2(edge,edge+radius));painter.ArcTo(new Vector2(edge,edge),new Vector2(edge+radius,edge),radius);painter.ClosePath();painter.Stroke();
        }
    }
}
