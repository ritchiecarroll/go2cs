// VariadicInterfaceShellTests.cs - Gbtc
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
/// An interface with a Go variadic method gets the OBJECT shell, the runtime implementation golib's AdapterBinder builds
/// with no generic instantiation, forwarding each variadic member through a bound delegate of the type a generated
/// method-set arm has. Without it, the binder closed the generic shell over the asserted struct with MakeGenericType,
/// which Native AOT cannot do, so every assertion of a struct (or a pointer to one) to such an interface read false
/// under AOT where Go and the JIT read true (docs/phase4/DATA-aot-variadic-shell.md).
/// </summary>
[TestClass]
public class VariadicInterfaceShellTests
{
    private const string Package =
        """
        namespace go;

        [GoPackage("vtest")]
        public static partial class vtest_package
        {
            [GoType] public partial interface Errorer
            {
                void Error(params Span<object> args);
            }

            [GoType] public partial interface Logger
            {
                void Logf(@string format, params Span<object> args);
                nint Count();
                @string Join(@string sep, params Span<@string> parts);
            }

            // A variadic member with more fixed parameters than golib's ꓸꓸꓸ family carries: no delegate can bind it.
            [GoType] public partial interface Wide
            {
                void M(nint a1, nint a2, nint a3, nint a4, nint a5, nint a6, nint a7, nint a8, nint a9, params Span<object> rest);
            }

            [GoType] public partial interface Plain
            {
                nint Count();
            }

            [GoType] public partial struct V { public nint n; }

            public static void Error(this V v, params Span<object> args) { }

            public static void logf(this V v, @string format, params Span<object> args) { }

            public static nint count(this V v) => v.n;
        }
        """;

    private const string TemplateUsings =
        """
        global using static go.builtin;
        global using System;
        global using any = System.Object;
        """;

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    private static Dictionary<string, string> Generate() => Run().sources;

    // The generated sources, and the errors of the package compiled with the interfaces' generated parts and V's method
    // set (the struct's main part is left out, as GoMethodSetTests leaves it: this harness's Roslyn cannot resolve its
    // ToString over the net10 reference assemblies).
    private static (Dictionary<string, string> sources, Diagnostic[] errors) Run()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("vtest",
            [CSharpSyntaxTree.ParseText(Package, options, path: @"C:\go2cs\src\core\vtest\vtest.cs"),
             CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\vtest\obj\vtest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        SyntaxTree[] generated = [.. output.SyntaxTrees.Except(compilation.SyntaxTrees)];

        Compilation compiled = compilation.AddSyntaxTrees(generated.Where(tree =>
            System.IO.Path.GetFileName(tree.FilePath) is var name && (name.EndsWith(".MethodSet.g.cs", StringComparison.Ordinal) ||
            new[] { "Errorer", "Logger", "Wide", "Plain" }.Any(type => name.EndsWith($".{type}.g.cs", StringComparison.Ordinal)))));

        return (generated.ToDictionary(tree => System.IO.Path.GetFileName(tree.FilePath), tree => tree.ToString()),
            [.. compiled.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)]);
    }

    private static string For(Dictionary<string, string> sources, string name)
    {
        string? key = sources.Keys.FirstOrDefault(hint => hint.EndsWith($".{name}.g.cs", StringComparison.Ordinal));
        Assert.IsNotNull(key, $"the TypeGenerator must generate {name}; got: {string.Join(", ", sources.Keys)}");
        return sources[key!];
    }

    [TestMethod]
    public void TheShellsAndTheMethodSetCompile()
    {
        Diagnostic[] errors = Run().errors;

        Assert.AreEqual(0, errors.Length, "the interfaces' shells and V's method set compile against golib: " + string.Join("; ", errors.Take(5)));
    }

    [TestMethod]
    public void AVariadicInterfaceGetsTheObjectShell()
    {
        string generated = For(Generate(), "Errorer");

        StringAssert.Contains(generated, "typeof(ΔErrorer<>), typeof(ΔErrorerᴛObj), \"Error\"", "the object shell is named on the attribute");
        StringAssert.Contains(generated, "VariadicMethods = new string[] { \"Error\" }");
        StringAssert.Contains(generated, "VariadicDelegates = new global::System.Type[] { typeof(global::go.Actionꓸꓸꓸ<object>) }");
        StringAssert.Contains(generated, "sealed class ΔErrorerᴛObj");
        StringAssert.Contains(generated, "m_variadicᴛ = bindingᴛ.BindVariadic(");
        StringAssert.Contains(generated, "((global::go.Actionꓸꓸꓸ<object>)m_variadicᴛ![0]!)(args);");
    }

    [TestMethod]
    public void AMixedInterfaceForwardsItsVariadicMembersByDelegateAndTheRestByInvoker()
    {
        string generated = For(Generate(), "Logger");

        StringAssert.Contains(generated, "typeof(ΔLoggerᴛObj)");
        StringAssert.Contains(generated, "VariadicMethods = new string[] { \"Logf\", \"Join\" }");
        StringAssert.Contains(generated, "typeof(global::go.Actionꓸꓸꓸ<global::go.@string, object>), typeof(global::go.Funcꓸꓸꓸ<global::go.@string, global::go.@string, global::go.@string>)");
        StringAssert.Contains(generated, "((global::go.Actionꓸꓸꓸ<global::go.@string, object>)m_variadicᴛ![0]!)(format, args);");
        StringAssert.Contains(generated, "m_bindingᴛ.Invoke(1, m_targetᴛ)", "a member the invoker carries keeps the invoker");
        StringAssert.Contains(generated, "((global::go.Funcꓸꓸꓸ<global::go.@string, global::go.@string, global::go.@string>)m_variadicᴛ![2]!)(sep, parts);");
    }

    [TestMethod]
    public void AVariadicMemberNoDelegateFamilyCarriesKeepsTheInterfaceOffTheObjectShell()
    {
        string generated = For(Generate(), "Wide");

        StringAssert.Contains(generated, "typeof(ΔWide<>), null, \"M\"", "no object shell when a variadic member has no delegate type");
        Assert.IsFalse(generated.Contains("VariadicMethods"), "nothing names a delegate no family declares");
    }

    [TestMethod]
    public void AnInterfaceWithNoVariadicMemberIsUnchanged()
    {
        string generated = For(Generate(), "Plain");

        StringAssert.Contains(generated, "typeof(ΔPlain<>), typeof(ΔPlainᴛObj), \"Count\")]");
        Assert.IsFalse(generated.Contains("m_variadicᴛ"), "an interface the invoker carries whole gets no delegate slots");
    }

    [TestMethod]
    public void AnUnexportedVariadicMethodGetsAMethodSetArm()
    {
        string generated = For(Generate(), "V.MethodSet");

        StringAssert.Contains(generated, "case (\"logf\", false):", "an unexported variadic method, which an interface in its package can name");
        StringAssert.Contains(generated, "case (\"Error\", false):");
        Assert.IsFalse(generated.Contains("case (\"count\""), "an unexported method with no variadic tail still has no arm");
    }
}
