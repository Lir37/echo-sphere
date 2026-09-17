import { useEffect, useRef, useState } from 'react';
import { Pause, Zap } from 'lucide-react';
import { ABILITIES, SPHERE_TYPES, type AbilityType, type SphereType } from './gameData';
import { activateByKey, activateDash, getMaxSpheres, placeSphere, setSphereType, type GameState } from './engine';
import { CHARACTER_DEFS } from './characters';
import { getCharacterFormation, getEngineerNetworkSpheres } from './characterRuntime';
import type { Lang, TranslationKey } from './i18n';
import { loadInterfaceScale } from './interfaceScale';
import { createMasteryRunTracker, getMasteryRunXp, tickCharacterMastery } from './characterMastery';
import { addCharacterMasteryXp } from './persistence';
import { canPlaceSphere } from './spaceCollision';

type PointerState = { startX: number; startY: number; moved: boolean; joystickCandidate: boolean };
type JoystickVisual = { pointerId: number; x: number; y: number; dx: number; dy: number; active: boolean };
type PlacementVisual = { x: number; y: number; color: string; id: number };
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
const TOWER_TOUCH_TOLERANCE = 26;

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

  const handleTowerTap = (clientX: number, clientY: number) => {
    const st = stateRef.current;
    if (!st || st.gameOver || st.paused) return;
    if (st.pendingUpgrade || st.pendingArtifact || st.pendingEvolution || st.pendingTowerUpgrade || st.pendingChest) return;
    const world = touchToWorld(clientX, clientY);
    if (!world) return;
    const nearExistingTower = st.spheres.some(
      (sphere) => sphere.alive && Math.hypot(sphere.pos.x - world.x, sphere.pos.y - world.y) < TOWER_TOUCH_TOLERANCE
    );

    // Tapping an existing tower keeps the old toggle/remove behavior.
    if (nearExistingTower) {
      const beforeCount = st.spheres.length;
      placeSphere(st, world.x, world.y);
      if (st.spheres.length < beforeCount) haptic(10);
      return;
    }

    if (st.spheres.length >= getMaxSpheres(st)) return;

    // A new tower cannot overlap another tower, cover the player, or close
    // the last usable route from the player to the outside of the arena.
    if (!canPlaceSphere(st, world.x, world.y)) {
      st.flashText = { text: lang === 'ru' ? 'Путь перекрыт' : 'Path blocked', life: 0.8, color: '#c4453d' };
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

  useEffect(() => {
    if (!placementFx) return;
    const timer = window.setTimeout(() => setPlacementFx(null), 450);
    return () => window.clearTimeout(timer);
  }, [placementFx]);

  const activeAbilities = Object.entries(stateRef.current?.activeKeyMap || {}) as [string, AbilityType][];
  const types: SphereType[] = ['standard', 'sniper', 'shotgun', 'chain', 'aura'];
  const selectedType = stateRef.current?.selectedSphereType || 'standard';
  const selectedDef = SPHERE_TYPES[selectedType];
  const preferredSphereTypes = stateRef.current
    ? CHARACTER_DEFS[stateRef.current.player.characterId]?.preferredSphereTypes || []
    : [];

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
        <button data-mobile-control="true" className="pointer-events-auto w-12 h-12 rounded-full bg-[#e8dcc0]/90 border border-[#c4b890] shadow-lg flex items-center justify-center text-[#5a4a32] active:scale-95" onPointerDown={(e) => { e.stopPropagation(); haptic(6); onPause(); }} aria-label={t('pause')}><Pause size={18} /></button>

        <div className="grid grid-cols-2 gap-1.5 pointer-events-auto">
          {activeAbilities.map(([key, ability]) => {
            const def = ABILITIES[ability];
            const cd = getAbilityCooldown(stateRef.current, ability);
            return <button key={`${key}-${ability}`} data-mobile-control="true" className="relative w-16 h-12 rounded-xl bg-[#e8dcc0]/90 border border-[#c4b890] shadow-lg px-1 overflow-hidden active:scale-95" onPointerDown={(e) => { e.stopPropagation(); const st = stateRef.current; if (!st || st.gameOver || st.paused) return; if (st.pendingUpgrade || st.pendingArtifact || st.pendingEvolution || st.pendingTowerUpgrade || st.pendingChest) return; haptic(12); activateByKey(st, key); }}>
              <span className="text-[9px] text-[#8a7a5a] block truncate">{def.name[lang]}</span>
              <span className="text-[8px] text-[#5a4a32]/60">{cd > 0 ? `${Math.ceil(cd)}s` : t('ready')}</span>
              {cd > 0 && <span className="absolute inset-x-0 bottom-0 h-1 bg-[#c4453d]/70" style={{ width: `${Math.min(100, (cd / getAbilityMaxCooldown(ability)) * 100)}%` }} />}
            </button>;
          })}
        </div>

        <button data-mobile-control="true" className="pointer-events-auto w-16 h-16 rounded-full bg-[#d4943d]/85 border border-[#c46d3d] shadow-lg flex flex-col items-center justify-center text-[#3a2e1f] active:scale-95" onPointerDown={(e) => { e.stopPropagation(); const st = stateRef.current; if (!st || st.gameOver || st.paused) return; const { x, y } = lastDirectionRef.current; clearMovementKeys(); if (y < -0.2) st.keys.w = true; if (y > 0.2) st.keys.s = true; if (x < -0.2) st.keys.a = true; if (x > 0.2) st.keys.d = true; haptic(16); activateDash(st); clearMovementKeys(); }} aria-label={t('dashCooldown')}>
          <Zap size={20} /><span className="text-[8px] font-bold">{stDashLabel(stateRef.current, lang, t)}</span>
        </button>

        <div className="rounded-xl border border-[#c4b890] bg-[#e8dcc0]/90 shadow-lg px-3 py-2 w-[220px] pointer-events-none text-center">
          <div className="font-bold text-sm text-[#3a2e1f]">{selectedDef.name[lang]}</div>
          <div className="text-[10px] leading-tight text-[#8a7a5a] mt-0.5">{selectedDef.desc[lang]}</div>
        </div>

        <div className="grid grid-cols-5 gap-1.5 pointer-events-auto">
          {types.map((type) => {
            const def = SPHERE_TYPES[type];
            const selected = selectedType === type;
            const preferred = preferredSphereTypes.includes(type);
            return <button key={type} data-mobile-control="true" className={`w-11 h-11 rounded-xl border bg-[#e8dcc0]/90 shadow-lg flex items-center justify-center active:scale-95 ${selected ? 'border-[#8a7a5a]' : 'border-[#c4b890]'} ${preferred ? 'ring-2 ring-[#d4943d]/45 ring-offset-1 ring-offset-[#e8dcc0]' : ''}`} style={{ borderColor: selected ? def.color : undefined }} onPointerDown={(e) => { e.stopPropagation(); const st = stateRef.current; if (!st) return; haptic(6); setSphereType(st, type); }} aria-label={`${def.name[lang]} — ${def.desc[lang]}`}>
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
    <div className="absolute left-1/2 top-1/2 pointer-events-none" style={{ transform: 'translate(-50%, -50%)', width: 74, height: 74 }}>
      <div className={`absolute inset-0 flex items-center justify-center ${pulse}`}>
        <CharacterSvg characterId={characterId} color={color} mutationStage={mutationStage} visual={visual} />
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
  const shade = characterId === 'berserker' ? '#7e2e2c' : '#f4ecd8';
  const common = { width: 58, height: 58, viewBox: '0 0 58 58', fill: 'none', xmlns: 'http://www.w3.org/2000/svg', overflow: 'visible' } as const;

  if (characterId === 'hunter') return (
    <svg {...common}>
      <path d="M29 6L46 17V34L29 49L12 34V17L29 6Z" fill={color} fillOpacity=".93" stroke="#3a2e1f" strokeWidth="2"/>
      <path d="M18 20H40L35 31H23L18 20Z" fill="#e8dcc0" stroke="#3a2e1f" strokeWidth="1.5"/>
      <circle cx="29" cy="25" r="3" fill="#3a2e1f"/>
      <path d="M29 11V17M29 39V46M11 29H17M41 29H47" stroke="#d4943d" strokeWidth="2" strokeLinecap="round"/>
      {visual.marked && <circle cx="29" cy="29" r={visual.hunt ? 26 : 23} stroke={visual.hunt ? '#d4943d' : '#c46d3d'} strokeWidth={visual.hunt ? 3 : 2} strokeDasharray={visual.hunt ? '5 4' : '3 4'} />}
      {visual.hunt && <circle cx="29" cy="29" r="4" stroke="#d4943d" strokeWidth="2" />}
    </svg>
  );

  if (characterId === 'engineer') return (
    <svg {...common}>
      <path d="M29 5L45 14V32L29 47L13 32V14L29 5Z" fill={color} fillOpacity=".92" stroke="#3a2e1f" strokeWidth="2"/>
      <path d="M22 20L29 14L36 20V32L29 38L22 32V20Z" fill="#e8dcc0" stroke="#3a2e1f" strokeWidth="1.5"/>
      <circle cx="29" cy="26" r="4" fill={color}/>
      <path d="M11 18L17 22M47 18L41 22M11 40L17 35M47 40L41 35" stroke={color} strokeWidth="2" strokeLinecap="round"/>
      <circle cx="9" cy="17" r="2.5" fill="#4a7a8a"/><circle cx="49" cy="17" r="2.5" fill="#4a7a8a"/>
      {visual.linkedCount >= 2 && <>
        {[0, 1, 2, 3].map((index) => {
          const a = (index / 4) * Math.PI * 2 - Math.PI / 4;
          const x = 29 + Math.cos(a) * 22;
          const y = 29 + Math.sin(a) * 22;
          return <g key={index}><line x1="29" y1="29" x2={x} y2={y} stroke="#4a7a8a" strokeWidth="1.5" opacity=".65" /><circle cx={x} cy={y} r="3" fill="#4a7a8a" opacity={visual.linkedCount >= 4 ? 1 : .7} /></g>;
        })}
      </>}
      {visual.linkedCount >= 3 && <circle cx="29" cy="29" r="25" stroke="#4a7a8a" strokeWidth="1.5" strokeDasharray="2 4" opacity=".8" />}
    </svg>
  );

  if (characterId === 'berserker') return (
    <svg {...common}>
      {visual.closeEnemy && <circle cx="29" cy="29" r={26 + visual.furySteps * 2} stroke="#c4453d" strokeWidth={2 + visual.furySteps * .5} strokeDasharray="6 3" opacity=".7" />}
      <path d="M14 17L22 10L29 15L36 10L44 17L41 39L29 50L17 39L14 17Z" fill={color} stroke="#3a2e1f" strokeWidth="2"/>
      <path d="M14 17L7 10L10 25L18 21M44 17L51 10L48 25L40 21" fill={color} stroke="#3a2e1f" strokeWidth="2" strokeLinejoin="round"/>
      <path d="M20 27L25 25M38 27L33 25M22 34L29 38L36 34" stroke={shade} strokeWidth="2.4" strokeLinecap="round"/>
      {mutationStage > 0 && <path d="M29 7L31 2L33 8M20 46L16 52M38 46L42 52" stroke="#d4943d" strokeWidth="2" strokeLinecap="round"/>}
      {Array.from({ length: 4 }, (_, index) => index < visual.furySteps ? index : null).filter((index): index is number => index !== null).map((index) => {
        const a = -Math.PI / 2 + index * (Math.PI / 2);
        return <circle key={index} cx={29 + Math.cos(a) * 27} cy={29 + Math.sin(a) * 27} r="2.5" fill="#c4453d" />;
      })}
    </svg>
  );

  if (characterId === 'alchemist') return (
    <svg {...common}>
      <path d="M23 7H35V16L43 23V39C43 44 37 48 29 48C21 48 15 44 15 39V23L23 16V7Z" fill={color} fillOpacity=".9" stroke="#3a2e1f" strokeWidth="2"/>
      <path d="M23 7H35" stroke="#3a2e1f" strokeWidth="3" strokeLinecap="round"/>
      <path d="M18 31C23 27 35 27 40 31V39C35 43 23 43 18 39V31Z" fill="#e8dcc0" fillOpacity=".65"/>
      <path d="M24 17H34" stroke="#e8dcc0" strokeWidth="2" strokeLinecap="round"/>
      {mutationStage > 0 && <circle cx="29" cy="35" r="3" fill="#d4943d"/>}
      {visual.reactionReady && <>
        <circle cx="13" cy="13" r="4" fill="#c46d3d" opacity=".9" />
        <circle cx="45" cy="13" r="4" fill="#5a8c4a" opacity=".9" />
        <circle cx="29" cy="52" r="4" fill="#4a7a8a" opacity=".9" />
        <path d="M16 15L24 23M42 15L34 23M29 48L29 40" stroke="#8a5a8a" strokeWidth="1.5" strokeDasharray="2 2" />
      </>}
      {visual.catalyst && <circle cx="29" cy="29" r="25" stroke="#d4943d" strokeWidth="2.5" strokeDasharray="5 3" />}
    </svg>
  );

  if (characterId === 'architect') return (
    <svg {...common}>
      <rect x="11" y="11" width="36" height="36" rx="3" fill="#e8dcc0" stroke={color} strokeWidth="3" transform="rotate(45 29 29)"/>
      <path d="M29 13L43 37H15L29 13Z" fill={color} fillOpacity=".78" stroke="#3a2e1f" strokeWidth="2"/>
      <path d="M29 21V36M21 34H37" stroke="#e8dcc0" strokeWidth="2" strokeLinecap="round"/>
      {mutationStage > 1 && <circle cx="29" cy="29" r="20" stroke="#d4943d" strokeWidth="2" strokeDasharray="4 4"/>}
      {visual.formation === 'line' && <line x1="7" y1="51" x2="51" y2="7" stroke="#d4943d" strokeWidth="2.5" opacity={Math.max(.35, visual.formationStrength)} />}
      {visual.formation === 'triangle' && <path d="M29 5L52 48H6Z" fill="none" stroke="#d4943d" strokeWidth="2" opacity={Math.max(.35, visual.formationStrength)} />}
      {visual.formation === 'square' && <rect x="6" y="6" width="46" height="46" fill="none" stroke="#d4943d" strokeWidth="2" opacity={Math.max(.35, visual.formationStrength)} />}
      {visual.formation === 'cluster' && <circle cx="29" cy="29" r="24" fill="none" stroke="#d4943d" strokeWidth="2" strokeDasharray="3 3" opacity={Math.max(.35, visual.formationStrength)} />}
    </svg>
  );

  return (
    <svg {...common}>
      <path d="M29 5L47 19L38 43L29 50L20 43L11 19L29 5Z" fill={color} fillOpacity=".92" stroke="#3a2e1f" strokeWidth="2"/>
      <path d="M20 17L29 11L38 17L34 29L29 38L24 29L20 17Z" fill="#e8dcc0" fillOpacity=".55" stroke="#3a2e1f" strokeWidth="1.5"/>
      <circle cx="29" cy="28" r="4" fill="#3a2e1f"/>
      <path d="M9 18L15 23M49 18L43 23M11 42L18 36M47 42L40 36" stroke={color} strokeWidth="2" strokeLinecap="round"/>
      {mutationStage > 1 && <circle cx="29" cy="28" r="21" stroke="#d4943d" strokeWidth="2" strokeDasharray="3 4"/>}
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
