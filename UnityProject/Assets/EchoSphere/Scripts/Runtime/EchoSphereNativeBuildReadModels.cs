using System;
using System.Collections.Generic;
using System.Linq;
using EchoSphere.Core;
using EchoSphere.Presentation;
namespace EchoSphere.Runtime
{
 public sealed partial class EchoSphereRuntime
 {
  internal readonly struct NativeProtocolState
  {
   public readonly ArtifactProtocolDefinition Definition;public readonly bool Discovered,Active;
   public NativeProtocolState(ArtifactProtocolDefinition definition,bool discovered,bool active){Definition=definition;Discovered=discovered;Active=active;}
  }
  internal sealed class NativeDiagnosticRow {public string Id;public List<string> Signals;}
  private int NativeLiveAbilityLevel(string id)=>Enum.TryParse<RunUpgradeKind>(id,true,out var kind)?GetAbilityLevel(kind):0;
  private int NativeLiveSphereLevel(string id)=>Enum.TryParse<SphereId>(id,true,out var type)&&_sphereLevels.TryGetValue(type,out int level)?level:0;
  private string NativeLiveSphereBranch(string id)=>Enum.TryParse<SphereId>(id,true,out var type)?GetSphereBranch(type):null;
  private IReadOnlyList<NativeDiagnosticRow> NativeLiveBuildDiagnostics(bool ru)
  {
   var catalog=NativeRunCatalog.Instance;var rows=new List<NativeDiagnosticRow>();
   List<string> SphereNames(string[] types)=>types.Where(id=>NativeLiveSphereLevel(id)>0).Select(id=>catalog.GetSphere(id).Name(ru)+" "+NativePresentationMath.NumberText(NativeLiveSphereLevel(id))).ToList();
   void Add(string id,List<string> signals)=>rows.Add(new NativeDiagnosticRow {Id=id,Signals=signals});
   var offense=SphereNames(new[]{"standard","sniper","shotgun","chain","pulse","void"});if(NativeLiveAbilityLevel("damage")>0)offense.Add(catalog.GetAbility("damage").Name(ru)+" "+NativePresentationMath.NumberText(NativeLiveAbilityLevel("damage")));Add("offense",offense);
   var control=SphereNames(new[]{"aura","gravity","prism"});if(GetGlobalSphereModifierLevel("freeze")>0)control.Add("Freeze");if(GetGlobalSphereModifierLevel("gravitic")>0)control.Add("Gravity");Add("control",control);
   var survival=new List<string>();string[] ids={"vitality","shield","dodge","vampire","movespeed"},russian={"Живучесть","Барьер","Уклонение","Вампиризм","Мобильность"},english={"Vitality","Barrier","Dodge","Vampirism","Mobility"};for(int i=0;i<ids.Length;i++)if(NativeLiveAbilityLevel(ids[i])>0)survival.Add(ru?russian[i]:english[i]);Add("survival",survival);
   string geometry=!string.IsNullOrEmpty(_resonanceGeometryKey)?_resonanceGeometryKey:"none";string geometrySignal=(ru?"Геометрия: ":"Geometry: ")+geometry;
   int typeCount=_spheres.Where(sphere=>sphere!=null&&sphere.gameObject.activeInHierarchy).Select(sphere=>sphere.Type.ToString().ToLowerInvariant()).Distinct(StringComparer.Ordinal).Count();
   var network=NativeMobileNetwork();
   Add("network",new List<string>{(ru?"Типов сфер: ":"Sphere types: ")+typeCount,(ru?"Связей: ":"Links: ")+network.Links.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),geometrySignal});
   Add("resonance",new List<string>{(ru?"Резонанс: ":"Resonance: ")+NativePresentationMath.NumberText(Math.Floor(_resonanceCharge))+"/100",geometrySignal});
   var completed=NativeLiveCompletedSets();var synergy=catalog.Sets.Where(set=>completed.Contains(set.Id)).Select(set=>set.Name(ru)).ToList();synergy.AddRange(NativeLiveProtocols().Where(row=>row.Active).Select(row=>ru?row.Definition.NameRussian:row.Definition.NameEnglish));
   int count=catalog.SphereAbilityLinks.Count(link=>NativeLiveSphereLevel(link.Sphere)>=7&&NativeLiveSphereBranch(link.Sphere)==link.Branch&&NativeLiveAbilityLevel(link.Ability)>=7&&NativeLiveAbilityChoice((RunUpgradeKind)Enum.Parse(typeof(RunUpgradeKind),link.Ability,true),catalog.GetAbility(link.Ability),7)?.Id==link.AbilityFinal);
   if(count>0)synergy.Add((ru?"Активных синергий сфер: ":"Active Sphere synergies: ")+count);Add("synergy",synergy);return rows;
  }
  private HashSet<string> NativeLiveCompletedSets()
  {
   var active=new HashSet<string>(NativeOwnedArtifactSynergies(_ownedArtifacts).Select(row=>row.Id),StringComparer.Ordinal);
   return new HashSet<string>(NativeRunCatalog.Instance.Sets.Where(set=>set.SynergyIds.All(active.Contains)).Select(set=>set.Id),StringComparer.Ordinal);
  }
  private IReadOnlyList<NativeProtocolState> NativeLiveProtocols()
  {
   var completed=NativeLiveCompletedSets();var spheres=_spheres.Where(s=>s!=null&&s.gameObject.activeInHierarchy).ToList();
   var types=new HashSet<string>(spheres.Select(s=>s.Type.ToString().ToLowerInvariant()),StringComparer.Ordinal);
   string geometry=!string.IsNullOrEmpty(_resonanceGeometryKey)?_resonanceGeometryKey:"none";bool geometryActive=geometry=="ring"||geometry=="lattice"||geometry=="fractal";
   bool priority=_enemies.Any(enemy=>enemy!=null&&enemy.IsAlive&&(enemy.IsElite||enemy.IsBoss));
   bool status=types.Contains("prism")&&_sphereMods.Values.Any(level=>level>0);
   double Distance(SphereAttackAgent a,SphereAttackAgent b){var pa=NativeSourcePixels(a.transform.position);var pb=NativeSourcePixels(b.transform.position);double dx=pa.x-pb.x,dy=pa.y-pb.y;return Math.Sqrt(dx*dx+dy*dy);}
   bool triangle=false;for(int i=0;i<spheres.Count&&!triangle;i++)for(int j=i+1;j<spheres.Count&&!triangle;j++)for(int k=j+1;k<spheres.Count;k++)if(!ReferenceEquals(spheres[i],spheres[j])&&!ReferenceEquals(spheres[i],spheres[k])&&!ReferenceEquals(spheres[j],spheres[k])&&Distance(spheres[i],spheres[j])<=300&&Distance(spheres[i],spheres[k])<=300&&Distance(spheres[j],spheres[k])<=300){triangle=true;break;}
   var rows=new List<NativeProtocolState>();foreach(var definition in ArtifactProtocolRules.Protocols){bool condition=false;
    switch(definition.Id){case "triangle_resonance":condition=triangle;break;case "sphere_relay":condition=types.Count>=2;break;case "critical_echo":condition=_combo>=5;break;case "geometry_circuit":condition=geometryActive;break;case "void_horizon_protocol":condition=types.Contains("void")&&priority;break;case "temporal_fold_protocol":condition=_timeStopTimer>0||_teleportDamageBuffTimer>0;break;case "status_circuit_protocol":condition=status;break;case "hunter_doctrine_protocol":condition=types.Contains("sniper")&&priority;break;case "pulse_engineering_protocol":condition=types.Contains("pulse")&&_enemies.Count(enemy=>enemy!=null&&enemy.IsAlive)>=4;break;case "prism_dominion_protocol":condition=types.Contains("prism")&&(types.Contains("orbital")||types.Contains("void"));break;case "core_forge_protocol":condition=types.Count>=3;break;case "network_legacy_protocol":condition=types.Count>=3&&geometryActive;break;}
    bool discovered=completed.Contains(definition.SetId);rows.Add(new NativeProtocolState(definition,discovered,discovered&&condition));
   }return rows;
  }
#if UNITY_EDITOR
  internal static IReadOnlyList<NativeDiagnosticRow> NativeBuildDiagnostics(V scene,bool ru)
  {
   var catalog=NativeRunCatalog.Instance;var player=scene["player"];var abilities=player["abilities"];var rows=new List<NativeDiagnosticRow>();
   List<string> SphereNames(string[] types)=>types.Where(id=>NativeSphereLevel(scene,id)>0).Select(id=>catalog.GetSphere(id).Name(ru)+" "+NativePresentationMath.NumberText(NativeSphereLevel(scene,id))).ToList();
   void Add(string id,List<string> signals)=>rows.Add(new NativeDiagnosticRow {Id=id,Signals=signals});
   var offense=SphereNames(new[]{"standard","sniper","shotgun","chain","pulse","void"});if(abilities["damage"].Number>0)offense.Add(catalog.GetAbility("damage").Name(ru)+" "+abilities["damage"].Text);Add("offense",offense);
   var control=SphereNames(new[]{"aura","gravity","prism"});if(player["sphereMods"]["freeze"].Number>0)control.Add("Freeze");if(player["sphereMods"]["gravitic"].Number>0)control.Add("Gravity");Add("control",control);
   var survival=new List<string>();string[] ids={"vitality","shield","dodge","vampire","movespeed"},russian={"Живучесть","Барьер","Уклонение","Вампиризм","Мобильность"},english={"Vitality","Barrier","Dodge","Vampirism","Mobility"};for(int i=0;i<ids.Length;i++)if(abilities[ids[i]].Number>0)survival.Add(ru?russian[i]:english[i]);Add("survival",survival);
   string geometry=player["resonanceGeometryKey"].Truth?player["resonanceGeometryKey"].Text:"none";string geometrySignal=(ru?"Геометрия: ":"Geometry: ")+geometry;
   int typeCount=scene["spheres"].Items.Where(sphere=>sphere["alive"].Truth).Select(sphere=>sphere["type"].Text).Distinct(StringComparer.Ordinal).Count();
   var network=NativeAnalyzeSphereNetwork(scene["spheres"]);
   Add("network",new List<string>{(ru?"Типов сфер: ":"Sphere types: ")+typeCount,(ru?"Связей: ":"Links: ")+network.Links.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),geometrySignal});
   Add("resonance",new List<string>{(ru?"Резонанс: ":"Resonance: ")+NativePresentationMath.NumberText(Math.Floor(player["resonanceCharge"].Number))+"/100",geometrySignal});
   var completed=NativeCompletedSets(scene);var synergy=catalog.Sets.Where(set=>completed.Contains(set.Id)).Select(set=>set.Name(ru)).ToList();synergy.AddRange(NativeProtocols(scene).Where(row=>row.Active).Select(row=>ru?row.Definition.NameRussian:row.Definition.NameEnglish));
   int count=catalog.SphereAbilityLinks.Count(link=>NativeSphereLevel(scene,link.Sphere)>=7&&player["sphereBranches"][link.Sphere].Text==link.Branch&&abilities[link.Ability].Number>=7&&NativeAbilityChoice(scene,link.Ability,7)?.Id==link.AbilityFinal);
   if(count>0)synergy.Add((ru?"Активных синергий сфер: ":"Active Sphere synergies: ")+count);Add("synergy",synergy);return rows;
  }
  internal static HashSet<string> NativeCompletedSets(V scene)
  {
   var active=new HashSet<string>(NativeArtifactSynergies(scene).Select(row=>row.Id),StringComparer.Ordinal);
   return new HashSet<string>(NativeRunCatalog.Instance.Sets.Where(set=>set.SynergyIds.All(active.Contains)).Select(set=>set.Id),StringComparer.Ordinal);
  }
  internal static IReadOnlyList<NativeProtocolState> NativeProtocols(V scene)
  {
   var completed=NativeCompletedSets(scene);var spheres=scene["spheres"].Items.Where(s=>!(s["alive"].Raw is bool alive&&!alive)).ToList();
   var types=new HashSet<string>(spheres.Select(s=>s["type"].Text),StringComparer.Ordinal);var player=scene["player"];
   string geometry=player["resonanceGeometryKey"].Truth?player["resonanceGeometryKey"].Text:"none";bool geometryActive=geometry=="ring"||geometry=="lattice"||geometry=="fractal";
   bool priority=scene["enemies"].Items.Any(enemy=>enemy["hp"].Number>0&&(enemy["isElite"].Truth||enemy["isBoss"].Truth));
   bool status=types.Contains("prism")&&player["sphereMods"].Raw is Dictionary<string,V> sphereMods&&sphereMods.Values.Any(level=>VisualJs.Number(level).Number>0);
   double Distance(V a,V b){double dx=a["pos"]["x"].Number-b["pos"]["x"].Number,dy=a["pos"]["y"].Number-b["pos"]["y"].Number;return Math.Sqrt(dx*dx+dy*dy);}
   bool triangle=false;for(int i=0;i<spheres.Count&&!triangle;i++)for(int j=i+1;j<spheres.Count&&!triangle;j++)for(int k=j+1;k<spheres.Count;k++)if(!ReferenceEquals(spheres[i].Raw,spheres[j].Raw)&&!ReferenceEquals(spheres[i].Raw,spheres[k].Raw)&&!ReferenceEquals(spheres[j].Raw,spheres[k].Raw)&&Distance(spheres[i],spheres[j])<=300&&Distance(spheres[i],spheres[k])<=300&&Distance(spheres[j],spheres[k])<=300){triangle=true;break;}
   var rows=new List<NativeProtocolState>();foreach(var definition in ArtifactProtocolRules.Protocols){bool condition=false;
    switch(definition.Id){case "triangle_resonance":condition=triangle;break;case "sphere_relay":condition=types.Count>=2;break;case "critical_echo":condition=player["combo"].Number>=5;break;case "geometry_circuit":condition=geometryActive;break;case "void_horizon_protocol":condition=types.Contains("void")&&priority;break;case "temporal_fold_protocol":condition=player["timestopTimer"].Number>0||player["teleportDamageBuffTimer"].Number>0;break;case "status_circuit_protocol":condition=status;break;case "hunter_doctrine_protocol":condition=types.Contains("sniper")&&priority;break;case "pulse_engineering_protocol":condition=types.Contains("pulse")&&scene["enemies"].Items.Count(enemy=>enemy["hp"].Number>0)>=4;break;case "prism_dominion_protocol":condition=types.Contains("prism")&&(types.Contains("orbital")||types.Contains("void"));break;case "core_forge_protocol":condition=types.Count>=3;break;case "network_legacy_protocol":condition=types.Count>=3&&geometryActive;break;}
    bool discovered=completed.Contains(definition.SetId);rows.Add(new NativeProtocolState(definition,discovered,discovered&&condition));
   }return rows;
  }
#endif
 }
}
