using System;
using UnityEngine;
namespace EchoSphere.Presentation {
 internal sealed partial class NativeWorldCreatures {
  private void DrawRegionalAttackParts(EnemyView e,Matrix4x4 root){
   bool ranged=e.Fleet||e.RegionStyle==3||e.RegionStyle==4||e.Role=="ranged"||e.Role=="sniper"||e.RegionStyle==2&&(e.Role=="bomber"||e.Role=="corruptor");
   double r=e.Radius,pose=Math.Max(0,Math.Min(1,e.AttackPose));var color=e.Freeze?C(105,214,255):e.Color;
   var arm=Get(27000,r,0,Math.Max(.8,r*.06),()=>Poly(P(0,-r*.10),P(r*.65,-r*.16),P(r*.83,0),P(r*.65,r*.16),P(0,r*.10)));
   for(int side=-1;side<=1;side+=2){double angle=side*(ranged?.14+pose*.35:.45-pose*.4);var joint=root*T(r*(.35-pose*.18),side*r*(.65+pose*.12),angle,sx:e.RegionStyle==5&&!ranged?1+pose*2:1,sy:side);Paint(arm,joint,C(5,11,20),color);}
   if(ranged){var emitter=Get(27001,r,0,Math.Max(.7,r*.04),()=>Oval(r*.17,r*.17));Paint(emitter,root*T(r*.85),A(color,.3+pose*.7),color);}
   if(e.AttackWindup>0){var ray=Get(27002,r,0,1,()=>Straight(P(r*1.05,0),P(r*3.5,0)));Paint(ray,root,null,A(color,.25+pose*.5));}
  }
 }
}
