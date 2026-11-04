using System;
using System.Linq;
using EchoSphere.Core;
using System.Collections.Generic;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeArchiveCatalog _nativeArchiveCatalog;
        private NativeArchiveScreen _nativeArchiveScreen;
        private NativeArchiveScreen.State _nativeArchiveState;
        private bool _nativeArchiveWasActive;
        private bool TryDrawNativeArchive()
        {
            if(!_mainMenu||_regionSelect||_mainMenuPage!=MainMenuPage.Knowledge){
                if(_nativeArchiveWasActive){_nativeArchiveScreen.Hide();_nativeArchiveWasActive=false;if(_sourceUi!=null&&!UsesNativeMainMenu&&!_nativeProgressionWasActive&&!_nativeSettingsWasActive&&!_nativeRegionWasActive&&!_nativeCharacterWasActive){_sourceUi.Dirty=true;}}
                return false;
            }
            if(!_nativeArchiveWasActive){
                if(_nativeArchiveScreen==null)_nativeArchiveScreen=AcquireNativeScreen<NativeArchiveScreen>("Native Echo Archive UI Toolkit");
                _knowledgeSection="basics";_knowledgeQuery="";_nativeArchiveState=ReadNativeArchiveState();_nativeArchiveWasActive=true;
                _camera.cullingMask=0;
            }
            _nativeArchiveScreen.Show(_nativeArchiveState,NativeSubmenuScale,()=>_mainMenuPage=MainMenuPage.Home,SelectNativeArchiveSection,SearchNativeArchive,ResearchNativeLore);
            return true;
        }
        private void SelectNativeArchiveSection(string section)
        {
            if(!_mainMenu||_mainMenuPage!=MainMenuPage.Knowledge||Array.IndexOf(KnowledgeSections,section)<0)return;
            _knowledgeSection=section;_knowledgeQuery="";_nativeArchiveState=ReadNativeArchiveState();
        }
        private void SearchNativeArchive(string value)
        {
            if(!_mainMenu||_mainMenuPage!=MainMenuPage.Knowledge)return;
            _knowledgeQuery=value;_nativeArchiveState=ReadNativeArchiveState();
        }
        private void BindArchiveArtifactBuilds(NativeArchiveScreen.Card card,bool ru)
        {
            if(!card.Id.StartsWith("artifact:",StringComparison.Ordinal))return;
            string artifactId=card.Id.Substring(9);
            card.Builds=new List<NativeArchiveScreen.ArtifactBuild>();
            void Add(string title,string description,IEnumerable<string> requirements,bool combination=false){
                var ids=requirements.Distinct().ToArray();if(!ids.Contains(artifactId))return;
                var build=new NativeArchiveScreen.ArtifactBuild{Title=NativePlayerCopy.Text(title,ru),Description=NativePlayerCopy.Text(description,ru),Combination=combination};
                foreach(string id in ids){var item=ArtifactRewardRules.Get(id);if(item==null)throw new InvalidOperationException("Unknown archive build artifact: "+id);build.Members.Add(new NativeArchiveScreen.ArtifactMember{Id="artifact:"+id,Name=NativePlayerCopy.Text(ru?item.NameRussian:item.NameEnglish,ru),Selected=id==artifactId});}
                card.Builds.Add(build);
            }
            foreach(var set in _nativeArchiveCatalog.Planner){
                if(set.category=="artifact-set")Add(ru?set.nameRu:set.nameEn,ru?set.descRu:set.descEn,SourceSetArtifacts(set));
                else if(set.category=="artifact-synergy")Add(ru?set.nameRu:set.nameEn,ru?set.descRu:set.descEn,set.requires??Array.Empty<string>(),true);
            }
            foreach(var set in CampaignArtifactSets.All)Add(ru?set.Ru:set.En,ru?set.DescriptionRu:set.DescriptionEn,set.Requires);
        }
        private NativeArchiveScreen.State ReadNativeArchiveState()
        {
            EnsureKnowledgeLoaded();
            if(_nativeArchiveCatalog==null)_nativeArchiveCatalog=Resources.Load<NativeArchiveCatalog>("EchoSphere/NativeArchiveCatalog");
            if(_nativeArchiveCatalog==null)throw new InvalidOperationException("Native archive catalog missing");
            bool ru=IsRussianUi;string query=_knowledgeQuery.Trim().ToLowerInvariant();var cards=new List<NativeArchiveScreen.Card>();int basic=0;
            foreach(var entry in _nativeArchiveCatalog.Entries){
                if(entry.section!=_knowledgeSection||_knowledgeSection=="monsters"||_knowledgeSection=="bosses"||_knowledgeSection=="characters")continue;
                string name=entry.Name(ru);
                if(entry.id.StartsWith("ability:",StringComparison.Ordinal)){var ability=NativeRunCatalog.Instance.GetAbility(entry.id.Substring(8));if(ability!=null)name=ability.Name(ru);}
                // The original evolution section does not filter by query. Keep
                // that behavior rather than invent a search rule during migration.
                if(_knowledgeSection!="basics"&&_knowledgeSection!="evolutions"&&query.Length>0&&!name.ToLowerInvariant().Contains(query))continue;
                cards.Add(new NativeArchiveScreen.Card {Id=entry.id,Group=entry.group,Title=name,Description=NativePlayerCopy.Text(ArtifactGameplayRules.DescriptionOverride(entry.id.StartsWith("artifact:",StringComparison.Ordinal)?entry.id.Substring(9):entry.id,ru)??entry.Description(ru),ru),Details=entry.Details(ru),Open=_knowledgeSection=="basics"||_knownKnowledgeIds.Contains(entry.id),BasicIndex=_knowledgeSection=="basics"?basic++:-1});
            }
            if(_knowledgeSection=="artifacts")foreach(var item in ArtifactRewardRules.Catalog(true)){
                if(ArtifactRewardRules.All.Contains(item))continue;
                string name=NativePlayerCopy.Text(ru?item.NameRussian:item.NameEnglish,ru);
                if(query.Length>0&&!name.ToLowerInvariant().Contains(query))continue;
                string id="artifact:"+item.Id;
                cards.Add(new NativeArchiveScreen.Card{Id=id,Group="",Title=name,Description=NativePlayerCopy.Text(ru?item.DescriptionRussian:item.DescriptionEnglish,ru),Details="",Open=_knownKnowledgeIds.Contains(id),BasicIndex=-1});
            }
            if(_knowledgeSection=="monsters"||_knowledgeSection=="bosses")foreach(var entry in BestiaryCatalog.All){
                if(_knowledgeSection=="bosses"&&!entry.Boss)continue;
                string name=entry.Name(ru);if(query.Length>0&&!name.ToLowerInvariant().Contains(query)&&!CampaignRegions.SourceId(entry.Region).Contains(query))continue;
                _bestiaryAppearances.TryGetValue(entry.Id,out var appearance);appearance??=entry.DefaultAppearance();
                string behavior=entry.Boss?BestiaryCatalog.BossBehavior(entry.Variant,ru):BestiaryCatalog.Behavior((int)entry.Region,appearance.Role,ru)+(entry.Elite?" "+BestiaryCatalog.EliteBehavior(entry.Type,ru):"");
                cards.Add(new NativeArchiveScreen.Card{Id=entry.Id,Group="bestiary-region:"+(int)entry.Region,Title=name,Description=behavior,Details=entry.Lore(ru),Open=_knownKnowledgeIds.Contains(entry.Id),Enemy=appearance,RegionLore=ArchiveLore.Region((int)entry.Region).Ru,RegionLoreEnglish=ArchiveLore.Region((int)entry.Region).En});
            }
            if(_knowledgeSection=="characters")foreach(var character in CharacterRules.All){string id=CharacterRules.ToSourceId(character.Id),name=ru?character.NameRu:character.NameEn;if(query.Length>0&&!name.ToLowerInvariant().Contains(query))continue;cards.Add(new NativeArchiveScreen.Card{Id="character:"+id,Title=name,Description=ru?character.DescriptionRu:character.DescriptionEn,Details=ArchiveLore.Character(id).Ru,DetailsEnglish=ArchiveLore.Character(id).En,Open=_knownKnowledgeIds.Contains("character:"+id),CharacterId=id});}
            if(_knowledgeSection=="regions")foreach(var lore in CampaignRegionLore.All)
            {
                string title=NativeRunCatalog.Instance.GetRegion(RegionSourceId(lore.Region)).Name(ru)+" · "+(lore.Index+1)+"/5";
                string description=NativePlayerCopy.Text(ru?lore.Ru:lore.En,ru);
                if(query.Length>0&&!title.ToLowerInvariant().Contains(query)&&!description.ToLowerInvariant().Contains(query))continue;
                cards.Add(new NativeArchiveScreen.Card{Id=lore.Id,Group=RegionSourceId(lore.Region),Title=title,Description=description,Details="",Open=_knownKnowledgeIds.Contains(lore.Id),CanResearch=IsRegionAvailable(lore.Region)&&GetMetaBalance(CurrencyId.ArchiveData)>=MetaCurrencyRules.LoreResearchCost,ResearchAction=IsRegionAvailable(lore.Region)?(ru?"Исследовать · ":"Research · ")+MetaCurrencyRules.LoreResearchCost+(ru?" ДА":" AD"):"",BasicIndex=-1});
            }
            if(_knowledgeSection=="basics")foreach(CurrencyId id in Enum.GetValues(typeof(CurrencyId))){
                cards.Add(new NativeArchiveScreen.Card{Id="currency:"+id,Group="",Title=MetaCurrencyRules.Name(id,ru),Description=NativePlayerCopy.Text(MetaCurrencyRules.Purpose(id,ru),ru),Details="",Open=true,BasicIndex=-1});
            }
            if(_knowledgeSection=="runes")for(int i=0;i<RuneRules.Count;i++){
                string id=RuneRules.TypeAt(i),name=RuneRules.Name(id,ru),description=NativePlayerCopy.Text(RuneRules.Description(id,ru),ru);
                if(query.Length>0&&!name.ToLowerInvariant().Contains(query)&&!description.ToLowerInvariant().Contains(query))continue;
                cards.Add(new NativeArchiveScreen.Card{Id="rune:"+id,Group="",Title=name,Description=description,Details=ru?"Подберите руну в течение 22 с.":"Pick up the rune within 22 s.",Open=true,BasicIndex=-1});
            }
            if(_knowledgeSection=="artifacts")foreach(var card in cards)BindArchiveArtifactBuilds(card,ru);
            return new NativeArchiveScreen.State {Russian=ru,Section=_knowledgeSection,Query=_knowledgeQuery,Known=_knownKnowledgeIds.Count,WalletSummary=MetaBalanceLabel(CurrencyId.Credits)+" · "+MetaBalanceLabel(CurrencyId.ResonanceShards)+" · "+MetaBalanceLabel(CurrencyId.ArchiveData),Cards=cards};
        }
    }
}
