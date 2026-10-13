namespace EchoSphere.Core
{
    /// <summary>
    /// Source-mapped completion gates and bonuses from src/artifactSystem.ts.
    /// Completion booleans represent all required source pair synergies being active.
    /// Temporal Fold and Status Circuit remain unhooked until their runtime-state
    /// prerequisites can be proven equivalent to the source.
    /// </summary>
    public static class ArtifactSetRules
    {
        public static bool HasCompletedResonanceGrid(bool fortressNetwork, bool perfectNetwork) =>
            fortressNetwork && perfectNetwork;

        public static bool HasCompletedSingularityPath(bool glassCannon, bool singularity) =>
            glassCannon && singularity;

        public static bool HasCompletedGeometryCraft(
            bool geometryLoop, bool latticeMemory, bool networkMemory) =>
            geometryLoop && latticeMemory && networkMemory;

        public static bool HasCompletedVoidHorizonSet(
            bool voidHorizon, bool orbitalPrism, bool gravityPulse) =>
            voidHorizon && orbitalPrism && gravityPulse;

        public static bool HasCompletedHunterDoctrineSet(bool hunterDoctrine, bool hunterFocus) =>
            hunterDoctrine && hunterFocus;

        public static bool HasCompletedPulseEngineeringSet(bool pulseEngineering, bool pulseOverload) =>
            pulseEngineering && pulseOverload;

        public static bool HasCompletedCoreForgeSet(bool coreForge, bool coreRelay) =>
            coreForge && coreRelay;

        public static bool HasCompletedPrismDominionSet(bool orbitalPrism, bool statusCircuit) =>
            orbitalPrism && statusCircuit;

        public static bool HasCompletedNetworkLegacySet(bool networkMemory, bool perfectNetwork) =>
            networkMemory && perfectNetwork;

        public static bool IsRingLatticeOrFractal(SphereNetworkFormation geometryType) =>
            geometryType == SphereNetworkFormation.Ring ||
            geometryType == SphereNetworkFormation.Lattice ||
            geometryType == SphereNetworkFormation.Fractal;

        public static float GetResonanceGridDamageMultiplier(
            bool complete, bool hasNearbySphere, bool hasActiveGeometry) =>
            complete && hasNearbySphere && hasActiveGeometry ? 1.06f : 1f;

        public static float GetSingularityPathDamageMultiplier(bool complete, int uniqueTypeCount) =>
            complete && uniqueTypeCount <= 1 ? 1.08f : 1f;

        public static float GetGeometryCraftDamageMultiplier(
            bool complete, SphereNetworkFormation geometryType) =>
            complete && IsRingLatticeOrFractal(geometryType) ? 1.10f : 1f;

        public static float GetVoidHorizonSetDamageMultiplier(bool complete, bool isVoidSphere) =>
            complete && isVoidSphere ? 1.10f : 1f;

        public static float GetHunterDoctrineSetDamageMultiplier(bool complete, bool isSniperSphere) =>
            complete && isSniperSphere ? 1.10f : 1f;

        public static float GetPulseEngineeringSetDamageMultiplier(bool complete, bool isPulseSphere) =>
            complete && isPulseSphere ? 1.10f : 1f;

        public static float GetCoreForgeSetDamageMultiplier(bool complete, int uniqueTypeCount) =>
            complete && uniqueTypeCount >= 3 ? 1.06f : 1f;

        public static float GetPrismDominionSetDamageMultiplier(bool complete, bool isPrismOrOrbital) =>
            complete && isPrismOrOrbital ? 1.08f : 1f;

        public static float GetNetworkLegacySetDamageMultiplier(
            bool complete, int uniqueTypeCount, SphereNetworkFormation geometryType) =>
            complete && uniqueTypeCount >= 3 && IsRingLatticeOrFractal(geometryType) ? 1.06f : 1f;
    }
}
