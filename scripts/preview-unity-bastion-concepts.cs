// Preview-only native mesh concepts. Never installed in the production enemy path.
using System;using System.IO;using System.Linq;using System.Reflection;using System.Collections.Generic;
using UnityEditor;using UnityEngine;using UnityEngine.UIElements;using EchoSphere.Presentation;
public static class BastionConceptPreview
{
 const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 static Type T(string name)=>typeof(NativeWorldPresentation).Assembly.GetType("EchoSphere.Presentation."+name,true);
 static object C(object o,string name,params object[] args){var m=o.GetType().GetMethods(F).Where(x=>x.Name==name&&x.GetParameters().Length>=args.Length).OrderBy(x=>x.GetParameters().Length).First();var p=m.GetParameters();return m.Invoke(o,p.Select((x,i)=>i<args.Length?args[i]:x.DefaultValue).ToArray());}
 static readonly string[] Names={"Tunnel Plug","Burst Sac","Gate Ram","Mold Carrier","Hush Drone","Casemate Worker","Resin Nurse","Cistern Leech","Crack Walker","Spore Spitter","Clutch Bearer","Brood Runner","Hive Shieldbearer","Linkbreaker","Rhythm Eater","Phantom Hunter","Pattern Shifter","Elder Brood","Mirror Warden","Stasis Warden","Nullifier","Ash Carrier","Elder Gatherer","Void Lancer","Breacher","Aura Titan","Stranger Conductor","Blind Architect","The Null","Stella Warden","Brood Queen"};
 sealed class Art:IDisposable
 {
  public Mesh Body,Head,Plate,Leg,Foot,Eye,Wing,Claw,Sac;
  public float Width,Length;public int Kind;public bool Winged,Segmented,Boss;
  static Mesh Poly(params Vector2[] pts){var m=new Mesh();var vertices=new Vector3[pts.Length];for(int i=0;i<pts.Length;i++)vertices[i]=pts[i];var tris=new int[(pts.Length-2)*3];for(int i=1;i<pts.Length-1;i++){tris[(i-1)*3]=0;tris[(i-1)*3+1]=i;tris[(i-1)*3+2]=i+1;}m.vertices=vertices;m.triangles=tris;m.RecalculateBounds();return m;}
  static Mesh Oval(float x,float y){return Poly(Enumerable.Range(0,24).Select(i=>new Vector2(Mathf.Cos(i*Mathf.PI/12)*x,Mathf.Sin(i*Mathf.PI/12)*y)).ToArray());}
  public Art(int id)
  {
   Kind=id;Boss=id>=23;int b=Boss?new[]{9,2,12,6,0,7,12,10}[id-23]:id%13;
   Width=new[]{.85f,.62f,.8f,.65f,.77f,.48f,.5f,.43f,.39f,.62f,.82f,.37f,.82f}[b];Length=b==7?1.6f:b==11?.8f:1;
   Winged=b==1||b==6||b==10||id==15||id==21||id==26;Segmented=b==7||b==10||id==17||id==30;
   Body=Oval(Length,Width);Head=Poly(new Vector2(.43f,-.4f),new Vector2(.88f,-.34f),new Vector2(1.08f,0),new Vector2(.88f,.34f),new Vector2(.43f,.4f));
   Plate=Poly(new Vector2(-.84f,0),new Vector2(-.63f,-.56f),new Vector2(-.18f,-.7f),new Vector2(.45f,-.46f),new Vector2(.73f,0),new Vector2(.3f,.22f));
   Leg=Poly(new Vector2(0,-.05f),new Vector2(.48f,-.1f),new Vector2(.72f,.04f),new Vector2(.48f,.09f),new Vector2(0,.04f));
   Foot=Poly(new Vector2(0,-.045f),new Vector2(.5f,-.1f),new Vector2(.74f,0),new Vector2(.5f,.035f),new Vector2(0,.035f));
   Eye=Oval(.07f,.045f);Wing=Poly(new Vector2(.2f,0),new Vector2(-.45f,-.71f),new Vector2(-1.38f,-.5f),new Vector2(-1.65f,-.12f),new Vector2(-.7f,.06f));
   Claw=Poly(new Vector2(0,-.09f),new Vector2(.49f,-.22f),new Vector2(.72f,0),new Vector2(.4f,-.03f),new Vector2(.25f,.09f));Sac=Oval(.35f,.3f);
  }
  public void Dispose(){foreach(var m in new[]{Body,Head,Plate,Leg,Foot,Eye,Wing,Claw,Sac})UnityEngine.Object.Destroy(m);}
 }
 public static object Main()
 {
  if(!Application.isPlaying)throw new Exception("Play required");string dir="Library/bastion-concepts";Directory.CreateDirectory(dir);
  var owner=new GameObject("Isolated Bastion game-sized concept preview");var surface=owner.AddComponent(T("NativeWorldSurface"));C(surface,"Initialize");C(surface,"SetViewport",450f,1000f,1.5f);
  var depth=Activator.CreateInstance(T("NativeWorldDepthBackdrop"),true);var player=Activator.CreateInstance(T("NativeWorldPlayer"),true);var creatures=Activator.CreateInstance(T("NativeWorldCreatures"),true);var feedback=Activator.CreateInstance(T("NativeWorldFeedback"),true);var minions=Activator.CreateInstance(T("NativeWorldMinions"),true);
  var standard=Activator.CreateInstance(T("NativeWorldStandardSphere"),true);
  var art=Enumerable.Range(0,31).Select(i=>new Art(i)).ToArray();var mat=new Material(Resources.Load<Shader>("EchoSphere/NativeWorldSolid"));var props=new List<MaterialPropertyBlock>();int used=0;
  var draw=surface.GetType().GetMethod("DrawNativeMesh",F);
  Matrix4x4 M(float x,float y,float angle=0,float sx=1,float sy=1)=>Matrix4x4.TRS(new Vector3(x,y,0),Quaternion.Euler(0,0,angle),new Vector3(sx,sy,1));
  void Paint(Mesh mesh,Matrix4x4 matrix,Color color){if(used==props.Count)props.Add(new MaterialPropertyBlock());var p=props[used++];p.SetColor("_NativeColor",color);draw.Invoke(surface,new object[]{mesh,mat,p,(Matrix4x4?)matrix,null,0f,false,null,false,false,false,false});}
  Color Col(float r,float g,float b)=>new Color(r,g,b,1);var edge=Col(.40f,.49f,.29f);var shell=Col(.24f,.30f,.15f);var dark=Col(.065f,.09f,.055f);var tip=Col(.64f,.61f,.40f);
  void Creature(int id,float x,float y,double time)
  {
   var a=art[id];float r=a.Boss?32:16.5f;float aim=Mathf.Atan2(510-y,225-x)*Mathf.Rad2Deg;var root=M(x,y,aim,r,r);
   float cycle=(float)time*5.5f+id*.7f;
   float bite=Mathf.Pow(Mathf.Max(0,Mathf.Sin((float)time*2.2f+id)),6);
   var pale=Col(.61f,.64f,.40f);var seam=Col(.12f,.17f,.10f);var glint=Col(.52f,.59f,.35f);
   // Each limb has a coxa, upper shell, elbow and independently articulated tarsus.
   for(int side=-1;side<=1;side+=2)for(int k=0;k<3;k++){
    float stride=Mathf.Sin(cycle+k*2.1f+side*1.5f)*13;
    var joint=root*M((k-1)*.43f,side*a.Width*.68f,side*(45+k*20)+stride);
    Paint(a.Sac,joint*M(0,0,0,.32f,.27f),dark);
    Paint(a.Leg,joint,edge);Paint(a.Leg,joint*M(.035f,-.018f,0,.82f,.36f),glint);
    var elbow=joint*M(.61f,.025f,side*52-stride);
    Paint(a.Sac,elbow*M(0,0,0,.24f,.22f),seam);
    Paint(a.Foot,elbow,tip);Paint(a.Foot,elbow*M(.03f,-.015f,0,.78f,.34f),edge);
    Paint(a.Claw,elbow*M(.64f,0,side*12,.24f,.22f),pale);
   }
   if(a.Winged)for(int side=-1;side<=1;side+=2){
    var wing=root*M(-.08f,side*.12f,side*(12+Mathf.Sin(cycle*1.4f)*8),1,side);
    Paint(a.Wing,wing,Col(.24f,.29f,.19f));
    Paint(a.Wing,wing*M(-.03f,-.015f,0,.90f,.74f),Col(.32f,.38f,.23f));
    for(int vein=0;vein<3;vein++)Paint(a.Leg,wing*M(-.15f,-.10f-vein*.13f,165+vein*8,1.3f,.23f),edge);
   }
   Paint(a.Body,root*M(-.18f,0,0,a.Length,a.Width),dark);
   // Overlapping chitin scutes, inset seams and a narrow ridge, never a solid oval body.
   int plates=a.Segmented?7:5;
   for(int j=0;j<plates;j++){
    float px=-1.10f+j*(a.Segmented?.34f:.31f);
    float taper=1-Mathf.Abs(j-(plates-1)*.5f)*.10f;
    float flex=a.Segmented?Mathf.Sin(cycle-j*.55f)*.045f:0;
    for(int side=-1;side<=1;side+=2){
     var plate=root*M(px,flex,side*(7+bite*3),.39f,a.Width*side*taper);
     Paint(a.Plate,plate,edge);Paint(a.Plate,plate*M(.03f,.015f,0,.83f,.79f),shell);
     Paint(a.Leg,plate*M(-.5f,-.40f,12,.8f,.18f),glint);
     if((id+j)%3==0)Paint(a.Claw,plate*M(-.35f,-.56f,-95,.33f,.24f),tip);
    }
    Paint(a.Sac,root*M(px-.13f,flex,0,.24f,.15f),seam);
   }
   // Armoured brow, inset sensory organs and opposing mandibles with visible bite travel.
   var head=root*M(.25f+bite*.07f,0,0,id==2||id==24?1.13f:.85f,.85f);
   Paint(a.Head,head,edge);Paint(a.Head,head*M(.035f,0,0,.88f,.75f),shell);
   Paint(a.Plate,head*M(.75f,0,90,.33f,.28f),glint);
   for(int side=-1;side<=1;side+=2){
    Paint(a.Sac,root*M(.90f,side*.22f,0,.32f,.24f),seam);
    Paint(a.Eye,root*M(.93f,side*.22f,0,1.1f,.8f),Col(.86f,.62f,.22f));
    var jaw=root*M(.99f,side*.24f,side*(19+bite*24),id==15||id==23?1.12f:.66f,side*.76f);
    Paint(a.Claw,jaw,tip);Paint(a.Claw,jaw*M(.02f,-.018f,0,.75f,.48f),edge);
    Paint(a.Claw,jaw*M(.31f,-.1f,-65,.23f,.25f),pale);
    var antenna=root*M(.86f,side*.31f,side*(40+Mathf.Sin(cycle*.6f)*7),.85f,side*.37f);
    Paint(a.Foot,antenna,edge);Paint(a.Eye,antenna*M(.68f,0,0,.45f,.45f),pale);
   }
   if(id==1||id==9||id==21||id==30)for(int side=-1;side<=1;side+=2){
    var sac=root*M(-.40f,side*.40f,side*(8+bite*12),1,.85f);
    Paint(a.Sac,sac,seam);Paint(a.Sac,sac*M(0,0,0,.83f,.80f),Col(.48f,.36f,.15f));
    for(int n=0;n<3;n++)Paint(a.Eye,sac*M(-.15f+n*.14f,0,0,.65f,2.4f),Col(.71f,.55f,.22f));
    Paint(a.Claw,sac*M(.25f,0,side*(12+bite*25),.5f,.4f),edge);
   }
   if(id==3||id==16||id==20)for(int side=-1;side<=1;side+=2)for(int j=0;j<3;j++){
    var spine=root*M(-.6f+j*.4f,side*a.Width,side*(95+Mathf.Sin(cycle+j)*4),.72f,side*.75f);
    Paint(a.Claw,spine,tip);Paint(a.Claw,spine*M(.03f,0,0,.7f,.44f),shell);
   }
   if(id==0||id==12||id==18||id==27||id==29)for(int j=0;j<4;j++){
    var shield=root*M(-.85f+j*.35f,0,0,.48f,a.Width*(1.65f-j*.13f));
    Paint(a.Head,shield,edge);Paint(a.Head,shield*M(.04f,0,0,.77f,.84f),shell);
   }
  }
  // Actual retained UI classes, not painted mock HUD labels.
  var panel=ScriptableObject.CreateInstance<PanelSettings>();panel.scaleMode=PanelScaleMode.ConstantPixelSize;panel.scale=1;panel.forceGammaRendering=true;panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");var rt=new RenderTexture(675,1500,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);rt.Create();panel.targetTexture=rt;panel.scale=1.5f;
  var doc=owner.AddComponent<UIDocument>();doc.panelSettings=panel;var rootUi=doc.rootVisualElement;rootUi.style.position=Position.Absolute;rootUi.style.left=rootUi.style.right=rootUi.style.top=rootUi.style.bottom=0;
  var field=new Image{scaleMode=ScaleMode.StretchToFill,pickingMode=PickingMode.Ignore};field.style.position=Position.Absolute;field.style.left=field.style.right=field.style.top=field.style.bottom=0;rootUi.Add(field);
  var hud=new NativeHudView(rootUi,()=>false,_=>null);hud.Bind(new NativeHudView.State{Russian=true,Level="12",Hp="140/140",XpPercent=40,HpPercent=100,ResonancePercent=45,Resonance="45/100",Spheres="СФЕРЫ 3/5",Wave="ВОЛНА 8",Links="3",Timer="06:42",HasRegion=true,Region="РАСКОЛОТЫЙ БАСТИОН",Phase="ГЛУБИННЫЙ УЛЕЙ",Pocket=""});
  int page=0,frame=0,last=-1;double started=EditorApplication.timeSinceStartup;bool bg=Application.runInBackground;Application.runInBackground=true;
  void Draw(double time)
  {
   used=0;C(surface,"Begin",new Vector2(225,500));C(surface,"ResetTransform");C(depth,"Draw",surface,450f,1000f,2,Vector2.zero,null,time);C(player,"BeginFrame");C(creatures,"BeginFrame");C(feedback,"BeginFrame",surface);
   for(int slot=0;slot<6;slot++){int id=page*6+slot;if(id>=31)break;float x=slot%2==0?102:348,y=230+slot/2*220;Creature(id,x,y,time);C(feedback,"Text",(id+1).ToString("00")+" "+Names[id],(double)(x-68),(double)(y+61),new Color(.8f,.85f,.72f),8f);}
   var ps=Activator.CreateInstance(T("NativeWorldPlayer").GetNestedType("State",F));void S(string n,object v)=>ps.GetType().GetField(n,F).SetValue(ps,v);S("X",225d);S("Y",510d);S("Radius",14d);S("Hp",100d);S("Time",time);S("Character","hunter");S("Color",new Color(.75f,.3f,1));C(player,"Draw",surface,ps,creatures);
   C(minions,"BeginFrame");C(minions,"DrawFusionCrown",surface,181f,555f,21f,new Color(.3f,.9f,1),time);
   C(standard,"BeginFrame");C(standard,"DrawCore",surface,21d,.7d,(Matrix4x4?)M(181,555));C(standard,"DrawRing",surface,24d,time*.2,.9d,(Matrix4x4?)M(181,555));
   C(surface,"End");field.image=surface.GetType().GetProperty("UiTexture",F).GetValue(surface) as Texture;hud.Prepare(1.5f);
  }
  EditorApplication.CallbackFunction tick=null;void Done(string result){EditorApplication.update-=tick;hud.Dispose();panel.targetTexture=null;foreach(var o in new[]{depth,player,creatures,feedback,minions,standard})((IDisposable)o).Dispose();foreach(var a in art)a.Dispose();UnityEngine.Object.Destroy(mat);UnityEngine.Object.Destroy(owner);rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(panel);Application.runInBackground=bg;File.WriteAllText(dir+"/result.json",result);}
  tick=()=>{try{EditorApplication.QueuePlayerLoopUpdate();if(Time.frameCount==last)return;last=Time.frameCount;if(EditorApplication.timeSinceStartup-started>120)throw new Exception("deadline");Draw(2+frame/18d);if(!(bool)depth.GetType().GetProperty("Ready",F).GetValue(depth)||!hud.Ready)return;if(page==0 && frame<24){
    var prev=RenderTexture.active;RenderTexture.active=rt;var f=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);f.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);f.Apply();File.WriteAllBytes(dir+"/motion-"+frame.ToString("D2")+".png",f.EncodeToPNG());UnityEngine.Object.Destroy(f);RenderTexture.active=prev;
   }if(++frame<24)return;
   var old=RenderTexture.active;var shot=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);RenderTexture.active=rt;shot.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);shot.Apply();File.WriteAllBytes(dir+"/game-page-"+(page+1)+".png",shot.EncodeToPNG());RenderTexture.active=old;UnityEngine.Object.Destroy(shot);if(++page==6){Done("PASS:31 preview-only native mesh creatures, six actual UI/GPU frames");return;}frame=0;
  }catch(Exception e){Done(e.ToString());}};EditorApplication.update+=tick;return "Queued 31 preview-only native models";
 }
}
