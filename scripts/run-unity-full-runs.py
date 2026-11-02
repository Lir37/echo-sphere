"""Run real isolated Unity campaigns. No copied combat/economy model.

Requires a ready Editor in Play Mode/main menu and current Unity CLI.
Each request is sequential: run_script jobs share the Editor and request file.
"""
import argparse
import json
import pathlib
import subprocess
import time


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--cli", required=True)
    parser.add_argument("--output", default="UnityProject/Library/full-run-balance/campaigns")
    parser.add_argument("--seconds", type=float, default=1800)
    parser.add_argument("--characters", action="store_true", help="60 tactical runs, all characters/regions")
    parser.add_argument("--policies", nargs="+", choices=("anchor", "collector", "kite", "tactical"),
                        default=("anchor", "collector", "kite"))
    args = parser.parse_args()
    root = pathlib.Path(__file__).resolve().parents[1]
    output = (root / args.output).resolve()
    output.mkdir(parents=True, exist_ok=True)
    work = root / "UnityProject/Library/full-run-balance"

    def cli(*command):
        result = subprocess.run([args.cli, *command, "--json"], cwd=root,
                                capture_output=True, text=True, encoding="utf-8", timeout=60)
        envelope = json.loads(result.stdout.lstrip("\ufeff"))
        if not envelope.get("success"):
            raise RuntimeError(str(envelope.get("errors")))
        return envelope["data"]

    requests = []
    for region in range(6):
        if args.characters:
            variants = [(17, "tactical", character, region % 10) for character in range(10)]
        else:
            variants = [(seed, policy, 0, (region * 3 + index) % 10)
                        for index, seed in enumerate((17, 29, 71))
                        for policy in args.policies]
        for seed, policy, character, family in variants:
            requests.append(dict(region=region, seed=seed, policy=policy, character=character,
                                 family=family, seconds=args.seconds, fastCosmetics=True))
    for index, request in enumerate(requests):
        name = f"r{request['region']}-s{request['seed']}-c{request['character']}-f{request['family']}-{request['policy']}.json"
        target = output / name
        if target.exists():
            cached = json.loads(target.read_text(encoding="utf-8"))
            if cached.get("request") != request or cached.get("result") != "COMPLETE":
                raise RuntimeError(f"Conflicting cached result: {target}")
            continue
        (work / "request.json").write_text(json.dumps(request), encoding="utf-8")
        job = cli("command", "run_script", "--file", str(root / "scripts/simulate-unity-full-runs.cs"),
                  "--entry", "FullRunSimulation.Main", "--timeout_ms", "600000", "--detach")["jobId"]
        deadline = time.monotonic() + 660
        while True:
            status = cli("job", "status", job)
            if status.get("state") in ("failed", "cancelled"):
                raise RuntimeError(str(status))
            if status.get("result") is not None:
                if not status["result"].get("success"):
                    raise RuntimeError(str(status["result"]))
                result = json.loads((work / "last-run.json").read_text(encoding="utf-8"))
                if result.get("request") != request or result.get("result") != "COMPLETE":
                    raise RuntimeError("Job output does not match request")
                target.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
                print(json.dumps(dict(index=index + 1, total=len(requests), case=name,
                                      **{key: result[key] for key in ("outcome", "seconds", "level", "kills", "bosses", "purchases", "casts")})), flush=True)
                break
            if time.monotonic() > deadline:
                raise TimeoutError(f"Unity job did not finish: {job}")
            time.sleep(2)


if __name__ == "__main__":
    main()
