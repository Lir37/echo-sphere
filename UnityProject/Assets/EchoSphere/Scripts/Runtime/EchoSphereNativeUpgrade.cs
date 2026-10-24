using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeUpgradeView _nativeUpgradeView;
        private int _nativeUpgradeBoundVersion=-1;
        private bool NativeUpgradeRequested=>!_mainMenu&&!_gameOver&&_levelUp;
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

        private void PickNativeUpgrade(int index){if(NativeUpgradeRequested){if(_choosingEvolution)ChooseEvolution(index);else ChooseUpgrade(index);RefreshNativeCommandModels();}if(_sourceUi!=null)_sourceUi.Dirty=true;}
        private void LockNativeUpgrade(int index){if(NativeUpgradeRequested&&!_choosingEvolution){LockLevelUpChoice(index);RefreshNativeCommandModels();}if(_sourceUi!=null)_sourceUi.Dirty=true;}
        private void RerollNativeUpgrades(){if(NativeUpgradeRequested&&!_choosingEvolution){RerollLevelUpChoices();RefreshNativeCommandModels();}if(_sourceUi!=null)_sourceUi.Dirty=true;}
        private sealed class NativeUpgradeInput
        {
            internal int Index,CurrentLevel,NewLevel;internal string Type,AbilityStage,SphereStage,SphereType,SphereBranch,Key;internal NativeRunCatalog.Text Text;
        }
        private List<NativeUpgradeInput> BuildNativeUpgradeInputs()
        {
            var result=new List<NativeUpgradeInput>();if(!_levelUp)return result;
            if(_choosingEvolution){
                int stage=_pendingFinalEvolution?7:4;
                if(_pendingEvolutionIsAbility){
                    string id=SourceAbilityId(_pendingEvolutionAbility);var definition=NativeRunCatalog.Instance.GetAbility(id);IEnumerable<NativeRunCatalog.Definition> pool=definition.HasProgression?definition.Branches:Enumerable.Empty<NativeRunCatalog.Definition>();
                    if(stage==7){var branch=definition.Branches.FirstOrDefault(row=>row.Id==GetAbilityBranch(_pendingEvolutionAbility));pool=branch==null?Enumerable.Empty<NativeRunCatalog.Definition>():branch.HasFinalPool?branch.Finals:definition.FallbackFinals;}
                    int index=0;foreach(var text in pool.Take(3)){result.Add(new NativeUpgradeInput {Index=index,Type="ability",AbilityStage=stage==4?"branch":"final",CurrentLevel=stage,NewLevel=stage,Text=text,Key=string.Join(":","ability",id,stage==4?"branch":"final",stage,stage,index)});index++;}
                }else{
                    string id=_pendingEvolutionSphere.ToString().ToLowerInvariant();var definition=NativeRunCatalog.Instance.GetSphere(id);
                    if(stage==4){foreach(var branch in definition.Branches.Take(3))result.Add(new NativeUpgradeInput {Index=result.Count,Type="sphere",SphereType=id,SphereStage="branch",SphereBranch=branch.Id,CurrentLevel=stage,NewLevel=stage,Text=branch.BranchCard,Key=string.Join(":","sphere",id,"branch",stage,stage,branch.Id,"")});}
                    else {var branch=definition.Branches.FirstOrDefault(row=>row.Id==GetSphereBranch(_pendingEvolutionSphere));if(branch!=null)for(int i=0;i<Math.Min(3,branch.FinalCards.Count);i++)result.Add(new NativeUpgradeInput {Index=i,Type="sphere",SphereType=id,SphereStage="final",SphereBranch=branch.Id,CurrentLevel=stage,NewLevel=stage,Text=branch.FinalCards[i],Key=string.Join(":","sphere",id,"final",stage,stage,branch.Id,i)});}
                }
                if(result.Count!=_evolutionChoices.Count)throw new InvalidOperationException("Evolution cards and native choices differ");
            }else foreach(var choice in _levelUpChoices){
                var text=SourceUpgradeText(choice);if(text==null)throw new InvalidOperationException("Missing source upgrade text: "+GetUpgradeChoiceKey(choice));
                var row=new NativeUpgradeInput {Index=result.Count,Type=choice.Kind==RunUpgradeKind.Sphere?"sphere":"ability",CurrentLevel=choice.CurrentLevel,NewLevel=choice.NextLevel,Text=new NativeRunCatalog.Text {NameRu=text.nameRu,NameEn=text.nameEn,DescriptionRu=text.descRu,DescriptionEn=text.descEn},Key=GetUpgradeChoiceKey(choice)};
                if(choice.Kind==RunUpgradeKind.Sphere){row.SphereType=choice.Sphere.ToString().ToLowerInvariant();row.SphereStage="upgrade";row.SphereBranch=GetSphereBranch(choice.Sphere);}
                else {var definition=NativeRunCatalog.Instance.GetAbility(SourceAbilityId(choice.Kind));row.Text.NameRu=definition.NameRu;row.Text.NameEn=definition.NameEn;}result.Add(row);
            }
            return result;
        }
        private NativeUpgradeView.State ReadNativeUpgradeState()=>ProjectNativeLiveUpgradeState(IsRussianUi);
        private NativeUpgradeView.State ProjectNativeLiveUpgradeState(bool ru)
        {
            string lang=ru?"ru":"en";string L(string a,string b)=>ru?a:b;string T(string key)=>NativeText(key,lang=="ru");
            var pending=BuildNativeUpgradeInputs();var first=pending.Count>0?pending[0]:new NativeUpgradeInput();
            string title=first.Type=="modifier"?L("Новый модификатор","New Modifier"):first.AbilityStage=="branch"?L("Эволюция способности I","Ability Evolution I"):first.AbilityStage=="final"?L("Финальная форма способности","Final Ability Form"):first.SphereStage=="branch"?L("Мутация сферы I","Sphere Mutation I"):first.SphereStage=="final"?L("Мутация сферы II","Sphere Mutation II"):T("chooseUpgrade");
            var choices=new List<NativeUpgradeView.Choice>();bool routine=true;string locked=_levelUpLockChoiceKey??"";
            int locks=_levelUpLocksRemaining,rerolls=_levelUpRerollsRemaining;
            var signature=new StringBuilder(lang).Append('|').Append(title).Append('|').Append(locked).Append('|').Append(locks).Append('|').Append(rerolls);
            foreach(var value in pending){
                string type=value.Type,abilityStage=value.AbilityStage,sphereStage=value.SphereStage;
                bool ordinary=type=="sphere"&&sphereStage=="upgrade"||type=="ability"&&string.IsNullOrEmpty(value.AbilityStage);routine&=ordinary;
                string key=value.Key;
                bool forecast=_sphereCharacterId==EchoSphere.Core.SphereCharacterId.Oracle&&_oracleForecastKeys.Contains(key);
                string category=type=="modifier"?L("МОДИФИКАТОР","MODIFIER"):abilityStage=="branch"?L("ВЕТКА СПОСОБНОСТИ","ABILITY BRANCH"):abilityStage=="final"?L("ФИНАЛЬНАЯ ФОРМА СПОСОБНОСТИ","FINAL ABILITY FORM"):sphereStage=="branch"?L("ВЕТКА СФЕРЫ","SPHERE BRANCH"):sphereStage=="final"?L("ФИНАЛЬНАЯ СПЕЦИАЛИЗАЦИЯ","FINAL SPECIALIZATION"):type=="ability"?L("АКТИВНАЯ СПОСОБНОСТЬ","ACTIVE ABILITY"):L("УЛУЧШЕНИЕ СФЕРЫ","SPHERE UPGRADE");
                string level=type=="modifier"?L("Разблокирует эффект","Unlocks the effect"):sphereStage=="branch"||abilityStage=="branch"?L("Уровень IV • выбор ветки","Level IV • choose a branch"):sphereStage=="final"||abilityStage=="final"?L("Уровень VII • выбор финальной формы","Level VII • choose final form"):T("level")+" "+NativePresentationMath.NumberText(value.CurrentLevel)+" → "+NativePresentationMath.NumberText(value.NewLevel);
                string element=type=="sphere"?NativeRunCatalog.Instance.GetSphereBranch(value.SphereBranch)?.Element:null;
                var meta=NativeRunCatalog.Instance.GetElement(element);
                bool elementCard=meta!=null&&(sphereStage=="branch"||sphereStage=="final");
                var hintNames=new List<string>();var hintDescriptions=new List<string>();
                if(type=="sphere"&&!string.IsNullOrEmpty(value.SphereBranch)){string sphereType=!string.IsNullOrEmpty(value.SphereType)?value.SphereType:"standard";foreach(var hint in NativeRunCatalog.Instance.SphereAbilityLinks){if(hint.Sphere!=sphereType||hint.Branch!=value.SphereBranch)continue;hintNames.Add(hint.Name(ru)+" · "+NativeRunCatalog.Instance.GetAbility(hint.Ability).Name(ru));hintDescriptions.Add(hint.Description(ru));}}
                var choice=new NativeUpgradeView.Choice {Index=value.Index,Key=key,Category=category,Name=!string.IsNullOrEmpty(value.Text.Name(ru))?value.Text.Name(ru):"Sphere",Description=value.Text.Description(ru)??"",Level=level,Locked=locked==key,CanLock=locks>0&&locked.Length==0&&ordinary,Forecast=forecast,ElementCard=elementCard,Element=elementCard?meta.Icon+" "+meta.Name(ru):"",ElementColor=elementCard?meta.Color:"",HintNames=string.Join("\n",hintNames),HintDescription=string.Join(" · ",hintDescriptions)};
                choices.Add(choice);signature.Append('|').Append(key).Append('|').Append(choice.Name).Append('|').Append(choice.Description).Append('|').Append(choice.HintDescription).Append('|').Append(forecast);
            }
            return new NativeUpgradeView.State {Russian=ru,Title=title,Reroll=L("Перебросить","Reroll")+" · "+rerolls,ShowReroll=choices.Count==3&&routine,CanReroll=rerolls>0,Signature=signature.ToString(),Choices=choices};
        }
#if UNITY_EDITOR
        private NativeUpgradeView.State LegacyReadNativeUpgradeState(bool ru)
        {
            string lang=ru?"ru":"en";string L(string a,string b)=>ru?a:b;string T(string key)=>NativeText(key,lang=="ru");
            var pending=_sourceRenderState["pendingUpgrade"];var first=pending[0];
            string title=first["type"].Text=="modifier"?L("Новый модификатор","New Modifier"):first["abilityStage"].Text=="branch"?L("Эволюция способности I","Ability Evolution I"):first["abilityStage"].Text=="final"?L("Финальная форма способности","Final Ability Form"):first["sphereStage"].Text=="branch"?L("Мутация сферы I","Sphere Mutation I"):first["sphereStage"].Text=="final"?L("Мутация сферы II","Sphere Mutation II"):T("chooseUpgrade");
            var choices=new List<NativeUpgradeView.Choice>();bool routine=true;string locked=_sourceRenderState["levelUpLockChoiceKey"].Truth?_sourceRenderState["levelUpLockChoiceKey"].Text:"";
            int locks=(int)_sourceRenderState["levelUpLocksRemaining"].Number,rerolls=(int)_sourceRenderState["levelUpRerollsRemaining"].Number;
            var signature=new StringBuilder(lang).Append('|').Append(title).Append('|').Append(locked).Append('|').Append(locks).Append('|').Append(rerolls);
            foreach(var value in pending.Items){
                string type=value["type"].Text,abilityStage=value["abilityStage"].Text,sphereStage=value["sphereStage"].Text;
                bool ordinary=type=="sphere"&&sphereStage=="upgrade"||type=="ability"&&!value["abilityStage"].Truth;routine&=ordinary;
                string key=NativeUpgradeChoiceKey(value);
                bool forecast=_sphereCharacterId==EchoSphere.Core.SphereCharacterId.Oracle&&_oracleForecastKeys.Contains(key);
                string category=type=="modifier"?L("МОДИФИКАТОР","MODIFIER"):abilityStage=="branch"?L("ВЕТКА СПОСОБНОСТИ","ABILITY BRANCH"):abilityStage=="final"?L("ФИНАЛЬНАЯ ФОРМА СПОСОБНОСТИ","FINAL ABILITY FORM"):sphereStage=="branch"?L("ВЕТКА СФЕРЫ","SPHERE BRANCH"):sphereStage=="final"?L("ФИНАЛЬНАЯ СПЕЦИАЛИЗАЦИЯ","FINAL SPECIALIZATION"):type=="ability"?L("АКТИВНАЯ СПОСОБНОСТЬ","ACTIVE ABILITY"):L("УЛУЧШЕНИЕ СФЕРЫ","SPHERE UPGRADE");
                string level=type=="modifier"?L("Разблокирует эффект","Unlocks the effect"):sphereStage=="branch"||abilityStage=="branch"?L("Уровень IV • выбор ветки","Level IV • choose a branch"):sphereStage=="final"||abilityStage=="final"?L("Уровень VII • выбор финальной формы","Level VII • choose final form"):T("level")+" "+value["currentLevel"].Text+" → "+value["newLevel"].Text;
                string element=type=="sphere"?NativeRunCatalog.Instance.GetSphereBranch(value["sphereBranch"].Text)?.Element:null;
                var meta=NativeRunCatalog.Instance.GetElement(element);
                bool elementCard=meta!=null&&(sphereStage=="branch"||sphereStage=="final");
                var hintNames=new List<string>();var hintDescriptions=new List<string>();
                if(type=="sphere"&&value["sphereBranch"].Truth){string sphereType=value["sphereType"].Truth?value["sphereType"].Text:"standard";foreach(var hint in NativeRunCatalog.Instance.SphereAbilityLinks){if(hint.Sphere!=sphereType||hint.Branch!=value["sphereBranch"].Text)continue;hintNames.Add(hint.Name(ru)+" · "+NativeRunCatalog.Instance.GetAbility(hint.Ability).Name(ru));hintDescriptions.Add(hint.Description(ru));}}
                var choice=new NativeUpgradeView.Choice {Index=(int)value["nativeIndex"].Number,Key=key,Category=category,Name=value["name"][lang].Truth?value["name"][lang].Text:"Sphere",Description=value["desc"][lang].Truth?value["desc"][lang].Text:"",Level=level,Locked=locked==key,CanLock=locks>0&&locked.Length==0&&ordinary,Forecast=forecast,ElementCard=elementCard,Element=elementCard?meta.Icon+" "+meta.Name(ru):"",ElementColor=elementCard?meta.Color:"",HintNames=string.Join("\n",hintNames),HintDescription=string.Join(" · ",hintDescriptions)};
                choices.Add(choice);signature.Append('|').Append(key).Append('|').Append(choice.Name).Append('|').Append(choice.Description).Append('|').Append(choice.HintDescription).Append('|').Append(forecast);
            }
            return new NativeUpgradeView.State {Russian=ru,Title=title,Reroll=L("Перебросить","Reroll")+" · "+rerolls,ShowReroll=choices.Count==3&&routine,CanReroll=rerolls>0,Signature=signature.ToString(),Choices=choices};
        }
#endif
    }
}
