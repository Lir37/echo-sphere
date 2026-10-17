using System;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // Exact, bounded cache for the solid geometry used by SourceCanvasSurface.
    // Returned geometry is owned by this cache and must only be read.
    internal sealed class SourceCanvasGeometryCache
    {
        private sealed class Entry
        {
            public int Hash;
            public int[] Key;
            public List<VectorUtils.Geometry> Geometry;
            public int Vertices, Indices;
            public LinkedListNode<Entry> Age;
        }
        private readonly Dictionary<int,List<Entry>> _buckets=new Dictionary<int,List<Entry>>();
        private readonly LinkedList<Entry> _age=new LinkedList<Entry>();
        private readonly List<int> _key=new List<int>();
        private readonly int _maxEntries,_maxVertices,_maxIndices,_maxKeyWords;
        private int _vertices,_indices,_keyWords;
        public long Hits { get; private set; }
        public long Misses { get; private set; }
        public int Count=>_age.Count;
        public int RetainedVertices=>_vertices;
        public int RetainedIndices=>_indices;
        public SourceCanvasGeometryCache(int maxEntries=512,int maxVertices=262144,int maxIndices=786432,int maxKeyWords=262144)
        {if(maxEntries<1||maxVertices<1||maxIndices<1||maxKeyWords<1)throw new ArgumentOutOfRangeException();_maxEntries=maxEntries;_maxVertices=maxVertices;_maxIndices=maxIndices;_maxKeyWords=maxKeyWords;}
        public void Clear(){_buckets.Clear();_age.Clear();_vertices=0;_indices=0;_keyWords=0;}
        private void Number(float value)=>_key.Add(BitConverter.SingleToInt32Bits(value));
        private void Point(Vector2 value){Number(value.x);Number(value.y);}
        private void ColorKey(Color value){Number(value.r);Number(value.g);Number(value.b);Number(value.a);}
        private void Matrix(Matrix2D value){Number(value.m00);Number(value.m01);Number(value.m02);Number(value.m10);Number(value.m11);Number(value.m12);}
        private bool FillKey(IFill fill)
        {
            if(fill==null){_key.Add(0);return true;}
            if(!(fill is SolidFill solid))return false;
            _key.Add(1);ColorKey(solid.Color);Number(solid.Opacity);_key.Add((int)solid.Mode);return true;
        }
        private bool BuildKey(Shape shape,VectorUtils.TessellationOptions options)
        {
            _key.Clear();Number(options.StepDistance);Number(options.MaxCordDeviation);Number(options.MaxTanAngleDeviation);Number(options.SamplingStepSize);
            _key.Add(shape.IsConvex?1:0);Matrix(shape.FillTransform);if(!FillKey(shape.Fill))return false;
            var props=shape.PathProps;_key.Add((int)props.Head);_key.Add((int)props.Tail);_key.Add((int)props.Corners);
            var stroke=props.Stroke;_key.Add(stroke==null?0:1);
            if(stroke!=null){ColorKey(stroke.Color);Matrix(stroke.FillTransform);if(!FillKey(stroke.Fill))return false;Number(stroke.HalfThickness);Number(stroke.PatternOffset);Number(stroke.TippedCornerLimit);_key.Add(stroke.Pattern==null?-1:stroke.Pattern.Length);if(stroke.Pattern!=null)foreach(var n in stroke.Pattern)Number(n);}
            _key.Add(shape.Contours==null?-1:shape.Contours.Length);
            if(shape.Contours!=null)foreach(var contour in shape.Contours){_key.Add(contour.Closed?1:0);_key.Add(contour.Segments==null?-1:contour.Segments.Length);if(contour.Segments!=null)foreach(var segment in contour.Segments){Point(segment.P0);Point(segment.P1);Point(segment.P2);}}
            return true;
        }
        private static IFill CopyFill(IFill fill)=>fill==null?null:new SolidFill {Color=((SolidFill)fill).Color,Opacity=((SolidFill)fill).Opacity,Mode=((SolidFill)fill).Mode};
        private static Shape Snapshot(Shape source)
        {
            var contours=source.Contours==null?null:new BezierContour[source.Contours.Length];
            if(contours!=null)for(int i=0;i<contours.Length;i++){var c=source.Contours[i];contours[i]=new BezierContour {Closed=c.Closed,Segments=c.Segments==null?null:(BezierPathSegment[])c.Segments.Clone()};}
            var props=new PathProperties {Head=source.PathProps.Head,Tail=source.PathProps.Tail,Corners=source.PathProps.Corners};var s=source.PathProps.Stroke;
            if(s!=null)props.Stroke=new Stroke {Color=s.Color,Fill=CopyFill(s.Fill),FillTransform=s.FillTransform,HalfThickness=s.HalfThickness,Pattern=s.Pattern==null?null:(float[])s.Pattern.Clone(),PatternOffset=s.PatternOffset,TippedCornerLimit=s.TippedCornerLimit};
            return new Shape {Contours=contours,Fill=CopyFill(source.Fill),FillTransform=source.FillTransform,IsConvex=source.IsConvex,PathProps=props};
        }
        private static List<VectorUtils.Geometry> Tessellate(Shape shape,VectorUtils.TessellationOptions options)=>VectorUtils.TessellateScene(new Scene {Root=new SceneNode {Shapes=new List<Shape>{shape}}},options);
        public List<VectorUtils.Geometry> GetOrCreate(Shape shape,VectorUtils.TessellationOptions options)
        {
            if(shape==null)throw new ArgumentNullException(nameof(shape));
            if(!BuildKey(shape,options)){Misses++;return Tessellate(shape,options);}
            int hash=17;unchecked{foreach(var word in _key)hash=hash*31+word;}
            if(_buckets.TryGetValue(hash,out var bucket))foreach(var candidate in bucket){if(candidate.Key.Length!=_key.Count)continue;int i=0;while(i<_key.Count&&candidate.Key[i]==_key[i])i++;if(i!=_key.Count)continue;Hits++;_age.Remove(candidate.Age);_age.AddLast(candidate.Age);return candidate.Geometry;}
            Misses++;var geometry=Tessellate(Snapshot(shape),options);int vertices=0,indices=0;
            foreach(var g in geometry){vertices+=g.Vertices.Length;indices+=g.Indices.Length;}
            if(vertices>_maxVertices||indices>_maxIndices||_key.Count>_maxKeyWords)return geometry;
            while(_age.Count>=_maxEntries||_vertices+vertices>_maxVertices||_indices+indices>_maxIndices||_keyWords+_key.Count>_maxKeyWords){var old=_age.First.Value;_age.RemoveFirst();var oldBucket=_buckets[old.Hash];oldBucket.Remove(old);if(oldBucket.Count==0)_buckets.Remove(old.Hash);_vertices-=old.Vertices;_indices-=old.Indices;_keyWords-=old.Key.Length;}
            var entry=new Entry {Hash=hash,Key=_key.ToArray(),Geometry=geometry,Vertices=vertices,Indices=indices};entry.Age=_age.AddLast(entry);
            if(!_buckets.TryGetValue(hash,out bucket)){bucket=new List<Entry>();_buckets.Add(hash,bucket);}bucket.Add(entry);_vertices+=vertices;_indices+=indices;_keyWords+=entry.Key.Length;return geometry;
        }
    }
}
