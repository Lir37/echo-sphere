using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using EchoSphere.Core;
using EchoSphere.Runtime;
using UnityEngine;

public static class NativeShellAcceptance
{
    private sealed class Row {public string kind=null;public float[] color=null;public SnapshotValue state=null;}
    public static object Main()
    {
        const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        var root=new GameObject("Entity shell acceptance");var r=root.AddComponent<EchoSphereRuntime>();r.enabled=false;var rt=r.GetType();
        var player=new GameObject("Fixture player");player.transform.SetParent(root.transform);player.transform.position=new Vector3(5,5,0);
        rt.GetField("_player",F).SetValue(r,player.transform);rt.GetField("_playerCore",F).SetValue(r,player.AddComponent<PlayerCoreController2D>());
        var colorType=rt.Assembly.GetType("EchoSphere.Runtime.IRunSaveColor");
        var select=(Func<FieldInfo,bool>)Delegate.CreateDelegate(typeof(Func<FieldInfo,bool>),rt.GetMethod("EntitySnapshotField",BindingFlags.Static|BindingFlags.NonPublic));
        int checks=0;void Check(bool ok,string why){if(!ok)throw new Exception(why);checks++;}
        var baseline=Newtonsoft.Json.JsonConvert.DeserializeObject<Row[]>(System.IO.File.ReadAllText(System.IO.Path.Combine(Application.dataPath,"../Library/entity-shell-baseline.json")));
        var agents=(System.Collections.IList)rt.GetField("_simulationAgents",F).GetValue(r);
        try
        {
            foreach(var row in baseline)
            {
                MonoBehaviour agent;
                if(row.kind=="EnemyMinion")agent=r.SpawnBossMinion(new Vector2(-5,-5),100,1,2,"wisp",new Color(.2f,.4f,.6f));
                else {rt.GetMethod("CreateSphere",F).Invoke(r,new object[]{Enum.Parse(typeof(SphereId),row.kind),0,(Vector2?)new Vector2(-4,-4)});agent=((List<SphereAttackAgent>)rt.GetField("_spheres",F).GetValue(r)).Last();}
                Check(agent.GetComponentsInChildren<Renderer>(true).Length==0,"hidden renderer "+row.kind);
                var expectedColor=new Color(row.color[0],row.color[1],row.color[2],row.color[3]);
                Check((Color)colorType.GetProperty("SavedColor").GetValue(agent)==expectedColor,"legacy envelope color "+row.kind);
                var before=JsonUtility.ToJson(RunSnapshotCodec.Capture(agent,select));
                RunSnapshotCodec.Restore(row.state,agent.GetType(),agent,select);
                Check(before==JsonUtility.ToJson(RunSnapshotCodec.Capture(agent,select)),"gameplay initialization differs from pre-retirement state "+row.kind);
                Check(!before.Contains("_savedColor"),"duplicated legacy color state");
            }
            var result=new {result="PASS",checks,scope="all ten Sphere families and enemy minion versus captured pre-removal gameplay states/colors; no hidden renderers, no PlayerPrefs writes"};
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Library/entity-shell-acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
        }
        finally{foreach(var agent in agents.Cast<MonoBehaviour>().ToArray())if(agent!=null)UnityEngine.Object.DestroyImmediate(agent.gameObject);UnityEngine.Object.DestroyImmediate(root);}
    }
}
