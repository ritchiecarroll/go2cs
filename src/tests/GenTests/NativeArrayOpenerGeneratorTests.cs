// NativeArrayOpenerGeneratorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Guards the generated native array openers (G's ruling, 2026-10-19: NARROWER): golib builds a foreign <c>*[N]*E</c>
/// and the window over a native struct's <c>[N]X</c> field only for an element some compilation registers, and a miss
/// refuses by name. The generator must register every element a door spells and nothing else. The fixture's doors:
/// AliasPointer, an inferred unsafe.Slice over a <c>**E</c>, ArrayPointer, NativeArrayPointer and
/// NativeFieldArrayPointer over <c>ж&lt;E&gt;</c>, and struct fields <c>[N]X</c> and <c>[N]*E</c>; and its
/// non-doors: a local <c>[N]*E</c>, a raw uintptr conversion, an unsafe.Slice over a value element, and an open
/// generic site.
/// </summary>
[TestClass]
public class NativeArrayOpenerGeneratorTests
{
    private const string Path = @"C:\go2cs\src\core\ntest\ntest.cs";

    private const string Package =
        """
        namespace go;

        [GoPackage("ntest")]
        public static partial class ntest_package
        {
            internal partial struct Alias { internal nint x; }
            internal partial struct Sliced { internal nint x; }
            internal partial struct Viewed { internal nint x; }
            internal partial struct Opened { internal nint x; }
            internal partial struct Field { internal nint x; }
            internal partial struct Pointed { internal nint x; }
            internal partial struct Local { internal nint x; }
            internal partial struct Raw { internal nint x; }
            private partial struct Secret { internal nint x; }

            internal partial struct Record
            {
                internal uint32 count;
                internal array<uint16> name;
                internal array<ж<Pointed>> chains;
                private array<Secret> secrets;
            }

            internal partial struct Generic<T>
            {
                internal array<T> items;
            }

            internal static void Use(ж<ж<Alias>> alias, ж<ж<Sliced>> sliced, ж<nint> values, uintptr raw)
            {
                _ = array<ж<Alias>>.AliasPointer(alias, 4);
                _ = @unsafe.Slice(sliced, 2);
                _ = @unsafe.Slice(values, 2);
                _ = @unsafe.ArrayPointer<ж<Viewed>>.Of(null, 2);
                _ = NativeArrayPointer<ж<Opened>>(0, 2);
                _ = new array<ж<Local>>(4);
                _ = (ж<array<ж<Raw>>>)raw;
            }

            internal static void Open<T>(ж<ж<T>> element) => _ = array<ж<T>>.AliasPointer(element, 4);
        }

        public static partial class unsafe_package
        {
            public static slice<T> Slice<T, TLen>(ж<T> ptr, TLen len) where TLen : System.Numerics.IBinaryInteger<TLen> => default;

            public static class ArrayPointer<T>
            {
                public static ж<array<T>> Of(INilPointer? field, nint length) => default!;
            }
        }
        """;

    private const string TemplateUsings =
        """
        global using static go.builtin;
        global using System;
        global using System.Numerics;
        global using any = System.Object;
        global using uint16 = System.UInt16;
        global using uint32 = System.UInt32;
        global using @unsafe = go.unsafe_package;
        """;

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    private static (string[] pointerArrays, string[] arrays, Compilation output) Run()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("ntest",
            [CSharpSyntaxTree.ParseText(Package, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\ntest\obj\ntest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new NativeArrayOpenerGenerator()).WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string[] sources = output.SyntaxTrees.Select(tree => tree.ToString()).ToArray();

        string[] registered(string door) => sources
            .SelectMany(source => Regex.Matches(source, $@"global::go\.NativeArrayOpeners\.{door}<(.+)>\(\);").Select(match => match.Groups[1].Value))
            .OrderBy(name => name, StringComparer.Ordinal).ToArray();

        return (registered("RegisterPointerArray"), registered("RegisterArray"), output);
    }

    [TestMethod]
    public void EverySpelledDoorRegistersItsElementAndNothingElse()
    {
        (string[] pointerArrays, string[] arrays, Compilation output) = Run();

        string[] expectedPointerArrays =
        [
            "global::go.ntest_package.Alias",    // array<ж<E>>.AliasPointer
            "global::go.ntest_package.Opened",   // NativeArrayPointer<ж<E>>
            "global::go.ntest_package.Pointed",  // a [N]*E field: its window opens the pointer array
            "global::go.ntest_package.Sliced",   // unsafe.Slice over a **E, inferred
            "global::go.ntest_package.Viewed",   // ArrayPointer<ж<E>>
        ];

        string[] expectedArrays =
        [
            "global::go.ntest_package.Secret",                   // a private element: registered inside the package class
            "global::go.ж<global::go.ntest_package.Pointed>",    // the [N]*E field's own window
            "ushort",                                            // a [N]uint16 field
        ];

        CollectionAssert.AreEqual(expectedPointerArrays.OrderBy(name => name, StringComparer.Ordinal).ToArray(), pointerArrays, "pointer arrays: " + string.Join(", ", pointerArrays));
        CollectionAssert.AreEqual(expectedArrays.OrderBy(name => name, StringComparer.Ordinal).ToArray(), arrays, "arrays: " + string.Join(", ", arrays));

        Assert.IsFalse(pointerArrays.Any(name => name.Contains("Local") || name.Contains("Raw")), "a local [N]*E and a raw uintptr conversion are not doors");
        Assert.IsFalse(pointerArrays.Concat(arrays).Any(name => name == "nint"), "unsafe.Slice over a value element reads no pointer words");
        Assert.IsFalse(pointerArrays.Concat(arrays).Any(name => name == "T" || name.Contains("<T>")), "an open generic site registers nothing");

        Diagnostic[] errors = output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.AreEqual(0, errors.Length, "the registry binds: " + string.Join("; ", errors.Take(3)));
    }
}
