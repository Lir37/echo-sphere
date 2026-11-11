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
            bool carrier=role=="fleet_carrier";
            int hull=carrier?28:role=="charger"?7:role=="swarmer"?8:role=="disruptor"||role=="corruptor"?9:role=="sniper"?1:role=="tank_guard"||role=="architect"||role=="anchor"?2:role=="healer"||role=="aura"||role=="conductor"?3:role=="splitter"||role=="summoner"?4:role=="phase"||role=="null"?5:role=="bomber"||role=="stella_warden"?6:0;
            var dark=RegionalInk(color,.17);var armor=RegionalInk(color,.43);var trim=Color.Lerp(color,Color.white,.38f);double stroke=Math.Max(.8,r*.055);
            Art Shape(int id,Func<Unity.VectorGraphics.BezierContour> make,double w=0)=>Get(2100+id,r,0,w,make);
            var body=Shape(hull,()=>carrier
                ?Poly(P(1.3*r,0),P(.34*r,.44*r),P(-.78*r,.65*r),P(-1.07*r,.46*r),P(-1.07*r,-.46*r),P(-.78*r,-.65*r),P(.34*r,-.44*r))
                :hull==1?Poly(P(1.05*r,0),P(-.8*r,-.27*r),P(-.56*r,0),P(-.8*r,.27*r)):
                hull==2?Poly(P(.83*r,-.32*r),P(.83*r,.32*r),P(-.36*r,.72*r),P(-.83*r,.43*r),P(-.83*r,-.43*r),P(-.36*r,-.72*r)):
                hull==3?Poly(P(.75*r,0),P(.20*r,.62*r),P(-.66*r,.49*r),P(-.45*r,0),P(-.66*r,-.49*r),P(.20*r,-.62*r)):
                hull==4?Poly(P(.82*r,-.22*r),P(.42*r,0),P(.82*r,.22*r),P(-.75*r,.65*r),P(-.55*r,0),P(-.75*r,-.65*r)):
                hull==5?Poly(P(.9*r,0),P(-.48*r,.36*r),P(-.9*r,.8*r),P(-.5*r,0),P(-.9*r,-.8*r),P(-.48*r,-.36*r)):
                hull==7?Poly(P(1.1*r,0),P(.70*r,-.55*r),P(.14*r,-.43*r),P(-.72*r,-.22*r),P(-.86*r,0),P(-.72*r,.22*r),P(.14*r,.43*r),P(.70*r,.55*r)):
                hull==8?Poly(P(.9*r,0),P(-.42*r,-.2*r),P(-1.05*r,-.87*r),P(-.81*r,-.10*r),P(-.98*r,.08*r),P(-.81*r,.10*r),P(-1.05*r,.87*r),P(-.42*r,.2*r)):
                hull==9?Poly(P(.90*r,-.40*r),P(-.25*r,-.60*r),P(-.76*r,-.37*r),P(-.45*r,0),P(-.76*r,.37*r),P(-.25*r,.60*r),P(.90*r,.40*r),P(.25*r,.17*r),P(.07*r,0),P(.25*r,-.17*r)):
                hull==6?Poly(P(.55*r,-.5*r),P(.85*r,0),P(.55*r,.5*r),P(-.65*r,.72*r),P(-.83*r,0),P(-.65*r,-.72*r)):
                Poly(P(.95*r,0),P(-.72*r,.62*r),P(-.36*r,0),P(-.72*r,-.62*r)),stroke);
            Paint(body,root,dark,color);
            // Flat inset deck and separate armor modules make the hull legible
            // at phone size without shaded extrusion or per-frame tessellation.
            Paint(body,root*T(sx:.78,sy:.70),armor,A(color,.65));
            var deck=Shape(25,()=>Poly(P(.56*r,-.10*r),P(.32*r,-.28*r),P(-.38*r,-.26*r),P(-.56*r,-.08*r),P(-.38*r,.26*r),P(.32*r,.28*r),P(.56*r,.10*r)),stroke*.6);
            Paint(deck,root,RegionalInk(color,.25),A(trim,.65));
            var hatch=Shape(26,()=>Poly(P(.12*r,-.10*r),P(-.12*r,-.10*r),P(-.18*r,0),P(-.12*r,.10*r),P(.12*r,.10*r),P(.18*r,0)),stroke*.55);
            var seam=Shape(27,()=>Open(P(-.55*r,-.17*r),P(-.28*r,-.17*r),P(-.16*r,0),P(-.28*r,.17*r),P(-.55*r,.17*r)),stroke*.6);
            Paint(hatch,root*T(-.24*r),RegionalInk(color,.62),trim);Paint(seam,root,null,A(color,.7));
            var spine=Shape(10,()=>Poly(P(.68*r,0),P(-.22*r,.12*r),P(-.43*r,0),P(-.22*r,-.12*r)),stroke*.7);
            Paint(spine,root,A(color,.22),C(220,250,255,.82));
            var wing=Shape(11,()=>Poly(P(.30*r,0),P(-.4*r,.5*r),P(-.8*r,.54*r),P(-.34*r,.14*r)),stroke);
            var engine=Shape(12,()=>Poly(P(-.35*r,-.10*r),P(-.65*r,-.12*r),P(-.72*r,.12*r),P(-.35*r,.10*r)),stroke);
            var plume=Shape(13,()=>Poly(P(0,-.085*r),P(-.75*r,0),P(0,.085*r)));
            double attack=Clamp(e.AttackPose),idle=Math.Sin(time*3.2);
            var vane=Shape(22,()=>Poly(P(-.10*r,0),P(-.36*r,-.10*r),P(-.56*r,0),P(-.36*r,.07*r)),stroke*.65);
            var vent=Shape(23,()=>Straight(P(-.12*r,-.07*r),P(.12*r,.07*r)),stroke*.7);
            var sensor=Shape(24,()=>Poly(P(.15*r,0),P(0,-.10*r),P(-.15*r,0),P(0,.10*r)),stroke*.6);
            double thrust=.28+.26*_gaitBlend+.10*Math.Sin(time*23+e.X*.02),spread=carrier?.43:hull==1?.3:hull==2?.55:.65;
            for(int side=-1;side<=1;side+=2){
                var part=root*T(0,side*r*spread,sx:1,sy:side);
                Paint(wing,part*T(angle:.12*idle+attack*.20,sx:carrier?.75:1,sy:carrier?.42:1),armor,A(color,.78));
                Paint(engine,part*T(x:attack*r*.07),RegionalInk(color,.30),color);
                Paint(vane,part*T(-.45*r,r*.10,.12+.10*idle+attack*.20),dark,color);
                for(int v=0;v<2;v++)Paint(vent,part*T(-.05*r-v*.18*r,0,idle*.08+attack*.22),null,A(color,.65));Paint(plume,part*T(-.68*r,0,sx:thrust),A(color,.50),null,additive:true);
                var slit=Shape(14,()=>Straight(P(-.36*r,0),P(-.58*r,0)),stroke*1.5);Paint(slit,part,null,C(235,255,255,.85));
            }
            if(hull==2||e.Elite||e.Boss){
                var plate=Shape(15,()=>Poly(P(.4*r,0),P(-.15*r,.14*r),P(-.52*r,0),P(-.15*r,-.14*r)),stroke);
                for(int side=-1;side<=1;side+=2)Paint(plate,root*T(-.1*r,side*r*.42),armor,color);
            }
            if(hull==3){
                var aerial=Shape(16,()=>Straight(P(-.2*r,0),P(.45*r,0)),stroke);
                for(int side=-1;side<=1;side+=2){var local=root*T(0,side*r*.8,side*.25);Paint(aerial,local,null,color);Ellipse(local*T(.45*r),r*.075,r*.075,C(235,255,255),color,stroke*.6);}
            }
            if(hull==4||hull==6){
                var pod=Shape(17,()=>Poly(P(.38*r,0),P(.1*r,.16*r),P(-.4*r,.16*r),P(-.5*r,0),P(-.4*r,-.16*r),P(.1*r,-.16*r)),stroke);
                for(int side=-1;side<=1;side+=2)Paint(pod,root*T(-.1*r,side*r*.95),armor,color);
            }
            if(role=="ranged"||role=="shooter"||role=="sniper"){
                var rail=Shape(19,()=>Poly(P(-.2*r,-.045*r),P(.95*r,-.045*r),P(1.1*r,0),P(.95*r,.045*r),P(-.2*r,.045*r)),stroke*.7);
                for(int side=-1;side<=1;side+=2)Paint(rail,root*T((.05-attack*.20)*r,side*r*((role=="sniper"?.19:.38)+attack*.08)),armor,color);
            }
            if(role=="healer"||role=="leech"||role=="scavenger"||role=="corruptor"){
                var claw=Shape(20,()=>Poly(P(.1*r,0),P(.6*r,.12*r),P(.95*r,.38*r),P(.45*r,.25*r)),stroke);
                for(int side=-1;side<=1;side+=2)Paint(claw,root*T(.1*r,side*r*.28,angle:side*(.06*idle-attack*.30),sy:side),dark,color);
            }
            if(role=="anchor"){
                var outrigger=Shape(21,()=>Poly(P(-.18*r,0),P(.18*r,0),P(.26*r,.25*r),P(0,.4*r),P(-.26*r,.25*r)),stroke);
                for(int side=-1;side<=1;side+=2)Paint(outrigger,root*T(-.1*r,side*r*.85,sy:side),dark,color);
            }
            Paint(sensor,root*T(.30*r,0,angle:Math.Sin(time*2.2)*.16),A(color,.20),C(225,250,255,.85));
            if(carrier)
            {
                var bay=Shape(29,()=>Poly(P(.52*r,-.09*r),P(.52*r,.09*r),P(-.77*r,.09*r),P(-.88*r,0),P(-.77*r,-.09*r)),stroke*.65);
                for(int side=-1;side<=1;side+=2)
                    Paint(bay,root*T(-.13*r,side*r*(.26+attack*.10)),dark,A(trim,.85));
            }
            if(e.Boss){
                var bridge=Shape(18,()=>Poly(P(.15*r,-.25*r),P(.5*r,0),P(.15*r,.25*r),P(-.3*r,.25*r),P(-.45*r,0),P(-.3*r,-.25*r)),stroke);
                Paint(bridge,root,A(color,.13),C(225,250,255,.9));
            }
            _gaitActive=false;
        }
    }
}
