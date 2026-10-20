using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static EchoSphere.Presentation.NativePauseStatsView;

namespace EchoSphere.Presentation
{
    // Diagnostic only. Native Yoga, TextCore, borders and weighted animation.
    public sealed class NativePauseSynergiesView : IDisposable
    {
        private sealed class Card
        {
            internal VisualElement Root, Tags;
            internal Label Name, Status, Description, Hint, Empty;
            internal readonly List<Label> TagLabels = new List<Label>();
            internal bool Pulse;
            internal double PulseStart;
        }
        private sealed class Section
        {
            internal VisualElement Root;
            internal Label Heading;
            internal readonly List<Card> Cards = new List<Card>();
        }
        private readonly List<Section> _sections = new List<Section>();
        private readonly NativeUiLineBoxes _lines = new NativeUiLineBoxes();
        private readonly VisualElement _root;
        private bool _bound, _disposed, _flow;
        private int _geometryFrame;
        public VisualElement Root => _root;
        public bool Ready { get; private set; }
        // SourceUiStyles: pulse 2s cubic-bezier(.4,0,.6,1), opacity .5 at50%.
        private static readonly AnimationCurve PulseCurve = new AnimationCurve(
            new Keyframe(0, 1, 0, 0, .4f, .4f) { weightedMode = WeightedMode.Both },
            new Keyframe(1, .5f, 0, 0, .4f, .4f) { weightedMode = WeightedMode.Both },
            new Keyframe(2, 1, 0, 0, .4f, .4f) { weightedMode = WeightedMode.Both });
        public static float EvaluatePulse(float seconds) => PulseCurve.Evaluate(Mathf.Repeat(seconds, 2));
        private static Color C(string hex, float alpha = 1) { ColorUtility.TryParseHtmlString(hex, out var c); c.a = alpha; return c; }
        private static void Surface(VisualElement root, Color fill, Color border, float padding, float radius)
        {
            root.style.backgroundColor = fill;
            root.style.paddingLeft = root.style.paddingRight = root.style.paddingTop = root.style.paddingBottom = padding;
            root.style.borderLeftWidth = root.style.borderRightWidth = root.style.borderTopWidth = root.style.borderBottomWidth = 1;
            root.style.borderLeftColor = root.style.borderRightColor = root.style.borderTopColor = root.style.borderBottomColor = border;
            root.style.borderTopLeftRadius = root.style.borderTopRightRadius = root.style.borderBottomLeftRadius = root.style.borderBottomRightRadius = radius;
        }
        public NativePauseSynergiesView(VisualElement parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            _root = Box(parent, "native-pause-synergies");
            _root.style.position = Position.Absolute; _root.style.left = _root.style.top = 0;
            _root.style.width = Length.Percent(100); _root.style.opacity = 0;
            _root.RegisterCallback<GeometryChangedEvent>(GeometryChanged); _geometryFrame = Time.frameCount;
        }
        private void GeometryChanged(GeometryChangedEvent evt) { _geometryFrame = Time.frameCount; }
        private Section AddSection(int index)
        {
            var section = new Section { Root = Box(_root, "pause-synergy-section-" + index) };
            if (index > 0) section.Root.style.marginTop = 16;
            section.Heading = Text(section.Root, "pause-synergy-section-" + index + "-heading", 10, "#64849b", "700", spacing: .5f);
            section.Heading.style.marginBottom = 8;
            _sections.Add(section); return section;
        }
        private static Card AddCard(Section section, int sectionIndex, int index)
        {
            string key = "pause-synergy-" + sectionIndex + "-" + index;
            var card = new Card { Root = Box(section.Root, key) };
            if (index > 0) card.Root.style.marginTop = 8;
            var row = Box(card.Root, key + "-row", true); row.style.alignItems = Align.Center; row.style.justifyContent = Justify.SpaceBetween;
            card.Name = Text(row, key + "-name", 14, "#dcecff", "700", 20d / 14, wrap: true);
            card.Name.style.flexShrink = 1; card.Name.style.marginRight = 8;
            card.Status = Text(row, key + "-status", 9, "#64849b", "700");
            card.Description = Text(card.Root, key + "-description", 10, "#cfe7f3", wrap: true); card.Description.style.marginTop = 4;
            card.Hint = Text(card.Root, key + "-hint", 10, "#64849b", wrap: true); card.Hint.style.marginTop = 4;
            card.Tags = Box(card.Root, key + "-tags", true); card.Tags.style.flexWrap = Wrap.Wrap; card.Tags.style.marginRight = -4;
            card.Empty = Text(card.Tags, key + "-empty", 9, "#64849b"); card.Empty.style.color = C("#64849b", .6f); card.Empty.style.marginTop = 4;
            section.Cards.Add(card); return card;
        }
        public void Bind(NativePauseSynergiesState state)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(NativePauseSynergiesView));
            if (state == null) throw new ArgumentNullException(nameof(state)); _bound = true;
            while (_sections.Count < state.Sections.Count) AddSection(_sections.Count);
            for (int i = 0; i < _sections.Count; i++)
            {
                var section = _sections[i]; section.Root.style.display = i < state.Sections.Count ? DisplayStyle.Flex : DisplayStyle.None;
                if (i >= state.Sections.Count) continue; var model = state.Sections[i]; Set(section.Heading, model.Heading.ToUpperInvariant());
                while (section.Cards.Count < model.Cards.Count) AddCard(section, i, section.Cards.Count);
                for (int j = 0; j < section.Cards.Count; j++)
                {
                    var card = section.Cards[j]; card.Root.style.display = j < model.Cards.Count ? DisplayStyle.Flex : DisplayStyle.None;
                    if (j >= model.Cards.Count) continue; var value = model.Cards[j];
                    string tone = value.Tone == "purple" ? "#8064a8" : value.Tone == "cyan" ? "#39d8ff" : value.Tone == "gold" ? "#ffb84d" : value.Tone == "ice" ? "#63e6ff" : null;
                    Surface(card.Root, tone == null ? C("#06111d", .78f) : C(tone, value.Tone == "cyan" ? .05f : .1f), tone == null ? C("#4ca5d6", .16f) : C(tone, value.Tone == "cyan" ? .25f : .4f), 12, 3);
                    Set(card.Name, value.Diagnostic ? value.Name.ToUpperInvariant() : value.Name); Set(card.Status, value.Status); Set(card.Description, value.Description); Set(card.Hint, value.Hint); Set(card.Empty, state.EmptySignals);
                    card.Name.style.fontSize = value.Diagnostic ? 9 : 14;
                    card.Name.style.letterSpacing = (value.Diagnostic ? 1.08f : .192f) * 100 / (value.Diagnostic ? 9 : 14);
                    card.Status.style.color = C(value.Tone == "purple" || value.Tone == "cyan" || value.Tone == "ice" ? tone : "#64849b");
                    card.Status.style.display = value.Diagnostic ? DisplayStyle.None : DisplayStyle.Flex;
                    card.Description.style.display = value.Diagnostic ? DisplayStyle.None : DisplayStyle.Flex;
                    card.Hint.style.display = string.IsNullOrEmpty(value.Hint) ? DisplayStyle.None : DisplayStyle.Flex;
                    card.Tags.style.display = value.Diagnostic || value.Tags.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                    card.Tags.style.marginTop = value.Diagnostic ? 0 : 4;
                    card.Empty.style.display = value.Diagnostic && value.Tags.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
                    while (card.TagLabels.Count < value.Tags.Count)
                    {
                        var tag = Text(card.Tags, card.Root.name + "-tag-" + card.TagLabels.Count, 9, "#64849b");
                        tag.style.marginRight = tag.style.marginTop = 4; card.TagLabels.Add(tag);
                    }
                    for (int k = 0; k < card.TagLabels.Count; k++)
                    {
                        var label = card.TagLabels[k]; label.style.display = k < value.Tags.Count ? DisplayStyle.Flex : DisplayStyle.None;
                        if (k >= value.Tags.Count) continue; var tag = value.Tags[k]; Set(label, tag.Text);
                        Surface(label, tag.Owned ? C("#5a8c4a", .1f) : C("#000000", .05f), tag.Owned ? C("#5a8c4a", .25f) : C("#4ca5d6", .16f), 4, 6);
                        label.style.paddingLeft = label.style.paddingRight = 6; label.style.height = 23.5f;
                        label.style.color = C(tag.Owned ? "#5a8c4a" : value.Diagnostic ? "#cfe7f3" : "#64849b");
                    }
                    if (value.Pulse && !card.Pulse) card.PulseStart = Time.unscaledTimeAsDouble;
                    card.Pulse = value.Pulse; if (!card.Pulse) card.Root.style.opacity = 1;
                }
            }
        }
        public void UseFlowLayout(bool flow)
        {
            if (_flow == flow) return; _flow = flow; _root.style.position = flow ? Position.Relative : Position.Absolute;
            if (flow) _root.style.left = _root.style.top = StyleKeyword.Auto; else _root.style.left = _root.style.top = 0;
        }
        public void Prepare(float scale)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(NativePauseSynergiesView));
            if (!_bound || !float.IsFinite(_root.layout.width + _root.layout.height) || _root.layout.width <= 0 || _root.layout.height <= 0) return;
            foreach (var section in _sections) foreach (var card in section.Cards)
            {
                if (card.Root.resolvedStyle.display == DisplayStyle.None) continue;
                if (_lines.Fit(card.Name, card.Status.resolvedStyle.display == DisplayStyle.None ? 13.5f : 20)) _geometryFrame = Time.frameCount;
                if (card.Description.resolvedStyle.display != DisplayStyle.None && _lines.Fit(card.Description, 15)) _geometryFrame = Time.frameCount;
                if (card.Hint.resolvedStyle.display != DisplayStyle.None && _lines.Fit(card.Hint, 15)) _geometryFrame = Time.frameCount;
                if (card.Pulse) card.Root.style.opacity = EvaluatePulse((float)(Time.unscaledTimeAsDouble - card.PulseStart));
            }
            if (!Ready && Time.frameCount - _geometryFrame >= 2) { Ready = true; _root.style.opacity = 1; }
        }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true; _root.UnregisterCallback<GeometryChangedEvent>(GeometryChanged); _root.RemoveFromHierarchy();
        }
    }
}
