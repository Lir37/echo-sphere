import type { CanvasRenderingContext2D } from 'canvas';
import type { EnemyEntity, PlayerState, SphereEntity } from './engine';

export function renderOrbitalSphereRuntimeVfx(
  ctx: CanvasRenderingContext2D,
  sphere: SphereEntity,
  player: PlayerState,
  time: number,
  scale?: number,
  enemies?: EnemyEntity[],
): void;
