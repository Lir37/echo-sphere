using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private NativeArtifactRewardView _nativeArtifactRewards;
        private NativeArtifactSetDetailView _nativeRewardSetDetail,_nativePauseSetDetail;
        private string _nativeRewardSetId;
        private int _nativeArtifactRewardVersion=-1;
        private bool NativeArtifactRewardRequested=>!_mainMenu&&!_gameOver&&_stellaRewardPending;
        private void DisposeNativeArtifactRewards(){_nativeRewardSetDetail?.Dispose();_nativeRewardSetDetail=null;_nativeArtifactRewards?.Dispose();_nativeArtifactRewards=null;_nativeArtifactRewardVersion=-1;_nativeRewardSetId=null;}
        private void DisposeNativePauseSetDetail(){_nativePauseSetDetail?.Dispose();_nativePauseSetDetail=null;}
        private void PrepareNativeArtifactRewards(NativeRunUiAnchors anchors)
        {
            if(!NativeArtifactRewardRequested){DisposeNativeArtifactRewards();return;}
            var slot=anchors.Rewards;var host=slot.Content;if(host==null){DisposeNativeArtifactRewards();return;}
            if(_nativeArtifactRewards==null||_nativeArtifactRewards.Root.parent!=host){DisposeNativeArtifactRewards();_nativeArtifactRewards=new NativeArtifactRewardView(host,anchors.Backdrops,PickNativeArtifact,id=>{_nativeRewardSetId=id;anchors.Invalidate?.Invoke();});}
            if(_nativeArtifactRewardVersion!=anchors.Version){_nativeArtifactRewards.Bind(ReadNativeArtifactRewardState());_nativeArtifactRewardVersion=anchors.Version;}
            bool ready=_nativeArtifactRewards.Ready;float scale=SourceDevicePixelRatio;_nativeArtifactRewards.Prepare(scale,Screen.width/scale);
            if(_nativeArtifactRewards.Ready){slot.HideLegacy();if(!ready)anchors.Invalidate?.Invoke();}
            if(string.IsNullOrEmpty(_nativeRewardSetId)){_nativeRewardSetDetail?.Dispose();_nativeRewardSetDetail=null;return;}
            if(_nativeRewardSetDetail==null||_nativeRewardSetDetail.Id!=_nativeRewardSetId){_nativeRewardSetDetail?.Dispose();var state=ReadNativeArtifactSetDetail(_nativeRewardSetId);_nativeRewardSetDetail=state==null?null:new NativeArtifactSetDetailView(_nativeArtifactRewards.Root,state,anchors.Backdrops,()=>{_nativeRewardSetId=null;anchors.Invalidate?.Invoke();});}
            _nativeRewardSetDetail?.Prepare(scale,Screen.width/scale,Screen.height/scale);
        }
        private void PrepareNativePauseSetDetail(NativeRunUiAnchors anchors)
        {
            if(!_userPaused||_mainMenu||_gameOver||string.IsNullOrEmpty(_pauseArtifactSetId)){DisposeNativePauseSetDetail();return;}
            var slot=anchors.PauseDetail;var host=slot.Content;if(host==null){DisposeNativePauseSetDetail();return;}
            if(_nativePauseSetDetail==null||_nativePauseSetDetail.Id!=_pauseArtifactSetId||_nativePauseSetDetail.Root.parent!=host){DisposeNativePauseSetDetail();var state=ReadNativeArtifactSetDetail(_pauseArtifactSetId);if(state!=null)_nativePauseSetDetail=new NativeArtifactSetDetailView(host,state,anchors.Backdrops,()=>{_pauseArtifactSetId=null;anchors.Invalidate?.Invoke();},false);}
            if(_nativePauseSetDetail==null)return;bool ready=_nativePauseSetDetail.Ready;float scale=SourceDevicePixelRatio;_nativePauseSetDetail.Prepare(scale,Screen.width/scale,Screen.height/scale);
            if(_nativePauseSetDetail.Ready){slot.HideLegacy();if(!ready)anchors.Invalidate?.Invoke();}
        }

        private void PickNativeArtifact(string id)
        {
            if(NativeArtifactRewardRequested){int index=_stellaRewardChoices.FindIndex(choice=>choice.Id==id);if(index>=0){ChooseStellaReward(index);RefreshNativeCommandModels();}}
            if(_sourceUi!=null)_sourceUi.Dirty=true;
        }
        private NativeArtifactRewardView.State ReadNativeArtifactRewardState()=>ProjectNativeLiveArtifactRewards(IsRussianUi);
        private NativeArtifactRewardView.State ProjectNativeLiveArtifactRewards(bool ru)
        {
            string lang=ru?"ru":"en";var choices=new List<NativeArtifactRewardView.Choice>();var active=new HashSet<string>();foreach(var synergy in NativeOwnedArtifactSynergies(_ownedArtifacts))active.Add(synergy.Id);
            var projected=ProjectNativeArtifactList(_ownedArtifacts,ru,_stellaRewardChoices.Select(choice=>choice.Id),_runEconomy!=null);var signature=new StringBuilder(lang);
            foreach(var artifact in projected.Artifacts){var newSynergies=new List<string>();foreach(var synergy in NativeOwnedArtifactSynergies(_ownedArtifacts,artifact.Id))if(!active.Contains(synergy.Id))newSynergies.Add(synergy.Name(ru));string mechanic=NativeRunCatalog.Instance.GetArtifact(artifact.Id).Mechanic;choices.Add(new NativeArtifactRewardView.Choice {Artifact=artifact,Mechanic=NativeArtifactMechanic(mechanic,ru),Synergies=newSynergies});signature.Append('|').Append(artifact.Id);}
            foreach(var id in _ownedArtifacts)signature.Append('|').Append(id);
            return new NativeArtifactRewardView.State {Russian=ru,Signature=signature.ToString(),Choices=choices};
        }
#if (UNITY_EDITOR && ECHO_SPHERE_REFERENCE_VISUALS)
        private NativeArtifactRewardView.State LegacyReadNativeArtifactRewardState(bool ru)
        {
            string lang=ru?"ru":"en";var choices=new List<NativeArtifactRewardView.Choice>();var active=new HashSet<string>();foreach(var synergy in NativeArtifactSynergies(_sourceRenderState))active.Add(synergy.Id);
            var projected=ProjectNativeArtifacts(_sourceRenderState,ru,_sourceRenderState["pendingArtifact"].Items);var signature=new StringBuilder(lang);
            foreach(var artifact in projected.Artifacts){var newSynergies=new List<string>();foreach(var synergy in NativeArtifactSynergies(_sourceRenderState,artifact.Id))if(!active.Contains(synergy.Id))newSynergies.Add(synergy.Name(ru));string mechanic=NativeRunCatalog.Instance.GetArtifact(artifact.Id).Mechanic;choices.Add(new NativeArtifactRewardView.Choice {Artifact=artifact,Mechanic=NativeArtifactMechanic(mechanic,ru),Synergies=newSynergies});signature.Append('|').Append(artifact.Id);}
            foreach(var id in _sourceRenderState["player"]["artifacts"].Items)signature.Append('|').Append(id.Text);
            return new NativeArtifactRewardView.State {Russian=ru,Signature=signature.ToString(),Choices=choices};
        }
#endif
        private static IReadOnlyList<NativeRunCatalog.Synergy> NativeOwnedArtifactSynergies(IEnumerable<string> ids,string added=null)
        {
            var owned=new HashSet<string>(ids,StringComparer.Ordinal);if(added!=null)owned.Add(added);return NativeRunCatalog.Instance.Synergies.Where(row=>row.Requires.All(owned.Contains)).ToList();
        }
        private static string NativeArtifactMechanic(string id,bool ru)
        {
            switch(id){case "swift":return ru?"Темп":"Tempo";case "mirror":return ru?"Контратака":"Counter";case "resonance":return ru?"Связь сфер":"Sphere Link";case "lone":return ru?"Одна сфера":"Solo Sphere";case "fivefold":case "network":return ru?"Сеть":"Network";case "relay":return ru?"Прогрессия":"Progression";case "triangle":return ru?"Геометрия":"Geometry";case "overclock":return ru?"Разгон":"Overclock";case "singularity":return ru?"Сингулярность":"Singularity";case "zero":return ru?"Архитектура":"Architecture";case "unified":return ru?"Единый разум":"Unified Mind";default:return "";}
        }
        private NativeArtifactSetDetailView.State ReadNativeArtifactSetDetail(string id)
        {
            var found=NativeRunCatalog.Instance.Sets.FirstOrDefault(set=>set.Id==id);if(found==null)return null;
            var owned=new HashSet<string>(_ownedArtifacts,StringComparer.Ordinal);
            var items=new List<NativeArtifactSetDetailView.Item>();foreach(var artifact in ProjectNativeArtifactList(_ownedArtifacts,IsRussianUi,found.ArtifactIds,_runEconomy!=null).Artifacts)items.Add(new NativeArtifactSetDetailView.Item {Artifact=artifact,Owned=owned.Contains(artifact.Id)});
            return new NativeArtifactSetDetailView.State {Id=id,Name=found.Name(IsRussianUi),Description=found.Description(IsRussianUi),Owned=found.ArtifactIds.Count(owned.Contains),Total=found.ArtifactIds.Count,Russian=IsRussianUi,Items=items};
        }
    }
}
