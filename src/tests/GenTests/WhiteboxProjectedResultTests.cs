// WhiteboxProjectedResultTests.cs - Gbtc
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
/// The WHITE-BOX shape of the projected-result wrap: the struct lives in the PRODUCTION assembly and
/// the interface in the internal-test package, which is the one arrangement
/// <c>ProjectedResultAdapterTests</c> cannot reach.
/// </summary>
/// <remarks>
/// <para>
/// Measured at the row rather than imagined (mailbox <c>d6d2970a2</c>, ruled at <c>5347b4aae</c>):
/// <c>crypto/mlkem</c> converts and its test host FAILS AT BUILD with exactly two errors, one per key
/// size —
/// </para>
/// <code>
///     CS0266  Cannot implicitly convert 'go.ж&lt;go.crypto.mlkem_package.EncapsulationKey768&gt;'
///             to 'go.crypto.mlkem_internal_test_package.encapsulationKey'
/// </code>
/// <para>
/// — because the wrap map skipped the pair. Its bound was <c>ImplementGenerator.cs</c>'s
/// <c>pairStruct.ContainingAssembly == context.Compilation.Assembly</c>, and in the white-box model
/// the struct is ALWAYS foreign (production) while the interface is ALWAYS local (the test package),
/// so the one arrangement the corpus actually needs was the one arrangement excluded. The adapter the
/// wrap wanted to name was being minted in that same compilation all along
/// (<c>mlkem_EncapsulationKey768жencapsulationKey</c>).
/// </para>
/// <para>
/// ⚠ THE BOUND WAS LOAD-BEARING FOR THE NAME, WHICH IS WHY LIFTING IT IS NOT A ONE-LINE DELETE. While
/// every pair was local, the map could compose its value from the bare simple name and be right BY
/// CONSTRUCTION. A FOREIGN struct's adapter carries a package prefix (<c>mlkem_</c>), so lifting the
/// bound means the value must be composed through the SAME helper the collision key and the main loop
/// use — <c>AdapterStructKey</c> — rather than through a second spelling of it. That is C1's
/// collision-key finding (mailbox <c>f89515008</c> §4) arriving from the other side: the two sides of
/// one comparison must be composed by one helper, and here the two sides are the map's VALUE and the
/// class the main loop actually emits.
/// </para>
/// <para>
/// The fixture is <c>ForeignGenericAdapterTests</c>'s two-assembly shape, for its reason rather than
/// by copying: with the <c>ж&lt;T&gt;</c> scaffolding in the compilation under test the generator
/// derives the package class from it and emits into <c>ж_package</c>. golib is a reference in the
/// corpus, never a source file, and the production package is a reference to a test assembly.
/// </para>
/// </remarks>
[TestClass]
public class WhiteboxProjectedResultTests
{
    /// <summary>
    /// The PRODUCTION assembly: the runtime shims plus the two subjects, in their own package class.
    /// </summary>
    /// <remarks>
    /// The members take the box-extension form a Go pointer-receiver method converts to, which is why
    /// a foreign struct's forward resolves through <c>GetExtensionMethods</c> and not through a
    /// declaration the generator can see.
    /// </remarks>
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

        namespace go
        {
            public static partial class prod_package
            {
                // crypto/mlkem's EncapsulationKey768 — the RESULT type of the projection.
                public partial struct EncapKey
                {
                }

                public static object Bytes(this ж<EncapKey> Ꮡe) => default!;

                // crypto/mlkem's DecapsulationKey768 — the struct under adaptation. Its Go method
                // returns *EncapsulationKey768, which converts to ж<EncapKey>; the interface it must
                // satisfy declares that member's result as the INTERFACE. Go has no return
                // covariance, so the adapter is where the two are reconciled.
                public partial struct DecapKey
                {
                }

                public static ж<EncapKey> EncapsulationKey(this ж<DecapKey> Ꮡd) => default!;

                public static object Bytes(this ж<DecapKey> Ꮡd) => default!;
            }
        }
        """;

    /// <summary>
    /// The consuming ("internal test") assembly: the interfaces are LOCAL, the structs are FOREIGN,
    /// and both records are present — the white-box arrangement exactly.
    /// </summary>
    /// <remarks>
    /// The LOCAL pair beside it is the CONTROL for the composition change: it must keep wrapping, and
    /// keep wrapping under its UNPREFIXED name, so a fix that lifted the bound by prefixing
    /// everything would red here rather than pass quietly.
    /// </remarks>
    private const string ConsumingSource =
        """
        using go;

        [assembly: GoImplement<global::go.prod_package.EncapKey, global::go.probe_package.encapKeyLike>(Pointer = true)]
        [assembly: GoImplement<global::go.prod_package.DecapKey, global::go.probe_package.decapKeyLike<global::go.probe_package.encapKeyLike>>(Pointer = true)]
        [assembly: GoImplement<global::go.probe_package.LocalEncap, global::go.probe_package.localEncapLike>(Pointer = true)]
        [assembly: GoImplement<global::go.probe_package.LocalDecap, global::go.probe_package.localDecapLike<global::go.probe_package.localEncapLike>>(Pointer = true)]

        namespace go;

        public static partial class probe_package
        {
            // The LOCAL control pair — the arrangement that already worked before the bound was
            // lifted, kept in the SAME compilation so the two differ on assembly and nothing else.
            public partial struct LocalEncap
            {
            }

            internal static object Bytes(this ж<LocalEncap> Ꮡe) => default!;

            public partial struct LocalDecap
            {
            }

            internal static ж<LocalEncap> EncapsulationKey(this ж<LocalDecap> Ꮡd) => default!;

            internal static object Bytes(this ж<LocalDecap> Ꮡd) => default!;

            internal partial interface encapKeyLike
            {
                object Bytes();
            }

            // The PROJECTED constraint: the member's result is the type parameter, bound at the
            // record to the sibling's own interface.
            internal partial interface decapKeyLike<E>
            {
                object Bytes();
                E EncapsulationKey();
            }

            internal partial interface localEncapLike
            {
                object Bytes();
            }

            internal partial interface localDecapLike<E>
            {
                object Bytes();
                E EncapsulationKey();
            }
        }
        """;

    /// <summary>
    /// The LOUD DIRECTION: the same fixture with the result pair's record REMOVED — one attribute
    /// line, nothing else.
    /// </summary>
    /// <remarks>
    /// A wrap must never be invented for a pair this compilation does not record. The generator emits
    /// the bare forward and the COMPILER says so, which is the direction that costs a build rather
    /// than a wrong answer — so this arm asserts both halves: the member is bare AND the compilation
    /// carries the CS0266 the row reported.
    /// </remarks>
    private const string UnrecordedConsumingSource =
        """
        using go;

        [assembly: GoImplement<global::go.prod_package.DecapKey, global::go.probe_package.decapKeyLike<global::go.probe_package.encapKeyLike>>(Pointer = true)]

        namespace go;

        public static partial class probe_package
        {
            internal partial interface encapKeyLike
            {
                object Bytes();
            }

            internal partial interface decapKeyLike<E>
            {
                object Bytes();
                E EncapsulationKey();
            }
        }
        """;

    private static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"));
    }

    private static (Dictionary<string, string> Adapters, Compilation Updated) RunImplementGenerator(string consumingSource, string assemblyName)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);

        CSharpCompilation production = CSharpCompilation.Create(
            "whitebox-projected-production",
            [CSharpSyntaxTree.ParseText(ProductionSource, parseOptions)],
            CoreReferences(),
            libraryOptions);

        using MemoryStream image = new();
        EmitResult emitted = production.Emit(image);
        Assert.IsTrue(emitted.Success, $"production compilation must emit clean: {string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))}");

        CSharpCompilation consuming = CSharpCompilation.Create(
            assemblyName,
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

    /// <summary>The one emitted line implementing <paramref name="member"/>.</summary>
    /// <remarks>
    /// ⚠ Scoped to the MEMBER LINE, never the file: the file also carries the module initializer's
    /// <c>static box =&gt; new …ж…((ж&lt;T&gt;)box)</c> registration, which contains the word
    /// <c>new</c> and makes a file-wide "is it wrapped" test read GREEN before anything is fixed.
    /// <c>ProjectedResultAdapterTests</c> paid for that one; it is not paid for twice here.
    /// </remarks>
    private static string MemberLine(string adapter, string member)
    {
        List<string> lines = adapter
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Contains($".{member}() =>"))
            .ToList();

        Assert.AreEqual(1, lines.Count,
            $"exactly one emitted line must implement {member}; saw {lines.Count} in: {adapter}");

        return lines[0];
    }

    private static IEnumerable<Diagnostic> Errors(Compilation compilation) =>
        compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

    /// <summary>
    /// THE SUBJECT: the white-box arrangement compiles. Red before the fix with the row's own CS0266.
    /// </summary>
    [TestMethod]
    public void AForeignStructWithALocalProjectedInterfaceCompiles()
    {
        (Dictionary<string, string> _, Compilation updated) = RunImplementGenerator(ConsumingSource, "whitebox-projected-test");

        List<Diagnostic> errors = Errors(updated).ToList();

        Assert.AreEqual(0, errors.Count,
            $"the white-box projected-result shape must compile; saw: {string.Join(" | ", errors.Select(diagnostic => diagnostic.ToString()))}");
    }

    /// <summary>
    /// The member WRAPS, and it wraps in the adapter named for the FOREIGN struct — the composition
    /// half of the fix, which a compile-only arm cannot see.
    /// </summary>
    [TestMethod]
    public void TheForeignProjectedMemberWrapsThroughThePrefixedAdapterName()
    {
        (Dictionary<string, string> adapters, Compilation _) = RunImplementGenerator(ConsumingSource, "whitebox-projected-test");

        string line = MemberLine(Adapter(adapters, "DecapKey", "decapKeyLike"), "EncapsulationKey");

        StringAssert.Contains(line, "m_box.EncapsulationKey()", "the forward itself must survive");
        StringAssert.Contains(line, "prod_EncapKey", "the wrap must name the FOREIGN struct's prefixed adapter, as the main loop mints it");
        Assert.IsFalse(line.TrimEnd().EndsWith("=> m_box.EncapsulationKey();"),
            $"the member must not forward bare: {line}");
    }

    /// <summary>
    /// THE CONTROL for the composition change: a LOCAL pair still wraps, and still under its
    /// UNPREFIXED name. A fix that lifted the bound by prefixing every struct would red here.
    /// </summary>
    [TestMethod]
    public void ALocalProjectedMemberStillWrapsUnderItsUnprefixedName()
    {
        (Dictionary<string, string> adapters, Compilation _) = RunImplementGenerator(ConsumingSource, "whitebox-projected-test");

        string line = MemberLine(Adapter(adapters, "LocalDecap", "localDecapLike"), "EncapsulationKey");

        StringAssert.Contains(line, "m_box.EncapsulationKey()", "the forward itself must survive");
        StringAssert.Contains(line, "LocalEncapжlocalEncapLike", "a LOCAL pair keeps its unprefixed adapter name");
        Assert.IsFalse(line.Contains("probe_LocalEncap"),
            $"a local struct must not acquire a package prefix: {line}");
    }

    /// <summary>
    /// THE LOUD DIRECTION: with the result pair's record removed, no wrap is invented — the forward
    /// stays bare and the COMPILER names it, which is a build failure rather than a wrong answer.
    /// </summary>
    [TestMethod]
    public void AnUnrecordedResultPairKeepsItsBareForwardAndFailsLoudly()
    {
        (Dictionary<string, string> adapters, Compilation updated) = RunImplementGenerator(UnrecordedConsumingSource, "whitebox-unrecorded-test");

        string line = MemberLine(Adapter(adapters, "DecapKey", "decapKeyLike"), "EncapsulationKey");

        Assert.IsTrue(line.TrimEnd().EndsWith("=> m_box.EncapsulationKey();"),
            $"an unrecorded result pair must keep its BARE forward: {line}");

        List<Diagnostic> errors = Errors(updated).ToList();

        Assert.IsTrue(errors.Any(diagnostic => diagnostic.Id == "CS0266"),
            $"and the compiler must say so — the loud direction; saw: {string.Join(" | ", errors.Select(diagnostic => diagnostic.ToString()))}");
    }
}
