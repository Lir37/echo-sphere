using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EchoSphere.Presentation
{
    // Authored active drawVoidField layers, as retained Unity meshes/materials.
    // The commented resonance-arena block is intentionally not implemented.
    internal sealed class NativeWorldField : IDisposable
    {
        private readonly Mesh _gradient=new Mesh {name="Native world field gradient"};
        private readonly Mesh _nebulae=new Mesh {name="Native world field nebulae"};
        private readonly Mesh _details=new Mesh {name="Native world field lines and stars"};
        private readonly Material _gradientMaterial,_shapeMaterial;
        private readonly MaterialPropertyBlock _properties=new MaterialPropertyBlock();
        private float _w=-1,_h,_ox,_oy;
        internal sealed class Builder
        {
            internal readonly List<Vector3> Vertices=new List<Vector3>();internal readonly List<Color> Colors=new List<Color>();internal readonly List<Vector4> Uv=new List<Vector4>();internal readonly List<int> Triangles=new List<int>();
            internal void Quad(float x,float y,float width,float height,Color color,bool circle=false,bool focusY=false,float animation=0)
            {
                int i=Vertices.Count;Vertices.Add(new Vector3(x,y,0));Vertices.Add(new Vector3(x+width,y,0));Vertices.Add(new Vector3(x+width,y+height,0));Vertices.Add(new Vector3(x,y+height,0));
                for(int j=0;j<4;j++)Colors.Add(color);
                float mode=animation>0?animation:focusY?1:0;
                Uv.Add(new Vector4(-1,-1,circle?1:0,mode));Uv.Add(new Vector4(1,-1,circle?1:0,mode));Uv.Add(new Vector4(1,1,circle?1:0,mode));Uv.Add(new Vector4(-1,1,circle?1:0,mode));
                Triangles.Add(i);Triangles.Add(i+1);Triangles.Add(i+2);Triangles.Add(i);Triangles.Add(i+2);Triangles.Add(i+3);
            }
            internal void Circle(float x,float y,float radius,Color color,float animation=0)
            {
                // Retain coverage outside the mathematical edge. Tight quads
                // would clip half the antialias fringe of subpixel stars.
                int start=Uv.Count;float padded=radius+1;Quad(x-padded,y-padded,padded*2,padded*2,color,true,animation:animation);
                for(int j=start;j<Uv.Count;j++){var uv=Uv[j];uv.x*=padded/radius;uv.y*=padded/radius;Uv[j]=uv;}
            }
            internal void Upload(Mesh mesh){mesh.Clear();mesh.indexFormat=IndexFormat.UInt32;mesh.SetVertices(Vertices);mesh.SetColors(Colors);mesh.SetUVs(0,Uv);mesh.SetTriangles(Triangles,0);mesh.RecalculateBounds();}
        }
        private static Color A(int r,int g,int b,float a)=>new Color(r/255f,g/255f,b/255f,a);
        internal NativeWorldField()
        {
            _gradientMaterial=Material("EchoSphere/NativeWorldField","Native world field gradient");_shapeMaterial=Material("EchoSphere/NativeWorldGeometry","Native world field geometry");
        }
        private static Material Material(string path,string name){var shader=Resources.Load<Shader>(path);if(shader==null||!shader.isSupported)throw new InvalidOperationException("Native world shader unavailable: "+path);return new Material(shader){name=name,hideFlags=HideFlags.DontSave};}
        internal void Draw(NativeWorldSurface surface,float w,float h,float ox,float oy,float focusX,float focusY)
        {
            if(w!=_w||h!=_h||ox!=_ox||oy!=_oy){_w=w;_h=h;_ox=ox;_oy=oy;Build();}
            _properties.SetVector("_Field",new Vector4(w,h,focusX,focusY));
            surface.DrawNativeMesh(_gradient,_gradientMaterial,_properties);surface.DrawNativeMesh(_nebulae,_shapeMaterial,_properties);surface.DrawNativeMesh(_details,_shapeMaterial,_properties);

        }
        private void Build()
        {
            var g=new Builder();g.Quad(_ox,_oy,_w,_h,Color.white);g.Upload(_gradient);
            var n=new Builder();n.Circle(_ox+_w*.18f,_oy+_h*.30f,_w*.24f,A(44,168,255,.035f));n.Circle(_ox+_w*.76f,_oy+_h*.28f,_w*.22f,A(178,75,255,.030f));n.Circle(_ox+_w*.52f,_oy+_h*.69f,_w*.30f,A(70,117,255,.030f));n.Upload(_nebulae);
            var d=new Builder();
            for(int i=1;i<5;i++)d.Quad(_ox+_w*.08f,_h*.22f+i*i*_h*.045f-.5f,_w*.84f,1,A(96,184,255,.032f),focusY:true);
            float horizon=_oy+_h*.40f;for(int i=0;i<8;i++){float y=horizon+(float)Math.Pow(i/8d,1.9)*_h*.48f;d.Quad(_ox+_w*.06f,y-.5f,_w*.88f,1,A(115,206,255,.055f));}
            for(int i=0;i<120;i++)d.Circle(_ox+(float)(i*137.31%_w),_oy+(float)(i*71.93%_h),(float)(.35+i%3*.18),A(190,230,255,(float)(.12+i*17%60/600d)));
            d.Upload(_details);
        }
        public void Dispose(){UnityEngine.Object.Destroy(_gradient);UnityEngine.Object.Destroy(_nebulae);UnityEngine.Object.Destroy(_details);UnityEngine.Object.Destroy(_gradientMaterial);UnityEngine.Object.Destroy(_shapeMaterial);}
    }
}
