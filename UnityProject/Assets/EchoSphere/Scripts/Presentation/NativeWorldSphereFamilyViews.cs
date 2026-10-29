using System;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldSphereFamilies
    {
        internal void DrawFamily(NativeWorldSurface surface,NativeWorldStandardSphere cores,State s,bool includeCombatParts=true)
        {
            _surface=surface;double r=24*s.Scale,t=s.Time,pulse=s.Disabled?.5:.5+.5*Math.Sin(t*3.4+s.X*.008+s.Y*.006);Color color;
            switch(s.Family){case "sniper":color=C(232,108,255);break;case "shotgun":color=C(255,143,61);break;case "chain":color=C(255,226,91);break;case "aura":color=C(87,230,180);break;case "prism":color=C(255,141,225);break;case "gravity":color=C(165,140,255);break;case "pulse":color=C(255,211,90);break;default:color=C(194,140,255);break;}
            if(s.Disabled)color=C(255,77,112);else if(s.Resonance)color=C(255,209,102);
            double glowSize=2.4,glowAlpha=.12,orbitSize=1.03,rotation=0,opacity=.66;string mode=s.Family;
            switch(s.Family){case "sniper":glowSize=2.35;glowAlpha=.11;orbitSize=1.02;rotation=t*.20;opacity=s.Disabled?.16:.68;break;case "shotgun":orbitSize=1.02;rotation=t*.18;opacity=s.Disabled?.15:.64;break;case "chain":glowSize=2.35;glowAlpha=.11;orbitSize=1.02;rotation=t*.17;opacity=s.Disabled?.12:.78;mode="lightning";break;case "aura":glowSize=2.45;rotation=t*.14;opacity=s.Disabled?.12:.68;break;case "prism":rotation=t*.16;opacity=s.Disabled?.12:.66;break;case "gravity":glowAlpha=.11;rotation=-t*.12;opacity=s.Disabled?.12:.65;break;case "pulse":glowSize=2.45;rotation=(s.Disabled?0:t*1.10)*.13;opacity=s.Disabled?.10:.66;break;case "void":glowSize=2.35;glowAlpha=.10;rotation=t*.11;opacity=s.Disabled?.10:.58;break;}
            var root=T(s.X,s.Y);DrawGlow(surface,r*glowSize,color,s.Disabled?(s.Family=="void"?.04:.05):glowAlpha,root,true);DrawOrbit(surface,r*orbitSize,color,rotation,opacity,mode,pulse,root,true);cores.DrawCore(surface,r,s.Family=="void"?pulse*.92:pulse,root,color);
            var dark=C(7,17,29);bool d=s.Disabled;var aim=root*T(angle:s.Rotation);
#if UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS
            if(includeCombatParts)switch(s.Family){
                case "sniper":{
                    var tip=Get(100,r,0,1.15,()=>new[]{PolylineContour(true,P(0,-r*.24),P(r*.18,-r*.05),P(r*.13,r*.24),P(0,r*.34),P(-r*.13,r*.24),P(-r*.18,-r*.05))},true);var local=aim*T(r*1.22,0,-Math.PI/2);Paint(tip,local,dark,color,d?.14:.94);
                    Paint(Get(101,r,0,1.15,()=>new[]{Line(0,-r*.18,0,r*.16),Line(-r*.10,-r*.02,r*.10,-r*.02)}),local,null,Color.white,d?.08:.70);Paint(Circle(r*.13),local,color,null,d?.04:.10+.05*Math.Sin(t*7));
                    double a=t*.46+Math.PI*.72;var p=Point(r,a,.34,1.15);Paint(Get(102,r,0,.9,()=>new[]{EllipseContour(Vector2.zero,(float)(r*.10),(float)(r*.10)),Line(-r*.18,0,r*.18,0),Line(0,-r*.18,0,r*.18)}),aim*T(p.x,p.y),null,color,d?.08:.62*p.depth);break;}
                case "shotgun":{
                    var tip=Get(110,r,0,1.1,()=>new[]{PolylineContour(true,P(-r*.02,-r*.10),P(r*.22,-r*.11),P(r*.30,0),P(r*.22,r*.11),P(-r*.02,r*.10))},true);var bore=Get(111,r,0,1.1,()=>new[]{EllipseContour(P(r*.25,0),(float)(r*.055),(float)(r*.055))});for(int i=-1;i<=1;i++){var local=aim*T(r*1.18,i*r*.25);Paint(tip,local,dark,color,d?.12:.92);Paint(bore,local,null,Color.white,d?.07:.72);}
                    var ray=Get(112,r,0,.8,()=>new[]{Line(-r*.10,0,r*.10,0)});for(int i=0;i<3;i++){double a=t*(.34+i*.035)+i*Math.PI*2/3;var p=Point(r,a,.34,1.12);Paint(ray,aim*T(p.x,p.y),null,color,d?.08:.48*p.depth);}
                    if(s.AttackActive&&Math.Sin(t*8)>.6)Paint(Get(113,r,0,.9,()=>new[]{EllipseContour(P(r*1.43,0),(float)(r*.11),(float)(r*.11))}),aim,null,color,d?.05:.22);break;}
                case "chain":{
                    var conductor=Get(120,r,0,Math.Max(.8,r*.032),()=>new[]{PolylineContour(false,P(-r*.08,0),P(-r*.01,-r*.07),P(r*.07,r*.03),P(r*.14,-r*.02),P(r*.21,r*.06))});for(int i=0;i<4;i++){double a=i*Math.PI/2+t*.28;var p=Point(r,a,.34,1.10);Paint(conductor,root*T(p.x,p.y,a+Math.PI/2),null,color,(d?.10:.56)*p.depth);}
                    double q=.5+.5*Math.Sin(t*10),rr=r*(1.04+q*.08);Paint(Dynamic(new[]{EllipseContour(Vector2.zero,(float)rr,(float)rr)},1),root,null,color,d?.06:.20+q*.16);break;}
                case "aura":{
                    var blade=Get(130,r,0,.9,()=>new[]{PolylineContour(true,P(-r*.10,0),P(0,-r*.14),P(r*.11,0),P(0,r*.14))},true);for(int i=0;i<6;i++){double a=i*Math.PI/3+t*.10;var p=Point(r,a,.34,1.08);Paint(blade,root*T(p.x,p.y,a+Math.PI/2),dark,color,(d?.07:.48)*p.depth);}
                    double q=.5+.5*Math.Sin(t*2.7),rr=r*(1.04+q*.06);Paint(Dynamic(new[]{EllipseContour(Vector2.zero,(float)rr,(float)rr)},1),root,null,color,d?.05:.16+q*.08);break;}
                case "prism":{
                    var emitter=Get(140,r,0,1.05,()=>new[]{PolylineContour(true,P(0,-r*.20),P(r*.17,-r*.06),P(r*.27,0),P(r*.17,r*.06),P(0,r*.20),P(r*.08,0))},true);var local=aim*T(r*1.18);Paint(emitter,local,dark,color,d?.12:.94);Paint(Get(141,r,0,1.05,()=>new[]{PolylineContour(false,P(r*.09,-r*.10),P(r*.21,0),P(r*.09,r*.10))}),local,null,Color.white,d?.06:.68);
                    var diamond=Get(142,r,0,.85,()=>new[]{PolylineContour(true,P(0,-r*.09),P(r*.07,0),P(0,r*.09),P(-r*.07,0))},true);for(int i=0;i<2;i++){double a=t*(.30+i*.05)+i*Math.PI;var p=Point(r,a,.34,1.10);Paint(diamond,aim*T(p.x,p.y,a+Math.PI/2),dark,color,d?.06:.42*p.depth);}break;}
                case "gravity":{
                    var anchor=Get(150,r,0,.9,()=>new[]{PolylineContour(true,P(-r*.09,0),P(0,-r*.12),P(r*.12,0),P(0,r*.12))},true);for(int i=0;i<5;i++){double a=i*Math.PI*2/5+t*.08;var p=Point(r,a,.34,1.08);Paint(anchor,root*T(p.x,p.y,a),dark,color,(d?.06:.44)*p.depth);}
                    double pull=.5+.5*Math.Sin(t*3.1),rr=r*(.98+pull*.05);Paint(Dynamic(new[]{EllipseContour(Vector2.zero,(float)rr,(float)rr)},.9),root,null,color,d?.05:.16+pull*.08);break;}
                case "pulse":{
                    double phase=(d?0:t)*1.10;var plate=Get(160,r,0,.8,()=>new[]{PolylineContour(true,P(-r*.08,-r*.10),P(r*.08,-r*.06),P(r*.08,r*.06),P(-r*.08,r*.10))},true);for(int i=0;i<4;i++){double a=phase*.28+i*Math.PI/2,q=.5+.5*Math.Sin(phase*2.8+i);var p=Point(r,a,.34,1.10);Paint(plate,root*T(p.x,p.y,a+Math.PI/2),dark,color,d?.06:(.36+q*.20)*p.depth);}
                    double q0=((phase*.42)%1+1)%1,rr=r*(1.02+q0*.86);Paint(Dynamic(new[]{EllipseContour(Vector2.zero,(float)rr,(float)rr)},1),root,null,color,d?.06:(1-q0)*.36);break;}
                case "void":{
                    var local=aim*T(r*1.20);var emitter=Get(170,r,0,1.1,()=>new[]{QuadraticContour(P(-r*.01,-r*.19),true,P(r*.21,-r*.08),P(r*.27,0),P(r*.21,r*.08),P(-r*.01,r*.19),P(r*.07,0),P(-r*.01,-r*.19))},true);Paint(emitter,local,C(3,7,14),color,d?.12:.94);Paint(Get(171,r,0,1.1,()=>new[]{EllipseContour(P(r*.18,0),(float)(r*.07),(float)(r*.07))}),local,null,C(234,223,255),d?.06:.65);
                    for(int i=0;i<4;i++){int index=i;double a=-t*(.22+i*.025)+i*Math.PI/2;var p=Point(r,a,.34,1.11);Paint(Get(172+i,r,0,.9,()=>new[]{EllipseContour(Vector2.zero,(float)(r*(.07+index*.012)),(float)(r*(.07+index*.012)),Mathf.PI*.2f,Mathf.PI*1.8f)}),aim*T(p.x,p.y),null,color,d?.05:.32*p.depth);}break;}
            }
#endif
            if(d){var art=Get(1,r,0,1.7,()=>new[]{EllipseContour(Vector2.zero,(float)(r*1.18),(float)(r*1.18),Mathf.PI*.20f,Mathf.PI*.80f)});Paint(art,(s.Family=="sniper"||s.Family=="shotgun"||s.Family=="prism"||s.Family=="void")?aim:root,null,color,.42);}

        }
    }
}
