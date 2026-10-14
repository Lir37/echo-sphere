using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private sealed class SourcePointerState
        {
            public Vector2 start, world;
            public bool moved, movement, placing, valid, locked;
            public SphereAttackAgent target;
            public SphereNetworkState preview;
            public int previewIndex;
            public readonly List<Vec2> positions = new List<Vec2>();
            public SphereNetworkFormation formation;
            public int[] nodes;
            public Vector2[] formationPoints;
        }
        private readonly Dictionary<int, SourcePointerState> _sourcePointers = new Dictionary<int, SourcePointerState>();
        private int? _sourceJoystickId;
        private Vector2 _sourceJoystickStart, _sourceJoystickDrag, _sourceJoystickDirection;
        private Vector2 _sourceTouchLastDirection = Vector2.up;
        private bool _sourceLockedSphereTouch;
        private int _sourceUiTouchFrame = -1;
        private readonly HashSet<int> _sourceUiTouches = new HashSet<int>();
        private float SourceInterfaceScale => Mathf.Clamp(PlayerPrefs.GetFloat("echosphere_interface_scale", .75f), .75f, 1.25f);
        private Vector2 SourceControlsAnchor => new Vector2(IsLeftHanded ? _uiWidth - 12f : 12f, _uiHeight - 16f);
        private bool SourceTouchClick(Rect rect)
        {
            if (!GUI.enabled || Event.current.type != EventType.Repaint) return false;
            if (_sourceUiTouchFrame != Time.frameCount) { _sourceUiTouchFrame = Time.frameCount; _sourceUiTouches.Clear(); }
            for (var i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                if (touch.phase != TouchPhase.Began || _sourceUiTouches.Contains(touch.fingerId)) continue;
                var point = GUIUtility.ScreenToGUIPoint(new Vector2(touch.position.x, Screen.height - touch.position.y));
                if (rect.Contains(point)) { _sourceUiTouches.Add(touch.fingerId); return true; }
            }
            return false;
        }
        private bool SourceGuiButton(Rect rect, GUIContent content, GUIStyle style)
        {
            if (Input.touchCount == 0) return GUI.Button(rect, content, style);
            GUI.Box(rect, content, style); return SourceTouchClick(rect);
        }
        private void StoreSourcePointer(int id)
        {
            var pointer = new SourcePointerState { start = _spherePointerStartScreen, world = _spherePointerWorld,
                moved = _spherePointerMoved, movement = _spherePointerIsMovement, placing = _spherePointerIsPlacing, locked = _sourceLockedSphereTouch,
                valid = _spherePointerPositionValid, target = _spherePointerTarget, preview = _spherePointerPreview,
                previewIndex = _spherePointerPreviewIndex, formation = _spherePointerOriginalFormation,
                nodes = _spherePointerOriginalNodeIndexes, formationPoints = _spherePointerOriginalFormationPoints };
            pointer.positions.AddRange(_spherePointerPreviewPositions); _sourcePointers[id] = pointer;
        }
        private void LoadSourcePointer(int id, SourcePointerState pointer)
        {
            _spherePointerActive = true; _spherePointerFromTouch = id >= 0; _spherePointerTouchId = id;
            _spherePointerStartScreen = pointer.start; _spherePointerWorld = pointer.world;
            _spherePointerMoved = pointer.moved; _spherePointerIsMovement = pointer.movement; _spherePointerIsPlacing = pointer.placing;
            _sourceLockedSphereTouch = pointer.locked;
            _spherePointerPositionValid = pointer.valid; _spherePointerTarget = pointer.target; _spherePointerPreview = pointer.preview;
            _spherePointerPreviewIndex = pointer.previewIndex; _spherePointerOriginalFormation = pointer.formation;
            _spherePointerOriginalNodeIndexes = pointer.nodes; _spherePointerOriginalFormationPoints = pointer.formationPoints;
            _spherePointerPreviewPositions.Clear(); _spherePointerPreviewPositions.AddRange(pointer.positions);
        }
        private void HandleSourcePointer(int id, Vector2 position, bool began, bool ended)
        {
            if (began && !_sourcePointers.ContainsKey(id))
            {
                if (IsScreenPointOverUi(position)) return;
                ClearSpherePointer(); BeginSpherePointer(position, id >= 0, id);
                if (!_spherePointerActive) return;
                if (_spherePointerIsMovement)
                { _sourceJoystickId = id; _sourceJoystickStart = position; _sourceJoystickDrag = Vector2.zero; }
                StoreSourcePointer(id);
            }
            if (!_sourcePointers.TryGetValue(id, out var pointer)) return;
            LoadSourcePointer(id, pointer); MoveSpherePointer(position);
            if (_sourceJoystickId == id)
            {
                var drag = (position - _sourceJoystickStart) / Mathf.Max(1f, _uiScale);
                _sourceJoystickDrag = Vector2.ClampMagnitude(drag, 58f);
                _sourceJoystickDirection = Vector2.zero;
                if (drag.magnitude >= 12f)
                {
                    var normal = drag.normalized; _sourceTouchLastDirection = normal;
                    _sourceJoystickDirection = new Vector2(Mathf.Abs(normal.x) > .2f ? Mathf.Sign(normal.x) : 0f,
                        Mathf.Abs(normal.y) > .2f ? Mathf.Sign(normal.y) : 0f);
                }
            }
            if (ended)
            {
                FinishSpherePointer(position); _sourcePointers.Remove(id);
                if (_sourceJoystickId == id) { _sourceJoystickId = null; _sourceJoystickDirection = Vector2.zero; }
            }
            else StoreSourcePointer(id);
        }
        private void ClearSourcePointers()
        { _sourcePointers.Clear(); _sourceJoystickId = null; _sourceJoystickDirection = Vector2.zero; ClearSpherePointer(); }
        private void DrawSourceJoystick()
        {
            if (!_sourceJoystickId.HasValue) return;
            var center = new Vector2(_sourceJoystickStart.x / _uiScale, _uiHeight - _sourceJoystickStart.y / _uiScale);
            var scale = SourceInterfaceScale;
            DrawSphereGuiFilledCircle(center, 58f * scale, new Color(0f, 0f, 0f, .2f));
            DrawSphereGuiCircle(center, 58f * scale, new Color(1f, 1f, 1f, .2f), 1f);
            var knob = center + new Vector2(_sourceJoystickDrag.x, -_sourceJoystickDrag.y) * scale;
            DrawSphereGuiFilledCircle(knob, 24f * scale, new Color(1f, 1f, 1f, _sourceJoystickDirection == Vector2.zero ? .175f : .225f));
            DrawSphereGuiCircle(knob, 24f * scale, new Color(1f, 1f, 1f, .3f), 1f);
        }
    }
}
