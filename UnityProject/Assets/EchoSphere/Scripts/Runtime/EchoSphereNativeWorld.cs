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
        private NativeWorldCreatures _nativeWorldCreatures;
        private NativeWorldStandardSphere _nativeWorldStandardSphere;
        private NativeWorldPickableViews _nativeWorldPickableViews;
        private void RenderNativeWorldFrame(float width,float height)
        {
            if(_nativeWorldField==null)_nativeWorldField=new NativeWorldField();
            if(_nativeWorldAmbient==null)_nativeWorldAmbient=new NativeWorldAmbient();
            if(_nativeWorldPickups==null)_nativeWorldPickups=new NativeWorldPickups();_nativeWorldPickups.BeginFrame();
            if(_nativeWorldMinions==null)_nativeWorldMinions=new NativeWorldMinions();_nativeWorldMinions.BeginFrame();
            if(_nativeWorldEnergyBolts==null)_nativeWorldEnergyBolts=new NativeWorldEnergyBolts();_nativeWorldEnergyBolts.BeginFrame();
            if(_nativeWorldSprites==null)_nativeWorldSprites=new NativeWorldSprites();_nativeWorldSprites.BeginFrame();
            if(_nativeWorldProjectiles==null)_nativeWorldProjectiles=new NativeWorldProjectiles();_nativeWorldProjectiles.BeginFrame();
            if(_nativeWorldCreatures==null)_nativeWorldCreatures=new NativeWorldCreatures();_nativeWorldCreatures.BeginFrame();
            if(_nativeWorldStandardSphere==null)_nativeWorldStandardSphere=new NativeWorldStandardSphere();_nativeWorldStandardSphere.BeginFrame();
            if(_nativeWorldPickableViews==null)_nativeWorldPickableViews=new NativeWorldPickableViews();_nativeWorldPickableViews.BeginFrame();
            var sourceRune=Visual_renderer.Scope["drawRune"];var sourceChest=Visual_renderer.Scope["drawChest"];var sourceStella=Visual_renderer.Scope["drawStellaChest"];
            var sourceCore=Visual_renderer.Scope["drawStandardSphereCore"];var sourceRing=Visual_renderer.Scope["drawStandardSphereRing"];var sourceAttack=Visual_renderer.Scope["drawStandardAttackVfx"];
            var sourceEnemy=Visual_renderer.Scope["drawModernEnemy"];var sourceDisabled=Visual_renderer.Scope["drawNetworkDisabledIndicator"];
            var sourceElite=Visual_renderer.Scope["drawEliteCrest"];var sourceStatus=Visual_renderer.Scope["drawEnemyStatusVfx"];var sourceBossSignal=Visual_renderer.Scope["drawBossAttackTelegraph"];
            var sourceOuterGlow=Visual_renderer.Scope["glowCircle"];var sourceGround=Visual_renderer.Scope["drawGroundShadow"];
            var sourceCreature=Visual_enemyVisual.Scope["drawEnemyCreature"];var sourceBossCreature=Visual_enemyVisual.Scope["drawBossCreature"];
            var sourceProjectile=Visual_renderer.Scope["drawEnemyProjectile"];var sourceBossProjectile=Visual_renderer.Scope["drawBossProjectile"];
            var sourceField=Visual_renderer.Scope["drawVoidField"];
            var sourceAmbient=Visual_renderer.Scope["drawVoidAmbient"];
            var sourceRegion=Visual_renderer.Scope["drawRegionLayer"];
            var sourceXp=Visual_renderer.Scope["drawModernXp"];var sourceHealth=Visual_renderer.Scope["drawModernHealth"];
            var sourceArt=Visual_renderer.Scope["drawArtSprite"];var sourcePart=Visual_renderer.Scope["drawStandardSpherePart"];
            var sourceMinion=Visual_renderer.Scope["drawModernMinion"];var sourceEnergy=Visual_renderer.Scope["drawEnergyBolt"];
            try{
                Visual_renderer.Scope["drawRune"]=VisualJs.Fn(args=>{var rune=args[1];ColorUtility.TryParseHtmlString(Visual_runes.Scope["RUNE_DEFS"][rune["type"]]["color"].Text,out var color);_nativeWorldPickableViews.DrawRune(_sourceCanvas,rune["pos"]["x"].Number,rune["pos"]["y"].Number,rune["type"].Text,color,_sourceRenderState["time"].Number);return default;});
                Visual_renderer.Scope["drawChest"]=VisualJs.Fn(args=>{var chest=args[1];_nativeWorldPickableViews.DrawChest(_sourceCanvas,chest["pos"]["x"].Number,chest["pos"]["y"].Number,_sourceRenderState["time"].Number,false,null);return default;});
                Visual_renderer.Scope["drawStellaChest"]=VisualJs.Fn(args=>{var chest=args[1];string label="STELLA";if(_sourceCanvas.LocalizeCanvasText)label=Visual_canvasLocale.Scope["localizeCanvasText"].Call(label).Text;_nativeWorldPickableViews.DrawChest(_sourceCanvas,chest["pos"]["x"].Number,chest["pos"]["y"].Number,_sourceRenderState["time"].Number,true,label);return default;});
                Visual_renderer.Scope["drawStandardSphereCore"]=VisualJs.Fn(args=>{_nativeWorldStandardSphere.DrawCore(_sourceCanvas,args[1].Number,args[2].Number);return default;});
                Visual_renderer.Scope["drawStandardSphereRing"]=VisualJs.Fn(args=>{_nativeWorldStandardSphere.DrawRing(_sourceCanvas,args[1].Number,args[2].Number,args[3].Number);return default;});
                Visual_renderer.Scope["drawStandardAttackVfx"]=VisualJs.Fn(args=>{var sphere=args[1];_nativeWorldStandardSphere.DrawAttack(_sourceCanvas,sphere["pos"]["x"].Number,sphere["pos"]["y"].Number,sphere["rotation"].Truth?sphere["rotation"].Number:0,args[2].Number,sphere["attackTimer"].Number);return default;});
                Visual_renderer.Scope["drawModernEnemy"]=VisualJs.Fn(args=>{DrawNativeEnemyWorld(args[1],args[2]);return default;});
                Visual_renderer.Scope["drawNetworkDisabledIndicator"]=VisualJs.Fn(args=>{var timer=args[1]["networkDisabledTimer"];_nativeWorldCreatures.DrawDisabledNetwork(_sourceCanvas,timer.Truth?timer.Number:0,Visual_eliteBalance.Scope["LINK_BREAKER_DISABLED_SECONDS"].Number,args[2].Number);return default;});
                Visual_renderer.Scope["glowCircle"]=VisualJs.Fn(args=>{ColorUtility.TryParseHtmlString(args[2].Text,out var color);var alpha=VisualJs.Arg(args,3);_nativeWorldCreatures.DrawOuterGlow(_sourceCanvas,args[1].Number,color,alpha.Undefined?.26:alpha.Number);return default;});
                Visual_renderer.Scope["drawGroundShadow"]=VisualJs.Fn(args=>{_nativeWorldCreatures.DrawGroundShadow(_sourceCanvas,args[1].Number,args[2].Number);return default;});
                Visual_renderer.Scope["drawEliteCrest"]=VisualJs.Fn(args=>{var enemy=args[1];ColorUtility.TryParseHtmlString(enemy["color"].Text,out var color);_nativeWorldCreatures.DrawElite(_sourceCanvas,enemy["radius"].Number,color,args[2].Number);return default;});
                Visual_renderer.Scope["drawEnemyStatusVfx"]=VisualJs.Fn(args=>{var enemy=args[1];_nativeWorldCreatures.DrawStatus(_sourceCanvas,enemy["radius"].Number,args[2].Number,enemy["fireTimer"].Number>0,enemy["poisonTimer"].Number>0,enemy["freezeTimer"].Number>0);return default;});
                Visual_renderer.Scope["drawBossAttackTelegraph"]=VisualJs.Fn(args=>{_nativeWorldCreatures.DrawBossSignal(_sourceCanvas,NativeBossSignal(args[1],args[2]),args[3].Number);return default;});
                Visual_enemyVisual.Scope["drawEnemyCreature"]=VisualJs.Fn(args=>{var enemy=args[1];ColorUtility.TryParseHtmlString(enemy["color"].Text,out var color);string variant=enemy["visualVariant"].Truth?enemy["visualVariant"].Text:"wisp";_nativeWorldCreatures.Draw(_sourceCanvas,enemy["radius"].Number,color,args[2].Number,variant,false);return default;});
                Visual_enemyVisual.Scope["drawBossCreature"]=VisualJs.Fn(args=>{var enemy=args[1];ColorUtility.TryParseHtmlString(enemy["color"].Text,out var color);_nativeWorldCreatures.Draw(_sourceCanvas,enemy["radius"].Number,color,args[2].Number,enemy["bossType"].Text,true);return default;});
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
            }finally{Visual_renderer.Scope["drawRune"]=sourceRune;Visual_renderer.Scope["drawChest"]=sourceChest;Visual_renderer.Scope["drawStellaChest"]=sourceStella;Visual_renderer.Scope["drawStandardSphereCore"]=sourceCore;Visual_renderer.Scope["drawStandardSphereRing"]=sourceRing;Visual_renderer.Scope["drawStandardAttackVfx"]=sourceAttack;Visual_renderer.Scope["drawModernEnemy"]=sourceEnemy;Visual_renderer.Scope["drawNetworkDisabledIndicator"]=sourceDisabled;Visual_renderer.Scope["drawEliteCrest"]=sourceElite;Visual_renderer.Scope["drawEnemyStatusVfx"]=sourceStatus;Visual_renderer.Scope["drawBossAttackTelegraph"]=sourceBossSignal;Visual_renderer.Scope["glowCircle"]=sourceOuterGlow;Visual_renderer.Scope["drawGroundShadow"]=sourceGround;Visual_enemyVisual.Scope["drawEnemyCreature"]=sourceCreature;Visual_enemyVisual.Scope["drawBossCreature"]=sourceBossCreature;Visual_renderer.Scope["drawEnemyProjectile"]=sourceProjectile;Visual_renderer.Scope["drawBossProjectile"]=sourceBossProjectile;Visual_renderer.Scope["drawVoidField"]=sourceField;Visual_renderer.Scope["drawVoidAmbient"]=sourceAmbient;Visual_renderer.Scope["drawRegionLayer"]=sourceRegion;Visual_renderer.Scope["drawModernXp"]=sourceXp;Visual_renderer.Scope["drawModernHealth"]=sourceHealth;Visual_renderer.Scope["drawModernMinion"]=sourceMinion;Visual_renderer.Scope["drawEnergyBolt"]=sourceEnergy;Visual_renderer.Scope["drawArtSprite"]=sourceArt;Visual_renderer.Scope["drawStandardSpherePart"]=sourcePart;}
        }
        private static NativeWorldCreatures.BossSignal NativeBossSignal(V e,V target)
        {
            var windows=Visual_bossBalance.Scope["BOSS_TELEGRAPH_WINDOWS"];
            return new NativeWorldCreatures.BossSignal {Boss=e["isBoss"].Truth,Charging=e["isCharging"].Truth,Type=e["bossType"].Text,Radius=e["radius"].Number,ChargeTimer=e["chargeTimer"].Number,ChargeX=e["chargeDir"]["x"].Number,ChargeY=e["chargeDir"]["y"].Number,ShootTimer=e["bossShootTimer"].Number,SummonTimer=e["summonTimer"].Number,AuraRadius=e["auraRadius"].Number,TargetX=target["x"].Number-e["pos"]["x"].Number,TargetY=target["y"].Number-e["pos"]["y"].Number,ShooterWindow=windows["shooter"].Number,SummonerWindow=windows["summoner"].Number,AuraWindow=windows["aura"].Number};
        }
        private void DrawNativeEnemyWorld(V e,V player)
        {
            ColorUtility.TryParseHtmlString(e["color"].Text,out var color);bool boss=e["isBoss"].Truth;string type=e["bossType"].Text;
            string name=type=="charger"?"BREACHER":type=="shooter"?"VOID LANCER":type=="summoner"?"BROOD MIND":"AURA TITAN";
            if(boss&&_sourceCanvas.LocalizeCanvasText)name=Visual_canvasLocale.Scope["localizeCanvasText"].Call(name).Text;
            var target=e["elitePulseTarget"];var timer=e["elitePulseTelegraphTimer"];
            _nativeWorldCreatures.DrawEnemy(_sourceCanvas,new NativeWorldCreatures.EnemyView {Identity=e.Raw,X=e["pos"]["x"].Number,Y=e["pos"]["y"].Number,Radius=e["radius"].Number,HasRotation=e["rotation"].Raw is double,Rotation=e["rotation"].Number,HitFlash=e["hitFlash"].Number,Tier=e["tier"].Number,Hp=e["hp"].Number,MaxHp=e["maxHp"].Number,Boss=boss,Elite=e["isElite"].Truth,Fire=e["fireTimer"].Number>0,Poison=e["poisonTimer"].Number>0,Freeze=e["freezeTimer"].Number>0,Variant=boss?type:e["visualVariant"].Truth?e["visualVariant"].Text:"wisp",BossName=name,Color=color,Signal=NativeBossSignal(e,player),LinkTarget=target.Truth,LinkTimer=timer.Truth?timer.Number:0,LinkX=target["pos"]["x"].Number,LinkY=target["pos"]["y"].Number,LinkWindow=Visual_eliteBalance.Scope["LINK_BREAKER_TELEGRAPH_SECONDS"].Number},_sourceRenderState["time"].Number);
        }
        private V DrawNativeSprite(V[] args,bool part)
        {
            // Retain the existing availability gate, including first-call false
            // and procedural fallback. Resources are already native Sprite assets.
            var image=Visual_renderer.Scope["getArtImage"].Call(args[1]);if(!(image.Raw is SourceCanvasImage sprite))return false;
            var rotation=VisualJs.Arg(args,5);var opacity=VisualJs.Arg(args,6);
            return _nativeWorldSprites.Draw(_sourceCanvas,sprite,args[2].Number,args[3].Number,args[4].Number,rotation.Undefined?0:rotation.Number,opacity.Undefined?1:opacity.Number,part,part&&VisualJs.Arg(args,7).Truth,part&&VisualJs.Arg(args,8).Truth);
        }
        private void DisposeNativeWorld(){_nativeWorldField?.Dispose();_nativeWorldField=null;_nativeWorldAmbient?.Dispose();_nativeWorldAmbient=null;_nativeWorldRegion?.Dispose();_nativeWorldRegion=null;_nativeWorldPickups?.Dispose();_nativeWorldPickups=null;_nativeWorldMinions?.Dispose();_nativeWorldMinions=null;_nativeWorldEnergyBolts?.Dispose();_nativeWorldEnergyBolts=null;_nativeWorldSprites?.Dispose();_nativeWorldSprites=null;_nativeWorldProjectiles?.Dispose();_nativeWorldProjectiles=null;_nativeWorldCreatures?.Dispose();_nativeWorldCreatures=null;_nativeWorldStandardSphere?.Dispose();_nativeWorldStandardSphere=null;_nativeWorldPickableViews?.Dispose();_nativeWorldPickableViews=null;}
    }
}
