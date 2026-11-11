using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    [CreateAssetMenu(menuName="ECHO SPHERE/Run presentation catalog")]
    public sealed class NativeRunCatalog : ScriptableObject
    {
        [Serializable] public class Text {public string NameRu,NameEn,DescriptionRu,DescriptionEn;public virtual string Name(bool ru)=>NativePlayerCopy.Text(ru?NameRu:NameEn,ru);public virtual string Description(bool ru)=>NativePlayerCopy.Text(ru?DescriptionRu:DescriptionEn,ru);}
        [Serializable]
        public class Definition : Text
        {
            public string Id;

            public override string Description(bool ru) => Id switch
            {
                "blast_core" => ru
                    ? "Каждая следующая сфера выпускает более сильную волну. Дополнительный взрыв происходит вокруг вас."
                    : "Each successive Sphere releases a stronger wave. An extra blast erupts around you.",
                "blast_echo_network" or "blast_resonance_f1" or "blast_network_f1" or "blast_core_f1" => ru
                    ? "Волна сначала проходит по связанным сферам и становится сильнее с каждой посещённой сферой."
                    : "The wave visits linked Spheres first and grows stronger with each Sphere it reaches.",
                "blast_resonant_core" or "blast_resonance_f2" or "blast_network_f2" or "blast_core_f2" => ru
                    ? "Каждая Стандартная сфера выпускает дополнительную ударную волну."
                    : "Each Standard Sphere releases an extra damaging pulse.",
                "blast_infinite_pulse" or "blast_resonance_f3" or "blast_network_f3" or "blast_core_f3" => ru
                    ? "После первой волны сферы выпускают ослабленную волну в обратном порядке."
                    : "After the first wave, the Spheres release a weaker wave in reverse order.",
                "firetrail_sanctum" => ru
                    ? "При активации Ауры сразу готовы к следующей атаке."
                    : "Activating Overheat makes Aura Spheres ready to attack immediately.",
                "firetrail_network" or "firetrail_overdrive_f1" or "firetrail_ignition_f1" or "firetrail_sanctum_f1" => ru
                    ? "Связанные огненные сферы быстрее совершают следующую атаку."
                    : "Linked fire-aligned Spheres make their next attack sooner.",
                "firetrail_catalyst" or "firetrail_overdrive_f2" or "firetrail_ignition_f2" or "firetrail_sanctum_f2" => ru
                    ? "Враги с горением, заморозкой или ядом загораются. Огненные сферы получают дополнительный урон на 4 секунды."
                    : "Burning, frozen, or poisoned enemies are set ablaze. Fire-aligned Spheres gain extra damage for 4 seconds.",
                "firetrail_inferno" or "firetrail_overdrive_f3" or "firetrail_ignition_f3" or "firetrail_sanctum_f3" => ru
                    ? "При активации каждая связь между огненными сферами ещё сильнее приближает их следующую атаку."
                    : "On activation, every link between fire-aligned Spheres brings their next attack forward by a larger amount.",
                "shield_reflector" => ru
                    ? "Когда щит поглощает удар, ближайший враг получает часть урона."
                    : "When the shield blocks a hit, the nearest enemy takes part of its damage.",
                "shield_echo_guard_f1" or "shield_reflector_f1" or "shield_bastion_f1" or "shield_network_guard" => ru
                    ? "Связанные сферы дают дополнительные заряды щита, до 5 зарядов. Активация приближает следующую атаку ближайших сфер."
                    : "Linked Spheres grant extra shield charges, up to 5 total. Activating the shield brings forward nearby Spheres' next attacks.",
                "shield_echo_guard_f2" or "shield_reflector_f2" or "shield_bastion_f2" or "shield_iron_dome" => ru
                    ? "Ближайшие сферы дают до двух дополнительных зарядов при активации. Всего можно получить не больше 5 зарядов."
                    : "Nearby Spheres grant up to two extra charges on activation, up to 5 charges total.",
                "shield_echo_guard_f3" or "shield_reflector_f3" or "shield_bastion_f3" or "shield_resonant_guard" => ru
                    ? "Когда щит поглощает удар, потраченный заряд сразу восстанавливается."
                    : "When the shield blocks a hit, the spent charge is immediately restored.",
                "teleport_beacon" => ru
                    ? "На 4 секунды после телепорта урон сфер удваивается."
                    : "Sphere damage doubles for 4 seconds after teleporting.",
                "teleport_spatial_network" or "teleport_echo_jump_f1" or "teleport_beacon_f1" or "teleport_phase_f1" => ru
                    ? "Прыжок к сфере поражает врагов рядом с ней и со связанными сферами."
                    : "Jumping to a Sphere damages enemies near it and its linked Spheres.",
                "teleport_hunter_beacon" or "teleport_echo_jump_f2" or "teleport_beacon_f2" or "teleport_phase_f2" => ru
                    ? "Прыжок к Снайперской сфере удваивает урон сфер на 5 секунд."
                    : "Jumping to a Sniper Sphere doubles Sphere damage for 5 seconds.",
                "teleport_phase_break" or "teleport_echo_jump_f3" or "teleport_beacon_f3" or "teleport_phase_f3" => ru
                    ? "После прыжка вы ненадолго неуязвимы. Длинный прыжок поражает врагов в нескольких точках на пути."
                    : "You are briefly invulnerable after jumping. Long jumps damage enemies at intervals along the path.",
                "lightning_echo_storm" => ru
                    ? "Разряд поражает врагов рядом с каждой установленной сферой, прежде чем ударить по основным целям."
                    : "The discharge hits enemies near every deployed Sphere before striking its main targets.",
                "lightning_thunder_chain" or "lightning_echo_storm_f2" or "lightning_relay_f2" or "lightning_overload_f2" => ru
                    ? "Каждая установленная сфера добавляет цель. Разряд также наносит ослабленный удар по следующему врагу в цепи."
                    : "Each deployed Sphere adds a target. The discharge also deals a weaker hit to the next enemy in the chain.",
                "timestop_time_anchor" => ru
                    ? "Враги в основной области остановки времени замирают на секунду дольше."
                    : "Enemies in the main Time Stop area stay frozen for one extra second.",
                "darkritual_void_pact" => ru
                    ? "Если после жертвы осталось меньше 35% здоровья, усиление длится ещё 2 секунды."
                    : "If the sacrifice leaves you below 35% health, the boost lasts 2 extra seconds.",
                "darkritual_void_engine" or "darkritual_blood_link_f3" or "darkritual_sacrifice_f3" or "darkritual_void_pact_f3" => ru
                    ? "Если после жертвы осталось меньше 35% здоровья, усиление длится ещё 2 секунды. При здоровье ниже 20% сферы также выпускают ударные волны."
                    : "If the sacrifice leaves you below 35% health, the boost lasts 2 extra seconds. Below 20% health, Spheres also release damaging pulses.",
                _ => base.Description(ru)
            };
        }
        [Serializable] public sealed class Choice : Definition
        {
            public string Element;
            public Text BranchCard=new Text();
            public List<Text> FinalCards=new List<Text>();
            public bool HasFinalPool;
            public List<Definition> Finals=new List<Definition>();

            public Text SphereCard(int final = -1)
            {
                var source = final < 0 ? BranchCard : FinalCards[final];
                var copy = (Id, final) switch
                {
                    ("gravity_tide", 0) => ("Огненные импульсы отталкивают врагов от сферы.", "Fiery pulses push enemies away from the Sphere."),
                    ("gravity_tide", 1) => ("Огненные импульсы сильнее притягивают врагов к сфере.", "Fiery pulses pull enemies toward the Sphere more strongly."),
                    ("gravity_tide", _) => ("Огненные импульсы то притягивают врагов, то отталкивают их.", "Fiery pulses alternate between pulling enemies in and pushing them away."),
                    ("gravity_collapse", 2) => ("Импульсы сильнее бьют по раненым врагам. Если в зоне четыре врага или больше, в центре происходит дополнительный взрыв, который замедляет цели.", "Pulses hit wounded enemies harder. With four or more enemies in range, an extra blast at the center slows its targets."),
                    ("pulse_wave", 1) => ("Широкая волна отталкивает врагов, замедляет их и накладывает заморозку.", "A wider wave pushes enemies back, slows them, and applies Freeze."),
                    ("pulse_wave", _) => ("Волна отталкивает врагов, замедляет их и накладывает заморозку.", "The wave pushes enemies back, slows them, and applies Freeze."),
                    ("void_reaper", 0) => ("Попадания лечат сильнее. Убийства восстанавливают здоровье и выпускают осколки Пустоты.", "Hits heal more. Kills restore health and release Void shards."),
                    ("void_reaper", 1) => ("Попадания лечат. Убийства восстанавливают больше здоровья и выпускают усиленные осколки Пустоты.", "Hits heal. Kills restore more health and release stronger Void shards."),
                    ("void_reaper", 2) => ("Попадания и убийства лечат сильнее. Убийства выпускают три осколка Пустоты.", "Hits and kills heal more. Kills release three Void shards."),
                    ("void_reaper", _) => ("Попадания и убийства восстанавливают здоровье. Убийства выпускают осколки Пустоты.", "Hits and kills restore health. Kills release Void shards."),
                    ("chain_leech", 1) => ("Попадания лечат сильнее. Убийства тоже восстанавливают здоровье.", "Hits heal more. Kills restore health as well."),
                    ("chain_leech", 2) => ("Попадания и убийства лечат. Цели с запасом здоровья ниже 40% получают дополнительный урон.", "Hits and kills heal. Targets below 40% health take extra damage."),
                    ("chain_leech", _) => ("Попадания восстанавливают здоровье.", "Hits restore health."),
                    _ => (null, null)
                };
                if (copy.Item1 == null) return source;
                if (Id == "void_reaper")
                {
                    var weakTargetBonus = final == 1 ? 25 : 12;
                    copy = (
                        copy.Item1 + $" Дополнительно: +{weakTargetBonus}% урона по целям с 25% здоровья или меньше.",
                        copy.Item2 + $" Also deals +{weakTargetBonus}% damage to targets at 25% health or lower.");
                }
                if (Id == "void_reaper" || Id == "chain_leech")
                    copy = (copy.Item1 + " У восстановления в бою есть предел.", copy.Item2 + " Combat healing has a limit.");
                return new Text
                {
                    NameRu = source.NameRu, NameEn = source.NameEn,
                    DescriptionRu = copy.Item1,
                    DescriptionEn = copy.Item2
                };
            }
        }
        [Serializable] public sealed class Sphere : Definition {public string Color;public double RangeMultiplier,DelayMultiplier,Pellets;public bool Projectile,Aura;public List<Choice> Branches=new List<Choice>();}
        [Serializable] public sealed class Ability : Definition {public override string Name(bool ru)=>Id=="crit"?(ru?"Шанс критического удара":"Critical Chance"):base.Name(ru);public override string Description(bool ru)=>NativePlayerCopy.AbilityDescription(Id,ru,base.Description(ru));public string Category;public bool HasProgression;public List<Text> Levels=new List<Text>(),BaseLevels=new List<Text>();public List<Choice> Branches=new List<Choice>();public List<Definition> FallbackFinals=new List<Definition>();}
        [Serializable] public sealed class Pocket : Definition {public double X,Y,Radius;public string Accent;}
        [Serializable] public sealed class Region : Definition {public List<Pocket> Pockets=new List<Pocket>();}
        [Serializable] public sealed class Character {public string Id;public double MoveSpeed,SphereDamage,SphereRadius,SphereAttackSpeed;}
        [Serializable] public sealed class Effect {public string Id;public double Value;}
        [Serializable] public sealed class Artifact : Definition {public string Rarity,Mechanic;public List<Effect> Effects=new List<Effect>();public double Effect(string id){foreach(var row in Effects)if(row.Id==id)return row.Value;return 0;}}
        [Serializable]
        public sealed class Synergy : Definition
        {
            public List<string> Requires = new List<string>();

            public override string Description(bool ru) => Id switch
            {
                "void_horizon" => ru
                    ? "Пустотная сфера наносит на 15% больше урона, пока на арене есть живой элитный враг."
                    : "Void deals 15% more damage while an elite enemy is alive in the arena.",
                "gravity_pulse" => ru
                    ? "Импульсная сфера наносит на 15% больше урона, пока рядом с вашими сферами в сумме не меньше трёх врагов."
                    : "Pulse deals 15% more damage while at least three enemies are near your Spheres.",
                "perfect_network" => ru
                    ? "При трёх и более активных типах сфер участники треугольника наносят на 10% больше урона."
                    : "With at least three active Sphere types, Spheres forming a triangle deal 10% more damage.",
                "hunter_doctrine" => ru
                    ? "Снайперская сфера наносит на 12% больше урона, пока на арене есть живой элитный враг или босс."
                    : "Sniper deals 12% more damage while an elite enemy or boss is alive in the arena.",
                "pulse_engineering" => ru
                    ? "Импульсная сфера наносит на 12% больше урона, пока на арене не меньше четырёх врагов."
                    : "Pulse deals 12% more damage while at least four enemies are alive in the arena.",
                "temporal_echo" => ru
                    ? "Сферы наносят на 12% больше урона во время Остановки времени или пока действует бонус к урону от Телепорта."
                    : "Spheres deal 12% more damage during Time Stop or while the Teleport damage bonus is active.",
                "axiom_fold" => ru
                    ? "Сферы наносят на 6% больше урона, если активны хотя бы три разных типа сфер."
                    : "Spheres deal 6% more damage with at least three active Sphere types.",
                "geometry_loop" => ru
                    ? "Сферы наносят на 8% больше урона, пока активно Кольцо, Решётка или Фрактал."
                    : "Spheres deal 8% more damage while Ring, Lattice or Fractal is active.",
                "status_circuit" => ru
                    ? "Призма наносит на 10% больше урона при наличии хотя бы одного общего модификатора сфер."
                    : "Prism deals 10% more damage with at least one global Sphere modifier.",
                "status_loop" => ru
                    ? "Призма наносит на 10% больше урона при наличии хотя бы одного общего модификатора сфер."
                    : "Prism deals 10% more damage with at least one global Sphere modifier.",
                _ => base.Description(ru)
            };
        }
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
