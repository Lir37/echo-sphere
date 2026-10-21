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
        private void PrepareNativePauseAbilitiesIsland(VisualCssNode root)
        {
            if(!NativePauseAbilitiesRequested){DisposeNativePauseAbilitiesIsland();return;}
            var scroll=NativePauseBody(root,out var body);
            if(scroll==null){DisposeNativePauseAbilitiesIsland();return;}
            var parent=scroll.contentContainer;
            if(_nativePauseAbilitiesIsland==null||_nativePauseAbilitiesIsland.Root.parent!=parent){DisposeNativePauseAbilitiesIsland();_nativePauseAbilitiesIsland=new NativePauseAbilitiesView(parent);}
            if(_nativePauseAbilitiesBoundVersion!=_sourceUi.FrameVersion){_nativePauseAbilitiesIsland.Bind(ReadNativePauseAbilitiesState());_nativePauseAbilitiesBoundVersion=_sourceUi.FrameVersion;}
            bool ready=_nativePauseAbilitiesIsland.Ready;_nativePauseAbilitiesIsland.Prepare(SourceDevicePixelRatio);
            if(_nativePauseAbilitiesIsland.Ready){foreach(var child in body?.Children ?? System.Linq.Enumerable.Empty<VisualCssNode>())if(child.Native is VisualElement element)element.style.display=DisplayStyle.None;_nativePauseAbilitiesIsland.UseFlowLayout(true);if(!ready)_sourceUi.Dirty=true;}
        }
        public NativePauseAbilitiesState ReadNativePauseAbilitiesState()
        {
            if(_sourceRenderState.Undefined)throw new InvalidOperationException("Existing render snapshot required");
            return ProjectNativePauseAbilitiesState(_sourceRenderState,IsRussianUi);
        }
        internal static NativePauseAbilitiesState ProjectNativePauseAbilitiesState(V scene,bool ru)
        {
            var lang=ru?"ru":"en";
            var definitions=Visual_gameData.Scope["ABILITIES"];
            var active=new List<NativePauseAbilitiesState.Ability>();
            var passive=new List<NativePauseAbilitiesState.Ability>();
            foreach(var id in VisualJs.ObjectObject["keys"].Call(definitions).Items){
                var definition=definitions[id];var raw=scene["player"]["abilities"][id];var level=raw.Truth?raw:new V(0);
                if(definition["category"].Text=="active"){
                    var progression=Visual_sphereProgression.Scope["ABILITY_PROGRESSION"][id];
                    var branch=Visual_sphereProgression.Scope["getAbilityEvolutionChoice"].Call(scene,id,new V(4));
                    var final=Visual_sphereProgression.Scope["getAbilityEvolutionChoice"].Call(scene,id,new V(7));
                    string Names(V choices)=>string.Join(" · ",choices.Items.Select(choice=>choice["name"][lang].Text));
                    active.Add(new NativePauseAbilitiesState.Ability {
                        Id=id.Text,Acquired=level.Truth,Level=level.Text+"/7",
                        Name=level.Truth?Visual_sphereProgression.Scope["getAbilityDisplayName"].Call(scene,id,new V(lang)).Text:definition["name"][lang].Text,
                        Description=level.Truth?Visual_sphereProgression.Scope["getAbilityDisplayDesc"].Call(scene,id,new V(lang)).Text:definition["desc"][lang].Call(new V(1)).Text,
                        HasProgression=progression.Truth,
                        Branch=progression.Truth?(branch.Truth?branch["name"][lang].Text:Names(progression["evolution4"])):null,
                        Final=progression.Truth?(final.Truth?final["name"][lang].Text:Names(progression["evolution7"])):null
                    });
                }else if(definition["category"].Text=="passive"&&level.Number>0){
                    passive.Add(new NativePauseAbilitiesState.Ability {
                        Id=id.Text,Acquired=true,Level=raw.Text+"/7",
                        Name=Visual_gameData.Scope["abilityName"].Call(id,new V(lang)).Text,
                        Description=definition["desc"][lang].Call(level.Truth?level:new V(1)).Text
                    });
                }
            }
            return new NativePauseAbilitiesState {ActiveHeading=ru?"Активные способности":"Active abilities",PassiveHeading=ru?"Пассивные":"Passive",Active=active,Passive=passive};
        }
    }
}
