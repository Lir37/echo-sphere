using System;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private bool _nativePauseChromePilot=true;
        private NativePauseChromeView _nativePauseChromeIsland;
        private NativePauseChromeComposition _nativePauseChromeComposition;
        private V _nativePauseChromeResume, _nativePauseChromeExit, _nativePauseChromeSelect;
        private bool NativePauseChromeRequested => _nativePauseChromePilot && !_mainMenu && !_gameOver && _userPaused && _sourceRenderState.Truth && !_sourceRenderState["pendingUpgrade"].Truth && !_sourceRenderState["pendingArtifact"].Truth && !_sourceRenderState["pendingStella"].Truth;
        public bool UsesNativePauseChrome => NativePauseChromeRequested && _nativePauseChromeIsland != null && _nativePauseChromeIsland.Ready;
        public void SetNativePauseChromePilot(bool enabled)
        {
            if (_nativePauseChromePilot == enabled) return; _nativePauseChromePilot = enabled;
            if (!enabled) DisposeNativePauseChromeIsland(); if (_sourceUi != null) _sourceUi.Dirty = true;
        }
        private void DisposeNativePauseChromeIsland()
        {
            _nativePauseChromeComposition?.Dispose(); _nativePauseChromeComposition = null;
            _nativePauseChromeIsland?.Dispose(); _nativePauseChromeIsland = null;
            _nativePauseChromeResume = _nativePauseChromeExit = _nativePauseChromeSelect = default;
        }
        private void DispatchNativePauseChrome(V action, params V[] arguments)
        {
            if (!UsesNativePauseChrome || action.Undefined) return;
            action.Call(arguments); ApplySourceUiWrites(); if (_sourceUi != null) _sourceUi.Dirty = true;
        }
        private void PrepareNativePauseChromeIsland(VisualCssNode root)
        {
            if (!NativePauseChromeRequested) { DisposeNativePauseChromeIsland(); return; }
            VisualCssNode header = null, footer = null;
            void Find(VisualCssNode node, bool pause)
            {
                pause |= node.HasClass("es-pause-overlay");
                if (pause && node.HasClass("px-4") && node.HasClass("pt-4") && node.HasClass("pb-3") && node.HasClass("border-b")) header = node;
                if (pause && node.HasClass("px-4") && node.HasClass("py-3") && node.HasClass("border-t")) footer = node;
                foreach (var child in node.Children) Find(child, pause);
            }
            Find(root, false);
            var head = _nativePauseScreen?.Header ?? (header?.Native as SourceUiFrame)?.Content;
            var foot = _nativePauseScreen?.Footer ?? (footer?.Native as SourceUiFrame)?.Content;
            if (head == null || foot == null) { DisposeNativePauseChromeIsland(); return; }
            if (_nativePauseChromeIsland == null)
            {
                // Original actions are already bound while source content is visible.
                // Keep them through native warmup; Dispose would clear first-frame input.
                _nativePauseChromeComposition?.Dispose(); _nativePauseChromeComposition = null;
                _nativePauseChromeIsland = new NativePauseChromeView(head, foot,
                    () => DispatchNativePauseChrome(_nativePauseChromeResume), () => DispatchNativePauseChrome(_nativePauseChromeExit),
                    index => DispatchNativePauseChrome(_nativePauseChromeSelect, new V(new[] { "stats", "skills", "artifacts", "synergies" }[index])));
                _nativePauseChromeComposition = new NativePauseChromeComposition(_nativePauseChromeIsland, _sourceUi);
            }
            else _nativePauseChromeIsland.Reparent(head, foot);
            _nativePauseChromeIsland.Bind(ReadNativePauseChromeState());
            bool ready = _nativePauseChromeIsland.Ready; _nativePauseChromeIsland.Prepare();
            _nativePauseChromeComposition.Prepare();
            if (_nativePauseChromeIsland.Ready)
            {
                foreach (var child in header?.Children ?? System.Linq.Enumerable.Empty<VisualCssNode>()) if (child.Native is VisualElement element) element.style.display = DisplayStyle.None;
                foreach (var child in footer?.Children ?? System.Linq.Enumerable.Empty<VisualCssNode>()) if (child.Native is VisualElement element) element.style.display = DisplayStyle.None;
                _nativePauseChromeIsland.UseFlowLayout(true); if (!ready) _sourceUi.Dirty = true;
            }
        }
        public NativePauseChromeState ReadNativePauseChromeState()
        {
            if (_sourceRenderState.Undefined) throw new InvalidOperationException("Existing render snapshot required");
            return ProjectNativePauseChromeState(_sourceRenderState, IsRussianUi, _pausePlannerTab);
        }
        internal static NativePauseChromeState ProjectNativePauseChromeState(V scene, bool ru, int tab)
        {
            var lang = ru ? "ru" : "en"; var character = Visual_characters.Scope["CHARACTER_DEFS"][scene["player"]["characterId"]];
            string T(string key) => Visual_i18n.Scope["translations"][lang][key].Text;
            return new NativePauseChromeState {
                Eyebrow = ru ? "Билд забега" : "Run Build", Title = T("pauseTitle") + " · " + character["name"][lang].Text,
                Subtitle = character["role"][lang].Text + " · " + (ru ? "уровень" : "level") + " " + scene["player"]["level"].Text + " · " + T("wave") + " " + scene["wave"].Text,
                Tabs = ru ? new[] { "Показатели", "Способности", "Артефакты", "Синергии" } : new[] { "Stats", "Abilities", "Artifacts", "Synergies" },
                Resume = T("resume"), Return = T("return"), SelectedTab = tab
            };
        }
    }
}
