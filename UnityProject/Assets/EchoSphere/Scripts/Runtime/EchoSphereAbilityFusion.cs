using System;
using System.Collections.Generic;
using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;
namespace EchoSphere.Runtime
{
 public sealed partial class EchoSphereRuntime
 {
  // Run data, captured by the existing backwards-compatible snapshot codec.
  private readonly List<string> _fusedAbilityLinks=new List<string>();
  private readonly Dictionary<string,float> _fusionNextAt=new Dictionary<string,float>();
  private ActiveAbilitySynergyLink FusionLink(string id){foreach(var link in ActiveAbilitySynergyLinks)if(link.Id==id)return link;return null;}
  private bool AbilityIsFused(RunUpgradeKind ability){foreach(var id in _fusedAbilityLinks)if(FusionLink(id)?.Ability==ability)return true;return false;}
  private bool CanFuse(ActiveAbilitySynergyLink link)=>link!=null&&!AbilityIsFused(link.Ability)&&
   MatchesAbilitySynergy(link.Sphere,link.SphereBranch,link.Ability,link.AbilityFinal)&&!string.IsNullOrEmpty(GetSphereFinal(link.Sphere));
  private NativeRunCatalog.SphereAbilityLink FusionText(string id){foreach(var row in NativeRunCatalog.Instance.SphereAbilityLinks)if(row.Id==id)return row;return null;}
  private void AddGuaranteedFusionChoices()
  {
   foreach(var link in ActiveAbilitySynergyLinks)if(CanFuse(link))
    _levelUpChoices.Add(new RunUpgradeChoice(RunUpgradeKind.Fusion,link.Sphere,7,7,link.Id,""));
  }
  private bool ChooseAbilityFusion(string id)
  {
   var link=FusionLink(id);if(!CanFuse(link))return false;
   EnsureKnowledgeLoaded();RecordNaturalKnowledge("synergy:"+id);_fusedAbilityLinks.Add(id);_fusionNextAt[id]=_runTime+GetActiveAbilityCooldown(link.Ability);
   _activeAbilityHotkeys.Remove(link.Ability);
   ShowTimedCombatMessage(FusionText(id)?.Name(IsRussianUi)??id,2.2f,"#91e8d3");SourceAudio2D.Play("evolve");
   return true;
  }
  public bool HasSphereFusion(SphereId type){foreach(var id in _fusedAbilityLinks)if(FusionLink(id)?.Sphere==type)return true;return false;}
  public void TriggerSphereFusion(SphereAttackAgent sphere)
  {
   if(sphere==null||!sphere.gameObject.activeInHierarchy||IsGameplayPaused)return;
   foreach(var id in _fusedAbilityLinks)
   {
    var link=FusionLink(id);if(link==null||link.Sphere!=sphere.Type||!IsActiveAbilitySynergyRaw(link.Sphere,link.SphereBranch,link.Ability,link.AbilityFinal))continue;
    if(_fusionNextAt.TryGetValue(id,out var next)&&_runTime<next)continue;
    EnsureKnowledgeLoaded();RecordNaturalKnowledge("ability:"+SourceAbilityId(link.Ability));int level=GetAbilityLevel(link.Ability);
    // Reuse the authored ability cadence and synergy effects, not an automatic
    // player teleport or an unrequested new damage formula.
    float seconds=link.Ability==RunUpgradeKind.Blast?Mathf.Max(8,(30-(level-1)*2)*GetActiveAbilityCooldownMultiplier()):
     link.Ability==RunUpgradeKind.Teleport?Mathf.Max(5,(15-Mathf.Min(4,(level-1)*2))*GetActiveAbilityCooldownMultiplier()):
     (float)NativeMobileMaxCooldown(SourceAbilityId(link.Ability))*GetActiveAbilityCooldownMultiplier();
    _fusionNextAt[id]=_runTime+seconds;
    if(link.Ability==RunUpgradeKind.TimeStop)ApplyTimeStopSynergyRiders(level);
    else if(link.Ability==RunUpgradeKind.Teleport)ApplyTeleportSynergyRiders(level);
    else ApplyActiveAbilitySynergyRiders(link.Ability,level);
    SpawnImpact(sphere.transform.position,new Color(.57f,.91f,.83f));
   }
  }
 }
}
