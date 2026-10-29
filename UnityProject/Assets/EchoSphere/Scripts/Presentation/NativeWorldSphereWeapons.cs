using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    // One retained mechanical assembly per authored family/branch/final. All
    // motion is rigid translation/rotation; no animated scale or tessellation.
    internal sealed class NativeWorldSphereWeapons : IDisposable
    {
        internal struct State
        {
            internal object Identity;
            internal string Family,Branch;
            internal int Final;
            internal double X,Y,Radius,Rotation,Time,AttackDelay;
            internal uint ShotSerial;
            internal bool Disabled,Resonance;
        }
        private sealed class Clock {internal uint Serial;internal double Started=double.NegativeInfinity,Seen;}
        private sealed class Part : IDisposable
        {
            internal Mesh Fill,Stroke,Halo;
            internal Color Color;
            internal Vector2 Rest,Slide;
            internal float Angle,Swing,Recoil,Delay;
            internal int Channel;
            public void Dispose(){Destroy(Fill);Destroy(Stroke);Destroy(Halo);}
        }
        private sealed class Rig : IDisposable
        {
            internal readonly List<Part> Parts=new List<Part>();
            public void Dispose(){foreach(var part in Parts)part.Dispose();}
        }
        private readonly Dictionary<(string,string,int),Rig> _rigs=new Dictionary<(string,string,int),Rig>();
        private readonly ConditionalWeakTable<object,Clock> _clocks=new ConditionalWeakTable<object,Clock>();
        private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();
        private readonly Material _solid=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Retained sphere mechanical parts",hideFlags=HideFlags.DontSave};
        private int _used;
        internal void BeginFrame(){_used=0;}
        internal int RigCount=>_rigs.Count;
        private static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        private static Matrix4x4 T(double x=0,double y=0,double angle=0)=>Matrix4x4.TRS(new Vector3((float)x,(float)y,0),Quaternion.Euler(0,0,(float)(angle*Mathf.Rad2Deg)),Vector3.one);
        private MaterialPropertyBlock Properties(Color c){if(_used==_properties.Count)_properties.Add(new MaterialPropertyBlock());var p=_properties[_used++];p.Clear();p.SetVector("_NativeColor",new Vector4(c.r,c.g,c.b,c.a));return p;}
        private static float Pulse(double age,double duration,int channel,float delay)
        {
            double p=age/duration-delay;if(p<0||p>=1)return 0;
            if(channel==1)return (float)(p<.18?Math.Sin(p/.18*Math.PI*.5):Math.Exp(-(p-.18)*7)*(1-p));
            if(channel==2)return (float)Math.Pow(Math.Sin(p*Math.PI),1.4);
            if(channel==3)return (float)(Math.Sin(p*Math.PI*2)*Math.Sin(p*Math.PI));
            return (float)Math.Sin(p*Math.PI);
        }
        private static Matrix4x4 Pose(Part part,double age,double duration)
        {
            float value=part.Channel==0?0:Pulse(age,duration,part.Channel,part.Delay);
            float recoil=part.Recoil==0?0:Pulse(age,duration,1,0)*part.Recoil;
            return T(part.Rest.x+part.Slide.x*value-recoil,part.Rest.y+part.Slide.y*value,part.Angle+part.Swing*value);
        }
        internal void Draw(NativeWorldSurface surface,State state)
        {
            if(state.Family!="standard"&&state.Family!="sniper"&&state.Family!="shotgun")throw new ArgumentException("Unsupported retained weapon family");
            string branch=state.Branch??"";int final=branch.Length==0?-1:Math.Max(-1,Math.Min(2,state.Final));var key=(state.Family,branch,final);
            if(!_rigs.TryGetValue(key,out var rig)){rig=Build(state.Family,branch,final);_rigs.Add(key,rig);}
            double age=double.PositiveInfinity,duration=Math.Min(.30,Math.Max(.10,state.AttackDelay*.32));
            if(state.Identity!=null){
                if(!_clocks.TryGetValue(state.Identity,out var clock)){clock=new Clock{Serial=state.ShotSerial,Seen=state.Time};_clocks.Add(state.Identity,clock);}
                if(clock.Serial!=state.ShotSerial){if(state.Time-clock.Seen<=.15)clock.Started=state.Time;clock.Serial=state.ShotSerial;}
                clock.Seen=state.Time;age=state.Time-clock.Started;
            }
            if(state.Disabled)age=double.PositiveInfinity;
            var root=T(state.X,state.Y,state.Rotation)*Matrix4x4.Scale(Vector3.one*(float)state.Radius);
            var dark=C("#030b17");float opacity=state.Disabled?.22f:1;
            foreach(var part in rig.Parts){
                var matrix=root*Pose(part,age,duration);var color=state.Disabled?C("#ff4d70"):state.Resonance?C("#ffd166"):part.Color;
                // Geometry provides the soft edge; no per-part blur resources.
                if(part.Fill!=null)surface.DrawNativeMesh(part.Fill,_solid,Properties(dark),matrix,opacity:opacity,inheritShadow:false);
                if(part.Halo!=null){var halo=color;halo.a=.14f;surface.DrawNativeMesh(part.Halo,_solid,Properties(halo),matrix,opacity:opacity,additive:true,inheritShadow:false);}
                if(part.Stroke!=null)surface.DrawNativeMesh(part.Stroke,_solid,Properties(color),matrix,opacity:opacity,additive:true,inheritShadow:false);
            }
        }
        private static Rig Build(string family,string branch,int final)
        {
            var rig=new Rig();var primary=C(family=="standard"?"#63e6ff":family=="sniper"?"#e86cff":"#ffb84d");var white=C("#e4f8ff");
            var accent=C(branch.EndsWith("resonator",StringComparison.Ordinal)||branch.EndsWith("assassin",StringComparison.Ordinal)||branch.EndsWith("burst",StringComparison.Ordinal)?"#ff9c62":branch.EndsWith("swarm",StringComparison.Ordinal)||branch.EndsWith("oracle",StringComparison.Ordinal)||branch.EndsWith("cataclysm",StringComparison.Ordinal)?"#9ce6bf":"#d9f6ff");
            Part Add(Unity.VectorGraphics.BezierContour path,bool fill,Color color,Vector2 rest=default,int channel=0,Vector2 slide=default,float swing=0,float recoil=0,float delay=0,float angle=0,float width=.033f)
            {
                var part=new Part{Color=color,Rest=rest,Channel=channel,Slide=slide,Swing=swing,Recoil=recoil,Delay=delay,Angle=angle};
                if(fill)part.Fill=Tessellate(path,0);
                part.Stroke=Tessellate(path,width,cap:Unity.VectorGraphics.PathEnding.Round,corners:Unity.VectorGraphics.PathCorner.Round);
                part.Halo=Tessellate(path,width*2.6f,cap:Unity.VectorGraphics.PathEnding.Round,corners:Unity.VectorGraphics.PathCorner.Round);
                rig.Parts.Add(part);return part;
            }
            Unity.VectorGraphics.BezierContour Poly(params Vector2[] pts)=>PolylineContour(true,pts);
            Vector2 P(float x,float y)=>new Vector2(x,y);
            Part Plate(float x,float y,float w,float h,int channel=0,float dx=0,float dy=0,float swing=0,float recoil=0,Color? color=null,float angle=0)=>Add(Poly(P(-w*.5f,-h*.5f),P(w*.5f,-h*.5f),P(w*.5f,h*.5f),P(-w*.5f,h*.5f)),true,color??primary,P(x,y),channel,P(dx,dy),swing,recoil,angle:angle);
            Part Arc(float x,float y,float rx,float ry,float start,float end,Color? color=null,int channel=0,float dx=0,float dy=0,float swing=0)=>Add(EllipseContour(Vector2.zero,rx,ry,start,end),false,color??primary,P(x,y),channel,P(dx,dy),swing);
            Part Disc(float x,float y,float radius,Color color,int channel=0,float dx=0,float dy=0,float recoil=0)=>Add(EllipseContour(Vector2.zero,radius,radius),true,color,P(x,y),channel,P(dx,dy),recoil:recoil);
            Part Fin(float x,float y,int sign,float length=.23f,int channel=2,float swing=.15f,Color? color=null,float dx=0)=>Add(Poly(P(0,0),P(length*.45f,-sign*.09f),P(length,-sign*.08f),P(length*.6f,sign*.025f)),true,color??primary,P(x,y),channel,P(dx,sign*.025f),sign*swing);
            void PairFins(float x=.99f,float y=.23f,float length=.22f,float swing=.15f,Color? color=null){Fin(x,-y,1,length,2,-swing,color);Fin(x,y,-1,length,2,-swing,color);}
            void Rail(float y,float length=.43f,float recoil=.07f,float x=1.13f)=>Plate(x,y,length,.055f,1,recoil:recoil);
            void Housing(float width=.49f,float height=.34f,float recoil=0){Add(Poly(P(-width*.50f,-height*.5f),P(width*.30f,-height*.5f),P(width*.50f,-height*.32f),P(width*.50f,height*.32f),P(width*.30f,height*.5f),P(-width*.50f,height*.5f)),true,primary,P(1.14f,0),1,recoil:recoil);}
            // Compact collar, independent side plates and moving carriage.
            Arc(.79f,0,.075f,.18f,Mathf.PI*.48f,Mathf.PI*1.52f);
            Arc(.87f,0,.06f,.145f,Mathf.PI*.48f,Mathf.PI*1.52f,white);
            if(family=="standard"){
                Housing();Rail(-.105f);Rail(.105f);PairFins();
                if(branch=="standard_resonator"){
                    Plate(1.03f,0,.07f,.30f,2,dx:-.025f,color:accent);
                    if(final==0){Arc(1.35f,0,.09f,.22f,-1.3f,1.3f,accent,2,dx:.025f);Arc(1.27f,0,.07f,.20f,-1.3f,1.3f,primary,2,dx:-.025f);}
                    else if(final==1){Fin(1.13f,-.22f,1,.27f,2,-.24f,accent);Fin(1.13f,.22f,-1,.27f,2,-.24f,accent);}
                    else if(final==2){for(int i=0;i<3;i++)Plate(1.10f+i*.09f,(i%2==0?1:-1)*.21f,.075f,.05f,3,dy:.025f, color:accent);}
                }else if(branch=="standard_singularity"){
                    Arc(1.30f,0,.10f,.20f,-1.35f,1.35f,accent,2,dx:-.045f);
                    if(final==0){Fin(1.14f,-.20f,-1,.27f,2,.24f);Fin(1.14f,.20f,1,.27f,2,.24f);}
                    else if(final==1){Plate(1.30f,-.20f,.18f,.04f,2,dy:.035f,color:accent);Plate(1.30f,.20f,.18f,.04f,2,dy:-.035f,color:accent);Plate(1.39f,0,.035f,.34f,2,dx:-.025f,color:accent);}
                    else if(final==2){Disc(1.34f,0,.075f,white,2,dx:-.04f);Fin(1.13f,-.19f,-1,.30f,2,.23f);Fin(1.13f,.19f,1,.30f,2,.23f);}
                }else if(branch=="standard_swarm"){
                    int guides=final==0?1:final==2?3:2;
                    for(int i=0;i<guides;i++){float offset=(i-(guides-1)*.5f)*.14f;float angle=(i-(guides-1)*.5f)*(final==1?.28f:.18f);float length=final<0?.23f:.34f;Add(Poly(P(0,-.045f),P(length*.68f,-.07f),P(length,0),P(length*.68f,.07f),P(0,.045f)),true,accent,P(1.18f,offset),2,P(-.035f,offset*.15f),offset*.5f,angle:angle);}
                }
            }else if(family=="sniper"){
                Plate(1.15f,0,.56f,.065f,1,recoil:.10f);Disc(.98f,0,.10f,primary,4,dx:-.035f);Disc(.98f,0,.04f,white,4,dx:.02f);PairFins(.98f,.14f,.21f,.13f);
                if(branch=="sniper_oracle"){
                    Arc(.98f,0,.15f,.21f,-1.2f,1.2f,accent,4,dx:-.035f);
                    if(final==0){Add(Poly(P(-.085f,0),P(0,-.07f),P(.085f,0),P(0,.07f)),true,white,P(1.36f,0),4,P(-.045f,0));}
                    else if(final==1){Fin(1.02f,-.18f,1,.28f,3,.18f,accent);Fin(1.02f,.18f,-1,.28f,3,.18f,accent);}
                    else if(final==2){for(int i=0;i<4;i++){float a=-1.1f+i*.73f;Plate(.98f+Mathf.Cos(a)*.23f,Mathf.Sin(a)*.23f,.075f,.055f,2,dy:Mathf.Sin(a)*.025f,swing:Mathf.Sin(a)*.24f,angle:a);}}
                }else if(branch=="sniper_assassin"){
                    Add(Poly(P(-.10f,-.045f),P(.17f,0),P(-.10f,.045f)),true,accent,P(1.38f,0),1,recoil:.10f);
                    if(final==0){Plate(1.22f,-.13f,.035f,.13f,4,dy:.025f,color:accent);Plate(1.22f,.13f,.035f,.13f,4,dy:-.025f,color:accent);}
                    else if(final==1){Fin(1.06f,-.16f,-1,.34f,2,.20f,accent);Fin(1.06f,.16f,1,.34f,2,.20f,accent);}
                    else if(final==2){Add(QuadraticContour(P(0,-.10f),false,P(.23f,-.18f),P(.27f,0),P(.23f,.18f),P(0,.10f)),false,accent,P(1.16f,0),3,P(-.045f,0),.16f);}
                }else if(branch=="sniper_beacon"){
                    Plate(1.10f,-.18f,.18f,.04f,2,dy:-.025f);Plate(1.10f,.18f,.18f,.04f,2,dy:.025f);
                    if(final==0){Plate(1.27f,-.12f,.035f,.17f,2,dy:.035f,color:accent);Plate(1.27f,.12f,.035f,.17f,2,dy:-.035f,color:accent);}
                    else if(final==1){Arc(1.05f,-.17f,.28f,.14f,-2.8f,-.3f,primary,2,dy:-.025f,swing:-.16f);Arc(1.05f,.17f,.28f,.14f,.3f,2.8f,primary,2,dy:.025f,swing:.16f);}
                    else if(final==2){for(int i=0;i<3;i++){float a=i*Mathf.PI*2/3;var pos=P(1.04f+Mathf.Cos(a)*.18f,Mathf.Sin(a)*.18f);Disc(pos.x,pos.y,.035f,accent,4,dx:-.025f);Plate((pos.x+1.04f)*.5f,pos.y*.5f,.16f,.025f,4,dx:-.025f,angle:a);}}
                }
            }else{
                Housing(.52f,.40f,.065f);PairFins(.99f,.25f,.20f,.20f);
                bool heavy=branch=="shotgun_cataclysm"||branch=="shotgun_burst"&&final==1;
                if(heavy){Disc(1.25f,0,.115f,primary,1,recoil:.07f);Disc(1.25f,0,.065f,white,1,recoil:.07f);}
                else for(int i=-1;i<=1;i++)Plate(1.33f,i*.12f,.10f,.045f,1,recoil:.065f);
                if(branch=="shotgun_burst"){
                    if(final==0||final==2){Plate(1.10f,-.24f,.16f,.05f,3,dy:-.035f);Plate(1.10f,.24f,.16f,.05f,3,dy:.035f);}
                    if(final==1){Plate(1.10f,-.24f,.29f,.055f,1,recoil:.035f);Plate(1.10f,.24f,.29f,.055f,1,recoil:.035f);}
                    if(final==2){Arc(1.35f,0,.12f,.22f,.5f,2.5f,primary,2,dx:.035f);}
                }else if(branch=="shotgun_cataclysm"){
                    if(final==0){for(int i=0;i<4;i++){float a=-1.2f+i*.8f;Plate(1.28f+Mathf.Cos(a)*.13f,Mathf.Sin(a)*.23f,.075f,.055f,2,dy:Mathf.Sin(a)*.04f,swing:Mathf.Sin(a)*.23f,angle:a);}}
                    else if(final==1){Plate(1.12f,-.255f,.30f,.07f,1,recoil:.045f,color:accent);Plate(1.12f,.255f,.30f,.07f,1,recoil:.045f,color:accent);}
                    else if(final==2){Plate(1.40f,-.13f,.035f,.19f,2,dy:.035f,color:accent);Plate(1.40f,.13f,.035f,.19f,2,dy:-.035f,color:accent);}
                }else if(branch=="shotgun_hail"){
                    int count=final<0?3:final==1?8:6;
                    for(int i=0;i<count;i++){float a=i*Mathf.PI*2/count;Plate(1.26f+Mathf.Cos(a)*.14f,Mathf.Sin(a)*.20f,.065f,.035f,2,dx:Mathf.Cos(a)*.025f,dy:Mathf.Sin(a)*.035f,swing:Mathf.Sin(a)*.12f,angle:a);}
                    if(final==1){Plate(1.02f,-.27f,.15f,.04f,3,dy:-.025f);Plate(1.02f,.27f,.15f,.04f,3,dy:.025f);}
                    if(final==2){Fin(1.05f,-.24f,1,.31f,2,-.20f,accent);Fin(1.05f,.24f,-1,.31f,2,-.20f,accent);}
                }
            }
            return rig;
        }
        private static void Destroy(UnityEngine.Object value){if(value!=null)UnityEngine.Object.Destroy(value);}
        public void Dispose(){foreach(var rig in _rigs.Values)rig.Dispose();_rigs.Clear();Destroy(_solid);}
    }
}
