using System;
using UnityEngine;
using UnityEngine.UIElements;
using static EchoSphere.Presentation.NativePauseStatsView;

namespace EchoSphere.Presentation
{
    // Header/footer content only. Existing shell paint, blur, body ScrollView
    // and detail-modal ordering remain owned by the existing pause screen.
    public sealed class NativePauseChromeView : IDisposable
    {
        private readonly VisualElement _header, _footer;
        private readonly Label _eyebrow, _title, _subtitle, _resumeText, _returnText;
        private readonly Label[] _tabTexts = new Label[4];
        private readonly VisualElement[] _icons = new VisualElement[4];
        private readonly Button[] _tabs = new Button[4];
        private readonly Action[] _tabClicks = new Action[4];
        private readonly Button _close, _resume, _return;
        private readonly Action _resumeAction, _returnAction;
        private readonly Action<int> _select;
        private readonly AnimationCurve _transition;
        private readonly float[] _shown = new float[4], _from = new float[4], _to = new float[4];
        private readonly Color[] _shownFill = new Color[4], _shownBorder = new Color[4];
        private Button[] _buttons;
        private bool _composition;
        internal Button[] Buttons => _buttons;
        internal readonly struct ButtonPaint
        {
            internal readonly Color Fill, Border;
            internal readonly float Radius;
            internal ButtonPaint(Color fill, Color border, float radius) { Fill = fill; Border = border; Radius = radius; }
        }
        internal ButtonPaint PaintFor(Button button)
        {
            for (int i = 0; i < 4; i++) if (button == _tabs[i]) return new ButtonPaint(_shownFill[i], _shownBorder[i], 6);
            if (button == _resume) return new ButtonPaint(C("#39d8ff"), C("#3a6a7a"), 3);
            return new ButtonPaint(new Color(6 / 255f, 17 / 255f, 29 / 255f, .78f), new Color(76 / 255f, 165 / 255f, 214 / 255f, .16f), button == _close ? 5 : 3);
        }
        internal void SetCompositionOwnership(bool owned)
        {
            _composition = owned;
            foreach (var button in _buttons)
            {
                button.style.backgroundColor = owned ? new StyleColor(Color.clear) : new StyleColor(StyleKeyword.Null);
                var color = owned ? new StyleColor(Color.clear) : new StyleColor(StyleKeyword.Null);
                button.style.borderLeftColor = button.style.borderRightColor = button.style.borderTopColor = button.style.borderBottomColor = color;
            }
            if (!owned && _bound) PaintTabs();
        }
        private double _transitionStart;
        private bool _bound, _disposed, _flow;
        private int _geometryFrame;
        public VisualElement Header => _header;
        public VisualElement Footer => _footer;
        public bool Ready { get; private set; }
        private static Color C(string html) { ColorUtility.TryParseHtmlString(html, out var color); return color; }
        public NativePauseChromeView(VisualElement header, VisualElement footer, Action resume, Action exit, Action<int> select)
        {
            if (header == null || footer == null) throw new ArgumentNullException("Pause header/footer hosts required");
            _resumeAction = resume; _returnAction = exit; _select = select;
            _transition = new AnimationCurve(new Keyframe(0, 0, 0, 0, 0, .4f) { weightedMode = WeightedMode.Both },
                // End control x=.2 means incoming weight .8. Both y tangents0.
                new Keyframe(1, 1, 0, 0, .8f, 0) { weightedMode = WeightedMode.Both });
            var sheet = Resources.Load<StyleSheet>("EchoSphere/NativePauseChrome");
            if (sheet == null) throw new InvalidOperationException("Pause chrome stylesheet missing");
            _header = Box(header, "native-pause-header"); _footer = Box(footer, "native-pause-footer", true);
            foreach (var root in new[] { _header, _footer })
            {
                root.AddToClassList("native-pause-chrome"); root.styleSheets.Add(sheet);
                root.style.position = Position.Absolute; root.style.left = root.style.top = 0;
                root.style.width = Length.Percent(100); root.style.opacity = 0;
                root.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
            }
            var row = Box(_header, "pause-chrome-heading-row", true); row.style.justifyContent = Justify.SpaceBetween; row.style.alignItems = Align.Center;
            var copy = Box(row, "pause-chrome-copy"); copy.style.flexShrink = 1; copy.style.marginRight = 12;
            _eyebrow = Text(copy, "pause-chrome-eyebrow", 10, "#64849b", spacing: 2);
            _title = Text(copy, "pause-chrome-title", 20, "#dcecff", "700", 28d / 20); _title.style.overflow = Overflow.Hidden; _title.style.textOverflow = TextOverflow.Ellipsis;
            _subtitle = Text(copy, "pause-chrome-subtitle", 12, "#64849b", line: 16d / 12); _subtitle.style.marginTop = 2;
            _close = Button(row, "pause-chrome-close", resume); Icon(_close, "X", 18, "#c5dbeb");
            var tabs = Box(_header, "pause-chrome-tabs", true); tabs.style.marginTop = 12;
            string[] icons = { "BarChart3", "Sparkles", "Package", "Network" };
            for (int i = 0; i < 4; i++)
            {
                int index = i; _tabClicks[i] = () => _select?.Invoke(index);
                _tabs[i] = Button(tabs, "pause-chrome-tab", _tabClicks[i]); _tabs[i].name += "-" + i;
                if (i > 0) _tabs[i].style.marginLeft = 6;
                _icons[i] = Icon(_tabs[i], icons[i], 15, "#64849b"); _icons[i].style.marginRight = 4;
                _tabTexts[i] = Text(_tabs[i], "pause-chrome-tab-text-" + i, 10, "#64849b", "700");
            }
            _footer.style.justifyContent = Justify.Center;
            _resume = Button(_footer, "pause-chrome-resume", resume); _resume.style.marginRight = 8;
            _resumeText = Text(_resume, "pause-chrome-resume-text", 16, "#c5dbeb", "700");
            _return = Button(_footer, "pause-chrome-return", exit); _returnText = Text(_return, "pause-chrome-return-text", 16, "#cfe7f3");
            _buttons = new[] { _close, _tabs[0], _tabs[1], _tabs[2], _tabs[3], _resume, _return };
            _geometryFrame = Time.frameCount;
        }
        private static Button Button(VisualElement parent, string name, Action clicked)
        {
            var button = new Button(clicked) { name = name }; button.AddToClassList(name);
            SourceUiCursor.Apply(button.style, "pointer");
            button.RegisterCallback<ClickEvent>(StopClick); parent.Add(button); return button;
        }
        private static void StopClick(ClickEvent evt) { evt.StopPropagation(); }
        private static VisualElement Icon(VisualElement parent, string name, float size, string color)
        {
            var image = Resources.Load<VectorImage>("EchoSphere/NativeMenuIcons/" + name);
            if (image == null) throw new InvalidOperationException("Native pause vector missing: " + name);
            var icon = Box(parent, "pause-chrome-icon-" + name); icon.style.width = icon.style.height = size;
            icon.style.backgroundImage = new StyleBackground(image); icon.style.unityBackgroundImageTintColor = C(color); return icon;
        }
        private void GeometryChanged(GeometryChangedEvent evt) { _geometryFrame = Time.frameCount; }
        public void Reparent(VisualElement header, VisualElement footer)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(NativePauseChromeView));
            if (header == null || footer == null) throw new ArgumentNullException("Pause hosts required");
            // A source detail modal changes the footer's reconciliation path.
            // Keep the controls and callbacks when only its host is replaced.
            if (_header.parent != header) header.Add(_header);
            if (_footer.parent != footer) footer.Add(_footer);
        }
        public void Bind(NativePauseChromeState state)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(NativePauseChromeView));
            if (state == null || state.Tabs.Length != 4) throw new ArgumentException("Four authored pause tabs required");
            Set(_eyebrow, state.Eyebrow.ToUpperInvariant()); Set(_title, state.Title); Set(_subtitle, state.Subtitle);
            Set(_resumeText, state.Resume); Set(_returnText, state.Return); _close.tooltip = state.Resume;
            bool changed = !_bound;
            for (int i = 0; i < 4; i++) { Set(_tabTexts[i], state.Tabs[i]); float target = state.SelectedTab == i ? 1 : 0; changed |= _to[i] != target; }
            if (changed)
            {
                for (int i = 0; i < 4; i++) { _to[i] = state.SelectedTab == i ? 1 : 0; _from[i] = _bound ? _shown[i] : _to[i]; }
                _transitionStart = Time.unscaledTimeAsDouble; _bound = true; PaintTabs();
            }
        }
        private void PaintTabs()
        {
            float t = _transition.Evaluate(Mathf.Clamp01((float)((Time.unscaledTimeAsDouble - _transitionStart) / .15)));
            for (int i = 0; i < 4; i++)
            {
                float amount = _shown[i] = Mathf.Lerp(_from[i], _to[i], t);
                var bg = BlendColor(new Color(6 / 255f, 17 / 255f, 29 / 255f, .78f), new Color(57 / 255f, 216 / 255f, 1, .15f), amount);
                var border = BlendColor(new Color(76 / 255f, 165 / 255f, 214 / 255f, .16f), new Color(57 / 255f, 216 / 255f, 1, .5f), amount);
                _shownFill[i] = bg; _shownBorder[i] = border;
                if (!_composition) { _tabs[i].style.backgroundColor = bg; _tabs[i].style.borderLeftColor = _tabs[i].style.borderRightColor = _tabs[i].style.borderTopColor = _tabs[i].style.borderBottomColor = border; }
                var color = Color.Lerp(C("#64849b"), C("#cfe7f3"), amount); _tabTexts[i].style.color = color; _icons[i].style.unityBackgroundImageTintColor = color;
            }
        }
        // Match authored premultiplied color transitions; no CSS resolver.
        public static Color BlendColor(Color from, Color to, float t)
        {
            float alpha = Mathf.Lerp(from.a, to.a, t);
            if (alpha <= 0) return Color.clear;
            return new Color(Mathf.Lerp(from.r * from.a, to.r * to.a, t) / alpha,
                Mathf.Lerp(from.g * from.a, to.g * to.a, t) / alpha,
                Mathf.Lerp(from.b * from.a, to.b * to.a, t) / alpha, alpha);
        }
        public void UseFlowLayout(bool flow)
        {
            if (_flow == flow) return; _flow = flow;
            foreach (var root in new[] { _header, _footer })
            {
                root.style.position = flow ? Position.Relative : Position.Absolute;
                root.style.width = Length.Percent(100);
                if (flow) root.style.left = root.style.top = StyleKeyword.Auto; else root.style.left = root.style.top = 0;
            }
        }
        public void Prepare()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(NativePauseChromeView)); if (!_bound) return; PaintTabs();
            if (!_flow) { WarmBounds(_header); WarmBounds(_footer); }
            if (!Ready && float.IsFinite(_header.layout.height + _footer.layout.height) && _header.layout.width > 0 && _header.layout.height > 0 && _footer.layout.height > 0 && Time.frameCount - _geometryFrame >= 2)
            { Ready = true; _header.style.opacity = _footer.style.opacity = 1; }
        }
        private static void WarmBounds(VisualElement root)
        {
            var parent = root.parent; if (parent == null || parent.contentRect.width <= 0) return;
            root.style.left = parent.resolvedStyle.paddingLeft; root.style.top = parent.resolvedStyle.paddingTop;
            root.style.width = parent.contentRect.width;
        }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true;
            _close.clicked -= _resumeAction; _resume.clicked -= _resumeAction; _return.clicked -= _returnAction;
            for (int i = 0; i < 4; i++) _tabs[i].clicked -= _tabClicks[i];
            foreach (var root in new[] { _header, _footer }) { root.UnregisterCallback<GeometryChangedEvent>(GeometryChanged); root.RemoveFromHierarchy(); }
        }
    }
}
