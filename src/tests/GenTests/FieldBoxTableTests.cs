// FieldBoxTableTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Guards the generated field-box table (golib's IGoFieldBoxSource; trim stage 3d, family E): every converted struct
/// answers reflect's addressable <c>Value.Field(i)</c> through a compiled switch over its Go fields, each arm the view
/// converted code builds for <c>&amp;s.f</c> (<c>box.of(S.Ꮡf)</c>), where golib emitted a DynamicMethod Native AOT cannot
/// run. GolibTests' FieldBoxTableRouteTests holds golib to taking the table; this holds the generator to writing it.
/// </summary>
/// <remarks>
/// Keys are GO field names, as golib's projection names them: a Δ-renamed field by its Go name, a blank field by its C#
/// name (Go's `_` repeats), a defined type over a struct by the fields it forwards. On EVERY struct with fields, not only
/// the operations face's: the face's rule misses a struct whose pointer comes only from `&amp;s` of an inferred variable.
/// An EMBEDDED field (the converter's `partial ref` property) is not covered here, because this harness's Roslyn predates
/// `partial ref` properties (CS0267); the trim stage 3d probe's `Outer` covers it where the SDK compiles it.
/// </remarks>
[TestClass]
public class FieldBoxTableTests
{
    private const string Path = @"C:\go2cs\src\core\ftest\ftest.cs";

    private const string Package =
        """
        namespace go;

        [GoPackage("ftest")]
        public static partial class ftest_package
        {
            public partial struct Plain { public nint A; public @string S; internal nint ΔB; internal nint _; }
            partial struct Unpointed { internal nint x; }
            [GoType("Plain")] public partial struct Def;
            public partial struct Generic<T> { public T V; }
            partial struct Empty { }
        }
        """;

    private const string TemplateUsings =
        """
        global using static go.builtin;
        global using System;
        global using System.Numerics;
        global using any = System.Object;
        """;

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    private static (string[] parts, Diagnostic[] tableErrors) Run()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("ftest",
            [CSharpSyntaxTree.ParseText(Package, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\ftest\obj\ftest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string[] parts = [.. output.SyntaxTrees.Except(compilation.SyntaxTrees).Select(tree => tree.ToString())];

        // This harness lacks some template usings, so only an error ON a table line is this change's (the real builds
        // compile the whole part).
        Diagnostic[] tableErrors = [.. output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error &&
            diagnostic.Location.SourceTree is { } tree && tree.GetText().Lines.GetLineFromPosition(diagnostic.Location.SourceSpan.Start).ToString() is var line &&
            (line.Contains("FieldBox") || line.Contains(".of(") || line.Contains("goref_")))];

        return (parts, tableErrors);
    }

    private static string PartFor(string[] parts, string declaration) => parts.Single(source => source.Contains(declaration));

    [TestMethod]
    public void EveryStructWithFieldsCarriesTheTable()
    {
        (string[] parts, Diagnostic[] tableErrors) = Run();

        Assert.AreEqual(0, tableErrors.Length, "the table lines compile: " + string.Join("; ", tableErrors.Take(3)));

        string plain = PartFor(parts, " partial struct Plain");
        StringAssert.Contains(plain, "global::go.IGoFieldBoxSource", "Plain implements the table");
        StringAssert.Contains(plain, "object? global::go.IGoFieldBoxSource.FieldBox(string field, object parentBox) => field switch", "through one switch");
        StringAssert.Contains(plain, "\"A\" => ((ж<Plain>)parentBox).of(ᏑA),", "a field is the view converted code builds for &s.A");
        StringAssert.Contains(plain, "\"S\" => ((ж<Plain>)parentBox).of(ᏑS),", "every field");
        StringAssert.Contains(plain, "\"B\" => ((ж<Plain>)parentBox).of(ᏑΔB),", "a collision-renamed field keyed by its Go name, accessed by its C# name");
        StringAssert.Contains(plain, "\"_\" => ((ж<Plain>)parentBox).of(goref__),", "a blank field keyed by its C# name");
        StringAssert.Contains(plain, "private static ref nint goref__(object box) => ref ((ж<Plain>)box).ValueSlot._;", "through its own accessor (Go gives a blank no address)");
        StringAssert.Contains(plain, "_ => null", "an unknown name answers null (golib falls back)");

        StringAssert.Contains(PartFor(parts, " partial struct Unpointed"), "\"x\" => ((ж<Unpointed>)parentBox).of(Ꮡx),", "a struct whose pointer is never spelled carries it too");
        StringAssert.Contains(PartFor(parts, " partial struct Generic<T>"), "\"V\" => ((ж<Generic<T>>)parentBox).of(ᏑV),", "a generic struct, over its own type parameters");

        string def = PartFor(parts, " partial struct Def");
        StringAssert.Contains(def, "global::go.IGoFieldBoxSource", "a defined type over a struct implements the table");
        StringAssert.Contains(def, "\"A\" => ((global::go.ж<Def>)parentBox).of(ᏑA),", "keyed by the field it forwards, through its forwarded accessor");

        Assert.IsFalse(PartFor(parts, " partial struct Empty").Contains("IGoFieldBoxSource"), "a struct with no fields carries none");
    }
}
