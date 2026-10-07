import { useRef, useState, type PointerEvent as ReactPointerEvent } from 'react';
import {
  REGION_CHALLENGES,
  getRegionChallengeProgress,
  loadRegionChallengeCompletions,
  loadRegionEndlessUnlock,
  loadRegionStabilized,
  type RegionChallengeId,
  type RegionMode,
} from './region';
import type { Difficulty } from './gameData';
import type { Lang } from './i18n';

type StartRun = (mode: RegionMode, challenge: RegionChallengeId) => void;

const REGION_BODIES = [
  { id: 'resonance_basin', x: 50, y: 51, scale: 1, active: true, color: '#63e6ff' },
  { id: 'unresolved_a', x: 18, y: 29, scale: .56, active: false, color: '#6fa9ff' },
  { id: 'unresolved_b', x: 80, y: 26, scale: .46, active: false, color: '#9d83ff' },
  { id: 'unresolved_c', x: 76, y: 78, scale: .62, active: false, color: '#6fe7cf' },
  { id: 'unresolved_d', x: 23, y: 76, scale: .40, active: false, color: '#e7aa63' },
] as const;

const CHALLENGE_GLYPHS = ['fracture', 'overload', 'gravity'] as const;

export default function EchoMapScreen({
  lang,
  difficulty,
  onStartRun,
  onBack,
  onCharacters: _onCharacters,
}: {
  lang: Lang;
  difficulty: Difficulty;
  onStartRun: StartRun;
  onBack: () => void;
  onCharacters?: () => void;
}) {
  const [focused, setFocused] = useState(false);
  const [offset, setOffset] = useState({ x: 0, y: 0 });
  const dragRef = useRef<{ id: number; startX: number; startY: number; baseX: number; baseY: number; moved: boolean } | null>(null);

  const completed = loadRegionChallengeCompletions();
  const progress = getRegionChallengeProgress();
  const endlessUnlocked = loadRegionEndlessUnlock();
  const stabilized = loadRegionStabilized();

  const onMapPointerDown = (event: ReactPointerEvent<HTMLDivElement>) => {
    dragRef.current = {
      id: event.pointerId,
      startX: event.clientX,
      startY: event.clientY,
      baseX: offset.x,
      baseY: offset.y,
      moved: false,
    };
    event.currentTarget.setPointerCapture(event.pointerId);
  };

  const onMapPointerMove = (event: ReactPointerEvent<HTMLDivElement>) => {
    const drag = dragRef.current;
    if (!drag || drag.id !== event.pointerId) return;
    const dx = event.clientX - drag.startX;
    const dy = event.clientY - drag.startY;
    if (Math.hypot(dx, dy) > 5) drag.moved = true;
    setOffset({
      x: Math.max(-28, Math.min(28, drag.baseX + dx)),
      y: Math.max(-20, Math.min(20, drag.baseY + dy)),
    });
  };

  const onMapPointerUp = (event: ReactPointerEvent<HTMLDivElement>) => {
    if (dragRef.current?.id === event.pointerId) {
      dragRef.current = null;
      try { event.currentTarget.releasePointerCapture(event.pointerId); } catch { /* pointer may already be released */ }
    }
  };

  const onRegionClick = (event: ReactPointerEvent<HTMLButtonElement>) => {
    event.stopPropagation();
    const drag = dragRef.current;
    if (drag?.moved) return;
    if (!focused) setFocused(true);
    else onStartRun('stabilization', 'none');
  };

  return (
    <div className={focused ? 'es-map-screen is-region-open' : 'es-map-screen is-region-overview'}>
      <header className="es-map-header">
        <button
          type="button"
          className="es-map-back"
          onClick={() => focused ? setFocused(false) : onBack()}
          aria-label={focused ? (lang === 'ru' ? 'Вернуться к звёздной карте' : 'Back to star map') : (lang === 'ru' ? 'Вернуться в меню' : 'Back to menu')}
        >
          <span>‹</span>{focused ? (lang === 'ru' ? 'К ЗВЁЗДНОЙ КАРТЕ' : 'STAR MAP') : (lang === 'ru' ? 'МЕНЮ' : 'MENU')}
        </button>
        <div className="es-map-title-cluster" aria-hidden="true">
          <div className="es-map-title">ECHO MAP</div>
          <div className="es-map-subtitle">{focused ? 'REGION FOCUS' : 'STAR FIELD'}</div>
        </div>
      </header>

      <main
        className="es-map-main"
        onPointerDown={onMapPointerDown}
        onPointerMove={onMapPointerMove}
        onPointerUp={onMapPointerUp}
        onPointerCancel={onMapPointerUp}
        style={{ touchAction: 'none' }}
      >
        <div className="es-map-starfield" style={{ transform: `translate3d(${offset.x}px,${offset.y}px,0)` }}>
          <div className="es-map-stars" />
          <div className="es-map-nebula nebula-a" />
          <div className="es-map-nebula nebula-b" />
          <div className="es-map-grid" />

          <div className="es-map-constellation-lines" aria-hidden="true">
            <span />
            <span />
            <span />
            <span />
            <span />
          </div>

          {REGION_BODIES.map((body, index) => {
            const isMain = body.id === 'resonance_basin';
            const bodyScale = focused && isMain ? 1.46 : body.scale;
            return (
              <div
                key={body.id}
                className={'es-map-celestial-body ' + (isMain ? 'is-main' : 'is-distant') + (focused && isMain ? ' is-focused' : '')}
                style={{
                  left: body.x + '%',
                  top: body.y + '%',
                  ['--body-scale' as string]: String(bodyScale),
                  ['--body-color' as string]: body.color,
                }}
              >
                <span className="es-map-celestial-atmosphere" />
                <span className="es-map-celestial-ring ring-a" />
                <span className="es-map-celestial-ring ring-b" />

                {isMain && CHALLENGE_GLYPHS.map((glyph, index) => {
                  const challenge = REGION_CHALLENGES[index];
                  const done = completed.includes(challenge.id);
                  const locked = !stabilized;
                  const start = index * 120 - 90;
                  return (
                    <button
                      key={glyph}
                      type="button"
                      className={'es-map-orbit-challenge challenge-' + index + (locked ? ' is-locked' : '') + (done ? ' is-complete' : '')}
                      style={{ ['--challenge-angle' as string]: start + 'deg', ['--challenge-color' as string]: challenge.id === 'fractured_network' ? '#ff6b6b' : challenge.id === 'overload' ? '#ffbd72' : '#aa91ff' }}
                      onPointerDown={(e) => e.stopPropagation()}
                      onClick={(e) => { e.stopPropagation(); if (!locked) onStartRun('stabilization', challenge.id); }}
                      aria-label={challenge.name[lang]}
                    >
                      <span className="es-map-challenge-glyph">
                        {glyph === 'fracture' ? '⟐' : glyph === 'overload' ? '✦' : '⌁'}
                      </span>
                    </button>
                  );
                })}

                {isMain && (
                  <>
                    <button
                      type="button"
                      className="es-map-region-body"
                      onPointerDown={(e) => e.stopPropagation()}
                      onClick={onRegionClick}
                      aria-label={lang === 'ru'
                        ? (focused ? 'Начать стандартную стабилизацию' : 'Приблизить Резонансный бассейн')
                        : (focused ? 'Start Standard Stabilization' : 'Approach Resonance Basin')}
                    >
                      <span className="es-map-region-core" />
                      <span className="es-map-region-surface" />
                    </button>
                    {focused && endlessUnlocked && (
                      <button
                        type="button"
                        className="es-map-endless-node"
                        onPointerDown={(e) => e.stopPropagation()}
                        onClick={(e) => { e.stopPropagation(); onStartRun('endless', 'none'); }}
                        aria-label={lang === 'ru' ? 'Бесконечное ядро' : 'Endless Core'}
                      >
                        <span>✦</span>
                      </button>
                    )}
                  </>
                )}
              </div>
            );
          })}

          <div className="es-map-progress-rune" aria-hidden="true">
            <span className={stabilized ? 'is-on' : ''}>01</span>
            <i />
            <span className={progress >= 1 ? 'is-on' : ''}>02</span>
            <i />
            <span className={endlessUnlocked ? 'is-on' : ''}>∞</span>
          </div>
        </div>

        <div className="es-map-gesture-hint" aria-hidden="true">
          <span>{focused ? '✦' : '◇'}</span>
          <small>{focused ? (lang === 'ru' ? 'СПУТНИКИ ИСПЫТАНИЙ' : 'CHALLENGE SATELLITES') : (lang === 'ru' ? 'ПЕРЕМЕЩАЙ ПАЛЬЦЕМ · ВЫБЕРИ МИР' : 'DRAG TO TRAVEL · CHOOSE A WORLD')}</small>
        </div>
      </main>
    </div>
  );
}
