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
        public void AccelerateNextAttack(float amount) => _attackTimer = Mathf.Max(0f, _attackTimer - Mathf.Max(0f, amount));

        public SphereId Type => _type;
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
            AttackRange = 6.8f * _profile.RangeMultiplier;
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
            _progressionLevel = Mathf.Clamp(level, 1, SphereProgressionRules.MaxLevel);
            _levelStats = SphereLevelRules.GetStats(_type, _progressionLevel);
            BaseDamage = _initialDamage * _levelStats.DamageMultiplier;
            AttackDelay = Mathf.Max(0.12f, _initialDelay * _levelStats.DelayMultiplier);
            AttackRange = 6.8f * _profile.RangeMultiplier * _levelStats.RangeMultiplier;
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

            if (_type == SphereId.Orbital)
            {
                UpdateOrbitalContacts(dt);
                return;
            }

            _attackTimer -= dt * _runtime.AttackSpeedMultiplier;
            if (_attackTimer > 0f) return;

            var origin = (Vector2)transform.position;
            var target = _runtime.FindNearestEnemy(origin, AttackRange);
            var damage = BaseDamage * _runtime.SphereDamageMultiplier;
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

            switch (_type)
            {
                case SphereId.Standard:
                    FireAt(target, origin, damage, ColorFor(_type));
                    break;
                case SphereId.Sniper:
                    FireAt(target, origin, damage, ColorFor(_type));
                    break;
                case SphereId.Shotgun:
                    FireSpread(target, origin, damage);
                    break;
                case SphereId.Chain:
                    ChainTargets(origin, damage);
                    break;
                case SphereId.Aura:
                    DamageArea(origin, _profile.AuraRadius / 100f, damage, ColorFor(_type), false);
                    break;
                case SphereId.Prism:
                    FireAtMultiple(origin, damage);
                    break;
                case SphereId.Gravity:
                    DamageArea(origin, _profile.AuraRadius / 100f, damage, ColorFor(_type), true);
                    break;
                case SphereId.Pulse:
                    DamageArea(origin, _profile.AuraRadius / 100f, damage, ColorFor(_type), false);
                    break;
                case SphereId.Void:
                    FireVoid(origin, damage);
                    break;
            }

            _attackTimer = Mathf.Max(0.05f, AttackDelay);
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
                var damage = BaseDamage * _runtime.SphereDamageMultiplier * damageMultiplier;
                contact.ReceiveDamage(damage);
                _runtime.SpawnImpact(contact.transform.position, new Color(0.56f, 0.94f, 1f));
                var afterimage = SphereEvolutionCombatRules.GetOrbitalAfterimageMultiplier(branch, finalId);
                if (afterimage > 0f) _runtime.TriggerOrbitalAfterimage(contact, damage * afterimage, 0.65f);
                _orbitalContactTimers[i] = 0.28f;
            }
        }

        private void FireAt(EnemyAgent2D target, Vector2 origin, float damage, Color tint)
        {
            if (target == null) return;
            var direction = ((Vector2)target.transform.position - origin).normalized;
            _runtime.SpawnProjectile(origin, direction, damage, tint, 8f * _profile.ProjectileSpeedMultiplier, _levelStats.Pierce, _levelStats.CritChanceBonus, this);
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
            var pierce = branch == "shotgun_cataclysm"
                ? SphereEvolutionCombatRules.GetShotgunCataclysmPierce(_levelStats.Pierce, finalId)
                : _levelStats.Pierce;
            for (var i = 0; i < count; i++)
            {
                var t = count == 1 ? 0f : (float)i / (count - 1) - 0.5f;
                var angle = t * _profile.Spread * _levelStats.SpreadMultiplier * Mathf.Rad2Deg;
                var shot = Rotate(direction, angle);
                _runtime.SpawnProjectile(origin, shot, damage, ColorFor(_type), 8f, pierce, 0f, this);
            }
        }

        private void ChainTargets(Vector2 origin, float damage)
        {
            var targets = _runtime.FindNearestEnemies(origin, AttackRange, _levelStats.ChainTargets);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                var hitDamage = _runtime.ResolveProjectileDamage(damage * Mathf.Pow(0.72f, i), _levelStats.CritChanceBonus, out var wasCritical);
                var direction = ((Vector2)target.transform.position - origin).normalized;
                hitDamage = OnProjectileHit(target, direction, hitDamage, wasCritical);
                target.ReceiveDamage(hitDamage);
                _runtime.SpawnImpact(target.transform.position, ColorFor(_type));
            }
        }

        private void FireAtMultiple(Vector2 origin, float damage)
        {
            var branch = _runtime.GetSphereBranch(_type);
            var finalId = _runtime.GetSphereFinal(_type);
            var targetCount = Mathf.Max(1, _levelStats.PrismDirections);
            if (branch == "prism_split" && finalId == "prism_split_final_1") targetCount += 2;
            var targets = _runtime.FindNearestEnemies(origin, AttackRange, targetCount);
            if (targets.Count == 0) return;
            var pierce = SphereEvolutionCombatRules.GetPrismPierce(_levelStats.Pierce, branch, finalId);
            for (var i = 0; i < targets.Count; i++)
            {
                var direction = ((Vector2)targets[i].transform.position - origin).normalized;
                _runtime.SpawnProjectile(origin, direction, damage, ColorFor(_type), 8f * _profile.ProjectileSpeedMultiplier, pierce, 0f, this);
                if (branch == "prism_split" && finalId == "prism_split_final_1" && i == 0)
                {
                    _runtime.SpawnProjectile(origin, Rotate(direction, -14f), damage * 0.72f, ColorFor(_type), 8f * _profile.ProjectileSpeedMultiplier, pierce, 0f, this);
                    _runtime.SpawnProjectile(origin, Rotate(direction, 14f), damage * 0.72f, ColorFor(_type), 8f * _profile.ProjectileSpeedMultiplier, pierce, 0f, this);
                }
            }
        }

        private void DamageArea(Vector2 origin, float radius, float damage, Color tint, bool pull)
        {
            radius *= _levelStats.AuraRadiusMultiplier;
            var targets = _runtime.FindEnemiesInRadius(origin, radius);
            var branch = _runtime.GetSphereBranch(_type);
            var finalId = _runtime.GetSphereFinal(_type);
            var auraDamage = damage;

            if (_type == SphereId.Gravity && branch == "gravity_well")
            {
                var control = SphereEvolutionCombatRules.GetGravityWellSlow(finalId);
                _runtime.ApplyGravityWellControl(origin, SphereEvolutionCombatRules.GetGravityWellRadius(_progressionLevel, finalId), SphereEvolutionCombatRules.GetGravityWellPullDistance(_progressionLevel, finalId), control.duration, control.multiplier);
            }
            else if (_type == SphereId.Gravity && branch == "gravity_tide")
            {
                _gravityTideHits++;
                var mode = finalId == "gravity_tide_final_1" ? -1 : finalId == "gravity_tide_final_2" ? 1 : finalId == "gravity_tide_final_3" ? (_gravityTideHits % 2 == 0 ? 1 : -1) : 1;
                _runtime.ApplyGravityTidePulse(origin, SphereEvolutionCombatRules.GetGravityTideRadius(finalId), SphereEvolutionCombatRules.GetGravityTideDistance(_progressionLevel), mode);
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
                _runtime.ApplyGravityWellControl(origin, radius, 0.32f, 0f, 1f);
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
                targets[i].ReceiveDamage(auraDamage);
            }
            if (targets.Count > 0) _runtime.SpawnImpact(origin, tint);
        }

        private void FireVoid(Vector2 origin, float damage)
        {
            var targets = _runtime.FindNearestEnemies(origin, AttackRange, 3);
            if (targets.Count == 0) return;
            targets.Sort((a, b) => a.HpFraction.CompareTo(b.HpFraction));
            var target = targets[0];
            var multiplier = target.HpFraction <= 0.25f ? 2.5f * _levelStats.WeakenedDamageMultiplier : 1f;
            var direction = ((Vector2)target.transform.position - origin).normalized;
            _runtime.SpawnProjectile(origin, direction, damage * multiplier, ColorFor(_type), 8f * _profile.ProjectileSpeedMultiplier, _levelStats.Pierce);
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
                if (receivesCollapseBonus && target != null) target.ReceiveDamage(dealtDamage * 0.15f);
                return dealtDamage;
            }
            if (_type == SphereId.Standard && branch == "standard_swarm")
            {
                var count = SphereEvolutionCombatRules.GetStandardSwarmShardCount(_progressionLevel, finalId);
                _runtime.SpawnStandardSwarmShards(target, direction, count, dealtDamage * SphereEvolutionCombatRules.GetStandardSwarmShardDamageMultiplier(_progressionLevel));
                return dealtDamage;
            }
            if (_type == SphereId.Prism && branch == "prism_split")
            {
                if (finalId == "prism_split_final_3")
                    _runtime.TriggerPrismRicochet(target, direction, dealtDamage, 1);
                return dealtDamage;
            }
            if (_type == SphereId.Prism && branch == "prism_spectrum")
            {
                if (finalId == "prism_spectrum_final_2")
                    target.ApplySlow(0.75f, 0.55f);
                else if (finalId == "prism_spectrum_final_3")
                    target.ApplyDamageOverTime(dealtDamage * 0.18f, 2.5f);
                else
                    target.ApplyDamageOverTime(dealtDamage * 0.24f, 1.5f);
                return dealtDamage * (finalId == "prism_spectrum_final_1" ? 1.08f : 1f);
            }
            if (_type == SphereId.Prism && branch == "prism_mirror")
            {
                var bounces = SphereEvolutionCombatRules.GetPrismMirrorBounces(finalId);
                if (bounces > 0) _runtime.TriggerPrismRicochet(target, direction, dealtDamage, bounces);
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
                _runtime.TriggerChainStorm(target, dealtDamage, _progressionLevel, finalId);
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
