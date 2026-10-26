using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
namespace EchoSphere.Presentation
{
    // Unity owns capture, clipping, dependency ordering and temporary textures.
    // This owner only attaches/restores a native style for the lifetime of a view.
    internal sealed class NativeEngineBackdrop:IDisposable
    {
        private readonly VisualElement _element;
        private readonly StyleList<FilterFunction> _original,_effect;
        private bool _enabled=true,_disposed;
        private readonly Material _material;
        private readonly FilterFunctionDefinition _definition;
        private readonly float _sigma;
        internal NativeEngineBackdrop(VisualElement element,float sigma)
        {
            _element=element??throw new ArgumentNullException(nameof(element));
            if(!float.IsFinite(sigma)||sigma<0)throw new ArgumentOutOfRangeException(nameof(sigma));
            _original=element.style.backdropFilter;
            _sigma=sigma;
            _material=new Material(Resources.Load<Shader>("EchoSphere/NativeBackdropGaussian")){name="Native backdrop Gaussian",hideFlags=HideFlags.DontSave};
            _definition=ScriptableObject.CreateInstance<FilterFunctionDefinition>();_definition.name=_definition.filterName="native-backdrop-gaussian";
            _definition.parameters=new[]{new FilterParameterDeclaration{name="sigma",interpolationDefaultValue=new FilterParameter(0f)}};
            PostProcessingMargins Support(FilterFunction f){float extent=Mathf.Ceil(4*_sigma);return new PostProcessingMargins{left=extent,right=extent,top=extent,bottom=extent};}
            _definition.passes=new[]{new PostProcessingPass{material=_material,passIndex=0,computeRequiredWriteMarginsCallback=Support,applySettingsCallback=ApplySettings},new PostProcessingPass{material=_material,passIndex=1,computeRequiredReadMarginsCallback=Support,applySettingsCallback=ApplySettings}};
            var blur=new FilterFunction(_definition);blur.AddParameter(new FilterParameter(sigma));
            _effect=new List<FilterFunction>{blur};element.style.backdropFilter=_effect;
        }
        private void ApplySettings(MaterialPropertyBlock properties,FilterPassContext context)
        {
            float density=context.scaledPixelsPerPoint;var matrix=_element.worldTransform;var inverse=matrix.inverse;var bounds=_element.worldBound;
            properties.SetFloat("_Sigma",_sigma);properties.SetFloat("_CssDpr",density);properties.SetFloat("_CssReadsGamma",context.readsGamma?1:0);
            bool cached=NativeGaussianWeights.TryGet(_sigma*density,out var weights);properties.SetInt("_BackdropWeightCount",cached?Mathf.CeilToInt(4*_sigma*density)+1:0);if(cached)properties.SetVectorArray("_BackdropWeights",weights);
            properties.SetVector("_Roi",new Vector4(bounds.x,bounds.y,bounds.width,bounds.height));properties.SetVector("_PanelSize",new Vector4(_element.layout.width,_element.layout.height,0,0));
            properties.SetVector("_ToPanelX",new Vector4(inverse.m00,inverse.m01,inverse.m03,0));properties.SetVector("_ToPanelY",new Vector4(inverse.m10,inverse.m11,inverse.m13,0));
            properties.SetVector("_ToScreenX",new Vector4(matrix.m00,matrix.m01,matrix.m03,0));properties.SetVector("_ToScreenY",new Vector4(matrix.m10,matrix.m11,matrix.m13,0));
        }
        internal void SetVisible(bool visible){if(_disposed||_enabled==visible)return;_enabled=visible;_element.style.backdropFilter=visible?_effect:_original;}
        public void Dispose(){if(_disposed)return;_disposed=true;_element.style.backdropFilter=_original;UnityEngine.Object.Destroy(_definition);UnityEngine.Object.Destroy(_material);}
    }
#if UNITY_EDITOR
    public static class NativeBackdropValidation
    {
        public static bool UseLegacy;
    }
#endif
}
