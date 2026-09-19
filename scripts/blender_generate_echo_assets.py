"""Echo Sphere offline asset generator.
Run with Blender 4.x: blender -b --python scripts/blender_generate_echo_assets.py
The game never builds geometry at runtime. This script creates authored meshes,
materials, named animation nodes and exports production GLBs to public/art3d.
"""
import bpy, math
from mathutils import Vector
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1] / 'public' / 'art3d'
ROOT.mkdir(parents=True,exist_ok=True)

def clean():
    bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)

def material(name, base, emission, metallic=.7, rough=.25):
    m=bpy.data.materials.new(name); m.use_nodes=True
    bs=m.node_tree.nodes.get('Principled BSDF')
    bs.inputs['Base Color'].default_value=(*base,1)
    bs.inputs['Metallic'].default_value=metallic
    bs.inputs['Roughness'].default_value=rough
    bs.inputs['Emission Color'].default_value=(*emission,1)
    bs.inputs['Emission Strength'].default_value=3.0
    return m

def ico(name,r,mat,sub=3):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub,radius=r)
    o=bpy.context.object;o.name=name;o.data.materials.append(mat);return o

def torus(name,r,minor,mat,rot=(0,0,0)):
    bpy.ops.mesh.primitive_torus_add(major_radius=r,minor_radius=minor,major_segments=64,minor_segments=12,rotation=rot)
    o=bpy.context.object;o.name=name;o.data.materials.append(mat);return o

def limb(name,a,b,r1,r2,mat):
    d=Vector(b)-Vector(a); mid=(Vector(a)+Vector(b))/2
    bpy.ops.mesh.primitive_cone_add(vertices=12,radius1=r1,radius2=r2,depth=d.length,location=mid)
    o=bpy.context.object;o.name=name;o.data.materials.append(mat);o.rotation_mode='QUATERNION';o.rotation_quaternion=Vector((0,0,1)).rotation_difference(d.normalized());return o

def export(name):
    for o in bpy.context.scene.objects:o.select_set(True)
    bpy.context.view_layer.objects.active=bpy.context.scene.objects[0]
    bpy.ops.export_scene.gltf(filepath=str(ROOT/f'{name}.glb'),export_format='GLB',use_selection=False,export_apply=True,export_animations=True,export_materials='EXPORT')

def sphere(name,base,glow):
    clean(); shell=material('Shell',(.025,.06,.09),glow,.85,.18); coremat=material('Core',base,glow,.45,.1)
    ico('Core',.55,coremat,4); shello=ico('Housing',1.02,shell,4); shello.scale=(1,.86,.94)
    for i,r in enumerate((.78,.94,1.08)):
        torus(f'Ring_{i}',r,.045 if i<2 else .06,material(f'RingMat{i}',glow,glow,.65,.14),(math.pi/2,0,0) if i==1 else (0,0,0))
    for i in range(6):
        a=i*math.tau/6; n=ico(f'EnergyNode_{i}',.11,coremat,2); n.location=(math.cos(a)*.82,math.sin(a)*.82,0)
    export(name)

def spider(name,base,glow,scale=.8,legs=8):
    clean(); bodymat=material('Chitin',base,glow,.8,.2); coremat=material('Core',(.12,.02,.2),glow,.35,.1)
    body=ico('Body',.72,bodymat,4);body.scale=(1,.75,.82);ico('Core',.28,coremat,3)
    for i in range(legs):
        a=i*math.tau/legs; bend=(math.cos(a)*1.0,.22,math.sin(a)*1.0); end=(math.cos(a)*1.8,-.25,math.sin(a)*1.8)
        limb(f'Leg_{i}_Upper',(math.cos(a)*.42,0,math.sin(a)*.42),bend,.13,.085,bodymat)
        limb(f'Leg_{i}_Lower',bend,end,.085,.045,bodymat)
    export(name)

def boss(name,kind):
    if kind=='colony': spider(name,(.22,.03,.015),(1,.16,.02),1.55,10);return
    clean(); base=(.08,.03,.16) if kind=='distortion' else (.04,.1,.18); glow=(.75,.1,1) if kind=='distortion' else (.1,.6,1)
    bm=material('BossBody',base,glow,.86,.16); cm=material('Core',(.2,.02,.35) if kind=='distortion' else (.04,.3,.8),glow,.3,.08)
    ico('Body',1.05,bm,4);ico('VoidCore' if kind=='distortion' else 'SingularityCore',.52,cm,4)
    count=12 if kind=='distortion' else 9
    for i in range(count):
        a=i*math.tau/count; limb(f'OrbitArm_{i}',(math.cos(a)*.5,0,math.sin(a)*.5),(math.cos(a)*1.8,.45*math.sin(a*2),math.sin(a)*1.8),.14,.025,bm)
    export(name)

for n,c,e in [('sphere_standard',(.05,.55,1),(.25,.95,1)),('sphere_sniper',(.45,.05,.95),(.8,.25,1)),('sphere_shotgun',(.95,.25,.04),(1,.55,.1)),('sphere_chain',(.95,.72,.03),(1,.9,.25)),('sphere_aura',(.03,.75,.5),(.15,1,.7)),('player_core',(.05,.55,1),(.2,.9,1))]: sphere(n,c,e)
spider('enemy_worker',(.15,.02,.03),(1,.25,.04),.55,6)
spider('enemy_guard',(.16,.02,.03),(1,.08,.02),.78,8)
spider('enemy_flyer',(.12,.02,.18),(.8,.15,1),.52,6)
spider('enemy_spider',(.08,.015,.12),(1,.1,.55),.92,8)
spider('enemy_queen',(.18,.07,.02),(1,.45,.06),1.15,8)
boss('boss_colony','colony');boss('boss_distortion','distortion');boss('boss_singularity','singularity')
