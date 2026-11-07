// OpenGenericImplementRecordTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// The OPEN-generic implement record, <c>[assembly: GoImplement(typeof(LocalG&lt;&gt;), typeof(localLike),
/// Pointer = true)]</c>, generates exactly the adapter a CLOSED record of the same pair generates, and the two
/// together generate it ONCE.
/// </summary>
/// <remarks>
/// <para>
/// The converter writes the open form for a conversion made at a generic type's own type parameters
/// (<c>return &amp;it</c> inside <c>NewIter[T]</c>), which no generic attribute's type argument can name. Before
/// the record existed the cast site still named the generic adapter, so the adapter was generated only when
/// some OTHER cast closed the pair, and a type reached only through its parameters was CS0246 at the cast.
/// </para>
/// <para>
/// Both forms reach the generator's existing local-generic arms: the unbound <c>typeof(LocalG&lt;&gt;)</c>
/// reads as its definition, and a closed record is collapsed to its definition there too, so the
/// de-duplication key (definition plus interface) is the same for both. The arms below assert that from the
/// generated TEXT (one adapter, byte-identical to the closed-only run) and from the compiler (no CS0101 for a
/// second class of one name).
/// </para>
/// </remarks>
[TestClass]
public class OpenGenericImplementRecordTests
{
    private const string GolibSurface =
        """
        namespace go
        {
            public class ж<T>
            {
                public static readonly ж<T> NilBox = new(default!);
                public ж(T value) { Value = value; }
                public T Value;
            }

            public interface IжAdapter { object? Box { get; } }

            public static class AdapterRegistry
            {
                public static void Register(System.Type boxType, System.Type interfaceType, System.Func<object, object> factory) { }
            }

            public sealed class GoDeferredRegistrations
            {
                public static readonly GoDeferredRegistrations Adapters = new();
                public void Defer(System.Action register) { }
            }

            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public class GoImplementAttribute : System.Attribute
            {
                public GoImplementAttribute(System.Type structDefinition, System.Type interfaceType) { }
                public bool Pointer { get; set; }
            }

            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public class GoImplementAttribute<TStruct, TInterface> : System.Attribute
            {
                public bool Promoted { get; set; }
                public bool Pointer { get; set; }
                public bool ConstraintProxy { get; set; }
            }
        }
        """;

    // The pointer subject has nistCurve[Point]'s shape: box-extension members that do not mention the type
    // parameter, so one adapter generic over TElement serves every instantiation. The value subject has
    // pair[K, V]'s shape: an instance method on the struct itself.
    private const string PackageSource =
        """
        namespace go;

        public static partial class probe_package
        {
            public partial struct LocalG<TElement>
            {
            }

            internal static bool Next<TElement>(this ж<LocalG<TElement>> Ꮡg) => false;

            public partial struct LocalV<TKey, TValue>
            {
                public TKey k;

                public @string Name() => default;
            }

            internal partial interface localLike
            {
                bool Next();
            }

            internal partial interface namer
            {
                @string Name();
            }

            public struct @string { }
        }
        """;

    private const string OpenPointer = "[assembly: GoImplement(typeof(go.probe_package.LocalG<>), typeof(go.probe_package.localLike), Pointer = true)]";
    private const string ClosedPointer = "[assembly: GoImplement<go.probe_package.LocalG<long>, go.probe_package.localLike>(Pointer = true)]";
    private const string OpenValue = "[assembly: GoImplement(typeof(go.probe_package.LocalV<,>), typeof(go.probe_package.namer))]";
    private const string ClosedValue = "[assembly: GoImplement<go.probe_package.LocalV<long, bool>, go.probe_package.namer>]";

    private static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"));
    }

    /// <summary>
    /// Runs the REAL generator over one compilation carrying the given records and answers the generated sources
    /// (keyed by hint name) and the compilation they were folded into.
    /// </summary>
    private static (Dictionary<string, string> Sources, Compilation Updated) Run(params string[] records)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);

        CSharpCompilation compilation = CSharpCompilation.Create(
            "open-generic-implement",
            [
                CSharpSyntaxTree.ParseText(GolibSurface, parseOptions),
                // The records head the package class's own file, as they do in package_info.cs: the generator places
                // each implementation in the package class its record's file declares.
                CSharpSyntaxTree.ParseText("using go;\n\n" + string.Join("\n", records) + "\n\n" + PackageSource, parseOptions)
            ],
            CoreReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ImplementGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation updated, out ImmutableArray<Diagnostic> generatorDiagnostics);

        // A generator that throws reports CS8785 and emits nothing, which would read as "no adapter" below.
        Assert.AreEqual("", string.Join("; ", generatorDiagnostics.Select(diagnostic => $"{diagnostic.Id} {diagnostic.GetMessage()}")),
            "the generator must run without diagnostics");

        Dictionary<string, string> sources = driver.GetRunResult().Results
            .SelectMany(generator => generator.GeneratedSources)
            .ToDictionary(source => source.HintName, source => source.SourceText.ToString());

        return (sources, updated);
    }

    // Every generated source that declares the given type, so a duplicate shows up as a count of two.
    private static List<string> Declaring(Dictionary<string, string> sources, string declaration) =>
        sources.Values.Where(text => text.Contains(declaration)).ToList();

    private static string Errors(Compilation compilation) =>
        string.Join("; ", compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Select(diagnostic => $"{diagnostic.Id} {diagnostic.GetMessage()}"));

    private const string PointerAdapter = "class LocalGжlocalLike<TElement> : global::go.probe_package.localLike, IжAdapter";

    private const string ValuePartial = "partial struct LocalV<TKey, TValue> : global::go.probe_package.namer";

    /// <summary>
    /// The open pointer record alone generates the generic adapter, and it compiles.
    /// </summary>
    [TestMethod]
    public void AnOpenPointerRecordGeneratesTheGenericAdapter()
    {
        (Dictionary<string, string> sources, Compilation updated) = Run(OpenPointer);

        Assert.AreEqual("", Errors(updated), "the open record's adapter must compile");
        Assert.AreEqual(1, Declaring(sources, PointerAdapter).Count, $"one adapter was expected, saw: {string.Join(" | ", sources.Keys)}");
    }

    /// <summary>
    /// The dedupe claim: an open record and a closed record of ONE pair generate ONE adapter, and its text is
    /// byte-identical to the adapter the closed record generates alone. This is what keeps an existing closed
    /// pair (crypto/ecdh's nistCurve) unchanged when its open record joins it.
    /// </summary>
    [TestMethod]
    public void AnOpenAndAClosedPointerRecordOfOnePairGenerateOneAdapter()
    {
        (Dictionary<string, string> both, Compilation updated) = Run(OpenPointer, ClosedPointer);
        (Dictionary<string, string> closedOnly, _) = Run(ClosedPointer);

        Assert.AreEqual("", Errors(updated), "the two records together must compile (a second class of one name is CS0101)");

        List<string> adapters = Declaring(both, PointerAdapter);
        List<string> closedAdapters = Declaring(closedOnly, PointerAdapter);

        Assert.AreEqual(1, adapters.Count, $"one adapter was expected, saw: {string.Join(" | ", both.Keys)}");
        Assert.AreEqual(1, closedAdapters.Count, "control: the closed record alone generates one adapter");
        Assert.AreEqual(closedAdapters[0], adapters[0], "the adapter must be byte-identical to the closed record's");
    }

    /// <summary>
    /// The value form: an open record and a closed record of one pair generate ONE implementing partial, the same
    /// text the closed record generates alone, and the open record alone generates it too.
    /// </summary>
    [TestMethod]
    public void AnOpenAndAClosedValueRecordOfOnePairGenerateOnePartial()
    {
        (Dictionary<string, string> both, Compilation updated) = Run(OpenValue, ClosedValue);
        (Dictionary<string, string> closedOnly, _) = Run(ClosedValue);
        (Dictionary<string, string> openOnly, Compilation openUpdated) = Run(OpenValue);

        Assert.AreEqual("", Errors(updated), "the two value records together must compile");
        Assert.AreEqual("", Errors(openUpdated), "the open value record alone must compile");

        List<string> partials = Declaring(both, ValuePartial);
        List<string> closedPartials = Declaring(closedOnly, ValuePartial);

        Assert.AreEqual(1, partials.Count, $"one partial was expected, saw: {string.Join(" | ", both.Keys)}");
        Assert.AreEqual(1, closedPartials.Count, "control: the closed record alone generates one partial");
        Assert.AreEqual(closedPartials[0], partials[0], "the partial must be byte-identical to the closed record's");
        Assert.AreEqual(1, Declaring(openOnly, ValuePartial).Count, "the open value record alone generates the partial");
    }
}
