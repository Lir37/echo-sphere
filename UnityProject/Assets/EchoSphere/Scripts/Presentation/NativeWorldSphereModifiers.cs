using System;
using Unity.VectorGraphics;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldSphereFamilies
    {
        internal void DrawModifiers(SourceCanvasSurface surface,double x,double y,double time,double scale,bool disabled,double[] levels)
        {
            _surface=surface;_last=null;double radius=24*scale*1.38,active=disabled?0:time;var root=T(x,y);
            for(int kind=0;kind<6;kind++){
                double value=levels[kind];if(value<=0)continue;double strength=Math.Min(1,value/2),size=Math.Max(2.8,3.8*scale+strength*1.25*scale),angle=-Math.PI/2+kind*Math.PI*2/6+active*.11,width=Math.Max(.85,1.05*scale),alpha=.56+strength*.25;
                var local=root*T(Math.Cos(angle)*radius,Math.Sin(angle)*radius*.86,angle+Math.PI/2+active*.18);Color color;
                switch(kind){case 0:color=C(223,248,255);break;case 1:color=C(233,255,255);break;case 2:color=C(184,151,255);break;case 3:color=C(255,116,61);break;case 4:color=C(191,244,255);break;default:color=C(109,240,169);break;}
                Art Glyph(int id,Func<BezierContour[]> create,double? ownWidth=null)=>Get(id,size,0,ownWidth??width,create,cap:PathEnding.Round,corner:PathCorner.Round);
                void Stroke(Art art,double opacity)=>Paint(art,local,null,color,opacity,true);
                switch(kind){
                    case 0:{int lanes=Math.Min(4,2+(int)Math.Floor(strength*2));for(int i=0;i<lanes;i++){int index=i;double yy=(i-(lanes-1)/2.0)*size*.46;Stroke(Get(200+lanes*4+index,size,0,width,()=>new[]{QuadraticContour(P(-size*.92,yy*.42),false,P(0,yy),P(size*.95,yy*.34))},cap:PathEnding.Round,corner:PathCorner.Round),alpha);}break;}
                    case 1:
                        Stroke(Glyph(230,()=>new[]{Line(-size*1.05,0,size*1.05,0)}),alpha);Stroke(Glyph(231,()=>new[]{PolylineContour(false,P(size*.55,-size*.32),P(size*1.05,0),P(size*.55,size*.32)),PolylineContour(false,P(-size*.55,-size*.32),P(-size*1.05,0),P(-size*.55,size*.32))}),alpha);break;
                    case 2:
                        Stroke(Glyph(232,()=>new[]{PolylineContour(false,P(-size*.95,size*.12),P(-size*.58,-size*.62),P(size*.12,-size*.44),P(size*.82,-size*.84),P(size*.98,-.08*size))}),alpha);Stroke(Glyph(233,()=>new[]{EllipseContour(P(size*.84,-size*.06),(float)(size*.18),(float)(size*.18))}),alpha);break;
                    case 3:
                        Stroke(Glyph(234,()=>new[]{QuadraticContour(P(0,-size),false,P(size*.95,-size*.20),P(size*.55,size*.58),P(0,size),P(-size*.55,size*.58),P(-size*.95,-size*.20),P(0,-size))}),alpha);Stroke(Glyph(235,()=>new[]{QuadraticContour(P(0,-size*.36),false,P(size*.42,0),P(0,size*.52),P(-size*.42,0),P(0,-size*.36))}),alpha);break;
                    case 4:
                        width=Math.Max(.7,size*.18);for(int i=0;i<6;i++){double a=i*Math.PI/3;Stroke(Glyph(240+i,()=>new[]{Line(Math.Cos(a)*size*.18,Math.Sin(a)*size*.18,Math.Cos(a)*size,Math.Sin(a)*size)}),alpha);}Stroke(Glyph(246,()=>new[]{EllipseContour(Vector2.zero,(float)(size*.22),(float)(size*.22))}),alpha);break;
                    case 5:
                        Stroke(Glyph(247,()=>new[]{QuadraticContour(P(0,-size),false,P(size*.86,-size*.20),P(size*.72,size*.34),P(size*.50,size),P(0,size),P(-size*.50,size),P(-size*.72,size*.34),P(-size*.86,-size*.20),P(0,-size))}),alpha);Stroke(Glyph(248,()=>new[]{Line(0,-size*.48,0,size*.52)}),alpha);break;
                }
                if(value>=2){double rad=Math.Max(.65,.8*scale);for(int i=0;i<Math.Min(3,Math.Floor(value));i++){double xx=-size*.82+i*size*.42;var mark=Get(249+i,size,rad,width,()=>new[]{EllipseContour(P(xx,size*1.28),(float)rad,(float)rad)},cap:PathEnding.Round,corner:PathCorner.Round);Stroke(mark,.34+strength*.22);}}
            }
            Finish();
        }
    }
}
