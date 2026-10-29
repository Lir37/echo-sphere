using System;
using UnityEngine;
namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldCreatures
    {
        // Spectral Rift is a fleet, not recolored organic anatomy. Local +X is forward.
        // All hull/panel/nozzle meshes are retained; animation changes rigid transforms only.
        private void DrawFleet(EnemyView e,double time,Matrix4x4 root)
        {
            double r=e.Radius;var color=e.Freeze?C(105,214,255):e.Color;
            string role=e.Boss?e.Variant:e.Role;
            int hull=role=="sniper"||role=="charger"?1:role=="tank_guard"||role=="architect"||role=="anchor"?2:role=="healer"||role=="aura"||role=="conductor"?3:role=="splitter"||role=="summoner"?4:role=="phase"||role=="null"?5:role=="bomber"||role=="stella_warden"?6:0;
            var dark=C(4,11,20,.99);double stroke=Math.Max(.8,r*.055);
            Art Shape(int id,Func<Unity.VectorGraphics.BezierContour> make,double w=0)=>Get(2100+id,r,0,w,make);
            var body=Shape(hull,()=>hull==1?Poly(P(1.05*r,0),P(-.8*r,-.27*r),P(-.56*r,0),P(-.8*r,.27*r)):
                hull==2?Poly(P(.83*r,-.32*r),P(.83*r,.32*r),P(-.36*r,.72*r),P(-.83*r,.43*r),P(-.83*r,-.43*r),P(-.36*r,-.72*r)):
                hull==3?Poly(P(.75*r,0),P(.20*r,.62*r),P(-.66*r,.49*r),P(-.45*r,0),P(-.66*r,-.49*r),P(.20*r,-.62*r)):
                hull==4?Poly(P(.82*r,-.22*r),P(.42*r,0),P(.82*r,.22*r),P(-.75*r,.65*r),P(-.55*r,0),P(-.75*r,-.65*r)):
                hull==5?Poly(P(.9*r,0),P(-.48*r,.36*r),P(-.9*r,.8*r),P(-.5*r,0),P(-.9*r,-.8*r),P(-.48*r,-.36*r)):
                hull==6?Poly(P(.55*r,-.5*r),P(.85*r,0),P(.55*r,.5*r),P(-.65*r,.72*r),P(-.83*r,0),P(-.65*r,-.72*r)):
                Poly(P(.95*r,0),P(-.72*r,.62*r),P(-.36*r,0),P(-.72*r,-.62*r)),stroke);
            Paint(body,root,dark,color);
            var spine=Shape(10,()=>Poly(P(.68*r,0),P(-.22*r,.12*r),P(-.43*r,0),P(-.22*r,-.12*r)),stroke*.7);
            Paint(spine,root,A(color,.22),C(220,250,255,.82));
            var wing=Shape(11,()=>Poly(P(.30*r,0),P(-.4*r,.5*r),P(-.8*r,.54*r),P(-.34*r,.14*r)),stroke);
            var engine=Shape(12,()=>Poly(P(-.35*r,-.10*r),P(-.65*r,-.12*r),P(-.72*r,.12*r),P(-.35*r,.10*r)),stroke);
            var plume=Shape(13,()=>Poly(P(0,-.085*r),P(-.75*r,0),P(0,.085*r)));
            double thrust=.28+.26*_gaitBlend+.10*Math.Sin(time*23+e.X*.02),spread=hull==1?.3:hull==2?.55:.65;
            for(int side=-1;side<=1;side+=2){
                var part=root*T(0,side*r*spread,sx:1,sy:side);
                Paint(wing,part*T(angle:.025*Math.Sin(time*2)),dark,A(color,.78));
                Paint(engine,part,dark,color);Paint(plume,part*T(-.68*r,0,sx:thrust),A(color,.50),null,additive:true);
                var slit=Shape(14,()=>Straight(P(-.36*r,0),P(-.58*r,0)),stroke*1.5);Paint(slit,part,null,C(235,255,255,.85));
            }
            if(hull==2||e.Elite||e.Boss){
                var plate=Shape(15,()=>Poly(P(.4*r,0),P(-.15*r,.14*r),P(-.52*r,0),P(-.15*r,-.14*r)),stroke);
                for(int side=-1;side<=1;side+=2)Paint(plate,root*T(-.1*r,side*r*.42),dark,color);
            }
            if(hull==3){
                var aerial=Shape(16,()=>Straight(P(-.2*r,0),P(.45*r,0)),stroke);
                for(int side=-1;side<=1;side+=2){var local=root*T(0,side*r*.8,side*.25);Paint(aerial,local,null,color);Ellipse(local*T(.45*r),r*.075,r*.075,C(235,255,255),color,stroke*.6);}
            }
            if(hull==4||hull==6){
                var pod=Shape(17,()=>Poly(P(.38*r,0),P(.1*r,.16*r),P(-.4*r,.16*r),P(-.5*r,0),P(-.4*r,-.16*r),P(.1*r,-.16*r)),stroke);
                for(int side=-1;side<=1;side+=2)Paint(pod,root*T(-.1*r,side*r*.95),dark,color);
            }
            if(role=="ranged"||role=="shooter"||role=="sniper"){
                var rail=Shape(19,()=>Poly(P(-.2*r,-.045*r),P(.95*r,-.045*r),P(1.1*r,0),P(.95*r,.045*r),P(-.2*r,.045*r)),stroke*.7);
                for(int side=-1;side<=1;side+=2)Paint(rail,root*T(.05*r,side*r*(role=="sniper"?.19:.38)),dark,color);
            }
            if(role=="leech"||role=="scavenger"||role=="corruptor"){
                var claw=Shape(20,()=>Poly(P(.1*r,0),P(.6*r,.12*r),P(.95*r,.38*r),P(.45*r,.25*r)),stroke);
                for(int side=-1;side<=1;side+=2)Paint(claw,root*T(.1*r,side*r*.28,sy:side),dark,color);
            }
            if(role=="anchor"){
                var outrigger=Shape(21,()=>Poly(P(-.18*r,0),P(.18*r,0),P(.26*r,.25*r),P(0,.4*r),P(-.26*r,.25*r)),stroke);
                for(int side=-1;side<=1;side+=2)Paint(outrigger,root*T(-.1*r,side*r*.85,sy:side),dark,color);
            }
            if(e.Boss){
                var bridge=Shape(18,()=>Poly(P(.15*r,-.25*r),P(.5*r,0),P(.15*r,.25*r),P(-.3*r,.25*r),P(-.45*r,0),P(-.3*r,-.25*r)),stroke);
                Paint(bridge,root,A(color,.13),C(225,250,255,.9));
            }
            _gaitActive=false;
        }
    }
}
