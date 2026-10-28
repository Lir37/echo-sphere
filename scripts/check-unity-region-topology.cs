using System;
using System.Linq;
using System.Reflection;
using EchoSphere.Runtime;
using UnityEngine;

public static class RegionTopologyAcceptance
{
    public static object Main()
    {
        const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        var type=typeof(EchoSphereRuntime).Assembly.GetType("EchoSphere.Presentation.NativeWorldRegion").GetNestedType("Geometry",BindingFlags.NonPublic);
        var geometry=Activator.CreateInstance(type,true);var mesh=(Mesh)type.GetField("Mesh",F).GetValue(geometry);
        void Call(string name,params object[] args){var method=type.GetMethod(name,F);var parameters=method.GetParameters();var actual=new object[parameters.Length];for(int i=0;i<actual.Length;i++)actual[i]=i<args.Length?args[i]:parameters[i].DefaultValue;method.Invoke(geometry,actual);}
        int Count()=>(int)type.GetProperty("TopologyUploads",F).GetValue(geometry);
        int checks=0;void Check(bool ok,string why){if(!ok)throw new Exception(why);checks++;}
        try
        {
            for(int frame=0;frame<120;frame++){
                Call("Clear");Call("Circle",new Vector2(frame*.03f,-4),20f,2f,Color.cyan,frame*.01f,1.3f,true);
                Call("Triangle",new Vector2(frame*.01f,0),new Vector2(1,2),new Vector2(3,0),Color.white);Call("Upload");
                int uploads=Count();var vertices=mesh.vertices;var colors=mesh.colors;var uv=mesh.uv;var indices=mesh.triangles;
                Call("Upload");Check(Count()==uploads,"unchanged topology rebuilt");
                mesh.Clear();Call("Upload");Check(Count()==uploads+1,"external topology reset not detected");
                Check(vertices.SequenceEqual(mesh.vertices)&&colors.SequenceEqual(mesh.colors)&&uv.SequenceEqual(mesh.uv)&&indices.SequenceEqual(mesh.triangles),"retained geometry differs from full upload");
            }
            Call("Clear");Call("Upload");Check(mesh.vertexCount==0&&mesh.triangles.Length==0,"empty geometry retained stale indices");
            var result=new {result="PASS",checks,scope="120 changing region geometries versus forced full mesh rebuild; exact vertices/colors/UV/indices, no repeated topology upload"};
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Library/region-topology-acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
        }
        finally{((IDisposable)geometry).Dispose();}
    }
}
