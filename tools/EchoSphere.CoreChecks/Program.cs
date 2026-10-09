using EchoSphere.Core;

internal static class Program
{
    private static int _assertions;

    private static int Main()
    {
        try
        {
            CheckRng();
            CheckCombatRules();
            CheckFormationFollow();
            CheckCatalogAndRewardedContracts();
            Console.WriteLine($"ECHO SPHERE Unity core contracts: PASS ({_assertions} assertions)");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine("ECHO SPHERE Unity core contracts: FAIL");
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static void CheckRng()
    {
        var rng = new SeededRng(1u);
        var expected = new[] { 0.62707394f, 0.0027357212f, 0.52744704f, 0.98105097f, 0.9683779f };
        foreach (var value in expected) Near(rng.NextFloat(), value, 0.000001f, "TypeScript RNG reference sequence");
        var first = new SeededRng(92731u);
        var second = new SeededRng(92731u);
        for (var i = 0; i < 64; i++) Near(first.NextFloat(), second.NextFloat(), 0f, "same seed must reproduce sequence");
        Equal(new SeededRng(1u).NextInt(0), 0, "empty integer range returns zero");
    }

    private static void CheckCombatRules()
    {
        Near(CombatRules.CritBase, 0.05f, 0f, "base crit chance");
        Near(CombatRules.CritMultiplierBase, 1.5f, 0f, "base crit multiplier");
        Near(CombatRules.GetContextualCritChance(0.70f, true, true), 0.75f, 0.000001f, "crit hard cap");
        Near(CombatRules.ClampCritChance(float.NaN), 0f, 0f, "NaN crit normalization");
        True(!CombatRules.CanReceivePlayerDamage(0f, 0.6f), "contact grace blocks repeated contact");
        True(!CombatRules.CanReceivePlayerDamage(0.2f, 0f), "dash invulnerability blocks damage");
        True(CombatRules.CanReceivePlayerDamage(0f, 0f), "damage allowed without protection");
        True(!CombatRules.CanReceivePlayerDotDamage(0.1f), "invulnerability blocks DoT");
        var critical = CombatRules.ResolveDamage(10f, 0.75f, 0.5f, 1.5f);
        Near(critical.Damage, 15f, 0.000001f, "critical damage amount");
        True(critical.WasCritical, "critical result flag");
        var normal = CombatRules.ResolveDamage(10f, 0.05f, 0.9f, 1.5f);
        Near(normal.Damage, 10f, 0f, "normal damage amount");
    }

    private static void CheckFormationFollow()
    {
        var nodes = new[]
        {
            new FormationNode(new Vec2(1f, 0f), true),
            new FormationNode(new Vec2(3f, 0f), true, 0.4f),
            new FormationNode(new Vec2(99f, 99f), false)
        };
        True(FormationFollowRules.TryGetCentroid(nodes, out var centroid), "active centroid exists");
        True(centroid.Equals(new Vec2(1f, 0f)), "disabled/dead nodes excluded from centroid");
        True(FormationFollowRules.CanActivate(new Vec2(1f, 0f), new[] { new FormationNode(new Vec2(2.5f, 0f), true) }), "activation inside 1.6 world units");
        True(!FormationFollowRules.CanActivate(new Vec2(0f, 0f), new[] { new FormationNode(new Vec2(1.61f, 0f), true) }), "activation outside 1.6 world units denied");
        Near(FormationFollowRules.GetNetworkEfficiency(true, 100f), 0.72f, 0.000001f, "network strain floor");
        Near(FormationFollowRules.GetResonanceEfficiency(true, 100f), 0.65f, 0.000001f, "resonance strain floor");
        Near(FormationFollowRules.GetMovementMultiplier(true, 100f, new Vec2(1f, 0f), new Vec2(0f, 1f)), 0.72f, 0.000001f, "movement multiplier floor");
        var updated = FormationFollowRules.UpdateStrain(10f, new Vec2(1f, 0f), new Vec2(0f, 1f), 0.1f, out var direction);
        Near(updated, 17.025f, 0.0001f, "turn strain plus continuous strain");
        True(direction.Equals(new Vec2(0f, 1f)), "requested direction stored normalized");
        Near(FormationFollowRules.UpdateStrain(20f, new Vec2(1f, 0f), Vec2.Zero, 1f, out direction), 12f, 0f, "idle strain recovery");
        True(direction.Equals(Vec2.Zero), "idle resets direction");
        var offsets = FormationFollowRules.CaptureOffsets(Vec2.Zero, new[] { new Vec2(1f, 2f), new Vec2(-1f, 0f) });
        var positions = FormationFollowRules.ApplyOffsets(new Vec2(10f, 5f), offsets);
        True(positions[0].Equals(new Vec2(11f, 7f)) && positions[1].Equals(new Vec2(9f, 5f)), "formation offsets preserve positions");
    }

    private static void CheckCatalogAndRewardedContracts()
    {
        Equal(GameCatalog.Spheres.Count, 10, "canonical Sphere roster size");
        var seen = new HashSet<SphereId>();
        foreach (var sphere in GameCatalog.Spheres)
        {
            True(!string.IsNullOrWhiteSpace(sphere.DisplayName), "Sphere display name exists");
            True(seen.Add(sphere.Id), "Sphere ids are unique");
        }
        True(RewardedAdsRules.CanOfferRevive(true, false), "revive available on eligible death");
        True(!RewardedAdsRules.CanOfferRevive(true, true), "one revive per run");
        True(!RewardedAdsRules.CanOfferRevive(false, false), "revive not offered while alive");
        True(RewardedAdsRules.CanOfferDoubleRewards(true, false), "double reward offered after complete run");
        True(!RewardedAdsRules.CanOfferDoubleRewards(true, true), "double reward cannot be claimed twice");
        True(RewardedAdsRules.ShouldGrantReward(RewardedShowResult.RewardEarned), "reward only on confirmed completion");
        True(!RewardedAdsRules.ShouldGrantReward(RewardedShowResult.ClosedWithoutReward), "early close grants no reward");
        Equal(RewardedAdsRules.GetDoubleRewardTotal(120), 240, "double reward calculation");
        Equal(RewardedAdsRules.GetDoubleRewardTotal(-5), 0, "negative reward normalized");
    }

    private static void True(bool condition, string name)
    {
        _assertions++;
        if (!condition) throw new InvalidOperationException(name);
    }

    private static void Equal<T>(T actual, T expected, string name)
    {
        _assertions++;
        if (!EqualityComparer<T>.Default.Equals(actual, expected))
            throw new InvalidOperationException($"{name}: expected {expected}, got {actual}");
    }

    private static void Near(float actual, float expected, float epsilon, string name)
    {
        _assertions++;
        if (Math.Abs(actual - expected) > epsilon)
            throw new InvalidOperationException($"{name}: expected {expected}, got {actual}");
    }
}
