using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    public enum NetworkFormation { None, Line, Triangle, Cluster, Square, Ring, Lattice, Fractal }

    public sealed class NetworkNode
    {
        public float X { get; }
        public float Y { get; }
        public bool Alive { get; }
        public NetworkNode(float x, float y, bool alive = true) { X = x; Y = y; Alive = alive; }
    }

    public sealed class NetworkLink
    {
        public int A { get; }
        public int B { get; }
        public float Distance { get; }
        public NetworkLink(int a, int b, float distance) { A = a; B = b; Distance = distance; }
    }

    public sealed class NetworkShape
    {
        public NetworkFormation Type { get; }
        public float Strength { get; }
        public float DominanceScore { get; }
        public List<int> Nodes { get; }
        public NetworkShape(NetworkFormation type, float strength, List<int> nodes)
        {
            Type = type; Strength = strength; Nodes = nodes;
            DominanceScore = strength * 100f + Specificity(type) + Math.Min(6f, Math.Max(0, nodes.Count - 3) * 1.5f);
        }
        private static int Specificity(NetworkFormation type)
        {
            switch (type)
            {
                case NetworkFormation.Triangle: return 1;
                case NetworkFormation.Cluster: return 2;
                case NetworkFormation.Ring: return 3;
                case NetworkFormation.Square:
                case NetworkFormation.Lattice: return 5;
                case NetworkFormation.Fractal: return 7;
                default: return 0;
            }
        }
    }

    public sealed class NetworkState
    {
        public List<int> AliveNodes { get; } = new List<int>();
        public List<NetworkLink> Links { get; } = new List<NetworkLink>();
        public List<NetworkShape> Candidates { get; } = new List<NetworkShape>();
        public NetworkShape Dominant { get; internal set; }
        public NetworkShape Secondary { get; internal set; }
        public float LinkDistance { get; internal set; }
    }

    public sealed class NetworkProfile
    {
        public int LinkedNeighbours { get; internal set; }
        public bool Line { get; internal set; }
        public bool Triangle { get; internal set; }
        public bool Square { get; internal set; }
        public bool Cluster { get; internal set; }
        public bool Ring { get; internal set; }
        public bool Lattice { get; internal set; }
        public bool Fractal { get; internal set; }
    }

    public static class NetworkCombatRules
    {
        public const float DefaultLinkDistance = 4.4f;
        private const float SecondaryScoreGap = 14f;
        private const float SecondaryDisjointScoreGap = 18f;

        public static NetworkState Analyze(IList<NetworkNode> nodes, float linkDistance = DefaultLinkDistance, NetworkFormation previousDominant = NetworkFormation.None)
        {
            var state = new NetworkState { LinkDistance = Math.Max(0.1f, linkDistance) };
            for (var i = 0; i < nodes.Count; i++) if (nodes[i] != null && nodes[i].Alive) state.AliveNodes.Add(i);
            for (var i = 0; i < state.AliveNodes.Count; i++)
            for (var j = i + 1; j < state.AliveNodes.Count; j++)
            {
                var a = state.AliveNodes[i]; var b = state.AliveNodes[j];
                var d = Distance(nodes[a], nodes[b]);
                if (d <= state.LinkDistance) state.Links.Add(new NetworkLink(a, b, d));
            }

            var line = FindLine(nodes, state.AliveNodes);
            var triangle = FindTriangle(nodes, state.AliveNodes, state.LinkDistance);
            var square = FindSquare(nodes, state.AliveNodes, state.LinkDistance);
            var cluster = FindCluster(nodes, state.AliveNodes, state.LinkDistance);
            var ring = FindRing(nodes, state.AliveNodes, state.LinkDistance);
            var lattice = FindLattice(nodes, state.AliveNodes, state.Links, state.LinkDistance);
            var fractal = FindFractal(state, ring, lattice, square, triangle);
            Add(state, fractal); Add(state, lattice); Add(state, ring); Add(state, square);
            Add(state, triangle); Add(state, cluster); Add(state, line);
            state.Candidates.Sort((a, b) =>
                b.DominanceScore.CompareTo(a.DominanceScore) != 0 ? b.DominanceScore.CompareTo(a.DominanceScore)
                : b.Strength.CompareTo(a.Strength) != 0 ? b.Strength.CompareTo(a.Strength)
                : b.Nodes.Count.CompareTo(a.Nodes.Count) != 0 ? b.Nodes.Count.CompareTo(a.Nodes.Count)
                : a.Type.CompareTo(b.Type));

            if (state.Candidates.Count == 0) return state;
            var top = state.Candidates[0];
            if (previousDominant != NetworkFormation.None)
            {
                NetworkShape previous = null;
                for (var i = 0; i < state.Candidates.Count; i++) if (state.Candidates[i].Type == previousDominant) { previous = state.Candidates[i]; break; }
                if (previous != null && previous.Type != top.Type && top.DominanceScore < previous.DominanceScore + 10f)
                    top = previous;
            }
            state.Dominant = top;
            var disjoint = FindSecondary(state.Candidates, top, true);
            state.Secondary = disjoint ?? FindSecondary(state.Candidates, top, false);
            return state;
        }

        public static float GetFormationBonusMultiplier(NetworkState state, NetworkFormation type, int nodeIndex = -1)
        {
            if (state == null) return 0f;
            if (state.Dominant != null && state.Dominant.Type == type && ContainsNode(state.Dominant, nodeIndex)) return 1f;
            if (state.Secondary != null && state.Secondary.Type == type && ContainsNode(state.Secondary, nodeIndex)) return 0.5f;
            return 0f;
        }

        public static NetworkProfile GetProfile(NetworkState state, int nodeIndex)
        {
            var profile = new NetworkProfile();
            if (state == null) return profile;
            for (var i = 0; i < state.Links.Count; i++)
                if (state.Links[i].A == nodeIndex || state.Links[i].B == nodeIndex) profile.LinkedNeighbours++;
            profile.Line = ContainsNode(state, NetworkFormation.Line, nodeIndex);
            profile.Triangle = ContainsNode(state, NetworkFormation.Triangle, nodeIndex);
            profile.Square = ContainsNode(state, NetworkFormation.Square, nodeIndex);
            profile.Cluster = ContainsNode(state, NetworkFormation.Cluster, nodeIndex);
            profile.Ring = ContainsNode(state, NetworkFormation.Ring, nodeIndex);
            profile.Lattice = ContainsNode(state, NetworkFormation.Lattice, nodeIndex);
            profile.Fractal = ContainsNode(state, NetworkFormation.Fractal, nodeIndex);
            return profile;
        }

        private static bool ContainsNode(NetworkShape shape, int index) => shape != null && (index < 0 || shape.Nodes.Contains(index));
        private static bool ContainsNode(NetworkState state, NetworkFormation type, int index)
        {
            if (state.Dominant != null && state.Dominant.Type == type && ContainsNode(state.Dominant, index)) return true;
            return state.Secondary != null && state.Secondary.Type == type && ContainsNode(state.Secondary, index);
        }
        private static void Add(NetworkState state, NetworkShape shape) { if (shape != null) state.Candidates.Add(shape); }
        private static float Distance(NetworkNode a, NetworkNode b)
        {
            var dx = a.X - b.X; var dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
        private static List<int> Copy(IList<int> indexes)
        {
            var result = new List<int>(indexes.Count);
            for (var i = 0; i < indexes.Count; i++) result.Add(indexes[i]);
            return result;
        }
        private static void Combinations3(IList<int> values, Action<int,int,int> action)
        {
            for (var i = 0; i < values.Count - 2; i++)
            for (var j = i + 1; j < values.Count - 1; j++)
            for (var k = j + 1; k < values.Count; k++) action(values[i], values[j], values[k]);
        }
        private static void Combinations4(IList<int> values, Action<int,int,int,int> action)
        {
            for (var i = 0; i < values.Count - 3; i++)
            for (var j = i + 1; j < values.Count - 2; j++)
            for (var k = j + 1; k < values.Count - 1; k++)
            for (var m = k + 1; m < values.Count; m++) action(values[i], values[j], values[k], values[m]);
        }
        private static float LineStrength(IList<NetworkNode> nodes, IList<int> indexes)
        {
            if (indexes.Count < 3) return 0f;
            var cx = 0f; var cy = 0f;
            for (var i = 0; i < indexes.Count; i++) { cx += nodes[indexes[i]].X; cy += nodes[indexes[i]].Y; }
            cx /= indexes.Count; cy /= indexes.Count;
            var best = 0f;
            for (var step = 0; step < 24; step++)
            {
                var angle = step / 24f * (float)Math.PI;
                var dx = (float)Math.Cos(angle); var dy = (float)Math.Sin(angle); var maxError = 0f;
                for (var i = 0; i < indexes.Count; i++)
                {
                    var n = nodes[indexes[i]];
                    maxError = Math.Max(maxError, Math.Abs((n.X - cx) * dy - (n.Y - cy) * dx));
                }
                best = Math.Max(best, 1f - Math.Min(1f, maxError / 1.44f));
            }
            return best;
        }
        private static NetworkShape FindLine(IList<NetworkNode> nodes, List<int> indexes)
        {
            if (indexes.Count < 3) return null;
            var whole = LineStrength(nodes, indexes);
            if (whole >= 0.8f) return new NetworkShape(NetworkFormation.Line, whole, Copy(indexes));
            NetworkShape best = null;
            Combinations3(indexes, (a,b,c) =>
            {
                var trio = new List<int> { a,b,c }; var strength = LineStrength(nodes, trio);
                if (strength >= 0.8f && (best == null || strength > best.Strength)) best = new NetworkShape(NetworkFormation.Line, strength, trio);
            });
            return best;
        }
        private static float TriangleStrength(IList<NetworkNode> nodes, int a, int b, int c, float linkDistance)
        {
            var ab = Distance(nodes[a], nodes[b]); var bc = Distance(nodes[b], nodes[c]); var ca = Distance(nodes[c], nodes[a]);
            if (ab > linkDistance || bc > linkDistance || ca > linkDistance) return 0f;
            var mean = (ab + bc + ca) / 3f;
            if (mean < 1.3f || mean > linkDistance) return 0f;
            var variance = (Math.Abs(ab - mean) + Math.Abs(bc - mean) + Math.Abs(ca - mean)) / (3f * mean);
            return 1f - Math.Min(1f, variance * 2.2f);
        }
        private static NetworkShape FindTriangle(IList<NetworkNode> nodes, List<int> indexes, float linkDistance)
        {
            NetworkShape best = null;
            Combinations3(indexes, (a,b,c) =>
            {
                var strength = TriangleStrength(nodes,a,b,c,linkDistance);
                if (strength >= 0.82f && (best == null || strength > best.Strength)) best = new NetworkShape(NetworkFormation.Triangle,strength,new List<int>{a,b,c});
            });
            return best;
        }
        private static NetworkShape FindSquare(IList<NetworkNode> nodes, List<int> indexes, float linkDistance)
        {
            NetworkShape best = null;
            Combinations4(indexes, (a,b,c,d) =>
            {
                var ids = new List<int>{a,b,c,d}; var cx=0f; var cy=0f;
                for (var i=0;i<4;i++){cx+=nodes[ids[i]].X;cy+=nodes[ids[i]].Y;} cx/=4f;cy/=4f;
                ids.Sort((i,j)=>Math.Atan2(nodes[i].Y-cy,nodes[i].X-cx).CompareTo(Math.Atan2(nodes[j].Y-cy,nodes[j].X-cx)));
                var sides=new float[4]; var sideMean=0f;
                for(var i=0;i<4;i++){sides[i]=Distance(nodes[ids[i]],nodes[ids[(i+1)%4]]);sideMean+=sides[i];} sideMean/=4f;
                var diag1=Distance(nodes[ids[0]],nodes[ids[2]]);var diag2=Distance(nodes[ids[1]],nodes[ids[3]]);
                if(sideMean<1.2f||sideMean>linkDistance||Array.Exists(sides,x=>x>linkDistance))return;
                var sideVariance=0f;for(var i=0;i<4;i++)sideVariance+=Math.Abs(sides[i]-sideMean);sideVariance/=4f*sideMean;
                var diagonalMean=(diag1+diag2)/2f;var diagonalVariance=Math.Abs(diag1-diag2)/Math.Max(1f,diagonalMean);
                var rightAngleError=Math.Abs(diagonalMean/sideMean-1.41421356f)/1.41421356f;
                var strength=Math.Max(0f,1f-Math.Min(1f,sideVariance*2f+diagonalVariance+rightAngleError));
                if(strength>=0.84f&&(best==null||strength>best.Strength))best=new NetworkShape(NetworkFormation.Square,strength,ids);
            });
            return best;
        }
        private static NetworkShape FindCluster(IList<NetworkNode> nodes, List<int> indexes, float linkDistance)
        {
            if(indexes.Count<4)return null;
            var sum=0f;var pairs=0;var linked=0;
            for(var i=0;i<indexes.Count;i++)for(var j=i+1;j<indexes.Count;j++){var d=Distance(nodes[indexes[i]],nodes[indexes[j]]);sum+=d;pairs++;if(d<=linkDistance)linked++;}
            var average=sum/Math.Max(1,pairs);var compact=1f-Math.Min(1f,Math.Max(0f,average-1.8f)/1.8f);
            var strength=compact*0.65f+(linked/(float)Math.Max(1,pairs))*0.35f;
            return strength>=0.78f?new NetworkShape(NetworkFormation.Cluster,strength,Copy(indexes)):null;
        }
        private static NetworkShape FindRing(IList<NetworkNode> nodes,List<int> indexes,float linkDistance)
        {
            if(indexes.Count<4)return null;
            var cx=0f;var cy=0f;for(var i=0;i<indexes.Count;i++){cx+=nodes[indexes[i]].X;cy+=nodes[indexes[i]].Y;}cx/=indexes.Count;cy/=indexes.Count;
            var ordered=Copy(indexes);ordered.Sort((a,b)=>Math.Atan2(nodes[a].Y-cy,nodes[a].X-cx).CompareTo(Math.Atan2(nodes[b].Y-cy,nodes[b].X-cx)));
            var mean=0f;var ds=new float[ordered.Count];for(var i=0;i<ordered.Count;i++){ds[i]=Distance(nodes[ordered[i]],nodes[ordered[(i+1)%ordered.Count]]);if(ds[i]>linkDistance)return null;mean+=ds[i];}mean/=ds.Length;
            if(mean<1.1f)return null;var variance=0f;for(var i=0;i<ds.Length;i++)variance+=Math.Abs(ds[i]-mean);variance/=ds.Length*mean;
            var strength=1f-Math.Min(1f,variance*2f);
            return strength>=0.68f?new NetworkShape(NetworkFormation.Ring,strength,ordered):null;
        }
        private static bool Linked(List<NetworkLink> links,int a,int b){for(var i=0;i<links.Count;i++)if((links[i].A==a&&links[i].B==b)||(links[i].A==b&&links[i].B==a))return true;return false;}
        private static NetworkShape FindLattice(IList<NetworkNode> nodes,List<int> indexes,List<NetworkLink> links,float linkDistance)
        {
            if(indexes.Count<4)return null;var triangles=new List<List<int>>();
            Combinations3(indexes,(a,b,c)=>{if(Linked(links,a,b)&&Linked(links,b,c)&&Linked(links,a,c)&&TriangleStrength(nodes,a,b,c,linkDistance)>=0.82f)triangles.Add(new List<int>{a,b,c});});
            if(triangles.Count<2)return null;var shared=false;
            for(var i=0;i<triangles.Count&&!shared;i++)for(var j=i+1;j<triangles.Count&&!shared;j++){var common=0;for(var k=0;k<3;k++)if(triangles[j].Contains(triangles[i][k]))common++;if(common>=2)shared=true;}
            if(!shared)return null;var used=new HashSet<int>();foreach(var tri in triangles)foreach(var n in tri)used.Add(n);
            return new NetworkShape(NetworkFormation.Lattice,Math.Min(1f,0.55f+Math.Min(0.45f,triangles.Count*0.08f)),new List<int>(used));
        }
        private static NetworkShape FindFractal(NetworkState state,params NetworkShape[] baseShapes)
        {
            var shapes=new List<NetworkShape>();foreach(var shape in baseShapes)if(shape!=null)shapes.Add(shape);
            if(shapes.Count<2)return null;var used=new HashSet<int>();var sum=0f;foreach(var shape in shapes){sum+=shape.Strength;foreach(var n in shape.Nodes)used.Add(n);}
            var nodes=new List<int>(used);if(nodes.Count<6||!IsConnected(nodes,state.Links))return null;
            return new NetworkShape(NetworkFormation.Fractal,Math.Min(1f,sum/shapes.Count),nodes);
        }
        private static bool IsConnected(List<int> indexes,List<NetworkLink> links)
        {
            if(indexes.Count<=1)return true;var allowed=new HashSet<int>(indexes);var visited=new HashSet<int>{indexes[0]};var queue=new Queue<int>();queue.Enqueue(indexes[0]);
            while(queue.Count>0){var cur=queue.Dequeue();for(var i=0;i<links.Count;i++){var next=links[i].A==cur?links[i].B:links[i].B==cur?links[i].A:-1;if(next>=0&&allowed.Contains(next)&&visited.Add(next))queue.Enqueue(next);}}
            return visited.Count==allowed.Count;
        }
        private static NetworkShape FindSecondary(List<NetworkShape> candidates,NetworkShape dominant,bool disjointOnly)
        {
            var dominantNodes=new HashSet<int>(dominant.Nodes);
            for(var i=0;i<candidates.Count;i++){var candidate=candidates[i];if(candidate.Type==dominant.Type)continue;var gap=dominant.DominanceScore-candidate.DominanceScore;
                if(disjointOnly){if(gap>SecondaryDisjointScoreGap)continue;var overlap=false;foreach(var n in candidate.Nodes)if(dominantNodes.Contains(n)){overlap=true;break;}if(!overlap)return candidate;}
                else if(gap<=SecondaryScoreGap)return candidate;
            }return null;
        }
    }
}