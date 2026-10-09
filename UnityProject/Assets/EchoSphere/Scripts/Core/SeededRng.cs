namespace EchoSphere.Core
{
    /// <summary>Deterministic port of the current TypeScript run RNG. Cosmetic VFX must not consume it.</summary>
    public sealed class SeededRng
    {
        private uint _state;
        public uint State => _state;
        public SeededRng(uint seed) { _state = seed == 0u ? 1u : seed; }
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
        public int NextInt(int maxExclusive) => maxExclusive <= 0 ? 0 : (int)(NextFloat() * maxExclusive);
    }
}
