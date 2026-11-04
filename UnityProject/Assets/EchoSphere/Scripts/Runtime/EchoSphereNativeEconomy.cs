using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private RunEconomyState _runEconomy;
        private float _timeSplitterBuffTimer;
        private float _mandalaDamageStored;
        private void ApplyMandalaHpLoss(float hpBefore)
        {
            if(_playerCore==null)return;
            if(ArtifactGameplayRules.AccumulateMandala(ref _mandalaDamageStored,hpBefore-_playerCore.CurrentHp,_playerCore.ShieldCharges,!_playerCore.IsDead,_runEconomy!=null&&_ownedArtifacts.Contains("stasis_mandala")))GrantPlayerShieldCharge(1);
        }
        private float GetRunArtifactEffectTotal(string key)=>ArtifactGameplayRules.EffectTotal(_ownedArtifacts,key,_runEconomy!=null);
        private float GetArtifactSphereDamageMultiplier(SphereAttackAgent sphere)
        {
            float family=GetArtifactSphereFamilyDamageMultiplier(sphere.Type);if(_runEconomy==null)return family;
            var context=new ArtifactDamageContext {ActiveNetworkLinks=_networkState?.Links.Count??0,UniqueTypes=CountUniqueActiveSphereTypes(),StrongestLevel=GetStrongestSphereLevel(),
                AnyActiveGeometry=_networkState?.DominantFormation?.Active==true||_networkState?.SecondaryFormation?.Active==true,
                TriangleActive=_networkState?.GetFormation(SphereNetworkFormation.Triangle)?.Active==true&&GetFormationBonus(SphereNetworkFormation.Triangle,GetSphereIndex(sphere))>0};
            int index=GetSphereIndex(sphere);if(_networkState!=null)foreach(var link in _networkState.Links)if(link.A==index||link.B==index){context.HasActiveLink=true;break;}
            float nearby=BossCombatRules.ToWorldUnits(190f);nearby*=nearby;
            foreach(var other in _spheres){if(other==null||other==sphere||!other.gameObject.activeInHierarchy)continue;
                if(other.ProgressionLevel>=7)context.AnotherLevelSeven=true;
                if((other.transform.position-sphere.transform.position).sqrMagnitude<=nearby)context.NearOtherSphere=true;
            }
            return family*ArtifactGameplayRules.ConditionalDamageMultiplier(_ownedArtifacts,context)*ArtifactGameplayRules.UniversalFoldMultiplier(_ownedArtifacts,true);
        }
        public float NetworkRecoverySpeed=>ArtifactGameplayRules.RecoverySpeedMultiplier(_ownedArtifacts,_runEconomy!=null);
        public int GetArtifactChainExtraJumps()=>ArtifactGameplayRules.ExtraChainJumps(_ownedArtifacts,_runEconomy!=null);
        private NativeMerchantView _nativeMerchantView;
        public void CollectExperienceOrb(int experience,int echoAmount)
        {
            if(_mainMenu||_gameOver||(_paused&&!_simulationFrame))return;
            RunEconomyRules.Collect(_runEconomy,echoAmount);AddExperience(experience);
        }
        private bool TryCreateExplorationChest()
        {
            if(_mainMenu||_gameOver||_tutorialStep>=0||_player==null||!RunEconomyRules.TakeExplorationChest(_runEconomy,_runTime))return false;
            var chest=new GameObject("Exploration Artifact Cache");chest.transform.position=_player.position+Vector3.left*.55f;
            chest.AddComponent<ArtifactChestAgent2D>().Initialize(this);return true;
        }
        private void TickNativeEconomy()
        {
            if(_runEconomy==null||_tutorialStep>=0||_mainMenu||_gameOver||_regionStabilized||_playerCore==null||_playerCore.IsDead)return;
            if(RunEconomyRules.TakeScheduledChest(_runEconomy,_runTime)){
                var chest=new GameObject("Scheduled Artifact Cache");chest.transform.position=_player.position+Vector3.right*.55f;
                chest.AddComponent<ArtifactChestAgent2D>().Initialize(this);
            }
            RunEconomyRules.AdvanceMerchant(_runEconomy,_runTime);
            if(_runEconomy.Pending&&!_runEconomy.Open){
                if(!_runEconomy.MerchantPlaced){var point=PlayerPosition+Vector2.right*(90*SpherePlacementRules.SourcePixelsToWorld);float edge=30*SpherePlacementRules.SourcePixelsToWorld;point.x=Mathf.Clamp(point.x,-SpherePlacementRules.WorldWidth/2+edge,SpherePlacementRules.WorldWidth/2-edge);point.y=Mathf.Clamp(point.y,-SpherePlacementRules.WorldHeight/2+edge,SpherePlacementRules.WorldHeight/2-edge);_runEconomy.MerchantX=point.x;_runEconomy.MerchantY=point.y;_runEconomy.MerchantPlaced=true;}
                if(Vector2.Distance(PlayerPosition,new Vector2(_runEconomy.MerchantX,_runEconomy.MerchantY))<=26*SpherePlacementRules.SourcePixelsToWorld)OpenNativeMerchant();
            }
        }
        private bool CanOpenNativeMerchant=>_runEconomy!=null&&_runEconomy.Pending&&!_runEconomy.Open&&!_mainMenu&&!_gameOver&&!_paused&&!_levelUp&&!_choosingEvolution&&!_stellaRewardPending&&_tutorialStep<0&&_playerCore!=null&&!_playerCore.IsDead;
        private void OpenNativeMerchant()
        {
            if(!CanOpenNativeMerchant||!_runEconomy.MerchantPlaced||Vector2.Distance(PlayerPosition,new Vector2(_runEconomy.MerchantX,_runEconomy.MerchantY))>26*SpherePlacementRules.SourcePixelsToWorld)return;
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
        private void DrawNativeMerchantUnit(){
            if(_runEconomy==null||!_runEconomy.Pending||!_runEconomy.MerchantPlaced||_mainMenu||_gameOver)return;
            var point=NativeSourcePixels(new Vector2(_runEconomy.MerchantX,_runEconomy.MerchantY));
            _nativeWorldMinions.DrawMerchant(_worldSurface,(float)point.x,(float)point.y,(int)ActiveCampaignRegion,_runTime);
        }
        private void PrepareNativeMerchant()
        {
            if(_nativeRunDocument==null)return;
            if(_nativeMerchantView==null)_nativeMerchantView=new NativeMerchantView(_nativeRunDocument.ControlsParent,OpenNativeMerchant,CloseNativeMerchant,BuyNativeMerchant);
            _nativeMerchantView.Bind(_runEconomy,_ownedArtifacts,IsRussianUi,_runTime,!_mainMenu&&!_gameOver&&_tutorialStep<0&&(_runEconomy!=null&&_runEconomy.Open||(!_userPaused&&!_levelUp&&!_choosingEvolution&&!_stellaRewardPending)),CanOpenNativeMerchant,IsLeftHanded);
        }
    }
}
