using UnityEngine;
using EchoSphere.Core;

namespace EchoSphere.Runtime
{
    /// <summary>Source-mapped projectile body from renderer.ts:drawBossProjectile.</summary>
    public sealed class BossProjectileAgent2D : MonoBehaviour, ISourceSimulationAgent
    {
        internal Vector2 SourceProjectionVelocity => _velocity;
        internal float SourceProjectionHitRadius => _hitRadius;
        internal string SourceProjectionBossType => _bossType;
        private EchoSphereRuntime _runtime;
        private Transform _player;
        private Vector2 _velocity;
        private float _damage, _hitRadius, _time;
        private Color _color;
        private string _bossType = "";
        private EnemyAgent2D _sourceOwner;
        private bool _hasSourceOwner, _removed;
        public bool HasSourceOwner => _hasSourceOwner;
        public EnemyAgent2D SourceOwner => _sourceOwner;
        public void SetSourceOwner(EnemyAgent2D owner) { _sourceOwner = owner; _hasSourceOwner = owner != null; }

        public void Initialize(EchoSphereRuntime runtime, Transform player, Vector2 velocity,
            float sourceRadius, float damage, string bossType, Color color)
        {
            _runtime = runtime;
            _runtime.RegisterSimulationAgent(this);
            _player = player;
            _velocity = velocity;
            _damage = Mathf.Max(0f, damage);
            _color = color;
            _bossType = bossType ?? "";
            _hitRadius = Mathf.Max(0.02f, sourceRadius / BossCombatRules.SourceUnitsPerWorldUnit) + BossCombatRules.PlayerRadiusWorld;
            transform.localScale = Vector3.one * Mathf.Max(0.04f, sourceRadius / BossCombatRules.SourceUnitsPerWorldUnit);
            if (_velocity.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg);

        }

        public void SimulationTick(float dt)
        {
            if (_removed || _runtime == null || _runtime.IsGameplayPaused || _player == null) return;
            _time += dt;
            transform.position += (Vector3)(_velocity * dt);
            if (Vector2.Distance(transform.position, _player.position) < _hitRadius)
            {
                _runtime.TryDamagePlayer(_damage);
                _removed = true;
                Destroy(gameObject);
                return;
            }

            var delta = (Vector2)transform.position - (Vector2)_player.position;
            if (Mathf.Abs(delta.x) > 1000f * SpherePlacementRules.SourcePixelsToWorld || Mathf.Abs(delta.y) > 1000f * SpherePlacementRules.SourcePixelsToWorld)
            {
                _removed = true;
                Destroy(gameObject);
                return;
            }
        }

    }
}
