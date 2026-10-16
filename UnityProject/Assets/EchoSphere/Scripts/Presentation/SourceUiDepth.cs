using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Profiling;

namespace EchoSphere.Presentation
{
    // CSS preserve-3d paints intersecting element planes in depth order. Splitting
    // planes (rather than sorting element centers) preserves the front/back arcs
    // of the authored menu rings and their transparent glows.
    internal sealed class SourceUiDepth : IDisposable
    {
        private static readonly ProfilerMarker DepthRenderMarker=new ProfilerMarker("EchoSphere.UI.DepthMeshAndRender");
        internal sealed class PlaneImage {public Texture Texture,Clip;public Rect Rect,ClipRect;public Matrix4x4 Matrix;public float Opacity;public string Blend;}
        private struct Point {public Vector3 Position;public Vector2 Uv;}
        private sealed class Polygon {public List<Point> Points;public PlaneImage Image;}
        private sealed class Branch {public Vector3 Normal,Position;public readonly List<Polygon> Coplanar=new List<Polygon>();public Branch Front,Back;}
        private readonly Material _material;
        private readonly CommandBuffer _commands=new CommandBuffer {name="Source CSS preserve-3d"};
        private readonly List<Mesh> _meshes=new List<Mesh>();
        // Buffers for Unity Mesh.SetVertices/SetUVs/SetTriangles: these native
        // setters copy the data; there is no reason to allocate per polygon.
        private readonly List<Vector3> _meshVertices=new List<Vector3>();
        private readonly List<Vector2> _meshUvs=new List<Vector2>();
        private readonly List<int> _meshTriangles=new List<int>();
        private readonly List<MaterialPropertyBlock> _meshProperties=new List<MaterialPropertyBlock>();
        private readonly List<Polygon> _paintOrder=new List<Polygon>();
        private readonly List<Polygon> _ordered=new List<Polygon>();
        private RenderTexture _premultiplied,_result;
        internal Rect Bounds;
        internal SourceUiDepth(){_material=new Material(Resources.Load<Shader>("EchoSphere/SourceUiDepth"));}
        internal static Matrix4x4 Transform(VisualCss sheet,VisualCssNode node)
        {
            var values=VisualCssTransform.Parse(sheet,node,origin:true);var matrix=new Matrix4x4();
            for(var column=0;column<4;column++)for(var row=0;row<4;row++)matrix[row,column]=(float)values[column*4+row];
            return matrix;
        }

        private static void Add(ref Branch branch,Polygon polygon)
        {
            if(polygon.Points.Count<3)return;
            if(branch==null){var normal=Vector3.Cross(polygon.Points[1].Position-polygon.Points[0].Position,polygon.Points[2].Position-polygon.Points[0].Position);if(normal.sqrMagnitude<.0000001f)return;branch=new Branch {Normal=normal.normalized,Position=polygon.Points[0].Position};branch.Coplanar.Add(polygon);return;}
            var front=new List<Point>();var back=new List<Point>();var hasFront=false;var hasBack=false;
            for(var index=0;index<polygon.Points.Count;index++){
                var a=polygon.Points[index];var b=polygon.Points[(index+1)%polygon.Points.Count];var da=Vector3.Dot(branch.Normal,a.Position-branch.Position);var db=Vector3.Dot(branch.Normal,b.Position-branch.Position);
                if(da>=-.0001f)front.Add(a);if(da<=.0001f)back.Add(a);hasFront|=da>.0001f;hasBack|=da<-.0001f;
                if(da>.0001f&&db<-.0001f||da<-.0001f&&db>.0001f){var fraction=da/(da-db);var point=new Point {Position=Vector3.LerpUnclamped(a.Position,b.Position,fraction),Uv=Vector2.LerpUnclamped(a.Uv,b.Uv,fraction)};front.Add(point);back.Add(point);}
            }
            if(!hasFront&&!hasBack)branch.Coplanar.Add(polygon);else {if(hasFront)Add(ref branch.Front,new Polygon {Points=front,Image=polygon.Image});if(hasBack)Add(ref branch.Back,new Polygon {Points=back,Image=polygon.Image});}
        }
        private static void Traverse(Branch branch,Vector3 eye,List<Polygon> ordered)
        {if(branch==null)return;var positive=Vector3.Dot(branch.Normal,eye-branch.Position)>=0f;Traverse(positive?branch.Back:branch.Front,eye,ordered);ordered.AddRange(branch.Coplanar);Traverse(positive?branch.Front:branch.Back,eye,ordered);}
        internal Texture Render(List<PlaneImage> images,float width,float height,float scale,float perspective,Vector2? eyeCenter=null,bool preserveDepth=true)
        {
            using var depthSample=DepthRenderMarker.Auto();
            Branch tree=null;var paintOrder=_paintOrder;paintOrder.Clear();var min=new Vector2(float.PositiveInfinity,float.PositiveInfinity);var max=new Vector2(float.NegativeInfinity,float.NegativeInfinity);var center=eyeCenter??new Vector2(width*.5f,height*.5f);
            foreach(var image in images){var r=image.Rect;var positions=new[]{new Vector2(r.xMin,r.yMin),new Vector2(r.xMax,r.yMin),new Vector2(r.xMax,r.yMax),new Vector2(r.xMin,r.yMax)};var points=new List<Point>();var uvs=new[]{new Vector2(0,1),new Vector2(1,1),new Vector2(1,0),new Vector2(0,0)};
                for(var index=0;index<4;index++){var p=image.Matrix.MultiplyPoint3x4(positions[index]);points.Add(new Point {Position=p,Uv=uvs[index]});var w=perspective>0f?1f-p.z/perspective:1f;var projected=center+((Vector2)p-center)/Mathf.Max(.00001f,w);min=Vector2.Min(min,projected);max=Vector2.Max(max,projected);}var polygon=new Polygon {Points=points,Image=image};paintOrder.Add(polygon);Add(ref tree,polygon);}
            if(tree==null)return null;Bounds=Rect.MinMaxRect(Mathf.Floor(min.x*scale)/scale-1f,Mathf.Floor(min.y*scale)/scale-1f,Mathf.Ceil(max.x*scale)/scale+1f,Mathf.Ceil(max.y*scale)/scale+1f);
            var wPixels=Math.Max(1,Mathf.CeilToInt(Bounds.width*scale));var hPixels=Math.Max(1,Mathf.CeilToInt(Bounds.height*scale));
            if(_result==null||_result.width!=wPixels||_result.height!=hPixels){ReleaseTargets();_premultiplied=new RenderTexture(wPixels,hPixels,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear){antiAliasing=4,filterMode=FilterMode.Bilinear};_premultiplied.Create();_result=new RenderTexture(wPixels,hPixels,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){filterMode=FilterMode.Bilinear};_result.Create();}
            // CSS grouping properties (including the source core's filter)
            // force the used transform-style to flat. They retain paint order
            // even when the authored declaration says preserve-3d.
            var ordered=_ordered;ordered.Clear();if(preserveDepth)Traverse(tree,new Vector3(center.x,center.y,perspective>0f?perspective:1000000f),ordered);else ordered.AddRange(paintOrder);
            _commands.Clear();_commands.SetRenderTarget(_premultiplied);_commands.ClearRenderTarget(false,true,Color.clear);
            var meshIndex=0;foreach(var polygon in ordered){
                if(meshIndex>=_meshes.Count){_meshes.Add(new Mesh {name="Source CSS split plane"});_meshProperties.Add(new MaterialPropertyBlock());}
                var mesh=_meshes[meshIndex];var properties=_meshProperties[meshIndex];meshIndex++;
                _meshVertices.Clear();_meshUvs.Clear();_meshTriangles.Clear();
                foreach(var point in polygon.Points){_meshVertices.Add(point.Position);_meshUvs.Add(point.Uv);}
                for(var i=1;i<polygon.Points.Count-1;i++){_meshTriangles.Add(0);_meshTriangles.Add(i);_meshTriangles.Add(i+1);}
                mesh.Clear();mesh.SetVertices(_meshVertices);mesh.SetUVs(0,_meshUvs);mesh.SetTriangles(_meshTriangles,0);
                properties.Clear();properties.SetTexture("_MainTex",polygon.Image.Texture);properties.SetFloat("_Opacity",polygon.Image.Opacity);properties.SetVector("_Bounds",new Vector4(Bounds.x,Bounds.y,Bounds.width,Bounds.height));properties.SetVector("_Eye",new Vector4(center.x,center.y,perspective>0f?perspective:100000000f,0));
                properties.SetTexture("_ClipMask",polygon.Image.Clip??Texture2D.whiteTexture);properties.SetFloat("_ClipEnabled",polygon.Image.Clip!=null?1f:0f);var clip=polygon.Image.ClipRect;properties.SetVector("_ClipRect",new Vector4(clip.x,clip.y,Math.Max(.00001f,clip.width),Math.Max(.00001f,clip.height)));
                _commands.DrawMesh(mesh,Matrix4x4.identity,_material,0,polygon.Image.Blend=="screen"?2:0,properties);}
            var previous=RenderTexture.active;var gamma=GL.sRGBWrite;
            try{_commands.Blit(_premultiplied,_result,_material,1);_commands.SetRenderTarget(previous!=null?new RenderTargetIdentifier(previous):new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));Graphics.ExecuteCommandBuffer(_commands);}
            finally{RenderTexture.active=previous;GL.sRGBWrite=gamma;}return _result;
        }
        private void ReleaseTargets(){if(_premultiplied!=null){_premultiplied.Release();UnityEngine.Object.Destroy(_premultiplied);}if(_result!=null){_result.Release();UnityEngine.Object.Destroy(_result);}}
        public void Dispose(){ReleaseTargets();foreach(var mesh in _meshes)UnityEngine.Object.Destroy(mesh);UnityEngine.Object.Destroy(_material);_commands.Dispose();}
    }
}
