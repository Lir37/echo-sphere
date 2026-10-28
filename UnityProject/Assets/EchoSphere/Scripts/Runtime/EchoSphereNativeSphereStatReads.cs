using System;
using System.Linq;
using System.Collections.Generic;
using EchoSphere.Presentation;
namespace EchoSphere.Runtime
{
 public sealed partial class EchoSphereRuntime
 {
  private double NativeLiveMastery=>_characterMasteryLevel!=0?_characterMasteryLevel:1;
  private static double NativeLiveLogicalDistance(UnityEngine.Component a,UnityEngine.Component b){var pa=NativeSourcePixels(a.transform.position);var pb=NativeSourcePixels(b.transform.position);return NativeMobileDistance(pa.x-pb.x,pa.y-pb.y);}
  private string NativeLiveAbilityMarker(string id,int stage){var kind=PlannerAbility(id);return stage==4?GetAbilityBranch(kind):GetAbilityFinal(kind);}
  private double NativeLiveFormationBonus(string kind,SphereAttackAgent sphere){int index=0,found=-1;foreach(var candidate in _spheres)if(candidate!=null&&candidate.gameObject.activeInHierarchy){if(candidate==sphere){found=index;break;}index++;}double efficiency=Math.Max(0,Math.Min(1,(double)(_networkState?.FormationEfficiency??1f)));bool Includes(EchoSphere.Core.SphereNetworkShape shape)=>shape!=null&&(found<0||shape.NodeIndexes.Contains(found));var dominant=_networkState?.DominantFormation;if(dominant!=null&&dominant.Active&&dominant.Type.ToString().ToLowerInvariant()==kind&&Includes(dominant))return efficiency;var secondary=_networkState?.SecondaryFormation;return secondary!=null&&secondary.Type.ToString().ToLowerInvariant()==kind&&Includes(secondary)?.5*efficiency:0;}
  private string NativeLiveCharacterFormationKind()
  {
   if(EchoSphere.Core.CharacterRules.ToSourceId(_sphereCharacterId)!="architect")return "none";var dominant=_networkState?.DominantFormation;string kind=dominant?.Type.ToString().ToLowerInvariant();return dominant!=null&&(kind=="line"||kind=="triangle"||kind=="square"||kind=="cluster")?kind:"none";
  }
  private double NativeLiveArchitectBonusScale(string kind)
  {
   double mastery=NativeLiveMastery,scale=1;
   if(mastery>=4&&_architectFormationType.ToString().ToLowerInvariant()==kind&&(double)_runTime-(double)_architectFormationChangedAt<=2)scale*=1.25;
   if(mastery>=5&&NativeLiveLocalSpherePoints().Count>=5)scale*=1.05;if(mastery>=7)scale*=1.05;if(mastery>=10&&kind!="none")scale*=1.10;return scale;
  }
  private double NativeLiveCharacterAttackSpeed()
  {
   string id=EchoSphere.Core.CharacterRules.ToSourceId(_sphereCharacterId);double mastery=NativeLiveMastery,multiplier=1+NativeRunCatalog.Instance.GetCharacter(id).SphereAttackSpeed;
   if(id=="conductor"&&(double)_conductorOverdriveTimer>0)multiplier+=mastery>=8?.28:mastery>=4?.23:.18;
   if(id=="spherist"){int count=_spheres.Count(sphere=>sphere!=null&&sphere.gameObject.activeInHierarchy);multiplier+=Math.Max(0,count-1)*.03;if(mastery>=4)multiplier+=Math.Max(0,count-1)*.005;if(count>=8&&mastery>=5)multiplier+=.05;if(count>=7&&mastery>=7)multiplier+=.02;if(count>=8&&mastery>=10)multiplier+=.03;}
   if(id=="berserker"){double missing=Math.Max(0,Math.Min(1,1-(double)_playerCore.CurrentHp/Math.Max(1,(double)_playerCore.MaxHp)));multiplier+=Math.Min(4,Math.Floor(missing/.2))*.04;if(mastery>=3)multiplier+=.02;}
   if(id=="architect"&&NativeLiveLocalSpherePoints().Count>=5&&NativeLiveCharacterFormationKind()=="cluster")multiplier+=.15*NativeLiveArchitectBonusScale("cluster");return multiplier;
  }
  private bool NativeLiveSphereTriangle(SphereAttackAgent sphere)
  {
   var others=_spheres.Where(s=>s!=null&&s.gameObject.activeInHierarchy).Where(other=>other!=sphere&&other.gameObject.activeInHierarchy).ToList();for(int i=0;i<others.Count;i++)for(int j=i+1;j<others.Count;j++)if(NativeLiveLogicalDistance(others[i],others[j])<=300&&NativeLiveLogicalDistance(others[i],sphere)<=300&&NativeLiveLogicalDistance(others[j],sphere)<=300)return true;return false;
  }
  private double NativeLiveSpecificArtifactDamage(SphereAttackAgent sphere)
  {
   double multiplier=1;string type=sphere.Type.ToString().ToLowerInvariant();
   if(_ownedArtifacts.Contains("resonance_core")&&sphere!=null&&_spheres.Where(s=>s!=null&&s.gameObject.activeInHierarchy).Any(other=>other!=sphere&&other.gameObject.activeInHierarchy&&NativeLiveLogicalDistance(other,sphere)<=190))multiplier*=1.12;
   if(_ownedArtifacts.Contains("triangle_circuit")&&NativeLiveSphereTriangle(sphere))multiplier*=1.15;
   if(_ownedArtifacts.Contains("unified_mind")){double strongest=0;foreach(var level in _sphereLevels.Values)strongest=Math.Max(strongest,level);multiplier*=1+strongest*.03;}
   if(_ownedArtifacts.Contains("zero_sphere"))multiplier*=1.20;
   var synergies=new HashSet<string>(NativeOwnedArtifactSynergies(_ownedArtifacts).Select(row=>row.Id),StringComparer.Ordinal);var completed=NativeLiveCompletedSets();
   if(completed.Contains("void_horizon")&&type=="void")multiplier*=1.08;
   if(completed.Contains("temporal_fold")&&((double)_timeStopTimer>0||(double)_teleportDamageBuffTimer>0))multiplier*=1.08;
   if(synergies.Contains("void_horizon")&&type=="void"&&_enemies.Any(enemy=>enemy!=null&&enemy.IsAlive&&enemy.IsElite))multiplier*=1.15;
   if(synergies.Contains("orbital_prism")&&type=="orbital")multiplier*=1.15;
   if(synergies.Contains("gravity_pulse")&&type=="pulse"){int grouped=_enemies.Count(enemy=>enemy!=null&&enemy.IsAlive&&_spheres.Where(s=>s!=null&&s.gameObject.activeInHierarchy).Any(other=>other.gameObject.activeInHierarchy&&NativeLiveLogicalDistance(enemy,other)<140));if(grouped>=3)multiplier*=1.15;}
   return multiplier;
  }
  private double NativeLiveSphereCopyOutput(SphereAttackAgent sphere)
  {
   string id=EchoSphere.Core.CharacterRules.ToSourceId(_sphereCharacterId);EchoSphere.Core.CharacterRules.TryParseSourceId(id,out var character);string signature=EchoSphere.Core.CharacterRules.Get(character).SignatureSphere.ToString().ToLowerInvariant();
   if(sphere.Type.ToString().ToLowerInvariant()!=signature||NativeLiveMastery<5)return 1;
   int index=0;foreach(var other in _spheres.Where(s=>s!=null&&s.gameObject.activeInHierarchy)){if(!other.gameObject.activeInHierarchy||other.Type.ToString().ToLowerInvariant()!=sphere.Type.ToString().ToLowerInvariant())continue;if(other==sphere)return index<2?1:index==2?.85:.70;index++;}return 1;
  }
  private double NativeLivePlannerSphereDamage(SphereAttackAgent sphere)
  {
   var mods=GetSourceSphereModifiers(sphere.Type);double damage=12;
   damage*=1+(double)NativeLiveAbilityLevel("damage")*.15;damage*=1+(double)GetShopUpgradeLevel("dmg")*.05;
   if((double)_mutationStage>=1)damage*=1.1;if(_chaosOrbBuff=="dmg"&&(double)_chaosOrbBuffTimer>0)damage*=1.2;if((double)_teleportDamageBuffTimer>0)damage*=2;
   if((double)_darkRitualOverloadTimer>0)damage*=1.25+(double)NativeLiveAbilityLevel("darkritual")*.04;
   if((double)_fireTrailTimer>0&&sphere!=null&&(double)mods.fire>0)damage*=1.2+(double)NativeLiveAbilityLevel("firetrail")*.025;
   if((double)_fireCatalystTimer>0&&sphere!=null&&(double)mods.fire>0)damage*=1.35;
   if(sphere!=null&&(double)mods.fire>0&&NativeLiveAbilityMarker("firetrail",4)=="firetrail_overdrive")damage*=1.15;
   if((double)_timeStopTimer>0&&(double)_timeStopTimer<=1&&NativeLiveAbilityMarker("timestop",7)=="timestop_temporal_core")damage*=2;
   double boost=(double)NativeLiveAbilityLevel("sphereboost");if(boost>0){double per=Math.Max(50,100-(boost-1)*10);damage+=Math.Floor((double)_kills/per);}
   // No native evolution marker producer emits the bare echoaccumulator token; retain its Editor oracle only.
   damage*=Math.Max(.25,1+NativeLiveArtifactEffect("sphereDamage"));damage*=NativeLiveSpecificArtifactDamage(sphere);damage*=(double)mods.damage;damage*=NativeLiveSphereCopyOutput(sphere);return damage;
  }
  private double NativeLivePlannerSphereDps(SphereAttackAgent sphere)
  {
   string type=sphere.Type.ToString().ToLowerInvariant();var definition=NativeRunCatalog.Instance.GetSphere(type);var mods=GetSourceSphereModifiers(sphere.Type);double damage=NativeLivePlannerSphereDamage(sphere),delay=Math.Max(.05,NativeLivePlannerSphereDelay(sphere)*definition.DelayMultiplier*(double)mods.delay);
   if(type=="orbital"){double level=NativeLiveSphereLevel("orbital");bool dance=GetSphereBranch(EchoSphere.Core.SphereId.Orbital)=="orbital_dance"&&NativeSphereFinalIndex(EchoSphere.Core.SphereId.Orbital)==2;var counts=EchoSphere.Core.OrbitalVisualGeometryRules.RingCounts(level,(_ownedArtifacts.Contains("orbital_crown")?1:0)+(dance?1:0));double angular=(1.8+Math.Min(2.4,Math.Max(1,level)*.28))*(double)mods.rotationSpeed,rate=(counts.Inner+counts.Outer)*angular/(Math.PI*2);return damage*.95*rate;}
   if(type=="prism")return damage*Math.Max(1,1+(double)mods.multishot)/delay;
   if(type=="pulse"){double waves=1+(_ownedArtifacts.Contains("pulse_crown")?1:0),interval=Math.Max(.08,1.15*(double)mods.auraPulse*(_ownedArtifacts.Contains("pulse_driver")?.90:1)*(NativeLivePlannerSphereDelay(sphere)/1.2));return damage*waves*4/interval;}
   if(type=="gravity"){double interval=Math.Max(.08,.80*(double)mods.auraPulse*(NativeLivePlannerSphereDelay(sphere)/1.2));return damage*4/interval;}
   if(type=="void")return damage*(NativeLiveSphereLevel("void")>=2?1.30:1.15)/delay;
   if(definition.Aura)return damage/delay;return damage*((1+(double)mods.multishot)*definition.Pellets)/delay;
  }
  private double NativeLivePlannerSphereDelay(SphereAttackAgent sphere=null)
  {
   double delay=1.2;delay*=Math.Pow(.93,(double)NativeLiveAbilityLevel("attackspeed"));delay/=Math.Max(.01,NativeLiveCharacterAttackSpeed());delay*=Math.Max(.5,1+NativeLiveArtifactEffect("sphereDelay"));
   if((double)_darkRitualOverloadTimer>0)delay*=.72;if((double)_fireTrailTimer>0&&sphere!=null&&(double)GetSourceSphereModifiers(sphere.Type).fire>0)delay*=.78;
   if(sphere!=null){double cluster=NativeLiveFormationBonus("cluster",sphere);if(cluster>0)delay*=1-.10*cluster;}return delay;
  }
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
  private static double NativeLogicalDistance(V a,V b)=>NativeMobileDistance(a["x"].Number-b["x"].Number,a["y"].Number-b["y"].Number);
  private static List<V> NativeLocalSpheres(V scene)=>scene["spheres"].Items.Where(sphere=>sphere["alive"].Truth&&NativeLogicalDistance(sphere["pos"],scene["player"]["pos"])<=420).OrderBy(sphere=>NativeLogicalDistance(sphere["pos"],scene["player"]["pos"])).Take(8).ToList();
  private static double NativeMastery(V player)=>player["characterMasteryLevel"].Truth?player["characterMasteryLevel"].Number:1;
  private static int NativeEngineerComponentSize(V scene)
  {
   var local=NativeLocalSpheres(scene);double mastery=NativeMastery(scene["player"]),range=mastery>=6?260:mastery>=2?240:220;int best=0;
   foreach(var start in local){var seen=new HashSet<V>{start};var queue=new Queue<V>();queue.Enqueue(start);while(queue.Count>0){var current=queue.Dequeue();foreach(var other in local)if(!seen.Contains(other)&&NativeLogicalDistance(current["pos"],other["pos"])<=range){seen.Add(other);queue.Enqueue(other);}}best=Math.Max(best,seen.Count);}return best;
  }
  private static double NativeCharacterRadius(V scene)
  {
   var player=scene["player"];string id=NativeCharacterWorldId(scene);double mastery=NativeMastery(player),multiplier=1+NativeRunCatalog.Instance.GetCharacter(id).SphereRadius;
   if(id=="spherist"&&mastery>=5&&scene["spheres"].Items.Count(sphere=>sphere["alive"].Truth)>=8)multiplier+=.05;
   if(id=="architect"){if(NativeLocalSpheres(scene).Count>=4)multiplier+=.05;if(mastery>=3)multiplier+=.02;}
   if(id=="engineer"&&NativeEngineerComponentSize(scene)>=3)multiplier+=.08;return multiplier;
  }
  private static string NativeCharacterFormationKind(V scene)
  {
   if(NativeCharacterWorldId(scene)!="architect")return "none";var dominant=scene["nativeNetwork"]["dominantFormation"];string kind=dominant["type"].Text;return dominant.Truth&&(kind=="line"||kind=="triangle"||kind=="square"||kind=="cluster")?kind:"none";
  }
  private static double NativeArchitectBonusScale(V scene,string kind)
  {
   var player=scene["player"];double mastery=NativeMastery(player),scale=1;
   if(mastery>=4&&player["architectFormationType"].Text==kind&&player["architectFormationChangedAt"].Raw is double&&scene["time"].Number-player["architectFormationChangedAt"].Number<=2)scale*=1.25;
   if(mastery>=5&&NativeLocalSpheres(scene).Count>=5)scale*=1.05;if(mastery>=7)scale*=1.05;if(mastery>=10&&kind!="none")scale*=1.10;return scale;
  }
  private static double NativeCharacterAttackSpeed(V scene)
  {
   var player=scene["player"];string id=NativeCharacterWorldId(scene);double mastery=NativeMastery(player),multiplier=1+NativeRunCatalog.Instance.GetCharacter(id).SphereAttackSpeed;
   if(id=="conductor"&&NativeOrZero(player["conductorOverdriveTimer"])>0)multiplier+=mastery>=8?.28:mastery>=4?.23:.18;
   if(id=="spherist"){int count=scene["spheres"].Items.Count(sphere=>sphere["alive"].Truth);multiplier+=Math.Max(0,count-1)*.03;if(mastery>=4)multiplier+=Math.Max(0,count-1)*.005;if(count>=8&&mastery>=5)multiplier+=.05;if(count>=7&&mastery>=7)multiplier+=.02;if(count>=8&&mastery>=10)multiplier+=.03;}
   if(id=="berserker"){double missing=Math.Max(0,Math.Min(1,1-player["hp"].Number/Math.Max(1,player["maxHp"].Number)));multiplier+=Math.Min(4,Math.Floor(missing/.2))*.04;if(mastery>=3)multiplier+=.02;}
   if(id=="architect"&&NativeLocalSpheres(scene).Count>=5&&NativeCharacterFormationKind(scene)=="cluster")multiplier+=.15*NativeArchitectBonusScale(scene,"cluster");return multiplier;
  }
  private static V NativeSphereMods(V scene,V sphere)=>scene["nativeSphereModifiers"][sphere["type"]];
  private static int NativeSphereSnapshotIndex(V scene,V sphere){int index=0;foreach(var candidate in scene["spheres"].Items){if(candidate==sphere)return index;index++;}return -1;}
  private static double NativeCachedFormationBonus(V scene,string kind,int index)
  {
   var network=scene["nativeNetwork"];var raw=network["formationEfficiency"];double efficiency=Math.Max(0,Math.Min(1,raw.Undefined||raw.Null?1:raw.Number));
   bool Includes(V shape)=>shape.Truth&&(index<0||shape["nodes"].Items.Any(node=>node.Number==index));
   var dominant=network["dominantFormation"];if(!(dominant["active"].Raw is bool active&&!active)&&dominant["type"].Text==kind&&Includes(dominant))return efficiency;
   var secondary=network["secondaryFormation"];return secondary["type"].Text==kind&&Includes(secondary)?.5*efficiency:0;
  }
  private static bool NativeOwnsArtifact(V scene,string id)=>scene["player"]["artifacts"].Items.Any(item=>item.Text==id);
  private static string NativeAbilityMarker(V scene,string ability,int stage){string prefix="ability:"+ability+":"+stage+":";foreach(var marker in scene["player"]["evolutions"].Items)if(marker.Text.StartsWith(prefix,StringComparison.Ordinal))return marker.Text.Substring(prefix.Length);return null;}
  private static bool NativeSphereTriangle(V scene,V sphere)
  {
   var others=scene["spheres"].Items.Where(other=>other!=sphere&&!(other["alive"].Raw is bool alive&&!alive)).ToList();for(int i=0;i<others.Count;i++)for(int j=i+1;j<others.Count;j++)if(NativeLogicalDistance(others[i]["pos"],others[j]["pos"])<=300&&NativeLogicalDistance(others[i]["pos"],sphere["pos"])<=300&&NativeLogicalDistance(others[j]["pos"],sphere["pos"])<=300)return true;return false;
  }
  private static double NativeSpecificArtifactDamage(V scene,V sphere)
  {
   double multiplier=1;string type=sphere["type"].Text;var player=scene["player"];
   if(NativeOwnsArtifact(scene,"resonance_core")&&sphere.Truth&&scene["spheres"].Items.Any(other=>other!=sphere&&!(other["alive"].Raw is bool alive&&!alive)&&NativeLogicalDistance(other["pos"],sphere["pos"])<=190))multiplier*=1.12;
   if(NativeOwnsArtifact(scene,"triangle_circuit")&&NativeSphereTriangle(scene,sphere))multiplier*=1.15;
   if(NativeOwnsArtifact(scene,"unified_mind")){double strongest=0;if(player["sphereProgression"].Raw is Dictionary<string,V> levels)foreach(var level in levels.Values)strongest=Math.Max(strongest,level.Number);multiplier*=1+strongest*.03;}
   if(NativeOwnsArtifact(scene,"zero_sphere"))multiplier*=1.20;
   var synergies=new HashSet<string>(NativeArtifactSynergies(scene).Select(row=>row.Id),StringComparer.Ordinal);var completed=NativeCompletedSets(scene);
   if(completed.Contains("void_horizon")&&type=="void")multiplier*=1.08;
   if(completed.Contains("temporal_fold")&&(NativeOrZero(player["timestopTimer"])>0||NativeOrZero(player["teleportDamageBuffTimer"])>0))multiplier*=1.08;
   if(synergies.Contains("void_horizon")&&type=="void"&&scene["enemies"].Items.Any(enemy=>enemy["isElite"].Truth&&enemy["hp"].Number>0))multiplier*=1.15;
   if(synergies.Contains("orbital_prism")&&type=="orbital")multiplier*=1.15;
   if(synergies.Contains("gravity_pulse")&&type=="pulse"){int grouped=scene["enemies"].Items.Count(enemy=>enemy["hp"].Number>0&&scene["spheres"].Items.Any(other=>!(other["alive"].Raw is bool alive&&!alive)&&NativeLogicalDistance(enemy["pos"],other["pos"])<140));if(grouped>=3)multiplier*=1.15;}
   return multiplier;
  }
  private static double NativeSphereCopyOutput(V scene,V sphere)
  {
   string id=NativeCharacterWorldId(scene);EchoSphere.Core.CharacterRules.TryParseSourceId(id,out var character);string signature=EchoSphere.Core.CharacterRules.Get(character).SignatureSphere.ToString().ToLowerInvariant();
   if(sphere["type"].Text!=signature||NativeMastery(scene["player"])<5)return 1;
   int index=0;foreach(var other in scene["spheres"].Items){if(!other["alive"].Truth||other["type"].Text!=sphere["type"].Text)continue;if(other==sphere)return index<2?1:index==2?.85:.70;index++;}return 1;
  }
  internal static double NativePlannerSphereDamage(V scene,V sphere)
  {
   var player=scene["player"];var abilities=player["abilities"];var mods=NativeSphereMods(scene,sphere);double damage=12;
   damage*=1+NativeOrZero(abilities["damage"])*.15;damage*=1+NativeOrZero(scene["shopUpgrades"]["dmg"])*.05;
   if(player["mutationStage"].Number>=1)damage*=1.1;if(player["chaosOrbBuff"].Text=="dmg"&&player["chaosOrbBuffTimer"].Number>0)damage*=1.2;if(player["teleportDamageBuffTimer"].Number>0)damage*=2;
   if(player["overloadTimer"].Number>0)damage*=1.25+NativeOrZero(abilities["darkritual"])*.04;
   if(player["fireTrailTimer"].Number>0&&sphere.Truth&&mods["fire"].Number>0)damage*=1.2+NativeOrZero(abilities["firetrail"])*.025;
   if(player["fireCatalystTimer"].Number>0&&sphere.Truth&&mods["fire"].Number>0)damage*=1.35;
   if(sphere.Truth&&mods["fire"].Number>0&&NativeAbilityMarker(scene,"firetrail",4)=="firetrail_overdrive")damage*=1.15;
   if(player["timestopTimer"].Number>0&&player["timestopTimer"].Number<=1&&NativeAbilityMarker(scene,"timestop",7)=="timestop_temporal_core")damage*=2;
   double boost=NativeOrZero(abilities["sphereboost"]);if(boost>0){double per=Math.Max(50,100-(boost-1)*10);damage+=Math.Floor(player["kills"].Number/per);}
   if(player["evolutions"].Items.Any(marker=>marker.Text=="echoaccumulator"))damage+=player["sphereXpAccumulator"].Number*.5;
   damage*=Math.Max(.25,1+NativeArtifactEffectTotal(scene,"sphereDamage"));damage*=NativeSpecificArtifactDamage(scene,sphere);damage*=mods["damage"].Number;damage*=NativeSphereCopyOutput(scene,sphere);return damage;
  }
  internal static double NativePlannerSphereDps(V scene,V sphere)
  {
   string type=sphere["type"].Text;var definition=NativeRunCatalog.Instance.GetSphere(type);var mods=NativeSphereMods(scene,sphere);double damage=NativePlannerSphereDamage(scene,sphere),delay=Math.Max(.05,NativePlannerSphereDelay(scene,sphere)*definition.DelayMultiplier*mods["delay"].Number);
   if(type=="orbital"){double level=NativeSphereLevel(scene,"orbital");bool dance=scene["player"]["sphereBranches"]["orbital"].Text=="orbital_dance"&&NativeMutationFinal(scene["player"],"orbital")==2;var counts=EchoSphere.Core.OrbitalVisualGeometryRules.RingCounts(level,(NativeOwnsArtifact(scene,"orbital_crown")?1:0)+(dance?1:0));double angular=(1.8+Math.Min(2.4,Math.Max(1,level)*.28))*mods["rotationSpeed"].Number,rate=(counts.Inner+counts.Outer)*angular/(Math.PI*2);return damage*.95*rate;}
   if(type=="prism")return damage*Math.Max(1,1+mods["multishot"].Number)/delay;
   if(type=="pulse"){double waves=1+(NativeOwnsArtifact(scene,"pulse_crown")?1:0),interval=Math.Max(.08,1.15*mods["auraPulse"].Number*(NativeOwnsArtifact(scene,"pulse_driver")?.90:1)*(NativePlannerSphereDelay(scene,sphere)/1.2));return damage*waves*4/interval;}
   if(type=="gravity"){double interval=Math.Max(.08,.80*mods["auraPulse"].Number*(NativePlannerSphereDelay(scene,sphere)/1.2));return damage*4/interval;}
   if(type=="void")return damage*(NativeSphereLevel(scene,"void")>=2?1.30:1.15)/delay;
   if(definition.Aura)return damage/delay;return damage*((1+mods["multishot"].Number)*definition.Pellets)/delay;
  }
  internal static double NativePlannerSphereRadius(V scene,V sphere)
  {
   var player=scene["player"];double radius=130;radius*=1+NativeOrZero(player["abilities"]["radius"])*.15;radius*=1+NativeOrZero(scene["shopUpgrades"]["radius"])*.05;radius*=Math.Max(.5,1+NativeArtifactEffectTotal(scene,"sphereRadius"));
   if(player["chaosOrbBuff"].Text=="radius"&&player["chaosOrbBuffTimer"].Number>0)radius*=1.2;if(player["mutationStage"].Number>=2)radius*=1.15;radius*=NativeCharacterRadius(scene);radius*=NativeSphereMods(scene,sphere)["radius"].Number;
   if(NativeCharacterWorldId(scene)=="architect"&&player["characterMasteryLevel"].Number>=3)radius*=1.02;return radius;
  }
  internal static double NativePlannerSphereDelay(V scene,V sphere=default)
  {
   var player=scene["player"];double delay=1.2;delay*=Math.Pow(.93,NativeOrZero(player["abilities"]["attackspeed"]));delay/=Math.Max(.01,NativeCharacterAttackSpeed(scene));delay*=Math.Max(.5,1+NativeArtifactEffectTotal(scene,"sphereDelay"));
   if(player["overloadTimer"].Number>0)delay*=.72;if(player["fireTrailTimer"].Number>0&&sphere.Truth&&NativeSphereMods(scene,sphere)["fire"].Number>0)delay*=.78;
   if(sphere.Truth){double cluster=NativeCachedFormationBonus(scene,"cluster",NativeSphereSnapshotIndex(scene,sphere));if(cluster>0)delay*=1-.10*cluster;}return delay;
  }
#endif
 }
}
