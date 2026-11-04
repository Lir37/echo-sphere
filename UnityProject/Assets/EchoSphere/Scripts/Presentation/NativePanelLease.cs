using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // Scene assets are templates. Each live screen owns its scale and runtime
    // panel, so changing density never edits an asset or another screen's panel.
    internal sealed class NativePanelLease : IDisposable
    {
        internal UIDocument Document {get;}
        internal PanelSettings Panel {get;}
        private readonly PanelSettings _template;
        private bool _disposed;
        private sealed class Continuity {
            internal PanelSettings Panel;internal GameObject Keeper;internal int Users;
        }
        private static readonly Dictionary<string,Continuity> AppPanels=new Dictionary<string,Continuity>();
        private readonly Continuity _continuity;
        static NativePanelLease(){Application.quitting+=()=>{foreach(var item in AppPanels.Values){UnityEngine.Object.Destroy(item.Keeper);UnityEngine.Object.Destroy(item.Panel);}AppPanels.Clear();};}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetContinuity(){AppPanels.Clear();}
        private static Continuity RetainPanel(string key,PanelSettings panel){
            var holder=new GameObject("ECHO UI panel continuity: "+key);UnityEngine.Object.DontDestroyOnLoad(holder);
            var keeper=holder.AddComponent<UIDocument>();keeper.panelSettings=panel;keeper.sortingOrder=-100000;
            keeper.rootVisualElement.pickingMode=PickingMode.Ignore;keeper.rootVisualElement.style.width=keeper.rootVisualElement.style.height=0;
            var entry=new Continuity {Panel=panel,Keeper=holder,Users=1};AppPanels.Add(key,entry);return entry;
        }

        internal NativePanelLease(GameObject owner,string name,int order,VisualTreeAsset layout=null,float? initialScale=null)
        {
            Document=owner.GetComponent<UIDocument>();
            if(Document!=null&&Document.panelSettings==null)
                throw new InvalidOperationException(name+" has an authored document without panel settings");
            _template=Document!=null?Document.panelSettings:null;
            // A scene reload must not tear down the last runtime panel while
            // filtered/clipped UI commands from it are still retiring on Android.
            // Keep one empty native document per logical screen for app lifetime.
            // Concurrent fixtures/screens still receive independent panels/scales.
            string key=name+":"+(_template!=null?_template.name:"runtime");
            bool reuse=Application.isPlaying&&AppPanels.TryGetValue(key,out var existing)&&existing.Users==0;
            if(reuse){_continuity=AppPanels[key];_continuity.Users++;Panel=_continuity.Panel;}
            else Panel=_template!=null?UnityEngine.Object.Instantiate(_template):ScriptableObject.CreateInstance<PanelSettings>();
            Panel.name=name+" (runtime)";Panel.hideFlags=HideFlags.DontSave;
            if(_template==null){
                Panel.scaleMode=PanelScaleMode.ConstantPixelSize;Panel.sortingOrder=order;Panel.forceGammaRendering=true;
                Panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
            }
            if(initialScale.HasValue)Panel.scale=initialScale.Value;
            if(Application.isPlaying&&_continuity==null&&!AppPanels.ContainsKey(key))_continuity=RetainPanel(key,Panel);
            try{
                // UIDocument attachment can create and lay out the runtime panel.
                // Supply the screen's density before that first attachment.
                if(initialScale.HasValue)Panel.scale=initialScale.Value;
                if(Document==null)Document=owner.AddComponent<UIDocument>();
                Document.panelSettings=Panel;
                if(layout!=null&&Document.visualTreeAsset!=layout)Document.visualTreeAsset=layout;
                Document.enabled=true;
                Document.rootVisualElement.style.overflow=Overflow.Hidden;
            }catch{if(_continuity!=null)_continuity.Users--;else UnityEngine.Object.Destroy(Panel);throw;}
        }
        public void Dispose()
        {
            if(_disposed)return;_disposed=true;
            if(Document!=null){Document.enabled=false;Document.panelSettings=_template;}
            if(_continuity!=null)_continuity.Users--;else UnityEngine.Object.Destroy(Panel);
        }
    }
}
