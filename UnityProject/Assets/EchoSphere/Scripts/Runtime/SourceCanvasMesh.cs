using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    internal sealed class SourceCanvasMesh
    {
        public readonly List<Vector3> Vertices = new List<Vector3>();
        public readonly List<Color> Colors = new List<Color>();
        public readonly List<int> Triangles = new List<int>();
        public Func<Vector2, Vector2> Pose = point => point;
        public void Clear() { Vertices.Clear(); Colors.Clear(); Triangles.Clear(); Pose = point => point; }
        private int V(Vector2 source, Color color)
        { var point = Pose(source) * SpherePlacementRules.SourcePixelsToWorld; var index = Vertices.Count; Vertices.Add(new Vector3(point.x, -point.y, 0f)); Colors.Add(color); return index; }
        private void T(int a, int b, int c) { Triangles.Add(a); Triangles.Add(b); Triangles.Add(c); }
        public void Line(Vector2 a, Vector2 b, Color color, float width)
        {
            var direction = b - a; if (direction.sqrMagnitude == 0f) return;
            var normal = new Vector2(-direction.y, direction.x).normalized * width * .5f;
            var first = V(a-normal,color); V(a+normal,color); V(b+normal,color); V(b-normal,color); T(first,first+1,first+2); T(first,first+2,first+3);
        }
        public void Polygon(Vector2[] points, Color fill, Color outline, float width)
        {
            var center = Vector2.zero; foreach (var point in points) center += point; center /= points.Length;
            var start = V(center, fill); foreach (var point in points) V(point, fill);
            for (var i = 0; i < points.Length; i++) { T(start,start+1+i,start+1+(i+1)%points.Length); if (width > 0f) Line(points[i],points[(i+1)%points.Length],outline,width); }
        }
        public void Ellipse(Vector2 center, float rx, float ry, Color color, int segments = 40)
        {
            var start = V(center,color);
            for (var i = 0; i < segments; i++) { var a=i*Mathf.PI*2f/segments; V(center+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry),color); }
            for(var i=0;i<segments;i++) T(start,start+1+i,start+1+(i+1)%segments);
        }
        public void Arc(Vector2 center, float radius, float from, float to, Color color, float width)
        {
            var count = Mathf.Max(4, Mathf.CeilToInt(Mathf.Abs(to-from)*16f));
            var last = center+new Vector2(Mathf.Cos(from),Mathf.Sin(from))*radius;
            for(var i=1;i<=count;i++) { var a=Mathf.Lerp(from,to,i/(float)count); var next=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius; Line(last,next,color,width); last=next; }
        }
        public void RadialCore(Color tint)
        {
            const int segments=40, rings=10; var start=Vertices.Count;
            for(var ring=0;ring<=rings;ring++) for(var i=0;i<segments;i++)
            {
                var a=i*Mathf.PI*2f/segments; var point=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*8f*ring/rings;
                // Canvas gradient starts at (-.18r,-.20r,1) and ends at (0,0,.55r).
                var offset = point - new Vector2(-2.88f,-3.2f); var shift=new Vector2(2.88f,3.2f);
                var aa=shift.sqrMagnitude-7.8f*7.8f; var bb=-2f*Vector2.Dot(offset,shift)-15.6f; var cc=offset.sqrMagnitude-1f;
                var disc=Mathf.Max(0f,bb*bb-4f*aa*cc); var t=Mathf.Clamp01((-bb-Mathf.Sqrt(disc))/(2f*aa));
                var color=t<=.28f?Color.Lerp(Color.white,tint,t/.28f):t<=.68f?Color.Lerp(tint,new Color(tint.r,tint.g,tint.b,.62f),(t-.28f)/.4f):Color.Lerp(new Color(tint.r,tint.g,tint.b,.62f),Color.clear,(t-.68f)/.32f);
                V(point,color);
            }
            for(var ring=0;ring<rings;ring++) for(var i=0;i<segments;i++) { var next=(i+1)%segments; var a=start+ring*segments+i; var b=start+ring*segments+next; var c=start+(ring+1)*segments+next; var d=start+(ring+1)*segments+i; T(a,b,c); T(a,c,d); }
        }
        public void Upload(Mesh mesh) { mesh.Clear(); mesh.SetVertices(Vertices); mesh.SetColors(Colors); mesh.SetTriangles(Triangles,0); mesh.RecalculateBounds(); }
    }
}
