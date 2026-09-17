import { useEffect, useRef, useState } from 'react';
import { Pause, Zap } from 'lucide-react';
import { ABILITIES, SPHERE_TYPES, type AbilityType, type SphereType } from './gameData';
import { activateByKey, activateDash, getMaxSpheres, placeSphere, setSphereType, type GameState } from './engine';
import { CHARACTER_DEFS } from './characters';
import type { Lang, TranslationKey } from './i18n';
import { loadInterfaceScale } from './interfaceScale';
import { createMasteryRunTracker, getMasteryRunXp, tickCharacterMastery } from './characterMastery';
import { addCharacterMasteryXp } from './persistence';

export type Handedness = 'right' | 'left';
type PointerState = { startX: number; startY: number; moved: boolean; joystickCandidate: boolean };
type JoystickVisual = { pointerId: number; x: number; y: number; dx: number; dy: number; active: boolean };

const JOYSTICK_DEADZONE = 12;
const JOYSTICK_RADIUS = 58;
const JOYSTICK_KNOB_RADIUS = 24;
const TOWER_TOUCH_TOLERANCE = 26;
const PLACEMENT_RADIUS = 175;
const PLACEMENT_NODES = 8;

function isBlockedByControl(target: EventTarget | null): boolean {
  return target instanceof Element && Boolean(target.closest('[data-mobile-control="true"]'));
}

function getSoftPlacementPoint(st: GameState, desired: { x: number; y: number }): { x: number; y: number } | null {
  const occupied = st.spheres
    .filter((sphere) => sphere.alive)
    .map((sphere) => sphere.pos);
  const candidates = Array.from({ length: PLACEMENT_NODES }, (_, index) => {
    const angle = (index / PLACEMENT_NODES) * Math.PI * 2;
    const x = st.player.pos.x + Math.cos(angle) * PLACEMENT_RADIUS;
    const y = st.player.pos.y + Math.sin(angle) * PLACEMENT_RADIUS;
    const worldLimitX = st.worldWidth / 2 - 80;
    const worldLimitY = st.worldHeight / 2 - 80;
    return {
      x: Math.max(-worldLimitX, Math.min(worldLimitX, x)),
      y: Math.max(-worldLimitY, Math.min(worldLimitY, y)),
    };
  });

  const available = candidates.filter((candidate) =>
    !occupied.some((position) => Math.hypot(position.x - candidate.x, position.y - candidate.y) < 70)
  );
  if (available.length === 0) return null;

  available.sort((a, b) =>
    Math.hypot(a.x - desired.x, a.y - desired.y) - Math.hypot(b.x - desired.x, b.y - desired.y)
  );
  return available[0];
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

  // Right-handed: joystick right, action controls left.
  // Left-handed: joystick left, action controls right.
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
    if (nearExistingTower) {
      placeSphere(st, world.x, world.y);
      return;
    }

    if (st.spheres.length >= getMaxSpheres(st)) return;
    const placementPoint = getSoftPlacementPoint(st, world);
    if (placementPoint) placeSphere(st, placementPoint.x, placementPoint.y);
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

      {joystick && (
        <div className="absolute pointer-events-none" style={{ left: joystick.x - JOYSTICK_RADIUS, top: joystick.y - JOYSTICK_RADIUS, width: JOYSTICK_RADIUS * 2, height: JOYSTICK_RADIUS * 2, transform: `scale(${interfaceScale})`, transformOrigin: 'center' }}>
          <div className="absolute inset-0 rounded-full border border-white/20 bg-black/20 backdrop-blur-[2px]" />
          <div className={`absolute rounded-full border border-white/30 bg-white/25 ${joystick.active ? 'opacity-90' : 'opacity-70'}`} style={{ width: JOYSTICK_KNOB_RADIUS * 2, height: JOYSTICK_KNOB_RADIUS * 2, left: JOYSTICK_RADIUS - JOYSTICK_KNOB_RADIUS + joystick.dx, top: JOYSTICK_RADIUS - JOYSTICK_KNOB_RADIUS + joystick.dy }} />
        </div>
      )}

      <div className={`absolute bottom-4 ${controlsSide} flex flex-col ${controlsAlign} gap-2 pointer-events-none`} style={{ transform: `scale(${interfaceScale})`, transformOrigin: controlsOnRight ? 'right bottom' : 'left bottom' }}>
        <button data-mobile-control="true" className="pointer-events-auto w-12 h-12 rounded-full bg-[#e8dcc0]/90 border border-[#c4b890] shadow-lg flex items-center justify-center text-[#5a4a32] active:scale-95" onPointerDown={(e) => { e.stopPropagation(); onPause(); }} aria-label={t('pause')}><Pause size={18} /></button>

        <div className="grid grid-cols-2 gap-1.5 pointer-events-auto">
          {activeAbilities.map(([key, ability]) => {
            const def = ABILITIES[ability];
            const cd = getAbilityCooldown(stateRef.current, ability);
            return <button key={`${key}-${ability}`} data-mobile-control="true" className="relative w-16 h-12 rounded-xl bg-[#e8dcc0]/90 border border-[#c4b890] shadow-lg px-1 overflow-hidden active:scale-95" onPointerDown={(e) => { e.stopPropagation(); const st = stateRef.current; if (!st || st.gameOver || st.paused) return; if (st.pendingUpgrade || st.pendingArtifact || st.pendingEvolution || st.pendingTowerUpgrade || st.pendingChest) return; activateByKey(st, key); }}>
              <span className="text-[9px] text-[#8a7a5a] block truncate">{def.name[lang]}</span>
              <span className="text-[8px] text-[#5a4a32]/60">{cd > 0 ? `${Math.ceil(cd)}s` : t('ready')}</span>
              {cd > 0 && <span className="absolute inset-x-0 bottom-0 h-1 bg-[#c4453d]/70" style={{ width: `${Math.min(100, (cd / getAbilityMaxCooldown(ability)) * 100)}%` }} />}
            </button>;
          })}
        </div>

        <button data-mobile-control="true" className="pointer-events-auto w-16 h-16 rounded-full bg-[#d4943d]/85 border border-[#c46d3d] shadow-lg flex flex-col items-center justify-center text-[#3a2e1f] active:scale-95" onPointerDown={(e) => { e.stopPropagation(); const st = stateRef.current; if (!st || st.gameOver || st.paused) return; const { x, y } = lastDirectionRef.current; clearMovementKeys(); if (y < -0.2) st.keys.w = true; if (y > 0.2) st.keys.s = true; if (x < -0.2) st.keys.a = true; if (x > 0.2) st.keys.d = true; activateDash(st); clearMovementKeys(); }} aria-label={t('dashCooldown')}>
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
            return <button key={type} data-mobile-control="true" className={`w-11 h-11 rounded-xl border bg-[#e8dcc0]/90 shadow-lg flex items-center justify-center active:scale-95 ${selected ? 'border-[#8a7a5a]' : 'border-[#c4b890]'} ${preferred ? 'ring-2 ring-[#d4943d]/45 ring-offset-1 ring-offset-[#e8dcc0]' : ''}`} style={{ borderColor: selected ? def.color : undefined }} onPointerDown={(e) => { e.stopPropagation(); const st = stateRef.current; if (!st) return; setSphereType(st, type); }} aria-label={`${def.name[lang]} — ${def.desc[lang]}`}><span className="w-4 h-4 rounded-full" style={{ backgroundColor: def.color }} /></button>;
          })}
        </div>
      </div>
    </div>
  );
}

function CharacterAvatarOverlay({ stateRef }: { stateRef: React.MutableRefObject<GameState | null> }) {
  const [characterId, setCharacterId] = useState(() => stateRef.current?.player.characterId || 'spherist');
  const [mutationStage, setMutationStage] = useState(() => stateRef.current?.player.mutationStage || 0);
  const frameRef = useRef<number | null>(null);
  const masteryRef = useRef(createMasteryRunTracker());
  const rewardedRef = useRef(false);

  useEffect(() => {
    const tick = () => {
      const st = stateRef.current;
      const nextCharacter = st?.player.characterId || 'spherist';
      const nextMutation = st?.player.mutationStage || 0;
      setCharacterId((current) => current === nextCharacter ? current : nextCharacter);
      setMutationStage((current) => current === nextMutation ? current : nextMutation);

      if (st && !st.gameOver) {
        tickCharacterMastery(st, 1 / 60, masteryRef.current);
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
  const background = getAvatarBackground(stateRef.current?.mapTheme);
  const pulse = mutationStage >= 3 ? 'animate-pulse' : '';

  return (
    <div className="absolute left-1/2 top-1/2 pointer-events-none" style={{ transform: 'translate(-50%, -50%)', width: 74, height: 74 }}>
      <div className="absolute inset-0 rounded-full" style={{ background, boxShadow: '0 3px 7px rgba(58,46,31,0.14)' }} />
      <div className={`absolute inset-0 flex items-center justify-center ${pulse}`}>
        <CharacterSvg characterId={characterId} color={color} mutationStage={mutationStage} />
      </div>
    </div>
  );
}

function CharacterSvg({ characterId, color, mutationStage }: { characterId: string; color: string; mutationStage: number }) {
  const shade = characterId === 'berserker' ? '#7e2e2c' : '#f4ecd8';
  const common = { width: 58, height: 58, viewBox: '0 0 58 58', fill: 'none', xmlns: 'http://www.w3.org/2000/svg' } as const;

  if (characterId === 'hunter') return (
    <svg {...common}>
      <path d="M29 6L46 17V34L29 49L12 34V17L29 6Z" fill={color} fillOpacity=".93" stroke="#3a2e1f" strokeWidth="2"/>
      <path d="M18 20H40L35 31H23L18 20Z" fill="#e8dcc0" stroke="#3a2e1f" strokeWidth="1.5"/>
      <circle cx="29" cy="25" r="3" fill="#3a2e1f"/>
      <path d="M29 11V17M29 39V46M11 29H17M41 29H47" stroke="#d4943d" strokeWidth="2" strokeLinecap="round"/>
    </svg>
  );

  if (characterId === 'engineer') return (
    <svg {...common}>
      <path d="M29 5L45 14V32L29 47L13 32V14L29 5Z" fill={color} fillOpacity=".92" stroke="#3a2e1f" strokeWidth="2"/>
      <path d="M22 20L29 14L36 20V32L29 38L22 32V20Z" fill="#e8dcc0" stroke="#3a2e1f" strokeWidth="1.5"/>
      <circle cx="29" cy="26" r="4" fill={color}/>
      <path d="M11 18L17 22M47 18L41 22M11 40L17 35M47 40L41 35" stroke={color} strokeWidth="2" strokeLinecap="round"/>
      <circle cx="9" cy="17" r="2.5" fill="#4a7a8a"/><circle cx="49" cy="17" r="2.5" fill="#4a7a8a"/>
    </svg>
  );

  if (characterId === 'berserker') return (
    <svg {...common}>
      <path d="M14 17L22 10L29 15L36 10L44 17L41 39L29 50L17 39L14 17Z" fill={color} stroke="#3a2e1f" strokeWidth="2"/>
      <path d="M14 17L7 10L10 25L18 21M44 17L51 10L48 25L40 21" fill={color} stroke="#3a2e1f" strokeWidth="2" strokeLinejoin="round"/>
      <path d="M20 27L25 25M38 27L33 25M22 34L29 38L36 34" stroke={shade} strokeWidth="2.4" strokeLinecap="round"/>
      {mutationStage > 0 && <path d="M29 7L31 2L33 8M20 46L16 52M38 46L42 52" stroke="#d4943d" strokeWidth="2" strokeLinecap="round"/>}
    </svg>
  );

  if (characterId === 'alchemist') return (
    <svg {...common}>
      <path d="M23 7H35V16L43 23V39C43 44 37 48 29 48C21 48 15 44 15 39V23L23 16V7Z" fill={color} fillOpacity=".9" stroke="#3a2e1f" strokeWidth="2"/>
      <path d="M23 7H35" stroke="#3a2e1f" strokeWidth="3" strokeLinecap="round"/>
      <path d="M18 31C23 27 35 27 40 31V39C35 43 23 43 18 39V31Z" fill="#e8dcc0" fillOpacity=".65"/>
      <path d="M24 17H34" stroke="#e8dcc0" strokeWidth="2" strokeLinecap="round"/>
      {mutationStage > 0 && <circle cx="29" cy="35" r="3" fill="#d4943d"/>}
    </svg>
  );

  if (characterId === 'architect') return (
    <svg {...common}>
      <rect x="11" y="11" width="36" height="36" rx="3" fill="#e8dcc0" stroke={color} strokeWidth="3" transform="rotate(45 29 29)"/>
      <path d="M29 13L43 37H15L29 13Z" fill={color} fillOpacity=".78" stroke="#3a2e1f" strokeWidth="2"/>
      <path d="M29 21V36M21 34H37" stroke="#e8dcc0" strokeWidth="2" strokeLinecap="round"/>
      {mutationStage > 1 && <circle cx="29" cy="29" r="20" stroke="#d4943d" strokeWidth="2" strokeDasharray="4 4"/>}
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

function getAvatarBackground(mapTheme: GameState['mapTheme'] | undefined): string {
  switch (mapTheme) {
    case 'bamboo': return '#e8e0c4';
    case 'ocean': return '#d8e0e4';
    case 'sunset': return '#f0d8c0';
    default: return '#f4ecd8';
  }
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
