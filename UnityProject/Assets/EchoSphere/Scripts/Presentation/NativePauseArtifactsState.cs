using System;
using System.Collections.Generic;

namespace EchoSphere.Presentation
{
    public sealed class NativePauseArtifactsState
    {
        public sealed class Set
        {
            public string Id,Name,Count,Status;
            public int Owned,Total;
            public bool Complete;
        }
        public sealed class Artifact
        {
            public string Id,Name,Description,Rarity,RarityLabel,BorderColor,GlyphColor,Glyph;
            public IReadOnlyList<Set> Sets=Array.Empty<Set>();
        }
        public string Heading,Empty;
        public IReadOnlyList<Artifact> Artifacts=Array.Empty<Artifact>();
    }
}
