#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Native UIR text/vector capture for composition. No tree cloning, custom text
    // renderer, private Unity API or GPU readback. The caller owns frame ordering
    // and must reserve this layer exclusively for this capture document.
    public sealed class NativeUiCapturePanel : IDisposable
    {
        private readonly GameObject _owner, _cameraOwner;
        private readonly PanelSettings _settings;
        private readonly UIDocument _document;
        private readonly Camera _camera;
        private Vector2 _size;
        private UIRenderer _renderer;
        private bool _disposed;
        public VisualElement Root => _document.rootVisualElement;
        public Bounds Bounds => Ready ? _renderer.bounds : default;
        public bool Ready
        {
            get
            {
                if (_disposed) return false;
                // UIDocument adds its native world renderer after joining a panel.
                if (_renderer == null) _renderer = _owner.GetComponent<UIRenderer>();
                // A read-only capture must not add a live hit target or collider.
                if (_owner.TryGetComponent<Collider>(out var collider)) collider.enabled = false;
                return _renderer != null && _renderer.bounds.size.y > 0;
            }
        }
        public NativeUiCapturePanel(Transform parent, string name, int layer, Vector2 logicalSize, ThemeStyleSheet theme)
        {
            if (layer < 8 || layer > 31) throw new ArgumentOutOfRangeException(nameof(layer));
            if (!float.IsFinite(logicalSize.x + logicalSize.y) || logicalSize.x <= 0 || logicalSize.y <= 0)
                throw new ArgumentOutOfRangeException(nameof(logicalSize));
            foreach (var renderer in UnityEngine.Object.FindObjectsByType<Renderer>())
                if (renderer.gameObject.layer == layer && renderer.enabled && renderer.gameObject.activeInHierarchy)
                    throw new InvalidOperationException("Native UI capture layer is already occupied");
            if (parent != null && (parent.position != Vector3.zero || parent.rotation != Quaternion.identity || parent.lossyScale != Vector3.one))
                throw new ArgumentException("Capture owner must have an identity world transform", nameof(parent));
            _size = logicalSize;
            try {
                _settings = ScriptableObject.CreateInstance<PanelSettings>();
                _settings.name = name + " native capture panel"; _settings.renderMode = PanelRenderMode.WorldSpace;
                _settings.scaleMode = PanelScaleMode.ConstantPixelSize; _settings.scale = 1;
                _settings.forceGammaRendering = true; _settings.themeStyleSheet = theme;
                _owner = new GameObject(name + " native capture document"); _owner.transform.SetParent(parent, false); _owner.layer = layer;
                _document = _owner.AddComponent<UIDocument>(); _document.panelSettings = _settings;
                _document.worldSpaceSizeMode = WorldSpaceSizeMode.Fixed; _document.worldSpaceSize = logicalSize;
                // Overflowing paint/input must not move the capture's logical origin.
                _document.pivotReferenceSize = PivotReferenceSize.Layout; _document.pivot = Pivot.Center;
                var root = Root; root.style.position = Position.Absolute; root.style.left = root.style.top = 0;
                root.style.width = logicalSize.x; root.style.height = logicalSize.y; root.pickingMode = PickingMode.Ignore;
                _cameraOwner = new GameObject(name + " native capture camera"); _cameraOwner.transform.SetParent(parent, false);
                _camera = _cameraOwner.AddComponent<Camera>(); _camera.enabled = false; _camera.orthographic = true;
                _camera.cullingMask = 1 << layer; _camera.clearFlags = CameraClearFlags.SolidColor; _camera.backgroundColor = Color.clear;
                _camera.nearClipPlane = .01f; _camera.farClipPlane = 100; _camera.allowHDR = false;
                _camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            } catch {
                if (_document != null) _document.enabled = false;
                if (_owner != null) _owner.SetActive(false);
                if (_cameraOwner != null) _cameraOwner.SetActive(false);
                UnityEngine.Object.Destroy(_owner); UnityEngine.Object.Destroy(_cameraOwner); UnityEngine.Object.Destroy(_settings); throw;
            }
        }
        public void Resize(Vector2 logicalSize)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeUiCapturePanel));
            if(!float.IsFinite(logicalSize.x+logicalSize.y)||logicalSize.x<=0||logicalSize.y<=0)throw new ArgumentOutOfRangeException(nameof(logicalSize));
            if(_size==logicalSize)return;_size=logicalSize;_document.worldSpaceSize=logicalSize;
            Root.style.width=logicalSize.x;Root.style.height=logicalSize.y;
        }
        // Invoke after native text preparation, outside the URP render loop.
        // The tested compositor uses Application.onBeforeRender. Rectangular
        // world clipping is not a substitute for a cached native stencil mask.
        public bool TryRender(RenderTexture destination)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(NativeUiCapturePanel));
            if (destination == null || !destination.IsCreated()) throw new ArgumentException("Created destination required", nameof(destination));
            float aspect = (float)destination.width / destination.height;
            if (Mathf.Abs(aspect - _size.x / _size.y) > .001f) throw new ArgumentException("Capture aspect differs from logical size", nameof(destination));
            if (!Ready) return false;
            _camera.transform.position = _owner.transform.position - Vector3.forward * 10;
            _camera.orthographicSize = _size.y / (_settings.referenceSpritePixelsPerUnit * 2);
            _camera.aspect = aspect; _camera.allowMSAA = destination.antiAliasing > 1;
            var request = new UniversalRenderPipeline.SingleCameraRequest { destination = destination };
            if (!RenderPipeline.SupportsRenderRequest(_camera, request)) throw new InvalidOperationException("URP single-camera request unsupported");
            RenderPipeline.SubmitRenderRequest(_camera, request); return true;
        }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true;
            _document.enabled = false; _camera.enabled = false; _camera.targetTexture = null;
            _owner.SetActive(false); _cameraOwner.SetActive(false);
            UnityEngine.Object.Destroy(_owner); UnityEngine.Object.Destroy(_cameraOwner); UnityEngine.Object.Destroy(_settings);
        }
    }
}
#endif
