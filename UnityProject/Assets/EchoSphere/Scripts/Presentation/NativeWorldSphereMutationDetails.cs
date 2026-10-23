using System;
using System.Globalization;
using Unity.VectorGraphics;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldSphereFamilies
    {
        private readonly Material _mutationGlow=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldMutationGlow")){name="Native mutation glow",hideFlags=HideFlags.DontSave};
        internal void MutationFin(NativeWorldSurface surface,double x,double y,double length,double width,Color color,double angle,double opacity,Matrix4x4? parent=null,bool additive=false)
        {
            _surface=surface;var path=QuadraticContour(P(0,-width*.20),true,P(length*.42,-width*.82),P(length,-width*.06),P(length*.48,width*.32),P(0,width*.22));var local=(parent??Matrix4x4.identity)*T(x,y,angle);
            Paint(Dynamic(new[]{path},Math.Max(.7,width*.07),true),local,C(3,8,15,.92),color,opacity,additive,!additive);
        }
        internal void MutationShard(NativeWorldSurface surface,double x,double y,double size,Color color,double angle,double opacity,Matrix4x4? parent=null,bool additive=false)
        {
            _surface=surface;var art=Get(500,size,0,Math.Max(.55,size*.11),()=>new[]{PolylineContour(true,P(size,0),P(0,-size*.54),P(-size*.68,-size*.25),P(-size*.44,size*.28))},true);Paint(art,(parent??Matrix4x4.identity)*T(x,y,angle),C(4,9,16,.94),color,opacity,additive,!additive);
        }
        internal void MutationArc(NativeWorldSurface surface,double radius,Color color,double width,double start,double end,double opacity,Matrix4x4? parent=null,bool additive=false)
        {_surface=surface;Paint(Dynamic(new[]{EllipseContour(Vector2.zero,(float)radius,(float)radius,(float)start,(float)end)},width,cap:PathEnding.Round),parent??Matrix4x4.identity,null,color,opacity,additive,!additive);}
        internal void MutationTether(NativeWorldSurface surface,double sx,double sy,double ex,double ey,double bend,Color color,double width,double opacity,double phase)
        {
            _surface=surface;double dx=ex-sx,dy=ey-sy,len=Math.Sqrt(dx*dx+dy*dy);if(len==0)len=1;double offset=bend+Math.Sin(phase)*width*2;
            Paint(Dynamic(new[]{QuadraticContour(P(sx,sy),false,P((sx+ex)/2-dy/len*offset,(sy+ey)/2+dx/len*offset),P(ex,ey))},width,cap:PathEnding.Round),Matrix4x4.identity,null,color,opacity,inheritBlend:true);
        }
        internal void MutationEye(NativeWorldSurface surface,double x,double y,double size,Color color,double angle)
        {
            _surface=surface;var root=T(x,y,angle);Paint(Get(501,size,0,Math.Max(.6,size*.14),()=>new[]{QuadraticContour(P(-size,0),false,P(0,-size*.55),P(size,0),P(0,size*.55),P(-size,0))}),root,null,color,.9,true);
            Paint(Get(502,size,0,0,()=>new[]{EllipseContour(Vector2.zero,(float)(size*.23),(float)(size*.48))},true),root,Color.white,null,.9,true);Paint(Get(503,size,0,0,()=>new[]{EllipseContour(Vector2.zero,(float)(size*.10),(float)(size*.32))},true),root,color,null,.9,true);
        }
        internal void MutationGlow(NativeWorldSurface surface,double radius,Color color,double alpha,Matrix4x4? parent=null)
        {
            _surface=surface;var art=Circle(radius);var p=Properties(color);p.SetFloat("_Radius",(float)radius);float Rounded(double n)=>float.Parse(n.ToString("F3",CultureInfo.InvariantCulture),CultureInfo.InvariantCulture);p.SetVector("_StopAlphas",new Vector4(Mathf.Clamp01(Rounded(alpha*3.4)),Mathf.Clamp01(Rounded(alpha*1.15)),Mathf.Clamp01(Rounded(alpha*.25)),0));surface.DrawNativeMesh(art.Fill,_mutationGlow,p,parent,inheritShadow:true,additive:true);
        }
        internal struct ProjectileView
        {internal double X,Y,Vx,Vy,Radius,Final;internal string Family,Branch;internal Color Color;}
        internal void DrawProjectile(NativeWorldSurface surface,ProjectileView p)
        {
            _surface=surface;double r=Math.Max(2,p.Radius),speed=Math.Sqrt(p.Vx*p.Vx+p.Vy*p.Vy);if(speed==0)speed=1;double len=Math.Min(58,14+speed*.045);var root=T(p.X,p.Y,Math.Atan2(p.Vy,p.Vx));var color=p.Color;
            var tail=QuadraticContour(Vector2.zero,false,P(-len*.35,-r*.82),P(-len*.86,-r*.12),P(-len,0),P(-len*.86,r*.12),P(-len*.35,r*.82),Vector2.zero);Paint(Dynamic(new[]{tail},fill:true),root,color,null,.17,inheritBlend:true);
            double inheritedWidth=Math.Max(1,r*.65);Paint(Dynamic(new[]{Line(-len*.88,0,-r*.10,0)},inheritedWidth),root,null,A(color,.80),.10,inheritBlend:true);Paint(Circle(r*1.65),root,color,null,.18,true);
            void Stroke(int id,double width,Func<BezierContour[]> create,Color? tint=null)=>Paint(Get(id,r,0,width,create),root,null,tint??color,1,true);
            switch(p.Family){
                case "standard":
                    Stroke(510,Math.Max(.7,r*.17),()=>new[]{Line(-r*.9,0,r*1.35,0)});Stroke(511,Math.Max(.7,r*.17),()=>new[]{EllipseContour(P(r*.22,0),(float)(r*.30),(float)(r*.30))});if(p.Branch=="standard_singularity")Paint(Circle(r*.62),root,C(5,2,11),null,1,true);if(p.Branch=="standard_swarm")for(int q=-1;q<=1;q++)MutationShard(surface,q*r*.42,0,r*.25,color,q*.2,.8,root,true);break;
                case "sniper":
                    Stroke(512,Math.Max(.7,r*.14),()=>new[]{Line(-r*.75,0,r*1.35,0)});if(p.Branch=="sniper_oracle")Stroke(513,Math.Max(.7,r*.14),()=>new[]{EllipseContour(P(r*.35,0),(float)(r*.50),(float)(r*.50))});else if(p.Branch=="sniper_assassin")MutationFin(surface,r*.18,0,r*(.85+.15*p.Final),r*.18,color,0,.9,root,true);else MutationArc(surface,r,color,.8,-.8,.8,.9,root,true);break;
                case "shotgun":{
                    int count=p.Branch=="shotgun_cataclysm"?1:p.Branch=="shotgun_hail"?4:3;for(int i=0;i<count;i++){double y=(i-(count-1)/2.0)*r*.34;Paint(Dynamic(new[]{Line(-r*.70,y,r*.92,y)},inheritedWidth),root,null,A(color,.80),1,true);}Paint(Get(514,r,0,0,()=>new[]{EllipseContour(P(r*.58,0),(float)(r*.18),(float)(r*.18))},true),root,Color.white,null,1,true);break;}
                case "prism":{
                    int count=p.Branch=="prism_split"?3:p.Branch=="prism_spectrum"?5:2;for(int i=0;i<count;i++)MutationShard(surface,0,(i-(count-1)/2.0)*r*.38,r*.34,color,0,.75,root,true);Stroke(515,.7,()=>new[]{Line(-r*.28,0,r*1.04,0)},Color.white);break;}
                case "void":
                    if(p.Branch=="void_hunger"){Paint(Circle(r*.68),root,C(8,3,15),null,1,true);MutationArc(surface,r*.42,color,.9,.2,5.4,.9,root,true);}else if(p.Branch=="void_reaper")MutationFin(surface,0,0,r*.95,r*.24,color,0,.9,root,true);else{Stroke(516,1,()=>new[]{Line(-r*.44,0,r*.9,0)});Stroke(517,1,()=>new[]{PolylineContour(false,P(r*.55,-r*.25),P(r*.9,0),P(r*.55,r*.25))});}break;
            }

        }
        internal bool DrawChainLightning(NativeWorldSurface surface,double fromX,double fromY,double toX,double toY,Color color,string branch,double final,double alpha,double time)
        {
            if(string.IsNullOrEmpty(branch))return false;_surface=surface;double dx=toX-fromX,dy=toY-fromY,len=Math.Sqrt(dx*dx+dy*dy);if(len==0)len=1;double nx=-dy/len,ny=dx/len;int pieces=branch=="chain_web"?10:branch=="chain_storm"?8:12;var points=new Vector2[pieces+1];points[0]=P(fromX,fromY);for(int i=1;i<pieces;i++){double q=i/(double)pieces,amp=len*(branch=="chain_storm"?.12:branch=="chain_leech"?.06:.09),zig=(i%2!=0?-1:1)*amp*(.7+.3*Math.Sin(time*22+i));points[i]=P(fromX+dx*q+nx*zig,fromY+dy*q+ny*zig);}points[pieces]=P(toX,toY);
            var path=PolylineContour(false,points);Paint(Dynamic(new[]{path},Math.Max(4,len*.015),cap:PathEnding.Round),Matrix4x4.identity,null,A(color,Mathf.Clamp01((float)(.18*alpha))),additive:true);Paint(Dynamic(new[]{path},Math.Max(1.2,len*.0045),cap:PathEnding.Round),Matrix4x4.identity,null,C(245,255,255,Mathf.Clamp01((float)(.88*alpha))),additive:true);
            if(branch=="chain_web")for(int i=0;i<4;i++){double a=time*2+i*Math.PI*2/4;Paint(Dynamic(new[]{Line(toX,toY,toX+Math.Cos(a)*(8+final*2),toY+Math.Sin(a)*(6+final))},1,cap:PathEnding.Round),Matrix4x4.identity,null,C(255,244,150,Mathf.Clamp01((float)(.68*alpha))),additive:true);}
            else if(branch=="chain_storm")Paint(Circle(3+final*.8),T(toX,toY),C(255,249,190,Mathf.Clamp01((float)(.9*alpha))),null,additive:true);
            else{double radius=3+Math.Sin(time*16)*.8;Paint(Dynamic(new[]{EllipseContour(P(toX,toY),(float)radius,(float)radius)},1,cap:PathEnding.Round),Matrix4x4.identity,null,C(255,255,255,Mathf.Clamp01((float)(.7*alpha))),additive:true);}return true;
        }
    }
}
