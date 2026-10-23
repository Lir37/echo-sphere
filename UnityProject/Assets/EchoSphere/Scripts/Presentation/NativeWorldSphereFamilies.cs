using System;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldSphereFamilies : IDisposable
    {
        internal struct State
        {
            internal double X,Y,Rotation,Time,Scale;
            internal bool Disabled,Resonance;
            internal string Family;
            internal Color PrismElement;
            internal bool HasPrismElement;
        }
        private sealed class Art : IDisposable
        {
            internal Mesh Fill,Stroke;internal BezierContour[] Paths;
            internal void Build(BezierContour[] paths,float width,bool fill,PathEnding cap,PathCorner corner,float miter,float[] dash=null,float offset=0)
            {Paths=paths;if(fill)Fill=Tessellate(paths[0],0,Fill,contours:paths);if(width>0)Stroke=Tessellate(paths[0],width,Stroke,cap:cap,corners:corner,miter:miter,contours:paths,dash:dash,dashOffset:offset);}
            public void Dispose(){Destroy(Fill);Destroy(Stroke);}
        }
        private readonly Dictionary<(int,double,double,double,PathEnding,PathCorner,float),Art> _art=new Dictionary<(int,double,double,double,PathEnding,PathCorner,float),Art>();
        private readonly Dictionary<Color,Matrix4x4> _glowSpaces=new Dictionary<Color,Matrix4x4>();
        private readonly List<Art> _dynamic=new List<Art>();private int _dynamicUsed,_used;
        private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();
        private readonly Material _solid=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Native sphere family art",hideFlags=HideFlags.DontSave};
        private readonly Material _glow=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldFamilyGlow")){name="Native sphere family glow",hideFlags=HideFlags.DontSave};
        private SourceCanvasSurface _surface;private BezierContour[] _last;private Matrix4x4 _lastMatrix;
        internal void BeginFrame(){_used=0;_dynamicUsed=0;}
        private static Vector2 P(double x,double y)=>new Vector2((float)x,(float)y);
        private static Matrix4x4 T(double x=0,double y=0,double angle=0,double scale=1)=>Matrix4x4.TRS(new Vector3((float)x,(float)y,0),Quaternion.Euler(0,0,(float)angle*Mathf.Rad2Deg),new Vector3((float)scale,(float)scale,1));
        private static Color C(int r,int g,int b,double a=1)=>new Color(r/255f,g/255f,b/255f,(float)a);
        private static Color A(Color c,double alpha){c.a=(float)alpha;return c;}
        private MaterialPropertyBlock Properties(Color c){if(_used==_properties.Count)_properties.Add(new MaterialPropertyBlock());var p=_properties[_used++];p.Clear();p.SetVector("_NativeColor",new Vector4(c.r,c.g,c.b,c.a));return p;}
        private Art Get(int kind,double r,double extra,double width,Func<BezierContour[]> create,bool fill=false,PathEnding? cap=null,PathCorner? corner=null)
        {var key=(kind,r,extra,width,cap??_surface.NativeStrokeEnding,corner??_surface.NativeStrokeCorner,_surface.NativeStrokeMiter);if(!_art.TryGetValue(key,out var art)){art=new Art();art.Build(create(),(float)width,fill,key.Item5,key.Item6,key.Item7);_art.Add(key,art);}return art;}
        private Art Dynamic(BezierContour[] paths,double width=0,bool fill=false,PathEnding? cap=null,float[] dash=null)
        {if(_dynamicUsed==_dynamic.Count)_dynamic.Add(new Art());var art=_dynamic[_dynamicUsed++];art.Build(paths,(float)width,fill,cap??_surface.NativeStrokeEnding,_surface.NativeStrokeCorner,_surface.NativeStrokeMiter,dash,_surface.NativeDashOffset);return art;}
        private Art Circle(double radius,double width=0)=>Get(0,radius,0,width,()=>new[]{EllipseContour(Vector2.zero,(float)radius,(float)radius)},true);
        private void Paint(Art art,Matrix4x4 matrix,Color? fill,Color? stroke,double? opacity=null,bool additive=false,bool inheritBlend=false)
        {if(fill.HasValue)_surface.DrawNativeMesh(art.Fill,_solid,Properties(fill.Value),matrix,opacity:opacity.HasValue?(float?)opacity.Value:null,additive:additive,inheritBlend:inheritBlend,inheritShadow:true);if(stroke.HasValue)_surface.DrawNativeMesh(art.Stroke,_solid,Properties(stroke.Value),matrix,opacity:opacity.HasValue?(float?)opacity.Value:null,additive:additive,inheritBlend:inheritBlend,inheritShadow:true);_last=art.Paths;_lastMatrix=matrix;}
        private void Finish(){if(_last!=null)_surface.ContinueNativeContours(_last,_lastMatrix);}
        private static BezierContour Line(double x1,double y1,double x2,double y2)=>PolylineContour(false,P(x1,y1),P(x2,y2));
        private static (double x,double y,double depth) Point(double r,double angle,double flatten=.34,double radiusScale=1.16)
        {double sine=Math.Sin(angle);return(Math.Cos(angle)*r*radiusScale,sine*r*flatten*radiusScale,.64+.36*((sine+1)*.5));}
        internal void DrawGlow(SourceCanvasSurface surface,double radius,Color color,double alpha,Matrix4x4? parent=null,bool additive=false)
        {
            _surface=surface;var root=parent??Matrix4x4.identity;if(!_glowSpaces.TryGetValue(color,out var inverse)){inverse=(surface.NativePaintMatrix*root).inverse;_glowSpaces.Add(color,inverse);}
            var matrix=root*T(scale:radius);var art=Circle(1);var p=Properties(color);p.SetMatrix("_GradientMap",inverse*surface.NativePaintMatrix*matrix);
            surface.DrawNativeMesh(art.Fill,_glow,p,matrix,opacity:(float)alpha,inheritShadow:true,inheritBlend:!additive,additive:additive);_last=art.Paths;_lastMatrix=matrix;Finish();
        }
        internal void DrawDisabled(SourceCanvasSurface surface,double radius,Color color,bool disabled,Matrix4x4? parent=null,double inheritedOpacity=1)
        {if(!disabled)return;_surface=surface;var art=Get(1,radius,0,1.7,()=>new[]{EllipseContour(Vector2.zero,(float)(radius*1.18),(float)(radius*1.18),Mathf.PI*.20f,Mathf.PI*.80f)});Paint(art,parent??Matrix4x4.identity,null,color,.42,inheritBlend:true);Finish();}
        private static BezierContour Trace(double rx,double ry,double start,double end,double wobble=0)
        {var points=new Vector2[43];for(int i=0;i<=42;i++){double a=start+(end-start)*i/42,n=wobble!=0?Math.Sin(a*7+.35)*wobble:0;points[i]=P(Math.Cos(a)*(rx+n),Math.Sin(a)*(ry+n*.34));}return PolylineContour(false,points);}
        internal void DrawOrbit(SourceCanvasSurface surface,double radius,Color color,double rotation,double opacity,string mode,double pulse,Matrix4x4? parent=null,bool lightningAdditive=false)
        {
            _surface=surface;double rx=radius*1.16,ry=radius*.34;var root=(parent??Matrix4x4.identity)*T(angle:rotation);bool lightning=mode=="lightning";
            void Stroke(Art art,double alpha)=>Paint(art,root,null,color,alpha,additive:!lightning||lightningAdditive,inheritBlend:lightning&&!lightningAdditive);
            if(lightning){for(int ring=0;ring<2;ring++){double x=ring==0?rx:radius*.98,y=ring==0?ry:radius*.28,q=pulse+ring*.5;var points=new Vector2[21];for(int i=0;i<=20;i++){double a=i/20.0*Math.PI*2,j=Math.Sin(i*3.7+q*5.2)*x*.025;points[i]=P(Math.Cos(a)*(x+j),Math.Sin(a)*(y+j*.34));}Stroke(Dynamic(new[]{PolylineContour(false,points)},Math.Max(.8,x*.028)),opacity*(ring==0?1:.36));}Finish();return;}
            if(mode=="void"){
                for(int i=0;i<7;i++){int index=i;double a=i*Math.PI*2/7+.08;Stroke(Get(10+i,radius,0,Math.Max(.8,radius*.030),()=>new[]{Trace(rx,ry,a,a+(index%2==0?.58:.34),.70)},cap:PathEnding.Round),opacity*(i%3==0?.94:.72));}
                Stroke(Get(17,radius,0,Math.Max(.55,radius*.018),()=>new[]{Trace(radius*1.03,radius*.295,Math.PI*.16,Math.PI*1.84,.35)},cap:PathEnding.Round),opacity*.42);
                var diamond=Get(18,radius,0,0,()=>new[]{PolylineContour(true,P(radius*.10,0),P(0,radius*.055),P(-radius*.10,0),P(0,-radius*.055))},true);for(int i=0;i<3;i++){double a=-rotation*.72+i*Math.PI*2/3;Paint(diamond,root*T(Math.Cos(a)*rx*.99,Math.Sin(a)*ry*.99),color,null,opacity*.72,true);}Finish();return;
            }
            Stroke(Get(20,radius,0,Math.Max(.9,radius*.040),()=>new[]{Trace(rx,ry,0,Math.PI*2)},cap:PathEnding.Round),opacity);
            Stroke(Get(21,radius,0,Math.Max(.55,radius*.022),()=>new[]{Trace(radius,radius*.285,Math.PI*.10,Math.PI*1.92)},cap:PathEnding.Round),opacity*.44);
            switch(mode){
                case "sniper":for(int i=0;i<5;i++){double a=-.56+i*.28,x=Math.Cos(a)*rx,y=Math.Sin(a)*ry,nx=Math.Cos(a),ny=Math.Sin(a);Stroke(Get(30+i,radius,0,Math.Max(.6,radius*.020),()=>new[]{Line(x-nx*radius*.045,y-ny*radius*.045,x+nx*radius*.075,y+ny*radius*.075)},cap:PathEnding.Round),opacity*.68);}break;
                case "shotgun":for(int i=0;i<3;i++){double a=-.44+i*.44;var p=P(Math.Cos(a)*rx,Math.Sin(a)*ry);Stroke(Get(40+i,radius,0,Math.Max(.7,radius*.024),()=>new[]{EllipseContour(p,(float)(radius*.065),(float)(radius*.065))},cap:PathEnding.Round),opacity*.72);}break;
                case "aura":for(int i=0;i<6;i++){double a=i*Math.PI/3+pulse*.10;Stroke(Dynamic(new[]{Trace(rx*.96,ry*.96,a-.26,a+.26)},Math.Max(.6,radius*.018),cap:PathEnding.Round),opacity*.64);}break;
                case "prism":Stroke(Dynamic(new[]{Trace(rx*1.01,ry*1.02,.08,Math.PI*1.95)},Math.Max(.75,radius*.024),cap:PathEnding.Round,dash:new[]{(float)(radius*.15),(float)(radius*.07)}),opacity*.74);break;
                case "gravity":Stroke(Get(50,radius,0,Math.Max(.7,radius*.026),()=>new[]{Trace(rx*.88,ry*.78,.25,Math.PI*1.78)},cap:PathEnding.Round),opacity*.56);Stroke(Get(51,radius,0,Math.Max(.7,radius*.026),()=>new[]{Trace(rx*1.05,ry*.86,Math.PI*1.08,Math.PI*2.64)},cap:PathEnding.Round),opacity*.56);break;
                case "orbital":Stroke(Get(52,radius,0,Math.Max(.65,radius*.022),()=>new[]{Trace(rx*.92,ry*.84,Math.PI*.12,Math.PI*1.86)},cap:PathEnding.Round),opacity*.52);break;
            }
            Finish();
        }
        private static void Destroy(UnityEngine.Object value){if(value!=null)UnityEngine.Object.Destroy(value);}
        public void Dispose(){foreach(var art in _art.Values)art.Dispose();foreach(var art in _dynamic)art.Dispose();_art.Clear();_dynamic.Clear();_glowSpaces.Clear();Destroy(_solid);Destroy(_glow);}
    }
}
