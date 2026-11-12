// InternalPackageAnalyzerTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// InternalPackageAnalyzer, GO2CS0014: hand-written C# that names a type of a Go internal or vendored package from
/// outside its root (docs/phase4/DESIGN-internal-package-marking.md, section 10).
/// </summary>
/// <remarks>
/// Each case compiles a consumer against marked packages emitted in memory with golib's real
/// <c>GoInternalRootAttribute</c>, and runs either copy of the analyzer: go2cs-gen's (<c>convertedCopy: true</c>) or
/// go.lib's consumer copy (<c>false</c>).
/// </remarks>
[TestClass]
public class InternalPackageAnalyzerTests
{
    private const string Poll =
        """
        [assembly: go.GoInternalRoot("", Standard = true)]
        namespace go.@internal;
        [go.GoPackage("poll")]
        public static partial class poll_package
        {
            public class FD { public int Fd() => 3; }
            public static FD Open() => new();
            public static int F() => 1;
        }
        """;

    private const string Aes =
        """
        [assembly: go.GoInternalRoot("crypto", Standard = true)]
        namespace go.crypto.@internal.fips140;
        [go.GoPackage("aes")]
        public static partial class aes_package
        {
            public static int BlockSize() => 16;
        }
        """;

    private const string ModuleSync =
        """
        [assembly: go.GoInternalRoot("RootedUsingCapture")]
        namespace go.RootedUsingCapture.@internal;
        [go.GoPackage("sync")]
        public static partial class sync_package
        {
            public static int Count() => 2;
        }
        """;

    private const string ModuleDeep =
        """
        [assembly: go.GoInternalRoot("RootedUsingCapture/a")]
        namespace go.RootedUsingCapture.a.@internal;
        [go.GoPackage("x")]
        public static partial class x_package
        {
            public static int Deep() => 4;
        }
        """;

    private const string PublicStrings =
        """
        namespace go;
        [go.GoPackage("strings")]
        public static partial class strings_package
        {
            public static int Count() => 0;
        }
        """;

    private static readonly MetadataReference[] s_platform = PlatformReferences();
    private static readonly MetadataReference s_poll = Library("internal.poll", Poll);
    private static readonly MetadataReference s_aes = Library("crypto.internal.fips140.aes", Aes);
    private static readonly MetadataReference s_moduleSync = Library("RootedUsingCapture.internal.sync", ModuleSync);
    private static readonly MetadataReference s_moduleDeep = Library("RootedUsingCapture.a.internal.x", ModuleDeep);
    private static readonly MetadataReference s_strings = Library("strings", PublicStrings);

    // os's own package, a public package that returns poll's FD: Go code may use the value without importing poll.
    private static readonly MetadataReference s_os = Library("os",
        """
        namespace go;
        [go.GoPackage("os")]
        public static partial class os_package
        {
            public static go.@internal.poll_package.FD File() => go.@internal.poll_package.Open();
        }
        """, s_poll);

    private static IEnumerable<MetadataReference> TrustedPlatform()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator).Select(path => MetadataReference.CreateFromFile(path));
    }

    private static MetadataReference[] PlatformReferences() =>
        [.. TrustedPlatform(), MetadataReference.CreateFromFile(typeof(golib::go.GoInternalRootAttribute).Assembly.Location)];

    private static MetadataReference Library(string name, string source, params MetadataReference[] references)
    {
        CSharpCompilation library = CSharpCompilation.Create(name, [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview))],
            [.. PlatformReferences(), .. references], new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using System.IO.MemoryStream image = new();
        Microsoft.CodeAnalysis.Emit.EmitResult emitted = library.Emit(image);
        Assert.IsTrue(emitted.Success, $"{name} compiles: " + string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Take(3)));

        return MetadataReference.CreateFromImage(image.ToArray());
    }

    // A converted project's metadata file for package `name` in `ns`, with an optional ImportPath stamp.
    private static (string path, string source) PackageInfo(string ns, string name, string? importPath = null, string file = "package_info.cs") =>
        (file, $"namespace {ns};\n[go.GoPackage(\"{name}\"{(importPath is null ? "" : $", ImportPath = \"{importPath}\"")})]\npublic static partial class {name}_package {{ }}\n");

    // A project's GoModInfo stamp, as the csproj's AssemblyAttribute item generates it.
    private static (string path, string source) ModInfo(string module) =>
        (@"obj/AssemblyInfo.cs", $"// <auto-generated/>\n[assembly: go.GoModInfo(\"path\\t{module}\\nmod\\t{module}\\t(devel)\\t\\n\")]\n");

    private static Diagnostic[] Analyze(bool convertedCopy, (string path, string source)[] files, MetadataReference[] references, bool standardLibrary = false)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("consumer",
            files.Select(file => CSharpSyntaxTree.ParseText(file.source, options, path: file.path)),
            [.. s_platform, .. references], new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Diagnostic[] errors = [.. compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];
        Assert.AreEqual(0, errors.Length, "the consumer compiles: " + string.Join("; ", errors.Take(3)));

        Dictionary<string, string> properties = standardLibrary ? new() { ["build_property.GoStandardLibrary"] = "true" } : [];

        ImmutableArray<Diagnostic> diagnostics = compilation
            .WithAnalyzers([new InternalPackageAnalyzer(convertedCopy)], new AnalyzerOptions([], new BuildProperties(properties)))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();

        return [.. diagnostics.OrderBy(diagnostic => diagnostic.Location.SourceTree?.FilePath, StringComparer.Ordinal).ThenBy(diagnostic => diagnostic.Location.SourceSpan.Start)];
    }

    // Each diagnostic as "file:line: <named text>", the text at its location.
    private static string[] Sites(Diagnostic[] diagnostics) =>
        [.. diagnostics.Select(diagnostic => $"{diagnostic.Location.SourceTree!.FilePath}:{diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1}: " +
            diagnostic.Location.SourceTree.GetText().ToString(diagnostic.Location.SourceSpan))];

    private static void AssertSites(Diagnostic[] diagnostics, params string[] expected)
    {
        Assert.IsTrue(diagnostics.All(diagnostic => diagnostic.Id == "GO2CS0014" && diagnostic.Severity == DiagnosticSeverity.Warning),
            "every diagnostic is a GO2CS0014 warning: " + string.Join("; ", diagnostics.Select(diagnostic => diagnostic.ToString())));

        CollectionAssert.AreEqual(expected, Sites(diagnostics), "sites: " + string.Join(" | ", Sites(diagnostics)));
    }

    // Case 1: a hand file of a module's package names the module's own internal package. Go allows it.
    [TestMethod]
    public void AnInRootHandFileIsSilent()
    {
        Diagnostic[] diagnostics = Analyze(true,
            [PackageInfo("go.RootedUsingCapture", "lib"), ("lib_impl.cs", "namespace go.RootedUsingCapture;\npartial class lib_package { static int N() => go.RootedUsingCapture.@internal.sync_package.Count(); }\n")],
            [s_moduleSync]);

        AssertSites(diagnostics);
    }

    // Case 2: a module's hand file names a standard-library internal package, three times: one warning, at the first name.
    [TestMethod]
    public void AModuleHandFileNamingAStandardInternalPackageIsWarnedOnceAtTheFirstName()
    {
        Diagnostic[] diagnostics = Analyze(true,
            [PackageInfo("go.example.com", "a", "example.com/a"),
             ("a_impl.cs", "namespace go.example.com;\npartial class a_package\n{\n    static int N() => go.@internal.poll_package.F();\n    static go.@internal.poll_package.FD M() => go.@internal.poll_package.Open();\n}\n")],
            [s_poll]);

        AssertSites(diagnostics, "a_impl.cs:4: poll_package");
        Assert.AreEqual("'go.@internal.poll_package' is in Go package 'internal/poll', which only Go code under the standard library may import. " +
            "Go internal and vendored packages are not covered by any compatibility promise; this warning may become an error.", diagnostics[0].GetMessage());
    }

    // Control for case 2: the same use in a file named as converter output is not checked.
    [TestMethod]
    public void AConvertedFileIsNotChecked()
    {
        Diagnostic[] diagnostics = Analyze(true,
            [PackageInfo("go.example.com", "a", "example.com/a"), ("a.cs", "namespace go.example.com;\npartial class a_package { static int N() => go.@internal.poll_package.F(); }\n")],
            [s_poll]);

        AssertSites(diagnostics);
    }

    // A [module: GoManualConversion] file and an *_impl_test.cs file are hand-written; package_init.cs and package_info.cs are not checked.
    [TestMethod]
    public void HandOwnedFilesAreCheckedAndMetadataFilesAreNot()
    {
        const string use = "namespace go.example.com;\npartial class a_package { static int N{0}() => go.@internal.poll_package.F(); }\n";

        Diagnostic[] diagnostics = Analyze(true,
            [PackageInfo("go.example.com", "a", "example.com/a"),
             ("manual.cs", "[module: go.GoManualConversion]\n" + use.Replace("{0}", "1")),
             ("a_impl_test.cs", use.Replace("{0}", "2")),
             ("package_init.cs", use.Replace("{0}", "3")),
             ("package_info_internal_test.cs", use.Replace("{0}", "4"))],
            [s_poll]);

        AssertSites(diagnostics, "a_impl_test.cs:2: poll_package", "manual.cs:3: poll_package");
    }

    // Case 3: pure C# (no [GoPackage] class) names an internal package through the package class, and through an alias
    // declared in a generated global-usings file, which is reported at the use.
    [TestMethod]
    public void PureCSharpIsWarnedThroughThePackageClassAndThroughAnAlias()
    {
        Diagnostic[] diagnostics = Analyze(false,
            [("Program.cs", "static class Program\n{\n    static int Main() => go.@internal.poll_package.F();\n}\n"),
             ("obj/GlobalUsings.g.cs", "// <auto-generated/>\nglobal using poll = go.@internal.poll_package;\n"),
             ("Other.cs", "static class Other\n{\n    static int N() => poll.F();\n}\n")],
            [s_poll]);

        AssertSites(diagnostics, "Other.cs:3: poll", "Program.cs:3: poll_package");
    }

    // Control for case 3: an unmarked package is never warned on.
    [TestMethod]
    public void APublicPackageIsSilent()
    {
        Diagnostic[] diagnostics = Analyze(false,
            [("Program.cs", "using static go.strings_package;\nstatic class Program { static int Main() => go.strings_package.Count() + Count(); }\n")],
            [s_strings]);

        AssertSites(diagnostics);
    }

    // Case 4: the standard library's own hand files use root "" freely; a non-empty standard root is still a path
    // test; and a module whose path looks like the standard library's is not the standard library.
    [TestMethod]
    public void TheStandardLibraryIsAPathTestByFlag()
    {
        (string, string) osUse = ("os_impl.cs", "namespace go;\npartial class os_package { static int N() => go.@internal.poll_package.F(); }\n");
        AssertSites(Analyze(true, [PackageInfo("go", "os"), osUse], [s_poll], standardLibrary: true));

        (string, string) tlsUse = ("tls_impl.cs", "namespace go.crypto;\npartial class tls_package { static int N() => go.crypto.@internal.fips140.aes_package.BlockSize(); }\n");
        AssertSites(Analyze(true, [PackageInfo("go.crypto", "tls"), tlsUse], [s_aes], standardLibrary: true));

        (string, string) httpUse = ("http_impl.cs", "namespace go.net;\npartial class http_package { static int N() => go.crypto.@internal.fips140.aes_package.BlockSize(); }\n");
        Diagnostic[] http = Analyze(true, [PackageInfo("go.net", "http"), httpUse], [s_aes], standardLibrary: true);
        AssertSites(http, "http_impl.cs:2: aes_package");
        StringAssert.Contains(http[0].GetMessage(), "in Go package 'crypto/internal/fips140/aes', which only Go code under 'crypto' may import.");

        (string, string) moduleUse = ("x_impl.cs", "namespace go.crypto;\npartial class x_package { static int N() => go.crypto.@internal.fips140.aes_package.BlockSize(); }\n");
        AssertSites(Analyze(true, [PackageInfo("go.crypto", "x", "crypto/x"), moduleUse], [s_aes]), "x_impl.cs:2: aes_package");

        // And the standard library may not name a module's internal package whose root happens to match its path.
        (string, string) stdNamingModule = ("os_impl.cs", "namespace go;\npartial class os_package { static int N() => go.RootedUsingCapture.@internal.sync_package.Count(); }\n");
        AssertSites(Analyze(true, [PackageInfo("go", "os", "RootedUsingCapture/os"), stdNamingModule], [s_moduleSync], standardLibrary: true), "os_impl.cs:2: sync_package");
    }

    // M1: a main package takes its module's path from GoModInfo, and may use every root inside the module.
    [TestMethod]
    public void AMainPackageUsesItsModulePath()
    {
        (string, string) mainUse = ("main_impl.cs", "namespace go;\npartial class main_package { static int N() => go.RootedUsingCapture.@internal.sync_package.Count(); }\n");

        AssertSites(Analyze(true, [PackageInfo("go", "main"), ModInfo("RootedUsingCapture"), mainUse], [s_moduleSync]));

        // Without the stamp there is no path, and every root is forbidden.
        AssertSites(Analyze(true, [PackageInfo("go", "main"), mainUse], [s_moduleSync]), "main_impl.cs:2: sync_package");

        // A main is not the standard library: it may not name a standard internal package.
        (string, string) mainPoll = ("main_impl.cs", "namespace go;\npartial class main_package { static int N() => go.@internal.poll_package.F(); }\n");
        AssertSites(Analyze(true, [PackageInfo("go", "main"), ModInfo("RootedUsingCapture"), mainPoll], [s_poll]), "main_impl.cs:2: poll_package");

        // A main in another module is outside the root.
        AssertSites(Analyze(true, [PackageInfo("go", "main"), ModInfo("Other"), mainUse], [s_moduleSync]), "main_impl.cs:2: sync_package");
    }

    // M1's documented gap: a main's directory is not recorded, so a deeper root inside its module is allowed, though Go
    // would refuse it from a main outside that root.
    [TestMethod]
    public void AMainPackageIsAllowedADeeperRootInsideItsModule()
    {
        (string, string) mainUse = ("main_impl.cs", "namespace go;\npartial class main_package { static int N() => go.RootedUsingCapture.a.@internal.x_package.Deep(); }\n");

        AssertSites(Analyze(true, [PackageInfo("go", "main"), ModInfo("RootedUsingCapture"), mainUse], [s_moduleDeep]));
    }

    // M2: a test compilation's internal and external package classes agree once _test is dropped from the name and the
    // stamped path; package classes that disagree give no path.
    [TestMethod]
    public void TestPackagesAgreeAndDisagreeingPackagesHaveNoPath()
    {
        (string, string) use = ("rand_impl_test.cs", "namespace go.math;\npartial class rand_package { static int N() => go.@internal.poll_package.F(); }\n");

        AssertSites(Analyze(true,
            [PackageInfo("go.math", "rand", "math/rand/v2"), PackageInfo("go.math", "rand_test", "math/rand/v2_test", "package_test_info.cs"), use],
            [s_poll], standardLibrary: true));

        AssertSites(Analyze(true,
            [PackageInfo("go.math", "rand", "math/rand/v2"), PackageInfo("go.math", "other", null, "package_test_info.cs"), use],
            [s_poll], standardLibrary: true), "rand_impl_test.cs:2: poll_package");
    }

    // The section 6.2 decode, over the converter's own cases.
    [TestMethod]
    public void TheDecodeMatchesTheConverter()
    {
        (string ns, string name, string? importPath, string wantName, string wantPath)[] cases =
        [
            ("go.internal", "poll", null, "poll", "internal/poll"),
            ("go", "os", null, "os", "os"),
            ("go", "main", null, "main", "main"),
            ("go.crypto.internal.fips140", "aes", null, "aes", "crypto/internal/fips140/aes"),
            ("go.math.rand", "rand", "math/rand/v2", "rand", "math/rand/v2"),
            ("go.math.rand", "rand_test", "math/rand/v2_test", "rand", "math/rand/v2"),
            ("go.vendor.golang.org.x.crypto", "chacha20", "vendor/golang.org/x/crypto/chacha20", "chacha20", "vendor/golang.org/x/crypto/chacha20"),
            ("go.example.com", "x", "example.com/x", "x", "example.com/x"),
            ("go", "testing_test", null, "testing", "testing"),
            ("go.RootedUsingCapture.internal", "sync", null, "sync", "RootedUsingCapture/internal/sync")
        ];

        foreach ((string ns, string name, string? importPath, string wantName, string wantPath) in cases)
            Assert.AreEqual((wantName, wantPath), InternalPackageAnalyzer.DecodePackage(ns, name, importPath), $"{ns} {name} {importPath}");
    }

    // Go-allowed shapes: a namespace using, a value of an internal type reached through a public package, a member
    // called through that value, and an inferred type. None names the internal package.
    [TestMethod]
    public void GoAllowedShapesAreSilent()
    {
        Diagnostic[] diagnostics = Analyze(true,
            [PackageInfo("go.example.com", "a", "example.com/a"),
             ("a_impl.cs", "using go.@internal;\nnamespace go.example.com;\npartial class a_package { static int N() { var file = go.os_package.File(); return file.Fd(); } }\n")],
            [s_poll, s_os]);

        AssertSites(diagnostics);
    }

    // The shapes that name a type, one per file: a using static, an alias target, typeof, new, a generic argument.
    [TestMethod]
    public void EachNamingShapeIsWarned()
    {
        Diagnostic[] diagnostics = Analyze(false,
            [("Static.cs", "using static go.@internal.poll_package;\nstatic class A { static int N() => F(); }\n"),
             ("Alias.cs", "using P = go.@internal.poll_package.FD;\nstatic class B { static P? N() => null; }\n"),
             ("TypeOf.cs", "static class C { static System.Type N() => typeof(go.@internal.poll_package.FD); }\n"),
             ("New.cs", "static class D { static object N() => new go.@internal.poll_package.FD(); }\n"),
             ("Generic.cs", "static class E { static System.Collections.Generic.List<go.@internal.poll_package.FD>? N() => null; }\n")],
            [s_poll]);

        AssertSites(diagnostics,
            "Alias.cs:1: poll_package", "Generic.cs:1: poll_package", "New.cs:1: poll_package", "Static.cs:1: poll_package", "TypeOf.cs:1: poll_package");
    }

    // S1: the two copies are disjoint. go2cs-gen's copy is silent in plain C#; the consumer copy is silent in a converted project.
    [TestMethod]
    public void TheTwoCopiesAreDisjoint()
    {
        (string, string)[] plain = [("Program.cs", "static class Program { static int Main() => go.@internal.poll_package.F(); }\n")];
        (string, string)[] converted = [PackageInfo("go.example.com", "a", "example.com/a"), ("a_impl.cs", "namespace go.example.com;\npartial class a_package { static int N() => go.@internal.poll_package.F(); }\n")];

        AssertSites(Analyze(true, plain, [s_poll]));
        AssertSites(Analyze(false, plain, [s_poll]), "Program.cs:1: poll_package");
        AssertSites(Analyze(false, converted, [s_poll]));
        AssertSites(Analyze(true, converted, [s_poll]), "a_impl.cs:2: poll_package");
    }

    // S5: one diagnostic per (tree, forbidden assembly), at the first use in source order, on every run.
    [TestMethod]
    public void DeduplicationIsPerTreeAndAssemblyAndDeterministic()
    {
        (string, string)[] files =
        [
            ("Program.cs", "static class Program\n{\n    static int A() => go.@internal.poll_package.F();\n    static int B() => go.crypto.@internal.fips140.aes_package.BlockSize();\n" +
                "    static int C() => go.@internal.poll_package.F() + go.crypto.@internal.fips140.aes_package.BlockSize();\n}\n"),
            ("Second.cs", "static class Second\n{\n    static object B() => new go.@internal.poll_package.FD();\n    static int A() => go.@internal.poll_package.F();\n}\n")
        ];

        string[] first = Sites(Analyze(false, files, [s_poll, s_aes]));

        CollectionAssert.AreEqual(new[] { "Program.cs:3: poll_package", "Program.cs:4: aes_package", "Second.cs:3: poll_package" }, first, string.Join(" | ", first));

        for (int run = 0; run < 8; run++)
            CollectionAssert.AreEqual(first, Sites(Analyze(false, files, [s_poll, s_aes])), $"run {run} reports the same sites");
    }

    private sealed class BuildProperties(Dictionary<string, string> properties) : AnalyzerConfigOptionsProvider
    {
        private readonly Options m_global = new(properties);

        public override AnalyzerConfigOptions GlobalOptions => m_global;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => m_global;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => m_global;

        private sealed class Options(Dictionary<string, string> values) : AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value) => values.TryGetValue(key, out value);
        }
    }
}
