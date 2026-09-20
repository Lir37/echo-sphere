"""Offline GLB asset generator for Echo Sphere.

The runtime never constructs gameplay meshes. This script authors the reusable 3D
assets offline and exports them to public/art3d for the web/Android build.
"""
from pathlib import Path
import math
import numpy as np
import trimesh

OUT = Path(__file__).resolve().parents[1] / "public" / "art3d"
OUT.mkdir(parents=True, exist_ok=True)

def mat(base, emission, metallic=0.7, rough=0.22):
    return trimesh.visual.material.PBRMaterial(
        baseColorFactor=(*base, 1),
        emissiveFactor=emission,
        metallicFactor=metallic,
        roughnessFactor=rough,
    )

def ico(radius, material, scale=(1, 1, 1), subdivisions=3):
    o = trimesh.creation.icosphere(subdivisions=subdivisions, radius=radius)
    o.apply_scale(scale)
    o.visual.material = material
    return o

def torus(radius, minor, material, sections=48):
    o = trimesh.creation.torus(
        major_radius=radius, minor_radius=minor,
        major_sections=sections, minor_sections=10,
    )
    o.visual.material = material
    return o

def tube(a, b, radius, material, sections=12):
    a, b = np.asarray(a, float), np.asarray(b, float)
    d = b - a
    o = trimesh.creation.cylinder(radius, float(np.linalg.norm(d)), sections=sections)
    o.visual.material = material
    o.apply_translation((a + b) / 2)
    o.apply_transform(trimesh.geometry.align_vectors([0, 0, 1], d))
    return o

def save(name, parts):
    scene = trimesh.Scene()
    for idx, part in enumerate(parts):
        scene.add_geometry(part, node_name=f"Part_{idx:02d}")
    scene.export(OUT / f"{name}.glb", file_type="glb")

def sphere(name, base, glow, tier=7):
    # Tier 1 is intentionally restrained. Tier 7 gets the full layered silhouette.
    M = mat(base, glow, 0.78, max(0.11, 0.30 - tier * 0.022))
    G = mat(tuple(min(1, x * 1.15) for x in glow), glow, 0.52, 0.12)
    parts = [
        ico(0.55, M, subdivisions=3),
        ico(1.0, G, (1, 0.86, 0.94), subdivisions=3),
    ]
    ring_count = 1 + tier
    for i in range(ring_count):
        r = 0.68 + i * (0.42 / max(1, ring_count - 1))
        parts.append(torus(r, 0.028 + tier * 0.004, G, sections=48))
    node_count = 2 + tier
    for i in range(node_count):
        a = i * math.tau / node_count
        n = ico(0.075 + tier * 0.006, M, subdivisions=2)
        n.apply_translation((0.82 * math.cos(a), 0.82 * math.sin(a), 0))
        parts.append(n)
    if tier >= 4:
        for axis in range(3):
            parts.append(torus(0.56 + axis * 0.14, 0.018, G, sections=36))
    if tier >= 6:
        for i in range(3):
            a = i * math.tau / 3
            parts.append(tube(
                (0, 0, 0),
                (0.9 * math.cos(a), 0.28, 0.9 * math.sin(a)),
                0.028, G, sections=10,
            ))
    save(name, parts)

def creature(name, base, glow, legs=8, scale=1):
    M = mat(base, glow, 0.72, 0.18)
    C = mat((0.08, 0.01, 0.12), glow, 0.35, 0.1)
    parts = [ico(0.72 * scale, M, (1, 0.75, 0.82)), ico(0.28 * scale, C)]
    for i in range(legs):
        a = i * math.tau / max(1, legs)
        s = (math.cos(a) * 0.42 * scale, 0, math.sin(a) * 0.42 * scale)
        b = (math.cos(a) * 1 * scale, 0.22 * scale, math.sin(a) * 1 * scale)
        e = (math.cos(a) * 1.8 * scale, -0.25 * scale, math.sin(a) * 1.8 * scale)
        parts += [tube(s, b, 0.13 * scale, M), tube(b, e, 0.07 * scale, M)]
    save(name, parts)

def boss(name, base, glow, count):
    M = mat(base, glow, 0.82, 0.14)
    C = mat((0.12, 0.02, 0.3), glow, 0.3, 0.08)
    parts = [ico(1.05, M, subdivisions=4), ico(0.52, C, subdivisions=3)]
    for i in range(count):
        a = i * math.tau / count
        parts.append(tube(
            (math.cos(a) * 0.5, 0, math.sin(a) * 0.5),
            (math.cos(a) * 1.8, 0.45 * math.sin(2 * a), math.sin(a) * 1.8),
            0.12, M, sections=14,
        ))
    for r in (1.15, 1.38):
        parts.append(torus(r, 0.045, C, sections=64))
    save(name, parts)

# Remove stale generated files so CI artifacts exactly match this revision.
for old in OUT.glob("*.glb"):
    old.unlink()

sphere_families = [
    ("standard", (.05, .55, 1), (.25, .95, 1)),
    ("sniper", (.45, .05, .95), (.8, .25, 1)),
    ("shotgun", (.95, .25, .04), (1, .55, .1)),
    ("chain", (.95, .72, .03), (1, .9, .25)),
    ("aura", (.03, .75, .5), (.15, 1, .7)),
]
for name, base, glow in sphere_families:
    for tier in range(1, 8):
        sphere(f"sphere_{name}_t{tier}", base, glow, tier)

# Keep a default alias for code/content that does not specify a tier.
for name, base, glow in sphere_families:
    sphere(f"sphere_{name}", base, glow, 7)

for n, c, e, l, s in [
    ("enemy_worker", (.15, .02, .03), (1, .25, .04), 6, .55),
    ("enemy_guard", (.16, .02, .03), (1, .08, .02), 8, .78),
    ("enemy_flyer", (.12, .02, .18), (.8, .15, 1), 6, .52),
    ("enemy_spider", (.08, .015, .12), (1, .1, .55), 8, .92),
    ("enemy_queen", (.18, .07, .02), (1, .45, .06), 8, 1.15),
    ("enemy_psionic", (.06, .04, .18), (.55, .2, 1), 6, .72),
    ("enemy_slime", (.03, .25, .12), (.1, 1, .45), 0, .68),
]:
    creature(n, c, e, l, s)

def player_core():
    shell = mat((0.03, 0.08, 0.16), (0.15, 0.65, 1.0), 0.9, 0.12)
    core = mat((0.72, 0.92, 1.0), (0.25, 0.95, 1.0), 0.35, 0.06)
    dark = mat((0.01, 0.02, 0.05), (0.05, 0.25, 0.55), 0.7, 0.16)
    parts = [
        ico(0.95, shell, (1.0, 0.92, 1.08), subdivisions=4),
        ico(0.48, core, (1.0, 1.0, 1.15), subdivisions=4),
    ]
    for radius, minor in ((0.72, 0.045), (0.88, 0.028), (1.05, 0.018)):
        parts.append(torus(radius, minor, core, sections=64))
    for i in range(4):
        a = i * math.tau / 4 + math.pi / 4
        x, z = math.cos(a), math.sin(a)
        parts.append(tube((x * 0.58, -0.05, z * 0.58), (x * 1.12, 0.18, z * 1.12), 0.045, dark, sections=12))
    save("player_core", parts)

player_core()

boss("boss_colony", (.22, .03, .015), (1, .16, .02), 10)
boss("boss_distortion", (.08, .03, .16), (.75, .1, 1), 12)
boss("boss_singularity", (.04, .1, .18), (.1, .6, 1), 9)

for n, c, e in [
    ("projectile_energy", (.05, .55, 1), (.1, .8, 1)),
    ("projectile_fire", (.9, .12, .02), (1, .4, .05)),
]:
    M = mat(c, e, 0.3, 0.08)
    save(n, [ico(.24, M, (.8, .8, 1), subdivisions=3), torus(.34, .035, M)])

print(f"Generated {len(list(OUT.glob('*.glb')))} GLB assets in {OUT}")
