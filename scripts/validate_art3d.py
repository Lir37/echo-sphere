"""Validate Echo Sphere GLB assets for CI.

This checks that every exported asset is a real, non-empty GLB and reports
geometry/material/texture complexity. The thresholds are intentionally role-aware:
a projectile can be light, while a boss or tier-7 sphere must not collapse into a
tiny placeholder mesh.
"""
from pathlib import Path
import json
import sys
import argparse

import trimesh


ROOT = Path(__file__).resolve().parents[1] / "public" / "art3d"


def role_threshold(path: Path) -> tuple[int, int, int]:
    name = path.stem
    if name.startswith("boss_"):
        return 15_000, 500_000, 500_000
    if name.startswith("player_"):
        return 10_000, 500_000, 500_000
    if "_t7" in name:
        return 6_000, 450_000, 650_000
    if name.startswith("enemy_"):
        return 3_000, 70_000, 250_000
    if name.startswith("sphere_"):
        return 1_500, 140_000, 300_000
    return 100, 30_000, 40_000


def validate(path: Path):
    scene = trimesh.load(path, force="scene", process=False)
    if not isinstance(scene, trimesh.Scene):
        raise RuntimeError("not a scene")

    vertices = 0
    triangles = 0
    materials = set()
    textured = 0
    normal_textured = 0
    mr_textured = 0

    for geom in scene.geometry.values():
        vertices += len(getattr(geom, "vertices", []))
        triangles += len(getattr(geom, "faces", []))
        material = getattr(getattr(geom, "visual", None), "material", None)
        if material is not None:
            materials.add(getattr(material, "name", repr(material)))
            if getattr(material, "baseColorTexture", None) is not None:
                textured += 1
            if getattr(material, "normalTexture", None) is not None:
                normal_textured += 1
            if getattr(material, "metallicRoughnessTexture", None) is not None:
                mr_textured += 1

    min_tri, max_tri, min_bytes = role_threshold(path)
    if path.stat().st_size < min_bytes:
        raise RuntimeError(f"suspiciously small GLB: {path.stat().st_size} bytes < {min_bytes}")
    if triangles < min_tri:
        raise RuntimeError(f"too few triangles: {triangles} < {min_tri}")
    if triangles > max_tri:
        raise RuntimeError(f"unexpectedly huge triangle count: {triangles} > {max_tri}")

    if materials == set():
        raise RuntimeError("no material data")
    if path.stem.startswith(("boss_", "player_", "sphere_")) and textured == 0:
        raise RuntimeError("complex asset has no base-color texture")
    if path.stem.startswith(("boss_", "player_", "sphere_")) and normal_textured == 0:
        raise RuntimeError("complex asset has no normal texture")
    if path.stem.startswith(("boss_", "player_", "sphere_")) and mr_textured == 0:
        raise RuntimeError("complex asset has no metallic/roughness texture")

    return {
        "file": path.name,
        "bytes": path.stat().st_size,
        "vertices": vertices,
        "triangles": triangles,
        "materials": len(materials),
        "textured_primitives": textured,
        "normal_textured_primitives": normal_textured,
        "mr_textured_primitives": mr_textured,
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--json-out", default=None, help="optional CI report path")
    args = parser.parse_args()

    prefixes = ("sphere_", "enemy_", "boss_", "projectile_", "player_core")
    files = sorted(
        path for path in ROOT.glob("*.glb")
        if path.stem.startswith(prefixes)
    )
    if not files:
        print("No generated GLB files found", file=sys.stderr)
        return 1

    rows = []
    for path in files:
        try:
            rows.append(validate(path))
        except Exception as exc:
            print(f"[FAIL] {path.name}: {exc}", file=sys.stderr)
            return 1

    report = Path(args.json_out) if args.json_out else ROOT / "validation-report.json"
    report.parent.mkdir(parents=True, exist_ok=True)
    report.write_text(json.dumps(rows, indent=2), encoding="utf-8")

    total_triangles = sum(row["triangles"] for row in rows)
    total_bytes = sum(row["bytes"] for row in rows)
    print(f"Validated {len(rows)} GLBs")
    print(f"Total geometry: {total_triangles:,} triangles")
    print(f"Total disk size: {total_bytes / 1024 / 1024:.1f} MiB")
    for row in rows:
        print(
            f'{row["file"]}: {row["triangles"]:,} tris, '
            f'{row["bytes"] / 1024:.0f} KiB, '
            f'{row["materials"]} materials, {row["textured_primitives"]} base, '
            f'{row["normal_textured_primitives"]} normal, {row["mr_textured_primitives"]} MR'
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
