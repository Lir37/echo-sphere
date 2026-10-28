using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Retained layout owner. The source shell remains a paint adapter for its
    // authored gradient, inset/outer shadows and existing backdrop prefix order.
    public sealed class NativePauseScreenView : IDisposable
    {
        public VisualElement Root { get; }
        public VisualElement Header { get; }
        public VisualElement Footer { get; }
        public ScrollView Body { get; }
        public bool Ready { get; private set; }
        private bool _disposed;
        public NativePauseScreenView(VisualElement parent, float initialHeight)
        {
            Root = new VisualElement { name = "native-pause-screen", pickingMode = PickingMode.Ignore };
            // Reserve the shell's layout while its presenters warm up. An
            // absolute root leaves an auto-sized shell only its border high.
            Root.style.position = Position.Relative; Root.style.left = Root.style.top = 0;
            Root.style.width = Length.Percent(100); Root.style.height = float.IsFinite(initialHeight) ? Mathf.Max(1, initialHeight) : 1;
            Root.style.minWidth = Root.style.minHeight = 0; Root.style.flexShrink = 1;
            Root.style.opacity = 0; parent.Add(Root);
            Header = Host("native-pause-screen-header", 16, 16, 12);
            Header.style.height = StyleKeyword.Auto; Header.style.borderBottomWidth = 1;
            Header.style.borderBottomColor = new Color(76/255f,165/255f,214/255f,.16f);
            Body = new ScrollView(ScrollViewMode.Vertical) { name = "native-pause-screen-body" };
            Body.style.flexGrow = 1; Body.style.flexShrink = 1; Body.style.minHeight = 0;
            Body.contentContainer.style.paddingLeft = Body.contentContainer.style.paddingRight = 16;
            Body.contentContainer.style.paddingTop = Body.contentContainer.style.paddingBottom = 16;
            ConfigureMobileScroll(Body);
            Root.Add(Body);
            Footer = Host("native-pause-screen-footer", 16, 12, 12);
            Footer.style.height = StyleKeyword.Auto; Footer.style.borderTopWidth = 1;
            Footer.style.borderTopColor = new Color(76/255f,165/255f,214/255f,.16f);
            Footer.style.backgroundColor = new Color(6/255f,17/255f,29/255f,1);
        }
        internal static void ConfigureMobileScroll(ScrollView scroll)
        {
            scroll.mode = ScrollViewMode.Vertical;
            scroll.horizontalScrollerVisibility = scroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            scroll.style.minWidth = 0;
            scroll.contentContainer.style.width = Length.Percent(100);
            scroll.contentContainer.style.minWidth = 0;
            scroll.contentContainer.style.maxWidth = Length.Percent(100);
        }
        private VisualElement Host(string name, float x, float top, float bottom)
        {
            var host = new VisualElement { name = name, pickingMode = PickingMode.Ignore };
            host.style.flexShrink = 0; host.style.minWidth = 0;
            host.style.paddingLeft = host.style.paddingRight = x;
            host.style.paddingTop = top; host.style.paddingBottom = bottom;
            Root.Add(host); return host;
        }
        public void Prepare(float viewportHeight, bool contentReady)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(NativePauseScreenView));
            // Original max-height: min(92vh,780px), including shell borders.
            float limit = Mathf.Max(1, Mathf.Min(viewportHeight * .92f, 780) - 2);
            if (contentReady && float.IsFinite(Body.contentContainer.layout.height))
                Root.style.height = Mathf.Min(limit, Mathf.Max(1,Header.layout.height+Footer.layout.height) + Mathf.Max(32, Body.contentContainer.layout.height));
            else Root.style.height = limit;
            Root.SendToBack(); // Original artifact detail blocks native controls.
            if (!contentReady) return;
            Ready = true; Root.style.position = Position.Relative; Root.style.opacity = 1;
        }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true; Root.RemoveFromHierarchy();
        }
    }
}
