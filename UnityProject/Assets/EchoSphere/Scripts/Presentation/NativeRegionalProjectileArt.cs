using System;using System.Collections.Generic;using UnityEngine;using static EchoSphere.Presentation.NativeWorldShapes;
namespace EchoSphere.Presentation {
 // Enemy-only silhouettes. Moving laser packets retain existing projectile travel/collision, not hitscan beams.
 internal sealed class NativeRegionalProjectileArt:IDisposable {
  sealed class Art:IDisposable{internal readonly List<Mesh> Parts=new List<Mesh>();internal Mesh Add(Mesh m){Parts.Add(m);return m;}public void Dispose(){foreach(var m in Parts)UnityEngine.Object.Destroy(m);}}
  readonly Dictionary<(float,int,int,int),Art> _art=new Dictionary<(float,int,int,int),Art>();readonly Material _material;readonly List<MaterialPropertyBlock> _blocks=new List<MaterialPropertyBlock>();int _used;
  internal void BeginFrame(){_used=0;}
  static readonly Color[] Palette={new Color(1,.3f,.17f),new Color(1,.38f,.12f),new Color(.64f,.84f,.19f),new Color(.34f,.83f,1),new Color(1,.74f,.2f),new Color(.84f,.4f,1)};
  internal NativeRegionalProjectileArt(){_material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Regional hostile ammunition",hideFlags=HideFlags.DontSave};}
  internal void Draw(NativeWorldSurface surface,float x,float y,float radius,double vx,double vy,int region,int kind,int style,double time){
   float r=Mathf.Max(2,radius);bool ground=kind==6||kind==7;var key=(r,region,ground?kind:0,style);if(!_art.TryGetValue(key,out var art)){art=new Art();_art.Add(key,art);Mesh Poly(params Vector2[] p)=>art.Add(Polygon(p));Mesh Stroke(float ax,float ay,float bx,float by,float w)=>art.Add(Line(new Vector2(ax,ay)*r,new Vector2(bx,by)*r,w*r));Mesh Disk(float rad)=>art.Add(Circle(Vector2.zero,rad*r));Vector2 P(float a,float b)=>new Vector2(a,b)*r;
    if(ground){Disk(1);art.Add(Arc(r*.94f,Mathf.PI*2,1));if(kind==7){for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Stroke(0,0,Mathf.Cos(a),Mathf.Sin(a),.045f);}art.Add(Arc(r*.48f,Mathf.PI*2,.8f));}else{for(int i=0;i<5;i++){float a=i*Mathf.PI*2/5;art.Add(Circle(P(Mathf.Cos(a)*.58f,Mathf.Sin(a)*.58f),r*.18f));}}}
    else if(region==0){Poly(P(1.1f,0),P(.25f,-.65f),P(-.9f,-.45f),P(-.6f,0),P(-.9f,.45f),P(.25f,.65f));Stroke(-2.2f,0,-.6f,0,.22f);Poly(P(.8f,0),P(-.1f,-.2f),P(-.35f,0),P(-.1f,.2f));Stroke(-.7f,-.75f,.1f,-.4f,.1f);Stroke(-.7f,.75f,.1f,.4f,.1f);}
    else if(region==1&&style==1){Poly(P(1.8f,0),P(.8f,-.25f),P(-2.8f,-.25f),P(-3.5f,0),P(-2.8f,.25f),P(.8f,.25f));Stroke(-2.8f,0,1.2f,0,.09f);Stroke(-1,-.55f,.2f,-.55f,.09f);Stroke(-1,.55f,.2f,.55f,.09f);}
    else if(region==1){Poly(P(1.6f,0),P(.45f,-.48f),P(-1.1f,-.48f),P(-1.1f,.48f),P(.45f,.48f));Poly(P(-.2f,-.45f),P(-1.3f,-1),P(-.95f,-.2f));Poly(P(-.2f,.45f),P(-1.3f,1),P(-.95f,.2f));Poly(P(-1,0),P(-2.5f,-.3f),P(-2.1f,0),P(-2.5f,.3f));Stroke(.2f,-.2f,.9f,0,.10f);}
    else if(region==2&&style==2){Poly(P(1.2f,0),P(0,-.9f),P(-1.2f,0),P(0,.9f));Stroke(-1.2f,0,1.2f,0,.08f);Stroke(0,-.9f,0,.9f,.08f);art.Add(Arc(r*.55f,Mathf.PI*2,r*.08f));Stroke(-2.3f,-.3f,-1,0,.09f);Stroke(-2.3f,.3f,-1,0,.09f);}
    else if(region==2){Disk(.65f);for(int i=0;i<4;i++){float a=i*Mathf.PI/2;art.Add(Circle(P(-.4f+Mathf.Cos(a)*.5f,Mathf.Sin(a)*.5f),r*.35f));}art.Add(Circle(P(.25f,-.18f),r*.18f));Poly(P(-.5f,-.3f),P(-2.6f,0),P(-.5f,.3f));}
    else if(region==3){Poly(P(1.65f,0),P(-.25f,-.7f),P(-1.1f,-.35f),P(-.8f,.55f),P(-.25f,.7f));Stroke(-.7f,0,1.4f,0,.09f);Poly(P(-1.6f,-.7f),P(-.8f,-.9f),P(-1.2f,-.25f));Poly(P(-1.5f,.8f),P(-.8f,.55f),P(-1.1f,1.1f));}
    else if(region==4){Poly(P(1.5f,0),P(.35f,-.8f),P(-.7f,-.4f),P(-.25f,0),P(-.7f,.4f),P(.35f,.8f));Stroke(-2.7f,.15f,-1.7f,-.35f,.12f);Stroke(-1.7f,-.35f,-.6f,.15f,.12f);art.Add(Arc(r*1.05f,Mathf.PI*1.5f,r*.12f));Disk(.2f);}
    else{Poly(P(1.3f,0),P(.3f,-.85f),P(-.35f,-.35f),P(-1.1f,0),P(-.35f,.35f),P(.3f,.85f));art.Add(Arc(r*.5f,Mathf.PI*1.5f,r*.12f));Stroke(-1.7f,-.65f,-1.2f,-.65f,.09f);Stroke(-2.2f,.65f,-1.3f,.65f,.09f);Poly(P(-2.1f,0),P(-1.7f,-.18f),P(-1.3f,0),P(-1.7f,.18f));}
   }
   var color=Palette[Mathf.Clamp(region,0,5)];if(kind==7)color=new Color(.76f,.9f,.68f);float angle=ground?0:(float)Math.Atan2(vy,vx)*Mathf.Rad2Deg;var transform=Matrix4x4.TRS(new Vector3(x,y),Quaternion.Euler(0,0,angle),new Vector3(!ground&&style==1&&region!=1?1.45f:1,!ground&&style==1&&region!=1?.72f:1,1));
   for(int i=0;i<art.Parts.Count;i++){Color tint=color;if(ground)tint.a=i==0?.12f:.65f;else if(i==0)tint.a=.85f;else{tint=Color.Lerp(color,Color.white,.55f);tint.a=.5f+.35f*(float)Math.Sin(time*9+i);}
    var local=Matrix4x4.identity;if(!ground&&region==3&&i>=2)local=Matrix4x4.Translate(new Vector3(0,(float)Math.Sin(time*8+i)*r*.12f));if(!ground&&region==4&&i==3)local=Matrix4x4.Rotate(Quaternion.Euler(0,0,(float)time*190));if(_used==_blocks.Count)_blocks.Add(new MaterialPropertyBlock());var properties=_blocks[_used++];properties.Clear();properties.SetVector("_NativeColor",new Vector4(tint.r,tint.g,tint.b,tint.a));surface.DrawNativeMesh(art.Parts[i],_material,properties,transform*local,shadow:Color.clear,shadowBlur:0,inheritShadow:false,additive:i>0,inheritShadowOffsets:false);}
  }
  public void Dispose(){foreach(var art in _art.Values)art.Dispose();_art.Clear();UnityEngine.Object.Destroy(_material);}
 }
}
