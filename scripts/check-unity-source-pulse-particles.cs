using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using EchoSphere.Core;
using EchoSphere.Runtime;
using UnityEngine;

public static class SourcePulseAcceptance
{
    public static object Main()
    {
        const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
        var root=new GameObject("Source pulse acceptance");var runtime=root.AddComponent<EchoSphereRuntime>();runtime.enabled=false;var rt=runtime.GetType();
        var rng=(SeededRng)rt.GetField("_rng",F).GetValue(runtime);rng.RestoreState(77821);var reference=new SeededRng(77821);
        int checks=0;void Check(bool ok,string why){if(!ok)throw new Exception(why);checks++;}
        try
        {
            rt.GetMethod("EmitSourceSpherePulse",F).Invoke(runtime,new object[]{null,new Vector2(2,-3),1f,1f,Color.cyan,false});
            var agents=(IList)rt.GetField("_simulationAgents",F).GetValue(runtime);var particles=agents.Cast<MonoBehaviour>().OfType<SourceParticle2D>().ToArray();
            Check(particles.Length==14,"pulse particle count");
            for(int i=0;i<14;i++){
                float a=reference.NextFloat()*Mathf.PI*2f;var expected=new Vector2(Mathf.Cos(a),-Mathf.Sin(a))*90f*SpherePlacementRules.SourcePixelsToWorld;
                Check(((Vector2)typeof(SourceParticle2D).GetField("_velocity",F).GetValue(particles[i])).Equals(expected),"seeded velocity/order");
                Check((float)typeof(SourceParticle2D).GetField("_life",F).GetValue(particles[i])==.35f&&(float)typeof(SourceParticle2D).GetField("_maxLife",F).GetValue(particles[i])==.35f,"source lifetime");
                Check((float)typeof(SourceParticle2D).GetField("_sourceSize",F).GetValue(particles[i])==3f,"source size");
                Check(particles[i].transform.position.Equals(new Vector3(2,-3,0)),"pulse center");
                Check(particles[i].GetComponentsInChildren<Renderer>(true).Length==0,"hidden renderer");
            }
            Check(rng.State==reference.State,"pulse must consume exactly14 RNG values");
            Check(rt.Assembly.GetType("EchoSphere.Runtime.ImpactVfx2D")==null&&rt.Assembly.GetType("EchoSphere.Runtime.PulseEffect2D")==null,"legacy scene previews loaded in normal Editor");
            var eg=new GameObject("Pulse fixture hit target");eg.transform.SetParent(root.transform);var enemy=eg.AddComponent<EnemyAgent2D>();enemy.Initialize(runtime,root.transform,100,0);
            int objects=Resources.FindObjectsOfTypeAll<GameObject>().Length;
            for(int hit=0;hit<50;hit++)enemy.ReceiveDamage(1,emitFeedback:false);
            Check(enemy.CurrentHp==50,"damage changed with scene preview isolated");
            Check((float)typeof(EnemyAgent2D).GetField("_sourceHitFlash",F).GetValue(enemy)==.15f,"native hit flash disabled");
            Check(Resources.FindObjectsOfTypeAll<GameObject>().Length==objects,"hidden impact objects created per hit");
            var result=new {result="PASS",checks,scope="source engineCombat.emitSpherePulse: 14 pooled model particles, seeded order/velocity, center, size/lifetime, no legacy renderer"};
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Library/source-pulse-acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
        }
        finally{var agents=(IList)rt.GetField("_simulationAgents",F).GetValue(runtime);foreach(var agent in agents.Cast<MonoBehaviour>().ToArray())if(agent!=null)UnityEngine.Object.DestroyImmediate(agent.gameObject);UnityEngine.Object.DestroyImmediate(root);}
    }
}
