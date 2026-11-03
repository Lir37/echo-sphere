using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace EchoSphere.Presentation {
    // Unity owns the alpha blur, sharp source composition and filter resources.
    internal sealed class NativeUiAlphaShadow : IDisposable {
        private readonly List<VisualElement> _targets=new List<VisualElement>();
        private readonly List<FilterFunction> _filters;
        internal NativeUiAlphaShadow(float sigma,Color color){
            var shadow=new FilterFunction(FilterFunctionType.DropShadow);
            shadow.AddParameter(new FilterParameter(0f));shadow.AddParameter(new FilterParameter(0f));
            shadow.AddParameter(new FilterParameter(sigma));shadow.AddParameter(new FilterParameter(color));
            _filters=new List<FilterFunction>{shadow};
        }
        internal VisualElement Build(VisualElement parent,float width,float height,Action<VisualElement> content,bool intrinsic=false){
            var host=new VisualElement{name="native-alpha-shadow-layers",pickingMode=PickingMode.Ignore};host.style.width=width;host.style.height=height;host.style.flexShrink=0;parent.Add(host);
            if(intrinsic)host.style.width=host.style.height=StyleKeyword.Auto;
            var layer=new VisualElement{name="native-alpha-content",pickingMode=PickingMode.Ignore};
            if(!intrinsic){layer.style.position=Position.Absolute;layer.style.left=layer.style.top=0;layer.style.width=width;layer.style.height=height;}
            host.Add(layer);content(layer);layer.style.filter=_filters;_targets.Add(layer);return host;
        }
        internal void Remove(VisualElement root){for(int i=_targets.Count-1;i>=0;i--)if(root.Contains(_targets[i])){_targets[i].style.filter=StyleKeyword.Null;_targets.RemoveAt(i);}root.RemoveFromHierarchy();}
        internal void Clear(){foreach(var target in _targets)target.style.filter=StyleKeyword.Null;_targets.Clear();}
        public void Dispose(){Clear();}
    }
}
