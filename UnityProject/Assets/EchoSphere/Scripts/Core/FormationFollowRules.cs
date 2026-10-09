using System.Collections.Generic;

namespace EchoSphere.Core
{
    /// <summary>Pure Formation Follow contract port; 100 logical pixels equal one Unity unit.</summary>
    public static class FormationFollowRules
    {
        public const float ActivateRadius = 1.6f;
        public const float StrainMax = 100f;
        public const float StrainBuildPerSecond = 0.80f;
        public const float StrainMovingRecoveryPerSecond = 0.55f;
        public const float StrainIdleRecoveryPerSecond = 8f;
        public const float MaxSteeringPenalty = 0.18f;
        public const float MaxStrainSpeedPenalty = 0.14f;
        public const float MinMovementMultiplier = 0.72f;
        public const float MinNetworkEfficiency = 0.72f;
        public const float MinResonanceEfficiency = 0.65f;
        private const float Epsilon = 0.0001f;

        public static bool TryGetCentroid(IReadOnlyList<FormationNode> nodes, out Vec2 centroid)
        {
            var sum = Vec2.Zero;
            var count = 0;
            if (nodes != null)
                for (var i = 0; i < nodes.Count; i++)
                    if (nodes[i].IsActive) { sum += nodes[i].Position; count++; }
            centroid = count == 0 ? Vec2.Zero : sum / count;
            return count > 0;
        }
        public static bool CanActivate(Vec2 player, IReadOnlyList<FormationNode> nodes)
            => TryGetCentroid(nodes, out var centroid) && Vec2.Distance(player, centroid) <= ActivateRadius;
        public static float GetNetworkEfficiency(bool active, float strain)
            => !active ? 1f : 1f - Clamp01(strain / StrainMax) * (1f - MinNetworkEfficiency);
        public static float GetResonanceEfficiency(bool active, float strain)
            => !active ? 1f : 1f - Clamp01(strain / StrainMax) * (1f - MinResonanceEfficiency);
        public static float GetMovementMultiplier(bool active, float strain, Vec2 previous, Vec2 requested)
        {
            if (!active) return 1f;
            var current = Normalize(requested);
            if (current.LengthSquared <= Epsilon * Epsilon) return 1f;
            var last = Normalize(previous);
            var turn = last.LengthSquared <= Epsilon * Epsilon ? 0f : Clamp01((1f - Vec2.Dot(last, current)) / 2f);
            var fatigue = Clamp01(strain / StrainMax);
            var value = 1f - MaxSteeringPenalty * turn - MaxStrainSpeedPenalty * fatigue;
            return value < MinMovementMultiplier ? MinMovementMultiplier : value;
        }
        public static float UpdateStrain(float currentStrain, Vec2 previous, Vec2 requested, float dt, out Vec2 nextDirection)
        {
            var strain = IsFinite(currentStrain) ? Clamp(currentStrain, 0f, StrainMax) : 0f;
            dt = IsFinite(dt) ? (dt < 0f ? 0f : dt) : 0f;
            var current = Normalize(requested);
            if (current.LengthSquared <= Epsilon * Epsilon)
            {
                nextDirection = Vec2.Zero;
                return Clamp(strain - StrainIdleRecoveryPerSecond * dt, 0f, StrainMax);
            }
            var last = Normalize(previous);
            var turn = last.LengthSquared <= Epsilon * Epsilon ? 0f : Clamp01((1f - Vec2.Dot(last, current)) / 2f);
            var continuous = Max(0f, StrainBuildPerSecond - StrainMovingRecoveryPerSecond) * dt;
            nextDirection = current;
            return Clamp(strain + continuous + turn * 14f, 0f, StrainMax);
        }
        public static Vec2[] CaptureOffsets(Vec2 core, IReadOnlyList<Vec2> positions)
        {
            if (positions == null) return new Vec2[0];
            var offsets = new Vec2[positions.Count];
            for (var i = 0; i < positions.Count; i++) offsets[i] = positions[i] - core;
            return offsets;
        }
        public static Vec2[] ApplyOffsets(Vec2 core, IReadOnlyList<Vec2> offsets)
        {
            if (offsets == null) return new Vec2[0];
            var positions = new Vec2[offsets.Count];
            for (var i = 0; i < offsets.Count; i++) positions[i] = core + offsets[i];
            return positions;
        }
        private static Vec2 Normalize(Vec2 v) => v.Length <= Epsilon ? Vec2.Zero : v / v.Length;
        private static bool IsFinite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
        private static float Clamp01(float v) => Clamp(v, 0f, 1f);
        private static float Clamp(float v, float min, float max) => v < min ? min : v > max ? max : v;
        private static float Max(float a, float b) => a > b ? a : b;
    }
}
