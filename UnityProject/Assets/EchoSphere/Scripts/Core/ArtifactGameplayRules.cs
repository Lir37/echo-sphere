using System.Collections.Generic;
namespace EchoSphere.Core
{
    // Correct card contracts in new rulesets; historical catalog values remain readable for legacy runs.
    public static class ArtifactGameplayRules
    {
        public static float Effect(ArtifactRewardDefinition item,string key,bool modern)
        {
            if(item==null||key==null)return 0;
            if(modern){
                if(item.Id=="sniper_scope")return key=="sniperDamage"?.10f:0f;
                if(item.Id=="sniper_crown")return key=="sniperRadius"?.10f:0f;
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
