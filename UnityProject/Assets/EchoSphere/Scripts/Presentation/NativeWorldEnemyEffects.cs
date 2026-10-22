using System;
using UnityEngine;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldCreatures
    {
        internal struct BossSignal
        {
            internal bool Boss,Charging;
            internal string Type;
            internal double Radius,ChargeTimer,ChargeX,ChargeY,ShootTimer,SummonTimer,AuraRadius,TargetX,TargetY;
            internal double ShooterWindow,SummonerWindow,AuraWindow;
        }
        private void FinishPath(){if(_last==null)return;if(_last.Multi!=null)_surface.ContinueNativeContours(_last.Multi,_lastTransform);else _surface.ContinueNativeContour(_last.Contour,_lastTransform);}
        private static Unity.VectorGraphics.BezierContour Straight(Vector2 a,Vector2 b)=>Quadratic(a,false,(a+b)*.5f,b);
        internal void DrawOuterGlow(SourceCanvasSurface surface,double radius,Color color,double alpha,Matrix4x4? parent=null)
        {
            _surface=surface;var root=parent??Matrix4x4.identity;var outer=Get(0,radius,radius,0,()=>Oval(radius,radius));Paint(outer,root,color,null,(float)(alpha*.16),true);
            double r=radius*.52;var inner=Get(0,r,r,0,()=>Oval(r,r));Paint(inner,root,color,null,(float)(alpha*.55),true);FinishPath();
        }
        internal void DrawGroundShadow(SourceCanvasSurface surface,double rx,double ry)
        {
            _surface=surface;var outer=Get(0,rx,ry,0,()=>Oval(rx,ry));Paint(outer,T(y:5),C(0,0,0,.34),null,.42f);
            double x=rx*.72,y=ry*.68;var inner=Get(0,x,y,0,()=>Oval(x,y));Paint(inner,T(y:5),C(0,0,0,.34),null,.16f);FinishPath();
        }
        internal void DrawElite(SourceCanvasSurface surface,double r,Color color,double t,Matrix4x4? parent=null)
        {
            _surface=surface;double pulse=.70+.30*Math.Sin(t*5);var ring=Get(30,r,0,1.5,()=>Oval(r*1.30,r*1.30,Math.PI*1.64),false);var root=parent??Matrix4x4.identity;var ringTransform=root*T(angle:-Math.PI*.82);Paint(ring,ringTransform,null,A(color,.38+.20*pulse),forceOver:true);
            var square=Get(31,0,0,0,()=>Poly(P(-2.5,-2.5),P(2.5,-2.5),P(2.5,2.5),P(-2.5,2.5)));
            for(int i=-1;i<=1;i++){double a=-Math.PI*.5+i*Math.PI*.5;Paint(square,root*T(Math.Cos(a)*r*1.30,Math.Sin(a)*r*1.30),color,null,forceOver:true);}
            // fillRect does not replace the current Canvas path.
            _last=ring;_lastTransform=ringTransform;FinishPath();
        }
        internal void DrawStatus(SourceCanvasSurface surface,double r,double t,bool fire,bool poison,bool freeze,Matrix4x4? parent=null)
        {
            _surface=surface;_last=null;var identity=parent??Matrix4x4.identity;
            if(fire){double p=.5+.5*Math.Sin(t*10);for(int i=0;i<4;i++){double a=t*2.4+i*Math.PI/2,rr=r*(.8+.18*Math.Sin(t*5+i)),radius=2.2+p*1.4;var art=Dynamic(Oval(radius,radius));Paint(art,identity*T(Math.Cos(a)*rr,Math.Sin(a)*rr*.65-r*.18),C(255,102,61,.28+.18*p),null,forceOver:true);}
                var flame=Dynamic(Quadratic(P(-r*.35,-r*.65),false,P(0,-r*(1.15+p*.15)),P(r*.35,-r*.65)),1.4,false);Paint(flame,identity,null,C(255,190,90,.25+.2*p),forceOver:true);}
            if(poison){for(int i=0;i<5;i++){double a=t*.65+i*1.23,rr=r*(.85+.18*((t*.7+i*.2)%1)),rad=2+i%2;var mote=Get(0,rad,rad,0,()=>Oval(rad,rad));Paint(mote,identity*T(Math.Cos(a)*rr,Math.Sin(a)*rr*.55-r*.12),C(101,227,143,.38),null,forceOver:true);}
                var arc=Get(32,r,0,1,()=>Quadratic(P(-r*.7,r*.35),false,P(0,r*.72),P(r*.7,r*.35)),false);Paint(arc,identity,null,C(113,245,157,.35),forceOver:true);}
            if(freeze){var ray=Get(33,r,0,1.3,()=>Straight(Vector2.zero,P(r*.22,0)),false);for(int i=0;i<6;i++){double a=t*.8+i*Math.PI/3;Paint(ray,identity*T(Math.Cos(a)*r*1.12,Math.Sin(a)*r*.72,a),null,C(140,233,255,.72),forceOver:true);}var center=Get(0,r*.22,r*.22,0,()=>Oval(r*.22,r*.22));Paint(center,identity,C(210,250,255,.32),null,forceOver:true);}
            FinishPath();
        }
        internal void DrawBossSignal(SourceCanvasSurface surface,BossSignal e,double t,Matrix4x4? parent=null)
        {
            if(!e.Boss)return;_surface=surface;_last=null;var root=parent??Matrix4x4.identity;double r=e.Radius,pulse=.55+Math.Sin(t*14)*.16;
            if(e.Type=="charger"&&e.Charging){bool windup=e.ChargeTimer>.35;double reach=r*(windup?2.7:4.2);var line=Dynamic(Straight(Vector2.zero,P(e.ChargeX*reach,e.ChargeY*reach)),windup?Math.Max(2,r*.025):Math.Max(3,r*.04),false);Paint(line,root,null,windup?C(255,184,77,.58+pulse*.25):C(255,98,87,.70+pulse*.20));
                if(windup){double nx=-e.ChargeY,ny=e.ChargeX,tip=r*2.05,half=r*.55;var cone=Dynamic(Poly(P(e.ChargeX*r*.82,e.ChargeY*r*.82),P(e.ChargeX*tip+nx*half,e.ChargeY*tip+ny*half),P(e.ChargeX*tip-nx*half,e.ChargeY*tip-ny*half)));Paint(cone,root,C(255,184,77,.08+pulse*.05),null);}}
            if(e.Type=="shooter"&&e.ShootTimer<=e.ShooterWindow){double angle=Math.Atan2(e.TargetY,e.TargetX);var line=Get(34,r,0,Math.Max(1.5,r*.018),()=>Straight(P(r*1.05,0),P(r*4.8,0)),false);for(int k=-1;k<=1;k++)Paint(line,root*T(angle:angle+k*.11),null,C(255,106,95,.46+pulse*.22));}
            if(e.Type=="summoner"&&e.SummonTimer<=e.SummonerWindow){double progress=1-Math.Max(0,e.SummonTimer)/e.SummonerWindow,rad=r*(1.15+progress*2.2);var ring=Dynamic(Oval(rad,rad),Math.Max(1.5,r*.02),false);Paint(ring,root,null,C(162,124,255,.34+pulse*.18));double rr=Math.Max(2,r*.045);var mote=Get(0,rr,rr,0,()=>Oval(rr,rr));for(int i=0;i<3;i++){double a=t*2.2+i/3.0*Math.PI*2;Paint(mote,root*T(Math.Cos(a)*r*(.95+progress*.8),Math.Sin(a)*r*(.95+progress*.8)),C(201,181,255,.48+pulse*.18),null);}}
            if(e.Type=="aura"&&e.ShootTimer<=e.AuraWindow){double rad=e.AuraRadius+r*(.12+(.10+pulse*.05));var ring=Dynamic(Oval(rad,rad),Math.Max(1.5,r*.02),false);Paint(ring,root,null,C(255,98,185,.42+pulse*.20));}
            FinishPath();
        }
    }
}
