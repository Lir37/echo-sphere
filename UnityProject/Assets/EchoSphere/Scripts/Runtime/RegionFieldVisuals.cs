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

        private readonly List<PocketVisual> _pockets = new List<PocketVisual>();
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
