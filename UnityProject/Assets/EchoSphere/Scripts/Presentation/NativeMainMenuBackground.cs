using System;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Owned static native GPU layer. Repaints only for a logical viewport/DPR
    // change. No DOM/CSS interpretation, CPU rasterizer or frame update loop.
    // The owner calls SetViewport before UI rendering, never from paint/filter callbacks.
    public sealed class NativeMainMenuBackground : IDisposable
    {
        private static readonly ProfilerMarker PaintMarker=new ProfilerMarker("EchoSphere.NativeMenu.BackgroundPaint");
        private static readonly int SizeId=Shader.PropertyToID("_LogicalSize");
        private static readonly int BackingId=Shader.PropertyToID("_BackingSize");
        private readonly Image _image;
        private readonly Material _material;
        private RenderTexture _target;
        private float _width,_height,_dpr;
        private bool _disposed;
        public int PaintCount {get;private set;}
        public RenderTexture Texture=>_target;

        public NativeMainMenuBackground(VisualElement parent,string resource="EchoSphere/NativeMainMenuBackground")
        {
            if(parent==null)throw new ArgumentNullException(nameof(parent));
            var shader=Resources.Load<Shader>(resource);
            if(shader==null)throw new InvalidOperationException("Native menu background shader missing");
            _material=new Material(shader){name="Native menu background",hideFlags=HideFlags.DontSave};
            _image=new Image {name="native-menu-background-image",pickingMode=PickingMode.Ignore,scaleMode=ScaleMode.StretchToFill};
            _image.style.position=Position.Absolute;_image.style.left=_image.style.top=0;
            parent.Insert(0,_image);
        }

        public void SetViewport(float width,float height,float dpr)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeMainMenuBackground));
            if(!float.IsFinite(width)||!float.IsFinite(height)||!float.IsFinite(dpr)||width<=0||height<=0||dpr<=0)
                throw new ArgumentOutOfRangeException(nameof(width),"Finite positive CSS viewport and density required");
            if(_target!=null&&width==_width&&height==_height&&dpr==_dpr)return;
            var pixelsX=Mathf.CeilToInt(width*dpr);var pixelsY=Mathf.CeilToInt(height*dpr);
            if(pixelsX>SystemInfo.maxTextureSize||pixelsY>SystemInfo.maxTextureSize)
                throw new ArgumentOutOfRangeException(nameof(width),"Native menu viewport exceeds GPU texture limits");
            using var sample=PaintMarker.Auto();
            if(_target==null||_target.width!=pixelsX||_target.height!=pixelsY){
                ReleaseTarget();
                _target=new RenderTexture(pixelsX,pixelsY,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name="Native menu background",hideFlags=HideFlags.DontSave,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
                _target.Create();_image.image=_target;
            }
            // Match the full source brush texel extent, including a fractional
            // final CSS pixel, instead of stretching the last row/column inward.
            _image.style.width=pixelsX/dpr;_image.style.height=pixelsY/dpr;
            _material.SetVector(SizeId,new Vector4(width,height,pixelsX/dpr,pixelsY/dpr));
            _material.SetVector(BackingId,new Vector4(pixelsX,pixelsY,dpr,0));
            var previous=RenderTexture.active;var gamma=GL.sRGBWrite;
            try{GL.sRGBWrite=false;Graphics.Blit(Texture2D.whiteTexture,_target,_material);}
            finally{RenderTexture.active=previous;GL.sRGBWrite=gamma;}
            _image.MarkDirtyRepaint();_width=width;_height=height;_dpr=dpr;PaintCount++;
        }
        private void ReleaseTarget()
        {if(_target!=null){_image.image=null;_target.Release();DestroyOwned(_target);_target=null;}}
        private static void DestroyOwned(UnityEngine.Object value)
        {if(Application.isPlaying)UnityEngine.Object.Destroy(value);else UnityEngine.Object.DestroyImmediate(value);}
        public void Dispose()
        {if(_disposed)return;_disposed=true;_image.RemoveFromHierarchy();ReleaseTarget();DestroyOwned(_material);}
    }
}
