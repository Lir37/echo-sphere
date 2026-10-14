using System.Text.Json;
using EchoSphere.Core;

internal static class SourceParityChecks
{
    private static int _count;
    private static void Near(float actual, float expected, string label, float tolerance = 0.004f)
    { if (Math.Abs(actual - expected) > tolerance) throw new InvalidOperationException($"Source parity {label}: expected {expected}, got {actual}"); _count++; }
    private static Vec2 Position(JsonElement value) => new Vec2(value.GetProperty("x").GetSingle(), value.GetProperty("y").GetSingle());
    public static void Run()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "source-parity-fixtures.json")));
        var root = document.RootElement;
        foreach (var fixture in root.GetProperty("killExperience").EnumerateArray())
            Near(ComboRules.GetExperienceOrbValue(fixture.GetProperty("reward").GetSingle(), fixture.GetProperty("multiplier").GetSingle()), fixture.GetProperty("value").GetInt32(), "fractional kill XP");
        foreach (var fixture in root.GetProperty("collisions").EnumerateArray())
        {
            var input = fixture.GetProperty("input"); var bodies = new List<CollisionBody>(); var towers = new List<Vec2>();
            foreach (var enemy in input.GetProperty("enemies").EnumerateArray())
            {
                var type = enemy.GetProperty("type").GetString();
                bodies.Add(new CollisionBody { Position = Position(enemy.GetProperty("pos")), Radius = enemy.GetProperty("radius").GetSingle(), Speed = enemy.GetProperty("speed").GetSingle(), Mass = type == "boss" ? 8f : type == "tank" ? 2.5f : type == "fast" ? 0.7f : 1f, FollowBreach = enemy.GetProperty("role").GetString() == "phase" });
            }
            foreach (var tower in input.GetProperty("spheres").EnumerateArray()) towers.Add(Position(tower.GetProperty("pos")));
            var player = SpaceCollisionRules.Resolve(Position(input.GetProperty("player").GetProperty("pos")), bodies, towers, fixture.GetProperty("dt").GetSingle(), input.GetProperty("player").GetProperty("formationFollowActive").GetBoolean());
            var expectedPlayer = Position(fixture.GetProperty("player")); Near(player.X, expectedPlayer.X, "player X"); Near(player.Y, expectedPlayer.Y, "player Y");
            var index = 0;
            foreach (var expected in fixture.GetProperty("enemies").EnumerateArray())
            { var position = Position(expected); Near(bodies[index].Position.X, position.X, "enemy X"); Near(bodies[index++].Position.Y, position.Y, "enemy Y"); }
        }
        foreach (var fixture in root.GetProperty("levels").EnumerateArray())
        {
            var type = Enum.Parse<SphereId>(fixture.GetProperty("type").GetString(), true); var level = fixture.GetProperty("level").GetInt32();
            var stats = SphereLevelRules.GetStats(type, level); var mods = fixture.GetProperty("mods");
            Near(stats.DamageMultiplier, mods.GetProperty("damage").GetSingle(), type + " damage " + level, .000002f);
            Near(stats.RangeMultiplier * (type == SphereId.Orbital ? stats.OrbitRadiusMultiplier : 1f), mods.GetProperty("radius").GetSingle(), type + " radius " + level, .000002f);
            Near(stats.DelayMultiplier, mods.GetProperty("delay").GetSingle(), type + " delay " + level, .000002f);
            Near(stats.AuraPulseMultiplier * .5f, mods.GetProperty("auraPulse").GetSingle(), type + " cadence " + level, .000002f);
            Near(stats.AuraRadiusMultiplier, mods.GetProperty("auraRadius").GetSingle(), type + " aura radius " + level, .000002f);
        }
        foreach (var fixture in root.GetProperty("modifiers").EnumerateArray())
        {
            var type = Enum.Parse<SphereId>(fixture.GetProperty("type").GetString(), true);
            var branch = fixture.GetProperty("branch").GetString(); var index = fixture.GetProperty("final").GetInt32();
            var final = index < 0 ? "" : branch + "_final_" + (index + 1);
            var global = new Dictionary<string, int>(); foreach (var item in fixture.GetProperty("global").EnumerateObject()) global[item.Name] = item.Value.GetInt32();
            var actual = SourceSphereGlobalModifiers.Merge(SourceSphereModifiersRules.Get(type, fixture.GetProperty("level").GetInt32(), branch, final), type, branch, final, global, fixture.GetProperty("range").GetInt32());
            foreach (var field in fixture.GetProperty("mods").EnumerateObject())
                Near((float)typeof(SourceSphereModifiers).GetField(field.Name).GetValue(actual), field.Value.GetSingle(), type + " " + branch + " " + field.Name, .000004f);
        }
        foreach (var fixture in root.GetProperty("orbital").EnumerateArray())
        {
            var level = fixture.GetProperty("level").GetInt32();
            var inner = SphereEvolutionCombatRules.GetOrbitalInnerCount(level, "");
            var outer = SphereEvolutionCombatRules.GetOrbitalOuterCount(level, "");
            for (var extra = 0; extra < fixture.GetProperty("extras").GetInt32(); extra++)
            { if (outer < inner) outer++; else if (inner < outer) inner++; else if (extra % 2 == 0) outer++; else inner++; }
            Near(inner, fixture.GetProperty("inner").GetInt32(), "Orbital inner"); Near(outer, fixture.GetProperty("outer").GetInt32(), "Orbital outer");
        }
        foreach (var fixture in root.GetProperty("mastery").EnumerateArray())
        {
            var branch = fixture.GetProperty("branch").GetString(); var final = fixture.GetProperty("final").GetInt32();
            var actual = SphereElementalRules.GetMasteryId(branch, branch + "_final_" + (final + 1));
            if (actual != fixture.GetProperty("id").GetString()) throw new InvalidOperationException("Source elemental mastery differs: " + branch + " " + final); _count++;
        }
        foreach (var fixture in root.GetProperty("seeds").EnumerateArray())
        {
            var actual = PlayerLoopRules.CreateRunSeed(fixture.GetProperty("player").GetString(), fixture.GetProperty("difficulty").GetString(), fixture.GetProperty("epoch").GetInt64());
            if (actual != fixture.GetProperty("seed").GetUInt32()) throw new InvalidOperationException("Source run seed differs"); _count++;
        }
        Near(PlayerLoopRules.MutationDamage(PlayerLoopRules.MutationStage(10)), 1.1f, "mutation I");
        Near(PlayerLoopRules.MutationRadius(PlayerLoopRules.MutationStage(20)), 1.15f, "mutation II");
        Near(PlayerLoopRules.MutationSpeed(PlayerLoopRules.MutationStage(30)), 1.2f, "mutation III");
        Near(PlayerLoopRules.MutationRegen(PlayerLoopRules.MutationStage(40)), 1.25f, "mutation IV");
        IList<int> live = new SourceEntityList<int> { 1, 2 };
        var visited = new List<int>(); foreach (var item in live) { visited.Add(item); if (item == 1) live.Add(3); }
        if (!visited.SequenceEqual(new[] { 1, 2, 3 })) throw new InvalidOperationException("Interface enumeration must preserve source live-array appends");
        var zeroRng = new SeededRng(1); zeroRng.RestoreState(0);
        var restoredRng = (SeededRng)RunSnapshotCodec.Restore(RunSnapshotCodec.Capture(zeroRng), typeof(SeededRng));
        if (restoredRng.State != 0 || restoredRng.NextFloat() != zeroRng.NextFloat()) throw new InvalidOperationException("Raw zero RNG state must survive a saved run");
        var state = new SnapshotFixture { xp = 2.75f, seed = 4294967295, nullable = SphereId.Pulse, geometry = new Vec2(12.5f, -7f) };
        state.levels[SphereId.Pulse] = 7; state.ids.Add("pulse"); state.choices.Add(new SphereEvolutionOption("id", "name", "description"));
        var options = new JsonSerializerOptions { IncludeFields = true };
        var raw = JsonSerializer.Serialize(RunSnapshotCodec.Capture(state), options);
        var encoded = JsonSerializer.Deserialize<SnapshotValue>(raw, options);
        var restored = (SnapshotFixture)RunSnapshotCodec.Restore(encoded, typeof(SnapshotFixture), new SnapshotFixture());
        Near(restored.xp, state.xp, "fractional snapshot XP"); Near(restored.geometry.Y, state.geometry.Y, "snapshot readonly Vec2");
        if (restored.seed != state.seed || restored.nullable != SphereId.Pulse || restored.levels[SphereId.Pulse] != 7 || restored.ids[0] != "pulse" || restored.choices[0].Id != "id") throw new InvalidOperationException("Snapshot collection/state roundtrip failed");
        Console.WriteLine($"Direct TypeScript → C# parity: PASS ({_count} comparisons, snapshot collections roundtrip)");
    }
    private sealed class SnapshotFixture
    {
        public float xp; public uint seed; public SphereId? nullable; public Vec2 geometry;
        public readonly Dictionary<SphereId, int> levels = new Dictionary<SphereId, int>();
        public readonly List<string> ids = new List<string>();
        public readonly List<SphereEvolutionOption> choices = new List<SphereEvolutionOption>();
    }
}
