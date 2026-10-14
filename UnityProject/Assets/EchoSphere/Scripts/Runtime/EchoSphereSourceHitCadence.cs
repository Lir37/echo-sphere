using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class SphereAttackAgent
    {
        private int _sourceSquareHits, _sourceTriangleHits, _sourcePredatorHits;
        public int CountSourceSquareHit() => ++_sourceSquareHits;
        public int CountSourceTriangleHit() => ++_sourceTriangleHits;
        public int CountSourcePredatorHit() => ++_sourcePredatorHits;
    }
    public sealed partial class EchoSphereRuntime
    {
        public float GetSourceCharacterDamageMultiplier(SphereAttackAgent owner) => GetCharacterGlobalSphereDamageMultiplier() + GetCharacterNetworkDamageMultiplier(owner) - 1f;
        public float GetSourceTimeStopHitMultiplier() => _timeStopTimer > 0f && GetAbilityBranch(RunUpgradeKind.TimeStop) == "timestop_echo_phase" ? 1.18f : 1f;
        public void ApplySourceTimeStopFreeze(EnemyAgent2D enemy)
        { if (_timeStopTimer > 0f && GetAbilityFinalArchetype(RunUpgradeKind.TimeStop) == "timestop_outside_time") enemy.ApplyFreeze(.35f); }
        public float ApplySourceBeforeModifierProcs(SphereAttackAgent owner, EnemyAgent2D target, float damage, bool procs, ref bool critical)
        {
            var index = GetSphereIndex(owner);
            var square = GetFormationBonus(SphereNetworkFormation.Square, index);
            if (square > 0f && owner.CountSourceSquareHit() % (square >= 1f ? 4 : 8) == 0)
            { owner.TriggerResonancePulse(.55f); GrantPlayerShieldCharge(1); }
            if (procs && critical && ArtifactSetRules.HasCompletedSingularityPath(
                ArtifactSynergyRules.HasGlassCannonSynergy(_ownedArtifacts), ArtifactSynergyRules.HasSingularitySynergy(_ownedArtifacts)))
            {
                var same = true; foreach (var sphere in _spheres) if (sphere != null && sphere.Type != owner.Type) same = false;
                if (same)
                {
                    var count = 0;
                    foreach (var enemy in _enemies)
                        if (enemy != null && enemy != target && enemy.IsAlive && Vector2.Distance(enemy.transform.position, target.transform.position) <= 72f * SpherePlacementRules.SourcePixelsToWorld)
                        { enemy.ReceiveSphereDamage(owner.Type, damage * .20f, false, false, owner); if (++count == 3) break; }
                }
            }
            if (HasArtifact("predator_claw") && owner.CountSourcePredatorHit() % 5 == 0) { damage *= 2f; critical = true; }
            return damage;
        }
        public void ApplySourceAfterHitGeometry(SphereAttackAgent owner, EnemyAgent2D target, float damage)
        {
            var triangle = GetFormationBonus(SphereNetworkFormation.Triangle, GetSphereIndex(owner));
            if (triangle > 0f)
            {
                var grid = ArtifactSetRules.HasCompletedResonanceGrid(ArtifactSynergyRules.HasFortressNetworkSynergy(_ownedArtifacts), ArtifactSynergyRules.HasPerfectNetworkSynergy(_ownedArtifacts));
                if (owner.CountSourceTriangleHit() % (grid ? 2 : 3) == 0)
                {
                    owner.TriggerResonancePulse(.45f);
                    foreach (var enemy in _enemies)
                        if (enemy != null && enemy != target && enemy.IsAlive && Vector2.Distance(enemy.transform.position, target.transform.position) <= 88f * SpherePlacementRules.SourcePixelsToWorld)
                            enemy.ReceiveSphereDamage(owner.Type, damage * .35f * triangle, false, false, owner);
                    SpawnCombatArc(owner.transform.position, target.transform.position, owner.Type, GetSphereBranch(owner.Type), GetSphereFinal(owner.Type), SphereColor(owner.Type), .24f);
                    AddSourceScreenShake(.025f, .16f);
                }
            }
            ApplyEchoArchitectureRelayOnSphereHit(owner);
        }
    }
}
