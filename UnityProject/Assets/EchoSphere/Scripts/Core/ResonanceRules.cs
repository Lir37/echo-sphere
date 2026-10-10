using System;

namespace EchoSphere.Core
{
    /// <summary>Run-level Resonance contract ported from new-desing/src/resonance.ts.</summary>
    public static class ResonanceRules
    {
        public const float BaseCap = 100f;
        public const float OverflowCap = 150f;
        public const float SphereHitCharge = 1f;
        public const float GeometryCharge = 5f;
        public const float NetworkCharge = 5f;
        public const float RuneCharge = 60f;

        public static float Clamp(float value, bool allowOverflow = false)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 0f;
            return Math.Max(0f, Math.Min(allowOverflow ? OverflowCap : BaseCap, value));
        }

        public static int Add(ref float charge, float amount, bool allowOverflow = false)
        {
            charge = Clamp(charge, allowOverflow);
            if (float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f) return 0;
            var total = (double)charge + amount;
            var events = (int)Math.Floor(total / BaseCap);
            charge = Clamp((float)(total - events * BaseCap), allowOverflow);
            return events;
        }
    }
}
