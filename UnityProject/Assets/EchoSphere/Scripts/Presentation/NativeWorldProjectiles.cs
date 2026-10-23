using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    // Authored renderer.ts projectile geometry. Simulation and hit detection remain
    // with the existing projectile entities; this owner only submits presentation.
    internal sealed class NativeWorldProjectiles : IDisposable
    {
        private sealed class Art : IDisposable
        {
            internal readonly Mesh Tail,InnerTail,Body,Outline,Core,Orbit;
            internal readonly Mesh[] Arcs;
            internal Art(float r,int kind,PathEnding ending,PathCorner corner,float miter)
            {
                Mesh Stroke(Vector2 a,Vector2 b,float width)=>Line(a,b,width,cap:ending);
                Mesh Ring(float radius,float sweep,float width)=>Arc(radius,sweep,width,ending,corner,miter);
                bool enemy=kind<2;
                Tail=Stroke(new Vector2(-r*(enemy?4.8f:3.8f),0),new Vector2(-r*.35f,0),Mathf.Max(2,r*(enemy?.55f:.45f)));
                if(!enemy)InnerTail=Stroke(new Vector2(-r*3.2f,0),new Vector2(-r*.4f,0),Mathf.Max(.6f,r*.08f));
                Vector2[] points=null;float width;
                if(kind==0)points=new[]{new Vector2(r*1.35f,0),new Vector2(0,-r*.72f),new Vector2(-r*.95f,0),new Vector2(0,r*.72f)};
                if(kind==1)points=new[]{new Vector2(r*1.9f,0),new Vector2(-r*.8f,-r*.60f),new Vector2(-r*.45f,0),new Vector2(-r*.8f,r*.60f)};
                if(kind==2)points=new[]{new Vector2(r*1.35f,0),new Vector2(r*.25f,-r*.75f),new Vector2(-r*.8f,-r*.42f),new Vector2(-r*.45f,r*.42f),new Vector2(r*.25f,r*.75f)};
                if(kind==3)points=new[]{new Vector2(r*1.5f,0),new Vector2(-r*.45f,-r*.62f),new Vector2(-r*.95f,0),new Vector2(-r*.45f,r*.62f)};
                width=Mathf.Max(1,r*(enemy?.14f:kind==3?.11f:.10f));
                if(points!=null){Body=Polygon(points);Outline=Polygon(points,width,corner,miter);}
                else{float rad=r*(kind==4?.65f:.72f);Body=Circle(Vector2.zero,rad);Outline=Ring(rad,Mathf.PI*2,width);}
                Core=Circle(enemy?new Vector2(r*.35f,0):Vector2.zero,enemy?Mathf.Max(1.4f,r*.24f):1);
                if(kind==2||kind==5){Arcs=new Mesh[kind==2?3:4];for(int i=0;i<Arcs.Length;i++)Arcs[i]=Ring(r*(kind==2?1.05f+i*.18f:.95f+i*.22f),kind==2?1.1f:1,width);}
                if(kind==3){Arcs=new Mesh[2];for(int i=0;i<2;i++){float side=i==0?-1:1;Arcs[i]=Stroke(new Vector2(-r*.1f,side*r*.25f),new Vector2(r*.9f,side*r*.55f),width);}}
                if(kind==4){Orbit=Circle(Vector2.zero,r*.12f);Arcs=new[]{Ring(r*.12f,Mathf.PI*2,width)};}
            }
            public void Dispose(){Destroy(Tail);Destroy(InnerTail);Destroy(Body);Destroy(Outline);Destroy(Core);Destroy(Orbit);if(Arcs!=null)foreach(var mesh in Arcs)Destroy(mesh);}
        }
        private readonly Dictionary<(float,int,PathEnding,PathCorner,float),Art> _art=new Dictionary<(float,int,PathEnding,PathCorner,float),Art>();
        private readonly Material _material=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Native hostile projectiles",hideFlags=HideFlags.DontSave};
        private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();private int _used;
        internal void BeginFrame(){_used=0;}
        private MaterialPropertyBlock Properties(Color c){if(_used==_properties.Count)_properties.Add(new MaterialPropertyBlock());var p=_properties[_used++];p.SetVector("_NativeColor",new Vector4(c.r,c.g,c.b,c.a));return p;}
        private static Color Alpha(Color c,float a){c.a=a;return c;}
        private static Color Html(string value){ColorUtility.TryParseHtmlString(value,out var c);return c;}
        internal void Draw(NativeWorldSurface surface,float x,float y,float r,double vx,double vy,int kind,Color color,double time)
        {
            var key=(r,kind,surface.NativeStrokeEnding,surface.NativeStrokeCorner,surface.NativeStrokeMiter);if(!_art.TryGetValue(key,out var art)){art=new Art(r,kind,key.Item3,key.Item4,key.Item5);_art.Add(key,art);}
            bool enemy=kind<2;var transform=Matrix4x4.TRS(new Vector3(x,y,0),Quaternion.Euler(0,0,(float)Math.Atan2(vy,vx)*Mathf.Rad2Deg),Vector3.one);
            void Paint(Mesh mesh,Color tint,Matrix4x4? local=null,bool additive=false){surface.DrawNativeMesh(mesh,_material,Properties(tint),transform*(local??Matrix4x4.identity),shadow:enemy?(Color?)color:null,shadowBlur:enemy?7:0,inheritShadow:!enemy,additive:additive,inheritShadowOffsets:enemy);}
            Paint(art.Tail,Alpha(color,enemy?.30f:.20f),additive:enemy);
            if(!enemy)Paint(art.InnerTail,new Color(245/255f,252/255f,1,.65f));
            Paint(art.Body,Html(enemy?"#07101a":kind==2?"#06101c":kind==3?"#130b06":kind==4?"#0c0717":"#120511"));Paint(art.Outline,Alpha(color,enemy?1:.95f));
            if(kind==2||kind==5){for(int i=0;i<art.Arcs.Length;i++)Paint(art.Arcs[i],Alpha(color,kind==2?.55f:.68f),Matrix4x4.Rotate(Quaternion.Euler(0,0,(float)(time*(kind==2?4:3)+i*(kind==2?2.1:Math.PI*2/4))*Mathf.Rad2Deg)));}
            if(kind==3)foreach(var mesh in art.Arcs)Paint(mesh,new Color(1,232/255f,190/255f,.72f));
            if(kind==4)for(int i=0;i<5;i++){double q=time*2.6+i*Math.PI*2/5;var local=Matrix4x4.Translate(new Vector3((float)(Math.Cos(q)*r*1.15),(float)(Math.Sin(q)*r*1.15),0));Paint(art.Orbit,Html("#d9c7ff"),local);Paint(art.Arcs[0],Alpha(color,.6f),local);}
            float coreR=enemy?1:(float)(r*.22*(1+.12*Math.Sin(time*10)));var coreTransform=enemy?Matrix4x4.identity:Matrix4x4.TRS(new Vector3(r*.35f,0,0),Quaternion.identity,new Vector3(coreR,coreR,1));Paint(art.Core,enemy?Color.white:Alpha(color,.65f),coreTransform,!enemy);

        }
        private static void Destroy(UnityEngine.Object value){if(value!=null)UnityEngine.Object.Destroy(value);}
        public void Dispose(){foreach(var art in _art.Values)art.Dispose();_art.Clear();Destroy(_material);}
    }
}
