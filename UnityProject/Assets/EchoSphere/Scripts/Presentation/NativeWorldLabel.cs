using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // Unity Font/TextGenerator owns shaping, fallback and atlas. This owner emits
    // a retained mesh at the original centre/alphabetic baseline, with no Canvas
    // text command or CSS declaration parsing. Glyph preparation shares the final
    // atlas-stabilization phase with the remaining source layers.
    internal sealed class NativeWorldLabel : IDisposable
    {
        private readonly Mesh _mesh=new Mesh {name="Native world label"};
        private readonly Material _material;
        private readonly MaterialPropertyBlock _properties=new MaterialPropertyBlock();
        private readonly List<Vector3> _vertices=new List<Vector3>();private readonly List<Vector2> _uv=new List<Vector2>();private readonly List<int> _triangles=new List<int>();
        private float _logicalSize=8;private string _family="system-ui,sans-serif";private FontStyle _style=FontStyle.Bold;
        private Font _font;private string _text;private int _size;private bool _dirty=true;
        internal NativeWorldLabel(){_material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldLabel")){name="Native world label",hideFlags=HideFlags.DontSave};Font.textureRebuilt+=Rebuilt;}
        private void Rebuilt(Font font){if(font==_font)_dirty=true;}
        internal void Draw(SourceCanvasSurface surface,string text,Vector2 position,Color color,Color? shadow=null,bool inheritShadowOffsets=false,Matrix4x4? localTransform=null,float shadowBlur=0,float fontSize=8,string family="system-ui,sans-serif",FontStyle style=FontStyle.Bold,bool inheritBlend=false,bool additive=false,bool inheritShadow=false,float? opacity=null)
        {
            int size=Mathf.Max(1,Mathf.CeilToInt(fontSize*(localTransform.HasValue?surface.NativeTextRasterScaleFor(localTransform.Value):surface.NativeTextRasterScale)));if(size!=_size||text!=_text||fontSize!=_logicalSize||family!=_family||style!=_style){_size=size;_text=text;_logicalSize=fontSize;_family=family;_style=style;_dirty=true;}
            _font=SourceCanvasTypography.ResolveFamily(family,size);_properties.SetVector("_LabelOffset",new Vector4(position.x,position.y,0,0));_properties.SetVector("_LabelColor",new Vector4(color.r,color.g,color.b,color.a));
            surface.DrawNativeTextMesh(_mesh,_material,_properties,Prepare,Emit,shadow,inheritShadowOffsets,localTransform,shadowBlur,inheritBlend,additive,inheritShadow,opacity);
        }
        private void Prepare(){SourceCanvasTypography.Vertices(_font,_text,_size,_style);}
        private void Emit()
        {
            var atlas=_font.material.mainTexture;_properties.SetTexture("_Glyphs",atlas);_properties.SetVector("_Glyphs_TexelSize",new Vector4(1f/atlas.width,1f/atlas.height,atlas.width,atlas.height));if(!_dirty)return;
            float scale=_logicalSize/_size,width=SourceCanvasTypography.Width(_font,_text,_size,_style)*scale,ascent=_font.ascent*(float)_size/_font.fontSize;
            var vertices=SourceCanvasTypography.Vertices(_font,_text,_size,_style);_vertices.Clear();_uv.Clear();_triangles.Clear();
            for(int i=0;i+3<vertices.Length;i+=4){int first=_vertices.Count;for(int j=0;j<4;j++){var v=vertices[i+j];_vertices.Add(new Vector3(-width*.5f+v.position.x*scale,-(v.position.y+ascent)*scale,0));_uv.Add(v.uv0);}_triangles.Add(first);_triangles.Add(first+1);_triangles.Add(first+2);_triangles.Add(first);_triangles.Add(first+2);_triangles.Add(first+3);}
            _mesh.Clear();_mesh.SetVertices(_vertices);_mesh.SetUVs(0,_uv);_mesh.SetTriangles(_triangles,0);_mesh.RecalculateBounds();_dirty=false;
        }
        public void Dispose(){Font.textureRebuilt-=Rebuilt;UnityEngine.Object.Destroy(_mesh);UnityEngine.Object.Destroy(_material);}
    }
}
