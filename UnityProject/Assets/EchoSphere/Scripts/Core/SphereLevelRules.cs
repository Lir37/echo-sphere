using System;

namespace EchoSphere.Core
{
    /// <summary>
    /// Source-mapped Sphere level I-III effects from new-desing/src/sphereProgression.ts.
    /// IV/VII branch mechanics are handled separately and are not approximated here.
    /// </summary>
    public readonly struct SphereLevelStats
    {
        public readonly float DamageMultiplier, RangeMultiplier, DelayMultiplier, SpreadMultiplier;
        public readonly float AuraRadiusMultiplier, OrbitRadiusMultiplier, OrbitSpeedMultiplier, PullMultiplier;
        public readonly float CloseRangeDamageMultiplier, WeakenedDamageMultiplier, CritChanceBonus;
        public readonly int Pierce, Pellets, ChainTargets, PrismDirections;

        public SphereLevelStats(float damage, float range, float delay, float spread, float auraRadius,
            float orbitRadius, float orbitSpeed, float pull, float closeDamage, float weakenedDamage,
            float critChanceBonus, int pierce, int pellets, int chainTargets, int prismDirections)
        {
            DamageMultiplier = damage; RangeMultiplier = range; DelayMultiplier = delay;
            SpreadMultiplier = spread; AuraRadiusMultiplier = auraRadius; OrbitRadiusMultiplier = orbitRadius;
            OrbitSpeedMultiplier = orbitSpeed; PullMultiplier = pull; CloseRangeDamageMultiplier = closeDamage;
            WeakenedDamageMultiplier = weakenedDamage; CritChanceBonus = critChanceBonus;
            Pierce = pierce; Pellets = pellets; ChainTargets = chainTargets; PrismDirections = prismDirections;
        }
    }

    public static class SphereLevelRules
    {
        public static SphereLevelStats GetStats(SphereId id, int level)
        {
            var l = Math.Max(1, Math.Min(7, level));
            var damage = 1f; var range = 1f; var delay = 1f; var spread = 1f;
            var aura = 1f; var orbitRadius = 1f; var orbitSpeed = 1f; var pull = 1f;
            var closeDamage = 1f; var weakenedDamage = 1f; var crit = 0f;
            var pierce = 0; var pellets = 3; var chainTargets = 3; var prismDirections = 1;

            switch (id)
            {
                case SphereId.Standard:
                    if (l >= 1) damage *= 1.15f;
                    if (l >= 2) pierce += 1;
                    if (l >= 3) delay *= 0.90f;
                    break;
                case SphereId.Sniper:
                    if (l >= 1) damage *= 1.25f;
                    if (l >= 2) range *= 1.15f;
                    if (l >= 3) crit += 0.15f;
                    break;
                case SphereId.Shotgun:
                    if (l >= 1) pellets += 1;
                    if (l >= 2) closeDamage *= 1.20f;
                    if (l >= 3) spread *= 0.88f;
                    break;
                case SphereId.Chain:
                    if (l >= 1) chainTargets += 1;
                    if (l >= 2) damage *= 1.10f;
                    if (l >= 3) delay *= 0.85f;
                    break;
                case SphereId.Aura:
                    if (l >= 1) aura *= 1.20f;
                    if (l >= 2) delay *= 0.90f;
                    if (l >= 3) damage *= 1.10f;
                    break;
                case SphereId.Orbital:
                    if (l >= 1) damage *= 1.15f;
                    if (l >= 2) orbitRadius *= 1.15f;
                    if (l >= 3) orbitSpeed *= 1.15f;
                    break;
                case SphereId.Prism:
                    if (l >= 1) damage *= 1.20f;
                    if (l >= 2) range *= 1.15f;
                    if (l >= 3) prismDirections += 1;
                    break;
                case SphereId.Gravity:
                    if (l >= 1) pull *= 1.20f;
                    if (l >= 2) aura *= 1.15f;
                    if (l >= 3) delay *= 0.85f;
                    break;
                case SphereId.Pulse:
                    if (l >= 1) damage *= 1.20f;
                    if (l >= 2) aura *= 1.15f;
                    if (l >= 3) delay *= 0.88f;
                    break;
                case SphereId.Void:
                    if (l >= 1) weakenedDamage *= 1.20f;
                    if (l >= 2) crit += 0.10f;
                    if (l >= 3) range *= 1.15f;
                    break;
            }

            return new SphereLevelStats(damage, range, delay, spread, aura, orbitRadius, orbitSpeed, pull,
                closeDamage, weakenedDamage, crit, pierce, pellets, chainTargets, prismDirections);
        }
    }
}
