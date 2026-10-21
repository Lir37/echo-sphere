using System;
using System.Collections.Generic;
using System.Linq;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeMobileControlsView _nativeMobileDock;
        private NativeMobileControlsView.State _nativeMobileDockState;
        private readonly Dictionary<string,V> _nativeMobileDockActions=new Dictionary<string,V>();
        private static bool NativeDockClass(string text)=>NativeClasses(text,"bottom-4","flex-col","pointer-events-none");
        private static bool NativeClasses(string text,params string[] required){var tokens=(text??"").Split(' ');return required.All(t=>Array.IndexOf(tokens,t)>=0);}
        private void DisposeNativeMobileDock(){_nativeMobileDock?.Dispose();_nativeMobileDock=null;}
        private V BuildNativeSourceUiTree()
        {
            var tree=SourceUiTree();CaptureNativeMobileDock(tree);
            return ProjectNativeMobileDock(ProjectNativeTutorial(ProjectNativeArtifactRewards(ProjectNativeUpgrade(ProjectNativeRunResult(ProjectNativePauseScreen(ProjectNativeHudIsland(tree)))))));
        }
        private void CaptureNativeMobileDock(V tree)
        {
            _nativeMobileDockState=null;_nativeMobileDockActions.Clear();
            string Plain(V v){if(v.Raw is VisualDom.Node n)return n.Tag=="svg"?"":string.Concat(n.Children.Select(Plain));if(v.Raw is List<V>)return string.Concat(v.Items.Select(Plain));return v.Null||v.Undefined||v.Raw is bool?"":v.Text;}
            IEnumerable<VisualDom.Node> Nodes(V value){if(value.Raw is VisualDom.Node node){yield return node;foreach(var child in node.Children)foreach(var n in Nodes(child))yield return n;}else if(value.Raw is List<V>)foreach(var child in value.Items)foreach(var n in Nodes(child))yield return n;}
            var mobile=Nodes(tree).FirstOrDefault(n=>NativeClasses(n.Properties["className"].Text,"es-mobile-controls"));if(mobile==null)return;
            var dock=Nodes(new V(mobile)).FirstOrDefault(n=>NativeDockClass(n.Properties["className"].Text));if(dock==null)return;
            var state=new NativeMobileControlsView.State {Right=IsLeftHanded};string lang=IsRussianUi?"ru":"en";
            string T(string key)=>Visual_i18n.Scope["translations"][lang][key].Text;
            foreach(var n in Nodes(new V(dock))){
                var props=n.Properties;string css=props["className"].Text??"";
                if(n.Tag=="button"&&props["data-mobile-control"].Text=="true"){
                    string key=props["key"].Text,id=null,icon=null;
                    if(props["data-game-control"].Text=="formation-follow"){id="follow";icon="Move";}
                    else if(props["aria-label"].Text==T("pause")){id="pause";icon="Pause";}
                    else if(props["aria-label"].Text=="Блокировка перемещения сфер"||props["aria-label"].Text=="Lock sphere movement"){id="lock";icon=_sourceRenderState["player"]["sphereMovementLocked"].Truth?"Lock":"Unlock";}
                    else if(props["aria-label"].Text==T("dashCooldown")){id="dash";icon="Zap";}
                    else if(!string.IsNullOrEmpty(key)&&Visual_gameData.Scope["SPHERE_TYPES"][key].Truth)id="sphere:"+key;
                    else if(!string.IsNullOrEmpty(key))id="ability:"+key;
                    if(id==null)throw new InvalidOperationException("Unrecognized source mobile control");
                    var spans=Nodes(new V(n)).Where(s=>s.Tag=="span").ToList();float cooldown=0;
                    foreach(var span in spans){string width=span.Properties["style"]["width"].Text;if(width!=null&&width.EndsWith("%",StringComparison.Ordinal))float.TryParse(width.TrimEnd('%'),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out cooldown);}
                    state.Controls.Add(new NativeMobileControlsView.Control {Id=id,Icon=icon,Name=spans.Count>0?Plain(new V(spans[0])):"",Detail=spans.Count>1?Plain(new V(spans[1])):"",Tooltip=props["aria-label"].Text,Disabled=props["disabled"].Truth,Cooldown=cooldown,Color=id.StartsWith("sphere:")?Visual_gameData.Scope["SPHERE_TYPES"][key]["color"].Text:null});
                    _nativeMobileDockActions[id]=props["onPointerDown"];
                }
                if(NativeClasses(css,"w-[220px]")){var rows=n.Children.Where(c=>c.Raw is VisualDom.Node).ToList();state.Name=rows.Count>0?Plain(rows[0]):"";state.Description=rows.Count>1?Plain(rows[1]):"";}
                if(NativeClasses(css,"tracking-[0.16em]","rounded-md"))state.Strain=Plain(new V(n));
            }
            _nativeMobileDockState=state;
        }
        private void BindNativeMobileInput(Button button,string id)
        {
            // Semantic marker only: no CSS/layout tree. It preserves the original
            // closest('[data-mobile-control]') guard even for disabled controls.
            var node=new VisualCssNode {Tag="button",Native=button,Properties=VisualJs.Obj("data-mobile-control","true")};
            var host=new SourceUiElement(_sourceUi,node,button);button.userData=host;
            button.RegisterCallback<PointerDownEvent>(evt=>{evt.StopPropagation();if(!button.enabledInHierarchy)return;if(_nativeMobileDockActions.TryGetValue(id,out var action)){node.Properties["onPointerDown"]=action;host.Send("onPointerDown",evt);}});
        }
        private void PrepareNativeMobileDock(VisualCssNode root)
        {
            if(_nativeMobileDockState==null){DisposeNativeMobileDock();return;}
            VisualCssNode dock=null;void Find(VisualCssNode n){if(dock==null&&NativeDockClass(n.Attribute("className")))dock=n;foreach(var c in n.Children)Find(c);}Find(root);
            if(!(dock?.Native is SourceUiFrame frame)){DisposeNativeMobileDock();return;}
            if(_nativeMobileDock==null||_nativeMobileDock.Root.parent!=frame.Content){DisposeNativeMobileDock();_nativeMobileDock=new NativeMobileControlsView(frame.Content,BindNativeMobileInput);}
            _nativeMobileDock.Bind(_nativeMobileDockState);bool ready=_nativeMobileDock.Ready;_nativeMobileDock.Prepare(SourceDevicePixelRatio);
            if(_nativeMobileDock.Ready){foreach(var c in dock.Children)if(c.Native is VisualElement e)e.style.display=DisplayStyle.None;if(!ready)_sourceUi.Dirty=true;}
        }
        private V ProjectNativeMobileDock(V tree)
        {
            if(_nativeMobileDock==null||!_nativeMobileDock.Ready||_nativeMobileDockState==null)return tree;
            void Visit(V value){if(value.Raw is VisualDom.Node n){if(NativeDockClass(n.Properties["className"].Text)){n.Children.Clear();return;}foreach(var c in n.Children)Visit(c);}else if(value.Raw is List<V>)foreach(var c in value.Items)Visit(c);}Visit(tree);return tree;
        }
    }
}
