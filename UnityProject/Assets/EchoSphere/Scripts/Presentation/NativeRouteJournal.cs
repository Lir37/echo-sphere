using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoSphere.Presentation
{
    // A bounded event journal, intentionally outside the saved runtime schema.
    internal sealed class NativeRouteJournal
    {
        private readonly Queue<string> _events=new Queue<string>(64);
        private bool _seen,_menu,_region,_paused;private int _page,_tutorial,_scene;
        internal bool RegionLaunchPending;
        internal string[] Events=>_events.ToArray();
        internal void Observe(int runtime,int scene,bool menu,bool region,int page,int tutorial,bool paused,string reason)
        {
            bool changed=!_seen||scene!=_scene||menu!=_menu||region!=_region||page!=_page||tutorial!=_tutorial||paused!=_paused;
            if(!changed&&reason=="state")return;
            _seen=true;_scene=scene;_menu=menu;_region=region;_page=page;_tutorial=tutorial;_paused=paused;
            var documents=new System.Text.StringBuilder();
            bool HasContent(VisualElement element){
                if(element.style.visibility==Visibility.Hidden||element.style.display==DisplayStyle.None||(element.style.opacity.keyword==StyleKeyword.Undefined&&element.style.opacity.value<=0))return false;
                if(element is TextElement text&&!string.IsNullOrEmpty(text.text)||element is Image image&&(image.image!=null||image.sprite!=null||image.vectorImage!=null)||element.style.backgroundColor.value.a>0)return true;
                for(int i=0;i<element.hierarchy.childCount;i++)if(HasContent(element.hierarchy[i]))return true;return false;
            }
            foreach(var document in UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None)){
                var root=document.rootVisualElement;if(root==null||!HasContent(root))continue;
                if(documents.Length>0)documents.Append(',');documents.Append(document.gameObject.name).Append('#').Append(document.GetInstanceID());
            }
            string entry=$"[EchoSphere.Route] frame={Time.frameCount} runtime={runtime} scene={scene} menu={menu} region={region} page={page} tutorial={tutorial} paused={paused} documents=[{documents}] reason={reason}";
            if(_events.Count==64)_events.Dequeue();_events.Enqueue(entry);Debug.Log(entry);
        }
    }
}
