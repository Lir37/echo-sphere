namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        // Null denotes pre-campaign saved runs; do not infer policy from a selected region.
        public bool UsesCampaignCombat => _runEconomy != null;
        internal void NotifyCampaignBossPhase() => ShowTimedCombatMessage(IsRussianUi?"ФАЗА II":"PHASE II",1.5f);
    }
}
