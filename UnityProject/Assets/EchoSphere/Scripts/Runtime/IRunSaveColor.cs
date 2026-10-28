using UnityEngine;

namespace EchoSphere.Runtime
{
    // Compatibility with SavedEntity.color. The simulation owns this value;
    // saving and loading must not require a hidden scene renderer.
    internal interface IRunSaveColor
    {
        Color SavedColor { get; }
        void RestoreSavedColor(Color color);
    }
}
