using System;
using System.Linq;
using System.Reflection;
using EchoSphere.Runtime;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;
public static class RetainedMenuAcceptance {
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  var flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
  var runtime=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();
  var state=(NativeCharacterScreen.State)runtime.GetType().GetMethod("ReadNativeCharacterState",flags).Invoke(runtime,null);
  var go=new GameObject("Retained character acceptance");var view=go.AddComponent<NativeCharacterScreen>();
  NativeCharacterScreen.State Clone(){var copy=new NativeCharacterScreen.State{Russian=state.Russian,Gold=state.Gold,Message=state.Message};copy.Characters=state.Characters.Select(old=>{var c=new NativeCharacterScreen.Character();foreach(var f in typeof(NativeCharacterScreen.Character).GetFields())f.SetValue(c,f.GetValue(old));return c;}).ToArray();return copy;}
  int frame=0,last=-1,checks=0;VisualElement[] cards=null;var initial=Clone();int index=0;var target=initial.Characters[index];target.Unlocked=true;target.Expanded=false;
  view.Show(initial,1,()=>{},_=>{},_=>{});
  var deadline=UnityEditor.EditorApplication.timeSinceStartup+60;UnityEditor.EditorApplication.CallbackFunction tick=null;
  void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
  void Finish(string result){UnityEditor.EditorApplication.update-=tick;UnityEngine.Object.Destroy(go);UnityEditor.SessionState.SetString("EchoRetainedMenu",Newtonsoft.Json.JsonConvert.SerializeObject(new{result,checks}));}
  tick=()=>{try{
   if(UnityEditor.EditorApplication.timeSinceStartup>deadline)throw new Exception("deadline");UnityEditor.EditorApplication.QueuePlayerLoopUpdate();if(last==Time.frameCount)return;last=Time.frameCount;
   if(++frame<12){view.Show(initial,1,()=>{},_=>{},_=>{});return;}
   var root=go.GetComponent<UIDocument>().rootVisualElement;
   if(frame==12){cards=initial.Characters.Select(c=>root.Q("character-"+c.Id)).ToArray();Check(cards.All(c=>c!=null),"cards exist");
    var changed=Clone();changed.Characters[index].Unlocked=true;changed.Characters[index].Expanded=true;changed.Characters[index].Selected=!target.Selected;changed.Characters[index].MasteryXp="fixture xp";changed.Characters[index].MasteryPercent=42;
    view.Show(changed,1,()=>{},_=>{},_=>{});
    for(int i=0;i<cards.Length;i++)Check(ReferenceEquals(cards[i],root.Q("character-"+initial.Characters[i].Id)),"card replaced");
    Check(cards[index].Q<Label>("mastery-xp").text=="fixture xp","xp did not update");Check(cards[index].Q("mastery-details")!=null,"mastery not expanded");
    var details=cards[index].Q("mastery-details");view.Show(initial,1,()=>{},_=>{},_=>{});Check(ReferenceEquals(details,cards[index].Q("mastery-details")),"collapse destroyed mastery");Check(details.style.display==DisplayStyle.None,"mastery not collapsed");
    view.Hide();view.Show(initial,1,()=>{},_=>{},_=>{});Check(ReferenceEquals(cards[index],root.Q("character-"+target.Id)),"hide/show replaced card");
   }
   if(frame>25)Finish("PASS");
  }catch(Exception e){Finish("FAIL "+e);}};
  UnityEditor.SessionState.SetString("EchoRetainedMenu","RUNNING");UnityEditor.EditorApplication.update+=tick;return "queued";
 }
}
