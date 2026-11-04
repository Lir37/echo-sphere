using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;
namespace EchoSphere.Presentation {
 internal sealed class NativeWorldMinions : IDisposable {
  private sealed class Art { internal Mesh Body,Edge,Fin,Trace; internal void Dispose(){Destroy(Body);Destroy(Edge);Destroy(Fin);Destroy(Trace);} }
  private readonly Dictionary<SphereId,Art> _art=new Dictionary<SphereId,Art>();
  private readonly Material _material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){hideFlags=HideFlags.DontSave};
  private readonly Mesh _core=Circle(Vector2.zero,2.1f),_pin=Circle(Vector2.zero,.9f);
  private readonly List<MaterialPropertyBlock> _props=new List<MaterialPropertyBlock>();private int _used;
  internal void BeginFrame(){_used=0;}
  private MaterialPropertyBlock ColorBlock(Color c){if(_used==_props.Count)_props.Add(new MaterialPropertyBlock());var p=_props[_used++];p.SetColor("_NativeColor",c);return p;}
  private static Vector2 P(float x,float y)=>new Vector2(x,y);
  private static Matrix4x4 T(float x,float y,float angle=0,float sx=1,float sy=1)=>Matrix4x4.TRS(new Vector3(x,y,0),Quaternion.Euler(0,0,angle),new Vector3(sx,sy,1));
  private Art Get(SphereId type){if(_art.TryGetValue(type,out var a))return a;
   Vector2[] hull;switch(type){
    case SphereId.Sniper:hull=new[]{P(0,-9),P(3,-3),P(3,5),P(0,8),P(-3,5),P(-3,-3)};break;
    case SphereId.Shotgun:hull=new[]{P(-6,-4),P(0,-7),P(6,-4),P(5,5),P(0,7),P(-5,5)};break;
    case SphereId.Orbital:hull=new[]{P(0,-7),P(6,0),P(0,7),P(-6,0)};break;
    case SphereId.Prism:hull=new[]{P(0,-8),P(6,5),P(-6,5)};break;
    default:hull=new[]{P(-4,-5),P(0,-7),P(4,-5),P(5,3),P(0,6),P(-5,3)};break;}
   a=new Art{Body=Polygon(hull),Edge=Polygon(hull,.85f)};
   Vector2[] fin;switch(type){
    case SphereId.Chain:fin=new[]{P(5,-5),P(8,-7),P(7,-1),P(10,1),P(6,5)};break;
    case SphereId.Sniper:fin=new[]{P(5,-6),P(6,-6),P(7,6),P(5,4)};break;
    case SphereId.Shotgun:fin=new[]{P(7,-4),P(10,-3),P(10,2),P(7,4)};break;
    case SphereId.Orbital:fin=new[]{P(6,0),P(13,-4),P(10,4)};break;
    case SphereId.Aura:fin=new[]{P(6,-5),P(9,-2),P(9,2),P(6,5),P(7,0)};break;
    case SphereId.Gravity:fin=new[]{P(6,-5),P(10,-3),P(10,3),P(6,5),P(8,0)};break;
    case SphereId.Pulse:fin=new[]{P(6,-6),P(8,-6),P(10,0),P(8,6),P(6,6),P(8,0)};break;
    case SphereId.Void:fin=new[]{P(6,-6),P(11,-3),P(8,0),P(10,5),P(6,3)};break;
    case SphereId.Prism:fin=new[]{P(7,-5),P(11,4),P(7,2)};break;
    default:fin=new[]{P(6,-4),P(9,-2),P(9,3),P(6,5)};break;}
   a.Fin=Polygon(fin,.95f);a.Trace=Line(P(0,-5),P(0,-2),.85f);_art.Add(type,a);return a;
  }
  internal void Draw(NativeWorldSurface surface,float x,float y,float radius,float rotation,Color color,double time,SphereId type=SphereId.Standard,float attack=0){
   var a=Get(type);float bob=(float)Math.Sin(time*3+x*.01)*.6f;var root=T(x,y+bob,rotation*12,radius/12,radius/12);
   void Paint(Mesh mesh,Matrix4x4 m,Color c)=>surface.DrawNativeMesh(mesh,_material,ColorBlock(c),m,inheritShadow:false);
   Paint(a.Body,root,new Color(.025f,.055f,.09f,1));Paint(a.Edge,root,color);
   float recoil=Mathf.Sin(attack*Mathf.PI)*2;for(int side=-1;side<=1;side+=2){var fin=root*T(side*recoil,0,side*recoil*3,side,1);Paint(a.Fin,fin,color);Paint(_pin,root*T(side*(8+recoil),3),Color.Lerp(color,Color.white,.5f));}
   Paint(a.Trace,root,color);Paint(_core,root,Color.Lerp(color,Color.white,.6f));
  }
  private readonly Dictionary<int,Art> _merchants=new Dictionary<int,Art>();
  internal void DrawMerchant(NativeWorldSurface surface,float x,float y,int region,double time){
   if(!_merchants.TryGetValue(region,out var a)){Vector2[] body;switch(region){
    case 1:body=new[]{P(0,-15),P(10,-3),P(12,11),P(0,7),P(-12,11),P(-10,-3)};break;
    case 2:body=new[]{P(0,-14),P(8,-8),P(11,3),P(6,12),P(-6,12),P(-11,3),P(-8,-8)};break;
    case 3:body=new[]{P(0,-17),P(12,1),P(7,12),P(-7,12),P(-12,1)};break;
    case 4:body=new[]{P(0,-13),P(9,-5),P(6,3),P(12,9),P(0,13),P(-12,9),P(-6,3),P(-9,-5)};break;
    case 5:body=new[]{P(0,-15),P(9,-7),P(7,4),P(13,13),P(0,9),P(-13,13),P(-7,4),P(-9,-7)};break;
    default:body=new[]{P(0,-15),P(9,-7),P(7,0),P(12,12),P(-12,12),P(-7,0),P(-9,-7)};break;}
    a=new Art{Body=Polygon(body),Edge=Polygon(body,1.2f),Fin=Polygon(new[]{P(13,-2),P(20,-4),P(22,4),P(17,8),P(12,5)},1),Trace=Line(P(-4,-5),P(4,-5),1.5f)};_merchants.Add(region,a);}
   Color[] palette={new Color(.5f,.87f,.65f),new Color(.45f,.8f,1),new Color(.85f,.66f,.34f),new Color(.75f,.6f,1),new Color(.55f,.95f,1),new Color(.85f,.5f,.8f)};var color=palette[Mathf.Clamp(region,0,5)];var root=T(x,y+(float)Math.Sin(time*2)*.7f);
   void Paint(Mesh mesh,Matrix4x4 m,Color c)=>surface.DrawNativeMesh(mesh,_material,ColorBlock(c),m,inheritShadow:false);
   Paint(a.Body,root,new Color(.025f,.05f,.07f,1));Paint(a.Edge,root,color);Paint(a.Trace,root,Color.white);
   for(int side=-1;side<=1;side+=2)Paint(a.Fin,root*T(side*(float)Math.Sin(time*1.7)*.5f,0,0,side,1),color);
   Paint(_core,root*T(0,3),color);Paint(_pin,root*T(0,-21),Color.Lerp(color,Color.white,.5f));
  }
  private static void Destroy(UnityEngine.Object o){if(o!=null)UnityEngine.Object.Destroy(o);}
  public void Dispose(){foreach(var a in _merchants.Values)a.Dispose();_merchants.Clear();foreach(var a in _art.Values)a.Dispose();_art.Clear();Destroy(_core);Destroy(_pin);Destroy(_material);}
 }
}
