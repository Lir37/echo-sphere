// Preview-only native mesh concepts. Never installed in the production enemy path.
using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;
using UnityEditor;using UnityEngine;using UnityEngine.UIElements;using EchoSphere.Presentation;
public static class QuadrupedConceptPreview
{
 const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 static Type T(string name)=>typeof(NativeWorldPresentation).Assembly.GetType("EchoSphere.Presentation."+name,true);
 static object C(object o,string name,params object[] args){var m=o.GetType().GetMethods(F).Where(x=>x.Name==name&&x.GetParameters().Length>=args.Length).OrderBy(x=>x.GetParameters().Length).First();var p=m.GetParameters();return m.Invoke(o,p.Select((x,i)=>i<args.Length?args[i]:x.DefaultValue).ToArray());}
 sealed class Entry {public string Name,Type,Role;public bool Boss;public Entry(string n,string t,string r,bool b){Name=n;Type=t;Role=r;Boss=b;}}
 static readonly Entry[][] Catalog={
 new[]{new Entry("Clouded Crystal","corruptor","corruptor",false),new Entry("Cluster Guard","tank_guard","tank_guard",false)},
 new[]{new Entry("Current Mender","healer","healer",false),new Entry("Phantom Hunter","phantom_hunter","phase",false)},
 new[]{new Entry("Body Draft","grunt","grunt",false),new Entry("Damaged Cast","corruptor","corruptor",false)}};

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
  if(!Application.isPlaying)throw new Exception("Play required");string dir="Library/quadruped-concepts-"+(Region+1);Directory.CreateDirectory(dir);
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
   var a=art[id];float t=(float)time;float gait=t*6.2f+id;float attack=Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*2.2f+id)),8);
   bool heavy=id==1&&Region==3;float r=16.5f;
   var root=M(x,y,Mathf.Atan2(510-y,225-x)*Mathf.Rad2Deg,r,r);
   Color low=Region==3?Col(.06f,.21f,.24f):Region==4?Col(.26f,.14f,.05f):Col(.13f,.08f,.18f);
   Color mid=Region==3?Col(.14f,.43f,.46f):Region==4?Col(.61f,.35f,.11f):Col(.32f,.21f,.41f);
   Color edge=Region==3?Col(.41f,.70f,.70f):Region==4?Col(.93f,.66f,.26f):Col(.56f,.40f,.66f);
   Color accent=Region==3?Col(.70f,.94f,.85f):Region==4?Col(1,.90f,.56f):Col(.79f,.52f,.92f);
   float width=heavy?.54f:id==0?.36f:.30f;
   // Exactly four legs, with a shoulder/hip, knee/elbow, wrist/hock and three toes.
   for(int side=-1;side<=1;side+=2)for(int front=0;front<2;front++){
    float phase=gait+(front==0?0:Mathf.PI)+(side==1?Mathf.PI:0);
    float swing=Mathf.Sin(phase)*18,lift=Mathf.Max(0,Mathf.Cos(phase));
    float anchor=front==1?.57f:-.63f;
    var shoulder=root*M(anchor,side*width,side*(front==1?60:120)+swing);
    Paint(a.Sac,shoulder*M(0,0,0,.57f,.65f),low);
    Paint(a.Body,shoulder*M(.28f,0,0,.44f,.18f),mid);
    Paint(a.Plate,shoulder*M(.10f,0,0,.50f,side*.33f),edge);
    var knee=shoulder*M(.56f,0,-side*(front==1?55:105)-swing*.5f);
    Paint(a.Sac,knee*M(0,0,0,.32f,.35f),low);
    Paint(a.Body,knee*M(.24f,0,0,.34f,.105f),mid);
    Paint(a.Shard,knee*M(.27f,-.025f,0,.52f,.25f),edge);
    var paw=knee*M(.48f,0,side*12-lift*side*12,.9f-lift*.15f,1);
    Paint(a.Sac,paw*M(.07f,0,0,.38f,.42f),mid);
    for(int toe=0;toe<3;toe++)Paint(a.Claw,paw*M(.13f,(toe-1)*.095f,(toe-1)*9,.22f,.20f),edge);
   }
   // A ribcage, narrow waist and pelvis establish an animal silhouette.
   Paint(a.Body,root*M(-.18f,0,0,1.0f,width*1.2f),low);
   Paint(a.Body,root*M(.35f,0,0,.62f,width*1.1f),mid);
   Paint(a.Body,root*M(-.72f,0,0,.43f,width),mid);
   for(int j=0;j<5;j++)for(int side=-1;side<=1;side+=2){
    var rib=root*M(-.78f+j*.29f,side*width*.15f,side*14,.45f,side*width*.91f);
    Paint(a.Plate,rib,edge);Paint(a.Plate,rib*M(.02f,0,0,.83f,.77f),mid);
    if(Region==3)Paint(a.Shard,rib*M(.06f,-.25f,side*18,.72f,.40f),accent);
    else Paint(a.Horn,rib*M(0,-.23f,0,.60f,.35f),edge);
   }
   // A distinct neck and paired jaw halves; the muzzle is not an abstract point.
   var neck=root*M(.75f,0,Mathf.Sin(gait*.5f)*3,1,1);
   Paint(a.Body,neck*M(.02f,0,0,.46f,.27f),mid);
   var head=neck*M(.35f+attack*.06f,0,0,.75f,1);
   Paint(a.Body,head*M(.05f,0,0,.48f,.24f),low);
   Paint(a.Plate,head*M(.03f,0,0,.60f,.35f),mid);
   Paint(a.Body,head*M(.39f,0,0,.28f,.15f),mid);
   for(int side=-1;side<=1;side+=2){
    Paint(a.Shard,head*M(.23f,side*(.14f+attack*.10f),side*(12+attack*14),.47f,side*.32f),edge);
    Paint(a.Eye,head*M(.22f,side*.16f,0,.85f,.6f),accent);
    var ear=head*M(-.12f,side*.19f,side*(120+Mathf.Sin(gait*.45f)*4),.60f,side*.45f);
    Paint(a.Horn,ear,edge);Paint(a.Horn,ear*M(0,0,0,.8f,.5f),mid);
   }
   // Tail bends in segments independently of the body and walking cycle.
   var tail=root*M(-.98f,0,180,1,1);
   for(int j=0;j<6;j++){
    tail=tail*M(.20f,0,Mathf.Sin(gait*.65f-j*.6f)*8);
    Paint(a.Body,tail*M(.06f,0,0,.23f,.12f*(1-j*.12f)),mid);
    Paint(a.Horn,tail*M(.03f,-.02f,0,.32f,.32f*(1-j*.1f)),edge);
   }
   if(Region==3){
    for(int side=-1;side<=1;side+=2)for(int j=0;j<(heavy?4:2);j++){
     var spine=root*M(-.6f+j*.32f,side*width*.8f,side*(75+j*8+attack*8),heavy?.95f:.6f,side*.65f);
     Paint(a.Shard,spine,mid);Paint(a.Shard,spine*M(.025f,0,0,.85f,.52f),edge);Paint(a.Shard,spine*M(.05f,0,0,.65f,.16f),accent);
    }
   }else if(Region==4){
    for(int side=-1;side<=1;side+=2)for(int j=0;j<4;j++){
     var flow=root*M(.40f-j*.35f,side*.20f,side*(120+j*10+Mathf.Sin(gait-j)*5),.80f,side*.6f);
     for(int k=0;k<3;k++){Paint(a.Horn,flow,edge);Paint(a.Horn,flow*M(.02f,0,0,.9f,.25f),accent);flow=flow*M(.44f,0,Mathf.Sin(gait-k)*10,.8f,.8f);}
    }
   }else{
    for(int side=-1;side<=1;side+=2){
     var horn=root*M(.37f,side*.25f,side*(130+Mathf.Sin(gait*.5f)*5),id==0?1.3f:1.0f,side*.70f);
     Paint(a.Horn,horn,edge);Paint(a.Horn,horn*M(.02f,0,0,.85f,.48f),mid);
     Paint(a.Horn,root*M(-.50f,side*.28f,side*110,.80f,side*.80f),edge);
    }
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
   for(int slot=0;slot<6;slot++){int id=page*6+slot;if(id>=Entries.Length)break;float x=slot%2==0?125:325,y=340+slot/2*220;Creature(id,x,y,time);C(feedback,"Text",(id+1).ToString("00")+" "+Names[id],(double)x,(double)(y+61),new Color(.8f,.85f,.72f),8f);}
   var ps=Activator.CreateInstance(T("NativeWorldPlayer").GetNestedType("State",F));void S(string n,object v)=>ps.GetType().GetField(n,F).SetValue(ps,v);S("X",225d);S("Y",510d);S("Radius",14d);S("Hp",100d);S("Time",time);S("Character","hunter");S("Color",new Color(.75f,.3f,1));C(player,"Draw",surface,ps,creatures);

   C(standard,"BeginFrame");C(standard,"DrawCore",surface,21d,.7d,(Matrix4x4?)M(181,555));C(standard,"DrawRing",surface,24d,time*.2,.9d,(Matrix4x4?)M(181,555));
   C(surface,"End");field.image=surface.GetType().GetProperty("UiTexture",F).GetValue(surface) as Texture;hud.Prepare(1.5f);
  }
  EditorApplication.CallbackFunction tick=null;void Done(string result){EditorApplication.update-=tick;hud.Dispose();panel.targetTexture=null;foreach(var o in new[]{depth,player,creatures,feedback,minions,standard})((IDisposable)o).Dispose();foreach(var a in art)a.Dispose();UnityEngine.Object.Destroy(mat);UnityEngine.Object.Destroy(owner);rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(panel);Application.runInBackground=bg;File.WriteAllText(dir+"/result.json",result);}
  tick=()=>{try{EditorApplication.QueuePlayerLoopUpdate();if(Time.frameCount==last)return;last=Time.frameCount;if(EditorApplication.timeSinceStartup-started>120)throw new Exception("deadline");Draw(2+frame/18d);if(!(bool)depth.GetType().GetProperty("Ready",F).GetValue(depth)||!hud.Ready)return;if(page==0 && frame<36){
    var prev=RenderTexture.active;RenderTexture.active=rt;var f=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);f.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);f.Apply();File.WriteAllBytes(dir+"/motion-"+frame.ToString("D2")+".png",f.EncodeToPNG());UnityEngine.Object.Destroy(f);RenderTexture.active=prev;
   }if(++frame<36)return;
   var old=RenderTexture.active;var shot=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);RenderTexture.active=rt;shot.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);shot.Apply();File.WriteAllBytes(dir+"/game-page-"+(page+1)+".png",shot.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.Destroy(shot);if(++page==(Entries.Length+5)/6){Done("PASS:"+Entries.Length+" preview-only creatures, region "+(Region+1));return;}frame=0;
  }catch(Exception e){Done(e.ToString());}};EditorApplication.update+=tick;return "Queued region "+(Region+1)+": "+Entries.Length+" preview-only native models";
 }
}
