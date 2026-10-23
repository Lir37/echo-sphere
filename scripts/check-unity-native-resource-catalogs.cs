var assembly=typeof(EchoSphere.Runtime.EchoSphereRuntime).Assembly;
EchoSphere.Presentation.V Scope(string name)=>(EchoSphere.Presentation.V)assembly.GetType("EchoSphere.Presentation.Visual_"+name,true).GetField("Scope").GetValue(null);
var catalog=EchoSphere.Presentation.NativeLocalizationCatalog.Instance;var source=Scope("i18n")["translations"];int translations=0,canvas=0,art=0,orbital=0;
foreach(bool ru in new[]{true,false}){
    string lang=ru?"ru":"en";foreach(var row in (System.Collections.Generic.Dictionary<string,EchoSphere.Presentation.V>)source[lang].Raw){if(catalog.Translate(row.Key,ru)!=row.Value.Text)throw new System.Exception("Translation "+lang+"/"+row.Key);translations++;}
}
var oldLocale=EchoSphere.Presentation.VisualBindings.Locale;
try{
    var original=Scope("canvasLocale")["localizeCanvasText"];
    var samples=new System.Collections.Generic.List<string>{"", "STELLA · 12", "RESONANCE BASIN · CHORUS SYNC", "СЕТЬ NETWORK_1 NETWORK"};
    foreach(var row in catalog.CanvasPhrases)samples.Add(row.Pattern);
    foreach(var row in catalog.CanvasPatterns)samples.Add(row.Pattern.Replace("\\b", ""));
    foreach(bool ru in new[]{true,false}){EchoSphere.Presentation.VisualBindings.Locale=ru?"ru":"en";foreach(var token in samples)foreach(var text in new[]{token,"x "+token+" y",token+" "+token}){
        var expected=original.Call(text).Text;var actual=catalog.CanvasText(text,ru);if(expected!=actual)throw new System.Exception("Canvas locale mismatch: "+text+" => "+expected+" / "+actual);canvas++;
    }}
}finally{EchoSphere.Presentation.VisualBindings.Locale=oldLocale;}
var originalImages=Scope("renderer")["getArtImage"];var spriteCatalog=UnityEngine.Resources.Load<EchoSphere.Presentation.NativeWorldArtCatalog>("EchoSphere/NativeWorldArtCatalog");
var resolver=assembly.GetType("EchoSphere.Presentation.NativeWorldArtLibrary").GetMethod("Resolve",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
var oldFactory=EchoSphere.Presentation.VisualJs.ImageFactory;
try{
    EchoSphere.Presentation.VisualJs.ImageFactory=()=>new EchoSphere.Presentation.V(new EchoSphere.Presentation.SourceCanvasImage());
    foreach(var row in spriteCatalog.Entries){originalImages.Call(row.Key);resolver.Invoke(null,new object[]{row.Key});var expected=originalImages.Call(row.Key);var actual=(EchoSphere.Presentation.INativeWorldSprite)resolver.Invoke(null,new object[]{row.Key});
        var sourceImage=expected.Raw as EchoSphere.Presentation.SourceCanvasImage;if(sourceImage==null||actual==null||sourceImage.Sprite!=actual.Sprite||sourceImage.Width!=actual.Width||sourceImage.Height!=actual.Height)throw new System.Exception("Art mismatch "+row.Key);art++;
    }
}finally{EchoSphere.Presentation.VisualJs.ImageFactory=oldFactory;}
var geometry=Scope("orbitalGeometry");
foreach(double level in new[]{-2d,0,1,2,3,4,5,6,7,9,3.75})foreach(int extra in new[]{0,1,2,3,4}){
    var expected=geometry["getOrbitalRingCounts"].Call(level,extra);var actual=EchoSphere.Core.OrbitalVisualGeometryRules.RingCounts(level,extra);
    if(expected["inner"].Number!=actual.Inner||expected["outer"].Number!=actual.Outer)throw new System.Exception("Orbit count mismatch");
    foreach(bool outer in new[]{true,false})for(int index=0;index<(outer?actual.Outer:actual.Inner);index++){
        int count=outer?actual.Outer:actual.Inner;double angle=geometry["orbitalElementAngle"].Call(.731,outer?"outer":"inner",index,count).Number;
        if(angle!=EchoSphere.Core.OrbitalVisualGeometryRules.ElementAngle(.731,outer,index,count))throw new System.Exception("Orbit angle mismatch");orbital++;
    }
}
return new {translations,canvas,art,orbital,matched=true};
