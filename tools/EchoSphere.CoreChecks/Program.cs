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
            CheckRunBalance();
            CheckSphereCombatProfiles();
            CheckSphereProgressionRules();
            CheckSphereEvolutionCatalog();
            CheckSphereLevelRules();
            CheckSphereEvolutionCombatRules();
            CheckSniperEvolutionCombatRules();
            CheckShotgunEvolutionCombatRules();
            CheckChainEvolutionCombatRules();
            CheckAuraEvolutionCombatRules();
            CheckGravityEvolutionCombatRules();
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
        Near(FormationFollowRules.GetMovementMultiplier(true, 100f, new Vec2(1f, 0f), new Vec2(0f, 1f)), 0.77f, 0.000001f, "quarter-turn movement multiplier");
        Near(FormationFollowRules.GetMovementMultiplier(true, 100f, new Vec2(1f, 0f), new Vec2(-1f, 0f)), 0.72f, 0.000001f, "movement multiplier floor on reversal");
        var updated = FormationFollowRules.UpdateStrain(10f, new Vec2(1f, 0f), new Vec2(0f, 1f), 0.1f, out var direction);
        Near(updated, 17.025f, 0.0001f, "turn strain plus continuous strain");
        True(direction.Equals(new Vec2(0f, 1f)), "requested direction stored normalized");
        Near(FormationFollowRules.UpdateStrain(20f, new Vec2(1f, 0f), Vec2.Zero, 1f, out direction), 12f, 0f, "idle strain recovery");
        True(direction.Equals(Vec2.Zero), "idle resets direction");
        var offsets = FormationFollowRules.CaptureOffsets(Vec2.Zero, new[] { new Vec2(1f, 2f), new Vec2(-1f, 0f) });
        var positions = FormationFollowRules.ApplyOffsets(new Vec2(10f, 5f), offsets);
        True(positions[0].Equals(new Vec2(11f, 7f)) && positions[1].Equals(new Vec2(9f, 5f)), "formation offsets preserve positions");
    }

    private static void CheckRunBalance()
    {
        Equal(RunBalanceRules.GetXpToNextLevel(1), 10, "new-desing first level XP threshold");
        Equal(RunBalanceRules.GetXpToNextLevel(2), 13, "new-desing level 2 XP threshold");
        Equal(RunBalanceRules.GetXpToNextLevel(3), 16, "new-desing level 3 XP threshold uses JS rounding");
        Equal(RunBalanceRules.GetXpToNextLevel(10), 43, "new-desing level 10 XP threshold");
        Equal(RunBalanceRules.GetXpToNextLevel(0), 10, "invalid low level normalized to first threshold");
        Equal(RunBalanceRules.GetXpToNextLevel(-50), 10, "negative level normalized to first threshold");
    }

    private static void CheckSphereCombatProfiles()
    {
        Equal(SphereCombatRules.GetProfile(SphereId.Standard).DamageMultiplier, 1f, "Standard Sphere damage multiplier");
        Equal(SphereCombatRules.GetProfile(SphereId.Sniper).DamageMultiplier, 2.5f, "Sniper damage multiplier");
        Equal(SphereCombatRules.GetProfile(SphereId.Shotgun).Pellets, 3, "Shotgun pellet count");
        Near(SphereCombatRules.GetProfile(SphereId.Chain).DelayMultiplier, 1.3f, 0f, "Chain delay multiplier");
        True(SphereCombatRules.GetProfile(SphereId.Aura).Aura, "Aura uses area damage");
        Near(SphereCombatRules.GetProfile(SphereId.Orbital).DamageMultiplier, 0.72f, 0f, "Orbital contact damage multiplier");
        Equal(SphereCombatRules.GetProfile(SphereId.Prism).Pellets, 3, "Prism split target count");
        True(SphereCombatRules.GetProfile(SphereId.Gravity).Aura, "Gravity uses pulse area");
        Near(SphereCombatRules.GetProfile(SphereId.Pulse).RangeMultiplier, 1.15f, 0f, "Pulse range multiplier");
        Near(SphereCombatRules.GetProfile(SphereId.Void).DamageMultiplier, 1.15f, 0f, "Void base damage multiplier");
    }

    private static void CheckSphereProgressionRules()
    {
        var levels = new Dictionary<SphereId, int> { [SphereId.Standard] = 1 };
        Equal(SphereProgressionRules.GetLevel(levels, SphereId.Standard), 1, "Standard starts at level I");
        True(SphereProgressionRules.IsUnlocked(levels, SphereId.Standard), "Standard is initially unlocked");
        True(!SphereProgressionRules.IsUnlocked(levels, SphereId.Sniper), "Sniper is initially locked");
        Equal(SphereProgressionRules.GetNextLevel(levels, SphereId.Standard), 2, "known Sphere advances one level");
        Equal(SphereProgressionRules.GetNextLevel(levels, SphereId.Sniper), 1, "new Sphere starts at level I");
        True(SphereProgressionRules.CanUpgrade(levels, SphereId.Void), "Void can be acquired");
        levels[SphereId.Void] = 7;
        True(!SphereProgressionRules.CanUpgrade(levels, SphereId.Void), "level VII is the Sphere cap");
        Equal(SphereProgressionRules.GetNextLevel(levels, SphereId.Void), 7, "capped Sphere cannot advance past VII");
        var unlock = SphereProgressionRules.CreateSphereChoice(levels, SphereId.Sniper);
        Equal(unlock.CurrentLevel, 0, "new Sphere choice records locked state");
        Equal(unlock.NextLevel, 1, "new Sphere choice grants level I");
        Equal(unlock.Kind, RunUpgradeKind.Sphere, "acquisition is a Sphere upgrade");
    }

    private static void CheckSphereEvolutionCatalog()
    {
        Equal(SphereEvolutionCatalog.GetBranches(SphereId.Standard).Count, 3, "Standard has three mutation branches");
        Equal(SphereEvolutionCatalog.GetBranches(SphereId.Sniper).Count, 3, "Sniper has three mutation branches");
        Equal(SphereEvolutionCatalog.GetBranches(SphereId.Gravity).Count, 3, "Gravity has three mutation branches");
        var branches = SphereEvolutionCatalog.GetBranches(SphereId.Standard);
        Equal(branches[0].Id, "standard_resonator", "source branch id preserved");
        Equal(SphereEvolutionCatalog.GetFinals(branches[0].Id).Count, 3, "each branch has three final variants");
        Equal(SphereEvolutionCatalog.GetFinals("void_execution").Count, 3, "Void execution has three final variants");
        Equal(SphereEvolutionCatalog.GetFinals("unknown").Count, 0, "unknown evolution branch safely returns no choices");
    }

    private static void CheckSphereLevelRules()
    {
        var standard1 = SphereLevelRules.GetStats(SphereId.Standard, 1);
        Near(standard1.DamageMultiplier, 1.15f, 0.00001f, "Standard I +15% damage");
        Equal(standard1.Pierce, 0, "Standard I has no extra pierce");
        var standard2 = SphereLevelRules.GetStats(SphereId.Standard, 2);
        Equal(standard2.Pierce, 1, "Standard II +1 pierce");
        Near(SphereLevelRules.GetStats(SphereId.Standard, 3).DelayMultiplier, 0.90f, 0.00001f, "Standard III -10% delay");

        var sniper = SphereLevelRules.GetStats(SphereId.Sniper, 3);
        Near(sniper.DamageMultiplier, 1.25f, 0.00001f, "Sniper I +25% damage persists");
        Near(sniper.RangeMultiplier, 1.15f, 0.00001f, "Sniper II +15% range");
        Near(sniper.CritChanceBonus, 0.15f, 0.00001f, "Sniper III +15% crit");

        Equal(SphereLevelRules.GetStats(SphereId.Shotgun, 1).Pellets, 4, "Shotgun I +1 pellet");
        Near(SphereLevelRules.GetStats(SphereId.Shotgun, 2).CloseRangeDamageMultiplier, 1.20f, 0.00001f, "Shotgun II close damage");
        Near(SphereLevelRules.GetStats(SphereId.Shotgun, 3).SpreadMultiplier, 0.88f, 0.00001f, "Shotgun III tighter spread");
        Equal(SphereLevelRules.GetStats(SphereId.Chain, 1).ChainTargets, 4, "Chain I +1 target");
        Near(SphereLevelRules.GetStats(SphereId.Chain, 2).DamageMultiplier, 1.10f, 0.00001f, "Chain II damage");
        Near(SphereLevelRules.GetStats(SphereId.Chain, 3).DelayMultiplier, 0.85f, 0.00001f, "Chain III interval");
        Near(SphereLevelRules.GetStats(SphereId.Aura, 1).AuraRadiusMultiplier, 1.20f, 0.00001f, "Aura I radius");
        Near(SphereLevelRules.GetStats(SphereId.Orbital, 3).OrbitSpeedMultiplier, 1.15f, 0.00001f, "Orbital III rotation speed");
        Equal(SphereLevelRules.GetStats(SphereId.Prism, 3).PrismDirections, 2, "Prism III +1 direction");
        Near(SphereLevelRules.GetStats(SphereId.Gravity, 1).PullMultiplier, 1.20f, 0.00001f, "Gravity I pull");
        Near(SphereLevelRules.GetStats(SphereId.Pulse, 3).DelayMultiplier, 0.88f, 0.00001f, "Pulse III interval");
        Near(SphereLevelRules.GetStats(SphereId.Void, 1).WeakenedDamageMultiplier, 1.20f, 0.00001f, "Void I weakened damage");
        Near(SphereLevelRules.GetStats(SphereId.Void, 2).CritChanceBonus, 0.10f, 0.00001f, "Void II execution crit chance");
        Near(SphereLevelRules.GetStats(SphereId.Void, 3).RangeMultiplier, 1.15f, 0.00001f, "Void III range");
        Near(SphereLevelRules.GetStats(SphereId.Standard, 99).DamageMultiplier, 1.15f, 0.00001f, "level input clamps to VII");
    }


    private static void CheckSphereEvolutionCombatRules()
    {
        True(!SphereEvolutionCombatRules.ShouldTriggerStandardResonatorPulse(2), "Resonator does not pulse before third hit");
        True(SphereEvolutionCombatRules.ShouldTriggerStandardResonatorPulse(3), "Resonator pulses every third hit");
        Near(SphereEvolutionCombatRules.GetStandardResonatorPulseDamageMultiplier(7, "standard_resonator_final_1"), 0.65f, 0f, "Resonator Pulse final damage");
        Near(SphereEvolutionCombatRules.GetStandardResonatorPulseRadius(7, "standard_resonator_final_2"), 1f, 0f, "Resonator Rebound radius");
        Near(SphereEvolutionCombatRules.GetStandardSingularityPullDistance(6), 3.6f, 0f, "Singularity level VI pull");
        Near(SphereEvolutionCombatRules.GetStandardSingularitySlowDuration(6), 1.2f, 0f, "Singularity level VI slow");
        Equal(SphereEvolutionCombatRules.GetStandardSwarmShardCount(6, null), 2, "Swarm level VI launches two shards");
        Equal(SphereEvolutionCombatRules.GetStandardSwarmShardCount(7, "standard_swarm_final_3"), 3, "Swarm final shard web launches three shards");
        Near(SphereEvolutionCombatRules.GetStandardSwarmShardDamageMultiplier(5), 0.45f, 0f, "Swarm level V shard damage");
    }

    private static void CheckSniperEvolutionCombatRules()
    {
        Near(SphereEvolutionCombatRules.GetSniperOracleCriticalMultiplier("sniper_oracle_final_1"), 1.50f, 0f, "Oracle critical focus bonus");
        Near(SphereEvolutionCombatRules.GetSniperOracleCriticalMultiplier("sniper_oracle_final_2"), 1.30f, 0f, "Oracle resonance sight critical bonus");
        Near(SphereEvolutionCombatRules.GetSniperOracleMarkDuration("sniper_oracle_final_2"), 3.5f, 0f, "Oracle mark duration");
        Near(SphereEvolutionCombatRules.GetSniperAssassinThreshold("sniper_assassin_final_1", 5), 0.45f, 0f, "Assassin execution mark threshold");
        Near(SphereEvolutionCombatRules.GetSniperAssassinDamageMultiplier(5, null, 0.25f), 1.5f, 0f, "Assassin level V weakened target damage");
        Near(SphereEvolutionCombatRules.GetSniperAssassinDamageMultiplier(6, null, 0.34f), 1.8f, 0f, "Assassin level VI execution damage");
        Near(SphereEvolutionCombatRules.GetSniperAssassinDamageMultiplier(7, "sniper_assassin_final_2", 0.19f), 2f, 0f, "Assassin Sentence execution multiplier");
        Near(SphereEvolutionCombatRules.GetSniperBeaconMarkDuration(6, "sniper_beacon_final_2"), 5f, 0f, "Beacon Spotlight mark duration");
        True(SphereEvolutionCombatRules.GetSniperBeaconRadius(6, "sniper_beacon_final_2") > SphereEvolutionCombatRules.GetSniperBeaconRadius(5, null), "Beacon Spotlight expands mark area");
        Near(SphereEvolutionCombatRules.GetSniperBeaconSlowMultiplier("sniper_beacon_final_1"), 0.45f, 0f, "Beacon Anchor slow");
    }

    private static void CheckShotgunEvolutionCombatRules()
    {
        Near(SphereEvolutionCombatRules.GetShotgunCataclysmRadius(5, "shotgun_cataclysm_final_1"), 1.2f, 0f, "Cataclysm Fragment Blast radius");
        Near(SphereEvolutionCombatRules.GetShotgunCataclysmSplash(5, "shotgun_cataclysm_final_2"), 0.65f, 0f, "Cataclysm Heavy Blast splash");
        True(SphereEvolutionCombatRules.GetShotgunCataclysmRadius(6, null) > SphereEvolutionCombatRules.GetShotgunCataclysmRadius(5, null), "Cataclysm grows at level VI");
        Near(SphereEvolutionCombatRules.GetShotgunHailChance(4, null), 0.32f, 0f, "Hail base proc chance");
        Near(SphereEvolutionCombatRules.GetShotgunHailChance(5, null), 0.45f, 0f, "Hail level V proc chance");
        Near(SphereEvolutionCombatRules.GetShotgunHailChance(6, null), 0.60f, 0f, "Hail level VI proc chance");
        Equal(SphereEvolutionCombatRules.GetShotgunHailShardCount(6, null), 7, "Hail level VI shard count");
        Equal(SphereEvolutionCombatRules.GetShotgunHailShardCount(7, "shotgun_hail_final_2"), 8, "Dense Hail shard count");
        Near(SphereEvolutionCombatRules.GetShotgunHailShardDamageMultiplier("shotgun_hail_final_2"), 0.22f, 0f, "Dense Hail shard damage");
        Near(SphereEvolutionCombatRules.GetShotgunBurstDamageMultiplier(4, null, 1.4f), 1.08f, 0.00001f, "Burst base branch damage");
        Near(SphereEvolutionCombatRules.GetShotgunBurstDamageMultiplier(5, null, 1.6f), 1.08f * 1.15f, 0.00001f, "Burst level V strengthens widened close range");
        Near(SphereEvolutionCombatRules.GetShotgunBurstDamageMultiplier(6, null, 1.9f), 1.08f * 1.25f, 0.00001f, "Burst level VI further strengthens widened close range");
        Near(SphereEvolutionCombatRules.GetShotgunBurstDamageMultiplier(4, "shotgun_burst_final_1", 1.4f), 1.08f * 1.20f * 1.30f, 0.00001f, "Close Burst damage and branch bonuses");
        Near(SphereEvolutionCombatRules.GetShotgunBurstDamageMultiplier(4, "shotgun_burst_final_2", 1.7f), 1.08f * 1.50f, 0.00001f, "Siege Burst extended close range");
        Near(SphereEvolutionCombatRules.GetShotgunBurstDamageMultiplier(4, "shotgun_burst_final_3", 2.0f), 1.08f, 0.00001f, "Impact Burst bonus is gated by range");
        Equal(SphereEvolutionCombatRules.GetShotgunBurstExtraPellets(4, "shotgun_burst_final_1", 1.4f), 2, "Close Burst adds two pellets");
        Equal(SphereEvolutionCombatRules.GetShotgunBurstExtraPellets(4, "shotgun_burst_final_1", 1.6f), 0, "Close Burst extra pellets require range");
        True(SphereEvolutionCombatRules.ShouldShotgunBurstApplySlow(6, "shotgun_burst_final_3", 1.05f), "Impact Burst level VI slow zone widens");
        True(!SphereEvolutionCombatRules.ShouldShotgunBurstApplySlow(4, "shotgun_burst_final_3", 1.05f), "Impact Burst base slow zone remains short");
        Equal(SphereEvolutionCombatRules.GetShotgunCataclysmPierce(0, null), 2, "Cataclysm grants two piercing hits");
        Equal(SphereEvolutionCombatRules.GetShotgunCataclysmPierce(0, "shotgun_cataclysm_final_1"), 4, "Fragment Blast adds two more pierces");
        Equal(SphereEvolutionCombatRules.GetShotgunHailBonusPellets(null), 1, "Hail adds one pellet");
        Equal(SphereEvolutionCombatRules.GetShotgunHailBonusPellets("shotgun_hail_final_1"), 2, "Fragment Hail adds a second bonus pellet");
    }

    private static void CheckChainEvolutionCombatRules()
    {
        Near(SphereEvolutionCombatRules.GetChainWebSlowDuration(5, null), 0.805f, 0.00001f, "Chain Web level V slow duration");
        Near(SphereEvolutionCombatRules.GetChainWebSlowDuration(7, "chain_web_final_2"), 1.4f, 0f, "Static Web slow duration");
        Near(SphereEvolutionCombatRules.GetChainWebSlowMultiplier(6, null), 0.676f, 0.0001f, "Chain Web level VI slow strength");
        Near(SphereEvolutionCombatRules.GetChainStormRadius(7, "chain_storm_final_2"), 2f, 0f, "Resonant Storm radius");
        Near(SphereEvolutionCombatRules.GetChainStormSplash(7, "chain_storm_final_2"), 0.4f, 0f, "Resonant Storm splash");
        Near(SphereEvolutionCombatRules.GetChainLeechHealRatio(7, "chain_leech_final_2"), 0.045f, 0f, "Harvest healing ratio");
        True(SphereEvolutionCombatRules.GetChainLeechHealRatio(6, null) > SphereEvolutionCombatRules.GetChainLeechHealRatio(5, null), "Chain Leech improves at level VI");
    }

    private static void CheckAuraEvolutionCombatRules()
    {
        Near(SphereEvolutionCombatRules.GetAuraSanctumSlowDuration(null), 0.8f, 0f, "Sanctum baseline slow");
        Near(SphereEvolutionCombatRules.GetAuraSanctumSlowDuration("aura_sanctum_final_2"), 2f, 0f, "Locking Sanctum duration");
        Near(SphereEvolutionCombatRules.GetAuraSanctumSlowMultiplier("aura_sanctum_final_2"), 0.42f, 0f, "Locking Sanctum slow strength");
        Near(SphereEvolutionCombatRules.GetAuraGravityPullRadius(7, "aura_gravity_final_2"), 3.8f, 0f, "Aura Gravity Hold radius");
        Near(SphereEvolutionCombatRules.GetAuraGravityPullDistance(7, "aura_gravity_final_1"), 1.1f, 0f, "Aura Gravity Pull distance");
        Near(SphereEvolutionCombatRules.GetAuraOvergrowthRadius(7, "aura_overgrowth_final_2"), 3.6f, 0f, "Overgrowth Overflow radius");
        Near(SphereEvolutionCombatRules.GetAuraOvergrowthAttackTimerReduction(7, "aura_overgrowth_final_2"), 0.30f, 0f, "Overgrowth Overflow acceleration");
        True(SphereEvolutionCombatRules.GetAuraOvergrowthRadius(6, null) > SphereEvolutionCombatRules.GetAuraOvergrowthRadius(5, null), "Overgrowth level VI expands its buff zone");
    }

    private static void CheckGravityEvolutionCombatRules()
    {
        Near(SphereEvolutionCombatRules.GetGravityWellRadius(7, "gravity_well_final_2"), 3f, 0f, "Gravity Well Pull Boost radius");
        Near(SphereEvolutionCombatRules.GetGravityWellPullDistance(7, "gravity_well_final_2"), 0.50f, 0f, "Gravity Well Pull Boost strength");
        Near(SphereEvolutionCombatRules.GetGravityWellSlow("gravity_well_final_1").duration, 0.75f, 0f, "Gravity Well Anchor slow duration");
        Near(SphereEvolutionCombatRules.GetGravityWellSlow("gravity_well_final_1").multiplier, 0.55f, 0f, "Gravity Well Anchor slow strength");
        Near(SphereEvolutionCombatRules.GetGravityTideRadius("gravity_tide_final_2"), 3.4f, 0f, "Gravity Tide Draw radius");
        Near(SphereEvolutionCombatRules.GetGravityTideDistance(6), 0.72f, 0f, "Gravity Tide level VI impulse");
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
