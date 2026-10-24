using System;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // Typed presentation of enemyVisual.ts. Unity Vector Graphics owns curve
    // tessellation. Source identities/palette remain; user-approved organic
    // facial details and stance/recovery articulation refine presentation.
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
        internal void BeginFrame(){_used=0;_segmentUsed=0;_dynamicUsed=0;_labelUsed=0;}
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
        {if(_organicFaces){OrganicEye(m*T(x,y,look),r,color);return;}var local=m*T(x,y,look);Ellipse(local,r,r*.62,C(245,252,255,.94),color,Math.Max(.7,r*.16));Ellipse(local*T(r*.18),r*.20,r*.42,color);}
        private void Core(Matrix4x4 m,double x,double y,double r,Color color)
        {var local=m*T(x,y);Glow(local,r*2.2,color,.95,true);Ellipse(local,r,r,C(7,17,27),color,Math.Max(.7,r*.15));}
        private void Segment(Matrix4x4 m,double x1,double y1,double x2,double y2,double w,Color color,double alpha,double? fixedLength=null)
        {
            double dx=x2-x1,dy=y2-y1,len=fixedLength??Math.Sqrt(dx*dx+dy*dy);Art art;
            var ending=_surface.NativeStrokeEnding;var corner=_surface.NativeStrokeCorner;float miter=_surface.NativeStrokeMiter;
            if(fixedLength.HasValue){
                // Use the solver's actual invariant bone length, not a length
                // reconstructed from its rounded Vector2 endpoints. No scaling
                // of the stroke width and no quantization of animated paths.
                var key=(901,len,w,Math.Max(.7,w*.09),ending,corner,miter);
                if(!_art.TryGetValue(key,out art)){art=new Art();art.Build(Quadratic(P(0,-w*.34),true,P(len*.48,-w*.48),P(len,-w*.10),P(len*.62,w*.42),P(0,w*.30)),(float)key.Item4,ending,corner,miter,fill:false);art.Length=len;art.Width=w;art.Ending=ending;art.Corner=corner;art.Miter=miter;_art.Add(key,art);}
            }else{if(_segmentUsed==_segments.Count)_segments.Add(new Art());art=_segments[_segmentUsed++];}
            if(art.Stroke==null||art.Length!=len||art.Width!=w||art.Ending!=ending||art.Corner!=corner||art.Miter!=miter){art.Build(Quadratic(P(0,-w*.34),true,P(len*.48,-w*.48),P(len,-w*.10),P(len*.62,w*.42),P(0,w*.30)),(float)Math.Max(.7,w*.09),ending,corner,miter,fill:false);art.Length=len;art.Width=w;art.Ending=ending;art.Corner=corner;art.Miter=miter;}
            // Every fill control X is proportional to length. Share a canonical
            // filled mesh and animate its X transform instead of retriangulating
            // each limb each frame. Only shrink from a power-of-two envelope, so
            // the original tessellation's absolute curve-error bound cannot grow.
            // Keep the stroke at the actual length: scaling it would distort width.
            double envelope=Math.Pow(2,Math.Ceiling(Math.Log(Math.Max(1,len),2)));
            var fillKey=(900,envelope,w,0d,ending,corner,miter);
            if(!_art.TryGetValue(fillKey,out var fill)){fill=new Art();fill.Build(Quadratic(P(0,-w*.34),true,P(envelope*.48,-w*.48),P(envelope,-w*.10),P(envelope*.62,w*.42),P(0,w*.30)),0,ending,corner,miter);_art.Add(fillKey,fill);}
            var root=m*T(x1,y1,Math.Atan2(dy,dx));
            _surface.DrawNativeMesh(fill.Fill,_solid,Properties(C(4,8,15,.98)),root*T(sx:len/envelope),inheritBlend:true,inheritShadow:true,opacity:(float)alpha);
            _surface.DrawNativeMesh(art.Stroke,_solid,Properties(color),root,inheritBlend:true,inheritShadow:true,opacity:(float)alpha);
        }
        private void Legs(Matrix4x4 m,double r,double[] xs,double bodySideY,double len,double phase,Color color,double thick)
        {
            double cycle=_gaitActive?_gaitTravel*(.64/(2*Math.Min(.30,len*.60)))*Math.PI*2:phase;
            for(int i=0;i<xs.Length;i++)for(int side=-1;side<=1;side+=2){
                double offset=xs.Length==4?i*Math.PI*.5:(i%2)*Math.PI;
                double p=(_airborneLegs?_poseTime*(_poseVariant=="moth"?8:5):cycle)+offset+(side<0?Math.PI:0),spread=(i-(xs.Length-1)*.5)*.17;
                double wave=0;if(_poseVariant=="leech"){double q=(xs[i]+.62)/1.22;wave=Math.Sin(_poseTime*3.2+q*4.8)*r*.14*(_gaitActive?.2+.8*_gaitBlend:1);}
                if(_poseVariant=="stalker")wave=Math.Sin(_poseTime*4)*r*.06;
                var pose=SolveOrganicLeg(r,xs[i],side,bodySideY,len,spread,p,_gaitActive?_gaitBlend:1,wave,_airborneLegs);
                Segment(m,pose.Hip.x,pose.Hip.y,pose.Knee.x,pose.Knee.y,Math.Max(1.4,r*.24*thick),color,.90,pose.UpperLength);
                Joint(m,pose.Knee.x,pose.Knee.y,Math.Max(.6,r*.038),color);
                Segment(m,pose.Knee.x,pose.Knee.y,pose.Foot.x,pose.Foot.y,Math.Max(1.1,r*.15*thick),color,.86,pose.LowerLength);
                var toe=Get(230,r,side,Math.Max(.55,r*.024),()=>Quadratic(Vector2.zero,false,P(r*.08,side*r*.025),P(r*.13,-side*r*.025)),false);
                Paint(toe,m*T(pose.Foot.x,pose.Foot.y),null,A(color,pose.Contact?.9:.65));
            }
        }
        private void Tail(Matrix4x4 m,double sx,double sy,double len,Color color,double t,int count=7,double amp=.18)
        {
            if(_gaitActive)amp*=.25+.75*_gaitBlend;double px=sx,py=sy;for(int i=0;i<count;i++){double q=(i+1)/(double)count,x=sx-len*q,y=sy+Math.Sin(t*3-q*4.2)*len*amp*(.30+.70*q),w=len*(.14-.009*i);Segment(m,px,py,x,y,w,color,.82-.025*i);if(i<count-1)Joint(m,x,y,Math.Max(.9,w*.22),color);px=x;py=y;}
        }
        private void Wing(Matrix4x4 m,double x,double y,double len,double w,double angle,Color color,double flap)
        {
            double side=y<0?-1:1;var local=m*T(x,y,angle+side*Math.Sin(flap)*.18,sy:-side*(Math.Cos(angle)<0?-1:1)*(.94+.06*Math.Cos(flap)));var art=Get(2,len,w,Math.Max(.8,w*.07),()=>Quadratic(Vector2.zero,true,P(len*.35,-w*.85),P(len,-w*.18),P(len*.68,w*.55),P(len*.12,w*.36)));Paint(art,local,C(4,9,17,.96),color);
            for(int i=1;i<4;i++){int index=i;var vein=Get(10+i,len,w,Math.Max(.5,w*.025),()=>{var start=P(len*.08,0);var end=P(len*(.24+index*.18),-w*(.42-index*.08));return Quadratic(start,false,(start+end)*.5f,end);},false);Paint(vein,local,null,C(235,249,255,.25));}
        }
        private void Frame(Matrix4x4 m,double r,Color color,double t)
        {var art=Get(3,r,0,Math.Max(.8,r*.025),()=>Oval(r*1.16,r*1.16,4.9),false);Paint(art,m*T(angle:t*.2),null,A(color,.30));}
        internal void Draw(NativeWorldSurface surface,double radius,Color color,double time,string variant,bool boss,Matrix4x4? parent=null)
        {
            _surface=surface;var root=parent??Matrix4x4.identity;Glow(root,radius*(boss?2.55:2.15),color,boss?.085:.065);
            _poseTime=time;_poseVariant=variant;_airborneLegs=!boss&&(variant=="wisp"||variant=="moth");_organicFaces=true;
            try{if(boss)Boss(root,radius,color,time,variant);else Creature(root,radius,color,time,variant);}
            finally{_gaitActive=false;_airborneLegs=false;_organicFaces=false;}

        }
        private static void Destroy(UnityEngine.Object value){if(value!=null)UnityEngine.Object.Destroy(value);}
        public void Dispose(){foreach(var art in _art.Values)art.Dispose();foreach(var art in _segments)art.Dispose();foreach(var art in _dynamic)art.Dispose();_art.Clear();_segments.Clear();_dynamic.Clear();foreach(var label in _bossLabels)label.Dispose();_bossLabels.Clear();Destroy(_solid);Destroy(_radial);}
    }
}
