using UnityEngine;
using EchoSphere.Core;

namespace EchoSphere.Runtime
{
    /// <summary>
    /// Boss behaviour port from src/engineEnemies.ts:updateEnemies.
    /// Spawn time remains controlled by the Region boss event, never by a timer here.
    /// </summary>
    public sealed class BossCombatAgent2D : MonoBehaviour
    {
        private EchoSphereRuntime _runtime;
        private Transform _player;
        private EnemyAgent2D _agent;
        private string _bossType = "";
        private int _wave;
        private float _moveSpeed, _damage, _shootTimer = 5f, _summonTimer = 4f, _chargeTimer = 3f;
        private float _auraRadius, _auraDps, _rotation;
        private bool _isCharging;
        private Vector2 _chargeDirection = Vector2.right;
        private Color _color = Color.white;

        public float ShootTimer => _shootTimer;
        public float SummonTimer => _summonTimer;
        public float ChargeTimer => _chargeTimer;
        public bool IsCharging => _isCharging;
        public Vector2 ChargeDirection => _chargeDirection;
        public float AuraRadius => _auraRadius;

        public void Initialize(EchoSphereRuntime runtime, Transform player, EnemyAgent2D agent,
            string bossType, int wave, float moveSpeed, float damage, Color color)
        {
            _runtime = runtime;
            _player = player;
            _agent = agent;
            _bossType = bossType ?? "";
            _wave = Mathf.Max(0, wave);
            _moveSpeed = Mathf.Max(0f, moveSpeed);
            _damage = Mathf.Max(0f, damage);
            _color = color;
            _auraRadius = _bossType == "aura" ? BossCombatRules.GetAuraRadiusWorld() : 0f;
            _auraDps = _bossType == "aura" ? BossCombatRules.GetAuraDamagePerSecond(_wave) : 0f;
            PushPresentationState();
        }

        public void Tick(float deltaTime)
        {
            if (_runtime == null || _player == null || _agent == null || !_agent.IsAlive || _runtime.IsGameplayPaused) return;
            var dt = Mathf.Max(0f, deltaTime);
            _rotation += dt;
            var playerPos = (Vector2)_player.position;
            var toPlayer = playerPos - (Vector2)transform.position;
            var distance = toPlayer.magnitude;
            transform.position = Vector2.MoveTowards(transform.position, playerPos, _moveSpeed * _agent.MovementSlowMultiplier * dt);
            _runtime.RefreshEnemyQuery(_agent);
            if (distance < _agent.SourceRadius * SpherePlacementRules.SourcePixelsToWorld + BossCombatRules.PlayerRadiusWorld) _runtime.TryDamagePlayer(_damage);

            if (_bossType == "charger")
            {
                _chargeTimer -= dt;
                if (_isCharging)
                {
                    if (_chargeTimer <= BossCombatRules.ChargerCommitSeconds)
                    {
                        transform.position += (Vector3)(_chargeDirection * _moveSpeed * 3f * dt);
                        _runtime.RefreshEnemyQuery(_agent);
                        if (Vector2.Distance(transform.position, playerPos) < BossCombatRules.BossRadiusWorld + BossCombatRules.PlayerRadiusWorld)
                        {
                            _runtime.TryDamagePlayer(_damage * 1.5f);
                            _isCharging = false;
                            _chargeTimer = 4f;
                        }
                    }
                    if (_isCharging && _chargeTimer <= 0f)
                    {
                        _isCharging = false;
                        _chargeTimer = 4f;
                    }
                }
                else if (_chargeTimer <= 0f)
                {
                    var dir = playerPos - (Vector2)transform.position;
                    _chargeDirection = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
                    _isCharging = true;
                    _chargeTimer = BossCombatRules.ChargerTotalTelegraphSeconds;
                }
            }
            else TickSpecialAttack(dt, playerPos, toPlayer, distance);

            if (_bossType == "aura" && Vector2.Distance(transform.position, playerPos) < _auraRadius)
                _runtime.TryDamagePlayerOverTime(_auraDps * dt);

            _runtime.TickSourceEnemyProjectiles(_agent, dt);
            PushPresentationState();
        }

        private void TickSpecialAttack(float dt, Vector2 playerPos, Vector2 toPlayer, float distance)
        {
            switch (_bossType)
            {
                case "shooter":
                    TickFan(dt, 5f, 3, 0.30f, 180f, 25f, 8f, toPlayer, "shooter");
                    break;
                case "summoner":
                    _summonTimer -= dt;
                    if (_summonTimer <= 0f)
                    {
                        _summonTimer = 5f;
                        for (var i = 0; i < 3; i++)
                        {
                            var a = _runtime.NextRunRandom() * Mathf.PI * 2f;
                            var at = (Vector2)transform.position + new Vector2(Mathf.Cos(a), -Mathf.Sin(a)) * BossCombatRules.ToWorldUnits(60f);
                            _runtime.SpawnBossMinion(at, 20f + _wave * 4f, BossCombatRules.ToWorldUnits(100f), 2, "wisp", ParseColor("#8a5a8a"));
                        }
                    }
                    TickFan(dt, 7f, 3, 0.30f, 180f, 20f, 8f, toPlayer, "summoner");
                    break;
                case "aura":
                    TickFan(dt, 6f, 3, 0.30f, 180f, 20f, 8f, toPlayer, "aura");
                    break;
                case "conductor":
                    _summonTimer -= dt;
                    _chargeTimer -= dt;
                    TickFan(dt, 4f, 5, 0.18f, 205f, 22f, 7f, toPlayer, "conductor");
                    if (_summonTimer <= 0f)
                    {
                        _summonTimer = 7f;
                        _runtime.DisableNearestNetworkSpheres(transform.position, 2, 1.6f, false);
                        _runtime.ShowBossMessage("CONDUCTOR BREAK");
                    }
                    if (_chargeTimer <= 0f)
                    {
                        _chargeTimer = 6f;
                        _runtime.ShowBossMessage("CONDUCTOR SURGE");
                        if (distance < BossCombatRules.ToWorldUnits(260f)) _runtime.TryDamagePlayer(_damage * 1.35f);
                    }
                    break;
                case "architect":
                    _summonTimer -= dt;
                    _chargeTimer -= dt;
                    TickRadial(dt, 4.5f, 6, 0.25f, 165f, 18f, 7f);
                    if (_summonTimer <= 0f)
                    {
                        _summonTimer = 7f;
                        var a = _runtime.NextRunRandom() * Mathf.PI * 2f;
                        transform.position = playerPos + new Vector2(Mathf.Cos(a), -Mathf.Sin(a)) * BossCombatRules.ToWorldUnits(360f);
                        _runtime.RefreshEnemyQuery(_agent);
                        _runtime.DrainResonance(12f);
                        _runtime.ShowBossMessage("GEOMETRY SHIFT");
                    }
                    if (_chargeTimer <= 0f)
                    {
                        _chargeTimer = 6f;
                        _runtime.DisableNearestNetworkSpheres(transform.position, 1, 1.2f, true);
                        _runtime.ShowBossMessage("ARCHITECT SEAL");
                    }
                    break;
                case "null":
                    _summonTimer -= dt;
                    _chargeTimer -= dt;
                    TickFan(dt, 3.5f, 5, 0.24f, 140f, 28f, 8f, toPlayer, "null");
                    if (_summonTimer <= 0f)
                    {
                        _summonTimer = 6f;
                        _runtime.DrainResonance(25f);
                        _runtime.DelayAllSphereAttacks(0.45f);
                        _runtime.ShowBossMessage("NULL FIELD");
                    }
                    if (_chargeTimer <= 0f)
                    {
                        _chargeTimer = 5f;
                        if (distance < BossCombatRules.ToWorldUnits(180f)) _runtime.TryDamagePlayerOverTime(18f * dt);
                        _runtime.ShowBossMessage("NULL SHOCK");
                    }
                    break;
                case "stella_warden":
                    _summonTimer -= dt;
                    _chargeTimer -= dt;
                    TickFan(dt, 3.5f, 5, 0.16f, 190f, 24f, 8f, toPlayer, "stella_warden");
                    if (_summonTimer <= 0f)
                    {
                        _summonTimer = 8f;
                        for (var i = 0; i < 2; i++)
                        {
                            var a = _runtime.NextRunRandom() * Mathf.PI * 2f;
                            var at = (Vector2)transform.position + new Vector2(Mathf.Cos(a), -Mathf.Sin(a)) * BossCombatRules.ToWorldUnits(55f);
                            _runtime.SpawnBossMinion(at, 18f + _wave * 3f, BossCombatRules.ToWorldUnits(85f), 3, "moth", ParseColor("#d4943d"));
                        }
                        _runtime.ShowBossMessage("STELLA GUARD");
                    }
                    if (_chargeTimer <= 0f)
                    {
                        _chargeTimer = 6f;
                        if (distance < BossCombatRules.ToWorldUnits(240f)) _runtime.TryDamagePlayer(_damage * 1.20f);
                        _runtime.ShowBossMessage("STELLA JUDGEMENT");
                    }
                    break;
            }
        }

        private void TickFan(float dt, float interval, int count, float step, float sourceSpeed,
            float damage, float sourceRadius, Vector2 toPlayer, string visualType)
        {
            _shootTimer -= dt;
            if (_shootTimer > 0f) return;
            _shootTimer = interval;
            var angle = toPlayer.sqrMagnitude > 0.0001f ? Mathf.Atan2(toPlayer.y, toPlayer.x) : 0f;
            for (var i = 0; i < count; i++)
            {
                var offset = (i - (count - 1) * 0.5f) * step;
                var dir = new Vector2(Mathf.Cos(angle - offset), Mathf.Sin(angle - offset));
                _runtime.SpawnBossProjectile(transform.position, dir * BossCombatRules.ToWorldUnits(sourceSpeed), damage, sourceRadius, visualType, _color, _agent);
            }
        }

        private void TickRadial(float dt, float interval, int count, float phaseMultiplier, float sourceSpeed, float damage, float sourceRadius)
        {
            _shootTimer -= dt;
            if (_shootTimer > 0f) return;
            _shootTimer = interval;
            for (var i = 0; i < count; i++)
            {
                var a = i * Mathf.PI * 2f / count + _rotation * phaseMultiplier;
                var dir = new Vector2(Mathf.Cos(a), -Mathf.Sin(a));
                _runtime.SpawnBossProjectile(transform.position, dir * BossCombatRules.ToWorldUnits(sourceSpeed), damage, sourceRadius, "architect", _color, _agent);
            }
        }

        private void PushPresentationState()
        {
        }

        private static Color ParseColor(string value) => ColorUtility.TryParseHtmlString(value, out var color) ? color : Color.white;
    }
}
