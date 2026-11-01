using UnityEngine;
using EchoSphere.Core;
namespace EchoSphere.Runtime
{
    public sealed partial class BossCombatAgent2D
    {
        private CampaignBossState _campaign;
        private float _campaignDashRemaining, _campaignDashSpeed;
        public CampaignBossState CampaignState => _runtime != null && _runtime.UsesCampaignCombat ? _campaign : null;
        public CampaignBossPattern CampaignPattern => CampaignBossRules.Pattern(_bossType,_campaign?.AttackIndex??0,_campaign==null?0:_campaign.Telegraphing?_campaign.AttackPhase:_campaign.Phase);

        private void TickCampaignBoss(float dt)
        {
            if(_campaign==null)_campaign=new CampaignBossState();
            var player=(Vector2)_player.position;
            var oldPhase=_campaign.Phase;
            int index=_campaign.AttackIndex;
            // Freeze current attack parameters for the event, even when index advances.
            var signal=CampaignBossRules.Advance(_bossType,_campaign,dt,_agent.CurrentHp/Mathf.Max(1f,_agent.MaxHp));
            var pattern=CampaignBossRules.Pattern(_bossType,index,_campaign.AttackPhase);
            if(oldPhase!=_campaign.Phase)_runtime.NotifyCampaignBossPhase();
            if(signal==CampaignBossEvent.Telegraph)
            {
                var aim=player-(Vector2)transform.position;
                aim=aim.sqrMagnitude>.0001f?aim.normalized:Vector2.right;
                _campaign.AimX=aim.x;_campaign.AimY=aim.y;
            }
            if(signal==CampaignBossEvent.Execute)ExecuteCampaignAttack(pattern);
            var direction=new Vector2(_campaign.AimX,_campaign.AimY);
            if(_campaignDashRemaining>0)
            {
                float step=Mathf.Min(dt,_campaignDashRemaining);
                transform.position+=(Vector3)(direction*BossCombatRules.ToWorldUnits(_campaignDashSpeed)*step);
                _campaignDashRemaining=Mathf.Max(0,_campaignDashRemaining-dt);
            }
            else if(!_campaign.Telegraphing && signal!=CampaignBossEvent.Execute)
                transform.position=Vector2.MoveTowards(transform.position,player,_moveSpeed*_agent.MovementSlowMultiplier*(_campaign.Phase>0?1.12f:1f)*dt);
            _runtime.RefreshEnemyQuery(_agent);
            if(Vector2.Distance(transform.position,player)<_agent.SourceRadius*SpherePlacementRules.SourcePixelsToWorld+BossCombatRules.PlayerRadiusWorld)
                _runtime.TryDamagePlayer(_damage*(_campaignDashRemaining>0?1.5f:1f));
            _rotation+=dt;
            _runtime.TickSourceEnemyProjectiles(_agent,dt);
        }

        private void ExecuteCampaignAttack(CampaignBossPattern p)
        {
            var origin=(Vector2)transform.position;
            var aim=new Vector2(_campaign.AimX,_campaign.AimY);
            float angle=Mathf.Atan2(aim.y,aim.x),scale=SpherePlacementRules.SourcePixelsToWorld;
            switch(p.Attack)
            {
                case CampaignBossAttack.Fan:
                case CampaignBossAttack.Radial:
                case CampaignBossAttack.Cross:
                    for(int i=0;i<p.Count;i++)
                    {
                        float a=p.Attack==CampaignBossAttack.Fan?angle+(i-(p.Count-1)*.5f)*.20f:angle+i*Mathf.PI*2/p.Count;
                        _runtime.SpawnBossProjectile(origin,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*BossCombatRules.ToWorldUnits(p.Speed),p.Damage,7f,_bossType,_color,_agent);
                    }
                    break;
                case CampaignBossAttack.Summon:
                    for(int i=0;i<p.Count;i++)
                    {
                        float a=angle+i*Mathf.PI*2/p.Count;
                        var child=_runtime.SpawnBossMinion(origin+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*60f*scale,20f+_wave*4f,BossCombatRules.ToWorldUnits(90f),2,_bossType=="stella_warden"?"moth":"wisp",_color);
                        if(child!=null&&_agent.CampaignRegionStyle>=0)child.SetCampaignRegion((EchoRegionId)_agent.CampaignRegionStyle);
                    }
                    break;
                case CampaignBossAttack.Dash: _campaignDashRemaining=.5f;_campaignDashSpeed=p.Speed;break;
                case CampaignBossAttack.Pulse:
                    if(Vector2.Distance(origin,_player.position)<=p.Radius*scale)_runtime.TryDamagePlayer(_damage*1.2f);
                    _runtime.SpawnImpact(origin,_color);
                    break;
                case CampaignBossAttack.Disable:
                    for(int i=0;i<p.Count;i++)_runtime.TryNullifyNearestSphere(origin,p.Radius,1.6f);
                    break;
                case CampaignBossAttack.Shift:
                    transform.position=(Vector2)_player.position-aim*p.Radius*scale;
                    _runtime.DrainResonance(12f);
                    break;
                case CampaignBossAttack.NullField:
                    if(Vector2.Distance(origin,_player.position)<=p.Radius*scale)
                    { _runtime.DrainResonance(25f); _runtime.DelayAllSphereAttacks(.45f); }
                    break;
            }
        }
    }
}
