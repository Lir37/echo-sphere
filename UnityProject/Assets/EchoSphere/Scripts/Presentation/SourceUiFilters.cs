using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    internal sealed class SourceUiFilters : IDisposable
    {
        private readonly Material _material;
        private readonly List<FilterFunctionDefinition> _definitions=new List<FilterFunctionDefinition>();
        private readonly Dictionary<string,FilterFunctionDefinition> _namedDefinitions=new Dictionary<string,FilterFunctionDefinition>();
        private readonly Dictionary<string,List<FilterFunction>> _cache=new Dictionary<string,List<FilterFunction>>();
        internal SourceUiFilters(){_material=new Material(Resources.Load<Shader>("EchoSphere/SourceUiFilters"));}
        private FilterFunctionDefinition Define(string name,int pass,int parameters)
        {
            if(_namedDefinitions.TryGetValue(name,out var existing))return existing;
            var definition=ScriptableObject.CreateInstance<FilterFunctionDefinition>();definition.name=name;definition.filterName=name;
            definition.parameters=Enumerable.Range(0,parameters).Select(index=>new FilterParameterDeclaration {name="value"+index,interpolationDefaultValue=index==3?new FilterParameter(Color.clear):new FilterParameter(index==0?1f:0f)}).ToArray();
            var effect=new PostProcessingPass {material=_material,passIndex=pass,parameterBindings=pass==2?new[]{new ParameterBinding {index=0,name="_OffsetX"},new ParameterBinding {index=1,name="_OffsetY"},new ParameterBinding {index=2,name="_Sigma"},new ParameterBinding {index=3,name="_CssColor"}}:new[]{new ParameterBinding {index=0,name="_Amount"}}};
            if(pass==2){PostProcessingMargins Margins(FilterFunction function){var sigma=function.GetParameter(2).floatValue*4f;var x=function.GetParameter(0).floatValue;var y=function.GetParameter(1).floatValue;return new PostProcessingMargins {left=Mathf.Max(0,sigma-x),right=Mathf.Max(0,sigma+x),top=Mathf.Max(0,sigma-y),bottom=Mathf.Max(0,sigma+y)};}effect.computeRequiredReadMarginsCallback=Margins;effect.computeRequiredWriteMarginsCallback=Margins;}
            effect.applySettingsCallback=(properties,context)=>{
                properties.SetFloat("_CssDpr",context.scaledPixelsPerPoint);properties.SetFloat("_CssReadsGamma",context.readsGamma?1f:0f);
                // Unity 6.3 calls this callback INSTEAD OF parameterBindings.
                var function=context.filterFunction;
                if(pass==2){properties.SetFloat("_OffsetX",function.GetParameter(0).floatValue);properties.SetFloat("_OffsetY",function.GetParameter(1).floatValue);properties.SetFloat("_Sigma",function.GetParameter(2).floatValue);properties.SetVector("_CssColor",function.GetParameter(3).colorValue);}
                else properties.SetFloat("_Amount",function.GetParameter(0).floatValue);
            };
            definition.passes=new[]{effect};_definitions.Add(definition);_namedDefinitions[name]=definition;return definition;
        }
        internal void ApplyNativeGameField(VisualElement element)
        {
            var filters=new List<FilterFunction>();
            foreach(var item in new[]{("saturate",0,1.16f),("contrast",3,1.08f),("brightness",1,.98f)}){
                var filter=new FilterFunction(Define("native-field-"+item.Item1,item.Item2,1));filter.AddParameter(new FilterParameter(item.Item3));filters.Add(filter);
            }
            element.style.filter=filters;
        }
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
        internal void Apply(VisualCss sheet,VisualCssNode node,VisualElement element,float dpr,bool surfaceFiltered=false)
        {
            var source=surfaceFiltered?"none":sheet.Get(node,"filter","none");var opacity=sheet.Get(node,"opacity","1");var key=source+"|"+opacity+"|"+sheet.Get(node,"color","#000000");
            var xAxis=element.worldTransform.MultiplyVector(Vector3.right);_material.SetFloat("_CssDpr",dpr*Mathf.Max(.0001f,xAxis.magnitude));
            if(!_cache.TryGetValue(key,out var filters)){
                filters=new List<FilterFunction>();
                foreach(Match match in Regex.Matches(source,@"([a-z-]+)\(([^()]*(?:\([^()]*\)[^()]*)*)\)")){
                    var name=match.Groups[1].Value;var argument=match.Groups[2].Value.Trim();var words=VisualCss.Split(argument,' ').Where(value=>value.Length>0).ToArray();
                    if(name=="drop-shadow"){
                        var lengths=words.Where(value=>Regex.IsMatch(value,@"^[-+\d.]")).Select(value=>(float)sheet.Length(value,node.Width)).ToArray();var cssColor=VisualCssColor.Parse(words.FirstOrDefault(value=>value.StartsWith("#",StringComparison.Ordinal)||value.Contains("(")||value=="currentColor")??sheet.Get(node,"color","#000000"),sheet.Get(node,"color","#000000"));
                        var function=new FilterFunction(Define("source-drop-shadow",2,4));function.AddParameter(new FilterParameter(lengths.Length>0?lengths[0]:0));function.AddParameter(new FilterParameter(lengths.Length>1?lengths[1]:0));function.AddParameter(new FilterParameter(lengths.Length>2?lengths[2]:0));function.AddParameter(new FilterParameter(new Color((float)cssColor.R,(float)cssColor.G,(float)cssColor.B,(float)cssColor.A)));filters.Add(function);continue;
                    }
                    var amount=argument.EndsWith("%",StringComparison.Ordinal)?float.Parse(argument.TrimEnd('%'),CultureInfo.InvariantCulture)*.01f:name=="blur"?(float)sheet.Length(argument,node.Width):float.Parse(argument,CultureInfo.InvariantCulture);
                    if(name=="saturate"||name=="brightness"||name=="contrast"){var custom=new FilterFunction(Define("source-"+name,name=="saturate"?0:name=="brightness"?1:3,1));custom.AddParameter(new FilterParameter(amount));filters.Add(custom);continue;}
                    var enumName=name=="hue-rotate"?"HueRotate":char.ToUpperInvariant(name[0])+name.Substring(1);
                    if(!Enum.TryParse<FilterFunctionType>(enumName,out var kind)&&!Enum.TryParse("Builtin"+enumName,out kind))throw new InvalidOperationException("Unimplemented authored source filter "+name);
                    var builtin=new FilterFunction(kind);builtin.AddParameter(new FilterParameter(amount));filters.Add(builtin);
                }
                var alpha=float.Parse(opacity,CultureInfo.InvariantCulture);if(alpha!=1f){var function=new FilterFunction(FilterFunctionType.Opacity);function.AddParameter(new FilterParameter(alpha));filters.Add(function);}
                // Replacing a completed transition / animation value should not
                // retain an unbounded sequence of runtime filter definitions.
                if(_cache.Count>64)_cache.Clear();_cache[key]=filters;
            }
            element.style.opacity=1f;element.style.filter=filters;
        }
#endif
        public void Dispose(){foreach(var definition in _definitions)UnityEngine.Object.Destroy(definition);UnityEngine.Object.Destroy(_material);}
    }
}
