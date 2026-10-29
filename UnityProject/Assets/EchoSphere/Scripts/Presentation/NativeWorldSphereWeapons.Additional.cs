using System;
using UnityEngine;
using Unity.VectorGraphics;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    internal sealed partial class NativeWorldSphereWeapons
    {
        private static bool IsFieldFamily(string family)=>family=="aura"||family=="gravity"||family=="pulse";

        // Radial axis remains outward; only the tangential axis is mirrored.
        // Thus a swept contact edge trails its own ring's actual travel direction.
        internal static Matrix4x4 OrbitalSatellitePose(double x,double y,double radius,double angle,bool innerRing)
            =>T(x,y,angle)*Matrix4x4.Scale(new Vector3((float)radius,(float)(innerRing?radius:-radius),1));
        internal void DrawOrbitalSatellite(NativeWorldSurface surface,State state,bool innerRing)
            =>PaintRig(surface,GetRig("orbital",state.Branch,state.Final),state,OrbitalSatellitePose(state.X,state.Y,state.Radius,state.Rotation,innerRing));

        private sealed class MechanismBuilder
        {
            internal readonly Rig Rig=new Rig();
            internal readonly Color Main,Accent;
            internal static readonly Color White=C("#e4f8ff");
            internal MechanismBuilder(Color main,Color accent){Main=main;Accent=accent;}
            internal static Vector2 P(float x,float y)=>new Vector2(x,y);
            internal static Vector2 Rad(float angle,float radius)=>new Vector2(Mathf.Cos(angle)*radius,Mathf.Sin(angle)*radius);
            internal void Add(BezierContour path,Vector2 rest,Vector2 slide,float swing=0,float angle=0,int channel=2,Color? color=null,bool fill=false,float width=.024f,float recoil=0)
            {
                var part=new Part{Color=color??Main,Rest=rest,Slide=slide,Swing=swing,Angle=angle,Channel=channel,Recoil=recoil};
                if(fill)part.Fill=Tessellate(path,0);
                part.Stroke=Tessellate(path,width,cap:PathEnding.Round,corners:PathCorner.Round);
                part.Halo=Tessellate(path,width*2.2f,cap:PathEnding.Round,corners:PathCorner.Round);
                Rig.Parts.Add(part);
            }
            internal void Plate(float x,float y,float w,float h,float dy=0,float angle=0,Color? color=null,float dx=-.055f,int channel=2,float swing=0)
                =>Add(PolylineContour(true,P(-w*.5f,-h*.5f),P(w*.5f,-h*.5f),P(w*.5f,h*.5f),P(-w*.5f,h*.5f)),P(x,y),P(dx,dy),swing,angle,channel,color,true);
            internal void Arc(float x,float y,float rx,float ry,float begin,float end,Color? color=null,float dx=-.045f,float dy=0,float swing=0)
                =>Add(EllipseContour(Vector2.zero,rx,ry,begin,end),P(x,y),P(dx,dy),swing,color:color);
            internal void Node(float x,float y,float radius,Color? color=null,float dx=-.04f,float dy=0)
                =>Add(EllipseContour(Vector2.zero,radius,radius),P(x,y),P(dx,dy),channel:4,color:color,fill:true);
            internal void Facet(float x,float y,float length,float halfWidth,float angle=0,Color? color=null,float dy=0,float swing=0)
                =>Add(PolylineContour(true,P(-length*.35f,0),P(0,-halfWidth),P(length*.65f,0),P(0,halfWidth)),P(x,y),P(-.055f,dy),swing,angle,color:color,fill:true);
            internal void Collar()
            {
                Arc(.78f,0,.055f,.16f,Mathf.PI*.5f,Mathf.PI*1.5f,White,swing:.12f);
                Arc(.86f,0,.045f,.13f,Mathf.PI*.5f,Mathf.PI*1.5f,swing:-.12f);
            }
            internal void Forks(float x,float y,float length,bool inward=false)
            {
                for(int side=-1;side<=1;side+=2){
                    Add(PolylineContour(false,P(-.12f,0),P(.03f,-side*.035f),P(length,-side*.08f)),P(x,side*y),P(-.055f,side*(inward?-.04f:.04f)),side*.16f,color:Accent);
                    Plate(x+.02f,side*(y+.035f),.10f,.025f,side*.04f,side*.2f,White,swing:side*.15f);
                }
            }
            internal void Teeth(float x,float y,int count,float step=.075f)
            {for(int side=-1;side<=1;side+=2)for(int i=0;i<count;i++)Plate(x+i*step,side*y,.035f,.08f,side*.035f,color:i%2==0?Accent:White,channel:3,swing:side*.12f);}
            // All field motion is radial/counter-rotating. No global gun recoil.
            internal void Rib(float a,float r,float span,float motion=-.04f,Color? color=null)
                =>Add(EllipseContour(P(-r,0),r,r,-span,span),Rad(a,r),Rad(a,motion),Mathf.Sin(a)*.12f,a,color:color,width:.018f);
            internal void Vane(float a,float r,int kind=0,float length=.14f,float motion=.045f,Color? color=null)
            {
                BezierContour path;
                if(kind==1)path=PolylineContour(false,P(-.045f,-.065f),P(.045f,-.065f),P(.045f,.065f),P(-.045f,.065f));
                else if(kind==2)path=PolylineContour(true,P(-length*.4f,-.022f),P(length*.65f,0),P(-length*.4f,.022f),P(-length*.1f,0));
                else if(kind==3)path=QuadraticContour(P(-.045f,-.07f),false,P(.09f,-.045f),P(.10f,.04f),P(.02f,.07f));
                else path=PolylineContour(false,P(-.025f,-.065f),P(.025f,-.03f),P(.025f,.03f),P(-.025f,.065f));
                Add(path,Rad(a,r),Rad(a,motion),Mathf.Sin(a)*.18f,a,color:color,width:.019f);
            }
            internal void RadialNode(float a,float r,Color? color=null)
                =>Add(EllipseContour(Vector2.zero,.028f,.028f),Rad(a,r),Rad(a,.035f),Mathf.Sin(a)*.10f,a,4,color,width:.018f);
        }

        private static Rig BuildAdditional(string family,string branch,int final)
        {
            Color main;
            switch(family){case "chain":main=C("#ffe25b");break;case "aura":main=C("#57e6b4");break;case "prism":main=C("#ff8de1");break;case "gravity":main=C("#a58cff");break;case "pulse":main=C("#ffd35a");break;case "void":main=C("#c28cff");break;case "orbital":main=C("#8ef0ff");break;default:throw new ArgumentException("Unknown sphere art family: "+family);}
            var accent=branch.EndsWith("storm",StringComparison.Ordinal)||branch.EndsWith("resonator",StringComparison.Ordinal)||branch.EndsWith("reaper",StringComparison.Ordinal)?C("#ffb66c"):branch.EndsWith("leech",StringComparison.Ordinal)||branch.EndsWith("hunger",StringComparison.Ordinal)||branch.EndsWith("split",StringComparison.Ordinal)?C("#9ce6bf"):MechanismBuilder.White;
            var b=new MechanismBuilder(main,accent);b.Rig.MainColor=main;
            if(IsFieldFamily(family))BuildField(b,family,branch,final);
            else if(family=="orbital")BuildContactSatellite(b,branch,final);
            else BuildDirected(b,family,branch,final);
            return b.Rig;
        }

        private static void BuildField(MechanismBuilder b,string family,string branch,int final)
        {
            bool advanced=final>=0;int count=advanced?6:4;
            bool network=branch=="aura_overgrowth",well=branch=="gravity_well",tide=branch=="gravity_tide",collapse=branch=="gravity_collapse";
            bool burst=branch=="pulse_burst",resonator=branch=="pulse_resonator",sanctum=branch=="aura_sanctum";
            if(network||burst)count=6;if(burst&&final==2)count=8;
            float step=Mathf.PI*2/count,offset=well||sanctum?0:Mathf.PI/count;
            for(int i=0;i<count;i++){
                float a=i*step+offset;
                if(network){b.RadialNode(a,.82f);b.Rib(a,.67f,.17f,.035f);}
                else if(resonator){b.Rib(a,.71f,.18f,.045f);b.Rib(a,.79f,.16f,.035f);}
                else if(burst){b.Vane(a,.76f,2,.17f,.05f);b.Vane(a+.035f,.65f,2,.08f,.04f);}
                else if(well){b.Vane(a,.77f,1,.14f,-.045f);b.Rib(a,.65f,.15f,-.035f);}
                else if(tide){b.Vane(a,.78f,3,.14f,(i%2==0?1:-1)*.045f);b.Rib(a,.66f,.17f,-.035f);}
                else if(collapse){b.Vane(a+Mathf.PI,.77f,2,.15f,-.05f);b.Rib(a,.66f,.19f,-.045f);}
                else if(family=="aura"&&branch=="aura_gravity"){b.Vane(a,.76f,3,.14f,-.045f);b.Rib(a,.67f,.18f,-.035f);}
                else{b.Rib(a,.75f,.26f,.04f);b.Vane(a,.85f,0,.10f,.045f);}
                if(!advanced)continue;
                // VII adds a distinct second radial layer, never broad armor.
                Color detail=family=="pulse"&&branch=="pulse_wave"&&final>0?C("#d9f6ff"):b.Accent;
                if(network){
                    if(final==0){b.Vane(a,.66f,2,.095f,.045f,detail);b.Rib(a,.58f,.13f,.04f,detail);}
                    else if(final==1){b.Rib(a+.055f,.60f,.15f,.035f,detail);b.RadialNode(a+.13f,.72f,detail);}
                    else{b.Vane(a,.63f,1,.08f,.04f,detail);b.Rib(a,.57f,.11f,.04f,detail);b.RadialNode(a+.09f,.73f,detail);}
                }else if(well){
                    if(final==0){b.Vane(a,.89f,1,.12f,-.055f,detail);b.RadialNode(a,.60f,detail);}
                    else if(final==1){b.Rib(a,.86f,.23f,-.055f,detail);b.Rib(a,.58f,.16f,-.05f,detail);}
                    else{b.Vane(a,.88f,1,.12f,-.055f,detail);b.Rib(a,.56f,.18f,-.055f,detail);b.RadialNode(a+.13f,.71f,detail);}
                }else if(tide){
                    int kind=final==0?2:3;float direction=final==0?.055f:final==1?-.055f:(i%2==0?.055f:-.055f);
                    b.Vane(a,.90f,kind,.19f,direction,detail);b.Rib(a,.58f,.13f,-direction,detail);if(final==2)b.Vane(a+.08f,.65f,2,.09f,-direction,detail);
                }else if(collapse){b.Vane(a+(final==1?Mathf.PI:0),.89f,final==2?0:2,final==0?.18f:.12f,-.055f,detail);b.Rib(a,.55f,.14f,-.04f,detail);if(final==2)b.RadialNode(a,.94f,detail);}
                else if(resonator){b.Rib(a,.61f,.14f,.04f,detail);b.Vane(a,.91f,final==0?0:final==1?1:2,.13f,.055f,detail);if(final==2)b.Vane(a+.07f,.85f,2,.10f,.05f,detail);}
                else if(burst){b.Vane(a+.07f,.87f,final==1?1:2,.14f,.055f,detail);b.Rib(a,.58f,final==2?.20f:.12f,.04f,detail);if(final==2)b.Rib(a,.92f,.13f,.05f,detail);}
                else if(sanctum){b.Vane(a,.93f,final==1?1:2,.13f,-.045f,detail);b.Rib(a,.60f,.14f,-.04f,detail);if(final==2)b.RadialNode(a+.11f,.80f,detail);}
                else if(family=="aura"){b.Vane(a,.90f,final==0?3:final==1?1:0,.13f,-.05f,detail);b.Rib(a,.56f,.13f,-.04f,detail);if(final==2)b.Vane(a+.10f,.66f,2,.08f,.04f,detail);}
                else{b.Vane(a,.94f,final==0?0:2,final==0?.14f:.19f,.055f,detail);b.Rib(a,.59f,.14f,.04f,detail);if(final==2)b.Vane(a+.10f,.83f,2,.085f,.05f,detail);}
            }
        }

        private static void BuildDirected(MechanismBuilder b,string family,string branch,int final)
        {
            b.Collar();bool advanced=final>=0;
            if(family=="chain"){
                b.Plate(1.03f,0,.23f,.23f);b.Plate(1.03f,0,.12f,.14f,color:MechanismBuilder.White,dx:-.035f);
                if(branch=="chain_storm"){b.Arc(1.15f,0,.20f,.25f,-1.2f,1.2f);b.Arc(1.12f,0,.14f,.20f,-1.2f,1.2f,b.Accent,dx:.045f);}
                else{b.Forks(1.06f,.17f,.28f,branch=="chain_leech");for(int side=-1;side<=1;side+=2)b.Node(1.38f,side*.21f,.035f,b.Accent,dy:side*.04f);}
                if(branch=="chain_leech")b.Arc(1.24f,0,.20f,.29f,-1.4f,1.4f,b.Accent);
                if(!advanced)return;
                if(branch=="chain_web"){
                    b.Forks(1.14f,.29f,final==0?.20f:.28f,true);b.Teeth(.98f,.28f,final==1?3:2);
                    if(final!=1)for(int side=-1;side<=1;side+=2)b.Plate(1.43f,side*.15f,.035f,.16f,-side*.045f,color:b.Accent);
                    if(final>0)for(int i=-1;i<=1;i++)b.Node(1.48f,i*.19f,.035f,b.Accent,dy:i*.045f);
                }else if(branch=="chain_storm"){
                    b.Teeth(.98f,.28f,3);b.Forks(1.12f,.27f,.30f);
                    if(final==0){b.Arc(1.23f,0,.22f,.31f,-1.3f,1.3f,MechanismBuilder.White,dx:.05f);}
                    else if(final==1){b.Arc(1.23f,0,.24f,.30f,-1.3f,1.3f,b.Accent);b.Arc(1.23f,0,.30f,.35f,-1.2f,1.2f,MechanismBuilder.White,dx:.035f);}
                    else{for(int side=-1;side<=1;side+=2){b.Arc(1.23f,side*.16f,.24f,.15f,-2.8f,2.8f,MechanismBuilder.White,dy:side*.05f);b.Node(1.45f,side*.16f,.04f,b.Accent,dy:side*.05f);}}
                }else{
                    b.Forks(1.13f,.28f,.30f,true);b.Teeth(1.01f,.29f,2);
                    if(final==0){for(int side=-1;side<=1;side+=2)b.Plate(1.20f,side*.12f,.29f,.035f,side*.04f,color:b.Accent,channel:3);}
                    else if(final==1){for(int i=-1;i<=1;i++)b.Facet(1.38f,i*.17f,.23f,.045f,i*.25f,MechanismBuilder.White,dy:i*.04f,swing:i*.15f);}
                    else{b.Forks(1.21f,.34f,.31f,true);for(int i=-1;i<=1;i++)b.Facet(1.40f,i*.16f,.19f,.04f,i*.2f,MechanismBuilder.White,dy:i*.04f);}
                }
            }else if(family=="prism"){
                b.Facet(1.12f,0,.35f,.11f);b.Forks(1.02f,.17f,.18f);b.Node(1.09f,0,.045f,MechanismBuilder.White);
                if(branch=="prism_mirror")for(int side=-1;side<=1;side+=2)b.Facet(1.25f,side*.16f,.27f,.055f,side*.55f,dy:side*.045f,swing:side*.16f);
                if(branch=="prism_spectrum"){b.Plate(1.10f,-.15f,.20f,.035f,-.035f,color:b.Accent);b.Plate(1.10f,.15f,.20f,.035f,.035f,color:b.Accent);}
                if(!advanced)return;
                if(branch=="prism_split"){
                    if(final==0){for(int i=-2;i<=2;i++)b.Facet(1.36f,i*.12f,.25f,.035f,i*.24f,MechanismBuilder.White,dy:i*.025f,swing:i*.10f);b.Teeth(.98f,.26f,2);}
                    else if(final==1){for(int i=0;i<3;i++)b.Plate(1.18f+i*.12f,0,.035f,.27f,color:MechanismBuilder.White,dx:-.04f-i*.02f);b.Forks(1.12f,.25f,.29f);b.Facet(1.53f,0,.22f,.06f);}
                    else{b.Forks(1.12f,.26f,.29f);for(int side=-1;side<=1;side+=2){b.Facet(1.39f,side*.20f,.26f,.065f,-side*.60f,MechanismBuilder.White,dy:-side*.045f,swing:-side*.20f);b.Node(1.23f,side*.28f,.035f,b.Accent,dy:side*.04f);}}
                }else if(branch=="prism_spectrum"){
                    Color status=final==0?C("#ff9c62"):final==1?C("#d9f6ff"):C("#72f08e");b.Node(1.10f,0,.065f,status);
                    for(int side=-1;side<=1;side+=2){b.Facet(1.08f,side*.25f,.20f,.05f,side*(final==1?.50f:.20f),status,side*.05f,side*.20f);b.Facet(1.35f,side*.19f,.22f,.04f,side*(final==2?.45f:.25f),status,side*.045f,-side*.20f);b.Plate(1.05f,side*.13f,.14f,.025f,side*.035f,color:MechanismBuilder.White);}
                    if(final==1)b.Plate(1.44f,0,.045f,.22f,color:status);if(final==2)b.Teeth(1.06f,.32f,2);
                }else{
                    b.Teeth(.98f,.28f,2);b.Forks(1.10f,.29f,.30f);
                    for(int side=-1;side<=1;side+=2){b.Facet(1.39f,side*.26f,.28f,.065f,side*(final==0?.65f:-.35f),MechanismBuilder.White,side*.05f,side*.18f);if(final>0)b.Facet(1.18f,side*.32f,.22f,.04f,-side*.45f,b.Accent,side*.04f,-side*.18f);}
                    if(final==2){b.Arc(1.22f,0,.28f,.36f,-2.8f,2.8f,b.Accent);for(int i=-1;i<=1;i++)b.Node(1.46f,i*.25f,.035f,MechanismBuilder.White,dy:i*.04f);}
                }
            }else{
                b.Node(1.02f,0,.065f);b.Forks(1.06f,.18f,.26f,true);
                for(int side=-1;side<=1;side+=2)b.Facet(1.25f,side*.19f,.26f,.06f,-side*.45f,dy:-side*.04f,swing:side*.20f);
                if(branch=="void_hunger")b.Arc(1.18f,0,.19f,.27f,-1.3f,1.3f,b.Accent);
                else if(branch=="void_reaper"){for(int side=-1;side<=1;side+=2)b.Facet(1.18f,side*.26f,.36f,.055f,-side*.35f,MechanismBuilder.White,side*.04f,swing:side*.14f);}
                else if(branch=="void_execution"){for(int side=-1;side<=1;side+=2)b.Plate(1.33f,side*.12f,.035f,.13f,-side*.04f,color:MechanismBuilder.White);}
                if(!advanced)return;
                b.Teeth(.99f,.28f,2);
                if(branch=="void_hunger"){
                    b.Forks(1.13f,.29f,.31f,true);
                    if(final==0){for(int i=-1;i<=1;i++)b.Facet(1.42f,i*.15f,.19f,.045f,i*.35f,b.Accent,dy:i*.04f);}
                    else if(final==1){b.Arc(1.20f,0,.23f,.31f,-1.2f,1.2f,MechanismBuilder.White);b.Arc(1.20f,0,.30f,.35f,-1.2f,1.2f,b.Accent,dx:.04f);}
                    else{b.Forks(1.19f,.34f,.29f,true);b.Facet(1.43f,0,.24f,.065f,0,MechanismBuilder.White);b.Node(1.14f,-.18f,.035f,b.Accent,dy:-.04f);b.Node(1.14f,.18f,.035f,b.Accent,dy:.04f);}
                }else if(branch=="void_reaper"){
                    b.Forks(1.08f,.31f,.34f,true);
                    if(final==0){for(int side=-1;side<=1;side+=2){b.Plate(1.20f,side*.12f,.29f,.04f,side*.045f,color:b.Accent,channel:3);b.Facet(1.27f,side*.30f,.32f,.045f,-side*.4f,MechanismBuilder.White,side*.05f);}}
                    else{for(int i=-1;i<=1;i++)b.Facet(1.40f,i*.17f,.26f,.045f,i*.30f,MechanismBuilder.White,dy:i*.05f,swing:i*.20f);if(final==2){b.Forks(1.20f,.36f,.29f,true);b.Teeth(1.20f,.27f,2);}}
                }else{
                    b.Forks(1.11f,.29f,.30f,true);
                    if(final==0){for(int side=-1;side<=1;side+=2){b.Facet(1.37f,side*.25f,.30f,.045f,-side*.30f,MechanismBuilder.White,-side*.05f);b.Plate(1.49f,side*.11f,.04f,.14f,-side*.05f,color:b.Accent);}}
                    else{for(int i=0;i<3;i++)b.Arc(1.23f+i*.12f,0,.035f,.27f,-1.3f,1.3f,MechanismBuilder.White,dx:-.035f-i*.02f);if(final==2){b.Forks(1.20f,.34f,.31f,true);b.Facet(1.48f,0,.25f,.065f,color:b.Accent);}}
                }
            }
        }

        private static void BuildContactSatellite(MechanismBuilder b,string branch,int final)
        {
            // +X is the damaging OUTWARD edge. The swept -Y side is the trailing
            // side; OrbitalSatellitePose mirrors it for the counter-moving ring.
            b.Add(PolylineContour(true,MechanismBuilder.P(-.30f,-.12f),MechanismBuilder.P(.12f,-.26f),MechanismBuilder.P(.92f,0),MechanismBuilder.P(.10f,.14f),MechanismBuilder.P(-.30f,.12f)),Vector2.zero,new Vector2(-.035f,0),.08f,fill:true,width:.07f);
            b.Node(-.20f,0,.13f,MechanismBuilder.White,dx:-.03f);
            b.Add(PolylineContour(false,MechanismBuilder.P(-.08f,-.10f),MechanismBuilder.P(.60f,0),MechanismBuilder.P(.10f,.07f)),Vector2.zero,new Vector2(.035f,0),-.08f,color:MechanismBuilder.White,width:.06f);
            if(branch.Length==0)return;
            if(branch=="orbital_dance"){
                b.Add(PolylineContour(true,MechanismBuilder.P(-.12f,-.18f),MechanismBuilder.P(.16f,-.42f),MechanismBuilder.P(.63f,-.34f),MechanismBuilder.P(.33f,-.10f)),Vector2.zero,new Vector2(-.04f,-.035f),-.12f,fill:true,width:.06f);
                if(final<0)return;
                if(final==0){for(int i=0;i<2;i++)b.Plate(.18f,-.35f-i*.12f,.55f,.055f,dy:-.04f,angle:-.18f,color:MechanismBuilder.White);}
                else if(final==1){b.Facet(.38f,-.32f,.75f,.07f,-.28f,MechanismBuilder.White,-.045f,-.12f);b.Facet(.35f,.23f,.61f,.055f,.18f,dy:.035f,swing:.10f);}
                else{b.Facet(.46f,-.25f,.64f,.09f,-.25f,MechanismBuilder.White,-.045f);b.Plate(.10f,-.40f,.28f,.06f,-.045f,color:b.Accent);}
            }else if(branch=="orbital_halo"){
                b.Arc(.16f,0,.30f,.28f,-1.5f,1.5f,b.Accent,dx:.035f);b.Plate(.03f,-.25f,.34f,.055f,-.04f,color:MechanismBuilder.White);b.Plate(.03f,.20f,.27f,.055f,.035f,color:MechanismBuilder.White);
                if(final<0)return;
                if(final==0){b.Arc(.21f,0,.42f,.36f,-1.4f,1.4f,MechanismBuilder.White,dx:.045f);b.Node(.05f,-.34f,.065f,b.Accent,dy:-.035f);}
                else if(final==1){b.Facet(.38f,-.28f,.64f,.09f,-.22f,MechanismBuilder.White,-.045f);b.Facet(.40f,.22f,.59f,.08f,.20f,dy:.04f);}
                else{b.Arc(.20f,0,.47f,.39f,-1.2f,1.2f,MechanismBuilder.White,dx:.05f);b.Facet(.14f,-.36f,.40f,.075f,-.18f,b.Accent,-.045f);b.Facet(.18f,.30f,.36f,.06f,.18f,b.Accent,.04f);}
            }else{
                b.Facet(.40f,-.22f,.78f,.10f,-.25f,MechanismBuilder.White,-.045f,-.12f);b.Facet(.48f,.15f,.57f,.07f,.18f,dy:.04f,swing:.10f);
                if(final<0)return;
                if(final==0){b.Plate(.12f,-.32f,.45f,.085f,-.04f,color:b.Accent);b.Facet(.60f,0,.56f,.11f,color:MechanismBuilder.White);}
                else if(final==1){for(int i=0;i<2;i++)b.Facet(.31f,-.38f-i*.13f,.68f,.06f,-.25f,MechanismBuilder.White,-.04f);}
                else{b.Facet(.35f,-.42f,.73f,.075f,-.30f,MechanismBuilder.White,-.045f);b.Facet(.56f,.25f,.62f,.07f,.22f,dy:.04f);b.Plate(.09f,-.31f,.33f,.07f,-.04f,color:b.Accent);}
            }
            if(final>=0){b.Teeth(-.12f,.15f,3,.10f);}
        }
    }
}
