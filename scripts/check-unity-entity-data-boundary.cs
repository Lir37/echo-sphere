using System;
using System.Linq;
using System.Reflection;
using EchoSphere.Core;
using EchoSphere.Runtime;
using UnityEngine;

public static class NativeEntityBoundaryAcceptance
{
    private sealed class Baseline {public SnapshotValue bullet=null,stella=null;}
    public static object Main()
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        var root=new GameObject("Entity data boundary acceptance");var runtime=root.AddComponent<EchoSphereRuntime>();runtime.enabled=false;
        var rt=runtime.GetType();var assembly=rt.Assembly;var colorType=assembly.GetType("EchoSphere.Runtime.IRunSaveColor");
        rt.GetField("_mainMenu",Flags).SetValue(runtime,false);rt.GetField("_paused",Flags).SetValue(runtime,false);
        var player=new GameObject("Fixture player");player.transform.SetParent(root.transform);player.transform.position=new Vector3(5,5,0);
        rt.GetField("_player",Flags).SetValue(runtime,player.transform);
        int checks=0;void Check(bool ok,string message){if(!ok)throw new Exception(message);checks++;}
        var select=(Func<FieldInfo,bool>)Delegate.CreateDelegate(typeof(Func<FieldInfo,bool>),rt.GetMethod("EntitySnapshotField",BindingFlags.Static|BindingFlags.NonPublic));
        var agents=(System.Collections.IList)rt.GetField("_simulationAgents",Flags).GetValue(runtime);
        Color ColorOf(object agent)=>(Color)colorType.GetProperty("SavedColor").GetValue(agent);
        MonoBehaviour Create(Type type){var go=new GameObject("Fixture "+type.Name);go.transform.SetParent(root.transform);go.transform.position=new Vector3(-5,-5,0);return (MonoBehaviour)go.AddComponent(type);}
        var types=new[]{typeof(ExperienceOrbAgent),typeof(HealthPackAgent2D),typeof(ArtifactChestAgent2D),typeof(AbilityDroneAgent2D),typeof(RuneAgent2D),typeof(SourceParticle2D)};
        var expected=new[]{new Color(99f/255f,230f/255f,1),new Color(.36f,.9f,.42f),new Color(1,.76f,.3f),new Color(.35f,.83f,1,.95f),RuneAgent2D.GetRuneColor("gravity"),Color.cyan};
        try
        {
            for(int i=0;i<types.Length;i++)
            {
                var agent=Create(types[i]);
                if(agent is ExperienceOrbAgent xp)xp.Initialize(runtime,player.transform,5);
                else if(agent is HealthPackAgent2D health)health.Initialize(runtime);
                else if(agent is ArtifactChestAgent2D chest)chest.Initialize(runtime);
                else if(agent is AbilityDroneAgent2D drone)drone.Initialize(runtime,SphereId.Standard,1,10,"","",0);
                else if(agent is RuneAgent2D rune)rune.Initialize(runtime,"gravity",agent.transform.position,1.3f);
                else if(agent is SourceParticle2D particle)particle.Initialize(runtime,Vector2.zero,2,Color.cyan);
                Check(ColorOf(agent)==expected[i],"legacy birth envelope color "+types[i].Name);
                Check(agent.GetComponentsInChildren<Renderer>(true).Length==0,"hidden renderer at birth");
                var state=RunSnapshotCodec.Capture(agent,select);
                Check(!state.nodes.Any(n=>n.name=="_savedColor"),"color duplicated into state schema");
                var envelopeType=rt.GetNestedType("SavedEntity",BindingFlags.NonPublic);var envelope=Activator.CreateInstance(envelopeType,true);
                void Set(string name,object value)=>envelopeType.GetField(name,Flags).SetValue(envelope,value);
                var customColor=new Color(.13f,.27f,.31f,.47f);
                Set("kind",types[i].Name);Set("name","Restored boundary fixture");Set("position",agent.transform.position);Set("scale",agent.transform.localScale);Set("state",state);Set("color",customColor);
                rt.GetMethod("RestoreSavedEntity",Flags).Invoke(runtime,new[]{envelope});
                var restored=(MonoBehaviour)agents[agents.Count-1];
                Check(restored.GetType()==types[i],"wrong restored entity");
                Check(ColorOf(restored)==customColor,"legacy color envelope lost");
                Check(restored.GetComponentsInChildren<Renderer>(true).Length==0,"hidden renderer after restore");
                if(restored is RuneAgent2D restoredRune){restoredRune.SimulationTick(.016f);var tint=RuneAgent2D.GetRuneColor("gravity");tint.a=Mathf.Clamp01(Mathf.Min(1f,(float)typeof(RuneAgent2D).GetField("_life",Flags).GetValue(restoredRune)/2));Check(ColorOf(restored)==tint,"rune fade after legacy restore");}
            }
            var baselinePath=System.IO.Path.Combine(Application.dataPath,"../Library/retired-mesh-baseline.json");
            bool legacyBaseline=System.IO.File.Exists(baselinePath);
            var bullet=(BossProjectileAgent2D)Create(typeof(BossProjectileAgent2D));bullet.Initialize(runtime,player.transform,new Vector2(.5f,-.2f),8,13,"architect",Color.cyan);
            var stella=(StellaChestAgent2D)Create(typeof(StellaChestAgent2D));stella.Initialize(runtime,new Vector2(-5,-5));
            if(legacyBaseline){var baseline=Newtonsoft.Json.JsonConvert.DeserializeObject<Baseline>(System.IO.File.ReadAllText(baselinePath));RunSnapshotCodec.Restore(baseline.bullet,typeof(BossProjectileAgent2D),bullet);RunSnapshotCodec.Restore(baseline.stella,typeof(StellaChestAgent2D),stella);
                foreach(var field in typeof(BossProjectileAgent2D).GetFields(Flags).Where(f=>!RunSnapshotCodec.IsEngineObject(f.FieldType)))
                    if(new[]{"_damage","_hitRadius","_time"}.Contains(field.Name))Check((float)field.GetValue(bullet)==RunSnapshotCodec.Read(baseline.bullet,field.Name,0f),"legacy scalar "+field.Name);
                Check((Vector2)typeof(BossProjectileAgent2D).GetField("_velocity",Flags).GetValue(bullet)==RunSnapshotCodec.Read(baseline.bullet,"_velocity",Vector2.zero),"legacy velocity");
                Check((float)typeof(StellaChestAgent2D).GetField("_time",Flags).GetValue(stella)==RunSnapshotCodec.Read(baseline.stella,"_time",0f),"legacy Stella time");
            }
            var before=bullet.transform.position;bullet.SimulationTick(.016f);Check(bullet.transform.position.Equals(before+new Vector3(.5f,-.2f,0)*.016f),"bullet movement changed");
            stella.SimulationTick(.016f);
            foreach(var agent in new MonoBehaviour[]{bullet,stella}){Check(agent.GetComponentsInChildren<Renderer>(true).Length==0,"retired mesh/label renderer");Check(agent.GetComponent<MeshFilter>()==null,"retired mesh filter");Check(!RunSnapshotCodec.Capture(agent,select).nodes.Any(n=>n.name=="_vertices"||n.name=="_triangles"||n.name=="_colors"||n.name=="_shape"),"mesh arrays still captured");}
            var result=new {result="PASS",checks,legacyBaseline,scope="owned Editor fixture; legacy state/color envelope restore; renderer-free birth/restore/tick; no PlayerPrefs writes"};
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Library/entity-data-boundary.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
        }
        finally{foreach(var agent in agents.Cast<MonoBehaviour>().ToArray())if(agent!=null)UnityEngine.Object.DestroyImmediate(agent.gameObject);UnityEngine.Object.DestroyImmediate(root);}
    }
}
