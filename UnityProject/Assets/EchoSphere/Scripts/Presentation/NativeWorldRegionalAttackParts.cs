using System;
using UnityEngine;
namespace EchoSphere.Presentation {
 internal sealed partial class NativeWorldCreatures {
  private void DrawRegionalAttackParts(EnemyView e,Matrix4x4 root){
   bool ranged=e.FaceTarget;
   double r=e.Radius,pose=Math.Max(0,Math.Min(1,e.AttackPose));var color=e.Freeze?C(105,214,255):e.RegionStyle==2?C(174,210,108):e.RegionStyle==3?C(123,219,245):e.RegionStyle==4?C(255,207,100):e.RegionStyle==5?C(192,123,230):e.Color;
   if(e.RegionStyle>=2&&e.RegionStyle<=5){
    int style=e.RegionStyle;double width=Math.Max(.7,r*.045);
    var organ=Get(27200+style,r,0,width,()=>style==2?
      Quadratic(P(0,-.10*r),true,P(.25*r,-.24*r),P(.54*r,-.13*r),P(.79*r,.04*r),P(.48*r,.28*r),P(.55*r,.05*r),P(.26*r,.08*r),P(.08*r,.15*r),P(0,-.10*r)):
      style==3?Poly(P(0,-.14*r),P(.27*r,-.24*r),P(.60*r,-.16*r),P(.84*r,0),P(.51*r,.15*r),P(.26*r,.09*r),P(.07*r,.20*r)):
      style==4?Quadratic(P(0,-.11*r),true,P(.32*r,-.31*r),P(.73*r,-.07*r),P(.46*r,-.02*r),P(.67*r,.22*r),P(.26*r,.08*r),P(0,.11*r),P(-.10*r,0),P(0,-.11*r)):
      Quadratic(P(0,-.11*r),true,P(.32*r,-.19*r),P(.68*r,-.02*r),P(.94*r,.13*r),P(.48*r,.38*r),P(.67*r,.11*r),P(.31*r,.13*r),P(.09*r,.19*r),P(0,-.11*r)));
    for(int side=-1;side<=1;side+=2){var joint=root*T((.15-pose*.10)*r,side*.48*r,side*(ranged?.04+pose*.18:.33-pose*.42),sy:side);Paint(organ,joint,RegionalInk(color,.40),color);}
    if(ranged){var socket=Get(27210+style,r,0,width,()=>style==3?Poly(P(.19*r,0),P(0,-.14*r),P(-.12*r,0),P(0,.14*r)):Oval(r*.16,r*.11));Paint(socket,root*T(r*.70),RegionalInk(color,.20),Color.Lerp(color,Color.white,(float)(.3+pose*.4)));}
   }else{
   var arm=Get(27000,r,0,Math.Max(.8,r*.06),()=>Poly(P(0,-r*.10),P(r*.65,-r*.16),P(r*.83,0),P(r*.65,r*.16),P(0,r*.10)));
   for(int side=-1;side<=1;side+=2){double angle=side*(ranged?.14+pose*.35:.45-pose*.4);var joint=root*T(r*(.35-pose*.18),side*r*(.65+pose*.12),angle,sx:e.RegionStyle==5&&!ranged?1+pose*2:1,sy:side);Paint(arm,joint,C(5,11,20),color);}
   if(ranged){var emitter=Get(27001,r,0,Math.Max(.7,r*.04),()=>Oval(r*.17,r*.17));Paint(emitter,root*T(r*.85),A(color,.3+pose*.7),color);}
   }
   if(e.AttackWindup>0){var ray=Get(27002,r,0,1,()=>Straight(P(r*1.05,0),P(r*3.5,0)));Paint(ray,root,null,A(color,.25+pose*.5));}
  }
 }
}
