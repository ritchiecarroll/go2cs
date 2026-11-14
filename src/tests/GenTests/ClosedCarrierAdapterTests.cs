// ClosedCarrierAdapterTests.cs - Gbtc
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
/// ONE instantiation of a LOCAL generic type satisfying a NON-generic interface because a method the
/// interface requires names the type's own type parameter: Go's <c>var v valuer = box[point]{}</c>, where
/// <c>valuer</c> is <c>interface{ val() point }</c> and <c>box[T]</c> declares <c>val() T</c>.
/// </summary>
/// <remarks>
/// <para>
/// A partial declaration can extend only the OPEN definition, so the open route emits
/// <c>partial struct box&lt;T&gt; : valuer</c> whose forwarding member returns a <c>T</c> where the interface
/// wants a <c>point</c> (CS0029), and the pointer form's generic <c>cellжgetter&lt;T&gt;</c> fails the same
/// way. No other instantiation satisfies the interface, so nothing open over <c>T</c> can implement it.
/// </para>
/// <para>
/// The converter marks such a pair <c>Closed = true</c> (its closedCarrier.go), and the generator then
/// emits an adapter class for the one instantiation: <c>boxᴠvaluer</c> holding a copy for a value,
/// <c>cellжgetter</c> holding the box for a pointer. Both identifiers come from the type's BARE name, the
/// spelling the converter composes at the conversion site, and the type arguments live only in the
/// wrapped type.
/// </para>
/// <para>
/// The route is selected by the MARKER and by nothing else. <see cref="WithoutTheMarkerTheOpenRouteIsKept"/>
/// runs the same three records unmarked and reads the open forms back, with the defect's own error: it is
/// the arm that shows the compile check in <see cref="AClosedCarrierPairCompiles"/> can fail.
/// </para>
/// </remarks>
[TestClass]
public class ClosedCarrierAdapterTests
{
    // The referenced assembly: the minimal golib surface the generated adapters bind.
    private const string ShimSource =
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

            public interface IValueAdapter { object? Value { get; } }

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
                public bool Closed { get; set; }
            }
        }
        """;

    private const string ValueMarker = "(Closed = true)";
    private const string PointerMarker = "(Pointer = true, Closed = true)";

    private const string Source =
        """
        using go;

        [assembly: GoImplement<global::go.probe_package.box<global::go.probe_package.point>, global::go.probe_package.valuer>(Closed = true)]
        [assembly: GoImplement<global::go.probe_package.cell<global::go.probe_package.point>, global::go.probe_package.getter>(Pointer = true, Closed = true)]
        [assembly: GoImplement<global::go.probe_package.bag<string>, global::go.probe_package.sizer>]

        namespace go;

        public static partial class probe_package
        {
            public partial struct point { public int x; }

            // THE VALUE SUBJECT: val names T, and valuer wants a point.
            public partial struct box<T> { public T v; public static bool operator ==(box<T> a, box<T> b) => Equals(a.v, b.v); public static bool operator !=(box<T> a, box<T> b) => !(a == b); public override bool Equals(object? o) => o is box<T> other && this == other; public override int GetHashCode() => 0; }

            public static T val<T>(this box<T> b) => b.v;

            internal partial interface valuer
            {
                point val();
            }

            // THE POINTER SUBJECT: a pointer-receiver method is a package extension over the box.
            public partial struct cell<T> { public T v; }

            public static T get<T>(this ж<cell<T>> Ꮡc) => Ꮡc.Value.v;

            internal partial interface getter
            {
                point get();
            }

            // THE CONTROL: size names no type parameter, so every bag satisfies sizer and the record
            // carries no marker. It keeps the open partial, before and after.
            public partial struct bag<T> { public static bool operator ==(bag<T> a, bag<T> b) => true; public static bool operator !=(bag<T> a, bag<T> b) => false; public override bool Equals(object? o) => o is bag<T>; public override int GetHashCode() => 0; }

            public static int size<T>(this bag<T> b) => 0;

            internal partial interface sizer
            {
                int size();
            }
        }
        """;

    private static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"));
    }

    private static (List<string> Lines, List<Diagnostic> Errors) Run(bool marked)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);

        CSharpCompilation shims = CSharpCompilation.Create(
            "closed-carrier-shims",
            [CSharpSyntaxTree.ParseText(ShimSource, parseOptions)],
            CoreReferences(),
            libraryOptions);

        using MemoryStream image = new();
        EmitResult emitted = shims.Emit(image);
        Assert.IsTrue(emitted.Success, $"shim compilation must emit clean: {string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))}");

        string source = Source;

        if (!marked)
        {
            Assert.IsTrue(source.Contains(ValueMarker) && source.Contains(PointerMarker), "the fixture carries both markers");
            source = source.Replace(PointerMarker, "(Pointer = true)").Replace(ValueMarker, "");
        }

        CSharpCompilation compilation = CSharpCompilation.Create(
            "closed-carrier",
            [CSharpSyntaxTree.ParseText(source, parseOptions)],
            CoreReferences().Append(MetadataReference.CreateFromImage(image.ToArray())),
            libraryOptions);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ImplementGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation updated, out ImmutableArray<Diagnostic> _);

        List<string> lines = driver.GetRunResult().Results
            .SelectMany(generator => generator.GeneratedSources)
            .SelectMany(generated => generated.SourceText.ToString().Split('\n'))
            .Select(line => line.Trim())
            .ToList();

        List<Diagnostic> errors = updated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();

        return (lines, errors);
    }

    /// <summary>THE SUBJECT: both closed pairs and the control compile. Red before the fix with CS0029.</summary>
    [TestMethod]
    public void AClosedCarrierPairCompiles()
    {
        (List<string> _, List<Diagnostic> errors) = Run(marked: true);

        Assert.AreEqual(0, errors.Count, $"the generated carriers must compile; saw: {string.Join(" | ", errors.Select(error => error.ToString()))}");
    }

    /// <summary>The value pair takes an adapter class over the one instantiation, and no open partial.</summary>
    [TestMethod]
    public void TheValuePairTakesAnAdapterNamedFromTheBareSymbol()
    {
        (List<string> lines, List<Diagnostic> _) = Run(marked: true);

        string carrier = lines.Single(line => line.Contains("class boxᴠvaluer"));

        StringAssert.Contains(carrier, "global::go.probe_package.valuer", "the carrier implements the interface");
        StringAssert.Contains(carrier, "IValueAdapter", "the carrier is a value adapter, so every unwrap site sees through it");
        Assert.IsTrue(lines.Any(line => line.Contains("global::go.probe_package.box<global::go.probe_package.point> m_value")), "the wrapped type is the closed instantiation, spelled in full");
        Assert.IsFalse(lines.Any(line => line.StartsWith("partial struct box<")), "the open definition takes no partial for a closed pair");
    }

    /// <summary>The pointer pair takes a NON-generic adapter over the one instantiation's box.</summary>
    [TestMethod]
    public void ThePointerPairTakesANonGenericAdapter()
    {
        (List<string> lines, List<Diagnostic> _) = Run(marked: true);

        string carrier = lines.Single(line => line.Contains("class cellжgetter"));

        Assert.IsFalse(carrier.Contains("cellжgetter<"), $"the carrier carries no type parameter list; saw: {carrier}");
        Assert.IsTrue(lines.Any(line => line.Contains("ж<global::go.probe_package.cell<global::go.probe_package.point>> m_box")), "the wrapped box is the closed instantiation's");
    }

    /// <summary>THE CONTROL: an unmarked generic record keeps the open partial.</summary>
    [TestMethod]
    public void AnUnmarkedGenericRecordKeepsTheOpenPartial()
    {
        (List<string> lines, List<Diagnostic> _) = Run(marked: true);

        StringAssert.Contains(lines.Single(line => line.StartsWith("partial struct bag<T>")), "sizer", "bag's record carries no marker");
    }

    /// <summary>
    /// The MARKER selects the route: the same records unmarked take the open forms, and those are the defect
    /// (a T returned where the interface wants a point). This arm is what shows the compile check can fail.
    /// </summary>
    [TestMethod]
    public void WithoutTheMarkerTheOpenRouteIsKept()
    {
        (List<string> lines, List<Diagnostic> errors) = Run(marked: false);

        Assert.IsTrue(lines.Any(line => line.StartsWith("partial struct box<T>")), "an unmarked value record extends the open definition");
        Assert.IsFalse(lines.Any(line => line.Contains("class boxᴠvaluer")), "no carrier without the marker");
        Assert.IsTrue(lines.Any(line => line.Contains("class cellжgetter<")), "an unmarked pointer record takes the generic adapter");
        Assert.IsTrue(errors.Any(error => error.Id is "CS0029" or "CS0266"), $"the open forms return a T for a point; saw: {string.Join(" | ", errors.Select(error => error.Id).Distinct())}");
    }
}
