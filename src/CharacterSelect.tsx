import { useMemo, useState } from 'react';
import { ArrowLeft, Check, Lock } from 'lucide-react';
import {
  CHARACTER_LIST,
  CHARACTER_UNLOCK_COST,
  type CharacterId,
} from './characters';
import {
  getCharacterMasteryNextThreshold,
  loadCharacterId,
  loadCharacterProfiles,
  saveCharacterId,
  saveCharacterProfiles,
} from './persistence';
import type { Lang } from './i18n';

interface CharacterSelectProps {
  lang: Lang;
  gold: number;
  onGoldChange: (gold: number) => void;
  onBack: () => void;
  onSelected?: (id: CharacterId) => void;
}

export default function CharacterSelect({ lang, gold, onGoldChange, onBack, onSelected }: CharacterSelectProps) {
  const [selected, setSelected] = useState<CharacterId>(() => loadCharacterId());
  const [profiles, setProfiles] = useState(() => loadCharacterProfiles());
  const [message, setMessage] = useState('');

  const profileMap = useMemo(
    () => new Map(profiles.map((profile) => [profile.id, profile])),
    [profiles],
  );

  const choose = (id: CharacterId) => {
    const profile = profileMap.get(id);
    if (!profile?.unlocked) return;
    saveCharacterId(id);
    setSelected(id);
    setMessage('');
    onSelected?.(id);
  };

  const unlock = (id: CharacterId) => {
    const profile = profileMap.get(id);
    if (!profile || profile.unlocked) {
      choose(id);
      return;
    }

    const cost = CHARACTER_UNLOCK_COST[id];
    if (gold < cost) {
      setMessage(lang === 'ru' ? 'Недостаточно золота.' : 'Not enough gold.');
      return;
    }

    const nextProfiles = profiles.map((item) =>
      item.id === id ? { ...item, unlocked: true } : item,
    );
    saveCharacterProfiles(nextProfiles);
    setProfiles(nextProfiles);
    onGoldChange(gold - cost);
    saveCharacterId(id);
    setSelected(id);
    setMessage('');
    onSelected?.(id);
  };

  return (
    <div className="relative w-full max-w-md mx-auto h-screen flex flex-col px-4 py-5 overflow-hidden">
      <div className="flex items-center justify-between mb-4 shrink-0">
        <button
          onClick={onBack}
          className="flex items-center gap-2 px-3 py-2 rounded-xl bg-[#e8dcc0] border border-[#c4b890] text-[#5a4a32]"
        >
          <ArrowLeft size={18} />
          {lang === 'ru' ? 'Назад' : 'Back'}
        </button>
        <div className="text-sm font-bold text-[#5a4a32]">
          🪙 {Math.floor(gold)}
        </div>
      </div>

      <div className="mb-4 shrink-0">
        <h2 className="text-2xl font-bold text-[#3a2e1f]">
          {lang === 'ru' ? 'Персонажи' : 'Characters'}
        </h2>
        <p className="text-xs text-[#8a7a5a] mt-1">
          {lang === 'ru'
            ? 'Персонаж меняет стиль билда, но не управление.'
            : 'Characters change your build style, not the controls.'}
        </p>
      </div>

      {message && (
        <div className="mb-3 rounded-xl border border-[#c46d3d]/40 bg-[#d4943d]/10 px-3 py-2 text-xs text-[#7a432c] shrink-0">
          {message}
        </div>
      )}

      <div className="flex-1 overflow-y-auto space-y-3 pb-4">
        {CHARACTER_LIST.map((character) => {
          const profile = profileMap.get(character.id)!;
          const unlocked = profile.unlocked;
          const active = selected === character.id;
          const cost = CHARACTER_UNLOCK_COST[character.id];
          const nextThreshold = getCharacterMasteryNextThreshold(profile.masteryLevel);
          const previousThreshold = profile.masteryLevel <= 1 ? 0 : getCharacterMasteryNextThreshold(profile.masteryLevel - 1) || 0;
          const masteryProgress = nextThreshold === null
            ? 100
            : Math.min(100, Math.max(0, ((profile.masteryXp - previousThreshold) / Math.max(1, nextThreshold - previousThreshold)) * 100));
          const masteryTitle = character.mastery.find((item) => item.level === profile.masteryLevel)?.title[lang];

          return (
            <button
              key={character.id}
              onClick={() => (unlocked ? choose(character.id) : unlock(character.id))}
              className={`w-full text-left rounded-2xl border p-4 transition ${
                active
                  ? 'border-[#4a7a8a] bg-[#4a7a8a]/10 shadow-md'
                  : 'border-[#c4b890] bg-[#e8dcc0]'
              }`}
            >
              <div className="flex items-start gap-3">
                <div
                  className="w-11 h-11 rounded-xl flex items-center justify-center shrink-0 border border-[#c4b890] bg-[#f4ecd8]"
                  style={{ color: character.color }}
                >
                  {unlocked ? <span className="text-xl">◈</span> : <Lock size={18} />}
                </div>

                <div className="min-w-0 flex-1">
                  <div className="flex items-center justify-between gap-2">
                    <div>
                      <div className="font-bold text-[#3a2e1f]">{character.name[lang]}</div>
                      <div className="text-[10px] uppercase tracking-wider text-[#8a7a5a]">
                        {character.role[lang]}
                      </div>
                    </div>
                    {active && <Check size={18} className="text-[#4a7a8a] shrink-0" />}
                  </div>

                  <p className="text-xs text-[#5a4a32] mt-2 leading-relaxed">
                    {character.description[lang]}
                  </p>

                  <div className="flex flex-wrap gap-1 mt-2">
                    {character.preferredSphereTypes.map((type) => (
                      <span key={type} className="text-[9px] px-2 py-1 rounded-full bg-[#f4ecd8] border border-[#c4b890] text-[#6b5b42]">
                        {type}
                      </span>
                    ))}
                  </div>

                  <div className="mt-2 text-[10px] text-[#8a7a5a] leading-relaxed">
                    {character.mechanic[lang]}
                  </div>

                  <div className="mt-3">
                    <div className="flex items-center justify-between gap-2 text-[10px]">
                      <span className="text-[#8a7a5a]">
                        {lang === 'ru' ? `Мастерство: ${profile.masteryLevel}/5` : `Mastery: ${profile.masteryLevel}/5`}
                        {masteryTitle ? ` · ${masteryTitle}` : ''}
                      </span>
                      <span className="font-mono text-[#6b5b42]">
                        {nextThreshold === null ? (lang === 'ru' ? 'МАКС' : 'MAX') : `${Math.floor(profile.masteryXp)}/${nextThreshold} XP`}
                      </span>
                    </div>
                    <div className="mt-1 h-1.5 rounded-full bg-[#c4b890]/70 overflow-hidden">
                      <div className="h-full rounded-full bg-[#4a7a8a] transition-all" style={{ width: `${masteryProgress}%` }} />
                    </div>
                  </div>

                  <div className="mt-3 flex items-center justify-end">
                    {unlocked ? (
                      <span className="text-xs font-bold text-[#4a7a8a]">
                        {active ? (lang === 'ru' ? 'Выбран' : 'Selected') : (lang === 'ru' ? 'Выбрать' : 'Select')}
                      </span>
                    ) : (
                      <span className="text-xs font-bold text-[#7a432c]">
                        🪙 {cost}
                      </span>
                    )}
                  </div>
                </div>
              </div>
            </button>
          );
        })}
      </div>
    </div>
  );
}
