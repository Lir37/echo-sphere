import { ABILITIES, SPHERE_TYPES } from './gameData';
import { CHARACTER_DEFS } from './characters';

const CHARACTER_IDS = Object.keys(CHARACTER_DEFS) as Array<keyof typeof CHARACTER_DEFS>;

function report(message: string): void {
  if (import.meta.env.DEV) console.warn(`[Echo Sphere] ${message}`);
}

function validateUnique<T>(items: T[], label: string, characterId: string): void {
  if (new Set(items).size !== items.length) {
    report(`${characterId}: duplicate ${label} preference detected`);
  }
}

export function validateCharacterData(): void {
  for (const id of CHARACTER_IDS) {
    const character = CHARACTER_DEFS[id];

    if (character.id !== id) report(`${id}: id does not match CHARACTER_DEFS key`);
    if (character.preferredSphereTypes.length === 0) report(`${id}: no preferred sphere types configured`);
    if (character.preferredSphereMods.length === 0) report(`${id}: no preferred tower mods configured`);
    if (character.preferredAbilities.length === 0) report(`${id}: no preferred abilities configured`);
    if (character.mastery.length !== 5) report(`${id}: expected exactly 5 mastery levels, got ${character.mastery.length}`);

    validateUnique(character.preferredSphereTypes, 'sphere types', id);
    validateUnique(character.preferredSphereMods, 'tower mods', id);
    validateUnique(character.preferredAbilities, 'abilities', id);

    for (const ability of character.preferredAbilities) {
      if (!(ability in ABILITIES)) report(`${id}: unknown preferred ability ${String(ability)}`);
    }
    for (const sphereType of character.preferredSphereTypes) {
      if (!(sphereType in SPHERE_TYPES)) report(`${id}: unknown preferred sphere type ${String(sphereType)}`);
    }

    character.mastery.forEach((mastery, index) => {
      if (mastery.level !== index + 1) report(`${id}: mastery levels are not ordered 1..5`);
      if (!mastery.title.ru || !mastery.title.en) report(`${id}: mastery ${mastery.level} has incomplete title localization`);
      if (!mastery.description.ru || !mastery.description.en) report(`${id}: mastery ${mastery.level} has incomplete description localization`);
    });

    if (!character.name.ru || !character.name.en) report(`${id}: incomplete name localization`);
    if (!character.role.ru || !character.role.en) report(`${id}: incomplete role localization`);
    if (!character.description.ru || !character.description.en) report(`${id}: incomplete description localization`);
    if (!character.mechanic.ru || !character.mechanic.en) report(`${id}: incomplete mechanic localization`);
  }
}

validateCharacterData();
