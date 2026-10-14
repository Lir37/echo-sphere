using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private string _sourceRegionBossForFrame;
        private void TickSourceRegionFrame(float dt)
        {
            _sourceRegionBossForFrame = null;
            if (_tutorialStep >= 0) return;
            if (_playerCore != null)
            {
                var hp = _playerCore.CurrentHp;
                if (_regionLastHp < 0f) _regionLastHp = hp;
                else { if (hp < _regionLastHp) _regionDamageTaken += _regionLastHp - hp; _regionLastHp = hp; }
            }
            _regionFieldVisuals.TickSourceRegionState();
            AdvanceRegionBossSchedule();
            TickRegionMastery(dt);
            if (string.IsNullOrEmpty(_sourceRegionBossForFrame)) TickSourceRegionFracture();
        }
        private void SpawnSourceRegionBossForFrame()
        { if (!string.IsNullOrEmpty(_sourceRegionBossForFrame)) SpawnBossForRegionEvent(_sourceRegionBossForFrame, _wave); }
    }
}
