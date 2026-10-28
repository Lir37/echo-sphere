"""Sample actual presented frames of one Android app, without video recording.

Requires Android platform-tools, an authorized device and a stable foreground
scenario. SurfaceFlinger timestamps are not Unity simulation FPS, CPU/GPU time
or GC allocation measurements. Keep other touches/overlays out of the sample.
"""

import argparse
import json
import math
from pathlib import Path
import re
import shlex
import subprocess
import time


def summarize(present_ns):
    timestamps = sorted(set(present_ns))
    if len(timestamps) < 2:
        raise RuntimeError("Insufficient newly presented frames")
    intervals = [(b - a) / 1e6 for a, b in zip(timestamps, timestamps[1:])]
    ordered = sorted(intervals)

    def percentile(q):
        return ordered[max(0, math.ceil(len(ordered) * q) - 1)]

    span = (timestamps[-1] - timestamps[0]) / 1e9
    return {
        "frames": len(timestamps),
        "presentSpanSeconds": span,
        "fps": (len(timestamps) - 1) / span,
        "p50ms": percentile(0.5),
        "p95ms": percentile(0.95),
        "p99ms": percentile(0.99),
        "maxMs": max(intervals),
        "stallsOver100ms": sum(value > 100 for value in intervals),
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--adb", default="adb")
    parser.add_argument("--serial", required=True)
    parser.add_argument("--package", default="com.lir37.echosphere")
    parser.add_argument("--scenario", required=True)
    parser.add_argument("--seconds", type=float, default=25)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    if not re.fullmatch(r"[A-Za-z0-9_.]+", args.package):
        parser.error("Invalid Android package identifier")
    if not math.isfinite(args.seconds) or args.seconds < 3:
        parser.error("Sample duration must be finite and at least 3 seconds")
    if args.output.exists():
        parser.error("Output already exists; do not overwrite prior evidence")

    def shell(command):
        return subprocess.check_output(
            [args.adb, "-s", args.serial, "shell", command], text=True, timeout=10
        )

    start = time.monotonic()
    baseline = None
    layer = None
    presented = set()
    samples = []
    while time.monotonic() - start < args.seconds:
        matches = [
            name for name in shell("dumpsys SurfaceFlinger --list").splitlines()
            if args.package in name and "SurfaceView[" in name and "(BLAST)" in name
        ]
        if len(matches) != 1:
            raise RuntimeError("Expected exactly one app BLAST surface")
        if layer is not None and layer != matches[0]:
            raise RuntimeError("Surface changed: measure the new scenario separately")
        layer = matches[0]
        raw = shell("dumpsys SurfaceFlinger --latency " + shlex.quote(layer))
        valid = []
        for line in raw.splitlines()[1:]:
            columns = line.split()
            if len(columns) != 3:
                continue
            actual = int(columns[1])
            if 0 < actual < 9223372036854775807:
                valid.append(actual)
                if baseline is not None and actual > baseline:
                    presented.add(actual)
        # Exclude frames retained in SurfaceFlinger's history before sampling.
        if baseline is None and valid:
            baseline = max(valid)
        samples.append({"elapsed": time.monotonic() - start, "layer": layer, "raw": raw})
        time.sleep(0.7)

    result = {
        "scenario": args.scenario,
        "durationSeconds": time.monotonic() - start,
        **summarize(presented),
        "method": "SurfaceFlinger app BLAST actualPresentTime, rolling buffers deduplicated",
        "scope": "Presented intervals only; caller keeps a stable foreground scenario; no video, CPU/GPU attribution or GC bytes",
    }
    args.output.parent.mkdir(parents=True, exist_ok=True)
    with args.output.open("x", encoding="utf-8") as output:
        json.dump({"summary": result, "samples": samples, "actualPresentNs": sorted(presented)}, output, indent=2)
    print(json.dumps(result))


if __name__ == "__main__":
    main()
