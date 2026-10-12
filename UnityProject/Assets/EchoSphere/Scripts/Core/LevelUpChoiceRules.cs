namespace EchoSphere.Core
{
    /// <summary>
    /// Source-mapped Lock/Reroll eligibility and Oracle Forecast mastery gates.
    /// Actual choice generation remains in EchoSphereRuntime.
    /// </summary>
    public static class LevelUpChoiceRules
    {
        public static int GetOracleForecastCount(int masteryLevel) =>
            masteryLevel >= 4 ? 2 : 1;

        public static int GetOracleRerollPreservationCount(int masteryLevel) =>
            masteryLevel >= 9 ? 2 : 1;

        public static bool CanLockChoice(int choiceCount, int locksRemaining, bool hasReservedChoice) =>
            choiceCount == 3 && locksRemaining > 0 && !hasReservedChoice;

        public static bool CanRerollChoices(int choiceCount, int rerollsRemaining) =>
            choiceCount == 3 && rerollsRemaining > 0;

        public static bool ShouldPreserveOracleForecast(
            bool isOracle, int masteryLevel, bool rerollUsed, bool hasLockedChoice, bool hasForecastedChoices) =>
            isOracle && masteryLevel >= 6 && !rerollUsed && !hasLockedChoice && hasForecastedChoices;

        public static bool ShouldShowOracleSourceFlash(int masteryLevel, bool hasChoices) =>
            masteryLevel >= 7 && hasChoices;
    }
}
