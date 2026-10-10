namespace EchoSphere.Core
{
    /// <summary>Baseline attack profiles ported from new-desing/src/gameData.ts.</summary>
    public readonly struct SphereCombatProfile
    {
        public readonly SphereId Id;
        public readonly float DamageMultiplier;
        public readonly float RangeMultiplier;
        public readonly float DelayMultiplier;
        public readonly float ProjectileSpeedMultiplier;
        public readonly int Pellets;
        public readonly float Spread;
        public readonly bool Chains;
        public readonly bool Aura;
        public readonly float AuraRadius;

        public SphereCombatProfile(SphereId id, float damage, float range, float delay, float projectileSpeed,
            int pellets, float spread, bool chains, bool aura, float auraRadius)
        {
            Id = id;
            DamageMultiplier = damage;
            RangeMultiplier = range;
            DelayMultiplier = delay;
            ProjectileSpeedMultiplier = projectileSpeed;
            Pellets = pellets;
            Spread = spread;
            Chains = chains;
            Aura = aura;
            AuraRadius = auraRadius;
        }
    }

    public static class SphereCombatRules
    {
        public static SphereCombatProfile GetProfile(SphereId id)
        {
            switch (id)
            {
                case SphereId.Standard: return new SphereCombatProfile(id, 1f, 1f, 1f, 1f, 1, 0f, false, false, 0f);
                case SphereId.Sniper: return new SphereCombatProfile(id, 2.5f, 2f, 2f, 2f, 1, 0f, false, false, 0f);
                case SphereId.Shotgun: return new SphereCombatProfile(id, 0.6f, 0.6f, 1.2f, 0.8f, 3, 0.4f, false, false, 0f);
                case SphereId.Chain: return new SphereCombatProfile(id, 1f, 1f, 1.3f, 1.5f, 1, 0f, true, false, 0f);
                case SphereId.Aura: return new SphereCombatProfile(id, 0.6f, 0.5f, 0.2f, 1f, 0, 0f, false, true, 80f);
                case SphereId.Orbital: return new SphereCombatProfile(id, 0.72f, 1f, 1f, 1f, 0, 0f, false, false, 105f);
                case SphereId.Prism: return new SphereCombatProfile(id, 1.25f, 1.35f, 1.35f, 2f, 3, 0.16f, false, false, 0f);
                case SphereId.Gravity: return new SphereCombatProfile(id, 0.42f, 1.1f, 0.9f, 1f, 0, 0f, false, true, 125f);
                case SphereId.Pulse: return new SphereCombatProfile(id, 1f, 1.15f, 1f, 1f, 0, 0f, false, false, 115f);
                case SphereId.Void: return new SphereCombatProfile(id, 1.15f, 1.05f, 1.2f, 1.3f, 1, 0f, false, false, 0f);
                default: return new SphereCombatProfile(SphereId.Standard, 1f, 1f, 1f, 1f, 1, 0f, false, false, 0f);
            }
        }
    }
}
