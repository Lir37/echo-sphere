using EchoSphere.Core;using UnityEngine;
namespace EchoSphere.Runtime {
 public sealed partial class EnemyAgent2D {
  private int _regionalIdentity,_summonParentId,_regionalSummonCycle,_fleetCommanderId,_energyTargetId;
  private float _crystalOpenTimer,_energyWindup,_energyCooldown=1.4f,_energyFlash;
  internal int RegionalIdentity=>_regionalIdentity;internal int SummonParentId=>_summonParentId;
  internal bool CrystalClosed=>_campaignRegionStyle==3&&_regionalWindup>0;
  internal float CrystalOpen=>_campaignRegionStyle==3?_crystalOpenTimer:0;
  internal float RegionalIncomingMultiplier=>CrystalClosed?RegionalCombatTuning.CrystalClosedMultiplier:CrystalOpen>0?RegionalCombatTuning.CrystalOpenMultiplier:1;
  internal int EnergyTargetId=>_energyTargetId;internal float EnergyWarning=>_energyWindup;internal float EnergyFlash=>_energyFlash;
  internal void EnsureRegionalIdentity(){if(_regionalIdentity==0&&_runtime!=null)_regionalIdentity=_runtime.AllocateRegionalIdentity();}
  internal int AdvanceSummonCycle()=>_regionalSummonCycle++;
  internal void SetSummonParent(EnemyAgent2D parent,string role){parent.EnsureRegionalIdentity();_summonParentId=parent.RegionalIdentity;_role=role;}
  internal bool PrepareFleetVolley(EnemyAgent2D commander){if(IsBoss||_campaignRegionStyle!=1||_regionalWindup>0||!IsAlive)return false;commander.EnsureRegionalIdentity();_fleetCommanderId=commander.RegionalIdentity;_regionalAim=commander._regionalAim;_regionalWindup=commander._regionalWindup;return true;}
  private void TickRegionalSpecials(float dt){
   EnsureRegionalIdentity();_crystalOpenTimer=Mathf.Max(0,_crystalOpenTimer-dt);_energyFlash=Mathf.Max(0,_energyFlash-dt);
   if(_campaignRegionStyle!=4||(_role!="ranged"&&_role!="anchor"&&_role!="disruptor"))return;
   var target=_runtime.FindRegionalIdentity(_energyTargetId);
   if(_energyWindup>0){if(target==null||!target.IsAlive){_energyWindup=0;_energyTargetId=0;_energyCooldown=RegionalCombatTuning.EnergyInterval;return;}_energyWindup=Mathf.Max(0,_energyWindup-dt);if(_energyWindup<=0){_energyFlash=.22f;_energyCooldown=RegionalCombatTuning.EnergyInterval;var a=(Vector2)transform.position;var b=(Vector2)target.transform.position;var ab=b-a;float t=ab.sqrMagnitude<.000001f?0:Mathf.Clamp01(Vector2.Dot(_runtime.PlayerPosition-a,ab)/ab.sqrMagnitude);if(Vector2.Distance(_runtime.PlayerPosition,a+ab*t)<=RegionalCombatTuning.EnergyThickness*SpherePlacementRules.SourcePixelsToWorld)_runtime.TryDamagePlayer(RegionalCombatTuning.EnergyDamage*_runtime.CampaignEnemyDamageMultiplier);}return;}
   _energyCooldown-=dt;if(_energyCooldown>0||_energyFlash>0)return;
   target=_runtime.FindRegionalEnergyPartner(this);if(target==null){_energyCooldown=RegionalCombatTuning.EnergyPartnerRetry;return;}_energyTargetId=target.RegionalIdentity;_energyWindup=RegionalCombatTuning.EnergyWindup;
  }
 }
 public sealed partial class EchoSphereRuntime {
  private int _regionalEnemySequence;
  internal int AllocateRegionalIdentity()=>++_regionalEnemySequence;
  internal EnemyAgent2D FindRegionalIdentity(int id){if(id==0)return null;foreach(var e in _enemies)if(e!=null&&e.IsAlive&&e.RegionalIdentity==id)return e;return null;}
  internal bool RegionalIdentityAlive(int id)=>FindRegionalIdentity(id)!=null;
  internal void PrepareRegionalFleetVolley(EnemyAgent2D leader){int count=0;foreach(var e in _enemies)if(e!=null&&e!=leader&&Vector2.Distance(e.transform.position,leader.transform.position)<=RegionalCombatTuning.FleetCommandRange*SpherePlacementRules.SourcePixelsToWorld&&e.PrepareFleetVolley(leader)&&++count>=RegionalCombatTuning.FleetGroupSize)break;}
  internal EnemyAgent2D FindRegionalEnergyPartner(EnemyAgent2D owner){int active=0;foreach(var e in _enemies)if(e!=null&&e.IsAlive&&(e.EnergyWarning>0||e.EnergyFlash>0))active++;if(active>=RegionalCombatTuning.EnergyLinkCap)return null;EnemyAgent2D best=null;float distance=RegionalCombatTuning.EnergyRange*SpherePlacementRules.SourcePixelsToWorld;foreach(var e in _enemies){if(e==null||e==owner||!e.IsAlive||e.IsBoss||e.CampaignRegionStyle!=4)continue;e.EnsureRegionalIdentity();if(e.RegionalIdentity<=owner.RegionalIdentity)continue;float d=Vector2.Distance(e.transform.position,owner.transform.position);if(d<distance){distance=d;best=e;}}return best;}
  internal int ReleaseRegionalBrood(EnemyAgent2D parent,int requested,int limit){
   if(parent==null||!parent.IsAlive||!UsesCampaignCombat||IsGameplayPaused)return 0;parent.EnsureRegionalIdentity();int local=0,total=0;foreach(var e in _enemies)if(e!=null&&e.IsAlive&&e.SummonParentId!=0){total++;if(e.SummonParentId==parent.RegionalIdentity)local++;}
   int count=Mathf.Max(0,Mathf.Min(requested,Mathf.Min(limit-local,RegionalCombatTuning.GlobalBroodCap-total))),cycle=parent.AdvanceSummonCycle();ColorUtility.TryParseHtmlString(parent.SourceColorHtml,out var tint);
   for(int i=0;i<count;i++){float a=cycle*1.618f+i*Mathf.PI*2/Mathf.Max(1,count);var pos=(Vector2)parent.transform.position+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*parent.SourceRadius*RegionalCombatTuning.SummonBodyOffset*SpherePlacementRules.SourcePixelsToWorld;var child=SpawnBossMinion(pos,Mathf.Clamp(parent.MaxHp*RegionalCombatTuning.SummonHpShare,RegionalCombatTuning.SummonMinHp*CampaignEnemyHealthMultiplier,RegionalCombatTuning.SummonMaxHp*CampaignEnemyHealthMultiplier),RegionalCombatTuning.SummonSpeed*SpherePlacementRules.SourcePixelsToWorld,2,"skitter",tint);if(child!=null){child.SetCampaignRegion((EchoRegionId)parent.CampaignRegionStyle);child.SetSummonParent(parent,parent.CampaignRegionStyle==1?(i%2==0?"swarmer":"ranged"):"swarmer");}}
   return count;
  }
 }
}
