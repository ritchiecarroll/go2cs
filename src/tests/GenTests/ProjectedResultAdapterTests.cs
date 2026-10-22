// ProjectedResultAdapterTests.cs - Gbtc
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
/// A pointer adapter whose interface declares a member returning ANOTHER INTERFACE, where the Go
/// method returns the pointer, must WRAP the forwarded result in that interface's own adapter.
/// </summary>
/// <remarks>
/// <para>
/// The corpus reaches this through <c>crypto/mlkem</c>'s <c>testRoundTrip</c> and the func-result
/// PROJECTION that serves it (mailbox <c>44812e89</c>, taken at <c>9b9f779de</c>). A projected
/// constraint closes over a SIBLING's projection — <c>decapsulationKey[encapsulationKey]</c> — so the
/// adapter generated for it must satisfy <c>EncapsulationKey() E</c> with <c>E</c> bound to the
/// INTERFACE, while the Go method on the concrete type returns <c>*EncapsulationKey768</c>, which
/// converts to <c>ж&lt;EncapsulationKey768&gt;</c>. Go has no return covariance, so the two are not
/// the same type and never will be: the adapter is where the projection is made good.
/// </para>
/// <para>
/// Measured at the emitted text before any fix — the member is declared with the interface's own
/// return type and forwards the Go result RAW:
/// </para>
/// <code>
///     global::go.probe_package.named global::go.probe_package.keyedNamed.encapKey() =&gt; m_box.encapKey();
/// </code>
/// <para>
/// which is CS0266 inside the generated file (an explicit conversion exists, so the compiler names it as a missing cast). The adapter to wrap it in EXISTS by the time this one
/// is generated: the projection records the element's own pair — for mlkem,
/// <c>GoImplement&lt;ж&lt;EncapsulationKey768&gt;, encapsulationKey&gt;</c> is one of the two records
/// that package already had — so the wrap names a class this same run emits.
/// </para>
/// <para>
/// ⚠ ONE AXIS, DELIBERATELY. The mlkem shape ALSO needs a GENERIC interface
/// (<c>decapsulationKey&lt;…&gt;</c>), and the adapter NAME for a generic-interface record is a
/// SEPARATE defect routed elsewhere (COORD <c>a3491cc95</c>: <c>AdapterName</c> composes
/// <c>GetSimpleName(interfaceName)</c> with no generic drop, so it would spell
/// <c>digestжkeyedNamed&lt;named&gt;</c> — the interface-side twin of C1's collision-key finding,
/// folded into the i7's one follow-up). These arms use a NON-GENERIC interface so that a red here is
/// the RESULT TYPE and nothing else; the generic-composition arm is owed once that follow-up lands.
/// </para>
/// <para>
/// The two-compilation shape is <c>ForeignGenericAdapterTests</c>'s and is load-bearing rather than
/// copied: with the <c>ж&lt;T&gt;</c> scaffolding declared in the SAME compilation, the generator
/// derived the package class from it and emitted into <c>ж_package</c>, where the bare struct name
/// does not resolve (CS0246 ×18). Putting the runtime shims in a referenced assembly is also what the
/// corpus does — golib is a reference, never a source file.
/// </para>
/// </remarks>
[TestClass]
public class ProjectedResultAdapterTests
{
    /// <summary>The runtime shims, in a REFERENCED assembly exactly as golib is.</summary>
    private const string RuntimeSource =
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
        """;

    private const string ProbeSource =
        """
        using go;

        [assembly: GoImplement<global::go.probe_package.digest, global::go.probe_package.named>(Pointer = true)]
        [assembly: GoImplement<global::go.probe_package.digest, global::go.probe_package.keyedNamed>(Pointer = true)]
        [assembly: GoImplement<global::go.probe_package.digest, global::go.probe_package.plainOnly>(Pointer = true)]
        [assembly: GoImplement<global::go.probe_package.digest, global::go.probe_package.keyedGeneric<global::go.probe_package.named>>(Pointer = true)]

        namespace go;

        public static partial class probe_package
        {
            public partial struct digest
            {
            }

            // The Go pointer-receiver methods, in the box-extension form the converter emits.
            // ⚠ `encapKey` returns the BOX: in Go it returns *digest, and the interface member it
            // must satisfy returns the INTERFACE. That gap is the whole subject.
            internal static object label(this ж<digest> Ꮡd) => default!;

            internal static ж<digest> encapKey(this ж<digest> Ꮡd) => default!;

            internal static object plain(this ж<digest> Ꮡd) => default!;

            internal partial interface named
            {
                object label();
            }

            // THE SUBJECT: a member whose declared result is another interface this same compilation
            // records an adapter for.
            internal partial interface keyedNamed
            {
                object label();
                named encapKey();
            }

            // THE CONTROL: the same struct, the same generator run, a member whose result needs NO
            // wrap. A fix that wrapped indiscriminately would pass the subject arms and fail here, so
            // this arm discriminates rather than merely passing.
            internal partial interface plainOnly
            {
                object plain();
            }

            // ⚠ crypto/mlkem's ACTUAL shape, and the reason the seat needed a follow-up before this
            // arm could exist: the interface is GENERIC and its record names a CLOSED instantiation,
            // exactly as a projected `decapsulationKey[encapsulationKey]` does. Its member returns the
            // type PARAMETER, which at this instantiation is the projected interface — so the wrap and
            // the naming are exercised together, which is how the row meets them.
            internal partial interface keyedGeneric<E>
            {
                object label();
                E encapKey();
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
    /// Runs the REAL generator over the probe compilation and hands back the generated adapter
    /// sources keyed by hint name together with the compilation they were folded into, so an arm can
    /// read either the emitted TEXT or the compiler's own verdict on it.
    /// </summary>
    private static (Dictionary<string, string> Adapters, Compilation Updated) RunImplementGenerator()
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);

        CSharpCompilation runtime = CSharpCompilation.Create(
            "projected-result-runtime",
            [CSharpSyntaxTree.ParseText(RuntimeSource, parseOptions)],
            CoreReferences(),
            libraryOptions);

        using MemoryStream image = new();
        EmitResult emitted = runtime.Emit(image);

        Assert.IsTrue(emitted.Success,
            $"the runtime shim compilation must emit clean: {string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))}");

        CSharpCompilation probe = CSharpCompilation.Create(
            "projected-result-test",
            [CSharpSyntaxTree.ParseText(ProbeSource, parseOptions)],
            CoreReferences().Append(MetadataReference.CreateFromImage(image.ToArray())),
            libraryOptions);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ImplementGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(probe, out Compilation updated, out ImmutableArray<Diagnostic> _);

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

    /// <summary>
    /// The one emitted line implementing <paramref name="member"/>.
    /// </summary>
    /// <remarks>
    /// ⚠ Scoped to the MEMBER LINE and not to the file, because the file also carries the module
    /// initializer's <c>static box =&gt; new digestжplainOnly((ж&lt;digest&gt;)box)</c>. A
    /// file-wide "is it wrapped" test matches that registration on EVERY adapter and reads green
    /// before anything is fixed — this arm's first cut did exactly that and its CONTROL failed,
    /// which is the only reason it was caught.
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
    /// The whole point: with a member whose declared result is another recorded interface, the
    /// compilation COMPILES. Before the fix this arm is CS0266 inside the generated adapter file — a
    /// <c>ж&lt;digest&gt;</c> where a <c>named</c> is required.
    /// </summary>
    [TestMethod]
    public void AProjectedResultPairProducesAnAdapterThatCompiles()
    {
        (Dictionary<string, string> adapters, Compilation updated) = RunImplementGenerator();

        List<Diagnostic> errors = Errors(updated).ToList();

        Assert.AreEqual(0, errors.Count,
            $"the generated adapters must compile; saw: {string.Join("; ", errors.Select(diagnostic => $"{diagnostic.Id} {diagnostic.GetMessage()}"))}" +
            $"{System.Environment.NewLine}generated: {string.Join(" | ", adapters.Keys)}");

        // Beside the adapters, the package's one deferred adapter registration (AdapterRegistry.g.cs).
        Assert.AreEqual(4, adapters.Keys.Count(name => !name.EndsWith(".AdapterRegistry.g.cs")), $"one adapter per record was expected, saw: {string.Join(" | ", adapters.Keys)}");
    }

    /// <summary>
    /// THE GENERIC-COMPOSITION ARM — <c>crypto/mlkem</c>'s actual shape, and the one this file owed
    /// from the day it was written: the record names a CLOSED instantiation of a GENERIC interface
    /// whose member returns the type parameter, so the adapter's NAME and the projected-result WRAP
    /// are exercised together.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It could not exist until the collision-key follow-up landed. Before it, the adapter's class
    /// identifier carried the argument list — <c>digestжkeyedGeneric&lt;…&gt;</c>, or the argument's
    /// own tail segment once the last-dot scan ran inside the list — which no class can be named, and
    /// which the converter's cast site would never reference. Both halves now strip the list BEFORE
    /// taking the last segment, so the two compose one identifier: the generator through
    /// <c>StripGenericTypeArguments</c> at its <c>AdapterName</c> site, the converter through
    /// <c>adapterTypeRef</c> before the deferred marker.
    /// </para>
    /// <para>
    /// ⚠ The collision KEYS on the interface side are deliberately NOT stripped on either half, so a
    /// generic interface reference garbles identically on both — parity rather than correctness. This
    /// arm does not reach that: both the interface and its closed argument are local and DOTLESS
    /// here, as they are in <c>crypto/mlkem</c>, so the last-dot reduction is a no-op and there is
    /// nothing to garble. Stated because the ruling says to expect the garble, and this row is not
    /// the one that would show it.
    /// </para>
    /// </remarks>
    [TestMethod]
    public void AGenericInterfaceRecordComposesOneIdentifierAndStillWraps()
    {
        (Dictionary<string, string> adapters, Compilation updated) = RunImplementGenerator();

        string adapter = Adapter(adapters, "keyedGeneric");

        Assert.IsTrue(adapter.Contains("class digestжkeyedGeneric :"),
            $"the adapter's class identifier must carry NO type-argument list — a generic interface record names one class, not a generic one: {adapter}");

        Assert.IsFalse(adapter.Contains("class digestжkeyedGeneric<"),
            $"the argument list must not land inside the identifier: {adapter}");

        // The wrap, on the shape that actually reaches it: the member returns the type PARAMETER,
        // bound at this instantiation to the projected interface, where the Go method returns the box.
        string line = MemberLine(adapter, "encapKey");

        StringAssert.Contains(line, "new digestжnamed(",
            $"the generic interface's member must wrap its forwarded result exactly as the non-generic one does: {line}");

        List<Diagnostic> errors = Errors(updated).ToList();

        Assert.AreEqual(0, errors.Count,
            $"the generic-interface adapter must compile; saw: {string.Join("; ", errors.Select(diagnostic => $"{diagnostic.Id} {diagnostic.GetMessage()}"))}");
    }

    /// <summary>
    /// The mechanism, read from the emitted TEXT rather than inferred from the compiler's silence:
    /// the member forwards THROUGH the result interface's own adapter instead of handing back the box.
    /// </summary>
    [TestMethod]
    public void AProjectedResultMemberWrapsTheForwardedResult()
    {
        (Dictionary<string, string> adapters, Compilation _) = RunImplementGenerator();

        string line = MemberLine(Adapter(adapters, "keyedNamed"), "encapKey");

        StringAssert.Contains(line, "new digestжnamed(",
            $"encapKey's forwarded result must be wrapped in the RESULT interface's own adapter — Go has no return covariance, so handing the box back is CS0266. Emitted: {line}");

        StringAssert.Contains(line, "m_box.encapKey()",
            $"the wrap must still forward to the Go method. Emitted: {line}");
    }

    /// <summary>
    /// THE CONTROL, and it is what makes the arm above a measurement: a member whose result needs no
    /// wrap keeps its bare forward. A fix that wrapped every member would pass both arms above and
    /// fail here.
    /// </summary>
    [TestMethod]
    public void APlainResultMemberKeepsItsBareForward()
    {
        (Dictionary<string, string> adapters, Compilation _) = RunImplementGenerator();

        string line = MemberLine(Adapter(adapters, "plainOnly"), "plain");

        StringAssert.EndsWith(line, "=> m_box.plain();",
            $"a member whose result needs no projection must forward bare. Emitted: {line}");
    }

    /// <summary>
    /// The SECOND control, on the other axis: the member of the RESULT interface itself. Its own
    /// result is an <c>object</c>, so the fix must not reach it either — and it shares an adapter
    /// with nothing, so a wrap here would be a pure over-reach.
    /// </summary>
    [TestMethod]
    public void TheResultInterfacesOwnMemberIsUnwrapped()
    {
        (Dictionary<string, string> adapters, Compilation _) = RunImplementGenerator();

        string line = MemberLine(Adapter(adapters, "-global__go.probe_package.named-"), "label");

        StringAssert.EndsWith(line, "=> m_box.label();",
            $"the result interface's own member must keep its bare forward. Emitted: {line}");
    }
}
