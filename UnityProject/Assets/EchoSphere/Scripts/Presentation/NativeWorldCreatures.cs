using System;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // Typed presentation of enemyVisual.ts. Unity Vector Graphics owns curve
    // tessellation; authored anatomy and animation values remain source-derived.
    internal sealed partial class NativeWorldCreatures : IDisposable
    {
        private sealed class Art : IDisposable
        {
            internal BezierContour Contour;
            internal BezierContour[] Multi;
            internal Mesh Fill,Stroke;
            internal double Length,Width;
            internal PathEnding Ending;
            internal PathCorner Corner;
            internal float Miter;
            internal void Build(BezierContour contour,float width,PathEnding ending,PathCorner corner,float miter,bool fill=true,BezierContour[] all=null)
            {
                Contour=contour;Multi=all;
                if(fill)Fill=NativeWorldShapes.Tessellate(contour,0,Fill,contours:all);
                if(width>0)Stroke=NativeWorldShapes.Tessellate(contour,width,Stroke,cap:ending,corners:corner,miter:miter,contours:all);
            }
            public void Dispose(){Destroy(Fill);Destroy(Stroke);}
        }
        private readonly Dictionary<(int,double,double,double,PathEnding,PathCorner,float),Art> _art=new Dictionary<(int,double,double,double,PathEnding,PathCorner,float),Art>();
        private readonly List<Art> _segments=new List<Art>();
        private readonly List<Art> _dynamic=new List<Art>();
        private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();
        private readonly Material _solid,_radial;
        private int _used,_segmentUsed,_dynamicUsed;
        private NativeWorldSurface _surface;


        internal NativeWorldCreatures()
        {
            _solid=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Native creature anatomy",hideFlags=HideFlags.DontSave};
            _radial=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldCreatureGlow")){name="Native creature glows",hideFlags=HideFlags.DontSave};
        }
        internal void BeginFrame(){_hostileUsed=0;_used=0;_segmentUsed=0;_dynamicUsed=0;_labelUsed=0;}
        private Art Dynamic(BezierContour contour,double width=0,bool fill=true)
        {
            if(_dynamicUsed==_dynamic.Count)_dynamic.Add(new Art());var art=_dynamic[_dynamicUsed++];art.Build(contour,(float)width,_surface.NativeStrokeEnding,_surface.NativeStrokeCorner,_surface.NativeStrokeMiter,fill);return art;
        }
        private static Vector2 P(double x,double y)=>new Vector2((float)x,(float)y);
        private static Matrix4x4 T(double x=0,double y=0,double angle=0,double sx=1,double sy=1)=>Matrix4x4.TRS(new Vector3((float)x,(float)y,0),Quaternion.Euler(0,0,(float)angle*Mathf.Rad2Deg),new Vector3((float)sx,(float)sy,1));
        private static Color C(int r,int g,int b,double a=1)=>new Color(r/255f,g/255f,b/255f,(float)a);
        private static Color A(Color c,double a){c.a=(float)a;return c;}
        private MaterialPropertyBlock Properties(Color c)
        {
            if(_used==_properties.Count)_properties.Add(new MaterialPropertyBlock());var p=_properties[_used++];p.Clear();p.SetVector("_NativeColor",new Vector4(c.r,c.g,c.b,c.a));return p;
        }
        private void Paint(Art art,Matrix4x4 transform,Color? fill,Color? stroke,float? opacity=null,bool additive=false,bool forceOver=false,Color? shadow=null,float shadowBlur=0)
        {
            if(fill.HasValue)_surface.DrawNativeMesh(art.Fill,_solid,Properties(fill.Value),transform,inheritBlend:!forceOver,inheritShadow:!shadow.HasValue,opacity:opacity,additive:additive,shadow:shadow,shadowBlur:shadowBlur,inheritShadowOffsets:shadow.HasValue);
            if(stroke.HasValue)_surface.DrawNativeMesh(art.Stroke,_solid,Properties(stroke.Value),transform,inheritBlend:!forceOver,inheritShadow:!shadow.HasValue,opacity:opacity,additive:additive,shadow:shadow,shadowBlur:shadowBlur,inheritShadowOffsets:shadow.HasValue);

        }
        private Art Get(int kind,double a,double b,double width,Func<BezierContour> create,bool fill=true)
        {
            var key=(kind,a,b,width,_surface.NativeStrokeEnding,_surface.NativeStrokeCorner,_surface.NativeStrokeMiter);
            if(!_art.TryGetValue(key,out var art)){art=new Art();art.Build(create(),(float)width,key.Item5,key.Item6,key.Item7,fill);_art.Add(key,art);}return art;
        }
        private static BezierContour Quadratic(Vector2 start,bool closed,params Vector2[] controlsAndEnds)
        {
            int count=controlsAndEnds.Length/2;var segments=new BezierPathSegment[count+1];var previous=start;
            for(int i=0;i<count;i++){var control=controlsAndEnds[i*2];var end=controlsAndEnds[i*2+1];segments[i]=new BezierPathSegment {P0=previous,P1=previous+(control-previous)*(2f/3),P2=end+(control-end)*(2f/3)};previous=end;}
            segments[count]=new BezierPathSegment {P0=previous,P1=previous,P2=previous};
            if(closed){if((previous-start).sqrMagnitude<.00000001f)Array.Resize(ref segments,count);else{segments[count].P1=Vector2.Lerp(previous,start,1f/3);segments[count].P2=Vector2.Lerp(previous,start,2f/3);}}
            return new BezierContour {Segments=segments,Closed=closed};
        }
        private static BezierContour Poly(params Vector2[] points)
        {
            var segments=new BezierPathSegment[points.Length];for(int i=0;i<points.Length;i++){var p=points[i];var q=points[(i+1)%points.Length];segments[i]=new BezierPathSegment {P0=p,P1=Vector2.Lerp(p,q,1f/3),P2=Vector2.Lerp(p,q,2f/3)};}return new BezierContour {Segments=segments,Closed=true};
        }
        private static BezierContour Oval(double rx,double ry,double sweep=Math.PI*2)
        {
            float delta=Math.Min((float)sweep,Mathf.PI*2);int count=Mathf.Max(1,Mathf.CeilToInt(delta/(Mathf.PI*.25f)));float step=delta/count;var segments=new BezierPathSegment[count+1];
            Vector2 At(float angle)=>P(rx*Mathf.Cos(angle),ry*Mathf.Sin(angle));Vector2 Tangent(float angle)=>P(-rx*Mathf.Sin(angle),ry*Mathf.Cos(angle));
            for(int i=0;i<count;i++){float a=i*step,b=a+step,k=4f/3*Mathf.Tan(step*.25f);segments[i]=new BezierPathSegment {P0=At(a),P1=At(a)+Tangent(a)*k,P2=At(b)-Tangent(b)*k};}var end=At(delta);segments[count]=new BezierPathSegment {P0=end,P1=end,P2=end};return new BezierContour {Segments=segments,Closed=false};
        }
        private void Ellipse(Matrix4x4 m,double rx,double ry,Color? fill,Color? stroke=null,double width=0)
        {var art=Get(0,rx,ry,width,()=>Oval(rx,ry));Paint(art,m,fill,stroke);}
        private void Glow(Matrix4x4 m,double radius,Color color,double alpha,bool core=false)
        {
            var art=Get(0,radius,radius,0,()=>Oval(radius,radius));var p=Properties(color);p.SetFloat("_Radius",(float)radius);p.SetFloat("_GlowAlpha",(float)alpha);p.SetFloat("_Core",core?1:0);
            _surface.DrawNativeMesh(art.Fill,_radial,p,m,inheritShadow:true,additive:true);
        }
        private void Body(Matrix4x4 m,double r,Color color,double rotation=0,double scaleY=1)
        {
            var art=Get(1,r,0,Math.Max(1,r*.055),()=>Quadratic(P(-r*.72,0),true,P(-r*.52,-r*.62),P(0,-r*.58),P(r*.58,-r*.52),P(r*.70,0),P(r*.52,r*.56),P(0,r*.62),P(-r*.56,r*.55),P(-r*.72,0)));
            Paint(art,m*T(angle:rotation,sy:scaleY),C(3,7,13,.98),color);
        }
        private void Joint(Matrix4x4 m,double x,double y,double r,Color color)=>Ellipse(m*T(x,y),r,r,C(4,8,14,.98),color,Math.Max(.7,r*.18));
        private void Eye(Matrix4x4 m,double x,double y,double r,Color color,double look=0)
        {var local=m*T(x,y,look);Ellipse(local,r,r*.62,C(245,252,255,.94),color,Math.Max(.7,r*.16));Ellipse(local*T(r*.18),r*.20,r*.42,color);}
        private void Core(Matrix4x4 m,double x,double y,double r,Color color)
        {var local=m*T(x,y);Glow(local,r*2.2,color,.95,true);Ellipse(local,r,r,C(7,17,27),color,Math.Max(.7,r*.15));}
        private void Segment(Matrix4x4 m,double x1,double y1,double x2,double y2,double w,Color color,double alpha)
        {
            double dx=x2-x1,dy=y2-y1,len=Math.Sqrt(dx*dx+dy*dy);if(_segmentUsed==_segments.Count)_segments.Add(new Art());var art=_segments[_segmentUsed++];
            var ending=_surface.NativeStrokeEnding;var corner=_surface.NativeStrokeCorner;float miter=_surface.NativeStrokeMiter;
            if(art.Stroke==null||art.Length!=len||art.Width!=w||art.Ending!=ending||art.Corner!=corner||art.Miter!=miter){art.Build(Quadratic(P(0,-w*.34),true,P(len*.48,-w*.48),P(len,-w*.10),P(len*.62,w*.42),P(0,w*.30)),(float)Math.Max(.7,w*.09),ending,corner,miter,fill:false);art.Length=len;art.Width=w;art.Ending=ending;art.Corner=corner;art.Miter=miter;}
            // Every fill control X is proportional to length. Share a canonical
            // filled mesh and animate its X transform instead of retriangulating
            // each limb each frame. Only shrink from a power-of-two envelope, so
            // the original tessellation's absolute curve-error bound cannot grow.
            // Keep the stroke at the actual length: scaling it would distort width.
            double envelope=Math.Pow(2,Math.Ceiling(Math.Log(Math.Max(1,len),2)));
            var fill=Get(900,envelope,w,0,()=>Quadratic(P(0,-w*.34),true,P(envelope*.48,-w*.48),P(envelope,-w*.10),P(envelope*.62,w*.42),P(0,w*.30)));
            var root=m*T(x1,y1,Math.Atan2(dy,dx));
            _surface.DrawNativeMesh(fill.Fill,_solid,Properties(C(4,8,15,.98)),root*T(sx:len/envelope),inheritBlend:true,inheritShadow:true,opacity:(float)alpha);
            _surface.DrawNativeMesh(art.Stroke,_solid,Properties(color),root,inheritBlend:true,inheritShadow:true,opacity:(float)alpha);
        }
        private void Legs(Matrix4x4 m,double r,double[] xs,double bodySideY,double len,double phase,Color color,double thick)
        {
            for(int i=0;i<xs.Length;i++)for(int side=-1;side<=1;side+=2){double x=xs[i]*r,y=side*bodySideY*r,l=len*r,p=phase+i*1.05+(side>0?0:Math.PI),stride=Math.Sin(p)*l*.30,lift=Math.Max(0,Math.Cos(p))*l*.12,kx=x+stride*.46+l*.05,ky=y+side*(l*.34-lift),ax=x+stride,ay=y+side*(l*.76-lift*.45);
                Segment(m,x,y,kx,ky,thick*5.8,color,.94);Joint(m,kx,ky,thick*3.5,color);Segment(m,kx,ky,ax,ay,thick*4,color,.90);Joint(m,ax,ay,thick*1.9,color);Segment(m,ax,ay,ax+l*.15,ay+side*l*.035,thick*1.45,color,.92);}
        }
        private void Tail(Matrix4x4 m,double sx,double sy,double len,Color color,double t,int count=7,double amp=.18)
        {
            double px=sx,py=sy;for(int i=0;i<count;i++){double q=(i+1)/(double)count,x=sx-len*q,y=sy+Math.Sin(t*3-q*4.2)*len*amp*(.30+.70*q),w=len*(.14-.009*i);Segment(m,px,py,x,y,w,color,.82-.025*i);if(i<count-1)Joint(m,x,y,Math.Max(.9,w*.22),color);px=x;py=y;}
        }
        private void Wing(Matrix4x4 m,double x,double y,double len,double w,double angle,Color color,double flap)
        {
            var local=m*T(x,y,angle+Math.Sin(flap)*.14);var art=Get(2,len,w,Math.Max(.8,w*.07),()=>Quadratic(Vector2.zero,true,P(len*.35,-w*.85),P(len,-w*.18),P(len*.68,w*.55),P(len*.12,w*.36)));Paint(art,local,C(4,9,17,.96),color);
            for(int i=1;i<4;i++){int index=i;var vein=Get(10+i,len,w,Math.Max(.5,w*.025),()=>{var start=P(len*.08,0);var end=P(len*(.24+index*.18),-w*(.42-index*.08));return Quadratic(start,false,(start+end)*.5f,end);},false);Paint(vein,local,null,C(235,249,255,.25));}
        }
        private void Frame(Matrix4x4 m,double r,Color color,double t)
        {var art=Get(3,r,0,Math.Max(.8,r*.025),()=>Oval(r*1.16,r*1.16,4.9),false);Paint(art,m*T(angle:t*.2),null,A(color,.30));}
        internal void Draw(NativeWorldSurface surface,double radius,Color color,double time,string variant,bool boss,Matrix4x4? parent=null)
        {
            _surface=surface;var root=parent??Matrix4x4.identity;Glow(root,radius*(boss?2.55:2.15),color,boss?.085:.065);
            if(boss)Boss(root,radius,color,time,variant);else Creature(root,radius,color,time,variant);

        }
        private static void Destroy(UnityEngine.Object value){if(value!=null)UnityEngine.Object.Destroy(value);}
        public void Dispose(){DisposeSimpleHostiles();foreach(var art in _art.Values)art.Dispose();foreach(var art in _segments)art.Dispose();foreach(var art in _dynamic)art.Dispose();_art.Clear();_segments.Clear();_dynamic.Clear();foreach(var label in _bossLabels)label.Dispose();_bossLabels.Clear();Destroy(_solid);Destroy(_radial);}
    }
}
