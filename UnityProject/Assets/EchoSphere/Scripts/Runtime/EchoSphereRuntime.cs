using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoSphere.Runtime
{
    /// <summary>First-playable migration harness; intentionally not the full production game.</summary>
    public sealed class EchoSphereRuntime : MonoBehaviour
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

        private enum MainMenuPage { Home, Shop, Settings, Leaderboard, Achievements, Characters }

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
        private const string SoundPreferenceKey = "echosphere_sound";
        private const string HandednessPreferenceKey = "echosphere_handedness";
        private ShopSaveData _shopSave;
        private bool _shopSaveLoaded;
        private MainMenuPage _mainMenuPage = MainMenuPage.Home;
        private string _playerName = "";
        private bool _settingsResetPending;
        private Vector2 _characterScrollPosition;
        private string _expandedCharacterId = "spherist";
        private List<CharacterProfileData> _characterProfiles = new List<CharacterProfileData>();
        private bool _characterProfilesLoaded;
        private bool _runResultSaved;
        private int _runGoldEarned;
        private int _runEliteKills;
        private int _runEvolutions;
        private int _runChestOpens;
        private List<LeaderboardEntry> _leaderboardEntries = new List<LeaderboardEntry>();
        private List<string> _unlockedAchievementIds = new List<string>();

        // Exact picker order from Object.keys(SPHERE_TYPES) in new-desing/src/gameData.ts.
        private static readonly SphereId[] SpherePickerOrder =
        {
            SphereId.Standard, SphereId.Sniper, SphereId.Shotgun, SphereId.Chain, SphereId.Aura,
            SphereId.Orbital, SphereId.Prism, SphereId.Gravity, SphereId.Pulse, SphereId.Void
        };

        private readonly List<EnemyAgent2D> _enemies = new List<EnemyAgent2D>();
        private readonly List<SphereAttackAgent> _spheres = new List<SphereAttackAgent>();
        private readonly List<string> _ownedArtifacts = new List<string>();
        private readonly List<ArtifactRewardDefinition> _stellaRewardChoices = new List<ArtifactRewardDefinition>();
        private readonly HashSet<SphereId> _artifactSphereTypeScratch = new HashSet<SphereId>();
        private readonly Dictionary<SphereId, int> _sphereLevels = new Dictionary<SphereId, int>();
        private readonly List<RunUpgradeChoice> _levelUpChoices = new List<RunUpgradeChoice>();
        private readonly Dictionary<RunUpgradeKind, int> _abilityLevels = new Dictionary<RunUpgradeKind, int>();
        private readonly List<string> _recentUpgradeChoiceKeys = new List<string>();
        private int _levelUpAbilityPity, _levelUpSpherePity;
        private readonly Dictionary<SphereId, string> _sphereBranches = new Dictionary<SphereId, string>();
        private readonly Dictionary<SphereId, string> _sphereFinals = new Dictionary<SphereId, string>();
        private readonly List<SphereEvolutionOption> _evolutionChoices = new List<SphereEvolutionOption>();
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
        private Vector2 _placementFlashPosition;
        private Color _placementFlashColor = Color.white;
        private float _spawnTimer = 3f, _runTime, _messageTimer, _formationStrain;
        private Vec2 _lastDirection = Vec2.Zero;
        private int _kills, _level = 1, _xp, _xpRequired = RunBalanceRules.GetXpToNextLevel(1);
        private int _bossesDefeated, _wave, _waveEnemiesToSpawn;
        private bool _follow, _paused, _levelUp, _gameOver, _userPaused, _regionStabilized;
        private bool _stellaRewardPending, _stellaRewardIsLegendary;
        private float _swiftBootsTimer;
        private float _alchemistCatalystTimer;
        private int _alchemistReactionCount;
        private RegionBossScheduleState _regionSchedule;
        private RegionFieldVisuals _regionFieldVisuals;
        private bool _mainMenu = true;
        private bool _regionSelect;
        private EchoRegionId _selectedRegion = EchoRegionId.ResonanceBasin;
        private const string BasinStabilizedPreferenceKey = "echo-sphere:region:resonance_basin:stabilized";
        private const string SelectedRegionPreferenceKey = "echo-sphere:region:selected";
        private float _uiScale = 1f, _uiWidth, _uiHeight;
        private Texture2D _uiPanelTexture, _uiButtonTexture, _uiButtonHoverTexture, _uiButtonActiveTexture, _uiAccentTexture;
        private GUIStyle _menuTitle;
        private float _damageMultiplier = 1f, _attackSpeedMultiplier = 1f;
        private float _resonanceCharge, _resonanceRingTimer, _resonanceRingPulseTimer;
        private int _resonanceEventsTriggered, _resonanceRingCursor, _resonanceLineBurst;
        private bool _resonanceEventActive;
        private readonly HashSet<string> _resonanceKnownFormationKeys = new HashSet<string>();
        private readonly List<int> _resonanceRingNodes = new List<int>();
        private string _message = "RIGHT HALF: MOVE. LEFT HALF: PLACE / DRAG SPHERES. TAP SPHERE: REMOVE.";
        private GUIStyle _box, _label, _button, _title, _sphereDescriptionStyle;
        private readonly List<LineRenderer> _networkLinkRenderers = new List<LineRenderer>();
        private Material _networkLineMaterial;
        private SphereNetworkState _networkState;
        private SphereNetworkFormation _previousDominantFormation = SphereNetworkFormation.None;
        private SphereNetworkFormation _preferredDominantFormation = SphereNetworkFormation.None;
        private SphereNetworkFormation _preferredSecondaryFormation = SphereNetworkFormation.None;
        private float _networkRefreshTimer;

        public bool IsGameplayPaused => _paused;
        public bool IsPresentationPaused => _mainMenu || _paused;
        public Vector2 PlayerPosition => _player == null ? Vector2.zero : (Vector2)_player.position;
        public float CurrentRunTime => _runTime;
        public int CurrentWave => _wave;
        public float GetEnemyAbilitySpeedMultiplier() => LevelUpAbilityRules.GetEnemySlowMultiplier(GetAbilityLevel(RunUpgradeKind.Slow));
        public float GetExperienceMagnetRadiusWorld() => LevelUpAbilityRules.GetMagnetRadiusWorld(GetAbilityLevel(RunUpgradeKind.Magnet));
        public bool RegionStabilized => _regionStabilized;
        public EchoRegionId SelectedRegion => _selectedRegion;
        public bool IsRegionAvailable(EchoRegionId region) =>
            RegionUnlockRules.IsAvailable(region, PlayerPrefs.GetInt(BasinStabilizedPreferenceKey, 0) == 1);
        public float NextRunRandom() => _rng.NextFloat();
        public int GetShopUpgradeLevel(string id) { EnsureShopSaveLoaded(); return _shopSave.upgrades.Get(id); }
        public float GetShopSphereRadiusMultiplier() => PersistentUpgradeRules.GetPercentMultiplier(GetShopUpgradeLevel("radius"));
        private DifficultyProfile CurrentDifficulty => DifficultyRules.GetProfile(PlayerPrefs.GetString(DifficultyPreferenceKey, "normal"));
        public float SphereDamageMultiplier => _damageMultiplier * LevelUpAbilityRules.GetSphereDamageMultiplier(GetAbilityLevel(RunUpgradeKind.Damage)) *
            PersistentUpgradeRules.GetPercentMultiplier(GetShopUpgradeLevel("dmg")) *
            Mathf.Max(0.25f, 1f + ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "sphereDamage"));
        public float AttackSpeedMultiplier => _attackSpeedMultiplier * LevelUpAbilityRules.GetAttackSpeedMultiplier(GetAbilityLevel(RunUpgradeKind.AttackSpeed)) /
            Mathf.Max(0.5f, 1f + ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "sphereDelay"));
        public float ResonanceCharge => _resonanceCharge;
        public bool ResonanceEventActive => _resonanceEventActive;
        public int ResonanceEventsTriggered => _resonanceEventsTriggered;

        public string GetSphereBranch(SphereId type) => _sphereBranches.TryGetValue(type, out var id) ? id : null;
        public string GetSphereFinal(SphereId type) => _sphereFinals.TryGetValue(type, out var id) ? id : null;
        public bool HasArtifact(string artifactId) =>
            !string.IsNullOrEmpty(artifactId) && _ownedArtifacts.Contains(artifactId);
        public int GetSphereAuthoredModifierLevel(SphereId type, SphereAuthoredModifier modifier) =>
            SphereAuthoredModifierRules.GetLevel(GetSphereBranch(type), GetSphereFinal(type), modifier);
        public int GetSphereProjectilePierceBonus(SphereId type) =>
            SphereAuthoredModifierRules.GetProjectilePierceBonus(GetSphereBranch(type), GetSphereFinal(type));
        public int GetSphereProjectileRicochetCount(SphereId type)
        {
            // Prism branches have a dedicated beam/refraction path that resolves their authored Ricochet.
            if (type == SphereId.Prism) return 0;
            return SphereAuthoredModifierRules.GetProjectileRicochetCount(GetSphereBranch(type), GetSphereFinal(type));
        }

        public float GetSphereAuthoredDamageMultiplier(SphereId type) =>
            SphereAuthoredModifierRules.GetDamageMultiplier(GetSphereBranch(type), GetSphereFinal(type));
        public float GetSphereStatusDurationMultiplier(SphereElement element)
        {
            if (_sphereCharacterId != SphereCharacterId.Alchemist) return 1f;
            var multiplier = 1.30f;
            // Source Alchemist mastery III extends every Freeze application by 50%.
            // A ready Catalyst instead extends the next Fire/Poison application and is consumed.
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
            return multiplier;
        }
        public float GetSphereStatusDamageMultiplier() => _sphereCharacterId == SphereCharacterId.Alchemist ? 1.15f : 1f;
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
                    var data = JsonUtility.FromJson<CharacterProfilesSaveData>("{\\"profiles\\":" + raw + "}");
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
            _expandedCharacterId = CharacterRules.ToSourceId(id);
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
            if (_runResultSaved) return;
            _runResultSaved = true;

            var difficulty = CurrentDifficulty;
            var goldFormula = _runTime * 0.15d + _bossesDefeated * 25d + _kills * 0.05d;
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
            _leaderboardEntries.Add(entry);
            _leaderboardEntries.Sort((a, b) => b.time.CompareTo(a.time));
            if (_leaderboardEntries.Count > 10) _leaderboardEntries.RemoveRange(10, _leaderboardEntries.Count - 10);
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
            UnlockAchievementIf(_runChestOpens >= 5, "chest_5");
            UnlockAchievementIf(_playerCore != null && _playerCore.DashCount >= 50, "dash_50");

            if (isNewRecord) ShowMessage(IsRussianUi ? "НОВЫЙ РЕКОРД!" : "NEW RECORD!");
            else ShowMessage(IsRussianUi ? "РЕЗУЛЬТАТ СОХРАНЁН" : "RUN RESULT SAVED");
        }

        private void ResetPersistentProgress()
        {
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
            Application.targetFrameRate = 60;
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
            _regionSchedule = RegionBossScheduleRules.Create(_selectedRegion, EchoRegionMode.Stabilization);
            _regionFieldVisuals = gameObject.AddComponent<RegionFieldVisuals>();
            _regionFieldVisuals.Initialize(this, _selectedRegion);
            CreatePlayer();
            CreateSpheres();
            _follow = false;
            // Source new-desing starts with Formation Follow off. Players can
            // enable it from the existing control when they choose to.
            RefreshNetworkState();
            if (PlayerPrefs.GetInt("echo-sphere:start_selected_region_after_reload", 0) == 1)
            {
                PlayerPrefs.DeleteKey("echo-sphere:start_selected_region_after_reload");
                PlayerPrefs.Save();
                BeginSelectedRegionRun();
            }
        }

        private void Update()
        {
            PruneDestroyed();
            if (Input.GetKeyDown(KeyCode.Escape) && !_gameOver && !_levelUp && !_choosingEvolution && !_stellaRewardPending)
                SetUserPaused(!_userPaused);
            if (_mainMenu || _paused || _playerCore == null) return;
            var dt = Time.deltaTime;
            _runTime += dt;
            TickSphereArtAnimation(dt);
            _playerCore.Tick(dt);
            _swiftBootsTimer = Mathf.Max(0f, _swiftBootsTimer - dt);
            _alchemistCatalystTimer = Mathf.Max(0f, _alchemistCatalystTimer - dt);
            _playerCore.Heal((Mathf.Max(0f, ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "regen")) + LevelUpAbilityRules.GetRegenPerSecond(GetAbilityLevel(RunUpgradeKind.Regen))) * dt);
            if (_messageTimer > 0f) _messageTimer -= dt;
            _formationMemoryTimer = Mathf.Max(0f, _formationMemoryTimer - dt);
            _placementFlashTimer = Mathf.Max(0f, _placementFlashTimer - dt);
            if (Input.GetKeyDown(KeyCode.Space)) _playerCore.TryDash(_playerCore.LastMoveDirection);

            HandleSpherePointerInput();
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

            _player.position += (Vector3)(_playerCore.GetVelocity(input, 3.6f * GetArtifactMoveSpeedMultiplier() * LevelUpAbilityRules.GetMoveSpeedMultiplier(GetAbilityLevel(RunUpgradeKind.MoveSpeed)), followMultiplier) * dt);
            _networkRefreshTimer -= dt;
            if (_networkRefreshTimer <= 0f)
            {
                RefreshNetworkState();
                _networkRefreshTimer = 0.12f;
            }
            TickResonanceRing(dt);
            var cameraTarget = new Vector3(_player.position.x, _player.position.y, -10f);
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, cameraTarget, 1f - Mathf.Exp(-5f * dt));
            AdvanceRegionBossSchedule();
            if (_playerCore.IsDead && !_gameOver)
            {
                _gameOver = true;
                _paused = true;
                FinalizeRunResult();
                ShowMessage(IsRussianUi ? "ЯДРО РАЗРУШЕНО" : "CORE DESTROYED");
                return;
            }
            if (_regionStabilized) return;

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
            if (_playerCore.IsDead && !_gameOver)
            {
                _gameOver = true;
                _paused = true;
                FinalizeRunResult();
                ShowMessage(IsRussianUi ? "ЯДРО РАЗРУШЕНО" : "CORE DESTROYED");
            }
        }

        private Vector2 ReadMovement()
        {
            var direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (direction.sqrMagnitude > 0.0001f) return direction.normalized;

            // new-desing/MobileControls.tsx places the movement joystick on
            // the right half by default. The left half is used for Sphere
            // placement and dragging, so those gestures must never move Core.
            if (_spherePointerActive && !_spherePointerIsMovement) return Vector2.zero;

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began) _touchStart = touch.position;
                if (IsScreenPointOverUi(_touchStart)) return Vector2.zero;
                if (_touchStart.x >= Screen.width * 0.5f && touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                {
                    var drag = touch.position - _touchStart;
                    if (drag.sqrMagnitude > 100f) return Vector2.ClampMagnitude(drag / 90f, 1f);
                }
            }
            if (Input.GetMouseButtonDown(0)) _touchStart = Input.mousePosition;
            if (IsScreenPointOverUi(_touchStart)) return Vector2.zero;
            if (Input.GetMouseButton(0) && _touchStart.x >= Screen.width * 0.5f)
            {
                var drag = (Vector2)Input.mousePosition - _touchStart;
                if (drag.sqrMagnitude > 100f) return Vector2.ClampMagnitude(drag / 90f, 1f);
            }
            return Vector2.zero;
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
            _camera.clearFlags = CameraClearFlags.SolidColor;
        }

        private void CreateStarfield()
        {
            // Presentation-only coordinates are deterministic and never consume the run RNG.
            for (var i = 0; i < 4; i++)
            {
                var nebula = new GameObject("Nebula Haze");
                nebula.transform.position = new Vector3((i % 2 == 0 ? -1f : 1f) * (3.4f + i * 0.7f), (i < 2 ? 1f : -1f) * (2.6f + i * 0.4f), 3f);
                nebula.transform.localScale = Vector3.one * (5.8f + (i % 2) * 1.4f);
                var haze = nebula.AddComponent<SpriteRenderer>();
                haze.sprite = RuntimeSpriteFactory.Disc;
                haze.color = i % 2 == 0 ? new Color(0.08f, 0.34f, 0.78f, 0.055f) : new Color(0.31f, 0.12f, 0.72f, 0.045f);
                haze.sortingOrder = -30;
            }

            for (var i = 0; i < 104; i++)
            {
                var go = new GameObject("Background Star");
                var x = Mathf.Repeat(i * 0.7548777f, 1f) * 27f - 13.5f;
                var y = Mathf.Repeat(i * 0.5698403f + 0.173f, 1f) * 27f - 13.5f;
                go.transform.position = new Vector3(x, y, 2f);
                var size = 0.018f + Mathf.Repeat(i * 0.381966f, 1f) * 0.047f;
                go.transform.localScale = Vector3.one * size;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = RuntimeSpriteFactory.Disc;
                var alpha = 0.12f + Mathf.Repeat(i * 0.217f, 1f) * 0.28f;
                sr.color = i % 9 == 0 ? new Color(0.45f, 0.86f, 1f, alpha) : new Color(0.31f, 0.65f, 1f, alpha);
                sr.sortingOrder = -20;
            }
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
        }


        private void CreateSpheres()
        {
            // Source engineState.ts starts with one unlevelled Standard Sphere
            // 95 logical px below Core. Unlocking/levelling a Sphere is a
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
            ApplySphereVisual(go, type);
            var agent = go.AddComponent<SphereAttackAgent>();
            agent.Initialize(this, _player, type, 12f);
            if (type != SphereId.Standard)
            {
                var runtimeVfx = go.AddComponent<SphereRuntimeVfx2D>();
                runtimeVfx.Initialize(this, agent);
            }
            var elementalVfx = go.AddComponent<SphereElementalVfx2D>();
            elementalVfx.Initialize(this, agent);
            agent.ApplyProgressionLevel(level);
            _spheres.Add(agent);
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
                    // Standard is composed from four production PNG parts;
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


        private static bool ApplyStandardSphereArtwork(GameObject root)
        {
            var upper = RuntimeSpriteFactory.LoadAuthored("standard-sphere/upper-crystal");
            var energy = RuntimeSpriteFactory.LoadAuthored("standard-sphere/energy-core");
            var ring = RuntimeSpriteFactory.LoadAuthored("standard-sphere/stabilization-ring");
            var lower = RuntimeSpriteFactory.LoadAuthored("standard-sphere/lower-crystal");
            if (upper == null || energy == null || ring == null || lower == null) return false;

            var shell = root.transform.Find("Shell");
            var frame = root.transform.Find("Inner Frame");
            var core = root.transform.Find("Core Light");
            if (shell == null || frame == null || core == null) return false;

            var upperRenderer = shell.GetComponent<SpriteRenderer>();
            var ringRenderer = frame.GetComponent<SpriteRenderer>();
            var coreRenderer = core.GetComponent<SpriteRenderer>();
            if (upperRenderer == null || ringRenderer == null || coreRenderer == null) return false;

            // The source renderer assembles Standard from independent production
            // parts. Keep that architecture instead of flattening it into a generic SVG.
            upperRenderer.sprite = upper;
            upperRenderer.color = Color.white;
            upperRenderer.sortingOrder = 10;
            shell.localScale = Vector3.one * 0.54f;
            shell.gameObject.SetActive(true);

            coreRenderer.sprite = energy;
            coreRenderer.color = Color.white;
            coreRenderer.sortingOrder = 8;
            core.localScale = Vector3.one * 0.50f;
            core.gameObject.SetActive(true);

            ringRenderer.sprite = ring;
            ringRenderer.color = Color.white;
            ringRenderer.sortingOrder = 9;
            frame.localScale = Vector3.one * 0.59f;
            frame.gameObject.SetActive(true);

            var lowerObject = root.transform.Find("Standard Lower Crystal");
            if (lowerObject == null)
            {
                var lowerGo = new GameObject("Standard Lower Crystal");
                lowerGo.transform.SetParent(root.transform, false);
                lowerObject = lowerGo.transform;
            }
            lowerObject.localPosition = Vector3.zero;
            lowerObject.localRotation = Quaternion.identity;
            lowerObject.localScale = Vector3.one * 0.54f;
            var lowerRenderer = lowerObject.GetComponent<SpriteRenderer>();
            if (lowerRenderer == null) lowerRenderer = lowerObject.gameObject.AddComponent<SpriteRenderer>();
            lowerRenderer.sprite = lower;
            lowerRenderer.color = Color.white;
            lowerRenderer.sortingOrder = 11;
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

                var frame = sphere.transform.Find("Inner Frame");
                if (frame != null && frame.gameObject.activeSelf)
                    frame.Rotate(0f, 0f, 12.6f * deltaTime, Space.Self);

                var pulse = 0.5f + 0.5f * Mathf.Sin(_runTime * 4.6f + sphere.transform.position.y * 0.009f);
                var core = sphere.transform.Find("Core Light");
                if (core != null && core.gameObject.activeSelf)
                    core.localScale = Vector3.one * (0.50f * (1f + pulse * 0.035f));

                var upper = sphere.transform.Find("Shell");
                if (upper != null && upper.gameObject.activeSelf)
                    upper.localScale = Vector3.one * (0.54f * (1f + pulse * 0.022f));

                var lower = sphere.transform.Find("Standard Lower Crystal");
                if (lower != null && lower.gameObject.activeSelf)
                    lower.localScale = Vector3.one * (0.54f * (1f + pulse * 0.022f));
            }
        }

        private void ApplySphereVisual(GameObject root, SphereId type)
        {
            var shell = root.transform.Find("Shell");
            var core = root.transform.Find("Core Light");
            if (shell == null || core == null) return;

            if (type == SphereId.Standard && ApplyStandardSphereArtwork(root)) return;
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
                return RuntimeSpriteFactory.LoadAuthored("standard-sphere/energy-core") ?? RuntimeSpriteFactory.Faceted;
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
                new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * BossCombatRules.BossSpawnDistanceWorld;
            var archetype = EnemySpawnRules.Select(_runTime, _rng.NextFloat());
            var profile = EnemySpawnRules.GetProfile(archetype);
            var difficulty = CurrentDifficulty;
            var baseHp = (18f + _wave * 5.5f) * difficulty.EnemyHpMultiplier;
            var baseSpeed = BossCombatRules.ToWorldUnits(68f + _wave * 1.8f) * difficulty.EnemySpeedMultiplier;
            var tint = GetEnemyTint(archetype);
            var go = CreateOrb(GetEnemyName(archetype), position, 0.72f * profile.VisualScale, tint, Color.white, 4);
            ApplyShellVisual(go, GetEnemySprite(archetype), archetype == EnemyArchetype.Normal ? 1.12f : 1.02f);
            var enemyCore = go.transform.Find("Core Light");
            if (enemyCore != null) enemyCore.localScale = Vector3.one * (archetype == EnemyArchetype.Tank ? 0.29f : 0.22f);
            AddOrbitRail(go.transform, new Color(tint.r, tint.g, tint.b, 0.65f), new Vector3(1.7f, 0.62f, 1f), archetype == EnemyArchetype.Fast ? -28f : 38f, 3);
            switch (archetype)
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
            agent.Initialize(this, _player, baseHp * profile.HpMultiplier, baseSpeed * profile.SpeedMultiplier, profile.XpReward,
                GetEnemyVisualVariant(archetype), "", archetype == EnemyArchetype.Elite);
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
                if (_enemies[i] != null && !_enemies[i].IsBoss && _enemies[i].IsAlive) count++;
            return count;
        }

        private void StartNextWave()
        {
            _wave++;
            _waveEnemiesToSpawn = RunBalanceRules.GetEnemiesPerWave(_wave);
        }

        private void AdvanceRegionBossSchedule()
        {
            var signal = RegionBossScheduleRules.Advance(_regionSchedule, _runTime, HasActiveBoss());
            if (signal.Kind == RegionBossSignalKind.SpawnBoss)
            {
                SpawnBossForRegionEvent(signal.BossType, _wave);
            }
            else if (signal.Kind == RegionBossSignalKind.RegionCleared)
            {
                _regionStabilized = true;
                if (_regionSchedule.RegionId == EchoRegionId.ResonanceBasin &&
                    _regionSchedule.Mode == EchoRegionMode.Stabilization)
                {
                    PlayerPrefs.SetInt(BasinStabilizedPreferenceKey, 1);
                    PlayerPrefs.Save();
                }
                // Region completion is not game-over: keep pickups alive after the final boss.
                ShowMessage(IsRussianUi ? "РЕГИОН СТАБИЛИЗИРОВАН" : "REGION STABILIZED");
                AddExperience(RegionBossScheduleRules.GetRegionXpReward(_level));
            }
        }

        /// <summary>
        /// Invoked only by RegionBossScheduleRules.Advance, matching the source
        /// updateRegion spawnBoss signal instead of using an independent timer.
        /// </summary>
        public EnemyAgent2D SpawnBossForRegionEvent(string forcedBossType, int wave)
        {
            if (_player == null || _mainMenu || _gameOver) return null;
            var type = string.IsNullOrEmpty(forcedBossType)
                ? BossCombatRules.GetTypeForDefeatedCount(_bossesDefeated)
                : forcedBossType;
            if (!BossCombatRules.IsKnownBossType(type)) return null;

            var angle = _rng.NextFloat() * Mathf.PI * 2f;
            var position = (Vector2)_player.position +
                new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * BossCombatRules.BossSpawnDistanceWorld;
            var tint = Color.white;
            ColorUtility.TryParseHtmlString(BossCombatRules.GetColorHex(type), out tint);
            var difficulty = CurrentDifficulty;
            var hp = BossCombatRules.GetMaxHp(wave) * difficulty.EnemyHpMultiplier;
            var speed = BossCombatRules.GetMoveSpeedWorldUnitsPerSecond(type, wave) * difficulty.EnemySpeedMultiplier;
            var damage = BossCombatRules.GetContactDamage(wave);
            var root = CreateOrb("ECHO SPHERE Boss / " + type, position, 1.23f, tint, Color.white, 4);
            var agent = root.AddComponent<EnemyAgent2D>();
            agent.Initialize(this, _player, hp, speed, BossCombatRules.GetXpReward(wave),
                BossCombatRules.GetVisualVariant(type), type);
            _enemies.Add(agent);
            var combat = root.AddComponent<BossCombatAgent2D>();
            combat.Initialize(this, _player, agent, type, wave, speed, damage, tint);
            agent.SetBossCombat(combat);
            return agent;
        }

        public EnemyAgent2D SpawnBossMinion(Vector2 position, float hp, float moveSpeed, int xpReward, string visualVariant, Color tint)
        {
            if (_player == null || _gameOver) return null;
            var difficulty = CurrentDifficulty;
            var root = CreateOrb("Boss Minion", position, 0.38f, tint, Color.white, 4);
            var agent = root.AddComponent<EnemyAgent2D>();
            agent.Initialize(this, _player, hp * difficulty.EnemyHpMultiplier, moveSpeed * difficulty.EnemySpeedMultiplier, xpReward, visualVariant);
            _enemies.Add(agent);
            return agent;
        }

        public void SpawnBossProjectile(Vector2 position, Vector2 velocity, float damage, float sourceRadius, string bossType, Color color)
        {
            var projectile = new GameObject("Boss Projectile / " + bossType);
            projectile.transform.position = position;
            projectile.AddComponent<BossProjectileAgent2D>().Initialize(this, _player, velocity, sourceRadius, damage, bossType, color);
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
        private static readonly string[] EliteEnemyVisualVariants = { "linkbreaker", "leech", "stalker", "prism", "serpent", "beetle", "wisp", "prism", "brute", "skitter" };

        private string GetEnemyVisualVariant(EnemyArchetype archetype)
        {
            string[] variants;
            switch (archetype)
            {
                case EnemyArchetype.Fast: variants = FastEnemyVisualVariants; break;
                case EnemyArchetype.Tank: variants = TankEnemyVisualVariants; break;
                case EnemyArchetype.Elite: variants = EliteEnemyVisualVariants; break;
                default: variants = NormalEnemyVisualVariants; break;
            }
            return variants[_rng.NextInt(variants.Length)];
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
                    (ignoredIds != null && ignoredIds.Contains(enemy.GetInstanceID()))) continue;
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
            var combined = radius + 0.27f;
            var limit = combined * combined;
            for (var i = 0; i < _enemies.Count; i++)
            {
                var e = _enemies[i];
                if (e != null && e.IsAlive && (ignoredIds == null || !ignoredIds.Contains(e.GetInstanceID())) &&
                    ((Vector2)e.transform.position - position).sqrMagnitude <= limit) return e;
            }
            return null;
        }

        public ProjectileAgent SpawnProjectile(
            Vector2 position, Vector2 direction, float damage, Color color,
            float speed = 8f, int pierce = 0, float critChanceBonus = 0f,
            SphereAttackAgent evolutionOwner = null, int ricochets = 0, float lifetime = 2.5f,
            bool allowAuthoredProcs = true, bool useEvolutionHitEffects = true, bool applyElementalStatus = true)
        {
            var go = new GameObject("Sphere Projectile");
            go.transform.position = position;
            var agent = go.AddComponent<ProjectileAgent>();
            agent.Initialize(this, direction, damage, color, speed, pierce, critChanceBonus,
                evolutionOwner, ricochets, lifetime, allowAuthoredProcs, useEvolutionHitEffects, applyElementalStatus);
            var projectileVfx = go.AddComponent<ProjectileVfx2D>();
            projectileVfx.Initialize(this, direction, speed, color, evolutionOwner);
            return agent;
        }


        public bool RollCombatChance(float chance) => _rng.NextFloat() < Mathf.Clamp01(chance);

        public void HealPlayer(float amount)
        {
            if (_playerCore != null && !_playerCore.IsDead) _playerCore.Heal(Mathf.Max(0f, amount));
        }

        public float ResolveAuthoredSphereHitDamage(SphereId source, EnemyAgent2D target, float damage)
        {
            if (target == null || !target.IsAlive) return 0f;
            var branch = GetSphereBranch(source);
            var finalId = GetSphereFinal(source);
            damage *= SphereAuthoredModifierRules.GetDamageMultiplier(branch, finalId);

            var execute = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Execute);
            damage *= SphereAuthoredModifierRules.GetExecuteDamageMultiplier(execute, target.HpFraction);
            var mark = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Mark);
            damage *= SphereAuthoredModifierRules.GetMarkedDamageMultiplier(mark, target.IsMarked);
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
                target.KnockBackFrom(origin,
                    SphereAuthoredModifierRules.GetImpactDistancePixels(impact) * SpherePlacementRules.SourcePixelsToWorld);

            var gravitic = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Gravitic);
            if (gravitic > 0)
            {
                var center = (Vector2)target.transform.position;
                var radius = SphereAuthoredModifierRules.GetGraviticRangePixels() * SpherePlacementRules.SourcePixelsToWorld;
                var pullDistance = SphereAuthoredModifierRules.GetGraviticPullDistancePixels(gravitic) * SpherePlacementRules.SourcePixelsToWorld;
                var nearby = FindEnemiesInRadius(center, radius);
                for (var i = 0; i < nearby.Count; i++)
                    if (nearby[i] != target)
                        nearby[i].PullByDistance(center, pullDistance);
            }

            var anchor = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Anchor);
            if (anchor > 0)
                target.ApplySlow(
                    SphereAuthoredModifierRules.GetAnchorSlowDuration(anchor),
                    SphereAuthoredModifierRules.GetAnchorSlowMultiplier(anchor));

            var vampiric = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Vampiric);
            if (vampiric > 0) HealPlayer(damage * SphereAuthoredModifierRules.GetVampiricHealFraction(vampiric));

            var resonant = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Resonant);
            if (resonant > 0 && branch != "pulse_resonator")
                ChargeResonanceFromSource(SphereAuthoredModifierRules.GetResonantCharge(resonant), false);

            if (allowAuthoredProcs)
            {
                var echoLevel = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Echo);
                var echoChance = SphereAuthoredModifierRules.GetEchoChance(echoLevel);
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
                        0, 0.42f, false, false, false);
                    if (projectile != null) projectile.IgnoreEnemy(target);
                }

                var staticLevel = GetSphereAuthoredModifierLevel(source, SphereAuthoredModifier.Static);
                var staticChance = SphereAuthoredModifierRules.GetStaticChance(staticLevel);
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
                target.ApplyMark(2.5f);
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
            HealCoreFromDamage(damage);
            if (_resonanceEventActive) return;
            var multiplier = 1f;
            if (owner.Type == SphereId.Orbital && GetSphereBranch(SphereId.Orbital) == "orbital_halo")
                multiplier += SphereEvolutionCombatRules.GetOrbitalResonanceBonus(owner.ProgressionLevel, GetSphereFinal(SphereId.Orbital));
            AddResonanceChargeFromSource(ResonanceRules.SphereHitCharge * multiplier, false);
        }

        public void AddResonanceCharge(float amount)
        {
            if (_resonanceEventActive) return;
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
            _resonanceLineBurst--;
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
            if (formation == null || _spheres.Count == 0) return _player == null ? Vector2.zero : (Vector2)_player.position;
            var center = Vector2.zero;
            var count = 0;
            for (var i = 0; i < formation.NodeIndexes.Count; i++)
            {
                var index = formation.NodeIndexes[i];
                if (index < 0 || index >= _spheres.Count || _spheres[index] == null) continue;
                center += (Vector2)_spheres[index].transform.position;
                count++;
            }
            return count > 0 ? center / count : (_player == null ? Vector2.zero : (Vector2)_player.position);
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
                        SpawnImpact(from, new Color(0.72f, 0.40f, 0.86f, 0.9f));
                        if (nextIndex >= 0 && nextIndex < _spheres.Count && _spheres[nextIndex] != null)
                            SpawnImpact(Vector2.Lerp(from, _spheres[nextIndex].transform.position, 0.5f), new Color(0.72f, 0.40f, 0.86f, 0.95f));
                        if (i < targets.Count && targets[i] != null && usedTargets.Add(targets[i].GetInstanceID()))
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

        public void SpawnVoidShards(EnemyAgent2D source, float damage, int count)
        {
            if (source == null || count <= 0) return;
            var origin = (Vector2)source.transform.position;
            for (var i = 0; i < count; i++)
            {
                var angle = i * Mathf.PI * 2f / count;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                SpawnProjectile(origin, direction, damage, new Color(0.58f, 0.36f, 1f), 9.5f, 0, 0f);
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
                targets[i].PullByDistance(center, pullDistance);
                if (slowDuration > 0f) targets[i].ApplySlow(slowDuration, slowMultiplier);
            }
        }

        public void ApplyGravityTidePulse(Vector2 center, float radius, float distance, int mode)
        {
            var targets = FindEnemiesInRadius(center, radius);
            for (var i = 0; i < targets.Count; i++)
            {
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

        public void SpawnStandardSwarmShards(EnemyAgent2D hitTarget, Vector2 incomingDirection, int count, float damage)
        {
            if (hitTarget == null || count <= 0) return;
            var origin = (Vector2)hitTarget.transform.position;
            var candidates = FindNearestEnemies(origin, 5.5f, 12);
            var targets = new List<EnemyAgent2D>();
            for (var i = 0; i < candidates.Count; i++)
                if (candidates[i] != hitTarget) targets.Add(candidates[i]);

            for (var i = 0; i < count; i++)
            {
                Vector2 direction;
                if (i < targets.Count)
                    direction = ((Vector2)targets[i].transform.position - origin).normalized;
                else
                {
                    var angle = (i - (count - 1) * 0.5f) * 22f * Mathf.Deg2Rad;
                    var sin = Mathf.Sin(angle);
                    var cos = Mathf.Cos(angle);
                    direction = new Vector2(incomingDirection.x * cos - incomingDirection.y * sin,
                        incomingDirection.x * sin + incomingDirection.y * cos).normalized;
                }
                SpawnProjectile(origin, direction, damage, new Color(0.36f, 0.9f, 1f), 9.5f, 0, 0f);
            }
        }

        public void SpawnCombatArc(Vector2 from, Vector2 to, SphereId family, string branch, string finalId, Color color, float duration = 0.30f)
        {
            if ((to - from).sqrMagnitude <= 0.000625f) return;
            var go = new GameObject(family == SphereId.Chain ? "Chain Lightning Arc" : "Prism Refraction Arc");
            var arc = go.AddComponent<CombatArcVfx2D>();
            arc.Initialize(this, from, to, family, branch, finalId, color, duration);
        }

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
            if (chest == null || _gameOver || _stellaRewardPending || _levelUp || _choosingEvolution) return;
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
            _runChestOpens++;
            _stellaRewardPending = true;
            _userPaused = false;
            _paused = true;
            ShowMessage(_stellaRewardIsLegendary ? "LEGENDARY STELLA CACHE" : "STELLA ARTIFACT CACHE");
        }

        private void ChooseStellaReward(int choiceIndex)
        {
            if (!_stellaRewardPending || choiceIndex < 0 || choiceIndex >= _stellaRewardChoices.Count) return;
            var choice = _stellaRewardChoices[choiceIndex];
            if (choice == null || _ownedArtifacts.Contains(choice.Id)) return;
            _ownedArtifacts.Add(choice.Id);
            if (choice.GetEffect("maxHp") > 0f && _playerCore != null) _playerCore.IncreaseMaxHp(choice.GetEffect("maxHp"));
            if (_playerCore != null)
                _playerCore.SetDashCooldownMultiplier(Mathf.Max(0.55f, 1f + ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "cooldown")));
            _stellaRewardPending = false;
            _stellaRewardChoices.Clear();
            _paused = false;
            _userPaused = false;
            ShowMessage("ARTIFACT ACQUIRED · " + choice.NameEnglish);
            OpenLevelUpIfReady();
        }

        public void RegisterEnemyDeath(EnemyAgent2D enemy, Vector2 position, int xpReward = 1, SphereId? sourceSphere = null)
        {
            _enemies.Remove(enemy);
            if (sourceSphere.HasValue)
            {
                var drain = GetSphereAuthoredModifierLevel(sourceSphere.Value, SphereAuthoredModifier.Drain);
                if (drain > 0 && _playerCore != null)
                    HealPlayer(_playerCore.MaxHp * 0.03f * drain);
            }
            if (_ownedArtifacts.Contains("swift_boots")) _swiftBootsTimer = 3f;
            if (enemy != null && enemy.IsBoss)
            {
                _bossesDefeated++;
                SpawnStellaChest(position);
                ShowMessage("BOSS DEFEATED!");
            }
            if (enemy != null && enemy.IsElite) _runEliteKills++;
            _kills++;
            var go = new GameObject("Echo Shard");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * (0.22f + Mathf.Min(0.12f, Mathf.Max(0, xpReward - 1) * 0.035f));
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = xpReward >= 5 ? RuntimeSpriteFactory.Star : xpReward >= 3 ? RuntimeSpriteFactory.Hexagon : RuntimeSpriteFactory.Disc;
            sr.color = xpReward >= 5 ? new Color(1f, 0.45f, 0.82f) : xpReward >= 3 ? new Color(0.76f, 0.55f, 1f) : new Color(0.26f, 0.91f, 1f);
            sr.sortingOrder = 7;
            go.AddComponent<ExperienceOrbAgent>().Initialize(this, _player, xpReward);
        }

        public void AddExperience(int amount)
        {
            if (_paused || _gameOver) return;
            var multiplier = Mathf.Max(0.5f, PersistentUpgradeRules.GetPercentMultiplier(GetShopUpgradeLevel("xp")) *
                (1f + ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "xpGain")));
            _xp += Mathf.Max(0, (int)Mathf.Floor(Mathf.Max(0, amount) * multiplier + 0.5f));
            OpenLevelUpIfReady();
        }

        private void OpenLevelUpIfReady()
        {
            if (_gameOver || _levelUp || _xp < _xpRequired) return;
            _xp -= _xpRequired;
            _level++;
            // Source new-desing grants +6 Max HP and restores up to +6 HP on every player level.
            _playerCore?.IncreaseMaxHp(6f);
            _xpRequired = RunBalanceRules.GetXpToNextLevel(_level);
            BuildLevelUpChoices();
            _levelUp = true;
            _paused = true;
            _message = "Choose one upgrade.";
        }

        public float ResolveProjectileDamage(float damage, float critChanceBonus = 0f)
        {
            return ResolveProjectileDamage(damage, critChanceBonus, out _);
        }

        public float ResolveProjectileDamage(float damage, float critChanceBonus, out bool wasCritical)
        {
            var artifactCrit = ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "critChance");
            var abilityCrit = LevelUpAbilityRules.GetCritChanceBonus(GetAbilityLevel(RunUpgradeKind.Crit));
            var shopCrit = PersistentUpgradeRules.GetCritChanceBonus(GetShopUpgradeLevel("crit"));
            var critChance = Mathf.Min(CombatRules.CritHardCap, CombatRules.CritBase + Mathf.Max(0f, critChanceBonus + artifactCrit + abilityCrit + shopCrit));
            var result = CombatRules.ResolveDamage(damage, critChance, _rng.NextFloat(), CombatRules.CritMultiplierBase);
            wasCritical = result.WasCritical;
            return result.Damage;
        }

        public void TryDamagePlayer(float damage)
        {
            if (_playerCore == null || _playerCore.IsDead) return;
            var dodgeBonus = LevelUpAbilityRules.GetDodgeChance(
                GetAbilityLevel(RunUpgradeKind.Dodge),
                ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "dodgeChance"));
            if (dodgeBonus > 0f && _playerCore.CanReceiveDamageNow && _rng.NextFloat() < dodgeBonus)
            {
                ShowMessage("DODGED");
                return;
            }
            var received = Mathf.Max(0f, damage) * GetArtifactDamageTakenMultiplier() * CurrentDifficulty.EnemyDamageMultiplier;
            if (_ownedArtifacts.Contains("mirror") && _playerCore.CanReceiveDamageNow && _rng.NextFloat() < 0.20f)
            {
                EnemyAgent2D nearest = null;
                var nearestDistance = float.PositiveInfinity;
                for (var i = 0; i < _enemies.Count; i++)
                {
                    var candidate = _enemies[i];
                    if (candidate == null || !candidate.IsAlive) continue;
                    var distance = Vector2.Distance(candidate.transform.position, _playerCore.transform.position);
                    if (distance < nearestDistance) { nearestDistance = distance; nearest = candidate; }
                }
                if (nearest != null) nearest.ReceiveDamage(received * 1.25f);
                return;
            }
            var shieldsBefore = _playerCore.ShieldCharges;
            if (_playerCore.TryTakeDamage(received))
            {
                ShowMessage("Core integrity damaged!");
                if (_ownedArtifacts.Contains("stasis_core"))
                    for (var i = 0; i < _enemies.Count; i++)
                        if (_enemies[i] != null && _enemies[i].IsAlive) _enemies[i].ApplySlow(2f, 0.5f);
            }
            else if (_playerCore.ShieldCharges < shieldsBefore) ShowMessage("Shield charge absorbed impact!");
        }

        public void TryDamagePlayerOverTime(float damage)
        {
            if (_playerCore != null && !_playerCore.IsDead &&
                _playerCore.TryTakeDotDamage(Mathf.Max(0f, damage) * GetArtifactDamageTakenMultiplier() * CurrentDifficulty.EnemyDamageMultiplier))
                ShowMessage("Core integrity damaged!");
        }

        public void HealCoreFromDamage(float damage)
        {
            var vampire = Mathf.Max(0f, ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "vampire"));
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
            if (_ownedArtifacts.Contains("formation_compass")) multiplier *= 1.08f;
            return Mathf.Clamp(multiplier, 0.5f, 3f);
        }

        private float GetArtifactDamageTakenMultiplier() =>
            Mathf.Max(0.25f, 1f - ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "damageTakenReduction")) *
            LevelUpAbilityRules.GetDamageTakenMultiplier(GetAbilityLevel(RunUpgradeKind.Armor));

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
                    nodes.Add(new FormationNode(new Vec2(p.x, p.y), true));
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

            if (SphereProgressionRules.CanUpgradeMaxSpheres(_maxSphereAbilityLevel))
                pool.Add(LevelUpAbilityRules.CreateChoice(RunUpgradeKind.MaxSpheres, _maxSphereAbilityLevel));

            foreach (var kind in LevelUpAbilityRules.RoutineAbilityKinds)
            {
                var current = GetAbilityLevel(kind);
                if (LevelUpAbilityRules.CanUpgrade(kind, current))
                    pool.Add(LevelUpAbilityRules.CreateChoice(kind, current));
            }

            if (pool.Count == 0) return;
            var recent = new HashSet<string>(_recentUpgradeChoiceKeys);
            var cooled = pool.FindAll(choice => !recent.Contains(GetUpgradeChoiceKey(choice)));
            var candidates = cooled.Count >= 3 ? cooled : pool;

            var abilityScore = 0f;
            foreach (var pair in _abilityLevels) if (pair.Value > 0) abilityScore += 1f;
            if (_maxSphereAbilityLevel > 0) abilityScore += 1f;

            var sphereScore = _spheres.FindAll(sphere => sphere != null && sphere.gameObject.activeInHierarchy).Count;
            foreach (var pair in _sphereLevels) sphereScore += pair.Value * 0.15f;
            var minimumScore = Mathf.Min(abilityScore, sphereScore);
            var abilitySourceWeight = (1f + Mathf.Min(4, _levelUpAbilityPity) * 0.20f) *
                (abilityScore <= minimumScore + 0.001f ? 1.15f : 1f);
            var sphereSourceWeight = (1f + Mathf.Min(4, _levelUpSpherePity) * 0.20f) *
                (sphereScore <= minimumScore + 0.001f ? 1.15f : 1f);

            while (candidates.Count > 0 && _levelUpChoices.Count < 3)
            {
                var usedSphere = false;
                var usedAbility = false;
                foreach (var selected in _levelUpChoices)
                {
                    if (selected.Kind == RunUpgradeKind.Sphere) usedSphere = true;
                    else usedAbility = true;
                }

                var totalWeight = 0f;
                var weights = new float[candidates.Count];
                for (var i = 0; i < candidates.Count; i++)
                {
                    var choice = candidates[i];
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
                        if (_levelUpChoices.Count > 0) weight *= usedSphere ? 0.88f : 1.16f;
                    }
                    else
                    {
                        weight = abilitySourceWeight * (choice.CurrentLevel == 0 ? 1.35f : 1.15f);
                        if (_sphereCharacterId == SphereCharacterId.Oracle && choice.Kind == RunUpgradeKind.Crit)
                            weight *= (1f + 0.75f / (choice.CurrentLevel == 0 ? 1.35f : 1.15f));
                        if (_levelUpChoices.Count > 0) weight *= usedAbility ? 0.88f : 1.16f;
                    }
                    weights[i] = Mathf.Max(0.01f, weight);
                    totalWeight += weights[i];
                }

                var roll = _rng.NextFloat() * totalWeight;
                var chosenIndex = candidates.Count - 1;
                for (var i = 0; i < candidates.Count; i++)
                {
                    roll -= weights[i];
                    if (roll <= 0f) { chosenIndex = i; break; }
                }
                _levelUpChoices.Add(candidates[chosenIndex]);
                candidates.RemoveAt(chosenIndex);
            }
        }

        private string GetUpgradeChoiceKey(RunUpgradeChoice choice) =>
            choice.Kind == RunUpgradeKind.Sphere
                ? "sphere:" + choice.Sphere + ":" + choice.CurrentLevel + ":" + choice.NextLevel
                : "ability:" + choice.Kind + ":" + choice.CurrentLevel + ":" + choice.NextLevel;

        private float GetSphereAffinityWeight(SphereId type)
        {
            var signature = SphereId.Standard;
            var partner = SphereId.Orbital;
            switch (_sphereCharacterId)
            {
                case SphereCharacterId.Hunter: signature = SphereId.Sniper; partner = SphereId.Void; break;
                case SphereCharacterId.Engineer: signature = SphereId.Chain; partner = SphereId.Aura; break;
                case SphereCharacterId.Berserker: signature = SphereId.Shotgun; partner = SphereId.Void; break;
                case SphereCharacterId.Alchemist: signature = SphereId.Aura; partner = SphereId.Chain; break;
                case SphereCharacterId.Architect: signature = SphereId.Gravity; partner = SphereId.Prism; break;
                case SphereCharacterId.Conductor: signature = SphereId.Pulse; partner = SphereId.Chain; break;
                case SphereCharacterId.Oracle: signature = SphereId.Prism; partner = SphereId.Pulse; break;
                case SphereCharacterId.Voidwalker: signature = SphereId.Void; partner = SphereId.Gravity; break;
                case SphereCharacterId.Fractal: signature = SphereId.Orbital; partner = SphereId.Aura; break;
            }
            return type == signature ? 0.70f : type == partner ? 0.30f : 0f;
        }

        private int GetAbilityLevel(RunUpgradeKind kind)
        {
            if (kind == RunUpgradeKind.MaxSpheres) return _maxSphereAbilityLevel;
            return _abilityLevels.TryGetValue(kind, out var level) ? level : 0;
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
            RecordLevelUpChoice(choice);
            switch (choice.Kind)
            {
                case RunUpgradeKind.Sphere:
                    _sphereLevels[choice.Sphere] = choice.NextLevel;
                    // Level Up changes the source Sphere progression only. The
                    // physical Sphere entity is independently placed/removed
                    // through MobileControls in new-desing.
                    foreach (var sphere in _spheres)
                        if (sphere != null && sphere.Type == choice.Sphere)
                            sphere.ApplyProgressionLevel(choice.NextLevel);
                    ShowMessage(choice.Title + " upgraded.");
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
                            ShowMessage(_pendingFinalEvolution ? "Choose final evolution." : "Choose mutation branch.");
                            return;
                        }
                    }
                    break;
                case RunUpgradeKind.MaxSpheres:
                    _maxSphereAbilityLevel = choice.NextLevel;
                    ShowMessage("MAX SPHERES: " + GetCurrentMaxSpheres());
                    break;
                case RunUpgradeKind.Damage:
                case RunUpgradeKind.AttackSpeed:
                case RunUpgradeKind.MoveSpeed:
                case RunUpgradeKind.Slow:
                case RunUpgradeKind.Dodge:
                case RunUpgradeKind.Crit:
                case RunUpgradeKind.Magnet:
                case RunUpgradeKind.Armor:
                case RunUpgradeKind.Regen:
                case RunUpgradeKind.Stability:
                    _abilityLevels[choice.Kind] = choice.NextLevel;
                    ShowMessage(choice.Title + " upgraded.");
                    break;
                case RunUpgradeKind.Vitality:
                    _abilityLevels[choice.Kind] = choice.NextLevel;
                    _playerCore?.IncreaseMaxHp(20f);
                    ShowMessage("VITALITY: Core capacity increased.");
                    break;
            }
            if (_choosingEvolution) return;
            _levelUp = false;
            _paused = false;
            _levelUpChoices.Clear();
            if (_follow) SetFormationFollowMode(true, true);
            OpenLevelUpIfReady();
        }

        private void ChooseEvolution(int index)
        {
            if (!_choosingEvolution || index < 0 || index >= _evolutionChoices.Count) return;
            var option = _evolutionChoices[index];
            if (_pendingFinalEvolution)
            {
                _sphereFinals[_pendingEvolutionSphere] = option.Id;
                ShowMessage("Final evolution: " + option.Name);
            }
            else
            {
                _sphereBranches[_pendingEvolutionSphere] = option.Id;
                ShowMessage("Mutation branch: " + option.Name);
            }
            _runEvolutions++;
            _choosingEvolution = false;
            _pendingFinalEvolution = false;
            _evolutionChoices.Clear();
            _levelUp = false;
            _paused = false;
            OpenLevelUpIfReady();
        }

        private void PruneDestroyed()
        {
            for (var i = _enemies.Count - 1; i >= 0; i--) if (_enemies[i] == null) _enemies.RemoveAt(i);
            for (var i = _spheres.Count - 1; i >= 0; i--) if (_spheres[i] == null) _spheres.RemoveAt(i);
        }

        private void ShowMessage(string text) { _message = text; _messageTimer = 2.2f; }

        private void SetUserPaused(bool paused)
        {
            if (_gameOver || _levelUp || _choosingEvolution || _stellaRewardPending) return;
            _userPaused = paused;
            _paused = paused;
            ShowMessage(paused ? "Run paused." : "Run resumed.");
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
            var index = GetSphereIndex(sphere);
            var multiplier = 1f;
            if (index >= 0)
            {
                multiplier += 0.10f * GetFormationBonus(SphereNetworkFormation.Line, index)
                    + 0.15f * GetFormationBonus(SphereNetworkFormation.Fractal, index);
                if (ConsumeResonanceLineBurst()) multiplier *= 1.60f;
            }
            return multiplier * GetArtifactSphereFamilyDamageMultiplier(sphere.Type);
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

        private void RefreshNetworkState()
        {
            var nodes = new List<SphereNetworkNode>(_spheres.Count);
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

            var previous = _preferredDominantFormation != SphereNetworkFormation.None ? _preferredDominantFormation : _previousDominantFormation;
            _networkState = SphereNetworkRules.Analyze(nodes, SphereNetworkRules.DefaultLinkDistance, previous);
            if (_preferredDominantFormation != SphereNetworkFormation.None &&
                !SphereNetworkRules.TrySetDominantFormation(_networkState, _preferredDominantFormation))
                _preferredDominantFormation = SphereNetworkFormation.None;
            if (_preferredSecondaryFormation != SphereNetworkFormation.None &&
                !SphereNetworkRules.TrySetSecondaryFormation(_networkState, _preferredSecondaryFormation))
                _preferredSecondaryFormation = SphereNetworkFormation.None;
            if (_networkState.DominantFormation != null)
                _previousDominantFormation = _networkState.DominantFormation.Type;
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
            if (_networkState == null) return;
            if (_networkLineMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null) _networkLineMaterial = new Material(shader);
            }

            while (_networkLinkRenderers.Count < _networkState.Links.Count)
            {
                var go = new GameObject("Sphere Network Link");
                go.transform.SetParent(transform, false);
                var line = go.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.positionCount = 2;
                line.startWidth = 0.022f;
                line.endWidth = 0.022f;
                line.numCapVertices = 2;
                line.sortingOrder = 5;
                if (_networkLineMaterial != null) line.sharedMaterial = _networkLineMaterial;
                _networkLinkRenderers.Add(line);
            }

            for (var i = 0; i < _networkLinkRenderers.Count; i++)
            {
                var line = _networkLinkRenderers[i];
                if (i >= _networkState.Links.Count)
                {
                    line.enabled = false;
                    continue;
                }

                var link = _networkState.Links[i];
                if (link.A < 0 || link.B < 0 || link.A >= _spheres.Count || link.B >= _spheres.Count ||
                    _spheres[link.A] == null || _spheres[link.B] == null)
                {
                    line.enabled = false;
                    continue;
                }

                line.enabled = true;
                line.SetPosition(0, _spheres[link.A].transform.position);
                line.SetPosition(1, _spheres[link.B].transform.position);
                var color = new Color(0.20f, 0.72f, 0.88f, 0.40f);
                if (IsFormationLink(_networkState.DominantFormation, link.A, link.B))
                    color = new Color(1f, 0.77f, 0.34f, 0.82f);
                else if (IsFormationLink(_networkState.SecondaryFormation, link.A, link.B))
                    color = new Color(0.63f, 0.52f, 1f, 0.70f);
                line.startColor = color;
                line.endColor = color;
            }
        }

        private static bool IsFormationLink(SphereNetworkShape shape, int a, int b) =>
            shape != null && shape.Contains(a) && shape.Contains(b);

        private void OnDestroy()
        {
            if (_networkLineMaterial != null) Destroy(_networkLineMaterial);
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
            }

            var previousColor = GUI.color;
            GUI.color = new Color(0.008f, 0.018f, 0.055f, 0.98f);
            GUI.DrawTexture(new Rect(0f, 0f, _uiWidth, _uiHeight), _uiPanelTexture);
            GUI.color = new Color(0.08f, 0.54f, 0.94f, 0.7f);
            GUI.DrawTexture(new Rect(0f, 0f, _uiWidth, 3f), _uiAccentTexture);
            GUI.DrawTexture(new Rect(0f, _uiHeight - 3f, _uiWidth, 3f), _uiAccentTexture);
            GUI.color = previousColor;

            var width = Mathf.Min(620f, _uiWidth - 32f);
            var height = Mathf.Min(560f, _uiHeight - 28f);
            var panel = new Rect((_uiWidth - width) * 0.5f, (_uiHeight - height) * 0.5f, width, height);
            GUI.Box(panel, GUIContent.none, _box);
            GUI.color = new Color(0.1f, 0.68f, 1f, 0.9f);
            GUI.DrawTexture(new Rect(panel.x + 28f, panel.y + 24f, panel.width - 56f, 2f), _uiAccentTexture);
            GUI.color = previousColor;

            var logoSize = Mathf.Min(88f, height * 0.18f);
            var coreLogo = RuntimeSpriteFactory.LoadAuthored("player") ?? RuntimeSpriteFactory.Faceted;
            DrawSpriteIcon(new Rect(panel.center.x - logoSize * 0.5f, panel.y + 28f, logoSize, logoSize), coreLogo, Color.white);
            DrawSpriteIcon(new Rect(panel.center.x - logoSize * 0.64f, panel.y + 23f, logoSize * 1.28f, logoSize * 1.28f), RuntimeSpriteFactory.Ring, new Color(0.22f, 0.67f, 1f, 0.8f));
            GUI.Label(new Rect(panel.x + 24f, panel.y + 115f, panel.width - 48f, 42f), "ECHO SPHERE", _menuTitle);
            GUI.Label(new Rect(panel.x + 24f, panel.y + 157f, panel.width - 48f, 23f),
                IsRussianUi ? "СИСТЕМА ВЫЖИВАНИЯ РЕЗОНАНСА" : "RESONANCE SURVIVAL PROTOCOL", _label);

            var characterArt = RuntimeSpriteFactory.LoadAuthored("character-spherist-3q");
            if (characterArt != null)
            {
                DrawSpriteIcon(new Rect(panel.x + 34f, panel.y + 185f, 76f, 96f), characterArt, Color.white);
                GUI.Label(new Rect(panel.x + 128f, panel.y + 198f, panel.width - 160f, 48f),
                    IsRussianUi ? "СФЕРИСТ  /  УНИВЕРСАЛ" : "SPHERIST  /  ALL-ROUNDER", _label);
                GUI.Label(new Rect(panel.x + 128f, panel.y + 229f, panel.width - 160f, 45f),
                    IsRussianUi ? "Сохраняй Core. Собирай сеть Сфер. Меняй ход боя." : "Keep the Core alive. Build your Sphere network. Bend the battlefield.", _label);
            }
            else
            {
                GUI.Label(new Rect(panel.x + 34f, panel.y + 205f, panel.width - 68f, 48f),
                    IsRussianUi ? "Сохраняй Core. Собирай сеть Сфер. Меняй ход боя." : "Keep the Core alive. Build your Sphere network. Bend the battlefield.", _label);
            }

            GUI.Label(new Rect(panel.x + 34f, panel.y + height - 276f, 76f, 26f), "ID", _label);
            var enteredName = GUI.TextField(new Rect(panel.x + 100f, panel.y + height - 280f, panel.width - 134f, 30f), _playerName, 16, _button);
            if (enteredName != _playerName)
            {
                _playerName = enteredName;
                PlayerPrefs.SetString(NamePreferenceKey, _playerName);
                PlayerPrefs.Save();
            }

            GUI.Label(new Rect(panel.x + 34f, panel.y + height - 238f, 96f, 28f),
                IsRussianUi ? "СЛОЖНОСТЬ" : "DIFFICULTY", _label);
            var difficultyIds = new[] { "easy", "normal", "hard", "nightmare" };
            var difficultyLabels = IsRussianUi
                ? new[] { "ЛЁГКАЯ", "ОБЫЧНАЯ", "СЛОЖНАЯ", "КОШМАР" }
                : new[] { "EASY", "NORMAL", "HARD", "NIGHTMARE" };
            var currentDifficulty = PlayerPrefs.GetString(DifficultyPreferenceKey, "normal");
            var difficultyLeft = panel.x + 126f;
            var difficultyWidth = (panel.width - 160f) / 4f;
            for (var i = 0; i < difficultyIds.Length; i++)
            {
                var rect = new Rect(difficultyLeft + i * difficultyWidth, panel.y + height - 240f, difficultyWidth - 4f, 30f);
                var oldColor = GUI.color;
                GUI.color = currentDifficulty == difficultyIds[i] ? new Color(0.25f, 0.82f, 1f, 1f) : Color.white;
                if (GUI.Button(rect, difficultyLabels[i], _button))
                {
                    PlayerPrefs.SetString(DifficultyPreferenceKey, difficultyIds[i]);
                    PlayerPrefs.Save();
                }
                GUI.color = oldColor;
            }

            var buttonWidth = Mathf.Min(330f, panel.width - 60f);
            var runButton = new Rect(panel.center.x - buttonWidth * 0.5f, panel.y + height - 188f, buttonWidth, 48f);
            if (GUI.Button(runButton, IsRussianUi ? "ВЫБРАТЬ ЗАБЕГ" : "SELECT RUN", _button))
            {
                _regionSelect = true;
            }

            var navY = panel.y + height - 124f;
            var navLeft = panel.x + 22f;
            var navGap = 7f;
            var navWidth = (panel.width - 44f - navGap * 4f) / 5f;
            if (GUI.Button(new Rect(navLeft, navY, navWidth, 38f), IsRussianUi ? "МАГАЗИН" : "SHOP", _button))
                _mainMenuPage = MainMenuPage.Shop;
            if (GUI.Button(new Rect(navLeft + (navWidth + navGap), navY, navWidth, 38f), IsRussianUi ? "НАСТРОЙКИ" : "SETTINGS", _button))
                _mainMenuPage = MainMenuPage.Settings;
            if (GUI.Button(new Rect(navLeft + (navWidth + navGap) * 2f, navY, navWidth, 38f), IsRussianUi ? "РЕКОРДЫ" : "RECORDS", _button))
                _mainMenuPage = MainMenuPage.Leaderboard;
            if (GUI.Button(new Rect(navLeft + (navWidth + navGap) * 3f, navY, navWidth, 38f), IsRussianUi ? "ДОСТИЖЕНИЯ" : "ACHIEVEMENTS", _button))
                _mainMenuPage = MainMenuPage.Achievements;
            if (GUI.Button(new Rect(navLeft + (navWidth + navGap) * 4f, navY, navWidth, 38f), IsRussianUi ? "ПЕРСОНАЖИ" : "CHARACTERS", _button))
                _mainMenuPage = MainMenuPage.Characters;

            GUI.Label(new Rect(panel.x + 24f, panel.y + height - 76f, panel.width - 48f, 34f),
                IsRussianUi ? "Движение: правая часть экрана · Сферы: левая часть · Рывок: кнопка DASH" :
                "Move: right half · Spheres: left half · Evade: DASH", _label);
            GUI.color = new Color(0.33f, 0.76f, 1f, 0.6f);
            GUI.DrawTexture(new Rect(panel.x + 28f, panel.y + height - 24f, panel.width - 56f, 1f), _uiAccentTexture);
            GUI.color = previousColor;
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
            GUI.BeginScrollView(viewport, _characterScrollPosition, new Rect(0f, 0f, contentWidth, contentHeight));
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

        private void DrawSettingsScreen()
        {
            var panel = DrawMenuScreenFrame(
                IsRussianUi ? "НАСТРОЙКИ" : "SETTINGS",
                IsRussianUi ? "ЯЗЫК · СЛОЖНОСТЬ · ПРОГРЕСС" : "LANGUAGE · DIFFICULTY · PROGRESS", 570f);

            GUI.Label(new Rect(panel.x + 28f, panel.y + 112f, 150f, 28f),
                IsRussianUi ? "ЯЗЫК / LANGUAGE" : "LANGUAGE", _label);
            var ruRect = new Rect(panel.x + 198f, panel.y + 106f, 190f, 40f);
            var enRect = new Rect(panel.x + 402f, panel.y + 106f, 190f, 40f);
            var lang = PlayerPrefs.GetString(LanguagePreferenceKey, Application.systemLanguage == SystemLanguage.English ? "en" : "ru");
            var oldColor = GUI.color;
            GUI.color = lang == "ru" ? new Color(0.25f, 0.82f, 1f, 1f) : Color.white;
            if (GUI.Button(ruRect, "Русский", _button)) { PlayerPrefs.SetString(LanguagePreferenceKey, "ru"); PlayerPrefs.Save(); }
            GUI.color = lang == "en" ? new Color(0.25f, 0.82f, 1f, 1f) : Color.white;
            if (GUI.Button(enRect, "English", _button)) { PlayerPrefs.SetString(LanguagePreferenceKey, "en"); PlayerPrefs.Save(); }
            GUI.color = oldColor;

            GUI.Label(new Rect(panel.x + 28f, panel.y + 170f, 150f, 28f),
                IsRussianUi ? "СЛОЖНОСТЬ" : "DIFFICULTY", _label);
            var difficultyIds = new[] { "easy", "normal", "hard", "nightmare" };
            var difficultyLabels = IsRussianUi
                ? new[] { "ЛЁГКАЯ", "ОБЫЧНАЯ", "СЛОЖНАЯ", "КОШМАР" }
                : new[] { "EASY", "NORMAL", "HARD", "NIGHTMARE" };
            var currentDifficulty = PlayerPrefs.GetString(DifficultyPreferenceKey, "normal");
            var difficultyLeft = panel.x + 198f;
            var difficultyWidth = (panel.width - 226f) / 4f;
            for (var i = 0; i < difficultyIds.Length; i++)
            {
                var rect = new Rect(difficultyLeft + i * difficultyWidth, panel.y + 164f, difficultyWidth - 5f, 40f);
                oldColor = GUI.color;
                GUI.color = currentDifficulty == difficultyIds[i] ? new Color(0.25f, 0.82f, 1f, 1f) : Color.white;
                if (GUI.Button(rect, difficultyLabels[i], _button))
                {
                    PlayerPrefs.SetString(DifficultyPreferenceKey, difficultyIds[i]);
                    PlayerPrefs.Save();
                }
                GUI.color = oldColor;
            }

            GUI.Label(new Rect(panel.x + 28f, panel.y + 232f, 150f, 26f),
                IsRussianUi ? "ИМЯ ИГРОКА" : "PLAYER NAME", _label);
            var nameValue = GUI.TextField(new Rect(panel.x + 198f, panel.y + 225f, panel.width - 228f, 34f), _playerName, 16, _button);
            if (nameValue != _playerName)
            {
                _playerName = nameValue;
                PlayerPrefs.SetString(NamePreferenceKey, _playerName);
                PlayerPrefs.Save();
            }

            GUI.Label(new Rect(panel.x + 28f, panel.y + 286f, panel.width - 56f, 70f),
                IsRussianUi
                    ? "Движение: правая часть экрана. Размещение и перемещение Сфер: левая часть. DASH выполняет рывок."
                    : "Movement: right half of the screen. Sphere placement and dragging: left half. DASH performs a quick evade.",
                _sphereDescriptionStyle);

            if (!_settingsResetPending)
            {
                if (GUI.Button(new Rect(panel.center.x - 165f, panel.y + 370f, 330f, 44f),
                    IsRussianUi ? "СБРОСИТЬ ВЕСЬ ПРОГРЕСС" : "RESET ALL PROGRESS", _button))
                    _settingsResetPending = true;
            }
            else
            {
                GUI.Label(new Rect(panel.x + 30f, panel.y + 363f, panel.width - 60f, 34f),
                    IsRussianUi ? "Удалить золото, улучшения, рекорды и достижения?" :
                    "Delete gold, upgrades, records and achievements?", _label);
                if (GUI.Button(new Rect(panel.center.x - 175f, panel.y + 405f, 165f, 42f),
                    IsRussianUi ? "ОТМЕНА" : "CANCEL", _button))
                    _settingsResetPending = false;
                if (GUI.Button(new Rect(panel.center.x + 10f, panel.y + 405f, 165f, 42f),
                    IsRussianUi ? "ПОДТВЕРДИТЬ" : "CONFIRM", _button))
                    ResetPersistentProgress();
            }

            if (GUI.Button(new Rect(panel.x + 22f, panel.yMax - 48f, 132f, 34f),
                IsRussianUi ? "НАЗАД" : "BACK", _button))
            {
                _settingsResetPending = false;
                _mainMenuPage = MainMenuPage.Home;
            }
        }

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

        private void DrawRegionSelect()
            var previousColor = GUI.color;
            GUI.color = new Color(0.008f, 0.018f, 0.055f, 0.98f);
            GUI.DrawTexture(new Rect(0f, 0f, _uiWidth, _uiHeight), _uiPanelTexture);
            GUI.color = new Color(0.08f, 0.54f, 0.94f, 0.7f);
            GUI.DrawTexture(new Rect(0f, 0f, _uiWidth, 3f), _uiAccentTexture);
            GUI.DrawTexture(new Rect(0f, _uiHeight - 3f, _uiWidth, 3f), _uiAccentTexture);
            GUI.color = previousColor;

            var width = Mathf.Min(620f, _uiWidth - 32f);
            var height = Mathf.Min(480f, _uiHeight - 28f);
            var panel = new Rect((_uiWidth - width) * 0.5f, (_uiHeight - height) * 0.5f, width, height);
            GUI.Box(panel, GUIContent.none, _box);
            GUI.color = new Color(0.1f, 0.68f, 1f, 0.9f);
            GUI.DrawTexture(new Rect(panel.x + 28f, panel.y + 24f, panel.width - 56f, 2f), _uiAccentTexture);
            GUI.color = previousColor;

            var logoSize = Mathf.Min(104f, height * 0.23f);
            var coreLogo = RuntimeSpriteFactory.LoadAuthored("player") ?? RuntimeSpriteFactory.Faceted;
            DrawSpriteIcon(new Rect(panel.center.x - logoSize * 0.5f, panel.y + 42f, logoSize, logoSize), coreLogo, Color.white);
            DrawSpriteIcon(new Rect(panel.center.x - logoSize * 0.64f, panel.y + 36f, logoSize * 1.28f, logoSize * 1.28f), RuntimeSpriteFactory.Ring, new Color(0.22f, 0.67f, 1f, 0.8f));
            GUI.Label(new Rect(panel.x + 24f, panel.y + 152f, panel.width - 48f, 48f), "ECHO SPHERE", _menuTitle);
            GUI.Label(new Rect(panel.x + 24f, panel.y + 198f, panel.width - 48f, 25f),
                IsRussianUi ? "СИСТЕМА ВЫЖИВАНИЯ РЕЗОНАНСА" : "RESONANCE SURVIVAL PROTOCOL", _label);
            var characterArt = RuntimeSpriteFactory.LoadAuthored("character-spherist-3q");
            if (characterArt != null)
            {
                DrawSpriteIcon(new Rect(panel.x + 44f, panel.y + 222f, 86f, 106f), characterArt, Color.white);
                GUI.Label(new Rect(panel.x + 144f, panel.y + 234f, panel.width - 180f, 58f),
                    IsRussianUi ? "СФЕРИСТ  /  УНИВЕРСАЛ" : "SPHERIST  /  ALL-ROUNDER", _label);
                GUI.Label(new Rect(panel.x + 144f, panel.y + 267f, panel.width - 180f, 51f),
                    IsRussianUi ? "Сохраняй Core. Собирай сеть Сфер. Меняй ход боя." : "Keep the Core alive. Build your Sphere network. Bend the battlefield.", _label);
            }
            else
            {
                GUI.Label(new Rect(panel.x + 38f, panel.y + 235f, panel.width - 76f, 44f),
                    IsRussianUi ? "Сохраняй Core. Собирай сеть Сфер. Меняй ход боя." : "Keep the Core alive. Build your Sphere network. Bend the battlefield.", _label);
            }
            var buttonWidth = Mathf.Min(330f, panel.width - 60f);
            var button = new Rect(panel.center.x - buttonWidth * 0.5f, panel.y + height - 148f, buttonWidth, 54f);
            if (GUI.Button(button, IsRussianUi ? "ВЫБРАТЬ ЗАБЕГ" : "SELECT RUN", _button))
            {
                _regionSelect = true;
            }
            GUI.Label(new Rect(panel.x + 24f, button.y + 65f, panel.width - 48f, 38f), "MOVE / drag left side     •     DASH / evade     •     NETWORK / stay connected", _label);
            GUI.color = new Color(0.33f, 0.76f, 1f, 0.6f);
            GUI.DrawTexture(new Rect(panel.x + 28f, panel.y + height - 24f, panel.width - 56f, 1f), _uiAccentTexture);
            GUI.color = previousColor;
        }


        private void DrawRegionSelect()
        {
            var previousColor = GUI.color;
            GUI.color = new Color(0.008f, 0.018f, 0.055f, 0.98f);
            GUI.DrawTexture(new Rect(0f, 0f, _uiWidth, _uiHeight), _uiPanelTexture);
            GUI.color = new Color(0.08f, 0.54f, 0.94f, 0.7f);
            GUI.DrawTexture(new Rect(0f, 0f, _uiWidth, 3f), _uiAccentTexture);
            GUI.color = previousColor;

            var width = Mathf.Min(760f, _uiWidth - 28f);
            var height = Mathf.Min(540f, _uiHeight - 24f);
            var panel = new Rect((_uiWidth - width) * 0.5f, (_uiHeight - height) * 0.5f, width, height);
            GUI.Box(panel, GUIContent.none, _box);
            GUI.Label(new Rect(panel.x + 24f, panel.y + 18f, panel.width - 48f, 42f),
                IsRussianUi ? "РЕГИОНЫ ЭХА" : "ECHO REGIONS", _menuTitle);
            GUI.Label(new Rect(panel.x + 24f, panel.y + 61f, panel.width - 48f, 24f),
                IsRussianUi ? "СТАБИЛИЗИРУЙ  /  ИССЛЕДУЙ" : "STABILIZE  /  EXPLORE", _label);
            GUI.Label(new Rect(panel.x + 24f, panel.y + 91f, panel.width - 48f, 34f),
                IsRussianUi ? "Выбери регион для забега." : "Choose a region for this run.", _label);

            var cardGap = 14f;
            var cardWidth = (panel.width - 60f - cardGap) * 0.5f;
            var cardTop = panel.y + 140f;
            var cardHeight = Mathf.Min(240f, panel.height - 230f);
            var basin = new Rect(panel.x + 20f, cardTop, cardWidth, cardHeight);
            var rift = new Rect(basin.xMax + cardGap, cardTop, cardWidth, cardHeight);
            var basinSelected = _selectedRegion == EchoRegionId.ResonanceBasin;
            var riftSelected = _selectedRegion == EchoRegionId.SpectralRift;
            var riftAvailable = IsRegionAvailable(EchoRegionId.SpectralRift);

            GUI.Box(basin, GUIContent.none, _box);
            GUI.color = new Color(0.25f, 0.9f, 0.76f, 0.95f);
            GUI.DrawTexture(new Rect(basin.x + 12f, basin.y + 12f, 3f, basin.height - 24f), _uiAccentTexture);
            GUI.color = previousColor;
            GUI.Label(new Rect(basin.x + 26f, basin.y + 18f, basin.width - 40f, 20f),
                IsRussianUi ? "РЕГИОН 01  /  ДОСТУПЕН" : "REGION 01  /  AVAILABLE", _label);
            GUI.Label(new Rect(basin.x + 26f, basin.y + 48f, basin.width - 40f, 48f),
                IsRussianUi ? "РЕЗОНАНСНЫЙ БАССЕЙН" : "RESONANCE BASIN", _title);
            GUI.Label(new Rect(basin.x + 26f, basin.y + 98f, basin.width - 40f, 45f),
                IsRussianUi ? "ПЕРВЫЙ СТАБИЛЬНЫЙ ECHO" : "THE FIRST STABLE ECHO", _label);
            GUI.Label(new Rect(basin.x + 26f, basin.y + 148f, basin.width - 40f, 38f),
                IsRussianUi ? "Маршрут стабилизации · 30:00" : "Stabilization route · 30:00", _label);
            var basinButtonLabel = basinSelected
                ? (IsRussianUi ? "ВЫБРАНО" : "SELECTED")
                : (IsRussianUi ? "ВЫБРАТЬ РЕГИОН" : "SELECT REGION");
            if (GUI.Button(new Rect(basin.x + 20f, basin.yMax - 58f, basin.width - 40f, 42f), basinButtonLabel, _button))
                _selectedRegion = EchoRegionId.ResonanceBasin;

            GUI.Box(rift, GUIContent.none, _box);
            GUI.color = new Color(0.66f, 0.52f, 1f, riftAvailable ? 0.95f : 0.55f);
            GUI.DrawTexture(new Rect(rift.x + 12f, rift.y + 12f, 3f, rift.height - 24f), _uiAccentTexture);
            GUI.color = previousColor;
            GUI.Label(new Rect(rift.x + 26f, rift.y + 18f, rift.width - 40f, 20f),
                riftAvailable
                    ? (IsRussianUi ? "РЕГИОН 02  /  ДОСТУПЕН" : "REGION 02  /  AVAILABLE")
                    : (IsRussianUi ? "РЕГИОН 02  /  ЗАКРЫТ" : "REGION 02  /  LOCKED"), _label);
            GUI.Label(new Rect(rift.x + 26f, rift.y + 48f, rift.width - 40f, 48f),
                IsRussianUi ? "СПЕКТРАЛЬНЫЙ РАЗЛОМ" : "SPECTRAL RIFT", _title);
            GUI.Label(new Rect(rift.x + 26f, rift.y + 98f, rift.width - 40f, 45f),
                IsRussianUi ? "ЗОНА ПЕРЕПИСАННЫХ РАССТОЯНИЙ" : "ZONE OF REWRITTEN DISTANCE", _label);
            GUI.Label(new Rect(rift.x + 26f, rift.y + 148f, rift.width - 40f, 38f),
                riftAvailable
                    ? (IsRussianUi ? "Маршрут стабилизации · 30:00" : "Stabilization route · 30:00")
                    : (IsRussianUi ? "Откроется после стабилизации Резонансного Бассейна." : "Unlocks after Resonance Basin is stabilized."), _label);

            var previousEnabled = GUI.enabled;
            GUI.enabled = riftAvailable;
            var riftButtonLabel = !riftAvailable
                ? (IsRussianUi ? "ЗАКРЫТО" : "LOCKED")
                : riftSelected
                    ? (IsRussianUi ? "ВЫБРАНО" : "SELECTED")
                    : (IsRussianUi ? "ВЫБРАТЬ РЕГИОН" : "SELECT REGION");
            if (GUI.Button(new Rect(rift.x + 20f, rift.yMax - 58f, rift.width - 40f, 42f), riftButtonLabel, _button) && riftAvailable)
                _selectedRegion = EchoRegionId.SpectralRift;
            GUI.enabled = previousEnabled;

            GUI.color = new Color(0.60f, 0.80f, 1f, 0.6f);
            GUI.DrawTexture(new Rect(panel.x + 26f, panel.yMax - 104f, panel.width - 52f, 1f), _uiAccentTexture);
            GUI.color = previousColor;
            GUI.Label(new Rect(panel.x + 142f, panel.yMax - 96f, panel.width - 284f, 22f),
                IsRussianUi
                    ? "Регион влияет на состав врагов и расписание боссов."
                    : "Region changes enemy composition and boss schedule.", _label);

            var actionWidth = Mathf.Min(330f, panel.width - 80f);
            var selectedName = _selectedRegion == EchoRegionId.SpectralRift
                ? (IsRussianUi ? "СПЕКТРАЛЬНЫЙ РАЗЛОМ" : "SPECTRAL RIFT")
                : (IsRussianUi ? "РЕЗОНАНСНЫЙ БАССЕЙН" : "RESONANCE BASIN");
            if (GUI.Button(new Rect(panel.center.x - actionWidth * 0.5f, panel.yMax - 70f, actionWidth, 42f),
                IsRussianUi ? "НАЧАТЬ: " + selectedName : "START: " + selectedName, _button))
                BeginSelectedRegionRun();

            if (GUI.Button(new Rect(panel.x + 18f, panel.yMax - 68f, 110f, 36f),
                IsRussianUi ? "НАЗАД" : "BACK", _button))
                _regionSelect = false;
        }

        private void BeginSelectedRegionRun()
        {
            if (!IsRegionAvailable(_selectedRegion))
            {
                _selectedRegion = EchoRegionId.ResonanceBasin;
                ShowMessage(IsRussianUi ? "СНАЧАЛА СТАБИЛИЗИРУЙ РЕЗОНАНСНЫЙ БАССЕЙН" : "STABILIZE RESONANCE BASIN FIRST");
                return;
            }

            PlayerPrefs.SetInt(SelectedRegionPreferenceKey, (int)_selectedRegion);
            PlayerPrefs.Save();
            _regionSchedule = RegionBossScheduleRules.Create(_selectedRegion, EchoRegionMode.Stabilization);
            _regionStabilized = false;
            _regionFieldVisuals?.SetRegion(_selectedRegion);
            _regionSelect = false;
            _mainMenuPage = MainMenuPage.Home;
            _mainMenu = false;
            _paused = false;
            _userPaused = false;
            _messageTimer = 0f;
            _runTime = 0f;
            _kills = 0;
            _bossesDefeated = 0;
            _runEliteKills = 0;
            _runEvolutions = 0;
            _runChestOpens = 0;
            _runResultSaved = false;
            _runGoldEarned = 0;
            _wave = 0;
            _waveEnemiesToSpawn = 0;
            _spawnTimer = 3f;
            StartNextWave();
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
            var scale = Mathf.Max(1f, _uiScale);
            var point = new Vector2(screenPoint.x / scale, _uiHeight - screenPoint.y / scale);
            if (new Rect(18f, 18f, 310f, 210f).Contains(point)) return true;

            var activeCount = Mathf.Min(_spheres.Count, 10);
            if (activeCount > 0)
            {
                var rosterWidth = activeCount * 56f + 12f;
                if (new Rect(_uiWidth - rosterWidth - 18f, 18f, rosterWidth, 110f).Contains(point)) return true;
            }

            if (GetSpherePickerRect().Contains(point)) return true;
            var bottomY = _uiHeight - 76f;
            if (new Rect(18f, bottomY, 215f, 54f).Contains(point)) return true;
            if (new Rect(_uiWidth - 176f, bottomY, 158f, 54f).Contains(point)) return true;
            if (new Rect(_uiWidth - 158f, 140f, 140f, 36f).Contains(point)) return true;
            return false;
        }

        private float GetSpherePickerDescriptionHeight()
        {
            var definition = GameCatalog.Spheres[(int)_selectedSphereType];
            // The source description expands naturally for Orbital's multi-line
            // text instead of clipping its last lines.
            var lines = Mathf.Max(1, Mathf.CeilToInt(definition.DescriptionEn.Length / 42f));
            return Mathf.Max(43f, 21f + lines * 12f);
        }

        private Rect GetSpherePickerRect()
        {
            // Source MobileControls uses a 5-column grid of compact type glyphs,
            // with the selected type's name/description directly above it.
            var width = Mathf.Min(252f, _uiWidth - 36f);
            var height = GetSpherePickerDescriptionHeight() + 103f;
            var bottomControlsY = _uiHeight - 76f;
            return new Rect(18f, bottomControlsY - height - 10f, width, height);
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
            if (_mainMenu) return;
            if (_paused || _gameOver || _levelUp || _choosingEvolution || _stellaRewardPending)
            {
                if (_spherePointerActive) ClearSpherePointer();
                return;
            }

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (!_spherePointerActive && touch.phase == TouchPhase.Began)
                    BeginSpherePointer(touch.position, true, touch.fingerId);

                if (_spherePointerActive && _spherePointerFromTouch && touch.fingerId == _spherePointerTouchId)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        MoveSpherePointer(touch.position);
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        FinishSpherePointer(touch.position);
                }
                return;
            }

            if (_spherePointerActive && _spherePointerFromTouch) ClearSpherePointer();
            if (Input.GetMouseButtonDown(0)) BeginSpherePointer(Input.mousePosition, false, -1);
            if (_spherePointerActive && !_spherePointerFromTouch)
            {
                if (Input.GetMouseButton(0)) MoveSpherePointer(Input.mousePosition);
                if (Input.GetMouseButtonUp(0))
                {
                    MoveSpherePointer(Input.mousePosition);
                    FinishSpherePointer(Input.mousePosition);
                }
            }
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
            _spherePointerTarget = null;
            _spherePointerPositionValid = false;
            _spherePointerPreview = null;
            _spherePointerPreviewIndex = -1;
            _touchStart = screenPoint;

            CaptureSpherePointerOriginalFormation();
            var hit = FindSphereAtWorld(_spherePointerWorld, 34f * SpherePlacementRules.SourcePixelsToWorld);
            if (hit != null)
            {
                if (!_sphereMovementLocked) _spherePointerTarget = hit;
                return;
            }

            _spherePointerIsMovement = screenPoint.x >= Screen.width * 0.5f;
            _spherePointerIsPlacing = !_spherePointerIsMovement
                && _spheres.Count < GetCurrentMaxSpheres();
        }

        private void MoveSpherePointer(Vector2 screenPoint)
        {
            if (!_spherePointerActive) return;
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
                    ShowMessage(GameCatalog.Spheres[(int)target.Type].DisplayName + " moved.");
                }
                else
                {
                    ShowMessage("Invalid position.");
                }
            }
            else if (placing)
            {
                if (_spheres.Count < GetCurrentMaxSpheres() && CanPlaceSphereAt(world, null))
                    TryPlaceOrRemoveSphereAt(world);
                else
                    ShowMessage("Path blocked.");
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
                var index = formation.NodeIndexes[i];
                if (index < 0 || index >= _spheres.Count || _spheres[index] == null) continue;
                _spherePointerOriginalFormationPoints[i] = _spheres[index].transform.position;
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
            SpherePlacementRules.GetMaxSpheres(_maxSphereAbilityLevel, PersistentUpgradeRules.GetExtraSpheres(GetShopUpgradeLevel("spheres")));

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
            ShowMessage(GameCatalog.Spheres[(int)_selectedSphereType].DisplayName + " placed.");
        }

        private void RemoveSphereAgent(SphereAttackAgent sphere)
        {
            if (sphere == null) return;
            var removedType = sphere.Type;
            _spheres.Remove(sphere);
            if (sphere.gameObject != null) Destroy(sphere.gameObject);
            RefreshNetworkState();
            CaptureSphereFormationMemoryIfChanged();
            ShowMessage(GameCatalog.Spheres[(int)removedType].DisplayName + " removed.");
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
            var descriptionRect = new Rect(panel.x, panel.y, panel.width, descriptionHeight);
            GUI.Box(descriptionRect, GUIContent.none, _box);
            var titleStyle = new GUIStyle(_label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip
            };
            var descriptionStyle = new GUIStyle(_sphereDescriptionStyle ?? _label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                clipping = TextClipping.Clip
            };
            GUI.Label(new Rect(descriptionRect.x + 4f, descriptionRect.y + 1f, descriptionRect.width - 8f, 17f),
                definition.DisplayName, titleStyle);
            GUI.Label(new Rect(descriptionRect.x + 5f, descriptionRect.y + 17f, descriptionRect.width - 10f, descriptionRect.height - 17f),
                definition.DescriptionEn, descriptionStyle);

            var buttonWidth = (panel.width - 12f) / 5f;
            const float buttonHeight = 42f;
            for (var i = 0; i < SpherePickerOrder.Length; i++)
            {
                var type = SpherePickerOrder[i];
                var column = i % 5;
                var row = i / 5;
                var rect = new Rect(panel.x + column * (buttonWidth + 3f),
                    panel.y + descriptionHeight + 5f + row * 43f, buttonWidth, buttonHeight);
                var color = SphereUiColor(type);
                if (GUI.Button(rect, GUIContent.none, _button)) _selectedSphereType = type;

                DrawSpherePickerGlyph(rect, type, color);
                var preferredForSpherist = type == SphereId.Standard || type == SphereId.Orbital;
                if (preferredForSpherist)
                {
                    // Port of CHARACTER_DEFS.spherist.preferredSphereTypes.
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
                    new Vector2(center.x, center.y - 8f), new Vector2(center.x + 8f, center.y),
                    new Vector2(center.x, center.y + 8f), new Vector2(center.x - 8f, center.y)
                };
                for (var i = 0; i < points.Length; i++)
                    DrawSphereGuiLine(points[i], points[(i + 1) % points.Length], color, 2f);
                return;
            }

            if (type == SphereId.Shotgun)
            {
                var chamber = new Rect(center.x - 10f, center.y - 4f, 12f, 8f);
                GUI.color = fill;
                GUI.DrawTexture(chamber, _uiAccentTexture);
                GUI.color = color;
                DrawSphereGuiLine(new Vector2(chamber.xMin, chamber.yMin), new Vector2(chamber.xMax, chamber.yMin), color, 2f);
                DrawSphereGuiLine(new Vector2(chamber.xMax, chamber.yMin), new Vector2(chamber.xMax, chamber.yMax), color, 2f);
                DrawSphereGuiLine(new Vector2(chamber.xMax, chamber.yMax), new Vector2(chamber.xMin, chamber.yMax), color, 2f);
                DrawSphereGuiLine(new Vector2(chamber.xMin, chamber.yMax), new Vector2(chamber.xMin, chamber.yMin), color, 2f);
                DrawSphereGuiLine(new Vector2(center.x + 2f, center.y - 8f), new Vector2(center.x + 10f, center.y - 8f), color, 2f);
                DrawSphereGuiLine(new Vector2(center.x + 10f, center.y - 8f), new Vector2(center.x + 10f, center.y + 8f), color, 2f);
                DrawSphereGuiLine(new Vector2(center.x + 10f, center.y + 8f), new Vector2(center.x + 2f, center.y + 8f), color, 2f);
                GUI.color = originalGuiColor;
                return;
            }

            if (type == SphereId.Chain)
            {
                for (var i = 0; i < 3; i++)
                {
                    var node = new Vector2(center.x + (i - 1) * 12f, center.y);
                    DrawSpriteIcon(new Rect(node.x - 5f, node.y - 5f, 10f, 10f), RuntimeSpriteFactory.Disc, fill);
                    DrawSphereGuiCircle(node, 5f, color, 2f);
                }
                return;
            }

            if (type == SphereId.Aura)
            {
                DrawSphereGuiCircle(center, 13f, new Color(color.r, color.g, color.b, 0.13f), 1.5f);
                DrawSphereGuiCircle(center, 10f, color, 2f);
                return;
            }

            // Source glyphs for Standard, Orbital, Prism, Gravity, Pulse and Void
            // share the same filled circular mark.
            var defaultFill = new Color(color.r, color.g, color.b, 0.27f);
            DrawSpriteIcon(new Rect(center.x - 8f, center.y - 8f, 16f, 16f), RuntimeSpriteFactory.Disc, defaultFill);
            DrawSphereGuiCircle(center, 8f, color, 2f);
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

            GUI.color = previousColor;
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

        private void OnGUI()
        {
            var previousMatrix = GUI.matrix;
            _uiScale = Mathf.Clamp(Screen.height / 720f, 1f, 1.5f);
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
                }

                if (_mainMenu)
                {
                    if (_regionSelect) DrawRegionSelect();
                    else DrawMainMenu();
                    return;
                }

                DrawHudBackdrop();

                var hp = _playerCore == null ? 0f : _playerCore.CurrentHp;
                var maxHp = _playerCore == null ? 100f : _playerCore.MaxHp;
                var hpRatio = maxHp <= 0f ? 0f : Mathf.Clamp01(hp / maxHp);
                var xpRatio = _xpRequired <= 0 ? 0f : Mathf.Clamp01(_xp / (float)_xpRequired);
                var resonanceRatio = Mathf.Clamp01(_resonanceCharge / ResonanceRules.BaseCap);

                GUI.Box(new Rect(18, 18, 310, 210), GUIContent.none, _box);
                GUI.Label(new Rect(32, 25, 276, 28), "ECHO SPHERE  /  CORE SYSTEM", _title);
                GUI.Label(new Rect(32, 56, 270, 20), $"CORE INTEGRITY   {Mathf.CeilToInt(hp)} / {Mathf.CeilToInt(maxHp)}", _label);
                var hpColor = hpRatio > 0.55f
                    ? new Color(0.25f, 0.92f, 0.73f, 1f)
                    : hpRatio > 0.25f ? new Color(1f, 0.66f, 0.25f, 1f) : new Color(1f, 0.28f, 0.39f, 1f);
                DrawProgressBar(new Rect(32, 79, 270, 9), hpRatio, hpColor);

                GUI.Label(new Rect(32, 95, 278, 20), $"LV {_level}     KILLS {_kills}     {FormatTime(_runTime)}", _label);
                GUI.Label(new Rect(32, 116, 270, 18), $"EXPERIENCE   {_xp} / {_xpRequired}", _label);
                DrawProgressBar(new Rect(32, 136, 270, 8), xpRatio, new Color(0.58f, 0.48f, 1f, 1f));

                GUI.Label(new Rect(32, 149, 276, 18), $"RESONANCE   {Mathf.FloorToInt(_resonanceCharge)} / 100   SHIELD {_playerCore?.ShieldCharges ?? 0}", _label);
                var resonanceColor = Color.Lerp(new Color(0.25f, 0.86f, 1f, 1f), new Color(1f, 0.82f, 0.33f, 1f), resonanceRatio);
                DrawProgressBar(new Rect(32, 169, 270, 8), resonanceRatio, resonanceColor);

                var geometryLabel = _networkState != null && _networkState.DominantFormation != null
                    ? "GEOMETRY " + _networkState.DominantFormation.Type.ToString().ToUpperInvariant()
                    : "NETWORK DISCONNECTED";
                if (GUI.Button(new Rect(32, 188, 132, 24), "MAIN: " + geometryLabel.Replace("GEOMETRY ", ""), _button))
                    CycleDominantFormation();
                var secondaryLabel = _networkState != null && _networkState.SecondaryFormation != null
                    ? _networkState.SecondaryFormation.Type.ToString().ToUpperInvariant() : "AUTO";
                if (GUI.Button(new Rect(170, 188, 132, 24), "SUB: " + secondaryLabel, _button))
                    CycleSecondaryFormation();

                var bottomY = _uiHeight - 76f;
                if (GUI.Button(new Rect(18f, bottomY, 105f, 54f), _sphereMovementLocked ? "SPHERES LOCKED" : "MOVE SPHERES", _button))
                    _sphereMovementLocked = !_sphereMovementLocked;
                if (GUI.Button(new Rect(127f, bottomY, 106f, 54f), _follow ? "FOLLOW ON" : "FOLLOW OFF", _button))
                    SetFormationFollowMode(!_follow);
                if (GUI.Button(new Rect(_uiWidth - 176f, bottomY, 158f, 54f), "DASH", _button))
                    _playerCore.TryDash(_playerCore.LastMoveDirection);
                if (!_gameOver && !_levelUp && !_choosingEvolution && !_stellaRewardPending &&
                    GUI.Button(new Rect(_uiWidth - 158f, 140f, 140f, 36f), _userPaused ? "RESUME" : "PAUSE", _button))
                    SetUserPaused(!_userPaused);

                DrawSphereRoster();
                DrawSpherePicker();
                DrawSpherePlacementOverlay();
                if (!string.IsNullOrEmpty(_message) && _messageTimer > 0f)
                    GUI.Label(new Rect(18f, 236f, Mathf.Min(_uiWidth - 36f, 520f), 30f), _message, _label);

                if (_stellaRewardPending)
                {
                    var count = _stellaRewardChoices.Count;
                    var panelWidth = Mathf.Min(660f, _uiWidth - 32f);
                    var panelHeight = 88f + count * 78f;
                    var panelLeft = (_uiWidth - panelWidth) * 0.5f;
                    var panelTop = (_uiHeight - panelHeight) * 0.5f;
                    GUI.Box(new Rect(panelLeft, panelTop, panelWidth, panelHeight), GUIContent.none, _box);
                    GUI.Label(new Rect(panelLeft + 24f, panelTop + 18f, panelWidth - 48f, 30f),
                        _stellaRewardIsLegendary ? "STELLA / LEGENDARY ARTIFACT" : "STELLA / ARTIFACT CHOICE", _title);
                    for (var i = 0; i < count; i++)
                    {
                        var choice = _stellaRewardChoices[i];
                        var rect = new Rect(panelLeft + 22f, panelTop + 62f + i * 78f, panelWidth - 44f, 70f);
                        if (GUI.Button(rect, GUIContent.none, _button)) ChooseStellaReward(i);
                        GUI.Label(new Rect(rect.x + 12f, rect.y + 5f, rect.width - 24f, 24f),
                            choice.NameEnglish + "  /  " + choice.Rarity.ToString().ToUpperInvariant(), _label);
                        GUI.Label(new Rect(rect.x + 12f, rect.y + 29f, rect.width - 24f, 36f),
                            choice.DescriptionEnglish, _button);
                    }
                }

                if (_levelUp)
                {
                    var left = _uiWidth * 0.5f - 220f;
                    var top = _uiHeight * 0.5f - 170f;
                    GUI.Box(new Rect(left, top, 440f, 340f), GUIContent.none, _box);
                    GUI.Label(new Rect(left + 36f, top + 25f, 368f, 32f), _choosingEvolution
                        ? (_pendingFinalEvolution ? "FINAL EVOLUTION / CHOOSE ONE" : "MUTATION BRANCH / CHOOSE ONE")
                        : "LEVEL UP / CHOOSE ONE", _title);
                    if (_choosingEvolution)
                    {
                        for (var i = 0; i < _evolutionChoices.Count; i++)
                        {
                            var yOffset = 70f + i * 78f;
                            var choice = _evolutionChoices[i];
                            var rect = new Rect(left + 35f, top + yOffset, 370f, 64f);
                            if (GUI.Button(rect, GUIContent.none, _button)) ChooseEvolution(i);
                            DrawSpriteIcon(new Rect(rect.x + 10f, rect.y + 10f, 42f, 42f), GetSphereIcon(_pendingEvolutionSphere), SphereColor(_pendingEvolutionSphere));
                            GUI.Label(new Rect(rect.x + 60f, rect.y + 8f, 298f, 25f), choice.Name, _label);
                            GUI.Label(new Rect(rect.x + 60f, rect.y + 31f, 298f, 29f), choice.Description, _button);
                        }
                    }
                    else
                    {
                        for (var i = 0; i < _levelUpChoices.Count; i++)
                        {
                            var yOffset = 70f + i * 78f;
                            var choice = _levelUpChoices[i];
                            var rect = new Rect(left + 35f, top + yOffset, 370f, 64f);
                            if (GUI.Button(rect, GUIContent.none, _button)) ChooseUpgrade(i);
                            var icon = choice.Kind == RunUpgradeKind.Sphere ? GetSphereIcon(choice.Sphere)
                                : choice.Kind == RunUpgradeKind.AttackSpeed || choice.Kind == RunUpgradeKind.MoveSpeed ? RuntimeSpriteFactory.Star
                                : choice.Kind == RunUpgradeKind.Vitality || choice.Kind == RunUpgradeKind.Regen ? RuntimeSpriteFactory.Hexagon
                                : RuntimeSpriteFactory.Faceted;
                            var tint = choice.Kind == RunUpgradeKind.Sphere ? Color.white
                                : choice.Kind == RunUpgradeKind.AttackSpeed || choice.Kind == RunUpgradeKind.Crit ? new Color(1f, 0.78f, 0.28f)
                                : choice.Kind == RunUpgradeKind.Vitality || choice.Kind == RunUpgradeKind.Regen ? new Color(0.35f, 1f, 0.72f)
                                : new Color(0.28f, 0.88f, 1f);
                            DrawSpriteIcon(new Rect(rect.x + 10f, rect.y + 10f, 42f, 42f), icon, tint);
                            GUI.Label(new Rect(rect.x + 60f, rect.y + 8f, 298f, 25f), choice.Title, _label);
                            GUI.Label(new Rect(rect.x + 60f, rect.y + 31f, 298f, 29f), choice.Description, _button);
                        }
                    }
                }

                if (_userPaused)
                {
                    var pauseLeft = _uiWidth * 0.5f - 190f;
                    var pauseTop = _uiHeight * 0.5f - 105f;
                    GUI.Box(new Rect(pauseLeft, pauseTop, 380f, 210f), GUIContent.none, _box);
                    GUI.Label(new Rect(pauseLeft + 40f, pauseTop + 25f, 300f, 34f), "RUN PAUSED", _title);
                    GUI.Label(new Rect(pauseLeft + 40f, pauseTop + 62f, 300f, 28f), "Your run is safe. Resume when ready.", _label);
                    if (GUI.Button(new Rect(pauseLeft + 30f, pauseTop + 112f, 150f, 52f), "RESUME", _button))
                        SetUserPaused(false);
                    if (GUI.Button(new Rect(pauseLeft + 200f, pauseTop + 112f, 150f, 52f), "RESTART RUN", _button))
                    {
                        _userPaused = false;
                        _paused = false;
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    }
                }

                if (_gameOver)
                {
                    var left = _uiWidth * 0.5f - 210f;
                    var top = _uiHeight * 0.5f - 112f;
                    GUI.Box(new Rect(left, top, 420f, 224f), GUIContent.none, _box);
                    GUI.Label(new Rect(left + 30f, top + 20f, 360f, 32f),
                        _regionStabilized
                            ? (IsRussianUi ? "РЕГИОН СТАБИЛИЗИРОВАН" : "REGION STABILIZED")
                            : (IsRussianUi ? "ЯДРО РАЗРУШЕНО" : "CORE DESTROYED"), _title);
                    GUI.Label(new Rect(left + 30f, top + 58f, 360f, 28f),
                        _regionStabilized
                            ? (IsRussianUi ? $"Бассейн сохранён · боссов: {_bossesDefeated}" : $"Region secured · bosses: {_bossesDefeated}")
                            : (IsRussianUi ? $"Выживание {FormatTime(_runTime)} · убийств: {_kills}" : $"Survived {FormatTime(_runTime)} · kills: {_kills}"), _label);
                    GUI.Label(new Rect(left + 30f, top + 88f, 360f, 24f),
                        (IsRussianUi ? "ЗОЛОТО ПОЛУЧЕНО" : "GOLD EARNED") + ": " + _runGoldEarned, _label);
                    if (GUI.Button(new Rect(left + 24f, top + 142f, 176f, 46f),
                        IsRussianUi ? "В МЕНЮ" : "MAIN MENU", _button))
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    if (GUI.Button(new Rect(left + 220f, top + 142f, 176f, 46f),
                        IsRussianUi ? "НАЧАТЬ ЗАНОВО" : "RESTART", _button))
                    {
                        PlayerPrefs.SetInt("echo-sphere:start_selected_region_after_reload", 1);
                        PlayerPrefs.Save();
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    }
                }
            }
            finally
            {
                GUI.matrix = previousMatrix;
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
