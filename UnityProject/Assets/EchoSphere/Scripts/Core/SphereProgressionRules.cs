using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    public enum RunUpgradeKind { Sphere, Damage, AttackSpeed, Repair }

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
            var title = current == 0 ? "UNLOCK / " + definition.DisplayName : definition.DisplayName + " / LEVEL " + ToRoman(next);
            var description = current == 0 ? "Add this Sphere to the combat network."
                : next == 4 || next == 7 ? "Sphere level " + ToRoman(next) + ": evolution choice is a pending parity task."
                : "Sphere level " + ToRoman(next) + ": improve its combat output.";
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
