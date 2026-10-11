using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Source-mapped 2.5D boss overlay from the active renderer.ts path:
    /// drawBossAttackTelegraph + boss HP/name strip. Boss silhouettes remain
    /// owned by EnemyCreatureVisual2D, mapped to src/enemies/enemyVisual.ts.
    /// </summary>
    public sealed class BossPresentationVfx2D : MonoBehaviour
    {
        private const float ShootTelegraphSeconds = 0.70f;
        private const float SummonTelegraphSeconds = 0.90f;
        private const float AuraTelegraphSeconds = 0.80f;
        private static Material _sharedMaterial;
        private readonly List<Vector3> _vertices = new List<Vector3>(420);
        private readonly List<Color> _colors = new List<Color>(420);
        private readonly List<int> _triangles = new List<int>(720);
        private readonly List<Vector2> _path = new List<Vector2>(64);
        private EnemyAgent2D _agent;
        private string _bossType = "";
        private Color _color = Color.white;
        private Transform _overlayRoot;
        private Mesh _mesh;
        private MeshRenderer _meshRenderer;
        private TextMesh _label;
        private float _shootTimer = 999f;
        private float _summonTimer = 999f;
        private float _chargeTimer;
        private bool _isCharging;
        private Vector2 _chargeDirection;
        private float _auraRadius;
        private float _time;

        public void Initialize(EnemyAgent2D agent, string bossType, Color color)
        {
            _agent = agent;
            _bossType = bossType ?? "";
            _color = color;
            _overlayRoot = new GameObject("Boss HUD And Telegraph").transform;
            _overlayRoot.SetParent(transform, false);
            _overlayRoot.localPosition = Vector3.zero;
            var scale = Mathf.Max(0.001f, Mathf.Abs(transform.localScale.x));
            _overlayRoot.localScale = Vector3.one / scale;

            _mesh = new Mesh { name = "ECHO SPHERE Boss HUD And Telegraph Mesh" };
            _mesh.MarkDynamic();
            var filter = _overlayRoot.gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = _mesh;
            _meshRenderer = _overlayRoot.gameObject.AddComponent<MeshRenderer>();
            _meshRenderer.sharedMaterial = SharedMaterial;
            _meshRenderer.sortingOrder = 9;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;

            var labelObject = new GameObject("Boss Nameplate");
            labelObject.transform.SetParent(_overlayRoot, false);
            _label = labelObject.AddComponent<TextMesh>();
            _label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _label.fontSize = 24;
            _label.characterSize = 0.0055f;
            _label.anchor = TextAnchor.MiddleCenter;
            _label.alignment = TextAlignment.Center;
            _label.color = new Color(0.90f, 0.96f, 1f, 0.92f);
            _label.text = GetSourceLabel(_bossType);
            var textRenderer = labelObject.GetComponent<MeshRenderer>();
            textRenderer.sortingOrder = 10;
            textRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            textRenderer.receiveShadows = false;
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
                        _sharedMaterial.name = "ECHO SPHERE Boss Presentation (Unlit)";
                        _sharedMaterial.hideFlags = HideFlags.HideAndDontSave;
                    }
                }
                return _sharedMaterial;
            }
        }

        public void SetCombatState(float shootTimer, float summonTimer, float chargeTimer,
            bool isCharging, Vector2 chargeDirection, float auraRadius)
        {
            _shootTimer = shootTimer;
            _summonTimer = summonTimer;
            _chargeTimer = chargeTimer;
            _isCharging = isCharging;
            _chargeDirection = chargeDirection.sqrMagnitude > 0.0001f ? chargeDirection.normalized : Vector2.right;
            _auraRadius = Mathf.Max(0f, auraRadius);
        }

        private void LateUpdate()
        {
            if (_agent == null || _mesh == null || _agent.PresentationPaused) return;
            _time += Time.deltaTime;
            var radius = Mathf.Max(0.36f, Mathf.Abs(transform.localScale.x) * 0.48f * 1.15f);
            if (_label != null) _label.transform.localPosition = new Vector3(0f, radius + 0.36f, 0f);
            Rebuild();
        }

        private void Rebuild()
        {
            if (_mesh == null || _agent == null) return;
            _vertices.Clear();
            _colors.Clear();
            _triangles.Clear();
            var radius = Mathf.Max(0.36f, Mathf.Abs(transform.localScale.x) * 0.48f * 1.15f);
            var pulse = 0.55f + Mathf.Sin(_time * 14f) * 0.16f;
            var playerDelta = _agent.PlayerPosition - (Vector2)transform.position;

            if (_bossType == "charger" && _isCharging)
            {
                var windup = _chargeTimer > 0.35f;
                var reach = radius * (windup ? 2.7f : 4.2f);
                var tint = windup ? new Color(1f, 0.72f, 0.30f, 0.75f) : new Color(1f, 0.25f, 0.22f, 0.9f);
                AddLine(Vector2.zero, _chargeDirection * reach, windup ? 0.035f : 0.055f, tint);
                if (windup)
                {
                    var dir = _chargeDirection;
                    var normal = new Vector2(-dir.y, dir.x);
                    var start = dir * radius * 0.82f;
                    var tip = dir * radius * 2.05f;
                    var half = normal * radius * 0.55f;
                    AddTriangle(start, tip + half, tip - half, new Color(1f, 0.72f, 0.30f, 0.13f + pulse * 0.05f));
                }
            }

            if (_bossType == "shooter" && _shootTimer >= 0f && _shootTimer <= ShootTelegraphSeconds)
            {
                var angle = playerDelta.sqrMagnitude > 0.0001f ? Mathf.Atan2(playerDelta.y, playerDelta.x) : 0f;
                var tint = new Color(1f, 0.42f, 0.37f, 0.46f + pulse * 0.22f);
                for (var k = -1; k <= 1; k++)
                {
                    var ray = new Vector2(Mathf.Cos(angle + k * 0.11f), Mathf.Sin(angle + k * 0.11f));
                    AddLine(ray * radius * 1.05f, ray * radius * 4.8f, 0.022f, tint);
                }
            }

            if (_bossType == "summoner" && _summonTimer >= 0f && _summonTimer <= SummonTelegraphSeconds)
            {
                var progress = 1f - Mathf.Clamp01(_summonTimer / SummonTelegraphSeconds);
                AddRing(radius * (1.15f + progress * 2.2f), new Color(0.64f, 0.49f, 1f, 0.34f + pulse * 0.18f), 0.025f, 36);
                for (var i = 0; i < 3; i++)
                {
                    var a = _time * 2.2f + i * Mathf.PI * 2f / 3f;
                    var at = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius * (0.95f + progress * 0.8f);
                    AddDiamond(at, Mathf.Max(0.035f, radius * 0.055f), new Color(0.79f, 0.71f, 1f, 0.62f));
                }
            }

            if (_bossType == "aura" && _shootTimer >= 0f && _shootTimer <= AuraTelegraphSeconds && _auraRadius > 0f)
            {
                AddRing(_auraRadius + radius * (0.22f + pulse * 0.05f),
                    new Color(1f, 0.38f, 0.73f, 0.42f + pulse * 0.20f), 0.028f, 56);
            }

            var ratio = _agent.MaxHp <= 0f ? 0f : Mathf.Clamp01(_agent.CurrentHp / _agent.MaxHp);
            var barWidth = 2.2f;
            var barHeight = 0.12f;
            var barY = radius + 0.54f;
            AddRect(new Rect(-barWidth * 0.5f, barY, barWidth, barHeight), new Color(0.008f, 0.027f, 0.055f, 0.92f));
            AddRect(new Rect(-barWidth * 0.5f, barY, barWidth * ratio, barHeight), new Color(_color.r, _color.g, _color.b, 0.96f));
            AddLine(new Vector2(-barWidth * 0.5f, barY), new Vector2(barWidth * 0.5f, barY), 0.012f, new Color(0.86f, 0.96f, 1f, 0.70f));
            AddLine(new Vector2(-barWidth * 0.5f, barY + barHeight), new Vector2(barWidth * 0.5f, barY + barHeight), 0.012f, new Color(0.86f, 0.96f, 1f, 0.70f));

            _mesh.Clear();
            _mesh.SetVertices(_vertices);
            _mesh.SetColors(_colors);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.RecalculateBounds();
        }

        private void AddRect(Rect rect, Color color)
        {
            var a = new Vector2(rect.xMin, rect.yMin);
            var b = new Vector2(rect.xMax, rect.yMin);
            var c = new Vector2(rect.xMax, rect.yMax);
            var d = new Vector2(rect.xMin, rect.yMax);
            AddQuad(a, b, c, d, color);
        }

        private void AddLine(Vector2 a, Vector2 b, float width, Color color)
        {
            var delta = b - a;
            if (delta.sqrMagnitude <= 0.0000001f || width <= 0f) return;
            var n = new Vector2(-delta.y, delta.x).normalized * width * 0.5f;
            AddQuad(a + n, a - n, b - n, b + n, color);
        }

        private void AddQuad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
        {
            var start = _vertices.Count;
            _vertices.Add(new Vector3(a.x, a.y, 0f));
            _vertices.Add(new Vector3(b.x, b.y, 0f));
            _vertices.Add(new Vector3(c.x, c.y, 0f));
            _vertices.Add(new Vector3(d.x, d.y, 0f));
            for (var i = 0; i < 4; i++) _colors.Add(color);
            _triangles.Add(start); _triangles.Add(start + 1); _triangles.Add(start + 2);
            _triangles.Add(start); _triangles.Add(start + 2); _triangles.Add(start + 3);
        }

        private void AddTriangle(Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            var start = _vertices.Count;
            _vertices.Add(new Vector3(a.x, a.y, 0f));
            _vertices.Add(new Vector3(b.x, b.y, 0f));
            _vertices.Add(new Vector3(c.x, c.y, 0f));
            _colors.Add(color); _colors.Add(color); _colors.Add(color);
            _triangles.Add(start); _triangles.Add(start + 1); _triangles.Add(start + 2);
        }

        private void AddRing(float radius, Color color, float width, int segments)
        {
            var count = Mathf.Clamp(segments, 12, 64);
            for (var i = 0; i < count; i++)
            {
                var a = i * Mathf.PI * 2f / count;
                var b = (i + 1) * Mathf.PI * 2f / count;
                AddLine(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius,
                    new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, width, color);
            }
        }

        private void AddDiamond(Vector2 center, float radius, Color color)
        {
            AddTriangle(center + Vector2.up * radius, center + Vector2.right * radius,
                center - Vector2.up * radius, color);
            AddTriangle(center + Vector2.up * radius, center - Vector2.up * radius,
                center - Vector2.right * radius, color);
        }

        private static string GetSourceLabel(string type)
        {
            if (type == "charger") return "BREACHER";
            if (type == "shooter") return "VOID LANCER";
            if (type == "summoner") return "BROOD MIND";
            return "AURA TITAN";
        }

        private void OnDestroy()
        {
            if (_mesh != null)
            {
                if (Application.isPlaying) Destroy(_mesh);
                else DestroyImmediate(_mesh);
            }
        }
    }
}
