using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    // Pooled connected lightning, shared by abilities and sphere mutations.
    internal sealed class NativeWorldEnergyBolts : IDisposable
    {
        private sealed class Bolt : IDisposable
        {
            internal readonly Mesh Glow=new Mesh {name="Native energy bolt glow"},Core=new Mesh {name="Native energy bolt core"},Branches=new Mesh {name="Native energy branches"};
            internal readonly MaterialPropertyBlock GlowProperties=new MaterialPropertyBlock(),CoreProperties=new MaterialPropertyBlock();internal readonly Vector2[] BranchPoints=new Vector2[13];internal readonly Vector2[] Points=new Vector2[17];
            public void Dispose(){UnityEngine.Object.Destroy(Glow);UnityEngine.Object.Destroy(Core);UnityEngine.Object.Destroy(Branches);}
        }
        private readonly Material _material;private readonly List<Bolt> _bolts=new List<Bolt>();private int _used;
        internal NativeWorldEnergyBolts(){_material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Native energy bolts",hideFlags=HideFlags.DontSave};}
        internal void BeginFrame(){_used=0;}
        internal void Draw(NativeWorldSurface surface,double x,double y,double endX,double endY,double alpha,double time,Color? tint=null,string branch="",double final=0)
        {
            if(_used==_bolts.Count)_bolts.Add(new Bolt());var bolt=_bolts[_used++];
            double dx=endX-x,dy=endY-y,len=Math.Sqrt(dx*dx+dy*dy);if(len<.01)return;double nx=-dy/len,ny=dx/len;
            double spread=Math.Min(branch=="chain_storm"?24:18,len*.10),phase=Math.Floor(time*24);
            bolt.Points[0]=new Vector2((float)x,(float)y);
            for(int i=1;i<16;i++){double q=i/16d,envelope=Math.Sin(q*Math.PI),j=(Math.Sin(i*12.31+phase*1.73)*.65+Math.Sin(i*4.17-phase*2.1)*.35)*spread*envelope;bolt.Points[i]=new Vector2((float)(x+dx*q+nx*j),(float)(y+dy*q+ny*j));}bolt.Points[16]=new Vector2((float)endX,(float)endY);
            NativeWorldShapes.StrokePolyline(bolt.Points,4.8f,bolt.Glow,surface.NativeStrokeCorner,surface.NativeStrokeMiter);NativeWorldShapes.StrokePolyline(bolt.Points,1.15f,bolt.Core,surface.NativeStrokeCorner,surface.NativeStrokeMiter);
            var color=tint??new Color(.41f,.92f,1);color.a=Mathf.Clamp01((float)(.22*alpha));bolt.GlowProperties.SetColor("_NativeColor",color);var hot=Color.Lerp(color,Color.white,.8f);hot.a=Mathf.Clamp01((float)(.94*alpha));bolt.CoreProperties.SetColor("_NativeColor",hot);
            surface.DrawNativeMesh(bolt.Glow,_material,bolt.GlowProperties,inheritBlend:false,additive:true,inheritShadow:false);surface.DrawNativeMesh(bolt.Core,_material,bolt.CoreProperties,inheritBlend:false,additive:true,inheritShadow:false);
            // Connected return strokes have zero-width retraced joins, never random full-screen glow.
            int k=0;bolt.BranchPoints[k++]=bolt.Points[4];
            for(int i=0;i<3;i++){int at=4+i*4;var origin=bolt.Points[at];float side=i%2==0?1:-1,length=(float)Math.Min(18,len*.12)*(branch=="chain_web"?1.4f:1);var tip=origin+new Vector2((float)(dx/len*.45+nx*side),(float)(dy/len*.45+ny*side))*length;bolt.BranchPoints[k++]=origin;bolt.BranchPoints[k++]=Vector2.Lerp(origin,tip,.5f)+new Vector2((float)nx,(float)ny)*2;bolt.BranchPoints[k++]=tip;bolt.BranchPoints[k++]=origin;}
            NativeWorldShapes.StrokePolyline(bolt.BranchPoints,.6f,bolt.Branches,surface.NativeStrokeCorner,surface.NativeStrokeMiter);surface.DrawNativeMesh(bolt.Branches,_material,bolt.GlowProperties,inheritBlend:false,additive:true,inheritShadow:false);
        }
        public void Dispose(){foreach(var bolt in _bolts)bolt.Dispose();_bolts.Clear();UnityEngine.Object.Destroy(_material);}
    }
}
