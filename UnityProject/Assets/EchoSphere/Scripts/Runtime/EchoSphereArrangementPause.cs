using UnityEngine.UIElements;
namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        // A gesture is transient input, never saved gameplay state or Time Stop.
        public bool IsSphereArrangementPaused
        {
            get
            {
                if(_mainMenu||_gameOver||_playerCore==null||_playerCore.IsDead)return false;
                foreach(var p in _nativeMobilePointers.Values)
                    if(p.Moved&&!p.Locked&&(p.Placing||(p.Sphere?.Agent!=null&&p.Sphere.Agent.gameObject.activeInHierarchy&&_spheres.Contains(p.Sphere.Agent))))return true;
                foreach(var p in _sourcePointers.Values)
                    if(p.moved&&!p.locked&&(p.placing||(p.target!=null&&p.target.gameObject.activeInHierarchy&&_spheres.Contains(p.target))))return true;
                return false;
            }
        }
        private void CancelSphereArrangement(){ResetNativeMobileGestures(false);ClearSourcePointers();_nativeMobileGhost=null;}
        private void ValidateSphereArrangement()
        {
            if(_mainMenu||_gameOver||(_paused&&_tutorialStep!=1&&_tutorialStep!=2)||_levelUp||_stellaRewardPending||(_playerCore!=null&&_playerCore.IsDead))
            {if(_nativeMobilePointers.Count>0||_sourcePointers.Count>0)CancelSphereArrangement();return;}
            int invalid=-1;bool found=false;
            foreach(var pair in _nativeMobilePointers){var h=pair.Value.Sphere;if(h!=null&&(h.Agent==null||!h.Agent.gameObject.activeInHierarchy||!_spheres.Contains(h.Agent))){invalid=pair.Key;found=true;break;}}
            if(found){NativeMobileAbort(invalid);_nativeMobileGestureRoot?.ReleasePointer(invalid);}
            foreach(var p in _sourcePointers.Values)if(!p.placing&&!p.movement&&(p.target==null||!p.target.gameObject.activeInHierarchy||!_spheres.Contains(p.target))){ClearSourcePointers();break;}
        }
    }
}
