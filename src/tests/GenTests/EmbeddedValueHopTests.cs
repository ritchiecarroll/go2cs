// EmbeddedValueHopTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go2cs.Symbols;

namespace go2cs.Tests;

// The embed-hop readers find a struct's PROMOTING embeds in either declared form: the legacy `partial ref` property,
// and the converter-declared `/*embed*/` field (owner ruling (A), 2026-10-16: every embed is a field in Go order). The
// same mark on an interface or predeclared embed promotes nothing, so it is never a hop: the reader decides by the
// field's TYPE, which is why it takes the compilation that owns the declaration.
[TestClass]
public class EmbeddedValueHopTests
{
    // A stand-in for golib's pointer box: the readers key on the `ж<T>` shape.
    private const string Box = "namespace go { public sealed class ж<T> { } }";

    private static (StructDeclarationSyntax declaration, Compilation compilation) Parse(string source, string structName)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
        Compilation compilation = CSharpCompilation.Create("EmbedHopProbe", [tree, CSharpSyntaxTree.ParseText(Box)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);

        StructDeclarationSyntax declaration = tree.GetCompilationUnitRoot().DescendantNodes().OfType<StructDeclarationSyntax>()
            .Single(candidate => candidate.Identifier.Text == structName);

        return (declaration, compilation);
    }

    [TestMethod]
    public void TypeCollisionRenamedPropertyStillCountsAsEmbeddedValue()
    {
        (StructDeclarationSyntax declaration, Compilation compilation) = Parse(
            """
            partial struct Buffer
            {
                public partial ref bytes.Buffer ΔBuffer { get; }
            }
            """, "Buffer");

        (string Name, string TypeName) hop = declaration.GetEmbeddedValueHopNames(compilation).Single();

        Assert.AreEqual($"{ShadowVarMarker}Buffer", hop.Name);
        Assert.AreEqual("bytes.Buffer", hop.TypeName);
    }

    [TestMethod]
    public void DeclaredValueEmbedFieldIsAValueHop()
    {
        (StructDeclarationSyntax declaration, Compilation compilation) = Parse(
            """
            public struct Inner { public int x; }
            partial struct Outer
            {
                /*embed*/ public Inner Inner;
                public int y;
            }
            """, "Outer");

        (string Name, string TypeName) hop = declaration.GetEmbeddedValueHopNames(compilation).Single();

        Assert.AreEqual("Inner", hop.Name);
        Assert.AreEqual("Inner", hop.TypeName);
        Assert.AreEqual(0, declaration.GetEmbeddedPointerHopNames(compilation).Count, "a value embed is not a pointer hop");
    }

    [TestMethod]
    public void DeclaredPointerEmbedFieldIsAPointerHop()
    {
        (StructDeclarationSyntax declaration, Compilation compilation) = Parse(
            """
            using go;
            public struct Inner { public int x; }
            partial struct Outer
            {
                /*embed*/ public ж<Inner> Inner;
            }
            """, "Outer");

        (string Name, string TypeName) hop = declaration.GetEmbeddedPointerHopNames(compilation).Single();

        Assert.AreEqual("Inner", hop.Name);
        Assert.AreEqual("Inner", hop.TypeName);
        Assert.AreEqual(0, declaration.GetEmbeddedValueHopNames(compilation).Count, "a pointer embed is not a value hop");
    }

    [TestMethod]
    public void InterfaceAndPredeclaredEmbedFieldsAreNotHops()
    {
        (StructDeclarationSyntax declaration, Compilation compilation) = Parse(
            """
            using go;
            public interface Reader { int Read(); }
            partial struct Outer
            {
                /*embed*/ public Reader Reader;
                /*embed*/ public int @int;
                /*embed*/ public ж<int> @byte;
            }
            """, "Outer");

        Assert.AreEqual(0, declaration.GetEmbeddedValueHopNames(compilation).Count, "an interface or predeclared embed promotes nothing");
        Assert.AreEqual(0, declaration.GetEmbeddedPointerHopNames(compilation).Count, "a pointer to a predeclared type promotes nothing");
    }

    [TestMethod]
    public void AnUnmarkedFieldNamedAfterItsTypeIsNotAHop()
    {
        (StructDeclarationSyntax declaration, Compilation compilation) = Parse(
            """
            public struct Inner { public int x; }
            partial struct Outer
            {
                public Inner Inner;
            }
            """, "Outer");

        Assert.AreEqual(0, declaration.GetEmbeddedValueHopNames(compilation).Count, "only the /*embed*/ mark makes a field an embed");
    }
}
