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
            CheckEnemySpawnRules();
            CheckBossCombatRules();
            CheckRegionBossSchedule();
            CheckRegionUnlockRules();
            CheckPersistentUpgradeRules();
            CheckDifficultyRules();
            CheckArtifactRewardRules();
            CheckSphereCombatProfiles();
            CheckSphereProgressionRules();
            CheckLevelUpAbilityRules();
            CheckSphereEvolutionCatalog();
            CheckSphereLevelRules();
            CheckSphereEvolutionCombatRules();
            CheckSniperEvolutionCombatRules();
            CheckShotgunEvolutionCombatRules();
            CheckChainEvolutionCombatRules();
            CheckAuraEvolutionCombatRules();
            CheckGravityEvolutionCombatRules();
            CheckOrbitalEvolutionCombatRules();
            CheckPrismEvolutionCombatRules();
            CheckPulseVoidEvolutionCombatRules();
            CheckSphereNetworkRules();
            CheckSpherePlacementRules();
            CheckCharacterRules();
            CheckHunterCombatRules();
            CheckEngineerRelayRules();
            CheckConductorOverdriveRules();
            CheckLevelUpChoiceRules();
            CheckFormationHitRules();
            CheckArtifactSynergyRules();
            CheckSphereElementalRules();
            CheckAlchemistReactionRules();
            CheckSphereAuthoredModifierRules();
            CheckSphereAuthoredHitEffects();
            CheckAuthoredProjectileRuntime();
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
        Equal(RunBalanceRules.GetEnemiesPerWave(1), 5, "source first wave has five enemies");
        Equal(RunBalanceRules.GetEnemiesPerWave(2), 6, "source second wave has six enemies");
        Equal(RunBalanceRules.GetEnemiesPerWave(10), 15, "source wave count growth formula");
        Near(RunBalanceRules.GetWaveSpawnInterval(1), 1.18f, 0.0001f, "source wave 1 spawn interval");
        Near(RunBalanceRules.GetWaveSpawnInterval(45), 0.3f, 0.0001f, "source wave spawn interval lower bound");
    }

    private static void CheckEnemySpawnRules()
    {
        Equal(EnemySpawnRules.Select(0f, 0.99f), EnemyArchetype.Normal, "opening phase uses baseline enemy");
        Equal(EnemySpawnRules.Select(20f, 0.10f), EnemyArchetype.Fast, "fast enemy unlock");
        Equal(EnemySpawnRules.Select(30f, 0.30f), EnemyArchetype.Tank, "tank enemy unlock");
        Equal(EnemySpawnRules.Select(90f, 0.55f), EnemyArchetype.Elite, "elite enemy unlock");
        var normal = EnemySpawnRules.GetProfile(EnemyArchetype.Normal);
        var fast = EnemySpawnRules.GetProfile(EnemyArchetype.Fast);
        var tank = EnemySpawnRules.GetProfile(EnemyArchetype.Tank);
        var elite = EnemySpawnRules.GetProfile(EnemyArchetype.Elite);
        Near(normal.HpMultiplier, 1f, 0f, "normal baseline HP");
        True(fast.SpeedMultiplier > normal.SpeedMultiplier && fast.HpMultiplier < normal.HpMultiplier, "fast trades HP for speed");
        True(tank.HpMultiplier > normal.HpMultiplier && tank.SpeedMultiplier < normal.SpeedMultiplier, "tank trades speed for HP");
        True(elite.HpMultiplier > tank.HpMultiplier && elite.XpReward > tank.XpReward, "elite is durable high-value target");
        Equal(normal.XpReward, 1, "normal XP reward");
        Equal(tank.XpReward, 3, "tank XP reward");
        Equal(elite.XpReward, 5, "elite XP reward");
        Near(RunBalanceRules.GetEnemySpawnInterval(0f), 1.25f, 0.000001f, "opening spawn interval");
        Near(RunBalanceRules.GetEnemySpawnInterval(1000f), 0.42f, 0.000001f, "spawn interval lower bound");
        Equal(RunBalanceRules.GetEnemyPopulationCap(0f), 30, "opening population cap");
        Equal(RunBalanceRules.GetEnemyPopulationCap(300f), 40, "population cap scales with time");
        Equal(RunBalanceRules.GetEnemyPopulationCap(900f), 60, "population cap reaches ceiling");
        Equal(RunBalanceRules.GetEnemyPopulationCap(5000f), 60, "population cap remains bounded");
    }

    private static void CheckBossCombatRules()
    {
        Equal(BossCombatRules.BossTypeCount, 8, "canonical eight boss types");
        Equal(BossCombatRules.GetTypeForDefeatedCount(0), "shooter", "boss cycle first type");
        Equal(BossCombatRules.GetTypeForDefeatedCount(7), "stella_warden", "boss cycle eighth type");
        Equal(BossCombatRules.GetTypeForDefeatedCount(8), "shooter", "boss cycle wraps after eight types");
        Equal(BossCombatRules.GetTypeForDefeatedCount(15), "stella_warden", "boss cycle repeats deterministically");
        True(BossCombatRules.IsKnownBossType("conductor"), "Conductor is canonical");
        True(BossCombatRules.IsKnownBossType("architect"), "Architect is canonical");
        True(!BossCombatRules.IsKnownBossType("origami_phoenix"), "legacy silhouette helper is not a gameplay boss type");
        Near(BossCombatRules.GetMaxHp(0), 450f, 0f, "source boss HP at wave 0");
        Near(BossCombatRules.GetMaxHp(10), 1000f, 0f, "source boss HP wave scaling");
        Near(BossCombatRules.GetContactDamage(10), 34.5f, 0.0001f, "source boss damage scaling");
        Equal(BossCombatRules.GetXpReward(10), 100, "source boss XP reward");
        Near(BossCombatRules.GetMoveSpeedSourceUnitsPerSecond("shooter", 10), 60f, 0f, "source standard boss speed");
        Near(BossCombatRules.GetMoveSpeedSourceUnitsPerSecond("charger", 10), 90f, 0f, "source charger speed");
        Near(BossCombatRules.GetMoveSpeedWorldUnitsPerSecond("shooter", 10), 1.05f, 0.0001f, "boss world-space speed conversion against source Canvas scale");
        Near(BossCombatRules.ToWorldUnits(700f), 12.25f, 0.0001f, "source spawn distance conversion");
        Near(BossCombatRules.ToWorldUnits(180f), 3.15f, 0.0001f, "source projectile speed conversion");
        Near(BossCombatRules.BossRadiusWorld + BossCombatRules.PlayerRadiusWorld, 0.875f, 0.0001f, "boss collision radius conversion");
        Near(BossCombatRules.GetAuraRadiusWorld(), 2.1f, 0.0001f, "Aura radius conversion");
        Near(BossCombatRules.GetAuraDamagePerSecond(10), 30f, 0f, "Aura DPS scaling");
        Equal(BossCombatRules.GetChargerPhase(0.8f), BossChargerPhase.Windup, "Charger windup phase");
        Equal(BossCombatRules.GetChargerPhase(0.35f), BossChargerPhase.CommittedDash, "Charger commit threshold");
        Equal(BossCombatRules.GetChargerPhase(0f), BossChargerPhase.Ready, "Charger reset phase");
        Equal(BossCombatRules.GetColorHex("stella_warden"), "#ffd15a", "Stella Warden source tint");
    }

    private static void CheckRegionUnlockRules()
    {
        True(RegionUnlockRules.IsAvailable(EchoRegionId.ResonanceBasin, false), "Basin is the source initial available region");
        True(!RegionUnlockRules.IsAvailable(EchoRegionId.SpectralRift, false), "Rift remains locked before Basin stabilization");
        True(RegionUnlockRules.IsAvailable(EchoRegionId.SpectralRift, true), "Rift opens after Basin stabilization");
        True(RegionUnlockRules.IsAvailable(EchoRegionId.ResonanceBasin, true), "Basin remains available after stabilization");
    }

    private static void CheckPersistentUpgradeRules()
    {
        Equal(PersistentUpgradeRules.Ids.Length, 7, "source shop has seven persistent upgrades");
        Equal(PersistentUpgradeRules.GetBaseCost("dmg"), 200, "source Sphere damage base cost");
        Equal(PersistentUpgradeRules.GetBaseCost("hp"), 150, "source starting HP base cost");
        Equal(PersistentUpgradeRules.GetCost("dmg", 0), 200, "shop cost at level zero");
        Equal(PersistentUpgradeRules.GetCost("dmg", 1), 600, "shop cost follows baseCost * 3^level");
        Equal(PersistentUpgradeRules.GetCost("dmg", 10), 0, "max-level item cannot be purchased");
        Equal(PersistentUpgradeRules.GetMaxLevel("spheres"), 3, "source start-sphere upgrade cap");
        Near(PersistentUpgradeRules.GetPercentMultiplier(4), 1.20f, 0.000001f, "shop +5% level scaling");
        Equal(PersistentUpgradeRules.GetStartingHpBonus(3), 30, "shop HP is +10 per level");
        Equal(PersistentUpgradeRules.GetExtraSpheres(3), 3, "shop capacity is +1 per level");
        Near(PersistentUpgradeRules.GetCritChanceBonus(4), 0.20f, 0.000001f, "shop crit is +5% per level");
    }

    private static void CheckDifficultyRules()
    {
        var easy = DifficultyRules.GetProfile("easy");
        Near(easy.EnemyHpMultiplier, 0.7f, 0f, "source Easy enemy HP");
        Near(easy.EnemySpeedMultiplier, 0.8f, 0f, "source Easy enemy speed");
        Near(easy.EnemyDamageMultiplier, 0.7f, 0f, "source Easy incoming damage");
        Near(easy.GoldMultiplier, 0.8f, 0f, "source Easy gold multiplier");
        Near(easy.SpawnRateMultiplier, 0.8f, 0f, "source Easy spawn rate");
        Near(DifficultyRules.GetProfile("normal").EnemyHpMultiplier, 1f, 0f, "source Normal baseline");
        var hard = DifficultyRules.GetProfile("hard");
        Near(hard.EnemyHpMultiplier, 1.4f, 0f, "source Hard enemy HP");
        Near(hard.GoldMultiplier, 1.5f, 0f, "source Hard gold multiplier");
        var nightmare = DifficultyRules.GetProfile("nightmare");
        Near(nightmare.EnemyHpMultiplier, 2f, 0f, "source Nightmare enemy HP");
        Near(nightmare.EnemyDamageMultiplier, 1.6f, 0f, "source Nightmare incoming damage");
        Near(nightmare.SpawnRateMultiplier, 1.5f, 0f, "source Nightmare spawn rate");
        Equal(DifficultyRules.Normalize("unknown"), "normal", "invalid difficulty falls back to normal");
    }

    private static void CheckRegionBossSchedule()
    {
        Equal(RegionBossScheduleRules.GetRegularBossCount(EchoRegionId.ResonanceBasin), 2, "Resonance Basin has two scheduled major bosses");
        Near(RegionBossScheduleRules.GetRegularBossTime(EchoRegionId.ResonanceBasin, 0), 480f, 0f, "Basin first boss at 8 minutes");
        Near(RegionBossScheduleRules.GetRegularBossTime(EchoRegionId.ResonanceBasin, 1), 960f, 0f, "Basin second boss at 16 minutes");
        Near(RegionBossScheduleRules.FinalPrepSeconds, 1620f, 0f, "final boss event starts at 27 minutes");
        Near(RegionBossScheduleRules.StabilizationSeconds, 1800f, 0f, "Basin stabilization check at 30 minutes");
        Equal(RegionBossScheduleRules.GetBaseGoldReward(EchoRegionId.ResonanceBasin), 250, "Basin source base region reward");
        Equal(RegionBossScheduleRules.GetBaseGoldReward(EchoRegionId.SpectralRift), 360, "Rift source base region reward");
        Equal(RegionBossScheduleRules.GetRegionXpReward(10), 240, "source region XP reward scales with level");

        var basin = RegionBossScheduleRules.Create(EchoRegionId.ResonanceBasin, EchoRegionMode.Stabilization);
        Equal(RegionBossScheduleRules.Advance(basin, 479f, false).Kind, RegionBossSignalKind.None, "no early Basin boss signal");
        var signal = RegionBossScheduleRules.Advance(basin, 480f, false);
        Equal(signal.Kind, RegionBossSignalKind.SpawnBoss, "Basin first boss signal");
        Equal(signal.BossType, "conductor", "Basin first boss type");
        Equal(basin.MajorBossesSpawned, 1, "Basin first major boss tracked");
        Equal(RegionBossScheduleRules.Advance(basin, 960f, true).Kind, RegionBossSignalKind.None, "active boss defers the next Region boss");
        signal = RegionBossScheduleRules.Advance(basin, 960f, false);
        Equal(signal.BossType, "architect", "Basin second boss type after previous boss dies");
        signal = RegionBossScheduleRules.Advance(basin, 1620f, false);
        Equal(signal.Kind, RegionBossSignalKind.SpawnBoss, "Basin final boss signal");
        Equal(signal.BossType, "stella_warden", "Basin final boss is Stella Warden");
        True(signal.IsFinalBoss && basin.FinalBossSpawned, "final spawn state is recorded");
        Equal(RegionBossScheduleRules.Advance(basin, 1800f, true).Kind, RegionBossSignalKind.None, "active final boss defers stabilization");
        Equal(RegionBossScheduleRules.Advance(basin, 1800f, false).Kind, RegionBossSignalKind.RegionCleared, "Basin clears after final boss is no longer active");
        True(basin.Cleared && basin.FinalBossDefeated, "Basin terminal state is persisted in the run state");
        Equal(RegionBossScheduleRules.Advance(basin, 2000f, false).Kind, RegionBossSignalKind.None, "cleared Basin cannot emit duplicate events");

        var rift = RegionBossScheduleRules.Create(EchoRegionId.SpectralRift, EchoRegionMode.Stabilization);
        Equal(RegionBossScheduleRules.Advance(rift, 450f, false).BossType, "null", "Rift first boss");
        Equal(RegionBossScheduleRules.Advance(rift, 930f, false).BossType, "aura", "Rift second boss");
        Equal(RegionBossScheduleRules.GetFinalBossType(EchoRegionId.SpectralRift), "stella_warden", "Rift final boss");

        var endless = RegionBossScheduleRules.Create(EchoRegionId.ResonanceBasin, EchoRegionMode.Endless);
        Equal(RegionBossScheduleRules.Advance(endless, 480f, false).BossType, "conductor", "Endless first scheduled boss");
        Near(endless.NextBossAt, 630f, 0f, "Endless schedule advances by 150 seconds");
        Equal(RegionBossScheduleRules.Advance(endless, 630f, false).BossType, "architect", "Endless second boss");
        True(!endless.Cleared, "Endless mode has no stabilization completion");
    }

    private static void CheckArtifactRewardRules()
    {
        Equal(ArtifactRewardRules.All.Count, 82, "canonical artifact catalog count");
        Equal(ArtifactRewardRules.LegendaryCount, 6, "six Stella-gated Legendary artifacts");
        True(ArtifactRewardRules.IsLegendaryStellaWindow(1499.9f), "Stella Legendary window before 25 minutes");
        True(!ArtifactRewardRules.IsLegendaryStellaWindow(1500f), "Stella changes to ordinary artifacts at 25 minutes");
        Equal(ArtifactRewardRules.GetStellaChoiceCount(Array.Empty<string>()), 3, "base Stella choice count");
        Equal(ArtifactRewardRules.GetStellaChoiceCount(new[] { "quantum_fold" }), 4, "Quantum Fold adds a fourth Stella choice");
        var legendary = ArtifactRewardRules.PickChoices(Array.Empty<string>(), () => 0f, true, 3);
        Equal(legendary.Count, 3, "Stella returns requested unowned Legendary choices");
        True(legendary.TrueForAll(x => x.Rarity == ArtifactRarity.Legendary), "Stella Legendary pool excludes normal rarity");
        Equal(new HashSet<string>(legendary.ConvertAll(x => x.Id)).Count, legendary.Count, "no duplicate choices within one chest");
        var ownedLegendary = new List<string> { "zero_sphere", "unified_mind", "time_splitter", "stasis_mandala", "axiom_core", "universal_fold" };
        Equal(ArtifactRewardRules.PickChoices(ownedLegendary, () => 0f, true, 3).Count, 0, "owned Legendary items cannot be offered twice");
        var ordinary = ArtifactRewardRules.PickChoices(Array.Empty<string>(), () => 0f, false, 3);
        Equal(ordinary.Count, 3, "ordinary Stella fallback offers up to three items");
        True(ordinary.TrueForAll(x => x.Rarity != ArtifactRarity.Legendary), "post-cutoff Stella pool excludes Legendary");
        Near(ArtifactRewardRules.GetEffectTotal(new[] { "crystal_speed", "dragon_heart" }, "moveSpeed"), 0.05f, 0.0001f, "move-speed metadata totals");
        Near(ArtifactRewardRules.GetEffectTotal(new[] { "zero_sphere", "time_splitter" }, "sphereDamage"), 0.47f, 0.0001f, "sphere-damage metadata totals");
        Near(ArtifactRewardRules.GetEffectTotal(new[] { "axiom_core", "universal_fold" }, "cooldown"), -0.16f, 0.0001f, "cooldown metadata totals");
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

    private static void CheckLevelUpAbilityRules()
    {
        Equal(LevelUpAbilityRules.GetMaxLevel(RunUpgradeKind.MaxSpheres), 3, "source maxspheres cap");
        Equal(LevelUpAbilityRules.GetMaxLevel(RunUpgradeKind.Damage), 7, "source passive Ability cap");
        True(LevelUpAbilityRules.CanUpgrade(RunUpgradeKind.Crit, 6), "passive Ability can advance to VII");
        True(!LevelUpAbilityRules.CanUpgrade(RunUpgradeKind.Crit, 7), "passive Ability is capped at VII");
        Equal(LevelUpAbilityRules.RoutineAbilityKinds.Count, 11, "only runtime-wired passive abilities are offered");
        Near(LevelUpAbilityRules.GetSphereDamageMultiplier(0), 1f, 0f, "no damage Ability starts at baseline");
        Near(LevelUpAbilityRules.GetSphereDamageMultiplier(2), 1.30f, 0.000001f, "Sphere Damage is additive +15% per level");
        Near(LevelUpAbilityRules.GetAttackSpeedMultiplier(1), 1f / 0.93f, 0.00001f, "attack speed matches source 0.93 delay multiplier");
        Near(LevelUpAbilityRules.GetMoveSpeedMultiplier(3), 1.30f, 0.000001f, "movement speed +10% per level");
        Near(LevelUpAbilityRules.GetEnemySlowMultiplier(0), 1f, 0f, "enemy slow baseline");
        Near(LevelUpAbilityRules.GetEnemySlowMultiplier(1), 0.90f, 0.000001f, "enemy slow level I");
        Near(LevelUpAbilityRules.GetEnemySlowMultiplier(7), 0.60f, 0.000001f, "enemy slow level VII");
        Near(LevelUpAbilityRules.GetDodgeChance(3), 0.30f, 0.000001f, "dodge is +10% per level");
        Near(LevelUpAbilityRules.GetDodgeChance(7, 0.10f), 0.75f, 0.000001f, "dodge source hard cap");
        Near(LevelUpAbilityRules.GetCritChanceBonus(3), 0.30f, 0.000001f, "critical chance +10% per level");
        Near(LevelUpAbilityRules.GetDamageTakenMultiplier(7), 0.72f, 0.000001f, "armor is -4% taken damage per level");
        Near(LevelUpAbilityRules.GetRegenPerSecond(3), 1.8f, 0.000001f, "regeneration is +0.6 HP/s per level");
        Near(LevelUpAbilityRules.GetMagnetRadiusWorld(0), 60f * SpherePlacementRules.SourcePixelsToWorld, 0.000001f, "XP magnet uses source 60 px baseline");
        Near(LevelUpAbilityRules.GetMagnetRadiusWorld(2), 60f * SpherePlacementRules.SourcePixelsToWorld * 1.4f, 0.000001f, "XP magnet is +20% radius per level");
        Near(LevelUpAbilityRules.GetNetworkDisableDuration(10f, 3), 6.4f, 0.000001f, "network stability reduces disable duration");
        Near(LevelUpAbilityRules.GetNetworkDisableDuration(10f, 7), 1.6f, 0.000001f, "network stability obeys source 16% duration floor");
        var damageChoice = LevelUpAbilityRules.CreateChoice(RunUpgradeKind.Damage, 2);
        Equal(damageChoice.CurrentLevel, 2, "Ability choice preserves current level");
        Equal(damageChoice.NextLevel, 3, "Ability choice advances exactly one level");
        Equal(damageChoice.Kind, RunUpgradeKind.Damage, "Ability card resolves to its authored Ability id");
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
        var unlevelledStandard = SphereLevelRules.GetUnlevelledStats();
        Near(unlevelledStandard.DamageMultiplier, 1f, 0f, "unlevelled Standard has no Level I damage bonus");
        Equal(unlevelledStandard.Pierce, 0, "unlevelled Standard has no bonus pierce");
        Equal(unlevelledStandard.Pellets, 3, "unlevelled baseline pellet count");
        Equal(unlevelledStandard.ChainTargets, 3, "unlevelled baseline chain target count");
        Near(unlevelledStandard.CritChanceBonus, 0f, 0f, "unlevelled Sphere has no progression crit");
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
        Near(SphereEvolutionCombatRules.GetGravityClusterPullMultiplier(1f), 1.2f, 0f, "Cluster formation boosts Gravity pull");
        Near(SphereEvolutionCombatRules.GetGravityClusterPullMultiplier(0.5f), 1.1f, 0f, "Secondary Cluster gives half Gravity pull bonus");
    }

    private static void CheckOrbitalEvolutionCombatRules()
    {
        Equal(SphereEvolutionCombatRules.GetOrbitalInnerCount(1, null), 1, "Orbital level I inner count");
        Equal(SphereEvolutionCombatRules.GetOrbitalOuterCount(1, null), 1, "Orbital level I outer count");
        Equal(SphereEvolutionCombatRules.GetOrbitalInnerCount(7, null), 4, "Orbital level VII inner count");
        Equal(SphereEvolutionCombatRules.GetOrbitalOuterCount(7, null), 4, "Orbital level VII outer count");
        Equal(SphereEvolutionCombatRules.GetOrbitalOuterCount(7, "orbital_dance_final_3"), 5, "Dance Swarm adds ninth element");
        True(SphereEvolutionCombatRules.GetOrbitalAngularSpeed(7, "orbital_dance_final_2") > SphereEvolutionCombatRules.GetOrbitalAngularSpeed(7, null), "Hypercycle speed");
        Near(SphereEvolutionCombatRules.GetOrbitalDamageMultiplier(7, "orbital_blade", "orbital_blade_final_3"), 1.30f, 0f, "Orbital Blade final damage");
        Near(SphereEvolutionCombatRules.GetOrbitalAfterimageMultiplier("orbital_blade", "orbital_blade_final_2"), 0.25f, 0f, "Blade afterimage multiplier");
    }

    private static void CheckPrismEvolutionCombatRules()
    {
        Equal(SphereEvolutionCombatRules.GetPrismBeamCount(1, null, null, 0), 1, "Prism default emits one direct beam");
        Equal(SphereEvolutionCombatRules.GetPrismBeamCount(2, null, null, 0), 2, "Prism level III emits its second direction");
        Equal(SphereEvolutionCombatRules.GetPrismBeamCount(2, "prism_split", "prism_split_final_2", 0), 3,
            "Prism Split final II adds its authored beam");
        Equal(SphereEvolutionCombatRules.GetPrismBeamCount(2, "prism_split", "prism_split_final_3", 0), 3,
            "Prism Split final III adds its authored beam");
        Equal(SphereEvolutionCombatRules.GetPrismBeamCount(2, "prism_split", "prism_split_final_2", 1), 4,
            "Prism beam count includes the independent authored Multishot modifier");

        var prismBaseRange = 130f * SpherePlacementRules.SourcePixelsToWorld *
            SphereCombatRules.GetProfile(SphereId.Prism).RangeMultiplier;
        Near(SphereEvolutionCombatRules.GetPrismRangeWorld(1, null, 1f), prismBaseRange, 0.0001f,
            "Prism range uses the source 130px base radius and source-pixel world scale");
        Near(SphereEvolutionCombatRules.GetPrismRangeWorld(2, null, 1f), prismBaseRange * 1.15f, 0.0001f,
            "Prism level II radius upgrade");
        Near(SphereEvolutionCombatRules.GetPrismRangeWorld(1, "prism_mirror", 1f), prismBaseRange * 1.08f, 0.0001f,
            "Prism Mirror source radius modifier");
        Near(SphereEvolutionCombatRules.GetPrismRangeWorld(1, null, 0.8f), prismBaseRange * 0.8f, 0.0001f,
            "Prism artifact radius modifier");

        Near(SphereEvolutionCombatRules.GetPrismBranchPower(4, null), 1f, 0f, "Prism pre-final branch power before level V");
        Near(SphereEvolutionCombatRules.GetPrismBranchPower(5, null), 1.15f, 0.0001f, "Prism pre-final branch power at level V");
        Near(SphereEvolutionCombatRules.GetPrismBranchPower(6, null), 1.30f, 0.0001f, "Prism pre-final branch power at level VI");
        Near(SphereEvolutionCombatRules.GetPrismBranchPower(7, "prism_mirror_final_1"), 1f, 0f, "Prism final choice replaces pre-final branch power");
        Near(SphereEvolutionCombatRules.GetPrismBeamDamageMultiplier(6, "prism_split", null, 1, false), 0.806f, 0.0001f,
            "Prism Split secondary pre-final beam scales by 62% and branch power");
        Near(SphereEvolutionCombatRules.GetPrismBeamDamageMultiplier(7, "prism_split", "prism_split_final_2", 1, false), 0.72f, 0.0001f,
            "Prism Split final II secondary beam");
        Near(SphereEvolutionCombatRules.GetPrismBeamDamageMultiplier(5, "prism_spectrum", null, 0, true), 1.242f, 0.0001f,
            "Prism Spectrum pre-final status beam bonus");
        Near(SphereEvolutionCombatRules.GetPrismBeamDamageMultiplier(7, "prism_spectrum", "prism_spectrum_final_3", 0, false), 1f, 0f,
            "Prism Spectrum does not apply status bonus without an active status");
        Near(SphereEvolutionCombatRules.GetPrismFormationDamageMultiplier(0.5f, 0.5f), 1.10f, 0.0001f,
            "Prism source Line and Lattice beam bonuses");

        Equal(SphereEvolutionCombatRules.GetPrismReflectionCount("prism_mirror", "prism_mirror_final_2", false, false, false), 1,
            "Prism Mirror final II reflects to one linked node");
        Equal(SphereEvolutionCombatRules.GetPrismReflectionCount("prism_mirror", "prism_mirror_final_3", true, true, true), 5,
            "Prism Mirror final III stacks filter, crown and Ring reflection");
        Equal(SphereEvolutionCombatRules.GetPrismReflectionCount("prism_split", "prism_split_final_2", true, true, true), 3,
            "Prism Filter opens reflections and stacks Crown plus Ring");
        Equal(SphereEvolutionCombatRules.GetPrismReflectionCount("prism_split", "prism_split_final_2", false, true, true), 0,
            "Prism Crown and Ring alone do not open the reflection path");
        Equal(SphereEvolutionCombatRules.GetPrismMirrorRicochetCount(6, "prism_mirror", null, 0), 1,
            "Prism Mirror adds its level-VI pre-final bounce");
        Equal(SphereEvolutionCombatRules.GetPrismMirrorRicochetCount(7, "prism_mirror", "prism_mirror_final_2", 2), 2,
            "Prism Mirror authored Ricochet is capped at two");
        Equal(SphereEvolutionCombatRules.GetPrismMirrorRicochetCount(6, "prism_split", null, 1), 1,
            "other branches do not receive Prism Mirror's extra pre-final bounce");
        Near(SphereEvolutionCombatRules.GetPrismNetworkRelayDamageMultiplier(), 0.42f, 0f, "Prism linked-node relay damage");
        Near(SphereEvolutionCombatRules.GetPrismRicochetDamageMultiplier(0, 5, null), 0.345f, 0.0001f,
            "Prism first pre-final ricochet damage");
        Near(SphereEvolutionCombatRules.GetPrismRicochetDamageMultiplier(1, 5, null), 0.276f, 0.0001f,
            "Prism second pre-final ricochet damage");
        Near(SphereEvolutionCombatRules.GetPrismRicochetRangeWorld(5, null), 2.314375f, 0.0001f,
            "Prism ricochet range is source 115 pixels scaled by branch power");
        Near(SphereEvolutionCombatRules.GetPrismEchoDamageMultiplier(), 0.18f, 0f, "Prism Mirror final III Echo multiplier");
    }

    private static void CheckPulseVoidEvolutionCombatRules()
    {
        True(SphereEvolutionCombatRules.GetPulseWaveRadiusMultiplier(6, null) > SphereEvolutionCombatRules.GetPulseWaveRadiusMultiplier(5, null), "Pulse Wave level VI radius");
        Near(SphereEvolutionCombatRules.GetPulseWaveRadiusMultiplier(7, "pulse_wave_final_2"), 1.24f, 0f, "Pulse Freeze radius");
        Near(SphereEvolutionCombatRules.GetPulseWaveKnockbackDistance(7, "pulse_wave_final_1"), 0.50f, 0f, "Pulse Rebound knockback");
        True(SphereEvolutionCombatRules.ShouldPulseBurstDoubleWave(5, null), "Pulse Burst branch emits secondary wave");
        True(!SphereEvolutionCombatRules.ShouldPulseBurstDoubleWave(7, "pulse_burst_final_2"), "Impact Burst is single wave");
        True(SphereEvolutionCombatRules.ShouldPulseBurstDoubleWave(7, "pulse_burst_final_3"), "Double Pulse emits second wave");
        True(SphereEvolutionCombatRules.GetVoidHungerDamageMultiplier(7, "void_hunger_final_3", 0.20f) > SphereEvolutionCombatRules.GetVoidHungerDamageMultiplier(7, "void_hunger_final_1", 0.20f), "Void Devour low-HP bonus");
        Equal(SphereEvolutionCombatRules.GetVoidReaperShardCount(6, null), 3, "Void Reaper level VI shard count");
        Near(SphereEvolutionCombatRules.GetVoidReaperKillHeal(7, "void_reaper_final_3"), 6f, 0f, "Devouring Reaper kill heal");
        Near(SphereEvolutionCombatRules.GetVoidExecutionThreshold(7, "void_execution_final_3"), 0.30f, 0f, "Absolute execution threshold");
        Near(SphereEvolutionCombatRules.GetVoidExecutionChance(7, "void_execution_final_3"), 0.18f, 0f, "Absolute execution chance");
        Equal(SphereEvolutionCombatRules.GetVoidExecutionPierce(0, "void_execution_final_2"), 1, "Phase adds projectile pierce");
    }

    private static void CheckResonanceRules()
    {
        Near(ResonanceRules.Clamp(float.NaN), 0f, 0f, "Resonance NaN normalized");
        Near(ResonanceRules.Clamp(170f), 100f, 0f, "Resonance baseline cap");
        Near(ResonanceRules.Clamp(170f, true), 150f, 0f, "Resonance explicit overflow cap");
        Near(ResonanceRules.SphereHitCharge, 1f, 0f, "Sphere hit charge");
        Near(ResonanceRules.GeometryCharge, 5f, 0f, "Geometry charge");
        Near(ResonanceRules.NetworkCharge, 5f, 0f, "Network charge");
        Near(ResonanceRules.RuneCharge, 60f, 0f, "Rune charge");
        var charge = 0f;
        Equal(ResonanceRules.Add(ref charge, 1f), 0, "one hit does not trigger Resonance event");
        Near(charge, 1f, 0f, "single hit charge retained");
        charge = 99f;
        Equal(ResonanceRules.Add(ref charge, 2f), 1, "crossing 100 triggers one event");
        Near(charge, 1f, 0f, "event preserves remainder");
        Equal(ResonanceRules.Add(ref charge, float.NaN), 0, "invalid Resonance gain ignored");
        Near(charge, 1f, 0f, "invalid gain leaves charge unchanged");
        charge = 149f;
        Equal(ResonanceRules.Add(ref charge, 2f, true), 1, "overflow mode still emits event");
        Near(charge, 51f, 0f, "overflow event preserves remainder");
        Near(SphereEvolutionCombatRules.GetOrbitalResonanceBonus(7, "orbital_halo_final_1"), 1f, 0f, "Halo Resonance final bonus");
        Near(SphereEvolutionCombatRules.GetOrbitalResonanceBonus(6, null), 0.25f, 0f, "Halo level VI charge bonus");
        True(SphereEvolutionCombatRules.ShouldOrbitalGrantShieldCharge(3), "Halo Guard shield cadence");
        True(!SphereEvolutionCombatRules.ShouldOrbitalGrantShieldCharge(2), "Halo Guard avoids charge on every pass");
    }

    private static void CheckSpherePlacementRules()
    {
        Equal(SpherePlacementRules.GetMaxSpheres(), 5, "source default Sphere cap");
        Equal(SpherePlacementRules.GetMaxSpheres(3, 4), 8, "source Sphere cap clamps to eight");
        True(SpherePlacementRules.CanAddSphere(4, 1, 5, 2), "place Sphere while below global and same-type caps");
        True(!SpherePlacementRules.CanAddSphere(5, 1, 5, 2), "global Sphere cap blocks placement");
        True(!SpherePlacementRules.CanAddSphere(2, 2, 5, 2), "source same-type copy cap blocks placement");
        True(SpherePlacementRules.CanAddSphere(4, 2, 5, 3), "mastery V signature cap permits a third copy");
        True(!SpherePlacementRules.CanAddSphere(4, 3, 5, 3), "mastery V signature cap blocks a fourth copy");
        True(SpherePlacementRules.CanAddSphere(6, 3, 8, 4), "mastery X signature cap permits a fourth copy");
        Equal(SpherePlacementRules.GetMaxSpheres(1, 0), 6, "one maxspheres ability level adds one active Sphere slot");
        Equal(SpherePlacementRules.GetMaxSpheres(3, 0), 8, "third maxspheres ability level reaches global cap");
        Equal(SpherePlacementRules.GetMaxSpheres(0, 2), 7, "source shop sphere upgrades add to the base cap");
        Equal(SpherePlacementRules.GetMaxSpheres(1, 2), 8, "ability and shop Sphere bonuses are combined");
        Equal(SpherePlacementRules.GetMaxSpheres(3, 3), 8, "combined Sphere capacity bonuses clamp at eight");

        Equal(SpherePlacementRules.GetSameTypeCap(SphereCharacterId.Spherist, SphereId.Standard, 1), 2, "Spherist signature starts at two copies");
        Equal(SpherePlacementRules.GetSameTypeCap(SphereCharacterId.Spherist, SphereId.Standard, 4), 2, "signature cap stays two below mastery V");
        Equal(SpherePlacementRules.GetSameTypeCap(SphereCharacterId.Spherist, SphereId.Standard, 5), 3, "signature cap rises to three at mastery V");
        Equal(SpherePlacementRules.GetSameTypeCap(SphereCharacterId.Spherist, SphereId.Standard, 9), 3, "signature cap stays three below mastery X");
        Equal(SpherePlacementRules.GetSameTypeCap(SphereCharacterId.Spherist, SphereId.Standard, 10), 4, "signature cap rises to four at mastery X");
        Equal(SpherePlacementRules.GetSameTypeCap(SphereCharacterId.Spherist, SphereId.Sniper, 10), 2, "non-signature cap remains two");
        Equal(SpherePlacementRules.GetSameTypeCap(SphereCharacterId.Hunter, SphereId.Sniper, 5), 3, "Hunter signature is Sniper");
        Equal(SpherePlacementRules.GetSameTypeCap(SphereCharacterId.Engineer, SphereId.Chain, 5), 3, "Engineer signature is Chain");
        Equal(SpherePlacementRules.GetSameTypeCap(SphereCharacterId.Berserker, SphereId.Shotgun, 5), 3, "Berserker signature is Shotgun");
        Equal(SpherePlacementRules.GetSameTypeCap(SphereCharacterId.Alchemist, SphereId.Aura, 5), 3, "Alchemist signature is Aura");
        Equal(SpherePlacementRules.GetSameTypeCap(SphereCharacterId.Architect, SphereId.Gravity, 5), 3, "Architect signature is Gravity");
        Equal(SpherePlacementRules.GetSameTypeCap(SphereCharacterId.Conductor, SphereId.Pulse, 5), 3, "Conductor signature is Pulse");
        Equal(SpherePlacementRules.GetSameTypeCap(SphereCharacterId.Oracle, SphereId.Prism, 5), 3, "Oracle signature is Prism");
        Equal(SpherePlacementRules.GetSameTypeCap(SphereCharacterId.Voidwalker, SphereId.Void, 5), 3, "Voidwalker signature is Void");
        Equal(SpherePlacementRules.GetSameTypeCap(SphereCharacterId.Fractal, SphereId.Orbital, 5), 3, "Fractal signature is Orbital");
        Near(SpherePlacementRules.GetCopyDamageMultiplier(SphereCharacterId.Spherist, SphereId.Standard, 4, 2), 1f, 0f,
            "extra-copy damage scaling is disabled before signature mastery unlocks");
        Near(SpherePlacementRules.GetCopyDamageMultiplier(SphereCharacterId.Spherist, SphereId.Standard, 5, 1), 1f, 0f,
            "first two signature copies retain full output");
        Near(SpherePlacementRules.GetCopyDamageMultiplier(SphereCharacterId.Spherist, SphereId.Standard, 5, 2), 0.85f, 0f,
            "third signature copy uses source 85% output");
        Near(SpherePlacementRules.GetCopyDamageMultiplier(SphereCharacterId.Spherist, SphereId.Standard, 10, 3), 0.70f, 0f,
            "fourth signature copy uses source 70% output");
        Near(SpherePlacementRules.GetCopyDamageMultiplier(SphereCharacterId.Hunter, SphereId.Sniper, 10, 2), 0.85f, 0f,
            "Hunter's third signature Sniper uses source 85% output");
        Near(SpherePlacementRules.GetCopyDamageMultiplier(SphereCharacterId.Hunter, SphereId.Standard, 10, 2), 1f, 0f,
            "non-signature Sphere output is not reduced");

        True(SphereProgressionRules.CanUpgradeMaxSpheres(0), "maxspheres ability starts available at level zero");
        True(SphereProgressionRules.CanUpgradeMaxSpheres(2), "maxspheres ability can reach level three");
        True(!SphereProgressionRules.CanUpgradeMaxSpheres(3), "maxspheres ability stops at source level three");
        var maxSphereChoice = SphereProgressionRules.CreateMaxSpheresChoice(0);
        Equal(maxSphereChoice.Kind, RunUpgradeKind.MaxSpheres, "source maxspheres choice kind");
        Equal(maxSphereChoice.NextLevel, 1, "source maxspheres ability advances one level");
        Equal(SphereProgressionRules.CreateMaxSpheresChoice(2).NextLevel, 3, "source maxspheres ability reaches level three");

        var noSpheres = Array.Empty<Vec2>();
        var noSphereAlive = Array.Empty<bool>();
        var noEnemies = Array.Empty<Vec2>();
        var noEnemyRadii = Array.Empty<float>();
        True(SpherePlacementRules.CanPlace(new Vec2(0f, -2f), Vec2.Zero, noSpheres, noSphereAlive, noEnemies, noEnemyRadii),
            "source-valid placement outside the Core exclusion radius");
        True(!SpherePlacementRules.CanPlace(new Vec2(0f, -0.5f), Vec2.Zero, noSpheres, noSphereAlive, noEnemies, noEnemyRadii),
            "placement near Core is blocked");
        True(SpherePlacementRules.CanPlace(new Vec2(22f, 0f), Vec2.Zero, noSpheres, noSphereAlive, noEnemies, noEnemyRadii),
            "placement does not add a world-bounds rule absent from source canPlaceSphere");

        var oneSphere = new[] { new Vec2(0f, -2f) };
        var oneAlive = new[] { true };
        True(!SpherePlacementRules.CanPlace(new Vec2(0f, -2.5f), Vec2.Zero, oneSphere, oneAlive, noEnemies, noEnemyRadii),
            "overlapping Sphere placement is blocked");
        True(SpherePlacementRules.CanPlace(new Vec2(0f, -4f), Vec2.Zero, oneSphere, oneAlive, noEnemies, noEnemyRadii),
            "valid placement preserves navigable route around existing Sphere");

        var enemyPositions = new[] { new Vec2(0f, -2.8f) };
        var enemyRadii = new[] { 0.4f };
        True(!SpherePlacementRules.CanPlace(new Vec2(0f, -2f), Vec2.Zero, noSpheres, noSphereAlive, enemyPositions, enemyRadii),
            "placement under a living enemy is blocked");

        Near(SpherePlacementRules.SourcePixelsToWorld, 0.0175f, 0.000001f, "Source-to-Unity Sphere coordinate calibration");
        var clampedReposition = SpherePlacementRules.ClampReposition(new Vec2(22f, -22f));
        Near(clampedReposition.X, SpherePlacementRules.WorldWidth * 0.5f, 0.000001f,
            "reposition clamps to source world X bounds only on commit");
        Near(clampedReposition.Y, -SpherePlacementRules.WorldHeight * 0.5f, 0.000001f,
            "reposition clamps to source world Y bounds only on commit");
        Near(SpherePlacementRules.DefaultMaxSpheres, 5f, 0f, "source default maximum Sphere count");
        Near(SphereNetworkRules.DefaultLinkDistance, 220f * SpherePlacementRules.SourcePixelsToWorld, 0.000001f,
            "source 220 px Network distance uses current camera scale");
    }

    private static void CheckCharacterRules()
    {
        Equal(CharacterRules.All.Count, 10, "source character roster contains ten characters");
        Equal(CharacterRules.Get(SphereCharacterId.Spherist).UnlockCost, 0, "Spherist unlock is free");
        Equal(CharacterRules.Get(SphereCharacterId.Hunter).UnlockCost, 1500, "Hunter source unlock cost");
        Equal(CharacterRules.Get(SphereCharacterId.Engineer).UnlockCost, 2000, "Engineer source unlock cost");
        Equal(CharacterRules.Get(SphereCharacterId.Berserker).UnlockCost, 2000, "Berserker source unlock cost");
        Equal(CharacterRules.Get(SphereCharacterId.Alchemist).UnlockCost, 2500, "Alchemist source unlock cost");
        Equal(CharacterRules.Get(SphereCharacterId.Architect).UnlockCost, 3000, "Architect source unlock cost");
        Equal(CharacterRules.Get(SphereCharacterId.Conductor).UnlockCost, 3500, "Conductor source unlock cost");
        Equal(CharacterRules.Get(SphereCharacterId.Oracle).UnlockCost, 4000, "Oracle source unlock cost");
        Equal(CharacterRules.Get(SphereCharacterId.Voidwalker).UnlockCost, 4500, "Voidwalker source unlock cost");
        Equal(CharacterRules.Get(SphereCharacterId.Fractal).UnlockCost, 5000, "Fractal source unlock cost");
        foreach (var character in CharacterRules.All)
        {
            True(!string.IsNullOrEmpty(character.NameRu) && !string.IsNullOrEmpty(character.NameEn), "character names are source-defined");
            Equal(character.Mastery.Count, 10, "every source character has ten mastery levels");
            Equal(character.Mastery[0].Level, 1, "mastery definitions preserve source ordering");
            Equal(character.Mastery[9].Level, 10, "mastery definitions include final level");
            True(CharacterRules.TryParseSourceId(CharacterRules.ToSourceId(character.Id), out var parsed) && parsed == character.Id,
                "character ID round-trips to source persistence schema");
        }

        Equal(CharacterRules.GetMasteryThreshold(1), 0, "mastery I threshold");
        Equal(CharacterRules.GetMasteryThreshold(2), 250, "mastery II threshold");
        Equal(CharacterRules.GetMasteryThreshold(4), 1500, "mastery IV threshold");
        Equal(CharacterRules.GetMasteryThreshold(10), 15000, "mastery X threshold");
        Equal(CharacterRules.GetMasteryLevelForXp(1499), 3, "XP below 1500 stays mastery III");
        Equal(CharacterRules.GetMasteryLevelForXp(1500), 4, "1500 XP reaches mastery IV");
        Equal(CharacterRules.GetMasteryLevelForXp(15000), 10, "maximum XP reaches mastery X");
        Equal(CharacterRules.GetNextMasteryThreshold(1), 250, "mastery I next threshold");
        Equal(CharacterRules.GetNextMasteryThreshold(10), -1, "mastery X has no next threshold");
        Equal(CharacterRules.GetUpdatedMasteryXp(14990, 100), 15000, "mastery XP clamps at source cap");
        Equal(CharacterRules.GetUpdatedMasteryXp(600, -10), 600, "negative mastery XP gain is ignored");

        Near(CharacterRules.GetSphereAffinityWeight(SphereCharacterId.Hunter, SphereId.Sniper), 0.70f, 0f, "Hunter signature affinity");
        Near(CharacterRules.GetSphereAffinityWeight(SphereCharacterId.Hunter, SphereId.Void), 0.30f, 0f, "Hunter partner affinity");
        Near(CharacterRules.GetSphereAffinityWeight(SphereCharacterId.Hunter, SphereId.Aura), 0f, 0f, "unpreferred Sphere has no character affinity");
        Near(CharacterRules.GetBerserkerFuryDamageBonus(0.6f), 0.14f, 0.000001f, "Berserker Fury +7% for each missing 20% HP");
        Near(CharacterRules.GetBerserkerFuryAttackSpeedBonus(0.6f), 0.08f, 0.000001f, "Berserker Fury +4% attack speed for each missing 20% HP");
        Near(CharacterRules.GetBerserkerFuryDamageBonus(0f), 0.28f, 0.000001f, "Berserker Fury damage is capped at +28%");
        Near(CharacterRules.GetBerserkerCloseDamageBonus(1), 0.12f, 0f, "Berserker close bonus at mastery I");
        Near(CharacterRules.GetBerserkerCloseDamageBonus(2), 0.15f, 0f, "Berserker close bonus at mastery II");
        Near(CharacterRules.GetBerserkerCloseDamageBonus(10), 0.20f, 0f, "Berserker close bonus at mastery X");
        Near(CharacterRules.GetSpheristDamageBonus(1, 4), 0f, 0f, "Spherist five-Sphere threshold at mastery I");
        Near(CharacterRules.GetSpheristDamageBonus(1, 5), 0.05f, 0f, "Spherist damage bonus at five Spheres");
        Near(CharacterRules.GetSpheristDamageBonus(2, 4), 0.05f, 0f, "Spherist mastery II lowers threshold to four Spheres");
        Near(CharacterRules.GetSpheristDamageBonus(8, 3), 0.07f, 0.000001f, "Spherist mastery VIII lowers threshold to three Spheres and includes the mastery III +2% damage bonus");
        Near(CharacterRules.GetSpheristDamageBonus(10, 8), 0.12f, 0.000001f, "Spherist mastery X damage bonuses stack from source thresholds");
        Near(CharacterRules.GetSpheristAttackSpeedBonus(1, 4), 0.09f, 0.000001f, "Spherist Resonance gives +3% per Sphere after first");
        Near(CharacterRules.GetSpheristAttackSpeedBonus(4, 4), 0.105f, 0.000001f, "Spherist mastery IV adds +0.5% per Sphere after first");
    }

    private static void CheckHunterCombatRules()
    {
        True(HunterCombatRules.CanMarkTarget(SphereId.Sniper, false, false, EnemyArchetype.Tank),
            "Hunter Sniper can mark a tank");
        True(HunterCombatRules.CanMarkTarget(SphereId.Chain, false, true, EnemyArchetype.Normal),
            "Hunter Chain can mark an elite");
        True(HunterCombatRules.CanMarkTarget(SphereId.Prism, true, false, EnemyArchetype.Normal),
            "Hunter Prism can mark a boss");
        True(HunterCombatRules.CanMarkTarget(SphereId.Void, false, false, EnemyArchetype.Normal) == false,
            "Hunter Void cannot mark an ordinary non-tank enemy");
        True(!HunterCombatRules.CanMarkTarget(SphereId.Aura, false, true, EnemyArchetype.Elite),
            "non-source eligible Aura hits do not build Hunter Mark");
        True(!HunterCombatRules.CanMarkTarget(SphereId.Standard, true, false, EnemyArchetype.Normal),
            "non-source eligible Standard hits do not build Hunter Mark");

        Near(HunterCombatRules.GetMarkDuration(1), 5f, 0f, "Hunter mastery I Mark duration is five seconds");
        Near(HunterCombatRules.GetMarkDuration(2), 6f, 0f, "Hunter mastery II extends Mark to six seconds");
        Equal(HunterCombatRules.GetHuntHitThreshold(1), 5, "Hunter Hunt takes five same-target hits at mastery I");
        Equal(HunterCombatRules.GetHuntHitThreshold(3), 5, "Hunter Hunt threshold remains five before mastery IV");
        Equal(HunterCombatRules.GetHuntHitThreshold(4), 4, "Hunter mastery IV starts Hunt after four same-target hits");

        Near(HunterCombatRules.GetMarkedDamageMultiplier(1, false, true), 1f, 0f,
            "Hunter does not amplify an unmarked target");
        Near(HunterCombatRules.GetMarkedDamageMultiplier(1, true, false), 1.20f, 0.000001f,
            "Hunter Mark adds twenty percent damage");
        Near(HunterCombatRules.GetMarkedDamageMultiplier(6, true, false), 1.25f, 0.000001f,
            "Hunter mastery VI raises Mark damage to twenty-five percent");
        Near(HunterCombatRules.GetMarkedDamageMultiplier(1, true, true), 1.56f, 0.000001f,
            "Hunter Hunt stacks the source 1.20 Mark and 1.30 Hunt multipliers");
        Near(HunterCombatRules.GetMarkedDamageMultiplier(8, true, true), 1.6875f, 0.000001f,
            "Hunter mastery VIII stacks 1.25 Mark and 1.35 Hunt multipliers");

        Near(HunterCombatRules.GetMarkedTargetCritBonus(2, true), 0f, 0f,
            "Hunter marked-target critical bonus is locked before mastery III");
        Near(HunterCombatRules.GetMarkedTargetCritBonus(3, true), 0.02f, 0.000001f,
            "Hunter mastery III adds two percent marked-target critical chance");
        Near(HunterCombatRules.GetMarkedTargetCritBonus(3, false), 0f, 0f,
            "Hunter critical bonus requires an active mark");

        Near(HunterCombatRules.GetTrophyMoveSpeedBonus(4, true), 0f, 0f,
            "Hunter Trophy speed is locked before mastery V");
        Near(HunterCombatRules.GetTrophyMoveSpeedBonus(5, true), 0.10f, 0.000001f,
            "Hunter mastery V Trophy grants ten percent movement speed");
        Near(HunterCombatRules.GetTrophyMoveSpeedBonus(10, false), 0f, 0f,
            "Hunter Trophy speed ends when its timer expires");
    }

    private static void CheckEngineerRelayRules()
    {
        Equal(EngineerRelayRules.GetNetworkRangePixels(1), 220, "Engineer mastery I link range");
        Equal(EngineerRelayRules.GetNetworkRangePixels(2), 240, "Engineer mastery II link range");
        Equal(EngineerRelayRules.GetNetworkRangePixels(5), 240, "Engineer link range stays 240 px before mastery VI");
        Equal(EngineerRelayRules.GetNetworkRangePixels(6), 260, "Engineer mastery VI link range");
        Near(EngineerRelayRules.GetWindowSeconds(1), 0.40f, 0f, "Engineer base relay window");
        Near(EngineerRelayRules.GetWindowSeconds(3), 0.40f, 0f, "Engineer relay window remains 0.40 s before mastery IV");
        Near(EngineerRelayRules.GetWindowSeconds(4), 0.55f, 0f, "Engineer mastery IV extends relay window");
        Near(EngineerRelayRules.GetDamageMultiplier(), 1.25f, 0f, "Engineer relay source multiplier");
        True(EngineerRelayRules.ConsumesOnAttack(SphereId.Standard), "Standard consumes relay bonus");
        True(EngineerRelayRules.ConsumesOnAttack(SphereId.Sniper), "Sniper consumes relay bonus");
        True(EngineerRelayRules.ConsumesOnAttack(SphereId.Shotgun), "Shotgun consumes relay bonus");
        True(EngineerRelayRules.ConsumesOnAttack(SphereId.Chain), "Chain consumes relay bonus");
        True(EngineerRelayRules.ConsumesOnAttack(SphereId.Void), "Void consumes relay bonus");
        True(!EngineerRelayRules.ConsumesOnAttack(SphereId.Aura), "Aura does not consume generic relay bonus");
        True(!EngineerRelayRules.ConsumesOnAttack(SphereId.Orbital), "Orbital does not consume generic relay bonus");
        True(!EngineerRelayRules.ConsumesOnAttack(SphereId.Prism), "Prism does not consume generic relay bonus");
        True(!EngineerRelayRules.ConsumesOnAttack(SphereId.Gravity), "Gravity does not consume generic relay bonus");
        True(!EngineerRelayRules.ConsumesOnAttack(SphereId.Pulse), "Pulse does not consume generic relay bonus");
        True(EngineerRelayRules.CanConsumeRelay(true, true, 0.01f, false, 220f, 1),
            "Engineer can consume a live relay at the source range boundary");
        True(EngineerRelayRules.CanConsumeRelay(true, true, 0.01f, false, 260f, 6),
            "Engineer mastery VI can consume at 260 px");
        True(!EngineerRelayRules.CanConsumeRelay(true, true, 0.01f, false, 220.01f, 1),
            "Engineer relay does not cross its source link range");
        True(!EngineerRelayRules.CanConsumeRelay(true, true, 0.01f, true, 0f, 1),
            "Engineer relay source cannot consume its own bonus");
        True(!EngineerRelayRules.CanConsumeRelay(true, true, 0f, false, 100f, 1),
            "expired Engineer relay cannot be consumed");
        True(!EngineerRelayRules.CanConsumeRelay(false, true, 0.40f, false, 100f, 1),
            "non-Engineer character cannot consume relay");
        True(!EngineerRelayRules.CanConsumeRelay(true, false, 0.40f, false, 100f, 1),
            "relay requires a source Sphere");
    }

    private static void CheckConductorOverdriveRules()
    {
        Near(ConductorOverdriveRules.GetOverdriveDurationSeconds(1), 2f, 0f, "Conductor mastery I Overdrive duration");
        Near(ConductorOverdriveRules.GetOverdriveDurationSeconds(2), 2.5f, 0f, "Conductor mastery II Overdrive duration");
        Near(ConductorOverdriveRules.GetAttackSpeedBonus(1), 0.18f, 0f, "Conductor base Overdrive speed");
        Near(ConductorOverdriveRules.GetAttackSpeedBonus(3), 0.18f, 0f, "Conductor speed before mastery IV");
        Near(ConductorOverdriveRules.GetAttackSpeedBonus(4), 0.23f, 0f, "Conductor mastery IV speed");
        Near(ConductorOverdriveRules.GetAttackSpeedBonus(7), 0.23f, 0f, "Conductor speed before mastery VIII");
        Near(ConductorOverdriveRules.GetAttackSpeedBonus(8), 0.28f, 0f, "Conductor mastery VIII speed");
        Equal(ConductorOverdriveRules.GetLineSurgeCharges(3), 1, "Conductor mastery III Line Surge");
        Equal(ConductorOverdriveRules.GetLineSurgeCharges(6), 1, "Conductor Line Surge before mastery VII");
        Equal(ConductorOverdriveRules.GetLineSurgeCharges(7), 2, "Conductor mastery VII Line Surge");
        Near(ConductorOverdriveRules.GetTriangleDischargeDamageMultiplier(7), 0.40f, 0f, "Conductor Triangle before mastery VIII");
        Near(ConductorOverdriveRules.GetTriangleDischargeDamageMultiplier(8), 0.55f, 0f, "Conductor mastery VIII Triangle");
        Near(ConductorOverdriveRules.GetClusterAttackAdvanceSeconds(7), 0.32f, 0f, "Conductor Cluster before mastery VIII");
        Near(ConductorOverdriveRules.GetClusterAttackAdvanceSeconds(8), 0.48f, 0f, "Conductor mastery VIII Cluster");
        Equal(ConductorOverdriveRules.GetSquareShieldChargeBonus(7), 1, "Conductor Square shield before mastery VIII");
        Equal(ConductorOverdriveRules.GetSquareShieldChargeBonus(8), 2, "Conductor mastery VIII Square shield");
        True(!ConductorOverdriveRules.ShouldTriggerCascade(5, 3), "Conductor Cascade locked before mastery VI");
        True(!ConductorOverdriveRules.ShouldTriggerCascade(6, 2), "Conductor Cascade before third event");
        True(ConductorOverdriveRules.ShouldTriggerCascade(6, 3), "Conductor mastery VI every-third-event Cascade");
        True(ConductorOverdriveRules.ShouldTriggerCascade(9, 6), "Conductor mastery IX retains Cascade cadence");
        Equal(ConductorOverdriveRules.GetCascadeTargetCount(6), 1, "Conductor Cascade base target count");
        Equal(ConductorOverdriveRules.GetCascadeTargetCount(7), 2, "Conductor mastery VII Cascade relay");
        Equal(ConductorOverdriveRules.GetCascadeTargetCount(8), 2, "Conductor mastery VIII Cascade target count");
        Equal(ConductorOverdriveRules.GetCascadeTargetCount(9), 3, "Conductor mastery IX Cascade target count");
        Near(ConductorOverdriveRules.GetCascadeDamageMultiplier(), 0.55f, 0f, "Conductor Cascade damage");
    }

    private static void CheckLevelUpChoiceRules()
    {
        Equal(LevelUpChoiceRules.GetOracleForecastCount(1), 1, "Oracle forecast before mastery IV");
        Equal(LevelUpChoiceRules.GetOracleForecastCount(3), 1, "Oracle forecast remains one before mastery IV");
        Equal(LevelUpChoiceRules.GetOracleForecastCount(4), 2, "Oracle mastery IV forecasts two choices");
        Equal(LevelUpChoiceRules.GetOracleRerollPreservationCount(6), 1, "Oracle mastery VI preserves one forecast");
        Equal(LevelUpChoiceRules.GetOracleRerollPreservationCount(8), 1, "Oracle preserves one forecast before mastery IX");
        Equal(LevelUpChoiceRules.GetOracleRerollPreservationCount(9), 2, "Oracle mastery IX preserves two forecasts");
        True(LevelUpChoiceRules.CanLockChoice(3, 1, false), "Lock is available for three choices");
        True(!LevelUpChoiceRules.CanLockChoice(2, 1, false), "Lock requires three choices");
        True(!LevelUpChoiceRules.CanLockChoice(3, 0, false), "Lock is one-use");
        True(!LevelUpChoiceRules.CanLockChoice(3, 1, true), "Only one choice can be reserved");
        True(LevelUpChoiceRules.CanRerollChoices(3, 1), "Reroll is available for three choices");
        True(!LevelUpChoiceRules.CanRerollChoices(2, 1), "Reroll requires three choices");
        True(!LevelUpChoiceRules.CanRerollChoices(3, 0), "Reroll is one-use");
        True(LevelUpChoiceRules.ShouldPreserveOracleForecast(true, 6, false, false, true),
            "Oracle mastery VI keeps a forecast card on its first unlocked reroll");
        True(!LevelUpChoiceRules.ShouldPreserveOracleForecast(false, 9, false, false, true),
            "Non-Oracle characters do not preserve forecast cards");
        True(!LevelUpChoiceRules.ShouldPreserveOracleForecast(true, 5, false, false, true),
            "Oracle reroll preservation is locked before mastery VI");
        True(!LevelUpChoiceRules.ShouldPreserveOracleForecast(true, 9, true, false, true),
            "Oracle forecast preservation can occur only once per run");
        True(!LevelUpChoiceRules.ShouldPreserveOracleForecast(true, 9, false, true, true),
            "Locked rerolls use the lock-first replacement path");
        True(!LevelUpChoiceRules.ShouldPreserveOracleForecast(true, 9, false, false, false),
            "Oracle preservation requires a forecasted choice");
        True(!LevelUpChoiceRules.ShouldShowOracleSourceFlash(6, true),
            "Oracle source flash is locked before mastery VII");
        True(LevelUpChoiceRules.ShouldShowOracleSourceFlash(7, true),
            "Oracle mastery VII shows the source flash");
        True(!LevelUpChoiceRules.ShouldShowOracleSourceFlash(9, false),
            "Oracle source flash requires choices");
    }

    private static void CheckFormationHitRules()
    {
        Equal(FormationHitRules.ResonanceRuneHitIncrement, 2, "Rune of Resonance adds two formation-charge hits");
        Near(FormationHitRules.ResonanceRunePulseSeconds, 0.5f, 0f, "Rune of Resonance extends pulse to 0.5 seconds");
        Equal(FormationHitRules.EchoArchitectureRelayHitIncrement, 2, "Echo Architecture relay adds two charge hits to linked Sphere");
        Near(FormationHitRules.EchoArchitectureRelayPulseSeconds, 0.35f, 0f, "Echo Architecture relay pulse duration");
        Near(FormationHitRules.EchoArchitectureRelayLightningSeconds, 0.20f, 0f, "Echo Architecture relay lightning duration");
        True(!FormationHitRules.ShouldTriggerEchoArchitectureRelay(0), "Echo Architecture does not relay with an empty source counter");
        True(!FormationHitRules.ShouldTriggerEchoArchitectureRelay(4), "Echo Architecture does not relay below its modulo boundary");
        True(FormationHitRules.ShouldTriggerEchoArchitectureRelay(5), "Echo Architecture source condition triggers on a positive multiple of five");
        True(FormationHitRules.ShouldTriggerEchoArchitectureRelay(10), "Echo Architecture source condition remains true at ten");
        True(!FormationHitRules.HasCompletedEchoArchitecture(null), "Echo Architecture requires owned artifact state");
        True(!FormationHitRules.HasCompletedEchoArchitecture(new List<string> { "echo_conductor", "network_relay" }),
            "Echo Relay synergy alone is not a completed set");
        True(!FormationHitRules.HasCompletedEchoArchitecture(new List<string> { "unified_mind", "zero_sphere" }),
            "Unified Core synergy alone is not a completed set");
        True(!FormationHitRules.HasCompletedEchoArchitecture(new List<string> { "echo_conductor", "network_relay", "unified_mind" }),
            "Echo Architecture remains incomplete when one required synergy artifact is missing");
        var echoPair = new List<string> { "echo_conductor", "network_relay" };
        var unifiedPair = new List<string> { "unified_mind", "zero_sphere" };
        True(FormationHitRules.HasEchoRelaySynergy(echoPair), "Echo Relay synergy requires its exact source pair");
        True(FormationHitRules.HasUnifiedCoreSynergy(unifiedPair), "Unified Core synergy requires its exact source pair");
        True(FormationHitRules.HasCompletedEchoArchitecture(new List<string>
            { "echo_conductor", "network_relay", "unified_mind", "zero_sphere" }),
            "Echo Architecture completes only when both source pair synergies are owned");

        Near(FormationHitRules.GetEchoRelayDamageMultiplier(true, 1), 1f, 0f,
            "Echo Relay does not bonus a single active Sphere type");
        Near(FormationHitRules.GetEchoRelayDamageMultiplier(true, 2), 1.08f, 0f,
            "Echo Relay grants +8% damage with two active Sphere types");
        Near(FormationHitRules.GetEchoRelayDamageMultiplier(false, 3), 1f, 0f,
            "Echo Relay requires the source synergy even with a mixed network");
        Near(FormationHitRules.GetEchoArchitectureDamageMultiplier(true, 1, true), 1f, 0f,
            "Echo Architecture completion bonus requires two active Sphere types");
        Near(FormationHitRules.GetEchoArchitectureDamageMultiplier(true, 2, false), 1f, 0f,
            "Echo Architecture completion bonus requires a neighbouring Sphere within 220 px");
        Near(FormationHitRules.GetEchoArchitectureDamageMultiplier(true, 2, true), 1.06f, 0f,
            "Echo Architecture completed set grants +6% inside its source range");
        Near(FormationHitRules.GetEchoArchitectureDamageMultiplier(false, 3, true), 1f, 0f,
            "Echo Architecture proximity bonus requires the complete source set");
        Near(FormationHitRules.GetUnifiedCoreDamageMultiplier(true, 3, 7), 1.35f, 0f,
            "Unified Core grants +5% per strongest-Sphere level to lower-level Spheres");
        Near(FormationHitRules.GetUnifiedCoreDamageMultiplier(true, 7, 7), 1f, 0f,
            "Unified Core does not boost the strongest Sphere itself");
        Near(FormationHitRules.GetUnifiedCoreDamageMultiplier(false, 2, 7), 1f, 0f,
            "Unified Core boost requires both source artifacts");
    }

    private static void CheckArtifactSynergyRules()
    {
        True(ArtifactSynergyRules.HasFortressNetworkSynergy(new List<string> { "heavy_core", "resonance_core" }),
            "Fortress Network requires Heavy Core and Resonance Core");
        True(!ArtifactSynergyRules.HasFortressNetworkSynergy(new List<string> { "heavy_core" }),
            "Fortress Network does not activate from one artifact");
        Near(ArtifactSynergyRules.GetFortressNetworkDamageMultiplier(true, false), 1f, 0f,
            "Fortress Network requires another nearby Sphere");
        Near(ArtifactSynergyRules.GetFortressNetworkDamageMultiplier(true, true), 1.10f, 0f,
            "Fortress Network grants +10% near a Sphere");
        Near(ArtifactSynergyRules.GetFortressNetworkDamageMultiplier(false, true), 1f, 0f,
            "Fortress Network requires its pair even when Spheres are nearby");

        True(ArtifactSynergyRules.HasGlassCannonSynergy(new List<string> { "overclock", "void_contract" }),
            "Glass Cannon requires Overclock and Void Contract");
        Near(ArtifactSynergyRules.GetGlassCannonDamageMultiplier(true), 1.12f, 0f,
            "Glass Cannon grants +12% Sphere damage");
        Near(ArtifactSynergyRules.GetGlassCannonDamageMultiplier(false), 1f, 0f,
            "Glass Cannon damage bonus requires both artifacts");
        Near(ArtifactSynergyRules.GetGlassCannonDamageTakenMultiplier(true), 1.05f, 0f,
            "Glass Cannon adds +5% incoming damage");
        Near(ArtifactSynergyRules.GetGlassCannonDamageTakenMultiplier(false), 1f, 0f,
            "Incoming damage penalty is absent without the Glass Cannon synergy");

        True(ArtifactSynergyRules.HasSingularitySynergy(new List<string> { "lone_bastion", "singularity_engine" }),
            "Singularity requires Lone Bastion and Singularity Engine");
        Near(ArtifactSynergyRules.GetSingularityDamageMultiplier(true, 1), 1.20f, 0f,
            "Singularity grants +20% at one active Sphere type");
        Near(ArtifactSynergyRules.GetSingularityDamageMultiplier(true, 2), 1f, 0f,
            "Singularity synergy is inactive above one active Sphere type");
        Near(ArtifactSynergyRules.GetSingularityDamageMultiplier(false, 1), 1f, 0f,
            "Singularity bonus requires both artifacts");

        True(ArtifactSynergyRules.HasCoreRelaySynergy(new List<string> { "unified_mind", "echo_thread" }),
            "Core Relay requires Unified Mind and Echo Thread");
        Near(ArtifactSynergyRules.GetCoreRelayDamageMultiplier(true, 1), 1f, 0f,
            "Core Relay requires at least two active Sphere types");
        Near(ArtifactSynergyRules.GetCoreRelayDamageMultiplier(true, 2), 1.08f, 0f,
            "Core Relay grants +8% to a mixed Sphere network");
        Near(ArtifactSynergyRules.GetCoreRelayDamageMultiplier(false, 3), 1f, 0f,
            "Core Relay requires both artifacts");
    }

    private static void CheckSphereElementalRules()
    {
        var branchElements = new Dictionary<string, SphereElement>
        {
            ["standard_resonator"] = SphereElement.Fire, ["standard_singularity"] = SphereElement.Freeze, ["standard_swarm"] = SphereElement.Poison,
            ["sniper_oracle"] = SphereElement.Poison, ["sniper_assassin"] = SphereElement.Fire, ["sniper_beacon"] = SphereElement.Freeze,
            ["shotgun_burst"] = SphereElement.Fire, ["shotgun_cataclysm"] = SphereElement.Poison, ["shotgun_hail"] = SphereElement.Freeze,
            ["orbital_dance"] = SphereElement.Poison, ["orbital_halo"] = SphereElement.Freeze, ["orbital_blade"] = SphereElement.Fire,
            ["prism_split"] = SphereElement.Poison, ["prism_mirror"] = SphereElement.Freeze,
            ["void_hunger"] = SphereElement.Poison, ["void_reaper"] = SphereElement.Fire, ["void_execution"] = SphereElement.Freeze,
            ["chain_web"] = SphereElement.Freeze, ["chain_storm"] = SphereElement.Fire, ["chain_leech"] = SphereElement.Poison,
            ["aura_sanctum"] = SphereElement.Freeze, ["aura_gravity"] = SphereElement.Fire, ["aura_overgrowth"] = SphereElement.Poison,
            ["gravity_well"] = SphereElement.Freeze, ["gravity_tide"] = SphereElement.Fire, ["gravity_collapse"] = SphereElement.Poison,
            ["pulse_wave"] = SphereElement.Freeze, ["pulse_resonator"] = SphereElement.Fire, ["pulse_burst"] = SphereElement.Poison
        };
        foreach (var entry in branchElements)
            Equal(SphereElementalRules.ResolveElement(entry.Key), entry.Value, "source element mapping: " + entry.Key);

        Equal(SphereElementalRules.ResolveElement("prism_spectrum", "prism_spectrum_final_1"), SphereElement.Fire, "Prism Spectrum final I uses Fire");
        Equal(SphereElementalRules.ResolveElement("prism_spectrum", "prism_spectrum_final_2"), SphereElement.Freeze, "Prism Spectrum final II uses Freeze");
        Equal(SphereElementalRules.ResolveElement("prism_spectrum", "prism_spectrum_final_3"), SphereElement.Poison, "Prism Spectrum final III uses Poison");
        Equal(SphereElementalRules.GetMasteryId("standard_resonator", "standard_resonator_final_1"), "fire_power", "Fire mastery power");
        Equal(SphereElementalRules.GetMasteryId("standard_resonator", "standard_resonator_final_2"), "fire_tempo", "Fire mastery tempo");
        Equal(SphereElementalRules.GetMasteryId("standard_singularity", "standard_singularity_final_3"), "freeze_permafrost", "Freeze mastery Permafrost");
        Equal(SphereElementalRules.GetMasteryId("chain_storm", "chain_storm_final_2"), "conduction_rate", "Chain Conduction Rate");
        Equal(SphereElementalRules.GetMasteryId("gravity_well", "gravity_well_final_3"), "field_duration", "Field Duration mastery");

        var fire = SphereElementalRules.CreateStatus(SphereElement.Fire, null, SphereElementalMode.Direct);
        Near(fire.Duration, 3f, 0f, "source Fire duration");
        Near(fire.DamagePerSecond, 8f, 0f, "source Fire DPS");
        Near(SphereElementalRules.CreateStatus(SphereElement.Fire, "fire_power", SphereElementalMode.Direct).DamagePerSecond, 10.8f, 0.0001f, "Fire Power plus 35%");
        var rapidFire = SphereElementalRules.CreateStatus(SphereElement.Fire, "fire_tempo", SphereElementalMode.Direct);
        Near(rapidFire.TickInterval, 0.12f, 0f, "Fire Tempo tick interval");
        Near(rapidFire.DamagePerSecond, 8f, 0f, "Fire Tempo preserves DPS");
        Near(SphereElementalRules.CreateStatus(SphereElement.Fire, "fire_duration", SphereElementalMode.Direct).Duration, 4.35f, 0.0001f, "Fire Duration");

        Near(SphereElementalRules.CreateStatus(SphereElement.Freeze, null, SphereElementalMode.Direct).Duration, 0.8f, 0.0001f, "source Freeze duration");
        Near(SphereElementalRules.CreateStatus(SphereElement.Freeze, "freeze_duration", SphereElementalMode.Direct).Duration, 1.12f, 0.0001f, "Freeze Duration");
        True(SphereElementalRules.CreateStatus(SphereElement.Freeze, "freeze_impact", SphereElementalMode.Direct).FreezeVulnerability, "Freeze Impact same-Sphere vulnerability");
        var permafrost = SphereElementalRules.CreateStatus(SphereElement.Freeze, "freeze_permafrost", SphereElementalMode.Direct);
        Near(permafrost.AdditionalSlowDuration, 1f, 0f, "Permafrost post-freeze slow duration");
        Near(permafrost.AdditionalSlowMultiplier, 0.72f, 0f, "Permafrost slow multiplier");

        var poison = SphereElementalRules.CreateStatus(SphereElement.Poison, null, SphereElementalMode.Direct);
        Near(poison.Duration, 4f, 0f, "source Poison duration");
        Near(poison.DamagePerSecond, 5f, 0f, "source Poison DPS");
        Near(SphereElementalRules.CreateStatus(SphereElement.Poison, "poison_power", SphereElementalMode.Direct).DamagePerSecond, 6.75f, 0.0001f, "Poison Power plus 35%");
        Near(SphereElementalRules.CreateStatus(SphereElement.Poison, "poison_tempo", SphereElementalMode.Direct).TickInterval, 0.12f, 0f, "Poison Tempo tick interval");
        Near(SphereElementalRules.CreateStatus(SphereElement.Poison, "poison_duration", SphereElementalMode.Direct).Duration, 5.8f, 0.0001f, "Poison Duration");

        Equal(SphereElementalRules.GetConductionThreshold(null), 3, "base Chain reaction threshold");
        Equal(SphereElementalRules.GetConductionThreshold("conduction_rate"), 2, "Conduction Rate threshold");
        Near(SphereElementalRules.GetConductionCooldown(null), 2f, 0f, "base Chain reaction cooldown");
        Near(SphereElementalRules.GetConductionCooldown("conduction_rate"), 1.4f, 0f, "Conduction Rate cooldown");
        Near(SphereElementalRules.GetConductionDamageMultiplier(null), 0.20f, 0f, "base Chain reaction damage");
        Near(SphereElementalRules.GetConductionDamageMultiplier("conduction_power"), 0.27f, 0f, "Conduction Power reaction damage");
        Near(SphereElementalRules.CreateStatus(SphereElement.Fire, "conduction_power", SphereElementalMode.Conduction).DamagePerSecond, 10.8f, 0.0001f, "Conduction Power Fire status");
        Near(SphereElementalRules.CreateStatus(SphereElement.Fire, "conduction_duration", SphereElementalMode.Conduction).Duration, 4.35f, 0.0001f, "Conduction Duration");
        Near(SphereElementalRules.GetFieldCooldown(null), 1.8f, 0f, "base field reaction cooldown");
        Near(SphereElementalRules.GetFieldCooldown("field_frequency"), 1.25f, 0f, "Field Frequency cooldown");
        var fieldFreeze = SphereElementalRules.CreateStatus(SphereElement.Freeze, "field_power", SphereElementalMode.Field);
        Near(fieldFreeze.Duration, 1.08f, 0.0001f, "Field Power freeze duration");
        Near(fieldFreeze.AdditionalSlowDuration, 0.8f, 0f, "Field Power control duration");
        Near(fieldFreeze.AdditionalSlowMultiplier, 0.62f, 0f, "Field Power control multiplier");
        Near(SphereElementalRules.CreateStatus(SphereElement.Poison, "field_power", SphereElementalMode.Field).DamagePerSecond, 6.75f, 0.0001f, "Field Power poison status");
        Near(SphereElementalRules.CreateStatus(SphereElement.Poison, "field_duration", SphereElementalMode.Field).Duration, 5.8f, 0.0001f, "Field Duration poison status");
        Near(SphereElementalRules.CreateStatus(SphereElement.Fire, null, SphereElementalMode.Direct, 1.30f, 1.15f).Duration, 3.9f, 0.0001f, "Alchemist source status duration multiplier");
        Near(SphereElementalRules.CreateStatus(SphereElement.Fire, null, SphereElementalMode.Direct, 1f, 1.15f).DamagePerSecond, 9.2f, 0.0001f, "Alchemist source status damage multiplier");
    }

    private static void CheckAlchemistReactionRules()
    {
        Equal(AlchemistReactionRules.Resolve(true, false, true), AlchemistReactionKind.ToxinIgnition,
            "Fire + Poison creates Toxin Ignition");
        Equal(AlchemistReactionRules.Resolve(false, true, true), AlchemistReactionKind.Cryotoxin,
            "Freeze + Poison creates Cryotoxin");
        Equal(AlchemistReactionRules.Resolve(true, true, false), AlchemistReactionKind.ThermalShock,
            "Fire + Freeze creates Thermal Shock");
        Equal(AlchemistReactionRules.Resolve(false, false, true), AlchemistReactionKind.None,
            "one elemental status cannot react alone");
        Equal(AlchemistReactionRules.Resolve(true, true, true), AlchemistReactionKind.ToxinIgnition,
            "when all three statuses coexist source priority consumes Fire + Poison first");

        Near(AlchemistReactionRules.GetBaseDamage(1), 22f, 0f, "source Alchemist burst damage at player level I");
        Near(AlchemistReactionRules.GetBaseDamage(10), 40f, 0f, "source Alchemist burst damage scales with player level");
        Near(AlchemistReactionRules.GetBurstMultiplier(AlchemistReactionKind.ToxinIgnition, 1, 1), 2f, 0f,
            "Toxin Ignition base multiplier");
        Near(AlchemistReactionRules.GetBurstMultiplier(AlchemistReactionKind.Cryotoxin, 1, 1), 1.6f, 0f,
            "Cryotoxin base multiplier");
        Near(AlchemistReactionRules.GetBurstMultiplier(AlchemistReactionKind.ThermalShock, 1, 1), 1.8f, 0f,
            "Thermal Shock base multiplier");
        Near(AlchemistReactionRules.GetBurstMultiplier(AlchemistReactionKind.ToxinIgnition, 7, 1), 2.1f, 0.0001f,
            "Alchemist mastery VII burst bonus");
        Near(AlchemistReactionRules.GetBurstMultiplier(AlchemistReactionKind.ToxinIgnition, 9, 3), 2.625f, 0.0001f,
            "Alchemist mastery IX boosts every third reaction");
        Near(AlchemistReactionRules.GetBurstMultiplier(AlchemistReactionKind.ToxinIgnition, 9, 2), 2.1f, 0.0001f,
            "Alchemist mastery IX leaves non-third reactions at the normal mastery bonus");
        Near(AlchemistReactionRules.GetBurstMultiplier(AlchemistReactionKind.None, 10, 3), 0f, 0f,
            "no reaction yields no burst damage");

        Equal(AlchemistReactionRules.GetReactionRadiusSourcePixels(1), 55, "Alchemist base reaction radius");
        Equal(AlchemistReactionRules.GetReactionRadiusSourcePixels(2), 60, "Alchemist mastery II reaction radius");
        Equal(AlchemistReactionRules.GetReactionRadiusSourcePixels(6), 70, "Alchemist mastery VI reaction radius");
        Equal(AlchemistReactionRules.GetSplashTargetCount(3), 0, "Alchemist splash locked before mastery IV");
        Equal(AlchemistReactionRules.GetSplashTargetCount(4), 1, "Alchemist mastery IV splashes one nearby enemy");
        Equal(AlchemistReactionRules.GetSplashTargetCount(9), 1, "Alchemist splash remains one target before mastery X");
        Equal(AlchemistReactionRules.GetSplashTargetCount(10), 2, "Alchemist mastery X splashes two nearby enemies");
        Near(AlchemistReactionRules.GetSplashDamageMultiplier(), 0.5f, 0f, "Alchemist splash damage is half the burst");
        Near(AlchemistReactionRules.GetSlowDuration(AlchemistReactionKind.Cryotoxin), 2f, 0f, "Cryotoxin slow duration");
        Near(AlchemistReactionRules.GetSlowMultiplier(AlchemistReactionKind.Cryotoxin), 0.35f, 0f, "Cryotoxin slow strength");
        Near(AlchemistReactionRules.GetSlowDuration(AlchemistReactionKind.ThermalShock), 2f, 0f, "Thermal Shock slow duration");
        Near(AlchemistReactionRules.GetSlowMultiplier(AlchemistReactionKind.ThermalShock), 0.40f, 0f, "Thermal Shock slow strength");
        Near(AlchemistReactionRules.GetSlowDuration(AlchemistReactionKind.ToxinIgnition), 0f, 0f, "Toxin Ignition does not add a slow");
        Near(AlchemistReactionRules.GetCatalystDurationSeconds(4), 0f, 0f, "Catalyst locked before mastery V");
        Near(AlchemistReactionRules.GetCatalystDurationSeconds(5), 2f, 0f, "mastery V Catalyst window");
    }

    private static void CheckSphereAuthoredModifierRules()
    {
        Equal(SphereAuthoredModifierRules.BranchCount, 30, "all 30 source Mutation I branches have modifier data");
        Equal(SphereAuthoredModifierRules.FinalChoiceCount, 90, "all 90 source Mutation II choices have modifier data");
        Equal(SphereAuthoredModifierRules.AllModifiers.Count, 24, "full source Sphere modifier set is represented");

        Equal(SphereAuthoredModifierRules.GetLevel("standard_resonator", null, SphereAuthoredModifier.Resonant), 1,
            "Standard Resonator base Resonant modifier");
        Equal(SphereAuthoredModifierRules.GetLevel("standard_resonator", "standard_resonator_final_2", SphereAuthoredModifier.Impact), 1,
            "Standard Resonator final II adds Impact");
        Equal(SphereAuthoredModifierRules.GetLevel("standard_singularity", "standard_singularity_final_1", SphereAuthoredModifier.Gravitic), 2,
            "Standard Singularity final I raises inherited Gravitic to II");
        Equal(SphereAuthoredModifierRules.GetLevel("sniper_oracle", "sniper_oracle_final_2", SphereAuthoredModifier.Resonant), 1,
            "Sniper Oracle final II adds Resonant");
        Equal(SphereAuthoredModifierRules.GetLevel("sniper_assassin", "sniper_assassin_final_2", SphereAuthoredModifier.Execute), 2,
            "Sniper Assassin final II sets Execute II");
        Equal(SphereAuthoredModifierRules.GetLevel("sniper_beacon", "sniper_beacon_final_2", SphereAuthoredModifier.Mark), 2,
            "Sniper Beacon final II sets Mark II");
        Equal(SphereAuthoredModifierRules.GetLevel("shotgun_burst", "shotgun_burst_final_1", SphereAuthoredModifier.Multishot), 2,
            "Shotgun Burst final I sets Multishot II");
        Equal(SphereAuthoredModifierRules.GetLevel("shotgun_cataclysm", "shotgun_cataclysm_final_3", SphereAuthoredModifier.Impact), 2,
            "Shotgun Cataclysm final III has Impact II");
        Equal(SphereAuthoredModifierRules.GetLevel("shotgun_hail", "shotgun_hail_final_2", SphereAuthoredModifier.Multishot), 2,
            "Shotgun Hail final II has Multishot II");
        Equal(SphereAuthoredModifierRules.GetLevel("chain_web", "chain_web_final_3", SphereAuthoredModifier.Static), 1,
            "Chain Web final III adds Static");
        Equal(SphereAuthoredModifierRules.GetLevel("chain_storm", "chain_storm_final_1", SphereAuthoredModifier.Static), 2,
            "Chain Storm final I sets Static II");
        Equal(SphereAuthoredModifierRules.GetLevel("chain_leech", "chain_leech_final_2", SphereAuthoredModifier.Drain), 1,
            "Chain Leech final II adds Drain");
        Equal(SphereAuthoredModifierRules.GetLevel("aura_sanctum", "aura_sanctum_final_2", SphereAuthoredModifier.Anchor), 2,
            "Aura Sanctum final II sets Anchor II");
        Equal(SphereAuthoredModifierRules.GetLevel("aura_gravity", "aura_gravity_final_1", SphereAuthoredModifier.Gravitic), 2,
            "Aura Gravity final I sets Gravitic II");
        Equal(SphereAuthoredModifierRules.GetLevel("orbital_dance", "orbital_dance_final_3", SphereAuthoredModifier.Afterimage), 3,
            "Orbital Dance final III sets Afterimage III");
        Equal(SphereAuthoredModifierRules.GetLevel("orbital_halo", "orbital_halo_final_3", SphereAuthoredModifier.Resonant), 2,
            "Orbital Halo final III sets Resonant II");
        Equal(SphereAuthoredModifierRules.GetLevel("orbital_blade", "orbital_blade_final_3", SphereAuthoredModifier.Impact), 2,
            "Orbital Blade final III keeps Impact II");
        Equal(SphereAuthoredModifierRules.GetLevel("prism_split", "prism_split_final_2", SphereAuthoredModifier.Pierce), 2,
            "Prism Split final II sets Pierce II");
        Equal(SphereAuthoredModifierRules.GetLevel("prism_spectrum", "prism_spectrum_final_3", SphereAuthoredModifier.Poison), 1,
            "Prism Spectrum final III switches to Poison");
        Equal(SphereAuthoredModifierRules.GetLevel("prism_mirror", "prism_mirror_final_3", SphereAuthoredModifier.Echo), 1,
            "Prism Mirror final III adds Echo");
        Equal(SphereAuthoredModifierRules.GetLevel("gravity_well", "gravity_well_final_1", SphereAuthoredModifier.Anchor), 2,
            "Gravity Well final I sets Anchor II");
        Equal(SphereAuthoredModifierRules.GetLevel("gravity_tide", "gravity_tide_final_2", SphereAuthoredModifier.Gravitic), 2,
            "Gravity Tide final II sets Gravitic II");
        Equal(SphereAuthoredModifierRules.GetLevel("gravity_collapse", "gravity_collapse_final_3", SphereAuthoredModifier.Execute), 2,
            "Gravity Collapse final III sets Execute II");
        Equal(SphereAuthoredModifierRules.GetLevel("pulse_wave", "pulse_wave_final_2", SphereAuthoredModifier.Freeze), 1,
            "Pulse Wave final II adds Freeze");
        Equal(SphereAuthoredModifierRules.GetLevel("pulse_resonator", "pulse_resonator_final_2", SphereAuthoredModifier.Impact), 1,
            "Pulse Resonator final II adds Impact");
        Equal(SphereAuthoredModifierRules.GetLevel("pulse_burst", "pulse_burst_final_3", SphereAuthoredModifier.Shatter), 2,
            "Pulse Burst final III sets Shatter II");
        Equal(SphereAuthoredModifierRules.GetLevel("void_hunger", "void_hunger_final_2", SphereAuthoredModifier.Corrupt), 2,
            "Void Hunger final II sets Corrupt II");
        Equal(SphereAuthoredModifierRules.GetLevel("void_reaper", "void_reaper_final_2", SphereAuthoredModifier.Drain), 2,
            "Void Reaper final II sets Drain II");
        Equal(SphereAuthoredModifierRules.GetLevel("void_execution", "void_execution_final_3", SphereAuthoredModifier.Phase), 2,
            "Void Execution final III sets Phase II");

        Near(SphereAuthoredModifierRules.GetDamageMultiplier("shotgun_cataclysm", "shotgun_cataclysm_final_3"),
            1.08f, 0.0001f, "Shatter II scales outgoing damage by source 8%");
        Near(SphereAuthoredModifierRules.GetDamageMultiplier("sniper_beacon", "sniper_beacon_final_2"),
            1.08f, 0.0001f, "Mark II contributes the source global 8% damage channel");
        Near(SphereAuthoredModifierRules.GetDamageMultiplier("standard_swarm", null), 1f, 0f,
            "branches without authored damage modifiers stay at baseline");
        Equal(SphereAuthoredModifierRules.GetLevel("unknown_branch", "unknown_branch_final_2", SphereAuthoredModifier.Pierce), 0,
            "unknown branches have no invented modifier levels");
    }

    private static void CheckSphereAuthoredHitEffects()
    {
        Near(SphereAuthoredModifierRules.GetExecuteDamageMultiplier(2, 0.30f), 1.36f, 0.0001f,
            "Execute II threshold includes exactly 30% HP");
        Near(SphereAuthoredModifierRules.GetExecuteDamageMultiplier(2, 0.3001f), 1f, 0f,
            "Execute does not trigger above 30% HP");
        Near(SphereAuthoredModifierRules.GetMarkedDamageMultiplier(2, true), 1.30f, 0.0001f,
            "Mark II increases damage against a marked target");
        Near(SphereAuthoredModifierRules.GetMarkedDamageMultiplier(2, false), 1f, 0f,
            "Mark has no conditional bonus before a target is marked");
        Near(SphereAuthoredModifierRules.GetCorruptDamageMultiplier(2, 1), 1.04f, 0.0001f,
            "Corrupt first stack adds 4% damage");
        Near(SphereAuthoredModifierRules.GetCorruptDamageMultiplier(2, 5), 1.20f, 0.0001f,
            "Corrupt caps at five stacks");
        Near(SphereAuthoredModifierRules.GetImpactDistancePixels(2), 36f, 0f,
            "Impact II pushes by 36 source pixels");
        Near(SphereAuthoredModifierRules.GetGraviticRangePixels(), 72f, 0f,
            "Gravitic source range is 72 pixels");
        Near(SphereAuthoredModifierRules.GetGraviticPullDistancePixels(2), 8f, 0f,
            "Gravitic II pull is eight source pixels");
        Near(SphereAuthoredModifierRules.GetAnchorSlowDuration(2), 1.3f, 0.0001f,
            "Anchor II duration");
        Near(SphereAuthoredModifierRules.GetAnchorSlowMultiplier(2), 0.55f, 0.0001f,
            "Anchor II strength");
        Near(SphereAuthoredModifierRules.GetVampiricHealFraction(2), 0.012f, 0.0001f,
            "Vampiric II heals 1.2% of dealt damage");
        Equal(SphereAuthoredModifierRules.GetResonantCharge(2), 2,
            "Resonant II adds two charge");
        Near(SphereAuthoredModifierRules.GetShatterRadiusPixels(), 48f, 0f,
            "Shatter source splash radius");
        Near(SphereAuthoredModifierRules.GetShatterDamageMultiplier(2), 0.36f, 0.0001f,
            "Shatter II burst is 36% of actual primary damage");
        Near(SphereAuthoredModifierRules.GetSplitChance(1), 0.18f, 0.0001f, "Split I chance");
        Near(SphereAuthoredModifierRules.GetSplitChance(3), 0.45f, 0.0001f, "Split chance cap");
        Near(SphereAuthoredModifierRules.GetEchoChance(1), 0.12f, 0.0001f, "Echo I chance");
        Near(SphereAuthoredModifierRules.GetEchoChance(3), 0.35f, 0.0001f, "Echo chance cap");
        Near(SphereAuthoredModifierRules.GetStaticChance(1), 0.12f, 0.0001f, "Static I chance");
        Near(SphereAuthoredModifierRules.GetStaticChance(3), 0.35f, 0.0001f, "Static chance cap");
        Near(SphereAuthoredModifierRules.GetAfterimageChance(1), 0.16f, 0.0001f, "Afterimage I chance");
        Near(SphereAuthoredModifierRules.GetAfterimageChance(3), 0.40f, 0.0001f, "Afterimage chance cap");
        Near(SphereAuthoredModifierRules.GetAfterimageDamageMultiplier(1), 0.20f, 0.0001f, "Afterimage I damage");
        Near(SphereAuthoredModifierRules.GetAfterimageDamageMultiplier(4), 0.32f, 0.0001f, "Afterimage damage cap");
        Equal(SphereAuthoredModifierRules.GetProjectilePierceBonus("prism_split", "prism_split_final_2"), 2,
            "authored Prism Split Pierce II is applied once");
        Equal(SphereAuthoredModifierRules.GetProjectilePierceBonus("void_execution", "void_execution_final_3"), 2,
            "authored Void Execution Phase II is applied as pierce");
        Equal(SphereAuthoredModifierRules.GetProjectileRicochetCount("prism_mirror", "prism_mirror_final_3"), 2,
            "Prism Mirror authored Ricochet II");
        Equal(SphereAuthoredModifierRules.GetMultishotCount("shotgun_hail", "shotgun_hail_final_2"), 2,
            "Shotgun Hail authored Multishot II");
    }

    private static void CheckAuthoredProjectileRuntime()
    {
        Equal(SphereAuthoredModifierRules.GetProjectilePierceBonus("prism_split", "prism_split_final_2"), 2,
            "Prism Split Pierce II resolves once");
        Equal(SphereAuthoredModifierRules.GetProjectilePierceBonus("void_execution", "void_execution_final_2"), 1,
            "Void Execution Phase I resolves as one pierce");
        Equal(SphereAuthoredModifierRules.GetProjectilePierceBonus("void_execution", "void_execution_final_3"), 2,
            "Void Execution Phase II resolves as two pierces");
        Near(SphereAuthoredModifierRules.GetSplitChance(2), 0.36f, 0.0001f, "Split II chance");
        Near(SphereAuthoredModifierRules.GetEchoChance(2), 0.24f, 0.0001f, "Echo II chance");
        Near(SphereAuthoredModifierRules.GetStaticChance(2), 0.24f, 0.0001f, "Static II chance");
        Near(SphereAuthoredModifierRules.GetAfterimageChance(2), 0.32f, 0.0001f, "Afterimage II chance");
    }

    private static void CheckSphereNetworkRules()
    {
        var lineNodes = new[]
        {
            new SphereNetworkNode(new Vec2(0f, 0f)),
            new SphereNetworkNode(new Vec2(1f, 0f)),
            new SphereNetworkNode(new Vec2(2f, 0f))
        };
        var line = SphereNetworkRules.Analyze(lineNodes);
        True(line.Links.Count == 3, "Network links include every pair within the source-calibrated range");
        True(line.GetFormation(SphereNetworkFormation.Line) != null, "collinear Spheres form a Line");
        Equal(SphereNetworkRules.GetLinkedNodeIndexes(line, 1).Count, 2, "linked-neighbour lookup");
        True(SphereNetworkRules.AreNodesLinked(line, 0, 2), "link query is symmetric");

        var squareNodes = new[]
        {
            new SphereNetworkNode(new Vec2(0f, 0f)),
            new SphereNetworkNode(new Vec2(1f, 0f)),
            new SphereNetworkNode(new Vec2(1f, 1f)),
            new SphereNetworkNode(new Vec2(0f, 1f))
        };
        var square = SphereNetworkRules.Analyze(squareNodes);
        True(square.GetFormation(SphereNetworkFormation.Square) != null, "four balanced nodes form a Square");
        True(square.FormationCandidates.Exists(shape => shape.Type == SphereNetworkFormation.Ring), "closed loop forms a Ring");
        var latticeNodes = new[]
        {
            new SphereNetworkNode(new Vec2(0f, 0f)),
            new SphereNetworkNode(new Vec2(1f, 0f)),
            new SphereNetworkNode(new Vec2(0.5f, 0.8660254f)),
            new SphereNetworkNode(new Vec2(1.5f, 0.8660254f))
        };
        var lattice = SphereNetworkRules.Analyze(latticeNodes);
        True(lattice.FormationCandidates.Exists(shape => shape.Type == SphereNetworkFormation.Lattice), "two equilateral triangles sharing an edge form a Lattice");
        True(lattice.FormationCandidates.Exists(shape => shape.Type == SphereNetworkFormation.Fractal), "overlapping Lattice and Triangle form a Fractal");
        True(square.DominantFormation != null, "geometry dominance selected");
        True(SphereNetworkRules.TrySetDominantFormation(square, SphereNetworkFormation.Square), "manual dominant geometry selection succeeds");
        True(square.DominantFormation.Type == SphereNetworkFormation.Square, "manual dominant geometry is applied");
        True(SphereNetworkRules.TrySetSecondaryFormation(square, SphereNetworkFormation.Ring), "manual secondary geometry selection succeeds");
        True(square.SecondaryFormation.Type == SphereNetworkFormation.Ring, "manual secondary geometry is applied");
        Near(SphereNetworkRules.GetFormationBonusMultiplier(square, square.DominantFormation.Type, 0), 1f, 0f, "dominant formation full bonus");
        True(SphereNetworkRules.GetLinkedNodeIndexes(square, 0).Count == 3, "square graph retains both sides and diagonal");

        var disruptedNodes = new[]
        {
            new SphereNetworkNode(new Vec2(0f, 0f)),
            new SphereNetworkNode(new Vec2(1f, 0f)),
            new SphereNetworkNode(new Vec2(2f, 0f), true, 0.5f)
        };
        var disrupted = SphereNetworkRules.Analyze(disruptedNodes);
        Equal(disrupted.Nodes.Count, 2, "disabled node leaves active Network");
        Equal(disrupted.Links.Count, 1, "disabled node contributes no links");
        Near(SphereNetworkRules.GetFormationBonusMultiplier(disrupted, SphereNetworkFormation.Line), 0f, 0f, "inactive geometry grants no bonus");
    }

    private static void CheckResonanceEventContracts()
    {
        var charge = 99f;
        Equal(ResonanceRules.Add(ref charge, ResonanceRules.SphereHitCharge), 1, "Sphere hit triggers event at full Resonance");
        Near(charge, 0f, 0f, "Resonance event clears consumed charge");
        charge = 95f;
        Equal(ResonanceRules.Add(ref charge, ResonanceRules.GeometryCharge), 1, "new geometry can trigger event");
        Near(charge, 0f, 0f, "geometry event preserves remainder");
        True(SphereEvolutionCombatRules.GetOrbitalResonanceBonus(7, "orbital_halo_final_3") > SphereEvolutionCombatRules.GetOrbitalResonanceBonus(7, "orbital_halo_final_1"), "Halo final three increases Resonance charge");
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
