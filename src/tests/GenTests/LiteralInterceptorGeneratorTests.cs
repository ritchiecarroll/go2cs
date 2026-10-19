// LiteralInterceptorGeneratorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// LiteralInterceptorGenerator, the any-slot half of plain string literals: a bare C# literal the converter passes to
/// an <c>object</c> parameter or a <c>params Span&lt;object&gt;</c> element reaches the callee as a cached Go string,
/// every call it cannot intercept is GO2CS0005, an overload that captures the literal is GO2CS0006, and GO2CS0004
/// stays the net when the generator does not run.
/// </summary>
/// <remarks>
/// Each arm runs the REAL generator (and, where it says so, the real StringLiteralBindingAnalyzer) over an in-memory
/// compilation, then EMITS and RUNS it: the callee reports the full name of the dynamic type it received, so an
/// accepted shape is proved by what the program sees, not by the generated text. The stand-ins mirror golib's shapes:
/// @string converts implicitly from a C# string and from a u8 span, and ж&lt;T&gt; is a reference box. RED before the
/// generator existed: every accepted shape read System.String, and no refusal or drift was reported.
/// </remarks>
[TestClass]
public class LiteralInterceptorGeneratorTests
{
    private const string Golib =
        """
        namespace go
        {
            [System.AttributeUsage(System.AttributeTargets.Parameter)]
            public sealed class GoTakesCSharpStringAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Assembly)]
            public sealed class GoHandOwnedPackageAttribute : System.Attribute { }

            public readonly struct @string
            {
                public static implicit operator @string(string value) => default;
                public static implicit operator @string(System.ReadOnlySpan<byte> value) => default;
            }

            public sealed class ж<T>
            {
                public T val = default!;
            }

            public static class builtin
            {
                public static System.Exception panic([GoTakesCSharpString] object? state) => new(state?.GetType().FullName);
            }
        }
        """;

    // The converted package's callees, in a hand-written (_impl) file so the generator reads only the call sites.
    private const string Callees =
        """
        namespace go
        {
            public interface Storer
            {
                string Store(object key);
            }

            public sealed class MapStorer : Storer
            {
                public string Store(object key) => main_package.Seen(key);
            }

            public struct Counter
            {
                public int n;
                public string Add(object v) { n++; return main_package.Seen(v); }
            }

            public struct Box<T>
            {
                public string Put(object v) => main_package.Seen(v);
            }

            public static partial class main_package
            {
                public static string Seen(object? v) => v?.GetType().FullName ?? "null";

                public static string Println(params System.Span<object> a)
                {
                    string[] names = new string[a.Length];

                    for (int i = 0; i < a.Length; i++)
                        names[i] = Seen(a[i]);

                    return string.Join(",", names);
                }

                public static string Show(object v) => Seen(v);
                public static string Pair(object k, @string s) => Seen(k);
                public static string Load(this ж<Counter> m, object key) => Seen(key);
                public static string Bump(this ref Counter m, object key) { m.n++; return Seen(key); }
                public static string Echo<T>(T x, object v) => Seen(v);
                public static string Overloaded(object v) => Seen(v);
                public static string Overloaded(@string v) => "go.string-overload";
                public static string Get(this ж<Counter> m, object key) => Seen(key);
                public static string Get(this ж<Counter> m, @string key) => "go.string-overload";
            }
        }
        """;

    private static readonly CSharpParseOptions s_parseOptions =
        new CSharpParseOptions(LanguageVersion.Latest).WithFeatures([new("InterceptorsNamespaces", "go2cs.Interceptors")]);

    private static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        return
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Collections.dll"))
        ];
    }

    private sealed record Run(Compilation Updated, ImmutableArray<Diagnostic> GeneratorDiagnostics, ImmutableArray<GeneratedSourceResult> Sources);

    private static Run Generate(string body, string path = "main.cs", string? assemblyAttributes = null, bool runGenerator = true)
    {
        string source =
            $$"""
            {{assemblyAttributes}}
            namespace go
            {
                public static partial class main_package
                {
                    public static string Body()
                    {
                        {{body}}
                    }
                }
            }
            """;

        CSharpCompilation compilation = CSharpCompilation.Create(
            "literal-interceptor-test",
            [
                CSharpSyntaxTree.ParseText(Golib, s_parseOptions, path: "golib_stand_in_impl.cs"),
                CSharpSyntaxTree.ParseText(Callees, s_parseOptions, path: "callees_impl.cs"),
                CSharpSyntaxTree.ParseText(source, s_parseOptions, path: path)
            ],
            CoreReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        if (!runGenerator)
            return new Run(compilation, [], []);

        GeneratorDriver driver = CSharpGeneratorDriver.Create([new LiteralInterceptorGenerator().AsSourceGenerator()], parseOptions: s_parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation updated, out ImmutableArray<Diagnostic> diagnostics);

        return new Run(updated, diagnostics, [.. driver.GetRunResult().Results.SelectMany(result => result.GeneratedSources)]);
    }

    // Emits the compilation, runs main_package.Body() and answers its result.
    private static string Execute(Compilation compilation)
    {
        using MemoryStream image = new();
        EmitResult emitted = compilation.Emit(image);

        Assert.IsTrue(emitted.Success, $"the fixture must emit clean: {string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))}");

        Assembly assembly = Assembly.Load(image.ToArray());
        System.Reflection.MethodInfo body = assembly.GetType("go.main_package")!.GetMethod("Body")!;

        return (string)body.Invoke(null, null)!;
    }

    private static string Accepted(string body)
    {
        Run run = Generate(body);

        Assert.AreEqual(0, run.GeneratorDiagnostics.Length, $"no generator diagnostic for `{body}`: {string.Join("; ", run.GeneratorDiagnostics)}");
        Assert.AreEqual(1, run.Sources.Length, "one generated file for the one converted source file");

        return Execute(run.Updated);
    }

    [TestMethod]
    [DataRow("""return Println("hello", 1, "world");""", "go.string,System.Int32,go.string", DisplayName = "static params Span<object>, literals beside a non-literal")]
    [DataRow("""return Show("one");""", "go.string", DisplayName = "static object parameter")]
    [DataRow("""return Pair("k", "s");""", "go.string", DisplayName = "an object slot beside a Go string slot")]
    [DataRow("""return new ж<Counter>().Load("k");""", "go.string", DisplayName = "extension on a pointer box (a ж<T> receiver forwarder)")]
    [DataRow("""Storer s = new MapStorer(); return s.Store("iface");""", "go.string", DisplayName = "interface method")]
    [DataRow("""var c = new Counter(); return c.Add("struct") + ":" + c.n;""", "go.string:1", DisplayName = "struct instance method, receiver by reference")]
    [DataRow("""var c = new Counter(); string r = c.Bump("b"); return r + ":" + c.n;""", "go.string:1", DisplayName = "extension with a ref struct receiver")]
    [DataRow("""return Println("");""", "go.string", DisplayName = "the empty literal")]
    public void AnInterceptedLiteralReachesTheCalleeAsAGoString(string body, string expected)
    {
        Assert.AreEqual(expected, Accepted(body));
    }

    // The control: without the generator the same call delivers System.String, which is what GO2CS0004 forbids.
    [TestMethod]
    public void WithoutTheGeneratorTheCalleeSeesSystemString()
    {
        Run run = Generate("""return Show("one");""", runGenerator: false);

        Assert.AreEqual("System.String", Execute(run.Updated));
    }

    [TestMethod]
    public void SitesWithTheSameMethodAndLiteralsShareOneInterceptor()
    {
        Run run = Generate("""return Show("x") + Show("x") + Show("y");""");

        Assert.AreEqual(0, run.GeneratorDiagnostics.Length);
        string generated = run.Sources.Single().SourceText.ToString();

        Assert.AreEqual(2, generated.Split(["public static string ˢI"], StringSplitOptions.None).Length - 1, generated);
        Assert.AreEqual(3, generated.Split(["InterceptsLocation(1,"], StringSplitOptions.None).Length - 1, generated);
        Assert.AreEqual(2, generated.Split(["internal static readonly object ˢ"], StringSplitOptions.None).Length - 1, generated);
        Assert.AreEqual("go.stringgo.stringgo.string", Execute(run.Updated));
    }

    [TestMethod]
    [DataRow("""System.Func<object, string> f = Show; return f("del");""", "invokes a delegate", DisplayName = "a delegate (a Go func value)")]
    [DataRow("""return Echo(1, "gen");""", "generic method", DisplayName = "a generic method")]
    [DataRow("""return new Box<int>().Put("box");""", "generic type", DisplayName = "a method of a generic type")]
    [DataRow("""return Show("\uD800");""", "lone UTF-16 surrogate", DisplayName = "a literal u8 cannot encode")]
    public void ACallTheGeneratorCannotInterceptIsGO2CS0005(string body, string reason)
    {
        Run run = Generate(body);
        Diagnostic[] refused = [.. run.GeneratorDiagnostics.Where(diagnostic => diagnostic.Id == "GO2CS0005")];

        Assert.AreEqual(1, refused.Length, $"one GO2CS0005 for `{body}`: {string.Join("; ", run.GeneratorDiagnostics)}");
        Assert.AreEqual(DiagnosticSeverity.Error, refused[0].Severity);
        StringAssert.Contains(refused[0].GetMessage(), reason);
    }

    // G's two drift arms: a Go string overload beside the any-slot callee captures the bare literal, for a static pair
    // and for an extension pair on a pointer box (most converted callees are ж<T> receiver forwarders).
    [TestMethod]
    [DataRow("""return Overloaded("x");""", "Overloaded(go.@string)", DisplayName = "static pair F(object) and F(@string)")]
    [DataRow("""return new ж<Counter>().Get("x");""", "Get(go.@string)", DisplayName = "extension pair F(this ж<T>, object) and F(this ж<T>, @string)")]
    public void AnOverloadThatCapturesTheLiteralIsGO2CS0006(string body, string bound)
    {
        Run run = Generate(body);
        Diagnostic[] drift = [.. run.GeneratorDiagnostics.Where(diagnostic => diagnostic.Id == "GO2CS0006")];

        Assert.AreEqual(1, drift.Length, $"one GO2CS0006 for `{body}`: {string.Join("; ", run.GeneratorDiagnostics)}");
        Assert.AreEqual(DiagnosticSeverity.Error, drift[0].Severity);
        StringAssert.Contains(drift[0].GetMessage(), bound);
    }

    [TestMethod]
    [DataRow("""return builtin.panic("p").Message!;""", DisplayName = "a [GoTakesCSharpString] parameter takes the C# string on purpose")]
    [DataRow("""return Show((@string)"x"u8);""", DisplayName = "a Go string argument is not a C# literal")]
    public void ALiteralThatIsNotABoxedCSharpStringIsLeftAlone(string body)
    {
        Run run = Generate(body);

        Assert.AreEqual(0, run.GeneratorDiagnostics.Length, string.Join("; ", run.GeneratorDiagnostics));
        Assert.AreEqual(0, run.Sources.Length, "nothing to intercept");
    }

    [TestMethod]
    [DataRow("main_impl.cs", null, DisplayName = "a hand-written companion file")]
    [DataRow("main.cs", "[assembly: go.GoHandOwnedPackage]", DisplayName = "a hand-owned package")]
    public void HandWrittenCodeIsNotIntercepted(string path, string? assemblyAttributes)
    {
        Run run = Generate("""return Show("one");""", path, assemblyAttributes);

        Assert.AreEqual(0, run.Sources.Length);
        Assert.AreEqual("System.String", Execute(run.Updated));
    }

    // GO2CS0004 is the net: with the generator absent a bare literal in an object slot is an error, and with it present
    // the intercepted literal is not.
    [TestMethod]
    public void TheAnalyzerExemptsAnInterceptedLiteralOnly()
    {
        ImmutableArray<Diagnostic> absent = Generate("""return Show("one");""", runGenerator: false).Updated
            .WithAnalyzers([new StringLiteralBindingAnalyzer()]).GetAnalyzerDiagnosticsAsync().Result;

        Assert.AreEqual(1, absent.Count(diagnostic => diagnostic.Id == "GO2CS0004"), string.Join("; ", absent));

        ImmutableArray<Diagnostic> present = Generate("""return Show("one");""").Updated
            .WithAnalyzers([new StringLiteralBindingAnalyzer()]).GetAnalyzerDiagnosticsAsync().Result;

        Assert.AreEqual(0, present.Length, string.Join("; ", present));

        ImmutableArray<Diagnostic> refused = Generate("""System.Func<object, string> f = Show; return f("del");""").Updated
            .WithAnalyzers([new StringLiteralBindingAnalyzer()]).GetAnalyzerDiagnosticsAsync().Result;

        Assert.AreEqual(1, refused.Count(diagnostic => diagnostic.Id == "GO2CS0004"), "a refused site keeps GO2CS0004 beside GO2CS0005: " + string.Join("; ", refused));
    }
}
