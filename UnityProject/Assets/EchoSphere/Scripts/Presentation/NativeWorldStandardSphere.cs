using System;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    internal sealed class NativeWorldStandardSphere : IDisposable
    {
        private sealed class Rig : IDisposable
        {
            internal readonly Mesh Core,Outline,Ring,Arc,Accent;
            internal readonly BezierContour CorePath,ArcPath,AccentPath;
            internal Rig(float r,PathEnding cap,PathCorner corner,float miter)
            {
                CorePath=EllipseContour(Vector2.zero,r*.42f,r*.42f);Core=Tessellate(CorePath,0);Outline=Tessellate(CorePath,Mathf.Max(.8f,r*.035f),cap:cap,corners:corner,miter:miter);
                Ring=Tessellate(EllipseContour(Vector2.zero,r*.98f,r*.34f),Mathf.Max(.9f,r*.045f),cap:cap,corners:corner,miter:miter);
                ArcPath=EllipseContour(Vector2.zero,r*.88f,r*.27f,Mathf.PI*.10f,Mathf.PI*1.92f);Arc=Tessellate(ArcPath,Mathf.Max(.55f,r*.022f),cap:cap,corners:corner,miter:miter);
                AccentPath=PolylineContour(false,P(-r*.44,-r*.22),P(-r*.08,-r*.48),P(r*.36,-r*.18));Accent=Tessellate(AccentPath,Mathf.Max(.7f,r*.030f),cap:cap,corners:corner,miter:miter);
            }
            public void Dispose(){Destroy(Accent);Destroy(Core);Destroy(Outline);Destroy(Ring);Destroy(Arc);}
        }
        private readonly Dictionary<(float,PathEnding,PathCorner,float),Rig> _rigs=new Dictionary<(float,PathEnding,PathCorner,float),Rig>();
        private readonly List<Mesh> _dynamic=new List<Mesh>();private int _meshUsed,_propertiesUsed;
        private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();
        private readonly Material _solid=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Native Standard Sphere details",hideFlags=HideFlags.DontSave};
        private readonly Material _glow=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldStandardCore")){name="Native Standard Sphere core",hideFlags=HideFlags.DontSave};
        internal void BeginFrame(){_meshUsed=0;_propertiesUsed=0;}
        private MaterialPropertyBlock Properties(Color c){if(_propertiesUsed==_properties.Count)_properties.Add(new MaterialPropertyBlock());var p=_properties[_propertiesUsed++];p.Clear();p.SetVector("_NativeColor",new Vector4(c.r,c.g,c.b,c.a));return p;}
        private Rig Get(NativeWorldSurface s,float r){var key=(r,s.NativeStrokeEnding,s.NativeStrokeCorner,s.NativeStrokeMiter);if(!_rigs.TryGetValue(key,out var rig)){rig=new Rig(r,key.Item2,key.Item3,key.Item4);_rigs.Add(key,rig);}return rig;}
        private Mesh Dynamic(NativeWorldSurface s,BezierContour contour,float width=0)
        {if(_meshUsed==_dynamic.Count){var mesh=new Mesh {name="Native Standard Sphere animated art"};mesh.MarkDynamic();_dynamic.Add(mesh);}return Tessellate(contour,width,_dynamic[_meshUsed++],cap:s.NativeStrokeEnding,corners:s.NativeStrokeCorner,miter:s.NativeStrokeMiter);}
        private static Color C(int r,int g,int b,double a=1)=>new Color(r/255f,g/255f,b/255f,(float)a);
        private static Vector2 P(double x,double y)=>new Vector2((float)x,(float)y);
        private static Matrix4x4 Transform(double x=0,double y=0,double angle=0)=>Matrix4x4.TRS(new Vector3((float)x,(float)y,0),Quaternion.Euler(0,0,(float)angle*Mathf.Rad2Deg),Vector3.one);
        internal void DrawCore(NativeWorldSurface s,double radius,double pulse,Matrix4x4? parent=null,Color? familyColor=null)
        {
            var root=parent??Matrix4x4.identity;float r=(float)radius,opacity=(float)(.82+pulse*.14);var rig=Get(s,r);float rad=(float)(radius*.58*(1+pulse*.035));var glow=Dynamic(s,EllipseContour(Vector2.zero,rad,rad));var p=Properties(Color.white);p.SetFloat("_Radius",r);p.SetFloat("_Family",familyColor.HasValue?1:0);if(familyColor.HasValue){var c=familyColor.Value;p.SetVector("_NativeColor",new Vector4(c.r,c.g,c.b,c.a));}
            s.DrawNativeMesh(glow,_glow,p,root,opacity:opacity,inheritShadow:true,additive:true);
            s.DrawNativeMesh(rig.Core,_solid,Properties(familyColor.HasValue?new Color(familyColor.Value.r,familyColor.Value.g,familyColor.Value.b,.72f):C(30,135,232,.72)),root,opacity:opacity,inheritShadow:true);s.DrawNativeMesh(rig.Outline,_solid,Properties(C(182,247,255,.90)),root,opacity:opacity,inheritShadow:true);
        }
        internal void DrawRing(NativeWorldSurface s,double radius,double rotation,double opacity,Matrix4x4? parent=null)
        {
            var rig=Get(s,(float)radius);var local=(parent??Matrix4x4.identity)*Transform(angle:rotation);var shadow=C(54,223,255);s.DrawNativeMesh(rig.Ring,_solid,Properties(C(48,214,255,.82)),local,shadow:shadow,shadowBlur:3,opacity:(float)opacity,additive:true,inheritShadowOffsets:true);
            s.DrawNativeMesh(rig.Arc,_solid,Properties(C(48,214,255,.82)),local,shadow:shadow,shadowBlur:3,opacity:(float)(opacity*.55),additive:true,inheritShadowOffsets:true);
        }
        internal bool DrawAssembly(NativeWorldSurface s,double x,double y,double r,double tier,double time,bool target,NativeWorldSprites sprites,Func<string,SourceCanvasImage> resolve)
        {
            double breathe=Math.Sin(time*2.2+x*.013)*.028,energy=.5+.5*Math.Sin(time*4.6+y*.009),targetPulse=target?.5+.5*Math.Sin(time*11):0,attackScale=1+targetPulse*.025;
            var root=Transform(x,y)*Matrix4x4.Scale(new Vector3((float)(attackScale+breathe*.18),(float)(attackScale-breathe*.12),1));
            DrawCore(s,r,energy,root);DrawRing(s,r*1.18,time*(.22+tier*.018),.72+targetPulse*.20,root);
            double size=r*1.34*(1+energy*.022);sprites.Draw(s,resolve("sphere-standard-upper-crystal"),0,-r*.62-energy*r*.010,size,0,1,true,false,false,root);sprites.Draw(s,resolve("sphere-standard-upper-crystal"),0,r*.66+energy*r*.010,size,0,1,true,false,true,root);
            if(targetPulse>.55){var rig=Get(s,(float)r);s.DrawNativeMesh(rig.Accent,_solid,Properties(C(217,251,255)),root,opacity:(float)(.14+targetPulse*.12),inheritShadow:true,additive:true);}return true;
        }
        internal void DrawAttack(NativeWorldSurface s,double x,double y,double rotation,double r,double timer)
        {
            double charge=Math.Max(0,Math.Min(1,1-timer/1.2));if(charge<.04)return;double reach=r*(1.02+charge*.84);float width=(float)(1.35+charge*.45);var local=Transform(x,y,rotation);var color=C(99,230,255);
            void Stroke(BezierContour path,double alpha){s.DrawNativeMesh(Dynamic(s,path,width),_solid,Properties(color),local,opacity:(float)alpha,inheritShadow:true,additive:true);}
            Stroke(QuadraticContour(P(reach*.68,-r*.18),false,P(reach*.90,-r*.32),P(reach,-r*.08),P(reach*1.08,0),P(reach,r*.08),P(reach*.90,r*.32),P(reach*.68,r*.18)),.20+charge*.24);
            Stroke(QuadraticContour(P(reach*.74,-r*.13),false,P(reach*.94,0),P(reach*1.02,-r*.03),P(reach*1.10,0),P(reach*1.02,r*.03),P(reach*.94,0),P(reach*.74,r*.13)),.34+charge*.32);
            float arcR=(float)(r*(.15+charge*.08));Stroke(EllipseContour(P(reach,0),arcR,arcR,-Mathf.PI*.72f,Mathf.PI*.72f),.24+charge*.28);
            float coreR=(float)(r*(.065+charge*.035));var core=EllipseContour(P(reach*.98,0),coreR,coreR);s.DrawNativeMesh(Dynamic(s,core),_solid,Properties(Color.white),local,opacity:(float)(.72+charge*.18),inheritShadow:true,additive:true);
        }
        private static void Destroy(UnityEngine.Object value){if(value!=null)UnityEngine.Object.Destroy(value);}
        public void Dispose(){foreach(var rig in _rigs.Values)rig.Dispose();foreach(var mesh in _dynamic)Destroy(mesh);_rigs.Clear();_dynamic.Clear();Destroy(_solid);Destroy(_glow);}
    }
}
