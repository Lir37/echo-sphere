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
        private NativeWorldPlayer _nativeWorldPlayer;
        private void RenderNativeWorldFrame(float width,float height)
        {
            if(_nativeWorldFeedback==null)_nativeWorldFeedback=new NativeWorldFeedback();_nativeWorldFeedback.BeginFrame(_worldSurface);
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
            if(_nativeWorldPlayer==null)_nativeWorldPlayer=new NativeWorldPlayer();_nativeWorldPlayer.BeginFrame();
            if(_nativeWorldSphereFamilies==null)_nativeWorldSphereFamilies=new NativeWorldSphereFamilies();_nativeWorldSphereFamilies.BeginFrame();
            if(_nativeWorldCharacterIndicators==null)_nativeWorldCharacterIndicators=new NativeWorldCharacterIndicators();_nativeWorldCharacterIndicators.BeginFrame();
            if(_nativeWorldNetwork==null)_nativeWorldNetwork=new NativeWorldNetwork();_nativeWorldNetwork.BeginFrame();
            RenderNativeFrameContents(width,height);
        }
        private V DrawNativeStandardAssembly(V[] args)
        {
            var scene=args[1];var sphere=args[2];double x=sphere["pos"]["x"].Number,y=sphere["pos"]["y"].Number,range=sphere["radius"].Number*NativeRunCatalog.Instance.GetSphere(sphere["type"].Text).RangeMultiplier;bool target=false;
            foreach(var enemy in scene["enemies"].Items){double dx=enemy["pos"]["x"].Number-x,dy=enemy["pos"]["y"].Number-y;if(enemy["hp"].Number>0&&System.Math.Sqrt(dx*dx+dy*dy)<range){target=true;break;}}
            var tier=sphere["visualTier"];return _nativeWorldStandardSphere.DrawAssembly(_worldSurface,x,y,args[3].Number,tier.Truth?tier.Number:0,args[4].Number,target,_nativeWorldSprites,ResolveNativePlayerArt);
        }
        private NativeWorldPlayer.State NativePlayerState(V player)
        {
            var raw=player["characterId"];string id=raw.Truth&&EchoSphere.Core.CharacterRules.TryParseSourceId(raw.Text,out _)?raw.Text:"spherist";EchoSphere.Core.CharacterRules.TryParseSourceId(id,out var character);ColorUtility.TryParseHtmlString(EchoSphere.Core.CharacterRules.Get(character).ColorHex,out var color);
            return new NativeWorldPlayer.State {X=player["pos"]["x"].Number,Y=player["pos"]["y"].Number,Radius=NativePlayerRadiusPixels,Time=_sourceRenderState["time"].Number,Hp=player["hp"].Number,ContactCooldown=player["contactDamageCooldown"].Number,ShieldTimer=player["shieldTimer"].Number,ShieldCharges=player["shieldCharges"].Number,DashTimer=player["dashTimer"].Number,DeathTimer=player["deathTimer"].Number,Character=id,Color=color};
        }
        private static INativeWorldSprite ResolveNativePlayerArt(string key)=>NativeWorldArtLibrary.Resolve(key);
        private static NativeWorldCreatures.BossSignal NativeBossSignal(V e,V target)
        {
            var windows=Visual_bossBalance.Scope["BOSS_TELEGRAPH_WINDOWS"];
            return new NativeWorldCreatures.BossSignal {Boss=e["isBoss"].Truth,Charging=e["isCharging"].Truth,Type=e["bossType"].Text,Radius=e["radius"].Number,ChargeTimer=e["chargeTimer"].Number,ChargeX=e["chargeDir"]["x"].Number,ChargeY=e["chargeDir"]["y"].Number,ShootTimer=e["bossShootTimer"].Number,SummonTimer=e["summonTimer"].Number,AuraRadius=e["auraRadius"].Number,TargetX=target["x"].Number-e["pos"]["x"].Number,TargetY=target["y"].Number-e["pos"]["y"].Number,ShooterWindow=windows["shooter"].Number,SummonerWindow=windows["summoner"].Number,AuraWindow=windows["aura"].Number};
        }
        private void DrawNativeEnemyWorld(V e,V player)
        {
            ColorUtility.TryParseHtmlString(e["color"].Text,out var color);bool boss=e["isBoss"].Truth;string type=e["bossType"].Text;
            string name=type=="charger"?"BREACHER":type=="shooter"?"VOID LANCER":type=="summoner"?"BROOD MIND":"AURA TITAN";
            if(boss&&_worldSurface.LocalizeCanvasText)name=NativeLocalizationCatalog.Instance.CanvasText(name,IsRussianUi);
            var target=e["elitePulseTarget"];var timer=e["elitePulseTelegraphTimer"];
            _nativeWorldCreatures.DrawEnemy(_worldSurface,new NativeWorldCreatures.EnemyView {Identity=e.Raw,X=e["pos"]["x"].Number,Y=e["pos"]["y"].Number,Radius=e["radius"].Number,HasRotation=e["rotation"].Raw is double,Rotation=e["rotation"].Number,HitFlash=e["hitFlash"].Number,Tier=e["tier"].Number,Hp=e["hp"].Number,MaxHp=e["maxHp"].Number,Boss=boss,Elite=e["isElite"].Truth,Fire=e["fireTimer"].Number>0,Poison=e["poisonTimer"].Number>0,Freeze=e["freezeTimer"].Number>0,Variant=boss?type:e["visualVariant"].Truth?e["visualVariant"].Text:"wisp",BossName=name,Color=color,Signal=NativeBossSignal(e,player),LinkTarget=target.Truth,LinkTimer=timer.Truth?timer.Number:0,LinkX=target["pos"]["x"].Number,LinkY=target["pos"]["y"].Number,LinkWindow=Visual_eliteBalance.Scope["LINK_BREAKER_TELEGRAPH_SECONDS"].Number},_sourceRenderState["time"].Number);
        }
        private V DrawNativeSprite(V[] args,bool part)
        {
            // Retain the existing availability gate, including first-call false
            // and procedural fallback. Resources are already native Sprite assets.
            var sprite=ResolveNativePlayerArt(args[1].Text);if(sprite==null)return false;
            var rotation=VisualJs.Arg(args,5);var opacity=VisualJs.Arg(args,6);
            return _nativeWorldSprites.Draw(_worldSurface,sprite,args[2].Number,args[3].Number,args[4].Number,rotation.Undefined?0:rotation.Number,opacity.Undefined?1:opacity.Number,part,part&&VisualJs.Arg(args,7).Truth,part&&VisualJs.Arg(args,8).Truth);
        }
        private void DisposeNativeWorld(){_nativeWorldFeedback?.Dispose();_nativeWorldFeedback=null;_nativeWorldNetwork?.Dispose();_nativeWorldNetwork=null;_nativeWorldField?.Dispose();_nativeWorldField=null;_nativeWorldAmbient?.Dispose();_nativeWorldAmbient=null;_nativeWorldRegion?.Dispose();_nativeWorldRegion=null;_nativeWorldPickups?.Dispose();_nativeWorldPickups=null;_nativeWorldMinions?.Dispose();_nativeWorldMinions=null;_nativeWorldEnergyBolts?.Dispose();_nativeWorldEnergyBolts=null;_nativeWorldSprites?.Dispose();_nativeWorldSprites=null;_nativeWorldProjectiles?.Dispose();_nativeWorldProjectiles=null;_nativeWorldCreatures?.Dispose();_nativeWorldCreatures=null;_nativeWorldStandardSphere?.Dispose();_nativeWorldStandardSphere=null;_nativeWorldPickableViews?.Dispose();_nativeWorldPickableViews=null;_nativeWorldPlayer?.Dispose();_nativeWorldPlayer=null;_nativeWorldSphereFamilies?.Dispose();_nativeWorldSphereFamilies=null;_nativeWorldCharacterIndicators?.Dispose();_nativeWorldCharacterIndicators=null;}
    }
}
