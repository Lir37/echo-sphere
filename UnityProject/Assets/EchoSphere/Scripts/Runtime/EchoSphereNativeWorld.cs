using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeWorldPresentation _nativeWorldPresentation;
        private NativeWorldPresentation WorldPresentation
        {
            get{
                if(_nativeWorldPresentation!=null)return _nativeWorldPresentation;
                var scene=NativeBattleSceneRoot.Find(gameObject.scene);
                return _nativeWorldPresentation=scene!=null?scene.Presentation:_worldSurface.GetComponent<NativeWorldPresentation>()??_worldSurface.gameObject.AddComponent<NativeWorldPresentation>();
            }
        }
        private NativeWorldCharacterIndicators _nativeWorldCharacterIndicators=>WorldPresentation.CharacterIndicators;
        private NativeWorldFeedback _nativeWorldFeedback=>WorldPresentation.Feedback;
        private NativeWorldNetwork _nativeWorldNetwork=>WorldPresentation.Network;
        private NativeWorldRegion _nativeWorldRegion=>WorldPresentation.Region;
        private NativeWorldSphereFamilies _nativeWorldSphereFamilies=>WorldPresentation.SphereFamilies;
        private NativeWorldField _nativeWorldField=>WorldPresentation.Field;
        private NativeWorldAmbient _nativeWorldAmbient=>WorldPresentation.Ambient;
        private NativeWorldPickups _nativeWorldPickups=>WorldPresentation.Pickups;
        private NativeWorldMinions _nativeWorldMinions=>WorldPresentation.Minions;
        private NativeWorldEnergyBolts _nativeWorldEnergyBolts=>WorldPresentation.EnergyBolts;
        private NativeWorldSprites _nativeWorldSprites=>WorldPresentation.Sprites;
        private NativeWorldProjectiles _nativeWorldProjectiles=>WorldPresentation.Projectiles;
        private NativeWorldCreatures _nativeWorldCreatures=>WorldPresentation.Creatures;
        private NativeWorldStandardSphere _nativeWorldStandardSphere=>WorldPresentation.StandardSphere;
        private NativeWorldPickableViews _nativeWorldPickableViews=>WorldPresentation.PickableViews;
        private NativeWorldPlayer _nativeWorldPlayer=>WorldPresentation.Player;
        private void RenderNativeWorldFrame(float width,float height)
        {
            WorldPresentation.BeginFrame(_worldSurface);
            RenderNativeFrameContents(width,height);
        }
        private bool DrawNativeStandardAssembly(NativeSphereVisual sphere,double radius,double time)
        {
            double x=sphere.X,y=sphere.Y,range=sphere.Radius*NativeRunCatalog.Instance.GetSphere(sphere.Family).RangeMultiplier;bool target=false;
            foreach(var enemy in _enemies){if(enemy==null||!enemy.IsAlive)continue;var point=NativeSourcePixels(enemy.transform.position);double dx=point.x-x,dy=point.y-y;if(enemy.CurrentHp>0&&System.Math.Sqrt(dx*dx+dy*dy)<range){target=true;break;}}
            return _nativeWorldStandardSphere.DrawAssembly(_worldSurface,x,y,radius,sphere.Tier,time,target,_nativeWorldSprites,ResolveNativePlayerArt);
        }
        private NativeWorldPlayer.State NativePlayerState()
        {
            string id=EchoSphere.Core.CharacterRules.ToSourceId(_sphereCharacterId);var point=NativeSourcePixels(PlayerPosition);ColorUtility.TryParseHtmlString(EchoSphere.Core.CharacterRules.Get(_sphereCharacterId).ColorHex,out var color);
            return new NativeWorldPlayer.State {X=point.x,Y=point.y,Radius=NativePlayerRadiusPixels,Time=_runTime,Hp=_playerCore.CurrentHp,ContactCooldown=_playerCore.ContactGraceTimer,ShieldTimer=_shieldTimer,ShieldCharges=_playerCore.ShieldCharges,DashTimer=_playerCore.DashTimer,DeathTimer=_deathTimer,Character=id,Color=color};
        }
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
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
            ColorUtility.TryParseHtmlString(enemy.SourceColorHtml,out var color);var combat=enemy.SourceProjectionBossCombat;var target=enemy.SourceProjectionLinkBreakerTarget;
            Vector2 direction=combat==null?Vector2.zero:combat.ChargeDirection*scale;
            var signal=new NativeWorldCreatures.BossSignal {Boss=boss,Charging=combat!=null&&combat.IsCharging,Type=type,Radius=enemy.SourceRadius,ChargeTimer=combat==null?double.NaN:combat.ChargeTimer,ChargeX=combat==null?double.NaN:direction.x/scale,ChargeY=combat==null?double.NaN:-direction.y/scale,ShootTimer=combat==null?double.NaN:combat.ShootTimer,SummonTimer=combat==null?double.NaN:combat.SummonTimer,AuraRadius=combat==null?double.NaN:combat.AuraRadius/scale,TargetX=playerX-x,TargetY=playerY-y,ShooterWindow=.7,SummonerWindow=.9,AuraWindow=.8};
            Vector2 targetPosition=target==null?Vector2.zero:(Vector2)target.transform.position;
            return new NativeWorldCreatures.EnemyView {Identity=enemy,X=x,Y=y,Radius=enemy.SourceRadius,HasRotation=true,Rotation=enemy.SourceProjectionRotation,HitFlash=enemy.SourceProjectionHitFlash,Tier=enemy.SourceVisualTier,Hp=enemy.CurrentHp,MaxHp=enemy.MaxHp,Boss=boss,Elite=enemy.IsElite,Fire=enemy.SourceProjectionFireTimer>0,Poison=enemy.SourceProjectionPoisonTimer>0,Freeze=enemy.SourceProjectionFreezeTimer>0,Variant=boss?type:enemy.SourceVisualVariant,BossName=name,Color=color,Signal=signal,LinkTarget=target!=null,LinkTimer=enemy.SourceProjectionLinkBreakerTelegraphTimer,LinkX=target==null?double.NaN:targetPosition.x/scale,LinkY=target==null?double.NaN:-targetPosition.y/scale,LinkWindow=.75};
        }
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
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
        private void DisposeNativeWorld(){ClearNativeEntityFrame();_nativeWorldPresentation?.ReleaseResources();_nativeWorldPresentation=null;}
    }
}
