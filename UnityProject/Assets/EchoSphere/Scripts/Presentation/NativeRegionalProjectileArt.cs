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
    if(ground){
     // Local retained masks: irregular liquid borders and concave silk strands.
     Vector2[] Border(float scale){var pts=new Vector2[32];for(int i=0;i<pts.Length;i++){float a=i*Mathf.PI*2/pts.Length;float q=kind==7?.90f+.08f*Mathf.Cos(a*8):.83f+.10f*Mathf.Sin(a*3+.6f)+.06f*Mathf.Cos(a*7);pts[i]=P(Mathf.Cos(a)*q*scale,Mathf.Sin(a)*q*scale);}return pts;}
     art.Add(Polygon(Border(1)));art.Add(Polygon(Border(.79f)));
     if(kind==7){
      for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Stroke(Mathf.Cos(a)*.08f,Mathf.Sin(a)*.08f,Mathf.Cos(a)*.98f,Mathf.Sin(a)*.98f,.025f);}
      for(int ring=1;ring<=3;ring++)for(int i=0;i<8;i++){
       float a=i*Mathf.PI/4,b=(i+1)*Mathf.PI/4,q=ring*.27f;var start=P(Mathf.Cos(a)*q,Mathf.Sin(a)*q);var end=P(Mathf.Cos(b)*q,Mathf.Sin(b)*q);var mid=P(Mathf.Cos((a+b)*.5f)*q*.79f,Mathf.Sin((a+b)*.5f)*q*.79f);
       var path=new Unity.VectorGraphics.BezierContour{Closed=false,Segments=new[]{new Unity.VectorGraphics.BezierPathSegment{P0=start,P1=Vector2.Lerp(start,mid,.7f),P2=Vector2.Lerp(end,mid,.7f)},new Unity.VectorGraphics.BezierPathSegment{P0=end,P1=end,P2=end}}};art.Add(Tessellate(path,Mathf.Max(.45f,r*.022f)));
      }
      for(int i=0;i<8;i++){float a=i*Mathf.PI/4;art.Add(Circle(P(Mathf.Cos(a)*.98f,Mathf.Sin(a)*.98f),r*.035f));}
     }else{
      art.Add(Polygon(Border(1),Mathf.Max(.7f,r*.035f)));art.Add(Polygon(Border(.80f),Mathf.Max(.55f,r*.022f)));
      for(int i=0;i<7;i++){float a=i*2.399963f,q=.15f+.07f*i;var center=P(Mathf.Cos(a)*q,Mathf.Sin(a)*q);art.Add(Circle(center,r*(.045f+i%3*.018f)));art.Add(Arc(r*(.07f+i%3*.02f),Mathf.PI*1.4f,Mathf.Max(.5f,r*.02f)));}
     }
    }
    else if(region==0){Poly(P(1.1f,0),P(.25f,-.65f),P(-.9f,-.45f),P(-.6f,0),P(-.9f,.45f),P(.25f,.65f));Stroke(-2.2f,0,-.6f,0,.22f);Poly(P(.8f,0),P(-.1f,-.2f),P(-.35f,0),P(-.1f,.2f));Stroke(-.7f,-.75f,.1f,-.4f,.1f);Stroke(-.7f,.75f,.1f,.4f,.1f);}
    else if(region==1&&style==1){Poly(P(1.8f,0),P(.8f,-.25f),P(-2.8f,-.25f),P(-3.5f,0),P(-2.8f,.25f),P(.8f,.25f));Stroke(-2.8f,0,1.2f,0,.09f);Stroke(-1,-.55f,.2f,-.55f,.09f);Stroke(-1,.55f,.2f,.55f,.09f);}
    else if(region==1){Poly(P(1.6f,0),P(.45f,-.48f),P(-1.1f,-.48f),P(-1.1f,.48f),P(.45f,.48f));Poly(P(-.2f,-.45f),P(-1.3f,-1),P(-.95f,-.2f));Poly(P(-.2f,.45f),P(-1.3f,1),P(-.95f,.2f));Poly(P(-1,0),P(-2.5f,-.3f),P(-2.1f,0),P(-2.5f,.3f));Stroke(.2f,-.2f,.9f,0,.10f);}
    else if(region==2&&style==2){Poly(P(1.2f,0),P(0,-.9f),P(-1.2f,0),P(0,.9f));Stroke(-1.2f,0,1.2f,0,.08f);Stroke(0,-.9f,0,.9f,.08f);art.Add(Arc(r*.55f,Mathf.PI*2,r*.08f));Stroke(-2.3f,-.3f,-1,0,.09f);Stroke(-2.3f,.3f,-1,0,.09f);}
    else if(region==2){Disk(.65f);for(int i=0;i<4;i++){float a=i*Mathf.PI/2;art.Add(Circle(P(-.4f+Mathf.Cos(a)*.5f,Mathf.Sin(a)*.5f),r*.35f));}art.Add(Circle(P(.25f,-.18f),r*.18f));Poly(P(-.5f,-.3f),P(-2.6f,0),P(-.5f,.3f));}
    else if(region==3){Poly(P(1.65f,0),P(-.25f,-.7f),P(-1.1f,-.35f),P(-.8f,.55f),P(-.25f,.7f));Stroke(-.7f,0,1.4f,0,.09f);Poly(P(-1.6f,-.7f),P(-.8f,-.9f),P(-1.2f,-.25f));Poly(P(-1.5f,.8f),P(-.8f,.55f),P(-1.1f,1.1f));}
    else if(region==4){Poly(P(1.5f,0),P(.35f,-.8f),P(-.7f,-.4f),P(-.25f,0),P(-.7f,.4f),P(.35f,.8f));Stroke(-2.7f,.15f,-1.7f,-.35f,.12f);Stroke(-1.7f,-.35f,-.6f,.15f,.12f);art.Add(Arc(r*1.05f,Mathf.PI*1.5f,r*.12f));Disk(.2f);}
    else{Poly(P(1.3f,0),P(.3f,-.85f),P(-.35f,-.35f),P(-1.1f,0),P(-.35f,.35f),P(.3f,.85f));art.Add(Arc(r*.5f,Mathf.PI*1.5f,r*.12f));Stroke(-1.7f,-.65f,-1.2f,-.65f,.09f);Stroke(-2.2f,.65f,-1.3f,.65f,.09f);Poly(P(-2.1f,0),P(-1.7f,-.18f),P(-1.3f,0),P(-1.7f,.18f));}
   }
   var color=Palette[Mathf.Clamp(region,0,5)];if(kind==7)color=new Color(.72f,.86f,.82f);else if(kind==6)color=new Color(.45f,.71f,.16f);float angle=ground?0:(float)Math.Atan2(vy,vx)*Mathf.Rad2Deg;var transform=Matrix4x4.TRS(new Vector3(x,y),Quaternion.Euler(0,0,angle),new Vector3(!ground&&style==1&&region!=1?1.45f:1,!ground&&style==1&&region!=1?.72f:1,1));
   for(int i=0;i<art.Parts.Count;i++){Color tint=color;if(ground){
      if(i<2){tint=kind==7?new Color(.29f,.48f,.40f):new Color(.20f,.36f,.06f);tint.a=i==0?(kind==7?.06f:.22f):.13f;}
      else{tint=Color.Lerp(color,Color.white,kind==7?.22f:.10f);tint.a=kind==7?.43f:i<4?.62f:.34f+.13f*Mathf.Sin((float)time*2+i);}
     }else if(i==0)tint.a=.85f;else{tint=Color.Lerp(color,Color.white,.55f);tint.a=.5f+.35f*(float)Math.Sin(time*9+i);}
    var local=Matrix4x4.identity;if(ground&&kind==6&&i>=4){int bubble=(i-4)/2;float a=bubble*2.399963f,q=.15f+.07f*bubble;local=Matrix4x4.Translate(new Vector3(Mathf.Cos(a)*q*r,Mathf.Sin(a)*q*r));if((i-4)%2==0)local=Matrix4x4.identity;}if(!ground&&region==3&&i>=2)local=Matrix4x4.Translate(new Vector3(0,(float)Math.Sin(time*8+i)*r*.12f));if(!ground&&region==4&&i==3)local=Matrix4x4.Rotate(Quaternion.Euler(0,0,(float)time*190));if(_used==_blocks.Count)_blocks.Add(new MaterialPropertyBlock());var properties=_blocks[_used++];properties.Clear();properties.SetVector("_NativeColor",new Vector4(tint.r,tint.g,tint.b,tint.a));surface.DrawNativeMesh(art.Parts[i],_material,properties,transform*local,shadow:Color.clear,shadowBlur:0,inheritShadow:false,additive:!ground&&i>0,inheritShadowOffsets:false);}
  }
  public void Dispose(){foreach(var art in _art.Values)art.Dispose();_art.Clear();UnityEngine.Object.Destroy(_material);}
 }
}
