using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private bool _nativePauseArtifactsPilot=true;
        private NativePauseArtifactsView _nativePauseArtifactsIsland;
        private int _nativePauseArtifactsBoundVersion=-1;
        private bool NativePauseArtifactsRequested=>_nativePauseArtifactsPilot&&!_mainMenu&&!_gameOver&&_userPaused&&_pausePlannerTab==2&&_playerCore!=null&&!_levelUp&&!_stellaRewardPending;
        public bool UsesNativePauseArtifacts=>NativePauseArtifactsRequested&&_nativePauseArtifactsIsland!=null&&_nativePauseArtifactsIsland.Ready;
        public void SetNativePauseArtifactsPilot(bool enabled){if(_nativePauseArtifactsPilot==enabled)return;_nativePauseArtifactsPilot=enabled;if(!enabled)DisposeNativePauseArtifactsIsland();if(_sourceUi!=null)_sourceUi.Dirty=true;}
        private void DisposeNativePauseArtifactsIsland(){_nativePauseArtifactsIsland?.Dispose();_nativePauseArtifactsIsland=null;_nativePauseArtifactsBoundVersion=-1;}
        private void PrepareNativePauseArtifactsIsland(NativeRunUiAnchors anchors)
        {
            if(!NativePauseArtifactsRequested){DisposeNativePauseArtifactsIsland();return;}
            var scroll=NativePauseBody(anchors,out var body);
            if(scroll==null){DisposeNativePauseArtifactsIsland();return;}
            var parent=scroll.contentContainer;
            if(_nativePauseArtifactsIsland==null||_nativePauseArtifactsIsland.Root.parent!=parent){DisposeNativePauseArtifactsIsland();_nativePauseArtifactsIsland=new NativePauseArtifactsView(parent,id=>{_pauseArtifactSetId=id;if(_sourceUi!=null)anchors.Invalidate?.Invoke();});}
            if(_nativePauseArtifactsBoundVersion!=anchors.Version){_nativePauseArtifactsIsland.Bind(ReadNativePauseArtifactsState());_nativePauseArtifactsBoundVersion=anchors.Version;}
            bool ready=_nativePauseArtifactsIsland.Ready;_nativePauseArtifactsIsland.Prepare(SourceDevicePixelRatio);
            if(_nativePauseArtifactsIsland.Ready){body?.HideLegacy(true);_nativePauseArtifactsIsland.UseFlowLayout(true);if(!ready)anchors.Invalidate?.Invoke();}
        }
        public NativePauseArtifactsState ReadNativePauseArtifactsState()=>ProjectNativeArtifactList(_ownedArtifacts,IsRussianUi,_ownedArtifacts);
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
        internal static NativePauseArtifactsState ProjectNativePauseArtifactsState(V scene,bool ru)=>ProjectNativeArtifacts(scene,ru,scene["player"]["artifacts"].Items);
        private static NativePauseArtifactsState ProjectNativeArtifacts(V scene,bool ru,IEnumerable<V> ids)=>ProjectNativeArtifactList(scene["player"]["artifacts"].Items.Select(item=>item.Text).ToArray(),ru,ids.Select(item=>item.Text));
#endif
        private static NativePauseArtifactsState ProjectNativeArtifactList(IReadOnlyCollection<string> owned,bool ru,IEnumerable<string> ids)
        {
            var catalog=NativeRunCatalog.Instance;var artifacts=new List<NativePauseArtifactsState.Artifact>();
            var ownedIds=new HashSet<string>(owned,StringComparer.Ordinal);
            foreach(var id in ids){
                var definition=catalog.GetArtifact(id);var rarity=definition.Rarity;
                var sets=new List<NativePauseArtifactsState.Set>();
                foreach(var set in catalog.Sets.Where(set=>set.ArtifactIds.Contains(id))){
                    int ownedCount=set.ArtifactIds.Count(ownedIds.Contains),total=set.ArtifactIds.Count;bool complete=ownedCount==total;
                    sets.Add(new NativePauseArtifactsState.Set {Id=set.Id,Name=set.Name(ru),Owned=ownedCount,Total=total,Count=ownedCount+"/"+total,Complete=complete,
                        Status=(ru?"СЕТ":"SET")+" · "+(complete?(ru?"СОБРАН":"COMPLETE"):(ru?"ПРОГРЕСС":"PROGRESS"))});
                }
                // Exact presentation-only ArtifactGlyph mapping from App.tsx.
                // Keep glyph selection separate from RNG and gameplay identity.
                var key=Regex.Replace(id,"[^a-z0-9]","",RegexOptions.IgnoreCase);int sum=0;foreach(char ch in key)sum+=ch;
                string[] glyphs={"◇","✦","◈","△","⬡","✧","⊙","✺","⌬","✹","◌","⟡"};
                string glyphColor=rarity=="legendary"?"#ffb84d":rarity=="special"?"#ff6b6b":rarity=="epic"?"#a98cff":rarity=="rare"?"#39d8ff":"#7f9bb8";
                string border=rarity=="legendary"?"#ffb84d":rarity=="special"?"#ff6b6b":rarity=="epic"?"#8064a8":rarity=="rare"?"#39d8ff":"#243b55";
                artifacts.Add(new NativePauseArtifactsState.Artifact {Id=id,Name=definition.Name(ru),Description=definition.Description(ru),Rarity=rarity,
                    RarityLabel=catalog.RarityLabel(rarity,ru),BorderColor=border,GlyphColor=glyphColor,Glyph=glyphs[sum%glyphs.Length],Sets=sets});
            }
            return new NativePauseArtifactsState {Heading=(ru?"Собрано: ":"Owned: ")+NativePresentationMath.NumberText(owned.Count),Empty=ru?"Артефактов пока нет.":"No artifacts yet.",Artifacts=artifacts};
        }
    }
}
