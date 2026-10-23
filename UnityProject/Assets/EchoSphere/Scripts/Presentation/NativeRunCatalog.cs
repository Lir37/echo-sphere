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
        [Serializable] public sealed class Sphere : Definition {public string Color;public double RangeMultiplier;public bool Projectile;public List<Choice> Branches=new List<Choice>();}
        [Serializable] public sealed class Ability : Definition {public string Category;public bool HasProgression;public List<Text> Levels=new List<Text>(),BaseLevels=new List<Text>();public List<Choice> Branches=new List<Choice>();public List<Definition> FallbackFinals=new List<Definition>();}
        [Serializable] public sealed class Effect {public string Id;public double Value;}
        [Serializable] public sealed class Artifact : Definition {public string Rarity,Mechanic;public List<Effect> Effects=new List<Effect>();public double Effect(string id){foreach(var row in Effects)if(row.Id==id)return row.Value;return 0;}}
        [Serializable] public sealed class Synergy : Definition {public List<string> Requires=new List<string>();}
        [Serializable] public sealed class Set : Definition {public List<string> SynergyIds=new List<string>(),ArtifactIds=new List<string>();}
        [Serializable] public sealed class Element : Definition {public string Icon,Color;}
        [Serializable] public sealed class SphereAbilityLink : Definition {public string Sphere,Branch,Ability,AbilityFinal;}
        public string SourceCommit;
        public List<Sphere> Spheres=new List<Sphere>();public List<Ability> Abilities=new List<Ability>();public List<Artifact> Artifacts=new List<Artifact>();
        public List<Synergy> Synergies=new List<Synergy>();public List<Set> Sets=new List<Set>();public List<Element> Elements=new List<Element>();
        public List<Element> Runes=new List<Element>();
        public List<Definition> Rarities=new List<Definition>();public List<SphereAbilityLink> SphereAbilityLinks=new List<SphereAbilityLink>();
        [NonSerialized] private Dictionary<string,Sphere> _spheres;[NonSerialized] private Dictionary<string,Ability> _abilities;[NonSerialized] private Dictionary<string,Artifact> _artifacts;
        [NonSerialized] private Dictionary<string,Element> _elements;[NonSerialized] private Dictionary<string,Choice> _branches;
        private static NativeRunCatalog _instance;
        public static NativeRunCatalog Instance {get{if(_instance==null)_instance=Resources.Load<NativeRunCatalog>("EchoSphere/NativeRunCatalog");if(_instance==null)throw new InvalidOperationException("Native run catalog missing");return _instance;}}
        private void OnEnable()=>ClearLookupCaches();
        private void OnValidate()=>ClearLookupCaches();
        private void ClearLookupCaches(){_spheres=null;_abilities=null;_artifacts=null;_elements=null;_branches=null;}
        private static Dictionary<string,T> Index<T>(IEnumerable<T> rows) where T:Definition {var map=new Dictionary<string,T>(StringComparer.Ordinal);foreach(var row in rows)map.Add(row.Id,row);return map;}
        public Sphere GetSphere(string id){if(_spheres==null)_spheres=Index(Spheres);return _spheres.TryGetValue(id,out var row)?row:null;}
        public Ability GetAbility(string id){if(_abilities==null)_abilities=Index(Abilities);return _abilities.TryGetValue(id,out var row)?row:null;}
        public Artifact GetArtifact(string id){if(_artifacts==null)_artifacts=Index(Artifacts);return _artifacts.TryGetValue(id,out var row)?row:null;}
        public Element GetElement(string id){if(id==null)return null;if(_elements==null)_elements=Index(Elements);return _elements.TryGetValue(id,out var row)?row:null;}
        public Choice GetSphereBranch(string id){if(id==null)return null;if(_branches==null){_branches=new Dictionary<string,Choice>(StringComparer.Ordinal);foreach(var sphere in Spheres)foreach(var branch in sphere.Branches)_branches.Add(branch.Id,branch);}return _branches.TryGetValue(id,out var row)?row:null;}
        public Element GetRune(string id){foreach(var rune in Runes)if(rune.Id==id)return rune;throw new InvalidOperationException("Unknown native rune: "+id);}
        public string RarityLabel(string id,bool ru){foreach(var row in Rarities)if(row.Id==id)return row.Name(ru);return "undefined";}
    }
}
