using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using EchoSphere.Presentation;
using Newtonsoft.Json;

public static class PlayerCopyCheck
{
    sealed class Row { public string Context, Language, Text; }
    static List<Row> rows=new List<Row>();
    static HashSet<object> visited;
    static void Add(string context,bool ru,string text){if(!string.IsNullOrWhiteSpace(text))rows.Add(new Row{Context=context,Language=ru?"ru":"en",Text=text});}
    static void Walk(object value,string path)
    {
        if(value==null||value is string||value.GetType().IsPrimitive||value.GetType().IsEnum)return;
        if(!visited.Add(value))return;
        if(value is NativeRunCatalog.Text t){foreach(bool ru in new[]{true,false}){Add(path+"/name",ru,t.Name(ru));Add(path+"/description",ru,t.Description(ru));}}
        if(value is NativeArchiveCatalog.Entry e)foreach(bool ru in new[]{true,false}){Add(path+"/name",ru,e.Name(ru));Add(path+"/description",ru,e.Description(ru));Add(path+"/details",ru,e.Details(ru));}
        if(value is IEnumerable list){int i=0;foreach(var item in list)Walk(item,path+"/"+i++);return;}
        foreach(var f in value.GetType().GetFields(BindingFlags.Instance|BindingFlags.Public)){
            object child=f.GetValue(value);if(child is string text){bool ru=f.Name.EndsWith("Ru")||f.Name.EndsWith("Russian")||f.Name=="Ru",en=f.Name.EndsWith("En")||f.Name.EndsWith("English")||f.Name=="En";
                // Text/Entry getters handle contextual overrides; do not reintroduce raw descriptions.
                if(!(value is NativeRunCatalog.Text)&&!(value is NativeArchiveCatalog.Entry)&&(ru||en))Add(path+"/"+f.Name,ru,NativePlayerCopy.Text(text,ru));
            }else Walk(child,path+"/"+f.Name);
        }
    }
    public static object Main()
    {
        UnityEditor.AssetDatabase.Refresh();typeof(NativePlayerCopy).GetField("_ru",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,null);
        rows.Clear();visited=new HashSet<object>();
        var run=NativeRunCatalog.Instance;var archive=Resources.Load<NativeArchiveCatalog>("EchoSphere/NativeArchiveCatalog");var local=NativeLocalizationCatalog.Instance;
        string before=JsonUtility.ToJson(run)+JsonUtility.ToJson(archive)+JsonUtility.ToJson(local);
        Walk(run,"run");Walk(archive,"archive");Walk(EchoSphere.Core.CharacterRules.All,"characters");Walk(EchoSphere.Core.ArtifactRewardRules.Catalog(true),"artifacts");Walk(EchoSphere.Core.CampaignArtifactSets.All,"sets");Walk(EchoSphere.Core.ArtifactProtocolRules.Protocols,"protocols");
        foreach(var row in local.Translations)foreach(bool ru in new[]{true,false})Add("translation",ru,local.Translate(row.Key,ru));
        if(before!=JsonUtility.ToJson(run)+JsonUtility.ToJson(archive)+JsonUtility.ToJson(local))throw new Exception("Copy changed imported source assets");
        foreach(var a in run.Abilities)foreach(bool ru in new[]{true,false})if(string.IsNullOrWhiteSpace(a.Description(ru))||a.Description(ru).Contains("[object Object]"))throw new Exception("Ability description missing "+a.Id);
        foreach(var row in rows)if(NativePlayerCopy.Text(row.Text,row.Language=="ru")!=row.Text)throw new Exception("Copy is not idempotent: "+row.Context);
        Directory.CreateDirectory("Library/player-copy");File.WriteAllText("Library/player-copy/resolved.json",JsonConvert.SerializeObject(rows,Formatting.Indented));
        return new{result="PASS",resolvedTexts=rows.Count,abilities=run.Abilities.Count,sourceAssets="unchanged",output="Library/player-copy/resolved.json"};
    }
}
