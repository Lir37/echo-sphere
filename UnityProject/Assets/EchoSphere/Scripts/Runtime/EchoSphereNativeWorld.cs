using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeWorldField _nativeWorldField;
        private NativeWorldAmbient _nativeWorldAmbient;
        private NativeWorldPickups _nativeWorldPickups;
        private NativeWorldMinions _nativeWorldMinions;
        private NativeWorldEnergyBolts _nativeWorldEnergyBolts;
        private void RenderNativeWorldFrame(float width,float height)
        {
            if(_nativeWorldField==null)_nativeWorldField=new NativeWorldField();
            if(_nativeWorldAmbient==null)_nativeWorldAmbient=new NativeWorldAmbient();
            if(_nativeWorldPickups==null)_nativeWorldPickups=new NativeWorldPickups();_nativeWorldPickups.BeginFrame();
            if(_nativeWorldMinions==null)_nativeWorldMinions=new NativeWorldMinions();_nativeWorldMinions.BeginFrame();
            if(_nativeWorldEnergyBolts==null)_nativeWorldEnergyBolts=new NativeWorldEnergyBolts();_nativeWorldEnergyBolts.BeginFrame();
            var sourceField=Visual_renderer.Scope["drawVoidField"];
            var sourceAmbient=Visual_renderer.Scope["drawVoidAmbient"];
            var sourceRegion=Visual_renderer.Scope["drawRegionLayer"];
            var sourceXp=Visual_renderer.Scope["drawModernXp"];var sourceHealth=Visual_renderer.Scope["drawModernHealth"];
            var sourceMinion=Visual_renderer.Scope["drawModernMinion"];var sourceEnergy=Visual_renderer.Scope["drawEnergyBolt"];
            try{
                Visual_renderer.Scope["drawVoidField"]=VisualJs.Fn(args=>{_nativeWorldField.Draw(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,(float)args[4].Number,(float)args[5].Number,(float)args[6].Number,(float)args[7].Number);return default;});
                Visual_renderer.Scope["drawVoidAmbient"]=VisualJs.Fn(args=>{_nativeWorldAmbient.Draw(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,args[3].Number);return default;});
                Visual_renderer.Scope["drawRegionLayer"]=VisualJs.Fn(args=>{DrawNativeRegionWorld(args[1]);return default;});
                Visual_renderer.Scope["drawModernXp"]=VisualJs.Fn(args=>{ColorUtility.TryParseHtmlString(args[4].Text,out var color);_nativeWorldPickups.DrawXp(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,(float)args[3].Number,color,(float)args[5].Number);return default;});
                Visual_renderer.Scope["drawModernHealth"]=VisualJs.Fn(args=>{ColorUtility.TryParseHtmlString(args[3].Text,out var color);_nativeWorldPickups.DrawHealth(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,color,_sourceRenderState["time"].Number);return default;});
                Visual_renderer.Scope["drawModernMinion"]=VisualJs.Fn(args=>{ColorUtility.TryParseHtmlString(args[5].Text,out var color);_nativeWorldMinions.Draw(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,(float)args[3].Number,(float)args[4].Number,color,_sourceRenderState["time"].Number);return default;});
                Visual_renderer.Scope["drawEnergyBolt"]=VisualJs.Fn(args=>{_nativeWorldEnergyBolts.Draw(_sourceCanvas,args[1]["x"].Number,args[1]["y"].Number,args[2]["x"].Number,args[2]["y"].Number,args[3].Number,_sourceRenderState["time"].Number);return default;});
                Visual_renderer.Scope["render"].Call(_sourceCanvas.Canvas.Value,_sourceRenderState,width,height);
            }finally{Visual_renderer.Scope["drawVoidField"]=sourceField;Visual_renderer.Scope["drawVoidAmbient"]=sourceAmbient;Visual_renderer.Scope["drawRegionLayer"]=sourceRegion;Visual_renderer.Scope["drawModernXp"]=sourceXp;Visual_renderer.Scope["drawModernHealth"]=sourceHealth;Visual_renderer.Scope["drawModernMinion"]=sourceMinion;Visual_renderer.Scope["drawEnergyBolt"]=sourceEnergy;}
        }
        private void DisposeNativeWorld(){_nativeWorldField?.Dispose();_nativeWorldField=null;_nativeWorldAmbient?.Dispose();_nativeWorldAmbient=null;_nativeWorldRegion?.Dispose();_nativeWorldRegion=null;_nativeWorldPickups?.Dispose();_nativeWorldPickups=null;_nativeWorldMinions?.Dispose();_nativeWorldMinions=null;_nativeWorldEnergyBolts?.Dispose();_nativeWorldEnergyBolts=null;}
    }
}
