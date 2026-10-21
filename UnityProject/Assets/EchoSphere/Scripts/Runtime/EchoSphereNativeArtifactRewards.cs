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
        private NativeArtifactRewardView _nativeArtifactRewards;
        private NativeArtifactSetDetailView _nativeRewardSetDetail,_nativePauseSetDetail;
        private string _nativeRewardSetId;
        private int _nativeArtifactRewardVersion=-1;
        private bool NativeArtifactRewardRequested=>!_mainMenu&&!_gameOver&&_sourceRenderState["pendingArtifact"].Truth;
        private void DisposeNativeArtifactRewards(){_nativeRewardSetDetail?.Dispose();_nativeRewardSetDetail=null;_nativeArtifactRewards?.Dispose();_nativeArtifactRewards=null;_nativeArtifactRewardVersion=-1;_nativeRewardSetId=null;}
        private void DisposeNativePauseSetDetail(){_nativePauseSetDetail?.Dispose();_nativePauseSetDetail=null;}
        private void PrepareNativeArtifactRewards(VisualCssNode root)
        {
            if(!NativeArtifactRewardRequested){DisposeNativeArtifactRewards();return;}
            VisualCssNode overlay=null;
            void Find(VisualCssNode node){if(overlay==null&&node.HasClass("bg-black/70")&&node.HasClass("backdrop-blur-sm")&&node.HasClass("z-50"))overlay=node;foreach(var child in node.Children)Find(child);}
            Find(root);if(!(overlay?.Native is SourceUiFrame frame)){DisposeNativeArtifactRewards();return;}
            if(_nativeArtifactRewards==null||_nativeArtifactRewards.Root.parent!=frame.Content){DisposeNativeArtifactRewards();_nativeArtifactRewards=new NativeArtifactRewardView(frame.Content,_sourceUi,PickNativeArtifact,id=>{_nativeRewardSetId=id;_sourceUi.Dirty=true;});}
            if(_nativeArtifactRewardVersion!=_sourceUi.FrameVersion){_nativeArtifactRewards.Bind(ReadNativeArtifactRewardState());_nativeArtifactRewardVersion=_sourceUi.FrameVersion;}
            bool ready=_nativeArtifactRewards.Ready;float scale=SourceDevicePixelRatio;_nativeArtifactRewards.Prepare(scale,Screen.width/scale);
            if(_nativeArtifactRewards.Ready){foreach(var child in overlay.Children)if(child.Native is VisualElement e)e.style.visibility=Visibility.Hidden;if(!ready)_sourceUi.Dirty=true;}
            if(string.IsNullOrEmpty(_nativeRewardSetId)){_nativeRewardSetDetail?.Dispose();_nativeRewardSetDetail=null;return;}
            if(_nativeRewardSetDetail==null||_nativeRewardSetDetail.Id!=_nativeRewardSetId){_nativeRewardSetDetail?.Dispose();var state=ReadNativeArtifactSetDetail(_nativeRewardSetId);_nativeRewardSetDetail=state==null?null:new NativeArtifactSetDetailView(_nativeArtifactRewards.Root,state,_sourceUi,()=>{_nativeRewardSetId=null;_sourceUi.Dirty=true;});}
            _nativeRewardSetDetail?.Prepare(scale,Screen.width/scale,Screen.height/scale);
        }
        private void PrepareNativePauseSetDetail(VisualCssNode root)
        {
            if(!_userPaused||_mainMenu||_gameOver||string.IsNullOrEmpty(_pauseArtifactSetId)){DisposeNativePauseSetDetail();return;}
            VisualCssNode overlay=null;
            void Find(VisualCssNode node,bool pause){pause|=node.HasClass("es-pause-overlay");if(pause&&overlay==null&&node.HasClass("z-[70]")&&node.HasClass("bg-black/75"))overlay=node;foreach(var child in node.Children)Find(child,pause);}
            Find(root,false);if(!(overlay?.Native is SourceUiFrame frame)){DisposeNativePauseSetDetail();return;}
            if(_nativePauseSetDetail==null||_nativePauseSetDetail.Id!=_pauseArtifactSetId||_nativePauseSetDetail.Root.parent!=frame.Content){DisposeNativePauseSetDetail();var state=ReadNativeArtifactSetDetail(_pauseArtifactSetId);if(state!=null)_nativePauseSetDetail=new NativeArtifactSetDetailView(frame.Content,state,_sourceUi,()=>{_pauseArtifactSetId=null;_sourceUi.Dirty=true;},false);}
            if(_nativePauseSetDetail==null)return;bool ready=_nativePauseSetDetail.Ready;float scale=SourceDevicePixelRatio;_nativePauseSetDetail.Prepare(scale,Screen.width/scale,Screen.height/scale);
            if(_nativePauseSetDetail.Ready){foreach(var child in overlay.Children)if(child.Native is VisualElement e)e.style.visibility=Visibility.Hidden;if(!ready)_sourceUi.Dirty=true;}
        }
        private V ProjectNativeArtifactRewards(V tree)
        {
            bool reward=NativeArtifactRewardRequested&&_nativeArtifactRewards!=null&&_nativeArtifactRewards.Ready,pause=_nativePauseSetDetail!=null&&_nativePauseSetDetail.Ready;
            if(!reward&&!pause)return tree;
            void Visit(V value){if(value.Raw is VisualDom.Node node){var tokens=(node.Properties["className"].Text??"").Split(' ');bool Has(string s)=>Array.IndexOf(tokens,s)>=0;if(reward&&Has("bg-black/70")&&Has("backdrop-blur-sm")&&Has("z-50")||pause&&Has("z-[70]")&&Has("bg-black/75")){node.Children.Clear();return;}foreach(var child in node.Children)Visit(child);}else if(value.Raw is List<V>)foreach(var child in value.Items)Visit(child);}
            Visit(tree);return tree;
        }
        private void PickNativeArtifact(string id)
        {
            if(NativeArtifactRewardRequested){int index=_stellaRewardChoices.FindIndex(choice=>choice.Id==id);if(index>=0){ChooseStellaReward(index);_sourceRenderState["pendingArtifact"]=VisualJs.Null;ApplySourceUiWrites();}}
            if(_sourceUi!=null)_sourceUi.Dirty=true;
        }
        private NativeArtifactRewardView.State ReadNativeArtifactRewardState()
        {
            bool ru=IsRussianUi;string lang=ru?"ru":"en";var choices=new List<NativeArtifactRewardView.Choice>();var active=new HashSet<string>();foreach(var synergy in Visual_artifactSystem.Scope["getActiveArtifactSynergies"].Call(_sourceRenderState).Items)active.Add(synergy["id"].Text);
            var projected=ProjectNativeArtifacts(_sourceRenderState,ru,_sourceRenderState["pendingArtifact"].Items);var signature=new StringBuilder(lang);
            foreach(var artifact in projected.Artifacts){var newSynergies=new List<string>();foreach(var synergy in Visual_artifactSystem.Scope["getArtifactSynergiesAfterPick"].Call(_sourceRenderState,artifact.Id).Items)if(!active.Contains(synergy["id"].Text))newSynergies.Add(synergy["name"][lang].Text);string mechanic=Visual_artifactSystem.Scope["ARTIFACT_META"][artifact.Id]["mechanic"].Text;choices.Add(new NativeArtifactRewardView.Choice {Artifact=artifact,Mechanic=NativeArtifactMechanic(mechanic,ru),Synergies=newSynergies});signature.Append('|').Append(artifact.Id);}
            foreach(var id in _sourceRenderState["player"]["artifacts"].Items)signature.Append('|').Append(id.Text);
            return new NativeArtifactRewardView.State {Russian=ru,Signature=signature.ToString(),Choices=choices};
        }
        private static string NativeArtifactMechanic(string id,bool ru)
        {
            switch(id){case "swift":return ru?"Темп":"Tempo";case "mirror":return ru?"Контратака":"Counter";case "resonance":return ru?"Связь сфер":"Sphere Link";case "lone":return ru?"Одна сфера":"Solo Sphere";case "fivefold":case "network":return ru?"Сеть":"Network";case "relay":return ru?"Прогрессия":"Progression";case "triangle":return ru?"Геометрия":"Geometry";case "overclock":return ru?"Разгон":"Overclock";case "singularity":return ru?"Сингулярность":"Singularity";case "zero":return ru?"Архитектура":"Architecture";case "unified":return ru?"Единый разум":"Unified Mind";default:return "";}
        }
        private NativeArtifactSetDetailView.State ReadNativeArtifactSetDetail(string id)
        {
            string lang=IsRussianUi?"ru":"en";V found=VisualJs.Null;foreach(var set in Visual_artifactSystem.Scope["getArtifactSetArtifactProgress"].Call(_sourceRenderState).Items)if(set["setId"].Text==id){found=set;break;}if(!found.Truth)return null;
            var owned=new HashSet<string>();foreach(var item in found["ownedArtifactIds"].Items)owned.Add(item.Text);var items=new List<NativeArtifactSetDetailView.Item>();foreach(var artifact in ProjectNativeArtifacts(_sourceRenderState,IsRussianUi,found["artifactIds"].Items).Artifacts)items.Add(new NativeArtifactSetDetailView.Item {Artifact=artifact,Owned=owned.Contains(artifact.Id)});
            return new NativeArtifactSetDetailView.State {Id=id,Name=found["name"][lang].Text,Description=found["desc"][lang].Text,Owned=(int)found["owned"].Number,Total=(int)found["total"].Number,Russian=IsRussianUi,Items=items};
        }
    }
}
