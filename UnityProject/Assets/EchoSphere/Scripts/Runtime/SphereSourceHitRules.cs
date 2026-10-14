using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class SphereAttackAgent
    {
        // engineCombat.ts: branch effects run after shared modifier procs and before HP subtraction.
        public float ApplySourceBranchHit(EnemyAgent2D enemy, float actual, bool critical)
        {
            var branch = _runtime.GetSphereBranch(_type);
            var final = _runtime.GetSphereFinal(_type) ?? "";
            var mode = final.EndsWith("_final_1") ? 0 : final.EndsWith("_final_2") ? 1 : final.EndsWith("_final_3") ? 2 : -1;
            var power = mode < 0 ? _progressionLevel >= 6 ? 1.30f : _progressionLevel >= 5 ? 1.15f : 1f : 1f;
            var unit = SpherePlacementRules.SourcePixelsToWorld;
            var center = (Vector2)enemy.transform.position;
            bool Near(EnemyAgent2D other, Vector2 point, float radius) => other != null && other.IsAlive && Vector2.Distance(other.transform.position, point) < radius * unit;
            void Secondary(EnemyAgent2D other, float damage) => other.ReceiveSphereDamage(_type, damage, false, false, this);
            void Arc(EnemyAgent2D other, float life) => _runtime.SpawnCombatArc(center, other.transform.position, _type, branch, final, ColorFor(_type), life);
            if (branch == "standard_resonator")
            {
                if (++_branchHitCount % 3 == 0)
                {
                    var radius = mode == 0 ? 115f : mode == 1 ? 100f : 90f;
                    foreach (var other in _runtime.SourceRadarEnemies)
                        if (other != enemy && Near(other, center, radius))
                        { Secondary(other, actual * (mode == 0 ? .65f : mode == 1 ? .45f : .35f) * power); if (mode == 2) other.ApplySlow(.8f, .7f); }
                    if (mode == 1) foreach (var other in _runtime.SourceRadarEnemies)
                        if (other != enemy && Near(other, center, 100f)) other.MoveSourceRadial(center, 18f * unit);
                    _runtime.AddSourceScreenShake(mode == 0 ? .04f : .025f, .14f);
                }
            }
            else if (branch == "standard_singularity")
            {
                enemy.ApplySlow((mode == 0 ? 1.2f : mode == 1 ? 1.8f : .9f) * power,
                    Mathf.Max(.45f, (mode == 0 ? .5f : mode == 1 ? .58f : .68f) - (power - 1f) * .08f));
                if (mode != 1) foreach (var other in _runtime.SourceRadarEnemies)
                    if (other != enemy && Near(other, center, (mode == 0 ? 75f : mode == 2 ? 100f : 65f) * power))
                        other.MoveSourceRadial(center, -(mode == 0 ? 24f : mode == 2 ? 40f : 20f) * power * unit);
                if (mode == 2 && enemy.HpFraction < .5f) actual *= 1.15f;
            }
            else if (branch == "standard_swarm")
            {
                if (_runtime.RollCombatChance(mode < 0 || mode == 2 ? 1f : mode == 0 ? .75f : .70f))
                    _runtime.SpawnStandardSwarmShards(enemy, Vector2.zero, mode < 0 ? _progressionLevel >= 6 ? 2 : 1 : mode + 1,
                        actual * (mode < 0 ? _progressionLevel >= 6 ? .65f : _progressionLevel >= 5 ? .58f : .50f : mode == 0 ? .72f : mode == 1 ? .56f : .44f), this);
            }
            else if (branch == "sniper_oracle" && _runtime.IsSourceMarkTarget(enemy))
            {
                if (mode < 0) actual *= power;
                else if (critical)
                {
                    actual *= mode == 0 ? 1.5f : mode == 1 ? 1.3f : 1.22f;
                    if (mode == 2) foreach (var other in _runtime.SourceRadarEnemies)
                        if (other != enemy && Near(other, center, 65f)) Secondary(other, actual * .25f);
                }
            }
            else if (branch == "sniper_assassin")
            {
                var hp = enemy.HpFraction;
                if (mode < 0) { if (hp < .35f) actual *= power; }
                else if (mode == 0 && hp < .45f) { actual *= 1.35f; _runtime.SetSourceMark(enemy, .9f); }
                else if (mode == 1 && hp < .20f) { actual *= 2f; _runtime.SetSourceMark(enemy, 1f); }
                else if (mode == 2 && hp < .35f) { actual *= 1.5f; _runtime.HealPlayer(actual * .012f); }
            }
            else if (branch == "sniper_beacon")
            {
                _runtime.SetSourceMark(enemy, (mode == 1 ? 5f : 3f) * power);
                enemy.ApplySlow((mode == 0 ? .9f : mode == 1 ? 1.2f : .7f) * power,
                    Mathf.Max(.42f, (mode == 0 ? .65f : mode == 1 ? .7f : .6f) - (power - 1f) * .06f));
                foreach (var other in _runtime.SourceRadarEnemies)
                    if (other != enemy && Near(other, center, (mode == 0 ? 90f : mode == 1 ? 140f : 110f) * power)) other.ApplySlow(mode == 1 ? 1f : .5f, mode == 2 ? .65f : .8f);
            }
            else if (branch == "shotgun_burst")
            {
                var distance = Vector2.Distance(center, transform.position) / unit;
                var radius = mode < 0 ? _progressionLevel >= 6 ? 150f : _progressionLevel >= 5 ? 135f : 120f : mode == 1 ? 180f : 150f;
                if (distance < radius) actual *= mode < 0 ? power : mode == 0 ? 1.30f : mode == 1 ? 1.50f : 1.22f;
                if (mode == 2 && distance < 90f) enemy.ExtendSlowStatus(.4f);
            }
            else if (branch == "shotgun_cataclysm")
            {
                for (var i = 0; i < 14; i++) _runtime.EmitSourceRadialParticle(center, 90f, .35f, 3f, "#c4453d");
                foreach (var other in _runtime.SourceRadarEnemies)
                    if (other != enemy && Near(other, center, (mode == 0 ? 60f : mode == 1 ? 85f : 55f) * power))
                        Secondary(other, actual * (mode == 0 ? .45f : mode == 1 ? .65f : .35f) * power);
                if (mode == 2) enemy.ApplySlow(.8f, .65f);
            }
            else if (branch == "shotgun_hail")
            {
                if (mode == 2) actual *= 1.08f;
                var chance = mode < 0 ? _progressionLevel >= 6 ? .60f : _progressionLevel >= 5 ? .45f : .32f : mode == 0 ? .25f : mode == 1 ? .4f : .32f;
                if (_runtime.RollCombatChance(chance))
                {
                    var count = mode == 1 ? 8 : mode < 0 && _progressionLevel >= 6 ? 7 : 6;
                    var guided = mode == 2 ? _runtime.FindNearestEnemies(center, float.PositiveInfinity, int.MaxValue) : null;
                    guided?.Remove(enemy);
                    for (var i = 0; i < count; i++)
                    {
                        var target = guided != null && i < guided.Count ? guided[i] : null;
                        var angle = target != null ? Mathf.Atan2(target.transform.position.y - center.y, target.transform.position.x - center.x) : -i * Mathf.PI * 2f / count - _runtime.SourceCombatRandom() * .18f;
                        _runtime.SpawnProjectile(center, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)), actual * (mode == 1 ? .22f : mode == 2 ? .19f : .16f),
                            new Color32(240, 179, 90, 255), 300f * unit, mode == 2 ? 1 : 0, 0f, this, 0, .45f, false, false, false, 5f);
                    }
                    var radius = mode == 1 ? 65f : mode < 0 ? _progressionLevel >= 6 ? 60f : _progressionLevel >= 5 ? 52f : 45f : 45f;
                    foreach (var other in _runtime.SourceRadarEnemies)
                        if (other != enemy && Near(other, center, radius)) Secondary(other, actual * (mode == 1 ? .18f : .12f));
                    for (var i = 0; i < count; i++) _runtime.SpawnSourceParticle(center,
                        new Vector2(Mathf.Cos(i * Mathf.PI * 2f / count) * 110f, -Mathf.Sin(i * Mathf.PI * 2f / count) * 110f), .35f, 3f, "#f0b35a", .35f);
                }
            }
            else if (branch == "chain_web")
            {
                enemy.ApplySlow((mode == 0 ? .7f : mode == 1 ? 1.4f : .5f) * power,
                    Mathf.Max(.42f, (mode == 0 ? .7f : mode == 1 ? .55f : .72f) - (power - 1f) * .08f));
                if (mode == 2) actual *= 1.25f;
                foreach (var other in _runtime.SourceRadarEnemies) if (other != enemy && Near(other, center, 90f)) Arc(other, .12f);
            }
            else if (branch == "chain_storm")
            {
                foreach (var other in _runtime.SourceRadarEnemies)
                    if (other != enemy && Near(other, center, (mode == 0 ? 70f : mode == 1 ? 100f : 55f) * power))
                    { Secondary(other, actual * (mode == 0 ? .25f : mode == 1 ? .4f : .2f) * power); Arc(other, .2f); }
                if (mode == 1 && ++_chainStormCycle % 2 == 0) TriggerResonancePulse(.45f);
                if (mode == 2 && ++_chainStormCycle % 3 == 0)
                {
                    var targets = _runtime.FindNearestEnemies(center, float.PositiveInfinity, int.MaxValue); targets.Remove(enemy);
                    if (targets.Count > 0) { Arc(targets[0], .24f); Secondary(targets[0], actual * .38f); }
                }
            }
            else if (branch == "chain_leech")
            { _runtime.HealPlayer(actual * (mode == 0 ? .025f : mode == 1 ? .045f : .018f) * power); if (mode == 2 && enemy.HpFraction < .4f) actual *= 1.2f; }
            else if (branch == "aura_sanctum")
            { enemy.ApplySlow(mode == 0 ? 1.4f : mode == 1 ? 2f : .9f, mode == 0 ? .5f : mode == 1 ? .42f : .62f); if (mode == 2) actual *= 1.12f; }
            else if (branch == "aura_gravity")
            {
                if (mode >= 0) foreach (var other in _runtime.SourceRadarEnemies)
                    if (Near(other, transform.position, mode == 0 ? 150f : 190f))
                    { other.MoveSourceRadial(transform.position, -(mode == 0 ? 55f : 80f) * unit); if (mode == 1) other.ApplySlow(.8f, .72f); }
                if (mode == 2) actual *= 1.18f;
            }
            else if (branch == "gravity_tide")
            {
                var hits = ++_gravityTideHits; var tideMode = mode < 0 ? 0 : mode;
                foreach (var other in _runtime.SourceRadarEnemies)
                    if (other != enemy && Near(other, transform.position, tideMode == 1 ? 170f : 150f))
                    { var direction = tideMode == 0 ? 1 : tideMode == 1 ? -1 : hits % 2 == 0 ? 1 : -1; other.MoveSourceRadial(transform.position, 52f * unit * direction); }
            }
            else if (branch == "aura_overgrowth")
            { _runtime.AccelerateNearbySpheres(this, (mode == 1 ? 180f : 140f) * unit, mode == 0 ? .18f : mode == 1 ? .3f : .1f); if (mode == 2) actual *= 1.12f; }
            return actual;
        }
    }
    public sealed partial class EchoSphereRuntime
    {
        public float SourceCombatRandom() => _rng.NextFloat();
        public bool IsSourceMarkTarget(EnemyAgent2D enemy) => _hunterMarkTarget == enemy;
        public void SetSourceMark(EnemyAgent2D enemy, float duration) { _hunterMarkTarget = enemy; _hunterMarkTimer = Mathf.Max(_hunterMarkTimer, duration); }
        public void AddSourceScreenShake(float amount, float cap) => _characterScreenShakeIntensity = Mathf.Min(cap, _characterScreenShakeIntensity + amount);
        public void EmitSourceRadialParticle(Vector2 position, float speed, float life, float size, string color)
        { var angle = _rng.NextFloat() * Mathf.PI * 2f; SpawnSourceParticle(position, new Vector2(Mathf.Cos(angle) * speed, -Mathf.Sin(angle) * speed), life, size, color, life); }
    }
}
