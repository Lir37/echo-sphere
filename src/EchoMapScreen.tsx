import { REGION_CHALLENGES, REGION_POCKETS, getRegionChallengeProgress, loadRegionChallengeCompletions, loadRegionEndlessUnlock, loadRegionStabilized, type RegionChallengeId, type RegionMode } from './region';
import { CHARACTER_DEFS } from './characters';
import { loadCharacterId } from './persistence';
import { DIFFICULTIES, type Difficulty, type Lang } from './gameData';

type StartRun = (mode: RegionMode, challenge: RegionChallengeId) => void;

export default function EchoMapScreen({
  lang,
  difficulty,
  onStartRun,
  onCharacters,
  onBack,
}: {
  lang: Lang;
  difficulty: Difficulty;
  onStartRun: StartRun;
  onCharacters: () => void;
  onBack: () => void;
}) {
  const completed = loadRegionChallengeCompletions();
  const progress = getRegionChallengeProgress();
  const endlessUnlocked = loadRegionEndlessUnlock();
  const stabilized = loadRegionStabilized();
  const character = CHARACTER_DEFS[loadCharacterId()];
  const challengePositions = ['es-map-satellite-a', 'es-map-satellite-b', 'es-map-satellite-c'];

  const challengeCopy: Record<RegionChallengeId, { kicker: { ru: string; en: string }; goal: { ru: string; en: string }; accent: string }> = {
    none: { kicker: { ru: 'БАЗОВЫЙ МАРШРУТ', en: 'BASE ROUTE' }, goal: { ru: '30:00 + 3 стража', en: '30:00 + 3 wardens' }, accent: '#63e6ff' },
    fractured_network: { kicker: { ru: 'ИСПЫТАНИЕ', en: 'CHALLENGE' }, goal: { ru: 'Сеть рвётся каждые 45с', en: 'Network breaks every 45s' }, accent: '#ff6b6b' },
    overload: { kicker: { ru: 'ИСПЫТАНИЕ', en: 'CHALLENGE' }, goal: { ru: 'Враги сильнее, XP больше', en: 'Stronger enemies, more XP' }, accent: '#ffb84d' },
    low_gravity: { kicker: { ru: 'ИСПЫТАНИЕ', en: 'CHALLENGE' }, goal: { ru: 'Быстрее Core, слабее отталкивание', en: 'Faster Core, weaker repulsion' }, accent: '#a88cff' },
  };

  return (
    <div className="es-map-screen">
      <header className="es-map-header">
        <button type="button" className="es-map-back" onClick={onBack}>
          <span>‹</span>
          {lang === 'ru' ? 'К МЕНЮ' : 'MENU'}
        </button>
        <div className="text-center">
          <div className="es-map-title">{lang === 'ru' ? 'КАРТА ЭХА' : 'ECHO MAP'}</div>
          <div className="es-map-subtitle">{lang === 'ru' ? 'Собирай осколки. Открывай формы. Углубляйся в Эхо.' : 'Collect fragments. Open forms. Descend into the Echo.'}</div>
        </div>
        <button type="button" className="es-map-character" onClick={onCharacters}>
          <span className="es-map-character-orb" style={{ ['--map-char' as string]: character.color }}>✦</span>
          <span>
            <b>{character.name[lang]}</b>
            <small>{DIFFICULTIES.find(item => item.id === difficulty)?.name[lang] || difficulty}</small>
          </span>
        </button>
      </header>

      <main className="es-map-main">
        <div className="es-map-space">
          <div className="es-map-stars" />
          <div className="es-map-grid" />

          <svg className="es-map-orbits" viewBox="0 0 1000 700" aria-hidden="true">
            <ellipse cx="500" cy="340" rx="285" ry="175" />
            <ellipse cx="500" cy="340" rx="220" ry="132" />
            <ellipse cx="500" cy="340" rx="150" ry="92" />
          </svg>

          {REGION_POCKETS.map((pocket, index) => (
            <span
              key={pocket.id}
              className={`es-map-fragment fragment-${index}`}
              title={pocket.name[lang]}
              style={{ ['--fragment-color' as string]: pocket.accent }}
            >
              ◇
            </span>
          ))}

          <button type="button" className="es-map-region-node" onClick={() => onStartRun('stabilization', 'none')} aria-label={lang === 'ru' ? 'Начать стабилизацию Резонансного бассейна' : 'Start Resonance Basin stabilization'}>
            <span className="es-map-planet-ring ring-outer" />
            <span className="es-map-planet-ring ring-mid" />
            <span className="es-map-planet" />
            <span className="es-map-planet-core" />
            <span className="es-map-node-label">
              <b>{lang === 'ru' ? 'РЕЗОНАНСНЫЙ БАССЕЙН' : 'RESONANCE BASIN'}</b>
              <small>{stabilized ? (lang === 'ru' ? 'СТАБИЛИЗИРОВАН' : 'STABILIZED') : (lang === 'ru' ? 'ОБЛАСТЬ СИГНАЛА' : 'SIGNAL REGION')}</small>
            </span>
          </button>

          {REGION_CHALLENGES.map((challenge, index) => {
            const done = completed.includes(challenge.id);
            const meta = challengeCopy[challenge.id];
            return (
              <button
                type="button"
                key={challenge.id}
                className={`es-map-satellite ${challengePositions[index]}`}
                style={{ ['--satellite-color' as string]: meta.accent }}
                onClick={() => onStartRun('stabilization', challenge.id)}
              >
                <span className="es-map-satellite-orbit" />
                <span className="es-map-satellite-core">◈</span>
                <span className="es-map-satellite-copy">
                  <small>{challenge.name[lang]}</small>
                  <b>{done ? (lang === 'ru' ? 'ПРОЙДЕНО' : 'CLEARED') : meta.kicker[lang]}</b>
                  <em>{challenge.desc[lang]}</em>
                </span>
              </button>
            );
          })}

          <button
            type="button"
            disabled={!endlessUnlocked}
            className={`es-map-endless-core ${endlessUnlocked ? 'is-unlocked' : 'is-locked'}`}
            onClick={() => endlessUnlocked && onStartRun('endless', 'none')}
          >
            <span className="es-map-endless-core-glow" />
            <span className="es-map-endless-core-symbol">✦</span>
            <span className="es-map-endless-core-copy">
              <b>{lang === 'ru' ? 'БЕСКОНЕЧНОЕ ЯДРО' : 'ENDLESS CORE'}</b>
              <small>{endlessUnlocked
                ? (lang === 'ru' ? 'ОТКРЫТО · ВХОД В БЕСКОНЕЧНОСТЬ' : 'UNLOCKED · ENTER INFINITY')
                : (lang === 'ru' ? `ЗАКРЫТО · ИСПЫТАНИЯ ${progress}/3` : `LOCKED · CHALLENGES ${progress}/3`)}</small>
            </span>
          </button>

          <div className="es-map-fragment-caption">
            <span>{lang === 'ru' ? '5 ОСКОЛКОВ' : '5 FRAGMENTS'}</span>
            <small>{lang === 'ru' ? 'покеты региона' : 'regional pockets'}</small>
          </div>
        </div>

        <section className="es-map-info">
          <div>
            <div className="es-map-info-kicker">{lang === 'ru' ? 'ТЕКУЩИЙ СИГНАЛ' : 'CURRENT SIGNAL'}</div>
            <h2>{lang === 'ru' ? 'Резонансный бассейн' : 'Resonance Basin'}</h2>
            <p>{lang === 'ru'
              ? 'Центральный регион-узел. Сначала стабилизируй его. Затем пройди три орбитальных испытания. Каждый маршрут меняет правила боя, а не просто повышает числа.'
              : 'The central region node. Stabilize it first, then clear the three orbital challenges. Each route changes how the run plays, not just the numbers.'}</p>
          </div>

          <div className="es-map-goal-row">
            <div>
              <small>{lang === 'ru' ? 'БАЗОВЫЙ ЗАБЕГ' : 'BASE RUN'}</small>
              <b>{challengeCopy.none.goal[lang]}</b>
            </div>
            <div>
              <small>{lang === 'ru' ? 'СПУТНИКИ' : 'SATELLITES'}</small>
              <b>{progress}/3</b>
            </div>
            <div className={endlessUnlocked ? 'is-ready' : ''}>
              <small>{lang === 'ru' ? 'ЯДРО' : 'CORE'}</small>
              <b>{endlessUnlocked ? (lang === 'ru' ? 'ГОТОВО' : 'READY') : (lang === 'ru' ? 'ЗАПЕЧАТАНО' : 'SEALED')}</b>
            </div>
          </div>
        </section>
      </main>
    </div>
  );
}
