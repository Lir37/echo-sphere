using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private bool _nativePauseArtifactsPilot;
        private NativePauseArtifactsView _nativePauseArtifactsIsland;
        private int _nativePauseArtifactsBoundVersion=-1;
        private bool NativePauseArtifactsRequested=>_nativePauseArtifactsPilot&&!_mainMenu&&!_gameOver&&_userPaused&&_pausePlannerTab==2&&_sourceRenderState.Truth&&!_sourceRenderState["pendingUpgrade"].Truth&&!_sourceRenderState["pendingArtifact"].Truth&&!_sourceRenderState["pendingStella"].Truth;
        public bool UsesNativePauseArtifacts=>NativePauseArtifactsRequested&&_nativePauseArtifactsIsland!=null&&_nativePauseArtifactsIsland.Ready;
        public void SetNativePauseArtifactsPilot(bool enabled){if(_nativePauseArtifactsPilot==enabled)return;_nativePauseArtifactsPilot=enabled;if(!enabled)DisposeNativePauseArtifactsIsland();if(_sourceUi!=null)_sourceUi.Dirty=true;}
        private void DisposeNativePauseArtifactsIsland(){_nativePauseArtifactsIsland?.Dispose();_nativePauseArtifactsIsland=null;_nativePauseArtifactsBoundVersion=-1;}
        private void PrepareNativePauseArtifactsIsland(VisualCssNode root)
        {
            if(!NativePauseArtifactsRequested){DisposeNativePauseArtifactsIsland();return;}
            VisualCssNode body=null;void Find(VisualCssNode node,bool pause){pause|=node.HasClass("es-pause-overlay");if(body==null&&pause&&node.HasClass("overflow-y-auto")&&node.HasClass("px-4")&&node.HasClass("py-4")&&node.HasClass("min-h-0")){body=node;return;}foreach(var child in node.Children)Find(child,pause);}
            Find(root,false);if(!(body?.Native is SourceUiFrame frame)||!(frame.Content is ScrollView scroll)){DisposeNativePauseArtifactsIsland();return;}
            var parent=scroll.contentContainer;
            if(_nativePauseArtifactsIsland==null||_nativePauseArtifactsIsland.Root.parent!=parent){DisposeNativePauseArtifactsIsland();_nativePauseArtifactsIsland=new NativePauseArtifactsView(parent,id=>{_pauseArtifactSetId=id;if(_sourceUi!=null)_sourceUi.Dirty=true;});}
            if(_nativePauseArtifactsBoundVersion!=_sourceUi.FrameVersion){_nativePauseArtifactsIsland.Bind(ReadNativePauseArtifactsState());_nativePauseArtifactsBoundVersion=_sourceUi.FrameVersion;}
            bool ready=_nativePauseArtifactsIsland.Ready;_nativePauseArtifactsIsland.Prepare(SourceDevicePixelRatio);
            if(_nativePauseArtifactsIsland.Ready){foreach(var child in body.Children)if(child.Native is VisualElement element)element.style.display=DisplayStyle.None;_nativePauseArtifactsIsland.UseFlowLayout(true);if(!ready)_sourceUi.Dirty=true;}
        }
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
