import type { GameState, SphereEntity, EnemyEntity, SphereProjectile, Particle } from './engine';
import { SPHERE_TYPES, type SphereType } from './gameData';

type Vec3 = [number, number, number];

const DEG = Math.PI / 180;

function mat4Identity(): Float32Array {
  return new Float32Array([1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1]);
}
function mat4Multiply(a: Float32Array, b: Float32Array): Float32Array {
  const out = new Float32Array(16);
  for (let c = 0; c < 4; c++) for (let r = 0; r < 4; r++) {
    out[c*4+r] = a[r]*b[c*4] + a[4+r]*b[c*4+1] + a[8+r]*b[c*4+2] + a[12+r]*b[c*4+3];
  }
  return out;
}
function mat4Translate(x:number,y:number,z:number): Float32Array {
  const m=mat4Identity(); m[12]=x; m[13]=y; m[14]=z; return m;
}
function mat4Scale(x:number,y:number,z:number): Float32Array {
  const m=mat4Identity(); m[0]=x; m[5]=y; m[10]=z; return m;
}
function mat4RotateY(a:number): Float32Array {
  const c=Math.cos(a), s=Math.sin(a);
  return new Float32Array([c,0,-s,0, 0,1,0,0, s,0,c,0, 0,0,0,1]);
}
function mat4RotateX(a:number): Float32Array {
  const c=Math.cos(a), s=Math.sin(a);
  return new Float32Array([1,0,0,0, 0,c,s,0, 0,-s,c,0, 0,0,0,1]);
}
function mat4RotateZ(a:number): Float32Array {
  const c=Math.cos(a), s=Math.sin(a);
  return new Float32Array([c,s,0,0, -s,c,0,0, 0,0,1,0, 0,0,0,1]);
}
function mat4Perspective(fov:number, aspect:number, near:number, far:number):Float32Array {
  const f=1/Math.tan(fov/2), nf=1/(near-far);
  return new Float32Array([f/aspect,0,0,0, 0,f,0,0, 0,0,(far+near)*nf,-1, 0,0,(2*far*near)*nf,0]);
}
function mat4LookAt(eye:Vec3, center:Vec3, up:Vec3):Float32Array {
  let zx=eye[0]-center[0], zy=eye[1]-center[1], zz=eye[2]-center[2];
  let l=Math.hypot(zx,zy,zz)||1; zx/=l; zy/=l; zz/=l;
  let xx=up[1]*zz-up[2]*zy, xy=up[2]*zx-up[0]*zz, xz=up[0]*zy-up[1]*zx;
  l=Math.hypot(xx,xy,xz)||1; xx/=l; xy/=l; xz/=l;
  const yx=zy*xz-zz*xy, yy=zz*xx-zx*xz, yz=zx*xy-zy*xx;
  return new Float32Array([
    xx,yx,zx,0, xy,yy,zy,0, xz,yz,zz,0,
    -(xx*eye[0]+xy*eye[1]+xz*eye[2]),
    -(yx*eye[0]+yy*eye[1]+yz*eye[2]),
    -(zx*eye[0]+zy*eye[1]+zz*eye[2]),1
  ]);
}
function hex(hex:string):[number,number,number] {
  const h=hex.replace('#','');
  return [parseInt(h.slice(0,2),16)/255,parseInt(h.slice(2,4),16)/255,parseInt(h.slice(4,6),16)/255];
}

interface Mesh {
  pos: WebGLBuffer;
  normal: WebGLBuffer;
  index: WebGLBuffer;
  uv: WebGLBuffer | null;
  texture: WebGLTexture | null;
  indexType: number;
  count: number;
  parts?: Mesh[];
}

const sphereTypes: Record<SphereType, {color:string; accent:string}> = {
  standard:{color:'#53ddff',accent:'#d8fbff'},
  sniper:{color:'#d65cff',accent:'#f5c8ff'},
  shotgun:{color:'#ff8638',accent:'#ffe0b0'},
  chain:{color:'#ffe14e',accent:'#fff8ba'},
  aura:{color:'#48e4b2',accent:'#c9ffec'},
  orbital:{color:'#8ef0ff',accent:'#e0fbff'},
  prism:{color:'#ff8de1',accent:'#ffe0f5'},
  gravity:{color:'#a58cff',accent:'#e2d9ff'},
  pulse:{color:'#ffd35a',accent:'#fff0ae'},
  void:{color:'#c28cff',accent:'#ead8ff'},
};

const enemyColors: Record<string,string> = {
  normal:'#49d9ff', fast:'#ff5f9d', tank:'#ff9b45', boss:'#b879ff',
};

const VERTEX = `
attribute vec3 a_position;
attribute vec3 a_normal;
attribute vec2 a_uv;
uniform mat4 u_mvp;
uniform mat4 u_model;
varying vec3 v_normal;
varying vec3 v_world;
varying vec3 v_local;
varying vec2 v_uv;
void main(){
  vec4 world=u_model*vec4(a_position,1.0);
  v_world=world.xyz;
  v_local=a_position;
  v_uv=a_uv;
  v_normal=normalize(mat3(u_model)*a_normal);
  gl_Position=u_mvp*vec4(a_position,1.0);
}`;

const FRAGMENT = `
precision mediump float;
uniform vec3 u_color;
uniform vec3 u_emissive;
uniform vec3 u_light;
uniform vec3 u_camera;
uniform float u_alpha;
uniform float u_time;
uniform sampler2D u_baseColorMap;
uniform float u_hasTexture;
varying vec3 v_normal;
varying vec3 v_world;
varying vec3 v_local;
varying vec2 v_uv;
void main(){
  vec3 N=normalize(v_normal);
  vec3 L=normalize(u_light);
  float ndl=max(dot(N,L),0.0);
  vec3 V=normalize(u_camera-v_world);
  float fresnel=pow(1.0-max(dot(N,V),0.0),3.2);
  float rim=pow(1.0-max(dot(N,vec3(0.0,1.0,0.0)),0.0),2.4);
  float micro=0.5+0.5*sin(v_local.x*9.0+v_local.z*7.0+sin(v_local.y*6.0)*1.7);
  float pulse=0.88+0.12*sin(u_time*3.0+v_local.y*4.0);
  vec3 albedo=u_color;
  if(u_hasTexture>0.5){
    vec4 texel=texture2D(u_baseColorMap,v_uv);
    albedo*=texel.rgb;
  }
  vec3 base=mix(albedo,albedo*vec3(0.48,0.58,0.72),micro*0.28);
  vec3 H=normalize(L+vec3(0.35,0.78,0.45));
  float spec=pow(max(dot(N,H),0.0),42.0);
  float edge=pow(1.0-max(dot(N,V),0.0),5.5);
  float contour=0.5+0.5*sin(v_local.y*13.0+v_local.x*4.0+v_local.z*2.0);
  float cavity=1.0-smoothstep(0.05,0.62,length(v_local));
  float energyBand=0.5+0.5*sin(v_local.x*18.0+v_local.z*15.0+u_time*2.2);
  vec3 col=base*(0.10+ndl*0.90);
  col+=u_emissive*(0.22+rim*0.92+fresnel*1.72)*pulse;
  col+=u_emissive*spec*(1.75+edge*1.8);
  col+=u_emissive*micro*0.06;
  col+=u_emissive*energyBand*(0.025+fresnel*0.08);
  col+=u_emissive*cavity*0.08;
  col+=u_emissive*edge*0.75;
  col+=base*contour*0.035;
  gl_FragColor=vec4(col,u_alpha);
}`;

const GROUND_V = `
attribute vec3 a_position;
uniform mat4 u_mvp;
varying vec3 v_world;
void main(){v_world=a_position;gl_Position=u_mvp*vec4(a_position,1.0);}
`;
const GROUND_F = `
precision mediump float;
varying vec3 v_world;
uniform vec3 u_center;
void main(){
  vec2 p=v_world.xz;
  float major=1.0-smoothstep(0.0,0.035,abs(fract(p.x/80.0)-0.5));
  float minor=1.0-smoothstep(0.0,0.018,abs(fract(p.x/20.0)-0.5));
  float majorZ=1.0-smoothstep(0.0,0.035,abs(fract(p.y/80.0)-0.5));
  float minorZ=1.0-smoothstep(0.0,0.018,abs(fract(p.y/20.0)-0.5));
  float d=length(p-u_center.xz);
  float glow=1.0-smoothstep(0.0,650.0,d);
  vec3 col=vec3(0.008,0.015,0.028)+vec3(0.025,0.07,0.11)*(minor+minorZ)*0.35+vec3(0.02,0.12,0.18)*(major+majorZ)*0.55;
  col+=vec3(0.035,0.015,0.08)*glow;
  gl_FragColor=vec4(col,1.0);
}`;

export class Echo3DRenderer {
  private gl: WebGLRenderingContext;
  private program: WebGLProgram;
  private groundProgram: WebGLProgram;
  private sphereMesh: Mesh;
  private torusMesh: Mesh;
  private fineTorusMesh: Mesh;
  private cylinderMesh: Mesh;
  private reactorShellMesh: Mesh;
  private facetCoreMesh: Mesh;
  private quad: WebGLBuffer;
  private posLoc:number;
  private normalLoc:number;
  private uvLoc:number;
  private mvpLoc:WebGLUniformLocation;
  private modelLoc:WebGLUniformLocation;
  private colorLoc:WebGLUniformLocation;
  private emissiveLoc:WebGLUniformLocation;
  private alphaLoc:WebGLUniformLocation;
  private timeLoc:WebGLUniformLocation;
  private baseColorMapLoc:WebGLUniformLocation;
  private hasTextureLoc:WebGLUniformLocation;
  private lightLoc:WebGLUniformLocation;
  private cameraLoc:WebGLUniformLocation;
  private groundPosLoc:number;
  private groundMvpLoc:WebGLUniformLocation;
  private groundCenterLoc:WebGLUniformLocation;
  private width=1;
  private height=1;
  private modelAssets=new Map<string,Mesh>();
  private modelLoadStarted=false;

  // High-fidelity tower animation state. The game simulation stays untouched:
  // these timestamps are derived from the existing attack/aura timers.
  private sphereAttackTimers=new Map<SphereEntity,number>();
  private sphereAuraTimers=new Map<SphereEntity,number>();
  private sphereShotTimes=new Map<SphereEntity,number>();
  private spherePulseTimes=new Map<SphereEntity,number>();
  private readonly icosaEdges:Array<[number,number]>=[
    [0,1],[0,4],[0,5],[0,7],[0,10],
    [1,5],[1,6],[1,8],[1,11],
    [2,3],[2,4],[2,6],[2,7],[2,9],
    [3,5],[3,6],[3,10],[3,11],
    [4,7],[4,8],[4,9],
    [5,8],[5,10],[5,11],
    [6,9],[6,11],
    [7,8],[7,9],
    [8,9],
    [10,11]
  ];

  constructor(private canvas:HTMLCanvasElement){
    const gl=canvas.getContext('webgl',{alpha:false,antialias:true,powerPreference:'high-performance'});
    if(!gl) throw new Error('WebGL is not supported');
    this.gl=gl;
    this.program=this.makeProgram(VERTEX,FRAGMENT);
    this.groundProgram=this.makeProgram(GROUND_V,GROUND_F);
    this.sphereMesh=this.makeUvSphere(1,32,20);
    this.torusMesh=this.makeTorus(1,0.055,48,10);
    this.fineTorusMesh=this.makeTorus(1,0.022,64,8);
    this.cylinderMesh=this.makeCylinder(1,1,10);
    this.reactorShellMesh=this.makeLathe([
      [-1.00,0.48],[-0.88,0.68],[-0.58,0.84],[-0.22,0.91],[0.22,0.91],[0.58,0.84],[0.88,0.68],[1.00,0.48]
    ],20);
    this.facetCoreMesh=this.makeIcoSphere(1);
    void this.loadModelAssets();
    this.quad=this.makeGroundBuffer();
    this.posLoc=gl.getAttribLocation(this.program,'a_position');
    this.normalLoc=gl.getAttribLocation(this.program,'a_normal');
    this.uvLoc=gl.getAttribLocation(this.program,'a_uv');
    this.mvpLoc=this.mustUniform(this.program,'u_mvp');
    this.modelLoc=this.mustUniform(this.program,'u_model');
    this.colorLoc=this.mustUniform(this.program,'u_color');
    this.emissiveLoc=this.mustUniform(this.program,'u_emissive');
    this.alphaLoc=this.mustUniform(this.program,'u_alpha');
    this.timeLoc=this.mustUniform(this.program,'u_time');
    this.baseColorMapLoc=this.mustUniform(this.program,'u_baseColorMap');
    this.hasTextureLoc=this.mustUniform(this.program,'u_hasTexture');
    this.lightLoc=this.mustUniform(this.program,'u_light');
    this.cameraLoc=this.mustUniform(this.program,'u_camera');
    this.groundPosLoc=gl.getAttribLocation(this.groundProgram,'a_position');
    this.groundMvpLoc=this.mustUniform(this.groundProgram,'u_mvp');
    this.groundCenterLoc=this.mustUniform(this.groundProgram,'u_center');
    gl.enable(gl.DEPTH_TEST);
    gl.enable(gl.CULL_FACE);
    gl.enable(gl.BLEND);
    gl.blendFunc(gl.SRC_ALPHA,gl.ONE_MINUS_SRC_ALPHA);
  }

  resize(){
    const dpr=Math.min(window.devicePixelRatio||1,2);
    const w=Math.max(1,Math.floor(this.canvas.clientWidth*dpr));
    const h=Math.max(1,Math.floor(this.canvas.clientHeight*dpr));
    if(this.canvas.width!==w||this.canvas.height!==h){this.canvas.width=w;this.canvas.height=h;}
    this.width=w; this.height=h;
    this.gl.viewport(0,0,w,h);
  }

  render(s:GameState){
    const gl=this.gl;
    this.resize();
    const t=s.time;
    gl.clearColor(0.002,0.004,0.009,1);
    gl.clear(gl.COLOR_BUFFER_BIT|gl.DEPTH_BUFFER_BIT);
    const aspect=this.width/Math.max(1,this.height);
    const player=s.player.pos;
    const distance=Math.max(430,Math.min(760,Math.max(s.worldWidth,s.worldHeight)*0.33));
    const eye:[number,number,number]=[player.x, distance*0.72, player.y+distance*0.72];
    const view=mat4LookAt(eye,[player.x,0,player.y],[0,1,0]);
    const proj=mat4Perspective(48*DEG,aspect,1,2400);
    const vp=mat4Multiply(proj,view);

    gl.useProgram(this.groundProgram);
    gl.disable(gl.CULL_FACE);
    gl.bindBuffer(gl.ARRAY_BUFFER,this.quad);
    gl.enableVertexAttribArray(this.groundPosLoc);
    gl.vertexAttribPointer(this.groundPosLoc,3,gl.FLOAT,false,0,0);
    gl.uniformMatrix4fv(this.groundMvpLoc,false,mat4Multiply(vp,mat4Translate(0,-3,0)));
    gl.uniform3f(this.groundCenterLoc,player.x,player.y,0);
    gl.drawArrays(gl.TRIANGLE_STRIP,0,4);
    gl.enable(gl.CULL_FACE);

    gl.useProgram(this.program);
    gl.uniform3f(this.lightLoc,-0.35,0.8,0.45);
    gl.uniform3f(this.cameraLoc,eye[0],eye[1],eye[2]);
    gl.uniform1f(this.timeLoc,t);
    for(const sphere of s.spheres) if(sphere.alive) this.drawSphere(sphere,t,vp,player.x,player.y,s.player);
    for(const enemy of s.enemies) if(enemy.hp>0) this.drawEnemy(enemy,t,vp);
    this.drawPlayer(s,t,vp);
    for(const lightning of s.lightnings) if(lightning.life>0) this.drawLightning(lightning,t,vp);
    for(const p of s.sphereProjectiles) if(p.alive) this.drawProjectile(p,t,vp);
    for(const p of s.particles) if(p.life>0 && p.size>1) this.drawParticle(p,t,vp);
    for(const orb of s.xpOrbs) if(orb.alive) this.drawOrb(orb.pos.x,orb.pos.y,orb.radius,t,vp);
    for(const hp of s.healthPacks) if(hp.alive) this.drawHealth(hp.pos.x,hp.pos.y,hp.radius,t,vp);
    for(const trail of s.fireTrails) if(trail.life>0) this.drawRing(trail.pos.x,trail.pos.y,18*(trail.life/trail.maxLife),trail.life/trail.maxLife,'#ff663d',t,vp,0.45);
    for(const enemy of s.enemies) for(const bp of enemy.bossProjectiles) if(bp.alive) this.drawOrb(bp.pos.x,bp.pos.y,bp.radius*1.7,t,vp);
  }

  dispose(){ /* WebGL resources are owned by the canvas and released with its context. */ }

  private drawPlayer(s:GameState,t:number,vp:Float32Array){
    const p=s.player.pos;
    const playerAsset=this.modelAssets.get('player');
    const moving=Math.hypot(s.player.dashDir.x,s.player.dashDir.y)>0.01 || s.player.dashTimer>0;
    const dash=s.player.dashTimer>0;
    const pulse=1+Math.sin(t*3.15)*0.025;
    const bob=21+Math.sin(t*2.7)*1.15+(moving?Math.sin(t*10)*0.8:0);
    const size=25*pulse;

    // The authored GLB is the complete hero presentation. Do not wrap it in
    // the old procedural blue sphere/circular containment frame.
    if(playerAsset){
      const bodyScale=size*(dash?1.10:1.0);
      const body=mat4Multiply(
        mat4Translate(p.x,bob,p.y),
        mat4Multiply(
          mat4RotateY(t*.10),
          mat4Scale(bodyScale,bodyScale,bodyScale)
        )
      );
      const bodyGlow=s.player.mutationStage>=3?'#9a6dff':'#73eaff';
      this.drawModel(playerAsset,body,vp,'#ffffff',bodyGlow,1);
    }

    // Keep only a restrained dash cue. It is gameplay feedback, not a persistent
    // containment frame around the hero.
    if(dash){
      const dashRing=mat4Multiply(
        mat4Translate(p.x,bob,p.y),
        mat4Multiply(mat4RotateZ(t*2.4),mat4Scale(size*1.02,size*1.02,size*1.02))
      );
      this.drawModel(this.torusMesh,dashRing,vp,'#d5faff','#ffffff',.70);
    }
  }

  private drawSphere(s:SphereEntity,t:number,vp:Float32Array,cx:number,cy:number,player:GameState['player']){
    const def=sphereTypes[s.type];
    const tier=Math.max(1,Math.min(7,Math.round(s.visualTier||1)));
    const pulse=1+Math.sin(t*2.65+s.rotation*.71)*.035;
    const y=24+Math.sin(t*1.9+s.pos.x*.01)*1.8;
    const base=17+Math.min(11,tier*1.55);
    const origin:Vec3=[s.pos.x,y,s.pos.y];

    // Attack/aura edge detection drives visual events without touching gameplay.
    const prevAttack=this.sphereAttackTimers.get(s);
    const prevAura=this.sphereAuraTimers.get(s);
    const delay=Math.max(.15,s.attackDelay);
    const fired=(prevAttack===undefined&&s.attackTimer>delay*.7) ||
      (prevAttack!==undefined&&s.attackTimer>prevAttack+Math.max(.12,delay*.35));
    const auraPulse=(prevAura===undefined&&s.auraTimer>.01) ||
      (prevAura!==undefined&&s.auraTimer>prevAura+.12);
    if(fired)this.sphereShotTimes.set(s,t);
    if(auraPulse)this.spherePulseTimes.set(s,t);
    this.sphereAttackTimers.set(s,s.attackTimer);
    this.sphereAuraTimers.set(s,s.auraTimer);
    const shotFlash=Math.max(0,1-(t-(this.sphereShotTimes.get(s)??-999))/.24);
    const auraFlash=Math.max(0,1-(t-(this.spherePulseTimes.get(s)??-999))/.60);

    const rot=s.rotation+t*(s.type==='sniper'?.13:s.type==='chain'?.63:.36);
    const coreR=base*(.255+tier*.006);

    // Imported authored tower mesh. This is the hard-surface "hero" body;
    // the procedural cage below acts as its animated containment architecture.
    // Keeping the authored mesh visible at every tier makes the tower read as
    // a designed 3D asset rather than a collection of primitives.
    const authored=this.modelAssets.get('sphere_'+s.type);
    if(authored){
      const authoredScale=base*(.56+tier*.012);
      const authoredModel=mat4Multiply(
        mat4Translate(origin[0],origin[1],origin[2]),
        mat4Multiply(
          mat4RotateY(rot*.55),
          mat4Scale(authoredScale,authoredScale,authoredScale)
        )
      );
      this.drawModel(authored,authoredModel,vp,'#ffffff',def.color,.98);
      // A second, slightly expanded emissive pass gives the authored mesh
      // controlled energy around its silhouette without turning it into a blob.
      const glowScale=authoredScale*1.035;
      const glowModel=mat4Multiply(
        mat4Translate(origin[0],origin[1],origin[2]),
        mat4Multiply(mat4RotateY(-rot*.22),mat4Scale(glowScale,glowScale,glowScale))
      );
      this.drawModelAdditive(authored,glowModel,vp,def.color,def.accent,.105+.045*Math.sin(t*2.4));
    }

    // The authored tower owns its central reactor. The procedural fallback is
    // intentionally limited to a tiny signal only while an asset is loading.
    if(!authored){
      const shell=mat4Multiply(
        mat4Translate(...origin),
        mat4Scale(coreR*1.25,coreR*1.25,coreR*1.25)
      );
      this.drawModel(this.sphereMesh,shell,vp,'#020914',def.color,.92);

      const core=mat4Multiply(
        mat4Translate(origin[0],origin[1]+Math.sin(t*5.4)*coreR*.055,origin[2]),
        mat4Multiply(mat4RotateY(rot*.7),mat4Scale(coreR*pulse,coreR*.96*pulse,coreR*pulse))
      );
      this.drawModelAdditive(this.facetCoreMesh,core,vp,def.color,def.accent,.94);
    }

    // --- TYPE-SPECIFIC BODY / NON-PHYSICAL SECONDARIES -------------------
    // Orbital satellites are visual combat emitters, not physics bodies. Their
    // damage is resolved in engine.ts from the same angular positions; they are
    // deliberately absent from s.spheres/enemies collision systems.
    const drawTypeSpecificBody = () => {
      if (s.type === 'orbital') {
        const satelliteCount = Math.max(
          1,
          1 + (player.sphereMods?.multishot || 0)
            + (player.artifacts.includes('orbital_crown') ? 1 : 0)
            + (tier >= 7 && player.sphereBranches?.orbital === 'orbital_blade' ? 1 : 0),
        );
        const orbitR = base * (1.05 + tier * .035);
        const satelliteSize = base * (tier >= 7 ? .14 : .115);
        for (let i = 0; i < satelliteCount; i++) {
          const a = s.rotation + t * 1.8 + i * Math.PI * 2 / satelliteCount;
          const n:Vec3 = [
            origin[0] + Math.cos(a) * orbitR,
            origin[1] + Math.sin(a * 1.7 + t * 2.2) * base * .10,
            origin[2] + Math.sin(a) * orbitR,
          ];
          this.drawModelAdditive(
            this.facetCoreMesh,
            mat4Multiply(mat4Translate(...n), mat4Multiply(mat4RotateY(-a), mat4Scale(satelliteSize, satelliteSize, satelliteSize))),
            vp, def.color, def.accent, .98,
          );
          this.drawRing(n[0], n[2], satelliteSize * 1.55, -a * 1.7, def.accent, t, vp, .62);
          if (tier >= 5) {
            const trail:Vec3 = [
              origin[0] + Math.cos(a - .22) * orbitR,
              origin[1] + Math.sin((a - .22) * 1.7 + t * 2.2) * base * .10,
              origin[2] + Math.sin(a - .22) * orbitR,
            ];
            this.drawBeamBetween(trail, n, base * .012, def.accent, '#ffffff', .32, vp);
          }
        }
      } else if (s.type === 'gravity') {
        // Gravity is a physical field, so its center must read as a body, not
        // merely as an empty aura ring.
        this.drawModelAdditive(
          this.reactorShellMesh,
          mat4Multiply(mat4Translate(...origin), mat4Multiply(mat4RotateY(rot * .35), mat4Scale(base * .72, base * .72, base * .72))),
          vp, '#07101f', def.color, .92,
        );
        this.drawModelAdditive(
          this.facetCoreMesh,
          mat4Multiply(mat4Translate(origin[0], origin[1] - base * .03, origin[2]), mat4Scale(base * .42, base * .42, base * .42)),
          vp, def.color, def.accent, .96,
        );
        const fieldR = SPHERE_TYPES.gravity.auraRadius * .34;
        this.drawRing(origin[0], origin[2], fieldR, -rot * 1.4, def.color, t, vp, .34);
        this.drawRing(origin[0], origin[2], fieldR * .72, rot * 1.9, def.accent, t, vp, .24);
      } else if (s.type === 'prism') {
        const prismScale = base * .52;
        this.drawModelAdditive(
          this.facetCoreMesh,
          mat4Multiply(mat4Translate(...origin), mat4Multiply(mat4RotateY(rot), mat4Scale(prismScale, prismScale * 1.18, prismScale))),
          vp, def.color, def.accent, 1,
        );
        for (let i = 0; i < 3; i++) {
          const a = rot + i * Math.PI * 2 / 3;
          const beam:Vec3 = [origin[0] + Math.cos(a) * base * .95, origin[1], origin[2] + Math.sin(a) * base * .95];
          this.drawBeamBetween(origin, beam, base * .018, def.accent, '#ffffff', .52, vp);
          this.drawModelAdditive(this.facetCoreMesh, mat4Translate(...beam), vp, def.accent, '#ffffff', .72);
        }
      } else if (s.type === 'pulse') {
        const pulseR = base * (.56 + .05 * Math.sin(t * 4.5));
        this.drawModelAdditive(
          this.facetCoreMesh,
          mat4Multiply(mat4Translate(...origin), mat4Multiply(mat4RotateY(rot), mat4Scale(pulseR, pulseR, pulseR))),
          vp, def.color, def.accent, 1,
        );
        this.drawRing(origin[0], origin[2], base * (1.0 + .12 * Math.sin(t * 5.0)), rot * 1.8, def.accent, t, vp, .72);
        this.drawRing(origin[0], origin[2], base * (1.35 + .18 * Math.sin(t * 5.0 + 1)), -rot * 1.25, def.color, t, vp, .42);
      } else if (s.type === 'void') {
        const voidR = base * (.60 + .08 * Math.sin(t * 2.6));
        this.drawModel(
          this.sphereMesh,
          mat4Multiply(mat4Translate(...origin), mat4Scale(voidR, voidR, voidR)),
          vp, '#01030a', def.color, .98,
        );
        this.drawModelAdditive(
          this.facetCoreMesh,
          mat4Multiply(mat4Translate(origin[0], origin[1], origin[2]), mat4Scale(voidR * .56, voidR * .56, voidR * .56)),
          vp, '#05010b', def.accent, .94,
        );
        this.drawRing(origin[0], origin[2], base * 1.05, -rot * 1.4, def.color, t, vp, .72);
        this.drawRing(origin[0], origin[2], base * 1.38, rot * .72, def.accent, t, vp, .34);
      }
    };
    drawTypeSpecificBody();

    // --- REFERENCE-FAITHFUL SPHERICAL FRAME -------------------------------
    // The reference is built around great-circle lines and articulated vertices.
    // We reproduce that as actual 3D struts, not flat decals or sprites.
    const cageR=base*(.73+tier*.045);
    const phi=(1+Math.sqrt(5))/2;
    const raw:Vec3[]=[
      [-1, phi,0],[1,phi,0],[-1,-phi,0],[1,-phi,0],
      [0,-1,phi],[0,1,phi],[0,-1,-phi],[0,1,-phi],
      [phi,0,-1],[phi,0,1],[-phi,0,-1],[-phi,0,1]
    ];
    const verts=raw.map(v=>{
      const l=Math.hypot(v[0],v[1],v[2])||1;
      return [v[0]/l*cageR,v[1]/l*cageR,v[2]/l*cageR] as Vec3;
    });

    const drawCage=(scale:number,spin:number,alpha:number,thickness:number)=>{
      for(const [a,b] of this.icosaEdges){
        const va=verts[a],vb=verts[b];
        const c=Math.cos(spin),sn=Math.sin(spin);
        const A:[number,number,number]=[
          origin[0]+(va[0]*c-va[2]*sn)*scale,
          origin[1]+va[1]*scale,
          origin[2]+(va[0]*sn+va[2]*c)*scale
        ];
        const B:[number,number,number]=[
          origin[0]+(vb[0]*c-vb[2]*sn)*scale,
          origin[1]+vb[1]*scale,
          origin[2]+(vb[0]*sn+vb[2]*c)*scale
        ];
        this.drawBeamBetween(A,B,base*thickness*.72,def.color,def.accent,alpha*.82,vp);
      }
    };

    // I is intentionally sparse. Each subsequent tier adds another physical
    // layer, matching the reference's I -> VII silhouette progression.
    if(tier>=1)this.drawModel(
      this.fineTorusMesh,
      mat4Multiply(mat4Translate(...origin),mat4Multiply(mat4RotateY(rot),mat4Scale(cageR,cageR,cageR))),
      vp,def.accent,'#ffffff',.80
    );
    if(tier>=2){
      this.drawModel(
        this.fineTorusMesh,
        mat4Multiply(mat4Translate(...origin),mat4Multiply(mat4RotateX(Math.PI/2),mat4Scale(cageR,cageR,cageR))),
        vp,def.accent,'#ffffff',.76
      );
    }
    if(tier>=3){
      drawCage(1,rot*.38,.64,.0105);
      this.drawModel(
        this.fineTorusMesh,
        mat4Multiply(mat4Translate(...origin),mat4Multiply(mat4RotateZ(Math.PI/2),mat4Scale(cageR,cageR,cageR))),
        vp,def.accent,'#ffffff',.62
      );
    }
    if(tier>=4){
      drawCage(1,-rot*.27,.47,.0075);
      const crossR=cageR*1.13;
      this.drawModel(
        this.fineTorusMesh,
        mat4Multiply(mat4Translate(...origin),mat4Multiply(mat4Multiply(mat4RotateX(58*DEG),mat4RotateY(rot*.41)),mat4Scale(crossR,crossR,crossR))),
        vp,def.accent,'#ffffff',.62
      );
    }
    if(tier>=5){
      drawCage(.86,rot*.52,.42,.0062);
      for(const tilt of [-27,27]){
        const rr=mat4Multiply(
          mat4Translate(...origin),
          mat4Multiply(mat4RotateX(tilt*DEG),mat4RotateY(rot*(tilt>0?.31:-.28)))
        );
        this.drawModel(this.torusMesh,mat4Multiply(rr,mat4Scale(cageR*1.02,cageR*.76,cageR*1.02)),vp,def.accent,'#ffffff',.55);
      }
    }
    if(tier>=6){
      drawCage(.72,-rot*.63,.36,.0052);
      const fineR=cageR*1.23;
      for(const tilt of [-44,44]){
        const rr=mat4Multiply(
          mat4Translate(...origin),
          mat4Multiply(mat4RotateZ(tilt*DEG),mat4RotateY(rot*.22))
        );
        this.drawModel(this.fineTorusMesh,mat4Multiply(rr,mat4Scale(fineR,fineR*.78,fineR)),vp,def.accent,'#ffffff',.48);
      }
    }
    if(tier>=7){
      // Final evolution: three nested cages at different scales create the same
      // dense "contained star" impression as the large VII reference.
      drawCage(1.24,rot*.23,.66,.009);
      drawCage(.91,-rot*.49,.48,.006);
      const outerR=cageR*1.34;
      const outer=[
        mat4RotateY(rot*.17),
        mat4Multiply(mat4RotateX(63*DEG),mat4RotateY(-rot*.21)),
        mat4Multiply(mat4RotateZ(63*DEG),mat4RotateY(rot*.15))
      ];
      for(const r of outer){
        this.drawModel(
          this.fineTorusMesh,
          mat4Multiply(mat4Translate(...origin),mat4Multiply(r,mat4Scale(outerR,outerR,outerR))),
          vp,def.accent,'#ffffff',.72
        );
      }
      // Radial energy spokes connect the inner reactor to the outer shell.
      for(let i=0;i<12;i++){
        const a=i*Math.PI/6+rot*.13;
        const q:Vec3=[origin[0]+Math.cos(a)*outerR*.98,origin[1]+Math.sin(a*2)*outerR*.20,origin[2]+Math.sin(a)*outerR*.98];
        this.drawBeamBetween(origin,q,base*.010,def.color,'#ffffff',.50,vp);
      }
    }

    // Articulated vertex caps make the cage read as manufactured geometry.
    const nodeScale=tier>=7?1.12:tier>=5?1.04:1;
    const nodeSize=base*(tier>=6?.054:tier>=3?.050:.046)*nodeScale;
    const nodeCount=tier>=7?12:tier>=5?12:tier>=3?12:0;
    for(let i=0;i<nodeCount;i++){
      const v=verts[i];
      const c=Math.cos(rot*.38),sn=Math.sin(rot*.38);
      const n:[number,number,number]=[
        origin[0]+(v[0]*c-v[2]*sn),
        origin[1]+v[1],
        origin[2]+(v[0]*sn+v[2]*c)
      ];
      const nm=mat4Multiply(
        mat4Translate(...n),
        mat4Scale(nodeSize*(i%3===0?1.24:1),nodeSize,nodeSize*(i%2===0?1.12:1))
      );
      this.drawModelAdditive(this.facetCoreMesh,nm,vp,def.accent,'#ffffff',.96);
      if(tier>=6&&i%2===0){
        this.drawModelAdditive(
          this.sphereMesh,
          mat4Multiply(mat4Translate(...n),mat4Scale(nodeSize*.48,nodeSize*.48,nodeSize*.48)),
          vp,def.color,'#ffffff',.72
        );
      }
    }

    // --- ARTICULATED SECONDARY ORBITS -----------------------------------
    // Faceted "petals" are the small structural members visible between the
    // reference's great-circle lines. They give the cage a designed, fabricated
    // silhouette instead of a purely mathematical wireframe.
    if(tier>=3){
      const petals=tier>=7?12:tier>=5?10:8;
      const petalR=cageR*(tier>=7?1.02:.92);
      for(let i=0;i<petals;i++){
        const a=i/petals*Math.PI*2+rot*.18;
        const p:Vec3=[
          origin[0]+Math.cos(a)*petalR,
          origin[1]+Math.sin(a*2.0)*petalR*.12,
          origin[2]+Math.sin(a)*petalR
        ];
        const pm=mat4Multiply(
          mat4Translate(...p),
          mat4Multiply(
            mat4RotateY(-a),
            mat4Multiply(mat4RotateZ(Math.sin(a*3.0+t*.7)*.12),mat4Scale(
              base*(tier>=7?.115:.085),
              base*(tier>=7?.032:.026),
              base*(tier>=7?.032:.026)
            ))
          )
        );
        this.drawModelAdditive(
          this.facetCoreMesh,pm,vp,def.accent,'#ffffff',
          tier>=7?.48:tier>=5?.37:.30
        );
      }
    }


    // Small mechanical halos sit between the main cage and the authored body.
    // Their phase offsets prevent the object from looking mathematically static.
    if(tier>=3){
      const orbitCount=tier>=7?6:tier>=5?4:2;
      for(let i=0;i<orbitCount;i++){
        const a=(i/orbitCount)*Math.PI*2+rot*(i%2===0?.37:-.29);
        const rr=cageR*(.78+(i%3)*.045);
        const center:Vec3=[
          origin[0]+Math.cos(a)*rr*.22,
          origin[1]+Math.sin(a*2.0+t*.34)*rr*.13,
          origin[2]+Math.sin(a)*rr*.22
        ];
        const orbit=mat4Multiply(
          mat4Translate(...center),
          mat4Multiply(
            mat4RotateX((i*31-38)*DEG),
            mat4Multiply(mat4RotateY(a),mat4Scale(rr*.43,rr*.43,rr*.43))
          )
        );
        this.drawModel(
          this.fineTorusMesh,orbit,vp,def.accent,'#ffffff',
          tier>=7?.38:tier>=5?.31:.25
        );
      }
    }

    // --- TYPE-SPECIFIC WEAPON ARCHITECTURE -------------------------------
    const dirX=Math.cos(s.rotation),dirZ=Math.sin(s.rotation);
    const sideX=-dirZ,sideZ=dirX;
    const forward=(d:number,side:number,yo:number):Vec3=>[
      origin[0]+dirX*d+sideX*side,origin[1]+yo,origin[2]+dirZ*d+sideZ*side
    ];

    if(s.type==='sniper'){
      // Long precision barrel with a focusing lens.
      const mount=forward(base*.36,0,0), muzzle=forward(base*1.04,0,0);
      this.drawBeamBetween(mount,muzzle,base*.034,def.color,def.accent,.72,vp);
      this.drawBeamBetween(muzzle,forward(base*1.16,0,0),base*.012,def.accent,'#ffffff',.55,vp);
      this.drawModelAdditive(this.facetCoreMesh,
        mat4Multiply(mat4Translate(...muzzle),mat4Scale(base*.105,base*.105,base*.105)),
        vp,def.accent,'#ffffff',1);
      if(shotFlash>0){
        const blast=forward(base*(1.14+shotFlash*.30),0,0);
        this.drawBeamBetween(muzzle,blast,base*(.055+.07*shotFlash),def.accent,'#ffffff',.8*shotFlash,vp);
        this.drawModelAdditive(this.sphereMesh,
          mat4Multiply(mat4Translate(...blast),mat4Scale(base*(.12+.15*shotFlash),base*(.12+.15*shotFlash),base*(.12+.15*shotFlash))),
          vp,def.accent,'#ffffff',.95);
      }
    }else if(s.type==='shotgun'){
      // Three physical emitters, like a compact energy shotgun.
      for(let i=-1;i<=1;i++){
        const a=s.rotation+i*.13;
        const d:Vec3=[Math.cos(a),0,Math.sin(a)];
        const muzzle:Vec3=[origin[0]+d[0]*base*.93,origin[1]+i*base*.075,origin[2]+d[2]*base*.93];
        this.drawBeamBetween(origin,muzzle,base*.020,def.color,def.accent,.58,vp);
        this.drawModelAdditive(this.facetCoreMesh,
          mat4Multiply(mat4Translate(...muzzle),mat4Scale(base*.06,base*.06,base*.06)),
          vp,def.accent,'#ffffff',1);
        if(shotFlash>0){
          const blast:Vec3=[muzzle[0]+d[0]*base*(.20+.26*shotFlash),muzzle[1],muzzle[2]+d[2]*base*(.20+.26*shotFlash)];
          this.drawBeamBetween(muzzle,blast,base*(.026+.045*shotFlash),def.accent,'#ffffff',.65*shotFlash,vp);
        }
      }
    }else if(s.type==='chain'){
      // The yellow reference sphere becomes a segmented kinetic lattice.
      const count=tier>=5?12:8;
      const pts:Vec3[]=[];
      for(let i=0;i<count;i++){
        const a=i/count*Math.PI*2-rot*.82;
        const p:Vec3=[origin[0]+Math.cos(a)*base*.78,origin[1]+Math.sin(a*2+t*.45)*base*.28,origin[2]+Math.sin(a)*base*.78];
        pts.push(p);
        this.drawModelAdditive(this.facetCoreMesh,
          mat4Multiply(mat4Translate(...p),mat4Scale(base*.045,base*.045,base*.045)),
          vp,def.color,'#ffffff',.95);
        if(i) this.drawBeamBetween(pts[i-1],p,base*.006,def.accent,'#ffffff',.40,vp);
      }
      this.drawBeamBetween(pts[pts.length-1],pts[0],base*.006,def.accent,'#ffffff',.40,vp);
    }else if(s.type==='aura'){
      // Aura keeps the same physical cage but adds a layered field volume.
      const field=base*(1.58+.19*tier);
      this.drawRing(s.pos.x,s.pos.y,field,t*.31,def.color,t,vp,.13+.035*auraFlash);
      this.drawRing(s.pos.x,s.pos.y,field*1.25,-t*.24,def.accent,t,vp,.08+.025*auraFlash);
      this.drawRing(s.pos.x,s.pos.y,field*1.54,t*.17,def.color,t,vp,.045+.018*auraFlash);
      if(auraFlash>0){
        this.drawModelAdditive(this.sphereMesh,
          mat4Multiply(mat4Translate(...origin),mat4Scale(base*(.36+.24*auraFlash),base*(.36+.24*auraFlash),base*(.36+.24*auraFlash))),
          vp,def.color,'#ffffff',.16*auraFlash);
      }
    }else{
      const rail=forward(base*.34,0,0),muzzle=forward(base*.90,0,0);
      this.drawBeamBetween(rail,muzzle,base*.015,def.color,def.accent,.38,vp);
      this.drawModelAdditive(this.facetCoreMesh,
        mat4Multiply(mat4Translate(...muzzle),mat4Scale(base*.052,base*.052,base*.052)),
        vp,def.accent,'#ffffff',.9);
      if(shotFlash>0){
        const blast=forward(base*(.93+.28*shotFlash),0,0);
        this.drawBeamBetween(muzzle,blast,base*(.028+.045*shotFlash),def.accent,'#ffffff',.65*shotFlash,vp);
      }
    }

    // A controlled breathing halo, plus an impact/shot energy bloom.
    this.drawRing(s.pos.x,s.pos.y,base*(1.12+tier*.05),rot,def.accent,t,vp,.15+.07*Math.sin(t*2.0+s.rotation));
    if(tier>=4)this.drawRing(s.pos.x,s.pos.y,base*(1.38+tier*.065),-rot*.71,def.color,t,vp,.075+.11*shotFlash);
    if(shotFlash>0){
      this.drawModelAdditive(
        this.sphereMesh,
        mat4Multiply(mat4Translate(...origin),mat4Scale(base*(.40+.48*shotFlash),base*(.40+.48*shotFlash),base*(.40+.48*shotFlash))),
        vp,def.accent,'#ffffff',.09+.16*shotFlash
      );
    }
    void cx;void cy;
  }

  private drawEnemy(e:EnemyEntity,t:number,vp:Float32Array){
    const c=e.isBoss?enemyColors.boss:(enemyColors[e.type]||e.color||enemyColors.normal);
    const scale=e.isBoss?Math.max(34,e.radius*1.7):Math.max(13,e.radius*.95);
    const hit=e.hitFlash>0?1.7:1;
    const y=scale*.62+Math.sin(t*2.2+e.rotation)*1.5;
    const facing=e.isCharging ? Math.atan2(e.chargeDir.x,e.chargeDir.y) : 0;
    const key=e.isBoss
      ? (e.bossType==='charger'?'boss_charger':e.bossType==='shooter'?'boss_shooter':e.bossType==='summoner'?'boss_summoner':'boss_aura')
      : e.type==='fast'?'enemy_fast'
        : e.type==='tank'?'enemy_tank'
        : 'enemy_normal';
    const asset=this.modelAssets.get(key);
    if(asset){
      const m=mat4Multiply(
        mat4Multiply(mat4Translate(e.pos.x,y,e.pos.y),mat4RotateY(facing)),
        mat4Scale(scale*1.18*hit,scale*1.18*hit,scale*1.18*hit)
      );
      this.drawModel(asset,m,vp,'#ffffff',c,1);
    }else{
      // Missing authored assets are represented only by an unobtrusive signal ring.
      this.drawRing(e.pos.x,e.pos.y,scale*(e.isBoss?1.35:1.12),t*.22,c,t,vp,e.isBoss?.24:.14);
    }
    const effectRot=t*(e.isBoss?.16:.28);
    this.drawRing(e.pos.x,e.pos.y,scale*(e.isBoss?1.35:1.15),effectRot,c,t,vp,e.isBoss?.42:.22);
    if(e.isElite)this.drawRing(e.pos.x,e.pos.y,scale*1.5,-t*1.3,'#ffe36d',t,vp,.45);
    if(e.freezeTimer>0)this.drawRing(e.pos.x,e.pos.y,scale*1.65,t*.9,'#8fdcff',t,vp,.42);
    if(e.fireTimer>0)this.drawRing(e.pos.x,e.pos.y,scale*1.55,-t*1.1,'#ff643d',t,vp,.3);
  }

  private drawProjectile(p:SphereProjectile,t:number,vp:Float32Array){
    const len=Math.max(7,Math.hypot(p.vel.x,p.vel.y)*.045);
    const a=Math.atan2(p.vel.y,p.vel.x);
    const asset=this.modelAssets.get('projectile');
    if(asset){
      const m=mat4Multiply(
        mat4Multiply(mat4Translate(p.pos.x,11,p.pos.y),mat4RotateY(-a)),
        mat4Scale(Math.max(4,p.radius*1.45),Math.max(4,p.radius*1.45),Math.max(6,len*1.1))
      );
      this.drawModel(asset,m,vp,'#ffffff','#ffffff',1);
    }else{
      const m=mat4Multiply(
        mat4Multiply(mat4Translate(p.pos.x,11,p.pos.y),mat4RotateY(-a)),
        mat4Scale(Math.max(4,p.radius*1.5),Math.max(4,p.radius*1.5),len)
      );
      this.drawModel(this.sphereMesh,m,vp,p.color,'#ffffff',1);
    }

    // Short volumetric trail. It keeps fast projectiles readable on small Android
    // displays without turning them into flat sprites.
    const speed=Math.hypot(p.vel.x,p.vel.y)||1;
    const tx=p.vel.x/speed, tz=p.vel.y/speed;
    const trailLen=Math.min(32,Math.max(10,len*1.35));
    this.drawBeamBetween(
      [p.pos.x-tx*trailLen,11,p.pos.y-tz*trailLen],
      [p.pos.x,11,p.pos.y],
      Math.max(1.3,p.radius*.34),
      p.color,'#ffffff',.42,vp
    );
    this.drawRing(p.pos.x,p.pos.y,Math.max(7,p.radius*2.2),t*4,p.color,t,vp,.18);
  }

  private drawParticle(p:Particle,t:number,vp:Float32Array){
    const a=Math.max(0,p.life/p.maxLife);
    const model=mat4Multiply(mat4Translate(p.pos.x,Math.max(2,p.size*0.45),p.pos.y),mat4Scale(Math.max(1,p.size*a*0.7),Math.max(1,p.size*a*0.7),Math.max(1,p.size*a*0.7)));
    this.drawModel(this.sphereMesh,model,vp,p.color,p.color,a);
  }

  private drawOrb(x:number,z:number,r:number,t:number,vp:Float32Array){
    const model=mat4Multiply(mat4Translate(x,8+Math.sin(t*4+x)*2,z),mat4Scale(Math.max(3,r*0.75),Math.max(3,r*0.75),Math.max(3,r*0.75)));
    this.drawModel(this.sphereMesh,model,vp,'#0a1b25','#64e6ff',1);
    this.drawRing(x,z,Math.max(6,r*1.6),t*2,'#64e6ff',t,vp,0.25);
  }

  private drawHealth(x:number,z:number,r:number,t:number,vp:Float32Array){
    const model=mat4Multiply(mat4Translate(x,8,z),mat4Scale(Math.max(5,r),Math.max(3,r*0.55),Math.max(5,r)));
    this.drawModel(this.cylinderMesh,model,vp,'#34101b','#ff6680',1);
    this.drawRing(x,z,Math.max(8,r*1.8),-t*2,'#ff6680',t,vp,0.25);
  }

  private drawBeamBetween(
    from:[number,number,number],
    to:[number,number,number],
    radius:number,
    color:string,
    emissive:string,
    alpha:number,
    vp:Float32Array
  ){
    const dx=to[0]-from[0],dy=to[1]-from[1],dz=to[2]-from[2];
    const len=Math.hypot(dx,dy,dz)||0.001;
    const mid:[number,number,number]=[
      (from[0]+to[0])*.5,
      (from[1]+to[1])*.5,
      (from[2]+to[2])*.5,
    ];
    const yaw=Math.atan2(dx,dz);
    const pitch=Math.atan2(Math.hypot(dx,dz),dy);
    const orient=mat4Multiply(mat4RotateY(yaw),mat4RotateX(pitch));
    const model=mat4Multiply(
      mat4Translate(mid[0],mid[1],mid[2]),
      mat4Multiply(orient,mat4Scale(radius,len,radius))
    );
    this.drawModelAdditive(this.cylinderMesh,model,vp,color,emissive,alpha);
  }

  private drawLightning(lightning:{from:{x:number;y:number};to:{x:number;y:number};life:number},t:number,vp:Float32Array){
    const life=Math.max(0,Math.min(1,lightning.life/.3));
    const ax=lightning.from.x, az=lightning.from.y;
    const bx=lightning.to.x, bz=lightning.to.y;
    const dx=bx-ax,dz=bz-az;
    const length=Math.hypot(dx,dz)||1;
    const nx=-dz/length,nz=dx/length;
    const segments=5;
    let px=ax, pz=az;
    for(let i=1;i<=segments;i++){
      const f=i/segments;
      const amp=(1-f*.55)*Math.min(20,length*.07);
      const wobble=Math.sin(t*95+i*8.73+ax*.013+bz*.017);
      const j=(i===segments?0:wobble*amp);
      const qx= i===segments ? bx : ax+dx*f+nx*j;
      const qz= i===segments ? bz : az+dz*f+nz*j;
      const from:[number,number,number]=[px,12,pz];
      const to:[number,number,number]=[qx,12,qz];
      this.drawBeamBetween(from,to,2.2+life*1.8,'#66ddff','#ffffff',.30+life*.42,vp);
      px=qx;pz=qz;
    }
    this.drawRing((ax+bx)*.5,(az+bz)*.5,8+length*.025,t*3.5,'#66ddff',t,vp,.09+life*.16);
  }

  private drawModelAdditive(mesh:Mesh,model:Float32Array,vp:Float32Array,color:string,emissive:string,alpha:number){
    const gl=this.gl;
    gl.depthMask(false);
    gl.blendFunc(gl.SRC_ALPHA,gl.ONE);
    this.drawModel(mesh,model,vp,color,emissive,Math.max(0,Math.min(1,alpha)));
    gl.blendFunc(gl.SRC_ALPHA,gl.ONE_MINUS_SRC_ALPHA);
    gl.depthMask(true);
  }

  private drawRing(x:number,z:number,r:number,rot:number,color:string,t:number,vp:Float32Array,alpha:number){
    const m=mat4Multiply(mat4Translate(x,7,z),mat4Multiply(mat4RotateY(rot),mat4Scale(r,r,r)));
    this.drawModel(this.torusMesh,m,vp,color,color,alpha);
    void t;
  }

  private textureCache=new Map<string,WebGLTexture>();

  private async loadModelAssets(): Promise<void>{
    if(this.modelLoadStarted)return;
    this.modelLoadStarted=true;
    const paths:Record<string,string>={
      player:'/art3d/player.glb',
      sphere_standard:'/art3d/sphere-standard.glb',
      sphere_sniper:'/art3d/sphere-sniper.glb',
      sphere_shotgun:'/art3d/sphere-shotgun.glb',
      sphere_chain:'/art3d/sphere-chain.glb',
      sphere_aura:'/art3d/sphere-aura.glb',
      enemy_normal:'/art3d/enemy-normal.glb',
      enemy_fast:'/art3d/enemy-fast.glb',
      enemy_tank:'/art3d/enemy-tank.glb',
      boss_shooter:'/art3d/boss-shooter.glb',
      boss_charger:'/art3d/boss-charger.glb',
      boss_summoner:'/art3d/boss-summoner.glb',
      boss_aura:'/art3d/boss-aura.glb',
      projectile:'/art3d/projectile.glb',
    };
    await Promise.all(Object.entries(paths).map(async ([key,path])=>{
      try{
        const r=await fetch(path,{cache:'force-cache'});
        if(!r.ok)throw new Error('HTTP '+r.status);
        const mesh=await this.loadGlbMesh(await r.arrayBuffer());
        if(mesh)this.modelAssets.set(key,mesh);
      }catch(error){
        console.warn('Echo3D asset failed:',key,error);
      }
    }));
  }

  private async loadGlbMesh(buffer:ArrayBuffer):Promise<Mesh|null>{
    if(buffer.byteLength<20)return null;
    const dv=new DataView(buffer);
    if(dv.getUint32(0,true)!==0x46546c67||dv.getUint32(4,true)!==2)return null;

    let off=12;
    let json:any=null;
    let bin:Uint8Array|null=null;
    while(off+8<=buffer.byteLength){
      const len=dv.getUint32(off,true);
      const type=dv.getUint32(off+4,true);
      const start=off+8;
      if(start+len>buffer.byteLength)break;
      if(type===0x4e4f534a)json=JSON.parse(new TextDecoder().decode(new Uint8Array(buffer,start,len)));
      else if(type===0x004e4942)bin=new Uint8Array(buffer,start,len);
      off=start+len;
    }
    if(!json||!bin||!json.meshes?.length)return null;

    const accessorRead=(idx:number):Float32Array|Uint16Array|Uint32Array|null=>{
      const a=json.accessors?.[idx];
      const view=a?json.bufferViews?.[a.bufferView]:null;
      if(!a||!view)return null;
      const cc=a.type==='VEC4'?4:a.type==='VEC3'?3:a.type==='VEC2'?2:a.type==='SCALAR'?1:0;
      if(!cc)return null;
      const componentSize=a.componentType===5120||a.componentType===5121?1:a.componentType===5122||a.componentType===5123?2:a.componentType===5125||a.componentType===5126?4:0;
      if(!componentSize)return null;
      const stride=view.byteStride||componentSize*cc;
      const base=(view.byteOffset||0)+(a.byteOffset||0);
      if(a.count<=0||base<0||base+stride*(a.count-1)+componentSize*cc>bin.length)return null;
      const dvBin=new DataView(bin.buffer,bin.byteOffset,bin.byteLength);

      if(a.componentType===5126){
        const out=new Float32Array(a.count*cc);
        for(let i=0;i<a.count;i++)for(let j=0;j<cc;j++)out[i*cc+j]=dvBin.getFloat32(base+i*stride+j*4,true);
        return out;
      }
      if(a.componentType===5123){
        const out=new Uint16Array(a.count*cc);
        for(let i=0;i<a.count;i++)for(let j=0;j<cc;j++)out[i*cc+j]=dvBin.getUint16(base+i*stride+j*2,true);
        return out;
      }
      if(a.componentType===5125){
        const out=new Uint32Array(a.count*cc);
        for(let i=0;i<a.count;i++)for(let j=0;j<cc;j++)out[i*cc+j]=dvBin.getUint32(base+i*stride+j*4,true);
        return out;
      }
      return null;
    };

    const meshDefs:any[] = json.meshes[0]?.primitives ?? [];
    const parts:Mesh[]=[];
    for(let primitiveIndex=0;primitiveIndex<meshDefs.length;primitiveIndex++){
      const prim=meshDefs[primitiveIndex];
      if(!prim?.attributes?.POSITION || (prim.mode!==undefined&&prim.mode!==4))continue;

      const pos=accessorRead(prim.attributes.POSITION) as Float32Array|null;
      const norm=prim.attributes.NORMAL!==undefined
        ? accessorRead(prim.attributes.NORMAL) as Float32Array|null
        : null;
      const uv=prim.attributes.TEXCOORD_0!==undefined
        ? accessorRead(prim.attributes.TEXCOORD_0) as Float32Array|null
        : null;
      if(!(pos instanceof Float32Array))continue;

      let idx=prim.indices!==undefined ? accessorRead(prim.indices) : null;
      if(!(idx instanceof Uint16Array)&&!(idx instanceof Uint32Array)){
        idx=new Uint32Array(pos.length/3);
        for(let i=0;i<idx.length;i++)idx[i]=i;
      }

      const n=norm instanceof Float32Array?norm:new Float32Array(pos.length);
      if(!norm)for(let i=0;i<n.length;i+=3){n[i]=0;n[i+1]=1;n[i+2]=0;}

      let texture:WebGLTexture|null=null;
      const mat=json.materials?.[prim.material??0];
      const texInfo=mat?.pbrMetallicRoughness?.baseColorTexture;
      const texDef=texInfo?json.textures?.[texInfo.index]:null;
      const imageDef=texDef?json.images?.[texDef.source]:null;

      if(imageDef?.bufferView!==undefined){
        const iv=json.bufferViews?.[imageDef.bufferView];
        if(iv){
          const start=(iv.byteOffset||0), end=start+(iv.byteLength||0);
          const key=(imageDef.name||'embedded')+':'+imageDef.bufferView;
          const cached=this.textureCache.get(key);
          if(cached)texture=cached;
          else if(end<=bin.length){
            texture=await this.createTextureFromBytes(
              bin.subarray(start,end),
              imageDef.mimeType||'image/png'
            );
            if(texture)this.textureCache.set(key,texture);
          }
        }
      }else if(typeof imageDef?.uri==='string' && imageDef.uri.startsWith('data:')){
        const comma=imageDef.uri.indexOf(',');
        if(comma>0){
          const head=imageDef.uri.slice(0,comma);
          const data=imageDef.uri.slice(comma+1);
          try{
            const bytes=head.includes(';base64')
              ? Uint8Array.from(atob(data),ch=>ch.charCodeAt(0))
              : new TextEncoder().encode(decodeURIComponent(data));
            const key=imageDef.name||imageDef.uri;
            const cached=this.textureCache.get(key);
            if(cached)texture=cached;
            else{
              texture=await this.createTextureFromBytes(
                bytes,
                head.slice(5).split(';')[0]||'image/png'
              );
              if(texture)this.textureCache.set(key,texture);
            }
          }catch(error){
            console.warn('Echo3D data URI texture decode failed:',error);
          }
        }
      }

      parts.push(this.makeMeshFromTypedArrays(pos,n,idx,uv,texture));
    }

    if(parts.length===0)return null;
    if(parts.length===1)return parts[0];

    const first=parts[0];
    // Wrapper mesh contains no geometry itself. drawModel() fans out over every
    // primitive so a multi-material GLB keeps all authored surfaces.
    return {
      pos:first.pos,
      normal:first.normal,
      index:first.index,
      uv:null,
      texture:null,
      indexType:this.gl.UNSIGNED_SHORT,
      count:0,
      parts,
    };
  }

  private async createTextureFromBytes(bytes:Uint8Array,mime:string):Promise<WebGLTexture|null>{
    const gl=this.gl;
    const tex=gl.createTexture();
    if(!tex)return null;

    const blob=new Blob([bytes],{type:mime});
    let source:ImageBitmap|HTMLImageElement|null=null;
    let objectUrl:string|null=null;
    try{
      if(typeof createImageBitmap==='function'){
        source=await createImageBitmap(blob);
      }else{
        objectUrl=URL.createObjectURL(blob);
        const img=new Image();
        img.src=objectUrl;
        await img.decode();
        source=img;
      }
      gl.bindTexture(gl.TEXTURE_2D,tex);
      gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL,1);
      gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_WRAP_S,gl.CLAMP_TO_EDGE);
      gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_WRAP_T,gl.CLAMP_TO_EDGE);
      gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_MIN_FILTER,gl.LINEAR);
      gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_MAG_FILTER,gl.LINEAR);
      gl.texImage2D(gl.TEXTURE_2D,0,gl.RGBA,gl.RGBA,gl.UNSIGNED_BYTE,source);
      gl.bindTexture(gl.TEXTURE_2D,null);
      if(source instanceof ImageBitmap)source.close();
      return tex;
    }catch(error){
      console.warn('Echo3D texture decode failed:',error);
      gl.deleteTexture(tex);
      return null;
    }finally{
      if(objectUrl)URL.revokeObjectURL(objectUrl);
    }
  }

  private makeMeshFromTypedArrays(
    pos:Float32Array,
    norm:Float32Array,
    idx:Uint16Array|Uint32Array,
    uv:Float32Array|null=null,
    texture:WebGLTexture|null=null,
  ):Mesh{
    const gl=this.gl;
    let indexData:Uint16Array|Uint32Array=idx;
    let indexType:number=gl.UNSIGNED_SHORT;
    if(idx instanceof Uint32Array){
      let max=0;
      for(let i=0;i<idx.length;i++)max=Math.max(max,idx[i]);
      if(max>65535){
        const ext=gl.getExtension('OES_element_index_uint');
        if(!ext)throw new Error('32-bit indices require OES_element_index_uint');
        indexType=gl.UNSIGNED_INT;
      }else{
        const compact=new Uint16Array(idx.length);
        compact.set(idx);
        indexData=compact;
      }
    }
    const pb=gl.createBuffer(),nb=gl.createBuffer(),ib=gl.createBuffer(),ub=uv?gl.createBuffer():null;
    if(!pb||!nb||!ib)throw new Error('buffer');
    gl.bindBuffer(gl.ARRAY_BUFFER,pb);gl.bufferData(gl.ARRAY_BUFFER,pos,gl.STATIC_DRAW);
    gl.bindBuffer(gl.ARRAY_BUFFER,nb);gl.bufferData(gl.ARRAY_BUFFER,norm,gl.STATIC_DRAW);
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER,ib);gl.bufferData(gl.ELEMENT_ARRAY_BUFFER,indexData,gl.STATIC_DRAW);
    if(ub&&uv){gl.bindBuffer(gl.ARRAY_BUFFER,ub);gl.bufferData(gl.ARRAY_BUFFER,uv,gl.STATIC_DRAW);}
    return {pos:pb,normal:nb,index:ib,uv:ub,texture,indexType,count:indexData.length};
  }

  private drawModel(mesh:Mesh,model:Float32Array,vp:Float32Array,color:string,emissive:string,alpha:number){
    const gl=this.gl;
    const mvp=mat4Multiply(vp,model);
    gl.uniformMatrix4fv(this.mvpLoc,false,mvp);
    gl.uniformMatrix4fv(this.modelLoc,false,model);
    gl.uniform1f(this.alphaLoc,alpha);

    const parts=mesh.parts??[mesh];
    for(const part of parts){
      const c=hex(color), e=hex(emissive);
      gl.uniform3f(this.colorLoc,c[0],c[1],c[2]);
      gl.uniform3f(this.emissiveLoc,e[0],e[1],e[2]);

      gl.bindBuffer(gl.ARRAY_BUFFER,part.pos);
      gl.enableVertexAttribArray(this.posLoc);
      gl.vertexAttribPointer(this.posLoc,3,gl.FLOAT,false,0,0);

      gl.bindBuffer(gl.ARRAY_BUFFER,part.normal);
      gl.enableVertexAttribArray(this.normalLoc);
      gl.vertexAttribPointer(this.normalLoc,3,gl.FLOAT,false,0,0);

      if(part.uv){
        gl.bindBuffer(gl.ARRAY_BUFFER,part.uv);
        gl.enableVertexAttribArray(this.uvLoc);
        gl.vertexAttribPointer(this.uvLoc,2,gl.FLOAT,false,0,0);
      }else{
        gl.disableVertexAttribArray(this.uvLoc);
        gl.vertexAttrib2f(this.uvLoc,0,0);
      }

      if(part.texture){
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D,part.texture);
        gl.uniform1i(this.baseColorMapLoc,0);
        gl.uniform1f(this.hasTextureLoc,1);
      }else{
        gl.uniform1f(this.hasTextureLoc,0);
      }

      gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER,part.index);
      gl.drawElements(gl.TRIANGLES,part.count,part.indexType,0);
    }
  }

  private makeIcoSphere(r:number):Mesh{
    const phi=(1+Math.sqrt(5))/2;
    const raw=[
      [-1,phi,0],[1,phi,0],[-1,-phi,0],[1,-phi,0],
      [0,-1,phi],[0,1,phi],[0,-1,-phi],[0,1,-phi],
      [phi,0,-1],[phi,0,1],[-phi,0,-1],[-phi,0,1]
    ];
    const faces=[
      [0,11,5],[0,5,1],[0,1,7],[0,7,10],[0,10,11],
      [1,5,9],[5,11,4],[11,10,2],[10,7,6],[7,1,8],
      [3,9,4],[3,4,2],[3,2,6],[3,6,8],[3,8,9],
      [4,9,5],[2,4,11],[6,2,10],[8,6,7],[9,8,1]
    ];
    const positions:number[]=[],normals:number[]=[],indices:number[]=[];
    for(const v of raw){
      const l=Math.hypot(v[0],v[1],v[2])||1;
      positions.push(v[0]/l*r,v[1]/l*r,v[2]/l*r);
      normals.push(v[0]/l,v[1]/l,v[2]/l);
    }
    for(const f of faces) indices.push(f[0],f[1],f[2]);
    return this.makeMesh(positions,normals,indices);
  }
  private makeLathe(profile:Array<[number,number]>,segments:number):Mesh{
    const p:number[]=[],n:number[]=[],idx:number[]=[];
    for(let i=0;i<profile.length;i++){
      const [y,rad]=profile[i];
      const prev=profile[Math.max(0,i-1)],next=profile[Math.min(profile.length-1,i+1)];
      const slope=(next[1]-prev[1])/(next[0]-prev[0]||1);
      for(let j=0;j<=segments;j++){
        const a=j/segments*Math.PI*2,c=Math.cos(a),s=Math.sin(a);
        p.push(rad*c,y,rad*s);
        const nx=c,ny=-slope,nz=s,l=Math.hypot(nx,ny,nz)||1;
        n.push(nx/l,ny/l,nz/l);
      }
    }
    for(let i=0;i<profile.length-1;i++) for(let j=0;j<segments;j++){
      const a=i*(segments+1)+j,b=a+segments+1;
      idx.push(a,b,a+1,b,b+1,a+1);
    }
    return this.makeMesh(p,n,idx);
  }
  private makeUvSphere(r:number,segments:number,rings:number):Mesh{
    const positions:number[]=[], normals:number[]=[], indices:number[]=[];
    for(let y=0;y<=rings;y++){
      const v=y/rings, phi=v*Math.PI;
      for(let x=0;x<=segments;x++){
        const u=x/segments, th=u*Math.PI*2;
        const sx=Math.sin(phi)*Math.cos(th), sy=Math.cos(phi), sz=Math.sin(phi)*Math.sin(th);
        positions.push(sx*r,sy*r,sz*r); normals.push(sx,sy,sz);
      }
    }
    for(let y=0;y<rings;y++) for(let x=0;x<segments;x++){
      const a=y*(segments+1)+x,b=a+segments+1;
      indices.push(a,b,a+1,b,b+1,a+1);
    }
    return this.makeMesh(positions,normals,indices);
  }
  private makeTorus(R:number,r:number,segments:number,tubes:number):Mesh{
    const p:number[]=[],n:number[]=[],idx:number[]=[];
    for(let i=0;i<=segments;i++){const u=i/segments*Math.PI*2;
      for(let j=0;j<=tubes;j++){const v=j/tubes*Math.PI*2; const rr=R+r*Math.cos(v);
        p.push(rr*Math.cos(u),r*Math.sin(v),rr*Math.sin(u));
        n.push(Math.cos(v)*Math.cos(u),Math.sin(v),Math.cos(v)*Math.sin(u));
      }}
    for(let i=0;i<segments;i++) for(let j=0;j<tubes;j++){const a=i*(tubes+1)+j,b=(i+1)*(tubes+1)+j;idx.push(a,b,a+1,b,b+1,a+1);}
    return this.makeMesh(p,n,idx);
  }
  private makeCylinder(r:number,h:number,segments:number):Mesh{
    const p:number[]=[],n:number[]=[],idx:number[]=[];
    for(let y=0;y<=1;y++) for(let i=0;i<=segments;i++){const a=i/segments*Math.PI*2; p.push(Math.cos(a)*r,(y-.5)*h,Math.sin(a)*r);n.push(Math.cos(a),0,Math.sin(a));}
    for(let i=0;i<segments;i++){const a=i,b=i+segments+1;idx.push(a,b,a+1,b,b+1,a+1);}
    return this.makeMesh(p,n,idx);
  }
  private makeGroundBuffer():WebGLBuffer{
    const gl=this.gl,b=gl.createBuffer(); if(!b) throw new Error('buffer');
    gl.bindBuffer(gl.ARRAY_BUFFER,b); gl.bufferData(gl.ARRAY_BUFFER,new Float32Array([-1400,0,-1400, 1400,0,-1400, -1400,0,1400, 1400,0,1400]),gl.STATIC_DRAW); return b;
  }
  private makeMesh(pos:number[],normal:number[],indices:number[]):Mesh{
    const gl=this.gl; const pb=gl.createBuffer(),nb=gl.createBuffer(),ib=gl.createBuffer();
    if(!pb||!nb||!ib) throw new Error('buffer');
    gl.bindBuffer(gl.ARRAY_BUFFER,pb);gl.bufferData(gl.ARRAY_BUFFER,new Float32Array(pos),gl.STATIC_DRAW);
    gl.bindBuffer(gl.ARRAY_BUFFER,nb);gl.bufferData(gl.ARRAY_BUFFER,new Float32Array(normal),gl.STATIC_DRAW);
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER,ib);gl.bufferData(gl.ELEMENT_ARRAY_BUFFER,new Uint16Array(indices),gl.STATIC_DRAW);
    return {
      pos:pb,
      normal:nb,
      index:ib,
      uv:null,
      texture:null,
      indexType:gl.UNSIGNED_SHORT,
      count:indices.length,
    };
  }
  private makeProgram(vs:string,fs:string):WebGLProgram{
    const gl=this.gl;
    const v=this.compile(gl.VERTEX_SHADER,vs), f=this.compile(gl.FRAGMENT_SHADER,fs), p=gl.createProgram();
    if(!p) throw new Error('program');
    gl.attachShader(p,v);gl.attachShader(p,f);gl.linkProgram(p);
    if(!gl.getProgramParameter(p,gl.LINK_STATUS)) throw new Error(gl.getProgramInfoLog(p)||'WebGL link failed');
    return p;
  }
  private compile(type:number,src:string):WebGLShader{
    const s=this.gl.createShader(type); if(!s) throw new Error('shader');
    this.gl.shaderSource(s,src);this.gl.compileShader(s);
    if(!this.gl.getShaderParameter(s,this.gl.COMPILE_STATUS)) throw new Error(this.gl.getShaderInfoLog(s)||'WebGL shader failed');
    return s;
  }
  private mustUniform(program:WebGLProgram,name:string):WebGLUniformLocation{
    const u=this.gl.getUniformLocation(program,name); if(!u) throw new Error('Missing uniform '+name); return u;
  }
}

export function createEcho3DRenderer(canvas:HTMLCanvasElement):Echo3DRenderer|null{
  try{return new Echo3DRenderer(canvas);}catch(error){console.warn('Echo Sphere 3D renderer unavailable, keeping 2D fallback.',error);return null;}
}
