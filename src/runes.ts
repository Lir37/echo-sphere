export type RuneType =
  | 'overdrive' | 'phase' | 'harvest' | 'purge' | 'resonance'
  | 'fortify' | 'hunt' | 'echo' | 'gravity';

export interface RuneDef {
  id: RuneType;
  name: { ru: string; en: string };
  color: string;
  description: { ru: string; en: string };
}

export const RUNE_DEFS: Record<RuneType, RuneDef> = {
  overdrive: { id: 'overdrive', name: { ru: 'Руна разгона', en: 'Overdrive Rune' }, color: '#ffe26a', description: { ru: 'Мгновенно ускоряет перезарядку всех сфер.', en: 'Instantly accelerates all sphere cooldowns.' } },
  phase: { id: 'phase', name: { ru: 'Руна фазы', en: 'Phase Rune' }, color: '#8cf0ff', description: { ru: 'Даёт короткую неуязвимость.', en: 'Grants brief invulnerability.' } },
  harvest: { id: 'harvest', name: { ru: 'Руна сбора', en: 'Harvest Rune' }, color: '#63e6ff', description: { ru: 'Притягивает ближайший опыт и добавляет его.', en: 'Pulls nearby XP and grants it.' } },
  purge: { id: 'purge', name: { ru: 'Руна очищения', en: 'Purge Rune' }, color: '#ff668c', description: { ru: 'Наносит массовый импульсный урон.', en: 'Deals burst damage.' } },
  resonance: { id: 'resonance', name: { ru: 'Руна резонанса', en: 'Resonance Rune' }, color: '#b68cff', description: { ru: 'Мгновенно даёт 60 Resonance.', en: 'Instantly grants 60 Resonance.' } },
  fortify: { id: 'fortify', name: { ru: 'Руна укрепления', en: 'Fortify Rune' }, color: '#69b7ff', description: { ru: 'Даёт защитные заряды сети.', en: 'Grants network shield charges.' } },
  hunt: { id: 'hunt', name: { ru: 'Руна охоты', en: 'Hunt Rune' }, color: '#ff9c4d', description: { ru: 'Помечает ближайшего элитного врага.', en: 'Marks the nearest elite.' } },
  echo: { id: 'echo', name: { ru: 'Руна эха', en: 'Echo Rune' }, color: '#d9f7ff', description: { ru: 'Даёт временный усилитель атаки.', en: 'Grants a temporary attack boost.' } },
  gravity: { id: 'gravity', name: { ru: 'Руна гравитации', en: 'Gravity Rune' }, color: '#a67cff', description: { ru: 'Сжимает ближайших врагов.', en: 'Compresses nearby enemies.' } },
};
