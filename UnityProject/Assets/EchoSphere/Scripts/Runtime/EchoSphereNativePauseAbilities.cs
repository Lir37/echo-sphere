using System;
using System.Collections.Generic;
using System.Linq;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private bool _nativePauseAbilitiesPilot=true;
        private NativePauseAbilitiesView _nativePauseAbilitiesIsland;
        private int _nativePauseAbilitiesBoundVersion=-1;
        private bool NativePauseAbilitiesRequested=>_nativePauseAbilitiesPilot&&!_mainMenu&&!_gameOver&&_userPaused&&_pausePlannerTab==1&&_sourceRenderState.Truth&&!_sourceRenderState["pendingUpgrade"].Truth&&!_sourceRenderState["pendingArtifact"].Truth&&!_sourceRenderState["pendingStella"].Truth;
        public bool UsesNativePauseAbilities=>NativePauseAbilitiesRequested&&_nativePauseAbilitiesIsland!=null&&_nativePauseAbilitiesIsland.Ready;
        public void SetNativePauseAbilitiesPilot(bool enabled){if(_nativePauseAbilitiesPilot==enabled)return;_nativePauseAbilitiesPilot=enabled;if(!enabled)DisposeNativePauseAbilitiesIsland();if(_sourceUi!=null)_sourceUi.Dirty=true;}
        private void DisposeNativePauseAbilitiesIsland(){_nativePauseAbilitiesIsland?.Dispose();_nativePauseAbilitiesIsland=null;_nativePauseAbilitiesBoundVersion=-1;}
        private void PrepareNativePauseAbilitiesIsland(NativeRunUiAnchors anchors)
        {
            if(!NativePauseAbilitiesRequested){DisposeNativePauseAbilitiesIsland();return;}
            var scroll=NativePauseBody(anchors,out var body);
            if(scroll==null){DisposeNativePauseAbilitiesIsland();return;}
            var parent=scroll.contentContainer;
            if(_nativePauseAbilitiesIsland==null||_nativePauseAbilitiesIsland.Root.parent!=parent){DisposeNativePauseAbilitiesIsland();_nativePauseAbilitiesIsland=new NativePauseAbilitiesView(parent);}
            if(_nativePauseAbilitiesBoundVersion!=anchors.Version){_nativePauseAbilitiesIsland.Bind(ReadNativePauseAbilitiesState());_nativePauseAbilitiesBoundVersion=anchors.Version;}
            bool ready=_nativePauseAbilitiesIsland.Ready;_nativePauseAbilitiesIsland.Prepare(SourceDevicePixelRatio);
            if(_nativePauseAbilitiesIsland.Ready){body?.HideLegacy(true);_nativePauseAbilitiesIsland.UseFlowLayout(true);if(!ready)anchors.Invalidate?.Invoke();}
        }
        public NativePauseAbilitiesState ReadNativePauseAbilitiesState()
        {
            if(_sourceRenderState.Undefined)throw new InvalidOperationException("Existing render snapshot required");
            return ProjectNativePauseAbilitiesState(_sourceRenderState,IsRussianUi);
        }
        internal static NativePauseAbilitiesState ProjectNativePauseAbilitiesState(V scene,bool ru)
        {
            var active=new List<NativePauseAbilitiesState.Ability>();
            var passive=new List<NativePauseAbilitiesState.Ability>();
            foreach(var definition in NativeRunCatalog.Instance.Abilities){
                string id=definition.Id;var raw=scene["player"]["abilities"][id];var level=raw.Truth?raw:new V(0);
                if(definition.Category=="active"){
                    var branch=NativeAbilityChoice(scene,id,4);var final=NativeAbilityChoice(scene,id,7);
                    string Names(IEnumerable<NativeRunCatalog.Definition> choices)=>string.Join(" · ",choices.Select(choice=>choice.Name(ru)));
                    active.Add(new NativePauseAbilitiesState.Ability {
                        Id=id,Acquired=level.Truth,Level=level.Text+"/7",
                        Name=level.Truth?NativeAbilityName(scene,id,ru):definition.Name(ru),
                        Description=level.Truth?NativeAbilityDescription(scene,id,ru):NativeBaseAbilityDescription(definition,1,ru),
                        HasProgression=definition.HasProgression,
                        Branch=definition.HasProgression?(branch!=null?branch.Name(ru):Names(definition.Branches)):null,
                        Final=definition.HasProgression?(final!=null?final.Name(ru):Names(definition.FallbackFinals)):null
                    });
                }else if(definition.Category=="passive"&&level.Number>0){
                    passive.Add(new NativePauseAbilitiesState.Ability {
                        Id=id,Acquired=true,Level=raw.Text+"/7",Name=definition.Name(ru),
                        Description=NativeBaseAbilityDescription(definition,level.Truth?level.Number:1,ru)
                    });
                }
            }
            return new NativePauseAbilitiesState {ActiveHeading=ru?"Активные способности":"Active abilities",PassiveHeading=ru?"Пассивные":"Passive",Active=active,Passive=passive};
        }
    }
}
