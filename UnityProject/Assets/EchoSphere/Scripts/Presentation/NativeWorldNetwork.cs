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
            internal readonly List<Vector3> Vertices=new List<Vector3>(128);internal readonly List<int> Indices=new List<int>(384);
            internal Mesh Mesh,RibbonMesh;internal readonly BezierContour[] Paths=new BezierContour[1];
            public void Dispose(){if(Mesh!=null)UnityEngine.Object.Destroy(Mesh);if(RibbonMesh!=null)UnityEngine.Object.Destroy(RibbonMesh);}
        }
        private readonly Material _material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Native sphere network",hideFlags=HideFlags.DontSave};
        private readonly List<Art> _art=new List<Art>();private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();
        private readonly Vector2[] _wavePoints=new Vector2[25];
#if UNITY_EDITOR
        internal bool UseVectorNetworkForValidation=false;
#endif
        private int _artUsed,_propertyUsed;private NativeWorldSurface _surface;
        internal void BeginFrame(){_artUsed=0;_propertyUsed=0;}
        private void Begin(NativeWorldSurface s){_surface=s;}

        private static Vector2 P(double x,double y)=>new Vector2((float)x,(float)y);
        private static Color C(int r,int g,int b,double a=1)=>new Color(r/255f,g/255f,b/255f,(float)a);
        private static Matrix4x4 T(double x=0,double y=0,double scale=1,double angle=0)=>Matrix4x4.TRS(new Vector3((float)x,(float)y,0),Quaternion.Euler(0,0,(float)(angle*180/Math.PI)),new Vector3((float)scale,(float)scale,1));
        private void Paint(BezierContour path,double width,Color color,double alpha,Matrix4x4 root,Color? shadow=null,float blur=0,float[] dash=null,bool round=false)
        {
            if(_artUsed==_art.Count)_art.Add(new Art());var art=_art[_artUsed++];art.Paths[0]=path;
            art.Mesh=Tessellate(path,(float)width,art.Mesh,cap:round?PathEnding.Round:_surface.NativeStrokeEnding,corners:round?PathCorner.Round:_surface.NativeStrokeCorner,miter:_surface.NativeStrokeMiter,dash:dash,dashOffset:_surface.NativeDashOffset);
            if(_propertyUsed==_properties.Count)_properties.Add(new MaterialPropertyBlock());var props=_properties[_propertyUsed++];props.SetVector("_NativeColor",new Vector4(color.r,color.g,color.b,color.a));
            _surface.DrawNativeMesh(art.Mesh,_material,props,root,shadow:shadow,shadowBlur:blur,inheritBlend:true,opacity:(float)alpha,inheritShadow:!shadow.HasValue,inheritShadowOffsets:shadow.HasValue);

        }
        private void Circle(double x,double y,double radius,double width,Color color,double alpha,Matrix4x4 root,Color? shadow=null,float blur=0,float[] dash=null,double start=0,double end=Math.PI*2)
            =>Paint(EllipseContour(P(x,y),(float)radius,(float)radius,(float)start,(float)end),width,color,alpha,root,shadow,blur,dash);
        private void Segment(double x,double y,double xx,double yy,double width,Color color,double alpha,Matrix4x4 root,Color? shadow=null,float blur=0)
            =>Paint(PolylineContour(false,P(x,y),P(xx,yy)),width,color,alpha,root,shadow,blur);
        internal void DrawMemory(NativeWorldSurface surface,string type,Node[] nodes,double expires,double time)
        {
            if(expires<=time||nodes.Length<2)return;Begin(surface);double alpha=Math.Max(0,Math.Min(1,(expires-time)/.9));var color=type=="triangle"?C(255,184,77):type=="square"?C(105,183,255):type=="cluster"?C(179,140,255):C(99,230,255);var dash=new[]{5f,7f};
            Paint(PolylineContour(type=="triangle"||type=="square",nodes.Select(n=>P(n.X,n.Y)).ToArray()),1.4,color,.34*alpha,Matrix4x4.identity,color,5,dash);
            foreach(var n in nodes)Circle(n.X,n.Y,10+Math.Sin(time*7)*1.5,1.4,color,.22*alpha,Matrix4x4.identity,color,5,dash);
        }
        internal void DrawLink(NativeWorldSurface surface,Node from,Node to,Color color,double alpha,double time,double seed,bool active)
        {Begin(surface);Wavy(from,to,color,alpha,time,seed,active);}
        private void Wavy(Node from,Node to,Color color,double alpha,double time,double seed,bool active)
        {
            double dx=to.X-from.X,dy=to.Y-from.Y,len=Math.Sqrt(dx*dx+dy*dy);if(len==0)len=1;
            double nx=-dy/len,ny=dx/len,amp=active?Math.Min(2.2,.7+len*.003):Math.Min(1.0,len*.007),phase=time*(active?1.9:1.35)+seed;
            Node At(double p){double q=Math.Max(0,Math.Min(1,p)),wave=Math.Sin(phase+q*Math.PI*4.2)*amp*Math.Sin(Math.PI*q);return new Node {X=from.X+dx*q+nx*wave,Y=from.Y+dy*q+ny*wave};}
            var points=_wavePoints;for(int i=0;i<=24;i++){var n=At(i/24.0);points[i]=P(n.X,n.Y);}
            if(!active){Ribbon(points,.85,color,alpha*.70);return;}
            Ribbon(points,4.6,color,alpha*.12);Ribbon(points,1.75,color,alpha);Ribbon(points,.55,C(231,252,255),alpha*.30);
            for(int packet=0;packet<2;packet++){
                double raw=((time*.26+seed*.027+packet*.5)%1+1)%1;
                double p=packet==0?raw:1-raw;var head=At(p);
                // Sample along the direction of travel, including the reverse packet.
                double direction=packet==0?1:-1;var before=At(Math.Max(0,Math.Min(1,p-direction*.002)));var after=At(Math.Max(0,Math.Min(1,p+direction*.002)));
                DrawComet(head,Math.Atan2(after.Y-before.Y,after.X-before.X),color,alpha);
            }
        }

        private void Ribbon(Vector2[] points,double width,Color color,double alpha){
#if UNITY_EDITOR
            if(UseVectorNetworkForValidation){Paint(PolylineContour(false,points),width,color,alpha,Matrix4x4.identity,round:true);return;}
#endif
            if(_artUsed==_art.Count)_art.Add(new Art());var art=_art[_artUsed++];var v=art.Vertices;var ids=art.Indices;v.Clear();ids.Clear();float half=(float)width*.5f;
            for(int i=0;i<points.Length;i++){var before=(points[i]-points[Math.Max(0,i-1)]).normalized;var after=(points[Math.Min(points.Length-1,i+1)]-points[i]).normalized;if(i==0)before=after;if(i==points.Length-1)after=before;var n0=new Vector2(-before.y,before.x);var n1=new Vector2(-after.y,after.x);var join=(n0+n1).normalized;float denom=Vector2.Dot(join,n1);var offset=join*(half/Mathf.Max(.1f,denom));v.Add(points[i]+offset);v.Add(points[i]-offset);if(i>0){int k=i*2;ids.Add(k-2);ids.Add(k-1);ids.Add(k);ids.Add(k);ids.Add(k-1);ids.Add(k+1);}}
            // Smooth sine links have tiny bend angles: joined strips preserve
            // width without overlapping alpha. Exact round end caps stay visible.
            for(int end=0;end<2;end++){int ix=end==0?0:points.Length-1;var direction=(points[ix]-points[end==0?1:ix-1]).normalized;float angle=Mathf.Atan2(direction.y,direction.x)-Mathf.PI*.5f;int center=v.Count;v.Add(points[ix]);for(int j=0;j<=32;j++){float q=angle+j*Mathf.PI/32;v.Add(points[ix]+new Vector2(Mathf.Cos(q),Mathf.Sin(q))*half);if(j>0){ids.Add(center);ids.Add(center+j);ids.Add(center+j+1);}}}
            if(art.RibbonMesh==null){art.RibbonMesh=new Mesh{name="Retained sphere network ribbon"};art.RibbonMesh.MarkDynamic();}art.RibbonMesh.Clear();art.RibbonMesh.SetVertices(v);art.RibbonMesh.SetTriangles(ids,0);art.RibbonMesh.RecalculateBounds();
            if(_propertyUsed==_properties.Count)_properties.Add(new MaterialPropertyBlock());var props=_properties[_propertyUsed++];props.SetVector("_NativeColor",new Vector4(color.r,color.g,color.b,color.a));_surface.DrawNativeMesh(art.RibbonMesh,_material,props,Matrix4x4.identity,inheritBlend:true,opacity:(float)alpha,inheritShadow:true);
        }
        private static bool Has(Formation f,int index)=>f!=null&&Array.IndexOf(f.Nodes,index)>=0;
        private static Node Center(Formation f,Node[] nodes){double x=0,y=0;foreach(int i in f.Nodes){x+=nodes[i].X;y+=nodes[i].Y;}return new Node {X=x/f.Nodes.Length,Y=y/f.Nodes.Length};}
        private static double Radius(Formation f,Node[] nodes,Node center){double r=0;foreach(int i in f.Nodes){double dx=nodes[i].X-center.X,dy=nodes[i].Y-center.Y;r=Math.Max(r,Math.Sqrt(dx*dx+dy*dy));}return r;}
        internal void Draw(NativeWorldSurface surface,Network n,Node?[] runtimeNodes,Link[] external,double t)
        {
#if UNITY_INCLUDE_INSTRUMENTATION
            using var diagnosticScope=NativeWorldDiagnostics.Network.Auto();
#endif

            if(n.NodeCount<2&&external.Length==0)return;Begin(surface);
            foreach(var l in external){if(l.A<0||l.B<0||l.A>=runtimeNodes.Length||l.B>=runtimeNodes.Length||!runtimeNodes[l.A].HasValue||!runtimeNodes[l.B].HasValue)continue;Wavy(runtimeNodes[l.A].Value,runtimeNodes[l.B].Value,C(99,185,255),.24,t,l.A*17.13+l.B*29.71,false);}
            _geometryEdges.Clear();
            CollectFormation(n,n.Line,0);CollectFormation(n,n.Triangle,1);CollectFormation(n,n.Square,2);CollectFormation(n,n.Cluster,3);CollectFormation(n,n.Ring,4);CollectFormation(n,n.Lattice,5);CollectFormation(n,n.Fractal,6);
            foreach(var link in n.Links){if(link.A<0||link.B<0||link.A>=n.Spheres.Length||link.B>=n.Spheres.Length)continue;
                if(FormedPair(n,link.A,link.B))continue;
                Wavy(n.Spheres[link.A],n.Spheres[link.B],C(99,185,220),.19,t,link.A*17.13+link.B*29.71,false);
            }
            foreach(var pair in _geometryEdges){var e=pair.Value;Wavy(n.Spheres[pair.Key.Item1],n.Spheres[pair.Key.Item2],e.Color,e.Active?.62:.16,t,pair.Key.Item1*17.13+pair.Key.Item2*29.71,e.Active);}
            if(n.Triangle!=null)TriangleCharges(n.Spheres,n.Triangle);
        }

        public void Dispose(){foreach(var art in _art)art.Dispose();_art.Clear();UnityEngine.Object.Destroy(_cometHead);UnityEngine.Object.Destroy(_cometHalo);UnityEngine.Object.Destroy(_material);}
    }
}
