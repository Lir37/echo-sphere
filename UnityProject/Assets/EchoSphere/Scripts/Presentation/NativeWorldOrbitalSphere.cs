using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.VectorGraphics;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldSphereFamilies
    {
        internal struct EnemyPoint {internal double X,Y,Hp;}
        internal struct OrbitalInput
        {
            internal object Identity;internal State View;
            internal double AuraTimer,RadiusScale;
            internal string Element;
            internal Color ElementColor;
            internal bool Blade;
            internal double[] InnerAngles,OuterAngles;
            internal EnemyPoint[] Enemies;
        }
        private sealed class OrbitalAnimation {internal double LastTime,LastAura,Attack,Resonance,Phase;}
        private readonly ConditionalWeakTable<object,OrbitalAnimation> _orbital=new ConditionalWeakTable<object,OrbitalAnimation>();
        private static bool Finite(double value)=>!double.IsNaN(value)&&!double.IsInfinity(value);
        private OrbitalAnimation UpdateOrbital(OrbitalInput input)
        {
            var s=input.View;double tau=Math.PI*2;if(!_orbital.TryGetValue(input.Identity,out var v)){v=new OrbitalAnimation {LastAura=Finite(input.AuraTimer)?input.AuraTimer:0,Phase=((s.X*.011+s.Y*.007)%tau+tau)%tau};_orbital.Add(input.Identity,v);}
            if(v.LastTime==s.Time&&v.LastTime!=0)return v;double dt=v.LastTime>0?Math.Min(.05,Math.Max(0,s.Time-v.LastTime)):0;v.LastTime=s.Time;
            if(!s.Disabled){double aura=Finite(input.AuraTimer)?input.AuraTimer:0;if(aura>v.LastAura+.12)v.Attack=.28;v.LastAura=aura;v.Attack=Math.Max(0,v.Attack-dt);if(s.Resonance)v.Resonance=1.15;v.Resonance=Math.Max(0,v.Resonance-dt);}return v;
        }
        internal void DrawOrbital(NativeWorldSurface surface,NativeWorldStandardSphere cores,OrbitalInput input,bool attackers)
        {
            var s=input.View;if(attackers&&s.Disabled)return;_surface=surface;var v=UpdateOrbital(input);double r=24*s.Scale,t=s.Time,mult=Finite(input.RadiusScale)&&input.RadiusScale>0?input.RadiusScale:1;bool resonance=!s.Disabled&&v.Resonance>0;Color color=s.Disabled?C(255,77,112):resonance?C(255,209,102):C(142,240,255);var root=T(s.X,s.Y);
            if(attackers){OrbitalAttackers(root,input,v,r,color);return;}
            DrawGlow(surface,r*2.55,color,s.Disabled?.05:.13,root,true);DrawOrbit(surface,r*1.05*mult,color,t*.18+v.Phase,s.Disabled?.12:.66,"orbital",.5+.5*Math.Sin(t*3+v.Phase),root);DrawOrbit(surface,r*.88*mult,color,-t*.22+v.Phase*.5,s.Disabled?.08:.38,"orbital",.5,root);cores.DrawCore(surface,r*.70,s.Disabled?.5:.5+.5*Math.Sin(t*3+v.Phase),root,color);
            var terminal=Get(400,r,0,1,()=>new[]{PolylineContour(true,P(0,-r*.28),P(r*.16,-r*.05),P(r*.11,r*.24),P(0,r*.31),P(-r*.11,r*.24),P(-r*.16,-r*.05))},true);var inset=Get(401,r,0,0,()=>new[]{PolylineContour(true,P(0,-r*.16),P(r*.07,0),P(0,r*.14),P(-r*.07,0))},true);
            for(int i=0;i<2;i++){double a=s.Disabled?(i==0?.20:.16):(i==0?.76:.62);var local=root*T(0,(i==0?-1:1)*r*1.02,(i==0?t:-t)*.08);Paint(terminal,local,C(7,17,29),color,a);Paint(inset,local,Color.white,null,a*.72);}
            if(resonance)Paint(Get(402,r,0,1,()=>new[]{EllipseContour(Vector2.zero,(float)(r*1.34),(float)(r*1.34),0,Mathf.PI*1.15f)}),root*T(angle:t*.65),null,C(255,209,102),.42);
            if(s.Disabled){Paint(Get(403,r,0,1.5,()=>new[]{Line(-r*.72,-r*.72,r*.72,r*.72),Line(r*.72,-r*.72,-r*.72,r*.72)}),root,null,C(255,77,112),.50);Paint(Get(1,r,0,1.7,()=>new[]{EllipseContour(Vector2.zero,(float)(r*1.18),(float)(r*1.18),Mathf.PI*.20f,Mathf.PI*.80f)}),root,null,color,.42);}
        }
        private void Satellite(Matrix4x4 root,double size,Color color,double alpha,double angle,bool blade)
        {
            var local=root*T(angle:blade?angle:angle+Math.PI/4);if(blade){Paint(Get(410,size,0,1.05,()=>new[]{PolylineContour(true,P(-size*.56,-size*.14),P(size*.78,0),P(-size*.34,size*.15),P(-size*.02,0))},true),local,C(7,17,29),color,alpha,true);
                Paint(Get(411,size,0,.72,()=>new[]{Line(-size*.30,0,size*.54,0)}),local,null,Color.white,alpha*.72,true);Paint(Get(412,size,0,.72,()=>new[]{PolylineContour(false,P(-size*.42,-size*.10),P(-size*.62,0),P(-size*.42,size*.10))}),local,null,Color.white,alpha*.50,true);
            }else{
                Paint(Circle(size*.62),local,A(color,alpha*.30),null,alpha,true);
                Paint(Get(413,size,0,1,()=>{var points=new Vector2[8];for(int i=0;i<8;i++){double a=-Math.PI/8+i*Math.PI*2/8,rr=i%2==0?size*.56:size*.44;points[i]=P(Math.Cos(a)*rr,Math.Sin(a)*rr);}return new[]{PolylineContour(true,points)};},true),local,C(7,17,29),color,alpha*.92);
                Paint(Get(414,size,0,0,()=>new[]{EllipseContour(P(-size*.10,-size*.12),(float)(size*.16),(float)(size*.16))},true),local,Color.white,null,alpha*.90);
            }
        }
        private void OrbitalTrail(Matrix4x4 root,string element,double size,double angle,double alpha,bool blade,Color color)
        {
            var local=root*T(angle:angle);double scale=blade?1:.86;
            Art Stroke(int id,double width,Func<BezierContour[]> create)=>Get(id,size,scale,width,create,cap:PathEnding.Round,corner:PathCorner.Round);
            if(element=="fire"){
                Paint(Stroke(420,Math.Max(1.1,size*.12),()=>new[]{QuadraticContour(P(-size*.34*scale,0),false,P(-size*.86*scale,-size*.18),P(-size*1.28*scale,0),P(-size*.86*scale,size*.18),P(-size*.34*scale,0))}),local,null,color,alpha*.82,true);
                Paint(Stroke(421,Math.Max(.9,size*.08),()=>new[]{PolylineContour(false,P(-size*.50,-size*.08),P(-size*.96,0),P(-size*.54,size*.10))}),local,null,color,alpha*.68,true);
            }else if(element=="freeze"){
                for(int i=0;i<2;i++){int side=i==0?-1:1;Paint(Stroke(422+i,Math.Max(1,size*.10),()=>new[]{PolylineContour(false,P(-size*.26*scale,side*size*.05),P(-size*.88*scale,side*size*.18),P(-size*1.28*scale,side*size*.03))}),local,null,color,alpha*.86,true);}
                Paint(Stroke(424,Math.Max(1,size*.10),()=>new[]{PolylineContour(false,P(-size*.78,-size*.22),P(-size*1.02,0),P(-size*.78,size*.22))}),local,null,color,alpha*.78,true);
            }else{
                Paint(Stroke(425,Math.Max(1,size*.095),()=>new[]{QuadraticContour(P(-size*.30*scale,0),false,P(-size*.74,-size*.20),P(-size*1.15,-size*.04),P(-size*.86,size*.20),P(-size*.30*scale,0))}),local,null,color,alpha*.84,true);
                for(int i=0;i<3;i++){int index=i;double rad=Math.Max(.9,size*(.08-i*.015));Paint(Get(426+i,size,0,0,()=>new[]{EllipseContour(P(-size*(.58+index*.25),(index-1)*size*.14),(float)rad,(float)rad)},true),local,A(color,alpha*.42),null,alpha*.64,true);}
            }
        }
        private void OrbitalAttackers(Matrix4x4 root,OrbitalInput input,OrbitalAnimation v,double r,Color color)
        {
            var s=input.View;double mult=Finite(input.RadiusScale)&&input.RadiusScale>0?input.RadiusScale:1,inner=r*1.68*mult,outer=r*2.02*mult;Color satellite=input.Element==null?color:input.ElementColor;
            void Ring(double[] angles,double radius,bool innerRing){for(int i=0;i<angles.Length;i++){double a=angles[i],x=Math.Cos(a)*radius,y=Math.Sin(a)*radius,depth=.74+.26*((Math.Sin(a)+1)*.5);Satellite(root*T(x,y),r*.20,satellite,depth,a,input.Blade);
                if(input.Element!=null){var local=root*T(x,y);Paint(Circle(r*.18,Math.Max(1.25,r*.075)),local,null,input.ElementColor,depth*.92,true);Paint(Circle(r*.095),local,A(input.ElementColor,depth*.22),null,depth*.92,true);
                    // Preserve the source trail's sphere-local origin; the source
                    // call occurs after restoring the satellite translation.
                    OrbitalTrail(root,input.Element,r*.20,a,depth,input.Blade,input.ElementColor);}
                if(innerRing&&i==0&&v.Attack>0){double q=1-v.Attack/.28;Paint(Get(430,r,0,1,()=>new[]{Line(-r*.32,0,-r*.04,0)}),root*T(x,y,a+Math.PI/2),null,color,(1-q)*.32,true);}}}
            Ring(input.InnerAngles,inner,true);Ring(input.OuterAngles,outer,false);
            if(v.Attack>0){double q=1-v.Attack/.28,phase=s.Rotation+(1-Math.Pow(1-q,3))*.95,sx=Math.Cos(phase)*inner,sy=Math.Sin(phase)*inner,ix=sx,iy=sy,near=double.PositiveInfinity;foreach(var enemy in input.Enemies){if(enemy.Hp<=0)continue;double ex=enemy.X-s.X-sx,ey=enemy.Y-s.Y-sy,d=Math.Sqrt(ex*ex+ey*ey);if(d<near&&d<=r*1.30){near=d;ix=sx+ex;iy=sy+ey;}}Paint(Circle(r*.07),root*T(ix,iy),Color.white,null,.65*(1-q),true);}
        }
    }
}
