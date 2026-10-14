using System.Collections.Generic;
namespace EchoSphere.Core
{
    // Source for-of arrays allow deaths and new spawns during an area effect.
    // List<T>'s fail-fast enumerator cannot represent that source behaviour.
    public sealed class SourceEntityList<T> : List<T>, IEnumerable<T>, System.Collections.IEnumerable
    {
        public new IEnumerator<T> GetEnumerator()
        { for (var index = 0; index < Count; index++) yield return this[index]; }
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
