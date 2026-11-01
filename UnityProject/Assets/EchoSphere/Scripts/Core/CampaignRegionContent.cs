using System;
using System.Collections.Generic;
namespace EchoSphere.Core
{
 [Flags] public enum RegionContractRule { None=0, Fracture=1, LowGravity=2, Capacity=4, Silent=8, Mixed=16 }
 public sealed class RegionContractDefinition
 {
  public readonly string Id,Ru,En,DescriptionRu,DescriptionEn;public readonly RegionContractRule Rule;
  public RegionContractDefinition(string id,string ru,string en,string dr,string de,RegionContractRule rule=RegionContractRule.None){Id=id;Ru=ru;En=en;DescriptionRu=dr;DescriptionEn=de;Rule=rule;}
 }
 [Serializable] public sealed class CampaignRegionProgress
 {
  public int GeometryMask,PeakSpheres,LevelSevenFamilies,PoiMask;public float DominantSeconds;
 }
 public static class CampaignRegionContent
 {
  public static readonly string[] PoiTypes={"resonance_cache","breach_node","echo_relay","lost_signal","elite_nest","rupture","boss_trace"};
  private static readonly RegionContractDefinition[][] Contracts={
   new[]{new RegionContractDefinition("fractured_network","РАЗЛОМ СЕТИ","FRACTURED NETWORK","Каждые 45 с случайная сфера отключается на 2,2 с.","Every 45 s a random sphere is disabled for 2.2 s.",RegionContractRule.Fracture),new RegionContractDefinition("overload","ПЕРЕГРУЗКА","OVERLOAD","Враги: HP +18%, скорость +12%, урон +8%; XP +15%.","Enemies: HP +18%, speed +12%, damage +8%; XP +15%."),new RegionContractDefinition("low_gravity","НИЗКАЯ ГРАВИТАЦИЯ","LOW GRAVITY","Скорость ядра +12%; отталкивание × 0,55.","Core speed +12%; knockback × 0.55.",RegionContractRule.LowGravity)},
   new[]{new RegionContractDefinition("mirror_tide","ЗЕРКАЛЬНЫЙ ПРИЛИВ","MIRROR TIDE","Каждые 36 с меняется ритм врагов.","Enemy rhythm alternates every 36 s."),new RegionContractDefinition("volatile_core","НЕСТАБИЛЬНОЕ ЯДРО","VOLATILE CORE","Заряд событий +12%; от 70 резонанса: +28% заряда, враги сильнее.","Event charge +12%; at 70 Resonance: +28% charge, stronger enemies."),new RegionContractDefinition("phase_drift","ФАЗОВЫЙ ДРЕЙФ","PHASE DRIFT","Скорость ядра +10%; отталкивание × 0,62.","Core speed +10%; knockback × 0.62.")},
   new[]{new RegionContractDefinition("broken_carapace","СЛОМАННЫЙ ПАНЦИРЬ","BROKEN CARAPACE","Каждые 45 с сфера отключается на 2,2 с.","Every 45 s a sphere is disabled for 2.2 s.",RegionContractRule.Fracture),new RegionContractDefinition("brood_pressure","ДАВЛЕНИЕ ВЫВОДКА","BROOD PRESSURE","Часть волн заменена группами поддержки и осады.","Some spawns become support and siege combinations.",RegionContractRule.Mixed),new RegionContractDefinition("tight_hive","ТЕСНЫЙ УЛЕЙ","TIGHT HIVE","Не более четырёх установленных сфер.","At most four placed spheres.",RegionContractRule.Capacity)},
   new[]{new RegionContractDefinition("low_orbit","НИЗКАЯ ОРБИТА","LOW ORBIT","Скорость ядра +12%; отталкивание× 0,55.","Core speed +12%; knockback× 0.55.",RegionContractRule.LowGravity),new RegionContractDefinition("crystal_prison","КРИСТАЛЬНАЯ КЛЕТКА","CRYSTAL PRISON","Не более четырёх установленных сфер.","At most four placed spheres.",RegionContractRule.Capacity),new RegionContractDefinition("unstable_facets","НЕСТАБИЛЬНЫЕ ГРАНИ","UNSTABLE FACETS","Каждые 45 с сфера отключается на 2,2 с.","Every 45 s a sphere is disabled for 2.2 s.",RegionContractRule.Fracture)},
   new[]{new RegionContractDefinition("silent_core","ТИХОЕ ЯДРО","SILENT CORE","Заряд событий × 0,65; POI и руны без изменений.","Event charge × 0.65; POIs and runes unchanged.",RegionContractRule.Silent),new RegionContractDefinition("overload_front","ФРОНТ ПЕРЕГРУЗКИ","OVERLOAD FRONT","В волнах больше сочетаний прерывания и дальнего огня.","More disruption and ranged support combinations.",RegionContractRule.Mixed),new RegionContractDefinition("severed_chorus","ОБОРВАННЫЙ ХОР","SEVERED CHORUS","Каждые 45 с сфера отключается на 2,2 с.","Every 45 s a sphere is disabled for 2.2 s.",RegionContractRule.Fracture)},
   new[]{new RegionContractDefinition("sealed_archive","ЗАПЕЧАТАННЫЙ АРХИВ","SEALED ARCHIVE","Не более четырёх установленных сфер.","At most four placed spheres.",RegionContractRule.Capacity),new RegionContractDefinition("dark_resonance","ТЁМНЫЙ РЕЗОНАНС","DARK RESONANCE","Заряд событий × 0,65; POI и руны без изменений.","Event charge × 0.65; POIs and runes unchanged.",RegionContractRule.Silent),new RegionContractDefinition("mixed_memory","СМЕШАННАЯ ПАМЯТЬ","MIXED MEMORY","Волны смешивают поддержку, контроль и фазовые угрозы.","Waves mix support, control and phase threats.",RegionContractRule.Mixed)}
  };
  public static IReadOnlyList<RegionContractDefinition> GetContracts(EchoRegionId region)=>Contracts[(int)region];
  public static RegionContractDefinition Find(EchoRegionId region,string id){var list=GetContracts(region);for(int i=0;i<list.Count;i++)if(list[i].Id==id)return list[i];return null;}
  public static bool Has(EchoRegionId region,string id,RegionContractRule rule)=>Find(region,id) is RegionContractDefinition c&&(c.Rule&rule)!=0;
  public static int PoiIndex(string type)=>Array.IndexOf(PoiTypes,type);
  public static int Bits(int mask){int n=0;for(uint v=(uint)mask;v!=0;v&=v-1)n++;return n;}
  // Objectives are assessed on clear, so shrinking the network at the last second cannot earn Small Network.
  public static int MasteryMask(EchoRegionId region,CampaignRegionProgress p)
  {
   if(p==null)return 0;int i=(int)region,mask=0;
   bool first=i==1||i==2?Bits(p.PoiMask)>=5:Bits(p.GeometryMask)>=(i==3||i==5?4:3);
   bool second=i==1?p.LevelSevenFamilies>=2:i==5?Bits(p.PoiMask)>=7:p.DominantSeconds>=(i==2||i==3?90:i==4?120:60);
   bool third=i==1?Bits(p.GeometryMask)>=3:i==0||i==2?p.PeakSpheres>0&&p.PeakSpheres<=4:p.LevelSevenFamilies>=(i==5?3:2);
   if(first)mask|=1;if(second)mask|=2;if(third)mask|=4;return mask;
  }
  public static string[] MasteryDescriptions(EchoRegionId region,bool ru)
  {
   int i=(int)region;
   string first=i==1||i==2?(ru?"Посетить 5 типов POI":"Visit 5 POI types"):(ru?"Активировать геометрий: ":"Activate Geometry types: ")+(i==3||i==5?4:3);
   string second=i==1?(ru?"Две семьи сфер уровня VII":"Two sphere families at VII"):i==5?(ru?"Посетить все 7 типов POI":"Visit all 7 POI types"):(ru?"Активная доминанта, секунд: ":"Active Dominant Geometry, seconds: ")+(i==2||i==3?90:i==4?120:60);
   string third=i==1?(ru?"Активировать геометрий: 3":"Activate Geometry types: 3"):i==0||i==2?(ru?"Завершить, ни разу не превысив 4 сферы":"Clear without ever exceeding 4 spheres"):(ru?"Семей сфер уровня VII: ":"Sphere families at VII: ")+(i==5?3:2);
   return new[]{first,second,third};
  }
  private static readonly string[][] Support={new[]{"healer","disruptor","tank_guard","ranged"},new[]{"sniper","phase","healer","bomber"},new[]{"healer","splitter","tank_guard","charger"},new[]{"anchor","sniper","healer","disruptor"},new[]{"disruptor","bomber","ranged","corruptor"},new[]{"leech","corruptor","phase","healer"}};
  public static string SelectSupport(EchoRegionId region,float roll,float time){int offset=(int)(Math.Max(0,time)/240)%4;int slot=Math.Min(3,(int)(Math.Max(0,Math.Min(.999999f,roll))*4));return Support[(int)region][(slot+offset)%4];}
 }
}
