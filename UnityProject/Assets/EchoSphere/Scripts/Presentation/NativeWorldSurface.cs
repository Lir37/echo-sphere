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
    internal sealed class NativeWorldSurface : MonoBehaviour
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
#if UNITY_EDITOR
        internal bool FullShadowViewportForValidation=false;
#endif
        private CommandBuffer _commands;private Material _blurMaterial,_presentMaterial;private RenderTexture _target,_uiTarget;private Rect _viewport;private float _dpr=1;private Vector2 _logicalSize;
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
        internal void Initialize()
        {_commands=new CommandBuffer {name="ECHO SPHERE native world"};_blurMaterial=new Material(Resources.Load<Shader>("EchoSphere/SourceCanvasShadow"));_presentMaterial=new Material(Resources.Load<Shader>("EchoSphere/SourceCanvasPresent"));}
        internal void SetViewport(float width,float height,float dpr)
        {
            width=Mathf.Max(1,width);height=Mathf.Max(1,height);_logicalSize=new Vector2(width,height);_viewport=new Rect(-width*.5f*SpherePlacementRules.SourcePixelsToWorld,-height*.5f*SpherePlacementRules.SourcePixelsToWorld,width*SpherePlacementRules.SourcePixelsToWorld,height*SpherePlacementRules.SourcePixelsToWorld);
            double density=Math.Min(1.75,Math.Max(1,float.IsNaN(dpr)||float.IsInfinity(dpr)?1:dpr));_dpr=(float)density;
            int w=Math.Max(1,(int)Math.Floor(width*density+.5)),h=Math.Max(1,(int)Math.Floor(height*density+.5));if(_target!=null&&_target.width==w&&_target.height==h)return;
            Release(ref _target);Release(ref _uiTarget);
            _target=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear){name="Native world compositing",antiAliasing=4,filterMode=FilterMode.Bilinear};_target.Create();
            _uiTarget=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name="Native world UI image",filterMode=FilterMode.Bilinear};_uiTarget.Create();
        }
        internal void Begin(Vector2 origin){SourceOrigin=origin;while(_stack.Count>0)_freeStates.Push(_stack.Pop());_batches.Clear();_batchUsed=0;}
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
            using var frame=FrameMarker.Auto();bool dirty=false;void Rebuilt(Font font){dirty=true;}Font.textureRebuilt+=Rebuilt;
            try{int attempts=0;do{dirty=false;foreach(var batch in _batches)batch.NativePrepareGlyphs?.Invoke();if(++attempts>8&&dirty)throw new InvalidOperationException("Native font atlas could not retain the frame glyphs");}while(dirty);}finally{Font.textureRebuilt-=Rebuilt;}
            foreach(var batch in _batches)batch.NativeEmitGlyphs?.Invoke();RenderMeshes();
            var previous=RenderTexture.active;var gamma=GL.sRGBWrite;try{GL.sRGBWrite=QualitySettings.activeColorSpace==ColorSpace.Linear;Graphics.Blit(_target,_uiTarget,_presentMaterial,2);}finally{RenderTexture.active=previous;GL.sRGBWrite=gamma;}
        }
        internal void DrawNativeMesh(Mesh mesh,Material material,MaterialPropertyBlock properties,Matrix4x4? localTransform=null,Color? shadow=null,float shadowBlur=0,bool inheritBlend=false,float? opacity=null,bool inheritShadow=false,bool screen=false,bool additive=false,bool inheritShadowOffsets=false)
        {
            float unit=SpherePlacementRules.SourcePixelsToWorld;
            properties.SetFloat("_NativeOpacity",opacity??_state.Alpha);
            var matrix=Matrix4x4.Translate(new Vector3(-SourceOrigin.x*unit,SourceOrigin.y*unit,0))*Matrix4x4.Scale(new Vector3(unit,-unit,1))*_state.Matrix*(localTransform??Matrix4x4.identity);
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
            string shader=batch.NativeMaterial.shader.name;
            switch(shader){
                case "EchoSphere/NativeWorldSolid":case "EchoSphere/NativeWorldLabel":case "EchoSphere/NativeWorldSprite":
                case "EchoSphere/NativeWorldCreatureGlow":case "EchoSphere/NativeWorldFamilyGlow":case "EchoSphere/NativeWorldMutationGlow":
                case "EchoSphere/NativeWorldPlayerCore":case "EchoSphere/NativeWorldStandardCore":case "EchoSphere/NativeWorldRegion":
                case "EchoSphere/NativeWorldField":case "EchoSphere/NativeWorldFrameBackground":break;
                default:return full;
            }
            var bounds=batch.NativeMesh.bounds;var offset=shader=="EchoSphere/NativeWorldLabel"?batch.NativeProperties.GetVector("_LabelOffset"):Vector4.zero;
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
        private void SetShadowKernel(float sigma)
        {
            _commands.SetGlobalFloat(ShadowSigmaId,sigma);
            int radius=Mathf.CeilToInt(sigma*4);
            bool cached=NativeGaussianWeights.TryGet(sigma,out var weights);
#if UNITY_EDITOR
            if(FullShadowViewportForValidation)cached=false;
#endif
            if(!cached){_commands.SetGlobalInt(ShadowWeightCountId,0);return;}
            _commands.SetGlobalInt(ShadowWeightCountId,radius+1);_commands.SetGlobalVectorArray(ShadowWeightsId,weights);
        }
        private void RenderMeshes()
        {
            if(_target==null)throw new InvalidOperationException("Native world viewport is not set");
            _commands.Clear();_commands.SetRenderTarget(_target);_commands.ClearRenderTarget(false,true,Color.clear);
            // This texture is sampled directly by UI Toolkit, not presented by a Camera.
            // Camera render-target Y inversion would mirror both the world and its labels.
            var projection=GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(_viewport.xMin,_viewport.xMax,_viewport.yMin,_viewport.yMax,-1f,1f),false);
            _commands.SetViewProjectionMatrices(Matrix4x4.identity,projection);
            var mask=ShadowMaskId;var ping=ShadowPingId;var blur=ShadowBlurId;
            ShadowPixelCount=0;ShadowPassCount=0;
            var pixelX=_viewport.width/_target.width;var pixelY=_viewport.height/_target.height;
            for(var i=0;i<_batches.Count;i++) {
                var batch=_batches[i];
                if(batch.NativeMesh==null)continue;var props=batch.NativeProperties;
                if(batch.Shadow.a>0f&&(batch.Blur>0f||batch.X!=0f||batch.Y!=0f)) {
                    using var shadowSample=ShadowCommandsMarker.Auto();
                    var area=ShadowArea(batch);
                    if(area.width>0&&area.height>0){
                    ShadowPixelCount+=(long)area.width*area.height;ShadowPassCount++;
                    var shadowProjection=GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(_viewport.xMin+area.xMin*pixelX,_viewport.xMin+area.xMax*pixelX,_viewport.yMin+area.yMin*pixelY,_viewport.yMin+area.yMax*pixelY,-1f,1f),false);
                    // Unity's temporary target pool reuses released sizes; no
                    // permanent per-object textures are retained for moving FX.
                    _commands.GetTemporaryRT(mask,area.width,area.height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear,4);
                    _commands.GetTemporaryRT(ping,area.width,area.height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
                    _commands.GetTemporaryRT(blur,area.width,area.height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
                    _commands.SetRenderTarget(mask);_commands.ClearRenderTarget(false,true,Color.clear);_commands.SetViewProjectionMatrices(Matrix4x4.identity,shadowProjection);
                    _commands.DrawMesh(batch.NativeMesh,batch.NativeMatrix,batch.NativeMaterial,0,0,props);
                    SetShadowKernel((float)VisualCanvasShadowViewport.Sigma(batch.Blur));
                    _commands.SetGlobalVector(ShadowDirectionId,new Vector4(1f/area.width,0f,0f,0f));_commands.Blit(mask,ping,_blurMaterial,0);
                    _commands.SetGlobalVector(ShadowDirectionId,new Vector4(0f,1f/area.height,0f,0f));_commands.Blit(ping,blur,_blurMaterial,0);
                    _commands.SetGlobalColor(ShadowColorId,batch.Shadow);
                    _commands.SetGlobalVector(ShadowOffsetId,new Vector4(batch.X/area.width,-batch.Y/area.height,0f,0f));
                    _commands.SetGlobalVector(ShadowRectId,new Vector4(-(float)area.x/area.width,-(float)area.y/area.height,(float)_target.width/area.width,(float)_target.height/area.height));
                    _commands.Blit(blur,_target,_blurMaterial,batch.Blend=="lighter"?2:batch.Blend=="screen"?3:1);
                    _commands.ReleaseTemporaryRT(mask);_commands.ReleaseTemporaryRT(ping);_commands.ReleaseTemporaryRT(blur);
                    }
                    _commands.SetViewProjectionMatrices(Matrix4x4.identity,projection);
                }
                _commands.SetRenderTarget(_target);int pass=batch.Blend=="lighter"?1:batch.Blend=="screen"?2:0;_commands.DrawMesh(batch.NativeMesh,batch.NativeMatrix,batch.NativeMaterial,0,pass,props);
            }
            _commands.SetGlobalInt(ShadowWeightCountId,0); // Do not leak cached weights into Editor reference compositors.
            var previous=RenderTexture.active;var gamma=GL.sRGBWrite;
            try{_commands.SetRenderTarget(previous!=null?new RenderTargetIdentifier(previous):new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));using(ExecuteMarker.Auto())Graphics.ExecuteCommandBuffer(_commands);}
            finally{RenderTexture.active=previous;GL.sRGBWrite=gamma;}
        }
        private static void Release(ref RenderTexture target){if(target==null)return;target.Release();UnityEngine.Object.Destroy(target);target=null;}
        private void OnDestroy(){_commands?.Release();Release(ref _target);Release(ref _uiTarget);if(_blurMaterial!=null)Destroy(_blurMaterial);if(_presentMaterial!=null)Destroy(_presentMaterial);}
    }
}
