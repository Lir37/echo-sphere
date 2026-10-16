using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    /// <summary>Native UI Toolkit pilot for the existing Settings screen.</summary>
    public sealed class SourceSettingsToolkitScreen : MonoBehaviour
    {
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
        private static readonly Color Main=Hex("#dcecff"), Muted=Hex("#7f9bb8"), Panel=Hex("#0d1726"), Border=Hex("#243b55"), Cyan=Hex("#39d8ff"), Red=Hex("#ff4d5d");

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
            _panel=ScriptableObject.CreateInstance<PanelSettings>();_panel.name="Echo Sphere Native Settings Panel";
            _panel.scaleMode=PanelScaleMode.ConstantPixelSize;_panel.sortingOrder=1001;_panel.forceGammaRendering=true;
            _panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
            _document=gameObject.AddComponent<UIDocument>();_document.panelSettings=_panel;
            var panelRoot=_document.rootVisualElement;panelRoot.style.position=Position.Absolute;panelRoot.style.left=0;panelRoot.style.right=0;panelRoot.style.top=0;panelRoot.style.bottom=0;panelRoot.pickingMode=PickingMode.Ignore;
            _root=new VisualElement();_root.name="native-settings-root";_root.pickingMode=PickingMode.Ignore;
            _root.style.position=Position.Absolute;_root.style.left=0;_root.style.right=0;_root.style.top=0;_root.style.bottom=0;_root.style.backgroundColor=Color.clear;_root.style.display=DisplayStyle.None;
            panelRoot.Add(_root);
        }

        private void Build()
        {
            _scroll=new ScrollView(ScrollViewMode.Vertical);_scroll.style.position=Position.Absolute;_scroll.style.left=0;_scroll.style.right=0;_scroll.style.top=0;_scroll.style.bottom=0;
            _scroll.style.backgroundColor=Color.clear;_scroll.contentContainer.style.alignItems=Align.Center;_root.Add(_scroll);
            _content=new VisualElement();_content.style.flexDirection=FlexDirection.Column;_content.style.flexShrink=0;_content.style.maxWidth=448;_content.style.width=448;
            _content.style.paddingLeft=24;_content.style.paddingRight=24;_content.style.paddingTop=28;_content.style.paddingBottom=32;_scroll.Add(_content);

            AddButton("back",()=>_back?.Invoke(),"← ");var title=AddLabel("settingsTitle",24,Main,true);title.style.unityTextAlign=TextAnchor.MiddleCenter;title.style.marginTop=12;title.style.marginBottom=24;
            AddSection("language");var language=Pair();AddButton("ru",()=>_setLanguage?.Invoke("ru"));AddButton("en",()=>_setLanguage?.Invoke("en"));_content.Add(language); // Buttons are attached by AddButton to the current row below.
            AddSection("sound");var sound=Pair();AddButton("soundOn",()=>_setSound?.Invoke(true));AddButton("soundOff",()=>_setSound?.Invoke(false));_content.Add(sound);
            AddSection("handedness");var hand=Pair();AddButton("rightHanded",()=>_setHandedness?.Invoke(false));AddButton("leftHanded",()=>_setHandedness?.Invoke(true));_content.Add(hand);
            AddSection("interfaceScale");var scaleRow=Pair();var low=new Label("75%");low.style.color=Muted;scaleRow.Add(low);
            _scale=new Slider(.75f,1.25f);_scale.style.flexGrow=1;_scale.style.marginLeft=10;_scale.style.marginRight=10;scaleRow.Add(_scale);
            var high=new Label("125%");high.style.color=Muted;scaleRow.Add(high);_content.Add(scaleRow);
            _scaleValue=new Label();_scaleValue.style.color=Main;_scaleValue.style.unityTextAlign=TextAnchor.MiddleCenter;_scaleValue.style.marginBottom=20;_content.Add(_scaleValue);
            _scale.RegisterValueChangedCallback(evt=>{_scaleValueNow=evt.newValue;_scaleValue.text=Mathf.RoundToInt(evt.newValue*100f)+"%";_setScale?.Invoke(evt.newValue);});
            AddSection("controls");var card=new VisualElement();card.style.flexDirection=FlexDirection.Column;card.style.backgroundColor=Panel;
            SetBorder(card,Border);card.style.borderTopLeftRadius=12;card.style.borderTopRightRadius=12;card.style.borderBottomLeftRadius=12;card.style.borderBottomRightRadius=12;
            card.style.paddingLeft=16;card.style.paddingRight=16;card.style.paddingTop=16;card.style.paddingBottom=8;card.style.marginBottom=24;
            foreach(var key in new[]{"moveControls","placeSphere","activeAbilities","dashAbility"}){var line=new Label();line.style.color=Muted;line.style.fontSize=12;line.style.whiteSpace=WhiteSpace.Normal;line.style.marginBottom=8;_labels[key]=line;card.Add(line);}_content.Add(card);
            AddButton("replayTutorial",()=>_replay?.Invoke(),"▷ ");AddButton("reset",()=>_reset?.Invoke());
            _confirmation=new VisualElement();_confirmation.style.flexDirection=FlexDirection.Column;_confirmation.style.marginTop=16;_confirmation.style.paddingTop=12;_confirmation.style.paddingBottom=12;
            _resetPrompt=new Label();_resetPrompt.style.color=Main;_resetPrompt.style.whiteSpace=WhiteSpace.Normal;_resetPrompt.style.marginBottom=12;_confirmation.Add(_resetPrompt);
            var row=Pair();AddButton("cancel",()=>_cancel?.Invoke());AddButton("ok",()=>_confirm?.Invoke());_content.Add(_confirmation);_built=true;
        }

        private VisualElement _activeRow;
        private void AddSection(string key){var label=AddLabel(key,11,Muted,false);label.style.marginBottom=8;}
        private Label AddLabel(string key,float size,Color color,bool bold)
        {var label=new Label();label.style.fontSize=size;label.style.color=color;label.style.unityFontStyleAndWeight=bold?FontStyle.Bold:FontStyle.Normal;_labels[key]=label;_content.Add(label);return label;}
        private void AddButton(string key,Action action,string prefix="")
        {
            var button=new Button();button.style.flexGrow=1;button.style.minHeight=42;button.style.paddingLeft=10;button.style.paddingRight=10;button.style.paddingTop=10;button.style.paddingBottom=10;
            button.style.marginRight=8;button.style.fontSize=13;button.style.color=Main;button.style.backgroundColor=Panel;SetBorder(button,Border);
            button.style.borderTopLeftRadius=12;button.style.borderTopRightRadius=12;button.style.borderBottomLeftRadius=12;button.style.borderBottomRightRadius=12;
            button.clicked+=action;_buttons[key]=button;
            if(prefix.Length>0)_labels["prefix:"+key]=new Label(prefix);
            if(_activeRow!=null)_activeRow.Add(button);else _content.Add(button);
        }
        private VisualElement Pair()
        {var row=new VisualElement();row.style.flexDirection=FlexDirection.Row;row.style.width=Length.Percent(100);row.style.marginBottom=24;_activeRow=row;return row;}
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
        private void OnDestroy(){if(_panel!=null)Destroy(_panel);}
    }
}
