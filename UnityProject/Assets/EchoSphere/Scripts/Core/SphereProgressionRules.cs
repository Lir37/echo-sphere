using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    public enum RunUpgradeKind { Sphere, MaxSpheres, Damage, AttackSpeed, MoveSpeed, Slow, Vitality, Dodge, Crit, Magnet, Armor, Regen, Stability, Teleport, TimeStop, Blast, Shield, FireTrail, Minion, Lightning, DarkRitual, Radius, Vampire, SphereBoost, Cooldown, Momentum, Resonance, CritPower, Range, ProjectileSpeed, Fortune }

    public readonly struct RunUpgradeChoice
    {
        public readonly RunUpgradeKind Kind;
        public readonly SphereId Sphere;
        public readonly int CurrentLevel;
        public readonly int NextLevel;
        public readonly string Title;
        public readonly string Description;

        public RunUpgradeChoice(RunUpgradeKind kind, SphereId sphere, int currentLevel, int nextLevel, string title, string description)
        {
            Kind = kind; Sphere = sphere; CurrentLevel = currentLevel; NextLevel = nextLevel; Title = title; Description = description;
        }
    }

    /// <summary>First source-mapped run-progression slice. Evolution branches remain a later parity task.</summary>
    public static class SphereProgressionRules
    {
        public const int MaxLevel = 7;
        public const int MaxSphereCapacityAbilityLevel = 3;

        public static bool CanUpgradeMaxSpheres(int currentLevel) =>
            currentLevel >= 0 && currentLevel < MaxSphereCapacityAbilityLevel;

        public static RunUpgradeChoice CreateMaxSpheresChoice(int currentLevel)
        {
            var current = Math.Max(0, Math.Min(MaxSphereCapacityAbilityLevel, currentLevel));
            var next = Math.Min(MaxSphereCapacityAbilityLevel, current + 1);
            return new RunUpgradeChoice(
                RunUpgradeKind.MaxSpheres,
                SphereId.Standard,
                current,
                next,
                "+1 SPHERE",
                "+1 maximum active Sphere.");
        }

        public static int GetLevel(IReadOnlyDictionary<SphereId, int> levels, SphereId id)
        {
            if (levels == null || !levels.TryGetValue(id, out var level)) return 0;
            return Math.Max(0, Math.Min(MaxLevel, level));
        }

        public static bool IsUnlocked(IReadOnlyDictionary<SphereId, int> levels, SphereId id) => GetLevel(levels, id) > 0;
        public static int GetNextLevel(IReadOnlyDictionary<SphereId, int> levels, SphereId id)
        {
            var current = GetLevel(levels, id);
            return current >= MaxLevel ? MaxLevel : current + 1;
        }
        public static bool CanUpgrade(IReadOnlyDictionary<SphereId, int> levels, SphereId id) => GetLevel(levels, id) < MaxLevel;

        public static RunUpgradeChoice CreateSphereChoice(IReadOnlyDictionary<SphereId, int> levels, SphereId id)
        {
            var current = GetLevel(levels, id);
            var next = GetNextLevel(levels, id);
            var definition = GameCatalog.Spheres[(int)id];
            var title = current == 0 ? "UNLOCK PROGRESSION / " + definition.DisplayName : definition.DisplayName + " / LEVEL " + ToRoman(next);
            var description = current == 0
                ? "Unlock this Sphere's progression. Place the physical Sphere separately on the battlefield."
                : next == 4
                    ? "Reach level IV, then choose one of the three source mutation branches."
                    : next == 7
                        ? "Reach level VII, then choose one of the three source final forms."
                        : "Advance this Sphere to level " + ToRoman(next) + " using its authored progression.";
            return new RunUpgradeChoice(RunUpgradeKind.Sphere, id, current, next, title, description);
        }

        private static string ToRoman(int value)
        {
            switch (value)
            {
                case 1: return "I"; case 2: return "II"; case 3: return "III"; case 4: return "IV";
                case 5: return "V"; case 6: return "VI"; case 7: return "VII"; default: return value.ToString();
            }
        }
    }
}
