#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    internal static class SourceUiRange
    {
        private static readonly Dictionary<string,Texture2D> Parts=new Dictionary<string,Texture2D>();
        internal static void Prepare()
        {foreach(var name in new[]{"range-left","range-filled","range-empty","range-right","range-thumb"})if(!Parts.ContainsKey(name))Parts.Add(name,Resources.Load<Texture2D>("EchoSphere/SourceUiControls/"+name));}
        internal static void Apply(Slider slider)
        {
            slider.style.height=16;slider.pageSize=0;
            foreach(var name in new[]{"unity-drag-container","unity-tracker","unity-dragger","unity-dragger-border"}){
                var element=slider.Q<VisualElement>(name);if(element==null)continue;
                element.style.marginLeft=element.style.marginRight=element.style.marginTop=element.style.marginBottom=0;element.style.paddingLeft=element.style.paddingRight=element.style.paddingTop=element.style.paddingBottom=0;
                if(name=="unity-dragger"||name=="unity-dragger-border"){element.style.width=16;element.style.height=16;}
                if(name!="unity-drag-container")element.style.opacity=0;
            }
            var input=slider.Q<VisualElement>(className:"unity-base-slider__input");if(input!=null){input.style.height=16;input.style.marginLeft=input.style.marginRight=input.style.marginTop=input.style.marginBottom=0;input.style.flexGrow=1;}
        }
        internal static void Paint(Slider slider,MeshGenerationContext context,Matrix4x4? matrix=null)
        {
            var fraction=(slider.value-slider.lowValue)/Mathf.Max(.00001f,slider.highValue-slider.lowValue);
            foreach(var part in VisualRangeLayout.Build(slider.layout.width,fraction)){
                if(!Parts.TryGetValue(part.Name,out var texture))continue;
                SourceUiDocument.Quad(context,texture,new Rect((float)part.X,0,(float)part.Width,16),new Rect((float)part.U,0,(float)part.Uw,1),matrix);
            }
        }
    }
}
#endif
