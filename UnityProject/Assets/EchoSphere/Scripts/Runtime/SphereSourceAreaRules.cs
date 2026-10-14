using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class SphereAttackAgent
    {
        private bool DamageSourceArea(Vector2 origin, float radius, float damage, float dt)
        {
            var branch = _runtime.GetSphereBranch(_type);
            var final = _runtime.GetSphereFinal(_type) ?? "";
            var mode = final.EndsWith("_final_1") ? 0 : final.EndsWith("_final_2") ? 1 : final.EndsWith("_final_3") ? 2 : -1;
            var power = mode < 0 ? _progressionLevel >= 6 ? 1.3f : _progressionLevel >= 5 ? 1.15f : 1f : 1f;
            var unit = SpherePlacementRules.SourcePixelsToWorld;
            var index = _runtime.GetSphereIndex(this);
            var cluster = _runtime.GetFormationBonus(SphereNetworkFormation.Cluster, index);
            bool Near(EnemyAgent2D enemy, float range) => enemy != null && enemy.IsAlive && Vector2.Distance(enemy.transform.position, origin) <= range;
            if (_type == SphereId.Pulse)
            {
                radius *= _sourceModifiers.radius;
                if (branch == "pulse_wave") radius *= (mode == 1 ? 1.24f : 1.10f) * power;
                if (cluster > 0f) radius *= 1f + .06f * cluster;
                var waves = 1 + (_runtime.HasArtifact("pulse_crown") ? 1 : 0) + (branch == "pulse_burst" && (mode == 2 || mode < 0) ? 1 : 0);
                for (var wave = 0; wave < waves; wave++)
                {
                    var secondaryPower = branch == "pulse_burst" && mode < 0 ? power : 1f;
                    var waveDamage = damage * (wave == 0 ? 1f : .46f + (branch == "pulse_burst" ? .14f : 0f) * secondaryPower * power);
                    var range = radius * (wave == 0 ? 1f : .68f);
                    foreach (var enemy in _runtime.SourceRadarEnemies) if (Near(enemy, range))
                    { enemy.ReceiveSphereDamage(this, waveDamage); if (branch == "pulse_wave") enemy.ApplySlow(1.2f, .6f); }
                    for (var i = 0; i < 14; i++) _runtime.EmitSourceRadialParticle(origin, 90f, .35f, 3f, GetColor(_type));
                    foreach (var enemy in _runtime.SourceRadarEnemies) if (Near(enemy, range)) OnSphereHitApplied(enemy);
                    if (branch == "pulse_wave") foreach (var enemy in _runtime.SourceRadarEnemies)
                        if (Near(enemy, radius)) enemy.MoveSourceRadial(origin, 18f * power * unit);
                }
                if (branch == "pulse_resonator" && (_runtime.HasNetworkCandidateForSphere(SphereNetworkFormation.Triangle, index) || _runtime.HasNetworkCandidateForSphere(SphereNetworkFormation.Lattice, index) || _runtime.HasNetworkCandidateForSphere(SphereNetworkFormation.Ring, index)))
                {
                    var bonus = Mathf.Max(_runtime.GetFormationBonus(SphereNetworkFormation.Triangle, index), Mathf.Max(_runtime.GetFormationBonus(SphereNetworkFormation.Lattice, index), _runtime.GetFormationBonus(SphereNetworkFormation.Ring, index)));
                    _runtime.ChargeResonanceFromSource(ResonanceRules.NetworkCharge * (bonus > 0f ? bonus : 1f) * Mathf.Max(1f, _sourceModifiers.resonantCharge / 2f), true);
                }
                if (branch == "pulse_burst" && _runtime.HasNetworkCandidateForSphere(SphereNetworkFormation.Cluster, index)) _runtime.ChargeResonanceFromSource(ResonanceRules.GeometryCharge, false);
                return true;
            }
            if (_type == SphereId.Gravity)
            {
                radius *= _sourceModifiers.radius * _sourceModifiers.auraRadius;
                var grouped = 0; foreach (var enemy in _runtime.SourceRadarEnemies) if (Near(enemy, radius)) grouped++;
                var pull = 34f * (_progressionLevel >= 1 ? 1.2f : 1f) * (_runtime.HasArtifact("gravity_bead") ? 1.12f : 1f) * (_runtime.HasArtifact("gravity_hook") ? 1.1f : 1f) * (1f + .20f * cluster);
                if (branch == "gravity_well") pull *= (mode == 1 ? 1.35f : mode == 2 ? 1.50f : 1.15f) * power;
                if (branch == "gravity_tide") pull *= 1.05f * power;
                if (branch == "gravity_collapse") pull *= .90f * power;
                foreach (var enemy in _runtime.SourceRadarEnemies) if (Near(enemy, radius))
                {
                    enemy.ApplySlow((branch == "gravity_well" ? .75f : .45f) * power, branch == "gravity_well" ? Mathf.Max(.46f, .56f - (power - 1f) * .06f) : .72f);
                    var actual = damage;
                    if (branch == "gravity_collapse")
                    { if (grouped >= 4) actual *= 1.20f * power; if (enemy.HpFraction < .45f) actual *= (mode == 2 ? 1.30f : 1.12f) * power; }
                    actual *= 1f + .08f * cluster;
                    enemy.ReceiveSphereDamage(this, actual);
                    if (branch == "gravity_tide")
                    {
                        var pulling = mode < 0 || mode == 2 ? Mathf.Sin(VisualRotation) >= 0f : mode == 1;
                        enemy.MoveSourceRadial(origin, pulling ? -pull * (mode == 1 ? 1.20f : 1f) * unit : Mathf.Max(22f, 36f * power) * unit);
                    }
                    OnSphereHitApplied(enemy);
                }
                if (grouped >= 4 && branch == "gravity_collapse" && mode == 2)
                {
                    foreach (var enemy in _runtime.SourceRadarEnemies) if (Near(enemy, radius * .45f))
                    { enemy.ReceiveDamage(damage * .65f); enemy.ApplySlow(1.2f, .6f); }
                    for (var i = 0; i < 14; i++) _runtime.EmitSourceRadialParticle(origin, 90f, .35f, 3f, GetColor(_type));
                }
                return true;
            }
            radius *= _sourceModifiers.auraRadius; var attacked = false;
            foreach (var enemy in _runtime.SourceRadarEnemies)
            {
                if (enemy == null || !enemy.IsAlive || Vector2.Distance(enemy.transform.position, origin) >= radius) continue;
                if (branch == "aura_sanctum") enemy.ApplySlow(.8f * power, Mathf.Max(.42f, .65f - (power - 1f) * .08f));
                if (branch == "aura_gravity") enemy.MoveSourceRadial(origin, -28f * power * dt * unit);
                if (branch == "aura_overgrowth") _runtime.AccelerateNearbySpheres(this, 110f * power * unit, dt * .08f * power);
                enemy.ReceiveSphereDamage(this, damage);
                if (SphereElementalRules.IsFieldBranch(branch)) OnSphereHitApplied(enemy);
                attacked = true;
            }
            if (branch == "aura_sanctum") _runtime.ApplySourceSanctumAllies(this);
            return attacked;
        }
    }
    public sealed partial class EchoSphereRuntime
    {
        public void ApplySourceSanctumAllies(SphereAttackAgent owner)
        {
            foreach (var ally in _spheres) if (ally != null && ally != owner && Vector2.Distance(ally.transform.position, owner.transform.position) < 120f * SpherePlacementRules.SourcePixelsToWorld)
            { ally.AccelerateNextAttack(.12f); SpawnSourceParticle(ally.transform.position, Vector2.zero, .18f, 3f, "#8a5a8a", .18f); }
        }
    }
}
