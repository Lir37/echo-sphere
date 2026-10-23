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
                    if(_sourceUi!=null&&!UsesNativeMainMenu&&!_nativeProgressionWasActive){_sourceUi.Dirty=true;}}
                return false;
            }
            if(_nativeSettingsScreen==null)_nativeSettingsScreen=new GameObject("Native Settings UI Toolkit").AddComponent<SourceSettingsToolkitScreen>();

            _nativeSettingsWasActive=true;_camera.cullingMask=0;
            string lang=IsRussianUi?"ru":"en";
            string T(string key)=>NativeText(key,lang=="ru");
            float scale=(float)NativeInterfaceScale;
            _nativeSettingsScreen.Show(SourceDevicePixelRatio*scale,IsRussianUi,SourceAudio2D.IsEnabled,IsLeftHanded,scale,_settingsResetPending,T,
                value=>{PlayerPrefs.SetString(LanguagePreferenceKey,value);PlayerPrefs.Save();},
                value=>{PlayerPrefs.SetInt(SoundPreferenceKey,value?1:0);SourceAudio2D.SetEnabled(value);PlayerPrefs.Save();},
                value=>{PlayerPrefs.SetString(HandednessPreferenceKey,value?"left":"right");PlayerPrefs.Save();ClearSourcePointers();},
                value=>{PlayerPrefs.SetString("echosphere_interface_scale",Mathf.Clamp(value,.75f,1.25f).ToString("R",CultureInfo.InvariantCulture));PlayerPrefs.Save();},
                ()=>{_mainMenuPage=MainMenuPage.Home;_settingsResetPending=false;},
                ()=>{PlayerPrefs.SetInt(TutorialCompletedPreferenceKey,0);PlayerPrefs.Save();_mainMenuPage=MainMenuPage.Home;},
                ()=>_settingsResetPending=true,()=>_settingsResetPending=false,
                ()=>{NativeResetAllProgress();SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);});
            return true;
        }
    }
}
