import type { GameState, SphereEntity, EnemyEntity, MinionEntity } from './engine';
import { PLAYER_RADIUS } from './engine';
import { SPHERE_TYPES } from './gameData';

type GL = WebGLRenderingContext;
type M4 = Float32Array;
type Mesh = { pos: WebGLBuffer; normal: WebGLBuffer; count: number };

const VS = [
  'attribute vec3 aPosition;',
  'attribute vec3 aNormal;',
  'uniform mat4 uModel;',
  'uniform vec2 uViewport;',
  'varying vec3 vNormal;',
  'void main(){',
  ' vec4 w=uModel*vec4(aPosition,1.0);',
  ' float sx=w.x*2.0/uViewport.x;',
  ' float sy=(-w.z+w.y*0.42)*2.0/uViewport.y;',
  ' float sz=clamp((-w.z-w.y*0.72)/1800.0,-0.95,0.95);',
  ' gl_Position=vec4(sx,sy,sz,1.0);',
  ' vNormal=normalize(mat3(uModel)*aNormal);',
  '}',
].join('\n');

const FS = [
  'precision mediump float;',
  'uniform vec3 uColor;',
  'uniform vec3 uLight;',
  'uniform float uGlow;',
  'uniform float uAlpha;',
  'varying vec3 vNormal;',
  'void main(){',
  ' vec3 n=normalize(vNormal);',
  ' float d=max(dot(n,normalize(uLight)),0.0);',
  ' float rim=pow(1.0-max(dot(n,vec3(0.0,0.75,0.66)),0.0),2.0);',
  ' vec3 c=uColor*(0.18+d*0.82);',
  ' c+=uColor*uGlow*(0.45+rim*1.5);',
  ' gl_FragColor=vec4(c,uAlpha);',
  '}',
].join('\n');

function rgb(hex:string):[number,number,number]{
  const h=hex.replace('#','');
  return [parseInt(h.slice(0,2),16)/255,parseInt(h.slice(2,4),16)/255,parseInt(h.slice(4,6),16)/255];
}
function I():M4{return new Float32Array([1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1]);}
function mul(a:M4,b:M4):M4{
  const o=new Float32Array(16);
  for(let r=0;r<4;r++)for(let c=0;c<4;c++)o[c*4+r]=a[r]*b[c*4]+a[4+r]*b[c*4+1]+a[8+r]*b[c*4+2]+a[12+r]*b[c*4+3];
  return o;
}
function T(x:number,y:number,z:number):M4{const m=I();m[12]=x;m[13]=y;m[14]=z;return m;}
function S(x:number,y:number,z:number):M4{const m=I();m[0]=x;m[5]=y;m[10]=z;return m;}
function RY(a:number):M4{const c=Math.cos(a),s=Math.sin(a);return new Float32Array([c,0,-s,0,0,1,0,0,s,0,c,0,0,0,0,1]);}
function RX(a:number):M4{const c=Math.cos(a),s=Math.sin(a);return new Float32Array([1,0,0,0,0,c,s,0,0,-s,c,0,0,0,0,1]);}
function rz(a:number):M4{const c=Math.cos(a),s=Math.sin(a);return new Float32Array([c,s,0,0,-s,c,0,0,0,0,1,0,0,0,0,1]);}
function model(x:number,y:number,z:number,rx:number,ry:number,rz0:number,sx:number,sy:number,sz:number):M4{
  return mul(mul(mul(mul(T(x,y,z),RY(ry)),RX(rx)),rz(rz0)),S(sx,sy,sz));
}
function tri(d:{p:number[];n:number[]},a:number[],b:number[],c:number[]):void{
  const ab=[b[0]-a[0],b[1]-a[1],b[2]-a[2]],ac=[c[0]-a[0],c[1]-a[1],c[2]-a[2]];
  const nx=ab[1]*ac[2]-ab[2]*ac[1],ny=ab[2]*ac[0]-ab[0]*ac[2],nz=ab[0]*ac[1]-ab[1]*ac[0],l=Math.hypot(nx,ny,nz)||1;
  const n=[nx/l,ny/l,nz/l];
  for(const p of [a,b,c]){d.p.push(p[0],p[1],p[2]);d.n.push(n[0],n[1],n[2]);}
}
function sphere():{p:number[];n:number[]}{
  const d={p:[],n:[]};const la=7,lo=10;
  for(let y=0;y<la;y++)for(let x=0;x<lo;x++){
    const p0=y/la*Math.PI,p1=(y+1)/la*Math.PI,u0=x/lo*Math.PI*2,u1=(x+1)/lo*Math.PI*2;
    const v=(p:number,u:number)=>[Math.sin(p)*Math.cos(u),Math.cos(p),Math.sin(p)*Math.sin(u)];
    const a=v(p0,u0),b=v(p1,u0),c=v(p1,u1),e=v(p0,u1);
    tri(d,a,b,c);tri(d,a,c,e);
  } return d;
}
function cyl():{p:number[];n:number[]}{
  const d={p:[],n:[]};const sides=8;
  for(let i=0;i<sides;i++){
    const a=i/sides*Math.PI*2,b=(i+1)/sides*Math.PI*2;
    const p=[Math.cos(a),-0.5,Math.sin(a)],q=[Math.cos(b),-0.5,Math.sin(b)],r=[Math.cos(a),0.5,Math.sin(a)],s=[Math.cos(b),0.5,Math.sin(b)];
    tri(d,p,r,s);tri(d,p,s,q);tri(d,[0,-0.5,0],q,p);tri(d,[0,0.5,0],r,s);
  }return d;
}
function cone():{p:number[];n:number[]}{
  const d={p:[],n:[]};const sides=8;
  for(let i=0;i<sides;i++){const a=i/sides*Math.PI*2,b=(i+1)/sides*Math.PI*2,p=[Math.cos(a),-0.5,Math.sin(a)],q=[Math.cos(b),-0.5,Math.sin(b)];tri(d,p,[0,0.7,0],q);tri(d,[0,-0.5,0],q,p);}return d;
}
function torus():{p:number[];n:number[]}{
  const d={p:[],n:[]};const seg=16,ring=6,R=1,r=0.13;
  for(let i=0;i<seg;i++)for(let j=0;j<ring;j++){
    const u0=i/seg*Math.PI*2,u1=(i+1)/seg*Math.PI*2,v0=j/ring*Math.PI*2,v1=(j+1)/ring*Math.PI*2;
    const p=(u:number,v:number)=>[(R+r*Math.cos(v))*Math.cos(u),r*Math.sin(v),(R+r*Math.cos(v))*Math.sin(u)];
    const a=p(u0,v0),b=p(u1,v0),c=p(u1,v1),e=p(u0,v1);tri(d,a,b,c);tri(d,a,c,e);
  }return d;
}
function makeMesh(gl:GL,d:{p:number[];n:number[]}):Mesh{
  const p=gl.createBuffer(),n=gl.createBuffer();if(!p||!n)throw new Error('buffer');
  gl.bindBuffer(gl.ARRAY_BUFFER,p);gl.bufferData(gl.ARRAY_BUFFER,new Float32Array(d.p),gl.STATIC_DRAW);
  gl.bindBuffer(gl.ARRAY_BUFFER,n);gl.bufferData(gl.ARRAY_BUFFER,new Float32Array(d.n),gl.STATIC_DRAW);
  return {pos:p,normal:n,count:d.p.length/3};
}
function shader(gl:GL,type:number,src:string):WebGLShader{
  const s=gl.createShader(type);if(!s)throw new Error('shader');gl.shaderSource(s,src);gl.compileShader(s);
  if(!gl.getShaderParameter(s,gl.COMPILE_STATUS))throw new Error(gl.getShaderInfoLog(s)||'shader');return s;
}
function program(gl:GL):WebGLProgram{
  const p=gl.createProgram();if(!p)throw new Error('program');
  gl.attachShader(p,shader(gl,gl.VERTEX_SHADER,VS));gl.attachShader(p,shader(gl,gl.FRAGMENT_SHADER,FS));gl.linkProgram(p);
  if(!gl.getProgramParameter(p,gl.LINK_STATUS))throw new Error(gl.getProgramInfoLog(p)||'link');return p;
}
function clamp(v:number,a:number,b:number):number{return Math.max(a,Math.min(b,v));}

export interface Realtime3DRenderer{
  render(state:GameState,w:number,h:number):void;
  dispose():void;
}

export function createRealtime3DRenderer(canvas:HTMLCanvasElement):Realtime3DRenderer|null{
  const gl=canvas.getContext('webgl',{alpha:true,antialias:true,depth:true});
  if(!gl)return null;
  try{
    const p=program(gl);
    const loc={
      pos:gl.getAttribLocation(p,'aPosition'),
      normal:gl.getAttribLocation(p,'aNormal'),
      model:gl.getUniformLocation(p,'uModel'),
      viewport:gl.getUniformLocation(p,'uViewport'),
      color:gl.getUniformLocation(p,'uColor'),
      light:gl.getUniformLocation(p,'uLight'),
      glow:gl.getUniformLocation(p,'uGlow'),
      alpha:gl.getUniformLocation(p,'uAlpha'),
    };
    const m={sphere:makeMesh(gl,sphere()),cyl:makeMesh(gl,cyl()),cone:makeMesh(gl,cone()),torus:makeMesh(gl,torus())};
    gl.useProgram(p);gl.enable(gl.DEPTH_TEST);gl.enable(gl.BLEND);gl.blendFunc(gl.SRC_ALPHA,gl.ONE_MINUS_SRC_ALPHA);gl.disable(gl.CULL_FACE);gl.clearColor(0,0,0,0);

    const draw=(mesh:Mesh,mat:M4,color:string,glow=0,alpha=1)=>{
      const c=rgb(color);gl.useProgram(p);gl.bindBuffer(gl.ARRAY_BUFFER,mesh.pos);gl.enableVertexAttribArray(loc.pos);gl.vertexAttribPointer(loc.pos,3,gl.FLOAT,false,0,0);
      gl.bindBuffer(gl.ARRAY_BUFFER,mesh.normal);gl.enableVertexAttribArray(loc.normal);gl.vertexAttribPointer(loc.normal,3,gl.FLOAT,false,0,0);
      gl.uniformMatrix4fv(loc.model,false,mat);gl.uniform2f(loc.viewport,canvas.width,canvas.height);gl.uniform3f(loc.color,c[0],c[1],c[2]);gl.uniform3f(loc.light,-0.45,0.9,0.35);gl.uniform1f(loc.glow,glow);gl.uniform1f(loc.alpha,alpha);
      gl.drawArrays(gl.TRIANGLES,0,mesh.count);
    };
    const core=(x:number,z:number,y:number,r:number,color:string)=>{
      draw(m.sphere,model(x,y,z,0,0,0,r,r*.8,r),color,1.2,.96);
      draw(m.sphere,model(x,y+r*.12,z,0,0,0,r*.38,r*.32,r*.38),'#e8fbff',.8,.95);
    };
    const ring=(x:number,z:number,y:number,r:number,color:string,rot:number)=>{
      draw(m.torus,model(x,y,z,.25,rot,0,r,r*.08,r),color,.65,.7);
    };

    const segment=(x:number,z:number,y:number,len:number,width:number,angle:number,color:string,glow=0.15)=>{
      draw(m.cyl,model(x,y,z,Math.PI/2,angle,0,width,len,width),color,glow,.94);
    };
    const insect=(x:number,z:number,y:number,r:number,color:string,t:number,legs:number,scale=1)=>{
      const phase=t*2.8+x*.006+z*.004;
      const body='#07111e';
      const shell='#102338';
      const legColor='#203950';
      const wobble=Math.sin(phase)*r*.035;
      draw(m.sphere,model(x,y+r*.42+wobble,z,0,phase*.12,0,r*.92*scale,r*.46*scale,r*1.08*scale),body,.18,.98);
      draw(m.sphere,model(x-r*.22*scale,y+r*.72+wobble,z+r*.08*scale,0,phase*.18,0,r*.58*scale,r*.40*scale,r*.62*scale),shell,.2,.98);
      draw(m.sphere,model(x+r*.42*scale,y+r*.77+wobble,z,0,phase*.22,0,r*.34*scale,r*.30*scale,r*.40*scale),body,.22,.98);
      core(x+r*.63*scale,z,r*.79*scale,r*.15*scale,color);
      for(let i=0;i<legs;i++){
        const a=(i/legs)*Math.PI*2+0.12;
        const side=i%2===0?1:-1;
        const gait=Math.sin(phase*1.7+a*2)*0.12*side;
        const a1=a+gait;
        const a2=a1+(i%2===0?.20:-.20);
        const p1x=x+Math.cos(a1)*r*.56*scale;
        const p1z=z+Math.sin(a1)*r*.56*scale;
        const p2x=x+Math.cos(a1)*r*.94*scale;
        const p2z=z+Math.sin(a1)*r*.94*scale;
        const p3x=x+Math.cos(a2)*r*1.30*scale;
        const p3z=z+Math.sin(a2)*r*1.30*scale;
        segment(p1x,p1z,y+r*.30,p2x-p1x>0?Math.hypot(p2x-p1x,p2z-p1z):Math.hypot(p2x-p1x,p2z-p1z),r*.075*scale,Math.atan2(p2z-p1z,p2x-p1x),legColor,.1);
        segment(p3x*.5+p2x*.5,p3z*.5+p2z*.5,y+r*.23,Math.hypot(p3x-p2x,p3z-p2z),r*.055*scale,Math.atan2(p3z-p2z,p3x-p2x),legColor,.08);
        draw(m.sphere,model(p2x,y+r*.28,p2z,0,0,0,r*.09*scale,r*.07*scale,r*.09*scale),color,.22,.9);
      }
      // Antennae and articulated mandibles make the silhouette read as an insect rather than primitives.
      for(const side of [-1,1]){
        const a=phase*.1+side*.38;
        segment(x+Math.cos(a)*r*.66*scale,z+Math.sin(a)*r*.66*scale,y+r*.83,r*.34*scale,r*.07*scale,a,legColor,.15);
        const jawA=phase*.08+side*.42;
        segment(x+Math.cos(jawA)*r*.73*scale,z+Math.sin(jawA)*r*.73*scale,y+r*.58,r*.25*scale,r*.065*scale,jawA,color,.24);
        draw(m.cone,model(x+Math.cos(jawA)*r*.92*scale,y+r*.55,z+Math.sin(jawA)*r*.92*scale,Math.PI/2,jawA,0,r*.07*scale,r*.22*scale,r*.07*scale),color,.4,.92);
      }
      // Small dorsal plates give the abdomen a segmented mechanical shell.
      for(let i=0;i<3;i++){
        const q=i/3;
        draw(m.torus,model(x-r*.18*scale*q,y+r*(.28+.16*q),z,.18,phase*.12,0,r*(.56-.08*i)*scale,r*.045*scale,r*(.56-.08*i)*scale),shell,.16,.7);
      }
    };
    const bossSpider=(x:number,z:number,r:number,color:string,t:number,bossType:string)=>{
      const pulse=1+Math.sin(t*2.4)*.035;
      draw(m.sphere,model(x,r*.52,z,0,t*.09,0,r*1.18*pulse,r*.58*pulse,r*1.32*pulse),'#050a13',.28,.99);
      draw(m.sphere,model(x-r*.12,r*.92,z+r*.10,0,t*.16,0,r*.70,r*.42,r*.76),'#14283b',.25,.98);
      core(x,z,r*.98,r*.31,color);
      core(x-r*.05,z+r*.01,r*1.01,r*.12,'#e8fbff');
      ring(x,z,r*.92,r*1.28,color,t*.16);
      for(let i=0;i<8;i++){
        const a=i/8*Math.PI*2+t*.12;
        const midR=r*1.02;
        const endR=r*(1.55+(i%2)*.16);
        const mx=x+Math.cos(a)*midR,mz=z+Math.sin(a)*midR;
        const ex=x+Math.cos(a+(i%2?-.14:.14))*endR,ez=z+Math.sin(a+(i%2?-.14:.14))*endR;
        segment((x+mx)*.5,(z+mz)*.5,r*.34,Math.hypot(mx-x,mz-z),r*.095,a,'#28445e',.2);
        segment((mx+ex)*.5,(mz+ez)*.5,r*.25,Math.hypot(ex-mx,ez-mz),r*.065,Math.atan2(ez-mz,ex-mx),'#1b3148',.15);
        draw(m.sphere,model(ex,r*.22,ez,0,0,0,r*.10,r*.07,r*.10),color,.35,.92);
      }
      if(bossType==='charger'){
        for(const side of [-1,1]){
          const a=side*.72;
          draw(m.cone,model(x+Math.cos(a)*r*.78,r*1.02,z+Math.sin(a)*r*.78,Math.PI/2,a,0,r*.15,r*.72,r*.15),color,.8,.94);
        }
      }else if(bossType==='shooter'){
        for(let i=0;i<3;i++){
          const a=t*.25+i*Math.PI*2/3;
          draw(m.cyl,model(x+Math.cos(a)*r*.68,r*.82,z+Math.sin(a)*r*.68,0,a,0,r*.13,r*.38,r*.13),color,.65,.94);
        }
      }else if(bossType==='summoner'){
        for(let i=0;i<4;i++){
          const a=i*Math.PI/2+t*.18;
          core(x+Math.cos(a)*r*1.05,z+Math.sin(a)*r*1.05,r*.82,r*.13,color);
        }
      }else{
        ring(x,z,r*1.12,r*1.65,'#b06dff',-t*.24);
        ring(x,z,r*1.28,r*1.9,color,t*.18);
      }
    };
    const sphereDraw=(s:GameState,e:SphereEntity,t:number)=>{
      const x=e.pos.x-s.camera.x,z=e.pos.y-s.camera.y,def=SPHERE_TYPES[e.type];
      const r=clamp(17+e.radius*.09+e.visualTier*1.8,18,30);
      const spin=e.rotation+t*.15;
      const pulse=1+Math.sin(t*3.2+e.rotation)*.035;
      // Floating industrial pedestal and armored core.
      draw(m.cyl,model(x,2.4,z,0,spin*.35,0,r*.82,r*.16,r*.82),'#172b3d',.12,.96);
      draw(m.torus,model(x,4.1,z,.12,spin*.7,0,r*.78,r*.055,r*.78),def.color,.55,.68);
      // The sphere is an armored reactor, not a plain ball: layered shell, cage ribs and a floating core.
      draw(m.sphere,model(x,r*.63,z,0,spin,0,r*.86*pulse,r*.58,r*.86*pulse),'#071321',.28,.99);
      draw(m.sphere,model(x,r*.82,z,0,-spin*.55,0,r*.55,r*.40,r*.55),'#13283c',.16,.98);
      draw(m.torus,model(x,r*.69,z,.18,spin,0,r*.74,r*.07,r*.74),'#2a4660',.18,.76);
      draw(m.torus,model(x,r*.70,z,Math.PI/2,spin*.55,0,r*.72,r*.065,r*.72),def.color,.42,.70);
      // Four armored petals clamp the reactor and create a recognisable silhouette from above.
      for(let i=0;i<4;i++){
        const a=spin+i*Math.PI/2;
        const px=x+Math.cos(a)*r*.66, pz=z+Math.sin(a)*r*.66;
        draw(m.cone,model(px,r*.70,pz,Math.PI/2,a,0,r*.18,r*.42,r*.12),'#1d354b',.14,.94);
        draw(m.sphere,model(px,r*.83,pz,0,a,0,r*.10,r*.15,r*.10),def.color,.72,.94);
      }
      // A suspended reactor core sits slightly above the shell.
      core(x,z,r*.98,r*.34,def.color);
      draw(m.sphere,model(x,r*1.02,z,0,spin*1.4,0,r*.16,r*.12,r*.16),'#e9fbff',1.1,.98);
      // Thin energy cage: three tilted rings make the orb read as a contained power source.
      ring(x,z,r*.92,r*1.02,def.color,spin*.55);
      ring(x,z,r*.98,r*1.18,'#8fb9ff',-spin*.72);
      ring(x,z,r*1.04,r*1.34,def.color,spin*.34);

      if(e.type==='sniper'){
        // Long precision rail.
        segment(x+Math.cos(spin)*r*.62,z+Math.sin(spin)*r*.62,r*.72,r*1.45,r*.10,spin,'#5a6f86',.22);
        segment(x+Math.cos(spin)*r*1.38,z+Math.sin(spin)*r*1.38,r*.72,r*.62,r*.055,spin,def.color,.8);
        draw(m.cone,model(x+Math.cos(spin)*r*1.7,r*.72,z+Math.sin(spin)*r*1.7,Math.PI/2,spin,0,r*.14,r*.32,r*.14),def.color,1,.95);
      }else if(e.type==='shotgun'){
        for(const offset of [-.18,0,.18]){
          const a=spin+offset;
          segment(x+Math.cos(a)*r*.62,z+Math.sin(a)*r*.62,r*.68,r*.82,r*.085,a,'#4b5d70',.18);
          draw(m.cone,model(x+Math.cos(a)*r*1.03,r*.70,z+Math.sin(a)*r*1.03,Math.PI/2,a,0,r*.11,r*.22,r*.11),def.color,.65,.95);
        }
      }else if(e.type==='chain'){
        for(let i=0;i<6;i++){
          const a=spin+i*Math.PI/3;
          const rr=r*(.78+.12*(i%2));
          draw(m.torus,model(x+Math.cos(a)*rr*.45,r*.73,z+Math.sin(a)*rr*.45,.25,a,0,r*.22,r*.045,r*.22),def.color,.5,.72);
        }
        ring(x,z,r*.98,r*1.34,def.color,-spin*1.4);
      }else if(e.type==='aura'){
        ring(x,z,r*.86,r*1.05,def.color,spin);
        ring(x,z,r*1.05,r*1.36,def.color,-spin*.8);
        draw(m.sphere,model(x,r*1.18,z,0,-spin,0,r*.22,r*.42,r*.22),'#eaffff',1.15,.78);
      }else{
        // Standard sphere: four rotating emitters.
        for(let i=0;i<4;i++){
          const a=spin+i*Math.PI/2;
          draw(m.cyl,model(x+Math.cos(a)*r*.83,r*.80,z+Math.sin(a)*r*.83,0,a,0,r*.075,r*.34,r*.075),'#35516a',.18,.94);
          draw(m.sphere,model(x+Math.cos(a)*r*1.08,r*.80,z+Math.sin(a)*r*1.08,0,0,0,r*.10,r*.10,r*.10),def.color,.65,.92);
        }
      }

      if(e.visualTier>1){
        // Higher tiers grow additional armor and a brighter containment ring.
        ring(x,z,r*1.13,r*1.06,'#f6d477',-spin*.7);
        draw(m.torus,model(x,r*1.10,z,Math.PI/2,spin*.3,0,r*1.08,r*.045,r*1.08),'#f6d477',.55,.66);
        for(let i=0;i<4;i++){
          const a=i*Math.PI/2-spin*.4;
          draw(m.cone,model(x+Math.cos(a)*r*1.12,r*.98,z+Math.sin(a)*r*1.12,Math.PI/2,a,0,r*.09,r*.38,r*.09),'#f6d477',.75,.9);
        }
      }
      if(e.visualTier>2){
        ring(x,z,r*1.34,r*1.28,'#e8fbff',spin*.55);
        for(let i=0;i<6;i++){
          const a=i*Math.PI/3+spin*.2;
          draw(m.sphere,model(x+Math.cos(a)*r*1.32,r*1.02,z+Math.sin(a)*r*1.32,0,spin,0,r*.075,r*.075,r*.075),def.color,.9,.95);
        }
      }
    };

    const enemyDraw=(e:EnemyEntity,t:number)=>{
      const x=e.pos.x-stateCameraX,z=e.pos.y-stateCameraY,r=Math.max(11,e.radius),f=e.rotation;
      if(e.isBoss){
        bossSpider(x,z,r,e.color,t,e.bossType);
        return;
      }
      const bob=Math.sin(t*4+x*.01+z*.007)*r*.035;
      if(e.type==='fast'){
        insect(x,z,r*.04+bob,r*.82,e.color,t,4,0.82);
        // Transparent insectoid wings with a hard leading edge.
        for(const side of [-1,1]){
          const a=f+side*.68+Math.sin(t*7+side)*.05;
          draw(m.sphere,model(x+Math.cos(a)*r*.78,r*.96+bob,z+Math.sin(a)*r*.78,0,a,0,r*.82,r*.035,r*.30),e.color,.38,.30);
          draw(m.cyl,model(x+Math.cos(a)*r*.72,r*1.01+bob,z+Math.sin(a)*r*.72,0,a,0,r*.035,r*.78,r*.035),'#8ca8c2',.18,.72);
        }
      }else if(e.type==='tank'){
        insect(x,z,r*.03+bob,r*1.12,e.color,t,6,1.15);
        draw(m.torus,model(x,r*.76+bob,z,.15,f,0,r*.82,r*.10,r*.82),e.color,.5,.72);
        for(let i=0;i<6;i++){
          const a=i/6*Math.PI*2+f;
          draw(m.cone,model(x+Math.cos(a)*r*.78,r*.94+bob,z+Math.sin(a)*r*.78,Math.PI/2,a,0,r*.12,r*.55,r*.12),e.color,.55,.9);
        }
      }else{
        insect(x,z,r*.02+bob,r*.98,e.color,t,6,1);
      }
    };
    let stateCameraX=0,stateCameraY=0;
    return {
      render(state,w,h){
        canvas.width=w;canvas.height=h;gl.viewport(0,0,w,h);gl.clear(gl.COLOR_BUFFER_BIT|gl.DEPTH_BUFFER_BIT);stateCameraX=state.camera.x;stateCameraY=state.camera.y;
        const t=state.time;
        for(const s of state.spheres)if(s.alive)sphereDraw(state,s,t);
        for(const p0 of state.sphereProjectiles)if(p0.alive){
          const x=p0.pos.x-state.camera.x,z=p0.pos.y-state.camera.y,a=Math.atan2(p0.vel.y,p0.vel.x),r=clamp(p0.radius*1.6,4,8);
          draw(m.cone,model(x,3,z,Math.PI/2,a,0,r*.8,r*1.8,r*.8),'#25394b',.2,.9);core(x,z,5,r*.65,p0.color);
        }
        for(const m0 of state.minions){
          const mx=m0.pos.x-state.camera.x,mz=m0.pos.y-state.camera.y,mr=Math.max(7,m0.radius);
          insect(mx,mz,mr*.02,mr,'#ffb84d',t+m0.rotation,4,.62);
        }
        for(const e of state.enemies)if(e.hp>0)enemyDraw(e,t);
        const p0=state.player,x=p0.pos.x-state.camera.x,z=p0.pos.y-state.camera.y,bob=Math.sin(t*3)*1.5,pr=PLAYER_RADIUS;
        // Central ECHO core: layered reactor, armored petals, radial energy cage and floating nodes.
        draw(m.cyl,model(x,pr*.16+bob,z,0,t*.2,0,pr*.94,pr*.16,pr*.94),'#182c40',.18,.96);
        draw(m.torus,model(x,pr*.30+bob,z,.14,t*.18,0,pr*.86,pr*.07,pr*.86),'#29465f',.22,.72);
        draw(m.sphere,model(x,pr*.50+bob,z,0,t*.08,0,pr*.78,pr*.5,pr*.78),'#07101a',.55,.99);
        draw(m.sphere,model(x,pr*.70+bob,z,0,-t*.12,0,pr*.50,pr*.32,pr*.50),'#10263a',.22,.98);
        // Four overlapping armor petals give the player a mechanical silhouette around the luminous center.
        for(let i=0;i<4;i++){
          const a=t*.20+i*Math.PI/2;
          const px=x+Math.cos(a)*pr*.66,pz=z+Math.sin(a)*pr*.66;
          draw(m.cone,model(px,pr*.58+bob,pz,Math.PI/2,a,0,pr*.13,pr*.42,pr*.10),'#1d3a53',.22,.96);
          draw(m.sphere,model(px,pr*.79+bob,pz,0,a,0,pr*.105,pr*.13,pr*.105),'#63e6ff',.8,.94);
        }
        core(x,z,pr*.92+bob,pr*.36,'#63e6ff');
        draw(m.sphere,model(x,pr*1.02+bob,z,0,t*1.4,0,pr*.17,pr*.13,pr*.17),'#eaffff',1.3,.99);
        // Three independent orbital rings rotate at different speeds and tilt, producing the reference-like energy cage.
        ring(x,z,pr*.88+bob,pr*1.20,'#63e6ff',t*.50);
        ring(x,z,pr*1.02+bob,pr*1.42,'#b06dff',-t*.35);
        ring(x,z,pr*1.10+bob,pr*1.58,'#7deaff',t*.18);
        // Six bright anchor nodes travel around the outer cage.
        for(let i=0;i<6;i++){
          const a=t*.28+i*Math.PI/3;
          const nr=pr*1.38;
          draw(m.sphere,model(x+Math.cos(a)*nr,pr*.92+bob,z+Math.sin(a)*nr,0,0,0,pr*.065,pr*.065,pr*.065),'#dffcff',1.0,.96);
        }
        // Four radial energy beams connect the reactor to the cage.
        for(let i=0;i<4;i++){
          const a=t*.16+i*Math.PI/2;
          segment(x+Math.cos(a)*pr*.76,z+Math.sin(a)*pr*.76,pr*.91+bob,pr*.50,pr*.035,a,'#63e6ff',.75); 
        }
      },
      dispose(){gl.getExtension('WEBGL_lose_context')?.loseContext();}
    };
  }catch{return null;}
}