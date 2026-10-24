using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    [CreateAssetMenu(menuName="ECHO SPHERE/Archive catalog")]
    public sealed class NativeArchiveCatalog : ScriptableObject
    {
        [Serializable] public sealed class Entry
        {
            public string section,id,group,nameRu,nameEn,descRu,descEn,detailsRu,detailsEn;
        }
        [Serializable] public sealed class PlannerEntry
        {
            public string category,id,nameRu,nameEn,descRu,descEn;
            public string sphere,sphereBranch,ability,abilityFinal,color;
            public string[] levelsRu,levelsEn,requires;
        }
        [Serializable] public sealed class Transfer { public string sourceCommit,sourceDigest;public List<Entry> entries;public List<PlannerEntry> planner; }
        public string SourceCommit,SourceDigest;
        public List<Entry> Entries=new List<Entry>();
        public List<PlannerEntry> Planner=new List<PlannerEntry>();
    }
}
