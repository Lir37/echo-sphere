using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Source-mapped 2.5D runtime accents for src/spheres/*Visual.ts, including src/spheres/visualHelpers.ts.
    /// Canvas-faithful 2.5D Sphere core and orbit port. The same luminous core geometry
    /// is shared by the family modules; each Sphere gets its source orbit and attachments.
    /// </summary>
    public sealed class SphereRuntimeVfx2D : MonoBehaviour
    {
        private const float R = 0.40f;
        private const float Tau = Mathf.PI * 2f;
        private static readonly Color Resonance = new Color(1f, 0.82f, 0.40f);
        private static readonly Color Disabled = new Color(1f, 0.30f, 0.44f, 1f);
        private static readonly Color CoreOutline = new Color(182f / 255f, 247f / 255f, 1f, 0.90f);
        private static readonly List<Vector2> Path = new List<Vector2>(32);
        private static Material _material;
        private EchoSphereRuntime _runtime;
        private SphereAttackAgent _agent;
        private SphereId _type;
        private Mesh _orbitMesh, _coreMesh, _detailMesh;
        private Painter _orbit, _core, _detail;
        private Transform _glow;
        private SpriteRenderer _glowSprite;
        private Color _baseColor;
        private float _time, _phase;

        public void Initialize(EchoSphereRuntime runtime, SphereAttackAgent agent)
        {
            _runtime = runtime; _agent = agent; _type = agent.Type;
            _baseColor = ColorFor(_type);
            _phase = ((int)_type + 1) * 0.6180339f;
            _glow = transform.Find("Glow");
            if (_glow != null)
            {
                _glowSprite = _glow.GetComponent<SpriteRenderer>();
                _glow.localScale = Vector3.one * 2.35f;
            }

            // The source Canvas renderer builds the body from a luminous core plus
            // orbit/emitter geometry. Disable the earlier SVG/disc prototype body so
            // it cannot cover these details or make every family look like the same ball.
            DisablePrototypeSphereLayers();
            _orbit = new Painter(); _core = new Painter(); _detail = new Painter();
            CreateLayer("Sphere Runtime Orbit", 7, out _orbitMesh);
            CreateLayer("Sphere Runtime Core", 8, out _coreMesh);
            CreateLayer("Sphere Runtime Details", 10, out _detailMesh);
            Rebuild();
        }

        private bool IsOrbitalSatelliteRenderer(SpriteRenderer renderer)
        {
            if (_type != SphereId.Orbital || renderer == null) return false;
            var node = renderer.transform;
            while (node != null && node != transform)
            {
                if (node.name.StartsWith("Orbital Combat Element", System.StringComparison.Ordinal) ||
                    node.name == "Orbital Element Glow") return true;
                node = node.parent;
            }
            return false;
        }

        private void DisablePrototypeSphereLayers()
        {
            var renderers = GetComponentsInChildren<SpriteRenderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || renderer == _glowSprite) continue;
                if (IsOrbitalSatelliteRenderer(renderer))
                {
                    // Orbital combat elements are redrawn below using the source's
                    // faceted satellite mesh; don't leave the oversized Disc prototype.
                    renderer.enabled = false;
                    continue;
                }
                renderer.enabled = false;
            }
        }

        private static Material SharedMaterial
        {
            get
            {
                if (_material == null)
                {
                    var shader = Shader.Find("Sprites/Default");
                    if (shader != null)
                    {
                        _material = new Material(shader);
                        _material.name = "ECHO SPHERE Family VFX (Unlit)";
                        _material.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
                return _material;
            }
        }

        private void CreateLayer(string name, int order, out Mesh mesh)
        {
            var layer = new GameObject(name);
            layer.transform.SetParent(transform, false);
            mesh = new Mesh { name = name + " Mesh" };
            mesh.MarkDynamic();
            layer.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = layer.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = SharedMaterial;
            renderer.sortingOrder = order;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private void LateUpdate()
        {
            if (_runtime == null || _agent == null || _runtime.IsPresentationPaused) return;
            _time += Time.deltaTime;
            Rebuild();
        }

        private void Rebuild()
        {
            if (_orbit == null || _core == null || _detail == null ||
                _orbitMesh == null || _coreMesh == null || _detailMesh == null) return;

            var disabled = _agent.NetworkDisabled;
            var resonance = !disabled && _runtime != null && _runtime.ResonanceEventActive;
            var color = disabled ? Disabled : resonance ? Resonance : _baseColor;
            // Orbital's source core pulse is sin(time * 3.0 + phase); other families retain their existing pulse.
            var pulseSpeed = _type == SphereId.Orbital ? 3f : 4.2f;
            var pulse = 0.5f + 0.5f * Mathf.Sin(_time * pulseSpeed + _phase);
            if (_glow != null) _glow.localScale = Vector3.one * (2.35f * (1f + pulse * .035f));
            if (_glowSprite != null)
                _glowSprite.color = new Color(color.r, color.g, color.b, disabled ? .05f : .10f + pulse * .045f);

            // Match src/spheres/visualHelpers.ts: a radial white-to-family-color glow,
            // a compact luminous disk and a crisp pale-cyan edge. Orbital's authored
            // source uses a smaller central core with the rings carrying its silhouette.
            var coreRadius = _type == SphereId.Orbital ? R * .70f : R;
            _core.Clear();
            _core.RadialGlow(new Vector2(-coreRadius * .08f, -coreRadius * .10f),
                coreRadius * .62f, color, .82f + pulse * .14f);
            _core.Circle(Vector2.zero, coreRadius * .42f, color, .72f, 0f);
            _core.Arc(Vector2.zero, coreRadius * .42f, coreRadius * .42f, 0f, Tau,
                CoreOutline, Mathf.Max(.014f, coreRadius * .035f), disabled ? .28f : .90f);
            Upload(_coreMesh, _core);

            _orbit.Clear();
            var spin = _time * OrbitSpeed(_type) + _phase;
            if (_type == SphereId.Orbital)
            {
                DrawSourceOrbit(_orbit, R * 1.05f, color, _time * .18f + _phase,
                    disabled ? .12f : .66f, pulse, disabled, SphereId.Orbital);
                DrawSourceOrbit(_orbit, R * .88f, color, -_time * .22f + _phase * .5f,
                    disabled ? .08f : .38f, .5f, disabled, SphereId.Orbital);
            }
            else
            {
                var radius = _type == SphereId.Aura ? R * 1.03f
                    : (_type == SphereId.Prism || _type == SphereId.Gravity || _type == SphereId.Pulse || _type == SphereId.Void)
                        ? R * 1.03f : R * 1.02f;
                var opacity = _type == SphereId.Chain ? .78f
                    : _type == SphereId.Shotgun ? .64f
                    : _type == SphereId.Void ? .58f : .66f;
                if (_type == SphereId.Sniper || _type == SphereId.Aura) opacity = .68f;
                DrawSourceOrbit(_orbit, radius, color, spin, disabled ? .12f : opacity, pulse, disabled, _type);
            }

            if (_type == SphereId.Pulse)
            {
                // The source Pulse sends out a circular wave, not a second flattened orbit.
                var q = Mathf.Repeat(_time * .42f, 1f);
                var waveRadius = R * (1.02f + q * .86f);
                _orbit.Arc(Vector2.zero, waveRadius, waveRadius, 0f, Tau, color,
                    Mathf.Max(.014f, R * .040f), disabled ? (1f - q) * .10f : (1f - q) * .36f);
            }
            Upload(_orbitMesh, _orbit);

            _detail.Clear();
            var facing = _agent.VisualRotation;
            switch (_type)
            {
                case SphereId.Standard:
                    // The source Standard Sphere is assembled from a core, stabilisation ring,
                    // and separate upper/lower crystals. The source crystal PNG is unreadable
                    // in this migration, so preserve its two-part silhouette with the same
                    // faceted terminal geometry already used by the authored Orbital family.
                    DrawTerminal(_detail, new Vector2(0f, -R * 0.66f), R * 0.92f, color, disabled ? .14f : .92f, _time);
                    DrawTerminal(_detail, new Vector2(0f, R * 0.66f), R * 0.92f, color, disabled ? .12f : .78f, -_time);
                    break;
                case SphereId.Sniper:
                    DrawEmitter(_detail, R, color, facing, 0, 0f, disabled, _time);
                    var scan = _time * .46f + _phase;
                    DrawReticle(_detail, OrbitPoint(R, scan, .34f, 1.15f), R * .10f, color, .62f);
                    break;
                case SphereId.Chain:
                    for (var i = 0; i < 4; i++) DrawConductor(_detail, R, i * Mathf.PI * .5f + _time * .28f, color, disabled ? .10f : .56f);
                    var surge = .5f + .5f * Mathf.Sin(_time * 10f);
                    _detail.Arc(Vector2.zero, coreRadius * (1.04f + surge * .08f), coreRadius * (1.04f + surge * .08f),
                        0f, Tau, color, Mathf.Max(.014f, coreRadius * .040f), disabled ? .04f : .20f + surge * .16f);
                    break;
                case SphereId.Shotgun:
                    for (var i = -1; i <= 1; i++) DrawEmitter(_detail, R, color, facing, 1, i * R * .25f, disabled);
                    for (var i = 0; i < 3; i++)
                    {
                        var a = _time * (.34f + i * .035f) + i * Tau / 3f;
                        DrawCross(_detail, OrbitPoint(R, a, .34f, 1.12f), R * .10f, color, .48f * Depth(a));
                    }
                    if (Mathf.Sin(_time * 8f) > .6f) _detail.Arc(new Vector2(R * 1.43f, 0f), R * .11f, R * .11f, 0f, Tau, color, .008f, .22f);
                    break;
                case SphereId.Aura:
                    for (var i = 0; i < 6; i++)
                    {
                        var a = i * Mathf.PI / 3f + _time * .10f;
                        DrawDiamond(_detail, OrbitPoint(R, a, .34f, 1.08f), R * .14f, color, disabled ? .07f : .48f * Depth(a), a + Mathf.PI / 2f);
                    }
                    var auraPulse = .5f + .5f * Mathf.Sin(_time * 2.7f);
                    _detail.Arc(Vector2.zero, coreRadius * (1.04f + auraPulse * .06f), coreRadius * (1.04f + auraPulse * .06f),
                        0f, Tau, color, Mathf.Max(.014f, coreRadius * .040f), disabled ? .04f : .16f + auraPulse * .08f);
                    break;
                case SphereId.Orbital:
                    DrawTerminal(_detail, new Vector2(0f, -R * 1.02f), R, color, .76f, _time);
                    DrawTerminal(_detail, new Vector2(0f, R * 1.02f), R, color, .62f, -_time);
                    if (resonance) _detail.Arc(Vector2.zero, R * 1.34f, R * 1.34f, _time * .65f, Mathf.PI * 1.15f, Resonance, .010f, .42f);
                    break;
                case SphereId.Prism:
                    DrawEmitter(_detail, R, color, facing, 2, 0f, disabled);
                    for (var i = 0; i < 2; i++)
                    {
                        var a = _time * (.30f + i * .05f) + i * Mathf.PI;
                        DrawDiamond(_detail, OrbitPoint(R, a, .34f, 1.10f), R * .09f, color, .42f * Depth(a), a + Mathf.PI / 2f);
                    }
                    break;
                case SphereId.Gravity:
                    for (var i = 0; i < 5; i++)
                    {
                        var a = i * Tau / 5f + _time * .08f;
                        DrawDiamond(_detail, OrbitPoint(R, a, .34f, 1.08f), R * .12f, color, disabled ? .06f : .44f * Depth(a), a);
                    }
                    var pull = .5f + .5f * Mathf.Sin(_time * 3.1f);
                    _detail.Arc(Vector2.zero, coreRadius * (.98f + pull * .05f), coreRadius * (.98f + pull * .05f),
                        0f, Tau, color, Mathf.Max(.014f, coreRadius * .038f), disabled ? .04f : .16f + pull * .08f);
                    break;
                case SphereId.Pulse:
                    for (var i = 0; i < 4; i++)
                    {
                        var a = _time * .28f + i * Mathf.PI / 2f;
                        DrawPlate(_detail, OrbitPoint(R, a, .34f, 1.10f), R * .17f, color,
                            .36f + .20f * (.5f + .5f * Mathf.Sin(_time * 2.8f + i)), a + Mathf.PI / 2f);
                    }
                    break;
                case SphereId.Void:
                    DrawEmitter(_detail, R, color, facing, 3, 0f, disabled);
                    for (var i = 0; i < 4; i++)
                    {
                        var a = -_time * (.22f + i * .025f) + i * Mathf.PI / 2f;
                        var rr = R * (.07f + i * .012f);
                        _detail.Arc(OrbitPoint(R, a, .34f, 1.11f), rr, rr, Mathf.PI * .2f, Mathf.PI * 1.6f, color, .008f, .32f * Depth(a));
                    }
                    break;
            }
            if (_type == SphereId.Orbital)
            {
                DrawOrbitalSatellites(_detail, disabled, resonance);
                DrawOrbitalAttackFlash(_detail, disabled);
            }

            if (_agent.NetworkDisabled)
            {
                var disabledColor = new Color(1f, 0.22f, 0.39f, 0.94f);
                _detail.Line(new Vector2(-R * 1.18f, -R * 0.54f), new Vector2(-R * 0.52f, -R * 0.02f), disabledColor, 0.025f);
                _detail.Line(new Vector2(-R * 1.18f, R * 0.54f), new Vector2(-R * 0.52f, R * 0.02f), disabledColor, 0.025f);
                _detail.Line(new Vector2(R * 1.18f, -R * 0.54f), new Vector2(R * 0.52f, -R * 0.02f), disabledColor, 0.025f);
                _detail.Line(new Vector2(R * 1.18f, R * 0.54f), new Vector2(R * 0.52f, R * 0.02f), disabledColor, 0.025f);
                _detail.Arc(Vector2.zero, R * 1.25f, R * 0.82f, _time * 2.1f, Mathf.PI * 1.4f, disabledColor, 0.016f, 0.62f);
            }
            Upload(_detailMesh, _detail);
        }

        
        // src/spheres/orbitalVisual.ts:drawSatellite and drawOrbitalElementTrail.
        // Render geometry at the combat simulation's existing element positions;
        // this method does not move elements or participate in hit detection.
        private void DrawOrbitalSatellites(Painter p, bool disabled, bool resonance)
        {
            // Source renderOrbitalSphereAttackersVfx returns while the Sphere is disabled.
            if (_agent == null || _runtime == null || disabled) return;
            var branch = _runtime.GetSphereBranch(SphereId.Orbital);
            var blade = branch == "orbital_blade";
            var element = SphereElementalRules.ResolveElement(branch, _runtime.GetSphereFinal(SphereId.Orbital));
            var elementColor = ElementVisualColor(element);
            var satelliteColor = element != SphereElement.None ? elementColor : (resonance ? Resonance : _baseColor);
            var count = _agent.OrbitalElementCount;
            var size = R * .20f;
            var sourcePixel = SpherePlacementRules.SourcePixelsToWorld;

            for (var i = 0; i < count; i++)
            {
                var node = _agent.GetOrbitalElementAt(i);
                if (node == null) continue;
                var pos3 = transform.InverseTransformPoint(node.position);
                var center = new Vector2(pos3.x, pos3.y);
                // Source Canvas +Y points down. Convert its orbital angle to Unity's +Y-up.
                var angle = Mathf.Atan2(-center.y, center.x);
                var depth = .74f + .26f * ((Mathf.Sin(angle) + 1f) * .5f);
                var rotation = -(blade ? angle : angle + Mathf.PI / 4f);

                p.Circle(center, size * .62f, satelliteColor, disabled ? .03f : depth * .30f, 0f);
                Path.Clear();
                AddOrbitalPoint(Path, center, size * -.56f, size * .14f, rotation);
                AddOrbitalPoint(Path, center, size * .78f, 0f, rotation);
                AddOrbitalPoint(Path, center, size * -.34f, size * -.15f, rotation);
                AddOrbitalPoint(Path, center, size * -.02f, 0f, rotation);
                var face = new Color(7f / 255f, 17f / 255f, 29f / 255f, disabled ? .20f : depth * .92f);
                var outline = new Color(satelliteColor.r, satelliteColor.g, satelliteColor.b,
                    disabled ? .22f : depth * .92f);
                p.Polygon(Path, face, outline, sourcePixel);

                if (blade)
                {
                    var a = Rotate(new Vector2(-size * .30f, 0f), rotation) + center;
                    var b = Rotate(new Vector2(size * .54f, 0f), rotation) + center;
                    p.Line(a, b, Color.white, .72f * sourcePixel, disabled ? .10f : depth * .72f);
                    var c = Rotate(new Vector2(-size * .42f, size * .10f), rotation) + center;
                    var d = Rotate(new Vector2(-size * .62f, 0f), rotation) + center;
                    var e = Rotate(new Vector2(-size * .42f, -size * .10f), rotation) + center;
                    p.Line(c, d, outline, sourcePixel * .72f, disabled ? .10f : depth * .50f);
                    p.Line(d, e, outline, sourcePixel * .72f, disabled ? .10f : depth * .50f);
                }
                else
                {
                    var highlight = Rotate(new Vector2(-size * .10f, size * .12f), rotation) + center;
                    p.Circle(highlight, size * .16f, Color.white, disabled ? .12f : depth * .90f, 0f);
                }

                if (element != SphereElement.None && !disabled)
                {
                    p.Circle(center, size * .18f, elementColor, depth * .22f, .70f * sourcePixel);
                    p.Circle(center, size * .095f, elementColor, depth * .22f, 0f);
                    DrawOrbitalElementTrail(p, center, size, angle, depth, element, blade);
                }
            }
        }

        // Source orbitalVisual.ts: the auraTimer edge produces a 0.28 s strike trail and white contact flash.
        // This is presentation-only and reads the existing Orbital cadence/rotation and live enemies.
        private void DrawOrbitalAttackFlash(Painter p, bool disabled)
        {
            if (disabled || _agent == null || _runtime == null) return;
            var timer = _agent.OrbitalAttackFlashTimer;
            if (timer <= 0f) return;

            const float duration = 0.28f;
            var q = 1f - Mathf.Clamp01(timer / duration);
            var strikePhase = _agent.OrbitalRotation + (1f - Mathf.Pow(1f - q, 3f)) * 0.95f;
            var firstElement = _agent.GetOrbitalElementAt(0);
            var radius = firstElement != null
                ? Vector2.Distance(transform.position, firstElement.position)
                : R * 1.68f;
            var strike = new Vector2(Mathf.Cos(strikePhase) * radius, -Mathf.Sin(strikePhase) * radius);
            var flashPosition = strike;
            var nearest = float.PositiveInfinity;

            for (var i = 0; i < _runtime.ActiveEnemyCount; i++)
            {
                var enemy = _runtime.GetActiveEnemyAt(i);
                if (enemy == null || !enemy.IsAlive) continue;
                var enemyLocal = (Vector2)transform.InverseTransformPoint(enemy.transform.position);
                var distance = Vector2.Distance(enemyLocal, strike);
                if (distance < nearest && distance <= R * 1.30f)
                {
                    nearest = distance;
                    flashPosition = enemyLocal;
                }
            }

            var streakRotation = -(strikePhase + Mathf.PI * 0.5f);
            var streakStart = strike + Rotate(new Vector2(-R * 0.32f, 0f), streakRotation);
            var streakEnd = strike + Rotate(new Vector2(-R * 0.04f, 0f), streakRotation);
            p.Line(streakStart, streakEnd, Color.white, SpherePlacementRules.SourcePixelsToWorld,
                (1f - q) * 0.32f);
            p.Circle(flashPosition, R * 0.07f, Color.white, 0.65f * (1f - q), 0f);
        }

        private static void AddOrbitalPoint(List<Vector2> points, Vector2 center, float x, float y, float rotation)
        {
            points.Add(center + Rotate(new Vector2(x, y), rotation));
        }

        private static Color ElementVisualColor(SphereElement element)
        {
            switch (element)
            {
                case SphereElement.Fire: return Hex("#ff743d");
                case SphereElement.Freeze: return Hex("#d9f6ff");
                case SphereElement.Poison: return Hex("#72f08e");
                default: return Color.white;
            }
        }

        private static void DrawOrbitalElementTrail(Painter p, Vector2 center, float size, float angle,
            float alpha, SphereElement element, bool blade)
        {
            var trailScale = blade ? 1f : .86f;
            var color = ElementVisualColor(element);
            var width = Mathf.Max(SpherePlacementRules.SourcePixelsToWorld, size * .10f);
            if (element == SphereElement.Fire)
            {
                var previous = Rotate(new Vector2(-size * .34f * trailScale, 0f), -angle) + center;
                for (var i = 1; i <= 6; i++)
                {
                    var t = i / 6f;
                    var x = -size * (.34f + .94f * t) * trailScale;
                    var y = size * .18f * Mathf.Sin(t * Mathf.PI);
                    var next = Rotate(new Vector2(x, y), -angle) + center;
                    p.Line(previous, next, color, width, alpha * .82f);
                    previous = next;
                }
                p.Line(Rotate(new Vector2(-size * .50f, size * .08f), -angle) + center,
                    Rotate(new Vector2(-size * .96f, 0f), -angle) + center, color, width * .72f, alpha * .68f);
                p.Line(Rotate(new Vector2(-size * .96f, 0f), -angle) + center,
                    Rotate(new Vector2(-size * .54f, -size * .10f), -angle) + center, color, width * .72f, alpha * .68f);
            }
            else if (element == SphereElement.Freeze)
            {
                for (var side = -1; side <= 1; side += 2)
                {
                    p.Line(Rotate(new Vector2(-size * .26f * trailScale, -side * size * .05f), -angle) + center,
                        Rotate(new Vector2(-size * .88f * trailScale, -side * size * .18f), -angle) + center, color, width, alpha * .86f);
                    p.Line(Rotate(new Vector2(-size * .88f * trailScale, -side * size * .18f), -angle) + center,
                        Rotate(new Vector2(-size * 1.28f * trailScale, -side * size * .03f), -angle) + center, color, width, alpha * .86f);
                }
                p.Line(Rotate(new Vector2(-size * .78f, size * .22f), -angle) + center,
                    Rotate(new Vector2(-size * 1.02f, 0f), -angle) + center, color, width * .78f, alpha * .78f);
                p.Line(Rotate(new Vector2(-size * 1.02f, 0f), -angle) + center,
                    Rotate(new Vector2(-size * .78f, -size * .22f), -angle) + center, color, width * .78f, alpha * .78f);
            }
            else
            {
                var previous = Rotate(new Vector2(-size * .30f * trailScale, 0f), -angle) + center;
                for (var i = 1; i <= 6; i++)
                {
                    var t = i / 6f;
                    var x = -size * (.30f + .85f * t);
                    var y = -size * .20f * Mathf.Sin(t * Mathf.PI);
                    var next = Rotate(new Vector2(x, y), -angle) + center;
                    p.Line(previous, next, color, width * .95f, alpha * .84f);
                    previous = next;
                }
                for (var i = 0; i < 3; i++)
                {
                    var dot = Rotate(new Vector2(-size * (.58f + i * .25f), -(i - 1) * size * .14f), -angle) + center;
                    p.Circle(dot, Mathf.Max(SpherePlacementRules.SourcePixelsToWorld * .9f, size * (.08f - i * .015f)),
                        color, alpha * .42f, 0f);
                }
            }
        }

private void Upload(Mesh mesh, Painter painter)
        {
            mesh.Clear(); mesh.SetVertices(painter.Vertices); mesh.SetColors(painter.Colors);
            mesh.SetTriangles(painter.Triangles, 0); mesh.RecalculateBounds();
        }

        private static Vector2 OrbitPoint(float radius, float angle, float flatten, float radiusScale)
        {
            return EllipsePoint(radius * radiusScale, radius * flatten * radiusScale, angle);
        }

        private static void DrawSourceOrbit(Painter p, float radius, Color color, float rotation,
            float opacity, float pulse, bool disabled, SphereId mode)
        {
            var rx = radius * 1.16f;
            var ry = radius * .34f;
            var majorWidth = Mathf.Max(.90f * SpherePlacementRules.SourcePixelsToWorld, radius * .040f);
            var minorWidth = Mathf.Max(.55f * SpherePlacementRules.SourcePixelsToWorld, radius * .022f);

            if (mode == SphereId.Chain)
            {
                DrawLightningEllipse(p, rx, ry, rotation, color, opacity, pulse);
                DrawLightningEllipse(p, radius * .98f, radius * .28f, rotation, color, opacity * .36f, pulse + .5f);
                return;
            }

            if (mode == SphereId.Void)
            {
                for (var i = 0; i < 7; i++)
                {
                    var angle = i * Tau / 7f + .08f;
                    var span = i % 2 == 0 ? .58f : .34f;
                    var alpha = opacity * (i % 3 == 0 ? .94f : .72f) * (disabled ? .22f : 1f);
                    TraceEllipse(p, rx, ry, angle, angle + span, rotation,
                        .70f * SpherePlacementRules.SourcePixelsToWorld, color,
                        Mathf.Max(.8f * SpherePlacementRules.SourcePixelsToWorld, radius * .030f), alpha);
                }
                TraceEllipse(p, radius * 1.03f, radius * .295f, Mathf.PI * .16f, Mathf.PI * 1.84f,
                    rotation, .35f * SpherePlacementRules.SourcePixelsToWorld, color,
                    Mathf.Max(.55f * SpherePlacementRules.SourcePixelsToWorld, radius * .018f), opacity * .42f * (disabled ? .22f : 1f));
                for (var i = 0; i < 3; i++)
                {
                    var angle = -rotation * .72f + i * Tau / 3f;
                    var at = Rotate(EllipsePoint(rx * .99f, ry * .99f, angle), rotation);
                    DrawVoidOrbitFragment(p, at, radius, color, opacity * .72f * (disabled ? .22f : 1f), rotation);
                }
                return;
            }

            TraceEllipse(p, rx, ry, 0f, Tau, rotation, 0f, color, majorWidth, opacity);
            TraceEllipse(p, radius * 1.00f, radius * .285f, Mathf.PI * .10f, Mathf.PI * 1.92f,
                rotation, 0f, color, minorWidth, opacity * .44f);

            if (mode == SphereId.Sniper)
            {
                var tickWidth = Mathf.Max(.60f * SpherePlacementRules.SourcePixelsToWorld, radius * .020f);
                for (var i = -2; i <= 2; i++)
                {
                    var a = i * .28f;
                    var point = EllipsePoint(rx, ry, a);
                    var normal = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    var from = Rotate(point - normal * radius * .045f, rotation);
                    var to = Rotate(point + normal * radius * .075f, rotation);
                    p.Line(from, to, color, tickWidth, opacity * .68f);
                }
            }
            else if (mode == SphereId.Shotgun)
            {
                var width = Mathf.Max(.70f * SpherePlacementRules.SourcePixelsToWorld, radius * .024f);
                for (var i = 0; i < 3; i++)
                {
                    var a = -.44f + i * .44f;
                    var point = Rotate(EllipsePoint(rx, ry, a), rotation);
                    p.Arc(point, radius * .065f, radius * .065f, 0f, Tau, color, width, opacity * .72f);
                }
            }
            else if (mode == SphereId.Aura)
            {
                var width = Mathf.Max(.60f * SpherePlacementRules.SourcePixelsToWorld, radius * .018f);
                for (var i = 0; i < 6; i++)
                {
                    var a = i * Mathf.PI / 3f + pulse * .10f;
                    TraceEllipse(p, rx * .96f, ry * .96f, a - .26f, a + .26f, rotation,
                        0f, color, width, opacity * .64f);
                }
            }
            else if (mode == SphereId.Prism)
            {
                var width = Mathf.Max(.75f * SpherePlacementRules.SourcePixelsToWorld, radius * .024f);
                var start = .08f;
                var end = Mathf.PI * 1.95f;
                var dash = radius * .15f / (rx * 1.01f);
                var gap = radius * .07f / (rx * 1.01f);
                for (var a = start; a < end; a += dash + gap)
                    TraceEllipse(p, rx * 1.01f, ry * 1.02f, a, Mathf.Min(end, a + dash), rotation,
                        0f, color, width, opacity * .74f);
            }
            else if (mode == SphereId.Gravity)
            {
                var width = Mathf.Max(.70f * SpherePlacementRules.SourcePixelsToWorld, radius * .026f);
                TraceEllipse(p, rx * .88f, ry * .78f, .25f, Mathf.PI * 1.78f, rotation,
                    0f, color, width, opacity * .56f);
                TraceEllipse(p, rx * 1.05f, ry * .86f, Mathf.PI * 1.08f, Mathf.PI * 2.64f, rotation,
                    0f, color, width, opacity * .56f);
            }
            else if (mode == SphereId.Orbital)
            {
                var width = Mathf.Max(.65f * SpherePlacementRules.SourcePixelsToWorld, radius * .022f);
                TraceEllipse(p, rx * .92f, ry * .84f, Mathf.PI * .12f, Mathf.PI * 1.86f, rotation,
                    0f, color, width, opacity * .52f);
            }
        }

        private static void DrawLightningEllipse(Painter p, float rx, float ry, float rotation,
            Color color, float alpha, float pulse)
        {
            const int segments = 20;
            var previous = Rotate(new Vector2(rx, 0f), rotation);
            for (var i = 1; i <= segments; i++)
            {
                var a = i * Tau / segments;
                var jitter = Mathf.Sin(i * 3.7f + pulse * 5.2f) * rx * .025f;
                var local = new Vector2(Mathf.Cos(a) * (rx + jitter), Mathf.Sin(a) * (ry + jitter * .34f));
                var next = Rotate(local, rotation);
                p.Line(previous, next, color, Mathf.Max(.8f * SpherePlacementRules.SourcePixelsToWorld, rx * .028f), alpha);
                previous = next;
            }
        }

        private static void TraceEllipse(Painter p, float rx, float ry, float start, float end,
            float rotation, float wobble, Color color, float width, float alpha)
        {
            var sweep = end - start;
            if (Mathf.Abs(sweep) < .0001f) return;
            var segments = Mathf.Max(2, Mathf.CeilToInt(42f * Mathf.Abs(sweep) / Tau));
            var a0 = start;
            var n0 = wobble == 0f ? 0f : Mathf.Sin(a0 * 7f + .35f) * wobble;
            var previous = Rotate(new Vector2(Mathf.Cos(a0) * (rx + n0), Mathf.Sin(a0) * (ry + n0 * .34f)), rotation);
            for (var i = 1; i <= segments; i++)
            {
                var a = start + sweep * i / segments;
                var n = wobble == 0f ? 0f : Mathf.Sin(a * 7f + .35f) * wobble;
                var point = Rotate(new Vector2(Mathf.Cos(a) * (rx + n), Mathf.Sin(a) * (ry + n * .34f)), rotation);
                p.Line(previous, point, color, width, alpha);
                previous = point;
            }
        }

        private static void DrawVoidOrbitFragment(Painter p, Vector2 at, float radius, Color color, float alpha, float rotation)
        {
            Path.Clear();
            Path.Add(at + Rotate(new Vector2(radius * .10f, 0f), rotation));
            Path.Add(at + Rotate(new Vector2(0f, radius * .055f), rotation));
            Path.Add(at + Rotate(new Vector2(-radius * .10f, 0f), rotation));
            Path.Add(at + Rotate(new Vector2(0f, -radius * .055f), rotation));
            var tint = new Color(color.r, color.g, color.b, Mathf.Clamp01(color.a * alpha));
            p.Polygon(Path, tint, tint, 0f);
        }

        private static float OrbitSpeed(SphereId t)
        {
            switch (t)
            {
                case SphereId.Sniper: return .20f; case SphereId.Chain: return .17f;
                case SphereId.Shotgun: return .18f; case SphereId.Aura: return .14f;
                case SphereId.Orbital: return .18f; case SphereId.Prism: return .16f;
                case SphereId.Gravity: return -.12f; case SphereId.Pulse: return .143f;
                case SphereId.Void: return .11f; default: return .15f;
            }
        }

        private static Color ColorFor(SphereId t)
        {
            switch (t)
            {
                case SphereId.Sniper: return Hex("#e86cff"); case SphereId.Chain: return Hex("#ffe25b");
                case SphereId.Shotgun: return Hex("#ff8f3d"); case SphereId.Aura: return Hex("#57e6b4");
                case SphereId.Orbital: return Hex("#8ef0ff"); case SphereId.Prism: return Hex("#ff8de1");
                case SphereId.Gravity: return Hex("#a58cff"); case SphereId.Pulse: return Hex("#ffd35a");
                case SphereId.Void: return Hex("#c28cff"); default: return Color.white;
            }
        }
        private static Color Hex(string v) { Color c; return ColorUtility.TryParseHtmlString(v, out c) ? c : Color.white; }
        private static Vector2 EllipsePoint(float x, float y, float a) { return new Vector2(Mathf.Cos(a) * x, Mathf.Sin(a) * y); }
        private static float Depth(float a) { return .74f + .26f * ((Mathf.Sin(a) + 1f) * .5f); }
        private static Vector2 Rotate(Vector2 v, float a) { var s = Mathf.Sin(a); var c = Mathf.Cos(a); return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c); }

        // kind: 0 sniper needle, 1 shotgun muzzle, 2 prism emitter, 3 void emitter.
        private static void DrawEmitter(Painter p, float r, Color c, float facing, int kind, float y, bool disabled, float time = 0f)
        {
            var center = new Vector2(r * (kind == 0 ? 1.22f : kind == 3 ? 1.20f : 1.18f), y);
            Path.Clear();
            if (kind == 0)
            {
                Path.Add(new Vector2(0f, -r * .24f)); Path.Add(new Vector2(r * .18f, -r * .05f));
                Path.Add(new Vector2(r * .13f, r * .24f)); Path.Add(new Vector2(0f, r * .34f));
                Path.Add(new Vector2(-r * .13f, r * .24f)); Path.Add(new Vector2(-r * .18f, -r * .05f));
            }
            else if (kind == 1)
            {
                Path.Add(new Vector2(-r * .02f, -r * .10f)); Path.Add(new Vector2(r * .22f, -r * .11f));
                Path.Add(new Vector2(r * .30f, 0f)); Path.Add(new Vector2(r * .22f, r * .11f)); Path.Add(new Vector2(-r * .02f, r * .10f));
            }
            else if (kind == 2)
            {
                Path.Add(new Vector2(0f, -r * .20f)); Path.Add(new Vector2(r * .17f, -r * .06f));
                Path.Add(new Vector2(r * .27f, 0f)); Path.Add(new Vector2(r * .17f, r * .06f));
                Path.Add(new Vector2(0f, r * .20f)); Path.Add(new Vector2(r * .08f, 0f));
            }
            else
            {
                Path.Add(new Vector2(-r * .01f, -r * .19f)); Path.Add(new Vector2(r * .21f, -r * .08f));
                Path.Add(new Vector2(r * .27f, 0f)); Path.Add(new Vector2(r * .21f, r * .08f));
                Path.Add(new Vector2(-r * .01f, r * .19f)); Path.Add(new Vector2(r * .07f, 0f));
            }
            var rot = (kind == 0 ? -Mathf.PI / 2f : 0f) + facing;
            for (var i = 0; i < Path.Count; i++) Path[i] = center + Rotate(Path[i], rot);
            var shapeAlpha = disabled ? .14f : kind == 3 ? .97f : .94f;
            var fill = new Color(.027f, .067f, .105f, shapeAlpha);
            var outline = new Color(c.r, c.g, c.b, shapeAlpha);
            p.Polygon(Path, fill, outline, .009f);
            if (kind == 0)
            {
                p.Line(center + Rotate(new Vector2(0f, -r * .18f), rot), center + Rotate(new Vector2(0f, r * .16f), rot),
                    Color.white, .006f, disabled ? .08f : .70f);
                var scan = .10f + .05f * Mathf.Sin(time * 7f);
                p.Circle(center, r * .13f, c, disabled ? .04f : scan, 0f);
            }
            else if (kind == 1)
            {
                var ringCenter = center + Rotate(new Vector2(r * .25f, 0f), rot);
                p.Arc(ringCenter, r * .055f, r * .055f, 0f, Tau, Color.white, .006f, disabled ? .07f : .72f);
            }
            else if (kind == 2)
            {
                p.Line(center + Rotate(new Vector2(r * .09f, -r * .10f), rot), center + Rotate(new Vector2(r * .21f, 0f), rot),
                    Color.white, .006f, disabled ? .06f : .68f);
                p.Line(center + Rotate(new Vector2(r * .21f, 0f), rot), center + Rotate(new Vector2(r * .09f, r * .10f), rot),
                    Color.white, .006f, disabled ? .06f : .68f);
            }
            else p.Arc(center + Rotate(new Vector2(r * .18f, 0f), rot), r * .07f, r * .07f, 0f, Tau,
                new Color(.92f, .875f, 1f), .006f, disabled ? .04f : .65f);
        }

        private static void DrawTerminal(Painter p, Vector2 pos, float r, Color c, float alpha, float phase)
        {
            Path.Clear();
            Path.Add(new Vector2(0f, -r * .28f)); Path.Add(new Vector2(r * .16f, -r * .05f));
            Path.Add(new Vector2(r * .11f, r * .24f)); Path.Add(new Vector2(0f, r * .31f));
            Path.Add(new Vector2(-r * .11f, r * .24f)); Path.Add(new Vector2(-r * .16f, -r * .05f));
            for (var i = 0; i < Path.Count; i++) Path[i] = pos + Rotate(Path[i], phase * .08f);
            p.Polygon(Path, new Color(.027f, .067f, .105f, alpha), new Color(c.r, c.g, c.b, alpha), .009f);
            Path.Clear();
            Path.Add(pos + Rotate(new Vector2(0f, -r * .16f), phase * .08f));
            Path.Add(pos + Rotate(new Vector2(r * .07f, 0f), phase * .08f));
            Path.Add(pos + Rotate(new Vector2(0f, r * .14f), phase * .08f));
            Path.Add(pos + Rotate(new Vector2(-r * .07f, 0f), phase * .08f));
            p.Polygon(Path, new Color(1f, 1f, 1f, alpha * .72f), Color.white, 0f);
        }

        private static void DrawConductor(Painter p, float r, float angle, Color c, float alpha)
        {
            var pos = OrbitPoint(r, angle, .34f, 1.10f); var rot = angle + Mathf.PI / 2f;
            var a = pos + Rotate(new Vector2(-r * .08f, 0f), rot);
            var b = pos + Rotate(new Vector2(-r * .01f, -r * .07f), rot);
            var d = pos + Rotate(new Vector2(r * .07f, r * .03f), rot);
            var e = pos + Rotate(new Vector2(r * .14f, -r * .02f), rot);
            var f = pos + Rotate(new Vector2(r * .21f, r * .06f), rot);
            var al = alpha * Depth(angle);
            p.Line(a,b,c,.009f,al); p.Line(b,d,c,.009f,al); p.Line(d,e,c,.009f,al); p.Line(e,f,c,.009f,al);
        }
        private static void DrawDiamond(Painter p, Vector2 at, float r, Color c, float alpha, float angle)
        {
            Path.Clear(); Path.Add(at + Rotate(new Vector2(0f,-r),angle)); Path.Add(at + Rotate(new Vector2(r*.72f,0f),angle));
            Path.Add(at + Rotate(new Vector2(0f,r),angle)); Path.Add(at + Rotate(new Vector2(-r*.72f,0f),angle));
            p.Polygon(Path,new Color(.027f,.067f,.105f,alpha*.82f),new Color(c.r,c.g,c.b,alpha),.008f);
        }
        private static void DrawPlate(Painter p, Vector2 at, float r, Color c, float alpha, float angle)
        {
            Path.Clear(); Path.Add(at+Rotate(new Vector2(-r*.48f,-r*.60f),angle)); Path.Add(at+Rotate(new Vector2(r*.48f,-r*.35f),angle));
            Path.Add(at+Rotate(new Vector2(r*.48f,r*.35f),angle)); Path.Add(at+Rotate(new Vector2(-r*.48f,r*.60f),angle));
            p.Polygon(Path,new Color(.027f,.067f,.105f,alpha*.85f),new Color(c.r,c.g,c.b,alpha),.007f);
        }
        private static void DrawReticle(Painter p, Vector2 at, float r, Color c, float alpha)
        {
            p.Arc(at,r,r,0f,Tau,c,.006f,alpha);
            p.Line(at+Vector2.left*r*1.8f,at+Vector2.right*r*1.8f,c,.005f,alpha);
            p.Line(at+Vector2.down*r*1.8f,at+Vector2.up*r*1.8f,c,.005f,alpha);
        }
        private static void DrawCross(Painter p, Vector2 at, float r, Color c, float alpha)
        {
            p.Line(at+Vector2.left*r,at+Vector2.right*r,c,.006f,alpha);
        }

        private sealed class Painter
        {
            public readonly List<Vector3> Vertices = new List<Vector3>(600);
            public readonly List<Color> Colors = new List<Color>(600);
            public readonly List<int> Triangles = new List<int>(1200);
            public void Clear() { Vertices.Clear(); Colors.Clear(); Triangles.Clear(); }
            private int V(Vector2 p,Color c){var i=Vertices.Count;Vertices.Add(new Vector3(p.x,p.y,0f));Colors.Add(c);return i;}
            private void T(int a,int b,int c){Triangles.Add(a);Triangles.Add(b);Triangles.Add(c);}
            public void Polygon(List<Vector2> pts,Color fill,Color outline,float width)
            {
                if(pts==null||pts.Count<3)return;
                var center=Vector2.zero;for(var i=0;i<pts.Count;i++)center+=pts[i];center/=pts.Count;
                var ci=V(center,fill);var start=Vertices.Count;for(var i=0;i<pts.Count;i++)V(pts[i],fill);
                for(var i=0;i<pts.Count;i++)T(ci,start+i,start+(i+1)%pts.Count);
                if(width>0f)for(var i=0;i<pts.Count;i++)Line(pts[i],pts[(i+1)%pts.Count],outline,width);
            }
            public void Line(Vector2 a,Vector2 b,Color c,float width,float alpha=1f)
            {
                var d=b-a;if(d.sqrMagnitude<.0000001f||width<=0f)return;
                var n=new Vector2(-d.y,d.x).normalized*width*.5f;var tint=new Color(c.r,c.g,c.b,Mathf.Clamp01(c.a*alpha));
                var i=Vertices.Count;V(a+n,tint);V(a-n,tint);V(b-n,tint);V(b+n,tint);T(i,i+1,i+2);T(i,i+2,i+3);
            }
            public void Circle(Vector2 at,float radius,Color c,float alpha,float outline)
            {
                Path.Clear();for(var i=0;i<20;i++){var a=i*Tau/20f;Path.Add(at+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius);}
                Polygon(Path,new Color(c.r,c.g,c.b,alpha),c,outline);
            }
            public void Ellipse(Vector2 center,float rx,float ry,float angle,Color c,float width,float gap,float alpha)
            {
                const int count=64;
                for(var i=0;i<count;i++)
                {
                    var q=i/(float)count;if(gap>0f&&q>.46f-gap*.5f&&q<.46f+gap*.5f)continue;
                    var a0=i*Tau/count;var a1=(i+1)*Tau/count;
                    var a=center+Rotate(new Vector2(Mathf.Cos(a0)*rx,Mathf.Sin(a0)*ry),angle);
                    var b=center+Rotate(new Vector2(Mathf.Cos(a1)*rx,Mathf.Sin(a1)*ry),angle);
                    Line(a,b,c,width,alpha);
                }
            }
            public void Arc(Vector2 center,float rx,float ry,float start,float sweep,Color c,float width,float alpha)
            {
                const int max=36;var count=Mathf.Max(2,Mathf.CeilToInt(max*Mathf.Abs(sweep)/Tau));
                var from=center+new Vector2(Mathf.Cos(start)*rx,Mathf.Sin(start)*ry);
                for(var i=1;i<=count;i++){var a=start+sweep*i/count;var to=center+new Vector2(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry);Line(from,to,c,width,alpha);from=to;}
            }
            public void RadialGlow(Vector2 at,float radius,Color color,float strength)
            {
                const int segments=32;
                var centerColor=Color.Lerp(color,Color.white,.98f);
                centerColor.a=Mathf.Clamp01(strength*.98f);
                var center=V(at,centerColor);
                var radii=CoreGlowRadii;
                var alphas=CoreGlowAlphas;
                var highlights=CoreGlowHighlights;
                for(var ring=0;ring<radii.Length;ring++)
                {
                    _ringStarts[ring]=Vertices.Count;
                    var tint=Color.Lerp(color,Color.white,highlights[ring]);
                    tint.a=alphas[ring]*strength;
                    var r=radius*radii[ring];
                    for(var i=0;i<segments;i++)
                    {
                        var a=i*Tau/segments;
                        V(at+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,tint);
                    }
                }
                for(var i=0;i<segments;i++) T(center,_ringStarts[0]+i,_ringStarts[0]+(i+1)%segments);
                for(var ring=0;ring<radii.Length-1;ring++)
                    for(var i=0;i<segments;i++)
                    {
                        var next=(i+1)%segments;
                        var a=_ringStarts[ring]+i;
                        var b=_ringStarts[ring]+next;
                        var c=_ringStarts[ring+1]+next;
                        var d=_ringStarts[ring+1]+i;
                        T(a,d,c);T(a,c,b);
                    }
            }
            private static readonly float[] CoreGlowRadii={.20f,.48f,.82f,1f};
            private static readonly float[] CoreGlowAlphas={.96f,.78f,.34f,0f};
            private static readonly float[] CoreGlowHighlights={.94f,.25f,.06f,0f};
            private readonly int[] _ringStarts=new int[4];
        }
    }
}
