using System;
using UnityEngine;
namespace EchoSphere.Presentation {
 internal sealed partial class NativeWorldCreatures {
  private void DrawRegionalAttackParts(EnemyView e,Matrix4x4 root){
   if(e.RegionStyle<=0)return; // Preserve the original region-one silhouettes.
   if(e.RegionStyle>0&&e.AttackPose>0)DrawRegionalAttackFeedback(e,root);
   if(e.RegionStyle==2&&e.Role=="anchor")return; // Its accepted carapace plates now own the hatch pose.
   bool ranged=e.FaceTarget;
   double r=e.Radius,pose=Math.Max(0,Math.Min(1,e.AttackPose));var color=e.Freeze?C(105,214,255):e.RegionStyle==2?C(174,210,108):e.RegionStyle==3?C(123,219,245):e.RegionStyle==4?C(255,207,100):e.RegionStyle==5?C(192,123,230):e.Color;
   if(e.RegionStyle>=2&&e.RegionStyle<=5){
    if(e.AttackWindup>0){var ray=Get(27002,r,0,1,()=>Straight(P(r*1.05,0),P(r*3.5,0)));Paint(ray,root,null,A(color,.25+pose*.5));}
    return;
   }
   var arm=Get(27000,r,0,Math.Max(.8,r*.06),()=>Poly(P(0,-r*.10),P(r*.65,-r*.16),P(r*.83,0),P(r*.65,r*.16),P(0,r*.10)));
   for(int side=-1;side<=1;side+=2){double angle=side*(ranged?.14+pose*.35:.45-pose*.4);var joint=root*T(r*(.35-pose*.18),side*r*(.65+pose*.12),angle,sx:e.RegionStyle==5&&!ranged?1+pose*2:1,sy:side);Paint(arm,joint,C(5,11,20),color);}
   if(ranged){var emitter=Get(27001,r,0,Math.Max(.7,r*.04),()=>Oval(r*.17,r*.17));Paint(emitter,root*T(r*.85),A(color,.3+pose*.7),color);}
   if(e.AttackWindup>0){var ray=Get(27002,r,0,1,()=>Straight(P(r*1.05,0),P(r*3.5,0)));Paint(ray,root,null,A(color,.25+pose*.5));}
  }
  private void DrawRegionalAttackFeedback(EnemyView e,Matrix4x4 root){
   double r=e.Radius,p=Clamp(e.AttackPose);Color color=e.RegionStyle==2?C(185,220,106):e.RegionStyle==3?C(134,235,255):e.RegionStyle==4?C(255,207,100):e.RegionStyle==5?C(198,131,255):C(105,228,255);
   var ring=Get(27400,r,0,Math.Max(.8,r*.045),()=>Oval(r*.40,r*.40));
   bool support=e.Role=="healer"||e.Role=="disruptor";
   if(support||e.FaceTarget){double size=e.AttackWindup>0?.55+p*.60:1.1+(1-p)*1.7;var at=root*T(e.FaceTarget?r*.72:0,0,sx:size,sy:size);Paint(ring,at,null,A(color,.30+p*.65));if(e.FaceTarget)RegionalEllipse(root*T(r*.76),r*.13,r*.13,A(color,.25+p*.65),C(245,255,255,.8));}
   else if(e.RegionStyle==5){
    var link=Get(27401,r,0,Math.Max(1,r*.07),()=>Straight(Vector2.zero,P(r*.65,0)));
    for(int side=-1;side<=1;side+=2){var joint=root*T(.25*r,side*.30*r,side*(.55-p*.42));for(int i=0;i<3;i++){Paint(link,joint,null,A(color,.45+p*.50));joint=joint*T(.65*r,0,angle:-side*(.20-p*.12));}}
   }else{
    var slash=Get(27402,r,0,Math.Max(1,r*.07),()=>Oval(r*1.12,r*1.12,Math.PI*.8),false);Paint(slash,root*T(.30*r,0,angle:-Math.PI*.4+(1-p)*.5),null,A(color,.28+p*.62));
   }
  }

 }
}
