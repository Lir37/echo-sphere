using System;
using System.Collections.Generic;

namespace EchoSphere.Presentation
{
    // Display inputs only; definitions, evolution choices and descriptions come
    // from the same existing helpers as the original PausePlanner.
    public sealed class NativePauseAbilitiesState
    {
        public sealed class Ability
        {
            public string Id,Name,Level,Description,Branch,Final;
            public bool Acquired,HasProgression;
        }
        public string ActiveHeading,PassiveHeading;
        public IReadOnlyList<Ability> Active=Array.Empty<Ability>();
        public IReadOnlyList<Ability> Passive=Array.Empty<Ability>();
    }
}
