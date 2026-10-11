using System.Collections.Generic;

namespace EchoSphere.Core
{
    public enum SphereId { Standard, Sniper, Chain, Shotgun, Aura, Orbital, Prism, Gravity, Pulse, Void }
    public enum SphereRole { SustainedDamage, LongRangeSingleTarget, MultiTargetRelay, CloseBurst, PersistentAreaControl, OrbitingContact, RefractionBeam, GravityControl, PeriodicWave, ConditionalExecution }

    public readonly struct SphereDefinition
    {
        public readonly SphereId Id;
        public readonly string DisplayName;
        public readonly string DisplayNameRu;
        public readonly string DescriptionEn;
        public readonly string DescriptionRu;
        public readonly string ColorHex;
        public readonly SphereRole Role;

        public SphereDefinition(
            SphereId id, string displayName, string displayNameRu, string descriptionEn,
            string descriptionRu, string colorHex, SphereRole role)
        {
            Id = id;
            DisplayName = displayName;
            DisplayNameRu = displayNameRu;
            DescriptionEn = descriptionEn;
            DescriptionRu = descriptionRu;
            ColorHex = colorHex;
            Role = role;
        }
    }

    /// <summary>Canonical final roster from Blueprint v1.2; balance belongs in data assets.</summary>
    public static class GameCatalog
    {
        private static readonly SphereDefinition[] SphereDefinitions =
        {
            new SphereDefinition(SphereId.Standard, "Standard", "Стандартная", "Balanced sphere", "Сбалансированная сфера", "#55dfff", SphereRole.SustainedDamage),
            new SphereDefinition(SphereId.Sniper, "Sniper", "Снайпер", "High damage, long range, slow", "Высокий урон, большая дальность, медленная", "#e86cff", SphereRole.LongRangeSingleTarget),
            new SphereDefinition(SphereId.Chain, "Chain", "Цепная", "Lightning jumps between enemies", "Молния прыгает между врагами", "#ffe25b", SphereRole.MultiTargetRelay),
            new SphereDefinition(SphereId.Shotgun, "Shotgun", "Дробовик", "3 pellets, short range", "3 снаряда, короткая дальность", "#ff8f3d", SphereRole.CloseBurst),
            new SphereDefinition(SphereId.Aura, "Aura", "Аура", "Continuous AoE damage", "Непрерывный урон по площади", "#57e6b4", SphereRole.PersistentAreaControl),
            new SphereDefinition(SphereId.Orbital, "Orbital", "Орбитальная", "Combat elements cut enemies on two counter-rotating orbits; starts 1+1 and reaches 4+4 at VII.", "Боевые элементы режут врагов на двух орбитах: внутренняя вращается по часовой, внешняя против часовой. Уровень I начинается с 1+1, затем добавления чередуются до 4+4 на VII.", "#8ef0ff", SphereRole.OrbitingContact),
            new SphereDefinition(SphereId.Prism, "Prism", "Призма", "A beam splits into additional directions", "Луч разделяется на дополнительные направления", "#ff8de1", SphereRole.RefractionBeam),
            new SphereDefinition(SphereId.Gravity, "Gravity", "Гравитационная", "Pulls enemies inward and pulses damage", "Стягивает врагов к центру и наносит импульсный урон", "#a58cff", SphereRole.GravityControl),
            new SphereDefinition(SphereId.Pulse, "Pulse", "Импульсная", "Periodic damage waves around the node", "Периодические волны урона вокруг узла", "#ffd35a", SphereRole.PeriodicWave),
            new SphereDefinition(SphereId.Void, "Void", "Пустотная", "Executes weakened enemies and ruptures dense groups", "Добивает ослабленных врагов и разрывает плотные группы", "#c28cff", SphereRole.ConditionalExecution)
        };
        public static IReadOnlyList<SphereDefinition> Spheres => SphereDefinitions;
    }
}
