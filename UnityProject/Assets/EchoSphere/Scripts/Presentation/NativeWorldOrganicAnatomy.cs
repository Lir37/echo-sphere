using System;
using UnityEngine;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldCreatures
    {
        private bool _gaitActive,_airborneLegs,_organicFaces;
        private double _gaitTravel,_gaitBlend=1,_poseTime;
        private string _poseVariant;
        internal struct OrganicLegPose {internal Vector2 Hip,Knee,Foot;internal bool Contact;internal double UpperLength,LowerLength;}
        // 64% planted stance, then eased recovery. Upper and lower bones keep
        // their length; the knee is solved from the hip and foot, not animated apart.
        internal static OrganicLegPose SolveOrganicLeg(double r,double hipX,int side,double bodySide,double length,double spread,double phase,double amount=1,double hipWave=0,bool flying=false)
        {
            double q=phase/(Math.PI*2);q-=Math.Floor(q);const double duty=.64;
            bool contact=q<duty;double stride,lift,half=Math.Min(.30,length*.60);
            if(contact){stride=half-2*half*q/duty;lift=0;}
            else{double u=(q-duty)/(1-duty),ease=u*u*(3-2*u);stride=-half+2*half*ease;lift=Math.Sin(u*Math.PI)*.12;}
            stride*=amount;lift*=amount;
            double hx=hipX*r,hy=side*bodySide*r+hipWave;
            double fx=hx+spread*r+stride*r,fy=side*(bodySide+length*.76-lift)*r;
            if(flying){fx=hx-length*r*.20+Math.Sin(phase)*r*.025;fy=side*(bodySide+length*.48+Math.Sin(phase+.7)*.025)*r;contact=false;}
            double upper=length*r*.72,lower=length*r*.80,dx=fx-hx,dy=fy-hy,d=Math.Sqrt(dx*dx+dy*dy);
            d=Math.Min(upper+lower-.00001,Math.Max(Math.Abs(upper-lower)+.00001,d));
            double direction=Math.Atan2(dy,dx);fx=hx+Math.Cos(direction)*d;fy=hy+Math.Sin(direction)*d;
            double along=(upper*upper-lower*lower+d*d)/(2*d),height=Math.Sqrt(Math.Max(0,upper*upper-along*along));
            double bend=(hipX<-.15?1:-1)*side,nx=-Math.Sin(direction),ny=Math.Cos(direction);
            return new OrganicLegPose {Hip=P(hx,hy),Knee=P(hx+Math.Cos(direction)*along+nx*height*bend,hy+Math.Sin(direction)*along+ny*height*bend),Foot=P(fx,fy),Contact=contact,UpperLength=upper,LowerLength=lower};
        }
        private void OrganicEye(Matrix4x4 root,double r,Color color)
        {
            var slit=Get(200,r,0,Math.Max(.65,r*.22),()=>Quadratic(P(-r,0),false,P(0,-r*.23),P(r,.02*r)),false);
            Paint(slit,root,null,C(225,85,112,.92));
        }
        private void OrganicFangs(Matrix4x4 root,double r,Color color,double time)
        {
            var jaw=Get(201,r,0,Math.Max(.7,r*.032),()=>Quadratic(P(0,-r*.18),false,P(r*.35,-r*.34),P(r*.46,-r*.03),P(r*.31,-r*.14),P(r*.20,0)),false);
            double breathe=Math.Sin(time*2.2)*.012;
            for(int side=-1;side<=1;side+=2)Paint(jaw,root*T(angle:side*breathe,sy:side),null,A(color,.90));
        }
    }
}
