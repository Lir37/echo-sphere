using System;
using System.Collections.Generic;
using UnityEngine;
namespace EchoSphere.Presentation {
 internal static class ArchiveEntryLore {
  [Serializable] internal sealed class Entry {public string Id="",Ru="",En="";}
  [Serializable] internal sealed class Collection {public List<Entry> Entries=new List<Entry>();}
  private static Dictionary<string,Entry> _entries;
  internal static string Get(string id,bool ru){
   if(_entries==null){_entries=new Dictionary<string,Entry>(StringComparer.Ordinal);var asset=Resources.Load<TextAsset>("EchoSphere/ArchiveEntryLore");if(asset!=null){var data=JsonUtility.FromJson<Collection>(asset.text);if(data?.Entries!=null)foreach(var row in data.Entries)if(!string.IsNullOrEmpty(row.Id))_entries[row.Id]=row;}}
   return id!=null&&_entries.TryGetValue(id,out var entry)?(ru?entry.Ru:entry.En):null;
  }
 }
}