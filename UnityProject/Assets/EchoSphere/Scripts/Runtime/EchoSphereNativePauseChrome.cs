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
        }
        private void PrepareNativePauseChromeIsland(NativeRunUiAnchors anchors)
        {
            if (!NativePauseChromeRequested) { DisposeNativePauseChromeIsland(); return; }
            var header=anchors.PauseHeader;var footer=anchors.PauseFooter;
            var head=_nativePauseScreen?.Header??header.Content;var foot=_nativePauseScreen?.Footer??footer.Content;
            if (head == null || foot == null) { DisposeNativePauseChromeIsland(); return; }
            if (_nativePauseChromeIsland == null)
            {
                // Native controls bind directly to the existing runtime commands.
                _nativePauseChromeComposition?.Dispose(); _nativePauseChromeComposition = null;
                _nativePauseChromeIsland = new NativePauseChromeView(head, foot,
                    NativeResumeRun, NativeExitRun, NativeSelectPauseTab);
                _nativePauseChromeComposition = new NativePauseChromeComposition(_nativePauseChromeIsland, anchors.Backdrops);
            }
            else _nativePauseChromeIsland.Reparent(head, foot);
            _nativePauseChromeIsland.Bind(ReadNativePauseChromeState());
            bool ready = _nativePauseChromeIsland.Ready; _nativePauseChromeIsland.Prepare();
            _nativePauseChromeComposition.Prepare();
            if (_nativePauseChromeIsland.Ready)
            {
                header.HideLegacy(true);
                footer.HideLegacy(true);
                _nativePauseChromeIsland.UseFlowLayout(true); if (!ready) anchors.Invalidate?.Invoke();
            }
        }
        public NativePauseChromeState ReadNativePauseChromeState()
        {
            if (_sourceRenderState.Undefined) throw new InvalidOperationException("Existing render snapshot required");
            return ProjectNativePauseChromeState(_sourceRenderState, IsRussianUi, _pausePlannerTab);
        }
        internal static NativePauseChromeState ProjectNativePauseChromeState(V scene, bool ru, int tab)
        {
            var lang = ru ? "ru" : "en"; EchoSphere.Core.CharacterRules.TryParseSourceId(scene["player"]["characterId"].Text,out var characterId);var character=EchoSphere.Core.CharacterRules.Get(characterId);
            string T(string key) => NativeText(key,lang=="ru");
            return new NativePauseChromeState {
                Eyebrow = ru ? "Билд забега" : "Run Build", Title = T("pauseTitle") + " · " + (ru?character.NameRu:character.NameEn),
                Subtitle = (ru?character.RoleRu:character.RoleEn) + " · " + (ru ? "уровень" : "level") + " " + scene["player"]["level"].Text + " · " + T("wave") + " " + scene["wave"].Text,
                Tabs = ru ? new[] { "Показатели", "Способности", "Артефакты", "Синергии" } : new[] { "Stats", "Abilities", "Artifacts", "Synergies" },
                Resume = T("resume"), Return = T("return"), SelectedTab = tab
            };
        }
    }
}
