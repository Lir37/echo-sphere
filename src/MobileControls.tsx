import { useEffect, useRef, useState } from 'react';
import { Pause, Zap } from 'lucide-react';
import { ABILITIES, SPHERE_TYPES, type AbilityType, type SphereType } from './gameData';
import { activateByKey, activateDash, getMaxSpheres, placeSphere, setSphereType, type GameState, type SphereEntity, type Vec } from './engine';
import { CHARACTER_DEFS } from './characters';
import { getAbilityDisplayName } from './sphereProgression';
import { getCharacterFormation, getEngineerNetworkSpheres } from './characterRuntime';
import type { Lang, TranslationKey } from './i18n';
import { loadInterfaceScale } from './interfaceScale';
import { createMasteryRunTracker, getMasteryRunXp, tickCharacterMastery } from './characterMastery';
import { addCharacterMasteryXp } from './persistence';
import { canPlaceSphere, canRepositionSphere, repositionSphere } from './spaceCollision';
import { analyzeSphereNetwork, type NetworkFormation, type SphereNetworkState } from './network';
import { buildGhostSnapPreview, getGhostSnapFormation } from './networkPreview';

type PointerState = { startX: number; startY: number; moved: boolean; joystickCandidate: boolean; draggingSphere: SphereEntity | null; placingSphere: boolean; repositioned: boolean };
type JoystickVisual = { pointerId: number; x: number; y: number; dx: number; dy: number; active: boolean };
type PlacementVisual = { x: number; y: number; color: string; id: number };
type GhostPreview = { pointerId: number; sphereIndex: number; x: number; y: number; valid: boolean; network: SphereNetworkState; formation: { type: Exclude<NetworkFormation, 'none'>; nodes: number[]; strength: number } | null };
type FormationMemoryVisual = { type: Exclude<NetworkFormation, 'none'>; points: Array<{ x: number; y: number }>; life: number; id: number };
type CharacterVisualState = {
  linkedCount: number;
  furySteps: number;
  closeEnemy: boolean;
  marked: boolean;
  hunt: boolean;
  reactionReady: boolean;
  catalyst: boolean;
  formation: 'none' | 'line' | 'triangle' | 'square' | 'cluster';
  formationStrength: number;
};

export type Handedness = 'right' | 'left';

const JOYSTICK_DEADZONE = 12;
const JOYSTICK_RADIUS = 58;
const JOYSTICK_KNOB_RADIUS = 24;
const sphere_TOUCH_TOLERANCE = 26;

function haptic(duration = 8): void {
  if (typeof navigator !== 'undefined' && typeof navigator.vibrate === 'function') navigator.vibrate(duration);
}

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
  const [placementFx, setPlacementFx] = useState<PlacementVisual | null>(null);
  const [ghostPreview, setGhostPreview] = useState<GhostPreview | null>(null);
  const [formationMemory, setFormationMemory] = useState<FormationMemoryVisual | null>(null);
  const interfaceScale = loadInterfaceScale();

  const joystickOnRight = handedness === 'right';
  const controlsOnRight = !joystickOnRight;
  const controlsSide = controlsOnRight ? 'right-3' : 'left-3';
  const controlsAlign = controlsOnRight ? 'items-end' : 'items-start';
  const joystickZoneStart = joystickOnRight ? window.innerWidth * 0.5 : 0;
  const joystickZoneEnd = joystickOnRight ? window.innerWidth : window.innerWidth * 0.5;

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

  const buildGhostPreview = (st: GameState, sphere: SphereEntity, sphereIndex: number, x: number, y: number): GhostPreview => {
    const preview = buildGhostSnapPreview(st.spheres, { x, y }, undefined, sphereIndex);
    const network = preview?.network || analyzeSphereNetwork(st.spheres.map((current) => ({
      pos: current.pos,
      alive: current.alive,
      networkDisabledTimer: current.networkDisabledTimer,
    })));
    return {
      pointerId: -1,
      sphereIndex,
      x,
      y,
      valid: canRepositionSphere(st, sphere, x, y),
      network,
      formation: preview?.formation ? getGhostSnapFormation(network) ? {
        type: preview.formation.type,
        nodes: preview.formation.nodes,
        strength: preview.formation.strength,
      } : null : null,
    };
  };

  const buildNewSphereGhostPreview = (st: GameState, x: number, y: number): GhostPreview | null => {
    const preview = buildGhostSnapPreview(st.spheres, { x, y });
    if (!preview) return null;
    return {
      pointerId: -1,
      sphereIndex: preview.candidateIndex,
      x,
      y,
      valid: canPlaceSphere(st, x, y),
      network: preview.network,
      formation: preview.formation
        ? {
            type: preview.formation.type,
            nodes: preview.formation.nodes,
            strength: preview.formation.strength,
          }
        : null,
    };
  };

  const captureFormationMemory = (st: GameState, network: SphereNetworkState) => {
    const formation = getGhostSnapFormation(network);
    if (!formation) return;
    const points = formation.nodes.map((index) => ({ ...st.spheres[index].pos }));
    setFormationMemory({ type: formation.type, points, life: 0.9, id: Date.now() });
  };

  const handlesphereTap = (clientX: number, clientY: number) => {
    const st = stateRef.current;
    if (!st || st.gameOver || st.paused) return;
    if (st.pendingUpgrade || st.pendingArtifact) return;
    const world = touchToWorld(clientX, clientY);
    if (!world) return;
    const nearExistingsphere = st.spheres.some(
      (sphere) => sphere.alive && Math.hypot(sphere.pos.x - world.x, sphere.pos.y - world.y) < sphere_TOUCH_TOLERANCE
    );

    // Tapping an existing sphere keeps the old toggle/remove behavior.
    if (nearExistingsphere) {
      const existing = st.spheres.find((sphere) => sphere.alive && Math.hypot(sphere.pos.x - world.x, sphere.pos.y - world.y) < sphere_TOUCH_TOLERANCE);
      if (existing) captureFormationMemory(st, analyzeSphereNetwork(st.spheres.map((sphere) => ({ pos: sphere.pos, alive: sphere.alive, networkDisabledTimer: sphere.networkDisabledTimer }))));
      const beforeCount = st.spheres.length;
      placeSphere(st, world.x, world.y);
      if (st.spheres.length < beforeCount) haptic(10);
      return;
    }

    if (st.spheres.length >= getMaxSpheres(st)) return;

    // A new sphere cannot overlap another sphere, cover the player, or close
    // the last usable route from the player to the outside of the arena.
    if (!canPlaceSphere(st, world.x, world.y)) {
      st.flashText = { text: lang === 'ru' ? 'Путь перекрыт' : 'Path blocked', life: 0.8, color: '#ff4d5d' };
      haptic(35);
      return;
    }

    const beforeCount = st.spheres.length;
    placeSphere(st, world.x, world.y);
    if (st.spheres.length > beforeCount) {
      haptic(8);
      setPlacementFx({
        x: clientX,
        y: clientY,
        color: SPHERE_TYPES[st.selectedSphereType].color,
        id: Date.now(),
      });
    }
  };

  const startJoystick = (pointerId: number, x: number, y: number) => {
    joystickIdRef.current = pointerId;
    setJoystick({ pointerId, x, y, dx: 0, dy: 0, active: false });
  };

  const endPointer = (pointerId: number, clientX: number, clientY: number) => {
    const pointer = pointersRef.current.get(pointerId);
    pointersRef.current.delete(pointerId);
    if (pointer?.draggingSphere) {
      const st = stateRef.current;
      if (st && !pointer.moved) {
        placeSphere(st, pointer.draggingSphere.pos.x, pointer.draggingSphere.pos.y);
        setGhostPreview(null);
        haptic(10);
      } else if (st && pointer.repositioned) {
        const oldNetwork = analyzeSphereNetwork(st.spheres.map((sphere) => ({ pos: sphere.pos, alive: sphere.alive, networkDisabledTimer: sphere.networkDisabledTimer })));
        const world = touchToWorld(clientX, clientY);
        if (world && canRepositionSphere(st, pointer.draggingSphere, world.x, world.y)) {
          repositionSphere(st, pointer.draggingSphere, world.x, world.y);
          const newNetwork = analyzeSphereNetwork(st.spheres.map((sphere) => ({ pos: sphere.pos, alive: sphere.alive, networkDisabledTimer: sphere.networkDisabledTimer })));
          const oldFormation = getGhostSnapFormation(oldNetwork);
          const newFormation = getGhostSnapFormation(newNetwork);
          if (oldFormation && (!newFormation || oldFormation.type !== newFormation.type || oldFormation.nodes.join(',') !== newFormation.nodes.join(','))) captureFormationMemory(st, oldNetwork);
          st.flashText = { text: lang === 'ru' ? 'Сфера перемещена' : 'Sphere moved', life: 0.8, color: SPHERE_TYPES[pointer.draggingSphere.type].color };
        }
        setGhostPreview(null);
        haptic(12);
      } else if (st && pointer.moved) {
        st.flashText = { text: lang === 'ru' ? 'Недоступная позиция' : 'Invalid position', life: 0.65, color: '#ff4d5d' };
        setGhostPreview(null);
        haptic(22);
      }
      return;
    }
    if (pointer?.placingSphere) {
      const st = stateRef.current;
      if (st && !pointer.moved) {
        handlesphereTap(clientX, clientY);
        setGhostPreview(null);
        return;
      }
      if (st && pointer.repositioned) {
        const world = touchToWorld(clientX, clientY);
        if (world && canPlaceSphere(st, world.x, world.y)) {
          const oldNetwork = analyzeSphereNetwork(st.spheres.map((sphere) => ({ pos: sphere.pos, alive: sphere.alive, networkDisabledTimer: sphere.networkDisabledTimer })));
          const beforeCount = st.spheres.length;
          placeSphere(st, world.x, world.y);
          if (st.spheres.length > beforeCount) {
            const newNetwork = analyzeSphereNetwork(st.spheres.map((sphere) => ({ pos: sphere.pos, alive: sphere.alive, networkDisabledTimer: sphere.networkDisabledTimer })));
            const oldFormation = getGhostSnapFormation(oldNetwork);
            const newFormation = getGhostSnapFormation(newNetwork);
            if (oldFormation && (!newFormation || oldFormation.type !== newFormation.type || oldFormation.nodes.join(',') !== newFormation.nodes.join(','))) captureFormationMemory(st, oldNetwork);
            haptic(8);
          }
        }
        setGhostPreview(null);
        return;
      }
      if (st && pointer.moved) {
        st.flashText = { text: lang === 'ru' ? 'Недоступная позиция' : 'Invalid position', life: 0.65, color: '#ff4d5d' };
        setGhostPreview(null);
        haptic(22);
      }
      return;
    }
    if (joystickIdRef.current === pointerId) {
      joystickIdRef.current = null;
      clearMovementKeys();
      setJoystick(null);
      if (pointer && !pointer.moved) handlesphereTap(clientX, clientY);
    } else if (pointer && !pointer.moved) handlesphereTap(clientX, clientY);
  };

  useEffect(() => {
    clearMovementKeys();
    setJoystick(null);
    joystickIdRef.current = null;
    pointersRef.current.clear();
    return () => clearMovementKeys();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [handedness]);

  useEffect(() => {
    if (!placementFx) return;
    const timer = window.setTimeout(() => setPlacementFx(null), 450);
    return () => window.clearTimeout(timer);
  }, [placementFx]);

  useEffect(() => {
    if (!formationMemory) return;
    const started = performance.now();
    let frame = 0;
    const tick = () => {
      const elapsed = (performance.now() - started) / 1000;
      setFormationMemory((current) => current ? { ...current, life: Math.max(0, 0.9 - elapsed) } : null);
      if (elapsed < 0.9) frame = requestAnimationFrame(tick); else setFormationMemory(null);
    };
    frame = requestAnimationFrame(tick);
    return () => cancelAnimationFrame(frame);
  }, [formationMemory?.id]);

  const activeAbilities = Object.entries(stateRef.current?.activeKeyMap || {}) as [string, AbilityType][];
  const types: SphereType[] = Object.keys(SPHERE_TYPES) as SphereType[];
  const selectedType = stateRef.current?.selectedSphereType || 'standard';
  const selectedDef = SPHERE_TYPES[selectedType];
  const preferredSphereTypes = stateRef.current
    ? CHARACTER_DEFS[stateRef.current.player.characterId]?.preferredSphereTypes || []
    : [];

  return (
    <div className="es-mobile-controls absolute inset-0 z-20 overflow-hidden select-none" style={{ touchAction: 'none', userSelect: 'none', WebkitUserSelect: 'none' }}
      onPointerDown={(e) => {
        if (isBlockedByControl(e.target)) return;
        const st = stateRef.current;
        if (!st || st.gameOver || st.paused) return;
        e.currentTarget.setPointerCapture(e.pointerId);
        const pointer: PointerState = {
          startX: e.clientX,
          startY: e.clientY,
          moved: false,
          joystickCandidate: false,
          draggingSphere: null,
          placingSphere: false,
          repositioned: false,
        };
        const world = touchToWorld(e.clientX, e.clientY);
        if (world) {
          const draggable = st.spheres.find((sphere) => sphere.alive && Math.hypot(sphere.pos.x - world.x, sphere.pos.y - world.y) < 34) || null;
          if (draggable) {
            pointer.draggingSphere = draggable;
          } else {
            pointer.joystickCandidate = joystickIdRef.current === null && e.clientX >= joystickZoneStart && e.clientX < joystickZoneEnd;
            pointer.placingSphere = !pointer.joystickCandidate && st.spheres.length < getMaxSpheres(st);
          }
        }
        pointersRef.current.set(e.pointerId, pointer);
        if (pointer.joystickCandidate) startJoystick(e.pointerId, e.clientX, e.clientY);
      }}
      onPointerMove={(e) => {
        const pointer = pointersRef.current.get(e.pointerId);
        if (!pointer) return;
        const dx = e.clientX - pointer.startX, dy = e.clientY - pointer.startY;
        const distance = Math.hypot(dx, dy);
        if (pointer.draggingSphere) {
          if (distance > JOYSTICK_DEADZONE) pointer.moved = true;
          if (distance > JOYSTICK_DEADZONE) {
            const world = touchToWorld(e.clientX, e.clientY);
            const st = stateRef.current;
            if (world && st) {
              const preview = buildGhostPreview(st, pointer.draggingSphere, st.spheres.indexOf(pointer.draggingSphere), world.x, world.y);
              preview.pointerId = e.pointerId;
              pointer.repositioned = preview.valid;
              setGhostPreview(preview);
            }
          }
          return;
        }
        if (pointer.placingSphere) {
          if (distance > JOYSTICK_DEADZONE) pointer.moved = true;
          if (distance > JOYSTICK_DEADZONE) {
            const world = touchToWorld(e.clientX, e.clientY);
            const st = stateRef.current;
            if (world && st) {
              const preview = buildNewSphereGhostPreview(st, world.x, world.y);
              if (preview) {
                preview.pointerId = e.pointerId;
                pointer.repositioned = preview.valid;
                setGhostPreview(preview);
              }
            }
          }
          return;
        }
        if (distance > JOYSTICK_DEADZONE) pointer.moved = true;
        if (joystickIdRef.current === e.pointerId) {
          const length = Math.hypot(dx, dy) || 1;
          const movementScale = Math.min(1, Math.max(0, length / JOYSTICK_RADIUS));
          setJoystick((current) => current ? {
            ...current,
            dx: (dx / length) * JOYSTICK_RADIUS * movementScale,
            dy: (dy / length) * JOYSTICK_RADIUS * movementScale,
            active: distance > JOYSTICK_DEADZONE,
          } : current);
          applyDirection(dx, dy);
        }
      }}
      onPointerUp={(e) => endPointer(e.pointerId, e.clientX, e.clientY)}
      onPointerCancel={(e) => endPointer(e.pointerId, e.clientX, e.clientY)}
    >
      {ghostPreview && <GhostSnapOverlay canvasRef={canvasRef} stateRef={stateRef} preview={ghostPreview} lang={lang} sphereColor={selectedDef.color} />}
      {formationMemory && <FormationMemoryOverlay canvasRef={canvasRef} stateRef={stateRef} memory={formationMemory} />}
      <CharacterAvatarOverlay stateRef={stateRef} />

      {placementFx && (
        <div key={placementFx.id} className="absolute pointer-events-none" style={{ left: placementFx.x - 26, top: placementFx.y - 26, width: 52, height: 52, transform: `scale(${interfaceScale})`, transformOrigin: 'center' }}>
          <div className="absolute inset-0 rounded-full border-2 opacity-70 animate-ping" style={{ borderColor: placementFx.color }} />
          <div className="absolute inset-[9px] rounded-full border" style={{ borderColor: placementFx.color, opacity: 0.55 }} />
        </div>
      )}

      {joystick && (
        <div className="absolute pointer-events-none" style={{ left: joystick.x - JOYSTICK_RADIUS, top: joystick.y - JOYSTICK_RADIUS, width: JOYSTICK_RADIUS * 2, height: JOYSTICK_RADIUS * 2, transform: `scale(${interfaceScale})`, transformOrigin: 'center' }}>
          <div className="absolute inset-0 rounded-full border border-white/20 bg-black/20 backdrop-blur-[2px]" />
          <div className={`absolute rounded-full border border-white/30 bg-white/25 ${joystick.active ? 'opacity-90' : 'opacity-70'}`} style={{ width: JOYSTICK_KNOB_RADIUS * 2, height: JOYSTICK_KNOB_RADIUS * 2, left: JOYSTICK_RADIUS - JOYSTICK_KNOB_RADIUS + joystick.dx, top: JOYSTICK_RADIUS - JOYSTICK_KNOB_RADIUS + joystick.dy }} />
        </div>
      )}

      <div className={`absolute bottom-4 ${controlsSide} flex flex-col ${controlsAlign} gap-2 pointer-events-none`} style={{ transform: `scale(${interfaceScale})`, transformOrigin: controlsOnRight ? 'right bottom' : 'left bottom' }}>
        <button data-mobile-control="true" className="pointer-events-auto w-12 h-12 rounded-full bg-[#0d1726]/90 border border-[#243b55] shadow-lg flex items-center justify-center text-[#b6c9de] active:scale-95" onPointerDown={(e) => { e.stopPropagation(); haptic(6); onPause(); }} aria-label={t('pause')}><Pause size={18} /></button>

        <div className="grid grid-cols-2 gap-1.5 pointer-events-auto">
          {activeAbilities.map(([key, ability]) => {
            const def = ABILITIES[ability];
            const cd = getAbilityCooldown(stateRef.current, ability);
            return <button key={`${key}-${ability}`} data-mobile-control="true" className="relative w-16 h-12 rounded-xl bg-[#0d1726]/90 border border-[#243b55] shadow-lg px-1 overflow-hidden active:scale-95" onPointerDown={(e) => { e.stopPropagation(); const st = stateRef.current; if (!st || st.gameOver || st.paused) return; if (st.pendingUpgrade || st.pendingArtifact) return; haptic(12); activateByKey(st, key); }}>
              <span className="text-[9px] text-[#7f9bb8] block truncate">{getAbilityDisplayName(stateRef.current, ability, lang) || def.name[lang]}</span>
              <span className="text-[8px] text-[#b6c9de]/60">{cd > 0 ? `${Math.ceil(cd)}s` : t('ready')}</span>
              {cd > 0 && <span className="absolute inset-x-0 bottom-0 h-1 bg-[#ff4d5d]/70" style={{ width: `${Math.min(100, (cd / getAbilityMaxCooldown(ability)) * 100)}%` }} />}
            </button>;
          })}
        </div>

        <button data-mobile-control="true" className="pointer-events-auto w-16 h-16 rounded-full bg-[#ffb84d]/85 border border-[#ff6b6b] shadow-lg flex flex-col items-center justify-center text-[#dcecff] active:scale-95" onPointerDown={(e) => { e.stopPropagation(); const st = stateRef.current; if (!st || st.gameOver || st.paused) return; const { x, y } = lastDirectionRef.current; clearMovementKeys(); if (y < -0.2) st.keys.w = true; if (y > 0.2) st.keys.s = true; if (x < -0.2) st.keys.a = true; if (x > 0.2) st.keys.d = true; haptic(16); activateDash(st); clearMovementKeys(); }} aria-label={t('dashCooldown')}>
          <Zap size={20} /><span className="text-[8px] font-bold">{stDashLabel(stateRef.current, lang, t)}</span>
        </button>

        <div className="rounded-xl border border-[#243b55] bg-[#0d1726]/90 shadow-lg px-3 py-2 w-[220px] pointer-events-none text-center">
          <div className="font-bold text-sm text-[#dcecff]">{selectedDef.name[lang]}</div>
          <div className="text-[10px] leading-tight text-[#7f9bb8] mt-0.5">{selectedDef.desc[lang]}</div>
        </div>

        <div className="grid grid-cols-5 gap-1.5 pointer-events-auto">
          {types.map((type) => {
            const def = SPHERE_TYPES[type];
            const selected = selectedType === type;
            const preferred = preferredSphereTypes.includes(type);
            return <button key={type} data-mobile-control="true" className={`w-11 h-11 rounded-xl border bg-[#0d1726]/90 shadow-lg flex items-center justify-center active:scale-95 ${selected ? 'border-[#7f9bb8]' : 'border-[#243b55]'} ${preferred ? 'ring-2 ring-[#ffb84d]/45 ring-offset-1 ring-offset-[#0d1726]' : ''}`} style={{ borderColor: selected ? def.color : undefined }} onPointerDown={(e) => { e.stopPropagation(); const st = stateRef.current; if (!st) return; haptic(6); setSphereType(st, type); }} aria-label={`${def.name[lang]} — ${def.desc[lang]}`}>
              <SphereTypeGlyph type={type} color={def.color} />
            </button>;
          })}
        </div>
      </div>
    </div>
  );
}

function SphereTypeGlyph({ type, color }: { type: SphereType; color: string }) {
  if (type === 'sniper') {
    return <span className="w-4 h-4 rotate-45 border-2" style={{ borderColor: color, backgroundColor: `${color}33` }} />;
  }
  if (type === 'shotgun') {
    return <span className="relative w-5 h-4" aria-hidden="true">
      <span className="absolute left-0 top-1 w-3 h-2 rounded-sm border-2" style={{ borderColor: color, backgroundColor: `${color}33` }} />
      <span className="absolute left-3 top-0 w-2 h-4 rounded-r border-2 border-l-0" style={{ borderColor: color }} />
    </span>;
  }
  if (type === 'chain') {
    return <span className="flex items-center gap-0.5" aria-hidden="true">
      <span className="w-2.5 h-2.5 rounded-full border-2" style={{ borderColor: color, backgroundColor: `${color}33` }} />
      <span className="w-2.5 h-2.5 rounded-full border-2" style={{ borderColor: color, backgroundColor: `${color}33` }} />
      <span className="w-2.5 h-2.5 rounded-full border-2" style={{ borderColor: color, backgroundColor: `${color}33` }} />
    </span>;
  }
  if (type === 'aura') {
    return <span className="w-5 h-5 rounded-full border-2" style={{ borderColor: color, boxShadow: `0 0 0 3px ${color}22` }} />;
  }
  return <span className="w-4 h-4 rounded-full border-2" style={{ borderColor: color, backgroundColor: `${color}44` }} />;
}

function worldToScreen(canvas: HTMLCanvasElement, camera: Vec, point: { x: number; y: number }): { x: number; y: number } {
  const rect = canvas.getBoundingClientRect();
  return { x: rect.left + (point.x - camera.x + canvas.width / 2) * (rect.width / canvas.width), y: rect.top + (point.y - camera.y + canvas.height / 2) * (rect.height / canvas.height) };
}

function GhostSnapOverlay({ canvasRef, stateRef, preview, lang, sphereColor }: { canvasRef: React.RefObject<HTMLCanvasElement | null>; stateRef: React.MutableRefObject<GameState | null>; preview: GhostPreview; lang: Lang; sphereColor: string }) {
  const canvas = canvasRef.current;
  const st = stateRef.current;
  if (!canvas || !st) return null;
  const ghost = worldToScreen(canvas, st.camera, { x: preview.x, y: preview.y });
  const scale = canvas.getBoundingClientRect().width / canvas.width;
  const links = preview.network.links.filter((link) => link.a === preview.sphereIndex || link.b === preview.sphereIndex);
  const points = preview.formation?.nodes.map((index) => { const node = index === preview.sphereIndex ? { x: preview.x, y: preview.y } : st.spheres[index]?.pos; return node ? worldToScreen(canvas, st.camera, node) : null; }).filter((x): x is { x: number; y: number } => Boolean(x)) || [];
  return <svg className="absolute inset-0 pointer-events-none z-10 overflow-visible">
    {links.map((link) => { const otherIndex = link.a === preview.sphereIndex ? link.b : link.a; const other = st.spheres[otherIndex]; if (!other) return null; const p = worldToScreen(canvas, st.camera, other.pos); return <line key={'ghost-link-' + otherIndex} x1={ghost.x} y1={ghost.y} x2={p.x} y2={p.y} stroke={preview.valid ? sphereColor : '#ff4d5d'} strokeWidth={2.2 * scale} strokeDasharray="7 5" opacity=".9" />; })}
    {points.length >= 2 && <polyline points={points.map((p) => p.x + ',' + p.y).join(' ')} fill="none" stroke={preview.valid ? '#ffb84d' : '#ff4d5d'} strokeWidth={2 * scale} strokeDasharray="6 5" opacity=".82" />}
    <circle cx={ghost.x} cy={ghost.y} r={25 * scale} fill={preview.valid ? sphereColor + '18' : 'rgba(255,77,93,.12)'} stroke={preview.valid ? sphereColor : '#ff4d5d'} strokeWidth={2 * scale} strokeDasharray="5 4" />
    <text x={ghost.x} y={ghost.y - 31 * scale} textAnchor="middle" fill={preview.valid ? '#dcecff' : '#ff7a86'} fontSize={11 * scale} fontWeight="700">{preview.valid ? (preview.formation ? preview.formation.type.toUpperCase() : (lang === 'ru' ? 'СЕТЬ' : 'NETWORK')) : (lang === 'ru' ? 'НЕДОСТУПНО' : 'INVALID')}</text>
  </svg>;
}

function FormationMemoryOverlay({ canvasRef, stateRef, memory }: { canvasRef: React.RefObject<HTMLCanvasElement | null>; stateRef: React.MutableRefObject<GameState | null>; memory: FormationMemoryVisual }) {
  const canvas = canvasRef.current;
  const st = stateRef.current;
  if (!canvas || !st) return null;
  const alpha = Math.min(0.55, memory.life / 0.9);
  const points = memory.points.map((point) => worldToScreen(canvas, st.camera, point));
  const closed = memory.type !== 'line';
  return <svg className="absolute inset-0 pointer-events-none z-9 overflow-visible">
    <polyline points={points.map((p) => p.x + ',' + p.y).join(' ')} fill="none" stroke="#b6c9de" strokeWidth="2" strokeDasharray="4 6" opacity={alpha} />
    {closed && points.length > 2 && <line x1={points[points.length - 1].x} y1={points[points.length - 1].y} x2={points[0].x} y2={points[0].y} stroke="#b6c9de" strokeWidth="1.5" strokeDasharray="4 6" opacity={alpha * .75} />}
    {points.map((p, index) => <circle key={index} cx={p.x} cy={p.y} r="9" fill="none" stroke="#b6c9de" strokeWidth="1.5" opacity={alpha} />)}
  </svg>;
}

function getEmptyCharacterVisualState(): CharacterVisualState {
  return {
    linkedCount: 0,
    furySteps: 0,
    closeEnemy: false,
    marked: false,
    hunt: false,
    reactionReady: false,
    catalyst: false,
    formation: 'none',
    formationStrength: 0,
  };
}

function readCharacterVisualState(st: GameState): CharacterVisualState {
  const characterId = st.player.characterId;
  const hpRatio = st.player.hp / Math.max(1, st.player.maxHp);
  const visual = getEmptyCharacterVisualState();

  if (characterId === 'engineer') {
    visual.linkedCount = getEngineerNetworkSpheres(st).length;
  }

  if (characterId === 'berserker') {
    visual.furySteps = Math.min(4, Math.floor(Math.max(0, 1 - hpRatio) / 0.2));
    visual.closeEnemy = st.enemies.some((enemy) => enemy.hp > 0 && Math.hypot(enemy.pos.x - st.player.pos.x, enemy.pos.y - st.player.pos.y) <= 110);
  }

  if (characterId === 'hunter') {
    visual.marked = st.player.hunterMarkTarget !== null && st.player.hunterMarkTimer > 0;
    visual.hunt = st.player.hunterHuntTimer > 0;
  }

  if (characterId === 'alchemist') {
    visual.reactionReady = st.enemies.some((enemy) => enemy.hp > 0 && countStatusEffects(enemy) >= 2);
    visual.catalyst = st.player.alchemistCatalystTimer > 0;
  }

  if (characterId === 'architect') {
    const formation = getCharacterFormation(st);
    visual.formation = formation.type;
    visual.formationStrength = formation.strength;
  }

  return visual;
}

function CharacterAvatarOverlay({ stateRef }: { stateRef: React.MutableRefObject<GameState | null> }) {
  const [characterId, setCharacterId] = useState(() => stateRef.current?.player.characterId || 'spherist');
  const [mutationStage, setMutationStage] = useState(() => stateRef.current?.player.mutationStage || 0);
  const [visual, setVisual] = useState<CharacterVisualState>(getEmptyCharacterVisualState);
  const frameRef = useRef<number | null>(null);
  const masteryRef = useRef(createMasteryRunTracker());
  const rewardedRef = useRef(false);

  useEffect(() => {
    let lastVisualUpdate = 0;
    const tick = (now: number) => {
      const st = stateRef.current;
      const nextCharacter = st?.player.characterId || 'spherist';
      const nextMutation = st?.player.mutationStage || 0;
      setCharacterId((current) => current === nextCharacter ? current : nextCharacter);
      setMutationStage((current) => current === nextMutation ? current : nextMutation);

      if (st && !st.gameOver) {
        tickCharacterMastery(st, 1 / 60, masteryRef.current);
        if (now - lastVisualUpdate >= 100) {
          setVisual(readCharacterVisualState(st));
          lastVisualUpdate = now;
        }
      } else if (st?.gameOver && !rewardedRef.current) {
        const gainedXp = getMasteryRunXp(masteryRef.current);
        if (gainedXp > 0) addCharacterMasteryXp(st.player.characterId, gainedXp);
        rewardedRef.current = true;
      }

      frameRef.current = requestAnimationFrame(tick);
    };
    frameRef.current = requestAnimationFrame(tick);
    return () => {
      if (frameRef.current !== null) cancelAnimationFrame(frameRef.current);
    };
  }, [stateRef]);

  const color = CHARACTER_DEFS[characterId]?.color || '#5a8c4a';
  const pulse = mutationStage >= 3 ? 'animate-pulse' : '';

  return (
    <div
      className="absolute left-1/2 top-1/2 pointer-events-none"
      data-character-avatar-overlay="disabled"
      aria-hidden="true"
      style={{ transform: 'translate(-50%, -50%)', width: 74, height: 74, display: 'none' }}
    >
      <div className={`absolute inset-0 flex items-center justify-center ${pulse}`}>
        <CharacterCore characterId={characterId} color={color} mutationStage={mutationStage} visual={visual} />
      </div>
    </div>
  );
}

function CharacterSvg({ characterId, color, mutationStage, visual }: {
  characterId: string;
  color: string;
  mutationStage: number;
  visual: CharacterVisualState;
}) {
  const shade = characterId === 'berserker' ? '#7e2e2c' : '#070d18';
  const common = { width: 58, height: 58, viewBox: '0 0 58 58', fill: 'none', xmlns: 'http://www.w3.org/2000/svg', overflow: 'visible' } as const;

  if (characterId === 'hunter') return (
    <svg {...common}>
      <path d="M29 6L46 17V34L29 49L12 34V17L29 6Z" fill={color} fillOpacity=".93" stroke="#dcecff" strokeWidth="2"/>
      <path d="M18 20H40L35 31H23L18 20Z" fill="#0d1726" stroke="#dcecff" strokeWidth="1.5"/>
      <circle cx="29" cy="25" r="3" fill="#dcecff"/>
      <path d="M29 11V17M29 39V46M11 29H17M41 29H47" stroke="#ffb84d" strokeWidth="2" strokeLinecap="round"/>
      {visual.marked && <circle cx="29" cy="29" r={visual.hunt ? 26 : 23} stroke={visual.hunt ? '#ffb84d' : '#ff6b6b'} strokeWidth={visual.hunt ? 3 : 2} strokeDasharray={visual.hunt ? '5 4' : '3 4'} />}
      {visual.hunt && <circle cx="29" cy="29" r="4" stroke="#ffb84d" strokeWidth="2" />}
    </svg>
  );

  if (characterId === 'engineer') return (
    <svg {...common}>
      <path d="M29 5L45 14V32L29 47L13 32V14L29 5Z" fill={color} fillOpacity=".92" stroke="#dcecff" strokeWidth="2"/>
      <path d="M22 20L29 14L36 20V32L29 38L22 32V20Z" fill="#0d1726" stroke="#dcecff" strokeWidth="1.5"/>
      <circle cx="29" cy="26" r="4" fill={color}/>
      <path d="M11 18L17 22M47 18L41 22M11 40L17 35M47 40L41 35" stroke={color} strokeWidth="2" strokeLinecap="round"/>
      <circle cx="9" cy="17" r="2.5" fill="#39d8ff"/><circle cx="49" cy="17" r="2.5" fill="#39d8ff"/>
      {visual.linkedCount >= 2 && <>
        {[0, 1, 2, 3].map((index) => {
          const a = (index / 4) * Math.PI * 2 - Math.PI / 4;
          const x = 29 + Math.cos(a) * 22;
          const y = 29 + Math.sin(a) * 22;
          return <g key={index}><line x1="29" y1="29" x2={x} y2={y} stroke="#39d8ff" strokeWidth="1.5" opacity=".65" /><circle cx={x} cy={y} r="3" fill="#39d8ff" opacity={visual.linkedCount >= 4 ? 1 : .7} /></g>;
        })}
      </>}
      {visual.linkedCount >= 3 && <circle cx="29" cy="29" r="25" stroke="#39d8ff" strokeWidth="1.5" strokeDasharray="2 4" opacity=".8" />}
    </svg>
  );

  if (characterId === 'berserker') return (
    <svg {...common}>
      {visual.closeEnemy && <circle cx="29" cy="29" r={26 + visual.furySteps * 2} stroke="#ff4d5d" strokeWidth={2 + visual.furySteps * .5} strokeDasharray="6 3" opacity=".7" />}
      <path d="M14 17L22 10L29 15L36 10L44 17L41 39L29 50L17 39L14 17Z" fill={color} stroke="#dcecff" strokeWidth="2"/>
      <path d="M14 17L7 10L10 25L18 21M44 17L51 10L48 25L40 21" fill={color} stroke="#dcecff" strokeWidth="2" strokeLinejoin="round"/>
      <path d="M20 27L25 25M38 27L33 25M22 34L29 38L36 34" stroke={shade} strokeWidth="2.4" strokeLinecap="round"/>
      {mutationStage > 0 && <path d="M29 7L31 2L33 8M20 46L16 52M38 46L42 52" stroke="#ffb84d" strokeWidth="2" strokeLinecap="round"/>}
      {Array.from({ length: 4 }, (_, index) => index < visual.furySteps ? index : null).filter((index): index is number => index !== null).map((index) => {
        const a = -Math.PI / 2 + index * (Math.PI / 2);
        return <circle key={index} cx={29 + Math.cos(a) * 27} cy={29 + Math.sin(a) * 27} r="2.5" fill="#ff4d5d" />;
      })}
    </svg>
  );

  if (characterId === 'alchemist') return (
    <svg {...common}>
      <path d="M23 7H35V16L43 23V39C43 44 37 48 29 48C21 48 15 44 15 39V23L23 16V7Z" fill={color} fillOpacity=".9" stroke="#dcecff" strokeWidth="2"/>
      <path d="M23 7H35" stroke="#dcecff" strokeWidth="3" strokeLinecap="round"/>
      <path d="M18 31C23 27 35 27 40 31V39C35 43 23 43 18 39V31Z" fill="#0d1726" fillOpacity=".65"/>
      <path d="M24 17H34" stroke="#0d1726" strokeWidth="2" strokeLinecap="round"/>
      {mutationStage > 0 && <circle cx="29" cy="35" r="3" fill="#ffb84d"/>}
      {visual.reactionReady && <>
        <circle cx="13" cy="13" r="4" fill="#ff6b6b" opacity=".9" />
        <circle cx="45" cy="13" r="4" fill="#5a8c4a" opacity=".9" />
        <circle cx="29" cy="52" r="4" fill="#39d8ff" opacity=".9" />
        <path d="M16 15L24 23M42 15L34 23M29 48L29 40" stroke="#8a5a8a" strokeWidth="1.5" strokeDasharray="2 2" />
      </>}
      {visual.catalyst && <circle cx="29" cy="29" r="25" stroke="#ffb84d" strokeWidth="2.5" strokeDasharray="5 3" />}
    </svg>
  );

  if (characterId === 'architect') return (
    <svg {...common}>
      <rect x="11" y="11" width="36" height="36" rx="3" fill="#0d1726" stroke={color} strokeWidth="3" transform="rotate(45 29 29)"/>
      <path d="M29 13L43 37H15L29 13Z" fill={color} fillOpacity=".78" stroke="#dcecff" strokeWidth="2"/>
      <path d="M29 21V36M21 34H37" stroke="#0d1726" strokeWidth="2" strokeLinecap="round"/>
      {mutationStage > 1 && <circle cx="29" cy="29" r="20" stroke="#ffb84d" strokeWidth="2" strokeDasharray="4 4"/>}
      {visual.formation === 'line' && <line x1="7" y1="51" x2="51" y2="7" stroke="#ffb84d" strokeWidth="2.5" opacity={Math.max(.35, visual.formationStrength)} />}
      {visual.formation === 'triangle' && <path d="M29 5L52 48H6Z" fill="none" stroke="#ffb84d" strokeWidth="2" opacity={Math.max(.35, visual.formationStrength)} />}
      {visual.formation === 'square' && <rect x="6" y="6" width="46" height="46" fill="none" stroke="#ffb84d" strokeWidth="2" opacity={Math.max(.35, visual.formationStrength)} />}
      {visual.formation === 'cluster' && <circle cx="29" cy="29" r="24" fill="none" stroke="#ffb84d" strokeWidth="2" strokeDasharray="3 3" opacity={Math.max(.35, visual.formationStrength)} />}
    </svg>
  );

  return (
    <svg {...common}>
      <path d="M29 5L47 19L38 43L29 50L20 43L11 19L29 5Z" fill={color} fillOpacity=".92" stroke="#dcecff" strokeWidth="2"/>
      <path d="M20 17L29 11L38 17L34 29L29 38L24 29L20 17Z" fill="#0d1726" fillOpacity=".55" stroke="#dcecff" strokeWidth="1.5"/>
      <circle cx="29" cy="28" r="4" fill="#dcecff"/>
      <path d="M9 18L15 23M49 18L43 23M11 42L18 36M47 42L40 36" stroke={color} strokeWidth="2" strokeLinecap="round"/>
      {mutationStage > 1 && <circle cx="29" cy="28" r="21" stroke="#ffb84d" strokeWidth="2" strokeDasharray="3 4"/>}
    </svg>
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

function countStatusEffects(enemy: GameState['enemies'][number]): number {
  return Number(enemy.fireTimer > 0) + Number(enemy.freezeTimer > 0) + Number(enemy.poisonTimer > 0);
}

function CharacterCore({ characterId, color, mutationStage, visual }: {
  characterId: string;
  color: string;
  mutationStage: number;
  visual: CharacterVisualState;
}) {
  const rgb = hexRgb(color);
  const pulse = 1 + Math.sin(Date.now() / 240) * 0.04;
  const glow = mutationStage >= 3 ? 0.9 : 0.62;

  return (
    <svg width="78" height="78" viewBox="0 0 78 78" fill="none" xmlns="http://www.w3.org/2000/svg" style={{ overflow: 'visible' }}>
      <defs>
        <radialGradient id="echo-core-gradient" cx="35%" cy="30%" r="75%">
          <stop offset="0%" stopColor="#ffffff" />
          <stop offset="18%" stopColor="#dff6ff" />
          <stop offset="48%" stopColor={color} />
          <stop offset="100%" stopColor={shadeHex(color, -55)} />
        </radialGradient>
      </defs>

      <circle cx="39" cy="39" r={30 * pulse} fill={`rgba(${rgb},0.09)`} />
      <circle cx="39" cy="39" r={24 + mutationStage * 2} stroke={`rgba(${rgb},${glow})`} strokeWidth="1" strokeDasharray={mutationStage >= 2 ? "2 4" : "5 6"} />
      {mutationStage >= 1 && (
        <g opacity=".72">
          <path d="M39 7L45 18L39 23L33 18L39 7Z" stroke={color} />
          <path d="M71 39L60 45L55 39L60 33L71 39Z" stroke={color} />
          <path d="M39 71L33 60L39 55L45 60L39 71Z" stroke={color} />
          <path d="M7 39L18 33L23 39L18 45L7 39Z" stroke={color} />
        </g>
      )}

      {characterId === 'engineer' && (
        <g opacity={visual.linkedCount >= 2 ? ".95" : ".65"} stroke={color}>
          <path d="M14 20L28 30M64 20L50 30M14 58L28 48M64 58L50 48" strokeWidth="1.4" />
          <circle cx="14" cy="20" r="3" fill={color} /><circle cx="64" cy="20" r="3" fill={color} />
          <circle cx="14" cy="58" r="3" fill={color} /><circle cx="64" cy="58" r="3" fill={color} />
          {visual.linkedCount >= 3 && <circle cx="39" cy="39" r="31" strokeDasharray="3 5" />}
        </g>
      )}

      {characterId === 'hunter' && (
        <g stroke={visual.hunt ? '#ffe07a' : color}>
          <path d="M8 39H23M55 39H70M39 8V23M39 55V70" strokeWidth={visual.hunt ? "2" : "1.2"} />
          {visual.marked && <circle cx="39" cy="39" r={visual.hunt ? "29" : "26"} strokeDasharray={visual.hunt ? "5 3" : "2 5"} opacity=".85" />}
        </g>
      )}

      {characterId === 'berserker' && (
        <g stroke="#ff5b58" opacity=".8">
          <path d="M39 4L43 14L52 7L51 19L63 15L57 25L70 28L58 34" strokeWidth={1.4 + visual.furySteps * .35} />
          <path d="M39 74L35 64L26 71L27 59L15 63L21 53L8 50L20 44" strokeWidth={1.4 + visual.furySteps * .35} />
          {visual.closeEnemy && <circle cx="39" cy="39" r={27 + visual.furySteps * 2} strokeDasharray="6 4" />}
        </g>
      )}

      {characterId === 'alchemist' && (
        <g stroke={color} opacity=".85">
          <circle cx="39" cy="39" r="29" strokeDasharray="1 5" />
          <path d="M39 10V68M10 39H68" opacity=".35" />
          {visual.reactionReady && <path d="M39 12L45 25L59 19L53 33L66 39L53 45L59 59L45 53L39 66L33 53L19 59L25 45L12 39L25 33L19 19L33 25L39 12Z" stroke="#e7a4ff" />}
        </g>
      )}

      {characterId === 'architect' && (
        <g stroke={color} opacity=".88">
          {visual.formation === 'triangle' && <path d="M39 9L67 58H11L39 9Z" />}
          {visual.formation === 'square' && <rect x="14" y="14" width="50" height="50" transform="rotate(45 39 39)" />}
          {visual.formation === 'line' && <path d="M9 39H69" />}
          {visual.formation === 'cluster' && <circle cx="39" cy="39" r="29" />}
          {mutationStage >= 2 && <circle cx="39" cy="39" r="20" strokeDasharray="2 4" />}
        </g>
      )}

      <circle cx="39" cy="39" r={13 + mutationStage * 1.2} fill="rgba(0,5,15,0.55)" stroke={`rgba(255,255,255,0.72)`} strokeWidth="1" />
      <circle cx="39" cy="39" r={10 + mutationStage * .8} fill="url(#echo-core-gradient)" />
      <circle cx="35.5" cy="35" r="3.5" fill="#fff" opacity=".92" />
      <circle cx="39" cy="39" r={17 + mutationStage * 1.5} stroke={`rgba(${rgb},0.28)`} strokeWidth="2" />
      {mutationStage >= 4 && (
        <g stroke="#fff" opacity=".7">
          <path d="M39 1V8M39 70V77M1 39H8M70 39H77" />
        </g>
      )}
    </svg>
  );
}

function hexRgb(hex: string): string {
  const h = hex.replace('#', '');
  return `${parseInt(h.slice(0, 2), 16)},${parseInt(h.slice(2, 4), 16)},${parseInt(h.slice(4, 6), 16)}`;
}

function shadeHex(hex: string, amount: number): string {
  const h = hex.replace('#', '');
  const r = Math.max(0, Math.min(255, parseInt(h.slice(0, 2), 16) + amount));
  const g = Math.max(0, Math.min(255, parseInt(h.slice(2, 4), 16) + amount));
  const b = Math.max(0, Math.min(255, parseInt(h.slice(4, 6), 16) + amount));
  return `#${r.toString(16).padStart(2, '0')}${g.toString(16).padStart(2, '0')}${b.toString(16).padStart(2, '0')}`;
}

