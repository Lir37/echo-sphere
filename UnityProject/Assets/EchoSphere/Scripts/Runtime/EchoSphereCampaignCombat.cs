namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        // Null denotes pre-campaign saved runs; do not infer policy from a selected region.
        public bool UsesCampaignCombat => _runEconomy != null;
        public float CampaignEnemyHealthMultiplier => UsesCampaignCombat && _tutorialStep < 0 ? EchoSphere.Core.RegionalCombatTuning.HealthTimeMultiplier(_runTime) : 1f;
        public float CampaignEnemyDamageMultiplier => UsesCampaignCombat && _tutorialStep < 0 ? EchoSphere.Core.RegionalCombatTuning.DamageTimeMultiplier(_runTime) : 1f;
        internal void NotifyCampaignBossPhase() => ShowTimedCombatMessage(IsRussianUi?"ФАЗА II":"PHASE II",1.5f);
    }
}
