using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    internal static class SourceUiCursor
    {
        [Serializable] private sealed class Reference { public int hotspotX=0; public int hotspotY=0; }
        private static Texture2D _pointer;
        private static Vector2 _hotspot;
        internal static void Apply(IStyle style,string cursor)
        {
            if(cursor!="pointer"){style.cursor=new StyleCursor(default(UnityEngine.UIElements.Cursor));return;}
            if(_pointer==null){
                _pointer=Resources.Load<Texture2D>("EchoSphere/SourceUiControls/cursor-pointer");
                var reference=JsonUtility.FromJson<Reference>(Resources.Load<TextAsset>("EchoSphere/SourceUiControls/cursor-reference").text);
                _hotspot=new Vector2(reference.hotspotX,reference.hotspotY);
            }
            style.cursor=new StyleCursor(new UnityEngine.UIElements.Cursor {texture=_pointer,hotspot=_hotspot});
        }
    }
}
