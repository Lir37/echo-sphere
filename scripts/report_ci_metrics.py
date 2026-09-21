from __future__ import annotations

import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]


def file_bytes(path: Path) -> int:
    if path.is_file():
        return path.stat().st_size
    if not path.exists():
        return 0
    return sum(item.stat().st_size for item in path.rglob("*") if item.is_file())


def main() -> int:
    production = {}
    validation = {}
    for name, target in (
        ("production", ROOT / "production-3d-report.json"),
        ("validation", ROOT / "glb-validation-report.json"),
    ):
        if target.exists():
            try:
                data = json.loads(target.read_text(encoding="utf-8"))
            except Exception as exc:
                raise SystemExit(f"cannot parse {target}: {exc}") from exc
            if name == "production":
                production = data
            else:
                validation = data

    apks = sorted((ROOT / "android").glob("**/*.apk")) if (ROOT / "android").exists() else []
    metrics = {
        "apk": {
            "files": [{"path": str(path.relative_to(ROOT)), "bytes": path.stat().st_size} for path in apks],
            "total_bytes": sum(path.stat().st_size for path in apks),
        },
        "web_build": {
            "bytes": file_bytes(ROOT / "dist"),
        },
        "production_art3d": {
            "bytes": int(production.get("production_art3d_bytes", 0)),
            "glb_count": int(production.get("production_asset_count", 0)),
            "embedded_texture_bytes": int(production.get("total_embedded_texture_bytes", 0)),
            "texture_count": int(production.get("texture_count", 0)),
            "largest_glb": production.get("largest_glb"),
            "largest_texture": production.get("largest_texture"),
        },
        "validation": {
            "glb_count": int(validation.get("glb_count", 0)),
            "production_glb_bytes": int(validation.get("production_glb_bytes", 0)),
            "failures": validation.get("failures", []),
        },
    }

    out = ROOT / "ci-metrics.json"
    out.write_text(json.dumps(metrics, indent=2), encoding="utf-8")
    print(json.dumps(metrics, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
