// NoInliningPartialGeneratorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// The converter writes a function that must keep its own frame (computeNoInliningClosure) as a
/// partial method's IMPLEMENTING part: the word <c>partial</c> stands where the
/// <c>[MethodImpl(MethodImplOptions.NoInlining)]</c> prefix stood. NoInliningPartialGenerator writes the
/// declaring part carrying the attribute, and C# merges the two into one compiled method. These arms
/// compile the converter's own rendering of each shape (the signature lines are copied from
/// src/tests/Behavioral/NoInlinePartial/main.cs.target; the bodies are cut down to what compiles without
/// golib) and read the NoInlining flag back out of the emitted metadata, so a lost attribute fails here
/// as a missing flag and a missing declaring part fails as CS0759.
/// </summary>
[TestClass]
public class NoInliningPartialGeneratorTests
{
    // The stubs stand in for golib and the project's global usings; everything inside main_package is
    // the converter's rendering.
    private const string Stubs = """
        using System;
        using System.Runtime.CompilerServices;
        using GoInitAttribute = System.Runtime.CompilerServices.ModuleInitializerAttribute;
        using ꓸꓸꓸnint = System.Span<nint>;

        namespace go
        {
            [AttributeUsage(AttributeTargets.All)] public sealed class GoRecvAttribute : Attribute { }
            [AttributeUsage(AttributeTargets.All)] public sealed class GoTypeAttribute : Attribute { }
            public class ж<T> { public ref T DerefOrNull() => throw null!; }
            public readonly struct @string { }

            public static partial class main_package
            {
                internal partial struct counter { }
                internal partial struct box<T> { }
            }
        }
        """;

    private const string Carriers = """
        using System;
        using System.Runtime.CompilerServices;
        using GoInitAttribute = System.Runtime.CompilerServices.ModuleInitializerAttribute;
        using ꓸꓸꓸnint = System.Span<nint>;

        namespace go;

        partial class main_package {

        internal static partial @string here() {
            return default;
        }

        internal static partial @string plain() {
            return here();
        }

        internal static partial @string variadic(params ꓸꓸꓸnint xsʗp) {
            return here();
        }

        internal static partial (@string name, nint n) pair() {
            return (here(), 1);
        }

        internal static partial @string generic<T>(T x) {
            return here();
        }

        [GoType] partial struct counter {
            internal nint n;
        }

        [GoRecv] internal static partial @string ptr(this ref counter c) {
            return here();
        }

        internal static partial @string val(this counter c) {
            return here();
        }

        [GoType] partial struct box<T> {
            internal T v;
        }

        [GoRecv] internal static partial @string get<T>(this ref box<T> b) {
            return here();
        }

        [GoInit] internal static partial void init() {
        }

        internal static partial void Main() {
        }

        } // end main_package
        """;

    // The two shapes that cannot be partial keep the attribute themselves, and an unmarked receiver
    // method is the forwarder control. None of these may gain a generated declaring part.
    private const string Fallbacks = """
        using System;
        using System.Runtime.CompilerServices;

        namespace go;

        partial class main_package {

        internal static @string here() {
            return default;
        }

        [GoRecv] internal static @string plainRecv(this ref counter c) {
            return here();
        }

        internal static void Main() {
            var lit = [MethodImpl(MethodImplOptions.NoInlining)] @string () => here();
            [MethodImpl(MethodImplOptions.NoInlining)] @string local() {
                return here();
            }
            lit();
            local();
        }

        } // end main_package
        """;

    // Hand-owned shapes: a converted bodyless declaration (the declaring part, attribute and all) whose
    // body lives in a *_impl.cs, and a plain method that writes the attribute itself.
    private const string HandOwnedDeclaration = """
        using System.Runtime.CompilerServices;

        namespace go;

        partial class main_package {

        [MethodImpl(MethodImplOptions.NoInlining)] internal static partial nint nanotime();

        } // end main_package
        """;

    private const string HandOwnedImpl = """
        using System.Runtime.CompilerServices;

        namespace go;

        partial class main_package {

        internal static partial nint nanotime() {
            return 0;
        }

        [MethodImpl(MethodImplOptions.NoInlining)] internal static nint walk() {
            return nanotime();
        }

        } // end main_package
        """;

    private static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"));
    }

    private sealed record Run(Dictionary<string, string> Generated, ImmutableArray<Diagnostic> Errors, Dictionary<string, MethodImplAttributes> Methods);

    /// <summary>
    /// Runs the given generators over the sources, compiles the result, and reads every emitted
    /// method's implementation flags back out of the image (keyed by method name; a compiler-generated
    /// lambda or local function keeps its mangled name).
    /// </summary>
    private static Run Generate(ISourceGenerator[] generators, params string[] sources)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "noinline-partial",
            sources.Select(source => CSharpSyntaxTree.ParseText(source, parseOptions)),
            CoreReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generators, parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation updated, out ImmutableArray<Diagnostic> _);

        Dictionary<string, string> generated = driver.GetRunResult().Results
            .SelectMany(result => result.GeneratedSources)
            .ToDictionary(source => source.HintName, source => source.SourceText.ToString());

        using MemoryStream image = new();
        EmitResult emitted = updated.Emit(image);
        ImmutableArray<Diagnostic> errors = [.. emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];
        Dictionary<string, MethodImplAttributes> methods = [];

        if (emitted.Success)
        {
            image.Position = 0;
            using PEReader pe = new(image);
            MetadataReader metadata = pe.GetMetadataReader();

            foreach (MethodDefinitionHandle handle in metadata.MethodDefinitions)
            {
                MethodDefinition method = metadata.GetMethodDefinition(handle);
                methods[metadata.GetString(method.Name)] = method.ImplAttributes;
            }
        }

        return new Run(generated, errors, methods);
    }

    private static string Describe(Run run) =>
        $"errors: {string.Join("; ", run.Errors)}\r\ngenerated: {string.Join("\r\n", run.Generated.Values)}";

    private static Run CarrierRun() => Generate([new NoInliningPartialGenerator()], Stubs, Carriers);

    [TestMethod]
    public void EveryCarrierCompilesWithoutCS0759()
    {
        Run run = CarrierRun();

        Assert.AreEqual(0, run.Errors.Length, $"every carrier must gain its declaring part; {Describe(run)}");
    }

    [DataTestMethod]
    [DataRow("here")]
    [DataRow("plain")]
    [DataRow("variadic")]
    [DataRow("pair")]
    [DataRow("generic")]
    [DataRow("ptr")]
    [DataRow("val")]
    [DataRow("get")]
    [DataRow("init")]
    [DataRow("Main")]
    public void ACarrierCompilesWithTheNoInliningFlag(string name)
    {
        Run run = CarrierRun();

        Assert.IsTrue(run.Methods.TryGetValue(name, out MethodImplAttributes flags), $"{name} was not emitted; {Describe(run)}");
        Assert.IsTrue(flags.HasFlag(MethodImplAttributes.NoInlining), $"{name} lost the NoInlining flag; {Describe(run)}");
    }

    [TestMethod]
    public void TheDeclaringPartCopiesTheSignatureWithoutTheBodyOrTheOtherAttributes()
    {
        Run run = CarrierRun();
        string text = string.Join("\r\n", run.Generated.Values);

        foreach (string signature in new[]
        {
            "internal static partial (@string name, nint n) pair();",
            "internal static partial @string variadic(params ꓸꓸꓸnint xsʗp);",
            "internal static partial @string generic<T>(T x);",
            "internal static partial @string ptr(this ref counter c);",
            "internal static partial @string get<T>(this ref box<T> b);",
            "internal static partial void init();"
        })
        {
            StringAssert.Contains(text, signature, $"the declaring part must repeat the implementing part's signature exactly; {Describe(run)}");
        }

        Assert.IsFalse(text.Contains("[GoRecv]") || text.Contains("[GoInit]") || text.Contains("[GoType]"),
            $"the implementing part keeps its own attributes; the declaring part carries only MethodImpl; {Describe(run)}");
        Assert.IsFalse(text.Contains("struct counter") || text.Contains("struct box"),
            $"the generator writes methods only, never the types they sit beside; {Describe(run)}");
    }

    [TestMethod]
    public void LambdasAndLocalFunctionsKeepTheirOwnAttributeAndGainNoDeclaringPart()
    {
        Run run = Generate([new NoInliningPartialGenerator()], Stubs, Fallbacks);

        Assert.AreEqual(0, run.Errors.Length, Describe(run));
        Assert.AreEqual(0, run.Generated.Count, $"no partial method here, so nothing is generated; {Describe(run)}");

        KeyValuePair<string, MethodImplAttributes>[] lambdas = [.. run.Methods.Where(method => method.Key.StartsWith("<Main>b__"))];
        KeyValuePair<string, MethodImplAttributes>[] locals = [.. run.Methods.Where(method => method.Key.StartsWith("<Main>g__local"))];

        Assert.AreEqual(1, lambdas.Length, $"exactly one lambda body; saw {string.Join(", ", run.Methods.Keys)}");
        Assert.AreEqual(1, locals.Length, $"exactly one local function; saw {string.Join(", ", run.Methods.Keys)}");
        Assert.IsTrue(lambdas[0].Value.HasFlag(MethodImplAttributes.NoInlining), "the lambda keeps its own NoInlining");
        Assert.IsTrue(locals[0].Value.HasFlag(MethodImplAttributes.NoInlining), "the local function keeps its own NoInlining");
        Assert.IsFalse(run.Methods["here"].HasFlag(MethodImplAttributes.NoInlining), "an unmarked method stays inlinable");
    }

    [TestMethod]
    public void AHandOwnedPairOrAttributeIsNotACarrier()
    {
        Run run = Generate([new NoInliningPartialGenerator()], HandOwnedDeclaration, HandOwnedImpl);

        Assert.AreEqual(0, run.Errors.Length, $"a second declaring part would be CS0756; {Describe(run)}");
        Assert.AreEqual(0, run.Generated.Count, $"a partial method with a declaring part, or no partial at all, gets nothing; {Describe(run)}");
        Assert.IsTrue(run.Methods["nanotime"].HasFlag(MethodImplAttributes.NoInlining), "the hand-owned declaring part's attribute still merges");
        Assert.IsTrue(run.Methods["walk"].HasFlag(MethodImplAttributes.NoInlining), "a hand-owned attribute still applies");
    }

    /// <summary>
    /// RecvGenerator's ж-forwarder carries its source method's mark (RecvForwarderNoInliningTests). It
    /// cannot see NoInliningPartialGenerator's output, so for a carrier it reads the carrier SHAPE
    /// instead of the attribute.
    /// </summary>
    [DataTestMethod]
    [DataRow(" ptr(this ж<", true)]
    [DataRow(" get<T>(this ж<", true)]
    [DataRow(" plainRecv(this ж<", false)]
    public void TheBoxForwarderOfACarrierIsNotInlinable(string forwarder, bool marked)
    {
        Run run = Generate([new RecvGenerator()], Stubs, Carriers, Fallbacks.Replace("internal static @string here()", "internal static @string unused()").Replace("internal static void Main()", "internal static void Unused()"));

        string[] matches = [.. run.Generated.Values.Where(text => text.Contains(forwarder))];

        Assert.AreEqual(1, matches.Length, $"exactly one ж-forwarder for{forwarder}; {Describe(run)}");
        Assert.AreEqual(marked, matches[0].Contains("MethodImplOptions.NoInlining"),
            marked ? "a carrier's forwarder must not be inlinable either" : "the mark is inherited, never invented");
    }
}
