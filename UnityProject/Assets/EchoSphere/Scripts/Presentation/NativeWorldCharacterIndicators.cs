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
            internal void Build(BezierContour path,float width,SourceCanvasSurface s,float[] dash,bool fill)
            {Path=path;Paths[0]=path;if(fill)Fill=Tessellate(path,0,Fill);if(width>0)Stroke=Tessellate(path,width,Stroke,cap:s.NativeStrokeEnding,corners:s.NativeStrokeCorner,miter:s.NativeStrokeMiter,dash:dash,dashOffset:s.NativeDashOffset);}
            public void Dispose(){Destroy(Fill);Destroy(Stroke);}
        }
        private readonly Material _material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Native character world indicators",hideFlags=HideFlags.DontSave};
        private readonly List<Art> _art=new List<Art>();private int _artUsed,_propertyUsed,_labelUsed;
        private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();
        private readonly List<NativeWorldLabel> _labels=new List<NativeWorldLabel>();
        private SourceCanvasSurface _surface;private BezierContour[] _last;private Matrix4x4 _lastMatrix;
        internal void BeginFrame(){_artUsed=0;_propertyUsed=0;_labelUsed=0;}
        private static Vector2 P(double x,double y)=>new Vector2((float)x,(float)y);
        private static Color C(int r,int g,int b,double a=1)=>new Color(r/255f,g/255f,b/255f,(float)a);
        private static Matrix4x4 T(double x=0,double y=0,double scale=1)=>Matrix4x4.TRS(new Vector3((float)x,(float)y,0),Quaternion.identity,new Vector3((float)scale,(float)scale,1));
        private void Begin(SourceCanvasSurface surface){_surface=surface;_last=null;}
        private Art Geometry(BezierContour path,double width,float[] dash=null,bool fill=false)
        {if(_artUsed==_art.Count)_art.Add(new Art());var art=_art[_artUsed++];art.Build(path,(float)width,_surface,dash,fill);return art;}
        private MaterialPropertyBlock Properties(Color c){if(_propertyUsed==_properties.Count)_properties.Add(new MaterialPropertyBlock());var p=_properties[_propertyUsed++];p.SetVector("_NativeColor",new Vector4(c.r,c.g,c.b,c.a));return p;}
        private void Paint(Art art,Matrix4x4 root,Color? fill,Color? stroke,bool additive=false,double? opacity=null,bool strokeFirst=false)
        {
            void Fill(){if(fill.HasValue)_surface.DrawNativeMesh(art.Fill,_material,Properties(fill.Value),root,inheritBlend:!additive,additive:additive,opacity:opacity.HasValue?(float?)opacity.Value:null,inheritShadow:true);}
            void Stroke(){if(stroke.HasValue)_surface.DrawNativeMesh(art.Stroke,_material,Properties(stroke.Value),root,inheritBlend:!additive,additive:additive,opacity:opacity.HasValue?(float?)opacity.Value:null,inheritShadow:true);}
            if(strokeFirst){Stroke();Fill();}else{Fill();Stroke();}_last=art.Paths;_lastMatrix=root;
        }
        private void Finish(){if(_last!=null)_surface.ContinueNativeContours(_last,_lastMatrix);}
        private void Text(string text,Vector2 position,Color color,float size,string family,Matrix4x4? root=null,bool additive=false,float? opacity=null)
        {if(_labelUsed==_labels.Count)_labels.Add(new NativeWorldLabel());_labels[_labelUsed++].Draw(_surface,text,position,color,localTransform:root,fontSize:size,family:family,inheritBlend:!additive,additive:additive,inheritShadow:true,opacity:opacity);}
        private static double Distance(Point a,Point b){double dx=a.X-b.X,dy=a.Y-b.Y;return Math.Sqrt(dx*dx+dy*dy);}
        internal void DrawEngineer(SourceCanvasSurface surface,Point[] spheres,double range)
        {
            Begin(surface);for(int i=0;i<spheres.Length;i++)for(int j=i+1;j<spheres.Length;j++){if(Distance(spheres[i],spheres[j])>range)continue;Paint(Geometry(PolylineContour(false,P(spheres[i].X,spheres[i].Y),P(spheres[j].X,spheres[j].Y)),1.5,new[]{5f,4f}),Matrix4x4.identity,null,C(74,122,138,.42));}Finish();
        }
        internal void DrawFormation(SourceCanvasSurface surface,string type,Point[] spheres,Point player)
        {
            if(type=="none")return;Begin(surface);var points=spheres.Take(8).ToArray();Point[] ordered=null;bool closed=false;
            if(type=="line")ordered=points.OrderBy(p=>p.X).ThenBy(p=>p.Y).ToArray();
            else if(type=="triangle"){ordered=points.Take(3).ToArray();closed=true;}
            else if(type=="square"){double x=points.Sum(p=>p.X)/points.Length,y=points.Sum(p=>p.Y)/points.Length;ordered=points.Take(4).OrderBy(p=>Math.Atan2(p.Y-y,p.X-x)).ToArray();closed=true;}
            else if(type=="cluster")foreach(var p in points){if(Distance(p,player)>220)continue;Paint(Geometry(EllipseContour(P(p.X,p.Y),20,20),2,new[]{8f,5f}),Matrix4x4.identity,null,C(212,148,61,.55));}
            if(ordered!=null&&ordered.Length>=(closed?3:2)){var path=PolylineContour(closed,ordered.Select(p=>P(p.X,p.Y)).ToArray());Paint(Geometry(path,2,new[]{8f,5f},closed),Matrix4x4.identity,closed?(Color?)C(212,148,61,.08):null,C(212,148,61,.55),strokeFirst:true);}Finish();
        }
        internal void DrawHunter(SourceCanvasSurface surface,double x,double y,double radius,bool hunt,string label)
        {
            Begin(surface);var color=hunt?C(212,148,61):C(196,109,61);float r=(float)(radius+(hunt?14:9));Paint(Geometry(EllipseContour(P(x,y),r,r),hunt?3:2,hunt?new[]{10f,5f}:new[]{6f,4f}),Matrix4x4.identity,null,color);Text(label,P(x,y-radius-12),color,10,"Georgia, serif");Finish();
        }
        internal void DrawAlchemist(SourceCanvasSurface surface,Reaction[] reactions)
        {Begin(surface);foreach(var r in reactions)Text(r.Symbol,P(r.X,r.Y-r.Radius-6),C(138,90,138),10,"Georgia, serif");}
        internal void DrawBerserker(SourceCanvasSurface surface,double x,double y,double hp,double maxHp)
        {Begin(surface);double missing=Math.Max(0,1-hp/Math.Max(1,maxHp)),steps=Math.Min(4,Math.Floor(missing/.2));Paint(Geometry(EllipseContour(P(x,y),110,110),steps>0?2:1,new[]{6f,5f}),Matrix4x4.identity,null,C(196,69,61,steps>0?.22:.10));Finish();}
        internal void DrawShield(SourceCanvasSurface surface,Shield s,NativeWorldSprites sprites,Func<string,SourceCanvasImage> resolve)
        {
            if(s.Charges<=0||s.Timer<=0)return;Begin(surface);double pulse=1+Math.Sin(s.Time*8)*.035,radius=s.PlayerRadius*1.38,recast=Math.Max(0,Math.Min(1,s.Pulse/.85)),wave=1+(1-recast)*.55,alpha=.20+recast*.65;var root=T(s.X,s.Y,pulse);
            if(s.Spherist)sprites.Draw(surface,resolve("player-spherist-shield"),0,0,radius*2.20,0,.78,false,false,false,root,additive:true);
            var points=new Vector2[8];for(int i=0;i<8;i++){double a=-Math.PI/8+i*Math.PI/4+Math.Sin(s.Time*1.8)*.03,rr=radius*(1+Math.Sin(s.Time*4+i)*.012);points[i]=P(Math.Cos(a)*rr,Math.Sin(a)*rr*.88);}Paint(Geometry(PolylineContour(true,points),2.2,fill:true),root,C(64,214,255,.08),C(99,230,255,.88),true);
            Paint(Geometry(EllipseContour(Vector2.zero,(float)(radius*.86),(float)(radius*.86)),1),root,null,C(225,251,255,.72),true);
            if(recast>.02){Paint(Geometry(EllipseContour(Vector2.zero,(float)(radius*wave),(float)(radius*wave)),2.2+recast*1.8),root,null,C(223,250,255),true,alpha);Paint(Geometry(EllipseContour(Vector2.zero,(float)(radius*(wave+.08)),(float)(radius*(wave+.08))),5),root,null,C(223,250,255),true,alpha*.42);}
            for(int i=0;i<s.Charges;i++){double a=-Math.PI/2+i*Math.PI*2/Math.Max(1,s.Charges)+s.Time*.45;Paint(Geometry(EllipseContour(P(Math.Cos(a)*radius,Math.Sin(a)*radius*.88),3.2f,3.2f),0,fill:true),root,C(223,250,255),null,true,1);}
            Text(s.Label,P(0,3),C(223,250,255),9,"system-ui,sans-serif",root,true,1);Finish();
        }
        private static void Destroy(UnityEngine.Object value){if(value!=null)UnityEngine.Object.Destroy(value);}
        public void Dispose(){foreach(var art in _art)art.Dispose();foreach(var label in _labels)label.Dispose();_art.Clear();_labels.Clear();Destroy(_material);}
    }
}
