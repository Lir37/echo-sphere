import test from 'node:test';
import assert from 'node:assert/strict';
import { RUNE_DEFS } from '../src/runes.ts';

test('final Rune catalogue contains nine distinct tactical field effects', () => {
  const ids = Object.keys(RUNE_DEFS);
  assert.equal(ids.length, 9);
  assert.equal(new Set(ids).size, 9);
  for (const id of ids) {
    assert.ok(RUNE_DEFS[id].name.ru);
    assert.ok(RUNE_DEFS[id].name.en);
    assert.ok(RUNE_DEFS[id].description.ru);
    assert.ok(RUNE_DEFS[id].color);
  }
});
