using System.Collections.Generic;
using UnityEngine;

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
        private readonly HashSet<int> _hitEnemyIds = new HashSet<int>();
        private float _critChanceBonus;
        private SphereAttackAgent _evolutionOwner;

        public void Initialize(EchoSphereRuntime runtime, Vector2 direction, float damage, Color color, float speed = 8f, int pierce = 0, float critChanceBonus = 0f, SphereAttackAgent evolutionOwner = null)
        {
            _runtime = runtime;
            _direction = direction.sqrMagnitude <= 0.0001f ? Vector2.right : direction.normalized;
            _damage = damage;
            _speed = speed;
            _remainingPierces = Mathf.Max(0, pierce);
            _critChanceBonus = Mathf.Max(0f, critChanceBonus);
            _impactColor = color;
            _evolutionOwner = evolutionOwner;
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
                var resolvedDamage = _runtime.ResolveProjectileDamage(_damage, _critChanceBonus);
                if (_evolutionOwner != null) _evolutionOwner.OnProjectileHit(enemy, _direction, resolvedDamage);
                enemy.ReceiveDamage(resolvedDamage);
                _runtime.SpawnImpact(transform.position, _impactColor);
                if (_remainingPierces > 0) _remainingPierces--;
                else { Destroy(gameObject); return; }
            }
            if (_life <= 0f) Destroy(gameObject);
        }
    }
}
