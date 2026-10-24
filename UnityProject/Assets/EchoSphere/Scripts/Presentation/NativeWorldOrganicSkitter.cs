using System;
using UnityEngine;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldCreatures
    {
        // Accepted organic skitter silhouette. Shared stance/recovery gait keeps
        // its limbs attached; contours and palette use the existing renderer.
        private static readonly double[] OrganicSkitterHips={-.42,-.08,.26};
        private void OrganicSkitter(Matrix4x4 root,double r,Color color,double t)
        {
            double phase=t*7;
            Legs(root,r,OrganicSkitterHips,.24,.78,phase,color,.85);
            // Smooth overlapping abdominal segments, not a hard surface chassis.
            for(int i=0;i<3;i++){
                double x=(-.38+i*.28)*r,rr=r*(.65-i*.045);
                Body(root*T(x,Math.Sin(phase+i*.8)*r*.012),rr,A(color,.78),0,.87);
            }
            var head=root*T(r*.52,0);
            var face=Get(124,r,0,Math.Max(.8,r*.035),()=>Quadratic(P(-r*.20,-r*.18),true,P(r*.02,-r*.29),P(r*.25,-r*.14),P(r*.34,-r*.07),P(r*.23,0),P(r*.34,r*.07),P(r*.25,r*.14),P(r*.02,r*.29),P(-r*.20,r*.18),P(-r*.28,0),P(-r*.20,-r*.18)));
            Paint(face,head,C(5,11,18),A(color,.95));
            // Recessed slits replace the large white eyeballs.
            var sensor=Get(121,r,0,Math.Max(.7,r*.04),()=>Quadratic(P(-r*.065,-r*.012),false,P(0,-r*.025),P(r*.07,r*.012)),false);
            for(int side=-1;side<=1;side+=2)Paint(sensor,head*T(r*.055,side*r*.10,side*.20),null,C(224,88,111,.9));
            // Hooked mandibles breathe slightly; this is not an attack telegraph.
            double gape=.018+Math.Sin(t*2.3)*.009;
            var jaw=Get(122,r,0,Math.Max(.7,r*.035),()=>Quadratic(P(r*.04,-r*.14),false,P(r*.36,-r*.27),P(r*.46,-r*.04),P(r*.34,-r*.12),P(r*.24,-r*.015)),false);
            for(int side=-1;side<=1;side+=2)Paint(jaw,head*T(angle:side*gape,sy:side),null,A(color,.95));
            var seam=Get(123,r,0,Math.Max(.55,r*.018),()=>Quadratic(P(-r*.73,-r*.05),false,P(-r*.25,-r*.12),P(r*.21,-r*.03)),false);
            Paint(seam,root,null,C(162,193,207,.18));
        }
    }
}
