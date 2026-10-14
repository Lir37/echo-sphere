namespace EchoSphere.Core
{
    /// <summary>Deterministic port of the TypeScript run RNG, including the source's particle draws. Unity-only presentation uses a separate RNG.</summary>
    public sealed class SeededRng
    {
        private uint _state;
        public uint State => _state;
        public SeededRng(uint seed) { _state = seed == 0u ? 1u : seed; }
        public void Reset(uint seed) { _state = seed == 0u ? 1u : seed; }
        public void RestoreState(uint state) { _state = state; }
        public float NextFloat()
        {
            unchecked
            {
                _state += 0x6D2B79F5u;
                uint t = _state;
                t = (t ^ (t >> 15)) * (t | 1u);
                t ^= t + ((t ^ (t >> 7)) * (t | 61u));
                return (t ^ (t >> 14)) / 4294967296f;
            }
        }
        /// <summary>Matches src/region.ts nextR(), which advances the shared run RNG with the regional LCG.</summary>
        public float NextRegionFloat()
        {
            unchecked { _state = _state * 1664525u + 1013904223u; }
            return _state / 4294967296f;
        }

        public int NextInt(int maxExclusive) => maxExclusive <= 0 ? 0 : System.Math.Min(maxExclusive - 1, (int)(NextFloat() * maxExclusive));
    }
}
