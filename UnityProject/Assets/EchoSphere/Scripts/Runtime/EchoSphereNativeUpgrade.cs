using System;
using System.Collections.Generic;
using System.Text;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeUpgradeView _nativeUpgradeView;
        private int _nativeUpgradeBoundVersion=-1;
        private bool NativeUpgradeRequested=>!_mainMenu&&!_gameOver&&_sourceRenderState["pendingUpgrade"].Truth;
        private void DisposeNativeUpgrade(){_nativeUpgradeView?.Dispose();_nativeUpgradeView=null;_nativeUpgradeBoundVersion=-1;}
        private void PrepareNativeUpgrade(NativeRunUiAnchors anchors)
        {
            if(!NativeUpgradeRequested){DisposeNativeUpgrade();return;}
            var slot=anchors.Upgrade;var host=slot.Content;if(host==null){DisposeNativeUpgrade();return;}
            if(_nativeUpgradeView==null||_nativeUpgradeView.Root.parent!=host){DisposeNativeUpgrade();_nativeUpgradeView=new NativeUpgradeView(host,anchors.Backdrops,PickNativeUpgrade,LockNativeUpgrade,RerollNativeUpgrades);}
            if(_nativeUpgradeBoundVersion!=anchors.Version){_nativeUpgradeView.Bind(ReadNativeUpgradeState());_nativeUpgradeBoundVersion=anchors.Version;}
            bool ready=_nativeUpgradeView.Ready;_nativeUpgradeView.Prepare(SourceDevicePixelRatio,Screen.width/SourceDevicePixelRatio);
            if(_nativeUpgradeView.Ready){slot.HideLegacy();if(!ready)anchors.Invalidate?.Invoke();}
        }

        private void PickNativeUpgrade(int index){if(NativeUpgradeRequested){if(_choosingEvolution)ChooseEvolution(index);else ChooseUpgrade(index);ApplySourceUiWrites();}if(_sourceUi!=null)_sourceUi.Dirty=true;}
        private void LockNativeUpgrade(int index){if(NativeUpgradeRequested&&!_choosingEvolution){LockLevelUpChoice(index);ApplySourceUiWrites();}if(_sourceUi!=null)_sourceUi.Dirty=true;}
        private void RerollNativeUpgrades(){if(NativeUpgradeRequested&&!_choosingEvolution){RerollLevelUpChoices();ApplySourceUiWrites();}if(_sourceUi!=null)_sourceUi.Dirty=true;}
        private NativeUpgradeView.State ReadNativeUpgradeState()
        {
            bool ru=IsRussianUi;string lang=ru?"ru":"en";string L(string a,string b)=>ru?a:b;string T(string key)=>Visual_i18n.Scope["translations"][lang][key].Text;
            var pending=_sourceRenderState["pendingUpgrade"];var first=pending[0];
            string title=first["type"].Text=="modifier"?L("Новый модификатор","New Modifier"):first["abilityStage"].Text=="branch"?L("Эволюция способности I","Ability Evolution I"):first["abilityStage"].Text=="final"?L("Финальная форма способности","Final Ability Form"):first["sphereStage"].Text=="branch"?L("Мутация сферы I","Sphere Mutation I"):first["sphereStage"].Text=="final"?L("Мутация сферы II","Sphere Mutation II"):T("chooseUpgrade");
            var choices=new List<NativeUpgradeView.Choice>();bool routine=true;string locked=_sourceRenderState["levelUpLockChoiceKey"].Truth?_sourceRenderState["levelUpLockChoiceKey"].Text:"";
            int locks=(int)_sourceRenderState["levelUpLocksRemaining"].Number,rerolls=(int)_sourceRenderState["levelUpRerollsRemaining"].Number;
            var signature=new StringBuilder(lang).Append('|').Append(title).Append('|').Append(locked).Append('|').Append(locks).Append('|').Append(rerolls);
            foreach(var value in pending.Items){
                string type=value["type"].Text,abilityStage=value["abilityStage"].Text,sphereStage=value["sphereStage"].Text;
                bool ordinary=type=="sphere"&&sphereStage=="upgrade"||type=="ability"&&!value["abilityStage"].Truth;routine&=ordinary;
                string key=Visual_engineProgression.Scope["getUpgradeChoiceKey"].Call(value).Text;
                bool forecast=_sphereCharacterId==EchoSphere.Core.SphereCharacterId.Oracle&&_oracleForecastKeys.Contains(key);
                string category=type=="modifier"?L("МОДИФИКАТОР","MODIFIER"):abilityStage=="branch"?L("ВЕТКА СПОСОБНОСТИ","ABILITY BRANCH"):abilityStage=="final"?L("ФИНАЛЬНАЯ ФОРМА СПОСОБНОСТИ","FINAL ABILITY FORM"):sphereStage=="branch"?L("ВЕТКА СФЕРЫ","SPHERE BRANCH"):sphereStage=="final"?L("ФИНАЛЬНАЯ СПЕЦИАЛИЗАЦИЯ","FINAL SPECIALIZATION"):type=="ability"?L("АКТИВНАЯ СПОСОБНОСТЬ","ACTIVE ABILITY"):L("УЛУЧШЕНИЕ СФЕРЫ","SPHERE UPGRADE");
                string level=type=="modifier"?L("Разблокирует эффект","Unlocks the effect"):sphereStage=="branch"||abilityStage=="branch"?L("Уровень IV • выбор ветки","Level IV • choose a branch"):sphereStage=="final"||abilityStage=="final"?L("Уровень VII • выбор финальной формы","Level VII • choose final form"):T("level")+" "+value["currentLevel"].Text+" → "+value["newLevel"].Text;
                var element=type=="sphere"?Visual_sphereProgression.Scope["getSphereElementForBranch"].Call(value["sphereBranch"]):VisualJs.Null;
                var meta=element.Truth?Visual_sphereProgression.Scope["SPHERE_ELEMENT_META"][element]:VisualJs.Null;
                bool elementCard=meta.Truth&&(sphereStage=="branch"||sphereStage=="final");
                var hintNames=new List<string>();var hintDescriptions=new List<string>();
                if(type=="sphere"&&value["sphereBranch"].Truth){var hints=Visual_sphereProgression.Scope["getSphereMutationSynergyHints"].Call(_sourceRenderState,value["sphereType"].Truth?value["sphereType"]:new V("standard"),value["sphereBranch"]);foreach(var hint in hints.Items){hintNames.Add(hint["name"][lang].Text+" · "+Visual_gameData.Scope["ABILITIES"][hint["ability"]]["name"][lang].Text);hintDescriptions.Add(hint["desc"][lang].Text);}}
                var choice=new NativeUpgradeView.Choice {Index=(int)value["nativeIndex"].Number,Key=key,Category=category,Name=value["name"][lang].Truth?value["name"][lang].Text:"Sphere",Description=value["desc"][lang].Truth?value["desc"][lang].Text:"",Level=level,Locked=locked==key,CanLock=locks>0&&locked.Length==0&&ordinary,Forecast=forecast,ElementCard=elementCard,Element=elementCard?meta["icon"].Text+" "+meta["name"][lang].Text:"",ElementColor=elementCard?meta["color"].Text:"",HintNames=string.Join("\n",hintNames),HintDescription=string.Join(" · ",hintDescriptions)};
                choices.Add(choice);signature.Append('|').Append(key).Append('|').Append(choice.Name).Append('|').Append(choice.Description).Append('|').Append(choice.HintDescription).Append('|').Append(forecast);
            }
            return new NativeUpgradeView.State {Russian=ru,Title=title,Reroll=L("Перебросить","Reroll")+" · "+rerolls,ShowReroll=choices.Count==3&&routine,CanReroll=rerolls>0,Signature=signature.ToString(),Choices=choices};
        }
    }
}
