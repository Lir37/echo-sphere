using System;
namespace EchoSphere.Core
{
    public enum CampaignBossAttack { Fan, Radial, Cross, Dash, Pulse, Summon, Disable, Shift, NullField }
    public enum CampaignBossEvent { None, Telegraph, Execute }

    [Serializable]
    public sealed class CampaignBossState
    {
        public int Phase, AttackIndex, AttackPhase;
        public float Remaining = 2f;
        public bool Telegraphing;
        // Locked at windup, persisted with the boss, never read from a renderer.
        public float AimX = 1f, AimY;
    }

    public readonly struct CampaignBossPattern
    {
        public readonly CampaignBossAttack Attack;
        public readonly float Windup, Cooldown, Radius, Speed, Damage;
        public readonly int Count;
        public CampaignBossPattern(CampaignBossAttack attack, float windup, float cooldown,
            float radius, float speed, float damage, int count)
        { Attack=attack; Windup=windup; Cooldown=cooldown; Radius=radius; Speed=speed; Damage=damage; Count=count; }
    }

    // Authored campaign starting balance. Legacy BossCombatRules and RNG are unchanged.
    public static class CampaignBossRules
    {
        public const float PhaseThreshold = .5f;
        public static readonly string[] AdditionalTypes={"brood_queen","fleet_carrier"};
        public static int Count=>BossCombatRules.BossTypeCount+AdditionalTypes.Length;
        public static string TypeAt(int index){if(index<0||index>=Count)throw new ArgumentOutOfRangeException(nameof(index));return index<BossCombatRules.BossTypeCount?BossCombatRules.GetTypeForDefeatedCount(index):AdditionalTypes[index-BossCombatRules.BossTypeCount];}
        public static bool IsKnown(string type)=>type=="brood_queen"||type=="fleet_carrier"||BossCombatRules.IsKnownBossType(type);
        public static string ForRegion(string type,EchoRegionId region)=>region==EchoRegionId.FracturedBastion&&type=="summoner"?"brood_queen":region==EchoRegionId.SpectralRift&&type=="null"?"fleet_carrier":type;
        public static string RepeatType(int index)=>index%10==8?"brood_queen":index%10==9?"fleet_carrier":BossCombatRules.GetTypeForDefeatedCount(index%10);
        public static string Description(string type,bool ru)=>type=="brood_queen"?(ru?"Выпускает из панциря малых насекомых. Раненая матка ускоряет выводок; между выпусками атакует сама.":"Releases small insects from its carapace. A wounded queen strengthens its brood and attacks between releases."):(ru?"Открывает бортовые отсеки и выпускает боевые дроны. Подготовку выпуска видно заранее.":"Opens its side bays and releases combat drones. Its launch sequence is clearly signalled.");
        public static string Name(string type,bool ru)
        {
            switch(type)
            {
                case "brood_queen":return ru?"МАТКА РОЯ":"BROOD QUEEN";
                case "fleet_carrier":return ru?"НОСИТЕЛЬ":"FLEET CARRIER";
                case "shooter":return ru?"ПИКОВИК ПУСТОТЫ":"VOID LANCER";
                case "charger":return ru?"ПРОРЫВАТЕЛЬ":"BREACHER";
                case "summoner":return ru?"РОЕВОЙ РАЗУМ":"BROOD MIND";
                case "aura":return ru?"ТИТАН АУРЫ":"AURA TITAN";
                case "conductor":return ru?"ПРОВОДНИК":"CONDUCTOR";
                case "architect":return ru?"АРХИТЕКТОР":"ARCHITECT";
                case "null":return ru?"НУЛЬ":"NULL";
                case "stella_warden":return ru?"СТРАЖ СТЕЛЛЫ":"STELLA WARDEN";
                default:throw new ArgumentException("Unknown boss",nameof(type));
            }
        }
        public static CampaignBossPattern Pattern(string type, int index, int phase)
        {
            if(!IsKnown(type)) throw new ArgumentException("Unknown boss",nameof(type));
            if(index<0 || index>2) throw new ArgumentOutOfRangeException(nameof(index));
            CampaignBossAttack attack;
            switch(type)
            {
                case "brood_queen": attack=index==0?CampaignBossAttack.Summon:index==1?CampaignBossAttack.Pulse:CampaignBossAttack.Fan;break;
                case "fleet_carrier": attack=index==0?CampaignBossAttack.Summon:index==1?CampaignBossAttack.Fan:CampaignBossAttack.Cross;break;
                case "shooter": attack=index==0?CampaignBossAttack.Fan:index==1?CampaignBossAttack.Radial:CampaignBossAttack.Cross;break;
                case "charger": attack=index==0?CampaignBossAttack.Dash:index==1?CampaignBossAttack.Radial:CampaignBossAttack.Pulse;break;
                case "summoner": attack=index==0?CampaignBossAttack.Summon:index==1?CampaignBossAttack.Fan:CampaignBossAttack.Radial;break;
                case "aura": attack=index==0?CampaignBossAttack.Pulse:index==1?CampaignBossAttack.Radial:CampaignBossAttack.Fan;break;
                case "conductor": attack=index==0?CampaignBossAttack.Fan:index==1?CampaignBossAttack.Disable:CampaignBossAttack.Pulse;break;
                case "architect": attack=index==0?CampaignBossAttack.Radial:index==1?CampaignBossAttack.Shift:CampaignBossAttack.Disable;break;
                case "null": attack=index==0?CampaignBossAttack.Fan:index==1?CampaignBossAttack.NullField:CampaignBossAttack.Pulse;break;
                default: attack=index==0?CampaignBossAttack.Fan:index==1?CampaignBossAttack.Summon:CampaignBossAttack.Pulse;break;
            }
            int p=phase>0?1:0;
            float windup=attack==CampaignBossAttack.Summon?.9f:attack==CampaignBossAttack.Dash?.8f:.85f;
            float radius=attack==CampaignBossAttack.Summon?60f:attack==CampaignBossAttack.Pulse?(type=="aura"?220f:180f):attack==CampaignBossAttack.Disable?300f:attack==CampaignBossAttack.NullField?240f:attack==CampaignBossAttack.Dash?300f:360f;
            int count=attack==CampaignBossAttack.Summon?(type=="summoner"?3:2):attack==CampaignBossAttack.Radial?8:attack==CampaignBossAttack.Cross?4:attack==CampaignBossAttack.Fan?5:1;
            if(p>0){radius*=1.15f;if(attack==CampaignBossAttack.Summon)count++;else if(attack==CampaignBossAttack.Fan||attack==CampaignBossAttack.Radial)count+=2;else if(attack==CampaignBossAttack.Disable)count=2;}
            if(attack==CampaignBossAttack.Summon){radius=60f;if(type=="brood_queen"||type=="fleet_carrier"){radius=type=="brood_queen"?13f:12f;count=type=="brood_queen"?(p>0?5:3):(p>0?4:2);}}
            if(attack==CampaignBossAttack.Dash)radius=p>0?360f:300f;
            float speed=attack==CampaignBossAttack.Dash?(p>0?720f:600f):(type=="shooter"?205f:175f)*(p>0?1.1f:1f);
            return new CampaignBossPattern(attack,windup,(attack==CampaignBossAttack.Summon?4.5f:3.3f)*(p>0?.8f:1f),radius,speed,type=="null"?28f:22f,count);
        }

        // One event per simulation step. Telegraph always precedes execution, including after restore.
        public static CampaignBossEvent Advance(string type, CampaignBossState state, float dt, float hpFraction)
        {
            if(state==null)throw new ArgumentNullException(nameof(state));
            if(hpFraction<=PhaseThreshold)state.Phase=1; // Healing cannot undo a phase.
            if(float.IsNaN(dt)||float.IsInfinity(dt)||dt<=0)return CampaignBossEvent.None;
            state.Remaining-=dt;
            if(state.Remaining>0)return CampaignBossEvent.None;
            var pattern=Pattern(type,state.AttackIndex,state.Phase);
            if(!state.Telegraphing){state.Telegraphing=true;state.AttackPhase=state.Phase;state.Remaining=pattern.Windup;return CampaignBossEvent.Telegraph;}
            state.Telegraphing=false;state.Remaining=pattern.Cooldown;
            state.AttackIndex=(state.AttackIndex+1)%3;
            return CampaignBossEvent.Execute;
        }
    }
}
