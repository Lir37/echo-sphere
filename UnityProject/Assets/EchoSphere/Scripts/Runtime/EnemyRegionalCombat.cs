using EchoSphere.Core;
using UnityEngine;
namespace EchoSphere.Runtime {
 public sealed partial class EnemyAgent2D {
  private float _regionalCooldown=1.2f,_regionalWindup,_regionalRecovery,_regionalClock;
  private Vector2 _regionalAim;
  internal float RegionalAttackPose=>_regionalWindup>0?RegionalEnemyRules.Telegraph(_regionalWindup,RegionalEnemyRules.Get(_campaignRegionStyle,_role).Windup):_regionalRecovery/.28f;
  internal bool RegionalRanged=>_campaignRegionStyle>=0&&RegionalEnemyRules.Get(_campaignRegionStyle,_role).Range>0;
  internal float RegionalWindup=>_regionalWindup;
  internal Vector2 RegionalAim=>_regionalAim;
  private Vector2 RegionalMovement(float dt,Vector2 toward,float distance){
   var p=RegionalEnemyRules.Get(_campaignRegionStyle,_role);_regionalClock+=dt;
   distance/=SpherePlacementRules.SourcePixelsToWorld;
   float radial=RegionalEnemyRules.Radial(p,distance),side=((int)SourceRadius%2==0?1:-1);
   Vector2 tangent=new Vector2(-toward.y,toward.x)*side;
   if(_regionalWindup>0)return Vector2.zero;
   if(p.Range<=0){
    if(_campaignRegionStyle==1){float cadence=.5f+.5f*Mathf.Sin(_regionalClock*2.5f);float drift=_role=="anchor"?.20f:_role=="charger"?.08f:.65f;return (toward+tangent*drift).normalized*(_role=="charger"?.65f+cadence*.70f:1);}
    if(_campaignRegionStyle==3){ // Crystals advance in deliberate diagonal steps.
     float step=Mathf.Sin(_regionalClock*3.2f);return (toward+tangent*(step>0?.50f:-.50f)).normalized*(step>-.55f?1:.18f);}
    if(_campaignRegionStyle==4)return (toward+tangent*Mathf.Sin(_regionalClock*3.5f)*1.15f).normalized;
    if(p.Motion==RegionalMotion.Lunge){var aim=(_regionalAim-(Vector2)transform.position).normalized;return (_regionalRecovery>0?Vector2.ClampMagnitude(aim+toward*.3f,1)*2.5f:toward*.72f);}
    if(p.Motion==RegionalMotion.Flank)return (toward+tangent*(Mathf.Sin(_regionalClock*.75f)>0?.85f:-.85f)).normalized*(_regionalRecovery>0?1.8f:1);return toward;}
   float orbit=p.Motion==RegionalMotion.Orbit?.65f:p.Motion==RegionalMotion.Skirmish?.4f:p.Motion==RegionalMotion.Flank?.85f:0;
   if(distance>p.Range*1.7f)orbit*=.3f;
   return Vector2.ClampMagnitude(toward*radial+tangent*orbit,1);
  }
  private void RegionalActionFeedback(){if(_campaignRegionStyle>0){_regionalRecovery=.28f;if(_player!=null)_regionalAim=_player.position;}}
  private void TickRegionalAttack(float dt,Vector2 target,float distance){
   var p=RegionalEnemyRules.Get(_campaignRegionStyle,_role);
   _regionalRecovery=Mathf.Max(0,_regionalRecovery-dt);
   if(p.Interval<=0)return;
   if(_regionalWindup>0){_regionalWindup=Mathf.Max(0,_regionalWindup-dt);if(_regionalWindup>0)return;
    _regionalRecovery=.28f;_regionalCooldown=p.Interval;
    if(p.Range>0){
     var direction=(_regionalAim-(Vector2)transform.position).normalized;if(direction.sqrMagnitude<.01f)direction=Vector2.right;
     for(int i=0;i<p.Pellets;i++){float angle=(i-(p.Pellets-1)*.5f)*.16f;var shot=new Vector2(direction.x*Mathf.Cos(angle)-direction.y*Mathf.Sin(angle),direction.x*Mathf.Sin(angle)+direction.y*Mathf.Cos(angle));
      _runtime.SpawnBossProjectile((Vector2)transform.position+shot*SourceRadius*SpherePlacementRules.SourcePixelsToWorld,shot*p.Speed*SpherePlacementRules.SourcePixelsToWorld,(_role=="sniper"?10:6)/Mathf.Sqrt(p.Pellets),p.Shot=="spore"?8:5,"regional_"+p.Shot,new Color(.6f,.85f,1),this);}
     SourceAudio2D.Play("shoot");
    }else if(p.Shot=="tentacle"&&_player!=null){
     float unit=SpherePlacementRules.SourcePixelsToWorld;
     if(Vector2.Distance(transform.position,_player.position)<SourceRadius*3.2f*unit&&Vector2.Distance(_player.position,_regionalAim)<SourceRadius*1.5f*unit)_runtime.TryDamagePlayer(_damage*.75f);
    }return;
   }
   _regionalCooldown-=dt;
   float limit=(p.Range>0?p.Range:145)*SpherePlacementRules.SourcePixelsToWorld;
   if(_regionalCooldown<=0&&distance<limit){_regionalAim=target;_regionalWindup=p.Windup;}
  }
 }
}
