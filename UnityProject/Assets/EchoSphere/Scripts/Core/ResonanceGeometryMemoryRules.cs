using System;
using System.Collections.Generic;

namespace EchoSphere.Core
{
    /// <summary>
    /// Source-mapped update semantics for engineResonance.ts:syncResonanceGeometry().
    /// It tracks key/candidate-set changes independently of pointer-drag preview memory.
    /// </summary>
    public static class ResonanceGeometryMemoryRules
    {
        public const float MemoryDurationSeconds = 0.9f;

        public static bool ShouldRefreshGeometry(
            string previousKey, string currentKey,
            IList<string> previousCandidateKeys, IList<string> currentCandidateKeys)
        {
            if (!string.Equals(previousKey ?? "none", currentKey ?? "none", StringComparison.Ordinal))
                return true;
            previousCandidateKeys = previousCandidateKeys ?? Array.Empty<string>();
            currentCandidateKeys = currentCandidateKeys ?? Array.Empty<string>();
            if (currentCandidateKeys.Count != previousCandidateKeys.Count) return true;
            for (var i = 0; i < currentCandidateKeys.Count; i++)
            {
                var found = false;
                for (var j = 0; j < previousCandidateKeys.Count; j++)
                {
                    if (!string.Equals(currentCandidateKeys[i], previousCandidateKeys[j], StringComparison.Ordinal))
                        continue;
                    found = true;
                    break;
                }
                if (!found) return true;
            }
            return false;
        }

        public static bool ShouldCapturePreviousGeometry(string previousKey, int previousNodeCount) =>
            !string.IsNullOrEmpty(previousKey) &&
            !string.Equals(previousKey, "none", StringComparison.Ordinal) &&
            previousNodeCount > 0;

        public static bool ShouldStoreMemory(int previousPointCount) => previousPointCount >= 2;
    }
}
