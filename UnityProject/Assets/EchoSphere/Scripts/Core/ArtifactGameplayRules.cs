using System;
using System.Collections.Generic;
namespace EchoSphere.Core
{
    // Correct card contracts in new rulesets; historical catalog values remain readable for legacy runs.
    public struct ArtifactDamageContext
    {
        public int UniqueTypes,StrongestLevel;
        public bool AnotherLevelSeven,NearOtherSphere,TriangleActive,AnyActiveGeometry,HasActiveLink;
    }
    public static class ArtifactGameplayRules
    {
        private static readonly string[] CorrectedIds = {"sniper_scope","sniper_crown","chain_crown","formation_compass","zero_sphere","unified_mind","lone_bastion","fivefold_resonance","relay_matrix","network_relay","network_coil","geometry_die","mirror_network","singularity_engine","triangle_circuit","resonance_core","chain_battery","shotgun_shell","aura_mist","folded_core","overclock","quantum_fold","void_star","mirror_dust","signal_knot","echo_thread","triangle_engine","ring_engine","lattice_engine","fractal_engine","lattice_chip","fractal_seed"};
        private static readonly string[] DescriptionOnlyIds = {"vampire_ring","predator_claw","foresight_eye","mirror","void_mark","void_lantern","gravity_hook","pulse_driver"};
        public static string DescriptionOverride(string id,bool ru)
        {
            switch(id){
                case "vampire_ring":return ru?"+3% вампиризма. Каждое убийство восстанавливает 5 HP.":"+3% lifesteal. Each kill restores 5 HP.";
                case "predator_claw":return ru?"+8% урона сфер. Каждое пятое попадание отдельной сферы наносит двойной урон и считается критическим.":"+8% Sphere damage. Every fifth hit by each Sphere deals double damage and counts as a critical hit.";
                case "foresight_eye":return ru?"+10% шанса крита, +8% урона Снайпера. Указывает направление к боссу.":"+10% critical chance and +8% Sniper damage. Shows the direction to a boss.";
                case "mirror":return ru?"При атаке на Core: 20% шанс контратаки по ближайшему врагу. Урон контратаки — 125% урона атаки.":"When the Core is attacked: 20% chance to counterattack a nearby enemy for 125% of the attack damage.";
                case "void_mark":return ru?"+10% урона Пустотной по целям с 35% HP или меньше.":"+10% Void damage against targets at 35% HP or lower.";
                case "void_lantern":return ru?"+20% урона Пустотной по целям с 35% HP или меньше.":"+20% Void damage against targets at 35% HP or lower.";
                case "gravity_hook":return ru?"+10% силы притяжения Гравитационной. Пауза между её импульсами короче на 8%.":"+10% Gravity pull strength. Its pulse interval is 8% shorter.";
                case "pulse_driver":return ru?"Пауза между волнами Импульсной короче на 10%.":"Pulse wave interval is 10% shorter.";
                case "mirror_dust":return ru?"+8 п.п. к шансу контратаки по ближайшему врагу при атаке на Core. Урон контратаки — 125% урона атаки.":"+8 percentage points of counterattack chance when the Core is attacked. Counters hit a nearby enemy for 125% of the attack damage.";
                case "overclock":return ru?"−12% задержки атак сфер, но −5% их урона.":"−12% Sphere attack delay, but −5% Sphere damage.";
                case "void_star":return ru?"Ветка Пустотной с добиванием: +8 п.п. к шансу срабатывания, до лимита 45%.":"Void's execution branch gains 8 percentage points of execution chance, up to the 45% cap.";
                default:return null;
            }
        }
        public static IReadOnlyList<string> CorrectedArtifactIds=>CorrectedIds;
        public static bool IsCorrected(string id){foreach(var value in CorrectedIds)if(value==id)return true;return false;}
        public static float ConditionalDamageMultiplier(IList<string> owned,ArtifactDamageContext c)
        {
            float m=1;if(owned==null)return m;
            foreach(var id in owned)switch(id){
                case "unified_mind":m*=1+.03f*Math.Max(0,Math.Min(7,c.StrongestLevel));break;
                case "lone_bastion":if(c.UniqueTypes==1)m*=1.30f;break;
                case "fivefold_resonance":m*=1+.04f*Math.Max(0,Math.Min(5,c.UniqueTypes));break;
                case "relay_matrix":if(c.AnotherLevelSeven)m*=1.10f;break;
                case "network_relay":if(c.UniqueTypes>=2)m*=1.05f;break;
                case "network_coil":if(c.UniqueTypes>=2)m*=1.08f;break;
                case "geometry_die":if(c.AnyActiveGeometry)m*=1.08f;break;
                case "mirror_network":if(c.UniqueTypes>=2)m*=1.12f;break;
                case "singularity_engine":if(c.UniqueTypes>=1&&c.UniqueTypes<=2)m*=1.18f;break;
                case "triangle_circuit":if(c.TriangleActive)m*=1.15f;break;
                case "resonance_core":if(c.NearOtherSphere)m*=1.12f;break;
                case "echo_thread":if(c.HasActiveLink)m*=1.05f;break;
            }
            return m;
        }
        public static float FormationMultiplier(IList<string> owned,SphereNetworkFormation type,bool modern)
        {
            if(!modern||owned==null)return 1;float multiplier=1;
            if(type==SphereNetworkFormation.Triangle&&owned.Contains("triangle_engine"))multiplier*=1.12f;
            if(type==SphereNetworkFormation.Ring&&owned.Contains("ring_engine"))multiplier*=1.12f;
            if(type==SphereNetworkFormation.Lattice){if(owned.Contains("lattice_engine"))multiplier*=1.12f;if(owned.Contains("lattice_chip"))multiplier*=1.10f;}
            if(type==SphereNetworkFormation.Fractal){if(owned.Contains("fractal_engine"))multiplier*=1.12f;if(owned.Contains("fractal_seed"))multiplier*=1.10f;}
            return multiplier;
        }
        public static float RecoverySpeedMultiplier(IList<string> owned,bool modern)=>modern&&owned!=null&&owned.Contains("signal_knot")?1.05f:1f;
        public static float RecoveryDurationMultiplier(IList<string> owned,bool modern)=>1f/RecoverySpeedMultiplier(owned,modern);
        public static float CounterattackChance(IList<string> owned,bool modern)=>(owned!=null&&owned.Contains("mirror")?.20f:0)+(modern&&owned!=null&&owned.Contains("mirror_dust")?.08f:0);
        public static float Effect(ArtifactRewardDefinition item,string key,bool modern)
        {
            if(item==null||key==null)return 0;
            if(modern){
                if(item.Id=="mirror_dust"||item.Id=="signal_knot"||item.Id=="echo_thread"||item.Id=="triangle_engine"||item.Id=="ring_engine"||item.Id=="lattice_engine"||item.Id=="fractal_engine"||item.Id=="lattice_chip"||item.Id=="fractal_seed")return 0f;
                if(item.Id=="chain_battery")return key=="chainDamage"?.10f:0f;
                if(item.Id=="shotgun_shell")return key=="shotgunDamage"?.10f:0f;
                if(item.Id=="aura_mist")return key=="auraRadius"?.10f:0f;
                if(item.Id=="folded_core")return key=="sphereDamage"?.08f:0f;
                if(item.Id=="overclock")return key=="sphereDelay"?-.12f:key=="sphereDamage"?-.05f:0f;
                if(item.Id=="quantum_fold"||item.Id=="void_star")return 0f;
                if(item.Id=="sniper_scope")return key=="sniperDamage"?.10f:0f;
                if(item.Id=="sniper_crown")return key=="sniperRadius"?.10f:0f;
                if(item.Id=="unified_mind"||item.Id=="lone_bastion"||item.Id=="fivefold_resonance"||item.Id=="relay_matrix"||item.Id=="network_relay"||item.Id=="network_coil"||item.Id=="geometry_die"||item.Id=="mirror_network"||item.Id=="singularity_engine"||item.Id=="triangle_circuit"||item.Id=="resonance_core")return 0f;
                if(item.Id=="chain_crown")return 0f; // The promised extra jump is a discrete behavior.
            }
            return item.GetEffect(key);
        }
        public static float EffectTotal(IList<string> owned,string key,bool modern)
        {
            float value=0;if(owned==null)return value;
            for(int i=0;i<owned.Count;i++)value+=Effect(ArtifactRewardRules.Get(owned[i]),key,modern);
            return value;
        }
        public static int ExtraChainJumps(IList<string> owned,bool modern)=>modern&&owned!=null&&owned.Contains("chain_crown")?1:0;
    }
}
