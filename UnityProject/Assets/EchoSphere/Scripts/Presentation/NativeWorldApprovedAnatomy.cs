using System;using System.Collections.Generic;using System.Linq;using UnityEngine;using EchoSphere.Core;
namespace EchoSphere.Presentation {
 internal sealed partial class NativeWorldCreatures {
  private readonly Dictionary<(int,int),ConceptParts> _approvedParts=new Dictionary<(int,int),ConceptParts>();
  private static readonly BestiaryEntry[][] ApprovedEntries=Enumerable.Range(0,6).Select(r=>BestiaryCatalog.All.Where(e=>(int)e.Region==r).ToArray()).ToArray();
  private int ApprovedIndex(EnemyView e){
   var facing=_facing.GetOrCreateValue(e.Identity);string type=e.BestiaryType??(e.Boss?e.Variant:e.Role);
   if(facing.AnatomyRegion==e.RegionStyle&&facing.AnatomyType==type&&facing.AnatomyBoss==e.Boss&&facing.AnatomyElite==e.Elite)return facing.AnatomyIndex;
   var entries=ApprovedEntries[e.RegionStyle];int index=-1;
   for(int i=0;i<entries.Length;i++)if(entries[i].Boss==e.Boss&&entries[i].Elite==e.Elite&&entries[i].Type==type){index=i;break;}
   if(index<0)for(int i=0;i<entries.Length;i++)if(entries[i].Boss==e.Boss&&entries[i].Elite==e.Elite&&(entries[i].Role==e.Role||entries[i].Variant==e.Variant)){index=i;break;}
   facing.AnatomyRegion=e.RegionStyle;facing.AnatomyType=type;facing.AnatomyBoss=e.Boss;facing.AnatomyElite=e.Elite;facing.AnatomyIndex=Math.Max(0,index);return facing.AnatomyIndex;
  }
  private ConceptParts ApprovedParts(int region,int id,bool boss){var key=(region,id);if(!_approvedParts.TryGetValue(key,out var parts)){parts=new ConceptParts(id,boss,region==2);_approvedParts.Add(key,parts);}return parts;}
  private static Matrix4x4 M(float x,float y,float angle=0,float sx=1,float sy=1)=>Matrix4x4.TRS(new Vector3(x,y,0),Quaternion.Euler(0,0,angle),new Vector3(sx,sy,1));
  private static Color Col(float r,float g,float b)=>new Color(r,g,b,1);
  private void PaintConcept(Mesh mesh,Matrix4x4 matrix,Color color,EnemyView e){
   if(e.Freeze)color=Color.Lerp(color,new Color(.41f,.84f,1),.65f);
   _surface.DrawNativeMesh(mesh,_solid,Properties(color),matrix,inheritBlend:true,inheritShadow:false);
  }
  private float ApprovedAttackPose(EnemyView e,double time){
   if(!e.Boss)return (float)Clamp(e.AttackPose);
   var facing=_facing.GetOrCreateValue(e.Identity);bool preparing=e.Signal.Campaign?e.Signal.Telegraph:e.Signal.Charging||e.Signal.ShootTimer<e.Signal.ShooterWindow||e.Signal.SummonTimer<e.Signal.SummonerWindow;
   if(!e.Freeze){if(facing.WasPreparing&&!preparing)facing.AttackUntil=time+.28;facing.WasPreparing=preparing;}
   double progress=e.Signal.Campaign?1-e.Signal.Remaining/Math.Max(.01,e.Signal.Window):.65;
   return (float)Clamp(preparing?progress:Math.Max(0,(facing.AttackUntil-time)/.28));
  }
  private static bool ApprovedQuadruped(EnemyView e)=>!e.Boss&&(
   e.RegionStyle==3&&(e.Role=="corruptor"||e.Role=="tank_guard")||
   e.RegionStyle==4&&(e.Role=="healer"||e.BestiaryType=="phantom_hunter")||
   e.RegionStyle==5&&(e.Role=="grunt"||e.Role=="corruptor"));
  private void DrawRegionalCreature(EnemyView e,double time,Matrix4x4 root){
   int id=ApprovedIndex(e);bool quadruped=ApprovedQuadruped(e);int localQuad=e.RegionStyle==3?(e.Role=="tank_guard"?1:0):e.RegionStyle==4?(e.BestiaryType=="phantom_hunter"?1:0):e.Role=="corruptor"?1:0;
   ConceptParts parts;
   if(quadruped){var key=(e.RegionStyle,100+localQuad);if(!_approvedParts.TryGetValue(key,out parts)){parts=new ConceptParts(localQuad,false,false);_approvedParts.Add(key,parts);}}
   else parts=ApprovedParts(e.RegionStyle,id,e.Boss);
   float attack=ApprovedAttackPose(e,time);
   if(quadruped)DrawApprovedQuadruped(e,time,root,parts,attack);
   else if(e.RegionStyle==2)DrawApprovedBastion(e,time,root,parts,id,attack);
   else DrawApprovedRegional(e,time,root,parts,id,attack);
  }
 sealed class ConceptParts:IDisposable
 {
  public Mesh Body,Head,Plate,Leg,Foot,Eye,Wing,Claw,Sac,Shard,Horn;
  public float Width,Length;public int Kind;public bool Winged,Segmented,Boss;
  static Mesh Poly(params Vector2[] pts){var m=new Mesh {name="Approved enemy anatomy part",hideFlags=HideFlags.DontSave};var vertices=new Vector3[pts.Length];for(int i=0;i<pts.Length;i++)vertices[i]=pts[i];var tris=new int[(pts.Length-2)*3];for(int i=1;i<pts.Length-1;i++){tris[(i-1)*3]=0;tris[(i-1)*3+1]=i;tris[(i-1)*3+2]=i+1;}m.vertices=vertices;m.triangles=tris;m.RecalculateBounds();return m;}
  static Mesh Oval(float x,float y){return Poly(Enumerable.Range(0,24).Select(i=>new Vector2(Mathf.Cos(i*Mathf.PI/12)*x,Mathf.Sin(i*Mathf.PI/12)*y)).ToArray());}
  static Mesh CurvedHorn(){
   const int n=18;var vertices=new Vector3[(n+1)*2];var triangles=new int[n*6];
   for(int i=0;i<=n;i++){float t=i/(float)n;float x=-.55f+1.22f*t,y=-.30f*Mathf.Sin(t*Mathf.PI);var tangent=new Vector2(1.22f,-.30f*Mathf.PI*Mathf.Cos(t*Mathf.PI)).normalized;var normal=new Vector2(-tangent.y,tangent.x);float width=.13f*(1-t)+.003f;vertices[i*2]=new Vector2(x,y)+normal*width;vertices[i*2+1]=new Vector2(x,y)-normal*width;if(i<n){int k=i*6,v=i*2;triangles[k]=v;triangles[k+1]=v+1;triangles[k+2]=v+2;triangles[k+3]=v+2;triangles[k+4]=v+1;triangles[k+5]=v+3;}}
   var mesh=new Mesh {name="Approved curved anatomy part",hideFlags=HideFlags.DontSave};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateBounds();return mesh;
  }
  public ConceptParts(int id,bool boss,bool bastion)
  {
   Kind=id;Boss=boss;int b=bastion&&boss?new[]{9,2,12,6,0,7,12,10}[Math.Min(7,Math.Max(0,id-23))]:id%13;
   Width=new[]{.85f,.62f,.8f,.65f,.77f,.48f,.5f,.43f,.39f,.62f,.82f,.37f,.82f}[b];Length=b==7?1.6f:b==11?.8f:1;
   Winged=b==1||b==6||b==10||id==15||id==21||id==26;Segmented=b==7||b==10||id==17||id==30;
   Body=Oval(Length,Width);Head=Poly(new Vector2(.43f,-.4f),new Vector2(.88f,-.34f),new Vector2(1.08f,0),new Vector2(.88f,.34f),new Vector2(.43f,.4f));
   Plate=Poly(new Vector2(-.84f,0),new Vector2(-.63f,-.56f),new Vector2(-.18f,-.7f),new Vector2(.45f,-.46f),new Vector2(.73f,0),new Vector2(.3f,.22f));
   Leg=Poly(new Vector2(0,-.05f),new Vector2(.48f,-.1f),new Vector2(.72f,.04f),new Vector2(.48f,.09f),new Vector2(0,.04f));
   Foot=Poly(new Vector2(0,-.045f),new Vector2(.5f,-.1f),new Vector2(.74f,0),new Vector2(.5f,.035f),new Vector2(0,.035f));
   Eye=Oval(.07f,.045f);Wing=Poly(new Vector2(.2f,0),new Vector2(-.45f,-.71f),new Vector2(-1.38f,-.5f),new Vector2(-1.65f,-.12f),new Vector2(-.7f,.06f));
   Shard=Poly(new Vector2(-.75f,0),new Vector2(-.28f,-.23f),new Vector2(.66f,0),new Vector2(-.28f,.23f));
   Horn=CurvedHorn();
   Claw=Poly(new Vector2(0,-.09f),new Vector2(.49f,-.22f),new Vector2(.72f,0),new Vector2(.4f,-.03f),new Vector2(.25f,.09f));Sac=Oval(.35f,.3f);
  }
  public void Dispose(){foreach(var m in new[]{Body,Head,Plate,Leg,Foot,Eye,Wing,Claw,Sac,Shard,Horn})UnityEngine.Object.Destroy(m);}
 }
 private void DrawApprovedBastion(EnemyView e,double time,Matrix4x4 root,ConceptParts a,int id,float attack){
   void DrawPart(Mesh mesh,Matrix4x4 matrix,Color color)=>PaintConcept(mesh,matrix,color,e);   float r=(float)e.Radius;root=root*M(0,0,0,r,r);
   var edge=Col(.40f,.49f,.29f);var shell=Col(.24f,.30f,.15f);var dark=Col(.065f,.09f,.055f);var tip=Col(.64f,.61f,.40f);
   var pale=Col(.61f,.64f,.40f);var seam=Col(.12f,.17f,.10f);var glint=Col(.52f,.59f,.35f);
   float cycle=(float)(_gaitTravel*2.5+id*.7);float bite=attack;
   // Each limb has a coxa, upper shell, elbow and independently articulated tarsus.
   for(int side=-1;side<=1;side+=2)for(int k=0;k<3;k++){
    float stride=Mathf.Sin(cycle+k*2.1f+side*1.5f)*13*(float)_gaitBlend;
    var joint=root*M((k-1)*.43f,side*a.Width*.68f,side*(45+k*20)+stride);
    DrawPart(a.Sac,joint*M(0,0,0,.32f,.27f),dark);
    DrawPart(a.Leg,joint,edge);DrawPart(a.Leg,joint*M(.035f,-.018f,0,.82f,.36f),glint);
    var elbow=joint*M(.61f,.025f,side*52-stride);
    DrawPart(a.Sac,elbow*M(0,0,0,.24f,.22f),seam);
    DrawPart(a.Foot,elbow,tip);DrawPart(a.Foot,elbow*M(.03f,-.015f,0,.78f,.34f),edge);
    DrawPart(a.Claw,elbow*M(.64f,0,side*12,.24f,.22f),pale);
   }
   if(a.Winged)for(int side=-1;side<=1;side+=2){
    var wing=root*M(-.08f,side*.12f,side*(12+Mathf.Sin(cycle*1.4f)*8),1,side);
    DrawPart(a.Wing,wing,Col(.24f,.29f,.19f));
    DrawPart(a.Wing,wing*M(-.03f,-.015f,0,.90f,.74f),Col(.32f,.38f,.23f));
    for(int vein=0;vein<3;vein++)DrawPart(a.Leg,wing*M(-.15f,-.10f-vein*.13f,165+vein*8,1.3f,.23f),edge);
   }
   DrawPart(a.Body,root*M(-.18f,0,0,a.Length,a.Width),dark);
   // Overlapping chitin scutes, inset seams and a narrow ridge, never a solid oval body.
   int plates=a.Segmented?7:5;
   for(int j=0;j<plates;j++){
    float px=-1.10f+j*(a.Segmented?.34f:.31f);
    float taper=1-Mathf.Abs(j-(plates-1)*.5f)*.10f;
    float flex=a.Segmented?Mathf.Sin(cycle-j*.55f)*.045f:0;
    for(int side=-1;side<=1;side+=2){
     var plate=root*M(px,flex,side*(7+bite*3),.39f,a.Width*side*taper);
     DrawPart(a.Plate,plate,edge);DrawPart(a.Plate,plate*M(.03f,.015f,0,.83f,.79f),shell);
     DrawPart(a.Leg,plate*M(-.5f,-.40f,12,.8f,.18f),glint);
     if((id+j)%3==0)DrawPart(a.Claw,plate*M(-.35f,-.56f,-95,.33f,.24f),tip);
    }
    DrawPart(a.Sac,root*M(px-.13f,flex,0,.24f,.15f),seam);
   }
   // Armoured brow, inset sensory organs and opposing mandibles with visible bite travel.
   var head=root*M(.25f+bite*.07f,0,0,id==2||id==24?1.13f:.85f,.85f);
   DrawPart(a.Head,head,edge);DrawPart(a.Head,head*M(.035f,0,0,.88f,.75f),shell);
   DrawPart(a.Plate,head*M(.75f,0,90,.33f,.28f),glint);
   for(int side=-1;side<=1;side+=2){
    DrawPart(a.Sac,root*M(.90f,side*.22f,0,.32f,.24f),seam);
    DrawPart(a.Eye,root*M(.93f,side*.22f,0,1.1f,.8f),Col(.86f,.62f,.22f));
    var jaw=root*M(.99f,side*.24f,side*(19+bite*24),id==15||id==23?1.12f:.66f,side*.76f);
    DrawPart(a.Claw,jaw,tip);DrawPart(a.Claw,jaw*M(.02f,-.018f,0,.75f,.48f),edge);
    DrawPart(a.Claw,jaw*M(.31f,-.1f,-65,.23f,.25f),pale);
    var antenna=root*M(.86f,side*.31f,side*(40+Mathf.Sin(cycle*.6f)*7),.85f,side*.37f);
    DrawPart(a.Foot,antenna,edge);DrawPart(a.Eye,antenna*M(.68f,0,0,.45f,.45f),pale);
   }
   if(id==1||id==9||id==21||id==30)for(int side=-1;side<=1;side+=2){
    var sac=root*M(-.40f,side*.40f,side*(8+bite*12),1,.85f);
    DrawPart(a.Sac,sac,seam);DrawPart(a.Sac,sac*M(0,0,0,.83f,.80f),Col(.48f,.36f,.15f));
    for(int n=0;n<3;n++)DrawPart(a.Eye,sac*M(-.15f+n*.14f,0,0,.65f,2.4f),Col(.71f,.55f,.22f));
    DrawPart(a.Claw,sac*M(.25f,0,side*(12+bite*25),.5f,.4f),edge);
   }
   if(id==3||id==16||id==20)for(int side=-1;side<=1;side+=2)for(int j=0;j<3;j++){
    var spine=root*M(-.6f+j*.4f,side*a.Width,side*(95+Mathf.Sin(cycle+j)*4),.72f,side*.75f);
    DrawPart(a.Claw,spine,tip);DrawPart(a.Claw,spine*M(.03f,0,0,.7f,.44f),shell);
   }
   if(id==0||id==12||id==18||id==27||id==29)for(int j=0;j<4;j++){
    var shield=root*M(-.85f+j*.35f,0,0,.48f,a.Width*(1.65f-j*.13f));
    DrawPart(a.Head,shield,edge);DrawPart(a.Head,shield*M(.04f,0,0,.77f,.84f),shell);
   }
 }
 private void DrawApprovedRegional(EnemyView e,double time,Matrix4x4 root,ConceptParts a,int id,float attack){
   void DrawPart(Mesh mesh,Matrix4x4 matrix,Color color)=>PaintConcept(mesh,matrix,color,e);   int Region=e.RegionStyle;string role=e.Boss?e.Variant:e.Role;
   float r=(float)e.Radius;root=root*M(0,0,0,r,r);float t=(float)time;float shot=attack;
   bool heavy=role=="anchor"||role=="tank_guard"||role=="aura"||role=="architect";
   bool ranged=role=="ranged"||role=="sniper"||role=="shooter";
   bool fork=role=="disruptor"||role=="conductor"||role=="healer";
   bool serpent=role=="leech"||role=="phase"||role=="null"||role=="swarmer";
   bool sacs=role=="bomber"||role=="splitter"||role=="summoner"||role=="corruptor";
   int mode=heavy?0:ranged?1:fork?2:serpent?3:sacs?4:5;
   // The same retained components are transformed; no per-frame tessellation.
   if(Region==3){
    var baseC=Col(.055f,.19f,.23f);var mid=Col(.12f,.43f,.48f);var light=Col(.37f,.69f,.68f);var bright=Col(.65f,.91f,.83f);
    int segments=serpent?7:heavy?5:3;
    for(int j=0;j<segments;j++){
     float px=serpent?-.95f+j*.35f:-.45f+j*.32f;
     float sway=serpent?Mathf.Sin(t*4-j*.7f)*.12f:0;
     float width=heavy?1.0f:serpent?.36f:.60f;
     var seg=root*M(px,sway,serpent?Mathf.Sin(t*4-j*.7f)*9:0,.60f,width);
     DrawPart(a.Head,seg*M(-.6f,0),baseC);
     for(int side=-1;side<=1;side+=2){
      DrawPart(a.Shard,seg*M(0,side*.24f,side*15,1.4f,side*1.5f),mid);
      DrawPart(a.Shard,seg*M(.03f,side*.17f,side*15,1.25f,side*.55f),light);
      DrawPart(a.Shard,seg*M(.05f,side*.12f,side*15,.9f,side*.16f),bright);
     }
    }
    int fins=heavy?4:fork?3:2;
    for(int side=-1;side<=1;side+=2)for(int j=0;j<fins;j++){
     float px=-.65f+j*.48f,ang=side*(65+j*17+Mathf.Sin(t*3+j)*5+shot*12);
     var fin=root*M(px,side*(heavy?.52f:.37f),ang,heavy?.95f:.78f,side);
     DrawPart(a.Shard,fin,baseC);DrawPart(a.Shard,fin*M(.05f,0,0,.90f,.70f),mid);DrawPart(a.Shard,fin*M(.08f,-.02f,0,.78f,.23f),light);
     DrawPart(a.Shard,fin*M(-.30f,-.25f,-12,.45f,.3f),bright);
    }
    var head=root*M(.68f,0,0,ranged?1.1f:.72f,.68f);
    DrawPart(a.Head,head*M(-.4f,0),baseC);DrawPart(a.Shard,head,mid);DrawPart(a.Shard,head*M(.02f,0,0,.86f,.5f),light);
    for(int side=-1;side<=1;side+=2){
     DrawPart(a.Eye,root*M(.65f,side*.16f,0,1.0f,.7f),bright);
     if(ranged||fork){var rail=root*M(.70f-shot*.09f,side*(.24f+shot*.13f),side*(shot*8),1.1f,side*.7f);DrawPart(a.Shard,rail,mid);DrawPart(a.Shard,rail*M(0,0,0,.8f,.25f),bright);}
    }
    if(sacs)for(int side=-1;side<=1;side+=2)for(int j=0;j<3;j++){
     var cluster=root*M(-.6f+j*.42f,side*.4f,side*(30+j*15+shot*10),.65f,side*.85f);DrawPart(a.Head,cluster,mid);DrawPart(a.Shard,cluster*M(.2f,0,0,.85f,.7f),light);
    }
   }else if(Region==4){
    var darkC=Col(.24f,.13f,.045f);var bronze=Col(.58f,.31f,.07f);var gold=Col(.93f,.62f,.18f);var hot=Col(1,.88f,.46f);
    // Curved, branching streamers with a dark structural skin and narrow luminous fissures.
    int arms=heavy?6:fork?5:mode==4?4:3;
    for(int side=-1;side<=1;side+=2)for(int arm=0;arm<arms;arm++){
     var branch=root*M(-.15f,side*.08f,side*(18+arm*17));
     for(int j=0;j<5;j++){
      float bend=Mathf.Sin(t*4-j*.65f+arm)*8;float scale=1-j*.13f;
      branch=branch*M(-.27f,side*.035f,side*(-5+bend));
      DrawPart(a.Horn,branch*M(0,0,0,scale,side*scale*.65f),bronze);
      DrawPart(a.Horn,branch*M(.015f,-side*.025f,0,scale*.91f,side*scale*.28f),gold);
      if(j<3)DrawPart(a.Horn,branch*M(.02f,-side*.035f,0,scale*.82f,side*scale*.08f),hot);
     }
    }
    var body=root*M(.05f,0,0,heavy?1.1f:.85f,heavy?.9f:.65f);
    DrawPart(a.Head,body*M(-.55f,0),darkC);DrawPart(a.Plate,body,bronze);
    for(int side=-1;side<=1;side+=2){
     var cheek=body*M(.25f,side*.13f,side*(18+shot*18),.8f,side*.62f);
     DrawPart(a.Horn,cheek,bronze);DrawPart(a.Horn,cheek*M(.02f,0,0,.87f,.43f),gold);
     DrawPart(a.Eye,body*M(.66f,side*.14f,0,1,.7f),hot);
     DrawPart(a.Claw,root*M(.50f,side*.27f,side*(25+shot*25),ranged?1.25f:.8f,side*.5f),gold);
    }
    DrawPart(a.Sac,root*M(-.18f,0,0,.55f,.55f),bronze);DrawPart(a.Sac,root*M(-.18f,0,0,.32f,.32f),gold);DrawPart(a.Eye,root*M(-.18f,0,0,1.9f,2.6f),hot);
    if(sacs||heavy)for(int j=0;j<4;j++)for(int side=-1;side<=1;side+=2){var vent=root*M(-.7f+j*.33f,side*.32f,side*(55+shot*15),.55f,side*.38f);DrawPart(a.Plate,vent,darkC);DrawPart(a.Horn,vent,gold);}
    if(ranged||fork)for(int j=0;j<3;j++)DrawPart(a.Shard,root*M(.9f+j*.21f+shot*.4f,Mathf.Sin(t*13+j)*.025f,0,.25f,.15f),hot);
   }else{
    var darkC=Col(.10f,.055f,.15f);var mid=Col(.26f,.16f,.35f);var pale=Col(.50f,.35f,.59f);var vein=Col(.70f,.44f,.83f);
    // Hollow interlocking armour, long jointed limbs and a split predatory head.
    int limbs=heavy?4:serpent?2:3;
    for(int side=-1;side<=1;side+=2)for(int j=0;j<limbs;j++){
     var joint=root*M(-.65f+j*.48f,side*.27f,side*(55+j*22+Mathf.Sin(t*4+j)*12));
     DrawPart(a.Horn,joint*M(0,0,0,1.1f,side*.85f),mid);
     var elbow=joint*M(.52f,-side*.14f,side*(52+Mathf.Sin(t*4+j+1)*12));
     DrawPart(a.Sac,elbow*M(0,0,0,.20f,.20f),darkC);DrawPart(a.Horn,elbow*M(0,0,0,.95f,side*.65f),pale);
     DrawPart(a.Claw,elbow*M(.50f,-side*.12f,side*40,.45f,side*.38f),mid);
    }
    int segs=serpent?7:heavy?5:4;
    for(int j=0;j<segs;j++){
     float flex=Mathf.Sin(t*3-j*.65f)*.07f;var plate=root*M(-1.0f+j*.35f,flex,0,.60f,(heavy?.9f:.62f)*(1-j*.035f));
     DrawPart(a.Plate,plate*M(-.18f,0,0,.85f,.8f),mid);DrawPart(a.Shard,plate*M(.10f,0,0,.34f,.22f),darkC);
     for(int side=-1;side<=1;side+=2){var shellP=plate*M(.10f,side*.15f,side*(22+shot*7),.75f,side*.80f);DrawPart(a.Horn,shellP,mid);DrawPart(a.Horn,shellP*M(.03f,0,0,.85f,.50f),pale);DrawPart(a.Horn,shellP*M(.06f,-.025f,0,.73f,.13f),vein);}
    }
    for(int side=-1;side<=1;side+=2){
     var mantle=root*M(-.35f,side*.28f,side*(80+Mathf.Sin(t*2)*5),heavy?1.25f:.9f,side*.7f);
     DrawPart(a.Wing,mantle,mid);DrawPart(a.Wing,mantle*M(0,0,0,.87f,.65f),pale);
     DrawPart(a.Horn,mantle*M(-.25f,-.1f,180,1.2f,.35f),vein);
    }
    var head=root*M(.62f,0,0,ranged?1.05f:.8f,.65f);DrawPart(a.Head,head*M(-.40f,0),mid);
    for(int side=-1;side<=1;side+=2){
     DrawPart(a.Plate,head*M(.2f,side*.12f,side*(18+shot*15),.65f,side*.60f),mid);
     DrawPart(a.Horn,head*M(.15f,side*.12f,side*(20+shot*18),.85f,side*.48f),pale);
     DrawPart(a.Eye,root*M(.73f,side*.15f,0,.9f,.65f),vein);
     var horn=root*M(.12f,side*.33f,side*(110+Mathf.Sin(t*2)*5),heavy?1.6f:1.1f,side*.85f);DrawPart(a.Horn,horn,mid);DrawPart(a.Horn,horn*M(.025f,0,0,.87f,.4f),pale);
    }
    if(sacs)for(int side=-1;side<=1;side+=2){DrawPart(a.Sac,root*M(-.25f,side*.34f,0,.7f,.65f),mid);DrawPart(a.Horn,root*M(-.25f,side*.34f,side*90,.65f,side*.5f),vein);}
    if(fork||ranged)for(int side=-1;side<=1;side+=2)DrawPart(a.Claw,root*M(.6f,side*(.3f+shot*.2f),side*(20+shot*20),1.1f,side*.6f),vein);
   }
 }
 private void DrawApprovedQuadruped(EnemyView e,double time,Matrix4x4 root,ConceptParts a,float attack){
   void DrawPart(Mesh mesh,Matrix4x4 matrix,Color color)=>PaintConcept(mesh,matrix,color,e);   int Region=e.RegionStyle;int id=Region==3?(e.Role=="tank_guard"?1:0):Region==4?(e.BestiaryType=="phantom_hunter"?1:0):e.Role=="corruptor"?1:0;
   float t=(float)time;float gait=(float)(_gaitTravel*3.1+id);bool heavy=id==1&&Region==3;
   float r=(float)e.Radius;root=root*M(0,0,0,r,r);
   Color low=Region==3?Col(.06f,.21f,.24f):Region==4?Col(.26f,.14f,.05f):Col(.13f,.08f,.18f);
   Color mid=Region==3?Col(.14f,.43f,.46f):Region==4?Col(.61f,.35f,.11f):Col(.32f,.21f,.41f);
   Color edge=Region==3?Col(.41f,.70f,.70f):Region==4?Col(.93f,.66f,.26f):Col(.56f,.40f,.66f);
   Color accent=Region==3?Col(.70f,.94f,.85f):Region==4?Col(1,.90f,.56f):Col(.79f,.52f,.92f);
   float width=heavy?.54f:id==0?.36f:.30f;
   // Exactly four legs, with a shoulder/hip, knee/elbow, wrist/hock and three toes.
   for(int side=-1;side<=1;side+=2)for(int front=0;front<2;front++){
    float phase=gait+(front==0?0:Mathf.PI)+(side==1?Mathf.PI:0);
    float swing=Mathf.Sin(phase)*18*(float)_gaitBlend,lift=Mathf.Max(0,Mathf.Cos(phase))*(float)_gaitBlend;
    float anchor=front==1?.57f:-.63f;
    var shoulder=root*M(anchor,side*width,side*(front==1?60:120)+swing);
    DrawPart(a.Sac,shoulder*M(0,0,0,.57f,.65f),low);
    DrawPart(a.Body,shoulder*M(.28f,0,0,.44f,.18f),mid);
    DrawPart(a.Plate,shoulder*M(.10f,0,0,.50f,side*.33f),edge);
    var knee=shoulder*M(.56f,0,-side*(front==1?55:105)-swing*.5f);
    DrawPart(a.Sac,knee*M(0,0,0,.32f,.35f),low);
    DrawPart(a.Body,knee*M(.24f,0,0,.34f,.105f),mid);
    DrawPart(a.Shard,knee*M(.27f,-.025f,0,.52f,.25f),edge);
    var paw=knee*M(.48f,0,side*12-lift*side*12,.9f-lift*.15f,1);
    DrawPart(a.Sac,paw*M(.07f,0,0,.38f,.42f),mid);
    for(int toe=0;toe<3;toe++)DrawPart(a.Claw,paw*M(.13f,(toe-1)*.095f,(toe-1)*9,.22f,.20f),edge);
   }
   // A ribcage, narrow waist and pelvis establish an animal silhouette.
   DrawPart(a.Body,root*M(-.18f,0,0,1.0f,width*1.2f),low);
   DrawPart(a.Body,root*M(.35f,0,0,.62f,width*1.1f),mid);
   DrawPart(a.Body,root*M(-.72f,0,0,.43f,width),mid);
   for(int j=0;j<5;j++)for(int side=-1;side<=1;side+=2){
    var rib=root*M(-.78f+j*.29f,side*width*.15f,side*14,.45f,side*width*.91f);
    DrawPart(a.Plate,rib,edge);DrawPart(a.Plate,rib*M(.02f,0,0,.83f,.77f),mid);
    if(Region==3)DrawPart(a.Shard,rib*M(.06f,-.25f,side*18,.72f,.40f),accent);
    else DrawPart(a.Horn,rib*M(0,-.23f,0,.60f,.35f),edge);
   }
   // A distinct neck and paired jaw halves; the muzzle is not an abstract point.
   var neck=root*M(.75f,0,Mathf.Sin(gait*.5f)*3,1,1);
   DrawPart(a.Body,neck*M(.02f,0,0,.46f,.27f),mid);
   var head=neck*M(.35f+attack*.06f,0,0,.75f,1);
   DrawPart(a.Body,head*M(.05f,0,0,.48f,.24f),low);
   DrawPart(a.Plate,head*M(.03f,0,0,.60f,.35f),mid);
   DrawPart(a.Body,head*M(.39f,0,0,.28f,.15f),mid);
   for(int side=-1;side<=1;side+=2){
    DrawPart(a.Shard,head*M(.23f,side*(.14f+attack*.10f),side*(12+attack*14),.47f,side*.32f),edge);
    DrawPart(a.Eye,head*M(.22f,side*.16f,0,.85f,.6f),accent);
    var ear=head*M(-.12f,side*.19f,side*(120+Mathf.Sin(gait*.45f)*4),.60f,side*.45f);
    DrawPart(a.Horn,ear,edge);DrawPart(a.Horn,ear*M(0,0,0,.8f,.5f),mid);
   }
   // Tail bends in segments independently of the body and walking cycle.
   var tail=root*M(-.98f,0,180,1,1);
   for(int j=0;j<6;j++){
    tail=tail*M(.20f,0,Mathf.Sin(gait*.65f-j*.6f)*8);
    DrawPart(a.Body,tail*M(.06f,0,0,.23f,.12f*(1-j*.12f)),mid);
    DrawPart(a.Horn,tail*M(.03f,-.02f,0,.32f,.32f*(1-j*.1f)),edge);
   }
   if(Region==3){
    for(int side=-1;side<=1;side+=2)for(int j=0;j<(heavy?4:2);j++){
     var spine=root*M(-.6f+j*.32f,side*width*.8f,side*(75+j*8+attack*8),heavy?.95f:.6f,side*.65f);
     DrawPart(a.Shard,spine,mid);DrawPart(a.Shard,spine*M(.025f,0,0,.85f,.52f),edge);DrawPart(a.Shard,spine*M(.05f,0,0,.65f,.16f),accent);
    }
   }else if(Region==4){
    for(int side=-1;side<=1;side+=2)for(int j=0;j<4;j++){
     var flow=root*M(.40f-j*.35f,side*.20f,side*(120+j*10+Mathf.Sin(gait-j)*5),.80f,side*.6f);
     for(int k=0;k<3;k++){DrawPart(a.Horn,flow,edge);DrawPart(a.Horn,flow*M(.02f,0,0,.9f,.25f),accent);flow=flow*M(.44f,0,Mathf.Sin(gait-k)*10,.8f,.8f);}
    }
   }else{
    for(int side=-1;side<=1;side+=2){
     var horn=root*M(.37f,side*.25f,side*(130+Mathf.Sin(gait*.5f)*5),id==0?1.3f:1.0f,side*.70f);
     DrawPart(a.Horn,horn,edge);DrawPart(a.Horn,horn*M(.02f,0,0,.85f,.48f),mid);
     DrawPart(a.Horn,root*M(-.50f,side*.28f,side*110,.80f,side*.80f),edge);
    }
   }
 }
 }
}
