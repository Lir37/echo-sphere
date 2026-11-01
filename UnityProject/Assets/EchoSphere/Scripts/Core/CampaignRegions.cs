using System;
using System.Collections.Generic;
namespace EchoSphere.Core {
 public sealed class CampaignRegionPocket {
  public readonly string Id,Ru,En;public readonly float X,Y;public readonly int Ecology;
  public CampaignRegionPocket(string id,string ru,string en,float x,float y,int ecology){Id=id;Ru=ru;En=en;X=x;Y=y;Ecology=ecology;}
 }
 public sealed class CampaignRegionProfile {
  public readonly EchoRegionId Region;public readonly string Id,Ru,En,Accent,DescriptionRu,DescriptionEn,FirstBoss,SecondBoss,FinalBoss;public readonly CampaignRegionPocket[] Pockets;
  public CampaignRegionProfile(EchoRegionId region,string id,string ru,string en,string accent,string dr,string de,string[] namesRu,string[] namesEn,string first,string second,string final){Region=region;Id=id;Ru=ru;En=en;Accent=accent;DescriptionRu=dr;DescriptionEn=de;FirstBoss=first;SecondBoss=second;FinalBoss=final;float[][] xs={new float[]{0,540,450,-510,-500},new float[]{0,500,0,-500,0},new float[]{0,460,520,-410,-540},new float[]{0,510,390,-540,-310}};float[][] ys={new float[]{0,-280,460,390,-340},new float[]{0,0,460,0,-460},new float[]{0,-390,350,460,-210},new float[]{0,-180,430,260,-440}};var x=xs[(int)region-2];var y=ys[(int)region-2];Pockets=new CampaignRegionPocket[5];for(int i=0;i<5;i++)Pockets[i]=new CampaignRegionPocket(id+"_"+i,namesRu[i],namesEn[i],x[i],y[i],i);}
 }
 public static class CampaignRegions {
  private static readonly CampaignRegionProfile[] Profiles={
   new CampaignRegionProfile(EchoRegionId.FracturedBastion,"fractured_bastion","РАСКОЛОТЫЙ БАСТИОН","FRACTURED BASTION","#b9cf69","Панцирные насекомые, роевые выводки и осадные особи.","Armored insects, brood swarms and siege creatures.",new[]{"ГНЕЗДО","ВЫВОДОК","ПАНЦИРЬ","ТОННЕЛЬ","УЛЕЙ"},new[]{"NEST","BROOD","CARAPACE","TUNNEL","HIVE"},"summoner","charger","architect"),
   new CampaignRegionProfile(EchoRegionId.GravityFolds,"gravity_folds","ГРАВИТАЦИОННЫЕ СКЛАДКИ","GRAVITY FOLDS","#7fd9ee","Кристаллические существа, якоря и линии дальнего огня.","Crystal creatures, anchors and long-range fire lanes.",new[]{"СЕРДЦЕВИНА","ГРАНИ","ЛИНЗА","ИЗЛОМ","КОЛОДЕЦ"},new[]{"CORE","FACETS","LENS","FAULT","WELL"},"architect","aura","null"),
   new CampaignRegionProfile(EchoRegionId.ResonanceStorm,"resonance_storm","РЕЗОНАНСНЫЙ ШТОРМ","RESONANCE STORM","#ffd36d","Энергетические сущности, разряды и прерыватели сети.","Energy entities, discharges and Network disruptors.",new[]{"ОКО","ПОТОК","ДУГА","ГРОМ","ФРОНТ"},new[]{"EYE","CURRENT","ARC","THUNDER","FRONT"},"shooter","conductor","aura"),
   new CampaignRegionProfile(EchoRegionId.NullArchive,"null_archive","НУЛЕВОЙ АРХИВ","NULL ARCHIVE","#bc87e8","Гибриды Пустоты, похитители ресурсов и фазовые угрозы.","Void hybrids, resource scavengers and phase threats.",new[]{"ПОРОГ","ПАМЯТЬ","ШРАМ","ТИШИНА","СЕРДЦЕ"},new[]{"THRESHOLD","MEMORY","SCAR","SILENCE","HEART"},"null","summoner","stella_warden")
  };
  public static IReadOnlyList<CampaignRegionProfile> Additional=>Profiles;
  public static CampaignRegionProfile Get(EchoRegionId region){int i=(int)region-2;if(i<0||i>=Profiles.Length)throw new ArgumentOutOfRangeException(nameof(region));return Profiles[i];}
  public static string SourceId(EchoRegionId region)=>region==EchoRegionId.ResonanceBasin?"resonance_basin":region==EchoRegionId.SpectralRift?"spectral_rift":Get(region).Id;
  public static bool TryParse(string id,out EchoRegionId region){for(int i=0;i<6;i++){var candidate=(EchoRegionId)i;if(SourceId(candidate)==id){region=candidate;return true;}}region=EchoRegionId.ResonanceBasin;return false;}
  public static bool IsUnlocked(EchoRegionId region,Func<EchoRegionId,bool> stable){int i=(int)region;return i==0||i>0&&i<6&&stable!=null&&stable((EchoRegionId)(i-1));}
  public static CampaignRegionPocket Nearest(EchoRegionId region,float worldX,float worldY){var pockets=Get(region).Pockets;var nearest=pockets[0];float best=float.PositiveInfinity;foreach(var p in pockets){float dx=p.X*SpherePlacementRules.SourcePixelsToWorld-worldX,dy=p.Y*SpherePlacementRules.SourcePixelsToWorld-worldY,d=dx*dx+dy*dy;if(d<best){best=d;nearest=p;}}return nearest;}
  private static readonly string[][][] EcologyRoles={
   new[]{new[]{"swarmer","splitter","charger","tank_guard"},new[]{"swarmer","swarmer","splitter","leech"},new[]{"tank_guard","anchor","healer","ranged"},new[]{"charger","phase","disruptor","swarmer"},new[]{"splitter","healer","bomber","tank_guard"}},
   new[]{new[]{"tank_guard","anchor","sniper","healer"},new[]{"ranged","sniper","tank_guard","phase"},new[]{"sniper","ranged","sniper","anchor"},new[]{"anchor","disruptor","tank_guard","corruptor"},new[]{"healer","tank_guard","anchor","bomber"}},
   new[]{new[]{"phase","disruptor","ranged","bomber"},new[]{"swarmer","phase","charger","ranged"},new[]{"ranged","sniper","disruptor","phase"},new[]{"bomber","bomber","disruptor","charger"},new[]{"corruptor","healer","phase","leech"}},
   new[]{new[]{"phase","leech","corruptor","scavenger"},new[]{"swarmer","leech","phase","splitter"},new[]{"anchor","tank_guard","healer","leech"},new[]{"phase","sniper","corruptor","disruptor"},new[]{"corruptor","leech","bomber","scavenger"}}
  };
  public static string SelectRole(EchoRegionId region,float x,float y,float roll,float time){var profile=Get(region);int faction=(int)region-2;var pocket=Nearest(region,x,y);float r=float.IsNaN(roll)||float.IsInfinity(roll)?.5f:Math.Max(0,Math.Min(.999999f,roll));int slot=(int)(r*4);if(time>=900&&slot==0)return faction==0?"tank_guard":faction==1?"anchor":faction==2?"bomber":"corruptor";if(time>=1500&&slot==3)return faction==0?"healer":faction==1?"phase":faction==2?"sniper":"leech";return EcologyRoles[faction][pocket.Ecology][slot];}
 }
}
