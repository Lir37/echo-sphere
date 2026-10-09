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

        public void Initialize(EchoSphereRuntime runtime, Vector2 direction, float damage, Color color, float speed = 8f)
        {
            _runtime = runtime;
            _direction = direction.sqrMagnitude <= 0.0001f ? Vector2.right : direction.normalized;
            _damage = damage;
            _speed = speed;
            _impactColor = color;
        }

        private void Update()
        {
            if (_runtime == null || _runtime.IsGameplayPaused) return;
            var deltaTime = Time.deltaTime;
            transform.position += (Vector3)(_direction * (_speed * deltaTime));
            _life -= deltaTime;
            var enemy = _runtime.TryHitEnemy(transform.position, 0.22f);
            if (enemy != null)
            {
                enemy.ReceiveDamage(_runtime.ResolveProjectileDamage(_damage));
                _runtime.SpawnImpact(transform.position, _impactColor);
                Destroy(gameObject);
                return;
            }
            if (_life <= 0f) Destroy(gameObject);
        }
    }
}
