#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoSphere.Presentation
{
    public sealed class VisualTimers
    {
        private sealed class Timer {public int Id;public double At,Period;public bool Frame;public V Callback;}
        private readonly Dictionary<int,Timer> _timers=new Dictionary<int,Timer>();
        private int _next;
        public double Now {get;private set;}
        public int Count=>_timers.Count;
        public V Schedule(V[] args,bool frame,bool interval)
        {
            var id=++_next;var delay=frame?0d:Math.Max(0d,VisualJs.Arg(args,1).Number);if(double.IsNaN(delay))delay=0d;
            _timers[id]=new Timer {Id=id,At=Now+delay,Period=interval?Math.Max(1d,delay):0d,Frame=frame,Callback=args[0]};return id;
        }
        public V Cancel(V[] args){_timers.Remove((int)args[0].Number);return default;}
        public void Tick(double milliseconds)
        {
            Now=milliseconds;
            foreach(var timer in _timers.Values.OrderBy(timer=>timer.Id).ToArray())if(timer.At<=Now&&_timers.ContainsKey(timer.Id)){
                if(timer.Period>0d)timer.At=Now+timer.Period;else _timers.Remove(timer.Id);
                if(timer.Frame)timer.Callback.Call(Now);else timer.Callback.Call();
            }
        }
        public void Clear()=>_timers.Clear();
    }
}
#endif
