using System.Collections.Generic;
namespace EchoSphere.Core {
 public sealed class CampaignArtifactSet {
  public string Id,CounterId,Ru,En,DescriptionRu,DescriptionEn,Effect;public int Cadence;public string[] Requires;
  public CampaignArtifactSet(string id,string ru,string en,string[] requires,int cadence,string effect,string dr,string de){Id=id;CounterId="set:"+id;Ru=ru;En=en;Requires=requires;Cadence=cadence;Effect=effect;DescriptionRu=dr;DescriptionEn=de;}
  public bool Complete(IList<string> owned){if(owned==null)return false;foreach(var id in Requires)if(!owned.Contains(id))return false;return true;}
 }
 public static class CampaignArtifactSets { public static readonly CampaignArtifactSet[] All={
new CampaignArtifactSet("glass_bastion","Стеклянный бастион","Glass Bastion",new[]{"glass_capacitor","overclock","void_contract"},30,"shield","При HP ниже 50%: каждые 30 попаданий дают заряд щита.","Below 50% HP: every 30 hits grants a shield charge."),
new CampaignArtifactSet("kinetic_relay","Кинетическое реле","Kinetic Relay",new[]{"kinetic_brace","crystal_speed","swift_boots"},12,"advance","Каждые 12 попаданий ускоряют следующую атаку атакующей сферы на 0,2 с.","Every 12 hits advances the attacking Sphere’s next attack by 0.2s."),
new CampaignArtifactSet("frozen_optics","Холодная оптика","Frozen Optics",new[]{"dense_lens","long_lens","sniper_scope"},8,"freeze","Каждые 8 попаданий Снайперской замораживают цель на 0,25 с.","Every 8 Sniper hits freezes the target for 0.25s."),
new CampaignArtifactSet("wave_rotor","Волновой ротор","Wave Rotor",new[]{"rapid_coil","pulse_driver","pulse_crown"},8,"knockback","Каждые 8 попаданий Импульсной отталкивают цель.","Every 8 Pulse hits knocks the target back."),
new CampaignArtifactSet("living_fortress","Живая крепость","Living Fortress",new[]{"living_alloy","amulet_hp","regen_stone"},20,"heal","Каждые 20 попаданий восстанавливают до 3 HP в пределах общего лимита боевого лечения.","Every 20 hits restores up to 3 HP within the shared combat healing limit."),
new CampaignArtifactSet("chronicle","Хроника","Chronicle",new[]{"ritual_clock","mage_pendant","time_splitter"},10,"resonance","Во время Остановки времени или пока действует бонус к урону от Телепорта каждые 10 попаданий дают 5 заряда Резонанса.","During Time Stop or while the Teleport damage bonus is active, every 10 hits grants 5 Resonance charge."),
new CampaignArtifactSet("blood_cycle","Кровавый цикл","Blood Cycle",new[]{"blood_crystal","vampire_ring","predator_claw"},15,"heal","Каждые 15 попаданий восстанавливают до 3 HP в пределах общего лимита боевого лечения.","Every 15 hits restores up to 3 HP within the shared combat healing limit."),
new CampaignArtifactSet("unstable_echo","Нестабильное эхо","Unstable Echo",new[]{"unstable_sigil","crit_sigil","luck_talisman"},12,"strike","Каждые 12 попаданий наносят дополнительный удар на 15% урона попадания.","Every 12 hits adds a strike for 15% of hit damage.")
 };}
}
