// Read-only view inspection. Outputs only UI text/styles/geometry, never save data.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Play required");
UnityEditor.EditorApplication.isPaused=true;
var ui=UnityEngine.Object.FindFirstObjectByType<EchoSphere.Presentation.SourceUiDocument>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var root=(EchoSphere.Presentation.VisualCssNode)ui.GetType().GetField("_root",flags).GetValue(ui);
var rows=new System.Collections.Generic.List<object>();int index=0;
void Walk(EchoSphere.Presentation.VisualCssNode node,bool included,int parent){
 var classes=node.Attribute("className")??"";var selected=included||classes.Split(' ').Contains("es-hud-panel");var id=index++;
 if(selected){var e=node.Native as UnityEngine.UIElements.VisualElement;rows.Add(new{id,parent,tag=node.Tag,text=node.Text,classes,style=node.Style,bounds=new{x=node.X,y=node.Y,w=node.Width,h=node.Height},native=e==null?null:new{x=e.worldBound.x,y=e.worldBound.y,w=e.worldBound.width,h=e.worldBound.height}});}
 foreach(var child in node.Children)Walk(child,selected,id);
}
Walk(root,false,-1);var path=System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath,"../Temp/gameplay-ui-reference.json"));System.IO.File.WriteAllText(path,Newtonsoft.Json.JsonConvert.SerializeObject(rows,Newtonsoft.Json.Formatting.Indented));return new{nodes=rows.Count,path};
