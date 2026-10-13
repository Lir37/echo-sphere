using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>Temporary active-ability drone that orbits a Sphere and fires at nearby enemies.</summary>
    public sealed class AbilityDroneAgent2D : MonoBehaviour
    {
        private EchoSphereRuntime _runtime;
        private Transform _anchor;
        private float _remaining;
        private float _attackTimer;
        private float _damage;
        private float _angle;
        private bool _relay;

        public void Initialize(EchoSphereRuntime runtime, Transform anchor, float duration, float damage, bool relay)
        {
            _runtime = runtime;
            _anchor = anchor;
            _remaining = Mathf.Max(0.1f, duration);
            _damage = Mathf.Max(0f, damage);
            _relay = relay;
            _angle = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            if (_runtime == null || _runtime.IsGameplayPaused) return;
            var dt = Time.deltaTime;
            _remaining -= dt;
            if (_remaining <= 0f) { Destroy(gameObject); return; }
            if (_anchor != null)
            {
                _angle += dt * 2.2f;
                var center = (Vector2)_anchor.position;
                transform.position = center + new Vector2(Mathf.Cos(_angle), Mathf.Sin(_angle)) * 0.48f;
            }
            _attackTimer -= dt;
            if (_attackTimer > 0f) return;
            _attackTimer = _relay ? 0.65f : 0.85f;
            var origin = (Vector2)transform.position;
            var target = _runtime.FindNearestEnemy(origin, 2.4f);
            if (target == null) return;
            var targetPosition = (Vector2)target.transform.position;
            if (_relay)
                _runtime.SpawnCombatArc(origin, targetPosition, EchoSphere.Core.SphereId.Chain, "", "", new Color(0.35f, 0.83f, 1f, 0.95f), 0.18f);
            target.ReceiveDamage(_damage);
            target.ApplySlow(0.45f, 0.82f);
            _runtime.SpawnImpact(targetPosition, new Color(0.35f, 0.83f, 1f, 0.9f));
        }
    }
}
