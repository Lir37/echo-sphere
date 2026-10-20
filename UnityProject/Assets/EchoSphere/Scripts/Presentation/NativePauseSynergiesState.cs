using System;
using System.Collections.Generic;

namespace EchoSphere.Presentation
{
    // Display values only. The existing source helpers own all gameplay conditions.
    public sealed class NativePauseSynergiesState
    {
        public sealed class Tag
        {
            public string Id, Text;
            public bool Owned;
        }
        public sealed class Card
        {
            public string Id, Name, Description, Status, Hint;
            public string Tone;
            public bool Pulse, Diagnostic;
            public IReadOnlyList<Tag> Tags = Array.Empty<Tag>();
        }
        public sealed class Section
        {
            public string Id, Heading;
            public IReadOnlyList<Card> Cards = Array.Empty<Card>();
        }
        public IReadOnlyList<Section> Sections = Array.Empty<Section>();
        public string EmptySignals;
    }
}
