using System;
using System.Collections.Generic;
using System.Reflection;
using EchoSphere.Runtime;
using UnityEngine;

public class EchoPhaseProbe : MonoBehaviour, ISourceSimulationAgent
{
    public Action Tick;
    public void SimulationTick(float dt)=>Tick?.Invoke();
}
public sealed class EchoOtherPhaseProbe : MonoBehaviour, ISourceSimulationAgent
{
    public Action Tick;
    public void SimulationTick(float dt)=>Tick?.Invoke();
}
public static class EchoPhaseOrderCheck
{
    public static object Main()
    {
        var owners=new List<GameObject>();var trace=new List<string>();int checks=0;
        T Create<T>(string name) where T:Component{var go=new GameObject(name);owners.Add(go);return go.AddComponent<T>();}
        var runtime=Create<EchoSphereRuntime>("Phase ordering test");runtime.enabled=false;
        var flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var tick=typeof(EchoSphereRuntime).GetMethod("TickSimulationPhase",flags);
        void Run<T>(bool reverse) where T:MonoBehaviour,ISourceSimulationAgent{trace.Clear();tick.MakeGenericMethod(typeof(T)).Invoke(runtime,new object[]{.01f,reverse});}
        void Expect(string expected){string actual=string.Join(",",trace);if(actual!=expected)throw new Exception(expected+" != "+actual);checks++;}
        try{
            var a=Create<EchoPhaseProbe>("a");var b=Create<EchoPhaseProbe>("b");var hidden=Create<EchoPhaseProbe>("hidden");
            runtime.RegisterSimulationAgent(a);runtime.RegisterSimulationAgent(b);runtime.RegisterSimulationAgent(hidden);hidden.gameObject.SetActive(false);
            bool spawned=false;a.Tick=()=>{trace.Add("a");if(spawned)return;spawned=true;var c=Create<EchoPhaseProbe>("c");c.Tick=()=>trace.Add("c");runtime.RegisterSimulationAgent(c);var other=Create<EchoOtherPhaseProbe>("other");other.Tick=()=>trace.Add("other");runtime.RegisterSimulationAgent(other);};
            b.Tick=()=>trace.Add("b");hidden.Tick=()=>trace.Add("hidden");
            Run<EchoPhaseProbe>(false);Expect("a,b"); // born during phase waits
            Run<EchoOtherPhaseProbe>(false);Expect("other"); // later phase sees birth
            Run<EchoPhaseProbe>(true);Expect("c,b,a");
            hidden.gameObject.SetActive(true);Run<EchoPhaseProbe>(false);Expect("a,b,hidden,c");
            runtime.RegisterSimulationAgent(a);Run<EchoPhaseProbe>(false);Expect("a,b,hidden,c"); // duplicate rejected
            UnityEngine.Object.DestroyImmediate(b.gameObject);Run<EchoPhaseProbe>(true);Expect("c,hidden,a");
            a.Tick=()=>{trace.Add("a");hidden.gameObject.SetActive(false);};Run<EchoPhaseProbe>(false);Expect("a,c");
            return new{result="PASS",checks};
        }finally{foreach(var go in owners)if(go!=null)UnityEngine.Object.DestroyImmediate(go);}
    }
}
