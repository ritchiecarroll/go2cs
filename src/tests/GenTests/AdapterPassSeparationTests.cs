// AdapterPassSeparationTests.cs - Gbtc
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
/// THE PASS SEPARATION. A pointer adapter's collision rule must read the record's <c>Production</c>
/// facet, so a test-half record joining a production record's group cannot rename the production
/// member.
/// </summary>
/// <remarks>
/// <para>
/// Under the recompile test model the production <c>.cs</c> files are COMPILE ITEMS of the test
/// assembly, so this generator reads the UNION of both halves' records — but the production text was
/// rendered in the production pass, against the production set alone, and cannot be re-rendered.
/// <c>crypto/sha3</c> is the corpus instance and the fixture below is its exact shape: production
/// records <c>&lt;SHA3, hash.Hash&gt;</c>, the external test half adds
/// <c>&lt;SHA3, fips140.Hash&gt;</c>, both compose <c>SHA3жHash</c>, and the ordinary rule prefixes
/// BOTH — so <c>sha3.cs</c>'s four already-written <c>SHA3жHash</c> sites name a class that is never
/// emitted (CS0246/CS0426 ×4).
/// </para>
/// <para>
/// ⚠ <see cref="WithoutTheFacetBothMembersAreRenamed"/> is the CONTROL, and it is what makes the rest
/// of this class a measurement rather than a restatement: it runs the SAME fixture with the facet
/// deleted and one axis moved, and reads both members renamed. Without it, an arm asserting
/// <c>SHA3жHash</c> could be satisfied by a fixture that never collided at all.
/// </para>
/// <para>
/// Keep in sync with the converter's <c>adapterNameCollisions.go</c>, which resolves the matching
/// cast-site references by the same rule over the same records. The four rules are stated there and
/// on <c>GoImplementAttribute.Production</c>; arms 1–4 below are one per rule.
/// </para>
/// </remarks>
[TestClass]
public class AdapterPassSeparationTests
{
    // The referenced assembly: the minimal golib surface a generated pointer adapter binds, plus the
    // TWO FOREIGN interface packages whose interfaces share the simple name `Hash`. Both are foreign,
    // which is the shape that makes the ordinary rule prefix both members and lose the production
    // name; a LOCAL member would keep the bare form under the ordinary rule and never reach this.
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

            public interface IGoAdapter;

            public interface IжAdapter : IGoAdapter { object? Box { get; } }

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
                public bool Production { get; set; }
                public bool ConstraintProxy { get; set; }
            }

            public static partial class hash_package
            {
                public interface Hash
                {
                    int Size();
                }
            }

            public static partial class fips140_package
            {
                public interface Hash
                {
                    int Size();
                }
            }
        }
        """;

    // crypto/sha3's shape. `sha3_package` is the FIRST class of the attribute-bearing unit, so the
    // generator's packageClassName is `sha3_package` and SHA3 keys BARE — which is what puts both
    // records in one group over the name `SHA3жHash`.
    //
    // The PRODUCTION record carries the facet exactly as the recompile seed stamps it; the test-half
    // record does not. The two placeholders are the facet text, so every arm below is the same
    // fixture with one axis moved and nothing else.
    //
    // ⚠ SUBSTITUTED BY Replace, NEVER BY string.Format, and the distinction cost three arms. This is
    // C# SOURCE: it is full of braces, and string.Format reads every one of them as a format
    // specifier. Three of the four arms threw FormatException at offset 286 — the `{` opening
    // `partial class sha3_package` — before asserting anything at all, so they reported nothing
    // while looking like tests (the fourth, AnUnfacetedRecordIsUnmovedByTheFacetRule, uses
    // NoCollisionSource and needs no substitution, which is why one arm passed and hid it).
    // Escaping every brace as {{ }} would work and would also make the fixture unreadable as the C#
    // it is; a placeholder no C# token can contain cannot have the problem at all.
    private const string ProductionFacetPlaceholder = "«PRODUCTION-FACET»";
    private const string TestFacetPlaceholder = "«TEST-FACET»";

    private const string SeparationSourceTemplate =
        """
        using go;

        [assembly: GoImplement<global::go.sha3_package.SHA3, global::go.hash_package.Hash>(Pointer = true«PRODUCTION-FACET»)]
        [assembly: GoImplement<global::go.sha3_package.SHA3, global::go.fips140_package.Hash>(Pointer = true«TEST-FACET»)]

        namespace go;

        public static partial class sha3_package
        {
            public partial struct SHA3
            {
            }

            public static int Size(this ж<SHA3> Ꮡd) => 0;
        }
        """;

    // The SCOPE fixture (rule 4): one record, no collision, no facet — the shape 337 of the corpus's
    // 342 .csproj-carrying packages are in, and the one that must not move at all.
    private const string NoCollisionSource =
        """
        using go;

        [assembly: GoImplement<global::go.sha3_package.SHA3, global::go.hash_package.Hash>(Pointer = true)]

        namespace go;

        public static partial class sha3_package
        {
            public partial struct SHA3
            {
            }

            public static int Size(this ж<SHA3> Ꮡd) => 0;
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
            "pass-separation-production",
            [CSharpSyntaxTree.ParseText(ProductionSource, parseOptions)],
            CoreReferences(),
            libraryOptions);

        using MemoryStream image = new();
        EmitResult emitted = production.Emit(image);
        Assert.IsTrue(emitted.Success, $"production compilation must emit clean: {string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))}");

        CSharpCompilation consuming = CSharpCompilation.Create(
            "pass-separation-test",
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
    /// Picks the one generated pointer adapter whose hint name carries every supplied fragment. The
    /// hint name is composed from the FULLY QUALIFIED interface name, so the two members of this
    /// fixture's group are told apart by their package segments and never by the shared simple name.
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
        string marker = " sealed class ";
        int start = adapter.IndexOf(marker, System.StringComparison.Ordinal);

        Assert.IsTrue(start >= 0, $"no class declaration found in the generated adapter:\r\n{adapter}");

        start += marker.Length;

        int end = adapter.IndexOfAny([':', '\r', '\n'], start);

        return (end < 0 ? adapter[start..] : adapter[start..end]).Trim();
    }

    private static (string Production, string Test) SeparatedNames(string productionFacet, string testFacet, out Compilation updated)
    {
        string source = SeparationSourceTemplate
            .Replace(ProductionFacetPlaceholder, productionFacet)
            .Replace(TestFacetPlaceholder, testFacet);

        (Dictionary<string, string> adapters, Compilation compilation) = RunImplementGenerator(source);

        updated = compilation;

        return (DeclaredAdapterName(Adapter(adapters, "hash_package", "ptr")),
                DeclaredAdapterName(Adapter(adapters, "fips140_package", "ptr")));
    }

    /// <summary>
    /// ARM 1, RULE 2 — the rule itself, at crypto/sha3's exact shape. The group collides over the
    /// union and exactly ONE member carries the facet: that member keeps <c>SHA3жHash</c>, which is
    /// what sha3.cs's four production sites already spell, and only the unfaceted member takes the
    /// interface prefix.
    /// </summary>
    [TestMethod]
    public void TheSoleProductionFacetedMemberKeepsItsCompiledName()
    {
        (string production, string test) = SeparatedNames(", Production = true", string.Empty, out Compilation updated);

        Assert.AreEqual("SHA3жHash", production,
            "the production half's adapter was renamed — sha3.cs cannot be re-rendered, so its four sites now name nothing");

        Assert.AreEqual("SHA3жfips140_Hash", test);

        Assert.AreNotEqual(production, test, "the collision must still be resolved — the facet decides naming, never grouping");

        List<Diagnostic> errors = Errors(updated).ToList();

        Assert.AreEqual(0, errors.Count,
            $"the generated adapters must compile: {string.Join("; ", errors.Select(error => $"{error.Id} {error.GetMessage()}"))}");
    }

    /// <summary>
    /// THE CONTROL, and the reason arm 1 measures anything: the SAME fixture with the facet deleted
    /// and nothing else moved. Both members are then renamed by the ordinary rule — which is the
    /// defect, reproduced on demand.
    /// </summary>
    [TestMethod]
    public void WithoutTheFacetBothMembersAreRenamed()
    {
        (string production, string test) = SeparatedNames(string.Empty, string.Empty, out _);

        Assert.AreEqual("SHA3жhash_Hash", production,
            "without the facet the ordinary rule must prefix BOTH members — if it does not, this fixture never collided and arm 1 proves nothing");

        Assert.AreEqual("SHA3жfips140_Hash", test);
    }

    /// <summary>
    /// ARM 3, RULE 3 — the negative that keeps the rule from over-firing. TWO faceted members is a
    /// collision the PRODUCTION pass already saw and already resolved, so the production text spells
    /// the PREFIXED names and the ordinary rule is what reproduces them. Exempting one of them would
    /// rename a production site in the opposite direction: the same defect, mirrored.
    /// </summary>
    [TestMethod]
    public void TwoFacetedMembersFallBackToTheOrdinaryRule()
    {
        (string production, string test) = SeparatedNames(", Production = true", ", Production = true", out _);

        Assert.AreEqual("SHA3жhash_Hash", production);
        Assert.AreEqual("SHA3жfips140_Hash", test);
    }

    /// <summary>
    /// ARM 4, RULE 4 AND THE SCOPE. A group with no faceted member, and here no collision either, is
    /// every production compilation and both reference test models — the answers must be exactly what
    /// they were before the facet existed. This is what makes the seat byte-neutral outside a
    /// recompile-model test project.
    /// </summary>
    [TestMethod]
    public void AnUnfacetedRecordIsUnmovedByTheFacetRule()
    {
        (Dictionary<string, string> adapters, Compilation updated) = RunImplementGenerator(NoCollisionSource);

        Assert.AreEqual("SHA3жHash", DeclaredAdapterName(Adapter(adapters, "hash_package", "ptr")));

        List<Diagnostic> errors = Errors(updated).ToList();

        Assert.AreEqual(0, errors.Count,
            $"the generated adapter must compile: {string.Join("; ", errors.Select(error => $"{error.Id} {error.GetMessage()}"))}");
    }

    private static IEnumerable<Diagnostic> Errors(Compilation compilation) =>
        compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
}
