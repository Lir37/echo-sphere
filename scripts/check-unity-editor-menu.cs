// Run with Unity CLI eval-file against the open Editor (no APK required).
var assembly = typeof(EchoSphere.Presentation.SourceUiDocument).Assembly;
var instanceFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
var staticFlags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
var replicaType = assembly.GetType("EchoSphere.Presentation.SourceUiReplica");
var painter = new System.Func<UnityEngine.UIElements.VisualElement, System.Action<UnityEngine.UIElements.MeshGenerationContext>>(element => null);
var replica = System.Activator.CreateInstance(replicaType, instanceFlags, null, new object[] { painter }, null);
var update = replicaType.GetMethod("Update", instanceFlags);
var root = new UnityEngine.UIElements.VisualElement();
var stop = new UnityEngine.UIElements.VisualElement();
var parent = new UnityEngine.UIElements.VisualElement();
var nodes = new UnityEngine.UIElements.VisualElement[6];
for (int i = 0; i < nodes.Length; i++) { nodes[i] = new UnityEngine.UIElements.VisualElement(); root.Add(nodes[i]); }
root.Add(stop);
var rng = new System.Random(531);
int orderChecks = 0;
try {
    for (int pass = 0; pass < 150; pass++) {
        for (int i = 0; i < 6; i++) nodes[rng.Next(6)].BringToFront();
        stop.BringToFront();
        if (pass % 3 == 0) nodes[pass % 6].RemoveFromHierarchy();
        else if (nodes[pass % 6].parent == null) root.Insert(0, nodes[pass % 6]);
        for (int repeat = 0; repeat < 2; repeat++) {
            update.Invoke(replica, new object[] { root, stop, parent, new UnityEngine.Rect(0, 0, 100, 100) });
            var copies = (System.Collections.IDictionary)replicaType.GetField("_copies", instanceFlags).GetValue(replica);
            var rootCopy = copies[root];
            var target = (UnityEngine.UIElements.VisualElement)rootCopy.GetType().GetField("Element", instanceFlags).GetValue(rootCopy);
            if (target.childCount != root.childCount - 1) throw new System.Exception("Replica count mismatch");
            for (int i = 0; i < target.childCount; i++) {
                var expected = (UnityEngine.UIElements.VisualElement)copies[root[i]].GetType().GetField("Element", instanceFlags).GetValue(copies[root[i]]);
                if (target[i] != expected) throw new System.Exception("Replica paint order mismatch");
                orderChecks++;
            }
        }
    }
} finally { ((System.IDisposable)replica).Dispose(); }

var appearance = replicaType.GetMethod("Appearance", staticFlags);
var inline = replicaType.GetMethod("InlineGeometry", staticFlags);
int styleChecks = 0;
for (int pass = 0; pass < 20; pass++) {
    var source = new UnityEngine.UIElements.Label("Replica text");
    source.style.width = 123 + pass; source.style.height = 47 + pass;
    source.style.color = UnityEngine.Color.cyan; source.style.opacity = pass / 20f;
    source.style.paddingLeft = pass; source.style.fontSize = 19;
    source.style.rotate = new UnityEngine.UIElements.Rotate(pass);
    var oldTarget = new UnityEngine.UIElements.Label();
    var newTarget = new UnityEngine.UIElements.Label();
    appearance.Invoke(null, new object[] { source, oldTarget });
    inline.Invoke(null, new object[] { source, oldTarget });
    inline.Invoke(null, new object[] { source, newTarget });
    foreach (var property in typeof(UnityEngine.UIElements.IStyle).GetProperties()) {
        if (!property.CanRead || property.GetIndexParameters().Length > 0) continue;
        if (!object.Equals(property.GetValue(oldTarget.style), property.GetValue(newTarget.style)))
            throw new System.Exception("Helper style mismatch: " + property.Name);
        styleChecks++;
    }
}

int indexChecks = 0;
foreach (var name in new[] { "character-spherist-front", "character-spherist-3q" }) {
    var sprite = UnityEngine.Resources.Load<UnityEngine.Sprite>("EchoSphere/Art/" + name);
    if (sprite == null) throw new System.Exception("Missing imported sprite: " + name);
    var triangles = sprite.triangles;
    var indices = new int[triangles.Length];
    for (int i = 0; i < indices.Length; i++) indices[i] = triangles[i];
    var independentRead = sprite.triangles;
    for (int i = 0; i < indices.Length; i++) {
        if (indices[i] != independentRead[i]) throw new System.Exception("Sprite index mismatch");
        indexChecks++;
    }
}
var sheet = new EchoSphere.Presentation.VisualCss(EchoSphere.Presentation.VisualJson.Parse("{\"rules\":[],\"keyframes\":{}}"));
var layoutType = assembly.GetType("EchoSphere.Presentation.SourceUiLayout");
var layout = System.Activator.CreateInstance(layoutType, new object[] { sheet });
var nativeColumn = layoutType.GetMethod("NativeAutoColumn", instanceFlags);
var grid = new EchoSphere.Presentation.VisualCssNode { Tag = "div" };
grid.Style["display"] = "grid"; grid.Style["grid-template-columns"] = "1fr";
var cell = new EchoSphere.Presentation.VisualCssNode { Tag = "div", Parent = grid };
grid.Children.Add(cell);
if (!(bool)nativeColumn.Invoke(layout, new object[] { grid })) throw new System.Exception("Intrinsic column should use native layout");
int gridChecks = 1;
foreach (var pair in new[] {
    new[] { "grid-template-columns", "repeat(3,1fr)" }, new[] { "grid-template-rows", "auto 1fr auto" },
    new[] { "height", "200px" }, new[] { "min-height", "20px" }, new[] { "max-height", "100%" },
    new[] { "align-items", "center" }, new[] { "justify-items", "end" }, new[] { "align-content", "center" }
}) {
    grid.Style.TryGetValue(pair[0], out var previous);
    grid.Style[pair[0]] = pair[1];
    if ((bool)nativeColumn.Invoke(layout, new object[] { grid })) throw new System.Exception("Specialized grid wrongly accepted: " + pair[0]);
    if (previous == null) grid.Style.Remove(pair[0]); else grid.Style[pair[0]] = previous;
    gridChecks++;
}
foreach (var property in new[] { "align-self", "justify-self", "grid-row", "grid-column", "z-index" }) {
    cell.Style[property] = property == "z-index" ? "2" : "center";
    if ((bool)nativeColumn.Invoke(layout, new object[] { grid })) throw new System.Exception("Specialized item wrongly accepted: " + property);
    cell.Style.Remove(property); gridChecks++;
}
var centered = layoutType.GetMethod("NativeCenteredCell", instanceFlags);
grid.Style.Clear(); grid.Style["display"]="grid";
grid.Style["align-items"]="center"; grid.Style["justify-items"]="center";
grid.Style["justify-content"]="center";
if (!(bool)centered.Invoke(layout,new object[]{grid})) throw new System.Exception("Expanded centered cell should use native alignment");
gridChecks++;
foreach(var pair in new[]{new[]{"grid-template-columns","1fr 1fr"},new[]{"grid-template-rows","40px"},new[]{"align-items","stretch"},new[]{"justify-items","end"},new[]{"grid-auto-rows","50px"},new[]{"justify-content","end"}}){
    grid.Style.TryGetValue(pair[0],out var previous);grid.Style[pair[0]]=pair[1];
    if((bool)centered.Invoke(layout,new object[]{grid}))throw new System.Exception("Specialized centered grid accepted: "+pair[0]);
    if(previous==null)grid.Style.Remove(pair[0]);else grid.Style[pair[0]]=previous;gridChecks++;
}
var extraCell=new EchoSphere.Presentation.VisualCssNode {Tag="div",Parent=grid};grid.Children.Add(extraCell);
if((bool)centered.Invoke(layout,new object[]{grid}))throw new System.Exception("Multiple centered cells accepted");gridChecks++;
extraCell.Style["position"]="absolute";
if(!(bool)centered.Invoke(layout,new object[]{grid}))throw new System.Exception("Absolute overlay must not count as a grid cell");gridChecks++;
cell.Style["grid-row"]="2";
if((bool)centered.Invoke(layout,new object[]{grid}))throw new System.Exception("Explicit centered grid placement accepted");gridChecks++;
return new { orderChecks, styleChecks, indexChecks, gridChecks, result = "PASS", scope = "Native retained hierarchy, helper styles, grid eligibility and imported sprite topology; not full visual/gameplay acceptance" };
