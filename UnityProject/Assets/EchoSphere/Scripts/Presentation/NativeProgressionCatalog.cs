using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Presentation
{
    [CreateAssetMenu(menuName="ECHO SPHERE/Progression catalog")]
    public sealed class NativeProgressionCatalog : ScriptableObject
    {
        [Serializable] public sealed class Achievement
        {
            public string Id,NameRu,NameEn,DescriptionRu,DescriptionEn;
        }
        public string SourceCommit;
        public List<Achievement> Achievements=new List<Achievement>();
    }
}
