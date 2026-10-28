using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using EchoSphere.Runtime;
using UnityEngine;

public static class ParticleGeometryAcceptance
{
    public static object Main()
    {
        const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        var assembly=typeof(EchoSphereRuntime).Assembly;var wt=assembly.GetType("EchoSphere.Presentation.NativeWorldSurface");var ft=assembly.GetType("EchoSphere.Presentation.NativeWorldFeedback");
        var root=new GameObject("Particle geometry GPU acceptance");var world=root.AddComponent(wt);var feedback=Activator.CreateInstance(ft,true);
        object Call(object target,string name,params object[] input){var method=target.GetType().GetMethods(F).Single(m=>m.Name==name);var parameters=method.GetParameters();var args=new object[parameters.Length];for(int i=0;i<args.Length;i++)args[i]=i<input.Length?input[i]:parameters[i].DefaultValue;return method.Invoke(target,args);}
        Color32[] Render(bool reference,float dpr,string blend,int phase){
            ft.GetField("ReferenceParticleGeometryForValidation",F).SetValue(feedback,reference);Call(world,"SetViewport",300f,220f,dpr);Call(world,"Begin",Vector2.zero);Call(world,"ResetTransform");wt.GetProperty("Blend",F).SetValue(world,blend);wt.GetProperty("Shadow",F).SetValue(world,Color.clear);Call(feedback,"BeginFrame",world);
            for(int i=0;i<64;i++){double angle=i*.31+phase*.47;Call(feedback,"Particle",-100d+i%8*25,-70d+i/8*20,1.5+i%5,.2+(i+phase)%9*.1,Math.Cos(angle),Math.Sin(angle),new Color(.2f,.6f,.9f));}
            Call(world,"End");var texture=(RenderTexture)wt.GetProperty("UiTexture",F).GetValue(world);var previous=RenderTexture.active;var read=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);
            try{RenderTexture.active=texture;read.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);read.Apply();return read.GetPixels32();}
            finally{RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(read);}
        }
        var rows=new List<object>();int maximum=0;double maximumMean=0;
        try{
            Call(world,"Initialize");
            foreach(float dpr in new[]{1f,1.75f,2.75f})foreach(string blend in new[]{"source-over","lighter","screen"})for(int phase=0;phase<2;phase++){
                var before=Render(true,dpr,blend,phase);int oldDraws=(int)wt.GetProperty("GeometryDrawCalls",F).GetValue(world);
                var after=Render(false,dpr,blend,phase);int delta=0,visible=0,changed=0,large=0,alphaDelta=0;long total=0;
                for(int i=0;i<before.Length;i++){if(before[i].a>0)visible++;int difference=Math.Max(Math.Max(Math.Abs(before[i].r-after[i].r),Math.Abs(before[i].g-after[i].g)),Math.Max(Math.Abs(before[i].b-after[i].b),Math.Abs(before[i].a-after[i].a)));delta=Math.Max(delta,difference);total+=difference;if(difference>0)changed++;if(difference>3)large++;alphaDelta=Math.Max(alphaDelta,Math.Abs(before[i].a-after[i].a));}
                double mean=(double)total/before.Length;maximum=Math.Max(maximum,delta);maximumMean=Math.Max(maximumMean,mean);
                // Matrix scaling can move one MSAA edge sample; reject broader shape/color differences.
                if(visible==0||mean>.001||large>2)throw new Exception("Particle pixels differ: delta="+delta+" mean="+mean);
                rows.Add(new{dpr,blend,phase,delta,mean,changed,large,alphaDelta,oldDraws,newDraws=wt.GetProperty("GeometryDrawCalls",F).GetValue(world),instances=wt.GetProperty("InstancedPrimitiveCount",F).GetValue(world)});
            }
            if((int)ft.GetProperty("ParticleMeshBuilds",F).GetValue(feedback)!=1)throw new Exception("Particle mesh rebuilt");
            var result=new{result="PASS",cases=rows.Count,maxDelta=maximum,maxMean=maximumMean,meshBuilds=1,rows,scope="actual GPU old dynamic tessellation versus one retained particle mesh; density/blend/rotation, unchanged alpha/shape"};
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Library/retained-particle-geometry.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
        }
        finally{((IDisposable)feedback).Dispose();UnityEngine.Object.DestroyImmediate(root);}
    }
}
