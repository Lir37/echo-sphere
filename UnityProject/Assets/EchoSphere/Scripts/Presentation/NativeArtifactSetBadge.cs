using System;
using UnityEngine;
using UnityEngine.UIElements;
using static EchoSphere.Presentation.NativePauseStatsView;

namespace EchoSphere.Presentation
{
    public sealed class NativeArtifactSetBadge : IDisposable
    {
        private static StyleSheet _sheet;
        private readonly Button _root;
        private readonly VisualElement _ring;
        private readonly Label _count,_name,_status;
        private readonly Action<string> _select;
        private string _id;
        private int _owned,_total;
        private bool _disposed;
        public Button Root=>_root;
        public NativeArtifactSetBadge(VisualElement parent,Action<string> select)
        {
            if(parent==null)throw new ArgumentNullException(nameof(parent));_select=select;
            _root=new Button(Select) {name="native-artifact-set-badge"};_root.AddToClassList("native-artifact-set-badge");
            if(_sheet==null)_sheet=Resources.Load<StyleSheet>("EchoSphere/NativeArtifactSetBadge");if(_sheet==null)throw new InvalidOperationException("Native artifact set badge stylesheet missing");_root.styleSheets.Add(_sheet);parent.Add(_root);
            _root.RegisterCallback<ClickEvent>(Click);
            _ring=Box(_root,"artifact-set-ring");_ring.AddToClassList("native-artifact-set-ring");_ring.generateVisualContent+=DrawRing;
            var hole=Box(_ring,"artifact-set-hole");hole.AddToClassList("native-artifact-set-hole");
            _count=Text(hole,"artifact-set-count",7,"#ffcf7a","900");
            var body=Box(_root,"artifact-set-labels");body.style.flexShrink=1;
            _name=Text(body,"artifact-set-name",8,"#cfe7f3","700",spacing:.4f);_name.style.maxWidth=150;_name.style.overflow=Overflow.Hidden;_name.style.textOverflow=TextOverflow.Ellipsis;
            _status=Text(body,"artifact-set-status",7,"#64849b");
            _name.style.unityTextAlign=_status.style.unityTextAlign=TextAnchor.UpperLeft;
        }
        private void Click(ClickEvent evt){evt.StopPropagation();}
        private void Select(){if(_id!=null)_select?.Invoke(_id);}
        public void Bind(NativePauseArtifactsState.Set state)
        {
            if(_disposed)throw new ObjectDisposedException(nameof(NativeArtifactSetBadge));if(state==null)throw new ArgumentNullException(nameof(state));
            _id=state.Id;_root.tooltip=state.Name;Set(_count,state.Count);Set(_name,state.Name.ToUpperInvariant());Set(_status,state.Status);
            if(_owned!=state.Owned||_total!=state.Total){_owned=state.Owned;_total=state.Total;_ring.MarkDirtyRepaint();}
        }
        private void DrawRing(MeshGenerationContext context)
        {
            // The conic fills the padding box; the 3px padding sizes the hole.
            var center=new Vector2(_ring.layout.width*.5f,_ring.layout.height*.5f);float radius=Mathf.Min(_ring.layout.width,_ring.layout.height)*.5f;if(radius<=0)return;
            var p=context.painter2D;p.fillColor=new Color(36/255f,59/255f,85/255f,.65f);p.BeginPath();p.Arc(center,radius,0,360);p.ClosePath();p.Fill();
            float pct=_total>0?(float)_owned/_total:0;if(pct<=0)return;p.fillColor=new Color(1,184/255f,77/255f,1);p.BeginPath();p.MoveTo(center);p.LineTo(center+new Vector2(0,-radius));p.Arc(center,radius,-90,-90+pct*360);p.ClosePath();p.Fill();
        }
        public void Dispose(){if(_disposed)return;_disposed=true;_root.clicked-=Select;_root.UnregisterCallback<ClickEvent>(Click);_ring.generateVisualContent-=DrawRing;_root.RemoveFromHierarchy();}
    }
}
