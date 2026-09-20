"""Production-oriented offline GLB generator for Echo Sphere.

Runtime code never constructs the gameplay meshes. This script creates the reusable
3D assets offline and exports textured GLB files to public/art3d.

The generator intentionally favors authored-looking layered geometry and real PBR
textures over tiny placeholder meshes. Android optimization is handled later with
LOD/culling/material limits, not by making the source assets primitive.
"""
from pathlib import Path
import math
import hashlib

import numpy as np
from PIL import Image, ImageFilter, ImageDraw
import trimesh


OUT = Path(__file__).resolve().parents[1] / "public" / "art3d"
OUT.mkdir(parents=True, exist_ok=True)


# ---------------------------------------------------------------------------
# PBR material + procedural texture helpers
# ---------------------------------------------------------------------------

_TEXTURE_CACHE: dict[str, Image.Image] = {}


def _surface_field(seed: int, size: int = 512) -> np.ndarray:
    rng = np.random.default_rng(seed)
    low = Image.fromarray(np.uint8(rng.random((64, 64)) * 255), "L")
    low = low.resize((size, size), Image.Resampling.BICUBIC).filter(ImageFilter.GaussianBlur(1.4))
    noise = np.asarray(low, dtype=np.float32) / 255.0
    y, x = np.mgrid[0:size, 0:size]
    micro = (
        0.55
        + 0.22 * np.sin(x / 7.0)
        + 0.16 * np.sin(y / 11.0)
        + 0.10 * np.sin((x + y) / 19.0)
    )
    return np.clip(0.55 * noise + 0.45 * micro, 0.0, 1.0)


def _normal_texture(seed: int, size: int = 512) -> Image.Image:
    height = _surface_field(seed, size)
    dx = np.gradient(height, axis=1) * 3.2
    dy = np.gradient(height, axis=0) * 3.2
    nx = -dx
    ny = np.ones_like(height)
    nz = -dy
    length = np.sqrt(nx * nx + ny * ny + nz * nz)
    rgb = np.stack(
        (
            nx / length * 0.5 + 0.5,
            ny / length * 0.5 + 0.5,
            nz / length * 0.5 + 0.5,
        ),
        axis=-1,
    )
    return Image.fromarray(np.uint8(np.clip(rgb, 0, 1) * 255), "RGB")


def _metal_rough_texture(metallic: float, roughness: float, seed: int, size: int = 512) -> Image.Image:
    field = _surface_field(seed + 17, size)
    rough = np.clip(roughness * (0.82 + 0.25 * field), 0.04, 1.0)
    metal = np.clip(metallic * (0.95 + 0.08 * field), 0.0, 1.0)
    rgb = np.stack((np.zeros_like(rough), rough, metal), axis=-1)
    return Image.fromarray(np.uint8(np.clip(rgb, 0, 1) * 255), "RGB")


def _texture(base, glow, seed: int, size: int = 512) -> Image.Image:
    key = f"{base}-{glow}-{seed}-{size}"
    if key in _TEXTURE_CACHE:
        return _TEXTURE_CACHE[key]

    rng = np.random.default_rng(seed)
    low = Image.fromarray(np.uint8(rng.random((64, 64)) * 255), "L")
    low = low.resize((size, size), Image.Resampling.BICUBIC).filter(ImageFilter.GaussianBlur(1.4))
    noise = np.asarray(low, dtype=np.float32) / 255.0

    y, x = np.mgrid[0:size, 0:size]
    micro = (
        0.55
        + 0.22 * np.sin(x / 7.0)
        + 0.16 * np.sin(y / 11.0)
        + 0.10 * np.sin((x + y) / 19.0)
    )
    micro = np.clip(micro, 0.15, 1.2)
    b = np.asarray(base, dtype=np.float32)[None, None, :]
    g = np.asarray(glow, dtype=np.float32)[None, None, :]
    rgb = b * (0.62 + noise[..., None] * 0.28) * micro[..., None] + g * (noise[..., None] * 0.16)

    # Thin sci-fi "circuit" accents give the otherwise procedural surface actual
    # high-frequency detail when the camera gets close.
    circuit = (
        ((x % 53) < 2)
        | ((y % 71) < 2)
        | (((x + 2 * y) % 97) < 1)
    )
    rgb[circuit] = np.clip(g * 0.72 + b * 0.28, 0, 1)

    image = Image.fromarray(np.uint8(np.clip(rgb, 0, 1) * 255), "RGB")
    _TEXTURE_CACHE[key] = image
    return image


def _uv(mesh: trimesh.Trimesh, mode: str = "sphere"):
    v = np.asarray(mesh.vertices, dtype=np.float32)
    if mode == "plane":
        lo, hi = v.min(axis=0), v.max(axis=0)
        span = np.maximum(hi - lo, 1e-6)
        uv = np.column_stack(((v[:, 0] - lo[0]) / span[0], (v[:, 1] - lo[1]) / span[1]))
    else:
        r = np.linalg.norm(v, axis=1)
        safe = np.maximum(r, 1e-6)
        u = np.arctan2(v[:, 2], v[:, 0]) / math.tau + 0.5
        vv = np.arcsin(np.clip(v[:, 1] / safe, -1, 1)) / math.pi + 0.5
        uv = np.column_stack((u, vv))
    return uv.astype(np.float32)


def pbr(
    name: str,
    base,
    glow,
    metallic: float = 0.78,
    rough: float = 0.2,
    alpha: float = 1.0,
    seed: int = 1,
):
    image = _texture(base, glow, seed)
    normal = _normal_texture(seed + 101, 256)
    metallic_roughness = _metal_rough_texture(metallic, rough, seed + 211, 256)
    return trimesh.visual.material.PBRMaterial(
        name=name,
        baseColorFactor=(1.0, 1.0, 1.0, alpha),
        baseColorTexture=image,
        normalTexture=normal,
        metallicRoughnessTexture=metallic_roughness,
        emissiveFactor=tuple(float(x) for x in glow),
        metallicFactor=metallic,
        roughnessFactor=rough,
        alphaMode="BLEND" if alpha < 0.98 else "OPAQUE",
        doubleSided=alpha < 0.98,
    )


def assign(mesh: trimesh.Trimesh, material, uv_mode: str = "sphere"):
    mesh.visual = trimesh.visual.TextureVisuals(
        uv=_uv(mesh, uv_mode),
        material=material,
    )
    return mesh


# ---------------------------------------------------------------------------
# Geometry primitives
# ---------------------------------------------------------------------------

def ico(name, radius, material, scale=(1, 1, 1), subdivisions=3):
    mesh = trimesh.creation.icosphere(subdivisions=subdivisions, radius=radius)
    mesh.apply_scale(scale)
    mesh.metadata["name"] = name
    return assign(mesh, material)


def cone_between(name, a, b, r1, r2, material, sections=20):
    a = np.asarray(a, dtype=np.float64)
    b = np.asarray(b, dtype=np.float64)
    axis = b - a
    length = float(np.linalg.norm(axis))
    if length < 1e-7:
        return ico(name, max(r1, r2), material, subdivisions=2)

    # Build a true tapered frustum explicitly. This avoids version-specific
    # trimesh creation helpers and gives us deterministic topology/UVs.
    z = np.linspace(0.0, length, 2)
    angles = np.linspace(0.0, math.tau, sections, endpoint=False)
    circle0 = np.column_stack((r1 * np.cos(angles), r1 * np.sin(angles), np.full(sections, z[0])))
    circle1 = np.column_stack((r2 * np.cos(angles), r2 * np.sin(angles), np.full(sections, z[1])))
    vertices = np.vstack((circle0, circle1))
    faces = []
    for i in range(sections):
        j = (i + 1) % sections
        faces.append((i, j, sections + j))
        faces.append((i, sections + j, sections + i))
    faces.extend(
        [(0, i + 1, i) for i in range(1, sections - 1)]
        + [(sections, sections + i, sections + i + 1) for i in range(1, sections - 1)]
    )
    mesh = trimesh.Trimesh(vertices=vertices, faces=np.asarray(faces, dtype=np.int64), process=True)
    mesh.metadata["name"] = name

    # Local XY cylindrical UVs.
    rr = np.maximum(np.linalg.norm(vertices[:, :2], axis=1), 1e-6)
    uv = np.column_stack(
        (
            np.arctan2(vertices[:, 1], vertices[:, 0]) / math.tau + 0.5,
            vertices[:, 2] / max(length, 1e-6),
        )
    ).astype(np.float32)
    mesh.visual = trimesh.visual.TextureVisuals(uv=uv, material=material)

    # Align the local +Z axis with the segment direction, then place it midway.
    mesh.apply_translation((a + b) / 2.0 - np.array([0.0, 0.0, length / 2.0]))
    mesh.apply_transform(trimesh.geometry.align_vectors([0, 0, 1], axis))
    return mesh


def torus(name, major, minor, material, rotation=(0, 0, 0), sections=64):
    mesh = trimesh.creation.torus(
        major_radius=major,
        minor_radius=minor,
        major_sections=sections,
        minor_sections=14,
        transform=trimesh.transformations.euler_matrix(*rotation),
    )
    mesh.metadata["name"] = name
    return assign(mesh, material)


def geodesic_cage(name, radius, material, subdivisions=2, thickness=0.018):
    """Turn an icosphere into a luminous geodesic edge cage."""
    ico_mesh = trimesh.creation.icosphere(subdivisions=subdivisions, radius=radius)
    edges = np.unique(np.sort(ico_mesh.edges_unique, axis=1), axis=0)
    parts = []
    for i, (a_idx, b_idx) in enumerate(edges):
        a = ico_mesh.vertices[a_idx]
        b = ico_mesh.vertices[b_idx]
        parts.append(cone_between(
            f"{name}_Edge_{i}", a, b, thickness, thickness * 0.72, material, sections=8
        ))
    return parts


def plate(name, center, scale, material, rotation=(0, 0, 0), subdivisions=2):
    mesh = trimesh.creation.icosphere(subdivisions=subdivisions, radius=1.0)
    mesh.apply_scale(scale)
    mesh.apply_transform(trimesh.transformations.euler_matrix(*rotation))
    mesh.apply_translation(center)
    mesh.metadata["name"] = name
    return assign(mesh, material)


def wing(name, center, length, width, material, rotation=(0, 0, 0)):
    # A shallow, faceted wing rather than a single rectangle.
    vertices = np.array([
        [-0.05, 0.0, 0.0],
        [length * 0.38, width, 0.0],
        [length, width * 0.28, 0.0],
        [length * 0.82, -width * 0.42, 0.0],
        [length * 0.30, -width * 0.72, 0.0],
        [length * 0.14, 0.0, 0.045],
    ], dtype=np.float32)
    faces = np.array([[0, 1, 5], [1, 2, 5], [2, 3, 5], [3, 4, 5], [4, 0, 5]], dtype=np.int64)
    mesh = trimesh.Trimesh(vertices=vertices, faces=faces, process=True)
    mesh.apply_transform(trimesh.transformations.euler_matrix(*rotation))
    mesh.apply_translation(center)
    mesh.metadata["name"] = name
    return assign(mesh, material, "plane")


def save(name, parts):
    scene = trimesh.Scene()
    for index, part in enumerate(parts):
        node = part.metadata.get("name", f"Part_{index:02d}")
        scene.add_geometry(part, node_name=node)
    scene.export(OUT / f"{name}.glb", file_type="glb")


# ---------------------------------------------------------------------------
# Sphere/tower family
# ---------------------------------------------------------------------------

def sphere_asset(name, base, glow, tier, family_seed):
    shell = pbr("Shell", (0.008, 0.025, 0.065), glow, 0.72, max(0.06, 0.16 - tier * 0.014), 0.24, seed=family_seed)
    core = pbr("Core", base, glow, 0.35, 0.08, seed=family_seed + 1)
    frame_glow = tuple(min(1.0, x * 0.34) for x in glow)
    metal = pbr("Frame", tuple(min(1.0, x * 0.62) for x in glow), frame_glow, 0.88, 0.18, seed=family_seed + 2)

    parts = [
        ico("Core", 0.52 + tier * 0.012, core, (1.0, 1.0, 1.12), 4),
        ico("Housing", 0.96 + tier * 0.035, shell, (1.0, 0.88, 0.94), 3),
    ]
    # The reference is a luminous geodesic device. The cage is deliberately
    # made from real 3D struts so it remains readable at gameplay distance.
    cage_subdivisions = 1
    cage_radius = 1.08 + tier * 0.07
    parts.extend(geodesic_cage("Cage", cage_radius, metal, cage_subdivisions,
                               thickness=0.052 + tier * 0.004))

    # The reference sphere is a luminous geodesic device, not a solid ball.
    # Build orthogonal and diagonal orbital frames so every tier has a recognisable
    # silhouette and tier VII reads as the fully evolved version.
    ring_count = 1 if tier == 1 else 2 if tier <= 3 else 3 if tier <= 5 else 4
    ring_radius = 1.00 + tier * 0.018
    for i in range(ring_count):
        r = ring_radius + (i - (ring_count - 1) * 0.5) * 0.11
        rot = (
            (0, 0, 0),
            (math.pi / 2, 0, 0),
            (0, math.pi / 2, 0),
            (math.pi / 4, math.pi / 4, 0),
        )[i % 4]
        parts.append(torus(f"Ring_{i}", r, 0.028 + tier * 0.004, metal, rot))

    if tier >= 2:
        diagonal_count = 2 if tier <= 4 else 4
        for i in range(diagonal_count):
            a = math.tau * i / diagonal_count
            rot = (math.pi / 4, a, math.pi / 5)
            parts.append(torus(f"Orbit_{i}", 1.08 + tier * 0.012, 0.020 + tier * 0.003, metal, rot))

    if tier >= 4:
        # Radial energy struts visually connect the core to the cage.
        for i in range(6):
            a = math.tau * i / 6
            parts.append(cone_between(
                f"RadialStrut_{i}",
                (0.30 * math.cos(a), 0.30 * math.sin(a), 0),
                (1.05 * math.cos(a), 1.05 * math.sin(a), 0),
                0.028 + tier * 0.003,
                0.009,
                metal,
                12,
            ))

    node_count = 2 + tier
    for i in range(node_count):
        a = math.tau * i / node_count
        radius = 0.82 + 0.025 * (tier - 1)
        parts.append(
            ico(
                f"EnergyNode_{i}",
                0.075 + tier * 0.006,
                core,
                (1.0, 1.0, 1.35),
                2,
            )
        )
        parts[-1].apply_translation((radius * math.cos(a), 0.08 * math.sin(a * 2), radius * math.sin(a)))

    # Outer "cage" becomes progressively more complex, matching the reference's
    # visual evolution instead of merely scaling the same object.
    if tier >= 3:
        for i in range(4):
            a = i * math.pi / 2
            parts.append(
                cone_between(
                    f"Strut_{i}",
                    (0, 0, 0),
                    (1.02 * math.cos(a), 0.28, 1.02 * math.sin(a)),
                    0.022 + tier * 0.003,
                    0.006,
                    metal,
                    12,
                )
            )

    if tier >= 5:
        for i in range(6):
            a = i * math.tau / 6
            parts.append(
                plate(
                    f"Blade_{i}",
                    (0.95 * math.cos(a), 0.0, 0.95 * math.sin(a)),
                    (0.24 + tier * 0.018, 0.055, 0.13 + tier * 0.01),
                    metal,
                    (0, -a, math.sin(a) * 0.22),
                    2,
                )
            )

    if tier >= 6:
        for i in range(8):
            a = i * math.tau / 8
            parts.append(
                cone_between(
                    f"Spike_{i}",
                    (0.62 * math.cos(a), 0.0, 0.62 * math.sin(a)),
                    (1.38 * math.cos(a), 0.0, 1.38 * math.sin(a)),
                    0.07,
                    0.008,
                    metal,
                    16,
                )
            )

    save(name, parts)


# ---------------------------------------------------------------------------
# Enemy family
# ---------------------------------------------------------------------------

def insect_asset(name, kind, base, glow, scale, seed):
    chitin = pbr("Chitin", base, glow, 0.86, 0.18, seed=seed)
    dark = pbr("ArmorDark", tuple(x * 0.22 for x in base), glow, 0.72, 0.24, seed=seed + 1)
    core = pbr("Core", tuple(min(1.0, x * 0.55 + 0.05) for x in glow), glow, 0.35, 0.08, seed=seed + 2)
    wingmat = pbr("Wing", tuple(min(1.0, x * 0.18 + 0.02) for x in glow), glow, 0.15, 0.12, 0.48, seed=seed + 3)

    parts = []
    body_scale = (1.25, 0.78, 0.95) if kind != "flyer" else (1.0, 0.7, 0.8)
    parts.append(ico("Body", 0.78 * scale, chitin, body_scale, 4))
    parts.append(ico("Core", 0.30 * scale, core, (1, 1, 1.2), 3))
    parts.append(ico("Head", 0.44 * scale, dark, (1.0, 0.82, 0.78), 3))

    eye_count = 4 if kind in {"worker", "flyer"} else 6
    for i in range(eye_count):
        a = (i - (eye_count - 1) / 2) * 0.24
        eye = ico(f"Eye_{i}", 0.07 * scale, core, 1)
        eye.apply_translation((0.35 * scale, 0.18 * scale, a * scale))

    leg_count = 6 if kind in {"worker", "flyer"} else 8 if kind != "queen" else 10
    for i in range(leg_count):
        a = math.tau * i / leg_count
        spread = 0.62 if leg_count <= 6 else 0.74
        hip = (math.cos(a) * spread * scale, -0.04 * scale, math.sin(a) * spread * scale)
        knee = (math.cos(a) * 1.08 * scale, -0.10 * scale + 0.08 * math.sin(a * 2), math.sin(a) * 1.08 * scale)
        foot = (math.cos(a) * (1.70 if kind != "queen" else 2.05) * scale, -0.32 * scale, math.sin(a) * (1.70 if kind != "queen" else 2.05) * scale)
        parts.append(cone_between(f"Leg_{i}_Upper", hip, knee, 0.12 * scale, 0.075 * scale, chitin, 18))
        parts.append(cone_between(f"Leg_{i}_Lower", knee, foot, 0.075 * scale, 0.028 * scale, dark, 16))
        parts.append(ico(f"Leg_{i}_Joint", 0.10 * scale, core, 2))

    # Mandibles / front armor make the silhouette read as a creature, not a ball
    # with cylinders attached.
    for side in (-1, 1):
        s = float(side)
        parts.append(cone_between(
            f"Mandible_{side}",
            (0.36 * scale, 0.0, 0.24 * s * scale),
            (0.78 * scale, -0.08 * scale, 0.50 * s * scale),
            0.09 * scale, 0.018 * scale, dark, 18
        ))

    if kind in {"guard", "queen", "spider"}:
        for i in range(5):
            a = math.pi * (i - 2) / 4
            parts.append(
                plate(
                    f"ArmorPlate_{i}",
                    (0.20 * scale, 0.20 * scale, math.sin(a) * 0.55 * scale),
                    (0.52 * scale, 0.09 * scale, 0.34 * scale),
                    dark,
                    (0.0, a * 0.18, 0.0),
                    2,
                )
            )

    if kind == "flyer":
        for side in (-1, 1):
            for row in range(2):
                parts.append(
                    wing(
                        f"Wing_{side}_{row}",
                        (0.05 * scale, 0.16 * scale + row * 0.03 * scale, side * 0.08 * scale),
                        (1.65 + row * 0.15) * scale,
                        (0.45 - row * 0.05) * scale,
                        wingmat,
                        (0.0, side * 0.18, side * (0.18 + row * 0.12)),
                    )
                )

    if kind == "queen":
        for i in range(6):
            a = math.tau * i / 6
            parts.append(plate(
                f"Crown_{i}",
                (0.25 * math.cos(a) * scale, 0.45 * scale, 0.25 * math.sin(a) * scale),
                (0.22 * scale, 0.48 * scale, 0.08 * scale),
                metal if False else chitin,
                (0, a, 0),
                2,
            ))

    save(name, parts)


def slime_asset(name, seed=71):
    body = pbr("Slime", (0.03, 0.24, 0.12), (0.10, 1.0, 0.42), 0.25, 0.16, 0.88, seed)
    core = pbr("SlimeCore", (0.08, 0.45, 0.18), (0.25, 1.0, 0.42), 0.15, 0.08, seed=seed + 1)
    parts = [ico("Body", 0.78, body, (1.25, 0.58, 0.92), 4), ico("Core", 0.32, core, 3)]
    for i in range(8):
        a = math.tau * i / 8
        parts.append(cone_between(
            f"Tendril_{i}",
            (0.35 * math.cos(a), -0.20, 0.35 * math.sin(a)),
            (1.15 * math.cos(a), -0.48, 1.15 * math.sin(a)),
            0.10, 0.018, body, 14
        ))
    save(name, parts)


def psionic_asset(name, seed=83):
    dark = pbr("PsionicShell", (0.035, 0.025, 0.13), (0.55, 0.20, 1.0), 0.72, 0.15, seed=seed)
    core = pbr("PsionicCore", (0.28, 0.06, 0.5), (0.8, 0.18, 1.0), 0.2, 0.06, seed=seed + 1)
    parts = [ico("Body", 0.64, dark, (0.9, 1.2, 0.9), 4), ico("Core", 0.32, core, 4)]
    for i in range(10):
        a = math.tau * i / 10
        parts.append(cone_between(
            f"VoidTendril_{i}",
            (0.15 * math.cos(a), -0.25, 0.15 * math.sin(a)),
            (0.85 * math.cos(a), -0.9 - 0.12 * math.sin(a * 3), 0.85 * math.sin(a)),
            0.045, 0.008, core, 12
        ))
    save(name, parts)


# ---------------------------------------------------------------------------
# Player / bosses / projectiles
# ---------------------------------------------------------------------------

def player_asset():
    shell = pbr("PlayerShell", (0.025, 0.07, 0.16), (0.12, 0.65, 1.0), 0.94, 0.12, seed=101)
    core = pbr("PlayerCore", (0.72, 0.92, 1.0), (0.22, 0.95, 1.0), 0.35, 0.05, seed=102)
    frame = pbr("PlayerFrame", (0.04, 0.12, 0.24), (0.10, 0.55, 1.0), 0.88, 0.16, seed=103)
    parts = [
        ico("Housing", 0.98, shell, (1.0, 0.92, 1.08), 5),
        ico("Core", 0.47, core, (1.0, 1.0, 1.16), 4),
    ]
    for i, (r, minor) in enumerate(((0.70, 0.045), (0.86, 0.032), (1.04, 0.020))):
        parts.append(torus(f"Ring_{i}", r, minor, frame, (math.pi / 2 if i == 1 else 0, 0, 0)))
    for i in range(6):
        a = math.tau * i / 6
        parts.append(cone_between(
            f"EnergySpine_{i}",
            (0.50 * math.cos(a), -0.04, 0.50 * math.sin(a)),
            (1.08 * math.cos(a), 0.12, 1.08 * math.sin(a)),
            0.045, 0.015, frame, 14
        ))
    for i in range(4):
        a = math.tau * i / 4 + math.pi / 4
        parts.append(plate(
            f"ShellPlate_{i}",
            (0.0, 0.0, 0.82),
            (0.32, 0.06, 0.26),
            frame,
            (0, a, 0),
            2,
        ))
    save("player_core", parts)


def boss_asset(name, kind, seed):
    if kind == "colony":
        insect_asset(name, "queen", (0.18, 0.025, 0.018), (1.0, 0.16, 0.02), 1.35, seed)
        return

    body = pbr("BossBody", (0.05, 0.02, 0.12) if kind == "distortion" else (0.02, 0.09, 0.18),
               (0.78, 0.10, 1.0) if kind == "distortion" else (0.08, 0.65, 1.0),
               0.9, 0.13, seed=seed)
    core = pbr("BossCore", (0.18, 0.03, 0.34) if kind == "distortion" else (0.03, 0.28, 0.72),
               (0.85, 0.15, 1.0) if kind == "distortion" else (0.10, 0.75, 1.0),
               0.25, 0.05, seed=seed + 1)
    parts = [
        ico("Body", 1.15, body, (1.1, 0.9, 1.1), 5),
        ico("VoidCore" if kind == "distortion" else "SingularityCore", 0.50, core, (1, 1, 1.2), 4),
    ]
    count = 14 if kind == "distortion" else 12
    for i in range(count):
        a = math.tau * i / count
        parts.append(cone_between(
            f"OrbitArm_{i}",
            (math.cos(a) * 0.52, 0, math.sin(a) * 0.52),
            (math.cos(a) * 1.95, 0.38 * math.sin(a * 2), math.sin(a) * 1.95),
            0.13, 0.018, body, 20
        ))
    for i, r in enumerate((1.22, 1.48, 1.74)):
        parts.append(torus(f"Ring_{i}", r, 0.035 + i * 0.008, core, (0, 0, 0) if i != 1 else (math.pi / 2, 0, 0)))
    save(name, parts)


def projectile(name, base, glow, seed):
    mat = pbr("Projectile", base, glow, 0.35, 0.06, seed=seed)
    frame = pbr("ProjectileFrame", glow, glow, 0.5, 0.08, seed=seed + 1)
    save(name, [
        ico("Core", 0.28, mat, (0.8, 0.8, 1.15), 4),
        torus("Ring_0", 0.38, 0.035, frame, (math.pi / 2, 0, 0)),
        cone_between("Spike", (0, 0, -0.12), (0, 0, 0.52), 0.08, 0.008, frame, 14),
    ])


# ---------------------------------------------------------------------------
# Build all authored assets. Existing manually-authored GLBs are preserved.
# ---------------------------------------------------------------------------

SPHERE_FAMILIES = [
    ("standard", (.05, .55, 1.0), (.25, .95, 1.0), 11),
    ("sniper", (.45, .05, .95), (.80, .25, 1.0), 23),
    ("shotgun", (.95, .25, .04), (1.0, .55, .10), 37),
    ("chain", (.95, .72, .03), (1.0, .90, .25), 49),
    ("aura", (.03, .75, .50), (.15, 1.0, .70), 61),
]

# Only remove assets owned by this generator. Do not destroy manually supplied
# high-quality source/reference GLBs.
owned_prefixes = (
    "sphere_", "enemy_", "boss_", "projectile_", "player_core",
)
for old in OUT.glob("*.glb"):
    if old.stem.startswith(owned_prefixes):
        old.unlink()

for family, base, glow, seed in SPHERE_FAMILIES:
    for tier in range(1, 8):
        sphere_asset(f"sphere_{family}_t{tier}", base, glow, tier, seed + tier * 7)
    sphere_asset(f"sphere_{family}", base, glow, 7, seed + 70)

insect_asset("enemy_worker", "worker", (.15, .02, .03), (1.0, .25, .04), .58, 201)
insect_asset("enemy_guard", "guard", (.16, .02, .03), (1.0, .08, .02), .82, 211)
insect_asset("enemy_flyer", "flyer", (.12, .02, .18), (.80, .15, 1.0), .60, 221)
insect_asset("enemy_spider", "spider", (.08, .015, .12), (1.0, .10, .55), .98, 231)
slime_asset("enemy_slime")
psionic_asset("enemy_psionic")
insect_asset("enemy_queen", "queen", (.18, .07, .02), (1.0, .45, .06), 1.18, 241)

player_asset()
boss_asset("boss_colony", "colony", 301)
boss_asset("boss_distortion", "distortion", 311)
boss_asset("boss_singularity", "singularity", 321)
projectile("projectile_energy", (.05, .55, 1.0), (.10, .82, 1.0), 401)
projectile("projectile_fire", (.90, .12, .02), (1.0, .42, .05), 411)

count = len(list(OUT.glob("*.glb")))
print(f"Generated {count} production GLB assets in {OUT}")
