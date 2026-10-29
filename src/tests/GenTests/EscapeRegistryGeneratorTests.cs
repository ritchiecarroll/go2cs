// EscapeRegistryGeneratorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Guards the generated escape registry (trim stage 3c-2b(iii)): every CLOSED container a package converts to an
/// interface, or reaches through a converted value's fields or a pointer's target, is registered with golib's GoTypeOps;
/// a container that never escapes is not, and an open one (inside generic code) cannot be. A container the registry
/// misses is one reflect.New / Zero(*C) cannot serve under Native AOT (row G's container probe, measured). The fixture's
/// seven shapes: direct to object, via any, via a struct field, a private element type, via a pointer's target, never
/// escaping (absent), open generic (absent).
/// </summary>
[TestClass]
public class EscapeRegistryGeneratorTests
{
    private const string Path = @"C:\go2cs\src\core\etest\etest.cs";

    private const string Package =
        """
        namespace go;

        [GoPackage("etest")]
        public static partial class etest_package
        {
            partial struct Holder { internal slice<@string> names; internal map<@string, nint> counts; }

            private partial struct Secret { internal nint x; }

            internal static void Use()
            {
                object direct = default(slice<nint>);
                any viaAny = default(array<byte>);
                object viaField = default(Holder);
                object privateElement = default(slice<Secret>);
                object viaPointer = default(ж<channel<int>>);
                channel<bool> local = default;
                _ = local;
            }

            internal static object Generic<T>(slice<T> s) => s;
        }
        """;

    private const string TemplateUsings =
        """
        global using static go.builtin;
        global using System;
        global using System.Numerics;
        global using any = System.Object;
        """;

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    private static (string[] registered, Compilation output) Run()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("etest",
            [CSharpSyntaxTree.ParseText(Package, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\etest\obj\etest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new EscapeRegistryGenerator()).WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string[] registered = output.SyntaxTrees.Select(tree => tree.ToString())
            .SelectMany(source => Regex.Matches(source, @"global::go\.GoTypeOps\.Register\(global::go\.GoTypeOps<(.+)>\.Instance\);").Select(match => match.Groups[1].Value))
            .OrderBy(name => name, StringComparer.Ordinal).ToArray();

        return (registered, output);
    }

    [TestMethod]
    public void EveryEscapingClosedContainerIsRegistered()
    {
        (string[] registered, Compilation output) = Run();

        string[] expected =
        [
            "global::go.array<byte>",                                  // converted to any
            "global::go.channel<int>",                                 // reached through a pointer's target
            "global::go.map<global::go.@string, nint>",                // reached through a converted struct's field
            "global::go.slice<global::go.@string>",                    // reached through a converted struct's field
            "global::go.slice<global::go.etest_package.Secret>",       // a private element: registered inside the package class
            "global::go.slice<nint>",                                  // converted to object
        ];

        CollectionAssert.AreEqual(expected.OrderBy(name => name, StringComparer.Ordinal).ToArray(), registered, "registered: " + string.Join(", ", registered));
        Assert.IsFalse(registered.Any(name => name.Contains("channel<bool>")), "a container that never escapes is not registered");
        Assert.IsFalse(registered.Any(name => name.Contains("<T>")), "an open container cannot be registered");

        Diagnostic[] errors = output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.AreEqual(0, errors.Length, "the registry binds: " + string.Join("; ", errors.Take(3)));
    }

    // A variadic func value reflect can hold is called through a trampoline closed over its signature, which Native AOT has
    // only when compiled code names it (golib's GoReflect.VariadicRegistry.cs; the varfunc probe threw NotSupportedException
    // at `reflect.ValueOf(join).Call`, 2026-10-21). The registry names the family of every variadic func type the package
    // converts to an interface or reaches through a converted value, spelled from the delegate's Invoke, so the C# natural
    // type of a method group or lambda registers too. The fixture's shapes: a method group, a lambda, a func field, a
    // pointer's target, a func variable through golib's typed-nil carrier (the varfunc probe's third line), a variadic
    // func in another func's parameters and in its multi-return (all registered, the last with the container beside it);
    // a variadic func that never escapes, a non-variadic func and an open one (absent).
    [TestMethod]
    public void EveryEscapingVariadicFuncSignatureIsRegistered()
    {
        const string package =
            """
            namespace go;

            [GoPackage("vtest")]
            public static partial class vtest_package
            {
                partial struct Holder { internal Funcꓸꓸꓸ<bool, byte, nint> f; }

                internal static @string join(@string sep, params Span<@string> parts) => sep;

                internal static void Use()
                {
                    object declared = join;
                    any literal = (nint n, params Span<nint> xs) => n + xs.Length;
                    object viaField = default(Holder);
                    object viaPointer = default(ж<Actionꓸꓸꓸ<int, uint>>);
                    Actionꓸꓸꓸ<short> local = static (params Span<short> xs) => { };
                    _ = local;
                    object plain = (Func<nint, nint>)(x => x);
                    var held = (byte b, params Span<byte> bs) => b;
                    object carried = held.OrTypedNilFunc();
                    object nested = default(Func<Funcꓸꓸꓸ<sbyte, sbyte, sbyte>, (Actionꓸꓸꓸ<long, ulong>, slice<short>)>);
                }

                internal static object Generic<T>(Actionꓸꓸꓸ<T> f) => f;
            }
            """;

        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("vtest",
            [CSharpSyntaxTree.ParseText(package, options, path: @"C:\go2cs\src\core\vtest\vtest.cs"), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\vtest\obj\vtest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new EscapeRegistryGenerator()).WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string[] registered = output.SyntaxTrees.Select(tree => tree.ToString())
            .SelectMany(source => Regex.Matches(source, @"global::go\.GoReflect\.(RegisterVariadic\w+<.+>)\(\);").Select(match => match.Groups[1].Value))
            .OrderBy(name => name, StringComparer.Ordinal).ToArray();

        string[] expected =
        [
            "RegisterVariadicAction<int, uint>",                                               // a pointer's target
            "RegisterVariadicFunc<byte, byte, byte>",                                          // through golib's typed-nil carrier (OrTypedNilFunc)
            "RegisterVariadicFunc<bool, byte, nint>",                                          // a converted struct's func field
            "RegisterVariadicFunc<global::go.@string, global::go.@string, global::go.@string>", // a method group (natural type)
            "RegisterVariadicFunc<nint, nint, nint>",                                          // a lambda (natural type)
            "RegisterVariadicFunc<sbyte, sbyte, sbyte>",                                       // a func's parameter
            "RegisterVariadicAction<long, ulong>",                                             // inside a func's multi-return
        ];

        CollectionAssert.AreEqual(expected.OrderBy(name => name, StringComparer.Ordinal).ToArray(), registered, "registered: " + string.Join(", ", registered));
        Assert.IsTrue(output.SyntaxTrees.Any(tree => tree.ToString().Contains("global::go.GoTypeOps.Register(global::go.GoTypeOps<global::go.slice<short>>.Instance);")),
            "a container inside a func's multi-return is walked like any other element");

        Diagnostic[] errors = output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.AreEqual(0, errors.Length, "the registry binds: " + string.Join("; ", errors.Take(3)));
    }

    // TRAIN T3 (2026-10-10): a *.tests.csproj sets DisableTransitiveProjectReferences, so a type the compilation reaches
    // only through a referenced assembly's field signature can live in an assembly it does not reference -- and a
    // registry line naming it fails to compile (CS0234: go/internal/srcimporter naming go.ast_package, internal/testenv
    // and testing naming go.io_package). Two hops: A references B, B references C, and A sees C's type only through B's
    // field. The registry keeps what A can name and skips the rest (that container falls to golib's fallback: trim
    // stage 3d's boundary under Native AOT, never a wrong answer).
    [TestMethod]
    public void AContainerTheCompilationCannotNameIsNotRegistered()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);
        MetadataReference[] platform = [.. References()];

        MetadataReference Emit(string name, string source, params MetadataReference[] references)
        {
            CSharpCompilation library = CSharpCompilation.Create(name,
                [CSharpSyntaxTree.ParseText(source, options), CSharpSyntaxTree.ParseText(TemplateUsings, options)],
                [.. platform, .. references], new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

            using System.IO.MemoryStream image = new();
            Microsoft.CodeAnalysis.Emit.EmitResult emitted = library.Emit(image);
            Assert.IsTrue(emitted.Success, $"{name} compiles: " + string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Take(3)));

            return MetadataReference.CreateFromImage(image.ToArray());
        }

        MetadataReference c = Emit("cpkg", "namespace go; [GoPackage(\"cpkg\")] public static partial class cpkg_package { public partial struct Deep { public nint x; } }");
        MetadataReference b = Emit("bpkg", "namespace go; [GoPackage(\"bpkg\")] public static partial class bpkg_package { public partial struct Holder { public slice<cpkg_package.Deep> deep; public slice<nint> nums; } }", c);

        // A references B only, as a tests project with DisableTransitiveProjectReferences does.
        CSharpCompilation a = CSharpCompilation.Create("apkg",
            [CSharpSyntaxTree.ParseText("namespace go; [GoPackage(\"apkg\")] public static partial class apkg_package { internal static object Use() => default(bpkg_package.Holder); }", options, path: @"C:\go2cs\src\core\apkg\apkg.cs"),
             CSharpSyntaxTree.ParseText(TemplateUsings, options)],
            [.. platform, b], new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new EscapeRegistryGenerator()).WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(a, out Compilation output, out _);

        string[] registered = output.SyntaxTrees.Select(tree => tree.ToString())
            .SelectMany(source => Regex.Matches(source, @"global::go\.GoTypeOps\.Register\(global::go\.GoTypeOps<(.+)>\.Instance\);").Select(match => match.Groups[1].Value))
            .ToArray();

        Diagnostic[] errors = output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.AreEqual(0, errors.Length, "the registry compiles: " + string.Join("; ", errors.Take(3)));

        CollectionAssert.Contains(registered, "global::go.slice<nint>", "what A can name through B's field is registered");
        Assert.IsFalse(registered.Any(name => name.Contains("cpkg_package")), "a container over C's type, which A cannot name, is not: " + string.Join(", ", registered));
    }

    // the 1.24.13 r6 release rehearsal (2026-10-11): a PLATFORM-NEUTRAL package's registry walked a PER-OS assembly's struct
    // fields and registered go.slice<go.syscall_package.WSABuf>, a type only syscall's windows flavor has, so the neutral
    // assembly differed per OS. A neutral compilation still walks a NEUTRAL foreign struct's fields (N below: coverage
    // kept), but never a per-OS assembly's (P, marked go2cs.PlatformVariant): only a per-OS assembly's fields vary by OS,
    // so the neutral registry is OS-invariant by construction. What it skips falls to golib's fallback: trim stage 3d's
    // boundary under Native AOT, never a wrong answer.
    [TestMethod]
    public void ANeutralPackageDoesNotWalkAPerOSAssemblysFields()
    {
        MetadataReference p = EmitLibrary("ppkg", "[assembly: global::System.Reflection.AssemblyMetadata(\"go2cs.PlatformVariant\", \"true\")] namespace go; [GoPackage(\"ppkg\")] public static partial class ppkg_package { public partial struct OnlyHere { public nint x; } public partial struct Holder { public slice<ppkg_package.OnlyHere> only; } }");
        MetadataReference n = EmitLibrary("npkg", "namespace go; [GoPackage(\"npkg\")] public static partial class npkg_package { public partial struct Inner { public nint x; } public partial struct Holder { public slice<npkg_package.Inner> inner; } }");

        (string[] registered, string[] generated, Diagnostic[] errors) = RunOn("apkg",
            "partial struct Mine { internal slice<ppkg_package.Holder> holders; } internal static object Use() => default(ppkg_package.Holder); internal static object UseN() => default(npkg_package.Holder); internal static object UseMine() => default(Mine);",
            goTargetOS: null, perOSSource: false, p, n);

        Assert.AreEqual(0, errors.Length, "the registry compiles: " + string.Join("; ", errors.Take(3)));
        CollectionAssert.Contains(registered, "global::go.slice<global::go.ppkg_package.Holder>", "a container the package reaches through its OWN field is registered");
        CollectionAssert.Contains(registered, "global::go.slice<global::go.npkg_package.Inner>", "a NEUTRAL foreign struct's fields are still walked");
        Assert.IsFalse(registered.Any(name => name.Contains("OnlyHere")), "a PER-OS foreign struct's fields are not: " + string.Join(", ", registered));
        Assert.IsFalse(generated.Any(source => source.Contains("go2cs.PlatformVariant")), "a neutral package carries no per-OS marker");
    }

    // The marker's source: a compilation is PER-OS exactly when one of its source files sits in <project>/<GoTargetOS>/,
    // the folder the L3 csproj's `$(GoTargetOS)/*.cs` glob compiles (core/Directory.Build.props makes both properties
    // visible to the generator). Such an assembly is stamped go2cs.PlatformVariant, and walks any assembly's fields: its
    // own output is per-OS anyway.
    [TestMethod]
    public void APerOSPackageIsMarkedAndWalksFreely()
    {
        MetadataReference p = EmitLibrary("ppkg", "[assembly: global::System.Reflection.AssemblyMetadata(\"go2cs.PlatformVariant\", \"true\")] namespace go; [GoPackage(\"ppkg\")] public static partial class ppkg_package { public partial struct OnlyHere { public nint x; } public partial struct Holder { public slice<ppkg_package.OnlyHere> only; } }");

        (string[] registered, string[] generated, Diagnostic[] errors) = RunOn("opkg", "internal static object Use() => default(ppkg_package.Holder);", goTargetOS: "linux", perOSSource: true, p);

        Assert.AreEqual(0, errors.Length, "the marker and registry compile: " + string.Join("; ", errors.Take(3)));
        Assert.IsTrue(generated.Any(source => source.Contains("[assembly: global::System.Reflection.AssemblyMetadata(\"go2cs.PlatformVariant\", \"true\")]")), "a per-OS package is stamped");
        CollectionAssert.Contains(registered, "global::go.slice<global::go.ppkg_package.OnlyHere>", "and walks a per-OS assembly's fields");

        (_, string[] neutral, _) = RunOn("opkg", "internal static object Use() => default(ppkg_package.Holder);", goTargetOS: "linux", perOSSource: false, p);
        Assert.IsFalse(neutral.Any(source => source.Contains("go2cs.PlatformVariant")), "the same package with its sources at the project root is neutral");
    }

    // The per-OS marker's one lost type (2026-10-11, the six-build count against 0904171759): expvar boxes runtime.MemStats,
    // whose BySize field is array<runtime.MemStats_BySize>; once expvar (neutral) stopped at runtime (per-OS), no registry
    // named it. A per-OS package therefore registers what a referencing compilation reaches through its EXPORTED structs:
    // their public fields, and the public fields of its own structs reached through them -- the metadata view a neutral
    // walk had. Not a non-exported struct, not a non-public field, not a generic struct; and a neutral package, which every
    // walk still reaches, adds nothing.
    [TestMethod]
    public void APerOSPackageRegistersWhatItsExportedStructsReach()
    {
        const string Members =
            "public partial struct Sz { public uint Size; public slice<nint> sizes; } " +
            "public partial struct Stats { public array<Sz> BySize; public Inner inner; internal slice<Sz> hidden; } " +
            "public partial struct Inner { public map<nint, Sz> byKey; internal channel<Sz> notThis; } " +
            "internal partial struct Unexported { public slice<bool> no; } " +
            "public partial struct Gen<T> { public slice<T> open; public slice<byte> closedButGeneric; }";

        (string[] registered, _, Diagnostic[] errors) = RunOn("opkg", Members, goTargetOS: "linux", perOSSource: true);

        Assert.AreEqual(0, errors.Length, "the registry compiles: " + string.Join("; ", errors.Take(3)));
        CollectionAssert.Contains(registered, "global::go.array<global::go.opkg_package.Sz>", "an exported struct's public field");
        CollectionAssert.Contains(registered, "global::go.slice<nint>", "through a container's element type, the element struct's public field");
        CollectionAssert.Contains(registered, "global::go.map<nint, global::go.opkg_package.Sz>", "through a public field of its own struct type");
        Assert.IsFalse(registered.Any(name => name.Contains("slice<global::go.opkg_package.Sz>") || name.Contains("channel<")), "a non-public field is not walked: " + string.Join(", ", registered));
        Assert.IsFalse(registered.Any(name => name.Contains("slice<bool>")), "a non-exported struct is not a root: " + string.Join(", ", registered));
        Assert.IsFalse(registered.Any(name => name.Contains("slice<byte>")), "a generic struct is not a root: " + string.Join(", ", registered));

        (string[] neutral, _, _) = RunOn("opkg", Members, goTargetOS: "linux", perOSSource: false);
        Assert.AreEqual(0, neutral.Length, "a neutral package registers only what it converts: " + string.Join(", ", neutral));
    }

    // Seat #34, gap 1: a struct's operations face (IGoTypeOpsSource) is generated in its OWN compilation, and only when that
    // compilation spells a pointer to it (TypeOpsScope). A pointer to it spelled in ANOTHER compilation therefore reached
    // golib's dynamic fallback for its operations, which Native AOT cannot run for a value type it never compiled (the
    // AotReflectGaps probe's fmt %v over a nil *lib.Hidden field: "GoTypeOps`1[Hidden] is missing native code"). The walk
    // registers GoTypeOps<X> where it crosses ж<X> for a struct X of another assembly that lacks the face; X with the face
    // is never registered twice (the control), nor is the compilation's own struct (its face is generated beside it), nor a
    // pointer that never reaches an interface.
    [TestMethod]
    public void AForeignStructWithoutTheFaceIsRegisteredWhereItsPointerEscapes()
    {
        MetadataReference f = EmitLibrary("fpkg",
            "namespace go; [GoPackage(\"fpkg\")] public static partial class fpkg_package { " +
            "public partial struct Bare { public nint x; } " +
            "public partial struct Faced : IGoTypeOpsSource { public nint y; IGoTypeOps IGoTypeOpsSource.TypeOps => GoTypeOps<Faced>.Instance; } " +
            "public partial struct Quiet { public nint z; } }");

        (string[] registered, _, Diagnostic[] errors) = RunOn("apkg",
            "partial struct Own { internal nint w; } " +
            "partial struct Holder { public ж<fpkg_package.Bare> bare; public ж<fpkg_package.Faced> faced; public ж<Own> own; } " +
            "internal static object Use() => default(Holder); " +
            "internal static void Local() { ж<fpkg_package.Quiet> quiet = default; _ = quiet; }",
            goTargetOS: null, perOSSource: false, f);

        Assert.AreEqual(0, errors.Length, "the registry compiles: " + string.Join("; ", errors.Take(3)));
        CollectionAssert.Contains(registered, "global::go.fpkg_package.Bare", "a foreign struct without the face, reached through an escaping pointer: " + string.Join(", ", registered));
        Assert.IsFalse(registered.Any(name => name.Contains("Faced")), "a foreign struct WITH the face is not registered again: " + string.Join(", ", registered));
        Assert.IsFalse(registered.Any(name => name.Contains("Own")), "the compilation's own struct is not (its face is generated beside it): " + string.Join(", ", registered));
        Assert.IsFalse(registered.Any(name => name.Contains("Quiet")), "a pointer that never reaches an interface is not: " + string.Join(", ", registered));
    }

    private const string ProjectDirectory = @"C:\go2cs\src\core\opkg";

    private static MetadataReference EmitLibrary(string name, string source, params MetadataReference[] references)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation library = CSharpCompilation.Create(name,
            [CSharpSyntaxTree.ParseText(source, options), CSharpSyntaxTree.ParseText(TemplateUsings, options)],
            [.. References(), .. references], new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        using System.IO.MemoryStream image = new();
        Microsoft.CodeAnalysis.Emit.EmitResult emitted = library.Emit(image);
        Assert.IsTrue(emitted.Success, $"{name} compiles: " + string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Take(3)));

        return MetadataReference.CreateFromImage(image.ToArray());
    }

    // Runs the generator over <name>_package holding `members`. goTargetOS non-null sets the two build properties the
    // generator reads (build_property.GoTargetOS, build_property.MSBuildProjectDirectory = ProjectDirectory); perOSSource
    // places the package's source file in ProjectDirectory/<goTargetOS>/ instead of ProjectDirectory/.
    private static (string[] registered, string[] generated, Diagnostic[] errors) RunOn(string name, string members, string? goTargetOS, bool perOSSource, params MetadataReference[] references)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);
        string path = perOSSource && goTargetOS is not null ? $@"{ProjectDirectory}\{goTargetOS}\{name}.cs" : $@"{ProjectDirectory}\{name}.cs";

        CSharpCompilation compilation = CSharpCompilation.Create(name,
            [CSharpSyntaxTree.ParseText($"namespace go; [GoPackage(\"{name}\")] public static partial class {name}_package {{ {members} }}", options, path: path),
             CSharpSyntaxTree.ParseText(TemplateUsings, options)],
            [.. References(), .. references], new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        Dictionary<string, string> properties = goTargetOS is null ? [] : new()
        {
            ["build_property.GoTargetOS"] = goTargetOS,
            ["build_property.MSBuildProjectDirectory"] = ProjectDirectory
        };

        CSharpGeneratorDriver.Create([new EscapeRegistryGenerator()], parseOptions: options, optionsProvider: new BuildProperties(properties))
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string[] generated = [.. output.SyntaxTrees.Skip(compilation.SyntaxTrees.Length).Select(tree => tree.ToString())];
        string[] registered = [.. generated.SelectMany(source => Regex.Matches(source, @"global::go\.GoTypeOps\.Register\(global::go\.GoTypeOps<(.+)>\.Instance\);").Select(match => match.Groups[1].Value))];

        return (registered, generated, output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray());
    }

    private sealed class BuildProperties(Dictionary<string, string> properties) : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider
    {
        private readonly Options m_global = new(properties);

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GlobalOptions => m_global;

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(SyntaxTree tree) => m_global;

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(AdditionalText textFile) => m_global;

        private sealed class Options(Dictionary<string, string> values) : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value) => values.TryGetValue(key, out value);
        }
    }
}
