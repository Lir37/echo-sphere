using System;
using System.Collections.Generic;
using static EchoSphere.Presentation.NativeWorldShapes;
using UnityEngine;

namespace EchoSphere.Presentation
{
    internal sealed class NativeWorldPickups : IDisposable
    {
        private sealed class XpArt : IDisposable
        {
            internal readonly Mesh Fill,Outline,Highlight;internal readonly Vector2 HighlightCenter;internal readonly float HighlightRadius;
            internal XpArt(float r){var p=new[]{new Vector2(0,-r*1.1f),new Vector2(r*.62f,-r*.35f),new Vector2(r*.52f,r*.68f),new Vector2(0,r*1.05f),new Vector2(-r*.52f,r*.68f),new Vector2(-r*.62f,-r*.35f)};Fill=Polygon(p);Outline=Polygon(p,Mathf.Max(.7f,r*.1f));HighlightCenter=new Vector2(-r*.20f,-r*.30f);HighlightRadius=Mathf.Max(.8f,r*.12f);Highlight=Circle(HighlightCenter,HighlightRadius);}
            public void Dispose(){Destroy(Fill);Destroy(Outline);Destroy(Highlight);}
        }
        private static readonly Vector2[] HealthPath={new Vector2(-4,-12),new Vector2(4,-12),new Vector2(4,-4),new Vector2(12,-4),new Vector2(12,4),new Vector2(4,4),new Vector2(4,12),new Vector2(-4,12),new Vector2(-4,4),new Vector2(-12,4),new Vector2(-12,-4),new Vector2(-4,-4)};
        private readonly Dictionary<float,XpArt> _xp=new Dictionary<float,XpArt>();
        private readonly Mesh _healthFill,_healthOutline,_healthVertical,_healthHorizontal;
        private readonly Material _material;
        private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();private int _used;
        private static Color A(int r,int g,int b,float a)=>new Color(r/255f,g/255f,b/255f,a);
        internal NativeWorldPickups()
        {
            _material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Native pickups",hideFlags=HideFlags.DontSave};_healthFill=Polygon(HealthPath);_healthOutline=Polygon(HealthPath,1.4f);_healthVertical=Rectangle(-2,-7,4,14);_healthHorizontal=Rectangle(-7,-2,14,4);
        }
        internal void BeginFrame(){_used=0;}
        private MaterialPropertyBlock Properties(Color color){if(_used==_properties.Count)_properties.Add(new MaterialPropertyBlock());var p=_properties[_used++];p.SetVector("_NativeColor",new Vector4(color.r,color.g,color.b,color.a));return p;}
        internal void DrawXp(SourceCanvasSurface surface,float x,float y,float radius,Color color,float rotation)
        {
            if(!_xp.TryGetValue(radius,out var art)){art=new XpArt(radius);_xp.Add(radius,art);}var transform=Matrix4x4.TRS(new Vector3(x,y,0),Quaternion.Euler(0,0,rotation*Mathf.Rad2Deg),Vector3.one);
            surface.DrawNativeMesh(art.Fill,_material,Properties(color),transform);surface.DrawNativeMesh(art.Outline,_material,Properties(A(230,253,255,.82f)),transform);surface.DrawNativeMesh(art.Highlight,_material,Properties(A(255,255,255,.58f)),transform);surface.ContinueNativeCircle(art.HighlightCenter,art.HighlightRadius,transform);
        }
        internal void DrawHealth(SourceCanvasSurface surface,float x,float y,Color color,double time)
        {
            float pulse=(float)(1+Math.Sin(time*5)*.07);var transform=Matrix4x4.TRS(new Vector3(x,y,0),Quaternion.identity,new Vector3(pulse,pulse,1));
            // All four source paints cast their own4px shadow, including each
            // white rectangle. Combining them would change overlap and glow.
            surface.DrawNativeMesh(_healthFill,_material,Properties(A(7,13,22,.94f)),transform,color,4);surface.DrawNativeMesh(_healthOutline,_material,Properties(A(255,125,150,.90f)),transform,color,4);surface.DrawNativeMesh(_healthVertical,_material,Properties(Color.white),transform,color,4);surface.DrawNativeMesh(_healthHorizontal,_material,Properties(Color.white),transform,color,4);surface.ContinueNativePolygon(HealthPath,transform);
        }
        private static void Destroy(UnityEngine.Object value){UnityEngine.Object.Destroy(value);}
        public void Dispose(){foreach(var art in _xp.Values)art.Dispose();_xp.Clear();Destroy(_healthFill);Destroy(_healthOutline);Destroy(_healthVertical);Destroy(_healthHorizontal);Destroy(_material);}
    }
}
