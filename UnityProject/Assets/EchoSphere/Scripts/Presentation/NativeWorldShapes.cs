using System;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // Typed authored geometry using Unity Vector Graphics. Static art is retained;
    // animated paths can upload into an existing Unity mesh.
    internal static class NativeWorldShapes
    {
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
        internal static Mesh Tessellate(BezierContour contour,float width,Mesh target=null,bool roundCaps=false,PathCorner corners=PathCorner.Tipped,float miter=10,PathEnding? cap=null,BezierContour[] contours=null)
        {
            var ending=cap??(roundCaps?PathEnding.Round:PathEnding.Chop);var shape=new Shape {Contours=contours??new[]{contour}};if(width>0)shape.PathProps=new PathProperties {Head=ending,Tail=ending,Corners=corners,Stroke=new Stroke {Color=Color.white,HalfThickness=width*.5f,Pattern=Array.Empty<float>(),TippedCornerLimit=miter}};else shape.Fill=new SolidFill {Color=Color.white,Mode=FillMode.NonZero};
            var options=new VectorUtils.TessellationOptions {StepDistance=float.MaxValue,MaxCordDeviation=.02f,MaxTanAngleDeviation=.08f,SamplingStepSize=.01f};var geometry=VectorUtils.TessellateScene(new Scene {Root=new SceneNode {Shapes=new List<Shape>{shape}}},options);var vertices=new List<Vector3>();var triangles=new List<int>();foreach(var g in geometry){int first=vertices.Count;foreach(var p in g.Vertices)vertices.Add(g.WorldTransform*p);foreach(var i in g.Indices)triangles.Add(first+i);}var mesh=target??new Mesh {name="Native world art"};mesh.Clear();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();return mesh;
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
