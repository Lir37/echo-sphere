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

        public SphereId Type => _type;
        public int ProgressionLevel => _progressionLevel;

        public float BaseDamage { get; private set; } = 12f;
        public float AttackDelay { get; private set; } = 1.2f;
        public float AttackRange { get; private set; } = 6.8f;

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
            // Common first-pass scaling; exact per-Sphere effects and IV/VII mutations remain open.
            BaseDamage = _initialDamage * (1f + 0.15f * (_progressionLevel - 1));
            AttackDelay = Mathf.Max(0.12f, _initialDelay * (1f - 0.04f * (_progressionLevel - 1)));
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
                _orbitAngle += Time.deltaTime * 2.2f;
                var orbitRadius = 1.05f;
                transform.position = (Vector2)_player.position + new Vector2(Mathf.Cos(_orbitAngle), Mathf.Sin(_orbitAngle)) * orbitRadius;
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
                var contact = _runtime.TryHitEnemy(transform.position, 0.20f);
                if (contact != null && _contactTimer <= 0f)
                {
                    contact.ReceiveDamage(BaseDamage * _runtime.SphereDamageMultiplier * 0.95f);
                    _runtime.SpawnImpact(contact.transform.position, new Color(0.56f, 0.94f, 1f));
                    _contactTimer = 0.32f;
                }
                return;
            }

            _attackTimer -= dt * _runtime.AttackSpeedMultiplier;
            if (_attackTimer > 0f) return;

            var origin = (Vector2)transform.position;
            var target = _runtime.FindNearestEnemy(origin, AttackRange);
            var damage = BaseDamage * _runtime.SphereDamageMultiplier;

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

        private void FireAt(EnemyAgent2D target, Vector2 origin, float damage, Color tint)
        {
            if (target == null) return;
            var direction = ((Vector2)target.transform.position - origin).normalized;
            _runtime.SpawnProjectile(origin, direction, damage, tint, 8f * _profile.ProjectileSpeedMultiplier);
        }

        private void FireSpread(EnemyAgent2D target, Vector2 origin, float damage)
        {
            if (target == null) return;
            var direction = ((Vector2)target.transform.position - origin).normalized;
            var count = Mathf.Max(1, _profile.Pellets);
            for (var i = 0; i < count; i++)
            {
                var t = count == 1 ? 0f : (float)i / (count - 1) - 0.5f;
                var angle = t * _profile.Spread * Mathf.Rad2Deg;
                var shot = Rotate(direction, angle);
                _runtime.SpawnProjectile(origin, shot, damage, ColorFor(_type));
            }
        }

        private void ChainTargets(Vector2 origin, float damage)
        {
            var targets = _runtime.FindNearestEnemies(origin, AttackRange, 3);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                target.ReceiveDamage(damage * Mathf.Pow(0.72f, i));
                _runtime.SpawnImpact(target.transform.position, ColorFor(_type));
            }
        }

        private void FireAtMultiple(Vector2 origin, float damage)
        {
            var targets = _runtime.FindNearestEnemies(origin, AttackRange, Mathf.Max(1, _profile.Pellets));
            if (targets.Count == 0) return;
            for (var i = 0; i < targets.Count; i++)
            {
                var direction = ((Vector2)targets[i].transform.position - origin).normalized;
                _runtime.SpawnProjectile(origin, direction, damage, ColorFor(_type), 8f * _profile.ProjectileSpeedMultiplier);
            }
        }

        private void DamageArea(Vector2 origin, float radius, float damage, Color tint, bool pull)
        {
            var targets = _runtime.FindEnemiesInRadius(origin, radius);
            for (var i = 0; i < targets.Count; i++)
            {
                if (pull) targets[i].PullToward(origin, 1.8f);
                targets[i].ReceiveDamage(damage);
            }
            if (targets.Count > 0) _runtime.SpawnImpact(origin, tint);
        }

        private void FireVoid(Vector2 origin, float damage)
        {
            var targets = _runtime.FindNearestEnemies(origin, AttackRange, 3);
            if (targets.Count == 0) return;
            targets.Sort((a, b) => a.HpFraction.CompareTo(b.HpFraction));
            var target = targets[0];
            var multiplier = target.HpFraction <= 0.25f ? 2.5f : 1f;
            var direction = ((Vector2)target.transform.position - origin).normalized;
            _runtime.SpawnProjectile(origin, direction, damage * multiplier, ColorFor(_type), 8f * _profile.ProjectileSpeedMultiplier);
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
