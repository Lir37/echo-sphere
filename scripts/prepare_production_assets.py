"""Prepare and report the production 3D asset set.

The source/generation directory may contain legacy or development GLBs. This
script makes the production boundary explicit: only assets listed in
scripts/production_assets.json remain under public/art3d.
"""
from __future__ import annotations

import argparse
import json
from pathlib import Path

import trimesh


ROOT = Path(__file__).resolve().parents[1]
ART3D = ROOT / "public" / "art3d"
MANIFEST = ROOT / "scripts" / "production_assets.json"


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--json-out", default="production-3d-report.json")
    args = parser.parse_args()

    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    expected = {Path(item["path"]).name for item in manifest["assets"]}
    actual = {path.name for path in ART3D.glob("*.glb")}

    missing = sorted(expected - actual)
    if missing:
        for name in missing:
            print(f"[FAIL] missing production asset: {name}")
        return 1

    removed = []
    for path in sorted(ART3D.glob("*.glb")):
        if path.name not in expected:
            path.unlink()
            removed.append(path.name)

    rows = []
    for path in sorted(ART3D.glob("*.glb")):
        scene = trimesh.load(path, force="scene", process=False)
        triangles = 0
        vertices = 0
        texture_count = 0
        decoded_texture_bytes = 0
        if isinstance(scene, trimesh.Scene):
            for geom in scene.geometry.values():
                vertices += len(getattr(geom, "vertices", []))
                triangles += len(getattr(geom, "faces", []))
                material = getattr(getattr(geom, "visual", None), "material", None)
                if material is None:
                    continue
                for attr in ("baseColorTexture", "normalTexture", "metallicRoughnessTexture", "emissiveTexture"):
                    texture = getattr(material, attr, None)
                    if texture is not None:
                        texture_count += 1
                        image = getattr(texture, "data", None)
                        if image is not None:
                            decoded_texture_bytes += getattr(image, "nbytes", 0)

        rows.append({
            "file": path.name,
            "bytes": path.stat().st_size,
            "vertices": vertices,
            "triangles": triangles,
            "texture_slots": texture_count,
            "decoded_texture_bytes": decoded_texture_bytes,
        })

    total_bytes = sum(row["bytes"] for row in rows)
    total_decoded_textures = sum(row["decoded_texture_bytes"] for row in rows)
    report = {
        "manifest_version": manifest["version"],
        "production_asset_count": len(rows),
        "removed_unlisted_glbs": removed,
        "total_glb_bytes": total_bytes,
        "total_decoded_texture_bytes": total_decoded_textures,
        "largest_glb": max(rows, key=lambda row: row["bytes"]) if rows else None,
        "assets": rows,
    }
    Path(args.json_out).write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(f"Production GLBs: {len(rows)}")
    print(f"Production GLB disk size: {total_bytes / (1024 * 1024):.1f} MiB")
    print(f"Decoded texture memory footprint: {total_decoded_textures / (1024 * 1024):.1f} MiB")
    print(f"Removed unlisted GLBs: {len(removed)}")
    for name in removed:
        print(f"  removed: {name}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
