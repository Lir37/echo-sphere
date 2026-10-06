import type { Lang } from './i18n';

export type AbilityType = 'radius' | 'damage' | 'attackspeed' | 'maxspheres' | 'blast' | 'shield' | 'teleport' | 'movespeed' | 'slow' | 'vitality' | 'firetrail' | 'minion' | 'vampire' | 'lightning' | 'dodge' | 'crit' | 'timestop' | 'magnet' | 'sphereboost' | 'darkritual' | 'armor' | 'regen' | 'resonance' | 'cooldown' | 'critpower' | 'range' | 'proj_speed' | 'momentum' | 'fortune' | 'stability';
export type AbilityCategory = 'active' | 'passive';

export interface AbilityDef {
  id: AbilityType;
  category: AbilityCategory;
  maxLevel: number;
  key?: string; // hotkey for active
  name: { ru: string; en: string };
  desc: { ru: (lvl: number) => string; en: (lvl: number) => string };
}

export const ABILITIES: Record<AbilityType, AbilityDef> = {
  radius: { id: 'radius', category: 'passive', maxLevel: 7, name: { ru: 'Радиус сфер', en: 'Sphere Radius' }, desc: { ru: () => '+15% радиуса', en: () => '+15% radius' } },
  damage: { id: 'damage', category: 'passive', maxLevel: 7, name: { ru: 'Урон сфер', en: 'Sphere Damage' }, desc: { ru: () => '+15% урона', en: () => '+15% damage' } },
  attackspeed: { id: 'attackspeed', category: 'passive', maxLevel: 7, name: { ru: 'Скорость атаки', en: 'Attack Speed' }, desc: { ru: () => '-7% задержки', en: () => '-7% delay' } },
  maxspheres: { id: 'maxspheres', category: 'passive', maxLevel: 3, name: { ru: '+1 Сфера', en: '+1 Sphere' }, desc: { ru: () => '+1 макс. сфер', en: () => '+1 max spheres' } },
  blast: { id: 'blast', category: 'active', maxLevel: 7, key: 'e', name: { ru: 'Эхо-импульс', en: 'Echo Pulse' }, desc: { ru: (l) => `Импульс проходит через сферы, КД ${Math.max(8, 30 - (l - 1) * 2)}с`, en: (l) => `Pulse travels through spheres, CD ${Math.max(8, 30 - (l - 1) * 2)}s` } },
  shield: { id: 'shield', category: 'active', maxLevel: 7, key: 'q', name: { ru: 'Сферный барьер', en: 'Sphere Barrier' }, desc: { ru: (l) => `Сферы создают защитный контур, КД 20с`, en: (l) => `Spheres create a defensive network, CD 20s` } },
  teleport: { id: 'teleport', category: 'active', maxLevel: 7, key: 'r', name: { ru: 'Эхо-прыжок', en: 'Echo Jump' }, desc: { ru: (l) => `Прыжок к сфере, КД ${Math.max(5, 15 - Math.min(4, (l - 1) * 2))}с`, en: (l) => `Jump to a sphere, CD ${Math.max(5, 15 - Math.min(4, (l - 1) * 2))}s` } },
  movespeed: { id: 'movespeed', category: 'passive', maxLevel: 7, name: { ru: 'Скорость движения', en: 'Move Speed' }, desc: { ru: () => '+10% скорости', en: () => '+10% speed' } },
  slow: { id: 'slow', category: 'passive', maxLevel: 7, name: { ru: 'Замедление врагов', en: 'Enemy Slow' }, desc: { ru: (l) => `-${10 + (l - 1) * 5}% скорости врагов`, en: (l) => `-${10 + (l - 1) * 5}% enemy speed` } },
  vitality: { id: 'vitality', category: 'passive', maxLevel: 7, name: { ru: 'Живучесть', en: 'Vitality' }, desc: { ru: () => '+20 макс. HP', en: () => '+20 max HP' } },
  firetrail: { id: 'firetrail', category: 'active', maxLevel: 7, key: 'f', name: { ru: 'Перегрев', en: 'Overheat' }, desc: { ru: (l) => `Перегружает сеть на ${5 + Math.min(3, l - 1)}с, КД 25с`, en: (l) => `Overheats the network for ${5 + Math.min(3, l - 1)}s, CD 25s` } },
  minion: { id: 'minion', category: 'active', maxLevel: 7, key: 'g', name: { ru: 'Эхо-дрон', en: 'Echo Drone' }, desc: { ru: (l) => `${1 + Math.floor((l - 1) / 2)} временных узлов, 10с, КД 30с`, en: (l) => `${1 + Math.floor((l - 1) / 2)} temporary nodes, 10s, CD 30s` } },
  vampire: { id: 'vampire', category: 'passive', maxLevel: 7, name: { ru: 'Вампиризм', en: 'Vampirism' }, desc: { ru: (l) => `${l * 3}% урона -> HP`, en: (l) => `${l * 3}% damage -> HP` } },
  lightning: { id: 'lightning', category: 'active', maxLevel: 7, key: 'e', name: { ru: 'Цепной разряд', en: 'Chain Lightning' }, desc: { ru: (l) => `Разряд проходит по сети сфер к цели, КД 20с`, en: (l) => `Lightning travels through the Sphere Network to its target, CD 20s` } },
  dodge: { id: 'dodge', category: 'passive', maxLevel: 7, name: { ru: 'Уклонение', en: 'Dodge' }, desc: { ru: (l) => `${10 + (l - 1) * 5}% шанс`, en: (l) => `${10 + (l - 1) * 5}% chance` } },
  crit: { id: 'crit', category: 'passive', maxLevel: 7, name: { ru: 'Критический урон', en: 'Critical Hit' }, desc: { ru: (l) => `${10 + (l - 1) * 5}% шанс x2`, en: (l) => `${10 + (l - 1) * 5}% chance x2` } },
  timestop: { id: 'timestop', category: 'active', maxLevel: 7, key: 'q', name: { ru: 'Эхо-заморозка', en: 'Echo Freeze' }, desc: { ru: (l) => `Сеть останавливает врагов на ${3 + Math.min(2, l - 1)}с, КД 40с`, en: (l) => `Network freezes enemies for ${3 + Math.min(2, l - 1)}s, CD 40s` } },
  magnet: { id: 'magnet', category: 'passive', maxLevel: 7, name: { ru: 'Магнит опыта', en: 'XP Magnet' }, desc: { ru: () => '+20% радиус подбора', en: () => '+20% pickup radius' } },
  sphereboost: { id: 'sphereboost', category: 'passive', maxLevel: 7, name: { ru: 'Усиление сфер', en: 'Sphere Boost' }, desc: { ru: (l) => `+1 урон за ${Math.max(50, 100 - (l - 1) * 10)} убийств`, en: (l) => `+1 dmg per ${Math.max(50, 100 - (l - 1) * 10)} kills` } },
  darkritual: { id: 'darkritual', category: 'active', maxLevel: 7, key: 'r', name: { ru: 'Перегрузка', en: 'Overload' }, desc: { ru: (l) => `-20% HP, перегружает всю сеть, КД 30с`, en: (l) => `-20% HP, overloads the network, CD 30s` } },
  armor: { id: 'armor', category: 'passive', maxLevel: 7, name: { ru: 'Стабильный корпус', en: 'Stable Hull' }, desc: { ru: (l) => `-${l * 4}% получаемого урона`, en: (l) => `-${l * 4}% damage taken` } },
  regen: { id: 'regen', category: 'passive', maxLevel: 7, name: { ru: 'Регенерация', en: 'Regeneration' }, desc: { ru: (l) => `+${l * 0.6} HP/с`, en: (l) => `+${l * 0.6} HP/sec` } },
  resonance: { id: 'resonance', category: 'passive', maxLevel: 7, name: { ru: 'Резонатор', en: 'Resonator' }, desc: { ru: (l) => `+${l * 8}% к получению Резонанса`, en: (l) => `+${l * 8}% Resonance gain` } },
  cooldown: { id: 'cooldown', category: 'passive', maxLevel: 7, name: { ru: 'Хроно-ядро', en: 'Chrono Core' }, desc: { ru: (l) => `-${l * 4}% перезарядки`, en: (l) => `-${l * 4}% cooldowns` } },
  critpower: { id: 'critpower', category: 'passive', maxLevel: 7, name: { ru: 'Критический резонанс', en: 'Critical Resonance' }, desc: { ru: (l) => `+${l * 8}% критического множителя`, en: (l) => `+${l * 8}% critical multiplier` } },
  range: { id: 'range', category: 'passive', maxLevel: 7, name: { ru: 'Фокус поля', en: 'Field Focus' }, desc: { ru: (l) => `+${l * 5}% радиуса/дальности сфер`, en: (l) => `+${l * 5}% Sphere range` } },
  proj_speed: { id: 'proj_speed', category: 'passive', maxLevel: 7, name: { ru: 'Импульсный вектор', en: 'Vector Drive' }, desc: { ru: (l) => `+${l * 8}% скорости снарядов`, en: (l) => `+${l * 8}% projectile speed` } },
  momentum: { id: 'momentum', category: 'passive', maxLevel: 7, name: { ru: 'Импульс', en: 'Momentum' }, desc: { ru: (l) => `+${l * 5}% скорости после Dash`, en: (l) => `+${l * 5}% speed after Dash` } },
  fortune: { id: 'fortune', category: 'passive', maxLevel: 7, name: { ru: 'Удача', en: 'Fortune' }, desc: { ru: (l) => `+${l * 2}% к шансам редких выпадений`, en: (l) => `+${l * 2}% rare drop chance` } },
  stability: { id: 'stability', category: 'passive', maxLevel: 7, name: { ru: 'Стабильная сеть', en: 'Network Stability' }, desc: { ru: (l) => `-${l * 12}% времени сетевого отключения`, en: (l) => `-${l * 12}% network-disable duration` } },
};

// Active ability hotkey assignment (order of acquisition). Keys: e, q, r, f, g
export const ACTIVE_KEYS = ['e', 'q', 'r', 'f', 'g'] as const;

export type ArtifactId = 'crystal_speed' | 'amulet_hp' | 'ring_xp' | 'regen_stone' | 'radius_shard' | 'vampire_ring' | 'swift_boots' | 'luck_talisman' | 'mage_pendant' | 'long_lens' | 'stasis_core' | 'dragon_heart' | 'mirror' | 'predator_claw' | 'foresight_eye' | 'echo_conductor' | 'heavy_core' | 'scattering_matrix' | 'aura_lens' | 'network_relay' | 'chaos_orb' | 'resonance_core' | 'lone_bastion' | 'fivefold_resonance' | 'relay_matrix' | 'triangle_circuit' | 'overclock' | 'soul_engine' | 'time_anchor' | 'void_contract' | 'mirror_network' | 'singularity_engine' | 'quantum_core' | 'zero_sphere' | 'unified_mind' | 'network_anchor' | 'pulse_lens' | 'orbit_charm' | 'prism_shard' | 'gravity_bead' | 'void_ink' | 'echo_thread' | 'folded_core' | 'paper_ward' | 'mirror_dust' | 'resonant_leaf' | 'signal_knot' | 'lattice_chip' | 'fractal_seed' | 'sniper_scope' | 'chain_battery' | 'shotgun_shell' | 'aura_mist' | 'orbital_blade' | 'prism_filter' | 'gravity_hook' | 'pulse_driver' | 'void_mark' | 'formation_compass' | 'geometry_die' | 'network_coil' | 'crit_sigil' | 'tempo_ring' | 'triangle_engine' | 'ring_engine' | 'lattice_engine' | 'fractal_engine' | 'resonance_lattice' | 'echo_weaver' | 'gravity_crown' | 'void_lantern' | 'prism_crown' | 'orbital_crown' | 'pulse_crown' | 'chain_crown' | 'sniper_crown' | 'time_splitter' | 'stasis_mandala' | 'quantum_fold' | 'void_star' | 'axiom_core' | 'universal_fold';

export interface ArtifactDef {
  id: ArtifactId;
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
}

export const ARTIFACTS: ArtifactDef[] = [
  // Common
  { id: 'crystal_speed', name: { ru: 'Кристалл скорости', en: 'Crystal of Speed' }, desc: { ru: '+15% скорости движения', en: '+15% move speed' } },
  { id: 'amulet_hp', name: { ru: 'Амулет здоровья', en: 'Health Amulet' }, desc: { ru: '+30 макс. HP', en: '+30 max HP' } },
  { id: 'ring_xp', name: { ru: 'Кольцо опыта', en: 'XP Ring' }, desc: { ru: '+20% получаемого опыта', en: '+20% XP gained' } },
  { id: 'regen_stone', name: { ru: 'Камень восстановления', en: 'Regen Stone' }, desc: { ru: '+1 HP/сек', en: '+1 HP/sec' } },
  { id: 'radius_shard', name: { ru: 'Осколок радиуса', en: 'Radius Shard' }, desc: { ru: '+10% радиуса сфер', en: '+10% sphere radius' } },
  { id: 'vampire_ring', name: { ru: 'Кольцо вампира', en: 'Vampire Ring' }, desc: { ru: '+3% вампиризма', en: '+3% lifesteal' } },
  { id: 'swift_boots', name: { ru: 'Ботинки скорости', en: 'Swift Boots' }, desc: { ru: 'после убийства +10% скорости на 3с', en: 'kills grant +10% speed for 3s' } },
  { id: 'luck_talisman', name: { ru: 'Талисман удачи', en: 'Luck Talisman' }, desc: { ru: '+15% шанс крита', en: '+15% crit chance' } },
  { id: 'mage_pendant', name: { ru: 'Кулон мага', en: 'Mage Pendant' }, desc: { ru: '-10% перезарядки способностей', en: '-10% ability cooldowns' } },
  { id: 'long_lens', name: { ru: 'Дальняя линза', en: 'Long Lens' }, desc: { ru: '+18% радиуса снайперской сферы', en: '+18% sniper range' } },
  { id: 'stasis_core', name: { ru: 'Ядро стазиса', en: 'Stasis Core' }, desc: { ru: 'при получении урона замедляет всех врагов на 2 секунды', en: 'taking damage slows all enemies for 2s' } },

  // Rare
  { id: 'dragon_heart', name: { ru: 'Сердце дракона', en: 'Dragon Heart' }, desc: { ru: '+50 HP, -10% скорости', en: '+50 HP, -10% speed' } },
  { id: 'mirror', name: { ru: 'Зеркало', en: 'Mirror' }, desc: { ru: '20% полученного урона отражается ближайшему врагу', en: '20% of incoming damage is reflected' } },
  { id: 'predator_claw', name: { ru: 'Коготь хищника', en: 'Predator Claw' }, desc: { ru: '+8% урона сфер', en: '+8% sphere damage' } },
  { id: 'foresight_eye', name: { ru: 'Око предвидения', en: 'Foresight Eye' }, desc: { ru: '+10% крита и +8% урона снайперской сферы', en: '+10% crit and +8% sniper damage' } },
  { id: 'echo_conductor', name: { ru: 'Проводник Эха', en: 'Echo Conductor' }, desc: { ru: '+12% урона цепной сферы', en: '+12% chain sphere damage' } },
  { id: 'heavy_core', name: { ru: 'Тяжёлое ядро', en: 'Heavy Core' }, desc: { ru: '+15% урона стандартной сферы', en: '+15% standard sphere damage' } },
  { id: 'scattering_matrix', name: { ru: 'Матрица рассеивания', en: 'Scattering Matrix' }, desc: { ru: '+12% урона дробовика', en: '+12% shotgun damage' } },
  { id: 'aura_lens', name: { ru: 'Линза ауры', en: 'Aura Lens' }, desc: { ru: '+18% радиуса ауры', en: '+18% aura radius' } },
  { id: 'network_relay', name: { ru: 'Сетевой реле', en: 'Network Relay' }, desc: { ru: 'при 2+ типах сфер +5% урона сети', en: 'with 2+ sphere types, +5% network damage' } },
  { id: 'chaos_orb', name: { ru: 'Матрица стабильности', en: 'Stability Matrix' }, desc: { ru: 'каждые 10 секунд даёт случайный бонус урона или радиуса на 3 секунды', en: 'every 10s grants a random damage or radius buff for 3s' } },

  // Epic
  { id: 'resonance_core', name: { ru: 'Сердце резонанса', en: 'Resonance Core' }, desc: { ru: 'сфера рядом с другой сферой получает +12% урона', en: 'nearby spheres deal +12% damage' } },
  { id: 'lone_bastion', name: { ru: 'Одинокий бастион', en: 'Lone Bastion' }, desc: { ru: 'если активен только один тип сферы, он получает +30% урона', en: 'with one sphere type, it gains +30% damage' } },
  { id: 'fivefold_resonance', name: { ru: 'Резонанс пяти', en: 'Fivefold Resonance' }, desc: { ru: 'каждый уникальный тип сферы даёт +4% урона сети', en: 'each unique sphere type grants +4% network damage' } },
  { id: 'relay_matrix', name: { ru: 'Релейная матрица', en: 'Relay Matrix' }, desc: { ru: 'наличие сферы VII уровня усиливает остальные на +10%', en: 'a level VII sphere empowers the network by +10%' } },
  { id: 'triangle_circuit', name: { ru: 'Треугольный контур', en: 'Triangle Circuit' }, desc: { ru: 'сферы внутри треугольной сети получают +15% урона', en: 'spheres in a triangular network deal +15% damage' } },
  { id: 'overclock', name: { ru: 'Разгон ядра', en: 'Overclock' }, desc: { ru: '-12% задержки сфер, но небольшая потеря эффективности', en: '-12% sphere delay with a small efficiency cost' } },
  { id: 'soul_engine', name: { ru: 'Двигатель душ', en: 'Soul Engine' }, desc: { ru: '+5% урона сфер и +10% опыта', en: '+5% sphere damage and +10% XP' } },
  { id: 'time_anchor', name: { ru: 'Якорь времени', en: 'Time Anchor' }, desc: { ru: '-12% перезарядки способностей', en: '-12% ability cooldowns' } },

  // Special
  { id: 'void_contract', name: { ru: 'Контракт пустоты', en: 'Void Contract' }, desc: { ru: '+22% урона сфер, но +15% получаемого урона', en: '+22% Sphere damage, but +15% damage taken' } },
  { id: 'mirror_network', name: { ru: 'Зеркальная сеть', en: 'Mirror Network' }, desc: { ru: 'две и более сферы дают сети +12% урона', en: 'two or more sphere types grant +12% network damage' } },
  { id: 'singularity_engine', name: { ru: 'Двигатель сингулярности', en: 'Singularity Engine' }, desc: { ru: 'билд из 1–2 типов сфер получает ещё +18% урона', en: 'a 1–2 sphere-type build gains +18% damage' } },
  { id: 'quantum_core', name: { ru: 'Квантовое ядро', en: 'Quantum Core' }, desc: { ru: '+8% урона сфер и +8% уклонения', en: '+8% sphere damage and +8% dodge' } },

  // Legendary
  { id: 'zero_sphere', name: { ru: 'Нулевая сфера', en: 'Zero Sphere' }, desc: { ru: '+35% урона сфер', en: '+35% Sphere damage' } },
  { id: 'unified_mind', name: { ru: 'Единый разум', en: 'Unified Mind' }, desc: { ru: 'самый высокий уровень сферы передаёт 3% за уровень всей сети', en: 'the highest sphere level grants 3% damage per level' } },
  { id: 'network_anchor', name: { ru: 'Якорь сети', en: 'Network Anchor' }, desc: { ru: '+4% урона сфер', en: '+4% Sphere damage' } },
  { id: 'pulse_lens', name: { ru: 'Линза импульса', en: 'Pulse Lens' }, desc: { ru: '+10% к радиусу импульсов', en: '+10% Pulse radius' } },
  { id: 'orbit_charm', name: { ru: 'Талисман орбиты', en: 'Orbit Charm' }, desc: { ru: '+12% к силе орбит', en: '+12% Orbital strength' } },
  { id: 'prism_shard', name: { ru: 'Осколок призмы', en: 'Prism Shard' }, desc: { ru: '+10% к урону Призмы', en: '+10% Prism damage' } },
  { id: 'gravity_bead', name: { ru: 'Гравитационная бусина', en: 'Gravity Bead' }, desc: { ru: '+12% к силе гравитации', en: '+12% Gravity pull strength' } },
  { id: 'void_ink', name: { ru: 'Чернила пустоты', en: 'Void Ink' }, desc: { ru: '+10% к урону Пустоты', en: '+10% Void damage' } },
  { id: 'echo_thread', name: { ru: 'Нить Эха', en: 'Echo Thread' }, desc: { ru: '+5% к урону связанных Сфер', en: '+5% damage from linked Spheres' } },
  { id: 'folded_core', name: { ru: 'Складное ядро', en: 'Folded Core' }, desc: { ru: '+8% к урону всех Сфер', en: '+8% damage for all Spheres' } },
  { id: 'paper_ward', name: { ru: 'Бумажный щит', en: 'Paper Ward' }, desc: { ru: '+4% урона сфер', en: '+4% Sphere damage' } },
  { id: 'mirror_dust', name: { ru: 'Зеркальная пыль', en: 'Mirror Dust' }, desc: { ru: '+8% к шансу отражения', en: '+8% reflection chance' } },
  { id: 'resonant_leaf', name: { ru: 'Резонансный лист', en: 'Resonant Leaf' }, desc: { ru: '+10% к Резонансу', en: '+10% Resonance effects' } },
  { id: 'signal_knot', name: { ru: 'Сигнальный узел', en: 'Signal Knot' }, desc: { ru: '+5% скорости восстановления сети', en: '+5% Network recovery speed' } },
  { id: 'lattice_chip', name: { ru: 'Чип решётки', en: 'Lattice Chip' }, desc: { ru: '+10% силы Решётки', en: '+10% Lattice strength' } },
  { id: 'fractal_seed', name: { ru: 'Фрактальное семя', en: 'Fractal Seed' }, desc: { ru: '+10% силы Фрактала', en: '+10% Fractal strength' } },
  { id: 'sniper_scope', name: { ru: 'Прицел охотника', en: 'Sniper Scope' }, desc: { ru: '+10% урона Снайпера', en: '+10% Sniper damage' } },
  { id: 'chain_battery', name: { ru: 'Цепной аккумулятор', en: 'Chain Battery' }, desc: { ru: '+10% урона Цепи', en: '+10% Chain damage' } },
  { id: 'shotgun_shell', name: { ru: 'Дробь усиления', en: 'Shotgun Shell' }, desc: { ru: '+10% урона Дробовика', en: '+10% Shotgun damage' } },
  { id: 'aura_mist', name: { ru: 'Туман ауры', en: 'Aura Mist' }, desc: { ru: '+10% радиуса Ауры', en: '+10% Aura radius' } },
  { id: 'orbital_blade', name: { ru: 'Орбитальный клинок', en: 'Orbital Blade' }, desc: { ru: '+10% урона Орбитальной', en: '+10% Orbital damage' } },
  { id: 'prism_filter', name: { ru: 'Призматический фильтр', en: 'Prism Filter' }, desc: { ru: '+10% пробития Призмы', en: '+10% Prism pierce' } },
  { id: 'gravity_hook', name: { ru: 'Гравитационный крюк', en: 'Gravity Hook' }, desc: { ru: '+10% контроля Гравитационной', en: '+10% Gravity control' } },
  { id: 'pulse_driver', name: { ru: 'Импульсный драйвер', en: 'Pulse Driver' }, desc: { ru: '+10% частоты Импульсной', en: '+10% Pulse frequency' } },
  { id: 'void_mark', name: { ru: 'Метка пустоты', en: 'Void Mark' }, desc: { ru: '+10% урона по ослабленным', en: '+10% damage to weakened targets' } },
  { id: 'formation_compass', name: { ru: 'Компас формации', en: 'Formation Compass' }, desc: { ru: 'увеличивает радиус сфер на 8%', en: 'increases Sphere radius by 8%' } },
  { id: 'geometry_die', name: { ru: 'Куб геометрии', en: 'Geometry Die' }, desc: { ru: '+8% урона сфер во время активной Геометрии', en: 'increases Sphere damage by 8% during active Geometry' } },
  { id: 'network_coil', name: { ru: 'Сетевая катушка', en: 'Network Coil' }, desc: { ru: '+8% урона при 2+ типах сфер', en: '+8% damage with 2+ Sphere types' } },
  { id: 'crit_sigil', name: { ru: 'Критическая печать', en: 'Crit Sigil' }, desc: { ru: '+5% шанса критического удара', en: '+5% critical chance' } },
  { id: 'tempo_ring', name: { ru: 'Кольцо темпа', en: 'Tempo Ring' }, desc: { ru: '-8% задержки сфер', en: '-8% Sphere attack delay' } },
  { id: 'triangle_engine', name: { ru: 'Двигатель треугольника', en: 'Triangle Engine' }, desc: { ru: '+12% силы Треугольника', en: '+12% Triangle strength' } },
  { id: 'ring_engine', name: { ru: 'Двигатель кольца', en: 'Ring Engine' }, desc: { ru: '+12% силы Кольца', en: '+12% Ring strength' } },
  { id: 'lattice_engine', name: { ru: 'Двигатель решётки', en: 'Lattice Engine' }, desc: { ru: '+12% силы Решётки', en: '+12% Lattice strength' } },
  { id: 'fractal_engine', name: { ru: 'Двигатель фрактала', en: 'Fractal Engine' }, desc: { ru: '+12% силы Фрактала', en: '+12% Fractal strength' } },
  { id: 'resonance_lattice', name: { ru: 'Резонансная решётка', en: 'Resonance Lattice' }, desc: { ru: '+15% эффектов Резонанса', en: '+15% Resonance effects' } },
  { id: 'echo_weaver', name: { ru: 'Ткач Эха', en: 'Echo Weaver' }, desc: { ru: 'каждая связь даёт небольшой бонус', en: 'Each active Network link grants a small bonus' } },
  { id: 'gravity_crown', name: { ru: 'Корона гравитации', en: 'Gravity Crown' }, desc: { ru: 'Гравитационная получает +20% радиуса', en: 'Gravity gains +20% radius' } },
  { id: 'void_lantern', name: { ru: 'Фонарь пустоты', en: 'Void Lantern' }, desc: { ru: 'Пустотная получает +20% урона добивания', en: 'Void gains +20% execution damage' } },
  { id: 'prism_crown', name: { ru: 'Корона призмы', en: 'Prism Crown' }, desc: { ru: 'Призма получает +1 отражение', en: 'Prism gains +1 reflection' } },
  { id: 'orbital_crown', name: { ru: 'Корона орбиты', en: 'Orbital Crown' }, desc: { ru: 'Орбитальная получает +1 спутник', en: 'Orbital gains +1 satellite' } },
  { id: 'pulse_crown', name: { ru: 'Корона импульса', en: 'Pulse Crown' }, desc: { ru: 'Импульсная получает +1 волну', en: 'Pulse gains +1 wave' } },
  { id: 'chain_crown', name: { ru: 'Корона цепи', en: 'Chain Crown' }, desc: { ru: 'Цепная получает +1 перескок', en: 'Chain gains +1 jump' } },
  { id: 'sniper_crown', name: { ru: 'Корона снайпера', en: 'Sniper Crown' }, desc: { ru: 'Снайперская получает +10% дальности', en: 'Sniper gains +10% range' } },
  { id: 'time_splitter', name: { ru: 'Разделитель времени', en: 'Time Splitter' }, desc: { ru: 'после временной остановки +15% урона', en: '+15% damage after Time Stop' } },
  { id: 'stasis_mandala', name: { ru: 'Мандала стазиса', en: 'stasis mandala' }, desc: { ru: 'полученный урон частично превращается в щит', en: 'Part of damage taken becomes a shield charge' } },
  { id: 'quantum_fold', name: { ru: 'Квантовый сгиб', en: 'quantum fold' }, desc: { ru: 'Стелла появляется с дополнительным выбором', en: 'Stella rewards gain one additional choice' } },
  { id: 'void_star', name: { ru: 'Звезда пустоты', en: 'void star' }, desc: { ru: 'Пустота получает дополнительный шанс добивания', en: 'Void gains an additional execution chance' } },
  { id: 'axiom_core', name: { ru: 'Аксиома ядра', en: 'axiom core' }, desc: { ru: '+5% ко всем ключевым эффектам', en: '+5% to key combat effects' } },
  { id: 'universal_fold', name: { ru: 'Универсальный сгиб', en: 'universal fold' }, desc: { ru: 'все собранные системы получают небольшой синергетический бонус', en: 'All assembled systems gain a small synergy bonus' } }
];

export const ARTIFACT_MAP: Record<ArtifactId, ArtifactDef> = Object.fromEntries(ARTIFACTS.map(a => [a.id, a])) as Record<ArtifactId, ArtifactDef>;

export interface ShopUpgradeDef {
  id: 'dmg' | 'radius' | 'speed' | 'spheres' | 'hp' | 'crit' | 'xp';
  name: { ru: string; en: string };
  desc: { ru: (lvl: number) => string; en: (lvl: number) => string };
  baseCost: number;
  maxLevel: number;
}

export const SHOP_UPGRADES: ShopUpgradeDef[] = [
  { id: 'dmg', name: { ru: 'Урон сфер', en: 'Sphere Damage' }, desc: { ru: () => '+5% урона', en: () => '+5% damage' }, baseCost: 200, maxLevel: 10 },
  { id: 'radius', name: { ru: 'Радиус сфер', en: 'Sphere Radius' }, desc: { ru: () => '+5% радиуса', en: () => '+5% radius' }, baseCost: 200, maxLevel: 10 },
  { id: 'speed', name: { ru: 'Скорость движения', en: 'Move Speed' }, desc: { ru: () => '+5% скорости', en: () => '+5% speed' }, baseCost: 300, maxLevel: 7 },
  { id: 'spheres', name: { ru: 'Стартовые сферы', en: 'Start Spheres' }, desc: { ru: () => '+1 сфера', en: () => '+1 sphere' }, baseCost: 500, maxLevel: 3 },
  { id: 'hp', name: { ru: 'Стартовое HP', en: 'Start HP' }, desc: { ru: () => '+10 HP', en: () => '+10 HP' }, baseCost: 150, maxLevel: 10 },
  { id: 'crit', name: { ru: 'Шанс крита', en: 'Crit Chance' }, desc: { ru: () => '+5% шанс крита', en: () => '+5% crit chance' }, baseCost: 400, maxLevel: 7 },
  { id: 'xp', name: { ru: 'Получаемый опыт', en: 'XP Gain' }, desc: { ru: () => '+5% опыта', en: () => '+5% XP' }, baseCost: 300, maxLevel: 7 },
];

export function shopCost(def: ShopUpgradeDef, currentLevel: number): number {
  return Math.floor(def.baseCost * Math.pow(3, currentLevel));
}

// ===== Sphere Types =====
export type SphereType =
  | 'standard' | 'sniper' | 'shotgun' | 'chain' | 'aura'
  | 'orbital' | 'prism' | 'gravity' | 'pulse' | 'void';

export type SphereTargetingRule = 'nearest' | 'high_value_far' | 'area_control' | 'highest_hp' | 'lowest_hp';

export const PROJECTILE_SPHERE_TYPES: readonly SphereType[] = ['standard', 'sniper', 'shotgun', 'prism', 'void'];

export function sphereUsesProjectileModifiers(type: SphereType): boolean {
  return PROJECTILE_SPHERE_TYPES.includes(type);
}

export interface SphereTypeDef {
  id: SphereType;
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
  targetingRule: SphereTargetingRule;
  color: string;
  damageMult: number;
  rangeMult: number;
  delayMult: number;
  projectileSpeedMult: number;
  pellets: number; // shots per fire
  spread: number; // radians
  chain: boolean; // chains to nearby enemies
  aura: boolean; // continuous AoE damage
  auraRadius: number;
}

export const SPHERE_TYPES: Record<SphereType, SphereTypeDef> = {
  standard: {
    id: 'standard', name: { ru: 'Стандартная', en: 'Standard' },
    desc: { ru: 'Сбалансированная сфера', en: 'Balanced sphere' },
    targetingRule: 'nearest',
    color: '#55dfff', damageMult: 1, rangeMult: 1, delayMult: 1, projectileSpeedMult: 1,
    pellets: 1, spread: 0, chain: false, aura: false, auraRadius: 0,
  },
  sniper: {
    id: 'sniper', name: { ru: 'Снайпер', en: 'Sniper' },
    desc: { ru: 'Высокий урон, большая дальность, медленная', en: 'High damage, long range, slow' },
    targetingRule: 'high_value_far',
    color: '#e86cff', damageMult: 2.5, rangeMult: 2, delayMult: 2, projectileSpeedMult: 2,
    pellets: 1, spread: 0, chain: false, aura: false, auraRadius: 0,
  },
  shotgun: {
    id: 'shotgun', name: { ru: 'Дробовик', en: 'Shotgun' },
    desc: { ru: '3 снаряда, короткая дальность', en: '3 pellets, short range' },
    targetingRule: 'nearest',
    color: '#ff8f3d', damageMult: 0.6, rangeMult: 0.6, delayMult: 1.2, projectileSpeedMult: 0.8,
    pellets: 3, spread: 0.4, chain: false, aura: false, auraRadius: 0,
  },
  chain: {
    id: 'chain', name: { ru: 'Цепная', en: 'Chain' },
    desc: { ru: 'Молния прыгает между врагами', en: 'Lightning jumps between enemies' },
    targetingRule: 'nearest',
    color: '#ffe25b', damageMult: 1, rangeMult: 1, delayMult: 1.3, projectileSpeedMult: 1.5,
    pellets: 1, spread: 0, chain: true, aura: false, auraRadius: 0,
  },
  aura: {
    id: 'aura', name: { ru: 'Аура', en: 'Aura' },
    desc: { ru: 'Непрерывный урон по площади', en: 'Continuous AoE damage' },
    targetingRule: 'nearest',
    color: '#57e6b4', damageMult: 0.60, rangeMult: 0.5, delayMult: 0.2, projectileSpeedMult: 1,
    pellets: 0, spread: 0, chain: false, aura: true, auraRadius: 80,
  },
  orbital: {
    id: 'orbital', name: { ru: 'Орбитальная', en: 'Orbital' },
    desc: { ru: 'Боевые элементы режут врагов на двух орбитах: внутренняя вращается по часовой, внешняя против часовой. Уровень I начинается с 1+1, затем добавления чередуются до 4+4 на VII.', en: 'Combat elements cut enemies on two orbits: the inner rotates clockwise and the outer counter-clockwise. Level I starts at 1+1, then additions alternate to 4+4 at VII.' },
    targetingRule: 'area_control', color: '#8ef0ff', damageMult: 0.72, rangeMult: 1, delayMult: 1, projectileSpeedMult: 1,
    pellets: 0, spread: 0, chain: false, aura: false, auraRadius: 105,
  },
  prism: {
    id: 'prism', name: { ru: 'Призма', en: 'Prism' },
    desc: { ru: 'Луч разделяется на дополнительные направления', en: 'A beam splits into additional directions' },
    targetingRule: 'highest_hp', color: '#ff8de1', damageMult: 1.25, rangeMult: 1.35, delayMult: 1.35, projectileSpeedMult: 2,
    pellets: 3, spread: 0.16, chain: false, aura: false, auraRadius: 0,
  },
  gravity: {
    id: 'gravity', name: { ru: 'Гравитационная', en: 'Gravity' },
    desc: { ru: 'Стягивает врагов к центру и наносит импульсный урон', en: 'Pulls enemies inward and pulses damage' },
    targetingRule: 'area_control', color: '#a58cff', damageMult: 0.42, rangeMult: 1.1, delayMult: 0.9, projectileSpeedMult: 1,
    pellets: 0, spread: 0, chain: false, aura: true, auraRadius: 125,
  },
  pulse: {
    id: 'pulse', name: { ru: 'Импульсная', en: 'Pulse' },
    desc: { ru: 'Периодические волны урона вокруг узла', en: 'Periodic damage waves around the node' },
    targetingRule: 'area_control', color: '#ffd35a', damageMult: 1, rangeMult: 1.15, delayMult: 1, projectileSpeedMult: 1,
    pellets: 0, spread: 0, chain: false, aura: false, auraRadius: 115,
  },
  void: {
    id: 'void', name: { ru: 'Пустотная', en: 'Void' },
    desc: { ru: 'Добивает ослабленных врагов и разрывает плотные группы', en: 'Executes weakened enemies and ruptures dense groups' },
    targetingRule: 'lowest_hp', color: '#c28cff', damageMult: 1.15, rangeMult: 1.05, delayMult: 1.2, projectileSpeedMult: 1.3,
    pellets: 1, spread: 0, chain: false, aura: false, auraRadius: 0,
  },
};

// ===== Boss Types =====
export type BossType = 'shooter' | 'charger' | 'summoner' | 'aura' | 'conductor' | 'architect' | 'null' | 'stella_warden';

export interface BossTypeDef {
  id: BossType;
  name: { ru: string; en: string };
  color: string;
}

export const BOSS_TYPES: Record<BossType, BossTypeDef> = {
  shooter: { id: 'shooter', name: { ru: 'Стрелок', en: 'Shooter' }, color: '#ff6a5f' },
  charger: { id: 'charger', name: { ru: 'Зарядник', en: 'Charger' }, color: '#ff9c3d' },
  summoner: { id: 'summoner', name: { ru: 'Призыватель', en: 'Summoner' }, color: '#a27cff' },
  aura: { id: 'aura', name: { ru: 'Аура', en: 'Aura' }, color: '#ff62b9' },
  conductor: { id: 'conductor', name: { ru: 'Кондуктор', en: 'Conductor' }, color: '#4fd8ff' },
  architect: { id: 'architect', name: { ru: 'Архитектор', en: 'Architect' }, color: '#ffc56a' },
  null: { id: 'null', name: { ru: 'Нуль', en: 'Null' }, color: '#9b7cff' },
  stella_warden: { id: 'stella_warden', name: { ru: 'Страж Стеллы', en: 'Stella Warden' }, color: '#ffd15a' },
};

// ===== Difficulty =====
export type Difficulty = 'easy' | 'normal' | 'hard' | 'nightmare';

export interface DifficultyDef {
  id: Difficulty;
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
  enemyHpMult: number;
  enemySpeedMult: number;
  enemyDamageMult: number;
  goldMult: number;
  spawnRateMult: number;
}

export const DIFFICULTIES: DifficultyDef[] = [
  { id: 'easy', name: { ru: 'Лёгкая', en: 'Easy' }, desc: { ru: 'Медленные враги, меньше HP', en: 'Slower enemies, less HP' },
    enemyHpMult: 0.7, enemySpeedMult: 0.8, enemyDamageMult: 0.7, goldMult: 0.8, spawnRateMult: 0.8 },
  { id: 'normal', name: { ru: 'Обычная', en: 'Normal' }, desc: { ru: 'Сбалансированный вызов', en: 'Balanced challenge' },
    enemyHpMult: 1, enemySpeedMult: 1, enemyDamageMult: 1, goldMult: 1, spawnRateMult: 1 },
  { id: 'hard', name: { ru: 'Сложная', en: 'Hard' }, desc: { ru: 'Быстрые враги, больше урона', en: 'Faster enemies, more damage' },
    enemyHpMult: 1.4, enemySpeedMult: 1.2, enemyDamageMult: 1.3, goldMult: 1.5, spawnRateMult: 1.2 },
  { id: 'nightmare', name: { ru: 'Кошмар', en: 'Nightmare' }, desc: { ru: 'Враги свирепые, золото x2', en: 'Brutal enemies, gold x2' },
    enemyHpMult: 2, enemySpeedMult: 1.4, enemyDamageMult: 1.6, goldMult: 2, spawnRateMult: 1.5 },
];

// ===== Achievements =====
export type AchievementId =
  | 'kills_500' | 'kills_1000' | 'wave_30' | 'wave_50' | 'boss_5' | 'boss_10'
  | 'evolve_1' | 'evolve_3' | 'elite_10' | 'combo_50' | 'chest_5' | 'dash_50';

export interface AchievementDef {
  id: AchievementId;
  name: { ru: string; en: string };
  desc: { ru: string; en: string };
}

export const ACHIEVEMENTS: AchievementDef[] = [
  { id: 'kills_500', name: { ru: 'Палач', en: 'Executioner' }, desc: { ru: 'Убить 500 врагов', en: 'Kill 500 enemies' } },
  { id: 'kills_1000', name: { ru: 'Мясник', en: 'Butcher' }, desc: { ru: 'Убить 1000 врагов', en: 'Kill 1000 enemies' } },
  { id: 'wave_30', name: { ru: 'Выживший', en: 'Survivor' }, desc: { ru: 'Достичь волны 30', en: 'Reach wave 30' } },
  { id: 'wave_50', name: { ru: 'Легенда', en: 'Legend' }, desc: { ru: 'Достичь волны 50', en: 'Reach wave 50' } },
  { id: 'boss_5', name: { ru: 'Охотник', en: 'Hunter' }, desc: { ru: 'Убить 5 боссов', en: 'Kill 5 bosses' } },
  { id: 'boss_10', name: { ru: 'Герой', en: 'Hero' }, desc: { ru: 'Убить 10 боссов', en: 'Kill 10 bosses' } },
  { id: 'evolve_1', name: { ru: 'Эволюция', en: 'Evolution' }, desc: { ru: 'Эволюционировать способность', en: 'Evolve an ability' } },
  { id: 'evolve_3', name: { ru: 'Совершенство', en: 'Perfection' }, desc: { ru: '3 эволюции за забег', en: '3 evolutions in one run' } },
  { id: 'elite_10', name: { ru: 'Элитный охотник', en: 'Elite Hunter' }, desc: { ru: 'Убить 10 элитных врагов', en: 'Kill 10 elite enemies' } },
  { id: 'combo_50', name: { ru: 'Мастер комбо', en: 'Combo Master' }, desc: { ru: 'Комбо x50', en: 'Reach combo x50' } },
  { id: 'chest_5', name: { ru: 'Сокровищница', en: 'Treasure Hunter' }, desc: { ru: 'Открыть 5 сундуков', en: 'Open 5 chests' } },
  { id: 'dash_50', name: { ru: 'Призрак', en: 'Phantom' }, desc: { ru: '50 рывков за забег', en: '50 dashes in one run' } },
];

export function abilityName(id: AbilityType, lang: Lang): string {
  return ABILITIES[id].name[lang];
}
export function artifactName(id: ArtifactId, lang: Lang): string {
  return ARTIFACT_MAP[id].name[lang];
}
