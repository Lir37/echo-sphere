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
        [Serializable] public sealed class Transfer { public string sourceCommit;public List<Entry> entries; }
        public string SourceCommit;
        public List<Entry> Entries=new List<Entry>();
    }
}
