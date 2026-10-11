namespace EchoSphere.Core
{
    /// <summary>Source Region availability from new-desing/src/region.ts:isRegionAvailable.</summary>
    public static class RegionUnlockRules
    {
        public static bool IsAvailable(EchoRegionId region, bool resonanceBasinStabilized)
        {
            switch (region)
            {
                case EchoRegionId.ResonanceBasin: return true;
                case EchoRegionId.SpectralRift: return resonanceBasinStabilized;
                default: return false;
            }
        }
    }
}
