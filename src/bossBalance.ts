export const BOSS_CHARGER_WINDUP_SECONDS = 0.45;
export const BOSS_CHARGER_COMMIT_SECONDS = 0.35;
export const BOSS_CHARGER_TOTAL_TELEGRAPH_SECONDS =
  BOSS_CHARGER_WINDUP_SECONDS + BOSS_CHARGER_COMMIT_SECONDS;

export type BossChargerPhase = 'windup' | 'committed-dash' | 'ready';

export function getBossChargerPhase(chargeTimer: number): BossChargerPhase {
  if (chargeTimer <= 0) return 'ready';
  return chargeTimer <= BOSS_CHARGER_COMMIT_SECONDS ? 'committed-dash' : 'windup';
}
