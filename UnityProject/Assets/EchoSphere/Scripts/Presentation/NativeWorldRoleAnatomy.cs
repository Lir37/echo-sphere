using System;
using UnityEngine;
namespace EchoSphere.Presentation {
 internal sealed partial class NativeWorldCreatures {
  // Each role has a recognisable body plan. Shapes are retained; poses move joints.
  private bool DrawRegionalRoleBody(EnemyView e,double time,Matrix4x4 root,Color color){
   string role=e.Boss?e.Variant:e.Role;
   if(e.Boss)role=role=="shooter"?"ranged":role=="summoner"?"splitter":role=="architect"?"anchor":role=="aura"||role=="conductor"?"healer":role=="null"?"phase":role=="stella_warden"?"bomber":role;
   if(string.IsNullOrEmpty(role)||role=="grunt")return false;
   int id=role=="swarmer"?0:role=="charger"?1:role=="tank_guard"?2:role=="anchor"?3:role=="ranged"?4:role=="sniper"?5:role=="healer"?6:role=="disruptor"?7:role=="splitter"?8:role=="corruptor"?9:role=="leech"?10:role=="phase"?11:role=="scavenger"?12:role=="bomber"?13:-1;
   if(id<0)return false;double r=e.Radius,w=Math.Max(.8,r*.05),attack=Clamp(e.AttackPose);int style=e.RegionStyle;
   Art Part(int part,Func<Unity.VectorGraphics.BezierContour> make,double width=0)=>Get(40000+style*1000+id*32+part,r,0,width,make);
   var shell=Part(0,()=>style==2?Quadratic(P(.72*r,0),true,P(.52*r,-.56*r),P(-.18*r,-.61*r),P(-.87*r,-.31*r),P(-.74*r,.21*r),P(-.32*r,.66*r),P(.38*r,.47*r),P(.65*r,.32*r),P(.72*r,0)):
    style==3?Poly(P(.84*r,0),P(.24*r,-.59*r),P(-.60*r,-.48*r),P(-.89*r,.08*r),P(-.30*r,.57*r),P(.43*r,.40*r)):
    style==4?Quadratic(P(.92*r,0),true,P(.37*r,-.18*r),P(.15*r,-.62*r),P(-.20*r,-.20*r),P(-.79*r,-.28*r),P(-.39*r,.04*r),P(-.62*r,.60*r),P(-.05*r,.28*r),P(.34*r,.55*r),P(.45*r,.15*r),P(.92*r,0)):
    Poly(P(.87*r,0),P(.24*r,-.43*r),P(-.21*r,-.63*r),P(-.32*r,-.30*r),P(-.84*r,-.41*r),P(-.58*r,0),P(-.87*r,.48*r),P(-.30*r,.32*r),P(-.07*r,.57*r),P(.38*r,.37*r)),w);
   var limb=Part(1,()=>style==2||style==5?Quadratic(P(0,-.08*r),true,P(.36*r,-.24*r),P(.74*r,-.08*r),P(.94*r,.10*r),P(.59*r,.36*r),P(.63*r,.08*r),P(.29*r,.11*r),P(.09*r,.14*r),P(0,-.08*r)):
    Poly(P(0,-.10*r),P(.36*r,-.23*r),P(.90*r,.03*r),P(.49*r,.28*r),P(.62*r,.07*r),P(.18*r,.11*r)),w*.7);
   var eye=Part(2,()=>Poly(P(.20*r,0),P(0,-.09*r),P(-.16*r,0),P(0,.09*r)));
   var seam=Part(3,()=>Open(P(-.45*r,-.15*r),P(-.13*r,-.24*r),P(.18*r,-.08*r)),w*.6);
   var plate=Part(4,()=>style==2?Quadratic(P(.45*r,0),true,P(.25*r,-.31*r),P(-.18*r,-.36*r),P(-.66*r,-.16*r),P(-.52*r,.06*r),P(-.17*r,.13*r),P(.45*r,0)):
    Poly(P(.52*r,0),P(.13*r,-.31*r),P(-.47*r,-.29*r),P(-.63*r,.02*r),P(-.21*r,.17*r),P(.28*r,.10*r)),w*.75);
   var light=Color.Lerp(color,Color.white,.55f);var dark=RegionalInk(color,.15);var body=RegionalInk(color,.45);
   void Armor(Matrix4x4 m){Paint(shell,m,body,color);Paint(seam,m,null,A(light,.65));}
   void Eye(Matrix4x4 m){Paint(eye,m,C(5,9,15),light);Paint(eye,m*T(.04*r,0,sx:.28,sy:.65),light,null);}
   void Claw(Matrix4x4 m){Paint(limb,m*T(angle:Math.Sin(time*6+id*.6+m.m13/Math.Max(1,r))*(.12+.18*_gaitBlend)),RegionalInk(color,.36),color);}
   if(id==10){ // Leech: a long articulated chain, not a recoloured beetle.
    for(int i=3;i>=0;i--){double q=time*3-i*.85;var joint=root*T(-i*.40*r,Math.Sin(q)*r*.13,-Math.Sin(q)*.16,sx:.45+i*.04,sy:.48-i*.025);Armor(joint);}
    for(int side=-1;side<=1;side+=2)Claw(root*T(.12*r,side*.18*r,side*(.25-attack*.36),sx:.65,sy:side*.65));Eye(root*T(.29*r,0,sx:.7,sy:.8));
   }else if(id==2||id==3){ // Guardian shield / anchored fortress.
    for(int side=-1;side<=1;side+=2){for(int k=0;k<(id==3?3:2);k++)Claw(root*T((-.55+k*.50)*r,side*.40*r,Math.PI+side*.5,sx:.52,sy:side*.60));var shield=root*T(.05*r,side*.17*r,side*(.18+attack*.20+Math.Sin(time*2)*.08),sx:1.15,sy:side*1.25);Paint(plate,shield,body,color);Paint(seam,shield,null,A(light,.55));}
    Armor(root*T(sx:.70,sy:.68));Eye(root*T(.40*r,0));
   }else if(id==8||id==9||id==13){ // Brood, corruptor, volatile sac.
    int sacs=id==13?3:2;for(int i=0;i<sacs;i++){double a=i*Math.PI*2/sacs+Math.PI;Armor(root*T(Math.Cos(a)*r*.40,Math.Sin(a)*r*.40,angle:a,sx:.65,sy:.63));}
    Armor(root*T(.18*r,0,sx:.60,sy:.66));
    for(int side=-1;side<=1;side+=2)Claw(root*T(.30*r,side*.25*r,side*(.35-attack*.42),sx:.60,sy:side*.65));
    RegionalEllipse(root*T(-.22*r),r*.22,r*.27,RegionalInk(color,id==13?.95:.65),light,w*.65);Eye(root*T(.40*r,0,sx:.7,sy:.8));
   }else if(id==6||id==7){ // Medic crown vs a two-pronged disruptor.
    int arms=id==6?3:2;for(int i=0;i<arms;i++){double a=id==6?i*Math.PI*2/3:Math.PI+(i==0?-.8:.8);var arm=root*T(angle:a+Math.Sin(time*2+i)*.15);Paint(plate,arm*T(.45*r,0,sx:.82,sy:.9),body,color);Claw(arm*T(.55*r,0,angle:-.5+attack*.4,sx:.55,sy:.55));}
    Armor(root*T(sx:.60,sy:.68));Eye(root*T(.27*r,0));
   }else if(id==4||id==5){ // Broad spitter vs long narrow sniper.
    Armor(root*T(sx:id==5?1.15:.86,sy:id==5?.47:.73));
    for(int side=-1;side<=1;side+=2){Claw(root*T(-.31*r,side*.20*r,Math.PI+side*.5,sx:.55,sy:side*.65));Paint(plate,root*T((.30-attack*.16)*r,side*.16*r,side*.10,sx:id==5?1.18:.70,sy:side*.55),body,color);}
    Eye(root*T(.54*r,0,sx:.7,sy:.9));
   }else if(id==11||id==12){ // Phantom swept crescent / four-clawed scavenger.
    Armor(root*T(sx:.80,sy:.55));for(int side=-1;side<=1;side+=2){Claw(root*T(-.32*r,side*.15*r,Math.PI+side*(.30+.14*Math.Sin(time*3)),sx:1,sy:side*.75));if(id==12)Claw(root*T(.30*r,side*.25*r,side*(.45-attack*.4),sx:.68,sy:side*.68));}
    if(id==11)Paint(plate,root*T(-.8*r,0,Math.PI,sx:.75,sy:.55),RegionalInk(color,.25),A(color,.5));Eye(root*T(.32*r,0));
   }else if(id==0){ // Four-winged / four-bladed darting scout.
    for(int side=-1;side<=1;side+=2)for(int i=0;i<2;i++){var wing=root*T((-.1-i*.30)*r,side*.18*r,side*(.60+i*.35+.18*Math.Sin(time*9+i)),sx:.75,sy:side*.85);Paint(plate,wing,RegionalInk(color,.34),color);}
    Armor(root*T(sx:.70,sy:.47));Eye(root*T(.32*r,0,sx:.7,sy:.75));
   }else{ // Charger: forward horns, narrow rear and articulated legs.
    Armor(root*T(sx:1.10,sy:.63));for(int side=-1;side<=1;side+=2){Paint(plate,root*T(.30*r,side*.15*r,side*(.13-attack*.17),sx:1.15,sy:side),body,color);Claw(root*T(-.4*r,side*.25*r,Math.PI+side*.45,sx:.55,sy:side*.65));}Eye(root*T(.47*r,0,sx:.75,sy:.8));
   }
   return true;
  }
 }
}
