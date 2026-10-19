using System;
using System.Collections.Generic;

namespace EchoSphere.Presentation
{
    // Display-only inputs for the native PausePlanner stats tab. No gameplay
    // calculations, CSS nodes or view construction belong to this model.
    public sealed class NativePauseStatsState
    {
        public readonly struct Stat
        {
            public readonly string Label,Value;
            public Stat(string label,string value){Label=label;Value=value;}
        }
        public sealed class Sphere
        {
            public string Id,Color,Name,Level,Placement,Description,BranchLabel,BranchName,FinalName;
            public bool OnField,HasBranch;
            public IReadOnlyList<Stat> Metrics=Array.Empty<Stat>();
        }
        public bool Russian;
        public string Kicker,Title,Subtitle,CharacterHeading,SpheresHeading,Resume,Return;
        public IReadOnlyList<Stat> Stats=Array.Empty<Stat>();
        public IReadOnlyList<Sphere> Spheres=Array.Empty<Sphere>();
    }
}
