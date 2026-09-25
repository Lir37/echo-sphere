import { useMemo, useState } from 'react';
import {
  ArrowLeft, BookOpen, ChevronRight, LockKeyhole, Search,
  Sparkles, Swords, Network, CircleDot, Shield, Gem, Zap,
} from 'lucide-react';
import {
  ABILITIES, ARTIFACTS, BOSS_TYPES, SPHERE_TYPES,
  type AbilityType, type ArtifactId, type SphereType, type BossType,
} from './gameData';
import {
  ABILITY_PROGRESSION, SPHERE_PROGRESSION,
} from './sphereProgression';
import { loadKnowledge, unlockKnowledge, type KnowledgeId } from './persistence';

type KnowledgeSection =
  | 'basics'
  | 'spheres'
  | 'monsters'
  | 'bosses'
  | 'abilities'
  | 'artifacts'
  | 'evolutions';

type Lang = 'ru' | 'en';

type Localized = { ru: string; en: string };

const ENEMY_DEFS: Array<{ id: string; name: Localized; desc: Localized; rule: Localized }> = [
  { id: 'normal', name: { ru: 'Обычный', en: 'Normal' }, desc: { ru: 'Базовый противник. Опасен числом и постоянным давлением.', en: 'The basic enemy. Dangerous through numbers and constant pressure.' }, rule: { ru: 'Держит курс на игрока и наносит контактный урон.', en: 'Moves toward the player and deals contact damage.' } },
  { id: 'fast', name: { ru: 'Стремительный', en: 'Fast' }, desc: { ru: 'Быстрый противник, который сокращает дистанцию быстрее остальных.', en: 'A fast enemy that closes distance quicker than the rest.' }, rule: { ru: 'Маленький и быстрый. Особенно опасен, когда отвлекает от сети.', en: 'Small and fast. Especially dangerous when it pulls attention away from the Network.' } },
  { id: 'tank', name: { ru: 'Танк', en: 'Tank' }, desc: { ru: 'Медленный тяжёлый противник с повышенной живучестью.', en: 'A slow heavy enemy with increased durability.' }, rule: { ru: 'Идёт медленно, но требует заметно больше урона.', en: 'Moves slowly but takes substantially more damage to bring down.' } },
  { id: 'elite', name: { ru: 'Link Breaker', en: 'Link Breaker' }, desc: { ru: 'Элитный враг, который атакует не только игрока, но и саму боевую сеть.', en: 'An elite enemy that attacks the combat Network itself.' }, rule: { ru: 'Телеграфирует цель, затем временно отключает сферу от Network.', en: 'Telegraphs a target, then temporarily removes a Sphere from the Network.' } },
];

const BOSS_INFO: Record<BossType, Localized> = {
  shooter: { ru: 'Стрелок: держит дистанцию и периодически выпускает веер снарядов.', en: 'Shooter: keeps distance and periodically fires a spread of projectiles.' },
  charger: { ru: 'Зарядник: телеграфирует рывок, затем совершает опасный прорыв.', en: 'Charger: telegraphs a charge, then commits to a dangerous dash.' },
  summoner: { ru: 'Призыватель: создаёт миньонов и одновременно атакует снарядами.', en: 'Summoner: creates minions while also attacking with projectiles.' },
  aura: { ru: 'Аура: создаёт опасную зону вокруг себя и дополнительно стреляет.', en: 'Aura: creates a dangerous zone around itself and also fires projectiles.' },
};

const SECTION_DEFS: Array<{ id: KnowledgeSection; label: Localized; sub: Localized; icon: React.ReactNode }> = [
  { id: 'basics', label: { ru: 'Ядро игры', en: 'Core' }, sub: { ru: 'Как работает забег', en: 'How a run works' }, icon: <BookOpen size={17} /> },
  { id: 'spheres', label: { ru: 'Сферы', en: 'Spheres' }, sub: { ru: 'Узлы боевой сети', en: 'Combat network nodes' }, icon: <CircleDot size={17} /> },
  { id: 'monsters', label: { ru: 'Монстры', en: 'Monsters' }, sub: { ru: 'Противники', en: 'Enemies' }, icon: <Swords size={17} /> },
  { id: 'bosses', label: { ru: 'Боссы', en: 'Bosses' }, sub: { ru: 'Главные угрозы', en: 'Major threats' }, icon: <Shield size={17} /> },
  { id: 'abilities', label: { ru: 'Способности', en: 'Abilities' }, sub: { ru: 'Активные и пассивные', en: 'Active and passive' }, icon: <Zap size={17} /> },
  { id: 'artifacts', label: { ru: 'Артефакты', en: 'Artifacts' }, sub: { ru: 'Редкие усиления', en: 'Rare upgrades' }, icon: <Gem size={17} /> },
  { id: 'evolutions', label: { ru: 'Эволюции', en: 'Evolutions' }, sub: { ru: 'Ветки и финалы', en: 'Branches and finals' }, icon: <Network size={17} /> },
];

function text(value: Localized, lang: Lang): string {
  return value[lang];
}

let knowledgeCache: Set<string> | null = null;

export function recordKnowledge(ids: KnowledgeId[]): void {
  if (!knowledgeCache) knowledgeCache = new Set(loadKnowledge());
  const next = ids.filter((id) => !knowledgeCache!.has(id));
  if (next.length > 0) {
    unlockKnowledge(next);
    for (const id of next) knowledgeCache.add(id);
  }
}

export function syncKnowledgeFromRun(s: {
  spheres: Array<{ type: SphereType; alive: boolean }>;
  enemies: Array<{ type: string; isBoss: boolean; bossType: BossType }>;
  bossDefeated: number;
  player: {
    abilities: Partial<Record<AbilityType, number>>;
    artifacts: ArtifactId[];
    sphereProgression: Partial<Record<SphereType, number>>;
    sphereBranches: Partial<Record<SphereType, string>>;
    sphereMods: Record<string, number>;
    evolutions: string[];
  };
}): void {
  const ids: KnowledgeId[] = [];

  for (const sphere of s.spheres) ids.push(`sphere:${sphere.type}`);
  for (const enemy of s.enemies) {
    if (enemy.isBoss) ids.push(`boss:${enemy.bossType}`);
    else ids.push(`monster:${enemy.type}`);
  }

  for (const [ability, level] of Object.entries(s.player.abilities)) {
    if ((level || 0) > 0) ids.push(`ability:${ability}`);
  }
  for (const artifact of s.player.artifacts) ids.push(`artifact:${artifact}`);
  for (const [sphere, level] of Object.entries(s.player.sphereProgression)) {
    if ((level || 0) > 0) ids.push(`sphere:${sphere}`);
  }
  for (const branch of Object.values(s.player.sphereBranches)) {
    if (branch) ids.push(`sphere-branch:${branch}`);
  }
  for (const marker of s.player.evolutions) {
    if (marker.startsWith('sphere:')) {
      const parts = marker.split(':');
      if (parts[3]) ids.push(`sphere-final:${parts[3]}`);
    }
    if (marker.startsWith('ability:')) {
      const parts = marker.split(':');
      if (parts[3]) ids.push(`ability-evolution:${parts[3]}`);
    }
  }

  recordKnowledge(ids);
}

function KnowledgeCard({ unlocked, title, desc, children, lang }: {
  unlocked: boolean; title: string; desc: string; children?: React.ReactNode; lang: Lang;
}) {
  return (
    <article className={`rounded-xl border p-4 transition ${unlocked
      ? 'border-cyan-300/20 bg-slate-950/55'
      : 'border-slate-700/40 bg-slate-950/35 opacity-80'}`}>
      <div className="flex items-start gap-3">
        <div className={`mt-0.5 flex h-9 w-9 shrink-0 items-center justify-center rounded-lg border ${unlocked
          ? 'border-cyan-300/25 bg-cyan-300/5 text-cyan-200'
          : 'border-slate-600/30 bg-slate-800/30 text-slate-500'}`}>
          {unlocked ? <Sparkles size={15} /> : <LockKeyhole size={15} />}
        </div>
        <div className="min-w-0 flex-1">
          <h3 className="font-semibold text-slate-100">{unlocked ? title : (lang === 'ru' ? 'Запись не открыта' : 'Entry locked')}</h3>
          <p className="mt-1 text-xs leading-5 text-slate-400">
            {unlocked ? desc : (lang === 'ru' ? 'Получите этот объект во время игры, чтобы открыть запись.' : 'Obtain or encounter this during a run to unlock the entry.')}
          </p>
          {unlocked && children}
        </div>
      </div>
    </article>
  );
}

export default function KnowledgeBase({ lang, onBack }: { lang: Lang; onBack: () => void }) {
  const [section, setSection] = useState<KnowledgeSection>('basics');
  const [query, setQuery] = useState('');
  const [, refresh] = useState(0);
  const unlocked = useMemo(() => new Set(loadKnowledge()), [section, query]);
  const isRu = lang === 'ru';
  const q = query.trim().toLowerCase();

  const matches = (value: string) => !q || value.toLowerCase().includes(q);

  const content = (() => {
    if (section === 'basics') {
      return (
        <div className="space-y-4">
          <KnowledgeCard unlocked title={isRu ? 'Что такое Echo Sphere?' : 'What is Echo Sphere?'}
            desc={isRu
              ? 'Ты строишь боевую сеть вокруг Core. Сферы автоматически атакуют, а их расположение создаёт Geometry и меняет поведение сети.'
              : 'You build a combat Network around the Core. Spheres attack automatically, while their arrangement creates Geometry and changes how the Network behaves.'} lang={lang}>
            <div className="mt-4 grid gap-3 sm:grid-cols-2">
              {[
                [isRu ? 'Сферы' : 'Spheres', isRu ? 'Ставь и развивай узлы сети.' : 'Place and evolve Network nodes.'],
                [isRu ? 'Network' : 'Network', isRu ? 'Связывает близкие сферы и открывает сетевые эффекты.' : 'Connects nearby Spheres and unlocks network effects.'],
                [isRu ? 'Geometry' : 'Geometry', isRu ? 'Форма расположения превращается в отдельный боевой эффект.' : 'The shape of your arrangement becomes a combat effect.'],
                [isRu ? 'Resonance' : 'Resonance', isRu ? 'Общий заряд сети. Доведи его до 100, чтобы запустить событие формации.' : 'The Network charge. Reach 100 to trigger a formation event.'],
              ].map(([title, desc]) => (
                <div key={title} className="rounded-lg border border-cyan-300/10 bg-cyan-300/[0.025] p-3">
                  <div className="text-xs font-bold uppercase tracking-wider text-cyan-200">{title}</div>
                  <div className="mt-1 text-xs leading-5 text-slate-400">{desc}</div>
                </div>
              ))}
            </div>
          </KnowledgeCard>

          <KnowledgeCard unlocked title={isRu ? '⚡ Resonance: зачем его копить' : '⚡ Resonance: why charge it'}
            desc={isRu
              ? 'Resonance — это не второй XP и не просто счётчик попаданий. Это общий ресурс забега, который награждает активную игру сети.'
              : 'Resonance is not XP and not merely a hit counter. It is a shared run resource that rewards active Network play.'} lang={lang}>
            <div className="mt-4 space-y-3 text-sm leading-6 text-slate-300">
              <p>{isRu ? 'Как копится: попадания сфер дают +1; появление новой формации Geometry даёт +5; отдельные руны и эффекты могут дать крупный заряд.' : 'How it charges: Sphere hits give +1; gaining a new Geometry formation gives +5; selected Runes and effects can provide larger bursts.'}</p>
              <p>{isRu ? 'Что делать: не нужно нажимать отдельную кнопку. Просто продолжай атаковать и поддерживай работающую сеть.' : 'What to do: there is no separate button to press. Keep attacking and maintain a functioning Network.'}</p>
              <p>{isRu ? 'Что происходит на 100: срабатывает Resonance Event, зависящий от текущей формации. Это временный боевой эффект, а не трата ресурса в пустоту.' : 'At 100: a Resonance Event fires based on the current formation. It is a temporary combat effect, not a resource spent for nothing.'}</p>
              <p className="text-cyan-200">{isRu ? 'Пример: Triangle может дать дугу по реальным связям, Cluster — отброс, Line — усилить следующий выстрел.' : 'Example: Triangle can fire along real links, Cluster can knock enemies back, and Line can empower the next shot.'}</p>
            </div>
          </KnowledgeCard>

          <KnowledgeCard unlocked title={isRu ? 'Как читать сеть' : 'How to read the Network'}
            desc={isRu
              ? 'Близость сфер создаёт связи. Ghost Snap показывает будущие связи до подтверждения перестановки. Если Link Breaker отключил узел, часть Geometry и сетевых эффектов может измениться.'
              : 'Nearby Spheres create links. Ghost Snap previews future links before you commit a move. If a Link Breaker disables a node, Geometry and network effects can change.'} lang={lang} />
        </div>
      );
    }

    if (section === 'spheres') {
      return (
        <div className="grid gap-3 sm:grid-cols-2">
          {(Object.keys(SPHERE_TYPES) as SphereType[]).filter((id) => matches(SPHERE_TYPES[id].name[lang])).map((id) => {
            const def = SPHERE_TYPES[id];
            const open = unlocked.has(`sphere:${id}`);
            return <KnowledgeCard key={id} unlocked={open} title={def.name[lang]}
              desc={isRu ? `Роль: ${id === 'standard' ? 'универсальный урон' : id === 'sniper' ? 'дальняя одиночная цель' : id === 'chain' ? 'перенос урона между целями' : id === 'shotgun' ? 'ближний burst' : id === 'aura' ? 'локальное поле' : 'специализированный сетевой узел'}.` : `Role: ${id} Sphere combat behavior.`} lang={lang}>
              <div className="mt-3 text-xs leading-5 text-slate-400">{isRu ? 'Эта запись открывается, когда сфера впервые появляется в твоём забеге.' : 'This entry unlocks when the Sphere first appears in your run.'}</div>
            </KnowledgeCard>;
          })}
        </div>
      );
    }

    if (section === 'monsters') {
      return (
        <div className="grid gap-3 sm:grid-cols-2">
          {ENEMY_DEFS.filter((enemy) => matches(text(enemy.name, lang))).map((enemy) => {
            const open = unlocked.has(`monster:${enemy.id}`);
            return <KnowledgeCard key={enemy.id} unlocked={open} title={text(enemy.name, lang)} desc={text(enemy.desc, lang)} lang={lang}>
              <div className="mt-2 text-xs text-slate-500">{text(enemy.rule, lang)}</div>
            </KnowledgeCard>;
          })}
        </div>
      );
    }

    if (section === 'bosses') {
      return (
        <div className="grid gap-3 sm:grid-cols-2">
          {(Object.keys(BOSS_TYPES) as BossType[]).filter((id) => matches(BOSS_TYPES[id].name[lang])).map((id) => {
            const open = unlocked.has(`boss:${id}`);
            return <KnowledgeCard key={id} unlocked={open} title={BOSS_TYPES[id].name[lang]} desc={BOSS_INFO[id][lang]} lang={lang} />;
          })}
        </div>
      );
    }

    if (section === 'abilities') {
      return (
        <div className="grid gap-3 sm:grid-cols-2">
          {(Object.keys(ABILITIES) as AbilityType[]).filter((id) => matches(ABILITIES[id].name[lang])).map((id) => {
            const open = unlocked.has(`ability:${id}`);
            const def = ABILITIES[id];
            const progression = ABILITY_PROGRESSION[id];
            const level1 = progression?.levels[0]?.desc[lang] || def.desc[lang](1);
            return <KnowledgeCard key={id} unlocked={open} title={def.name[lang]} desc={level1} lang={lang}>
              {open && progression && (
                <div className="mt-3 space-y-1 text-xs text-slate-500">
                  <div>{isRu ? 'Максимум: 7 уровней. На IV и VII открывается отдельный выбор эволюции.' : 'Max level: 7. Separate evolution choices appear at IV and VII.'}</div>
                </div>
              )}
            </KnowledgeCard>;
          })}
        </div>
      );
    }

    if (section === 'artifacts') {
      return (
        <div className="grid gap-3 sm:grid-cols-2">
          {ARTIFACTS.filter((a) => matches(a.name[lang])).map((artifact) => {
            const open = unlocked.has(`artifact:${artifact.id}`);
            return <KnowledgeCard key={artifact.id} unlocked={open} title={artifact.name[lang]} desc={artifact.desc[lang]} lang={lang} />;
          })}
        </div>
      );
    }

    const sphereEntries = (Object.values(SPHERE_PROGRESSION) as any[]).filter(Boolean);
    const abilityEntries = (Object.values(ABILITY_PROGRESSION) as any[]).filter(Boolean);
    return (
      <div className="space-y-5">
        <div>
          <div className="mb-2 text-xs font-bold uppercase tracking-wider text-fuchsia-200">{isRu ? 'Ветки сфер' : 'Sphere branches'}</div>
          <div className="grid gap-3 sm:grid-cols-2">
            {sphereEntries.flatMap((def) => def.evolution4Choices.flatMap((branch: any) => {
              const open = unlocked.has(`sphere-branch:${branch.id}`);
              return [<KnowledgeCard key={branch.id} unlocked={open} title={`${def.name[lang]} · ${branch.name[lang]}`} desc={branch.desc[lang]} lang={lang} />];
            }))}
          </div>
        </div>
        <div>
          <div className="mb-2 text-xs font-bold uppercase tracking-wider text-fuchsia-200">{isRu ? 'Финальные формы сфер' : 'Final Sphere forms'}</div>
          <div className="grid gap-3 sm:grid-cols-2">
            {sphereEntries.flatMap((def) => def.evolution4Choices.flatMap((branch: any) => branch.final.map((final: any) => {
              const open = unlocked.has(`sphere-final:${final.id}`);
              return <KnowledgeCard key={final.id} unlocked={open} title={`${def.name[lang]} · ${final.name[lang]}`} desc={final.desc[lang]} lang={lang} />;
            })))}
          </div>
        </div>
        <div>
          <div className="mb-2 text-xs font-bold uppercase tracking-wider text-fuchsia-200">{isRu ? 'Эволюции способностей' : 'Ability evolutions'}</div>
          <div className="grid gap-3 sm:grid-cols-2">
            {abilityEntries.flatMap((def: any) => [...(def.evolution4 || []), ...(def.evolution7 || [])].map((evo: any) => {
              const open = unlocked.has(`ability-evolution:${evo.id}`);
              return <KnowledgeCard key={evo.id} unlocked={open} title={evo.name[lang]} desc={evo.desc[lang]} lang={lang} />;
            }))}
          </div>
        </div>
      </div>
    );
  })();

  const totalKnown = unlocked.size;
  return (
    <div className="es-list-screen h-[100dvh] w-full overflow-hidden">
      <div className="mx-auto flex h-full max-w-6xl flex-col p-3 sm:p-5">
        <header className="flex shrink-0 items-center gap-3 border-b border-cyan-300/10 pb-3">
          <button onClick={onBack} className="es-icon-button rounded-lg border border-cyan-300/15 bg-slate-950/50" aria-label={isRu ? 'Назад' : 'Back'}>
            <ArrowLeft size={17} />
          </button>
          <div className="min-w-0 flex-1">
            <div className="text-[9px] font-bold uppercase tracking-[0.28em] text-cyan-300/70">{isRu ? 'ECHO SPHERE // ARCHIVE' : 'ECHO SPHERE // ARCHIVE'}</div>
            <h1 className="mt-1 text-xl font-semibold tracking-wide text-slate-100 sm:text-2xl">{isRu ? 'Архив Эха' : 'Echo Archive'}</h1>
            <p className="text-xs text-slate-500">{isRu ? 'База знаний, которая открывается вместе с твоим прогрессом.' : 'A knowledge base that grows with your progress.'}</p>
          </div>
          <div className="hidden rounded-lg border border-cyan-300/10 bg-slate-950/40 px-3 py-2 text-right sm:block">
            <div className="text-[8px] uppercase tracking-wider text-slate-500">{isRu ? 'Открыто записей' : 'Entries unlocked'}</div>
            <div className="text-sm font-bold text-cyan-200">{totalKnown}</div>
          </div>
        </header>

        <div className="mt-3 flex min-h-0 flex-1 flex-col gap-3 md:flex-row">
          <aside className="shrink-0 overflow-x-auto md:w-56 md:overflow-y-auto">
            <div className="flex gap-2 md:grid">
              {SECTION_DEFS.map((item) => (
                <button key={item.id} onClick={() => { setSection(item.id); setQuery(''); refresh(v => v + 1); }}
                  className={`flex min-w-max items-center gap-2 rounded-lg border px-3 py-2 text-left text-xs transition ${section === item.id
                    ? 'border-cyan-300/30 bg-cyan-300/[0.06] text-cyan-100'
                    : 'border-slate-700/30 bg-slate-950/30 text-slate-500'}`}>
                  {item.icon}
                  <span>
                    <span className="block font-semibold">{text(item.label, lang)}</span>
                    <span className="hidden text-[9px] text-slate-600 md:block">{text(item.sub, lang)}</span>
                  </span>
                  <ChevronRight size={13} className="ml-auto opacity-40" />
                </button>
              ))}
            </div>
          </aside>

          <main className="min-h-0 flex-1 overflow-y-auto rounded-xl border border-cyan-300/10 bg-slate-950/25 p-3 sm:p-5">
            {section !== 'basics' && (
              <div className="mb-4 flex items-center gap-2 rounded-lg border border-cyan-300/10 bg-slate-950/35 px-3 py-2">
                <Search size={14} className="text-slate-500" />
                <input value={query} onChange={(e) => setQuery(e.target.value)} placeholder={isRu ? 'Поиск по архиву' : 'Search archive'} className="min-w-0 flex-1 border-0 bg-transparent text-xs outline-none" />
              </div>
            )}
            {content}
          </main>
        </div>
      </div>
    </div>
  );
}
