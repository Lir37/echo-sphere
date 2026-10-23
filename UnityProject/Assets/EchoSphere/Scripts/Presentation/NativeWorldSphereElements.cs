using System;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldSphereFamilies
    {
        // Consecutive Canvas arc calls connect their starts to the preceding arc.
        // Preserve these authored connectors in poison clouds, not separate rings.
        private static BezierContour Connected(params BezierContour[] arcs)
        {
            var segments=new List<BezierPathSegment>(arcs[0].Segments);for(int i=1;i<arcs.Length;i++){var next=arcs[i].Segments;int last=segments.Count-1;var previous=segments[last];
                if((previous.P0-next[0].P0).sqrMagnitude>1e-10f){previous.P1=Vector2.Lerp(previous.P0,next[0].P0,1f/3);previous.P2=Vector2.Lerp(previous.P0,next[0].P0,2f/3);segments[last]=previous;segments.AddRange(next);}
                else{previous.P1=next[0].P1;previous.P2=next[0].P2;segments[last]=previous;for(int j=1;j<next.Length;j++)segments.Add(next[j]);}}
            return new BezierContour {Segments=segments.ToArray(),Closed=false};
        }
        private void ElementGlyph(Matrix4x4 root,string element,double size,int variant,Color color,double width,double opacity)
        {
            void Stroke(int id,Func<BezierContour[]> create)=>Paint(Get(id,size,variant,width,create),root,null,color,opacity,true);
            if(element=="fire"){
                double lean=(variant-1)*size*.12;Stroke(300,()=>new[]{QuadraticContour(P(lean,-size),true,P(size*(.92+variant*.06),-size*.20),P(size*(.44-variant*.05),size*.55),P(lean,size*.92),P(-size*(.46+variant*.03),size*.58),P(-size*(.90-variant*.04),-size*.18),P(lean,-size))});
                Stroke(301,()=>new[]{QuadraticContour(P(lean,-size*.34),false,P(size*.40,-size*.02),P(lean+size*.04,size*.46),P(-size*.30,size*.18),P(lean,-size*.34))});
            }else if(element=="freeze"){
                int arms=variant==1?4:6;for(int i=0;i<arms;i++){double a=i*Math.PI*2/arms;Stroke(310+i,()=>new[]{Line(Math.Cos(a)*size*.14,Math.Sin(a)*size*.14,Math.Cos(a)*size,Math.Sin(a)*size)});
                    if(variant!=2){double tip=variant==1?.68:.62,branch=variant==1?.42:.34;Stroke(320+i,()=>new[]{Line(Math.Cos(a)*size*tip,Math.Sin(a)*size*tip,Math.Cos(a+branch)*size*.88,Math.Sin(a+branch)*size*.88),Line(Math.Cos(a)*size*tip,Math.Sin(a)*size*tip,Math.Cos(a-branch)*size*.88,Math.Sin(a-branch)*size*.88)});}}
                Stroke(326,()=>new[]{EllipseContour(Vector2.zero,(float)(size*(variant==2?.28:.20)),(float)(size*(variant==2?.28:.20)))});
            }else{
                double drift=(variant-1)*size*.08;Stroke(330,()=>new[]{Connected(EllipseContour(P(-size*.34+drift,size*.10),(float)(size*.34),(float)(size*.34)),EllipseContour(P(0,-size*(.12+variant*.04)),(float)(size*(.42-variant*.03)),(float)(size*(.42-variant*.03))),EllipseContour(P(size*.34+drift,size*.08),(float)(size*(.29+variant*.02)),(float)(size*(.29+variant*.02))))});
                Stroke(331,()=>new[]{Connected(EllipseContour(P(size*(.10-variant*.04),-size*(.50+variant*.04)),(float)(size*(.11+variant*.02)),(float)(size*(.11+variant*.02))),EllipseContour(P(-size*.22,-size*.42),(float)(size*.07),(float)(size*.07)))});
            }
        }
        internal void DrawElements(NativeWorldSurface surface,double x,double y,double direction,double time,double scale,string family,string element,Color color,bool active)
        {
            _surface=surface;double r=24*scale*(family=="orbital"?1.46:1.34),rx=r*1.16,ry=r*.34,spin=-time*.18,particleSpin=-time*.34,alpha=active?.78:.20,width=Math.Max(1,r*.040);Matrix4x4 root=T(x,y),orbit=root*T(angle:spin);
            Paint(Get(340,r,0,Math.Max(2,width*3),()=>new[]{EllipseContour(Vector2.zero,(float)rx,(float)ry)}),orbit,null,A(color,alpha*.18),additive:true);
            Paint(Get(341,r,0,width,()=>new[]{EllipseContour(Vector2.zero,(float)rx,(float)ry,Mathf.PI,Mathf.PI*2)}),orbit,null,A(color,alpha*.58),additive:true);
            Paint(Get(342,r,0,width,()=>new[]{EllipseContour(Vector2.zero,(float)rx,(float)ry,0,Mathf.PI)}),orbit,null,A(color,alpha),additive:true);
            double highlightWidth=Math.Max(.75,width*.55),start=particleSpin%(Math.PI*2);Paint(Dynamic(new[]{EllipseContour(Vector2.zero,(float)(rx*.995),(float)(ry*.995),(float)start,(float)(start+.72))},highlightWidth),orbit,null,A(color,alpha*.72),additive:true);
            for(int i=0;i<6;i++){double a=particleSpin+i*Math.PI*2/6,px=Math.Cos(a)*rx,py=Math.Sin(a)*ry,bob=Math.Sin(time*2.8+i*1.73)*r*.018,depth=.38+.62*((Math.Sin(a)+1)*.5),size=Math.Max(2.2,r*(.055+(i%3)*.008));ElementGlyph(root*T(px,py+bob,a+Math.PI/2),element,size,i%3,A(color,alpha*.72),highlightWidth,alpha*(.72+depth*.28));
                if(i%2==0){double side=i%4==0?1:-1,ma=a+side*.16,rad=Math.Max(.65,r*.018);Paint(Circle(rad),root*T(Math.Cos(ma)*rx*1.035,Math.Sin(ma)*ry*1.18),color,null,alpha*(.28+depth*.24),true);}}
            if(family=="chain"||family=="aura"||family=="gravity"||family=="pulse"){
                double signature=active?.42:.10,sw=Math.Max(.8,r*.024);
                if(family=="chain")for(int i=0;i<3;i++){double a=time*.9+i*Math.PI*2/3,px=Math.Cos(a)*r*1.02,py=Math.Sin(a)*r*.30;Paint(Dynamic(new[]{Line(px,py,px+Math.Cos(a+1.2)*r*.18,py+Math.Sin(a+1.2)*r*.12)},sw),root,null,color,additive:true);}
                else if(family=="aura"){
                    Paint(Get(343,r,0,sw,()=>new[]{EllipseContour(Vector2.zero,(float)(r*1.85),(float)(r*.58))}),root,null,color,signature*.72,true);
                    for(int i=0;i<3;i++){double a=-time*.45+i*Math.PI*2/3;ElementGlyph(root*T(Math.Cos(a)*r*1.55,Math.Sin(a)*r*.48,a+Math.PI/2),element,r*.065,i,color,sw,signature*.65);}}
                else if(family=="gravity")for(int i=0;i<3;i++){double q=((time*.35+i/3.0)%1+1)%1,rr=r*(1.72-q*.72);Paint(Dynamic(new[]{EllipseContour(Vector2.zero,(float)rr,(float)rr,(float)(-.55+i*2),(float)(.55+i*2))},sw),root,null,color,signature*(1-q)*.8,true);}
                else{double q=((time*.72)%1+1)%1,rr=r*(.72+q*1.15);Paint(Dynamic(new[]{EllipseContour(Vector2.zero,(float)rr,(float)rr)},sw),root,null,color,signature*(1-q),true);}
            }
            if(family!="orbital"){
                double weaponRadius=24*scale,pulse=.75+.25*Math.Sin(time*8+x*.01),reach=weaponRadius*1.05,opacity=(active?.72:.18)*pulse;var local=T(x+Math.Cos(direction)*reach,y+Math.Sin(direction)*reach,direction);
                ElementGlyph(local,element,weaponRadius*(element=="fire"?.12:element=="freeze"?.11:.10),0,color,Math.Max(.85,1.1*scale),opacity);
                double rad=weaponRadius*.10;Paint(Get(344,rad,0,0,()=>new[]{EllipseContour(P(rad,0),(float)rad,(float)rad)},true),local,A(color,.16),null,opacity*.56,true);
            }

        }
    }
}
