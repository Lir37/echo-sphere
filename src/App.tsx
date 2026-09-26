import { useEffect, useRef, useState } from 'react';
import { Settings, Store, Trophy, Play, Globe, ArrowLeft, RotateCcw, Award, Volume2, VolumeX, UserRound, BarChart3, Sparkles, Package, Network, X, BookOpen } from 'lucide-react';
import { translations, type Lang, type TranslationKey } from './i18n';
import {
  ABILITIES, ARTIFACTS, ARTIFACT_MAP, SHOP_UPGRADES, shopCost,
  abilityName, artifactName, type AbilityType, type ArtifactId,
  DIFFICULTIES, ACHIEVEMENTS, SPHERE_TYPES, type Difficulty,
} from './gameData';
import {
  createInitialState, update,
  generateUpgradeChoices, applyUpgrade, applyArtifact,
  getMaxSpheres, getMoveSpeed, getSphereRadius, getSphereDamage, getSphereDelay, getSphereDpsEstimate,
  getCritChance, getDodgeChance, getVampirePercent,
  type GameState, type ShopState, type LeaderEntry, type UpgradeChoice,
  MAP_THEMES, type MapTheme,
} from './engine';
import { render } from './renderer';
import { createEcho3DRenderer, type Echo3DRenderer } from './visual3d';
import { analyzeSphereNetwork } from './network';
import { ARTIFACT_META, RARITY_LABELS, artifactRarity, getActiveArtifactSynergies, getArtifactSynergiesAfterPick, ARTIFACT_SYNERGIES, getArtifactSetProgress, getArtifactProtocolStates } from './artifactSystem';
import { resolveSpaceCollisions } from './spaceCollision';
import {
  loadShop, saveShop, loadLeaderboard, addLeaderEntry, loadLang, saveLang,
  loadName, saveName, resetAll, saveGold, loadGold,
  loadAchievements, unlockAchievement, loadDifficulty, saveDifficulty,
  loadSound, saveSound, loadHandedness, saveHandedness, loadCharacterId, type Handedness,
} from './persistence';
import { playSound, setAudioEnabled } from './audio';
import MobileControls from './MobileControls';
import CharacterSelect from './CharacterSelect';
import { CHARACTER_DEFS } from './characters';
import KnowledgeBase, { syncKnowledgeFromRun } from './KnowledgeBase';
import {
  ABILITY_PROGRESSION, SPHERE_PROGRESSION, getAbilityDisplayName, getAbilityDisplayDesc,
  getAbilityEvolutionChoice, sphereLevel, sphereModifiers, getActiveSphereAbilitySynergies, SPHERE_ABILITY_SYNERGIES,
} from './sphereProgression';

type Screen = 'menu' | 'game' | 'shop' | 'leaderboard' | 'settings' | 'achievements' | 'characters' | 'knowledge';

// Canvas/2.5D is the active battlefield renderer.
// The authored 3D/GLB path is preserved for the later controlled 3D re-evaluation phase.
const ENABLE_3D_RENDERER = false;

export default function App() {
  const [lang, setLang] = useState<Lang>(() => loadLang());
  const [screen, setScreen] = useState<Screen>('menu');
  const [shop, setShop] = useState<ShopState>(() => loadShop());
  const [gold, setGold] = useState<number>(() => loadGold());
  const [difficulty, setDifficulty] = useState<Difficulty>(() => loadDifficulty() as Difficulty);
  const [soundOn, setSoundOn] = useState<boolean>(() => loadSound());
  const [handedness, setHandedness] = useState<Handedness>(() => loadHandedness());
  const [mapTheme, setMapTheme] = useState<MapTheme>(() => (localStorage.getItem('echosphere_map') || 'parchment') as MapTheme);

  const t = (k: TranslationKey) => translations[lang][k];

  useEffect(() => { saveLang(lang); }, [lang]);
  useEffect(() => { saveDifficulty(difficulty); }, [difficulty]);
  useEffect(() => { saveHandedness(handedness); }, [handedness]);
  useEffect(() => {
    saveSound(soundOn);
    setAudioEnabled(soundOn);
  }, [soundOn]);

  return (
    <div className="es-app min-h-screen w-full text-[#dcecff] overflow-hidden flex items-center justify-center">
      {screen === 'menu' && <Menu lang={lang} setLang={setLang} t={t} difficulty={difficulty} setDifficulty={setDifficulty} soundOn={soundOn} setSoundOn={setSoundOn} mapTheme={mapTheme} setMapTheme={setMapTheme} onPlay={(mt) => { setMapTheme(mt); setScreen('game'); }} onShop={() => { setShop(loadShop()); setGold(loadGold()); setScreen('shop'); }} onCharacters={() => { setGold(loadGold()); setScreen('characters'); }} onLeader={() => setScreen('leaderboard')} onSettings={() => setScreen('settings')} onAchievements={() => setScreen('achievements')} onKnowledge={() => setScreen('knowledge')} />}
      {screen === 'game' && <GameScreen lang={lang} t={t} shop={shop} difficulty={difficulty} mapTheme={mapTheme} handedness={handedness} onExit={() => { setShop(loadShop()); setGold(loadGold()); setScreen('menu'); }} />}
      {screen === 'shop' && <ShopScreen lang={lang} t={t} shop={shop} setShop={setShop} onBack={() => { setGold(loadGold()); setScreen('menu'); }} />}
      {screen === 'characters' && <CharacterSelect lang={lang} gold={gold} onGoldChange={(nextGold) => { setGold(nextGold); setShop(loadShop()); }} onBack={() => { setGold(loadGold()); setShop(loadShop()); setScreen('menu'); }} />}
      {screen === 'leaderboard' && <LeaderboardScreen lang={lang} t={t} onBack={() => setScreen('menu')} />}
      {screen === 'settings' && <SettingsScreen lang={lang} setLang={setLang} t={t} soundOn={soundOn} setSoundOn={setSoundOn} handedness={handedness} setHandedness={setHandedness} onBack={() => setScreen('menu')} />}
      {screen === 'achievements' && <AchievementsScreen lang={lang} t={t} onBack={() => setScreen('menu')} />}
      {screen === 'knowledge' && <KnowledgeBase lang={lang} onBack={() => setScreen('menu')} />}
    </div>
  );
}

// ===== Menu =====
function Menu({ lang, setLang, t, difficulty, setDifficulty, soundOn, setSoundOn, mapTheme, setMapTheme, onPlay, onShop, onCharacters, onLeader, onSettings, onAchievements, onKnowledge }: {
  lang: Lang; setLang: (l: Lang) => void; t: (k: TranslationKey) => string;
  difficulty: Difficulty; setDifficulty: (d: Difficulty) => void;
  soundOn: boolean; setSoundOn: (v: boolean) => void;
  mapTheme: MapTheme; setMapTheme: (m: MapTheme) => void;
  onPlay: (mapTheme: MapTheme) => void; onShop: () => void; onCharacters: () => void; onLeader: () => void; onSettings: () => void; onAchievements: () => void; onKnowledge: () => void;
}) {
  const [name, setName] = useState(() => loadName());
  const selectedCharacter = CHARACTER_DEFS[loadCharacterId()];
  useEffect(() => { saveName(name); }, [name]);
  useEffect(() => { localStorage.setItem('echosphere_map', mapTheme); }, [mapTheme]);

  return (
    <div className="es-main-menu">
      <header className="es-main-topbar">
        <div>
          <div className="es-main-logo">ECHO SPHERE</div>
          <div className="es-main-sub">RESONANCE SURVIVAL SYSTEM</div>
        </div>
        <div className="es-main-tools">
          <button onClick={() => setSoundOn(!soundOn)} className="es-tech-button" aria-label={soundOn ? 'Mute' : 'Sound'}>
            {soundOn ? <Volume2 size={15} /> : <VolumeX size={15} />}
          </button>
          <button onClick={() => setLang(lang === 'ru' ? 'en' : 'ru')} className="es-tech-button">
            <Globe size={14} /> {lang.toUpperCase()}
          </button>
        </div>
      </header>

      <main className="es-main-stage">
        <section className="es-main-core">
          <div className="es-core-caption">ECHO CORE // {lang === 'ru' ? 'ГОТОВ' : 'READY'}</div>
          <div className="es-main-core-art" aria-hidden="true">
            <div className="es-core-orbit orbit-a" />
            <div className="es-core-orbit orbit-b" />
            <div className="es-core-orbit orbit-c" />
            <div className="es-core-ring ring-a" />
            <div className="es-core-ring ring-b" />
            <div className="es-core-body" />
            <div className="es-core-highlight" />
          </div>
          <div className="es-main-core-title">ENTER THE ECHO</div>
          <div className="es-main-core-copy">
            {lang === 'ru' ? 'Размещай сферы. Строй резонанс. Переживи волну.' : 'Deploy spheres. Build resonance. Survive the wave.'}
          </div>

          <div className="es-main-loadout">
            <button onClick={onCharacters} className="es-loadout-character">
              <span className="es-loadout-avatar" style={{ ['--character-color' as string]: selectedCharacter.color }}>✦</span>
              <span>
                <b>{selectedCharacter.name[lang]}</b>
                <small>{selectedCharacter.role[lang]}</small>
              </span>
              <UserRound size={14} />
            </button>

            <div className="es-loadout-divider" />

            <div className="es-loadout-setting">
              <span>{t('difficulty')}</span>
              <div className="es-loadout-options">
                {DIFFICULTIES.map((d) => (
                  <button key={d.id} onClick={() => setDifficulty(d.id)} className={difficulty === d.id ? 'is-active' : ''}>{d.name[lang]}</button>
                ))}
              </div>
            </div>

            <div className="es-loadout-setting">
              <span>{t('chooseMap')}</span>
              <div className="es-loadout-options">
                {MAP_THEMES.map((m) => (
                  <button key={m.id} onClick={() => setMapTheme(m.id)} className={mapTheme === m.id ? 'is-active' : ''}>{m.name[lang]}</button>
                ))}
              </div>
            </div>
          </div>

          <div className="es-main-name">
            <span>ID</span>
            <input value={name} onChange={(e) => setName(e.target.value.slice(0, 16))} placeholder={t('namePlaceholder')} />
          </div>

          <button onClick={() => onPlay(mapTheme)} className="es-main-play">
            <Play size={18} fill="currentColor" />
            <span>{t('play')}</span>
            <small>START RUN</small>
          </button>
        </section>

      </main>

      <nav className="es-main-nav">
        <button onClick={onCharacters}><UserRound size={16} /><span>{lang === 'ru' ? 'Персонажи' : 'Characters'}</span></button>
        <button onClick={onShop}><Store size={16} /><span>{t('shop')}</span></button>
        <button onClick={onLeader}><Trophy size={16} /><span>{t('leaderboard')}</span></button>
        <button onClick={onAchievements}><Award size={16} /><span>{t('achievements')}</span></button>
        <button onClick={onKnowledge} className="es-knowledge-nav"><BookOpen size={16} /><span>{lang === 'ru' ? 'Архив Эха' : 'Echo Archive'}</span></button>
        <button onClick={onSettings}><Settings size={16} /><span>{t('settings')}</span></button>
      </nav>
    </div>
  );
}


function GameScreen({ lang, t, shop, difficulty, mapTheme, handedness, onExit }: {
  lang: Lang; t: (k: TranslationKey) => string; shop: ShopState; difficulty: Difficulty; mapTheme: MapTheme; handedness: Handedness; onExit: () => void;
}) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const renderer3dRef = useRef<Echo3DRenderer | null>(null);
  const stateRef = useRef<GameState | null>(null);
  const rafRef = useRef<number>(0);
  const knowledgeTickRef = useRef(0);
  const lastTimeRef = useRef<number>(0);
  const [, forceRender] = useState(0);
  const [gameOverData, setGameOverData] = useState<{ time: number; wave: number; gold: number; rank: number; isNewRecord: boolean } | null>(null);
  const [paused, setPaused] = useState(false);
  const [pauseTab, setPauseTab] = useState<PauseTab>('stats');

  useEffect(() => {
    const name = loadName() || translations[lang].namePlaceholder;
    const s = createInitialState(shop, name, difficulty, mapTheme);
    stateRef.current = s;

    const canvas = canvasRef.current;
    if (canvas && ENABLE_3D_RENDERER) renderer3dRef.current = createEcho3DRenderer(canvas);

    lastTimeRef.current = performance.now();

    const loop = (now: number) => {
      const dt = Math.min(0.05, (now - lastTimeRef.current) / 1000);
      lastTimeRef.current = now;
      const st = stateRef.current;
      if (st) {
        update(st, dt);
        resolveSpaceCollisions(st, dt);
        knowledgeTickRef.current += dt;
        if (knowledgeTickRef.current >= 0.35) {
          knowledgeTickRef.current = 0;
          syncKnowledgeFromRun(st);
        }

        if (st.gameOver && !gameOverData) {
          const time = Math.floor(st.time);
          const diff = DIFFICULTIES.find(d => d.id === st.difficulty)!;
          const gold = Math.floor((time * 0.15 + st.bossDefeated * 25 + st.stats.enemiesKilled * 0.05) * diff.goldMult);
          saveGold(loadGold() + gold);
          const entry: LeaderEntry = { name: name || 'Player', time, wave: st.wave, date: Date.now() };
          const { rank, isNewRecord } = addLeaderEntry(entry);
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

        if (ENABLE_3D_RENDERER && renderer3dRef.current) {
          renderer3dRef.current.render(st);
        } else if (canvas) {
          const ctx = canvas.getContext('2d');
          if (ctx) render(ctx, st, canvas.width, canvas.height);
        }

        forceRender(v => v + 1);
      }
      rafRef.current = requestAnimationFrame(loop);
    };

    rafRef.current = requestAnimationFrame(loop);
    return () => {
      cancelAnimationFrame(rafRef.current);
      renderer3dRef.current?.dispose();
      renderer3dRef.current = null;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const st = stateRef.current;

  return (
    <div className="es-game-screen relative w-full h-screen flex items-center justify-center" style={{ touchAction: 'none' }}>
      <canvas
        ref={canvasRef}
        width={Math.min(window.innerWidth, 1280)}
        height={Math.min(window.innerHeight, 800)}
        className="max-w-full max-h-full select-none"
        style={{ touchAction: 'none', userSelect: 'none', WebkitUserSelect: 'none' }}
      />

      {st && !gameOverData && (
        <>
          <Hud lang={lang} t={t} st={st} />
          <MobileControls
            lang={lang}
            t={t}
            stateRef={stateRef}
            canvasRef={canvasRef}
            handedness={handedness}
            onPause={() => {
              if (st.pendingUpgrade || st.pendingArtifact) return;
              const next = !st.paused;
              st.paused = next;
              setPaused(next);
            }}
          />
          {st.player.combo >= 5 && (
            <div className="absolute top-16 left-1/2 -translate-x-1/2 pointer-events-none text-center z-10">
              <div className="text-2xl font-bold" style={{ color: st.player.combo >= 50 ? '#ffb84d' : st.player.combo >= 25 ? '#ff6b6b' : '#ff4d5d' }}>
                {t('combo')} x{st.player.combo}
              </div>
              <div className="text-sm font-bold text-[#b6c9de]">
                {t('comboMultiplier').replace('{mult}', String(st.player.comboMult))}
              </div>
              <div className="w-24 h-1 bg-[#243b55] rounded-full mt-1 overflow-hidden">
                <div className="h-full bg-[#ffb84d] transition-all" style={{ width: `${(st.player.comboTimer / 3) * 100}%` }} />
              </div>
            </div>
          )}
          {st.pendingUpgrade && <UpgradeModal lang={lang} t={t} st={st} onPick={(c) => { applyUpgrade(st, c); }} />}
          {st.pendingArtifact && <ArtifactModal lang={lang} t={t} st={st} choices={st.pendingArtifact} onPick={(id) => { applyArtifact(st, id); st.pendingArtifact = null; }} />}
          {paused && !st.pendingUpgrade && !st.pendingArtifact && !st.pendingStella && (
            <PausePlanner
              lang={lang}
              t={t}
              st={st}
              tab={pauseTab}
              setTab={setPauseTab}
              onResume={() => { setPaused(false); st.paused = false; }}
              onExit={onExit}
            />
          )}
        </>
      )}

      {gameOverData && (
        <div className="absolute inset-0 bg-black/80 flex items-center justify-center z-50">
          <div className="text-center max-w-sm px-6">
            <h2 className="text-4xl font-bold text-[#ff4d5d] mb-2">{t('gameOver')}</h2>
            {gameOverData.isNewRecord && <p className="text-2xl font-bold text-[#ffb84d] mb-4 animate-pulse">{t('newRecord')}</p>}
            <div className="bg-[#0d1726] border border-[#243b55] rounded-xl p-6 mb-6 space-y-2 text-left">
              <Row label={t('survived')} value={`${gameOverData.time} ${t('seconds')}`} />
              <Row label={t('wave')} value={`${gameOverData.wave}`} />
              <Row label={t('goldEarned')} value={`${gameOverData.gold}`} />
              {gameOverData.rank > 0 && gameOverData.rank <= 10 && <Row label={t('rank')} value={`#${gameOverData.rank}`} />}
            </div>
            <button onClick={onExit} className="px-8 py-3 rounded-xl bg-[#39d8ff]/20 border border-[#39d8ff]/40 text-[#dcecff] hover:bg-[#39d8ff]/30 transition w-full">{t('return')}</button>
          </div>
        </div>
      )}
    </div>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  return <div className="flex justify-between"><span className="text-[#7f9bb8]">{label}</span><span className="font-medium">{value}</span></div>;
}

type PauseTab = 'stats' | 'skills' | 'artifacts' | 'synergies';

function PausePlanner({ lang, t, st, tab, setTab, onResume, onExit }: {
  lang: Lang;
  t: (k: TranslationKey) => string;
  st: GameState;
  tab: PauseTab;
  setTab: (tab: PauseTab) => void;
  onResume: () => void;
  onExit: () => void;
}) {
  const character = CHARACTER_DEFS[st.player.characterId];
  const activeAbilities = (Object.keys(ABILITIES) as AbilityType[]).filter((id) => ABILITIES[id].category === 'active');
  const acquiredAbilities = (Object.keys(ABILITIES) as AbilityType[]).filter((id) => (st.player.abilities[id] || 0) > 0);

  const tabDefs: Array<{ id: PauseTab; label: string; icon: React.ReactNode }> = [
    { id: 'stats', label: lang === 'ru' ? 'Статы' : 'Stats', icon: <BarChart3 size={15} /> },
    { id: 'skills', label: lang === 'ru' ? 'Скиллы' : 'Skills', icon: <Sparkles size={15} /> },
    { id: 'artifacts', label: lang === 'ru' ? 'Артефакты' : 'Artifacts', icon: <Package size={15} /> },
    { id: 'synergies', label: lang === 'ru' ? 'Синергии' : 'Synergies', icon: <Network size={15} /> },
  ];

  return (
    <div className="es-pause-overlay absolute inset-0 bg-black/75 backdrop-blur-sm flex items-center justify-center z-50 p-3">
      <div className="es-modal-shell w-full max-w-3xl max-h-[92vh] rounded-2xl overflow-hidden flex flex-col">
        <div className="px-4 pt-4 pb-3 border-b border-[#243b55] shrink-0">
          <div className="flex items-center justify-between gap-3">
            <div className="min-w-0">
              <div className="text-[10px] uppercase tracking-[0.2em] text-[#7f9bb8]">{lang === 'ru' ? 'Билд забега' : 'Run Build'}</div>
              <div className="text-xl font-bold truncate">{t('pauseTitle')} · {character.name[lang]}</div>
              <div className="text-xs text-[#7f9bb8] mt-0.5">{character.role[lang]} · {lang === 'ru' ? 'уровень' : 'level'} {st.player.level} · Wave {st.wave}</div>
            </div>
            <button onClick={onResume} className="w-9 h-9 rounded-full bg-[#0d1726] border border-[#243b55] flex items-center justify-center" aria-label={t('resume')}><X size={18} /></button>
          </div>
          <div className="grid grid-cols-4 gap-1.5 mt-3">
            {tabDefs.map((item) => (
              <button key={item.id} onClick={() => setTab(item.id)} className={`flex items-center justify-center gap-1 px-2 py-2 rounded-lg border text-[10px] font-bold transition ${tab === item.id ? 'bg-[#39d8ff]/15 border-[#39d8ff]/50 text-[#dcecff]' : 'bg-[#0d1726] border-[#243b55] text-[#7f9bb8]'}`}>
                {item.icon}{item.label}
              </button>
            ))}
          </div>
        </div>

        <div className="overflow-y-auto px-4 py-4 min-h-0">
          {tab === 'stats' && (
            <div className="space-y-4">
              <section>
                <SectionTitle>{lang === 'ru' ? 'Персонаж' : 'Character'}</SectionTitle>
                <div className="grid grid-cols-2 sm:grid-cols-4 gap-2">
                  <StatBox label="HP" value={`${Math.ceil(st.player.hp)} / ${Math.ceil(st.player.maxHp)}`} />
                  <StatBox label={lang === 'ru' ? 'Скорость' : 'Speed'} value={`${Math.round(getMoveSpeed(st))}`} />
                  <StatBox label={lang === 'ru' ? 'Крит' : 'Crit'} value={`${Math.round(getCritChance(st) * 100)}%`} />
                  <StatBox label={lang === 'ru' ? 'Уклонение' : 'Dodge'} value={`${Math.round(getDodgeChance(st) * 100)}%`} />
                  <StatBox label={lang === 'ru' ? 'Вампиризм' : 'Lifesteal'} value={`${Math.round(getVampirePercent(st) * 100)}%`} />
                  <StatBox label={lang === 'ru' ? 'Сфер' : 'Spheres'} value={`${st.spheres.length} / ${getMaxSpheres(st)}`} />
                  <StatBox label={lang === 'ru' ? 'Убийства' : 'Kills'} value={`${st.player.kills}`} />
                  <StatBox label={lang === 'ru' ? 'Множитель XP' : 'XP Mult'} value={`${Math.round((getXpPlannerMult(st)) * 100)}%`} />
                </div>
              </section>

              <section>
                <SectionTitle>{lang === 'ru' ? 'Сферы' : 'Spheres'}</SectionTitle>
                <div className="space-y-2">
                  {(Object.keys(SPHERE_TYPES) as Array<keyof typeof SPHERE_TYPES>).map((type) => {
                    const def = SPHERE_TYPES[type];
                    const lvl = sphereLevel(st, type);
                    const placed = st.spheres.filter((sphere) => sphere.type === type && sphere.alive);
                    const live = placed[0];
                    const mods = sphereModifiers(st, type, live);
                    const branch = st.player.sphereBranches[type];
                    const finalChoice = branch ? SPHERE_PROGRESSION[type].evolution4Choices.find((x) => x.id === branch)?.final[
                      Number((st.player.evolutions.find((x) => x.startsWith('sphere:' + type + ':7:')) || '').split(':').pop())
                    ] : null;
                    return (
                      <div key={type} className="rounded-xl bg-[#0d1726] border border-[#243b55] p-3">
                        <div className="flex items-center justify-between gap-2">
                          <div className="flex items-center gap-2 min-w-0">
                            <span className="w-3 h-3 rounded-full border-2" style={{ borderColor: def.color }} />
                            <span className="font-bold truncate">{def.name[lang]}</span>
                            <span className="text-[10px] text-[#7f9bb8]">{lang === 'ru' ? 'ур.' : 'lvl'} {lvl}</span>
                          </div>
                          <span className={`text-[10px] font-bold px-2 py-1 rounded-md ${placed.length ? 'bg-[#5a8c4a]/15 text-[#5a8c4a]' : 'bg-black/5 text-[#7f9bb8]'}`}>{placed.length ? (lang === 'ru' ? `на поле ×${placed.length}` : `field ×${placed.length}`) : (lang === 'ru' ? 'не установлена' : 'not placed')}</span>
                        </div>
                        {live ? (
                          <div className="grid grid-cols-3 gap-2 mt-2 text-[10px]">
                            <MiniStat label={lang === 'ru' ? 'урон' : 'damage'} value={live.type === 'aura' ? `${Math.round(getSphereDamage(st, live))}/имп.` : `${Math.round(getSphereDamage(st, live))}`} />
                            <MiniStat label={lang === 'ru' ? 'интервал' : 'delay'} value={`${getSphereDelay(st, live).toFixed(2)}с`} />
                            <MiniStat label={lang === 'ru' ? 'дальность' : 'range'} value={`${Math.round(getSphereRadius(st, live))}`} />
                            <MiniStat label={lang === 'ru' ? 'DPS' : 'DPS'} value={`${getSphereDpsEstimate(st, live).toFixed(1)}`} />
                          </div>
                        ) : (
                          <div className="text-[10px] text-[#7f9bb8] mt-2">{lang === 'ru' ? def.desc[lang] : def.desc[lang]}</div>
                        )}
                        {branch && (
                          <div className="mt-2 text-[10px]">
                            <span className="text-[#8064a8] font-bold">{lang === 'ru' ? 'Ветка:' : 'Branch:'}</span> {SPHERE_PROGRESSION[type].evolution4Choices.find((x) => x.id === branch)?.name[lang]}
                            {finalChoice && <> <span className="text-[#ff6b6b] font-bold ml-1">{lang === 'ru' ? '→' : '→'}</span> <span className="text-[#ff6b6b] font-bold">{finalChoice.name[lang]}</span></>}
                          </div>
                        )}
                      </div>
                    );
                  })}
                </div>
              </section>
            </div>
          )}

          {tab === 'skills' && (
            <div className="space-y-3">
              <SectionTitle>{lang === 'ru' ? 'Активные способности' : 'Active abilities'}</SectionTitle>
              {activeAbilities.map((id) => {
                const level = st.player.abilities[id] || 0;
                const def = ABILITIES[id];
                const progression = ABILITY_PROGRESSION[id];
                const branch = getAbilityEvolutionChoice(st, id, 4);
                const final = getAbilityEvolutionChoice(st, id, 7);
                return (
                  <div key={id} className={`rounded-xl bg-[#0d1726] border p-3 ${level ? 'border-[#39d8ff]/30' : 'border-[#243b55] opacity-75'}`}>
                    <div className="flex items-center justify-between gap-2">
                      <div className="font-bold text-sm">{level ? getAbilityDisplayName(st, id, lang) : def.name[lang]}</div>
                      <span className="text-[10px] font-bold text-[#39d8ff]">{level}/7</span>
                    </div>
                    <div className="text-[10px] text-[#7f9bb8] mt-1">{level ? getAbilityDisplayDesc(st, id, lang) : def.desc[lang](1)}</div>
                    {progression && (
                      <div className="mt-2 space-y-1.5 text-[10px]">
                        <div><span className="font-bold text-[#8064a8]">{lang === 'ru' ? 'IV:' : 'IV:'}</span> {branch ? branch.name[lang] : progression.evolution4.map((x) => x.name[lang]).join(' · ')}</div>
                        <div><span className="font-bold text-[#ff6b6b]">{lang === 'ru' ? 'VII:' : 'VII:'}</span> {final ? final.name[lang] : progression.evolution7.map((x) => x.name[lang]).join(' · ')}</div>
                      </div>
                    )}
                  </div>
                );
              })}

              {acquiredAbilities.some((id) => ABILITIES[id].category === 'passive') && (
                <section className="pt-2">
                  <SectionTitle>{lang === 'ru' ? 'Пассивные' : 'Passive'}</SectionTitle>
                  <div className="space-y-2">
                    {acquiredAbilities.filter((id) => ABILITIES[id].category === 'passive').map((id) => (
                      <div key={id} className="rounded-lg bg-[#0d1726] border border-[#243b55] p-3 text-xs">
                        <div className="flex justify-between font-bold"><span>{abilityName(id, lang)}</span><span>{st.player.abilities[id]}/7</span></div>
                        <div className="text-[10px] text-[#7f9bb8] mt-1">{ABILITIES[id].desc[lang](st.player.abilities[id] || 1)}</div>
                      </div>
                    ))}
                  </div>
                </section>
              )}
            </div>
          )}

          {tab === 'artifacts' && (
            <div className="space-y-3">
              <SectionTitle>{lang === 'ru' ? `Собрано: ${st.player.artifacts.length}` : `Owned: ${st.player.artifacts.length}`}</SectionTitle>
              {st.player.artifacts.length === 0 ? (
                <div className="rounded-xl bg-[#0d1726] border border-[#243b55] p-5 text-center text-sm text-[#7f9bb8]">{lang === 'ru' ? 'Артефактов пока нет.' : 'No artifacts yet.'}</div>
              ) : st.player.artifacts.map((id) => {
                const rarity = artifactRarity(id);
                return (
                  <div key={id} className="rounded-xl bg-[#0d1726] border-2 p-3" style={{ borderColor: rarity === 'legendary' ? '#ffb84d' : rarity === 'special' ? '#ff6b6b' : rarity === 'epic' ? '#8064a8' : rarity === 'rare' ? '#39d8ff' : '#243b55' }}>
                    <div className="flex items-center justify-between gap-2">
                      <span className="font-bold text-sm">{ARTIFACT_MAP[id].name[lang]}</span>
                      <span className="text-[9px] uppercase tracking-wider text-[#7f9bb8]">{RARITY_LABELS[rarity][lang]}</span>
                    </div>
                    <div className="text-[10px] text-[#b6c9de] mt-1">{ARTIFACT_MAP[id].desc[lang]}</div>
                  </div>
                );
              })}
            </div>
          )}

          {tab === 'synergies' && (
            <div className="space-y-4">
              <section>
                <SectionTitle>{lang === 'ru' ? 'Персонаж + сфера + способность' : 'Character + sphere + ability'}</SectionTitle>
                <div className="space-y-2">
                  {SPHERE_ABILITY_SYNERGIES.filter((link) => link.character === st.player.characterId).map((link) => {
                    const sphereOk = sphereLevel(st, link.sphere) >= 7;
                    const abilityOk = (st.player.abilities[link.ability] || 0) >= 7;
                    const active = sphereOk && abilityOk;
                    return (
                      <div key={link.name.ru} className={`rounded-xl border p-3 ${active ? 'bg-[#8064a8]/10 border-[#8064a8]/40' : 'bg-[#0d1726] border-[#243b55]'}`}>
                        <div className="flex items-center justify-between gap-2">
                          <span className="font-bold text-sm">{link.name[lang]}</span>
                          <span className={`text-[9px] uppercase font-bold ${active ? 'text-[#8064a8]' : 'text-[#7f9bb8]'}`}>{active ? (lang === 'ru' ? 'АКТИВНА' : 'ACTIVE') : (lang === 'ru' ? 'ЦЕЛЬ' : 'TARGET')}</span>
                        </div>
                        <div className="text-[10px] text-[#b6c9de] mt-1">{link.desc[lang]}</div>
                        <div className="text-[10px] text-[#7f9bb8] mt-1">
                          {SPHERE_TYPES[link.sphere].name[lang]} VII {sphereOk ? '✓' : '•'} · {ABILITIES[link.ability].name[lang]} VII {abilityOk ? '✓' : '•'}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </section>
              <section>
                <SectionTitle>{lang === 'ru' ? 'Сеты артефактов' : 'Artifact Sets'}</SectionTitle>
                <div className="space-y-2">
                  {getArtifactSetProgress(st).map((set) => (
                    <div key={set.id} className={`rounded-xl border p-3 transition-all ${set.complete ? 'bg-[#ffb84d]/10 border-[#ffb84d]/40 animate-pulse' : 'bg-[#0d1726] border-[#243b55]'}`}>
                      <div className="flex items-center justify-between gap-2">
                        <span className="font-bold text-sm">{set.complete ? set.name[lang] : '???'}</span>
                        <span className="text-[9px] font-bold text-[#7f9bb8]">{set.activeSynergies}/{set.totalSynergies}</span>
                      </div>
                      <div className="text-[10px] text-[#b6c9de] mt-1">{set.complete ? set.desc[lang] : (lang === 'ru' ? 'Сет ещё не открыт.' : 'Set not discovered yet.')}</div>
                    </div>
                  ))}
                </div>
              </section>
              <section>
                <SectionTitle>{lang === 'ru' ? 'Протоколы' : 'Protocols'}</SectionTitle>
                <div className="space-y-2">
                  {getArtifactProtocolStates(st).map((protocol) => (
                    <div key={protocol.id} className={`rounded-xl border p-3 ${protocol.active ? 'bg-[#63e6ff]/10 border-[#63e6ff]/40' : 'bg-[#0d1726] border-[#243b55]'}`}>
                      <div className="flex items-center justify-between gap-2">
                        <span className="font-bold text-sm">{protocol.discovered ? protocol.name[lang] : '???'}</span>
                        <span className={`text-[9px] font-bold ${protocol.active ? 'text-[#63e6ff]' : 'text-[#7f9bb8]'}`}>
                          {protocol.active ? (lang === 'ru' ? 'АКТИВЕН' : 'ACTIVE') : protocol.discovered ? (lang === 'ru' ? 'ОТКРЫТ' : 'DISCOVERED') : '???'}
                        </span>
                      </div>
                      <div className="text-[10px] text-[#b6c9de] mt-1">{protocol.discovered ? protocol.desc[lang] : (lang === 'ru' ? 'Неизвестный протокол.' : 'Unknown protocol.')}</div>
                    </div>
                  ))}
                </div>
              </section>
              <section>
                <SectionTitle>{lang === 'ru' ? 'Синергии артефактов' : 'Artifact synergies'}</SectionTitle>
                <div className="space-y-2">
                  {ARTIFACT_SYNERGIES.map((synergy) => {
                    const owned = synergy.requires.filter((id) => st.player.artifacts.includes(id)).length;
                    const active = owned === synergy.requires.length;
                    return (
                      <div key={synergy.id} className={`rounded-xl border p-3 ${active ? 'bg-[#8064a8]/10 border-[#8064a8]/40' : 'bg-[#0d1726] border-[#243b55]'}`}>
                        <div className="flex items-center justify-between gap-2">
                          <span className="font-bold text-sm">{synergy.name[lang]}</span>
                          <span className={`text-[9px] font-bold ${active ? 'text-[#8064a8]' : 'text-[#7f9bb8]'}`}>{owned}/{synergy.requires.length}</span>
                        </div>
                        <div className="text-[10px] text-[#b6c9de] mt-1">{synergy.desc[lang]}</div>
                        <div className="flex flex-wrap gap-1 mt-2">
                          {synergy.requires.map((id) => (
                            <span key={id} className={`px-1.5 py-1 rounded-md text-[9px] border ${st.player.artifacts.includes(id) ? 'bg-[#5a8c4a]/10 border-[#5a8c4a]/25 text-[#5a8c4a]' : 'bg-black/5 border-[#243b55] text-[#7f9bb8]'}`}>
                              {ARTIFACT_MAP[id].name[lang]}
                            </span>
                          ))}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </section>
            </div>
          )}
        </div>

        <div className="shrink-0 px-4 py-3 border-t border-[#243b55] bg-[#06111d]">
          <div className="flex gap-2 justify-center">
            <button onClick={onResume} className="px-5 py-2.5 rounded-xl bg-[#39d8ff] border border-[#3a6a7a] text-white font-bold">{t('resume')}</button>
            <button onClick={onExit} className="px-5 py-2.5 rounded-xl bg-[#0d1726] border border-[#243b55] text-[#b6c9de]">{t('return')}</button>
          </div>
        </div>
      </div>
    </div>
  );
}

function SectionTitle({ children }: { children: React.ReactNode }) {
  return <div className="text-[10px] uppercase tracking-wider font-bold text-[#7f9bb8] mb-2">{children}</div>;
}

function StatBox({ label, value }: { label: string; value: string }) {
  return <div className="rounded-xl bg-[#0d1726] border border-[#243b55] px-3 py-2"><div className="text-[9px] uppercase text-[#7f9bb8]">{label}</div><div className="font-bold text-sm">{value}</div></div>;
}

function MiniStat({ label, value }: { label: string; value: string }) {
  return <div className="rounded-lg bg-black/5 px-2 py-1.5"><div className="text-[9px] text-[#7f9bb8]">{label}</div><div className="font-bold">{value}</div></div>;
}

function getXpPlannerMult(st: GameState): number {
  let mult = 1 + (st.shopUpgrades.xp || 0) * 0.05;
  for (const id of st.player.artifacts) {
    const meta = ARTIFACT_META[id];
    if (meta?.effects.xpGain) mult += meta.effects.xpGain;
  }
  return mult;
}

// ===== HUD =====
function Hud({ lang, t, st }: { lang: Lang; t: (k: TranslationKey) => string; st: GameState }) {
  const xpPct = Math.max(0, Math.min(100, (st.player.xp / st.player.xpToNext) * 100));
  const hpPct = Math.max(0, Math.min(100, (st.player.hp / st.player.maxHp) * 100));
  const resonancePct = Math.max(0, Math.min(100, st.player.resonanceCharge));
  const resonanceEventReady = st.player.resonanceCharge >= 100;
  const mins = Math.floor(st.time / 60);
  const secs = Math.floor(st.time % 60);
  const timer = `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  const activeBoss = st.bossActive;
  const network = analyzeSphereNetwork(st.spheres);
  const networkBadges = [
    network.line ? { label: lang === 'ru' ? 'ЛИНИЯ' : 'LINE', className: 'border-[#63e6ff]/45 text-[#9fefff]' } : null,
    network.triangle ? { label: lang === 'ru' ? 'ТРЕУГОЛЬНИК' : 'TRIANGLE', className: 'border-[#ffb84d]/45 text-[#ffd48f]' } : null,
    network.ring ? { label: lang === 'ru' ? 'КОЛЬЦО' : 'RING', className: 'border-[#55e69a]/45 text-[#9affc8]' } : null,
    network.lattice ? { label: lang === 'ru' ? 'РЕШЁТКА' : 'LATTICE', className: 'border-[#39d8ff]/45 text-[#8eeeff]' } : null,
    network.fractal ? { label: lang === 'ru' ? 'ФРАКТАЛ' : 'FRACTAL', className: 'border-[#ff6b9d]/45 text-[#ffb3ca]' } : null,
    network.cluster ? { label: lang === 'ru' ? 'КЛАСТЕР' : 'CLUSTER', className: 'border-[#b38cff]/45 text-[#d4c0ff]' } : null,
    network.square ? { label: lang === 'ru' ? 'КВАДРАТ' : 'SQUARE', className: 'border-[#69b7ff]/45 text-[#9bcfff]' } : null,
  ].filter((badge): badge is { label: string; className: string } => badge !== null);

  return (
    <>
      <div className="es-hud-panel es-top-left absolute top-3 left-3 z-30 pointer-events-none">
        <div className="flex items-center gap-2">
          <div className="es-hud-avatar">✦</div>
          <div className="min-w-0 flex-1">
            <div className="flex items-center justify-between gap-2">
              <span className="es-hud-title">{t('level')} {st.player.level}</span>
              <span className="es-hud-value">{Math.ceil(st.player.hp)}/{Math.ceil(st.player.maxHp)}</span>
            </div>
            <div className="es-progress mt-1.5"><span style={{ width: `${xpPct}%` }} /></div>
            <div className="es-hp-progress mt-1"><span style={{ width: `${hpPct}%` }} /></div>
            <div className="mt-1.5">
              <div className="flex items-center justify-between text-[7px] uppercase tracking-[0.12em] text-[#7f9bb8]"><span>{lang === "ru" ? "РЕЗОНАНС" : "RESONANCE"}</span><b className={resonanceEventReady ? "text-[#ffb84d]" : "text-[#dcecff]"}>{Math.floor(st.player.resonanceCharge)}/100</b></div>
              <div className="h-1.5 mt-0.5 rounded-full bg-[#13243a] overflow-hidden border border-[#243b55]"><span className="block h-full" style={{ width: resonancePct + "%", background: resonanceEventReady ? "#ffb84d" : "#39d8ff" }} /></div>
            </div>
          </div>
        </div>
        <div className="es-hud-meta-grid mt-2">
          <span>{t('spheres').toUpperCase()} <b>{st.spheres.length}/{getMaxSpheres(st)}</b></span>
          <span>{t('wave').toUpperCase()} <b>{st.wave}</b></span>
        </div>
        <div className="mt-1.5 flex items-center gap-1.5 text-[7px] uppercase tracking-[0.12em] text-[#7f9bb8]">
          <Network size={10} />
          <span>{lang === 'ru' ? 'СЕТЬ' : 'NETWORK'}</span>
          <b className="text-[#dcecff]">{network.links.length}</b>
        </div>
        <div className="mt-1 flex flex-wrap gap-1">
          {networkBadges.length > 0 ? networkBadges.map((badge) => (
            <span key={badge.label} className={`rounded border px-1 py-0.5 text-[7px] font-bold tracking-wide ${badge.className}`}>
              {badge.label}
            </span>
          )) : <span className="text-[7px] text-[#7f9bb8]">{lang === 'ru' ? 'ФОРМАЦИЯ НЕ АКТИВНА' : 'NO FORMATION'}</span>}
        </div>
      </div>

      <div className="es-time-hud absolute top-3 left-1/2 -translate-x-1/2 z-30 pointer-events-none">
        <div className="es-time-hud-line">
          <span className="es-time-hud-dot" />
          <span className="es-time-hud-value">{timer}</span>
          <span className="es-time-hud-dot" />
        </div>
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
      <span className="es-radar-player" />
    </div>
  );
}

function ArtifactModal({ lang, t, st, choices, onPick }: {
  lang: Lang;
  t: (k: TranslationKey) => string;
  st: GameState;
  choices: ArtifactId[];
  onPick: (id: ArtifactId) => void;
}) {
  const rarityClass: Record<string,string> = {
    common: 'border-[#243b55]',
    rare: 'border-[#39d8ff]',
    epic: 'border-[#8064a8]',
    special: 'border-[#ff6b6b]',
    legendary: 'border-[#ffb84d]',
  };
  const rarityText: Record<string,string> = {
    common: 'text-[#7f9bb8]',
    rare: 'text-[#39d8ff]',
    epic: 'text-[#8064a8]',
    special: 'text-[#ff6b6b]',
    legendary: 'text-[#ffb84d]',
  };
  const mechanicText: Record<string,{ru:string;en:string}> = {
    swift: {ru:'Темп',en:'Tempo'}, mirror:{ru:'Контратака',en:'Counter'},
    resonance:{ru:'Связь сфер',en:'Sphere Link'}, lone:{ru:'Одна сфера',en:'Solo Sphere'},
    fivefold:{ru:'Сеть',en:'Network'}, relay:{ru:'Прогрессия',en:'Progression'},
    triangle:{ru:'Геометрия',en:'Geometry'}, overclock:{ru:'Разгон',en:'Overclock'},
    network:{ru:'Сеть',en:'Network'}, singularity:{ru:'Сингулярность',en:'Singularity'},
    zero:{ru:'Архитектура',en:'Architecture'}, unified:{ru:'Единый разум',en:'Unified Mind'},
  };
  const active = getActiveArtifactSynergies(st);
  return (
    <div className="absolute inset-0 bg-black/70 backdrop-blur-sm flex items-center justify-center z-50">
      <div className="max-w-5xl w-full px-6">
        <h2 className="text-2xl font-bold text-center mb-2">{lang === 'ru' ? 'Артефакт' : 'Artifact'}</h2>
        <p className="text-sm text-center text-[#7f9bb8] mb-6">{lang === 'ru' ? 'Артефакты меняют правила взаимодействия сфер и персонажа' : 'Artifacts change the rules of how spheres and the player interact'}</p>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          {choices.map((id) => {
            const meta = ARTIFACT_META[id];
            const a = ARTIFACT_MAP[id];
            const rarity = artifactRarity(id);
            const mechanic = meta.mechanic;
            const newSynergies = getArtifactSynergiesAfterPick(st, id).filter((x) => !active.some((y) => y.id === x.id));
            return (
              <button key={id} onClick={() => onPick(id)} className={`p-5 rounded-xl bg-[#0d1726] border-2 ${rarityClass[rarity] || 'border-[#243b55]'} hover:scale-[1.02] transition-all text-left`}>
                <div className="flex items-center justify-between mb-3">
                  <span className={`text-[10px] uppercase tracking-widest font-bold ${rarityText[rarity] || ''}`}>{RARITY_LABELS[rarity][lang]}</span>
                  <span className="text-[#ffb84d]">✦</span>
                </div>
                <div className="font-bold text-lg mb-2">{a.name[lang]}</div>
                <div className="text-sm text-[#b6c9de] min-h-[4.5rem]">{a.desc[lang]}</div>
                {newSynergies.map((synergy) => (
                  <div key={synergy.id} className="mt-3 rounded-lg bg-[#8064a8]/10 border border-[#8064a8]/30 px-2 py-1.5">
                    <div className="text-[9px] uppercase tracking-wider font-bold text-[#8064a8]">{lang === 'ru' ? 'Активирует синергию' : 'Activates synergy'}</div>
                    <div className="text-xs font-bold text-[#8064a8]">{synergy.name[lang]}</div>
                  </div>
                ))}
                {mechanic && mechanicText[mechanic] && (
                  <div className={`mt-3 inline-flex px-2 py-1 rounded-md bg-black/5 text-[10px] uppercase tracking-wider font-bold ${rarityText[rarity] || ''}`}>
                    {mechanicText[mechanic][lang]}
                  </div>
                )}
              </button>
            );
          })}
        </div>
      </div>
    </div>
  );
}

// ===== Upgrade Modal =====
function UpgradeModal({ lang, t, st, onPick }: {
  lang: Lang; t: (k: TranslationKey) => string; st: GameState; onPick: (c: UpgradeChoice) => void;
}) {
  const choices = st.pendingUpgrade || [];
  const first = choices[0];
  const title = first?.type === 'modifier'
    ? (lang === 'ru' ? 'Новый модификатор' : 'New Modifier')
    : first?.abilityStage === 'branch'
      ? (lang === 'ru' ? 'Эволюция способности I' : 'Ability Evolution I')
      : first?.abilityStage === 'final'
        ? (lang === 'ru' ? 'Финальная форма способности' : 'Final Ability Form')
        : first?.sphereStage === 'branch'
          ? (lang === 'ru' ? 'Мутация сферы I' : 'Sphere Mutation I')
          : first?.sphereStage === 'final'
            ? (lang === 'ru' ? 'Мутация сферы II' : 'Sphere Mutation II')
            : t('chooseUpgrade');
  return (
    <div className="absolute inset-0 bg-black/70 flex items-center justify-center z-50">
      <div className="max-w-2xl w-full px-6">
        <h2 className="text-2xl font-bold text-center mb-6 text-[#39d8ff]">{title}</h2>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          {choices.map((choice, i) => (
            <button key={i} onClick={() => onPick(choice)} className="p-5 rounded-xl bg-[#0d1726] border border-[#5a8c4a]/30 hover:border-[#5a8c4a]/60 hover:scale-105 transition-all text-left">
              <div className="text-[#5a8c4a] text-[10px] uppercase tracking-wider mb-1">
                {choice.type === 'modifier' ? (lang === 'ru' ? 'МОДИФИКАТОР' : 'MODIFIER')
                  : choice.abilityStage === 'branch' ? (lang === 'ru' ? 'ВЕТКА СПОСОБНОСТИ' : 'ABILITY BRANCH')
                  : choice.abilityStage === 'final' ? (lang === 'ru' ? 'ФИНАЛЬНАЯ ФОРМА СПОСОБНОСТИ' : 'FINAL ABILITY FORM')
                  : choice.sphereStage === 'branch' ? (lang === 'ru' ? 'ВЕТКА СФЕРЫ' : 'SPHERE BRANCH')
                  : choice.sphereStage === 'final' ? (lang === 'ru' ? 'ФИНАЛЬНАЯ СПЕЦИАЛИЗАЦИЯ' : 'FINAL SPECIALIZATION')
                  : choice.type === 'ability' ? (lang === 'ru' ? 'АКТИВНАЯ СПОСОБНОСТЬ' : 'ACTIVE ABILITY')
                  : (lang === 'ru' ? 'УЛУЧШЕНИЕ СФЕРЫ' : 'SPHERE UPGRADE')}
              </div>
              <div className="font-bold text-lg mb-2">{choice.name?.[lang] || 'Sphere'}</div>
              <div className="text-sm text-[#b6c9de] mb-2">{choice.desc?.[lang] || ''}</div>
              <div className="text-xs text-[#7f9bb8]/70">
                {choice.type === 'modifier'
                  ? (lang === 'ru' ? 'Разблокирует эффект' : 'Unlocks the effect')
                  : choice.sphereStage === 'branch' || choice.abilityStage === 'branch'
                    ? (lang === 'ru' ? 'Уровень IV • выбор ветки' : 'Level IV • choose a branch')
                    : choice.sphereStage === 'final' || choice.abilityStage === 'final'
                      ? (lang === 'ru' ? 'Уровень VII • выбор финальной формы' : 'Level VII • choose final form')
                      : <>{t('level')} {choice.currentLevel} → {choice.newLevel}</>}
              </div>
            </button>
          ))}
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
    <div className="es-list-screen w-full max-w-2xl mx-auto px-6 py-8 overflow-y-auto max-h-screen">
      <div className="flex items-center justify-between mb-6">
        <button onClick={onBack} className="flex items-center gap-2 text-[#7f9bb8] hover:text-[#dcecff] transition"><ArrowLeft size={20} /> {t('back')}</button>
        <div className="flex items-center gap-2 text-[#ffb84d] font-bold"><span className="text-2xl">●</span> {shop.gold}</div>
      </div>
      <h2 className="text-2xl font-bold mb-6 text-center">{t('shopTitle')}</h2>
      <div className="grid gap-3">
        {SHOP_UPGRADES.map((def) => {
          const cur = shop.upgrades[def.id] || 0;
          const maxed = cur >= def.maxLevel;
          const cost = shopCost(def, cur);
          const canBuy = !maxed && shop.gold >= cost;
          return (
            <div key={def.id} className="flex items-center gap-4 p-4 rounded-xl bg-[#0d1726] border border-[#243b55]">
              <div className="flex-1">
                <div className="font-medium">{def.name[lang]}</div>
                <div className="text-sm text-[#7f9bb8]">{def.desc[lang](cur)}</div>
                <div className="text-xs text-[#7f9bb8]/50 mt-1">{t('owned')}: {cur}/{def.maxLevel}</div>
              </div>
              <button
                onClick={() => buy(def.id)}
                disabled={!canBuy}
                className={`px-5 py-2.5 rounded-lg font-medium text-sm transition min-w-24 ${maxed ? 'bg-[#0d1726] text-[#7f9bb8]/50' : canBuy ? 'bg-[#39d8ff]/20 border border-[#39d8ff]/40 text-[#dcecff] hover:bg-[#39d8ff]/30' : 'bg-[#0d1726] border border-[#243b55] text-[#7f9bb8]/50'}`}
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
  const [entries] = useState<LeaderEntry[]>(() => loadLeaderboard());
  return (
    <div className="es-list-screen w-full max-w-2xl mx-auto px-6 py-8 overflow-y-auto max-h-screen">
      <div className="flex items-center justify-between mb-6">
        <button onClick={onBack} className="flex items-center gap-2 text-[#7f9bb8] hover:text-[#dcecff] transition"><ArrowLeft size={20} /> {t('back')}</button>
      </div>
      <h2 className="text-2xl font-bold mb-6 text-center">{t('leaderTitle')}</h2>
      {entries.length === 0 ? (
        <p className="text-center text-[#7f9bb8]/70 py-12">{t('noScores')}</p>
      ) : (
        <div className="space-y-2">
          <div className="grid grid-cols-12 gap-2 text-xs text-[#7f9bb8]/70 uppercase px-4">
            <div className="col-span-2">{t('rank')}</div>
            <div className="col-span-6">{t('player')}</div>
            <div className="col-span-2 text-right">{t('wave')}</div>
            <div className="col-span-2 text-right">{t('score')}</div>
          </div>
          {entries.map((e, i) => (
            <div key={i} className={`grid grid-cols-12 gap-2 px-4 py-3 rounded-lg items-center ${i === 0 ? 'bg-[#ffb84d]/10 border border-[#ffb84d]/30' : 'bg-[#0d1726]'}`}>
              <div className="col-span-2 font-bold text-[#39d8ff]">#{i + 1}</div>
              <div className="col-span-6 truncate">{e.name}</div>
              <div className="col-span-2 text-right text-[#7f9bb8] text-sm">{e.wave}</div>
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
  const [unlocked] = useState<string[]>(() => loadAchievements());
  return (
    <div className="w-full max-w-2xl mx-auto px-6 py-8 overflow-y-auto max-h-screen">
      <div className="flex items-center justify-between mb-6">
        <button onClick={onBack} className="flex items-center gap-2 text-[#7f9bb8] hover:text-[#dcecff] transition"><ArrowLeft size={20} /> {t('back')}</button>
        <span className="text-sm text-[#7f9bb8]/70">{unlocked.length}/{ACHIEVEMENTS.length}</span>
      </div>
      <h2 className="text-2xl font-bold mb-6 text-center">{t('achievementsTitle')}</h2>
      <div className="grid gap-3">
        {ACHIEVEMENTS.map((ach) => {
          const isUnlocked = unlocked.includes(ach.id);
          return (
            <div key={ach.id} className={`flex items-center gap-4 p-4 rounded-xl border transition ${isUnlocked ? 'bg-[#ffb84d]/10 border-[#ffb84d]/30' : 'bg-[#0d1726] border-[#243b55] opacity-50'}`}>
              <div className={`w-10 h-10 rounded-full flex items-center justify-center ${isUnlocked ? 'bg-[#ffb84d]/20' : 'bg-[#0d1726]'}`}>
                <Award size={20} className={isUnlocked ? 'text-[#ffb84d]' : 'text-[#7f9bb8]/50'} />
              </div>
              <div className="flex-1">
                <div className={`font-medium ${isUnlocked ? 'text-[#ffb84d]' : 'text-[#7f9bb8]'}`}>{ach.name[lang]}</div>
                <div className="text-sm text-[#7f9bb8]/70">{ach.desc[lang]}</div>
              </div>
              <span className={`text-xs font-bold ${isUnlocked ? 'text-[#ffb84d]' : 'text-[#dcecff]/20'}`}>
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
function SettingsScreen({ lang, setLang, t, soundOn, setSoundOn, handedness, setHandedness, onBack }: {
  lang: Lang;
  setLang: (l: Lang) => void;
  t: (k: TranslationKey) => string;
  soundOn: boolean;
  setSoundOn: (v: boolean) => void;
  handedness: Handedness;
  setHandedness: (value: Handedness) => void;
  onBack: () => void;
}) {
  return (
    <div className="es-list-screen w-full max-w-md mx-auto px-6 py-8">
      <div className="flex items-center justify-between mb-6">
        <button onClick={onBack} className="flex items-center gap-2 text-[#7f9bb8] hover:text-[#dcecff] transition"><ArrowLeft size={20} /> {t('back')}</button>
      </div>
      <h2 className="text-2xl font-bold mb-6 text-center">{t('settingsTitle')}</h2>
      <div className="space-y-6">
        <div>
          <label className="text-xs text-[#7f9bb8] uppercase tracking-wider mb-2 block">{t('language')}</label>
          <div className="flex gap-2">
            <button onClick={() => setLang('ru')} className={`flex-1 py-3 rounded-xl border transition ${lang === 'ru' ? 'bg-[#39d8ff]/20 border-[#39d8ff]/40 text-[#dcecff]' : 'bg-[#0d1726] border-[#243b55] text-[#7f9bb8]'}`}>Русский</button>
            <button onClick={() => setLang('en')} className={`flex-1 py-3 rounded-xl border transition ${lang === 'en' ? 'bg-[#39d8ff]/20 border-[#39d8ff]/40 text-[#dcecff]' : 'bg-[#0d1726] border-[#243b55] text-[#7f9bb8]'}`}>English</button>
          </div>
        </div>
        <div>
          <label className="text-xs text-[#7f9bb8] uppercase tracking-wider mb-2 block">{t('sound')}</label>
          <div className="flex gap-2">
            <button onClick={() => setSoundOn(true)} className={`flex-1 py-3 rounded-xl border transition ${soundOn ? 'bg-[#39d8ff]/20 border-[#39d8ff]/40 text-[#dcecff]' : 'bg-[#0d1726] border-[#243b55] text-[#7f9bb8]'}`}>{t('soundOn')}</button>
            <button onClick={() => setSoundOn(false)} className={`flex-1 py-3 rounded-xl border transition ${!soundOn ? 'bg-[#39d8ff]/20 border-[#39d8ff]/40 text-[#dcecff]' : 'bg-[#0d1726] border-[#243b55] text-[#7f9bb8]'}`}>{t('soundOff')}</button>
          </div>
        </div>
        <div>
          <label className="text-xs text-[#7f9bb8] uppercase tracking-wider mb-2 block">{t('handedness')}</label>
          <div className="flex gap-2">
            <button onClick={() => setHandedness('right')} className={`flex-1 py-3 rounded-xl border transition ${handedness === 'right' ? 'bg-[#39d8ff]/20 border-[#39d8ff]/40 text-[#dcecff]' : 'bg-[#0d1726] border-[#243b55] text-[#7f9bb8]'}`}>{t('rightHanded')}</button>
            <button onClick={() => setHandedness('left')} className={`flex-1 py-3 rounded-xl border transition ${handedness === 'left' ? 'bg-[#39d8ff]/20 border-[#39d8ff]/40 text-[#dcecff]' : 'bg-[#0d1726] border-[#243b55] text-[#7f9bb8]'}`}>{t('leftHanded')}</button>
          </div>
        </div>
        <div>
          <label className="text-xs text-[#7f9bb8] uppercase tracking-wider mb-2 block">{t('controls')}</label>
          <div className="space-y-2 text-sm text-[#7f9bb8] bg-[#0d1726] rounded-xl p-4 border border-[#243b55]">
            <div>{t('moveControls')}</div>
            <div>{t('placeSphere')}</div>
            <div>{t('activeAbilities')}</div>
            <div>{t('dashAbility')}</div>
          </div>
        </div>
        <button
          onClick={() => { if (confirm(t('resetConfirm'))) { resetAll(); location.reload(); } }}
          className="w-full flex items-center justify-center gap-2 py-3 rounded-xl bg-[#ff4d5d]/10 border border-[#ff4d5d]/30 text-[#ff4d5d] hover:bg-[#ff4d5d]/20 transition"
        >
          <RotateCcw size={18} /> {t('reset')}
        </button>
      </div>
    </div>
  );
}
