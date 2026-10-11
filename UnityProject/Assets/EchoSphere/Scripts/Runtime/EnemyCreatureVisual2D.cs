using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Source-mapped 2.5D port of src/enemies/enemyVisual.ts. Canvas-authored
    /// creature paths are rebuilt as one dynamic mesh per enemy; no gameplay RNG.
    /// </summary>
    public sealed class EnemyCreatureVisual2D : MonoBehaviour
    {
        private const float R = 0.48f;
        private const float HitFlashDuration = 0.15f;
        private static readonly Color Dark = new Color(0.012f, 0.027f, 0.047f, 0.985f);
        private static readonly Color EyeWhite = new Color(0.96f, 0.985f, 1f, 0.94f);
        // Reused scratch paths and immutable anchors avoid per-frame path/list allocations.
        private static readonly List<Vector2> PathScratch = new List<Vector2>(96);
        private static readonly float[] LegWisp = { -.28f, .20f };
        private static readonly float[] LegLeech = { -.36f, -.02f };
        private static readonly float[] LegSkitter = { -.40f, -.04f, .32f };
        private static readonly float[] LegMoth = { -.16f, .10f };
        private static readonly float[] LegBeetle = { -.34f, .12f };
        private static readonly float[] LegBrute = { -.45f, -.12f, .20f, .48f };
        private static readonly float[] LegPrism = { -.34f, .10f };
        private static readonly float[] LegStalker = { -.22f, .16f };
        private static readonly float[] LegLinkbreaker = { -.36f, -.12f, .12f, .36f };
        private static readonly float[] LegBossShooter = { -.30f, .18f };
        private static readonly float[] LegBossCharger = { -.34f, .20f };
        private static readonly float[] LegBossSummoner = { -.34f, .18f };
        private static readonly float[] LegBossAura = { -.28f, .10f };
        private static Material _sharedMaterial;
        private EchoSphereRuntime _runtime;
        private Transform _visualRoot;
        private Mesh _mesh;
        private MeshRenderer _meshRenderer;
        private Painter _p;
        private string _variant = "wisp";
        private string _bossType = "";
        private Color _baseColor = new Color(0.29f, 0.478f, 0.541f, 1f);
        private Vector2 _facing = Vector2.right;
        private float _time;
        private float _hitFlash;
        private bool _marked;
        private float _fireRemaining, _poisonRemaining, _freezeRemaining;

        public void Initialize(EchoSphereRuntime runtime, string variant, string bossType, Color color)
        {
            _runtime = runtime;
            _variant = string.IsNullOrEmpty(variant) ? "wisp" : variant;
            _bossType = bossType ?? "";
            _baseColor = color;
            _p = new Painter();

            // Remove the prototype shell/ring/node stack instead of layering it over the creature.
            var legacySprites = GetComponentsInChildren<SpriteRenderer>(true);
            for (var i = 0; i < legacySprites.Length; i++) legacySprites[i].enabled = false;

            var visual = new GameObject("Source-Mapped Enemy Creature");
            visual.transform.SetParent(transform, false);
            visual.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            _visualRoot = visual.transform;
            _mesh = new Mesh { name = "ECHO SPHERE Enemy Creature Mesh" };
            _mesh.MarkDynamic();
            visual.AddComponent<MeshFilter>().sharedMesh = _mesh;
            _meshRenderer = visual.AddComponent<MeshRenderer>();
            _meshRenderer.sharedMaterial = SharedMaterial;
            _meshRenderer.sortingOrder = string.IsNullOrEmpty(_bossType) ? 5 : 7;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
            Rebuild();
        }

        private static Material SharedMaterial
        {
            get
            {
                if (_sharedMaterial == null)
                {
                    var shader = Shader.Find("Sprites/Default");
                    if (shader != null)
                    {
                        _sharedMaterial = new Material(shader);
                        _sharedMaterial.name = "ECHO SPHERE Enemy Creature (Unlit)";
                        _sharedMaterial.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
                return _sharedMaterial;
            }
        }

        public void SetFacing(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.0001f) _facing = direction.normalized;
        }
        public void SetMarked(bool value) { _marked = value; }
        public void SetStatusVfx(float fireRemaining, float poisonRemaining, float freezeRemaining)
        {
            _fireRemaining = Mathf.Max(0f, fireRemaining);
            _poisonRemaining = Mathf.Max(0f, poisonRemaining);
            _freezeRemaining = Mathf.Max(0f, freezeRemaining);
        }
        public void PulseHit() { _hitFlash = HitFlashDuration; }

        private void LateUpdate()
        {
            if (_mesh == null || (_runtime != null && _runtime.IsGameplayPaused)) return;
            _time += Time.deltaTime;
            _hitFlash = Mathf.Max(0f, _hitFlash - Time.deltaTime);
            var hit = Mathf.Clamp01(_hitFlash / HitFlashDuration);
            var sourceX = transform.position.x / SpherePlacementRules.SourcePixelsToWorld;
            var pulse = 1f + Mathf.Sin(_time * 5f + sourceX * 0.01f) * 0.025f;
            _visualRoot.localScale = Vector3.one * (pulse + hit * 0.05f);
            _visualRoot.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_facing.y, _facing.x) * Mathf.Rad2Deg);
            Rebuild();
        }

        private void Rebuild()
        {
            if (_mesh == null || _p == null) return;
            _p.Clear();
            var c = _freezeRemaining > 0f
                ? new Color(105f / 255f, 214f / 255f, 1f, 1f)
                : _marked ? Color.Lerp(_baseColor, new Color(0.50f, 0.82f, 1f, 1f), 0.72f) : _baseColor;
            var hit = Mathf.Clamp01(_hitFlash / HitFlashDuration);
            if (!string.IsNullOrEmpty(_bossType))
            {
                DrawSourceOuterGlow(_p, R * 1.65f, c, 0.21f);
                _p.Gradient(Vector2.zero, R * 2.55f, c, 0.085f);
                // The boss root scale already maps the source 34 px radius to world units.
                // Do not apply the old prototype's extra 1.15 visual-size multiplier.
                DrawBoss(_p, R, c, _time, _bossType);
            }
            else
            {
                DrawSourceOuterGlow(_p, R * 1.15f, c, 0.10f);
                _p.Gradient(Vector2.zero, R * 2.15f, c, 0.065f);
                DrawCreature(_p, R, c, _time, _variant);
                DrawElementalStatusVfx(_p, R, _time, _fireRemaining > 0f, _poisonRemaining > 0f, _freezeRemaining > 0f);
                if (hit > 0f) DrawHitFlash(_p, hit);
            }
            _mesh.Clear();
            _mesh.SetVertices(_p.Vertices);
            _mesh.SetColors(_p.Colors);
            _mesh.SetTriangles(_p.Triangles, 0);
            _mesh.RecalculateBounds();
        }

        private static void DrawElementalStatusVfx(Painter p, float r, float t, bool burning, bool poisoned, bool frozen)
        {
            // Source-mapped from src/renderer.ts:drawEnemyStatusVfx. The effects are
            // presentation-only and use the calibrated 19 px creature radius as a unit.
            const float pixelsPerRadius = 19f;
            if (burning)
            {
                var pulse = .5f + .5f * Mathf.Sin(t * 10f);
                var fireColor = new Color(1f, 102f / 255f, 61f / 255f, .28f + .18f * pulse);
                for (var i = 0; i < 4; i++)
                {
                    var a = t * 2.4f + i * Mathf.PI / 2f;
                    var rr = r * (.8f + .18f * Mathf.Sin(t * 5f + i));
                    var at = new Vector2(Mathf.Cos(a) * rr, Mathf.Sin(a) * rr * .65f - r * .18f);
                    var pxRadius = r * (2.2f + pulse * 1.4f) / pixelsPerRadius;
                    var particle = new Color(fireColor.r, fireColor.g, fireColor.b, .28f + .18f * pulse);
                    p.Circle(at, pxRadius, particle, particle, 0f);
                }

                var flameCrest = PathScratch; flameCrest.Clear();
                QuadCurve(flameCrest, new Vector2(-r * .35f, -r * .65f),
                    new Vector2(0f, -r * (1.15f + pulse * .15f)),
                    new Vector2(r * .35f, -r * .65f), 8);
                p.StrokePath(flameCrest, new Color(1f, 190f / 255f, 90f / 255f, .25f + .2f * pulse),
                    r * 1.4f / pixelsPerRadius, false);
            }

            if (poisoned)
            {
                for (var i = 0; i < 5; i++)
                {
                    var a = t * .65f + i * 1.23f;
                    var rr = r * (.85f + .18f * Mathf.Repeat(t * .7f + i * .2f, 1f));
                    var at = new Vector2(Mathf.Cos(a) * rr, Mathf.Sin(a) * rr * .55f - r * .12f);
                    var pxRadius = r * (2f + i % 2) / pixelsPerRadius;
                    var particle = new Color(101f / 255f, 227f / 255f, 143f / 255f, .38f);
                    p.Circle(at, pxRadius, particle, particle, 0f);
                }

                var poisonTrail = PathScratch; poisonTrail.Clear();
                QuadCurve(poisonTrail, new Vector2(-r * .7f, r * .35f),
                    new Vector2(0f, r * .72f), new Vector2(r * .7f, r * .35f), 8);
                p.StrokePath(poisonTrail, new Color(113f / 255f, 245f / 255f, 157f / 255f, .35f),
                    r / pixelsPerRadius, false);
            }

            if (frozen)
            {
                var spin = t * .8f;
                var ice = new Color(140f / 255f, 233f / 255f, 1f, .72f);
                for (var i = 0; i < 6; i++)
                {
                    var a = spin + i * Mathf.PI / 3f;
                    var direction = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    var from = new Vector2(direction.x * r * 1.12f, direction.y * r * .72f);
                    var to = from + new Vector2(direction.x * r * .22f, direction.y * r * .22f);
                    p.Stroke(from, to, ice, r * 1.3f / pixelsPerRadius);
                }
                var frostCore = new Color(210f / 255f, 250f / 255f, 1f, .32f);
                p.Circle(Vector2.zero, r * .22f, frostCore, frostCore, 0f);
            }
        }

        private static void DrawSourceOuterGlow(Painter p, float radius, Color color, float alpha)
        {
            // Source renderer.ts:glowCircle uses two additive discs before the creature body.
            var outer = new Color(color.r, color.g, color.b, alpha * 0.16f);
            p.Circle(Vector2.zero, radius, outer, outer, 0f);
            var inner = new Color(color.r, color.g, color.b, alpha * 0.55f);
            p.Circle(Vector2.zero, radius * 0.52f, inner, inner, 0f);
        }

        private static void DrawHitFlash(Painter p, float hit)
        {
            // Source renderer.ts draws a short white X over hit enemies, rather than tinting the whole body.
            var color = new Color(1f, 1f, 1f, hit * 0.75f);
            var line = R * 0.105f; // 2 source pixels at the authored 19 px normal-enemy radius.
            p.Stroke(new Vector2(-R * 0.8f, -R * 0.8f), new Vector2(R * 0.8f, R * 0.8f), color, line);
            p.Stroke(new Vector2(R * 0.8f, -R * 0.8f), new Vector2(-R * 0.8f, R * 0.8f), color, line);
        }

        private static void DrawCreature(Painter p, float r, Color c, float t, string v)
        {
            switch (v)
            {
                case "wisp": // Veil Ripper
                    Body(p, Vector2.zero, r, c, Mathf.Sin(t * 2.4f) * .04f, .82f);
                    Wing(p, new Vector2(-r * .18f, -r * .38f), r * .82f, r * .34f, -1.15f, c, t * 5f);
                    Wing(p, new Vector2(-r * .18f, r * .38f), r * .82f, r * .34f, 1.15f, c, t * 5f);
                    Legs(p, r, LegWisp, .30f, .58f, t * 3f, c, .60f);
                    Eye(p, new Vector2(r * .30f, -r * .12f), r * .13f, c);
                    Core(p, new Vector2(-r * .16f, r * .10f), r * .09f, c);
                    break;
                case "leech": // Grave Leech
                    for (var i = 0; i < 7; i++)
                    {
                        var q = i / 6f; var x = -r * .62f + q * r * 1.22f;
                        var y = Mathf.Sin(t * 3.2f + q * 4.8f) * r * .14f;
                        Body(p, new Vector2(x, y), r * (.34f - .025f * q), c, Mathf.Sin(t * 3f + q * 3.2f) * .14f, .72f);
                        if (i < 6) Joint(p, new Vector2(x + r * .14f, y), r * .07f, c);
                    }
                    var lh = new Vector2(r * .55f, Mathf.Sin(t * 3.2f + 5f) * r * .10f);
                    Body(p, lh, r * .34f, c, .05f, .66f);
                    Eye(p, lh + new Vector2(r * .14f, -r * .10f), r * .09f, c);
                    Eye(p, lh + new Vector2(r * .14f, r * .10f), r * .09f, c);
                    Legs(p, r, LegLeech, .20f, .30f, t * 4f, c, .34f);
                    break;
                case "serpent": // Fanged Coil
                    for (var i = 0; i < 8; i++)
                    {
                        var q = i / 7f; var x = r * .46f - q * r * 1.10f;
                        var y = Mathf.Sin(t * 3.1f + q * 4.8f) * r * (.08f + .06f * q);
                        var dx = -r * 1.10f / 7f;
                        var dy = Mathf.Cos(t * 3.1f + q * 4.8f) * r * (.08f + .06f * q) * 4.8f / 7f;
                        Body(p, new Vector2(x, y), r * (.29f - .018f * i), c, Mathf.Atan2(dy, dx), .64f);
                        if (i < 7) Joint(p, new Vector2(x + dx * .5f, y + dy * .5f), r * .055f, c);
                    }
                    var sh = new Vector2(r * .50f, -r * .06f);
                    Body(p, sh, r * .36f, c, -.06f, .62f);
                    Eye(p, sh + new Vector2(r * .14f, -r * .10f), r * .09f, c);
                    Eye(p, sh + new Vector2(r * .14f, r * .10f), r * .09f, c);
                    var fang = PathScratch; fang.Clear();
                    fang.Add(sh + new Vector2(r * .12f, 0f)); fang.Add(sh + new Vector2(r * .34f, r * .05f)); fang.Add(sh + new Vector2(r * .12f, r * .10f));
                    p.Polygon(fang, Dark, c, .008f);
                    break;
                case "skitter": // Carrion Skitter
                    Body(p, Vector2.zero, r, c, Mathf.Sin(t * 4f) * .05f, .58f);
                    Legs(p, r, LegSkitter, .22f, .58f, t * 7f, c, .62f);
                    var kh = new Vector2(r * .45f, 0f);
                    Body(p, kh, r * .34f, c, -.05f, .70f);
                    Eye(p, kh + new Vector2(0f, -r * .10f), r * .075f, c);
                    Eye(p, kh + new Vector2(0f, r * .10f), r * .075f, c);
                    break;
                case "moth": // Umbral Moth
                    Wing(p, new Vector2(-r * .08f, -r * .40f), r * .92f, r * .62f, -2.72f, c, t * 8f);
                    Wing(p, new Vector2(-r * .08f, r * .40f), r * .92f, r * .62f, 2.72f, c, t * 8f);
                    Body(p, Vector2.zero, r * .72f, c, 0f, 1.125f);
                    Legs(p, r, LegMoth, .28f, .48f, t * 8f, c, .36f);
                    Eye(p, new Vector2(-r * .07f, -r * .26f), r * .09f, c);
                    Eye(p, new Vector2(r * .07f, -r * .26f), r * .09f, c);
                    break;
                case "beetle": // Rift Scarab
                    Body(p, Vector2.zero, r, c, Mathf.Sin(t * 1.6f) * .03f, .68f);
                    for (var i = -1; i <= 1; i++) p.Stroke(new Vector2(-r * .52f, i * r * .12f), new Vector2(r * .50f, i * r * .08f), new Color(.92f, .976f, 1f, .28f), Mathf.Max(.006f, r * .025f));
                    Legs(p, r, LegBeetle, .24f, .55f, t * 3f, c, .55f);
                    Eye(p, new Vector2(r * .48f, -r * .10f), r * .07f, c);
                    Eye(p, new Vector2(r * .48f, r * .10f), r * .07f, c);
                    Core(p, new Vector2(-r * .22f, 0f), r * .08f, c);
                    break;
                case "brute": // Boneback Brute
                    Body(p, Vector2.zero, r * 1.05f, c, Mathf.Sin(t * 1.7f) * .025f, .88f);
                    Legs(p, r, LegBrute, .30f, .72f, t * 6f, c, .78f);
                    for (var i = 0; i < 3; i++)
                    {
                        var x = -r * .25f + i * r * .22f;
                        var horn = PathScratch; horn.Clear();
                        horn.Add(new Vector2(x, -r * .40f)); horn.Add(new Vector2(x + r * .10f, -r * .70f)); horn.Add(new Vector2(x + r * .20f, -r * .40f));
                        p.Polygon(horn, new Color(.024f, .039f, .063f, .98f), new Color(.90f, .94f, .98f, .46f), .009f);
                    }
                    Eye(p, new Vector2(r * .36f, -r * .17f), r * .11f, c);
                    Eye(p, new Vector2(r * .36f, r * .17f), r * .11f, c);
                    break;
                case "prism": // Glass Hound
                    Body(p, Vector2.zero, r * .92f, c, Mathf.Sin(t * 2.5f) * .04f, .62f);
                    Legs(p, r, LegPrism, .30f, .60f, t * 5f, c, .48f);
                    var ph = new Vector2(r * .48f, -r * .10f);
                    Body(p, ph, r * .42f, c, -.12f, .62f);
                    Eye(p, ph + new Vector2(r * .13f, -r * .10f), r * .09f, c);
                    Tail(p, -r * .96f, 0f, r * .82f, c, t, 6, .20f);
                    break;
                case "stalker": // Hollow Stalker
                    Body(p, new Vector2(0f, Mathf.Sin(t * 4f) * r * .06f), r * .72f, c, 0f, 1.25f);
                    Legs(p, r, LegStalker, .24f, .90f, t * 5f, c, .52f);
                    Eye(p, new Vector2(r * .10f, -r * .24f), r * .12f, c);
                    Core(p, new Vector2(-r * .12f, r * .20f), r * .075f, c);
                    break;
                case "linkbreaker":
                default: // Cable Widow; source dispatcher intentionally falls back here.
                    Body(p, Vector2.zero, r * .92f, c, 0f, .72f);
                    Legs(p, r, LegLinkbreaker, .18f, .58f, t * 4f, c, .40f);
                    Eye(p, new Vector2(r * .26f, -r * .10f), r * .085f, new Color(1f, .39f, .50f));
                    Eye(p, new Vector2(r * .26f, r * .10f), r * .085f, new Color(1f, .39f, .50f));
                    Core(p, new Vector2(-r * .20f, 0f), r * .08f, new Color(1f, .39f, .50f));
                    break;
            }
        }

        private static void DrawBoss(Painter p, float r, Color c, float t, string type)
        {
            var arc = PathScratch; arc.Clear();
            for (var i = 0; i <= 48; i++)
            {
                var a = t * .2f + 4.9f * i / 48f;
                arc.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r * 1.16f);
            }
            p.StrokePath(arc, new Color(c.r, c.g, c.b, .30f), Mathf.Max(.008f, r * .025f), false);
            switch (type)
            {
                case "shooter": // Void Lancer
                    Body(p, Vector2.zero, r * 1.12f, c, Mathf.Sin(t * 1.7f) * .025f, .62f);
                    Wing(p, new Vector2(-r * .18f, -r * .48f), r, r * .36f, -1.15f, c, t * 4f);
                    Wing(p, new Vector2(-r * .18f, r * .48f), r, r * .36f, 1.15f, c, t * 4f + .35f);
                    Legs(p, r, LegBossShooter, .34f, .62f, t * 3f, c, .82f);
                    var vh = new Vector2(r * .62f, 0f);
                    Body(p, vh, r * .48f, c, 0f, .58f);
                    Eye(p, vh + new Vector2(r * .18f, -r * .12f), r * .14f, c);
                    Eye(p, vh + new Vector2(r * .18f, r * .12f), r * .14f, c);
                    Tail(p, -r * .90f, 0f, r * 1.10f, c, t, 8, .16f);
                    Core(p, new Vector2(-r * .12f, 0f), r * .12f, c);
                    break;
                case "charger": // Dread Charger
                    Body(p, Vector2.zero, r * 1.14f, c, Mathf.Sin(t * 2f) * .025f, .84f);
                    Legs(p, r, LegBossCharger, .35f, .82f, t * 6f, c, .90f);
                    var ch = new Vector2(r * .50f, 0f);
                    Body(p, ch, r * .50f, c, 0f, .72f);
                    Eye(p, ch + new Vector2(r * .20f, -r * .14f), r * .15f, c);
                    Eye(p, ch + new Vector2(r * .20f, r * .14f), r * .15f, c);
                    Wing(p, new Vector2(-r * .05f, -r * .50f), r * .72f, r * .25f, -.72f, c, t * 3f);
                    Wing(p, new Vector2(-r * .05f, r * .50f), r * .72f, r * .25f, .72f, c, t * 3f + .35f);
                    Core(p, new Vector2(-r * .30f, 0f), r * .14f, c);
                    break;
                case "summoner": // Brood Matriarch
                    Body(p, Vector2.zero, r, c, Mathf.Sin(t) * .02f, .96f);
                    Legs(p, r, LegBossSummoner, .38f, 1f, t * 3f, c, .95f);
                    var bh = new Vector2(r * .34f, -r * .05f);
                    Body(p, bh, r * .52f, c, -.05f, .76f);
                    Eye(p, bh + new Vector2(r * .20f, -r * .12f), r * .14f, c);
                    Eye(p, bh + new Vector2(r * .20f, r * .12f), r * .14f, c);
                    for (var i = 0; i < 5; i++)
                    {
                        var a = t * .8f + i * Mathf.PI * 2f / 5f;
                        var pos = new Vector2(Mathf.Cos(a) * r * 1.10f, Mathf.Sin(a) * r * .72f);
                        Body(p, pos, r * .24f, c, a, .9f);
                        Eye(p, pos + new Vector2(0f, -r * .03f), r * .055f, c);
                    }
                    Core(p, new Vector2(-r * .20f, r * .06f), r * .13f, c);
                    break;
                default: // source enemyVisual.ts fallback: Abyssal Leviathan
                    for (var i = 0; i < 8; i++)
                    {
                        var x = -r * .76f + i * r * .22f;
                        var y = Mathf.Sin(t * 2f + i * .72f) * r * .16f;
                        Body(p, new Vector2(x, y), r * (.50f - .035f * i), c, Mathf.Sin(t + i) * .12f, .70f);
                    }
                    var ah = new Vector2(r * .54f, 0f);
                    Body(p, ah, r * .58f, c, -.08f, .68f);
                    Eye(p, ah + new Vector2(r * .22f, -r * .16f), r * .15f, c);
                    Eye(p, ah + new Vector2(r * .22f, r * .16f), r * .15f, c);
                    Legs(p, r, LegBossAura, .32f, .82f, t * 2f, c, .70f);
                    Tail(p, -r * .90f, 0f, r * 1.16f, c, t, 9, .14f);
                    Core(p, new Vector2(-r * .08f, 0f), r * .12f, c);
                    break;
            }
        }

        private static void Body(Painter p, Vector2 center, float r, Color c, float rot, float sy)
        {
            var v = PathScratch; v.Clear();
            QuadCurve(v, new Vector2(-r * .72f, 0f), new Vector2(-r * .52f, -r * .62f), new Vector2(0f, -r * .58f), 6);
            QuadCurve(v, v[v.Count - 1], new Vector2(r * .58f, -r * .52f), new Vector2(r * .70f, 0f), 6, true);
            QuadCurve(v, v[v.Count - 1], new Vector2(r * .52f, r * .56f), new Vector2(0f, r * .62f), 6, true);
            QuadCurve(v, v[v.Count - 1], new Vector2(-r * .56f, r * .55f), new Vector2(-r * .72f, 0f), 6, true);
            for (var i = 0; i < v.Count; i++) v[i] = center + Rotate(new Vector2(v[i].x, v[i].y * sy), rot);
            p.Polygon(v, Dark, c, Mathf.Max(.012f, r * .055f));
        }

        private static void Wing(Painter p, Vector2 at, float len, float w, float angle, Color c, float flap)
        {
            angle += Mathf.Sin(flap) * .14f;
            var v = PathScratch; v.Clear();
            QuadCurve(v, Vector2.zero, new Vector2(len * .35f, -w * .85f), new Vector2(len, -w * .18f), 6);
            QuadCurve(v, v[v.Count - 1], new Vector2(len * .68f, w * .55f), new Vector2(len * .12f, w * .36f), 6, true);
            for (var i = 0; i < v.Count; i++) v[i] = at + Rotate(v[i], angle);
            p.Polygon(v, new Color(.016f, .035f, .067f, .96f), c, Mathf.Max(.008f, w * .07f));
            for (var i = 1; i < 4; i++)
                p.Stroke(at + Rotate(new Vector2(len * .08f, 0f), angle),
                    at + Rotate(new Vector2(len * (.24f + i * .18f), -w * (.42f - i * .08f)), angle),
                    new Color(.92f, .976f, 1f, .25f), Mathf.Max(.006f, w * .025f));
        }

        private static void Legs(Painter p, float r, float[] anchors, float sideY, float length, float phase, Color c, float thick)
        {
            for (var i = 0; i < anchors.Length; i++)
            {
                var x = anchors[i] * r;
                Leg(p, new Vector2(x, -sideY * r), length * r, -1f, phase + i * 1.05f + Mathf.PI, c, thick);
                Leg(p, new Vector2(x, sideY * r), length * r, 1f, phase + i * 1.05f, c, thick);
            }
        }

        private static void Leg(Painter p, Vector2 hip, float len, float side, float phase, Color c, float thick)
        {
            var stride = Mathf.Sin(phase) * len * .30f;
            var lift = Mathf.Max(0f, Mathf.Cos(phase)) * len * .12f;
            var knee = hip + new Vector2(stride * .46f + len * .05f, side * (len * .34f - lift));
            var ankle = hip + new Vector2(stride, side * (len * .76f - lift * .45f));
            Segment(p, hip, knee, thick * 5.8f, c, .94f); Joint(p, knee, thick * 3.5f, c);
            Segment(p, knee, ankle, thick * 4f, c, .90f); Joint(p, ankle, thick * 1.9f, c);
            Segment(p, ankle, ankle + new Vector2(len * .15f, side * len * .035f), thick * 1.45f, c, .92f);
        }

        private static void Tail(Painter p, float x, float y, float len, Color c, float t, int count, float amp)
        {
            var from = new Vector2(x, y);
            for (var i = 0; i < count; i++)
            {
                var q = (i + 1f) / count;
                var to = new Vector2(x - len * q, y + Mathf.Sin(t * 3f - q * 4.2f) * len * amp * (.30f + .70f * q));
                var w = len * (.14f - .009f * i);
                Segment(p, from, to, w, c, .82f - .025f * i);
                if (i < count - 1) Joint(p, to, Mathf.Max(.009f, w * .22f), c);
                from = to;
            }
        }

        private static void Segment(Painter p, Vector2 from, Vector2 to, float w, Color c, float alpha)
        {
            var d = to - from; var len = d.magnitude; if (len <= .00001f) return;
            var v = PathScratch; v.Clear();
            QuadCurve(v, new Vector2(0f, -w * .34f), new Vector2(len * .48f, -w * .48f), new Vector2(len, -w * .10f), 5);
            QuadCurve(v, v[v.Count - 1], new Vector2(len * .62f, w * .42f), new Vector2(0f, w * .30f), 5, true);
            var angle = Mathf.Atan2(d.y, d.x);
            for (var i = 0; i < v.Count; i++) v[i] = from + Rotate(v[i], angle);
            p.Polygon(v, new Color(.016f, .032f, .055f, .98f * Mathf.Clamp01(alpha)), new Color(c.r, c.g, c.b, Mathf.Clamp01(alpha)), Mathf.Max(.007f, w * .09f));
        }

        private static void Joint(Painter p, Vector2 at, float radius, Color c)
        {
            p.Circle(at, Mathf.Max(.012f, radius), new Color(.016f, .031f, .055f, .98f), c, Mathf.Max(.007f, radius * .18f));
        }

        private static void Eye(Painter p, Vector2 at, float radius, Color c)
        {
            p.Ellipse(at, radius, radius * .62f, EyeWhite, c, Mathf.Max(.007f, radius * .16f), 0f);
            p.Ellipse(at + new Vector2(radius * .18f, 0f), radius * .20f, radius * .42f, c, c, .003f, 0f);
        }

        private static void Core(Painter p, Vector2 at, float radius, Color c)
        {
            p.Gradient(at, radius * 2.2f, c, .70f);
            p.Circle(at, radius, new Color(.027f, .067f, .105f, 1f), c, Mathf.Max(.007f, radius * .15f));
        }

        private static void QuadCurve(List<Vector2> points, Vector2 a, Vector2 b, Vector2 c, int steps, bool skipFirst = false)
        {
            for (var i = skipFirst ? 1 : 0; i <= steps; i++)
            {
                var t = i / (float)steps; var u = 1f - t;
                points.Add(a * (u * u) + b * (2f * u * t) + c * (t * t));
            }
        }

        private static Vector2 Rotate(Vector2 v, float a)
        {
            var s = Mathf.Sin(a); var c = Mathf.Cos(a);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }

        private sealed class Painter
        {
            public readonly List<Vector3> Vertices = new List<Vector3>(1400);
            public readonly List<Color> Colors = new List<Color>(1400);
            public readonly List<int> Triangles = new List<int>(3000);
            private readonly List<Vector2> _shapeScratch = new List<Vector2>(24);
            private readonly int[] _ringStarts = new int[3];
            private static readonly float[] GradientRadii = { .30f, .63f, 1f };
            public void Clear() { Vertices.Clear(); Colors.Clear(); Triangles.Clear(); }

            private int Vertex(Vector2 p, Color c)
            {
                var i = Vertices.Count; Vertices.Add(new Vector3(p.x, p.y, 0f)); Colors.Add(c); return i;
            }
            private void Triangle(int a, int b, int c) { Triangles.Add(a); Triangles.Add(b); Triangles.Add(c); }

            public void Polygon(List<Vector2> points, Color fill, Color outline, float line)
            {
                if (points == null || points.Count < 3) return;
                var center = Vector2.zero; for (var i = 0; i < points.Count; i++) center += points[i];
                center /= points.Count; var ci = Vertex(center, fill); var first = Vertices.Count;
                for (var i = 0; i < points.Count; i++) Vertex(points[i], fill);
                for (var i = 0; i < points.Count; i++) Triangle(ci, first + i, first + (i + 1) % points.Count);
                if (line > 0f) StrokePath(points, outline, line, true);
            }

            public void Stroke(Vector2 a, Vector2 b, Color c, float width)
            {
                var d = b - a; if (d.sqrMagnitude < .0000001f || width <= 0f) return;
                var n = new Vector2(-d.y, d.x).normalized * width * .5f; var start = Vertices.Count;
                Vertex(a + n, c); Vertex(a - n, c); Vertex(b - n, c); Vertex(b + n, c);
                Triangle(start, start + 1, start + 2); Triangle(start, start + 2, start + 3);
            }
            public void StrokePath(List<Vector2> points, Color c, float width, bool closed)
            {
                if (points == null || points.Count < 2) return;
                var count = closed ? points.Count : points.Count - 1;
                for (var i = 0; i < count; i++) Stroke(points[i], points[(i + 1) % points.Count], c, width);
            }
            public void Circle(Vector2 at, float radius, Color fill, Color outline, float width)
            {
                var pts = _shapeScratch; pts.Clear();
                for (var i = 0; i < 20; i++) { var a = i * Mathf.PI * 2f / 20f; pts.Add(at + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius); }
                Polygon(pts, fill, outline, width);
            }
            public void Ellipse(Vector2 at, float rx, float ry, Color fill, Color outline, float width, float rotation)
            {
                var pts = _shapeScratch; pts.Clear();
                for (var i = 0; i < 20; i++) { var a = i * Mathf.PI * 2f / 20f; pts.Add(at + Rotate(new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry), rotation)); }
                Polygon(pts, fill, outline, width);
            }
            public void Gradient(Vector2 at, float radius, Color color, float alpha)
            {
                const int n = 28;
                var radii = GradientRadii;
                var starts = _ringStarts;
                var ci = Vertex(at, new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha * 1.6f)));
                for (var ring = 0; ring < radii.Length; ring++)
                {
                    starts[ring] = Vertices.Count;
                    var level = ring == 0 ? alpha * 1.6f : ring == 1 ? alpha * .48f : 0f;
                    var tint = new Color(color.r, color.g, color.b, Mathf.Clamp01(level));
                    for (var i = 0; i < n; i++)
                    {
                        var a = i * Mathf.PI * 2f / n;
                        Vertex(at + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius * radii[ring], tint);
                    }
                }
                for (var i = 0; i < n; i++) Triangle(ci, starts[0] + i, starts[0] + (i + 1) % n);
                for (var ring = 0; ring < radii.Length - 1; ring++)
                    for (var i = 0; i < n; i++)
                    {
                        var next = (i + 1) % n; var a = starts[ring] + i; var b = starts[ring] + next;
                        var c = starts[ring + 1] + next; var d = starts[ring + 1] + i;
                        Triangle(a, d, c); Triangle(a, c, b);
                    }
            }
        }
    }
}
