using System;
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

        internal NativePanelLease(GameObject owner,string name,int order,VisualTreeAsset layout=null,float? initialScale=null)
        {
            Document=owner.GetComponent<UIDocument>();
            if(Document!=null&&Document.panelSettings==null)
                throw new InvalidOperationException(name+" has an authored document without panel settings");
            _template=Document!=null?Document.panelSettings:null;
            Panel=_template!=null?UnityEngine.Object.Instantiate(_template):ScriptableObject.CreateInstance<PanelSettings>();
            Panel.name=name+" (runtime)";Panel.hideFlags=HideFlags.DontSave;
            if(_template==null){
                Panel.scaleMode=PanelScaleMode.ConstantPixelSize;Panel.sortingOrder=order;Panel.forceGammaRendering=true;
                Panel.themeStyleSheet=Resources.Load<ThemeStyleSheet>("EchoSphere/SourceUiTheme");
            }
            try{
                // UIDocument attachment can create and lay out the runtime panel.
                // Supply the screen's density before that first attachment.
                if(initialScale.HasValue)Panel.scale=initialScale.Value;
                if(Document==null)Document=owner.AddComponent<UIDocument>();
                Document.panelSettings=Panel;
                if(layout!=null&&Document.visualTreeAsset!=layout)Document.visualTreeAsset=layout;
                Document.enabled=true;
            }catch{UnityEngine.Object.Destroy(Panel);throw;}
        }
        public void Dispose()
        {
            if(_disposed)return;_disposed=true;
            if(Document!=null){Document.enabled=false;Document.panelSettings=_template;}
            UnityEngine.Object.Destroy(Panel);
        }
    }
}
