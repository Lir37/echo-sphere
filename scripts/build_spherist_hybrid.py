"""Build the production Spherist by combining the user-authored body with the
previous procedural Spherist's missing structural language.

The user's GLB remains the primary mesh/material source. This script only adds the
parts that gave the earlier Spherist its readable identity: orbital rings, layered
armor accents, energy fins, cardinal emitters, nodes and an inner energy core.
"""

from pathlib import Path
import math

import numpy as np
import trimesh
from PIL import Image, ImageFilter


def _texture(base, accent, seed, size=1024):
    rng = np.random.default_rng(seed)
    low = Image.fromarray(np.uint8(rng.random((64, 64)) * 255), "L")
    low = low.resize((size, size), Image.Resampling.BICUBIC).filter(ImageFilter.GaussianBlur(1.25))
    noise = np.asarray(low, dtype=np.float32) / 255.0
    y, x = np.mgrid[0:size, 0:size]
    circuit = (
        ((x % 47) < 2)
        | ((y % 71) < 2)
        | (((x + 2 * y) % 109) < 1)
    )
    b = np.asarray(base, dtype=np.float32)[None, None, :]
    a = np.asarray(accent, dtype=np.float32)[None, None, :]
    rgb = b * (0.62 + noise[..., None] * 0.30) + a * (noise[..., None] * 0.08)
    rgb[circuit] = np.clip(a * 0.72 + b * 0.28, 0, 1)
    return Image.fromarray(np.uint8(np.clip(rgb, 0, 1) * 255), "RGB")


def _normal(seed, size=512):
    rng = np.random.default_rng(seed)
    h = np.asarray(
        Image.fromarray(np.uint8(rng.random((64, 64)) * 255), "L")
        .resize((size, size), Image.Resampling.BICUBIC)
        .filter(ImageFilter.GaussianBlur(1.1)),
        dtype=np.float32,
    ) / 255.0
    dx = np.gradient(h, axis=1) * 2.5
    dy = np.gradient(h, axis=0) * 2.5
    length = np.sqrt(dx * dx + 1.0 + dy * dy)
    rgb = np.stack((-dx / length * 0.5 + 0.5, 1.0 / length * 0.5 + 0.5, -dy / length * 0.5 + 0.5), -1)
    return Image.fromarray(np.uint8(np.clip(rgb, 0, 1) * 255), "RGB")


def _metal_rough(metallic, roughness, seed, size=512):
    rng = np.random.default_rng(seed)
    field = rng.random((size, size)) * 0.18
    rgb = np.stack(
        (np.zeros_like(field), np.clip(roughness * (0.82 + field), 0, 1), np.full_like(field, metallic)),
        -1,
    )
    return Image.fromarray(np.uint8(rgb * 255), "RGB")


def _material(name, base, accent, metallic, roughness, emissive, seed):
    return trimesh.visual.material.PBRMaterial(
        name=name,
        baseColorTexture=_texture(base, accent, seed),
        normalTexture=_normal(seed + 100),
        metallicRoughnessTexture=_metal_rough(metallic, roughness, seed + 200),
        metallicFactor=metallic,
        roughnessFactor=roughness,
        emissiveFactor=tuple(emissive),
    )


def _uv(mesh):
    vertices = np.asarray(mesh.vertices, dtype=np.float32)
    radius = np.maximum(np.linalg.norm(vertices, axis=1), 1e-6)
    uv = np.column_stack(
        (
            np.arctan2(vertices[:, 2], vertices[:, 0]) / math.tau + 0.5,
            np.arcsin(np.clip(vertices[:, 1] / radius, -1, 1)) / math.pi + 0.5,
        )
    )
    return uv.astype(np.float32)


def _assign(mesh, material):
    mesh.visual = trimesh.visual.TextureVisuals(uv=_uv(mesh), material=material)
    return mesh


def _torus(name, major, minor, material, rotation=(0, 0, 0), sections=112):
    mesh = trimesh.creation.torus(
        major_radius=major,
        minor_radius=minor,
        major_sections=sections,
        minor_sections=16,
        transform=trimesh.transformations.euler_matrix(*rotation),
    )
    mesh.metadata["name"] = name
    return _assign(mesh, material)


def _ico(name, radius, material, scale=(1, 1, 1), subdivisions=3):
    mesh = trimesh.creation.icosphere(subdivisions=subdivisions, radius=radius)
    mesh.apply_scale(scale)
    mesh.metadata["name"] = name
    return _assign(mesh, material)


def _cone_between(name, a, b, r1, r2, material, sections=14):
    a = np.asarray(a, dtype=np.float64)
    b = np.asarray(b, dtype=np.float64)
    axis = b - a
    length = float(np.linalg.norm(axis))
    angles = np.linspace(0.0, math.tau, sections, endpoint=False)
    circle0 = np.column_stack((r1 * np.cos(angles), r1 * np.sin(angles), np.zeros(sections)))
    circle1 = np.column_stack((r2 * np.cos(angles), r2 * np.sin(angles), np.full(sections, length)))
    vertices = np.vstack((circle0, circle1))
    faces = []
    for i in range(sections):
        j = (i + 1) % sections
        faces.extend(((i, j, sections + j), (i, sections + j, sections + i)))
    faces.extend((0, i + 1, i) for i in range(1, sections - 1))
    faces.extend((sections, sections + i, sections + i + 1) for i in range(1, sections - 1))
    mesh = trimesh.Trimesh(vertices=vertices, faces=np.asarray(faces), process=True)
    mesh.apply_translation((a + b) / 2.0 - np.array([0.0, 0.0, length / 2.0]))
    mesh.apply_transform(trimesh.geometry.align_vectors([0, 0, 1], axis))
    mesh.metadata["name"] = name
    uv = np.column_stack(
        (
            np.arctan2(vertices[:, 1], vertices[:, 0]) / math.tau + 0.5,
            vertices[:, 2] / max(length, 1e-6),
        )
    ).astype(np.float32)
    mesh.visual = trimesh.visual.TextureVisuals(uv=uv, material=material)
    return mesh


def _armor_panel(name, angle, center_y, material):
    direction = np.array([math.cos(angle), 0.0, math.sin(angle)])
    mesh = _ico(name, 1.0, material, (0.34, 0.10, 0.24), 3)
    mesh.apply_transform(trimesh.transformations.rotation_matrix(-angle, [0, 1, 0]))
    mesh.apply_translation(direction * 0.70 + np.array([0.0, center_y, 0.0]))
    return mesh


def build_hybrid(source: Path, destination: Path) -> None:
    scene = trimesh.load(source, force="scene")
    raw = next(iter(scene.geometry.values()))
    lo, hi = raw.bounds
    center_y = float((lo[1] + hi[1]) * 0.5)
    # The user-authored body is the hero geometry. Enlarge it slightly so its
    # detailed silhouette and PBR surface remain visible instead of being read
    # as the small core inside the old orbital language.
    raw.apply_scale(1.32)

    # The user's authored GLB defines the silhouette, surface detail and PBR body.
    # Added geometry stays deliberately subordinate to that body.
    frame = _material(
        "SpheristFrame", (0.025, 0.16, 0.42), (0.08, 0.72, 1.0),
        0.95, 0.085, (0.02, 0.22, 0.55), 21,
    )
    accent = _material(
        "SpheristAccent", (0.04, 0.30, 0.62), (0.12, 0.90, 1.0),
        0.78, 0.07, (0.06, 0.55, 0.9), 22,
    )
    core = _material(
        "SpheristCore", (0.06, 0.25, 0.55), (0.15, 0.95, 1.0),
        0.25, 0.05, (0.10, 0.70, 1.0), 23,
    )

    parts = []

    # The three rings and outer halo are the strongest readable traits from the
    # previous Spherist. They are centered on the user's actual mesh, not replaced.
    for index, (radius, minor, rotation) in enumerate(
        (
            (0.78, 0.018, (0, 0, 0)),
            (0.98, 0.015, (math.pi / 2, 0, 0)),
            (1.18, 0.011, (0, math.pi / 2, 0)),
        )
    ):
        ring = _torus(f"Ring_{index}", radius, minor, frame, rotation)
        ring.apply_translation((0.0, center_y, 0.0))
        parts.append(ring)

    halo = _torus("Halo", 1.30, 0.008, accent, (math.pi / 4, 0, math.pi / 8), 128)
    halo.apply_translation((0.0, center_y, 0.0))
    parts.append(halo)

    # Layered armor panels add the missing hard-surface structure without hiding
    # the user's detailed body texture.
    for index, angle in enumerate(np.linspace(0, math.tau, 6, endpoint=False)):
        parts.append(_armor_panel(f"ArmorPanel_{index}", float(angle), center_y, frame))

    # Energy fins reproduce the earlier model's silhouette language.
    for index, angle in enumerate(
        np.linspace(math.pi / 6, math.tau + math.pi / 6, 6, endpoint=False)
    ):
        direction = np.array([math.cos(angle), 0.12 * math.sin(angle), math.sin(angle)])
        fin = _ico(f"EnergyFin_{index}", 1.0, accent, (0.22, 0.045, 0.085), 2)
        fin.apply_transform(trimesh.transformations.rotation_matrix(-angle, [0, 1, 0]))
        fin.apply_translation(direction * 0.95 + np.array([0.0, center_y, 0.0]))
        parts.append(fin)

    # Compact cardinal emitters and energy nodes are deliberately shorter than the
    # earlier procedural rods, so the user's geometry remains the hero.
    for index, angle in enumerate((0.0, math.pi / 2, math.pi, 3 * math.pi / 2)):
        direction = np.array([math.cos(angle), 0.0, math.sin(angle)])
        origin = np.array([0.0, center_y, 0.0])
        parts.append(
            _cone_between(
                f"Cardinal_{index}",
                origin + direction * 0.35,
                origin + direction * 0.92,
                0.020,
                0.006,
                frame,
            )
        )
        node = _ico(f"EnergyNode_{index}", 0.038, core, (1, 1, 1.25), 3)
        node.apply_translation(origin + direction * 0.96)
        parts.append(node)

    # The raw model is opaque in places but has cavities and bright seams. A small
    # inner core gives those openings a controlled source of light.
    inner = _ico("Core", 0.19, core, (1, 1, 1.08), 4)
    inner.apply_translation((0.0, center_y, 0.0))
    parts.append(inner)
    core_inner = _ico("CoreInner", 0.075, accent, (1, 1, 1.12), 4)
    core_inner.apply_translation((0.0, center_y, 0.0))
    parts.append(core_inner)

    output_scene = trimesh.Scene()
    output_scene.add_geometry(raw, node_name="UserAuthoredBody")
    for part in parts:
        output_scene.add_geometry(part, node_name=part.metadata["name"])

    destination.parent.mkdir(parents=True, exist_ok=True)
    output_scene.export(destination, file_type="glb")
