using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EchoSphere.Presentation
{
    [CreateAssetMenu(menuName="ECHO SPHERE/Localization catalog")]
    public sealed class NativeLocalizationCatalog : ScriptableObject
    {
        [Serializable] public sealed class Translation {public string Key,Ru,En;}
        [Serializable] public sealed class Replacement {public string Pattern,Text;}
        public string SourceCommit;
        public List<Translation> Translations=new List<Translation>();
        public List<Replacement> CanvasPhrases=new List<Replacement>(),CanvasPatterns=new List<Replacement>();
        [NonSerialized] private Dictionary<string,Translation> _translations;
        [NonSerialized] private List<Regex> _patterns;
        private static NativeLocalizationCatalog _instance;
        public static NativeLocalizationCatalog Instance
        {get{if(_instance==null)_instance=Resources.Load<NativeLocalizationCatalog>("EchoSphere/NativeLocalizationCatalog");if(_instance==null)throw new InvalidOperationException("Native localization catalog missing");return _instance;}}
        public string Translate(string key,bool russian)
        {
            if(_translations==null){_translations=new Dictionary<string,Translation>(StringComparer.Ordinal);foreach(var row in Translations)_translations.Add(row.Key,row);}
            return _translations.TryGetValue(key,out var entry)?russian?entry.Ru:entry.En:"undefined";
        }
        public string CanvasText(string text,bool russian)
        {
            if(!russian)return text;
            string result=text;foreach(var row in CanvasPhrases)result=result.Replace(row.Pattern,row.Text);
            if(_patterns==null){_patterns=new List<Regex>();foreach(var row in CanvasPatterns)_patterns.Add(new Regex(row.Pattern,RegexOptions.CultureInvariant));}
            for(int i=0;i<_patterns.Count;i++)result=_patterns[i].Replace(result,CanvasPatterns[i].Text);
            return result;
        }
    }
}
