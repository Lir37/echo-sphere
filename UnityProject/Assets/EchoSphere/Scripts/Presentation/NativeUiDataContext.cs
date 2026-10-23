using System;

namespace EchoSphere.Presentation
{
    // Storage/date compatibility for retained catalogue/read-model functions.
    // This is plain data: no GameObject, DOM, timers, hooks, layout or renderer.
    internal sealed class NativeUiDataContext
    {
        internal bool Dirty=true;
        #if UNITY_EDITOR
        internal Func<string,V> ReadStorage;
        internal Action<string,V> WriteStorage;
        internal Action<string> RemoveStorage;
        internal Action Reload;
        private readonly V _date=new V(new VisualDateHost());
        private readonly V _storage;
        internal NativeUiDataContext()
        {
            _storage=VisualJs.Obj("getItem",VisualJs.Fn(args=>ReadStorage?.Invoke(args[0].Text)??VisualJs.Null),
                "setItem",VisualJs.Fn(args=>{WriteStorage?.Invoke(args[0].Text,args[1]);return default;}),
                "removeItem",VisualJs.Fn(args=>{RemoveStorage?.Invoke(args[0].Text);return default;}));
        }
        internal V Get(string name)
        {
            switch(name){case "localStorage":return _storage;case "Date":return _date;
                case "performance":return VisualJs.PerformanceObject;
                case "location":return VisualJs.Obj("reload",VisualJs.Fn(args=>{Reload?.Invoke();return default;}));
                default:throw new InvalidOperationException("Native data context has no UI environment API: "+name);}
        }
        #endif
    }
}
