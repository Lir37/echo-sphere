using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;
namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        [Serializable] private sealed class SourceDamageNumber
        { public Vector2 position, velocity; public float value, life, maxLife; public bool critical; public string element; }
        private readonly List<SourceDamageNumber> _damageNumbers = new List<SourceDamageNumber>();
        public int GetSphereProgressionLevel(SphereId type) => _sphereLevels.TryGetValue(type, out var level) ? level : 0;
        public EnemyAgent2D FindSourceSphereTarget(SphereId type, Vector2 position, float range)
        {
            EnemyAgent2D best = null; var bestDistance = float.PositiveInfinity; var bestTier = -1;
            foreach (var enemy in _enemies)
            {
                if (enemy == null || !enemy.IsAlive) continue;
                var distance = ((Vector2)enemy.transform.position - position).sqrMagnitude;
                if (distance > range * range) continue;
                var tier = enemy.IsBoss ? 4 : enemy.IsElite ? 3 : enemy.IsTank ? 2 : enemy.Archetype == EnemyArchetype.Fast ? 1 : 0;
                if (best == null || (type == SphereId.Sniper ? tier > bestTier || (tier == bestTier && distance > bestDistance) :
                    type == SphereId.Prism ? enemy.CurrentHp > best.CurrentHp : type == SphereId.Void ? enemy.CurrentHp < best.CurrentHp : distance < bestDistance))
                { best = enemy; bestDistance = distance; bestTier = tier; }
            }
            return best;
        }
        public void EmitSourceHitFeedback(EnemyAgent2D enemy, float damage, bool sphere, bool procs, bool critical, SphereId? source)
        {
            var count = !sphere ? 1 : procs ? (enemy.IsBoss ? 10 : critical ? 9 : enemy.IsElite ? 7 : 4) : 1;
            for (var i = 0; i < count; i++)
            {
                var a = _rng.NextFloat() * Mathf.PI * 2f;
                var speed = (critical ? 120f : 80f) + _rng.NextFloat() * (critical ? 140f : 100f);
                var life = 0.16f + _rng.NextFloat() * (critical ? 0.26f : 0.14f);
                var size = (critical ? 2.5f : 1.5f) + _rng.NextFloat() * (critical ? 2f : 1.7f);
                SpawnSourceParticle(enemy.transform.position, new Vector2(Mathf.Cos(a) * speed, -Mathf.Sin(a) * speed), life, size, critical ? "#c4453d" : enemy.SourceColorHtml, 0.42f);
            }
            if (critical) _characterScreenShakeIntensity = Mathf.Min(0.24f, _characterScreenShakeIntensity + 0.06f);
            if (!sphere || !procs) return;
            var resolvedElement = source.HasValue ? SphereElementalRules.ResolveElement(GetSphereBranch(source.Value)) : SphereElement.None;
            if (source.HasValue && resolvedElement == SphereElement.None)
            {
                var mods = GetSourceSphereModifiers(source.Value);
                resolvedElement = mods.fire > 0f ? SphereElement.Fire : mods.freeze > 0f ? SphereElement.Freeze : mods.poison > 0f ? SphereElement.Poison : SphereElement.None;
            }
            var element = resolvedElement.ToString().ToLowerInvariant();
            AddSourceDamageNumber(enemy, damage, critical, element, false);
            SourceAudio2D.Play(critical ? "crit" : "hit");
        }
        public void AddSourceStatusDamageNumber(EnemyAgent2D enemy, float damage, string element)
        { if (damage > 0f) AddSourceDamageNumber(enemy, damage, false, element, true); }
        public void EmitSourceProjectileImpact(Vector2 position, Color color)
        {
            for (var i = 0; i < 6; i++)
            {
                var angle = _rng.NextFloat() * Mathf.PI * 2f;
                SpawnSourceParticle(position, new Vector2(Mathf.Cos(angle) * 80f, -Mathf.Sin(angle) * 80f), 0.3f, 2f, "#" + ColorUtility.ToHtmlStringRGB(color), 0.3f);
            }
        }
        public void EmitSourceStatusParticle(EnemyAgent2D enemy, bool fire, float tickInterval)
        {
            if (_rng.NextFloat() >= (fire ? tickInterval > 0f ? 0.55f : 0.3f : tickInterval > 0f ? 0.48f : 0.2f)) return;
            var radius = enemy.SourceRadius * SpherePlacementRules.SourcePixelsToWorld;
            var position = (Vector2)enemy.transform.position + new Vector2((_rng.NextFloat() * 2f - 1f) * radius, -(_rng.NextFloat() * 2f - 1f) * radius);
            var life = fire ? 0.3f : 0.4f;
            SpawnSourceParticle(position, new Vector2(0f, fire ? 30f : 20f), life, 2f, fire ? "#ff743d" : "#72f08e", life);
        }
        private void AddSourceDamageNumber(EnemyAgent2D enemy, float damage, bool critical, string element, bool status)
        {
            if (_damageNumbers.Count >= 72) _damageNumbers.RemoveRange(0, _damageNumbers.Count - 71);
            var jitter = status ? 6f : 8f; var velocity = status ? 18f : 30f;
            var scale = SpherePlacementRules.SourcePixelsToWorld;
            _damageNumbers.Add(new SourceDamageNumber { position = (Vector2)enemy.transform.position + new Vector2((-jitter + _rng.NextFloat() * jitter * 2f) * scale, (enemy.SourceRadius + (status ? 4f : 5f)) * scale),
                velocity = new Vector2((-velocity + _rng.NextFloat() * velocity * 2f) * scale, (status ? 42f : 60f) * scale),
                value = Mathf.Floor(damage + 0.5f), life = status ? 0.65f : 0.8f, maxLife = status ? 0.65f : 0.8f, critical = critical, element = element });
        }
        private void TickSourceDamageNumbers(float dt)
        {
            for (var i = _damageNumbers.Count - 1; i >= 0; i--)
            {
                var number = _damageNumbers[i]; number.position += number.velocity * dt;
                number.velocity.y -= 80f * SpherePlacementRules.SourcePixelsToWorld * dt;
                number.life -= dt; if (number.life <= 0f) _damageNumbers.RemoveAt(i);
            }
        }
        private void DrawSourceDamageNumbers()
        {
            var previous = GUI.color;
            foreach (var number in _damageNumbers)
            {
                var color = number.critical ? new Color(1f, 0.72f, 0.30f) : number.element == "fire" ? new Color(1f, .45f, .24f) : number.element == "freeze" ? new Color(.39f, .87f, 1f) : number.element == "poison" ? new Color(.45f, .94f, .56f) : new Color(.91f, .86f, .75f);
                color.a = Mathf.Clamp01(number.life / number.maxLife); GUI.color = color;
                var p = WorldToGuiPoint(number.position);
                var style = new GUIStyle(_label) { alignment = TextAnchor.MiddleCenter, fontSize = number.critical ? 18 : 13, fontStyle = number.critical ? FontStyle.Bold : FontStyle.Normal };
                GUI.Label(new Rect(p.x - 35f, p.y - 13f, 70f, 26f), number.value.ToString("0", System.Globalization.CultureInfo.InvariantCulture), style);
            }
            GUI.color = previous;
        }
    }
}
