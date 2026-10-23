using System;
using UnityEngine;

namespace EchoSphere.Presentation
{
    internal sealed class NativeWorldAmbient : IDisposable
    {
        private readonly Mesh _mesh=new Mesh {name="Native ambient motes"};
        private readonly Material _material;
        private readonly MaterialPropertyBlock _properties=new MaterialPropertyBlock();
        private float _w=-1,_h;private int _count;
        internal NativeWorldAmbient()
        {
            var shader=Resources.Load<Shader>("EchoSphere/NativeWorldGeometry");if(shader==null||!shader.isSupported)throw new InvalidOperationException("Native ambient shader unavailable");_material=new Material(shader){name="Native ambient motes",hideFlags=HideFlags.DontSave};
        }
        internal void Draw(NativeWorldSurface surface,float w,float h,double time)
        {
            if(w!=_w||h!=_h){
                _w=w;_h=h;_count=Math.Min(90,(int)Math.Floor((double)w*h/11000));var builder=new NativeWorldField.Builder();
                for(int i=0;i<_count;i++)builder.Circle((float)(i*97.13%w),(float)(i*53.71%h),(float)(.7+i%3*.35),new Color(120/255f,190/255f,1,.16f),2+i);
                builder.Upload(_mesh);
            }
            if(_count==0)return;_properties.SetVector("_Ambient",new Vector4(w,h,(float)time,0));surface.DrawNativeMesh(_mesh,_material,_properties);
            // Preserve only the current-path continuation of the source helper;
            // its drawing and animation are entirely the native mesh/material.
        }
        public void Dispose(){UnityEngine.Object.Destroy(_mesh);UnityEngine.Object.Destroy(_material);}
    }
}
