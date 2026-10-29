using System.Linq;
using EchoSphere;
using System;
using System.Collections.Generic;
using System.Reflection;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private static bool EntitySnapshotField(FieldInfo field) =>
            !(field.DeclaringType == typeof(SphereAttackAgent) && (field.Name == "_presentationAttackActive" || field.Name == "_presentationAimRotation" || field.Name == "_presentationAimValid" || field.Name == "_presentationShotSerial" || field.Name == "_presentationShotTime")) &&
            !(field.DeclaringType == typeof(EnemyAgent2D) && field.Name == "_visualVariant") &&
            !(typeof(IRunSaveColor).IsAssignableFrom(field.DeclaringType) && field.Name == "_savedColor");
        private const string RunSnapshotKey = "echosphere_active_run_v1";
        private float _runSaveAccumulator;
        private sealed class RunSaveBuffers
        {
            internal SnapshotValue Player,Runtime;
            internal readonly SavedRun Envelope=new SavedRun();
            private readonly List<SavedEntity> _records=new List<SavedEntity>();
            private int _used;
            internal SavedEntity NextEntity(){
                if(_used==_records.Count)_records.Add(new SavedEntity());var record=_records[_used++];
                record.kind=record.name=record.variant=null;record.state=record.auxiliary=null;
                record.hasProjectileOwner=false;record.projectileOwnerIndex=0;return record;
            }
            internal readonly List<EnemyAgent2D> LiveEnemies=new List<EnemyAgent2D>();
            private readonly Dictionary<int,SnapshotValue> _states=new Dictionary<int,SnapshotValue>();
            private readonly HashSet<int> _seen=new HashSet<int>();private readonly List<int> _stale=new List<int>();
            internal void Begin(){LiveEnemies.Clear();_seen.Clear();_used=0;Envelope.entities.Clear();}
            internal SnapshotValue Capture(MonoBehaviour agent){int id=agent.ExistingObjectId();_seen.Add(id);_states.TryGetValue(id,out var state);state=RunSnapshotCodec.CaptureRetained(agent,state,EntitySnapshotField);_states[id]=state;return state;}
            internal void End(){for(int i=_used;i<_records.Count;i++){_records[i].state=_records[i].auxiliary=null;_records[i].kind=_records[i].name=_records[i].variant=null;}_stale.Clear();foreach(var pair in _states)if(!_seen.Contains(pair.Key))_stale.Add(pair.Key);foreach(int id in _stale)_states.Remove(id);_stale.Clear();}
        }
        private readonly RunSaveBuffers _runSaveBuffers=new RunSaveBuffers();
        [Serializable] private sealed class SavedEntity
        {
            public string kind, name, variant;
            public Vector3 position, scale;
            public Color color;
            public SnapshotValue state, auxiliary;
            public bool hasProjectileOwner;
            public int projectileOwnerIndex;
        }
        [Serializable] private sealed class SavedRun
        {
            public int version = 2;
            public long savedAt;
            public float time;
            public Vector3 playerPosition;
            public SnapshotValue runtime, player;
            public RegionFieldVisuals.PoiSnapshot regionPois;
            public List<SavedEntity> entities = new List<SavedEntity>();
        }
        private static readonly HashSet<string> NonRunFields = new HashSet<string>
        {
            "_nativeMerchantView", "_spaceCollisionBodies", "_spaceCollisionBodyPool", "_spaceCollisionTowers", "_spaceCollisionWorkspace", "_enemyQueryListCount",
            "_nativeNetworkProjection", "_nativePausedWorldFrame",
            "_nativeTransientPools",
            "_simulationTypes", "_simulationPhases",
            "_runSaveBuffers",
            "_nativeProgressionScreen", "_nativeProgressionCatalog", "_nativeProgressionPage", "_nativeProgressionWasActive", "_nativeProgressionState",
            "_nativeRegionScreen", "_nativeRegionState", "_nativeRegionWasActive",
            "_nativeCharacterScreen", "_nativeCharacterState", "_nativeCharacterWasActive", "_nativeCharacterMessage",
            "_nativeArchiveCatalog", "_nativeArchiveScreen", "_nativeArchiveState", "_nativeArchiveWasActive",
            "_nativeRunResult",
            "_nativeUpgradeView", "_nativeUpgradeBoundVersion",
            "_nativeArtifactRewards", "_nativeRewardSetDetail", "_nativePauseSetDetail", "_nativeRewardSetId", "_nativeArtifactRewardVersion",
            "_nativeGhostNetwork", "_nativeFormationMemory", "_nativeGhostNetworkState", "_nativeFormationMemoryState",
            "_nativeJoystickFeedback", "_nativePlacementFeedback", "_nativeJoystickState", "_nativePlacementState",
            "_nativePresentedDominant", "_nativePresentedSecondary", "_nativeRegionPois", "_nativeProjectileCount", "_nativeParticleCount", "_nativeEnemyProjectileGroups", "_nativeEnemyProjectileLists",
            "_nativeWorldField", "_nativeWorldAmbient", "_nativeWorldRegion", "_nativeWorldPickups", "_nativeWorldMinions", "_nativeWorldEnergyBolts", "_nativeWorldSprites", "_nativeWorldProjectiles", "_nativeWorldCreatures", "_nativeWorldStandardSphere", "_nativeWorldPickableViews", "_nativeWorldPlayer", "_nativeWorldSphereFamilies", "_nativeWorldCharacterIndicators", "_nativeWorldNetwork", "_nativeWorldFeedback",
            "_nativeMobileMoveInput",
            "_nativeMobilePointers", "_nativeMobileJoystickId", "_nativeMobileJoystickX", "_nativeMobileJoystickY", "_nativeMobileDx", "_nativeMobileDy", "_nativeMobileJoystickActive",
            "_nativeMobileLastX", "_nativeMobileLastY", "_nativeMobileGhost", "_nativeMobileMemory", "_nativeMobileMemoryClosed", "_nativeMobileMounted", "_nativeMobileHandedness", "_nativeMobilePresent",
            "_nativeMobileMemoryAt", "_nativeMobilePlacementAt", "_nativeMobilePlacementColor", "_nativeMobilePlacement", "_nativeMobileGestureRoot",
            "_nativeMobileDock", "_nativeMobileDockState",
            "_nativeTutorial", "_nativeTutorialTarget", "_nativeTutorialTargetAt", "_nativeTutorialTargetKey",
            "_nativeHudBoundVersion", "_nativeHudIsland", "_nativeHudPilot", "_nativeMainMenuEnabled", "_nativeMainMenuScreen",
            "_nativeMainMenuWasActive", "_nativeMenuCharacterColor", "_nativeMenuModelVersion", "_nativeMenuShownVersion",
            "_maximumEnemyQueryRadius", "_nativeMenuState", "_nativePauseAbilitiesBoundVersion", "_nativePauseAbilitiesIsland", "_nativePauseAbilitiesPilot",
            "_nativePauseArtifactsBoundVersion", "_nativePauseArtifactsIsland", "_nativePauseArtifactsPilot",
            "_nativePauseChromeComposition", "_nativePauseChromeIsland", "_nativePauseChromePilot",
            "_nativePauseScreen", "_nativePauseScreenPilot",
            "_nativePauseStatsBoundVersion", "_nativePauseStatsIsland", "_nativePauseStatsPilot",
            "_nativePauseSynergiesBoundVersion", "_nativePauseSynergiesIsland", "_nativePauseSynergiesPilot",
            "_nativeSettingsScreen", "_nativeSettingsWasActive", "_sourceUiWasPaused",
            "_shopSaveLoaded", "_characterProfiles", "_characterProfilesLoaded", "_leaderboardEntries", "_unlockedAchievementIds",
            "_knownArtifactProtocolIds", "_artifactProtocolKnowledgeLoaded", "_knownKnowledgeIds", "_knowledgeLoaded", "_sourceKnowledge",
            "_mainMenuPage", "_mainMenu", "_regionSelect", "_settingsResetPending", "_characterScrollPosition", "_expandedCharacterId",
            "_uiScale", "_uiWidth", "_uiHeight", "_artifactProtocolScrollPosition", "_knowledgeScrollPosition", "_knowledgeQuery", "_knowledgeSection",
            "_spherePointerActive", "_spherePointerFromTouch", "_spherePointerMoved", "_spherePointerIsMovement", "_spherePointerIsPlacing",
            "_spherePointerTouchId", "_spherePointerStartScreen", "_spherePointerWorld", "_spherePointerPositionValid", "_spherePointerPreview",
            "_spherePointerPreviewPositions", "_spherePointerPreviewIndex", "_runSaveAccumulator", "_lastHunterScreenShakeOffset",
            "_simulationAgents", "_simulationFrame", "_sourceParticles", "_pausePlannerTab", "_pausePlannerScroll", "_pauseArtifactSetId", "_upgradePickerScroll", "_sourceRegionScroll",
            "_sourceRegionBossForFrame", "_sourceSettingsScroll", "_sourceLockedSphereTouch", "_sourceUiTouchFrame", "_sourceUiTouches",
            "_sourceArtifactScroll", "_sourceSetDetailScroll", "_sourceResultRank", "_sourceResultNewRecord",
            "_sourcePointers", "_sourceJoystickId", "_sourceJoystickStart", "_sourceJoystickDrag", "_sourceJoystickDirection", "_sourceTouchLastDirection",
            "_networkGlowRenderers", "_networkLinkRenderers", "_networkHighlightRenderers", "_networkComets", "_networkLineMaterial",
            "_sourceRenderState", "_sourceRenderEntities", "_sourceUi", "_nativeRunDocument", "_nativeRunNextModel", "_nativeMobileLayout", "_nativeRunField", "_sourceUiStateRef", "_sourceUiCanvasRef", "_sourceUiTutorialRef", "_sourceUiSavedRunTime", "_worldSurface", "_sourceHudTooltip", "_sourceHudTooltipUntil", "_sourceHudDragSlot", "_sourceHudTooltipRect"
        };
        private void TickRunAutosave()
        {
            if (_mainMenu || _gameOver || _tutorialStep >= 0) return;
            _runSaveAccumulator += Mathf.Min(0.05f, Time.unscaledDeltaTime);
            if (_runSaveAccumulator < 1f) return;
            _runSaveAccumulator = 0f;
            SaveCurrentRun();
        }
        private void OnApplicationPause(bool paused) { TraceNativeRoute(paused?"application-pause":"application-resume");if (paused) PauseAndSaveRun(); }
        private void OnApplicationFocus(bool focused) { TraceNativeRoute(focused?"application-focus":"application-blur");if (!focused) PauseAndSaveRun(); else InvalidateSourceDensity(); }
        private void OnApplicationQuit() { SaveCurrentRun(); }
        private void PauseAndSaveRun()
        {
            if (_mainMenu || _gameOver || _tutorialStep >= 0) return;
            SetUserPaused(true); ClearSpherePointer(); SaveCurrentRun();
        }
        private void ClearRunSnapshot() { PlayerPrefs.DeleteKey(RunSnapshotKey); PlayerPrefs.Save(); }
        private SavedRun LoadRunSnapshot()
        {
            if (!PlayerPrefs.HasKey(RunSnapshotKey)) return null;
            try
            {
                var save = JsonUtility.FromJson<SavedRun>(PlayerPrefs.GetString(RunSnapshotKey));
                if (save == null || save.version != 2 || save.runtime == null || save.player == null || save.entities == null ||
                    float.IsNaN(save.time) || float.IsInfinity(save.time) || RunSnapshotCodec.Read(save.runtime, "_gameOver", false))
                    throw new FormatException("Invalid run snapshot");
                return save;
            }
            catch (Exception) { ClearRunSnapshot(); return null; }
        }
        private void SaveCurrentRun()
        {
            if (_mainMenu || _gameOver || _tutorialStep >= 0 || _playerCore == null) return;
            try
            {
                var save=CaptureRunSnapshot();
                PlayerPrefs.SetString(RunSnapshotKey, JsonUtility.ToJson(save)); PlayerPrefs.Save();
            }
            catch (Exception error) { Debug.LogWarning("[ECHO SPHERE] Run save failed: " + error.Message); }
            finally { _runSaveBuffers.End(); }
        }
        // Consumed synchronously before another capture may reuse these buffers.
        // Separate from PlayerPrefs so compatibility can be verified without touching a live save.
        private SavedRun CaptureRunSnapshot()
        {
            _runSaveBuffers.Begin();
            _runSaveBuffers.Player=RunSnapshotCodec.CaptureRetained(_playerCore,_runSaveBuffers.Player);
            _runSaveBuffers.Runtime=RunSnapshotCodec.CaptureRetained(this,_runSaveBuffers.Runtime,field=>!NonRunFields.Contains(field.Name));
            var save = _runSaveBuffers.Envelope;
            save.savedAt=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();save.time=_runTime;
            save.playerPosition=_player.position;save.player=_runSaveBuffers.Player;
            save.runtime=_runSaveBuffers.Runtime;save.regionPois=_regionFieldVisuals.CapturePoiState();
            var liveEnemies = _runSaveBuffers.LiveEnemies; foreach (var enemy in _enemies) if (enemy != null && enemy.IsAlive) liveEnemies.Add(enemy);
            foreach (var agent in _simulationAgents)
            {
                if (agent == null || !agent.gameObject.activeInHierarchy) continue;
                var type = agent.GetType();
                if (type != typeof(SphereAttackAgent) && type != typeof(EnemyAgent2D) && type != typeof(ProjectileAgent) &&
                    type != typeof(BossProjectileAgent2D) && type != typeof(AbilityDroneAgent2D) && type != typeof(ExperienceOrbAgent) &&
                    type != typeof(HealthPackAgent2D) && type != typeof(RuneAgent2D) && type != typeof(ArtifactChestAgent2D) &&
                    type != typeof(StellaChestAgent2D) && type != typeof(SourceParticle2D)) continue;
                if (agent is EnemyAgent2D enemy && !enemy.IsAlive) continue;
                var entity = _runSaveBuffers.NextEntity();
                entity.kind=type.Name;entity.name=agent.name;entity.position=agent.transform.position;
                entity.scale=agent.transform.localScale;entity.color=agent is IRunSaveColor appearance?appearance.SavedColor:Color.white;
                entity.state=_runSaveBuffers.Capture(agent);
                if (agent is BossProjectileAgent2D bullet && bullet.HasSourceOwner)
                { entity.hasProjectileOwner = true; entity.projectileOwnerIndex = liveEnemies.IndexOf(bullet.SourceOwner); }
                if (agent is EnemyAgent2D)
                {
                    entity.variant = ((EnemyAgent2D)agent).SourceVisualVariant;
                    var boss = agent.GetComponent<BossCombatAgent2D>();
                    if (boss != null) entity.auxiliary = _runSaveBuffers.Capture(boss);
                }
                save.entities.Add(entity);
            }
            return save;
        }
        private void ContinueSavedRun()
        {
            var save = LoadRunSnapshot(); if (save == null) return;
            RestoreRunSnapshot(save);
        }
        private void RestoreRunSnapshot(SavedRun save)
        {
            _nativePausedWorldFrame=null;
            // Stop old entity updates immediately; Unity's deferred Destroy happens after this frame.
            foreach (var agent in _simulationAgents.Where(agent=>agent!=null&&agent.gameObject.activeInHierarchy).OrderBy(agent=>agent.ExistingObjectId()).ToArray())
            {
                if (agent is SphereAttackAgent || agent is EnemyAgent2D || agent is ProjectileAgent || agent is BossProjectileAgent2D ||
                    agent is AbilityDroneAgent2D || agent is ExperienceOrbAgent || agent is HealthPackAgent2D || agent is RuneAgent2D ||
                    agent is ArtifactChestAgent2D || agent is StellaChestAgent2D || agent is SourceParticle2D)
                { agent.gameObject.SetActive(false); Destroy(agent.gameObject); }
            }
            _spheres.Clear(); _enemies.Clear(); _enemyQueries?.Clear(); _maximumEnemyQueryRadius=0; _enemyQueryListCount=-1; _abilityDrones.Clear();
            // Old snapshots may contain UI ownership flags. Apply the same boundary
            // while reading as while writing so a live menu cannot be orphaned.
            _runEconomy=null; // Missing economy in a legacy snapshot must remain legacy.
            RunSnapshotCodec.Restore(save.runtime, GetType(), this, field => !NonRunFields.Contains(field.Name));
            if(_runEconomy!=null)_runEconomy.Open=false; // Reopen the same saved stock after the resume gate.
            // JSON keeps lightning positions/lifetime but drops sourceSphere.
            foreach(var lightning in _sourceLightningVisuals)lightning.sphereType=null;
            RunSnapshotCodec.Restore(save.player, typeof(PlayerCoreController2D), _playerCore);
            _player.position = save.playerPosition;
            _regionFieldVisuals.SetRegion(_selectedRegion);
            _regionFieldVisuals.RestorePoiState(save.regionPois);
            foreach (var entity in save.entities) RestoreSavedEntity(entity);
            // Recreating Unity presentation can call entity initializers which draw random values.
            // Source restore resumes the saved generator state without those construction draws.
            _rng.RestoreState(RunSnapshotCodec.Read(save.runtime, "_rng", new SeededRng(1u)).State);
            // Identical reference reset policy to src/runPersistence.ts.
            _hunterMarkTarget = _hunterHuntTarget = null; _hunterMarkTimer = _hunterHuntTimer = 0f; _hunterHitCount = 0;
            _engineerRelaySource = null; _engineerRelayTimer = 0f; _voidPhantomSource = null;
            ClearSpherePointer();
            _tutorialStep = -1; _mainMenu = false; _regionSelect = false; _userPaused = true; _paused = true;
            _runSaveAccumulator = 0f;
            RefreshNetworkState();
            _camera.transform.position = new Vector3(_player.position.x, _player.position.y, -10f);
        }
        private void RestoreSavedEntity(SavedEntity entity)
        {
            MonoBehaviour agent = null;
            var state = entity.state;
            if (entity.kind == nameof(SphereAttackAgent))
            {
                CreateSphere(RunSnapshotCodec.Read(state, "_type", SphereId.Standard), RunSnapshotCodec.Read(state, "_progressionLevel", 0), entity.position);
                agent = _spheres[_spheres.Count - 1];
            }
            else if (entity.kind == nameof(EnemyAgent2D))
            {
                var baseColor = RunSnapshotCodec.Read(state, "_baseColor", entity.color);
                var go = CreateEntityRoot(entity.name,entity.position,1f);
                var enemy = go.AddComponent<EnemyAgent2D>();
                var bossType = RunSnapshotCodec.Read(state, "_bossType", "");
                enemy.Initialize(this, _player, RunSnapshotCodec.Read(state, "_maxHp", 1f), RunSnapshotCodec.Read(state, "_moveSpeed", 1f),
                    RunSnapshotCodec.Read(state, "_xpReward", 1f), entity.variant, bossType, RunSnapshotCodec.Read(state, "_isElite", false),
                    RunSnapshotCodec.Read(state, "_archetype", EnemyArchetype.Normal), RunSnapshotCodec.Read(state, "_isPhaseRole", false),
                    RunSnapshotCodec.Read(state, "_role", "grunt"), RunSnapshotCodec.Read(state, "_roleDamageMultiplier", 1f),
                    RunSnapshotCodec.Read(state, "_roleRadiusMultiplier", 1f), RunSnapshotCodec.Read(state, "_eliteVariant", ""), RunSnapshotCodec.Read(state, "_damage", 1f));
                enemy.SetSourceAppearance(baseColor);
                if (entity.auxiliary != null)
                {
                    var boss = go.AddComponent<BossCombatAgent2D>();
                    boss.Initialize(this, _player, enemy, bossType, _wave, enemy.SourceSpeed * SpherePlacementRules.SourcePixelsToWorld, enemy.ContactDamage, baseColor);
                    RunSnapshotCodec.Restore(entity.auxiliary, typeof(BossCombatAgent2D), boss);
                    enemy.SetBossCombat(boss);
                }
                _enemies.Add(enemy); RegisterEnemyQuery(enemy); agent = enemy;
            }
            else if (entity.kind == nameof(ProjectileAgent))
                agent = SpawnProjectile(entity.position, RunSnapshotCodec.Read(state, "_direction", Vector2.right),
                    RunSnapshotCodec.Read(state, "_damage", 1f), RunSnapshotCodec.Read(state, "_impactColor", Color.white), RunSnapshotCodec.Read(state, "_speed", 8f));
            else
            {
                var go = new GameObject(entity.name); go.transform.position = entity.position;
                if (entity.kind == nameof(BossProjectileAgent2D))
                {
                    var bullet = go.AddComponent<BossProjectileAgent2D>();
                    bullet.Initialize(this, _player, RunSnapshotCodec.Read(state, "_velocity", Vector2.zero),
                        Mathf.Max(0f, RunSnapshotCodec.Read(state, "_hitRadius", 0.1f) - BossCombatRules.PlayerRadiusWorld) * BossCombatRules.SourceUnitsPerWorldUnit,
                        RunSnapshotCodec.Read(state, "_damage", 1f), RunSnapshotCodec.Read(state, "_bossType", ""), RunSnapshotCodec.Read(state, "_color", Color.white));
                    if (entity.hasProjectileOwner && entity.projectileOwnerIndex >= 0 && entity.projectileOwnerIndex < _enemies.Count) bullet.SetSourceOwner(_enemies[entity.projectileOwnerIndex]);
                    agent = bullet;
                }
                else if (entity.kind == nameof(SourceParticle2D))
                { var particle = go.AddComponent<SourceParticle2D>(); particle.Initialize(this, Vector2.zero, 1f, entity.color); agent = particle; }
                else if (entity.kind == nameof(StellaChestAgent2D))
                { var chest = go.AddComponent<StellaChestAgent2D>(); chest.Initialize(this, entity.position); agent = chest; }
                else
                {
                    if (entity.kind == nameof(ExperienceOrbAgent))
                    { var amount = RunSnapshotCodec.Read(state, "_amount", 1); var orb = go.AddComponent<ExperienceOrbAgent>(); orb.Initialize(this, _player, amount); agent = orb; }
                    else if (entity.kind == nameof(HealthPackAgent2D))
                    { var health = go.AddComponent<HealthPackAgent2D>(); health.Initialize(this); agent = health; }
                    else if (entity.kind == nameof(ArtifactChestAgent2D))
                    { var chest = go.AddComponent<ArtifactChestAgent2D>(); chest.Initialize(this); agent = chest; }
                    else if (entity.kind == nameof(RuneAgent2D))
                    { var rune = go.AddComponent<RuneAgent2D>(); rune.Initialize(this, RunSnapshotCodec.Read(state, "_type", "overdrive"), entity.position, RunSnapshotCodec.Read(state, "_life", 22f)); agent = rune; }
                    else if (entity.kind == nameof(AbilityDroneAgent2D))
                    {
                        var drone = go.AddComponent<AbilityDroneAgent2D>();
                        drone.Initialize(this, RunSnapshotCodec.Read(state, "_anchorType", SphereId.Standard), GetAbilityLevel(RunUpgradeKind.Minion),
                            RunSnapshotCodec.Read(state, "_remaining", 1f), RunSnapshotCodec.Read(state, "_branch", ""), RunSnapshotCodec.Read(state, "_final", ""), RunSnapshotCodec.Read(state, "_ordinal", 0));
                        _abilityDrones.Add(drone); agent = drone;
                    }
                }
            }
            if (agent == null) throw new FormatException("Unknown run entity " + entity.kind);
            RunSnapshotCodec.Restore(state, agent.GetType(), agent);
            if (agent is IRunSaveColor restoredAppearance) restoredAppearance.RestoreSavedColor(entity.color);
            if (agent is ProjectileAgent projectile) projectile.ResetRestoredSourceReferences();
            agent.transform.position = entity.position; agent.transform.localScale = entity.scale;
            if(agent is EnemyAgent2D restoredEnemy)RefreshEnemyQuery(restoredEnemy);
        }
    }
}
