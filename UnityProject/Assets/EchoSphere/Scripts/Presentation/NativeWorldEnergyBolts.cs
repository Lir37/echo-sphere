using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // The fallback drawEnergyBolt only. Mutation-specific chain lightning still
    // belongs to its separate original renderer until that renderer is ported.
    internal sealed class NativeWorldEnergyBolts : IDisposable
    {
        private sealed class Bolt : IDisposable
        {
            internal readonly Mesh Glow=new Mesh {name="Native energy bolt glow"},Core=new Mesh {name="Native energy bolt core"};
            internal readonly MaterialPropertyBlock GlowProperties=new MaterialPropertyBlock(),CoreProperties=new MaterialPropertyBlock();internal readonly Vector2[] Points=new Vector2[8];
            public void Dispose(){UnityEngine.Object.Destroy(Glow);UnityEngine.Object.Destroy(Core);}
        }
        private readonly Material _material;private readonly List<Bolt> _bolts=new List<Bolt>();private int _used;
        internal NativeWorldEnergyBolts(){_material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Native energy bolts",hideFlags=HideFlags.DontSave};}
        internal void BeginFrame(){_used=0;}
        internal void Draw(SourceCanvasSurface surface,double x,double y,double endX,double endY,double alpha,double time)
        {
            if(_used==_bolts.Count)_bolts.Add(new Bolt());var bolt=_bolts[_used++];double dx=endX-x,dy=endY-y,len=Math.Sqrt(dx*dx+dy*dy);if(len==0)len=1;double nx=-dy/len,ny=dx/len,wobble=Math.Min(26,len*.08);bolt.Points[0]=new Vector2((float)x,(float)y);
            for(int i=1;i<7;i++){double p=i/7d,wave=Math.Sin(i*3.7+time*33.3333333)*wobble*.35;bolt.Points[i]=new Vector2((float)(x+dx*p+nx*wave),(float)(y+dy*p+ny*wave));}bolt.Points[7]=new Vector2((float)endX,(float)endY);
            NativeWorldShapes.StrokePolyline(bolt.Points,7,bolt.Glow,surface.NativeStrokeCorner,surface.NativeStrokeMiter);NativeWorldShapes.StrokePolyline(bolt.Points,1.5f,bolt.Core,surface.NativeStrokeCorner,surface.NativeStrokeMiter);
            bolt.GlowProperties.SetVector("_NativeColor",new Vector4(105/255f,235/255f,1,Mathf.Clamp01((float)(.28*alpha))));bolt.CoreProperties.SetVector("_NativeColor",new Vector4(235/255f,252/255f,1,Mathf.Clamp01((float)(.9*alpha))));
            surface.DrawNativeMesh(bolt.Glow,_material,bolt.GlowProperties,inheritBlend:true);surface.DrawNativeMesh(bolt.Core,_material,bolt.CoreProperties,inheritBlend:true);surface.ContinueNativePolygon(bolt.Points,Matrix4x4.identity,false);
        }
        public void Dispose(){foreach(var bolt in _bolts)bolt.Dispose();_bolts.Clear();UnityEngine.Object.Destroy(_material);}
    }
}
