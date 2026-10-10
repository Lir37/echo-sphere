using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    public enum SphereNetworkFormation
    {
        None, Line, Triangle, Cluster, Square, Ring, Lattice, Fractal
    }

    public readonly struct SphereNetworkNode
    {
        public readonly Vec2 Position;
        public readonly bool Alive;
        public readonly float DisabledTimer;

        public SphereNetworkNode(Vec2 position, bool alive = true, float disabledTimer = 0f)
        {
            Position = position;
            Alive = alive;
            DisabledTimer = disabledTimer;
        }

        public bool IsActive => Alive && DisabledTimer <= 0f;
    }

    public readonly struct SphereNetworkLink
    {
        public readonly int A;
        public readonly int B;
        public readonly float Distance;

        public SphereNetworkLink(int a, int b, float distance)
        {
            A = a;
            B = b;
            Distance = distance;
        }
    }

    public sealed class SphereNetworkShape
    {
        public SphereNetworkFormation Type { get; }
        public float Strength { get; }
        public float DominanceScore { get; set; }
        public List<int> NodeIndexes { get; }
        public bool Active { get; set; } = true;

        public SphereNetworkShape(SphereNetworkFormation type, float strength, List<int> nodeIndexes)
        {
            Type = type;
            Strength = strength;
            NodeIndexes = nodeIndexes ?? new List<int>();
        }

        public bool Contains(int nodeIndex) => NodeIndexes.Contains(nodeIndex);
    }

    public sealed class SphereNetworkState
    {
        public float LinkDistance { get; internal set; }
        public List<int> Nodes { get; } = new List<int>();
        public List<SphereNetworkLink> Links { get; } = new List<SphereNetworkLink>();
        public List<SphereNetworkShape> FormationCandidates { get; } = new List<SphereNetworkShape>();
        public SphereNetworkShape DominantFormation { get; internal set; }
        public SphereNetworkShape SecondaryFormation { get; internal set; }
        public float FormationEfficiency { get; set; } = 1f;

        public SphereNetworkShape GetFormation(SphereNetworkFormation type)
        {
            if (DominantFormation != null && DominantFormation.Type == type) return DominantFormation;
            if (SecondaryFormation != null && SecondaryFormation.Type == type) return SecondaryFormation;
            return null;
        }
    }

    public static class SphereNetworkRules
    {
        public const float DefaultLinkDistance = 2.20f;
        private const float SecondaryScoreGap = 0.14f * 100f;
        private const float SecondaryDisjointScoreGap = 0.18f * 100f;
        private const float DominanceSwitchMargin = 10f;
        private const float Epsilon = 0.0001f;

        public static SphereNetworkState Analyze(
            IReadOnlyList<SphereNetworkNode> nodes,
            float linkDistance = DefaultLinkDistance,
            SphereNetworkFormation previousDominant = SphereNetworkFormation.None)
        {
            var state = new SphereNetworkState { LinkDistance = Math.Max(0.01f, linkDistance) };
            if (nodes == null || nodes.Count == 0) return state;

            for (var i = 0; i < nodes.Count; i++)
                if (nodes[i].IsActive) state.Nodes.Add(i);

            for (var i = 0; i < state.Nodes.Count; i++)
            {
                for (var j = i + 1; j < state.Nodes.Count; j++)
                {
                    var a = state.Nodes[i];
                    var b = state.Nodes[j];
                    var distance = Vec2.Distance(nodes[a].Position, nodes[b].Position);
                    if (distance <= state.LinkDistance)
                        state.Links.Add(new SphereNetworkLink(a, b, distance));
                }
            }

            var line = FindLine(nodes, state.Nodes);
            var triangle = FindTriangle(nodes, state.Nodes, state.LinkDistance);
            var square = FindSquare(nodes, state.Nodes, state.LinkDistance);
            var cluster = FindCluster(nodes, state.Nodes, state.Links, state.LinkDistance);
            var ring = FindRing(nodes, state.Nodes, state.LinkDistance);
            var lattice = FindLattice(nodes, state.Nodes, state.Links, state.LinkDistance);
            var fractal = FindFractal(nodes, state.Nodes, state.Links, ring, lattice, square, triangle);

            AddIfPresent(state.FormationCandidates, fractal);
            AddIfPresent(state.FormationCandidates, lattice);
            AddIfPresent(state.FormationCandidates, ring);
            AddIfPresent(state.FormationCandidates, square);
            AddIfPresent(state.FormationCandidates, triangle);
            AddIfPresent(state.FormationCandidates, cluster);
            AddIfPresent(state.FormationCandidates, line);
            RankCandidates(state.FormationCandidates, previousDominant);

            if (state.FormationCandidates.Count > 0)
            {
                state.DominantFormation = state.FormationCandidates[0];
                state.SecondaryFormation = ChooseSecondary(state.FormationCandidates, state.DominantFormation);
            }
            return state;
        }

        public static List<int> GetLinkedNodeIndexes(SphereNetworkState state, int nodeIndex)
        {
            var result = new List<int>();
            if (state == null) return result;
            for (var i = 0; i < state.Links.Count; i++)
            {
                var link = state.Links[i];
                if (link.A == nodeIndex) result.Add(link.B);
                else if (link.B == nodeIndex) result.Add(link.A);
            }
            result.Sort();
            return result;
        }

        public static bool AreNodesLinked(SphereNetworkState state, int a, int b)
        {
            if (state == null) return false;
            for (var i = 0; i < state.Links.Count; i++)
            {
                var link = state.Links[i];
                if ((link.A == a && link.B == b) || (link.A == b && link.B == a)) return true;
            }
            return false;
        }

        public static float GetFormationBonusMultiplier(SphereNetworkState state, SphereNetworkFormation type, int nodeIndex = -1)
        {
            if (state == null) return 0f;
            var dominant = state.DominantFormation;
            if (dominant != null && dominant.Active && dominant.Type == type &&
                (nodeIndex < 0 || dominant.Contains(nodeIndex)))
                return Clamp01(state.FormationEfficiency);
            var secondary = state.SecondaryFormation;
            if (secondary != null && secondary.Type == type &&
                (nodeIndex < 0 || secondary.Contains(nodeIndex)))
                return 0.5f * Clamp01(state.FormationEfficiency);
            return 0f;
        }

        private static void AddIfPresent(List<SphereNetworkShape> candidates, SphereNetworkShape shape)
        {
            if (shape != null) candidates.Add(shape);
        }

        private static SphereNetworkShape FindLine(IReadOnlyList<SphereNetworkNode> nodes, List<int> indexes)
        {
            if (indexes.Count < 3) return null;
            var wholeStrength = LineStrength(nodes, indexes);
            if (wholeStrength >= 0.80f) return new SphereNetworkShape(SphereNetworkFormation.Line, wholeStrength, new List<int>(indexes));
            SphereNetworkShape best = null;
            ForEachTriple(indexes, combo =>
            {
                var strength = LineStrength(nodes, combo);
                if (strength >= 0.80f && (best == null || strength > best.Strength))
                    best = new SphereNetworkShape(SphereNetworkFormation.Line, strength, combo);
            });
            return best;
        }

        private static SphereNetworkShape FindTriangle(IReadOnlyList<SphereNetworkNode> nodes, List<int> indexes, float linkDistance)
        {
            SphereNetworkShape best = null;
            ForEachTriple(indexes, combo =>
            {
                var strength = TriangleStrength(nodes, combo, linkDistance);
                if (strength >= 0.82f && (best == null || strength > best.Strength))
                    best = new SphereNetworkShape(SphereNetworkFormation.Triangle, strength, combo);
            });
            return best;
        }

        private static SphereNetworkShape FindSquare(IReadOnlyList<SphereNetworkNode> nodes, List<int> indexes, float linkDistance)
        {
            SphereNetworkShape best = null;
            ForEachQuad(indexes, combo =>
            {
                var strength = SquareStrength(nodes, combo, linkDistance);
                if (strength >= 0.84f && (best == null || strength > best.Strength))
                    best = new SphereNetworkShape(SphereNetworkFormation.Square, strength, combo);
            });
            return best;
        }

        private static SphereNetworkShape FindCluster(IReadOnlyList<SphereNetworkNode> nodes, List<int> indexes, List<SphereNetworkLink> links, float linkDistance)
        {
            if (indexes.Count < 4) return null;
            var strength = ClusterStrength(nodes, indexes, linkDistance);
            return strength >= 0.78f ? new SphereNetworkShape(SphereNetworkFormation.Cluster, strength, new List<int>(indexes)) : null;
        }

        private static SphereNetworkShape FindRing(IReadOnlyList<SphereNetworkNode> nodes, List<int> indexes, float linkDistance)
        {
            if (indexes.Count < 4) return null;
            var ordered = OrderAroundCentroid(nodes, indexes);
            var distances = new List<float>();
            for (var i = 0; i < ordered.Count; i++)
            {
                var distance = Vec2.Distance(nodes[ordered[i]].Position, nodes[ordered[(i + 1) % ordered.Count]].Position);
                if (distance > linkDistance) return null;
                distances.Add(distance);
            }
            var mean = Mean(distances);
            if (mean < 0.55f) return null;
            var variance = 0f;
            for (var i = 0; i < distances.Count; i++) variance += Math.Abs(distances[i] - mean);
            variance /= distances.Count * mean;
            var strength = 1f - Math.Min(1f, variance * 2f);
            return strength >= 0.68f ? new SphereNetworkShape(SphereNetworkFormation.Ring, strength, ordered) : null;
        }

        private static SphereNetworkShape FindLattice(IReadOnlyList<SphereNetworkNode> nodes, List<int> indexes, List<SphereNetworkLink> links, float linkDistance)
        {
            if (indexes.Count < 4) return null;
            var triangles = new List<List<int>>();
            ForEachTriple(indexes, combo =>
            {
                if (AreLinked(links, combo[0], combo[1]) &&
                    AreLinked(links, combo[1], combo[2]) &&
                    AreLinked(links, combo[0], combo[2]) &&
                    TriangleStrength(nodes, combo, linkDistance) >= 0.82f)
                    triangles.Add(combo);
            });
            if (triangles.Count < 2) return null;
            var shared = false;
            for (var i = 0; i < triangles.Count && !shared; i++)
                for (var j = i + 1; j < triangles.Count && !shared; j++)
                {
                    var common = 0;
                    for (var a = 0; a < triangles[i].Count; a++)
                        if (triangles[j].Contains(triangles[i][a])) common++;
                    if (common >= 2) shared = true;
                }
            if (!shared) return null;
            var strength = Math.Min(1f, 0.55f + Math.Min(0.45f, triangles.Count * 0.08f));
            var used = new List<int>();
            for (var i = 0; i < triangles.Count; i++)
                for (var j = 0; j < triangles[i].Count; j++)
                    if (!used.Contains(triangles[i][j])) used.Add(triangles[i][j]);
            used.Sort();
            return new SphereNetworkShape(SphereNetworkFormation.Lattice, strength, used);
        }

        private static SphereNetworkShape FindFractal(
            IReadOnlyList<SphereNetworkNode> nodes, List<int> indexes, List<SphereNetworkLink> links,
            SphereNetworkShape ring, SphereNetworkShape lattice, SphereNetworkShape square, SphereNetworkShape triangle)
        {
            var shapes = new List<SphereNetworkShape>();
            if (ring != null) shapes.Add(ring);
            if (lattice != null) shapes.Add(lattice);
            if (square != null) shapes.Add(square);
            if (triangle != null) shapes.Add(triangle);
            if (shapes.Count < 2) return null;
            var used = new List<int>();
            var strength = 0f;
            for (var i = 0; i < shapes.Count; i++)
            {
                strength += shapes[i].Strength;
                for (var j = 0; j < shapes[i].NodeIndexes.Count; j++)
                    if (!used.Contains(shapes[i].NodeIndexes[j])) used.Add(shapes[i].NodeIndexes[j]);
            }
            if (!IsConnected(used, links)) return null;
            used.Sort();
            return new SphereNetworkShape(SphereNetworkFormation.Fractal, Math.Min(1f, strength / shapes.Count), used);
        }

        private static float LineStrength(IReadOnlyList<SphereNetworkNode> nodes, List<int> indexes)
        {
            if (indexes.Count < 3) return 0f;
            var center = Centroid(nodes, indexes);
            var best = 0f;
            for (var step = 0; step < 24; step++)
            {
                var angle = step / 24f * (float)Math.PI;
                var dirX = (float)Math.Cos(angle);
                var dirY = (float)Math.Sin(angle);
                var maxError = 0f;
                for (var i = 0; i < indexes.Count; i++)
                {
                    var delta = nodes[indexes[i]].Position - center;
                    maxError = Math.Max(maxError, Math.Abs(delta.X * dirY - delta.Y * dirX));
                }
                best = Math.Max(best, 1f - Math.Min(1f, maxError / 0.72f));
            }
            return best;
        }

        private static float TriangleStrength(IReadOnlyList<SphereNetworkNode> nodes, List<int> indexes, float linkDistance)
        {
            if (indexes.Count != 3) return 0f;
            var ab = Vec2.Distance(nodes[indexes[0]].Position, nodes[indexes[1]].Position);
            var bc = Vec2.Distance(nodes[indexes[1]].Position, nodes[indexes[2]].Position);
            var ca = Vec2.Distance(nodes[indexes[2]].Position, nodes[indexes[0]].Position);
            if (ab > linkDistance || bc > linkDistance || ca > linkDistance) return 0f;
            var mean = (ab + bc + ca) / 3f;
            if (mean < 0.65f || mean > linkDistance) return 0f;
            var variance = (Math.Abs(ab - mean) + Math.Abs(bc - mean) + Math.Abs(ca - mean)) / (3f * mean);
            return 1f - Math.Min(1f, variance * 2.2f);
        }

        private static float SquareStrength(IReadOnlyList<SphereNetworkNode> nodes, List<int> indexes, float linkDistance)
        {
            if (indexes.Count != 4) return 0f;
            var ordered = OrderAroundCentroid(nodes, indexes);
            var sides = new List<float>();
            for (var i = 0; i < 4; i++) sides.Add(Vec2.Distance(nodes[ordered[i]].Position, nodes[ordered[(i + 1) % 4]].Position));
            var diagonals0 = Vec2.Distance(nodes[ordered[0]].Position, nodes[ordered[2]].Position);
            var diagonals1 = Vec2.Distance(nodes[ordered[1]].Position, nodes[ordered[3]].Position);
            var sideMean = Mean(sides);
            if (sideMean < 0.60f || sideMean > linkDistance) return 0f;
            for (var i = 0; i < sides.Count; i++) if (sides[i] > linkDistance) return 0f;
            var sideVariance = 0f;
            for (var i = 0; i < sides.Count; i++) sideVariance += Math.Abs(sides[i] - sideMean);
            sideVariance /= 4f * sideMean;
            var diagonalMean = (diagonals0 + diagonals1) / 2f;
            var diagonalVariance = Math.Abs(diagonals0 - diagonals1) / Math.Max(1f, diagonalMean);
            var sqrt2 = (float)Math.Sqrt(2f);
            var rightAngleError = Math.Abs(diagonalMean / sideMean - sqrt2) / sqrt2;
            return Math.Max(0f, 1f - Math.Min(1f, sideVariance * 2f + diagonalVariance + rightAngleError));
        }

        private static float ClusterStrength(IReadOnlyList<SphereNetworkNode> nodes, List<int> indexes, float linkDistance)
        {
            if (indexes.Count < 4) return 0f;
            var sum = 0f; var pairs = 0; var linkedPairs = 0;
            for (var i = 0; i < indexes.Count; i++)
                for (var j = i + 1; j < indexes.Count; j++)
                {
                    var distance = Vec2.Distance(nodes[indexes[i]].Position, nodes[indexes[j]].Position);
                    sum += distance; pairs++;
                    if (distance <= linkDistance) linkedPairs++;
                }
            var average = sum / Math.Max(1, pairs);
            var compactness = 1f - Math.Min(1f, Math.Max(0f, average - 0.90f) / 0.90f);
            var connectivity = linkedPairs / (float)Math.Max(1, pairs);
            return compactness * 0.65f + connectivity * 0.35f;
        }

        private static bool AreLinked(List<SphereNetworkLink> links, int a, int b)
        {
            for (var i = 0; i < links.Count; i++)
                if ((links[i].A == a && links[i].B == b) || (links[i].A == b && links[i].B == a)) return true;
            return false;
        }

        private static bool IsConnected(List<int> indexes, List<SphereNetworkLink> links)
        {
            if (indexes.Count <= 1) return true;
            var visited = new HashSet<int> { indexes[0] };
            var queue = new Queue<int>();
            queue.Enqueue(indexes[0]);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                for (var i = 0; i < links.Count; i++)
                {
                    var link = links[i];
                    var next = link.A == current ? link.B : link.B == current ? link.A : -1;
                    if (next >= 0 && indexes.Contains(next) && visited.Add(next)) queue.Enqueue(next);
                }
            }
            return visited.Count == indexes.Count;
        }

        private static Vec2 Centroid(IReadOnlyList<SphereNetworkNode> nodes, List<int> indexes)
        {
            var center = Vec2.Zero;
            for (var i = 0; i < indexes.Count; i++) center += nodes[indexes[i]].Position;
            return indexes.Count == 0 ? Vec2.Zero : center / indexes.Count;
        }

        private static List<int> OrderAroundCentroid(IReadOnlyList<SphereNetworkNode> nodes, List<int> indexes)
        {
            var center = Centroid(nodes, indexes);
            var ordered = new List<int>(indexes);
            ordered.Sort((a, b) =>
            {
                var angleA = Math.Atan2(nodes[a].Position.Y - center.Y, nodes[a].Position.X - center.X);
                var angleB = Math.Atan2(nodes[b].Position.Y - center.Y, nodes[b].Position.X - center.X);
                return angleA.CompareTo(angleB);
            });
            return ordered;
        }

        private static void ForEachTriple(List<int> indexes, Action<List<int>> action)
        {
            for (var i = 0; i < indexes.Count - 2; i++)
                for (var j = i + 1; j < indexes.Count - 1; j++)
                    for (var k = j + 1; k < indexes.Count; k++)
                        action(new List<int> { indexes[i], indexes[j], indexes[k] });
        }

        private static void ForEachQuad(List<int> indexes, Action<List<int>> action)
        {
            for (var i = 0; i < indexes.Count - 3; i++)
                for (var j = i + 1; j < indexes.Count - 2; j++)
                    for (var k = j + 1; k < indexes.Count - 1; k++)
                        for (var l = k + 1; l < indexes.Count; l++)
                            action(new List<int> { indexes[i], indexes[j], indexes[k], indexes[l] });
        }

        private static void RankCandidates(List<SphereNetworkShape> candidates, SphereNetworkFormation previousDominant)
        {
            for (var i = 0; i < candidates.Count; i++)
                candidates[i].DominanceScore = candidates[i].Strength * 100f + Specificity(candidates[i].Type) +
                    Math.Min(6f, Math.Max(0, candidates[i].NodeIndexes.Count - 3) * 1.5f);
            candidates.Sort((a, b) =>
            {
                var score = b.DominanceScore.CompareTo(a.DominanceScore);
                if (score != 0) return score;
                var strength = b.Strength.CompareTo(a.Strength);
                if (strength != 0) return strength;
                var size = b.NodeIndexes.Count.CompareTo(a.NodeIndexes.Count);
                return size != 0 ? size : string.CompareOrdinal(a.Type.ToString(), b.Type.ToString());
            });
            if (previousDominant == SphereNetworkFormation.None || candidates.Count < 2) return;
            var previousIndex = candidates.FindIndex(shape => shape.Type == previousDominant);
            if (previousIndex <= 0) return;
            if (candidates[0].DominanceScore < candidates[previousIndex].DominanceScore + DominanceSwitchMargin)
            {
                var previous = candidates[previousIndex];
                candidates.RemoveAt(previousIndex);
                candidates.Insert(0, previous);
            }
        }

        private static SphereNetworkShape ChooseSecondary(List<SphereNetworkShape> ranked, SphereNetworkShape dominant)
        {
            if (dominant == null) return null;
            for (var i = 0; i < ranked.Count; i++)
            {
                var candidate = ranked[i];
                if (candidate.Type == dominant.Type) continue;
                var gap = dominant.DominanceScore - candidate.DominanceScore;
                if (gap > SecondaryDisjointScoreGap) continue;
                var disjoint = true;
                for (var j = 0; j < candidate.NodeIndexes.Count; j++)
                    if (dominant.NodeIndexes.Contains(candidate.NodeIndexes[j])) { disjoint = false; break; }
                if (disjoint) return candidate;
            }
            for (var i = 0; i < ranked.Count; i++)
            {
                var candidate = ranked[i];
                if (candidate.Type != dominant.Type && dominant.DominanceScore - candidate.DominanceScore <= SecondaryScoreGap)
                    return candidate;
            }
            return null;
        }

        private static float Specificity(SphereNetworkFormation type)
        {
            switch (type)
            {
                case SphereNetworkFormation.Triangle: return 1f;
                case SphereNetworkFormation.Cluster: return 2f;
                case SphereNetworkFormation.Ring: return 3f;
                case SphereNetworkFormation.Square: return 5f;
                case SphereNetworkFormation.Lattice: return 5f;
                case SphereNetworkFormation.Fractal: return 7f;
                default: return 0f;
            }
        }

        private static float Mean(List<float> values)
        {
            var sum = 0f;
            for (var i = 0; i < values.Count; i++) sum += values[i];
            return values.Count == 0 ? 0f : sum / values.Count;
        }

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
    }
}
