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
        private bool NativePauseSynergiesRequested=>_nativePauseSynergiesPilot&&!_mainMenu&&!_gameOver&&_userPaused&&_pausePlannerTab==3&&_sourceRenderState.Truth&&!_sourceRenderState["pendingUpgrade"].Truth&&!_sourceRenderState["pendingArtifact"].Truth&&!_sourceRenderState["pendingStella"].Truth;
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
        public NativePauseSynergiesState ReadNativePauseSynergiesState()
        {
            if (_sourceRenderState.Undefined) throw new InvalidOperationException("Existing render snapshot required");
            return ProjectNativePauseSynergiesState(_sourceRenderState, IsRussianUi);
        }

        internal static NativePauseSynergiesState ProjectNativePauseSynergiesState(V scene, bool ru)
        {
            string lang = ru ? "ru" : "en";
            var progression = Visual_sphereProgression.Scope;
            var data = Visual_gameData.Scope;
            var artifacts = Visual_artifactSystem.Scope;
            var sections = new List<NativePauseSynergiesState.Section>();
            var cards = new List<NativePauseSynergiesState.Card>();
            foreach (var link in progression["SPHERE_ABILITY_SYNERGIES"].Items)
            {
                bool sphereOk = progression["sphereLevel"].Call(scene, link["sphere"]).Number >= 7;
                bool abilityOk = scene["player"]["abilities"][link["ability"]].Number >= 7;
                bool selected = scene["player"]["sphereBranches"][link["sphere"]] == link["sphereBranch"];
                bool active = sphereOk && abilityOk && selected;
                bool inProgress = selected && !active;
                bool eligible = !selected && sphereOk;
                string branch = link["sphereBranch"].Text;
                foreach (var choice in progression["SPHERE_PROGRESSION"][link["sphere"]]["evolution4Choices"].Items)
                    if (choice["id"] == link["sphereBranch"]) { branch = choice["name"][lang].Text; break; }
                cards.Add(new NativePauseSynergiesState.Card {
                    Id = link["id"].Text, Name = link["name"][lang].Text, Description = link["desc"][lang].Text,
                    Status = active ? (ru ? "АКТИВНА" : "ACTIVE") : inProgress ? (ru ? "В ПРОЦЕССЕ" : "IN PROGRESS") : eligible ? (ru ? "ДОСТУПНА" : "ELIGIBLE") : (ru ? "ЗАКРЫТА" : "LOCKED"),
                    Tone = active ? "purple" : selected ? "cyan" : "base",
                    Hint = data["SPHERE_TYPES"][link["sphere"]]["name"][lang].Text + " · " + branch + " · VII " + (sphereOk ? "✓" : "•") + " · " + data["ABILITIES"][link["ability"]]["name"][lang].Text + " VII " + (abilityOk ? "✓" : "•")
                });
            }
            sections.Add(new NativePauseSynergiesState.Section { Id = "sphere", Heading = ru ? "Сфера + ветка + способность" : "Sphere + branch + ability", Cards = cards });
            cards = new List<NativePauseSynergiesState.Card>();
            foreach (var row in Visual_engineStats.Scope["getBuildDiagnostics"].Call(scene).Items)
            {
                string id = row["id"].Text;
                string name = id == "offense" ? (ru ? "Атака" : "Offense") : id == "control" ? (ru ? "Контроль" : "Control") : id == "survival" ? (ru ? "Выживание" : "Survival") : id == "network" ? (ru ? "Сеть" : "Network") : id == "resonance" ? (ru ? "Резонанс" : "Resonance") : (ru ? "Синергия" : "Synergy");
                var tags = new List<NativePauseSynergiesState.Tag>();
                foreach (var signal in row[lang].Items) tags.Add(new NativePauseSynergiesState.Tag { Id = signal.Text, Text = signal.Text });
                cards.Add(new NativePauseSynergiesState.Card { Id = id, Name = name, Diagnostic = true, Tone = "base", Tags = tags });
            }
            sections.Add(new NativePauseSynergiesState.Section { Id = "diagnostics", Heading = ru ? "Диагностика билда" : "Build Diagnostics", Cards = cards });
            cards = new List<NativePauseSynergiesState.Card>();
            foreach (var set in artifacts["getArtifactSetProgress"].Call(scene).Items)
            {
                bool complete = set["complete"].Truth;
                cards.Add(new NativePauseSynergiesState.Card { Id = set["id"].Text, Name = complete ? set["name"][lang].Text : "???", Description = complete ? set["desc"][lang].Text : (ru ? "Сет ещё не открыт." : "Set not discovered yet."), Status = set["activeSynergies"].Text + "/" + set["totalSynergies"].Text, Tone = complete ? "gold" : "base", Pulse = complete });
            }
            sections.Add(new NativePauseSynergiesState.Section { Id = "sets", Heading = ru ? "Наборы артефактов" : "Artifact Sets", Cards = cards });
            cards = new List<NativePauseSynergiesState.Card>();
            foreach (var protocol in artifacts["getArtifactProtocolStates"].Call(scene).Items)
            {
                bool active = protocol["active"].Truth, discovered = protocol["discovered"].Truth;
                cards.Add(new NativePauseSynergiesState.Card { Id = protocol["id"].Text, Name = discovered ? protocol["name"][lang].Text : "???", Description = discovered ? protocol["desc"][lang].Text : (ru ? "Неизвестный протокол." : "Unknown protocol."), Status = active ? (ru ? "АКТИВЕН" : "ACTIVE") : discovered ? (ru ? "ОТКРЫТ" : "DISCOVERED") : "???", Tone = active ? "ice" : "base" });
            }
            sections.Add(new NativePauseSynergiesState.Section { Id = "protocols", Heading = ru ? "Протоколы" : "Protocols", Cards = cards });
            cards = new List<NativePauseSynergiesState.Card>();
            foreach (var synergy in artifacts["ARTIFACT_SYNERGIES"].Items)
            {
                var tags = new List<NativePauseSynergiesState.Tag>(); int owned = 0;
                foreach (var id in synergy["requires"].Items)
                {
                    bool has = false; foreach (var item in scene["player"]["artifacts"].Items) if (item == id) { has = true; break; }
                    if (has) owned++;
                    tags.Add(new NativePauseSynergiesState.Tag { Id = id.Text, Text = data["ARTIFACT_MAP"][id]["name"][lang].Text, Owned = has });
                }
                cards.Add(new NativePauseSynergiesState.Card { Id = synergy["id"].Text, Name = synergy["name"][lang].Text, Description = synergy["desc"][lang].Text, Status = owned + "/" + tags.Count, Tone = owned == tags.Count ? "purple" : "base", Tags = tags });
            }
            sections.Add(new NativePauseSynergiesState.Section { Id = "artifacts", Heading = ru ? "Синергии артефактов" : "Artifact synergies", Cards = cards });
            return new NativePauseSynergiesState { Sections = sections, EmptySignals = ru ? "Признаки пока не сформированы" : "No active signals yet" };
        }
    }
}
