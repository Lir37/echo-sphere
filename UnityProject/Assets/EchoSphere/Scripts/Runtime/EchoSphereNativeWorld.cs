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
        private NativeWorldPlayer.State NativePlayerState()
        {
            string id=EchoSphere.Core.CharacterRules.ToSourceId(_sphereCharacterId);var point=NativeSourcePixels(PlayerPosition);ColorUtility.TryParseHtmlString(EchoSphere.Core.CharacterRules.Get(_sphereCharacterId).ColorHex,out var color);
            // These two presentation timers were constant zero in the previous
            // snapshot path. Preserve that behavior; do not invent runtime wiring.
            return new NativeWorldPlayer.State {X=point.x,Y=point.y,Radius=NativePlayerRadiusPixels,Time=_runTime,Hp=_playerCore.CurrentHp,ContactCooldown=_playerCore.ContactGraceTimer,ShieldTimer=0,ShieldCharges=_playerCore.ShieldCharges,DashTimer=_playerCore.DashTimer,DeathTimer=0,Character=id,Color=color};
        }
#if UNITY_EDITOR
        private NativeWorldPlayer.State LegacyPlayerState(V player)
        {
            var raw=player["characterId"];string id=raw.Truth&&EchoSphere.Core.CharacterRules.TryParseSourceId(raw.Text,out _)?raw.Text:"spherist";EchoSphere.Core.CharacterRules.TryParseSourceId(id,out var character);ColorUtility.TryParseHtmlString(EchoSphere.Core.CharacterRules.Get(character).ColorHex,out var color);
            return new NativeWorldPlayer.State {X=player["pos"]["x"].Number,Y=player["pos"]["y"].Number,Radius=NativePlayerRadiusPixels,Time=_sourceRenderState["time"].Number,Hp=player["hp"].Number,ContactCooldown=player["contactDamageCooldown"].Number,ShieldTimer=player["shieldTimer"].Number,ShieldCharges=player["shieldCharges"].Number,DashTimer=player["dashTimer"].Number,DeathTimer=player["deathTimer"].Number,Character=id,Color=color};
        }
#endif
        private static INativeWorldSprite ResolveNativePlayerArt(string key)=>NativeWorldArtLibrary.Resolve(key);
        private void DrawNativeEnemyWorld(EnemyAgent2D enemy)
        {
            _nativeWorldCreatures.DrawEnemy(_worldSurface,ProjectNativeEnemyView(enemy,PlayerPosition,_worldSurface.LocalizeCanvasText,IsRussianUi),_runTime);
        }
        internal static NativeWorldCreatures.EnemyView ProjectNativeEnemyView(EnemyAgent2D enemy,Vector2 playerPosition,bool localize,bool ru)
        {
            float scale=EchoSphere.Core.SpherePlacementRules.SourcePixelsToWorld;Vector2 position=enemy.transform.position;
            double x=position.x/scale,y=-position.y/scale,playerX=playerPosition.x/scale,playerY=-playerPosition.y/scale;
            bool boss=enemy.IsBoss;string type=enemy.BossType;string name=type=="charger"?"BREACHER":type=="shooter"?"VOID LANCER":type=="summoner"?"BROOD MIND":"AURA TITAN";
            if(boss&&localize)name=NativeLocalizationCatalog.Instance.CanvasText(name,ru);
            ColorUtility.TryParseHtmlString(enemy.SourceColorHtml,out var color);var combat=enemy.SourceProjectionBossCombat;var target=enemy.SourceProjectionLinkBreakerTarget;var visual=enemy.SourceProjectionCreatureVisual;
            Vector2 direction=combat==null?Vector2.zero:combat.ChargeDirection*scale;
            var signal=new NativeWorldCreatures.BossSignal {Boss=boss,Charging=combat!=null&&combat.IsCharging,Type=type,Radius=enemy.SourceRadius,ChargeTimer=combat==null?double.NaN:combat.ChargeTimer,ChargeX=combat==null?double.NaN:direction.x/scale,ChargeY=combat==null?double.NaN:-direction.y/scale,ShootTimer=combat==null?double.NaN:combat.ShootTimer,SummonTimer=combat==null?double.NaN:combat.SummonTimer,AuraRadius=combat==null?double.NaN:combat.AuraRadius/scale,TargetX=playerX-x,TargetY=playerY-y,ShooterWindow=.7,SummonerWindow=.9,AuraWindow=.8};
            Vector2 targetPosition=target==null?Vector2.zero:(Vector2)target.transform.position;
            return new NativeWorldCreatures.EnemyView {Identity=enemy,X=x,Y=y,Radius=enemy.SourceRadius,HasRotation=true,Rotation=enemy.SourceProjectionRotation,HitFlash=enemy.SourceProjectionHitFlash,Tier=enemy.SourceVisualTier,Hp=enemy.CurrentHp,MaxHp=enemy.MaxHp,Boss=boss,Elite=enemy.IsElite,Fire=enemy.SourceProjectionFireTimer>0,Poison=enemy.SourceProjectionPoisonTimer>0,Freeze=enemy.SourceProjectionFreezeTimer>0,Variant=boss?type:visual!=null&&!string.IsNullOrEmpty(visual.SourceVariant)?visual.SourceVariant:"wisp",BossName=name,Color=color,Signal=signal,LinkTarget=target!=null,LinkTimer=enemy.SourceProjectionLinkBreakerTelegraphTimer,LinkX=target==null?double.NaN:targetPosition.x/scale,LinkY=target==null?double.NaN:-targetPosition.y/scale,LinkWindow=.75};
        }
#if UNITY_EDITOR
        private static NativeWorldCreatures.BossSignal NativeBossSignal(V e,V target)
        {
            var windows=Visual_bossBalance.Scope["BOSS_TELEGRAPH_WINDOWS"];
            return new NativeWorldCreatures.BossSignal {Boss=e["isBoss"].Truth,Charging=e["isCharging"].Truth,Type=e["bossType"].Text,Radius=e["radius"].Number,ChargeTimer=e["chargeTimer"].Number,ChargeX=e["chargeDir"]["x"].Number,ChargeY=e["chargeDir"]["y"].Number,ShootTimer=e["bossShootTimer"].Number,SummonTimer=e["summonTimer"].Number,AuraRadius=e["auraRadius"].Number,TargetX=target["x"].Number-e["pos"]["x"].Number,TargetY=target["y"].Number-e["pos"]["y"].Number,ShooterWindow=windows["shooter"].Number,SummonerWindow=windows["summoner"].Number,AuraWindow=windows["aura"].Number};
        }
        internal static NativeWorldCreatures.EnemyView ProjectLegacyEnemyView(V e,V player,bool localize,bool ru)
        {
            ColorUtility.TryParseHtmlString(e["color"].Text,out var color);bool boss=e["isBoss"].Truth;string type=e["bossType"].Text;
            string name=type=="charger"?"BREACHER":type=="shooter"?"VOID LANCER":type=="summoner"?"BROOD MIND":"AURA TITAN";
            if(boss&&localize)name=NativeLocalizationCatalog.Instance.CanvasText(name,ru);
            var target=e["elitePulseTarget"];var timer=e["elitePulseTelegraphTimer"];
            return new NativeWorldCreatures.EnemyView {Identity=e.Raw,X=e["pos"]["x"].Number,Y=e["pos"]["y"].Number,Radius=e["radius"].Number,HasRotation=e["rotation"].Raw is double,Rotation=e["rotation"].Number,HitFlash=e["hitFlash"].Number,Tier=e["tier"].Number,Hp=e["hp"].Number,MaxHp=e["maxHp"].Number,Boss=boss,Elite=e["isElite"].Truth,Fire=e["fireTimer"].Number>0,Poison=e["poisonTimer"].Number>0,Freeze=e["freezeTimer"].Number>0,Variant=boss?type:e["visualVariant"].Truth?e["visualVariant"].Text:"wisp",BossName=name,Color=color,Signal=NativeBossSignal(e,player),LinkTarget=target.Truth,LinkTimer=timer.Truth?timer.Number:0,LinkX=target["pos"]["x"].Number,LinkY=target["pos"]["y"].Number,LinkWindow=Visual_eliteBalance.Scope["LINK_BREAKER_TELEGRAPH_SECONDS"].Number};
        }
#endif
        private void DisposeNativeWorld(){ClearNativeEntityFrame();_nativeWorldFeedback?.Dispose();_nativeWorldFeedback=null;_nativeWorldNetwork?.Dispose();_nativeWorldNetwork=null;_nativeWorldField?.Dispose();_nativeWorldField=null;_nativeWorldAmbient?.Dispose();_nativeWorldAmbient=null;_nativeWorldRegion?.Dispose();_nativeWorldRegion=null;_nativeWorldPickups?.Dispose();_nativeWorldPickups=null;_nativeWorldMinions?.Dispose();_nativeWorldMinions=null;_nativeWorldEnergyBolts?.Dispose();_nativeWorldEnergyBolts=null;_nativeWorldSprites?.Dispose();_nativeWorldSprites=null;_nativeWorldProjectiles?.Dispose();_nativeWorldProjectiles=null;_nativeWorldCreatures?.Dispose();_nativeWorldCreatures=null;_nativeWorldStandardSphere?.Dispose();_nativeWorldStandardSphere=null;_nativeWorldPickableViews?.Dispose();_nativeWorldPickableViews=null;_nativeWorldPlayer?.Dispose();_nativeWorldPlayer=null;_nativeWorldSphereFamilies?.Dispose();_nativeWorldSphereFamilies=null;_nativeWorldCharacterIndicators?.Dispose();_nativeWorldCharacterIndicators=null;}
    }
}
