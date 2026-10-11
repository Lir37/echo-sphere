using System.Collections.Generic;
using UnityEngine;
using EchoSphere.Core;

namespace EchoSphere.Runtime
{
    public sealed class ProjectileAgent : MonoBehaviour
    {
        private EchoSphereRuntime _runtime;
        private Vector2 _direction;
        private float _damage;
        private float _speed;
        private float _life = 2.5f;
        private Color _impactColor;
        private int _remainingPierces;
        private int _remainingRicochets;
        private readonly HashSet<int> _hitEnemyIds = new HashSet<int>();
        private float _critChanceBonus;
        private SphereAttackAgent _evolutionOwner;
        private bool _allowAuthoredProcs = true;
        private bool _useEvolutionHitEffects = true;
        private bool _applyElementalStatus = true;

        public void Initialize(EchoSphereRuntime runtime, Vector2 direction, float damage, Color color,
            float speed = 8f, int pierce = 0, float critChanceBonus = 0f, SphereAttackAgent evolutionOwner = null,
            int ricochets = 0, float lifetime = 2.5f, bool allowAuthoredProcs = true,
            bool useEvolutionHitEffects = true, bool applyElementalStatus = true)
        {
            _runtime = runtime;
            _direction = direction.sqrMagnitude <= 0.0001f ? Vector2.right : direction.normalized;
            _damage = Mathf.Max(0f, damage);
            _speed = Mathf.Max(0f, speed);
            _remainingPierces = Mathf.Max(0, pierce);
            _remainingRicochets = Mathf.Max(0, ricochets);
            _critChanceBonus = Mathf.Max(0f, critChanceBonus);
            _impactColor = color;
            _evolutionOwner = evolutionOwner;
            _life = Mathf.Max(0.01f, lifetime);
            _allowAuthoredProcs = allowAuthoredProcs;
            _useEvolutionHitEffects = useEvolutionHitEffects;
            _applyElementalStatus = applyElementalStatus;
        }

        public void IgnoreEnemy(EnemyAgent2D enemy)
        {
            if (enemy != null) _hitEnemyIds.Add(enemy.GetInstanceID());
        }

        public void CopyIgnoredEnemiesFrom(ProjectileAgent source)
        {
            if (source == null) return;
            foreach (var id in source._hitEnemyIds) _hitEnemyIds.Add(id);
        }

        private void Update()
        {
            if (_runtime == null || _runtime.IsGameplayPaused) return;
            var deltaTime = Time.deltaTime;
            transform.position += (Vector3)(_direction * (_speed * deltaTime));
            _life -= deltaTime;
            var enemy = _runtime.TryHitEnemy(transform.position, 0.22f, _hitEnemyIds);
            if (enemy != null)
            {
                _hitEnemyIds.Add(enemy.GetInstanceID());
                var resolvedDamage = _runtime.ResolveProjectileDamage(_damage, _critChanceBonus, out var wasCritical);
                if (_evolutionOwner != null)
                {
                    resolvedDamage = _runtime.ResolveAuthoredSphereHitDamage(_evolutionOwner.Type, enemy, resolvedDamage);
                    if (_useEvolutionHitEffects)
                        resolvedDamage = _evolutionOwner.OnProjectileHit(enemy, _direction, resolvedDamage, wasCritical);
                    resolvedDamage = enemy.ReceiveSphereDamage(
                        _evolutionOwner.Type, resolvedDamage, true, _allowAuthoredProcs, _evolutionOwner);
                    if (_useEvolutionHitEffects && _applyElementalStatus)
                        _evolutionOwner.OnSphereHitApplied(enemy);
                    if (_allowAuthoredProcs && _useEvolutionHitEffects)
                        _runtime.RegisterSphereHit(_evolutionOwner, enemy, resolvedDamage);
                }
                else
                {
                    enemy.ReceiveDamage(resolvedDamage);
                }

                if (_allowAuthoredProcs)
                {
                    _runtime.AddResonanceCharge(1f);
                    TrySplitAfterHit();
                }
                _runtime.SpawnImpact(transform.position, _impactColor);

                if (_remainingPierces > 0)
                {
                    _remainingPierces--;
                }
                else if (_remainingRicochets > 0)
                {
                    _remainingRicochets--;
                    var next = _runtime.FindNearestUnhitEnemy(transform.position, _hitEnemyIds);
                    if (next != null)
                    {
                        _direction = ((Vector2)next.transform.position - (Vector2)transform.position).normalized;
                        if (_direction.sqrMagnitude <= 0.0001f) _direction = Vector2.right;
                        _speed = 350f * SpherePlacementRules.SourcePixelsToWorld;
                    }
                    else
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }
            if (_life <= 0f) Destroy(gameObject);
        }

        private void TrySplitAfterHit()
        {
            if (_evolutionOwner == null) return;
            var level = _runtime.GetSphereAuthoredModifierLevel(_evolutionOwner.Type, SphereAuthoredModifier.Split);
            var chance = SphereAuthoredModifierRules.GetSplitChance(level);
            if (chance <= 0f || !_runtime.RollCombatChance(chance)) return;

            var direction = _direction.sqrMagnitude <= 0.0001f ? Vector2.right : _direction.normalized;
            var damage = _damage * 0.30f;
            var speed = 290f * SpherePlacementRules.SourcePixelsToWorld;
            var left = _runtime.SpawnProjectile(transform.position, Rotate(direction, -0.34f), damage, _impactColor,
                speed, 0, 0f, _evolutionOwner, 0, 0.9f, false, false, false);
            var right = _runtime.SpawnProjectile(transform.position, Rotate(direction, 0.34f), damage, _impactColor,
                speed, 0, 0f, _evolutionOwner, 0, 0.9f, false, false, false);
            if (left != null) left.CopyIgnoredEnemiesFrom(this);
            if (right != null) right.CopyIgnoredEnemiesFrom(this);
        }

        private static Vector2 Rotate(Vector2 direction, float radians)
        {
            var sine = Mathf.Sin(radians);
            var cosine = Mathf.Cos(radians);
            return new Vector2(direction.x * cosine - direction.y * sine,
                direction.x * sine + direction.y * cosine).normalized;
        }
    }
}
