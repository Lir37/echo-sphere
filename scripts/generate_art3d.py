"""Offline GLB asset generator for Echo Sphere.
Uses trimesh only, so CI can build assets without Blender installed.
The generated assets are authored offline and loaded at runtime by visual3d.ts.
"""
from pathlib import Path
import math
import numpy as np
import trimesh

OUT = Path(__file__).resolve().parents[1] / "public" / "art3d"
OUT.mkdir(parents=True, exist_ok=True)

def mat(base, emission, metallic=.7, rough=.22):
    return trimesh.visual.material.PBRMaterial(
        baseColorFactor=(*base, 1), emissiveFactor=emission,
        metallicFactor=metallic, roughnessFactor=rough)

def ico(radius, material, scale=(1,1,1)):
    o=trimesh.creation.icosphere(subdivisions=3, radius=radius); o.apply_scale(scale); o.visual.material=material; return o

def torus(radius, minor, material):
    o=trimesh.creation.torus(major_radius=radius, minor_radius=minor, major_sections=48, minor_sections=10); o.visual.material=material; return o

def tube(a,b,r,material):
    a=np.asarray(a,float); b=np.asarray(b,float); d=b-a
    o=trimesh.creation.cylinder(r,float(np.linalg.norm(d)),sections=12); o.visual.material=material
    o.apply_translation((a+b)/2); o.apply_transform(trimesh.geometry.align_vectors([0,0,1],d)); return o

def save(name, parts): trimesh.Scene(parts).export(OUT/f"{name}.glb", file_type="glb")

def sphere(name, base, glow):
    M=mat(base,glow); G=mat(tuple(min(1,x*1.15) for x in glow),glow,.5,.12)
    parts=[ico(.55,M),ico(1,G,(1,.86,.94))]
    for r in (.78,.94,1.08): parts.append(torus(r,.045,G))
    for i in range(6):
        a=i*math.tau/6; n=ico(.11,M); n.apply_translation((.82*math.cos(a),.82*math.sin(a),0)); parts.append(n)
    save(name,parts)

def creature(name, base, glow, legs=8, scale=1):
    M=mat(base,glow); C=mat((.08,.01,.12),glow,.35,.1)
    parts=[ico(.72*scale,M,(1,.75,.82)),ico(.28*scale,C)]
    for i in range(legs):
        a=i*math.tau/legs; s=(math.cos(a)*.42*scale,0,math.sin(a)*.42*scale)
        b=(math.cos(a)*1*scale,.22*scale,math.sin(a)*1*scale); e=(math.cos(a)*1.8*scale,-.25*scale,math.sin(a)*1.8*scale)
        parts += [tube(s,b,.13*scale,M),tube(b,e,.07*scale,M)]
    save(name,parts)

def boss(name,base,glow,count):
    M=mat(base,glow); C=mat((.12,.02,.3),glow,.3,.08); parts=[ico(1.05,M),ico(.52,C)]
    for i in range(count):
        a=i*math.tau/count; parts.append(tube((math.cos(a)*.5,0,math.sin(a)*.5),(math.cos(a)*1.8,.45*math.sin(2*a),math.sin(a)*1.8),.12,M))
    save(name,parts)

for n,c,e in [
('sphere_standard',(.05,.55,1),(.25,.95,1)),('sphere_sniper',(.45,.05,.95),(.8,.25,1)),
('sphere_shotgun',(.95,.25,.04),(1,.55,.1)),('sphere_chain',(.95,.72,.03),(1,.9,.25)),
('sphere_aura',(.03,.75,.5),(.15,1,.7)),('player_core',(.05,.55,1),(.2,.9,1))]: sphere(n,c,e)
for n,c,e,l,s in [
('enemy_worker',(.15,.02,.03),(1,.25,.04),6,.55),('enemy_guard',(.16,.02,.03),(1,.08,.02),8,.78),
('enemy_flyer',(.12,.02,.18),(.8,.15,1),6,.52),('enemy_spider',(.08,.015,.12),(1,.1,.55),8,.92),
('enemy_queen',(.18,.07,.02),(1,.45,.06),8,1.15),('enemy_psionic',(.06,.04,.18),(.55,.2,1),6,.72),
('enemy_slime',(.03,.25,.12),(.1,1,.45),0,.68)]: creature(n,c,e,l,s)
boss('boss_colony',(.22,.03,.015),(1,.16,.02),10)
boss('boss_distortion',(.08,.03,.16),(.75,.1,1),12)
boss('boss_singularity',(.04,.1,.18),(.1,.6,1),9)
for n,c,e in [('projectile_energy',(.05,.55,1),(.1,.8,1)),('projectile_fire',(.9,.12,.02),(1,.4,.05))]:
    M=mat(c,e,.3,.08); save(n,[ico(.24,M,(.8,.8,1)),torus(.34,.035,M)])
print(f"Generated {len(list(OUT.glob('*.glb')))} GLB assets in {OUT}")
