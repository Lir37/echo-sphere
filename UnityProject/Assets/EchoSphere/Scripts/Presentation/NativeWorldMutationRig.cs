using System;
using Unity.VectorGraphics;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldSphereFamilies
    {
        internal struct MutationView
        {
            internal double X,Y,Radius,Time,Rotation,FiniteRotation,Final,AttackDelay,AttackTimer;
            internal string Branch;
            internal Color Color;
            internal bool Directed,Emitter;
        }
        // A typed authored layer: Unity owns meshes/tessellation/materials. Each
        // layer states its original transform and stroke/composition settings.
        private readonly struct MutationRig
        {
            private readonly NativeWorldSphereFamilies _owner;
            internal readonly Matrix4x4 Root;
            private readonly bool _additive,_inherit;
            private readonly PathEnding _cap;private readonly PathCorner _corner;
            internal MutationRig(NativeWorldSphereFamilies owner,Matrix4x4 root,bool additive,bool round,bool inherit=false)
            {_owner=owner;Root=root;_additive=additive;_inherit=inherit;_cap=round?PathEnding.Round:owner._surface.NativeStrokeEnding;_corner=round?PathCorner.Round:owner._surface.NativeStrokeCorner;}
            private MutationRig(MutationRig source,Matrix4x4 root){_owner=source._owner;Root=root;_additive=source._additive;_inherit=source._inherit;_cap=source._cap;_corner=source._corner;}
            internal MutationRig At(Matrix4x4 transform)=>new MutationRig(this,Root*transform);
            internal void Stroke(BezierContour path,Color color,double width,double? alpha,PathEnding? cap=null)
            {var art=_owner.Dynamic(new[]{path},width,cap:cap??_cap,corner:_corner);_owner.Paint(art,Root,null,color,alpha,_additive,_inherit);}
            internal void Poly(Color color,double width,double? alpha,params Vector2[] points)=>Stroke(PolylineContour(false,points),color,width,alpha);
            internal void Ring(double x,double y,double rx,double ry,Color? fill,Color? stroke,double width,double? alpha,double start=0,double end=Math.PI*2)
            {var path=EllipseContour(P(x,y),(float)rx,(float)ry,(float)start,(float)end);var art=_owner.Dynamic(new[]{path},stroke.HasValue?width:0,fill.HasValue,_cap,corner:_corner);_owner.Paint(art,Root,fill,stroke,alpha,_additive,_inherit);}
            internal void Fin(double x,double y,double length,double width,Color color,double angle,double alpha=.75)
            {var path=QuadraticContour(P(0,-width*.20),true,P(length*.42,-width*.82),P(length,-width*.06),P(length*.48,width*.32),P(0,width*.22));var art=_owner.Dynamic(new[]{path},Math.Max(.7,width*.07),true,_cap,corner:_corner);_owner.Paint(art,Root*T(x,y,angle),C(3,8,15,.92),color,alpha,_additive,_inherit);}
            internal void Shard(double x,double y,double size,Color color,double angle,double alpha=.8)
            {var art=_owner.Get(500,size,0,Math.Max(.55,size*.11),()=>new[]{PolylineContour(true,P(size,0),P(0,-size*.54),P(-size*.68,-size*.25),P(-size*.44,size*.28))},true,_cap,_corner);_owner.Paint(art,Root*T(x,y,angle),C(4,9,16,.94),color,alpha,_additive,_inherit);}
            internal void Arc(double radius,Color color,double width,double start,double end,double alpha=.65)=>Stroke(EllipseContour(Vector2.zero,(float)radius,(float)radius,(float)start,(float)end),color,width,alpha,PathEnding.Round);
            internal void Tether(double sx,double sy,double ex,double ey,double bend,Color color,double width,double alpha=.65,double phase=0)
            {double dx=ex-sx,dy=ey-sy,len=Math.Sqrt(dx*dx+dy*dy);if(len==0)len=1;double offset=bend+Math.Sin(phase)*width*2;Stroke(QuadraticContour(P(sx,sy),false,P((sx+ex)/2-dy/len*offset,(sy+ey)/2+dx/len*offset),P(ex,ey)),color,width,alpha,PathEnding.Round);}
            internal void Pulse(double r,Color color,double t,double scale=1){double q=(t*.62)%1;Arc(r*(.72+q*1.02),color,Math.Max(.7,r*.028),0,Math.PI*2,.44*(1-q)*scale);}
        }
        internal void DrawMutation(SourceCanvasSurface surface,MutationView s)
        {
            if(string.IsNullOrEmpty(s.Branch))return;_surface=surface;_last=null;var root=T(s.X,s.Y,s.Directed?s.Rotation:0);MutationGlow(surface,s.Radius*1.75,s.Color,.055,root);
            MutationSilhouette(new MutationRig(this,root,false,true),s);MutationExpansion(new MutationRig(this,root,true,true),s);if(s.Emitter)MutationEmitter(new MutationRig(this,root,false,false,true),s);MutationAttack(new MutationRig(this,root,true,false),s);Finish();
        }
        private void MutationSilhouette(MutationRig rig,MutationView s)
        {
            double r=s.Radius,t=s.Time,f=s.Final;string b=s.Branch;var c=s.Color;const double tau=Math.PI*2;
            if(b=="standard_resonator"){for(int y=-1;y<=1;y+=2)rig.Fin(-r*.10,y*r*.62,r*.72,r*.16,c,y>0?.55:-.55,.70);rig.Arc(r*1.22,c,1.1,-.9,.9,.60);}
            else if(b=="standard_singularity"){rig.Arc(r*(1.16+f*.08),C(216,185,255),1.4,.2,5.9,.72);for(int i=0;i<2;i++){double a=i==0?-.55:.55;rig.Tether(Math.Cos(a)*r*.92,Math.Sin(a)*r*.52,0,0,r*.18,c,1.2,.55,a);}}
            else if(b=="standard_swarm")for(int y=-1;y<=1;y+=2){rig.Shard(r*1.02,y*r*.56,r*.18,c,y>0?.35:-.35,.78);rig.Shard(r*.72,y*r*.76,r*.13,c,y>0?.8:-.8,.64);}
            else if(b=="sniper_oracle"){rig.Arc(r*1.18,C(245,221,255),1.5,-.95,.95,.82);for(int y=-1;y<=1;y+=2)rig.Fin(r*.42,y*r*.30,r*.72,r*.13,c,y>0?.12:-.12,.88);rig.Poly(C(245,221,255),1,.78,P(r*.78,-r*.08),P(r*1.72,0),P(r*.78,r*.08));}
            else if(b=="sniper_assassin"){for(int y=-1;y<=1;y+=2)rig.Fin(r*.40,y*r*.28,r*.82,r*.16,c,y>0?.34:-.34,.92);rig.Arc(r*1.42,c,1.1,-.42,.42,.70);}
            else if(b=="sniper_beacon"){for(int y=-1;y<=1;y+=2)rig.Fin(-r*.18,y*r*.48,r*.58,r*.15,c,y>0?-.95:.95,.82);rig.Arc(r*1.34,c,1.3,-.65,.65,.72);rig.Arc(r*1.56,c,.8,-.28,.28,.55);}
            else if(b=="shotgun_burst"){for(int y=-1;y<=1;y++)rig.Poly(c,2,.78,P(r*.34,y*r*.22),P(r*1.16,y*r*.22*(1+.08*f)));rig.Arc(r*1.26,c,1.2,-.42,.42,.55);}
            else if(b=="shotgun_cataclysm"){rig.Arc(r*1.30,C(255,210,156),2,-.34,.34,.86);for(int y=-1;y<=1;y+=2)rig.Fin(r*.30,y*r*.32,r*.72,r*.18,c,y>0?.18:-.18,.90);rig.Ring(r*1.30,0,r*(.18+.04*f),r*(.18+.04*f),C(255,169,77,.28),null,0,null);}
            else if(b=="shotgun_hail"){for(int i=0;i<5;i++)rig.Shard(r*(.78+.06*(i%2)),(i-2)*r*.24,r*.12,c,0,.72);rig.Arc(r*1.12,c,1,-.72,.72,.56);}
            else if(b=="chain_web"){for(int y=-1;y<=1;y+=2)rig.Tether(-r*.25,y*r*.20,r*1.10,y*r*.72,r*.18,c,1.1,.62,y*.8);rig.Arc(r*1.15,c,1,-.9,.9,.58);}
            else if(b=="chain_storm")for(int i=0;i<5;i++){double a=-.8+i*.4;rig.Shard(r*(.86+.12*(i%2)),Math.Sin(a)*r*.55,r*.12,c,a,.72);}
            else if(b=="chain_leech"){for(int y=-1;y<=1;y+=2)rig.Fin(r*.15,y*r*.45,r*.62,r*.16,c,y>0?.7:-.7,.76);rig.Arc(r*1.12,c,1,-1,1,.58);}
            else if(b.StartsWith("aura_",StringComparison.Ordinal)){rig.Arc(r*(1.18+f*.08),c,1.5,-Math.PI*.82,Math.PI*.82,.70);for(int y=-1;y<=1;y+=2)rig.Tether(0,y*r*.20,r*.96,y*r*.66,r*.16,c,1,.48,y*.7);}
            else if(b.StartsWith("orbital_",StringComparison.Ordinal)){int n=f==2?5:4;for(int i=0;i<n;i++){double a=t*(b=="orbital_dance"?1.4:.55)+i*tau/n;rig.Shard(Math.Cos(a)*r*1.18,Math.Sin(a)*r*.72,r*.13,c,a,.74);}}
            else if(b.StartsWith("prism_",StringComparison.Ordinal)){int n=b=="prism_spectrum"?5:b=="prism_split"?3:2;for(int i=0;i<n;i++)rig.Shard(r*1.12,(i-(n-1)/2.0)*r*.28,r*.12,c,0,.74);}
            else if(b.StartsWith("gravity_",StringComparison.Ordinal)){rig.Arc(r*(1.12+f*.10),c,1.25,0,tau,.64);for(int i=0;i<3;i++)rig.Tether(r*.18,0,r*(.72+i*.18),0,r*.10,c,1,.46,t+i);}
            else if(b.StartsWith("pulse_",StringComparison.Ordinal)){int lobes=b=="pulse_resonator"?8:b=="pulse_wave"?6:5;var points=new Vector2[37];for(int i=0;i<=36;i++){double a=i/36.0*tau,rr=r*(1.02+.12*Math.Sin(a*lobes+t*2.4));points[i]=P(Math.Cos(a)*rr,Math.Sin(a)*rr*.54);}rig.Poly(c,1.25,.62,points);}
            else if(b.StartsWith("void_",StringComparison.Ordinal)){for(int y=-1;y<=1;y+=2)rig.Fin(r*.36,y*r*.24,r*.72,r*.18,c,y>0?.22:-.22,.78);rig.Arc(r*(1.15+f*.08),c,1.2,.15,tau-.15,.64);}
            if(f>=0){double fs=1+f*.08;rig.Ring(r*.92,0,r*.30*fs,r*.62*fs,null,c,1,.42);double a=t*(.35+f*.08);rig.Shard(r*1.20*Math.Cos(a),r*.72*Math.Sin(a),r*.11,c,a,.76);}
        }
    }
}
