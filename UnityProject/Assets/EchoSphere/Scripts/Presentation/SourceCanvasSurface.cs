using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EchoSphere.Core;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Profiling;

namespace EchoSphere.Presentation
{
    // Native Canvas 2D path sink. Keeps source-space paths, Bezier control points,
    // fill rules, dashes, caps, joins, gradient stops and source-over/lighter order.
    public sealed class SourceCanvasSurface : MonoBehaviour, IVisualCanvasSink
    {
        private static readonly ProfilerMarker CanvasEndMarker=new ProfilerMarker("EchoSphere.Canvas.EndFrame");
        private static readonly ProfilerMarker CanvasTextMarker=new ProfilerMarker("EchoSphere.Canvas.PrepareText");
        private static readonly ProfilerMarker CanvasMeshMarker=new ProfilerMarker("EchoSphere.Canvas.MeshUpload");
        private static readonly ProfilerMarker CanvasGradientMarker=new ProfilerMarker("EchoSphere.Canvas.GradientUpload");
        private static readonly ProfilerMarker CanvasDrawMarker=new ProfilerMarker("EchoSphere.Canvas.DrawBatches");
        private static readonly ProfilerMarker CanvasPresentMarker=new ProfilerMarker("EchoSphere.Canvas.Present");
        private static readonly int ShadowMaskId=Shader.PropertyToID("_SourceShadowMask");
        private static readonly int ShadowPingId=Shader.PropertyToID("_SourceShadowPing");
        private static readonly int ShadowBlurId=Shader.PropertyToID("_SourceShadowBlur");
        private static readonly ProfilerMarker CanvasTessellationMarker=new ProfilerMarker("EchoSphere.Canvas.Tessellate");
        private static readonly ProfilerMarker CanvasShadowCommandsMarker=new ProfilerMarker("EchoSphere.Canvas.ShadowCommands");
        private static readonly ProfilerMarker CanvasExecuteMarker=new ProfilerMarker("EchoSphere.Canvas.ExecuteCommands");
        private static readonly int ImageTexId=Shader.PropertyToID("_ImageTex");
        private static readonly int ImageTexelSizeId=Shader.PropertyToID("_ImageTex_TexelSize");
        private static readonly int PaintsId=Shader.PropertyToID("_Paints");
        private static readonly int ShadowSigmaId=Shader.PropertyToID("_SourceShadowSigma");
        private static readonly int ShadowDirectionId=Shader.PropertyToID("_SourceShadowDirection");
        private static readonly int ShadowColorId=Shader.PropertyToID("_SourceShadowColor");
        private static readonly int ShadowOffsetId=Shader.PropertyToID("_SourceShadowOffset");
        private static readonly int ShadowRectId=Shader.PropertyToID("_SourceShadowRect");
        private sealed class State
        {
            public Matrix4x4 Matrix=Matrix4x4.identity;
            public V Fill="#000000",Stroke="#000000";
            public float Alpha=1f,Width=1f,DashOffset,Miter=10f;
            public string Blend="source-over",Cap="butt",Join="miter";
            public string Font="10px sans-serif",Align="start",Baseline="alphabetic";
            public float ShadowBlur,ShadowX,ShadowY;
            public Color Shadow=Color.clear;
            public float[] Dash=Array.Empty<float>();
            public State Copy()=>(State)MemberwiseClone();
        }
        private sealed class Contour
        {
            public readonly List<BezierPathSegment> Segments=new List<BezierPathSegment>();
            public bool Closed;
        }
        private readonly SourceCanvasGeometryCache _geometryCache=new SourceCanvasGeometryCache();
        private State _state=new State();
        private readonly Stack<State> _stack=new Stack<State>();
        private readonly List<Contour> _path=new List<Contour>();
        // Reusable save buffer for fillRect/strokeRect, which temporarily replace the current path.
        private readonly List<Contour> _rectSavedPath=new List<Contour>();
        private readonly List<float> _dashScratch=new List<float>();
        private Contour _contour;
        private readonly List<Vector3> _vertices=new List<Vector3>();
        private readonly List<Color> _colors=new List<Color>();
        private readonly List<Vector4> _uv=new List<Vector4>(),_data=new List<Vector4>();
        private readonly List<int> _triangles=new List<int>();
        private sealed class Batch { public int Start,Count;public Texture Texture;public string Blend;public float Blur,X,Y;public Color Shadow;public string Text;public float TextX,TextY,TextWidth;public State TextState; }
        private readonly List<Batch> _batches=new List<Batch>();
        private Texture _image;
        private Mesh _displayMesh;
        private Material _displayMaterial,_blurMaterial;
        private RenderTexture _target;
        private RenderTexture _uiTarget;
        public Texture UiTexture=>_uiTarget;
        private CommandBuffer _commands;
        private bool _initialized;
        private Rect _viewport;
        private float _dpr=1f;
        private readonly List<VisualGradient> _gradients=new List<VisualGradient>();
        // Stable authored stop order with reusable native Canvas gradient indices.
        private readonly List<(double Key,int Index)> _gradientStopOrder=new List<(double Key,int Index)>();
        private sealed class GradientSpace {public Matrix4x4 Inverse;}
        private readonly System.Runtime.CompilerServices.ConditionalWeakTable<VisualGradient,GradientSpace> _gradientSpaces=new System.Runtime.CompilerServices.ConditionalWeakTable<VisualGradient,GradientSpace>();
        private readonly Dictionary<VisualGradient,int> _gradientIndices=new Dictionary<VisualGradient,int>();
        private Mesh _mesh;
        private Material _material;
        private MaterialPropertyBlock _properties;
        private Texture2D _paintTexture;
        private Color[] _paintPixels;
        public VisualCanvas Canvas { get; private set; }
        public Vector2 SourceOrigin;
        public bool LocalizeCanvasText=true;
        private const int PaintWidth=36;
        private int _paintHeight=32;
        public void Initialize(int sortingOrder)
        {
            if(_initialized)return;
            _commands=new CommandBuffer {name="Authored source Canvas"};
            _properties=new MaterialPropertyBlock();
            _paintHeight=Mathf.Max(1,_paintHeight);
            Canvas=new VisualCanvas(this);_mesh=new Mesh {name="Source Canvas Paths",indexFormat=IndexFormat.UInt32};_mesh.MarkDynamic();
            _displayMesh=new Mesh {name="Source Canvas presentation"};gameObject.AddComponent<MeshFilter>().sharedMesh=_displayMesh;
            var renderer=gameObject.AddComponent<MeshRenderer>();renderer.sortingOrder=sortingOrder;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
            _material=new Material(Resources.Load<Shader>("EchoSphere/SourceCanvas"));
            _displayMaterial=new Material(Resources.Load<Shader>("EchoSphere/SourceCanvasPresent"));renderer.sharedMaterial=_displayMaterial;
            _blurMaterial=new Material(Resources.Load<Shader>("EchoSphere/SourceCanvasShadow"));
            ResizePaintTexture();
            _initialized=true;
        }
        public void SetViewport(float width,float height,float dpr)
        {
            if(!_initialized)return;
            width=Mathf.Max(1f,width);height=Mathf.Max(1f,height);
            _viewport=new Rect(-width*.5f*SpherePlacementRules.SourcePixelsToWorld,-height*.5f*SpherePlacementRules.SourcePixelsToWorld,width*SpherePlacementRules.SourcePixelsToWorld,height*SpherePlacementRules.SourcePixelsToWorld);
            // Canvas backing pixels have their own source clamp. CSS viewport
            // density may be below one on Android and must remain unchanged.
            var backing=Visual_renderScale.Scope["calculateBackingSize"].Call(width,height,dpr);
            _dpr=(float)backing["dpr"].Number;
            var w=(int)backing["width"].Number;var h=(int)backing["height"].Number;
            if(_target!=null&&_target.width==w&&_target.height==h)return;
            if(_target!=null){_target.Release();Destroy(_target);}
            if(_uiTarget!=null){_uiTarget.Release();Destroy(_uiTarget);}
            _target=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear){name="Source Canvas sRGB compositing",antiAliasing=4,filterMode=FilterMode.Bilinear};_target.Create();
            _uiTarget=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name="Source Canvas UI image",filterMode=FilterMode.Bilinear};_uiTarget.Create();
            _displayMaterial.mainTexture=_target;
            _displayMesh.Clear();_displayMesh.vertices=new[]{new Vector3(_viewport.xMin,_viewport.yMin,0f),new Vector3(_viewport.xMax,_viewport.yMin,0f),new Vector3(_viewport.xMax,_viewport.yMax,0f),new Vector3(_viewport.xMin,_viewport.yMax,0f)};
            _displayMesh.uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)};_displayMesh.triangles=new[]{0,1,2,0,2,3};_displayMesh.RecalculateBounds();
        }
        public void Begin(Vector2 sourceOrigin)
        {
            if(!_initialized)throw new InvalidOperationException("Source Canvas surface is not initialized.");
            SourceOrigin=sourceOrigin;_vertices.Clear();_colors.Clear();_uv.Clear();_data.Clear();_triangles.Clear();
            _gradients.Clear();_gradientIndices.Clear();_path.Clear();_contour=null;_stack.Clear();_batches.Clear();_image=null;
        }
        public void End()
        {
            using var endSample=CanvasEndMarker.Auto();
            using(CanvasTextMarker.Auto())PrepareText();
            var scale=transform.parent!=null?transform.parent.lossyScale:Vector3.one;
            transform.localScale=new Vector3(1f/scale.x,1f/scale.y,1f/scale.z);
            using(var meshSample=CanvasMeshMarker.Auto()){
            _mesh.Clear();_mesh.SetVertices(_vertices);_mesh.SetColors(_colors);_mesh.SetUVs(0,_uv);_mesh.SetUVs(1,_data);_mesh.subMeshCount=_batches.Count;
            for(var i=0;i<_batches.Count;i++)_mesh.SetTriangles(_triangles,_batches[i].Start,_batches[i].Count,i,false);
            _mesh.RecalculateBounds();}
            using(var gradientSample=CanvasGradientMarker.Auto()){
            while(_paintHeight<_gradients.Count) { _paintHeight*=2;ResizePaintTexture(); }
            for(var row=0;row<_gradients.Count;row++)
            {
                var gradient=_gradients[row];var offset=row*PaintWidth;var p=gradient.Coordinates;
                _paintPixels[offset]=gradient.Kind=="createRadialGradient"?new Color((float)p[0],(float)p[1],(float)p[2],1f):new Color((float)p[0],(float)p[1],0f,2f);
                _paintPixels[offset+1]=gradient.Kind=="createRadialGradient"?new Color((float)p[3],(float)p[4],(float)p[5],gradient.Stops.Count):new Color((float)p[2],(float)p[3],0f,gradient.Stops.Count);
                if(gradient.Stops.Count>16)throw new InvalidOperationException("Source gradient exceeds its native stop buffer.");
                _gradientStopOrder.Clear();
                for(var i=0;i<gradient.Stops.Count;i++)_gradientStopOrder.Add((gradient.Stops[i].Key,i));
                _gradientStopOrder.Sort((a,b)=>{var order=a.Key.CompareTo(b.Key);return order!=0?order:a.Index.CompareTo(b.Index);});
                for(var i=0;i<_gradientStopOrder.Count;i++){
                    var stop=gradient.Stops[_gradientStopOrder[i].Index];
                    _paintPixels[offset+2+i*2]=ParseColor(stop.Value);
                    _paintPixels[offset+3+i*2]=new Color((float)stop.Key,0f,0f,0f);
                }
            }
            if(_gradients.Count>0){_paintTexture.SetPixels(_paintPixels);_paintTexture.Apply(false,false);}}
            using(CanvasDrawMarker.Auto())RenderCanvas();
            using(var presentSample=CanvasPresentMarker.Auto()){
            var previous=RenderTexture.active;var gamma=GL.sRGBWrite;try{GL.sRGBWrite=QualitySettings.activeColorSpace==ColorSpace.Linear;Graphics.Blit(_target,_uiTarget,_displayMaterial,1);}finally{RenderTexture.active=previous;GL.sRGBWrite=gamma;}}
        }
        private void CompletePaint(int first)
        {
            var count=_triangles.Count-first;if(count==0)return;
            var batch=new Batch {Start=first,Count=count,Texture=_image,Blend=_state.Blend,Blur=_state.ShadowBlur,X=_state.ShadowX,Y=_state.ShadowY,Shadow=_state.Shadow};
            if(batch.Shadow.a<=0f&&_batches.Count>0) {
                var previous=_batches[_batches.Count-1];
                if(previous.Text==null&&previous.Shadow.a<=0f&&previous.Texture==batch.Texture&&previous.Blend==batch.Blend){previous.Count+=count;return;}
            }
            _batches.Add(batch);
        }
        private void RenderCanvas()
        {
            if(_target==null)throw new InvalidOperationException("Source Canvas viewport is not set");
            _commands.Clear();_commands.SetRenderTarget(_target);_commands.ClearRenderTarget(false,true,Color.clear);
            var projection=GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(_viewport.xMin,_viewport.xMax,_viewport.yMin,_viewport.yMax,-1f,1f),true);
            _commands.SetViewProjectionMatrices(Matrix4x4.identity,projection);
            var mask=ShadowMaskId;var ping=ShadowPingId;var blur=ShadowBlurId;
            var hasShadows=false;var paddingX=0;var paddingY=0;foreach(var batch in _batches)if(batch.Count>0&&batch.Shadow.a>0f&&(batch.Blur>0f||batch.X!=0f||batch.Y!=0f)){hasShadows=true;paddingX=Math.Max(paddingX,VisualCanvasShadowViewport.Padding(batch.Blur,batch.X));paddingY=Math.Max(paddingY,VisualCanvasShadowViewport.Padding(batch.Blur,batch.Y));}
            var shadowViewport=new VisualCanvasShadowViewport(_target.width,_target.height,paddingX,paddingY);
            var pixelX=_viewport.width/_target.width;var pixelY=_viewport.height/_target.height;
            var shadowProjection=GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(_viewport.xMin-paddingX*pixelX,_viewport.xMax+paddingX*pixelX,_viewport.yMin-paddingY*pixelY,_viewport.yMax+paddingY*pixelY,-1f,1f),true);
            if(hasShadows){_commands.GetTemporaryRT(mask,shadowViewport.Width,shadowViewport.Height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear,4);
            _commands.GetTemporaryRT(ping,shadowViewport.Width,shadowViewport.Height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
            _commands.GetTemporaryRT(blur,shadowViewport.Width,shadowViewport.Height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);}
            for(var i=0;i<_batches.Count;i++) {
                var batch=_batches[i];if(batch.Count==0)continue;var props=_properties;var image=batch.Texture!=null?batch.Texture:Texture2D.whiteTexture;props.SetTexture(ImageTexId,image);props.SetVector(ImageTexelSizeId,new Vector4(1f/image.width,1f/image.height,image.width,image.height));
                if(batch.Shadow.a>0f&&(batch.Blur>0f||batch.X!=0f||batch.Y!=0f)) {
                    using var shadowSample=CanvasShadowCommandsMarker.Auto();
                    _commands.SetRenderTarget(mask);_commands.ClearRenderTarget(false,true,Color.clear);_commands.SetViewProjectionMatrices(Matrix4x4.identity,shadowProjection);
                    _commands.DrawMesh(_mesh,Matrix4x4.identity,_material,i,0,props);
                    _commands.SetGlobalFloat(ShadowSigmaId,(float)VisualCanvasShadowViewport.Sigma(batch.Blur));
                    _commands.SetGlobalVector(ShadowDirectionId,new Vector4(1f/shadowViewport.Width,0f,0f,0f));_commands.Blit(mask,ping,_blurMaterial,0);
                    _commands.SetGlobalVector(ShadowDirectionId,new Vector4(0f,1f/shadowViewport.Height,0f,0f));_commands.Blit(ping,blur,_blurMaterial,0);
                    _commands.SetGlobalColor(ShadowColorId,batch.Shadow);
                    _commands.SetGlobalVector(ShadowOffsetId,new Vector4(batch.X/shadowViewport.Width,-batch.Y/shadowViewport.Height,0f,0f));
                    _commands.SetGlobalVector(ShadowRectId,new Vector4((float)shadowViewport.U,(float)shadowViewport.V,(float)shadowViewport.UWidth,(float)shadowViewport.VHeight));
                    _commands.Blit(blur,_target,_blurMaterial,batch.Blend=="lighter"?2:batch.Blend=="screen"?3:1);
                    _commands.SetViewProjectionMatrices(Matrix4x4.identity,projection);
                }
                _commands.SetRenderTarget(_target);_commands.DrawMesh(_mesh,Matrix4x4.identity,_material,i,batch.Blend=="lighter"?1:batch.Blend=="screen"?2:0,props);
            }
            var previous=RenderTexture.active;var gamma=GL.sRGBWrite;
            try{if(hasShadows){_commands.ReleaseTemporaryRT(mask);_commands.ReleaseTemporaryRT(ping);_commands.ReleaseTemporaryRT(blur);}_commands.SetRenderTarget(previous!=null?new RenderTargetIdentifier(previous):new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));using(CanvasExecuteMarker.Auto())Graphics.ExecuteCommandBuffer(_commands);}
            finally{RenderTexture.active=previous;GL.sRGBWrite=gamma;}
        }
        private void ResizePaintTexture()
        {
            _paintHeight=Mathf.Max(1,_paintHeight);
            if(_paintTexture!=null)Destroy(_paintTexture);
            _paintTexture=new Texture2D(Mathf.Max(1,PaintWidth),_paintHeight,TextureFormat.RGBAFloat,false,true) {filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,name="Source Canvas Gradient Stops"};
            _paintPixels=new Color[PaintWidth*_paintHeight];_material.SetTexture(PaintsId,_paintTexture);
        }
        public void Set(string name,V value)
        {
            switch(name)
            {
                case "fillStyle":_state.Fill=value;break;case "strokeStyle":_state.Stroke=value;break;
                case "globalAlpha":if(value.Number>=0d&&value.Number<=1d)_state.Alpha=(float)value.Number;break;
                case "globalCompositeOperation":_state.Blend=value.Text;break;
                case "lineWidth":if(value.Number>0d)_state.Width=(float)value.Number;break;
                case "lineCap":_state.Cap=value.Text;break;case "lineJoin":_state.Join=value.Text;break;
                case "lineDashOffset":_state.DashOffset=(float)value.Number;break;case "miterLimit":_state.Miter=(float)value.Number;break;
                case "shadowBlur":_state.ShadowBlur=(float)value.Number;break;case "shadowColor":_state.Shadow=ParseColor(value.Text);break;
                case "shadowOffsetX":_state.ShadowX=(float)value.Number;break;case "shadowOffsetY":_state.ShadowY=(float)value.Number;break;
                case "font":_state.Font=value.Text;break;case "textAlign":_state.Align=value.Text;break;case "textBaseline":_state.Baseline=value.Text;break;
                case "canvas":break;
                default:throw new InvalidOperationException("Unimplemented Canvas property in authored drawing: "+name);
            }
        }
        public V Call(string name,V[] args)
        {
            float N(int i)=>(float)VisualJs.Arg(args,i).Number;
            switch(name)
            {
                case "save":_stack.Push(_state.Copy());break;
                case "restore":if(_stack.Count>0)_state=_stack.Pop();break;
                case "translate":_state.Matrix*=Matrix4x4.Translate(new Vector3(N(0),N(1),0f));break;
                case "rotate":_state.Matrix*=Matrix4x4.Rotate(Quaternion.Euler(0f,0f,N(0)*Mathf.Rad2Deg));break;
                case "scale":_state.Matrix*=Matrix4x4.Scale(new Vector3(N(0),N(1),1f));break;
                case "transform":case "setTransform":
                    var matrix=Matrix4x4.identity;matrix.m00=N(0);matrix.m10=N(1);matrix.m01=N(2);matrix.m11=N(3);matrix.m03=N(4);matrix.m13=N(5);
                    _state.Matrix=name=="transform"?_state.Matrix*matrix:matrix;break;
                case "resetTransform":_state.Matrix=Matrix4x4.identity;break;
                case "beginPath":_path.Clear();_contour=null;break;
                case "moveTo":Move(new Vector2(N(0),N(1)));break;
                case "lineTo":Line(new Vector2(N(0),N(1)));break;
                case "bezierCurveTo":Cubic(new Vector2(N(0),N(1)),new Vector2(N(2),N(3)),new Vector2(N(4),N(5)));break;
                case "quadraticCurveTo":
                    var end=new Vector2(N(2),N(3));var control=new Vector2(N(0),N(1));
                    var current=_contour==null||_contour.Segments.Count==0?control:(Vector2)_state.Matrix.inverse.MultiplyPoint3x4(_contour.Segments[_contour.Segments.Count-1].P0);
                    if(_contour==null)Move(control);Cubic(current+(control-current)*(2f/3f),end+(control-end)*(2f/3f),end);break;
                case "closePath":Close();break;
                case "arc":Ellipse(N(0),N(1),N(2),N(2),0f,N(3),N(4),args.Length>5&&args[5].Truth);break;
                case "ellipse":Ellipse(N(0),N(1),N(2),N(3),N(4),N(5),N(6),args.Length>7&&args[7].Truth);break;
                case "rect":Rectangle(N(0),N(1),N(2),N(3));break;
                case "roundRect":RoundRectangle(N(0),N(1),N(2),N(3),args.Length>4?args[4]:new V(0d));break;
                case "measureText":var measured=SourceCanvasTypography.Resolve(_state.Font,out var fontSize,out var fontStyle);return VisualJs.Obj("width",SourceCanvasTypography.Width(measured,args[0].Text,fontSize,fontStyle)*SourceCanvasTypography.Size(_state.Font)/fontSize);
                case "fillText":Text(args[0].Text,N(1),N(2),args.Length>3?N(3):float.PositiveInfinity);break;
                case "drawImage":Image(args);break;
                case "fill":Paint(false,args.Length>0&&args[0].Text=="evenodd");break;
                case "stroke":Paint(true,false);break;
                case "fillRect":case "strokeRect":
                     _rectSavedPath.Clear();_rectSavedPath.AddRange(_path);var oldContour=_contour;_path.Clear();_contour=null;
                    try{Rectangle(N(0),N(1),N(2),N(3));Paint(name=="strokeRect",false);}
                    finally{_path.Clear();_path.AddRange(_rectSavedPath);_rectSavedPath.Clear();_contour=oldContour;}break;
                case "setLineDash":_dashScratch.Clear();foreach(var x in args[0].Items)_dashScratch.Add((float)x.Number);if(_dashScratch.Count%2==1){var count=_dashScratch.Count;for(var i=0;i<count;i++)_dashScratch.Add(_dashScratch[i]);}_state.Dash=_dashScratch.ToArray();break;
                case "createRadialGradient":case "createLinearGradient":var gradient=new VisualGradient(name,args);_gradientSpaces.Add(gradient,new GradientSpace {Inverse=_state.Matrix.inverse});return new V(gradient);
                default:throw new InvalidOperationException("Unimplemented Canvas command in authored drawing: "+name);
            }
            return default;
        }
        private Vector2 Point(Vector2 p)=>_state.Matrix.MultiplyPoint3x4(p);
        private void Move(Vector2 point)
        { _contour=new Contour();_path.Add(_contour);var p=Point(point);_contour.Segments.Add(new BezierPathSegment {P0=p,P1=p,P2=p}); }
        private void Line(Vector2 point)
        {
            if(_contour==null){Move(point);return;}
            var p=Point(point);var index=_contour.Segments.Count-1;var previous=_contour.Segments[index];
            previous.P1=Vector2.Lerp(previous.P0,p,1f/3f);previous.P2=Vector2.Lerp(previous.P0,p,2f/3f);_contour.Segments[index]=previous;
            _contour.Segments.Add(new BezierPathSegment {P0=p,P1=p,P2=p});
        }
        private void Cubic(Vector2 first,Vector2 second,Vector2 end)
        {
            if(_contour==null)Move(first);var index=_contour.Segments.Count-1;var previous=_contour.Segments[index];previous.P1=Point(first);previous.P2=Point(second);_contour.Segments[index]=previous;
            var p=Point(end);_contour.Segments.Add(new BezierPathSegment {P0=p,P1=p,P2=p});
        }
        private void Close()
        {
            if(_contour==null||_contour.Segments.Count<2)return;
            var lastIndex=_contour.Segments.Count-1;var first=_contour.Segments[0].P0;var last=_contour.Segments[lastIndex];
            if((last.P0-first).sqrMagnitude<.00000001f)_contour.Segments.RemoveAt(lastIndex);
            else {last.P1=Vector2.Lerp(last.P0,first,1f/3f);last.P2=Vector2.Lerp(last.P0,first,2f/3f);_contour.Segments[lastIndex]=last;}
            _contour.Closed=true;
        }
        private void Rectangle(float x,float y,float width,float height)
        {Move(new Vector2(x,y));Line(new Vector2(x+width,y));Line(new Vector2(x+width,y+height));Line(new Vector2(x,y+height));Close();}
        private void RoundRectangle(float x,float y,float width,float height,V radii)
        {
            var radiusList=radii.Raw as List<V>;
            var count=radiusList==null?1:radiusList.Count;
            if(count==0||count>4)throw new ArgumentOutOfRangeException(nameof(radii));
            var a=radiusList==null?(float)radii.Number:(float)radiusList[0].Number;
            var b=count>1?(float)radiusList[1].Number:a;
            var c=count>2?(float)radiusList[2].Number:a;
            var d=count>3?(float)radiusList[3].Number:b;
            if(width<0f){x+=width;width=-width;var swap=a;a=b;b=swap;swap=c;c=d;d=swap;}
            if(height<0f){y+=height;height=-height;var swap=a;a=d;d=swap;swap=b;b=c;c=swap;}
            var shrink=Mathf.Min(1f,width/Mathf.Max(a+b,c+d),height/Mathf.Max(a+d,b+c));a*=shrink;b*=shrink;c*=shrink;d*=shrink;
            Move(new Vector2(x+a,y));Line(new Vector2(x+width-b,y));Ellipse(x+width-b,y+b,b,b,0f,-Mathf.PI*.5f,0f,false);
            Line(new Vector2(x+width,y+height-c));Ellipse(x+width-c,y+height-c,c,c,0f,0f,Mathf.PI*.5f,false);
            Line(new Vector2(x+d,y+height));Ellipse(x+d,y+height-d,d,d,0f,Mathf.PI*.5f,Mathf.PI,false);
            Line(new Vector2(x,y+a));Ellipse(x+a,y+a,a,a,0f,Mathf.PI,Mathf.PI*1.5f,false);Close();
        }
        private void AddQuadCorner(Vector2 point,Vector2 uv,Color color)
        {
            var p=Point(point)-SourceOrigin;
            _vertices.Add(new Vector3(p.x*SpherePlacementRules.SourcePixelsToWorld,-p.y*SpherePlacementRules.SourcePixelsToWorld,0f));
            _colors.Add(color);_uv.Add(new Vector4(uv.x,uv.y,-1f,0f));_data.Add(Vector4.zero);
        }
        private void Quad(Vector2 a,Vector2 b,Vector2 c,Vector2 d,Vector2 ua,Vector2 ub,Vector2 uc,Vector2 ud,Color color)
        {
            var first=_vertices.Count;
            AddQuadCorner(a,ua,color);AddQuadCorner(b,ub,color);AddQuadCorner(c,uc,color);AddQuadCorner(d,ud,color);
            _triangles.Add(first);_triangles.Add(first+1);_triangles.Add(first+2);_triangles.Add(first);_triangles.Add(first+2);_triangles.Add(first+3);
        }
        private void Image(V[] args)
        {
            if(!(args[0].Raw is SourceCanvasImage image)||image.Sprite==null)return;
            var sprite=image.Sprite;var rect=sprite.textureRect;var texture=sprite.texture;var width=(float)image.Get("naturalWidth").Number;var height=(float)image.Get("naturalHeight").Number;
            var x=(float)args[1].Number;var y=(float)args[2].Number;var dw=args.Length>3?(float)args[3].Number:width;var dh=args.Length>4?(float)args[4].Number:height;
            if(args.Length>5)throw new InvalidOperationException("Unexpected cropped drawImage in authored renderer");
            _image=texture;var start=_triangles.Count;
            Quad(new Vector2(x,y),new Vector2(x+dw,y),new Vector2(x+dw,y+dh),new Vector2(x,y+dh),
                new Vector2(rect.xMin/texture.width,rect.yMax/texture.height),new Vector2(rect.xMax/texture.width,rect.yMax/texture.height),new Vector2(rect.xMax/texture.width,rect.yMin/texture.height),new Vector2(rect.xMin/texture.width,rect.yMin/texture.height),new Color(1,1,1,_state.Alpha));
            CompletePaint(start);_image=null;
        }
        private void Text(string text,float x,float y,float maxWidth)
        {
            if(LocalizeCanvasText)text=Visual_canvasLocale.Scope["localizeCanvasText"].Call(text).Text;
            _batches.Add(new Batch {Text=text,TextX=x,TextY=y,TextWidth=maxWidth,TextState=_state.Copy(),Blend=_state.Blend,Blur=_state.ShadowBlur,X=_state.ShadowX,Y=_state.ShadowY,Shadow=_state.Shadow});
        }
        private void PrepareText()
        {
            // Dynamic legacy fonts can rebuild their atlas while later text is
            // requested. Resolve every required glyph before retaining any UVs.
            var dirty=false;void Rebuilt(Font font){dirty=true;}Font.textureRebuilt+=Rebuilt;
            try{var attempts=0;do{dirty=false;foreach(var batch in _batches)if(batch.Text!=null){var font=SourceCanvasTypography.Resolve(batch.TextState.Font,out var size,out var style,TextRasterScale(batch.TextState));SourceCanvasTypography.Vertices(font,batch.Text,size,style);}if(++attempts>8&&dirty)throw new InvalidOperationException("Source font atlas could not retain the frame's glyphs");}while(dirty);}
            finally{Font.textureRebuilt-=Rebuilt;}
            var previous=_state;
            try{foreach(var batch in _batches)if(batch.Text!=null){_state=batch.TextState;EmitText(batch);}}
            finally{_state=previous;_image=null;}
        }
        private void EmitText(Batch batch)
        {
            var text=batch.Text;var x=batch.TextX;var y=batch.TextY;var maxWidth=batch.TextWidth;
            var font=SourceCanvasTypography.Resolve(_state.Font,out var size,out var style,TextRasterScale(_state));var fontScale=SourceCanvasTypography.Size(_state.Font)/size;var width=SourceCanvasTypography.Width(font,text,size,style)*fontScale;
            var scale=width>maxWidth?maxWidth/width:1f;
            if(_state.Align=="center")x-=width*scale*.5f;else if(_state.Align=="right"||_state.Align=="end")x-=width*scale;
            var sampleAscent=font.ascent*(float)size/font.fontSize;var ascent=sampleAscent*fontScale;var lineHeight=font.lineHeight*(float)size/font.fontSize*fontScale;
            if(_state.Baseline=="top"||_state.Baseline=="hanging")y+=ascent;
            else if(_state.Baseline=="middle")y+=ascent-lineHeight*.5f;
            else if(_state.Baseline=="bottom"||_state.Baseline=="ideographic")y-=lineHeight-ascent;
            var color=ParseColor(_state.Fill.Text);color.a*=_state.Alpha;var start=_triangles.Count;_image=font.material.mainTexture;
            var vertices=SourceCanvasTypography.Vertices(font,text,size,style);
            Vector2 Position(UIVertex vertex)=>new Vector2(x+vertex.position.x*fontScale*scale,y-(vertex.position.y+sampleAscent)*fontScale);
            for(var index=0;index+3<vertices.Length;index+=4) {
                var a=vertices[index];var b=vertices[index+1];var c=vertices[index+2];var d=vertices[index+3];
                Quad(Position(a),Position(b),Position(c),Position(d),a.uv0,b.uv0,c.uv0,d.uv0,color);
            }
            batch.Start=start;batch.Count=_triangles.Count-start;batch.Texture=_image;_image=null;
        }
        private float TextRasterScale(State state)=>Mathf.Max(.0001f,_dpr*Mathf.Max(state.Matrix.MultiplyVector(Vector3.right).magnitude,state.Matrix.MultiplyVector(Vector3.up).magnitude));
        private void Ellipse(float x,float y,float rx,float ry,float rotation,float start,float end,bool anticlockwise)
        {
            if(rx<0f||ry<0f)throw new ArgumentOutOfRangeException("Source Canvas ellipse radius");
            var delta=end-start;var tau=Mathf.PI*2f;
            if(!anticlockwise){delta=delta>=tau?tau:Mathf.Repeat(delta,tau);}else{delta=-delta>=tau?-tau:-Mathf.Repeat(-delta,tau);}
            Vector2 At(float a)=>new Vector2(x+rx*Mathf.Cos(a)*Mathf.Cos(rotation)-ry*Mathf.Sin(a)*Mathf.Sin(rotation),y+rx*Mathf.Cos(a)*Mathf.Sin(rotation)+ry*Mathf.Sin(a)*Mathf.Cos(rotation));
            Vector2 Tangent(float a)=>new Vector2(-rx*Mathf.Sin(a)*Mathf.Cos(rotation)-ry*Mathf.Cos(a)*Mathf.Sin(rotation),-rx*Mathf.Sin(a)*Mathf.Sin(rotation)+ry*Mathf.Cos(a)*Mathf.Cos(rotation));
            var first=At(start);if(_contour==null)Move(first);else if((Point(first)-_contour.Segments[_contour.Segments.Count-1].P0).sqrMagnitude>1e-10f)Line(first);
            var steps=Mathf.Max(1,Mathf.CeilToInt(Mathf.Abs(delta)/(Mathf.PI*.25f)));var step=delta/steps;
            for(var i=0;i<steps;i++){var a=start+i*step;var b=a+step;var tangent=4f/3f*Mathf.Tan(step*.25f);Cubic(At(a)+Tangent(a)*tangent,At(b)-Tangent(b)*tangent,At(b));}
        }
        private void Paint(bool stroke,bool evenOdd)
        {
            if(_state.Alpha<=0f||_path.Count==0)return;
            var paintStart=_triangles.Count;
            var inverse=_state.Matrix.inverse;var contours=new List<BezierContour>();
            foreach(var path in _path)
            {
                if(path.Segments.Count<2)continue;var segments=new BezierPathSegment[path.Segments.Count];
                for(var i=0;i<segments.Length;i++){var segment=path.Segments[i];segment.P0=inverse.MultiplyPoint3x4(segment.P0);segment.P1=inverse.MultiplyPoint3x4(segment.P1);segment.P2=inverse.MultiplyPoint3x4(segment.P2);segments[i]=segment;}
                contours.Add(new BezierContour {Segments=segments,Closed=path.Closed});
            }
            if(contours.Count==0)return;
            var shape=new Shape {Contours=contours.ToArray()};
            if(stroke)
            {
                var ending=_state.Cap=="round"?PathEnding.Round:_state.Cap=="square"?PathEnding.Square:PathEnding.Chop;
                shape.PathProps=new PathProperties {Head=ending,Tail=ending,Corners=_state.Join=="round"?PathCorner.Round:_state.Join=="bevel"?PathCorner.Beveled:PathCorner.Tipped,
                    Stroke=new Stroke {Color=Color.white,HalfThickness=_state.Width*.5f,Pattern=_state.Dash,PatternOffset=_state.DashOffset,TippedCornerLimit=_state.Miter}};
            }
            else shape.Fill=new SolidFill {Color=Color.white,Mode=evenOdd?FillMode.OddEven:FillMode.NonZero};
            var options=new VectorUtils.TessellationOptions {StepDistance=float.MaxValue,MaxCordDeviation=.02f,MaxTanAngleDeviation=.08f,SamplingStepSize=.01f};
            List<VectorUtils.Geometry> geometries;
            using(CanvasTessellationMarker.Auto())geometries=_geometryCache.GetOrCreate(shape,options);
            var paint=stroke?_state.Stroke:_state.Fill;var index=-1;var color=Color.white;
            if(paint.Raw is VisualGradient gradient)
            {if(!_gradientIndices.TryGetValue(gradient,out index)){index=_gradients.Count;_gradientIndices.Add(gradient,index);_gradients.Add(gradient);}}
            else color=ParseColor(paint.Text);
            color.a*=_state.Alpha;var blend=_state.Blend=="lighter"?1f:0f;
            foreach(var geometry in geometries)
            {
                var first=_vertices.Count;
                foreach(var local in geometry.Vertices)
                {
                    var source=(Vector2)_state.Matrix.MultiplyPoint3x4(local)-SourceOrigin;
                    _vertices.Add(new Vector3(source.x*SpherePlacementRules.SourcePixelsToWorld,-source.y*SpherePlacementRules.SourcePixelsToWorld,0f));_colors.Add(color);
                    var paintPoint=local;if(paint.Raw is VisualGradient sourceGradient&&_gradientSpaces.TryGetValue(sourceGradient,out var space))paintPoint=space.Inverse.MultiplyPoint3x4(_state.Matrix.MultiplyPoint3x4(local));
                    _uv.Add(new Vector4(paintPoint.x,paintPoint.y,index+1f,blend));_data.Add(Vector4.zero);
                }
                foreach(var triangle in geometry.Indices)_triangles.Add(first+triangle);
            }
            CompletePaint(paintStart);
        }
        public static Color ParseColor(string text)
        {
            if(text=="transparent")return Color.clear;if(ColorUtility.TryParseHtmlString(text,out var parsed))return parsed;
            if(text.StartsWith("rgb",StringComparison.Ordinal))
            {
                var start=text.IndexOf('(');var parts=text.Substring(start+1,text.Length-start-2).Split(',');
                float Component(int i,float divisor)=>Mathf.Clamp01(float.Parse(parts[i],NumberStyles.Float,CultureInfo.InvariantCulture)/divisor);
                return new Color(Component(0,255f),Component(1,255f),Component(2,255f),parts.Length>3?Component(3,1f):1f);
            }
            throw new InvalidOperationException("Unsupported authored Canvas color: "+text);
        }
        private void OnDestroy(){if(_commands!=null)_commands.Release();if(_target!=null){_target.Release();Destroy(_target);}if(_uiTarget!=null){_uiTarget.Release();Destroy(_uiTarget);}if(_displayMesh!=null)Destroy(_displayMesh);if(_displayMaterial!=null)Destroy(_displayMaterial);if(_blurMaterial!=null)Destroy(_blurMaterial);if(_mesh!=null)Destroy(_mesh);if(_material!=null)Destroy(_material);if(_paintTexture!=null)Destroy(_paintTexture);}
    }
}
