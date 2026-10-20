// MethodSetKeepTests.cs - Gbtc
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
/// Guards the full-trim method-set keep (MethodSetKeep, 2026-10-12): under Native AOT with a full trim, fmt printed
/// <c>reflect.Array</c> as <c>17</c> because golib's run-time method-set scan (the only way a type satisfies an interface
/// no converted code declares it does: fmt's Stringer against reflect.Kind, or against main's own types) could not see
/// the type's Go methods, whose metadata neither trimmer can know the scan reads. The TypeGenerator now names each Go
/// method of a type in a <c>[DynamicDependency]</c> on the type's generated <c>ToString</c> override, which both trimmers
/// keep exactly when the type is boxed.
/// </summary>
[TestClass]
public class MethodSetKeepTests
{
    private const string Path = @"C:\go2cs\src\core\mtest\mtest.cs";

    private const string Package =
        """
        namespace go;

        [GoPackage("mtest")]
        public static partial class mtest_package
        {
            [GoType("num:nuint")] public partial struct Kind;

            public partial struct S { public nint n; }

            public partial struct P { public nint n; }

            public partial struct Plain { public nint n; }

            public static @string String(this Kind k) => "kind"u8;

            public static @string String(this S s) => "ess"u8;

            internal static nint size(this S s) => s.n;

            public static @string String(this ref P p) => "pee"u8;

            public static nint NotAMethod(nint x) => x;
        }
        """;

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    private const string TemplateUsings =
        """
        global using static go.builtin;
        global using System;
        global using System.Numerics;
        global using any = System.Object;
        global using uint8 = System.Byte;
        global using uint16 = System.UInt16;
        global using uint32 = System.UInt32;
        global using uint64 = System.UInt64;
        global using int8 = System.SByte;
        global using int16 = System.Int16;
        global using int32 = System.Int32;
        global using int64 = System.Int64;
        """;

    private static (Dictionary<string, string> generated, Diagnostic[] errors, Compilation output) Run()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("mtest",
            [CSharpSyntaxTree.ParseText(Package, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\mtest\obj\mtest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        // RecvGenerator runs too, so the ж<P> twin a pointer receiver's dependency names exists to resolve against.
        CSharpGeneratorDriver.Create(new TypeGenerator(), new RecvGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        Dictionary<string, string> generated = output.SyntaxTrees.Except(compilation.SyntaxTrees)
            .Where(tree => tree.FilePath.Contains("go2cs.TypeGenerator"))
            .ToDictionary(tree => System.IO.Path.GetFileName(tree.FilePath), tree => tree.ToString());

        Diagnostic[] errors = output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

        return (generated, errors, output);
    }

    // The [DynamicDependency] signatures directly above the generated ToString of the type whose part is named name.
    private static string[] KeptOnToString(Dictionary<string, string> generated, string name)
    {
        string source = generated.Single(pair => pair.Key.EndsWith($".{name}.g.cs", StringComparison.Ordinal)).Value;
        string[] lines = source.Split('\n').Select(line => line.TrimEnd('\r')).ToArray();
        int toString = Array.FindIndex(lines, line => line.Contains("public override string ToString()"));

        Assert.IsTrue(toString > 0, $"{name}'s generated part has a ToString override");

        List<string> kept = [];

        for (int i = toString - 1; i >= 0 && lines[i].Contains("DynamicDependency("); i--)
        {
            Match match = Regex.Match(lines[i], @"DynamicDependency\(""([^""]+)"", typeof\(global::go\.mtest_package\)\)");
            Assert.IsTrue(match.Success, $"{name}: a dependency names the package class: {lines[i]}");
            kept.Add(match.Groups[1].Value);
        }

        return [.. kept.OrderBy(signature => signature, StringComparer.Ordinal)];
    }

    [TestMethod]
    public void EachGoTypesToStringKeepsItsGoMethods()
    {
        (Dictionary<string, string> generated, Diagnostic[] errors, Compilation output) = Run();

        // The inserted lines compile (this test compilation does not carry every template using, so only errors ON a
        // dependency line are this change's; the real builds compile the whole part).
        Diagnostic[] onDependencies = errors.Where(error => error.Location.SourceTree is { } tree &&
            tree.GetText().Lines.GetLineFromPosition(error.Location.SourceSpan.Start).ToString().Contains("DynamicDependency(")).ToArray();

        Assert.AreEqual(0, onDependencies.Length, "the dependency lines compile: " + string.Join("; ", onDependencies.Take(3)));

        // By NAME: a name keeps every same-named method of the package class, the ж<P> twin with P's own (and full
        // signatures crashed ILLink on the template-default publish, so none is written).
        CollectionAssert.AreEqual(new[] { "String" }, KeptOnToString(generated, "Kind"), "a named numeric type");
        CollectionAssert.AreEqual(new[] { "String", "size" }, KeptOnToString(generated, "S"), "a struct, exported and unexported methods (a sealed interface's marker is unexported)");
        CollectionAssert.AreEqual(new[] { "String" }, KeptOnToString(generated, "P"), "a pointer receiver (this ref P)");
        Assert.AreEqual(0, KeptOnToString(generated, "Plain").Length, "a type without methods keeps nothing");

        // Each name keeps the receiver methods golib binds: P's covers both its `this ref P` method and the ж<P> twin
        // RecvGenerator writes, and the parameter-less form resolves through the package class alone.
        INamedTypeSymbol package = output.GetTypeByMetadataName("go.mtest_package")!;
        string[] stringReceivers = [.. package.GetMembers("String").OfType<IMethodSymbol>().Select(method => method.Parameters[0].Type.ToDisplayString()).OrderBy(name => name, StringComparer.Ordinal)];

        CollectionAssert.AreEqual(new[] { "go.mtest_package.Kind", "go.mtest_package.P", "go.mtest_package.S", "go.ж<go.mtest_package.P>" }, stringReceivers,
            "the kept name covers every receiver form, the twin included: " + string.Join(", ", stringReceivers));
    }
}
