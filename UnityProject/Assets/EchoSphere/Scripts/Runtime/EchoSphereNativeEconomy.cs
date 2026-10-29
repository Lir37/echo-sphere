using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private RunEconomyState _runEconomy;
        private float GetRunArtifactEffectTotal(string key)=>ArtifactGameplayRules.EffectTotal(_ownedArtifacts,key,_runEconomy!=null);
        private float GetArtifactSphereDamageMultiplier(SphereAttackAgent sphere)
        {
            float family=GetArtifactSphereFamilyDamageMultiplier(sphere.Type);if(_runEconomy==null)return family;
            var context=new ArtifactDamageContext {UniqueTypes=CountUniqueActiveSphereTypes(),StrongestLevel=GetStrongestSphereLevel(),
                AnyActiveGeometry=_networkState?.DominantFormation?.Active==true||_networkState?.SecondaryFormation?.Active==true,
                TriangleActive=_networkState?.GetFormation(SphereNetworkFormation.Triangle)?.Active==true&&GetFormationBonus(SphereNetworkFormation.Triangle,GetSphereIndex(sphere))>0};
            int index=GetSphereIndex(sphere);if(_networkState!=null)foreach(var link in _networkState.Links)if(link.A==index||link.B==index){context.HasActiveLink=true;break;}
            float nearby=BossCombatRules.ToWorldUnits(190f);nearby*=nearby;
            foreach(var other in _spheres){if(other==null||other==sphere||!other.gameObject.activeInHierarchy)continue;
                if(other.ProgressionLevel>=7)context.AnotherLevelSeven=true;
                if((other.transform.position-sphere.transform.position).sqrMagnitude<=nearby)context.NearOtherSphere=true;
            }
            return family*ArtifactGameplayRules.ConditionalDamageMultiplier(_ownedArtifacts,context);
        }
        public float NetworkRecoverySpeed=>ArtifactGameplayRules.RecoverySpeedMultiplier(_ownedArtifacts,_runEconomy!=null);
        public int GetArtifactChainExtraJumps()=>ArtifactGameplayRules.ExtraChainJumps(_ownedArtifacts,_runEconomy!=null);
        private NativeMerchantView _nativeMerchantView;
        public void CollectExperienceOrb(int experience,int echoAmount)
        {
            if(_mainMenu||_gameOver||(_paused&&!_simulationFrame))return;
            RunEconomyRules.Collect(_runEconomy,echoAmount);AddExperience(experience);
        }
        private void TickNativeEconomy()
        {
            if(_runEconomy==null||_tutorialStep>=0||_mainMenu||_gameOver||_regionStabilized||_playerCore==null||_playerCore.IsDead)return;
            if(RunEconomyRules.TakeScheduledChest(_runEconomy,_runTime)){
                var chest=new GameObject("Scheduled Artifact Cache");chest.transform.position=_player.position+Vector3.right*.55f;
                chest.AddComponent<ArtifactChestAgent2D>().Initialize(this);
            }
            RunEconomyRules.AdvanceMerchant(_runEconomy,_runTime);
            if(_runEconomy.Pending&&!_runEconomy.Open&&_runTime>=_runEconomy.ArrivedAt+40)OpenNativeMerchant();
        }
        private bool CanOpenNativeMerchant=>_runEconomy!=null&&_runEconomy.Pending&&!_runEconomy.Open&&!_mainMenu&&!_gameOver&&!_paused&&!_levelUp&&!_choosingEvolution&&!_stellaRewardPending&&_tutorialStep<0&&_playerCore!=null&&!_playerCore.IsDead;
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
            if(_playerCore!=null)_playerCore.SetDashCooldownMultiplier(Mathf.Max(.55f,1f+GetRunArtifactEffectTotal("cooldown")));
        }
        private void PrepareNativeMerchant()
        {
            if(_nativeRunDocument==null)return;
            if(_nativeMerchantView==null)_nativeMerchantView=new NativeMerchantView(_nativeRunDocument.ControlsParent,OpenNativeMerchant,CloseNativeMerchant,BuyNativeMerchant);
            _nativeMerchantView.Bind(_runEconomy,_ownedArtifacts,IsRussianUi,_runTime,!_mainMenu&&!_gameOver&&_tutorialStep<0&&(_runEconomy!=null&&_runEconomy.Open||(!_userPaused&&!_levelUp&&!_choosingEvolution&&!_stellaRewardPending)),CanOpenNativeMerchant,IsLeftHanded);
        }
    }
}
