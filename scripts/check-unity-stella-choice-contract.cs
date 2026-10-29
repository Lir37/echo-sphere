using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using EchoSphere.Core;
using EchoSphere.Runtime;
public static class StellaChoiceAcceptance
{
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
  var r=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var t=r.GetType();var names=new[]{"_runEconomy","_runTime","_gameOver","_stellaRewardPending","_levelUp","_choosingEvolution","_userPaused","_paused","_stellaRewardIsLegendary","_message","_messageTimer","_sourceFlashKind","_sourceFlashColor"};var old=new Dictionary<string,object>();foreach(var name in names)old[name]=t.GetField(name,F).GetValue(r);
  var owned=(List<string>)t.GetField("_ownedArtifacts",F).GetValue(r);var before=owned.ToArray();var choices=(List<ArtifactRewardDefinition>)t.GetField("_stellaRewardChoices",F).GetValue(r);var oldChoices=choices.ToArray();var rng=(SeededRng)t.GetField("_rng",F).GetValue(r);uint oldRng=rng.State;int checks=0;
  try{
   owned.Clear();owned.Add("quantum_fold");
   foreach(bool modern in new[]{false,true})foreach(float time in new[]{1000f,1600f}){
    t.GetField("_runEconomy",F).SetValue(r,modern?RunEconomyRules.Create(1):null);t.GetField("_runTime",F).SetValue(r,time);foreach(var flag in new[]{"_gameOver","_stellaRewardPending","_levelUp","_choosingEvolution"})t.GetField(flag,F).SetValue(r,false);
    var go=new GameObject("Stella choice acceptance");var chest=go.AddComponent<StellaChestAgent2D>();
    try{r.CollectStellaChest(chest);int expected=modern||time<1500?4:3;checks++;if(choices.Count!=expected)throw new Exception("Wrong choice count "+modern+"/"+time+": "+choices.Count);}
    finally{UnityEngine.Object.DestroyImmediate(go);}
   }
   var result=new{result="PASS",checks,scope="Actual CollectStellaChest early/late and legacy/modern quantum_fold; run RNG and borrowed state restored; no prefs writes"};File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/stella-choice-acceptance.json")),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }finally{foreach(var name in names)t.GetField(name,F).SetValue(r,old[name]);owned.Clear();owned.AddRange(before);choices.Clear();choices.AddRange(oldChoices);rng.RestoreState(oldRng);}
 }
}
