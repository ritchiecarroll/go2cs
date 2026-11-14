// InlineLayoutGeneratorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// The TypeGenerator over structs the converter admits to inline arrays (docs/phase4/DESIGN-array-value-storage.md,
/// 7.7.4, item 3): a <c>[GoInlineLayout]</c> struct measures its own managed layout for golib's gate, and generated
/// equality and hashing skip a <c>[0]T</c> alignment carrier, which shares bytes with the field Go places beside it.
/// </summary>
[TestClass]
public class InlineLayoutGeneratorTests
{
    private const string Path = @"C:\go2cs\src\core\ltest\ltest.cs";

    // S22 as I0 emits it; Z, a [0]int64 carrier over [2]float64 (Go: 0 == -0, so the arrays are equal while the
    // carrier's shared bytes differ); and Plain, a struct with no stamp.
    private const string Package =
        """
        using System.Runtime.InteropServices;
        using go.arrays;
        using go.golib;

        namespace go;

        [GoPackage("ltest")]
        public static partial class ltest_package
        {
            [GoInlineLayout, StructLayout(LayoutKind.Explicit)]
            public partial struct S22
            {
                [FieldOffset(0), GoAlignmentCarrier(typeof(array<short>))] public readonly short FforceAlignment;
                [FieldOffset(0)] public array201<sbyte> Fa;
                [FieldOffset(201)] public array1<byte> F__ccgo_pad2;
            }

            [GoInlineLayout, StructLayout(LayoutKind.Explicit)]
            public partial struct Z
            {
                [FieldOffset(0), GoAlignmentCarrier(typeof(array<long>))] public readonly long Carrier;
                [FieldOffset(0)] public array2<double> A;
            }

            public partial struct Plain
            {
                public long X;
            }
        }
        """;

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    // The family's hand-written helpers, compiled beside a generated chunk.
    private static string InlineArraysSource()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            foreach (string path in new[] { System.IO.Path.Combine(directory.FullName, "src", "core", "go.arrays", "InlineArrays.cs"), System.IO.Path.Combine(directory.FullName, "core", "go.arrays", "InlineArrays.cs") })
            {
                if (File.Exists(path))
                    return File.ReadAllText(path);
            }
        }

        throw new FileNotFoundException("src/core/go.arrays/InlineArrays.cs not found above " + AppContext.BaseDirectory);
    }

    // The package with the TypeGenerator's output added, and the generated source of each struct by name.
    private static (Compilation Output, Dictionary<string, string> Parts) Generate()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("ltest",
            [
                CSharpSyntaxTree.ParseText(Package, options, path: Path),
                CSharpSyntaxTree.ParseText(ArrayFamilyGenerator.Chunk(1, 201), options, path: "family.g.cs"),
                CSharpSyntaxTree.ParseText(InlineArraysSource(), options, path: "InlineArrays.cs"),
                CSharpSyntaxTree.ParseText("global using static go.builtin;\nglobal using System;\n", options, path: @"C:\go2cs\src\core\ltest\obj\ltest.GlobalUsings.g.cs")
            ],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out ImmutableArray<Diagnostic> _);

        Dictionary<string, string> parts = new();

        foreach (string name in new[] { "S22", "Z", "Plain" })
            parts[name] = output.SyntaxTrees.Skip(compilation.SyntaxTrees.Length).Select(tree => tree.ToString()).Single(source => source.Contains($" partial struct {name}\r\n") || source.Contains($" partial struct {name} "));

        return (output, parts);
    }

    private static Assembly Load(Compilation output)
    {
        using MemoryStream image = new();
        EmitResult emitted = output.Emit(image);

        Assert.IsTrue(emitted.Success, string.Join("\n", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));
        return Assembly.Load(image.ToArray());
    }

    [TestMethod]
    public void AStampedStructMeasuresItsOwnLayout()
    {
        (_, Dictionary<string, string> parts) = Generate();

        StringAssert.Contains(parts["S22"], "global::go.golib.IGoManagedLayout");
        StringAssert.Contains(parts["S22"], "[\"FforceAlignment\", \"Fa\", \"F__ccgo_pad2\"]");
        StringAssert.Contains(parts["S22"], "global::go.golib.GoManagedLayout.OffsetOf(in probe, in probe.Fa)");
        StringAssert.Contains(parts["S22"], "global::go.golib.GoManagedLayout.AlignmentOf<S22>()");
        Assert.IsFalse(parts["Plain"].Contains("IGoManagedLayout"), "an unstamped struct measures nothing");
    }

    [TestMethod]
    public void EqualityHashingAndToStringSkipTheCarrier()
    {
        (_, Dictionary<string, string> parts) = Generate();

        string equals = parts["Z"][parts["Z"].IndexOf("public bool Equals(", StringComparison.Ordinal)..parts["Z"].IndexOf("public override bool Equals(", StringComparison.Ordinal)];
        string hash = parts["Z"][parts["Z"].IndexOf("public override int GetHashCode()", StringComparison.Ordinal)..parts["Z"].IndexOf("public static bool operator ==", StringComparison.Ordinal)];

        Assert.IsFalse(equals.Contains("Carrier"), equals);
        StringAssert.Contains(equals, "this.A == other.A");
        Assert.IsFalse(hash.Contains("Carrier"), hash);

        string text = parts["Z"][parts["Z"].IndexOf("public override string ToString()", StringComparison.Ordinal)..];
        text = text[..text.IndexOf("}\");", StringComparison.Ordinal)];

        Assert.IsFalse(text.Contains("Carrier"), text);
        StringAssert.Contains(text, "A.ToString()");
    }

    // End to end: the generated table proves S22 to golib's gate, and Go's == holds where the carrier's bytes differ.
    [TestMethod]
    public void TheGeneratedMembersAnswerAsGoDoes()
    {
        (Compilation output, _) = Generate();
        Assembly assembly = Load(output);

        Type s22 = assembly.GetType("go.ltest_package+S22", throwOnError: true)!;
        Assert.IsNull(golib::go.GoReflect.InlineLayoutMismatch(s22));

        Type z = assembly.GetType("go.ltest_package+Z", throwOnError: true)!;
        object zero = Activator.CreateInstance(z)!, negative = Activator.CreateInstance(z)!;

        // A[0] = -0.0: the carrier's long reads 0x8000000000000000, the arrays are equal in Go.
        FieldInfo a = z.GetField("A")!;
        a.SetValue(negative, NegativeZeroArray(a.FieldType));

        Assert.AreNotEqual(0L, (long)z.GetField("Carrier")!.GetValue(negative)!, "the control: the carrier's bytes differ");
        Assert.IsTrue(zero.Equals(negative), "0 == -0, element by element");
        Assert.AreEqual(zero.GetHashCode(), negative.GetHashCode());
    }

    // An array2<double> whose element 0 is -0.0, built through the family's own indexer.
    private static object NegativeZeroArray(Type arrayType)
    {
        Type holder = typeof(NegativeZeroWriter<>).MakeGenericType(arrayType);
        return holder.GetMethod(nameof(NegativeZeroWriter<int>.Make))!.Invoke(null, null)!;
    }

    private static class NegativeZeroWriter<TArray> where TArray : struct
    {
        public static TArray Make()
        {
            TArray value = default;
            System.Runtime.CompilerServices.Unsafe.As<TArray, double>(ref value) = -0.0;
            return value;
        }
    }
}
