namespace EchoSphere.Core
{
    public enum RewardedPlacement { Revive, DoubleRunRewards }
    public enum RewardedShowResult { RewardEarned, ClosedWithoutReward, NotReady, Failed }

    /// <summary>Provider-neutral contracts only. No ad SDK or credentials are included.</summary>
    public static class RewardedAdsRules
    {
        public static bool CanOfferRevive(bool runIsOver, bool reviveAlreadyUsed) => runIsOver && !reviveAlreadyUsed;
        public static bool CanOfferDoubleRewards(bool runIsComplete, bool doubleRewardAlreadyClaimed) => runIsComplete && !doubleRewardAlreadyClaimed;
        public static bool ShouldGrantReward(RewardedShowResult result) => result == RewardedShowResult.RewardEarned;
        public static int GetDoubleRewardTotal(int baseReward)
        {
            if (baseReward <= 0) return 0;
            return baseReward > int.MaxValue / 2 ? int.MaxValue : baseReward * 2;
        }
    }
}
