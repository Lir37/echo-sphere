using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoSphere.Runtime
{
    /// <summary>First-playable migration harness; intentionally not the full production game.</summary>
    public sealed class EchoSphereRuntime : MonoBehaviour
    {
        private readonly List<EnemyAgent2D> _enemies = new List<EnemyAgent2D>();
        private readonly List<SphereAttackAgent> _spheres = new List<SphereAttackAgent>();
        private readonly Dictionary<SphereId, int> _sphereLevels = new Dictionary<SphereId, int>();
        private readonly List<RunUpgradeChoice> _levelUpChoices = new List<RunUpgradeChoice>();
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
        private float _spawnTimer = 0.6f, _runTime, _messageTimer, _formationStrain;
        private Vec2 _lastDirection = Vec2.Zero;
        private int _kills, _level = 1, _xp, _xpRequired = RunBalanceRules.GetXpToNextLevel(1);
        private bool _follow, _paused, _levelUp, _gameOver, _userPaused;
        private float _damageMultiplier = 1f, _attackSpeedMultiplier = 1f;
        private float _resonanceCharge, _resonanceRingTimer, _resonanceRingPulseTimer;
        private int _resonanceEventsTriggered, _resonanceRingCursor, _resonanceLineBurst;
        private bool _resonanceEventActive;
        private readonly HashSet<string> _resonanceKnownFormationKeys = new HashSet<string>();
        private readonly List<int> _resonanceRingNodes = new List<int>();
        private string _message = "Drag on the left half to move. Tap DASH to evade.";
        private GUIStyle _box, _label, _button, _title;
        private readonly List<LineRenderer> _networkLinkRenderers = new List<LineRenderer>();
        private Material _networkLineMaterial;
        private SphereNetworkState _networkState;
        private SphereNetworkFormation _previousDominantFormation = SphereNetworkFormation.None;
        private SphereNetworkFormation _preferredDominantFormation = SphereNetworkFormation.None;
        private SphereNetworkFormation _preferredSecondaryFormation = SphereNetworkFormation.None;
        private float _networkRefreshTimer;

        public bool IsGameplayPaused => _paused;
        public float SphereDamageMultiplier => _damageMultiplier;
        public float AttackSpeedMultiplier => _attackSpeedMultiplier;
        public float ResonanceCharge => _resonanceCharge;
        public int ResonanceEventsTriggered => _resonanceEventsTriggered;

        public string GetSphereBranch(SphereId type) => _sphereBranches.TryGetValue(type, out var id) ? id : null;
        public string GetSphereFinal(SphereId type) => _sphereFinals.TryGetValue(type, out var id) ? id : null;

        private void Start()
        {
            Application.targetFrameRate = 60;
            ConfigureCamera();
            CreateStarfield();
            CreatePlayer();
            CreateSpheres();
            _follow = false;
            SetFormationFollowMode(true, true);
        }

        private void Update()
        {
            PruneDestroyed();
            if (Input.GetKeyDown(KeyCode.Escape) && !_gameOver && !_levelUp && !_choosingEvolution)
                SetUserPaused(!_userPaused);
            if (_paused || _playerCore == null) return;
            var dt = Time.deltaTime;
            _runTime += dt;
            _playerCore.Tick(dt);
            if (_messageTimer > 0f) _messageTimer -= dt;
            if (Input.GetKeyDown(KeyCode.Space)) _playerCore.TryDash(_playerCore.LastMoveDirection);

            var input = ReadMovement();
            if (input.sqrMagnitude > 0.0001f) _playerCore.RememberMoveDirection(input);
            var requested = new Vec2(input.x, input.y);
            var followMultiplier = _follow ? FormationFollowRules.GetMovementMultiplier(true, _formationStrain, _lastDirection, requested) : 1f;
            if (_follow) _formationStrain = FormationFollowRules.UpdateStrain(_formationStrain, _lastDirection, requested, dt, out _lastDirection);
            else { _formationStrain = 0f; _lastDirection = Vec2.Zero; }

            _player.position += (Vector3)(_playerCore.GetVelocity(input, 3.6f, followMultiplier) * dt);
            _networkRefreshTimer -= dt;
            if (_networkRefreshTimer <= 0f)
            {
                RefreshNetworkState();
                _networkRefreshTimer = 0.12f;
            }
            TickResonanceRing(dt);
            var cameraTarget = new Vector3(_player.position.x, _player.position.y, -10f);
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, cameraTarget, 1f - Mathf.Exp(-5f * dt));
            _spawnTimer -= dt;
            if (_spawnTimer <= 0f)
            {
                if (_enemies.Count < 60) SpawnEnemy();
                _spawnTimer = Mathf.Max(0.42f, 1.25f - _runTime * 0.008f);
            }
            if (_playerCore.IsDead && !_gameOver)
            {
                _gameOver = true;
                _paused = true;
                ShowMessage("Core destroyed.");
            }
        }

        private Vector2 ReadMovement()
        {
            var direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (direction.sqrMagnitude > 0.0001f) return direction.normalized;
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began) _touchStart = touch.position;
                if (_touchStart.x < Screen.width * 0.58f && touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                {
                    var drag = touch.position - _touchStart;
                    if (drag.sqrMagnitude > 100f) return Vector2.ClampMagnitude(drag / 90f, 1f);
                }
            }
            if (Input.GetMouseButtonDown(0)) _touchStart = Input.mousePosition;
            if (Input.GetMouseButton(0) && _touchStart.x < Screen.width * 0.58f)
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
            for (var i = 0; i < 52; i++)
            {
                var go = new GameObject("Background Star");
                go.transform.position = new Vector3(_rng.NextFloat() * 26f - 13f, _rng.NextFloat() * 26f - 13f, 2f);
                go.transform.localScale = Vector3.one * (0.025f + _rng.NextFloat() * 0.045f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = RuntimeSpriteFactory.Disc;
                sr.color = new Color(0.31f, 0.65f, 1f, 0.28f);
                sr.sortingOrder = -20;
            }
        }

        private void CreatePlayer()
        {
            var tint = new Color(0.2f, 0.91f, 1f);
            var go = CreateOrb("Core", Vector2.zero, 1f, tint, Color.white, 10);
            ApplyShellVisual(go, RuntimeSpriteFactory.Faceted, 1.06f);
            var core = go.transform.Find("Core Light");
            if (core != null) core.localScale = Vector3.one * 0.25f;
            AddOrbitRail(go.transform, tint, new Vector3(1.82f, 0.72f, 1f), -28f, 7);
            AddOrbitRail(go.transform, new Color(0.43f, 0.76f, 1f, 0.8f), new Vector3(1.45f, 0.54f, 1f), 42f, 8);
            _player = go.transform;
            _playerCore = go.AddComponent<PlayerCoreController2D>();
        }

        private void CreateSpheres()
        {
            _sphereLevels[SphereId.Standard] = 1;
            CreateSphere(SphereId.Standard, 1);
        }

        private void CreateSphere(SphereId type, int level)
        {
            var angle = _spheres.Count * 2.3999632f;
            var radius = 0.9f + 0.12f * (_spheres.Count % 3);
            var position = (Vector2)_player.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            var definition = GameCatalog.Spheres[(int)type];
            var go = CreateOrb(definition.DisplayName + " Sphere", position, 0.67f, SphereColor(type), Color.white, 8);
            ApplySphereVisual(go, type);
            var agent = go.AddComponent<SphereAttackAgent>();
            agent.Initialize(this, _player, type, 12f);
            agent.ApplyProgressionLevel(level);
            _spheres.Add(agent);
            if (_follow) agent.SetFollowMode(true, (Vector2)_player.position);
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
            shell.GetComponent<SpriteRenderer>().sprite = sprite;
            shell.localScale = Vector3.one * scale;
        }

        private void ApplySphereVisual(GameObject root, SphereId type)
        {
            var shell = root.transform.Find("Shell");
            var core = root.transform.Find("Core Light");
            if (shell == null || core == null) return;
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
            if (type == SphereId.Orbital)
            {
                AddOrbitRail(root.transform, SphereColor(type), new Vector3(1.75f, 0.7f, 1f), -34f, 6);
                AddOrbitRail(root.transform, new Color(0.8f, 0.98f, 1f, 0.85f), new Vector3(1.5f, 0.58f, 1f), 36f, 7);
            }
            else if (type == SphereId.Gravity || type == SphereId.Void)
                AddOrbitRail(root.transform, SphereColor(type), new Vector3(1.55f, 0.62f, 1f), type == SphereId.Gravity ? 28f : -22f, 6);
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

        private static Sprite GetSphereIcon(SphereId type)
        {
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
            var core = new GameObject("Core Light");
            core.transform.SetParent(root.transform, false);
            core.transform.localScale = Vector3.one * 0.30f;
            var coreRenderer = core.AddComponent<SpriteRenderer>();
            coreRenderer.sprite = RuntimeSpriteFactory.Disc;
            coreRenderer.color = coreTint;
            coreRenderer.sortingOrder = order + 1;
            return root;
        }

        private void SpawnEnemy()
        {
            var angle = _rng.NextFloat() * Mathf.PI * 2f;
            var radius = 8f + _rng.NextFloat() * 2.8f;
            var position = (Vector2)_player.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            var enemyTint = new Color(1f, 0.19f, 0.34f);
            var go = CreateOrb("Echo Wraith", position, 0.72f, enemyTint, new Color(1f, 0.7f, 0.75f), 4);
            ApplyShellVisual(go, RuntimeSpriteFactory.Shard, 1.12f);
            var enemyCore = go.transform.Find("Core Light");
            if (enemyCore != null) enemyCore.localScale = Vector3.one * 0.22f;
            AddOrbitRail(go.transform, new Color(1f, 0.16f, 0.35f, 0.65f), new Vector3(1.7f, 0.62f, 1f), 38f, 3);
            var enemy = go.AddComponent<EnemyAgent2D>();
            enemy.Initialize(this, _player, 34f + _runTime * 0.3f, 0.72f + Mathf.Min(0.55f, _runTime * 0.003f));
            _enemies.Add(enemy);
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

        public void SpawnProjectile(Vector2 position, Vector2 direction, float damage, Color color, float speed = 8f, int pierce = 0, float critChanceBonus = 0f, SphereAttackAgent evolutionOwner = null)
        {
            var go = new GameObject("Sphere Projectile");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.20f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSpriteFactory.Disc;
            sr.color = color;
            sr.sortingOrder = 12;
            var glow = new GameObject("Projectile Glow");
            glow.transform.SetParent(go.transform, false);
            glow.transform.localScale = Vector3.one * 2.2f;
            var gs = glow.AddComponent<SpriteRenderer>();
            gs.sprite = RuntimeSpriteFactory.Disc;
            gs.color = new Color(color.r, color.g, color.b, 0.34f);
            gs.sortingOrder = 11;
            go.AddComponent<ProjectileAgent>().Initialize(this, direction, damage, color, speed, pierce, critChanceBonus, evolutionOwner);
        }


        public bool RollCombatChance(float chance) => _rng.NextFloat() < Mathf.Clamp01(chance);

        public void HealPlayer(float amount)
        {
            if (_playerCore != null && !_playerCore.IsDead) _playerCore.Heal(Mathf.Max(0f, amount));
        }

        public void RegisterSphereHit(SphereAttackAgent owner, EnemyAgent2D target, float damage)
        {
            if (owner == null || _resonanceEventActive) return;
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

        public void TriggerPrismRicochet(EnemyAgent2D primary, Vector2 incomingDirection, float damage, int bounceCount)
        {
            if (primary == null || bounceCount <= 0) return;
            var origin = (Vector2)primary.transform.position;
            var candidates = FindNearestEnemies(origin, 6f, 12);
            var hits = 0;
            for (var i = 0; i < candidates.Count && hits < bounceCount; i++)
            {
                var target = candidates[i];
                if (target == primary) continue;
                var multiplier = hits == 0 ? 0.30f : 0.24f;
                target.ReceiveDamage(damage * multiplier);
                SpawnImpact(Vector2.Lerp(origin, target.transform.position, 0.5f), new Color(1f, 0.55f, 0.88f, 0.9f));
                hits++;
            }
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

        public void TriggerChainStorm(EnemyAgent2D primary, float directDamage, int level, string finalId)
        {
            if (primary == null) return;
            var center = (Vector2)primary.transform.position;
            var radius = SphereEvolutionCombatRules.GetChainStormRadius(level, finalId);
            var splash = directDamage * SphereEvolutionCombatRules.GetChainStormSplash(level, finalId);
            var targets = FindEnemiesInRadius(center, radius);
            for (var i = 0; i < targets.Count; i++)
            {
                if (targets[i] == primary) continue;
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

        public void SpawnImpact(Vector2 position, Color color)
        {
            var go = new GameObject("Impact Pulse");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.2f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSpriteFactory.Ring;
            sr.color = color;
            sr.sortingOrder = 18;
            go.AddComponent<PulseEffect2D>().Initialize(color, 0.18f, 1.1f);
        }

        public void RegisterEnemyDeath(EnemyAgent2D enemy, Vector2 position)
        {
            _enemies.Remove(enemy);
            _kills++;
            var go = new GameObject("Echo Shard");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.22f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = RuntimeSpriteFactory.Disc;
            sr.color = new Color(0.26f, 0.91f, 1f);
            sr.sortingOrder = 7;
            go.AddComponent<ExperienceOrbAgent>().Initialize(this, _player, 1);
        }

        public void AddExperience(int amount)
        {
            if (_paused || _gameOver) return;
            _xp += Mathf.Max(0, amount);
            OpenLevelUpIfReady();
        }

        private void OpenLevelUpIfReady()
        {
            if (_gameOver || _levelUp || _xp < _xpRequired) return;
            _xp -= _xpRequired;
            _level++;
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
            var critChance = Mathf.Min(CombatRules.CritHardCap, CombatRules.CritBase + Mathf.Max(0f, critChanceBonus));
            var result = CombatRules.ResolveDamage(damage, critChance, _rng.NextFloat(), CombatRules.CritMultiplierBase);
            wasCritical = result.WasCritical;
            return result.Damage;
        }

        public void TryDamagePlayer(float damage)
        {
            if (_playerCore == null || _playerCore.IsDead) return;
            var shieldsBefore = _playerCore.ShieldCharges;
            if (_playerCore.TryTakeDamage(damage)) ShowMessage("Core integrity damaged!");
            else if (_playerCore.ShieldCharges < shieldsBefore) ShowMessage("Shield charge absorbed impact!");
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
            pool.Add(new RunUpgradeChoice(RunUpgradeKind.Damage, SphereId.Standard, 0, 0, "RESONANT CORE", "+18% damage for all Spheres."));
            pool.Add(new RunUpgradeChoice(RunUpgradeKind.AttackSpeed, SphereId.Standard, 0, 0, "ACCELERATION", "+15% attack speed for the network."));
            pool.Add(new RunUpgradeChoice(RunUpgradeKind.Repair, SphereId.Standard, 0, 0, "REPAIR PROTOCOL", "Restore 35 Core HP."));

            // Use the run RNG so choices are reproducible for an equivalent run seed/state.
            while (pool.Count > 0 && _levelUpChoices.Count < 3)
            {
                var index = _rng.NextInt(pool.Count);
                _levelUpChoices.Add(pool[index]);
                pool.RemoveAt(index);
            }
        }

        private void ChooseUpgrade(int choiceIndex)
        {
            if (choiceIndex < 0 || choiceIndex >= _levelUpChoices.Count) return;
            var choice = _levelUpChoices[choiceIndex];
            switch (choice.Kind)
            {
                case RunUpgradeKind.Sphere:
                    _sphereLevels[choice.Sphere] = choice.NextLevel;
                    if (choice.CurrentLevel == 0) CreateSphere(choice.Sphere, choice.NextLevel);
                    else
                        foreach (var sphere in _spheres)
                            if (sphere != null && sphere.Type == choice.Sphere)
                                sphere.ApplyProgressionLevel(choice.NextLevel);
                    ShowMessage(choice.Title + " acquired.");
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
                case RunUpgradeKind.Damage:
                    _damageMultiplier *= 1.18f;
                    ShowMessage("Sphere damage increased.");
                    break;
                case RunUpgradeKind.AttackSpeed:
                    _attackSpeedMultiplier *= 1.15f;
                    ShowMessage("Network attack speed increased.");
                    break;
                case RunUpgradeKind.Repair:
                    _playerCore.Heal(35f);
                    ShowMessage("Core repaired.");
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
            if (_gameOver || _levelUp || _choosingEvolution) return;
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
            var index = GetSphereIndex(sphere);
            if (index < 0) return 1f;
            var multiplier = 1f
                + 0.10f * GetFormationBonus(SphereNetworkFormation.Line, index)
                + 0.15f * GetFormationBonus(SphereNetworkFormation.Fractal, index);
            if (ConsumeResonanceLineBurst()) multiplier *= 1.60f;
            return multiplier;
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
                nodes.Add(new SphereNetworkNode(new Vec2(position.x, position.y), true));
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
                var index = (Math.Max(-1, currentIndex) + offset) % _networkState.FormationCandidates.Count;
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
        }

        private void OnGUI()
        {
            if (_box == null)
            {
                _box = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, fontSize = 15, padding = new RectOffset(14, 14, 10, 10) };
                _label = new GUIStyle(GUI.skin.label) { fontSize = 15, normal = { textColor = new Color(0.82f, 0.92f, 1f) } };
                _button = new GUIStyle(GUI.skin.button) { fontSize = 14, wordWrap = true };
                _title = new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            }
            var hp = _playerCore == null ? 0f : _playerCore.CurrentHp;
            var maxHp = _playerCore == null ? 100f : _playerCore.MaxHp;
            GUI.Box(new Rect(18, 18, 285, 154), GUIContent.none, _box);
            var geometryLabel = _networkState != null && _networkState.DominantFormation != null
                ? "GEOMETRY " + _networkState.DominantFormation.Type.ToString().ToUpperInvariant()
                : "NETWORK DISCONNECTED";
            if (GUI.Button(new Rect(32, 151, 125, 20), "MAIN: " + geometryLabel.Replace("GEOMETRY ", ""), _button))
                CycleDominantFormation();
            var secondaryLabel = _networkState != null && _networkState.SecondaryFormation != null
                ? _networkState.SecondaryFormation.Type.ToString().ToUpperInvariant() : "AUTO";
            if (GUI.Button(new Rect(162, 151, 125, 20), "SUB: " + secondaryLabel, _button))
                CycleSecondaryFormation();
            GUI.Label(new Rect(32, 26, 255, 28), "ECHO SPHERE / UNITY SLICE", _title);
            GUI.Label(new Rect(32, 57, 250, 22), $"CORE {Mathf.CeilToInt(hp)} / {Mathf.CeilToInt(maxHp)}", _label);
            GUI.Box(new Rect(32, 81, 245, 10), GUIContent.none);
            GUI.Box(new Rect(32, 81, 245f * (maxHp <= 0f ? 0f : hp / maxHp), 10), GUIContent.none);
            GUI.Label(new Rect(32, 96, 250, 20), $"LV {_level}   KILLS {_kills}   {FormatTime(_runTime)}", _label);
            GUI.Label(new Rect(32, 116, 250, 18), $"RESONANCE {Mathf.FloorToInt(_resonanceCharge)} / 100   SHIELD {_playerCore?.ShieldCharges ?? 0}", _label);
            GUI.Box(new Rect(32, 137, 245, 7), GUIContent.none);
            GUI.Box(new Rect(32, 137, 245f * Mathf.Clamp01(_resonanceCharge / ResonanceRules.BaseCap), 7), GUIContent.none);
            var y = Screen.height - 76f;
            if (GUI.Button(new Rect(18, y, 215, 54), _follow ? "FORMATION FOLLOW ON" : "FORMATION FOLLOW OFF", _button))
                SetFormationFollowMode(!_follow);
            if (GUI.Button(new Rect(Screen.width - 176, y, 158, 54), "DASH", _button)) _playerCore.TryDash(_playerCore.LastMoveDirection);
            if (!_gameOver && !_levelUp && !_choosingEvolution &&
                GUI.Button(new Rect(Screen.width - 158f, 140f, 140f, 36f), _userPaused ? "RESUME" : "PAUSE", _button))
                SetUserPaused(!_userPaused);
            DrawSphereRoster();
            if (!string.IsNullOrEmpty(_message) && _messageTimer > 0f)
                GUI.Label(new Rect(18, 178, Mathf.Min(Screen.width - 36f, 520f), 30), _message, _label);
            if (_levelUp)
            {
                var left = Screen.width * 0.5f - 220f;
                var top = Screen.height * 0.5f - 170f;
                GUI.Box(new Rect(left, top, 440f, 340f), GUIContent.none, _box);
                GUI.Label(new Rect(left + 36f, top + 25f, 360f, 32f), _choosingEvolution
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
                            : choice.Kind == RunUpgradeKind.Repair ? RuntimeSpriteFactory.Hexagon
                            : choice.Kind == RunUpgradeKind.AttackSpeed ? RuntimeSpriteFactory.Star
                            : RuntimeSpriteFactory.Faceted;
                        var tint = choice.Kind == RunUpgradeKind.Sphere ? SphereColor(choice.Sphere)
                            : choice.Kind == RunUpgradeKind.Repair ? new Color(0.35f, 1f, 0.72f)
                            : choice.Kind == RunUpgradeKind.AttackSpeed ? new Color(1f, 0.78f, 0.28f)
                            : new Color(0.28f, 0.88f, 1f);
                        DrawSpriteIcon(new Rect(rect.x + 10f, rect.y + 10f, 42f, 42f), icon, tint);
                        GUI.Label(new Rect(rect.x + 60f, rect.y + 8f, 298f, 25f), choice.Title, _label);
                        GUI.Label(new Rect(rect.x + 60f, rect.y + 31f, 298f, 29f), choice.Description, _button);
                    }
                }
            }
            if (_userPaused)
            {
                var pauseLeft = Screen.width * 0.5f - 190f;
                var pauseTop = Screen.height * 0.5f - 105f;
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
                var left = Screen.width * 0.5f - 190f;
                var top = Screen.height * 0.5f - 85f;
                GUI.Box(new Rect(left, top, 380f, 170f), GUIContent.none, _box);
                GUI.Label(new Rect(left + 40f, top + 24f, 300f, 32f), "CORE DESTROYED", _title);
                GUI.Label(new Rect(left + 40f, top + 60f, 300f, 28f), $"Survived {FormatTime(_runTime)} · {_kills} kills", _label);
                if (GUI.Button(new Rect(left + 100f, top + 102f, 180f, 44f), "RESTART"))
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }


        private void DrawSphereRoster()
        {
            var count = Mathf.Min(_spheres.Count, 10);
            if (count <= 0) return;
            const float cardWidth = 56f;
            var panelWidth = count * cardWidth + 12f;
            var panel = new Rect(Screen.width - panelWidth - 18f, 18f, panelWidth, 110f);
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
