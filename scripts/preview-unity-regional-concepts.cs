// Preview-only native mesh concepts. Never installed in the production enemy path.
using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;
using UnityEditor;using UnityEngine;using UnityEngine.UIElements;using EchoSphere.Presentation;
public static class RegionalCreatureConceptPreview
{
 const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 static Type T(string name)=>typeof(NativeWorldPresentation).Assembly.GetType("EchoSphere.Presentation."+name,true);
 static object C(object o,string name,params object[] args){var m=o.GetType().GetMethods(F).Where(x=>x.Name==name&&x.GetParameters().Length>=args.Length).OrderBy(x=>x.GetParameters().Length).First();var p=m.GetParameters();return m.Invoke(o,p.Select((x,i)=>i<args.Length?args[i]:x.DefaultValue).ToArray());}
 sealed class Entry {public string Name,Type,Role;public bool Boss;public Entry(string n,string t,string r,bool b){Name=n;Type=t;Role=r;Boss=b;}}
 static readonly Entry[][] Catalog={new Entry[]{new Entry("Keystone Monolith","anchor","anchor",false),new Entry("Strained Geode","bomber","bomber",false),new Entry("Clouded Crystal","corruptor","corruptor",false),new Entry("Fault Tuning Fork","disruptor","disruptor",false),new Entry("Chipling","grunt","grunt",false),new Entry("Facet Mender","healer","healer",false),new Entry("Hollow Facet","phase","phase",false),new Entry("Facet Shooter","ranged","ranged",false),new Entry("Needle Lens","sniper","sniper",false),new Entry("Cluster Guard","tank_guard","tank_guard",false),new Entry("Linkbreaker","linkbreaker","disruptor",false),new Entry("Rhythm Eater","resonance_leech","leech",false),new Entry("Phantom Hunter","phantom_hunter","phase",false),new Entry("Pattern Shifter","geometry_shifter","disruptor",false),new Entry("Elder Brood","splitter_prime","splitter",false),new Entry("Mirror Warden","mirror_warden","tank_guard",false),new Entry("Stasis Warden","stasis_warden","anchor",false),new Entry("Nullifier","nullifier","disruptor",false),new Entry("Ash Carrier","pyroclast","bomber",false),new Entry("Elder Gatherer","scavenger_prime","scavenger",false),new Entry("Void Lancer","shooter","grunt",true),new Entry("Breacher","charger","grunt",true),new Entry("Brood Mind","summoner","grunt",true),new Entry("Aura Titan","aura","grunt",true),new Entry("Stranger Conductor","conductor","grunt",true),new Entry("Blind Architect","architect","grunt",true),new Entry("The Null","null","grunt",true),new Entry("Stella Warden","stella_warden","grunt",true)},new Entry[]{new Entry("Thunder Blister","bomber","bomber",false),new Entry("Thunder Wedge","charger","charger",false),new Entry("Fouled Discharge","corruptor","corruptor",false),new Entry("Beat Breaker","disruptor","disruptor",false),new Entry("Wandering Pulse","grunt","grunt",false),new Entry("Current Mender","healer","healer",false),new Entry("Return Current","leech","leech",false),new Entry("Interval","phase","phase",false),new Entry("Arc Shooter","ranged","ranged",false),new Entry("Straight Lightning","sniper","sniper",false),new Entry("Spark Swarm","swarmer","swarmer",false),new Entry("Linkbreaker","linkbreaker","disruptor",false),new Entry("Rhythm Eater","resonance_leech","leech",false),new Entry("Phantom Hunter","phantom_hunter","phase",false),new Entry("Pattern Shifter","geometry_shifter","disruptor",false),new Entry("Elder Brood","splitter_prime","splitter",false),new Entry("Mirror Warden","mirror_warden","tank_guard",false),new Entry("Stasis Warden","stasis_warden","anchor",false),new Entry("Nullifier","nullifier","disruptor",false),new Entry("Ash Carrier","pyroclast","bomber",false),new Entry("Elder Gatherer","scavenger_prime","scavenger",false),new Entry("Void Lancer","shooter","grunt",true),new Entry("Breacher","charger","grunt",true),new Entry("Brood Mind","summoner","grunt",true),new Entry("Aura Titan","aura","grunt",true),new Entry("Stranger Conductor","conductor","grunt",true),new Entry("Blind Architect","architect","grunt",true),new Entry("The Null","null","grunt",true),new Entry("Stella Warden","stella_warden","grunt",true)},new Entry[]{new Entry("Fixed Address","anchor","anchor",false),new Entry("Emergency Dump","bomber","bomber",false),new Entry("Damaged Cast","corruptor","corruptor",false),new Entry("Redactor","disruptor","disruptor",false),new Entry("Body Draft","grunt","grunt",false),new Entry("Restorer","healer","healer",false),new Entry("Name Collector","leech","leech",false),new Entry("Omission","phase","phase",false),new Entry("Lost Fragment Gatherer","scavenger","scavenger",false),new Entry("Last Witness","sniper","sniper",false),new Entry("Backup Copy","splitter","splitter",false),new Entry("Runaway Copy","swarmer","swarmer",false),new Entry("Shell Keeper","tank_guard","tank_guard",false),new Entry("Linkbreaker","linkbreaker","disruptor",false),new Entry("Rhythm Eater","resonance_leech","leech",false),new Entry("Phantom Hunter","phantom_hunter","phase",false),new Entry("Pattern Shifter","geometry_shifter","disruptor",false),new Entry("Elder Brood","splitter_prime","splitter",false),new Entry("Mirror Warden","mirror_warden","tank_guard",false),new Entry("Stasis Warden","stasis_warden","anchor",false),new Entry("Nullifier","nullifier","disruptor",false),new Entry("Ash Carrier","pyroclast","bomber",false),new Entry("Elder Gatherer","scavenger_prime","scavenger",false),new Entry("Void Lancer","shooter","grunt",true),new Entry("Breacher","charger","grunt",true),new Entry("Brood Mind","summoner","grunt",true),new Entry("Aura Titan","aura","grunt",true),new Entry("Stranger Conductor","conductor","grunt",true),new Entry("Blind Architect","architect","grunt",true),new Entry("The Null","null","grunt",true),new Entry("Stella Warden","stella_warden","grunt",true)}};
 static int Region;
 static Entry[] Entries=>Catalog[Region-3];
 static string[] Names=>Entries.Select(x=>x.Name).ToArray();
 public static object Region4()=>Run(3);
 public static object Region5()=>Run(4);
 public static object Region6()=>Run(5);
 public static object Run(int region){Region=region;return Main();}

 sealed class Art:IDisposable
 {
  public Mesh Body,Head,Plate,Leg,Foot,Eye,Wing,Claw,Sac,Shard,Horn;
  public float Width,Length;public int Kind;public bool Winged,Segmented,Boss;
  static Mesh Poly(params Vector2[] pts){var m=new Mesh();var vertices=new Vector3[pts.Length];for(int i=0;i<pts.Length;i++)vertices[i]=pts[i];var tris=new int[(pts.Length-2)*3];for(int i=1;i<pts.Length-1;i++){tris[(i-1)*3]=0;tris[(i-1)*3+1]=i;tris[(i-1)*3+2]=i+1;}m.vertices=vertices;m.triangles=tris;m.RecalculateBounds();return m;}
  static Mesh Oval(float x,float y){return Poly(Enumerable.Range(0,24).Select(i=>new Vector2(Mathf.Cos(i*Mathf.PI/12)*x,Mathf.Sin(i*Mathf.PI/12)*y)).ToArray());}
  static Mesh CurvedHorn(){
   const int n=18;var vertices=new Vector3[(n+1)*2];var triangles=new int[n*6];
   for(int i=0;i<=n;i++){float t=i/(float)n;float x=-.55f+1.22f*t,y=-.30f*Mathf.Sin(t*Mathf.PI);var tangent=new Vector2(1.22f,-.30f*Mathf.PI*Mathf.Cos(t*Mathf.PI)).normalized;var normal=new Vector2(-tangent.y,tangent.x);float width=.13f*(1-t)+.003f;vertices[i*2]=new Vector2(x,y)+normal*width;vertices[i*2+1]=new Vector2(x,y)-normal*width;if(i<n){int k=i*6,v=i*2;triangles[k]=v;triangles[k+1]=v+1;triangles[k+2]=v+2;triangles[k+3]=v+2;triangles[k+4]=v+1;triangles[k+5]=v+3;}}
   var mesh=new Mesh();mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateBounds();return mesh;
  }
  public Art(int id)
  {
   Kind=id;Boss=Entries[id].Boss;int b=id%13;
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
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");string dir="Library/regional-concepts-"+(Region+1);Directory.CreateDirectory(dir);
  var owner=new GameObject("Isolated Bastion game-sized concept preview");var surface=owner.AddComponent(T("NativeWorldSurface"));C(surface,"Initialize");C(surface,"SetViewport",450f,1000f,1.5f);
  var depth=Activator.CreateInstance(T("NativeWorldDepthBackdrop"),true);var player=Activator.CreateInstance(T("NativeWorldPlayer"),true);var creatures=Activator.CreateInstance(T("NativeWorldCreatures"),true);var feedback=Activator.CreateInstance(T("NativeWorldFeedback"),true);var minions=Activator.CreateInstance(T("NativeWorldMinions"),true);
  var standard=Activator.CreateInstance(T("NativeWorldStandardSphere"),true);
  var art=Enumerable.Range(0,Entries.Length).Select(i=>new Art(i)).ToArray();var mat=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid"));var props=new List<MaterialPropertyBlock>();int used=0;
  var draw=surface.GetType().GetMethod("DrawNativeMesh",F);
  Matrix4x4 M(float x,float y,float angle=0,float sx=1,float sy=1)=>Matrix4x4.TRS(new Vector3(x,y,0),Quaternion.Euler(0,0,angle),new Vector3(sx,sy,1));
  void Paint(Mesh mesh,Matrix4x4 matrix,Color color){if(used==props.Count)props.Add(new MaterialPropertyBlock());var p=props[used++];p.SetColor("_NativeColor",color);draw.Invoke(surface,new object[]{mesh,mat,p,(Matrix4x4?)matrix,null,0f,false,null,false,false,false,false});}
  Color Col(float r,float g,float b)=>new Color(r,g,b,1);var edge=Col(.40f,.49f,.29f);var shell=Col(.24f,.30f,.15f);var dark=Col(.065f,.09f,.055f);var tip=Col(.64f,.61f,.40f);
  void Creature(int id,float x,float y,double time)
  {
   var a=art[id];var e=Entries[id];float r=e.Boss?32:16.5f;
   float aim=Mathf.Atan2(510-y,225-x)*Mathf.Rad2Deg;
   var root=M(x,y,aim,r,r);float t=(float)time;float phase=t*2.2f+id*.7f;
   float shot=Mathf.Pow(Mathf.Max(0,Mathf.Sin(phase)),8);
   string role=e.Boss?e.Type:e.Role;
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
     Paint(a.Head,seg*M(-.6f,0),baseC);
     for(int side=-1;side<=1;side+=2){
      Paint(a.Shard,seg*M(0,side*.24f,side*15,1.4f,side*1.5f),mid);
      Paint(a.Shard,seg*M(.03f,side*.17f,side*15,1.25f,side*.55f),light);
      Paint(a.Shard,seg*M(.05f,side*.12f,side*15,.9f,side*.16f),bright);
     }
    }
    int fins=heavy?4:fork?3:2;
    for(int side=-1;side<=1;side+=2)for(int j=0;j<fins;j++){
     float px=-.65f+j*.48f,ang=side*(65+j*17+Mathf.Sin(t*3+j)*5+shot*12);
     var fin=root*M(px,side*(heavy?.52f:.37f),ang,heavy?.95f:.78f,side);
     Paint(a.Shard,fin,baseC);Paint(a.Shard,fin*M(.05f,0,0,.90f,.70f),mid);Paint(a.Shard,fin*M(.08f,-.02f,0,.78f,.23f),light);
     Paint(a.Shard,fin*M(-.30f,-.25f,-12,.45f,.3f),bright);
    }
    var head=root*M(.68f,0,0,ranged?1.1f:.72f,.68f);
    Paint(a.Head,head*M(-.4f,0),baseC);Paint(a.Shard,head,mid);Paint(a.Shard,head*M(.02f,0,0,.86f,.5f),light);
    for(int side=-1;side<=1;side+=2){
     Paint(a.Eye,root*M(.65f,side*.16f,0,1.0f,.7f),bright);
     if(ranged||fork){var rail=root*M(.70f-shot*.09f,side*(.24f+shot*.13f),side*(shot*8),1.1f,side*.7f);Paint(a.Shard,rail,mid);Paint(a.Shard,rail*M(0,0,0,.8f,.25f),bright);}
    }
    if(sacs)for(int side=-1;side<=1;side+=2)for(int j=0;j<3;j++){
     var cluster=root*M(-.6f+j*.42f,side*.4f,side*(30+j*15+shot*10),.65f,side*.85f);Paint(a.Head,cluster,mid);Paint(a.Shard,cluster*M(.2f,0,0,.85f,.7f),light);
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
      Paint(a.Horn,branch*M(0,0,0,scale,side*scale*.65f),bronze);
      Paint(a.Horn,branch*M(.015f,-side*.025f,0,scale*.91f,side*scale*.28f),gold);
      if(j<3)Paint(a.Horn,branch*M(.02f,-side*.035f,0,scale*.82f,side*scale*.08f),hot);
     }
    }
    var body=root*M(.05f,0,0,heavy?1.1f:.85f,heavy?.9f:.65f);
    Paint(a.Head,body*M(-.55f,0),darkC);Paint(a.Plate,body,bronze);
    for(int side=-1;side<=1;side+=2){
     var cheek=body*M(.25f,side*.13f,side*(18+shot*18),.8f,side*.62f);
     Paint(a.Horn,cheek,bronze);Paint(a.Horn,cheek*M(.02f,0,0,.87f,.43f),gold);
     Paint(a.Eye,body*M(.66f,side*.14f,0,1,.7f),hot);
     Paint(a.Claw,root*M(.50f,side*.27f,side*(25+shot*25),ranged?1.25f:.8f,side*.5f),gold);
    }
    Paint(a.Sac,root*M(-.18f,0,0,.55f,.55f),bronze);Paint(a.Sac,root*M(-.18f,0,0,.32f,.32f),gold);Paint(a.Eye,root*M(-.18f,0,0,1.9f,2.6f),hot);
    if(sacs||heavy)for(int j=0;j<4;j++)for(int side=-1;side<=1;side+=2){var vent=root*M(-.7f+j*.33f,side*.32f,side*(55+shot*15),.55f,side*.38f);Paint(a.Plate,vent,darkC);Paint(a.Horn,vent,gold);}
    if(ranged||fork)for(int j=0;j<3;j++)Paint(a.Shard,root*M(.9f+j*.21f+shot*.4f,Mathf.Sin(t*13+j)*.025f,0,.25f,.15f),hot);
   }else{
    var darkC=Col(.10f,.055f,.15f);var mid=Col(.26f,.16f,.35f);var pale=Col(.50f,.35f,.59f);var vein=Col(.70f,.44f,.83f);
    // Hollow interlocking armour, long jointed limbs and a split predatory head.
    int limbs=heavy?4:serpent?2:3;
    for(int side=-1;side<=1;side+=2)for(int j=0;j<limbs;j++){
     var joint=root*M(-.65f+j*.48f,side*.27f,side*(55+j*22+Mathf.Sin(t*4+j)*12));
     Paint(a.Horn,joint*M(0,0,0,1.1f,side*.85f),mid);
     var elbow=joint*M(.52f,-side*.14f,side*(52+Mathf.Sin(t*4+j+1)*12));
     Paint(a.Sac,elbow*M(0,0,0,.20f,.20f),darkC);Paint(a.Horn,elbow*M(0,0,0,.95f,side*.65f),pale);
     Paint(a.Claw,elbow*M(.50f,-side*.12f,side*40,.45f,side*.38f),mid);
    }
    int segs=serpent?7:heavy?5:4;
    for(int j=0;j<segs;j++){
     float flex=Mathf.Sin(t*3-j*.65f)*.07f;var plate=root*M(-1.0f+j*.35f,flex,0,.60f,(heavy?.9f:.62f)*(1-j*.035f));
     Paint(a.Plate,plate*M(-.18f,0,0,.85f,.8f),mid);Paint(a.Shard,plate*M(.10f,0,0,.34f,.22f),darkC);
     for(int side=-1;side<=1;side+=2){var shellP=plate*M(.10f,side*.15f,side*(22+shot*7),.75f,side*.80f);Paint(a.Horn,shellP,mid);Paint(a.Horn,shellP*M(.03f,0,0,.85f,.50f),pale);Paint(a.Horn,shellP*M(.06f,-.025f,0,.73f,.13f),vein);}
    }
    for(int side=-1;side<=1;side+=2){
     var mantle=root*M(-.35f,side*.28f,side*(80+Mathf.Sin(t*2)*5),heavy?1.25f:.9f,side*.7f);
     Paint(a.Wing,mantle,mid);Paint(a.Wing,mantle*M(0,0,0,.87f,.65f),pale);
     Paint(a.Horn,mantle*M(-.25f,-.1f,180,1.2f,.35f),vein);
    }
    var head=root*M(.62f,0,0,ranged?1.05f:.8f,.65f);Paint(a.Head,head*M(-.40f,0),mid);
    for(int side=-1;side<=1;side+=2){
     Paint(a.Plate,head*M(.2f,side*.12f,side*(18+shot*15),.65f,side*.60f),mid);
     Paint(a.Horn,head*M(.15f,side*.12f,side*(20+shot*18),.85f,side*.48f),pale);
     Paint(a.Eye,root*M(.73f,side*.15f,0,.9f,.65f),vein);
     var horn=root*M(.12f,side*.33f,side*(110+Mathf.Sin(t*2)*5),heavy?1.6f:1.1f,side*.85f);Paint(a.Horn,horn,mid);Paint(a.Horn,horn*M(.025f,0,0,.87f,.4f),pale);
    }
    if(sacs)for(int side=-1;side<=1;side+=2){Paint(a.Sac,root*M(-.25f,side*.34f,0,.7f,.65f),mid);Paint(a.Horn,root*M(-.25f,side*.34f,side*90,.65f,side*.5f),vein);}
    if(fork||ranged)for(int side=-1;side<=1;side+=2)Paint(a.Claw,root*M(.6f,side*(.3f+shot*.2f),side*(20+shot*20),1.1f,side*.6f),vein);
   }
  }
  // Actual retained UI classes, not painted mock HUD labels.
  var panel=ScriptableObject.CreateInstance<PanelSettings>();panel.scaleMode=PanelScaleMode.ConstantPixelSize;panel.scale=1;panel.forceGammaRendering=true;panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");var rt=new RenderTexture(675,1500,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);rt.Create();panel.targetTexture=rt;panel.scale=1.5f;
  var doc=owner.AddComponent<UIDocument>();doc.panelSettings=panel;var rootUi=doc.rootVisualElement;rootUi.style.position=Position.Absolute;rootUi.style.left=rootUi.style.right=rootUi.style.top=rootUi.style.bottom=0;
  var field=new Image{scaleMode=ScaleMode.StretchToFill,pickingMode=PickingMode.Ignore};field.style.position=Position.Absolute;field.style.left=field.style.right=field.style.top=field.style.bottom=0;rootUi.Add(field);
  var hud=new NativeHudView(rootUi,()=>false,_=>null);hud.Bind(new NativeHudView.State{Russian=true,Level="12",Hp="140/140",XpPercent=40,HpPercent=100,ResonancePercent=45,Resonance="45/100",Spheres="СФЕРЫ 3/5",Wave="ВОЛНА 8",Links="3",Timer="06:42",HasRegion=true,Region=new[]{"ГРАВИТАЦИОННЫЕ СКЛАДКИ","РЕЗОНАНСНЫЙ ШТОРМ","НУЛЕВОЙ АРХИВ"}[Region-3],Phase="",Pocket=""});
  int page=0,frame=0,last=-1;double started=EditorApplication.timeSinceStartup;bool bg=Application.runInBackground;Application.runInBackground=true;
  void Draw(double time)
  {
   used=0;C(surface,"Begin",new Vector2(225,500));C(surface,"ResetTransform");C(depth,"Draw",surface,450f,1000f,Region,Vector2.zero,null,time);C(player,"BeginFrame");C(creatures,"BeginFrame");C(feedback,"BeginFrame",surface);
   for(int slot=0;slot<6;slot++){int id=page*6+slot;if(id>=Entries.Length)break;float x=slot%2==0?102:348,y=230+slot/2*220;Creature(id,x,y,time);C(feedback,"Text",(id+1).ToString("00")+" "+Names[id],(double)x,(double)(y+61),new Color(.8f,.85f,.72f),8f);}
   var ps=Activator.CreateInstance(T("NativeWorldPlayer").GetNestedType("State",F));void S(string n,object v)=>ps.GetType().GetField(n,F).SetValue(ps,v);S("X",225d);S("Y",510d);S("Radius",14d);S("Hp",100d);S("Time",time);S("Character","hunter");S("Color",new Color(.75f,.3f,1));C(player,"Draw",surface,ps,creatures);
   
   C(standard,"BeginFrame");C(standard,"DrawCore",surface,21d,.7d,(Matrix4x4?)M(181,555));C(standard,"DrawRing",surface,24d,time*.2,.9d,(Matrix4x4?)M(181,555));
   C(surface,"End");field.image=surface.GetType().GetProperty("UiTexture",F).GetValue(surface) as Texture;hud.Prepare(1.5f);
  }
  EditorApplication.CallbackFunction tick=null;void Done(string result){EditorApplication.update-=tick;hud.Dispose();panel.targetTexture=null;foreach(var o in new[]{depth,player,creatures,feedback,minions,standard})((IDisposable)o).Dispose();foreach(var a in art)a.Dispose();UnityEngine.Object.Destroy(mat);UnityEngine.Object.Destroy(owner);rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(panel);Application.runInBackground=bg;File.WriteAllText(dir+"/result.json",result);}
  tick=()=>{try{EditorApplication.QueuePlayerLoopUpdate();if(Time.frameCount==last)return;last=Time.frameCount;if(EditorApplication.timeSinceStartup-started>120)throw new Exception("deadline");Draw(2+frame/15d);if(!(bool)depth.GetType().GetProperty("Ready",F).GetValue(depth)||!hud.Ready)return;if(page==0 && frame<24){
    var prev=RenderTexture.active;RenderTexture.active=rt;var f=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);f.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);f.Apply();File.WriteAllBytes(dir+"/motion-"+frame.ToString("D2")+".png",f.EncodeToPNG());UnityEngine.Object.Destroy(f);RenderTexture.active=prev;
   }if(++frame<24)return;
   var old=RenderTexture.active;var shot=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);RenderTexture.active=rt;shot.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);shot.Apply();File.WriteAllBytes(dir+"/game-page-"+(page+1)+".png",shot.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.Destroy(shot);if(++page==(Entries.Length+5)/6){Done("PASS:"+Entries.Length+" preview-only creatures, region "+(Region+1));return;}frame=0;
  }catch(Exception e){Done(e.ToString());}};EditorApplication.update+=tick;return "Queued region "+(Region+1)+": "+Entries.Length+" preview-only native models";
 }
}
