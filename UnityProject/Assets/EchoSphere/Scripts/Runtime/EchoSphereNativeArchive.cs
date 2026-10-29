using System;
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
            _nativeArchiveScreen.Show(_nativeArchiveState,NativeSubmenuScale,()=>_mainMenuPage=MainMenuPage.Home,SelectNativeArchiveSection,SearchNativeArchive);
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
        private NativeArchiveScreen.State ReadNativeArchiveState()
        {
            EnsureKnowledgeLoaded();
            if(_nativeArchiveCatalog==null)_nativeArchiveCatalog=Resources.Load<NativeArchiveCatalog>("EchoSphere/NativeArchiveCatalog");
            if(_nativeArchiveCatalog==null)throw new InvalidOperationException("Native archive catalog missing");
            bool ru=IsRussianUi;string query=_knowledgeQuery.Trim().ToLowerInvariant();var cards=new List<NativeArchiveScreen.Card>();int basic=0;
            foreach(var entry in _nativeArchiveCatalog.Entries){
                if(entry.section!=_knowledgeSection)continue;
                string name=ru?entry.nameRu:entry.nameEn;
                // The original evolution section does not filter by query. Keep
                // that behavior rather than invent a search rule during migration.
                if(_knowledgeSection!="basics"&&_knowledgeSection!="evolutions"&&query.Length>0&&!name.ToLowerInvariant().Contains(query))continue;
                cards.Add(new NativeArchiveScreen.Card {Id=entry.id,Group=entry.group,Title=name,Description=ru?entry.descRu:entry.descEn,Details=ru?entry.detailsRu:entry.detailsEn,Open=_knowledgeSection=="basics"||_knownKnowledgeIds.Contains(entry.id),BasicIndex=_knowledgeSection=="basics"?basic++:-1});
            }
            return new NativeArchiveScreen.State {Russian=ru,Section=_knowledgeSection,Query=_knowledgeQuery,Known=_knownKnowledgeIds.Count,Cards=cards};
        }
    }
}
