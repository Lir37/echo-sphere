using UnityEngine;
using EchoSphere.Core;

namespace EchoSphere.Runtime
{
    public sealed class PlayerCoreController2D : MonoBehaviour
    {
        public float MaxHp { get; private set; } = 100f;
        public float CurrentHp { get; private set; } = 100f;
        public float InvulnerableTimer { get; private set; }
        public float ContactGraceTimer { get; private set; }
        public float DashCooldown { get; private set; }
        public float DashTimer { get; private set; }
        public Vector2 LastMoveDirection { get; private set; } = Vector2.down;
        public Vector2 DashDirection { get; private set; } = Vector2.down;
        public bool IsDead => CurrentHp <= 0f;

        public void Tick(float deltaTime)
        {
            InvulnerableTimer = Mathf.Max(0f, InvulnerableTimer - deltaTime);
            ContactGraceTimer = Mathf.Max(0f, ContactGraceTimer - deltaTime);
            DashCooldown = Mathf.Max(0f, DashCooldown - deltaTime);
            DashTimer = Mathf.Max(0f, DashTimer - deltaTime);
        }

        public void RememberMoveDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.0001f) LastMoveDirection = direction.normalized;
        }

        public Vector2 GetVelocity(Vector2 requestedDirection, float speed, float followMultiplier)
        {
            if (DashTimer > 0f) return DashDirection * speed * 4.2f;
            return requestedDirection.sqrMagnitude <= 0.0001f ? Vector2.zero : requestedDirection.normalized * speed * followMultiplier;
        }

        public bool TryDash(Vector2 direction)
        {
            if (IsDead || DashCooldown > 0f) return false;
            if (direction.sqrMagnitude <= 0.0001f) direction = LastMoveDirection;
            DashDirection = direction.normalized;
            DashTimer = 0.14f;
            InvulnerableTimer = Mathf.Max(InvulnerableTimer, 0.30f);
            DashCooldown = 3f;
            return true;
        }

        public bool TryTakeDamage(float amount)
        {
            if (IsDead || !CombatRules.CanReceivePlayerDamage(InvulnerableTimer, ContactGraceTimer)) return false;
            CurrentHp = Mathf.Max(0f, CurrentHp - Mathf.Max(0f, amount));
            ContactGraceTimer = CombatRules.ContactDamageGraceSeconds;
            return true;
        }

        public void Heal(float amount)
        {
            if (!IsDead) CurrentHp = Mathf.Min(MaxHp, CurrentHp + Mathf.Max(0f, amount));
        }
    }
}
