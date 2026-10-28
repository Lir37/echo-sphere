using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    /// <summary>Authored UI Toolkit settings; existing runtime callbacks own all changes.</summary>
    public sealed class SourceSettingsToolkitScreen : MonoBehaviour
    {
        private NativePanelLease _panelLease;
        private UIDocument _document;
        private PanelSettings _panel;
        private VisualElement _root, _content, _confirmation;
        private ScrollView _scroll;
        private Slider _scale;
        private Label _scaleValue, _resetPrompt;
        private readonly Dictionary<string,Label> _labels=new Dictionary<string,Label>();
        private readonly Dictionary<string,Button> _buttons=new Dictionary<string,Button>();
        private Func<string,string> _t;
        private Action<string> _setLanguage;
        private Action<bool> _setSound, _setHandedness;
        private Action<float> _setScale;
        private Action _back, _replay, _reset, _cancel, _confirm;
        private bool _built, _initialized, _russian, _sound, _left, _resetPending;
        private float _shownScale=1f, _scaleValueNow=1f;
        private static readonly Color Main=Hex("#dcecff"), Muted=Hex("#7f9bb8"), Panel=Hex("#0d1726"), Border=Hex("#243b55"), Cyan=Hex("#39d8ff");

        public void Show(float dpr,bool russian,bool sound,bool left,float scale,bool resetPending,Func<string,string> translate,
            Action<string> setLanguage,Action<bool> setSound,Action<bool> setHandedness,Action<float> setScale,
            Action back,Action replay,Action reset,Action cancel,Action confirm)
        {
            EnsureDocument();
            var clamped=Mathf.Clamp(scale,.75f,1.25f);
            var changed=!_initialized||_russian!=russian||_sound!=sound||_left!=left||_resetPending!=resetPending||Mathf.Abs(_shownScale-clamped)>.001f;
            _t=translate;_setLanguage=setLanguage;_setSound=setSound;_setHandedness=setHandedness;_setScale=setScale;
            _back=back;_replay=replay;_reset=reset;_cancel=cancel;_confirm=confirm;
            _russian=russian;_sound=sound;_left=left;_scaleValueNow=clamped;_resetPending=resetPending;
            _panel.scale=Mathf.Max(.1f,dpr);
            if(!_built)Build();
            if(changed)RefreshLabelsAndState();
            _initialized=true;_shownScale=_scaleValueNow;
            var logicalScale=Mathf.Max(.1f,dpr);var safe=Screen.safeArea;
            _content.style.width=Mathf.Min(448f,Mathf.Max(240f,Screen.width/logicalScale-48f));
            _content.style.paddingTop=Mathf.Max(28f,(Screen.height-safe.yMax)/logicalScale+16f);
            _content.style.paddingBottom=Mathf.Max(32f,safe.yMin/logicalScale+16f);
            _root.style.display=DisplayStyle.Flex;_document.rootVisualElement.BringToFront();
        }

        public void Hide(){if(_root!=null)_root.style.display=DisplayStyle.None;}

        private void EnsureDocument()
        {
            if(_document!=null)return;
            var layout=Resources.Load<VisualTreeAsset>("EchoSphere/NativeBattle/NativeSettings")??throw new InvalidOperationException("Native settings UXML is missing");
            _panelLease=new NativePanelLease(gameObject,"Echo Sphere Native Settings Panel",1001,layout);
            _document=_panelLease.Document;_panel=_panelLease.Panel;
            var panelRoot=_document.rootVisualElement;panelRoot.style.position=Position.Absolute;panelRoot.style.left=0;panelRoot.style.right=0;panelRoot.style.top=0;panelRoot.style.bottom=0;panelRoot.pickingMode=PickingMode.Ignore;
            _root=Require<VisualElement>("native-settings-root");
        }
        private T Require<T>(string name) where T:VisualElement=>_document.rootVisualElement.Q<T>(name)??throw new InvalidOperationException("Native settings layout is missing "+name);
        private void Build()
        {
            _scroll=Require<ScrollView>("settings-scroll");NativePauseScreenView.ConfigureMobileScroll(_scroll);_scroll.contentContainer.style.alignItems=Align.Center;
            _content=Require<VisualElement>("settings-content");_scale=Require<Slider>("interface-scale");
            _scaleValue=Require<Label>("settings-scale-value");_confirmation=Require<VisualElement>("settings-confirmation");_resetPrompt=Require<Label>("settings-reset-prompt");
            foreach(var key in new[]{"settingsTitle","language","sound","handedness","interfaceScale","controls","moveControls","placeSphere","activeAbilities","dashAbility"})_labels[key]=Require<Label>(key);
            void Bind(string key,Action action){var button=Require<Button>(key);button.clicked+=action;_buttons[key]=button;}
            Bind("back",()=>_back?.Invoke());Bind("ru",()=>_setLanguage?.Invoke("ru"));Bind("en",()=>_setLanguage?.Invoke("en"));
            Bind("soundOn",()=>_setSound?.Invoke(true));Bind("soundOff",()=>_setSound?.Invoke(false));
            Bind("rightHanded",()=>_setHandedness?.Invoke(false));Bind("leftHanded",()=>_setHandedness?.Invoke(true));
            Bind("replayTutorial",()=>_replay?.Invoke());Bind("reset",()=>_reset?.Invoke());Bind("cancel",()=>_cancel?.Invoke());Bind("ok",()=>_confirm?.Invoke());
            _scale.RegisterValueChangedCallback(evt=>{_scaleValueNow=evt.newValue;_scaleValue.text=Mathf.RoundToInt(evt.newValue*100f)+"%";_setScale?.Invoke(evt.newValue);});
            _built=true;
        }

        private void RefreshLabelsAndState()
        {
            string T(string key)=>_t==null?key:_t(key);
            foreach(var pair in _labels)
            {
                if(pair.Key.StartsWith("prefix:"))continue;
                pair.Value.text=pair.Key=="interfaceScale"?(_russian?"Масштаб интерфейса":"Interface scale"):T(pair.Key);
            }
            foreach(var pair in _buttons)
            {
                var key=pair.Key;var text=key=="ru"?"Русский":key=="en"?"English":key=="cancel"?(_russian?"Отмена":"Cancel"):key=="ok"?"OK":T(key);
                if(key=="back")text="← "+text;if(key=="replayTutorial")text="▷ "+text;pair.Value.text=text;
            }
            Select("ru",_russian);Select("en",!_russian);Select("soundOn",_sound);Select("soundOff",!_sound);Select("rightHanded",!_left);Select("leftHanded",_left);
            _scale.SetValueWithoutNotify(_scaleValueNow);_scaleValue.text=Mathf.RoundToInt(_scaleValueNow*100f)+"%";_resetPrompt.text=T("resetConfirm");
            _confirmation.style.display=_resetPending?DisplayStyle.Flex:DisplayStyle.None;
        }
        private void Select(string key,bool selected){var button=_buttons[key];button.style.backgroundColor=selected?new Color(Cyan.r,Cyan.g,Cyan.b,.2f):Panel;SetBorder(button,selected?new Color(Cyan.r,Cyan.g,Cyan.b,.4f):Border);button.style.color=selected?Main:Muted;}
        private static void SetBorder(VisualElement element,Color color){element.style.borderLeftWidth=element.style.borderRightWidth=element.style.borderTopWidth=element.style.borderBottomWidth=1;element.style.borderLeftColor=element.style.borderRightColor=element.style.borderTopColor=element.style.borderBottomColor=color;}
        private static Color Hex(string value){ColorUtility.TryParseHtmlString(value,out var color);return color;}
        private void OnDestroy(){_panelLease?.Dispose();}
    }
}
