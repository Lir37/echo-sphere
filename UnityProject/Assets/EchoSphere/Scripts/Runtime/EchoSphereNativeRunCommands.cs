using System;
using System.Linq;
using EchoSphere.Core;
using EchoSphere.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        // Typed entry points shared by native controls and the remaining legacy UI
        // adapter. They retain the original game methods and refresh boundaries.
        private void RefreshNativeCommandModels(){UpdateSourceRenderState();UpdateSourceUiState();}
        private bool NativeCanActivateFollow()=>FormationFollowRules.CanActivate(new Vec2(_player.position.x,_player.position.y),_spheres.Where(agent=>agent!=null).Select(agent=>new FormationNode(new Vec2(agent.transform.position.x,agent.transform.position.y),true)).ToList());
        private bool NativeSetFormationFollow(bool active)
        {bool followed=SetFormationFollowMode(active);RefreshNativeCommandModels();return followed;}
        private void NativeActivateHotkey(string key)
        {foreach(var binding in _activeAbilityHotkeys)if(binding.Value.ToString().Replace("Alpha","").ToLowerInvariant()==key)ActivateActiveAbility(binding.Key);RefreshNativeCommandModels();}
        private void NativeActivateDash()
        {if(_playerCore.TryDash(NativeMobileMovement))SourceAudio2D.Play("dash");RefreshNativeCommandModels();}
        private void NativeSelectSphere(string id)
        {if(Enum.TryParse<SphereId>(id,true,out var type)){_selectedSphereType=type;SourceAudio2D.Play("place");}RefreshNativeCommandModels();}
        private void NativePlaceOrRemoveSphere(double x,double y)
        {TryPlaceOrRemoveSphereAt(new Vector2((float)x,-(float)y)*SpherePlacementRules.SourcePixelsToWorld);RefreshNativeCommandModels();}
        private SphereAttackAgent NativeSphereForView(V view)=>_spheres.Find(agent=>agent!=null&&RenderEntity(agent)==view);
        private bool NativeRepositionSphere(SphereAttackAgent sphere,double x,double y)
        {
            if(sphere==null)return false;var position=new Vector2((float)x,-(float)y)*SpherePlacementRules.SourcePixelsToWorld;if(!CanPlaceSphereAt(position,sphere))return false;
            var clamped=SpherePlacementRules.ClampReposition(new Vec2(position.x,position.y));sphere.transform.position=new Vector3(clamped.X,clamped.Y,sphere.transform.position.z);sphere.SetFollowMode(_follow,(Vector2)_player.position);RefreshNetworkState();UpdateSourceRenderState();return true;
        }
        private void NativeSetSphereMovementLocked(bool locked)
        {_sphereMovementLocked=locked;if(_sourceRenderState.Truth){var player=_sourceRenderState["player"];player["sphereMovementLocked"]=locked;}}
        private bool NativeSwapFormationSlots()
        {
            // Use the same last projected network the original HUD action read;
            // choosing a newer simulation network here could swap different slots.
            var network=_sourceRenderState["nativeNetwork"];var dominant=network["dominantFormation"]["type"];var secondary=network["secondaryFormation"]["type"];if(!dominant.Truth||!secondary.Truth)return false;
            if(!Enum.TryParse<SphereNetworkFormation>(dominant.Text,true,out var first)||!Enum.TryParse<SphereNetworkFormation>(secondary.Text,true,out var second))return false;
            _preferredDominantFormation=second;_preferredSecondaryFormation=first;RefreshNetworkState();
            if(_sourceRenderState.Truth)_sourceRenderState["networkFormationSelection"]=VisualJs.Null;
            RefreshNativeCommandModels();if(_sourceUi!=null)_sourceUi.Dirty=true;return true;
        }
        private void NativeResumeRun()
        {
            if(!UsesNativePauseChrome)return;SetUserPaused(false);if(_sourceRenderState.Truth)_sourceRenderState["paused"]=false;SaveCurrentRun();_pauseArtifactSetId=null;
            if(_sourceUi!=null)_sourceUi.Dirty=true;
        }
        private void NativeExitRun()
        {
            if(!UsesNativePauseChrome)return;ClearRunSnapshot();PlayerPrefs.SetInt("echo-sphere:open_region_select",1);PlayerPrefs.Save();SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        private void NativeSelectPauseTab(int index)
        {if(!UsesNativePauseChrome)return;_pausePlannerTab=Math.Max(0,Math.Min(3,index));if(_sourceUi!=null)_sourceUi.Dirty=true;}
    }
}
