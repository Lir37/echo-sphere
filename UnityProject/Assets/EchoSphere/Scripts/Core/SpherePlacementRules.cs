using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    /// <summary>
    /// Source-mapped sphere placement rules from new-desing/src/spaceCollision.ts
    /// and src/engineSpheres.ts. Distances are converted from source logical
    /// pixels using the Unity 720 px / 12.6 world-unit camera calibration.
    /// </summary>
    public static class SpherePlacementRules
    {
        public const float SourcePixelsToWorld = 12.6f / 720f;
        public const float WorldWidth = 2400f * SourcePixelsToWorld;
        public const float WorldHeight = 2400f * SourcePixelsToWorld;
        public const float TowerBodyRadius = 20f * SourcePixelsToWorld;
        public const float TowerPlacementGap = 8f * SourcePixelsToWorld;
        public const float PlayerBuildGap = 48f * SourcePixelsToWorld;
        public const float EnemyPlacementGap = 10f * SourcePixelsToWorld;
        public const float EnemyNavigationRadius = 22f * SourcePixelsToWorld;
        public const float NavigationCellSize = 40f * SourcePixelsToWorld;
        public const float PlayerRadius = 16f * SourcePixelsToWorld;
        public const int DefaultMaxSpheres = 5;
        public const int MaxSpheresCap = 8;
        public const int MaxSameSphereCopies = 2;

        public static int GetMaxSpheres(int abilityBonus = 0, int shopBonus = 0)
        {
            return Math.Min(MaxSpheresCap, Math.Max(0, DefaultMaxSpheres + abilityBonus + shopBonus));
        }

        public static bool CanAddSphere(int currentCount, int sameTypeCount, int maxSpheres, int sameTypeCap = MaxSameSphereCopies)
        {
            return currentCount < Math.Max(0, maxSpheres)
                && sameTypeCount < Math.Max(0, sameTypeCap);
        }

        /// <summary>Mirrors repositionSphere: placement is checked first, then committed coordinates are clamped to the source world bounds.</summary>
        public static Vec2 ClampReposition(Vec2 candidate, float worldWidth = WorldWidth, float worldHeight = WorldHeight)
        {
            if (worldWidth <= 0f || worldHeight <= 0f) return candidate;
            return new Vec2(
                ClampScalar(candidate.X, -worldWidth * 0.5f, worldWidth * 0.5f),
                ClampScalar(candidate.Y, -worldHeight * 0.5f, worldHeight * 0.5f));
        }

        public static bool CanPlace(
            Vec2 candidate,
            Vec2 player,
            IReadOnlyList<Vec2> spherePositions,
            IReadOnlyList<bool> sphereAlive,
            IReadOnlyList<Vec2> enemyPositions,
            IReadOnlyList<float> enemyRadii,
            int ignoreSphereIndex = -1,
            float worldWidth = WorldWidth,
            float worldHeight = WorldHeight)
        {
            if (spherePositions == null || sphereAlive == null || enemyPositions == null || enemyRadii == null) return false;
            if (spherePositions.Count != sphereAlive.Count || enemyPositions.Count != enemyRadii.Count) return false;
            if (worldWidth <= 0f || worldHeight <= 0f || NavigationCellSize <= 0f) return false;

            // Source canPlaceSphere has no separate world-bounds rejection.
            // Its route search clamps grid coordinates; repositionSphere clamps
            // the committed move. Keep those source semantics rather than adding
            // a new placement prohibition here.
            if (Vec2.Distance(candidate, player) < TowerBodyRadius + PlayerBuildGap) return false;

            for (var i = 0; i < spherePositions.Count; i++)
            {
                if (!sphereAlive[i] || i == ignoreSphereIndex) continue;
                if (Vec2.Distance(candidate, spherePositions[i]) < TowerBodyRadius * 2f + TowerPlacementGap) return false;
            }

            for (var i = 0; i < enemyPositions.Count; i++)
            {
                var radius = Math.Max(0f, enemyRadii[i]);
                if (Vec2.Distance(candidate, enemyPositions[i]) < TowerBodyRadius + radius + EnemyPlacementGap) return false;
            }

            return HasRouteToOutside(candidate, player, spherePositions, sphereAlive, ignoreSphereIndex, worldWidth, worldHeight);
        }

        private static bool HasRouteToOutside(
            Vec2 candidate,
            Vec2 player,
            IReadOnlyList<Vec2> spherePositions,
            IReadOnlyList<bool> sphereAlive,
            int ignoreSphereIndex,
            float worldWidth,
            float worldHeight)
        {
            var minX = -worldWidth * 0.5f;
            var minY = -worldHeight * 0.5f;
            // Round the camera-calibrated ratio before converting to a cell count;
            // 42 / 0.7 can otherwise round up to 61 because of float representation.
            var columns = (int)Math.Round(worldWidth / NavigationCellSize);
            var rows = (int)Math.Round(worldHeight / NavigationCellSize);
            if (columns <= 0 || rows <= 0) return false;

            var obstaclePositions = new List<Vec2>();
            for (var i = 0; i < spherePositions.Count; i++)
                if (sphereAlive[i] && i != ignoreSphereIndex) obstaclePositions.Add(spherePositions[i]);
            obstaclePositions.Add(candidate);

            int CellX(float x) => Clamp((int)Math.Floor((x - minX) / NavigationCellSize), 0, columns - 1);
            int CellY(float y) => Clamp((int)Math.Floor((y - minY) / NavigationCellSize), 0, rows - 1);
            int CellKey(int x, int y) => y * columns + x;

            bool IsBlocked(int x, int y)
            {
                var center = new Vec2(
                    minX + (x + 0.5f) * NavigationCellSize,
                    minY + (y + 0.5f) * NavigationCellSize);
                for (var i = 0; i < obstaclePositions.Count; i++)
                    if (Vec2.Distance(center, obstaclePositions[i]) < TowerBodyRadius + EnemyNavigationRadius) return true;
                return false;
            }

            var startX = CellX(player.X);
            var startY = CellY(player.Y);
            if (IsBlocked(startX, startY)) return false;

            var visited = new bool[columns * rows];
            var queue = new int[columns * rows];
            var head = 0;
            var tail = 0;
            var startKey = CellKey(startX, startY);
            queue[tail++] = startKey;
            visited[startKey] = true;

            while (head < tail)
            {
                var key = queue[head++];
                var x = key % columns;
                var y = key / columns;
                if (x == 0 || y == 0 || x == columns - 1 || y == rows - 1) return true;

                for (var dx = -1; dx <= 1; dx++)
                {
                    for (var dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        var nx = x + dx;
                        var ny = y + dy;
                        if (nx < 0 || ny < 0 || nx >= columns || ny >= rows) continue;
                        var nextKey = CellKey(nx, ny);
                        if (visited[nextKey] || IsBlocked(nx, ny)) continue;
                        if (dx != 0 && dy != 0 && (IsBlocked(x + dx, y) || IsBlocked(x, y + dy))) continue;
                        visited[nextKey] = true;
                        queue[tail++] = nextKey;
                    }
                }
            }

            return false;
        }

        private static int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(max, value));
    }
}
