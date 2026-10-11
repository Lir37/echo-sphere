using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoSphere.Core
{
    public enum SphereAuthoredModifier
    {
        Multishot, Pierce, Ricochet, Fire, Freeze, Poison, Breach, Overload,
        Split, Shatter, Execute, Mark, Echo, Anchor, Phase, Static, Resonant,
        Magnetic, Vampiric, Corrupt, Drain, Afterimage, Impact, Gravitic
    }

    /// <summary>
    /// Exact branch/final authored modifier overrides from new-desing/src/sphereProgression.ts.
    /// Final values follow the source rule: max(branch base modifier, selected final modifier).
    /// </summary>
    public static class SphereAuthoredModifierRules
    {
        private readonly struct Entry
        {
            public readonly SphereAuthoredModifier Kind;
            public readonly int Level;
            public Entry(SphereAuthoredModifier kind, int level) { Kind = kind; Level = level; }
        }

        private static Entry P(SphereAuthoredModifier kind, int level) => new Entry(kind, level);

        private static Dictionary<SphereAuthoredModifier, int> M(params Entry[] entries)
        {
            var result = new Dictionary<SphereAuthoredModifier, int>();
            foreach (var entry in entries)
                result[entry.Kind] = Math.Max(0, entry.Level);
            return result;
        }

        private static readonly Dictionary<string, Dictionary<SphereAuthoredModifier, int>> BaseLevels =
            new Dictionary<string, Dictionary<SphereAuthoredModifier, int>>(StringComparer.Ordinal)
            {
                ["standard_resonator"] = M(P(SphereAuthoredModifier.Resonant, 1)),
                ["standard_singularity"] = M(P(SphereAuthoredModifier.Anchor, 1), P(SphereAuthoredModifier.Gravitic, 1)),
                ["standard_swarm"] = M(),
                ["sniper_oracle"] = M(P(SphereAuthoredModifier.Mark, 1)),
                ["sniper_assassin"] = M(),
                ["sniper_beacon"] = M(P(SphereAuthoredModifier.Anchor, 1), P(SphereAuthoredModifier.Mark, 1)),
                ["shotgun_burst"] = M(P(SphereAuthoredModifier.Multishot, 1)),
                ["shotgun_cataclysm"] = M(P(SphereAuthoredModifier.Shatter, 1)),
                ["shotgun_hail"] = M(P(SphereAuthoredModifier.Multishot, 1)),
                ["chain_web"] = M(P(SphereAuthoredModifier.Anchor, 1)),
                ["chain_storm"] = M(P(SphereAuthoredModifier.Static, 1), P(SphereAuthoredModifier.Resonant, 1)),
                ["chain_leech"] = M(P(SphereAuthoredModifier.Vampiric, 1)),
                ["aura_sanctum"] = M(P(SphereAuthoredModifier.Freeze, 1), P(SphereAuthoredModifier.Anchor, 1)),
                ["aura_gravity"] = M(P(SphereAuthoredModifier.Gravitic, 1), P(SphereAuthoredModifier.Anchor, 1)),
                ["aura_overgrowth"] = M(),
                ["orbital_dance"] = M(P(SphereAuthoredModifier.Afterimage, 1)),
                ["orbital_halo"] = M(P(SphereAuthoredModifier.Resonant, 1)),
                ["orbital_blade"] = M(P(SphereAuthoredModifier.Impact, 1), P(SphereAuthoredModifier.Afterimage, 1)),
                ["prism_split"] = M(P(SphereAuthoredModifier.Multishot, 1)),
                ["prism_spectrum"] = M(P(SphereAuthoredModifier.Resonant, 1)),
                ["prism_mirror"] = M(P(SphereAuthoredModifier.Ricochet, 1)),
                ["gravity_well"] = M(P(SphereAuthoredModifier.Anchor, 1), P(SphereAuthoredModifier.Gravitic, 1)),
                ["gravity_tide"] = M(P(SphereAuthoredModifier.Impact, 1), P(SphereAuthoredModifier.Gravitic, 1)),
                ["gravity_collapse"] = M(P(SphereAuthoredModifier.Execute, 1), P(SphereAuthoredModifier.Gravitic, 1)),
                ["pulse_wave"] = M(P(SphereAuthoredModifier.Impact, 1)),
                ["pulse_resonator"] = M(P(SphereAuthoredModifier.Resonant, 1)),
                ["pulse_burst"] = M(P(SphereAuthoredModifier.Shatter, 1)),
                ["void_hunger"] = M(P(SphereAuthoredModifier.Corrupt, 1)),
                ["void_reaper"] = M(P(SphereAuthoredModifier.Vampiric, 1)),
                ["void_execution"] = M(P(SphereAuthoredModifier.Execute, 1), P(SphereAuthoredModifier.Phase, 1))
            };

        private static readonly Dictionary<string, Dictionary<SphereAuthoredModifier, int>[]> FinalLevels =
            new Dictionary<string, Dictionary<SphereAuthoredModifier, int>[]>(StringComparer.Ordinal)
            {
                ["standard_resonator"] = new[] { M(), M(P(SphereAuthoredModifier.Impact, 1)), M() },
                ["standard_singularity"] = new[] { M(P(SphereAuthoredModifier.Gravitic, 2)), M(), M(P(SphereAuthoredModifier.Gravitic, 2)) },
                ["standard_swarm"] = new[] { M(), M(), M() },
                ["sniper_oracle"] = new[] { M(P(SphereAuthoredModifier.Mark, 2)), M(P(SphereAuthoredModifier.Mark, 2), P(SphereAuthoredModifier.Resonant, 1)), M(P(SphereAuthoredModifier.Mark, 2), P(SphereAuthoredModifier.Execute, 1)) },
                ["sniper_assassin"] = new[] { M(P(SphereAuthoredModifier.Execute, 1)), M(P(SphereAuthoredModifier.Execute, 2)), M(P(SphereAuthoredModifier.Execute, 1), P(SphereAuthoredModifier.Vampiric, 1)) },
                ["sniper_beacon"] = new[] { M(P(SphereAuthoredModifier.Anchor, 2)), M(P(SphereAuthoredModifier.Mark, 2)), M(P(SphereAuthoredModifier.Anchor, 2), P(SphereAuthoredModifier.Mark, 2)) },
                ["shotgun_burst"] = new[] { M(P(SphereAuthoredModifier.Multishot, 2)), M(), M(P(SphereAuthoredModifier.Multishot, 2)) },
                ["shotgun_cataclysm"] = new[] { M(P(SphereAuthoredModifier.Shatter, 2)), M(P(SphereAuthoredModifier.Impact, 2)), M(P(SphereAuthoredModifier.Shatter, 2), P(SphereAuthoredModifier.Impact, 2)) },
                ["shotgun_hail"] = new[] { M(), M(P(SphereAuthoredModifier.Multishot, 2)), M(P(SphereAuthoredModifier.Multishot, 1)) },
                ["chain_web"] = new[] { M(P(SphereAuthoredModifier.Anchor, 2)), M(P(SphereAuthoredModifier.Static, 1)), M(P(SphereAuthoredModifier.Anchor, 2), P(SphereAuthoredModifier.Static, 1)) },
                ["chain_storm"] = new[] { M(P(SphereAuthoredModifier.Static, 2)), M(P(SphereAuthoredModifier.Resonant, 2)), M(P(SphereAuthoredModifier.Static, 1), P(SphereAuthoredModifier.Resonant, 1)) },
                ["chain_leech"] = new[] { M(P(SphereAuthoredModifier.Vampiric, 2)), M(P(SphereAuthoredModifier.Drain, 1)), M(P(SphereAuthoredModifier.Vampiric, 2), P(SphereAuthoredModifier.Drain, 1)) },
                ["aura_sanctum"] = new[] { M(P(SphereAuthoredModifier.Freeze, 2)), M(P(SphereAuthoredModifier.Anchor, 2)), M(P(SphereAuthoredModifier.Freeze, 2), P(SphereAuthoredModifier.Anchor, 2)) },
                ["aura_gravity"] = new[] { M(P(SphereAuthoredModifier.Gravitic, 2)), M(P(SphereAuthoredModifier.Gravitic, 1), P(SphereAuthoredModifier.Anchor, 2)), M(P(SphereAuthoredModifier.Gravitic, 2), P(SphereAuthoredModifier.Anchor, 2)) },
                ["aura_overgrowth"] = new[] { M(), M(), M() },
                ["orbital_dance"] = new[] { M(P(SphereAuthoredModifier.Afterimage, 2)), M(P(SphereAuthoredModifier.Afterimage, 3)), M(P(SphereAuthoredModifier.Afterimage, 3)) },
                ["orbital_halo"] = new[] { M(P(SphereAuthoredModifier.Resonant, 1)), M(), M(P(SphereAuthoredModifier.Resonant, 2)) },
                ["orbital_blade"] = new[] { M(P(SphereAuthoredModifier.Impact, 2)), M(P(SphereAuthoredModifier.Afterimage, 2)), M(P(SphereAuthoredModifier.Impact, 2), P(SphereAuthoredModifier.Afterimage, 3)) },
                ["prism_split"] = new[] { M(P(SphereAuthoredModifier.Multishot, 2)), M(P(SphereAuthoredModifier.Multishot, 1), P(SphereAuthoredModifier.Pierce, 2)), M(P(SphereAuthoredModifier.Multishot, 1), P(SphereAuthoredModifier.Ricochet, 1)) },
                ["prism_spectrum"] = new[] { M(P(SphereAuthoredModifier.Fire, 1)), M(P(SphereAuthoredModifier.Freeze, 1)), M(P(SphereAuthoredModifier.Poison, 1)) },
                ["prism_mirror"] = new[] { M(P(SphereAuthoredModifier.Ricochet, 1)), M(P(SphereAuthoredModifier.Ricochet, 2)), M(P(SphereAuthoredModifier.Ricochet, 2), P(SphereAuthoredModifier.Echo, 1)) },
                ["gravity_well"] = new[] { M(P(SphereAuthoredModifier.Anchor, 2)), M(P(SphereAuthoredModifier.Gravitic, 2)), M(P(SphereAuthoredModifier.Anchor, 2), P(SphereAuthoredModifier.Gravitic, 2)) },
                ["gravity_tide"] = new[] { M(P(SphereAuthoredModifier.Impact, 2)), M(P(SphereAuthoredModifier.Gravitic, 2)), M(P(SphereAuthoredModifier.Impact, 1), P(SphereAuthoredModifier.Gravitic, 1)) },
                ["gravity_collapse"] = new[] { M(P(SphereAuthoredModifier.Execute, 2)), M(P(SphereAuthoredModifier.Gravitic, 2)), M(P(SphereAuthoredModifier.Execute, 2), P(SphereAuthoredModifier.Gravitic, 2)) },
                ["pulse_wave"] = new[] { M(P(SphereAuthoredModifier.Impact, 2)), M(P(SphereAuthoredModifier.Freeze, 1)), M(P(SphereAuthoredModifier.Impact, 2), P(SphereAuthoredModifier.Freeze, 1)) },
                ["pulse_resonator"] = new[] { M(P(SphereAuthoredModifier.Resonant, 2)), M(P(SphereAuthoredModifier.Resonant, 2), P(SphereAuthoredModifier.Impact, 1)), M(P(SphereAuthoredModifier.Resonant, 2), P(SphereAuthoredModifier.Shatter, 1)) },
                ["pulse_burst"] = new[] { M(P(SphereAuthoredModifier.Shatter, 2)), M(P(SphereAuthoredModifier.Impact, 2)), M(P(SphereAuthoredModifier.Shatter, 2), P(SphereAuthoredModifier.Impact, 2)) },
                ["void_hunger"] = new[] { M(P(SphereAuthoredModifier.Corrupt, 2)), M(P(SphereAuthoredModifier.Corrupt, 2), P(SphereAuthoredModifier.Resonant, 1)), M(P(SphereAuthoredModifier.Corrupt, 2), P(SphereAuthoredModifier.Drain, 1)) },
                ["void_reaper"] = new[] { M(P(SphereAuthoredModifier.Vampiric, 2)), M(P(SphereAuthoredModifier.Drain, 2)), M(P(SphereAuthoredModifier.Vampiric, 2), P(SphereAuthoredModifier.Drain, 2)) },
                ["void_execution"] = new[] { M(P(SphereAuthoredModifier.Execute, 2)), M(P(SphereAuthoredModifier.Phase, 1)), M(P(SphereAuthoredModifier.Execute, 2), P(SphereAuthoredModifier.Phase, 2)) }
            };

        public static IReadOnlyList<SphereAuthoredModifier> AllModifiers { get; } =
            (SphereAuthoredModifier[])Enum.GetValues(typeof(SphereAuthoredModifier));

        public static int BranchCount => BaseLevels.Count;
        public static int FinalChoiceCount
        {
            get
            {
                var count = 0;
                foreach (var choices in FinalLevels.Values) count += choices.Length;
                return count;
            }
        }

        public static int GetLevel(string branchId, string finalId, SphereAuthoredModifier modifier)
        {
            if (string.IsNullOrEmpty(branchId) || !BaseLevels.TryGetValue(branchId, out var baseMap)) return 0;
            var level = baseMap.TryGetValue(modifier, out var baseLevel) ? baseLevel : 0;
            if (string.IsNullOrEmpty(finalId) || !FinalLevels.TryGetValue(branchId, out var choices)) return level;
            var index = finalId == branchId + "_final_1" ? 0
                : finalId == branchId + "_final_2" ? 1
                : finalId == branchId + "_final_3" ? 2 : -1;
            if (index < 0) return level;
            var finalMap = choices[index];
            return Math.Max(level, finalMap.TryGetValue(modifier, out var finalLevel) ? finalLevel : 0);
        }

        public static float GetDamageMultiplier(string branchId, string finalId)
        {
            var overload = GetLevel(branchId, finalId, SphereAuthoredModifier.Overload);
            var shatter = GetLevel(branchId, finalId, SphereAuthoredModifier.Shatter);
            var mark = GetLevel(branchId, finalId, SphereAuthoredModifier.Mark);
            return (1f + 0.08f * overload) * (1f + 0.04f * shatter) * (1f + 0.04f * mark);
        }

        public static float GetExecuteDamageMultiplier(int level, float hpFraction) =>
            level > 0 && hpFraction <= 0.30f ? 1f + 0.18f * level : 1f;

        public static float GetMarkedDamageMultiplier(int level, bool marked) =>
            level > 0 && marked ? 1f + 0.15f * level : 1f;

        public static float GetCorruptDamageMultiplier(int level, int stackCount) =>
            level > 0 ? 1f + 0.04f * Math.Max(0, Math.Min(5, stackCount)) : 1f;

        public static float GetImpactDistancePixels(int level) => Math.Max(0, level) * 18f;
        public static float GetGraviticRangePixels() => 72f;
        public static float GetGraviticPullDistancePixels(int level) => Math.Min(10f, Math.Max(0, level) * 4f);
        public static float GetAnchorSlowDuration(int level) => level > 0 ? 0.8f + 0.25f * level : 0f;
        public static float GetAnchorSlowMultiplier(int level) => level > 0 ? Math.Max(0.35f, 0.65f - 0.05f * level) : 1f;
        public static float GetVampiricHealFraction(int level) => Math.Max(0, level) * 0.006f;
        public static int GetResonantCharge(int level) => Math.Max(0, level);
        public static float GetShatterRadiusPixels() => 48f;
        public static float GetShatterDamageMultiplier(int level) => level > 0 ? 0.20f + 0.08f * level : 0f;

        public static float GetSplitChance(int level) => Math.Min(0.45f, 0.18f * Math.Max(0, level));
        public static float GetEchoChance(int level) => Math.Min(0.35f, 0.12f * Math.Max(0, level));
        public static float GetStaticChance(int level) => Math.Min(0.35f, 0.12f * Math.Max(0, level));
        public static float GetAfterimageChance(int level) => Math.Min(0.40f, 0.16f * Math.Max(0, level));
        public static float GetAfterimageDamageMultiplier(int level) =>
            Math.Min(0.32f, 0.16f + 0.04f * Math.Max(0, level));

        public static int GetProjectilePierceBonus(string branchId, string finalId) =>
            GetLevel(branchId, finalId, SphereAuthoredModifier.Breach) +
            GetLevel(branchId, finalId, SphereAuthoredModifier.Pierce) +
            GetLevel(branchId, finalId, SphereAuthoredModifier.Phase);

        public static int GetProjectileRicochetCount(string branchId, string finalId) =>
            GetLevel(branchId, finalId, SphereAuthoredModifier.Ricochet);

        public static int GetMultishotCount(string branchId, string finalId) =>
            GetLevel(branchId, finalId, SphereAuthoredModifier.Multishot);
    }
}
