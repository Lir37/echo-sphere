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
import shutil

import numpy as np
from PIL import Image, ImageFilter, ImageDraw
import trimesh


OUT = Path(__file__).resolve().parents[1] / "public" / "art3d"
OUT.mkdir(parents=True, exist_ok=True)

# Source assets supplied outside this generator are benchmarks and must never be
# deleted or overwritten by a regeneration pass. The supplied spider GLB is the
# current geometry/PBR benchmark for the enemy family.
PROTECTED_SOURCE_ASSETS = {"enemy_spider.glb"}


# ---------------------------------------------------------------------------
# PBR material + procedural texture helpers
# ---------------------------------------------------------------------------

_TEXTURE_CACHE: dict[str, Image.Image] = {}

# Texture quality profile. Standard assets use 1024px color maps and 512px
# detail maps. Hero, boss, and T7 assets use a 2048px color map plus 1024px
# normal/metallic-roughness maps. This keeps Android memory reasonable without
# making the important close/readable assets look soft.
_TEXTURE_PROFILE = "standard"


def _surface_field(seed: int, size: int = 512) -> np.ndarray:
    rng = np.random.default_rng(seed)
    low = Image.fromarray(np.uint8(rng.random((64, 64)) * 255))
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
    return Image.fromarray(np.uint8(np.clip(rgb, 0, 1) * 255))


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
    if _TEXTURE_PROFILE == "hero":
        color_size, detail_size = 2048, 1024
    elif _TEXTURE_PROFILE == "compact":
        color_size, detail_size = 512, 256
    else:
        color_size, detail_size = 1024, 512
    image = _texture(base, glow, seed, color_size)
    normal = _normal_texture(seed + 101, detail_size)
    metallic_roughness = _metal_rough_texture(metallic, rough, seed + 211, detail_size)
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


def torus(name, major, minor, material, rotation=(0, 0, 0), sections=64, minor_sections=18):
    mesh = trimesh.creation.torus(
        major_radius=major,
        minor_radius=minor,
        major_sections=sections,
        minor_sections=minor_sections,
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
    """Export a compact production GLB while preserving only parts that animate independently.

    Most authored pieces are rigid and can share a draw call when they use the same
    material. Legs/wings/rings/core are kept separate because the runtime animates them.
    """
    scene = trimesh.Scene()
    animated_tokens = ("Leg_", "Wing_", "Ring_", "Orbit_", "CageCurve_", "Latitude_", "Petal_", "VerticalRing", "Core", "VoidCore", "SingularityCore", "Mandible_")
    static_groups: dict[int, list[trimesh.Trimesh]] = {}
    static_names: dict[int, list[str]] = {}

    for index, part in enumerate(parts):
        node = part.metadata.get("name", f"Part_{index:02d}")
        is_animated = any(token in node for token in animated_tokens)
        if is_animated:
            scene.add_geometry(part, node_name=node)
            continue

        material = getattr(getattr(part, "visual", None), "material", None)
        key = id(material)
        static_groups.setdefault(key, []).append(part)
        static_names.setdefault(key, []).append(node)

    for group_index, group in enumerate(static_groups.values()):
        merged = trimesh.util.concatenate(group) if len(group) > 1 else group[0]
        merged.metadata["name"] = f"Static_{group_index:02d}"
        scene.add_geometry(merged, node_name=f"Static_{group_index:02d}")

    destination = OUT / f"{name}.glb"
    if destination.name in PROTECTED_SOURCE_ASSETS and destination.exists():
        print(f"Preserving protected source asset: {destination.name}")
        return
    scene.export(destination, file_type="glb")


# ---------------------------------------------------------------------------
# Sphere/tower family
# ---------------------------------------------------------------------------

def sphere_asset(name, base, glow, tier, family_seed):
    global _TEXTURE_PROFILE
    # Sphere/tower assets are visible primarily at gameplay distance. Keep their
    # source textures at the standard 1024/512 profile; close-up hero assets get
    # the 2048/1024 profile explicitly in their own generators.
    _TEXTURE_PROFILE = "hero" if tier >= 7 else "standard"
    """Build a clean energy-orbit tower family matching the supplied reference.

    The reference is not a mechanical ball covered in spokes. It is a luminous core
    held by a small number of coherent curved orbital frames. Evolution increases
    the number/complexity of those frames while preserving one readable silhouette.
    """
    # Keep the orb's body dark and volumetric. The reference reads as a dark
    # energy shell with a concentrated luminous nucleus, not as a white ball.
    core_base = tuple(min(1.0, 0.055 + x * 0.26) for x in base)
    core_glow = tuple(min(1.0, 0.72 + x * 0.28) for x in glow)
    core = pbr("Core", core_base, core_glow, 0.34, 0.075, seed=family_seed + 1)
    inner = pbr("CoreInner", (0.72, 0.82, 1.0), core_glow, 0.08, 0.045, seed=family_seed + 4)

    frame_base = tuple(min(1.0, 0.12 + x * 0.28) for x in glow)
    frame_glow = tuple(min(1.0, 0.55 + x * 0.45) for x in glow)
    frame = pbr("OrbitalFrame", frame_base, frame_glow, 0.90, 0.07, seed=family_seed + 2)
    bright = pbr("OrbitalBright", frame_glow, frame_glow, 0.68, 0.055, seed=family_seed + 3)

    # The physical energy nucleus is deliberately small. Outer frames carry the
    # silhouette; bloom is not allowed to inflate the core into the whole asset.
    core_radius = 0.145 + tier * 0.0045
    parts = [
        ico("Core", core_radius, core, (1.0, 1.0, 1.04), 6 if tier >= 5 else 5),
        ico("CoreInner", core_radius * 0.34, inner, (1.0, 1.0, 1.08), 5),
    ]

    # A recessed dark shell gives the tower a physical volume between the
    # luminous nucleus and the orbital hardware.
    shell = pbr("CoreShell", tuple(x * 0.18 for x in base), tuple(x * 0.42 for x in glow), 0.86, 0.16, seed=family_seed + 5)
    parts.append(ico("CoreShell", core_radius * 1.42, shell, (1.04, 0.92, 1.04), 5 if tier >= 5 else 4))

    # Four small anchor collars visually connect the energy core to the frame.
    for i, a in enumerate((0.0, math.pi / 2, math.pi, math.pi * 1.5)):
        d = np.array((math.cos(a), 0.0, math.sin(a)))
        collar = torus(f"AnchorCollar_{i}", core_radius * 0.78, 0.026 + tier * 0.002, bright, (0.0, math.pi / 2, a))
        collar.apply_translation(d * (core_radius * 0.78))
        parts.append(collar)

    # Every tier keeps the same visual language: a glowing core plus three major
    # great-circle orbits. There are deliberately no free-standing radial rods.
    radius = 0.74 + tier * 0.034
    major_orbits = (
        (0, 0, 0),
        (math.pi / 2, 0, 0),
        (0, math.pi / 2, 0),
    )
    for i, rot in enumerate(major_orbits):
        parts.append(torus(f"Ring_{i}", radius, 0.013 + tier * 0.0015, frame, rot, sections=128, minor_sections=20))

    if tier >= 2:
        # A pair of diagonal orbital planes creates the characteristic faceted globe
        # from the reference without introducing crossing sticks.
        for i, rot in enumerate((
            (math.pi / 4, 0.0, math.pi / 6),
            (-math.pi / 4, 0.0, -math.pi / 6),
        )):
            parts.append(torus(f"Orbit_{i}", radius + 0.055, 0.011 + tier * 0.0014, frame, rot, sections=128, minor_sections=18))

    if tier >= 3:
        # Third-stage cage: four tilted curves form a stable diamond/sphere envelope.
        # Add broad armor petals at the same anchor points. These are shallow,
        # faceted surfaces, so the silhouette gains real mass instead of more rods.
        for i, a in enumerate((0.0, math.pi / 2, math.pi, math.pi * 1.5)):
            d = np.array((math.cos(a), 0.0, math.sin(a)))
            armor = plate(
                f"ArmorPetal_{i}",
                d * (radius * 0.78),
                (0.28 + tier * 0.012, 0.055 + tier * 0.004, 0.19 + tier * 0.008),
                frame,
                (0.0, -a, 0.0),
                3,
            )
            parts.append(armor)
        for i, rot in enumerate((
            (math.pi / 4, math.pi / 4, 0),
            (-math.pi / 4, math.pi / 4, 0),
            (math.pi / 4, -math.pi / 4, 0),
            (-math.pi / 4, -math.pi / 4, 0),
        )):
            parts.append(torus(f"CageCurve_{i}", radius + 0.105, 0.010 + tier * 0.0012, frame, rot, sections=128, minor_sections=18))

    if tier >= 4:
        # Higher tiers add offset latitude curves, still all curved and connected.
        # These are the visual density increase, not a collection of spokes.
        for i, y in enumerate((-0.34, 0.34)):
            ring = torus(f"Latitude_{i}", radius * 0.82, 0.012 + tier * 0.0012, bright, (0, 0, 0), sections=128, minor_sections=18)
            ring.apply_translation((0.0, y, 0.0))
            parts.append(ring)

    if tier >= 5:
        # Add four large tilted "orbital petals". Scaling the torus produces the
        # diamond-like silhouette of the reference while keeping a continuous curve.
        for i, rot in enumerate((
            (0.0, math.pi / 4, 0.0),
            (0.0, -math.pi / 4, 0.0),
            (math.pi / 4, 0.0, 0.0),
            (-math.pi / 4, 0.0, 0.0),
        )):
            petal = torus(f"Petal_{i}", radius + 0.16, 0.015 + tier * 0.0013, bright, rot, sections=128, minor_sections=18)
            petal.apply_scale((1.0, 0.62, 1.0))
            parts.append(petal)

    if tier >= 5:
        # Secondary emitter housings sit between the major orbital planes.
        for i, a in enumerate((math.pi / 4, 3 * math.pi / 4, 5 * math.pi / 4, 7 * math.pi / 4)):
            d = np.array((math.cos(a), 0.18 * math.sin(a * 2.0), math.sin(a)))
            emitter = ico(
                f"EmitterHousing_{i}",
                0.10 + tier * 0.008,
                frame,
                (1.55, 0.62, 0.82),
                3,
            )
            emitter.apply_translation(d * (radius + 0.19))
            emitter.apply_transform(trimesh.transformations.rotation_matrix(-a, [0, 1, 0]))
            parts.append(emitter)

    if tier >= 6:
        # Energy nodes sit on the existing orbital frame, rather than being connected
        # by additional sticks. They become bright visual anchors at gameplay distance.
        node_count = 6
        for i in range(node_count):
            a = math.tau * i / node_count
            r = radius + 0.16
            node = ico(f"EnergyNode_{i}", 0.042 + tier * 0.003, bright, (1.0, 1.0, 1.25), 2)
            node.apply_translation((r * math.cos(a), 0.12 * math.sin(a * 2.0), r * math.sin(a)))
            parts.append(node)

    if tier >= 7:
        # VII gets six short crystal tips at the cardinal points. They are compact
        # faceted crystals, not long rods, so the silhouette remains intentional.
        for i in range(6):
            a = math.tau * i / 6
            d = np.array((math.cos(a), 0.0, math.sin(a)), dtype=np.float64)
            crystal = ico(
                f"CrownCrystal_{i}",
                0.12,
                bright,
                (1.9, 0.55, 0.55),
                2,
            )
            crystal.apply_translation(d * (radius + 0.33))
            crystal.apply_transform(trimesh.transformations.rotation_matrix(-a, [0, 1, 0]))
            parts.append(crystal)

    save(name, parts)


# ---------------------------------------------------------------------------
# Enemy family
# ---------------------------------------------------------------------------

def insect_asset(name, kind, base, glow, scale, seed):
    global _TEXTURE_PROFILE
    _TEXTURE_PROFILE = "hero" if kind in {"spider", "queen"} else "standard"
    # Dark chitin + restrained emissive seams gives the insects the armored,
    # high-contrast silhouette from the reference instead of a flat orange blob.
    chitin = pbr("Chitin", tuple(x * 0.58 for x in base), tuple(x * 0.72 for x in glow), 0.90, 0.20, seed=seed)
    dark = pbr("ArmorDark", tuple(x * 0.12 for x in base), tuple(x * 0.62 for x in glow), 0.80, 0.27, seed=seed + 1)
    core = pbr("Core", tuple(min(1.0, x * 0.55 + 0.05) for x in glow), glow, 0.35, 0.08, seed=seed + 2)
    wingmat = pbr("Wing", tuple(min(1.0, x * 0.18 + 0.02) for x in glow), glow, 0.15, 0.12, 0.48, seed=seed + 3)

    parts = []
    body_scale = (1.25, 0.78, 0.95) if kind != "flyer" else (1.0, 0.7, 0.8)
    # Insect silhouette: distinct thorax + abdomen + head. The worker/guard/flyer
    # should read as a creature from a gameplay camera, not a spherical hub with
    # spokes attached.
    body_subdivisions = 6 if kind in {"spider", "queen"} else 5
    parts.append(ico("Thorax", 0.62 * scale, chitin, (1.18, 0.78, 0.86), body_subdivisions))
    abdomen = ico("Abdomen", 0.64 * scale, dark, (1.38, 0.72, 0.82), body_subdivisions)
    abdomen.apply_translation((-0.62 * scale, -0.01 * scale, 0.0))
    parts.append(abdomen)
    parts.append(ico("Core", 0.24 * scale, core, (1, 1, 1.25), 3))
    head = ico("Head", 0.43 * scale, dark, (1.05, 0.80, 0.78), 4)
    head.apply_translation((0.60 * scale, 0.05 * scale, 0.0))
    parts.append(head)

    eye_count = 4 if kind in {"worker", "flyer"} else 6
    for i in range(eye_count):
        a = (i - (eye_count - 1) / 2) * 0.24
        eye = ico(f"Eye_{i}", 0.07 * scale, core, 1)
        eye.apply_translation((0.74 * scale, 0.27 * scale, a * scale))

    if kind in {"worker", "guard", "flyer"}:
        # Six articulated insect legs, arranged as three bilateral pairs along the
        # body. This is deliberately not a radial spider pose.
        for pair, x in enumerate((0.42, 0.0, -0.42)):
            for side_index, side in enumerate((-1, 1)):
                s = float(side)
                i = pair * 2 + side_index
                hip = (x * scale, -0.08 * scale, s * 0.30 * scale)
                knee = ((x + (0.12 if pair == 0 else -0.05 if pair == 2 else 0.02)) * scale,
                        -0.20 * scale, s * 0.78 * scale)
                foot = ((x + (0.30 if pair == 0 else -0.12 if pair == 2 else 0.04)) * scale,
                        -0.30 * scale, s * 1.18 * scale)
                parts.append(cone_between(f"Leg_{i}_Upper", hip, knee, 0.105 * scale, 0.062 * scale, chitin, 18))
                parts.append(cone_between(f"Leg_{i}_Lower", knee, foot, 0.062 * scale, 0.020 * scale, dark, 16))
                parts.append(ico(f"Leg_{i}_Joint", 0.085 * scale, core, 2))
    else:
        leg_count = 8 if kind == "spider" else 10
        for i in range(leg_count):
            a = math.tau * i / leg_count
            spread = 0.72 if kind == "spider" else 0.82
            hip = (math.cos(a) * spread * scale, -0.04 * scale, math.sin(a) * spread * scale)
            knee = (math.cos(a) * 1.18 * scale, -0.12 * scale, math.sin(a) * 1.18 * scale)
            foot = (math.cos(a) * (1.72 if kind == "spider" else 2.0) * scale, -0.34 * scale, math.sin(a) * (1.72 if kind == "spider" else 2.0) * scale)
            parts.append(cone_between(f"Leg_{i}_Upper", hip, knee, 0.12 * scale, 0.075 * scale, chitin, 18))
            parts.append(cone_between(f"Leg_{i}_Lower", knee, foot, 0.075 * scale, 0.028 * scale, dark, 16))
            parts.append(ico(f"Leg_{i}_Joint", 0.10 * scale, core, 2))

    # Mandibles / front armor make the silhouette read as a creature, not a ball
    # with cylinders attached.
    for side in (-1, 1):
        s = float(side)
        parts.append(cone_between(
            f"Mandible_{side}",
            (0.62 * scale, 0.0, 0.24 * s * scale),
            (1.02 * scale, -0.08 * scale, 0.50 * s * scale),
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
                    4 if kind in {"spider", "queen"} else 2,
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

    if kind == "spider":
        # Additional dorsal plates and sensory nodes give the spider a stronger
        # authored silhouette when the protected benchmark GLB is not present.
        for i, a in enumerate((0.0, math.pi / 2, math.pi, 3 * math.pi / 2)):
            plate_pos = (0.18 * math.cos(a) * scale, 0.34 * scale, 0.48 * math.sin(a) * scale)
            parts.append(plate(
                f"DorsalPlate_{i}",
                plate_pos,
                (0.38 * scale, 0.10 * scale, 0.24 * scale),
                chitin,
                (0.0, -a, 0.12),
                4,
            ))
            sensor = ico(f"Sensor_{i}", 0.075 * scale, core, (1.0, 1.0, 1.25), 3)
            sensor.apply_translation((0.66 * scale, 0.18 * scale, 0.24 * math.sin(a) * scale))
            parts.append(sensor)

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


def crawler_asset(name, seed=71):
    global _TEXTURE_PROFILE
    _TEXTURE_PROFILE = "standard"
    shell = pbr("CrawlerShell", (0.04, 0.15, 0.05), (0.18, 0.85, 0.08), 0.88, 0.20, seed=seed)
    dark = pbr("CrawlerArmor", (0.012, 0.028, 0.018), (0.08, 0.42, 0.04), 0.92, 0.28, seed=seed + 1)
    core = pbr("CrawlerCore", (0.08, 0.38, 0.05), (0.28, 1.0, 0.12), 0.18, 0.08, seed=seed + 2)
    parts = [
        ico("Thorax", 0.56, shell, (1.30, 0.62, 0.88), 5),
        ico("Abdomen", 0.68, dark, (1.48, 0.52, 0.82), 5),
        ico("Head", 0.32, dark, (1.08, 0.70, 0.72), 4),
        ico("Core", 0.16, core, (1.0, 1.0, 1.15), 3),
    ]
    parts[2].apply_translation((0.66, 0.0, 0.0))
    for i, x in enumerate((0.48, 0.16, -0.18, -0.50)):
        for side in (-1, 1):
            s=float(side)
            hip=(x,-0.04,0.20*s)
            knee=(x+(0.10 if i<2 else -0.04),-0.18,0.64*s)
            foot=(x+(0.22 if i<2 else -0.16),-0.31,0.98*s)
            parts.append(cone_between(f"Leg_{i}_{side}_Upper",hip,knee,0.085,0.045,shell,16))
            parts.append(cone_between(f"Leg_{i}_{side}_Lower",knee,foot,0.045,0.014,dark,14))
            parts.append(ico(f"Leg_{i}_{side}_Joint",0.065,core,2))
    for i,x in enumerate((-0.42,-0.08,0.26,0.58)):
        parts.append(plate(f"ArmorScale_{i}",(x,0.24,0.0),(0.25,0.07,0.22),shell,(0.0,0.1*i,0.0),3))
    parts.append(cone_between("ToxinLance",(0.72,0.0,0.0),(1.20,-0.04,0.0),0.08,0.014,core,16))
    for side in (-1,1):
        parts.append(cone_between(f"Mandible_{side}",(0.72,0.0,0.13*side),(1.02,-0.10,0.30*side),0.06,0.012,dark,14))
    save(name,parts)

def psionic_asset(name, seed=83):
    global _TEXTURE_PROFILE
    _TEXTURE_PROFILE = "standard"
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

def player_asset(kind, base, glow, seed):
    global _TEXTURE_PROFILE
    _TEXTURE_PROFILE = "hero"
    # Six distinct character silhouettes. These are authored offline as GLBs, so the
    # gameplay renderer only loads meshes and never reconstructs them from primitives.
    shell = pbr("CharacterShell", tuple(x * 0.22 for x in base), glow, 0.90, 0.11, 1.0, seed=seed)
    core = pbr("CharacterCore", tuple(min(1.0, 0.16 + x * 0.48) for x in base), glow, 0.26, 0.06, 1.0, seed=seed + 1)
    core_inner = pbr("CharacterCoreInner", (0.72, 0.84, 1.0), glow, 0.08, 0.045, 1.0, seed=seed + 4)
    frame = pbr("CharacterFrame", tuple(min(1.0, 0.18 + x * 0.34) for x in glow), glow, 0.95, 0.075, 1.0, seed=seed + 2)
    accent = pbr("CharacterAccent", tuple(min(1.0, 0.24 + x * 0.30) for x in glow), glow, 0.74, 0.095, 1.0, seed=seed + 3)

    parts = [
        ico("Core", 0.19, core, (1.0, 1.0, 1.10), 6),
        ico("CoreInner", 0.065, core_inner, (1.0, 1.0, 1.12), 5),
        ico("CoreShell", 0.43, shell, (1.0, 0.92, 0.98), 5),
    ]

    # Layered armor gives the hero a readable 3D body before the orbital FX are
    # added. Each plate is a shallow faceted shield around the central core.
    for i, a in enumerate((0.0, math.pi / 3, 2 * math.pi / 3, math.pi, 4 * math.pi / 3, 5 * math.pi / 3)):
        d = np.array((math.cos(a), 0.0, math.sin(a)))
        armor = plate(
            f"ArmorPanel_{i}",
            d * 0.43 + np.array((0.0, 0.02 * math.sin(a * 3.0), 0.0)),
            (0.25, 0.075, 0.18),
            shell,
            (0.0, -a, 0.0),
            4,
        )
        parts.append(armor)

    for i, a in enumerate((math.pi / 6, math.pi / 2, 5 * math.pi / 6, 7 * math.pi / 6, 3 * math.pi / 2, 11 * math.pi / 6)):
        d = np.array((math.cos(a), 0.20 * math.sin(a), math.sin(a)))
        fin = plate(
            f"EnergyFin_{i}",
            d * 0.70,
            (0.18, 0.035, 0.065),
            accent,
            (0.0, -a, math.pi / 10),
            2,
        )
        parts.append(fin)

    def ring(name, radius, minor, material, rot=(0, 0, 0), sections=80):
        parts.append(torus(name, radius, minor, material, rot, sections=sections))

    def spike(name, direction, length, thickness, material, offset=0.18):
        # Character accents are compact emitters, not long floating rods.
        d = np.asarray(direction, dtype=np.float64)
        d = d / (np.linalg.norm(d) or 1.0)
        start = d * offset
        end = d * min(length, 0.92)
        parts.append(cone_between(name, start, end, thickness * 1.15, thickness * 0.18, material, 14))

    def node(name, pos, radius=0.055, material=accent):
        n = ico(name, radius, material, (1.0, 1.0, 1.25), 2)
        n.apply_translation(np.asarray(pos, dtype=np.float64))
        parts.append(n)

    if kind == "spherist":
        for i, (r, minor, rot) in enumerate((
            (0.63, 0.034, (0, 0, 0)),
            (0.84, 0.027, (math.pi / 2, 0, 0)),
            (1.04, 0.018, (0, math.pi / 2, 0)),
        )):
            ring(f"Ring_{i}", r, minor, frame, rot)
        for i, a in enumerate((0, math.pi / 2, math.pi, math.pi * 1.5)):
            d = (math.cos(a), 0.0, math.sin(a))
            spike(f"Cardinal_{i}", d, 0.92, 0.045, frame, 0.30)
            node(f"EnergyNode_{i}", (0.86 * d[0], 0.03, 0.86 * d[2]))
        ring("Halo", 1.13, 0.010, accent, (math.pi / 4, 0, math.pi / 8), sections=96)

    elif kind == "hunter":
        # Targeting reticle: concentric planes and four precise cardinal emitters.
        for i, r in enumerate((0.56, 0.76, 0.96, 1.14)):
            ring(f"Ring_{i}", r, 0.022 if i < 3 else 0.014, frame if i < 3 else accent,
                 (0, 0, 0) if i % 2 == 0 else (math.pi / 2, 0, 0), sections=96)
        for i, a in enumerate((0, math.pi / 2, math.pi, math.pi * 1.5)):
            d = (math.cos(a), 0, math.sin(a))
            spike(f"Cardinal_{i}", d, 0.96, 0.052, accent, 0.34)
            node(f"TargetNode_{i}", (0.86 * d[0], 0.0, 0.86 * d[2]), 0.060, core)
        for i, a in enumerate((math.pi / 4, 3 * math.pi / 4, 5 * math.pi / 4, 7 * math.pi / 4)):
            d = (math.cos(a), 0.12, math.sin(a))
            node(f"TrackNode_{i}", tuple(0.92 * np.asarray(d)), 0.042, accent)

    elif kind == "engineer":
        # A suspended 3D network. Nodes are connected into a real volumetric lattice.
        verts = np.array([
            (-0.86, -0.42, -0.58), (-0.86, -0.42, 0.58), (-0.86, 0.42, -0.58), (-0.86, 0.42, 0.58),
            (0.86, -0.42, -0.58), (0.86, -0.42, 0.58), (0.86, 0.42, -0.58), (0.86, 0.42, 0.58),
        ], dtype=np.float64)
        edges = []
        for i, v in enumerate(verts):
            node(f"NetworkNode_{i}", v * 1.02, 0.070, core)
            for j in range(i + 1, len(verts)):
                w = verts[j]
                if np.linalg.norm(v - w) < 1.25 or abs(v[0] - w[0]) < 0.01:
                    edges.append((i, j))
        for i, (u, v) in enumerate(edges):
            parts.append(cone_between(f"Link_{i}", verts[u], verts[v], 0.018, 0.010, frame, 14))
        ring("Ring_0", 0.98, 0.016, accent, (0, 0, 0), sections=96)
        ring("Ring_1", 1.10, 0.010, frame, (math.pi / 2, 0, 0), sections=96)
        ring("VerticalRing", 1.02, 0.010, accent, (0, math.pi / 2, 0), sections=96)

    elif kind == "berserker":
        # Dense radial crystal burst with uneven shard lengths. No uniform gear look.
        ring("Ring_0", 0.68, 0.026, frame, (0, 0, 0), sections=96)
        ring("Ring_1", 0.93, 0.018, accent, (math.pi / 2, 0, 0), sections=96)
        for i in range(16):
            a = math.tau * i / 16.0
            length = 1.18 + 0.27 * (0.5 + 0.5 * math.sin(i * 2.7 + 1.4))
            d = (math.cos(a), 0.0, math.sin(a))
            spike(f"RageSpike_{i}", d, min(length, 0.98), 0.062 if i % 2 == 0 else 0.042, accent, 0.26)
            if i % 2 == 0:
                node(f"RageNode_{i}", (0.76 * d[0], 0.02, 0.76 * d[2]), 0.050, core)
        for i in range(8):
            a = math.tau * i / 8.0 + math.pi / 8
            d = (0.52 * math.cos(a), 0.24 * math.sin(i), 0.52 * math.sin(a))
            rage_plate = ico(f"RageShard_{i}", 0.14, accent, (1.8, 0.22, 0.60), 2)
            rage_plate.apply_translation(d)
            rage_plate.apply_transform(trimesh.transformations.rotation_matrix(a, [0, 1, 0]))
            parts.append(rage_plate)

    elif kind == "alchemist":
        # Organic alchemical compass: layered rings, leaf-like satellite crystals and a
        # central catalyst. The silhouette is asymmetric enough to read as its own hero.
        ring("Ring_0", 0.62, 0.028, frame, (0, 0, 0), sections=96)
        ring("Ring_1", 0.86, 0.019, accent, (math.pi / 2, 0, 0), sections=96)
        ring("Ring_2", 1.06, 0.012, frame, (0, math.pi / 2, 0), sections=96)
        for i, a in enumerate((math.pi / 4, 3 * math.pi / 4, 5 * math.pi / 4, 7 * math.pi / 4)):
            d = (math.cos(a), 0, math.sin(a))
            spike(f"LeafSpine_{i}", d, 0.92, 0.036, accent, 0.28)
            leaf = ico(f"Leaf_{i}", 0.16, accent, (1.65, 0.26, 0.72), 2)
            leaf.apply_translation((0.84 * d[0], 0.16 * math.sin(a * 2), 0.84 * d[2]))
            leaf.apply_transform(trimesh.transformations.rotation_matrix(a, [0, 1, 0]))
            parts.append(leaf)
            node(f"ReagentNode_{i}", (0.56 * d[0], 0.05, 0.56 * d[2]), 0.050, core)
        ring("CatalystHalo", 0.42, 0.012, core, (math.pi / 4, math.pi / 7, 0), sections=72)

    elif kind == "architect":
        # Gold wireframe polyhedron: outer cube, diagonals, inner octahedral frame.
        cube = np.array([
            (-0.90,-0.90,-0.90), (-0.90,-0.90,0.90), (-0.90,0.90,-0.90), (-0.90,0.90,0.90),
            (0.90,-0.90,-0.90), (0.90,-0.90,0.90), (0.90,0.90,-0.90), (0.90,0.90,0.90),
        ], dtype=np.float64)
        cube_edges = [(0,1),(0,2),(0,4),(1,3),(1,5),(2,3),(2,6),(3,7),(4,5),(4,6),(5,7),(6,7)]
        for i,p in enumerate(cube):
            node(f"Vertex_{i}", p, 0.060, accent)
        for i,(u,v) in enumerate(cube_edges):
            parts.append(cone_between(f"Frame_{i}", cube[u], cube[v], 0.020, 0.010, frame, 16))
        diagonals = [(0,7),(1,6),(2,5),(3,4),(0,3),(1,2),(4,7),(5,6)]
        for i,(u,v) in enumerate(diagonals):
            parts.append(cone_between(f"Diagonal_{i}", cube[u], cube[v], 0.014, 0.007, accent, 12))
        for i,d in enumerate(((1,0,0),(-1,0,0),(0,1,0),(0,-1,0),(0,0,1),(0,0,-1))):
            spike(f"AxisTip_{i}", d, 0.94, 0.042, accent, 0.34)
        ring("Ring_0", 1.10, 0.010, frame, (0, 0, 0), sections=96)
        ring("Ring_1", 1.12, 0.010, frame, (math.pi / 2, 0, 0), sections=96)
        ring("VerticalRing", 1.12, 0.010, accent, (0, math.pi / 2, 0), sections=96)

    else:
        raise ValueError(f"unknown character visual: {kind}")

    save(f"player_{kind}", parts)


def player_core_asset():
    """Small authored energy core used by minions, not a duplicate hero model."""
    global _TEXTURE_PROFILE
    _TEXTURE_PROFILE = "standard"
    core = pbr("MinionCore", (.035, .22, .42), (.10, .65, .95), 0.22, 0.07, seed=171)
    frame = pbr("MinionCoreFrame", (.04, .16, .30), (.08, .42, .85), 0.78, 0.11, seed=172)
    save("player_core", [
        ico("Core", 0.32, core, (1.0, 1.0, 1.08), 5),
        torus("Ring_0", 0.47, 0.026, frame, (math.pi / 2, 0, 0), sections=96, minor_sections=12),
        torus("Ring_1", 0.49, 0.018, frame, (0, math.pi / 2, 0), sections=96, minor_sections=10),
    ])



def boss_asset(name, kind, seed):
    global _TEXTURE_PROFILE
    _TEXTURE_PROFILE = "hero"
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
        ico("Body", 1.15, body, (1.1, 0.9, 1.1), 6),
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
        parts.append(torus(f"Ring_{i}", r, 0.035 + i * 0.008, core, (0, 0, 0) if i != 1 else (math.pi / 2, 0, 0), sections=128, minor_sections=22))
    save(name, parts)


def projectile(name, base, glow, seed):
    global _TEXTURE_PROFILE
    _TEXTURE_PROFILE = "standard"
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
protected_names = PROTECTED_SOURCE_ASSETS
for old in OUT.glob("*.glb"):
    if old.stem.startswith(owned_prefixes) and old.name not in protected_names:
        old.unlink()

for family, base, glow, seed in SPHERE_FAMILIES:
    for tier in range(1, 8):
        sphere_asset(f"sphere_{family}_t{tier}", base, glow, tier, seed + tier * 7)

insect_asset("enemy_worker", "worker", (.15, .02, .03), (1.0, .25, .04), .58, 201)
insect_asset("enemy_guard", "guard", (.16, .02, .03), (1.0, .08, .02), .82, 211)
insect_asset("enemy_flyer", "flyer", (.12, .02, .18), (.80, .15, 1.0), .60, 221)
# If the user-supplied benchmark GLB is present it survives generation and is used
# verbatim. Until that source file is committed to the repository, generate a
# clearly marked fallback so CI remains executable rather than silently claiming
# the benchmark was used.
if not (OUT / "enemy_spider.glb").exists():
    print("[WARN] enemy_spider benchmark GLB is not present; generating fallback spider asset")
    insect_asset("enemy_spider", "spider", (.08, .015, .12), (1.0, .10, .55), .98, 231)
crawler_asset("enemy_crawler")
psionic_asset("enemy_psionic")
insect_asset("enemy_queen", "queen", (.18, .07, .02), (1.0, .45, .06), 1.18, 241)

player_asset("spherist", (.05, .55, 1.0), (.25, .95, 1.0), 101)
player_asset("hunter", (.45, .05, .95), (.80, .25, 1.0), 111)
player_asset("engineer", (.03, .60, .78), (.15, 1.0, 1.0), 121)
player_asset("berserker", (.82, .03, .02), (1.0, .13, .04), 131)
player_asset("alchemist", (.12, .72, .04), (.42, 1.0, .18), 141)
player_asset("architect", (.72, .48, .03), (1.0, .78, .18), 151)
player_core_asset()
boss_asset("boss_colony", "colony", 301)
boss_asset("boss_distortion", "distortion", 311)
boss_asset("boss_singularity", "singularity", 321)
projectile("projectile_energy", (.05, .55, 1.0), (.10, .82, 1.0), 401)
projectile("projectile_fire", (.90, .12, .02), (1.0, .42, .05), 411)

count = len(list(OUT.glob("*.glb")))
print(f"Generated {count} production GLB assets in {OUT}")
