using System;
using System.Collections.Generic;
namespace EchoSphere.Core {
 [Serializable] public sealed class BestiaryAppearance {
  public string Id,Role,Variant,ColorHex;public int Region;public bool Boss,Elite;public float Radius;
 }
 public sealed class BestiaryEntry {
  public string Id,Type,Role,Variant,NameRu,NameEn,LoreRu,LoreEn;public EchoRegionId Region;public bool Boss,Elite;
  public string Name(bool ru)=>ru?NameRu:NameEn;
  public string Lore(bool ru)=>ru?LoreRu:LoreEn;
  public BestiaryAppearance DefaultAppearance()=>new BestiaryAppearance{Id=Id,Region=(int)Region,Role=Role,Variant=Variant,Boss=Boss,Elite=Elite,Radius=Boss?34:19,ColorHex=Elite?"#b8475a":Boss?BossCombatRules.GetColorHex(Variant):"#4a7a8a"};
 }
 public static class BestiaryCatalog {
  public static readonly string[] Forms={"wisp","leech","serpent","stalker","prism","moth","skitter","beetle","brute"};
  public static readonly string[] EliteIds={"linkbreaker","resonance_leech","phantom_hunter","geometry_shifter","splitter_prime","mirror_warden","stasis_warden","nullifier","pyroclast","scavenger_prime"};
  public static readonly string[] EliteForms={"linkbreaker","leech","stalker","prism","serpent","beetle","wisp","prism","brute","skitter"};
  private static readonly string[] EliteRoles={"disruptor","leech","phase","disruptor","splitter","tank_guard","anchor","disruptor","bomber","scavenger"};
  public static string Key(int region,bool boss,bool elite,string type)=>(boss?"defeat:boss:":elite?"defeat:elite:":"defeat:enemy:")+region+":"+type;
  private static readonly List<BestiaryEntry> Entries=Build();
  public static IReadOnlyList<BestiaryEntry> All=>Entries;
  public static BestiaryEntry Find(string id){foreach(var e in Entries)if(e.Id==id)return e;return null;}
  private static List<BestiaryEntry> Build(){var list=new List<BestiaryEntry>();
   for(int r=0;r<6;r++){
    if(r==0){foreach(string form in Forms)Add(list,r,false,false,form,"grunt",form,ArchiveLore.Form(form));}
    else foreach(string role in RegionPocketRules.AvailableCampaignRoles((EchoRegionId)r))Add(list,r,false,false,role,role,"wisp",ArchiveLore.Role(r,role));
    for(int i=0;i<EliteIds.Length;i++)Add(list,r,false,true,EliteIds[i],EliteRoles[i],EliteForms[i],ArchiveLore.Elite(i));
    for(int i=0;i<CampaignBossRules.Count;i++){string type=CampaignBossRules.TypeAt(i);if(type=="brood_queen"&&r!=2||type=="fleet_carrier"&&r!=1||r==2&&type=="summoner"||r==1&&type=="null")continue;Add(list,r,true,false,type,"grunt",type,ArchiveLore.Boss(type));}
   }return list;
  }
  private static void Add(List<BestiaryEntry> all,int r,bool boss,bool elite,string type,string role,string variant,ArchiveLore.Text text){
   if(boss||elite){
    var region=ArchiveLore.Region(r);
    var fieldNote=r switch {
     0=>("Следы этой особи ведут вдоль опор первой сети.", "Its trail follows the supports of the first Network."),
     1=>("Эту сигнатуру засекли среди обломков флота.", "This signature was picked up among the fleet wreckage."),
     2=>("Первое наблюдение сделано у входа в выводковые галереи.", "It was first observed at the mouth of the brood galleries."),
     3=>("На местных гранях остался характерный след её резонанса.", "Its passage left a distinct resonance in the local crystal faces."),
     4=>("Её присутствие выдают перебои в янтарных потоках.", "Interruptions in the amber streams give away its presence."),
     _=>("Запись восстановлена по отголоскам в глубине архива.", "This record was reconstructed from echoes deep in the Archive.")
    };
    text=new ArchiveLore.Text(text.NameRu+" · "+region.NameRu,text.NameEn+" · "+region.NameEn,fieldNote.Item1+" "+text.Ru,fieldNote.Item2+" "+text.En);
   }
   all.Add(new BestiaryEntry{Id=Key(r,boss,elite,type),Region=(EchoRegionId)r,Boss=boss,Elite=elite,Type=type,Role=role,Variant=variant,NameRu=text.NameRu,NameEn=text.NameEn,LoreRu=text.Ru,LoreEn=text.En});
  }
  public static string Behavior(int region,string role,bool ru){
   var p=RegionalEnemyRules.Get(region,role);string movement=p.Motion==RegionalMotion.Hold?(ru?"Удерживает дистанцию.":"Holds its firing distance."):p.Motion==RegionalMotion.Flank?(ru?"Заходит сбоку.":"Approaches from the flank."):p.Motion==RegionalMotion.Orbit?(ru?"Маневрирует вокруг цели.":"Circles its target."):(ru?"Сближается с ядром.":"Closes on the Core.");
   string attack=p.Shot=="brood"?(ru?"Выпускает выводок.":"Releases a brood."):p.Shot=="shadow"?(ru?"Бьёт по отмеченному месту.":"Strikes a marked position."):p.Shot=="tentacle"?(ru?"Атакует щупальцем вблизи.":"Strikes nearby targets with a tendril."):p.Shot=="web"?(ru?"Выпускает паутину, которая на короткое время останавливает движение ядра.":"Fires webs that briefly stop the Core from moving."):p.Shot=="spore"?(ru?"Выпускает споры.":"Launches spores."):p.Range>0?(ru?"Стреляет после короткой подготовки.":"Fires after a short wind-up."):(ru?"Опасен при соприкосновении.":"Deals damage on contact.");
   if(region==0)movement=ru?"Сближается с ядром.":"Closes on the Core.";
   string support=role=="healer"?(ru?" Восстанавливает здоровье ближайших союзников.":" Heals nearby allies."):role=="disruptor"?(ru?" Прерывает работу ближайшей сферы.":" Disrupts a nearby Sphere."):role=="leech"?(ru?" Восстанавливается рядом с ядром.":" Recovers health near the Core."):role=="scavenger"?(ru?" Ускоряется после тяжёлого ранения.":" Speeds up when badly hurt."):role=="corruptor"?(ru?" При контакте отнимает резонанс.":" Drains Resonance on contact."):"";
   return movement+" "+attack+support;
  }
  public static string BossBehavior(string type,bool ru){var parts=new List<string>();for(int i=0;i<3;i++){var attack=CampaignBossRules.Pattern(type,i,0).Attack;string text;
   switch(attack){case CampaignBossAttack.Fan:text=ru?"Выпускает веер снарядов.":"Fires a fan of projectiles.";break;case CampaignBossAttack.Radial:text=ru?"Выпускает круговой залп.":"Fires a radial volley.";break;case CampaignBossAttack.Cross:text=ru?"Стреляет перекрёстными залпами.":"Fires crossing volleys.";break;case CampaignBossAttack.Dash:text=ru?"Совершает рывок по выбранному направлению.":"Dashes along a chosen direction.";break;case CampaignBossAttack.Pulse:text=ru?"Поражает область вокруг себя.":"Strikes the surrounding area.";break;case CampaignBossAttack.Summon:text=ru?"Выпускает помощников.":"Releases reinforcements.";break;case CampaignBossAttack.Disable:text=ru?"Временно отключает сферы поблизости.":"Temporarily disables nearby Spheres.";break;case CampaignBossAttack.Shift:text=ru?"Меняет позицию относительно ядра.":"Repositions around the Core.";break;default:text=ru?"Подавляет резонанс и задерживает атаки сфер в своей области.":"Suppresses Resonance and delays Sphere attacks within its field.";break;}if(!parts.Contains(text))parts.Add(text);
   }return string.Join(" ",parts)+(ru?" Перед атакой показывает направление или область удара. Раненый босс атакует чаще. При движении босс может раздвигать сферы и нарушать формации.":" Attacks are signalled by aim or ground warnings. A wounded boss attacks more often. A moving boss can push Spheres out of formation.");}
  public static string EliteBehavior(string type,bool ru){switch(type){
   case "linkbreaker":return ru?"Готовит удар, временно отключающий выбранную сферу.":"Prepares a strike that temporarily disables a chosen Sphere.";
   case "resonance_leech":return ru?"Поглощает резонанс рядом с ядром.":"Absorbs Resonance near the Core.";
   case "phantom_hunter":return ru?"Ускоряется, когда ядро находится далеко.":"Speeds up when far from the Core.";
   case "geometry_shifter":return ru?"Периодически отнимает часть резонанса.":"Periodically drains part of your Resonance.";
   case "splitter_prime":return ru?"После гибели оставляет малых противников.":"Leaves smaller enemies behind when defeated.";
   case "mirror_warden":return ru?"Лучше переносит атаки сфер и отражает часть их урона в ядро.":"Resists Sphere hits and reflects part of their damage to the Core.";
   case "stasis_warden":return ru?"Наносит постепенный урон ядру поблизости.":"Deals damage over time to a nearby Core.";
   case "nullifier":return ru?"Периодически отключает ближайшую сферу.":"Periodically disables the nearest Sphere.";
   case "pyroclast":return ru?"Наносит постепенный урон на близком расстоянии.":"Deals damage over time at close range.";
   default:return ru?"Ускоряется, потеряв половину здоровья.":"Speeds up below half health.";
  }}
 }
}
