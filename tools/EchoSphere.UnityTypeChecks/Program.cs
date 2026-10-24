using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.Cryptography;
using System.Text.Json;

// Compile project C# against actual Unity DLLs. No API stubs and no Editor
// process, asset import, shader compilation, player build or assembly execution.
if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: dotnet run --project tools/EchoSphere.UnityTypeChecks -- <UnityProject> <Editor/Data or extracted reference root> [diagnostics.json]");
    return 2;
}
var project = Path.GetFullPath(args[0]);
var data = Path.GetFullPath(args[1]);
var engine = Path.Combine(data, "Managed", "UnityEngine");
var bcl = Path.Combine(data, "UnityReferenceAssemblies", "unity-4.8-api");
if (!Directory.Exists(engine) || !Directory.Exists(bcl)) throw new DirectoryNotFoundException("Actual Unity engine/reference DLL directories are required.");
var sources = Directory.GetFiles(Path.Combine(project, "Assets"), "*.cs", SearchOption.AllDirectories);
var standard = Path.Combine(AppContext.BaseDirectory, "NetStandardReferences");
if (!File.Exists(Path.Combine(standard, "netstandard.dll"))) throw new DirectoryNotFoundException("The .NET SDK NETStandard.Library.Ref 2.1.0 targeting pack is required.");
var provenance = Path.Combine(data, "reference-manifest.json");
if (File.Exists(provenance))
{
    using var manifest = JsonDocument.Parse(File.ReadAllText(provenance));
    foreach (var file in manifest.RootElement.GetProperty("files").EnumerateArray())
    {
        var path = Path.Combine(data, file.GetProperty("target").GetString());
        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
        if (hash != file.GetProperty("sha256").GetString()) throw new InvalidDataException("Unity reference DLL hash mismatch: " + path);
    }
}
var results = new List<object>();
var failures = 0;
byte[] runtimeImage = null;
foreach (var configuration in new[] { "Android player", "Android editor runtime", "Android editor assembly" })
{
    var editor = configuration != "Android player";
    var editorAssembly = configuration == "Android editor assembly";
    var symbols = new List<string> { "UNITY_6000_3_OR_NEWER", "UNITY_6000_0_OR_NEWER", "UNITY_2023_1_OR_NEWER", "UNITY_2022_1_OR_NEWER", "UNITY_2021_1_OR_NEWER", "UNITY_ANDROID", "ENABLE_IL2CPP", "NET_STANDARD_2_1", "NET_STANDARD", "ENABLE_LEGACY_INPUT_MANAGER" };
    if (editor) symbols.AddRange(new[] { "UNITY_EDITOR", "UNITY_EDITOR_LINUX" });
    var parse = new CSharpParseOptions(LanguageVersion.CSharp9, preprocessorSymbols: symbols);
    var trees = sources.Where(file => Path.GetRelativePath(Path.Combine(project,"Assets"), file).Split(Path.DirectorySeparatorChar).Contains("Editor") == editorAssembly)
        .Select(file => CSharpSyntaxTree.ParseText(File.ReadAllText(file), parse, file)).ToArray();
    var files = Directory.GetFiles(editorAssembly ? bcl : standard, "*.dll", SearchOption.AllDirectories)
        .Concat(Directory.GetFiles(engine, "*.dll").Where(file => editor || !Path.GetFileName(file).StartsWith("UnityEditor", StringComparison.Ordinal)))
        .ToList();
    var packages = Path.Combine(data, "PackageReferences");
    if (Directory.Exists(packages)) files.AddRange(Directory.GetFiles(packages, "*.dll", SearchOption.AllDirectories));
    // Imported projects can supply their own exact package assemblies.
    var imported = Path.Combine(project,"Library","ScriptAssemblies");
    if (!Directory.Exists(packages) && Directory.Exists(imported)) files.AddRange(Directory.GetFiles(imported,"Unity.*.dll").Where(file => editorAssembly || !Path.GetFileName(file).Contains("Editor")));
    var references = files.Distinct().Select(file => MetadataReference.CreateFromFile(file)).Cast<MetadataReference>().ToList();
    if(configuration=="Android player")
    {
        var probe=CSharpSyntaxTree.ParseText("sealed class Probe : UnityEngine.MonoBehaviour { UnityEngine.Rendering.CommandBuffer immediate = new UnityEngine.Rendering.CommandBuffer(); System.Func<UnityEngine.Rendering.CommandBuffer> deferred = () => new UnityEngine.Rendering.CommandBuffer(); }",parse);
        var check=CSharpCompilation.Create("InitializationGuardProbe",new[]{probe},references,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        if(NativeInitializers(check).Count()!=1)throw new InvalidOperationException("Native initialization guard must reject immediate construction and allow a deferred factory.");
        Console.WriteLine("Native initialization guard: PASS (immediate field construction rejected; deferred factory allowed).");
    }
    if (editorAssembly)
    {
        if (runtimeImage == null) { Console.Error.WriteLine("Editor assembly check requires a successful editor runtime compilation."); failures++; continue; }
        references.Add(MetadataReference.CreateFromImage(runtimeImage));
    }
    var compilation = CSharpCompilation.Create(editorAssembly ? "Assembly-CSharp-Editor" : "Assembly-CSharp", trees, references,
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, warningLevel: 4, allowUnsafe: false));
    if(configuration=="Android player")
    {
        foreach(var name in new[]{"V","VisualJs","VisualJson","IVisualHost","IVisualCallable","SourceCanvasImage","SourceCanvasArtIndex","NativeRunSnapshotDefaults","SourceUiDocument","SourceCanvasSurface"})
            if(compilation.GetTypeByMetadataName("EchoSphere.Presentation."+name)!=null)throw new InvalidOperationException("Retired presentation compatibility type leaked into Android player: "+name);
        var runtime=compilation.GetTypeByMetadataName("EchoSphere.Runtime.EchoSphereRuntime");
        foreach(var name in new[]{"_sourceRenderState","_sourceRenderEntities","UpdateSourceRenderState","UpdateSourceUiState","ApplySourceUiWrites"})
            if(runtime?.GetMembers(name).Length>0)throw new InvalidOperationException("Retired scene snapshot member leaked into Android player: "+name);
        Console.WriteLine("Native presentation retirement: PASS (value emulator, old UI/canvas and scene snapshot absent from Android player).");
    }
    using var output = new MemoryStream();
    var diagnostics = compilation.Emit(output).Diagnostics.Concat(NativeInitializers(compilation)).Where(d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning).OrderBy(d => d.Location.SourceTree?.FilePath).ThenBy(d => d.Location.SourceSpan.Start).ToArray();
    var errors = diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);
    var warnings = diagnostics.Length - errors;
    failures += errors + warnings;
    if (configuration == "Android editor runtime" && errors == 0) runtimeImage = output.ToArray();
    Console.WriteLine($"{configuration}: {trees.Length} C# files, {errors} errors, {warnings} compiler warnings ({(editorAssembly ? "Unity .NET Framework references" : ".NET Standard 2.1")}; real Unity API DLLs; no Unity execution).");
    foreach (var diagnostic in diagnostics.OrderByDescending(d => d.Severity).Take(30)) Console.WriteLine(diagnostic);
    if (editorAssembly) Console.WriteLine("URP API reference: " + compilation.GetTypeByMetadataName("UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset")?.ContainingAssembly.Identity);
    results.Add(new { configuration, files = trees.Length, errors, warnings, diagnostics = diagnostics.Select(d => new { id = d.Id, severity = d.Severity.ToString(), message = d.GetMessage(), location = d.Location.GetLineSpan().ToString() }).ToArray() });
}
if (args.Length > 2) File.WriteAllText(args[2], JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
return failures == 0 ? 0 : 1;

static IEnumerable<Diagnostic> NativeInitializers(CSharpCompilation compilation)
{
    var rule=new DiagnosticDescriptor("ES1001","Unity native initialization","Initialize native Unity object '{0}' in Awake/Initialize, not a MonoBehaviour field initializer","Unity lifecycle",DiagnosticSeverity.Error,true);
    foreach(var tree in compilation.SyntaxTrees)
    {
        var model=compilation.GetSemanticModel(tree);
        foreach(var field in tree.GetRoot().DescendantNodes().OfType<FieldDeclarationSyntax>())
        {
            if(field.Modifiers.Any(SyntaxKind.StaticKeyword))continue;
            var owner=model.GetDeclaredSymbol(field.Declaration.Variables[0])?.ContainingType;
            var mono=false;for(var type=owner;type!=null;type=type.BaseType)if(type.ToDisplayString()=="UnityEngine.MonoBehaviour"){mono=true;break;}
            if(!mono)continue;
            foreach(var variable in field.Declaration.Variables)
            foreach(var creation in variable.Initializer?.Value.DescendantNodesAndSelf().OfType<ObjectCreationExpressionSyntax>()??Enumerable.Empty<ObjectCreationExpressionSyntax>())
            {
                if(creation.Ancestors().OfType<AnonymousFunctionExpressionSyntax>().Any())continue;
                var type=model.GetTypeInfo(creation).Type;
                if(type!=null&&!type.IsValueType&&type.ContainingNamespace.ToDisplayString().StartsWith("UnityEngine",StringComparison.Ordinal))
                    yield return Diagnostic.Create(rule,creation.GetLocation(),type.ToDisplayString());
            }
        }
    }
}
