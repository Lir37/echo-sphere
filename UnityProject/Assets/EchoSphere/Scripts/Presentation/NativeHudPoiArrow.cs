using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // RadarHud's CSS border triangle is a native vector, not a bitmap/SDF substitute.
    internal sealed class NativeHudPoiArrow : VisualElement,IDisposable
    {
        private Color _color;
        private NativeUiAlphaShadow _shadow;private VisualElement _layers;
        private sealed class Triangle:VisualElement
        {
            private readonly Color _color;
            internal Triangle(Color color){_color=color;pickingMode=PickingMode.Ignore;style.position=Position.Absolute;style.left=style.right=style.top=style.bottom=0;generateVisualContent+=Draw;}
            private void Draw(MeshGenerationContext context){var p=context.painter2D;p.fillColor=_color;p.BeginPath();p.MoveTo(new Vector2(5,0));p.LineTo(new Vector2(10,10));p.LineTo(new Vector2(0,10));p.ClosePath();p.Fill();}
        }
        internal NativeHudPoiArrow()
        {
            pickingMode=PickingMode.Ignore;style.position=Position.Absolute;style.left=style.right=style.top=style.bottom=0;
            style.display=DisplayStyle.None;
        }
        internal void Set(Color color,NativeUiAlphaShadow shadow)
        {
            if(_layers!=null&&_color==color&&_shadow==shadow)return;
            Dispose();_color=color;_shadow=shadow;
            // Preserve the authored 10px triangle, including native vector AA.
            _layers=shadow.Build(this,10,10,layer=>layer.Add(new Triangle(color)));
        }
        public void Dispose(){if(_layers!=null)_shadow.Remove(_layers);_layers=null;_shadow=null;}
    }
}
