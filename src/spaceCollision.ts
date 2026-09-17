import type { EnemyEntity, GameState, Vec } from './engine';

// Physical size of a tower is intentionally much smaller than its attack radius.
// It closely follows the visible tower base instead of its much larger firing range.
export const TOWER_BODY_RADIUS = 20;
const TOWER_PLACEMENT_GAP = 8;
const PLAYER_BUILD_GAP = 48;
const ENEMY_PLACEMENT_GAP = 10;
const ENEMY_NAV_RADIUS = 22;
const NAV_CELL_SIZE = 40;
const ENEMY_SEPARATION_CELL = 80;
const MAX_PLAYER_PUSH_PER_FRAME = 14;
const PLAYER_RADIUS = 16;
const CONTACT_INVULNERABILITY = 0.25;

function distance(a: Vec, b: Vec): number {
  return Math.hypot(a.x - b.x, a.y - b.y);
}

function clampScalar(value: number, min: number, max: number): number {
  return Math.max(min, Math.min(max, value));
}

function normalize(dx: number, dy: number): Vec {
  const length = Math.hypot(dx, dy);
  if (length < 0.0001) return { x: 1, y: 0 };
  return { x: dx / length, y: dy / length };
}

function cellKey(x: number, y: number): string {
  return `${x},${y}`;
}

function getCellCoord(value: number): number {
  return Math.floor(value / ENEMY_SEPARATION_CELL);
}

/**
 * Validate a new tower placement before placeSphere() is called.
 *
 * Walls and corridors are intentional. The rule is that a new tower must not
 * remove the last navigable route between the player and the outside world.
 */
export function canPlaceSphere(s: GameState, x: number, y: number): boolean {
  if (distance({ x, y }, s.player.pos) < TOWER_BODY_RADIUS + PLAYER_BUILD_GAP) return false;

  for (const sphere of s.spheres) {
    if (!sphere.alive) continue;
    if (distance({ x, y }, sphere.pos) < TOWER_BODY_RADIUS * 2 + TOWER_PLACEMENT_GAP) return false;
  }

  // Never materialize a solid tower under an enemy. This keeps placement
  // predictable and avoids a sudden artificial displacement of the horde.
  for (const enemy of s.enemies) {
    if (enemy.hp <= 0) continue;
    if (distance({ x, y }, enemy.pos) < TOWER_BODY_RADIUS + enemy.radius + ENEMY_PLACEMENT_GAP) return false;
  }

  return hasRouteToOutside(s, { x, y });
}

/** Resolve solid-space collisions after the normal game update. */
export function resolveSpaceCollisions(s: GameState, dt: number): void {
  for (let pass = 0; pass < 2; pass++) {
    resolveEnemyTowerCollisions(s, dt);
    resolveEnemyEnemyCollisions(s);
    resolvePlayerTowerCollisions(s);
    resolveEnemyPlayerCollisions(s);
  }

  clampPosition(s.player.pos, s);
  for (const enemy of s.enemies) clampEnemy(enemy, s);
}

function resolveEnemyTowerCollisions(s: GameState, dt: number): void {
  for (const enemy of s.enemies) {
    if (enemy.hp <= 0) continue;

    for (const tower of s.spheres) {
      if (!tower.alive) continue;
      const dx = enemy.pos.x - tower.pos.x;
      const dy = enemy.pos.y - tower.pos.y;
      const minDistance = enemy.radius + TOWER_BODY_RADIUS;
      const currentDistance = Math.hypot(dx, dy);
      if (currentDistance >= minDistance) continue;

      const normal = normalize(dx, dy);
      const overlap = minDistance - currentDistance;
      enemy.pos.x += normal.x * overlap;
      enemy.pos.y += normal.y * overlap;

      // A pure radial push can stall an enemy against the exact center of a
      // wall. A tiny deterministic tangent bias makes it slide around the wall.
      const tangent = { x: -normal.y, y: normal.x };
      const toPlayer = normalize(s.player.pos.x - enemy.pos.x, s.player.pos.y - enemy.pos.y);
      const tangentSign = (toPlayer.x * tangent.x + toPlayer.y * tangent.y) >= 0 ? 1 : -1;
      const slide = Math.min(enemy.speed * dt * 0.35, 4);
      enemy.pos.x += tangent.x * tangentSign * slide;
      enemy.pos.y += tangent.y * tangentSign * slide;
    }
  }
}

function resolveEnemyEnemyCollisions(s: GameState): void {
  const grid = new Map<string, number[]>();

  for (let i = 0; i < s.enemies.length; i++) {
    const enemy = s.enemies[i];
    if (enemy.hp <= 0) continue;
    const cx = getCellCoord(enemy.pos.x);
    const cy = getCellCoord(enemy.pos.y);
    const key = cellKey(cx, cy);
    const bucket = grid.get(key);
    if (bucket) bucket.push(i);
    else grid.set(key, [i]);
  }

  for (let i = 0; i < s.enemies.length; i++) {
    const a = s.enemies[i];
    if (a.hp <= 0) continue;
    const cx = getCellCoord(a.pos.x);
    const cy = getCellCoord(a.pos.y);

    for (let ox = -1; ox <= 1; ox++) {
      for (let oy = -1; oy <= 1; oy++) {
        const bucket = grid.get(cellKey(cx + ox, cy + oy));
        if (!bucket) continue;

        for (const j of bucket) {
          if (j <= i) continue;
          const b = s.enemies[j];
          if (b.hp <= 0) continue;

          const dx = b.pos.x - a.pos.x;
          const dy = b.pos.y - a.pos.y;
          const minDistance = a.radius + b.radius;
          const currentDistance = Math.hypot(dx, dy);
          if (currentDistance >= minDistance) continue;

          const normal = normalize(dx, dy);
          const overlap = minDistance - currentDistance;
          const pushA = overlap * 0.5;
          const pushB = overlap - pushA;
          a.pos.x -= normal.x * pushA;
          a.pos.y -= normal.y * pushA;
          b.pos.x += normal.x * pushB;
          b.pos.y += normal.y * pushB;
        }
      }
    }
  }
}

function resolvePlayerTowerCollisions(s: GameState): void {
  for (const tower of s.spheres) {
    if (!tower.alive) continue;

    const dx = s.player.pos.x - tower.pos.x;
    const dy = s.player.pos.y - tower.pos.y;
    const minDistance = PLAYER_RADIUS + TOWER_BODY_RADIUS;
    const currentDistance = Math.hypot(dx, dy);
    if (currentDistance >= minDistance) continue;

    const normal = normalize(dx, dy);
    const overlap = minDistance - currentDistance;
    s.player.pos.x += normal.x * overlap;
    s.player.pos.y += normal.y * overlap;
  }
}

function resolveEnemyPlayerCollisions(s: GameState): void {
  let pushX = 0;
  let pushY = 0;
  let touchingPlayer = false;

  for (const enemy of s.enemies) {
    if (enemy.hp <= 0) continue;
    const dx = s.player.pos.x - enemy.pos.x;
    const dy = s.player.pos.y - enemy.pos.y;
    const minDistance = enemy.radius + PLAYER_RADIUS;
    const currentDistance = Math.hypot(dx, dy);
    if (currentDistance >= minDistance) continue;

    touchingPlayer = true;
    const normal = normalize(dx, dy);
    const overlap = minDistance - currentDistance;
    const enemyPush = overlap * 0.7;
    const playerPush = overlap - enemyPush;

    // Enemy receives the larger correction so the player feels physically
    // blocked instead of being thrown around by the horde.
    enemy.pos.x -= normal.x * enemyPush;
    enemy.pos.y -= normal.y * enemyPush;
    pushX += normal.x * playerPush;
    pushY += normal.y * playerPush;
  }

  const pushLength = Math.hypot(pushX, pushY);
  if (pushLength > MAX_PLAYER_PUSH_PER_FRAME) {
    const scale = MAX_PLAYER_PUSH_PER_FRAME / pushLength;
    pushX *= scale;
    pushY *= scale;
  }
  s.player.pos.x += pushX;
  s.player.pos.y += pushY;

  // updateEnemies() applies the first contact hit before this resolver runs.
  // Give the player a brief contact grace period so a solid enemy does not
  // deal damage every animation frame while pinning the player in place.
  if (touchingPlayer && s.player.invulnerableTimer <= 0) {
    s.player.invulnerableTimer = CONTACT_INVULNERABILITY;
  }
}

function clampPosition(pos: Vec, s: GameState): void {
  const limitX = s.worldWidth / 2 - PLAYER_RADIUS;
  const limitY = s.worldHeight / 2 - PLAYER_RADIUS;
  pos.x = clampScalar(pos.x, -limitX, limitX);
  pos.y = clampScalar(pos.y, -limitY, limitY);
}

function clampEnemy(enemy: EnemyEntity, s: GameState): void {
  const limitX = s.worldWidth / 2 - enemy.radius;
  const limitY = s.worldHeight / 2 - enemy.radius;
  enemy.pos.x = clampScalar(enemy.pos.x, -limitX, limitX);
  enemy.pos.y = clampScalar(enemy.pos.y, -limitY, limitY);
}

function hasRouteToOutside(s: GameState, candidate: Vec): boolean {
  const obstacles = s.spheres
    .filter((sphere) => sphere.alive)
    .map((sphere) => sphere.pos)
    .concat([candidate]);

  const minX = -s.worldWidth / 2;
  const minY = -s.worldHeight / 2;
  const cols = Math.ceil(s.worldWidth / NAV_CELL_SIZE);
  const rows = Math.ceil(s.worldHeight / NAV_CELL_SIZE);

  const toCell = (pos: Vec) => ({
    x: clampScalar(Math.floor((pos.x - minX) / NAV_CELL_SIZE), 0, cols - 1),
    y: clampScalar(Math.floor((pos.y - minY) / NAV_CELL_SIZE), 0, rows - 1),
  });

  const isBlocked = (cx: number, cy: number): boolean => {
    const center = {
      x: minX + (cx + 0.5) * NAV_CELL_SIZE,
      y: minY + (cy + 0.5) * NAV_CELL_SIZE,
    };
    return obstacles.some((obstacle) => distance(center, obstacle) < TOWER_BODY_RADIUS + ENEMY_NAV_RADIUS);
  };

  const start = toCell(s.player.pos);
  if (isBlocked(start.x, start.y)) return false;

  const queue: Array<{ x: number; y: number }> = [start];
  const visited = new Set<string>([cellKey(start.x, start.y)]);
  let cursor = 0;

  while (cursor < queue.length) {
    const current = queue[cursor++];
    if (current.x === 0 || current.y === 0 || current.x === cols - 1 || current.y === rows - 1) return true;

    for (let dx = -1; dx <= 1; dx++) {
      for (let dy = -1; dy <= 1; dy++) {
        if (dx === 0 && dy === 0) continue;
        const nx = current.x + dx;
        const ny = current.y + dy;
        if (nx < 0 || ny < 0 || nx >= cols || ny >= rows) continue;
        const key = cellKey(nx, ny);
        if (visited.has(key) || isBlocked(nx, ny)) continue;

        // Do not cut diagonally through a touching corner of two obstacles.
        if (dx !== 0 && dy !== 0) {
          if (isBlocked(current.x + dx, current.y) || isBlocked(current.x, current.y + dy)) continue;
        }

        visited.add(key);
        queue.push({ x: nx, y: ny });
      }
    }
  }

  return false;
}
