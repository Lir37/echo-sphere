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
        private bool _follow, _paused, _levelUp, _gameOver;
        private float _damageMultiplier = 1f, _attackSpeedMultiplier = 1f;
        private string _message = "Drag on the left half to move. Tap DASH to evade.";
        private GUIStyle _box, _label, _button, _title;

        public bool IsGameplayPaused => _paused;
        public float SphereDamageMultiplier => _damageMultiplier;
        public float AttackSpeedMultiplier => _attackSpeedMultiplier;

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
            var go = CreateOrb("Core", Vector2.zero, 1f, new Color(0.2f, 0.91f, 1f), Color.white, 10);
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
            var go = CreateOrb("Echo Wraith", position, 0.72f, new Color(1f, 0.19f, 0.34f), new Color(1f, 0.7f, 0.75f), 4);
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

        public EnemyAgent2D TryHitEnemy(Vector2 position, float radius)
        {
            var combined = radius + 0.27f;
            var limit = combined * combined;
            for (var i = 0; i < _enemies.Count; i++)
            {
                var e = _enemies[i];
                if (e != null && e.IsAlive && ((Vector2)e.transform.position - position).sqrMagnitude <= limit) return e;
            }
            return null;
        }

        public void SpawnProjectile(Vector2 position, Vector2 direction, float damage, Color color, float speed = 8f)
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
            go.AddComponent<ProjectileAgent>().Initialize(this, direction, damage, color, speed);
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

        public float ResolveProjectileDamage(float damage)
        {
            return CombatRules.ResolveDamage(damage, CombatRules.CritBase, _rng.NextFloat(), CombatRules.CritMultiplierBase).Damage;
        }

        public void TryDamagePlayer(float damage)
        {
            if (_playerCore != null && !_playerCore.IsDead && _playerCore.TryTakeDamage(damage)) ShowMessage("Core integrity damaged!");
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
            GUI.Box(new Rect(18, 18, 285, 110), GUIContent.none, _box);
            GUI.Label(new Rect(32, 26, 255, 28), "ECHO SPHERE / UNITY SLICE", _title);
            GUI.Label(new Rect(32, 57, 250, 22), $"CORE {Mathf.CeilToInt(hp)} / {Mathf.CeilToInt(maxHp)}", _label);
            GUI.Box(new Rect(32, 81, 245, 10), GUIContent.none);
            GUI.Box(new Rect(32, 81, 245f * (maxHp <= 0f ? 0f : hp / maxHp), 10), GUIContent.none);
            GUI.Label(new Rect(32, 96, 250, 24), $"LV {_level}   KILLS {_kills}   {FormatTime(_runTime)}", _label);
            var y = Screen.height - 76f;
            if (GUI.Button(new Rect(18, y, 215, 54), _follow ? "FORMATION FOLLOW ON" : "FORMATION FOLLOW OFF", _button))
                SetFormationFollowMode(!_follow);
            if (GUI.Button(new Rect(Screen.width - 176, y, 158, 54), "DASH", _button)) _playerCore.TryDash(_playerCore.LastMoveDirection);
            if (!string.IsNullOrEmpty(_message) && _messageTimer > 0f)
                GUI.Label(new Rect(18, 140, Mathf.Min(Screen.width - 36f, 520f), 30), _message, _label);
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
                        if (GUI.Button(new Rect(left + 35f, top + yOffset, 370f, 64f), choice.Name + "\n" + choice.Description, _button))
                            ChooseEvolution(i);
                    }
                }
                else
                {
                    for (var i = 0; i < _levelUpChoices.Count; i++)
                    {
                        var yOffset = 70f + i * 78f;
                        var choice = _levelUpChoices[i];
                        if (GUI.Button(new Rect(left + 35f, top + yOffset, 370f, 64f), choice.Title + "\n" + choice.Description, _button))
                            ChooseUpgrade(i);
                    }
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

        private static string FormatTime(float seconds)
        {
            var whole = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return (whole / 60).ToString("00") + ":" + (whole % 60).ToString("00");
        }
    }
}
