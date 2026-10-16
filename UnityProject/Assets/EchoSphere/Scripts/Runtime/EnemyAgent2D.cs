using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed class EnemyAgent2D : MonoBehaviour, ISourceSimulationAgent
    {
        internal float SourceProjectionHitFlash => _sourceHitFlash;
        internal float SourceProjectionFreezeTimer => _freezeTimer;
        internal float SourceProjectionFireTimer => _fireTimer;
        internal float SourceProjectionPoisonTimer => _poisonTimer;
        internal float SourceProjectionRotation => _sourceVisualRotation;
        internal float SourceProjectionLinkBreakerTelegraphTimer => _linkBreakerTelegraphTimer;
        internal SphereAttackAgent SourceProjectionLinkBreakerTarget => _linkBreakerTarget;
        internal EnemyCreatureVisual2D SourceProjectionCreatureVisual => _creatureVisual;
        internal BossCombatAgent2D SourceProjectionBossCombat => _bossCombat;
        private EchoSphereRuntime _runtime;
        private Transform _player;
        private SpriteRenderer _shell;
        private float _hp; private float _sourceHitFlash,_sourceVisualRotation; private int _sourceVisualTier; public void SetSourceVisualTier(int tier)=>_sourceVisualTier=tier; public int SourceVisualTier=>_sourceVisualTier;
        private float _maxHp;
        private float _xpReward = 1f;
        private float _moveSpeed;
        private float _damage = 9f;
        private bool _dead;
        private bool _isElite;
        private bool _isPhaseRole;
        private string _role = "grunt";
        private float _roleDamageMultiplier = 1f;
        private float _roleRadiusMultiplier = 1f;
        private float _sourceRadiusOverride;
        public float SourceRadius => _sourceRadiusOverride > 0f ? _sourceRadiusOverride :
            ((IsBoss ? 34f : _archetype == EnemyArchetype.Fast ? 15f : _archetype == EnemyArchetype.Tank ? 26f : 19f) * _roleRadiusMultiplier + (_isElite ? 4f : 0f));
        public float SourceSpeed => _moveSpeed / SpherePlacementRules.SourcePixelsToWorld;
        public float CollisionMass => IsBoss ? 8f : _isElite ? 1f : _archetype == EnemyArchetype.Tank ? 2.5f : _archetype == EnemyArchetype.Fast ? 0.7f : 1f;
        public string Role => _role;
        public string EliteVariant => _eliteVariant;
        public float ContactDamage => _damage;
        public string SourceColorHtml => IsBoss ? "#" + ColorUtility.ToHtmlStringRGB(_baseColor) : _isElite ? "#b8475a" : _archetype == EnemyArchetype.Fast ? "#d4a830" : _archetype == EnemyArchetype.Tank ? "#8a5a8a" : "#4a7a8a";
        public void SetSourceRadius(float radius)
        {
            _sourceRadiusOverride = radius;
            if (_creatureVisual != null) _creatureVisual.RefreshSourceRadius();
        }
        private float _roleActionTimer;
        private EnemyArchetype _archetype = EnemyArchetype.Normal;
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
        private float _fireDamageNumberTimer = 0.25f, _poisonDamageNumberTimer = 0.25f;
        private float _fireDamageNumberAccumulator, _poisonDamageNumberAccumulator;
        private float _freezeTimer, _freezeVulnerabilityTimer, _elementalReactionTimer;
        private float _linkBreakerCooldownTimer, _linkBreakerTelegraphTimer, _linkBreakerImpactTimer;
        private string _eliteVariant = "";
        private float _elitePulseTimer;
        private SphereAttackAgent _linkBreakerTarget, _linkBreakerImpactTarget;
        private LineRenderer[] _linkBreakerLines;
        private static Material _linkBreakerLineMaterial;
        private SphereId? _freezeVulnerabilitySource, _elementalConductionSource;
        private int _elementalConduction;
        private Color _baseColor = Color.white;
        private EnemyCreatureVisual2D _creatureVisual;
        private BossPresentationVfx2D _bossPresentation;
        private BossCombatAgent2D _bossCombat;
        private string _bossType = "";
        public bool IsMarked => _markTimer > 0f;
        public void ApplyRuneMark(float duration) => _markTimer = Mathf.Max(_markTimer, Mathf.Max(0f, duration));
        public bool IsBoss => !string.IsNullOrEmpty(_bossType);
        public bool IsElite => _isElite;
        public bool IsPhaseRole => _isPhaseRole;
        public bool IsTank => _archetype == EnemyArchetype.Tank;
        public string SourceMonsterType => _isElite ? "elite" : _archetype == EnemyArchetype.Fast ? "fast" : _archetype == EnemyArchetype.Tank ? "tank" : "normal";
        public EnemyArchetype Archetype => _archetype;
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
        public void HealByFraction(float fraction) { if (!_dead) _hp = Mathf.Min(_maxHp, _hp + _maxHp * Mathf.Max(0f, fraction)); }
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
            bool allowAuthoredProcs = true, SphereAttackAgent owner = null, bool characterHitRegistered = false, bool wasCritical = false)
        {
            if (_dead) return 0f;
            if (_runtime != null && !characterHitRegistered) _runtime.RegisterCharacterSphereHit(source, this);
            _runtime?.ApplySourceTimeStopFreeze(this);
            var contextualDamage = owner == null || _runtime == null ? damage : damage * _runtime.GetSourceCharacterDamageMultiplier(owner);
            var authoredDamage = authoredDamageResolved || _runtime == null
                ? contextualDamage : _runtime.ResolveAuthoredSphereHitDamage(source, this, contextualDamage);
            var characterMultiplier = _runtime == null ? 1f : _runtime.GetHunterMarkedTargetDamageMultiplier(source, this);
            var actual = authoredDamage * characterMultiplier * IncomingSphereDamageMultiplier(source);
            if (_eliteVariant == "mirror_warden") actual *= 0.88f;
            actual *= _runtime == null ? 1f : _runtime.GetSourceTimeStopHitMultiplier();
            if (!authoredDamageResolved && _runtime != null)
                actual = _runtime.ResolveProjectileDamage(actual, source == SphereId.Sniper && _runtime.GetSphereProgressionLevel(source) >= 3 ? 0.15f : 0f, out wasCritical, this);
            if (owner != null) actual = _runtime.ApplySourceBeforeModifierProcs(owner, this, actual, allowAuthoredProcs, ref wasCritical);
            if (_runtime != null) _runtime.ApplyAuthoredSphereHitEffects(source, this, actual, allowAuthoredProcs, owner);
            if (source == SphereId.Shotgun && owner != null && _runtime.GetSphereProgressionLevel(source) >= 2 &&
                Vector2.Distance(transform.position, owner.transform.position) < 110f * SpherePlacementRules.SourcePixelsToWorld) actual *= 1.20f;
            if (source == SphereId.Void) actual = _runtime.ResolveSourceVoidHit(this, actual, owner);
            if (owner != null && allowAuthoredProcs) actual = owner.ApplySourceBranchHit(this, actual, wasCritical);
            actual *= _runtime == null ? 1f : _runtime.SourceBuffDamageMultiplier;
            _hp -= Mathf.Max(0f, actual);
            RefreshReceivedDamageVisuals();
            if (_eliteVariant == "mirror_warden" && actual > 0f && _runtime != null)
                _runtime.TryDamagePlayer(actual * 0.12f);
            if (owner != null) _runtime.RegisterSphereHit(owner, this, actual);
            else _runtime?.ChargeResonanceFromSource(ResonanceRules.SphereHitCharge, false);
            if (_runtime != null && owner != null)
                _runtime.RegisterCharacterHitProcs(owner, actual, allowAuthoredProcs);
            if (owner != null) _runtime.ApplySourceAfterHitGeometry(owner, this, actual);
            _runtime?.EmitSourceHitFeedback(this, actual, true, allowAuthoredProcs, wasCritical, source);
            _runtime?.HealCoreFromDamage(actual);
            FinishReceivedDamage(source, owner, actual);
            return actual;
        }

        public float ReceiveSphereDamage(SphereAttackAgent owner, float damage,
            bool authoredDamageResolved = false, bool allowAuthoredProcs = true)
        {
            if (owner == null) return 0f;
            return ReceiveSphereDamage(owner.Type, damage, authoredDamageResolved, allowAuthoredProcs, owner);
        }

        public void ApplyFireStatus(float duration, float damagePerSecond)
        {
            if (_dead) return;
            _fireTimer = Mathf.Max(_fireTimer, Mathf.Max(0f, duration));
            _fireDps = Mathf.Max(_fireDps, Mathf.Max(0f, damagePerSecond));
        }

        public void ExtendFireStatus(float duration)
        {
            if (!_dead) _fireTimer = Mathf.Max(_fireTimer, Mathf.Max(0f, duration));
        }

        public void ExtendSlowStatus(float duration)
        {
            if (!_dead) _slowTimer = Mathf.Max(_slowTimer, Mathf.Max(0f, duration));
        }
        public void ExtendPoisonStatus(float duration) { if (!_dead) _poisonTimer = Mathf.Max(_poisonTimer, duration); }

        public void ApplySourceSlow(float duration, float multiplier)
        {
            if (_dead) return;
            _slowTimer = Mathf.Max(_slowTimer, Mathf.Max(0f, duration));
            _slowMultiplier = Mathf.Min(_slowMultiplier, Mathf.Clamp(multiplier, 0f, 1f));
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
            _slowMultiplier = Mathf.Min(_slowMultiplier, Mathf.Clamp(multiplier, 0.15f, 1f));
        }

        public void ApplyFreeze(float duration)
        {
            if (_dead) return;
            _freezeTimer = Mathf.Max(_freezeTimer, Mathf.Max(0f, duration));
            RefreshElementalStatusVfx();
        }

        public void SetFreezeDuration(float duration)
        {
            if (_dead) return;
            _freezeTimer = Mathf.Max(0f, duration);
            RefreshElementalStatusVfx();
        }

        public void KnockBackFrom(Vector2 center, float distance)
        {
            if (_dead || _runtime == null || _runtime.IsGameplayPaused) return;
            var away = ((Vector2)transform.position - center).normalized;
            if (away.sqrMagnitude <= 0.0001f) away = Vector2.up;
            transform.position += (Vector3)(away * Mathf.Max(0f, distance) * _runtime.GetRegionRepulsionMultiplier());
        }

        public void MoveSourceRadial(Vector2 center, float signedDistance)
        {
            var delta = (Vector2)transform.position - center;
            var distance = delta.magnitude;
            if (distance > 0f) transform.position += (Vector3)(delta / distance * signedDistance);
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

        public void Initialize(EchoSphereRuntime runtime, Transform player, float hp, float moveSpeed, float xpReward = 1f,
            string visualVariant = "wisp", string bossType = "", bool isElite = false, EnemyArchetype archetype = EnemyArchetype.Normal,
            bool isPhaseRole = false, string role = "grunt", float roleDamageMultiplier = 1f, float roleRadiusMultiplier = 1f,
            string eliteVariant = "", float damage = 9f)
        {
            _runtime = runtime;
            _runtime.RegisterSimulationAgent(this);
            _player = player;
            _bossType = bossType ?? "";
            _isElite = isElite;
            _eliteVariant = isElite ? (eliteVariant ?? "") : "";
            _elitePulseTimer = isElite ? 5f : 0f;
            _linkBreakerCooldownTimer = _eliteVariant == "linkbreaker" ? 5f : 0f;
            _archetype = archetype;
            _isPhaseRole = isPhaseRole;
            _role = role ?? "grunt";
            _roleDamageMultiplier = Mathf.Max(0f, roleDamageMultiplier);
            _roleRadiusMultiplier = Mathf.Max(0.01f, roleRadiusMultiplier);
            _roleActionTimer = 0f;
            _hp = hp;
            _maxHp = Mathf.Max(1f, hp);
            _moveSpeed = Mathf.Max(0f, moveSpeed);
            _damage = Mathf.Max(0f, damage);
            _xpReward = Mathf.Max(1, xpReward);
            _shell = transform.Find("Shell")?.GetComponent<SpriteRenderer>();
            if (_shell != null) _baseColor = _shell.color;
            if (_eliteVariant == "linkbreaker") EnsureLinkBreakerLines();
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

        public void SimulationTick(float dt)
        {
            if (_dead || _runtime == null || _runtime.IsGameplayPaused || _player == null) return;
            _sourceVisualRotation+=dt; if(_sourceHitFlash>0f)_sourceHitFlash-=dt; _elementalReactionTimer = Mathf.Max(0f, _elementalReactionTimer - dt);
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
                _fireDamageNumberTimer = Mathf.Max(0f, _fireDamageNumberTimer - dt);
                _fireTimer = Mathf.Max(0f, _fireTimer - dt);
                var damage = 0f;
                if (_fireTickInterval > 0f)
                {
                    _fireTickTimer -= dt;
                    if (_fireTickTimer <= 0f) { _fireTickTimer = _fireTickInterval; damage = _fireDps * _fireTickInterval; }
                }
                else damage = _fireDps * dt;
                TickStatusFeedback(damage, true, _fireTimer, _fireTickInterval, ref _fireDamageNumberTimer, ref _fireDamageNumberAccumulator);
                ApplyStatusDamage(damage);
                if (_dead) return;
                if (_fireTimer <= 0f) { _fireDps = 0f; _fireTickInterval = 0f; _fireTickTimer = 0f; }
            }

            if (_poisonTimer > 0f)
            {
                _poisonDamageNumberTimer = Mathf.Max(0f, _poisonDamageNumberTimer - dt);
                _poisonTimer = Mathf.Max(0f, _poisonTimer - dt);
                var damage = 0f;
                if (_poisonTickInterval > 0f)
                {
                    _poisonTickTimer -= dt;
                    if (_poisonTickTimer <= 0f) { _poisonTickTimer = _poisonTickInterval; damage = _poisonDps * _poisonTickInterval; }
                }
                else damage = _poisonDps * dt;
                TickStatusFeedback(damage, false, _poisonTimer, _poisonTickInterval, ref _poisonDamageNumberTimer, ref _poisonDamageNumberAccumulator);
                ApplyStatusDamage(damage);
                if (_dead) return;
                if (_poisonTimer <= 0f) { _poisonDps = 0f; _poisonTickInterval = 0f; _poisonTickTimer = 0f; }
            }

            RefreshElementalStatusVfx();

            if (_markTimer > 0f)
            {
                _markTimer = Mathf.Max(0f, _markTimer - _runtime.SimulationDeltaTime);
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
                return;
            }
            if (_slowTimer > 0f) _slowTimer = Mathf.Max(0f, _slowTimer - dt);
            else _slowMultiplier = 1f;
            var target = (Vector2)_player.position;
            if (_bossCombat != null)
            {
                _bossCombat.Tick(_runtime.SimulationDeltaTime);
                if (_creatureVisual != null) _creatureVisual.SetFacing(target - (Vector2)transform.position);
                return;
            }
            var toPlayer = target - (Vector2)transform.position;
            var distanceToPlayer = toPlayer.magnitude;
            var directionToPlayer = distanceToPlayer > 0.0001f ? toPlayer / distanceToPlayer : Vector2.zero;
            if (_eliteVariant == "linkbreaker") TickLinkBreaker(dt);
            var speedMultiplier = _runtime.GetEnemyAbilitySpeedMultiplier(distanceToPlayer, _slowMultiplier) * (_isPhaseRole ? 1.12f : 1f);
            if (_role == "scavenger" && _hp / Mathf.Max(1f, _maxHp) < 0.5f) speedMultiplier *= 1.30f;
            if (_eliteVariant == "phantom_hunter" && distanceToPlayer > 180f * SpherePlacementRules.SourcePixelsToWorld) speedMultiplier *= 1.35f;
            if (_eliteVariant == "scavenger_prime" && _hp / Mathf.Max(1f, _maxHp) < 0.5f) speedMultiplier *= 1.30f;
            transform.position += (Vector3)(directionToPlayer * _moveSpeed * speedMultiplier * dt);
            if (_creatureVisual != null) _creatureVisual.SetFacing(target - (Vector2)transform.position);
            if (_role == "leech" && distanceToPlayer < 160f * SpherePlacementRules.SourcePixelsToWorld)
                _hp = Mathf.Min(_maxHp, _hp + 3f * dt);

            if (_role == "ranged" || _role == "sniper")
            {
                _roleActionTimer -= dt;
                if (_roleActionTimer <= 0f && distanceToPlayer > 240f * SpherePlacementRules.SourcePixelsToWorld &&
                    distanceToPlayer < 620f * SpherePlacementRules.SourcePixelsToWorld)
                {
                    var sniper = _role == "sniper";
                    _roleActionTimer = sniper ? 2.2f : 3.2f;
                    var direction = (target - (Vector2)transform.position).normalized;
                    var projectileSpeed = BossCombatRules.ToWorldUnits(sniper ? 320f : 245f);
                    var projectileDamage = sniper ? 10f : 6f;
                    _runtime.SpawnBossProjectile(transform.position, direction * projectileSpeed, projectileDamage,
                        sniper ? 5f : 6f, sniper ? "enemy_sniper" : "enemy_ranged",
                        sniper ? new Color(1f, 0.55f, 0.30f) : new Color(0.35f, 0.85f, 1f), this);
                    SourceAudio2D.Play("shoot");
                }
            }
            else if (_role == "healer")
            {
                _roleActionTimer -= dt;
                if (_roleActionTimer <= 0f)
                {
                    _roleActionTimer = 4.5f;
                    _runtime.HealNearbyEnemies(this, 120f, 0.08f, 3);
                }
            }
            else if (_role == "disruptor")
            {
                _roleActionTimer -= dt;
                if (_roleActionTimer <= 0f)
                {
                    _roleActionTimer = 6f;
                    _runtime.TryDisruptNearestSphere(transform.position, 280f);
                }
            }

            _runtime.TickSourceEnemyProjectiles(this, dt);
            TickEliteVariant(dt, distanceToPlayer);

            var sourceContactRadius = (SourceRadius + 16f) * SpherePlacementRules.SourcePixelsToWorld;
            if (distanceToPlayer < sourceContactRadius)
            {
                // Source damage is resolved at spawn, including difficulty, role and region multipliers.
                var contactDamage = _damage;
                if (_role == "charger") contactDamage *= 1.20f;
                _runtime.TryDamagePlayer(contactDamage);
                if (_role == "corruptor") _runtime.DrainResonance(4f);

                // Source engineEnemies.ts pushes normal enemies away by 10 source pixels on contact.
                var awayFromPlayer = -directionToPlayer;
                if (awayFromPlayer.sqrMagnitude > 0.0001f)
                    transform.position += (Vector3)(awayFromPlayer * 10f * SpherePlacementRules.SourcePixelsToWorld);
            }
        }

        private void TickStatusFeedback(float damage, bool fire, float remaining, float interval, ref float timer, ref float accumulator)
        {
            accumulator += damage;
            if (timer <= 0f && accumulator > 0f)
            {
                _runtime.AddSourceStatusDamageNumber(this, accumulator, fire ? "fire" : "poison");
                accumulator = 0f; timer = 0.28f;
            }
            if (remaining <= 0f && accumulator > 0f)
            {
                _runtime.AddSourceStatusDamageNumber(this, accumulator, fire ? "fire" : "poison");
                accumulator = 0f;
            }
            _runtime.EmitSourceStatusParticle(this, fire, interval);
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

        public void ScaleCurrentHp(float multiplier)
        {
            if (_dead) return;
            _hp *= multiplier;
        }

        public void ReceiveDamage(float damage, SphereId? sourceSphere = null, bool emitFeedback = true)
        {
            if (_dead) return;
            _runtime?.ApplySourceTimeStopFreeze(this);
            damage *= _runtime == null ? 1f : _runtime.SourceBuffDamageMultiplier;
            _hp -= Mathf.Max(0f, damage);
            RefreshReceivedDamageVisuals();
            if (emitFeedback) _runtime.EmitSourceHitFeedback(this, damage, false, false, false, sourceSphere);
            FinishReceivedDamage(sourceSphere);
        }

        private void RefreshReceivedDamageVisuals()
        {
            if (_shell != null) _shell.color = IsMarked ? new Color(0.5f, 0.82f, 1f, 1f) : _baseColor;
            _sourceHitFlash=.15f; if (_creatureVisual != null) _creatureVisual.PulseHit();
            _runtime.SpawnImpact(transform.position, new Color(1f, 0.26f, 0.34f));
        }
        private void FinishReceivedDamage(SphereId? sourceSphere, SphereAttackAgent owner = null, float damage = 0f)
        {
            if (_hp <= 0f && !_dead)
            {
                if (_runtime.HasArtifact("vampire_ring")) _runtime.HealPlayer(5f);
                _runtime.ApplySourceVoidReaperKill(this, owner, damage);
                _dead = true;
                _runtime.RegisterEnemyDeath(this, transform.position, _xpReward, sourceSphere);
                Destroy(gameObject);
            }
        }
        private void TickEliteVariant(float dt, float distanceToPlayer)
        {
            if (!_isElite || string.IsNullOrEmpty(_eliteVariant) || _eliteVariant == "linkbreaker") return;

            if (_eliteVariant == "resonance_leech" &&
                distanceToPlayer < 220f * SpherePlacementRules.SourcePixelsToWorld)
                _runtime.DrainResonance(5f * dt);
            if (_eliteVariant == "pyroclast" &&
                distanceToPlayer < 95f * SpherePlacementRules.SourcePixelsToWorld)
                _runtime.TryDamagePlayerOverTime(8f * dt);
            if (_eliteVariant == "stasis_warden" &&
                distanceToPlayer < 170f * SpherePlacementRules.SourcePixelsToWorld)
                _runtime.TryDamagePlayerOverTime(6f * dt);

            _elitePulseTimer -= dt;
            if (_elitePulseTimer > 0f) return;
            _elitePulseTimer = _eliteVariant == "geometry_shifter" || _eliteVariant == "nullifier" ? 6f : 4f;

            if (_eliteVariant == "geometry_shifter")
            {
                _runtime.DrainResonance(18f);
                _runtime.ShowTimedCombatMessage("GEOMETRY SHIFT", 0.8f);
            }
            else if (_eliteVariant == "nullifier")
            {
                _runtime.TryNullifyNearestSphere(transform.position, 300f, 2f);
            }
            else if (_eliteVariant == "stasis_warden" &&
                distanceToPlayer < 170f * SpherePlacementRules.SourcePixelsToWorld)
            {
                _runtime.ShowTimedCombatMessage("STASIS FIELD", 0.5f);
            }
        }

        private void TickLinkBreaker(float dt)
        {
            if (!_isElite || _runtime == null) return;
            _linkBreakerImpactTimer = Mathf.Max(0f, _linkBreakerImpactTimer - dt);
            if (_linkBreakerTelegraphTimer > 0f)
            {
                _linkBreakerTelegraphTimer = Mathf.Max(0f, _linkBreakerTelegraphTimer - dt);
                if (_linkBreakerTelegraphTimer <= 0f)
                {
                    var target = _linkBreakerTarget;
                    _linkBreakerTarget = null;
                    if (target != null && target.gameObject.activeInHierarchy &&
                        Vector2.Distance(target.transform.position, transform.position) <= 320f * SpherePlacementRules.SourcePixelsToWorld)
                    {
                        target.ApplyNetworkDisable(_runtime.GetNetworkDisableDuration(2f));
                        _runtime.ShowTimedCombatMessage("NETWORK BREAK", 0.8f);
                        _linkBreakerImpactTarget = target;
                        _linkBreakerImpactTimer = 0.30f;
                    }
                }
            }
            else
            {
                _linkBreakerCooldownTimer -= dt;
                if (_linkBreakerCooldownTimer <= 0f)
                {
                    _linkBreakerCooldownTimer = 6f;
                    _linkBreakerTarget = _runtime.FindNearestLinkBreakerTarget(transform.position, 260f);
                    if (_linkBreakerTarget != null) _linkBreakerTelegraphTimer = 0.75f;
                }
            }
            UpdateLinkBreakerLines();
        }

        private void EnsureLinkBreakerLines()
        {
            if (_linkBreakerLines != null) return;
            if (_linkBreakerLineMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader != null) _linkBreakerLineMaterial = new Material(shader);
            }
            _linkBreakerLines = new LineRenderer[5];
            for (var i = 0; i < _linkBreakerLines.Length; i++)
            {
                var lineObject = new GameObject("Link Breaker Telegraph " + i);
                lineObject.transform.SetParent(transform, false);
                var line = lineObject.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.positionCount = (i == 1 || i == 2) ? 3 : 2;
                line.numCapVertices = 2;
                line.numCornerVertices = 2;
                if (_linkBreakerLineMaterial != null) line.material = _linkBreakerLineMaterial;
                line.startWidth = line.endWidth = (i == 0 ? 2.5f : 2f) * SpherePlacementRules.SourcePixelsToWorld;
                line.sortingOrder = 30;
                line.enabled = false;
                _linkBreakerLines[i] = line;
            }
        }

        private void UpdateLinkBreakerLines()
        {
            if (_linkBreakerLines == null) return;
            var telegraph = _linkBreakerTelegraphTimer > 0f && _linkBreakerTarget != null;
            var impact = !telegraph && _linkBreakerImpactTimer > 0f && _linkBreakerImpactTarget != null;
            var target = telegraph ? _linkBreakerTarget : (impact ? _linkBreakerImpactTarget : null);
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                for (var i = 0; i < _linkBreakerLines.Length; i++) _linkBreakerLines[i].enabled = false;
                return;
            }
            var from = (Vector2)transform.position;
            var to = (Vector2)target.transform.position;
            var direction = (to - from).sqrMagnitude > 0.0001f ? (to - from).normalized : Vector2.right;
            var perpendicular = new Vector2(-direction.y, direction.x);
            var scale = SpherePlacementRules.SourcePixelsToWorld;
            var remaining = Mathf.Clamp01(_linkBreakerTelegraphTimer / 0.75f);
            var pulse = 0.65f + 0.35f * Mathf.Sin(Time.time * 16f);
            var alpha = impact ? Mathf.Clamp01(_linkBreakerImpactTimer / 0.30f) : 0.30f + (1f - remaining) * 0.55f;
            var color = new Color(1f, 0.302f, 0.439f, alpha);
            for (var i = 0; i < _linkBreakerLines.Length; i++)
            {
                _linkBreakerLines[i].enabled = telegraph || (impact && i == 0);
                _linkBreakerLines[i].startColor = _linkBreakerLines[i].endColor = color;
            }
            _linkBreakerLines[0].SetPosition(0, new Vector3(from.x, from.y, -0.1f));
            _linkBreakerLines[0].SetPosition(1, new Vector3(to.x, to.y, -0.1f));
            if (!telegraph) return;
            var back = to - direction * (16f * scale);
            var tip = to - direction * (5f * scale);
            var spread = perpendicular * (10f * scale);
            _linkBreakerLines[1].SetPosition(0, new Vector3(back.x - spread.x, back.y - spread.y, -0.1f));
            _linkBreakerLines[1].SetPosition(1, new Vector3(tip.x, tip.y, -0.1f));
            _linkBreakerLines[1].SetPosition(2, new Vector3(back.x + spread.x, back.y + spread.y, -0.1f));
            var forward = to + direction * (16f * scale);
            var forwardTip = to + direction * (5f * scale);
            _linkBreakerLines[2].SetPosition(0, new Vector3(forward.x - spread.x, forward.y - spread.y, -0.1f));
            _linkBreakerLines[2].SetPosition(1, new Vector3(forwardTip.x, forwardTip.y, -0.1f));
            _linkBreakerLines[2].SetPosition(2, new Vector3(forward.x + spread.x, forward.y + spread.y, -0.1f));
            for (var i = 0; i < 2; i++)
            {
                var offset = perpendicular * ((i == 0 ? -15f : 15f) * scale);
                var start = to - direction * (9f * scale) + offset;
                var finish = to + direction * (9f * scale) + offset;
                _linkBreakerLines[3 + i].SetPosition(0, new Vector3(start.x, start.y, -0.1f));
                _linkBreakerLines[3 + i].SetPosition(1, new Vector3(finish.x, finish.y, -0.1f));
            }
            color.a = alpha * (0.55f + 0.35f * pulse);
            for (var i = 1; i < _linkBreakerLines.Length; i++)
                _linkBreakerLines[i].startColor = _linkBreakerLines[i].endColor = color;
        }

    }
}
