using EchoSphere.Core;
internal static class ArchiveContentChecks {
 internal static int Run(){int n=0;void Check(bool ok,string why){n++;if(!ok)throw new Exception("Archive: "+why);}var ids=new HashSet<string>();
  foreach(var entry in BestiaryCatalog.All){Check(ids.Add(entry.Id),"unique identity "+entry.Id);Check(entry.Id==BestiaryCatalog.Key((int)entry.Region,entry.Boss,entry.Elite,entry.Type),"stable key");Check(entry.Name(true).Length>2&&entry.Name(false).Length>2,"localized names");Check(entry.Lore(true).Length>30&&entry.Lore(false).Length>30,"authored localized lore "+entry.Id);var a=entry.DefaultAppearance();Check(a.Id==entry.Id&&a.Region==(int)entry.Region&&a.Boss==entry.Boss&&a.Elite==entry.Elite,"default appearance identity");var restored=(BestiaryAppearance)RunSnapshotCodec.Restore(RunSnapshotCodec.Capture(a),typeof(BestiaryAppearance));Check(restored.Id==a.Id&&restored.Variant==a.Variant&&restored.Role==a.Role,"appearance codec");}
  foreach(bool ru in new[]{false,true}){
   var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
   var stories=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
   foreach(var entry in BestiaryCatalog.All){
    Check(names.Add(entry.Name(ru)),"unique localized name "+entry.Id);
    Check(stories.Add(entry.Lore(ru)),"unique localized lore "+entry.Id);
   }
  }
  for(int r=0;r<6;r++){foreach(string elite in BestiaryCatalog.EliteIds)Check(ids.Contains(BestiaryCatalog.Key(r,false,true,elite)),"every elite "+r+elite);for(int b=0;b<CampaignBossRules.Count;b++){string type=CampaignBossRules.ForRegion(CampaignBossRules.TypeAt(b),(EchoRegionId)r);int origin=type=="brood_queen"?2:type=="fleet_carrier"?1:r;Check(ids.Contains(BestiaryCatalog.Key(origin,true,false,type)),"every emitted boss "+r+b);}if(r==0){foreach(var form in BestiaryCatalog.Forms)Check(ids.Contains(BestiaryCatalog.Key(r,false,false,form)),"basin anatomy "+form);}else foreach(var role in RegionPocketRules.AvailableCampaignRoles((EchoRegionId)r))Check(ids.Contains(BestiaryCatalog.Key(r,false,false,role)),"all emitted roles "+r+role);Check(ArchiveLore.Region(r).Ru.Length>150&&ArchiveLore.Region(r).En.Length>150,"regional history");}
  foreach(var character in CharacterRules.All){var lore=ArchiveLore.Character(CharacterRules.ToSourceId(character.Id));Check(lore.Ru.Length>100&&lore.En.Length>100,"character story "+character.Id);}
  Console.WriteLine("Archive content: "+n+" checks, "+BestiaryCatalog.All.Count+" bestiary entries PASS");return n;
 }
}