// NamedPointerValueSlotTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// The converter reads a dereference whose result is itself reference-like (a map, a slice, a pointer)
/// through the box's nil-check-free <c>ValueSlot</c>, because Go's <c>*pp</c> may legally yield nil. A
/// named pointer type (<c>type P *M</c>) is generated as a class over a <c>ж&lt;M&gt;</c> box, not the box
/// itself, so it must forward that slot as a ref: without it <c>len(*p)</c> was CS1061 at every such
/// dereference.
/// </summary>
[TestClass]
public class NamedPointerValueSlotTests
{
    private const string Source =
        """
        namespace go
        {
            public class GoTypeAttribute : System.Attribute
            {
                public GoTypeAttribute() { }
                public GoTypeAttribute(string definition) { }
            }

            partial class demo_package
            {
                [GoType("map[nint, nint]")] partial struct M;

                [GoType("ж<M>")] partial class P;
            }
        }
        """;

    private static string GeneratedNamedPointer()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        CSharpCompilation compilation = CSharpCompilation.Create("named-pointer-valueslot-test",
            [CSharpSyntaxTree.ParseText(Source, new CSharpParseOptions(LanguageVersion.Latest))],
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"))
            ],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new TypeGenerator());
        driver = driver.RunGenerators(compilation);

        Dictionary<string, string> sources = driver.GetRunResult().Results
            .SelectMany(generator => generator.GeneratedSources)
            .ToDictionary(source => source.HintName, source => source.SourceText.ToString());

        string? key = sources.Keys.FirstOrDefault(hint => hint.Contains(".P."));
        Assert.IsNotNull(key, $"the TypeGenerator must generate P; got: {string.Join(", ", sources.Keys)}");

        return sources[key!];
    }

    [TestMethod]
    public void ANamedPointerForwardsTheBoxValueSlotByRef()
    {
        string generated = GeneratedNamedPointer();

        PropertyDeclarationSyntax? valueSlot = CSharpSyntaxTree.ParseText(generated).GetRoot()
            .DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault(property => property.Identifier.Text == "ValueSlot");

        Assert.IsNotNull(valueSlot, $"the named pointer P must declare ValueSlot:\n{generated}");
        Assert.IsInstanceOfType(valueSlot!.Type, typeof(RefTypeSyntax), "ValueSlot must return by ref, so a write through `*p` reaches the box");
        StringAssert.Contains(valueSlot.ExpressionBody?.Expression.ToString() ?? "", "m_value.ValueSlot", "ValueSlot must forward the box's own slot");
    }

    // TS row C18: a *T parameter the converter lowers to `ref T` takes `ref p.DerefOrNull()` at the call site, and a named
    // pointer argument is the wrapper, which the box's extension does not reach. The wrapper's own alias is an EXTENSION
    // method of the package class, null-safe, because a named pointer's zero value is a null reference and Go's `f(p)`
    // with a nil p panics inside f, at its first dereference, not at the call.
    [TestMethod]
    public void ANamedPointerGetsANullSafeDerefAliasBesideIt()
    {
        string generated = GeneratedNamedPointer();

        MethodDeclarationSyntax? alias = CSharpSyntaxTree.ParseText(generated).GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(method => method.Identifier.Text == "DerefOrNull");

        Assert.IsNotNull(alias, $"the named pointer P must have a DerefOrNull alias:\n{generated}");
        Assert.IsInstanceOfType(alias!.ReturnType, typeof(RefTypeSyntax), "the alias must return by ref, so the callee's writes reach the pointee");
        Assert.IsTrue(alias.Modifiers.Any(SyntaxKind.StaticKeyword) && alias.ParameterList.Parameters.Single().Modifiers.Any(SyntaxKind.ThisKeyword),
            "the alias must be an extension method, which a null wrapper reaches without a fault");
        Assert.AreEqual("demo_package", (alias.Parent as ClassDeclarationSyntax)?.Identifier.Text, "the alias must live in the package class, beside the wrapper");

        string body = alias.ExpressionBody?.Expression.ToString() ?? "";
        StringAssert.Contains(body, "ʗp is null ? ref global::System.Runtime.CompilerServices.Unsafe.NullRef<M>()", "a null wrapper must bind a null ref");
        StringAssert.Contains(body, "((ж<M>)ʗp).DerefOrNull()", "a wrapper must answer the box's own alias");
    }
}
