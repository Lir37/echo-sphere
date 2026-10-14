using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    public sealed class CollisionBody
    {
        public Vec2 Position;
        public float Radius, Speed, Mass;
        public bool FollowBreach;
    }

    // Direct translation of src/spaceCollision.ts. Inputs and outputs use source pixels,
    // including source Y direction; no Unity physics, frame ordering or RNG is involved.
    public static class SpaceCollisionRules
    {
        private static Vec2 Normal(float x, float y)
        {
            var length = (float)Math.Sqrt(x * x + y * y);
            return length < 0.0001f ? new Vec2(1f, 0f) : new Vec2(x / length, y / length);
        }
        private static float Distance(float x, float y) => (float)Math.Sqrt(x * x + y * y);
        private static Vec2 Add(Vec2 a, Vec2 b, float scale) => new Vec2(a.X + b.X * scale, a.Y + b.Y * scale);
        public static Vec2 Resolve(Vec2 player, IList<CollisionBody> enemies, IList<Vec2> towers, float dt, bool follow, float width = 2400f, float height = 2400f)
        {
            for (var pass = 0; pass < 2; pass++)
            {
                foreach (var enemy in enemies)
                {
                    if (follow && enemy.FollowBreach) continue;
                    foreach (var tower in towers)
                    {
                        var dx = enemy.Position.X - tower.X;
                        var dy = enemy.Position.Y - tower.Y;
                        var minimum = enemy.Radius + 20f;
                        var distance = Distance(dx, dy);
                        if (distance >= minimum)
                        {
                            var travel = Math.Min(enemy.Speed * dt, 140f);
                            var direction = Normal(player.X - enemy.Position.X, player.Y - enemy.Position.Y);
                            var previous = Add(enemy.Position, direction, -travel);
                            var sx = enemy.Position.X - previous.X;
                            var sy = enemy.Position.Y - previous.Y;
                            var lengthSq = sx * sx + sy * sy;
                            var projection = lengthSq > 0f ? Math.Max(0f, Math.Min(1f, ((tower.X - previous.X) * sx + (tower.Y - previous.Y) * sy) / lengthSq)) : 0f;
                            var closest = new Vec2(previous.X + sx * projection, previous.Y + sy * projection);
                            dx = closest.X - tower.X; dy = closest.Y - tower.Y;
                            if (Distance(dx, dy) >= minimum) continue;
                            enemy.Position = Add(tower, Normal(dx, dy), minimum);
                        }
                        else enemy.Position = Add(enemy.Position, Normal(dx, dy), minimum - distance);
                        var normal = Normal(enemy.Position.X - tower.X, enemy.Position.Y - tower.Y);
                        var tangent = new Vec2(-normal.Y, normal.X);
                        var toPlayer = Normal(player.X - enemy.Position.X, player.Y - enemy.Position.Y);
                        var sign = toPlayer.X * tangent.X + toPlayer.Y * tangent.Y >= 0f ? 1f : -1f;
                        enemy.Position = Add(enemy.Position, tangent, sign * Math.Min(enemy.Speed * dt * 0.35f, 4f));
                    }
                }
                // Preserve source grid traversal and mass-weighted separation order.
                var grid = new Dictionary<(int, int), List<int>>();
                for (var i = 0; i < enemies.Count; i++)
                {
                    var p = enemies[i].Position;
                    var key = ((int)Math.Floor(p.X / 80f), (int)Math.Floor(p.Y / 80f));
                    if (!grid.TryGetValue(key, out var bucket)) grid[key] = bucket = new List<int>();
                    bucket.Add(i);
                }
                for (var i = 0; i < enemies.Count; i++)
                {
                    var a = enemies[i];
                    var cx = (int)Math.Floor(a.Position.X / 80f); var cy = (int)Math.Floor(a.Position.Y / 80f);
                    for (var ox = -1; ox <= 1; ox++) for (var oy = -1; oy <= 1; oy++)
                    {
                        if (!grid.TryGetValue((cx + ox, cy + oy), out var bucket)) continue;
                        foreach (var j in bucket)
                        {
                            if (j <= i) continue;
                            var b = enemies[j];
                            var dx = b.Position.X - a.Position.X; var dy = b.Position.Y - a.Position.Y;
                            var distance = Distance(dx, dy); var minimum = a.Radius + b.Radius;
                            if (distance >= minimum) continue;
                            var normal = Normal(dx, dy); var overlap = minimum - distance; var mass = a.Mass + b.Mass;
                            a.Position = Add(a.Position, normal, -overlap * b.Mass / mass);
                            b.Position = Add(b.Position, normal, overlap * a.Mass / mass);
                        }
                    }
                }
                foreach (var tower in towers)
                {
                    var dx = player.X - tower.X; var dy = player.Y - tower.Y; var distance = Distance(dx, dy);
                    if (distance < 36f) player = Add(player, Normal(dx, dy), 36f - distance);
                }
                var push = Vec2.Zero;
                foreach (var enemy in enemies)
                {
                    var dx = player.X - enemy.Position.X; var dy = player.Y - enemy.Position.Y;
                    var distance = Distance(dx, dy); var minimum = enemy.Radius + 16f;
                    if (distance >= minimum) continue;
                    var normal = Normal(dx, dy); var overlap = minimum - distance;
                    enemy.Position = Add(enemy.Position, normal, -overlap * 2f / (enemy.Mass + 2f));
                    push = Add(push, normal, overlap * enemy.Mass / (enemy.Mass + 2f));
                }
                var pushLength = Distance(push.X, push.Y);
                player = Add(player, push, pushLength > 14f ? 14f / pushLength : 1f);
            }
            player = Clamp(player, width / 2f - 16f, height / 2f - 16f);
            foreach (var enemy in enemies) enemy.Position = Clamp(enemy.Position, width / 2f - enemy.Radius, height / 2f - enemy.Radius);
            return player;
        }
        private static Vec2 Clamp(Vec2 p, float x, float y) => new Vec2(Math.Max(-x, Math.Min(x, p.X)), Math.Max(-y, Math.Min(y, p.Y)));
    }
}
