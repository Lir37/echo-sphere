import type { GameState, SphereEntity, EnemyEntity, SphereProjectile, Particle } from './engine';
import type { SphereType } from './gameData';

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

interface Mesh { pos: WebGLBuffer; normal: WebGLBuffer; index: WebGLBuffer; count:number; }

const sphereTypes: Record<SphereType, {color:string; accent:string}> = {
  standard:{color:'#53ddff',accent:'#d8fbff'},
  sniper:{color:'#d65cff',accent:'#f5c8ff'},
  shotgun:{color:'#ff8638',accent:'#ffe0b0'},
  chain:{color:'#ffe14e',accent:'#fff8ba'},
  aura:{color:'#48e4b2',accent:'#c9ffec'},
};

const enemyColors: Record<string,string> = {
  normal:'#49d9ff', fast:'#ff5f9d', tank:'#ff9b45', boss:'#b879ff',
};

const VERTEX = `
attribute vec3 a_position;
attribute vec3 a_normal;
uniform mat4 u_mvp;
uniform mat4 u_model;
varying vec3 v_normal;
varying vec3 v_world;
varying vec3 v_local;
void main(){
  vec4 world=u_model*vec4(a_position,1.0);
  v_world=world.xyz;
  v_local=a_position;
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
varying vec3 v_normal;
varying vec3 v_world;
varying vec3 v_local;
void main(){
  vec3 N=normalize(v_normal);
  vec3 L=normalize(u_light);
  float ndl=max(dot(N,L),0.0);
  vec3 V=normalize(u_camera-v_world);
  float fresnel=pow(1.0-max(dot(N,V),0.0),3.2);
  float rim=pow(1.0-max(dot(N,vec3(0.0,1.0,0.0)),0.0),2.4);
  float micro=0.5+0.5*sin(v_local.x*9.0+v_local.z*7.0+sin(v_local.y*6.0)*1.7);
  float pulse=0.88+0.12*sin(u_time*3.0+v_local.y*4.0);
  vec3 base=mix(u_color,u_color*vec3(0.48,0.58,0.72),micro*0.28);
  vec3 H=normalize(L+vec3(0.35,0.78,0.45));
  float spec=pow(max(dot(N,H),0.0),42.0);
  float edge=pow(1.0-max(dot(N,V),0.0),5.5);
  float contour=0.5+0.5*sin(v_local.y*13.0+v_local.x*4.0);
  vec3 col=base*(0.10+ndl*0.90);
  col+=u_emissive*(0.22+rim*0.92+fresnel*1.72)*pulse;
  col+=u_emissive*spec*(1.75+edge*1.8);
  col+=u_emissive*micro*0.06;
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
  private mvpLoc:WebGLUniformLocation;
  private modelLoc:WebGLUniformLocation;
  private colorLoc:WebGLUniformLocation;
  private emissiveLoc:WebGLUniformLocation;
  private alphaLoc:WebGLUniformLocation;
  private timeLoc:WebGLUniformLocation;
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
    this.mvpLoc=this.mustUniform(this.program,'u_mvp');
    this.modelLoc=this.mustUniform(this.program,'u_model');
    this.colorLoc=this.mustUniform(this.program,'u_color');
    this.emissiveLoc=this.mustUniform(this.program,'u_emissive');
    this.alphaLoc=this.mustUniform(this.program,'u_alpha');
    this.timeLoc=this.mustUniform(this.program,'u_time');
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
    for(const sphere of s.spheres) if(sphere.alive) this.drawSphere(sphere,t,vp,player.x,player.y);
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

    // The GLB is the actual hero body. The procedural geometry below is only the
    // containment hardware and energy system around it, so the player no longer
    // reads as a stack of primitive spheres.
    if(playerAsset){
      const bodyScale=size*(dash?1.10:1.0);
      const body=mat4Multiply(
        mat4Translate(p.x,bob,p.y),
        mat4Multiply(
          mat4RotateY(t*.10),
          mat4Scale(bodyScale,bodyScale,bodyScale)
        )
      );
      const bodyColor=s.player.mutationStage>=3?'#102b3d':'#071522';
      const bodyGlow=s.player.mutationStage>=3?'#9a6dff':'#73eaff';
      this.drawModel(playerAsset,body,vp,bodyColor,bodyGlow,1);
    }else{
      const shell=mat4Multiply(
        mat4Translate(p.x,bob,p.y),
        mat4Multiply(mat4RotateY(t*.11),mat4Scale(size*.64,size*.72,size*.64))
      );
      this.drawModel(this.reactorShellMesh,shell,vp,'#071a29','#2b9fc8',.78);

      const inner=mat4Multiply(
        mat4Translate(p.x,bob,p.y),
        mat4Multiply(mat4RotateY(-t*.18),mat4Scale(size*.49,size*.57,size*.49))
      );
      this.drawModel(this.reactorShellMesh,inner,vp,'#03101b','#0c5d83',.92);

      const core=mat4Multiply(
        mat4Translate(p.x,bob,p.y),
        mat4Multiply(mat4RotateY(t*.34),mat4Scale(size*.285,size*.34,size*.285))
      );
      this.drawModel(this.facetCoreMesh,core,vp,'#bfefff','#ffffff',1);
    }

    // Hot reactor core remains visually separate from the shell and is now framed
    // by the imported hero mesh.
    const coreGlow=mat4Multiply(
      mat4Translate(p.x,bob,p.y),
      mat4Scale(size*.36,size*.41,size*.36)
    );
    this.drawModel(this.sphereMesh,coreGlow,vp,
      s.player.mutationStage>=3?'#32145f':'#0b5d87',
      s.player.mutationStage>=3?'#d39aff':'#51ddff',
      dash?0.28:0.18
    );

    // Machined collar rings lock the body to the containment frame.
    const collarR=size*.60;
    for(const [tilt,rot] of [[0,t*.34],[90*DEG,-t*.28]] as Array<[number,number]>){
      const collar=mat4Multiply(
        mat4Multiply(
          mat4Translate(p.x,bob,p.y),
          mat4Multiply(mat4RotateX(tilt),mat4RotateY(rot))
        ),
        mat4Scale(size*.66,size*.66,size*.66)
      );
      this.drawModel(this.torusMesh,collar,vp,'#173e52','#62dfff',.55);
    }

    // Three independently rotating containment bands create the characteristic
    // sci-fi silhouette seen in the reference.
    const cageR=size*.82;
    const rings=[
      mat4Multiply(mat4Translate(p.x,bob,p.y),mat4RotateY(t*.62)),
      mat4Multiply(mat4Translate(p.x,bob,p.y),mat4Multiply(mat4RotateX(61*DEG),mat4RotateY(-t*.47))),
      mat4Multiply(mat4Translate(p.x,bob,p.y),mat4Multiply(mat4RotateZ(61*DEG),mat4RotateY(t*.31)))
    ];
    for(const r of rings){
      this.drawModel(
        this.fineTorusMesh,
        mat4Multiply(r,mat4Scale(cageR,cageR,cageR)),
        vp,'#b7efff','#efffff',dash?1:0.94
      );
    }

    // Structural emitters are individual 3D nodes, not particles/sprites.
    const nodeR=cageR*1.015;
    const nodeSize=size*.064*(dash?1.18:1);
    const nodes=[
      [ nodeR,0,0],[-nodeR,0,0],
      [0, nodeR*Math.cos(61*DEG), nodeR*Math.sin(61*DEG)],
      [0,-nodeR*Math.cos(61*DEG),-nodeR*Math.sin(61*DEG)],
      [nodeR*.54,0,nodeR*.84],[-nodeR*.54,0,-nodeR*.84]
    ];
    for(let i=0;i<nodes.length;i++){
      const [nx,ny,nz]=nodes[i];
      const nm=mat4Multiply(
        mat4Translate(p.x+nx,bob+ny,p.y+nz),
        mat4Scale(nodeSize*(i%2?0.9:1.08),nodeSize,nodeSize*(i%2?0.9:1.08))
      );
      this.drawModel(this.facetCoreMesh,nm,vp,
        s.player.mutationStage>=3?'#e2d6ff':'#d9fbff',
        s.player.mutationStage>=3?'#c88cff':'#ffffff',.98);
    }

    // Thin struts make the frame physically believable and visually connected.
    const strutR=size*.037;
    const strutL=cageR*.72;
    const struts=[
      mat4Multiply(mat4Translate(p.x,bob,p.y),mat4RotateZ(90*DEG)),
      mat4Multiply(mat4Translate(p.x,bob,p.y),mat4RotateX(90*DEG)),
      mat4Multiply(
        mat4Translate(p.x,bob,p.y),
        mat4Multiply(mat4RotateX(61*DEG),mat4RotateY(t*.31))
      )
    ];
    for(const r of struts){
      this.drawModel(
        this.cylinderMesh,
        mat4Multiply(r,mat4Scale(strutR,strutL,strutR)),
        vp,'#2b7796','#9ceeff',.72
      );
    }

    // Animated energy filaments and caps provide the final manufactured detail.
    const filamentR=size*.70;
    for(let i=0;i<3;i++){
      const a=t*(1.1+i*.21)+i*Math.PI*2/3;
      const fm=mat4Multiply(
        mat4Translate(
          p.x+Math.cos(a)*filamentR,
          bob+Math.sin(a*1.7)*size*.12,
          p.y+Math.sin(a)*filamentR
        ),
        mat4Scale(size*.026,size*.026,size*.13)
      );
      this.drawModel(this.facetCoreMesh,fm,vp,'#7ee9ff','#dfffff',.8);
    }

    for(const y of [-1,1]){
      const cap=mat4Multiply(
        mat4Translate(p.x,bob+y*size*.67,p.y),
        mat4Scale(size*.23,size*.075,size*.23)
      );
      this.drawModel(this.facetCoreMesh,cap,vp,'#12394d','#54dfff',.8);
    }

    // Dash state gets a stronger energy shell without changing gameplay geometry.
    if(dash){
      const dashRing=mat4Multiply(
        mat4Translate(p.x,bob,p.y),
        mat4Multiply(mat4RotateZ(t*2.4),mat4Scale(size*1.02,size*1.02,size*1.02))
      );
      this.drawModel(this.torusMesh,dashRing,vp,'#d5faff','#ffffff',.70);
    }

    const outer=mat4Multiply(
      mat4Translate(p.x,bob,p.y),
      mat4Multiply(mat4RotateX(34*DEG),mat4Scale(size*.94,size*.94,size*.94))
    );
    this.drawModel(this.fineTorusMesh,outer,vp,'#3aafdc','#77eaff',dash?.34:.20);
    this.drawRing(p.x,p.y,size*(dash?1.16:1.06),t*.45,'#52ddff',t,vp,dash?.28:.18);
  }

  private drawSphere(s:SphereEntity,t:number,vp:Float32Array,cx:number,cy:number){
    const def=sphereTypes[s.type];
    const tier=Math.max(1,Math.min(7,Math.round(s.visualTier||1)));
    const pulse=1+Math.sin(t*3.1+s.rotation*0.7)*0.035;
    const y=24+Math.sin(t*2.4+s.pos.x*.01)*2.5;
    const base=17+Math.min(10,tier*1.45);

    const previousAttack=this.sphereAttackTimers.get(s);
    const previousAura=this.sphereAuraTimers.get(s);
    const delay=Math.max(0.15,s.attackDelay);
    const fired=
      (previousAttack===undefined && s.attackTimer>delay*0.7) ||
      (previousAttack!==undefined && s.attackTimer>previousAttack+Math.max(0.12,delay*0.35));
    const auraPulse=
      previousAura===undefined && s.auraTimer>0.01 ||
      (previousAura!==undefined && s.auraTimer>previousAura+0.12);
    if(fired) this.sphereShotTimes.set(s,t);
    if(auraPulse) this.spherePulseTimes.set(s,t);
    this.sphereAttackTimers.set(s,s.attackTimer);
    this.sphereAuraTimers.set(s,s.auraTimer);

    const shotAge=t-(this.sphereShotTimes.get(s)??-999);
    const auraAge=t-(this.spherePulseTimes.get(s)??-999);
    const shotFlash=Math.max(0,1-shotAge/0.22);
    const auraFlash=Math.max(0,1-auraAge/0.55);
    const rot=s.rotation+t*(s.type==='sniper'?.16:s.type==='chain'?.72:.45);
    const origin:[number,number,number]=[s.pos.x,y,s.pos.y];

    // The reference language is a luminous "contained energy core":
    // one bright heart, transparent-looking orbital bands, and articulated
    // nodes. Every tier adds real 3D structure instead of merely scaling it.
    const coreR=base*(0.23+Math.min(7,tier)*0.012);
    const outerCore=mat4Multiply(
      mat4Translate(origin[0],origin[1],origin[2]),
      mat4Scale(coreR*1.35*pulse,coreR*1.35*pulse,coreR*1.35*pulse)
    );
    this.drawModel(this.sphereMesh,outerCore,vp,'#071321',def.color,.95);

    const core=mat4Multiply(
      mat4Translate(origin[0],origin[1],origin[2]),
      mat4Scale(coreR*pulse,coreR*0.96*pulse,coreR*pulse)
    );
    this.drawModelAdditive(this.sphereMesh,core,vp,def.color,def.accent,.95);

    const coreHot=mat4Multiply(
      mat4Translate(origin[0],origin[1]+Math.sin(t*6+s.rotation)*coreR*.06,origin[2]),
      mat4Scale(coreR*.52*pulse,coreR*.52*pulse,coreR*.52*pulse)
    );
    this.drawModelAdditive(this.facetCoreMesh,coreHot,vp,def.accent,'#ffffff',1);

    const haloR=base*(0.68+tier*.025);
    this.drawModelAdditive(
      this.sphereMesh,
      mat4Multiply(mat4Translate(origin[0],origin[1],origin[2]),mat4Scale(haloR,haloR,haloR)),
      vp,
      def.color,
      def.accent,
      0.08+Math.sin(t*2.2)*0.02
    );

    const ringAlpha=0.58+Math.min(tier,4)*0.045;
    const drawRing3D=(rotation:Float32Array,scale:[number,number,number],alpha:number,thick=false)=>{
      const ring=mat4Multiply(
        mat4Translate(origin[0],origin[1],origin[2]),
        mat4Multiply(rotation,mat4Scale(haloR*scale[0],haloR*scale[1],haloR*scale[2]))
      );
      this.drawModel(this.fineTorusMesh,ring,vp,def.accent,def.accent,alpha*(thick?1.22:1));
    };

    // Tier I: the simple reference sphere. Tier II-III build the tri-axial cage.
    drawRing3D(mat4RotateY(rot),[1,1,1],ringAlpha);
    if(tier>=2) drawRing3D(mat4RotateX(Math.PI/2),[1,1,1],ringAlpha*.94);
    if(tier>=3) drawRing3D(mat4RotateZ(Math.PI/2),[1,1,1],ringAlpha*.9);

    // Tier III onward gets the diagonal "orbital cage" visible in the reference.
    const diagonal=[
      mat4Multiply(mat4RotateX(54*DEG),mat4RotateY(rot*.73)),
      mat4Multiply(mat4RotateZ(54*DEG),mat4RotateY(-rot*.61)),
      mat4Multiply(mat4RotateX(-54*DEG),mat4RotateY(rot*1.12)),
      mat4Multiply(mat4RotateZ(-54*DEG),mat4RotateY(-rot*.91)),
    ];
    for(let i=0;i<Math.min(diagonal.length,Math.max(0,tier-2));i++){
      drawRing3D(diagonal[i],[1,0.93,1],ringAlpha*.72);
    }

    // Tier V-VI fill the spaces between the main rings with finer latitude bands.
    if(tier>=5){
      for(const tilt of [-28,28]){
        const r=mat4Multiply(mat4RotateX(tilt*DEG),mat4RotateY(rot*(tilt>0?.55:-.47)));
        drawRing3D(r,[1,.72,1],ringAlpha*.56);
      }
    }
    if(tier>=6){
      for(const tilt of [-43,43]){
        const r=mat4Multiply(mat4RotateZ(tilt*DEG),mat4RotateY(rot*(tilt>0?.44:-.38)));
        drawRing3D(r,[.76,1,.76],ringAlpha*.46);
      }
    }

    // Tier VII is the final "sphere of spheres": a larger external cage, bright
    // anchor nodes, radial braces and a living halo.
    if(tier>=7){
      const outerScale=haloR*1.18;
      const outerRings=[
        mat4RotateY(rot*.83),
        mat4Multiply(mat4RotateX(67*DEG),mat4RotateY(-rot*.52)),
        mat4Multiply(mat4RotateZ(67*DEG),mat4RotateY(rot*.36)),
      ];
      for(const r of outerRings){
        this.drawModel(
          this.fineTorusMesh,
          mat4Multiply(
            mat4Translate(origin[0],origin[1],origin[2]),
            mat4Multiply(r,mat4Scale(outerScale,outerScale,outerScale))
          ),
          vp,def.accent,'#ffffff',.72
        );
      }
      this.drawRing(s.pos.x,s.pos.y,base*1.62,t*.74,def.accent,t,vp,.3);
      this.drawRing(s.pos.x,s.pos.y,base*2.08,-t*.48,def.color,t,vp,.19);
      this.drawRing(s.pos.x,s.pos.y,base*2.62,t*.25,def.accent,t,vp,.11);
    }

    // Structural anchor points make the model read as manufactured hardware, not
    // as a collection of floating circles.
    const nodeR=haloR*(tier>=7?1.02:.97);
    const nodes:Array<[number,number,number]>=[
      [nodeR,0,0],[-nodeR,0,0],[0,nodeR,0],[0,-nodeR,0],[0,0,nodeR],[0,0,-nodeR]
    ];
    if(tier>=3){
      for(let i=0;i<8;i++){
        const a=i/8*Math.PI*2+rot*.16;
        nodes.push([nodeR*.82*Math.cos(a),nodeR*.26*Math.sin(a*2),nodeR*.82*Math.sin(a)]);
      }
    }
    if(tier>=5){
      for(let i=0;i<8;i++){
        const a=i/8*Math.PI*2-rot*.12;
        nodes.push([nodeR*.70*Math.cos(a),nodeR*.54*Math.sin(a),nodeR*.70*Math.sin(a)]);
      }
    }

    const nodeSize=base*(tier>=6?.061:tier>=3?.052:.045);
    const nodeLimit=tier>=7?nodes.length:tier>=5?16:tier>=3?14:6;
    for(let i=0;i<nodeLimit;i++){
      const n=nodes[i];
      const wobble=1+Math.sin(t*2.6+i*.87)*0.035;
      const nm=mat4Multiply(
        mat4Translate(origin[0]+n[0]*wobble,origin[1]+n[1]*wobble,origin[2]+n[2]*wobble),
        mat4Scale(nodeSize*(i%3===0?1.12:1),nodeSize,nodeSize*(i%2===0?1.08:1))
      );
      this.drawModelAdditive(this.sphereMesh,nm,vp,def.color,def.accent,.88);
      if(tier>=6 && i%2===0){
        const nm2=mat4Multiply(
          mat4Translate(origin[0]+n[0]*wobble,origin[1]+n[1]*wobble,origin[2]+n[2]*wobble),
          mat4Scale(nodeSize*.42,nodeSize*.42,nodeSize*.42)
        );
        this.drawModelAdditive(this.sphereMesh,nm2,vp,def.accent,'#ffffff',1);
      }
    }

    // Braces tie cardinal nodes into the central reactor.
    if(tier>=3){
      for(let i=0;i<Math.min(6,nodes.length);i++){
        const n=nodes[i];
        this.drawBeamBetween(
          [origin[0],origin[1],origin[2]],
          [origin[0]+n[0],origin[1]+n[1],origin[2]+n[2]],
          base*(tier>=7?.012:.009),
          def.color,def.accent,tier>=7?.56:.42,vp
        );
      }
    }
    if(tier>=5){
      for(let i=6;i<Math.min(14,nodes.length);i+=2){
        const a=nodes[i],b=nodes[(i+2)%Math.min(14,nodes.length)];
        this.drawBeamBetween(
          [origin[0]+a[0],origin[1]+a[1],origin[2]+a[2]],
          [origin[0]+b[0],origin[1]+b[1],origin[2]+b[2]],
          base*.006,def.accent,def.accent,.22,vp
        );
      }
    }

    // Functional modules by tower type. The silhouette stays reference-faithful,
    // while each weapon gets a readable "job" built into the reactor.
    const dirX=Math.cos(s.rotation), dirZ=Math.sin(s.rotation);
    const sideX=-dirZ, sideZ=dirX;
    const forward=(d:number,side:number,yOff:number):[number,number,number]=>[
      origin[0]+dirX*d+sideX*side,
      origin[1]+yOff,
      origin[2]+dirZ*d+sideZ*side
    ];

    if(s.type==='sniper'){
      const emitter=forward(base*.98,0,0);
      this.drawBeamBetween(origin,emitter,base*.028,def.color,def.accent,.74,vp);
      const lens=mat4Multiply(
        mat4Translate(emitter[0],emitter[1],emitter[2]),
        mat4Scale(base*.095,base*.095,base*.095)
      );
      this.drawModelAdditive(this.sphereMesh,lens,vp,def.accent,'#ffffff',1);
      this.drawModel(
        this.fineTorusMesh,
        mat4Multiply(
          mat4Translate(emitter[0],emitter[1],emitter[2]),
          mat4Multiply(mat4RotateY(s.rotation),mat4Scale(base*.18,base*.18,base*.18))
        ),
        vp,def.accent,'#ffffff',.85
      );
      if(shotFlash>0){
        const muzzle=forward(base*(1.06+shotFlash*.18),0,0);
        this.drawBeamBetween(emitter,muzzle,base*(.05+.06*shotFlash),def.accent,'#ffffff',.55*shotFlash,vp);
        this.drawModelAdditive(
          this.sphereMesh,
          mat4Multiply(mat4Translate(muzzle[0],muzzle[1],muzzle[2]),mat4Scale(base*(.10+.12*shotFlash),base*(.10+.12*shotFlash),base*(.10+.12*shotFlash))),
          vp,def.accent,'#ffffff',.95
        );
      }
    }else if(s.type==='shotgun'){
      const spread=.12;
      for(let i=-1;i<=1;i++){
        const a=s.rotation+i*spread;
        const dx=Math.cos(a),dz=Math.sin(a);
        const emitter:[number,number,number]=[origin[0]+dx*base*.82,origin[1]+i*base*.06,origin[2]+dz*base*.82];
        this.drawBeamBetween(origin,emitter,base*.019,def.color,def.accent,.55,vp);
        const pellet=mat4Multiply(
          mat4Translate(emitter[0],emitter[1],emitter[2]),
          mat4Scale(base*.052,base*.052,base*.052)
        );
        this.drawModelAdditive(this.sphereMesh,pellet,vp,def.accent,'#ffffff',.95);
        if(shotFlash>0){
          const end:[number,number,number]=[emitter[0]+dx*base*(.22+.20*shotFlash),emitter[1],emitter[2]+dz*base*(.22+.20*shotFlash)];
          this.drawBeamBetween(emitter,end,base*(.022+.032*shotFlash),def.accent,'#ffffff',.38*shotFlash,vp);
        }
      }
    }else if(s.type==='chain'){
      const chainCount=tier>=5?8:6;
      for(let i=0;i<chainCount;i++){
        const a=i/chainCount*Math.PI*2-t*.72;
        const n:[number,number,number]=[
          origin[0]+Math.cos(a)*base*.74,
          origin[1]+Math.sin(a*2.0+t*.45)*base*.32,
          origin[2]+Math.sin(a)*base*.74
        ];
        const rr=mat4Multiply(
          mat4Translate(n[0],n[1],n[2]),
          mat4Scale(base*.042,base*.042,base*.042)
        );
        this.drawModelAdditive(this.sphereMesh,rr,vp,def.color,'#ffffff',.9);
        if(i>0){
          const pA:[number,number,number]=[
            origin[0]+Math.cos((i-1)/chainCount*Math.PI*2-t*.72)*base*.74,
            origin[1]+Math.sin(((i-1)/chainCount*Math.PI*2-t*.72)*2+t*.45)*base*.32,
            origin[2]+Math.sin((i-1)/chainCount*Math.PI*2-t*.72)*base*.74
          ];
          this.drawBeamBetween(pA,n,base*.0055,def.accent,def.accent,.22,vp);
        }
      }
    }else if(s.type==='aura'){
      const fieldBase=base*(1.7+.22*tier);
      this.drawRing(s.pos.x,s.pos.y,fieldBase,t*.34,def.color,t,vp,.11+.035*auraFlash);
      this.drawRing(s.pos.x,s.pos.y,fieldBase*1.28,-t*.28,def.accent,t,vp,.07+.025*auraFlash);
      this.drawRing(s.pos.x,s.pos.y,fieldBase*1.62,t*.17,def.color,t,vp,.045+.018*auraFlash);
      if(auraFlash>0){
        this.drawModelAdditive(
          this.sphereMesh,
          mat4Multiply(
            mat4Translate(origin[0],origin[1],origin[2]),
            mat4Scale(base*(.40+.22*auraFlash),base*(.40+.22*auraFlash),base*(.40+.22*auraFlash))
          ),
          vp,def.color,'#ffffff',.13*auraFlash
        );
      }
    }else{
      // Standard sphere gets a clean forward "core rail" and a restrained pulse.
      const rail=forward(base*.76,0,0);
      this.drawBeamBetween(origin,rail,base*.014,def.color,def.accent,.36,vp);
      if(shotFlash>0){
        const p=forward(base*(.86+.22*shotFlash),0,0);
        this.drawBeamBetween(rail,p,base*(.024+.038*shotFlash),def.accent,'#ffffff',.38*shotFlash,vp);
      }
    }

    // Every tower gets the reference's breathing halo. Higher tiers are brighter
    // and more "contained" instead of simply becoming bigger.
    const breath=.12+.08*Math.sin(t*2.1+s.rotation);
    this.drawRing(s.pos.x,s.pos.y,base*(1.16+tier*.045),rot,def.accent,t,vp,.16+breath);
    if(tier>=4) this.drawRing(s.pos.x,s.pos.y,base*(1.45+tier*.05),-rot*.7,def.color,t,vp,.10+shotFlash*.16);
    if(shotFlash>0){
      this.drawModelAdditive(
        this.sphereMesh,
        mat4Multiply(
          mat4Translate(origin[0],origin[1],origin[2]),
          mat4Scale(base*(.48+.42*shotFlash),base*(.48+.42*shotFlash),base*(.48+.42*shotFlash))
        ),
        vp,def.accent,'#ffffff',.10+.18*shotFlash
      );
    }

    void cx;void cy;
  }

  private drawEnemy(e:EnemyEntity,t:number,vp:Float32Array){
    const c=e.isBoss?enemyColors.boss:(enemyColors[e.type]||e.color||enemyColors.normal),scale=e.isBoss?Math.max(34,e.radius*1.7):Math.max(13,e.radius*.95),hit=e.hitFlash>0?1.7:1;
    const y=scale*.62+Math.sin(t*2.2+e.rotation)*1.5,rot=e.rotation+t*(e.type==='fast'?1.8:.7),key=e.isBoss?'boss':'enemy_'+(e.type==='fast'?'fast':e.type==='tank'?'tank':'normal'),asset=this.modelAssets.get(key);
    if(asset){
      const m=mat4Multiply(mat4Multiply(mat4Translate(e.pos.x,y,e.pos.y),mat4RotateY(rot)),mat4Scale(scale*1.18*hit,scale*1.18*hit,scale*1.18*hit));
      this.drawModel(asset,m,vp,'#07111c',c,1);
    }else{
      const m=mat4Multiply(mat4Multiply(mat4Translate(e.pos.x,y,e.pos.y),mat4RotateY(rot)),mat4Scale(scale*hit,scale*.85*hit,scale*hit));
      this.drawModel(this.sphereMesh,m,vp,'#0a101a',c,1);
    }
    this.drawRing(e.pos.x,e.pos.y,scale*(e.isBoss?1.35:1.15),rot,c,t,vp,e.isBoss?.42:.22);
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
      this.drawModel(asset,m,vp,p.color,'#ffffff',1);
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

  private async loadModelAssets(): Promise<void>{
    if(this.modelLoadStarted)return;this.modelLoadStarted=true;
    const paths:Record<string,string>={player:'/art3d/player.glb',sphere_standard:'/art3d/sphere-standard.glb',sphere_sniper:'/art3d/sphere-sniper.glb',sphere_shotgun:'/art3d/sphere-shotgun.glb',sphere_chain:'/art3d/sphere-chain.glb',sphere_aura:'/art3d/sphere-aura.glb',enemy_normal:'/art3d/enemy-normal.glb',enemy_fast:'/art3d/enemy-fast.glb',enemy_tank:'/art3d/enemy-tank.glb',boss_shooter:'/art3d/boss-shooter.glb',boss_charger:'/art3d/boss-charger.glb',boss_summoner:'/art3d/boss-summoner.glb',boss_aura:'/art3d/boss-aura.glb',projectile:'/art3d/projectile.glb'};
    for(const [key,path] of Object.entries(paths)){
      try{const r=await fetch(path,{cache:'force-cache'});if(!r.ok)throw new Error('HTTP '+r.status);const mesh=this.loadGlbMesh(await r.arrayBuffer());if(mesh)this.modelAssets.set(key,mesh);}
      catch(error){console.warn('Echo3D asset failed:',key,error);}
    }
  }
  private loadGlbMesh(buffer:ArrayBuffer):Mesh|null{
    if(buffer.byteLength<20)return null;const dv=new DataView(buffer);
    if(dv.getUint32(0,true)!==0x46546c67||dv.getUint32(4,true)!==2)return null;
    let off=12,json=null,bin=null;
    while(off+8<=buffer.byteLength){const len=dv.getUint32(off,true),type=dv.getUint32(off+4,true),start=off+8;
      if(type===0x4e4f534a)json=JSON.parse(new TextDecoder().decode(new Uint8Array(buffer,start,len)));
      else if(type===0x004e4942)bin=new Uint8Array(buffer,start,len);off=start+len;
    }
    if(!json||!bin||!json.meshes?.length)return null;const prim=json.meshes[0]?.primitives?.[0];if(!prim?.attributes?.POSITION)return null;
    const read=(idx:number)=>{const a=json.accessors[idx],v=json.bufferViews[a.bufferView],cc=a.type==='VEC3'?3:a.type==='VEC2'?2:1,start=(v.byteOffset||0)+(a.byteOffset||0);
      if(a.componentType===5126)return new Float32Array(bin.buffer,bin.byteOffset+start,a.count*cc).slice();
      if(a.componentType===5123)return new Uint16Array(bin.buffer,bin.byteOffset+start,a.count*cc).slice();
      return null;};
    const pos=read(prim.attributes.POSITION),norm=prim.attributes.NORMAL!==undefined?read(prim.attributes.NORMAL):null,idx=prim.indices!==undefined?read(prim.indices):null;
    if(!(pos instanceof Float32Array)||!(idx instanceof Uint16Array))return null;const n=norm instanceof Float32Array?norm:new Float32Array(pos.length);
    if(!norm)for(let i=0;i<n.length;i+=3){n[i]=0;n[i+1]=1;n[i+2]=0;}
    return this.makeMeshFromTypedArrays(pos,n,idx);
  }
  private makeMeshFromTypedArrays(pos:Float32Array,norm:Float32Array,idx:Uint16Array):Mesh{
    const gl=this.gl,pb=gl.createBuffer(),nb=gl.createBuffer(),ib=gl.createBuffer();if(!pb||!nb||!ib)throw new Error('buffer');
    gl.bindBuffer(gl.ARRAY_BUFFER,pb);gl.bufferData(gl.ARRAY_BUFFER,pos,gl.STATIC_DRAW);gl.bindBuffer(gl.ARRAY_BUFFER,nb);gl.bufferData(gl.ARRAY_BUFFER,norm,gl.STATIC_DRAW);gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER,ib);gl.bufferData(gl.ELEMENT_ARRAY_BUFFER,idx,gl.STATIC_DRAW);return{pos:pb,normal:nb,index:ib,count:idx.length};
  }

  private drawModel(mesh:Mesh,model:Float32Array,vp:Float32Array,color:string,emissive:string,alpha:number){
    const gl=this.gl;
    const mvp=mat4Multiply(vp,model);
    gl.uniformMatrix4fv(this.mvpLoc,false,mvp);
    gl.uniformMatrix4fv(this.modelLoc,false,model);
    const c=hex(color), e=hex(emissive);
    gl.uniform3f(this.colorLoc,c[0],c[1],c[2]);
    gl.uniform3f(this.emissiveLoc,e[0],e[1],e[2]);
    gl.uniform1f(this.alphaLoc,alpha);
    gl.bindBuffer(gl.ARRAY_BUFFER,mesh.pos); gl.enableVertexAttribArray(this.posLoc); gl.vertexAttribPointer(this.posLoc,3,gl.FLOAT,false,0,0);
    gl.bindBuffer(gl.ARRAY_BUFFER,mesh.normal); gl.enableVertexAttribArray(this.normalLoc); gl.vertexAttribPointer(this.normalLoc,3,gl.FLOAT,false,0,0);
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER,mesh.index);
    gl.drawElements(gl.TRIANGLES,mesh.count,gl.UNSIGNED_SHORT,0);
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
    return {pos:pb,normal:nb,index:ib,count:indices.length};
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
