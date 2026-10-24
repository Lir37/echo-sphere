using System;
using UnityEngine;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldCreatures
    {
        private static readonly double[] WispLegs={-.28,.20},LeechLegs={-.36,-.02},SkitterLegs={-.40,-.04,.32},MothLegs={-.16,.10},BeetleLegs={-.34,.12},BruteLegs={-.45,-.12,.20,.48},HoundLegs={-.34,.10},StalkerLegs={-.22,.16},WidowLegs={-.36,-.12,.12,.36};
        private static readonly double[] LancerLegs={-.30,.18},ChargerLegs={-.34,.20},BroodLegs={-.34,.18},LeviathanLegs={-.28,.10};
        private void Creature(Matrix4x4 m,double r,Color color,double t,string variant)
        {
            switch(variant)
            {
                case "wisp":
                    Body(m,r,color,Math.Sin(t*2.4)*.04,.82);Wing(m,-r*.18,-r*.38,r*.82,r*.34,-1.15,color,t*5);Wing(m,-r*.18,r*.38,r*.82,r*.34,1.15,color,t*5);Legs(m,r,WispLegs,.30,.58,t*3,color,.60);Eye(m,r*.30,-r*.12,r*.13,color,-.08);Core(m,-r*.16,r*.10,r*.09,color);break;
                case "leech":
                    for(int i=0;i<7;i++){double q=i/6.0,x=-r*.62+q*r*1.22,y=Math.Sin(t*3.2+q*4.8)*r*.14;Body(m*T(x,y,Math.Sin(t*3+q*3.2)*.14),r*(.34-.025*q),color,0,.72);if(i<6)Joint(m,x+r*.14,y,r*.07,color);}
                    var leech=m*T(r*.55,Math.Sin(t*3.2+5)*r*.10);Body(leech,r*.34,color,.05,.66);Eye(leech,r*.14,-r*.10,r*.09,color);Eye(leech,r*.14,r*.10,r*.09,color);Legs(m,r,LeechLegs,.20,.30,t*4,color,.34);break;
                case "serpent":
                    for(int i=0;i<8;i++){double q=i/7.0,x=r*.46-q*r*1.10,y=Math.Sin(t*3.1+q*4.8)*r*(.08+.06*q),dx=-r*1.10/7,dy=Math.Cos(t*3.1+q*4.8)*r*(.08+.06*q)*4.8/7;Body(m*T(x,y),r*(.29-.018*i),color,Math.Atan2(dy,dx),.64);if(i<7)Joint(m,x+dx*.5,y+dy*.5,r*.055,color);}
                    var serpent=m*T(r*.50,-r*.06);Body(serpent,r*.36,color,-.06,.62);Eye(serpent,r*.14,-r*.10,r*.09,color);Eye(serpent,r*.14,r*.10,r*.09,color);
                    var fang=Get(4,r,0,1,()=>Poly(P(r*.12,0),P(r*.34,r*.05),P(r*.12,r*.10)));Paint(fang,serpent,C(2,5,10),color);break;
                case "skitter":
                    OrganicSkitter(m,r,color,t);break;
                case "moth":
                    Wing(m,-r*.08,-r*.40,r*.92,r*.62,-2.72,color,t*8);Wing(m,-r*.08,r*.40,r*.92,r*.62,2.72,color,t*8);Body(m*T(sx:.45,sy:1.25),r*.72,color,0,.9);Legs(m,r,MothLegs,.28,.48,t*8,color,.36);Eye(m,-r*.07,-r*.26,r*.09,color);Eye(m,r*.07,-r*.26,r*.09,color);break;
                case "beetle":
                    Body(m,r,color,Math.Sin(t*1.6)*.03,.68);
                    for(int i=-1;i<=1;i++){int index=i;var ridge=Get(20+i,r,0,Math.Max(.6,r*.025),()=>Quadratic(P(-r*.52,index*r*.12),false,P(0,index*r*.22),P(r*.50,index*r*.08)),false);Paint(ridge,m,null,C(235,249,255,.28));}
                    Legs(m,r,BeetleLegs,.24,.55,t*3,color,.55);Eye(m,r*.48,-r*.10,r*.07,color);Eye(m,r*.48,r*.10,r*.07,color);Core(m,-r*.22,0,r*.08,color);break;
                case "brute":
                    Body(m,r*1.05,color,Math.Sin(t*1.7)*.025,.88);Legs(m,r,BruteLegs,.30,.72,t*6,color,.78);
                    var spike=Get(5,r,0,1,()=>Poly(Vector2.zero,P(r*.10,-r*.30),P(r*.20,0)));for(int i=0;i<3;i++)Paint(spike,m*T(-r*.25+i*r*.22,-r*.40,-.1+i*.1),C(6,10,16,.98),C(230,240,250,.46));Eye(m,r*.36,-r*.17,r*.11,color);Eye(m,r*.36,r*.17,r*.11,color);break;
                case "prism":
                    Body(m,r*.92,color,Math.Sin(t*2.5)*.04,.62);Legs(m,r,HoundLegs,.30,.60,t*5,color,.48);var hound=m*T(r*.48,-r*.10);Body(hound,r*.42,color,-.12,.62);Eye(hound,r*.13,-r*.10,r*.09,color);Tail(m,-r*.96,0,r*.82,color,t,6,.20);break;
                case "stalker":
                    Body(m*T(y:Math.Sin(t*4)*r*.06),r*.72,color,0,1.25);Legs(m,r,StalkerLegs,.24,.90,t*5,color,.52);Eye(m,r*.10,-r*.24,r*.12,color);Core(m,-r*.12,r*.20,r*.075,color);break;
                default:
                    Body(m,r*.92,color,0,.72);Legs(m,r,WidowLegs,.18,.58,t*4,color,.40);var red=C(255,100,128);Eye(m,r*.26,-r*.10,r*.085,red);Eye(m,r*.26,r*.10,r*.085,red);Core(m,-r*.20,0,r*.08,red);break;
            }
        }
        private void Boss(Matrix4x4 m,double r,Color color,double t,string type)
        {
            Frame(m,r,color,t);
            switch(type)
            {
                case "shooter":
                    Body(m,r*1.12,color,Math.Sin(t*1.7)*.025,.62);Wing(m,-r*.18,-r*.48,r,r*.36,-1.15,color,t*4);Wing(m,-r*.18,r*.48,r,r*.36,1.15,color,t*4+.35);Legs(m,r,LancerLegs,.34,.62,t*3,color,.82);var lancer=m*T(r*.62);Body(lancer,r*.48,color,0,.58);Eye(lancer,r*.18,-r*.12,r*.14,color);Eye(lancer,r*.18,r*.12,r*.14,color);Tail(m,-r*.90,0,r*1.10,color,t,8,.16);Core(m,-r*.12,0,r*.12,color);break;
                case "charger":
                    Body(m,r*1.14,color,Math.Sin(t*2)*.025,.84);Legs(m,r,ChargerLegs,.35,.82,t*6,color,.90);var charger=m*T(r*.50);Body(charger,r*.50,color,0,.72);Eye(charger,r*.20,-r*.14,r*.15,color);Eye(charger,r*.20,r*.14,r*.15,color);
                    for(int side=-1;side<=1;side+=2)Wing(m,-r*.05,side*r*.50,r*.72,r*.25,side>0?.72:-.72,color,t*3+(side>0?0:.35));Core(m,-r*.30,0,r*.14,color);break;
                case "summoner":
                    Body(m,r,color,Math.Sin(t)*.02,.96);Legs(m,r,BroodLegs,.38,1,t*3,color,.95);var brood=m*T(r*.34,-r*.05);Body(brood,r*.52,color,-.05,.76);Eye(brood,r*.20,-r*.12,r*.14,color);Eye(brood,r*.20,r*.12,r*.14,color);
                    for(int i=0;i<5;i++){double a=t*.8+i*Math.PI*2/5;var child=m*T(Math.Cos(a)*r*1.10,Math.Sin(a)*r*.72,sx:.8+.15*Math.Sin(t*3+i));Body(child,r*.24,color,a,.9);Eye(child,0,-r*.03,r*.055,color);}Core(m,-r*.20,r*.06,r*.13,color);break;
                default:
                    for(int i=0;i<8;i++){double x=-r*.76+i*r*.22,y=Math.Sin(t*2+i*.72)*r*.16;Body(m*T(x,y),r*(.50-.035*i),color,Math.Sin(t+i)*.12,.70);}var leviathan=m*T(r*.54);Body(leviathan,r*.58,color,-.08,.68);Eye(leviathan,r*.22,-r*.16,r*.15,color);Eye(leviathan,r*.22,r*.16,r*.15,color);Legs(m,r,LeviathanLegs,.32,.82,t*2,color,.70);Tail(m,-r*.90,0,r*1.16,color,t,9,.14);Core(m,-r*.08,0,r*.12,color);break;
            }
        }
    }
}
