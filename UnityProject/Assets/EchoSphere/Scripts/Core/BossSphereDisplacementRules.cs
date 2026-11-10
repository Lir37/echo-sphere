using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    // Source-pixel coordinates, deterministic contact response; never consumes RNG.
    public static class BossSphereDisplacementRules
    {
        public const float MaximumPushSpeed = 120f;
        public sealed class Workspace
        {
            internal float[] Remaining = Array.Empty<float>();
            internal bool[] Moved = Array.Empty<bool>();
        }

        public static bool Resolve(IReadOnlyList<CollisionBody> bodies, IList<Vec2> spheres,
            float dt, Workspace workspace, float width = 2400f, float height = 2400f)
        {
            if (dt <= 0f || bodies == null || spheres == null || workspace == null) return false;
            if (workspace.Remaining.Length < spheres.Count)
            {
                workspace.Remaining = new float[spheres.Count];
                workspace.Moved = new bool[spheres.Count];
            }
            for (int i = 0; i < spheres.Count; i++)
            {
                workspace.Remaining[i] = MaximumPushSpeed * dt;
                workspace.Moved[i] = false;
            }
            bool changed = false;
            bool Move(int index, Vec2 delta)
            {
                var before = spheres[index];
                var candidate = before + delta;
                var x = Math.Max(0f, width * .5f - 20f);
                var y = Math.Max(0f, height * .5f - 20f);
                candidate = new Vec2(Math.Max(-x, Math.Min(x, candidate.X)), Math.Max(-y, Math.Min(y, candidate.Y)));
                delta = candidate - before;
                float length = delta.Length, budget = workspace.Remaining[index];
                if (length < .00001f || budget <= 0f) return false;
                float travel = Math.Min(length, budget);
                spheres[index] = before + delta * (travel / length);
                workspace.Remaining[index] -= travel;
                workspace.Moved[index] = true;
                changed = true;
                return true;
            }
            for(int bodyIndex=0;bodyIndex<bodies.Count;bodyIndex++)
            {
                var boss=bodies[bodyIndex];
                float travel = boss.SpherePushMotion.Length;
                if (!boss.PushesSpheres || travel <= .00001f) continue;
                for (int i = 0; i < spheres.Count; i++)
                {
                    var outward = spheres[i] - boss.Position;
                    float distance = outward.Length, overlap = boss.Radius + 20f - distance;
                    if (overlap <= 0f || Vec2.Dot(outward, boss.SpherePushMotion) < -.00001f) continue;
                    var normal = distance > .00001f ? outward / distance : boss.SpherePushMotion / travel;
                    float step=Math.Min(overlap,travel*1.5f);
                    if(!Move(i,normal*step)){
                        var tangent=new Vec2(-normal.Y,normal.X);
                        if(Vec2.Dot(tangent,Vec2.Zero-spheres[i])<0f)tangent=tangent*-1f;
                        if(!Move(i,tangent*step))Move(i,tangent*-step);
                    }
                }
            }
            if (!changed) return false;
            // Propagate compression only from touched nodes; leave old close saves untouched elsewhere.
            for (int pass = 0; pass < spheres.Count; pass++)
            {
                bool separated = false;
                for (int i = 0; i < spheres.Count; i++) for (int j = i + 1; j < spheres.Count; j++)
                {
                    if (!workspace.Moved[i] && !workspace.Moved[j]) continue;
                    var delta = spheres[j] - spheres[i]; float distance = delta.Length;
                    float overlap = SpherePlacementRules.EditingMinimumSphereDistance / SpherePlacementRules.SourcePixelsToWorld - distance;
                    if (overlap <= .0001f) continue;
                    var normal = distance > .00001f ? delta / distance : new Vec2(1f, 0f);
                    bool movedI=workspace.Moved[i],movedJ=workspace.Moved[j];bool both = movedI && movedJ;
                    if (movedI) separated |= Move(j, normal * (both ? overlap * .5f : overlap));
                    if (movedJ) separated |= Move(i, normal * -(both ? overlap * .5f : overlap));
                }
                if (!separated) break;
            }
            return changed;
        }
    }
}
