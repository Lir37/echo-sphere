using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        public float ResolveSourceVoidHit(EnemyAgent2D target, float damage, SphereAttackAgent owner = null)
        {
            var level = GetSphereProgressionLevel(SphereId.Void); var branch = GetSphereBranch(SphereId.Void); var final = GetSphereFinal(SphereId.Void);
            var fraction = target.HpFraction;
            if (level >= 1 && fraction <= .50f) damage *= 1.20f;
            if (fraction <= .20f) damage *= 2.25f;
            if (fraction <= .35f && HasArtifact("void_mark")) damage *= 1.10f;
            if (fraction <= .35f && HasArtifact("void_lantern")) damage *= 1.20f;
            if (branch == "void_hunger") damage *= SphereEvolutionCombatRules.GetVoidHungerDamageMultiplier(level, final, fraction);
            if (branch == "void_reaper" && fraction <= .25f) damage *= final == "void_reaper_final_2" ? 1.25f : 1.12f;
            if (branch == "void_execution")
            {
                var chance = SphereEvolutionCombatRules.GetVoidExecutionChance(level, final) + (HasArtifact("void_star") ? .08f : 0f);
                if (fraction <= SphereEvolutionCombatRules.GetVoidExecutionThreshold(level, final) && RollCombatChance(Mathf.Min(.45f, chance)))
                    damage = target.IsBoss ? damage * 2.5f : Mathf.Max(damage, target.CurrentHp + 1f);
            }
            else if (level >= 2 && fraction <= .20f && RollCombatChance(.10f)) damage = target.IsBoss ? damage * 1.35f : Mathf.Max(damage, target.CurrentHp + 1f);
            var index = GetSphereIndex(owner);
            damage *= 1f + .10f * GetFormationBonus(SphereNetworkFormation.Line, index);
            damage *= 1f + .15f * GetFormationBonus(SphereNetworkFormation.Fractal, index);
            return damage;
        }
        public void ApplySourceVoidReaperKill(EnemyAgent2D target, SphereAttackAgent owner, float damage)
        {
            if (owner == null || owner.Type != SphereId.Void || GetSphereBranch(SphereId.Void) != "void_reaper") return;
            var level = owner.ProgressionLevel; var final = GetSphereFinal(SphereId.Void);
            var power = string.IsNullOrEmpty(final) ? SphereEvolutionCombatRules.GetChainBranchPower(level) : 1f;
            HealPlayer(SphereEvolutionCombatRules.GetVoidReaperKillHeal(level, final));
            SpawnVoidShards(target, Mathf.Max(4f, damage * SphereEvolutionCombatRules.GetVoidReaperShardDamageMultiplier(final) * power), SphereEvolutionCombatRules.GetVoidReaperShardCount(level, final), owner);
        }
    }
}
