using System;
using System.Collections.Generic;
using System.Reflection;

namespace EchoSphere.Core
{
    public static class SourceSphereGlobalModifiers
    {
        private static readonly Dictionary<string, FieldInfo> Fields = new Dictionary<string, FieldInfo>(StringComparer.Ordinal);
        static SourceSphereGlobalModifiers()
        { foreach (var field in typeof(SourceSphereModifiers).GetFields()) Fields[field.Name] = field; }

        // sphereModifiers combines authored and global modifier levels using max, never addition.
        public static SourceSphereModifiers Merge(SourceSphereModifiers mods, SphereId type, string branch, string final, IReadOnlyDictionary<string, int> global, int abilityRange)
        {
            var projectile = type == SphereId.Standard || type == SphereId.Sniper || type == SphereId.Shotgun || type == SphereId.Prism || type == SphereId.Void;
            var oldOverload = mods.overload; var oldShatter = mods.shatter; var oldMark = mods.mark; var oldAnchor = mods.anchor;
            var oldBreach = mods.breach; var oldPhase = mods.phase;
            var resonantScale = mods.resonant > 0f ? mods.resonantCharge / mods.resonant : 2f;
            if (global != null) foreach (var pair in global)
            {
                if (!Fields.TryGetValue(pair.Key, out var field) || pair.Key == "damage" || pair.Key == "radius" || pair.Key == "delay" || pair.Key == "rotationSpeed") continue;
                var previous = (float)field.GetValue(mods);
                if (pair.Key == "pierce") { if (projectile) mods.pierce += Math.Max(0, pair.Value - SphereAuthoredModifierRules.GetLevel(branch, final, SphereAuthoredModifier.Pierce)); }
                else if (pair.Key == "multishot") { if (projectile) mods.multishot += Math.Max(0, pair.Value - SphereAuthoredModifierRules.GetLevel(branch, final, SphereAuthoredModifier.Multishot)); }
                else field.SetValue(mods, Math.Max(previous, pair.Value));
            }
            mods.damage *= (1f + 0.08f * mods.overload) / (1f + 0.08f * oldOverload);
            mods.damage *= (1f + 0.04f * mods.shatter) / (1f + 0.04f * oldShatter);
            mods.damage *= (1f + 0.04f * mods.mark) / (1f + 0.04f * oldMark);
            mods.auraRadius *= (1f + 0.05f * mods.anchor) / (1f + 0.05f * oldAnchor);
            mods.radius *= 1f + 0.05f * Math.Max(0, abilityRange);
            mods.auraRadius *= 1f + 0.05f * Math.Max(0, abilityRange);
            if (projectile) mods.pierce += mods.breach - oldBreach + mods.phase - oldPhase;
            else { mods.pierce = 0f; mods.multishot = 0f; mods.ricochet = 0f; }
            mods.splitChance = Math.Min(0.45f, 0.18f * mods.split);
            mods.echoChance = Math.Min(0.35f, 0.12f * mods.echo);
            mods.staticChance = Math.Min(0.35f, 0.12f * mods.@static);
            mods.resonantCharge = resonantScale * mods.resonant;
            mods.healOnHit = 0.006f * mods.vampiric;
            mods.healOnKill = 0.03f * mods.drain;
            mods.knockback = mods.impact;
            return mods;
        }
    }
}
