using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    public sealed class NativeMainMenuScreen : MonoBehaviour
    {
        private UIDocument _document;
        private PanelSettings _panel;
        private NativeMainMenuView _view;
        private NativeMainMenuComposition _composition;
        private bool _bound,_visible;
        private int _prepared=-1,_settled;
        private NativeMainMenuView.State _state;
        public bool Ready {get;private set;}
        public NativeMainMenuView View=>_view;
        public NativeMainMenuComposition Composition=>_composition;
        public void Initialize(Func<VisualElement,NativeMainMenuView> create)
        {
            if(_document!=null)return;
            _panel=ScriptableObject.CreateInstance<PanelSettings>();_panel.name="Echo Sphere Native Main Menu Panel";
            _panel.scaleMode=PanelScaleMode.ConstantPixelSize;_panel.sortingOrder=1001;_panel.forceGammaRendering=true;
            _panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
            _document=gameObject.AddComponent<UIDocument>();_document.panelSettings=_panel;
            var root=_document.rootVisualElement;root.style.position=Position.Absolute;root.style.left=root.style.right=root.style.top=root.style.bottom=0;root.style.opacity=0;root.pickingMode=PickingMode.Ignore;
            _view=create(root);_composition=new NativeMainMenuComposition(_view);
        }
        public void Show(in NativeMainMenuView.State state,float dpr,Color characterColor)
        {
            if(_document==null)throw new InvalidOperationException("Native main menu not initialized");
            if(_panel.scale!=dpr)_panel.scale=dpr;
            if(!_visible){_view.Show(state);_composition.SetVisible(true);_settled=0;_visible=true;}
            if(!_bound||!_state.Equals(state)){_view.Show(state);_composition.Invalidate();_state=state;_bound=true;}
            var safe=Screen.safeArea;_view.SetSafeArea(safe.xMin/dpr,(Screen.height-safe.yMax)/dpr,safe.yMin/dpr);
            if(!_composition.Prepare(dpr,characterColor))return;
            if(_prepared==_composition.PrepareCount)_settled++;else _settled=0;
            _prepared=_composition.PrepareCount;
            if(_settled>=2&&!Ready){Ready=true;_document.rootVisualElement.style.opacity=1;}
        }
        public void Hide(){if(_view==null)return;_view.Hide();_composition.SetVisible(false);_visible=false;Ready=false;_document.rootVisualElement.style.opacity=0;}
        private void OnDestroy(){_composition?.Dispose();_view?.Dispose();if(_panel!=null)Destroy(_panel);}
    }
}
