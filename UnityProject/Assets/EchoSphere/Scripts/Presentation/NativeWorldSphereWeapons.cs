using System;
using System.Collections.Generic;
using UnityEngine;
using static EchoSphere.Presentation.NativeWorldShapes;

namespace EchoSphere.Presentation
{
    // One retained mechanical assembly per authored family/branch/final. All
    // motion is rigid translation/rotation; no animated scale or tessellation.
    internal sealed class NativeWorldSphereWeapons : IDisposable
    {
        internal struct State
        {
            internal string Family,Branch;
            internal int Final;
            internal double X,Y,Radius,Rotation,Time,AttackDelay,ShotTime;
            internal uint ShotSerial;
            internal bool Disabled,Resonance;
        }
        private sealed class Part : IDisposable
        {
            internal Mesh Fill,Stroke,Halo;
            internal Color Color;
            internal Vector2 Rest,Slide;
            internal float Angle,Swing,Recoil,Delay;
            internal int Channel;
            public void Dispose(){Destroy(Fill);Destroy(Stroke);Destroy(Halo);}
        }
        private sealed class Rig : IDisposable
        {
            internal readonly List<Part> Parts=new List<Part>();
            public void Dispose(){foreach(var part in Parts)part.Dispose();}
        }
        private readonly Dictionary<(string,string,int),Rig> _rigs=new Dictionary<(string,string,int),Rig>();
        private readonly List<MaterialPropertyBlock> _properties=new List<MaterialPropertyBlock>();
        private readonly Material _solid=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid")){name="Retained sphere mechanical parts",hideFlags=HideFlags.DontSave};
        private int _used;
        internal void BeginFrame(){_used=0;}
        internal int RigCount=>_rigs.Count;
        private static Color C(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        private static Matrix4x4 T(double x=0,double y=0,double angle=0)=>Matrix4x4.TRS(new Vector3((float)x,(float)y,0),Quaternion.Euler(0,0,(float)(angle*Mathf.Rad2Deg)),Vector3.one);
        private MaterialPropertyBlock Properties(Color c){if(_used==_properties.Count)_properties.Add(new MaterialPropertyBlock());var p=_properties[_used++];p.Clear();p.SetVector("_NativeColor",new Vector4(c.r,c.g,c.b,c.a));return p;}
        private static float Pulse(double age,double duration,int channel,float delay)
        {
            double p=age/duration-delay;if(p<0||p>=1)return 0;
            if(channel==1)return (float)(p<.18?Math.Sin(p/.18*Math.PI*.5):Math.Exp(-(p-.18)*7)*(1-p));
            if(channel==2)return (float)Math.Pow(Math.Sin(p*Math.PI),1.4);
            if(channel==3)return (float)(Math.Sin(p*Math.PI*2)*Math.Sin(p*Math.PI));
            return (float)Math.Sin(p*Math.PI);
        }
        private static Matrix4x4 Pose(Part part,double age,double duration)
        {
            float value=part.Channel==0?0:Pulse(age,duration,part.Channel,part.Delay);
            float recoil=part.Recoil==0?0:Pulse(age,duration,1,0)*part.Recoil;
            return T(part.Rest.x+part.Slide.x*value*2.2-recoil*1.8,part.Rest.y+part.Slide.y*value*2.2,part.Angle+part.Swing*value*2.2);
        }
        internal void Draw(NativeWorldSurface surface,State state)
        {
            if(state.Family!="standard"&&state.Family!="sniper"&&state.Family!="shotgun")throw new ArgumentException("Unsupported retained weapon family");
            string branch=state.Branch??"";int final=branch.Length==0?-1:Math.Max(-1,Math.Min(2,state.Final));var key=(state.Family,branch,final);
            if(!_rigs.TryGetValue(key,out var rig)){rig=Build(state.Family,branch,final);_rigs.Add(key,rig);}
            // Use the simulation's actual shot time. A slow render frame must not
            // discard an attack, and returning to view must not replay an old one.
            double duration=Math.Min(.48,Math.Max(.28,state.AttackDelay*.65));
            double age=state.ShotSerial==0?double.PositiveInfinity:Math.Max(0,state.Time-state.ShotTime);
            if(state.Disabled)age=double.PositiveInfinity;
            var root=T(state.X,state.Y,state.Rotation)*Matrix4x4.Scale(Vector3.one*(float)state.Radius);
            var dark=C("#030b17");float opacity=state.Disabled?.22f:1;
            foreach(var part in rig.Parts){
                var matrix=root*Pose(part,age,duration);var color=state.Disabled?C("#ff4d70"):state.Resonance?C("#ffd166"):part.Color;
                // Geometry provides the soft edge; no per-part blur resources.
                if(part.Fill!=null)surface.DrawNativeMesh(part.Fill,_solid,Properties(dark),matrix,opacity:opacity,inheritShadow:false);
                if(part.Halo!=null){var halo=color;halo.a=.14f;surface.DrawNativeMesh(part.Halo,_solid,Properties(halo),matrix,opacity:opacity,additive:true,inheritShadow:false);}
                if(part.Stroke!=null)surface.DrawNativeMesh(part.Stroke,_solid,Properties(color),matrix,opacity:opacity,additive:true,inheritShadow:false);
            }
        }
        private static Rig Build(string family,string branch,int final)
        {
            var rig=new Rig();var primary=C(family=="standard"?"#63e6ff":family=="sniper"?"#e86cff":"#ffb84d");var white=C("#e4f8ff");
            var accent=C(branch.EndsWith("resonator",StringComparison.Ordinal)||branch.EndsWith("assassin",StringComparison.Ordinal)||branch.EndsWith("burst",StringComparison.Ordinal)?"#ff9c62":branch.EndsWith("swarm",StringComparison.Ordinal)||branch.EndsWith("oracle",StringComparison.Ordinal)||branch.EndsWith("cataclysm",StringComparison.Ordinal)?"#9ce6bf":"#d9f6ff");
            Part Add(Unity.VectorGraphics.BezierContour path,bool fill,Color color,Vector2 rest=default,int channel=0,Vector2 slide=default,float swing=0,float recoil=0,float delay=0,float angle=0,float width=.033f)
            {
                // Even collars and housing are articulated: no ornamental part
                // silently remains static while the carriage fires.
                if(channel==0){channel=4;slide=new Vector2(-.065f,rest.y==0?0:Mathf.Sign(rest.y)*.035f);swing=rest.y==0?.16f:Mathf.Sign(rest.y)*.16f;}
                if(slide.sqrMagnitude<.0001f&&Mathf.Abs(recoil)<.001f){recoil=.085f;}
                recoil=Math.Max(.065f,recoil);
                var part=new Part{Color=color,Rest=rest,Channel=channel,Slide=slide,Swing=swing,Recoil=recoil,Delay=delay,Angle=angle};
                if(fill)part.Fill=Tessellate(path,0);
                part.Stroke=Tessellate(path,width,cap:Unity.VectorGraphics.PathEnding.Round,corners:Unity.VectorGraphics.PathCorner.Round);
                part.Halo=Tessellate(path,width*2.6f,cap:Unity.VectorGraphics.PathEnding.Round,corners:Unity.VectorGraphics.PathCorner.Round);
                rig.Parts.Add(part);return part;
            }
            Unity.VectorGraphics.BezierContour Poly(params Vector2[] pts)=>PolylineContour(true,pts);
            Vector2 P(float x,float y)=>new Vector2(x,y);
            Part Plate(float x,float y,float w,float h,int channel=0,float dx=0,float dy=0,float swing=0,float recoil=0,Color? color=null,float angle=0)=>Add(Poly(P(-w*.5f,-h*.5f),P(w*.5f,-h*.5f),P(w*.5f,h*.5f),P(-w*.5f,h*.5f)),true,color??primary,P(x,y),channel,P(dx,dy),swing,recoil,angle:angle);
            Part Arc(float x,float y,float rx,float ry,float start,float end,Color? color=null,int channel=0,float dx=0,float dy=0,float swing=0)=>Add(EllipseContour(Vector2.zero,rx,ry,start,end),false,color??primary,P(x,y),channel,P(dx,dy),swing);
            Part Disc(float x,float y,float radius,Color color,int channel=0,float dx=0,float dy=0,float recoil=0)=>Add(EllipseContour(Vector2.zero,radius,radius),true,color,P(x,y),channel,P(dx,dy),recoil:recoil);
            Part Fin(float x,float y,int sign,float length=.23f,int channel=2,float swing=.15f,Color? color=null,float dx=0)=>Add(Poly(P(0,0),P(length*.45f,-sign*.09f),P(length,-sign*.08f),P(length*.6f,sign*.025f)),true,color??primary,P(x,y),channel,P(dx,sign*.025f),sign*swing);
            void PairFins(float x=.99f,float y=.23f,float length=.22f,float swing=.15f,Color? color=null){Fin(x,-y,1,length,2,-swing,color);Fin(x,y,-1,length,2,-swing,color);}
            void Rail(float y,float length=.43f,float recoil=.07f,float x=1.13f)=>Plate(x,y,length,.055f,1,recoil:recoil);
            void Housing(float width=.49f,float height=.34f,float recoil=0){Add(Poly(P(-width*.50f,-height*.5f),P(width*.30f,-height*.5f),P(width*.50f,-height*.32f),P(width*.50f,height*.32f),P(width*.30f,height*.5f),P(-width*.50f,height*.5f)),true,primary,P(1.14f,0),1,recoil:recoil);}
            // Compact collar, independent side plates and moving carriage.
            Arc(.79f,0,.075f,.18f,Mathf.PI*.48f,Mathf.PI*1.52f);
            Arc(.87f,0,.06f,.145f,Mathf.PI*.48f,Mathf.PI*1.52f,white);
            if(family=="standard"){
                Housing(recoil:.11f);Rail(-.105f,.43f,.13f);Rail(.105f,.43f,.08f);PairFins(swing:.24f);
                if(branch=="standard_resonator"){
                    Plate(1.03f,0,.07f,.30f,2,dx:-.025f,color:accent);
                    if(final==0){Arc(1.35f,0,.09f,.22f,-1.3f,1.3f,accent,2,dx:.025f);Arc(1.27f,0,.07f,.20f,-1.3f,1.3f,primary,2,dx:-.025f);}
                    else if(final==1){Fin(1.13f,-.22f,1,.27f,2,-.24f,accent);Fin(1.13f,.22f,-1,.27f,2,-.24f,accent);}
                    else if(final==2){for(int i=0;i<3;i++)Plate(1.10f+i*.09f,(i%2==0?1:-1)*.21f,.075f,.05f,3,dy:.025f, color:accent);}
                }else if(branch=="standard_singularity"){
                    Arc(1.30f,0,.10f,.20f,-1.35f,1.35f,accent,2,dx:-.045f);
                    if(final==0){Fin(1.14f,-.20f,-1,.27f,2,.24f);Fin(1.14f,.20f,1,.27f,2,.24f);}
                    else if(final==1){Plate(1.30f,-.20f,.18f,.04f,2,dy:.035f,color:accent);Plate(1.30f,.20f,.18f,.04f,2,dy:-.035f,color:accent);Plate(1.39f,0,.035f,.34f,2,dx:-.025f,color:accent);}
                    else if(final==2){Disc(1.34f,0,.075f,white,2,dx:-.04f);Fin(1.13f,-.19f,-1,.30f,2,.23f);Fin(1.13f,.19f,1,.30f,2,.23f);}
                }else if(branch=="standard_swarm"){
                    int guides=final==0?1:final==2?3:2;
                    for(int i=0;i<guides;i++){float offset=(i-(guides-1)*.5f)*.14f;float angle=(i-(guides-1)*.5f)*(final==1?.28f:.18f);float length=final<0?.23f:.34f;Add(Poly(P(0,-.045f),P(length*.68f,-.07f),P(length,0),P(length*.68f,.07f),P(0,.045f)),true,accent,P(1.18f,offset),2,P(-.035f,offset*.15f),offset*.5f,angle:angle);}
                }
            }else if(family=="sniper"){
                Plate(1.15f,0,.56f,.065f,1,recoil:.10f);Disc(.98f,0,.10f,primary,4,dx:-.035f);Disc(.98f,0,.04f,white,4,dx:.02f);PairFins(.98f,.14f,.21f,.13f);
                if(branch=="sniper_oracle"){
                    Arc(.98f,0,.15f,.21f,-1.2f,1.2f,accent,4,dx:-.035f);
                    if(final==0){Add(Poly(P(-.085f,0),P(0,-.07f),P(.085f,0),P(0,.07f)),true,white,P(1.36f,0),4,P(-.045f,0));}
                    else if(final==1){Fin(1.02f,-.18f,1,.28f,3,.18f,accent);Fin(1.02f,.18f,-1,.28f,3,.18f,accent);}
                    else if(final==2){for(int i=0;i<4;i++){float a=-1.1f+i*.73f;Plate(.98f+Mathf.Cos(a)*.23f,Mathf.Sin(a)*.23f,.075f,.055f,2,dy:Mathf.Sin(a)*.025f,swing:Mathf.Sin(a)*.24f,angle:a);}}
                }else if(branch=="sniper_assassin"){
                    Add(Poly(P(-.10f,-.045f),P(.17f,0),P(-.10f,.045f)),true,accent,P(1.38f,0),1,recoil:.10f);
                    if(final==0){Plate(1.22f,-.13f,.035f,.13f,4,dy:.025f,color:accent);Plate(1.22f,.13f,.035f,.13f,4,dy:-.025f,color:accent);}
                    else if(final==1){Fin(1.06f,-.16f,-1,.34f,2,.20f,accent);Fin(1.06f,.16f,1,.34f,2,.20f,accent);}
                    else if(final==2){Add(QuadraticContour(P(0,-.10f),false,P(.23f,-.18f),P(.27f,0),P(.23f,.18f),P(0,.10f)),false,accent,P(1.16f,0),3,P(-.045f,0),.16f);}
                }else if(branch=="sniper_beacon"){
                    Plate(1.10f,-.18f,.18f,.04f,2,dy:-.025f);Plate(1.10f,.18f,.18f,.04f,2,dy:.025f);
                    if(final==0){Plate(1.27f,-.12f,.035f,.17f,2,dy:.035f,color:accent);Plate(1.27f,.12f,.035f,.17f,2,dy:-.035f,color:accent);}
                    else if(final==1){Arc(1.05f,-.17f,.28f,.14f,-2.8f,-.3f,primary,2,dy:-.025f,swing:-.16f);Arc(1.05f,.17f,.28f,.14f,.3f,2.8f,primary,2,dy:.025f,swing:.16f);}
                    else if(final==2){for(int i=0;i<3;i++){float a=i*Mathf.PI*2/3;var pos=P(1.04f+Mathf.Cos(a)*.18f,Mathf.Sin(a)*.18f);Disc(pos.x,pos.y,.035f,accent,4,dx:-.025f);Plate((pos.x+1.04f)*.5f,pos.y*.5f,.16f,.025f,4,dx:-.025f,angle:a);}}
                }
            }else{
                Housing(.52f,.40f,.065f);PairFins(.99f,.25f,.20f,.20f);
                bool heavy=branch=="shotgun_cataclysm"||branch=="shotgun_burst"&&final==1;
                if(heavy){Disc(1.25f,0,.115f,primary,1,recoil:.07f);Disc(1.25f,0,.065f,white,1,recoil:.07f);}
                else for(int i=-1;i<=1;i++)Plate(1.33f,i*.12f,.10f,.045f,1,recoil:.065f);
                if(branch=="shotgun_burst"){
                    if(final==0||final==2){Plate(1.10f,-.24f,.16f,.05f,3,dy:-.035f);Plate(1.10f,.24f,.16f,.05f,3,dy:.035f);}
                    if(final==1){Plate(1.10f,-.24f,.29f,.055f,1,recoil:.035f);Plate(1.10f,.24f,.29f,.055f,1,recoil:.035f);}
                    if(final==2){Arc(1.35f,0,.12f,.22f,.5f,2.5f,primary,2,dx:.035f);}
                }else if(branch=="shotgun_cataclysm"){
                    if(final==0){for(int i=0;i<4;i++){float a=-1.2f+i*.8f;Plate(1.28f+Mathf.Cos(a)*.13f,Mathf.Sin(a)*.23f,.075f,.055f,2,dy:Mathf.Sin(a)*.04f,swing:Mathf.Sin(a)*.23f,angle:a);}}
                    else if(final==1){Plate(1.12f,-.255f,.30f,.07f,1,recoil:.045f,color:accent);Plate(1.12f,.255f,.30f,.07f,1,recoil:.045f,color:accent);}
                    else if(final==2){Plate(1.40f,-.13f,.035f,.19f,2,dy:.035f,color:accent);Plate(1.40f,.13f,.035f,.19f,2,dy:-.035f,color:accent);}
                }else if(branch=="shotgun_hail"){
                    int count=final<0?3:final==1?8:6;
                    for(int i=0;i<count;i++){float a=i*Mathf.PI*2/count;Plate(1.26f+Mathf.Cos(a)*.14f,Mathf.Sin(a)*.20f,.065f,.035f,2,dx:Mathf.Cos(a)*.025f,dy:Mathf.Sin(a)*.035f,swing:Mathf.Sin(a)*.12f,angle:a);}
                    if(final==1){Plate(1.02f,-.27f,.15f,.04f,3,dy:-.025f);Plate(1.02f,.27f,.15f,.04f,3,dy:.025f);}
                    if(final==2){Fin(1.05f,-.24f,1,.31f,2,-.20f,accent);Fin(1.05f,.24f,-1,.31f,2,-.20f,accent);}
                }
            }
            if(final>=0){
                void Slats(float x,float y,int count,float step=.085f,float angle=0){for(int i=0;i<count;i++)Plate(x+i*step,y,.045f,.11f,3,dx:-.065f,dy:y>0?.03f:-.03f,swing:y>0?.12f:-.12f,color:i%2==0?accent:white,angle:angle);}
                void Petals(float x,float rx,float ry,int count,float begin=0,float sweep=Mathf.PI*2){for(int i=0;i<count;i++){float a=begin+(i+.5f)*sweep/count;Plate(x+Mathf.Cos(a)*rx,Mathf.Sin(a)*ry,.105f,.055f,2,dx:Mathf.Cos(a)*.06f,dy:Mathf.Sin(a)*.06f,swing:Mathf.Sin(a)*.28f,color:i%2==0?accent:white,angle:a);}}
                void Cheek(float x,float y,int sign,float length=.32f){Fin(x,y,sign,length,2,sign*.28f,accent,dx:-.035f);Plate(x+.10f,y-sign*.045f,.17f,.035f,3,dx:-.06f,dy:-sign*.03f,color:white,angle:-sign*.18f);}
                switch(branch){
                    case "standard_resonator":
                        if(final==0){ // Impulse: nested pulse forks and charged side capacitors.
                            Arc(1.43f,0,.16f,.31f,-1.35f,1.35f,white,2,dx:.065f);Slats(.99f,-.29f,3);Slats(.99f,.29f,3);
                        }else if(final==1){ // Rebound: wide counter-thrust vanes, paired pistons.
                            Cheek(.92f,-.29f,1,.39f);Cheek(.92f,.29f,-1,.39f);Plate(1.15f,-.34f,.28f,.055f,1,recoil:.13f,color:white);Plate(1.15f,.34f,.28f,.055f,1,recoil:.10f,color:white);
                        }else{ // Suppression: alternating teeth and braking shutters.
                            Slats(1.03f,-.28f,4,.10f,.25f);Slats(1.03f,.28f,4,.10f,-.25f);Arc(1.45f,0,.08f,.30f,-1.15f,1.15f,white,2,dx:-.065f);
                        }break;
                    case "standard_singularity":
                        if(final==0){ // Compression: contracting six-part iris.
                            Petals(1.30f,.22f,.30f,6);Arc(1.30f,0,.17f,.24f,-1.3f,1.3f,white,2,dx:-.07f);
                        }else if(final==1){ // Stasis: squared restraining cage.
                            for(int sign=-1;sign<=1;sign+=2){Plate(1.20f,sign*.32f,.42f,.045f,2,dy:-sign*.06f,color:accent);Slats(1.04f,sign*.25f,3);Plate(1.48f,sign*.18f,.055f,.24f,2,dy:-sign*.05f,color:white);}
                        }else{ // Collapse: three inward claws around the focus.
                            Petals(1.31f,.24f,.32f,3,-Mathf.PI/3);Arc(1.31f,0,.23f,.31f,-1.2f,1.2f,accent,2,dx:-.085f);Disc(1.31f,0,.045f,white,4,dx:-.08f);Cheek(1.01f,-.28f,-1);Cheek(1.01f,.28f,1);
                        }break;
                    case "standard_swarm":
                        if(final==0){ // One hunter shard with angular guidance fins.
                            Add(Poly(P(-.10f,0),P(.05f,-.085f),P(.22f,0),P(.05f,.085f)),true,white,P(1.47f,0),1,recoil:.15f);Cheek(1.07f,-.29f,1,.38f);Cheek(1.07f,.29f,-1,.38f);Disc(1.01f,0,.07f,accent,4,dx:-.065f);Arc(1.05f,0,.12f,.25f,-2.6f,2.6f,accent,4,dx:-.06f);
                        }else if(final==1){ // Crossfire: two visibly divergent launch tracks.
                            for(int sign=-1;sign<=1;sign+=2){Plate(1.32f,sign*.30f,.40f,.06f,1,recoil:sign>0?.12f:.08f,color:white,angle:sign*.30f);Fin(1.07f,sign*.31f,-sign,.39f,2,-sign*.30f,accent);Disc(.99f,sign*.22f,.045f,white,3,dy:sign*.06f);}
                        }else{ // Web: three fan channels and serrated side magazines.
                            for(int i=-1;i<=1;i++)Plate(1.40f,i*.25f,.33f,.045f,1,recoil:.11f+i*.025f,color:white,angle:i*.32f);Slats(.99f,-.35f,3);Slats(.99f,.35f,3);
                        }break;
                    case "sniper_oracle":
                        if(final==0){ // Critical focus: nested optic and focusing jaws.
                            Petals(1.07f,.24f,.28f,4,Mathf.PI/4);Arc(1.07f,0,.19f,.24f,-2.6f,2.6f,white,4,dx:-.065f);Plate(1.44f,0,.24f,.035f,1,recoil:.14f,color:white);
                        }else if(final==1){ // Resonance: paired conductive diamonds and pickup coils.
                            for(int sign=-1;sign<=1;sign+=2){Add(Poly(P(-.11f,0),P(0,-.07f),P(.11f,0),P(0,.07f)),true,accent,P(1.10f,sign*.29f),3,P(-.06f,sign*.04f),sign*.2f);Slats(1.27f,sign*.23f,3,.07f);}
                        }else{ // Explosive critical: segmented radial charge crown.
                            Petals(1.07f,.28f,.33f,7,-2.4f,4.8f);Arc(1.07f,0,.22f,.28f,-1.2f,1.2f,white,2,dx:.06f);
                        }break;
                    case "sniper_assassin":
                        if(final==0){ // Execution: closing paired execution hooks.
                            Cheek(1.10f,-.28f,-1,.42f);Cheek(1.10f,.28f,1,.42f);Plate(1.47f,-.11f,.14f,.045f,1,recoil:.15f,color:white,angle:.35f);Plate(1.47f,.11f,.14f,.045f,1,recoil:.15f,color:white,angle:-.35f);
                        }else if(final==1){ // Sentence: parallel locking blades and rear feed.
                            for(int sign=-1;sign<=1;sign+=2){Add(Poly(P(-.22f,-.025f),P(.12f,-.055f),P(.29f,0),P(.12f,.055f),P(-.22f,.025f)),true,white,P(1.26f,sign*.25f),1,P(0,sign*.04f),sign*.12f,recoil:.12f);Slats(.91f,sign*.31f,3,.07f);}
                        }else{ // Blood trail: swept feather vanes around the hunter focus.
                            for(int sign=-1;sign<=1;sign+=2){Cheek(.99f,sign*.26f,-sign,.41f);Fin(1.19f,sign*.30f,-sign,.29f,3,-sign*.25f,white,dx:-.07f);}Disc(1.44f,0,.065f,accent,4,dx:-.08f);
                        }break;
                    case "sniper_beacon":
                        if(final==0){ // Anchor: hooked stabilizers and gripping front prongs.
                            for(int sign=-1;sign<=1;sign+=2){Plate(1.25f,sign*.31f,.42f,.05f,2,dy:-sign*.065f,color:white);Plate(1.46f,sign*.24f,.06f,.19f,2,dy:-sign*.06f,color:accent);Disc(1.03f,sign*.24f,.055f,accent,3,dy:sign*.045f);}
                        }else if(final==1){ // Spotlight: split reflecting dish, three radial ribs.
                            for(int sign=-1;sign<=1;sign+=2){Arc(1.06f,sign*.17f,.34f,.21f,sign<0?-2.8f:.25f,sign<0?-.25f:2.8f,white,2,dy:sign*.055f,swing:sign*.25f);Slats(.94f,sign*.32f,3,.08f);}
                        }else{ // Network: linked triangular signal nodes, not a new projectile.
                            for(int i=0;i<3;i++){float a=i*Mathf.PI*2/3;float x=1.12f+Mathf.Cos(a)*.27f,y=Mathf.Sin(a)*.33f;Arc(x,y,.075f,.075f,0,Mathf.PI*1.8f,white,4,dx:-.055f,dy:Mathf.Sin(a)*.045f);Plate((x+1.12f)*.5f,y*.5f,.23f,.035f,3,dx:-.07f,color:accent,angle:a);}Cheek(.98f,-.29f,1);Cheek(.98f,.29f,-1);
                        }break;
                    case "shotgun_burst":
                        if(final==0){ // Close-range: three sliding pumps and spread vanes.
                            for(int i=-1;i<=1;i++)Plate(1.42f,i*.23f,.22f,.085f,1,recoil:.18f+Math.Abs(i)*.025f,color:white,angle:i*.18f);Cheek(.96f,-.31f,1);Cheek(.96f,.31f,-1);
                        }else if(final==1){ // Siege: reinforced breech and braced counterweights.
                            Slats(.97f,-.30f,4,.09f,.3f);Slats(.97f,.30f,4,.09f,-.3f);Plate(1.43f,-.15f,.10f,.19f,1,recoil:.20f,color:white);Plate(1.43f,.15f,.10f,.19f,1,recoil:.20f,color:white);
                        }else{ // Impact: twin shock forks and flared side kickers.
                            Arc(1.42f,0,.15f,.34f,-1.3f,1.3f,white,2,dx:.08f);Cheek(1.01f,-.31f,-1,.40f);Cheek(1.01f,.31f,1,.40f);Slats(1.08f,-.25f,2);Slats(1.08f,.25f,2);
                        }break;
                    case "shotgun_cataclysm":
                        if(final==0){ // Shatter: six independently opening fragment pods.
                            Petals(1.28f,.25f,.34f,6);Slats(.96f,-.29f,2);Slats(.96f,.29f,2);
                        }else if(final==1){ // Heavy impact: large paired pistons with segmented armour.
                            for(int sign=-1;sign<=1;sign+=2){Plate(1.19f,sign*.32f,.41f,.085f,1,recoil:.18f,color:white);Disc(.99f,sign*.32f,.055f,accent,3,dx:-.075f);Plate(1.46f,sign*.22f,.065f,.17f,2,dy:sign*.06f,color:accent);}Arc(1.40f,0,.13f,.24f,-1.3f,1.3f,white,1,dx:-.08f);
                        }else{ // Holding blast: hooked containment cage around heavy charge.
                            for(int sign=-1;sign<=1;sign+=2){Cheek(1.03f,sign*.31f,sign,.42f);Plate(1.48f,sign*.22f,.055f,.23f,2,dy:-sign*.07f,color:white);Slats(1.14f,sign*.29f,2,.09f);}Disc(1.27f,0,.045f,white,4,dx:-.08f);
                        }break;
                    case "shotgun_hail":
                        if(final==0){ // Fragment fan: staggered shutters around six exits.
                            for(int i=-1;i<=1;i++){Plate(1.47f,i*.21f,.16f,.05f,1,recoil:.14f+i*.03f,color:white,angle:i*.25f);Plate(1.08f+i*.10f,-.31f,.07f,.10f,3,dx:-.07f,color:accent);Plate(1.08f+i*.10f,.31f,.07f,.10f,3,dx:-.07f,color:accent);}
                        }else if(final==1){ // Dense hail: twin stacked magazines and alternating feed.
                            Slats(.95f,-.33f,5,.10f);Slats(.95f,.33f,5,.10f);Plate(1.49f,-.14f,.13f,.09f,1,recoil:.18f,color:white);Plate(1.49f,.14f,.13f,.09f,1,recoil:.13f,color:white);
                        }else{ // Hunter: three steering tracks and swept guidance wings.
                            for(int i=-1;i<=1;i++){Plate(1.44f,i*.25f,.31f,.045f,2,dx:-.07f,dy:i*.04f,swing:i*.18f,color:white,angle:i*.35f);}Cheek(.99f,-.32f,1,.42f);Cheek(.99f,.32f,-1,.42f);
                        }break;
                }
            }
            return rig;
        }
        private static void Destroy(UnityEngine.Object value){if(value!=null)UnityEngine.Object.Destroy(value);}
        public void Dispose(){foreach(var rig in _rigs.Values)rig.Dispose();_rigs.Clear();Destroy(_solid);}
    }
}
