using System;
using System.Collections.Generic;
using System.Linq;
using EchoSphere.Presentation;
namespace EchoSphere.Runtime
{
 public sealed partial class EchoSphereRuntime
 {
  internal static double NativeSphereLevel(V scene,string id){var level=scene["player"]["sphereProgression"][id];return level.Truth?level.Number:0;}
  internal static double NativeMaximumSphereCount(V scene){var ability=scene["player"]["abilities"]["maxspheres"];var shop=scene["shopUpgrades"]["spheres"];return Math.Min(5+(ability.Truth?ability.Number:0)+(shop.Truth?shop.Number:0),8);}
  internal static IReadOnlyList<NativeRunCatalog.Synergy> NativeArtifactSynergies(V scene,string added=null)
  {
   var owned=new HashSet<string>(scene["player"]["artifacts"].Items.Select(item=>item.Text),StringComparer.Ordinal);if(added!=null)owned.Add(added);
   return NativeRunCatalog.Instance.Synergies.Where(row=>row.Requires.All(owned.Contains)).ToList();
  }
  internal static NativeRunCatalog.Definition NativeAbilityChoice(V scene,string id,int stage)
  {
   var definition=NativeRunCatalog.Instance.GetAbility(id);if(definition==null||!definition.HasProgression)return null;
   string prefix="ability:"+id+":"+stage+":";string selected=null;
   foreach(var marker in scene["player"]["evolutions"].Items)if(marker.Text.StartsWith(prefix,StringComparison.Ordinal)){selected=marker.Text.Substring(prefix.Length);break;}
   if(selected==null)return null;
   IEnumerable<NativeRunCatalog.Definition> pool=definition.Branches;
   if(stage==7){var branch=NativeAbilityChoice(scene,id,4) as NativeRunCatalog.Choice;if(branch==null)return null;pool=branch.HasFinalPool?branch.Finals:definition.FallbackFinals;}
   return pool.FirstOrDefault(choice=>choice.Id==selected);
  }
  internal static string NativeAbilityName(V scene,string id,bool ru)
  {
   var definition=NativeRunCatalog.Instance.GetAbility(id);double level=scene["player"]["abilities"][id].Number;
   var choice=level>=7?NativeAbilityChoice(scene,id,7):null;if(choice==null&&level>=4)choice=NativeAbilityChoice(scene,id,4);
   return definition.Name(ru)+(choice==null?"":" · "+choice.Name(ru));
  }
  internal static string NativeAbilityDescription(V scene,string id,bool ru)
  {
   var definition=NativeRunCatalog.Instance.GetAbility(id);double level=scene["player"]["abilities"][id].Number;
   if(!definition.HasProgression||!(level>0))return "";
   var final=level>=7?NativeAbilityChoice(scene,id,7):null;if(final!=null)return final.Description(ru);
   var branch=NativeAbilityChoice(scene,id,4) as NativeRunCatalog.Choice;
   string description=level==Math.Truncate(level)&&level>=1&&level<=definition.Levels.Count?definition.Levels[(int)level-1].Description(ru):"";
   if(level>=5&&branch!=null)return string.IsNullOrEmpty(description)?branch.Description(ru):description;
   return level>=4&&branch!=null?branch.Description(ru):description;
  }
  internal static string NativeBaseAbilityDescription(NativeRunCatalog.Ability definition,double level,bool ru)
  {
   if(level!=Math.Truncate(level)||level<0||level>=definition.BaseLevels.Count)throw new ArgumentOutOfRangeException(nameof(level),"Native ability level outside imported source domain");
   return definition.BaseLevels[(int)level].Description(ru);
  }
 }
}
