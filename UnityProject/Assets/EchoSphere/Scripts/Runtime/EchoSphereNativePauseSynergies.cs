using System;
using System.Collections.Generic;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private bool _nativePauseSynergiesPilot=true;
        private NativePauseSynergiesView _nativePauseSynergiesIsland;
        private int _nativePauseSynergiesBoundVersion=-1;
        private bool NativePauseSynergiesRequested=>_nativePauseSynergiesPilot&&!_mainMenu&&!_gameOver&&_userPaused&&_pausePlannerTab==3&&_playerCore!=null&&!_levelUp&&!_stellaRewardPending;
        public bool UsesNativePauseSynergies=>NativePauseSynergiesRequested&&_nativePauseSynergiesIsland!=null&&_nativePauseSynergiesIsland.Ready;
        public void SetNativePauseSynergiesPilot(bool enabled){if(_nativePauseSynergiesPilot==enabled)return;_nativePauseSynergiesPilot=enabled;if(!enabled)DisposeNativePauseSynergiesIsland();if(_sourceUi!=null)_sourceUi.Dirty=true;}
        private void DisposeNativePauseSynergiesIsland(){_nativePauseSynergiesIsland?.Dispose();_nativePauseSynergiesIsland=null;_nativePauseSynergiesBoundVersion=-1;}
        private void PrepareNativePauseSynergiesIsland(NativeRunUiAnchors anchors)
        {
            if(!NativePauseSynergiesRequested){DisposeNativePauseSynergiesIsland();return;}
            var scroll=NativePauseBody(anchors,out var body);
            if(scroll==null){DisposeNativePauseSynergiesIsland();return;}
            var parent=scroll.contentContainer;
            if(_nativePauseSynergiesIsland==null||_nativePauseSynergiesIsland.Root.parent!=parent){DisposeNativePauseSynergiesIsland();_nativePauseSynergiesIsland=new NativePauseSynergiesView(parent);}
            if(_nativePauseSynergiesBoundVersion!=anchors.Version){_nativePauseSynergiesIsland.Bind(ReadNativePauseSynergiesState());_nativePauseSynergiesBoundVersion=anchors.Version;}
            bool ready=_nativePauseSynergiesIsland.Ready;_nativePauseSynergiesIsland.Prepare(SourceDevicePixelRatio);
            if(_nativePauseSynergiesIsland.Ready){body?.HideLegacy(true);_nativePauseSynergiesIsland.UseFlowLayout(true);if(!ready)anchors.Invalidate?.Invoke();}
        }
        public NativePauseSynergiesState ReadNativePauseSynergiesState()=>ProjectNativeLivePauseSynergiesState(IsRussianUi);
        private NativePauseSynergiesState ProjectNativeLivePauseSynergiesState(bool ru)
        {
            string lang = ru ? "ru" : "en";
            var catalog=NativeRunCatalog.Instance;

            var sections=new List<NativePauseSynergiesState.Section>();var cards=new List<NativePauseSynergiesState.Card>();
            foreach(var link in catalog.SphereAbilityLinks){
                bool sphereOk=NativeLiveSphereLevel(link.Sphere)>=7,abilityOk=NativeLiveAbilityLevel(link.Ability)>=7;
                bool selected=NativeLiveSphereBranch(link.Sphere)==link.Branch;
                bool active=sphereOk&&abilityOk&&selected,inProgress=selected&&!active,eligible=!selected&&sphereOk;
                string branch=catalog.GetSphereBranch(link.Branch)?.Name(ru)??link.Branch;
                cards.Add(new NativePauseSynergiesState.Card {
                    Id=link.Id,Name=link.Name(ru),Description=link.Description(ru),
                    Status=active?(ru?"АКТИВНА":"ACTIVE"):inProgress?(ru?"В ПРОЦЕССЕ":"IN PROGRESS"):eligible?(ru?"ДОСТУПНА":"ELIGIBLE"):(ru?"ЗАКРЫТА":"LOCKED"),
                    Tone=active?"purple":selected?"cyan":"base",
                    Hint=catalog.GetSphere(link.Sphere).Name(ru)+" · "+branch+" · VII "+(sphereOk?"✓":"•")+" · "+catalog.GetAbility(link.Ability).Name(ru)+" VII "+(abilityOk?"✓":"•")
                });
            }
            sections.Add(new NativePauseSynergiesState.Section { Id = "sphere", Heading = ru ? "Сфера + ветка + способность" : "Sphere + branch + ability", Cards = cards });
            cards = new List<NativePauseSynergiesState.Card>();
            foreach (var row in NativeLiveBuildDiagnostics(ru))
            {
                string id = row.Id;
                string name = id == "offense" ? (ru ? "Атака" : "Offense") : id == "control" ? (ru ? "Контроль" : "Control") : id == "survival" ? (ru ? "Выживание" : "Survival") : id == "network" ? (ru ? "Сеть" : "Network") : id == "resonance" ? (ru ? "Резонанс" : "Resonance") : (ru ? "Синергия" : "Synergy");
                var tags = new List<NativePauseSynergiesState.Tag>();
                foreach (var signal in row.Signals) tags.Add(new NativePauseSynergiesState.Tag { Id = signal, Text = signal });
                cards.Add(new NativePauseSynergiesState.Card { Id = id, Name = name, Diagnostic = true, Tone = "base", Tags = tags });
            }
            sections.Add(new NativePauseSynergiesState.Section { Id = "diagnostics", Heading = ru ? "Диагностика билда" : "Build Diagnostics", Cards = cards });
            cards = new List<NativePauseSynergiesState.Card>();
            var activeSynergies=new HashSet<string>();foreach(var synergy in NativeOwnedArtifactSynergies(_ownedArtifacts))activeSynergies.Add(synergy.Id);
            foreach(var set in catalog.Sets){int count=0;foreach(var id in set.SynergyIds)if(activeSynergies.Contains(id))count++;bool complete=count==set.SynergyIds.Count;
                cards.Add(new NativePauseSynergiesState.Card {Id=set.Id,Name=complete?set.Name(ru):"???",Description=complete?set.Description(ru):(ru?"Сет ещё не открыт.":"Set not discovered yet."),Status=count+"/"+set.SynergyIds.Count,Tone=complete?"gold":"base",Pulse=complete});
            }
            sections.Add(new NativePauseSynergiesState.Section { Id = "sets", Heading = ru ? "Наборы артефактов" : "Artifact Sets", Cards = cards });
            cards = new List<NativePauseSynergiesState.Card>();
            foreach(var state in NativeLiveProtocols()){
                var protocol=state.Definition;bool active=state.Active,discovered=state.Discovered;
                cards.Add(new NativePauseSynergiesState.Card {Id=protocol.Id,Name=discovered?(ru?protocol.NameRussian:protocol.NameEnglish):"???",Description=discovered?(ru?protocol.DescriptionRussian:protocol.DescriptionEnglish):(ru?"Неизвестный протокол.":"Unknown protocol."),Status=active?(ru?"АКТИВЕН":"ACTIVE"):discovered?(ru?"ОТКРЫТ":"DISCOVERED"):"???",Tone=active?"ice":"base"});
            }
            sections.Add(new NativePauseSynergiesState.Section { Id = "protocols", Heading = ru ? "Протоколы" : "Protocols", Cards = cards });
            cards = new List<NativePauseSynergiesState.Card>();
            var ownedIds=new HashSet<string>(_ownedArtifacts);
            foreach(var synergy in catalog.Synergies){var tags=new List<NativePauseSynergiesState.Tag>();int owned=0;
                foreach(string id in synergy.Requires){bool has=ownedIds.Contains(id);if(has)owned++;tags.Add(new NativePauseSynergiesState.Tag {Id=id,Text=catalog.GetArtifact(id).Name(ru),Owned=has});}
                cards.Add(new NativePauseSynergiesState.Card {Id=synergy.Id,Name=synergy.Name(ru),Description=synergy.Description(ru),Status=owned+"/"+tags.Count,Tone=owned==tags.Count?"purple":"base",Tags=tags});
            }
            sections.Add(new NativePauseSynergiesState.Section { Id = "artifacts", Heading = ru ? "Синергии артефактов" : "Artifact synergies", Cards = cards });
            return new NativePauseSynergiesState { Sections = sections, EmptySignals = ru ? "Признаки пока не сформированы" : "No active signals yet" };
        }
#if UNITY_EDITOR
        internal static NativePauseSynergiesState ProjectNativePauseSynergiesState(V scene, bool ru)
        {
            string lang = ru ? "ru" : "en";
            var catalog=NativeRunCatalog.Instance;

            var sections=new List<NativePauseSynergiesState.Section>();var cards=new List<NativePauseSynergiesState.Card>();
            foreach(var link in catalog.SphereAbilityLinks){
                bool sphereOk=NativeSphereLevel(scene,link.Sphere)>=7,abilityOk=scene["player"]["abilities"][link.Ability].Number>=7;
                bool selected=scene["player"]["sphereBranches"][link.Sphere].Text==link.Branch;
                bool active=sphereOk&&abilityOk&&selected,inProgress=selected&&!active,eligible=!selected&&sphereOk;
                string branch=catalog.GetSphereBranch(link.Branch)?.Name(ru)??link.Branch;
                cards.Add(new NativePauseSynergiesState.Card {
                    Id=link.Id,Name=link.Name(ru),Description=link.Description(ru),
                    Status=active?(ru?"АКТИВНА":"ACTIVE"):inProgress?(ru?"В ПРОЦЕССЕ":"IN PROGRESS"):eligible?(ru?"ДОСТУПНА":"ELIGIBLE"):(ru?"ЗАКРЫТА":"LOCKED"),
                    Tone=active?"purple":selected?"cyan":"base",
                    Hint=catalog.GetSphere(link.Sphere).Name(ru)+" · "+branch+" · VII "+(sphereOk?"✓":"•")+" · "+catalog.GetAbility(link.Ability).Name(ru)+" VII "+(abilityOk?"✓":"•")
                });
            }
            sections.Add(new NativePauseSynergiesState.Section { Id = "sphere", Heading = ru ? "Сфера + ветка + способность" : "Sphere + branch + ability", Cards = cards });
            cards = new List<NativePauseSynergiesState.Card>();
            foreach (var row in NativeBuildDiagnostics(scene,ru))
            {
                string id = row.Id;
                string name = id == "offense" ? (ru ? "Атака" : "Offense") : id == "control" ? (ru ? "Контроль" : "Control") : id == "survival" ? (ru ? "Выживание" : "Survival") : id == "network" ? (ru ? "Сеть" : "Network") : id == "resonance" ? (ru ? "Резонанс" : "Resonance") : (ru ? "Синергия" : "Synergy");
                var tags = new List<NativePauseSynergiesState.Tag>();
                foreach (var signal in row.Signals) tags.Add(new NativePauseSynergiesState.Tag { Id = signal, Text = signal });
                cards.Add(new NativePauseSynergiesState.Card { Id = id, Name = name, Diagnostic = true, Tone = "base", Tags = tags });
            }
            sections.Add(new NativePauseSynergiesState.Section { Id = "diagnostics", Heading = ru ? "Диагностика билда" : "Build Diagnostics", Cards = cards });
            cards = new List<NativePauseSynergiesState.Card>();
            var activeSynergies=new HashSet<string>();foreach(var synergy in NativeArtifactSynergies(scene))activeSynergies.Add(synergy.Id);
            foreach(var set in catalog.Sets){int count=0;foreach(var id in set.SynergyIds)if(activeSynergies.Contains(id))count++;bool complete=count==set.SynergyIds.Count;
                cards.Add(new NativePauseSynergiesState.Card {Id=set.Id,Name=complete?set.Name(ru):"???",Description=complete?set.Description(ru):(ru?"Сет ещё не открыт.":"Set not discovered yet."),Status=count+"/"+set.SynergyIds.Count,Tone=complete?"gold":"base",Pulse=complete});
            }
            sections.Add(new NativePauseSynergiesState.Section { Id = "sets", Heading = ru ? "Наборы артефактов" : "Artifact Sets", Cards = cards });
            cards = new List<NativePauseSynergiesState.Card>();
            foreach(var state in NativeProtocols(scene)){
                var protocol=state.Definition;bool active=state.Active,discovered=state.Discovered;
                cards.Add(new NativePauseSynergiesState.Card {Id=protocol.Id,Name=discovered?(ru?protocol.NameRussian:protocol.NameEnglish):"???",Description=discovered?(ru?protocol.DescriptionRussian:protocol.DescriptionEnglish):(ru?"Неизвестный протокол.":"Unknown protocol."),Status=active?(ru?"АКТИВЕН":"ACTIVE"):discovered?(ru?"ОТКРЫТ":"DISCOVERED"):"???",Tone=active?"ice":"base"});
            }
            sections.Add(new NativePauseSynergiesState.Section { Id = "protocols", Heading = ru ? "Протоколы" : "Protocols", Cards = cards });
            cards = new List<NativePauseSynergiesState.Card>();
            var ownedIds=new HashSet<string>();foreach(var item in scene["player"]["artifacts"].Items)ownedIds.Add(item.Text);
            foreach(var synergy in catalog.Synergies){var tags=new List<NativePauseSynergiesState.Tag>();int owned=0;
                foreach(string id in synergy.Requires){bool has=ownedIds.Contains(id);if(has)owned++;tags.Add(new NativePauseSynergiesState.Tag {Id=id,Text=catalog.GetArtifact(id).Name(ru),Owned=has});}
                cards.Add(new NativePauseSynergiesState.Card {Id=synergy.Id,Name=synergy.Name(ru),Description=synergy.Description(ru),Status=owned+"/"+tags.Count,Tone=owned==tags.Count?"purple":"base",Tags=tags});
            }
            sections.Add(new NativePauseSynergiesState.Section { Id = "artifacts", Heading = ru ? "Синергии артефактов" : "Artifact synergies", Cards = cards });
            return new NativePauseSynergiesState { Sections = sections, EmptySignals = ru ? "Признаки пока не сформированы" : "No active signals yet" };
        }
#endif
    }
}
