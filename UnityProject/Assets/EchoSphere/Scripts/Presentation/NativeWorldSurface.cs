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
            internal Matrix4x4 Matrix=Matrix4x4.identity;internal float Alpha=1,ShadowBlur,ShadowX,ShadowY,DashOffset,Miter=10;
            internal string Blend="source-over",Baseline="alphabetic";internal Color Shadow=Color.clear;
            internal float[] Dash=Array.Empty<float>();internal State Copy()=>(State)MemberwiseClone();
        }
        private sealed class Batch
        {
            internal Mesh NativeMesh;internal Material NativeMaterial;internal Matrix4x4 NativeMatrix;
            internal MaterialPropertyBlock NativeProperties;internal string Blend;internal Color Shadow;internal float Blur,X,Y;
            internal Action NativePrepareGlyphs,NativeEmitGlyphs;
        }
        private State _state=new State();private readonly Stack<State> _stack=new Stack<State>();private readonly List<Batch> _batches=new List<Batch>();
        private CommandBuffer _commands;private Material _blurMaterial,_presentMaterial;private RenderTexture _target,_uiTarget;private Rect _viewport;private float _dpr=1;
        internal Vector2 SourceOrigin;internal bool LocalizeCanvasText=true;internal Texture UiTexture=>_uiTarget;
        private static readonly ProfilerMarker ShadowCommandsMarker=new ProfilerMarker("EchoSphere.World.ShadowCommands"),ExecuteMarker=new ProfilerMarker("EchoSphere.World.Execute"),FrameMarker=new ProfilerMarker("EchoSphere.World.Composite");
        private static readonly int ShadowMaskId=Shader.PropertyToID("_SourceShadowMask");
        private static readonly int ShadowPingId=Shader.PropertyToID("_SourceShadowPing");
        private static readonly int ShadowBlurId=Shader.PropertyToID("_SourceShadowBlur");
        private static readonly int ShadowSigmaId=Shader.PropertyToID("_SourceShadowSigma");
        private static readonly int ShadowDirectionId=Shader.PropertyToID("_SourceShadowDirection");
        private static readonly int ShadowColorId=Shader.PropertyToID("_SourceShadowColor");
        private static readonly int ShadowOffsetId=Shader.PropertyToID("_SourceShadowOffset");
        private static readonly int ShadowRectId=Shader.PropertyToID("_SourceShadowRect");
        internal void Initialize()
        {_commands=new CommandBuffer {name="ECHO SPHERE native world"};_blurMaterial=new Material(Resources.Load<Shader>("EchoSphere/SourceCanvasShadow"));_presentMaterial=new Material(Resources.Load<Shader>("EchoSphere/SourceCanvasPresent"));}
        internal void SetViewport(float width,float height,float dpr)
        {
            width=Mathf.Max(1,width);height=Mathf.Max(1,height);_viewport=new Rect(-width*.5f*SpherePlacementRules.SourcePixelsToWorld,-height*.5f*SpherePlacementRules.SourcePixelsToWorld,width*SpherePlacementRules.SourcePixelsToWorld,height*SpherePlacementRules.SourcePixelsToWorld);
            double density=Math.Min(1.75,Math.Max(1,float.IsNaN(dpr)||float.IsInfinity(dpr)?1:dpr));_dpr=(float)density;
            int w=Math.Max(1,(int)Math.Floor(width*density+.5)),h=Math.Max(1,(int)Math.Floor(height*density+.5));if(_target!=null&&_target.width==w&&_target.height==h)return;
            Release(ref _target);Release(ref _uiTarget);
            _target=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear){name="Native world compositing",antiAliasing=4,filterMode=FilterMode.Bilinear};_target.Create();
            _uiTarget=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name="Native world UI image",filterMode=FilterMode.Bilinear};_uiTarget.Create();
        }
        internal void Begin(Vector2 origin){SourceOrigin=origin;_stack.Clear();_batches.Clear();}
        internal void Save()=>_stack.Push(_state.Copy());
        internal void Restore(){if(_stack.Count>0)_state=_stack.Pop();}
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
            var previous=RenderTexture.active;var gamma=GL.sRGBWrite;try{GL.sRGBWrite=QualitySettings.activeColorSpace==ColorSpace.Linear;Graphics.Blit(_target,_uiTarget,_presentMaterial,1);}finally{RenderTexture.active=previous;GL.sRGBWrite=gamma;}
        }
        internal void DrawNativeMesh(Mesh mesh,Material material,MaterialPropertyBlock properties,Matrix4x4? localTransform=null,Color? shadow=null,float shadowBlur=0,bool inheritBlend=false,float? opacity=null,bool inheritShadow=false,bool screen=false,bool additive=false,bool inheritShadowOffsets=false)
        {
            float unit=SpherePlacementRules.SourcePixelsToWorld;
            properties.SetFloat("_NativeOpacity",opacity??_state.Alpha);
            var matrix=Matrix4x4.Translate(new Vector3(-SourceOrigin.x*unit,SourceOrigin.y*unit,0))*Matrix4x4.Scale(new Vector3(unit,-unit,1))*_state.Matrix*(localTransform??Matrix4x4.identity);
            _batches.Add(new Batch {NativeMesh=mesh,NativeMaterial=material,NativeMatrix=matrix,NativeProperties=properties,Blend=additive?"lighter":screen?"screen":inheritBlend?_state.Blend:"source-over",Shadow=inheritShadow?_state.Shadow:shadow??Color.clear,Blur=inheritShadow?_state.ShadowBlur:shadowBlur,X=inheritShadow||inheritShadowOffsets?_state.ShadowX:0,Y=inheritShadow||inheritShadowOffsets?_state.ShadowY:0});
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
        private void RenderMeshes()
        {
            if(_target==null)throw new InvalidOperationException("Native world viewport is not set");
            _commands.Clear();_commands.SetRenderTarget(_target);_commands.ClearRenderTarget(false,true,Color.clear);
            var projection=GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(_viewport.xMin,_viewport.xMax,_viewport.yMin,_viewport.yMax,-1f,1f),true);
            _commands.SetViewProjectionMatrices(Matrix4x4.identity,projection);
            var mask=ShadowMaskId;var ping=ShadowPingId;var blur=ShadowBlurId;
            var hasShadows=false;var paddingX=0;var paddingY=0;foreach(var batch in _batches)if(batch.NativeMesh!=null&&batch.Shadow.a>0f&&(batch.Blur>0f||batch.X!=0f||batch.Y!=0f)){hasShadows=true;paddingX=Math.Max(paddingX,VisualCanvasShadowViewport.Padding(batch.Blur,batch.X));paddingY=Math.Max(paddingY,VisualCanvasShadowViewport.Padding(batch.Blur,batch.Y));}
            var shadowViewport=new VisualCanvasShadowViewport(_target.width,_target.height,paddingX,paddingY);
            var pixelX=_viewport.width/_target.width;var pixelY=_viewport.height/_target.height;
            var shadowProjection=GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(_viewport.xMin-paddingX*pixelX,_viewport.xMax+paddingX*pixelX,_viewport.yMin-paddingY*pixelY,_viewport.yMax+paddingY*pixelY,-1f,1f),true);
            if(hasShadows){_commands.GetTemporaryRT(mask,shadowViewport.Width,shadowViewport.Height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear,4);
            _commands.GetTemporaryRT(ping,shadowViewport.Width,shadowViewport.Height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
            _commands.GetTemporaryRT(blur,shadowViewport.Width,shadowViewport.Height,0,FilterMode.Bilinear,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);}
            for(var i=0;i<_batches.Count;i++) {
                var batch=_batches[i];
                if(batch.NativeMesh==null)continue;var props=batch.NativeProperties;
                if(batch.Shadow.a>0f&&(batch.Blur>0f||batch.X!=0f||batch.Y!=0f)) {
                    using var shadowSample=ShadowCommandsMarker.Auto();
                    _commands.SetRenderTarget(mask);_commands.ClearRenderTarget(false,true,Color.clear);_commands.SetViewProjectionMatrices(Matrix4x4.identity,shadowProjection);
                    _commands.DrawMesh(batch.NativeMesh,batch.NativeMatrix,batch.NativeMaterial,0,0,props);
                    _commands.SetGlobalFloat(ShadowSigmaId,(float)VisualCanvasShadowViewport.Sigma(batch.Blur));
                    _commands.SetGlobalVector(ShadowDirectionId,new Vector4(1f/shadowViewport.Width,0f,0f,0f));_commands.Blit(mask,ping,_blurMaterial,0);
                    _commands.SetGlobalVector(ShadowDirectionId,new Vector4(0f,1f/shadowViewport.Height,0f,0f));_commands.Blit(ping,blur,_blurMaterial,0);
                    _commands.SetGlobalColor(ShadowColorId,batch.Shadow);
                    _commands.SetGlobalVector(ShadowOffsetId,new Vector4(batch.X/shadowViewport.Width,-batch.Y/shadowViewport.Height,0f,0f));
                    _commands.SetGlobalVector(ShadowRectId,new Vector4((float)shadowViewport.U,(float)shadowViewport.V,(float)shadowViewport.UWidth,(float)shadowViewport.VHeight));
                    _commands.Blit(blur,_target,_blurMaterial,batch.Blend=="lighter"?2:batch.Blend=="screen"?3:1);
                    _commands.SetViewProjectionMatrices(Matrix4x4.identity,projection);
                }
                _commands.SetRenderTarget(_target);int pass=batch.Blend=="lighter"?1:batch.Blend=="screen"?2:0;_commands.DrawMesh(batch.NativeMesh,batch.NativeMatrix,batch.NativeMaterial,0,pass,props);
            }
            var previous=RenderTexture.active;var gamma=GL.sRGBWrite;
            try{if(hasShadows){_commands.ReleaseTemporaryRT(mask);_commands.ReleaseTemporaryRT(ping);_commands.ReleaseTemporaryRT(blur);}_commands.SetRenderTarget(previous!=null?new RenderTargetIdentifier(previous):new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));using(ExecuteMarker.Auto())Graphics.ExecuteCommandBuffer(_commands);}
            finally{RenderTexture.active=previous;GL.sRGBWrite=gamma;}
        }
        private static void Release(ref RenderTexture target){if(target==null)return;target.Release();UnityEngine.Object.Destroy(target);target=null;}
        private void OnDestroy(){_commands?.Release();Release(ref _target);Release(ref _uiTarget);if(_blurMaterial!=null)Destroy(_blurMaterial);if(_presentMaterial!=null)Destroy(_presentMaterial);}
    }
}
