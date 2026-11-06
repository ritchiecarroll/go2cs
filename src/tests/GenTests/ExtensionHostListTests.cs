// ExtensionHostListTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using go2cs.Templates.InterfaceImpl;
using go2cs.Templates.InterfaceType;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go2cs.Symbols;

namespace go2cs.Tests;

/// <summary>
/// The extension-host list's generator half (docs/phase4/DESIGN-extension-host-list.md). golib reads the list in place
/// of <c>Assembly.GetTypes()</c> under Native AOT, so a class that declares extension methods and is not listed loses
/// its Go methods there and nowhere else. Two writers fill it: ExtensionHostListGenerator lists the classes the
/// compilation's own sources declare, and each template that ADDS such a class declares it in its own output. This
/// census reads the generated text here, in the test, and holds every generator site that can write a static class to
/// a case.
/// </summary>
[TestClass]
public class ExtensionHostListTests
{
    // ---------------------------------------------------------------------------------------------
    // Reading generated text
    // ---------------------------------------------------------------------------------------------

    private static CompilationUnitSyntax Parse(string source) =>
        CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)).GetCompilationUnitRoot();

    // The top-level classes a file declares that declare an extension method, by namespace-qualified name.
    private static string[] ExtensionHostsDeclaredIn(string source) =>
    [
        .. Parse(source).DescendantNodes().OfType<ClassDeclarationSyntax>()
            .Where(type => type.Parent is BaseNamespaceDeclarationSyntax or CompilationUnitSyntax)
            .Where(type => type.Members.OfType<MethodDeclarationSyntax>().Any(method =>
                method.ParameterList.Parameters.FirstOrDefault()?.Modifiers.Any(SyntaxKind.ThisKeyword) == true))
            .Select(type => string.Join(".", type.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse()
                .Select(space => space.Name.ToString()).Append(type.Identifier.ValueText)))
    ];

    // The classes a file's [assembly: GoExtensionHost(typeof(X))] lines list, without `global::`.
    private static string[] ExtensionHostsListedIn(string source) =>
    [
        .. Parse(source).AttributeLists
            .Where(list => list.Target?.Identifier.IsKind(SyntaxKind.AssemblyKeyword) == true)
            .SelectMany(list => list.Attributes)
            .Where(attribute => attribute.Name.ToString() == "global::go.GoExtensionHost")
            .Select(attribute => ((TypeOfExpressionSyntax)attribute.ArgumentList!.Arguments[0].Expression).Type.ToString())
            .Select(name => name.StartsWith("global::", StringComparison.Ordinal) ? name["global::".Length..] : name)
    ];

    // Every extension host the generated files declare is listed by one of them, or by the converter's own list.
    private static void AssertEveryDeclaredHostIsListed(IEnumerable<string> generated, params string[] listedByWriterOne)
    {
        string[] sources = [.. generated];
        HashSet<string> listed = [.. sources.SelectMany(ExtensionHostsListedIn), .. listedByWriterOne];
        string[] unlisted = [.. sources.SelectMany(ExtensionHostsDeclaredIn).Where(host => !listed.Contains(host)).Distinct()];

        Assert.AreEqual(0, unlisted.Length, $"generated extension hosts no GoExtensionHost lists: {string.Join(", ", unlisted)}");
    }

    private static MethodInfo Method(string name, string returnType) =>
        new()
        {
            Name = name,
            ReturnType = returnType,
            Parameters = [],
            GenericTypes = "",
            TypeConstraints = new Dictionary<string, string[]>()
        };

    // ---------------------------------------------------------------------------------------------
    // One case per generator site that can write a class holding extension methods
    // ---------------------------------------------------------------------------------------------

    // InterfaceTypeTemplate: an exported interface's assert overload lives in `<pkg>_package_asserts`.
    [TestMethod]
    public void TheInterfaceAssertsClassIsListed()
    {
        string generated = new InterfaceTypeTemplate
        {
            PackageNamespace = "go.probe",
            PackageName = "probe",
            Scope = "public",
            InterfaceName = "Reader",
            OperatorConstraints = [],
            Methods = [Method("Read", "nint")],
            EmitAssertOverload = true,
            UsingStatements = []
        }
        .Generate();

        CollectionAssert.Contains(ExtensionHostsDeclaredIn(generated), "go.probe.probe_package_asserts", "the case reaches the template's host");
        CollectionAssert.Contains(ExtensionHostsListedIn(generated), "go.probe.probe_package_asserts");
        AssertEveryDeclaredHostIsListed([generated]);
    }

    // InterfaceImplTemplate: a promoted interface method's Go method-set twin lives in a sibling class.
    [TestMethod]
    public void ThePromotedTwinsClassIsListed()
    {
        string generated = new InterfaceImplTemplate
        {
            PackageNamespace = "go",
            PackageName = "probe",
            StructName = "conn",
            InterfaceName = "Reader",
            Promoted = true,
            Overrides = [],
            Methods = [Method("Read", "nint")],
            UsingStatements = []
        }
        .Generate();

        string twins = $"go.probe{TempVarMarker}conn{TempVarMarker}Reader{TempVarMarker}promoted";

        CollectionAssert.Contains(ExtensionHostsDeclaredIn(generated), twins, "the case reaches the template's host");
        CollectionAssert.Contains(ExtensionHostsListedIn(generated), twins);
        AssertEveryDeclaredHostIsListed([generated]);
    }

    // StructTypeTemplate: methods promoted through another package's embed are forwarded from `{pkg}ᴛ{Struct}ᴛxpkg`.
    [TestMethod]
    public void TheCrossPackageSiblingClassIsListed()
    {
        CrossPackagePromotedForwarderTests.Run run = CrossPackagePromotedForwarderTests.GenerateClean(CrossPackagePromotedForwarderTests.Consumer);
        string sibling = $"go.main{TempVarMarker}Direct{TempVarMarker}xpkg";

        CollectionAssert.Contains(run.Sources.Values.SelectMany(ExtensionHostsDeclaredIn).ToArray(), sibling, "the case reaches the template's host");
        CollectionAssert.Contains(run.Sources.Values.SelectMany(ExtensionHostsListedIn).ToArray(), sibling);
        AssertEveryDeclaredHostIsListed(run.Sources.Values, "go.main_package");
    }

    // LiteralInterceptorGenerator: an instance target's interceptor takes its receiver as `this`, in a `file` class
    // only its own file can name. The fixture compiles and runs, so the declaration of a file class binds.
    [TestMethod]
    public void TheInterceptorClassIsListedAndBinds()
    {
        const string attributes =
            """
            namespace go
            {
                [System.AttributeUsage(System.AttributeTargets.Assembly)]
                public sealed class GoExtensionHostListAttribute : System.Attribute;

                [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
                public sealed class GoExtensionHostAttribute(System.Type host) : System.Attribute;
            }
            """;

        LiteralInterceptorGeneratorTests.Run run = LiteralInterceptorGeneratorTests.Generate("""return Println("hello", 1, "world");""", assemblyAttributes: attributes);
        string[] generated = [.. run.Sources.Select(source => source.SourceText.ToString())];

        CollectionAssert.Contains(generated.SelectMany(ExtensionHostsListedIn).ToArray(), "go2cs.Interceptors.ˢInterceptors");
        Assert.AreEqual("go.string,System.Int32,go.string", LiteralInterceptorGeneratorTests.Execute(run.Updated), "the intercepted call still runs");
    }

    // A compilation that cannot name the attribute (a stand-in golib, an older one) gets no declaration to bind.
    [TestMethod]
    public void TheInterceptorDeclaresNoHostWithoutTheAttribute()
    {
        LiteralInterceptorGeneratorTests.Run run = LiteralInterceptorGeneratorTests.Generate("""return Println("hello", 1, "world");""");

        Assert.IsFalse(run.Sources.Any(source => source.SourceText.ToString().Contains("GoExtensionHost")));
    }

    private static IEnumerable<MetadataReference> References() =>
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoExtensionHostAttribute).Assembly.Location));

    private const string ConvertedSources =
        """
        namespace go
        {
            [GoPackage("demo")]
            public static partial class demo_package
            {
                public static nint Twice(this nint x) => x * 2;
            }

            partial class demo_package
            {
                internal static nint Thrice(nint x) => x * 3;
            }

            public static class helpers { }

            internal static class holder<T> { }

            public class plain { }
        }

        namespace go.nested
        {
            internal static class inner_package { }
        }
        """;

    private static (string? list, Compilation output) RunHostListGenerator(params MetadataReference[] references)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);
        CSharpCompilation compilation = CSharpCompilation.Create("demo",
            [CSharpSyntaxTree.ParseText("global using go;", options), CSharpSyntaxTree.ParseText(ConvertedSources, options)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ExtensionHostListGenerator()).WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string? list = driver.GetRunResult().Results.Single().GeneratedSources
            .Where(source => source.HintName == ExtensionHostListGenerator.HintName)
            .Select(source => source.SourceText.ToString())
            .SingleOrDefault();

        return (list, output);
    }

    // ExtensionHostListGenerator: every top-level, non-generic static class the compilation's own sources declare,
    // whether or not those sources give it an extension method (a template may), behind the completeness marker.
    [TestMethod]
    public void TheHostListGeneratorListsEveryStaticClassOfTheSources()
    {
        (string? list, Compilation output) = RunHostListGenerator([.. References()]);

        Assert.IsNotNull(list, "a compilation that references golib gets a list");
        StringAssert.Contains(list, GeneratedSource.ExtensionHostListDeclaration);
        CollectionAssert.AreEquivalent(new[] { "go.demo_package", "go.helpers", "go.nested.inner_package" }, ExtensionHostsListedIn(list));

        Diagnostic[] errors = [.. output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];
        Assert.AreEqual(0, errors.Length, "the list binds: " + string.Join("; ", errors.Take(3)));
    }

    [TestMethod]
    public void TheHostListGeneratorWritesNothingWithoutTheAttribute()
    {
        (string? list, _) = RunHostListGenerator([.. References().Take(0), MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);

        Assert.IsNull(list);
    }

    // A template that adds no class lists nothing: its file is what it was.
    [TestMethod]
    public void ATemplateThatAddsNoClassListsNothing()
    {
        string generated = new InterfaceTypeTemplate
        {
            PackageNamespace = "go",
            PackageName = "probe",
            Scope = "public",
            InterfaceName = "Reader",
            OperatorConstraints = [],
            Methods = [Method("Read", "nint")],
            UsingStatements = []
        }
        .Generate();

        Assert.IsFalse(generated.Contains("[assembly:"), generated);
    }

    // ---------------------------------------------------------------------------------------------
    // The census: every generator site that writes a static class is held to a case or a reason
    // ---------------------------------------------------------------------------------------------

    private const string ListedByWriterOne = "reopens the converter's package class, which ExtensionHostListGenerator lists";
    private const string NoExtensionMethod = "declares no extension method";

    // Each site, as "{file}:{member}", with how many such literals it holds, and the case or reason that covers it.
    private static readonly Dictionary<string, (int count, string coveredBy)> s_sites = new()
    {
        ["InterfaceTypeTemplate:AssertSourceOverloadClass"] = (1, nameof(TheInterfaceAssertsClassIsListed)),
        ["InterfaceImplTemplate:PromotedTwinsClass"] = (1, nameof(ThePromotedTwinsClassIsListed)),
        ["StructTypeTemplate:TemplateFooter"] = (1, nameof(TheCrossPackageSiblingClassIsListed)),
        ["LiteralInterceptorGenerator:Emit"] = (2, nameof(TheInterceptorClassIsListedAndBinds) + "; ˢLiterals " + NoExtensionMethod),
        ["TemplateBase:TemplateHeader"] = (1, ListedByWriterOne + ": " + nameof(TheHostListGeneratorListsEveryStaticClassOfTheSources)),
        ["TypeGenerator:EmitGoTypeRegistrations"] = (1, ListedByWriterOne),
        ["TypeGenerator:EmitGoZeroFactories"] = (1, ListedByWriterOne),
        ["ImplementGenerator:EmitAdapterRegistrations"] = (1, ListedByWriterOne),
        ["StructTypeTemplate:FieldReferences"] = (1, "a holder nested in the struct: not top-level, and " + NoExtensionMethod),
        ["BoundedGoroutineInterceptorGenerator:Emit"] = (1, NoExtensionMethod),
        ["RuntimeInitHookGenerator:Source"] = (1, NoExtensionMethod),
        ["FieldAccessGenerator:Source"] = (1, "C# 14 extension blocks of properties, which the scan's method filter does not read"),
    };

    private static readonly Regex s_staticClass = new(@"\bstatic\s+(partial\s+)?class\b", RegexOptions.Compiled);

    private static DirectoryInfo GeneratorSources()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            DirectoryInfo candidate = new(Path.Combine(directory.FullName, "src", "gen", "go2cs-gen"));

            if (File.Exists(Path.Combine(candidate.FullName, "GeneratedSource.cs")))
                return candidate;
        }

        throw new DirectoryNotFoundException("src/gen/go2cs-gen not found above " + AppContext.BaseDirectory);
    }

    // The generator's string literals (plain, interpolated and raw) that write `static class` or `static partial class`.
    private static Dictionary<string, int> StaticClassSites()
    {
        Dictionary<string, int> sites = new(StringComparer.Ordinal);
        DirectoryInfo root = GeneratorSources();

        foreach (FileInfo file in root.EnumerateFiles("*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root.FullName, file.FullName);

            if (relative.StartsWith("obj", StringComparison.Ordinal) || relative.StartsWith("bin", StringComparison.Ordinal))
                continue;

            SyntaxNode tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file.FullName), new CSharpParseOptions(LanguageVersion.Preview)).GetRoot();

            foreach (SyntaxToken token in tree.DescendantTokens())
            {
                if (token.Kind() is not (SyntaxKind.StringLiteralToken or SyntaxKind.InterpolatedStringTextToken or
                    SyntaxKind.SingleLineRawStringLiteralToken or SyntaxKind.MultiLineRawStringLiteralToken))
                    continue;

                int count = s_staticClass.Matches(token.Text).Count;

                if (count == 0)
                    continue;

                string member = token.Parent!.AncestorsAndSelf().OfType<MemberDeclarationSyntax>().First() switch
                {
                    MethodDeclarationSyntax method => method.Identifier.ValueText,
                    PropertyDeclarationSyntax property => property.Identifier.ValueText,
                    FieldDeclarationSyntax field => field.Declaration.Variables.First().Identifier.ValueText,
                    MemberDeclarationSyntax other => other.Kind().ToString()
                };

                string key = $"{Path.GetFileNameWithoutExtension(file.Name)}:{member}";
                sites[key] = sites.GetValueOrDefault(key) + count;
            }
        }

        return sites;
    }

    [TestMethod]
    public void EveryGeneratorSiteThatWritesAStaticClassIsCensused()
    {
        Dictionary<string, int> sites = StaticClassSites();

        string[] uncensused = [.. sites
            .Where(site => !s_sites.TryGetValue(site.Key, out var known) || known.count != site.Value)
            .Select(site => $"{site.Key} ({site.Value})")];

        Assert.AreEqual(0, uncensused.Length,
            "a generator site writes a static class the census does not cover; give it a case asserting its host is listed " +
            $"(or the reason it holds no extension method): {string.Join(", ", uncensused)}");

        string[] gone = [.. s_sites.Keys.Where(key => !sites.ContainsKey(key))];
        Assert.AreEqual(0, gone.Length, $"censused sites that no longer exist: {string.Join(", ", gone)}");
    }
}
