namespace EchoSphere.Core
{
    public readonly struct DamageResolution
    {
        public readonly float Damage;
        public readonly bool WasCritical;
        public DamageResolution(float damage, bool wasCritical) { Damage = damage; WasCritical = wasCritical; }
    }

    public static class CombatRules
    {
        public const float CritBase = 0.05f;
        public const float CritMultiplierBase = 1.5f;
        public const float CritHardCap = 0.75f;
        public const float ContactDamageGraceSeconds = 0.60f;
        public static float ClampCritChance(float value) => !IsFinite(value) ? 0f : Clamp(value, 0f, CritHardCap);
        public static float GetContextualCritChance(float baseChance, bool hunterMarked = false, bool architectTriangle = false)
        {
            var chance = !IsFinite(baseChance) ? CritBase : baseChance;
            if (hunterMarked) chance += 0.02f;
            if (architectTriangle) chance += 0.10f;
            return ClampCritChance(chance);
        }
        public static bool CanReceivePlayerDamage(float invulnerableTimer, float contactDamageCooldown)
            => !(IsFinite(invulnerableTimer) && invulnerableTimer > 0f) && !(IsFinite(contactDamageCooldown) && contactDamageCooldown > 0f);
        public static bool CanReceivePlayerDotDamage(float invulnerableTimer)
            => !(IsFinite(invulnerableTimer) && invulnerableTimer > 0f);
        public static DamageResolution ResolveDamage(float damage, float critChance, float critRoll, float critMultiplier)
        {
            var safeDamage = IsFinite(damage) ? System.Math.Max(0f, damage) : 0f;
            var critical = IsFinite(critRoll) && critRoll >= 0f && critRoll < ClampCritChance(critChance);
            var multiplier = IsFinite(critMultiplier) ? System.Math.Max(1f, critMultiplier) : CritMultiplierBase;
            return new DamageResolution(critical ? safeDamage * multiplier : safeDamage, critical);
        }
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static float Clamp(float value, float min, float max) => value < min ? min : value > max ? max : value;
    }
}
