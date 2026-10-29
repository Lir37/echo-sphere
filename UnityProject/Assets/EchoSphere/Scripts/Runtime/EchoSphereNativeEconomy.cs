using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private RunEconomyState _runEconomy;
        private NativeMerchantView _nativeMerchantView;
        public void CollectExperienceOrb(int experience,int echoAmount)
        {
            if(_gameOver||(_paused&&!_simulationFrame))return;
            RunEconomyRules.Collect(_runEconomy,echoAmount);AddExperience(experience);
        }
        private void TickNativeEconomy()
        {
            if(_runEconomy==null||_tutorialStep>=0||_mainMenu||_gameOver||_regionStabilized)return;
            if(RunEconomyRules.TakeScheduledChest(_runEconomy,_runTime)){
                var chest=new GameObject("Scheduled Artifact Cache");chest.transform.position=_player.position+Vector3.right*.55f;
                chest.AddComponent<ArtifactChestAgent2D>().Initialize(this);
            }
            RunEconomyRules.AdvanceMerchant(_runEconomy,_runTime);
            if(_runEconomy.Pending&&!_runEconomy.Open&&_runTime>=_runEconomy.ArrivedAt+40)OpenNativeMerchant();
        }
        private bool CanOpenNativeMerchant=>_runEconomy!=null&&_runEconomy.Pending&&!_runEconomy.Open&&!_mainMenu&&!_gameOver&&!_paused&&!_levelUp&&!_choosingEvolution&&!_stellaRewardPending&&_tutorialStep<0;
        private void OpenNativeMerchant()
        {
            if(!CanOpenNativeMerchant)return;
            RunEconomyRules.Prepare(_runEconomy,_ownedArtifacts);_runEconomy.Open=true;_paused=true;
            ResetNativeMobileGestures(false);ClearSpherePointer();RefreshNativeCommandModels();SaveCurrentRun();
        }
        private void CloseNativeMerchant()
        {
            if(_runEconomy==null||!_runEconomy.Open)return;
            RunEconomyRules.Close(_runEconomy);_paused=_userPaused;RefreshNativeCommandModels();SaveCurrentRun();
        }
        private void BuyNativeMerchant(int slot)
        {
            if(_mainMenu||_gameOver||_levelUp||_choosingEvolution||_stellaRewardPending||!RunEconomyRules.TryBuy(_runEconomy,slot,_ownedArtifacts))return;
            ApplyOwnedArtifactEffects(ArtifactRewardRules.Get(_runEconomy.Offers[slot].ArtifactId));
            SourceAudio2D.Play("chest");RefreshNativeCommandModels();SaveCurrentRun();
        }
        private void ApplyOwnedArtifactEffects(ArtifactRewardDefinition choice)
        {
            RecordDiscoveredArtifactProtocols();
            if(choice.GetEffect("maxHp")>0f&&_playerCore!=null)_playerCore.IncreaseMaxHp(choice.GetEffect("maxHp"));
            if(_playerCore!=null)_playerCore.SetDashCooldownMultiplier(Mathf.Max(.55f,1f+ArtifactRewardRules.GetEffectTotal(_ownedArtifacts,"cooldown")));
        }
        private void PrepareNativeMerchant()
        {
            if(_nativeRunDocument==null)return;
            if(_nativeMerchantView==null)_nativeMerchantView=new NativeMerchantView(_nativeRunDocument.ControlsParent,OpenNativeMerchant,CloseNativeMerchant,BuyNativeMerchant);
            _nativeMerchantView.Bind(_runEconomy,_ownedArtifacts,IsRussianUi,_runTime,!_mainMenu&&!_gameOver&&_tutorialStep<0,CanOpenNativeMerchant,IsLeftHanded);
        }
    }
}
