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
        private NativeWorldSprites _nativeWorldSprites;
        private NativeWorldProjectiles _nativeWorldProjectiles;
        private void RenderNativeWorldFrame(float width,float height)
        {
            if(_nativeWorldField==null)_nativeWorldField=new NativeWorldField();
            if(_nativeWorldAmbient==null)_nativeWorldAmbient=new NativeWorldAmbient();
            if(_nativeWorldPickups==null)_nativeWorldPickups=new NativeWorldPickups();_nativeWorldPickups.BeginFrame();
            if(_nativeWorldMinions==null)_nativeWorldMinions=new NativeWorldMinions();_nativeWorldMinions.BeginFrame();
            if(_nativeWorldEnergyBolts==null)_nativeWorldEnergyBolts=new NativeWorldEnergyBolts();_nativeWorldEnergyBolts.BeginFrame();
            if(_nativeWorldSprites==null)_nativeWorldSprites=new NativeWorldSprites();_nativeWorldSprites.BeginFrame();
            if(_nativeWorldProjectiles==null)_nativeWorldProjectiles=new NativeWorldProjectiles();_nativeWorldProjectiles.BeginFrame();
            var sourceProjectile=Visual_renderer.Scope["drawEnemyProjectile"];var sourceBossProjectile=Visual_renderer.Scope["drawBossProjectile"];
            var sourceField=Visual_renderer.Scope["drawVoidField"];
            var sourceAmbient=Visual_renderer.Scope["drawVoidAmbient"];
            var sourceRegion=Visual_renderer.Scope["drawRegionLayer"];
            var sourceXp=Visual_renderer.Scope["drawModernXp"];var sourceHealth=Visual_renderer.Scope["drawModernHealth"];
            var sourceArt=Visual_renderer.Scope["drawArtSprite"];var sourcePart=Visual_renderer.Scope["drawStandardSpherePart"];
            var sourceMinion=Visual_renderer.Scope["drawModernMinion"];var sourceEnergy=Visual_renderer.Scope["drawEnergyBolt"];
            try{
                Visual_renderer.Scope["drawEnemyProjectile"]=VisualJs.Fn(args=>{bool sniper=args[5].Truth;ColorUtility.TryParseHtmlString(sniper?"#ffe066":"#ff6b55",out var color);_nativeWorldProjectiles.Draw(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,(float)args[3].Number,args[4]["x"].Number,args[4]["y"].Number,sniper?1:0,color,_sourceRenderState["time"].Number);return default;});
                Visual_renderer.Scope["drawBossProjectile"]=VisualJs.Fn(args=>{ColorUtility.TryParseHtmlString(args[4].Text,out var color);string type=args[5].Text;_nativeWorldProjectiles.Draw(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,(float)args[3].Number,args[6]["x"].Number,args[6]["y"].Number,type=="shooter"?2:type=="charger"?3:type=="summoner"?4:5,color,_sourceRenderState["time"].Number);return default;});
                Visual_renderer.Scope["drawVoidField"]=VisualJs.Fn(args=>{_nativeWorldField.Draw(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,(float)args[4].Number,(float)args[5].Number,(float)args[6].Number,(float)args[7].Number);return default;});
                Visual_renderer.Scope["drawVoidAmbient"]=VisualJs.Fn(args=>{_nativeWorldAmbient.Draw(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,args[3].Number);return default;});
                Visual_renderer.Scope["drawRegionLayer"]=VisualJs.Fn(args=>{DrawNativeRegionWorld(args[1]);return default;});
                Visual_renderer.Scope["drawModernXp"]=VisualJs.Fn(args=>{ColorUtility.TryParseHtmlString(args[4].Text,out var color);_nativeWorldPickups.DrawXp(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,(float)args[3].Number,color,(float)args[5].Number);return default;});
                Visual_renderer.Scope["drawModernHealth"]=VisualJs.Fn(args=>{ColorUtility.TryParseHtmlString(args[3].Text,out var color);_nativeWorldPickups.DrawHealth(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,color,_sourceRenderState["time"].Number);return default;});
                Visual_renderer.Scope["drawModernMinion"]=VisualJs.Fn(args=>{ColorUtility.TryParseHtmlString(args[5].Text,out var color);_nativeWorldMinions.Draw(_sourceCanvas,(float)args[1].Number,(float)args[2].Number,(float)args[3].Number,(float)args[4].Number,color,_sourceRenderState["time"].Number);return default;});
                Visual_renderer.Scope["drawEnergyBolt"]=VisualJs.Fn(args=>{_nativeWorldEnergyBolts.Draw(_sourceCanvas,args[1]["x"].Number,args[1]["y"].Number,args[2]["x"].Number,args[2]["y"].Number,args[3].Number,_sourceRenderState["time"].Number);return default;});
                Visual_renderer.Scope["drawArtSprite"]=VisualJs.Fn(args=>DrawNativeSprite(args,false));
                Visual_renderer.Scope["drawStandardSpherePart"]=VisualJs.Fn(args=>DrawNativeSprite(args,true));
                Visual_renderer.Scope["render"].Call(_sourceCanvas.Canvas.Value,_sourceRenderState,width,height);
            }finally{Visual_renderer.Scope["drawEnemyProjectile"]=sourceProjectile;Visual_renderer.Scope["drawBossProjectile"]=sourceBossProjectile;Visual_renderer.Scope["drawVoidField"]=sourceField;Visual_renderer.Scope["drawVoidAmbient"]=sourceAmbient;Visual_renderer.Scope["drawRegionLayer"]=sourceRegion;Visual_renderer.Scope["drawModernXp"]=sourceXp;Visual_renderer.Scope["drawModernHealth"]=sourceHealth;Visual_renderer.Scope["drawModernMinion"]=sourceMinion;Visual_renderer.Scope["drawEnergyBolt"]=sourceEnergy;Visual_renderer.Scope["drawArtSprite"]=sourceArt;Visual_renderer.Scope["drawStandardSpherePart"]=sourcePart;}
        }
        private V DrawNativeSprite(V[] args,bool part)
        {
            // Retain the existing availability gate, including first-call false
            // and procedural fallback. Resources are already native Sprite assets.
            var image=Visual_renderer.Scope["getArtImage"].Call(args[1]);if(!(image.Raw is SourceCanvasImage sprite))return false;
            var rotation=VisualJs.Arg(args,5);var opacity=VisualJs.Arg(args,6);
            return _nativeWorldSprites.Draw(_sourceCanvas,sprite,args[2].Number,args[3].Number,args[4].Number,rotation.Undefined?0:rotation.Number,opacity.Undefined?1:opacity.Number,part,part&&VisualJs.Arg(args,7).Truth,part&&VisualJs.Arg(args,8).Truth);
        }
        private void DisposeNativeWorld(){_nativeWorldField?.Dispose();_nativeWorldField=null;_nativeWorldAmbient?.Dispose();_nativeWorldAmbient=null;_nativeWorldRegion?.Dispose();_nativeWorldRegion=null;_nativeWorldPickups?.Dispose();_nativeWorldPickups=null;_nativeWorldMinions?.Dispose();_nativeWorldMinions=null;_nativeWorldEnergyBolts?.Dispose();_nativeWorldEnergyBolts=null;_nativeWorldSprites?.Dispose();_nativeWorldSprites=null;_nativeWorldProjectiles?.Dispose();_nativeWorldProjectiles=null;}
    }
}
