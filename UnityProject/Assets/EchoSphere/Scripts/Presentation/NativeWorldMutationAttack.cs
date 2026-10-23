using System;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldSphereFamilies
    {
        private void MutationAttack(MutationRig rig,MutationView s)
        {
            double delay=Math.Max(.15,s.AttackDelay),progress=Math.Max(0,Math.Min(1,1-s.AttackTimer/delay)),p=Math.Exp(-Math.Pow((progress-.95)/.07,2));if(!Finite(p)||p<.03)return;
            double r=s.Radius,t=s.Time,f=s.Final,strength=.28+.72*p;var c=s.Color;string b=s.Branch;const double tau=Math.PI*2;
            if(b=="standard_resonator"){for(int i=0;i<3;i++){double a=t*.55+i*tau/3;rig.Fin(Math.Cos(a)*r*1.18,Math.Sin(a)*r*.72,r*.44,r*.13,c,a+.2,.45+.4*p);}rig.Pulse(r*1.25,c,t);}
            else if(b=="standard_singularity"){for(int i=0;i<4;i++){double a=-t*.35+i*tau/4;rig.Tether(Math.Cos(a)*r*1.15,Math.Sin(a)*r*.7,Math.Cos(a+.6)*r*.42,Math.Sin(a+.6)*r*.25,r*.16,c,1.1,.35+.4*p,t+i);}rig.Arc(r*(1.08+p*.42),C(216,185,255),1.2,.4,2.55,.8);}
            else if(b=="standard_swarm")for(int i=0;i<5;i++){double a=t*.48+i*tau/5;rig.Shard(Math.Cos(a)*r*(1+strength*.5),Math.Sin(a)*r*.67,r*.16,c,a+.7,.45+.4*p);}
            else if(b=="sniper_oracle"){rig.Ring(r*1.15,0,r*(.25+.07*p),r*(.25+.07*p),null,C(245,221,255),1.2,.45+.45*p,-1,1);rig.Poly(C(245,221,255),1.2,.45+.45*p,P(r*.75,-r*.06),P(r*(1.62+.20*p),0),P(r*.75,r*.06));}
            else if(b=="sniper_assassin")for(int y=-1;y<=1;y+=2)rig.Stroke(QuadraticContour(P(r*.65,y*r*.16),false,P(r*(1.20+p*.25),y*r*.30),P(r*(1.72+p*.20),y*r*.05)),c,1.2,.35+.45*p);
            else if(b=="sniper_beacon"){for(int i=0;i<3;i++)rig.Pulse(r*(1.05+i*.25),c,t+i*.17);rig.Fin(0,-r*1.05,r*.40,r*.13,c,-Math.PI/2,.8);}
            else if(b=="shotgun_burst")for(int i=0;i<3;i++)rig.Poly(c,1.3,.4+.3*p,P(r*.72,(i-1)*r*.15),P(r*(1.58+p*.3),(i-1)*r*.22));
            else if(b=="shotgun_cataclysm"){rig.Poly(C(255,210,156),1.4,.4+.5*p,P(r*.65,-r*.20),P(r*(1.28+p*.30),0),P(r*.65,r*.20));rig.Ring(r*(1.35+p*.2),0,r*(.08+.06*p),r*(.08+.06*p),null,C(255,210,156),1.4,.4+.5*p);}
            else if(b=="shotgun_hail")for(int i=0;i<6;i++){double a=-.75+i*.30,rr=r*(1.05+p*.55);rig.Shard(Math.Cos(a)*rr,Math.Sin(a)*rr*.55,r*.10,c,a+.7,.35+.4*p);}
            else if(b=="chain_web"||b=="chain_storm"||b=="chain_leech"){int n=b=="chain_storm"?7:8;for(int i=0;i<n;i++){double a=t*1.3+i*tau/n,rr=r*(.8+strength*.9);rig.Tether(Math.Cos(a)*r*.18,Math.Sin(a)*r*.13,Math.Cos(a)*rr,Math.Sin(a)*rr*.62,r*.10,c,b=="chain_storm"?1.35:1,.30+.45*p,a);}}
            else if(b=="aura_sanctum"||b=="aura_gravity"||b=="aura_overgrowth"){for(int i=0;i<6;i++){double a=t*.3+i*tau/6,rr=r*(1+strength*.8);rig.Tether(Math.Cos(a)*r*.25,Math.Sin(a)*r*.16,Math.Cos(a)*rr,Math.Sin(a)*rr*.58,r*.18,c,1,.22+.5*p,a);}rig.Arc(r*(1.15+strength*.55),c,1,-Math.PI*.75,Math.PI*.75,.35+.35*p);}
            else if(b=="orbital_dance"){int n=f==2?6:4;for(int i=0;i<n;i++){double a=t*1.8+i*tau/n;rig.Fin(Math.Cos(a)*r*(1.04+strength*.5),Math.Sin(a)*r*.62,r*(.30+.18*p),r*.12,c,a,.32+.48*p);}}
            else if(b=="orbital_halo"){for(int i=0;i<3;i++)rig.Pulse(r*(1.08+i*.18),c,t+i*.16,.75+.18*p);rig.Arc(r*1.48,c,1.2,-.72,.72,.55+.25*p);}
            else if(b=="orbital_blade")for(int i=0;i<4;i++){double a=t*.72+i*tau/4;rig.Fin(Math.Cos(a)*r*1.14,Math.Sin(a)*r*.68,r*(.38+.12*p),r*.10,c,a,.45+.45*p);}
            else if(b=="prism_split"||b=="prism_spectrum"||b=="prism_mirror"){int n=b=="prism_spectrum"?5:b=="prism_split"?3:2;for(int i=0;i<n;i++)rig.Shard(r*(1+strength*.25),(i-(n-1)/2.0)*r*.28,r*.13,c,0,.38+.45*p);rig.Poly(Color.white,1,.30+.45*p,P(r*.72,0),P(r*(1.68+p*.2),0));}
            else if(b=="gravity_well"||b=="gravity_tide"||b=="gravity_collapse")for(int i=0;i<3;i++){double a=-t*.3+i/3.0*1.5;var points=new Vector2[15];for(int k=0;k<=14;k++){double u=k/14.0,rr=r*(.35+u*(.85+strength*.7)),angle=a+u*2.15;points[k]=P(Math.Cos(angle)*rr,Math.Sin(angle)*rr*.56);}rig.Poly(c,1.1,.22+.46*p,points);}
            else if(b=="pulse_wave"||b=="pulse_resonator"||b=="pulse_burst"){int lobes=b=="pulse_resonator"?8:b=="pulse_wave"?6:5;double rr=r*(.9+strength*.85);var points=new Vector2[49];for(int i=0;i<=48;i++){double a=i/48.0*tau,q=1+.16*Math.Sin(a*lobes+t*4.8)+.08*Math.Sin(a*(lobes+2)-t*3.4);points[i]=P(Math.Cos(a)*rr*q,Math.Sin(a)*rr*.54*q);}rig.Poly(c,1.1,.25+.55*p,points);if(b=="pulse_burst")rig.Arc(rr*.46,C(255,240,169),1,0,tau,.40+.4*p);}
            else if(b=="void_hunger"||b=="void_reaper"||b=="void_execution"){for(int i=0;i<4;i++){double a=t*.24+i*tau/4;rig.Tether(Math.Cos(a)*r*.3,Math.Sin(a)*r*.2,Math.Cos(a+.55)*r*(1.1+strength*.55),Math.Sin(a+.55)*r*.65,r*.18,c,1.25,.30+.46*p,a);}if(b=="void_reaper")rig.Fin(r*1.02,0,r*(.62+.18*p),r*.18,c,0,.8);else if(b=="void_execution")rig.Poly(c,1.25,.4+.4*p,P(r*.8,-r*.28),P(r*(1.55+.25*p),0),P(r*.8,r*.28));}
        }
    }
}
