using System.Collections.Generic;
using EchoSphere.Core;
namespace EchoSphere.Runtime {
 public sealed partial class EchoSphereRuntime {
  private Dictionary<string,int> _artifactProtocolHitCounters=new Dictionary<string,int>();
  private bool HasCampaignProtocolLink(SphereAttackAgent owner){if(_networkState==null)return false;int index=GetSphereIndex(owner);foreach(var link in _networkState.Links)if(link.A==index||link.B==index)return true;return false;}
  public void ApplyCampaignArtifactProtocolHit(SphereAttackAgent owner,EnemyAgent2D target,float damage,bool allowProcs){
   if(_runEconomy==null||owner==null||target==null||damage<=0||!allowProcs)return;
   for(int protocolIndex=0;protocolIndex<ArtifactProtocolRules.Protocols.Count;protocolIndex++){
    var protocol=ArtifactProtocolRules.Protocols[protocolIndex];
    if(!IsArtifactProtocolCurrentlyActive(protocol))continue;
    string id=protocol.Id;
    if(id=="void_horizon_protocol"&&(owner.Type!=SphereId.Void||!target.IsElite&&!target.IsBoss))continue;
    if(id=="hunter_doctrine_protocol"&&(owner.Type!=SphereId.Sniper||!target.IsElite&&!target.IsBoss))continue;
    if(id=="status_circuit_protocol"&&owner.Type!=SphereId.Prism)continue;
    if(id=="pulse_engineering_protocol"&&owner.Type!=SphereId.Pulse)continue;
    if(id=="prism_dominion_protocol"&&owner.Type!=SphereId.Prism&&owner.Type!=SphereId.Orbital)continue;
    if(id=="triangle_resonance"&&!SphereFormsTriangleWithinSourceRange(owner))continue;
    if(id=="sphere_relay"&&!HasCampaignProtocolLink(owner))continue;
    int cadence=ArtifactProtocolRules.HitCadence(id);if(cadence<=0)continue;
    _artifactProtocolHitCounters.TryGetValue(id,out int hits);hits++;
    _artifactProtocolHitCounters[id]=hits>=cadence?0:hits;if(hits<cadence)continue;
    switch(id){
     case "triangle_resonance":AddResonanceCharge(5);break;
     case "geometry_circuit":AddResonanceCharge(8);break;
     case "sphere_relay":case "prism_dominion_protocol":owner.AccelerateNextAttack(.2f);break;
     case "critical_echo":case "core_forge_protocol":HealPlayer(2);break;
     case "void_horizon_protocol":case "status_circuit_protocol":if(target.IsAlive)target.ApplySlow(1,.7f);break;
     case "temporal_fold_protocol":if(target.IsAlive)target.ApplyFreeze(.25f);break;
     case "hunter_doctrine_protocol":if(target.IsAlive)target.ReceiveArtifactProtocolDamage(damage*.25f,owner);break;
     case "pulse_engineering_protocol":if(target.IsAlive)target.KnockBackFrom(owner.transform.position,.2f);break;
     case "network_legacy_protocol":GrantPlayerShieldCharge(1);break;
    }
   }
   foreach(var set in CampaignArtifactSets.All){
    if(!set.Complete(_ownedArtifacts))continue;
    if(set.Id=="glass_bastion"&&(_playerCore==null||_playerCore.CurrentHp>=_playerCore.MaxHp*.5f))continue;
    if(set.Id=="frozen_optics"&&owner.Type!=SphereId.Sniper)continue;
    if(set.Id=="wave_rotor"&&owner.Type!=SphereId.Pulse)continue;
    if(set.Id=="chronicle"&&_timeStopTimer<=0&&_teleportDamageBuffTimer<=0)continue;
    string key=set.CounterId;_artifactProtocolHitCounters.TryGetValue(key,out int hits);hits++;
    _artifactProtocolHitCounters[key]=hits>=set.Cadence?0:hits;if(hits<set.Cadence)continue;
    switch(set.Effect){case "shield":GrantPlayerShieldCharge(1);break;case "advance":owner.AccelerateNextAttack(.2f);break;case "freeze":if(target.IsAlive)target.ApplyFreeze(.25f);break;case "knockback":if(target.IsAlive)target.KnockBackFrom(owner.transform.position,.2f);break;case "heal":HealPlayer(3);break;case "resonance":AddResonanceCharge(5);break;case "strike":if(target.IsAlive)target.ReceiveArtifactProtocolDamage(damage*.15f,owner);break;}
   }
  }
 }
}
