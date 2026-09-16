import { useEffect, useRef, useState } from 'react';
import { Pause, Zap } from 'lucide-react';
import { ABILITIES, SPHERE_TYPES, type AbilityType, type SphereType } from './gameData';
import {
  activateByKey,
  activateDash,
  getMaxSpheres,
  placeSphere,
  setSphereType,
  type GameState,
} from './engine';
import type { Lang, TranslationKey } from './i18n';
import { loadInterfaceScale } from './interfaceScale';

export type Handedness = 'right' | 'left';

type PointerState = { startX: number; startY: number; moved: boolean; joystickCandidate: boolean };
type JoystickVisual = { pointerId: number; x: number; y: number; dx: number; dy: number; active: boolean };

const JOYSTICK_DEADZONE = 12;
const JOYSTICK_RADIUS = 58;
const JOYSTICK_KNOB_RADIUS = 24;
const TOWER_TOUCH_TOLERANCE = 26;

function isBlockedByControl(target: EventTarget | null): boolean {
  return target instanceof Element && Boolean(target.closest('[data-mobile-control="true"]'));
}

export default function MobileControls({ lang, t, stateRef, canvasRef, handedness, onPause }: {
  lang: Lang;
  t: (key: TranslationKey) => string;
  stateRef: React.MutableRefObject<GameState | null>;
  canvasRef: React.RefObject<HTMLCanvasElement | null>;
  handedness: Handedness;
  onPause: () => void;
}) {
  const pointersRef = useRef<Map<number, PointerState>>(new Map());
  const joystickIdRef = useRef<number | null>(null);
  const lastDirectionRef = useRef({ x: 0, y: -1 });
  const [joystick, setJoystick] = useState<JoystickVisual | null>(null);
  const interfaceScale = loadInterfaceScale();

  const controlsOnRight = handedness === 'right';
  const controlsSide = controlsOnRight ? 'right-3' : 'left-3';
  const controlsAlign = controlsOnRight ? 'items-end' : 'items-start';
  const joystickZoneStart = controlsOnRight ? window.innerWidth * 0.5 : 0;
  const joystickZoneEnd = controlsOnRight ? window.innerWidth : window.innerWidth * 0.5;

  const clearMovementKeys = () => {
    const st = stateRef.current;
    if (!st) return;
    st.keys.w = false; st.keys.a = false; st.keys.s = false; st.keys.d = false;
    st.keys.arrowup = false; st.keys.arrowleft = false; st.keys.arrowdown = false; st.keys.arrowright = false;
  };

  const applyDirection = (dx: number, dy: number) => {
    const st = stateRef.current;
    if (!st) return;
    const length = Math.hypot(dx, dy);
    if (length < JOYSTICK_DEADZONE) { clearMovementKeys(); return; }
    const nx = dx / length, ny = dy / length;
    lastDirectionRef.current = { x: nx, y: ny };
    st.keys.w = ny < -0.2; st.keys.s = ny > 0.2; st.keys.a = nx < -0.2; st.keys.d = nx > 0.2;
  };

  const touchToWorld = (clientX: number, clientY: number): { x: number; y: number } | null => {
    const st = stateRef.current, canvas = canvasRef.current;
    if (!st || !canvas) return null;
    const rect = canvas.getBoundingClientRect();
    if (rect.width <= 0 || rect.height <= 0) return null;
    const canvasX = (clientX - rect.left) * (canvas.width / rect.width);
    const canvasY = (clientY - rect.top) * (canvas.height / rect.height);
    return { x: canvasX - canvas.width / 2 + st.camera.x, y: canvasY - canvas.height / 2 + st.camera.y };
  };

  const handleTowerTap = (clientX: number, clientY: number) => {
    const st = stateRef.current;
    if (!st || st.gameOver || st.paused) return;
    if (st.pendingUpgrade || st.pendingArtifact || st.pendingEvolution || st.pendingTowerUpgrade || st.pendingChest) return;
    const world = touchToWorld(clientX, clientY);
    if (!world) return;
    const nearExistingTower = st.spheres.some((sphere) => sphere.alive && Math.hypot(sphere.pos.x - world.x, sphere.pos.y - world.y) < TOWER_TOUCH_TOLERANCE);
    if (nearExistingTower || st.spheres.length < getMaxSpheres(st)) placeSphere(st, world.x, world.y);
  };

  const startJoystick = (pointerId: number, x: number, y: number) => {
    joystickIdRef.current = pointerId;
    setJoystick({ pointerId, x, y, dx: 0, dy: 0, active: false });
  };

  const endPointer = (pointerId: number, clientX: number, clientY: number) => {
    const pointer = pointersRef.current.get(pointerId);
    pointersRef.current.delete(pointerId);
    if (joystickIdRef.current === pointerId) {
      joystickIdRef.current = null;
      clearMovementKeys();
      setJoystick(null);
      if (pointer && !pointer.moved) handleTowerTap(clientX, clientY);
    } else if (pointer && !pointer.moved) handleTowerTap(clientX, clientY);
  };

  useEffect(() => {
    clearMovementKeys();
    setJoystick(null);
    joystickIdRef.current = null;
    pointersRef.current.clear();
    return () => clearMovementKeys();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [handedness]);

  const activeAbilities = Object.entries(stateRef.current?.activeKeyMap || {}) as [string, AbilityType][];
  const types: SphereType[] = ['standard', 'sniper', 'shotgun', 'chain', 'aura'];
  const selectedType = stateRef.current?.selectedSphereType || 'standard';
  const selectedDef = SPHERE_TYPES[selectedType];

  return (
    <div className="absolute inset-0 z-20 overflow-hidden select-none" style={{ touchAction: 'none', userSelect: 'none', WebkitUserSelect: 'none' }}
      onPointerDown={(e) => {
        if (isBlockedByControl(e.target)) return;
        const st = stateRef.current;
        if (!st || st.gameOver || st.paused) return;
        e.currentTarget.setPointerCapture(e.pointerId);
        const pointer: PointerState = {
          startX: e.clientX,
          startY: e.clientY,
          moved: false,
          joystickCandidate: joystickIdRef.current === null && e.clientX >= joystickZoneStart && e.clientX < joystickZoneEnd,
        };
        pointersRef.current.set(e.pointerId, pointer);
        if (pointer.joystickCandidate) startJoystick(e.pointerId, e.clientX, e.clientY);
      }}
      onPointerMove={(e) => {
        const pointer = pointersRef.current.get(e.pointerId);
        if (!pointer) return;
        const dx = e.clientX - pointer.startX, dy = e.clientY - pointer.startY;
        const distance = Math.hypot(dx, dy);
        if (distance > JOYSTICK_DEADZONE) pointer.moved = true;
        if (joystickIdRef.current === e.pointerId) {
          const length = Math.hypot(dx, dy) || 1;
          const scale = Math.min(1, Math.max(0, length / JOYSTICK_RADIUS));
          setJoystick((current) => current ? { ...current, dx: (dx / length) * JOYSTICK_RADIUS * scale, dy: (dy / length) * JOYSTICK_RADIUS * scale, active: distance > JOYSTICK_DEADZONE } : current);
          applyDirection(dx, dy);
        }
      }}
      onPointerUp={(e) => endPointer(e.pointerId, e.clientX, e.clientY)}
      onPointerCancel={(e) => endPointer(e.pointerId, e.clientX, e.clientY)}
    >
      {joystick && (
        <div
          className="absolute pointer-events-none"
          style={{
            left: joystick.x - JOYSTICK_RADIUS,
            top: joystick.y - JOYSTICK_RADIUS,
            width: JOYSTICK_RADIUS * 2,
            height: JOYSTICK_RADIUS * 2,
            transform: `scale(${interfaceScale})`,
            transformOrigin: 'center',
          }}
        >
          <div className="absolute inset-0 rounded-full border border-white/20 bg-black/20 backdrop-blur-[2px]" />
          <div className={`absolute rounded-full border border-white/30 bg-white/25 ${joystick.active ? 'opacity-90' : 'opacity-70'}`}
            style={{ width: JOYSTICK_KNOB_RADIUS * 2, height: JOYSTICK_KNOB_RADIUS * 2, left: JOYSTICK_RADIUS - JOYSTICK_KNOB_RADIUS + joystick.dx, top: JOYSTICK_RADIUS - JOYSTICK_KNOB_RADIUS + joystick.dy }} />
        </div>
      )}

      <div
        className={`absolute bottom-4 ${controlsSide} flex flex-col ${controlsAlign} gap-2 pointer-events-none`}
        style={{
          transform: `scale(${interfaceScale})`,
          transformOrigin: controlsOnRight ? 'right bottom' : 'left bottom',
        }}
      >
        <button data-mobile-control="true" className="pointer-events-auto w-12 h-12 rounded-full bg-[#e8dcc0]/90 border border-[#c4b890] shadow-lg flex items-center justify-center text-[#5a4a32] active:scale-95" onPointerDown={(e) => { e.stopPropagation(); onPause(); }} aria-label={t('pause')}>
          <Pause size={18} />
        </button>

        <div className="grid grid-cols-2 gap-1.5 pointer-events-auto">
          {activeAbilities.map(([key, ability]) => {
            const def = ABILITIES[ability];
            const cd = getAbilityCooldown(stateRef.current, ability);
            return <button key={`${key}-${ability}`} data-mobile-control="true" className="relative w-16 h-12 rounded-xl bg-[#e8dcc0]/90 border border-[#c4b890] shadow-lg px-1 overflow-hidden active:scale-95"
              onPointerDown={(e) => { e.stopPropagation(); const st = stateRef.current; if (!st || st.gameOver || st.paused) return; if (st.pendingUpgrade || st.pendingArtifact || st.pendingEvolution || st.pendingTowerUpgrade || st.pendingChest) return; activateByKey(st, key); }}>
              <span className="text-[9px] text-[#8a7a5a] block truncate">{def.name[lang]}</span>
              <span className="text-[8px] text-[#5a4a32]/60">{cd > 0 ? `${Math.ceil(cd)}s` : t('ready')}</span>
              {cd > 0 && <span className="absolute inset-x-0 bottom-0 h-1 bg-[#c4453d]/70" style={{ width: `${Math.min(100, (cd / getAbilityMaxCooldown(ability)) * 100)}%` }} />}
            </button>;
          })}
        </div>

        <button data-mobile-control="true" className="pointer-events-auto w-16 h-16 rounded-full bg-[#d4943d]/85 border border-[#c46d3d] shadow-lg flex flex-col items-center justify-center text-[#3a2e1f] active:scale-95"
          onPointerDown={(e) => { e.stopPropagation(); const st = stateRef.current; if (!st || st.gameOver || st.paused) return; const { x, y } = lastDirectionRef.current; clearMovementKeys(); if (y < -0.2) st.keys.w = true; if (y > 0.2) st.keys.s = true; if (x < -0.2) st.keys.a = true; if (x > 0.2) st.keys.d = true; activateDash(st); clearMovementKeys(); }}
          aria-label={t('dashCooldown')}>
          <Zap size={20} />
          <span className="text-[8px] font-bold">{stDashLabel(stateRef.current, lang, t)}</span>
        </button>

        <div className="rounded-xl border border-[#c4b890] bg-[#e8dcc0]/90 shadow-lg px-3 py-2 w-[220px] pointer-events-none text-center">
          <div className="font-bold text-sm text-[#3a2e1f]">{selectedDef.name[lang]}</div>
          <div className="text-[10px] leading-tight text-[#8a7a5a] mt-0.5">{selectedDef.desc[lang]}</div>
        </div>

        <div className="grid grid-cols-5 gap-1.5 pointer-events-auto">
          {types.map((type) => {
            const def = SPHERE_TYPES[type];
            const selected = selectedType === type;
            return <button key={type} data-mobile-control="true" className={`w-11 h-11 rounded-xl border bg-[#e8dcc0]/90 shadow-lg flex items-center justify-center active:scale-95 ${selected ? 'border-[#8a7a5a]' : 'border-[#c4b890]'}`}
              style={{ borderColor: selected ? def.color : undefined }}
              onPointerDown={(e) => { e.stopPropagation(); const st = stateRef.current; if (!st) return; setSphereType(st, type); }}
              aria-label={`${def.name[lang]} — ${def.desc[lang]}`}>
              <span className="w-4 h-4 rounded-full" style={{ backgroundColor: def.color }} />
            </button>;
          })}
        </div>
      </div>
    </div>
  );
}

function getAbilityCooldown(st: GameState | null, ability: AbilityType): number {
  if (!st) return 0;
  const cooldownMap: Partial<Record<AbilityType, number>> = { blast: st.player.blastCooldown, shield: st.player.shieldCooldown, teleport: st.player.teleportCooldown, firetrail: st.player.fireTrailCooldown, minion: st.player.minionCooldown, lightning: st.player.lightningCooldown, timestop: st.player.timestopCooldown, darkritual: st.player.darkritualCooldown };
  return cooldownMap[ability] || 0;
}

function getAbilityMaxCooldown(ability: AbilityType): number {
  const map: Partial<Record<AbilityType, number>> = { blast: 30, shield: 20, teleport: 15, firetrail: 25, minion: 30, lightning: 20, timestop: 40, darkritual: 30 };
  return map[ability] || 1;
}

function stDashLabel(st: GameState | null, lang: Lang, t: (key: TranslationKey) => string): string {
  if (!st) return t('dashCooldown');
  return st.player.dashCooldown > 0 ? t('dashOnCooldown').replace('{sec}', String(Math.ceil(st.player.dashCooldown))) : (lang === 'ru' ? 'Рывок' : 'Dash');
}
