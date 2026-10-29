using System;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    internal sealed class NativeWorldPlayer : IDisposable
    {
        internal struct State
        {
            internal double X,Y,Radius,Time,Hp,ContactCooldown,ShieldTimer,ShieldCharges,DashTimer,DeathTimer;
            internal string Character;
            internal Color Color;
        }
        private sealed class Art : IDisposable
        {
            internal Mesh Fill,Stroke;internal BezierContour[] Paths;
            internal Art(BezierContour[] paths,float width,NativeWorldSurface s,bool fill){Build(paths,width,s,fill);}
            internal void Build(BezierContour[] paths,float width,NativeWorldSurface s,bool fill)
            {Paths=paths;if(fill)Fill=Tessellate(paths[0],0,Fill,contours:paths);if(width>0)Stroke=Tessellate(paths[0],width,Stroke,cap:s.NativeStrokeEnding,corners:s.NativeStrokeCorner,miter:s.NativeStrokeMiter,contours:paths);}
            public void Dispose(){Destroy(Fill);Destroy(Stroke);}
        }
        private readonly Dictionary<(int,double,double,PathEnding,PathCorner,float),Art> _art=new Dictionary<(int,double,double,PathEnding,PathCorner,float),Art>();
        private readonly NativeCharacterContours _contours=new NativeCharacterContours();
        private readonly Material _solid=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Native player silhouette",hideFlags=HideFlags.DontSave};
        private readonly Material _core=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldPlayerCore")){name="Native player core",hideFlags=HideFlags.DontSave};
        private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();private int _used;
        private readonly List<Art> _dynamic=new List<Art>();private int _dynamicUsed;
        private struct TrailSample {internal double X,Y,Time;}
        private readonly TrailSample[] _trail=new TrailSample[18];
        private int _trailHead,_trailCount,_cometPointCount;
        private Mesh _cometMesh;private Material _cometMaterial;
        private readonly MaterialPropertyBlock _cometProperties=new MaterialPropertyBlock();
        private readonly List<Vector3> _cometVertices=new List<Vector3>(38);
        private readonly List<Vector2> _cometUvs=new List<Vector2>(38);
        private readonly List<Color> _cometColors=new List<Color>(38);
        private readonly List<int> _cometTriangles=new List<int>(108);
        private string _trailCharacter;
        private double _lastX,_lastY,_lastTime=-1;
        private NativeWorldSurface _surface;
        internal void BeginFrame(){_used=0;_dynamicUsed=0;_contours.BeginFrame();}
        private static Vector2 P(double x,double y)=>new Vector2((float)x,(float)y);
        private static Color C(int r,int g,int b,double a=1)=>new Color(r/255f,g/255f,b/255f,(float)a);
        private static Color A(Color color,double alpha){color.a=(float)alpha;return color;}
        private static Matrix4x4 T(double x=0,double y=0,double angle=0,double sx=1,double sy=1)=>Matrix4x4.TRS(new Vector3((float)x,(float)y,0),Quaternion.Euler(0,0,(float)angle*Mathf.Rad2Deg),new Vector3((float)sx,(float)sy,1));
        private MaterialPropertyBlock Properties(Color color){if(_used==_properties.Count)_properties.Add(new MaterialPropertyBlock());var p=_properties[_used++];p.Clear();p.SetVector("_NativeColor",new Vector4(color.r,color.g,color.b,color.a));return p;}
        private Art Get(int kind,double r,double width,Func<BezierContour[]> create,bool fill=true)
        {var key=(kind,r,width,_surface.NativeStrokeEnding,_surface.NativeStrokeCorner,_surface.NativeStrokeMiter);if(!_art.TryGetValue(key,out var art)){art=new Art(create(),(float)width,_surface,fill);_art.Add(key,art);}return art;}
        private Art Dynamic(BezierContour[] paths,float width)
        {if(_dynamicUsed==_dynamic.Count)_dynamic.Add(new Art(paths,width,_surface,false));else _dynamic[_dynamicUsed].Build(paths,width,_surface,false);return _dynamic[_dynamicUsed++];}
        private Art Circle(double r)=>Get(0,r,0,()=>new[]{EllipseContour(Vector2.zero,(float)r,(float)r)});
        private void Paint(Art art,Matrix4x4 root,Color? fill,Color? stroke,Color? shadow=null,float blur=0,float? opacity=null,bool additive=false,bool preservePath=false)
        {if(fill.HasValue)_surface.DrawNativeMesh(art.Fill,_solid,Properties(fill.Value),root,shadow,blur,opacity:opacity,additive:additive,inheritShadow:!shadow.HasValue,inheritShadowOffsets:shadow.HasValue);if(stroke.HasValue)_surface.DrawNativeMesh(art.Stroke,_solid,Properties(stroke.Value),root,shadow,blur,opacity:opacity,additive:additive,inheritShadow:!shadow.HasValue,inheritShadowOffsets:shadow.HasValue);}

        internal void Draw(NativeWorldSurface surface,State p,NativeWorldCreatures shadows)
        {
            // Live art never resolves the old portrait/side sprite.
            // Menus bake this same contour rig.
            _surface=surface;double r=p.Radius,t=p.Time,pulse=1+Math.Sin(t*5.5)*.025;
            var root=T(p.X,p.Y-r*.1+Math.Sin(t*2.4)*.8,sx:pulse,sy:pulse);
            DrawMovementTrail(p);
            shadows.DrawGroundShadow(surface,r*.95,r*.25,root);
            float opacity=1;double progress=0;
            if(p.Character=="spherist"){
                if(p.Hp<=0){
                    progress=Math.Min(1,Math.Max(0,1-p.DeathTimer/.90));double fold=progress*progress;
                    var shear=Matrix4x4.identity;shear.m00=(float)(1-fold*.45);shear.m01=(float)(-.10*fold);shear.m11=(float)(1+fold*.06);
                    root*=T(y:-progress*r*.10)*shear*T(sx:pulse*(1-fold*.18),sy:pulse*(1-fold*.18));opacity=(float)(1-progress*.92);
                }else{root*=T(sx:pulse,sy:pulse);}
            }
            var bodyRoot=p.Character=="spherist"&&p.Hp<=0?root*T(angle:progress*progress*.25):root;
            DrawArtwork(surface,p,bodyRoot,opacity);
            if(p.Character=="spherist"){
                if(p.Hp<=0){
                    double fragment=Math.Max(0,(progress-.22)/.78),size=2+fragment*2;
                    var diamond=Get(1,1,0,()=>new[]{PolylineContour(true,P(0,-1),P(.75,0),P(0,1),P(-.75,0))});
                    for(int i=0;i<8;i++){double a=i*Math.PI/4+.18,travel=r*(.10+fragment*.72);Paint(diamond,root*T(Math.Cos(a)*travel,Math.Sin(a)*travel,a+fragment*1.3,size,size),C(223,250,255),null,opacity:(float)((1-fragment)*.72));}
                }else{
                    double spark=.45+.35*Math.Sin(t*5.5);
                    var arc=Get(2,r,1,()=>new[]{EllipseContour(Vector2.zero,(float)(r*1.18),(float)(r*1.18),-.55f,.55f)},false);
                    Paint(arc,root*T(angle:t*.7),null,C(100,232,255,.24+spark*.18),additive:true);
                    var diamond=Get(3,0,0,()=>new[]{PolylineContour(true,P(0,-1.8),P(1.8,0),P(0,1.8),P(-1.8,0))});
                    for(int i=0;i<4;i++){double a=t*.9+i*Math.PI/2,rr=r*(1.08+.08*Math.Sin(t*3+i));Paint(diamond,root*T(Math.Cos(a)*rr,Math.Sin(a)*rr*.70),C(220,252,255,.34+spark*.20),null,additive:true);}
                }
            }else DrawCharacterEffects(surface,p);
        }
        private void DrawMovementTrail(State p)
        {
            double dx=p.X-_lastX,dy=p.Y-_lastY,dt=p.Time-_lastTime;
            bool discontinuity=_lastTime<0||_trailCharacter!=p.Character||dt<0||dt>.50||dx*dx+dy*dy>p.Radius*p.Radius*36;
            if(discontinuity||p.Hp<=0){_trailCount=0;_trailHead=0;}
            else if(dt>0&&dx*dx+dy*dy>.01){
                _trail[_trailHead]=new TrailSample{X=_lastX,Y=_lastY,Time=p.Time};
                _trailHead=(_trailHead+1)%_trail.Length;_trailCount=Math.Min(_trailCount+1,_trail.Length);
            }
            _lastX=p.X;_lastY=p.Y;_lastTime=p.Time;_trailCharacter=p.Character;
            bool active=false;for(int i=0;i<_trailCount;i++){var sample=_trail[(_trailHead-_trailCount+i+_trail.Length)%_trail.Length];if(p.Time-sample.Time<.38)active=true;}
            if(!active)return;
            if(_cometMesh==null){_cometMesh=new Mesh{name="Retained player comet ribbon"};_cometMesh.MarkDynamic();_cometMaterial=new Material(Resources.Load<Shader>("EchoSphere/NativeCharacterCometTrail")){name="Player comet trail",hideFlags=HideFlags.DontSave};}
            TrailSample Point(int i)=>i==_trailCount?new TrailSample{X=p.X,Y=p.Y,Time=p.Time}:_trail[(_trailHead-_trailCount+i+_trail.Length)%_trail.Length];
            _cometVertices.Clear();_cometUvs.Clear();_cometColors.Clear();int count=_trailCount+1;
            for(int i=0;i<count;i++){
                var at=Point(i);var before=Point(Math.Max(0,i-1));var after=Point(Math.Min(count-1,i+1));
                double tx=after.X-before.X,ty=after.Y-before.Y,length=Math.Sqrt(tx*tx+ty*ty);if(length<.0001){tx=1;ty=0;length=1;}
                double fade=Math.Max(0,1-(p.Time-at.Time)/.38),width=p.Radius*(.08+.32*fade);var tint=A(p.Color,.14*fade*fade);
                for(int side=-1;side<=1;side+=2){_cometVertices.Add(new Vector3((float)(at.X-ty/length*width*side),(float)(at.Y-p.Radius*.10+tx/length*width*side),0));_cometUvs.Add(new Vector2(i/(float)Math.Max(1,count-1),side<0?0:1));_cometColors.Add(tint);}
            }
            bool topology=count!=_cometPointCount;if(topology)_cometMesh.Clear();
            _cometMesh.SetVertices(_cometVertices);_cometMesh.SetUVs(0,_cometUvs);_cometMesh.SetColors(_cometColors);
            if(topology){_cometTriangles.Clear();for(int i=0;i<count-1;i++){int v=i*2;_cometTriangles.Add(v);_cometTriangles.Add(v+1);_cometTriangles.Add(v+2);_cometTriangles.Add(v+2);_cometTriangles.Add(v+1);_cometTriangles.Add(v+3);}_cometMesh.SetTriangles(_cometTriangles,0);_cometPointCount=count;}
            _cometMesh.RecalculateBounds();_surface.DrawNativeMesh(_cometMesh,_cometMaterial,_cometProperties,inheritShadow:false,additive:true);
        }
        // UI sprites are generated in Editor from the exact live body, without
        // ground/contact/status effects or the run-time bob/death pose.
        internal void DrawPortrait(NativeWorldSurface surface,State p)=>DrawArtwork(surface,p,T(p.X,p.Y),1);
        private void DrawArtwork(NativeWorldSurface surface,State p,Matrix4x4 root,float opacity)
        {
            _surface=surface;var core=Circle(p.Radius*.50);var properties=Properties(p.Color);properties.SetFloat("_Radius",(float)p.Radius);
            surface.DrawNativeMesh(core.Fill,_core,properties,root,opacity:opacity,inheritShadow:false);
            _contours.Draw(surface,p.Character,p.Color,root*Matrix4x4.Scale(Vector3.one*(float)p.Radius),opacity);
        }
        internal void DrawCharacterEffects(NativeWorldSurface surface,State p)
        {
            _surface=surface;double r=p.Radius,t=p.Time;var root=T(p.X,p.Y-r*.1);var color=p.Color;
            if(p.Character=="hunter"){double scan=Math.Sin(t*3.5)*.5+.5;var line=Get(20,r,1.2,()=>new[]{PolylineContour(false,P(-r*.95,0),P(r*.95,0))},false);Paint(line,root*T(y:r*(.15-scan*.25)),null,A(color,.25+scan*.3));}
            else if(p.Character=="engineer"){var square=Get(21,0,0,()=>new[]{PolylineContour(true,P(-3,-3),P(3,-3),P(3,3),P(-3,3))});for(int i=0;i<4;i++){double a=t*.6+i*Math.PI/2;Paint(square,root*T(Math.Cos(a)*r*1.05,Math.Sin(a)*r*.72),A(color,.34),null,preservePath:true);}}
            else if(p.Character=="berserker"){double pulse=1+Math.Sin(t*7)*.08;var path=new[]{PolylineContour(false,P(-r*.9,r*.55*pulse),P(0,r*.85*pulse),P(r*.9,r*.55*pulse))};Paint(Dynamic(path,2),root,null,A(color,.58));}
            else if(p.Character=="alchemist")for(int i=0;i<5;i++){double a=t*.8+i*1.2,rr=r*(.85+.15*Math.Sin(t*2+i));Paint(Circle(2.2+i%2),root*T(Math.Cos(a)*rr,Math.Sin(a)*rr*.6),A(color,.38),null);}
            else if(p.Character=="architect"){var h=Get(22,0,1.2,()=>new[]{PolylineContour(false,P(-5,0),P(5,0))},false);var v=Get(23,0,1.2,()=>new[]{PolylineContour(false,P(0,-5),P(0,5))},false);for(int i=0;i<4;i++){double a=t*.7+i*Math.PI/2;var local=root*T(Math.Cos(a)*r*1.15,Math.Sin(a)*r*.72);Paint(h,local,null,A(color,.46));Paint(v,local,null,A(color,.46));}}

        }
        private static void Destroy(UnityEngine.Object value){if(value!=null)UnityEngine.Object.Destroy(value);}
        public void Dispose(){_contours.Dispose();foreach(var art in _art.Values)art.Dispose();_art.Clear();foreach(var art in _dynamic)art.Dispose();_dynamic.Clear();Destroy(_solid);Destroy(_core);Destroy(_cometMesh);Destroy(_cometMaterial);}
    }
}
