/* eslint-disable @typescript-eslint/no-explicit-any */
import type { GameState, SphereEntity, SphereProjectile, EnemyEntity, Particle, LightningBolt, MinionEntity } from './engine';
import type { SphereType } from './gameData';

type V3 = { x: number; y: number; z: number };
type Mat4 = Float32Array;

const DEG = Math.PI / 180;
const ASSET_BASE = '/art3d/';

function identity(): Mat4 {
  return new Float32Array([1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1]);
}
function mul(a: Mat4, b: Mat4): Mat4 {
  const o = new Float32Array(16);
  for (let c = 0; c < 4; c++) for (let r = 0; r < 4; r++) {
    o[c * 4 + r] = a[r] * b[c * 4] + a[4 + r] * b[c * 4 + 1] + a[8 + r] * b[c * 4 + 2] + a[12 + r] * b[c * 4 + 3];
  }
  return o;
}
function tr(x: number, y: number, z: number): Mat4 {
  const m = identity(); m[12] = x; m[13] = y; m[14] = z; return m;
}
function sc(x: number, y: number, z: number): Mat4 {
  const m = identity(); m[0] = x; m[5] = y; m[10] = z; return m;
}
function ry(a: number): Mat4 {
  const c = Math.cos(a), s = Math.sin(a);
  return new Float32Array([c, 0, -s, 0, 0, 1, 0, 0, s, 0, c, 0, 0, 0, 0, 1]);
}
function persp(fov: number, aspect: number, near: number, far: number): Mat4 {
  const f = 1 / Math.tan(fov / 2), nf = 1 / (near - far);
  return new Float32Array([f / aspect, 0, 0, 0, 0, f, 0, 0, 0, 0, (far + near) * nf, -1, 0, 0, 2 * far * near * nf, 0]);
}
function lookAt(e: V3, c: V3, u: V3): Mat4 {
  let zx = e.x - c.x, zy = e.y - c.y, zz = e.z - c.z;
  let l = Math.hypot(zx, zy, zz) || 1; zx /= l; zy /= l; zz /= l;
  let xx = u.y * zz - u.z * zy, xy = u.z * zx - u.x * zz, xz = u.x * zy - u.y * zx;
  l = Math.hypot(xx, xy, xz) || 1; xx /= l; xy /= l; xz /= l;
  const yx = zy * xz - zz * xy, yy = zz * xx - zx * xz, yz = zx * xy - zy * xx;
  return new Float32Array([
    xx, yx, zx, 0, xy, yy, zy, 0, xz, yz, zz, 0,
    -(xx * e.x + xy * e.y + xz * e.z),
    -(yx * e.x + yy * e.y + yz * e.z),
    -(zx * e.x + zy * e.y + zz * e.z), 1,
  ]);
}
function qmat(q: number[]): Mat4 {
  const [x, y, z, w] = q;
  return new Float32Array([
    1 - 2 * y * y - 2 * z * z, 2 * x * y + 2 * w * z, 2 * x * z - 2 * w * y, 0,
    2 * x * y - 2 * w * z, 1 - 2 * x * x - 2 * z * z, 2 * y * z + 2 * w * x, 0,
    2 * x * z + 2 * w * y, 2 * y * z - 2 * w * x, 1 - 2 * x * x - 2 * y * y, 0,
    0, 0, 0, 1,
  ]);
}
function trs(t: number[] | undefined, r: number[] | undefined, s: number[] | undefined): Mat4 {
  let m = identity();
  if (t) m = mul(m, tr(t[0], t[1], t[2]));
  if (r) m = mul(m, qmat(r));
  if (s) m = mul(m, sc(s[0], s[1], s[2]));
  return m;
}

interface GLBImage {
  bytes: Uint8Array;
  mime: string;
}
interface GLBPrimitive {
  position: Float32Array;
  normal: Float32Array;
  uv: Float32Array | null;
  tangent: Float32Array | null;
  indices: Uint8Array | Uint16Array | Uint32Array;
  color: [number, number, number];
  alpha: number;
  emissive: [number, number, number];
  metallic: number;
  roughness: number;
  doubleSided: boolean;
  baseImage: number | null;
  mrImage: number | null;
  normalImage: number | null;
  emissiveImage: number | null;
  alphaMode: 'OPAQUE' | 'MASK' | 'BLEND';
  alphaCutoff: number;
}
interface GLBNode {
  name: string;
  local: Mat4;
  children: number[];
  mesh: number | null;
}
interface GLBAsset {
  nodes: GLBNode[];
  meshes: GLBPrimitive[][];
  roots: number[];
  images: GLBImage[];
}

function componentCount(type: string): number {
  return type === 'SCALAR' ? 1 : type === 'VEC2' ? 2 : type === 'VEC3' ? 3 : type === 'VEC4' ? 4 : 1;
}

function tangentFromMesh(position: Float32Array, normal: Float32Array, uv: Float32Array | null, indices: Uint8Array | Uint16Array | Uint32Array): Float32Array | null {
  if (!uv) return null;
  const tan1 = new Float32Array(position.length);
  const tan2 = new Float32Array(position.length);
  const readIndex = (i: number) => Number(indices[i]);
  for (let i = 0; i + 2 < indices.length; i += 3) {
    const ia = readIndex(i), ib = readIndex(i + 1), ic = readIndex(i + 2);
    const ax = position[ia * 3], ay = position[ia * 3 + 1], az = position[ia * 3 + 2];
    const bx = position[ib * 3], by = position[ib * 3 + 1], bz = position[ib * 3 + 2];
    const cx = position[ic * 3], cy = position[ic * 3 + 1], cz = position[ic * 3 + 2];
    const au = uv[ia * 2], av = uv[ia * 2 + 1];
    const bu = uv[ib * 2], bv = uv[ib * 2 + 1];
    const cu = uv[ic * 2], cv = uv[ic * 2 + 1];
    const x1 = bx - ax, x2 = cx - ax, y1 = by - ay, y2 = cy - ay, z1 = bz - az, z2 = cz - az;
    const s1 = bu - au, s2 = cu - au, t1 = bv - av, t2 = cv - av;
    const d = s1 * t2 - s2 * t1;
    if (Math.abs(d) < 1e-7) continue;
    const r = 1 / d;
    const sx = (t2 * x1 - t1 * x2) * r, sy = (t2 * y1 - t1 * y2) * r, sz = (t2 * z1 - t1 * z2) * r;
    const tx = (s1 * x2 - s2 * x1) * r, ty = (s1 * y2 - s2 * y1) * r, tz = (s1 * z2 - s2 * z1) * r;
    for (const idx of [ia, ib, ic]) {
      tan1[idx * 3] += sx; tan1[idx * 3 + 1] += sy; tan1[idx * 3 + 2] += sz;
      tan2[idx * 3] += tx; tan2[idx * 3 + 1] += ty; tan2[idx * 3 + 2] += tz;
    }
  }
  const out = new Float32Array((position.length / 3) * 4);
  for (let i = 0; i < position.length / 3; i++) {
    const nx = normal[i * 3], ny = normal[i * 3 + 1], nz = normal[i * 3 + 2];
    let tx = tan1[i * 3], ty = tan1[i * 3 + 1], tz = tan1[i * 3 + 2];
    const ndt = nx * tx + ny * ty + nz * tz;
    tx -= nx * ndt; ty -= ny * ndt; tz -= nz * ndt;
    const len = Math.hypot(tx, ty, tz) || 1;
    tx /= len; ty /= len; tz /= len;
    const bx = ny * tz - nz * ty, by = nz * tx - nx * tz, bz = nx * ty - ny * tx;
    const handed = bx * tan2[i * 3] + by * tan2[i * 3 + 1] + bz * tan2[i * 3 + 2] < 0 ? -1 : 1;
    out[i * 4] = tx; out[i * 4 + 1] = ty; out[i * 4 + 2] = tz; out[i * 4 + 3] = handed;
  }
  return out;
}

export class GLTFLoader {
  async load(url: string): Promise<GLBAsset> {
    const r = await fetch(url);
    if (!r.ok) throw new Error(`GLB ${url}: ${r.status}`);
    return this.parse(await r.arrayBuffer(), url);
  }

  private async parse(buf: ArrayBuffer, baseUrl: string): Promise<GLBAsset> {
    const dv = new DataView(buf);
    if (dv.getUint32(0, true) !== 0x46546c67) throw new Error('Not a GLB');
    let off = 12;
    let json: any = null;
    let bin = new Uint8Array();
    while (off < buf.byteLength) {
      const len = dv.getUint32(off, true);
      const type = dv.getUint32(off + 4, true);
      const data = buf.slice(off + 8, off + 8 + len);
      off += 8 + len;
      if (type === 0x4e4f534a) json = JSON.parse(new TextDecoder().decode(data));
      else if (type === 0x004e4942) bin = new Uint8Array(data);
    }
    if (!json) throw new Error('GLB JSON missing');

    const read = (acc: any): any => {
      const bv = json.bufferViews[acc.bufferView];
      const base = (bv.byteOffset || 0) + (acc.byteOffset || 0);
      const count = acc.count;
      const comps = componentCount(acc.type);
      const ct = acc.componentType;
      const C = ct === 5126 ? Float32Array : ct === 5125 ? Uint32Array : ct === 5123 ? Uint16Array : Uint8Array;
      const bytesPer = C.BYTES_PER_ELEMENT;
      const stride = bv.byteStride || bytesPer * comps;
      const out = new C(count * comps);
      for (let i = 0; i < count; i++) {
        for (let j = 0; j < comps; j++) {
          const pos = base + i * stride + j * bytesPer;
          out[i * comps + j] = new C(bin.buffer, bin.byteOffset + pos, 1)[0];
        }
      }
      return out;
    };

    const decodeDataUri = (uri: string): Uint8Array => {
      const comma = uri.indexOf(',');
      if (comma < 0) throw new Error('Invalid data URI image');
      const meta = uri.slice(0, comma);
      const payload = uri.slice(comma + 1);
      if (/;base64/i.test(meta)) {
        const binary = atob(payload);
        const out = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) out[i] = binary.charCodeAt(i);
        return out;
      }
      return new TextEncoder().encode(decodeURIComponent(payload));
    };

    const images: GLBImage[] = await Promise.all((json.images || []).map(async (img: any) => {
      if (img.bufferView !== undefined) {
        const bv = json.bufferViews[img.bufferView];
        const start = bv.byteOffset || 0;
        return { bytes: bin.slice(start, start + bv.byteLength), mime: img.mimeType || 'image/png' };
      }
      if (typeof img.uri === 'string') {
        const bytes = img.uri.startsWith('data:')
          ? decodeDataUri(img.uri)
          : new Uint8Array(await (await fetch(new URL(img.uri, baseUrl))).arrayBuffer());
        return { bytes, mime: img.mimeType || 'image/png' };
      }
      return { bytes: new Uint8Array(), mime: img.mimeType || 'image/png' };
    }));

    const textureImage = (textureIndex: number | undefined): number | null => {
      if (textureIndex === undefined) return null;
      const tex = json.textures?.[textureIndex];
      return tex?.source === undefined ? null : Number(tex.source);
    };

    const materials = (json.materials || []).map((m: any) => {
      const p = m.pbrMetallicRoughness || {};
      return {
        c: (p.baseColorFactor || [1, 1, 1, 1]).slice(0, 3),
        alpha: Number((p.baseColorFactor || [1, 1, 1, 1])[3] ?? 1),
        e: m.emissiveFactor || [0, 0, 0],
        metallic: Number(p.metallicFactor ?? 1),
        roughness: Number(p.roughnessFactor ?? 1),
        doubleSided: Boolean(m.doubleSided),
        alphaMode: (m.alphaMode || 'OPAQUE') as 'OPAQUE' | 'MASK' | 'BLEND',
        alphaCutoff: Number(m.alphaCutoff ?? 0.5),
        baseImage: textureImage(p.baseColorTexture?.index),
        mrImage: textureImage(p.metallicRoughnessTexture?.index),
        normalImage: textureImage(m.normalTexture?.index),
        emissiveImage: textureImage(m.emissiveTexture?.index),
      };
    });

    const meshes = (json.meshes || []).map((mesh: any) => (mesh.primitives || []).map((p: any) => {
      const position = read(json.accessors[p.attributes.POSITION]);
      const normal = p.attributes.NORMAL !== undefined ? read(json.accessors[p.attributes.NORMAL]) : new Float32Array(position.length);
      const uv = p.attributes.TEXCOORD_0 !== undefined ? read(json.accessors[p.attributes.TEXCOORD_0]) : null;
      let indices: Uint8Array | Uint16Array | Uint32Array;
      if (p.indices !== undefined) indices = read(json.accessors[p.indices]);
      else {
        indices = new Uint32Array(position.length / 3);
        for (let i = 0; i < indices.length; i++) indices[i] = i;
      }
      const m = materials[p.material || 0] || {
        c: [1, 1, 1], alpha: 1, e: [0, 0, 0], metallic: 0, roughness: 1,
        doubleSided: false, alphaMode: 'OPAQUE' as const, alphaCutoff: 0.5,
        baseImage: null, mrImage: null, normalImage: null, emissiveImage: null,
      };
      return {
        position, normal, uv, tangent: tangentFromMesh(position, normal, uv, indices), indices,
        color: m.c as [number, number, number], alpha: m.alpha, emissive: m.e as [number, number, number],
        metallic: m.metallic, roughness: m.roughness, doubleSided: m.doubleSided,
        alphaMode: m.alphaMode, alphaCutoff: m.alphaCutoff,
        baseImage: m.baseImage, mrImage: m.mrImage, normalImage: m.normalImage, emissiveImage: m.emissiveImage,
      };
    }));

    const nodes = (json.nodes || []).map((n: any) => ({
      name: n.name || 'node',
      local: n.matrix ? new Float32Array(n.matrix) : trs(n.translation, n.rotation, n.scale),
      children: n.children || [],
      mesh: n.mesh === undefined ? null : n.mesh,
    }));
    const child = new Set<number>();
    nodes.forEach((n: GLBNode) => n.children.forEach(i => child.add(i)));
    const roots = nodes.map((_: GLBNode, i: number) => i).filter((i: number) => !child.has(i));
    return { nodes, meshes, roots, images };
  }
}

interface GPUPrim extends GLBPrimitive {
  p: WebGLBuffer;
  n: WebGLBuffer;
  u: WebGLBuffer | null;
  t: WebGLBuffer | null;
  i: WebGLBuffer;
  count: number;
  indexType: number;
  baseTex: WebGLTexture | null;
  mrTex: WebGLTexture | null;
  normalTex: WebGLTexture | null;
  emissiveTex: WebGLTexture | null;
}

interface GPUAsset {
  asset: GLBAsset;
  gpu: GPUPrim[][];
}

const VERT = `
attribute vec3 a_position;
attribute vec3 a_normal;
attribute vec2 a_uv;
attribute vec4 a_tangent;
uniform mat4 u_mvp;
uniform mat4 u_model;
varying vec3 v_n;
varying vec3 v_w;
varying vec2 v_uv;
varying vec4 v_tangent;
void main(){
  vec4 w=u_model*vec4(a_position,1.0);
  v_w=w.xyz;
  v_n=normalize(mat3(u_model)*a_normal);
  v_uv=a_uv;
  v_tangent=a_tangent;
  gl_Position=u_mvp*vec4(a_position,1.0);
}`;

const FRAG = `
#ifdef GL_FRAGMENT_PRECISION_HIGH
precision highp float;
#else
precision mediump float;
#endif
uniform vec3 u_color;
uniform vec3 u_emissive;
uniform vec3 u_camera;
uniform vec3 u_lightColor;
uniform vec3 u_fillColor;
uniform float u_time;
uniform float u_alpha;
uniform float u_glow;
uniform float u_metallic;
uniform float u_roughness;
uniform float u_hasBase;
uniform float u_hasMR;
uniform float u_hasNormal;
uniform float u_hasEmissive;
uniform float u_decodeBase;
uniform float u_decodeEmissive;
uniform float u_alphaMode;
uniform float u_alphaCutoff;
uniform sampler2D u_baseTex;
uniform sampler2D u_mrTex;
uniform sampler2D u_normalTex;
uniform sampler2D u_emissiveTex;
varying vec3 v_n;
varying vec3 v_w;
varying vec2 v_uv;
varying vec4 v_tangent;

vec3 srgbToLinear(vec3 c){ return pow(max(c, vec3(0.0)), vec3(2.2)); }
float sat(float x){ return clamp(x,0.0,1.0); }
vec3 fresnelSchlick(float cosTheta, vec3 F0){ return F0 + (1.0-F0)*pow(1.0-sat(cosTheta),5.0); }

float D_GGX(vec3 N, vec3 H, float r){
  float a=r*r, a2=a*a, nh=sat(dot(N,H)), nh2=nh*nh;
  float d=nh2*(a2-1.0)+1.0;
  return a2/max(3.14159265*d*d,0.0001);
}
float G_Schlick(float nv, float r){
  float k=((r+1.0)*(r+1.0))/8.0;
  return nv/max(nv*(1.0-k)+k,0.0001);
}
float G_Smith(vec3 N, vec3 V, vec3 L, float r){
  return G_Schlick(sat(dot(N,V)),r)*G_Schlick(sat(dot(N,L)),r);
}

void main(){
  vec4 base=vec4(u_color,1.0);
  if(u_hasBase>0.5) base*=texture2D(u_baseTex,v_uv);
  if(u_decodeBase>0.5) base.rgb=srgbToLinear(base.rgb);

  vec3 N=normalize(v_n);
  if(u_hasNormal>0.5 && length(v_tangent.xyz)>0.1){
    vec3 T=normalize(v_tangent.xyz-N*dot(N,v_tangent.xyz));
    vec3 B=normalize(cross(N,T))*v_tangent.w;
    vec3 nm=texture2D(u_normalTex,v_uv).xyz*2.0-1.0;
    N=normalize(mat3(T,B,N)*nm);
  }

  float metallic=clamp(u_metallic,0.0,1.0);
  float roughness=clamp(u_roughness,0.045,1.0);
  if(u_hasMR>0.5){
    vec4 mr=texture2D(u_mrTex,v_uv);
    roughness=clamp(roughness*mr.g,0.045,1.0);
    metallic=clamp(metallic*mr.b,0.0,1.0);
  }

  vec3 V=normalize(u_camera-v_w);
  vec3 L=normalize(vec3(-0.42,0.82,0.40));
  vec3 H=normalize(V+L);
  float nv=sat(dot(N,V)), nl=sat(dot(N,L));
  vec3 F0=mix(vec3(0.04),base.rgb,metallic);
  vec3 F=fresnelSchlick(sat(dot(H,V)),F0);
  float D=D_GGX(N,H,roughness);
  float G=G_Smith(N,V,L,roughness);
  vec3 spec=(D*G*F)/max(4.0*nv*nl,0.001);
  vec3 kS=F;
  vec3 kD=(vec3(1.0)-kS)*(1.0-metallic);
  vec3 diffuse=kD*base.rgb/3.14159265;

  vec3 fillL=normalize(vec3(0.55,0.48,-0.62));
  float nfl=sat(dot(N,fillL));
  vec3 ambient=base.rgb*(0.065+0.10*nv);
  vec3 direct=(diffuse+spec)*(u_lightColor*nl+u_fillColor*nfl);

  float rim=pow(1.0-nv,3.0);
  float pulse=0.96+0.04*sin(u_time*3.2+v_w.y*2.5);
  vec3 emission=u_emissive;
  if(u_hasEmissive>0.5) emission*=texture2D(u_emissiveTex,v_uv).rgb;
  if(u_decodeEmissive>0.5) emission=srgbToLinear(emission);
  emission*=u_glow*(0.28+1.18*rim)*pulse;

  vec3 c=max(ambient+direct+emission,vec3(0.0));
  if(u_alphaMode > 0.5 && u_alphaMode < 1.5 && base.a*u_alpha < u_alphaCutoff) discard;
  gl_FragColor=vec4(c,base.a*u_alpha);
}`


const LINE_V = `
attribute vec3 a_position;
uniform mat4 u_mvp;
void main(){gl_Position=u_mvp*vec4(a_position,1.0);}
`;
const LINE_F = `
precision mediump float;
uniform vec3 u_color;
uniform float u_alpha;
uniform float u_time;
void main(){float p=.82+.18*sin(u_time*8.0+gl_FragCoord.x*.02);gl_FragColor=vec4(u_color*p,u_alpha);}
`;
const POST_V = `
attribute vec2 a_position;
varying vec2 v_uv;
void main(){v_uv=a_position*.5+.5;gl_Position=vec4(a_position,0,1);}
`;
const POST_F = `
#ifdef GL_FRAGMENT_PRECISION_HIGH
precision highp float;
#else
precision mediump float;
#endif
varying vec2 v_uv;
uniform sampler2D u_scene;
uniform vec2 u_texel;
void main(){
  vec3 scene=texture2D(u_scene,v_uv).rgb;
  vec3 bloom=vec3(0.0);
  float weights=0.0;
  for(int i=-4;i<=4;i++){
    float fi=float(i);
    float w=5.0-abs(fi);
    vec2 o=vec2(fi)*u_texel*2.0;
    vec3 tap=texture2D(u_scene,v_uv+o).rgb;
    bloom+=max(tap-vec3(0.72),vec3(0.0))*w;
    weights+=w;
  }
  bloom/=max(weights,1.0);
  float radial=1.0-smoothstep(0.15,0.78,distance(v_uv,vec2(0.5)));
  vec3 bg=vec3(0.004,0.010,0.030)+vec3(0.0,0.018,0.055)*radial;
  vec3 c=max(scene,bg)+bloom*(0.72+0.18*radial);
  c=c/(vec3(1.0)+c);
  c=pow(max(c,vec3(0.0)),vec3(1.0/2.2));
  c*=0.992+0.008*sin(v_uv.y*1100.0);
  gl_FragColor=vec4(c,1.0);
}`


export class Echo3DRenderer {
  private gl: WebGLRenderingContext;
  private program: WebGLProgram;
  private lineProgram: WebGLProgram;
  private postProgram: WebGLProgram;
  private loader = new GLTFLoader();
  private assets = new Map<string, GPUAsset>();
  private loading = new Map<string, Promise<void>>();
  private loadErrors: string[] = [];
  private quad: WebGLBuffer;
  private sceneTex: WebGLTexture;
  private sceneFb: WebGLFramebuffer;
  private depth: WebGLRenderbuffer;
  private width = 1;
  private height = 1;
  private cameraPos: V3 = { x: 0, y: 500, z: 500 };
  private renderStats = { frame: 0, drawCalls: 0, triangles: 0, visibleEntities: 0, players: 0, spheres: 0, enemies: 0 };
  private arenaGridBuffer: WebGLBuffer | null = null;
  private arenaRingBuffer: WebGLBuffer | null = null;
  private arenaGridCount = 0;
  private arenaRingCount = 0;
  private currentGlow = 1.0;

  constructor(private canvas: HTMLCanvasElement) {
    const gl = (canvas.getContext('webgl2', { alpha: false, antialias: true, powerPreference: 'high-performance' })
      || canvas.getContext('webgl', { alpha: false, antialias: true, powerPreference: 'high-performance' })) as WebGLRenderingContext | null;
    if (!gl) throw new Error('WebGL unavailable');
    this.gl = gl;
    this.program = this.make(VERT, FRAG);
    this.lineProgram = this.make(LINE_V, LINE_F);
    this.postProgram = this.make(POST_V, POST_F);
    this.quad = this.buf(new Float32Array([-1, -1, 1, -1, -1, 1, 1, 1]));
    this.sceneTex = gl.createTexture()!;
    this.sceneFb = gl.createFramebuffer()!;
    this.depth = gl.createRenderbuffer()!;
    gl.enable(gl.DEPTH_TEST);
    gl.enable(gl.CULL_FACE);
    gl.enable(gl.BLEND);
    gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
    this.resize();
    void this.preload();
  }

  private make(v: string, f: string): WebGLProgram {
    const g = this.gl;
    const shader = (type: number, src: string) => {
      const s = g.createShader(type)!;
      g.shaderSource(s, src);
      g.compileShader(s);
      if (!g.getShaderParameter(s, g.COMPILE_STATUS)) throw new Error(g.getShaderInfoLog(s) || 'shader');
      return s;
    };
    const p = g.createProgram()!;
    g.attachShader(p, shader(g.VERTEX_SHADER, v));
    g.attachShader(p, shader(g.FRAGMENT_SHADER, f));
    g.linkProgram(p);
    if (!g.getProgramParameter(p, g.LINK_STATUS)) throw new Error(g.getProgramInfoLog(p) || 'program');
    return p;
  }

  private buf(data: BufferSource): WebGLBuffer {
    const b = this.gl.createBuffer()!;
    this.gl.bindBuffer(this.gl.ARRAY_BUFFER, b);
    this.gl.bufferData(this.gl.ARRAY_BUFFER, data, this.gl.STATIC_DRAW);
    return b;
  }

  private async texture(image: GLBImage, colorSpace: 'srgb' | 'linear'): Promise<WebGLTexture> {
    const blob = new Blob([image.bytes], { type: image.mime });
    const bitmap = await createImageBitmap(blob);
    const tex = this.gl.createTexture()!;
    const g = this.gl;
    g.bindTexture(g.TEXTURE_2D, tex);
    g.pixelStorei(g.UNPACK_FLIP_Y_WEBGL, 0);
    const isPOT = (value: number) => value > 0 && (value & (value - 1)) === 0;
    g.texParameteri(g.TEXTURE_2D, g.TEXTURE_MAG_FILTER, g.LINEAR);
    g.texParameteri(g.TEXTURE_2D, g.TEXTURE_WRAP_S, g.REPEAT);
    g.texParameteri(g.TEXTURE_2D, g.TEXTURE_WRAP_T, g.REPEAT);
    const internal = g instanceof WebGL2RenderingContext && colorSpace === 'srgb'
      ? (g as WebGL2RenderingContext).SRGB8_ALPHA8 : g.RGBA;
    g.texImage2D(g.TEXTURE_2D, 0, internal, g.RGBA, g.UNSIGNED_BYTE, bitmap);
    if (isPOT(bitmap.width) && isPOT(bitmap.height)) {
      g.generateMipmap(g.TEXTURE_2D);
      g.texParameteri(g.TEXTURE_2D, g.TEXTURE_MIN_FILTER, g.LINEAR_MIPMAP_LINEAR);
    } else {
      g.texParameteri(g.TEXTURE_2D, g.TEXTURE_MIN_FILTER, g.LINEAR);
    }
    bitmap.close();
    return tex;
  }

  private recordLoadError(name: string, error: unknown) {
    const message = String(error);
    if (!this.loadErrors.some(entry => entry.startsWith(`${name}:`))) {
      this.loadErrors.push(`${name}: ${message}`);
    }
    (window as any).__ECHO3D_LOAD_ERRORS = [...this.loadErrors];
  }

  private async preload() {
    const names = [
      'player_core',
      'player_spherist', 'player_hunter', 'player_engineer', 'player_berserker', 'player_alchemist', 'player_architect',
      'enemy_worker', 'enemy_guard', 'enemy_flyer', 'enemy_spider', 'enemy_slime', 'enemy_psionic', 'enemy_queen',
      'boss_colony', 'boss_distortion', 'boss_singularity',
      'projectile_energy', 'projectile_fire',
      // Load only the first tier of each family at startup. Higher tiers stay
      // as real GLB assets but are streamed on demand after an upgrade, so
      // heavy geodesic cages never stall the first gameplay frame.
      ...['standard', 'sniper', 'shotgun', 'chain', 'aura'].map(t => `sphere_${t}_t1`),
    ];
    await Promise.all(names.map(async name => {
      try { await this.load(name); }
      catch (e) {
        this.recordLoadError(name, e);
        console.warn('[Echo3D]', String(e));
      }
    }));
    (window as any).__ECHO3D_LOAD_ERRORS = [...this.loadErrors];
  }

  private async load(name: string) {
    if (this.assets.has(name)) return;
    if (this.loading.has(name)) return this.loading.get(name)!;

    const p = this.loader.load(`${ASSET_BASE}${name}.glb`).then(async asset => {
      const textureCache = new Map<string, WebGLTexture>();
      const getTex = async (index: number | null, role: 'srgb' | 'linear') => {
        if (index === null) return null;
        const key = `${index}:${role}`;
        const cached = textureCache.get(key);
        if (cached) return cached;
        const image = asset.images[index];
        if (!image || image.bytes.length === 0) return null;
        const tex = await this.texture(image, role);
        textureCache.set(key, tex);
        return tex;
      };

      const gpu = await Promise.all(asset.meshes.map(ms => Promise.all(ms.map(async m => {
        const p = this.buf(m.position);
        const n = this.buf(m.normal);
        const u = m.uv ? this.buf(m.uv) : null;
        const t = m.tangent ? this.buf(m.tangent) : null;
        const i = this.gl.createBuffer()!;
        this.gl.bindBuffer(this.gl.ELEMENT_ARRAY_BUFFER, i);
        this.gl.bufferData(this.gl.ELEMENT_ARRAY_BUFFER, m.indices, this.gl.STATIC_DRAW);
        const indexType = m.indices instanceof Uint32Array ? this.gl.UNSIGNED_INT :
          m.indices instanceof Uint16Array ? this.gl.UNSIGNED_SHORT : this.gl.UNSIGNED_BYTE;
        return {
          ...m, p, n, u, t, i, count: m.indices.length, indexType,
          baseTex: await getTex(m.baseImage, 'srgb'),
          mrTex: await getTex(m.mrImage, 'linear'),
          normalTex: await getTex(m.normalImage, 'linear'),
          emissiveTex: await getTex(m.emissiveImage, 'srgb'),
        };
      }))));
      this.assets.set(name, { asset, gpu });
    });

    this.loading.set(name, p);
    p.catch(error => {
      this.recordLoadError(name, error);
    });
    return p;
  }

  private resize() {
    const d = Math.min(devicePixelRatio || 1, 2);
    const w = Math.max(1, Math.floor(this.canvas.clientWidth * d));
    const h = Math.max(1, Math.floor(this.canvas.clientHeight * d));
    if (w === this.width && h === this.height) return;
    this.width = w; this.height = h;
    this.canvas.width = w; this.canvas.height = h;
    const g = this.gl;
    g.viewport(0, 0, w, h);
    g.bindTexture(g.TEXTURE_2D, this.sceneTex);
    g.texImage2D(g.TEXTURE_2D, 0, g.RGBA, w, h, 0, g.RGBA, g.UNSIGNED_BYTE, null);
    g.texParameteri(g.TEXTURE_2D, g.TEXTURE_MIN_FILTER, g.LINEAR);
    g.texParameteri(g.TEXTURE_2D, g.TEXTURE_MAG_FILTER, g.LINEAR);
    g.bindRenderbuffer(g.RENDERBUFFER, this.depth);
    g.renderbufferStorage(g.RENDERBUFFER, g.DEPTH_COMPONENT16, w, h);
    g.bindFramebuffer(g.FRAMEBUFFER, this.sceneFb);
    g.framebufferTexture2D(g.FRAMEBUFFER, g.COLOR_ATTACHMENT0, g.TEXTURE_2D, this.sceneTex, 0);
    g.framebufferRenderbuffer(g.FRAMEBUFFER, g.DEPTH_ATTACHMENT, g.RENDERBUFFER, this.depth);
    g.bindFramebuffer(g.FRAMEBUFFER, null);
  }

  render(s: GameState) {
    this.resize();
    this.renderStats.frame += 1;
    this.renderStats.drawCalls = 0;
    this.renderStats.triangles = 0;
    this.renderStats.visibleEntities = 0;
    this.renderStats.players = 0;
    this.renderStats.spheres = 0;
    this.renderStats.enemies = 0;
    const t = s.time;
    const p = s.player.pos;
    const aspect = this.width / Math.max(1, this.height);
    const distance = Math.max(245, Math.min(345, Math.max(s.worldWidth, s.worldHeight) * 0.15));
    this.cameraPos = { x: p.x, y: distance * 0.74, z: p.y + distance * 0.74 };
    const vp = mul(
      persp(48 * DEG, aspect, 1, 2400),
      lookAt(this.cameraPos, { x: p.x, y: 0, z: p.y }, { x: 0, y: 1, z: 0 }),
    );

    const g = this.gl;
    g.bindFramebuffer(g.FRAMEBUFFER, this.sceneFb);
    g.viewport(0, 0, this.width, this.height);
    g.clearColor(0.008, 0.018, 0.048, 1);
    g.clear(g.COLOR_BUFFER_BIT | g.DEPTH_BUFFER_BIT);
    this.drawArena(vp, t, s.worldWidth, s.worldHeight);
    for (const sp of s.spheres) if (sp.alive && this.nearCamera(sp.pos.x, sp.pos.y)) { this.renderStats.visibleEntities += 1; this.renderStats.spheres += 1; this.drawSphere(sp, vp, t); }
    for (const e of s.enemies) if (e.hp > 0 && this.nearCamera(e.pos.x, e.pos.y)) { this.renderStats.visibleEntities += 1; this.renderStats.enemies += 1; this.drawEnemy(e, vp, t); }
    this.renderStats.visibleEntities += 1;
    this.renderStats.players = 1;
    this.drawPlayer(s, vp, t);
    for (const m of s.minions) if (this.nearCamera(m.pos.x, m.pos.y)) this.drawMinion(m, vp, t);
    for (const q of s.sphereProjectiles) if (q.alive) this.drawProjectile(q, vp, t);
    for (const e of s.enemies) for (const q of e.bossProjectiles) if (q.alive) this.drawPointAsset('projectile_energy', q.pos.x, q.pos.y, q.radius * 2, vp, t);
    for (const o of s.xpOrbs) if (o.alive) this.drawPointAsset('projectile_energy', o.pos.x, o.pos.y, o.radius * 1.6, vp, t);
    for (const h of s.healthPacks) if (h.alive) this.drawPointAsset('projectile_fire', h.pos.x, h.pos.y, h.radius * 1.8, vp, t);
    for (const f of s.fireTrails) if (f.life > 0) this.drawPointAsset('projectile_fire', f.pos.x, f.pos.y, 10 + f.life * 3, vp, t);
    this.drawNetwork(s.spheres, vp, t);
    this.drawLightnings(s.lightnings, vp, t);
    for (const pa of s.particles) if (pa.life > 0 && this.nearCamera(pa.pos.x, pa.pos.y, 1100)) this.drawParticle(pa, vp, t);

    (window as any).__ECHO3D_STATS = { ...this.renderStats };

    g.bindFramebuffer(g.FRAMEBUFFER, null);
    g.disable(g.DEPTH_TEST);
    g.useProgram(this.postProgram);
    g.bindBuffer(g.ARRAY_BUFFER, this.quad);
    const a = g.getAttribLocation(this.postProgram, 'a_position');
    g.enableVertexAttribArray(a);
    g.vertexAttribPointer(a, 2, g.FLOAT, false, 0, 0);
    g.activeTexture(g.TEXTURE0);
    g.bindTexture(g.TEXTURE_2D, this.sceneTex);
    g.uniform1i(g.getUniformLocation(this.postProgram, 'u_scene'), 0);
    g.uniform2f(g.getUniformLocation(this.postProgram, 'u_texel'), 1 / this.width, 1 / this.height);
    g.drawArrays(g.TRIANGLE_STRIP, 0, 4);
    g.enable(g.DEPTH_TEST);
  }

  dispose() {
    this.assets.clear();
    this.loading.clear();
  }

  private modelForSphere(type: SphereType, tier: number) {
    return `sphere_${type}_t${tier}`;
  }

  private drawSphere(s: SphereEntity, vp: Mat4, t: number) {
    const tier = Math.max(1, Math.min(7, s.visualTier));
    const playerY = this.cameraPos.z - this.cameraPos.y;
    const distance = Math.hypot(s.pos.x - this.cameraPos.x, s.pos.y - playerY);
    const lodTier = distance > 720 ? Math.min(tier, 3) : distance > 470 ? Math.min(tier, 5) : tier;
    // Generated GLBs use Blender-style unit scale; gameplay radii are much larger world units.
    // Normalize the authored model to the same visual footprint as the legacy 2D sphere.
    const visualScale = Math.max(21, s.radius / 4.65);
    this.drawAsset(this.modelForSphere(s.type, lodTier), s.pos.x, 0, s.pos.y, visualScale, vp, t, `sphere:${lodTier}`, s.rotation);
  }

  private drawEnemy(e: EnemyEntity, vp: Mat4, t: number) {
    const n = e.isBoss
      ? (String(e.bossType).toLowerCase().includes('dist') ? 'boss_distortion'
        : String(e.bossType).toLowerCase().includes('sing') ? 'boss_singularity' : 'boss_colony')
      : (e.type === 'fast' ? 'enemy_flyer'
        : e.type === 'tank' ? 'enemy_guard'
        : e.type === 'boss' ? 'enemy_queen'
        : e.shape === 'triangle' ? 'enemy_psionic'
        : e.shape === 'square' ? 'enemy_slime' : 'enemy_worker');
    const bob = e.type === 'fast' ? Math.sin(t * 7 + e.pos.x * 0.01) * 0.08 : Math.sin(t * 3 + e.pos.y * 0.01) * 0.025;
    const bossPulse = e.isBoss ? 1 + 0.045 * Math.sin(t * 2.6) : 1;
    // Enemies keep a stable authored orientation. They move, bob, recoil and animate through VFX, but do not spin like rigid turntables.
    const authoredFacing = Math.PI / 2;
    const creatureScale = Math.max(10.5, e.radius / 0.88) * (e.isBoss ? 1.65 : 1.25) * bossPulse;
    this.drawAsset(n, e.pos.x, bob, e.pos.y, creatureScale, vp, t, 'enemy', authoredFacing);
  }

  private drawPlayer(s: GameState, vp: Mat4, t: number) {
    const pulse = 1 + 0.06 * Math.sin(t * 4);
    const tilt = 0;
    const characterAsset = ({
      spherist: 'player_spherist',
      hunter: 'player_hunter',
      engineer: 'player_engineer',
      berserker: 'player_berserker',
      alchemist: 'player_alchemist',
      architect: 'player_architect',
    } as Record<string, string>)[s.player.characterId] || 'player_spherist';
    this.drawAsset(characterAsset, s.player.pos.x, 0, s.player.pos.y, 28.0 * pulse, vp, t, 'player', 0);
  }

  private drawMinion(m: MinionEntity, vp: Mat4, t: number) {
    const pulse = 0.46 + 0.035 * Math.sin(t * 5 + m.pos.x * 0.02);
    this.drawAsset('player_core', m.pos.x, 0, m.pos.y, Math.max(8.0, m.radius / 1.45) * pulse, vp, t, 'minion', m.rotation);
  }

  private drawProjectile(p: SphereProjectile, vp: Mat4, t: number) {
    const n = p.effect === 'fire' ? 'projectile_fire' : 'projectile_energy';
    this.drawAsset(n, p.pos.x, 0, p.pos.y, Math.max(4.0, p.radius / 0.6), vp, t, 'projectile', Math.atan2(p.vel.y, p.vel.x));
  }

  private drawPointAsset(name: string, x: number, z: number, size: number, vp: Mat4, t: number) {
    this.drawAsset(name, x, 0, z, Math.max(0.28, size / 5.5), vp, t, 'fx', t * 2);
  }

  private drawAsset(name: string, x: number, y: number, z: number, scale: number, vp: Mat4, t: number, tag: string, angle = 0) {
    const a = this.assets.get(name);
    if (!a) { void this.load(name); return; }
    if (tag === 'enemy') this.currentGlow = 0.55;
    else if (tag.startsWith('sphere:7')) this.currentGlow = 0.72;
    else if (tag.startsWith('sphere:')) this.currentGlow = 0.60;
    else if (tag === 'player') this.currentGlow = 0.82;
    else if (tag === 'projectile') this.currentGlow = 1.15;
    else if (tag === 'fx') this.currentGlow = 1.0;
    else this.currentGlow = 0.82;
    const root = mul(tr(x, y, z), mul(ry(angle), sc(scale, scale, scale)));
    for (const i of a.asset.roots) this.walk(a, i, root, vp, t);
  }

  private walk(a: GPUAsset, ni: number, parent: Mat4, vp: Mat4, t: number) {
    const node = a.asset.nodes[ni];
    let local = node.local;
    // Authored enemy limbs are already posed in model space. Do not rotate each
    // leg/wing around the world origin: that produces the artificial "turntable"
    // motion and tangles the silhouette. Creature motion is conveyed by translation,
    // recoil and local VFX instead.
    if (node.name.includes('Ring_')) local = mul(local, ry(t * (node.name.endsWith('2') ? 0.75 : 1.2)));
    if (node.name.includes('Core') || node.name.includes('VoidCore') || node.name.includes('SingularityCore')) {
      const q = 1 + 0.055 * Math.sin(t * 4.5);
      local = mul(local, sc(q, q, q));
    }
    const world = mul(parent, local);
    if (node.mesh !== null) this.drawMesh(a.gpu[node.mesh], world, vp, t);
    for (const c of node.children) this.walk(a, c, world, vp, t);
  }

  private drawMesh(ms: GPUPrim[], model: Mat4, vp: Mat4, t: number) {
    const g = this.gl;
    for (const m of ms) {
      g.useProgram(this.program);
      const ap = g.getAttribLocation(this.program, 'a_position');
      const an = g.getAttribLocation(this.program, 'a_normal');
      const au = g.getAttribLocation(this.program, 'a_uv');
      const at = g.getAttribLocation(this.program, 'a_tangent');

      g.bindBuffer(g.ARRAY_BUFFER, m.p);
      g.enableVertexAttribArray(ap);
      g.vertexAttribPointer(ap, 3, g.FLOAT, false, 0, 0);
      g.bindBuffer(g.ARRAY_BUFFER, m.n);
      g.enableVertexAttribArray(an);
      g.vertexAttribPointer(an, 3, g.FLOAT, false, 0, 0);

      if (m.u && au >= 0) {
        g.bindBuffer(g.ARRAY_BUFFER, m.u);
        g.enableVertexAttribArray(au);
        g.vertexAttribPointer(au, 2, g.FLOAT, false, 0, 0);
      } else if (au >= 0) {
        g.disableVertexAttribArray(au);
        g.vertexAttrib2f(au, 0, 0);
      }

      if (m.t && at >= 0) {
        g.bindBuffer(g.ARRAY_BUFFER, m.t);
        g.enableVertexAttribArray(at);
        g.vertexAttribPointer(at, 4, g.FLOAT, false, 0, 0);
      } else if (at >= 0) {
        g.disableVertexAttribArray(at);
        g.vertexAttrib4f(at, 1, 0, 0, 1);
      }

      g.bindBuffer(g.ELEMENT_ARRAY_BUFFER, m.i);
      g.uniformMatrix4fv(g.getUniformLocation(this.program, 'u_model'), false, model);
      g.uniformMatrix4fv(g.getUniformLocation(this.program, 'u_mvp'), false, mul(vp, model));
      g.uniform3f(g.getUniformLocation(this.program, 'u_color'), m.color[0], m.color[1], m.color[2]);
      g.uniform3f(g.getUniformLocation(this.program, 'u_emissive'), m.emissive[0], m.emissive[1], m.emissive[2]);
      g.uniform3f(g.getUniformLocation(this.program, 'u_lightColor'), 1.0, 0.92, 0.84);
      g.uniform3f(g.getUniformLocation(this.program, 'u_fillColor'), 0.22, 0.30, 0.48);
      g.uniform3f(g.getUniformLocation(this.program, 'u_camera'), this.cameraPos.x, this.cameraPos.y, this.cameraPos.z);
      g.uniform1f(g.getUniformLocation(this.program, 'u_time'), t);
      g.uniform1f(g.getUniformLocation(this.program, 'u_alpha'), m.alpha);
      g.uniform1f(g.getUniformLocation(this.program, 'u_glow'), this.currentGlow);
      g.uniform1f(g.getUniformLocation(this.program, 'u_metallic'), m.metallic);
      g.uniform1f(g.getUniformLocation(this.program, 'u_roughness'), Math.max(0.045, m.roughness));
      g.uniform1f(g.getUniformLocation(this.program, 'u_alphaMode'), m.alphaMode === 'BLEND' ? 2 : m.alphaMode === 'MASK' ? 1 : 0);
      g.uniform1f(g.getUniformLocation(this.program, 'u_alphaCutoff'), m.alphaCutoff);
      g.uniform1f(g.getUniformLocation(this.program, 'u_decodeBase'), m.baseTex && !(this.gl instanceof WebGL2RenderingContext) ? 1 : 0);
      g.uniform1f(g.getUniformLocation(this.program, 'u_decodeEmissive'), m.emissiveTex && !(this.gl instanceof WebGL2RenderingContext) ? 1 : 0);

      const bindTex = (unit: number, uniform: string, tex: WebGLTexture | null) => {
        g.activeTexture(g.TEXTURE0 + unit);
        g.bindTexture(g.TEXTURE_2D, tex);
        g.uniform1i(g.getUniformLocation(this.program, uniform), unit);
      };
      bindTex(0, 'u_baseTex', m.baseTex);
      bindTex(1, 'u_mrTex', m.mrTex);
      bindTex(2, 'u_normalTex', m.normalTex);
      bindTex(3, 'u_emissiveTex', m.emissiveTex);
      g.uniform1f(g.getUniformLocation(this.program, 'u_hasBase'), m.baseTex ? 1 : 0);
      g.uniform1f(g.getUniformLocation(this.program, 'u_hasMR'), m.mrTex ? 1 : 0);
      g.uniform1f(g.getUniformLocation(this.program, 'u_hasNormal'), m.normalTex && m.t ? 1 : 0);
      g.uniform1f(g.getUniformLocation(this.program, 'u_hasEmissive'), m.emissiveTex ? 1 : 0);

      if (m.doubleSided) g.disable(g.CULL_FACE); else g.enable(g.CULL_FACE);
      if (m.alphaMode === 'BLEND') {
        g.enable(g.BLEND);
        g.depthMask(false);
        g.blendFunc(g.SRC_ALPHA, g.ONE_MINUS_SRC_ALPHA);
      } else {
        g.disable(g.BLEND);
        g.depthMask(true);
      }
      g.drawElements(g.TRIANGLES, m.count, m.indexType, 0);
      this.renderStats.drawCalls += 1;
      this.renderStats.triangles += Math.floor(m.count / 3);
      if (m.alphaMode === 'BLEND') g.depthMask(true);
    }
  }

  private nearCamera(x: number, z: number, range = 980): boolean {
    const dx = x - this.cameraPos.x;
    const dz = z - this.cameraPos.z;
    return dx * dx + dz * dz <= range * range;
  }

  private drawArena(vp: Mat4, t: number, worldWidth: number, worldHeight: number) {
    const extent = Math.min(1800, Math.max(worldWidth, worldHeight) * 0.5 + 420);
    if (!this.arenaGridBuffer) {
      const grid: number[] = [];
      for (let x = -extent; x <= extent; x += 80) grid.push(x, -2.9, -extent, x, -2.9, extent);
      for (let z = -extent; z <= extent; z += 80) grid.push(-extent, -2.9, z, extent, -2.9, z);
      this.arenaGridBuffer = this.buf(new Float32Array(grid));
      this.arenaGridCount = grid.length / 3;

      const rings: number[] = [];
      // The arena should frame the action, not become a giant radar overlay.
      // Three primary rings plus a very soft outer boundary preserve depth while
      // keeping the player and authored assets visually dominant.
      const arenaRadii = [190, 370, 610, Math.min(extent, 860)];
      const steps = 96;
      for (const radius of arenaRadii) {
        const alphaBias = radius >= 850 ? 0.55 : 1;
        for (let i = 0; i < steps; i++) {
          const a0 = i / steps * Math.PI * 2;
          const a1 = (i + 1) / steps * Math.PI * 2;
          rings.push(
            Math.cos(a0) * radius, -2.6, Math.sin(a0) * radius,
            Math.cos(a1) * radius, -2.6, Math.sin(a1) * radius,
          );
        }
        void alphaBias;
      }
      this.arenaRingBuffer = this.buf(new Float32Array(rings));
      this.arenaRingCount = rings.length / 3;
    }

    if (this.arenaGridBuffer) this.drawLineBuffer(this.arenaGridBuffer, this.arenaGridCount, vp, [0.045, 0.20, 0.38], 0.34, t);
    if (this.arenaRingBuffer) this.drawLineBuffer(this.arenaRingBuffer, this.arenaRingCount, vp, [0.075, 0.32, 0.62], 0.30 + 0.06 * Math.sin(t * 2), t);
  }

  private drawLineBuffer(buffer: WebGLBuffer, count: number, vp: Mat4, color: number[], alpha: number, t: number) {
    const g = this.gl;
    g.enable(g.BLEND);
    g.blendFunc(g.SRC_ALPHA, g.ONE_MINUS_SRC_ALPHA);
    g.useProgram(this.lineProgram);
    const a = g.getAttribLocation(this.lineProgram, 'a_position');
    g.bindBuffer(g.ARRAY_BUFFER, buffer);
    g.enableVertexAttribArray(a);
    g.vertexAttribPointer(a, 3, g.FLOAT, false, 0, 0);
    g.uniformMatrix4fv(g.getUniformLocation(this.lineProgram, 'u_mvp'), false, vp);
    g.uniform3f(g.getUniformLocation(this.lineProgram, 'u_color'), color[0], color[1], color[2]);
    g.uniform1f(g.getUniformLocation(this.lineProgram, 'u_alpha'), alpha);
    g.uniform1f(g.getUniformLocation(this.lineProgram, 'u_time'), t);
    g.lineWidth(2);
    g.drawArrays(g.LINES, 0, count);
    this.renderStats.drawCalls += 1;
  }

  private drawNetwork(ss: SphereEntity[], vp: Mat4, t: number) {
    for (let i = 0; i < ss.length; i++) for (let j = i + 1; j < ss.length; j++) {
      if (ss[i].alive && ss[j].alive) this.line(
        [ss[i].pos.x, 0, ss[i].pos.y],
        [ss[j].pos.x, 0, ss[j].pos.y],
        vp, [0.15, 0.65, 1], 0.5 + 0.2 * Math.sin(t * 5), t,
      );
    }
  }

  private drawLightnings(ls: LightningBolt[], vp: Mat4, t: number) {
    for (const l of ls) {
      const pts: number[] = [];
      for (let i = 0; i <= 8; i++) {
        const q = i / 8;
        pts.push(
          l.from.x + (l.to.x - l.from.x) * q + (i % 2 ? Math.sin(t * 50 + i) * 7 : 0),
          0,
          l.from.y + (l.to.y - l.from.y) * q + (i % 2 ? Math.cos(t * 47 + i) * 7 : 0),
        );
      }
      this.polyline(pts, vp, [0.3, 0.7, 1], Math.max(0.1, l.life), t);
    }
  }

  private drawParticle(p: Particle, vp: Mat4, t: number) {
    this.drawPointAsset(p.size > 18 ? 'projectile_fire' : 'projectile_energy', p.pos.x, p.pos.y, p.size, vp, t);
  }

  private line(a: number[], b: number[], vp: Mat4, c: number[], alpha: number, t: number) {
    this.polyline([...a, ...b], vp, c, alpha, t);
  }

  private polyline(points: number[], vp: Mat4, c: number[], alpha: number, t: number) {
    const g = this.gl;
    const b = this.buf(new Float32Array(points));
    g.useProgram(this.lineProgram);
    const a = g.getAttribLocation(this.lineProgram, 'a_position');
    g.bindBuffer(g.ARRAY_BUFFER, b);
    g.enableVertexAttribArray(a);
    g.vertexAttribPointer(a, 3, g.FLOAT, false, 0, 0);
    g.uniformMatrix4fv(g.getUniformLocation(this.lineProgram, 'u_mvp'), false, vp);
    g.uniform3f(g.getUniformLocation(this.lineProgram, 'u_color'), c[0], c[1], c[2]);
    g.uniform1f(g.getUniformLocation(this.lineProgram, 'u_alpha'), alpha);
    g.uniform1f(g.getUniformLocation(this.lineProgram, 'u_time'), t);
    g.lineWidth(2);
    g.drawArrays(points.length === 6 ? g.LINES : g.LINE_STRIP, 0, points.length / 3);
    g.deleteBuffer(b);
  }
}

export function createEcho3DRenderer(canvas: HTMLCanvasElement) {
  return new Echo3DRenderer(canvas);
}
