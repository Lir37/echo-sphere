using System;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private bool _nativePauseScreenPilot=true;
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
        private void PrepareNativePauseScreen(NativeRunUiAnchors anchors)
        {
            if (!_nativePauseScreenPilot || !NativePauseChromeRequested) { DisposeNativePauseScreen(); return; }
            var shell=anchors.PauseShell;var content=shell.Content;
            if(content==null){DisposeNativePauseScreen();return;}
            if (_nativePauseScreen == null) _nativePauseScreen = new NativePauseScreenView(content, shell.Element.contentRect.height);
            else if (_nativePauseScreen.Root.parent != content) content.Add(_nativePauseScreen.Root);
        }
        private void CompleteNativePauseScreen(NativeRunUiAnchors anchors)
        {
            if (_nativePauseScreen == null) return;
            bool ready = UsesNativePauseChrome && (UsesNativePauseStats || UsesNativePauseAbilities || UsesNativePauseArtifacts || UsesNativePauseSynergies);
            bool previous = _nativePauseScreen.Ready;
            _nativePauseScreen.Prepare(Screen.height / SourceDevicePixelRatio, ready);
            if (!previous && _nativePauseScreen.Ready) anchors.Invalidate?.Invoke();
        }
        private ScrollView NativePauseBody(NativeRunUiAnchors anchors,out NativeRunUiAnchors.Slot body)
        {body=null;if(_nativePauseScreen!=null)return _nativePauseScreen.Body;body=anchors.PauseBody;return body.Content as ScrollView;}

    }
}
