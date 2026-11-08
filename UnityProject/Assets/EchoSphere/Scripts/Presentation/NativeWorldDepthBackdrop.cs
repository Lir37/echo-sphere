using System;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // A screen quad combines the distant environment and a transparent near arena.
    // The camera/gameplay transforms are only READ; their coordinates stay unchanged.
    internal sealed class NativeWorldDepthBackdrop : IDisposable
    {
        internal const float Parallax=.035f, Shear=.16f, VerticalProjection=.94f;
        private readonly Mesh _mesh=new Mesh{name="Regional depth background"};
        private readonly Material _material;
        private readonly MaterialPropertyBlock _properties=new MaterialPropertyBlock();
        private NativeRegionBackdropAssets.Lease _active,_previous,_pending;
        private int _requested=-1,_trial;
        private float _width=-1,_height;
        private double _transitionStarted;
        internal int GeometryBuilds{get;private set;}
        internal int DisplayedRegion=>_active?.Region??-1;
        internal bool Ready=>_active?.Texture!=null;

        internal NativeWorldDepthBackdrop()
        {
            var shader=Resources.Load<Shader>("EchoSphere/NativeWorldDepthBackdrop");
            if(shader==null||!shader.isSupported)throw new InvalidOperationException("Regional depth shader unavailable");
            _material=new Material(shader){name="Regional depth background",hideFlags=HideFlags.DontSave};
        }
        internal static int TrialStyle(int region,string id)
        {
            if(string.IsNullOrEmpty(id)||id=="none")return 0;
            var contracts=CampaignRegionContent.GetContracts((EchoRegionId)Mathf.Clamp(region,0,5));
            for(int i=0;i<contracts.Count;i++)if(contracts[i].Id==id)return i+1;
            return 0;
        }
        // Fixed for a viewport/aspect: camera motion must never change the zoom.
        internal static Vector2 ImageSpan(float width,float height,float aspect)
        {
            float imageHeight=Mathf.Max(height*VerticalProjection+160,(width+Shear*height+160)/Mathf.Max(.1f,aspect));
            return new Vector2(imageHeight*aspect,imageHeight);
        }
        internal static Vector2 DistantUv(Vector2 point,Vector2 viewport,Vector2 camera,Vector2 imageSpan)
        {
            var p=point-viewport*.5f+camera*Parallax;
            return new Vector2(.5f+(p.x+Shear*p.y)/imageSpan.x,.5f-p.y*VerticalProjection/imageSpan.y);
        }
        internal void Draw(NativeWorldSurface surface,float width,float height,int region,Vector2 camera,string trial,double runTime)
        {
            region=Mathf.Clamp(region,0,5);
            if(_requested!=region)
            {
                _pending?.Dispose();_pending=null;_requested=region;
                if(_active?.Region!=region)_pending=NativeRegionBackdropAssets.Acquire(region);
            }
            if(_pending?.Texture!=null)
            {
                _previous?.Dispose();_previous=_active;_active=_pending;_pending=null;
                _transitionStarted=Time.realtimeSinceStartupAsDouble;
            }
            if(_active?.Region==region)_trial=TrialStyle(region,trial);
            float blend=Mathf.SmoothStep(0,1,(float)((Time.realtimeSinceStartupAsDouble-_transitionStarted)/.55));
            // Detach the old texture before binding properties when its fade ends.
            if(blend>=1&&_previous!=null){_previous.Dispose();_previous=null;}
            if(width!=_width||height!=_height)
            {
                _width=width;_height=height;_mesh.Clear();
                _mesh.vertices=new[]{Vector3.zero,new Vector3(width,0,0),new Vector3(width,height,0),new Vector3(0,height,0)};
                _mesh.triangles=new[]{0,1,2,0,2,3};_mesh.RecalculateBounds();GeometryBuilds++;
            }
            var texture=_active?.Texture;var previous=_previous?.Texture;
            float aspect=texture!=null?(float)texture.width/texture.height:9f/16;
            var span=ImageSpan(width,height,aspect);
            _properties.SetTexture("_Backdrop",texture!=null?texture:Texture2D.blackTexture);
            _properties.SetTexture("_PreviousBackdrop",previous!=null?previous:Texture2D.blackTexture);
            _properties.SetVector("_FrameSize",new Vector4(width,height,span.x,span.y));
            _properties.SetVector("_DepthCamera",new Vector4(camera.x,camera.y,Parallax,Shear));
            _properties.SetVector("_DepthStyle",new Vector4(_active?.Region??region,_trial,(float)runTime,blend));
            _properties.SetVector("_DepthReady",new Vector4(texture!=null?1:0,previous!=null?1:0,VerticalProjection,0));
            surface.DrawNativeMesh(_mesh,_material,_properties);
        }
        public void Dispose()
        {
            _pending?.Dispose();_active?.Dispose();_previous?.Dispose();
            _pending=_active=_previous=null;
            UnityEngine.Object.Destroy(_mesh);UnityEngine.Object.Destroy(_material);
        }
    }
}
