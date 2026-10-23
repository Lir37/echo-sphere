using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    // Presentation only. Network membership, formations and charge remain game data.
    internal sealed partial class NativeWorldNetwork : IDisposable
    {
        internal struct Node {internal double X,Y,Charge,Pulse;}
        internal struct Link {internal int A,B;}
        internal sealed class Formation {internal int[] Nodes;internal bool Active;}
        internal sealed class Network
        {
            internal int NodeCount;internal Node[] Spheres;internal Link[] Links;
            internal Formation Line,Triangle,Square,Cluster,Ring,Lattice,Fractal;
        }
        private sealed class Art : IDisposable
        {
            internal Mesh Mesh;internal readonly BezierContour[] Paths=new BezierContour[1];
            public void Dispose(){if(Mesh!=null)UnityEngine.Object.Destroy(Mesh);}
        }
        private readonly Material _material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Native sphere network",hideFlags=HideFlags.DontSave};
        private readonly List<Art> _art=new List<Art>();private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();
        private int _artUsed,_propertyUsed;private SourceCanvasSurface _surface;private BezierContour[] _last;private Matrix4x4 _lastMatrix;
        internal void BeginFrame(){_artUsed=0;_propertyUsed=0;}
        private void Begin(SourceCanvasSurface s){_surface=s;_last=null;}
        private void Finish(){if(_last!=null)_surface.ContinueNativeContours(_last,_lastMatrix);}
        private static Vector2 P(double x,double y)=>new Vector2((float)x,(float)y);
        private static Color C(int r,int g,int b,double a=1)=>new Color(r/255f,g/255f,b/255f,(float)a);
        private static Matrix4x4 T(double x=0,double y=0,double scale=1,double angle=0)=>Matrix4x4.TRS(new Vector3((float)x,(float)y,0),Quaternion.Euler(0,0,(float)(angle*180/Math.PI)),new Vector3((float)scale,(float)scale,1));
        private void Paint(BezierContour path,double width,Color color,double alpha,Matrix4x4 root,Color? shadow=null,float blur=0,float[] dash=null,bool round=false)
        {
            if(_artUsed==_art.Count)_art.Add(new Art());var art=_art[_artUsed++];art.Paths[0]=path;
            art.Mesh=Tessellate(path,(float)width,art.Mesh,cap:round?PathEnding.Round:_surface.NativeStrokeEnding,corners:round?PathCorner.Round:_surface.NativeStrokeCorner,miter:_surface.NativeStrokeMiter,dash:dash,dashOffset:_surface.NativeDashOffset);
            if(_propertyUsed==_properties.Count)_properties.Add(new MaterialPropertyBlock());var props=_properties[_propertyUsed++];props.SetVector("_NativeColor",new Vector4(color.r,color.g,color.b,color.a));
            _surface.DrawNativeMesh(art.Mesh,_material,props,root,shadow:shadow,shadowBlur:blur,inheritBlend:true,opacity:(float)alpha,inheritShadow:!shadow.HasValue,inheritShadowOffsets:shadow.HasValue);
            _last=art.Paths;_lastMatrix=root;
        }
        private void Circle(double x,double y,double radius,double width,Color color,double alpha,Matrix4x4 root,Color? shadow=null,float blur=0,float[] dash=null,double start=0,double end=Math.PI*2)
            =>Paint(EllipseContour(P(x,y),(float)radius,(float)radius,(float)start,(float)end),width,color,alpha,root,shadow,blur,dash);
        private void Segment(double x,double y,double xx,double yy,double width,Color color,double alpha,Matrix4x4 root,Color? shadow=null,float blur=0)
            =>Paint(PolylineContour(false,P(x,y),P(xx,yy)),width,color,alpha,root,shadow,blur);
        internal void DrawMemory(SourceCanvasSurface surface,string type,Node[] nodes,double expires,double time)
        {
            if(expires<=time||nodes.Length<2)return;Begin(surface);double alpha=Math.Max(0,Math.Min(1,(expires-time)/.9));var color=type=="triangle"?C(255,184,77):type=="square"?C(105,183,255):type=="cluster"?C(179,140,255):C(99,230,255);var dash=new[]{5f,7f};
            Paint(PolylineContour(type=="triangle"||type=="square",nodes.Select(n=>P(n.X,n.Y)).ToArray()),1.4,color,.34*alpha,Matrix4x4.identity,color,5,dash);
            foreach(var n in nodes)Circle(n.X,n.Y,10+Math.Sin(time*7)*1.5,1.4,color,.22*alpha,Matrix4x4.identity,color,5,dash);Finish();
        }
        internal void DrawLink(SourceCanvasSurface surface,Node from,Node to,Color color,double alpha,double time,double seed,bool active)
        {Begin(surface);Wavy(from,to,color,alpha,time,seed,active);Finish();}
        private void Wavy(Node from,Node to,Color color,double alpha,double time,double seed,bool active)
        {
            double dx=to.X-from.X,dy=to.Y-from.Y,len=Math.Sqrt(dx*dx+dy*dy);if(len==0)len=1;
            double nx=-dy/len,ny=dx/len,amp=Math.Min(6.2,2.2+len*.008),phase=time*(active?1.9:1.35)+seed;
            Node At(double p){double q=Math.Max(0,Math.Min(1,p)),wave=Math.Sin(phase+q*Math.PI*4.2)*amp*Math.Sin(Math.PI*q);return new Node {X=from.X+dx*q+nx*wave,Y=from.Y+dy*q+ny*wave};}
            var points=new Vector2[25];for(int i=0;i<=24;i++){var n=At(i/24.0);points[i]=P(n.X,n.Y);}var path=PolylineContour(false,points);
            Paint(path,active?5.8:3.6,color,alpha*(active?.22:.12),Matrix4x4.identity,round:true);
            Paint(path,active?2.55:1.95,color,alpha*(active?.82:.64),Matrix4x4.identity,round:true);
            Paint(path,active?.92:.70,color,alpha*(active?.72:.58),Matrix4x4.identity,round:true);
            for(int packet=0;packet<3;packet++){
                double raw=((time*(active?.34:.27)+seed*.027+packet/3.0)%1+1)%1,p=packet%2==0?raw:1-raw;var head=At(p);var tail=At(Math.Max(0,p-(active?.075:.06)));var root=T(head.X,head.Y,angle:Math.Atan2(head.Y-tail.Y,head.X-tail.X));
                Paint(PolylineContour(true,P(7.5,0),P(-2,-3),P(-5.2,0),P(-2,3)),0,C(248,255,255),(active?.86:.62)*alpha,root);
                Paint(PolylineContour(true,P(-1.5,0),P(-10.5,-1.7),P(-15,0),P(-10.5,1.7)),0,color,(active?.48:.34)*alpha,root);
            }
        }
        private static bool Has(Formation f,int index)=>f!=null&&Array.IndexOf(f.Nodes,index)>=0;
        private static Node Center(Formation f,Node[] nodes){double x=0,y=0;foreach(int i in f.Nodes){x+=nodes[i].X;y+=nodes[i].Y;}return new Node {X=x/f.Nodes.Length,Y=y/f.Nodes.Length};}
        private static double Radius(Formation f,Node[] nodes,Node center){double r=0;foreach(int i in f.Nodes){double dx=nodes[i].X-center.X,dy=nodes[i].Y-center.Y;r=Math.Max(r,Math.Sqrt(dx*dx+dy*dy));}return r;}
        internal void Draw(SourceCanvasSurface surface,Network n,Node?[] runtimeNodes,Link[] external,double t)
        {
            if(n.NodeCount<2&&external.Length==0)return;Begin(surface);
            foreach(var l in external){if(l.A<0||l.B<0||l.A>=runtimeNodes.Length||l.B>=runtimeNodes.Length||!runtimeNodes[l.A].HasValue||!runtimeNodes[l.B].HasValue)continue;Wavy(runtimeNodes[l.A].Value,runtimeNodes[l.B].Value,C(99,185,255),.24,t,l.A*17.13+l.B*29.71,false);}
            double pulse=.82+Math.Sin(t*4.2)*.10;
            foreach(var l in n.Links){double alpha=.27;var color=C(99,185,255);bool active=false;
                if(n.Line!=null&&n.Line.Active&&Has(n.Line,l.A)&&Has(n.Line,l.B)){alpha=.50;color=C(99,230,255);active=true;}
                else if(Has(n.Square,l.A)&&Has(n.Square,l.B)){alpha=.48;color=C(105,183,255);active=true;}
                else if(n.Triangle!=null&&n.Triangle.Active&&Has(n.Triangle,l.A)&&Has(n.Triangle,l.B)){alpha=.54;color=C(255,184,77);active=true;}
                else if(Has(n.Cluster,l.A)&&Has(n.Cluster,l.B)){alpha=.44;color=C(179,140,255);active=true;}
                Wavy(n.Spheres[l.A],n.Spheres[l.B],color,alpha*pulse,t,l.A*17.13+l.B*29.71,active);
            }
            if(n.Line!=null){var c=Center(n.Line,n.Spheres);double spread=34+n.Line.Nodes.Length*5;var color=C(99,230,255);Segment(-spread,0,spread,0,1,color,.20+.05*Math.Sin(t*5),T(c.X,c.Y),color,4);}
            if(n.Triangle!=null)Triangle(n.Spheres,n.Triangle,t);
            if(n.Square!=null)Square(n.Spheres,n.Square,t);
            if(n.Cluster!=null)Cluster(n.Spheres,n.Cluster,t);
            if(n.Ring!=null){var color=C(85,230,154);Paint(PolylineContour(true,n.Ring.Nodes.Select(i=>P(n.Spheres[i].X,n.Spheres[i].Y)).ToArray()),1.6,color,.24+.08*Math.Sin(t*5),Matrix4x4.identity,color);}
            if(n.Lattice!=null){var c=Center(n.Lattice,n.Spheres);var color=C(57,216,255);foreach(int i in n.Lattice.Nodes){var p=n.Spheres[i];Segment(c.X,c.Y,p.X,p.Y,1,color,.20+.06*Math.Sin(t*6),Matrix4x4.identity);Circle(p.X,p.Y,2.8+Math.Sin(t*7+i)*.7,0,color,.62,Matrix4x4.identity);}}
            if(n.Fractal!=null){var c=Center(n.Fractal,n.Spheres);double r=Radius(n.Fractal,n.Spheres,c);var color=C(255,107,157);foreach(double scale in new[]{1,.62,.34})Circle(c.X,c.Y,Math.Max(8,r*scale),1.2,color,.24+.10*Math.Sin(t*8),Matrix4x4.identity,color);}
            Finish();
        }
        public void Dispose(){foreach(var art in _art)art.Dispose();_art.Clear();UnityEngine.Object.Destroy(_material);}
    }
}
