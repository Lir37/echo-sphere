using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private SourceCanvasSurface _sourceCanvas;
        private V _sourceRenderState;
        [Serializable] private sealed class SourceLightningVisual {public Vector2 from,to;public float life;public string sphereType;}
        private readonly List<SourceLightningVisual> _sourceLightningVisuals=new List<SourceLightningVisual>();
        private void AddSourceLightningVisual(Vector2 from,Vector2 to,float life,SphereId? type=null)
        {_sourceLightningVisuals.Add(new SourceLightningVisual {from=from,to=to,life=life,sphereType=type.HasValue?type.Value.ToString().ToLowerInvariant():null});}
        private void TickSourceLightningVisuals(float dt)
        {for(var i=_sourceLightningVisuals.Count-1;i>=0;i--){_sourceLightningVisuals[i].life-=dt;if(_sourceLightningVisuals[i].life<=0f)_sourceLightningVisuals.RemoveAt(i);}if(_sourceLightningVisuals.Count>36)_sourceLightningVisuals.RemoveRange(0,_sourceLightningVisuals.Count-36);}
        private readonly Dictionary<MonoBehaviour,V> _sourceRenderEntities=new Dictionary<MonoBehaviour,V>();
        private static readonly Dictionary<Type,Dictionary<string,FieldInfo>> SourceRenderFields=new Dictionary<Type,Dictionary<string,FieldInfo>>();
        private static object RenderField(object owner,string key)
        {
            if(owner==null)return null;var type=owner.GetType();
            if(!SourceRenderFields.TryGetValue(type,out var fields)) {
                fields=new Dictionary<string,FieldInfo>();
                foreach(var field in type.GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public))fields[field.Name]=field;
                SourceRenderFields.Add(type,fields);
            }
            if(!fields.TryGetValue(key,out var result))throw new InvalidOperationException("Missing source presentation state: "+type.Name+"."+key);
            return result.GetValue(owner);
        }
        private static V SourcePoint(Vector2 world)=>VisualJs.Obj("x",world.x/SpherePlacementRules.SourcePixelsToWorld,"y",-world.y/SpherePlacementRules.SourcePixelsToWorld);
        private static V SourcePoint(Vec2 world)=>VisualJs.Obj("x",world.X/SpherePlacementRules.SourcePixelsToWorld,"y",-world.Y/SpherePlacementRules.SourcePixelsToWorld);
        private V RenderValue(object value)
        {
            if(value==null)return VisualJs.Null;
            if(value is V v)return v;
            if(value is string text)return text;if(value is bool boolean)return boolean;
            if(value is Vector2 point)return SourcePoint(point);if(value is Vector3 point3)return SourcePoint((Vector2)point3);if(value is Vec2 vector)return SourcePoint(vector);
            if(value is Color color)return "#"+ColorUtility.ToHtmlStringRGB(color);
            if(value is MonoBehaviour entity)return RenderEntity(entity);
            if(value is Enum enumeration)return enumeration.ToString().ToLowerInvariant();
            if(value is IDictionary dictionary) {var result=VisualJs.Obj();foreach(DictionaryEntry entry in dictionary)result[RenderValue(entry.Key)]=RenderValue(entry.Value);return result;}
            if(value is IEnumerable list) {var result=new List<V>();foreach(var item in list)result.Add(RenderValue(item));return VisualJs.Arr(result.ToArray());}
            if(value is IConvertible)return new V(Convert.ToDouble(value,System.Globalization.CultureInfo.InvariantCulture));
            var members=VisualJs.Obj();foreach(var field in value.GetType().GetFields(BindingFlags.Instance|BindingFlags.Public))members[char.ToLowerInvariant(field.Name[0])+field.Name.Substring(1)]=RenderValue(field.GetValue(value));return members;
        }
        private V RenderEntity(MonoBehaviour entity)
        {
            if(entity==null)return VisualJs.Null;
            if(!_sourceRenderEntities.TryGetValue(entity,out var view)){view=VisualJs.Obj();_sourceRenderEntities.Add(entity,view);}
            view["pos"]=SourcePoint((Vector2)entity.transform.position);view["alive"]=entity.gameObject.activeInHierarchy;
            void Field(string source,string field)=>VisualJs.Assign(view,source,"=",RenderValue(RenderField(entity,field)));
            if(entity is SphereAttackAgent sphere) {
                view["type"]=sphere.Type.ToString().ToLowerInvariant();view["radius"]=130;view["visualTier"]=sphere.ProgressionLevel;
                view["damage"]=12;view["attackDelay"]=1.2;view["attackTimer"]=sphere.AttackTimer;view["auraTimer"]=sphere.Type==SphereId.Orbital?RenderValue(RenderField(sphere,"_orbitalContactCheckTimer")):new V(sphere.AttackTimer);
                view["rotation"]=sphere.Type==SphereId.Orbital?RenderValue(RenderField(sphere,"_orbitAngle")):new V(sphere.Type==SphereId.Gravity?sphere.VisualRotation:-sphere.VisualRotation);
                Field("networkDisabledTimer","_networkDisabledTimer");view["formationHitCount"]=sphere.FormationHitCount;view["resonancePulseTimer"]=sphere.ResonancePulseTimer;
            } else if(entity is EnemyAgent2D enemy) {
                view["hp"]=enemy.CurrentHp;view["maxHp"]=enemy.MaxHp;view["radius"]=enemy.SourceRadius;view["color"]=enemy.SourceColorHtml;
                view["type"]=enemy.SourceMonsterType;view["isBoss"]=enemy.IsBoss;view["isElite"]=enemy.IsElite;view["bossType"]=enemy.BossType;view["role"]=enemy.Role;view["eliteVariant"]=enemy.EliteVariant;
                var visual=(EnemyCreatureVisual2D)RenderField(enemy,"_creatureVisual");view["visualVariant"]=visual==null?VisualJs.Null:new V(visual.SourceVariant);
                Field("hitFlash","_sourceHitFlash");Field("tier","_sourceVisualTier");Field("freezeTimer","_freezeTimer");Field("fireTimer","_fireTimer");Field("poisonTimer","_poisonTimer");
                Field("rotation","_sourceVisualRotation");
                Field("elitePulseTelegraphTimer","_linkBreakerTelegraphTimer");Field("elitePulseTarget","_linkBreakerTarget");
                view["bossProjectiles"]=VisualJs.Arr();
                var combat=(BossCombatAgent2D)RenderField(enemy,"_bossCombat");
                if(combat!=null){view["bossShootTimer"]=combat.ShootTimer;view["summonTimer"]=combat.SummonTimer;view["chargeTimer"]=combat.ChargeTimer;view["isCharging"]=combat.IsCharging;view["chargeDir"]=SourcePoint(combat.ChargeDirection*SpherePlacementRules.SourcePixelsToWorld);view["auraRadius"]=combat.AuraRadius/SpherePlacementRules.SourcePixelsToWorld;}
            } else if(entity is ProjectileAgent) {
                Field("life","_life");Field("radius","_sourceRadius");Field("color","_impactColor");Field("damage","_damage");Field("pierce","_remainingPierces");Field("ricochet","_remainingRicochets");Field("sourceSphere","_evolutionOwner");
                view["vel"]=SourcePoint((Vector2)RenderField(entity,"_direction")*(float)RenderField(entity,"_speed"));
                var element=(SphereElement)RenderField(entity,"_sourceElement");view["effect"]=element==SphereElement.None?VisualJs.Null:new V(element.ToString().ToLowerInvariant());
            } else if(entity is BossProjectileAgent2D bullet) {
                Field("vel","_velocity");view["radius"]=((float)RenderField(bullet,"_hitRadius")-BossCombatRules.PlayerRadiusWorld)/SpherePlacementRules.SourcePixelsToWorld;
                var kind=(string)RenderField(bullet,"_bossType");view["visualType"]=kind;
            } else if(entity is AbilityDroneAgent2D) {
                Field("life","_remaining");Field("rotation","_rotation");Field("anchorType","_anchorType");view["radius"]=12;
            } else if(entity is ExperienceOrbAgent) {view["radius"]=6;Field("rotation","_rotation");}
            else if(entity is RuneAgent2D){Field("type","_type");Field("life","_life");}
            else if(entity is SourceParticle2D) {Field("life","_life");Field("maxLife","_maxLife");Field("vel","_velocity");Field("color","_color");Field("size","_sourceSize");}
            return view;
        }
        private static V RenderShape(SphereNetworkShape shape)=>shape==null?VisualJs.Null:VisualJs.Obj("type",shape.Type.ToString().ToLowerInvariant(),"strength",shape.Strength,"dominanceScore",shape.DominanceScore,"nodes",VisualJs.Arr(shape.NodeIndexes.ConvertAll(x=>new V((double)x)).ToArray()),"active",shape.Active);
        private V RenderNetwork()
        {
            var network=_networkState??SphereNetworkRules.Analyze(new List<SphereNetworkNode>());
            var result=VisualJs.Obj("linkDistance",network.LinkDistance/SpherePlacementRules.SourcePixelsToWorld,"nodes",RenderValue(network.Nodes),"formationCandidates",VisualJs.Arr(network.FormationCandidates.ConvertAll(RenderShape).ToArray()),"dominantFormation",RenderShape(network.DominantFormation),"secondaryFormation",RenderShape(network.SecondaryFormation),"formationEfficiency",network.FormationEfficiency);
            result["links"]=VisualJs.Arr(network.Links.ConvertAll(x=>VisualJs.Obj("a",x.A,"b",x.B,"distance",x.Distance/SpherePlacementRules.SourcePixelsToWorld)).ToArray());
            foreach(SphereNetworkFormation type in Enum.GetValues(typeof(SphereNetworkFormation)))if(type!=SphereNetworkFormation.None)result[type.ToString().ToLowerInvariant()]=RenderShape(network.GetFormation(type));
            return result;
        }
        private void UpdateSourceRenderState()
        {
            if(_sourceRenderState.Undefined)_sourceRenderState=VisualInitialState.Create();var scene=_sourceRenderState;var player=scene["player"];
            // Directly corresponding timers and counters keep their source names.
            foreach(var pair in new List<KeyValuePair<string,V>>((Dictionary<string,V>)player.Raw)) {
                var field=typeof(EchoSphereRuntime).GetField("_"+pair.Key,BindingFlags.Instance|BindingFlags.NonPublic);
                if(field!=null)player[pair.Key]=RenderValue(field.GetValue(this));
            }
            player["pos"]=SourcePoint(PlayerPosition);player["hp"]=_playerCore.CurrentHp;player["maxHp"]=_playerCore.MaxHp;
            player["characterId"]=CharacterRules.ToSourceId(_sphereCharacterId);player["characterMasteryLevel"]=_characterMasteryLevel;
            player["contactDamageCooldown"]=_playerCore.ContactGraceTimer;player["invulnerableTimer"]=_playerCore.InvulnerableTimer;player["shieldCharges"]=_playerCore.ShieldCharges;
            player["dashCooldown"]=_playerCore.DashCooldown;player["dashTimer"]=_playerCore.DashTimer;player["dashDir"]=SourcePoint(_playerCore.DashDirection*SpherePlacementRules.SourcePixelsToWorld);player["dashCount"]=_playerCore.DashCount;
            player["xpToNext"]=_xpRequired;player["comboMult"]=_comboMultiplier;player["eliteKills"]=_runEliteKills;player["chestOpens"]=_runChestOpens;
            player["timestopTimer"]=_timeStopTimer;player["timestopCooldown"]=_timeStopCooldown;player["darkritualCooldown"]=_darkRitualCooldown;player["overloadTimer"]=_darkRitualOverloadTimer;
            player["artifacts"]=RenderValue(_ownedArtifacts);player["sphereProgression"]=RenderValue(_sphereLevels);player["sphereBranches"]=RenderValue(_sphereBranches);player["sphereMods"]=RenderValue(_sphereMods);
            player["formationFollowActive"]=_follow;player["formationFollowStrain"]=_formationStrain;player["voidPhantomPos"]=_voidPhantomTimer>0f?SourcePoint(_voidPhantomPosition):VisualJs.Null;
            player["formationFollowLastDirection"]=SourcePoint(_lastDirection*SpherePlacementRules.SourcePixelsToWorld);
            player["resonanceGeometryNodes"]=RenderValue(_resonanceGeometryNodeIndexes);player["resonanceLastActiveFormationKey"]=_resonanceGeometryKey;player["resonanceFormationCandidateKeys"]=RenderValue(_resonanceGeometryCandidateKeys);
            var abilities=VisualJs.Obj();foreach(var pair in _abilityLevels)abilities[SourceAbilityId(pair.Key)]=pair.Value;player["abilities"]=abilities;
            var abilityBranches=VisualJs.Obj();foreach(var pair in _abilityBranches)abilityBranches[SourceAbilityId(pair.Key)]=pair.Value;player["abilityBranches"]=abilityBranches;
            var evolutions=new List<V>();foreach(var pair in _sphereBranches)if(!string.IsNullOrEmpty(pair.Value))evolutions.Add("sphere:"+pair.Key.ToString().ToLowerInvariant()+":4:"+pair.Value);
            foreach(var pair in _sphereFinals)if(!string.IsNullOrEmpty(pair.Value)) {
                var type=pair.Key.ToString().ToLowerInvariant();var branch=GetSphereBranch(pair.Key);var finals=SphereEvolutionCatalog.GetFinals(branch);var finalIndex=-1;
                for(var i=0;i<finals.Count;i++)if(finals[i].Id==pair.Value)finalIndex=i;
                if(finalIndex<0)throw new InvalidOperationException("Unknown source Sphere final: "+pair.Value);
                evolutions.Add("sphere:"+type+":7:"+branch+":"+finalIndex);
            }
            foreach(var pair in _abilityBranches)if(!string.IsNullOrEmpty(pair.Value))evolutions.Add("ability:"+SourceAbilityId(pair.Key)+":4:"+pair.Value);
            foreach(var pair in _abilityFinals)if(!string.IsNullOrEmpty(pair.Value))evolutions.Add("ability:"+SourceAbilityId(pair.Key)+":7:"+pair.Value);player["evolutions"]=VisualJs.Arr(evolutions.ToArray());
            var modifiers=VisualJs.Obj();foreach(SphereId type in Enum.GetValues(typeof(SphereId)))modifiers[type.ToString().ToLowerInvariant()]=RenderValue(GetSourceSphereModifiers(type));scene["nativeSphereModifiers"]=modifiers;
            scene["time"]=_runTime;scene["wave"]=_wave;scene["gameOver"]=_gameOver;scene["paused"]=_paused;scene["camera"]=SourcePoint((Vector2)(_camera.transform.position-_lastHunterScreenShakeOffset));scene["screenShake"]=Mathf.Max(_hunterScreenShakeIntensity,_characterScreenShakeIntensity);scene["evolutionsThisRun"]=_runEvolutions;scene["bossDefeated"]=_bossesDefeated;scene["tutorialMode"]=_tutorialStep>=0;scene["difficulty"]=PlayerPrefs.GetString(DifficultyPreferenceKey,"normal");
            scene["formationMemory"]=_formationMemoryTimer>0f&&_formationMemoryPoints!=null?VisualJs.Obj("type",_formationMemoryType.ToString().ToLowerInvariant(),"nodes",RenderValue(_formationMemoryPoints),"expiresAt",_runTime+_formationMemoryTimer):VisualJs.Null;
            scene["flashText"]=_messageTimer>0f?VisualJs.Obj("text",_message,"color",_sourceFlashColor,"kind",_sourceFlashKind,"life",_messageTimer):VisualJs.Null;
            var upgrades=VisualJs.Obj();foreach(var key in new[]{"hp","dmg","speed","xp","radius","spheres","crit"})upgrades[key]=GetShopUpgradeLevel(key);scene["shopUpgrades"]=upgrades;
            foreach(var key in new[]{"spheres","enemies","sphereProjectiles","minions","xpOrbs","healthPacks","runes","chests","stellaChests","particles","damageNumbers","lightnings"})scene[key]=VisualJs.Arr();
            void Add(string key,V value)=>scene[key]["push"].Call(value);
            foreach(var sphere in _spheres)if(sphere!=null&&sphere.gameObject.activeInHierarchy)Add("spheres",RenderEntity(sphere));
            foreach(var enemy in _enemies)if(enemy!=null&&enemy.IsAlive)Add("enemies",RenderEntity(enemy));
            scene["bossArrow"]=VisualJs.Null;
            if(_ownedArtifacts.Contains("foresight_eye"))foreach(var enemy in _enemies)if(enemy!=null&&enemy.IsAlive&&enemy.IsBoss){var delta=(Vector2)enemy.transform.position-PlayerPosition;if(delta.sqrMagnitude>0f)scene["bossArrow"]=SourcePoint(delta.normalized*SpherePlacementRules.SourcePixelsToWorld);break;}
            foreach(var agent in _simulationAgents) {
                if(agent==null||!agent.gameObject.activeInHierarchy)continue;
                var key=agent is ProjectileAgent?"sphereProjectiles":agent is AbilityDroneAgent2D?"minions":agent is ExperienceOrbAgent?"xpOrbs":agent is HealthPackAgent2D?"healthPacks":agent is RuneAgent2D?"runes":agent is ArtifactChestAgent2D?"chests":agent is StellaChestAgent2D?"stellaChests":agent is SourceParticle2D?"particles":null;
                if(key!=null)Add(key,RenderEntity(agent));
                if(agent is BossProjectileAgent2D bullet&&bullet.SourceOwner!=null&&bullet.SourceOwner.IsAlive)_sourceRenderEntities[bullet.SourceOwner]["bossProjectiles"]["push"].Call(RenderEntity(bullet));
            }
            foreach(var number in _damageNumbers)Add("damageNumbers",VisualJs.Obj("pos",SourcePoint(number.position),"value",number.value,"crit",number.critical,"element",number.element,"life",number.life,"maxLife",number.maxLife,"sourceSphereType",number.sourceSphereType==null?VisualJs.Null:new V(number.sourceSphereType)));
            foreach(var lightning in _sourceLightningVisuals)Add("lightnings",VisualJs.Obj("from",SourcePoint(lightning.from),"to",SourcePoint(lightning.to),"life",lightning.life,"sourceSphere",lightning.sphereType==null?VisualJs.Null:VisualJs.Obj("type",lightning.sphereType)));
            var pois=new List<V>();if(_regionFieldVisuals!=null)foreach(var poi in _regionFieldVisuals.CapturePoiState().pois)pois.Add(VisualJs.Obj("id",poi.id,"type",poi.type,"pos",SourcePoint(poi.position),"alive",true));
            var region=scene["region"];region["id"]=RegionSourceId(_selectedRegion);region["mode"]=_regionEndlessMode?"endless":"stabilization";region["pocketId"]=_regionHudPocketId;region["pois"]=VisualJs.Arr(pois.ToArray());
            var phase=Visual_region.Scope["getRegionPhase"].Call(_runTime);region["phaseId"]=phase["id"];region["phaseName"]=phase["name"];
            region["cleared"]=_regionStabilized;region["endlessUnlocked"]=IsRegionEndlessUnlocked(_selectedRegion);region["challengeId"]=_regionChallengeId;region["challengeContractsCompleted"]=_regionChallengeContractsCompleted;region["masteryChallengesCompleted"]=_regionMasteryChallengesCompleted;region["masteryGeometryTypes"]=RenderValue(_regionMasteryGeometryTypes);region["masteryDominantSeconds"]=_regionMasteryDominantSeconds;region["loreDiscovered"]=RenderValue(_regionLoreDiscovered);region["damageTaken"]=_regionDamageTaken;region["lastHp"]=_regionLastHp;region["regionGoldReward"]=_regionGoldReward;region["regionXpReward"]=_regionXpReward;
            if(_regionSchedule!=null){region["majorBossesSpawned"]=_regionEndlessMode?_endlessBossesSpawned:_regionSchedule.MajorBossesSpawned;region["nextBossAt"]=_regionEndlessMode?_endlessNextBossAt:_regionSchedule.NextBossAt;region["finalBossSpawned"]=_regionSchedule.FinalBossSpawned;region["finalBossDefeated"]=_regionSchedule.FinalBossDefeated;}
            scene["nativeNetwork"]=RenderNetwork();
            foreach(var entity in new List<MonoBehaviour>(_sourceRenderEntities.Keys))if(entity==null)_sourceRenderEntities.Remove(entity);
        }
        private void LateUpdate()
        {
            if(_playerCore==null||_camera==null)return;
            VisualBindings.Locale=IsRussianUi?"ru":"en";
            if(_sourceCanvas==null) {
                VisualJs.ImageFactory=()=>new V(new SourceCanvasImage());
                _sourceCanvas=new GameObject("Source Canvas — renderer.ts").AddComponent<SourceCanvasSurface>();_sourceCanvas.Initialize(100);
            }
            var dpr=SourceDevicePixelRatio;var width=Mathf.Max(1f,Mathf.Floor(Mathf.Min(1280f,Screen.width/dpr)+.5f));var height=Mathf.Max(1f,Mathf.Floor(Mathf.Min(800f,Screen.height/dpr)+.5f));
            _camera.orthographicSize=Screen.height/dpr*SpherePlacementRules.SourcePixelsToWorld*.5f;
            UpdateSourceRenderState();_sourceCanvas.SetViewport(width,height,dpr);
            _sourceCanvas.transform.position=new Vector3(_camera.transform.position.x,_camera.transform.position.y,0f);
            _sourceCanvas.Begin(new Vector2(width*.5f,height*.5f));
            _sourceCanvas.Canvas.Set("canvas",VisualJs.Obj("nativeViewport",VisualJs.Obj("cssWidth",width,"cssHeight",height,"dpr",1)));
            Visual_renderer.Scope["render"].Call(_sourceCanvas.Canvas.Value,_sourceRenderState,width,height);_sourceCanvas.End();
            // The canvas is an authored DOM element, underneath the original
            // HUD and overlays. Its CSS filter is applied by that element.
            DrawSourceDocument();
            foreach(var renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))renderer.enabled=false;
        }
        private static float SourceDevicePixelRatio
        {
            get {
#if UNITY_ANDROID && !UNITY_EDITOR
                using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))using(var resources=activity.Call<AndroidJavaObject>("getResources"))using(var metrics=resources.Call<AndroidJavaObject>("getDisplayMetrics"))return Mathf.Max(1f,metrics.Get<float>("density"));
#else
                return 1f;
#endif
            }
        }
    }
}
