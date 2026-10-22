// ForeignGenericAdapterTests.cs - Gbtc
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
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// A GoImplement record naming a struct that is BOTH foreign (declared in a referenced assembly)
/// AND generic must produce an adapter that compiles: its class identifier can carry no type-argument
/// list, and the wrapped type must be qualified enough to resolve from the generated file.
/// </summary>
/// <remarks>
/// <para>
/// The corpus had no foreign-and-generic pair until Go 1.24.13 moved <c>sync.Map</c>'s reference
/// model onto <c>internal/sync</c>: <c>sync/map_reference_test.go:32</c> writes
/// <c>_ mapInterface = &amp;isync.HashTrieMap[any, any]{}</c>, so the <c>-tests</c> pipeline records
/// <c>[assembly: GoImplement&lt;go.@internal.sync_package.HashTrieMap&lt;any, any&gt;, mapInterface&gt;(Pointer = true)]</c>
/// in sync's package_test_info.cs. The pointer-adapter block guarded its generic branch with
/// <c>!foreignStruct</c>, so both non-generic fallbacks misfired on that record and the generated
/// adapter was 32 errors in one file:
/// </para>
/// <list type="bullet">
///   <item>the adapter NAME composed from <c>GetFullTypeName</c>, which spells a generic as
///   <c>Name&lt;typeArgs&gt;</c> — so the argument list landed INSIDE the class identifier
///   (<c>class sync_HashTrieMap&lt;Object, Object&gt;жmapInterface</c>): CS0692 for the two type
///   parameters both named <c>Object</c>, then the ctor parsed as a method and every member fell
///   into the enclosing static partial class (CS0708 ×17, CS0540 ×10, CS0548, CS0050);</item>
///   <item>the WRAPPED type came from the same helper, whose generic case drops everything left of
///   the name, so <c>GlobalQualify</c> found no <c>go.</c> prefix to qualify and
///   <c>ж&lt;HashTrieMap&lt;object, object&gt;&gt;</c> bound nothing: CS0246 ×2.</item>
/// </list>
/// <para>
/// A CLOSED foreign instantiation is the only foreign-generic form that can reach here — an
/// assembly-level attribute cannot name an open one — so it takes a NON-generic adapter over the
/// closed type: the identifier is the package-qualified BARE name, and the type arguments live only
/// in the wrapped type. The open-generic route the LOCAL branch takes is unrepresentable here, and
/// not for want of a constraint (the converted <c>HashTrieMap&lt;K, V&gt;</c> declares none): the
/// interface is non-generic and typed at the CLOSED arguments (<c>Load(any) (any, bool)</c>) while
/// the struct's extensions are typed at the parameters, so an adapter open over <c>&lt;K, V&gt;</c>
/// would pass <c>object</c> where <c>K</c> is expected — CS1503 on every member.
/// </para>
/// <para>
/// A THIRD facet of the same defect is guarded by
/// <see cref="AForeignClosedGenericAdapterForwardsThroughThePackageClassStatic"/>: the metadata scan
/// that finds a foreign struct's box extensions compared a CONSTRUCTED generic against the OPEN
/// receiver those extensions declare, so it bound nothing and every member fell back to
/// <c>m_box.Value.&lt;name&gt;</c> — which binds nothing either, because a Go pointer-receiver
/// method is a package-class extension and not an instance member.
/// </para>
/// <para>
/// ⚠ COLLISION HAZARD, stated rather than defended against. The foreign name carries no
/// per-instantiation suffix, so two DIFFERENT closed instantiations of one foreign generic against
/// one interface compose one class twice (CS0102). A suffix cannot be used: the CONVERTER composes
/// the same name at the cast site and spells the arguments in Go-alias form (<c>any</c>) where this
/// generator spells them in C# keyword form (<c>object</c>), so a name derived from the argument
/// spelling cannot be kept in sync across the two halves. The generator's de-duplication key is the
/// CLOSED instantiation, so that case fails LOUDLY at CS0102 rather than silently binding the first
/// instantiation's arguments. Unreachable from Go whenever the interface mentions the type
/// arguments, which is the only case that reaches this branch at all; no corpus instance.
/// </para>
/// <para>
/// ⚠ The two CONTROLS are green before and after the fix, and they are read from the generated TEXT
/// rather than from a compile, deliberately: before the fix the shared compilation cannot compile at
/// all (the subject's adapter is in it), so a control phrased as "the compilation is clean" would be
/// red for a reason that has nothing to do with the axis it guards. The local-generic arm
/// (<c>nistCurve&lt;Point&gt;</c>'s shape) and the foreign NON-generic arm (<c>testing_TжTB</c>'s
/// shape) sit in the SAME two compilations as the subject, so the three differ on exactly the two
/// axes under test — foreign vs local, and generic vs not.
/// </para>
/// </remarks>
[TestClass]
public class ForeignGenericAdapterTests
{
    // The referenced ("production") assembly: the minimal golib surface the generated adapter binds
    // — the box with its canonical typed nil, the IжAdapter unwrap seam, the registry the module
    // initializer calls, and the attribute the generator matches by full name — plus the two foreign
    // subjects. Both live in a NESTED namespace segment, mirroring internal/sync's `go.@internal`
    // against sync's own `go`, so an unqualified reference to either cannot resolve by accident.
    private const string ProductionSource =
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
            public class GoImplementAttribute<TStruct, TInterface> : System.Attribute
            {
                public bool Promoted { get; set; }
                public bool Pointer { get; set; }
                public bool ConstraintProxy { get; set; }
            }
        }

        namespace go.@internal
        {
            public static partial class pkg_package
            {
                // The foreign GENERIC subject — internal/sync's HashTrieMap[K, V] shape, members and
                // all. A Go POINTER-receiver method converts to a public package-class EXTENSION
                // over the box, declared OPEN over the struct's own parameters; the struct itself
                // has no instance member of that name, which is why a foreign generic's forwarding
                // cannot fall back to `m_box.Value.<name>`.
                public partial struct G<TK, TV>
                {
                }

                public static TV Get<TK, TV>(this ж<G<TK, TV>> Ꮡg, TK key) => default!;

                public static void Delete<TK, TV>(this ж<G<TK, TV>> Ꮡg, TK key) { }

                // The foreign NON-generic control — testing.T's shape against testing.TB, with its
                // members in the same box-extension form.
                public partial struct T
                {
                }

                public static object Get(this ж<T> Ꮡt, object key) => default!;

                public static void Delete(this ж<T> Ꮡt, object key) { }
            }
        }
        """;

    // The consuming ("test") assembly: three GoImplement records over one package class — the
    // foreign generic subject, the foreign non-generic control and the local generic control — so
    // the subject and both controls are generated by ONE generator run over ONE compilation and
    // differ on the axes under test rather than on the fixture.
    private const string ConsumingSource =
        """
        using go;

        [assembly: GoImplement<global::go.@internal.pkg_package.G<object, object>, global::go.probe_package.mapLike>(Pointer = true)]
        [assembly: GoImplement<global::go.@internal.pkg_package.T, global::go.probe_package.namedLike>(Pointer = true)]
        [assembly: GoImplement<global::go.probe_package.LocalG<object>, global::go.probe_package.localLike>(Pointer = true)]

        namespace go;

        public static partial class probe_package
        {
            // The LOCAL generic control — crypto/elliptic's nistCurve[Point] shape: the record names a
            // CLOSED instantiation and the adapter must still collapse to ONE open generic class.
            // Its members take the same box-extension form as the foreign ones (that is how the
            // converter emits a Go pointer-receiver method), but they are declared HERE, so the
            // adapter binds them by extension lookup — `m_box.Get(key)` — rather than through a
            // package-class static. Neither the interface nor these members mentions TElement, which
            // is exactly why every instantiation satisfies localLike and one GENERIC adapter serves.
            public partial struct LocalG<TElement>
            {
            }

            internal static object Get<TElement>(this ж<LocalG<TElement>> Ꮡg, object key) => default!;

            internal static void Delete<TElement>(this ж<LocalG<TElement>> Ꮡg, object key) { }

            internal partial interface mapLike
            {
                object Get(object key);
                void Delete(object key);
            }

            internal partial interface namedLike
            {
                object Get(object key);
                void Delete(object key);
            }

            internal partial interface localLike
            {
                object Get(object key);
                void Delete(object key);
            }
        }
        """;

    private static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"));
    }

    /// <summary>
    /// Runs the REAL generator over the two-assembly shape and hands back both the generated adapter
    /// sources (keyed by hint name) and the compilation they were folded into, so an arm can read
    /// either the emitted TEXT or the compiler's own verdict on it.
    /// </summary>
    private static (Dictionary<string, string> Adapters, Compilation Updated) RunImplementGenerator()
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);

        CSharpCompilation production = CSharpCompilation.Create(
            "foreign-generic-production",
            [CSharpSyntaxTree.ParseText(ProductionSource, parseOptions)],
            CoreReferences(),
            libraryOptions);

        using MemoryStream image = new();
        EmitResult emitted = production.Emit(image);
        Assert.IsTrue(emitted.Success, $"production compilation must emit clean: {string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))}");

        CSharpCompilation consuming = CSharpCompilation.Create(
            "foreign-generic-test",
            [CSharpSyntaxTree.ParseText(ConsumingSource, parseOptions)],
            CoreReferences().Append(MetadataReference.CreateFromImage(image.ToArray())),
            libraryOptions);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ImplementGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(consuming, out Compilation updated, out ImmutableArray<Diagnostic> _);

        Dictionary<string, string> adapters = driver.GetRunResult().Results
            .SelectMany(generator => generator.GeneratedSources)
            .ToDictionary(source => source.HintName, source => source.SourceText.ToString());

        return (adapters, updated);
    }

    /// <summary>
    /// Picks the one generated source whose hint name carries every supplied fragment. Hint names run
    /// through <c>GetValidFileName</c>, which maps '&lt;' and '&gt;' to '_' and drops '@' — so a
    /// fragment naming a generic instantiation is spelled the way the FILE name spells it
    /// ("G_Object, Object_"), not the way the type displays.
    /// </summary>
    private static string Adapter(Dictionary<string, string> adapters, params string[] hintFragments)
    {
        List<string> matches = adapters
            .Where(entry => hintFragments.All(fragment => entry.Key.Contains(fragment)))
            .Select(entry => entry.Value)
            .ToList();

        Assert.AreEqual(1, matches.Count,
            $"exactly one generated source must match [{string.Join(", ", hintFragments)}] — saw {matches.Count} of: {string.Join(" | ", adapters.Keys)}");

        return matches[0];
    }

    private static IEnumerable<Diagnostic> Errors(Compilation compilation) =>
        compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

    #region [ Subject — the foreign CLOSED generic pair ]

    /// <summary>
    /// The whole point: with a foreign-and-generic record in the compilation, the compilation
    /// COMPILES. Before the fix this arm is CS0692 + CS0246 + the CS0708/CS0540/CS0548/CS0050
    /// cascade, all inside the one generated adapter file.
    /// </summary>
    [TestMethod]
    public void AForeignClosedGenericPairProducesAnAdapterThatCompiles()
    {
        (Dictionary<string, string> adapters, Compilation updated) = RunImplementGenerator();

        List<Diagnostic> errors = Errors(updated).ToList();

        Assert.AreEqual(0, errors.Count,
            $"the generated adapters must compile: {string.Join("; ", errors.Select(error => $"{error.Id} {error.GetMessage()}"))}");

        Assert.IsTrue(adapters.Count >= 3, $"one adapter per record was expected, saw: {string.Join(" | ", adapters.Keys)}");
    }

    /// <summary>
    /// The identifier rule, read directly rather than inferred from the compile: the adapter's class
    /// name is minted from the symbol's BARE name plus a sanitized-argument suffix, so no '&lt;' can
    /// reach the identifier; and the wrapped type is fully qualified so it resolves from a file that
    /// imports only <c>go</c>.
    /// </summary>
    [TestMethod]
    public void AForeignClosedGenericAdapterCarriesNoTypeArgumentListInItsIdentifier()
    {
        (Dictionary<string, string> adapters, _) = RunImplementGenerator();

        string adapter = Adapter(adapters, "G_Object, Object_", "mapLike");

        StringAssert.Contains(adapter, "class pkg_GжmapLike : global::go.probe_package.mapLike, IжAdapter");
        StringAssert.Contains(adapter, "private readonly ж<global::go.@internal.pkg_package.G<object, object>> m_box;");
        StringAssert.Contains(adapter, "public pkg_GжmapLike(ж<global::go.@internal.pkg_package.G<object, object>> box)");

        // No '<' may reach the identifier — that is the CS0692 cascade's entire cause.
        Assert.IsFalse(adapter.Contains("class pkg_G<"), "the adapter identifier must carry no type-argument list");

        // A NON-generic adapter registers, so the closed instantiation is reachable from golib's
        // type-assert machinery rather than stranded on the nominal path: its own registration,
        // called from the package's one registration that the module initializer defers.
        StringAssert.Contains(adapter, "internal static void ᴛRegisterAdapter() =>");
        StringAssert.Contains(adapter, "global::go.AdapterRegistry.Register(typeof(ж<global::go.@internal.pkg_package.G<object, object>>)");

        string registry = adapters.Single(pair => pair.Key.EndsWith(".AdapterRegistry.g.cs")).Value;

        StringAssert.Contains(registry, "[global::System.Runtime.CompilerServices.ModuleInitializer]");
        StringAssert.Contains(registry, "global::go.GoDeferredRegistrations.Adapters.Defer(ᴛRegisterAdapters);");
        StringAssert.Contains(registry, "pkg_GжmapLike.ᴛRegisterAdapter();");
    }

    /// <summary>
    /// The THIRD facet: a foreign generic's members must forward through the package-class STATIC
    /// with the BOX as the receiver argument. The metadata scan that decides this compared the
    /// CONSTRUCTED struct against the OPEN receiver its extensions declare and bound nothing, so
    /// every member fell back to <c>m_box.Value.&lt;name&gt;</c> — a form that binds nothing at all
    /// here, because a Go pointer-receiver method is an extension and not an instance member.
    /// </summary>
    [TestMethod]
    public void AForeignClosedGenericAdapterForwardsThroughThePackageClassStatic()
    {
        (Dictionary<string, string> adapters, _) = RunImplementGenerator();

        string adapter = Adapter(adapters, "G_Object, Object_", "mapLike");

        StringAssert.Contains(adapter, "=> global::go.@internal.pkg_package.Get(m_box, key);");
        StringAssert.Contains(adapter, "=> global::go.@internal.pkg_package.Delete(m_box, key);");

        Assert.IsFalse(adapter.Contains("m_box.Value."),
            "a foreign generic's members live on the box extensions, never on the struct value");
    }

    #endregion

    #region [ Controls — green BEFORE and AFTER, read from the emitted text ]

    /// <summary>
    /// CONTROL (local generic, crypto/elliptic's <c>nistCurve[Point]</c>): a record naming a CLOSED
    /// instantiation of a LOCALLY declared generic still collapses to ONE adapter generic over the
    /// struct's OPEN type parameters — and still carries no module initializer, because an open
    /// registration key is unrepresentable. Nothing on this path moves.
    /// </summary>
    [TestMethod]
    public void ALocalGenericPairKeepsItsOpenGenericAdapter()
    {
        (Dictionary<string, string> adapters, _) = RunImplementGenerator();

        string adapter = Adapter(adapters, "LocalG_Object_", "localLike");

        StringAssert.Contains(adapter, "class LocalGжlocalLike<TElement> : global::go.probe_package.localLike, IжAdapter");
        StringAssert.Contains(adapter, "private readonly ж<LocalG<TElement>> m_box;");
        StringAssert.Contains(adapter, "public LocalGжlocalLike(ж<LocalG<TElement>> box)");
        StringAssert.Contains(adapter, "=> m_box.Get(key);");

        Assert.IsFalse(adapter.Contains("ModuleInitializer"),
            "a GENERIC adapter cannot host a module initializer — its open registration key is unrepresentable");
    }

    /// <summary>
    /// CONTROL (foreign, NOT generic — <c>testing_TжTB</c>'s shape): the package-qualified name and
    /// the fully-qualified wrapped type are exactly what they were, and the initializer still
    /// registers. This is the path the fix must leave byte-identical.
    /// </summary>
    [TestMethod]
    public void AForeignNonGenericPairKeepsItsPackageQualifiedAdapter()
    {
        (Dictionary<string, string> adapters, _) = RunImplementGenerator();

        string adapter = Adapter(adapters, ".T-", "namedLike");

        StringAssert.Contains(adapter, "class pkg_TжnamedLike : global::go.probe_package.namedLike, IжAdapter");
        StringAssert.Contains(adapter, "private readonly ж<global::go.@internal.pkg_package.T> m_box;");
        StringAssert.Contains(adapter, "public pkg_TжnamedLike(ж<global::go.@internal.pkg_package.T> box)");
        StringAssert.Contains(adapter, "global::go.AdapterRegistry.Register(typeof(ж<global::go.@internal.pkg_package.T>)");
        StringAssert.Contains(adapter, "=> global::go.@internal.pkg_package.Get(m_box, key);");
    }

    #endregion
}
