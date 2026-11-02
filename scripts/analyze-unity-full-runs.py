"""Summarize executed production campaigns without treating bots as players."""
import argparse
import collections
import hashlib
import json
import pathlib
import statistics
import subprocess

REGIONS = ("ResonanceBasin", "SpectralRift", "FracturedBastion", "GravityFolds", "ResonanceStorm", "NullArchive")


def summarize(rows):
    return dict(trials=len(rows), outcomes=dict(collections.Counter(r["outcome"] for r in rows)),
                secondsMedian=statistics.median(r["seconds"] for r in rows),
                levelMedian=statistics.median(r["level"] for r in rows),
                levelMax=max(r["level"] for r in rows), kills=sum(r["kills"] for r in rows),
                bosses=sum(r["bosses"] for r in rows), casts=sum(r["casts"] for r in rows),
                purchases=sum(r["purchases"] for r in rows))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("inputs", nargs="+")
    parser.add_argument("--output", required=True)
    args = parser.parse_args()
    root = pathlib.Path(__file__).resolve().parents[1]
    trials, sources, replays = [], [], {}
    for directory in args.inputs:
        for file in sorted(pathlib.Path(directory).resolve().glob("r*-s*-c*-f*-*.json")):
            data = file.read_bytes()
            row = json.loads(data)
            assert row["result"] == "COMPLETE", file
            assert 0 < row["seconds"] <= row["request"]["seconds"] + .06, file
            assert row["outcome"] in ("stabilized", "dead", "time_limit"), file
            assert row["spentEcho"] <= row["collectedEcho"], file
            if row["outcome"] == "stabilized":
                assert row["bosses"] >= 1 and row["seconds"] >= 1800, file
            if row["outcome"] == "dead":
                assert row["hp"] == 0, file
            key = json.dumps(row["request"], sort_keys=True)
            projection = {k: v for k, v in row.items() if k != "elapsedMs"}
            if key in replays:
                assert replays[key] == projection, f"Nonrepeatable campaign: {file}"
            replays[key] = projection
            sources.append(dict(file=str(file.relative_to(root)).replace("\\", "/"), sha256=hashlib.sha256(data).hexdigest()))
            # Full histories and samples retained; huge end-state debug arrays are not evidence of balance.
            trials.append(dict(dataset=pathlib.Path(directory).name,
                               **{k: v for k, v in row.items() if not k.startswith("debug")}))
    assert trials, "No campaign results"
    unique = {json.dumps(r["request"], sort_keys=True) for r in trials}
    abilities = collections.Counter()
    sphere_upgrades = collections.Counter()
    evolutions = collections.Counter()
    artifact_ids = set()
    for row in trials:
        artifact_ids.update(row["artifacts"])
        for event in row["history"]:
            if "upgrade" in event:
                (sphere_upgrades if event["upgrade"] == "Sphere" else abilities)[event["sphere"] if event["upgrade"] == "Sphere" else event["upgrade"]] += 1
            if "evolution" in event:
                evolutions[event["evolution"]] += 1
    probes = {}
    for name in ("isolation", "frame-equivalence", "cosmetics-equivalence", "snapshot-guard"):
        file = root / f"UnityProject/Library/full-run-balance/{name}.json"
        probes[name] = json.loads(file.read_text(encoding="utf-8"))
        assert probes[name]["result"] == "PASS", name
    boss_file = root / "UnityProject/Library/full-run-balance/boss-scaling.json"
    if boss_file.exists():
        probes["boss-scaling"] = json.loads(boss_file.read_text(encoding="utf-8"))
    reviewed = json.loads((root / "GPT/UNITY_SPECIAL_ARTIFACT_REVIEW_2026-11-01.json").read_text(encoding="utf-8"))
    source_files = list(reviewed["fileSha256"]) + ["scripts/simulate-unity-full-runs.cs", "scripts/run-unity-full-runs.py", "scripts/analyze-unity-full-runs.py"]
    source_hashes = {file: hashlib.sha256((root / file).read_text(encoding="utf-8").replace("\r\n", "\n").encode("utf-8")).hexdigest() for file in source_files}
    report = dict(schemaVersion=1, baselineCommit=subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=root, text=True).strip(),
                  scope="Actual shared StepGameplay, real waves/bosses, XP/drops, offered upgrades and mutations, merchant budget, normal difficulty, mastery I and no shop upgrades. Bot inputs, isolated PlayerPrefs. Not human win rates, subjective fun, exhaustive builds, Android FPS or meta-completion balance.",
                  uniqueRequests=len(unique), replayRequests=len(trials)-len(unique),
                  overall=summarize(trials),
                  byRegion={name: summarize([r for r in trials if r["request"]["region"] == i]) for i, name in enumerate(REGIONS)},
                  byPolicy={policy: summarize([r for r in trials if r["request"]["policy"] == policy]) for policy in sorted({r["request"]["policy"] for r in trials})},
                  byDataset={dataset: summarize([r for r in trials if r["dataset"] == dataset]) for dataset in sorted({r["dataset"] for r in trials})},
                  coverage=dict(abilitiesOfferedAndChosen=dict(abilities), sphereUpgrades=dict(sphere_upgrades),
                                evolutionChoices=dict(evolutions), acquiredArtifacts=sorted(artifact_ids)),
                  probes=probes, sourceHashFormat="sha256-utf8-lf-v1", sourceSha256=source_hashes, sources=sources, trials=trials)
    pathlib.Path(args.output).write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({k: v for k, v in report.items() if k not in ("trials", "sources")}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
