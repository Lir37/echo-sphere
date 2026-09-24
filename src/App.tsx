            <span className="text-[#dcecff]">{network.links.length}</span>
          </div>
          <div className="mt-1.5 flex flex-wrap gap-1">
            {networkBadges.length > 0 ? networkBadges.map((badge) => (
              <span key={badge.label} className={`rounded-md border px-1.5 py-0.5 text-[8px] font-bold tracking-wide ${badge.className}`}>
                {badge.label}
              </span>
            )) : (
              <span className="rounded-md border border-[#243b55] px-1.5 py-0.5 text-[8px] font-bold text-[#7f9bb8]">
                {lang === 'ru' ? 'ОЖИДАНИЕ ФОРМАЦИИ' : 'AWAITING FORMATION'}
              </span>
            )}
          </div>
        </div>
      </div>

      <div className="es-time-hud absolute top-3 left-1/2 -translate-x-1/2 z-30 pointer-events-none">
        <div className="es-time-hud-line">
          <span className="es-time-hud-dot" />
          <span className="es-time-hud-value">{timer}</span>
          <span className="es-time-hud-dot" />
        </div>
        <div className="es-time-hud-phase">{activeBoss ? 'VOID BREACH // BOSS' : 'ECHO FIELD // ACTIVE'}</div>
      </div>

      <div className="es-hud-panel es-top-right absolute top-3 right-3 z-30 pointer-events-none">
        <div className="flex items-start gap-3">
          <RadarHud st={st} />
          <div className="min-w-[64px] pt-1 text-right">
            <div className="es-hud-stat"><span className="es-stat-gem">◆</span>{Math.floor(st.xpOrbs.reduce((sum, orb) => sum + orb.radius, 0))}</div>
            <div className="es-hud-stat text-[#c8b7ff]"><span className="es-stat-gem">◇</span>{st.player.kills}</div>
            {st.player.buffTimer > 0 && <div className="es-hud-buff">{Math.ceil(st.player.buffTimer)}s</div>}
          </div>
        </div>
        {activeBoss && <div className="es-boss-telemetry mt-2">{t('bossWave')}</div>}
      </div>
    </>
  );
}

function RadarHud({ st }: { st: GameState }) {
  const radius = 900;
  const dots = st.enemies.filter((enemy) => enemy.hp > 0).slice(0, 40).map((enemy, index) => {
    const dx = enemy.pos.x - st.player.pos.x;
    const dy = enemy.pos.y - st.player.pos.y;
    const distance = Math.hypot(dx, dy);
    const scale = Math.min(1, distance / radius);
    const angle = Math.atan2(dy, dx);
    const rr = scale * 25;
    return {
      key: `${enemy.type}-${enemy.pos.x}-${index}`,
      x: 50 + Math.cos(angle) * rr,
      y: 50 + Math.sin(angle) * rr,
      boss: enemy.isBoss,
      elite: enemy.isElite,
    };
  });

  return (
    <div className="es-radar" aria-hidden="true">
      <span className="es-radar-ring es-radar-ring-1" />
      <span className="es-radar-ring es-radar-ring-2" />
      <span className="es-radar-cross-h" />
      <span className="es-radar-cross-v" />
      {dots.map((dot) => (
        <span
          key={dot.key}
          className={dot.boss ? 'es-radar-dot boss' : dot.elite ? 'es-radar-dot elite' : 'es-radar-dot'}
          style={{ left: `${dot.x}%`, top: `${dot.y}%` }}
        />
      ))}