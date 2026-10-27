using System;
using System.Collections.Generic;
using EchoSphere.Core;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Profiling;

namespace EchoSphere.Presentation
{
    // Ordered Unity mesh compositor. It has no Canvas command sink, paths,
    // generic paint texture, DOM tree, generated drawing code or scene Renderer.
    internal sealed partial class NativeWorldSurface : MonoBehaviour
    {
        private sealed class State
        {
            internal Matrix4x4 Matrix=Matrix4x4.identity;internal float Alpha=1,ShadowBlur,Miter=10;
            internal readonly float ShadowX=0,ShadowY=0,DashOffset=0;
            internal string Blend="source-over",Baseline="alphabetic";internal Color Shadow=Color.clear;
            internal float[] Dash=Array.Empty<float>();
            internal void CopyFrom(State value){Matrix=value.Matrix;Alpha=value.Alpha;ShadowBlur=value.ShadowBlur;Miter=value.Miter;Blend=value.Blend;Baseline=value.Baseline;Shadow=value.Shadow;Dash=value.Dash;}
        }
        private sealed class Batch
        {
            internal Mesh NativeMesh;internal Material NativeMaterial;internal Matrix4x4 NativeMatrix;
            internal MaterialPropertyBlock NativeProperties;internal string Blend;internal Color Shadow;internal float Blur,X,Y;
            internal Action NativePrepareGlyphs,NativeEmitGlyphs;
        }
        private State _state=new State();private readonly Stack<State> _stack=new Stack<State>();private readonly List<Batch> _batches=new List<Batch>();
        private readonly Stack<State> _freeStates=new Stack<State>();
        private readonly List<Batch> _batchPool=new List<Batch>();private int _batchUsed;
        internal long ShadowPixelCount {get;private set;}
        internal int ShadowPassCount {get;private set;}
        internal long ShadowCompositePixelCount {get;private set;}
        internal int ShadowCompositeCount {get;private set;}
        internal int CulledBatchCount {get;private set;}
        private readonly NativeWorldShadowCache _shadowCache=new NativeWorldShadowCache();
        internal int ShadowCacheHits {get;private set;}
        internal long ShadowCacheBytes=>_shadowCache.ResidentBytes;
#if UNITY_EDITOR
        internal bool FullShadowViewportForValidation=false;
        internal bool DisableShadowCacheForValidation=false;
        internal bool DisableInstancingForValidation=false;
        internal bool FullShadowCompositeForValidation=false;
        internal bool DisableBatchCullingForValidation=false;
#endif
        private CommandBuffer _commands;private Material _blurMaterial,_presentMaterial;private RenderTexture _target,_uiTarget;private Rect _viewport;private float _dpr=1;private Vector2 _logicalSize;
        private NativeWorldRenderGraphLease _renderGraphLease;
        private RTHandle _graphTarget,_graphUi;
        internal bool GraphPending {get;private set;}
        internal int GraphPassCount {get;private set;}
        internal int LastGraphFrame {get;private set;}=-1;
        internal RTHandle GraphTarget=>_graphTarget??(_graphTarget=RTHandles.Alloc(_target));
        internal RTHandle GraphUi=>_graphUi??(_graphUi=RTHandles.Alloc(_uiTarget));
#if UNITY_EDITOR
        internal bool ImmediateWorldForValidation=false;
#endif
        internal void BindCamera(Camera camera)
        {
            if(_renderGraphLease!=null&&_renderGraphLease.IsCurrent(camera))return;
            _renderGraphLease?.Dispose();_renderGraphLease=new NativeWorldRenderGraphLease(this,camera);
        }
        internal void CancelGraphFrame()=>GraphPending=false;
        private Matrix4x4 _sourceToWorld=Matrix4x4.identity;private Shader _solidShader,_labelShader;
        private readonly Dictionary<Shader,bool> _boundedShaders=new Dictionary<Shader,bool>();
        private Material _solidInstances;private MaterialPropertyBlock _instanceProperties;private bool _canInstance;
        private readonly Matrix4x4[] _instanceMatrices=new Matrix4x4[64];
        private readonly Vector4[] _instanceColors=new Vector4[64];private readonly float[] _instanceOpacities=new float[64];
        internal int GeometryDrawCalls {get;private set;}
        internal int InstancedPrimitiveCount {get;private set;}
        internal Vector2 SourceOrigin;internal bool LocalizeCanvasText=true;internal Texture UiTexture=>_uiTarget;
        internal Vector2 LogicalSize=>_logicalSize;
        private static readonly ProfilerMarker ShadowCommandsMarker=new ProfilerMarker("EchoSphere.World.ShadowCommands"),ExecuteMarker=new ProfilerMarker("EchoSphere.World.Execute"),FrameMarker=new ProfilerMarker("EchoSphere.World.Composite");
        private static readonly int ShadowMaskId=Shader.PropertyToID("_SourceShadowMask");
        private static readonly int ShadowPingId=Shader.PropertyToID("_SourceShadowPing");
        private static readonly int ShadowBlurId=Shader.PropertyToID("_SourceShadowBlur");
        private static readonly int ShadowSigmaId=Shader.PropertyToID("_SourceShadowSigma");
        private static readonly int ShadowDirectionId=Shader.PropertyToID("_SourceShadowDirection");
        private static readonly int ShadowColorId=Shader.PropertyToID("_SourceShadowColor");
        private static readonly int ShadowOffsetId=Shader.PropertyToID("_SourceShadowOffset");
        private static readonly int ShadowRectId=Shader.PropertyToID("_SourceShadowRect");
        private static readonly int ShadowWeightsId=Shader.PropertyToID("_SourceShadowWeights");
        private static readonly int ShadowWeightCountId=Shader.PropertyToID("_SourceShadowWeightCount");
        private static readonly int NativeColorId=Shader.PropertyToID("_NativeColor"),NativeOpacityId=Shader.PropertyToID("_NativeOpacity"),LabelOffsetId=Shader.PropertyToID("_LabelOffset");
        internal void Initialize()
        {_commands=new CommandBuffer {name="ECHO SPHERE native world"};_blurMaterial=new Material(Resources.Load<Shader>("EchoSphere/SourceCanvasShadow"));_presentMaterial=new Material(Resources.Load<Shader>("EchoSphere/SourceCanvasPresent"));_solidShader=Resources.Load<Shader>("EchoSphere/NativeWorldSolid");_labelShader=Resources.Load<Shader>("EchoSphere/NativeWorldLabel");_solidInstances=Resources.Load<Material>("EchoSphere/NativeWorldSolidInstances");_instanceProperties=new MaterialPropertyBlock();_canInstance=SystemInfo.supportsInstancing&&_solidInstances!=null&&_solidInstances.enableInstancing&&_solidInstances.shader.isSupported;}
        internal void SetViewport(float width,float height,float dpr)
        {
            width=Mathf.Max(1,width);height=Mathf.Max(1,height);_logicalSize=new Vector2(width,height);_viewport=new Rect(-width*.5f*SpherePlacementRules.SourcePixelsToWorld,-height*.5f*SpherePlacementRules.SourcePixelsToWorld,width*SpherePlacementRules.SourcePixelsToWorld,height*SpherePlacementRules.SourcePixelsToWorld);
            double density=Math.Min(1.75,Math.Max(1,float.IsNaN(dpr)||float.IsInfinity(dpr)?1:dpr));_dpr=(float)density;
            int w=Math.Max(1,(int)Math.Floor(width*density+.5)),h=Math.Max(1,(int)Math.Floor(height*density+.5));if(_target!=null&&_target.width==w&&_target.height==h)return;
            _shadowCache.Dispose();_graphTarget?.Release();_graphTarget=null;_graphUi?.Release();_graphUi=null;Release(ref _target);Release(ref _uiTarget);
            _target=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear){name="Native world compositing",antiAliasing=4,filterMode=FilterMode.Bilinear};_target.Create();
            _uiTarget=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name="Native world UI image",filterMode=FilterMode.Bilinear};_uiTarget.Create();
        }
        internal void Begin(Vector2 origin){SourceOrigin=origin;float unit=SpherePlacementRules.SourcePixelsToWorld;_sourceToWorld=Matrix4x4.Translate(new Vector3(-origin.x*unit,origin.y*unit,0))*Matrix4x4.Scale(new Vector3(unit,-unit,1));while(_stack.Count>0)_freeStates.Push(_stack.Pop());_batches.Clear();_batchUsed=0;}
        internal void Save(){var copy=_freeStates.Count>0?_freeStates.Pop():new State();copy.CopyFrom(_state);_stack.Push(copy);}
        internal void Restore(){if(_stack.Count>0){_freeStates.Push(_state);_state=_stack.Pop();}}
        internal void Translate(double x,double y)=>_state.Matrix*=Matrix4x4.Translate(new Vector3((float)x,(float)y,0));
        internal void ResetTransform()=>_state.Matrix=Matrix4x4.identity;
        internal void SetDash(float[] dash)=>_state.Dash=dash;
        internal void SetOpacity(double alpha){if(alpha>=0&&alpha<=1)_state.Alpha=(float)alpha;}
        internal string Blend {set=>_state.Blend=value;}
        internal Color Shadow {set=>_state.Shadow=value;}
        internal float ShadowBlur {set=>_state.ShadowBlur=value;}
        internal void End()
        {
            using var frame=FrameMarker.Auto();
            bool graph=_renderGraphLease!=null&&_renderGraphLease.CanSubmit;
#if UNITY_EDITOR
            if(ImmediateWorldForValidation||UnityEditor.EditorApplication.isPaused)graph=false;
#endif
            if(graph){GraphPending=true;return;}
            GraphPending=false;PrepareFrameGlyphs();RenderMeshes();
            var previous=RenderTexture.active;var gamma=GL.sRGBWrite;try{GL.sRGBWrite=QualitySettings.activeColorSpace==ColorSpace.Linear;Graphics.Blit(_target,_uiTarget,_presentMaterial,2);}finally{RenderTexture.active=previous;GL.sRGBWrite=gamma;}
        }
        private void PrepareFrameGlyphs()
        {
            // Resolve against the final atlas at the point of command recording,
            // including fonts requested by another camera after LateUpdate.
            bool dirty=false;void Rebuilt(Font font){dirty=true;}Font.textureRebuilt+=Rebuilt;
            try{int attempts=0;do{dirty=false;foreach(var batch in _batches)batch.NativePrepareGlyphs?.Invoke();if(++attempts>8&&dirty)throw new InvalidOperationException("Native font atlas could not retain the frame glyphs");}while(dirty);}finally{Font.textureRebuilt-=Rebuilt;}
            foreach(var batch in _batches)batch.NativeEmitGlyphs?.Invoke();
        }
        internal void DrawNativeMesh(Mesh mesh,Material material,MaterialPropertyBlock properties,Matrix4x4? localTransform=null,Color? shadow=null,float shadowBlur=0,bool inheritBlend=false,float? opacity=null,bool inheritShadow=false,bool screen=false,bool additive=false,bool inheritShadowOffsets=false)
        {
            properties.SetFloat(NativeOpacityId,opacity??_state.Alpha);
            var matrix=_sourceToWorld*_state.Matrix*(localTransform??Matrix4x4.identity);
            if(_batchUsed==_batchPool.Count)_batchPool.Add(new Batch());var batch=_batchPool[_batchUsed++];
            batch.NativeMesh=mesh;batch.NativeMaterial=material;batch.NativeMatrix=matrix;batch.NativeProperties=properties;batch.Blend=additive?"lighter":screen?"screen":inheritBlend?_state.Blend:"source-over";batch.Shadow=inheritShadow?_state.Shadow:shadow??Color.clear;batch.Blur=inheritShadow?_state.ShadowBlur:shadowBlur;batch.X=inheritShadow||inheritShadowOffsets?_state.ShadowX:0;batch.Y=inheritShadow||inheritShadowOffsets?_state.ShadowY:0;batch.NativePrepareGlyphs=null;batch.NativeEmitGlyphs=null;_batches.Add(batch);
        }
        internal Matrix4x4 NativePaintMatrix=>_state.Matrix;
        internal float NativeDashOffset=>_state.DashOffset;
        internal float[] NativeStrokeDash=>_state.Dash;
        internal string NativeTextBaseline=>_state.Baseline;
        internal float NativeOpacity=>_state.Alpha;
        internal PathEnding NativeStrokeEnding=>PathEnding.Chop;
        internal PathCorner NativeStrokeCorner=>PathCorner.Tipped;
        internal float NativeStrokeMiter=>_state.Miter;
        internal float NativeTextRasterScaleFor(Matrix4x4 local){var matrix=_state.Matrix*local;return Mathf.Max(.0001f,_dpr*Mathf.Max(matrix.MultiplyVector(Vector3.right).magnitude,matrix.MultiplyVector(Vector3.up).magnitude));}
        internal float NativeTextRasterScale=>NativeTextRasterScaleFor(Matrix4x4.identity);
        // Conservative source-space envelope before any geometry is built.
        // Transform all corners so camera translation, shake and affine source
        // transforms use exactly the same coordinates as the final compositor.
        internal bool IntersectsLocalBounds(Rect bounds,Matrix4x4 local)
        {
            if(_target==null)return true;
            var matrix=_sourceToWorld*_state.Matrix*local;
            float left=float.PositiveInfinity,bottom=float.PositiveInfinity,right=float.NegativeInfinity,top=float.NegativeInfinity;
            for(int i=0;i<4;i++){
                var p=matrix.MultiplyPoint3x4(new Vector3(i%2==0?bounds.xMin:bounds.xMax,i<2?bounds.yMin:bounds.yMax,0));
                left=Mathf.Min(left,p.x);right=Mathf.Max(right,p.x);bottom=Mathf.Min(bottom,p.y);top=Mathf.Max(top,p.y);
            }
            if(!float.IsFinite(left+right+top+bottom))return true;
            float padX=(VisualCanvasShadowViewport.Padding(_state.ShadowBlur,_state.ShadowX)+2f)*_viewport.width/_target.width;
            float padY=(VisualCanvasShadowViewport.Padding(_state.ShadowBlur,_state.ShadowY)+2f)*_viewport.height/_target.height;
            return right+padX>=_viewport.xMin&&left-padX<=_viewport.xMax&&top+padY>=_viewport.yMin&&bottom-padY<=_viewport.yMax;
        }
        internal void DrawNativeTextMesh(Mesh mesh,Material material,MaterialPropertyBlock properties,Action prepareGlyphs,Action emitGlyphs,Color? shadow=null,bool inheritShadowOffsets=false,Matrix4x4? localTransform=null,float shadowBlur=0,bool inheritBlend=false,bool additive=false,bool inheritShadow=false,float? opacity=null)
        {DrawNativeMesh(mesh,material,properties,localTransform,shadow:shadow,shadowBlur:shadowBlur,inheritShadowOffsets:inheritShadowOffsets,inheritBlend:inheritBlend,additive:additive,inheritShadow:inheritShadow,opacity:opacity);var batch=_batches[_batches.Count-1];batch.NativePrepareGlyphs=prepareGlyphs;batch.NativeEmitGlyphs=emitGlyphs;}
        private RectInt ShadowArea(Batch batch)
        {
            int px=VisualCanvasShadowViewport.Padding(batch.Blur,batch.X),py=VisualCanvasShadowViewport.Padding(batch.Blur,batch.Y);
            var full=new RectInt(-px,-py,_target.width+px*2,_target.height+py*2);
#if UNITY_EDITOR
            if(FullShadowViewportForValidation)return full;
#endif
            // Unknown vertex deformation must retain the old full viewport.
            var shader=batch.NativeMaterial.shader;
            if(!_boundedShaders.TryGetValue(shader,out var bounded)){
            switch(shader.name){
                case "EchoSphere/NativeWorldSolid":case "EchoSphere/NativeWorldLabel":case "EchoSphere/NativeWorldSprite":
                case "EchoSphere/NativeWorldCreatureGlow":case "EchoSphere/NativeWorldFamilyGlow":case "EchoSphere/NativeWorldMutationGlow":
                case "EchoSphere/NativeWorldPlayerCore":case "EchoSphere/NativeWorldStandardCore":case "EchoSphere/NativeWorldRegion":
                case "EchoSphere/NativeWorldField":case "EchoSphere/NativeWorldFrameBackground":bounded=true;break;
                default:bounded=false;break;
            }
            _boundedShaders.Add(shader,bounded);}
            if(!bounded)return full;
            var bounds=batch.NativeMesh.bounds;var offset=shader==_labelShader?batch.NativeProperties.GetVector(LabelOffsetId):Vector4.zero;
            float x0=float.PositiveInfinity,y0=float.PositiveInfinity,x1=float.NegativeInfinity,y1=float.NegativeInfinity;
            for(int i=0;i<4;i++){
                var point=batch.NativeMatrix.MultiplyPoint3x4(new Vector3((i%2==0?bounds.min.x:bounds.max.x)+offset.x,(i<2?bounds.min.y:bounds.max.y)+offset.y,bounds.center.z));
                float x=(point.x-_viewport.xMin)/_viewport.width*_target.width,y=(point.y-_viewport.yMin)/_viewport.height*_target.height;
                x0=Mathf.Min(x0,x);x1=Mathf.Max(x1,x);y0=Mathf.Min(y0,y);y1=Mathf.Max(y1,y);
            }
            if(!float.IsFinite(x0+y0+x1+y1))return full;
            // Integer pixel origin keeps the MSAA/sample grid identical to the
            // original target. Include the complete kernel and bilinear margin.
            int left=Math.Max(full.xMin,Mathf.FloorToInt(x0)-px-1),bottom=Math.Max(full.yMin,Mathf.FloorToInt(y0)-py-1);
            int right=Math.Min(full.xMax,Mathf.CeilToInt(x1)+px+1),top=Math.Min(full.yMax,Mathf.CeilToInt(y1)+py+1);
            return new RectInt(left,bottom,Math.Max(0,right-left),Math.Max(0,top-bottom));
        }
        private readonly struct ShadowPlacement
        {
            internal readonly RectInt Area;internal readonly Matrix4x4 Matrix;internal readonly Vector2 Translation;internal readonly bool Local;
            internal ShadowPlacement(RectInt area,Matrix4x4 matrix,Vector2 translation=default,bool local=false){Area=area;Matrix=matrix;Translation=translation;Local=local;}
        }
        // A complete local mask follows the object without rerasterizing at each
        // world position. Rotation/scale/geometry/alpha still invalidate the mask.
        // Very large or unblurred shapes retain the exact viewport raster path.
        private ShadowPlacement PlaceShadow(Batch batch,RectInt visibleArea)
        {
            var fallback=new ShadowPlacement(visibleArea,batch.NativeMatrix);
#if UNITY_EDITOR
            if(FullShadowViewportForValidation||DisableShadowCacheForValidation)return fallback;
#endif
            if(batch.Blur<2||batch.NativeMaterial.shader!=_solidShader||NativeWorldShapes.GeometryVersion(batch.NativeMesh)==0)return fallback;
            var matrix=batch.NativeMatrix;var shift=new Vector2(matrix.m03/_viewport.width*_target.width,matrix.m13/_viewport.height*_target.height);matrix.m03=matrix.m13=0;
            var bounds=batch.NativeMesh.bounds;float x0=float.PositiveInfinity,y0=float.PositiveInfinity,x1=float.NegativeInfinity,y1=float.NegativeInfinity;
            for(int i=0;i<4;i++){
                var point=matrix.MultiplyPoint3x4(new Vector3(i%2==0?bounds.min.x:bounds.max.x,i<2?bounds.min.y:bounds.max.y,bounds.center.z));
                float x=(point.x-_viewport.xMin)/_viewport.width*_target.width,y=(point.y-_viewport.yMin)/_viewport.height*_target.height;
                x0=Mathf.Min(x0,x);x1=Mathf.Max(x1,x);y0=Mathf.Min(y0,y);y1=Mathf.Max(y1,y);
            }
            if(!float.IsFinite(x0+y0+x1+y1+shift.x+shift.y))return fallback;
            int px=VisualCanvasShadowViewport.Padding(batch.Blur,batch.X)+2,py=VisualCanvasShadowViewport.Padding(batch.Blur,batch.Y)+2;
            int left=Mathf.FloorToInt(x0)-px,bottom=Mathf.FloorToInt(y0)-py,right=Mathf.CeilToInt(x1)+px,top=Mathf.CeilToInt(y1)+py;
            long pixels=(long)(right-left)*(top-bottom);
            if(pixels<=0||pixels>256*1024||right-left>SystemInfo.maxTextureSize||top-bottom>SystemInfo.maxTextureSize)return fallback;
            return new ShadowPlacement(new RectInt(left,bottom,right-left,top-bottom),matrix,shift,true);
        }
        private Rect ShadowCompositeRect(Batch batch,ShadowPlacement placement)
        {
            var area=placement.Area;var shift=placement.Translation;
            int mx=Mathf.CeilToInt(Mathf.Abs(batch.X))+2,my=Mathf.CeilToInt(Mathf.Abs(batch.Y))+2;
            float left=Mathf.Clamp(area.xMin+shift.x-mx,0,_target.width),bottom=Mathf.Clamp(area.yMin+shift.y-my,0,_target.height);
            float right=Mathf.Clamp(area.xMax+shift.x+mx,0,_target.width),top=Mathf.Clamp(area.yMax+shift.y+my,0,_target.height);
            return new Rect(left,bottom,right-left,top-bottom);
        }
        private void SetShadowKernel(CommandBuffer commands,float sigma)
        {
            commands.SetGlobalFloat(ShadowSigmaId,sigma);
            int radius=Mathf.CeilToInt(sigma*4);
            bool cached=NativeGaussianWeights.TryGet(sigma,out var weights);
#if UNITY_EDITOR
            if(FullShadowViewportForValidation)cached=false;
#endif
            if(!cached){commands.SetGlobalInt(ShadowWeightCountId,0);return;}
            commands.SetGlobalInt(ShadowWeightCountId,radius+1);commands.SetGlobalVectorArray(ShadowWeightsId,weights);
        }
        private int DrawInstanceRun(CommandBuffer commands,int first,int pass,int end=int.MaxValue)
        {
            if(!_canInstance||_batches[first].NativeMaterial.shader!=_solidShader)return 0;
#if UNITY_EDITOR
            if(DisableInstancingForValidation)return 0;
#endif
            var initial=_batches[first];int count=0;
            for(int i=first;i<_batches.Count&&i<end&&count<64;i++){
                var batch=_batches[i];
                if(batch.NativeMesh!=initial.NativeMesh||batch.Blend!=initial.Blend||batch.NativeMaterial.shader!=_solidShader||batch.Shadow.a>0&&(batch.Blur>0||batch.X!=0||batch.Y!=0)||!batch.NativeProperties.HasVector(NativeColorId))break;
                _instanceMatrices[count]=batch.NativeMatrix;_instanceColors[count]=batch.NativeProperties.GetVector(NativeColorId);_instanceOpacities[count]=batch.NativeProperties.GetFloat(NativeOpacityId);count++;
            }
            if(count<2)return 0;
            _instanceProperties.SetVectorArray(NativeColorId,_instanceColors);_instanceProperties.SetFloatArray(NativeOpacityId,_instanceOpacities);
            commands.DrawMeshInstanced(initial.NativeMesh,0,_solidInstances,pass,_instanceMatrices,count,_instanceProperties);
            GeometryDrawCalls++;InstancedPrimitiveCount+=count;return count;
        }
        private void RenderMeshes(CommandBuffer recorder=null)
        {
            if(_target==null)throw new InvalidOperationException("Native world viewport is not set");
            var commands=recorder??_commands;if(recorder==null)commands.Clear();commands.SetGlobalFloat(ShadowAtlasId,0);commands.SetRenderTarget(_target);commands.ClearRenderTarget(false,true,Color.clear);
            // This texture is sampled directly by UI Toolkit, not presented by a Camera.
            // Camera render-target Y inversion would mirror both the world and its labels.
            var projection=GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(_viewport.xMin,_viewport.xMax,_viewport.yMin,_viewport.yMax,-1f,1f),false);
            commands.SetViewProjectionMatrices(Matrix4x4.identity,projection);
            var mask=ShadowMaskId;var ping=ShadowPingId;var blur=ShadowBlurId;
            ShadowPixelCount=0;ShadowPassCount=0;ShadowCompositePixelCount=0;ShadowCompositeCount=0;CulledBatchCount=0;ShadowCacheHits=0;GeometryDrawCalls=0;InstancedPrimitiveCount=0;_shadowCache.Begin();
            var pixelX=_viewport.width/_target.width;var pixelY=_viewport.height/_target.height;
            for(var i=0;i<_batches.Count;i++) {
                var batch=_batches[i];
                if(batch.NativeMesh==null)continue;var props=batch.NativeProperties;
                // Reuse the conservative geometry+shadow support calculation.
                // Unknown vertex-deforming shaders return the complete viewport
                // and are never culled. Simulation/pose updates stay untouched.
                var area=ShadowArea(batch);
                bool cull=area.width<=0||area.height<=0;
#if UNITY_EDITOR
                if(DisableBatchCullingForValidation)cull=false;
#endif
                if(cull){CulledBatchCount++;continue;}
                if(batch.Shadow.a>0f&&(batch.Blur>0f||batch.X!=0f||batch.Y!=0f)) {
                    using var shadowSample=ShadowCommandsMarker.Auto();
                    if(area.width>0&&area.height>0){
                    var placement=PlaceShadow(batch,area);area=placement.Area;
                    RenderTexture retained=null;var cacheResult=NativeWorldShadowCache.Result.Miss;
                    bool cacheAllowed=batch.NativeMaterial.shader==_solidShader&&NativeWorldShapes.GeometryVersion(batch.NativeMesh)!=0&&props.HasVector(NativeColorId);
#if UNITY_EDITOR
                    if(FullShadowViewportForValidation||DisableShadowCacheForValidation)cacheAllowed=false;
#endif
                    if(cacheAllowed){
                        var key=new NativeWorldShadowCache.Key(batch.NativeMesh,NativeWorldShapes.GeometryVersion(batch.NativeMesh),placement.Matrix,_viewport,area,props.GetVector(NativeColorId).w,props.GetFloat(NativeOpacityId),batch.Blur);
                        cacheResult=_shadowCache.Probe(key,area.width,area.height,out retained);
                    }
                    if(cacheResult!=NativeWorldShadowCache.Result.Hit){
                    ShadowPixelCount+=(long)area.width*area.height;ShadowPassCount++;
                    var shadowProjection=GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(_viewport.xMin+area.xMin*pixelX,_viewport.xMin+area.xMax*pixelX,_viewport.yMin+area.yMin*pixelY,_viewport.yMin+area.yMax*pixelY,-1f,1f),false);
                    // Unity's temporary target pool reuses released sizes; no
                    // permanent per-object textures are retained for moving FX.
                    commands.GetTemporaryRT(mask,area.width,area.height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear,4);
                    commands.GetTemporaryRT(ping,area.width,area.height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
                    commands.GetTemporaryRT(blur,area.width,area.height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
                    commands.SetRenderTarget(mask);commands.ClearRenderTarget(false,true,Color.clear);commands.SetViewProjectionMatrices(Matrix4x4.identity,shadowProjection);
                    commands.DrawMesh(batch.NativeMesh,placement.Matrix,batch.NativeMaterial,0,0,props);
                    SetShadowKernel(commands,(float)VisualCanvasShadowViewport.Sigma(batch.Blur));
                    commands.SetGlobalVector(ShadowDirectionId,new Vector4(1f/area.width,0f,0f,0f));commands.Blit(mask,ping,_blurMaterial,0);
                    commands.SetGlobalVector(ShadowDirectionId,new Vector4(0f,1f/area.height,0f,0f));commands.Blit(ping,blur,_blurMaterial,0);
                    if(cacheResult==NativeWorldShadowCache.Result.Write)commands.CopyTexture(new RenderTargetIdentifier(blur),new RenderTargetIdentifier(retained));
                    }else ShadowCacheHits++;
                    commands.SetGlobalColor(ShadowColorId,batch.Shadow);
                    commands.SetGlobalVector(ShadowOffsetId,new Vector4(batch.X/area.width,-batch.Y/area.height,0f,0f));
                    commands.SetGlobalVector(ShadowRectId,new Vector4(-(area.x+placement.Translation.x)/area.width,-(area.y+placement.Translation.y)/area.height,(float)_target.width/area.width,(float)_target.height/area.height));
                    // Keep the original full-target UV mapping and blend order,
                    // but rasterize only the nonzero support of this shadow.
                    // Include offset and bilinear/MSAA edge coverage. This also
                    // bounds cache-hit composites, which still need blending.
                    var composite=ShadowCompositeRect(batch,placement);
#if UNITY_EDITOR
                    if(FullShadowCompositeForValidation)composite=new Rect(0,0,_target.width,_target.height);
#endif
                    if(composite.width>0&&composite.height>0){
                        commands.EnableScissorRect(composite);
                        commands.Blit(retained!=null?new RenderTargetIdentifier(retained):new RenderTargetIdentifier(blur),_target,_blurMaterial,batch.Blend=="lighter"?2:batch.Blend=="screen"?3:1);
                        commands.DisableScissorRect();
                        ShadowCompositeCount++;ShadowCompositePixelCount+=(long)composite.width*(long)composite.height;
                    }
                    if(cacheResult!=NativeWorldShadowCache.Result.Hit){commands.ReleaseTemporaryRT(mask);commands.ReleaseTemporaryRT(ping);commands.ReleaseTemporaryRT(blur);}
                    }
                    commands.SetViewProjectionMatrices(Matrix4x4.identity,projection);
                }
                commands.SetRenderTarget(_target);int pass=batch.Blend=="lighter"?1:batch.Blend=="screen"?2:0;
                int instances=DrawInstanceRun(commands,i,pass);if(instances>0)i+=instances-1;
                else{commands.DrawMesh(batch.NativeMesh,batch.NativeMatrix,batch.NativeMaterial,0,pass,props);GeometryDrawCalls++;}
            }
            commands.SetGlobalInt(ShadowWeightCountId,0); // Do not leak cached weights into Editor reference compositors.
            if(recorder!=null)return;
            var previous=RenderTexture.active;var gamma=GL.sRGBWrite;
            try{commands.SetRenderTarget(previous!=null?new RenderTargetIdentifier(previous):new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));using(ExecuteMarker.Auto())Graphics.ExecuteCommandBuffer(commands);}
            finally{RenderTexture.active=previous;GL.sRGBWrite=gamma;}
        }
        private static void DestroyOwned(UnityEngine.Object value){if(Application.isPlaying)UnityEngine.Object.Destroy(value);else UnityEngine.Object.DestroyImmediate(value);}
        private static void Release(ref RenderTexture target){if(target==null)return;target.Release();DestroyOwned(target);target=null;}
        private void OnDestroy(){_renderGraphLease?.Dispose();_renderGraphLease=null;_graphTarget?.Release();_graphUi?.Release();_commands?.Release();_shadowCache.Dispose();Release(ref _target);Release(ref _uiTarget);if(_blurMaterial!=null)DestroyOwned(_blurMaterial);if(_presentMaterial!=null)DestroyOwned(_presentMaterial);}
    }
}
