namespace EchoSphere.Core
{
    /// <summary>
    /// Source-mapped Artifact Protocol combat multipliers from
    /// src/artifactSystem.ts:getSphereArtifactDamageMultiplier().
    /// The prerequisites are passed from source-equivalent set/build state.
    /// </summary>
    public static class ArtifactProtocolRules
    {
        public static float GetTriangleResonanceMultiplier(bool completedGrid, bool sphereFormsTriangle) =>
            completedGrid && sphereFormsTriangle ? 1.05f : 1f;

        public static float GetSphereRelayMultiplier(
            bool completedEchoArchitecture, int uniqueTypeCount, bool hasNearbySphere) =>
            completedEchoArchitecture && uniqueTypeCount >= 2 && hasNearbySphere ? 1.05f : 1f;

        public static float GetGeometryCircuitMultiplier(
            bool completedGeometryCraft, SphereNetworkFormation geometryType) =>
            completedGeometryCraft && ArtifactSetRules.IsRingLatticeOrFractal(geometryType) ? 1.05f : 1f;

        public static float GetVoidHorizonProtocolMultiplier(
            bool completedVoidHorizon, bool isVoidSphere, bool hasLivingEliteOrBoss) =>
            completedVoidHorizon && isVoidSphere && hasLivingEliteOrBoss ? 1.08f : 1f;

        public static float GetHunterDoctrineProtocolMultiplier(
            bool completedHunterDoctrine, bool isSniperSphere, bool hasLivingEliteOrBoss) =>
            completedHunterDoctrine && isSniperSphere && hasLivingEliteOrBoss ? 1.08f : 1f;

        public static float GetPulseEngineeringProtocolMultiplier(
            bool completedPulseEngineering, bool isPulseSphere, int livingEnemyCount) =>
            completedPulseEngineering && isPulseSphere && livingEnemyCount >= 4 ? 1.08f : 1f;

        public static float GetCoreForgeProtocolMultiplier(bool completedCoreForge, int uniqueTypeCount) =>
            completedCoreForge && uniqueTypeCount >= 3 ? 1.05f : 1f;

        public static float GetPrismDominionProtocolMultiplier(
            bool completedPrismDominion, bool isPrismOrbitalOrVoid, bool refractionPressure) =>
            completedPrismDominion && isPrismOrbitalOrVoid && refractionPressure ? 1.06f : 1f;

        public static float GetNetworkLegacyProtocolMultiplier(
            bool completedNetworkLegacy, int uniqueTypeCount, SphereNetworkFormation geometryType) =>
            completedNetworkLegacy && uniqueTypeCount >= 3 &&
            ArtifactSetRules.IsRingLatticeOrFractal(geometryType) ? 1.06f : 1f;
    }
}
