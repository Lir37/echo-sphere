using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Typed UI Toolkit alpha-shadow authoring. Shares the existing native shader
    // kernel, with no CSS parser or dynamic drawing interpreter.
    internal sealed class NativeUiAlphaShadow : IDisposable
    {
        private readonly Material _material;
        private readonly FilterFunctionDefinition _definition;
        private readonly List<VisualElement> _targets=new List<VisualElement>();
        private readonly List<FilterFunction> _filters=new List<FilterFunction>();
        internal NativeUiAlphaShadow(float sigma,Color color)
        {
            _material=new Material(Resources.Load<Shader>("EchoSphere/SourceUiFilters")){name="Native mobile glyph alpha shadow"};
            _definition=ScriptableObject.CreateInstance<FilterFunctionDefinition>();_definition.filterName="native-mobile-glyph-shadow";_definition.parameters=Array.Empty<FilterParameterDeclaration>();
            PostProcessingMargins Margins(FilterFunction f)=>new PostProcessingMargins {left=4*sigma,right=4*sigma,top=4*sigma,bottom=4*sigma};
            _definition.passes=new[]{new PostProcessingPass {material=_material,passIndex=2,computeRequiredReadMarginsCallback=Margins,computeRequiredWriteMarginsCallback=Margins,applySettingsCallback=(properties,context)=>{properties.SetFloat("_CssDpr",context.scaledPixelsPerPoint);properties.SetFloat("_CssReadsGamma",context.readsGamma?1:0);properties.SetFloat("_OffsetX",0);properties.SetFloat("_OffsetY",0);properties.SetFloat("_Sigma",sigma);properties.SetVector("_CssColor",color);}}};_filters.Add(new FilterFunction(_definition));
        }
        internal void Apply(VisualElement target){target.style.filter=_filters;_targets.Add(target);}
        internal void Clear(){foreach(var target in _targets)target.style.filter=StyleKeyword.Null;_targets.Clear();}
        public void Dispose(){Clear();UnityEngine.Object.Destroy(_definition);UnityEngine.Object.Destroy(_material);}
    }
}
