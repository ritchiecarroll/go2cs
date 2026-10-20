// NamedArrayGoCloneTests.cs - Gbtc
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
/// A named ARRAY type whose Go declaration carries its own <c>Clone</c> method (Go 1.26 hash/fnv's
/// <c>func (d *sum128) Clone() (hash.Cloner, error)</c> over <c>type sum128 [2]uint64</c>) gets no public
/// <c>Clone()</c> from the array template: an instance member is bound before the extension method Go's Clone
/// becomes, so the wrapper's copy shadowed Go's method and RecvGenerator's forwarder <c>return d.Clone();</c>
/// bound the array copy (CS0029). The copy is <c>ΔClone()</c> alone for such a type; the converter spells its
/// copy sites that way (valueCloneSuffix). A named array without a Go Clone keeps its public <c>Clone()</c>.
/// </summary>
[TestClass]
public class NamedArrayGoCloneTests
{
    private const string FilePath = @"C:\go2cs\src\core\atest\atest.cs";

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
        global using uint64 = System.UInt64;
        """;

    private static string Generated(string declarations, string typeName)
    {
        string source =
            $$"""
            namespace go;

            [GoPackage("atest")]
            public static partial class atest_package
            {
            {{declarations}}
            }
            """;

        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("atest",
            [CSharpSyntaxTree.ParseText(source, options, path: FilePath), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\atest\obj\atest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        return string.Join("\n", output.SyntaxTrees.Except(compilation.SyntaxTrees)
            .Where(tree => tree.FilePath.EndsWith($"{typeName}.g.cs", StringComparison.Ordinal))
            .Select(tree => tree.ToString()));
    }

    [TestMethod]
    public void ANamedArrayDeclaringGoCloneCopiesThroughValueCloneOnly()
    {
        string withGoClone = Generated(
            """
                [GoType("[2]uint64")] partial struct sum;

                internal static (any, error) Clone(this ref sum d) => default;
            """, "sum");

        Assert.IsTrue(withGoClone.Length > 0, "the generator emitted no part for sum");
        Assert.IsFalse(withGoClone.Contains("public sum Clone()", StringComparison.Ordinal),
            "the wrapper still declares a public Clone() that shadows the Go method:\n" + withGoClone);
        StringAssert.Contains(withGoClone, "public sum ΔClone() => new sum(Value.Clone());", "the by-value copy is not ΔClone:\n" + withGoClone);
        StringAssert.Contains(withGoClone, "object global::System.ICloneable.Clone() => ΔClone();", "ICloneable does not reach the copy:\n" + withGoClone);

        // CONTROL: no Go Clone, the public Clone() stays, as every copy site in the corpus spells it.
        string plain = Generated("""    [GoType("[2]uint64")] partial struct plain;""", "plain");

        StringAssert.Contains(plain, "public plain Clone() => new plain(Value.Clone());", "a named array without a Go Clone lost its public Clone():\n" + plain);
    }
}
