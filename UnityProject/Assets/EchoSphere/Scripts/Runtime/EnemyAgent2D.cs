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
        private int _xpReward = 1;
        private float _moveSpeed;
        private bool _dead;
        private float _slowTimer;
        private float _slowMultiplier = 1f;
        private float _markTimer;
        private float _dotTimer;
        private float _dotRemaining;
        private float _dotDamagePerSecond;
        private Color _baseColor = Color.white;
        public bool IsMarked => _markTimer > 0f;
        public float CurrentHp => Mathf.Max(0f, _hp);
        public bool IsAlive => !_dead && _hp > 0f;
        public float HpFraction => _maxHp <= 0f ? 0f : Mathf.Clamp01(_hp / _maxHp);

        public void ApplyDamageOverTime(float damagePerSecond, float duration)
        {
            if (_dead) return;
            _dotDamagePerSecond = Mathf.Max(_dotDamagePerSecond, Mathf.Max(0f, damagePerSecond));
            _dotRemaining = Mathf.Max(_dotRemaining, Mathf.Max(0f, duration));
            if (_dotTimer <= 0f) _dotTimer = 0.5f;
        }

        public void ApplyMark(float duration)
        {
            if (_dead) return;
            _markTimer = Mathf.Max(_markTimer, Mathf.Max(0f, duration));
            if (_shell != null) _shell.color = new Color(0.5f, 0.82f, 1f, 1f);
        }

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

        public void PullByDistance(Vector2 center, float distance)
        {
            if (_dead || _runtime == null || _runtime.IsGameplayPaused) return;
            transform.position = Vector2.MoveTowards(transform.position, center, Mathf.Max(0f, distance));
        }

        public void PullToward(Vector2 center, float distance)
        {
            if (_dead || _runtime == null || _runtime.IsGameplayPaused) return;
            transform.position = Vector2.MoveTowards(transform.position, center, Mathf.Max(0f, distance) * 0.22f);
        }

        public void Initialize(EchoSphereRuntime runtime, Transform player, float hp, float moveSpeed, int xpReward = 1)
        {
            _runtime = runtime;
            _player = player;
            _hp = hp;
            _maxHp = Mathf.Max(1f, hp);
            _moveSpeed = Mathf.Max(0f, moveSpeed);
            _xpReward = Mathf.Max(1, xpReward);
            _shell = transform.Find("Shell")?.GetComponent<SpriteRenderer>();
            if (_shell != null) _baseColor = _shell.color;
        }

        private void Update()
        {
            if (_dead || _runtime == null || _runtime.IsGameplayPaused || _player == null) return;
            if (_dotRemaining > 0f)
            {
                _dotRemaining = Mathf.Max(0f, _dotRemaining - Time.deltaTime);
                _dotTimer -= Time.deltaTime;
                if (_dotTimer <= 0f)
                {
                    ReceiveDamage(_dotDamagePerSecond * 0.5f);
                    _dotTimer = 0.5f;
                    if (_dead) return;
                }
                if (_dotRemaining <= 0f) _dotDamagePerSecond = 0f;
            }
            if (_markTimer > 0f)
            {
                _markTimer = Mathf.Max(0f, _markTimer - Time.deltaTime);
                if (_markTimer <= 0f && _shell != null) _shell.color = _baseColor;
            }
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
            if (_shell != null) _shell.color = IsMarked ? new Color(0.5f, 0.82f, 1f, 1f) : _baseColor;
            _runtime.SpawnImpact(transform.position, new Color(1f, 0.26f, 0.34f));
            if (_hp <= 0f)
            {
                _dead = true;
                _runtime.RegisterEnemyDeath(this, transform.position, _xpReward);
                Destroy(gameObject);
            }
        }
    }
}
