using UnityEngine;
using EchoSphere.Core;

namespace EchoSphere.Runtime
{
    /// <summary>Temporary minion that follows the source anchor Sphere and uses its source attack profile.</summary>
    public sealed class AbilityDroneAgent2D : MonoBehaviour
    {
        private EchoSphereRuntime _runtime;
        private SphereId _anchorType;
        private float _remaining;
        private float _attackTimer;
        private float _rotation;
        private int _abilityLevel;
        private int _ordinal;
        private string _branch;
        private string _final;

        public bool IsNetworkNodeAlive => _remaining > 0f && gameObject.activeInHierarchy;
        public Vector2 NetworkPosition => transform.position;

        public void Initialize(EchoSphereRuntime runtime, SphereId anchorType, int abilityLevel,
            float duration, string branch, string final, int ordinal)
        {
            _runtime = runtime;
            _anchorType = anchorType;
            _abilityLevel = Mathf.Max(0, abilityLevel);
            _remaining = Mathf.Max(0.1f, duration);
            _branch = branch ?? string.Empty;
            _final = final ?? string.Empty;
            _ordinal = Mathf.Max(0, ordinal);
            _rotation = 0f;
        }

        private void Update()
        {
            if (_runtime == null || _runtime.IsGameplayPaused) return;
            var dt = Time.deltaTime;
            _remaining -= dt;
            if (_remaining <= 0f) { Destroy(gameObject); return; }

            var origin = (Vector2)transform.position;
            var anchor = _runtime.FindNearestSphereForDrone(_anchorType, origin);
            if (anchor != null)
            {
                var anchorSpeed = _anchorType == SphereId.Orbital ? 5.5f :
                    _anchorType == SphereId.Sniper ? 2.2f : 3f;
                _rotation += dt * anchorSpeed;
                var angle = _rotation * 0.7f + _ordinal * 2.1f;
                var scale = SpherePlacementRules.SourcePixelsToWorld;
                var targetPosition = (Vector2)anchor.transform.position +
                    new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (48f * scale);
                var current = (Vector2)transform.position;
                current += (targetPosition - current) * Mathf.Min(1f, dt * 4f);
                transform.position = new Vector3(current.x, current.y, transform.position.z);

                var nearby = _runtime.FindNearestSphereNearDrone(current, anchor, 240f * scale);
                if (_final == "minion_sphere_guard")
                    anchor.AccelerateNextAttack(dt * 0.12f);
                if (_branch == "minion_relay_drone" && nearby != null && _runtime.NextRunRandom() < dt * 4f)
                {
                    _runtime.SpawnCombatArc(anchor.transform.position, nearby.transform.position, anchor.Type,
                        _branch, _final, new Color(0.35f, 0.83f, 1f, 0.95f), 0.1f);
                    anchor.AccelerateNextAttack(0.16f);
                    nearby.AccelerateNextAttack(0.08f);
                }
                else if (nearby != null && _runtime.NextRunRandom() < dt * 2f)
                {
                    _runtime.SpawnCombatArc(anchor.transform.position, nearby.transform.position, anchor.Type,
                        _branch, _final, new Color(0.35f, 0.83f, 1f, 0.95f), 0.08f);
                }


            }
            else
            {
                var player = _runtime.PlayerPosition;
                var delta = player - origin;
                var distance = delta.magnitude;
                if (distance > 0.0001f)
                {
                    var next = origin + delta / distance * (90f * SpherePlacementRules.SourcePixelsToWorld * dt);
                    transform.position = new Vector3(next.x, next.y, transform.position.z);
                }
            }

            _attackTimer -= dt;
            if (_attackTimer > 0f) return;
            var attackOrigin = (Vector2)transform.position;
            var rangePixels = _anchorType == SphereId.Sniper ? 260f :
                _anchorType == SphereId.Shotgun ? 135f : 180f;
            var scaleWorld = SpherePlacementRules.SourcePixelsToWorld;
            var target = _runtime.FindNearestEnemy(attackOrigin, rangePixels * scaleWorld);
            if (target == null) return;

            var baseDamage = (5f + _abilityLevel * 2f) * (_runtime.IsDarkRitualOverloaded ? 1.25f : 1f);
            var damageMultiplier = _anchorType == SphereId.Sniper ? 1.55f :
                _anchorType == SphereId.Orbital ? 0.9f :
                _anchorType == SphereId.Void ? 1.2f : 1f;
            switch (_anchorType)
            {
                case SphereId.Shotgun:
                    for (var pellet = 0; pellet < 3; pellet++) target.ReceiveDamage(baseDamage * 0.42f);
                    break;
                case SphereId.Aura:
                case SphereId.Pulse:
                {
                    var pulseRadius = (_anchorType == SphereId.Aura ? 52f : 68f) * scaleWorld;
                    var pulseDamage = baseDamage * (_anchorType == SphereId.Pulse ? 0.62f : 0.52f);
                    foreach (var enemy in _runtime.FindEnemiesInRadius(attackOrigin, pulseRadius))
                        if (enemy != null && enemy.IsAlive) enemy.ReceiveDamage(pulseDamage);
                    break;
                }
                case SphereId.Chain:
                    target.ReceiveDamage(baseDamage);
                    var chainTargets = _runtime.FindNearestEnemies(target.transform.position, 85f * scaleWorld, 2);
                    foreach (var secondary in chainTargets)
                        if (secondary != null && secondary != target && secondary.IsAlive)
                        {
                            secondary.ReceiveDamage(baseDamage * 0.45f);
                            break;
                        }
                    break;
                default:
                    target.ReceiveDamage(baseDamage * damageMultiplier);
                    break;
            }

            if (_anchorType == SphereId.Gravity && anchor != null)
            {
                var targetPosition = (Vector2)target.transform.position;
                var anchorPosition = (Vector2)anchor.transform.position;
                var direction = anchorPosition - targetPosition;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    target.transform.position += (Vector3)(direction.normalized * (16f * scaleWorld));
                }
            }

            _attackTimer = _anchorType == SphereId.Sniper ? 1f :
                _anchorType == SphereId.Orbital ? 0.62f : 0.8f;
        }
    }
}
