using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Native shadow and sharp-content layers. A horizontal/vertical Gaussian
    // filters only alpha; the original content never passes through that blur.
    internal sealed class NativeUiAlphaShadow : IDisposable
    {
        private readonly Material _material;
        private readonly FilterFunctionDefinition _shadowDefinition,_contentDefinition;
        private readonly List<VisualElement> _targets=new List<VisualElement>();
        private readonly List<FilterFunction> _shadowFilters,_contentFilters;
        internal NativeUiAlphaShadow(float sigma,Color color)
        {
            _material=new Material(Resources.Load<Shader>("EchoSphere/NativeUiAlphaShadow")){name="Native separable UI alpha shadow",hideFlags=HideFlags.DontSave};
            _shadowDefinition=ScriptableObject.CreateInstance<FilterFunctionDefinition>();_shadowDefinition.filterName="native-mobile-glyph-shadow";_shadowDefinition.parameters=Array.Empty<FilterParameterDeclaration>();
            _contentDefinition=ScriptableObject.CreateInstance<FilterFunctionDefinition>();_contentDefinition.filterName="native-ui-sharp-content";_contentDefinition.parameters=Array.Empty<FilterParameterDeclaration>();
            PostProcessingMargins Margins(FilterFunction f)=>new PostProcessingMargins {left=4*sigma,right=4*sigma,top=4*sigma,bottom=4*sigma};
            PostProcessingMargins Horizontal(FilterFunction f)=>new PostProcessingMargins {left=4*sigma,right=4*sigma};
            PostProcessingMargins Vertical(FilterFunction f)=>new PostProcessingMargins {top=4*sigma,bottom=4*sigma};
            void Settings(MaterialPropertyBlock properties,FilterPassContext context){
                properties.SetFloat("_CssDpr",context.scaledPixelsPerPoint);properties.SetFloat("_CssReadsGamma",context.readsGamma?1:0);properties.SetFloat("_Sigma",sigma);properties.SetVector("_CssColor",color);
                float pixels=sigma*context.scaledPixelsPerPoint;bool cached=NativeGaussianWeights.TryGet(pixels,out var weights);properties.SetInt("_WeightCount",cached?Mathf.CeilToInt(4*pixels)+1:0);if(cached)properties.SetVectorArray("_Weights",weights);
            }
            _shadowDefinition.passes=new[]{new PostProcessingPass{material=_material,passIndex=0,computeRequiredReadMarginsCallback=Horizontal,computeRequiredWriteMarginsCallback=Horizontal,applySettingsCallback=Settings},new PostProcessingPass{material=_material,passIndex=1,computeRequiredReadMarginsCallback=Vertical,computeRequiredWriteMarginsCallback=Vertical,applySettingsCallback=Settings}};
            // Keep the same one-pass sharp-content capture and margins as the
            // old drop shadow, including native AA and nested filter order.
            _contentDefinition.passes=new[]{new PostProcessingPass{material=_material,passIndex=2,computeRequiredReadMarginsCallback=Margins,computeRequiredWriteMarginsCallback=Margins,applySettingsCallback=Settings}};
            _shadowFilters=new List<FilterFunction>{new FilterFunction(_shadowDefinition)};_contentFilters=new List<FilterFunction>{new FilterFunction(_contentDefinition)};
        }
        internal VisualElement Build(VisualElement parent,float width,float height,Action<VisualElement> content,bool intrinsic=false)
        {
            var host=new VisualElement{name="native-alpha-shadow-layers",pickingMode=PickingMode.Ignore};host.style.width=width;host.style.height=height;host.style.flexShrink=0;parent.Add(host);
            if(intrinsic)host.style.width=host.style.height=StyleKeyword.Auto;
            for(int i=0;i<2;i++){
                var layer=new VisualElement{name=i==0?"native-alpha-shadow":"native-alpha-content",pickingMode=PickingMode.Ignore};layer.style.position=Position.Absolute;layer.style.left=layer.style.top=0;layer.style.width=width;layer.style.height=height;host.Add(layer);
                // The flow content must size an intrinsic row, as in the
                // original. A fixed intermediate row changes Yoga pixel rounding.
                if(intrinsic){layer.style.width=layer.style.height=StyleKeyword.Auto;if(i==0)layer.style.right=layer.style.bottom=0;else layer.style.position=Position.Relative;}
                content(layer);layer.style.filter=i==0?_shadowFilters:_contentFilters;_targets.Add(layer);
            }
            return host;
        }
        internal void Remove(VisualElement root){for(int i=_targets.Count-1;i>=0;i--)if(root.Contains(_targets[i])){_targets[i].style.filter=StyleKeyword.Null;_targets.RemoveAt(i);}root.RemoveFromHierarchy();}
        internal void Clear(){foreach(var target in _targets)target.style.filter=StyleKeyword.Null;_targets.Clear();}
        public void Dispose(){Clear();UnityEngine.Object.Destroy(_shadowDefinition);UnityEngine.Object.Destroy(_contentDefinition);UnityEngine.Object.Destroy(_material);}
    }
}
