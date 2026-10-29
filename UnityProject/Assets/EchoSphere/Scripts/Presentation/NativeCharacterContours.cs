using System;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    // One authored, retained design for both the live player and baked UI portraits.
    // Coordinates are in player radii. No bitmap portrait, topology rebuild or blur per part.
    internal sealed class NativeCharacterContours : IDisposable
    {
        private sealed class Part : IDisposable
        {
            internal Mesh Fill, Stroke, Halo;
            internal Matrix4x4 Local;
            internal Color? Accent;
            internal bool White;
            internal float Tone;
            public void Dispose(){Destroy(Fill);Destroy(Stroke);Destroy(Halo);}
        }
        private sealed class Rig : IDisposable
        {
            internal readonly List<Part> Parts=new List<Part>();
            public void Dispose(){foreach(var part in Parts)part.Dispose();}
        }
        private readonly Dictionary<string,Rig> _rigs=new Dictionary<string,Rig>(StringComparer.Ordinal);
        private readonly List<MaterialPropertyBlock> _blocks=new List<MaterialPropertyBlock>();
        private readonly Material _solid=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Contour character parts",hideFlags=HideFlags.DontSave};
        private int _used;
        internal void BeginFrame(){_used=0;}
        internal int RigCount=>_rigs.Count;
        private MaterialPropertyBlock Properties(Color color)
        {
            if(_used==_blocks.Count)_blocks.Add(new MaterialPropertyBlock());
            var result=_blocks[_used++];result.Clear();result.SetVector("_NativeColor",color);return result;
        }
        internal void Draw(NativeWorldSurface surface,string id,Color color,Matrix4x4 root,float opacity=1)
        {
            id=id??"spherist";
            if(!_rigs.TryGetValue(id,out var rig)){rig=Build(id);_rigs.Add(id,rig);}
            foreach(var part in rig.Parts){
                var local=root*part.Local;
                var edge=part.White?new Color(.79f,.96f,1,1):part.Accent??color;
                var fill=Color.Lerp(new Color(.018f,.047f,.080f,1),part.Accent??color,part.Tone);
                if(part.Fill!=null)surface.DrawNativeMesh(part.Fill,_solid,Properties(fill),local,opacity:opacity,inheritShadow:false);
                var halo=edge;halo.a=.12f;
                if(part.Halo!=null)surface.DrawNativeMesh(part.Halo,_solid,Properties(halo),local,opacity:opacity,additive:true,inheritShadow:false);
                if(part.Stroke!=null)surface.DrawNativeMesh(part.Stroke,_solid,Properties(edge),local,opacity:opacity,additive:true,inheritShadow:false);
            }
        }
        private static Vector2 P(float x,float y)=>new Vector2(x,y);
        private static Matrix4x4 Pose(float x=0,float y=0,float angle=0)=>Matrix4x4.TRS(new Vector3(x,y,0),Quaternion.Euler(0,0,angle*180/Mathf.PI),Vector3.one);
        private static Rig Build(string id)
        {
            var rig=new Rig();
            void Add(BezierContour contour,bool fill=false,float x=0,float y=0,float angle=0,bool white=false,Color? accent=null,float width=.027f,float tone=.12f){
                var part=new Part{Local=Pose(x,y,angle),White=white,Accent=accent,Tone=tone};
                if(fill)part.Fill=Tessellate(contour,0);
                if(width>0){part.Stroke=Tessellate(contour,width,cap:PathEnding.Round,corners:PathCorner.Round);
                part.Halo=Tessellate(contour,width*2.7f,cap:PathEnding.Round,corners:PathCorner.Round);}rig.Parts.Add(part);
            }
            void Arc(float rx,float ry,float start,float end,float angle=0,bool white=false,float x=0,float y=0)=>Add(EllipseContour(Vector2.zero,rx,ry,start,end),angle:angle,white:white,x:x,y:y);
            void Poly(bool closed,bool fill,params Vector2[] points)=>Add(PolylineContour(closed,points),fill);
            void Diamond(float x,float y,float size,Color? accent=null)=>Add(PolylineContour(true,P(0,-size),P(size*.65f,0),P(0,size),P(-size*.65f,0)),true,x,y,accent:accent,width:.018f,tone:.65f);
            void Fin(float a,float length=1.18f,float breadth=.18f){
                Add(PolylineContour(true,P(.72f,-breadth),P(length,0),P(.72f,breadth),P(.82f,0)),true,angle:a,white:true,tone:.32f);
                Add(PolylineContour(true,P(.81f,-breadth*.60f),P(length-.09f,0),P(.90f,.015f)),true,angle:a,width:0,tone:.70f);
                Add(PolylineContour(false,P(.75f,breadth*.52f),P(.87f,.025f)),angle:a,white:true,width:.015f);
                Add(PolylineContour(false,P(.84f,0),P(length-.07f,0)),angle:a,width:.017f);
            }
            void Crescent(float angle,float reach=1.15f){
                Add(QuadraticContour(P(-.46f,-.77f),true,P(.10f,-1.13f),P(.80f,-reach),P(.55f,-.80f),P(.24f,-.67f),P(-.11f,-.72f),P(-.46f,-.77f)),true,angle:angle,tone:.30f);
                Add(PolylineContour(true,P(-.27f,-.79f),P(.65f,-reach+.07f),P(.22f,-.79f)),true,angle:angle,width:0,tone:.66f);
                Add(PolylineContour(false,P(-.32f,-.79f),P(.18f,-.96f),P(.57f,-reach+.07f)),angle:angle,white:true,width:.014f);
            }
            // Every identity shares the same light core collar, never a solid armor shell.
            Arc(.55f,.55f,0,Mathf.PI*2,white:true);
            Arc(.63f,.63f,-.78f,.78f);Arc(.63f,.63f,2.36f,3.93f);
            // Broad energy cells and doubled collars follow the approved concept;
            // they are flat colored pieces, not lit metal or extruded armor.
            Add(PolylineContour(true,P(-.32f,-.31f),P(-.10f,-.44f),P(.02f,-.28f),P(-.10f,-.09f),P(-.38f,-.13f)),true,width:0,tone:.92f);
            Add(PolylineContour(true,P(.16f,-.37f),P(.36f,-.24f),P(.43f,.04f),P(.30f,.15f),P(.18f,-.04f)),true,width:0,tone:.72f);
            Add(PolylineContour(true,P(-.19f,.25f),P(.03f,.35f),P(.20f,.24f),P(.24f,.39f),P(.02f,.47f),P(-.15f,.40f)),true,width:0,tone:.80f);
            Arc(.59f,.59f,-2.40f,-1.04f,white:true);Arc(.69f,.69f,.17f,.72f);Arc(.69f,.69f,3.31f,3.86f);
            switch(id){
                case "spherist":
                    Arc(1.05f,.88f,.18f,2.96f);Arc(1.05f,.88f,3.32f,6.10f);
                    for(int i=0;i<4;i++)Fin(i*Mathf.PI*.5f,i%2==0?1.22f:1.26f,i%2==0?.15f:.19f);
                    Diamond(.79f,-.63f,.065f);Diamond(-.79f,.63f,.065f);
                    Arc(.96f,.80f,.23f,.82f,white:true);Arc(.96f,.80f,3.37f,3.96f,white:true);
                    Add(PolylineContour(false,P(-.60f,-.27f),P(-.78f,-.20f),P(-.89f,-.08f)),white:true,width:.017f);
                    Add(PolylineContour(false,P(.60f,.27f),P(.78f,.20f),P(.89f,.08f)),white:true,width:.017f);break;
                case "hunter":
                    Crescent(-.12f,1.15f);Crescent(Mathf.PI+.16f,1.15f);
                    Arc(.86f,.86f,-.65f,.82f);Arc(.86f,.86f,2.50f,3.90f);
                    Poly(false,false,P(-.89f,.43f),P(-1.12f,.76f),P(-.61f,.62f));
                    Diamond(.87f,.10f,.075f);Diamond(-.80f,-.22f,.06f);
                    Add(PolylineContour(true,P(-.90f,-.29f),P(-1.12f,-.61f),P(-.78f,-.45f),P(-.68f,-.21f)),true,tone:.30f);
                    Add(PolylineContour(true,P(.66f,.33f),P(.99f,.58f),P(.77f,.61f),P(.57f,.43f)),true,tone:.42f);
                    Arc(.76f,.76f,-2.69f,-1.91f,white:true);Arc(.76f,.76f,.45f,1.21f,white:true);break;
                case "engineer":
                    for(int i=0;i<2;i++){
                        float a=i*Mathf.PI;
                        Add(PolylineContour(true,P(.77f,-.55f),P(.96f,-.42f),P(.96f,.42f),P(.77f,.55f),P(.77f,.31f),P(.84f,.24f),P(.84f,-.24f),P(.77f,-.31f)),true,angle:a,white:true);
                        Add(PolylineContour(true,P(-.32f,-.94f),P(.32f,-.94f),P(.40f,-.85f),P(.31f,-.76f),P(-.31f,-.76f),P(-.40f,-.85f)),true,angle:a,white:true);
                        Add(PolylineContour(false,P(-.22f,-.85f),P(.22f,-.85f)),angle:a);
                        Add(PolylineContour(true,P(.64f,-.13f),P(.71f,-.13f),P(.71f,.13f),P(.64f,.13f)),true,angle:a,accent:new Color(1,.68f,.28f),tone:.65f);
                        Diamond(0,i==0?-1.09f:1.09f,.045f);
                        Add(PolylineContour(false,P(.78f,-.34f),P(.88f,-.26f),P(.88f,.26f),P(.78f,.34f)),angle:a,width:.015f);
                        Add(PolylineContour(true,P(.91f,-.075f),P(1.045f,-.075f),P(1.045f,.075f),P(.91f,.075f)),true,angle:a,tone:.62f,width:.018f);
                        Add(PolylineContour(true,P(-.26f,-.89f),P(.26f,-.89f),P(.22f,-.82f),P(-.22f,-.82f)),true,angle:a,width:0,tone:.50f);
                        Add(PolylineContour(false,P(-.19f,-1.025f),P(.19f,-1.025f)),angle:a,width:.015f);
                    }break;
                case "berserker":
                    for(int i=0;i<3;i++){
                        float a=i*Mathf.PI*2/3;
                        Add(PolylineContour(true,P(.56f,-.48f),P(.89f,-.62f),P(1.15f,-.22f),P(.92f,-.31f),P(.81f,.04f),P(.73f,-.24f)),true,angle:a);
                        Arc(.92f,.92f,-.17f,.51f,a,white:true);
                        Add(PolylineContour(true,P(.68f,-.45f),P(.86f,-.53f),P(.98f,-.33f),P(.84f,-.36f),P(.77f,-.16f)),true,angle:a,width:0,tone:.58f);
                        Add(PolylineContour(false,P(.65f,-.63f),P(.80f,-.74f),P(.93f,-.65f)),angle:a,white:true,width:.017f);
                    }break;
                case "alchemist":
                    Arc(1.01f,.48f,0,Mathf.PI*2,.35f);Arc(1.01f,.48f,0,Mathf.PI*2,-.35f);
                    Diamond(0,-1.04f,.21f);Diamond(-.89f,.40f,.15f,new Color(1,.74f,.28f));Diamond(.89f,.40f,.15f,new Color(.34f,.72f,1));
                    Arc(.77f,.77f,.68f,2.46f);
                    Arc(.96f,.43f,.25f,1.35f,.35f,white:true);Arc(.96f,.43f,3.39f,4.49f,-.35f,white:true);
                    Add(PolylineContour(false,P(-.12f,-1.04f),P(0,-.94f),P(.12f,-1.04f)),white:true,width:.015f);
                    Diamond(0,.77f,.075f);break;
                case "architect":
                    Poly(true,false,P(0,-1.12f),P(.97f,.64f),P(-.97f,.64f));
                    Add(PolylineContour(true,P(-.71f,-.61f),P(.71f,-.61f),P(.71f,.81f),P(-.71f,.81f)),angle:.22f,white:true,width:.018f);
                    Diamond(0,-1.12f,.085f);Diamond(.97f,.64f,.085f);Diamond(-.97f,.64f,.085f);
                    Arc(.77f,.77f,-.20f,1.0f);
                    Poly(false,false,P(-.60f,.56f),P(0,-.91f),P(.60f,.56f));
                    for(int i=0;i<3;i++){float a=i*Mathf.PI*2/3;Add(PolylineContour(true,P(-.07f,-.86f),P(0,-1.0f),P(.07f,-.86f)),true,angle:a,width:0,tone:.65f);}
                    Arc(.83f,.83f,1.54f,2.81f,white:true);break;
                case "conductor":
                    for(int i=0;i<2;i++){
                        float a=i*Mathf.PI;
                        Add(PolylineContour(true,P(-.18f,-.76f),P(.36f,-1.20f),P(.17f,-.86f),P(.46f,-.91f),P(.12f,-.51f),P(.19f,-.75f)),true,angle:a,white:true);
                        Add(PolylineContour(false,P(.02f,-.77f),P(.15f,-.96f),P(.10f,-.80f)),angle:a,width:.017f);
                        Arc(.82f,.82f,-.52f,.49f,a,white:true);
                        Add(PolylineContour(true,P(.78f,-.43f),P(1.02f,-.47f),P(.94f,-.28f)),true,angle:a,tone:.42f);
                        Arc(.94f,.94f,-.75f,.68f,a);Diamond(i==0?.91f:-.91f,i==0?.24f:-.24f,.07f);
                    }break;
                case "oracle":
                    Add(QuadraticContour(P(-1.10f,0),true,P(0,-.84f),P(1.10f,0),P(0,.84f),P(-1.10f,0)),white:true);
                    Arc(.83f,.83f,-2.58f,-.56f);Arc(.83f,.83f,.56f,2.58f);
                    Diamond(0,-1.02f,.17f);Diamond(0,1.02f,.17f);
                    Add(QuadraticContour(P(-.91f,0),false,P(-.45f,-.36f),P(-.14f,-.31f)),width:.018f);
                    Add(QuadraticContour(P(.91f,0),false,P(.45f,.36f),P(.14f,.31f)),width:.018f);
                    Add(PolylineContour(false,P(0,-1.13f),P(.055f,-1.02f),P(0,-.91f)),white:true,width:.015f);
                    Arc(.96f,.96f,-2.39f,-1.92f,white:true);Arc(.96f,.96f,.75f,1.22f,white:true);break;
                case "voidwalker":
                    for(int i=0;i<3;i++){
                        float a=i*Mathf.PI*2/3+.24f;
                        Arc(.94f,.94f,-.70f,.38f,a,white:true);
                        Add(PolylineContour(true,P(.71f,.31f),P(1.10f,.25f),P(.89f,.62f),P(.88f,.41f)),true,angle:a,tone:.28f);
                        Add(PolylineContour(true,P(.81f,.34f),P(1.01f,.30f),P(.91f,.47f)),true,angle:a,width:0,tone:.65f);
                        Arc(.84f,.84f,-.63f,-.05f,a);
                        Add(PolylineContour(false,P(.68f,-.63f),P(.84f,-.58f),P(.90f,-.40f)),angle:a,width:.018f);
                    }
                    Arc(.71f,.71f,.13f,1.12f);break;
                case "fractal":
                    for(int i=0;i<3;i++){
                        float a=i*Mathf.PI*2/3;
                        Add(PolylineContour(true,P(0,-1.13f),P(.28f,-.66f),P(-.28f,-.66f)),angle:a,white:true);
                        Add(PolylineContour(false,P(-.19f,-.72f),P(0,-1.02f),P(.19f,-.72f)),angle:a);
                        Diamond(Mathf.Cos(a+.53f)*.92f,Mathf.Sin(a+.53f)*.92f,.065f);
                        Add(PolylineContour(true,P(-.15f,-.73f),P(0,-.99f),P(.15f,-.73f)),true,angle:a,width:0,tone:.24f);
                        Add(PolylineContour(false,P(-.25f,-.63f),P(0,-1.06f),P(.25f,-.63f)),angle:a,width:.014f);
                        Arc(.89f,.89f,-.80f,-.35f,a);
                    }break;
                default:Fin(-Mathf.PI*.5f);Fin(Mathf.PI*.5f);Arc(.94f,.94f,0,Mathf.PI*2);break;
            }
            return rig;
        }
        private static void Destroy(UnityEngine.Object obj){if(obj!=null)UnityEngine.Object.Destroy(obj);}
        public void Dispose(){foreach(var rig in _rigs.Values)rig.Dispose();_rigs.Clear();Destroy(_solid);}
    }
}
