using UnityEngine.UIElements;
namespace EchoSphere.Presentation {
 // Unity 6.6 hierarchy moves invalidate retained render data even when the
 // requested sibling order is already satisfied. Only reorder actual changes.
 internal static class NativeUiOrder {
  internal static void InFront(VisualElement element,VisualElement sibling){
   var parent=element.parent;
   if(parent!=null&&sibling!=null&&sibling.parent==parent){int current=parent.IndexOf(element),anchor=parent.IndexOf(sibling);int destination=current>anchor?anchor+1:anchor;if(current==destination)return;}
   element.PlaceInFront(sibling);
  }
  internal static void Last(VisualElement element){var parent=element.parent;if(parent!=null&&parent.IndexOf(element)==parent.childCount-1)return;element.BringToFront();}
 }
}
