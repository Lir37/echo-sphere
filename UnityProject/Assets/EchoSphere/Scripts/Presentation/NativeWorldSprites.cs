using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // Existing imported Sprite assets, native UV mesh and material submission.
    // No runtime SVG decode, Canvas drawImage or replacement artwork.
    internal sealed class NativeWorldSprites : IDisposable
    {
        private readonly Dictionary<Sprite,Mesh> _meshes=new Dictionary<Sprite,Mesh>();
        private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();private int _used;
        private readonly Material _material;
        internal NativeWorldSprites(){var shader=Resources.Load<Shader>("EchoSphere/NativeWorldSprite");if(shader==null||!shader.isSupported)throw new InvalidOperationException("Native world sprite shader unavailable");_material=new Material(shader){name="Native world sprites",hideFlags=HideFlags.DontSave};}
        internal void BeginFrame(){_used=0;}
        internal bool Draw(SourceCanvasSurface surface,SourceCanvasImage image,double x,double y,double size,double rotation,double opacity,bool part,bool mirrorX,bool mirrorY,Matrix4x4? parent=null,bool forceOver=false)
        {
            if(image?.Sprite==null)return false;var sprite=image.Sprite;var texture=sprite.texture;
            if(!_meshes.TryGetValue(sprite,out var mesh)){var r=sprite.textureRect;mesh=new Mesh {name="Native world sprite "+sprite.name};mesh.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};mesh.uv=new[]{new Vector2(r.xMin/texture.width,r.yMax/texture.height),new Vector2(r.xMax/texture.width,r.yMax/texture.height),new Vector2(r.xMax/texture.width,r.yMin/texture.height),new Vector2(r.xMin/texture.width,r.yMin/texture.height)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateBounds();_meshes.Add(sprite,mesh);}
            double iw=Math.Max(1,image.Get("naturalWidth").Number),ih=Math.Max(1,image.Get("naturalHeight").Number),scale=Math.Min(size/iw,size/ih);
            var transform=Matrix4x4.TRS(new Vector3((float)x,(float)y,0),Quaternion.Euler(0,0,(float)rotation*Mathf.Rad2Deg),new Vector3((float)(iw*scale)*(mirrorX?-1:1),(float)(ih*scale)*(mirrorY?-1:1),1));
            if(_used==_properties.Count)_properties.Add(new MaterialPropertyBlock());var properties=_properties[_used++];properties.SetTexture("_ImageTex",texture);properties.SetVector("_ImageTex_TexelSize",new Vector4(1f/texture.width,1f/texture.height,texture.width,texture.height));
            // Canvas ignores an invalid globalAlpha assignment rather than
            // clamping it. These helpers assign opacity, not multiply it.
            float effective=opacity>=0&&opacity<=1?(float)opacity:surface.NativeOpacity;
            surface.DrawNativeMesh(mesh,_material,properties,(parent??Matrix4x4.identity)*transform,inheritBlend:!part&&!forceOver,opacity:effective,inheritShadow:true,screen:part);return true;
        }
        public void Dispose(){foreach(var mesh in _meshes.Values)UnityEngine.Object.Destroy(mesh);_meshes.Clear();UnityEngine.Object.Destroy(_material);}
    }
}
