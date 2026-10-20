using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Native TextCore owns line breaking; presentation owns authored line boxes.
    internal sealed class NativeUiLineBoxes
    {
        private readonly Dictionary<Label,(string Text,float Width)> _cache=new Dictionary<Label,(string,float)>();
        internal bool Fit(Label label,float lineHeight)
        {
            float width=label.contentRect.width;if(width<=0||!float.IsFinite(width))return false;
            if(_cache.TryGetValue(label,out var previous)&&previous.Text==label.text&&Mathf.Abs(previous.Width-width)<.01f)return false;
            var one=label.MeasureTextSize("M",0,VisualElement.MeasureMode.Undefined,0,VisualElement.MeasureMode.Undefined);
            var all=label.MeasureTextSize(label.text,width,VisualElement.MeasureMode.Exactly,0,VisualElement.MeasureMode.Undefined);
            if(one.y<=0||!float.IsFinite(one.y+all.y))return false;
            label.style.height=Mathf.Max(1,Mathf.RoundToInt(all.y/one.y))*lineHeight;_cache[label]=(label.text,width);return true;
        }
    }
}
