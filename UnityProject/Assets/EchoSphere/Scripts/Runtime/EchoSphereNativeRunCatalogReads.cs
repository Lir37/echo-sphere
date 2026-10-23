using System;
using System.Collections.Generic;
using System.Linq;
using EchoSphere.Presentation;
namespace EchoSphere.Runtime
{
 public sealed partial class EchoSphereRuntime
 {
  internal static V NativeLocalizedName(NativeRunCatalog.Text text)=>VisualJs.Obj("ru",text.NameRu,"en",text.NameEn);
#if UNITY_EDITOR
  internal static V NativeEvolutionChoices(V scene,string id,bool ability,int stage)
  {
   var result=new List<V>();
   V Card(NativeRunCatalog.Text text)=>VisualJs.Obj("type",ability?"ability":"sphere","name",NativeLocalizedName(text),"desc",VisualJs.Obj("ru",text.DescriptionRu,"en",text.DescriptionEn),"currentLevel",stage,"newLevel",stage);
   if(ability){var definition=NativeRunCatalog.Instance.GetAbility(id);if(!definition.HasProgression)return VisualJs.Arr();IEnumerable<NativeRunCatalog.Definition> pool=definition.Branches;if(stage==7){var branch=NativeAbilityChoice(scene,id,4) as NativeRunCatalog.Choice;if(branch==null)return VisualJs.Arr();pool=branch.HasFinalPool?branch.Finals:definition.FallbackFinals;}int index=0;foreach(var text in pool.Take(3)){var card=Card(text);card["ability"]=id;card["abilityStage"]=stage==4?"branch":"final";card["abilityEvolutionIndex"]=index++;result.Add(card);}}
   else {var definition=NativeRunCatalog.Instance.GetSphere(id);if(stage==4){foreach(var branch in definition.Branches.Take(3)){var card=Card(branch.BranchCard);card["sphereType"]=id;card["sphereBranch"]=branch.Id;card["sphereStage"]="branch";result.Add(card);}}else{var branch=definition.Branches.FirstOrDefault(row=>row.Id==scene["player"]["sphereBranches"][id].Text);if(branch!=null)for(int index=0;index<Math.Min(3,branch.FinalCards.Count);index++){var card=Card(branch.FinalCards[index]);card["sphereType"]=id;card["sphereBranch"]=branch.Id;card["sphereStage"]="final";card["sphereFinalIndex"]=index;result.Add(card);}}}
   return VisualJs.Arr(result.ToArray());
  }
  internal static string NativeUpgradeChoiceKey(V choice)
  {
   string Value(string key,string fallback=""){var value=choice[key];return value.Undefined||value.Null?fallback:value.Text;}
   if(choice["type"].Text=="modifier")return "modifier:"+Value("modifier","unknown");
   if(choice["type"].Text=="sphere")return string.Join(":","sphere",Value("sphereType","unknown"),Value("sphereStage","upgrade"),Value("currentLevel"),Value("newLevel"),Value("sphereBranch"),Value("sphereFinalIndex"));
   return string.Join(":","ability",Value("ability","unknown"),Value("abilityStage","upgrade"),Value("currentLevel"),Value("newLevel"),Value("abilityEvolutionIndex"));
  }
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
#endif
  internal static string NativeBaseAbilityDescription(NativeRunCatalog.Ability definition,double level,bool ru)
  {
   if(level!=Math.Truncate(level)||level<0||level>=definition.BaseLevels.Count)throw new ArgumentOutOfRangeException(nameof(level),"Native ability level outside imported source domain");
   return definition.BaseLevels[(int)level].Description(ru);
  }
 }
}
