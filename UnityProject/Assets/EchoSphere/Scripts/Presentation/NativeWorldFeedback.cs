using System;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    // Mesh/font owners for the formerly inline frame feedback. No simulation data is changed.
    internal sealed class NativeWorldFeedback : IDisposable
    {
        private sealed class Art {internal Mesh Mesh;internal readonly BezierContour[] Paths=new BezierContour[1];}
        private readonly Material _solid=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Native frame feedback",hideFlags=HideFlags.DontSave};
        private readonly Mesh _background=new Mesh {name="Native frame background"};
        private readonly Material _backgroundMaterial=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldFrameBackground")){name="Native frame background",hideFlags=HideFlags.DontSave};
        private readonly MaterialPropertyBlock _backgroundProperties=new MaterialPropertyBlock();private double _width=-1,_height=-1;
        private readonly List<Art> _art=new List<Art>();private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();private readonly List<NativeWorldLabel> _labels=new List<NativeWorldLabel>();
        private int _used,_labelUsed;private NativeWorldSurface _surface;
        internal void BeginFrame(NativeWorldSurface surface){_used=0;_labelUsed=0;_surface=surface;}
        private static Vector2 P(double x,double y)=>new Vector2((float)x,(float)y);
        private static Color C(int r,int g,int b,double a=1)=>new Color(r/255f,g/255f,b/255f,(float)a);
        private static Matrix4x4 T(double x=0,double y=0,double angle=0)=>Matrix4x4.TRS(new Vector3((float)x,(float)y,0),Quaternion.Euler(0,0,(float)(angle*180/Math.PI)),Vector3.one);
        private void Paint(BezierContour contour,double width,Color color,Matrix4x4 root,bool continuePath=true)
        {
            if(_used==_art.Count){_art.Add(new Art());_properties.Add(new MaterialPropertyBlock());}var art=_art[_used];var props=_properties[_used++];art.Paths[0]=contour;
            art.Mesh=Tessellate(contour,(float)width,art.Mesh,cap:_surface.NativeStrokeEnding,corners:_surface.NativeStrokeCorner,miter:_surface.NativeStrokeMiter,dash:_surface.NativeStrokeDash,dashOffset:_surface.NativeDashOffset);
            props.SetVector("_NativeColor",new Vector4(color.r,color.g,color.b,color.a));_surface.DrawNativeMesh(art.Mesh,_solid,props,root,inheritBlend:true,inheritShadow:true);

        }
        internal void Background(double w,double h,Color center,Color middle)
        {
            if(w!=_width||h!=_height){_width=w;_height=h;_background.Clear();_background.vertices=new[]{new Vector3(0,0,0),new Vector3((float)w,0,0),new Vector3((float)w,(float)h,0),new Vector3(0,(float)h,0)};_background.triangles=new[]{0,1,2,0,2,3};_background.RecalculateBounds();}
            _backgroundProperties.SetVector("_FrameSize",new Vector4((float)w,(float)h,0,0));_backgroundProperties.SetColor("_Center",center);_backgroundProperties.SetColor("_Middle",middle);
            _surface.DrawNativeMesh(_background,_backgroundMaterial,_backgroundProperties,inheritBlend:true,inheritShadow:true);
        }
        internal void Rectangle(double x,double y,double w,double h,double width,Color color)
            =>Paint(PolylineContour(true,P(x,y),P(x+w,y),P(x+w,y+h),P(x,y+h)),width,color,Matrix4x4.identity,false);
        internal void FireTrail(double x,double time,double alpha,NativeWorldCreatures glow)
        {
            glow.DrawOuterGlow(_surface,34,C(255,97,61),alpha*.24);
            Paint(EllipseContour(Vector2.zero,22,22),0,C(255,91,56,alpha*.16),Matrix4x4.identity);
            double radius=15+Math.Sin(time*50+x)*2;Paint(EllipseContour(Vector2.zero,(float)radius,(float)radius),1.5,C(255,180,70,alpha*.75),Matrix4x4.identity);
        }
        internal void StandardFallback(double r)=>Paint(EllipseContour(Vector2.zero,(float)(r*.62),(float)(r*.62)),1,C(90,220,255,.28),Matrix4x4.identity);
        internal void Particle(double x,double y,double size,double alpha,double vx,double vy,Color color)
        {
            double s=size*(.55+alpha*.65);color.a=(float)Math.Max(0,Math.Min(1,alpha*.82));
            Paint(PolylineContour(true,P(s*1.8,0),P(0,-s*.55),P(-s*1.2,0),P(0,s*.55)),0,color,T(x,y,Math.Atan2(vy,vx)));
        }
        internal void BossArrow(double x,double y,double angle)
        {
            var path=PolylineContour(true,P(16,0),P(-6,-9),P(-2,0),P(-6,9));var root=T(x,y,angle);
            Paint(path,0,C(255,92,121),root);Paint(path,1,C(255,219,228),root);
        }
        internal void Boundary(double radius,double progress,Color color)
        {
            Paint(EllipseContour(Vector2.zero,(float)(radius+8+progress*58),(float)(radius+8+progress*58)),2.2,color,Matrix4x4.identity);
        }
        internal void BoundaryOuter(double radius,double progress,Color color)
            =>Paint(EllipseContour(Vector2.zero,(float)(radius+20+progress*96),(float)(radius+20+progress*96)),1,color,Matrix4x4.identity);
        internal void BoundaryShards(double radius,double progress,Color color)
        {
            for(int i=0;i<4;i++){double a=i*Math.PI/2+progress*1.7,rr=radius+14+progress*62,size=5+progress*4,x=Math.Cos(a)*rr,y=Math.Sin(a)*rr;
                Paint(PolylineContour(true,P(x+Math.Cos(a)*size,y+Math.Sin(a)*size),P(x-Math.Sin(a)*size*.6,y+Math.Cos(a)*size*.6),P(x+Math.Sin(a)*size*.6,y-Math.Cos(a)*size*.6)),0,color,Matrix4x4.identity);}
        }
        internal float Measure(string text,float logicalSize)
        {int size=Mathf.Max(1,Mathf.CeilToInt(logicalSize));var font=SourceCanvasTypography.ResolveFamily("system-ui,sans-serif",size);return SourceCanvasTypography.Width(font,text,size,FontStyle.Bold)*logicalSize/size;}
        internal void Text(string text,double x,double y,Color color,float size)
        {if(_labelUsed==_labels.Count)_labels.Add(new NativeWorldLabel());_labels[_labelUsed++].Draw(_surface,text,P(x,y),color,fontSize:size,inheritBlend:true,inheritShadow:true,baseline:_surface.NativeTextBaseline);}
        public void Dispose(){foreach(var art in _art)if(art.Mesh!=null)UnityEngine.Object.Destroy(art.Mesh);foreach(var label in _labels)label.Dispose();UnityEngine.Object.Destroy(_solid);UnityEngine.Object.Destroy(_background);UnityEngine.Object.Destroy(_backgroundMaterial);}
    }
}
