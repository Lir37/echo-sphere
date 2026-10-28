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

        private sealed class PocketVisual
        {
            public Vector2 Center;
            public float Radius;
            public Color Accent;
            public bool Active;
        }

        private sealed class PoiVisual
        {
            public int Id;
            public string Type;
            public Vector2 Position;
            public float ExpiresAt;
        }
        [Serializable] public sealed class PoiState
        { public int id; public string type; public Vector2 position; public float expiresAt; }
        [Serializable] public sealed class PoiSnapshot
        { public float nextPoiAt; public List<PoiState> pois = new List<PoiState>(); }
        internal readonly struct PresentationPoi
        {
            internal readonly int Id;internal readonly string Type;internal readonly Vector2 Position;
            internal PresentationPoi(int id,string type,Vector2 position){Id=id;Type=type;Position=position;}
        }
        internal void CopyPresentationPois(List<PresentationPoi> output)
        {output.Clear();foreach(var poi in _pois)output.Add(new PresentationPoi(poi.Id,poi.Type,poi.Position));}
        public PoiSnapshot CapturePoiState()
        {
            var save = new PoiSnapshot { nextPoiAt = _nextPoiAt };
            foreach (var poi in _pois) save.pois.Add(new PoiState { id = poi.Id, type = poi.Type, position = poi.Position, expiresAt = poi.ExpiresAt });
            return save;
        }
        public void RestorePoiState(PoiSnapshot save)
        {
            if (save == null) return;
            _nextPoiAt = save.nextPoiAt;
            _pois.Clear();
            foreach (var poi in save.pois) CreatePoi(poi.id, poi.type, poi.position, poi.expiresAt);
        }

        private readonly List<PocketVisual> _pockets = new List<PocketVisual>();
        private readonly List<PoiVisual> _pois = new List<PoiVisual>();
        private float _nextPoiAt = 70f;
        private EchoSphereRuntime _runtime;
#if UNITY_EDITOR
        private Texture2D _radarTexture;
        private Texture2D _radarPixel;
#endif
        private bool _initialized;
        public EchoRegionId ActiveRegionId { get; private set; } = EchoRegionId.ResonanceBasin;

        public void Initialize(EchoSphereRuntime runtime, EchoRegionId regionId = EchoRegionId.ResonanceBasin)
        {
            if (runtime == null) return;
            _runtime = runtime;
            _initialized=true;
            SetRegion(regionId);
        }

        public void SetRegion(EchoRegionId regionId)
        {
            if (!_initialized) return;
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
            var center = new Vector2(sourceX * SourcePixelsToWorld, -sourceY * SourcePixelsToWorld);
            var radius = sourceRadius * SourcePixelsToWorld;
            var pocket = new PocketVisual { Center = center, Radius = radius, Accent = accent, Active = active };

            _pockets.Add(pocket);
        }

        public void TickSourceRegionState()
        {
            var time = _runtime.CurrentRunTime;
            if (!_runtime.RegionStabilized && time >= _nextPoiAt)
            {
                SpawnPoi(time);
                _nextPoiAt += 38f;
            }

            for (var i = _pois.Count - 1; i >= 0; i--) if (time >= _pois[i].ExpiresAt) { _pois.RemoveAt(i); }
        }
        public void TickGameplayPois()
        {
            var time = _runtime.CurrentRunTime;
            var collected = false;
            for (var i = 0; i < _pois.Count; i++)
            {
                var poi = _pois[i];
                if (!collected && Vector2.Distance(_runtime.PlayerPosition, poi.Position) <= 42f * SourcePixelsToWorld)
                {
                    _runtime.RegisterRegionPoiCollected(poi.Type);
                    _pois.RemoveAt(i--); collected = true; continue;
                }


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
            var sourceDirection = directions[id % directions.Length];
            var position = pocket.Center + new Vector2(sourceDirection.x, -sourceDirection.y) * pocket.Radius * 0.62f;
            CreatePoi(id, type, position, time + (type == "boss_trace" ? 45f : 70f));
        }
        private void CreatePoi(int id, string type, Vector2 position, float expiresAt)
        {
            _pois.Add(new PoiVisual {Id=id,Type=type,Position=position,ExpiresAt=expiresAt});
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


#if UNITY_EDITOR
        private void EnsureLegacyRadar()
        {
            if(_radarTexture!=null)return;
            _radarTexture=CreateRadarTexture();
            _radarPixel=new Texture2D(1,1,TextureFormat.RGBA32,false){name="Editor reference radar pixel",hideFlags=HideFlags.DontSave};
            _radarPixel.SetPixel(0,0,Color.white);_radarPixel.Apply();
        }
        private static Texture2D CreateRadarTexture()
        {
            const int size = 100;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "ECHO SPHERE / Region Radar",
                hideFlags = HideFlags.DontSave
            };
            var pixels = new Color32[size * size];
            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = x - center;
                var dy = y - center;
                var distance = Mathf.Sqrt(dx * dx + dy * dy);
                var ring = Mathf.Abs(distance - 47f) < 0.75f || Mathf.Abs(distance - 32f) < 0.65f;
                var cross = (Mathf.Abs(dx) < 0.45f || Mathf.Abs(dy) < 0.45f) && distance < 47f;
                var alpha = ring ? 54 : cross ? 28 : 0;
                pixels[y * size + x] = new Color32(100, 190, 220, (byte)alpha);
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

#endif
        public void DrawSourceRadar(Rect radar)
        {
#if UNITY_EDITOR
            EnsureLegacyRadar();
            if (!_initialized || _runtime == null || _radarTexture == null || _radarPixel == null) return;
            var previousMatrix = GUI.matrix; var previousColor = GUI.color;
            GUI.matrix = previousMatrix * Matrix4x4.TRS(new Vector3(radar.x, radar.y, 0f), Quaternion.identity, new Vector3(radar.width / 100f, radar.height / 100f, 1f));
            radar = new Rect(0f, 0f, 100f, 100f);

            GUI.color = Color.white;
            GUI.DrawTexture(radar, _radarTexture);

            var player = _runtime.PlayerPosition;
            const float sourceRadius = 900f * SourcePixelsToWorld;
            var visibleEnemies = 0;
            foreach (var enemy in _runtime.SourceRadarEnemies)
            {
                if (enemy == null || !enemy.IsAlive) continue; if (visibleEnemies++ >= 40) break;
                var delta = (Vector2)enemy.transform.position - player; var angle = Mathf.Atan2(-delta.y, delta.x);
                var radius = Mathf.Min(1f, delta.magnitude / sourceRadius) * 25f;
                var point = new Vector2(50f + Mathf.Cos(angle) * radius, 50f + Mathf.Sin(angle) * radius);
                _runtime.DrawSourceRadarDot(point, enemy.IsBoss ? 2.5f : enemy.IsElite ? 1.5f : 1f, enemy.IsBoss ? new Color32(255, 106, 95, 255) : enemy.IsElite ? new Color32(230, 108, 255, 255) : new Color32(107, 233, 255, 255));
            }
            for (var i = 0; i < Mathf.Min(12, _pois.Count); i++)
            {
                var poi = _pois[i];
                if (_runtime.CurrentRunTime >= poi.ExpiresAt) continue;
                var delta = poi.Position - player;
                var distance = delta.magnitude;
                var angle = Mathf.Atan2(-delta.y, delta.x);
                var outside = distance > sourceRadius;
                var radius = outside ? 42f : Mathf.Min(25f, distance / sourceRadius * 25f);
                var point = new Vector2(radar.x + 50f + Mathf.Cos(angle) * radius,
                    radar.y + 50f + Mathf.Sin(angle) * radius);
                var color = GetPoiColor(poi.Type);
                if (outside)
                {
                    var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    var perpendicular = new Vector2(-direction.y, direction.x);
                    var tip = point + direction * 5f;
                    var baseCenter = point - direction * 4f;
                    DrawRadarLine(tip, baseCenter + perpendicular * 4f, color, 1.8f);
                    DrawRadarLine(baseCenter + perpendicular * 4f, baseCenter - perpendicular * 4f, color, 1.8f);
                    DrawRadarLine(baseCenter - perpendicular * 4f, tip, color, 1.8f);
                }
                else
                {
                    var previous = GUI.color;
                    var dotMatrix = GUI.matrix;
                    GUI.color = color;
                    var dot = new Rect(point.x - 3.5f, point.y - 3.5f, 7f, 7f);
                    GUIUtility.RotateAroundPivot(45f, point);
                    GUI.DrawTexture(dot, _radarPixel);
                    GUI.matrix = dotMatrix;
                    GUI.color = previous;
                }
            }

            GUI.color = new Color(0.92f, 0.98f, 1f, 1f);
            GUI.DrawTexture(new Rect(radar.x + 46f, radar.y + 46f, 8f, 8f), _radarPixel);
            GUI.color = previousColor;
            GUI.matrix = previousMatrix;
#endif
        }

#if UNITY_EDITOR
        private void DrawRadarLine(Vector2 start, Vector2 end, Color color, float thickness)
        {
            var delta = end - start;
            var length = delta.magnitude;
            if (length <= 0.01f) return;
            var previousMatrix = GUI.matrix;
            var previousColor = GUI.color;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, start);
            GUI.color = color;
            GUI.DrawTexture(new Rect(start.x, start.y - thickness * 0.5f, length, thickness), _radarPixel);
            GUI.matrix = previousMatrix;
            GUI.color = previousColor;
        }

        private static Color WithAlpha(Color color, float alpha) =>
            new Color(color.r, color.g, color.b, alpha);

        private void OnDestroy()
        {
            if (_radarTexture != null)
            {
                if (Application.isPlaying) Destroy(_radarTexture);
                else DestroyImmediate(_radarTexture);
            }
            if (_radarPixel != null)
            {
                if (Application.isPlaying) Destroy(_radarPixel);
                else DestroyImmediate(_radarPixel);
            }
        }
#endif
    }
}
