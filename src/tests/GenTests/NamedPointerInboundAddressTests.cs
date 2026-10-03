// NamedPointerInboundAddressTests.cs - Gbtc
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
/// A named pointer type (`type nodeptr *node`) is generated as a class over a `ж&lt;node&gt;` box, with
/// conversions to and from a raw address. Converting an ADDRESS into the pointer must ALIAS that
/// address, exactly as `ж&lt;T&gt;`'s own `uintptr` operator does and says ("it must never box a COPY of
/// the pointed-at value"): a copy silently discards the address, so a write through the pointer never
/// reaches the storage it was made from. The template's two inbound operators did copy -- they read
/// `*(T*)value` into a fresh box -- which for a pointee that holds references is also CS8500, a pointer
/// to a managed type (four records in log/slog's generated groupptr and timeLocation).
/// </summary>
[TestClass]
public class NamedPointerInboundAddressTests
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
                partial struct node
                {
                    public nint v;
                    public object next;
                }

                [GoType("ж<node>")] partial class nodeptr;
            }
        }
        """;

    private static string GeneratedNamedPointer()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        CSharpCompilation compilation = CSharpCompilation.Create("named-pointer-test",
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

        string? key = sources.Keys.FirstOrDefault(hint => hint.Contains(".nodeptr."));
        Assert.IsNotNull(key, $"the TypeGenerator must generate nodeptr; got: {string.Join(", ", sources.Keys)}");

        return sources[key!];
    }

    // The conversion operators that take a raw address and return the named pointer.
    private static List<ConversionOperatorDeclarationSyntax> InboundAddressOperators(string generated)
    {
        List<ConversionOperatorDeclarationSyntax> inbound = CSharpSyntaxTree.ParseText(generated).GetRoot()
            .DescendantNodes().OfType<ConversionOperatorDeclarationSyntax>()
            .Where(op => op.Type.ToString() == "nodeptr" &&
                         op.ParameterList.Parameters.Single().Type!.ToString() is "uintptr" or "void*")
            .ToList();

        Assert.AreEqual(2, inbound.Count, "control: the named pointer must carry an inbound operator from uintptr and one from void*");

        return inbound;
    }

    [TestMethod]
    public void AnInboundAddressAliasesTheAddressAndNeverReadsThePointee()
    {
        foreach (ConversionOperatorDeclarationSyntax op in InboundAddressOperators(GeneratedNamedPointer()))
        {
            string from = op.ParameterList.Parameters.Single().Type!.ToString();

            // No dereference of the address at all: `*(node*)value` copies the pointee out of it.
            bool dereferences = op.DescendantNodes().OfType<PrefixUnaryExpressionSyntax>()
                .Any(unary => unary.IsKind(SyntaxKind.PointerIndirectionExpression));

            Assert.IsFalse(dereferences,
                $"the {from} -> nodeptr operator reads the pointee through the address, so the result is a copy and not an alias:\n{op}");

            // No pointer to the pointee type either: for a pointee holding references that is CS8500.
            bool pointeePointer = op.DescendantNodes().OfType<PointerTypeSyntax>()
                .Any(pointer => pointer.ElementType.ToString().Contains("node"));

            Assert.IsFalse(pointeePointer, $"the {from} -> nodeptr operator declares a pointer to the pointee type:\n{op}");

            // The box's own operator does the conversion: it aliases, and resolves an order token.
            bool defersToTheBox = op.DescendantNodes().OfType<CastExpressionSyntax>()
                .Any(cast => cast.Type.ToString() == "ж<node>");

            Assert.IsTrue(defersToTheBox, $"the {from} -> nodeptr operator must convert through ж<node>'s own address operator:\n{op}");
        }
    }
}
