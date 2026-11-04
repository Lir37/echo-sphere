using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    internal sealed class NativeWorldCharacterIndicators : IDisposable
    {
        internal struct Point {internal double X,Y;}
        internal struct Reaction {internal double X,Y,Radius;internal string Symbol;}
        internal struct Shield {internal double X,Y,PlayerRadius,Charges,Timer,Pulse,Time;internal bool Spherist;internal string Label;}
        private sealed class Art : IDisposable
        {
            internal Mesh Fill,Stroke;internal BezierContour Path;internal readonly BezierContour[] Paths=new BezierContour[1];
            internal void Build(BezierContour path,float width,NativeWorldSurface s,float[] dash,bool fill)
            {Path=path;Paths[0]=path;if(fill)Fill=Tessellate(path,0,Fill);if(width>0)Stroke=Tessellate(path,width,Stroke,cap:s.NativeStrokeEnding,corners:s.NativeStrokeCorner,miter:s.NativeStrokeMiter,dash:dash,dashOffset:s.NativeDashOffset);}
            public void Dispose(){Destroy(Fill);Destroy(Stroke);}
        }
        private readonly Material _material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Native character world indicators",hideFlags=HideFlags.DontSave};
        private readonly List<Art> _art=new List<Art>();private int _artUsed,_propertyUsed,_labelUsed;
        private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();
        private readonly List<NativeWorldLabel> _labels=new List<NativeWorldLabel>();
        private NativeWorldSurface _surface;
        internal void BeginFrame(){_artUsed=0;_propertyUsed=0;_labelUsed=0;}
        private static Vector2 P(double x,double y)=>new Vector2((float)x,(float)y);
        private static Color C(int r,int g,int b,double a=1)=>new Color(r/255f,g/255f,b/255f,(float)a);
        private static Matrix4x4 T(double x=0,double y=0,double scale=1)=>Matrix4x4.TRS(new Vector3((float)x,(float)y,0),Quaternion.identity,new Vector3((float)scale,(float)scale,1));
        private void Begin(NativeWorldSurface surface){_surface=surface;}
        private Art Geometry(BezierContour path,double width,float[] dash=null,bool fill=false)
        {if(_artUsed==_art.Count)_art.Add(new Art());var art=_art[_artUsed++];art.Build(path,(float)width,_surface,dash,fill);return art;}
        private MaterialPropertyBlock Properties(Color c){if(_propertyUsed==_properties.Count)_properties.Add(new MaterialPropertyBlock());var p=_properties[_propertyUsed++];p.SetVector("_NativeColor",new Vector4(c.r,c.g,c.b,c.a));return p;}
        private void Paint(Art art,Matrix4x4 root,Color? fill,Color? stroke,bool additive=false,double? opacity=null,bool strokeFirst=false)
        {
            void Fill(){if(fill.HasValue)_surface.DrawNativeMesh(art.Fill,_material,Properties(fill.Value),root,inheritBlend:!additive,additive:additive,opacity:opacity.HasValue?(float?)opacity.Value:null,inheritShadow:true);}
            void Stroke(){if(stroke.HasValue)_surface.DrawNativeMesh(art.Stroke,_material,Properties(stroke.Value),root,inheritBlend:!additive,additive:additive,opacity:opacity.HasValue?(float?)opacity.Value:null,inheritShadow:true);}
            if(strokeFirst){Stroke();Fill();}else{Fill();Stroke();}
        }

        private void Text(string text,Vector2 position,Color color,float size,string family,Matrix4x4? root=null,bool additive=false,float? opacity=null)
        {if(_labelUsed==_labels.Count)_labels.Add(new NativeWorldLabel());_labels[_labelUsed++].Draw(_surface,text,position,color,localTransform:root,fontSize:size,family:family,inheritBlend:!additive,additive:additive,inheritShadow:true,opacity:opacity);}
        private static double Distance(Point a,Point b){double dx=a.X-b.X,dy=a.Y-b.Y;return Math.Sqrt(dx*dx+dy*dy);}
        internal void DrawEngineer(NativeWorldSurface surface,Point[] spheres,double range)
        {
            Begin(surface);for(int i=0;i<spheres.Length;i++)for(int j=i+1;j<spheres.Length;j++){if(Distance(spheres[i],spheres[j])>range)continue;Paint(Geometry(PolylineContour(false,P(spheres[i].X,spheres[i].Y),P(spheres[j].X,spheres[j].Y)),1.5,new[]{5f,4f}),Matrix4x4.identity,null,C(74,122,138,.42));}
        }
        internal void DrawFormation(NativeWorldSurface surface,string type,Point[] spheres,Point player)
        {
            if(type=="none")return;Begin(surface);var points=spheres.Take(8).ToArray();Point[] ordered=null;bool closed=false;
            if(type=="line")ordered=points.OrderBy(p=>p.X).ThenBy(p=>p.Y).ToArray();
            else if(type=="triangle"){ordered=points.Take(3).ToArray();closed=true;}
            else if(type=="square"){double x=points.Sum(p=>p.X)/points.Length,y=points.Sum(p=>p.Y)/points.Length;ordered=points.Take(4).OrderBy(p=>Math.Atan2(p.Y-y,p.X-x)).ToArray();closed=true;}
            else if(type=="cluster")foreach(var p in points){if(Distance(p,player)>220)continue;Paint(Geometry(EllipseContour(P(p.X,p.Y),20,20),2,new[]{8f,5f}),Matrix4x4.identity,null,C(212,148,61,.55));}
            if(ordered!=null&&ordered.Length>=(closed?3:2)){var path=PolylineContour(closed,ordered.Select(p=>P(p.X,p.Y)).ToArray());Paint(Geometry(path,2,new[]{8f,5f},closed),Matrix4x4.identity,closed?(Color?)C(212,148,61,.08):null,C(212,148,61,.55),strokeFirst:true);}
        }
        internal void DrawHunter(NativeWorldSurface surface,double x,double y,double radius,bool hunt,string label)
        {
            Begin(surface);var color=hunt?C(212,148,61):C(196,109,61);float r=(float)(radius+(hunt?14:9));Paint(Geometry(EllipseContour(P(x,y),r,r),hunt?3:2,hunt?new[]{10f,5f}:new[]{6f,4f}),Matrix4x4.identity,null,color);Text(label,P(x,y-radius-12),color,10,"Georgia, serif");
        }
        internal void DrawAlchemist(NativeWorldSurface surface,Reaction[] reactions)
        {Begin(surface);foreach(var r in reactions)Text(r.Symbol,P(r.X,r.Y-r.Radius-6),C(138,90,138),10,"Georgia, serif");}
        internal void DrawBerserker(NativeWorldSurface surface,double x,double y,double hp,double maxHp)
        {Begin(surface);double missing=Math.Max(0,1-hp/Math.Max(1,maxHp)),steps=Math.Min(4,Math.Floor(missing/.2));Paint(Geometry(EllipseContour(P(x,y),110,110),steps>0?2:1,new[]{6f,5f}),Matrix4x4.identity,null,C(196,69,61,steps>0?.22:.10));}
        internal void DrawShield(NativeWorldSurface surface,Shield s,NativeWorldSprites sprites,Func<string,INativeWorldSprite> resolve)
        {
            if(s.Charges<=0&&s.Timer<=0)return;Begin(surface);
            double radius=s.PlayerRadius*1.48,hit=Math.Max(0,Math.Min(1,s.Pulse/.85));var root=T(s.X,s.Y);
            // A light segmented membrane leaves the character and ground readable.
            for(int i=0;i<6;i++){
                double start=i*Math.PI/3+s.Time*.12;var points=new Vector2[9];
                for(int j=0;j<9;j++){double a=start+j/8d*Math.PI*.245;points[j]=P(Math.Cos(a)*radius,Math.Sin(a)*radius);}
                Paint(Geometry(PolylineContour(false,points),1.4+hit*.8),root,null,C(105,225,250,.68+hit*.28),true);
            }
            Paint(Geometry(EllipseContour(Vector2.zero,(float)(radius*.93),(float)(radius*.93)),.65,fill:true),root,C(100,210,245,.035+hit*.055),C(180,245,255,.24),true);
            int charges=(int)Math.Min(12,Math.Max(0,s.Charges));
            for(int i=0;i<charges;i++){double a=-Math.PI/2+(i-(charges-1)*.5)*.22;var q=P(Math.Cos(a)*radius,Math.Sin(a)*radius);Paint(Geometry(EllipseContour(q,1.8f,1.8f),0,fill:true),root,C(225,252,255),null,true);}
            if(hit>.02)Paint(Geometry(EllipseContour(Vector2.zero,(float)(radius*(1+(1-hit)*.25)),(float)(radius*(1+(1-hit)*.25))),1),root,null,C(185,245,255,hit*.55),true);
        }
        private static void Destroy(UnityEngine.Object value){if(value!=null)UnityEngine.Object.Destroy(value);}
        public void Dispose(){foreach(var art in _art)art.Dispose();foreach(var label in _labels)label.Dispose();_art.Clear();_labels.Clear();Destroy(_material);}
    }
}
