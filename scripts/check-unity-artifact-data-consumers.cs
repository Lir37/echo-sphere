using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using EchoSphere.Core;
using EchoSphere.Runtime;
public static class ArtifactDataConsumerAcceptance
{
 private static readonly string[] ReviewedIds = {"crystal_speed","amulet_hp","ring_xp","regen_stone","radius_shard","luck_talisman","mage_pendant","long_lens","dragon_heart","echo_conductor","heavy_core","scattering_matrix","aura_lens","soul_engine","time_anchor","void_contract","quantum_core","network_anchor","pulse_lens","orbit_charm","prism_shard","void_ink","paper_ward","orbital_blade","crit_sigil","tempo_ring","gravity_crown"};
 public static object Main(){
  if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required");const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
  var r=UnityEngine.Object.FindAnyObjectByType<EchoSphereRuntime>();var t=r.GetType();var names=new[]{"_runEconomy","_paused","_sphereCharacterId","_networkState","_xp","_xpRequired","_artifactProtocolKnowledgeLoaded"};var old=new Dictionary<string,object>();foreach(var n in names)old[n]=t.GetField(n,F).GetValue(r);
  var owned=(List<string>)t.GetField("_ownedArtifacts",F).GetValue(r);var original=owned.ToArray();var abilities=(Dictionary<RunUpgradeKind,int>)t.GetField("_abilityLevels",F).GetValue(r);var oldAbilities=new Dictionary<RunUpgradeKind,int>(abilities);var core=(PlayerCoreController2D)t.GetField("_playerCore",F).GetValue(r);var player=RunSnapshotCodec.Capture(core);var rng=(SeededRng)t.GetField("_rng",F).GetValue(r);uint seed=rng.State;var known=(HashSet<string>)t.GetField("_knownArtifactProtocolIds",F).GetValue(r);var oldKnown=new List<string>(known);int checks=0;
  object Call(string name,params object[] a)=>t.GetMethod(name,F).Invoke(r,a);void Equip(string id){owned.Clear();if(id!=null)owned.Add(id);}void Near(float a,float b,string why){checks++;if(Math.Abs(a-b)>.0002f)throw new Exception(why+": "+a+" expected "+b);}
  const string Pref="echosphere_artifact_protocol_knowledge_v1";bool had=PlayerPrefs.HasKey(Pref);string pref=PlayerPrefs.GetString(Pref,"");
  try{
   t.GetField("_runEconomy",F).SetValue(r,RunEconomyRules.Create(1));t.GetField("_paused",F).SetValue(r,false);t.GetField("_sphereCharacterId",F).SetValue(r,SphereCharacterId.Spherist);t.GetField("_networkState",F).SetValue(r,new SphereNetworkState());abilities.Clear();Equip(null);
   float baseDamage=r.SphereDamageMultiplier,baseSpeed=(float)Call("GetArtifactMoveSpeedMultiplier"),baseAttack=r.AttackSpeedMultiplier,baseCooldown=(float)Call("GetActiveAbilityCooldownMultiplier");
   foreach(var c in new[]{("crystal_speed",1.15f),("dragon_heart",.9f)}){Equip(c.Item1);Near((float)Call("GetArtifactMoveSpeedMultiplier"),baseSpeed*c.Item2,c.Item1+" move speed");}
   foreach(var c in new[]{("soul_engine",1.05f),("quantum_core",1.08f),("network_anchor",1.04f),("paper_ward",1.04f)}){Equip(c.Item1);Near(r.SphereDamageMultiplier,baseDamage*c.Item2,c.Item1+" global damage");}
   foreach(var c in new[]{("echo_conductor",SphereId.Chain,1.12f),("heavy_core",SphereId.Standard,1.15f),("scattering_matrix",SphereId.Shotgun,1.12f),("orbit_charm",SphereId.Orbital,1.12f),("prism_shard",SphereId.Prism,1.1f),("void_ink",SphereId.Void,1.1f),("orbital_blade",SphereId.Orbital,1.1f)}){Equip(c.Item1);Near((float)Call("GetArtifactSphereFamilyDamageMultiplier",c.Item2),c.Item3,c.Item1+" family damage");}
   foreach(var c in new[]{("radius_shard",SphereId.Standard,1.1f),("long_lens",SphereId.Sniper,1.18f),("aura_lens",SphereId.Aura,1.18f),("pulse_lens",SphereId.Pulse,1.1f),("gravity_crown",SphereId.Gravity,1.2f)}){Equip(c.Item1);Near(r.GetArtifactSphereRadiusMultiplier(c.Item2),c.Item3,c.Item1+" radius");}
   foreach(var c in new[]{("mage_pendant",.9f),("time_anchor",.88f)}){Equip(c.Item1);Near((float)Call("GetActiveAbilityCooldownMultiplier"),baseCooldown*c.Item2,c.Item1+" cooldown");}
   Equip("tempo_ring");Near(r.AttackSpeedMultiplier,baseAttack/.92f,"tempo cadence");Equip("void_contract");Near(r.SphereDamageMultiplier,baseDamage*1.22f,"void contract offense");Near((float)Call("GetArtifactOnlyDamageTakenMultiplier"),1.15f,"void contract penalty");
   Equip("quantum_core");Near((float)(double)Call("NativeLivePlannerDodgeChance"),.08f,"quantum dodge");Equip("regen_stone");Near((float)Call("GetRunArtifactEffectTotal","regen"),1,"regen consumer input");
   t.GetField("_xpRequired",F).SetValue(r,100000);Equip(null);t.GetField("_xp",F).SetValue(r,0f);r.AddExperience(1);float unit=(float)t.GetField("_xp",F).GetValue(r);
   foreach(var c in new[]{("ring_xp",1.2f),("soul_engine",1.1f)}){Equip(c.Item1);t.GetField("_xp",F).SetValue(r,0f);r.AddExperience(1);Near((float)t.GetField("_xp",F).GetValue(r),unit*c.Item2,c.Item1+" actual XP");}
   foreach(var c in new[]{("amulet_hp",30f),("dragon_heart",50f)}){RunSnapshotCodec.Restore(player,core.GetType(),core);Equip(c.Item1);float max=core.MaxHp,hp=core.CurrentHp;Call("ApplyOwnedArtifactEffects",ArtifactRewardRules.Get(c.Item1));Near(core.MaxHp,max+c.Item2,c.Item1+" acquisition max HP");Near(core.CurrentHp,Math.Min(core.MaxHp,hp+c.Item2),c.Item1+" acquisition heal");}
   float shopCrit=PersistentUpgradeRules.GetCritChanceBonus((int)Call("GetShopUpgradeLevel","crit"));
   foreach(var c in new[]{("luck_talisman",.15f),("crit_sigil",.05f)})for(uint i=1;i<=32;i++){Equip(c.Item1);rng.Reset(i);var reference=new SeededRng(i);float roll=reference.NextFloat();bool expected=CombatRules.ResolveDamage(100,Math.Min(CombatRules.CritHardCap,CombatRules.CritBase+shopCrit+c.Item2),roll,CombatRules.CritMultiplierBase).WasCritical;r.ResolveProjectileDamage(100,0,out bool actual);Near(actual?1:0,expected?1:0,c.Item1+" seeded crit outcome");}
   var result=new{result="PASS",checks,reviewed=ReviewedIds,scope="Existing unchanged data-driven artifacts:actual runtime stat consumers,XP,HP acquisition and seeded crit;regen scalar wiring inspected,not a timed regen playtest;no gameplay changes"};File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../Library/artifact-data-consumers-acceptance.json")),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
  }finally{foreach(var n in names)t.GetField(n,F).SetValue(r,old[n]);owned.Clear();owned.AddRange(original);abilities.Clear();foreach(var pair in oldAbilities)abilities[pair.Key]=pair.Value;RunSnapshotCodec.Restore(player,core.GetType(),core);rng.RestoreState(seed);known.Clear();foreach(var id in oldKnown)known.Add(id);if(had)PlayerPrefs.SetString(Pref,pref);else PlayerPrefs.DeleteKey(Pref);PlayerPrefs.Save();}
 }
}
