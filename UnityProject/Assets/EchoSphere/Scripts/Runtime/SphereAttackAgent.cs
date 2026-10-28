using EchoSphere;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class SphereAttackAgent : MonoBehaviour, ISourceSimulationAgent
    {
        internal float SourceProjectionOrbitalContactCheckTimer => _orbitalContactCheckTimer;
        internal float SourceProjectionOrbitAngle => _orbitAngle;
        internal float SourceProjectionNetworkDisabledTimer => _networkDisabledTimer;
        private EchoSphereRuntime _runtime;
        private Transform _player;
        private Vector2 _followOffset;
        private float _attackTimer;
        private float _contactTimer;
        private bool _followEnabled;
        private bool _followOffsetValid;
        private SphereCombatProfile _profile;
        private SphereId _type;
        private float _orbitAngle;
        private float _orbitalLastSweepAngle;
        private float _orbitalContactCheckTimer;
        private float _orbitalAttackFlashTimer;
        private readonly Dictionary<EnemyAgent2D, HashSet<int>> _orbitalContactLatch =
            new Dictionary<EnemyAgent2D, HashSet<int>>();
        private float _initialDamage;
        private float _initialDelay;
        private Vector3 _initialScale = Vector3.one;
        private int _progressionLevel = 1;
        private SphereLevelStats _levelStats;
        private SourceSphereModifiers _sourceModifiers;
        private bool _sourceLineBurstAttack;
        private int _branchHitCount;
        private int _chainStormCycle;
        private int _gravityTideHits;
        private readonly List<Transform> _orbitalElements = new List<Transform>();
        private int _orbitalHitCount;
        private float _networkDisabledTimer;
        private float _resonancePulseTimer;
        private int _formationHitCount;
        public float AttackTimer => _attackTimer;
        public void ResetAttackTimer() => _attackTimer = 0f;
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
        // Read-only presentation access: gameplay owns and positions these elements.
        public int OrbitalElementCount => _orbitalElements.Count;
        public float OrbitalAttackFlashTimer => _orbitalAttackFlashTimer;
        public float OrbitalRotation => _orbitAngle;
        public Transform GetOrbitalElementAt(int index) =>
            index >= 0 && index < _orbitalElements.Count ? _orbitalElements[index] : null;

        public float BaseDamage { get; private set; } = 12f;
        public float AttackDelay { get; private set; } = 1.2f;
        public float AttackRange { get; private set; } = (130f * SpherePlacementRules.SourcePixelsToWorld);
        public int PierceCount => _levelStats.Pierce;
        public float GetSourceDpsEstimate(float damage, float delay)
        {
            var multishot = _sourceModifiers.multishot;
            if (_type == SphereId.Orbital)
            {
                GetOrbitalElementCounts(out var inner, out var outer);
                var rotation = (1.8f + Mathf.Min(2.4f, Mathf.Max(1, _progressionLevel) * 0.28f)) * _sourceModifiers.rotationSpeed;
                return damage * 0.95f * (inner + outer) * rotation / (Mathf.PI * 2f);
            }
            if (_type == SphereId.Prism) return damage * Mathf.Max(1, 1 + multishot) / delay;
            if (_type == SphereId.Pulse) return damage * (_runtime.HasArtifact("pulse_crown") ? 2f : 1f) * 4f / Mathf.Max(0.08f, GetSourceCadenceInterval() / _runtime.GetSphereAttackSpeedMultiplier(_type));
            if (_type == SphereId.Gravity) return damage * 4f / Mathf.Max(0.08f, GetSourceCadenceInterval() / _runtime.GetSphereAttackSpeedMultiplier(_type));
            if (_type == SphereId.Chain) return damage * (1f + _sourceModifiers.chainTargets * 0.7f) / delay;
            if (_type == SphereId.Void) return damage * (_progressionLevel >= 2 ? 1.30f : 1.15f) / delay;
            if (_profile.Aura) return damage / delay;
            return damage * (1 + multishot) * _profile.Pellets / delay;
        }

        public void Initialize(EchoSphereRuntime runtime, Transform player, SphereId type, float damage)
        {
            _runtime = runtime;
            _runtime.RegisterSimulationAgent(this);
            _player = player;
            _type = type;
            _profile = SphereCombatRules.GetProfile(type);
            _initialDamage = damage * _profile.DamageMultiplier;
            _initialDelay = 1.2f * _profile.DelayMultiplier;
            _initialScale = transform.localScale;
            ApplyProgressionLevel(1);
            AttackRange = (130f * SpherePlacementRules.SourcePixelsToWorld) * _profile.RangeMultiplier * _runtime.GetSphereRadiusMultiplier() * _runtime.GetSphereRangeMultiplier() * _runtime.GetArtifactSphereRadiusMultiplier(type) *
                _runtime.GetShopSphereRadiusMultiplier() * _runtime.GetCharacterSphereRadiusMultiplier();
            _attackTimer = 0f;
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
            transform.localScale = _initialScale * _runtime.GetSphereRadiusMultiplier();
            _levelStats = _progressionLevel == 0
                ? SphereLevelRules.GetUnlevelledStats()
                : SphereLevelRules.GetStats(_type, _progressionLevel);
            RefreshSourceModifiers();
        }

        private void RefreshSourceModifiers()
        {
            _sourceModifiers = _runtime.GetSourceSphereModifiers(_type, _progressionLevel);
            BaseDamage = _initialDamage * _sourceModifiers.damage;
            AttackDelay = Mathf.Max(0.05f, _initialDelay * _sourceModifiers.delay);
            AttackRange = (130f * SpherePlacementRules.SourcePixelsToWorld) * _profile.RangeMultiplier * _sourceModifiers.radius * _runtime.GetSphereRadiusMultiplier() * _runtime.GetArtifactSphereRadiusMultiplier(_type) *
                _runtime.GetShopSphereRadiusMultiplier() * _runtime.GetCharacterSphereRadiusMultiplier();
        }

        public void SetFollowMode(bool enabled, Vector2 playerPosition)
        {
            _followEnabled = enabled;
            // Source assigns offsets only to alive Spheres. An inactive Sphere
            // joining later captures its offset on its first live sync.
            _followOffsetValid = enabled && gameObject.activeInHierarchy;
            if (_followOffsetValid) _followOffset = (Vector2)transform.position - playerPosition;
        }

        public void SyncFollowPosition()
        {
            if (!_followEnabled || _player == null || !gameObject.activeInHierarchy) return;
            if (!_followOffsetValid)
            {
                _followOffset = (Vector2)transform.position - (Vector2)_player.position;
                _followOffsetValid = true;
                return;
            }
            transform.position = (Vector2)_player.position + _followOffset;
        }

        private void LateUpdate()
        {
            if (_runtime != null && !_runtime.IsGameplayPaused) SyncFollowPosition();
        }

        public void SimulationTick(float dt)
        {
            if (_runtime == null || _runtime.IsGameplayPaused) return;
            RefreshSourceModifiers();
            _contactTimer = Mathf.Max(0f, _contactTimer - dt);
            _networkDisabledTimer = Mathf.Max(0f, _networkDisabledTimer - dt);
            _resonancePulseTimer = Mathf.Max(0f, _resonancePulseTimer - dt);
            _orbitalAttackFlashTimer = Mathf.Max(0f, _orbitalAttackFlashTimer - dt);

            if (_type == SphereId.Orbital)
            {
                UpdateOrbitalElements(dt);
                UpdateOrbitalContacts(dt);
                return;
            }

            if (_type == SphereId.Gravity) VisualRotation += dt * (_runtime.GetSphereBranch(_type) == "gravity_tide" ? 1.9f : .9f);

            // Source Aura uses its authored auraPulse interval directly.
            // Gravity/Pulse use special cadence intervals scaled by the global attack-speed path.
            if (_type == SphereId.Aura) _attackTimer -= dt;
            else _attackTimer -= dt * _runtime.GetSphereAttackSpeedMultiplier(_type);
            if (_attackTimer > 0f) return;

            var origin = (Vector2)transform.position;
            var targetRange = _type == SphereId.Prism ? GetPrismSourceRangeWorld() : AttackRange;
            var target = _runtime.FindSourceSphereTarget(_type, origin, targetRange);
            if (target != null)
            {
                var targetDelta = (Vector2)target.transform.position - origin;
                if (targetDelta.sqrMagnitude > 0.0001f) VisualRotation = Mathf.Atan2(targetDelta.y, targetDelta.x);
            }
            var damage = _runtime.GetSourceSphereAttackDamage(this);
            _sourceLineBurstAttack = target != null && (_type == SphereId.Standard || _type == SphereId.Sniper || _type == SphereId.Shotgun || _type == SphereId.Chain || _type == SphereId.Void) && _runtime.ConsumeResonanceLineBurst();

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
                    triggeredEngineerRelay = DamageSourceArea(origin, _profile.AuraRadius * SpherePlacementRules.SourcePixelsToWorld, damage, dt);
                    break;
                case SphereId.Prism:
                    FireAtMultiple(origin, damage);
                    // The source Prism cadence opens a Relay window even when
                    // no enemy is available for the direct beam.
                    triggeredEngineerRelay = true;
                    break;
                case SphereId.Gravity:
                    DamageSourceArea(origin, _profile.AuraRadius * SpherePlacementRules.SourcePixelsToWorld, damage, dt);
                    // The source Gravity pulse triggers Relay on its own cadence,
                    // independently of whether an enemy is caught in the field.
                    triggeredEngineerRelay = true;
                    break;
                case SphereId.Pulse:
                    DamageSourceArea(origin, _profile.AuraRadius * SpherePlacementRules.SourcePixelsToWorld, damage, dt);
                    // The source Pulse cadence triggers Relay even in an empty field.
                    triggeredEngineerRelay = true;
                    break;
                case SphereId.Void:
                    FireVoid(origin, damage);
                    triggeredEngineerRelay = target != null;
                    break;
            }

            if (triggeredEngineerRelay) _runtime.TriggerEngineerRelay(this);
            if (target != null && (_type == SphereId.Standard || _type == SphereId.Sniper || _type == SphereId.Shotgun || _type == SphereId.Chain || _type == SphereId.Void)) SourceAudio2D.Play("shoot");
            _attackTimer = GetSourceCadenceInterval();
        }

        private float GetSourceCadenceInterval()
        {
            var branch = _runtime.GetSphereBranch(_type);
            var finalId = _runtime.GetSphereFinal(_type);
            switch (_type)
            {
                case SphereId.Aura:
                    var auraInterval = _sourceModifiers.auraPulse;
                    return Mathf.Max(0.05f, auraInterval);
                case SphereId.Gravity:
                    var gravityInterval = 0.80f * _sourceModifiers.auraPulse;
                    if (_runtime.HasArtifact("gravity_hook")) gravityInterval *= 0.92f;
                    gravityInterval *= 1f - 0.10f * _runtime.GetFormationBonus(
                        SphereNetworkFormation.Cluster, _runtime.GetSphereIndex(this));
                    return Mathf.Max(.08f * _runtime.GetSphereAttackSpeedMultiplier(_type), gravityInterval);
                case SphereId.Pulse:
                    var pulseInterval = 1.15f * _sourceModifiers.auraPulse;
                    if (_runtime.HasArtifact("pulse_driver")) pulseInterval *= 0.90f;
                    pulseInterval *= 1f - 0.10f * _runtime.GetFormationBonus(
                        SphereNetworkFormation.Cluster, _runtime.GetSphereIndex(this));
                    return Mathf.Max(.08f * _runtime.GetSphereAttackSpeedMultiplier(_type), pulseInterval);
                case SphereId.Prism:
                    return Mathf.Max(.14f * _runtime.GetSphereAttackSpeedMultiplier(_type), AttackDelay);
                default:
                    return Mathf.Max(0.05f, AttackDelay);
            }
        }

        public void OnSphereHitApplied(EnemyAgent2D target) => OnSphereHitApplied(target, _type);

        public void OnSphereHitApplied(EnemyAgent2D target, SphereId statusSource, SphereElement? effectOverride = null)
        {
            if (target == null || !target.IsAlive || _runtime == null) return;
            var branch = _runtime.GetSphereBranch(_type);
            var finalId = _runtime.GetSphereFinal(_type);
            var element = SphereElementalRules.ResolveElement(branch, finalId);
            if (element == SphereElement.None) element = _sourceModifiers.fire > 0f ? SphereElement.Fire : _sourceModifiers.freeze > 0f ? SphereElement.Freeze : _sourceModifiers.poison > 0f ? SphereElement.Poison : SphereElement.None;
            if (effectOverride.HasValue) element = effectOverride.Value;
            if (element == SphereElement.None) return;
            var sourceBranch = _runtime.GetSphereBranch(statusSource);
            var sourceFinalId = _runtime.GetSphereFinal(statusSource);
            var masteryId = SphereElementalRules.GetMasteryId(sourceBranch, sourceFinalId);
            // applyDirectSphereStatus gates direct mastery by the branch element,
            // even when Prism Spectrum's final changes the emitted status.
            if (!SphereElementalRules.IsChainBranch(sourceBranch) && !SphereElementalRules.IsFieldBranch(sourceBranch) &&
                SphereElementalRules.ResolveElement(sourceBranch) != element) masteryId = null;
            var damageMultiplier = _runtime.GetSphereStatusDamageMultiplier() * _runtime.GetSphereElementalMasteryDamageMultiplier();
            var statusModifiers = _runtime.GetSourceSphereModifiers(statusSource);
            var statusLevel = (int)(element == SphereElement.Fire ? statusModifiers.fire : element == SphereElement.Freeze ? statusModifiers.freeze : statusModifiers.poison);
            if (SphereElementalRules.ResolveElement(sourceBranch) == element) statusLevel = Mathf.Max(1, statusLevel);

            if (_type == SphereId.Chain && SphereElementalRules.IsChainBranch(branch))
            {
                if (!target.TryAdvanceElementalConduction(_type,
                    SphereElementalRules.GetConductionThreshold(masteryId), SphereElementalRules.GetConductionCooldown(masteryId))) return;
                var baseDamage = _initialDamage;
                target.ReceiveSphereDamage(_type, baseDamage * SphereElementalRules.GetConductionDamageMultiplier(masteryId), false, false, this);
                if (target.IsAlive)
                {
                    var durationMultiplier = _runtime.GetSphereStatusDurationMultiplier(element);
                    var status = SphereElementalRules.CreateStatus(element, masteryId, SphereElementalMode.Conduction,
                        durationMultiplier, damageMultiplier, statusLevel);
                    target.ApplyElementalStatus(status, statusSource);
                }
                if (target.IsAlive) _runtime.SpawnSourceParticle(target.transform.position, new Vector2(0f, 18f), .38f, 5f, "#" + ColorUtility.ToHtmlStringRGB(ElementColor(element)), .38f);
                return;
            }

            if ((_type == SphereId.Aura || _type == SphereId.Gravity || _type == SphereId.Pulse) &&
                SphereElementalRules.IsFieldBranch(branch))
            {
                if (!target.TryBeginFieldElementalReaction(SphereElementalRules.GetFieldCooldown(masteryId))) return;
                var durationMultiplier = _runtime.GetSphereStatusDurationMultiplier(element);
                var status = SphereElementalRules.CreateStatus(element, masteryId, SphereElementalMode.Field,
                    durationMultiplier, damageMultiplier, statusLevel);
                target.ApplyElementalStatus(status, statusSource);
                _runtime.SpawnSourceParticle(target.transform.position, new Vector2(0f, 14f), .34f, 4f, "#" + ColorUtility.ToHtmlStringRGB(ElementColor(element)), .34f);
                return;
            }

            var directDurationMultiplier = _runtime.GetSphereStatusDurationMultiplier(element);
            var directStatus = SphereElementalRules.CreateStatus(element, masteryId, SphereElementalMode.Direct,
                directDurationMultiplier, damageMultiplier, statusLevel);
            target.ApplyElementalStatus(directStatus, statusSource);
        }

        private static Color ElementColor(SphereElement element)
        {
            switch (element)
            {
                case SphereElement.Fire: return new Color32(255, 116, 61, 255);
                case SphereElement.Freeze: return new Color32(217, 246, 255, 255);
                case SphereElement.Poison: return new Color32(114, 240, 142, 255);
                default: return Color.white;
            }
        }

        private void GetOrbitalElementCounts(out int innerCount, out int outerCount)
        {
            var finalId = _runtime.GetSphereFinal(SphereId.Orbital);
            var branch = _runtime.GetSphereBranch(SphereId.Orbital);
            innerCount = SphereEvolutionCombatRules.GetOrbitalInnerCount(_progressionLevel, finalId);
            outerCount = SphereEvolutionCombatRules.GetOrbitalOuterCount(_progressionLevel, finalId);
            // Source engineSpheres.ts adds extras in authored order: orbital_crown,
            // then Orbital Dance final III. Balance each extra across both rings.
            var extraElements = (_runtime.HasArtifact("orbital_crown") ? 1 : 0)
                + (branch == "orbital_dance" && finalId == "orbital_dance_final_3" ? 1 : 0);
            for (var extra = 0; extra < extraElements; extra++)
            {
                if (outerCount < innerCount) outerCount++;
                else if (innerCount < outerCount) innerCount++;
                else if (extra % 2 == 0) outerCount++;
                else innerCount++;
            }
        }

        private void UpdateOrbitalElements(float dt)
        {
            var finalId = _runtime.GetSphereFinal(SphereId.Orbital);
            GetOrbitalElementCounts(out var innerCount, out var outerCount);
            EnsureOrbitalElements(innerCount + outerCount);
            var sphereIndex = _runtime.GetSphereIndex(this);
            var linkedSpheres = _runtime.GetLinkedSpheres(sphereIndex);
            var linkedNeighbourCount = 0;
            for (var i = 0; i < linkedSpheres.Count; i++)
                if (linkedSpheres[i] != null && linkedSpheres[i].gameObject.activeInHierarchy) linkedNeighbourCount++;
            var clusterBonus = _runtime.GetFormationBonus(SphereNetworkFormation.Cluster, sphereIndex);
            var ringBonus = _runtime.GetFormationBonus(SphereNetworkFormation.Ring, sphereIndex);
            var networkRadiusMultiplier = 1f
                + (clusterBonus > 0f ? 0.08f * clusterBonus : 0f)
                + (ringBonus > 0f ? 0.12f * ringBonus : 0f)
                + Mathf.Min(0.20f, linkedNeighbourCount * 0.03f);
            var orbitalBranch = _runtime.GetSphereBranch(SphereId.Orbital);
            _orbitAngle += dt * (1.8f + Mathf.Min(2.4f, Mathf.Max(1, _progressionLevel) * 0.28f)) * _sourceModifiers.rotationSpeed;
            for (var i = 0; i < _orbitalElements.Count; i++)
            {
                var inner = i < innerCount;
                var index = inner ? i : i - innerCount;
                var count = inner ? innerCount : outerCount;
                var angle = _orbitAngle * (inner ? 1f : -1f) + index * Mathf.PI * 2f / Mathf.Max(1, count);
                var liveSphereRadiusPixels = _runtime.GetOrbitalLiveRadiusPixels(_progressionLevel);
                var bodyRadiusPixels = Mathf.Clamp(130f * 0.19f + _progressionLevel * 0.8f, 15f, 25f);
                var liveRadiusMultiplier = liveSphereRadiusPixels / 130f;
                var sourceOrbitRadiusPixels = bodyRadiusPixels * (inner ? 1.68f : 2.02f) * liveRadiusMultiplier;
                var radius = sourceOrbitRadiusPixels * SpherePlacementRules.SourcePixelsToWorld * networkRadiusMultiplier;
                _orbitalElements[i].position = transform.position + new Vector3(Mathf.Cos(angle) * radius, -Mathf.Sin(angle) * radius, -0.08f);
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
                // NativeWorldSphereFamilies owns this visual; keep
                // the legacy Disc prototype disabled as elements are created at runtime.
                renderer.enabled = false;
                renderer.sprite = RuntimeSpriteFactory.Disc;
                renderer.color = new Color(0.45f, 0.94f, 1f, 1f);
                renderer.sortingOrder = 13;
                var glow = new GameObject("Orbital Element Glow");
                glow.transform.SetParent(go.transform, false);
                glow.transform.localScale = Vector3.one * 2.2f;
                var glowRenderer = glow.AddComponent<SpriteRenderer>();
                glowRenderer.enabled = false;
                glowRenderer.sprite = RuntimeSpriteFactory.Disc;
                glowRenderer.color = new Color(0.35f, 0.82f, 1f, 0.30f);
                glowRenderer.sortingOrder = 12;
                _orbitalElements.Add(go.transform);
            }
        }

        private void UpdateOrbitalContacts(float dt)
        {
            var sphereIndexForCadence = _runtime.GetSphereIndex(this);
            var clusterBonusForCadence = _runtime.GetFormationBonus(SphereNetworkFormation.Cluster, sphereIndexForCadence);
            _orbitalContactCheckTimer -= dt;
            if (_orbitalContactCheckTimer > 0f) return;
            var cadenceMultiplier = clusterBonusForCadence > 0f ? 1f - 0.10f * clusterBonusForCadence : 1f;
            _orbitalContactCheckTimer = Mathf.Max(0.08f,
                0.42f * _sourceModifiers.auraPulse * cadenceMultiplier / Mathf.Max(0.01f, _runtime.GetSphereAttackSpeedMultiplier(SphereId.Orbital)));
            // Source orbitalVisual.ts detects the auraTimer reset and runs a 0.28 s attack flash.
            _orbitalAttackFlashTimer = Mathf.Max(0f, 0.28f - dt);

            var hitSomething = false;
            var staleLatchEnemies = new List<EnemyAgent2D>();
            foreach (var entry in _orbitalContactLatch)
                if (entry.Key == null || !entry.Key.IsAlive) staleLatchEnemies.Add(entry.Key);
            for (var i = 0; i < staleLatchEnemies.Count; i++)
                _orbitalContactLatch.Remove(staleLatchEnemies[i]);

            var branch = _runtime.GetSphereBranch(SphereId.Orbital);
            var finalId = _runtime.GetSphereFinal(SphereId.Orbital);
            GetOrbitalElementCounts(out var innerCount, out var outerCount);
            EnsureOrbitalElements(innerCount + outerCount);

            var finalIndex = finalId != null && finalId.EndsWith("_final_1") ? 0
                : finalId != null && finalId.EndsWith("_final_2") ? 1
                : finalId != null && finalId.EndsWith("_final_3") ? 2 : -1;
            var preFinalPower = _progressionLevel >= 6 ? 1.30f : _progressionLevel >= 5 ? 1.15f : 1f;
            var contactBandPixels = finalIndex == 1 ? 26f
                : finalIndex == 0 ? 24f
                : finalIndex < 0 ? 23f * preFinalPower : 19f;
            var angularTolerance = finalIndex == 1 ? 0.30f : 0.22f;
            var bodyRadiusPixels = Mathf.Clamp(130f * 0.19f + _progressionLevel * 0.8f, 15f, 25f);
            var contactScale = SpherePlacementRules.SourcePixelsToWorld;
            var innerSweepStart = _orbitalLastSweepAngle;
            var outerSweepStart = _orbitalLastSweepAngle;
            var innerSweepEnd = _orbitAngle;
            var outerSweepEnd = _orbitAngle;

            for (var enemyIndex = 0; enemyIndex < _runtime.ActiveEnemyCount; enemyIndex++)
            {
                var enemy = _runtime.GetActiveEnemyAt(enemyIndex);
                if (enemy == null || !enemy.IsAlive) continue;

                var delta = (Vector2)enemy.transform.position - (Vector2)transform.position;
                var distance = delta.magnitude;
                var enemyAngle = Mathf.Atan2(-delta.y, delta.x);
                var contactBand = Mathf.Max(contactBandPixels,
                    enemy.SourceRadius + Mathf.Max(6f, bodyRadiusPixels * 0.24f)) * contactScale;
                var touchedElements = new HashSet<int>();
                if (!_orbitalContactLatch.TryGetValue(enemy, out var latch))
                {
                    latch = new HashSet<int>();
                    _orbitalContactLatch[enemy] = latch;
                }
                var newContactHit = false;

                for (var elementIndex = 0; elementIndex < innerCount + outerCount; elementIndex++)
                {
                    var inner = elementIndex < innerCount;
                    var ringIndex = inner ? elementIndex : elementIndex - innerCount;
                    var ringCount = inner ? innerCount : outerCount;
                    var element = _orbitalElements[elementIndex];
                    if (element == null) continue;
                    var orbitRadius = ((Vector2)element.position - (Vector2)transform.position).magnitude;
                    if (Mathf.Abs(distance - orbitRadius) > contactBand) continue;

                    var direction = inner ? 1 : -1;
                    var startAngle = (inner ? innerSweepStart : outerSweepStart) * direction
                        + ringIndex * Mathf.PI * 2f / Mathf.Max(1, ringCount);
                    var endAngle = (inner ? innerSweepEnd : outerSweepEnd) * direction
                        + ringIndex * Mathf.PI * 2f / Mathf.Max(1, ringCount);
                    if (!IsAngleOnOrbitalSweep(enemyAngle, startAngle, endAngle, direction, angularTolerance)) continue;

                    var contactKey = inner ? ringIndex : 1024 + ringIndex;
                    touchedElements.Add(contactKey);
                    // One hit per individual orbit element per continuous contact episode.
                    if (!latch.Add(contactKey)) continue;
                    newContactHit = true;

                    var damageMultiplier = SphereEvolutionCombatRules.GetOrbitalDamageMultiplier(_progressionLevel, branch, finalId);
                    var damage = _runtime.GetSourceSphereAttackDamage(this) * damageMultiplier * (1f + .08f * _runtime.GetFormationBonus(SphereNetworkFormation.Cluster, _runtime.GetSphereIndex(this)));
                    var dealtDamage = enemy.ReceiveSphereDamage(this, damage);

                    OnSphereHitApplied(enemy);
                    _orbitalHitCount++;

                    if (branch == "orbital_halo")
                    {
                        if (finalId == "orbital_halo_final_3" && SphereEvolutionCombatRules.ShouldOrbitalGrantShieldCharge(_orbitalHitCount)) _runtime.GrantPlayerShieldCharge(1);
                    }
                    _runtime.SpawnImpact(enemy.transform.position, new Color(0.56f, 0.94f, 1f));
                }

                // Leaving the swept interval re-arms this individual orbit element.
                var stale = new List<int>();
                foreach (var latchedElement in latch)
                    if (!touchedElements.Contains(latchedElement)) stale.Add(latchedElement);
                for (var i = 0; i < stale.Count; i++) latch.Remove(stale[i]);
                if (newContactHit) hitSomething = true;
            }

            if (hitSomething) _runtime.TriggerEngineerRelay(this);

            // The source stores the end angle only after collision processing.
            _orbitalLastSweepAngle = _orbitAngle;

            // Source engineSpheres.ts: orbital_blade + prism_filter relay a
            // visual bolt to the first linked Sphere on the same special-sphere cadence.
            if (_runtime.HasArtifact("orbital_blade") && _runtime.HasArtifact("prism_filter"))
            {
                var sphereIndex = _runtime.GetSphereIndex(this);
                if (_runtime.GetFormationBonus(SphereNetworkFormation.Ring, sphereIndex) > 0f)
                {
                    var linkedSpheres = _runtime.GetLinkedSpheres(sphereIndex);
                    for (var i = 0; i < linkedSpheres.Count; i++)
                    {
                        var linked = linkedSpheres[i];
                        if (linked == null || !linked.gameObject.activeInHierarchy) continue;
                        var tint = new Color(105f / 255f, 235f / 255f, 1f, 0.9f);
                        _runtime.SpawnCombatArc(transform.position, linked.transform.position,
                            SphereId.Orbital, branch, finalId, tint, 0.18f);
                        break;
                    }
                }
            }
        }

        private static bool IsAngleOnOrbitalSweep(float targetAngle, float startAngle, float endAngle,
            int direction, float tolerance)
        {
            const float tau = Mathf.PI * 2f;
            var safeTolerance = Mathf.Max(0f, tolerance);
            var travelled = direction == 1
                ? NormalizeAnglePositive(endAngle - startAngle)
                : NormalizeAnglePositive(startAngle - endAngle);
            var targetTravel = direction == 1
                ? NormalizeAnglePositive(targetAngle - startAngle)
                : NormalizeAnglePositive(startAngle - targetAngle);
            if (travelled >= tau - safeTolerance) return true;
            return targetTravel <= travelled + safeTolerance || targetTravel >= tau - safeTolerance;
        }

        private static float NormalizeAnglePositive(float angle)
        {
            const float tau = Mathf.PI * 2f;
            return (angle % tau + tau) % tau;
        }

        private void FireAt(EnemyAgent2D target, Vector2 origin, float damage, Color tint)
        {
            if (target == null) return;
            var direction = ((Vector2)target.transform.position - origin).normalized;
            var count = 1 + (int)_sourceModifiers.multishot;
            var pierce = (int)_sourceModifiers.pierce;
            var spread = _profile.Spread * _sourceModifiers.spreadMult / Mathf.Max(1, count - 1);
            if (spread == 0f) spread = .15f;
            for (var i = 0; i < count; i++)
            {
                var angle = count == 1 ? 0f : (i - (count - 1) * .5f) * spread;
                SpawnAuthoredMainProjectile(origin, Rotate(direction, -angle * Mathf.Rad2Deg), damage,
                    tint, 350f * SpherePlacementRules.SourcePixelsToWorld * _profile.ProjectileSpeedMultiplier, pierce, _levelStats.CritChanceBonus);
            }
        }

        private void SpawnAuthoredMainProjectile(Vector2 origin, Vector2 direction, float damage,
            Color tint, float speed, int pierce, float critChanceBonus)
        {
            var element = SphereElementalRules.ResolveElement(_runtime.GetSphereBranch(_type));
            if (element == SphereElement.None) element = _sourceModifiers.fire > 0f ? SphereElement.Fire : _sourceModifiers.freeze > 0f ? SphereElement.Freeze : _sourceModifiers.poison > 0f ? SphereElement.Poison : SphereElement.None;
            if (element != SphereElement.None) tint = ElementColor(element);
            var ricochets = _runtime.GetSphereProjectileRicochetCount(_type);
            pierce = _runtime.GetSourceMainProjectilePierce(this, pierce);
            _runtime.SpawnProjectile(origin, direction, damage * (_sourceLineBurstAttack ? 1.60f : 1f), tint, speed * _runtime.GetProjectileSpeedMultiplier(), pierce, critChanceBonus,
                this, ricochets, 2.0f, true, true, true);

            var echoLevel = _runtime.GetSphereAuthoredModifierLevel(_type, SphereAuthoredModifier.Echo);
            var echoChance = _sourceModifiers.echoChance;
            if (echoChance <= 0f || !_runtime.RollCombatChance(echoChance)) return;
            var echoAngle = -(_runtime.NextRunRandom() - 0.5f) * 10.31324f;
            _runtime.SpawnProjectile(origin, Rotate(direction, echoAngle), damage * 0.28f, tint,
                speed, 0, 0f, this, 0, 1.1f, false, false, false, 4f);
        }

        private void FireSpread(EnemyAgent2D target, Vector2 origin, float damage)
        {
            if (target == null) return;
            var direction = ((Vector2)target.transform.position - origin).normalized;
            var count = Mathf.Max(1, _profile.Pellets + (int)_sourceModifiers.multishot);
            var pierce = (int)_sourceModifiers.pierce;
            for (var i = 0; i < count; i++)
            {
                var t = count == 1 ? 0f : (float)i / (count - 1) - 0.5f;
                var angle = t * _profile.Spread * _sourceModifiers.spreadMult * Mathf.Rad2Deg;
                var shot = Rotate(direction, -angle);
                SpawnAuthoredMainProjectile(origin, shot, damage, ColorFor(_type), 350f * SpherePlacementRules.SourcePixelsToWorld * _profile.ProjectileSpeedMultiplier, pierce, 0f);
            }
        }

        private void ChainTargets(Vector2 origin, float damage)
        {
            var primary = _runtime.FindSourceSphereTarget(_type, origin, AttackRange);
            if (primary == null) return;
            var targets = new List<EnemyAgent2D> { primary };
            var used = new HashSet<EnemyAgent2D> { primary };
            var current = primary;
            for (var jump = 0; jump < (int)_sourceModifiers.chainTargets; jump++)
            {
                EnemyAgent2D next = null;
                foreach (var candidate in _runtime.FindNearestEnemies(current.transform.position, 150f * SpherePlacementRules.SourcePixelsToWorld, int.MaxValue))
                    if (!used.Contains(candidate)) { next = candidate; break; }
                if (next == null) break;
                targets.Add(next); used.Add(next); current = next;
            }
            var branch = _runtime.GetSphereBranch(_type); var final = _runtime.GetSphereFinal(_type);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                _runtime.SpawnCombatArc(i == 0 ? origin : (Vector2)primary.transform.position, target.transform.position, SphereId.Chain,
                    _runtime.GetSphereBranch(_type), _runtime.GetSphereFinal(_type), ColorFor(_type));
                var multiplier = i == 0 ? 1f : .7f * (branch == "chain_storm" ? 1f + (i - 1) * .15f : 1f) * (final == branch + "_final_1" ? 1.15f : 1f);
                var hitDamage = target.ReceiveSphereDamage(_type, damage * multiplier, false, true, this);
                if (i > 0 && _runtime.HasSourceChainToxicNetwork()) { target.ExtendFireStatus(1.5f); target.ExtendPoisonStatus(1.5f); }
                OnSphereHitApplied(target);

                _runtime.SpawnImpact(target.transform.position, ColorFor(_type));
            }
        }

        // Source mapping: new-desing/src/engineSpheres.ts:updatePrismSphere.
        // Prism resolves each beam immediately; it is not a travelling projectile.
        private float GetPrismSourceRangeWorld() => AttackRange;

        private void FireAtMultiple(Vector2 origin, float damage)
        {
            var branch = _runtime.GetSphereBranch(_type);
            var finalId = _runtime.GetSphereFinal(_type);
            var beamRange = GetPrismSourceRangeWorld();
            var candidates = _runtime.FindEnemiesInRadius(origin, beamRange);
            if (candidates.Count == 0) return;

            var targetCount = Mathf.Max(1, 1 + (int)_sourceModifiers.multishot) + (branch == "prism_split" && finalId == "prism_split_final_3" ? 1 : 0);
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
                    if (candidate == null || !candidate.IsAlive || usedTargets.Contains(candidate.ExistingObjectId())) continue;
                    if (candidate.CurrentHp <= highestHp) continue;
                    highestHp = candidate.CurrentHp;
                    beamTarget = candidate;
                }
                if (beamTarget == null) break;

                usedTargets.Add(beamTarget.ExistingObjectId());
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
            var dealtDamage = target.ReceiveSphereDamage(SphereId.Prism, damage, false, true, this);
            OnSphereHitApplied(target);

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
                var visited = new HashSet<int> { primaryTarget.ExistingObjectId() };
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
                        if (candidate == null || !candidate.IsAlive || visited.Contains(candidate.ExistingObjectId())) continue;
                        var distanceSquared = ((Vector2)candidate.transform.position - center).sqrMagnitude;
                        if (distanceSquared >= nearestDistanceSquared) continue;
                        nearestDistanceSquared = distanceSquared;
                        next = candidate;
                    }
                    if (next == null) break;

                    visited.Add(next.ExistingObjectId());
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
            var dealtDamage = target.ReceiveSphereDamage(relay.Type, damage, false, false, relay);
            OnSphereHitApplied(target, relay.Type);

            return dealtDamage;
        }

        private void FireVoid(Vector2 origin, float damage)
        {
            var target = _runtime.FindSourceSphereTarget(SphereId.Void, origin, AttackRange);
            if (target == null) return;
            FireAt(target, origin, damage, ColorFor(_type));
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
