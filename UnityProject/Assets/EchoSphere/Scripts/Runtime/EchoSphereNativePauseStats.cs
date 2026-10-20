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
        private bool _nativePauseStatsPilot;
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
        private V ProjectNativePauseStatsIsland(V tree)
        {
            if(!UsesNativePauseStats)return tree;
            void Visit(V value,bool pause){
                if(value.Raw is VisualDom.Node node){var classes=(node.Properties["className"].Text??"").Split(' ');pause|=Array.IndexOf(classes,"es-pause-overlay")>=0;
                    if(pause&&Array.IndexOf(classes,"overflow-y-auto")>=0&&Array.IndexOf(classes,"px-4")>=0&&Array.IndexOf(classes,"py-4")>=0&&Array.IndexOf(classes,"min-h-0")>=0){node.Children.Clear();return;}
                    foreach(var child in node.Children)Visit(child,pause);
                }else if(value.Raw is List<V>)foreach(var child in value.Items)Visit(child,pause);
            }
            Visit(tree,false);return tree;
        }
        private void PrepareNativePauseStatsIsland(VisualCssNode root)
        {
            if(!NativePauseStatsRequested){DisposeNativePauseStatsIsland();return;}
            VisualCssNode body=null;
            void Find(VisualCssNode node,bool pause){pause|=node.HasClass("es-pause-overlay");if(body==null&&pause&&node.HasClass("overflow-y-auto")&&node.HasClass("px-4")&&node.HasClass("py-4")&&node.HasClass("min-h-0")){body=node;return;}foreach(var child in node.Children)Find(child,pause);}
            Find(root,false);
            if(!(body?.Native is SourceUiFrame frame)||!(frame.Content is ScrollView scroll)){DisposeNativePauseStatsIsland();return;}
            var parent=scroll.contentContainer;
            if(_nativePauseStatsIsland==null||_nativePauseStatsIsland.Root.parent!=parent){DisposeNativePauseStatsIsland();_nativePauseStatsIsland=new NativePauseStatsView(parent);}
            if(_nativePauseStatsBoundVersion!=_sourceUi.FrameVersion){_nativePauseStatsIsland.Bind(ReadNativePauseStatsState());_nativePauseStatsBoundVersion=_sourceUi.FrameVersion;}
            bool ready=_nativePauseStatsIsland.Ready;_nativePauseStatsIsland.Prepare(SourceDevicePixelRatio,Screen.width/SourceDevicePixelRatio);
            if(_nativePauseStatsIsland.Ready){
                // Warm up absolutely over the old content, without doubling the
                // scroll range. Native flow takes ownership only when ready.
                foreach(var child in body.Children)if(child.Native is VisualElement element)element.style.display=DisplayStyle.None;
                _nativePauseStatsIsland.UseFlowLayout(true);if(!ready)_sourceUi.Dirty=true;
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
            string T(string key)=>Visual_i18n.Scope["translations"][lang][key].Text;
            string Round(V value)=>VisualJs.MathObject["round"].Call(value).Text;
            string Ceil(V value)=>VisualJs.MathObject["ceil"].Call(value).Text;
            string Percent(V value)=>Round(value*new V(100))+"%";
            var character=Visual_characters.Scope["CHARACTER_DEFS"][player["characterId"]];
            var result=new NativePauseStatsState {
                Russian=ru,Kicker=ru?"Билд забега":"Run Build",
                Title=T("pauseTitle")+" · "+character["name"][lang].Text,
                Subtitle=character["role"][lang].Text+" · "+(ru?"уровень":"level")+" "+player["level"].Text+" · "+T("wave")+" "+scene["wave"].Text,
                CharacterHeading=ru?"Персонаж":"Character",SpheresHeading=ru?"Сферы":"Spheres",Resume=T("resume"),Return=T("return"),
                Stats=new[] {
                    new NativePauseStatsState.Stat("HP",Ceil(player["hp"])+" / "+Ceil(player["maxHp"])),
                    new NativePauseStatsState.Stat(ru?"Скорость":"Speed",Round(Visual_engineStats.Scope["getMoveSpeed"].Call(scene))),
                    new NativePauseStatsState.Stat(ru?"Крит":"Crit",Percent(Visual_engineCombat.Scope["getCritChance"].Call(scene))),
                    new NativePauseStatsState.Stat(ru?"Уклонение":"Dodge",Percent(Visual_engineCombat.Scope["getDodgeChance"].Call(scene))),
                    new NativePauseStatsState.Stat(ru?"Вампиризм":"Lifesteal",Percent(Visual_engineCombat.Scope["getVampirePercent"].Call(scene))),
                    new NativePauseStatsState.Stat(ru?"Сфер":"Spheres",scene["spheres"]["length"].Text+" / "+Visual_engineSpheres.Scope["getMaxSpheres"].Call(scene).Text),
                    new NativePauseStatsState.Stat(ru?"Убийства":"Kills",player["kills"].Text),
                    new NativePauseStatsState.Stat(ru?"Множитель XP":"XP Mult",Percent(Visual_App.Scope["getXpPlannerMult"].Call(scene)))
                }
            };
            var cards=new List<NativePauseStatsState.Sphere>();
            foreach(var type in VisualJs.ObjectObject["keys"].Call(Visual_gameData.Scope["SPHERE_TYPES"]).Items){
                var definition=Visual_gameData.Scope["SPHERE_TYPES"][type];
                var placed=scene["spheres"].Items.Where(sphere=>sphere["type"]==type&&sphere["alive"].Truth).ToList();
                var live=placed.Count>0?placed[0]:VisualJs.Null;
                var branch=player["sphereBranches"][type];V branchChoice=VisualJs.Null,finalChoice=VisualJs.Null;
                if(branch.Truth){
                    branchChoice=Visual_sphereProgression.Scope["SPHERE_PROGRESSION"][type]["evolution4Choices"].Items.FirstOrDefault(item=>item["id"]==branch);
                    if(branchChoice.Truth){
                        // Preserve the source presentation lookup, including its
                        // empty-string Number conversion. Do not invent a level
                        // gate or change actual evolution state in a UI reader.
                        var prefix="sphere:"+type.Text+":7:";
                        var evolution=player["evolutions"].Items.FirstOrDefault(item=>(item.Text??"").StartsWith(prefix,StringComparison.Ordinal));
                        var index=VisualJs.Number(new V((evolution.Truth?evolution.Text:"").Split(':').Last()));
                        finalChoice=branchChoice["final"][index];
                    }
                }
                var card=new NativePauseStatsState.Sphere {
                    Id=type.Text,Color=definition["color"].Text,Name=definition["name"][lang].Text,
                    Level=(ru?"ур. ":"lvl ")+Visual_sphereProgression.Scope["sphereLevel"].Call(scene,type).Text,
                    OnField=live.Truth,Placement=placed.Count>0?(ru?"на поле ×":"field ×")+placed.Count:(ru?"не установлена":"not placed"),
                    Description=live.Truth?null:definition["desc"][lang].Text,
                    HasBranch=branch.Truth,BranchLabel=ru?"Ветка:":"Branch:",BranchName=branchChoice.Truth?branchChoice["name"][lang].Text:"",
                    FinalName=finalChoice.Truth?finalChoice["name"][lang].Text:null
                };
                if(live.Truth)card.Metrics=new[] {
                    new NativePauseStatsState.Stat(ru?"урон":"damage",Round(Visual_engineSpheres.Scope["getSphereDamage"].Call(scene,live))+(live["type"].Text=="aura"?"/имп.":"")),
                    new NativePauseStatsState.Stat(ru?"интервал":"delay",Visual_engineSpheres.Scope["getSphereDelay"].Call(scene,live)["toFixed"].Call(new V(2)).Text+"с"),
                    new NativePauseStatsState.Stat(ru?"дальность":"range",Round(Visual_engineSpheres.Scope["getSphereRadius"].Call(scene,live))),
                    new NativePauseStatsState.Stat("DPS",Visual_engineSpheres.Scope["getSphereDpsEstimate"].Call(scene,live)["toFixed"].Call(new V(1)).Text)
                };
                cards.Add(card);
            }
            result.Spheres=cards;return result;
        }
    }
}
