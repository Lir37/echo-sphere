using System;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private bool _nativePauseScreenPilot;
        private NativePauseScreenView _nativePauseScreen;
        public bool UsesNativePauseScreen => NativePauseChromeRequested && _nativePauseScreenPilot && _nativePauseScreen != null && _nativePauseScreen.Ready;
        public void SetNativePauseScreenPilot(bool enabled)
        {
            if (_nativePauseScreenPilot == enabled) return;
            _nativePauseScreenPilot = enabled;
            SetNativePauseChromePilot(enabled); SetNativePauseStatsPilot(enabled);
            SetNativePauseAbilitiesPilot(enabled); SetNativePauseArtifactsPilot(enabled); SetNativePauseSynergiesPilot(enabled);
            if (!enabled) DisposeNativePauseScreen();
            if (_sourceUi != null) _sourceUi.Dirty = true;
        }
        private void DisposeNativePauseScreen() { _nativePauseScreen?.Dispose(); _nativePauseScreen = null; }
        private void PrepareNativePauseScreen(VisualCssNode root)
        {
            if (!_nativePauseScreenPilot || !NativePauseChromeRequested) { DisposeNativePauseScreen(); return; }
            VisualCssNode shell = null;
            void Find(VisualCssNode node, bool pause)
            {
                pause |= node.HasClass("es-pause-overlay");
                if (pause && shell == null && node.HasClass("es-modal-shell")) shell = node;
                foreach (var child in node.Children) Find(child, pause);
            }
            Find(root, false);
            if (!(shell?.Native is SourceUiFrame frame)) { DisposeNativePauseScreen(); return; }
            if (_nativePauseScreen == null) _nativePauseScreen = new NativePauseScreenView(frame.Content, frame.contentRect.height);
            else if (_nativePauseScreen.Root.parent != frame.Content) frame.Content.Add(_nativePauseScreen.Root);
        }
        private void CompleteNativePauseScreen()
        {
            if (_nativePauseScreen == null) return;
            bool ready = UsesNativePauseChrome && (UsesNativePauseStats || UsesNativePauseAbilities || UsesNativePauseArtifacts || UsesNativePauseSynergies);
            bool previous = _nativePauseScreen.Ready;
            _nativePauseScreen.Prepare(Screen.height / SourceDevicePixelRatio, ready);
            if (!previous && _nativePauseScreen.Ready) _sourceUi.Dirty = true;
        }
        private ScrollView NativePauseBody(VisualCssNode root, out VisualCssNode body)
        {
            body = null;
            if (_nativePauseScreen != null) return _nativePauseScreen.Body;
            VisualCssNode found = null;
            void Find(VisualCssNode node, bool pause)
            {
                pause |= node.HasClass("es-pause-overlay");
                if (found == null && pause && node.HasClass("overflow-y-auto") && node.HasClass("px-4") && node.HasClass("py-4") && node.HasClass("min-h-0")) { found = node; return; }
                foreach (var child in node.Children) Find(child, pause);
            }
            Find(root, false); body = found;
            return (found?.Native as SourceUiFrame)?.Content as ScrollView;
        }
        private V ProjectNativePauseScreen(V tree)
        {
            if (!UsesNativePauseScreen) return tree;
            void Visit(V value, bool pause)
            {
                if (value.Raw is VisualDom.Node node)
                {
                    var classes = (node.Properties["className"].Text ?? "").Split(' ');
                    pause |= Array.IndexOf(classes, "es-pause-overlay") >= 0;
                    if (pause && Array.IndexOf(classes, "es-modal-shell") >= 0)
                    {
                        // Retain the original detail modal and shell artwork.
                        node.Children.RemoveAll(child => {
                            if (!(child.Raw is VisualDom.Node element)) return false;
                            var tokens = (element.Properties["className"].Text ?? "").Split(' ');
                            bool Has(string token) => Array.IndexOf(tokens, token) >= 0;
                            return Has("px-4") && ((Has("pt-4") && Has("pb-3") && Has("border-b")) ||
                                (Has("overflow-y-auto") && Has("py-4") && Has("min-h-0")) || (Has("py-3") && Has("border-t")));
                        });
                        return;
                    }
                    foreach (var child in node.Children) Visit(child, pause);
                }
                else if (value.Raw is System.Collections.Generic.List<V>) foreach (var child in value.Items) Visit(child, pause);
            }
            Visit(tree, false); return tree;
        }
    }
}
