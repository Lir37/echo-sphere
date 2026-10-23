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
        private bool _nativePauseStatsPilot=true;
        private NativePauseStatsView _nativePauseStatsIsland;
        private int _nativePauseStatsBoundVersion=-1;
        private bool NativePauseStatsRequested=>_nativePauseStatsPilot&&!_mainMenu&&!_gameOver&&_userPaused&&_pausePlannerTab==0&&_sourceRenderState.Truth&&!_sourceRenderState["pendingUpgrade"].Truth&&!_sourceRenderState["pendingArtifact"].Truth&&!_sourceRenderState["pendingStella"].Truth;
        public bool UsesNativePauseStats=>NativePauseStatsRequested&&_nativePauseStatsIsland!=null&&_nativePauseStatsIsland.Ready;
        public void SetNativePauseStatsPilot(bool enabled)
        {
            if(_nativePauseStatsPilot==enabled)return;_nativePauseStatsPilot=enabled;
            if(!enabled)DisposeNativePauseStatsIsland();if(_sourceUi!=null)_sourceUi.Dirty=true;
        }
        private void DisposeNativePauseStatsIsland(){_nativePauseStatsIsland?.Dispose();_nativePauseStatsIsland=null;_nativePauseStatsBoundVersion=-1;}
        private void PrepareNativePauseStatsIsland(NativeRunUiAnchors anchors)
        {
            if(!NativePauseStatsRequested){DisposeNativePauseStatsIsland();return;}
            var scroll=NativePauseBody(anchors,out var body);
            if(scroll==null){DisposeNativePauseStatsIsland();return;}
            var parent=scroll.contentContainer;
            if(_nativePauseStatsIsland==null||_nativePauseStatsIsland.Root.parent!=parent){DisposeNativePauseStatsIsland();_nativePauseStatsIsland=new NativePauseStatsView(parent);}
            if(_nativePauseStatsBoundVersion!=anchors.Version){_nativePauseStatsIsland.Bind(ReadNativePauseStatsState());_nativePauseStatsBoundVersion=anchors.Version;}
            bool ready=_nativePauseStatsIsland.Ready;_nativePauseStatsIsland.Prepare(SourceDevicePixelRatio,Screen.width/SourceDevicePixelRatio);
            if(_nativePauseStatsIsland.Ready){
                // Warm up absolutely over the old content, without doubling the
                // scroll range. Native flow takes ownership only when ready.
                body?.HideLegacy(true);
                _nativePauseStatsIsland.UseFlowLayout(true);if(!ready)anchors.Invalidate?.Invoke();
            }
        }
        // Projection only, not selected by a production pause view yet. Use the
        // same existing UI/Core outputs as App.tsx, not the older IMGUI formulas.
        public NativePauseStatsState ReadNativePauseStatsState()
        {
            if(_sourceRenderState.Undefined)throw new InvalidOperationException("Existing render snapshot required");
            return ProjectNativePauseStatsState(_sourceRenderState,IsRussianUi);
        }
        internal static NativePauseStatsState ProjectNativePauseStatsState(V scene,bool ru)
        {
            var lang=ru?"ru":"en";var player=scene["player"];
            string T(string key)=>NativeText(key,lang=="ru");
            string Round(V value)=>VisualJs.NumberText(Math.Floor(value.Number+.5d));
            string Ceil(V value)=>VisualJs.NumberText(Math.Ceiling(value.Number));
            string Percent(V value)=>Round(value*new V(100))+"%";
            EchoSphere.Core.CharacterRules.TryParseSourceId(player["characterId"].Text,out var characterId);var character=EchoSphere.Core.CharacterRules.Get(characterId);
            var result=new NativePauseStatsState {
                Russian=ru,Kicker=ru?"Билд забега":"Run Build",
                Title=T("pauseTitle")+" · "+(ru?character.NameRu:character.NameEn),
                Subtitle=(ru?character.RoleRu:character.RoleEn)+" · "+(ru?"уровень":"level")+" "+player["level"].Text+" · "+T("wave")+" "+scene["wave"].Text,
                CharacterHeading=ru?"Персонаж":"Character",SpheresHeading=ru?"Сферы":"Spheres",Resume=T("resume"),Return=T("return"),
                Stats=new[] {
                    new NativePauseStatsState.Stat("HP",Ceil(player["hp"])+" / "+Ceil(player["maxHp"])),
                    new NativePauseStatsState.Stat(ru?"Скорость":"Speed",Round(new V(NativePlannerMoveSpeed(scene)))),
                    new NativePauseStatsState.Stat(ru?"Крит":"Crit",Percent(new V(NativePlannerCritChance(scene)))),
                    new NativePauseStatsState.Stat(ru?"Уклонение":"Dodge",Percent(new V(NativePlannerDodgeChance(scene)))),
                    new NativePauseStatsState.Stat(ru?"Вампиризм":"Lifesteal",Percent(new V(NativePlannerVampire(scene)))),
                    new NativePauseStatsState.Stat(ru?"Сфер":"Spheres",scene["spheres"]["length"].Text+" / "+VisualJs.NumberText(NativeMaximumSphereCount(scene))),
                    new NativePauseStatsState.Stat(ru?"Убийства":"Kills",player["kills"].Text),
                    new NativePauseStatsState.Stat(ru?"Множитель XP":"XP Mult",Percent(new V(NativeXpPlannerMultiplier(scene))))
                }
            };
            var cards=new List<NativePauseStatsState.Sphere>();
            foreach(var definition in NativeRunCatalog.Instance.Spheres){
                var type=new V(definition.Id);
                var placed=scene["spheres"].Items.Where(sphere=>sphere["type"]==type&&sphere["alive"].Truth).ToList();
                var live=placed.Count>0?placed[0]:VisualJs.Null;
                var branch=player["sphereBranches"][type];NativeRunCatalog.Choice branchChoice=null;NativeRunCatalog.Definition finalChoice=null;
                if(branch.Truth){
                    branchChoice=definition.Branches.FirstOrDefault(item=>item.Id==branch.Text);
                    if(branchChoice!=null){
                        // Preserve the source presentation lookup, including its
                        // empty-string Number conversion. Do not invent a level
                        // gate or change actual evolution state in a UI reader.
                        var prefix="sphere:"+type.Text+":7:";
                        var evolution=player["evolutions"].Items.FirstOrDefault(item=>(item.Text??"").StartsWith(prefix,StringComparison.Ordinal));
                        var index=VisualJs.Number(new V((evolution.Truth?evolution.Text:"").Split(':').Last()));
                        double finalIndex=index.Number;if(finalIndex==Math.Truncate(finalIndex)&&finalIndex>=0&&finalIndex<branchChoice.Finals.Count)finalChoice=branchChoice.Finals[(int)finalIndex];
                    }
                }
                var card=new NativePauseStatsState.Sphere {
                    Id=type.Text,Color=definition.Color,Name=definition.Name(ru),
                    Level=(ru?"ур. ":"lvl ")+VisualJs.NumberText(NativeSphereLevel(scene,type.Text)),
                    OnField=live.Truth,Placement=placed.Count>0?(ru?"на поле ×":"field ×")+placed.Count:(ru?"не установлена":"not placed"),
                    Description=live.Truth?null:definition.Description(ru),
                    HasBranch=branch.Truth,BranchLabel=ru?"Ветка:":"Branch:",BranchName=branchChoice!=null?branchChoice.Name(ru):"",
                    FinalName=finalChoice!=null?finalChoice.Name(ru):null
                };
                if(live.Truth)card.Metrics=new[] {
                    new NativePauseStatsState.Stat(ru?"урон":"damage",Round(new V(NativePlannerSphereDamage(scene,live)))+(live["type"].Text=="aura"?"/имп.":"")),
                    new NativePauseStatsState.Stat(ru?"интервал":"delay",NativePlannerSphereDelay(scene,live).ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+"с"),
                    new NativePauseStatsState.Stat(ru?"дальность":"range",Round(new V(NativePlannerSphereRadius(scene,live)))),
                    new NativePauseStatsState.Stat("DPS",NativePlannerSphereDps(scene,live).ToString("F1",System.Globalization.CultureInfo.InvariantCulture))
                };
                cards.Add(card);
            }
            result.Spheres=cards;return result;
        }
    }
}
