using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    internal sealed class SourceUiElement : IVisualHost
    {
        private readonly SourceUiDocument _document;
        internal VisualCssNode Node;
        internal readonly VisualElement Element;
        internal float GridHeight=float.NaN;
        internal SourceUiElement(SourceUiDocument document,VisualCssNode node,VisualElement element){_document=document;Node=node;Element=element;}
        public V Get(string name)
        {
            switch(name){
                case "value":return Element is TextField field?field.value:Node.Properties["value"];
                case "width":if(Node.Tag=="canvas")return _document.CanvasTexture?.Invoke()?.width??300;return Node.Properties[name];
                case "height":if(Node.Tag=="canvas")return _document.CanvasTexture?.Invoke()?.height??150;return Node.Properties[name];
                case "dataset":var data=VisualJs.Obj();foreach(var pair in (Dictionary<string,V>)Node.Properties.Raw)if(pair.Key.StartsWith("data-",StringComparison.Ordinal))data[Regex.Replace(pair.Key.Substring(5),"-([a-z])",match=>match.Groups[1].Value.ToUpperInvariant())]=pair.Value;return data;
                case "getBoundingClientRect":return VisualJs.Fn(args=>{var r=Element.worldBound;return VisualJs.Obj("x",r.x,"y",r.y,"left",r.xMin,"top",r.yMin,"right",r.xMax,"bottom",r.yMax,"width",r.width,"height",r.height);});
                case "closest":return VisualJs.Fn(args=>{for(var current=Node;current!=null;current=current.Parent)if(VisualCss.Match(current,args[0].Text)&&current.Native is VisualElement native&&native.userData is SourceUiElement host)return new V(host);return VisualJs.Null;});
                case "querySelector":return VisualJs.Fn(args=>_document.Query(args[0].Text,Node));
                case "querySelectorAll":return VisualJs.Fn(args=>_document.Query(args[0].Text,Node,true));
                case "getAttribute":return VisualJs.Fn(args=>Node.Attribute(args[0].Text));
                case "setAttribute":return VisualJs.Fn(args=>{Node.Properties[args[0]]=args[1];_document.Dirty=true;return default;});
                case "parentElement":return Node.Parent?.Native is VisualElement parent&&parent.userData is SourceUiElement parentHost?new V(parentHost):VisualJs.Null;
                case "setPointerCapture":return VisualJs.Fn(args=>{Element.CapturePointer((int)args[0].Number);return default;});
                case "releasePointerCapture":return VisualJs.Fn(args=>{Element.ReleasePointer((int)args[0].Number);return default;});
                case "hasPointerCapture":return VisualJs.Fn(args=>Element.HasPointerCapture((int)args[0].Number));
                case "textContent":string Text(VisualCssNode item)=>item.Tag=="#text"?item.Text:string.Concat(item.Children.Select(Text));return Text(Node);
                default:return Node.Properties[name];
            }
        }
        public void Set(string name,V value){Node.Properties[name]=value;if(name=="value"&&Element is TextField field)field.SetValueWithoutNotify(value.Text);_document.Dirty=true;}
        internal static SourceUiElement Target(VisualElement element){for(var current=element;current!=null;current=current.parent)if(current.userData is SourceUiElement host)return host;return null;}
        // Preserve the source DOM preventDefault contract; Unity marks this API obsolete but still exposes no equivalent for arbitrary default actions.
#pragma warning disable 618
        internal void Send(string name,EventBase source)
        {
            var callback=Node.Properties[name];if(!(callback.Raw is Func<V[],V>))return;
            var target=Target(source.target as VisualElement);var evt=VisualJs.Obj("target",target==null?VisualJs.Null:new V(target),"currentTarget",new V(this),"stopPropagation",VisualJs.Fn(args=>{source.StopPropagation();return default;}),"preventDefault",VisualJs.Fn(args=>{source.PreventDefault();return default;}));
            if(source is IPointerEvent pointer){evt["pointerId"]=pointer.pointerId;evt["clientX"]=pointer.position.x;evt["clientY"]=pointer.position.y;evt["button"]=pointer.button;evt["buttons"]=pointer.pressedButtons;evt["pointerType"]=pointer.pointerType;}
            if(source is IKeyboardEvent keyboard){evt["key"]=keyboard.keyCode==KeyCode.Return||keyboard.keyCode==KeyCode.KeypadEnter?"Enter":keyboard.keyCode==KeyCode.Escape?"Escape":keyboard.character.ToString();evt["ctrlKey"]=keyboard.ctrlKey;evt["shiftKey"]=keyboard.shiftKey;}
            evt["nativeEvent"]=evt;_document.Environment.Invoke(callback,evt);_document.EventCompleted();
        }
#pragma warning restore 618
        internal void Change(string value)
        {var evt=VisualJs.Obj("target",VisualJs.Obj("value",value),"currentTarget",new V(this));foreach(var name in new[]{"onInput","onChange"})if(Node.Properties[name].Raw is Func<V[],V>)_document.Environment.Invoke(Node.Properties[name],evt);_document.EventCompleted();}
    }
    public sealed class SourceUiEnvironment : IDisposable
    {
        private sealed class ConfirmationPending : Exception {}
        private Action _currentInvocation;
        private bool? _confirmAnswer;
        private readonly SourceUiDocument _document;
        private readonly VisualTimers _timers=new VisualTimers();
        private readonly Dictionary<string,List<V>> _listeners=new Dictionary<string,List<V>>();
        private readonly V _window,_storage,_documentObject;
        private readonly V _date=new V(new VisualDateHost());
        public Func<string,V> ReadStorage;
        public Action<string,V> WriteStorage;
        public Action<string> RemoveStorage;
        public Action Reload;
        public Func<string,bool> Confirm;
        internal void Invoke(V callback,V argument){Action invocation=null;invocation=()=>{var previous=_currentInvocation;_currentInvocation=invocation;try{callback.Call(argument);}catch(ConfirmationPending){}finally{_currentInvocation=previous;}};invocation();}
        public SourceUiEnvironment(SourceUiDocument document)
        {
            _document=document;
            V Schedule(V[] args,bool frame,bool interval)=>_timers.Schedule(args,frame,interval);
            var clear=VisualJs.Fn(_timers.Cancel);
            var add=VisualJs.Fn(args=>{var name=args[0].Text;if(!_listeners.TryGetValue(name,out var handlers)){handlers=new List<V>();_listeners.Add(name,handlers);}if(!handlers.Contains(args[1]))handlers.Add(args[1]);return default;});
            var remove=VisualJs.Fn(args=>{if(_listeners.TryGetValue(args[0].Text,out var handlers))handlers.Remove(args[1]);return default;});
            _window=VisualJs.Obj("innerWidth",0,"innerHeight",0,"setTimeout",VisualJs.Fn(args=>Schedule(args,false,false)),"clearTimeout",clear,"setInterval",VisualJs.Fn(args=>Schedule(args,false,true)),"clearInterval",clear,"requestAnimationFrame",VisualJs.Fn(args=>Schedule(args,true,false)),"cancelAnimationFrame",clear,"addEventListener",add,"removeEventListener",remove);
            _storage=VisualJs.Obj("getItem",VisualJs.Fn(args=>ReadStorage?.Invoke(args[0].Text)??VisualJs.Null),"setItem",VisualJs.Fn(args=>{WriteStorage?.Invoke(args[0].Text,args[1]);return default;}),"removeItem",VisualJs.Fn(args=>{RemoveStorage?.Invoke(args[0].Text);return default;}));
            _documentObject=VisualJs.Obj("querySelector",VisualJs.Fn(args=>_document.Query(args[0].Text)),"querySelectorAll",VisualJs.Fn(args=>_document.Query(args[0].Text,null,true)),"elementFromPoint",VisualJs.Fn(args=>_document.ElementFromPoint((float)args[0].Number,(float)args[1].Number)),"addEventListener",add,"removeEventListener",remove,"visibilityState","visible");
            _window["location"]=Get("location");_window["confirm"]=Get("confirm");_window["navigator"]=Get("navigator");_window["performance"]=VisualJs.PerformanceObject;_window["localStorage"]=_storage;_window["document"]=_documentObject;
        }
        private void UpdateViewport(){_window["innerWidth"]=_document.Sheet.Width;_window["innerHeight"]=_document.Sheet.Height;_window["devicePixelRatio"]=_document.Scale;}
        public void Tick(double milliseconds){UpdateViewport();_timers.Tick(milliseconds);}
        public void Dispatch(string name){UpdateViewport();if(_listeners.TryGetValue(name,out var callbacks))foreach(var callback in callbacks.ToArray())callback.Call();}
        public V Get(string name)
        {
            if(!_window[name].Undefined)return _window[name];
            switch(name){
                case "window":return _window;case "document":return _documentObject;case "localStorage":return _storage;
                case "isElement":return VisualJs.Fn(args=>args[0].Raw is SourceUiElement);
                case "navigator":return VisualJs.Obj("language",Application.systemLanguage==SystemLanguage.English?"en":"ru","vibrate",VisualJs.Fn(args=>{
#if UNITY_ANDROID || UNITY_IOS
                    Handheld.Vibrate();
#endif
                    return true;}));
                case "Date":return _date;
                case "location":return VisualJs.Obj("reload",VisualJs.Fn(args=>{Reload?.Invoke();return default;}));
                case "confirm":return VisualJs.Fn(args=>{if(Confirm!=null)return Confirm(args[0].Text);if(_confirmAnswer.HasValue){var answer=_confirmAnswer.Value;_confirmAnswer=null;return answer;}var resume=_currentInvocation;if(resume==null)throw new InvalidOperationException("Source confirm called outside a UI event");_document.ShowConfirmation(args[0].Text,answer=>{_confirmAnswer=answer;resume();_confirmAnswer=null;_document.EventCompleted();});throw new ConfirmationPending();});
                default:throw new InvalidOperationException("Unbound source UI environment API: "+name);
            }
        }
        public void Dispose(){_timers.Clear();_listeners.Clear();}
    }
}
