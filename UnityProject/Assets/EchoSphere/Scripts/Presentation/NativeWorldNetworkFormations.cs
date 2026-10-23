using System;
using System.Linq;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldNetwork
    {
        private void Triangle(Node[] nodes,Formation f,double t)
        {
            var orange=C(255,184,77);double factor=f.Active?1:.30;
            var hull=PolylineContour(true,f.Nodes.Take(3).Select(i=>P(nodes[i].X,nodes[i].Y)).ToArray());
            Paint(hull,0,orange,(.028+.018*Math.Sin(t*3))*factor,Matrix4x4.identity);
            Paint(hull,1,C(255,207,115),(.24+.08*Math.Sin(t*4))*factor,Matrix4x4.identity,orange,4);
            var center=Center(f,nodes);bool pulse=f.Nodes.Any(i=>nodes[i].Pulse>0);
            foreach(int i in f.Nodes){var s=nodes[i];double charge=s.Charge%3,radius=25+Math.Sin(t*5+i)*1.5,width=1.4;var root=T(s.X,s.Y);Color? shadow=null;float blur=0;
                Circle(0,0,radius,width,orange,.20,root,start:-Math.PI/2,end:Math.PI*1.5);
                if(charge>0){shadow=orange;blur=4;Circle(0,0,radius,width,orange,.86,root,shadow,blur,start:-Math.PI/2,end:-Math.PI/2+Math.PI*2*charge/3);}
                if(s.Pulse>0){double flash=s.Pulse/.45;shadow=C(255,240,194);blur=6;width=2.6;Circle(0,0,radius+5+(1-flash)*10,width,orange,.45+.4*flash,root,shadow,blur,start:-Math.PI/2,end:Math.PI*1.5);}
                for(int tick=0;tick<3;tick++){double a=-Math.PI/2+tick/3.0*Math.PI*2,inner=radius+2,outer=inner+(tick<charge?5:2);Segment(Math.Cos(a)*inner,Math.Sin(a)*inner,Math.Cos(a)*outer,Math.Sin(a)*outer,width,orange,tick<charge?.95:.30,root,shadow,blur);}
            }
            double scale=1+(pulse?.28+.08*Math.Sin(t*24):.04*Math.Sin(t*6));Circle(0,0,pulse?4.5:2.5,0,orange,pulse?.9:.26,T(center.X,center.Y,scale),pulse?C(255,240,194):orange,7);
        }
        private void Square(Node[] nodes,Formation f,double t)
        {
            var c=Center(f,nodes);var root=T(c.X,c.Y,1+Math.Sin(t*4.8)*.035);var color=C(105,183,255);
            Paint(PolylineContour(true,f.Nodes.Select(i=>P(nodes[i].X-c.X,nodes[i].Y-c.Y)).ToArray()),1.8,color,.28+.08*Math.Sin(t*4),root,color,4);
            double radius=Radius(f,nodes,c)+22;Circle(0,0,radius,1.8,color,.16+.05*Math.Sin(t*6),root,color,6);Circle(0,0,4.5,0,color,.30+.10*Math.Sin(t*8),root,color,6);
            for(int i=0;i<4;i++){double a=t*.8+i*Math.PI/2;Circle(Math.Cos(a)*radius,Math.Sin(a)*radius,2.2,0,color,.65,root,color,6);}
        }
        private void Cluster(Node[] nodes,Formation f,double t)
        {
            var c=Center(f,nodes);double radius=Radius(f,nodes,c);var color=C(179,140,255);
            Circle(c.X,c.Y,radius+18+Math.Sin(t*3)*3,1.4,color,.20+.07*Math.Sin(t*3.2),Matrix4x4.identity,color,5);
            foreach(int i in f.Nodes)Segment(c.X,c.Y,nodes[i].X,nodes[i].Y,.8,C(206,178,255,.42),.55,Matrix4x4.identity,color,5);
            Circle(c.X,c.Y,4.2*(1+Math.Sin(t*3.4)*.035),0,color,.42+.1*Math.Sin(t*7),Matrix4x4.identity,color,6);
        }
    }
}
