using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // Native GPU artwork authoring parameters, not a CSS interpreter. These are
    // the rounded fills/borders/gradients/shadows used by the existing menu.
    public sealed class NativeMenuBrush : IDisposable
    {
        public readonly struct Shadow
        {
            public readonly float Blur,Spread; public readonly Vector2 Offset; public readonly Color Color;
            public Shadow(float blur,Vector2 offset,Color color):this(blur,offset,color,0){}
            public Shadow(float blur,Vector2 offset,Color color,float spread){Blur=blur;Offset=offset;Color=color;Spread=spread;}
        }
        public sealed class Paint
        {
            public bool Ellipse,Linear,Horizontal,TopBorderOnly;
            public float Radius,Border;
            public float? LinearAngle;
            public Color Fill,BorderColor;
            public Vector2 Origin=new Vector2(.5f,.5f);
            public Color[] Colors=Array.Empty<Color>();
            public float[] Stops=Array.Empty<float>();
            public Shadow? OuterA,OuterB,Inset,InsetB;
        }
        private readonly Material _material;
        private readonly Vector4[] _colors=new Vector4[8];
        private readonly float[] _stops=new float[8];
        private readonly List<RenderTexture> _temporary=new List<RenderTexture>();
        private RenderTexture _target;
        private bool _disposed;
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
        internal bool AnalyticBlurForValidation=false;
#endif
        public RenderTexture Texture=>_target;
        public Rect Bounds {get;private set;}
        public int PaintCount {get;private set;}
#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
        internal static long DiagnosticRenders,DiagnosticBlurReads;
#endif
        public NativeMenuBrush()
        {
            var shader=Resources.Load<Shader>("EchoSphere/NativeMenuBrush");
            if(shader==null||!shader.isSupported)throw new InvalidOperationException("Native menu brush shader unavailable");
            _material=new Material(shader){name="Native menu brush",hideFlags=HideFlags.DontSave};
        }
        private RenderTexture Temporary(int width,int height)
        {var texture=RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.RFloat,RenderTextureReadWrite.Linear);texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;_temporary.Add(texture);return texture;}
        private static Vector4 V(Color color)=>new Vector4(color.r,color.g,color.b,color.a);
        private RenderTexture ShadowMask(Shadow shadow,bool inset,int width,int height,float scale,float padding)
        {
            var sigma=shadow.Blur*.5f*scale;var extra=Mathf.CeilToInt((shadow.Blur*2f+Mathf.Max(0,shadow.Spread))*scale);
            int w=width+extra*2,h=height+extra*2;
            var mask=Temporary(w,h);_material.SetVector("_MaskSize",new Vector4(w,h,extra,inset?1:0));
            _material.SetVector("_Offset",new Vector4(shadow.Offset.x,shadow.Offset.y,shadow.Spread,0));
            Graphics.Blit(Texture2D.blackTexture,mask,_material,0);
            if(sigma>0){
                int radius=Mathf.Max(1,Mathf.CeilToInt(4*sigma));
                _material.SetFloat("_Sigma",sigma);_material.SetInt("_KernelRadius",radius);_material.SetFloat("_Outside",inset?1:0);
                bool cached=NativeGaussianWeights.TryGet(sigma,out var weights)&&SystemInfo.IsFormatSupported(UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SFloat,UnityEngine.Experimental.Rendering.GraphicsFormatUsage.Linear);
#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
                if(NativeDeviceReplay.Active)DiagnosticBlurReads+=(long)w*h*2*(cached?2*((radius+1)/2)+1:2*radius+1);
#endif
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
                if(AnalyticBlurForValidation)cached=false;
#endif
                _material.SetInt("_KernelWeightCount",cached?radius+1:0);if(cached){_material.SetVectorArray("_KernelWeights",weights);mask.filterMode=FilterMode.Bilinear;}
                var ping=Temporary(w,h);var blur=Temporary(w,h);
                if(cached)ping.filterMode=FilterMode.Bilinear;
                _material.SetVector("_Direction",new Vector4(1f/w,0,0,0));Graphics.Blit(mask,ping,_material,1);
                _material.SetVector("_Direction",new Vector4(0,1f/h,0,0));Graphics.Blit(ping,blur,_material,1);mask=blur;
            }
            var crop=Temporary(width,height);Graphics.Blit(mask,crop,_material,3);return crop;
        }
        public void Render(float width,float height,float scale,Paint paint)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeMenuBrush));
            if(width<=0||height<=0||scale<=0||!float.IsFinite(width+height+scale))throw new ArgumentOutOfRangeException(nameof(width));
            if(paint.Colors.Length!=paint.Stops.Length||paint.Colors.Length>8)throw new ArgumentException("Native gradient stop count");
            float padding=0;
            foreach(var shadow in new[]{paint.OuterA,paint.OuterB})if(shadow.HasValue)padding=Mathf.Max(padding,Mathf.Max(Mathf.Abs(shadow.Value.Offset.x),Mathf.Abs(shadow.Value.Offset.y))+shadow.Value.Blur*2+Mathf.Max(0,shadow.Value.Spread));
            int w=Mathf.CeilToInt((width+padding*2)*scale),h=Mathf.CeilToInt((height+padding*2)*scale);
#if UNITY_EDITOR || UNITY_INCLUDE_INSTRUMENTATION
            if(NativeDeviceReplay.Active)DiagnosticRenders++;
#endif
            if(_target==null||_target.width!=w||_target.height!=h){ReleaseTarget();_target=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){name="Native menu brush",hideFlags=HideFlags.DontSave,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};if(!_target.Create())throw new InvalidOperationException("Native menu brush target allocation failed");}
            Bounds=new Rect(-padding,-padding,w/scale,h/scale);
            var previous=RenderTexture.active;bool srgb=GL.sRGBWrite;
            try{
                GL.sRGBWrite=false;
                _material.SetVector("_Size",new Vector4(width,height,scale,padding));_material.SetVector("_Pixels",new Vector4(w,h,0,0));
                _material.SetVector("_Radius",paint.Ellipse?new Vector4(width*.5f,height*.5f,0,0):new Vector4(Mathf.Min(paint.Radius,width*.5f,height*.5f),Mathf.Min(paint.Radius,width*.5f,height*.5f),0,0));
                _material.SetFloat("_Border",paint.Border);_material.SetVector("_Fill",V(paint.Fill));_material.SetVector("_BorderColor",V(paint.BorderColor));
                _material.SetVector("_BorderInsets",paint.TopBorderOnly?new Vector4(0,paint.Border,0,0):new Vector4(paint.Border,paint.Border,paint.Border,paint.Border));
                _material.SetVector("_Origin",new Vector4(paint.Origin.x,paint.Origin.y,0,0));_material.SetFloat("_Linear",paint.LinearAngle.HasValue?3:paint.Linear?(paint.Horizontal?2:1):0);
                var angle=(paint.LinearAngle??180)*Mathf.Deg2Rad;_material.SetVector("_GradientDirection",new Vector4(Mathf.Sin(angle),-Mathf.Cos(angle),0,0));
                for(int i=0;i<paint.Colors.Length;i++){_colors[i]=V(paint.Colors[i]);_stops[i]=paint.Stops[i];}
                _material.SetVectorArray("_Colors",_colors);_material.SetFloatArray("_Stops",_stops);_material.SetInt("_StopCount",paint.Colors.Length);
                var a=paint.OuterA.HasValue?ShadowMask(paint.OuterA.Value,false,w,h,scale,padding):null;
                var b=paint.OuterB.HasValue?ShadowMask(paint.OuterB.Value,false,w,h,scale,padding):null;
                var inner=paint.Inset.HasValue?ShadowMask(paint.Inset.Value,true,w,h,scale,padding):null;
                var innerB=paint.InsetB.HasValue?ShadowMask(paint.InsetB.Value,true,w,h,scale,padding):null;
                _material.SetTexture("_OuterA",a!=null?(Texture)a:Texture2D.blackTexture);_material.SetTexture("_OuterB",b!=null?(Texture)b:Texture2D.blackTexture);_material.SetTexture("_Inner",inner!=null?(Texture)inner:Texture2D.blackTexture);
                _material.SetTexture("_InnerB",innerB!=null?(Texture)innerB:Texture2D.blackTexture);_material.SetVector("_ShadowInnerB",V(paint.InsetB?.Color??Color.clear));
                _material.SetVector("_ShadowA",V(paint.OuterA?.Color??Color.clear));_material.SetVector("_ShadowB",V(paint.OuterB?.Color??Color.clear));_material.SetVector("_ShadowInner",V(paint.Inset?.Color??Color.clear));
                Graphics.Blit(Texture2D.blackTexture,_target,_material,2);PaintCount++;
            }finally{RenderTexture.active=previous;GL.sRGBWrite=srgb;foreach(var texture in _temporary)RenderTexture.ReleaseTemporary(texture);_temporary.Clear();}
        }
        private static void DestroyOwned(UnityEngine.Object value){if(Application.isPlaying)UnityEngine.Object.Destroy(value);else UnityEngine.Object.DestroyImmediate(value);}
        private void ReleaseTarget(){if(_target!=null){_target.Release();DestroyOwned(_target);_target=null;}}
        public void Dispose(){if(_disposed)return;_disposed=true;ReleaseTarget();DestroyOwned(_material);}
    }
}
