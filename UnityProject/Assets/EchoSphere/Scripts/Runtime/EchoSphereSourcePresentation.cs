using System;
using System.Collections;
using System.Collections.Generic;
using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeWorldSurface _worldSurface;
        public bool UsesSourcePresentation=>true;
#if UNITY_EDITOR
        private V _sourceRenderState;
#endif
        [Serializable] private sealed class SourceLightningVisual {public Vector2 from,to;public float life;public string sphereType;}
        private readonly List<SourceLightningVisual> _sourceLightningVisuals=new List<SourceLightningVisual>();
        private void AddSourceLightningVisual(Vector2 from,Vector2 to,float life,SphereId? type=null)
        {_sourceLightningVisuals.Add(new SourceLightningVisual {from=from,to=to,life=life,sphereType=type.HasValue?type.Value.ToString().ToLowerInvariant():null});}
        private void TickSourceLightningVisuals(float dt)
        {for(var i=_sourceLightningVisuals.Count-1;i>=0;i--){_sourceLightningVisuals[i].life-=dt;if(_sourceLightningVisuals[i].life<=0f)_sourceLightningVisuals.RemoveAt(i);}if(_sourceLightningVisuals.Count>36)_sourceLightningVisuals.RemoveRange(0,_sourceLightningVisuals.Count-36);}
#if UNITY_EDITOR
        private readonly Dictionary<MonoBehaviour,V> _sourceRenderEntities=new Dictionary<MonoBehaviour,V>();
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
            if(value is SourceSphereModifiers modifiers)return VisualJs.Obj(
                "damage",modifiers.damage,"radius",modifiers.radius,"delay",modifiers.delay,"pierce",modifiers.pierce,"multishot",modifiers.multishot,
                "chainTargets",modifiers.chainTargets,"auraRadius",modifiers.auraRadius,"auraPulse",modifiers.auraPulse,"spreadMult",modifiers.spreadMult,
                "splitChance",modifiers.splitChance,"echoChance",modifiers.echoChance,"staticChance",modifiers.staticChance,"resonantCharge",modifiers.resonantCharge,
                "healOnHit",modifiers.healOnHit,"healOnKill",modifiers.healOnKill,"knockback",modifiers.knockback,"rotationSpeed",modifiers.rotationSpeed,
                "fire",modifiers.fire,"freeze",modifiers.freeze,"poison",modifiers.poison,"breach",modifiers.breach,"overload",modifiers.overload,
                "split",modifiers.split,"shatter",modifiers.shatter,"execute",modifiers.execute,"ricochet",modifiers.ricochet,"mark",modifiers.mark,
                "echo",modifiers.echo,"anchor",modifiers.anchor,"phase",modifiers.phase,"static",modifiers.@static,"resonant",modifiers.resonant,
                "magnetic",modifiers.magnetic,"vampiric",modifiers.vampiric,"corrupt",modifiers.corrupt,"drain",modifiers.drain,"afterimage",modifiers.afterimage,
                "impact",modifiers.impact,"gravitic",modifiers.gravitic);
            if(value is FractalFormationSnapshot snapshot)return VisualJs.Obj("type",snapshot.Type.ToString().ToLowerInvariant(),"nodes",RenderValue(snapshot.Nodes));
            if(value is Enum enumeration)return enumeration.ToString().ToLowerInvariant();
            if(value is IDictionary dictionary) {var result=VisualJs.Obj();foreach(DictionaryEntry entry in dictionary)result[RenderValue(entry.Key)]=RenderValue(entry.Value);return result;}
            if(value is IEnumerable list) {var result=new List<V>();foreach(var item in list)result.Add(RenderValue(item));return VisualJs.Arr(result.ToArray());}
            if(value is IConvertible)return new V(Convert.ToDouble(value,System.Globalization.CultureInfo.InvariantCulture));
            if(value is ShopUpgradeData shop)return VisualJs.Obj("dmg",shop.dmg,"radius",shop.radius,"speed",shop.speed,"spheres",shop.spheres,"hp",shop.hp,"crit",shop.crit,"xp",shop.xp);
            if(value is LeaderboardEntry leaderboard)return VisualJs.Obj("name",leaderboard.name,"time",leaderboard.time,"wave",leaderboard.wave,"date",(double)leaderboard.date);
            if(value is CharacterProfileData profile)return VisualJs.Obj("id",profile.id,"masteryLevel",profile.masteryLevel,"masteryXp",profile.masteryXp,"unlocked",profile.unlocked);
            throw new InvalidOperationException("Unmapped presentation model: "+value.GetType().Name);
        }
        private V RenderEntity(MonoBehaviour entity)
        {
            if(entity==null)return VisualJs.Null;
            if(!_sourceRenderEntities.TryGetValue(entity,out var view)){view=VisualJs.Obj();_sourceRenderEntities.Add(entity,view);}
            view["pos"]=SourcePoint((Vector2)entity.transform.position);view["alive"]=entity.gameObject.activeInHierarchy;
            if(entity is SphereAttackAgent sphere) {
                view["type"]=sphere.Type.ToString().ToLowerInvariant();view["radius"]=130;view["visualTier"]=sphere.ProgressionLevel;
                view["damage"]=12;view["attackDelay"]=1.2;view["attackTimer"]=sphere.AttackTimer;view["auraTimer"]=sphere.Type==SphereId.Orbital?sphere.SourceProjectionOrbitalContactCheckTimer:new V(sphere.AttackTimer);
                view["rotation"]=sphere.Type==SphereId.Orbital?sphere.SourceProjectionOrbitAngle:new V(sphere.Type==SphereId.Gravity?sphere.VisualRotation:-sphere.VisualRotation);
                view["networkDisabledTimer"]=sphere.SourceProjectionNetworkDisabledTimer;view["formationHitCount"]=sphere.FormationHitCount;view["resonancePulseTimer"]=sphere.ResonancePulseTimer;
            } else if(entity is EnemyAgent2D enemy) {
                view["hp"]=enemy.CurrentHp;view["maxHp"]=enemy.MaxHp;view["radius"]=enemy.SourceRadius;view["color"]=enemy.SourceColorHtml;
                view["type"]=enemy.SourceMonsterType;view["isBoss"]=enemy.IsBoss;view["isElite"]=enemy.IsElite;view["bossType"]=enemy.BossType;view["role"]=enemy.Role;view["eliteVariant"]=enemy.EliteVariant;
                var visual=enemy.SourceProjectionCreatureVisual;view["visualVariant"]=visual==null?VisualJs.Null:new V(visual.SourceVariant);
                view["hitFlash"]=enemy.SourceProjectionHitFlash;view["tier"]=enemy.SourceVisualTier;view["freezeTimer"]=enemy.SourceProjectionFreezeTimer;view["fireTimer"]=enemy.SourceProjectionFireTimer;view["poisonTimer"]=enemy.SourceProjectionPoisonTimer;
                view["rotation"]=enemy.SourceProjectionRotation;
                view["elitePulseTelegraphTimer"]=enemy.SourceProjectionLinkBreakerTelegraphTimer;view["elitePulseTarget"]=enemy.SourceProjectionLinkBreakerTarget==null?VisualJs.Null:RenderEntity(enemy.SourceProjectionLinkBreakerTarget);
                view["bossProjectiles"]=VisualJs.Arr();
                var combat=enemy.SourceProjectionBossCombat;
                if(combat!=null){view["bossShootTimer"]=combat.ShootTimer;view["summonTimer"]=combat.SummonTimer;view["chargeTimer"]=combat.ChargeTimer;view["isCharging"]=combat.IsCharging;view["chargeDir"]=SourcePoint(combat.ChargeDirection*SpherePlacementRules.SourcePixelsToWorld);view["auraRadius"]=combat.AuraRadius/SpherePlacementRules.SourcePixelsToWorld;}
            } else if(entity is ProjectileAgent projectile) {
                view["life"]=projectile.SourceProjectionLife;view["radius"]=projectile.SourceProjectionRadius;view["color"]=RenderValue(projectile.SourceProjectionImpactColor);view["damage"]=projectile.SourceProjectionDamage;
                view["pierce"]=projectile.SourceProjectionRemainingPierces;view["ricochet"]=projectile.SourceProjectionRemainingRicochets;view["sourceSphere"]=projectile.SourceProjectionEvolutionOwner==null?VisualJs.Null:RenderEntity(projectile.SourceProjectionEvolutionOwner);
                view["vel"]=SourcePoint(projectile.SourceProjectionDirection*projectile.SourceProjectionSpeed);
                var element=projectile.SourceProjectionElement;view["effect"]=element==SphereElement.None?VisualJs.Null:new V(element.ToString().ToLowerInvariant());
            } else if(entity is BossProjectileAgent2D bullet) {
                view["vel"]=SourcePoint(bullet.SourceProjectionVelocity);view["radius"]=(bullet.SourceProjectionHitRadius-BossCombatRules.PlayerRadiusWorld)/SpherePlacementRules.SourcePixelsToWorld;
                view["visualType"]=bullet.SourceProjectionBossType;
            } else if(entity is AbilityDroneAgent2D drone) {
                view["life"]=drone.SourceProjectionRemaining;view["rotation"]=drone.SourceProjectionRotation;view["anchorType"]=drone.SourceProjectionAnchorType.ToString().ToLowerInvariant();view["radius"]=12;
            } else if(entity is ExperienceOrbAgent orb) {view["radius"]=6;view["rotation"]=orb.SourceProjectionRotation;}
            else if(entity is RuneAgent2D rune){view["type"]=rune.SourceProjectionType;view["life"]=rune.SourceProjectionLife;}
            else if(entity is SourceParticle2D particle) {view["life"]=particle.SourceProjectionLife;view["maxLife"]=particle.SourceProjectionMaxLife;view["vel"]=SourcePoint(particle.SourceProjectionVelocity);view["color"]=RenderValue(particle.SourceProjectionColor);view["size"]=particle.SourceProjectionSize;}
            return view;
        }
        private static V RenderShape(SphereNetworkShape shape)=>shape==null?VisualJs.Null:VisualJs.Obj("type",shape.Type.ToString().ToLowerInvariant(),"strength",shape.Strength,"dominanceScore",shape.DominanceScore,"nodes",VisualJs.Arr(shape.NodeIndexes.ConvertAll(x=>new V((double)x)).ToArray()),"active",shape.Active);
        private V RenderNetwork()=>RenderNativeNetworkSnapshot(_networkState);
        private static V RenderNativeNetworkSnapshot(SphereNetworkState network)
        {
            network=network??SphereNetworkRules.Analyze(new List<SphereNetworkNode>());
            var result=VisualJs.Obj("linkDistance",network.LinkDistance/SpherePlacementRules.SourcePixelsToWorld,"nodes",VisualJs.Arr(network.Nodes.ConvertAll(index=>new V(index)).ToArray()),"formationCandidates",VisualJs.Arr(network.FormationCandidates.ConvertAll(RenderShape).ToArray()),"dominantFormation",RenderShape(network.DominantFormation),"secondaryFormation",RenderShape(network.SecondaryFormation),"formationEfficiency",network.FormationEfficiency);
            result["links"]=VisualJs.Arr(network.Links.ConvertAll(x=>VisualJs.Obj("a",x.A,"b",x.B,"distance",x.Distance/SpherePlacementRules.SourcePixelsToWorld)).ToArray());
            foreach(SphereNetworkFormation type in Enum.GetValues(typeof(SphereNetworkFormation)))if(type!=SphereNetworkFormation.None)result[type.ToString().ToLowerInvariant()]=RenderShape(network.GetFormation(type));
            return result;
        }
        private void UpdateSourceRenderState()
        {
            if(_sourceRenderState.Undefined)_sourceRenderState=NativeRunSnapshotDefaults.Create();var scene=_sourceRenderState;var player=scene["player"];
            // Directly corresponding timers and counters keep their source names.
            // Explicit typed projection for live runtime fields; avoid per-frame FieldInfo.GetValue.
            // Live presentation inputs: these values are already updated by the
            // native simulation and must not remain at createInitialState defaults.
            player["level"]=_level;player["mutationStage"]=_mutationStage;player["buffTimer"]=_buffTimer;
            player["shieldCooldown"]=_shieldCooldown;player["fireTrailCooldown"]=_fireTrailCooldown;player["minionCooldown"]=_minionCooldown;player["lightningCooldown"]=_lightningCooldown;
            player["shieldTimer"]=_shieldTimer;player["shieldVisualPulse"]=_shieldVisualPulse;player["deathTimer"]=_deathTimer;
            player["chaosOrbBuff"]=_chaosOrbBuff==null?VisualJs.Null:new V(_chaosOrbBuff);player["chaosOrbBuffTimer"]=_chaosOrbBuffTimer;
            player["teleportDamageBuffTimer"]=_teleportDamageBuffTimer;player["fireCatalystTimer"]=_fireCatalystTimer;
            player["architectFormationType"]=_architectFormationType.ToString().ToLowerInvariant();player["architectFormationChangedAt"]=_architectFormationChangedAt;
            player["xp"]=_xp;player["activeAbilitySlots"]=_activeAbilitySlots;player["kills"]=_kills;
            player["resonanceCharge"]=_resonanceCharge;player["resonanceEventActive"]=_resonanceEventActive;player["resonanceGeometryKey"]=_resonanceGeometryKey;
            player["resonanceEventsTriggered"]=_resonanceEventsTriggered;player["spheristChorusHits"]=_spheristChorusHits;
            player["fireTrailTimer"]=_fireTrailTimer;player["blastCooldown"]=_blastCooldown;player["teleportCooldown"]=_teleportCooldown;
            player["swiftBootsTimer"]=_swiftBootsTimer;player["sphereMovementLocked"]=_sphereMovementLocked;player["combo"]=_combo;player["comboTimer"]=_comboTimer;
            player["characterMasteryLevel"]=_characterMasteryLevel;
            player["hunterMarkTarget"]=_hunterMarkTarget==null?VisualJs.Null:RenderEntity(_hunterMarkTarget);player["hunterMarkTimer"]=_hunterMarkTimer;player["hunterHitCount"]=_hunterHitCount;
            player["hunterHuntTarget"]=_hunterHuntTarget==null?VisualJs.Null:RenderEntity(_hunterHuntTarget);player["hunterHuntTimer"]=_hunterHuntTimer;player["hunterTrophyTimer"]=_hunterTrophyTimer;
            player["engineerRelaySource"]=_engineerRelaySource==null?VisualJs.Null:RenderEntity(_engineerRelaySource);player["engineerRelayTimer"]=_engineerRelayTimer;
            player["alchemistCatalystTimer"]=_alchemistCatalystTimer;player["alchemistReactionCount"]=_alchemistReactionCount;player["conductorOverdriveTimer"]=_conductorOverdriveTimer;
            player["oracleForecastKeys"]=RenderValue(_oracleForecastKeys);player["oracleForecastRerollUsed"]=_oracleForecastRerollUsed;
            player["fractalFormationHistory"]=RenderValue(_fractalFormationHistory);player["fractalEchoTimer"]=_fractalEchoTimer;player["fractalEchoNodes"]=RenderValue(_fractalEchoNodes);
            player["voidPhantomTimer"]=_voidPhantomTimer;player["voidPhantomPulseCount"]=_voidPhantomPulseCount;player["voidPhantomSource"]=_voidPhantomSource==null?VisualJs.Null:RenderEntity(_voidPhantomSource);
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
            var abilities=VisualJs.Obj();foreach(var pair in _abilityLevels)abilities[SourceAbilityId(pair.Key)]=pair.Value;abilities["maxspheres"]=GetAbilityLevel(RunUpgradeKind.MaxSpheres);player["abilities"]=abilities;
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
            foreach(var key in new[]{"spheres","enemies"})scene[key]=VisualJs.Arr();
            void Add(string key,V value)=>((List<V>)scene[key].Raw).Add(value);
            foreach(var sphere in _spheres)if(sphere!=null&&sphere.gameObject.activeInHierarchy)Add("spheres",RenderEntity(sphere));
            foreach(var enemy in _enemies)if(enemy!=null&&enemy.IsAlive)Add("enemies",RenderEntity(enemy));
            scene["bossArrow"]=VisualJs.Null;
            if(_ownedArtifacts.Contains("foresight_eye"))foreach(var enemy in _enemies)if(enemy!=null&&enemy.IsAlive&&enemy.IsBoss){var delta=(Vector2)enemy.transform.position-PlayerPosition;if(delta.sqrMagnitude>0f)scene["bossArrow"]=SourcePoint(delta.normalized*SpherePlacementRules.SourcePixelsToWorld);break;}
            // Transient entity layers read Unity components directly. Do not
            // build duplicate V dictionaries for those native consumers.
            var pois=new List<V>();_nativeRegionPois.Clear();if(_regionFieldVisuals!=null)_regionFieldVisuals.CopyPresentationPois(_nativeRegionPois);foreach(var poi in _nativeRegionPois)pois.Add(VisualJs.Obj("id",poi.Id,"type",poi.Type,"pos",SourcePoint(poi.Position),"alive",true));
            var region=scene["region"];region["id"]=RegionSourceId(_selectedRegion);region["mode"]=_regionEndlessMode?"endless":"stabilization";region["pocketId"]=_regionHudPocketId;region["pois"]=VisualJs.Arr(pois.ToArray());
            int phase=NativeRegionPhaseIndex(_runTime);region["phaseId"]=phase;region["phaseName"]=NativeLocalizedName(NativeRunCatalog.Instance.RegionPhases[phase]);
            region["cleared"]=_regionStabilized;region["endlessUnlocked"]=IsRegionEndlessUnlocked(_selectedRegion);region["challengeId"]=_regionChallengeId;region["challengeContractsCompleted"]=_regionChallengeContractsCompleted;region["masteryChallengesCompleted"]=_regionMasteryChallengesCompleted;region["masteryGeometryTypes"]=RenderValue(_regionMasteryGeometryTypes);region["masteryDominantSeconds"]=_regionMasteryDominantSeconds;region["loreDiscovered"]=RenderValue(_regionLoreDiscovered);region["damageTaken"]=_regionDamageTaken;region["lastHp"]=_regionLastHp;region["regionGoldReward"]=_regionGoldReward;region["regionXpReward"]=_regionXpReward;
            if(_regionSchedule!=null){region["majorBossesSpawned"]=_regionEndlessMode?_endlessBossesSpawned:_regionSchedule.MajorBossesSpawned;region["nextBossAt"]=_regionEndlessMode?_endlessNextBossAt:_regionSchedule.NextBossAt;region["finalBossSpawned"]=_regionSchedule.FinalBossSpawned;region["finalBossDefeated"]=_regionSchedule.FinalBossDefeated;}
            scene["nativeNetwork"]=RenderNetwork();
            foreach(var entity in new List<MonoBehaviour>(_sourceRenderEntities.Keys))if(entity==null)_sourceRenderEntities.Remove(entity);
        }
#endif
        private void LateUpdate()
        {
            try{
            if(_playerCore==null||_camera==null)return;
            #if UNITY_EDITOR
            VisualBindings.Locale=IsRussianUi?"ru":"en";
            #endif
            if(_mainMenu)_nativeRunDocument?.Hide();
            bool nativeHome=TryDrawNativeMainMenu();
            bool nativeProgression=TryDrawNativeProgression();
            bool nativeSettings=TryDrawNativeSettings();
            bool nativeRegions=TryDrawNativeRegions();
            bool nativeCharacters=TryDrawNativeCharacters();
            bool nativeArchive=TryDrawNativeArchive();
            TraceNativeRoute("state");
            if(nativeHome||nativeProgression||nativeSettings||nativeRegions||nativeCharacters||nativeArchive)return;
            if(_worldSurface==null) {
                var nativeScene=NativeBattleSceneRoot.Find(gameObject.scene);
                _worldSurface=nativeScene!=null?nativeScene.World:new GameObject("Native world compositor").AddComponent<NativeWorldSurface>();_worldSurface.Initialize();
                // The native world is consumed as a UI texture. Culling the
                // unused scene presentation avoids scanning every Renderer on
                // every frame and also covers newly spawned legacy objects.
                _camera.cullingMask=0;
            }
            var dpr=SourceDevicePixelRatio;var width=Mathf.Max(1f,Mathf.Floor(Mathf.Min(1280f,Screen.width/dpr)+.5f));var height=Mathf.Max(1f,Mathf.Floor(Mathf.Min(800f,Screen.height/dpr)+.5f));
            _camera.orthographicSize=Screen.height/dpr*SpherePlacementRules.SourcePixelsToWorld*.5f;
            RefreshNativePresentedState();
            // Retained menu screens own their rendering; only a run needs
            // the native world texture.
            if(!_mainMenu){
            _worldSurface.SetViewport(width,height,dpr);
            _worldSurface.transform.position=new Vector3(_camera.transform.position.x,_camera.transform.position.y,0f);
            _worldSurface.Begin(new Vector2(width*.5f,height*.5f));
            RenderNativeWorldFrame(width,height);_worldSurface.End();
            }
            // The retained UI Toolkit document composites the native world
            // texture beneath the HUD and authored material/filter treatments.
            DrawSourceDocument();
            }finally{NativeEngineBackdrop.RefreshAllVisibility();}
        }
        private static float SourceDevicePixelRatio
        {
            get {
#if UNITY_ANDROID && !UNITY_EDITOR
                var width=Screen.width;var height=Screen.height;var dpi=Screen.dpi;
                if(_sourceDensity>0f&&width==_sourceDensityWidth&&height==_sourceDensityHeight&&dpi==_sourceDensityDpi)return _sourceDensity;
                using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))using(var resources=activity.Call<AndroidJavaObject>("getResources"))using(var metrics=resources.Call<AndroidJavaObject>("getDisplayMetrics")){
                    var density=metrics.Get<float>("density");
                    _sourceDensity=density>0f&&!float.IsInfinity(density)?density:1f;
                }
                _sourceDensityWidth=width;_sourceDensityHeight=height;_sourceDensityDpi=dpi;return _sourceDensity;
#else
                return 1f;
#endif
            }
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        private static float _sourceDensity,_sourceDensityDpi;
        private static int _sourceDensityWidth,_sourceDensityHeight;
#endif
        private static void InvalidateSourceDensity()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _sourceDensity=0f;
#endif
        }
    }
}
