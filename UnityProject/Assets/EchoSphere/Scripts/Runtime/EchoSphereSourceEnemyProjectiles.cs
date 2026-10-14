using System.Collections.Generic;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        // engineEnemies.ts advances each enemy's own projectile array during that enemy's turn.
        public void TickSourceEnemyProjectiles(EnemyAgent2D owner, float dt)
        {
            var bullets = new List<BossProjectileAgent2D>();
            foreach (var agent in _simulationAgents) if (agent is BossProjectileAgent2D bullet && bullet.HasSourceOwner && bullet.SourceOwner == owner) bullets.Add(bullet);
            for (var i = bullets.Count - 1; i >= 0; i--) if (bullets[i] != null && bullets[i].gameObject.activeInHierarchy) bullets[i].SimulationTick(dt);
        }
        private void TickSourceUnownedEnemyProjectiles(float dt)
        {
            var bullets = new List<BossProjectileAgent2D>();
            foreach (var agent in _simulationAgents) if (agent is BossProjectileAgent2D bullet && !bullet.HasSourceOwner) bullets.Add(bullet);
            for (var i = bullets.Count - 1; i >= 0; i--) if (bullets[i] != null && bullets[i].gameObject.activeInHierarchy) bullets[i].SimulationTick(dt);
        }
        private void RemoveSourceEnemyProjectiles(EnemyAgent2D owner)
        {
            foreach (var agent in _simulationAgents) if (agent is BossProjectileAgent2D bullet && bullet.HasSourceOwner && bullet.SourceOwner == owner)
            { bullet.gameObject.SetActive(false); Destroy(bullet.gameObject); }
        }
    }
}
