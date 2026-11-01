using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    [CreateAssetMenu(menuName="ECHO SPHERE/Run presentation catalog")]
    public sealed class NativeRunCatalog : ScriptableObject
    {
        [Serializable] public class Text {public string NameRu,NameEn,DescriptionRu,DescriptionEn;public string Name(bool ru)=>ru?NameRu:NameEn;public string Description(bool ru)=>ru?DescriptionRu:DescriptionEn;}
        [Serializable] public class Definition : Text {public string Id;}
        [Serializable] public sealed class Choice : Definition {public string Element;public Text BranchCard=new Text();public List<Text> FinalCards=new List<Text>();public bool HasFinalPool;public List<Definition> Finals=new List<Definition>();}
        [Serializable] public sealed class Sphere : Definition {public string Color;public double RangeMultiplier,DelayMultiplier,Pellets;public bool Projectile,Aura;public List<Choice> Branches=new List<Choice>();}
        [Serializable] public sealed class Ability : Definition {public string Category;public bool HasProgression;public List<Text> Levels=new List<Text>(),BaseLevels=new List<Text>();public List<Choice> Branches=new List<Choice>();public List<Definition> FallbackFinals=new List<Definition>();}
        [Serializable] public sealed class Pocket : Definition {public double X,Y,Radius;public string Accent;}
        [Serializable] public sealed class Region : Definition {public List<Pocket> Pockets=new List<Pocket>();}
        [Serializable] public sealed class Character {public string Id;public double MoveSpeed,SphereDamage,SphereRadius,SphereAttackSpeed;}
        [Serializable] public sealed class Effect {public string Id;public double Value;}
        [Serializable] public sealed class Artifact : Definition {public string Rarity,Mechanic;public List<Effect> Effects=new List<Effect>();public double Effect(string id){foreach(var row in Effects)if(row.Id==id)return row.Value;return 0;}}
        [Serializable] public sealed class Synergy : Definition {public List<string> Requires=new List<string>();}
        [Serializable] public sealed class Set : Definition {public List<string> SynergyIds=new List<string>(),ArtifactIds=new List<string>();}
        [Serializable] public sealed class Element : Definition {public string Icon,Color;}
        [Serializable] public sealed class SphereAbilityLink : Definition {public string Sphere,Branch,Ability,AbilityFinal;}
        public string SourceCommit;
        public List<Sphere> Spheres=new List<Sphere>();public List<Ability> Abilities=new List<Ability>();public List<Artifact> Artifacts=new List<Artifact>();
        public List<Synergy> Synergies=new List<Synergy>();public List<Set> Sets=new List<Set>();public List<Element> Elements=new List<Element>();
        public List<Region> Regions=new List<Region>();public List<Pocket> WorldPockets=new List<Pocket>();public List<Text> RegionPhases=new List<Text>();
        public List<Character> Characters=new List<Character>();
        public List<Element> Runes=new List<Element>();
        public List<Definition> Rarities=new List<Definition>();public List<SphereAbilityLink> SphereAbilityLinks=new List<SphereAbilityLink>();
        [NonSerialized] private Dictionary<string,Sphere> _spheres;[NonSerialized] private Dictionary<string,Ability> _abilities;[NonSerialized] private Dictionary<string,Artifact> _artifacts;
        [NonSerialized] private Dictionary<string,Element> _elements;[NonSerialized] private Dictionary<string,Choice> _branches;
        [NonSerialized] private Dictionary<string,Region> _campaignRegions;
        private static NativeRunCatalog _instance;
        public static NativeRunCatalog Instance {get{if(_instance==null)_instance=Resources.Load<NativeRunCatalog>("EchoSphere/NativeRunCatalog");if(_instance==null)throw new InvalidOperationException("Native run catalog missing");return _instance;}}
        private void OnEnable()=>ClearLookupCaches();
        private void OnValidate()=>ClearLookupCaches();
        private void ClearLookupCaches(){_spheres=null;_abilities=null;_artifacts=null;_elements=null;_branches=null;_campaignSets=null;_campaignSynergies=null;_campaignRegions=null;}
        private static Dictionary<string,T> Index<T>(IEnumerable<T> rows) where T:Definition {var map=new Dictionary<string,T>(StringComparer.Ordinal);foreach(var row in rows)map.Add(row.Id,row);return map;}
        [NonSerialized] private List<Set> _campaignSets;
        [NonSerialized] private List<Synergy> _campaignSynergies;
        public IReadOnlyList<Set> GetSets(bool modern){if(!modern)return Sets;PrepareCampaignSets();return _campaignSets;}
        public IReadOnlyList<Synergy> GetSynergies(bool modern){if(!modern)return Synergies;PrepareCampaignSets();return _campaignSynergies;}
        private void PrepareCampaignSets(){if(_campaignSets!=null)return;_campaignSets=new List<Set>(Sets);_campaignSynergies=new List<Synergy>(Synergies);
            foreach(var row in EchoSphere.Core.CampaignArtifactSets.All){string synergy="campaign:"+row.Id;_campaignSets.Add(new Set{Id=row.Id,NameRu=row.Ru,NameEn=row.En,DescriptionRu=row.DescriptionRu,DescriptionEn=row.DescriptionEn,ArtifactIds=new List<string>(row.Requires),SynergyIds=new List<string>{synergy}});_campaignSynergies.Add(new Synergy{Id=synergy,NameRu=row.Ru,NameEn=row.En,DescriptionRu=row.DescriptionRu,DescriptionEn=row.DescriptionEn,Requires=new List<string>(row.Requires)});}}
        public Sphere GetSphere(string id){if(_spheres==null)_spheres=Index(Spheres);return _spheres.TryGetValue(id,out var row)?row:null;}
        public Ability GetAbility(string id){if(_abilities==null)_abilities=Index(Abilities);return _abilities.TryGetValue(id,out var row)?row:null;}
        public Artifact GetArtifact(string id){if(_artifacts==null)_artifacts=Index(Artifacts);if(_artifacts.TryGetValue(id,out var row))return row;
            var item=EchoSphere.Core.ArtifactRewardRules.Get(id);if(item==null)return null;
            row=new Artifact{Id=item.Id,NameRu=item.NameRussian,NameEn=item.NameEnglish,DescriptionRu=item.DescriptionRussian,DescriptionEn=item.DescriptionEnglish,Rarity=item.Rarity.ToString().ToLowerInvariant(),Mechanic=item.Mechanic};foreach(var effect in item.Effects)row.Effects.Add(new Effect{Id=effect.Key,Value=effect.Value});_artifacts[id]=row;return row;}
        public Element GetElement(string id){if(id==null)return null;if(_elements==null)_elements=Index(Elements);return _elements.TryGetValue(id,out var row)?row:null;}
        public Choice GetSphereBranch(string id){if(id==null)return null;if(_branches==null){_branches=new Dictionary<string,Choice>(StringComparer.Ordinal);foreach(var sphere in Spheres)foreach(var branch in sphere.Branches)_branches.Add(branch.Id,branch);}return _branches.TryGetValue(id,out var row)?row:null;}
        public Region GetRegion(string id){foreach(var row in Regions)if(row.Id==id)return row;
            if(EchoSphere.Core.CampaignRegions.TryParse(id,out var region)&&(int)region>=2){if(_campaignRegions==null)_campaignRegions=new Dictionary<string,Region>();if(_campaignRegions.TryGetValue(id,out var cached))return cached;var profile=EchoSphere.Core.CampaignRegions.Get(region);var value=new Region{Id=id,NameRu=profile.Ru,NameEn=profile.En,DescriptionRu=profile.DescriptionRu,DescriptionEn=profile.DescriptionEn};foreach(var p in profile.Pockets)value.Pockets.Add(new Pocket{Id=p.Id,NameRu=p.Ru,NameEn=p.En,X=p.X,Y=p.Y,Radius=p.Ecology==0?340:330,Accent=profile.Accent});_campaignRegions[id]=value;return value;}
if(Regions.Count==0)throw new InvalidOperationException("Native region catalog missing");return Regions[0];}
        public Character GetCharacter(string id){foreach(var row in Characters)if(row.Id==id)return row;throw new InvalidOperationException("Unknown native character: "+id);}
        private static readonly Element RunicCell=new Element{Id="runic_cell",NameRu="Руническая ячейка",NameEn="Runic Cell",Color="#8cffb8",Icon="◇",DescriptionRu="+5 кредитов при завершении забега. Только Endless и Convergence.",DescriptionEn="+5 Credits at run end. Endless and Convergence only."};
        public Element GetRune(string id){if(id=="runic_cell")return RunicCell;foreach(var rune in Runes)if(rune.Id==id)return rune;throw new InvalidOperationException("Unknown native rune: "+id);}
        public string RarityLabel(string id,bool ru){foreach(var row in Rarities)if(row.Id==id)return row.Name(ru);return "undefined";}
    }
}
