using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed class EnemyAgent2D : MonoBehaviour
    {
        private EchoSphereRuntime _runtime;
        private Transform _player;
        private SpriteRenderer _shell;
        private float _hp;
        private float _maxHp;
        private int _xpReward = 1;
        private float _moveSpeed;
        private bool _dead;
        private float _slowTimer;
        private float _slowMultiplier = 1f;
        private float _markTimer;
        private int _corruptStacks;
        public int CorruptStacks => _corruptStacks;
        private float _dotTimer;
        private float _dotRemaining;
        private float _dotDamagePerSecond;
        private float _fireTimer, _fireDps, _fireTickTimer, _fireTickInterval;
        private float _poisonTimer, _poisonDps, _poisonTickTimer, _poisonTickInterval;
        private float _freezeTimer, _freezeVulnerabilityTimer, _elementalReactionTimer;
        private SphereId? _freezeVulnerabilitySource, _elementalConductionSource;
        private int _elementalConduction;
        private Color _baseColor = Color.white;
        private EnemyCreatureVisual2D _creatureVisual;
        private BossPresentationVfx2D _bossPresentation;
        private BossCombatAgent2D _bossCombat;
        private string _bossType = "";
        public bool IsMarked => _markTimer > 0f;
        public bool IsBoss => !string.IsNullOrEmpty(_bossType);
        public string BossType => _bossType;
        public float MaxHp => _maxHp;
        public Vector2 PlayerPosition => _player == null ? Vector2.zero : (Vector2)_player.position;
        public bool PresentationPaused => _runtime == null || _runtime.IsPresentationPaused;
        public float MovementSlowMultiplier => _slowTimer > 0f ? _slowMultiplier : 1f;
        public void SetBossCombat(BossCombatAgent2D combat) { _bossCombat = combat; }

        public void SetBossPresentationState(float shootTimer, float summonTimer, float chargeTimer,
            bool isCharging, Vector2 chargeDirection, float auraRadius)
        {
            if (_bossPresentation != null)
                _bossPresentation.SetCombatState(shootTimer, summonTimer, chargeTimer, isCharging, chargeDirection, auraRadius);
        }
        public float CurrentHp => Mathf.Max(0f, _hp);
        public bool IsAlive => !_dead && _hp > 0f;
        public float HpFraction => _maxHp <= 0f ? 0f : Mathf.Clamp01(_hp / _maxHp);
        public bool IsFrozen => _freezeTimer > 0f;
        public bool HasFireStatus => _fireTimer > 0f;
        public bool HasFreezeStatus => _freezeTimer > 0f;
        public bool HasPoisonStatus => _poisonTimer > 0f;

        public float IncomingSphereDamageMultiplier(SphereId source)
        {
            return _freezeTimer > 0f && _freezeVulnerabilityTimer > 0f &&
                _freezeVulnerabilitySource.HasValue && _freezeVulnerabilitySource.Value == source ? 1.22f : 1f;
        }

        public int AddCorruptStack()
        {
            _corruptStacks = Mathf.Min(5, _corruptStacks + 1);
            return _corruptStacks;
        }

        public float ReceiveSphereDamage(SphereId source, float damage, bool authoredDamageResolved = false,
            bool allowAuthoredProcs = true, SphereAttackAgent owner = null)
        {
            if (_dead) return 0f;
            var authoredDamage = authoredDamageResolved || _runtime == null
                ? damage : _runtime.ResolveAuthoredSphereHitDamage(source, this, damage);
            var actual = authoredDamage * IncomingSphereDamageMultiplier(source);
            if (_runtime != null) _runtime.ApplyAuthoredSphereHitEffects(source, this, actual, allowAuthoredProcs, owner);
            ReceiveDamage(actual, source);
            return actual;
        }

        public float ReceiveSphereDamage(SphereAttackAgent owner, float damage,
            bool authoredDamageResolved = false, bool allowAuthoredProcs = true)
        {
            if (owner == null) return 0f;
            return ReceiveSphereDamage(owner.Type, damage, authoredDamageResolved, allowAuthoredProcs, owner);
        }

        public void ApplyElementalStatus(SphereElementalStatus status, SphereId source)
        {
            if (_dead || status.Element == SphereElement.None) return;
            switch (status.Element)
            {
                case SphereElement.Fire:
                    _fireTimer = Mathf.Max(_fireTimer, status.Duration);
                    _fireDps = status.DamagePerSecond;
                    _fireTickInterval = status.TickInterval;
                    _fireTickTimer = 0f;
                    break;
                case SphereElement.Freeze:
                    _freezeTimer = Mathf.Max(_freezeTimer, status.Duration);
                    if (status.FreezeVulnerability)
                    {
                        _freezeVulnerabilityTimer = Mathf.Max(_freezeVulnerabilityTimer, status.Duration);
                        _freezeVulnerabilitySource = source;
                    }
                    else
                    {
                        _freezeVulnerabilityTimer = 0f;
                        _freezeVulnerabilitySource = null;
                    }
                    break;
                case SphereElement.Poison:
                    _poisonTimer = Mathf.Max(_poisonTimer, status.Duration);
                    _poisonDps = status.DamagePerSecond;
                    _poisonTickInterval = status.TickInterval;
                    _poisonTickTimer = 0f;
                    break;
            }

            if (status.AdditionalSlowDuration > 0f)
            {
                _slowTimer = Mathf.Max(_slowTimer, status.AdditionalSlowDuration);
                _slowMultiplier = Mathf.Min(_slowMultiplier, status.AdditionalSlowMultiplier);
            }
            if (_runtime != null) _runtime.TryTriggerAlchemistReaction(this);
            RefreshElementalStatusVfx();
        }

        public AlchemistReactionKind GetAlchemistReactionKind() =>
            AlchemistReactionRules.Resolve(HasFireStatus, HasFreezeStatus, HasPoisonStatus);

        private void RefreshElementalStatusVfx()
        {
            if (_creatureVisual != null)
                _creatureVisual.SetStatusVfx(_fireTimer, _poisonTimer, _freezeTimer);
        }

        public void ConsumeAlchemistReaction(AlchemistReactionKind kind)
        {
            if (_dead) return;
            switch (kind)
            {
                case AlchemistReactionKind.ToxinIgnition:
                    ClearFireStatus();
                    ClearPoisonStatus();
                    break;
                case AlchemistReactionKind.Cryotoxin:
                    ClearFreezeStatus();
                    ClearPoisonStatus();
                    ApplySlow(2f, 0.35f);
                    break;
                case AlchemistReactionKind.ThermalShock:
                    ClearFireStatus();
                    ClearFreezeStatus();
                    ApplySlow(2f, 0.40f);
                    break;
            }
        }

        private void ClearFireStatus()
        {
            _fireTimer = 0f;
            _fireDps = 0f;
            _fireTickTimer = 0f;
            _fireTickInterval = 0f;
            RefreshElementalStatusVfx();
        }

        private void ClearFreezeStatus()
        {
            _freezeTimer = 0f;
            _freezeVulnerabilityTimer = 0f;
            _freezeVulnerabilitySource = null;
            RefreshElementalStatusVfx();
        }

        private void ClearPoisonStatus()
        {
            _poisonTimer = 0f;
            _poisonDps = 0f;
            _poisonTickTimer = 0f;
            _poisonTickInterval = 0f;
            RefreshElementalStatusVfx();
        }

        public bool TryAdvanceElementalConduction(SphereId source, int threshold, float cooldown)
        {
            if (_dead || _elementalReactionTimer > 0f) return false;
            if (!_elementalConductionSource.HasValue || _elementalConductionSource.Value != source)
            {
                _elementalConductionSource = source;
                _elementalConduction = 0;
            }
            _elementalConduction = Mathf.Min(Mathf.Max(1, threshold), _elementalConduction + 1);
            if (_elementalConduction < Mathf.Max(1, threshold)) return false;
            _elementalConduction = 0;
            _elementalReactionTimer = Mathf.Max(0f, cooldown);
            return true;
        }

        public bool TryBeginFieldElementalReaction(float cooldown)
        {
            if (_dead || _elementalReactionTimer > 0f) return false;
            _elementalReactionTimer = Mathf.Max(0f, cooldown);
            return true;
        }

        public void ApplyDamageOverTime(float damagePerSecond, float duration)
        {
            if (_dead) return;
            _dotDamagePerSecond = Mathf.Max(_dotDamagePerSecond, Mathf.Max(0f, damagePerSecond));
            _dotRemaining = Mathf.Max(_dotRemaining, Mathf.Max(0f, duration));
            if (_dotTimer <= 0f) _dotTimer = 0.5f;
        }

        public void ApplyMark(float duration)
        {
            if (_dead) return;
            _markTimer = Mathf.Max(_markTimer, Mathf.Max(0f, duration));
            if (_shell != null) _shell.color = new Color(0.5f, 0.82f, 1f, 1f);
            if (_creatureVisual != null) _creatureVisual.SetMarked(true);
        }

        public void ApplySlow(float duration, float multiplier)
        {
            if (_dead) return;
            _slowTimer = Mathf.Max(_slowTimer, Mathf.Max(0f, duration));
            _slowMultiplier = Mathf.Clamp(multiplier, 0.15f, 1f);
        }

        public void KnockBackFrom(Vector2 center, float distance)
        {
            if (_dead || _runtime == null || _runtime.IsGameplayPaused) return;
            var away = ((Vector2)transform.position - center).normalized;
            if (away.sqrMagnitude <= 0.0001f) away = Vector2.up;
            transform.position += (Vector3)(away * Mathf.Max(0f, distance));
        }

        public void PullByDistance(Vector2 center, float distance)
        {
            if (_dead || _runtime == null || _runtime.IsGameplayPaused) return;
            transform.position = Vector2.MoveTowards(transform.position, center, Mathf.Max(0f, distance));
        }

        public void PullToward(Vector2 center, float distance)
        {
            if (_dead || _runtime == null || _runtime.IsGameplayPaused) return;
            transform.position = Vector2.MoveTowards(transform.position, center, Mathf.Max(0f, distance) * 0.22f);
        }

        public void Initialize(EchoSphereRuntime runtime, Transform player, float hp, float moveSpeed, int xpReward = 1, string visualVariant = "wisp", string bossType = "")
        {
            _runtime = runtime;
            _player = player;
            _bossType = bossType ?? "";
            _hp = hp;
            _maxHp = Mathf.Max(1f, hp);
            _moveSpeed = Mathf.Max(0f, moveSpeed);
            _xpReward = Mathf.Max(1, xpReward);
            _shell = transform.Find("Shell")?.GetComponent<SpriteRenderer>();
            if (_shell != null) _baseColor = _shell.color;
            _creatureVisual = GetComponent<EnemyCreatureVisual2D>();
            if (_creatureVisual == null) _creatureVisual = gameObject.AddComponent<EnemyCreatureVisual2D>();
            _creatureVisual.Initialize(_runtime, visualVariant, bossType, _baseColor);
            if (!string.IsNullOrEmpty(bossType))
            {
                _bossPresentation = GetComponent<BossPresentationVfx2D>();
                if (_bossPresentation == null) _bossPresentation = gameObject.AddComponent<BossPresentationVfx2D>();
                _bossPresentation.Initialize(this, bossType, _baseColor);
            }
        }

        private void Update()
        {
            if (_dead || _runtime == null || _runtime.IsGameplayPaused || _player == null) return;
            var dt = Time.deltaTime;
            _elementalReactionTimer = Mathf.Max(0f, _elementalReactionTimer - dt);
            if (_freezeVulnerabilityTimer > 0f)
            {
                _freezeVulnerabilityTimer = Mathf.Max(0f, _freezeVulnerabilityTimer - dt);
                if (_freezeVulnerabilityTimer <= 0f) _freezeVulnerabilitySource = null;
            }

            if (_dotRemaining > 0f)
            {
                _dotRemaining = Mathf.Max(0f, _dotRemaining - dt);
                _dotTimer -= dt;
                if (_dotTimer <= 0f)
                {
                    ReceiveDamage(_dotDamagePerSecond * 0.5f);
                    _dotTimer = 0.5f;
                    if (_dead) return;
                }
                if (_dotRemaining <= 0f) _dotDamagePerSecond = 0f;
            }

            if (_fireTimer > 0f)
            {
                _fireTimer = Mathf.Max(0f, _fireTimer - dt);
                var damage = 0f;
                if (_fireTickInterval > 0f)
                {
                    _fireTickTimer -= dt;
                    if (_fireTickTimer <= 0f) { _fireTickTimer = _fireTickInterval; damage = _fireDps * _fireTickInterval; }
                }
                else damage = _fireDps * dt;
                ApplyStatusDamage(damage);
                if (_dead) return;
                if (_fireTimer <= 0f) { _fireDps = 0f; _fireTickInterval = 0f; _fireTickTimer = 0f; }
            }

            if (_poisonTimer > 0f)
            {
                _poisonTimer = Mathf.Max(0f, _poisonTimer - dt);
                var damage = 0f;
                if (_poisonTickInterval > 0f)
                {
                    _poisonTickTimer -= dt;
                    if (_poisonTickTimer <= 0f) { _poisonTickTimer = _poisonTickInterval; damage = _poisonDps * _poisonTickInterval; }
                }
                else damage = _poisonDps * dt;
                ApplyStatusDamage(damage);
                if (_dead) return;
                if (_poisonTimer <= 0f) { _poisonDps = 0f; _poisonTickInterval = 0f; _poisonTickTimer = 0f; }
            }

            RefreshElementalStatusVfx();

            if (_markTimer > 0f)
            {
                _markTimer = Mathf.Max(0f, _markTimer - Time.deltaTime);
                if (_markTimer <= 0f)
                {
                    if (_shell != null) _shell.color = _baseColor;
                    if (_creatureVisual != null) _creatureVisual.SetMarked(false);
                }
            }
            if (_freezeTimer > 0f)
            {
                _freezeTimer = Mathf.Max(0f, _freezeTimer - dt);
                RefreshElementalStatusVfx();
                if (_freezeTimer > 0f)
                {
                    if (_bossCombat != null)
                    {
                        _bossCombat.Tick(dt);
                        if (_creatureVisual != null) _creatureVisual.SetFacing((Vector2)_player.position - (Vector2)transform.position);
                    }
                    return;
                }
            }
            if (_slowTimer > 0f) _slowTimer = Mathf.Max(0f, _slowTimer - dt);
            else _slowMultiplier = 1f;
            var target = (Vector2)_player.position;
            if (_bossCombat != null)
            {
                _bossCombat.Tick(Time.deltaTime);
                if (_creatureVisual != null) _creatureVisual.SetFacing(target - (Vector2)transform.position);
                return;
            }
            transform.position = Vector2.MoveTowards(transform.position, target, _moveSpeed * _slowMultiplier * _runtime.GetEnemyAbilitySpeedMultiplier() * Time.deltaTime);
            if (_creatureVisual != null) _creatureVisual.SetFacing(target - (Vector2)transform.position);
            if (Vector2.Distance(transform.position, target) < 0.54f) _runtime.TryDamagePlayer(9f);
        }

        private void ApplyStatusDamage(float damage)
        {
            if (_dead || damage <= 0f) return;
            _hp -= damage;
            if (_hp <= 0f)
            {
                _dead = true;
                _runtime.RegisterEnemyDeath(this, transform.position, _xpReward);
                Destroy(gameObject);
            }
        }

        public void ReceiveDamage(float damage, SphereId? sourceSphere = null)
        {
            if (_dead) return;
            _hp -= Mathf.Max(0f, damage);
            if (_shell != null) _shell.color = IsMarked ? new Color(0.5f, 0.82f, 1f, 1f) : _baseColor;
            if (_creatureVisual != null) _creatureVisual.PulseHit();
            _runtime.SpawnImpact(transform.position, new Color(1f, 0.26f, 0.34f));
            if (_hp <= 0f)
            {
                _dead = true;
                _runtime.RegisterEnemyDeath(this, transform.position, _xpReward, sourceSphere);
                Destroy(gameObject);
            }
        }
    }
}
