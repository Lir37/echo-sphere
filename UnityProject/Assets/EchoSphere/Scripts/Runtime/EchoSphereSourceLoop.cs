using System;
using System.Collections.Generic;
using EchoSphere.Core;
using UnityEngine;

namespace EchoSphere.Runtime
{
    public sealed partial class EchoSphereRuntime
    {
        private int _mutationStage;
        private readonly List<EnemyAgent2D> _spaceCollisionAgents=new List<EnemyAgent2D>();
        private readonly List<CollisionBody> _spaceCollisionBodies=new List<CollisionBody>();
        private readonly List<CollisionBody> _spaceCollisionBodyPool=new List<CollisionBody>();
        private readonly List<Vec2> _spaceCollisionTowers=new List<Vec2>();
        private readonly SpaceCollisionRules.Workspace _spaceCollisionWorkspace=new SpaceCollisionRules.Workspace();
        private float _chaosOrbTimer, _chaosOrbBuffTimer, _buffTimer, _deathTimer;
        private string _chaosOrbBuff;
        private float ChaosOrbMultiplier(string kind) => _chaosOrbBuff == kind && _chaosOrbBuffTimer > 0f ? 1.2f : 1f;
        private void TrySourceDash(bool useTouchDirection = false)
        {
            if (_follow) { ShowTimedCombatMessage("FOLLOW: DASH LOCKED", 0.55f); return; }
            var direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) + (_sourceUi!=null?NativeMobileMovement:_sourceJoystickDirection);
            if (useTouchDirection && _playerCore != null)
            {
                var remembered = _sourceTouchLastDirection;
                direction = new Vector2(Mathf.Abs(remembered.x) > .2f ? Mathf.Sign(remembered.x) : 0f, Mathf.Abs(remembered.y) > .2f ? Mathf.Sign(remembered.y) : 0f);
            }
            if (_playerCore != null && _playerCore.TryDash(direction)) SourceAudio2D.Play("dash");
        }
        private void TickSourceEarlyPlayerState(float dt, bool dashWasActive)
        {
            _buffTimer = Mathf.Max(0f, _buffTimer - dt);
            if (dashWasActive)
            {
                _player.position += (Vector3)(_playerCore.DashDirection * (600f * SpherePlacementRules.SourcePixelsToWorld * dt));
                if (_rng.NextFloat() < 0.5f) SpawnSourceParticle(PlayerPosition, Vector2.zero, 0.3f, 3f, "#d4943d");
            }
            TickSourceDamageNumbers(dt);
        }
        private void TickSourceMutation()
        {
            var stage = PlayerLoopRules.MutationStage(_level);
            if (stage > _mutationStage)
            {
                _mutationStage = stage;
                ShowTimedCombatMessage("MUTATION!", 2f, "#b8475a");
                var colors = new[] { "#8a5a8a", "#c4453d", "#d4943d", "#e8dcc0" };
                for (var i = 0; i < 40; i++)
                {
                    var a = _rng.NextFloat() * Mathf.PI * 2f;
                    var vx = Mathf.Cos(a) * (100f + _rng.NextFloat() * 150f);
                    var vy = Mathf.Sin(a) * (100f + _rng.NextFloat() * 150f);
                    SpawnSourceParticle(PlayerPosition, new Vector2(vx, -vy), 1f, 3f + _rng.NextFloat() * 3f, colors[stage - 1]);
                }
                RefreshAllSphereProgression();
            }
        }
        private void TickSourceLatePlayerState(float dt)
        {
            _playerCore.Heal(PlayerLoopRules.MutationRegen(_mutationStage) * dt);
            if (_ownedArtifacts.Contains("chaos_orb"))
            {
                _chaosOrbTimer += dt;
                if (_chaosOrbTimer >= 10f)
                {
                    _chaosOrbTimer = 0f;
                    _chaosOrbBuff = _rng.NextFloat() < 0.5f ? "dmg" : "radius";
                    _chaosOrbBuffTimer = 3f;
                    RefreshAllSphereProgression();
                }
            }
            if (_chaosOrbBuffTimer > 0f)
            {
                _chaosOrbBuffTimer -= dt;
                if (_chaosOrbBuffTimer <= 0f) RefreshAllSphereProgression();
            }
        }
        private void RefreshAllSphereProgression()
        { foreach (var sphere in _spheres) if (sphere != null) sphere.ApplyProgressionLevel(sphere.ProgressionLevel); }
        private bool TickSourceDeath()
        {
            if (_mainMenu || _playerCore == null || (_deathTimer <= 0f && !_playerCore.IsDead) || _gameOver) return false;
            if (_deathTimer <= 0f) _deathTimer = PlayerLoopRules.DeathDuration;
            _deathTimer = Mathf.Max(0f, _deathTimer - Mathf.Min(0.05f, Time.deltaTime));
            if (_deathTimer <= 0f)
            {
                _gameOver = true; _paused = true;
                SourceAudio2D.Play("gameover"); FinalizeRunResult();
            }
            return true;
        }
        private void ResolveSourceSpaceCollisions(float dt)
        {
            if (_mainMenu || _playerCore == null) return;
            var scale = SpherePlacementRules.SourcePixelsToWorld;
            var agents = _spaceCollisionAgents; var bodies = _spaceCollisionBodies;
            agents.Clear();bodies.Clear();
            foreach (var e in _enemies)
            {
                if (e == null || !e.IsAlive) continue;
                agents.Add(e);
                if(_spaceCollisionBodyPool.Count<=bodies.Count)_spaceCollisionBodyPool.Add(new CollisionBody());
                var body=_spaceCollisionBodyPool[bodies.Count];
                body.Position=new Vec2(e.transform.position.x / scale, -e.transform.position.y / scale);
                body.Radius=e.SourceRadius;body.Speed=e.SourceSpeed;body.Mass=e.CollisionMass;body.FollowBreach=e.Role=="charger"||e.Role=="phase";
                bodies.Add(body);
            }
            var towers = _spaceCollisionTowers;towers.Clear();
            foreach (var s in _spheres) if (s != null) towers.Add(new Vec2(s.transform.position.x / scale, -s.transform.position.y / scale));
            var p = SpaceCollisionRules.ResolveRetained(new Vec2(_player.position.x / scale, -_player.position.y / scale), bodies, towers, dt, _follow,_spaceCollisionWorkspace);
            _player.position = new Vector3(p.X * scale, -p.Y * scale, _player.position.z);
            for (var i = 0; i < agents.Count; i++){agents[i].transform.position = new Vector3(bodies[i].Position.X * scale, -bodies[i].Position.Y * scale, agents[i].transform.position.z);RefreshEnemyQuery(agents[i]);}
            if (_follow) foreach (var sphere in _spheres) if (sphere != null) sphere.SyncFollowPosition();
        }
        public void SpawnSourceParticle(Vector2 position, Vector2 sourceVelocity, float life, float size, string htmlColor, float maxLife = 0f)
        {
            ColorUtility.TryParseHtmlString(htmlColor, out var color);
            var particle=TransientPools.Particle();var go=particle.gameObject;go.transform.position=position;
            go.transform.localScale = Vector3.one * (size * 2f * SpherePlacementRules.SourcePixelsToWorld);
            particle.Initialize(this, sourceVelocity * SpherePlacementRules.SourcePixelsToWorld, life, color, maxLife,size);
        }
        private void SpawnSourceSplitChildren(EnemyAgent2D parent, bool elite)
        {
            var radius = elite ? 10f : 9f; var offset = elite ? 18f : 14f;
            var hp = Mathf.Max(elite ? 8f : 5f, parent.MaxHp * (elite ? 0.18f : 0.16f));
            for (var i = 0; i < 2; i++)
            {
                var a = _rng.NextFloat() * Mathf.PI * 2f;
                var position = (Vector2)parent.transform.position + new Vector2(Mathf.Cos(a), -Mathf.Sin(a)) * (offset * SpherePlacementRules.SourcePixelsToWorld);
                var go = CreateEntityRoot("Splitter Child", position, radius * 2f * SpherePlacementRules.SourcePixelsToWorld);
                var child = go.AddComponent<EnemyAgent2D>();
                child.Initialize(this, _player, hp, parent.SourceSpeed * 1.15f * SpherePlacementRules.SourcePixelsToWorld,
                    elite ? 2 : 1, "skitter", "", false, EnemyArchetype.Fast, false, elite ? "grunt" : "swarmer", 1f, 1f, "", parent.ContactDamage * 0.45f);
                child.SetSourceAppearance(new Color(.54f,.35f,.54f)); child.SetSourceRadius(radius);child.SetSourceVisualTier(parent.SourceVisualTier); _enemies.Add(child); RegisterEnemyQuery(child);
            }
            if (elite) ShowTimedCombatMessage("SPLITTER", 0.6f);
        }
        private void ApplySourceEnemyDeathEffects(EnemyAgent2D enemy)
        {
            if (enemy == null) return;
            if (enemy.IsElite && enemy.EliteVariant == "splitter_prime") SpawnSourceSplitChildren(enemy, true);
            if (!enemy.IsBoss && enemy.Role == "bomber")
            {
                var center = (Vector2)enemy.transform.position;
                foreach (var target in new List<EnemyAgent2D>(_enemies))
                    if (target != null && target != enemy && target.IsAlive && Vector2.Distance(target.transform.position, center) <= 95f * SpherePlacementRules.SourcePixelsToWorld) target.ReceiveDamage(12f + _level);
                if (Vector2.Distance(PlayerPosition, center) <= 95f * SpherePlacementRules.SourcePixelsToWorld) TryDamagePlayer(18f + _level * 0.5f);
                _characterScreenShakeIntensity = Mathf.Min(0.16f, _characterScreenShakeIntensity + 0.035f);
            }
            if (!enemy.IsBoss && enemy.Role == "splitter") SpawnSourceSplitChildren(enemy, false);
            if (_sphereCharacterId == SphereCharacterId.Berserker && _characterMasteryLevel >= 5 && Vector2.Distance(PlayerPosition, enemy.transform.position) <= 110f * SpherePlacementRules.SourcePixelsToWorld) _buffTimer = Mathf.Max(_buffTimer, 2f);
            var count = enemy.IsBoss ? 56 : enemy.IsElite ? 18 : 10;
            for (var i = 0; i < count; i++)
            {
                var vx = -180f + _rng.NextFloat() * 360f; var vy = -180f + _rng.NextFloat() * 360f;
                var life = 0.3f + _rng.NextFloat() * 0.5f; var size = 2f + _rng.NextFloat() * 3f;
                SpawnSourceParticle(enemy.transform.position, new Vector2(vx, -vy), life, size, enemy.SourceColorHtml, 0.8f);
            }
        }
    }
}
