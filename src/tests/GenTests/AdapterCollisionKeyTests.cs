// AdapterCollisionKeyTests.cs - Gbtc
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
/// The two OPERANDS of a pointer adapter's collision key — the struct side's key and the interface
/// side's minted name — must derive the same identity for the same type as the converter's own
/// halves do, and neither may carry a generic type-argument list.
/// </summary>
/// <remarks>
/// <para>
/// The foreign-generic seat made a foreign generic STRUCT reach the pointer-adapter path for the
/// first time and left both operands behind:
/// </para>
/// <list type="bullet">
///   <item>STRUCT SIDE. <c>AdapterStructKey</c> reduces the struct's display string with
///   <c>GetSimpleName</c>, which splits on the LAST '.' and drops a type-argument list only when
///   asked — and it is not asked. The converter's <c>splitAdapterStructReference</c> does the two
///   operations in the OPPOSITE order and its doc states that order as load-bearing
///   (<c>"bytes_package.Reader&lt;int&gt;"</c> → <c>("bytes_package", "Reader")</c>), so the two
///   halves key one struct two ways. Reversed, the last-dot scan lands INSIDE the argument list and
///   the "simple name" becomes the argument's own tail segment: <c>nistCurve&lt;P224PointжnistPoint&gt;</c>
///   keys as <c>P224PointжnistPoint&gt;</c>, and <c>a.G&lt;b.T&gt;</c> keys as <c>T&gt;</c>.</item>
///   <item>INTERFACE SIDE. <c>AdapterName</c> mints the interface half of the class identifier from
///   the same helper, so a record naming a GENERIC interface puts an argument list inside the
///   identifier — the exact defect the seat fixed on the struct side, one operand over. Zero
///   generic-interface records carry <c>Pointer = true</c> in the corpus today; the mlkem
///   func-result projection is the first row that reaches it.</item>
/// </list>
/// <para>
/// ⚠ Scope, stated so the residue is not mistaken for coverage. Only the minted NAME takes the strip
/// on the interface side. The two KEY compositions — the pre-pass's grouping key and this expression's
/// <c>collidingAdapterNames</c> lookup — deliberately keep the last-dot-only reduction, because the
/// converter's <c>adapterInterfaceSimpleName</c> keeps it too: both halves garble a generic interface
/// reference IDENTICALLY, which is parity, and stripping on one side alone would manufacture the very
/// divergence the struct-side fix removes. The consequence is the seat's own ruled behaviour, now
/// symmetric across both operands: two records that compose one class name and are NOT seen as a
/// collision fail LOUDLY at CS0102 rather than binding the first one silently.
/// </para>
/// <para>
/// ⚠ The VALUE (non-pointer) adapter path is NOT in scope and does NOT share the key defect: it never
/// calls <c>AdapterStructKey</c> and never consults <c>collidingAdapterNames</c> — both call sites are
/// gated on <c>Pointer = true</c>. It does carry the seat's IDENTIFIER defect, measured by
/// <see cref="AForeignGenericValueRecordIsUnmovedByTheKeyFix"/>, and fixing that needs the converter's
/// <c>valueAdapterTypeRef</c> half in the same commit — a separate seat, not this one.
/// </para>
/// </remarks>
[TestClass]
public class AdapterCollisionKeyTests
{
    // The referenced ("production") assembly: the minimal golib surface a generated adapter binds,
    // plus a foreign GENERIC struct, a foreign NON-generic struct, and a foreign interface whose
    // SIMPLE NAME collides with one the consuming assembly declares. Everything foreign lives under
    // a nested namespace segment so no unqualified reference can resolve by accident.
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
                // The foreign GENERIC subject — internal/sync's HashTrieMap[K, V] shape. Its Go
                // pointer-receiver methods convert to package-class EXTENSIONS over the box,
                // declared OPEN over the struct's own parameters.
                public partial struct G<TK, TV>
                {
                }

                public static TV Get<TK, TV>(this ж<G<TK, TV>> Ꮡg, TK key) => default!;

                public static void Delete<TK, TV>(this ж<G<TK, TV>> Ꮡg, TK key) { }

                // The foreign NON-generic control — testing.T's shape against testing.TB.
                public partial struct T
                {
                }

                public static object Get(this ж<T> Ꮡt, object key) => default!;

                public static void Delete(this ж<T> Ꮡt, object key) { }

                // A FOREIGN interface whose simple name is also declared by the consuming package —
                // compress/flate's own `Reader` against `io.Reader`, the shape the collision
                // pre-pass exists for. Typed at the arguments of the instantiation that adapts to
                // it, because a non-generic interface that mentions the type arguments can be
                // satisfied by exactly one.
                public interface mapLike
                {
                    int Get(int key);
                    void Delete(int key);
                }
            }
        }
        """;

    // SUBJECT fixture: ONE foreign generic struct, TWO closed instantiations, TWO interfaces that
    // share the simple name `mapLike`. This is the shape the collision pre-pass was written for, and
    // it is reachable only now that a foreign generic compiles at all.
    //
    //   struct key BEFORE the fix   pkg_G<object, object>  /  pkg_G<int, int>   -> two groups of one
    //                               -> neither is "colliding" -> both mint the BARE name
    //                               -> one class declared twice (CS0102)
    //   struct key AFTER the fix    pkg_G both times                            -> ONE group of two
    //                               -> colliding -> the FOREIGN interface takes its package prefix
    private const string CollidingInterfacesSource =
        """
        using go;

        [assembly: GoImplement<global::go.@internal.pkg_package.G<object, object>, global::go.probe_package.mapLike>(Pointer = true)]
        [assembly: GoImplement<global::go.@internal.pkg_package.G<int, int>, global::go.@internal.pkg_package.mapLike>(Pointer = true)]
        [assembly: GoImplement<global::go.@internal.pkg_package.T, global::go.probe_package.namedLike>(Pointer = true)]

        namespace go;

        public static partial class probe_package
        {
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
        }
        """;

    // CONTROL fixture for the "only where the converter would also collide" half: ONE foreign generic
    // struct, TWO closed instantiations, ONE interface. The merged key groups them together, but a
    // group holds ONE distinct interface, so nothing is qualified and both still mint the bare name —
    // the seat's ruled LOUD failure, unchanged by this commit.
    //
    // ⚠ Read from the emitted TEXT only, never compiled: a non-generic interface that mentions the
    // type arguments admits exactly ONE instantiation, so the second adapter is CS1503 by
    // construction and a compile verdict here would measure the fixture rather than the axis.
    private const string SharedInterfaceSource =
        """
        using go;

        [assembly: GoImplement<global::go.@internal.pkg_package.G<object, object>, global::go.probe_package.mapLike>(Pointer = true)]
        [assembly: GoImplement<global::go.@internal.pkg_package.G<int, int>, global::go.probe_package.mapLike>(Pointer = true)]

        namespace go;

        public static partial class probe_package
        {
            internal partial interface mapLike
            {
                object Get(object key);
                void Delete(object key);
            }
        }
        """;

    // INTERFACE-SIDE fixture: crypto/mlkem's shape — a NON-generic local struct recorded against a
    // CLOSED instantiation of a GENERIC interface. Before the fix the minted identifier is the
    // argument's own tail segment (`digestжnamed>`), which does not parse.
    private const string GenericInterfaceSource =
        """
        using go;

        [assembly: GoImplement<global::go.probe_package.digest, global::go.probe_package.keyedLike<global::go.probe_package.named>>(Pointer = true)]

        namespace go;

        public static partial class probe_package
        {
            public partial struct named
            {
            }

            public partial struct digest
            {
            }

            internal static named encapKey(this ж<digest> Ꮡd) => default!;

            internal partial interface keyedLike<TKey>
            {
                TKey encapKey();
            }
        }
        """;

    // VALUE-PATH fixture: the same foreign generic struct with NO Pointer flag, so the record takes
    // the value adapter. Measures the residue rather than asserting a fix.
    private const string ValuePathSource =
        """
        using go;

        [assembly: GoImplement<global::go.@internal.pkg_package.G<object, object>, global::go.probe_package.mapLike>]

        namespace go;

        public static partial class probe_package
        {
            internal partial interface mapLike
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
    /// Runs the REAL generator over the two-assembly shape for one consuming source, and hands back
    /// both the generated sources (keyed by hint name) and the compilation they were folded into.
    /// </summary>
    private static (Dictionary<string, string> Adapters, Compilation Updated) RunImplementGenerator(string consumingSource)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);

        CSharpCompilation production = CSharpCompilation.Create(
            "collision-key-production",
            [CSharpSyntaxTree.ParseText(ProductionSource, parseOptions)],
            CoreReferences(),
            libraryOptions);

        using MemoryStream image = new();
        EmitResult emitted = production.Emit(image);
        Assert.IsTrue(emitted.Success, $"production compilation must emit clean: {string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))}");

        CSharpCompilation consuming = CSharpCompilation.Create(
            "collision-key-test",
            [CSharpSyntaxTree.ParseText(consumingSource, parseOptions)],
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
    /// through <c>GetValidFileName</c>, which maps '&lt;' and '&gt;' to '_' — so a fragment naming a
    /// generic instantiation is spelled the way the FILE name spells it ("G_Object, Object_").
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

    /// <summary>
    /// The class identifier a generated adapter declares, read out of the emitted text so an arm can
    /// name it exactly rather than asserting a substring that a longer name would also satisfy.
    /// </summary>
    private static string DeclaredAdapterName(string adapter)
    {
        // "sealed class", not "class": every adapter template declares the adapter sealed, while the
        // containing package class it nests in is `static partial` — a bare " class " scan reads the
        // WRAPPER's name and every arm then fails identically, for the instrument rather than for its
        // axis (measured: five of five reported `probe_package`).
        string marker = " sealed class ";
        int start = adapter.IndexOf(marker, System.StringComparison.Ordinal);

        Assert.IsTrue(start >= 0, $"no class declaration found in the generated adapter:\r\n{adapter}");

        start += marker.Length;

        // Terminate at the base list, NOT at whitespace: an identifier that still carries a
        // type-argument list spells a space inside it ("pkg_G<Object, Object>ᴠmapLike"), and a
        // space-terminated read truncates the very defect an arm is measuring.
        int end = adapter.IndexOfAny([':', '\r', '\n'], start);

        return (end < 0 ? adapter[start..] : adapter[start..end]).Trim();
    }

    private static IEnumerable<Diagnostic> Errors(Compilation compilation) =>
        compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

    #region [ Subject — the STRUCT-side operand ]

    /// <summary>
    /// The struct-side key must drop the type-argument list and THEN take the last path segment, the
    /// order <c>splitAdapterStructReference</c> documents — so two closed instantiations of one
    /// foreign generic land in ONE collision group, the interface side is qualified, and the two
    /// adapters carry different identifiers. Before the fix the two instantiations key apart, neither
    /// group holds a second interface, and both adapters mint <c>pkg_GжmapLike</c>: one class declared
    /// twice.
    /// </summary>
    [TestMethod]
    public void TwoClosedInstantiationsOfOneForeignGenericShareAStructKey()
    {
        (Dictionary<string, string> adapters, Compilation updated) = RunImplementGenerator(CollidingInterfacesSource);

        string localInterfaceAdapter = Adapter(adapters, "G_Object, Object_", "mapLike");
        string foreignInterfaceAdapter = Adapter(adapters, "G_Int32, Int32_", "mapLike");

        string localName = DeclaredAdapterName(localInterfaceAdapter);
        string foreignName = DeclaredAdapterName(foreignInterfaceAdapter);

        Assert.AreNotEqual(localName, foreignName,
            $"two interfaces sharing a simple name must not compose one class twice — both minted {localName}");

        // At most one member of a colliding group can be LOCAL, so it keeps the bare Go-like form
        // and the FOREIGN one takes its package prefix.
        Assert.AreEqual("pkg_GжmapLike", localName);
        Assert.AreEqual("pkg_Gжpkg_mapLike", foreignName);

        List<Diagnostic> errors = Errors(updated).ToList();

        Assert.AreEqual(0, errors.Count,
            $"the generated adapters must compile: {string.Join("; ", errors.Select(error => $"{error.Id} {error.GetMessage()}"))}");
    }

    /// <summary>
    /// CONTROL for the "only where the converter would also collide" half: merging the key must not
    /// invent a qualification. Two closed instantiations against the SAME interface group together
    /// and the group holds ONE distinct interface, so both adapters keep the bare name — the seat's
    /// ruled LOUD duplicate, not a silent rename. Green before and after, read from the text.
    /// </summary>
    [TestMethod]
    public void TwoClosedInstantiationsAgainstOneInterfaceAreNotQualified()
    {
        (Dictionary<string, string> adapters, _) = RunImplementGenerator(SharedInterfaceSource);

        Assert.AreEqual("pkg_GжmapLike", DeclaredAdapterName(Adapter(adapters, "G_Object, Object_", "mapLike")));
        Assert.AreEqual("pkg_GжmapLike", DeclaredAdapterName(Adapter(adapters, "G_Int32, Int32_", "mapLike")));
    }

    #endregion

    #region [ Subject — the INTERFACE-side operand ]

    /// <summary>
    /// A record naming a CLOSED instantiation of a GENERIC interface mints an adapter whose identifier
    /// carries no type-argument list — the interface half of the seat's identifier rule. Before the
    /// fix the last-dot scan runs inside the argument list and the identifier is the argument's tail
    /// segment (<c>digestжnamed&gt;</c>), which does not parse; the interface the class IMPLEMENTS
    /// keeps its arguments, because that is a type reference and not an identifier.
    /// </summary>
    [TestMethod]
    public void AGenericInterfaceRecordMintsAnAdapterWithNoTypeArgumentList()
    {
        (Dictionary<string, string> adapters, Compilation updated) = RunImplementGenerator(GenericInterfaceSource);

        string adapter = Adapter(adapters, "digest-", "keyedLike");

        Assert.AreEqual("digestжkeyedLike", DeclaredAdapterName(adapter));

        // The implemented interface is a type reference, named through its own class since the
        // cross-class generic fix (CrossClassGenericInterfaceAdapterTests); the identifier is unchanged.
        StringAssert.Contains(adapter, "class digestжkeyedLike : global::go.probe_package.keyedLike<global::go.probe_package.named>, IжAdapter");
        StringAssert.Contains(adapter, "public digestжkeyedLike(ж<digest> box)");

        List<Diagnostic> errors = Errors(updated).ToList();

        Assert.AreEqual(0, errors.Count,
            $"the generated adapter must compile: {string.Join("; ", errors.Select(error => $"{error.Id} {error.GetMessage()}"))}");
    }

    #endregion

    #region [ Controls — byte-identical before and after ]

    /// <summary>
    /// CONTROL: a NON-generic record's names are untouched. No non-generic reference contains a '&lt;',
    /// so both strips are no-ops for it — the foreign struct keeps its package-qualified identifier and
    /// the interface side keeps its bare simple name. This sits in the SAME compilation as the subject,
    /// so it differs from it on exactly the axis under test.
    /// </summary>
    [TestMethod]
    public void ANonGenericRecordKeepsItsNamesUnchanged()
    {
        (Dictionary<string, string> adapters, _) = RunImplementGenerator(CollidingInterfacesSource);

        string adapter = Adapter(adapters, ".T-", "namedLike");

        Assert.AreEqual("pkg_TжnamedLike", DeclaredAdapterName(adapter));

        StringAssert.Contains(adapter, "private readonly ж<global::go.@internal.pkg_package.T> m_box;");
        StringAssert.Contains(adapter, "global::go.AdapterRegistry.Register(typeof(ж<global::go.@internal.pkg_package.T>)");
        StringAssert.Contains(adapter, "=> global::go.@internal.pkg_package.Get(m_box, key);");
    }

    /// <summary>
    /// CONTROL and MEASUREMENT, answering the question the design read could not close without a
    /// compile: does the VALUE adapter path share the key defect? It does NOT — it calls neither
    /// <c>AdapterStructKey</c> nor <c>collidingAdapterNames</c>, both of which are reached only from
    /// the <c>Pointer = true</c> arms — so this commit cannot move it, and the arm pins that.
    /// </summary>
    /// <remarks>
    /// ⚠ What it DOES carry is the seat's own IDENTIFIER defect, one path over: the value name composes
    /// from <c>GetFullTypeName</c>, which spells a generic <c>Name&lt;typeArgs&gt;</c>, so the argument
    /// list lands inside the class identifier exactly as it did on the pointer path before the seat.
    /// The identifier asserted below is that defect, pinned deliberately rather than fixed: the
    /// CONVERTER composes the matching name at the cast site through <c>valueAdapterTypeRef</c>, and a
    /// generator-only change would leave the two halves naming different classes. It is unreachable
    /// for every committed record — the only generic struct sides on the value side of the corpus are
    /// LOCAL and DECLARED, which route to the partial-stub path instead of here, and there is no
    /// foreign-and-generic value record at all.
    /// </remarks>
    [TestMethod]
    public void AForeignGenericValueRecordIsUnmovedByTheKeyFix()
    {
        (Dictionary<string, string> adapters, _) = RunImplementGenerator(ValuePathSource);

        string adapter = Adapter(adapters, "G_Object, Object_", "mapLike");

        Assert.AreEqual("pkg_G<Object, Object>ᴠmapLike", DeclaredAdapterName(adapter),
            "the value path's identifier is the seat's residue — it must not move under a key fix, and it is not fixable from this half alone");
    }

    #endregion
}
