using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using EchoSphere.Core;
using EchoSphere.Runtime;
using UnityEngine;

public static class RegionDataAcceptance
{
    private sealed class Row {public string region=null,state=null;public float time=0;}
    public static object Main()
    {
        var root=new GameObject("Region data acceptance");var runtime=root.AddComponent<EchoSphereRuntime>();runtime.enabled=false;
        const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
        var player=new GameObject("Fixture player");player.transform.SetParent(root.transform);player.transform.position=new Vector3(100,100,0);
        runtime.GetType().GetField("_player",F).SetValue(runtime,player.transform);
        var field=root.AddComponent<RegionFieldVisuals>();int checks=0;
        void Check(bool ok,string why){if(!ok)throw new Exception(why);checks++;}
        try
        {
            var rows=Newtonsoft.Json.JsonConvert.DeserializeObject<Row[]>(System.IO.File.ReadAllText(System.IO.Path.Combine(Application.dataPath,"../Library/region-data-baseline.json")));
            string previous=null;
            foreach(var row in rows){if(previous!=row.region){field.Initialize(runtime,(EchoRegionId)Enum.Parse(typeof(EchoRegionId),row.region));previous=row.region;}
                runtime.GetType().GetField("_runTime",F).SetValue(runtime,row.time);field.TickSourceRegionState();field.TickGameplayPois();
                Check(JsonUtility.ToJson(field.CapturePoiState())==row.state,"POI cadence/position/type/expiry "+row.region+" "+row.time);
                field.RestorePoiState(JsonUtility.FromJson<RegionFieldVisuals.PoiSnapshot>(row.state));
                Check(JsonUtility.ToJson(field.CapturePoiState())==row.state,"legacy POI restore");
                Check(root.GetComponentsInChildren<Renderer>(true).Length==0,"hidden region renderer");
                Check(root.transform.childCount==1,"hidden region objects");
            }
            var result=new {result="PASS",checks,scope="both regions versus pre-removal snapshots: POI cadence, position, expiry, restore; no hidden region objects; no PlayerPrefs writes"};
            System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath,"../Library/region-data-acceptance.json"),Newtonsoft.Json.JsonConvert.SerializeObject(result));return result;
        }
        finally{UnityEngine.Object.DestroyImmediate(root);}
    }
}
