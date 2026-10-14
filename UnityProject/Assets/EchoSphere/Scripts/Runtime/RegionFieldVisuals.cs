using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Source-mapped 2.5D region field from src/region.ts and renderer.ts.
    /// Coordinates are converted from the source 720 px viewport baseline to
    /// the Unity camera's 12.6 world-unit vertical span. This is presentation
    /// only: it never reads or consumes the gameplay RNG.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RegionFieldVisuals : MonoBehaviour
    {
        private const float SourcePixelsToWorld = 12.6f / 720f;
        private const int CircleSegments = 96;
        private const int SeamSegments = 10;

        private sealed class PocketVisual
        {
            public Vector2 Center;
            public float Radius;
            public Color Accent;
            public bool Active;
            public readonly List<LineRenderer> Seams = new List<LineRenderer>();
            public readonly List<SpriteRenderer> Markers = new List<SpriteRenderer>();
        }

        private sealed class PoiVisual
        {
            public int Id;
            public string Type;
            public Vector2 Position;
            public float ExpiresAt;
            public GameObject Root;
            public SpriteRenderer Glow;
            public LineRenderer Diamond;
            public TextMesh Label;
        }

        private readonly List<PocketVisual> _pockets = new List<PocketVisual>();
        private readonly List<PoiVisual> _pois = new List<PoiVisual>();
        private float _nextPoiAt = 70f;
        private EchoSphereRuntime _runtime;
        private Material _lineMaterial;
        private Transform _fieldRoot;
        private bool _initialized;
        private float _seamTime;
        public EchoRegionId ActiveRegionId { get; private set; } = EchoRegionId.ResonanceBasin;

        public void Initialize(EchoSphereRuntime runtime, EchoRegionId regionId = EchoRegionId.ResonanceBasin)
        {
            if (runtime == null) return;
            _runtime = runtime;
            if (!_initialized)
            {
                var shader = Shader.Find("Sprites/Default");
                if (shader == null) return;

                _lineMaterial = new Material(shader)
                {
                    name = "ECHO SPHERE / Region Field Lines",
                    hideFlags = HideFlags.DontSave
                };
                var root = new GameObject("Region Field Root");
                root.transform.SetParent(transform, false);
                _fieldRoot = root.transform;
                _initialized = true;
            }
            SetRegion(regionId);
        }

        public void SetRegion(EchoRegionId regionId)
        {
            if (!_initialized || _fieldRoot == null) return;
            for (var i = _fieldRoot.childCount - 1; i >= 0; i--)
            {
                var child = _fieldRoot.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
            _pockets.Clear();
            _pois.Clear();
            _nextPoiAt = 70f;
            ActiveRegionId = regionId;

            if (regionId == EchoRegionId.SpectralRift)
            {
                // Exact pocket layout from new-desing/src/region.ts:RIFT_POCKETS.
                AddPocket("Prism Shift", 0f, 0f, 330f, "#7fe8ff", true);
                AddPocket("Mirror Shoal", 520f, -270f, 320f, "#d48cff", false);
                AddPocket("Ember Fault", 470f, 450f, 330f, "#ff9d63", false);
                AddPocket("Silent Lattice", -500f, 430f, 320f, "#65f0cf", false);
                AddPocket("Null Garden", -520f, -290f, 330f, "#a78cff", false);
            }
            else
            {
                // Exact pocket layout from new-desing/src/region.ts:BASIN_POCKETS.
                AddPocket("Axis Node", 0f, 0f, 340f, "#39d8ff", true);
                AddPocket("Glass Flow", 520f, -260f, 330f, "#7cf7d4", false);
                AddPocket("Fracture", 500f, 430f, 330f, "#ff6b6b", false);
                AddPocket("Hollow Contour", -480f, 430f, 330f, "#9b7cff", false);
                AddPocket("Pressure Field", -520f, -300f, 330f, "#ffb84d", false);
            }
        }

        private void AddPocket(string id, float sourceX, float sourceY, float sourceRadius, string htmlColor, bool active)
        {
            if (!ColorUtility.TryParseHtmlString(htmlColor, out var accent)) return;
            var center = new Vector2(sourceX * SourcePixelsToWorld, sourceY * SourcePixelsToWorld);
            var radius = sourceRadius * SourcePixelsToWorld;
            var pocket = new PocketVisual { Center = center, Radius = radius, Accent = accent, Active = active };

            var ring = CreateLine(id + " / Boundary", WithAlpha(accent, active ? 0.13f : 0.035f),
                active ? 0.0315f : 0.0175f, -18, true);
            ring.positionCount = CircleSegments;
            for (var i = 0; i < CircleSegments; i++)
            {
                var angle = i * Mathf.PI * 2f / CircleSegments;
                ring.SetPosition(i, center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }

            for (var i = 0; i < 6; i++)
                pocket.Seams.Add(CreateLine(id + " / Fold Seam " + i,
                    WithAlpha(accent, active ? 0.16f : 0.045f), 0.0175f, -17, false));

            for (var i = 0; i < 8; i++)
            {
                var marker = new GameObject(id + " / Boundary Marker " + i);
                marker.transform.SetParent(_fieldRoot, false);
                var sprite = marker.AddComponent<SpriteRenderer>();
                sprite.sprite = RuntimeSpriteFactory.Prism;
                sprite.color = WithAlpha(accent, active ? 0.24f : 0.055f);
                sprite.sortingOrder = -16;
                marker.transform.localScale = Vector3.one * (7f * SourcePixelsToWorld);
                pocket.Markers.Add(sprite);
            }

            _pockets.Add(pocket);
        }

        private LineRenderer CreateLine(string objectName, Color color, float width, int sortingOrder, bool loop)
        {
            var lineObject = new GameObject(objectName);
            lineObject.transform.SetParent(_fieldRoot, false);
            var line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = loop;
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Stretch;
            line.sharedMaterial = _lineMaterial;
            line.startColor = color;
            line.endColor = color;
            line.startWidth = width;
            line.endWidth = width;
            line.sortingOrder = sortingOrder;
            line.numCapVertices = 0;
            line.numCornerVertices = 0;
            return line;
        }

        private void Update()
        {
            if (!_initialized || _runtime == null || _runtime.IsPresentationPaused) return;
            _seamTime += Time.deltaTime * 0.65f;

            foreach (var pocket in _pockets)
            {
                for (var i = 0; i < pocket.Seams.Count; i++)
                {
                    var angle = i * Mathf.PI / 3f
                        + _seamTime * (i % 2 == 0 ? 0.08f : -0.06f)
                        + pocket.Center.x * 0.0001f;
                    var span = 0.10f + (pocket.Active ? 0.045f : 0.02f);
                    var seamRadius = pocket.Radius - 4f * SourcePixelsToWorld + (i % 2) * 2f * SourcePixelsToWorld;
                    var line = pocket.Seams[i];
                    line.positionCount = SeamSegments;
                    for (var j = 0; j < SeamSegments; j++)
                    {
                        var a = angle + span * j / (SeamSegments - 1f);
                        line.SetPosition(j, pocket.Center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * seamRadius);
                    }
                }

                for (var i = 0; i < pocket.Markers.Count; i++)
                {
                    var angle = i * Mathf.PI / 4f + _seamTime * 0.08f;
                    var position = pocket.Center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * pocket.Radius;
                    var marker = pocket.Markers[i].transform;
                    marker.position = position;
                    marker.rotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg - 90f);
                }
            }

            UpdateRegionPois();
        }

        private void UpdateRegionPois()
        {
            var time = _runtime.CurrentRunTime;
            if (!_runtime.RegionStabilized && time >= _nextPoiAt)
            {
                SpawnPoi(time);
                _nextPoiAt += 38f;
            }

            for (var i = _pois.Count - 1; i >= 0; i--)
            {
                var poi = _pois[i];
                if (time >= poi.ExpiresAt ||
                    Vector2.Distance(_runtime.PlayerPosition, poi.Position) <= 42f * SourcePixelsToWorld)
                {
                    if (time < poi.ExpiresAt &&
                        Vector2.Distance(_runtime.PlayerPosition, poi.Position) <= 42f * SourcePixelsToWorld)
                        _runtime.RegisterRegionPoiCollected(poi.Type);
                    RemovePoi(poi);
                    _pois.RemoveAt(i);
                    continue;
                }

                var color = GetPoiColor(poi.Type);
                var pulse = 38f + Mathf.Sin(time * 4f + poi.Id) * 4f;
                poi.Root.transform.position = new Vector3(poi.Position.x, poi.Position.y, 0.01f);
                poi.Glow.color = WithAlpha(color, 0.18f);
                poi.Glow.transform.localScale = Vector3.one * (pulse * 2f * SourcePixelsToWorld);
                poi.Diamond.startColor = poi.Diamond.endColor = WithAlpha(color, 0.9f);
                poi.Label.color = WithAlpha(color, 0.86f);
                poi.Label.transform.position = new Vector3(
                    poi.Position.x, poi.Position.y + 16f * SourcePixelsToWorld, 0f);

                const float halfDiamond = 11f * SourcePixelsToWorld;
                poi.Diamond.SetPosition(0, new Vector3(poi.Position.x, poi.Position.y + halfDiamond, 0f));
                poi.Diamond.SetPosition(1, new Vector3(poi.Position.x + halfDiamond, poi.Position.y, 0f));
                poi.Diamond.SetPosition(2, new Vector3(poi.Position.x, poi.Position.y - halfDiamond, 0f));
                poi.Diamond.SetPosition(3, new Vector3(poi.Position.x - halfDiamond, poi.Position.y, 0f));
            }
        }

        private void SpawnPoi(float time)
        {
            var id = 1;
            for (var i = 0; i < _pois.Count; i++) id = Mathf.Max(id, _pois[i].Id + 1);
            if (_pockets.Count == 0) return;
            var pocketIndex = (id + Mathf.FloorToInt(time / 60f)) % _pockets.Count;
            var pocket = _pockets[pocketIndex];
            var directions = new[]
            {
                new Vector2(0f, -1f), new Vector2(0.92f, -0.38f),
                new Vector2(0.88f, 0.48f), new Vector2(-0.82f, 0.55f),
                new Vector2(-0.92f, -0.34f)
            };
            var types = new[]
            {
                "resonance_cache", "breach_node", "echo_relay", "lost_signal",
                "elite_nest", "rupture", "boss_trace"
            };
            var type = types[(id - 1) % types.Length];
            var position = pocket.Center + directions[id % directions.Length] * pocket.Radius * 0.62f;
            var color = GetPoiColor(type);

            var root = new GameObject("Region POI / " + type);
            root.transform.SetParent(_fieldRoot, false);
            root.transform.position = new Vector3(position.x, position.y, 0.01f);
            var glowObject = new GameObject("Pulse");
            glowObject.transform.SetParent(root.transform, false);
            var glow = glowObject.AddComponent<SpriteRenderer>();
            glow.sprite = RuntimeSpriteFactory.Disc;
            glow.color = WithAlpha(color, 0.18f);
            glow.sortingOrder = 3;

            var diamond = CreateLine("Region POI / Marker / " + id,
                WithAlpha(color, 0.9f), 2f * SourcePixelsToWorld, 6, true);
            diamond.positionCount = 4;

            var labelObject = new GameObject("Region POI / Label / " + id);
            labelObject.transform.SetParent(_fieldRoot, false);
            labelObject.transform.position = new Vector3(
                position.x, position.y + 16f * SourcePixelsToWorld, 0f);
            var label = labelObject.AddComponent<TextMesh>();
            label.text = GetPoiLabel(type);
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 100;
            label.characterSize = 0.12f;
            label.color = WithAlpha(color, 0.86f);
            var labelRenderer = label.GetComponent<MeshRenderer>();
            if (labelRenderer != null) labelRenderer.sortingOrder = 6;

            _pois.Add(new PoiVisual
            {
                Id = id,
                Type = type,
                Position = position,
                ExpiresAt = time + (type == "boss_trace" ? 45f : 70f),
                Root = root,
                Glow = glow,
                Diamond = diamond,
                Label = label
            });
        }

        private void RemovePoi(PoiVisual poi)
        {
            if (poi.Root != null)
            {
                if (Application.isPlaying) Destroy(poi.Root);
                else DestroyImmediate(poi.Root);
            }
            if (poi.Diamond != null)
            {
                if (Application.isPlaying) Destroy(poi.Diamond.gameObject);
                else DestroyImmediate(poi.Diamond.gameObject);
            }
            if (poi.Label != null)
            {
                if (Application.isPlaying) Destroy(poi.Label.gameObject);
                else DestroyImmediate(poi.Label.gameObject);
            }
        }

        private static Color GetPoiColor(string type)
        {
            var hex = type == "elite_nest" || type == "rupture" ? "#ff6b6b"
                : type == "boss_trace" ? "#ffb84d" : "#55e6c1";
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : Color.white;
        }

        private static string GetPoiLabel(string type)
        {
            switch (type)
            {
                case "resonance_cache": return "RESONANCE CACHE";
                case "breach_node": return "BREACH NODE";
                case "echo_relay": return "ECHO RELAY";
                case "lost_signal": return "LOST SIGNAL";
                case "elite_nest": return "ELITE NEST";
                case "rupture": return "RUPTURE";
                default: return "BOSS TRACE";
            }
        }

        private static Color WithAlpha(Color color, float alpha) =>
            new Color(color.r, color.g, color.b, alpha);

        private void OnDestroy()
        {
            if (_lineMaterial == null) return;
            if (Application.isPlaying) Destroy(_lineMaterial);
            else DestroyImmediate(_lineMaterial);
        }
    }
}
