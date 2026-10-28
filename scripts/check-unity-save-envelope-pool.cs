using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using EchoSphere.Core;
using EchoSphere.Runtime;
using UnityEngine;

public static class SaveEnvelopeAcceptance
{
    public static object Main()
    {
        const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
        var root=new GameObject("Save envelope acceptance");var runtime=root.AddComponent<EchoSphereRuntime>();runtime.enabled=false;var rt=runtime.GetType();
        var player=new GameObject("Fixture player");player.transform.SetParent(root.transform);player.transform.position=new Vector3(5,5,0);
        rt.GetField("_player",F).SetValue(runtime,player.transform);rt.GetField("_playerCore",F).SetValue(runtime,player.AddComponent<PlayerCoreController2D>());
        var region=root.AddComponent<RegionFieldVisuals>();region.Initialize(runtime);rt.GetField("_regionFieldVisuals",F).SetValue(runtime,region);
        var agents=(IList)rt.GetField("_simulationAgents",F).GetValue(runtime);var enemies=(IList)rt.GetField("_enemies",F).GetValue(runtime);
        var buffers=rt.GetField("_runSaveBuffers",F).GetValue(runtime);var end=buffers.GetType().GetMethod("End",F);var capture=rt.GetMethod("CaptureRunSnapshot",F);
        int checks=0;void Check(bool ok,string why){if(!ok)throw new Exception(why);checks++;}
        object Capture(){var saved=capture.Invoke(runtime,null);end.Invoke(buffers,null);saved.GetType().GetField("savedAt",F).SetValue(saved,0L);return saved;}
        IList Entities(object saved)=>(IList)saved.GetType().GetField("entities",F).GetValue(saved);
        object Field(object target,string name)=>target.GetType().GetField(name,F).GetValue(target);
        try
        {
            var enemy=runtime.SpawnBossMinion(new Vector2(-3,-4),100,1,2,"wisp",Color.cyan);
            runtime.SpawnSourceParticle(new Vector2(-2,-3),Vector2.zero,1,3,"#ff0000");
            var bg=new GameObject("Owned bullet fixture");var bullet=bg.AddComponent<BossProjectileAgent2D>();bullet.Initialize(runtime,player.transform,Vector2.right,8,10,"shooter",Color.cyan);bullet.SetSourceOwner(enemy);
            var first=Capture();var record=Entities(first)[0];string json=JsonUtility.ToJson(first);
            Check(Entities(first).Count==3,"save entity count");
            Check((int)Field(first,"version")==2,"save version changed");
            Check((bool)Field(Entities(first)[2],"hasProjectileOwner")&&(int)Field(Entities(first)[2],"projectileOwnerIndex")==0,"owner mapping changed");
            var second=Capture();Check(ReferenceEquals(first,second),"run envelope recreated");Check(ReferenceEquals(record,Entities(second)[0]),"entity envelope recreated");
            Check(json==JsonUtility.ToJson(second),"unchanged state serialized differently across captures");
            // Reuse a former enemy record for a particle. No variant, auxiliary
            // boss state or projectile owner flags may leak from an earlier save.
            record.GetType().GetField("auxiliary",F).SetValue(record,new SnapshotValue());
            agents.Remove(enemy);enemies.Remove(enemy);agents.Remove(bullet);UnityEngine.Object.DestroyImmediate(enemy.gameObject);UnityEngine.Object.DestroyImmediate(bg);
            var third=Capture();Check(Entities(third).Count==1,"retired entities retained in save");
            var particle=Entities(third)[0];Check((string)Field(particle,"kind")==nameof(SourceParticle2D),"wrong reused kind");
            Check(Field(particle,"variant")==null&&Field(particle,"auxiliary")==null,"stale enemy/boss fields");
            Check(!(bool)Field(particle,"hasProjectileOwner")&&(int)Field(particle,"projectileOwnerIndex")==0,"stale owner fields");
            Check((Color)Field(particle,"color")==Color.red,"particle envelope color changed");
            var spare=(IList)buffers.GetType().GetField("_records",F).GetValue(buffers);
            for(int i=1;i<spare.Count;i++)Check(Field(spare[i],"state")==null&&Field(spare[i],"auxiliary")==null,"spare records retain dead snapshot graphs");
            var roundtrip=JsonUtility.FromJson(JsonUtility.ToJson(third),third.GetType());
            Check(Entities(roundtrip).Count==1&&(int)Field(roundtrip,"version")==2,"existing JSON envelope roundtrip");
            var result=new {result="PASS",checks,scope="actual capture/JSON pipeline without PlayerPrefs; reused records, stable JSON, owner mapping, stale fields, spare references and version2 roundtrip"};
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Library/save-envelope-acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
        }
        finally{end.Invoke(buffers,null);foreach(var agent in agents.Cast<MonoBehaviour>().ToArray())if(agent!=null)UnityEngine.Object.DestroyImmediate(agent.gameObject);UnityEngine.Object.DestroyImmediate(root);}
    }
}
