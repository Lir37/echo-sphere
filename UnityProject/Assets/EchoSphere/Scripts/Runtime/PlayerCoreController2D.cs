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
        public int ShieldCharges { get; private set; }
        public int DashCount { get; private set; }
        public Vector2 LastMoveDirection { get; private set; } = Vector2.down;
        public Vector2 DashDirection { get; private set; } = Vector2.down;
        public bool IsDead => CurrentHp <= 0f;
        public bool CanReceiveDamageNow => !IsDead && CombatRules.CanReceivePlayerDamage(InvulnerableTimer, ContactGraceTimer);
        private float _dashCooldownMultiplier = 1f;
        public void SetDashCooldownMultiplier(float multiplier) => _dashCooldownMultiplier = Mathf.Clamp(multiplier, 0.55f, 1.5f);
        public void ApplyBaseMaxHpMultiplier(float multiplier)
        {
            var previousMax = Mathf.Max(1f, MaxHp);
            var ratio = Mathf.Clamp01(CurrentHp / previousMax);
            MaxHp = Mathf.Max(1f, MaxHp * Mathf.Max(0.01f, multiplier));
            CurrentHp = Mathf.Clamp(MaxHp * ratio, 0f, MaxHp);
        }

        public void SetMaxHpForNewRun(float maxHp)
        {
            MaxHp = Mathf.Max(1f, maxHp);
            CurrentHp = MaxHp;
        }

        public void IncreaseMaxHp(float amount)
        {
            var gain = Mathf.Max(0f, amount);
            MaxHp += gain;
            CurrentHp = Mathf.Min(MaxHp, CurrentHp + gain);
        }

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
            DashCount++;
            DashTimer = 0.14f;
            InvulnerableTimer = Mathf.Max(InvulnerableTimer, 0.30f);
            DashCooldown = 3f * _dashCooldownMultiplier;
            return true;
        }

        public void GrantInvulnerability(float duration) =>
            InvulnerableTimer = Mathf.Max(InvulnerableTimer, Mathf.Max(0f, duration));

        public bool TryTakeDamage(float amount)
        {
            if (IsDead || !CombatRules.CanReceivePlayerDamage(InvulnerableTimer, ContactGraceTimer)) return false;
            if (ShieldCharges > 0)
            {
                ShieldCharges--;
                InvulnerableTimer = Mathf.Max(InvulnerableTimer, 0.45f);
                ContactGraceTimer = Mathf.Max(ContactGraceTimer, CombatRules.ContactDamageGraceSeconds);
                return false;
            }
            CurrentHp = Mathf.Max(0f, CurrentHp - Mathf.Max(0f, amount));
            ContactGraceTimer = CombatRules.ContactDamageGraceSeconds;
            return true;
        }

        public bool TryTakeDotDamage(float amount)
        {
            if (IsDead || !CombatRules.CanReceivePlayerDotDamage(InvulnerableTimer)) return false;
            CurrentHp = Mathf.Max(0f, CurrentHp - Mathf.Max(0f, amount));
            return true;
        }

        public void SetShieldCharges(int amount)
        {
            ShieldCharges = Mathf.Clamp(amount, 0, 5);
        }

        public void GrantShieldCharge(int amount = 1)
        {
            ShieldCharges = Mathf.Clamp(ShieldCharges + Mathf.Max(0, amount), 0, 5);
        }

        public bool SpendHp(float amount)
        {
            var cost = Mathf.Max(0f, amount);
            if (IsDead || CurrentHp <= cost) return false;
            CurrentHp -= cost;
            return true;
        }

        public void Heal(float amount)
        {
            if (!IsDead) CurrentHp = Mathf.Min(MaxHp, CurrentHp + Mathf.Max(0f, amount));
        }
    }
}
