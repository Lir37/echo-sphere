import { useCallback, useEffect, useRef, useState } from 'react';
import { Settings, Store, Trophy, Play, Globe, ArrowLeft, RotateCcw, Award, Volume2, VolumeX } from 'lucide-react';
import { translations, type Lang, type TranslationKey } from './i18n';
import {
  ABILITIES, ARTIFACTS, ARTIFACT_MAP, EVOLUTION_MAP, SHOP_UPGRADES, shopCost,
  abilityName, artifactName, type AbilityType, type ArtifactId,
  DIFFICULTIES, ACHIEVEMENTS, SPHERE_TYPES, type Difficulty, type SphereType,
} from './gameData';
import {
  createInitialState, update, placeSphere, activateByKey,
  generateUpgradeChoices, applyUpgrade, applyArtifact,
  applyTowerUpgrade, activateDash, openChest, setSphereType,
  getMaxSpheres, getMoveSpeed, getSphereRadius, getSphereDamage, getSphereDelay,
  getCritChance, getDodgeChance, getVampirePercent,
  type GameState, type ShopState, type LeaderEntry, type UpgradeChoice,
  type TowerUpgradeChoice, MAP_THEMES, type MapTheme,
} from './engine';
import { render } from './renderer';
import {
  loadShop, saveShop, loadLeaderboard, addLeaderEntry, loadLang, saveLang,
  loadName, saveName, resetAll, saveGold, loadGold,
  loadAchievements, unlockAchievement, loadDifficulty, saveDifficulty,
  loadSound, saveSound,
} from './persistence';
import { playSound, setAudioEnabled } from './audio';

function normalizeKey(e: KeyboardEvent): string {
  const code = e.code;
  if (code === 'KeyW' || code === 'ArrowUp') return 'w';
  if (code === 'KeyS' || code === 'ArrowDown') return 's';
  if (code === 'KeyA' || code === 'ArrowLeft') return 'a';
  if (code === 'KeyD' || code === 'ArrowRight') return 'd';
  if (code === 'Space') return ' ';
  if (code === 'Escape') return 'escape';
  if (code === 'ShiftLeft' || code === 'ShiftRight') return 'shift';
  if (code === 'KeyE') return 'e';
  if (code === 'KeyQ') return 'q';
  if (code === 'KeyR') return 'r';
  if (code === 'KeyF') return 'f';
  if (code === 'KeyG') return 'g';
  return e.key.toLowerCase();
}

type Screen = 'menu' | 'game' | 'shop' | 'leaderboard' | 'settings' | 'achievements';

export default function App() {
  const [lang, setLang] = useState<Lang>(() => loadLang());
  const [screen, setScreen] = useState<Screen>('menu');
  const [shop, setShop] = useState<ShopState>(() => loadShop());
  const [difficulty, setDifficulty] = useState<Difficulty>(() => loadDifficulty() as Difficulty);
  const [soundOn, setSoundOn] = useState<boolean>(() => loadSound());
  const [mapTheme, setMapTheme] = useState<MapTheme>(() => (localStorage.getItem('echosphere_map') || 'parchment') as MapTheme);

  const t = (k: TranslationKey) => translations[lang][k];

  useEffect(() => { saveLang(lang); }, [lang]);
  useEffect(() => { saveDifficulty(difficulty); }, [difficulty]);
  useEffect(() => {
    saveSound(soundOn);
    setAudioEnabled(soundOn);
  }, [soundOn]);

  return (
    <div className="min-h-screen w-full bg-[#f4ecd8] text-[#3a2e1f] overflow-hidden flex items-center justify-center" style={{ fontFamily: 'Georgia, "Times New Roman", serif' }}>
      {screen === 'menu' && <Menu lang={lang} setLang={setLang} t={t} difficulty={difficulty} setDifficulty={setDifficulty} soundOn={soundOn} setSoundOn={setSoundOn} mapTheme={mapTheme} setMapTheme={setMapTheme} onPlay={(mt) => { setMapTheme(mt); setScreen('game'); }} onShop={() => setScreen('shop')} onLeader={() => setScreen('leaderboard')} onSettings={() => setScreen('settings')} onAchievements={() => setScreen('achievements')} />}
      {screen === 'game' && <GameScreen lang={lang} t={t} shop={shop} difficulty={difficulty} mapTheme={mapTheme} onExit={() => { setShop(loadShop()); setScreen('menu'); }} />}
      {screen === 'shop' && <ShopScreen lang={lang} t={t} shop={shop} setShop={setShop} onBack={() => setScreen('menu')} />}
      {screen === 'leaderboard' && <LeaderboardScreen lang={lang} t={t} onBack={() => setScreen('menu')} />}
      {screen === 'settings' && <SettingsScreen lang={lang} setLang={setLang} t={t} soundOn={soundOn} setSoundOn={setSoundOn} onBack={() => setScreen('menu')} />}
      {screen === 'achievements' && <AchievementsScreen lang={lang} t={t} onBack={() => setScreen('menu')} />}
    </div>
  );
}

// ===== Menu =====
function Menu({ lang, setLang, t, difficulty, setDifficulty, soundOn, setSoundOn, mapTheme, setMapTheme, onPlay, onShop, onLeader, onSettings, onAchievements }: {
  lang: Lang; setLang: (l: Lang) => void; t: (k: TranslationKey) => string;
  difficulty: Difficulty; setDifficulty: (d: Difficulty) => void;
  soundOn: boolean; setSoundOn: (v: boolean) => void;
  mapTheme: MapTheme; setMapTheme: (m: MapTheme) => void;
  onPlay: (mapTheme: MapTheme) => void; onShop: () => void; onLeader: () => void; onSettings: () => void; onAchievements: () => void;
}) {
  const [name, setName] = useState(() => loadName());
  useEffect(() => { saveName(name); }, [name]);
  useEffect(() => { localStorage.setItem('echosphere_map', mapTheme); }, [mapTheme]);

  return (
    <div className="relative w-full max-w-md mx-auto px-6 py-12 flex flex-col items-center gap-6 overflow-y-auto max-h-screen">
      <div className="text-center mt-4">
        <h1 className="text-5xl font-bold tracking-tight" style={{ color: '#3a2e1f', textShadow: '2px 2px 0 rgba(58,46,31,0.1)' }}>
          {t('title')}
        </h1>
        <p className="text-sm text-[#8a7a5a] mt-2 tracking-widest uppercase">Horde Survival</p>
      </div>

      <div className="w-full flex flex-col gap-3">
        <label className="text-xs text-[#8a7a5a] uppercase tracking-wider">{t('name')}</label>
        <input
          value={name}
          onChange={(e) => setName(e.target.value.slice(0, 16))}
          placeholder={t('namePlaceholder')}
          className="w-full px-4 py-3 bg-[#e8dcc0] border border-[#c4b890] rounded-xl text-[#3a2e1f] placeholder-[#8a7a5a]/50 focus:outline-none focus:border-[#8a7a5a] transition"
        />
      </div>

      {/* Difficulty selection */}
      <div className="w-full flex flex-col gap-2">
        <label className="text-xs text-[#8a7a5a] uppercase tracking-wider">{t('difficulty')}</label>
        <div className="grid grid-cols-2 gap-2">
          {DIFFICULTIES.map((d) => (
            <button
              key={d.id}
              onClick={() => setDifficulty(d.id)}
              className={`px-3 py-3 rounded-xl border transition text-center ${difficulty === d.id
                ? 'bg-[#d4943d]/20 border-[#c46d3d]/50 text-[#3a2e1f]'
                : 'bg-[#e8dcc0] border-[#c4b890] text-[#8a7a5a] hover:border-[#8a7a5a]'}`}
            >
              <div className="font-bold text-sm">{d.name[lang]}</div>
              <div className="text-[10px] text-[#8a7a5a]/70 mt-1">{t('goldMultiplier')}: x{d.goldMult}</div>
            </button>
          ))}
        </div>
        <p className="text-xs text-[#8a7a5a]/70 mt-1">
          {DIFFICULTIES.find(d => d.id === difficulty)?.desc[lang]}
        </p>
      </div>

      {/* Map selection */}
      <div className="w-full flex flex-col gap-2">
        <label className="text-xs text-[#8a7a5a] uppercase tracking-wider">{t('chooseMap')}</label>
        <div className="grid grid-cols-4 gap-2">
          {MAP_THEMES.map((m) => (
            <button
              key={m.id}
              onClick={() => setMapTheme(m.id)}
              className={`px-2 py-3 rounded-xl border transition text-center ${mapTheme === m.id
                ? 'bg-[#d4943d]/20 border-[#c46d3d]/50 text-[#3a2e1f]'
                : 'bg-[#e8dcc0] border-[#c4b890] text-[#8a7a5a] hover:border-[#8a7a5a]'}`}
            >
              <div className="font-bold text-xs">{m.name[lang]}</div>
            </button>
          ))}
        </div>
      </div>

      <div className="w-full flex flex-col gap-3">
        <MenuButton icon={<Play size={20} />} label={t('play')} onClick={() => onPlay(mapTheme)} primary />
        <div className="grid grid-cols-2 gap-3">
          <MenuButton icon={<Store size={20} />} label={t('shop')} onClick={onShop} />
          <MenuButton icon={<Trophy size={20} />} label={t('leaderboard')} onClick={onLeader} />
          <MenuButton icon={<Award size={20} />} label={t('achievements')} onClick={onAchievements} />
          <MenuButton icon={<Settings size={20} />} label={t('settings')} onClick={onSettings} />
        </div>
      </div>

      <div className="absolute top-4 right-4 flex gap-2">
        <button
          onClick={() => setSoundOn(!soundOn)}
          className="flex items-center gap-2 px-3 py-2 rounded-lg bg-[#e8dcc0] border border-[#c4b890] text-[#5a4a32] hover:text-[#3a2e1f] hover:border-[#8a7a5a] transition text-sm"
        >
          {soundOn ? <Volume2 size={16} /> : <VolumeX size={16} />}
        </button>
        <button
          onClick={() => setLang(lang === 'ru' ? 'en' : 'ru')}
          className="flex items-center gap-2 px-3 py-2 rounded-lg bg-[#e8dcc0] border border-[#c4b890] text-[#5a4a32] hover:text-[#3a2e1f] hover:border-[#8a7a5a] transition text-sm"
        >
          <Globe size={16} /> {lang.toUpperCase()}
        </button>
      </div>
    </div>
  );
}

function MenuButton({ icon, label, onClick, primary }: { icon: React.ReactNode; label: string; onClick: () => void; primary?: boolean }) {
  return (
    <button
      onClick={onClick}
      className={`w-full flex items-center gap-3 px-5 py-4 rounded-xl border transition-all duration-200 hover:scale-[1.02] active:scale-[0.98] ${
        primary
          ? 'bg-[#e8dcc0] from-[#4a7a8a]/20 to-[#4a7a8a]/10 border-[#4a7a8a]/40 text-[#3a2e1f] hover:border-[#4a7a8a]/60 shadow-lg shadow-[#4a7a8a]/10'
          : 'bg-[#e8dcc0] border-[#c4b890] text-[#5a4a32] hover:border-[#a89878] hover:bg-[#e0d4b8]'
      }`}
    >
      <span className={primary ? 'text-[#4a7a8a]' : 'text-[#8a7a5a]'}>{icon}</span>
      <span className="font-medium">{label}</span>
    </button>
  );
}

// ===== Game Screen =====
function GameScreen({ lang, t, shop, difficulty, mapTheme, onExit }: {
  lang: Lang; t: (k: TranslationKey) => string; shop: ShopState; difficulty: Difficulty; mapTheme: MapTheme; onExit: () => void;
}) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const stateRef = useRef<GameState | null>(null);
  const rafRef = useRef<number>(0);
  const lastTimeRef = useRef<number>(0);
  const [, forceRender] = useState(0);
  const [gameOverData, setGameOverData] = useState<{ time: number; wave: number; gold: number; rank: number; isNewRecord: boolean } | null>(null);
  const [paused, setPaused] = useState(false);

  useEffect(() => {
    const name = loadName() || translations[lang].namePlaceholder;
    const s = createInitialState(shop, name, difficulty, mapTheme);
    stateRef.current = s;
    lastTimeRef.current = performance.now();
    const loop = (now: number) => {
      const dt = Math.min(0.05, (now - lastTimeRef.current) / 1000);
      lastTimeRef.current = now;
      const st = stateRef.current;
      if (st) {
        update(st, dt);
        if (st.gameOver && !gameOverData) {
          const time = Math.floor(st.time);
          const diff = DIFFICULTIES.find(d => d.id === st.difficulty)!;
          const gold = Math.floor((time * 0.15 + st.bossDefeated * 25 + st.stats.enemiesKilled * 0.05) * diff.goldMult);
          saveGold(loadGold() + gold);
          const entry: LeaderEntry = { name: name || 'Player', time, wave: st.wave, date: Date.now() };
          const { rank, isNewRecord } = addLeaderEntry(entry);
          // unlock achievements
          if (st.stats.enemiesKilled >= 500) unlockAchievement('kills_500');
          if (st.stats.enemiesKilled >= 1000) unlockAchievement('kills_1000');
          if (st.wave >= 30) unlockAchievement('wave_30');
          if (st.wave >= 50) unlockAchievement('wave_50');
          if (st.bossDefeated >= 5) unlockAchievement('boss_5');
          if (st.bossDefeated >= 10) unlockAchievement('boss_10');
          if (st.evolutionsThisRun >= 1) unlockAchievement('evolve_1');
          if (st.evolutionsThisRun >= 3) unlockAchievement('evolve_3');
          if (st.player.eliteKills >= 10) unlockAchievement('elite_10');
          if (st.player.combo >= 50) unlockAchievement('combo_50');
          if (st.player.chestOpens >= 5) unlockAchievement('chest_5');
          if (st.player.dashCount >= 50) unlockAchievement('dash_50');
          setGameOverData({ time, wave: st.wave, gold, rank, isNewRecord });
        }
        const canvas = canvasRef.current;
        if (canvas) {
          const ctx = canvas.getContext('2d');
          if (ctx) render(ctx, st, canvas.width, canvas.height);
        }
        forceRender(v => v + 1);
      }
      rafRef.current = requestAnimationFrame(loop);
    };
    rafRef.current = requestAnimationFrame(loop);
    return () => cancelAnimationFrame(rafRef.current);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      const st = stateRef.current;
      if (!st) return;
      const key = normalizeKey(e);
      if (key === 'escape') {
        e.preventDefault();
        if (e.type === 'keydown' && !e.repeat) {
          setPaused(p => { const np = !p; if (st) { st.paused = np; if (np) st.keys = {}; } return np; });
        }
        return;
      }
      if (st.gameOver || st.paused) return;
      if (st.pendingUpgrade || st.pendingArtifact || st.pendingEvolution || st.pendingTowerUpgrade || st.pendingChest) {
        if (e.type === 'keyup') st.keys[key] = false;
        return;
      }
      st.keys[key] = e.type === 'keydown';
      if (e.type === 'keydown' && ['e', 'q', 'r', 'f', 'g'].includes(key) && !e.repeat) {
        activateByKey(st, key);
      }
      if (e.type === 'keydown' && key === 'shift' && !e.repeat) {
        e.preventDefault();
        activateDash(st);
      }
      if (e.type === 'keydown' && key === ' ' && !e.repeat) {
        e.preventDefault();
        placeSphere(st, st.player.pos.x, st.player.pos.y);
      }
      if (e.type === 'keydown' && ['1','2','3','4','5'].includes(key) && !e.repeat) {
        const types: SphereType[] = ['standard', 'sniper', 'shotgun', 'chain', 'aura'];
        const idx = parseInt(key) - 1;
        if (idx < types.length) setSphereType(st, types[idx]);
      }
    };
    window.addEventListener('keydown', onKey);
    window.addEventListener('keyup', onKey);
    return () => { window.removeEventListener('keydown', onKey); window.removeEventListener('keyup', onKey); };
  }, []);

  const onCanvasClick = useCallback((e: React.MouseEvent<HTMLCanvasElement>) => {
    const st = stateRef.current;
    if (!st || st.gameOver || st.paused) return;
    if (st.pendingUpgrade || st.pendingArtifact || st.pendingEvolution || st.pendingTowerUpgrade || st.pendingChest) return;
    const canvas = canvasRef.current;
    if (!canvas) return;
    const rect = canvas.getBoundingClientRect();
    const sx = e.clientX - rect.left - canvas.width / 2 + st.camera.x;
    const sy = e.clientY - rect.top - canvas.height / 2 + st.camera.y;
    placeSphere(st, sx, sy);
  }, []);

  const st = stateRef.current;

  return (
    <div className="relative w-full h-screen flex items-center justify-center">
      <canvas
        ref={canvasRef}
        width={Math.min(window.innerWidth, 1280)}
        height={Math.min(window.innerHeight, 800)}
        onClick={onCanvasClick}
        className="max-w-full max-h-full"
        style={{ cursor: 'crosshair' }}
      />

      {/* HUD */}
      {st && !gameOverData && (
        <>
          <Hud lang={lang} t={t} st={st} />
          <SphereTypeSelector lang={lang} st={st} />
          <AbilityBar lang={lang} t={t} st={st} />
          {/* Combo indicator */}
          {st.player.combo >= 5 && (
            <div className="absolute top-16 left-1/2 -translate-x-1/2 pointer-events-none text-center">
              <div className="text-2xl font-bold" style={{ color: st.player.combo >= 50 ? '#d4943d' : st.player.combo >= 25 ? '#c46d3d' : '#c4453d' }}>
                {t('combo')} x{st.player.combo}
              </div>
              <div className="text-sm font-bold text-[#5a4a32]">
                {t('comboMultiplier').replace('{mult}', String(st.player.comboMult))}
              </div>
              <div className="w-24 h-1 bg-[#c4b890] rounded-full mt-1 overflow-hidden">
                <div className="h-full bg-[#d4943d] transition-all" style={{ width: `${(st.player.comboTimer / 3) * 100}%` }} />
              </div>
            </div>
          )}
          {/* Dash cooldown indicator */}
          <div className="absolute bottom-20 left-1/2 -translate-x-1/2 pointer-events-none">
            <div className={`text-xs font-bold ${st.player.dashCooldown <= 0 ? 'text-[#4a7a8a]' : 'text-[#8a7a5a]/50'}`}>
              {st.player.dashCooldown <= 0 ? t('dashCooldown') : t('dashOnCooldown').replace('{sec}', String(Math.ceil(st.player.dashCooldown)))}
            </div>
          </div>
          {st.pendingUpgrade && <UpgradeModal lang={lang} t={t} st={st} onPick={(c) => { applyUpgrade(st, c); st.pendingUpgrade = null; }} />}
          {st.pendingArtifact && <ArtifactModal lang={lang} t={t} choices={st.pendingArtifact} onPick={(id) => { applyArtifact(st, id); st.pendingArtifact = null; }} />}
          {st.pendingTowerUpgrade && <TowerUpgradeModal lang={lang} t={t} choices={st.pendingTowerUpgrade} onPick={(c) => { applyTowerUpgrade(st, c); }} />}
          {st.pendingChest && <ChestModal lang={lang} t={t} st={st} onPick={() => { openChest(st, 'artifact'); }} />}
          {paused && !st.pendingUpgrade && !st.pendingArtifact && !st.pendingTowerUpgrade && !st.pendingChest && (
            <div className="absolute inset-0 bg-black/70 flex items-center justify-center z-50">
              <div className="text-center max-w-md w-full px-6">
                <h2 className="text-3xl font-bold mb-4">{t('pauseTitle')}</h2>
                {st.player.artifacts.length > 0 && (
                  <div className="mb-6 bg-[#e8dcc0] border border-[#c4b890] rounded-xl p-4 text-left">
                    <div className="text-xs text-[#8a7a5a] uppercase tracking-wider mb-2">{t('artifacts')}</div>
                    <div className="flex flex-wrap gap-2">
                      {st.player.artifacts.map((id) => {
                        const a = ARTIFACT_MAP[id];
                        return (
                          <div key={id} className="flex items-center gap-1.5 px-2 py-1 rounded-lg bg-[#d4943d]/10 border border-[#d4943d]/20">
                            <span className="text-[#d4943d]">✦</span>
                            <span className="text-xs font-medium text-[#3a2e1f]">{a.name[lang]}</span>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                )}
                <button onClick={() => { setPaused(false); st.paused = false; }} className="px-6 py-3 rounded-xl bg-[#4a7a8a] border border-[#3a6a7a] text-white font-bold hover:bg-[#5a8a9a] transition mb-3 block w-48 mx-auto">{t('resume')}</button>
                <button onClick={onExit} className="px-6 py-3 rounded-xl bg-[#e8dcc0] border border-[#c4b890] text-[#5a4a32] hover:bg-[#e0d4b8] transition block w-48 mx-auto">{t('return')}</button>
              </div>
            </div>
          )}
        </>
      )}

      {/* Game Over */}
      {gameOverData && (
        <div className="absolute inset-0 bg-black/80 flex items-center justify-center z-50">
          <div className="text-center max-w-sm px-6">
            <h2 className="text-4xl font-bold text-[#c4453d] mb-2">{t('gameOver')}</h2>
            {gameOverData.isNewRecord && <p className="text-2xl font-bold text-[#d4943d] mb-4 animate-pulse">{t('newRecord')}</p>}
            <div className="bg-[#e8dcc0] border border-[#c4b890] rounded-xl p-6 mb-6 space-y-2 text-left">
              <Row label={t('survived')} value={`${gameOverData.time} ${t('seconds')}`} />
              <Row label={t('wave')} value={`${gameOverData.wave}`} />
              <Row label={t('goldEarned')} value={`${gameOverData.gold}`} />
              {gameOverData.rank > 0 && gameOverData.rank <= 10 && <Row label={t('rank')} value={`#${gameOverData.rank}`} />}
            </div>
            <button onClick={onExit} className="px-8 py-3 rounded-xl bg-[#4a7a8a]/20 border border-[#4a7a8a]/40 text-[#3a2e1f] hover:bg-[#4a7a8a]/30 transition w-full">{t('return')}</button>
          </div>
        </div>
      )}
    </div>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  return <div className="flex justify-between"><span className="text-[#8a7a5a]">{label}</span><span className="font-medium">{value}</span></div>;
}

// ===== Sphere Type Selector =====
function SphereTypeSelector({ lang, st }: { lang: Lang; st: GameState }) {
  const types: SphereType[] = ['standard', 'sniper', 'shotgun', 'chain', 'aura'];
  const [hovered, setHovered] = useState<SphereType | null>(null);
  const hoveredDef = hovered ? SPHERE_TYPES[hovered] : null;
  return (
    <>
      <div className="absolute bottom-20 right-4 flex flex-col gap-1.5 pointer-events-auto">
        {types.map((type, i) => {
          const def = SPHERE_TYPES[type];
          const selected = st.selectedSphereType === type;
          return (
            <button
              key={type}
              onClick={() => setSphereType(st, type)}
              onMouseEnter={() => setHovered(type)}
              onMouseLeave={() => setHovered(null)}
              onPointerDown={() => setHovered(type)}
              className={`flex items-center gap-2 px-3 py-2 rounded-lg border transition text-left ${selected ? 'bg-[#e0d4b8] border-[#8a7a5a]' : 'bg-[#e8dcc0] border-[#c4b890] opacity-50 hover:opacity-80'}`}
              style={{ borderColor: selected ? def.color : undefined }}
            >
              <span className="text-xs font-bold text-[#8a7a5a]/70 w-4">{i + 1}</span>
              <span className="w-3 h-3 rounded-full" style={{ backgroundColor: def.color, boxShadow: selected ? '0 0 8px ' + def.color : 'none' }} />
              <span className="text-xs font-medium" style={{ color: selected ? def.color : '#8a7a5a' }}>{def.name[lang]}</span>
            </button>
          );
        })}
      </div>
      {hoveredDef && (
        <div className="absolute bottom-20 right-24 w-56 p-3 rounded-xl bg-[#f4ecd8] border border-[#8a7a5a]/40 shadow-lg z-30 pointer-events-none">
          <div className="flex items-center gap-2 mb-1">
            <span className="w-3 h-3 rounded-full" style={{ backgroundColor: hoveredDef.color }} />
            <span className="font-bold text-sm" style={{ color: hoveredDef.color }}>{hoveredDef.name[lang]}</span>
          </div>
          <div className="text-xs text-[#5a4a32] mb-2">{hoveredDef.desc[lang]}</div>
          <div className="text-[10px] text-[#8a7a5a] space-y-0.5">
            <div>{lang === 'ru' ? 'Урон' : 'Damage'}: x{hoveredDef.damageMult}</div>
            <div>{lang === 'ru' ? 'Дальность' : 'Range'}: x{hoveredDef.rangeMult}</div>
            <div>{lang === 'ru' ? 'Скорость' : 'Speed'}: x{hoveredDef.projectileSpeedMult}</div>
            {hoveredDef.pellets > 1 && <div>{lang === 'ru' ? 'Снаряды' : 'Pellets'}: {hoveredDef.pellets}</div>}
            {hoveredDef.chain && <div>{lang === 'ru' ? 'Цепная молния' : 'Chain lightning'}</div>}
            {hoveredDef.aura && <div>{lang === 'ru' ? 'Аура урона' : 'Damage aura'}: {hoveredDef.auraRadius}px</div>}
          </div>
        </div>
      )}
    </>
  );
}

// ===== HUD =====
function Hud({ lang, t, st }: { lang: Lang; t: (k: TranslationKey) => string; st: GameState }) {
  const xpPct = (st.player.xp / st.player.xpToNext) * 100;
  const hpPct = (st.player.hp / st.player.maxHp) * 100;
  const mins = Math.floor(st.time / 60);
  const secs = Math.floor(st.time % 60);
  return (
    <>
      <div className="absolute top-3 left-3 flex flex-col gap-1.5 w-56 pointer-events-none">
        <div className="flex items-center gap-2 text-sm">
          <span className="text-[#4a7a8a] font-bold">{t('level')} {st.player.level}</span>
          <div className="flex-1 h-2 bg-[#2a2218] rounded-full overflow-hidden border border-[#3a2e1f]">
            <div className="h-full bg-gradient-to-r from-[#3a8ab0] to-[#6acaff] transition-all" style={{ width: `${xpPct}%` }} />
          </div>
        </div>
        <div className="flex items-center gap-2 text-sm">
          <span className="text-[#c4453d] font-bold text-xs w-10">{Math.ceil(st.player.hp)} HP</span>
          <div className="flex-1 h-3 bg-[#2a2218] rounded-full overflow-hidden border border-[#3a2e1f]">
            <div className="h-full bg-gradient-to-r from-[#c4453d] to-[#ff6b63] transition-all" style={{ width: `${hpPct}%` }} />
          </div>
        </div>
        {st.wave % 10 === 0 && st.bossActive && (
          <div className="text-[#c4453d] font-bold text-xs animate-pulse">{t('bossWave')}</div>
        )}
      </div>
      <div className="absolute top-3 right-3 flex flex-col items-end gap-1 text-sm pointer-events-none">
        <span className="text-[#5a4a32] font-mono">{mins.toString().padStart(2, '0')}:{secs.toString().padStart(2, '0')}</span>
        <span className="text-[#4a7a8a]/80 text-xs">{t('spheres')}: {st.spheres.length}/{getMaxSpheres(st)}</span>
        <span className="text-[#8a7a5a]/70 text-xs">{t('wave')} {st.wave}</span>
        {st.player.buffTimer > 0 && <span className="text-[#c46d3d] text-xs font-bold animate-pulse">BUFF {Math.ceil(st.player.buffTimer)}s</span>}
      </div>
    </>
  );
}

// ===== Ability Bar =====
function AbilityBar({ lang, t, st }: { lang: Lang; t: (k: TranslationKey) => string; st: GameState }) {
  const activeAbilities = Object.entries(st.activeKeyMap) as [string, AbilityType][];
  const cooldownMap: Partial<Record<AbilityType, number>> = {
    blast: st.player.blastCooldown, shield: st.player.shieldCooldown, teleport: st.player.teleportCooldown,
    firetrail: st.player.fireTrailCooldown, minion: st.player.minionCooldown, lightning: st.player.lightningCooldown,
    timestop: st.player.timestopCooldown, darkritual: st.player.darkritualCooldown,
  };
  const maxCdMap: Partial<Record<AbilityType, number>> = {
    blast: 30, shield: 20, teleport: 15, firetrail: 25, minion: 30, lightning: 20, timestop: 40, darkritual: 30,
  };
  return (
    <div className="absolute bottom-4 left-1/2 -translate-x-1/2 flex gap-2 pointer-events-none">
      {activeAbilities.map(([key, ability]) => {
        const def = ABILITIES[ability];
        const cd = cooldownMap[ability] || 0;
        const maxCd = maxCdMap[ability] || 1;
        const pct = cd > 0 ? (cd / maxCd) * 100 : 0;
        return (
          <div key={key} className="relative w-14 h-14 rounded-lg bg-[#e8dcc0] border border-[#c4b890] flex flex-col items-center justify-center overflow-hidden">
            <span className="text-[10px] text-[#8a7a5a]/70 uppercase absolute top-1 left-1">{key}</span>
            <span className="text-[9px] text-[#3a2e1f] text-center px-1 leading-tight">{def.name[lang]}</span>
            {cd > 0 && (
              <>
                <div className="absolute inset-0 bg-black/60 flex items-center justify-center">
                  <span className="text-sm font-bold text-[#3a2e1f]">{Math.ceil(cd)}</span>
                </div>
                <div className="absolute bottom-0 left-0 right-0 h-1 bg-[#c4453d]" style={{ width: `${pct}%` }} />
              </>
            )}
          </div>
        );
      })}
    </div>
  );
}

// ===== Upgrade Modal =====
function UpgradeModal({ lang, t, st, onPick }: {
  lang: Lang; t: (k: TranslationKey) => string; st: GameState; onPick: (c: UpgradeChoice) => void;
}) {
  const choices = st.pendingUpgrade || [];
  return (
    <div className="absolute inset-0 bg-black/70 flex items-center justify-center z-50">
      <div className="max-w-2xl w-full px-6">
        <h2 className="text-2xl font-bold text-center mb-6 text-[#4a7a8a]">
          {choices[0]?.type === 'evolve' ? t('evolution') : t('chooseUpgrade')}
        </h2>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          {choices.map((c, i) => {
            if (c.type === 'evolve' && c.evolution) {
              const evo = EVOLUTION_MAP[c.evolution];
              return (
                <button key={i} onClick={() => onPick(c)} className="p-5 rounded-xl bg-[#e8dcc0] from-[#d4943d]/20 to-[#c46d3d]/10 border border-[#d4943d]/40 hover:border-[#d4943d]/60 hover:scale-105 transition-all text-left">
                  <div className="text-[#d4943d] text-xs uppercase mb-1">{t('evolution')}</div>
                  <div className="font-bold text-lg mb-2">{evo.name[lang]}</div>
                  <div className="text-sm text-[#5a4a32]">{evo.desc[lang]}</div>
                </button>
              );
            }
            const def = ABILITIES[c.ability!];
            return (
              <button key={i} onClick={() => onPick(c)} className="p-5 rounded-xl bg-[#e8dcc0] from-[#4a7a8a]/15 to-[#4a7a8a]/5 border border-[#4a7a8a]/30 hover:border-[#4a7a8a]/60 hover:scale-105 transition-all text-left">
                <div className="text-[#4a7a8a] text-xs uppercase mb-1">{def.category === 'active' ? t('active') : t('passive')}</div>
                <div className="font-bold text-lg mb-2">{def.name[lang]}</div>
                <div className="text-sm text-[#5a4a32] mb-2">{def.desc[lang](c.newLevel)}</div>
                <div className="text-xs text-[#8a7a5a]/70">{t('level')} {c.currentLevel} → {c.newLevel} / {def.maxLevel}</div>
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
}

// ===== Tower Upgrade Modal =====
function TowerUpgradeModal({ lang, t, choices, onPick }: {
  lang: Lang; t: (k: TranslationKey) => string; choices: TowerUpgradeChoice[]; onPick: (c: TowerUpgradeChoice) => void;
}) {
  return (
    <div className="absolute inset-0 bg-black/70 flex items-center justify-center z-50">
      <div className="max-w-2xl w-full px-6">
        <h2 className="text-2xl font-bold text-center mb-6 text-[#5a8c4a]">{t('chooseTowerUpgrade')}</h2>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          {choices.map((c, i) => (
            <button key={i} onClick={() => onPick(c)} className="p-5 rounded-xl bg-[#e8dcc0] border border-[#5a8c4a]/30 hover:border-[#5a8c4a]/60 hover:scale-105 transition-all text-left">
              <div className="font-bold text-lg mb-2 text-[#5a8c4a]">{c.name[lang]}</div>
              <div className="text-sm text-[#5a4a32]">{c.desc[lang]}</div>
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}

// ===== Artifact Modal =====
function ArtifactModal({ lang, t, choices, onPick }: {
  lang: Lang; t: (k: TranslationKey) => string; choices: ArtifactId[]; onPick: (id: ArtifactId) => void;
}) {
  return (
    <div className="absolute inset-0 bg-black/70 flex items-center justify-center z-50">
      <div className="max-w-2xl w-full px-6">
        <h2 className="text-2xl font-bold text-center mb-6 text-[#d4943d]">{t('chooseArtifact')}</h2>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          {choices.map((id) => {
            const a = ARTIFACT_MAP[id];
            return (
              <button key={id} onClick={() => onPick(id)} className="p-5 rounded-xl bg-[#e8dcc0] from-[#d4943d]/15 to-[#c4a060]/5 border border-[#d4943d]/30 hover:border-[#d4943d]/60 hover:scale-105 transition-all text-left">
                <div className="font-bold text-lg mb-2 text-[#d4943d]">{a.name[lang]}</div>
                <div className="text-sm text-[#5a4a32]">{a.desc[lang]}</div>
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
}

// ===== Chest Modal =====
function ChestModal({ lang, t, st, onPick }: {
  lang: Lang; t: (k: TranslationKey) => string; st: GameState; onPick: (r: 'artifact') => void;
}) {
  return (
    <div className="absolute inset-0 bg-black/70 flex items-center justify-center z-50">
      <div className="max-w-lg w-full px-6">
        <h2 className="text-2xl font-bold text-center mb-2 text-[#d4943d]">{t('chestFound')}</h2>
        <p className="text-center text-[#8a7a5a] mb-6">{t('chooseChestReward')}</p>
        <div className="grid grid-cols-1 gap-4">
          <button onClick={() => onPick('artifact')} className="p-5 rounded-xl bg-[#e8dcc0] border border-[#8a5a8a]/30 hover:border-[#8a5a8a]/60 hover:scale-105 transition-all text-left flex items-center gap-4">
            <span className="text-3xl">✦</span>
            <div>
              <div className="font-bold text-lg text-[#8a5a8a]">{t('chestArtifact')}</div>
            </div>
          </button>
        </div>
      </div>
    </div>
  );
}

// ===== Shop =====
function ShopScreen({ lang, t, shop, setShop, onBack }: {
  lang: Lang; t: (k: TranslationKey) => string; shop: ShopState; setShop: (s: ShopState) => void; onBack: () => void;
}) {
  const buy = (id: string) => {
    const def = SHOP_UPGRADES.find(u => u.id === id)!;
    const cur = shop.upgrades[id] || 0;
    if (cur >= def.maxLevel) return;
    const cost = shopCost(def, cur);
    if (shop.gold < cost) return;
    const newShop: ShopState = {
      gold: shop.gold - cost,
      upgrades: { ...shop.upgrades, [id]: cur + 1 },
    };
    saveShop(newShop);
    saveGold(newShop.gold);
    setShop(newShop);
  };

  return (
    <div className="w-full max-w-2xl mx-auto px-6 py-8 overflow-y-auto max-h-screen">
      <div className="flex items-center justify-between mb-6">
        <button onClick={onBack} className="flex items-center gap-2 text-[#8a7a5a] hover:text-[#3a2e1f] transition"><ArrowLeft size={20} /> {t('back')}</button>
        <div className="flex items-center gap-2 text-[#d4943d] font-bold"><span className="text-2xl">●</span> {shop.gold}</div>
      </div>
      <h2 className="text-2xl font-bold mb-6 text-center">{t('shopTitle')}</h2>
      <div className="grid gap-3">
        {SHOP_UPGRADES.map((def) => {
          const cur = shop.upgrades[def.id] || 0;
          const maxed = cur >= def.maxLevel;
          const cost = shopCost(def, cur);
          const canBuy = !maxed && shop.gold >= cost;
          return (
            <div key={def.id} className="flex items-center gap-4 p-4 rounded-xl bg-[#e8dcc0] border border-[#c4b890]">
              <div className="flex-1">
                <div className="font-medium">{def.name[lang]}</div>
                <div className="text-sm text-[#8a7a5a]">{def.desc[lang](cur)}</div>
                <div className="text-xs text-[#8a7a5a]/50 mt-1">{t('owned')}: {cur}/{def.maxLevel}</div>
              </div>
              <button
                onClick={() => buy(def.id)}
                disabled={!canBuy}
                className={`px-5 py-2.5 rounded-lg font-medium text-sm transition min-w-24 ${maxed ? 'bg-[#e8dcc0] text-[#8a7a5a]/50' : canBuy ? 'bg-[#4a7a8a]/20 border border-[#4a7a8a]/40 text-[#3a2e1f] hover:bg-[#4a7a8a]/30' : 'bg-[#e8dcc0] border border-[#c4b890] text-[#8a7a5a]/50'}`}
              >
                {maxed ? t('max') : `${cost} ●`}
              </button>
            </div>
          );
        })}
      </div>
    </div>
  );
}

// ===== Leaderboard =====
function LeaderboardScreen({ lang, t, onBack }: { lang: Lang; t: (k: TranslationKey) => string; onBack: () => void }) {
  const [entries, setEntries] = useState<LeaderEntry[]>(() => loadLeaderboard());
  return (
    <div className="w-full max-w-2xl mx-auto px-6 py-8 overflow-y-auto max-h-screen">
      <div className="flex items-center justify-between mb-6">
        <button onClick={onBack} className="flex items-center gap-2 text-[#8a7a5a] hover:text-[#3a2e1f] transition"><ArrowLeft size={20} /> {t('back')}</button>
      </div>
      <h2 className="text-2xl font-bold mb-6 text-center">{t('leaderTitle')}</h2>
      {entries.length === 0 ? (
        <p className="text-center text-[#8a7a5a]/70 py-12">{t('noScores')}</p>
      ) : (
        <div className="space-y-2">
          <div className="grid grid-cols-12 gap-2 text-xs text-[#8a7a5a]/70 uppercase px-4">
            <div className="col-span-2">{t('rank')}</div>
            <div className="col-span-6">{t('player')}</div>
            <div className="col-span-2 text-right">{t('wave')}</div>
            <div className="col-span-2 text-right">{t('score')}</div>
          </div>
          {entries.map((e, i) => (
            <div key={i} className={`grid grid-cols-12 gap-2 px-4 py-3 rounded-lg items-center ${i === 0 ? 'bg-[#d4943d]/10 border border-[#d4943d]/30' : 'bg-[#e8dcc0]'}`}>
              <div className="col-span-2 font-bold text-[#4a7a8a]">#{i + 1}</div>
              <div className="col-span-6 truncate">{e.name}</div>
              <div className="col-span-2 text-right text-[#8a7a5a] text-sm">{e.wave}</div>
              <div className="col-span-2 text-right font-mono text-sm">{e.time}s</div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

// ===== Achievements =====
function AchievementsScreen({ lang, t, onBack }: { lang: Lang; t: (k: TranslationKey) => string; onBack: () => void }) {
  const [unlocked, setUnlocked] = useState<string[]>(() => loadAchievements());
  return (
    <div className="w-full max-w-2xl mx-auto px-6 py-8 overflow-y-auto max-h-screen">
      <div className="flex items-center justify-between mb-6">
        <button onClick={onBack} className="flex items-center gap-2 text-[#8a7a5a] hover:text-[#3a2e1f] transition"><ArrowLeft size={20} /> {t('back')}</button>
        <span className="text-sm text-[#8a7a5a]/70">{unlocked.length}/{ACHIEVEMENTS.length}</span>
      </div>
      <h2 className="text-2xl font-bold mb-6 text-center">{t('achievementsTitle')}</h2>
      <div className="grid gap-3">
        {ACHIEVEMENTS.map((ach) => {
          const isUnlocked = unlocked.includes(ach.id);
          return (
            <div key={ach.id} className={`flex items-center gap-4 p-4 rounded-xl border transition ${isUnlocked ? 'bg-[#d4943d]/10 border-[#d4943d]/30' : 'bg-[#e8dcc0] border-[#c4b890] opacity-50'}`}>
              <div className={`w-10 h-10 rounded-full flex items-center justify-center ${isUnlocked ? 'bg-[#d4943d]/20' : 'bg-[#e8dcc0]'}`}>
                <Award size={20} className={isUnlocked ? 'text-[#d4943d]' : 'text-[#8a7a5a]/50'} />
              </div>
              <div className="flex-1">
                <div className={`font-medium ${isUnlocked ? 'text-[#d4943d]' : 'text-[#8a7a5a]'}`}>{ach.name[lang]}</div>
                <div className="text-sm text-[#8a7a5a]/70">{ach.desc[lang]}</div>
              </div>
              <span className={`text-xs font-bold ${isUnlocked ? 'text-[#d4943d]' : 'text-[#3a2e1f]/20'}`}>
                {isUnlocked ? t('achievementsUnlocked') : t('achievementsLocked')}
              </span>
            </div>
          );
        })}
      </div>
    </div>
  );
}

// ===== Settings =====
function SettingsScreen({ lang, setLang, t, soundOn, setSoundOn, onBack }: {
  lang: Lang; setLang: (l: Lang) => void; t: (k: TranslationKey) => string; soundOn: boolean; setSoundOn: (v: boolean) => void; onBack: () => void;
}) {
  return (
    <div className="w-full max-w-md mx-auto px-6 py-8">
      <div className="flex items-center justify-between mb-6">
        <button onClick={onBack} className="flex items-center gap-2 text-[#8a7a5a] hover:text-[#3a2e1f] transition"><ArrowLeft size={20} /> {t('back')}</button>
      </div>
      <h2 className="text-2xl font-bold mb-6 text-center">{t('settingsTitle')}</h2>
      <div className="space-y-6">
        <div>
          <label className="text-xs text-[#8a7a5a] uppercase tracking-wider mb-2 block">{t('language')}</label>
          <div className="flex gap-2">
            <button onClick={() => setLang('ru')} className={`flex-1 py-3 rounded-xl border transition ${lang === 'ru' ? 'bg-[#4a7a8a]/20 border-[#4a7a8a]/40 text-[#3a2e1f]' : 'bg-[#e8dcc0] border-[#c4b890] text-[#8a7a5a]'}`}>Русский</button>
            <button onClick={() => setLang('en')} className={`flex-1 py-3 rounded-xl border transition ${lang === 'en' ? 'bg-[#4a7a8a]/20 border-[#4a7a8a]/40 text-[#3a2e1f]' : 'bg-[#e8dcc0] border-[#c4b890] text-[#8a7a5a]'}`}>English</button>
          </div>
        </div>
        <div>
          <label className="text-xs text-[#8a7a5a] uppercase tracking-wider mb-2 block">{t('sound')}</label>
          <div className="flex gap-2">
            <button onClick={() => setSoundOn(true)} className={`flex-1 py-3 rounded-xl border transition ${soundOn ? 'bg-[#4a7a8a]/20 border-[#4a7a8a]/40 text-[#3a2e1f]' : 'bg-[#e8dcc0] border-[#c4b890] text-[#8a7a5a]'}`}>{t('soundOn')}</button>
            <button onClick={() => setSoundOn(false)} className={`flex-1 py-3 rounded-xl border transition ${!soundOn ? 'bg-[#4a7a8a]/20 border-[#4a7a8a]/40 text-[#3a2e1f]' : 'bg-[#e8dcc0] border-[#c4b890] text-[#8a7a5a]'}`}>{t('soundOff')}</button>
          </div>
        </div>
        <div>
          <label className="text-xs text-[#8a7a5a] uppercase tracking-wider mb-2 block">{t('controls')}</label>
          <div className="space-y-2 text-sm text-[#8a7a5a] bg-[#e8dcc0] rounded-xl p-4 border border-[#c4b890]">
            <div>{t('moveControls')}</div>
            <div>{t('placeSphere')}</div>
            <div>{t('activeAbilities')}</div>
            <div>{t('dashAbility')}</div>
          </div>
        </div>
        <button
          onClick={() => { if (confirm(t('resetConfirm'))) { resetAll(); location.reload(); } }}
          className="w-full flex items-center justify-center gap-2 py-3 rounded-xl bg-[#c4453d]/10 border border-[#c4453d]/30 text-[#c4453d] hover:bg-[#c4453d]/20 transition"
        >
          <RotateCcw size={18} /> {t('reset')}
        </button>
      </div>
    </div>
  );
}
