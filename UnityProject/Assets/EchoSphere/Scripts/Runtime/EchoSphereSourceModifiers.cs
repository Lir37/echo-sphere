using System;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        public int GetSourceMainProjectilePierce(SphereAttackAgent owner, int pierce)
        {
            if (_sphereCharacterId == SphereCharacterId.Architect && _networkState?.DominantFormation?.Type == SphereNetworkFormation.Line) pierce++;
            var line = GetFormationBonus(SphereNetworkFormation.Line, GetSphereIndex(owner));
            if (line >= 1f || line > 0f && RollCombatChance(line)) pierce++;
            return pierce;
        }
        public bool HasSourceChainToxicNetwork() => HasActiveAbilitySynergy(SphereId.Chain, "chain_storm", RunUpgradeKind.Lightning, "lightning_echo_storm_f3");
        public float GetSourceSphereAttackDamage(SphereAttackAgent sphere)
        {
            var type = sphere.Type; var modifiers = GetSourceSphereModifiers(type, sphere.ProgressionLevel);
            var damage = 12f * _damageMultiplier * PlayerLoopRules.MutationDamage(_mutationStage) * ChaosOrbMultiplier("dmg") *
                (_teleportDamageBuffTimer > 0f ? 2f : 1f) *
                (_darkRitualOverloadTimer > 0f ? 1.25f + GetAbilityLevel(RunUpgradeKind.DarkRitual) * .04f : 1f) *
                LevelUpAbilityRules.GetSphereDamageMultiplier(GetAbilityLevel(RunUpgradeKind.Damage)) * PersistentUpgradeRules.GetPercentMultiplier(GetShopUpgradeLevel("dmg"));
            if (modifiers.fire > 0f)
            {
                if (_fireTrailTimer > 0f) damage *= 1.2f + GetAbilityLevel(RunUpgradeKind.FireTrail) * .025f;
                if (_fireCatalystTimer > 0f) damage *= 1.35f;
                if (GetAbilityBranch(RunUpgradeKind.FireTrail) == "firetrail_overdrive") damage *= 1.15f;
            }
            if (_timeStopTimer > 0f && _timeStopTimer <= 1f && GetAbilityFinalArchetype(RunUpgradeKind.TimeStop) == "timestop_temporal_core") damage *= 2f;
            damage += SphereBoostDamageBonus;
            damage *= Mathf.Max(.25f, 1f + ArtifactRewardRules.GetEffectTotal(_ownedArtifacts, "sphereDamage"));
            damage *= GetArtifactSphereFamilyDamageMultiplier(type) * GetArtifactSphereSynergyDamageMultiplier(sphere) * modifiers.damage * GetSphereCopyOutputMultiplier(sphere);
            return damage * SphereCombatRules.GetProfile(type).DamageMultiplier;
        }
        public SourceSphereModifiers GetSourceSphereModifiers(SphereId type, int level = -1)
        {
            var branch = GetSphereBranch(type); var final = GetSphereFinal(type);
            return SourceSphereGlobalModifiers.Merge(SourceSphereModifiersRules.Get(type, level < 0 ? GetSphereProgressionLevel(type) : level, branch, final),
                type, branch, final, _sphereMods, GetAbilityLevel(RunUpgradeKind.Range));
        }
        public int GetSourceSphereModifierLevel(SphereId type, SphereAuthoredModifier modifier)
        {
            if (modifier == SphereAuthoredModifier.Multishot || modifier == SphereAuthoredModifier.Pierce)
                return Math.Max(SphereAuthoredModifierRules.GetLevel(GetSphereBranch(type), GetSphereFinal(type), modifier), GetGlobalSphereModifierLevel(modifier.ToString().ToLowerInvariant()));
            var values = GetSourceSphereModifiers(type);
            return (int)(float)typeof(SourceSphereModifiers).GetField(modifier.ToString().ToLowerInvariant()).GetValue(values);
        }
    }
}
