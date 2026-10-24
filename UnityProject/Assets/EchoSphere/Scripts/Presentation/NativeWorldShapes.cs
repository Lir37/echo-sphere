using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Profiling;
using Unity.VectorGraphics;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // Typed authored geometry using Unity Vector Graphics. Static art is retained;
    // animated paths can upload into an existing Unity mesh.
    internal static class NativeWorldShapes
    {
        private static readonly ConditionalWeakTable<Mesh,TessellationInput> RetainedInputs=new ConditionalWeakTable<Mesh,TessellationInput>();
        private static ulong _geometryVersion;
        internal static ulong GeometryVersion(Mesh mesh)=>RetainedInputs.TryGetValue(mesh,out var input)?input.Version:0;
        // Uploads are synchronous on Unity's main thread. One shared high-water
        // buffer avoids retaining a duplicate CPU vertex/index copy per mesh.
        private static readonly List<Vector3> UploadVertices=new List<Vector3>();
        private static readonly List<int> UploadTriangles=new List<int>();
        private static readonly ProfilerMarker TessellationMarker=new ProfilerMarker("EchoSphere.World.Tessellate");
        // Owners already retain these meshes. Keep an exact immutable input snapshot
        // so unchanged paths need no Vector Graphics tessellation or mesh upload.
        // Weak keys do not extend the lifetime of discarded entity mesh owners.
        private sealed class TessellationInput
        {
            internal ulong Version;
            private BezierContour[] _paths=Array.Empty<BezierContour>();
            private float _width,_miter,_offset;
            private PathEnding _ending;private PathCorner _corner;
            private float[] _dash=Array.Empty<float>();
            internal void Capture(BezierContour contour,BezierContour[] contours,float width,PathEnding ending,PathCorner corner,float miter,float[] dash,float offset)
            {
                int count=contours?.Length??1;if(_paths.Length!=count)_paths=new BezierContour[count];
                for(int i=0;i<count;i++){
                    var path=contours==null?contour:contours[i];var points=_paths[i].Segments;
                    if(points==null||points.Length!=path.Segments.Length)points=new BezierPathSegment[path.Segments.Length];
                    Array.Copy(path.Segments,points,points.Length);_paths[i]=new BezierContour {Closed=path.Closed,Segments=points};
                }
                _width=width;_ending=ending;_corner=corner;_miter=miter;_offset=offset;
                int dashCount=dash?.Length??0;if(_dash.Length!=dashCount)_dash=new float[dashCount];if(dashCount>0)Array.Copy(dash,_dash,dashCount);
                Version=++_geometryVersion;
            }
            internal bool Matches(BezierContour contour,BezierContour[] contours,float width,PathEnding ending,PathCorner corner,float miter,float[] dash,float offset)
            {
                if(!_width.Equals(width)||_ending!=ending||_corner!=corner||!_miter.Equals(miter)||!_offset.Equals(offset)||_dash.Length!=(dash?.Length??0)||_paths.Length!=(contours?.Length??1))return false;
                for(int i=0;i<_dash.Length;i++)if(!_dash[i].Equals(dash[i]))return false;
                for(int i=0;i<_paths.Length;i++){
                    var a=_paths[i];var b=contours==null?contour:contours[i];
                    if(a.Closed!=b.Closed||a.Segments.Length!=b.Segments.Length)return false;
                    for(int j=0;j<a.Segments.Length;j++){var x=a.Segments[j];var y=b.Segments[j];if(!x.P0.Equals(y.P0)||!x.P1.Equals(y.P1)||!x.P2.Equals(y.P2))return false;}
                }
                return true;
            }
        }
        internal static BezierContour EllipseContour(Vector2 center,float rx,float ry,float start=0,float end=Mathf.PI*2)
        {
            float delta=end-start,tau=Mathf.PI*2;delta=delta>=tau?tau:Mathf.Repeat(delta,tau);int count=Mathf.Max(1,Mathf.CeilToInt(delta/(Mathf.PI*.25f)));float step=delta/count;var segments=new BezierPathSegment[count+1];
            Vector2 Point(float a)=>center+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry);Vector2 Tangent(float a)=>new Vector2(-Mathf.Sin(a)*rx,Mathf.Cos(a)*ry);
            for(int i=0;i<count;i++){float a=start+i*step,b=a+step,k=4f/3*Mathf.Tan(step*.25f);segments[i]=new BezierPathSegment {P0=Point(a),P1=Point(a)+Tangent(a)*k,P2=Point(b)-Tangent(b)*k};}var last=Point(end);segments[count]=new BezierPathSegment {P0=last,P1=last,P2=last};return new BezierContour {Segments=segments,Closed=false};
        }
        internal static BezierContour QuadraticContour(Vector2 start,bool closed,params Vector2[] controlsAndEnds)
        {
            int count=controlsAndEnds.Length/2;var segments=new BezierPathSegment[count+1];var previous=start;
            for(int i=0;i<count;i++){var c=controlsAndEnds[i*2];var end=controlsAndEnds[i*2+1];segments[i]=new BezierPathSegment {P0=previous,P1=previous+(c-previous)*(2f/3),P2=end+(c-end)*(2f/3)};previous=end;}
            segments[count]=new BezierPathSegment {P0=previous,P1=previous,P2=previous};if(closed){if((previous-start).sqrMagnitude<.00000001f)Array.Resize(ref segments,count);else{segments[count].P1=Vector2.Lerp(previous,start,1f/3);segments[count].P2=Vector2.Lerp(previous,start,2f/3);}}
            return new BezierContour {Segments=segments,Closed=closed};
        }
        internal static BezierContour PolylineContour(bool closed,params Vector2[] points)
        {
            var segments=new BezierPathSegment[points.Length];for(int i=0;i<points.Length;i++){var a=points[i];var b=points[closed?(i+1)%points.Length:Math.Min(i+1,points.Length-1)];segments[i]=new BezierPathSegment {P0=a,P1=Vector2.Lerp(a,b,1f/3),P2=Vector2.Lerp(a,b,2f/3)};}return new BezierContour {Segments=segments,Closed=closed};
        }
        internal static Mesh Rectangle(float x,float y,float w,float h)=>Polygon(new[]{new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h)});
        internal static Mesh Polygon(Vector2[] points,float width=0,PathCorner corners=PathCorner.Tipped,float miter=10)
        {
            var segments=new BezierPathSegment[points.Length];for(int i=0;i<points.Length;i++){var a=points[i];var b=points[(i+1)%points.Length];segments[i]=new BezierPathSegment {P0=a,P1=Vector2.Lerp(a,b,1f/3),P2=Vector2.Lerp(a,b,2f/3)};}
            return Tessellate(new BezierContour {Segments=segments,Closed=true},width,corners:corners,miter:miter);
        }
        internal static Mesh Circle(Vector2 center,float radius)=>Ellipse(center,radius,radius);
        internal static Mesh Ellipse(Vector2 center,float rx,float ry)
        {
            var segments=new BezierPathSegment[9];float step=Mathf.PI*.25f,k=4f/3*Mathf.Tan(step*.25f);
            Vector2 Point(float a)=>center+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry);Vector2 Tangent(float a)=>new Vector2(-Mathf.Sin(a)*rx,Mathf.Cos(a)*ry);
            for(int i=0;i<8;i++){float a=i*step,b=a+step;segments[i]=new BezierPathSegment {P0=Point(a),P1=Point(a)+Tangent(a)*k,P2=Point(b)-Tangent(b)*k};}segments[8]=new BezierPathSegment {P0=Point(Mathf.PI*2),P1=Point(Mathf.PI*2),P2=Point(Mathf.PI*2)};return Tessellate(new BezierContour {Segments=segments,Closed=false},0);
        }
        internal static Mesh Tessellate(BezierContour contour,float width,Mesh target=null,bool roundCaps=false,PathCorner corners=PathCorner.Tipped,float miter=10,PathEnding? cap=null,BezierContour[] contours=null,float[] dash=null,float dashOffset=0)
        {
            var ending=cap??(roundCaps?PathEnding.Round:PathEnding.Chop);
            TessellationInput previous=null;
            if(target!=null&&RetainedInputs.TryGetValue(target,out previous)&&previous.Matches(contour,contours,width,ending,corners,miter,dash,dashOffset))return target;
            using var tessellation=TessellationMarker.Auto();var retained=previous??new TessellationInput();
            var shape=new Shape {Contours=contours??new[]{contour}};if(width>0)shape.PathProps=new PathProperties {Head=ending,Tail=ending,Corners=corners,Stroke=new Stroke {Color=Color.white,HalfThickness=width*.5f,Pattern=dash??Array.Empty<float>(),PatternOffset=dashOffset,TippedCornerLimit=miter}};else shape.Fill=new SolidFill {Color=Color.white,Mode=FillMode.NonZero};
            var options=new VectorUtils.TessellationOptions {StepDistance=float.MaxValue,MaxCordDeviation=.02f,MaxTanAngleDeviation=.08f,SamplingStepSize=.01f};var geometry=VectorUtils.TessellateScene(new Scene {Root=new SceneNode {Shapes=new List<Shape>{shape}}},options);var vertices=UploadVertices;var triangles=UploadTriangles;vertices.Clear();triangles.Clear();foreach(var g in geometry){int first=vertices.Count;foreach(var p in g.Vertices)vertices.Add(g.WorldTransform*p);foreach(var i in g.Indices)triangles.Add(first+i);}var mesh=target??new Mesh {name="Native world art"};mesh.Clear();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();retained.Capture(contour,contours,width,ending,corners,miter,dash,dashOffset);if(previous==null)RetainedInputs.Add(mesh,retained);return mesh;
        }
        internal static Mesh Line(Vector2 a,Vector2 b,float width,bool roundCaps=false,PathEnding? cap=null)
        {
            var segments=new[]{new BezierPathSegment {P0=a,P1=Vector2.Lerp(a,b,1f/3),P2=Vector2.Lerp(a,b,2f/3)},new BezierPathSegment {P0=b,P1=b,P2=b}};
            return Tessellate(new BezierContour {Segments=segments,Closed=false},width,roundCaps:roundCaps,cap:cap);
        }
        internal static Mesh Arc(float radius,float sweep,float width,PathEnding? cap=null,PathCorner corners=PathCorner.Tipped,float miter=10)
        {
            int count=Mathf.Max(1,Mathf.CeilToInt(sweep/(Mathf.PI*.25f)));float step=sweep/count;var segments=new BezierPathSegment[count+1];
            for(int i=0;i<count;i++){float a=i*step,b=a+step,k=4f/3*Mathf.Tan(step/4);var p=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;var q=new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius;segments[i]=new BezierPathSegment {P0=p,P1=p+new Vector2(-Mathf.Sin(a),Mathf.Cos(a))*radius*k,P2=q-new Vector2(-Mathf.Sin(b),Mathf.Cos(b))*radius*k};}
            var end=new Vector2(Mathf.Cos(sweep),Mathf.Sin(sweep))*radius;segments[count]=new BezierPathSegment {P0=end,P1=end,P2=end};return Tessellate(new BezierContour {Segments=segments,Closed=false},width,cap:cap,corners:corners,miter:miter);
        }
        internal static void StrokePolyline(Vector2[] points,float width,Mesh target,PathCorner corners,float miter)
        {
            var segments=new BezierPathSegment[points.Length];for(int i=0;i<points.Length;i++){var a=points[i];var b=points[Math.Min(i+1,points.Length-1)];segments[i]=new BezierPathSegment {P0=a,P1=Vector2.Lerp(a,b,1f/3),P2=Vector2.Lerp(a,b,2f/3)};}
            Tessellate(new BezierContour {Segments=segments,Closed=false},width,target,true,corners,miter);
        }
    }
}
