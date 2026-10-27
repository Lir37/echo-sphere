using EchoSphere;
using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoSphere.Runtime
{
    /// <summary>First-playable migration harness; intentionally not the full production game.</summary>
    [DefaultExecutionOrder(-100)]
    public sealed partial class EchoSphereRuntime : MonoBehaviour
    {
        [System.Serializable]
        private sealed class ShopUpgradeData
        {
            public int dmg, radius, speed, spheres, hp, crit, xp;
            public int Get(string id)
            {
                switch (id) { case "dmg": return dmg; case "radius": return radius; case "speed": return speed; case "spheres": return spheres; case "hp": return hp; case "crit": return crit; case "xp": return xp; default: return 0; }
            }
            public void Set(string id, int level)
            {
                switch (id) { case "dmg": dmg = level; break; case "radius": radius = level; break; case "speed": speed = level; break; case "spheres": spheres = level; break; case "hp": hp = level; break; case "crit": crit = level; break; case "xp": xp = level; break; }
            }
            public void Clamp()
            {
                for (var i = 0; i < PersistentUpgradeRules.Ids.Length; i++)
                {
                    var id = PersistentUpgradeRules.Ids[i];
                    Set(id, PersistentUpgradeRules.ClampLevel(id, Get(id)));
                }
            }
        }
        [System.Serializable]
        private sealed class ShopSaveData { public ShopUpgradeData upgrades = new ShopUpgradeData(); }

        private sealed class FractalFormationSnapshot
        {
            public SphereNetworkFormation Type;
            public readonly List<Vector2> Nodes = new List<Vector2>();
        }

        [System.Serializable]
        private sealed class LeaderboardEntry
        {
            public string name;
            public float time;
            public int wave;
            public long date;
        }

        [System.Serializable]
        private sealed class LeaderboardSaveData { public List<LeaderboardEntry> entries = new List<LeaderboardEntry>(); }
        [System.Serializable]
        private sealed class AchievementSaveData { public List<string> ids = new List<string>(); }

        private sealed class ActiveAbilitySynergyLink
        {
            public readonly string Id;
            public readonly SphereId Sphere;
            public readonly string SphereBranch;
            public readonly RunUpgradeKind Ability;
            public readonly string AbilityFinal;

            public ActiveAbilitySynergyLink(string id, SphereId sphere, string sphereBranch, RunUpgradeKind ability, string abilityFinal)
            {
                Id = id;
                Sphere = sphere;
                SphereBranch = sphereBranch;
                Ability = ability;
                AbilityFinal = abilityFinal;
            }
        }

        private static readonly ActiveAbilitySynergyLink[] ActiveAbilitySynergyLinks =
        {
            new ActiveAbilitySynergyLink("std_res_blast", SphereId.Standard, "standard_resonator", RunUpgradeKind.Blast, "blast_resonance_f1"),
            new ActiveAbilitySynergyLink("std_singularity_timestop", SphereId.Standard, "standard_singularity", RunUpgradeKind.TimeStop, "timestop_echo_phase_f1"),
            new ActiveAbilitySynergyLink("std_swarm_minion", SphereId.Standard, "standard_swarm", RunUpgradeKind.Minion, "minion_echo_drone_f1"),
            new ActiveAbilitySynergyLink("sniper_oracle_teleport", SphereId.Sniper, "sniper_oracle", RunUpgradeKind.Teleport, "teleport_echo_jump_f1"),
            new ActiveAbilitySynergyLink("sniper_assassin_darkritual", SphereId.Sniper, "sniper_assassin", RunUpgradeKind.DarkRitual, "darkritual_blood_link_f1"),
            new ActiveAbilitySynergyLink("sniper_beacon_firetrail", SphereId.Sniper, "sniper_beacon", RunUpgradeKind.FireTrail, "firetrail_overdrive_f1"),
            new ActiveAbilitySynergyLink("shotgun_burst_shield", SphereId.Shotgun, "shotgun_burst", RunUpgradeKind.Shield, "shield_echo_guard_f1"),
            new ActiveAbilitySynergyLink("shotgun_cataclysm_blast", SphereId.Shotgun, "shotgun_cataclysm", RunUpgradeKind.Blast, "blast_network_f2"),
            new ActiveAbilitySynergyLink("shotgun_hail_lightning", SphereId.Shotgun, "shotgun_hail", RunUpgradeKind.Lightning, "lightning_echo_storm_f1"),
            new ActiveAbilitySynergyLink("chain_web_timestop", SphereId.Chain, "chain_web", RunUpgradeKind.TimeStop, "timestop_closed_time_f2"),
            new ActiveAbilitySynergyLink("chain_storm_lightning", SphereId.Chain, "chain_storm", RunUpgradeKind.Lightning, "lightning_relay_f2"),
            new ActiveAbilitySynergyLink("chain_leech_darkritual", SphereId.Chain, "chain_leech", RunUpgradeKind.DarkRitual, "darkritual_sacrifice_f2"),
            new ActiveAbilitySynergyLink("aura_sanctum_shield", SphereId.Aura, "aura_sanctum", RunUpgradeKind.Shield, "shield_reflector_f2"),
            new ActiveAbilitySynergyLink("aura_gravity_teleport", SphereId.Aura, "aura_gravity", RunUpgradeKind.Teleport, "teleport_beacon_f2"),
            new ActiveAbilitySynergyLink("aura_overgrowth_firetrail", SphereId.Aura, "aura_overgrowth", RunUpgradeKind.FireTrail, "firetrail_sanctum_f3"),
            new ActiveAbilitySynergyLink("orbital_dance_minion", SphereId.Orbital, "orbital_dance", RunUpgradeKind.Minion, "minion_relay_drone_f2"),
            new ActiveAbilitySynergyLink("orbital_halo_shield", SphereId.Orbital, "orbital_halo", RunUpgradeKind.Shield, "shield_bastion_f3"),
            new ActiveAbilitySynergyLink("orbital_blade_darkritual", SphereId.Orbital, "orbital_blade", RunUpgradeKind.DarkRitual, "darkritual_void_pact_f3"),
            new ActiveAbilitySynergyLink("prism_split_blast", SphereId.Prism, "prism_split", RunUpgradeKind.Blast, "blast_core_f3"),
            new ActiveAbilitySynergyLink("prism_spectrum_firetrail", SphereId.Prism, "prism_spectrum", RunUpgradeKind.FireTrail, "firetrail_ignition_f2"),
            new ActiveAbilitySynergyLink("prism_mirror_teleport", SphereId.Prism, "prism_mirror", RunUpgradeKind.Teleport, "teleport_phase_f3"),
            new ActiveAbilitySynergyLink("gravity_well_firetrail", SphereId.Gravity, "gravity_well", RunUpgradeKind.FireTrail, "firetrail_overdrive_f3"),
            new ActiveAbilitySynergyLink("gravity_tide_timestop", SphereId.Gravity, "gravity_tide", RunUpgradeKind.TimeStop, "timestop_time_anchor_f3"),
            new ActiveAbilitySynergyLink("gravity_collapse_blast", SphereId.Gravity, "gravity_collapse", RunUpgradeKind.Blast, "blast_network_f1"),
            new ActiveAbilitySynergyLink("pulse_wave_blast", SphereId.Pulse, "pulse_wave", RunUpgradeKind.Blast, "blast_core_f2"),
            new ActiveAbilitySynergyLink("pulse_resonator_lightning", SphereId.Pulse, "pulse_resonator", RunUpgradeKind.Lightning, "lightning_echo_storm_f3"),
            new ActiveAbilitySynergyLink("pulse_burst_shield", SphereId.Pulse, "pulse_burst", RunUpgradeKind.Shield, "shield_echo_guard_f2"),
            new ActiveAbilitySynergyLink("void_hunger_darkritual", SphereId.Void, "void_hunger", RunUpgradeKind.DarkRitual, "darkritual_blood_link_f2"),
            new ActiveAbilitySynergyLink("void_reaper_minion", SphereId.Void, "void_reaper", RunUpgradeKind.Minion, "minion_guardian_f3"),
            new ActiveAbilitySynergyLink("void_execution_teleport", SphereId.Void, "void_execution", RunUpgradeKind.Teleport, "teleport_echo_jump_f2")
        };

        [System.Serializable]
        private sealed class CharacterProfileData
        {
            public string id;
            public int masteryLevel = 1;
            public int masteryXp;
            public bool unlocked;
        }

        [System.Serializable]
        private sealed class CharacterProfilesSaveData { public List<CharacterProfileData> profiles = new List<CharacterProfileData>(); }

        private sealed class AchievementDefinition
        {
            public readonly string Id;
            public readonly string NameRu;
            public readonly string NameEn;
            public readonly string DescriptionRu;
            public readonly string DescriptionEn;

            public AchievementDefinition(string id, string nameRu, string nameEn, string descriptionRu, string descriptionEn)
            {
                Id = id;
                NameRu = nameRu;
                NameEn = nameEn;
                DescriptionRu = descriptionRu;
                DescriptionEn = descriptionEn;
            }
        }

        private enum MainMenuPage { Home, Shop, Settings, Leaderboard, Achievements, Characters, Knowledge }

        private static readonly AchievementDefinition[] SourceAchievements =
        {
            new AchievementDefinition("kills_500", "Палач", "Executioner", "Убить 500 врагов", "Kill 500 enemies"),
            new AchievementDefinition("kills_1000", "Мясник", "Butcher", "Убить 1000 врагов", "Kill 1000 enemies"),
            new AchievementDefinition("wave_30", "Выживший", "Survivor", "Достичь волны 30", "Reach wave 30"),
            new AchievementDefinition("wave_50", "Легенда", "Legend", "Достичь волны 50", "Reach wave 50"),
            new AchievementDefinition("boss_5", "Охотник", "Hunter", "Убить 5 боссов", "Kill 5 bosses"),
            new AchievementDefinition("boss_10", "Герой", "Hero", "Убить 10 боссов", "Kill 10 bosses"),
            new AchievementDefinition("evolve_1", "Эволюция", "Evolution", "Эволюционировать способность", "Evolve an ability"),
            new AchievementDefinition("evolve_3", "Совершенство", "Perfection", "3 эволюции за забег", "3 evolutions in one run"),
            new AchievementDefinition("elite_10", "Элитный охотник", "Elite Hunter", "Убить 10 элитных врагов", "Kill 10 elite enemies"),
            new AchievementDefinition("combo_50", "Мастер комбо", "Combo Master", "Комбо x50", "Reach combo x50"),
            new AchievementDefinition("chest_5", "Сокровищница", "Treasure Hunter", "Открыть 5 сундуков", "Open 5 chests"),
            new AchievementDefinition("dash_50", "Призрак", "Phantom", "50 рывков за забег", "50 dashes in one run")
        };

        private const string ShopPreferenceKey = "echosphere_shop";
        private const string GoldPreferenceKey = "echosphere_gold";
        private const string DifficultyPreferenceKey = "echosphere_difficulty";
        private const string LanguagePreferenceKey = "echosphere_lang";
        private const string NamePreferenceKey = "echosphere_name";
        private const string LeaderboardPreferenceKey = "echosphere_leaderboard";
        private const string AchievementPreferenceKey = "echosphere_achievements";
        private const string CharacterPreferenceKey = "echosphere_character";
        private const string CharacterProfilesPreferenceKey = "echosphere_character_profiles";
        private const string ArtifactProtocolKnowledgePreferenceKey = "echosphere_artifact_protocol_knowledge_v1";
        private const string SoundPreferenceKey = "echosphere_sound";
        private const string HandednessPreferenceKey = "echosphere_handedness";
        private const string TutorialCompletedPreferenceKey = "echosphere_tutorial_completed_v1";
        private int _tutorialStep = -1;
        private Vector3 _tutorialStartPosition;
        private bool _tutorialUpgradeOpened;
        private bool IsLeftHanded => PlayerPrefs.GetString(HandednessPreferenceKey, "right") == "left";
        private ShopSaveData _shopSave;
        private bool _shopSaveLoaded;
        private MainMenuPage _mainMenuPage = MainMenuPage.Home;
        private readonly HashSet<string> _knownArtifactProtocolIds = new HashSet<string>();
        private bool _artifactProtocolKnowledgeLoaded;
        private bool _artifactPanelOpen;
        private Vector2 _artifactProtocolScrollPosition;
        private Vector2 _knowledgeScrollPosition;
        private string _playerName = "";
        private bool _settingsResetPending;
        private bool _architectFormationTracked;
        private SphereNetworkFormation _architectFormationType = SphereNetworkFormation.None;
        private float _architectFormationChangedAt = float.NegativeInfinity;
        private EnemyAgent2D _hunterMarkTarget;
        private EnemyAgent2D _hunterHuntTarget;
        private float _hunterMarkTimer;
        private float _hunterHuntTimer;
        private float _hunterTrophyTimer;
        private float _hunterScreenShakeIntensity;
        private Vector3 _lastHunterScreenShakeOffset;
        private int _hunterHitCount;
        private readonly List<FractalFormationSnapshot> _fractalFormationHistory = new List<FractalFormationSnapshot>();
        private readonly List<Vector2> _fractalEchoNodes = new List<Vector2>();
        private float _fractalEchoTimer, _fractalEchoPulseTimer, _fractalEchoCooldown, _fractalEchoStrength;
        private int _spheristChorusHits, _berserkerRedlineHits;
        private float _characterScreenShakeIntensity;
        private SphereAttackAgent _voidPhantomSource;
        private Vector2 _voidPhantomPosition;
        private float _voidPhantomTimer, _voidPhantomPulseTimer;
        private int _voidPhantomPulseCount;
        private SphereAttackAgent _engineerRelaySource;
        private float _engineerRelayTimer;
        private Vector2 _characterScrollPosition;
        private string _expandedCharacterId = "spherist";
        private List<CharacterProfileData> _characterProfiles = new List<CharacterProfileData>();
        private bool _characterProfilesLoaded;
        private bool _runResultSaved;
        private int _runGoldEarned;
        private float _characterMasteryRunXp;
        private float _characterMasteryLastTime = -1f;
        private int _characterMasteryLastEliteKills;
        private int _characterMasteryLastBosses;
        private int _characterMasteryLastResonanceEvents;
        private int _runEliteKills;
        private int _runEvolutions;
        private int _runChestOpens;
        private int _combo;
        private float _comboTimer;
        private float _comboMultiplier = 1f;
        private List<LeaderboardEntry> _leaderboardEntries = new List<LeaderboardEntry>();
        private List<string> _unlockedAchievementIds = new List<string>();

        // Exact picker order from Object.keys(SPHERE_TYPES) in new-desing/src/gameData.ts.
        private static readonly SphereId[] SpherePickerOrder =
        {
            SphereId.Standard, SphereId.Sniper, SphereId.Shotgun, SphereId.Chain, SphereId.Aura,
            SphereId.Orbital, SphereId.Prism, SphereId.Gravity, SphereId.Pulse, SphereId.Void
        };

        private readonly SourceEntityList<EnemyAgent2D> _enemies = new SourceEntityList<EnemyAgent2D>();
        private readonly HashSet<string> _regionLoreDiscovered = new HashSet<string>();
        private int _regionXpReward, _regionGoldReward, _regionChallengeContractsCompleted, _regionMasteryChallengesCompleted;
        private readonly HashSet<SphereNetworkFormation> _regionMasteryGeometryTypes = new HashSet<SphereNetworkFormation>();
        private float _regionMasteryDominantSeconds, _regionDamageTaken, _regionLastHp = -1f;
        private readonly List<SphereAttackAgent> _spheres = new List<SphereAttackAgent>();
        private readonly List<AbilityDroneAgent2D> _abilityDrones = new List<AbilityDroneAgent2D>();
        private readonly List<string> _ownedArtifacts = new List<string>();
        private readonly List<ArtifactRewardDefinition> _stellaRewardChoices = new List<ArtifactRewardDefinition>();
        private readonly HashSet<SphereId> _artifactSphereTypeScratch = new HashSet<SphereId>();
        private readonly Dictionary<SphereId, int> _sphereLevels = new Dictionary<SphereId, int>();
        private readonly List<RunUpgradeChoice> _levelUpChoices = new List<RunUpgradeChoice>();
        private readonly Dictionary<RunUpgradeKind, int> _abilityLevels = new Dictionary<RunUpgradeKind, int>();
        private readonly Dictionary<RunUpgradeKind, KeyCode> _activeAbilityHotkeys = new Dictionary<RunUpgradeKind, KeyCode>();
        private static readonly KeyCode[] SourceActiveAbilityKeys = { KeyCode.E, KeyCode.Q, KeyCode.R, KeyCode.F, KeyCode.G };
        private int _activeAbilitySlots;
        private float _teleportCooldown, _timeStopCooldown;
        private float _blastCooldown, _shieldCooldown, _fireTrailCooldown, _minionCooldown, _lightningCooldown, _darkRitualCooldown;
        private float _fireTrailTimer, _fireCatalystTimer, _darkRitualOverloadTimer, _shieldTimer, _shieldVisualPulse;
        private bool _applyingActiveAbilitySynergyRiders;
        private float _activeAbilitySynergyPower = 1f;

        private readonly List<string> _recentUpgradeChoiceKeys = new List<string>();
        private readonly List<string> _oracleForecastKeys = new List<string>();
        private int _levelUpAbilityPity, _levelUpSpherePity;
        private int _levelUpRerollsRemaining = 1;
        private int _levelUpLocksRemaining = 1;
        private string _levelUpLockChoiceKey;
        private bool _oracleForecastRerollUsed;
        private string _oracleSourceFlashText = string.Empty;
        private float _oracleSourceFlashTimer;
        private readonly Dictionary<SphereId, string> _sphereBranches = new Dictionary<SphereId, string>();
        private readonly Dictionary<SphereId, string> _sphereFinals = new Dictionary<SphereId, string>();
        private readonly List<SphereEvolutionOption> _evolutionChoices = new List<SphereEvolutionOption>();
        private readonly Dictionary<RunUpgradeKind, string> _abilityBranches = new Dictionary<RunUpgradeKind, string>();
        private readonly Dictionary<RunUpgradeKind, string> _abilityFinals = new Dictionary<RunUpgradeKind, string>();
        private RunUpgradeKind _pendingEvolutionAbility;
        private bool _pendingEvolutionIsAbility;
        private SphereId _pendingEvolutionSphere;
        private bool _choosingEvolution, _pendingFinalEvolution;
        private readonly SeededRng _rng = new SeededRng(20261009u);
        private Camera _camera;
        private Transform _player;
        private PlayerCoreController2D _playerCore;
        private Vector2 _touchStart;
        private SphereId _selectedSphereType = SphereId.Standard;
        // Current Unity character path starts as the source default Spherist.
        // Character selection/mastery progression is not yet exposed by this runtime.
        private SphereCharacterId _sphereCharacterId = SphereCharacterId.Spherist;
        private int _characterMasteryLevel = 1;
        private int _maxSphereAbilityLevel;
        private bool _sphereMovementLocked;
        private bool _spherePointerActive, _spherePointerFromTouch, _spherePointerMoved;
        private bool _spherePointerIsMovement, _spherePointerIsPlacing;
        private int _spherePointerTouchId = -1;
        private Vector2 _spherePointerStartScreen, _spherePointerWorld;
        private SphereAttackAgent _spherePointerTarget;
        private bool _spherePointerPositionValid;
        private SphereNetworkState _spherePointerPreview;
        private readonly List<Vec2> _spherePointerPreviewPositions = new List<Vec2>();
        private int _spherePointerPreviewIndex = -1;
        private SphereNetworkFormation _spherePointerOriginalFormation = SphereNetworkFormation.None;
        private int[] _spherePointerOriginalNodeIndexes;
        private Vector2[] _spherePointerOriginalFormationPoints;
        private Vector2[] _formationMemoryPoints;
        private SphereNetworkFormation _formationMemoryType = SphereNetworkFormation.None;
        private float _formationMemoryTimer, _placementFlashTimer;

        private readonly List<int> _resonanceGeometryNodeIndexes = new List<int>();
        private readonly List<string> _resonanceGeometryCandidateKeys = new List<string>();
        private readonly List<Vector2> _resonanceGeometryMemoryPoints = new List<Vector2>();
        private string _resonanceGeometryKey = "none";
        private SphereNetworkFormation _resonanceGeometryType = SphereNetworkFormation.None;
        private SphereNetworkFormation _resonanceGeometryMemoryType = SphereNetworkFormation.None;
        private float _resonanceGeometryMemoryTimer;
        private Vector2 _placementFlashPosition;
        private Color _placementFlashColor = Color.white;
        private float _spawnTimer = 3f, _runTime, _messageTimer, _formationStrain;
        private Vec2 _lastDirection = Vec2.Zero;
        private int _kills, _level = 1, _xpRequired = RunBalanceRules.GetXpToNextLevel(1);
        private float _xp;
        private int _bossesDefeated, _wave, _waveEnemiesToSpawn;
        private bool _follow, _paused, _levelUp, _gameOver, _userPaused, _regionStabilized;
        private bool _stellaRewardPending, _stellaRewardIsLegendary;
        private float _swiftBootsTimer;
        // Source player.timestopTimer / teleportDamageBuffTimer. Time Stop now produces
        // its timer; the Teleport buff remains branch/final-specific and gated until those
        // ability evolution choices are ported. Artifact bonuses remain independently gated.
        private float _timeStopTimer, _teleportDamageBuffTimer;
        // Exact source PlayerState.sphereMods keys and initial values from engineState.ts.
        // rotationSpeed starts at 1 in the source, so source Object.values(...).some(level > 0)
        // is true even before a modifier upgrade is learned.
        private readonly Dictionary<string, int> _sphereMods = new Dictionary<string, int>
        {
            { "multishot", 0 }, { "pierce", 0 }, { "ricochet", 0 },
            { "fire", 0 }, { "freeze", 0 }, { "poison", 0 },
            { "breach", 0 }, { "overload", 0 }, { "split", 0 },
            { "shatter", 0 }, { "execute", 0 }, { "mark", 0 },
            { "echo", 0 }, { "anchor", 0 }, { "phase", 0 },
            { "static", 0 }, { "resonant", 0 }, { "magnetic", 0 },
            { "vampiric", 0 }, { "corrupt", 0 }, { "drain", 0 },
            { "afterimage", 0 }, { "impact", 0 }, { "gravitic", 0 },
            { "rotationSpeed", 1 }
        };
        private float _alchemistCatalystTimer;
        private float _conductorOverdriveTimer;
        private int _alchemistReactionCount;
        private RegionBossScheduleState _regionSchedule;
        private RegionFieldVisuals _regionFieldVisuals;
        private string _regionHudPocketId = "";
        private float _regionPocketNoticeTimer;
        private string _regionPocketNotice = "";

        private bool _mainMenu = true;
        private bool _regionSelect;
        private EchoRegionId _selectedRegion = EchoRegionId.ResonanceBasin;
        private string _regionChallengeId = "none";
        private bool _regionEndlessMode;
        private float _endlessNextBossAt;
        private int _endlessBossesSpawned, _fractureIntervalIndex;
        private const string BasinStabilizedPreferenceKey = "echo-sphere:region:resonance_basin:stabilized";
        private const string SelectedRegionPreferenceKey = "echo-sphere:region:selected";
        private const string RegionChallengePreferencePrefix = "echo-sphere:region:";
        private const string RegionEndlessPreferenceSuffix = ":endless";
        private float _uiScale = 1f, _uiWidth, _uiHeight;
        private Texture2D _uiPanelTexture, _uiButtonTexture, _uiButtonHoverTexture, _uiButtonActiveTexture, _uiAccentTexture;
        private GUIStyle _menuTitle, _comboTitleStyle, _comboMultiplierStyle;
        private float _damageMultiplier = 1f, _attackSpeedMultiplier = 1f;
        private float _resonanceCharge, _resonanceRingTimer, _resonanceRingPulseTimer;
        private int _resonanceEventsTriggered, _resonanceRingCursor, _resonanceLineBurst;
        private bool _resonanceEventActive;
        private readonly HashSet<string> _resonanceKnownFormationKeys = new HashSet<string>();
        private readonly List<int> _resonanceRingNodes = new List<int>();
        private string _message = "RIGHT HALF: MOVE. LEFT HALF: PLACE / DRAG SPHERES. TAP SPHERE: REMOVE.";
        private GUIStyle _box, _label, _button, _title, _sphereDescriptionStyle;
        private readonly List<LineRenderer> _networkGlowRenderers = new List<LineRenderer>();
        private readonly List<LineRenderer> _networkLinkRenderers = new List<LineRenderer>();
        private readonly List<LineRenderer> _networkHighlightRenderers = new List<LineRenderer>();
        private readonly List<NetworkCometVisual> _networkComets = new List<NetworkCometVisual>();
        private Material _networkLineMaterial;

        private sealed class NetworkCometVisual
        {
            public SpriteRenderer Head;
            public SpriteRenderer Tail;
        }
        private SphereNetworkState _networkState;
        private SphereNetworkFormation _previousDominantFormation = SphereNetworkFormation.None;
        private SphereNetworkFormation _preferredDominantFormation = SphereNetworkFormation.None;
        private SphereNetworkFormation _preferredSecondaryFormation = SphereNetworkFormation.None;

        public bool IsGameplayPaused => _mainMenu || (_paused && !_simulationFrame) || (!_simulationFrame && _playerCore != null && _playerCore.IsDead);
        public bool IsPresentationPaused => _mainMenu || _paused;
        public Vector2 PlayerPosition => _player == null ? Vector2.zero : (Vector2)_player.position;
        public bool IsDarkRitualOverloaded => _darkRitualOverloadTimer > 0f;
        public bool IsCombatSimulationPaused => IsGameplayPaused || _playerCore == null;
        public float CurrentRunTime => _runTime;
        public int CurrentWave => _wave;
        public float GetEnemyAbilitySpeedMultiplier(float distanceToPlayer, float statusSlowMultiplier)
        {
            var abilitySlow = LevelUpAbilityRules.GetEnemySlowMultiplier(GetAbilityLevel(RunUpgradeKind.Slow));
            var slowRadius = 300f * SpherePlacementRules.SourcePixelsToWorld;
            return CombatRules.GetEnemySpeedMultiplier(statusSlowMultiplier, abilitySlow, distanceToPlayer, slowRadius);
        }
        public float GetExperienceMagnetRadiusWorld() => PlayerLoopRules.MagnetRadius(GetAbilityLevel(RunUpgradeKind.Magnet), GetGlobalSphereModifierLevel("magnetic")) * SpherePlacementRules.SourcePixelsToWorld;
        public bool RegionStabilized => _regionStabilized;
        public EchoRegionId SelectedRegion => _selectedRegion;
        public void RegisterRegionPoiCollected(string poiType)
        {
            if (string.IsNullOrEmpty(poiType)) return;
            _regionLoreDiscovered.Add(poiType);
            var radiusScale = SpherePlacementRules.SourcePixelsToWorld;
            switch (poiType)
            {
                case "resonance_cache":
                    _resonanceCharge = Mathf.Min(100f, _resonanceCharge + 28f);
                    ShowTimedCombatMessage(IsRussianUi ? "ТАЙНИК РЕЗОНАНСА · +28 РЕЗОНАНСА" : "RESONANCE CACHE · +28 RESONANCE", 1.35f);
                    break;
                case "echo_relay":
                    _resonanceCharge = Mathf.Min(100f, _resonanceCharge + 45f);
                    ShowTimedCombatMessage(IsRussianUi ? "РЕЛЕ ЭХА · +45 РЕЗОНАНСА" : "ECHO RELAY · +45 RESONANCE", 1.35f);
                    break;
                case "breach_node":
                    ScaleLivingEnemiesNearPlayer(260f * radiusScale, 0.72f);
                    ShowTimedCombatMessage(IsRussianUi ? "УЗЕЛ ПРОБОЯ · ВРАГИ -28% HP" : "BREACH NODE · ENEMIES -28% HP", 1.35f);
                    break;
                case "rupture":
                    ScaleLivingEnemiesNearPlayer(220f * radiusScale, 0.62f);
                    ShowTimedCombatMessage(IsRussianUi ? "РАЗРЫВ · ВРАГИ -38% HP" : "RUPTURE · ENEMIES -38% HP", 1.35f);
                    break;
                case "lost_signal":
                    _regionXpReward += 12;
                    ShowTimedCombatMessage(IsRussianUi ? "ПОТЕРЯННЫЙ СИГНАЛ · +12 XP РЕГИОНА" : "LOST SIGNAL · +12 REGION XP", 1.35f);
                    break;
                case "elite_nest":
                    ShowTimedCombatMessage(IsRussianUi ? "ГНЕЗДО ЭЛИТЫ · 2 ЭЛИТНЫХ ВРАГА" : "ELITE NEST · 2 ELITE HOSTILES", 1.35f);
                    SpawnEnemy();
                    SpawnEnemy();
                    break;
                case "boss_trace":
                    ShowTimedCombatMessage(IsRussianUi ? "СЛЕД БОССА · БЛИЗКИЙ СТРАЖ" : "BOSS TRACE · NEAR WARDEN", 1.35f);
                    break;
            }
        }

        private void ScaleLivingEnemiesNearPlayer(float radius, float multiplier)
        {
            var center = PlayerPosition;
            for (var i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive ||
                    Vector2.Distance(enemy.transform.position, center) >= radius) continue;
                enemy.ScaleCurrentHp(multiplier);
            }
        }
        public bool IsRegionAvailable(EchoRegionId region) =>
            RegionUnlockRules.IsAvailable(region, PlayerPrefs.GetInt(BasinStabilizedPreferenceKey, 0) == 1);
        public float GetRegionRepulsionMultiplier()
        {
            if (_regionChallengeId == "low_gravity") return 0.55f;
            if (_selectedRegion == EchoRegionId.SpectralRift && _regionChallengeId == "phase_drift") return 0.62f;
            return 1f;
        }
        private static string RegionSourceId(EchoRegionId region) =>
            region == EchoRegionId.SpectralRift ? "spectral_rift" : "resonance_basin";
        private static string RegionStabilizedKey(EchoRegionId region) =>
            RegionChallengePreferencePrefix + RegionSourceId(region) + ":stabilized";
        private static string RegionChallengesKey(EchoRegionId region) =>
            RegionChallengePreferencePrefix + RegionSourceId(region) + ":challenges";
        private static string RegionEndlessKey(EchoRegionId region) =>
            RegionChallengePreferencePrefix + RegionSourceId(region) + RegionEndlessPreferenceSuffix;
        private bool IsRegionStabilized(EchoRegionId region) =>
            PlayerPrefs.GetInt(RegionStabilizedKey(region), region == EchoRegionId.ResonanceBasin &&
                PlayerPrefs.GetInt(BasinStabilizedPreferenceKey, 0) == 1 ? 1 : 0) == 1;
        private string[] GetCompletedRegionChallenges(EchoRegionId region)
        {
            var raw = PlayerPrefs.GetString(RegionChallengesKey(region), string.Empty);
            if (string.IsNullOrEmpty(raw)) return Array.Empty<string>();
            var valid = region == EchoRegionId.ResonanceBasin
                ? new[] { "fractured_network", "overload", "low_gravity" }
                : new[] { "mirror_tide", "volatile_core", "phase_drift" };
            var result = new List<string>();
            foreach (var id in raw.Split(','))
                if (Array.IndexOf(valid, id) >= 0 && !result.Contains(id)) result.Add(id);
            return result.ToArray();
        }
        private bool IsRegionEndlessUnlocked(EchoRegionId region) =>
            GetCompletedRegionChallenges(region).Length >= 3 &&
            PlayerPrefs.GetInt(RegionEndlessKey(region), 0) == 1;
        private void MarkRegionChallengeComplete()
        {
            if (_regionChallengeId == "none" || _regionEndlessMode) return;
            var completed = new List<string>(GetCompletedRegionChallenges(_selectedRegion));
            if (!completed.Contains(_regionChallengeId)) completed.Add(_regionChallengeId);
            PlayerPrefs.SetString(RegionChallengesKey(_selectedRegion), string.Join(",", completed));
            if (completed.Count >= 3) PlayerPrefs.SetInt(RegionEndlessKey(_selectedRegion), 1);
            PlayerPrefs.Save();
        }
        private float GetChallengeEnemyHpMultiplier()
        {
            if (_selectedRegion == EchoRegionId.ResonanceBasin && _regionChallengeId == "overload") return 1.18f;
            if (_selectedRegion == EchoRegionId.SpectralRift && _regionChallengeId == "mirror_tide")
                return Mathf.FloorToInt(_runTime / 36f) % 2 == 0 ? 0.94f : 1.18f;
            if (_selectedRegion == EchoRegionId.SpectralRift && _regionChallengeId == "volatile_core" && _resonanceCharge >= 70f) return 1.12f;
            return 1f;
        }
        private float GetChallengeEnemySpeedMultiplier()
        {
            if (_selectedRegion == EchoRegionId.ResonanceBasin && _regionChallengeId == "overload") return 1.12f;
            if (_selectedRegion == EchoRegionId.SpectralRift && _regionChallengeId == "mirror_tide")
                return Mathf.FloorToInt(_runTime / 36f) % 2 == 0 ? 1.20f : 0.88f;
            if (_selectedRegion == EchoRegionId.SpectralRift && _regionChallengeId == "volatile_core" && _resonanceCharge >= 70f) return 1.06f;
            return 1f;
        }
        private float GetChallengeEnemyXpMultiplier()
        {
            if (_selectedRegion == EchoRegionId.ResonanceBasin && _regionChallengeId == "overload") return 1.15f;
            if (_selectedRegion == EchoRegionId.SpectralRift && _regionChallengeId == "mirror_tide")
                return Mathf.FloorToInt(_runTime / 36f) % 2 == 0 ? 1.08f : 1.20f;
            if (_selectedRegion == EchoRegionId.SpectralRift && _regionChallengeId == "volatile_core" && _resonanceCharge >= 70f) return 1.10f;
            return 1f;
        }
        public float GetRegionEnemyDamageMultiplier()
        {
            var pocketDamage = _player == null ? 1f :
                RegionPocketRules.GetAt(_selectedRegion, _player.position.x, -_player.position.y).Damage;
            if (_selectedRegion == EchoRegionId.ResonanceBasin && _regionChallengeId == "overload") return pocketDamage * 1.08f;
            if (_selectedRegion == EchoRegionId.SpectralRift && _regionChallengeId == "mirror_tide")
                return pocketDamage * (Mathf.FloorToInt(_runTime / 36f) % 2 == 0 ? 0.94f : 1.16f);
            if (_selectedRegion == EchoRegionId.SpectralRift && _regionChallengeId == "volatile_core" && _resonanceCharge >= 70f) return pocketDamage * 1.12f;
            return pocketDamage;
        }
        private float GetChallengeMovementMultiplier()
        {
            if (_regionChallengeId == "low_gravity") return 1.12f;
            if (_selectedRegion == EchoRegionId.SpectralRift &&
                (_regionChallengeId == "volatile_core" || _regionChallengeId == "phase_drift")) return 1.10f;
            return 1f;
        }
        public float NextRunRandom() => _rng.NextFloat();
        public int GetShopUpgradeLevel(string id) { EnsureShopSaveLoaded(); return _shopSave.upgrades.Get(id); }
        public float GetShopSphereRadiusMultiplier() => PersistentUpgradeRules.GetPercentMultiplier(GetShopUpgradeLevel("radius"));
        private DifficultyProfile CurrentDifficulty => DifficultyRules.GetProfile(PlayerPrefs.GetString(DifficultyPreferenceKey, "normal"));
        public float SourceBuffDamageMultiplier => _buffTimer > 0f ? 1.3f : 1f;
        public float SphereDamageMultiplier => _damageMultiplier * PlayerLoopRules.MutationDamage(_mutationStage) * ChaosOrbMultiplier("dmg") *
            (_teleportDamageBuffTimer > 0f ? 2f : 1f) *
            (_timeStopTimer > 0f && _timeStopTimer <= 1f && GetAbilityFinalArchetype(RunUpgradeKind.TimeStop) == "timestop_temporal_core" ? 2f : 1f) *

            (_darkRitualOverloadTimer > 0f ? 1.25f + GetAbilityLevel(RunUpgradeKind.DarkRitual) * 0.04f : 1f) *
            LevelUpAbilityRules.GetSphereDamageMultiplier(GetAbilityLevel(RunUpgradeKind.Damage)) *
            PersistentUpgradeRules.GetPercentMultiplier(GetShopUpgradeLevel("dmg")) *
            Mathf.Max(0.25f, 1f + ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "sphereDamage"));
        public float AttackSpeedMultiplier => _attackSpeedMultiplier * (_darkRitualOverloadTimer > 0f ? 1f / 0.72f : 1f) *
            LevelUpAbilityRules.GetAttackSpeedMultiplier(GetAbilityLevel(RunUpgradeKind.AttackSpeed)) *
            GetCharacterAttackSpeedMultiplier() /
            Mathf.Max(0.5f, 1f + ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "sphereDelay"));

        private bool IsFireAlignedSphere(SphereId type) =>
            Mathf.Max(GetGlobalSphereModifierLevel("fire"), GetSphereAuthoredModifierLevel(type, SphereAuthoredModifier.Fire)) > 0;

        public float GetSphereDamageMultiplier(SphereId type)
        {
            var multiplier = SphereDamageMultiplier;
            var fireAligned = IsFireAlignedSphere(type);
            if (fireAligned && _fireTrailTimer > 0f)
                multiplier *= 1.2f + GetAbilityLevel(RunUpgradeKind.FireTrail) * 0.025f;
            if (fireAligned && _fireCatalystTimer > 0f) multiplier *= 1.35f;
            if (fireAligned && GetAbilityBranch(RunUpgradeKind.FireTrail) == "firetrail_overdrive") multiplier *= 1.15f;
            return multiplier;
        }

        public float GetSphereAttackSpeedMultiplier(SphereId type)
        {
            var multiplier = AttackSpeedMultiplier;
            if (IsFireAlignedSphere(type) && _fireTrailTimer > 0f) multiplier *= 1f / 0.78f;
            return multiplier;
        }
        public float GetSphereRadiusMultiplier() => LevelUpAbilityRules.GetSphereRadiusMultiplier(GetAbilityLevel(RunUpgradeKind.Radius)) * PlayerLoopRules.MutationRadius(_mutationStage) * ChaosOrbMultiplier("radius");
        public float GetSphereRangeMultiplier() => LevelUpAbilityRules.GetSphereRangeMultiplier(GetAbilityLevel(RunUpgradeKind.Range));

        // Source engineSpheres.ts:getSphereRadius + sphereModifiers(..., 'orbital').
        public float GetOrbitalLiveRadiusPixels(int progressionLevel)
        {
            var multiplier = GetSphereRadiusMultiplier() * GetShopSphereRadiusMultiplier()
                * GetArtifactSphereRadiusMultiplier(SphereId.Orbital) * GetCharacterSphereRadiusMultiplier()
                * GetSourceSphereModifiers(SphereId.Orbital, progressionLevel).radius;
            // The source applies an additional Architect mastery radius factor
            // after getCharacterRadiusMultiplier has already been applied.
            if (_sphereCharacterId == SphereCharacterId.Architect && _characterMasteryLevel >= 3)
                multiplier *= 1.02f;

            return 130f * multiplier;
        }
        public float GetProjectileSpeedMultiplier() => 1f;
        public int SphereBoostDamageBonus => LevelUpAbilityRules.GetSphereBoostDamageBonus(_kills, GetAbilityLevel(RunUpgradeKind.SphereBoost));
        public float ResonanceCharge => _resonanceCharge;
        public bool ResonanceEventActive => _resonanceEventActive;
        public int ResonanceEventsTriggered => _resonanceEventsTriggered;
        public int CurrentCombo => _combo;
        public float CurrentComboTimer => _comboTimer;
        public float CurrentComboMultiplier => _comboMultiplier;

        public string GetSphereBranch(SphereId type) => _sphereBranches.TryGetValue(type, out var id) ? id : null;
        public string GetSphereFinal(SphereId type) => _sphereFinals.TryGetValue(type, out var id) ? id : null;
        public bool HasArtifact(string artifactId) =>
            !string.IsNullOrEmpty(artifactId) && _ownedArtifacts.Contains(artifactId);
        public int GetSphereAuthoredModifierLevel(SphereId type, SphereAuthoredModifier modifier) =>
            GetSourceSphereModifierLevel(type, modifier);
        public int GetSphereProjectilePierceBonus(SphereId type) =>
            SphereAuthoredModifierRules.GetProjectilePierceBonus(GetSphereBranch(type), GetSphereFinal(type));
        public int GetSphereProjectileRicochetCount(SphereId type)
        {
            // Prism branches have a dedicated beam/refraction path that resolves their authored Ricochet.
            if (type == SphereId.Prism) return 0;
            return (int)GetSourceSphereModifiers(type).ricochet;
        }

        public float GetSphereAuthoredDamageMultiplier(SphereId type) =>
            SphereAuthoredModifierRules.GetDamageMultiplier(GetSphereBranch(type), GetSphereFinal(type));
        public float GetSphereStatusDurationMultiplier(SphereElement element)
        {
            var multiplier = 1f + CharacterRules.Get(_sphereCharacterId).StatusDuration;
            // Source Alchemist mastery III extends every Freeze application by 50%.
            // A ready Catalyst instead extends the next Fire/Poison application and is consumed.
            if (_sphereCharacterId == SphereCharacterId.Alchemist)
            {
                if (element == SphereElement.Freeze && _characterMasteryLevel >= 3)
                {
                    multiplier *= 1.5f;
                    _alchemistCatalystTimer = 0f;
                }
                else if ((element == SphereElement.Fire || element == SphereElement.Poison) && _alchemistCatalystTimer > 0f)
                {
                    multiplier *= 1.5f;
                    _alchemistCatalystTimer = 0f;
                }
            }
            return multiplier;
        }
        public float GetSphereStatusDamageMultiplier() => 1f + CharacterRules.Get(_sphereCharacterId).StatusDamage;
        public float GetSphereElementalMasteryDamageMultiplier() =>
            _sphereCharacterId == SphereCharacterId.Alchemist && _characterMasteryLevel >= 3 ? 1.05f : 1f;

        public void TryTriggerAlchemistReaction(EnemyAgent2D enemy)
        {
            if (_sphereCharacterId != SphereCharacterId.Alchemist || enemy == null || !enemy.IsAlive) return;
            var kind = enemy.GetAlchemistReactionKind();
            if (kind == AlchemistReactionKind.None) return;

            _alchemistReactionCount++;
            var mastery = Mathf.Clamp(_characterMasteryLevel, 1, 10);
            var multiplier = AlchemistReactionRules.GetBurstMultiplier(kind, mastery, _alchemistReactionCount);
            if (multiplier <= 0f) return;

            var center = (Vector2)enemy.transform.position;
            var damage = AlchemistReactionRules.GetBaseDamage(_level) * multiplier;
            var color = kind == AlchemistReactionKind.ToxinIgnition
                ? new Color(0.35f, 0.55f, 0.29f, 0.95f)
                : kind == AlchemistReactionKind.Cryotoxin
                    ? new Color(0.29f, 0.48f, 0.54f, 0.95f)
                    : new Color(0.77f, 0.43f, 0.24f, 0.95f);

            enemy.ConsumeAlchemistReaction(kind);
            enemy.ReceiveDamage(damage);

            var splashCount = AlchemistReactionRules.GetSplashTargetCount(mastery);
            if (splashCount > 0)
            {
                var radius = AlchemistReactionRules.GetReactionRadiusSourcePixels(mastery) * SpherePlacementRules.SourcePixelsToWorld;
                var nearby = FindEnemiesInRadius(center, radius);
                var applied = 0;
                for (var i = 0; i < nearby.Count && applied < splashCount; i++)
                {
                    var target = nearby[i];
                    if (target == null || target == enemy || !target.IsAlive) continue;
                    target.ReceiveDamage(damage * AlchemistReactionRules.GetSplashDamageMultiplier());
                    SpawnImpact(target.transform.position, color);
                    applied++;
                }
            }

            SpawnImpact(center, color);
            _alchemistCatalystTimer = AlchemistReactionRules.GetCatalystDurationSeconds(mastery);
        }

        private void EnsureShopSaveLoaded()
        {
            if (_shopSaveLoaded) return;
            var raw = PlayerPrefs.GetString(ShopPreferenceKey, "");
            try { _shopSave = string.IsNullOrEmpty(raw) ? new ShopSaveData() : JsonUtility.FromJson<ShopSaveData>(raw); }
            catch { _shopSave = new ShopSaveData(); }
            if (_shopSave == null || _shopSave.upgrades == null) _shopSave = new ShopSaveData();
            _shopSave.upgrades.Clamp();
            _shopSaveLoaded = true;
        }

        private void SaveShopState()
        {
            EnsureShopSaveLoaded();
            _shopSave.upgrades.Clamp();
            PlayerPrefs.SetString(ShopPreferenceKey, JsonUtility.ToJson(_shopSave));
            PlayerPrefs.Save();
        }

        private int GetGold() => Mathf.Max(0, PlayerPrefs.GetInt(GoldPreferenceKey, 0));

        private void SaveGold(int value)
        {
            PlayerPrefs.SetInt(GoldPreferenceKey, Mathf.Max(0, value));
            PlayerPrefs.Save();
        }

        private List<LeaderboardEntry> LoadLeaderboardEntries()
        {
            var entries = new List<LeaderboardEntry>();
            var raw = PlayerPrefs.GetString(LeaderboardPreferenceKey, "");
            if (string.IsNullOrEmpty(raw)) return entries;

            try
            {
                // Source persistence stores a JSON array. Wrap it temporarily for Unity JsonUtility.
                var data = JsonUtility.FromJson<LeaderboardSaveData>("{\"entries\":" + raw + "}");
                if (data != null && data.entries != null) entries = data.entries;
            }
            catch
            {
                return new List<LeaderboardEntry>();
            }

            entries.RemoveAll(entry => entry == null || string.IsNullOrEmpty(entry.name) ||
                float.IsNaN(entry.time) || float.IsInfinity(entry.time) || entry.time < 0f || entry.wave < 0 || entry.date < 0);
            entries.Sort((a, b) => b.time.CompareTo(a.time));
            if (entries.Count > 10) entries.RemoveRange(10, entries.Count - 10);
            return entries;
        }

        private void SaveLeaderboardEntries()
        {
            var json = JsonUtility.ToJson(new LeaderboardSaveData { entries = _leaderboardEntries });
            var start = json.IndexOf('[');
            var end = json.LastIndexOf(']');
            if (start < 0 || end < start) return;
            // Keep the exact top-level JSON-array schema used by src/persistence.ts.
            PlayerPrefs.SetString(LeaderboardPreferenceKey, json.Substring(start, end - start + 1));
            PlayerPrefs.Save();
        }

        private List<string> LoadUnlockedAchievementIds()
        {
            var raw = PlayerPrefs.GetString(AchievementPreferenceKey, "");
            if (string.IsNullOrEmpty(raw)) return new List<string>();
            try
            {
                var data = JsonUtility.FromJson<AchievementSaveData>("{\"ids\":" + raw + "}");
                return data != null && data.ids != null ? data.ids : new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private void SaveUnlockedAchievementIds()
        {
            var json = JsonUtility.ToJson(new AchievementSaveData { ids = _unlockedAchievementIds });
            var start = json.IndexOf('[');
            var end = json.LastIndexOf(']');
            if (start < 0 || end < start) return;
            // Source stores an array of IDs, not a Unity-specific wrapper object.
            PlayerPrefs.SetString(AchievementPreferenceKey, json.Substring(start, end - start + 1));
            PlayerPrefs.Save();
        }

        private void EnsureCharacterProfilesLoaded()
        {
            if (_characterProfilesLoaded) return;
            _characterProfilesLoaded = true;
            _characterProfiles = new List<CharacterProfileData>();
            for (var i = 0; i < CharacterRules.All.Count; i++)
            {
                var definition = CharacterRules.All[i];
                _characterProfiles.Add(new CharacterProfileData
                {
                    id = CharacterRules.ToSourceId(definition.Id),
                    masteryLevel = 1,
                    masteryXp = 0,
                    unlocked = definition.Id == SphereCharacterId.Spherist
                });
            }

            var raw = PlayerPrefs.GetString(CharacterProfilesPreferenceKey, "");
            if (!string.IsNullOrEmpty(raw))
            {
                try
                {
                    var data = JsonUtility.FromJson<CharacterProfilesSaveData>("{\"profiles\":" + raw + "}");
                    if (data != null && data.profiles != null)
                    {
                        for (var i = 0; i < data.profiles.Count; i++)
                        {
                            var saved = data.profiles[i];
                            if (saved == null || !CharacterRules.TryParseSourceId(saved.id, out var characterId)) continue;
                            var current = FindCharacterProfile(characterId);
                            if (current == null) continue;
                            current.unlocked = characterId == SphereCharacterId.Spherist || saved.unlocked;
                            current.masteryLevel = Mathf.Clamp(saved.masteryLevel, 1, 10);
                            current.masteryXp = Mathf.Max(CharacterRules.GetMasteryThreshold(current.masteryLevel),
                                Mathf.Clamp(saved.masteryXp, 0, CharacterRules.MasteryMaxXp));
                            current.masteryLevel = Mathf.Max(current.masteryLevel, CharacterRules.GetMasteryLevelForXp(current.masteryXp));
                        }
                    }
                }
                catch
                {
                    // Keep valid defaults when the persisted source array is malformed.
                }
            }

            var selectedRaw = PlayerPrefs.GetString(CharacterPreferenceKey, "spherist");
            if (CharacterRules.TryParseSourceId(selectedRaw, out var selectedId) &&
                FindCharacterProfile(selectedId) != null && FindCharacterProfile(selectedId).unlocked)
                _sphereCharacterId = selectedId;
            else
                _sphereCharacterId = SphereCharacterId.Spherist;
            _characterMasteryLevel = GetCharacterProfile(_sphereCharacterId).masteryLevel;
            _expandedCharacterId = CharacterRules.ToSourceId(_sphereCharacterId);
        }

        private CharacterProfileData FindCharacterProfile(SphereCharacterId id)
        {
            var sourceId = CharacterRules.ToSourceId(id);
            for (var i = 0; i < _characterProfiles.Count; i++)
                if (_characterProfiles[i].id == sourceId) return _characterProfiles[i];
            return null;
        }

        private CharacterProfileData GetCharacterProfile(SphereCharacterId id)
        {
            EnsureCharacterProfilesLoaded();
            return FindCharacterProfile(id) ?? _characterProfiles[0];
        }

        private void SaveCharacterProfiles()
        {
            EnsureCharacterProfilesLoaded();
            var json = JsonUtility.ToJson(new CharacterProfilesSaveData { profiles = _characterProfiles });
            var start = json.IndexOf('[');
            var end = json.LastIndexOf(']');
            if (start < 0 || end < start) return;
            // new-desing persists a top-level array; do not change its schema to Unity's wrapper object.
            PlayerPrefs.SetString(CharacterProfilesPreferenceKey, json.Substring(start, end - start + 1));
            PlayerPrefs.SetString(CharacterPreferenceKey, CharacterRules.ToSourceId(_sphereCharacterId));
            PlayerPrefs.Save();
        }

        private bool SelectOrUnlockCharacter(SphereCharacterId id)
        {
            EnsureCharacterProfilesLoaded();
            var definition = CharacterRules.Get(id);
            var profile = FindCharacterProfile(id);
            if (profile == null) return false;
            if (!profile.unlocked)
            {
                if (GetGold() < definition.UnlockCost)
                {
                    ShowMessage(IsRussianUi ? "НЕДОСТАТОЧНО ЗОЛОТА" : "NOT ENOUGH GOLD");
                    return false;
                }
                SaveGold(GetGold() - definition.UnlockCost);
                profile.unlocked = true;
            }
            _sphereCharacterId = id;
            _characterMasteryLevel = profile.masteryLevel;
            _architectFormationTracked = false;
            TrackArchitectFormationChange();
            _expandedCharacterId = CharacterRules.ToSourceId(id);
            if (_mainMenu && _playerCore != null)
            {
                var newMaxHp = (100f + PersistentUpgradeRules.GetStartingHpBonus(GetShopUpgradeLevel("hp"))) * (1f + definition.MaxHp);
                _playerCore.SetMaxHpForNewRun(newMaxHp);
                for (var i = 0; i < _spheres.Count; i++)
                {
                    var sphere = _spheres[i];
                    if (sphere != null) sphere.ApplyProgressionLevel(sphere.ProgressionLevel);
                }
            }
            SaveCharacterProfiles();
            ShowMessage((IsRussianUi ? "ВЫБРАН ПЕРСОНАЖ: " : "CHARACTER SELECTED: ") +
                (IsRussianUi ? definition.NameRu : definition.NameEn));
            return true;
        }

        private void ResetCharacterProgress()
        {
            _characterProfiles.Clear();
            _characterProfilesLoaded = false;
            EnsureCharacterProfilesLoaded();
            _sphereCharacterId = SphereCharacterId.Spherist;
            _characterMasteryLevel = 1;
            PlayerPrefs.DeleteKey(CharacterPreferenceKey);
            PlayerPrefs.DeleteKey(CharacterProfilesPreferenceKey);
        }

        private void UnlockAchievementIf(bool condition, string id)
        {
            if (!condition || _unlockedAchievementIds.Contains(id)) return;
            _unlockedAchievementIds.Add(id);
            SaveUnlockedAchievementIds();
        }

        private void FinalizeRunResult()
        {
            ClearRunSnapshot();
            if (_runResultSaved) return;
            _runResultSaved = true;

            var characterProfile = GetCharacterProfile(_sphereCharacterId);
            var masteryRunXp = Mathf.FloorToInt(_characterMasteryRunXp);
            characterProfile.masteryXp = CharacterRules.GetUpdatedMasteryXp(characterProfile.masteryXp, masteryRunXp);
            characterProfile.masteryLevel = Mathf.Max(characterProfile.masteryLevel,
                CharacterRules.GetMasteryLevelForXp(characterProfile.masteryXp));
            _characterMasteryLevel = characterProfile.masteryLevel;
            SaveCharacterProfiles();

            var difficulty = CurrentDifficulty;
            var goldFormula = System.Math.Floor(_runTime) * 0.15d + _bossesDefeated * 25d + _kills * 0.05d;
            _runGoldEarned = (int)System.Math.Floor(goldFormula * difficulty.GoldMultiplier);
            if (_regionStabilized)
            {
                // Exact src/region.ts reward: region base + 5 per elite + 35 per defeated boss.
                _runGoldEarned += RegionBossScheduleRules.GetBaseGoldReward(_selectedRegion) +
                    _runEliteKills * 5 + _bossesDefeated * 35;
            }
            SaveGold(GetGold() + _runGoldEarned);

            var fallbackName = IsRussianUi ? "Игрок" : "Player";
            var entry = new LeaderboardEntry
            {
                name = string.IsNullOrEmpty(_playerName) ? fallbackName : _playerName,
                time = Mathf.FloorToInt(_runTime),
                wave = _wave,
                date = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            var isNewRecord = _leaderboardEntries.Count == 0 || entry.time > _leaderboardEntries[0].time;
            _sourceResultNewRecord = isNewRecord;
            _leaderboardEntries.Add(entry);
            var sourceOrder = new Dictionary<LeaderboardEntry, int>();
            for (var i = 0; i < _leaderboardEntries.Count; i++) sourceOrder[_leaderboardEntries[i]] = i;
            _leaderboardEntries.Sort((a, b) => a.time == b.time ? sourceOrder[a].CompareTo(sourceOrder[b]) : b.time.CompareTo(a.time));
            if (_leaderboardEntries.Count > 10) _leaderboardEntries.RemoveRange(10, _leaderboardEntries.Count - 10);
            _sourceResultRank = _leaderboardEntries.IndexOf(entry) + 1;
            SaveLeaderboardEntries();

            UnlockAchievementIf(_kills >= 500, "kills_500");
            UnlockAchievementIf(_kills >= 1000, "kills_1000");
            UnlockAchievementIf(_wave >= 30, "wave_30");
            UnlockAchievementIf(_wave >= 50, "wave_50");
            UnlockAchievementIf(_bossesDefeated >= 5, "boss_5");
            UnlockAchievementIf(_bossesDefeated >= 10, "boss_10");
            UnlockAchievementIf(_runEvolutions >= 1, "evolve_1");
            UnlockAchievementIf(_runEvolutions >= 3, "evolve_3");
            UnlockAchievementIf(_runEliteKills >= 10, "elite_10");
            UnlockAchievementIf(_combo >= 50, "combo_50");
            UnlockAchievementIf(_runChestOpens >= 5, "chest_5");
            UnlockAchievementIf(_playerCore != null && _playerCore.DashCount >= 50, "dash_50");

            if (isNewRecord) ShowMessage(IsRussianUi ? "НОВЫЙ РЕКОРД!" : "NEW RECORD!");
            else ShowMessage(IsRussianUi ? "РЕЗУЛЬТАТ СОХРАНЁН" : "RUN RESULT SAVED");
        }

        private void ResetPersistentProgress()
        {
            ClearRunSnapshot();
            _knownKnowledgeIds.Clear();
            PlayerPrefs.DeleteKey(GoldPreferenceKey);
            PlayerPrefs.DeleteKey(ShopPreferenceKey);
            PlayerPrefs.DeleteKey(LeaderboardPreferenceKey);
            PlayerPrefs.DeleteKey(AchievementPreferenceKey);
            PlayerPrefs.DeleteKey(DifficultyPreferenceKey);
            PlayerPrefs.DeleteKey(SoundPreferenceKey);
            PlayerPrefs.DeleteKey(HandednessPreferenceKey);
            PlayerPrefs.DeleteKey(LanguagePreferenceKey);
            PlayerPrefs.DeleteKey(NamePreferenceKey);
            ResetCharacterProgress();
            PlayerPrefs.DeleteKey("echosphere_knowledge_v1");
            PlayerPrefs.DeleteKey(ArtifactProtocolKnowledgePreferenceKey);
            _knownArtifactProtocolIds.Clear();
            _artifactProtocolKnowledgeLoaded = true;
            _artifactPanelOpen = false;
            PlayerPrefs.DeleteKey("echosphere_tutorial_completed_v1");
            PlayerPrefs.DeleteKey("echo-sphere:region:resonance_basin:stabilized");
            PlayerPrefs.DeleteKey("echo-sphere:region:resonance_basin:challenges");
            PlayerPrefs.DeleteKey("echo-sphere:region:resonance_basin:endless");
            PlayerPrefs.DeleteKey("echo-sphere:region:spectral_rift:stabilized");
            PlayerPrefs.DeleteKey("echo-sphere:region:spectral_rift:challenges");
            PlayerPrefs.DeleteKey("echo-sphere:region:spectral_rift:endless");
            PlayerPrefs.DeleteKey(SelectedRegionPreferenceKey);
            PlayerPrefs.Save();

            _shopSave = new ShopSaveData();
            _shopSaveLoaded = true;
            _playerName = "";
            _leaderboardEntries.Clear();
            _unlockedAchievementIds.Clear();
            _selectedRegion = EchoRegionId.ResonanceBasin;
            _settingsResetPending = false;
            _mainMenuPage = MainMenuPage.Home;
            ShowMessage(IsRussianUi ? "ПРОГРЕСС СБРОШЕН" : "PROGRESS RESET");
        }

        private void Start()
        {
            Application.lowMemory+=OnNativeLowMemory;TraceNativeRoute("runtime-start");
            Application.targetFrameRate = 60;
            SourceAudio2D.SetEnabled(PlayerPrefs.GetInt(SoundPreferenceKey, 1) != 0);
            EnsureShopSaveLoaded();
            EnsureCharacterProfilesLoaded();
            _playerName = PlayerPrefs.GetString(NamePreferenceKey, "");
            _leaderboardEntries = LoadLeaderboardEntries();
            _unlockedAchievementIds = LoadUnlockedAchievementIds();
            ConfigureCamera();
            CreateStarfield();
            var savedRegionValue = PlayerPrefs.GetInt(SelectedRegionPreferenceKey, (int)EchoRegionId.ResonanceBasin);
            _selectedRegion = (EchoRegionId)Mathf.Clamp(savedRegionValue, (int)EchoRegionId.ResonanceBasin, (int)EchoRegionId.SpectralRift);
            if (!IsRegionAvailable(_selectedRegion)) _selectedRegion = EchoRegionId.ResonanceBasin;
            _regionChallengeId = "none";
            _regionEndlessMode = false;
            _regionSchedule = RegionBossScheduleRules.Create(_selectedRegion, EchoRegionMode.Stabilization);
            _tutorialStep = PlayerPrefs.GetInt(TutorialCompletedPreferenceKey, 0) == 0 ? 0 : -1;
            _tutorialUpgradeOpened = false;
            _tutorialStartPosition = _player != null ? _player.position : Vector3.zero;
            if (_tutorialStep >= 0) _spawnTimer = 999f;
            _regionFieldVisuals = gameObject.AddComponent<RegionFieldVisuals>();
            _regionFieldVisuals.Initialize(this, _selectedRegion);
            CreatePlayer();
            CreateSpheres();
            _follow = false;
            // Source new-desing starts with Formation Follow off. Players can
            // enable it from the existing control when they choose to.
            RefreshNetworkState();
            if(PlayerPrefs.GetInt("echo-sphere:open_region_select",0)==1){PlayerPrefs.DeleteKey("echo-sphere:open_region_select");PlayerPrefs.Save();_regionSelect=true;}
            if (PlayerPrefs.GetInt("echo-sphere:start_selected_region_after_reload", 0) == 1)
            {
                _regionChallengeId=PlayerPrefs.GetString("echo-sphere:pending_region_challenge","none");_regionEndlessMode=PlayerPrefs.GetInt("echo-sphere:pending_region_endless",0)==1;
                PlayerPrefs.DeleteKey("echo-sphere:start_selected_region_after_reload");
                PlayerPrefs.DeleteKey("echo-sphere:pending_region_challenge");PlayerPrefs.DeleteKey("echo-sphere:pending_region_endless");
                PlayerPrefs.Save();
                BeginSelectedRegionRun();
            }
        }

        private void Update()
        {
            PruneDestroyed();
            if (!_mainMenu) TickSourceKnowledge(Mathf.Min(0.05f, Time.unscaledDeltaTime));
            TickRunAutosave();
            if (TickSourceDeath()) return;
            if (Input.GetKeyDown(KeyCode.Escape) && !_gameOver && !_levelUp && !_choosingEvolution && !_stellaRewardPending)
                SetUserPaused(!_userPaused);
            if (_mainMenu || (_paused && !(_tutorialStep == 1 || _tutorialStep == 2)) || _playerCore == null) return;
            _simulationFrame = true;
            try
            {
            var dt = Mathf.Min(0.05f, Time.deltaTime);
            SimulationDeltaTime = dt;
            if (Input.GetKeyDown(KeyCode.Space)) TrySourceDash();
            foreach (var binding in _activeAbilityHotkeys)
                if (Input.GetKeyDown(binding.Value)) ActivateActiveAbility(binding.Key);
            if(_sourceUi==null)HandleSpherePointerInput();
            _runTime += dt;
            TickRegionHudState(dt);
            TickSourceRegionFrame(dt);
            TickHunterCharacterState(dt);
            TickEngineerRelay(dt);
            _alchemistCatalystTimer = Mathf.Max(0f, _alchemistCatalystTimer - dt);
            _playerCore.Heal(LevelUpAbilityRules.GetRegenPerSecond(GetAbilityLevel(RunUpgradeKind.Regen)) * dt);
            if (_comboTimer > 0f)
            {
                _comboTimer = ComboRules.TickTimer(_comboTimer, dt);
                if (_comboTimer <= 0f)
                {
                    _combo = 0;
                    _comboMultiplier = 1f;
                }
            }

            var dashWasActive = _playerCore.DashTimer > 0f;
            _playerCore.Tick(dt);
            TickSourceEarlyPlayerState(dt, dashWasActive);
            TickSimulationPhase<StellaChestAgent2D>(dt, true);
            TickSimulationPhase<ArtifactChestAgent2D>(dt, true);
            TickSourceMutation();
            _swiftBootsTimer = Mathf.Max(0f, _swiftBootsTimer - dt);
            _timeStopTimer = Mathf.Max(0f, _timeStopTimer - dt);
            _teleportDamageBuffTimer = Mathf.Max(0f, _teleportDamageBuffTimer - dt);
            _teleportCooldown = Mathf.Max(0f, _teleportCooldown - dt);
            _timeStopCooldown = Mathf.Max(0f, _timeStopCooldown - dt);
            _blastCooldown = Mathf.Max(0f, _blastCooldown - dt);
            _shieldCooldown = Mathf.Max(0f, _shieldCooldown - dt);
            _fireTrailCooldown = Mathf.Max(0f, _fireTrailCooldown - dt);
            _minionCooldown = Mathf.Max(0f, _minionCooldown - dt);
            _lightningCooldown = Mathf.Max(0f, _lightningCooldown - dt);
            _darkRitualCooldown = Mathf.Max(0f, _darkRitualCooldown - dt);
            _fireTrailTimer = Mathf.Max(0f, _fireTrailTimer - dt);
            _fireCatalystTimer = Mathf.Max(0f, _fireCatalystTimer - dt);
            _darkRitualOverloadTimer = Mathf.Max(0f, _darkRitualOverloadTimer - dt);
            _shieldTimer = Mathf.Max(0f, _shieldTimer - dt);
            _shieldVisualPulse = Mathf.Max(0f, _shieldVisualPulse - dt);
            _activeAbilitySlots = _level >= 20 ? 3 : _level >= 12 ? 2 : _level >= 5 ? 1 : 0;
            _conductorOverdriveTimer = Mathf.Max(0f, _conductorOverdriveTimer - dt);
            _oracleSourceFlashTimer = Mathf.Max(0f, _oracleSourceFlashTimer - dt);
            _characterScreenShakeIntensity = Mathf.Max(0f, _characterScreenShakeIntensity - dt);
            if (_messageTimer > 0f) _messageTimer -= dt;
            _formationMemoryTimer = Mathf.Max(0f, _formationMemoryTimer - dt);
            _resonanceGeometryMemoryTimer = Mathf.Max(0f, _resonanceGeometryMemoryTimer - dt);
            if (_resonanceGeometryMemoryTimer <= 0f)
            {
                _resonanceGeometryMemoryPoints.Clear();
                _resonanceGeometryMemoryType = SphereNetworkFormation.None;
            }
            _placementFlashTimer = Mathf.Max(0f, _placementFlashTimer - dt);
            var input = ReadMovement();
            var playerShell = _player != null ? _player.Find("Shell") : null;
            var playerSprite = playerShell != null ? playerShell.GetComponent<SpriteRenderer>() : null;
            // The authored three-quarter Spherist faces left; mirror only while
            // travelling right, matching the active Canvas renderer contract.
            if (playerSprite != null && Mathf.Abs(input.x) > 0.05f) playerSprite.flipX = input.x > 0f;
            if (input.sqrMagnitude > 0.0001f) _playerCore.RememberMoveDirection(input);
            var requested = new Vec2(input.x, input.y);
            var followMultiplier = _follow ? FormationFollowRules.GetMovementMultiplier(true, _formationStrain, _lastDirection, requested) : 1f;
            if (_follow) _formationStrain = FormationFollowRules.UpdateStrain(_formationStrain, _lastDirection, requested, dt, out _lastDirection);
            else { _formationStrain = 0f; _lastDirection = Vec2.Zero; }

            _player.position += (Vector3)(_playerCore.GetVelocity(input, 180f * SpherePlacementRules.SourcePixelsToWorld * GetChallengeMovementMultiplier() * GetArtifactMoveSpeedMultiplier() *
                LevelUpAbilityRules.GetMoveSpeedMultiplier(GetAbilityLevel(RunUpgradeKind.MoveSpeed)) *
                LevelUpAbilityRules.GetMomentumSpeedMultiplier(GetAbilityLevel(RunUpgradeKind.Momentum), _playerCore.DashTimer > 0f) *
                GetCharacterMoveSpeedMultiplier() * PlayerLoopRules.MutationSpeed(_mutationStage) *
                (_sphereCharacterId == SphereCharacterId.Berserker && _characterMasteryLevel >= 5 && _buffTimer > 0f ? 1.05f : 1f), followMultiplier) * dt);
            if (_follow) foreach (var sphere in _spheres) if (sphere != null) sphere.SyncFollowPosition();
            _playerCore.Heal(Mathf.Max(0f, ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "regen")) * dt);
            TickSourceLatePlayerState(dt);
            RefreshNetworkState();
            TickResonanceRing(dt);
            TickSimulationPhase<SphereAttackAgent>(dt);
            TickSimulationPhase<ProjectileAgent>(dt, true);
            TickCharacterSpecialRuntime(dt);
            TickSimulationPhase<AbilityDroneAgent2D>(dt, true);
            var cameraTarget = new Vector3(_player.position.x, _player.position.y, -10f);
            var cameraBase = _camera.transform.position - _lastHunterScreenShakeOffset;
            cameraBase = Vector3.Lerp(cameraBase, cameraTarget, 1f - Mathf.Exp(-5f * dt));
            var screenShakeIntensity = Mathf.Max(_hunterScreenShakeIntensity, _characterScreenShakeIntensity);
            var shakePixelsX = (UnityEngine.Random.value - 0.5f) * screenShakeIntensity * 20f;
            var shakePixelsY = (UnityEngine.Random.value - 0.5f) * screenShakeIntensity * 20f;
            _lastHunterScreenShakeOffset = new Vector3(
                shakePixelsX * SpherePlacementRules.SourcePixelsToWorld,
                shakePixelsY * SpherePlacementRules.SourcePixelsToWorld, 0f);
            _camera.transform.position = cameraBase + _lastHunterScreenShakeOffset;
            TickCharacterMastery();
            if (_tutorialStep < 0)
            {
            SpawnSourceRegionBossForFrame();
            _spawnTimer -= dt;
            if (_spawnTimer <= 0f && _waveEnemiesToSpawn > 0)
            {
                SpawnEnemy();
                _waveEnemiesToSpawn--;
                _spawnTimer = RunBalanceRules.GetWaveSpawnInterval(_wave) / Mathf.Max(0.1f, CurrentDifficulty.SpawnRateMultiplier);
            }
            if (_waveEnemiesToSpawn <= 0 && CountLivingNonBossEnemies() == 0 && !HasActiveBoss())
            {
                _spawnTimer = 3f;
                StartNextWave();
            }
            TickSimulationPhase<EnemyAgent2D>(dt, true);
            TickSourceUnownedEnemyProjectiles(dt);
            _regionFieldVisuals.TickGameplayPois();
            }
            TickSimulationPhase<ExperienceOrbAgent>(dt, true);
            TickSimulationPhase<HealthPackAgent2D>(dt, true);
            TickSimulationPhase<RuneAgent2D>(dt, true);
            TickSimulationPhase<SourceParticle2D>(dt, true); TickSourceLightningVisuals(dt);
            ResolveSourceSpaceCollisions(dt);
            _camera.transform.position = new Vector3(_player.position.x, _player.position.y, -10f) + _lastHunterScreenShakeOffset;
            if (_regionStabilized) { _gameOver = true; _paused = true; FinalizeRunResult(); }
            }
            finally { _simulationFrame = false; }
        }

        private Vector2 ReadMovement()
        {
            var direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) + (_sourceUi!=null?NativeMobileMovement:_sourceJoystickDirection);
            direction.x = Mathf.Clamp(direction.x, -1f, 1f); direction.y = Mathf.Clamp(direction.y, -1f, 1f);
            return direction.sqrMagnitude > 0f ? direction.normalized : Vector2.zero;
        }

        private void ConfigureCamera()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                var go = new GameObject("Main Camera");
                _camera = go.AddComponent<Camera>();
                go.tag = "MainCamera";
            }
            _camera.orthographic = true;
            _camera.orthographicSize = 6.3f;
            _camera.backgroundColor = new Color(0.018f, 0.027f, 0.075f, 1f);
            _camera.transform.position = new Vector3(0f, 0f, -10f);
            _lastHunterScreenShakeOffset = Vector3.zero;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            // The prototype scene is generated empty, so ensure the runtime
            // camera supplies the single listener Unity expects. Do not add a
            // duplicate if another active listener is already present.
            if (FindObjectsByType<AudioListener>().Length == 0)
            {
                var listener = _camera.GetComponent<AudioListener>();
                if (listener == null) listener = _camera.gameObject.AddComponent<AudioListener>();
                listener.enabled = true;
            }
        }

        private void CreateStarfield()
        {
            if (UsesSourcePresentation) return;
            var field = new GameObject("Source Backdrop").AddComponent<SourceBackdrop2D>();
            field.Initialize(this, _camera);
        }

        private void CreatePlayer()
        {
            var tint = new Color(0.2f, 0.91f, 1f);
            var go = CreateOrb("Spherist", Vector2.zero, 1f, tint, Color.white, 10);

            // In the source renderer, player.svg is the Core glyph. The actual
            // playable character is the authored three-quarter Spherist art.
            // Use that production sprite and do not stack primitive rings/core
            // layers over its already-authored shell and energy ring.
            var authoredCharacter = RuntimeSpriteFactory.LoadAuthored("character-spherist-3q");
            if (!ApplyAuthoredShellVisual(go, authoredCharacter, 0.62f))
                ApplyShellVisual(go, RuntimeSpriteFactory.Faceted, 0.95f);

            var glow = go.transform.Find("Glow");
            if (glow != null)
            {
                glow.localScale = Vector3.one * 1.16f;
                var glowRenderer = glow.GetComponent<SpriteRenderer>();
                if (glowRenderer != null) glowRenderer.color = new Color(tint.r, tint.g, tint.b, 0.09f);
            }

            _player = go.transform;
            _playerCore = go.AddComponent<PlayerCoreController2D>();
            _playerCore.IncreaseMaxHp(PersistentUpgradeRules.GetStartingHpBonus(GetShopUpgradeLevel("hp")));
            _playerCore.ApplyBaseMaxHpMultiplier(1f + CharacterRules.Get(_sphereCharacterId).MaxHp);
            go.AddComponent<SourcePlayerVisual2D>().Initialize(this, _playerCore);
        }


        private void CreateSpheres()
        {
            // Source engineState.ts starts with one unlevelled Standard Sphere
            // 95 logical px above Core. Unlocking/levelling a Sphere is a
            // separate progression action from physically placing it.
            _sphereLevels[SphereId.Standard] = 0;
            CreateSphere(SphereId.Standard, 0, (Vector2)_player.position + new Vector2(0f, 95f * SpherePlacementRules.SourcePixelsToWorld));
        }

        private void CreateSphere(SphereId type, int level, Vector2? explicitPosition = null)
        {
            var angle = _spheres.Count * 2.3999632f;
            var radius = 0.9f + 0.12f * (_spheres.Count % 3);
            var position = explicitPosition ?? ((Vector2)_player.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            var definition = GameCatalog.Spheres[(int)type];
            var go = CreateOrb(definition.DisplayName + " Sphere", position, 1.05f, SphereColor(type), Color.white, 8);
            ApplySphereVisual(go, type, level);
            var agent = go.AddComponent<SphereAttackAgent>();
            agent.Initialize(this, _player, type, 12f);
            if (type == SphereId.Standard)
            {
                // Source renderer.ts:drawStandardAttackVfx is a transient charge muzzle,
                // separate from the Standard core/ring assembly and combat simulation.
                go.AddComponent<StandardSphereAttackVfx2D>().Initialize(this, agent);
            }
            if (type != SphereId.Standard)
            {
                var runtimeVfx = go.AddComponent<SphereRuntimeVfx2D>();
                runtimeVfx.Initialize(this, agent);
            }
            var elementalVfx = go.AddComponent<SphereElementalVfx2D>();
            elementalVfx.Initialize(this, agent);
            _spheres.Add(agent);
            // Character radius bonuses depend on the active network/count. Include this Sphere before recalculating its range.
            agent.ApplyProgressionLevel(level);
            if (_follow) agent.SetFollowMode(true, (Vector2)_player.position);
        }

        private static Color SphereUiColor(SphereId type)
        {
            var definition = GameCatalog.Spheres[(int)type];
            if (!string.IsNullOrEmpty(definition.ColorHex) && ColorUtility.TryParseHtmlString(definition.ColorHex, out var sourceColor))
                return sourceColor;
            return SphereColor(type);
        }

        private static Color SphereColor(SphereId type)
        {
            switch (type)
            {
                case SphereId.Standard: return new Color(0.24f, 0.88f, 1f);
                case SphereId.Sniper: return new Color(0.77f, 0.45f, 1f);
                case SphereId.Chain: return new Color(1f, 0.88f, 0.35f);
                case SphereId.Shotgun: return new Color(1f, 0.56f, 0.24f);
                case SphereId.Aura: return new Color(0.34f, 0.90f, 0.71f);
                case SphereId.Orbital: return new Color(0.56f, 0.94f, 1f);
                case SphereId.Prism: return new Color(1f, 0.55f, 0.88f);
                case SphereId.Gravity: return new Color(0.65f, 0.55f, 1f);
                case SphereId.Pulse: return new Color(1f, 0.83f, 0.35f);
                case SphereId.Void: return new Color(0.76f, 0.55f, 1f);
                default: return Color.white;
            }
        }

        private static void ApplyShellVisual(GameObject root, Sprite sprite, float scale)
        {
            var shell = root.transform.Find("Shell");
            if (shell == null) return;
            var renderer = shell.GetComponent<SpriteRenderer>();
            if (renderer == null) return;
            renderer.sprite = sprite;
            shell.localScale = Vector3.one * scale;
        }

        private static bool ApplyAuthoredShellVisual(GameObject root, Sprite sprite, float scale)
        {
            if (root == null || sprite == null) return false;
            var shell = root.transform.Find("Shell");
            if (shell == null) return false;
            var renderer = shell.GetComponent<SpriteRenderer>();
            if (renderer == null) return false;
            renderer.sprite = sprite;
            renderer.color = Color.white;
            shell.localScale = Vector3.one * scale;
            var frame = root.transform.Find("Inner Frame");
            if (frame != null) frame.gameObject.SetActive(false);
            var core = root.transform.Find("Core Light");
            if (core != null) core.gameObject.SetActive(false);
            return true;
        }

        private static Sprite GetAuthoredSphereArtwork(SphereId type)
        {
            switch (type)
            {
                case SphereId.Standard:
                case SphereId.Orbital:
                    // Active Standard core/ring are procedural in src/renderer.ts.
                    // Orbital is a native 2.5D core/ring construction.
                    return null;
                case SphereId.Sniper: return RuntimeSpriteFactory.LoadAuthored("sphere-sniper");
                case SphereId.Chain: return RuntimeSpriteFactory.LoadAuthored("sphere-chain");
                case SphereId.Shotgun: return RuntimeSpriteFactory.LoadAuthored("sphere-shotgun");
                case SphereId.Aura: return RuntimeSpriteFactory.LoadAuthored("sphere-aura");
                case SphereId.Prism: return RuntimeSpriteFactory.LoadAuthored("sphere-prism");
                case SphereId.Gravity: return RuntimeSpriteFactory.LoadAuthored("sphere-gravity");
                case SphereId.Pulse: return RuntimeSpriteFactory.LoadAuthored("sphere-pulse");
                case SphereId.Void: return RuntimeSpriteFactory.LoadAuthored("sphere-void");
                default: return null;
            }
        }


        private static float GetStandardSphereLocalRadius(Transform root, int visualTier)
        {
            // Source renderer.ts: clamp(sphere.radius * 0.19 + visualTier * 0.8, 15, 25).
            // The Standard Sphere's base gameplay radius is 130 source pixels.
            var sourcePixels = Mathf.Clamp(130f * 0.19f + Mathf.Max(0, visualTier) * 0.8f, 15f, 25f);
            var rootScale = root == null ? 1f : Mathf.Max(0.0001f, Mathf.Abs(root.localScale.x));
            return sourcePixels * SpherePlacementRules.SourcePixelsToWorld / rootScale;
        }

        private static bool ApplyStandardSphereArtwork(GameObject root, int visualTier)
        {
            if (root == null) return false;

            // Active new-desing draws the core and the orbit ring procedurally.
            // The only authored component used by the live Standard assembly is
            // upper-crystal.png, drawn twice with the lower copy mirrored.
            var upper = RuntimeSpriteFactory.LoadAuthored("standard-sphere/upper-crystal");
            var shell = root.transform.Find("Shell");
            var frame = root.transform.Find("Inner Frame");
            var core = root.transform.Find("Core Light");
            if (shell == null || frame == null || core == null) return false;

            var upperRenderer = shell.GetComponent<SpriteRenderer>();
            var ringRenderer = frame.GetComponent<SpriteRenderer>();
            var coreRenderer = core.GetComponent<SpriteRenderer>();
            if (upperRenderer == null || ringRenderer == null || coreRenderer == null) return false;

            var radius = GetStandardSphereLocalRadius(root.transform, visualTier);
            var coreGlow = root.transform.Find("Glow");
            if (coreGlow != null)
            {
                coreGlow.localScale = Vector3.one * (1.16f * radius);
                var glowRenderer = coreGlow.GetComponent<SpriteRenderer>();
                if (glowRenderer != null)
                {
                    glowRenderer.sprite = RuntimeSpriteFactory.StandardCoreGlow;
                    glowRenderer.color = new Color(1f, 1f, 1f, 0.89f);
                    glowRenderer.sortingOrder = 7;
                }
            }

            coreRenderer.sprite = RuntimeSpriteFactory.Disc;
            coreRenderer.color = new Color(30f / 255f, 135f / 255f, 232f / 255f, 0.72f);
            coreRenderer.sortingOrder = 8;
            core.localScale = Vector3.one * (0.84f * radius / Mathf.Max(0.0001f, coreRenderer.sprite.bounds.size.x));
            core.gameObject.SetActive(true);

            var outline = root.transform.Find("Standard Core Outline");
            if (outline == null)
            {
                var outlineObject = new GameObject("Standard Core Outline");
                outlineObject.transform.SetParent(root.transform, false);
                outline = outlineObject.transform;
            }
            var outlineRenderer = outline.GetComponent<SpriteRenderer>();
            if (outlineRenderer == null) outlineRenderer = outline.gameObject.AddComponent<SpriteRenderer>();
            outlineRenderer.sprite = RuntimeSpriteFactory.StandardCoreOutline;
            outlineRenderer.color = new Color(182f / 255f, 247f / 255f, 1f, 0.90f);
            outlineRenderer.sortingOrder = 9;
            outline.localScale = Vector3.one * (0.84f * radius / (0.73f * Mathf.Max(0.0001f, outlineRenderer.sprite.bounds.size.x)));
            outline.gameObject.SetActive(true);

            ringRenderer.sprite = RuntimeSpriteFactory.StandardOrbitRing;
            ringRenderer.color = new Color(48f / 255f, 214f / 255f, 1f, 0.82f);
            ringRenderer.sortingOrder = 10;
            frame.localScale = new Vector3(
                2f * 1.18f * 0.98f * radius / (0.73f * Mathf.Max(0.0001f, ringRenderer.sprite.bounds.size.x)),
                2f * 1.18f * 0.34f * radius / (0.73f * Mathf.Max(0.0001f, ringRenderer.sprite.bounds.size.y)),
                1f);
            frame.gameObject.SetActive(true);

            if (upper != null)
            {
                var crystalScale = 1.34f * radius / Mathf.Max(0.0001f, upper.bounds.size.x);
                upperRenderer.sprite = upper;
                upperRenderer.color = Color.white;
                upperRenderer.sortingOrder = 11;
                shell.localPosition = new Vector3(0f, 0.62f * radius, 0f);
                shell.localScale = Vector3.one * crystalScale;
                shell.gameObject.SetActive(true);

                var lowerObject = root.transform.Find("Standard Lower Crystal");
                if (lowerObject == null)
                {
                    var lowerGo = new GameObject("Standard Lower Crystal");
                    lowerGo.transform.SetParent(root.transform, false);
                    lowerObject = lowerGo.transform;
                }
                lowerObject.localPosition = new Vector3(0f, -0.66f * radius, 0f);
                lowerObject.localRotation = Quaternion.identity;
                lowerObject.localScale = Vector3.one * crystalScale;
                var lowerRenderer = lowerObject.GetComponent<SpriteRenderer>();
                if (lowerRenderer == null) lowerRenderer = lowerObject.gameObject.AddComponent<SpriteRenderer>();
                lowerRenderer.sprite = upper;
                lowerRenderer.color = Color.white;
                lowerRenderer.flipY = true;
                lowerRenderer.sortingOrder = 12;
                lowerObject.gameObject.SetActive(true);
            }
            else
            {
                // Upstream upper-crystal.png has a corrupt DEFLATE payload in
                // both branches. Do not replace it with an invented silhouette:
                // the source renderer itself falls back to core + ring when it
                // cannot load the crystal.
                shell.gameObject.SetActive(false);
                var lowerObject = root.transform.Find("Standard Lower Crystal");
                if (lowerObject != null) lowerObject.gameObject.SetActive(false);
            }

            return true;
        }

        private static void ApplyOrbitalSphereVisual(GameObject root)
        {
            var shell = root.transform.Find("Shell");
            var frame = root.transform.Find("Inner Frame");
            var core = root.transform.Find("Core Light");
            if (shell == null || core == null) return;

            // The source has no single orbital SVG. Its identity is a 2.5D core,
            // crossing rings and crystal terminals. Do not use the old static mock.
            var shellRenderer = shell.GetComponent<SpriteRenderer>();
            if (shellRenderer != null)
            {
                shellRenderer.sprite = RuntimeSpriteFactory.Faceted;
                shellRenderer.color = new Color(0.12f, 0.30f, 0.43f, 1f);
                shellRenderer.sortingOrder = 8;
                shell.localScale = Vector3.one * 0.78f;
            }
            if (frame != null) frame.gameObject.SetActive(false);

            var coreRenderer = core.GetComponent<SpriteRenderer>();
            if (coreRenderer != null)
            {
                coreRenderer.sprite = RuntimeSpriteFactory.Disc;
                coreRenderer.color = new Color(0.56f, 0.94f, 1f, 1f);
                coreRenderer.sortingOrder = 9;
            }
            core.localScale = Vector3.one * 0.31f;

            var glow = root.transform.Find("Glow");
            if (glow != null)
            {
                glow.localScale = Vector3.one * 1.45f;
                var glowRenderer = glow.GetComponent<SpriteRenderer>();
                if (glowRenderer != null) glowRenderer.color = new Color(0.30f, 0.87f, 1f, 0.11f);
            }

            // Animated Orbital contours/terminals are owned by SphereRuntimeVfx2D.
            // Remove the old static rail/node layer to avoid doubled rings.
        }

        private void TickSphereArtAnimation(float deltaTime)
        {
            for (var i = 0; i < _spheres.Count; i++)
            {
                var sphere = _spheres[i];
                if (sphere == null || sphere.Type != SphereId.Standard) continue;

                var tier = Mathf.Max(0, sphere.ProgressionLevel);
                var radius = GetStandardSphereLocalRadius(sphere.transform, tier);
                var sourceX = sphere.transform.position.x / SpherePlacementRules.SourcePixelsToWorld;
                var sourceY = sphere.transform.position.y / SpherePlacementRules.SourcePixelsToWorld;
                var breathe = Mathf.Sin(_runTime * 2.2f + sourceX * 0.013f) * 0.028f;
                var energyPulse = 0.5f + 0.5f * Mathf.Sin(_runTime * 4.6f + sourceY * 0.009f);

                // Source renderer.ts: targetPulse only affects the visual assembly,
                // not the Sphere's gameplay transform or attack calculations.
                var sourceTargetRange = 130f * SphereCombatRules.GetProfile(sphere.Type).RangeMultiplier *
                    SpherePlacementRules.SourcePixelsToWorld;
                var hasTarget = false;
                for (var enemyIndex = 0; enemyIndex < _enemies.Count; enemyIndex++)
                {
                    var enemy = _enemies[enemyIndex];
                    if (enemy == null || !enemy.IsAlive) continue;
                    if (Vector2.Distance((Vector2)enemy.transform.position, sphere.transform.position) >= sourceTargetRange) continue;
                    hasTarget = true;
                    break;
                }
                var targetPulse = hasTarget ? 0.5f + 0.5f * Mathf.Sin(_runTime * 11f) : 0f;
                var attackScale = 1f + targetPulse * 0.025f;
                var xScale = attackScale + breathe * 0.18f;
                var yScale = attackScale - breathe * 0.12f;
                var crystalPulse = 1f + energyPulse * 0.022f;

                var frame = sphere.transform.Find("Inner Frame");
                if (frame != null && frame.gameObject.activeSelf)
                {
                    frame.Rotate(0f, 0f,
                        Mathf.Rad2Deg * (0.22f + tier * 0.018f) * deltaTime, Space.Self);
                    var frameRenderer = frame.GetComponent<SpriteRenderer>();
                    if (frameRenderer != null && frameRenderer.sprite != null)
                    {
                        frameRenderer.color = new Color(48f / 255f, 214f / 255f, 1f, 0.82f * (0.72f + targetPulse * 0.20f));
                        frame.localScale = new Vector3(
                            2f * 1.18f * 0.98f * radius / (0.73f * frameRenderer.sprite.bounds.size.x) * xScale,
                            2f * 1.18f * 0.34f * radius / (0.73f * frameRenderer.sprite.bounds.size.y) * yScale,
                            1f);
                    }
                }

                var glow = sphere.transform.Find("Glow");
                if (glow != null && glow.gameObject.activeSelf)
                {
                    glow.localPosition = Vector3.zero;
                    glow.localScale = new Vector3(
                        1.16f * radius * (1f + energyPulse * 0.035f) * xScale,
                        1.16f * radius * (1f + energyPulse * 0.035f) * yScale,
                        1f);
                    var glowRenderer = glow.GetComponent<SpriteRenderer>();
                    if (glowRenderer != null)
                        glowRenderer.color = new Color(1f, 1f, 1f, 0.82f + energyPulse * 0.14f);
                }

                var core = sphere.transform.Find("Core Light");
                if (core != null && core.gameObject.activeSelf)
                {
                    core.localPosition = Vector3.zero;
                    var coreRenderer = core.GetComponent<SpriteRenderer>();
                    if (coreRenderer != null && coreRenderer.sprite != null)
                    {
                        var scale = 0.84f * radius * (1f + energyPulse * 0.035f) /
                            Mathf.Max(0.0001f, coreRenderer.sprite.bounds.size.x);
                        core.localScale = new Vector3(scale * xScale, scale * yScale, 1f);
                    }
                }

                var outline = sphere.transform.Find("Standard Core Outline");
                if (outline != null && outline.gameObject.activeSelf)
                {
                    outline.localPosition = Vector3.zero;
                    var outlineRenderer = outline.GetComponent<SpriteRenderer>();
                    if (outlineRenderer != null && outlineRenderer.sprite != null)
                    {
                        var scale = 0.84f * radius / (0.73f * Mathf.Max(0.0001f, outlineRenderer.sprite.bounds.size.x));
                        outline.localScale = new Vector3(scale * xScale, scale * yScale, 1f);
                    }
                }

                var upper = sphere.transform.Find("Shell");
                if (upper != null && upper.gameObject.activeSelf)
                {
                    var upperRenderer = upper.GetComponent<SpriteRenderer>();
                    if (upperRenderer != null && upperRenderer.sprite != null)
                    {
                        upper.localPosition = new Vector3(0f, 0.62f * radius * yScale, 0f);
                        var scale = 1.34f * radius * crystalPulse / Mathf.Max(0.0001f, upperRenderer.sprite.bounds.size.x);
                        upper.localScale = new Vector3(scale * xScale, scale * yScale, 1f);
                    }
                }

                var lower = sphere.transform.Find("Standard Lower Crystal");
                if (lower != null && lower.gameObject.activeSelf)
                {
                    var lowerRenderer = lower.GetComponent<SpriteRenderer>();
                    if (lowerRenderer != null && lowerRenderer.sprite != null)
                    {
                        lower.localPosition = new Vector3(0f, -0.66f * radius * yScale, 0f);
                        var scale = 1.34f * radius * crystalPulse / Mathf.Max(0.0001f, lowerRenderer.sprite.bounds.size.x);
                        lower.localScale = new Vector3(scale * xScale, scale * yScale, 1f);
                    }
                }
            }
        }

        private void ApplySphereVisual(GameObject root, SphereId type, int visualTier)
        {
            var shell = root.transform.Find("Shell");
            var core = root.transform.Find("Core Light");
            if (shell == null || core == null) return;

            if (type == SphereId.Standard && ApplyStandardSphereArtwork(root, visualTier)) return;
            if (type == SphereId.Orbital)
            {
                ApplyOrbitalSphereVisual(root);
                return;
            }

            var authored = GetAuthoredSphereArtwork(type);
            if (ApplyAuthoredShellVisual(root, authored, 1.04f)) return;

            var renderer = shell.GetComponent<SpriteRenderer>();
            switch (type)
            {
                case SphereId.Standard: renderer.sprite = RuntimeSpriteFactory.Faceted; break;
                case SphereId.Sniper: renderer.sprite = RuntimeSpriteFactory.Diamond; break;
                case SphereId.Shotgun: renderer.sprite = RuntimeSpriteFactory.Star; break;
                case SphereId.Chain: renderer.sprite = RuntimeSpriteFactory.Hexagon; break;
                case SphereId.Aura: renderer.sprite = RuntimeSpriteFactory.Ring; break;
                case SphereId.Orbital: renderer.sprite = RuntimeSpriteFactory.Faceted; break;
                case SphereId.Prism: renderer.sprite = RuntimeSpriteFactory.Prism; break;
                case SphereId.Gravity: renderer.sprite = RuntimeSpriteFactory.Diamond; break;
                case SphereId.Pulse: renderer.sprite = RuntimeSpriteFactory.Star; break;
                case SphereId.Void: renderer.sprite = RuntimeSpriteFactory.Shard; break;
                default: renderer.sprite = RuntimeSpriteFactory.Faceted; break;
            }
            shell.localScale = Vector3.one * (type == SphereId.Aura ? 1.18f : 1.02f);
            core.localScale = Vector3.one * (type == SphereId.Aura ? 0.24f : type == SphereId.Prism ? 0.20f : 0.27f);
            var coreRenderer = core.GetComponent<SpriteRenderer>();
            if (coreRenderer != null)
            {
                switch (type)
                {
                    case SphereId.Gravity: coreRenderer.color = new Color(0.73f, 0.56f, 1f); break;
                    case SphereId.Void: coreRenderer.color = new Color(0.84f, 0.72f, 1f); break;
                    case SphereId.Chain: coreRenderer.color = new Color(1f, 0.97f, 0.76f); break;
                    case SphereId.Prism: coreRenderer.color = new Color(1f, 0.88f, 0.98f); break;
                    case SphereId.Pulse: coreRenderer.color = new Color(1f, 0.96f, 0.72f); break;
                    default: coreRenderer.color = Color.white; break;
                }
            }

            // Procedural fallback is only used if a source asset fails to import.
            switch (type)
            {
                case SphereId.Sniper:
                    AddRadialNodes(root.transform, RuntimeSpriteFactory.Diamond, new Color(0.91f, 0.78f, 1f), 2, 0.78f, 0.15f, 11, Mathf.PI * 0.5f);
                    AddOrbitRail(root.transform, new Color(0.83f, 0.62f, 1f, 0.72f), new Vector3(1.52f, 0.30f, 1f), 0f, 6);
                    break;
                case SphereId.Chain:
                    AddRadialNodes(root.transform, RuntimeSpriteFactory.Disc, new Color(1f, 0.92f, 0.46f), 3, 0.78f, 0.17f, 11, Mathf.PI * 0.5f);
                    break;
                case SphereId.Shotgun:
                    AddRadialNodes(root.transform, RuntimeSpriteFactory.Disc, new Color(1f, 0.76f, 0.37f), 5, 0.76f, 0.11f, 11, Mathf.PI * 0.5f);
                    break;
                case SphereId.Aura:
                    AddRadialNodes(root.transform, RuntimeSpriteFactory.Disc, new Color(0.67f, 1f, 0.86f), 4, 0.78f, 0.12f, 11, Mathf.PI * 0.25f);
                    AddOrbitRail(root.transform, new Color(0.34f, 0.95f, 0.76f, 0.45f), new Vector3(1.72f, 0.78f, 1f), 18f, 6);
                    break;
                case SphereId.Prism:
                    AddRadialNodes(root.transform, RuntimeSpriteFactory.Prism, new Color(1f, 0.78f, 0.95f), 3, 0.78f, 0.14f, 11, Mathf.PI * 0.5f);
                    AddOrbitRail(root.transform, new Color(1f, 0.58f, 0.88f, 0.65f), new Vector3(1.55f, 0.38f, 1f), 60f, 6);
                    break;
                case SphereId.Gravity:
                    AddRadialNodes(root.transform, RuntimeSpriteFactory.Hexagon, new Color(0.79f, 0.68f, 1f), 4, 0.79f, 0.12f, 11, Mathf.PI * 0.25f);
                    AddOrbitRail(root.transform, SphereColor(type), new Vector3(1.55f, 0.62f, 1f), 28f, 6);
                    break;
                case SphereId.Pulse:
                    AddRadialNodes(root.transform, RuntimeSpriteFactory.Disc, new Color(1f, 0.9f, 0.54f), 4, 0.78f, 0.13f, 11, Mathf.PI * 0.25f);
                    AddOrbitRail(root.transform, new Color(1f, 0.82f, 0.32f, 0.62f), new Vector3(1.62f, 0.64f, 1f), -45f, 6);
                    break;
                case SphereId.Void:
                    AddRadialNodes(root.transform, RuntimeSpriteFactory.Shard, new Color(0.85f, 0.72f, 1f), 3, 0.79f, 0.14f, 11, Mathf.PI * 0.5f);
                    AddOrbitRail(root.transform, SphereColor(type), new Vector3(1.55f, 0.62f, 1f), -22f, 6);
                    break;
            }
        }


        private static void AddOrbitRail(Transform parent, Color tint, Vector3 scale, float angle, int order)
        {
            var rail = new GameObject("Visual Orbit Rail");
            rail.transform.SetParent(parent, false);
            rail.transform.localScale = scale;
            rail.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            var renderer = rail.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSpriteFactory.Ring;
            renderer.color = new Color(tint.r, tint.g, tint.b, Mathf.Min(tint.a, 0.78f));
            renderer.sortingOrder = order;
        }

        private static void AddRadialNodes(Transform parent, Sprite sprite, Color tint, int count, float radius, float size, int order, float phase = 0f)
        {
            for (var i = 0; i < count; i++)
            {
                var angle = phase + Mathf.PI * 2f * i / count;
                var node = new GameObject("Sphere Signature Node");
                node.transform.SetParent(parent, false);
                node.transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                node.transform.localScale = Vector3.one * size;
                var renderer = node.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = tint;
                renderer.sortingOrder = order;
            }
        }

        private static Sprite GetSphereIcon(SphereId type)
        {
            if (type == SphereId.Standard)
                return RuntimeSpriteFactory.StandardCoreGlow;
            var authored = GetAuthoredSphereArtwork(type);
            if (authored != null) return authored;
            switch (type)
            {
                case SphereId.Sniper: return RuntimeSpriteFactory.Diamond;
                case SphereId.Shotgun: return RuntimeSpriteFactory.Star;
                case SphereId.Chain: return RuntimeSpriteFactory.Hexagon;
                case SphereId.Aura: return RuntimeSpriteFactory.Ring;
                case SphereId.Prism: return RuntimeSpriteFactory.Prism;
                case SphereId.Void: return RuntimeSpriteFactory.Shard;
                case SphereId.Gravity: return RuntimeSpriteFactory.Diamond;
                case SphereId.Pulse: return RuntimeSpriteFactory.Star;
                default: return RuntimeSpriteFactory.Faceted;
            }
        }

        // Retained for the existing compact active-Sphere roster; the new type
        // picker itself uses the authored MobileControls glyphs without text labels.
        private static string SphereShortName(SphereId type)
        {
            switch (type)
            {
                case SphereId.Standard: return "STD";
                case SphereId.Sniper: return "SNP";
                case SphereId.Shotgun: return "SGN";
                case SphereId.Chain: return "CHN";
                case SphereId.Aura: return "AUR";
                case SphereId.Orbital: return "ORB";
                case SphereId.Prism: return "PRS";
                case SphereId.Gravity: return "GRV";
                case SphereId.Pulse: return "PLS";
                case SphereId.Void: return "VOI";
                default: return "???";
            }
        }

        private static void DrawSpriteIcon(Rect rect, Sprite sprite, Color tint)
        {
            if (sprite == null) return;
            var previous = GUI.color;
            GUI.color = tint;
            GUI.DrawTexture(rect, sprite.texture, ScaleMode.ScaleToFit, true);
            GUI.color = previous;
        }

        private GameObject CreateOrb(string name, Vector2 position, float scale, Color tint, Color coreTint, int order)
        {
            var root = new GameObject(name);
            root.transform.position = position;
            root.transform.localScale = Vector3.one * scale;
            var glow = new GameObject("Glow");
            glow.transform.SetParent(root.transform, false);
            glow.transform.localScale = Vector3.one * 1.75f;
            var glowRenderer = glow.AddComponent<SpriteRenderer>();
            glowRenderer.sprite = RuntimeSpriteFactory.Disc;
            glowRenderer.color = new Color(tint.r, tint.g, tint.b, 0.18f);
            glowRenderer.sortingOrder = order - 2;
            var shell = new GameObject("Shell");
            shell.transform.SetParent(root.transform, false);
            var shellRenderer = shell.AddComponent<SpriteRenderer>();
            shellRenderer.sprite = RuntimeSpriteFactory.Ring;
            shellRenderer.color = tint;
            shellRenderer.sortingOrder = order;
            var innerFrame = new GameObject("Inner Frame");
            innerFrame.transform.SetParent(root.transform, false);
            innerFrame.transform.localScale = Vector3.one * 0.66f;
            var frameRenderer = innerFrame.AddComponent<SpriteRenderer>();
            frameRenderer.sprite = RuntimeSpriteFactory.Ring;
            frameRenderer.color = new Color(tint.r * 0.68f, tint.g * 0.78f, tint.b, 0.9f);
            frameRenderer.sortingOrder = order + 1;

            var core = new GameObject("Core Light");
            core.transform.SetParent(root.transform, false);
            core.transform.localScale = Vector3.one * 0.30f;
            var coreRenderer = core.AddComponent<SpriteRenderer>();
            coreRenderer.sprite = RuntimeSpriteFactory.Disc;
            coreRenderer.color = coreTint;
            coreRenderer.sortingOrder = order + 2;
            return root;
        }

        private void SpawnEnemy()
        {
            var angle = _rng.NextFloat() * Mathf.PI * 2f;
            var position = (Vector2)_player.position +
                new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle)) * BossCombatRules.BossSpawnDistanceWorld;

            // Match src/engineEnemies.ts RNG order: base archetype, base visual,
            // role, elite conversion, then elite variant (only when converted).
            var archetype = EnemySpawnRules.SelectBase(_wave, _rng.NextFloat());
            var profile = EnemySpawnRules.GetProfile(archetype);
            var visualVariant = GetEnemyVisualVariant(archetype);
            // src/engineEnemies.ts evaluates the fallback ENEMY_ROLES draw before
            // getRegionEnemyRole(), even when the regional pool replaces that fallback.
            _rng.NextFloat();
            var roleRoll = _rng.NextRegionFloat();
            var role = RegionPocketRules.SelectRole(
                _selectedRegion, _player.position.x, -_player.position.y, roleRoll);
            var roleHp = 1f;
            var roleSpeed = 1f;
            var roleDamage = 1f;
            var roleRadius = 1f;
            switch (role)
            {
                case "swarmer": roleHp = 0.65f; roleSpeed = 1.35f; roleRadius = 0.85f; break;
                case "charger": roleSpeed = 1.25f; roleDamage = 1.10f; break;
                case "tank_guard": roleHp = 1.55f; roleSpeed = 0.72f; roleRadius = 1.15f; break;
                case "ranged": roleSpeed = 0.82f; roleDamage = 1.18f; break;
                case "splitter": roleHp = 0.85f; break;
                case "healer": roleSpeed = 0.90f; roleDamage = 0.80f; break;
                case "bomber": roleDamage = 1.45f; roleSpeed = 0.90f; break;
                case "leech": roleSpeed = 0.95f; roleDamage = 1.05f; break;
                case "sniper": roleSpeed = 0.72f; roleDamage = 1.65f; break;
                case "disruptor": roleSpeed = 0.92f; break;
                case "anchor": roleHp = 1.30f; roleSpeed = 0.65f; roleRadius = 1.10f; break;
                case "phase": roleSpeed = 1.18f; roleRadius = 0.90f; break;
                case "scavenger": roleSpeed = 1.10f; break;
                case "corruptor": roleDamage = 1.20f; break;
            }

            var isPhaseRole = role == "phase";
            var difficulty = CurrentDifficulty;
            var pocket = RegionPocketRules.GetAt(_selectedRegion, _player.position.x, -_player.position.y);
            var challengeHp = GetChallengeEnemyHpMultiplier();
            var challengeSpeed = GetChallengeEnemySpeedMultiplier();
            var challengeXp = GetChallengeEnemyXpMultiplier();

            var hp = (profile.HpBase + _wave * profile.HpPerWave) *
                difficulty.EnemyHpMultiplier * pocket.Hp * challengeHp * roleHp;
            var speed = BossCombatRules.ToWorldUnits(profile.SpeedBase + _wave * profile.SpeedPerWave) *
                difficulty.EnemySpeedMultiplier * pocket.Speed * challengeSpeed * roleSpeed;
            // Source stores the fully modified damage on the enemy at spawn time.
            var damage = (profile.DamageBase + _wave * profile.DamagePerWave) *
                difficulty.EnemyDamageMultiplier * GetRegionEnemyDamageMultiplier() * roleDamage;
            var xpReward = profile.XpReward * pocket.Xp * challengeXp;
            var isElite = EnemySpawnRules.ShouldBeElite(_wave, _rng.NextFloat());
            var eliteVariant = "";
            if (isElite)
            {
                hp *= 3f;
                damage *= 1.5f;
                xpReward *= 5f;
                visualVariant = SelectEliteVariant(out eliteVariant);
            }

            // Source radius is 19/15/26 pixels for normal/fast/tank; elite adds 4.
            var eliteRadiusBonus = isElite ? 4f : 0f;
            var visualArchetype = isElite ? EnemyArchetype.Elite : archetype;
            var visualScale = profile.VisualScale * (profile.Radius + eliteRadiusBonus) / profile.Radius;
            var tint = GetEnemyTint(visualArchetype);
            var go = CreateOrb(GetEnemyName(visualArchetype), position,
                0.72f * visualScale * roleRadius, tint, Color.white, 4);
            ApplyShellVisual(go, GetEnemySprite(visualArchetype),
                visualArchetype == EnemyArchetype.Normal ? 1.12f : 1.02f);
            var enemyCore = go.transform.Find("Core Light");
            if (enemyCore != null) enemyCore.localScale = Vector3.one * (archetype == EnemyArchetype.Tank ? 0.29f : 0.22f);
            AddOrbitRail(go.transform, new Color(tint.r, tint.g, tint.b, 0.65f),
                new Vector3(1.7f, 0.62f, 1f), visualArchetype == EnemyArchetype.Fast ? -28f : 38f, 3);
            switch (visualArchetype)
            {
                case EnemyArchetype.Fast:
                    AddRadialNodes(go.transform, RuntimeSpriteFactory.Diamond, new Color(1f, 0.78f, 0.5f), 2, 0.78f, 0.15f, 7, Mathf.PI * 0.5f);
                    break;
                case EnemyArchetype.Tank:
                    AddOrbitRail(go.transform, new Color(0.88f, 0.65f, 1f, 0.62f), new Vector3(1.36f, 1.12f, 1f), -18f, 5);
                    AddRadialNodes(go.transform, RuntimeSpriteFactory.Hexagon, new Color(0.88f, 0.72f, 1f), 4, 0.8f, 0.13f, 7, Mathf.PI * 0.25f);
                    break;
                case EnemyArchetype.Elite:
                    AddOrbitRail(go.transform, new Color(1f, 0.44f, 0.78f, 0.82f), new Vector3(1.28f, 1.28f, 1f), 0f, 5);
                    AddRadialNodes(go.transform, RuntimeSpriteFactory.Star, new Color(1f, 0.76f, 0.9f), 3, 0.82f, 0.13f, 7, Mathf.PI * 0.5f);
                    break;
            }

            var agent = go.AddComponent<EnemyAgent2D>();
            agent.Initialize(this, _player, hp, speed, xpReward,
                visualVariant, "", isElite, archetype, isPhaseRole,
                role, roleDamage, roleRadius, eliteVariant, damage); agent.SetSourceVisualTier(_bossesDefeated);
            _enemies.Add(agent);
        }
        private bool HasActiveBoss()
        {
            for (var i = 0; i < _enemies.Count; i++)
                if (_enemies[i] != null && _enemies[i].IsBoss && _enemies[i].IsAlive) return true;
            return false;
        }

        private int CountLivingNonBossEnemies()
        {
            var count = 0;
            for (var i = 0; i < _enemies.Count; i++)
                // Source wave selection runs before updateEnemies removes deaths from this frame.
                if (_enemies[i] != null && !_enemies[i].IsBoss) count++;
            return count;
        }

        private void StartNextWave()
        {
            _wave++;
            _waveEnemiesToSpawn = RunBalanceRules.GetEnemiesPerWave(_wave);
            SourceAudio2D.Play("wave");
        }

        private void TickRegionMastery(float dt)
        {
            if (_regionStabilized || _playerCore == null) return;
            var formation = _networkState == null ? null : _networkState.DominantFormation;
            if (formation == null) return;
            _regionMasteryGeometryTypes.Add(formation.Type);
            if (formation.Active) _regionMasteryDominantSeconds += dt;
        }

        private void TickSourceRegionFracture()
        {
            if (_selectedRegion == EchoRegionId.ResonanceBasin && _regionChallengeId == "fractured_network")
            {
                var interval = Mathf.FloorToInt(_runTime / 45f);
                if (interval > _fractureIntervalIndex)
                {
                    _fractureIntervalIndex = interval;
                    var eligibleSpheres = new List<SphereAttackAgent>();
                    for (var sphereIndex = 0; sphereIndex < _spheres.Count; sphereIndex++)
                    {
                        var sphere = _spheres[sphereIndex];
                        if (sphere != null && sphere.gameObject.activeInHierarchy && !sphere.NetworkDisabled)
                            eligibleSpheres.Add(sphere);
                    }
                    if (eligibleSpheres.Count > 0)
                    {
                        var randomIndex = Mathf.Min(eligibleSpheres.Count - 1,
                            Mathf.FloorToInt(_rng.NextRegionFloat() * eligibleSpheres.Count));
                        eligibleSpheres[randomIndex].ApplyNetworkDisable(2.2f);
                        RefreshNetworkState();
                    }
                }
            }
        }

        private void AdvanceRegionBossSchedule()
        {
            if (_regionEndlessMode)
            {
                if (_runTime >= _endlessNextBossAt && !HasActiveBoss())
                {
                    var basinTypes = new[] { "conductor", "architect", "stella_warden" };
                    var riftTypes = new[] { "null", "aura", "stella_warden" };
                    var types = _selectedRegion == EchoRegionId.SpectralRift ? riftTypes : basinTypes;
                    var bossType = types[Mathf.Min(_endlessBossesSpawned, types.Length - 1)];
                    _endlessBossesSpawned++;
                    _endlessNextBossAt += 150f;
                    _sourceRegionBossForFrame = bossType;
                }
                return;
            }
            var signal = RegionBossScheduleRules.Advance(_regionSchedule, _runTime, HasActiveBoss());
            if (signal.Kind == RegionBossSignalKind.SpawnBoss)
            {
                _sourceRegionBossForFrame = signal.BossType;
            }
            else if (signal.Kind == RegionBossSignalKind.RegionCleared)
            {
                _regionStabilized = true;
                PlayerPrefs.SetInt(RegionStabilizedKey(_selectedRegion), 1);
                if (_selectedRegion == EchoRegionId.ResonanceBasin)
                    PlayerPrefs.SetInt(BasinStabilizedPreferenceKey, 1);
                MarkRegionChallengeComplete();
                PlayerPrefs.Save();
                ShowMessage(IsRussianUi ? "РЕГИОН СТАБИЛИЗИРОВАН" : "REGION STABILIZED");
                _regionChallengeContractsCompleted = RegionBossScheduleRules.GetChallengeContractsCompleted(_regionChallengeId);
                _regionMasteryChallengesCompleted = RegionBossScheduleRules.GetMasteryChallengesCompleted(
                    _regionMasteryGeometryTypes.Count, _regionMasteryDominantSeconds, GetActiveCharacterSphereCount());
                _regionGoldReward = RegionBossScheduleRules.GetRegionGoldReward(_selectedRegion, _runEliteKills, _bossesDefeated);
                _regionXpReward = RegionBossScheduleRules.GetRegionXpReward(_level);
            }
        }

        /// <summary>
        /// Invoked only by RegionBossScheduleRules.Advance, matching the source
        /// updateRegion spawnBoss signal instead of using an independent timer.
        /// </summary>
        public EnemyAgent2D SpawnBossForRegionEvent(string forcedBossType, int wave)
        {
            SourceAudio2D.Play("boss");
            if (_player == null || _mainMenu || _gameOver) return null;
            var type = string.IsNullOrEmpty(forcedBossType)
                ? BossCombatRules.GetTypeForDefeatedCount(_bossesDefeated)
                : forcedBossType;
            if (!BossCombatRules.IsKnownBossType(type)) return null;

            var angle = _rng.NextFloat() * Mathf.PI * 2f;
            var position = (Vector2)_player.position +
                new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle)) * BossCombatRules.BossSpawnDistanceWorld;
            var tint = Color.white;
            ColorUtility.TryParseHtmlString(BossCombatRules.GetColorHex(type), out tint);
            var difficulty = CurrentDifficulty;
            var hp = BossCombatRules.GetMaxHp(wave) * difficulty.EnemyHpMultiplier;
            var speed = BossCombatRules.GetMoveSpeedWorldUnitsPerSecond(type, wave) * difficulty.EnemySpeedMultiplier;
            var damage = BossCombatRules.GetContactDamage(wave) * difficulty.EnemyDamageMultiplier;
            var root = CreateOrb("ECHO SPHERE Boss / " + type, position, 1.23f, tint, Color.white, 4);
            var agent = root.AddComponent<EnemyAgent2D>();
            agent.Initialize(this, _player, hp, speed, BossCombatRules.GetXpReward(wave),
                BossCombatRules.GetVisualVariant(type), type); agent.SetSourceVisualTier(Mathf.FloorToInt(wave/10f));
            _enemies.Add(agent);
            var combat = root.AddComponent<BossCombatAgent2D>();
            combat.Initialize(this, _player, agent, type, wave, speed, damage, tint);
            agent.SetBossCombat(combat);
            return agent;
        }

        public EnemyAgent2D SpawnBossMinion(Vector2 position, float hp, float moveSpeed, int xpReward, string visualVariant, Color tint)
        {
            if (_player == null || _gameOver) return null;
            var root = CreateOrb("Boss Minion", position, 0.38f, tint, Color.white, 4);
            var agent = root.AddComponent<EnemyAgent2D>();
            agent.Initialize(this, _player, hp, moveSpeed, xpReward, visualVariant, damage: 8f);
            agent.SetSourceRadius(10f);
            _enemies.Add(agent);
            return agent;
        }

        public void SpawnBossProjectile(Vector2 position, Vector2 velocity, float damage, float sourceRadius, string bossType, Color color, EnemyAgent2D owner = null)
        {
            var projectile = new GameObject("Boss Projectile / " + bossType);
            projectile.transform.position = position;
            var agent = projectile.AddComponent<BossProjectileAgent2D>();
            agent.Initialize(this, _player, velocity, sourceRadius, damage, bossType, color);
            agent.SetSourceOwner(owner);
        }

        public void DisableNearestNetworkSpheres(Vector2 origin, int count, float duration, bool includeCurrentlyDisabled)
        {
            var candidates = new List<SphereAttackAgent>(_spheres.Count);
            for (var i = 0; i < _spheres.Count; i++)
            {
                var sphere = _spheres[i];
                if (sphere == null || !sphere.gameObject.activeInHierarchy) continue;
                if (!includeCurrentlyDisabled && sphere.NetworkDisabled) continue;
                candidates.Add(sphere);
            }
            candidates.Sort((a, b) =>
                ((Vector2)a.transform.position - origin).sqrMagnitude.CompareTo(
                    ((Vector2)b.transform.position - origin).sqrMagnitude));
            var limit = Mathf.Min(Mathf.Max(0, count), candidates.Count);
            var authoredDuration = LevelUpAbilityRules.GetNetworkDisableDuration(duration, GetAbilityLevel(RunUpgradeKind.Stability));
            for (var i = 0; i < limit; i++) candidates[i].ApplyNetworkDisable(authoredDuration);
            if (limit > 0) RefreshNetworkState();
        }

        public void DelayAllSphereAttacks(float amount)
        {
            for (var i = 0; i < _spheres.Count; i++)
                if (_spheres[i] != null) _spheres[i].DelayNextAttack(amount);
        }

        public void DrainResonance(float amount) => _resonanceCharge = Mathf.Max(0f, _resonanceCharge - Mathf.Max(0f, amount));
        public void ShowBossMessage(string message) => ShowMessage(message);

        // Source enemyVisual selection in new-desing varies the authored creature silhouette
        // within each archetype. Use the run seed at spawn time for reproducible variety;
        // presentation animation itself remains free of gameplay RNG.
        private static readonly string[] NormalEnemyVisualVariants = { "wisp", "leech", "serpent", "stalker", "prism" };
        private static readonly string[] FastEnemyVisualVariants = { "moth", "skitter", "stalker" };
        private static readonly string[] TankEnemyVisualVariants = { "beetle", "brute", "prism" };
        private static readonly string[] EliteVariantIds = { "linkbreaker", "resonance_leech", "phantom_hunter", "geometry_shifter", "splitter_prime", "mirror_warden", "stasis_warden", "nullifier", "pyroclast", "scavenger_prime" };
        private static readonly string[] EliteVariantVisuals = { "linkbreaker", "leech", "stalker", "prism", "serpent", "beetle", "wisp", "prism", "brute", "skitter" };

        private string GetEnemyVisualVariant(EnemyArchetype archetype)
        {
            string[] variants;
            switch (archetype)
            {
                case EnemyArchetype.Fast: variants = FastEnemyVisualVariants; break;
                case EnemyArchetype.Tank: variants = TankEnemyVisualVariants; break;
                default: variants = NormalEnemyVisualVariants; break;
            }
            return variants[_rng.NextInt(variants.Length)];
        }

        private string SelectEliteVariant(out string eliteVariant)
        {
            var index = _rng.NextInt(EliteVariantIds.Length);
            eliteVariant = EliteVariantIds[index];
            return EliteVariantVisuals[index];
        }

        private static string GetEnemyName(EnemyArchetype archetype)
        {
            switch (archetype) { case EnemyArchetype.Fast: return "Rift Skirmisher"; case EnemyArchetype.Tank: return "Abyss Bulwark"; case EnemyArchetype.Elite: return "Fracture Elite"; default: return "Echo Wraith"; }
        }
        private static Sprite GetEnemySprite(EnemyArchetype archetype)
        {
            switch (archetype) { case EnemyArchetype.Fast: return RuntimeSpriteFactory.Diamond; case EnemyArchetype.Tank: return RuntimeSpriteFactory.Hexagon; case EnemyArchetype.Elite: return RuntimeSpriteFactory.Star; default: return RuntimeSpriteFactory.Shard; }
        }
        private static Color GetEnemyTint(EnemyArchetype archetype)
        {
            switch (archetype) { case EnemyArchetype.Fast: return new Color(0.831f, 0.659f, 0.188f); case EnemyArchetype.Tank: return new Color(0.541f, 0.353f, 0.541f); case EnemyArchetype.Elite: return new Color(0.722f, 0.278f, 0.353f); default: return new Color(0.290f, 0.478f, 0.541f); }
        }

        public int ActiveEnemyCount => _enemies.Count;

        public EnemyAgent2D GetActiveEnemyAt(int index)
        {
            return index >= 0 && index < _enemies.Count ? _enemies[index] : null;
        }

        public EnemyAgent2D FindNearestEnemy(Vector2 from, float range)
        {
            EnemyAgent2D best = null;
            var bestDistance = range * range;
            for (var i = 0; i < _enemies.Count; i++)
            {
                var e = _enemies[i];
                if (e == null || !e.IsAlive) continue;
                var distance = ((Vector2)e.transform.position - from).sqrMagnitude;
                if (distance < bestDistance) { best = e; bestDistance = distance; }
            }
            return best;
        }

        public SphereAttackAgent FindNearestLinkBreakerTarget(Vector2 from, float rangePixels)
        {
            var range = Mathf.Max(0f, rangePixels) * SpherePlacementRules.SourcePixelsToWorld;
            var bestDistance = range * range;
            SphereAttackAgent best = null;
            for (var i = 0; i < _spheres.Count; i++)
            {
                var sphere = _spheres[i];
                if (sphere == null || !sphere.gameObject.activeInHierarchy || sphere.NetworkDisabled) continue;
                var distance = ((Vector2)sphere.transform.position - from).sqrMagnitude;
                if (distance < bestDistance) { best = sphere; bestDistance = distance; }
            }
            return best;
        }

        public float GetNetworkDisableDuration(float baseDuration)
        {
            return Mathf.Max(0.16f, 1f - GetAbilityLevel(RunUpgradeKind.Stability) * 0.12f) * Mathf.Max(0f, baseDuration);
        }

        public void ShowTimedCombatMessage(string text, float duration, string color = "#39d8ff", string kind = null)
        {
            _message = text ?? "";
            _messageTimer = Mathf.Max(0f, duration);
            _sourceFlashColor = color; _sourceFlashKind = kind;
        }

        public void HealNearbyEnemies(EnemyAgent2D source, float radiusPixels, float maxHpFraction, int maxTargets)
        {
            if (source == null) return;
            var radius = Mathf.Max(0f, radiusPixels) * SpherePlacementRules.SourcePixelsToWorld;
            var radiusSquared = radius * radius;
            var healed = 0;
            for (var i = 0; i < _enemies.Count && healed < Mathf.Max(0, maxTargets); i++)
            {
                var ally = _enemies[i];
                if (ally == null || ally == source || !ally.IsAlive) continue;
                if (((Vector2)ally.transform.position - (Vector2)source.transform.position).sqrMagnitude > radiusSquared) continue;
                ally.HealByFraction(maxHpFraction);
                healed++;
            }
        }

        public bool TryNullifyNearestSphere(Vector2 origin, float rangePixels, float disableDuration)
        {
            var range = Mathf.Max(0f, rangePixels) * SpherePlacementRules.SourcePixelsToWorld;
            var bestDistance = range * range;
            SphereAttackAgent best = null;
            for (var i = 0; i < _spheres.Count; i++)
            {
                var sphere = _spheres[i];
                if (sphere == null || !sphere.gameObject.activeInHierarchy || sphere.NetworkDisabled) continue;
                var distance = ((Vector2)sphere.transform.position - origin).sqrMagnitude;
                if (distance < bestDistance) { bestDistance = distance; best = sphere; }
            }
            if (best == null) return false;
            best.ApplyNetworkDisable(GetNetworkDisableDuration(disableDuration));
            RefreshNetworkState();
            ShowTimedCombatMessage("SPHERE NULLIFIED", 0.8f);
            SpawnEngineerRelayLightning(origin, best.transform.position, 0.25f);
            return true;
        }

        public void TryDisruptNearestSphere(Vector2 origin, float rangePixels)
        {
            var range = Mathf.Max(0f, rangePixels) * SpherePlacementRules.SourcePixelsToWorld;
            var bestDistance = range * range;
            SphereAttackAgent best = null;
            for (var i = 0; i < _spheres.Count; i++)
            {
                var sphere = _spheres[i];
                if (sphere == null || !sphere.gameObject.activeInHierarchy || sphere.NetworkDisabled) continue;
                var distance = ((Vector2)sphere.transform.position - origin).sqrMagnitude;
                if (distance < bestDistance) { bestDistance = distance; best = sphere; }
            }
            if (best == null) return;
            best.ApplyNetworkDisable(GetNetworkDisableDuration(1f));
            RefreshNetworkState();
            ShowTimedCombatMessage("DISRUPTOR", 0.5f);
        }

        public float TimeStopTimer => _timeStopTimer;
        public float TeleportDamageBuffTimer => _teleportDamageBuffTimer;

        // Called by the source-equivalent active-ability handlers once those
        // handlers are ported. Do not synthesize time-control state from unrelated effects.
        public void SetTimeStopTimer(float duration) => _timeStopTimer = Mathf.Max(0f, duration);
        public void SetTeleportDamageBuffTimer(float duration) => _teleportDamageBuffTimer = Mathf.Max(0f, duration);

        public int GetGlobalSphereModifierLevel(string modifier)
        {
            return !string.IsNullOrEmpty(modifier) && _sphereMods.TryGetValue(modifier, out var level) ? level : 0;
        }

        public void SetGlobalSphereModifierLevel(string modifier, int level)
        {
            if (!string.IsNullOrEmpty(modifier) && _sphereMods.ContainsKey(modifier))
                _sphereMods[modifier] = Mathf.Max(0, level);
        }

        public bool HasAnyGlobalSphereModifier()
        {
            foreach (var level in _sphereMods.Values)
                if (level > 0) return true;
            return false;
        }

        public List<EnemyAgent2D> FindNearestEnemies(Vector2 from, float range, int maxCount)
        {
            var found = new List<EnemyAgent2D>();
            var maxDistance = range * range;
            for (var i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive) continue;
                if (((Vector2)enemy.transform.position - from).sqrMagnitude <= maxDistance) found.Add(enemy);
            }
            found.Sort((a, b) =>
            {
                var da = ((Vector2)a.transform.position - from).sqrMagnitude;
                var db = ((Vector2)b.transform.position - from).sqrMagnitude;
                return da.CompareTo(db);
            });
            if (found.Count > maxCount) found.RemoveRange(maxCount, found.Count - maxCount);
            return found;
        }

        public EnemyAgent2D FindNearestUnhitEnemy(Vector2 from, HashSet<int> ignoredIds)
        {
            EnemyAgent2D best = null;
            var bestDistance = float.PositiveInfinity;
            for (var i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive ||
                    (ignoredIds != null && ignoredIds.Contains(enemy.ExistingObjectId()))) continue;
                var distance = ((Vector2)enemy.transform.position - from).sqrMagnitude;
                if (distance < bestDistance) { bestDistance = distance; best = enemy; }
            }
            return best;
        }

        public List<EnemyAgent2D> FindEnemiesInRadius(Vector2 center, float radius)
        {
            var found = new List<EnemyAgent2D>();
            var maxDistance = radius * radius;
            for (var i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (enemy != null && enemy.IsAlive &&
                    ((Vector2)enemy.transform.position - center).sqrMagnitude <= maxDistance)
                    found.Add(enemy);
            }
            return found;
        }

        public EnemyAgent2D TryHitEnemy(Vector2 position, float radius, HashSet<int> ignoredIds = null)
        {
            for (var i = 0; i < _enemies.Count; i++)
            {
                var e = _enemies[i];
                var combined = radius + (e != null ? e.SourceRadius * SpherePlacementRules.SourcePixelsToWorld : 0f);
                if (e != null && e.IsAlive && (ignoredIds == null || !ignoredIds.Contains(e.ExistingObjectId())) &&
                    ((Vector2)e.transform.position - position).sqrMagnitude < combined * combined) return e;
            }
            return null;
        }

        public ProjectileAgent SpawnProjectile(
            Vector2 position, Vector2 direction, float damage, Color color,
            float speed = 8f, int pierce = 0, float critChanceBonus = 0f,
            SphereAttackAgent evolutionOwner = null, int ricochets = 0, float lifetime = 2f,
            bool allowAuthoredProcs = true, bool useEvolutionHitEffects = true, bool applyElementalStatus = true, float sourceRadius = 5f)
        {
            var agent = TransientPools.Projectile();
            var go = agent.gameObject;
            go.transform.position = position;
            agent.Initialize(this, direction, damage, color, speed, pierce, critChanceBonus,
                evolutionOwner, ricochets, lifetime, allowAuthoredProcs, useEvolutionHitEffects, applyElementalStatus, sourceRadius);
            var projectileVfx = go.GetComponent<ProjectileVfx2D>()??go.AddComponent<ProjectileVfx2D>();
            projectileVfx.Initialize(this, direction, speed, color, evolutionOwner);
            return agent;
        }


        public bool RollCombatChance(float chance) => _rng.NextFloat() < Mathf.Clamp01(chance);

        public void HealPlayer(float amount)
        {
            if (_playerCore != null) _playerCore.Heal(Mathf.Max(0f, amount));
        }

        public float ResolveAuthoredSphereHitDamage(SphereId source, EnemyAgent2D target, float damage)
        {
            if (target == null || !target.IsAlive) return 0f;
            var branch = GetSphereBranch(source);
            var finalId = GetSphereFinal(source);

            var execute = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Execute);
            damage *= SphereAuthoredModifierRules.GetExecuteDamageMultiplier(execute, target.HpFraction);
            var mark = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Mark);
            if (mark > 0 && _sphereCharacterId != SphereCharacterId.Hunter && _hunterMarkTarget == target && _hunterMarkTimer > 0f)
            { damage *= 1f + .15f * mark; _hunterMarkTimer = 0f; }
            var corrupt = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Corrupt);
            if (corrupt > 0) damage *= SphereAuthoredModifierRules.GetCorruptDamageMultiplier(corrupt, target.AddCorruptStack());
            return Mathf.Max(0f, damage);
        }

        public void ApplyAuthoredSphereHitEffects(SphereId source, EnemyAgent2D target, float damage,
            bool allowAuthoredProcs, SphereAttackAgent owner = null)
        {
            if (target == null || _gameOver) return;
            var branch = GetSphereBranch(source);
            var finalId = GetSphereFinal(source);
            var origin = owner != null ? (Vector2)owner.transform.position : FindNearestSphereOrigin(source, target.transform.position);
            var shatter = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Shatter);
            if (allowAuthoredProcs && shatter > 0 && target.IsFrozen)
            {
                var nearby = FindEnemiesInRadius(target.transform.position,
                    SphereAuthoredModifierRules.GetShatterRadiusPixels() * SpherePlacementRules.SourcePixelsToWorld);
                var burst = damage * SphereAuthoredModifierRules.GetShatterDamageMultiplier(shatter);
                for (var i = 0; i < nearby.Count; i++)
                    if (nearby[i] != target && nearby[i].IsAlive)
                        nearby[i].ReceiveSphereDamage(source, burst, false, false, owner);
            }

            var impact = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Impact);
            if (impact > 0)
                target.MoveSourceRadial(origin,
                    SphereAuthoredModifierRules.GetImpactDistancePixels(impact) * SpherePlacementRules.SourcePixelsToWorld * GetRegionRepulsionMultiplier());

            var gravitic = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Gravitic);
            if (gravitic > 0)
            {
                var center = (Vector2)target.transform.position;
                var radius = SphereAuthoredModifierRules.GetGraviticRangePixels() * SpherePlacementRules.SourcePixelsToWorld;
                var pullDistance = SphereAuthoredModifierRules.GetGraviticPullDistancePixels(gravitic) * SpherePlacementRules.SourcePixelsToWorld;
                var nearby = FindEnemiesInRadius(center, radius);
                for (var i = 0; i < nearby.Count; i++)
                    if (nearby[i] != target)
                        nearby[i].MoveSourceRadial(center, -pullDistance);
            }

            var anchor = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Anchor);
            if (anchor > 0)
                target.ApplySlow(
                    SphereAuthoredModifierRules.GetAnchorSlowDuration(anchor),
                    SphereAuthoredModifierRules.GetAnchorSlowMultiplier(anchor));

            var vampiric = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Vampiric);
            if (vampiric > 0) HealPlayer(damage * GetSourceSphereModifiers(source).healOnHit);

            var resonant = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Resonant);
            if (resonant > 0 && branch != "pulse_resonator")
                ChargeResonanceFromSource(Mathf.Max(1f, GetSourceSphereModifiers(source).resonantCharge / 2f), false);

            if (allowAuthoredProcs)
            {
                var echoLevel = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Echo);
                var echoChance = GetSourceSphereModifiers(source).echoChance;
                if (echoChance > 0f && RollCombatChance(echoChance))
                    target.ReceiveSphereDamage(source, damage * 0.22f, false, false, owner);

                var afterimageLevel = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Afterimage);
                var afterimageChance = SphereAuthoredModifierRules.GetAfterimageChance(afterimageLevel);
                if (afterimageChance > 0f && RollCombatChance(afterimageChance))
                {
                    var direction = ((Vector2)target.transform.position - origin).normalized;
                    if (direction.sqrMagnitude <= 0.0001f) direction = Vector2.right;
                    var projectile = SpawnProjectile(
                        target.transform.position, direction,
                        damage * SphereAuthoredModifierRules.GetAfterimageDamageMultiplier(afterimageLevel),
                        new Color(0.85f, 0.76f, 1f, 0.9f),
                        250f * SpherePlacementRules.SourcePixelsToWorld, 0, 0f, owner,
                        0, 0.42f, false, false, false, 4f);
                    if (projectile != null) projectile.IgnoreEnemy(target);
                }

                var staticLevel = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Static);
                var staticChance = GetSourceSphereModifiers(source).staticChance;
                if (staticChance > 0f && RollCombatChance(staticChance))
                {
                    var center = (Vector2)target.transform.position;
                    var candidates = FindNearestEnemies(center,
                        110f * SpherePlacementRules.SourcePixelsToWorld, _enemies.Count);
                    EnemyAgent2D next = null;
                    for (var i = 0; i < candidates.Count; i++)
                    {
                        if (candidates[i] == target) continue;
                        next = candidates[i];
                        break;
                    }
                    if (next != null)
                    {
                        SpawnCombatArc(center, next.transform.position, source, branch, finalId,
                            new Color(0.40f, 0.90f, 1f, 0.9f), 0.22f);
                        next.ReceiveSphereDamage(source, damage * 0.35f, false, false, owner);
                    }
                }
            }

            var mark = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Mark);
            if (mark > 0 && _sphereCharacterId != SphereCharacterId.Hunter)
            { _hunterMarkTarget = target; _hunterMarkTimer = Mathf.Max(_hunterMarkTimer, 2.5f); }


        }

        // Source equivalent: engineCombat.ts Echo Architecture block inside
        // dealDamageToEnemy. Preserve its current counter test and first-linked
        // living-node selection; do not increment the source node per hit.
        public void ApplyEchoArchitectureRelayOnSphereHit(SphereAttackAgent source)
        {
            if (source == null || !FormationHitRules.HasCompletedEchoArchitecture(_ownedArtifacts) ||
                !FormationHitRules.ShouldTriggerEchoArchitectureRelay(source.FormationHitCount)) return;

            var sourceIndex = GetSphereIndex(source);
            if (sourceIndex < 0) return;
            var linked = SphereNetworkRules.GetLinkedNodeIndexes(_networkState, sourceIndex);
            for (var i = 0; i < linked.Count; i++)
            {
                var index = linked[i];
                if (index < 0 || index >= _spheres.Count) continue;
                var relay = _spheres[index];
                if (relay == null || !relay.gameObject.activeInHierarchy) continue;

                relay.AddFormationHitCount(FormationHitRules.EchoArchitectureRelayHitIncrement);
                relay.TriggerResonancePulse(FormationHitRules.EchoArchitectureRelayPulseSeconds);
                SpawnEngineerRelayLightning(source.transform.position, relay.transform.position,
                    FormationHitRules.EchoArchitectureRelayLightningSeconds);
                return;
            }
        }

        private Vector2 FindNearestSphereOrigin(SphereId type, Vector2 targetPosition)
        {
            SphereAttackAgent best = null;
            var bestDistance = float.PositiveInfinity;
            for (var i = 0; i < _spheres.Count; i++)
            {
                var sphere = _spheres[i];
                if (sphere == null || sphere.Type != type) continue;
                var distance = ((Vector2)sphere.transform.position - targetPosition).sqrMagnitude;
                if (distance < bestDistance) { bestDistance = distance; best = sphere; }
            }
            return best == null ? targetPosition : (Vector2)best.transform.position;
        }

        public void RegisterSphereHit(SphereAttackAgent owner, EnemyAgent2D target, float damage)
        {
            if (owner == null || target == null) return;
            if (_resonanceEventActive) return;
            AddResonanceChargeFromSource(ResonanceRules.SphereHitCharge, false);
        }

        // Source equivalent: engineLoop.ts activateRune('resonance').
        // The source branch has no live rune-spawn writer at this HEAD; a
        // future Rune pickup must call this method rather than inventing a
        // new spawn cadence here.
        public void ActivateResonanceRune()
        {
            AddResonanceCharge(ResonanceRules.RuneCharge);
            for (var i = 0; i < _spheres.Count; i++)
            {
                var sphere = _spheres[i];
                if (sphere == null) continue;
                sphere.AddFormationHitCount(FormationHitRules.ResonanceRuneHitIncrement);
                sphere.TriggerResonancePulse(FormationHitRules.ResonanceRunePulseSeconds);
            }
        }

        public void AddResonanceCharge(float amount)
        {
            if (_resonanceEventActive) return;
            amount *= LevelUpAbilityRules.GetResonanceGainMultiplier(GetAbilityLevel(RunUpgradeKind.Resonance));
            amount *= RegionBossScheduleRules.GetResonanceMultiplier(
                _selectedRegion, _regionChallengeId, _runTime, _resonanceCharge);
            var events = ResonanceRules.Add(ref _resonanceCharge, amount);
            for (var i = 0; i < events; i++) TriggerResonanceEvent();
        }

        public void ChargeResonanceFromSource(float amount, bool applyFormationEfficiency) =>
            AddResonanceChargeFromSource(amount, applyFormationEfficiency);

        private void AddResonanceChargeFromSource(float amount, bool applyFormationEfficiency)
        {
            if (_resonanceEventActive) return;
            if (applyFormationEfficiency)
                amount *= FormationFollowRules.GetResonanceEfficiency(_follow, _formationStrain);
            AddResonanceCharge(amount);
        }

        public void GrantPlayerShieldCharge(int amount)
        {
            if (_playerCore != null && !_playerCore.IsDead) _playerCore.GrantShieldCharge(amount);
        }

        public bool ConsumeResonanceLineBurst()
        {
            if (_resonanceLineBurst <= 0) return false;
            _resonanceLineBurst = 0;
            return true;
        }

        private string GetFormationKey(SphereNetworkShape formation)
        {
            if (formation == null) return "none";
            var indexes = new List<int>(formation.NodeIndexes);
            indexes.Sort();
            return formation.Type + ":" + string.Join(",", indexes.ConvertAll(index => index.ToString()).ToArray());
        }

        private Vector2 GetFormationCenter(SphereNetworkShape formation)
        {
            if (formation == null || (_spheres.Count == 0 &&
                (GetAbilityLevel(RunUpgradeKind.Minion) < 3 || _abilityDrones.Count == 0)))
                return _player == null ? Vector2.zero : (Vector2)_player.position;
            var center = Vector2.zero;
            var count = 0;
            for (var i = 0; i < formation.NodeIndexes.Count; i++)
            {
                if (!TryGetNetworkNodePosition(formation.NodeIndexes[i], out var position)) continue;
                center += position;
                count++;
            }
            return count > 0 ? center / count : (_player == null ? Vector2.zero : (Vector2)_player.position);
        }

        private bool TryGetNetworkNodePosition(int index, out Vector2 position)
        {
            position = Vector2.zero;
            if (index < 0) return false;
            if (index < _spheres.Count)
            {
                var sphere = _spheres[index];
                if (sphere == null || !sphere.gameObject.activeInHierarchy) return false;
                position = sphere.transform.position;
                return true;
            }
            if (GetAbilityLevel(RunUpgradeKind.Minion) < 3) return false;
            var droneIndex = index - _spheres.Count;
            if (droneIndex < 0 || droneIndex >= _abilityDrones.Count) return false;
            var drone = _abilityDrones[droneIndex];
            if (drone == null || !drone.IsNetworkNodeAlive) return false;
            position = drone.NetworkPosition;
            return true;
        }

        private SphereNetworkShape FindFormationCandidate(SphereNetworkFormation type)
        {
            if (_networkState == null) return null;
            for (var i = 0; i < _networkState.FormationCandidates.Count; i++)
                if (_networkState.FormationCandidates[i].Type == type) return _networkState.FormationCandidates[i];
            return null;
        }

        private void DamageEnemiesInRadius(Vector2 center, float radius, float damage)
        {
            var targets = FindEnemiesInRadius(center, radius);
            for (var i = 0; i < targets.Count; i++) targets[i].ReceiveDamage(damage);
        }

        private void ApplyConductorResonanceEffects(SphereNetworkFormation type, Vector2 center, float baseDamage)
        {
            if (_sphereCharacterId != SphereCharacterId.Conductor) return;

            _conductorOverdriveTimer = Mathf.Max(_conductorOverdriveTimer,
                ConductorOverdriveRules.GetOverdriveDurationSeconds(_characterMasteryLevel));

            var nodeIndexes = new List<int>();
            var formation = _networkState != null ? _networkState.DominantFormation : null;
            if (formation != null && formation.Active && formation.Type == type)
            {
                for (var i = 0; i < formation.NodeIndexes.Count; i++)
                {
                    var index = formation.NodeIndexes[i];
                    if (index >= 0 && index < _spheres.Count && _spheres[index] != null &&
                        _spheres[index].gameObject.activeInHierarchy)
                        nodeIndexes.Add(index);
                }
            }

            if (_characterMasteryLevel >= 3)
            {
                switch (type)
                {
                    case SphereNetworkFormation.Line:
                        _resonanceLineBurst = Mathf.Max(_resonanceLineBurst,
                            ConductorOverdriveRules.GetLineSurgeCharges(_characterMasteryLevel));
                        break;

                    case SphereNetworkFormation.Triangle:
                    {
                        EnemyAgent2D nearest = null;
                        var nearestDistanceSq = float.PositiveInfinity;
                        for (var i = 0; i < _enemies.Count; i++)
                        {
                            var enemy = _enemies[i];
                            if (enemy == null || !enemy.IsAlive) continue;
                            var delta = (Vector2)enemy.transform.position - center;
                            var distanceSq = delta.sqrMagnitude;
                            if (distanceSq >= nearestDistanceSq) continue;
                            nearest = enemy;
                            nearestDistanceSq = distanceSq;
                        }
                        if (nearest != null)
                            nearest.ReceiveDamage(baseDamage *
                                ConductorOverdriveRules.GetTriangleDischargeDamageMultiplier(_characterMasteryLevel));

                        if (nodeIndexes.Count >= 2)
                        {
                            var from = (Vector2)_spheres[nodeIndexes[0]].transform.position;
                            var to = (Vector2)_spheres[nodeIndexes[1]].transform.position;
                            SpawnEngineerRelayLightning(from, to, 0.28f);
                        }
                        break;
                    }

                    case SphereNetworkFormation.Cluster:
                        for (var i = 0; i < nodeIndexes.Count; i++)
                        {
                            var sphere = _spheres[nodeIndexes[i]];
                            sphere.AccelerateNextAttack(
                                ConductorOverdriveRules.GetClusterAttackAdvanceSeconds(_characterMasteryLevel));
                            sphere.TriggerResonancePulse(0.42f);
                        }
                        break;

                    case SphereNetworkFormation.Square:
                        GrantPlayerShieldCharge(ConductorOverdriveRules.GetSquareShieldChargeBonus(_characterMasteryLevel));
                        break;
                }
            }

            if (!ConductorOverdriveRules.ShouldTriggerCascade(_characterMasteryLevel, _resonanceEventsTriggered))
                return;

            var targetCount = ConductorOverdriveRules.GetCascadeTargetCount(_characterMasteryLevel);
            var radius = 170f * SpherePlacementRules.SourcePixelsToWorld;
            var radiusSq = radius * radius;
            var hitCount = 0;
            // The source takes the first qualifying living enemies in the
            // engine list, rather than re-sorting the Cascade by distance.
            for (var i = 0; i < _enemies.Count && hitCount < targetCount; i++)
            {
                var enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive ||
                    ((Vector2)enemy.transform.position - center).sqrMagnitude > radiusSq) continue;
                enemy.ReceiveDamage(baseDamage * ConductorOverdriveRules.GetCascadeDamageMultiplier());
                hitCount++;
            }
        }

        private void TriggerResonanceEvent()
        {
            if (_resonanceEventActive) return;
            _resonanceEventActive = true;
            _resonanceEventsTriggered++;
            try
            {
                var formation = _networkState == null ? null : _networkState.DominantFormation;
                if (formation != null && !formation.Active) formation = null;
                var type = formation == null ? SphereNetworkFormation.None : formation.Type;
                var center = GetFormationCenter(formation);
                var baseDamage = 16f + _level * 2f;
                ApplyConductorResonanceEffects(type, center, baseDamage);

                if (type == SphereNetworkFormation.Fractal)
                {
                    var replay = FindFormationCandidate(SphereNetworkFormation.Lattice)
                        ?? FindFormationCandidate(SphereNetworkFormation.Ring)
                        ?? FindFormationCandidate(SphereNetworkFormation.Square)
                        ?? FindFormationCandidate(SphereNetworkFormation.Triangle)
                        ?? formation;
                    for (var i = 0; i < replay.NodeIndexes.Count; i++)
                    {
                        var index = replay.NodeIndexes[i];
                        if (index < 0 || index >= _spheres.Count || _spheres[index] == null) continue;
                        var sphere = _spheres[index];
                        sphere.AccelerateNextAttack(0.35f);
                        sphere.TriggerResonancePulse(0.45f);
                        SpawnImpact(sphere.transform.position, new Color(1f, 0.72f, 0.28f, 0.95f));
                        DamageEnemiesInRadius(sphere.transform.position, 0.95f, baseDamage * 0.55f);
                    }
                    DamageEnemiesInRadius(center, 1.55f, baseDamage * 0.65f);
                    ShowMessage("FRACTAL ECHO");
                }
                else if (type == SphereNetworkFormation.Lattice)
                {
                    for (var i = 0; i < formation.NodeIndexes.Count; i++)
                    {
                        var index = formation.NodeIndexes[i];
                        if (index < 0 || index >= _spheres.Count || _spheres[index] == null) continue;
                        _spheres[index].AccelerateNextAttack(0.55f);
                        _spheres[index].TriggerResonancePulse(0.40f);
                        SpawnImpact(_spheres[index].transform.position, new Color(0.22f, 0.82f, 1f, 0.9f));
                    }
                    DamageEnemiesInRadius(center, 1.45f, baseDamage * 0.95f);
                    ShowMessage("LATTICE CASCADE");
                }
                else if (type == SphereNetworkFormation.Ring)
                {
                    _resonanceRingTimer = 2.4f;
                    _resonanceRingPulseTimer = 0f;
                    _resonanceRingCursor = 0;
                    _resonanceRingNodes.Clear();
                    _resonanceRingNodes.AddRange(formation.NodeIndexes);
                    ShowMessage("RING LOOP");
                }
                else if (type == SphereNetworkFormation.Square)
                {
                    GrantPlayerShieldCharge(2);
                    DamageEnemiesInRadius(center, 1.50f, baseDamage * 1.20f);
                    SpawnImpact(center, new Color(1f, 0.70f, 0.28f, 0.95f));
                    ShowMessage("SQUARE RESONANCE");
                }
                else if (type == SphereNetworkFormation.Triangle)
                {
                    var nodes = new List<int>();
                    for (var i = 0; i < formation.NodeIndexes.Count; i++)
                    {
                        var index = formation.NodeIndexes[i];
                        if (index >= 0 && index < _spheres.Count && _spheres[index] != null) nodes.Add(index);
                    }
                    var targets = FindNearestEnemies(center, 100f, nodes.Count);
                    var usedTargets = new HashSet<int>();
                    for (var i = 0; i < nodes.Count; i++)
                    {
                        var nodeIndex = nodes[i];
                        var linked = SphereNetworkRules.GetLinkedNodeIndexes(_networkState, nodeIndex);
                        var nextIndex = -1;
                        for (var j = 0; j < linked.Count; j++)
                            if (nodes.Contains(linked[j])) { nextIndex = linked[j]; break; }
                        if (nextIndex < 0 && nodes.Count > 1) nextIndex = nodes[(i + 1) % nodes.Count];
                        var from = (Vector2)_spheres[nodeIndex].transform.position;
                        _spheres[nodeIndex].TriggerResonancePulse(0.38f);
                        SpawnImpact(from, new Color(0.72f, 0.40f, 0.86f, 0.9f));
                        if (nextIndex >= 0 && nextIndex < _spheres.Count && _spheres[nextIndex] != null)
                            SpawnImpact(Vector2.Lerp(from, _spheres[nextIndex].transform.position, 0.5f), new Color(0.72f, 0.40f, 0.86f, 0.95f));
                        if (i < targets.Count && targets[i] != null && usedTargets.Add(targets[i].ExistingObjectId()))
                            targets[i].ReceiveDamage(baseDamage * 0.65f);
                    }
                    ShowMessage("TRIANGLE RESONANCE");
                }
                else if (type == SphereNetworkFormation.Cluster)
                {
                    var targets = FindEnemiesInRadius(center, 1.35f);
                    for (var i = 0; i < targets.Count; i++)
                    {
                        targets[i].KnockBackFrom(center, 0.36f);
                        targets[i].ReceiveDamage(baseDamage * 0.90f);
                    }
                    ShowMessage("CLUSTER RESONANCE");
                }
                else if (type == SphereNetworkFormation.Line)
                {
                    _resonanceLineBurst = Mathf.Max(_resonanceLineBurst, 1);
                    ShowMessage("LINE RESONANCE");
                }
                else
                {
                    DamageEnemiesInRadius(center, 0.90f, baseDamage * 0.60f);
                    ShowMessage("RESONANCE");
                }
            }
            finally
            {
                _resonanceEventActive = false;
            }
        }

        private void TickResonanceRing(float deltaTime)
        {
            if (_resonanceRingTimer <= 0f) return;
            _resonanceRingTimer = Mathf.Max(0f, _resonanceRingTimer - deltaTime);
            _resonanceRingPulseTimer -= deltaTime;
            if (_resonanceRingPulseTimer > 0f) return;

            var nodes = new List<int>();
            for (var i = 0; i < _resonanceRingNodes.Count; i++)
            {
                var index = _resonanceRingNodes[i];
                if (index >= 0 && index < _spheres.Count && _spheres[index] != null) nodes.Add(index);
            }
            if (nodes.Count == 0)
            {
                _resonanceRingTimer = 0f;
                return;
            }

            _resonanceRingPulseTimer = 0.42f;
            var cursor = _resonanceRingCursor % nodes.Count;
            var nodeIndex = nodes[cursor];
            var nextIndex = nodes[(cursor + 1) % nodes.Count];
            _resonanceRingCursor = (cursor + 1) % nodes.Count;
            var sphere = _spheres[nodeIndex];
            sphere.AccelerateNextAttack(0.30f);
            sphere.TriggerResonancePulse(0.40f);
            SpawnImpact(sphere.transform.position, new Color(0.32f, 0.90f, 0.60f, 0.9f));
            if (_spheres[nextIndex] != null)
                SpawnImpact(Vector2.Lerp(sphere.transform.position, _spheres[nextIndex].transform.position, 0.5f), new Color(0.32f, 0.90f, 0.60f, 0.85f));
            DamageEnemiesInRadius(sphere.transform.position, 1.05f, (16f + _level * 2f) * 0.42f);
        }

        public void TriggerSniperOracleSplash(EnemyAgent2D primary, float damage, float radius)
        {
            if (primary == null) return;
            var targets = FindEnemiesInRadius(primary.transform.position, radius);
            for (var i = 0; i < targets.Count; i++) if (targets[i] != primary) targets[i].ReceiveDamage(damage);
            SpawnImpact(primary.transform.position, new Color(0.76f, 0.58f, 1f, 0.9f));
        }

        public void SpawnVoidShards(EnemyAgent2D source, float damage, int count, SphereAttackAgent owner = null)
        {
            if (source == null || count <= 0) return;
            var origin = (Vector2)source.transform.position;
            for (var i = 0; i < count; i++)
            {
                var angle = i * Mathf.PI * 2f / count;
                var direction = new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle));
                SpawnProjectile(origin, direction, damage, new Color32(143, 99, 255, 255), 320f * SpherePlacementRules.SourcePixelsToWorld, 0, 0f, owner, 0, .65f, false, false, false, 4f);
            }
            SpawnImpact(origin, new Color(0.62f, 0.40f, 1f, 0.9f));
        }

        public void TriggerOrbitalAfterimage(EnemyAgent2D primary, float damage, float radius)
        {
            if (primary == null) return;
            var center = (Vector2)primary.transform.position;
            var targets = FindEnemiesInRadius(center, radius);
            for (var i = 0; i < targets.Count; i++)
                if (targets[i] != primary) targets[i].ReceiveDamage(damage * 0.65f);
            primary.ReceiveDamage(damage);
            SpawnImpact(center, new Color(0.52f, 0.92f, 1f, 0.85f));
        }

        public void ApplyGravityWellControl(Vector2 center, float radius, float pullDistance, float slowDuration, float slowMultiplier)
        {
            var targets = FindEnemiesInRadius(center, radius);
            for (var i = 0; i < targets.Count; i++)
            {
                if (targets[i].IsPhaseRole) continue;
                targets[i].PullByDistance(center, pullDistance);
                if (slowDuration > 0f) targets[i].ApplySlow(slowDuration, slowMultiplier);
            }
        }

        public void ApplyGravityTidePulse(Vector2 center, float radius, float distance, int mode)
        {
            var targets = FindEnemiesInRadius(center, radius);
            for (var i = 0; i < targets.Count; i++)
            {
                if (targets[i].IsPhaseRole) continue;
                if (mode < 0) targets[i].KnockBackFrom(center, distance);
                else targets[i].PullByDistance(center, distance);
            }
            SpawnImpact(center, new Color(0.65f, 0.55f, 1f, 0.9f));
        }

        public void TriggerGravityCollapse(Vector2 center, float damage, float radius)
        {
            var targets = FindEnemiesInRadius(center, radius);
            for (var i = 0; i < targets.Count; i++) targets[i].ReceiveDamage(damage);
            SpawnImpact(center, new Color(0.75f, 0.58f, 1f, 0.95f));
        }

        public void AccelerateNearbySpheres(SphereAttackAgent source, float radius, float timerReduction)
        {
            var radiusSquared = radius * radius;
            for (var i = 0; i < _spheres.Count; i++)
            {
                var sphere = _spheres[i];
                if (sphere == null || sphere == source) continue;
                if (((Vector2)sphere.transform.position - (Vector2)source.transform.position).sqrMagnitude <= radiusSquared)
                    sphere.AccelerateNextAttack(timerReduction);
            }
        }

        public void ApplyAuraGravityControl(Vector2 center, float radius, float pullDistance, bool applySlow)
        {
            var targets = FindEnemiesInRadius(center, radius);
            for (var i = 0; i < targets.Count; i++)
            {
                targets[i].PullByDistance(center, pullDistance);
                if (applySlow) targets[i].ApplySlow(0.8f, 0.72f);
            }
        }

        public void TriggerChainStorm(SphereAttackAgent source, EnemyAgent2D primary, float directDamage, int level, string finalId)
        {
            if (primary == null) return;
            var center = (Vector2)primary.transform.position;
            var radius = SphereEvolutionCombatRules.GetChainStormRadius(level, finalId);
            var splash = directDamage * SphereEvolutionCombatRules.GetChainStormSplash(level, finalId);
            var targets = FindEnemiesInRadius(center, radius);
            for (var i = 0; i < targets.Count; i++)
            {
                if (targets[i] == primary) continue;
                SpawnCombatArc(center, targets[i].transform.position, SphereId.Chain,
                    GetSphereBranch(SphereId.Chain), finalId, new Color(1f, 0.88f, 0.35f, 0.96f), 0.28f);
                targets[i].ReceiveDamage(splash);
                SpawnImpact(Vector2.Lerp(center, targets[i].transform.position, 0.5f), new Color(1f, 0.88f, 0.35f, 0.75f));
            }
            SpawnImpact(center, new Color(1f, 0.88f, 0.35f, 0.9f));
        }

        public void TriggerChainStormExtraStrike(EnemyAgent2D primary, float damage)
        {
            if (primary == null) return;
            var target = FindNearestEnemy(primary.transform.position, 5.5f);
            if (target == null || target == primary)
            {
                var candidates = FindNearestEnemies(primary.transform.position, 5.5f, 2);
                for (var i = 0; i < candidates.Count; i++)
                    if (candidates[i] != primary) { target = candidates[i]; break; }
            }
            if (target == null || target == primary) return;
            SpawnCombatArc(primary.transform.position, target.transform.position, SphereId.Chain,
                GetSphereBranch(SphereId.Chain), GetSphereFinal(SphereId.Chain),
                new Color(1f, 0.92f, 0.45f, 0.96f), 0.28f);
            SpawnImpact(Vector2.Lerp(primary.transform.position, target.transform.position, 0.5f), new Color(1f, 0.92f, 0.45f, 0.9f));
            target.ReceiveDamage(damage);
        }

        public void TriggerShotgunCataclysm(EnemyAgent2D primary, float directDamage, int level, string finalId)
        {
            if (primary == null) return;
            var radius = SphereEvolutionCombatRules.GetShotgunCataclysmRadius(level, finalId);
            var splash = SphereEvolutionCombatRules.GetShotgunCataclysmSplash(level, finalId);
            var center = (Vector2)primary.transform.position;
            var targets = FindEnemiesInRadius(center, radius);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == primary) continue;
                target.ReceiveDamage(directDamage * splash);
            }
            SpawnImpact(center, new Color(0.85f, 0.28f, 0.24f, 0.92f));
            var effect = new GameObject("Cataclysm Shatter");
            effect.transform.position = center;
            effect.transform.localScale = Vector3.one * radius * 1.8f;
            var sr = effect.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSpriteFactory.Ring;
            sr.color = new Color(1f, 0.3f, 0.22f, 0.55f);
            sr.sortingOrder = 17;
            effect.AddComponent<PulseEffect2D>().Initialize(sr.color, 0.24f, 1.15f);
            if (finalId == "shotgun_cataclysm_final_3")
            {
                primary.ApplySlow(0.8f, 0.65f);
                for (var i = 0; i < targets.Count; i++)
                    if (targets[i] != primary) targets[i].ApplySlow(0.8f, 0.65f);
            }
        }

        public void TriggerShotgunHail(EnemyAgent2D primary, Vector2 incomingDirection, float directDamage, int level, string finalId, int shardCount)
        {
            if (primary == null || shardCount <= 0) return;
            var origin = (Vector2)primary.transform.position;
            var radius = SphereEvolutionCombatRules.GetShotgunHailRadius(level, finalId);
            var shardDamage = directDamage * SphereEvolutionCombatRules.GetShotgunHailShardDamageMultiplier(finalId);
            var targets = FindNearestEnemies(origin, 5.5f, shardCount + 2);
            var guided = finalId == "shotgun_hail_final_3";
            for (var i = 0; i < shardCount; i++)
            {
                Vector2 direction;
                if (guided && i < targets.Count && targets[i] != primary)
                    direction = ((Vector2)targets[i].transform.position - origin).normalized;
                else
                {
                    var angle = (i / (float)shardCount) * Mathf.PI * 2f;
                    if (incomingDirection.sqrMagnitude > 0.001f && !guided)
                        angle += Mathf.Atan2(incomingDirection.y, incomingDirection.x);
                    direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                }
                SpawnProjectile(origin, direction, shardDamage, new Color(1f, 0.69f, 0.30f), 9f, guided ? 1 : 0, 0f);
            }
            var nearby = FindEnemiesInRadius(origin, radius);
            var splash = directDamage * SphereEvolutionCombatRules.GetShotgunHailSplashMultiplier(finalId);
            for (var i = 0; i < nearby.Count; i++)
                if (nearby[i] != primary) nearby[i].ReceiveDamage(splash);
            SpawnImpact(origin, new Color(1f, 0.69f, 0.30f, 0.9f));
        }

        public void TriggerStandardResonatorPulse(Vector2 center, float damage, float radius, string finalId)
        {
            var targets = FindEnemiesInRadius(center, radius);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (finalId == "standard_resonator_final_2")
                    target.KnockBackFrom(center, 0.85f);
                else if (finalId == "standard_resonator_final_3")
                    target.ApplySlow(0.8f, 0.55f);
                target.ReceiveDamage(damage);
            }
            SpawnImpact(center, new Color(0.32f, 0.9f, 1f, 0.9f));
        }

        public void TriggerStandardSingularity(EnemyAgent2D primary, float pullDistance, float slowDuration)
        {
            if (primary == null) return;
            var center = (Vector2)primary.transform.position;
            var targets = FindEnemiesInRadius(center, 1.65f);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                target.PullToward(center, pullDistance);
                target.ApplySlow(slowDuration, 0.68f);
            }
            SpawnImpact(center, new Color(0.68f, 0.52f, 1f, 0.85f));
        }

        public void SpawnStandardSwarmShards(EnemyAgent2D hitTarget, Vector2 incomingDirection, int count, float damage, SphereAttackAgent owner = null)
        {
            if (hitTarget == null || count <= 0) return;
            var origin = (Vector2)hitTarget.transform.position;
            var targets = FindNearestEnemies(origin, float.PositiveInfinity, int.MaxValue);
            var other = targets.Find(enemy => enemy != hitTarget);
            var baseAngle = other != null ? Mathf.Atan2(other.transform.position.y - origin.y, other.transform.position.x - origin.x) : -_rng.NextFloat() * Mathf.PI * 2f;
            var final = GetSphereFinal(SphereId.Standard);
            var angles = final == "standard_swarm_final_1" ? new[] { baseAngle } : final == "standard_swarm_final_2" ? new[] { baseAngle + .55f, baseAngle - .55f } : new[] { baseAngle + .7f, baseAngle, baseAngle - .7f };
            for (var i = 0; i < count; i++)
            {
                var angle = i < angles.Length ? angles[i] : -_rng.NextFloat() * Mathf.PI * 2f;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var shard = SpawnProjectile(origin, direction, damage, new Color32(212, 148, 61, 255), 320f * SpherePlacementRules.SourcePixelsToWorld, final == "standard_swarm_final_3" ? 1 : 0, 0f, owner, 0, .55f, false, false, false, 4f);
                shard.IgnoreEnemy(hitTarget);
            }
        }

        public void SpawnEngineerRelayLightning(Vector2 from, Vector2 to, float duration = 0.12f) { AddSourceLightningVisual(from,to,duration); }

        public void SpawnCombatArc(Vector2 from, Vector2 to, SphereId family, string branch, string finalId, Color color, float duration = 0.30f) { AddSourceLightningVisual(from,to,duration,family); }

        public void SpawnImpact(Vector2 position, Color color)
        {
            var go = new GameObject("Impact Pulse");
            go.transform.position = position;
            var impactVfx = go.AddComponent<ImpactVfx2D>();
            impactVfx.Initialize(color);
        }

        private void SpawnStellaChest(Vector2 position)
        {
            var root = new GameObject("STELLA Reward Cache");
            root.transform.position = position;
            root.AddComponent<StellaChestAgent2D>().Initialize(this, position);
        }

        public void CollectStellaChest(StellaChestAgent2D chest)
        {
            if (chest == null || _gameOver || (!_simulationFrame && (_stellaRewardPending || _levelUp || _choosingEvolution))) return;
            Destroy(chest.gameObject);
            _stellaRewardChoices.Clear();
            var count = ArtifactRewardRules.GetStellaChoiceCount(_ownedArtifacts);
            if (ArtifactRewardRules.IsLegendaryStellaWindow(_runTime))
                _stellaRewardChoices.AddRange(ArtifactRewardRules.PickChoices(_ownedArtifacts, _rng.NextFloat, true, count));
            _stellaRewardIsLegendary = _stellaRewardChoices.Count > 0;
            if (_stellaRewardChoices.Count == 0)
                _stellaRewardChoices.AddRange(ArtifactRewardRules.PickChoices(_ownedArtifacts, _rng.NextFloat, false,
                    ArtifactRewardRules.BaseStellaChoiceCount));
            if (_stellaRewardChoices.Count == 0) { ShowMessage("STELLA CACHE EMPTY"); return; }
            _stellaRewardPending = true;
            _userPaused = false;
            _paused = true;
            ShowTimedCombatMessage("STELLA",1.2f,"#ffb84d");
            SourceAudio2D.Play("chest");
        }

        private void ChooseStellaReward(int choiceIndex)
        {
            if (!_stellaRewardPending || choiceIndex < 0 || choiceIndex >= _stellaRewardChoices.Count) return;
            var choice = _stellaRewardChoices[choiceIndex];
            if (choice == null || _ownedArtifacts.Contains(choice.Id)) return;
            _ownedArtifacts.Add(choice.Id);
            RecordDiscoveredArtifactProtocols();
            if (choice.GetEffect("maxHp") > 0f && _playerCore != null) _playerCore.IncreaseMaxHp(choice.GetEffect("maxHp"));
            if (_playerCore != null)
                _playerCore.SetDashCooldownMultiplier(Mathf.Max(0.55f, 1f + ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "cooldown")));
            _stellaRewardPending = false;
            _stellaRewardChoices.Clear();
            _paused = false;
            _userPaused = false;
            SourceAudio2D.Play("chest");
            OpenLevelUpIfReady();
        }

        public void CollectHealthPack(Vector2? position = null)
        {
            if (_playerCore == null || _gameOver) return;
            _playerCore.Heal(20f);
            for (var i = 0; i < 10; i++) SpawnSourceParticle(position ?? PlayerPosition,
                new Vector2((_rng.NextFloat() * 2f - 1f) * 100f, -(_rng.NextFloat() * 2f - 1f) * 100f), .5f, 3f, "#5a8c4a", .5f);
        }

        public void CollectArtifactChest(ArtifactChestAgent2D chest)
        {
            if (chest == null || _gameOver || (!_simulationFrame && (_stellaRewardPending || _levelUp || _choosingEvolution))) return;
            Destroy(chest.gameObject);
            _runChestOpens++;
            _stellaRewardChoices.Clear();
            _stellaRewardChoices.AddRange(ArtifactRewardRules.PickChoices(
                _ownedArtifacts, _rng.NextFloat, false, ArtifactRewardRules.BaseStellaChoiceCount));
            if (_stellaRewardChoices.Count == 0)
            {
                ShowMessage("ARTIFACT CACHE EMPTY");
                return;
            }
            _stellaRewardIsLegendary = false;
            _stellaRewardPending = true;
            _userPaused = false;
            _paused = true;

        }

        public void ActivateRunePickup(string runeType, Vector2 position)
        {
            if (_gameOver || _playerCore == null) return;
            var scale = SpherePlacementRules.SourcePixelsToWorld;
            switch (runeType)
            {
                case "overdrive":
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy)
                            sphere.AccelerateNextAttack(sphere.AttackDelay * 0.75f);
                    break;
                case "phase":
                    _playerCore.GrantInvulnerability(1f);
                    break;
                case "harvest":
                    for (var i = _simulationAgents.Count - 1; i >= 0; i--)
                        if (_simulationAgents[i] is ExperienceOrbAgent orb && orb != null && Vector2.Distance(orb.transform.position,position) <= 450f*scale) orb.CollectForRune();
                    break;
                case "purge":
                    foreach (var enemy in _enemies)
                        if (enemy != null && enemy.IsAlive && !enemy.IsBoss &&
                            Vector2.Distance(enemy.transform.position, position) <= 240f * scale)
                            enemy.ReceiveDamage(45f + _level * 4f);
                    break;
                case "resonance":
                    AddResonanceCharge(60f);
                    foreach (var sphere in _spheres)
                    {
                        if (sphere == null || !sphere.gameObject.activeInHierarchy) continue;
                        sphere.AddFormationHitCount(2);
                        sphere.TriggerResonancePulse(0.5f);
                    }
                    break;
                case "fortify":
                    _playerCore.GrantShieldCharge(2);
                    break;
                case "hunt":
                {
                    EnemyAgent2D target = null;
                    var nearest = float.PositiveInfinity;
                    foreach (var enemy in _enemies)
                    {
                        if (enemy == null || !enemy.IsAlive || (!enemy.IsElite && !enemy.IsBoss)) continue;
                        var distance = Vector2.Distance(enemy.transform.position, position);
                        if (distance >= nearest) continue;
                        target = enemy;
                        nearest = distance;
                    }
                    if (target != null)
                    {
                        _hunterMarkTarget = target;
                        _hunterMarkTimer = 8f;
                        target.ApplyRuneMark(8f);
                    }
                    break;
                }
                case "echo":
                    _buffTimer = Mathf.Max(_buffTimer, 4f);
                    break;
                case "gravity":
                    foreach (var enemy in _enemies)
                    {
                        if (enemy == null || !enemy.IsAlive) continue;
                        var delta = position - (Vector2)enemy.transform.position;
                        var distance = delta.magnitude;
                        if (distance <= 0.0001f || distance > 240f * scale) continue;
                        var pull = Mathf.Min(90f * scale, distance * 0.55f);
                        enemy.transform.position += (Vector3)(delta / distance * pull);
                    }
                    break;
            }
            SpawnImpact(position, RuneAgent2D.GetRuneColor(runeType));
            ShowTimedCombatMessage(RuneAgent2D.GetRuneNameRussian(runeType), 1f);
        }

        public void RegisterEnemyDeath(EnemyAgent2D enemy, Vector2 position, float xpReward = 1f, SphereId? sourceSphere = null)
        {
            if (enemy != null) RemoveSourceEnemyProjectiles(enemy);
            var drain = sourceSphere.HasValue ? GetSphereAuthoredModifierLevel(sourceSphere.Value, SphereAuthoredModifier.Drain) : GetGlobalSphereModifierLevel("drain");
            if (drain > 0 && _playerCore != null) HealPlayer(_playerCore.MaxHp * 0.03f * drain);
            if (enemy == null || !enemy.IsBoss) _kills++;
            if (_sphereCharacterId == SphereCharacterId.Hunter && enemy != null)
            {
                if (enemy.IsElite && _characterMasteryLevel >= 5 &&
                    _hunterMarkTarget == enemy && _hunterMarkTimer > 0f)
                    _hunterTrophyTimer = 4f;
                if (_hunterMarkTarget == enemy)
                {
                    _hunterMarkTarget = null;
                    _hunterMarkTimer = 0f;
                    _hunterHitCount = 0;
                }
                if (_hunterHuntTarget == enemy)
                {
                    _hunterHuntTarget = null;
                    _hunterHuntTimer = 0f;
                }
            }
            if (enemy != null && !enemy.IsBoss)
            {
                _combo = ComboRules.IncrementOnKill(_combo, false);
                _comboTimer = ComboRules.WindowSeconds;
                _comboMultiplier = ComboRules.GetExperienceMultiplier(_combo);
            }
            ApplySourceEnemyDeathEffects(enemy);
            if (_ownedArtifacts.Contains("swift_boots")) _swiftBootsTimer = 3f;
            if (enemy != null && enemy.IsBoss)
            {
                _bossesDefeated++;
                SpawnStellaChest(position);
                ShowTimedCombatMessage("BOSS DEFEATED!",2f,"#d4943d");
                SourceAudio2D.Play("boss");
                SpawnSourceParticle(position,Vector2.zero,1.2f,9f,"#ffb84d",1.2f);
            }
            if (enemy != null && enemy.IsElite) _runEliteKills++;
            SourceAudio2D.Play(enemy != null && enemy.IsBoss ? "explosion" : "kill");
            var go = new GameObject("Echo Shard");
            go.transform.position = position;
            // Source drawModernXp uses radius 6 for every orb, regardless of XP value.
            go.transform.localScale = Vector3.one * (2.2f * 6f * SpherePlacementRules.SourcePixelsToWorld);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSpriteFactory.ExperienceShard;
            sr.color = new Color(99f / 255f, 230f / 255f, 1f);
            sr.sortingOrder = 7;

            var outline = new GameObject("XP Shard Outline");
            outline.transform.SetParent(go.transform, false);
            var outlineRenderer = outline.AddComponent<SpriteRenderer>();
            outlineRenderer.sprite = RuntimeSpriteFactory.ExperienceShardOutline;
            outlineRenderer.color = new Color(230f / 255f, 253f / 255f, 1f, 0.82f);
            outlineRenderer.sortingOrder = 8;

            var highlight = new GameObject("XP Shard Highlight");
            highlight.transform.SetParent(go.transform, false);
            highlight.transform.localPosition = new Vector3(-0.0909f, 0.1364f, -0.01f);
            highlight.transform.localScale = Vector3.one * 0.12f;
            var highlightRenderer = highlight.AddComponent<SpriteRenderer>();
            highlightRenderer.sprite = RuntimeSpriteFactory.Disc;
            highlightRenderer.color = new Color(1f, 1f, 1f, 0.58f);
            highlightRenderer.sortingOrder = 9;

            // The source rounds enemy XP * combo multiplier once, at orb creation.
            var comboExperience = ComboRules.GetExperienceOrbValue(xpReward, _comboMultiplier);
            var orbVelocity = new Vector2(-40f + _rng.NextFloat() * 80f, 40f - _rng.NextFloat() * 80f);
            var orbRotation = _rng.NextFloat() * Mathf.PI * 2f;
            go.AddComponent<ExperienceOrbAgent>().Initialize(this, _player, comboExperience, orbVelocity, orbRotation);

            // Match the source death-drop order and keep its seeded RNG consumption.
            var healthChance = enemy != null && enemy.IsBoss ? 1f : enemy != null && enemy.IsElite ? 0.5f : 0.04f;
            if (_rng.NextFloat() < healthChance)
            {
                var health = new GameObject("Health Pack");
                health.transform.position = position;
                health.transform.localScale = Vector3.one * 0.22f;
                var healthRenderer = health.AddComponent<SpriteRenderer>();
                healthRenderer.sprite = RuntimeSpriteFactory.Star;
                healthRenderer.color = new Color(0.36f, 0.9f, 0.42f);
                healthRenderer.sortingOrder = 8;
                health.AddComponent<HealthPackAgent2D>().Initialize(this);
            }

            var runeChance = enemy != null && enemy.IsBoss ? 1f : enemy != null && enemy.IsElite ? 0.35f : 0f;
            if (_rng.NextFloat() < runeChance)
            {
                var runeTypes = RuneAgent2D.AllTypes;
                var runeType = runeTypes[Mathf.Min(runeTypes.Length - 1, Mathf.FloorToInt(_rng.NextFloat() * runeTypes.Length))];
                var rune = new GameObject("Tactical Rune · " + runeType);
                rune.transform.position = position;
                rune.transform.localScale = Vector3.one * 0.26f;
                var runeRenderer = rune.AddComponent<SpriteRenderer>();
                runeRenderer.sprite = RuntimeSpriteFactory.Diamond;
                runeRenderer.color = RuneAgent2D.GetRuneColor(runeType);
                runeRenderer.sortingOrder = 9;
                rune.AddComponent<RuneAgent2D>().Initialize(this, runeType, position, 22f);
            }

            var chestChance = enemy != null && enemy.IsBoss ? 1f : enemy != null && enemy.IsElite ? 0.2f : 0.02f;
            if (_rng.NextFloat() < chestChance)
            {
                var chest = new GameObject("Artifact Cache");
                chest.transform.position = position;
                chest.transform.localScale = Vector3.one * 0.28f;
                var chestRenderer = chest.AddComponent<SpriteRenderer>();
                chestRenderer.sprite = RuntimeSpriteFactory.Hexagon;
                chestRenderer.color = new Color(1f, 0.76f, 0.3f);
                chestRenderer.sortingOrder = 8;
                chest.AddComponent<ArtifactChestAgent2D>().Initialize(this);
            }
        }

        public void AddExperience(int amount)
        {
            if ((_paused && !_simulationFrame) || _gameOver) return;
            var multiplier = Mathf.Max(0.5f, PersistentUpgradeRules.GetPercentMultiplier(GetShopUpgradeLevel("xp")) *
                (1f + ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "xpGain")));
            _xp += Mathf.Max(0f, amount * multiplier);
            OpenLevelUpIfReady();
        }

        private void OpenLevelUpIfReady()
        {
            if (_gameOver || _xp < _xpRequired) return;
            while (_xp >= _xpRequired)
            {
                SourceAudio2D.Play("levelup");
                _xp -= _xpRequired;
                _level++;
                _activeAbilitySlots = _level >= 20 ? 3 : _level >= 12 ? 2 : _level >= 5 ? 1 : 0;
                // Source new-desing grants +6 Max HP and restores up to +6 HP on every player level.
                _playerCore?.IncreaseMaxHp(6f);
                _xpRequired = RunBalanceRules.GetXpToNextLevel(_level);
                BuildLevelUpChoices();
                _levelUp = true;
                _paused = true;
            }
        }

        public float ResolveProjectileDamage(float damage, float critChanceBonus = 0f)
        {
            return ResolveProjectileDamage(damage, critChanceBonus, out _);
        }

        public float ResolveProjectileDamage(float damage, float critChanceBonus, out bool wasCritical, EnemyAgent2D target = null)
        {
            var artifactCrit = ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "critChance");
            var abilityCrit = LevelUpAbilityRules.GetCritChanceBonus(GetAbilityLevel(RunUpgradeKind.Crit));
            var shopCrit = PersistentUpgradeRules.GetCritChanceBonus(GetShopUpgradeLevel("crit"));
            var hunterCrit = GetHunterCriticalChanceBonus(target);
            var critChance = Mathf.Min(CombatRules.CritHardCap, CombatRules.CritBase +
                Mathf.Max(0f, critChanceBonus + artifactCrit + abilityCrit + shopCrit + hunterCrit + GetArchitectFormationCritBonus()));
            var result = CombatRules.ResolveDamage(damage, critChance, _rng.NextFloat(), CombatRules.CritMultiplierBase);
            wasCritical = result.WasCritical;
            return result.Damage;
        }

        public void TryDamagePlayer(float damage)
        {
            if (_playerCore == null || !_playerCore.CanReceiveDamageNow) return;
            var dodgeBonus = LevelUpAbilityRules.GetDodgeChance(
                GetAbilityLevel(RunUpgradeKind.Dodge),
                ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "dodgeChance"));
            if (_rng.NextFloat() < dodgeBonus)
            {
                SpawnSourceParticle(PlayerPosition, new Vector2(0f, 60f), 0.5f, 3f, "#e8dcc0");

                return;
            }
            var artifactDamage = Mathf.Max(0f, damage) * GetArtifactOnlyDamageTakenMultiplier();
            if (_rng.NextFloat() < (_ownedArtifacts.Contains("mirror") ? 0.20f : 0f))
            {
                var reflected = FindNearestEnemy(PlayerPosition, 180f * SpherePlacementRules.SourcePixelsToWorld);
                if (reflected != null) reflected.ReceiveDamage(artifactDamage * 1.25f);
            }
            var received = artifactDamage * GetCharacterDamageTakenMultiplier() * LevelUpAbilityRules.GetDamageTakenMultiplier(GetAbilityLevel(RunUpgradeKind.Armor));
            if (GetAbilityBranch(RunUpgradeKind.Shield) == "shield_bastion" && _shieldTimer > 0f)
                received *= 0.70f;
            var shieldsBefore = _playerCore.ShieldCharges;
            if (_playerCore.TryTakeDamage(received))
            {
                if (_playerCore.IsDead) _deathTimer = PlayerLoopRules.DeathDuration;
                ResetCombo();
                _characterScreenShakeIntensity = Mathf.Min(0.4f, _characterScreenShakeIntensity + 0.2f);
                SourceAudio2D.Play("damage");
                if (_ownedArtifacts.Contains("stasis_core"))
                    for (var i = 0; i < _enemies.Count; i++)
                        if (_enemies[i] != null && _enemies[i].IsAlive) _enemies[i].ApplySlow(2f, 0.5f);
            }
            else if (_playerCore.ShieldCharges < shieldsBefore)
            {

                var shieldBranch = GetAbilityBranch(RunUpgradeKind.Shield);
                var shieldFinal = GetAbilityFinalArchetype(RunUpgradeKind.Shield);
                if (shieldFinal == "shield_resonant_guard") _playerCore.GrantShieldCharge(1);
                if (shieldBranch == "shield_reflector")
                {
                    var nearest = FindNearestEnemy(PlayerPosition, 220f * SpherePlacementRules.SourcePixelsToWorld);
                    if (nearest != null) nearest.ReceiveDamage(artifactDamage * 0.75f);
                }
                if (shieldBranch == "shield_bastion")
                {
                    var center = PlayerPosition;
                    foreach (var enemy in _enemies)
                    {
                        if (enemy == null || !enemy.IsAlive) continue;
                        var delta = (Vector2)enemy.transform.position - center;
                        if (delta.magnitude > 150f * SpherePlacementRules.SourcePixelsToWorld) continue;
                        var direction = delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector2.zero;
                        enemy.transform.position += (Vector3)(direction * (36f * SpherePlacementRules.SourcePixelsToWorld));
                    }
                }
                for (var i = 0; i < 12; i++) SpawnSourceParticle(PlayerPosition,
                    new Vector2((_rng.NextFloat() * 2f - 1f) * 150f, -(_rng.NextFloat() * 2f - 1f) * 150f), 0.4f, 3f, "#4a7a8a");
            }
        }

        public void TryDamagePlayerOverTime(float damage)
        {
            if (_playerCore != null && !_playerCore.IsDead &&
                _playerCore.TryTakeDotDamage(Mathf.Max(0f, damage) * GetArtifactDamageTakenMultiplier()))
            {
                if (_playerCore.IsDead) _deathTimer = PlayerLoopRules.DeathDuration;
                _characterScreenShakeIntensity = Mathf.Min(0.25f, _characterScreenShakeIntensity + 0.04f);
            }
        }

        private void ResetCombo()
        {
            _combo = 0;
            _comboTimer = 0f;
            _comboMultiplier = 1f;
        }

        public void HealCoreFromDamage(float damage)
        {
            var vampire = Mathf.Max(0f, ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "vampire") + GetAbilityLevel(RunUpgradeKind.Vampire) * 0.03f);
            if (_playerCore != null && !_playerCore.IsDead && vampire > 0f)
                _playerCore.Heal(Mathf.Max(0f, damage) * vampire);
        }

        private float GetArtifactMoveSpeedMultiplier()
        {
            var multiplier = Mathf.Max(0.5f, PersistentUpgradeRules.GetPercentMultiplier(GetShopUpgradeLevel("speed")) *
                (1f + ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "moveSpeed")));
            if (_swiftBootsTimer > 0f && _ownedArtifacts.Contains("swift_boots")) multiplier *= 1.10f;
            return multiplier;
        }

        public float GetArtifactSphereRadiusMultiplier(SphereId type)
        {
            var key = type switch
            {
                SphereId.Standard => "standardRadius",
                SphereId.Sniper => "sniperRadius",
                SphereId.Shotgun => "shotgunRadius",
                SphereId.Chain => "chainRadius",
                SphereId.Aura => "auraRadius",
                SphereId.Gravity => "gravityRadius",
                SphereId.Pulse => "pulseRadius",
                _ => null
            };
            var multiplier = 1f + ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "sphereRadius");
            if (!string.IsNullOrEmpty(key)) multiplier += ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, key);
            // Source artifactSystem.ts applies Formation Compass twice by design:
            // once through the global sphereRadius effect and once per Sphere here.
            if (_ownedArtifacts.Contains("formation_compass")) multiplier *= 1.08f;
            return Mathf.Clamp(multiplier, 0.5f, 3f);
        }

        private float GetArtifactDamageTakenMultiplier() =>
            GetCharacterDamageTakenMultiplier() *
            GetArtifactOnlyDamageTakenMultiplier() *
            LevelUpAbilityRules.GetDamageTakenMultiplier(GetAbilityLevel(RunUpgradeKind.Armor));

        private float GetArtifactOnlyDamageTakenMultiplier() =>
            Mathf.Max(0.45f, 1f - ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "damageTakenReduction")) *
            ArtifactSynergyRules.GetGlassCannonDamageTakenMultiplier(ArtifactSynergyRules.HasGlassCannonSynergy(_ownedArtifacts));

        private float GetArtifactSphereFamilyDamageMultiplier(SphereId type)
        {
            var key = type switch
            {
                SphereId.Standard => "standardDamage",
                SphereId.Sniper => "sniperDamage",
                SphereId.Shotgun => "shotgunDamage",
                SphereId.Chain => "chainDamage",
                SphereId.Aura => "auraDamage",
                SphereId.Orbital => "orbitalDamage",
                SphereId.Prism => "prismDamage",
                SphereId.Gravity => "gravityDamage",
                SphereId.Pulse => "pulseDamage",
                SphereId.Void => "voidDamage",
                _ => null
            };
            var multiplier = string.IsNullOrEmpty(key) ? 1f :
                Mathf.Max(0.25f, 1f + ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, key));
            var uniqueTypes = CountUniqueActiveSphereTypes();
            if (_ownedArtifacts.Contains("zero_sphere")) multiplier *= 1.20f;
            if (_ownedArtifacts.Contains("unified_mind")) multiplier *= 1f + GetStrongestSphereLevel() * 0.03f;
            if (_ownedArtifacts.Contains("lone_bastion") && uniqueTypes <= 1) multiplier *= 1.30f;
            if (_ownedArtifacts.Contains("fivefold_resonance") && uniqueTypes > 1) multiplier *= 1f + 0.04f * Mathf.Min(5, uniqueTypes);
            if (_ownedArtifacts.Contains("relay_matrix") && HasLevelSevenSphere()) multiplier *= 1.10f;
            if (_ownedArtifacts.Contains("network_relay") && uniqueTypes >= 2) multiplier *= 1.05f;
            if (_ownedArtifacts.Contains("network_coil") && uniqueTypes >= 2) multiplier *= 1.08f;
            if (_ownedArtifacts.Contains("geometry_die") && _networkState != null && _networkState.DominantFormation != null && _networkState.DominantFormation.Active) multiplier *= 1.08f;
            if (_ownedArtifacts.Contains("mirror_network") && uniqueTypes >= 2) multiplier *= 1.12f;
            if (_ownedArtifacts.Contains("singularity_engine") && uniqueTypes <= 2) multiplier *= 1.18f;
            if (_ownedArtifacts.Contains("triangle_circuit"))
            {
                var index = _spheres.FindIndex(x => x != null && x.Type == type);
                if (index >= 0 && GetFormationBonus(SphereNetworkFormation.Triangle, index) > 0f) multiplier *= 1.15f;
            }
            if (_ownedArtifacts.Contains("resonance_core") && SphereNearAnotherType(type)) multiplier *= 1.12f;
            return multiplier;
        }

        private bool SphereNearAnotherType(SphereId type)
        {
            SphereAttackAgent first = null;
            for (var i = 0; i < _spheres.Count; i++)
                if (_spheres[i] != null && _spheres[i].Type == type) { first = _spheres[i]; break; }
            if (first == null) return false;
            for (var i = 0; i < _spheres.Count; i++)
            {
                var other = _spheres[i];
                if (other == null || other == first) continue;
                if (Vector2.Distance(first.transform.position, other.transform.position) <= BossCombatRules.ToWorldUnits(190f)) return true;
            }
            return false;
        }

        private int CountUniqueActiveSphereTypes()
        {
            _artifactSphereTypeScratch.Clear();
            for (var i = 0; i < _spheres.Count; i++)
                if (_spheres[i] != null && _spheres[i].gameObject.activeInHierarchy) _artifactSphereTypeScratch.Add(_spheres[i].Type);
            return _artifactSphereTypeScratch.Count;
        }

        private int GetStrongestSphereLevel()
        {
            var strongest = 0;
            foreach (var level in _sphereLevels.Values) if (level > strongest) strongest = level;
            return strongest;
        }

        private bool HasLevelSevenSphere()
        {
            foreach (var level in _sphereLevels.Values) if (level >= 7) return true;
            return false;
        }

        public bool SetFormationFollowMode(bool active, bool skipRangeCheck = false)
        {
            if (active == _follow && _spheres.Count > 0) return true;
            if (active && !skipRangeCheck)
            {
                var nodes = new List<FormationNode>(_spheres.Count);
                foreach (var sphere in _spheres)
                {
                    if (sphere == null) continue;
                    var p = (Vector2)sphere.transform.position;
                    nodes.Add(new FormationNode(new Vec2(p.x, p.y), sphere.gameObject.activeInHierarchy && !sphere.NetworkDisabled));
                }
                var core = (Vector2)_player.position;
                if (!FormationFollowRules.CanActivate(new Vec2(core.x, core.y), nodes))
                {
                    ShowMessage("Move closer to the Sphere formation first.");
                    return false;
                }
            }
            _follow = active;
            var playerPosition = (Vector2)_player.position;
            foreach (var sphere in _spheres) if (sphere != null) sphere.SetFollowMode(active, playerPosition);
            _formationStrain = 0f;
            _lastDirection = Vec2.Zero;
            ShowMessage(active ? "Formation Follow engaged." : "Formation Follow disengaged.");
            return true;
        }

        private void BuildLevelUpChoices()
        {
            _levelUpChoices.Clear();
            var pool = new List<RunUpgradeChoice>();

            foreach (var definition in GameCatalog.Spheres)
                if (SphereProgressionRules.CanUpgrade(_sphereLevels, definition.Id))
                    pool.Add(SphereProgressionRules.CreateSphereChoice(_sphereLevels, definition.Id));

            // new-desing builds a passivePool but deliberately excludes it from
            // sourcePools. Routine Level-Up offers active abilities and Sphere
            // progression only, so do not expose passive stats or Max Spheres here.

            var activeAbilityCount = _activeAbilityHotkeys.Count;
            foreach (var kind in new[] { RunUpgradeKind.Teleport, RunUpgradeKind.TimeStop, RunUpgradeKind.Blast, RunUpgradeKind.Shield, RunUpgradeKind.FireTrail, RunUpgradeKind.Minion, RunUpgradeKind.Lightning, RunUpgradeKind.DarkRitual })
            {
                var current = GetAbilityLevel(kind);
                if (current > 0 || activeAbilityCount < _activeAbilitySlots)
                    if (LevelUpAbilityRules.CanUpgrade(kind, current))
                        pool.Add(LevelUpAbilityRules.CreateChoice(kind, current, IsRussianUi));
            }

            if (pool.Count == 0)
            {
                _levelUpLockChoiceKey = null;
                _oracleForecastKeys.Clear();
                _oracleSourceFlashText = string.Empty;
                _oracleSourceFlashTimer = 0f;
                return;
            }

            var lockedChoiceIndex = string.IsNullOrEmpty(_levelUpLockChoiceKey)
                ? -1
                : pool.FindIndex(choice => GetUpgradeChoiceKey(choice) == _levelUpLockChoiceKey);
            if (!string.IsNullOrEmpty(_levelUpLockChoiceKey) && lockedChoiceIndex < 0)
                _levelUpLockChoiceKey = null;

            var recent = new HashSet<string>(_recentUpgradeChoiceKeys);
            var cooled = pool.FindAll(choice => !recent.Contains(GetUpgradeChoiceKey(choice)));
            var candidates = cooled.Count >= 3 ? cooled : pool;
            var selectionCandidates = lockedChoiceIndex >= 0
                ? candidates.FindAll(choice => GetUpgradeChoiceKey(choice) != _levelUpLockChoiceKey)
                : candidates;
            if (lockedChoiceIndex >= 0) _levelUpChoices.Add(pool[lockedChoiceIndex]);

            var abilityScore = 0f;
            foreach (var pair in _abilityLevels) if (pair.Value > 0) abilityScore += 1f;

            var sphereScore = (float)_spheres.FindAll(sphere => sphere != null && sphere.gameObject.activeInHierarchy).Count;
            foreach (var pair in _sphereLevels) sphereScore += pair.Value * 0.15f;
            var minimumScore = Mathf.Min(abilityScore, sphereScore);
            var abilitySourceWeight = (1f + Mathf.Min(4, _levelUpAbilityPity) * 0.20f) *
                (abilityScore <= minimumScore + 0.001f ? 1.15f : 1f);
            var sphereSourceWeight = (1f + Mathf.Min(4, _levelUpSpherePity) * 0.20f) *
                (sphereScore <= minimumScore + 0.001f ? 1.15f : 1f);

            while (selectionCandidates.Count > 0 && _levelUpChoices.Count < 3)
            {
                var usedSphere = false;
                var usedAbility = false;
                foreach (var selected in _levelUpChoices)
                {
                    if (selected.Kind == RunUpgradeKind.Sphere) usedSphere = true;
                    else usedAbility = true;
                }

                var totalWeight = 0f;
                var weights = new float[selectionCandidates.Count];
                for (var i = 0; i < selectionCandidates.Count; i++)
                {
                    var choice = selectionCandidates[i];
                    var weight = GetLevelUpChoiceBaseWeight(choice, abilitySourceWeight, sphereSourceWeight);
                    if (_levelUpChoices.Count > 0)
                        weight *= choice.Kind == RunUpgradeKind.Sphere
                            ? (usedSphere ? 0.88f : 1.16f)
                            : (usedAbility ? 0.88f : 1.16f);
                    weights[i] = Mathf.Max(0.01f, weight);
                    totalWeight += weights[i];
                }

                var roll = _rng.NextFloat() * totalWeight;
                var chosenIndex = selectionCandidates.Count - 1;
                for (var i = 0; i < selectionCandidates.Count; i++)
                {
                    roll -= weights[i];
                    if (roll <= 0f) { chosenIndex = i; break; }
                }
                _levelUpChoices.Add(selectionCandidates[chosenIndex]);
                selectionCandidates.RemoveAt(chosenIndex);
            }

            RefreshOracleForecast(abilitySourceWeight, sphereSourceWeight);
        }

        private string GetUpgradeChoiceKey(RunUpgradeChoice choice) =>
            choice.Kind == RunUpgradeKind.Sphere
                ? "sphere:" + choice.Sphere + ":" + choice.CurrentLevel + ":" + choice.NextLevel
                : "ability:" + choice.Kind + ":" + choice.CurrentLevel + ":" + choice.NextLevel;

        private bool IsCharacterPreferredActiveAbility(RunUpgradeKind kind)
        {
            switch (_sphereCharacterId)
            {
                case SphereCharacterId.Spherist:
                    return kind == RunUpgradeKind.Blast || kind == RunUpgradeKind.Lightning || kind == RunUpgradeKind.Shield;
                case SphereCharacterId.Hunter:
                    return kind == RunUpgradeKind.Teleport || kind == RunUpgradeKind.Lightning || kind == RunUpgradeKind.Blast;
                case SphereCharacterId.Engineer:
                    return kind == RunUpgradeKind.Shield || kind == RunUpgradeKind.Minion || kind == RunUpgradeKind.Lightning;
                case SphereCharacterId.Berserker:
                    return kind == RunUpgradeKind.Shield || kind == RunUpgradeKind.DarkRitual || kind == RunUpgradeKind.FireTrail;
                case SphereCharacterId.Alchemist:
                    return kind == RunUpgradeKind.FireTrail || kind == RunUpgradeKind.Lightning || kind == RunUpgradeKind.TimeStop;
                case SphereCharacterId.Architect:
                    return kind == RunUpgradeKind.Teleport || kind == RunUpgradeKind.TimeStop || kind == RunUpgradeKind.Shield;
                case SphereCharacterId.Conductor:
                    return kind == RunUpgradeKind.Blast || kind == RunUpgradeKind.Lightning;
                case SphereCharacterId.Oracle:
                    return kind == RunUpgradeKind.Teleport || kind == RunUpgradeKind.TimeStop;
                case SphereCharacterId.Voidwalker:
                    return kind == RunUpgradeKind.Teleport || kind == RunUpgradeKind.DarkRitual || kind == RunUpgradeKind.Shield;
                case SphereCharacterId.Fractal:
                    return kind == RunUpgradeKind.Blast || kind == RunUpgradeKind.TimeStop || kind == RunUpgradeKind.Minion;
                default:
                    return false;
            }
        }

        private float GetLevelUpChoiceBaseWeight(
            RunUpgradeChoice choice, float abilitySourceWeight, float sphereSourceWeight)
        {
            float weight;
            if (choice.Kind == RunUpgradeKind.Sphere)
            {
                var level = SphereProgressionRules.GetLevel(_sphereLevels, choice.Sphere);
                var activeCopies = 0;
                for (var sphereIndex = 0; sphereIndex < _spheres.Count; sphereIndex++)
                    if (_spheres[sphereIndex] != null && _spheres[sphereIndex].gameObject.activeInHierarchy &&
                        _spheres[sphereIndex].Type == choice.Sphere) activeCopies++;

                weight = sphereSourceWeight * (1f + (7 - level) * 0.25f +
                    (activeCopies > 0 ? 1.5f : 0f) + GetSphereAffinityWeight(choice.Sphere));
            }
            else
            {
                // Source getAbilityUpgradeChoiceWeight adds +0.75 for preferred
                // abilities and +0.08 for every active Ability before weighting.
                var levelWeight = choice.CurrentLevel == 0 ? 1.35f : 1.15f;
                var characterAffinity = IsCharacterPreferredActiveAbility(choice.Kind) ? 0.75f : 0f;
                weight = abilitySourceWeight * (levelWeight + characterAffinity + 0.08f);
            }
            return Mathf.Max(0.01f, weight);
        }

        private void RefreshOracleForecast(float abilitySourceWeight, float sphereSourceWeight)
        {
            _oracleForecastKeys.Clear();
            _oracleSourceFlashText = string.Empty;
            _oracleSourceFlashTimer = 0f;
            if (_sphereCharacterId != SphereCharacterId.Oracle || _levelUpChoices.Count == 0) return;

            var selectedIndexes = new List<int>();
            var forecastCount = LevelUpChoiceRules.GetOracleForecastCount(_characterMasteryLevel);
            for (var rank = 0; rank < forecastCount && rank < _levelUpChoices.Count; rank++)
            {
                var bestIndex = -1;
                var bestWeight = float.NegativeInfinity;
                for (var i = 0; i < _levelUpChoices.Count; i++)
                {
                    if (selectedIndexes.Contains(i)) continue;
                    var weight = GetLevelUpChoiceBaseWeight(_levelUpChoices[i], abilitySourceWeight, sphereSourceWeight);
                    if (weight <= bestWeight) continue;
                    bestWeight = weight;
                    bestIndex = i;
                }
                if (bestIndex < 0) break;
                selectedIndexes.Add(bestIndex);
                _oracleForecastKeys.Add(GetUpgradeChoiceKey(_levelUpChoices[bestIndex]));
            }

            if (LevelUpChoiceRules.ShouldShowOracleSourceFlash(_characterMasteryLevel, _levelUpChoices.Count > 0))
            {
                _oracleSourceFlashText = "ORACLE: " +
                    (_levelUpChoices[0].Kind == RunUpgradeKind.Sphere ? "SPHERE" : "ABILITY");
                _oracleSourceFlashTimer = 0.65f;
            }
        }

        private bool LockLevelUpChoice(int choiceIndex)
        {
            if (!_levelUp || _choosingEvolution || choiceIndex < 0 || choiceIndex >= _levelUpChoices.Count)
                return false;
            if (!LevelUpChoiceRules.CanLockChoice(
                    _levelUpChoices.Count, _levelUpLocksRemaining, !string.IsNullOrEmpty(_levelUpLockChoiceKey)))
                return false;

            _levelUpLockChoiceKey = GetUpgradeChoiceKey(_levelUpChoices[choiceIndex]);
            _levelUpLocksRemaining--;
            _oracleSourceFlashTimer = 0f;
            ShowTimedCombatMessage("LOCK",.8f,"#d4943d");
            _messageTimer = 0.8f;
            return true;
        }

        private bool RerollLevelUpChoices()
        {
            if (!_levelUp || _choosingEvolution ||
                !LevelUpChoiceRules.CanRerollChoices(_levelUpChoices.Count, _levelUpRerollsRemaining))
                return false;

            var current = new List<RunUpgradeChoice>(_levelUpChoices);
            var currentKeys = new HashSet<string>();
            foreach (var choice in current) currentKeys.Add(GetUpgradeChoiceKey(choice));

            var lockedChoiceIndex = string.IsNullOrEmpty(_levelUpLockChoiceKey)
                ? -1
                : current.FindIndex(choice => GetUpgradeChoiceKey(choice) == _levelUpLockChoiceKey);
            var oracleForecastChoices = new List<RunUpgradeChoice>();
            if (_sphereCharacterId == SphereCharacterId.Oracle &&
                _characterMasteryLevel >= 6 && !_oracleForecastRerollUsed)
            {
                var forecastLimit = LevelUpChoiceRules.GetOracleRerollPreservationCount(_characterMasteryLevel);
                for (var i = 0; i < _oracleForecastKeys.Count && i < forecastLimit; i++)
                {
                    var index = current.FindIndex(choice => GetUpgradeChoiceKey(choice) == _oracleForecastKeys[i]);
                    if (index >= 0) oracleForecastChoices.Add(current[index]);
                }
            }

            var next = current;
            if (lockedChoiceIndex >= 0)
            {
                var replacements = new List<RunUpgradeChoice>(2);
                var replacementKeys = new HashSet<string>();
                for (var attempt = 0; attempt < 8 && replacements.Count < 2; attempt++)
                {
                    BuildLevelUpChoices();
                    foreach (var choice in _levelUpChoices)
                    {
                        var key = GetUpgradeChoiceKey(choice);
                        if (key == _levelUpLockChoiceKey || currentKeys.Contains(key) || !replacementKeys.Add(key))
                            continue;
                        replacements.Add(choice);
                        if (replacements.Count >= 2) break;
                    }
                }
                if (replacements.Count >= 2)
                    next = new List<RunUpgradeChoice> { current[lockedChoiceIndex], replacements[0], replacements[1] };
            }
            else
            {
                for (var attempt = 0; attempt < 4; attempt++)
                {
                    BuildLevelUpChoices();
                    if (_levelUpChoices.Count != 3) continue;
                    var candidate = new List<RunUpgradeChoice>(_levelUpChoices);
                    var hasNewChoice = false;
                    foreach (var choice in candidate)
                    {
                        if (!currentKeys.Contains(GetUpgradeChoiceKey(choice)))
                        {
                            hasNewChoice = true;
                            break;
                        }
                    }
                    next = candidate;
                    if (hasNewChoice) break;
                }
            }

            if (oracleForecastChoices.Count > 0 && lockedChoiceIndex < 0 &&
                LevelUpChoiceRules.ShouldPreserveOracleForecast(
                    _sphereCharacterId == SphereCharacterId.Oracle, _characterMasteryLevel,
                    _oracleForecastRerollUsed, false, true))
            {
                var preservedKeys = new HashSet<string>();
                var merged = new List<RunUpgradeChoice>(3);
                foreach (var choice in oracleForecastChoices)
                {
                    var key = GetUpgradeChoiceKey(choice);
                    if (merged.Count < 3 && preservedKeys.Add(key)) merged.Add(choice);
                }
                foreach (var choice in next)
                {
                    var key = GetUpgradeChoiceKey(choice);
                    if (merged.Count < 3 && preservedKeys.Add(key)) merged.Add(choice);
                }
                next = merged;
                _oracleForecastRerollUsed = true;
            }

            _levelUpChoices.Clear();
            _levelUpChoices.AddRange(next);
            _levelUpRerollsRemaining--;
            _oracleSourceFlashTimer = 0f;
            ShowTimedCombatMessage(lockedChoiceIndex >= 0 ? "LOCK + REROLL" : "REROLL",.8f);
            _messageTimer = 0.8f;
            return true;
        }

        private int GetActiveCharacterSphereCount()
        {
            var count = 0;
            for (var i = 0; i < _spheres.Count; i++)
                if (_spheres[i] != null && _spheres[i].gameObject.activeInHierarchy) count++;
            return count;
        }

        private List<SphereAttackAgent> GetLocalCharacterSpheres()
        {
            var result = new List<SphereAttackAgent>();
            var radius = 420f * SpherePlacementRules.SourcePixelsToWorld;
            var radiusSq = radius * radius;
            var playerPosition = PlayerPosition;
            for (var i = 0; i < _spheres.Count; i++)
            {
                var sphere = _spheres[i];
                if (sphere == null || !sphere.gameObject.activeInHierarchy) continue;
                if (((Vector2)sphere.transform.position - playerPosition).sqrMagnitude <= radiusSq) result.Add(sphere);
            }
            return result;
        }

        private int GetSphereNeighbourCount(SphereAttackAgent sphere, float rangePixels)
        {
            if (sphere == null) return 0;
            var range = rangePixels * SpherePlacementRules.SourcePixelsToWorld;
            var rangeSq = range * range;
            var center = (Vector2)sphere.transform.position;
            var neighbours = 0;
            for (var i = 0; i < _spheres.Count; i++)
            {
                var candidate = _spheres[i];
                if (candidate == null || candidate == sphere || !candidate.gameObject.activeInHierarchy || candidate.NetworkDisabled) continue;
                if (((Vector2)candidate.transform.position - center).sqrMagnitude <= rangeSq) neighbours++;
            }
            return neighbours;
        }

        private int GetConnectedSphereNetworkSize(SphereAttackAgent sphere, float rangePixels)
        {
            if (sphere == null || !sphere.gameObject.activeInHierarchy) return 0;
            var local = GetLocalCharacterSpheres();
            if (!local.Contains(sphere)) return 0;
            var visited = new HashSet<SphereAttackAgent> { sphere };
            var queue = new Queue<SphereAttackAgent>();
            queue.Enqueue(sphere);
            var range = rangePixels * SpherePlacementRules.SourcePixelsToWorld;
            var rangeSq = range * range;
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var center = (Vector2)current.transform.position;
                for (var i = 0; i < local.Count; i++)
                {
                    var candidate = local[i];
                    if (visited.Contains(candidate) || candidate.NetworkDisabled) continue;
                    if (((Vector2)candidate.transform.position - center).sqrMagnitude > rangeSq) continue;
                    visited.Add(candidate);
                    queue.Enqueue(candidate);
                }
            }
            return visited.Count;
        }

        private float GetCharacterGlobalSphereDamageMultiplier()
        {
            var definition = CharacterRules.Get(_sphereCharacterId);
            var multiplier = 1f + definition.SphereDamage;
            var sphereCount = GetActiveCharacterSphereCount();
            if (_sphereCharacterId == SphereCharacterId.Spherist)
                multiplier += CharacterRules.GetSpheristDamageBonus(_characterMasteryLevel, sphereCount);
            else if (_sphereCharacterId == SphereCharacterId.Berserker && _playerCore != null)
            {
                var hpRatio = _playerCore.CurrentHp / Mathf.Max(1f, _playerCore.MaxHp);
                multiplier += CharacterRules.GetBerserkerFuryDamageBonus(hpRatio);
                var closeRange = 110f * SpherePlacementRules.SourcePixelsToWorld;
                var closeEnemy = false;
                for (var i = 0; i < _enemies.Count; i++)
                {
                    var enemy = _enemies[i];
                    if (enemy != null && enemy.IsAlive && Vector2.Distance(enemy.transform.position, PlayerPosition) <= closeRange)
                    {
                        closeEnemy = true;
                        break;
                    }
                }
                if (closeEnemy) multiplier += CharacterRules.GetBerserkerCloseDamageBonus(_characterMasteryLevel);
            }
            return multiplier;
        }

        public void TriggerEngineerRelay(SphereAttackAgent source)
        {
            if (_sphereCharacterId != SphereCharacterId.Engineer || source == null ||
                !source.gameObject.activeInHierarchy) return;

            _engineerRelaySource = source;
            _engineerRelayTimer = EngineerRelayRules.GetWindowSeconds(_characterMasteryLevel);
            var rangeWorld = EngineerRelayRules.GetNetworkRangePixels(_characterMasteryLevel) *
                SpherePlacementRules.SourcePixelsToWorld;
            var rangeWorldSq = rangeWorld * rangeWorld;
            var sourcePosition = (Vector2)source.transform.position;
            for (var i = 0; i < _spheres.Count; i++)
            {
                var neighbour = _spheres[i];
                if (neighbour == null || neighbour == source || !neighbour.gameObject.activeInHierarchy) continue;
                var delta = (Vector2)neighbour.transform.position - sourcePosition;
                if (delta.sqrMagnitude > rangeWorldSq) continue;
                SpawnEngineerRelayLightning(sourcePosition, neighbour.transform.position);
            }
        }

        public float ConsumeEngineerRelayBonus(SphereAttackAgent consumer)
        {
            if (consumer == null || !EngineerRelayRules.ConsumesOnAttack(consumer.Type)) return 1f;
            var hasSource = _engineerRelaySource != null;
            var distancePixels = hasSource
                ? Vector2.Distance(_engineerRelaySource.transform.position, consumer.transform.position) /
                    SpherePlacementRules.SourcePixelsToWorld
                : float.PositiveInfinity;
            if (!EngineerRelayRules.CanConsumeRelay(
                    _sphereCharacterId == SphereCharacterId.Engineer, hasSource, _engineerRelayTimer,
                    consumer == _engineerRelaySource, distancePixels, _characterMasteryLevel)) return 1f;

            _engineerRelaySource = null;
            _engineerRelayTimer = 0f;
            return EngineerRelayRules.GetDamageMultiplier();
        }

        private void TickEngineerRelay(float dt)
        {
            if (_engineerRelaySource == null || !_engineerRelaySource.gameObject.activeInHierarchy)
            {
                _engineerRelaySource = null;
                _engineerRelayTimer = 0f;
                return;
            }

            _engineerRelayTimer = Mathf.Max(0f, _engineerRelayTimer - dt);
            if (_engineerRelayTimer <= 0f) _engineerRelaySource = null;
        }

        private float GetCharacterNetworkDamageMultiplier(SphereAttackAgent sphere)
        {
            if (_sphereCharacterId == SphereCharacterId.Engineer)
            {
                var range = _characterMasteryLevel >= 6 ? 260f : _characterMasteryLevel >= 2 ? 240f : 220f;
                var neighbours = GetSphereNeighbourCount(sphere, range);
                var multiplier = 1f + Mathf.Min(2, neighbours) * 0.06f;
                if (_characterMasteryLevel >= 5 && neighbours > 0)
                {
                    var connectedToMasterNode = false;
                    var rangeWorldSq = range * SpherePlacementRules.SourcePixelsToWorld;
                    rangeWorldSq *= rangeWorldSq;
                    for (var i = 0; i < _spheres.Count; i++)
                    {
                        var neighbour = _spheres[i];
                        if (neighbour == null || neighbour == sphere || !neighbour.gameObject.activeInHierarchy ||
                            Vector2.Distance(neighbour.transform.position, sphere.transform.position) > range * SpherePlacementRules.SourcePixelsToWorld) continue;
                        if (GetSphereNeighbourCount(neighbour, range) >= 2) { connectedToMasterNode = true; break; }
                    }
                    if (connectedToMasterNode) multiplier += _characterMasteryLevel >= 10 ? 0.04f : 0.02f;
                }
                var networkSize = GetConnectedSphereNetworkSize(sphere, range);
                if (networkSize >= 4) multiplier += 0.03f;
                if (_characterMasteryLevel >= 7 && networkSize >= 4) multiplier += 0.03f;
                return multiplier;
            }
            return 1f;
        }

        private float GetCharacterAttackSpeedMultiplier()
        {
            var definition = CharacterRules.Get(_sphereCharacterId);
            var multiplier = 1f + definition.SphereAttackSpeed;
            var sphereCount = GetActiveCharacterSphereCount();
            if (_sphereCharacterId == SphereCharacterId.Spherist)
                multiplier += CharacterRules.GetSpheristAttackSpeedBonus(_characterMasteryLevel, sphereCount);
            else if (_sphereCharacterId == SphereCharacterId.Conductor && _conductorOverdriveTimer > 0f)
                multiplier += ConductorOverdriveRules.GetAttackSpeedBonus(_characterMasteryLevel);
            else if (_sphereCharacterId == SphereCharacterId.Berserker && _playerCore != null)
            {
                var hpRatio = _playerCore.CurrentHp / Mathf.Max(1f, _playerCore.MaxHp);
                multiplier += CharacterRules.GetBerserkerFuryAttackSpeedBonus(hpRatio);
                if (_characterMasteryLevel >= 3) multiplier += 0.02f;
            }
            else if (_sphereCharacterId == SphereCharacterId.Architect && GetLocalCharacterSpheres().Count >= 5 &&
                _networkState != null && _networkState.DominantFormation != null && _networkState.DominantFormation.Active &&
                _networkState.DominantFormation.Type == SphereNetworkFormation.Cluster)
                multiplier += 0.15f * GetArchitectFormationBonusScale(SphereNetworkFormation.Cluster);
            return multiplier;
        }

        public float GetCharacterSphereRadiusMultiplier()
        {
            var definition = CharacterRules.Get(_sphereCharacterId);
            var multiplier = 1f + definition.SphereRadius;
            var sphereCount = GetActiveCharacterSphereCount();
            var localCount = GetLocalCharacterSpheres().Count;
            if (_sphereCharacterId == SphereCharacterId.Spherist && _characterMasteryLevel >= 5 && sphereCount >= 8)
                multiplier += 0.05f;
            if (_sphereCharacterId == SphereCharacterId.Architect)
            {
                if (localCount >= 4) multiplier += 0.05f;
                if (_characterMasteryLevel >= 3) multiplier += 0.02f;
            }
            if (_sphereCharacterId == SphereCharacterId.Engineer)
            {
                var connected = 0;
                var local = GetLocalCharacterSpheres();
                var range = _characterMasteryLevel >= 6 ? 260f : _characterMasteryLevel >= 2 ? 240f : 220f;
                for (var i = 0; i < local.Count; i++)
                    connected = Mathf.Max(connected, GetConnectedSphereNetworkSize(local[i], range));
                if (connected >= 3) multiplier += 0.08f;
            }
            return multiplier;
        }

        private float GetCharacterMoveSpeedMultiplier() =>
            1f + CharacterRules.Get(_sphereCharacterId).MoveSpeed +
            (_sphereCharacterId == SphereCharacterId.Hunter
                ? HunterCombatRules.GetTrophyMoveSpeedBonus(_characterMasteryLevel, _hunterTrophyTimer > 0f)
                : 0f);

        private void TickHunterCharacterState(float dt)
        {
            _hunterMarkTimer = Mathf.Max(0f, _hunterMarkTimer - dt);
            _hunterHuntTimer = Mathf.Max(0f, _hunterHuntTimer - dt);
            _hunterTrophyTimer = Mathf.Max(0f, _hunterTrophyTimer - dt);
            _hunterScreenShakeIntensity = Mathf.Max(0f, _hunterScreenShakeIntensity - dt);

            if (_hunterMarkTimer <= 0f)
            {
                _hunterMarkTarget = null;
                _hunterHitCount = 0;
            }
            if (_hunterHuntTimer <= 0f) _hunterHuntTarget = null;
            if (_sphereCharacterId != SphereCharacterId.Hunter)
            {
                _hunterHuntTarget = null;
                _hunterHuntTimer = 0f;
                _hunterTrophyTimer = 0f;
                _hunterHitCount = 0;
            }
        }

        private void SpawnHunterMarkBurst(EnemyAgent2D target)
        {
            if (target == null) return;
            var position = (Vector2)target.transform.position;
            var color = new Color32(184, 71, 90, 255);
            for (var i = 0; i < 10; i++)
            {
                var angle = _rng.NextFloat() * Mathf.PI * 2f;
                var speed = 50f + _rng.NextFloat() * 80f;
                var velocity = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
                SpawnHunterParticle(position, velocity, 0.35f, 2.5f, color);
            }
        }

        private void SpawnHunterHuntBurst(EnemyAgent2D target)
        {
            if (target == null) return;
            var position = (Vector2)target.transform.position;
            var color = new Color32(196, 69, 61, 255);
            for (var i = 0; i < 18; i++)
            {
                var angle = _rng.NextFloat() * Mathf.PI * 2f;
                var radius = 18f + _rng.NextFloat() * 24f;
                var particlePosition = position + new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius) *
                    SpherePlacementRules.SourcePixelsToWorld;
                SpawnHunterParticle(particlePosition, new Vector2(0f, -25f), 0.5f, 3f, color);
            }
            _hunterScreenShakeIntensity = Mathf.Min(0.16f, _hunterScreenShakeIntensity + 0.04f);
        }

        private void SpawnHunterParticle(Vector2 worldPosition, Vector2 sourceVelocityPixels, float lifetime, float sourceSizePixels, Color color) { SpawnSourceParticle(worldPosition,new Vector2(sourceVelocityPixels.x,-sourceVelocityPixels.y),lifetime,sourceSizePixels,"#"+ColorUtility.ToHtmlStringRGB(color)); }

        public void RegisterCharacterHitProcs(SphereAttackAgent owner, float actualDamage, bool allowSphereProc)
        {
            if (owner == null || !allowSphereProc || actualDamage <= 0f || _gameOver) return;

            if (_sphereCharacterId == SphereCharacterId.Spherist)
            {
                var sphereCount = 0;
                for (var i = 0; i < _spheres.Count; i++)
                    if (_spheres[i] != null && _spheres[i].gameObject.activeInHierarchy) sphereCount++;
                if (_characterMasteryLevel >= 9 && sphereCount >= 8)
                {
                    _spheristChorusHits++;
                    if (_spheristChorusHits >= CharacterRules.GetSpheristChorusThreshold(_characterMasteryLevel))
                    {
                        _spheristChorusHits = 0;
                        var companions = new List<SphereAttackAgent>();
                        for (var i = 0; i < _spheres.Count; i++)
                        {
                            var candidate = _spheres[i];
                            if (candidate == null || !candidate.gameObject.activeInHierarchy || candidate == owner) continue;
                            companions.Add(candidate);
                        }
                        companions.Sort((a, b) => b.AttackTimer.CompareTo(a.AttackTimer));
                        var count = Mathf.Min(_characterMasteryLevel >= 10 ? 2 : 1, companions.Count);
                        for (var i = 0; i < count; i++)
                        {
                            companions[i].ResetAttackTimer();
                            companions[i].TriggerResonancePulse(0.45f);
                        }
                        _characterScreenShakeIntensity = Mathf.Min(0.16f, _characterScreenShakeIntensity + 0.025f);
                        ShowTimedCombatMessage(_characterMasteryLevel >= 10 ? "CHORUS SYNC" : "CHORUS PULSE", 0.7f);
                    }
                }
                else _spheristChorusHits = 0;
            }

            if (_sphereCharacterId == SphereCharacterId.Berserker && _characterMasteryLevel >= 7)
            {
                var hpRatio = _playerCore == null ? 1f : _playerCore.CurrentHp / Mathf.Max(1f, _playerCore.MaxHp);
                if (CharacterRules.ShouldAccumulateBerserkerRedline(_characterMasteryLevel, hpRatio))
                {
                    _berserkerRedlineHits++;
                    if (_berserkerRedlineHits >= CharacterRules.GetBerserkerRedlineThreshold(_characterMasteryLevel))
                    {
                        _berserkerRedlineHits = 0;
                        var pulseDamage = actualDamage * CharacterRules.GetBerserkerRedlineDamageMultiplier(_characterMasteryLevel);
                        var radius = 105f * SpherePlacementRules.SourcePixelsToWorld;
                        for (var i = 0; i < _enemies.Count; i++)
                        {
                            var target = _enemies[i];
                            if (target == null || !target.IsAlive ||
                                Vector2.Distance(target.transform.position, PlayerPosition) > radius) continue;
                            target.ReceiveDamage(pulseDamage);
                        }
                        for (var i = 0; i < 16; i++)
                        {
                            var angle = _rng.NextFloat() * Mathf.PI * 2f;
                            var velocity = new Vector2(Mathf.Cos(angle) * 120f, Mathf.Sin(angle) * 120f);
                            SpawnHunterParticle(PlayerPosition, velocity, 0.35f, 3f, new Color32(255, 98, 93, 255));
                        }
                        _characterScreenShakeIntensity = Mathf.Min(0.20f, _characterScreenShakeIntensity + 0.035f);
                        ShowTimedCombatMessage("REDLINE", 0.65f);
                    }
                }
                else _berserkerRedlineHits = 0;
            }
        }

        public void RegisterCharacterSphereHit(SphereId source, EnemyAgent2D target)
        {
            if (_sphereCharacterId != SphereCharacterId.Hunter || target == null || !target.IsAlive) return;
            if (!HunterCombatRules.CanMarkTarget(source, target.IsBoss, target.IsElite, target.Archetype)) return;

            var duration = HunterCombatRules.GetMarkDuration(_characterMasteryLevel);
            var activeSameTarget = _hunterMarkTarget == target && _hunterMarkTimer > 0f;
            _hunterMarkTarget = target;
            _hunterMarkTimer = duration;
            target.ApplyMark(duration);
            if (!activeSameTarget)
            {
                _hunterHitCount = 1;
                SpawnHunterMarkBurst(target);
                return;
            }

            _hunterHitCount++;
            var threshold = HunterCombatRules.GetHuntHitThreshold(_characterMasteryLevel);
            if (_hunterHitCount < threshold) return;
            _hunterHuntTarget = target;
            _hunterHuntTimer = 3f;
            _hunterHitCount = 0;
            SpawnHunterHuntBurst(target);
        }

        public float GetHunterMarkedTargetDamageMultiplier(SphereId source, EnemyAgent2D target)
        {
            if (_sphereCharacterId != SphereCharacterId.Hunter || target == null ||
                _hunterMarkTarget != target || _hunterMarkTimer <= 0f) return 1f;

            return HunterCombatRules.GetMarkedDamageMultiplier(
                _characterMasteryLevel, true, _hunterHuntTimer > 0f && _hunterHuntTarget == target);
        }

        private float GetHunterCriticalChanceBonus(EnemyAgent2D target)
        {
            return _sphereCharacterId == SphereCharacterId.Hunter
                ? HunterCombatRules.GetMarkedTargetCritBonus(_characterMasteryLevel,
                    target != null && _hunterMarkTarget == target && _hunterMarkTimer > 0f)
                : 0f;
        }

        private float GetCharacterDamageTakenMultiplier()
        {
            var multiplier = 1f + CharacterRules.Get(_sphereCharacterId).DamageTaken;
            if (_sphereCharacterId == SphereCharacterId.Architect && _networkState != null &&
                _networkState.DominantFormation != null && _networkState.DominantFormation.Type == SphereNetworkFormation.Square)
                multiplier *= Mathf.Max(0.70f, 1f - 0.12f * GetArchitectFormationBonusScale(SphereNetworkFormation.Square));
            return multiplier;
        }

        private void TrackArchitectFormationChange()
        {
            if (_sphereCharacterId != SphereCharacterId.Architect)
            {
                _architectFormationTracked = false;
                return;
            }

            var formation = _networkState == null ? null : _networkState.DominantFormation;
            var nextType = formation != null && formation.Active &&
                (formation.Type == SphereNetworkFormation.Line || formation.Type == SphereNetworkFormation.Triangle ||
                 formation.Type == SphereNetworkFormation.Square || formation.Type == SphereNetworkFormation.Cluster)
                    ? formation.Type : SphereNetworkFormation.None;
            if (!_architectFormationTracked)
            {
                _architectFormationTracked = true;
                _architectFormationType = nextType;
                _architectFormationChangedAt = float.NegativeInfinity;
                return;
            }
            if (_architectFormationType == nextType) return;
            _architectFormationType = nextType;
            _architectFormationChangedAt = _runTime;
        }

        private float GetArchitectFormationCritBonus()
        {
            if (_sphereCharacterId != SphereCharacterId.Architect || _networkState == null ||
                _networkState.DominantFormation == null || !_networkState.DominantFormation.Active ||
                _networkState.DominantFormation.Type != SphereNetworkFormation.Triangle)
                return 0f;

            // Source engineCombat.ts grants Architect +10 percentage points of crit chance in Triangle formation.
            return 0.10f;
        }

        private float GetArchitectFormationBonusScale(SphereNetworkFormation type)
        {
            var scale = 1f;
            if (_characterMasteryLevel >= 4 && _architectFormationType == type &&
                _runTime - _architectFormationChangedAt <= 2f) scale *= 1.25f;
            if (_characterMasteryLevel >= 5 && GetLocalCharacterSpheres().Count >= 5) scale *= 1.05f;
            if (_characterMasteryLevel >= 7) scale *= 1.05f;
            if (_characterMasteryLevel >= 10 && type != SphereNetworkFormation.None) scale *= 1.10f;
            return scale;
        }

        private void RecordFractalFormation(SphereNetworkShape formation)
        {
            if (_sphereCharacterId != SphereCharacterId.Fractal || formation == null ||
                !formation.Active || formation.Type == SphereNetworkFormation.None) return;
            if (_fractalFormationHistory.Count > 0 &&
                _fractalFormationHistory[_fractalFormationHistory.Count - 1].Type == formation.Type) return;

            var snapshot = new FractalFormationSnapshot { Type = formation.Type };
            for (var i = 0; i < formation.NodeIndexes.Count; i++)
            {
                var index = formation.NodeIndexes[i];
                if (index >= 0 && index < _spheres.Count && _spheres[index] != null)
                    snapshot.Nodes.Add(_spheres[index].transform.position);
            }
            _fractalFormationHistory.Add(snapshot);
            var historyLimit = _characterMasteryLevel >= 7 ? 4 : 3;
            while (_fractalFormationHistory.Count > historyLimit) _fractalFormationHistory.RemoveAt(0);

            if (_characterMasteryLevel < 3 || _fractalFormationHistory.Count < 3 || _fractalEchoCooldown > 0f) return;
            var recentStart = _fractalFormationHistory.Count - 3;
            if (_fractalFormationHistory[recentStart].Type == _fractalFormationHistory[recentStart + 1].Type ||
                _fractalFormationHistory[recentStart].Type == _fractalFormationHistory[recentStart + 2].Type ||
                _fractalFormationHistory[recentStart + 1].Type == _fractalFormationHistory[recentStart + 2].Type) return;

            _fractalEchoNodes.Clear();
            _fractalEchoNodes.AddRange(_fractalFormationHistory[recentStart].Nodes);
            _fractalEchoStrength = CharacterRules.GetFractalEchoStrength(_characterMasteryLevel);
            _fractalEchoTimer = CharacterRules.GetFractalEchoDuration(_characterMasteryLevel);
            _fractalEchoPulseTimer = 0f;
            _fractalEchoCooldown = 6f;
            ShowTimedCombatMessage("RECURSIVE ECHO", 0.9f);
        }

        private void TickCharacterSpecialRuntime(float dt)
        {
            _fractalEchoCooldown = Mathf.Max(0f, _fractalEchoCooldown - dt);
            if (_sphereCharacterId == SphereCharacterId.Fractal && _fractalEchoTimer > 0f)
            {
                _fractalEchoTimer = Mathf.Max(0f, _fractalEchoTimer - dt);
                _fractalEchoPulseTimer -= dt;
                if (_fractalEchoPulseTimer <= 0f)
                {
                    _fractalEchoPulseTimer = CharacterRules.GetFractalEchoPulseInterval(_characterMasteryLevel);
                    var damage = (16f + _level * 2f) * _fractalEchoStrength;
                    var radius = 95f * SpherePlacementRules.SourcePixelsToWorld;
                    for (var n = 0; n < _fractalEchoNodes.Count; n++)
                    {
                        var center = _fractalEchoNodes[n];
                        for (var i = 0; i < _enemies.Count; i++)
                        {
                            var enemy = _enemies[i];
                            if (enemy == null || !enemy.IsAlive ||
                                Vector2.Distance(enemy.transform.position, center) > radius) continue;
                            enemy.ReceiveDamage(damage);
                        }
                    }
                }
            }

            if (_sphereCharacterId != SphereCharacterId.Voidwalker) return;
            SphereAttackAgent disabled = null;
            for (var i = 0; i < _spheres.Count; i++)
            {
                var sphere = _spheres[i];
                if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.NetworkDisabled)
                {
                    disabled = sphere;
                    break;
                }
            }

            if (_voidPhantomTimer <= 0f && disabled != null)
            {
                _voidPhantomTimer = CharacterRules.GetVoidPhantomDuration(_characterMasteryLevel);
                _voidPhantomPulseTimer = 0f;
                _voidPhantomPulseCount = 0;
                _voidPhantomSource = disabled;
                _voidPhantomPosition = disabled.transform.position;
                ShowTimedCombatMessage("PHANTOM NODE", 0.8f);
            }
            else if (_voidPhantomTimer > 0f && _characterMasteryLevel >= 6 &&
                disabled != null && _voidPhantomSource != disabled)
            {
                _voidPhantomTimer = CharacterRules.GetVoidPhantomDuration(_characterMasteryLevel);
                _voidPhantomPulseTimer = 0f;
                _voidPhantomPulseCount = 0;
                _voidPhantomSource = disabled;
                _voidPhantomPosition = disabled.transform.position;
                ShowTimedCombatMessage("BREACH REFRESH", 0.7f);
            }

            if (_voidPhantomTimer > 0f)
            {
                _voidPhantomTimer = Mathf.Max(0f, _voidPhantomTimer - dt);
                _voidPhantomPulseTimer -= dt;
                if (_voidPhantomPulseTimer <= 0f)
                {
                    _voidPhantomPulseTimer = 0.6f;
                    _voidPhantomPulseCount++;
                    var hasVoid = false;
                    var hasGravity = false;
                    for (var i = 0; i < _spheres.Count; i++)
                    {
                        var sphere = _spheres[i];
                        if (sphere == null || !sphere.gameObject.activeInHierarchy) continue;
                        if (sphere.Type == SphereId.Void) hasVoid = true;
                        if (sphere.Type == SphereId.Gravity) hasGravity = true;
                    }
                    var activeVoidGravity = hasVoid && hasGravity;
                    var radius = 105f * SpherePlacementRules.SourcePixelsToWorld;
                    for (var i = 0; i < _enemies.Count; i++)
                    {
                        var enemy = _enemies[i];
                        if (enemy == null || !enemy.IsAlive) continue;
                        if (Vector2.Distance(enemy.transform.position, _voidPhantomPosition) > radius) continue;
                        var damage = CharacterRules.GetVoidPhantomDamage(
                            _level, _characterMasteryLevel, enemy.HpFraction, activeVoidGravity, _voidPhantomPulseCount);
                        enemy.ReceiveDamage(damage);
                        if (_characterMasteryLevel >= 3)
                            enemy.ApplySourceSlow(CharacterRules.VoidPhantomSlowDuration, CharacterRules.VoidPhantomSlowMultiplier);
                    }
                }
            }
            else
            {
                _voidPhantomPosition = Vector2.zero;
                _voidPhantomSource = null;
                _voidPhantomPulseCount = 0;
            }
        }

        private void TickCharacterMastery()
        {
            if (_gameOver || _paused || _levelUp || _choosingEvolution || _stellaRewardPending)
            {
                _characterMasteryLastTime = _runTime;
                return;
            }

            if (_characterMasteryLastTime < 0f)
            {
                _characterMasteryLastTime = _runTime;
                _characterMasteryLastEliteKills = _runEliteKills;
                _characterMasteryLastBosses = _bossesDefeated;
                _characterMasteryLastResonanceEvents = _resonanceEventsTriggered;
                return;
            }

            // Mirrors src/characterMastery.ts: use simulation time, cap a frame's credited
            // interval at 0.1 seconds, and pause mastery progress during source modal states.
            var dt = Mathf.Clamp(_runTime - _characterMasteryLastTime, 0f, 0.1f);
            _characterMasteryLastTime = _runTime;
            if (dt <= 0f) return;

            var eliteDelta = Mathf.Max(0, _runEliteKills - _characterMasteryLastEliteKills);
            var bossDelta = Mathf.Max(0, _bossesDefeated - _characterMasteryLastBosses);
            var resonanceDelta = Mathf.Max(0, _resonanceEventsTriggered - _characterMasteryLastResonanceEvents);
            _characterMasteryLastEliteKills = _runEliteKills;
            _characterMasteryLastBosses = _bossesDefeated;
            _characterMasteryLastResonanceEvents = _resonanceEventsTriggered;

            var activeSphereCount = GetActiveCharacterSphereCount();
            switch (_sphereCharacterId)
            {
                case SphereCharacterId.Spherist:
                    if (activeSphereCount >= 4) _characterMasteryRunXp += dt * 0.85f;
                    if (activeSphereCount >= 6) _characterMasteryRunXp += dt * 0.65f;
                    if (activeSphereCount >= 8) _characterMasteryRunXp += dt * 0.5f;
                    break;

                case SphereCharacterId.Hunter:
                    _characterMasteryRunXp += eliteDelta * 20f + bossDelta * 45f;
                    if (_hunterMarkTimer > 0f) _characterMasteryRunXp += dt * 1.5f;
                    if (_hunterHuntTimer > 0f) _characterMasteryRunXp += dt * 2.25f;
                    break;

                case SphereCharacterId.Engineer:
                {
                    var local = GetLocalCharacterSpheres();
                    var largestNetwork = 0;
                    var range = _characterMasteryLevel >= 6 ? 260f : _characterMasteryLevel >= 2 ? 240f : 220f;
                    for (var i = 0; i < local.Count; i++)
                        largestNetwork = Mathf.Max(largestNetwork, GetConnectedSphereNetworkSize(local[i], range));
                    if (largestNetwork >= 3) _characterMasteryRunXp += dt;
                    if (largestNetwork >= 5) _characterMasteryRunXp += dt * 0.65f;
                    if (_engineerRelayTimer > 0f) _characterMasteryRunXp += dt * 0.75f;
                    break;
                }

                case SphereCharacterId.Berserker:
                {
                    var hpRatio = _playerCore == null ? 1f : _playerCore.CurrentHp / Mathf.Max(1f, _playerCore.MaxHp);
                    if (hpRatio <= 0.6f) _characterMasteryRunXp += dt * 0.85f;
                    if (hpRatio <= 0.3f) _characterMasteryRunXp += dt * 0.8f;
                    var closeRange = 110f * SpherePlacementRules.SourcePixelsToWorld;
                    for (var i = 0; i < _enemies.Count; i++)
                    {
                        var enemy = _enemies[i];
                        if (enemy == null || !enemy.IsAlive || Vector2.Distance(enemy.transform.position, PlayerPosition) > closeRange) continue;
                        _characterMasteryRunXp += dt * 0.55f;
                        break;
                    }
                    break;
                }

                case SphereCharacterId.Alchemist:
                {
                    var statusTargets = 0;
                    var multiStatusTargets = 0;
                    for (var i = 0; i < _enemies.Count; i++)
                    {
                        var enemy = _enemies[i];
                        if (enemy == null || !enemy.IsAlive) continue;
                        var statusCount = (enemy.HasFireStatus ? 1 : 0) + (enemy.HasFreezeStatus ? 1 : 0) + (enemy.HasPoisonStatus ? 1 : 0);
                        if (statusCount > 0) statusTargets++;
                        if (statusCount >= 2) multiStatusTargets++;
                    }
                    _characterMasteryRunXp += statusTargets * dt * 0.07f;
                    _characterMasteryRunXp += multiStatusTargets * dt * 0.12f;
                    if (_alchemistCatalystTimer > 0f) _characterMasteryRunXp += dt * 2f;
                    break;
                }

                case SphereCharacterId.Architect:
                {
                    var formation = _networkState == null ? null : _networkState.DominantFormation;
                    if (formation != null && formation.Active) _characterMasteryRunXp += dt;
                    if (formation != null && formation.Active && formation.Strength >= 0.9f) _characterMasteryRunXp += dt * 0.75f;
                    break;
                }

                case SphereCharacterId.Conductor:
                    _characterMasteryRunXp += resonanceDelta * 28f;
                    if (_conductorOverdriveTimer > 0f) _characterMasteryRunXp += dt * 1.1f;
                    break;

                case SphereCharacterId.Oracle:
                    if (_oracleForecastKeys.Count > 0) _characterMasteryRunXp += dt * 0.9f;
                    break;

                case SphereCharacterId.Voidwalker:
                    for (var i = 0; i < _spheres.Count; i++)
                    {
                        var sphere = _spheres[i];
                        if (sphere == null || !sphere.gameObject.activeInHierarchy || !sphere.NetworkDisabled) continue;
                        _characterMasteryRunXp += dt * 1.1f;
                        break;
                    }
                    if (_voidPhantomTimer > 0f) _characterMasteryRunXp += dt * 1.5f;
                    break;

                case SphereCharacterId.Fractal:
                    if (_fractalFormationHistory.Count >= 2) _characterMasteryRunXp += dt;
                    if (_fractalEchoTimer > 0f) _characterMasteryRunXp += dt * 2.2f;
                    break;
            }
            _characterMasteryRunXp = Mathf.Max(0f, _characterMasteryRunXp);
        }

        private float GetSphereAffinityWeight(SphereId type)
        {
            return CharacterRules.GetSphereAffinityWeight(_sphereCharacterId, type);
        }

        private int GetAbilityLevel(RunUpgradeKind kind)
        {
            if (kind == RunUpgradeKind.MaxSpheres) return _maxSphereAbilityLevel;
            return _abilityLevels.TryGetValue(kind, out var level) ? level : 0;
        }

        private string GetAbilityBranch(RunUpgradeKind kind)
        {
            return _abilityBranches.TryGetValue(kind, out var branch) ? branch : string.Empty;
        }

        private string GetAbilityFinal(RunUpgradeKind kind)
        {
            return _abilityFinals.TryGetValue(kind, out var final) ? final : string.Empty;
        }

        private int GetAbilityFinalVariant(RunUpgradeKind kind)
        {
            var final = GetAbilityFinal(kind);
            if (string.IsNullOrEmpty(final) || final.Length < 3) return 0;
            var suffix = final.Substring(final.Length - 3);
            return suffix == "_f1" ? 1 : suffix == "_f2" ? 2 : suffix == "_f3" ? 3 : 0;
        }

        // Source getAbilityFinalArchetype maps the chosen branch-specific _f1/_f2/_f3
        // card to the ability's canonical evolution7 archetype by option index.
        private string GetAbilityFinalArchetype(RunUpgradeKind kind)
        {
            switch (kind)
            {
                case RunUpgradeKind.Blast:
                    return GetAbilityFinalVariant(kind) == 1 ? "blast_echo_network" :
                        GetAbilityFinalVariant(kind) == 2 ? "blast_resonant_core" :
                        GetAbilityFinalVariant(kind) == 3 ? "blast_infinite_pulse" : string.Empty;
                case RunUpgradeKind.Shield:
                    return GetAbilityFinalVariant(kind) == 1 ? "shield_network_guard" :
                        GetAbilityFinalVariant(kind) == 2 ? "shield_iron_dome" :
                        GetAbilityFinalVariant(kind) == 3 ? "shield_resonant_guard" : string.Empty;
                case RunUpgradeKind.Teleport:
                    return GetAbilityFinalVariant(kind) == 1 ? "teleport_spatial_network" :
                        GetAbilityFinalVariant(kind) == 2 ? "teleport_hunter_beacon" :
                        GetAbilityFinalVariant(kind) == 3 ? "teleport_phase_break" : string.Empty;
                case RunUpgradeKind.FireTrail:
                    return GetAbilityFinalVariant(kind) == 1 ? "firetrail_network" :
                        GetAbilityFinalVariant(kind) == 2 ? "firetrail_catalyst" :
                        GetAbilityFinalVariant(kind) == 3 ? "firetrail_inferno" : string.Empty;
                case RunUpgradeKind.Minion:
                    return GetAbilityFinalVariant(kind) == 1 ? "minion_echo_swarm" :
                        GetAbilityFinalVariant(kind) == 2 ? "minion_network_nodes" :
                        GetAbilityFinalVariant(kind) == 3 ? "minion_sphere_guard" : string.Empty;
                case RunUpgradeKind.Lightning:
                    return GetAbilityFinalVariant(kind) == 1 ? "lightning_storm_network" :
                        GetAbilityFinalVariant(kind) == 2 ? "lightning_thunder_chain" :
                        GetAbilityFinalVariant(kind) == 3 ? "lightning_overload_core" : string.Empty;
                case RunUpgradeKind.TimeStop:
                    return GetAbilityFinalVariant(kind) == 1 ? "timestop_outside_time" :
                        GetAbilityFinalVariant(kind) == 2 ? "timestop_closed_network" :
                        GetAbilityFinalVariant(kind) == 3 ? "timestop_temporal_core" : string.Empty;
                case RunUpgradeKind.DarkRitual:
                    return GetAbilityFinalVariant(kind) == 1 ? "darkritual_blood_network" :
                        GetAbilityFinalVariant(kind) == 2 ? "darkritual_sacrifice_core" :
                        GetAbilityFinalVariant(kind) == 3 ? "darkritual_void_engine" : string.Empty;
                default:
                    return string.Empty;
            }
        }

        private void AssignActiveAbilityHotkey(RunUpgradeKind kind)
        {
            if (_activeAbilityHotkeys.ContainsKey(kind)) return;
            foreach (var key in SourceActiveAbilityKeys)
            {
                var occupied = false;
                foreach (var binding in _activeAbilityHotkeys)
                    if (binding.Value == key) { occupied = true; break; }
                if (occupied) continue;
                _activeAbilityHotkeys[kind] = key;
                return;
            }
        }

        private float GetActiveAbilityCooldown(RunUpgradeKind kind)
        {
            switch (kind)
            {
                case RunUpgradeKind.Blast: return _blastCooldown;
                case RunUpgradeKind.Shield: return _shieldCooldown;
                case RunUpgradeKind.Teleport: return _teleportCooldown;
                case RunUpgradeKind.FireTrail: return _fireTrailCooldown;
                case RunUpgradeKind.Minion: return _minionCooldown;
                case RunUpgradeKind.Lightning: return _lightningCooldown;
                case RunUpgradeKind.TimeStop: return _timeStopCooldown;
                case RunUpgradeKind.DarkRitual: return _darkRitualCooldown;
                default: return 0f;
            }
        }

        private string GetActiveAbilityShortLabel(RunUpgradeKind kind)
        {
            switch (kind)
            {
                case RunUpgradeKind.Blast: return "ECHO PULSE";
                case RunUpgradeKind.Shield: return "SHIELD";
                case RunUpgradeKind.Teleport: return "ECHO JUMP";
                case RunUpgradeKind.FireTrail: return "OVERHEAT";
                case RunUpgradeKind.Minion: return "ECHO DRONE";
                case RunUpgradeKind.Lightning: return "LIGHTNING";
                case RunUpgradeKind.TimeStop: return "ECHO FREEZE";
                case RunUpgradeKind.DarkRitual: return "DARK RITUAL";
                default: return kind.ToString().ToUpperInvariant();
            }
        }

        private float GetActiveAbilityCooldownMultiplier() =>
            Mathf.Max(0.55f, (1f - GetAbilityLevel(RunUpgradeKind.Cooldown) * 0.04f) * (1f + ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "cooldown")));

        private void ActivateActiveAbility(RunUpgradeKind kind)
        {
            if (_mainMenu || _paused || _levelUp || _choosingEvolution || _gameOver || _stellaRewardPending)
                return;
            switch (kind)
            {
                case RunUpgradeKind.TimeStop: ActivateTimeStop(); break;
                case RunUpgradeKind.Teleport: ActivateTeleport(); break;
                case RunUpgradeKind.Blast: ActivateBlast(); break;
                case RunUpgradeKind.Shield: ActivateShield(); break;
                case RunUpgradeKind.FireTrail: ActivateFireTrail(); break;
                case RunUpgradeKind.Minion: ActivateMinion(); break;
                case RunUpgradeKind.Lightning: ActivateLightning(); break;
                case RunUpgradeKind.DarkRitual: ActivateDarkRitual(); break;
            }
        }

        private bool HasActiveAbilitySynergy(SphereId sphere, string sphereBranch, RunUpgradeKind ability, string abilityFinal)
        {
            if (!IsActiveAbilitySynergyRaw(sphere, sphereBranch, ability, abilityFinal)) return false;
            _activeAbilitySynergyPower = GetActiveAbilitySynergyPower(sphere, sphereBranch, ability, abilityFinal);
            return true;
        }

        private bool IsActiveAbilitySynergyRaw(SphereId sphere, string sphereBranch, RunUpgradeKind ability, string abilityFinal)
        {
            if (GetAbilityLevel(ability) < 7 ||
                !_sphereLevels.TryGetValue(sphere, out var sphereLevel) || sphereLevel < 7 ||
                !_sphereBranches.TryGetValue(sphere, out var selectedSphereBranch) || selectedSphereBranch != sphereBranch ||
                string.IsNullOrEmpty(abilityFinal) || abilityFinal.Length < 3)
                return false;

            var suffix = abilityFinal.Substring(abilityFinal.Length - 3);
            if (suffix != "_f1" && suffix != "_f2" && suffix != "_f3") return false;
            var expectedBranch = abilityFinal.Substring(0, abilityFinal.Length - 3);
            if (!_abilityBranches.TryGetValue(ability, out var selectedAbilityBranch) ||
                selectedAbilityBranch != expectedBranch ||
                !_abilityFinals.TryGetValue(ability, out var selectedFinalId))
                return false;

            var expectedVariant = suffix[2] - '1';
            var choices = LevelUpAbilityRules.GetActiveAbilityEvolutionChoices(ability, true, expectedBranch);
            for (var i = 0; i < choices.Count; i++)
                if (choices[i].Id == selectedFinalId) return i == expectedVariant;
            return false;
        }

        private float GetActiveAbilitySynergyPower(SphereId sphere, string sphereBranch, RunUpgradeKind ability, string abilityFinal)
        {
            var active = new List<ActiveAbilitySynergyLink>();
            foreach (var link in ActiveAbilitySynergyLinks)
                if (link.Ability == ability && IsActiveAbilitySynergyRaw(link.Sphere, link.SphereBranch, link.Ability, link.AbilityFinal))
                    active.Add(link);
            active.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            for (var i = 0; i < active.Count; i++)
                if (active[i].Sphere == sphere && active[i].SphereBranch == sphereBranch &&
                    active[i].Ability == ability && active[i].AbilityFinal == abilityFinal)
                    return 1f / (1f + i * 0.18f);
            return 1f;
        }

        public SphereAttackAgent FindNearestSphereOfType(SphereId type, Vector2 origin)
        {
            SphereAttackAgent nearest = null;
            var nearestDistance = float.PositiveInfinity;
            foreach (var sphere in _spheres)
            {
                if (sphere == null || !sphere.gameObject.activeInHierarchy || sphere.Type != type) continue;
                var distance = Vector2.Distance(origin, sphere.transform.position);
                if (distance < nearestDistance) { nearest = sphere; nearestDistance = distance; }
            }
            return nearest;
        }

        public SphereAttackAgent FindNearestSphereForDrone(SphereId type, Vector2 origin)
        {
            var matching = FindNearestSphereOfType(type, origin);
            if (matching != null) return matching;
            SphereAttackAgent nearest = null;
            var nearestDistance = float.PositiveInfinity;
            foreach (var sphere in _spheres)
            {
                if (sphere == null || !sphere.gameObject.activeInHierarchy) continue;
                var distance = Vector2.Distance(origin, sphere.transform.position);
                if (distance < nearestDistance) { nearest = sphere; nearestDistance = distance; }
            }
            return nearest;
        }

        public SphereAttackAgent FindNearestSphereNearDrone(Vector2 origin, SphereAttackAgent excluded, float radiusWorld)
        {
            SphereAttackAgent nearest = null;
            var nearestDistance = Mathf.Max(0f, radiusWorld);
            foreach (var sphere in _spheres)
            {
                if (sphere == null || sphere == excluded || !sphere.gameObject.activeInHierarchy) continue;
                var distance = Vector2.Distance(origin, sphere.transform.position);
                if (distance < nearestDistance) { nearest = sphere; nearestDistance = distance; }
            }
            return nearest;
        }

        private void ApplyTimeStopSynergyRiders(int level)
        {
            var scale = SpherePlacementRules.SourcePixelsToWorld;
            if (HasActiveAbilitySynergy(SphereId.Standard, "standard_singularity", RunUpgradeKind.TimeStop, "timestop_echo_phase_f1"))
            {
                var standard = FindNearestSphereOfType(SphereId.Standard, _player.position);
                if (standard != null)
                    foreach (var enemy in _enemies)
                    {
                        if (enemy == null || !enemy.IsAlive) continue;
                        var delta = (Vector2)standard.transform.position - (Vector2)enemy.transform.position;
                        if (delta.magnitude > 150f * scale) continue;
                        var direction = delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector2.zero;
                        enemy.transform.position += (Vector3)(direction * 42f * scale);
                        enemy.ApplyFreeze(_timeStopTimer + 0.45f);
                    }
            }

            if (HasActiveAbilitySynergy(SphereId.Chain, "chain_web", RunUpgradeKind.TimeStop, "timestop_closed_time_f2"))
                foreach (var sphere in _spheres)
                {
                    if (sphere == null || !sphere.gameObject.activeInHierarchy || sphere.Type != SphereId.Chain) continue;
                    foreach (var enemy in _enemies)
                        if (enemy != null && enemy.IsAlive &&
                            Vector2.Distance(enemy.transform.position, sphere.transform.position) < 150f * scale)
                            enemy.ApplyFreeze(_timeStopTimer + 0.75f);
                }

            if (HasActiveAbilitySynergy(SphereId.Gravity, "gravity_tide", RunUpgradeKind.TimeStop, "timestop_time_anchor_f3"))
                foreach (var sphere in _spheres)
                {
                    if (sphere == null || !sphere.gameObject.activeInHierarchy || sphere.Type != SphereId.Gravity) continue;
                    foreach (var enemy in _enemies)
                    {
                        if (enemy == null || !enemy.IsAlive) continue;
                        var delta = (Vector2)enemy.transform.position - (Vector2)sphere.transform.position;
                        if (delta.magnitude > 155f * scale) continue;
                        var direction = delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector2.zero;
                        enemy.transform.position += (Vector3)(direction * 58f * scale);
                        enemy.ReceiveDamage((12f + level * 3f) * 0.55f);
                    }
                }
        }

        private void ApplyTeleportSynergyRiders(int level)
        {
            var scale = SpherePlacementRules.SourcePixelsToWorld;
            if (HasActiveAbilitySynergy(SphereId.Sniper, "sniper_oracle", RunUpgradeKind.Teleport, "teleport_echo_jump_f1"))
            {
                var sniper = FindNearestSphereOfType(SphereId.Sniper, _player.position);
                var target = FindNearestEnemy(sniper == null ? (Vector2)_player.position : (Vector2)sniper.transform.position, float.PositiveInfinity);
                if (sniper != null && target != null)
                {
                    _hunterMarkTarget = target;
                    _hunterMarkTimer = Mathf.Max(_hunterMarkTimer, 5f);
                    target.ApplyRuneMark(5f);
                    target.ReceiveDamage((18f + level * 5f) * 0.75f);
                }
            }

            if (HasActiveAbilitySynergy(SphereId.Aura, "aura_gravity", RunUpgradeKind.Teleport, "teleport_beacon_f2"))
                foreach (var sphere in _spheres)
                {
                    if (sphere == null || !sphere.gameObject.activeInHierarchy || sphere.Type != SphereId.Aura) continue;
                    foreach (var enemy in _enemies)
                    {
                        if (enemy == null || !enemy.IsAlive) continue;
                        var delta = (Vector2)sphere.transform.position - (Vector2)enemy.transform.position;
                        if (delta.magnitude > 125f * scale) continue;
                        var direction = delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector2.zero;
                        enemy.transform.position += (Vector3)(direction * 58f * scale);
                    }
                }

            if (HasActiveAbilitySynergy(SphereId.Prism, "prism_mirror", RunUpgradeKind.Teleport, "teleport_phase_f3"))
            {
                var targets = new List<EnemyAgent2D>();
                foreach (var enemy in _enemies)
                    if (enemy != null && enemy.IsAlive) targets.Add(enemy);
                targets.Sort((a, b) => Vector2.Distance(a.transform.position, _player.position).CompareTo(
                    Vector2.Distance(b.transform.position, _player.position)));
                var prism = FindNearestSphereOfType(SphereId.Prism, _player.position);
                for (var i = 0; i < Mathf.Min(3, targets.Count); i++)
                {
                    targets[i].ReceiveDamage((18f + level * 5f) * 0.55f);
                    if (prism != null) SpawnImpact(targets[i].transform.position, new Color(1f, 0.55f, 0.9f));
                }
            }

            if (HasActiveAbilitySynergy(SphereId.Void, "void_execution", RunUpgradeKind.Teleport, "teleport_echo_jump_f2"))
                foreach (var sphere in _spheres)
                {
                    if (sphere == null || !sphere.gameObject.activeInHierarchy || sphere.Type != SphereId.Void) continue;
                    foreach (var enemy in _enemies)
                    {
                        if (enemy == null || !enemy.IsAlive ||
                            Vector2.Distance(enemy.transform.position, sphere.transform.position) > 155f * scale) continue;
                        var hpRatio = enemy.CurrentHp / Mathf.Max(1f, enemy.MaxHp);
                        if (hpRatio <= 0.30f && !enemy.IsBoss) enemy.ReceiveDamage(enemy.CurrentHp + 1f);
                        else enemy.ReceiveDamage((20f + level * 5f) * 0.70f);
                    }
                }
        }



        private void ApplyActiveAbilitySynergyRiders(RunUpgradeKind ability, int level)
        {
            _applyingActiveAbilitySynergyRiders = true;
            _activeAbilitySynergyPower = 1f;
            var scale = SpherePlacementRules.SourcePixelsToWorld;
            if (ability == RunUpgradeKind.Blast)
            {
                if (HasActiveAbilitySynergy(SphereId.Standard, "standard_resonator", ability, "blast_resonance_f1"))
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Standard)
                            EmitSourceSpherePulse(sphere, sphere.transform.position, 92f * scale, (26f + level * 8f) * 0.30f, new Color(0.83f, 0.43f, 0.24f));
                if (HasActiveAbilitySynergy(SphereId.Shotgun, "shotgun_cataclysm", ability, "blast_network_f2"))
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Shotgun)
                            EmitSourceSpherePulse(sphere, sphere.transform.position, 88f * scale, (30f + (level - 1) * 10f) * 0.42f, new Color(1f, 0.82f, 0.42f));
                if (HasActiveAbilitySynergy(SphereId.Prism, "prism_split", ability, "blast_core_f3"))
                {
                    var targets = FindNearestEnemies(_player.position, float.PositiveInfinity, 3);
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Prism)
                            foreach (var target in targets)
                            {
                                if (target == null || !target.IsAlive) continue;
                                SpawnCombatArc(sphere.transform.position, target.transform.position, SphereId.Prism,
                                    GetSphereBranch(SphereId.Prism), GetSphereFinal(SphereId.Prism), new Color(0.82f, 0.48f, 1f), 0.18f);
                                target.ReceiveDamage((30f + (level - 1) * 10f) * 0.28f);
                            }
                }
                if (HasActiveAbilitySynergy(SphereId.Gravity, "gravity_collapse", ability, "blast_network_f1"))
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Gravity)
                        {
                            foreach (var enemy in _enemies)
                                if (enemy != null && enemy.IsAlive && Vector2.Distance(enemy.transform.position, sphere.transform.position) <= 145f * scale)
                                {
                                    enemy.PullToward(sphere.transform.position, 48f * scale);
                                    enemy.ReceiveDamage((30f + (level - 1) * 10f) * 0.25f);
                                }
                            EmitSourceSpherePulse(sphere, sphere.transform.position, 82f * scale,
                                (30f + (level - 1) * 10f) * 0.22f, new Color(0.63f, 0.42f, 1f));
                        }
                if (HasActiveAbilitySynergy(SphereId.Pulse, "pulse_wave", ability, "blast_core_f2"))
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Pulse)
                            foreach (var enemy in _enemies)
                                if (enemy != null && enemy.IsAlive && Vector2.Distance(enemy.transform.position, sphere.transform.position) <= 92f * scale)
                                    enemy.KnockBackFrom(sphere.transform.position, 46f * scale);
            }
            else if (ability == RunUpgradeKind.Shield)
            {
                if (HasActiveAbilitySynergy(SphereId.Shotgun, "shotgun_burst", ability, "shield_echo_guard_f1"))
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Shotgun)
                            foreach (var enemy in _enemies)
                                if (enemy != null && enemy.IsAlive && Vector2.Distance(enemy.transform.position, sphere.transform.position) <= 95f * scale)
                                    enemy.KnockBackFrom(sphere.transform.position, 62f * scale);
                if (HasActiveAbilitySynergy(SphereId.Aura, "aura_sanctum", ability, "shield_reflector_f2"))
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Aura)
                        {
                            _playerCore?.GrantShieldCharge(1);
                            foreach (var enemy in _enemies)
                                if (enemy != null && enemy.IsAlive && Vector2.Distance(enemy.transform.position, sphere.transform.position) <= 105f * scale)
                                    enemy.ApplySlow(_shieldTimer + 0.5f, 0.68f);
                        }
                if (HasActiveAbilitySynergy(SphereId.Orbital, "orbital_halo", ability, "shield_bastion_f3"))
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Orbital)
                        {
                            _playerCore?.GrantShieldCharge(1);
                            EmitSourceSpherePulse(sphere, sphere.transform.position, 72f * scale, (10f + level * 2f) * 0.45f, new Color(0.72f, 0.90f, 1f));
                        }
                if (HasActiveAbilitySynergy(SphereId.Pulse, "pulse_burst", ability, "shield_echo_guard_f2"))
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Pulse)
                            EmitSourceSpherePulse(sphere, sphere.transform.position, 92f * scale, (10f + level * 2f) * 0.65f, new Color(1f, 0.92f, 0.65f));
            }
            else if (ability == RunUpgradeKind.FireTrail)
            {
                if (HasActiveAbilitySynergy(SphereId.Sniper, "sniper_beacon", ability, "firetrail_overdrive_f1"))
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Sniper)
                            foreach (var enemy in _enemies)
                                if (enemy != null && enemy.IsAlive && Vector2.Distance(enemy.transform.position, sphere.transform.position) <= 115f * scale)
                                {
                                    enemy.ApplyFireStatus(2.2f, 6f + level * 2f);
                                    enemy.ApplySlow(2f, 0.72f);
                                }
                if (HasActiveAbilitySynergy(SphereId.Aura, "aura_overgrowth", ability, "firetrail_sanctum_f3"))
                    foreach (var aura in _spheres)
                        if (aura != null && aura.gameObject.activeInHierarchy && aura.Type == SphereId.Aura)
                            foreach (var sphere in _spheres)
                                if (sphere != null && sphere.gameObject.activeInHierarchy && Vector2.Distance(sphere.transform.position, aura.transform.position) <= 150f * scale)
                                    sphere.AccelerateNextAttack(0.5f);
                if (HasActiveAbilitySynergy(SphereId.Prism, "prism_spectrum", ability, "firetrail_ignition_f2"))
                    foreach (var prism in _spheres)
                        if (prism != null && prism.gameObject.activeInHierarchy && prism.Type == SphereId.Prism)
                            foreach (var enemy in _enemies)
                                if (enemy != null && enemy.IsAlive && Vector2.Distance(enemy.transform.position, prism.transform.position) <= 105f * scale)
                                {
                                    var hadStatus = enemy.HasFreezeStatus || enemy.HasPoisonStatus;
                                    enemy.ApplyFireStatus(1.8f, 6f + level * 2f);
                                    if (hadStatus) enemy.ReceiveDamage((16f + level * 4f) * 0.85f);
                                }
                if (HasActiveAbilitySynergy(SphereId.Gravity, "gravity_well", ability, "firetrail_overdrive_f3"))
                    foreach (var gravity in _spheres)
                        if (gravity != null && gravity.gameObject.activeInHierarchy && gravity.Type == SphereId.Gravity)
                            foreach (var enemy in _enemies)
                                if (enemy != null && enemy.IsAlive && Vector2.Distance(enemy.transform.position, gravity.transform.position) <= 125f * scale)
                                {
                                    enemy.ApplyFireStatus(3f, 6f + level * 2f);
                                    enemy.ApplySlow(2.5f, 0.60f);
                                }
            }
            else if (ability == RunUpgradeKind.Minion)
            {
                if (HasActiveAbilitySynergy(SphereId.Standard, "standard_swarm", ability, "minion_echo_drone_f1"))
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Standard)
                            EmitSourceSpherePulse(sphere, sphere.transform.position, 72f * scale, (10f + level * 2f) * 0.55f, new Color(0.45f, 0.95f, 0.58f));
                if (HasActiveAbilitySynergy(SphereId.Orbital, "orbital_dance", ability, "minion_relay_drone_f2"))
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Orbital)
                        {
                            sphere.AccelerateNextAttack(0.65f);
                            EmitSourceSpherePulse(sphere, sphere.transform.position, 62f * scale, (8f + level * 2f) * 0.5f, new Color(0.40f, 0.82f, 1f));
                        }
                if (HasActiveAbilitySynergy(SphereId.Void, "void_reaper", ability, "minion_guardian_f3"))
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Void)
                        {
                            var kills = 0;
                            foreach (var enemy in _enemies)
                                if (enemy != null && enemy.IsAlive && Vector2.Distance(enemy.transform.position, sphere.transform.position) <= 92f * scale)
                                {
                                    enemy.ReceiveDamage((10f + level * 2f) * 0.55f);
                                    if (!enemy.IsAlive) kills++;
                                }
                            if (kills > 0) _playerCore?.Heal(kills * 2f);
                        }
            }
            else if (ability == RunUpgradeKind.Lightning)
            {
                if (HasActiveAbilitySynergy(SphereId.Shotgun, "shotgun_hail", ability, "lightning_echo_storm_f1"))
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Shotgun)
                            foreach (var enemy in FindNearestEnemies(sphere.transform.position, 250f * scale, 3))
                            {
                                SpawnCombatArc(sphere.transform.position, enemy.transform.position, SphereId.Chain,
                                    GetSphereBranch(SphereId.Chain), GetSphereFinal(SphereId.Chain), new Color(0.65f, 0.42f, 1f), 0.14f);
                                enemy.ReceiveDamage((20f + level * 5f) * 0.40f);
                            }
                if (HasActiveAbilitySynergy(SphereId.Chain, "chain_storm", ability, "lightning_relay_f2"))
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Chain)
                            foreach (var enemy in FindNearestEnemies(sphere.transform.position, 95f * scale, 2))
                            {
                                SpawnCombatArc(sphere.transform.position, enemy.transform.position, SphereId.Chain,
                                    GetSphereBranch(SphereId.Chain), GetSphereFinal(SphereId.Chain), new Color(1f, 0.88f, 0.35f), 0.12f);
                                enemy.ReceiveDamage((8f + level * 3f) * 0.80f);
                            }
                if (HasActiveAbilitySynergy(SphereId.Pulse, "pulse_resonator", ability, "lightning_echo_storm_f3"))
                {
                    AddResonanceCharge(0.45f * _activeAbilitySynergyPower);
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Pulse)
                            sphere.AccelerateNextAttack(0.45f * _activeAbilitySynergyPower);
                }
            }
            else if (ability == RunUpgradeKind.DarkRitual)
            {
                if (HasActiveAbilitySynergy(SphereId.Sniper, "sniper_assassin", ability, "darkritual_blood_link_f1"))
                {
                    EnemyAgent2D weakest = null;
                    var ratio = float.PositiveInfinity;
                    foreach (var enemy in _enemies)
                        if (enemy != null && enemy.IsAlive && enemy.HpFraction < ratio) { ratio = enemy.HpFraction; weakest = enemy; }
                    if (weakest != null) weakest.ReceiveDamage(Mathf.Min(weakest.CurrentHp, Mathf.Max(18f, weakest.MaxHp * 0.08f)));
                }
                if (HasActiveAbilitySynergy(SphereId.Chain, "chain_leech", ability, "darkritual_sacrifice_f2"))
                    _playerCore?.Heal(Mathf.Min(_playerCore.MaxHp * 0.08f, _playerCore.MaxHp * 0.04f * _activeAbilitySynergyPower));
                if (HasActiveAbilitySynergy(SphereId.Orbital, "orbital_blade", ability, "darkritual_void_pact_f3"))
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Orbital)
                            EmitSourceSpherePulse(sphere, sphere.transform.position, 85f * scale, (22f + level * 5f) * 0.48f, new Color(1f, 0.75f, 0.40f));
                if (HasActiveAbilitySynergy(SphereId.Void, "void_hunger", ability, "darkritual_blood_link_f2"))
                {
                    var missing = 1f - (_playerCore != null ? _playerCore.CurrentHp / Mathf.Max(1f, _playerCore.MaxHp) : 0f);
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Void)
                            EmitSourceSpherePulse(sphere, sphere.transform.position, 92f * scale, (18f + level * 5f) * (0.65f + missing * 0.75f), new Color(0.77f, 0.55f, 1f));
                }
            }
            ApplyActiveAbilityFinalVariantRiders(ability, level);
            _applyingActiveAbilitySynergyRiders = false;
            _activeAbilitySynergyPower = 1f;
        }

        private void ApplyActiveAbilityFinalVariantRiders(RunUpgradeKind ability, int level)
        {
            var active = new List<ActiveAbilitySynergyLink>();
            foreach (var link in ActiveAbilitySynergyLinks)
                if (link.Ability == ability && IsActiveAbilitySynergyRaw(link.Sphere, link.SphereBranch, link.Ability, link.AbilityFinal))
                    active.Add(link);
            active.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            for (var i = 0; i < active.Count; i++)
            {
                var link = active[i];
                var power = 1f / (1f + i * 0.18f);
                var variant = GetAbilityFinalVariant(link.Ability) - 1;
                _activeAbilitySynergyPower = power;
                ApplyActiveAbilityFinalVariantRider(link, level, power, variant);
            }
        }

        private void ApplyActiveAbilityFinalVariantRider(ActiveAbilitySynergyLink link, int level, float power, int variant)
        {
            if (variant < 0 || variant > 2) return;
            var scale = SpherePlacementRules.SourcePixelsToWorld;
            var spheres = new List<SphereAttackAgent>();
            foreach (var sphere in _spheres)
                if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == link.Sphere) spheres.Add(sphere);
            var enemies = new List<EnemyAgent2D>();
            foreach (var enemy in _enemies) if (enemy != null && enemy.IsAlive) enemies.Add(enemy);

            switch (link.Id)
            {
                case "std_res_blast":
                    if (variant == 0)
                    {
                        var target = FindNearestEnemy(PlayerPosition, float.PositiveInfinity);
                        if (target != null && spheres.Count > 0) target.ReceiveDamage((10f + level * 2f) * power);
                    }
                    else if (variant == 1) foreach (var sphere in spheres) sphere.AccelerateNextAttack(0.65f * power);
                    else if (spheres.Count > 0)
                    {
                        var origin = (Vector2)spheres[spheres.Count - 1].transform.position;
                        foreach (var target in FindNearestEnemies(origin, 110f * scale, 4))
                            target.ReceiveDamage((7f + level) * power);
                    }
                    break;

                case "std_singularity_timestop":
                    foreach (var sphere in spheres)
                        foreach (var enemy in FindNearestEnemies(sphere.transform.position, 150f * scale, 6))
                        {
                            if (variant == 0) enemy.ApplyFreeze(0.8f * power);
                            else if (variant == 1) enemy.ApplySlow(1.6f * power, 0.5f);
                            else enemy.ReceiveDamage((8f + level * 2f) * power);
                        }
                    break;

                case "std_swarm_minion":
                    foreach (var sphere in spheres)
                    {
                        if (variant == 0) EmitSourceSpherePulse(sphere, sphere.transform.position, 70f * scale, 9f + level * 2f, new Color(0.45f, 0.95f, 0.58f));
                        else if (variant == 1) sphere.AccelerateNextAttack(0.8f * power);
                        else foreach (var enemy in FindNearestEnemies(sphere.transform.position, 90f * scale, 3))
                            enemy.ReceiveDamage((7f + level) * power);
                    }
                    break;

                case "sniper_oracle_teleport":
                    foreach (var sphere in spheres)
                    {
                        var target = FindNearestEnemy(sphere.transform.position, 190f * scale);
                        if (target == null) continue;
                        if (variant == 0) target.ApplyMark(4f);
                        else if (variant == 1) target.ReceiveDamage((14f + level * 3f) * power);
                        else target.ApplySlow(1.1f, 0.58f);
                    }
                    break;

                case "sniper_assassin_darkritual":
                    {
                        EnemyAgent2D target = null;
                        var lowest = float.PositiveInfinity;
                        foreach (var enemy in enemies)
                            if (enemy.HpFraction < lowest) { lowest = enemy.HpFraction; target = enemy; }
                        if (target == null) break;
                        if (variant == 0) target.ReceiveDamage(Mathf.Min(target.CurrentHp, Mathf.Max(12f, target.MaxHp * 0.05f)) * power);
                        else if (variant == 1) target.ApplySlow(1.2f, 0.55f);
                        else _playerCore?.Heal(Mathf.Min(_playerCore.MaxHp * 0.03f, target.MaxHp * 0.03f) * power);
                        break;
                    }

                case "sniper_beacon_firetrail":
                    foreach (var sphere in spheres)
                        foreach (var enemy in FindNearestEnemies(sphere.transform.position, 125f * scale, 5))
                        {
                            if (variant == 0) enemy.ExtendFireStatus(2.5f);
                            else if (variant == 1) enemy.ApplySlow(1.8f, 0.58f);
                            else enemy.ReceiveDamage((9f + level * 2f) * power);
                        }
                    break;

                case "shotgun_burst_shield":
                    foreach (var sphere in spheres)
                        foreach (var enemy in FindNearestEnemies(sphere.transform.position, 110f * scale, 6))
                        {
                            if (variant == 0) enemy.KnockBackFrom(sphere.transform.position, 34f * power * scale);
                            else if (variant == 1) enemy.ReceiveDamage((12f + level * 2f) * power);
                            else enemy.ApplySlow(1f, 0.55f);
                        }
                    break;

                case "shotgun_cataclysm_blast":
                    foreach (var sphere in spheres)
                    {
                        var radius = (variant == 1 ? 105f : 85f) * scale;
                        var targets = FindNearestEnemies(sphere.transform.position, radius, variant == 0 ? 3 : variant == 1 ? 5 : 1);
                        if (variant == 0) foreach (var enemy in targets) enemy.ReceiveDamage((12f + level * 2f) * power);
                        else if (variant == 1) foreach (var enemy in targets) enemy.KnockBackFrom(sphere.transform.position, 28f * power * scale);
                        else sphere.AccelerateNextAttack(0.9f * power);
                    }
                    break;

                case "shotgun_hail_lightning":
                    foreach (var sphere in spheres)
                        foreach (var enemy in FindNearestEnemies(sphere.transform.position, 250f * scale, 3))
                        {
                            if (variant == 0) enemy.ReceiveDamage((9f + level * 2f) * power);
                            else if (variant == 1) enemy.ApplySlow(1f, 0.58f);
                            else sphere.AccelerateNextAttack(0.75f * power);
                        }
                    break;

                case "chain_web_timestop":
                    foreach (var sphere in spheres)
                    {
                        var radius = variant == 0 ? 170f : variant == 1 ? 120f : 105f;
                        var count = variant == 0 ? 4 : variant == 1 ? 6 : 3;
                        foreach (var enemy in FindNearestEnemies(sphere.transform.position, radius * scale, count))
                        {
                            if (variant == 0) enemy.ApplyFreeze(0.8f * power);
                            else if (variant == 1) enemy.ApplySlow(1.2f, 0.55f);
                            else enemy.ReceiveDamage((9f + level) * power);
                        }
                    }
                    break;

                case "chain_storm_lightning":
                    foreach (var sphere in spheres)
                    {
                        var targets = FindNearestEnemies(sphere.transform.position, 105f * scale, 3);
                        if (variant == 0) foreach (var enemy in targets) enemy.ReceiveDamage((9f + level * 2f) * power);
                        else if (variant == 1) foreach (var enemy in targets)
                            SpawnCombatArc(sphere.transform.position, enemy.transform.position, SphereId.Chain,
                                GetSphereBranch(SphereId.Chain), GetSphereFinal(SphereId.Chain), new Color(0.63f, 0.42f, 1f), 0.16f);
                        else
                        {
                            sphere.AccelerateNextAttack(0.75f * power);
                            AddResonanceCharge(0.4f * power);
                        }
                    }
                    break;

                case "chain_leech_darkritual":
                    if (variant == 0) _playerCore?.Heal(_playerCore.MaxHp * 0.035f * power);
                    else if (variant == 1) AddResonanceCharge(0.55f * power);
                    else foreach (var sphere in spheres) sphere.AccelerateNextAttack(0.7f * power);
                    break;

                case "aura_sanctum_shield":
                    foreach (var sphere in spheres)
                    {
                        if (variant == 0) _playerCore?.GrantShieldCharge(1);
                        else if (variant == 1)
                            foreach (var enemy in FindNearestEnemies(sphere.transform.position, 110f * scale, 5)) enemy.ApplySlow(1.4f, 0.55f);
                        else EmitSourceSpherePulse(sphere, sphere.transform.position, 90f * scale, 14f + level * 2f, new Color(0.35f, 0.78f, 1f));
                    }
                    break;

                case "aura_gravity_teleport":
                    foreach (var sphere in spheres)
                        foreach (var enemy in FindNearestEnemies(sphere.transform.position, 140f * scale, 6))
                        {
                            if (variant == 0) enemy.ExtendSlowStatus(1.3f);
                            else if (variant == 1) enemy.PullToward(sphere.transform.position, 34f * power * scale);
                            else enemy.ReceiveDamage((9f + level) * power);
                        }
                    break;

                case "aura_overgrowth_firetrail":
                    foreach (var aura in spheres)
                    {
                        var allies = new List<SphereAttackAgent>();
                        foreach (var sphere in _spheres)
                            if (sphere != null && sphere.gameObject.activeInHierarchy && sphere != aura &&
                                Vector2.Distance(sphere.transform.position, aura.transform.position) < 155f * scale) allies.Add(sphere);
                        if (variant == 0) foreach (var sphere in allies) sphere.AccelerateNextAttack(0.7f * power);
                        else if (variant == 1)
                            foreach (var enemy in FindNearestEnemies(aura.transform.position, 105f * scale, 5)) enemy.ApplyFireStatus(2.2f, 5f + level);
                        else if (allies.Count > 0) EmitSourceSpherePulse(allies[0], allies[0].transform.position, 72f * scale, 12f + level * 2f, new Color(1f, 0.48f, 0.18f));
                    }
                    break;

                case "orbital_dance_minion":
                    foreach (var sphere in spheres)
                    {
                        if (variant == 0) sphere.AccelerateNextAttack(1.1f * power);
                        else if (variant == 1) EmitSourceSpherePulse(sphere, sphere.transform.position, 80f * scale, 12f + level * 2f, new Color(0.40f, 0.82f, 1f));
                        else foreach (var enemy in FindNearestEnemies(sphere.transform.position, 100f * scale, 4))
                            enemy.ReceiveDamage((7f + level) * power);
                    }
                    break;

                case "orbital_halo_shield":
                    foreach (var sphere in spheres)
                    {
                        if (variant == 0) _playerCore?.GrantShieldCharge(1);
                        else if (variant == 1) EmitSourceSpherePulse(sphere, sphere.transform.position, 78f * scale, 13f + level * 2f, new Color(0.72f, 0.90f, 1f));
                        else sphere.AccelerateNextAttack(0.8f * power);
                    }
                    break;

                case "orbital_blade_darkritual":
                    foreach (var sphere in spheres)
                    {
                        if (variant == 0) EmitSourceSpherePulse(sphere, sphere.transform.position, 90f * scale, 15f + level * 3f, new Color(1f, 0.75f, 0.40f));
                        else if (variant == 1) sphere.AccelerateNextAttack(0.9f * power);
                        else foreach (var enemy in FindNearestEnemies(sphere.transform.position, 105f * scale, 4))
                            enemy.ReceiveDamage((9f + level * 2f) * power);
                    }
                    break;

                case "prism_split_blast":
                    foreach (var sphere in spheres)
                    {
                        var targets = FindNearestEnemies(sphere.transform.position, 180f * scale, variant == 0 ? 2 : variant == 1 ? 4 : 3);
                        if (variant == 0) foreach (var enemy in targets) enemy.ReceiveDamage((10f + level * 2f) * power);
                        else if (variant == 1) foreach (var enemy in targets) enemy.ApplySlow(0.7f, 0.72f);
                        else foreach (var enemy in targets)
                            SpawnCombatArc(sphere.transform.position, enemy.transform.position, SphereId.Prism,
                                GetSphereBranch(SphereId.Prism), GetSphereFinal(SphereId.Prism), new Color(0.82f, 0.48f, 1f), 0.18f);
                    }
                    break;

                case "prism_spectrum_firetrail":
                    foreach (var sphere in spheres)
                        foreach (var enemy in FindNearestEnemies(sphere.transform.position, 115f * scale, 5))
                        {
                            if (variant == 0) enemy.ReceiveDamage((11f + level * 2f) * power);
                            else if (variant == 1) enemy.ApplyFireStatus(2.8f, 7f + level);
                            else enemy.ApplySlow(1f, 0.62f);
                        }
                    break;

                case "prism_mirror_teleport":
                    foreach (var sphere in spheres)
                        foreach (var enemy in FindNearestEnemies(sphere.transform.position, 190f * scale, variant == 0 ? 2 : variant == 1 ? 4 : 3))
                        {
                            if (variant == 0) enemy.ReceiveDamage((12f + level * 2f) * power);
                            else if (variant == 1) SpawnCombatArc(sphere.transform.position, enemy.transform.position, SphereId.Prism,
                                GetSphereBranch(SphereId.Prism), GetSphereFinal(SphereId.Prism), new Color(0.82f, 0.48f, 1f), 0.16f);
                            else enemy.ApplySlow(0.9f, 0.65f);
                        }
                    break;

                case "gravity_well_firetrail":
                    foreach (var sphere in spheres)
                        foreach (var enemy in FindNearestEnemies(sphere.transform.position, 135f * scale, 6))
                        {
                            if (variant == 0) enemy.ReceiveDamage((10f + level * 2f) * power);
                            else if (variant == 1) enemy.ApplySlow(1.8f, 0.5f);
                            else enemy.ApplyFireStatus(4f, 7f + level);
                        }
                    break;

                case "gravity_tide_timestop":
                    foreach (var sphere in spheres)
                        foreach (var enemy in FindNearestEnemies(sphere.transform.position, 160f * scale, 6))
                        {
                            if (variant == 0) enemy.KnockBackFrom(sphere.transform.position, 72f * power * scale);
                            else if (variant == 1) enemy.KnockBackFrom(sphere.transform.position, 38f * power * scale);
                            else
                            {
                                enemy.KnockBackFrom(sphere.transform.position, 20f * power * scale);
                                enemy.ReceiveDamage((10f + level * 2f) * power);
                            }
                        }
                    break;

                case "gravity_collapse_blast":
                    foreach (var sphere in spheres)
                    {
                        var targets = FindNearestEnemies(sphere.transform.position, (variant == 1 ? 165f : 135f) * scale, 5);
                        if (variant == 0) foreach (var enemy in targets) enemy.PullToward(sphere.transform.position, 22f * power * scale);
                        else if (variant == 1) foreach (var enemy in targets) enemy.ReceiveDamage((11f + level * 2f) * power);
                        else foreach (var enemy in targets) enemy.ApplySlow(1.1f, 0.62f);
                    }
                    break;

                case "pulse_wave_blast":
                    foreach (var sphere in spheres)
                    {
                        if (variant == 0) sphere.AccelerateNextAttack(1f * power);
                        else if (variant == 1) AddResonanceCharge(0.75f * power);
                        else foreach (var enemy in FindNearestEnemies(sphere.transform.position, 100f * scale, 6))
                            enemy.ReceiveDamage((8f + level) * power);
                    }
                    break;

                case "pulse_resonator_lightning":
                    foreach (var sphere in spheres)
                    {
                        if (variant == 0) AddResonanceCharge(0.75f * power);
                        else if (variant == 1) sphere.AccelerateNextAttack(1f * power);
                        else EmitSourceSpherePulse(sphere, sphere.transform.position, 86f * scale, 10f + level * 2f, new Color(0.63f, 0.42f, 1f));
                    }
                    break;

                case "pulse_burst_shield":
                    foreach (var sphere in spheres)
                    {
                        if (variant == 0) EmitSourceSpherePulse(sphere, sphere.transform.position, 95f * scale, 12f + level * 2f, new Color(1f, 0.92f, 0.65f));
                        else if (variant == 1)
                        {
                            sphere.AccelerateNextAttack(0.9f * power);
                            AddResonanceCharge(0.35f * power);
                        }
                        else foreach (var enemy in FindNearestEnemies(sphere.transform.position, 95f * scale, 5))
                            enemy.ReceiveDamage((8f + level) * power);
                    }
                    break;

                case "void_hunger_darkritual":
                    {
                        var missing = 1f - (_playerCore != null ? _playerCore.CurrentHp / Mathf.Max(1f, _playerCore.MaxHp) : 0f);
                        foreach (var sphere in spheres)
                        {
                            if (variant == 0) EmitSourceSpherePulse(sphere, sphere.transform.position, 95f * scale,
                                (12f + level * 2f) * (0.7f + missing), new Color(0.77f, 0.55f, 1f));
                            else if (variant == 1)
                            {
                                if (missing > 0.45f && _playerCore != null)
                                    _playerCore.Heal(_playerCore.MaxHp * 0.025f * power);
                            }
                            else sphere.AccelerateNextAttack((0.5f + missing * 0.8f) * power);
                        }
                        break;
                    }

                case "void_reaper_minion":
                    foreach (var sphere in spheres)
                    {
                        if (variant == 0)
                            foreach (var enemy in FindNearestEnemies(sphere.transform.position, 105f * scale, 3))
                                enemy.ReceiveDamage((8f + level * 2f) * power);
                        else if (variant == 1)
                        {
                            var missing = 1f - (_playerCore != null ? _playerCore.CurrentHp / Mathf.Max(1f, _playerCore.MaxHp) : 0f);
                            if (_playerCore != null) _playerCore.Heal(missing * _playerCore.MaxHp * 0.04f * power);
                        }
                        else sphere.AccelerateNextAttack(0.75f * power);
                    }
                    break;

                case "void_execution_teleport":
                    foreach (var sphere in spheres)
                    {
                        if (variant == 0)
                            foreach (var enemy in FindNearestEnemies(sphere.transform.position, 155f * scale, 3))
                            {
                                var hpRatio = enemy.CurrentHp / Mathf.Max(1f, enemy.MaxHp);
                                if (hpRatio <= 0.30f && !enemy.IsBoss) enemy.ReceiveDamage(enemy.CurrentHp + 1f);
                                else enemy.ReceiveDamage((8f + level) * power);
                            }
                        else if (variant == 1) AddResonanceCharge(0.55f * power);
                        else
                            foreach (var enemy in FindEnemiesInRadius(sphere.transform.position, 155f * scale))
                            {
                                var hpRatio = enemy.CurrentHp / Mathf.Max(1f, enemy.MaxHp);
                                if (hpRatio <= 0.30f && !enemy.IsBoss) enemy.ReceiveDamage(enemy.CurrentHp + 1f);
                                else enemy.ReceiveDamage((20f + level * 5f) * 0.70f * power);
                            }
                    }
                    break;
            }
        }

        private void EmitSourceSpherePulse(SphereAttackAgent source, Vector2 center, float radiusWorld, float damage, Color color, bool slow = false)
        {
            DamageEnemiesNearFromSphere(source, center, radiusWorld, damage, color, slow);
            // Source emitSpherePulse deals damage first, then consumes one seeded
            // random angle for each of its 14 radial particles.
            var angles = new float[14];
            for (var i = 0; i < angles.Length; i++) angles[i] = _rng.NextFloat() * Mathf.PI * 2f;
            var pulse = new GameObject("Sphere Pulse Particles");
            pulse.transform.position = center;
            pulse.AddComponent<SpherePulseVfx2D>().Initialize(color, SpherePlacementRules.SourcePixelsToWorld, angles);
        }

        private void DamageEnemiesNearFromSphere(
            SphereAttackAgent source, Vector2 center, float radiusWorld, float damage, Color color,
            bool slow = false)
        {
            // Source emitSpherePulse/dealDamageToEnemy calls from active abilities
            // omit the sourceSphere argument. Keep the Sphere only as the pulse
            // origin; do not apply authored Sphere modifiers, crits or sphere-hit procs.
            var hitRadius = Mathf.Max(0f, radiusWorld);
            var synergyMultiplier = _applyingActiveAbilitySynergyRiders ? _activeAbilitySynergyPower : 1f;
            var effectiveDamage = damage * synergyMultiplier;
            foreach (var enemy in _enemies)
            {
                if (enemy == null || !enemy.IsAlive || Vector2.Distance(enemy.transform.position, center) > hitRadius) continue;
                enemy.ReceiveDamage(effectiveDamage);
                if (slow) enemy.ApplySlow(1.2f, 0.6f);
            }
        }

        public void DamageEnemiesNear(Vector2 center, float radiusWorld, float damage, Color color)
        {
            var hitRadius = Mathf.Max(0f, radiusWorld);
            var effectiveDamage = damage * (_applyingActiveAbilitySynergyRiders ? _activeAbilitySynergyPower : 1f);
            foreach (var enemy in _enemies)
                if (enemy != null && enemy.IsAlive && Vector2.Distance(enemy.transform.position, center) <= hitRadius)
                    enemy.ReceiveDamage(effectiveDamage);
            SpawnImpact(center, color);
        }

        private void ActivateBlast()
        {
            var level = GetAbilityLevel(RunUpgradeKind.Blast);
            if (level <= 0 || _blastCooldown > 0f) return;
            _blastCooldown = Mathf.Max(8f, (30f - (level - 1) * 2f) * GetActiveAbilityCooldownMultiplier());
            var radius = (125f + level * 12f) * SpherePlacementRules.SourcePixelsToWorld;
            var damage = 30f + (level - 1) * 10f;
            var origin = (Vector2)_player.position;
            var branch = GetAbilityBranch(RunUpgradeKind.Blast);
            var final = GetAbilityFinalArchetype(RunUpgradeKind.Blast);
            var ordered = new List<SphereAttackAgent>();
            foreach (var sphere in _spheres)
                if (sphere != null && sphere.gameObject.activeInHierarchy) ordered.Add(sphere);
            ordered.Sort((a, b) => Vector2.Distance(a.transform.position, origin).CompareTo(
                Vector2.Distance(b.transform.position, origin)));

            if ((branch == "blast_network" || final == "blast_echo_network" || final == "blast_infinite_pulse") &&
                _networkState != null && ordered.Count > 0)
            {
                var remaining = new HashSet<SphereAttackAgent>(ordered);
                var networkOrder = new List<SphereAttackAgent>();
                var current = ordered[0];
                while (current != null)
                {
                    networkOrder.Add(current);
                    remaining.Remove(current);
                    var currentIndex = _spheres.IndexOf(current);
                    SphereAttackAgent next = null;
                    var nextDistance = float.PositiveInfinity;
                    if (currentIndex >= 0)
                    {
                        foreach (var linkedIndex in SphereNetworkRules.GetLinkedNodeIndexes(_networkState, currentIndex))
                        {
                            if (linkedIndex < 0 || linkedIndex >= _spheres.Count) continue;
                            var candidate = _spheres[linkedIndex];
                            if (candidate == null || !remaining.Contains(candidate)) continue;
                            var distance = Vector2.Distance(candidate.transform.position, current.transform.position);
                            if (distance < nextDistance) { nextDistance = distance; next = candidate; }
                        }
                    }
                    current = next;
                }
                if (networkOrder.Count > 0) ordered = networkOrder;
            }

            if (ordered.Count == 0)
            {
                DamageEnemiesNear(origin, 180f * SpherePlacementRules.SourcePixelsToWorld, damage,
                    new Color(0.77f, 0.43f, 0.24f));
            }
            else
            {
                var strength = 1f;
                for (var i = 0; i < ordered.Count; i++)
                {
                    var sphere = ordered[i];
                    var center = (Vector2)sphere.transform.position;
                    EmitSourceSpherePulse(sphere, center, radius, damage * strength, new Color(0.77f, 0.43f, 0.24f), final == "blast_resonant_core");

                    if (branch == "blast_resonance" && sphere.Type == SphereId.Standard)
                        EmitSourceSpherePulse(sphere, center, radius * 0.72f, damage * 0.40f, new Color(0.83f, 0.58f, 0.24f));
                    if (final == "blast_resonant_core" && sphere.Type == SphereId.Standard)
                        EmitSourceSpherePulse(sphere, center, radius * 0.75f, damage * 0.45f, new Color(0.83f, 0.58f, 0.24f));

                    if (branch == "blast_core") strength *= 1.12f;
                    if (branch == "blast_network") strength *= 1.08f;
                    if (final == "blast_echo_network") strength *= 1.15f;
                }

                if (branch == "blast_core")
                    DamageEnemiesNear(origin, 100f * SpherePlacementRules.SourcePixelsToWorld,
                        damage * 0.5f, new Color(0.83f, 0.43f, 0.24f));
                if (branch == "blast_resonance")
                    foreach (var sphere in ordered)
                        if (sphere.Type == SphereId.Standard)
                            EmitSourceSpherePulse(sphere, sphere.transform.position, 90f * SpherePlacementRules.SourcePixelsToWorld,
                                damage * 0.25f, new Color(0.83f, 0.58f, 0.24f));
                if (final == "blast_infinite_pulse")
                    for (var i = ordered.Count - 1; i >= 0; i--)
                        EmitSourceSpherePulse(ordered[i], ordered[i].transform.position, radius * 0.75f,
                            damage * 0.35f, new Color(0.83f, 0.58f, 0.24f));
            }

            ApplyActiveAbilitySynergyRiders(RunUpgradeKind.Blast, level);
            ShowTimedCombatMessage("ECHO PULSE", 0.9f);
        }

        private void ActivateShield()
        {
            var level = GetAbilityLevel(RunUpgradeKind.Shield);
            if (level <= 0 || _shieldCooldown > 0f) return;
            _shieldCooldown = 20f * GetActiveAbilityCooldownMultiplier();
            var nearby = 0;
            foreach (var sphere in _spheres)
                if (sphere != null && sphere.gameObject.activeInHierarchy &&
                    Vector2.Distance(sphere.transform.position, _player.position) <= 260f * SpherePlacementRules.SourcePixelsToWorld)
                    nearby++;

            var branch = GetAbilityBranch(RunUpgradeKind.Shield);
            var final = GetAbilityFinalArchetype(RunUpgradeKind.Shield);
            var networkBonus = level >= 3 ? Mathf.Min(2, nearby / 2) : level >= 2 ? Mathf.Min(1, nearby / 2) : 0;
            var branchBonus = branch == "shield_echo_guard" ? Mathf.Min(2, nearby / 2) : 0;
            var bastionBonus = branch == "shield_bastion" ? 1 : 0;
            var networkGuardBonus = 0;
            if (final == "shield_network_guard" && _networkState != null)
            {
                var connectedSphereCount = 0;
                for (var i = 0; i < _spheres.Count; i++)
                {
                    var sphere = _spheres[i];
                    if (sphere == null || !sphere.gameObject.activeInHierarchy) continue;
                    if (SphereNetworkRules.GetLinkedNodeIndexes(_networkState, i).Count > 0) connectedSphereCount++;
                }
                networkGuardBonus = Mathf.Min(2, connectedSphereCount / 2);
            }

            var charges = Mathf.Min(5, 1 + (level - 1) / 2 + networkBonus + branchBonus + bastionBonus + networkGuardBonus);
            _shieldTimer = 10f + (level >= 5 ? 2f : 0f);
            _shieldVisualPulse = 0.85f;
            if (branch == "shield_bastion") charges++;
            if (final == "shield_iron_dome")
            {
                var protectedCount = 0;
                foreach (var sphere in _spheres)
                    if (sphere != null && sphere.gameObject.activeInHierarchy &&
                        Vector2.Distance(sphere.transform.position, _player.position) <= 300f * SpherePlacementRules.SourcePixelsToWorld)
                        protectedCount++;
                charges += Mathf.Min(2, protectedCount);
            }
            _playerCore?.SetShieldCharges(Mathf.Clamp(charges, 0, 5));

            if (branch == "shield_echo_guard" || final == "shield_network_guard")
                foreach (var sphere in _spheres)
                    if (sphere != null && sphere.gameObject.activeInHierarchy &&
                        Vector2.Distance(sphere.transform.position, _player.position) <= 260f * SpherePlacementRules.SourcePixelsToWorld)
                        sphere.AccelerateNextAttack(0.35f);

            if (final == "shield_network_guard" && _networkState != null)
            {
                for (var i = 0; i < _spheres.Count; i++)
                {
                    var sphere = _spheres[i];
                    if (sphere == null || !sphere.gameObject.activeInHierarchy) continue;
                    foreach (var linkedIndex in SphereNetworkRules.GetLinkedNodeIndexes(_networkState, i))
                    {
                        if (linkedIndex <= i || linkedIndex >= _spheres.Count) continue;
                        var linked = _spheres[linkedIndex];
                        if (linked == null || !linked.gameObject.activeInHierarchy) continue;
                        SpawnCombatArc(sphere.transform.position, linked.transform.position, sphere.Type,
                            branch, final, new Color(0.35f, 0.78f, 1f, 0.9f), 0.22f);
                    }
                }
            }
            ApplyActiveAbilitySynergyRiders(RunUpgradeKind.Shield, level);
            ShowTimedCombatMessage("SPHERE BARRIER", 0.9f);
        }

        private void ActivateFireTrail()
        {
            var level = GetAbilityLevel(RunUpgradeKind.FireTrail);
            if (level <= 0 || _fireTrailCooldown > 0f) return;
            _fireTrailCooldown = 25f * GetActiveAbilityCooldownMultiplier();
            _fireTrailTimer = 5f + Mathf.Min(3, level - 1);
            var branch = GetAbilityBranch(RunUpgradeKind.FireTrail);
            var final = GetAbilityFinalArchetype(RunUpgradeKind.FireTrail);
            var fireSpheres = new List<SphereAttackAgent>();

            foreach (var sphere in _spheres)
            {
                if (sphere == null || !sphere.gameObject.activeInHierarchy || GetGlobalSphereModifierLevel("fire") <= 0) continue;
                fireSpheres.Add(sphere);
                sphere.AccelerateNextAttack(0.5f);
                SpawnImpact(sphere.transform.position, new Color(0.77f, 0.43f, 0.24f, 0.9f));
            }

            if (branch == "firetrail_overdrive")
                foreach (var sphere in fireSpheres)
                    sphere.AccelerateNextAttack(0.7f);

            if (branch == "firetrail_sanctum")
                foreach (var sphere in _spheres)
                    if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == SphereId.Aura)
                        sphere.AccelerateNextAttack(999f);

            if (branch == "firetrail_ignition" || final == "firetrail_catalyst")
                foreach (var enemy in _enemies)
                    if (enemy != null && enemy.IsAlive &&
                        (enemy.HasFireStatus || enemy.HasFreezeStatus || enemy.HasPoisonStatus))
                        enemy.ApplyFireStatus(2f, 5f + level * 2f);

            if (final == "firetrail_catalyst") _fireCatalystTimer = 4f;

            if ((final == "firetrail_network" || final == "firetrail_inferno") &&
                fireSpheres.Count > 1 && _networkState != null)
            {
                var fireIndexes = new HashSet<int>();
                foreach (var sphere in fireSpheres) fireIndexes.Add(_spheres.IndexOf(sphere));
                var processedPairs = new HashSet<string>();
                foreach (var sourceIndex in fireIndexes)
                {
                    if (sourceIndex < 0) continue;
                    foreach (var targetIndex in SphereNetworkRules.GetLinkedNodeIndexes(_networkState, sourceIndex))
                    {
                        if (targetIndex < 0 || targetIndex >= _spheres.Count || !fireIndexes.Contains(targetIndex)) continue;
                        var key = sourceIndex < targetIndex ? sourceIndex + ":" + targetIndex : targetIndex + ":" + sourceIndex;
                        if (!processedPairs.Add(key)) continue;
                        var source = _spheres[sourceIndex];
                        var target = _spheres[targetIndex];
                        if (source == null || target == null) continue;
                        SpawnCombatArc(source.transform.position, target.transform.position, source.Type,
                            branch, final, new Color(0.77f, 0.43f, 0.24f, 0.9f), 0.18f);
                        var boost = final == "firetrail_inferno" ? 0.32f : 0.20f;
                        source.AccelerateNextAttack(boost);
                        target.AccelerateNextAttack(boost);
                    }
                }
            }

            ApplyActiveAbilitySynergyRiders(RunUpgradeKind.FireTrail, level);
            ShowTimedCombatMessage("OVERHEAT", 0.9f);
        }

        private void ActivateMinion()
        {
            var level = GetAbilityLevel(RunUpgradeKind.Minion);
            if (level <= 0 || _minionCooldown > 0f) return;
            _minionCooldown = 30f * GetActiveAbilityCooldownMultiplier();
            var count = 1 + (level - 1) / 2;
            var branch = GetAbilityBranch(RunUpgradeKind.Minion);
            var final = GetAbilityFinalArchetype(RunUpgradeKind.Minion);
            var liveSpheres = new List<SphereAttackAgent>();
            foreach (var sphere in _spheres)
                if (sphere != null && sphere.gameObject.activeInHierarchy) liveSpheres.Add(sphere);

            SphereAttackAgent nearest = null;
            var nearestDistance = float.PositiveInfinity;
            foreach (var sphere in liveSpheres)
            {
                var distance = Vector2.Distance(_player.position, sphere.transform.position);
                if (distance < nearestDistance) { nearestDistance = distance; nearest = sphere; }
            }

            SphereAttackAgent pairA = null, pairB = null;
            var pairDistance = float.PositiveInfinity;
            var relayMode = branch == "minion_relay_drone" || final == "minion_network_nodes";
            if (relayMode && liveSpheres.Count >= 2)
                for (var i = 0; i < liveSpheres.Count - 1; i++)
                    for (var j = i + 1; j < liveSpheres.Count; j++)
                    {
                        var distance = Vector2.Distance(liveSpheres[i].transform.position, liveSpheres[j].transform.position);
                        if (distance < pairDistance) { pairDistance = distance; pairA = liveSpheres[i]; pairB = liveSpheres[j]; }
                    }

            var scale = SpherePlacementRules.SourcePixelsToWorld;
            var spawnedDrones = new List<AbilityDroneAgent2D>();
            for (var i = 0; i < count; i++)
            {
                var anchorPosition = nearest != null ? (Vector2)nearest.transform.position : (Vector2)_player.position;
                var angle = (i / (float)Mathf.Max(1, count)) * Mathf.PI * 2f;
                var spawnPosition = anchorPosition + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (42f * scale);
                if (relayMode && pairA != null && pairB != null && pairDistance <= 400f * scale)
                    spawnPosition = ((Vector2)pairA.transform.position + (Vector2)pairB.transform.position) * 0.5f;

                var drone = new GameObject("Echo Drone");
                drone.transform.position = spawnPosition;
                drone.transform.localScale = Vector3.one * 0.28f;
                var renderer = drone.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeSpriteFactory.Diamond;
                renderer.color = new Color(0.35f, 0.83f, 1f, 0.95f);
                renderer.sortingOrder = 16;
                var agent = drone.AddComponent<AbilityDroneAgent2D>();
                agent.Initialize(this, nearest != null ? nearest.Type : SphereId.Standard, level,
                    10f + (level >= 5 ? 2f : 0f), branch, final, i);
                _abilityDrones.Add(agent);
                spawnedDrones.Add(agent);

                if (nearest != null && branch == "minion_echo_drone") nearest.AccelerateNextAttack(0.45f);
                else if (nearest != null && branch == "minion_guardian") nearest.AccelerateNextAttack(0.35f);
                if (final == "minion_echo_swarm")
                    SpawnImpact(spawnPosition, new Color(0.83f, 0.58f, 0.24f, 0.95f));
                SpawnImpact(spawnPosition, new Color(0.35f, 0.83f, 1f, 0.9f));
            }

            // Source activateMinion rebuilds the network after adding drones, then
            // accelerates only Sphere nodes linked to each newly spawned drone.
            if (final == "minion_echo_swarm" && GetAbilityLevel(RunUpgradeKind.Minion) >= 3)
            {
                RefreshNetworkState();
                foreach (var drone in spawnedDrones)
                {
                    var droneIndex = _spheres.Count + _abilityDrones.IndexOf(drone);
                    foreach (var linkedIndex in SphereNetworkRules.GetLinkedNodeIndexes(_networkState, droneIndex))
                    {
                        if (linkedIndex < 0 || linkedIndex >= _spheres.Count) continue;
                        var linkedSphere = _spheres[linkedIndex];
                        if (linkedSphere == null || !linkedSphere.gameObject.activeInHierarchy || linkedSphere.NetworkDisabled) continue;
                        linkedSphere.AccelerateNextAttack(0.22f);
                    }
                }
            }

            // Source minion_sphere_guard accelerates every live Sphere near the player
            // when the ability is activated, not only the drone's current anchor.
            if (final == "minion_sphere_guard")
                foreach (var sphere in liveSpheres)
                    if (Vector2.Distance(sphere.transform.position, _player.position) < 300f * scale)
                        sphere.AccelerateNextAttack(0.35f);

            ApplyActiveAbilitySynergyRiders(RunUpgradeKind.Minion, level);
            ShowTimedCombatMessage("ECHO DRONE", 0.9f);
        }

        private void ActivateLightning()
        {
            var level = GetAbilityLevel(RunUpgradeKind.Lightning);
            if (level <= 0 || _lightningCooldown > 0f) return;
            _lightningCooldown = 20f * GetActiveAbilityCooldownMultiplier();
            var branch = GetAbilityBranch(RunUpgradeKind.Lightning);
            var final = GetAbilityFinalArchetype(RunUpgradeKind.Lightning);
            var scale = SpherePlacementRules.SourcePixelsToWorld;
            var ordered = new List<SphereAttackAgent>();
            foreach (var sphere in _spheres)
                if (sphere != null && sphere.gameObject.activeInHierarchy) ordered.Add(sphere);
            ordered.Sort((a, b) => Vector2.Distance(a.transform.position, _player.position).CompareTo(
                Vector2.Distance(b.transform.position, _player.position)));

            var targets = new List<EnemyAgent2D>();
            foreach (var enemy in _enemies)
                if (enemy != null && enemy.IsAlive) targets.Add(enemy);
            targets.Sort((a, b) => Vector2.Distance(a.transform.position, _player.position).CompareTo(
                Vector2.Distance(b.transform.position, _player.position)));
            if (targets.Count == 0) return;

            if ((branch == "lightning_relay" || final == "lightning_storm_network") &&
                ordered.Count > 0 && _networkState != null)
            {
                var remaining = new HashSet<SphereAttackAgent>(ordered);
                var networkOrder = new List<SphereAttackAgent>();
                var current = ordered[0];
                while (current != null)
                {
                    networkOrder.Add(current);
                    remaining.Remove(current);
                    var currentIndex = _spheres.IndexOf(current);
                    SphereAttackAgent next = null;
                    var nextDistance = float.PositiveInfinity;
                    if (currentIndex >= 0)
                        foreach (var linkedIndex in SphereNetworkRules.GetLinkedNodeIndexes(_networkState, currentIndex))
                        {
                            if (linkedIndex < 0 || linkedIndex >= _spheres.Count) continue;
                            var candidate = _spheres[linkedIndex];
                            if (candidate == null || !remaining.Contains(candidate)) continue;
                            var distance = Vector2.Distance(candidate.transform.position, current.transform.position);
                            if (distance < nextDistance) { nextDistance = distance; next = candidate; }
                        }
                    current = next;
                }
                // Preserve source ordering for disconnected Spheres. The source
                // appends leftovers from the original distance-sorted list, not Set order.
                var networkOrdered = new List<SphereAttackAgent>(networkOrder);
                foreach (var sphere in ordered)
                    if (remaining.Contains(sphere)) networkOrdered.Add(sphere);
                ordered = networkOrdered;
            }

            var previous = ordered.Count > 0 ? (Vector2)ordered[0].transform.position : (Vector2)_player.position;
            var jump = 0;
            for (var networkIndex = 0; networkIndex < ordered.Count; networkIndex++)
            {
                var sphere = ordered[networkIndex];
                var node = (Vector2)sphere.transform.position;
                if (networkIndex > 0)
                    SpawnCombatArc(previous, node, SphereId.Chain, branch, final,
                        new Color(0.63f, 0.42f, 1f, 0.95f), 0.24f);
                if (branch == "lightning_echo_storm")
                    DamageEnemiesNearFromSphere(sphere, node, 55f * scale, 10f + level * 3f, new Color(0.63f, 0.42f, 1f, 0.9f));
                previous = node;
                jump++;
            }

            var maxTargets = 1 + (level - 1) / 2;
            if (final == "lightning_thunder_chain") maxTargets += ordered.Count;
            var targetCount = Mathf.Min(maxTargets, targets.Count);
            for (var i = 0; i < targetCount; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive) continue;
                var targetPosition = (Vector2)target.transform.position;
                SpawnCombatArc(previous, targetPosition, SphereId.Chain, branch, final,
                    new Color(0.63f, 0.42f, 1f, 0.95f), 0.30f);
                var damage = (40f + level * 15f) * (1f + jump * 0.12f);
                target.ReceiveDamage(damage);
                previous = targetPosition;
                jump++;

                if (branch == "lightning_relay" && ordered.Count > 0 && _networkState != null)
                {
                    var sourceSphere = ordered[i % ordered.Count];
                    var sourceIndex = _spheres.IndexOf(sourceSphere);
                    SphereAttackAgent next = null;
                    var nextDistance = float.PositiveInfinity;
                    if (sourceIndex >= 0)
                        foreach (var linkedIndex in SphereNetworkRules.GetLinkedNodeIndexes(_networkState, sourceIndex))
                        {
                            if (linkedIndex < 0 || linkedIndex >= _spheres.Count) continue;
                            var candidate = _spheres[linkedIndex];
                            if (candidate == null || !candidate.gameObject.activeInHierarchy) continue;
                            var distance = Vector2.Distance(candidate.transform.position, sourceSphere.transform.position);
                            if (distance < nextDistance) { nextDistance = distance; next = candidate; }
                        }
                    if (next != null)
                        SpawnCombatArc(targetPosition, next.transform.position, next.Type, branch, final,
                            new Color(0.63f, 0.42f, 1f, 0.95f), 0.22f);
                }

                if (final == "lightning_thunder_chain")
                {
                    var nextTarget = targets[(i + 1) % targets.Count];
                    if (nextTarget != null && nextTarget != target)
                    {
                        SpawnCombatArc(targetPosition, nextTarget.transform.position, SphereId.Chain, branch, final,
                            new Color(0.63f, 0.42f, 1f, 0.95f), 0.22f);
                        nextTarget.ReceiveDamage(damage * 0.25f);
                    }
                }
            }

            if (branch == "lightning_overload" || final == "lightning_overload_core")
            {
                var lastIndex = Mathf.Min(maxTargets, targets.Count) - 1;
                if (lastIndex >= 0 && targets[lastIndex] != null && targets[lastIndex].IsAlive)
                    targets[lastIndex].ReceiveDamage(30f + level * 10f);
            }

            if (final == "lightning_storm_network" && ordered.Count > 0 && _networkState != null)
            {
                var chainIndexes = new HashSet<int>();
                foreach (var sphere in ordered) chainIndexes.Add(_spheres.IndexOf(sphere));
                var processedPairs = new HashSet<string>();
                var edgeCount = 0;
                foreach (var sourceIndex in chainIndexes)
                {
                    if (sourceIndex < 0) continue;
                    foreach (var targetIndex in SphereNetworkRules.GetLinkedNodeIndexes(_networkState, sourceIndex))
                    {
                        if (targetIndex < 0 || targetIndex >= _spheres.Count || !chainIndexes.Contains(targetIndex)) continue;
                        var key = sourceIndex < targetIndex ? sourceIndex + ":" + targetIndex : targetIndex + ":" + sourceIndex;
                        if (!processedPairs.Add(key)) continue;
                        edgeCount++;
                        var from = _spheres[sourceIndex].transform.position;
                        var to = _spheres[targetIndex].transform.position;
                        SpawnCombatArc(from, to, SphereId.Chain, branch, final,
                            new Color(0.63f, 0.42f, 1f, 0.95f), 0.18f);
                        DamageEnemiesNearFromSphere(_spheres[targetIndex], to, 85f * scale, (35f + level * 8f) * 0.35f, new Color(0.63f, 0.42f, 1f, 0.9f));
                    }
                }
                if (edgeCount == 0)
                    for (var i = ordered.Count - 1; i >= 0; i--)
                    {
                        var from = i > 0 ? (Vector2)ordered[i - 1].transform.position : (Vector2)_player.position;
                        var to = (Vector2)ordered[i].transform.position;
                        SpawnCombatArc(from, to, SphereId.Chain, branch, final,
                            new Color(0.63f, 0.42f, 1f, 0.95f), 0.18f);
                        DamageEnemiesNearFromSphere(ordered[i], to, 85f * scale, (35f + level * 8f) * 0.35f, new Color(0.63f, 0.42f, 1f, 0.9f));
                    }
            }

            ApplyActiveAbilitySynergyRiders(RunUpgradeKind.Lightning, level);
            ShowTimedCombatMessage("CHAIN LIGHTNING", 0.9f);
        }

        private void ActivateDarkRitual()
        {
            var level = GetAbilityLevel(RunUpgradeKind.DarkRitual);
            if (level <= 0 || _darkRitualCooldown > 0f || _playerCore == null) return;
            var cost = _playerCore.MaxHp * 0.20f;
            if (_playerCore.CurrentHp <= cost || !_playerCore.SpendHp(cost)) return;

            _darkRitualCooldown = 30f * GetActiveAbilityCooldownMultiplier();
            var branch = GetAbilityBranch(RunUpgradeKind.DarkRitual);
            var final = GetAbilityFinalArchetype(RunUpgradeKind.DarkRitual);
            _darkRitualOverloadTimer = 5f + Mathf.Min(3, level - 1);

            foreach (var sphere in _spheres)
            {
                if (sphere == null || !sphere.gameObject.activeInHierarchy) continue;
                sphere.AccelerateNextAttack(0.8f);
                SpawnImpact(sphere.transform.position, new Color(0.65f, 0.22f, 0.34f, 0.95f));
            }

            if (branch == "darkritual_blood_link")
            {
                SphereAttackAgent nearestStandard = null;
                var nearestDistance = float.PositiveInfinity;
                foreach (var sphere in _spheres)
                {
                    if (sphere == null || !sphere.gameObject.activeInHierarchy || sphere.Type != SphereId.Standard) continue;
                    var distance = Vector2.Distance(_player.position, sphere.transform.position);
                    if (distance < nearestDistance) { nearestDistance = distance; nearestStandard = sphere; }
                }
                nearestStandard?.AccelerateNextAttack(1.4f);
            }

            if (branch == "darkritual_void_pact" || final == "darkritual_void_engine")
                if (_playerCore.CurrentHp / Mathf.Max(1f, _playerCore.MaxHp) < 0.35f)
                    _darkRitualOverloadTimer += 2f;

            if (branch == "darkritual_sacrifice" || final == "darkritual_sacrifice_core")
            {
                SphereAttackAgent nearest = null;
                var nearestDistance = float.PositiveInfinity;
                foreach (var sphere in _spheres)
                {
                    if (sphere == null || !sphere.gameObject.activeInHierarchy) continue;
                    var distance = Vector2.Distance(_player.position, sphere.transform.position);
                    if (distance < nearestDistance) { nearestDistance = distance; nearest = sphere; }
                }
                var center = nearest != null ? (Vector2)nearest.transform.position : (Vector2)_player.position;
                var pulseDamage = (20f + level * 5f) * (final == "darkritual_sacrifice_core" ? 1.35f : 1f);
                var pulseRadius = (final == "darkritual_sacrifice_core" ? 145f : 130f) * SpherePlacementRules.SourcePixelsToWorld;
                EmitSourceSpherePulse(nearest, center, pulseRadius, pulseDamage, new Color(0.65f, 0.22f, 0.34f, 0.95f));
            }

            if (final == "darkritual_blood_network" && _networkState != null)
            {
                var standardIndexes = new List<int>();
                for (var i = 0; i < _spheres.Count; i++)
                    if (_spheres[i] != null && _spheres[i].gameObject.activeInHierarchy && _spheres[i].Type == SphereId.Standard)
                        standardIndexes.Add(i);
                foreach (var index in standardIndexes)
                {
                    var connectedToStandard = false;
                    foreach (var linkedIndex in SphereNetworkRules.GetLinkedNodeIndexes(_networkState, index))
                        if (standardIndexes.Contains(linkedIndex)) { connectedToStandard = true; break; }
                    if (!connectedToStandard) continue;
                    var sphere = _spheres[index];
                    sphere.AccelerateNextAttack(0.8f);
                    foreach (var linkedIndex in SphereNetworkRules.GetLinkedNodeIndexes(_networkState, index))
                    {
                        if (linkedIndex < 0 || linkedIndex >= _spheres.Count) continue;
                        var linked = _spheres[linkedIndex];
                        if (linked == null || !linked.gameObject.activeInHierarchy || linked.Type != SphereId.Standard) continue;
                        SpawnCombatArc(sphere.transform.position, linked.transform.position, SphereId.Standard,
                            GetSphereBranch(SphereId.Standard), GetSphereFinal(SphereId.Standard),
                            new Color(0.65f, 0.22f, 0.34f, 0.9f), 0.16f);
                    }
                }
            }

            if (final == "darkritual_void_engine" &&
                _playerCore.CurrentHp / Mathf.Max(1f, _playerCore.MaxHp) < 0.20f)
                foreach (var sphere in _spheres)
                    if (sphere != null && sphere.gameObject.activeInHierarchy)
                        EmitSourceSpherePulse(sphere, sphere.transform.position, 110f * SpherePlacementRules.SourcePixelsToWorld,
                            18f + level * 6f, new Color(0.65f, 0.22f, 0.34f, 0.95f));

            ApplyActiveAbilitySynergyRiders(RunUpgradeKind.DarkRitual, level);
            ShowTimedCombatMessage("OVERLOAD", 1.2f);
        }

        private void ActivateTimeStop()
        {
            var level = GetAbilityLevel(RunUpgradeKind.TimeStop);
            if (level <= 0 || _timeStopCooldown > 0f) return;
            _timeStopCooldown = 40f * GetActiveAbilityCooldownMultiplier();
            _timeStopTimer = 3f + Mathf.Min(2, level - 1);
            var branch = GetAbilityBranch(RunUpgradeKind.TimeStop);
            var final = GetAbilityFinalArchetype(RunUpgradeKind.TimeStop);
            SphereAttackAgent nearest = null;
            var origin = (Vector2)_player.position;
            var nearestDistance = float.PositiveInfinity;
            foreach (var sphere in _spheres)
            {
                if (sphere == null || !sphere.gameObject.activeInHierarchy) continue;
                var distance = Vector2.Distance(origin, sphere.transform.position);
                if (distance < nearestDistance) { nearest = sphere; nearestDistance = distance; }
            }
            var scale = SpherePlacementRules.SourcePixelsToWorld;
            var radius = nearest != null ? 520f * scale : float.PositiveInfinity;
            var freezeDuration = _timeStopTimer + (branch == "timestop_time_anchor" ? 1f : 0f);
            foreach (var enemy in _enemies)
                if (enemy != null && enemy.IsAlive &&
                    (nearest == null || Vector2.Distance(enemy.transform.position, nearest.transform.position) <= radius))
                    enemy.SetFreezeDuration(freezeDuration);

            if (branch == "timestop_echo_phase")
                foreach (var sphere in _spheres)
                    if (sphere != null && sphere.gameObject.activeInHierarchy) sphere.AccelerateNextAttack(999f);

            if ((branch == "timestop_closed_time" || final == "timestop_closed_network") &&
                nearest != null && _networkState != null)
            {
                var startIndex = _spheres.IndexOf(nearest);
                var queue = new Queue<int>();
                var visited = new HashSet<int>();
                if (startIndex >= 0) queue.Enqueue(startIndex);
                while (queue.Count > 0)
                {
                    var sphereIndex = queue.Dequeue();
                    if (!visited.Add(sphereIndex) || sphereIndex < 0 || sphereIndex >= _spheres.Count) continue;
                    var sphere = _spheres[sphereIndex];
                    if (sphere == null || !sphere.gameObject.activeInHierarchy) continue;
                    foreach (var enemy in _enemies)
                        if (enemy != null && enemy.IsAlive &&
                            Vector2.Distance(enemy.transform.position, sphere.transform.position) <= 260f * scale)
                            enemy.ApplyFreeze(_timeStopTimer);
                    foreach (var linkedIndex in SphereNetworkRules.GetLinkedNodeIndexes(_networkState, sphereIndex))
                        if (!visited.Contains(linkedIndex)) queue.Enqueue(linkedIndex);
                }
            }
            if (nearest != null)
            {
                var pulseRadius = 150f * scale;
                var pulseDamage = final == "timestop_temporal_core" ? 10f + level * 8f : 10f + level * 4f;
                EmitSourceSpherePulse(nearest, nearest.transform.position, pulseRadius, pulseDamage, new Color(0.63f, 0.42f, 1f, 0.9f));
            }
            ApplyTimeStopSynergyRiders(level);
            ShowTimedCombatMessage("ECHO FREEZE", 1.2f);
        }

        private void ActivateTeleport()
        {
            var level = GetAbilityLevel(RunUpgradeKind.Teleport);
            if (level <= 0 || _teleportCooldown > 0f) return;
            if (_follow) { ShowTimedCombatMessage("FOLLOW: TELEPORT LOCKED", 0.55f); return; }
            _teleportCooldown = Mathf.Max(5f,
                (15f - Mathf.Min(4, (level - 1) * 2)) * GetActiveAbilityCooldownMultiplier());
            var branch = GetAbilityBranch(RunUpgradeKind.Teleport);
            var final = GetAbilityFinalArchetype(RunUpgradeKind.Teleport);
            var origin = (Vector2)_player.position;
            SphereAttackAgent nearest = null;
            var nearestDistance = float.PositiveInfinity;
            foreach (var sphere in _spheres)
            {
                if (sphere == null || !sphere.gameObject.activeInHierarchy) continue;
                var distance = Vector2.Distance(origin, sphere.transform.position);
                if (level >= 3)
                {
                    var sourceDistance = distance / SpherePlacementRules.SourcePixelsToWorld;
                    if (sourceDistance < 220f || sourceDistance > 700f) continue;
                }
                if (distance < nearestDistance) { nearest = sphere; nearestDistance = distance; }
            }
            if (nearest == null && level >= 3)
                foreach (var sphere in _spheres)
                {
                    if (sphere == null || !sphere.gameObject.activeInHierarchy) continue;
                    var distance = Vector2.Distance(origin, sphere.transform.position);
                    if (distance < nearestDistance) { nearest = sphere; nearestDistance = distance; }
                }

            var scale = SpherePlacementRules.SourcePixelsToWorld;
            Vector2 destination;
            if (nearest != null) destination = nearest.transform.position;
            else
            {
                var offset = 300f * scale;
                destination = origin + new Vector2((_rng.NextFloat() * 2f - 1f) * offset,
                    (_rng.NextFloat() * 2f - 1f) * offset);
            }
            var worldLimit = 1200f * scale;
            destination.x = Mathf.Clamp(destination.x, -worldLimit, worldLimit);
            destination.y = Mathf.Clamp(destination.y, -worldLimit, worldLimit);
            _player.position = new Vector3(destination.x, destination.y, _player.position.z);
            for (var i = 0; i < 64; i++) _rng.NextFloat();

            if (branch == "teleport_phase" || final == "teleport_phase_break") _playerCore?.GrantInvulnerability(0.8f);
            if (branch == "teleport_beacon") _teleportDamageBuffTimer = 4f;
            if (branch == "teleport_phase") _playerCore?.GrantInvulnerability(1.25f);
            if (final == "teleport_hunter_beacon" && nearest != null && nearest.Type == SphereId.Sniper)
                _teleportDamageBuffTimer = 5f;

            if (nearest != null && final == "teleport_spatial_network")
            {
                SpawnCombatArc(origin, destination, nearest.Type, branch, final,
                    new Color(0.35f, 0.55f, 0.75f, 0.9f), 0.5f);
                foreach (var enemy in _enemies)
                    if (enemy != null && enemy.IsAlive &&
                        Vector2.Distance(enemy.transform.position, destination) <= 90f * scale)
                        enemy.ReceiveDamage(22f);
                if (_networkState != null)
                    foreach (var linkedIndex in SphereNetworkRules.GetLinkedNodeIndexes(_networkState, _spheres.IndexOf(nearest)))
                    {
                        if (linkedIndex < 0 || linkedIndex >= _spheres.Count) continue;
                        var linkedSphere = _spheres[linkedIndex];
                        if (linkedSphere == null || !linkedSphere.gameObject.activeInHierarchy) continue;
                        foreach (var enemy in _enemies)
                            if (enemy != null && enemy.IsAlive &&
                                Vector2.Distance(enemy.transform.position, linkedSphere.transform.position) <= 70f * scale)
                                enemy.ReceiveDamage(12f);
                    }
            }
            if (final == "teleport_phase_break")
            {
                var distancePixels = Vector2.Distance(origin, destination) / scale;
                if (distancePixels > 140f)
                {
                    var steps = Mathf.Max(1, Mathf.FloorToInt(distancePixels / 140f));
                    for (var step = 1; step < steps; step++)
                    {
                        var point = Vector2.Lerp(origin, destination, step / (float)steps);
                        foreach (var enemy in _enemies)
                            if (enemy != null && enemy.IsAlive &&
                                Vector2.Distance(enemy.transform.position, point) <= 70f * scale)
                                enemy.ReceiveDamage(14f);
                    }
                }
            }
            ApplyTeleportSynergyRiders(level);
            ShowTimedCombatMessage("ECHO JUMP", 0.9f);
        }

        private void RecordLevelUpChoice(RunUpgradeChoice choice)
        {
            var key = GetUpgradeChoiceKey(choice);
            _recentUpgradeChoiceKeys.Remove(key);
            _recentUpgradeChoiceKeys.Insert(0, key);
            while (_recentUpgradeChoiceKeys.Count > 2) _recentUpgradeChoiceKeys.RemoveAt(_recentUpgradeChoiceKeys.Count - 1);

            if (choice.Kind == RunUpgradeKind.Sphere)
            {
                _levelUpSpherePity = 0;
                _levelUpAbilityPity = Mathf.Min(4, _levelUpAbilityPity + 1);
            }
            else
            {
                _levelUpAbilityPity = 0;
                _levelUpSpherePity = Mathf.Min(4, _levelUpSpherePity + 1);
            }
        }

        private void ChooseUpgrade(int choiceIndex)
        {
            if (choiceIndex < 0 || choiceIndex >= _levelUpChoices.Count) return;
            var choice = _levelUpChoices[choiceIndex];
            var selectedChoiceKey = GetUpgradeChoiceKey(choice);
            if (_levelUpLockChoiceKey == selectedChoiceKey) _levelUpLockChoiceKey = null;
            RecordLevelUpChoice(choice);
            switch (choice.Kind)
            {
                case RunUpgradeKind.Sphere:
                    _pendingEvolutionIsAbility = false;
                    _sphereLevels[choice.Sphere] = choice.NextLevel;
                    // Level Up changes the source Sphere progression only. The
                    // physical Sphere entity is independently placed/removed
                    // through MobileControls in new-desing.
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.Type == choice.Sphere)
                            sphere.ApplyProgressionLevel(choice.NextLevel);

                    if (choice.NextLevel == 4 || choice.NextLevel == 7)
                    {
                        _pendingEvolutionSphere = choice.Sphere;
                        _pendingFinalEvolution = choice.NextLevel == 7;
                        _evolutionChoices.Clear();
                        var branchId = _sphereBranches.TryGetValue(choice.Sphere, out var selectedBranch) ? selectedBranch : null;
                        var options = _pendingFinalEvolution
                            ? SphereEvolutionCatalog.GetFinals(branchId)
                            : SphereEvolutionCatalog.GetBranches(choice.Sphere);
                        _evolutionChoices.AddRange(options);
                        _choosingEvolution = _evolutionChoices.Count > 0;
                        if (_choosingEvolution)
                        {

                            return;
                        }
                    }
                    break;
                case RunUpgradeKind.MaxSpheres:
                    _maxSphereAbilityLevel = choice.NextLevel;

                    break;
                case RunUpgradeKind.Teleport:
                case RunUpgradeKind.TimeStop:
                case RunUpgradeKind.Blast:
                case RunUpgradeKind.Shield:
                case RunUpgradeKind.FireTrail:
                case RunUpgradeKind.Minion:
                case RunUpgradeKind.Lightning:
                case RunUpgradeKind.DarkRitual:
                    _abilityLevels[choice.Kind] = choice.NextLevel;
                    if (choice.CurrentLevel == 0) AssignActiveAbilityHotkey(choice.Kind);

                    if (choice.NextLevel == 4 || choice.NextLevel == 7)
                    {
                        _pendingEvolutionAbility = choice.Kind;
                        _pendingEvolutionIsAbility = true;
                        _pendingEvolutionSphere = SphereId.Standard;
                        _pendingFinalEvolution = choice.NextLevel == 7;
                        _evolutionChoices.Clear();
                        _abilityBranches.TryGetValue(choice.Kind, out var activeBranchId);
                        _evolutionChoices.AddRange(LevelUpAbilityRules.GetActiveAbilityEvolutionChoices(
                            choice.Kind, _pendingFinalEvolution, activeBranchId));
                        _choosingEvolution = _evolutionChoices.Count > 0;
                        if (_choosingEvolution)
                        {

                            return;
                        }
                    }
                    break;
                case RunUpgradeKind.Damage:
                case RunUpgradeKind.AttackSpeed:
                case RunUpgradeKind.MoveSpeed:
                case RunUpgradeKind.Slow:
                case RunUpgradeKind.Radius:
                case RunUpgradeKind.Range:
                    _abilityLevels[choice.Kind] = choice.NextLevel;
                    for (var i = 0; i < _spheres.Count; i++)
                        if (_spheres[i] != null) _spheres[i].ApplyProgressionLevel(_spheres[i].ProgressionLevel);

                    break;
                case RunUpgradeKind.Dodge:
                case RunUpgradeKind.Crit:
                case RunUpgradeKind.Magnet:
                case RunUpgradeKind.Armor:
                case RunUpgradeKind.Regen:
                case RunUpgradeKind.Stability:
                case RunUpgradeKind.Vampire:
                case RunUpgradeKind.SphereBoost:
                case RunUpgradeKind.Cooldown:
                case RunUpgradeKind.Momentum:
                case RunUpgradeKind.Resonance:
                case RunUpgradeKind.CritPower:
                case RunUpgradeKind.ProjectileSpeed:
                case RunUpgradeKind.Fortune:
                    _abilityLevels[choice.Kind] = choice.NextLevel;

                    break;
                case RunUpgradeKind.Vitality:
                    _abilityLevels[choice.Kind] = choice.NextLevel;
                    _playerCore?.IncreaseMaxHp(20f);

                    break;
            }
            if (_choosingEvolution) return;
            _levelUp = false;
            _paused = false;
            _levelUpChoices.Clear();
            if (_tutorialStep == 4) SetTutorialStep(5);
            if (_follow) SetFormationFollowMode(true, true);
            OpenLevelUpIfReady();
        }

        private void ChooseEvolution(int index)
        {
            if (!_choosingEvolution || index < 0 || index >= _evolutionChoices.Count) return;
            var option = _evolutionChoices[index];
            if (_pendingEvolutionIsAbility)
            {
                if (_pendingFinalEvolution) _abilityFinals[_pendingEvolutionAbility] = option.Id;
                else _abilityBranches[_pendingEvolutionAbility] = option.Id;

            }
            else if (_pendingFinalEvolution)
            {
                _sphereFinals[_pendingEvolutionSphere] = option.Id;

            }
            else
            {
                _sphereBranches[_pendingEvolutionSphere] = option.Id;

            }
            var text=SourceEvolutionText(option);
            ShowTimedCombatMessage(text?.nameRu ?? option.Name, 2.2f, _pendingFinalEvolution ? "#c4453d" : "#d4943d");
            SourceAudio2D.Play("evolve");
            _runEvolutions++;
            _choosingEvolution = false;
            _pendingFinalEvolution = false;
            _pendingEvolutionIsAbility = false;
            _evolutionChoices.Clear();
            _levelUp = false;
            _paused = false;
            OpenLevelUpIfReady();
        }

        private void PruneDestroyed()
        {
            _simulationAgents.RemoveAll(agent=>agent==null);
            for (var i = _enemies.Count - 1; i >= 0; i--) if (_enemies[i] == null) _enemies.RemoveAt(i);
            for (var i = _spheres.Count - 1; i >= 0; i--) if (_spheres[i] == null) _spheres.RemoveAt(i);
        }

        private void ShowMessage(string text) { ShowTimedCombatMessage(text, 2.2f); }

        private void SetUserPaused(bool paused)
        {
            if (_gameOver || _levelUp || _choosingEvolution || _stellaRewardPending) return;
            _userPaused = paused;
            _paused = paused;
        }

        public SphereNetworkState NetworkState => _networkState;

        public bool AreNetworkSpheresLinked(int a, int b) => SphereNetworkRules.AreNodesLinked(_networkState, a, b);

        public List<SphereAttackAgent> GetLinkedSpheres(int sphereIndex)
        {
            var result = new List<SphereAttackAgent>();
            var linked = SphereNetworkRules.GetLinkedNodeIndexes(_networkState, sphereIndex);
            for (var i = 0; i < linked.Count; i++)
                if (linked[i] >= 0 && linked[i] < _spheres.Count && _spheres[linked[i]] != null)
                    result.Add(_spheres[linked[i]]);
            return result;
        }

        public int GetSphereIndex(SphereAttackAgent sphere) => sphere == null ? -1 : _spheres.IndexOf(sphere);

        public float GetNetworkDamageMultiplier(SphereAttackAgent sphere)
        {
            if (sphere == null) return 1f;
            var multiplier = 1f;
            multiplier *= GetArtifactSphereFamilyDamageMultiplier(sphere.Type);
            return multiplier * GetArtifactSphereSynergyDamageMultiplier(sphere);
        }

        private bool IsArtifactProtocolSetComplete(string setId)
        {
            switch (setId)
            {
                case "resonance_grid":
                    return ArtifactSetRules.HasCompletedResonanceGrid(
                        ArtifactSynergyRules.HasFortressNetworkSynergy(_ownedArtifacts),
                        ArtifactSynergyRules.HasPerfectNetworkSynergy(_ownedArtifacts));
                case "echo_architecture":
                    return FormationHitRules.HasCompletedEchoArchitecture(_ownedArtifacts);
                case "singularity_path":
                    return ArtifactSetRules.HasCompletedSingularityPath(
                        ArtifactSynergyRules.HasGlassCannonSynergy(_ownedArtifacts),
                        ArtifactSynergyRules.HasSingularitySynergy(_ownedArtifacts));
                case "geometry_craft":
                    return ArtifactSetRules.HasCompletedGeometryCraft(
                        ArtifactSynergyRules.HasGeometryLoopSynergy(_ownedArtifacts),
                        ArtifactSynergyRules.HasLatticeMemorySynergy(_ownedArtifacts),
                        ArtifactSynergyRules.HasNetworkMemorySynergy(_ownedArtifacts));
                case "void_horizon":
                    return ArtifactSetRules.HasCompletedVoidHorizonSet(
                        ArtifactSynergyRules.HasVoidHorizonSynergy(_ownedArtifacts),
                        ArtifactSynergyRules.HasOrbitalPrismSynergy(_ownedArtifacts),
                        ArtifactSynergyRules.HasGravityPulseSynergy(_ownedArtifacts));
                case "temporal_fold":
                    return ArtifactSynergyRules.HasTemporalEchoSynergy(_ownedArtifacts) &&
                        ArtifactSynergyRules.HasAxiomFoldSynergy(_ownedArtifacts);
                case "status_circuit":
                    return ArtifactSynergyRules.HasStatusCircuitSynergy(_ownedArtifacts) &&
                        ArtifactSynergyRules.HasStatusLoopSynergy(_ownedArtifacts);
                case "hunter_doctrine":
                    return ArtifactSetRules.HasCompletedHunterDoctrineSet(
                        ArtifactSynergyRules.HasHunterDoctrineSynergy(_ownedArtifacts),
                        ArtifactSynergyRules.HasHunterFocusSynergy(_ownedArtifacts));
                case "pulse_engineering":
                    return ArtifactSetRules.HasCompletedPulseEngineeringSet(
                        ArtifactSynergyRules.HasPulseEngineeringSynergy(_ownedArtifacts),
                        ArtifactSynergyRules.HasPulseOverloadSynergy(_ownedArtifacts));
                case "core_forge":
                    return ArtifactSetRules.HasCompletedCoreForgeSet(
                        ArtifactSynergyRules.HasCoreForgeSynergy(_ownedArtifacts),
                        ArtifactSynergyRules.HasCoreRelaySynergy(_ownedArtifacts));
                case "prism_dominion":
                    return ArtifactSetRules.HasCompletedPrismDominionSet(
                        ArtifactSynergyRules.HasOrbitalPrismSynergy(_ownedArtifacts),
                        ArtifactSynergyRules.HasStatusCircuitSynergy(_ownedArtifacts));
                case "network_legacy":
                    return ArtifactSetRules.HasCompletedNetworkLegacySet(
                        ArtifactSynergyRules.HasNetworkMemorySynergy(_ownedArtifacts),
                        ArtifactSynergyRules.HasPerfectNetworkSynergy(_ownedArtifacts));
                default:
                    return false;
            }
        }

        private bool IsArtifactProtocolCurrentlyActive(ArtifactProtocolDefinition protocol)
        {
            var discovered = IsArtifactProtocolSetComplete(protocol.SetId);
            if (!discovered || protocol.HasUnmappedRuntimeState) return false;

            bool conditionSatisfied;
            switch (protocol.Id)
            {
                case "triangle_resonance":
                    conditionSatisfied = false;
                    for (var i = 0; i < _spheres.Count; i++)
                        if (SphereFormsTriangleWithinSourceRange(_spheres[i])) { conditionSatisfied = true; break; }
                    break;
                case "sphere_relay":
                    conditionSatisfied = CountUniqueActiveSphereTypes() >= 2;
                    break;
                case "critical_echo":
                    conditionSatisfied = ArtifactProtocolRules.IsCriticalEchoActive(discovered, _combo);
                    break;
                case "temporal_fold_protocol":
                    conditionSatisfied = _timeStopTimer > 0f || _teleportDamageBuffTimer > 0f;
                    break;
                case "status_circuit_protocol":
                    conditionSatisfied = HasActiveSphereType(SphereId.Prism) && HasAnyGlobalSphereModifier();
                    break;
                case "geometry_circuit":
                    conditionSatisfied = ArtifactSetRules.IsRingLatticeOrFractal(_resonanceGeometryType);
                    break;
                case "void_horizon_protocol":
                    conditionSatisfied = HasActiveSphereType(SphereId.Void) && HasLivingEliteOrBossEnemy();
                    break;
                case "hunter_doctrine_protocol":
                    conditionSatisfied = HasActiveSphereType(SphereId.Sniper) && HasLivingEliteOrBossEnemy();
                    break;
                case "pulse_engineering_protocol":
                    conditionSatisfied = HasActiveSphereType(SphereId.Pulse) && CountLivingEnemiesInRun() >= 4;
                    break;
                case "core_forge_protocol":
                    conditionSatisfied = CountUniqueActiveSphereTypes() >= 3;
                    break;
                case "prism_dominion_protocol":
                    conditionSatisfied = HasActiveSphereType(SphereId.Prism) &&
                        (HasActiveSphereType(SphereId.Orbital) || HasActiveSphereType(SphereId.Void));
                    break;
                case "network_legacy_protocol":
                    conditionSatisfied = CountUniqueActiveSphereTypes() >= 3 &&
                        ArtifactSetRules.IsRingLatticeOrFractal(_resonanceGeometryType);
                    break;
                default:
                    conditionSatisfied = false;
                    break;
            }
            return ArtifactProtocolRules.IsProtocolActive(discovered, conditionSatisfied);
        }

        private void EnsureArtifactProtocolKnowledgeLoaded()
        {
            if (_artifactProtocolKnowledgeLoaded) return;
            _artifactProtocolKnowledgeLoaded = true;
            _knownArtifactProtocolIds.Clear();
            var raw = PlayerPrefs.GetString(ArtifactProtocolKnowledgePreferenceKey, string.Empty);
            var ids = raw.Split(new[] { '|' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < ids.Length; i++)
                if (ArtifactProtocolRules.TryGetProtocol(ids[i], out _)) _knownArtifactProtocolIds.Add(ids[i]);
        }

        private void RecordDiscoveredArtifactProtocols()
        {
            EnsureArtifactProtocolKnowledgeLoaded();
            var changed = false;
            for (var i = 0; i < ArtifactProtocolRules.Protocols.Count; i++)
            {
                var protocol = ArtifactProtocolRules.Protocols[i];
                if (IsArtifactProtocolSetComplete(protocol.SetId) && _knownArtifactProtocolIds.Add(protocol.Id))
                    changed = true;
            }
            if (!changed) return;
            var savedIds = new List<string>();
            for (var i = 0; i < ArtifactProtocolRules.Protocols.Count; i++)
            {
                var id = ArtifactProtocolRules.Protocols[i].Id;
                if (_knownArtifactProtocolIds.Contains(id)) savedIds.Add(id);
            }
            PlayerPrefs.SetString(ArtifactProtocolKnowledgePreferenceKey, System.String.Join("|", savedIds.ToArray()));
            PlayerPrefs.Save();
        }

        private float GetArtifactSphereSynergyDamageMultiplier(SphereAttackAgent sphere)
        {
            var uniqueTypes = CountUniqueActiveSphereTypes();
            var timeControlActive = _timeStopTimer > 0f || _teleportDamageBuffTimer > 0f;
            var hasAnySphereModifier = HasAnyGlobalSphereModifier();
            var multiplier = FormationHitRules.GetEchoRelayDamageMultiplier(
                FormationHitRules.HasEchoRelaySynergy(_ownedArtifacts), uniqueTypes);
            multiplier *= FormationHitRules.GetEchoArchitectureDamageMultiplier(
                FormationHitRules.HasCompletedEchoArchitecture(_ownedArtifacts), uniqueTypes,
                HasNearbyActiveSphere(sphere, 220f));
            multiplier *= FormationHitRules.GetUnifiedCoreDamageMultiplier(
                FormationHitRules.HasUnifiedCoreSynergy(_ownedArtifacts),
                SphereProgressionRules.GetLevel(_sphereLevels, sphere.Type),
                GetStrongestSphereLevel());
            multiplier *= ArtifactSynergyRules.GetFortressNetworkDamageMultiplier(
                ArtifactSynergyRules.HasFortressNetworkSynergy(_ownedArtifacts),
                HasNearbyActiveSphere(sphere, 190f));
            multiplier *= ArtifactSynergyRules.GetGlassCannonDamageMultiplier(
                ArtifactSynergyRules.HasGlassCannonSynergy(_ownedArtifacts));
            multiplier *= ArtifactSynergyRules.GetSingularityDamageMultiplier(
                ArtifactSynergyRules.HasSingularitySynergy(_ownedArtifacts), uniqueTypes);
            multiplier *= ArtifactSynergyRules.GetCoreRelayDamageMultiplier(
                ArtifactSynergyRules.HasCoreRelaySynergy(_ownedArtifacts), uniqueTypes);
            multiplier *= ArtifactSynergyRules.GetTemporalEchoDamageMultiplier(
                ArtifactSynergyRules.HasTemporalEchoSynergy(_ownedArtifacts), timeControlActive);
            multiplier *= ArtifactSynergyRules.GetStatusCircuitDamageMultiplier(
                ArtifactSynergyRules.HasStatusCircuitSynergy(_ownedArtifacts),
                sphere.Type == SphereId.Prism, hasAnySphereModifier);
            multiplier *= ArtifactSynergyRules.GetStatusLoopDamageMultiplier(
                ArtifactSynergyRules.HasStatusLoopSynergy(_ownedArtifacts),
                sphere.Type == SphereId.Prism, hasAnySphereModifier);

            var hasVoidHorizon = ArtifactSynergyRules.HasVoidHorizonSynergy(_ownedArtifacts) &&
                sphere.Type == SphereId.Void;
            multiplier *= ArtifactSynergyRules.GetVoidHorizonDamageMultiplier(
                hasVoidHorizon, sphere.Type == SphereId.Void, hasVoidHorizon && HasLivingEliteEnemy());

            multiplier *= ArtifactSynergyRules.GetOrbitalPrismDamageMultiplier(
                ArtifactSynergyRules.HasOrbitalPrismSynergy(_ownedArtifacts), sphere.Type == SphereId.Orbital);

            var hasGravityPulse = ArtifactSynergyRules.HasGravityPulseSynergy(_ownedArtifacts) &&
                sphere.Type == SphereId.Pulse;
            multiplier *= ArtifactSynergyRules.GetGravityPulseDamageMultiplier(
                hasGravityPulse, sphere.Type == SphereId.Pulse,
                hasGravityPulse ? CountLivingEnemiesNearAnySphere(140f) : 0);

            var hasPerfectNetwork = ArtifactSynergyRules.HasPerfectNetworkSynergy(_ownedArtifacts);
            var sphereFormsTriangle = hasPerfectNetwork && uniqueTypes >= 3 &&
                SphereFormsTriangleWithinSourceRange(sphere);
            multiplier *= ArtifactSynergyRules.GetPerfectNetworkDamageMultiplier(
                hasPerfectNetwork, uniqueTypes, sphereFormsTriangle);

            var hasHunterDoctrine = ArtifactSynergyRules.HasHunterDoctrineSynergy(_ownedArtifacts);
            multiplier *= ArtifactSynergyRules.GetHunterDoctrineDamageMultiplier(
                hasHunterDoctrine, sphere.Type == SphereId.Sniper,
                hasHunterDoctrine && sphere.Type == SphereId.Sniper && HasLivingEliteOrBossEnemy());

            var hasPulseEngineering = ArtifactSynergyRules.HasPulseEngineeringSynergy(_ownedArtifacts);
            multiplier *= ArtifactSynergyRules.GetPulseEngineeringDamageMultiplier(
                hasPulseEngineering, sphere.Type == SphereId.Pulse,
                hasPulseEngineering && sphere.Type == SphereId.Pulse ? CountLivingEnemiesInRun() : 0);

            multiplier *= ArtifactSynergyRules.GetCoreForgeDamageMultiplier(
                ArtifactSynergyRules.HasCoreForgeSynergy(_ownedArtifacts), uniqueTypes);

            multiplier *= ArtifactSynergyRules.GetHunterFocusDamageMultiplier(
                ArtifactSynergyRules.HasHunterFocusSynergy(_ownedArtifacts),
                sphere.Type == SphereId.Sniper, _ownedArtifacts.Contains("long_lens"));

            multiplier *= ArtifactSynergyRules.GetPulseOverloadDamageMultiplier(
                ArtifactSynergyRules.HasPulseOverloadSynergy(_ownedArtifacts),
                sphere.Type == SphereId.Pulse, _ownedArtifacts.Contains("pulse_driver"));

            multiplier *= ArtifactSynergyRules.GetAxiomFoldDamageMultiplier(
                ArtifactSynergyRules.HasAxiomFoldSynergy(_ownedArtifacts), uniqueTypes);
            multiplier *= ArtifactSynergyRules.GetGeometryLoopDamageMultiplier(
                ArtifactSynergyRules.HasGeometryLoopSynergy(_ownedArtifacts), _resonanceGeometryType);
            multiplier *= ArtifactSynergyRules.GetLatticeMemoryDamageMultiplier(
                ArtifactSynergyRules.HasLatticeMemorySynergy(_ownedArtifacts),
                _resonanceGeometryMemoryTimer > 0f, _resonanceGeometryMemoryType);
            multiplier *= ArtifactSynergyRules.GetNetworkMemoryDamageMultiplier(
                ArtifactSynergyRules.HasNetworkMemorySynergy(_ownedArtifacts),
                _resonanceGeometryMemoryTimer > 0f);

            var hasResonanceGrid = ArtifactSetRules.HasCompletedResonanceGrid(
                ArtifactSynergyRules.HasFortressNetworkSynergy(_ownedArtifacts),
                ArtifactSynergyRules.HasPerfectNetworkSynergy(_ownedArtifacts));
            multiplier *= ArtifactSetRules.GetResonanceGridDamageMultiplier(
                hasResonanceGrid, HasNearbyActiveSphere(sphere, 190f),
                !string.Equals(_resonanceGeometryKey, "none", StringComparison.Ordinal));

            var hasSingularityPath = ArtifactSetRules.HasCompletedSingularityPath(
                ArtifactSynergyRules.HasGlassCannonSynergy(_ownedArtifacts),
                ArtifactSynergyRules.HasSingularitySynergy(_ownedArtifacts));
            multiplier *= ArtifactSetRules.GetSingularityPathDamageMultiplier(hasSingularityPath, uniqueTypes);

            var hasGeometryCraft = ArtifactSetRules.HasCompletedGeometryCraft(
                ArtifactSynergyRules.HasGeometryLoopSynergy(_ownedArtifacts),
                ArtifactSynergyRules.HasLatticeMemorySynergy(_ownedArtifacts),
                ArtifactSynergyRules.HasNetworkMemorySynergy(_ownedArtifacts));
            multiplier *= ArtifactSetRules.GetGeometryCraftDamageMultiplier(
                hasGeometryCraft, _resonanceGeometryType);

            var hasTemporalFoldSet = ArtifactSetRules.HasCompletedTemporalFold(
                ArtifactSynergyRules.HasTemporalEchoSynergy(_ownedArtifacts),
                ArtifactSynergyRules.HasAxiomFoldSynergy(_ownedArtifacts));
            multiplier *= ArtifactSetRules.GetTemporalFoldDamageMultiplier(hasTemporalFoldSet, timeControlActive);

            var hasVoidHorizonSet = ArtifactSetRules.HasCompletedVoidHorizonSet(
                ArtifactSynergyRules.HasVoidHorizonSynergy(_ownedArtifacts),
                ArtifactSynergyRules.HasOrbitalPrismSynergy(_ownedArtifacts),
                ArtifactSynergyRules.HasGravityPulseSynergy(_ownedArtifacts));
            multiplier *= ArtifactSetRules.GetVoidHorizonSetDamageMultiplier(
                hasVoidHorizonSet, sphere.Type == SphereId.Void);

            var hasHunterDoctrineSet = ArtifactSetRules.HasCompletedHunterDoctrineSet(
                ArtifactSynergyRules.HasHunterDoctrineSynergy(_ownedArtifacts),
                ArtifactSynergyRules.HasHunterFocusSynergy(_ownedArtifacts));
            multiplier *= ArtifactSetRules.GetHunterDoctrineSetDamageMultiplier(
                hasHunterDoctrineSet, sphere.Type == SphereId.Sniper);

            var hasPulseEngineeringSet = ArtifactSetRules.HasCompletedPulseEngineeringSet(
                ArtifactSynergyRules.HasPulseEngineeringSynergy(_ownedArtifacts),
                ArtifactSynergyRules.HasPulseOverloadSynergy(_ownedArtifacts));
            multiplier *= ArtifactSetRules.GetPulseEngineeringSetDamageMultiplier(
                hasPulseEngineeringSet, sphere.Type == SphereId.Pulse);

            var hasCoreForgeSet = ArtifactSetRules.HasCompletedCoreForgeSet(
                ArtifactSynergyRules.HasCoreForgeSynergy(_ownedArtifacts),
                ArtifactSynergyRules.HasCoreRelaySynergy(_ownedArtifacts));
            multiplier *= ArtifactSetRules.GetCoreForgeSetDamageMultiplier(hasCoreForgeSet, uniqueTypes);

            var hasStatusCircuitSet = ArtifactSetRules.HasCompletedStatusCircuit(
                ArtifactSynergyRules.HasStatusCircuitSynergy(_ownedArtifacts),
                ArtifactSynergyRules.HasStatusLoopSynergy(_ownedArtifacts));
            multiplier *= ArtifactSetRules.GetStatusCircuitSetDamageMultiplier(
                hasStatusCircuitSet, sphere.Type == SphereId.Prism, hasAnySphereModifier);

            var hasPrismDominionSet = ArtifactSetRules.HasCompletedPrismDominionSet(
                ArtifactSynergyRules.HasOrbitalPrismSynergy(_ownedArtifacts),
                ArtifactSynergyRules.HasStatusCircuitSynergy(_ownedArtifacts));
            multiplier *= ArtifactSetRules.GetPrismDominionSetDamageMultiplier(
                hasPrismDominionSet, sphere.Type == SphereId.Prism || sphere.Type == SphereId.Orbital);

            var hasNetworkLegacySet = ArtifactSetRules.HasCompletedNetworkLegacySet(
                ArtifactSynergyRules.HasNetworkMemorySynergy(_ownedArtifacts),
                ArtifactSynergyRules.HasPerfectNetworkSynergy(_ownedArtifacts));
            multiplier *= ArtifactSetRules.GetNetworkLegacySetDamageMultiplier(
                hasNetworkLegacySet, uniqueTypes, _resonanceGeometryType);

            // Source Artifact Protocol damage gates, kept separate from their
            // completed-set bonus so their multiplicative stacking is preserved.
            multiplier *= ArtifactProtocolRules.GetTriangleResonanceMultiplier(
                hasResonanceGrid, SphereFormsTriangleWithinSourceRange(sphere));
            multiplier *= ArtifactProtocolRules.GetSphereRelayMultiplier(
                FormationHitRules.HasCompletedEchoArchitecture(_ownedArtifacts), uniqueTypes,
                HasNearbyActiveSphere(sphere, 220f));
            multiplier *= ArtifactProtocolRules.GetTemporalFoldProtocolMultiplier(
                hasTemporalFoldSet, timeControlActive);
            multiplier *= ArtifactProtocolRules.GetStatusCircuitProtocolMultiplier(
                hasStatusCircuitSet, sphere.Type == SphereId.Prism, hasAnySphereModifier);
            multiplier *= ArtifactProtocolRules.GetCriticalEchoMultiplier(hasSingularityPath, _combo);
            multiplier *= ArtifactProtocolRules.GetGeometryCircuitMultiplier(
                hasGeometryCraft, _resonanceGeometryType);
            multiplier *= ArtifactProtocolRules.GetVoidHorizonProtocolMultiplier(
                hasVoidHorizonSet, sphere.Type == SphereId.Void, HasLivingEliteOrBossEnemy());
            multiplier *= ArtifactProtocolRules.GetHunterDoctrineProtocolMultiplier(
                hasHunterDoctrineSet, sphere.Type == SphereId.Sniper, HasLivingEliteOrBossEnemy());
            multiplier *= ArtifactProtocolRules.GetPulseEngineeringProtocolMultiplier(
                hasPulseEngineeringSet, sphere.Type == SphereId.Pulse, CountLivingEnemiesInRun());
            multiplier *= ArtifactProtocolRules.GetCoreForgeProtocolMultiplier(
                hasCoreForgeSet, uniqueTypes);
            var refractionPressure = HasActiveSphereType(SphereId.Prism) &&
                (HasActiveSphereType(SphereId.Orbital) || HasActiveSphereType(SphereId.Void));
            multiplier *= ArtifactProtocolRules.GetPrismDominionProtocolMultiplier(
                hasPrismDominionSet, sphere.Type == SphereId.Prism ||
                    sphere.Type == SphereId.Orbital || sphere.Type == SphereId.Void,
                refractionPressure);
            multiplier *= ArtifactProtocolRules.GetNetworkLegacyProtocolMultiplier(
                hasNetworkLegacySet, uniqueTypes, _resonanceGeometryType);
            return multiplier;
        }

        // Source artifactSystem.ts:formsTriangle checks any alive triplet
        // containing the attacking Sphere. It uses a 300 logical-pixel maximum
        // for every side, not Unity's stricter active Triangle geometry profile.
        private bool SphereFormsTriangleWithinSourceRange(SphereAttackAgent sphere)
        {
            if (sphere == null || !sphere.gameObject.activeInHierarchy) return false;
            const float maxSidePixels = 300f;
            var sourcePosition = (Vector2)sphere.transform.position;
            var sourceDistanceScale = SpherePlacementRules.SourcePixelsToWorld;

            for (var i = 0; i < _spheres.Count; i++)
            {
                var a = _spheres[i];
                if (a == null || a == sphere || !a.gameObject.activeInHierarchy) continue;
                var asDistance = Vector2.Distance(sourcePosition, a.transform.position) / sourceDistanceScale;
                if (asDistance > maxSidePixels) continue;

                for (var j = i + 1; j < _spheres.Count; j++)
                {
                    var other = _spheres[j];
                    if (other == null || other == sphere || other == a ||
                        !other.gameObject.activeInHierarchy) continue;
                    var bsDistance = Vector2.Distance(sourcePosition, other.transform.position) / sourceDistanceScale;
                    if (bsDistance > maxSidePixels) continue;
                    var abDistance = Vector2.Distance(a.transform.position, other.transform.position) / sourceDistanceScale;
                    if (ArtifactSynergyRules.IsTriangleWithinSourceRange(abDistance, asDistance, bsDistance))
                        return true;
                }
            }
            return false;
        }

        private bool HasActiveSphereType(SphereId type)
        {
            for (var i = 0; i < _spheres.Count; i++)
            {
                var sphere = _spheres[i];
                if (sphere != null && sphere.gameObject.activeInHierarchy && sphere.Type == type)
                    return true;
            }
            return false;
        }

        private bool HasLivingEliteOrBossEnemy()
        {
            for (var i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (enemy != null && enemy.IsAlive && (enemy.IsElite || enemy.IsBoss)) return true;
            }
            return false;
        }

        private int CountLivingEnemiesInRun()
        {
            var count = 0;
            for (var i = 0; i < _enemies.Count; i++)
                if (_enemies[i] != null && _enemies[i].IsAlive) count++;
            return count;
        }

        private bool HasLivingEliteEnemy()
        {
            for (var i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (enemy != null && enemy.IsAlive && enemy.IsElite) return true;
            }
            return false;
        }

        // Source gravity_pulse groups each living enemy once when any living
        // Sphere is strictly closer than 140 source pixels.
        private int CountLivingEnemiesNearAnySphere(float sourceDistancePixels)
        {
            var radius = sourceDistancePixels * SpherePlacementRules.SourcePixelsToWorld;
            var radiusSq = radius * radius;
            var count = 0;
            for (var enemyIndex = 0; enemyIndex < _enemies.Count; enemyIndex++)
            {
                var enemy = _enemies[enemyIndex];
                if (enemy == null || !enemy.IsAlive) continue;
                var enemyPosition = (Vector2)enemy.transform.position;
                for (var sphereIndex = 0; sphereIndex < _spheres.Count; sphereIndex++)
                {
                    var sphere = _spheres[sphereIndex];
                    if (sphere == null || !sphere.gameObject.activeInHierarchy) continue;
                    var delta = enemyPosition - (Vector2)sphere.transform.position;
                    if (delta.sqrMagnitude < radiusSq)
                    {
                        count++;
                        break;
                    }
                }
            }
            return count;
        }

        private bool HasNearbyActiveSphere(SphereAttackAgent sphere, float sourceDistancePixels)
        {
            if (sphere == null || !sphere.gameObject.activeInHierarchy) return false;
            var radius = sourceDistancePixels * SpherePlacementRules.SourcePixelsToWorld;
            var radiusSq = radius * radius;
            var position = (Vector2)sphere.transform.position;
            for (var i = 0; i < _spheres.Count; i++)
            {
                var other = _spheres[i];
                if (other == null || other == sphere || !other.gameObject.activeInHierarchy) continue;
                var delta = (Vector2)other.transform.position - position;
                if (delta.sqrMagnitude <= radiusSq) return true;
            }
            return false;
        }

        public bool HasNetworkCandidateForSphere(SphereNetworkFormation type, int sphereIndex)
        {
            if (_networkState == null) return false;
            for (var i = 0; i < _networkState.FormationCandidates.Count; i++)
                if (_networkState.FormationCandidates[i].Type == type && _networkState.FormationCandidates[i].Contains(sphereIndex))
                    return true;
            return false;
        }

        public float GetFormationBonus(SphereNetworkFormation type, int sphereIndex = -1) =>
            SphereNetworkRules.GetFormationBonusMultiplier(_networkState, type, sphereIndex);

        private void TrackResonanceGeometryState()
        {
            var formation = _networkState != null ? _networkState.DominantFormation : null;
            if (formation != null && !formation.Active) formation = null;
            var currentKey = GetFormationKey(formation);
            var currentCandidateKeys = new List<string>();
            if (_networkState != null)
            {
                for (var i = 0; i < _networkState.FormationCandidates.Count; i++)
                    currentCandidateKeys.Add(GetFormationKey(_networkState.FormationCandidates[i]));
            }

            if (!ResonanceGeometryMemoryRules.ShouldRefreshGeometry(
                    _resonanceGeometryKey, currentKey,
                    _resonanceGeometryCandidateKeys, currentCandidateKeys))
                return;

            if (ResonanceGeometryMemoryRules.ShouldCapturePreviousGeometry(
                    _resonanceGeometryKey, _resonanceGeometryNodeIndexes.Count))
            {
                var previousPoints = new List<Vector2>();
                for (var i = 0; i < _resonanceGeometryNodeIndexes.Count; i++)
                {
                    var index = _resonanceGeometryNodeIndexes[i];
                    // Source maps stored old node indexes to current positions if
                    // the Sphere entry still exists, without requiring it alive.
                    if (index >= 0 && index < _spheres.Count && _spheres[index] != null)
                        previousPoints.Add(_spheres[index].transform.position);
                }

                // Source only replaces formationMemory if at least two previous
                // points resolve. Otherwise an already-running memory remains
                // intact until its own 0.9 s expiry.
                if (ResonanceGeometryMemoryRules.ShouldStoreMemory(previousPoints.Count))
                {
                    _resonanceGeometryMemoryPoints.Clear();
                    _resonanceGeometryMemoryPoints.AddRange(previousPoints);
                    _resonanceGeometryMemoryType = _resonanceGeometryType;
                    _resonanceGeometryMemoryTimer = ResonanceGeometryMemoryRules.MemoryDurationSeconds;
                }
            }

            _resonanceGeometryKey = currentKey;
            _resonanceGeometryType = formation != null ? formation.Type : SphereNetworkFormation.None;
            _resonanceGeometryNodeIndexes.Clear();
            if (formation != null) _resonanceGeometryNodeIndexes.AddRange(formation.NodeIndexes);
            _resonanceGeometryCandidateKeys.Clear();
            _resonanceGeometryCandidateKeys.AddRange(currentCandidateKeys);
        }

        private EchoSphere.Presentation.NativeNetworkAnalysisCache _nativeNetworkAnalysis;
        private void RefreshNetworkState()
        {
            _abilityDrones.RemoveAll(drone => drone == null || !drone.IsNetworkNodeAlive);
            var includeDrones = GetAbilityLevel(RunUpgradeKind.Minion) >= 3;
            if(_nativeNetworkAnalysis==null)_nativeNetworkAnalysis=new EchoSphere.Presentation.NativeNetworkAnalysisCache();
            var nodes = _nativeNetworkAnalysis.Nodes;nodes.Clear();
            for (var i = 0; i < _spheres.Count; i++)
            {
                var sphere = _spheres[i];
                if (sphere == null || !sphere.gameObject.activeInHierarchy)
                {
                    nodes.Add(new SphereNetworkNode(Vec2.Zero, false));
                    continue;
                }
                var position = (Vector2)sphere.transform.position;
                nodes.Add(new SphereNetworkNode(new Vec2(position.x, position.y), !sphere.NetworkDisabled));
            }
            // new-desing appends temporary Minions to the runtime network from
            // Minion Ability level III onward. Keep Sphere indexes first, then
            // drone indexes in their live-list order.
            if (includeDrones)
                foreach (var drone in _abilityDrones)
                {
                    var position = drone.NetworkPosition;
                    nodes.Add(new SphereNetworkNode(new Vec2(position.x, position.y), drone.IsNetworkNodeAlive));
                }

            var previous = _preferredDominantFormation != SphereNetworkFormation.None ? _preferredDominantFormation : _previousDominantFormation;
            _networkState = _nativeNetworkAnalysis.Analyze(_networkState,previous,_preferredDominantFormation,_preferredSecondaryFormation);
            if (_preferredDominantFormation != SphereNetworkFormation.None &&
                !SphereNetworkRules.TrySetDominantFormation(_networkState, _preferredDominantFormation))
                _preferredDominantFormation = SphereNetworkFormation.None;
            if (_preferredSecondaryFormation != SphereNetworkFormation.None &&
                !SphereNetworkRules.TrySetSecondaryFormation(_networkState, _preferredSecondaryFormation))
                _preferredSecondaryFormation = SphereNetworkFormation.None;

            var dominantForFractal = _networkState.DominantFormation;
            if (_sphereCharacterId == SphereCharacterId.Fractal && dominantForFractal != null &&
                dominantForFractal.Active &&
                !_resonanceGeometryCandidateKeys.Contains(GetFormationKey(dominantForFractal)))
                RecordFractalFormation(dominantForFractal);

            TrackResonanceGeometryState();

            if (_networkState.DominantFormation != null)
                _previousDominantFormation = _networkState.DominantFormation.Type;
            TrackArchitectFormationChange();
            UpdateNetworkLinkVisuals();
            var dominant = _networkState.DominantFormation;
            if (dominant != null && dominant.Active && _resonanceKnownFormationKeys.Add(GetFormationKey(dominant)))
                AddResonanceChargeFromSource(ResonanceRules.GeometryCharge, true);
        }

        private void CycleDominantFormation()
        {
            if (_networkState == null || _networkState.FormationCandidates.Count == 0) return;
            var current = _preferredDominantFormation != SphereNetworkFormation.None
                ? _preferredDominantFormation : _networkState.DominantFormation?.Type ?? SphereNetworkFormation.None;
            var currentIndex = _networkState.FormationCandidates.FindIndex(x => x.Type == current);
            if (currentIndex >= _networkState.FormationCandidates.Count - 1)
            {
                _preferredDominantFormation = SphereNetworkFormation.None;
                RefreshNetworkState();
                ShowMessage("GEOMETRY AUTO");
                return;
            }
            _preferredDominantFormation = _networkState.FormationCandidates[currentIndex + 1].Type;
            RefreshNetworkState();
            ShowMessage("MAIN GEOMETRY: " + _preferredDominantFormation.ToString().ToUpperInvariant());
        }

        private void CycleSecondaryFormation()
        {
            if (_networkState == null || _networkState.FormationCandidates.Count < 2) return;
            var current = _preferredSecondaryFormation != SphereNetworkFormation.None
                ? _preferredSecondaryFormation : _networkState.SecondaryFormation?.Type ?? SphereNetworkFormation.None;
            var currentIndex = _networkState.FormationCandidates.FindIndex(x => x.Type == current);
            var next = SphereNetworkFormation.None;
            for (var offset = 1; offset <= _networkState.FormationCandidates.Count; offset++)
            {
                var index = (Mathf.Max(-1, currentIndex) + offset) % _networkState.FormationCandidates.Count;
                var candidate = _networkState.FormationCandidates[index];
                if (_networkState.DominantFormation != null && candidate.Type == _networkState.DominantFormation.Type) continue;
                next = candidate.Type;
                break;
            }
            if (next == SphereNetworkFormation.None || next == current)
            {
                _preferredSecondaryFormation = SphereNetworkFormation.None;
                RefreshNetworkState();
                ShowMessage("SECONDARY GEOMETRY AUTO");
                return;
            }
            _preferredSecondaryFormation = next;
            RefreshNetworkState();
            ShowMessage("SECONDARY: " + next.ToString().ToUpperInvariant());
        }

        private void UpdateNetworkLinkVisuals()
        {
            if (UsesSourcePresentation) return;
            if (_networkState == null) return;
            if (_networkLineMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null) _networkLineMaterial = new Material(shader);
            }

            EnsureNetworkCometVisuals(_networkState.Links.Count * 3);
            for (var cometIndex = 0; cometIndex < _networkComets.Count; cometIndex++)
            {
                var comet = _networkComets[cometIndex];
                if (comet.Head != null) comet.Head.enabled = false;
                if (comet.Tail != null) comet.Tail.enabled = false;
            }

            while (_networkLinkRenderers.Count < _networkState.Links.Count)
            {
                _networkGlowRenderers.Add(CreateNetworkLineRenderer("Sphere Network Glow", 4));
                _networkLinkRenderers.Add(CreateNetworkLineRenderer("Sphere Network Link", 5));
                _networkHighlightRenderers.Add(CreateNetworkLineRenderer("Sphere Network Highlight", 6));
            }

            for (var i = 0; i < _networkLinkRenderers.Count; i++)
            {
                var line = _networkLinkRenderers[i];
                var glowLine = _networkGlowRenderers[i];
                var highlightLine = _networkHighlightRenderers[i];
                if (i >= _networkState.Links.Count)
                {
                    line.enabled = glowLine.enabled = highlightLine.enabled = false;
                    continue;
                }

                var link = _networkState.Links[i];
                if (!TryGetNetworkNodePosition(link.A, out var from) ||
                    !TryGetNetworkNodePosition(link.B, out var to))
                {
                    line.enabled = glowLine.enabled = highlightLine.enabled = false;
                    continue;
                }

                line.enabled = glowLine.enabled = highlightLine.enabled = true;
                const int segments = 24;
                line.positionCount = glowLine.positionCount = highlightLine.positionCount = segments + 1;
                var delta = to - from;
                var length = Mathf.Max(0.0001f, delta.magnitude);
                var normal = new Vector2(-delta.y / length, delta.x / length);
                var pixelScale = SpherePlacementRules.SourcePixelsToWorld;
                var amplitude = Mathf.Min(6.2f * pixelScale, 2.2f * pixelScale + length * 0.008f);
                var lineShape = _networkState.GetFormation(SphereNetworkFormation.Line);
                var squareShape = _networkState.GetFormation(SphereNetworkFormation.Square);
                var triangleShape = _networkState.GetFormation(SphereNetworkFormation.Triangle);
                var clusterShape = _networkState.GetFormation(SphereNetworkFormation.Cluster);
                var isLine = lineShape != null && lineShape.Active && IsFormationLink(lineShape, link.A, link.B);
                var isSquare = IsFormationLink(squareShape, link.A, link.B);
                var isTriangle = triangleShape != null && triangleShape.Active && IsFormationLink(triangleShape, link.A, link.B);
                var isCluster = IsFormationLink(clusterShape, link.A, link.B);
                var active = isLine || isSquare || isTriangle || isCluster;
                var seed = link.A * 17.13f + link.B * 29.71f;
                var phase = _runTime * (active ? 1.9f : 1.35f) + seed;
                for (var point = 0; point <= segments; point++)
                {
                    var p = point / (float)segments;
                    var wave = Mathf.Sin(phase + p * Mathf.PI * 4.2f) * amplitude * Mathf.Sin(Mathf.PI * p);
                    var position = from + delta * p + normal * wave;
                    line.SetPosition(point, position);
                    glowLine.SetPosition(point, position);
                    highlightLine.SetPosition(point, position);
                }

                var pulse = 0.84f + Mathf.Sin(_runTime * 4.2f) * 0.10f;
                var color = new Color(0.388f, 0.725f, 1f, 0.27f * pulse);
                if (isLine) color = new Color(0.388f, 0.902f, 1f, 0.50f * pulse);
                else if (isTriangle) color = new Color(1f, 0.722f, 0.302f, 0.54f * pulse);
                else if (isCluster) color = new Color(0.702f, 0.549f, 1f, 0.44f * pulse);
                else if (isSquare) color = new Color(0.412f, 0.718f, 1f, 0.48f * pulse);

                var glowColor = color; glowColor.a *= active ? 0.22f : 0.12f;
                var mainColor = color; mainColor.a *= active ? 0.82f : 0.64f;
                var highlightColor = color; highlightColor.a *= active ? 0.72f : 0.58f;
                glowLine.startWidth = glowLine.endWidth = (active ? 5.8f : 3.6f) * pixelScale;
                line.startWidth = line.endWidth = (active ? 2.55f : 1.95f) * pixelScale;
                highlightLine.startWidth = highlightLine.endWidth = (active ? 0.92f : 0.70f) * pixelScale;
                glowLine.startColor = glowLine.endColor = glowColor;
                line.startColor = line.endColor = mainColor;
                highlightLine.startColor = highlightLine.endColor = highlightColor;
                UpdateNetworkCometVisuals(i, from, to, color, active, seed, amplitude);
            }
        }

        private LineRenderer CreateNetworkLineRenderer(string objectName, int sortingOrder)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 25;
            line.startWidth = 0.020f;
            line.endWidth = 0.020f;
            line.numCapVertices = 2;
            line.numCornerVertices = 2;
            line.sortingOrder = sortingOrder;
            if (_networkLineMaterial != null) line.sharedMaterial = _networkLineMaterial;
            return line;
        }

        private void EnsureNetworkCometVisuals(int count)
        {
            while (_networkComets.Count < count)
            {
                var headObject = new GameObject("Sphere Network Comet Head");
                headObject.transform.SetParent(transform, false);
                var head = headObject.AddComponent<SpriteRenderer>();
                head.sprite = RuntimeSpriteFactory.NetworkCometHead;
                head.sortingOrder = 8;
                head.enabled = false;

                var tailObject = new GameObject("Sphere Network Comet Tail");
                tailObject.transform.SetParent(transform, false);
                var tail = tailObject.AddComponent<SpriteRenderer>();
                tail.sprite = RuntimeSpriteFactory.NetworkCometTail;
                tail.sortingOrder = 7;
                tail.enabled = false;
                _networkComets.Add(new NetworkCometVisual { Head = head, Tail = tail });
            }
        }

        private void UpdateNetworkCometVisuals(int linkIndex, Vector2 from, Vector2 to, Color linkColor, bool active, float seed, float amplitude)
        {
            var delta = to - from;
            var length = Mathf.Max(0.0001f, delta.magnitude);
            var normal = new Vector2(-delta.y / length, delta.x / length);
            var packetSpeed = active ? 0.34f : 0.27f;
            var tailDistance = active ? 0.075f : 0.06f;
            var alpha = linkColor.a;
            var pixelScale = SpherePlacementRules.SourcePixelsToWorld;
            for (var packet = 0; packet < 3; packet++)
            {
                var index = linkIndex * 3 + packet;
                if (index < 0 || index >= _networkComets.Count) continue;
                var visual = _networkComets[index];
                if (visual.Head == null || visual.Tail == null) continue;
                var raw = ((_runTime * packetSpeed + seed + packet / 3f) % 1f + 1f) % 1f;
                var p = packet % 2 == 0 ? raw : 1f - raw;
                var tailP = Mathf.Max(0f, p - tailDistance);
                var phase = _runTime * (active ? 1.9f : 1.35f) + seed;
                var headWave = Mathf.Sin(phase + p * Mathf.PI * 4.2f) * amplitude * Mathf.Sin(Mathf.PI * p);
                var tailWave = Mathf.Sin(phase + tailP * Mathf.PI * 4.2f) * amplitude * Mathf.Sin(Mathf.PI * tailP);
                var headPosition = from + delta * p + normal * headWave;
                var tailPosition = from + delta * tailP + normal * tailWave;
                var angle = Mathf.Atan2(headPosition.y - tailPosition.y, headPosition.x - tailPosition.x) * Mathf.Rad2Deg;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                visual.Head.enabled = visual.Tail.enabled = true;
                visual.Head.transform.position = new Vector3(headPosition.x + direction.x * 1.15f * pixelScale, headPosition.y + direction.y * 1.15f * pixelScale, -0.04f);
                visual.Head.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                visual.Head.transform.localScale = new Vector3(12.7f * pixelScale, 6f * pixelScale, 1f);
                visual.Head.color = new Color(0.973f, 1f, 1f, (active ? 0.86f : 0.62f) * alpha);
                var tailSpritePosition = headPosition - direction * (8.25f * pixelScale);
                visual.Tail.transform.position = new Vector3(tailSpritePosition.x, tailSpritePosition.y, -0.035f);
                visual.Tail.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                visual.Tail.transform.localScale = new Vector3(13.5f * pixelScale, 3.4f * pixelScale, 1f);
                var tailColor = linkColor; tailColor.a *= active ? 0.48f : 0.34f;
                visual.Tail.color = tailColor;
            }
        }

        private static bool IsFormationLink(SphereNetworkShape shape, int a, int b) =>
            shape != null && shape.Contains(a) && shape.Contains(b);

        private void OnDestroy()
        {
            _nativeTransientPools?.Dispose();_nativeTransientPools=null;
            Application.lowMemory-=OnNativeLowMemory;TraceNativeRoute("runtime-destroy");
            DisposeNativePauseStatsIsland();
            DisposeNativePauseAbilitiesIsland();
            DisposeNativePauseArtifactsIsland();
            DisposeNativePauseSynergiesIsland();
            DisposeNativePauseChromeIsland();
            DisposeNativePauseScreen();
            DisposeNativeHudIsland();
            DisposeNativeRunResult();
            DisposeNativeUpgrade();
            DisposeNativeArtifactRewards();DisposeNativePauseSetDetail();
            DisposeNativeTutorial();
            DisposeNativeWorld();
            DetachNativeMobileGestures();
            DisposeNativeMobileDock();
            DisposeNativeMobileFeedback();
            DisposeNativeMobileNetwork();
            _nativeMobileLayout?.Dispose();_nativeMobileLayout=null;
            if(_nativeMainMenuScreen!=null)Destroy(_nativeMainMenuScreen.gameObject);
            if(_nativeProgressionScreen!=null)Destroy(_nativeProgressionScreen.gameObject);
            if(_nativeSettingsScreen!=null)Destroy(_nativeSettingsScreen.gameObject);
            if(_nativeRegionScreen!=null)Destroy(_nativeRegionScreen.gameObject);
            if(_nativeCharacterScreen!=null)Destroy(_nativeCharacterScreen.gameObject);
            if(_nativeArchiveScreen!=null)Destroy(_nativeArchiveScreen.gameObject);
            if(_nativeRunDocument!=null)Destroy(_nativeRunDocument.gameObject);
            if(_worldSurface!=null)Destroy(_worldSurface.gameObject); if (_networkLineMaterial != null) Destroy(_networkLineMaterial);
            if (_uiPanelTexture != null) Destroy(_uiPanelTexture);
            if (_uiButtonTexture != null) Destroy(_uiButtonTexture);
            if (_uiButtonHoverTexture != null) Destroy(_uiButtonHoverTexture);
            if (_uiButtonActiveTexture != null) Destroy(_uiButtonActiveTexture);
            if (_uiAccentTexture != null) Destroy(_uiAccentTexture);
        }

        private static bool IsRussianUi =>
            PlayerPrefs.GetString(LanguagePreferenceKey, Application.systemLanguage == SystemLanguage.English ? "en" : "ru") != "en";

        private static Texture2D CreateUiTexture(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                name = "EchoSphere UI Surface",
                hideFlags = HideFlags.DontSave,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixel(0, 0, color);
            texture.Apply(false, true);
            return texture;
        }

        private void DrawMainMenu()
        {
            switch (_mainMenuPage)
            {
                case MainMenuPage.Shop: DrawShopScreen(); return;
                case MainMenuPage.Settings: DrawSettingsScreen(); return;
                case MainMenuPage.Leaderboard: DrawLeaderboardScreen(); return;
                case MainMenuPage.Achievements: DrawAchievementsScreen(); return;
                case MainMenuPage.Characters: DrawCharacterSelectScreen(); return;
                case MainMenuPage.Knowledge: DrawKnowledgeBaseScreen(); return;
            }
            DrawSourceMainHome();
        }

        private Rect DrawMenuScreenFrame(string title, string subtitle, float preferredHeight)
        {
            var previousColor = GUI.color;
            GUI.color = new Color(0.008f, 0.018f, 0.055f, 0.98f);
            GUI.DrawTexture(new Rect(0f, 0f, _uiWidth, _uiHeight), _uiPanelTexture);
            GUI.color = new Color(0.08f, 0.54f, 0.94f, 0.7f);
            GUI.DrawTexture(new Rect(0f, 0f, _uiWidth, 3f), _uiAccentTexture);
            GUI.color = previousColor;

            var width = Mathf.Min(760f, _uiWidth - 28f);
            var height = Mathf.Min(preferredHeight, _uiHeight - 28f);
            var panel = new Rect((_uiWidth - width) * 0.5f, (_uiHeight - height) * 0.5f, width, height);
            GUI.Box(panel, GUIContent.none, _box);
            GUI.Label(new Rect(panel.x + 22f, panel.y + 16f, panel.width - 44f, 38f), title, _menuTitle);
            GUI.Label(new Rect(panel.x + 22f, panel.y + 58f, panel.width - 44f, 24f), subtitle, _label);
            GUI.color = new Color(0.1f, 0.68f, 1f, 0.7f);
            GUI.DrawTexture(new Rect(panel.x + 22f, panel.y + 88f, panel.width - 44f, 2f), _uiAccentTexture);
            GUI.color = previousColor;
            return panel;
        }

        private void DrawShopScreen()
        {
            var panel = DrawMenuScreenFrame(
                IsRussianUi ? "МАГАЗИН ПОСТОЯННЫХ УЛУЧШЕНИЙ" : "PERMANENT UPGRADES",
                (IsRussianUi ? "ЗОЛОТО" : "GOLD") + "  ●  " + GetGold(), 620f);
            var ids = PersistentUpgradeRules.Ids;
            var rowTop = panel.y + 101f;
            var rowHeight = 58f;
            var left = panel.x + 20f;
            var rowWidth = panel.width - 40f;
            for (var i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var level = GetShopUpgradeLevel(id);
                var maxLevel = PersistentUpgradeRules.GetMaxLevel(id);
                var cost = PersistentUpgradeRules.GetCost(id, level);
                var row = new Rect(left, rowTop + i * rowHeight, rowWidth, rowHeight - 5f);
                GUI.Box(row, GUIContent.none, _box);
                GUI.Label(new Rect(row.x + 12f, row.y + 4f, row.width * 0.54f, 22f),
                    GetShopUpgradeName(id), _label);
                GUI.Label(new Rect(row.x + 12f, row.y + 25f, row.width * 0.54f, 20f),
                    GetShopUpgradeDescription(id) + "  ·  " + (IsRussianUi ? "КУПЛЕНО" : "OWNED") + " " + level + "/" + maxLevel,
                    _sphereDescriptionStyle);
                var buyRect = new Rect(row.xMax - 160f, row.y + 6f, 146f, row.height - 12f);
                var canBuy = level < maxLevel && GetGold() >= cost;
                var oldEnabled = GUI.enabled;
                GUI.enabled = canBuy;
                var buttonLabel = level >= maxLevel
                    ? (IsRussianUi ? "МАКС." : "MAX")
                    : cost.ToString() + "  ●";
                if (GUI.Button(buyRect, buttonLabel, _button))
                    PurchasePersistentUpgrade(id);
                GUI.enabled = oldEnabled;
            }

            if (GUI.Button(new Rect(panel.x + 22f, panel.yMax - 48f, 132f, 34f),
                IsRussianUi ? "НАЗАД" : "BACK", _button))
                _mainMenuPage = MainMenuPage.Home;
            GUI.Label(new Rect(panel.x + 168f, panel.yMax - 45f, panel.width - 190f, 28f),
                IsRussianUi ? "Цена следующего уровня растёт в 3 раза." : "Each next level costs 3× more.", _label);
        }

        private string GetShopUpgradeName(string id)
        {
            switch (id)
            {
                case "dmg": return IsRussianUi ? "Урон сфер" : "Sphere Damage";
                case "radius": return IsRussianUi ? "Радиус сфер" : "Sphere Radius";
                case "speed": return IsRussianUi ? "Скорость движения" : "Move Speed";
                case "spheres": return IsRussianUi ? "Стартовые сферы" : "Start Spheres";
                case "hp": return IsRussianUi ? "Стартовое HP" : "Start HP";
                case "crit": return IsRussianUi ? "Шанс крита" : "Crit Chance";
                case "xp": return IsRussianUi ? "Получаемый опыт" : "XP Gain";
                default: return id;
            }
        }

        private string GetShopUpgradeDescription(string id)
        {
            switch (id)
            {
                case "dmg": return "+5% " + (IsRussianUi ? "урона" : "damage");
                case "radius": return "+5% " + (IsRussianUi ? "радиуса" : "radius");
                case "speed": return "+5% " + (IsRussianUi ? "скорости" : "speed");
                case "spheres": return "+1 " + (IsRussianUi ? "сфера" : "sphere");
                case "hp": return "+10 HP";
                case "crit": return "+5% " + (IsRussianUi ? "шанс крита" : "crit chance");
                case "xp": return "+5% " + (IsRussianUi ? "опыта" : "XP");
                default: return "";
            }
        }

        private void PurchasePersistentUpgrade(string id)
        {
            var level = GetShopUpgradeLevel(id);
            var cost = PersistentUpgradeRules.GetCost(id, level);
            if (level >= PersistentUpgradeRules.GetMaxLevel(id) || cost <= 0 || GetGold() < cost) return;
            _shopSave.upgrades.Set(id, level + 1);
            SaveGold(GetGold() - cost);
            SaveShopState();
            ShowMessage((IsRussianUi ? "КУПЛЕНО: " : "PURCHASED: ") + GetShopUpgradeName(id));
        }

        private void DrawCharacterSelectScreen()
        {
            EnsureCharacterProfilesLoaded();
            var panel = DrawMenuScreenFrame(
                IsRussianUi ? "ПЕРСОНАЖИ" : "CHARACTERS",
                IsRussianUi ? "Персонаж меняет стиль билда, но не управление." : "Characters change your build style, not the controls.", 640f);
            GUI.Label(new Rect(panel.x + 22f, panel.y + 82f, panel.width - 44f, 20f),
                (IsRussianUi ? "ЗОЛОТО" : "GOLD") + "  ●  " + GetGold() +
                "     ·     " + (IsRussianUi ? "ВЫБРАН" : "SELECTED") + ": " +
                (IsRussianUi ? CharacterRules.Get(_sphereCharacterId).NameRu : CharacterRules.Get(_sphereCharacterId).NameEn), _label);

            var viewport = new Rect(panel.x + 12f, panel.y + 108f, panel.width - 24f, panel.height - 164f);
            var contentWidth = Mathf.Max(120f, viewport.width - 18f);
            var contentHeight = 0f;
            for (var i = 0; i < CharacterRules.All.Count; i++)
            {
                var definition = CharacterRules.All[i];
                var profile = GetCharacterProfile(definition.Id);
                var expanded = profile.unlocked && _expandedCharacterId == CharacterRules.ToSourceId(definition.Id);
                contentHeight += 184f + (expanded ? definition.Mastery.Count * 36f + 10f : 0f) + 8f;
            }
            _characterScrollPosition = GUI.BeginScrollView(viewport, _characterScrollPosition, new Rect(0f, 0f, contentWidth, contentHeight));
            var cardY = 0f;
            for (var i = 0; i < CharacterRules.All.Count; i++)
            {
                var definition = CharacterRules.All[i];
                var profile = GetCharacterProfile(definition.Id);
                var isSelected = definition.Id == _sphereCharacterId;
                var expanded = profile.unlocked && _expandedCharacterId == CharacterRules.ToSourceId(definition.Id);
                var cardHeight = 184f + (expanded ? definition.Mastery.Count * 36f + 10f : 0f);
                var card = new Rect(0f, cardY, contentWidth, cardHeight);
                GUI.Box(card, GUIContent.none, _box);

                var oldColor = GUI.color;
                if (ColorUtility.TryParseHtmlString(definition.ColorHex, out var characterColor))
                    GUI.color = new Color(characterColor.r, characterColor.g, characterColor.b, 0.95f);
                GUI.DrawTexture(new Rect(card.x + 8f, card.y + 9f, 3f, 18f), _uiAccentTexture);
                GUI.color = oldColor;

                GUI.Label(new Rect(card.x + 18f, card.y + 6f, card.width - 150f, 22f),
                    (IsRussianUi ? definition.NameRu : definition.NameEn) + "  ·  " +
                    (IsRussianUi ? definition.RoleRu : definition.RoleEn), _label);
                GUI.Label(new Rect(card.x + 18f, card.y + 29f, card.width - 28f, 29f),
                    IsRussianUi ? definition.DescriptionRu : definition.DescriptionEn, _sphereDescriptionStyle);
                GUI.Label(new Rect(card.x + 18f, card.y + 56f, card.width - 28f, 39f),
                    IsRussianUi ? definition.MechanicRu : definition.MechanicEn, _sphereDescriptionStyle);

                var signature = GameCatalog.Spheres[(int)definition.SignatureSphere];
                var partner = GameCatalog.Spheres[(int)definition.PartnerSphere];
                GUI.Label(new Rect(card.x + 18f, card.y + 96f, card.width - 28f, 18f),
                    (IsRussianUi ? "СИГНАТУРНАЯ" : "SIGNATURE") + ": " +
                    (IsRussianUi ? signature.DisplayNameRu : signature.DisplayName) + "  ·  " +
                    (IsRussianUi ? "ПАРТНЁР" : "PARTNER") + ": " +
                    (IsRussianUi ? partner.DisplayNameRu : partner.DisplayName), _sphereDescriptionStyle);

                var masteryThreshold = CharacterRules.GetNextMasteryThreshold(profile.masteryLevel);
                var xpText = masteryThreshold < 0
                    ? (IsRussianUi ? "МАКС." : "MAX")
                    : profile.masteryXp + "/" + masteryThreshold + " XP";
                GUI.Label(new Rect(card.x + 18f, card.y + 115f, card.width - 28f, 17f),
                    (IsRussianUi ? "МАСТЕРСТВО " : "MASTERY ") + profile.masteryLevel + "/10  ·  " + xpText, _sphereDescriptionStyle);
                var currentThreshold = CharacterRules.GetMasteryThreshold(profile.masteryLevel);
                var progress = masteryThreshold < 0 ? 1f :
                    Mathf.Clamp01((profile.masteryXp - currentThreshold) / (float)Mathf.Max(1, masteryThreshold - currentThreshold));
                DrawProgressBar(new Rect(card.x + 18f, card.y + 133f, card.width - 36f, 5f), progress, new Color(0.22f, 0.85f, 1f, 0.95f));

                var actionY = card.y + 145f;
                if (profile.unlocked)
                {
                    var actionWidth = Mathf.Min(124f, (card.width - 48f) * 0.5f);
                    if (GUI.Button(new Rect(card.x + 18f, actionY, actionWidth, 29f),
                        isSelected ? (IsRussianUi ? "ВЫБРАН" : "SELECTED") : (IsRussianUi ? "ВЫБРАТЬ" : "SELECT"), _button))
                        SelectOrUnlockCharacter(definition.Id);
                    if (GUI.Button(new Rect(card.x + 26f + actionWidth, actionY, actionWidth, 29f),
                        expanded ? (IsRussianUi ? "СКРЫТЬ" : "HIDE") : (IsRussianUi ? "МАСТЕРСТВО" : "MASTERY"), _button))
                        _expandedCharacterId = expanded ? "" : CharacterRules.ToSourceId(definition.Id);
                }
                else
                {
                    var buyWidth = Mathf.Min(150f, card.width - 36f);
                    if (GUI.Button(new Rect(card.x + 18f, actionY, buyWidth, 29f),
                        (IsRussianUi ? "ОТКРЫТЬ · " : "UNLOCK · ") + definition.UnlockCost + " ●", _button))
                        SelectOrUnlockCharacter(definition.Id);
                }

                if (expanded)
                {
                    var rowY = card.y + 180f;
                    for (var j = 0; j < definition.Mastery.Count; j++)
                    {
                        var mastery = definition.Mastery[j];
                        var reached = mastery.Level <= profile.masteryLevel;
                        var current = mastery.Level == profile.masteryLevel;
                        var row = new Rect(card.x + 8f, rowY + j * 36f, card.width - 16f, 33f);
                        GUI.Box(row, GUIContent.none, _box);
                        var rowColor = GUI.color;
                        GUI.color = reached ? new Color(0.34f, 0.88f, 1f, 1f) : new Color(0.5f, 0.62f, 0.75f, 0.8f);
                        GUI.Label(new Rect(row.x + 7f, row.y + 2f, row.width - 16f, 15f),
                            mastery.Level + ". " + (IsRussianUi ? mastery.TitleRu : mastery.TitleEn) +
                            (current ? (IsRussianUi ? " · ТЕКУЩИЙ" : " · CURRENT") : "") +
                            "   " + CharacterRules.GetMasteryThreshold(mastery.Level) + " XP", _sphereDescriptionStyle);
                        GUI.color = rowColor;
                        GUI.Label(new Rect(row.x + 7f, row.y + 16f, row.width - 14f, 16f),
                            IsRussianUi ? mastery.DescriptionRu : mastery.DescriptionEn, _sphereDescriptionStyle);
                    }
                }
                cardY += cardHeight + 8f;
            }
            GUI.EndScrollView();

            if (GUI.Button(new Rect(panel.x + 22f, panel.yMax - 48f, 132f, 34f),
                IsRussianUi ? "НАЗАД" : "BACK", _button))
                _mainMenuPage = MainMenuPage.Home;
        }

        private void DrawSettingsScreen() => DrawSourceSettings();

        private void DrawLeaderboardScreen()
        {
            var panel = DrawMenuScreenFrame(
                IsRussianUi ? "РЕКОРДЫ ЗАБЕГА" : "RUN RECORDS",
                IsRussianUi ? "ТОП-10 ПО ВРЕМЕНИ ВЫЖИВАНИЯ" : "TOP 10 BY SURVIVAL TIME", 610f);

            if (_leaderboardEntries.Count == 0)
            {
                GUI.Label(new Rect(panel.x + 30f, panel.y + 150f, panel.width - 60f, 48f),
                    IsRussianUi ? "ПОКА НЕТ РЕЗУЛЬТАТОВ" : "NO SCORES YET", _label);
            }
            else
            {
                var left = panel.x + 22f;
                var tableWidth = panel.width - 44f;
                GUI.Label(new Rect(left, panel.y + 101f, tableWidth * 0.14f, 24f),
                    IsRussianUi ? "МЕСТО" : "RANK", _label);
                GUI.Label(new Rect(left + tableWidth * 0.16f, panel.y + 101f, tableWidth * 0.46f, 24f),
                    IsRussianUi ? "ИГРОК" : "PLAYER", _label);
                GUI.Label(new Rect(left + tableWidth * 0.66f, panel.y + 101f, tableWidth * 0.14f, 24f),
                    IsRussianUi ? "ВОЛНА" : "WAVE", _label);
                GUI.Label(new Rect(left + tableWidth * 0.82f, panel.y + 101f, tableWidth * 0.17f, 24f),
                    IsRussianUi ? "ВРЕМЯ" : "TIME", _label);

                for (var i = 0; i < _leaderboardEntries.Count; i++)
                {
                    var entry = _leaderboardEntries[i];
                    var rowY = panel.y + 128f + i * 35f;
                    GUI.Box(new Rect(left, rowY, tableWidth, 32f), GUIContent.none, _box);
                    GUI.Label(new Rect(left + 10f, rowY + 4f, tableWidth * 0.14f, 24f), "#" + (i + 1), _label);
                    GUI.Label(new Rect(left + tableWidth * 0.16f, rowY + 4f, tableWidth * 0.46f, 24f), entry.name, _label);
                    GUI.Label(new Rect(left + tableWidth * 0.66f, rowY + 4f, tableWidth * 0.14f, 24f), entry.wave.ToString(), _label);
                    GUI.Label(new Rect(left + tableWidth * 0.82f, rowY + 4f, tableWidth * 0.17f, 24f), Mathf.FloorToInt(entry.time) + "s", _label);
                }
            }

            if (GUI.Button(new Rect(panel.x + 22f, panel.yMax - 48f, 132f, 34f),
                IsRussianUi ? "НАЗАД" : "BACK", _button))
                _mainMenuPage = MainMenuPage.Home;
        }

        private void DrawKnowledgeBaseScreen()
        {
            DrawSourceKnowledgeBaseScreen();
        }

        private void DrawArtifactProtocolOverlay()
        {
            var previousColor = GUI.color;
            GUI.color = new Color(0.006f, 0.015f, 0.045f, 0.96f);
            GUI.DrawTexture(new Rect(0f, 0f, _uiWidth, _uiHeight), _uiPanelTexture);
            GUI.color = previousColor;

            var panelWidth = Mathf.Min(760f, _uiWidth - 24f);
            var panelHeight = Mathf.Min(660f, _uiHeight - 24f);
            var panel = new Rect((_uiWidth - panelWidth) * 0.5f, (_uiHeight - panelHeight) * 0.5f, panelWidth, panelHeight);
            GUI.Box(panel, GUIContent.none, _box);
            GUI.Label(new Rect(panel.x + 20f, panel.y + 14f, panel.width - 40f, 34f),
                IsRussianUi ? "АРТЕФАКТЫ / ПРОТОКОЛЫ" : "ARTIFACTS / PROTOCOLS", _menuTitle);
            GUI.Label(new Rect(panel.x + 22f, panel.y + 55f, panel.width - 44f, 22f),
                IsRussianUi ? "Открытые протоколы и текущие боевые условия" : "Discovered protocols and current combat conditions",
                _sphereDescriptionStyle);

            var view = new Rect(panel.x + 16f, panel.y + 86f, panel.width - 32f, panel.height - 142f);
            const float rowHeight = 68f;
            var contentHeight = ArtifactProtocolRules.Protocols.Count * rowHeight;
            _artifactProtocolScrollPosition = GUI.BeginScrollView(
                view, _artifactProtocolScrollPosition, new Rect(0f, 0f, view.width - 18f, contentHeight));

            for (var i = 0; i < ArtifactProtocolRules.Protocols.Count; i++)
            {
                var protocol = ArtifactProtocolRules.Protocols[i];
                var discovered = IsArtifactProtocolSetComplete(protocol.SetId);
                var active = discovered && IsArtifactProtocolCurrentlyActive(protocol);
                var rect = new Rect(0f, i * rowHeight, view.width - 18f, rowHeight - 5f);
                GUI.Box(rect, GUIContent.none, _box);

                var status = !discovered
                    ? "???"
                    : protocol.HasUnmappedRuntimeState
                        ? (IsRussianUi ? "ОЖИДАЕТ СИСТЕМУ" : "WAITING FOR STATE")
                        : active
                            ? (IsRussianUi ? "АКТИВЕН" : "ACTIVE")
                            : (IsRussianUi ? "ОТКРЫТ" : "DISCOVERED");
                var title = discovered
                    ? (IsRussianUi ? protocol.NameRussian : protocol.NameEnglish)
                    : "???";
                var description = discovered
                    ? (IsRussianUi ? protocol.DescriptionRussian : protocol.DescriptionEnglish)
                    : (IsRussianUi ? "Неизвестный протокол." : "Unknown protocol.");

                GUI.Label(new Rect(rect.x + 10f, rect.y + 4f, rect.width - 164f, 24f), title, _label);
                var previousCardColor = GUI.color;
                GUI.color = active ? new Color(0.33f, 0.91f, 1f, 1f)
                    : protocol.HasUnmappedRuntimeState && discovered ? new Color(1f, 0.75f, 0.32f, 1f)
                    : new Color(0.52f, 0.68f, 0.82f, 0.9f);
                GUI.Label(new Rect(rect.xMax - 154f, rect.y + 5f, 144f, 20f), status, _sphereDescriptionStyle);
                GUI.color = previousCardColor;
                GUI.Label(new Rect(rect.x + 10f, rect.y + 28f, rect.width - 20f, 34f), description, _sphereDescriptionStyle);
            }

            GUI.EndScrollView();
            if (GUI.Button(new Rect(panel.x + 20f, panel.yMax - 44f, 150f, 30f),
                IsRussianUi ? "ВОЗВРАТ В ЗАБЕГ" : "BACK TO RUN", _button))
            {
                _artifactPanelOpen = false;
                SetUserPaused(false);
            }
        }

        private void DrawAchievementsScreen()
        {
            var panel = DrawMenuScreenFrame(
                IsRussianUi ? "ДОСТИЖЕНИЯ" : "ACHIEVEMENTS",
                (IsRussianUi ? "ОТКРЫТО" : "UNLOCKED") + "  " + _unlockedAchievementIds.Count + "/" + SourceAchievements.Length, 620f);
            for (var i = 0; i < SourceAchievements.Length; i++)
            {
                var achievement = SourceAchievements[i];
                var unlocked = _unlockedAchievementIds.Contains(achievement.Id);
                var rowY = panel.y + 101f + i * 35f;
                GUI.Box(new Rect(panel.x + 22f, rowY, panel.width - 44f, 32f), GUIContent.none, _box);
                var statusColor = GUI.color;
                GUI.color = unlocked ? new Color(1f, 0.73f, 0.28f, 1f) : new Color(0.52f, 0.68f, 0.82f, 0.75f);
                GUI.Label(new Rect(panel.x + 34f, rowY + 2f, 210f, 24f),
                    IsRussianUi ? achievement.NameRu : achievement.NameEn, _label);
                GUI.color = statusColor;
                GUI.Label(new Rect(panel.x + 248f, rowY + 2f, panel.width - 370f, 24f),
                    IsRussianUi ? achievement.DescriptionRu : achievement.DescriptionEn, _sphereDescriptionStyle);
                GUI.Label(new Rect(panel.xMax - 112f, rowY + 2f, 80f, 24f),
                    unlocked ? (IsRussianUi ? "ОТКРЫТО" : "UNLOCKED") : (IsRussianUi ? "ЗАКРЫТО" : "LOCKED"), _sphereDescriptionStyle);
            }

            if (GUI.Button(new Rect(panel.x + 22f, panel.yMax - 48f, 132f, 34f),
                IsRussianUi ? "НАЗАД" : "BACK", _button))
                _mainMenuPage = MainMenuPage.Home;
        }

        private void DrawRegionSelect() => DrawSourceRegionSelect();

        private void BeginSelectedRegionRun()
        {
            if (!IsRegionAvailable(_selectedRegion))
            {
                _selectedRegion = EchoRegionId.ResonanceBasin;
                ShowMessage(IsRussianUi ? "СНАЧАЛА СТАБИЛИЗИРУЙ РЕЗОНАНСНЫЙ БАССЕЙН" : "STABILIZE RESONANCE BASIN FIRST");
                return;
            }

            var challengeAllowed = IsRegionStabilized(_selectedRegion);
            if (_regionChallengeId != "none" && !challengeAllowed)
            {
                ShowMessage(IsRussianUi ? "СНАЧАЛА СТАБИЛИЗИРУЙ РЕГИОН" : "STABILIZE THE REGION FIRST");
                _regionChallengeId = "none";
                _regionEndlessMode = false;
                return;
            }
            if (_regionEndlessMode && !IsRegionEndlessUnlocked(_selectedRegion))
            {
                ShowMessage(IsRussianUi ? "СНАЧАЛА ПРОЙДИ ТРИ ИСПЫТАНИЯ" : "CLEAR ALL THREE CHALLENGES FIRST");
                _regionEndlessMode = false;
                return;
            }
            PlayerPrefs.SetInt(SelectedRegionPreferenceKey, (int)_selectedRegion);
            _regionSchedule = RegionBossScheduleRules.Create(_selectedRegion, EchoRegionMode.Stabilization);
            _endlessNextBossAt = _selectedRegion == EchoRegionId.SpectralRift ? 450f : 480f;
            _endlessBossesSpawned = 0;
            _fractureIntervalIndex = 0;
            _regionStabilized = false;
            _regionFieldVisuals?.SetRegion(_selectedRegion);
            _regionSelect = false;
            _mainMenuPage = MainMenuPage.Home;
            _mainMenu = false;
            _tutorialStep=PlayerPrefs.GetInt(TutorialCompletedPreferenceKey,0)==0?0:-1;_tutorialUpgradeOpened=false;_tutorialStartPosition=_player!=null?_player.position:Vector3.zero;
            _paused = _tutorialStep >= 0 && _tutorialStep != 1 && _tutorialStep != 2;
            _userPaused = false;
            _messageTimer = 0f;
            _runTime = 0f;
#if UNITY_EDITOR
            _sourceRenderState=default;_sourceRenderEntities.Clear();
#endif
            _nativePresentedDominant=_nativePresentedSecondary=null;
            _rng.Reset(PlayerLoopRules.CreateRunSeed(_playerName, PlayerPrefs.GetString(DifficultyPreferenceKey, "normal"), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
            _sourceLightningVisuals.Clear(); _mutationStage = 0; _chaosOrbTimer = _chaosOrbBuffTimer = _buffTimer = _deathTimer = 0f; _chaosOrbBuff = null;
            ClearRunSnapshot();
            _regionLoreDiscovered.Clear();
            _regionXpReward = _regionGoldReward = _regionChallengeContractsCompleted = _regionMasteryChallengesCompleted = 0;
            _regionMasteryGeometryTypes.Clear();
            _regionMasteryDominantSeconds = _regionDamageTaken = 0f;
            _regionLastHp = -1f;
            _combo = 0;
            _comboTimer = 0f;
            _comboMultiplier = 1f;
            _kills = 0;
            _bossesDefeated = 0;
            _runEliteKills = 0;
            _runEvolutions = 0;
            _runChestOpens = 0;
            _runResultSaved = false;
            _runGoldEarned = 0;
            _levelUpAbilityPity = 0;
            _levelUpSpherePity = 0;
            _levelUpRerollsRemaining = 1;
            _levelUpLocksRemaining = 1;
            _levelUpLockChoiceKey = null;
            _recentUpgradeChoiceKeys.Clear();
            _levelUpChoices.Clear();
            _oracleForecastKeys.Clear();
            _oracleForecastRerollUsed = false;
            _fractalFormationHistory.Clear();
            _fractalEchoNodes.Clear();
            _fractalEchoTimer = _fractalEchoPulseTimer = _fractalEchoCooldown = _fractalEchoStrength = 0f;
            _voidPhantomSource = null;
            _voidPhantomPosition = Vector2.zero;
            _voidPhantomTimer = _voidPhantomPulseTimer = 0f;
            _voidPhantomPulseCount = 0;
            _spheristChorusHits = _berserkerRedlineHits = 0;
            _characterScreenShakeIntensity = 0f;
            _oracleSourceFlashText = string.Empty;
            _oracleSourceFlashTimer = 0f;
            _characterMasteryRunXp = 0f;
            _characterMasteryLastTime = -1f;
            _characterMasteryLastEliteKills = 0;
            _characterMasteryLastBosses = 0;
            _characterMasteryLastResonanceEvents = 0;
            _wave = 0;
            _waveEnemiesToSpawn = 0;
            _spawnTimer = 3f;
        }


        // Translate the active conceptStyle.css battlefield overlay: a very
        // low-contrast 140 px technical grid with a restrained center focus.
        // This is UI composition only; gameplay coordinates and simulation are untouched.
        private void DrawHudBackdrop()
        {
            var previousColor = GUI.color;
            GUI.color = new Color(0.47f, 0.83f, 1f, 0.035f);
            const float cell = 140f;
            for (var x = 0f; x <= _uiWidth; x += cell)
                GUI.DrawTexture(new Rect(x, 0f, 1f, _uiHeight), _uiAccentTexture);
            for (var y = 0f; y <= _uiHeight; y += cell)
                GUI.DrawTexture(new Rect(0f, y, _uiWidth, 1f), _uiAccentTexture);

            // Center cross-lines are the restrained focal cue used by the
            // source app shell, not a new gameplay-world object.
            GUI.color = new Color(0.47f, 0.83f, 1f, 0.055f);
            GUI.DrawTexture(new Rect(_uiWidth * 0.5f - 0.5f, 0f, 1f, _uiHeight), _uiAccentTexture);
            GUI.DrawTexture(new Rect(0f, _uiHeight * 0.5f - 0.5f, _uiWidth, 1f), _uiAccentTexture);
            GUI.color = previousColor;
        }

        private void DrawProgressBar(Rect rect, float amount, Color fillColor)
        {
            var previousColor = GUI.color;
            GUI.color = new Color(0.018f, 0.045f, 0.09f, 1f);
            GUI.DrawTexture(rect, _uiPanelTexture);
            var inset = new Rect(rect.x + 1f, rect.y + 1f,
                Mathf.Max(0f, (rect.width - 2f) * Mathf.Clamp01(amount)),
                Mathf.Max(1f, rect.height - 2f));
            if (inset.width > 0f)
            {
                GUI.color = fillColor;
                GUI.DrawTexture(inset, _uiAccentTexture);
            }
            GUI.color = previousColor;
        }


        private bool IsScreenPointOverUi(Vector2 screenPoint)
        {
            if (_uiWidth <= 0f || _uiHeight <= 0f) return false;
            var point = new Vector2(screenPoint.x / Mathf.Max(1f, _uiScale), _uiHeight - screenPoint.y / Mathf.Max(1f, _uiScale));
            return IsSourceHudPoint(point);
        }

        private float GetSpherePickerDescriptionHeight()
        {
            var definition = GameCatalog.Spheres[(int)_selectedSphereType];
            // The source description expands naturally for Orbital's multi-line
            // text instead of clipping its last lines.
            var lines = Mathf.Max(1, Mathf.CeilToInt(definition.DescriptionEn.Length / 42f));
            return Mathf.Max(64f, 30f + lines * 18f);
        }

        private Rect GetSpherePickerRect()
        {
            // Source MobileControls uses a 5-column grid of compact type glyphs,
            // with the selected type's name/description directly above it.
            var width = Mathf.Min(340f, _uiWidth - 24f);
            var height = GetSpherePickerDescriptionHeight() + 135f;
            return new Rect(IsLeftHanded ? _uiWidth - width - 12f : 12f, _uiHeight - 16f - height, width, height);
        }

        private Vector2 ScreenToWorldPoint(Vector2 screenPoint)
        {
            if (_camera == null) return Vector2.zero;
            var world = _camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, -_camera.transform.position.z));
            return new Vector2(world.x, world.y);
        }

        private Vector2 WorldToGuiPoint(Vector2 worldPoint)
        {
            var screen = _camera == null ? Vector3.zero : _camera.WorldToScreenPoint(worldPoint);
            return new Vector2(screen.x / Mathf.Max(1f, _uiScale), _uiHeight - screen.y / Mathf.Max(1f, _uiScale));
        }

        private void HandleSpherePointerInput()
        {
            if (_mainMenu || _paused || _gameOver || _levelUp || _choosingEvolution || _stellaRewardPending)
            { ClearSourcePointers(); return; }
            if (Input.touchCount > 0)
            {
                for (var i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    HandleSourcePointer(touch.fingerId, touch.position, touch.phase == TouchPhase.Began,
                        touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled);
                }
                return;
            }
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
                HandleSourcePointer(-1, Input.mousePosition, Input.GetMouseButtonDown(0), Input.GetMouseButtonUp(0));
        }

        private void BeginSpherePointer(Vector2 screenPoint, bool fromTouch, int touchId)
        {
            if (_mainMenu || _paused || _gameOver || _levelUp || _choosingEvolution || _stellaRewardPending) return;
            if (IsScreenPointOverUi(screenPoint)) return;

            _spherePointerActive = true;
            _spherePointerFromTouch = fromTouch;
            _spherePointerTouchId = touchId;
            _spherePointerStartScreen = screenPoint;
            _spherePointerWorld = ScreenToWorldPoint(screenPoint);
            _spherePointerMoved = false;
            _spherePointerIsMovement = false;
            _spherePointerIsPlacing = false;
            _sourceLockedSphereTouch = false;
            _spherePointerTarget = null;
            _spherePointerPositionValid = false;
            _spherePointerPreview = null;
            _spherePointerPreviewIndex = -1;
            _touchStart = screenPoint;

            CaptureSpherePointerOriginalFormation();
            var hit = FindSphereAtWorld(_spherePointerWorld, 34f * SpherePlacementRules.SourcePixelsToWorld);
            if (hit != null && _tutorialStep != 1 && _tutorialStep != 2)
            {
                if (!_sphereMovementLocked) _spherePointerTarget = hit;
                else _sourceLockedSphereTouch = true;
                return;
            }

            _spherePointerIsMovement = _tutorialStep != 2 && !_sourceJoystickId.HasValue && (IsLeftHanded ? screenPoint.x < Screen.width * 0.5f : screenPoint.x >= Screen.width * 0.5f);
            _spherePointerIsPlacing = _tutorialStep != 1 && !_spherePointerIsMovement
                && _spheres.Count < GetCurrentMaxSpheres();
        }

        private void MoveSpherePointer(Vector2 screenPoint)
        {
            if (!_spherePointerActive || _sourceLockedSphereTouch) return;
            _spherePointerWorld = ScreenToWorldPoint(screenPoint);
            var deadzone = 12f * Mathf.Max(1f, _uiScale);
            if ((screenPoint - _spherePointerStartScreen).sqrMagnitude > deadzone * deadzone)
                _spherePointerMoved = true;
            if (!_spherePointerMoved) return;

            if (_spherePointerTarget != null)
            {
                var index = _spheres.IndexOf(_spherePointerTarget);
                _spherePointerPositionValid = index >= 0 && CanPlaceSphereAt(_spherePointerWorld, _spherePointerTarget);
                BuildSphereNetworkPreview(_spherePointerWorld, index, false);
            }
            else if (_spherePointerIsPlacing)
            {
                _spherePointerPositionValid = _spheres.Count < GetCurrentMaxSpheres()
                    && CanPlaceSphereAt(_spherePointerWorld, null);
                BuildSphereNetworkPreview(_spherePointerWorld, -1, true);
            }
        }

        private void FinishSpherePointer(Vector2 screenPoint)
        {
            if (!_spherePointerActive) return;
            _spherePointerWorld = ScreenToWorldPoint(screenPoint);
            var moved = _spherePointerMoved;
            var target = _spherePointerTarget;
            var placing = _spherePointerIsPlacing;
            var world = _spherePointerWorld;

            if (!moved)
            {
                if (_tutorialStep == 2 && FindSphereAtWorld(world, 26f * SpherePlacementRules.SourcePixelsToWorld) != null) { ClearSpherePointer(); return; }
                if (target != null)
                    RemoveSphereAgent(target);
                else
                    TryPlaceOrRemoveSphereAt(world);
            }
            else if (target != null)
            {
                if (target != null && target.gameObject != null && CanPlaceSphereAt(world, target))
                {
                    var committed = SpherePlacementRules.ClampReposition(
                        new Vec2(world.x, world.y),
                        SpherePlacementRules.WorldWidth,
                        SpherePlacementRules.WorldHeight);
                    target.transform.position = new Vector3(committed.X, committed.Y, target.transform.position.z);
                    target.SetFollowMode(_follow, (Vector2)_player.position);
                    RefreshNetworkState();
                    CaptureSphereFormationMemoryIfChanged();
                    ShowTimedCombatMessage(IsRussianUi ? "Сфера перемещена" : "Sphere moved", .8f);
                }
                else
                {
                    ShowTimedCombatMessage(IsRussianUi ? "Недоступная позиция" : "Invalid position", .65f);
                }
            }
            else if (placing)
            {
                if (_spheres.Count < GetCurrentMaxSpheres() && CanPlaceSphereAt(world, null))
                    TryPlaceOrRemoveSphereAt(world);
                else
                    ShowTimedCombatMessage(IsRussianUi ? "Путь перекрыт" : "Path blocked", .8f);
            }

            ClearSpherePointer();
        }

        private void ClearSpherePointer()
        {
            _spherePointerActive = false;
            _spherePointerFromTouch = false;
            _spherePointerTouchId = -1;
            _spherePointerMoved = false;
            _spherePointerIsMovement = false;
            _spherePointerIsPlacing = false;
            _spherePointerTarget = null;
            _spherePointerPositionValid = false;
            _spherePointerPreview = null;
            _spherePointerPreviewIndex = -1;
        }

        private void CaptureSpherePointerOriginalFormation()
        {
            RefreshNetworkState();
            _spherePointerOriginalFormation = SphereNetworkFormation.None;
            _spherePointerOriginalNodeIndexes = null;
            _spherePointerOriginalFormationPoints = null;
            var formation = _networkState == null ? null : _networkState.DominantFormation;
            if (formation == null || !formation.Active || formation.NodeIndexes.Count == 0) return;

            _spherePointerOriginalFormation = formation.Type;
            _spherePointerOriginalNodeIndexes = formation.NodeIndexes.ToArray();
            _spherePointerOriginalFormationPoints = new Vector2[formation.NodeIndexes.Count];
            for (var i = 0; i < formation.NodeIndexes.Count; i++)
            {
                if (!TryGetNetworkNodePosition(formation.NodeIndexes[i], out var position)) continue;
                _spherePointerOriginalFormationPoints[i] = position;
            }
        }

        private void CaptureSphereFormationMemoryIfChanged()
        {
            var oldIndexes = _spherePointerOriginalNodeIndexes;
            var oldPoints = _spherePointerOriginalFormationPoints;
            if (_spherePointerOriginalFormation == SphereNetworkFormation.None || oldIndexes == null || oldPoints == null) return;
            var next = _networkState == null ? null : _networkState.DominantFormation;
            var changed = next == null || !next.Active || next.Type != _spherePointerOriginalFormation
                || next.NodeIndexes.Count != oldIndexes.Length;
            if (!changed && next != null)
            {
                for (var i = 0; i < oldIndexes.Length; i++)
                    if (next.NodeIndexes[i] != oldIndexes[i]) { changed = true; break; }
            }
            if (!changed) return;
            _formationMemoryType = _spherePointerOriginalFormation;
            _formationMemoryPoints = (Vector2[])oldPoints.Clone();
            _formationMemoryTimer = 0.9f;
        }

        private SphereAttackAgent FindSphereAtWorld(Vector2 world, float tolerance)
        {
            SphereAttackAgent nearest = null;
            var best = tolerance * tolerance;
            for (var i = 0; i < _spheres.Count; i++)
            {
                var sphere = _spheres[i];
                if (sphere == null || !sphere.gameObject.activeInHierarchy) continue;
                var distance = ((Vector2)sphere.transform.position - world).sqrMagnitude;
                if (distance < best) { best = distance; nearest = sphere; }
            }
            return nearest;
        }

        private int GetCurrentMaxSpheres() =>
            SpherePlacementRules.GetMaxSpheres(GetAbilityLevel(RunUpgradeKind.MaxSpheres), PersistentUpgradeRules.GetExtraSpheres(GetShopUpgradeLevel("spheres")));

        private int CountSpheresOfType(SphereId type)
        {
            var count = 0;
            for (var i = 0; i < _spheres.Count; i++)
                if (_spheres[i] != null && _spheres[i].gameObject.activeInHierarchy && _spheres[i].Type == type) count++;
            return count;
        }

        public float GetSphereCopyOutputMultiplier(SphereAttackAgent sphere)
        {
            if (sphere == null) return 1f;
            var copyCap = SpherePlacementRules.GetCharacterSphereCopyCap(
                _sphereCharacterId, sphere.Type, _characterMasteryLevel);
            if (copyCap <= SpherePlacementRules.MaxSameSphereCopies) return 1f;

            var copyIndex = 0;
            var found = false;
            for (var i = 0; i < _spheres.Count; i++)
            {
                var candidate = _spheres[i];
                if (candidate == null || !candidate.gameObject.activeInHierarchy || candidate.Type != sphere.Type) continue;
                if (candidate == sphere) { found = true; break; }
                copyIndex++;
            }
            if (!found) return 1f;
            return SpherePlacementRules.GetCopyDamageMultiplier(
                _sphereCharacterId, sphere.Type, _characterMasteryLevel, copyIndex);
        }

        private bool CanPlaceSphereAt(Vector2 world, SphereAttackAgent ignoredSphere)
        {
            var spherePositions = new List<Vec2>(_spheres.Count);
            var sphereAlive = new List<bool>(_spheres.Count);
            var ignoredIndex = ignoredSphere == null ? -1 : _spheres.IndexOf(ignoredSphere);
            for (var i = 0; i < _spheres.Count; i++)
            {
                var sphere = _spheres[i];
                var alive = sphere != null && sphere.gameObject.activeInHierarchy;
                var position = alive ? (Vector2)sphere.transform.position : Vector2.zero;
                spherePositions.Add(new Vec2(position.x, position.y));
                sphereAlive.Add(alive);
            }

            var enemyPositions = new List<Vec2>(_enemies.Count);
            var enemyRadii = new List<float>(_enemies.Count);
            for (var i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive) continue;
                var position = (Vector2)enemy.transform.position;
                var scale = enemy.transform.lossyScale;
                var radius = 0.5f * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
                enemyPositions.Add(new Vec2(position.x, position.y));
                enemyRadii.Add(radius);
            }

            var playerPosition = (Vector2)_player.position;
            return SpherePlacementRules.CanPlace(
                new Vec2(world.x, world.y),
                new Vec2(playerPosition.x, playerPosition.y),
                spherePositions, sphereAlive, enemyPositions, enemyRadii,
                ignoredIndex, SpherePlacementRules.WorldWidth, SpherePlacementRules.WorldHeight);
        }

        private void TryPlaceOrRemoveSphereAt(Vector2 world)
        {
            var existing = FindSphereAtWorld(world, 26f * SpherePlacementRules.SourcePixelsToWorld);
            if (existing != null)
            {
                RemoveSphereAgent(existing);
                return;
            }

            if (_spheres.Count >= GetCurrentMaxSpheres()) return;
            var sameTypeCap = SpherePlacementRules.GetSameTypeCap(
                _sphereCharacterId, _selectedSphereType, _characterMasteryLevel);
            if (CountSpheresOfType(_selectedSphereType) >= sameTypeCap)
            {
                ShowMessage("MAX " + sameTypeCap);
                return;
            }
            if (!CanPlaceSphereAt(world, null))
            {
                ShowMessage("Path blocked.");
                return;
            }

            var level = _sphereLevels.TryGetValue(_selectedSphereType, out var currentLevel) ? currentLevel : 0;
            CreateSphere(_selectedSphereType, level, world);
            _placementFlashPosition = world;
            _placementFlashColor = SphereUiColor(_selectedSphereType);
            _placementFlashTimer = 0.45f;
            RefreshNetworkState();
            CaptureSphereFormationMemoryIfChanged();
            for (var i = 0; i < 15; i++) EmitSourceRadialParticle(world, 120f, .5f, 3f, "#" + ColorUtility.ToHtmlStringRGB(SphereUiColor(_selectedSphereType)));
            SourceAudio2D.Play("place");
        }

        private void RemoveSphereAgent(SphereAttackAgent sphere)
        {
            if (sphere == null) return;
            var removedType = sphere.Type;
            var removedPosition = (Vector2)sphere.transform.position;
            if (_engineerRelaySource == sphere) { _engineerRelaySource = null; _engineerRelayTimer = 0f; }
            _spheres.Remove(sphere);
            if (sphere.gameObject != null) Destroy(sphere.gameObject);
            RefreshNetworkState();
            CaptureSphereFormationMemoryIfChanged();
            for (var i = 0; i < 15; i++) EmitSourceRadialParticle(removedPosition, 120f, .5f, 3f, "#b8475a");
        }

        private void BuildSphereNetworkPreview(Vector2 world, int movingSphereIndex, bool addingSphere)
        {
            var nodes = new List<SphereNetworkNode>(_spheres.Count + (addingSphere ? 1 : 0));
            _spherePointerPreviewPositions.Clear();
            var playerIndex = -1;
            for (var i = 0; i < _spheres.Count; i++)
            {
                var sphere = _spheres[i];
                var active = sphere != null && sphere.gameObject.activeInHierarchy && !sphere.NetworkDisabled;
                var position = sphere == null ? Vector2.zero : (Vector2)sphere.transform.position;
                if (i == movingSphereIndex)
                {
                    position = world;
                    playerIndex = i;
                }
                var point = new Vec2(position.x, position.y);
                _spherePointerPreviewPositions.Add(point);
                nodes.Add(new SphereNetworkNode(point, sphere != null && sphere.gameObject.activeInHierarchy, active ? 0f : 1f));
            }
            if (addingSphere)
            {
                playerIndex = nodes.Count;
                var point = new Vec2(world.x, world.y);
                _spherePointerPreviewPositions.Add(point);
                nodes.Add(new SphereNetworkNode(point, true));
            }
            _spherePointerPreviewIndex = playerIndex;
            _spherePointerPreview = SphereNetworkRules.Analyze(nodes, SphereNetworkRules.DefaultLinkDistance);
        }

        private void DrawSpherePicker()
        {
            var panel = GetSpherePickerRect();
            var definition = GameCatalog.Spheres[(int)_selectedSphereType];
            var previousColor = GUI.color;

            // Source UI: selected type/description card above five columns of
            // SphereTypeGlyph controls. No invented abbreviations or asset-list labels.
            var descriptionHeight = GetSpherePickerDescriptionHeight();
            var descriptionWidth = Mathf.Min(panel.width, 310f);
            var descriptionRect = new Rect(panel.center.x - descriptionWidth * .5f, panel.y, descriptionWidth, descriptionHeight);
            GUI.Box(descriptionRect, GUIContent.none, _box);
            var titleStyle = new GUIStyle(_label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip
            };
            var descriptionStyle = new GUIStyle(_sphereDescriptionStyle ?? _label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                wordWrap = true,
                clipping = TextClipping.Clip
            };
            GUI.Label(new Rect(descriptionRect.x + 4f, descriptionRect.y + 1f, descriptionRect.width - 8f, 22f),
                IsRussianUi ? definition.DisplayNameRu : definition.DisplayName, titleStyle);
            GUI.Label(new Rect(descriptionRect.x + 5f, descriptionRect.y + 23f, descriptionRect.width - 10f, descriptionRect.height - 23f),
                IsRussianUi ? definition.DescriptionRu : definition.DescriptionEn, descriptionStyle);

            var buttonWidth = (panel.width - 24f) / 5f;
            const float buttonHeight = 58f;
            for (var i = 0; i < SpherePickerOrder.Length; i++)
            {
                var type = SpherePickerOrder[i];
                var column = i % 5;
                var row = i / 5;
                var rect = new Rect(panel.x + 10f + column * (buttonWidth + 3.5f),
                    panel.y + descriptionHeight + 11f + row * 64f, buttonWidth, buttonHeight);
                var color = SphereUiColor(type);
                var transparentButton = new GUIStyle(GUI.skin.button);
                transparentButton.normal.background = transparentButton.hover.background = transparentButton.active.background = transparentButton.focused.background = null;
                transparentButton.normal.textColor = transparentButton.hover.textColor = transparentButton.active.textColor = Color.white;
                if (SourceGuiButton(rect, GUIContent.none, transparentButton)) { _selectedSphereType = type; SourceAudio2D.Play("place"); }
                var center = rect.center;
                var circleRadius = Mathf.Min(rect.width, rect.height) * 0.44f;
                DrawSpriteIcon(new Rect(center.x - circleRadius, center.y - circleRadius, circleRadius * 2f, circleRadius * 2f), RuntimeSpriteFactory.Disc, new Color(0.035f, 0.075f, 0.12f, 0.96f));
                DrawSphereGuiCircle(center, circleRadius, new Color(0.24f, 0.39f, 0.52f, 0.58f), 1.1f);
                if (type == _selectedSphereType) DrawSphereGuiCircle(center, circleRadius + 2f, color, 1.8f);
                DrawSpherePickerGlyph(rect, type, color);
                var character = CharacterRules.Get(_sphereCharacterId);
                var preferredForSpherist = type == character.SignatureSphere || type == character.PartnerSphere;
                if (preferredForSpherist)
                {
                    // Port of the selected CHARACTER_DEFS preferredSphereTypes.
                    var preferredColor = new Color(1f, 0.72f, 0.30f, 0.5f);
                    GUI.color = preferredColor;
                    GUI.DrawTexture(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, 1f), _uiAccentTexture);
                    GUI.DrawTexture(new Rect(rect.x + 2f, rect.yMax - 3f, rect.width - 4f, 1f), _uiAccentTexture);
                    GUI.DrawTexture(new Rect(rect.x + 2f, rect.y + 2f, 1f, rect.height - 4f), _uiAccentTexture);
                    GUI.DrawTexture(new Rect(rect.xMax - 3f, rect.y + 2f, 1f, rect.height - 4f), _uiAccentTexture);
                    GUI.color = previousColor;
                }
                if (type == _selectedSphereType)
                {
                    GUI.color = color;
                    GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2f), _uiAccentTexture);
                    GUI.DrawTexture(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), _uiAccentTexture);
                    GUI.DrawTexture(new Rect(rect.x, rect.y, 2f, rect.height), _uiAccentTexture);
                    GUI.DrawTexture(new Rect(rect.xMax - 2f, rect.y, 2f, rect.height), _uiAccentTexture);
                    GUI.color = previousColor;
                }
            }

            GUI.color = previousColor;
        }

        private void DrawSpherePickerGlyph(Rect button, SphereId type, Color color)
        {
            var originalGuiColor = GUI.color;
            var center = button.center;
            var fill = new Color(color.r, color.g, color.b, 0.20f);
            if (type == SphereId.Sniper)
            {
                var previousMatrix = GUI.matrix;
                var previousColor = GUI.color;
                GUIUtility.RotateAroundPivot(45f, center);
                GUI.color = fill;
                GUI.DrawTexture(new Rect(center.x - 6f, center.y - 6f, 12f, 12f), _uiAccentTexture);
                GUI.matrix = previousMatrix;
                GUI.color = previousColor;
                var points = new[]
                {
                    new Vector2(center.x, center.y - 11f), new Vector2(center.x + 11f, center.y),
                    new Vector2(center.x, center.y + 11f), new Vector2(center.x - 11f, center.y)
                };
                for (var i = 0; i < points.Length; i++)
                    DrawSphereGuiLine(points[i], points[(i + 1) % points.Length], color, 2f);
                return;
            }

            if (type == SphereId.Shotgun)
            {
                var chamber = new Rect(center.x - 13f, center.y - 5f, 15f, 10f);
                GUI.color = fill;
                GUI.DrawTexture(chamber, _uiAccentTexture);
                GUI.color = color;
                DrawSphereGuiLine(new Vector2(chamber.xMin, chamber.yMin), new Vector2(chamber.xMax, chamber.yMin), color, 2f);
                DrawSphereGuiLine(new Vector2(chamber.xMax, chamber.yMin), new Vector2(chamber.xMax, chamber.yMax), color, 2f);
                DrawSphereGuiLine(new Vector2(chamber.xMax, chamber.yMax), new Vector2(chamber.xMin, chamber.yMax), color, 2f);
                DrawSphereGuiLine(new Vector2(chamber.xMin, chamber.yMax), new Vector2(chamber.xMin, chamber.yMin), color, 2f);
                DrawSphereGuiLine(new Vector2(center.x + 2f, center.y - 10f), new Vector2(center.x + 13f, center.y - 10f), color, 2f);
                DrawSphereGuiLine(new Vector2(center.x + 13f, center.y - 10f), new Vector2(center.x + 13f, center.y + 10f), color, 2f);
                DrawSphereGuiLine(new Vector2(center.x + 13f, center.y + 10f), new Vector2(center.x + 2f, center.y + 10f), color, 2f);
                GUI.color = originalGuiColor;
                return;
            }

            if (type == SphereId.Chain)
            {
                for (var i = 0; i < 3; i++)
                {
                    var node = new Vector2(center.x + (i - 1) * 14f, center.y);
                    DrawSpriteIcon(new Rect(node.x - 6f, node.y - 6f, 12f, 12f), RuntimeSpriteFactory.Disc, fill);
                    DrawSphereGuiCircle(node, 6f, color, 2f);
                }
                return;
            }

            if (type == SphereId.Aura)
            {
                DrawSphereGuiCircle(center, 15f, new Color(color.r, color.g, color.b, 0.13f), 1.5f);
                DrawSphereGuiCircle(center, 11f, color, 2f);
                return;
            }

            // Source glyphs for Standard, Orbital, Prism, Gravity, Pulse and Void
            // share the same filled circular mark.
            var defaultFill = new Color(color.r, color.g, color.b, 0.27f);
            DrawSpriteIcon(new Rect(center.x - 11f, center.y - 11f, 22f, 22f), RuntimeSpriteFactory.Disc, defaultFill);
            DrawSphereGuiCircle(center, 11f, color, 2f);
        }

        private void DrawSpherePlacementOverlay()
        {
            if (_camera == null || _uiAccentTexture == null) return;
            var previousColor = GUI.color;

            if (_spherePointerActive && _spherePointerMoved &&
                (_spherePointerTarget != null || _spherePointerIsPlacing) && _spherePointerPreview != null)
            {
                var previewType = _spherePointerTarget != null ? _spherePointerTarget.Type : _selectedSphereType;
                var previewColor = _spherePointerPositionValid ? SphereUiColor(previewType) : new Color(1f, 0.28f, 0.38f);
                var candidateIndex = _spherePointerPreviewIndex;
                for (var i = 0; i < _spherePointerPreview.Links.Count; i++)
                {
                    var link = _spherePointerPreview.Links[i];
                    if (link.A != candidateIndex && link.B != candidateIndex) continue;
                    var other = link.A == candidateIndex ? link.B : link.A;
                    if (other < 0 || other >= _spherePointerPreviewPositions.Count) continue;
                    var point = _spherePointerPreviewPositions[other];
                    DrawSphereGuiDashedLine(WorldToGuiPoint(new Vector2(point.X, point.Y)),
                        WorldToGuiPoint(_spherePointerWorld), new Color(previewColor.r, previewColor.g, previewColor.b, 0.85f), 2f, 7f, 5f);
                }

                var candidateGui = WorldToGuiPoint(_spherePointerWorld);
                var formation = _spherePointerPreview.DominantFormation;
                if (formation != null && formation.NodeIndexes.Count >= 2)
                {
                    for (var i = 0; i < formation.NodeIndexes.Count - 1; i++)
                    {
                        var fromIndex = formation.NodeIndexes[i];
                        var toIndex = formation.NodeIndexes[i + 1];
                        if (fromIndex < 0 || toIndex < 0 ||
                            fromIndex >= _spherePointerPreviewPositions.Count ||
                            toIndex >= _spherePointerPreviewPositions.Count) continue;
                        var from = _spherePointerPreviewPositions[fromIndex];
                        var to = _spherePointerPreviewPositions[toIndex];
                        DrawSphereGuiDashedLine(
                            WorldToGuiPoint(new Vector2(from.X, from.Y)),
                            WorldToGuiPoint(new Vector2(to.X, to.Y)),
                            new Color(1f, 0.72f, 0.30f, 0.82f), 2f, 6f, 5f);
                    }
                }
                var candidateFill = _spherePointerPositionValid
                    ? new Color(previewColor.r, previewColor.g, previewColor.b, 0.09f)
                    : new Color(1f, 0.30f, 0.37f, 0.12f);
                DrawSpriteIcon(new Rect(candidateGui.x - 25f, candidateGui.y - 25f, 50f, 50f),
                    RuntimeSpriteFactory.Disc, candidateFill);
                DrawSphereGuiDashedCircle(candidateGui, 25f,
                    new Color(previewColor.r, previewColor.g, previewColor.b, 1f), 2f, 5f, 4f);
                var formationName = GetSphereFormationDisplayName(formation);
                var status = _spherePointerPositionValid ? formationName : "INVALID";
                var style = new GUIStyle(_label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                GUI.color = _spherePointerPositionValid
                    ? new Color(0.86f, 0.92f, 1f)
                    : new Color(1f, 0.48f, 0.52f);
                GUI.Label(new Rect(candidateGui.x - 80f, candidateGui.y - 31f, 160f, 24f), status, style);
            }

            if (_formationMemoryTimer > 0f && _formationMemoryPoints != null && _formationMemoryPoints.Length >= 2)
            {
                var alpha = Mathf.Clamp01(_formationMemoryTimer / 0.9f) * 0.55f;
                var memoryColor = new Color(0.72f, 0.83f, 0.94f, alpha);
                for (var i = 0; i < _formationMemoryPoints.Length - 1; i++)
                    DrawSphereGuiDashedLine(WorldToGuiPoint(_formationMemoryPoints[i]),
                        WorldToGuiPoint(_formationMemoryPoints[i + 1]), memoryColor, 1.5f, 4f, 6f);
                if (_formationMemoryType != SphereNetworkFormation.Line && _formationMemoryPoints.Length > 2)
                    DrawSphereGuiDashedLine(WorldToGuiPoint(_formationMemoryPoints[_formationMemoryPoints.Length - 1]),
                        WorldToGuiPoint(_formationMemoryPoints[0]), memoryColor, 1.5f, 4f, 6f);
                for (var i = 0; i < _formationMemoryPoints.Length; i++)
                    DrawSphereGuiCircle(WorldToGuiPoint(_formationMemoryPoints[i]), 9f, memoryColor, 1.5f);
            }

            DrawResonanceGeometryMemoryOverlay();

            if (_placementFlashTimer > 0f)
            {
                var ratio = Mathf.Clamp01(_placementFlashTimer / 0.45f);
                var radius = 18f + (1f - ratio) * 25f;
                var placementPoint = WorldToGuiPoint(_placementFlashPosition);
                DrawSphereGuiCircle(placementPoint, radius,
                    new Color(_placementFlashColor.r, _placementFlashColor.g, _placementFlashColor.b, ratio * 0.8f), 2f);
                DrawSphereGuiCircle(placementPoint, 17f,
                    new Color(_placementFlashColor.r, _placementFlashColor.g, _placementFlashColor.b, ratio * 0.55f), 1f);
            }

            DrawTriangleResonanceOverlay();
            GUI.color = previousColor;
        }

        private void DrawTriangleResonanceOverlay()
        {
            if (_networkState == null || _camera == null || _uiAccentTexture == null) return;

            SphereNetworkShape triangle = null;
            for (var i = 0; i < _networkState.FormationCandidates.Count; i++)
            {
                var candidate = _networkState.FormationCandidates[i];
                if (candidate == null || candidate.Type != SphereNetworkFormation.Triangle) continue;
                triangle = candidate;
                break;
            }
            if (triangle == null || triangle.NodeIndexes == null || triangle.NodeIndexes.Count < 3) return;

            var indexes = new int[3];
            var points = new Vector2[3];
            for (var i = 0; i < 3; i++)
            {
                var index = triangle.NodeIndexes[i];
                if (index < 0 || index >= _spheres.Count ||
                    _spheres[index] == null || !_spheres[index].gameObject.activeInHierarchy) return;
                indexes[i] = index;
                points[i] = WorldToGuiPoint(_spheres[index].transform.position);
            }

            var time = Time.unscaledTime;
            var formationAlpha = triangle.Active ? 1f : 0.3f;
            var fillAlpha = (0.028f + 0.018f * Mathf.Sin(time * 3f)) * formationAlpha;
            DrawSphereGuiTriangleFill(points[0], points[1], points[2],
                new Color(1f, 184f / 255f, 77f / 255f, fillAlpha));

            var edgeAlpha = (0.24f + 0.08f * Mathf.Sin(time * 3f)) * formationAlpha;
            var edgeColor = new Color(1f, 207f / 255f, 115f / 255f, edgeAlpha);
            DrawSphereGuiLine(points[0], points[1], edgeColor, 1f);
            DrawSphereGuiLine(points[1], points[2], edgeColor, 1f);
            DrawSphereGuiLine(points[2], points[0], edgeColor, 1f);

            var center = Vector2.zero;
            var activePulse = false;
            for (var i = 0; i < 3; i++)
            {
                var sphereIndex = indexes[i];
                var sphere = _spheres[sphereIndex];
                var point = points[i];
                center += point;

                var radius = 25f + Mathf.Sin(time * 5f + sphereIndex) * 1.5f;
                DrawSphereGuiCircle(point, radius,
                    new Color(1f, 184f / 255f, 77f / 255f, 0.20f * formationAlpha), 1.4f);

                var charge = sphere.FormationHitCount % 3;
                if (charge > 0)
                {
                    var startAngle = -Mathf.PI * 0.5f;
                    var endAngle = startAngle + Mathf.PI * 2f * (charge / 3f);
                    DrawSphereGuiArc(point, radius, startAngle, endAngle,
                        new Color(1f, 184f / 255f, 77f / 255f, 0.28f * formationAlpha), 3.8f);
                    DrawSphereGuiArc(point, radius, startAngle, endAngle,
                        new Color(1f, 184f / 255f, 77f / 255f, 0.86f * formationAlpha), 1.4f);
                }

                for (var tick = 0; tick < 3; tick++)
                {
                    var angle = -Mathf.PI * 0.5f + tick / 3f * Mathf.PI * 2f;
                    var inner = radius + 2f;
                    var outer = inner + (tick < charge ? 5f : 2f);
                    var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    var alpha = tick < charge ? 0.95f : 0.30f;
                    DrawSphereGuiLine(point + direction * inner, point + direction * outer,
                        new Color(1f, 184f / 255f, 77f / 255f, alpha * formationAlpha), 1.4f);
                }

                var pulseTimer = sphere.ResonancePulseTimer;
                if (pulseTimer > 0f)
                {
                    activePulse = true;
                    var flash = pulseTimer / 0.45f;
                    DrawSphereGuiCircle(point, radius + 5f + (1f - flash) * 10f,
                        new Color(1f, 240f / 255f, 194f / 255f,
                            (0.45f + 0.4f * flash) * formationAlpha), 2.6f);
                }
            }
            center /= 3f;
            var corePulse = 1f + (activePulse
                ? 0.28f + 0.08f * Mathf.Sin(time * 24f)
                : 0.04f * Mathf.Sin(time * 6f));
            DrawSphereGuiFilledCircle(center, (activePulse ? 4.5f : 2.5f) * corePulse,
                new Color(1f, 184f / 255f, 77f / 255f,
                    (activePulse ? 0.90f : 0.26f) * formationAlpha));
        }

        private void DrawSphereGuiArc(
            Vector2 center, float radius, float startAngle, float endAngle, Color color, float width)
        {
            if (radius <= 0f || Mathf.Approximately(startAngle, endAngle)) return;
            var segmentCount = Mathf.Max(2, Mathf.CeilToInt(Mathf.Abs(endAngle - startAngle) * radius / 3f));
            var previous = center + new Vector2(Mathf.Cos(startAngle), Mathf.Sin(startAngle)) * radius;
            for (var i = 1; i <= segmentCount; i++)
            {
                var angle = Mathf.Lerp(startAngle, endAngle, i / (float)segmentCount);
                var next = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                DrawSphereGuiLine(previous, next, color, width);
                previous = next;
            }
        }

        private void DrawSphereGuiTriangleFill(Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            var minY = Mathf.Min(a.y, Mathf.Min(b.y, c.y));
            var maxY = Mathf.Max(a.y, Mathf.Max(b.y, c.y));
            for (var y = minY; y <= maxY; y += 4f)
            {
                var count = 0;
                var x0 = 0f;
                var x1 = 0f;
                var x2 = 0f;
                AddTriangleGuiIntersection(a, b, y, ref count, ref x0, ref x1, ref x2);
                AddTriangleGuiIntersection(b, c, y, ref count, ref x0, ref x1, ref x2);
                AddTriangleGuiIntersection(c, a, y, ref count, ref x0, ref x1, ref x2);
                if (count < 2) continue;

                float left;
                float right;
                if (count == 2)
                {
                    left = Mathf.Min(x0, x1);
                    right = Mathf.Max(x0, x1);
                }
                else
                {
                    left = Mathf.Min(x0, Mathf.Min(x1, x2));
                    right = Mathf.Max(x0, Mathf.Max(x1, x2));
                }
                DrawSphereGuiLine(new Vector2(left, y), new Vector2(right, y), color, 4.2f);
            }
        }

        private static void AddTriangleGuiIntersection(
            Vector2 from, Vector2 to, float y, ref int count, ref float x0, ref float x1, ref float x2)
        {
            if (!((from.y <= y && to.y > y) || (to.y <= y && from.y > y))) return;
            var x = from.x + (y - from.y) * (to.x - from.x) / (to.y - from.y);
            if (count == 0) x0 = x;
            else if (count == 1) x1 = x;
            else if (count == 2) x2 = x;
            count++;
        }

        private void DrawSphereGuiFilledCircle(Vector2 center, float radius, Color color)
        {
            for (var y = -radius; y <= radius; y += 1f)
            {
                var halfWidth = Mathf.Sqrt(Mathf.Max(0f, radius * radius - y * y));
                DrawSphereGuiLine(new Vector2(center.x - halfWidth, center.y + y),
                    new Vector2(center.x + halfWidth, center.y + y), color, 1.2f);
            }
        }

        private void DrawResonanceGeometryMemoryOverlay()
        {
            if (_resonanceGeometryMemoryTimer <= 0f || _resonanceGeometryMemoryPoints.Count < 2) return;
            var alpha = Mathf.Clamp01(
                _resonanceGeometryMemoryTimer / ResonanceGeometryMemoryRules.MemoryDurationSeconds);
            Color memoryColor;
            switch (_resonanceGeometryMemoryType)
            {
                case SphereNetworkFormation.Triangle:
                    memoryColor = new Color(1f, 184f / 255f, 77f / 255f);
                    break;
                case SphereNetworkFormation.Square:
                    memoryColor = new Color(105f / 255f, 183f / 255f, 1f);
                    break;
                case SphereNetworkFormation.Cluster:
                    memoryColor = new Color(179f / 255f, 140f / 255f, 1f);
                    break;
                default:
                    memoryColor = new Color(99f / 255f, 230f / 255f, 1f);
                    break;
            }

            var lineColor = new Color(memoryColor.r, memoryColor.g, memoryColor.b, 0.34f * alpha);
            var screenPoints = new List<Vector2>(_resonanceGeometryMemoryPoints.Count);
            for (var i = 0; i < _resonanceGeometryMemoryPoints.Count; i++)
                screenPoints.Add(WorldToGuiPoint(_resonanceGeometryMemoryPoints[i]));

            for (var i = 0; i < screenPoints.Count - 1; i++)
                DrawSphereGuiDashedLine(screenPoints[i], screenPoints[i + 1], lineColor, 1.4f, 5f, 7f);
            if ((_resonanceGeometryMemoryType == SphereNetworkFormation.Triangle ||
                _resonanceGeometryMemoryType == SphereNetworkFormation.Square) && screenPoints.Count > 2)
                DrawSphereGuiDashedLine(screenPoints[screenPoints.Count - 1], screenPoints[0],
                    lineColor, 1.4f, 5f, 7f);

            var nodeColor = new Color(memoryColor.r, memoryColor.g, memoryColor.b, 0.22f * alpha);
            var pulseRadius = 10f + Mathf.Sin(_runTime * 7f) * 1.5f;
            for (var i = 0; i < screenPoints.Count; i++)
                DrawSphereGuiCircle(screenPoints[i], pulseRadius, nodeColor, 1.2f);
        }

        private static string GetSphereFormationDisplayName(SphereNetworkShape formation)
        {
            if (formation == null) return "NETWORK";
            switch (formation.Type)
            {
                case SphereNetworkFormation.Line: return "LINE";
                case SphereNetworkFormation.Triangle: return "TRIANGLE";
                case SphereNetworkFormation.Square: return "SQUARE";
                case SphereNetworkFormation.Cluster: return "CLUSTER";
                case SphereNetworkFormation.Ring: return "RING";
                case SphereNetworkFormation.Lattice: return "LATTICE";
                case SphereNetworkFormation.Fractal: return "FRACTAL";
                default: return "NETWORK";
            }
        }

        private void DrawSphereGuiDashedCircle(Vector2 center, float radius, Color color, float width, float dashLength, float gapLength)
        {
            var circumference = Mathf.PI * 2f * radius;
            for (var offset = 0f; offset < circumference; offset += dashLength + gapLength)
            {
                var endOffset = Mathf.Min(circumference, offset + dashLength);
                var startAngle = offset / radius;
                var endAngle = endOffset / radius;
                const int arcSegments = 3;
                var previous = center + new Vector2(Mathf.Cos(startAngle), Mathf.Sin(startAngle)) * radius;
                for (var segment = 1; segment <= arcSegments; segment++)
                {
                    var angle = Mathf.Lerp(startAngle, endAngle, segment / (float)arcSegments);
                    var next = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    DrawSphereGuiLine(previous, next, color, width);
                    previous = next;
                }
            }
        }

        private void DrawSphereGuiDashedLine(Vector2 start, Vector2 end, Color color, float width, float dashLength, float gapLength)
        {
            var delta = end - start;
            var length = delta.magnitude;
            if (length < 0.1f) return;
            var direction = delta / length;
            var cycle = Mathf.Max(0.1f, dashLength + gapLength);
            for (var offset = 0f; offset < length; offset += cycle)
            {
                var segmentEnd = Mathf.Min(length, offset + dashLength);
                DrawSphereGuiLine(start + direction * offset, start + direction * segmentEnd, color, width);
            }
        }

        private void DrawSphereGuiCircle(Vector2 center, float radius, Color color, float width)
        {
            const int segments = 28;
            var previous = center + new Vector2(radius, 0f);
            for (var i = 1; i <= segments; i++)
            {
                var angle = Mathf.PI * 2f * i / segments;
                var next = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                DrawSphereGuiLine(previous, next, color, width);
                previous = next;
            }
        }

        private void DrawSphereGuiLine(Vector2 start, Vector2 end, Color color, float width)
        {
            var delta = end - start;
            var length = delta.magnitude;
            if (length < 0.1f) return;
            var previousMatrix = GUI.matrix;
            var previousColor = GUI.color;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, start);
            GUI.color = color;
            GUI.DrawTexture(new Rect(start.x, start.y - width * 0.5f, length, width), _uiAccentTexture);
            GUI.matrix = previousMatrix;
            GUI.color = previousColor;
        }


        private void TickRegionHudState(float dt)
        {
            _regionPocketNoticeTimer = Mathf.Max(0f, _regionPocketNoticeTimer - dt);
            if (_player == null) return;

            var sourceX = _player.position.x / (12.6f / 720f);
            var sourceY = -_player.position.y / (12.6f / 720f);
            var pocketNames = _selectedRegion == EchoRegionId.SpectralRift
                ? new[] { "prism", "shoal", "ember", "silent", "null" }
                : new[] { "axis", "glass", "fracture", "hollow", "pressure" };
            var pocketLabelsRu = _selectedRegion == EchoRegionId.SpectralRift
                ? new[] { "ПРИЗМАТИЧЕСКИЙ СДВИГ", "ЗЕРКАЛЬНАЯ МЕЛЬ", "ТЛЕЮЩИЙ ИЗЛОМ", "ТИХАЯ РЕШЁТКА", "НУЛЕВОЙ САД" }
                : new[] { "ОСЕВОЙ УЗЕЛ", "СТЕКЛЯННЫЙ ПОТОК", "РАЗЛОМ", "ПУСТОЙ КОНТУР", "ЗОНА ДАВЛЕНИЯ" };
            var pocketLabelsEn = _selectedRegion == EchoRegionId.SpectralRift
                ? new[] { "PRISM SHIFT", "MIRROR SHOAL", "EMBER FAULT", "SILENT LATTICE", "NULL GARDEN" }
                : new[] { "AXIS NODE", "GLASS FLOW", "FRACTURE", "HOLLOW CONTOUR", "PRESSURE FIELD" };
            var pocketX = _selectedRegion == EchoRegionId.SpectralRift
                ? new[] { 0f, 520f, 470f, -500f, -520f } : new[] { 0f, 520f, 500f, -480f, -520f };
            var pocketY = _selectedRegion == EchoRegionId.SpectralRift
                ? new[] { 0f, -270f, 450f, 430f, -290f } : new[] { 0f, -260f, 430f, 430f, -300f };
            var closest = 0;
            var closestDistance = float.MaxValue;
            for (var i = 0; i < pocketNames.Length; i++)
            {
                var dx = sourceX - pocketX[i];
                var dy = sourceY - pocketY[i];
                var distance = dx * dx + dy * dy;
                if (distance < closestDistance) { closestDistance = distance; closest = i; }
            }

            if (_tutorialStep < 0 && !_regionStabilized && _runTime > 1f && _regionHudPocketId != pocketNames[closest])
            {
                EnsureKnowledgeLoaded();
                var regionId = _selectedRegion == EchoRegionId.SpectralRift ? "spectral_rift" : "resonance_basin";
                var pocket = PlannerEntry("region-pocket", regionId + "|" + pocketNames[closest]);
                _regionPocketNotice = PlannerDesc(pocket); _regionPocketNoticeTimer = 1.35f;
                ShowTimedCombatMessage(_regionPocketNotice, 1.35f, pocket?.color ?? "#39d8ff", "boundary");
            }
            _regionHudPocketId = pocketNames[closest];
        }

        private string GetRegionPhaseLabel()
        {
            if (_runTime < 300f) return IsRussianUi ? "ОРИЕНТАЦИЯ" : "ORIENTATION";
            if (_runTime < 600f) return IsRussianUi ? "ПЕРВЫЙ РЕЗОНАНС" : "FIRST RESONANCE";
            if (_runTime < 900f) return IsRussianUi ? "СПЕЦИАЛИЗАЦИЯ" : "SPECIALIZATION";
            if (_runTime < 1500f) return IsRussianUi ? "ЭСКАЛАЦИЯ" : "ESCALATION";
            if (_runTime < 1620f) return IsRussianUi ? "СХОДИМОСТЬ" : "CONVERGENCE";
            return IsRussianUi ? "СТАБИЛИЗАЦИЯ" : "STABILIZATION";
        }

        private void DrawPreviousMigrationInterface()
        {
            var previousMatrix = GUI.matrix;
            _uiScale = SourceDevicePixelRatio;
            _uiWidth = Screen.width / _uiScale;
            _uiHeight = Screen.height / _uiScale;
            GUI.matrix = previousMatrix * Matrix4x4.Scale(new Vector3(_uiScale, _uiScale, 1f));

            try
            {
                if (_box == null)
                {
                    _uiPanelTexture = CreateUiTexture(new Color(0.012f, 0.032f, 0.082f, 0.96f));
                    _uiButtonTexture = CreateUiTexture(new Color(0.025f, 0.12f, 0.23f, 0.98f));
                    _uiButtonHoverTexture = CreateUiTexture(new Color(0.04f, 0.24f, 0.39f, 1f));
                    _uiButtonActiveTexture = CreateUiTexture(new Color(0.04f, 0.38f, 0.55f, 1f));
                    _uiAccentTexture = CreateUiTexture(Color.white);
                    _box = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 15, padding = new RectOffset(14, 14, 10, 10) };
                    _box.normal.background = _uiPanelTexture;
                    _box.border = new RectOffset(5, 5, 5, 5);
                    _label = new GUIStyle(GUI.skin.label) { fontSize = 15, normal = { textColor = new Color(0.72f, 0.88f, 1f) } };
                    _sphereDescriptionStyle = new GUIStyle(_label) { fontSize = 11, wordWrap = true, clipping = TextClipping.Clip };
                    _button = new GUIStyle(GUI.skin.button) { fontSize = 14, wordWrap = true, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                    _button.normal.background = _uiButtonTexture;
                    _button.hover.background = _uiButtonHoverTexture;
                    _button.active.background = _uiButtonActiveTexture;
                    _button.normal.textColor = new Color(0.86f, 0.97f, 1f);
                    _button.hover.textColor = Color.white;
                    _button.active.textColor = Color.white;
                    _title = new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
                    _menuTitle = new GUIStyle(_title) { fontSize = Mathf.Clamp(Mathf.RoundToInt(_uiWidth / 23f), 30, 44), alignment = TextAnchor.MiddleCenter };
                    _comboTitleStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 24,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = Color.white }
                    };
                    _comboMultiplierStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = new Color(0.7137255f, 0.7882353f, 0.87058824f, 1f) }
                    };
                }

                if (_mainMenu)
                {
                    if (_regionSelect) DrawRegionSelect();
                    else DrawMainMenu();
                    return;
                }

                var hudEnabled = GUI.enabled;
                GUI.enabled = !_userPaused && !_levelUp && !_stellaRewardPending && !_gameOver && !_artifactPanelOpen;
                DrawSourceGameplayHud();
                GUI.enabled = hudEnabled;

                // Port of src/App.tsx's combo overlay. The first HUD row, roster,
                // pause controls and transient message occupy the space above this
                // compact center-aligned position; width follows the portrait viewport.
                if (_combo >= 5)
                {
                    var comboWidth = Mathf.Min(240f, Mathf.Max(1f, _uiWidth - 36f));
                    var comboLeft = (_uiWidth - comboWidth) * 0.5f;
                    const float comboTop = 64f;
                    var comboColor = _combo >= 50
                        ? new Color(1f, 0.72156864f, 0.3019608f, 1f)
                        : _combo >= 25
                            ? new Color(1f, 0.41960785f, 0.41960785f, 1f)
                            : new Color(1f, 0.3019608f, 0.3647059f, 1f);
                    var comboTitle = (IsRussianUi ? "КОМБО" : "COMBO") + " x" + _combo;
                    var previousComboGuiColor = GUI.color;
                    GUI.color = comboColor;
                    GUI.Label(new Rect(comboLeft, comboTop, comboWidth, 30f), comboTitle, _comboTitleStyle);
                    GUI.color = Color.white;
                    GUI.Label(new Rect(comboLeft, comboTop + 27f, comboWidth, 20f),
                        "x" + _comboMultiplier.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                        _comboMultiplierStyle);

                    var comboBarWidth = Mathf.Min(96f, comboWidth);
                    var comboBar = new Rect((_uiWidth - comboBarWidth) * 0.5f, comboTop + 51f, comboBarWidth, 4f);
                    GUI.color = new Color(0.14117648f, 0.23137255f, 0.33333334f, 1f);
                    GUI.DrawTexture(comboBar, _uiAccentTexture);
                    var comboFillWidth = comboBar.width * Mathf.Clamp01(_comboTimer / ComboRules.WindowSeconds);
                    if (comboFillWidth > 0f)
                    {
                        GUI.color = new Color(1f, 0.72156864f, 0.3019608f, 1f);
                        GUI.DrawTexture(new Rect(comboBar.x, comboBar.y, comboFillWidth, comboBar.height), _uiAccentTexture);
                    }
                    GUI.color = previousComboGuiColor;
                }

                if (_levelUp) DrawSourceUpgradePicker();

                if (_stellaRewardPending) DrawSourceArtifactChoices();

                if (_userPaused && !_artifactPanelOpen) DrawSourcePausePlanner();

                if (_gameOver) DrawSourceRunResult();
                DrawTutorialOverlay();

            }
            finally
            {
                GUI.matrix = previousMatrix;
            }
        }

        private void SetTutorialStep(int step)
        {
            _tutorialStep = Mathf.Clamp(step, 0, 5);
            if (_tutorialStep == 1 || _tutorialStep == 2)
            {
                _paused = false;
                _userPaused = false;
                return;
            }

            _paused = true;
            _userPaused = false;
            if (_tutorialStep == 4 && !_tutorialUpgradeOpened)
            {
                _tutorialUpgradeOpened = true;
                _level++;
                _playerCore?.IncreaseMaxHp(6f);
                _xp = 0f;
                _xpRequired = RunBalanceRules.GetXpToNextLevel(_level);
                BuildLevelUpChoices();
                SourceAudio2D.Play("levelup");
                if (_levelUpChoices.Count > 0)
                {
                    _levelUp = true;
                    _paused = true;
                }
                else
                {
                    SetTutorialStep(5);
                }
            }
        }

        private void FinishTutorial(bool skipped)
        {
            _tutorialStep = -1;
            _levelUp = false;
            _levelUpChoices.Clear();
            _paused = false;
            _userPaused = false;
            _wave = 0; _waveEnemiesToSpawn = 0; _runTime = 0f;
            _spawnTimer = 3f;
            PlayerPrefs.SetInt(TutorialCompletedPreferenceKey, 1);
            PlayerPrefs.Save();
            if (_player != null) _tutorialStartPosition = _player.position;
        }

        private void DrawTutorialOverlay()
        {
            if (_tutorialStep < 0 || _mainMenu || _gameOver) return;

            DrawSourceTutorialSpotlight();
            var panel = SourceTutorialCard;
            SourceUiRect(panel, new Color(3f/255f,10f/255f,20f/255f,.95f));
            var title = IsRussianUi
                ? new[] { "Добро пожаловать в Эхо", "Управляй ядром", "Размести сферу", "Смотри на Сеть", "Первый выбор", "Теперь ты у руля" }[_tutorialStep]
                : new[] { "Welcome to Echo", "Control the Core", "Place a Sphere", "Read the Network", "First Choice", "You are in control" }[_tutorialStep];
            var body = IsRussianUi
                ? new[] {
                    "Это короткое обучение покажет главное: движение, размещение сфер, Сеть и первый выбор улучшения.",
                    "Двигайся джойстиком. Положение ядра влияет на то, где безопасно строить Сеть.",
                    "Тапни по свободному месту на поле. Сфера станет новым узлом твоей боевой Сети.",
                    "Линии между сферами меняют доступные формации. Геометрия становится частью боя, а не украшением.",
                    "Выбери любое улучшение. Так каждый забег начинает собирать собственный билд.",
                    "Дальше начинается настоящий забег. Строй сеть, развивай сферы и ищи сильные синергии."
                }[_tutorialStep]
                : new[] {
                    "This short tutorial teaches the essentials: movement, sphere placement, the Network, and your first upgrade choice.",
                    "Move with the joystick. Your position affects where you can safely build the Network.",
                    "Tap an open spot on the field. The Sphere becomes a new node in your combat Network.",
                    "Links between Spheres change the available formations. Geometry is part of combat, not decoration.",
                    "Choose any upgrade. Each run starts building its own identity from your choices.",
                    "The real run starts now. Build the Network, evolve Spheres, and discover powerful synergies."
                }[_tutorialStep];

            SourceUiLabel(new Rect(panel.x+16f,panel.y+16f,panel.width-32f,12f), (IsRussianUi ? "ОБУЧЕНИЕ · " : "TUTORIAL · ")+(_tutorialStep+1)+"/6",9,"#63e6ff",false,true);
            SourceUiLabel(new Rect(panel.x+16f,panel.y+29f,panel.width-32f,26f),title,18,"#dcecff",false,true);
            SourceUiLabel(new Rect(panel.x+16f,panel.y+63f,panel.width-32f,58f),body,12,"#a9c0d2");
            var ready = _tutorialStep != 1 && _tutorialStep != 2 || (_tutorialStep == 1 ? Vector2.Distance(_tutorialStartPosition,_player.position)>=72f*SpherePlacementRules.SourcePixelsToWorld : _spheres.Count>=2);
            if (_tutorialStep == 1 || _tutorialStep == 2 || _tutorialStep == 4)
            {
                var action = _tutorialStep == 4 ? (IsRussianUi ? "Выбери одну карту. Обучение продолжится после выбора." : "Choose one card. The tutorial continues after your choice.")
                    : ready ? (IsRussianUi ? _tutorialStep == 1 ? "Отлично. Движение освоено." : "Сфера установлена." : _tutorialStep == 1 ? "Great. Movement learned." : "Sphere placed.")
                    : IsRussianUi ? "Выполни действие, чтобы продолжить." : "Perform the action to continue.";
                var actionRect = new Rect(panel.x+16f,panel.y+133f,panel.width-32f,34f);
                SourceUiRect(actionRect, _tutorialStep == 4 ? new Color(.71f,.55f,1f,.10f) : ready ? new Color(.33f,.90f,.76f,.10f) : new Color32(9,18,31,255));
                SourceUiLabel(new Rect(actionRect.x+12f,actionRect.y+8f,actionRect.width-24f,25f),action,10,_tutorialStep == 4 ? "#dbcfff" : ready ? "#b9ffe8" : "#7089a0",false,true);
            }
            if (SourceUiButton(new Rect(panel.x+16f,panel.yMax-51f,110f,35f),IsRussianUi ? "ПРОПУСТИТЬ" : "SKIP",9,Color.clear,"#718aa1")) { FinishTutorial(true); return; }
            if (_tutorialStep != 4)
            {
                var enabled = GUI.enabled; GUI.enabled = ready;
                if (SourceUiButton(new Rect(panel.xMax-116f,panel.yMax-51f,100f,35f),IsRussianUi ? "ДАЛЕЕ" : "NEXT",10,new Color(.39f,.90f,1f,.10f),"#dffaff"))
                { if (_tutorialStep < 5) SetTutorialStep(_tutorialStep+1); else FinishTutorial(false); }
                GUI.enabled = enabled;
            }
        }

        private void DrawSphereRoster()
        {
            var count = Mathf.Min(_spheres.Count, 10);
            if (count <= 0) return;
            const float cardWidth = 56f;
            var panelWidth = count * cardWidth + 12f;
            var panel = new Rect(_uiWidth - panelWidth - 18f, 18f, panelWidth, 110f);
            GUI.Box(panel, GUIContent.none, _box);
            GUI.Label(new Rect(panel.x + 8f, panel.y + 4f, panel.width - 16f, 18f), "ACTIVE SPHERES", _label);
            for (var i = 0; i < count; i++)
            {
                var sphere = _spheres[i];
                if (sphere == null) continue;
                var x = panel.x + 6f + i * cardWidth;
                DrawSpriteIcon(new Rect(x + 13f, panel.y + 24f, 30f, 30f), GetSphereIcon(sphere.Type), SphereColor(sphere.Type));
                GUI.Label(new Rect(x + 1f, panel.y + 56f, cardWidth - 2f, 16f), SphereShortName(sphere.Type), _label);
                GUI.Label(new Rect(x + 1f, panel.y + 74f, cardWidth - 2f, 16f), "LV " + RomanLevel(sphere.ProgressionLevel), _label);
            }
        }

        private static string RomanLevel(int level)
        {
            switch (Mathf.Clamp(level, 1, 7))
            {
                case 1: return "I";
                case 2: return "II";
                case 3: return "III";
                case 4: return "IV";
                case 5: return "V";
                case 6: return "VI";
                default: return "VII";
            }
        }

        private static string FormatTime(float seconds)
        {
            var whole = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return (whole / 60).ToString("00") + ":" + (whole % 60).ToString("00");
        }
    }
}
