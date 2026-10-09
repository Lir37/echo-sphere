using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed class SphereAttackAgent : MonoBehaviour
    {
        private EchoSphereRuntime _runtime;
        private Transform _player;
        private Vector2 _followOffset;
        private float _attackTimer;
        private bool _followEnabled;

        public float BaseDamage { get; private set; } = 12f;
        public float AttackDelay { get; private set; } = 0.75f;
        public float AttackRange { get; private set; } = 6.8f;

        public void Initialize(EchoSphereRuntime runtime, Transform player, float damage, float delay)
        {
            _runtime = runtime;
            _player = player;
            BaseDamage = damage;
            AttackDelay = delay;
            _attackTimer = 0.25f;
        }

        public void SetFollowMode(bool enabled, Vector2 playerPosition)
        {
            _followEnabled = enabled;
            if (enabled) _followOffset = (Vector2)transform.position - playerPosition;
        }

        private void LateUpdate()
        {
            if (_runtime == null || _runtime.IsGameplayPaused || !_followEnabled || _player == null) return;
            transform.position = (Vector2)_player.position + _followOffset;
        }

        private void Update()
        {
            if (_runtime == null || _runtime.IsGameplayPaused) return;
            _attackTimer -= Time.deltaTime * _runtime.AttackSpeedMultiplier;
            if (_attackTimer > 0f) return;
            var target = _runtime.FindNearestEnemy(transform.position, AttackRange);
            if (target == null) return;
            var direction = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
            _runtime.SpawnProjectile(transform.position, direction, BaseDamage * _runtime.SphereDamageMultiplier, new Color(0.35f, 0.92f, 1f));
            _attackTimer = AttackDelay;
        }
    }
}
