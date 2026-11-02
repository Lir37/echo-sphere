"""Aggregate actual Unity arena outputs; never infer full-run win rates from DPS.

Usage: python scripts/analyze-unity-combat-balance.py INPUT_DIR OUTPUT_JSON
Raw CSV/arenas stay in Library. This compact report retains every tested state.
"""
import collections
import hashlib
import json
import math
import pathlib
import statistics
import subprocess
import sys


def aggregate(root):
    rows = json.loads((root / "arenas.json").read_text(encoding="utf-8-sig"))
    groups = collections.defaultdict(list)
    for row in rows:
        assert math.isfinite(row["dps"]) and row["dps"] >= 0
        groups[(row["state"], row["targets"], row["anchored"])].append(row)
    out = []
    for (state, count, anchored), trials in sorted(groups.items()):
        values = [r["dps"] for r in trials]
        assert len({r["seed"] for r in trials}) == len(trials)
        out.append(dict(state=state, targets=count, anchored=anchored,
                        trials=len(values), meanDps=statistics.mean(values),
                        minDps=min(values), maxDps=max(values),
                        meanEndDistance=statistics.mean(r["endDistance"] for r in trials)))
    return out


def main():
    root = pathlib.Path(sys.argv[1])
    summaries = {}
    for path in sorted(root.glob("*-summary.json")):
        value = json.loads(path.read_text(encoding="utf-8-sig"))
        if value.get("result") in {"COMPLETE", "PASS"}:
            summaries[path.stem] = value
    for name in ("summary.json", "tuning-isolation.json", "visual-isolation.json", "placement-probe.json", "triangle-regression.json"):
        if (root / name).exists():
            summaries[pathlib.Path(name).stem] = json.loads((root / name).read_text(encoding="utf-8-sig"))
    artifacts = {}
    names = {"arenas.json", "stats.csv", "artifact-builds.json", "abilities.json",
             "regions.json", "artifact-combat.json", "hail-probe.json", "wounded.json"}
    names.update(p.name for p in root.glob("artifact-combat-[0-9]*.json"))
    names.update(p.name for p in root.glob("ability-loadouts-[0-9]*.jsonl"))
    names.update(p.name for p in root.glob("sphere-pairs-[0-9]*.json"))
    names.update(p.name for p in root.glob("ensembles-[0-9]*.json"))
    names.update({"ensembles.json", "replays.json", "stat-extremes.json"})
    names.update({"triangle-regression.json", "placement-probe.json", "tuning-isolation.json", "visual-isolation.json"})
    for name in sorted(names):
        path = root / name
        if path.exists():
            artifacts[name] = dict(bytes=path.stat().st_size, sha256=hashlib.sha256(path.read_bytes()).hexdigest())
    result = dict(scope="Controlled production-code arenas and stat sweeps; not full-run win rates or player acceptance.",
                  summaries=summaries, artifactSubsetSpace=str(2 ** 90),
                  rawFiles=artifacts, sphereStates=aggregate(root))
    repo = pathlib.Path(__file__).resolve().parents[1]
    result["sourceBaseline"] = subprocess.check_output(
        ["git", "rev-parse", "HEAD"], cwd=repo, text=True).strip()
    result["sourceNote"] = "Checkout revision at collection time; exact source SHA256 below identifies tested code including any local campaign tuning."
    source_paths = ["scripts/simulate-unity-combat-balance.cs",
                    "scripts/analyze-unity-combat-balance.py",
                    "UnityProject/Assets/EchoSphere/Scripts/Core/CampaignCombatTuningRules.cs",
                    "UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereSourceHitCadence.cs",
                    "UnityProject/Assets/EchoSphere/Scripts/Runtime/EchoSphereSourceModifiers.cs",
                    "UnityProject/Assets/EchoSphere/Scripts/Runtime/SphereSourceHitRules.cs"]
    result["testedSourceHashFormat"] = "sha256-utf8-lf-v1"
    result["testedSourceSha256"] = {name: hashlib.sha256((repo / name).read_bytes().replace(b"\r\n", b"\n")).hexdigest()
                                    for name in source_paths}
    if (root / "verification.json").exists():
        result["verification"] = json.loads((root / "verification.json").read_text(encoding="utf-8-sig"))
    abilities = collections.defaultdict(list)
    for row in json.loads((root / "abilities.json").read_text(encoding="utf-8-sig")):
        abilities[(row["kind"], row["level"], row["branch"] or "", row["final"] or "")].append(row)
    assert len(abilities) == 168
    result["activeAbilityStates"] = []
    for (kind, level, branch, final), rows in sorted(abilities.items()):
        assert {row["region"] for row in rows} == set(range(6))
        result["activeAbilityStates"].append(dict(kind=kind, level=level, branch=branch, final=final,
            regions=6, minDamage=min(r["damage"] for r in rows), maxDamage=max(r["damage"] for r in rows),
            cooldown=rows[0]["cooldown"], maxHpCost=max(r["hpCost"] for r in rows),
            maxShields=max(r["shields"] for r in rows), maxInvulnerable=max(r["invulnerable"] for r in rows),
            maxDisplacement=max(r["displacement"] for r in rows), maxBuff=max(r["buff"] for r in rows),
            maxFrozenTargets=max(r["frozen"] for r in rows)))
    wounded = collections.defaultdict(list)
    for row in json.loads((root / "wounded.json").read_text(encoding="utf-8-sig")):
        wounded[(row["state"], row["targets"])].append(row["dps"])
    result["woundedSphereStates"] = [dict(state=s, targets=n, trials=len(values),
        meanDps=statistics.mean(values), minDps=min(values), maxDps=max(values))
        for (s, n), values in sorted(wounded.items())]
    result["regionalRoleArenas"] = json.loads((root / "regions.json").read_text(encoding="utf-8-sig"))
    result["hailRegression"] = json.loads((root / "hail-probe.json").read_text(encoding="utf-8-sig"))
    assert len(result["hailRegression"]) == 6
    for row in result["hailRegression"]:
        if row["modern"]:
            assert row["firstTickDamage"] == 0 and row["neighborDamage"] > 0
        else:
            assert row["originTargetDamage"] > 0
    assert len(result["sphereStates"]) == 880
    assert all(r["trials"] == 3 for r in result["sphereStates"])
    assert summaries["stats-summary"]["checks"] == 1126400
    assert summaries["stats-summary"]["states"] == 220
    assert summaries["abilities-summary"]["casts"] == 1008
    assert summaries["regions-summary"]["arenas"] == 156
    assert summaries["wounded-summary"]["arenas"] == 540
    assert summaries["tuning-isolation"]["checks"] == 220
    assert summaries["visual-isolation"]["checks"] == 10
    assert summaries["placement-probe"]["checks"] == 5
    assert summaries["triangle-regression"]["checks"] == 3
    assert summaries["ensembles-summary"]["arenas"] == 384
    assert summaries["replays-summary"]["arenas"] == 30
    coverage = {}
    for prefix, suffix, expected, field in (("artifact-combat", "json", 4224, "arenas"),
                                            ("ability-loadouts", "jsonl", 43164, "loadouts"),
                                            ("sphere-pairs", "json", 3735, "arenas")):
        batches = []
        for path in root.glob(prefix + "-summary-[0-9]*.json"):
            batches.append(json.loads(path.read_text(encoding="utf-8-sig")))
        batches.sort(key=lambda x: x["start"])
        if not batches:
            continue
        cursor = 0
        total = 0
        identities = set()
        for batch in batches:
            assert batch["result"] == "COMPLETE" and batch["start"] == cursor, batch
            path = root / f"{prefix}-{cursor}.{suffix}"
            if suffix == "jsonl":
                with path.open(encoding="utf-8-sig") as stream:
                    rows = [json.loads(line) for line in stream if line.strip()]
                keys = [tuple(row["loadout"]) for row in rows]
            else:
                rows = json.loads(path.read_text(encoding="utf-8-sig"))
                keys = [row["build"] for row in rows]
                assert keys == list(range(cursor, batch["end"]))
            assert len(rows) == batch[field] == batch["end"] - cursor
            assert not identities.intersection(keys), "Duplicate coverage"
            identities.update(keys)
            cursor = batch["end"]
            total += len(rows)
        assert cursor == total == expected, (prefix, cursor, expected)
        coverage[prefix] = dict(result="PASS", batches=len(batches), distinctCases=total)
        if prefix == "ability-loadouts":
            winner = max(batches, key=lambda x: x["maxBurst"])
            coverage[prefix]["largestImmediateBurst"] = dict(damage=winner["maxBurst"],
                loadout=winner["maxLoadout"], scope="Fixed eight-target activation fixture; not a sustained-DPS or survival ranking.")
    result["batchCoverage"] = coverage
    ensembles = json.loads((root / "ensembles.json").read_text(encoding="utf-8-sig"))
    assert {(row["build"], row["seed"]) for row in ensembles} == {
        (build, seed) for build in range(128) for seed in (17, 29, 71)}
    assert len(ensembles) == 384 and all(math.isfinite(row["damage"]) and row["damage"] >= 0 for row in ensembles)
    result["ensembleCoverage"] = dict(result="PASS", distinctBuilds=128, arenas=384,
        minDps=min(r["dps"] for r in ensembles), maxDps=max(r["dps"] for r in ensembles),
        scope="Sampled open-arc fixed-target contexts; extrema are not full-run rankings.")
    poison = json.loads((root / "ensembles-9.json").read_text(encoding="utf-8-sig"))
    assert len(poison) == 3 and {r["seed"] for r in poison} == {17, 29, 71}
    result["triangleReproduction"] = dict(build=9, before="Repeated native fault/hang in first tick, phase sphere Gravity; no completed pre-fix arena.",
        after=poison, scope="Previously failing production-code fixture; not attribution of historical Android 3:29 freeze.")
    if (root / "before-triangle").exists():
        changed_loadouts = 0
        for path in sorted(root.glob("ability-loadouts-[0-9]*.jsonl")):
            previous = root / "before-triangle" / path.name
            if not previous.exists():
                continue
            with previous.open(encoding="utf-8-sig") as old, path.open(encoding="utf-8-sig") as new:
                old_rows = [json.loads(line) for line in old if line.strip()]
                new_rows = [json.loads(line) for line in new if line.strip()]
            assert len(old_rows) == len(new_rows)
            changed_loadouts += sum(a != b for a, b in zip(old_rows, new_rows))
        result["postTriangleActiveReplay"] = dict(loadouts=43164, changedRows=changed_loadouts,
            scope="Same fixed-support activation fixtures replayed after Triangle guard; not the mixed-sphere stress fixtures.")
    if (root / "replays.json").exists():
        grouped = collections.defaultdict(list)
        for row in json.loads((root / "replays.json").read_text(encoding="utf-8-sig")):
            grouped[row["state"]].append(row)
        replay_summary = []
        for state, rows in sorted(grouped.items()):
            plain = {r["seed"]: r for r in rows if not r["equipped"]}
            geared = {r["seed"]: r for r in rows if r["equipped"]}
            assert plain.keys() == geared.keys() and len(plain) == 3
            replay_summary.append(dict(state=state, artifacts=rows[0]["artifacts"], seeds=sorted(plain),
                meanBaselineDps=statistics.mean(r["dps"] for r in plain.values()),
                meanEquippedDps=statistics.mean(r["dps"] for r in geared.values()),
                meanPairedDpsRatio=statistics.mean(geared[s]["dps"] / plain[s]["dps"] for s in plain),
                equippedDamageTakenMultiplier=geared[min(geared)]["damageTakenMultiplier"],
                scope="Same-seed 20s free-target comparison; no kill procs, economy or survival valuation."))
        result["pairedReplays"] = replay_summary
    if (root / "before" / "arenas.json").exists():
        result["beforeArenaSha256"] = hashlib.sha256((root / "before" / "arenas.json").read_bytes()).hexdigest()
        before = {(r["state"], r["targets"], r["anchored"]): r for r in aggregate(root / "before")}
        changes = []
        for row in result["sphereStates"]:
            old = before.get((row["state"], row["targets"], row["anchored"]))
            if old is None:  # Level-0 coverage was added after baseline capture.
                continue
            if abs(row["meanDps"] - old["meanDps"]) > 1e-6:
                assert row["state"].startswith("Pulse:") or ":shotgun_hail:" in row["state"], row
                changes.append(dict(state=row["state"], targets=row["targets"], anchored=row["anchored"],
                                    before=old["meanDps"], after=row["meanDps"]))
        result["changedGroups"] = changes
    pathlib.Path(sys.argv[2]).write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(dict(states=len(result["sphereStates"]), summaries=list(summaries), changedGroups=len(result.get("changedGroups", [])))))


if __name__ == "__main__":
    main()
