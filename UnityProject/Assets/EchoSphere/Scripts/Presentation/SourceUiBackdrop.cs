using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Executes in UI paint order, before the corresponding element. Capture the
    // already painted framebuffer so backdrop blur includes the world and lower
    // UI layers, including HUD panels underneath the pause / result overlays.
    internal sealed class SourceUiBackdrop : ImmediateModeElement,IDisposable
    {
        private readonly SourceUiDocument _document;
        private readonly Material _material;
        private readonly CommandBuffer _commands=new CommandBuffer {name="Source CSS backdrop-filter"};
        private RenderTexture _horizontal,_result;
        private int _captureWidth,_captureHeight;
        private Texture2D _mask;
        private string _maskKey;
        private int _frame=-1;
        private VisualCssNode _node;
        private VisualElement _original;
        private float _sigma;
        internal Texture Texture=>_result;
        internal SourceUiBackdrop(SourceUiDocument document)
        {
            _document=document;_material=new Material(Resources.Load<Shader>("EchoSphere/SourceUiBackdrop"));pickingMode=PickingMode.Ignore;
        }
        internal void Update(VisualCssNode node,VisualElement original,float sigma)
        {
            _node=node;_original=original;_sigma=sigma;var box=original.layout;style.position=Position.Absolute;style.left=box.x;style.top=box.y;style.width=box.width;style.height=box.height;style.marginLeft=0;style.marginTop=0;
            style.translate=original.style.translate;style.rotate=original.style.rotate;style.scale=original.style.scale;style.transformOrigin=original.style.transformOrigin;
            style.display=original.style.display;style.visibility=original.style.visibility;if(node.Width>0&&node.Height>0)PrepareTargets();MarkDirtyRepaint();
        }
        private bool PrepareTargets()
        {
            var dpr=_document.Scale;var width=Mathf.Max(1,Mathf.CeilToInt((float)_node.Width*dpr));var height=Mathf.Max(1,Mathf.CeilToInt((float)_node.Height*dpr));
            var resized=_captureWidth!=Screen.width||_captureHeight!=Screen.height||_result==null||_result.width!=width||_result.height!=height;
            var padding=Mathf.CeilToInt(_sigma*dpr*4f);resized|=_horizontal!=null&&_horizontal.height!=height+padding*2;
            if(resized){ReleaseTargets();_frame=-1;_captureWidth=Screen.width;_captureHeight=Screen.height;_horizontal=Target(width,height+padding*2,RenderTextureReadWrite.Linear);_result=Target(width,height,RenderTextureReadWrite.sRGB);}
            var corners=new[]{"border-top-left-radius","border-top-right-radius","border-bottom-left-radius","border-bottom-right-radius"};var key=width+"/"+height;foreach(var corner in corners)key+="/"+_document.Sheet.Get(_node,corner,"0");
            if(_maskKey!=key){if(_mask!=null)UnityEngine.Object.Destroy(_mask);var clip=new VisualCssNode {Tag="div",Width=_node.Width,Height=_node.Height};clip.Style["background-color"]="#ffffff";foreach(var corner in corners)clip.Style[corner]=_document.Sheet.Get(_node,corner,"0");var raster=VisualCssPaint.Paint(_document.Sheet,clip,dpr);_mask=new Texture2D(raster.Width,raster.Height,TextureFormat.RGBA32,false,true);var pixels=new byte[raster.Pixels.Length];var stride=raster.Width*4;for(var y=0;y<raster.Height;y++)Buffer.BlockCopy(raster.Pixels,y*stride,pixels,(raster.Height-y-1)*stride,stride);_mask.LoadRawTextureData(pixels);_mask.Apply(false,true);_maskKey=key;_frame=-1;}
            return resized;
        }
        protected override void ImmediateRepaint()
        {
            if(_node==null||_node.Width<=0||_node.Height<=0)return;var resized=PrepareTargets();var dpr=_document.Scale;var height=_result.height;var padding=(_horizontal.height-height)/2;
            if(!resized&&_frame==_document.FrameVersion)return;_frame=_document.FrameVersion;
            var matrix=_original.worldTransform;var origin=matrix.MultiplyPoint3x4(new Vector3(0,(float)_node.Height+padding/dpr,0));var axisX=matrix.MultiplyVector(new Vector3((float)_node.Width,0,0));var axisY=matrix.MultiplyVector(new Vector3(0,-(float)_node.Height-padding*2f/dpr,0));var horizontal=matrix.MultiplyVector(Vector3.right);
            _material.SetVector("_CaptureOrigin",new Vector4(origin.x*dpr/Screen.width,1f-origin.y*dpr/Screen.height,0,0));_material.SetVector("_CaptureAxisX",new Vector4(axisX.x*dpr/Screen.width,-axisX.y*dpr/Screen.height,0,0));_material.SetVector("_CaptureAxisY",new Vector4(axisY.x*dpr/Screen.width,-axisY.y*dpr/Screen.height,0,0));_material.SetVector("_HorizontalStep",new Vector4(horizontal.x/Screen.width,-horizontal.y/Screen.height,0,0));_material.SetVector("_VerticalRect",new Vector4(0,padding/(float)_horizontal.height,1,height/(float)_horizontal.height));_material.SetFloat("_Sigma",_sigma*dpr);_material.SetTexture("_ClipMask",_mask);
            // This sibling only captures. The original element paints Texture
            // before its own brush, inside its opacity/filter compositing group.
            var capture=_document.BackdropCapture();
            var previous=RenderTexture.active;var previousGamma=GL.sRGBWrite;GL.PushMatrix();
            try{_commands.Clear();_commands.Blit(BuiltinRenderTextureType.CurrentActive,capture);_commands.Blit(capture,_horizontal,_material,0);_commands.Blit(_horizontal,_result,_material,1);_commands.SetRenderTarget(previous!=null?new RenderTargetIdentifier(previous):new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget));Graphics.ExecuteCommandBuffer(_commands);}
            finally{RenderTexture.active=previous;GL.sRGBWrite=previousGamma;GL.PopMatrix();}
        }
        private static RenderTexture Target(int width,int height,RenderTextureReadWrite colorSpace){var texture=new RenderTexture(width,height,0,RenderTextureFormat.ARGB32,colorSpace){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};texture.Create();return texture;}
        private void ReleaseTargets(){foreach(var texture in new[]{_horizontal,_result})if(texture!=null){texture.Release();UnityEngine.Object.Destroy(texture);}_horizontal=null;_result=null;}
        public void Dispose(){RemoveFromHierarchy();ReleaseTargets();if(_mask!=null)UnityEngine.Object.Destroy(_mask);UnityEngine.Object.Destroy(_material);_commands.Dispose();}
    }
}
