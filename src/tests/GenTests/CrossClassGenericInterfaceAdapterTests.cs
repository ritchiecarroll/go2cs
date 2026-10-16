// CrossClassGenericInterfaceAdapterTests.cs - Gbtc
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
/// A pointer adapter hosted in one package class for a GENERIC interface declared in ANOTHER package
/// class of the same compilation.
/// </summary>
/// <remarks>
/// <para>
/// Measured on <c>crypto/mlkem</c> at Go 1.26.9. Its test moved to the external <c>package mlkem_test</c>,
/// which declares <c>decapsulationKey[E encapsulationKey]</c> and <c>encapsulationKey</c>, implemented by
/// production <c>*DecapsulationKey768/1024</c> and <c>*EncapsulationKey768/1024</c>. Those records anchor to
/// the production class under the recompile model, so the adapters are hosted in <c>mlkem_package</c> while
/// the interfaces live in <c>mlkem_test_package</c>. The non-generic adapter compiled. The generic one named
/// its base interface BARE (<c>decapsulationKey&lt;global::go.crypto.mlkem_test_package.encapsulationKey&gt;</c>),
/// which does not resolve from <c>mlkem_package</c>: CS0246 x2 and CS0540 x6.
/// </para>
/// <para>
/// The bare name came from <c>Common.GetFullTypeName</c>, whose generic arm renders <c>Name&lt;args&gt;</c>
/// without the containing class even when asked for the display string. A same-class interface resolves
/// bare by construction, which is why every generic-interface record before this one compiled.
/// </para>
/// <para>
/// The shims are a SEPARATE referenced assembly for <c>WhiteboxProjectedResultTests</c>' reason: with
/// <c>ж&lt;T&gt;</c> in the compilation under test the generator derives its host class from it.
/// </para>
/// </remarks>
[TestClass]
public class CrossClassGenericInterfaceAdapterTests
{
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
        """;

    /// <summary>
    /// The attribute-bearing file. Its FIRST class is <c>prod_package</c>, so every adapter it records is
    /// hosted there.
    /// </summary>
    /// <remarks>
    /// Three records: the subject (generic interface in the OTHER class), the non-generic interface in the
    /// other class (compiled before the fix, the control for the cross-class host), and a generic interface
    /// declared in the HOST class (compiled before the fix, the control for the generic arm).
    /// The control's struct is its own: two generic records on ONE struct whose type arguments share a
    /// tail group as a collision (the key is the simple name of the whole display string) and take a
    /// package prefix, an axis this fixture does not vary.
    /// </remarks>
    private const string HostSource =
        """
        using go;

        [assembly: GoImplement<global::go.prod_package.DecapKey, global::go.probe_package.decapKeyLike<global::go.probe_package.encapKeyLike>>(Pointer = true)]
        [assembly: GoImplement<global::go.prod_package.EncapKey, global::go.probe_package.encapKeyLike>(Pointer = true)]
        [assembly: GoImplement<global::go.prod_package.HostKey, global::go.prod_package.hostDecapLike<global::go.probe_package.encapKeyLike>>(Pointer = true)]

        namespace go;

        public static partial class prod_package
        {
            public partial struct EncapKey
            {
            }

            public static object Bytes(this ж<EncapKey> Ꮡe) => default!;

            public partial struct DecapKey
            {
            }

            public static ж<EncapKey> EncapsulationKey(this ж<DecapKey> Ꮡd) => default!;

            public static object Bytes(this ж<DecapKey> Ꮡd) => default!;

            public partial struct HostKey
            {
            }

            public static object Bytes(this ж<HostKey> Ꮡh) => default!;

            internal partial interface hostDecapLike<E>
            {
                object Bytes();
            }
        }
        """;

    /// <summary>The interfaces' own class: the external test package's shape.</summary>
    private const string InterfaceSource =
        """
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

    private static (Dictionary<string, string> Adapters, Compilation Updated) RunImplementGenerator()
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);

        CSharpCompilation shims = CSharpCompilation.Create(
            "cross-class-generic-shims",
            [CSharpSyntaxTree.ParseText(ShimSource, parseOptions)],
            CoreReferences(),
            libraryOptions);

        using MemoryStream image = new();
        EmitResult emitted = shims.Emit(image);
        Assert.IsTrue(emitted.Success, $"shim compilation must emit clean: {string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))}");

        CSharpCompilation consuming = CSharpCompilation.Create(
            "cross-class-generic-test",
            [CSharpSyntaxTree.ParseText(HostSource, parseOptions), CSharpSyntaxTree.ParseText(InterfaceSource, parseOptions)],
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

    /// <summary>The adapter's class declaration line.</summary>
    private static string ClassLine(string adapter)
    {
        List<string> lines = adapter
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Contains("sealed class "))
            .ToList();

        Assert.AreEqual(1, lines.Count, $"exactly one adapter class declaration; saw {lines.Count} in: {adapter}");

        return lines[0];
    }

    /// <summary>
    /// THE SUBJECT: the whole compilation is clean. Red before the fix with crypto/mlkem's CS0246 and
    /// CS0540.
    /// </summary>
    [TestMethod]
    public void AGenericInterfaceFromAnotherClassCompiles()
    {
        (Dictionary<string, string> _, Compilation updated) = RunImplementGenerator();

        List<Diagnostic> errors = updated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToList();

        Assert.AreEqual(0, errors.Count,
            $"an adapter hosted outside its generic interface's class must compile; saw: {string.Join(" | ", errors.Select(diagnostic => diagnostic.ToString()))}");
    }

    /// <summary>
    /// The base list names the interface through its OWN class, and the class name stays the simple
    /// composition (struct, pointer glyph, unqualified interface name).
    /// </summary>
    [TestMethod]
    public void TheBaseListQualifiesTheGenericInterface()
    {
        (Dictionary<string, string> adapters, Compilation _) = RunImplementGenerator();

        string line = ClassLine(Adapter(adapters, "DecapKey", "decapKeyLike", "-ptr"));

        StringAssert.Contains(line, "class DecapKeyжdecapKeyLike : global::go.probe_package.decapKeyLike<global::go.probe_package.encapKeyLike>,",
            "the generic interface must be named through probe_package, where it is declared");
    }

    /// <summary>
    /// THE GENERIC-ARM CONTROL: a generic interface declared in the HOST class keeps its adapter name, and
    /// its base list names the same type it did before (now qualified, which resolves the same symbol).
    /// </summary>
    [TestMethod]
    public void AHostClassGenericInterfaceKeepsItsAdapterName()
    {
        (Dictionary<string, string> adapters, Compilation _) = RunImplementGenerator();

        string line = ClassLine(Adapter(adapters, "HostKey", "hostDecapLike", "-ptr"));

        StringAssert.Contains(line, "class HostKeyжhostDecapLike : ", "the adapter name is composed from the simple interface name");
        StringAssert.Contains(line, "hostDecapLike<global::go.probe_package.encapKeyLike>,", "the type argument is unchanged");
    }

    /// <summary>
    /// THE CROSS-CLASS CONTROL: the non-generic interface from the other class was already qualified.
    /// </summary>
    [TestMethod]
    public void ANonGenericInterfaceFromAnotherClassIsQualified()
    {
        (Dictionary<string, string> adapters, Compilation _) = RunImplementGenerator();

        string line = ClassLine(Adapter(adapters, "EncapKey", "encapKeyLike", "-ptr"));

        StringAssert.Contains(line, "class EncapKeyжencapKeyLike : global::go.probe_package.encapKeyLike,",
            "the non-generic interface is qualified through its own class");
    }
}
