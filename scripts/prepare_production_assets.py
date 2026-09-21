"""Prepare and report the production 3D asset set.

The source/generation directory may contain legacy or development GLBs. This
script makes the production boundary explicit: only assets listed in
scripts/production_assets.json remain under public/art3d.
"""
from __future__ import annotations

import argparse
import json
import struct
from io import BytesIO
from pathlib import Path

from PIL import Image
import trimesh


ROOT = Path(__file__).resolve().parents[1]
ART3D = ROOT / "public" / "art3d"
MANIFEST = ROOT / "scripts" / "production_assets.json"


def inspect_embedded_images(path: Path) -> list[dict]:
    data = path.read_bytes()
    if data[:4] != b"glTF":
        return []
    json_length, json_type = struct.unpack_from("<II", data, 12)
    if json_type != 0x4E4F534A:
        return []
    document = json.loads(data[20:20 + json_length].decode("utf-8").rstrip(" \x00"))
    bin_offset = 20 + json_length
    images = []
    for index, image in enumerate(document.get("images", [])):
        view_index = image.get("bufferView")
        if view_index is None:
            continue
        view = document["bufferViews"][view_index]
        start = bin_offset + view.get("byteOffset", 0)
        end = start + view["byteLength"]
        payload = data[start:end]
        width = height = None
        try:
            with Image.open(BytesIO(payload)) as im:
                width, height = im.size
        except Exception:
            pass
        images.append({
            "index": index,
            "bytes": len(payload),
            "width": width,
            "height": height,
            "mime": image.get("mimeType"),
        })
    return images


def runtime_asset_entry(item: dict) -> dict:
    entry = dict(item)
    asset_id = str(entry.get("id", ""))
    entry["facingOffset"] = 1.5707963267948966 if asset_id.startswith(("enemy_", "boss_")) else 0.0

    if asset_id.startswith("enemy_"):
        name = asset_id.removeprefix("enemy_")
        entry["gameType"] = {
            "worker": "spider",
            "guard": "tank",
            "flyer": "flyer",
            "spider": "spider",
            "slime": "slime",
            "psionic": "psionic",
            "queen": "queen",
        }.get(name, name)
    elif asset_id.startswith("boss_"):
        entry["gameType"] = asset_id.removeprefix("boss_")
    elif asset_id.startswith("sphere_"):
        parts = asset_id.split("_")
        if len(parts) >= 3:
            entry["type"] = parts[1]
            try:
                entry["tier"] = int(parts[-1].removeprefix("t"))
            except ValueError:
                pass
    return entry


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
        embedded_images = inspect_embedded_images(path)
        embedded_texture_bytes = sum(item["bytes"] for item in embedded_images)
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
            "embedded_texture_bytes": embedded_texture_bytes,
            "embedded_images": embedded_images,
        })

    total_bytes = sum(row["bytes"] for row in rows)
    total_decoded_textures = sum(row["decoded_texture_bytes"] for row in rows)
    total_embedded_textures = sum(row["embedded_texture_bytes"] for row in rows)
    all_images = [
        {**image, "file": row["file"]}
        for row in rows for image in row["embedded_images"]
    ]
    runtime_manifest = {
        "version": manifest["version"],
        "assets": [runtime_asset_entry(item) for item in manifest["assets"]],
    }
    (ART3D / "manifest.json").write_text(
        json.dumps(runtime_manifest, indent=2),
        encoding="utf-8",
    )

    total_art3d_bytes = sum(path.stat().st_size for path in ART3D.rglob("*") if path.is_file() and path.name != "manifest.json")
    report = {
        "manifest_version": manifest["version"],
        "production_art3d_bytes": total_art3d_bytes,
        "production_asset_count": len(rows),
        "removed_unlisted_glbs": removed,
        "total_glb_bytes": total_bytes,
        "total_decoded_texture_bytes": total_decoded_textures,
        "total_embedded_texture_bytes": total_embedded_textures,
        "texture_count": len(all_images),
        "largest_texture": max(all_images, key=lambda item: item["bytes"]) if all_images else None,
        "largest_glb": max(rows, key=lambda row: row["bytes"]) if rows else None,
        "assets": rows,
        "runtime_manifest": "public/art3d/manifest.json",
    }
    Path(args.json_out).write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(f"Production GLBs: {len(rows)}")
    print(f"Production GLB disk size: {total_bytes / (1024 * 1024):.1f} MiB")
    print(f"Production art3d disk size: {total_art3d_bytes / (1024 * 1024):.1f} MiB")
    print(f"Embedded texture payload: {total_embedded_textures / (1024 * 1024):.1f} MiB")
    print(f"Embedded texture count: {len(all_images)}")
    print(f"Decoded texture memory footprint: {total_decoded_textures / (1024 * 1024):.1f} MiB")
    print(f"Removed unlisted GLBs: {len(removed)}")
    for name in removed:
        print(f"  removed: {name}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
