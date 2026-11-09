// DefinedTypeOverInstantiationTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// A Go DEFINED type over an INSTANTIATED generic struct: <c>type Q pair[int]</c>, <c>type R[U comparable] pair[U]</c>,
/// <c>type C[V any] R[V]</c>, <c>type E C[bool]</c>, and <c>type M lib.Pair[string]</c> over another package's
/// struct (GO2CS2004's other half). Each has the struct's fields at the instantiation's types.
/// <list type="bullet">
/// <item>The TypeGenerator forwards each field with the struct's type parameter replaced by the argument the
/// wrapper's definition gives it, through every definition in a chain: Q's <c>ref nint A</c>, R's
/// <c>ref U A</c>, E's <c>ref bool A</c>, M's <c>ref @string A</c> read from metadata. Unsubstituted, the
/// wrapper declared <c>ref T A</c> over a T nothing declares (CS0246).</item>
/// <item>The ImplicitConvGenerator hosts a conversion with a generic operand where C# allows one: on the
/// non-generic side (<c>partial struct E</c>), or OPEN on the generic wrapper when both sides are generic
/// (<c>partial struct C&lt;V&gt; { operator pair&lt;V&gt;(C&lt;V&gt; src) }</c>). A closed host,
/// <c>partial struct pair&lt;Boolean&gt;</c>, is CS0264.</item>
/// <item>A conversion TO any defined type over a struct constructs it through each definition from the
/// source's fields, <c>new Q(new pair&lt;nint&gt;(src.A, src.B))</c>. It was built bare, <c>new Q()</c>,
/// which compiled and dropped every field value: Go's <c>Q(s).B</c> read 0 (the plain <c>type t3 t4</c>
/// shape too).</item>
/// </list>
/// </summary>
[TestClass]
public class DefinedTypeOverInstantiationTests
{
    private const string Path = @"C:\go2cs\src\core\dtoi\dtoi.cs";

    private const string Package =
        """
        using go;

        [assembly: GoImplicitConv<go.dtoi_package.E, go.dtoi_package.pair<bool>>]
        [assembly: GoImplicitConv<go.dtoi_package.pair<bool>, go.dtoi_package.E>]
        [assembly: GoImplicitConv<go.dtoi_package.C<go.@string>, go.dtoi_package.pair<go.@string>>]
        [assembly: GoImplicitConv<go.dtoi_package.pair<go.@string>, go.dtoi_package.C<go.@string>>]
        [assembly: GoImplicitConv<go.dtoi_package.C<nint>, go.dtoi_package.pair<nint>>]
        [assembly: GoImplicitConv<go.dtoi_package.S, go.dtoi_package.Q>]
        [assembly: GoImplicitConv<go.dtoi_package.Q, go.dtoi_package.S>]

        namespace go;

        [GoPackage("dtoi")]
        public static partial class dtoi_package
        {
            public partial struct pair<T> {
                public T A;
                public nint B;
            }

            public partial struct S {
                public nint A;
                public nint B;
            }

            public partial struct Q /*pair<nint>*/;

            public partial struct R<U> /*pair<U>*/;

            public partial struct C<V> /*R<V>*/;

            public partial struct E /*C<bool>*/;

            public partial struct M /*global::go.lib_package.Pair<@string>*/;

            public static string Probe()
            {
                var q = (Q)new S(A: 1, B: 2);
                var e = (E)new pair<bool>(A: true, B: 3);
                var c = (C<@string>)new pair<@string>(A: "c", B: 4);
                var m = new M(new lib_package.Pair<@string>(A: "m", B: 5));
                var r = new R<nint>(new pair<nint>(A: 6));
                return $"{q.A} {q.B} {((S)q).B} {e.A} {e.B} {((pair<bool>)e).B} {c.A} {((pair<@string>)c).B} {((pair<nint>)new C<nint>(r)).A} {m.A} {m.B}";
            }
        }
        """;

    // The other package: a generic struct the consumer reads from metadata, as a referenced project arrives.
    private const string Library =
        """
        namespace go;

        public static partial class lib_package
        {
            public partial struct Pair<T>
            {
                public T A;
                public nint B;

                public Pair(T A = default!, nint B = default!) { this.A = A; this.B = B; }
            }
        }
        """;

    private const string TemplateUsings =
        """
        global using static go.builtin;
        global using System;
        global using System.Numerics;
        global using any = System.Object;
        """;

    private static List<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location))
            .ToList();
    }

    private static (Dictionary<string, string> generated, Diagnostic[] errors, Compilation output, byte[] library) Run()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);
        CSharpCompilationOptions compilationOptions = new(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);
        List<MetadataReference> references = References();

        CSharpCompilation library = CSharpCompilation.Create("dtoilib", [CSharpSyntaxTree.ParseText(Library, options)], references, compilationOptions);

        using MemoryStream libraryImage = new();
        EmitResult emitted = library.Emit(libraryImage);
        Assert.IsTrue(emitted.Success, $"the library must emit clean: {string.Join("; ", emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))}");
        references.Add(MetadataReference.CreateFromImage(libraryImage.ToArray()));

        CSharpCompilation compilation = CSharpCompilation.Create("dtoi",
            [CSharpSyntaxTree.ParseText(Package, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\dtoi\obj\dtoi.GlobalUsings.g.cs")],
            references,
            compilationOptions);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new TypeGenerator(), new ImplicitConvGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        Dictionary<string, string> generated = driver.GetRunResult().Results
            .SelectMany(result => result.GeneratedSources)
            .ToDictionary(source => source.HintName, source => source.SourceText.ToString());

        Diagnostic[] errors = output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

        return (generated, errors, output, libraryImage.ToArray());
    }

    private static string GeneratedFor(Dictionary<string, string> sources, string hint)
    {
        string? key = sources.Keys.FirstOrDefault(name => name.Contains(hint));
        Assert.IsNotNull(key, $"nothing was generated for {hint}; got: {string.Join(", ", sources.Keys)}");
        return sources[key!];
    }

    [TestMethod]
    public void AWrapperOverAnInstantiationForwardsTheFieldsAtTheInstantiationsTypes()
    {
        (Dictionary<string, string> generated, _, _, _) = Run();

        StringAssert.Contains(GeneratedFor(generated, "dtoi_package.Q.g.cs"), "public ref nint A => ref m_value.A;");
        StringAssert.Contains(GeneratedFor(generated, "dtoi_package.R_U_.g.cs"), "public ref U A => ref m_value.A;");
        StringAssert.Contains(GeneratedFor(generated, "dtoi_package.C_V_.g.cs"), "public ref V A => ref m_value.A;");
        StringAssert.Contains(GeneratedFor(generated, "dtoi_package.E.g.cs"), "public ref bool A => ref m_value.A;");
        StringAssert.Contains(GeneratedFor(generated, "dtoi_package.M.g.cs"), "public ref @string A => ref m_value.A;");
    }

    [TestMethod]
    public void AConversionWithAGenericOperandIsHostedWhereCSharpAllowsIt()
    {
        (Dictionary<string, string> generated, _, _, _) = Run();
        string conversions = string.Join("\n", generated.Where(source => source.Key.Contains('-')).Select(source => source.Value));

        StringAssert.Contains(conversions, "partial struct E\r\n    {\r\n        public static implicit operator pair<bool>(E src) => new pair<bool>(src.A, src.B);");
        StringAssert.Contains(conversions, "public static implicit operator E(pair<bool> src) => new E(new C<bool>(new R<bool>(new pair<bool>(src.A, src.B))));");
        StringAssert.Contains(conversions, "partial struct C<V>\r\n    {\r\n        public static implicit operator pair<V>(C<V> src) => new pair<V>(src.A, src.B);");
        StringAssert.Contains(conversions, "public static implicit operator C<V>(pair<V> src) => new C<V>(new R<V>(new pair<V>(src.A, src.B)));");
        Assert.AreEqual(1, generated.Values.Count(source => source.Contains("operator pair<V>(C<V> src)")), "two instantiations of one open conversion declare it once");
    }

    [TestMethod]
    public void AConversionToADefinedTypeOverAStructKeepsTheFieldValues()
    {
        (Dictionary<string, string> generated, Diagnostic[] errors, Compilation output, byte[] library) = Run();

        StringAssert.Contains(string.Join("\n", generated.Values), "public static implicit operator Q(S src) => new Q(new pair<nint>(src.A, src.B));");
        Assert.AreEqual(0, errors.Length, "the package compiles: " + string.Join("; ", errors.Take(5)));

        AppDomain.CurrentDomain.AssemblyResolve += (_, args) => args.Name.StartsWith("dtoilib,", StringComparison.Ordinal) ? Assembly.Load(library) : null;

        using MemoryStream image = new();
        Assert.IsTrue(output.Emit(image).Success, "the package emits an assembly");
        System.Reflection.MethodInfo probe = Assembly.Load(image.ToArray()).GetType("go.dtoi_package", throwOnError: true)!.GetMethod("Probe")!;

        // Go: Q(S{1, 2}) is {1 2}; E(pair[bool]{true, 3}) is {true 3}; C[string](pair[string]{"c", 4}) is {c 4}.
        Assert.AreEqual("1 2 2 True 3 3 c 4 6 m 5", probe.Invoke(null, null)!.ToString());
    }
}
