using System;
using System.Collections.Generic;
namespace EchoSphere.Core
{
    // Correct card contracts in new rulesets; historical catalog values remain readable for legacy runs.
    public struct ArtifactDamageContext
    {
        public int UniqueTypes,StrongestLevel;
        public bool AnotherLevelSeven,NearOtherSphere,TriangleActive,AnyActiveGeometry;
    }
    public static class ArtifactGameplayRules
    {
        private static readonly string[] CorrectedIds = {"sniper_scope","sniper_crown","chain_crown","formation_compass","zero_sphere","unified_mind","lone_bastion","fivefold_resonance","relay_matrix","network_relay","network_coil","geometry_die","mirror_network","singularity_engine","triangle_circuit","resonance_core"};
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
            }
            return m;
        }
        public static float Effect(ArtifactRewardDefinition item,string key,bool modern)
        {
            if(item==null||key==null)return 0;
            if(modern){
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
