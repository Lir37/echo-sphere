using System;
using System.Collections.Generic;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    internal sealed class NativeWorldMinions : IDisposable
    {
        private sealed class Art : IDisposable
        {
            internal readonly Mesh Shadow,InnerShadow,Body,Outline,WhiteCore,Core;
            internal Art(float r){Shadow=Ellipse(new Vector2(0,5),r*.72f,r*.22f);InnerShadow=Ellipse(new Vector2(0,5),r*.72f*.72f,r*.22f*.68f);var path=new[]{new Vector2(0,-r),new Vector2(r*.72f,-r*.34f),new Vector2(r*.52f,r*.62f),new Vector2(0,r*.72f),new Vector2(-r*.52f,r*.62f),new Vector2(-r*.72f,-r*.34f)};Body=Polygon(path);Outline=Polygon(path,1.1f);WhiteCore=Circle(new Vector2(0,-r*.1f),r*.24f);Core=Circle(new Vector2(0,-r*.1f),r*.16f);}
            public void Dispose(){Destroy(Shadow);Destroy(InnerShadow);Destroy(Body);Destroy(Outline);Destroy(WhiteCore);Destroy(Core);}
        }
        private readonly Dictionary<float,Art> _art=new Dictionary<float,Art>();
        private readonly Mesh _ray=Line(Vector2.zero,Vector2.right,1.1f);
        private readonly Material _material;
        private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();private int _used;
        internal NativeWorldMinions(){_material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Native summoned minions",hideFlags=HideFlags.DontSave};}
        internal void BeginFrame(){_used=0;}
        private MaterialPropertyBlock Properties(Color color){if(_used==_properties.Count)_properties.Add(new MaterialPropertyBlock());var p=_properties[_used++];p.SetVector("_NativeColor",new Vector4(color.r,color.g,color.b,color.a));return p;}
        private static Color Alpha(Color color,float alpha){color.a=alpha;return color;}
        internal void Draw(SourceCanvasSurface surface,float x,float y,float radius,float rotation,Color color,double time)
        {
            if(!_art.TryGetValue(radius,out var art)){art=new Art(radius);_art.Add(radius,art);}float bob=(float)(Math.Sin(time*5+x*.01)*radius*.08),angle=(float)(rotation+time*.12);var transform=Matrix4x4.TRS(new Vector3(x,y+bob,0),Quaternion.Euler(0,0,angle*Mathf.Rad2Deg),Vector3.one);
            // The source ground-shadow helper uses two solid ellipses. Its blur
            // parameter is unused; adding a blur here would change the artwork.
            surface.DrawNativeMesh(art.Shadow,_material,Properties(new Color(0,0,0,.34f*.42f)),transform);surface.DrawNativeMesh(art.InnerShadow,_material,Properties(new Color(0,0,0,.34f*.16f)),transform);surface.DrawNativeMesh(art.Body,_material,Properties(new Color(6/255f,16/255f,28/255f,1)),transform);surface.DrawNativeMesh(art.Outline,_material,Properties(Alpha(color,.86f)),transform);
            for(int i=0;i<3;i++){double a=time*1.7+i*Math.PI*2/3;Vector2 start=new Vector2((float)(Math.Cos(a)*radius*.25),(float)(Math.Sin(a)*radius*.20)),end=new Vector2((float)(Math.Cos(a)*radius*.88),(float)(Math.Sin(a)*radius*.52));var delta=end-start;var ray=Matrix4x4.TRS(start,Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg),new Vector3(delta.magnitude,1,1));surface.DrawNativeMesh(_ray,_material,Properties(Alpha(color,.42f)),transform*ray);}
            surface.DrawNativeMesh(art.WhiteCore,_material,Properties(Color.white),transform);surface.DrawNativeMesh(art.Core,_material,Properties(color),transform);surface.ContinueNativeCircle(new Vector2(0,-radius*.1f),radius*.16f,transform);
        }
        private static void Destroy(UnityEngine.Object value){UnityEngine.Object.Destroy(value);}
        public void Dispose(){foreach(var art in _art.Values)art.Dispose();_art.Clear();Destroy(_ray);Destroy(_material);}
    }
}
