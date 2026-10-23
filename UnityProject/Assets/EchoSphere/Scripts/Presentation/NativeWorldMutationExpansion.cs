using System;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldSphereFamilies
    {
        private void MutationExpansion(MutationRig rig,MutationView s)
        {
            double f=s.Final;if(f<0)return;double r=s.Radius,t=s.Time;var c=s.Color;string family=s.Branch.Split('_')[0];int mode=f==0?0:f==1?1:2;double phase=t*(mode==0?.72:mode==1?1.05:1.35);const double tau=Math.PI*2;
            if(family=="orbital"){
                int count=mode==0?3:mode==1?4:5;double radius=r*(1.36+(mode==2?.10:0));for(int i=0;i<count;i++){double a=phase+i*tau/count;rig.Shard(Math.Cos(a)*radius,Math.Sin(a)*radius*.66,r*(mode==2?.16:.12),c,a,.72);}if(mode==1)rig.Pulse(r*1.18,c,t,.8);if(mode==2)rig.Arc(r*1.52,c,1.4,-.52,.52,.72);
            }else if(family=="chain"||family=="aura"||family=="gravity"){
                if(mode==0){for(int i=0;i<3;i++){double a=phase+i*tau/3;rig.Tether(Math.Cos(a)*r*.34,Math.Sin(a)*r*.22,Math.Cos(a)*r*1.25,Math.Sin(a)*r*.70,r*.08,c,1.1,.62,a);}rig.Pulse(r*1.18,c,t,.72);}
                else if(mode==1){for(int i=0;i<4;i++){double a=-.92+i*.62,q=.55+.22*Math.Sin(phase+i);rig.Tether(r*.10,0,Math.Cos(a)*r*(.92+q),Math.Sin(a)*r*(.52+q*.35),r*.10,c,1.05,.42+.08*i,phase+i);}rig.Arc(r*1.42,c,1.25,-1.05,1.05,.70);}
                else{var directed=rig.At(T(angle:s.FiniteRotation));directed.Fin(r*.62,0,r*.72,r*.18,c,0,.90);directed.Fin(r*.96,0,r*.48,r*.13,c,0,.62);rig.Pulse(r*1.32,c,t);}
            }else if(family=="pulse"){
                int lobes=mode==0?6:mode==1?8:10;double rr=r*(1.24+(mode==2?.12:0));var points=new Vector2[61];for(int i=0;i<=60;i++){double a=i/60.0*tau,q=1+.12*Math.Sin(a*lobes+phase)+.05*Math.Sin(a*(lobes+2)-phase*1.4);points[i]=P(Math.Cos(a)*rr*q,Math.Sin(a)*rr*.56*q);}rig.Poly(c,mode==2?1.55:1.15,.62,points);if(mode==1)rig.Pulse(r*1.48,c,t,.75);if(mode==2)rig.Arc(r*1.58,A(c,.95),1.6,-.28,.28,.85);
            }else if(family=="prism"){
                int count=mode==0?3:mode==1?4:5;for(int i=0;i<count;i++){double a=(i-(count-1)/2.0)*.22+Math.Sin(phase+i)*.03,rr=r*(1.10+(mode==2?.18:0));rig.Shard(Math.Cos(a)*rr,Math.Sin(a)*rr*.38,r*(mode==2?.15:.11),c,a,.78);}if(mode==1)rig.Tether(r*.62,0,r*1.58,0,r*.08,Color.white,.8,.52,phase);if(mode==2)rig.At(T(angle:s.FiniteRotation)).Fin(r*.72,0,r*.88,r*.17,c,0,.92);
            }else{
                // The source applies this turn even when the outer directed
                // mutation layer has already rotated. Keep that authored order.
                var directed=family=="sniper"||family=="shotgun"||family=="standard"||family=="void"?rig.At(T(angle:s.FiniteRotation)):rig;
                if(mode==0){for(int y=-1;y<=1;y+=2)directed.Fin(r*.62,y*r*.28,r*.56,r*.14,c,y>0?.18:-.18,.72);directed.Pulse(r*1.22,c,t,.72);}
                else if(mode==1){for(int i=0;i<4;i++){double q=i/3.0;directed.Fin(r*(.52+q*.58),Math.Sin(phase+i)*r*.34,r*(.34+.10*q),r*(.11+.02*q),c,phase+i,.72-.08*q);}directed.Arc(r*1.46,c,1.15,-.70,.70,.70);}
                else{directed.Fin(r*.54,0,r*1.02,r*.22,c,0,.94);directed.Poly(c,1.35,.82,P(r*.82,-r*.30),P(r*1.62,0),P(r*.82,r*.30));directed.Pulse(r*1.52,c,t,.82);}
            }
        }
        private void MutationEmitter(MutationRig layer,MutationView s)
        {
            double r=s.Radius,f=s.Final;string b=s.Branch;var c=s.Color;var rig=layer.At(T(r*1.03));var fill=C(5,11,19,.96);
            if(b.StartsWith("standard_",StringComparison.Ordinal)){
                double n=f+2;for(int i=0;i<n;i++){double y=(i-(n-1)/2)*r*.15;rig.Poly(c,1.05,.86,P(-r*.15,y),P(r*.34,y*.82));}double rad=r*(.07+.02*f);rig.Ring(r*.33,0,rad,rad,fill,c,1.05,.86);
            }else if(b.StartsWith("sniper_",StringComparison.Ordinal)){
                rig.Poly(c,1.05,.86,P(-r*.14,0),P(r*.40,0));if(b=="sniper_oracle")rig.Arc(r*.20,c,1,-.9,.9,.8);else if(b=="sniper_assassin")rig.Fin(r*.12,0,r*(.46+.08*f),r*.10,c,0,.88);else rig.Poly(c,1.05,.86,P(r*.06,-r*.16),P(r*.06,r*.16));
            }else if(b.StartsWith("shotgun_",StringComparison.Ordinal)){
                int n=b=="shotgun_cataclysm"?1:b=="shotgun_hail"?4:3;for(int i=0;i<n;i++){double y=(i-(n-1)/2.0)*r*.14;rig.Poly(c,1.05,.86,P(-r*.16,y),P(r*.38,y));}if(b=="shotgun_cataclysm")rig.Ring(r*.35,0,r*.09,r*.09,C(255,169,77,.78),null,0,.86);
            }else if(b.StartsWith("prism_",StringComparison.Ordinal)){
                int n=b=="prism_split"?3:b=="prism_spectrum"?5:2;for(int i=0;i<n;i++){double y=(i-(n-1)/2.0)*r*.13;rig.Stroke(QuadraticContour(P(-r*.12,0),false,P(r*.15,y*.2),P(r*.38,y)),c,1.05,.86);}rig.Shard(r*.37,0,r*(.12+.02*f),c,0,.82);
            }else if(b.StartsWith("void_",StringComparison.Ordinal)){
                if(b=="void_hunger")rig.Ring(r*.25,0,r*.16,r*.11,null,c,1.05,.86);else if(b=="void_reaper")rig.Fin(r*.2,0,r*(.48+.08*f),r*.13,c,0,.9);else rig.Poly(c,1.05,.86,P(r*.02,-r*.18),P(r*(.42+.08*f),0),P(r*.02,r*.18));
            }
        }
    }
}
