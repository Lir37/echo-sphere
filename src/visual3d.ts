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

  constructor(private canvas:HTMLCanvasElement){
    const gl=canvas.getContext('webgl',{alpha:false,antialias:true,powerPreference:'high-performance'});
    if(!gl) throw new Error('WebGL is not supported');
    this.gl=gl;
    this.program=this.makeProgram(VERTEX,FRAGMENT);
    this.groundProgram=this.makeProgram(GROUND_V,GROUND_F);
    this.sphereMesh=this.makeUvSphere(1,18,12);
    this.torusMesh=this.makeTorus(1,0.055,32,8);
    this.fineTorusMesh=this.makeTorus(1,0.022,48,6);
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
    const pulse=1+Math.sin(t*3.15)*0.032;
    const size=25*pulse;
    const bob=21+Math.sin(t*2.7)*1.15;
    const base=mat4Translate(p.x,bob,p.y);

    // Primary silhouette: a faceted reactor body with a tapered, machined profile.
    const shell=mat4Multiply(
      base,
      mat4Multiply(mat4RotateY(t*.11),mat4Scale(size*.64,size*.72,size*.64))
    );
    this.drawModel(this.reactorShellMesh,shell,vp,'#071a29','#2b9fc8',.78);

    // Dark inset volume gives the shell actual depth instead of a flat outer glow.
    const inner=mat4Multiply(
      base,
      mat4Multiply(mat4RotateY(-t*.18),mat4Scale(size*.49,size*.57,size*.49))
    );
    this.drawModel(this.reactorShellMesh,inner,vp,'#03101b','#0c5d83',.92);

    // Faceted luminous nucleus. Two nested volumes create a hot center and a cooler energy envelope.
    const core=mat4Multiply(
      base,
      mat4Multiply(mat4RotateY(t*.34),mat4Scale(size*.285,size*.34,size*.285))
    );
    this.drawModel(this.facetCoreMesh,core,vp,'#bfefff','#ffffff',1);

    const coreGlow=mat4Multiply(base,mat4Scale(size*.39,size*.44,size*.39));
    this.drawModel(this.sphereMesh,coreGlow,vp,'#0b5d87','#51ddff',.19);

    // Machined collar rings lock the central body to the containment frame.
    // A second, slightly wider set creates visible stepped geometry at the shell joints.
    const collarR=size*.60;
    for(const [tilt,rot] of [[0,t*.34],[90*DEG,-t*.28]] as Array<[number,number]>){
      const collar=mat4Multiply(
        mat4Multiply(base,mat4Multiply(mat4RotateX(tilt),mat4RotateY(rot))),
        mat4Scale(size*.66,size*.66,size*.66)
      );
      this.drawModel(this.torusMesh,collar,vp,'#173e52','#62dfff',.55);
    }
    for(const [ang,tilt,scale] of [
      [t*.52,68*DEG,1.0],[-t*.43,-68*DEG,1.0],[t*.27,18*DEG,.88]
    ] as Array<[number,number,number]>){
      const m=mat4Multiply(
        mat4Multiply(base,mat4Multiply(mat4RotateX(tilt),mat4RotateY(ang))),
        mat4Scale(collarR*scale,collarR*scale,collarR*scale)
      );
      this.drawModel(this.fineTorusMesh,m,vp,'#63c9e8','#b8f5ff',.62);
    }

    // Three independent orbital bands form the recognizable outer cage.
    // The slight radius offset between layers prevents the silhouette from collapsing into one ring.
    const cageR=size*.82;
    const rings=[
      mat4Multiply(base,mat4RotateY(t*.62)),
      mat4Multiply(base,mat4Multiply(mat4RotateX(61*DEG),mat4RotateY(-t*.47))),
      mat4Multiply(base,mat4Multiply(mat4RotateZ(61*DEG),mat4RotateY(t*.31)))
    ];
    for(const r of rings){
      this.drawModel(
        this.fineTorusMesh,
        mat4Multiply(r,mat4Scale(cageR,cageR,cageR)),
        vp,'#b7efff','#efffff',.94
      );
    }

    // Six structural emitters sit at the cage poles. They are volumetric, not sprites.
    const nodeR=cageR*1.015;
    const nodeSize=size*.064;
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
      this.drawModel(this.facetCoreMesh,nm,vp,'#d9fbff','#ffffff',.98);
    }

    // Short structural struts connect the cage to the body. They are deliberately thin,
    // so the silhouette stays elegant rather than becoming a robotic ball.
    const strutR=size*.037;
    const strutL=cageR*.72;
    const struts=[
      mat4Multiply(base,mat4RotateZ(90*DEG)),
      mat4Multiply(base,mat4RotateX(90*DEG)),
      mat4Multiply(base,mat4Multiply(mat4RotateX(61*DEG),mat4RotateY(t*.31)))
    ];
    for(const r of struts){
      const m=mat4Multiply(r,mat4Scale(strutR,strutL,strutR));
      this.drawModel(this.cylinderMesh,m,vp,'#2b7796','#9ceeff',.72);
    }

    // Small rotating energy filaments add motion at close range.
    const filamentR=size*.70;
    for(let i=0;i<3;i++){
      const a=t*(1.1+i*.21)+i*Math.PI*2/3;
      const fm=mat4Multiply(
        mat4Translate(p.x+Math.cos(a)*filamentR,bob+Math.sin(a*1.7)*size*.12,p.y+Math.sin(a)*filamentR),
        mat4Scale(size*.026,size*.026,size*.13)
      );
      this.drawModel(this.facetCoreMesh,fm,vp,'#7ee9ff','#dfffff',.8);
    }

    // A small top and bottom cap make the reactor read as a manufactured artifact.
    for(const y of [-1,1]){
      const cap=mat4Multiply(
        mat4Translate(p.x,bob+y*size*.67,p.y),
        mat4Scale(size*.23,size*.075,size*.23)
      );
      this.drawModel(this.facetCoreMesh,cap,vp,'#12394d','#54dfff',.8);
    }

    // External energy field and the separate ground halo complete the presentation.
    const outer=mat4Multiply(
      base,
      mat4Multiply(mat4RotateX(34*DEG),mat4Scale(size*.94,size*.94,size*.94))
    );
    this.drawModel(this.fineTorusMesh,outer,vp,'#3aafdc','#77eaff',.20);
    this.drawRing(p.x,p.y,size*1.06,t*.45,'#52ddff',t,vp,.18);
  }

  private drawSphere(s:SphereEntity,t:number,vp:Float32Array,cx:number,cy:number){
    const def=sphereTypes[s.type],pulse=1+Math.sin(t*3+s.rotation)*.045,y=24+Math.sin(t*2.4+s.pos.x*.01)*3,base=17+Math.min(10,s.visualTier*1.5),asset=this.modelAssets.get('sphere_'+s.type);
    const m=mat4Multiply(mat4Multiply(mat4Translate(s.pos.x,y,s.pos.y),mat4RotateY(s.rotation+t*.55)),mat4Scale(base*pulse,base*pulse,base*pulse));
    if(asset)this.drawModel(asset,m,vp,def.color,def.accent,1);else this.drawModel(this.sphereMesh,m,vp,def.color,def.accent,1);
    this.drawRing(s.pos.x,s.pos.y,base*1.45,t*(s.type==='sniper'?-1.3:1),def.color,t,vp,.34);
    if(s.type==='aura'){this.drawRing(s.pos.x,s.pos.y,base*2.4,t*.5,def.color,t,vp,.18);this.drawRing(s.pos.x,s.pos.y,base*3,-t*.3,def.accent,t,vp,.1);}
    if(s.visualTier>=4)this.drawRing(s.pos.x,s.pos.y,base*1.8,-t*1.6,def.accent,t,vp,.5);
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
    const len=Math.max(7,Math.hypot(p.vel.x,p.vel.y)*.045),a=Math.atan2(p.vel.y,p.vel.x),asset=this.modelAssets.get('projectile');
    if(asset){
      const m=mat4Multiply(mat4Multiply(mat4Translate(p.pos.x,11,p.pos.y),mat4RotateY(-a)),mat4Scale(Math.max(4,p.radius*1.45),Math.max(4,p.radius*1.45),Math.max(6,len*1.1)));
      this.drawModel(asset,m,vp,p.color,'#ffffff',1);
    }else{
      const m=mat4Multiply(mat4Multiply(mat4Translate(p.pos.x,11,p.pos.y),mat4RotateY(-a)),mat4Scale(Math.max(4,p.radius*1.5),Math.max(4,p.radius*1.5),len));
      this.drawModel(this.sphereMesh,m,vp,p.color,'#ffffff',1);
    }
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
