// GeneratedLayoutTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Holds every go2cs-gen generator's output to the layout half of the project's C# style (GeneratedLayout): the
/// generated code a reader opens, in the tour's Generated tab or under obj, reads as standard C#, not as converted
/// code. docs/PLAN-generated-code-format.md, seat 1.
/// </summary>
/// <remarks>
/// One fixture package reaches the three writers the scout found out of layout: LiteralInterceptorGenerator's
/// interceptor file (a block namespace's body written at column 0), TypeGenerator's method set (a declaration and its
/// <c>return</c> on one line) and StructTypeTemplate's field-wise constructor (<c>if (…) this.f = f;</c> on one line).
/// Every [Generator] in go2cs-gen runs over it, found by reflection so a new generator is read without a change here,
/// and each file it writes is read. RED at master ce5072e657: the three writers' files, and nothing else.
/// </remarks>
[TestClass]
public class GeneratedLayoutTests
{
    // A path in the host's own form: the interceptor generator names its file after the converted file's name.
    private static readonly string s_directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "go2cs", "src", "core", "ltest");

    // A struct with a value and a pointer method (its method set), a struct with a fixed-array field and a
    // direction-stamped channel field (the two guarded assignments of its field-wise constructor), and a converted call
    // passing string literals to `params Span<object>` (an interceptor file).
    private const string Package =
        """
        namespace go;

        [GoPackage("ltest")]
        public static partial class ltest_package
        {
            public partial struct Plain { public nint n; }

            [GoType] public partial struct Buf
            {
                public array<byte> b = new(4);
                public channel<nint> c = channel<nint>.RecvOnly;
                public nint n;
            }

            public static nint Get(this Plain p) => p.n;

            public static void Set(this ref Plain p, nint v) => p.n = v;

            public static void main()
            {
                Println("one", 1, "two");
                Println("three");
            }
        }
        """;

    // The callee, in a hand-written (_impl) file so the interceptor generator reads only the call sites.
    private const string Callees =
        """
        namespace go;

        public static partial class ltest_package
        {
            public static void Println(params global::System.Span<object> a) { }
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

    private static ISourceGenerator[] Generators() =>
    [
        .. typeof(TypeGenerator).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && type.GetCustomAttribute<GeneratorAttribute>() is not null)
            .OrderBy(type => type.Name, StringComparer.Ordinal)
            .Select(type => Activator.CreateInstance(type) switch
            {
                IIncrementalGenerator incremental => incremental.AsSourceGenerator(),
                ISourceGenerator generator => generator,
                _ => throw new InvalidOperationException($"{type.Name} is neither kind of generator")
            })
    ];

    private static (ImmutableArray<GeneratedSourceResult> sources, ImmutableArray<Diagnostic> diagnostics) Run()
    {
        CSharpParseOptions options = new CSharpParseOptions(LanguageVersion.Preview).WithFeatures([new("InterceptorsNamespaces", "go2cs.Interceptors")]);

        CSharpCompilation compilation = CSharpCompilation.Create("ltest",
            [
                CSharpSyntaxTree.ParseText(Package, options, path: System.IO.Path.Combine(s_directory, "ltest.cs")),
                CSharpSyntaxTree.ParseText(Callees, options, path: System.IO.Path.Combine(s_directory, "ltest_impl.cs")),
                CSharpSyntaxTree.ParseText(TemplateUsings, options, path: System.IO.Path.Combine(s_directory, "obj", "ltest.GlobalUsings.g.cs"))
            ],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(Generators(), parseOptions: options)
            .RunGeneratorsAndUpdateCompilation(compilation, out _, out ImmutableArray<Diagnostic> diagnostics);

        return ([.. driver.GetRunResult().Results.SelectMany(result => result.GeneratedSources)], diagnostics);
    }

    [TestMethod]
    public void EveryGeneratedFileHoldsTheProjectLayout()
    {
        (ImmutableArray<GeneratedSourceResult> sources, ImmutableArray<Diagnostic> diagnostics) = Run();

        // A generator that throws writes nothing, which would read as clean.
        Diagnostic[] failures = [.. diagnostics.Where(diagnostic => diagnostic.Id is "CS8784" or "CS8785")];
        Assert.AreEqual(0, failures.Length, $"every generator runs: {string.Join("; ", failures.Select(failure => failure.GetMessage()))}");

        // The fixture must keep reaching the three writers, or the guard reads clean by not looking.
        Assert.IsTrue(sources.Any(source => source.HintName.EndsWith(".Interceptors.g.cs", StringComparison.Ordinal)), "an interceptor file");
        Assert.IsTrue(sources.Any(source => source.HintName.EndsWith(".MethodSet.g.cs", StringComparison.Ordinal)), "a method set");
        Assert.IsTrue(sources.Any(source => source.SourceText.ToString().Contains("this.b = b;")), "Buf's field-wise constructor");

        string[] findings =
        [
            .. sources.SelectMany(source => GeneratedLayout.Findings(source.SourceText.ToString())
                .Select(finding => $"{source.HintName}: {finding}"))
        ];

        Assert.AreEqual(0, findings.Length, $"{findings.Length} layout finding(s):\n{string.Join("\n", findings.Take(40))}");
    }

    // The positive control: each arm names its planted defect, and the shapes the style or the owner admits read clean.
    [TestMethod]
    [DataRow("namespace n\n{\n    class C\n    {\n        void M() {\n        }\n    }\n}\n", "kr 5", DisplayName = "an opening brace at the end of its line")]
    [DataRow("namespace n\n{\n    class C\n    {\n        void M()\n        {\n          }\n    }\n}\n", "misalign 7", DisplayName = "a closing brace in another column")]
    [DataRow("namespace n\n{\nclass C\n{\n}\n}\n", "depth 4", DisplayName = "a block namespace's body at column 0")]
    [DataRow("namespace n\n{\n    class C\n    {\n      int f;\n    }\n}\n", "indent 5", DisplayName = "a member at the wrong indent")]
    [DataRow("class C\n{\n    int M()\n    {\n        int a = 1; return a;\n    }\n}\n", "two statements 5", DisplayName = "two statements on one line")]
    [DataRow("class C\n{\n    void M(int a)\n    {\n        if (a > 0) a = 0;\n    }\n}\n", "embedded 5", DisplayName = "an if and its statement on one line")]
    public void EachArmNamesItsPlantedDefect(string text, string expected)
    {
        List<string> findings = GeneratedLayout.Findings(text);

        Assert.IsTrue(findings.Count > 0, "the planted defect is read");
        StringAssert.StartsWith(findings[0], expected + ":", string.Join("; ", findings));
    }

    [TestMethod]
    [DataRow("class C\n{\n    [A][B] void M() { }\n    [A] [B]\n    void N()\n    {\n    }\n}\n", DisplayName = "attribute lists on one line (owner ruling)")]
    [DataRow("class C\n{\n    int M(int a)\n    {\n        switch (a)\n        {\n            case 1:\n                return 1;\n            default:\n                {\n                    return 0;\n                }\n        }\n    }\n}\n", DisplayName = "switch sections")]
    [DataRow("class C\n{\n    int M(int a)\n    {\n        if (a > 0)\n            return 1;\n        else if (a < 0)\n            return -1;\n        else\n            return 0;\n    }\n}\n", DisplayName = "embedded statements and else if")]
    [DataRow("class C\n{\n    System.Func<int> M(int a)\n    {\n        { int c = a; return () => c; }\n    }\n}\n", DisplayName = "a braced block on one line")]
    [DataRow("class C\n{\n    void M()\n    {\n        using (A())\n        using (A())\n        {\n        }\n    }\n\n    static System.IDisposable A() => null;\n}\n", DisplayName = "a using nested directly in a using")]
    public void AdmittedShapesReadClean(string text)
    {
        List<string> findings = GeneratedLayout.Findings(text);

        Assert.AreEqual(0, findings.Count, string.Join("; ", findings));
    }
}
