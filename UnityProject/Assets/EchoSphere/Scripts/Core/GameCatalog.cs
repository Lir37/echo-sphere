using System.Collections.Generic;

namespace EchoSphere.Core
{
    public enum SphereId { Standard, Sniper, Chain, Shotgun, Aura, Orbital, Prism, Gravity, Pulse, Void }
    public enum SphereRole { SustainedDamage, LongRangeSingleTarget, MultiTargetRelay, CloseBurst, PersistentAreaControl, OrbitingContact, RefractionBeam, GravityControl, PeriodicWave, ConditionalExecution }

    public readonly struct SphereDefinition
    {
        public readonly SphereId Id;
        public readonly string DisplayName;
        public readonly SphereRole Role;
        public SphereDefinition(SphereId id, string displayName, SphereRole role) { Id = id; DisplayName = displayName; Role = role; }
    }

    /// <summary>Canonical final roster from Blueprint v1.2; balance belongs in data assets.</summary>
    public static class GameCatalog
    {
        private static readonly SphereDefinition[] SphereDefinitions =
        {
            new SphereDefinition(SphereId.Standard, "Standard", SphereRole.SustainedDamage),
            new SphereDefinition(SphereId.Sniper, "Sniper", SphereRole.LongRangeSingleTarget),
            new SphereDefinition(SphereId.Chain, "Chain", SphereRole.MultiTargetRelay),
            new SphereDefinition(SphereId.Shotgun, "Shotgun", SphereRole.CloseBurst),
            new SphereDefinition(SphereId.Aura, "Aura", SphereRole.PersistentAreaControl),
            new SphereDefinition(SphereId.Orbital, "Orbital", SphereRole.OrbitingContact),
            new SphereDefinition(SphereId.Prism, "Prism", SphereRole.RefractionBeam),
            new SphereDefinition(SphereId.Gravity, "Gravity", SphereRole.GravityControl),
            new SphereDefinition(SphereId.Pulse, "Pulse", SphereRole.PeriodicWave),
            new SphereDefinition(SphereId.Void, "Void", SphereRole.ConditionalExecution)
        };
        public static IReadOnlyList<SphereDefinition> Spheres => SphereDefinitions;
    }
}
