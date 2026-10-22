using System;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    // Pickable presentation only. Rune effects, collection, chests and rewards
    // remain owned by the existing simulation and catalogs.
    internal sealed class NativeWorldPickableViews : IDisposable
    {
        private sealed class Art : IDisposable
        {
            internal Mesh Fill,Stroke;
            internal BezierContour[] Paths;
            internal void Build(BezierContour[] paths,float width,SourceCanvasSurface surface,bool fill)
            {Paths=paths;if(fill)Fill=Tessellate(paths[0],0,Fill,contours:paths);if(width>0)Stroke=Tessellate(paths[0],width,Stroke,cap:surface.NativeStrokeEnding,corners:surface.NativeStrokeCorner,miter:surface.NativeStrokeMiter,contours:paths);}
            public void Dispose(){Destroy(Fill);Destroy(Stroke);}
        }
        private readonly Dictionary<(int,float,float,float,PathEnding,PathCorner,float),Art> _art=new Dictionary<(int,float,float,float,PathEnding,PathCorner,float),Art>();
        private readonly List<Art> _dynamic=new List<Art>();
        private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();
        private readonly List<NativeWorldLabel> _labels=new List<NativeWorldLabel>();
        private int _dynamicUsed,_propertyUsed,_labelUsed;
        private readonly Material _material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Native world pickables",hideFlags=HideFlags.DontSave};
        private SourceCanvasSurface _surface;private Art _last;private Matrix4x4 _lastMatrix;
        internal void BeginFrame(){_dynamicUsed=0;_propertyUsed=0;_labelUsed=0;}
        private static Vector2 P(double x,double y)=>new Vector2((float)x,(float)y);
        private static Color C(int r,int g,int b,double a=1)=>new Color(r/255f,g/255f,b/255f,(float)a);
        private static Matrix4x4 T(double x=0,double y=0,double angle=0,double scale=1)=>Matrix4x4.TRS(new Vector3((float)x,(float)y,0),Quaternion.Euler(0,0,(float)angle*Mathf.Rad2Deg),new Vector3((float)scale,(float)scale,1));
        private Art Get(int kind,float a,float b,float width,Func<BezierContour[]> create,bool fill=true)
        {var key=(kind,a,b,width,_surface.NativeStrokeEnding,_surface.NativeStrokeCorner,_surface.NativeStrokeMiter);if(!_art.TryGetValue(key,out var art)){art=new Art();art.Build(create(),width,_surface,fill);_art.Add(key,art);}return art;}
        private Art Oval(float radius,float width=0)=>Get(0,radius,radius,width,()=>new[]{EllipseContour(Vector2.zero,radius,radius)});
        private Art Ellipse(float rx,float ry)=>Get(0,rx,ry,0,()=>new[]{EllipseContour(Vector2.zero,rx,ry)});
        private Art Dynamic(BezierContour path,float width=0,bool fill=true)
        {if(_dynamicUsed==_dynamic.Count)_dynamic.Add(new Art());var art=_dynamic[_dynamicUsed++];art.Build(new[]{path},width,_surface,fill);return art;}
        private MaterialPropertyBlock Properties(Color c)
        {if(_propertyUsed==_properties.Count)_properties.Add(new MaterialPropertyBlock());var p=_properties[_propertyUsed++];p.SetVector("_NativeColor",new Vector4(c.r,c.g,c.b,c.a));return p;}
        private void Paint(Art art,Matrix4x4 root,Color? fill,Color? stroke,Color? shadow,float blur,float? opacity=null)
        {if(fill.HasValue)_surface.DrawNativeMesh(art.Fill,_material,Properties(fill.Value),root,shadow,blur,opacity:opacity,inheritShadow:!shadow.HasValue,inheritShadowOffsets:shadow.HasValue);if(stroke.HasValue)_surface.DrawNativeMesh(art.Stroke,_material,Properties(stroke.Value),root,shadow,blur,opacity:opacity,inheritShadow:!shadow.HasValue,inheritShadowOffsets:shadow.HasValue);_last=art;_lastMatrix=root;}
        private void Finish(){if(_last!=null)_surface.ContinueNativeContours(_last.Paths,_lastMatrix);}
        private void Ground(Matrix4x4 root,float rx,float ry)
        {Paint(Ellipse(rx,ry),root*T(y:5),C(0,0,0,.34),null,null,0,.42f);Paint(Ellipse(rx*.72f,ry*.68f),root*T(y:5),C(0,0,0,.34),null,null,0,.16f);}
        internal void DrawChest(SourceCanvasSurface surface,double x,double y,double t,bool stella,string label)
        {
            _surface=surface;var root=T(x,y,scale:1+Math.Sin(t*(stella?2.6:3.2))*(stella?.08:.05));var shadow=stella?C(255,184,77):C(169,138,255);float blur=stella?13:10;Ground(root,stella?24:18,stella?7:6);
            var body=Get(stella?2:1,0,0,stella?2:1.4f,()=>new[]{stella?PolylineContour(true,P(0,-23),P(18,-9),P(20,10),P(0,23),P(-20,10),P(-18,-9)):PolylineContour(true,P(-17,7),P(-13,-8),P(0,-14),P(13,-8),P(17,7),P(0,14))});Paint(body,root,stella?C(24,12,2,.96):C(4,10,19,.96),stella?shadow:C(194,160,255,.86),shadow,blur);
            if(stella){for(int i=0;i<4;i++){int index=i;var line=Get(10+i,0,0,1.4f,()=>new[]{PolylineContour(false,P(-17+index*4,-8),P(17-index*4,8))},false);Paint(line,root*T(angle:t*.25),null,C(255,240,191),shadow,blur);}Paint(Oval(5),root,C(255,248,223),null,shadow,blur);
                if(_labelUsed==_labels.Count)_labels.Add(new NativeWorldLabel());_labels[_labelUsed++].Draw(surface,label,P(0,37),C(255,210,126),shadow,true,root,blur);}
            else{var folds=Get(3,0,0,1.4f,()=>new[]{PolylineContour(false,P(-13,-8),P(0,-2),P(13,-8)),PolylineContour(false,P(0,-2),P(0,10))},false);Paint(folds,root,null,C(111,224,255,.55),shadow,blur);Paint(Oval(3),root*T(y:-2),Color.white,null,shadow,blur);}
            Finish();
        }
        internal void DrawRune(SourceCanvasSurface surface,double x,double y,string type,Color color,double t)
        {
            _surface=surface;_last=null;var root=T(x,y,scale:1+Math.Sin(t*4)*.06);var dark=C(5,12,22,.96);
            void Draw(Art art,Color? fill,Color? stroke,Matrix4x4? local=null)=>Paint(art,root*(local??Matrix4x4.identity),fill,stroke,color,8);
            switch(type)
            {
                case "phase":
                    Draw(Get(20,0,0,1.5f,()=>new[]{PolylineContour(true,P(0,-16),P(8,-8),P(16,0),P(8,8),P(0,16),P(-8,8),P(-16,0),P(-8,-8))}),dark,color);Draw(Get(21,0,0,1.5f,()=>new[]{PolylineContour(false,P(-7,-7),P(7,7)),PolylineContour(false,P(7,-7),P(-7,7))},false),null,C(234,255,255));break;
                case "harvest":
                    Draw(Get(22,0,0,1.5f,()=>new[]{QuadraticContour(P(0,-16),false,P(15,-8),P(10,9),P(0,18),P(-10,9),P(-15,-8),P(0,-16))}),dark,color);
                    for(int i=0;i<3;i++){double a=t*1.3+i*Math.PI*2/3;Draw(Dynamic(PolylineContour(false,Vector2.zero,P(Math.Cos(a)*9,Math.Sin(a)*7)),1.5f,false),null,C(255,255,255,.55));}break;
                case "purge":
                    Draw(Get(23,0,0,1.5f,()=>{var points=new Vector2[10];for(int i=0;i<10;i++){double a=-Math.PI/2+i*Math.PI/5,rr=i%2==0?17:8;points[i]=P(Math.Cos(a)*rr,Math.Sin(a)*rr);}return new[]{PolylineContour(true,points)};}),dark,color);Draw(Oval(4),C(255,255,255,.55),null);break;
                case "resonance":
                    Draw(Oval(15,1.5f),null,color);Draw(Oval(7,1.5f),null,color);Draw(Get(24,0,0,1.5f,()=>new[]{PolylineContour(false,P(4,0),P(13,0))},false),null,Color.white,T(angle:t*2));break;
                case "fortify":
                    Draw(Get(25,0,0,1.5f,()=>new[]{PolylineContour(true,P(0,-17),P(13,-10),P(10,9),P(0,17),P(-10,9),P(-13,-10))}),dark,color);Draw(Get(26,0,0,1.5f,()=>new[]{PolylineContour(false,P(-7,-3),P(0,6),P(8,-7))},false),null,C(230,250,255,.7));break;
                case "hunt":
                    Draw(Get(27,0,0,1.5f,()=>new[]{PolylineContour(false,P(-15,-9),Vector2.zero,P(-15,9)),PolylineContour(false,P(15,-9),Vector2.zero,P(15,9))},false),null,color);float huntR=(float)(6+Math.Sin(t*7)*1.2);Draw(Dynamic(EllipseContour(Vector2.zero,huntR,huntR),1.5f),dark,color);break;
                case "echo":
                    for(int i=0;i<3;i++){float rr=(float)(5+i*5+Math.Sin(t*3+i)*1.2);Draw(Dynamic(EllipseContour(Vector2.zero,rr,rr),1.5f,false),null,C(220,248,255,.3+i*.2));}Draw(Oval(3),Color.white,null);break;
                case "gravity":
                    for(int i=0;i<4;i++){int index=i;Draw(Get(30+i,0,0,1.5f,()=>new[]{EllipseContour(Vector2.zero,6+index*3,6+index*3,0,1.9f)},false),null,C(190,150,255,.75-i*.12),T(angle:t*.7+i*Math.PI/2));}Draw(Oval(2.8f),Color.white,null);break;
                default:
                    Draw(Get(28,0,0,1.5f,()=>new[]{PolylineContour(true,P(0,-15),P(13,0),P(0,15),P(-13,0))}),dark,color);Draw(Oval(3.5f),Color.white,null);break;
            }
            Finish();
        }
        private static void Destroy(UnityEngine.Object value){if(value!=null)UnityEngine.Object.Destroy(value);}
        public void Dispose(){foreach(var art in _art.Values)art.Dispose();foreach(var art in _dynamic)art.Dispose();foreach(var label in _labels)label.Dispose();_art.Clear();_dynamic.Clear();_labels.Clear();Destroy(_material);}
    }
}
