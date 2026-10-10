using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed class EnemyAgent2D : MonoBehaviour
    {
        private EchoSphereRuntime _runtime;
        private Transform _player;
        private SpriteRenderer _shell;
        private float _hp;
        private float _maxHp;
        private float _moveSpeed;
        private bool _dead;
        private float _slowTimer;
        private float _slowMultiplier = 1f;
        public bool IsAlive => !_dead && _hp > 0f;
        public float HpFraction => _maxHp <= 0f ? 0f : Mathf.Clamp01(_hp / _maxHp);

        public void ApplySlow(float duration, float multiplier)
        {
            if (_dead) return;
            _slowTimer = Mathf.Max(_slowTimer, Mathf.Max(0f, duration));
            _slowMultiplier = Mathf.Clamp(multiplier, 0.15f, 1f);
        }

        public void KnockBackFrom(Vector2 center, float distance)
        {
            if (_dead || _runtime == null || _runtime.IsGameplayPaused) return;
            var away = ((Vector2)transform.position - center).normalized;
            if (away.sqrMagnitude <= 0.0001f) away = Vector2.up;
            transform.position += (Vector3)(away * Mathf.Max(0f, distance));
        }

        public void PullToward(Vector2 center, float distance)
        {
            if (_dead || _runtime == null || _runtime.IsGameplayPaused) return;
            transform.position = Vector2.MoveTowards(transform.position, center, Mathf.Max(0f, distance) * 0.22f);
        }

        public void Initialize(EchoSphereRuntime runtime, Transform player, float hp, float moveSpeed)
        {
            _runtime = runtime;
            _player = player;
            _hp = hp;
            _maxHp = Mathf.Max(1f, hp);
            _moveSpeed = moveSpeed;
            _shell = GetComponentInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            if (_dead || _runtime == null || _runtime.IsGameplayPaused || _player == null) return;
            if (_slowTimer > 0f) _slowTimer = Mathf.Max(0f, _slowTimer - Time.deltaTime);
            else _slowMultiplier = 1f;
            var target = (Vector2)_player.position;
            transform.position = Vector2.MoveTowards(transform.position, target, _moveSpeed * _slowMultiplier * Time.deltaTime);
            if (Vector2.Distance(transform.position, target) < 0.54f) _runtime.TryDamagePlayer(9f);
        }

        public void ReceiveDamage(float damage)
        {
            if (_dead) return;
            _hp -= Mathf.Max(0f, damage);
            if (_shell != null) _shell.color = Color.white;
            _runtime.SpawnImpact(transform.position, new Color(1f, 0.26f, 0.34f));
            if (_hp <= 0f)
            {
                _dead = true;
                _runtime.RegisterEnemyDeath(this, transform.position);
                Destroy(gameObject);
            }
        }
    }
}
