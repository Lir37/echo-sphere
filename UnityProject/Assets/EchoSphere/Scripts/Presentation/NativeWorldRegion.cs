using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    internal sealed class NativeWorldRegion : IDisposable
    {
        internal sealed class Pocket {internal string Id;internal Vector2 Center;internal float Radius;internal Color Accent;}
        internal sealed class Poi {internal int Id;internal Vector2 Position;internal Color Color;internal string Label;}
        internal sealed class State {internal double Time;internal string Active;internal readonly List<Pocket> Pockets=new List<Pocket>();internal readonly List<Poi> Pois=new List<Poi>();}
        private sealed class Geometry : IDisposable
        {
            internal readonly Mesh Mesh=new Mesh {name="Native region geometry"};
            private readonly List<Vector3> _vertices=new List<Vector3>();private readonly List<Vector4> _uv=new List<Vector4>(),_arcs=new List<Vector4>();private readonly List<Color> _colors=new List<Color>();private readonly List<int> _triangles=new List<int>();
            internal void Clear(){_vertices.Clear();_uv.Clear();_arcs.Clear();_colors.Clear();_triangles.Clear();}
            private void Vertex(Vector2 p,Vector4 uv,Vector4 arc,Color color){_vertices.Add(p);_uv.Add(uv);_arcs.Add(arc);_colors.Add(color);}
            internal void Circle(Vector2 center,float radius,float width,Color color,float start=0,float end=0,bool arc=false)
            {
                int first=_vertices.Count;float extent=radius+Mathf.Max(0,width*.5f)+1;var data=new Vector4(start,end,arc?1:0,0);
                foreach(var q in new[]{new Vector2(-extent,-extent),new Vector2(extent,-extent),new Vector2(extent,extent),new Vector2(-extent,extent)})Vertex(center+q,new Vector4(q.x,q.y,radius,width),data,color);
                Quad(first);
            }
            internal void Triangle(Vector2 a,Vector2 b,Vector2 c,Color color){int i=_vertices.Count;Vertex(a,new Vector4(0,0,-1,0),Vector4.zero,color);Vertex(b,new Vector4(0,0,-1,0),Vector4.zero,color);Vertex(c,new Vector4(0,0,-1,0),Vector4.zero,color);_triangles.Add(i);_triangles.Add(i+1);_triangles.Add(i+2);}
            internal void Diamond(Vector2 p,Color color)
            {
                // A2px miter stroke on the source11px diamond.
                float delta=Mathf.Sqrt(2);Vector2[] outer={p+new Vector2(0,-11-delta),p+new Vector2(11+delta,0),p+new Vector2(0,11+delta),p+new Vector2(-11-delta,0)},inner={p+new Vector2(0,-11+delta),p+new Vector2(11-delta,0),p+new Vector2(0,11-delta),p+new Vector2(-11+delta,0)};
                for(int i=0;i<4;i++){int next=(i+1)%4;Triangle(outer[i],outer[next],inner[next],color);Triangle(outer[i],inner[next],inner[i],color);}
            }
            private void Quad(int i){_triangles.Add(i);_triangles.Add(i+1);_triangles.Add(i+2);_triangles.Add(i);_triangles.Add(i+2);_triangles.Add(i+3);}
            internal void Upload(){Mesh.Clear();Mesh.SetVertices(_vertices);Mesh.SetColors(_colors);Mesh.SetUVs(0,_uv);Mesh.SetUVs(1,_arcs);Mesh.SetTriangles(_triangles,0);Mesh.RecalculateBounds();}
            public void Dispose(){UnityEngine.Object.Destroy(Mesh);}
        }
        private sealed class PoiView : IDisposable {internal readonly Geometry Geometry=new Geometry();internal readonly NativeWorldLabel Label=new NativeWorldLabel();public void Dispose(){Geometry.Dispose();Label.Dispose();}}
        private readonly Geometry _pockets=new Geometry();
        private readonly Dictionary<int,PoiView> _pois=new Dictionary<int,PoiView>();
        private readonly Material _material;private readonly MaterialPropertyBlock _properties=new MaterialPropertyBlock();
        internal NativeWorldRegion(){var shader=Resources.Load<Shader>("EchoSphere/NativeWorldRegion");if(shader==null||!shader.isSupported)throw new InvalidOperationException("Native region shader unavailable");_material=new Material(shader){name="Native region geometry",hideFlags=HideFlags.DontSave};}
        private static Color Alpha(Color color,float alpha){color.a*=alpha;return color;}
        private static Vector2 Offset(double angle,double radius)=>new Vector2((float)(Math.Cos(angle)*radius),(float)(Math.Sin(angle)*radius));
        internal void Draw(SourceCanvasSurface surface,State state)
        {
            _pockets.Clear();double seam=state.Time*.65;
            foreach(var pocket in state.Pockets){bool active=pocket.Id==state.Active;float width=active?1.8f:1;
                _pockets.Circle(pocket.Center,pocket.Radius,width,Alpha(pocket.Accent,active?.13f:.035f));
                for(int i=0;i<6;i++){double a=i*Math.PI/3+seam*(i%2==0?.08:-.06)+pocket.Center.x*.0001;double span=.10+(active?.045:.02);_pockets.Circle(pocket.Center,pocket.Radius-4+i%2*2,width,Alpha(pocket.Accent,active?.16f:.045f),(float)a,(float)(a+span),true);}
                for(int i=0;i<8;i++){double a=i*Math.PI/4+seam*.08;var p=pocket.Center+Offset(a,pocket.Radius);_pockets.Triangle(p,p-Offset(a-.16,7),p-Offset(a+.16,7),Alpha(pocket.Accent,active?.24f:.055f));}
            }
            _pockets.Upload();surface.DrawNativeMesh(_pockets.Mesh,_material,_properties);var live=new HashSet<int>();
            foreach(var poi in state.Pois){live.Add(poi.Id);if(!_pois.TryGetValue(poi.Id,out var view)){view=new PoiView();_pois.Add(poi.Id,view);}view.Geometry.Clear();float pulse=(float)(38+Math.Sin(state.Time*4+poi.Id)*4);view.Geometry.Circle(poi.Position,pulse,-1,Alpha(poi.Color,.18f));view.Geometry.Diamond(poi.Position,Alpha(poi.Color,.9f));view.Geometry.Upload();surface.DrawNativeMesh(view.Geometry.Mesh,_material,_properties);view.Label.Draw(surface,poi.Label,poi.Position+new Vector2(0,-16),Alpha(poi.Color,.86f));}
            var removed=new List<int>();foreach(var pair in _pois)if(!live.Contains(pair.Key)){pair.Value.Dispose();removed.Add(pair.Key);}foreach(int id in removed)_pois.Remove(id);
        }
        public void Dispose(){_pockets.Dispose();foreach(var view in _pois.Values)view.Dispose();_pois.Clear();UnityEngine.Object.Destroy(_material);}
    }
}
