using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EchoSphere.Presentation;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        public NativePauseArtifactsState ReadNativePauseArtifactsState()
        {
            if(_sourceRenderState.Undefined)throw new InvalidOperationException("Existing render snapshot required");
            return ProjectNativePauseArtifactsState(_sourceRenderState,IsRussianUi);
        }
        internal static NativePauseArtifactsState ProjectNativePauseArtifactsState(V scene,bool ru)
        {
            var lang=ru?"ru":"en";var artifacts=new List<NativePauseArtifactsState.Artifact>();
            foreach(var id in scene["player"]["artifacts"].Items){
                var rarity=Visual_artifactSystem.Scope["artifactRarity"].Call(id).Text;
                var definition=Visual_gameData.Scope["ARTIFACT_MAP"][id];
                var sets=new List<NativePauseArtifactsState.Set>();
                foreach(var set in Visual_artifactSystem.Scope["getArtifactSetsForArtifact"].Call(scene,id).Items){
                    bool complete=set["complete"].Truth;var owned=(int)set["owned"].Number;var total=(int)set["total"].Number;
                    sets.Add(new NativePauseArtifactsState.Set {Id=set["setId"].Text,Name=set["name"][lang].Text,Owned=owned,Total=total,Count=owned+"/"+total,Complete=complete,
                        Status=(ru?"СЕТ":"SET")+" · "+(complete?(ru?"СОБРАН":"COMPLETE"):(ru?"ПРОГРЕСС":"PROGRESS"))});
                }
                // Exact presentation-only ArtifactGlyph mapping from App.tsx.
                // Keep glyph selection separate from RNG and gameplay identity.
                var key=Regex.Replace(id.Text,"[^a-z0-9]","",RegexOptions.IgnoreCase);int sum=0;foreach(char ch in key)sum+=ch;
                string[] glyphs={"◇","✦","◈","△","⬡","✧","⊙","✺","⌬","✹","◌","⟡"};
                string glyphColor=rarity=="legendary"?"#ffb84d":rarity=="special"?"#ff6b6b":rarity=="epic"?"#a98cff":rarity=="rare"?"#39d8ff":"#7f9bb8";
                string border=rarity=="legendary"?"#ffb84d":rarity=="special"?"#ff6b6b":rarity=="epic"?"#8064a8":rarity=="rare"?"#39d8ff":"#243b55";
                artifacts.Add(new NativePauseArtifactsState.Artifact {Id=id.Text,Name=definition["name"][lang].Text,Description=definition["desc"][lang].Text,Rarity=rarity,
                    RarityLabel=Visual_artifactSystem.Scope["RARITY_LABELS"][rarity][lang].Text,BorderColor=border,GlyphColor=glyphColor,Glyph=glyphs[sum%glyphs.Length],Sets=sets});
            }
            return new NativePauseArtifactsState {Heading=(ru?"Собрано: ":"Owned: ")+scene["player"]["artifacts"]["length"].Text,Empty=ru?"Артефактов пока нет.":"No artifacts yet.",Artifacts=artifacts};
        }
    }
}
