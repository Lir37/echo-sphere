#if UNITY_EDITOR
using System;
using System.Globalization;

namespace EchoSphere.Presentation
{
    public sealed class VisualDateHost : IVisualHost,IVisualCallable
    {
        private readonly Func<double> _clock;
        private readonly V _now;
        public VisualDateHost(Func<double> clock=null){_clock=clock??(()=>DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());_now=VisualJs.Fn(args=>_clock());}
        public V Get(string name)=>name=="now"?_now:default;
        public void Set(string name,V value)=>throw new InvalidOperationException("Unimplemented authored Date member "+name);
        public V Call(V[] args)
        {
            var milliseconds=args.Length==0?_clock():args[0].Number;var date=DateTimeOffset.FromUnixTimeMilliseconds((long)milliseconds).ToLocalTime();
            return VisualJs.Obj("toLocaleDateString",VisualJs.Fn(parameters=>{
                var culture=parameters.Length==0?CultureInfo.CurrentCulture:CultureInfo.GetCultureInfo(parameters[0].Text);
                return date.ToString("d",culture);
            }));
        }
    }
}
#endif
