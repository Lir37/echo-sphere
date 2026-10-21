using System.Globalization;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private bool TryDrawNativeSettings()
        {
            bool selected=_mainMenu&&!_regionSelect&&_mainMenuPage==MainMenuPage.Settings;
            if(!selected){
                if(_nativeSettingsWasActive){_nativeSettingsScreen.Hide();_nativeSettingsWasActive=false;
                    if(_sourceUi!=null&&!UsesNativeMainMenu&&!_nativeProgressionWasActive){_sourceUi.SetPresentationSuspended(false);_sourceUi.Dirty=true;}}
                return false;
            }
            if(_nativeSettingsScreen==null)_nativeSettingsScreen=new GameObject("Native Settings UI Toolkit").AddComponent<SourceSettingsToolkitScreen>();
            if(!_nativeSettingsWasActive&&_sourceUi!=null)_sourceUi.SetPresentationSuspended(true);
            _nativeSettingsWasActive=true;_camera.cullingMask=0;
            string lang=IsRussianUi?"ru":"en";
            string T(string key)=>Visual_i18n.Scope["translations"][lang][key].Text;
            // Preserve Number(localStorage.getItem(...)): absent/empty is zero,
            // then clamped to .75; non-finite input uses the source default 1.
            string raw=PlayerPrefs.GetString("echosphere_interface_scale","");double parsed;
            if(string.IsNullOrWhiteSpace(raw))parsed=0;
            else if(!double.TryParse(raw,NumberStyles.Float,CultureInfo.InvariantCulture,out parsed)||!double.IsFinite(parsed))parsed=1;
            float scale=Mathf.Clamp((float)parsed,.75f,1.25f);
            _nativeSettingsScreen.Show(SourceDevicePixelRatio*scale,IsRussianUi,SourceAudio2D.IsEnabled,IsLeftHanded,scale,_settingsResetPending,T,
                value=>{PlayerPrefs.SetString(LanguagePreferenceKey,value);PlayerPrefs.Save();},
                value=>{PlayerPrefs.SetInt(SoundPreferenceKey,value?1:0);SourceAudio2D.SetEnabled(value);PlayerPrefs.Save();},
                value=>{PlayerPrefs.SetString(HandednessPreferenceKey,value?"left":"right");PlayerPrefs.Save();ClearSourcePointers();},
                value=>{PlayerPrefs.SetString("echosphere_interface_scale",Mathf.Clamp(value,.75f,1.25f).ToString("R",CultureInfo.InvariantCulture));PlayerPrefs.Save();},
                ()=>{_mainMenuPage=MainMenuPage.Home;_settingsResetPending=false;},
                ()=>{PlayerPrefs.SetInt(TutorialCompletedPreferenceKey,0);PlayerPrefs.Save();_mainMenuPage=MainMenuPage.Home;},
                ()=>_settingsResetPending=true,()=>_settingsResetPending=false,
                ()=>{SourceUiBinding("persistence.ts","resetAll").Call();SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);});
            return true;
        }
    }
}
