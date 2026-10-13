using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed class SphereAttackAgent : MonoBehaviour
    {
        private EchoSphereRuntime _runtime;
        private Transform _player;
        private Vector2 _followOffset;
        private float _attackTimer;
        private float _contactTimer;
        private bool _followEnabled;
        private SphereCombatProfile _profile;
        private SphereId _type;
        private float _orbitAngle;
        private float _initialDamage;
        private float _initialDelay;
        private int _progressionLevel = 1;
        private SphereLevelStats _levelStats;
        private int _branchHitCount;
        private int _chainStormCycle;
        private int _gravityTideHits;
        private readonly List<Transform> _orbitalElements = new List<Transform>();
        private readonly List<float> _orbitalContactTimers = new List<float>();
        private int _orbitalHitCount;
        private float _networkDisabledTimer;
        private float _resonancePulseTimer;
        private int _formationHitCount;
        public void AccelerateNextAttack(float amount) => _attackTimer = Mathf.Max(0f, _attackTimer - Mathf.Max(0f, amount));
        public void DelayNextAttack(float amount) => _attackTimer += Mathf.Max(0f, amount);
        public bool NetworkDisabled => _networkDisabledTimer > 0f;
        public float ResonancePulseTimer => _resonancePulseTimer;
        public int FormationHitCount => _formationHitCount;
        public void AddFormationHitCount(int amount) => _formationHitCount += amount;
        public void TriggerResonancePulse(float duration) =>
            _resonancePulseTimer = Mathf.Max(_resonancePulseTimer, Mathf.Max(0f, duration));
        public void ApplyNetworkDisable(float duration) => _networkDisabledTimer = Mathf.Max(_networkDisabledTimer, Mathf.Max(0f, duration));

        public SphereId Type => _type;
        public float VisualRotation { get; private set; }
        public int ProgressionLevel => _progressionLevel;

        public float BaseDamage { get; private set; } = 12f;
        public float AttackDelay { get; private set; } = 1.2f;
        public float AttackRange { get; private set; } = 6.8f;
        public int PierceCount => _levelStats.Pierce;

        public void Initialize(EchoSphereRuntime runtime, Transform player, SphereId type, float damage)
        {
            _runtime = runtime;
            _player = player;
            _type = type;
            _profile = SphereCombatRules.GetProfile(type);
            _initialDamage = damage * _profile.DamageMultiplier;
            _initialDelay = 1.2f * _profile.DelayMultiplier;
            ApplyProgressionLevel(1);
            AttackRange = 6.8f * _profile.RangeMultiplier * _runtime.GetArtifactSphereRadiusMultiplier(type) *
                _runtime.GetShopSphereRadiusMultiplier() * _runtime.GetCharacterSphereRadiusMultiplier();
            _attackTimer = 0.25f;
            _orbitAngle = type == SphereId.Orbital ? 0f : 0f;
            Color tint;
            if (ColorUtility.TryParseHtmlString(GetColor(type), out tint))
            {
                var renderers = GetComponentsInChildren<SpriteRenderer>();
                for (var i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i].sortingOrder > 8) continue;
                    var alpha = renderers[i].color.a;
                    renderers[i].color = new Color(tint.r, tint.g, tint.b, alpha);
                }
            }
        }

        public void ApplyProgressionLevel(int level)
        {
            // Level 0 is the source's unlevelled but physically placeable Sphere.
            // It uses the base combat profile until the first progression pick.
            _progressionLevel = Mathf.Clamp(level, 0, SphereProgressionRules.MaxLevel);
            _levelStats = _progressionLevel == 0
                ? SphereLevelRules.GetUnlevelledStats()
                : SphereLevelRules.GetStats(_type, _progressionLevel);
            BaseDamage = _initialDamage * _levelStats.DamageMultiplier;
            AttackDelay = Mathf.Max(0.12f, _initialDelay * _levelStats.DelayMultiplier);
            AttackRange = 6.8f * _profile.RangeMultiplier * _levelStats.RangeMultiplier * _runtime.GetArtifactSphereRadiusMultiplier(_type) *
                _runtime.GetShopSphereRadiusMultiplier() * _runtime.GetCharacterSphereRadiusMultiplier();
        }

        public void SetFollowMode(bool enabled, Vector2 playerPosition)
        {
            _followEnabled = enabled;
            if (enabled) _followOffset = (Vector2)transform.position - playerPosition;
        }

        private void LateUpdate()
        {
            if (_runtime == null || _runtime.IsGameplayPaused || _player == null) return;
            if (_type == SphereId.Orbital)
            {
                if (_followEnabled) transform.position = (Vector2)_player.position + _followOffset;
                UpdateOrbitalElements(Time.deltaTime);
                return;
            }
            if (!_followEnabled) return;
            transform.position = (Vector2)_player.position + _followOffset;
        }

        private void Update()
        {
            if (_runtime == null || _runtime.IsGameplayPaused) return;
            var dt = Time.deltaTime;
            _contactTimer = Mathf.Max(0f, _contactTimer - dt);
            _networkDisabledTimer = Mathf.Max(0f, _networkDisabledTimer - dt);
            _resonancePulseTimer = Mathf.Max(0f, _resonancePulseTimer - dt);

            if (_type == SphereId.Orbital)
            {
                UpdateOrbitalContacts(dt);
                return;
            }

            _attackTimer -= dt * _runtime.AttackSpeedMultiplier;
            if (_attackTimer > 0f) return;

            var origin = (Vector2)transform.position;
            var targetRange = _type == SphereId.Prism ? GetPrismSourceRangeWorld() : AttackRange;
            var target = _runtime.FindNearestEnemy(origin, targetRange);
            if (target != null)
            {
                var targetDelta = (Vector2)target.transform.position - origin;
                if (targetDelta.sqrMagnitude > 0.0001f) VisualRotation = Mathf.Atan2(targetDelta.y, targetDelta.x);
            }
            var damage = BaseDamage * _runtime.SphereDamageMultiplier * _runtime.GetNetworkDamageMultiplier(this) *
                _runtime.GetSphereCopyOutputMultiplier(this);
            if (target != null && _runtime.ConsumeResonanceLineBurst()) damage *= 1.60f;
            if (_type == SphereId.Shotgun && target != null)
            {
                var targetDistance = Vector2.Distance(origin, target.transform.position);
                if (targetDistance <= 1.5f) damage *= _levelStats.CloseRangeDamageMultiplier;
                var shotgunBranch = _runtime.GetSphereBranch(_type);
                var shotgunFinal = _runtime.GetSphereFinal(_type);
                if (shotgunBranch == "shotgun_burst")
                    damage *= SphereEvolutionCombatRules.GetShotgunBurstDamageMultiplier(_progressionLevel, shotgunFinal, targetDistance);
                else if (shotgunBranch == "shotgun_cataclysm")
                    damage *= 1.12f;
            }

            if (target != null && EngineerRelayRules.ConsumesOnAttack(_type))
                damage *= _runtime.ConsumeEngineerRelayBonus(this);
            var triggeredEngineerRelay = false;
            switch (_type)
            {
                case SphereId.Standard:
                    FireAt(target, origin, damage, ColorFor(_type));
                    triggeredEngineerRelay = target != null;
                    break;
                case SphereId.Sniper:
                    FireAt(target, origin, damage, ColorFor(_type));
                    triggeredEngineerRelay = target != null;
                    break;
                case SphereId.Shotgun:
                    FireSpread(target, origin, damage);
                    triggeredEngineerRelay = target != null;
                    break;
                case SphereId.Chain:
                    ChainTargets(origin, damage);
                    triggeredEngineerRelay = target != null;
                    break;
                case SphereId.Aura:
                    triggeredEngineerRelay = DamageArea(origin, _profile.AuraRadius / 100f, damage, ColorFor(_type), false);
                    break;
                case SphereId.Prism:
                    FireAtMultiple(origin, damage);
                    // The source Prism cadence opens a Relay window even when
                    // no enemy is available for the direct beam.
                    triggeredEngineerRelay = true;
                    break;
                case SphereId.Gravity:
                    DamageArea(origin, _profile.AuraRadius / 100f, damage, ColorFor(_type), true);
                    // The source Gravity pulse triggers Relay on its own cadence,
                    // independently of whether an enemy is caught in the field.
                    triggeredEngineerRelay = true;
                    break;
                case SphereId.Pulse:
                    DamageArea(origin, _profile.AuraRadius / 100f, damage, ColorFor(_type), false);
                    // The source Pulse cadence triggers Relay even in an empty field.
                    triggeredEngineerRelay = true;
                    break;
                case SphereId.Void:
                    FireVoid(origin, damage);
                    triggeredEngineerRelay = target != null;
                    break;
            }

            if (triggeredEngineerRelay) _runtime.TriggerEngineerRelay(this);
            _attackTimer = Mathf.Max(0.05f, AttackDelay);
        }

        public void OnSphereHitApplied(EnemyAgent2D target) => OnSphereHitApplied(target, _type);

        public void OnSphereHitApplied(EnemyAgent2D target, SphereId statusSource)
        {
            if (target == null || !target.IsAlive || _runtime == null) return;
            var branch = _runtime.GetSphereBranch(_type);
            var finalId = _runtime.GetSphereFinal(_type);
            var element = SphereElementalRules.ResolveElement(branch, finalId);
            if (element == SphereElement.None) return;
            var sourceBranch = _runtime.GetSphereBranch(statusSource);
            var sourceFinalId = _runtime.GetSphereFinal(statusSource);
            var masteryId = SphereElementalRules.GetMasteryId(sourceBranch, sourceFinalId);
            var damageMultiplier = _runtime.GetSphereStatusDamageMultiplier() * _runtime.GetSphereElementalMasteryDamageMultiplier();

            if (_type == SphereId.Chain && SphereElementalRules.IsChainBranch(branch))
            {
                if (!target.TryAdvanceElementalConduction(_type,
                    SphereElementalRules.GetConductionThreshold(masteryId), SphereElementalRules.GetConductionCooldown(masteryId))) return;
                var baseDamage = BaseDamage * _runtime.SphereDamageMultiplier *
                    _runtime.GetNetworkDamageMultiplier(this) * _runtime.GetSphereCopyOutputMultiplier(this);
                target.ReceiveSphereDamage(_type, baseDamage * SphereElementalRules.GetConductionDamageMultiplier(masteryId));
                if (target.IsAlive)
                {
                    var durationMultiplier = _runtime.GetSphereStatusDurationMultiplier(element);
                    var status = SphereElementalRules.CreateStatus(element, masteryId, SphereElementalMode.Conduction,
                        durationMultiplier, damageMultiplier);
                    target.ApplyElementalStatus(status, statusSource);
                }
                _runtime.SpawnImpact(target.transform.position, ElementColor(element));
                return;
            }

            if ((_type == SphereId.Aura || _type == SphereId.Gravity || _type == SphereId.Pulse) &&
                SphereElementalRules.IsFieldBranch(branch))
            {
                if (!target.TryBeginFieldElementalReaction(SphereElementalRules.GetFieldCooldown(masteryId))) return;
                var durationMultiplier = _runtime.GetSphereStatusDurationMultiplier(element);
                var status = SphereElementalRules.CreateStatus(element, masteryId, SphereElementalMode.Field,
                    durationMultiplier, damageMultiplier);
                target.ApplyElementalStatus(status, statusSource);
                return;
            }

            var directDurationMultiplier = _runtime.GetSphereStatusDurationMultiplier(element);
            var directStatus = SphereElementalRules.CreateStatus(element, masteryId, SphereElementalMode.Direct,
                directDurationMultiplier, damageMultiplier);
            target.ApplyElementalStatus(directStatus, statusSource);
        }

        private static Color ElementColor(SphereElement element)
        {
            switch (element)
            {
                case SphereElement.Fire: return new Color(1f, 0.45f, 0.24f, 0.95f);
                case SphereElement.Freeze: return new Color(0.85f, 0.96f, 1f, 0.95f);
                case SphereElement.Poison: return new Color(0.45f, 0.94f, 0.56f, 0.95f);
                default: return Color.white;
            }
        }

        private void UpdateOrbitalElements(float dt)
        {
            var finalId = _runtime.GetSphereFinal(SphereId.Orbital);
            var innerCount = SphereEvolutionCombatRules.GetOrbitalInnerCount(_progressionLevel, finalId);
            var outerCount = SphereEvolutionCombatRules.GetOrbitalOuterCount(_progressionLevel, finalId);
            EnsureOrbitalElements(innerCount + outerCount);
            _orbitAngle += dt * SphereEvolutionCombatRules.GetOrbitalAngularSpeed(_progressionLevel, finalId) * _levelStats.OrbitSpeedMultiplier;
            for (var i = 0; i < _orbitalElements.Count; i++)
            {
                var inner = i < innerCount;
                var index = inner ? i : i - innerCount;
                var count = inner ? innerCount : outerCount;
                var angle = _orbitAngle * (inner ? 1f : -1f) + index * Mathf.PI * 2f / Mathf.Max(1, count);
                var radius = (inner ? 0.72f : 0.98f) * _levelStats.OrbitRadiusMultiplier;
                _orbitalElements[i].localPosition = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, -0.08f);
            }
        }

        private void EnsureOrbitalElements(int count)
        {
            while (_orbitalElements.Count < count)
            {
                var index = _orbitalElements.Count;
                var go = new GameObject("Orbital Combat Element " + (index + 1));
                go.transform.SetParent(transform, false);
                go.transform.localScale = Vector3.one * (index % 2 == 0 ? 0.24f : 0.20f);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeSpriteFactory.Disc;
                renderer.color = new Color(0.45f, 0.94f, 1f, 1f);
                renderer.sortingOrder = 13;
                var glow = new GameObject("Orbital Element Glow");
                glow.transform.SetParent(go.transform, false);
                glow.transform.localScale = Vector3.one * 2.2f;
                var glowRenderer = glow.AddComponent<SpriteRenderer>();
                glowRenderer.sprite = RuntimeSpriteFactory.Disc;
                glowRenderer.color = new Color(0.35f, 0.82f, 1f, 0.30f);
                glowRenderer.sortingOrder = 12;
                _orbitalElements.Add(go.transform);
                _orbitalContactTimers.Add(0f);
            }
        }

        private void UpdateOrbitalContacts(float dt)
        {
            var hitSomething = false;
            var branch = _runtime.GetSphereBranch(SphereId.Orbital);
            var finalId = _runtime.GetSphereFinal(SphereId.Orbital);
            var contactRadius = SphereEvolutionCombatRules.GetOrbitalContactRadius(finalId);
            var damageMultiplier = SphereEvolutionCombatRules.GetOrbitalDamageMultiplier(_progressionLevel, branch, finalId);
            for (var i = 0; i < _orbitalElements.Count; i++)
            {
                _orbitalContactTimers[i] = Mathf.Max(0f, _orbitalContactTimers[i] - dt);
                if (_orbitalContactTimers[i] > 0f) continue;
                var contact = _runtime.TryHitEnemy(_orbitalElements[i].position, contactRadius);
                if (contact == null) continue;
                hitSomething = true;
                var damage = BaseDamage * _runtime.SphereDamageMultiplier * _runtime.GetNetworkDamageMultiplier(this) *
                    _runtime.GetSphereCopyOutputMultiplier(this) * damageMultiplier;
                var dealtDamage = contact.ReceiveSphereDamage(this, damage);
                OnSphereHitApplied(contact);
                _runtime.AddResonanceCharge(1f);
                _orbitalHitCount++;
                if (branch == "orbital_halo")
                {
                    var bonusCharge = SphereEvolutionCombatRules.GetOrbitalResonanceBonus(_progressionLevel, finalId);
                    if (bonusCharge > 0f) _runtime.AddResonanceCharge(bonusCharge);
                    if (finalId == "orbital_halo_final_3" &&
                        SphereEvolutionCombatRules.ShouldOrbitalGrantShieldCharge(_orbitalHitCount))
                        _runtime.GrantPlayerShieldCharge(1);
                }
                _runtime.SpawnImpact(contact.transform.position, new Color(0.56f, 0.94f, 1f));
                var afterimage = SphereEvolutionCombatRules.GetOrbitalAfterimageMultiplier(branch, finalId);
                if (afterimage > 0f) _runtime.TriggerOrbitalAfterimage(contact, damage * afterimage, 0.65f);
                _orbitalContactTimers[i] = 0.28f;
            }
            if (hitSomething) _runtime.TriggerEngineerRelay(this);
        }

        private void FireAt(EnemyAgent2D target, Vector2 origin, float damage, Color tint)
        {
            if (target == null) return;
            var direction = ((Vector2)target.transform.position - origin).normalized;
            var count = 1 + _runtime.GetSphereAuthoredModifierLevel(_type, SphereAuthoredModifier.Multishot);
            var pierce = _levelStats.Pierce + _runtime.GetSphereProjectilePierceBonus(_type);
            var spread = _profile.Spread * _levelStats.SpreadMultiplier;
            if (spread <= 0.0001f) spread = 0.15f;
            for (var i = 0; i < count; i++)
            {
                var t = count == 1 ? 0f : (float)i / (count - 1) - 0.5f;
                SpawnAuthoredMainProjectile(origin, Rotate(direction, t * spread * Mathf.Rad2Deg), damage,
                    tint, 8f * _profile.ProjectileSpeedMultiplier, pierce, _levelStats.CritChanceBonus);
            }
        }

        private void SpawnAuthoredMainProjectile(Vector2 origin, Vector2 direction, float damage,
            Color tint, float speed, int pierce, float critChanceBonus)
        {
            var ricochets = _runtime.GetSphereProjectileRicochetCount(_type);
            _runtime.SpawnProjectile(origin, direction, damage, tint, speed, pierce, critChanceBonus,
                this, ricochets, 2.0f, true, true, true);

            var echoLevel = _runtime.GetSphereAuthoredModifierLevel(_type, SphereAuthoredModifier.Echo);
            var echoChance = SphereAuthoredModifierRules.GetEchoChance(echoLevel);
            if (echoChance <= 0f || !_runtime.RollCombatChance(echoChance)) return;
            var echoAngle = (_runtime.NextRunRandom() - 0.5f) * 10.31324f;
            _runtime.SpawnProjectile(origin, Rotate(direction, echoAngle), damage * 0.28f, tint,
                speed, 0, 0f, this, 0, 1.1f, false, false, false);
        }

        private void FireSpread(EnemyAgent2D target, Vector2 origin, float damage)
        {
            if (target == null) return;
            var direction = ((Vector2)target.transform.position - origin).normalized;
            var targetDistance = Vector2.Distance(origin, target.transform.position);
            var branch = _runtime.GetSphereBranch(_type);
            var finalId = _runtime.GetSphereFinal(_type);
            var count = Mathf.Max(1, _profile.Pellets + _levelStats.Pellets - 3);
            if (branch == "shotgun_hail")
                count += SphereEvolutionCombatRules.GetShotgunHailBonusPellets(finalId);
            else if (branch == "shotgun_burst")
                count += SphereEvolutionCombatRules.GetShotgunBurstExtraPellets(_progressionLevel, finalId, targetDistance);
            count += _runtime.GetSphereAuthoredModifierLevel(_type, SphereAuthoredModifier.Multishot);
            var pierce = branch == "shotgun_cataclysm"
                ? SphereEvolutionCombatRules.GetShotgunCataclysmPierce(_levelStats.Pierce, finalId)
                : _levelStats.Pierce;
            pierce += _runtime.GetSphereProjectilePierceBonus(_type);
            for (var i = 0; i < count; i++)
            {
                var t = count == 1 ? 0f : (float)i / (count - 1) - 0.5f;
                var angle = t * _profile.Spread * _levelStats.SpreadMultiplier * Mathf.Rad2Deg;
                var shot = Rotate(direction, angle);
                SpawnAuthoredMainProjectile(origin, shot, damage, ColorFor(_type), 8f, pierce, 0f);
            }
        }

        private void ChainTargets(Vector2 origin, float damage)
        {
            var targets = _runtime.FindNearestEnemies(origin, AttackRange, _levelStats.ChainTargets);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                _runtime.SpawnCombatArc(origin, target.transform.position, SphereId.Chain,
                    _runtime.GetSphereBranch(_type), _runtime.GetSphereFinal(_type), ColorFor(_type));
                _runtime.RegisterCharacterSphereHit(_type, target);
                var hitDamage = _runtime.ResolveProjectileDamage(damage * Mathf.Pow(0.72f, i), _levelStats.CritChanceBonus, out var wasCritical, target);
                var direction = ((Vector2)target.transform.position - origin).normalized;
                hitDamage = _runtime.ResolveAuthoredSphereHitDamage(_type, target, hitDamage);
                hitDamage = OnProjectileHit(target, direction, hitDamage, wasCritical);
                hitDamage = target.ReceiveSphereDamage(_type, hitDamage, true, true, this, true);
                OnSphereHitApplied(target);
                _runtime.RegisterSphereHit(this, target, hitDamage);
                _runtime.SpawnImpact(target.transform.position, ColorFor(_type));
            }
        }

        // Source mapping: new-desing/src/engineSpheres.ts:updatePrismSphere.
        // Prism resolves each beam immediately; it is not a travelling projectile.
        private float GetPrismSourceRangeWorld() =>
            SphereEvolutionCombatRules.GetPrismRangeWorld(
                _progressionLevel, _runtime.GetSphereBranch(SphereId.Prism),
                _runtime.GetArtifactSphereRadiusMultiplier(SphereId.Prism));

        private void FireAtMultiple(Vector2 origin, float damage)
        {
            var branch = _runtime.GetSphereBranch(_type);
            var finalId = _runtime.GetSphereFinal(_type);
            var beamRange = GetPrismSourceRangeWorld();
            var candidates = _runtime.FindEnemiesInRadius(origin, beamRange);
            if (candidates.Count == 0) return;

            var targetCount = SphereEvolutionCombatRules.GetPrismBeamCount(
                _levelStats.PrismDirections, branch, finalId,
                _runtime.GetSphereAuthoredModifierLevel(_type, SphereAuthoredModifier.Multishot));
            var hasActiveStatus = SphereElementalRules.ResolveElement(branch, finalId) != SphereElement.None;
            var sphereIndex = _runtime.GetSphereIndex(this);
            var formationMultiplier = SphereEvolutionCombatRules.GetPrismFormationDamageMultiplier(
                _runtime.GetFormationBonus(SphereNetworkFormation.Line, sphereIndex),
                _runtime.GetFormationBonus(SphereNetworkFormation.Lattice, sphereIndex));
            var usedTargets = new HashSet<int>();
            EnemyAgent2D primaryTarget = null;

            for (var beam = 0; beam < targetCount; beam++)
            {
                EnemyAgent2D beamTarget = null;
                var highestHp = float.NegativeInfinity;
                for (var i = 0; i < candidates.Count; i++)
                {
                    var candidate = candidates[i];
                    if (candidate == null || !candidate.IsAlive || usedTargets.Contains(candidate.GetInstanceID())) continue;
                    if (candidate.CurrentHp <= highestHp) continue;
                    highestHp = candidate.CurrentHp;
                    beamTarget = candidate;
                }
                if (beamTarget == null) break;

                usedTargets.Add(beamTarget.GetInstanceID());
                if (beam == 0)
                {
                    primaryTarget = beamTarget;
                    var direction = (Vector2)beamTarget.transform.position - origin;
                    if (direction.sqrMagnitude > 0.0001f)
                        VisualRotation = Mathf.Atan2(direction.y, direction.x);
                }

                var beamMultiplier = SphereEvolutionCombatRules.GetPrismBeamDamageMultiplier(
                    _progressionLevel, branch, finalId, beam, hasActiveStatus);
                DealPrismBeamHit(beamTarget, damage * beamMultiplier * formationMultiplier);
                _runtime.SpawnCombatArc(origin, beamTarget.transform.position, SphereId.Prism,
                    branch, finalId, ColorFor(_type), 0.10f);
            }

            if (primaryTarget == null) return;
            ResolvePrismNetworkRefraction(primaryTarget, damage, branch, finalId);

            if (branch == "prism_spectrum" &&
                _runtime.HasNetworkCandidateForSphere(SphereNetworkFormation.Triangle, sphereIndex))
            {
                var triangleBonus = _runtime.GetFormationBonus(SphereNetworkFormation.Triangle, sphereIndex);
                _runtime.ChargeResonanceFromSource(
                    ResonanceRules.NetworkCharge * (triangleBonus > 0f ? triangleBonus : 1f), true);
            }
        }

        private void DealPrismBeamHit(EnemyAgent2D target, float damage)
        {
            if (target == null || !target.IsAlive || _runtime == null) return;
            _runtime.RegisterCharacterSphereHit(SphereId.Prism, target);
            var resolvedDamage = _runtime.ResolveProjectileDamage(damage, _levelStats.CritChanceBonus, out _, target);
            resolvedDamage = _runtime.ResolveAuthoredSphereHitDamage(SphereId.Prism, target, resolvedDamage);
            var dealtDamage = target.ReceiveSphereDamage(SphereId.Prism, resolvedDamage, true, true, this, true);
            OnSphereHitApplied(target);
            _runtime.RegisterSphereHit(this, target, dealtDamage);
        }

        private void ResolvePrismNetworkRefraction(
            EnemyAgent2D primaryTarget, float damage, string branch, string finalId)
        {
            var prismFilter = _runtime.HasArtifact("prism_filter");
            if (branch != "prism_mirror" && !prismFilter) return;

            var sphereIndex = _runtime.GetSphereIndex(this);
            var ringBonus = _runtime.GetFormationBonus(SphereNetworkFormation.Ring, sphereIndex);
            var ringReflection = ringBonus >= 1f || (ringBonus > 0f && _runtime.RollCombatChance(ringBonus));
            var reflectionCount = SphereEvolutionCombatRules.GetPrismReflectionCount(
                branch, finalId, prismFilter, _runtime.HasArtifact("prism_crown"), ringReflection);
            if (reflectionCount <= 0) return;

            var linkedSpheres = _runtime.GetLinkedSpheres(sphereIndex);
            var relayCount = Mathf.Min(reflectionCount, linkedSpheres.Count);
            for (var i = 0; i < relayCount; i++)
            {
                var relay = linkedSpheres[i];
                if (relay == null) continue;
                var relayOrigin = (Vector2)relay.transform.position;
                _runtime.SpawnCombatArc(relayOrigin, primaryTarget.transform.position, SphereId.Prism,
                    branch, finalId, new Color(1f, 0.55f, 0.88f, 0.95f), 0.12f);
                DealPrismRelayHit(relay, primaryTarget,
                    damage * SphereEvolutionCombatRules.GetPrismNetworkRelayDamageMultiplier());

                var ricochetModifier = SphereAuthoredModifierRules.GetProjectileRicochetCount(branch, finalId);
                var bounceCount = SphereEvolutionCombatRules.GetPrismMirrorRicochetCount(
                    _progressionLevel, branch, finalId, ricochetModifier);
                var bounceRange = SphereEvolutionCombatRules.GetPrismRicochetRangeWorld(_progressionLevel, finalId);
                var visited = new HashSet<int> { primaryTarget.GetInstanceID() };
                var bounceTarget = primaryTarget;

                for (var bounce = 0; bounce < bounceCount; bounce++)
                {
                    if (bounceTarget == null) break;
                    var center = (Vector2)bounceTarget.transform.position;
                    var nearby = _runtime.FindEnemiesInRadius(center, bounceRange);
                    EnemyAgent2D next = null;
                    var nearestDistanceSquared = bounceRange * bounceRange;
                    for (var candidateIndex = 0; candidateIndex < nearby.Count; candidateIndex++)
                    {
                        var candidate = nearby[candidateIndex];
                        if (candidate == null || !candidate.IsAlive || visited.Contains(candidate.GetInstanceID())) continue;
                        var distanceSquared = ((Vector2)candidate.transform.position - center).sqrMagnitude;
                        if (distanceSquared >= nearestDistanceSquared) continue;
                        nearestDistanceSquared = distanceSquared;
                        next = candidate;
                    }
                    if (next == null) break;

                    visited.Add(next.GetInstanceID());
                    _runtime.SpawnCombatArc(center, next.transform.position, SphereId.Prism,
                        branch, finalId, new Color(1f, 0.55f, 0.88f, 0.95f), 0.12f);
                    DealPrismRelayHit(relay, next,
                        damage * SphereEvolutionCombatRules.GetPrismRicochetDamageMultiplier(
                            bounce, _progressionLevel, finalId));
                    bounceTarget = next;
                }

                if (finalId == "prism_mirror_final_3" &&
                    _runtime.GetSphereAuthoredModifierLevel(SphereId.Prism, SphereAuthoredModifier.Echo) > 0 &&
                    bounceTarget != null && bounceTarget.IsAlive)
                {
                    DealPrismRelayHit(relay, bounceTarget,
                        damage * SphereEvolutionCombatRules.GetPrismEchoDamageMultiplier());
                }
            }
        }

        private float DealPrismRelayHit(SphereAttackAgent relay, EnemyAgent2D target, float sourceDamage)
        {
            if (relay == null || target == null || !target.IsAlive || _runtime == null) return 0f;

            // Source damage starts with the Prism's attack stats, but the receiving
            // Sphere supplies target-side procs/conditional modifiers for the relay.
            // The ratio prevents reapplying the receiver's global damage factors.
            var prismDamageFactor = _runtime.GetSphereAuthoredDamageMultiplier(SphereId.Prism);
            var relayDamageFactor = _runtime.GetSphereAuthoredDamageMultiplier(relay.Type);
            var damage = sourceDamage * prismDamageFactor / Mathf.Max(0.0001f, relayDamageFactor);
            _runtime.RegisterCharacterSphereHit(relay.Type, target);
            damage = _runtime.ResolveProjectileDamage(damage, relay._levelStats.CritChanceBonus, out _, target);
            damage = _runtime.ResolveAuthoredSphereHitDamage(relay.Type, target, damage);
            var dealtDamage = target.ReceiveSphereDamage(relay.Type, damage, true, false, relay, true);
            OnSphereHitApplied(target, relay.Type);
            _runtime.RegisterSphereHit(relay, target, dealtDamage);
            return dealtDamage;
        }

        private bool DamageArea(Vector2 origin, float radius, float damage, Color tint, bool pull)
        {
            radius *= _levelStats.AuraRadiusMultiplier;
            var branch = _runtime.GetSphereBranch(_type);
            var finalId = _runtime.GetSphereFinal(_type);
            if (_type == SphereId.Pulse && branch == "pulse_wave")
                radius *= SphereEvolutionCombatRules.GetPulseWaveRadiusMultiplier(_progressionLevel, finalId);
            var targets = _runtime.FindEnemiesInRadius(origin, radius);
            var auraDamage = damage;

            if (_type == SphereId.Pulse && branch == "pulse_wave")
            {
                var knockback = SphereEvolutionCombatRules.GetPulseWaveKnockbackDistance(_progressionLevel, finalId);
                for (var i = 0; i < targets.Count; i++)
                {
                    targets[i].KnockBackFrom(origin, knockback);
                    if (finalId == "pulse_wave_final_2" || finalId == "pulse_wave_final_3")
                        targets[i].ApplySlow(0.8f, finalId == "pulse_wave_final_2" ? 0.42f : 0.58f);
                }
            }
            else if (_type == SphereId.Pulse && branch == "pulse_burst")
            {
                if (finalId == "pulse_burst_final_2" || finalId == "pulse_burst_final_3")
                    for (var i = 0; i < targets.Count; i++) targets[i].KnockBackFrom(origin, 0.35f);
                if (SphereEvolutionCombatRules.ShouldPulseBurstDoubleWave(_progressionLevel, finalId))
                {
                    var secondTargets = _runtime.FindEnemiesInRadius(origin, radius * 0.68f);
                    var secondDamage = damage * SphereEvolutionCombatRules.GetPulseBurstSecondaryDamageMultiplier(_progressionLevel, finalId);
                    for (var i = 0; i < secondTargets.Count; i++)
                    {
                        if (finalId == "pulse_burst_final_3") secondTargets[i].KnockBackFrom(origin, 0.30f);
                        var secondDealt = secondTargets[i].ReceiveSphereDamage(this, secondDamage);
                        OnSphereHitApplied(secondTargets[i]);
                        _runtime.RegisterSphereHit(this, secondTargets[i], secondDealt);
                    }
                    if (secondTargets.Count > 0) _runtime.SpawnImpact(origin, new Color(1f, 0.72f, 0.34f, 0.85f));
                }
            }
            else if (_type == SphereId.Pulse && branch == "pulse_resonator")
            {
                var index = _runtime.GetSphereIndex(this);
                var formationBonus = Mathf.Max(_runtime.GetFormationBonus(SphereNetworkFormation.Triangle, index),
                    Mathf.Max(_runtime.GetFormationBonus(SphereNetworkFormation.Lattice, index), _runtime.GetFormationBonus(SphereNetworkFormation.Ring, index)));
                if (formationBonus > 0f) _runtime.ChargeResonanceFromSource(ResonanceRules.NetworkCharge * formationBonus, true);
                _runtime.SpawnImpact(origin, new Color(1f, 0.88f, 0.38f, 0.9f));
            }
            if (_type == SphereId.Pulse && branch == "pulse_burst")
            {
                var index = _runtime.GetSphereIndex(this);
                var network = _runtime.NetworkState;
                var clusterCandidate = false;
                if (network != null)
                    for (var i = 0; i < network.FormationCandidates.Count; i++)
                        if (network.FormationCandidates[i].Type == SphereNetworkFormation.Cluster &&
                            network.FormationCandidates[i].Contains(index))
                        {
                            clusterCandidate = true;
                            break;
                        }
                if (clusterCandidate)
                    _runtime.ChargeResonanceFromSource(ResonanceRules.GeometryCharge, true);
            }

            var gravityIndex = _runtime.GetSphereIndex(this);
            var clusterBonus = _runtime.GetFormationBonus(SphereNetworkFormation.Cluster, gravityIndex);
            var clusterPullMultiplier = SphereEvolutionCombatRules.GetGravityClusterPullMultiplier(clusterBonus);
            if (_type == SphereId.Gravity && branch == "gravity_well")
            {
                var control = SphereEvolutionCombatRules.GetGravityWellSlow(finalId);
                _runtime.ApplyGravityWellControl(origin, SphereEvolutionCombatRules.GetGravityWellRadius(_progressionLevel, finalId), SphereEvolutionCombatRules.GetGravityWellPullDistance(_progressionLevel, finalId) * clusterPullMultiplier, control.duration, control.multiplier);
            }
            else if (_type == SphereId.Gravity && branch == "gravity_tide")
            {
                _gravityTideHits++;
                var mode = finalId == "gravity_tide_final_1" ? -1 : finalId == "gravity_tide_final_2" ? 1 : finalId == "gravity_tide_final_3" ? (_gravityTideHits % 2 == 0 ? 1 : -1) : 1;
                _runtime.ApplyGravityTidePulse(origin, SphereEvolutionCombatRules.GetGravityTideRadius(finalId), SphereEvolutionCombatRules.GetGravityTideDistance(_progressionLevel) * clusterPullMultiplier, mode);
            }
            else if (_type == SphereId.Gravity && branch == "gravity_collapse")
            {
                var grouped = targets.Count;
                var multiplier = 1f;
                if (grouped >= 4) multiplier *= finalId == "gravity_collapse_final_3" ? 1.25f : 1.20f;
                for (var i = 0; i < targets.Count; i++)
                    if (targets[i].HpFraction < 0.45f)
                        multiplier = Mathf.Max(multiplier, finalId == "gravity_collapse_final_1" ? 1.30f : 1.12f);
                if (finalId == "gravity_collapse_final_3" && grouped >= 4)
                    _runtime.TriggerGravityCollapse(origin, damage * 0.30f, radius * 1.25f);
                auraDamage *= multiplier;
                _runtime.ApplyGravityWellControl(origin, radius, 0.32f * clusterPullMultiplier, 0f, 1f);
            }
            else if (_type == SphereId.Aura && branch == "aura_sanctum")
            {
                var slowDuration = SphereEvolutionCombatRules.GetAuraSanctumSlowDuration(finalId);
                var slowMultiplier = SphereEvolutionCombatRules.GetAuraSanctumSlowMultiplier(finalId);
                for (var i = 0; i < targets.Count; i++) targets[i].ApplySlow(slowDuration, slowMultiplier);
                auraDamage *= finalId == "aura_sanctum_final_3" ? 1.12f : 1f;
                _runtime.AccelerateNearbySpheres(this, 2.4f, 0.12f);
            }
            else if (_type == SphereId.Aura && branch == "aura_gravity")
            {
                _runtime.ApplyAuraGravityControl(origin,
                    SphereEvolutionCombatRules.GetAuraGravityPullRadius(_progressionLevel, finalId),
                    SphereEvolutionCombatRules.GetAuraGravityPullDistance(_progressionLevel, finalId),
                    finalId == "aura_gravity_final_2");
                if (finalId == "aura_gravity_final_3") auraDamage *= 1.18f;
            }
            else if (_type == SphereId.Aura && branch == "aura_overgrowth")
            {
                var boostRadius = SphereEvolutionCombatRules.GetAuraOvergrowthRadius(_progressionLevel, finalId);
                var reduction = SphereEvolutionCombatRules.GetAuraOvergrowthAttackTimerReduction(_progressionLevel, finalId);
                _runtime.AccelerateNearbySpheres(this, boostRadius, reduction);
                if (finalId == "aura_overgrowth_final_3") auraDamage *= 1.12f;
            }

            for (var i = 0; i < targets.Count; i++)
            {
                if (pull && !(_type == SphereId.Gravity && !string.IsNullOrEmpty(branch))) targets[i].PullToward(origin, 1.8f * _levelStats.PullMultiplier);
                var dealtDamage = targets[i].ReceiveSphereDamage(this, auraDamage);
                OnSphereHitApplied(targets[i]);
                _runtime.RegisterSphereHit(this, targets[i], dealtDamage);
            }
            if (targets.Count > 0) _runtime.SpawnImpact(origin, tint);
            return targets.Count > 0;
        }

        private void FireVoid(Vector2 origin, float damage)
        {
            var targets = _runtime.FindNearestEnemies(origin, AttackRange, 3);
            if (targets.Count == 0) return;
            targets.Sort((a, b) => a.HpFraction.CompareTo(b.HpFraction));
            var target = targets[0];
            var multiplier = target.HpFraction <= 0.25f ? 2.5f * _levelStats.WeakenedDamageMultiplier : 1f;
            var direction = ((Vector2)target.transform.position - origin).normalized;
            var voidPierce = SphereEvolutionCombatRules.GetVoidExecutionPierce(_levelStats.Pierce, _runtime.GetSphereFinal(_type));
            SpawnAuthoredMainProjectile(origin, direction, damage * multiplier, ColorFor(_type),
                8f * _profile.ProjectileSpeedMultiplier, voidPierce, 0f);
        }


        public float OnProjectileHit(EnemyAgent2D target, Vector2 direction, float dealtDamage, bool wasCritical)
        {
            if (target == null || _runtime == null) return dealtDamage;
            var branch = _runtime.GetSphereBranch(_type);
            if (string.IsNullOrEmpty(branch)) return dealtDamage;
            var finalId = _runtime.GetSphereFinal(_type);
            _branchHitCount++;

            if (_type == SphereId.Standard && branch == "standard_resonator")
            {
                if (SphereEvolutionCombatRules.ShouldTriggerStandardResonatorPulse(_branchHitCount))
                {
                    var pulseDamage = dealtDamage * SphereEvolutionCombatRules.GetStandardResonatorPulseDamageMultiplier(_progressionLevel, finalId);
                    var radius = SphereEvolutionCombatRules.GetStandardResonatorPulseRadius(_progressionLevel, finalId);
                    _runtime.TriggerStandardResonatorPulse(target.transform.position, pulseDamage, radius, finalId);
                }
                return dealtDamage;
            }
            if (_type == SphereId.Standard && branch == "standard_singularity")
            {
                var pull = SphereEvolutionCombatRules.GetStandardSingularityPullDistance(_progressionLevel);
                var slow = SphereEvolutionCombatRules.GetStandardSingularitySlowDuration(_progressionLevel);
                if (finalId == "standard_singularity_final_1" || finalId == "standard_singularity_final_3") pull *= 1.25f;
                if (finalId == "standard_singularity_final_2") slow = Mathf.Max(slow, 1.8f);
                var receivesCollapseBonus = finalId == "standard_singularity_final_3" && target.HpFraction <= 0.5f;
                _runtime.TriggerStandardSingularity(target, pull, slow);
                if (receivesCollapseBonus && target != null) target.ReceiveSphereDamage(_type, dealtDamage * 0.15f);
                return dealtDamage;
            }
            if (_type == SphereId.Standard && branch == "standard_swarm")
            {
                var count = SphereEvolutionCombatRules.GetStandardSwarmShardCount(_progressionLevel, finalId);
                _runtime.SpawnStandardSwarmShards(target, direction, count, dealtDamage * SphereEvolutionCombatRules.GetStandardSwarmShardDamageMultiplier(_progressionLevel));
                return dealtDamage;
            }
            if (_type == SphereId.Void && branch == "void_hunger")
            {
                return dealtDamage * SphereEvolutionCombatRules.GetVoidHungerDamageMultiplier(_progressionLevel, finalId, target.HpFraction);
            }
            if (_type == SphereId.Void && branch == "void_reaper")
            {
                var multiplier = target.HpFraction <= 0.25f ? (finalId == "void_reaper_final_1" ? 1.25f : 1.12f) : 1f;
                var expectedKill = dealtDamage * multiplier >= target.CurrentHp;
                if (expectedKill)
                {
                    _runtime.HealPlayer(SphereEvolutionCombatRules.GetVoidReaperKillHeal(_progressionLevel, finalId));
                    _runtime.SpawnVoidShards(target, Mathf.Max(4f, dealtDamage * SphereEvolutionCombatRules.GetVoidReaperShardDamageMultiplier(finalId)), SphereEvolutionCombatRules.GetVoidReaperShardCount(_progressionLevel, finalId));
                }
                return dealtDamage * multiplier;
            }
            if (_type == SphereId.Void && branch == "void_execution")
            {
                var threshold = SphereEvolutionCombatRules.GetVoidExecutionThreshold(_progressionLevel, finalId);
                var chance = SphereEvolutionCombatRules.GetVoidExecutionChance(_progressionLevel, finalId);
                if (target.HpFraction <= threshold && _runtime.RollCombatChance(chance))
                    return Mathf.Max(dealtDamage, target.CurrentHp + 1f);
                return dealtDamage;
            }
            if (_type == SphereId.Chain && branch == "chain_web")
            {
                var slowDuration = SphereEvolutionCombatRules.GetChainWebSlowDuration(_progressionLevel, finalId);
                var slowMultiplier = SphereEvolutionCombatRules.GetChainWebSlowMultiplier(_progressionLevel, finalId);
                target.ApplySlow(slowDuration, slowMultiplier);
                var nearby = _runtime.FindEnemiesInRadius(target.transform.position, 1.8f);
                for (var i = 0; i < nearby.Count; i++)
                    if (nearby[i] != target) nearby[i].ApplySlow(slowDuration * 0.65f, slowMultiplier);
                return dealtDamage * (finalId == "chain_web_final_3" ? 1.25f : 1f);
            }
            if (_type == SphereId.Chain && branch == "chain_storm")
            {
                _runtime.TriggerChainStorm(this, target, dealtDamage, _progressionLevel, finalId);
                _chainStormCycle++;
                if (finalId == "chain_storm_final_3" && _chainStormCycle % 3 == 0)
                    _runtime.TriggerChainStormExtraStrike(target, dealtDamage * 0.38f);
                return dealtDamage;
            }
            if (_type == SphereId.Chain && branch == "chain_leech")
            {
                var healRatio = SphereEvolutionCombatRules.GetChainLeechHealRatio(_progressionLevel, finalId);
                _runtime.HealPlayer(dealtDamage * healRatio);
                return dealtDamage * (finalId == "chain_leech_final_3" && target.HpFraction < 0.40f ? 1.20f : 1f);
            }
            if (_type == SphereId.Shotgun && branch == "shotgun_burst")
            {
                var targetDistance = Vector2.Distance(transform.position, target.transform.position);
                if (SphereEvolutionCombatRules.ShouldShotgunBurstApplySlow(_progressionLevel, finalId, targetDistance))
                    target.ApplySlow(0.8f, 0.62f);
                return dealtDamage;
            }
            if (_type == SphereId.Shotgun && branch == "shotgun_cataclysm")
            {
                _runtime.TriggerShotgunCataclysm(target, dealtDamage, _progressionLevel, finalId);
                return dealtDamage;
            }
            if (_type == SphereId.Shotgun && branch == "shotgun_hail")
            {
                var shardCount = SphereEvolutionCombatRules.GetShotgunHailShardCount(_progressionLevel, finalId);
                var chance = SphereEvolutionCombatRules.GetShotgunHailChance(_progressionLevel, finalId);
                if (_runtime.RollCombatChance(chance))
                    _runtime.TriggerShotgunHail(target, direction, dealtDamage, _progressionLevel, finalId, shardCount);
                return dealtDamage * (finalId == "shotgun_hail_final_3" ? 1.08f : 1f);
            }
            if (_type == SphereId.Sniper && branch == "sniper_oracle")
            {
                var alreadyMarked = target.IsMarked;
                var multiplier = alreadyMarked && wasCritical ? SphereEvolutionCombatRules.GetSniperOracleCriticalMultiplier(finalId) : 1f;
                if (finalId == "sniper_oracle_final_3" && target.HpFraction <= 0.30f) multiplier *= 1.22f;
                target.ApplyMark(SphereEvolutionCombatRules.GetSniperOracleMarkDuration(finalId));
                if (finalId == "sniper_oracle_final_3" && alreadyMarked && wasCritical)
                    _runtime.TriggerSniperOracleSplash(target, dealtDamage * 0.22f, 1.15f);
                return dealtDamage * multiplier;
            }
            if (_type == SphereId.Sniper && branch == "sniper_assassin")
            {
                var threshold = SphereEvolutionCombatRules.GetSniperAssassinThreshold(finalId, _progressionLevel);
                var multiplier = SphereEvolutionCombatRules.GetSniperAssassinDamageMultiplier(_progressionLevel, finalId, target.HpFraction);
                var expectedKill = dealtDamage * multiplier >= target.CurrentHp;
                if (finalId == "sniper_assassin_final_1" && target.HpFraction <= threshold) target.ApplyMark(1.1f);
                if (finalId == "sniper_assassin_final_3" && expectedKill && target.HpFraction <= threshold)
                    _runtime.HealPlayer(SphereEvolutionCombatRules.GetSniperAssassinKillHeal(finalId));
                return dealtDamage * multiplier;
            }
            if (_type == SphereId.Sniper && branch == "sniper_beacon")
            {
                var duration = SphereEvolutionCombatRules.GetSniperBeaconMarkDuration(_progressionLevel, finalId);
                var radius = SphereEvolutionCombatRules.GetSniperBeaconRadius(_progressionLevel, finalId);
                var slowMultiplier = SphereEvolutionCombatRules.GetSniperBeaconSlowMultiplier(finalId);
                target.ApplyMark(duration);
                var nearby = _runtime.FindEnemiesInRadius(target.transform.position, radius);
                for (var i = 0; i < nearby.Count; i++)
                {
                    nearby[i].ApplyMark(duration);
                    nearby[i].ApplySlow(duration, slowMultiplier);
                }
                _runtime.SpawnImpact(target.transform.position, new Color(0.55f, 0.78f, 1f, 0.85f));
                return dealtDamage * (finalId == "sniper_beacon_final_2" ? 1.15f : 1f);
            }
            return dealtDamage;
        }

        private static Vector2 Rotate(Vector2 direction, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            var sin = Mathf.Sin(radians);
            var cos = Mathf.Cos(radians);
            return new Vector2(direction.x * cos - direction.y * sin, direction.x * sin + direction.y * cos).normalized;
        }

        private static Color ColorFor(SphereId type)
        {
            switch (type)
            {
                case SphereId.Standard: return new Color(0.33f, 0.87f, 1f);
                case SphereId.Sniper: return new Color(0.91f, 0.42f, 1f);
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

        private static string GetColor(SphereId type)
        {
            switch (type)
            {
                case SphereId.Standard: return "#55dfff";
                case SphereId.Sniper: return "#e86cff";
                case SphereId.Chain: return "#ffe25b";
                case SphereId.Shotgun: return "#ff8f3d";
                case SphereId.Aura: return "#57e6b4";
                case SphereId.Orbital: return "#8ef0ff";
                case SphereId.Prism: return "#ff8de1";
                case SphereId.Gravity: return "#a58cff";
                case SphereId.Pulse: return "#ffd35a";
                case SphereId.Void: return "#c28cff";
                default: return "#55dfff";
            }
        }
    }
}
