using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using EchoSphere.Core;
using EchoSphere.Runtime;
using UnityEngine;

public static class RunDataRoundtripAcceptance
{
    public static object Main()
    {
        if(!UnityEditor.EditorApplication.isPlaying)throw new Exception("Play required for deferred entity destruction");
        const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        var root=new GameObject("Run data roundtrip acceptance");var runtime=root.AddComponent<EchoSphereRuntime>();runtime.enabled=false;var rt=runtime.GetType();
        var foreign=new GameObject("Foreign owner acceptance");var other=foreign.AddComponent<EchoSphereRuntime>();other.enabled=false;
        var player=new GameObject("Fixture player");player.transform.SetParent(root.transform);player.transform.position=new Vector3(5,5,0);
        void Set(string field,object value)=>rt.GetField(field,F).SetValue(runtime,value);
        object Get(object target,string field)=>target.GetType().GetField(field,F).GetValue(target);
        Set("_player",player.transform);var core=player.AddComponent<PlayerCoreController2D>();core.SetMaxHpForNewRun(121);core.TryTakeDamage(17);Set("_playerCore",core);
        Set("_camera",root.AddComponent<Camera>());Set("_mainMenu",false);Set("_paused",false);Set("_tutorialStep",-1);Set("_runTime",123.45f);
        var region=root.AddComponent<RegionFieldVisuals>();region.Initialize(runtime);Set("_regionFieldVisuals",region);
        var buffers=Get(runtime,"_runSaveBuffers");var end=buffers.GetType().GetMethod("End",F);var capture=rt.GetMethod("CaptureRunSnapshot",F);
        var agents=(IList)Get(runtime,"_simulationAgents");int checks=0;void Check(bool ok,string why){if(!ok)throw new Exception(why);checks++;}
        MonoBehaviour Add(Type type){var go=new GameObject("Roundtrip "+type.Name);go.transform.SetParent(root.transform);go.transform.position=new Vector3(-4,-4,0);return (MonoBehaviour)go.AddComponent(type);}
        try
        {
            rt.GetMethod("CreateSphere",F).Invoke(runtime,new object[]{SphereId.Standard,2,(Vector2?)new Vector2(-2,1)});
            rt.GetMethod("CreateSphere",F).Invoke(runtime,new object[]{SphereId.Orbital,3,(Vector2?)new Vector2(2,1)});
            var enemy=runtime.SpawnBossMinion(new Vector2(-3,-4),100,1,2,"wisp",Color.cyan);
            runtime.SpawnProjectile(new Vector2(-4,-4),Vector2.right,17,Color.cyan);
            var bullet=(BossProjectileAgent2D)Add(typeof(BossProjectileAgent2D));bullet.Initialize(runtime,player.transform,Vector2.up,8,13,"shooter",Color.cyan);bullet.SetSourceOwner(enemy);
            ((ExperienceOrbAgent)Add(typeof(ExperienceOrbAgent))).Initialize(runtime,player.transform,4,new Vector2(30,10),.4f);
            ((HealthPackAgent2D)Add(typeof(HealthPackAgent2D))).Initialize(runtime);
            ((ArtifactChestAgent2D)Add(typeof(ArtifactChestAgent2D))).Initialize(runtime);
            ((StellaChestAgent2D)Add(typeof(StellaChestAgent2D))).Initialize(runtime,new Vector2(-4,-4));
            ((RuneAgent2D)Add(typeof(RuneAgent2D))).Initialize(runtime,"phase",new Vector2(-4,-4),22);
            ((AbilityDroneAgent2D)Add(typeof(AbilityDroneAgent2D))).Initialize(runtime,SphereId.Chain,3,10,"minion_echo_drone","",0);
            runtime.SpawnSourceParticle(new Vector2(-3,-3),new Vector2(10,20),.6f,3,"#ff0000");
            var foreignEnemy=foreign.AddComponent<EnemyAgent2D>();foreignEnemy.Initialize(other,player.transform,100,0);
            var rng=(SeededRng)Get(runtime,"_rng");rng.RestoreState(781239u);uint state=rng.State;
            var saved=capture.Invoke(runtime,null);string json=JsonUtility.ToJson(saved);end.Invoke(buffers,null);
            var detached=JsonUtility.FromJson(json,saved.GetType());
            rt.GetMethod("RestoreRunSnapshot",F).Invoke(runtime,new[]{detached});
            Check(foreignEnemy!=null&&foreignEnemy.gameObject.activeSelf,"restore touched foreign runtime entities");
            Check(rng.State==state,"restore changed RNG state");Check(core.CurrentHp==104&&core.MaxHp==121,"player data roundtrip");
            var after=capture.Invoke(runtime,null);end.Invoke(buffers,null);
            var beforeEntities=(IList)Get(detached,"entities");var afterEntities=(IList)Get(after,"entities");
            Check(beforeEntities.Count==12&&afterEntities.Count==beforeEntities.Count,"full entity count");
            for(int i=0;i<beforeEntities.Count;i++){
                var a=beforeEntities[i];var b=afterEntities[i];
                foreach(string field in new[]{"kind","name","variant","position","scale","color","hasProjectileOwner","projectileOwnerIndex"})Check(field=="kind"||field=="name"||field=="variant"?((string)Get(a,field)??"")==((string)Get(b,field)??""):Equals(Get(a,field),Get(b,field)),"entity envelope "+i+" "+field);
                Check(JsonUtility.ToJson(Get(a,"state"))==JsonUtility.ToJson(Get(b,"state")),"entity state "+Get(a,"kind"));
            }
            Check(JsonUtility.ToJson(Get(detached,"regionPois"))==JsonUtility.ToJson(Get(after,"regionPois")),"POI restore");
            Check((bool)Get(runtime,"_userPaused")&&(bool)Get(runtime,"_paused"),"source restore pause policy");
            var result=new {result="PASS",checks,scope="actual capture -> version2 JSON -> restore -> capture, 12 entities/all 11 kinds, RNG/player/POI, foreign-owner isolation; no PlayerPrefs writes"};
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Library/run-data-roundtrip.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
        }
        finally{end.Invoke(buffers,null);foreach(var agent in agents.Cast<MonoBehaviour>().ToArray())if(agent!=null)UnityEngine.Object.DestroyImmediate(agent.gameObject);UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(foreign);}
    }
}
