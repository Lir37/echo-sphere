import { ChevronRight, Infinity as InfinityIcon, LockKeyhole, Swords, Target } from 'lucide-react';
import { REGION_CONFIGS, getRegionConfig, isRegionAvailable, loadRegionChallengeCompletions, loadRegionEndlessUnlock, loadRegionStabilized, type RegionChallengeId, type RegionId, type RegionMode } from './region';
import type { Lang } from './i18n';

type StartRun = (regionId: RegionId, mode: RegionMode, challenge: RegionChallengeId) => void;

const challengeAccent: Record<string, string> = {
  fractured_network: '#ff6b6b',
  overload: '#ffb84d',
  low_gravity: '#a88cff',
  mirror_tide: '#7fd8ff',
  volatile_core: '#f49cff',
  phase_drift: '#64e4c4',
};

export default function EchoMapScreen({
  lang,
  onStartRun,
  onBack,
}: {
  lang: Lang;
  onStartRun: StartRun;
  onBack: () => void;
}) {
  return (
    <div className="es-stage-select">
      <header className="es-stage-select-header">
        <button type="button" className="es-stage-select-back" onClick={onBack}>
          <span>‹</span>
          {lang === 'ru' ? 'В МЕНЮ' : 'MENU'}
        </button>
        <div className="es-stage-select-heading">
          <div className="es-stage-select-title">{lang === 'ru' ? 'РЕГИОНЫ ЭХА' : 'ECHO REGIONS'}</div>
          <div className="es-stage-select-subtitle">
            {lang === 'ru' ? 'СТАБИЛИЗИРУЙ · ИСПЫТАЙ · УГЛУБИСЬ' : 'STABILIZE · CHALLENGE · DESCEND'}
          </div>
        </div>
      </header>

      <main className="es-stage-select-main">
        <div className="es-stage-select-intro">
          <span>{lang === 'ru' ? 'ВЫБОР ОБЛАСТИ' : 'REGION SELECT'}</span>
          <b>{lang === 'ru' ? 'Один регион · несколько способов пройти его.' : 'One region · several ways to run it.'}</b>
        </div>

        <div className="es-region-list">
          {REGION_CONFIGS.map((region, index) => {
            const available = isRegionAvailable(region.id);
            const stabilized = available && loadRegionStabilized(region.id);
            const completed = loadRegionChallengeCompletions(region.id);
            const endlessUnlocked = available && loadRegionEndlessUnlock(region.id);
            const progress = completed.length;
            const baseLabel = stabilized
              ? (lang === 'ru' ? 'ПОВТОРИТЬ СТАБИЛИЗАЦИЮ' : 'REPLAY STABILIZATION')
              : (lang === 'ru' ? 'СТАБИЛИЗАЦИЯ · 30:00' : 'STABILIZATION · 30:00');

            return (
              <section
                key={region.id}
                className={'es-region-card ' + (available ? 'is-available' : 'is-locked')}
                style={{ ['--region-accent' as string]: region.accent }}
              >
                <div className="es-region-card-header">
                  <div className="es-region-index">0{index + 1}</div>
                  <div className="es-region-card-title-group">
                    <div className="es-region-card-kicker">{lang === 'ru' ? 'РЕГИОН' : 'REGION'} {String(index + 1).padStart(2, '0')}</div>
                    <h2>{region.name[lang]}</h2>
                    <p>{region.subtitle[lang]}</p>
                  </div>
                  <div className="es-region-status">
                    {available ? (
                      stabilized ? (
                        <span className="is-stable">{lang === 'ru' ? 'СТАБИЛЕН' : 'STABLE'}</span>
                      ) : (
                        <span>{lang === 'ru' ? 'ДОСТУПЕН' : 'AVAILABLE'}</span>
                      )
                    ) : <LockKeyhole size={15} />}
                  </div>
                </div>

                <div className="es-region-card-rule" />

                {!available ? (
                  <div className="es-region-locked-message">
                    <LockKeyhole size={15} />
                    <span>
                      {lang === 'ru'
                        ? 'Откроется после стабилизации Резонансного Бассейна.'
                        : 'Unlocks after Resonance Basin is stabilized.'}
                    </span>
                  </div>
                ) : (
                  <>
                    <button
                      type="button"
                      className="es-region-primary-run"
                      onClick={() => onStartRun(region.id, 'stabilization', 'none')}
                    >
                      <span className="es-run-icon"><Swords size={17} /></span>
                      <span className="es-run-copy">
                        <b>{baseLabel}</b>
                        <small>{lang === 'ru' ? 'Базовый маршрут · открывает историю региона' : 'Base route · opens the region story'}</small>
                      </span>
                      <ChevronRight size={18} />
                    </button>

                    <div className="es-region-section-heading">
                      <span>{lang === 'ru' ? 'ИСПЫТАНИЯ' : 'CHALLENGES'}</span>
                      <b>{progress}/3</b>
                    </div>

                    <div className="es-region-challenges">
                      {region.challenges.map((challenge) => {
                        const done = completed.includes(challenge.id);
                        const accent = challengeAccent[challenge.id] || region.accent;
                        return (
                          <button
                            key={challenge.id}
                            type="button"
                            disabled={!stabilized}
                            className={'es-region-challenge ' + (done ? 'is-complete' : '') + (!stabilized ? ' is-locked' : '')}
                            style={{ ['--challenge-accent' as string]: accent }}
                            onClick={() => stabilized && onStartRun(region.id, 'stabilization', challenge.id)}
                          >
                            <span className="es-region-challenge-mark" />
                            <span className="es-region-challenge-copy">
                              <b>{challenge.name[lang]}</b>
                              <small>{challenge.desc[lang]}</small>
                            </span>
                            <span className="es-region-challenge-state">
                              {done ? (lang === 'ru' ? 'ПРОЙДЕНО' : 'CLEARED') : stabilized ? <ChevronRight size={15} /> : <LockKeyhole size={14} />}
                            </span>
                          </button>
                        );
                      })}
                    </div>

                    <button
                      type="button"
                      disabled={!endlessUnlocked}
                      className={'es-region-endless ' + (endlessUnlocked ? 'is-unlocked' : 'is-locked')}
                      onClick={() => endlessUnlocked && onStartRun(region.id, 'endless', 'none')}
                    >
                      <span className="es-region-endless-mark"><InfinityIcon size={20} /></span>
                      <span className="es-region-endless-copy">
                        <b>{lang === 'ru' ? 'БЕСКОНЕЧНОСТЬ' : 'ENDLESS'}</b>
                        <small>
                          {endlessUnlocked
                            ? (lang === 'ru' ? 'Все три испытания пройдены · вход открыт' : 'All three challenges cleared · access open')
                            : (lang === 'ru' ? `Закрыто · испытания ${progress}/3` : `Locked · challenges ${progress}/3`)}
                        </small>
                      </span>
                      {endlessUnlocked ? <ChevronRight size={18} /> : <LockKeyhole size={15} />}
                    </button>
                  </>
                )}
              </section>
            );
          })}
        </div>

        <div className="es-stage-select-foot">
          <Target size={13} />
          <span>{lang === 'ru' ? 'Каждая область меняет ритм врагов, события и давление на Сеть.' : 'Each region changes enemy rhythm, events and Network pressure.'}</span>
        </div>
      </main>
    </div>
  );
}
