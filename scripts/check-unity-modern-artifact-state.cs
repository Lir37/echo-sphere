using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using EchoSphere.Core;
using EchoSphere.Runtime;
public static class ModernArtifactStateAcceptance {
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");
  const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
  var r=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var t=r.GetType();
  var names=new[]{"_runEconomy","_resonanceCharge","_resonanceEventActive","_timeSplitterBuffTimer","_mandalaDamageStored","_deathTimer","_characterScreenShakeIntensity"};
  var old=new Dictionary<string,object>();foreach(var n in names)old[n]=t.GetField(n,F).GetValue(r);
  var owned=(List<string>)t.GetField("_ownedArtifacts",F).GetValue(r);var before=owned.ToArray();var core=(PlayerCoreController2D)t.GetField("_playerCore",F).GetValue(r);var player=RunSnapshotCodec.Capture(core);int checks=0;GameObject created=null,enemyObject=null;var enemies=(List<EnemyAgent2D>)t.GetField("_enemies",F).GetValue(r);EnemyAgent2D fixtureEnemy=null;
  void Near(float a,float b,string why){checks++;if(Math.Abs(a-b)>.001f)throw new Exception(why+": "+a+" expected "+b);}
  void Equip(params string[] ids){owned.Clear();owned.AddRange(ids);}
  void ChargeReset(){t.GetField("_resonanceCharge",F).SetValue(r,0f);t.GetField("_resonanceEventActive",F).SetValue(r,false);}
  try{
   t.GetField("_runEconomy",F).SetValue(r,RunEconomyRules.Create(9));Equip();ChargeReset();r.AddResonanceCharge(1);float baseline=(float)t.GetField("_resonanceCharge",F).GetValue(r);
   foreach(var x in new[]{("resonant_leaf",1.10f),("resonance_lattice",1.15f)}){Equip(x.Item1);ChargeReset();r.AddResonanceCharge(1);Near((float)t.GetField("_resonanceCharge",F).GetValue(r),baseline*x.Item2,x.Item1+" actual charge");}
   Equip("resonant_leaf","resonance_lattice");ChargeReset();r.AddResonanceCharge(1);Near((float)t.GetField("_resonanceCharge",F).GetValue(r),baseline*1.25f,"combined charge once");
   t.GetField("_resonanceEventActive",F).SetValue(r,true);r.AddResonanceCharge(10);Near((float)t.GetField("_resonanceCharge",F).GetValue(r),baseline*1.25f,"no charge during event");
   Equip("stasis_mandala");core.SetMaxHpForNewRun(1000);core.SetShieldCharges(0);core.Tick(100);t.GetField("_mandalaDamageStored",F).SetValue(r,0f);
   r.TryDamagePlayerOverTime(25);float lost=1000-core.CurrentHp;Near((float)t.GetField("_mandalaDamageStored",F).GetValue(r),lost,"DoT records actual HP loss");
   r.TryDamagePlayerOverTime(100);Near(core.ShieldCharges,1,"DoT grants bounded charge");
   float stored=(float)t.GetField("_mandalaDamageStored",F).GetValue(r);core.GrantInvulnerability(1);r.TryDamagePlayerOverTime(100);Near(core.ShieldCharges,1,"invulnerable damage no shield");Near((float)t.GetField("_mandalaDamageStored",F).GetValue(r),stored,"invulnerable damage not stored");
   t.GetField("_timeSplitterBuffTimer",F).SetValue(r,3.25f);t.GetField("_mandalaDamageStored",F).SetValue(r,17f);
   var snapshot=RunSnapshotCodec.Capture(r,field=>field.Name=="_timeSplitterBuffTimer"||field.Name=="_mandalaDamageStored");
   t.GetField("_timeSplitterBuffTimer",F).SetValue(r,0f);t.GetField("_mandalaDamageStored",F).SetValue(r,0f);RunSnapshotCodec.Restore(snapshot,t,r);
   Near((float)t.GetField("_timeSplitterBuffTimer",F).GetValue(r),3.25f,"buff snapshot roundtrip");Near((float)t.GetField("_mandalaDamageStored",F).GetValue(r),17f,"mandala snapshot roundtrip");
   t.GetField("_runEconomy",F).SetValue(r,null);Equip("resonant_leaf");ChargeReset();r.AddResonanceCharge(1);Near((float)t.GetField("_resonanceCharge",F).GetValue(r),baseline,"legacy charge unchanged");
   t.GetField("_runEconomy",F).SetValue(r,RunEconomyRules.Create(9));Equip();
   created=new GameObject("Modern artifact damage fixture");var sphere=created.AddComponent<SphereAttackAgent>();sphere.Initialize(r,core.transform,SphereId.Standard,12);
   t.GetField("_timeSplitterBuffTimer",F).SetValue(r,0f);float baseDamage=r.GetSourceSphereAttackDamage(sphere);
   Equip("time_splitter");Near(r.GetSourceSphereAttackDamage(sphere),baseDamage,"splitter no permanent damage");
   t.GetField("_timeSplitterBuffTimer",F).SetValue(r,2f);Near(r.GetSourceSphereAttackDamage(sphere),baseDamage*1.15f,"splitter actual timed damage");
   Equip();Near(r.GetSourceSphereAttackDamage(sphere),baseDamage,"timer alone grants no damage");
   enemyObject=new GameObject("Stasis damage fixture");fixtureEnemy=enemyObject.AddComponent<EnemyAgent2D>();fixtureEnemy.Initialize(r,core.transform,100,0);enemies.Add(fixtureEnemy);
   Equip("stasis_core");core.SetMaxHpForNewRun(1000);core.SetShieldCharges(0);core.Tick(100);r.TryDamagePlayerOverTime(10);Near(fixtureEnemy.MovementSlowMultiplier,.5f,"stasis actual DoT slows living enemy");
   typeof(EnemyAgent2D).GetField("_slowTimer",F).SetValue(fixtureEnemy,0f);t.GetField("_runEconomy",F).SetValue(r,null);r.TryDamagePlayerOverTime(10);Near(fixtureEnemy.MovementSlowMultiplier,1,"legacy DoT no new stasis behavior");
   var result=new{result="PASS",checks,scope="Actual runtime charge,DoT shield consumer,invulnerability and snapshot codec; no PlayerPrefs writes"};File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/modern-artifact-state-acceptance.json")),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }finally{if(fixtureEnemy!=null)enemies.Remove(fixtureEnemy);if(enemyObject!=null)UnityEngine.Object.Destroy(enemyObject);if(created!=null)UnityEngine.Object.Destroy(created);foreach(var n in names)t.GetField(n,F).SetValue(r,old[n]);owned.Clear();owned.AddRange(before);RunSnapshotCodec.Restore(player,core.GetType(),core);}
 }
}
