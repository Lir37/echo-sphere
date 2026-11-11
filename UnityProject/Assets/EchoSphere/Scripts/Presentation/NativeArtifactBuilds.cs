using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;
using UnityEngine.UIElements;
namespace EchoSphere.Presentation {
 internal static class NativeArtifactBuilds {
  internal sealed class Build {
   internal string Id,NameRu,NameEn,DescriptionRu,DescriptionEn;internal bool Combination;internal string[] Requires;
   internal string Name(bool ru)=>ru?NameRu:NameEn;
   internal string Description(bool ru)=>ru?DescriptionRu:DescriptionEn;
  }
  private static List<Build> _all;
  private static readonly Dictionary<string,List<Build>> Membership=new Dictionary<string,List<Build>>(StringComparer.Ordinal);
  internal static IReadOnlyList<Build> All {
   get{if(_all!=null)return _all;var rows=new List<Build>();var catalog=NativeRunCatalog.Instance;
    foreach(var s in catalog.Sets)rows.Add(new Build{Id="artifact-build:set:"+s.Id,NameRu=s.NameRu,NameEn=s.NameEn,DescriptionRu=s.DescriptionRu,DescriptionEn=s.DescriptionEn,Requires=s.ArtifactIds.ToArray()});
    foreach(var s in catalog.Synergies)rows.Add(new Build{Id="artifact-build:combination:"+s.Id,NameRu=s.NameRu,NameEn=s.NameEn,DescriptionRu=s.Description(true),DescriptionEn=s.Description(false),Requires=s.Requires.ToArray(),Combination=true});
    foreach(var s in CampaignArtifactSets.All)rows.Add(new Build{Id="artifact-build:campaign:"+s.Id,NameRu=s.Ru,NameEn=s.En,DescriptionRu=s.DescriptionRu,DescriptionEn=s.DescriptionEn,Requires=s.Requires});
    _all=rows;foreach(var b in rows)foreach(var artifact in b.Requires){if(!Membership.TryGetValue(artifact,out var list))Membership[artifact]=list=new List<Build>();if(!list.Contains(b))list.Add(b);}return rows;
   }
  }
  internal static IReadOnlyList<Build> ForArtifact(string id){_ = All;return Membership.TryGetValue(id,out var list)?list:Array.Empty<Build>();}
  internal static bool Draw(Painter2D p,string id,Vector2 origin,float scale){
   int index=-1;for(int i=0;i<All.Count;i++)if(All[i].Id==id){index=i;break;}if(index<0)return false;
   var color=Color.HSVToRGB(Mathf.Repeat(.54f+index*.618034f,1),.55f,.98f);
   p.strokeColor=color;p.fillColor=new Color(.025f,.045f,.075f,1);p.lineWidth=1.4f*scale;
   Vector2 V(float x,float y)=>origin+new Vector2(x,y)*scale;
   int sides=5+index%4;p.BeginPath();for(int k=0;k<sides;k++){float a=-Mathf.PI/2+k*2*Mathf.PI/sides;var v=V(Mathf.Cos(a)*13,Mathf.Sin(a)*13);if(k==0)p.MoveTo(v);else p.LineTo(v);}p.ClosePath();p.Fill();p.Stroke();
   string key=id.ToLowerInvariant();string motif=key.Contains("blood")||key.Contains("living")?"ability:vitality":key.Contains("frozen")||key.Contains("optic")?"ability:crit":key.Contains("wave")?"ability:blast":key.Contains("kinetic")||key.Contains("tempo")?"ability:movespeed":key.Contains("chron")||key.Contains("time")?"ability:timestop":key.Contains("bastion")||key.Contains("fort")?"ability:shield":key.Contains("void")?"ability:darkritual":"ability:resonance";
   NativeArchiveIllustrations.Draw(p,motif,origin,scale*.63f);
   // The frame is the build's shared emblem; its six open/closed slots distinguish every build.
   p.strokeColor=color;p.lineWidth=1.7f*scale;
   for(int bit=0;bit<6;bit++){float a=(-75+bit*60)*Mathf.Deg2Rad;var v=V(Mathf.Cos(a)*12,Mathf.Sin(a)*12);p.BeginPath();p.Arc(v,(((index>>bit)&1)==0?1:1.8f)*scale,0,360);p.Stroke();}
   return true;
  }
 }
}