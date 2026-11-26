// ValueAssignFieldNameTests.cs - Gbtc
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
/// The generated <c>AssignFrom</c> of a <c>[GoValueClone]</c> struct (Go's whole assignment into an existing location,
/// docs/phase4/DESIGN-array-reassign-in-place.md §8) must compile whatever the struct's fields are called. A cgo struct
/// with a field named <c>from</c> (behavioral CgoPseudoPackageTypes' <c>struct_segment</c>) shadowed the member's local
/// of the same name, so <c>from = own.from</c> wrote the local and <c>GoValueAssign.Into</c> could not infer (CS0411).
/// The member writes every field as <c>this.f</c> and names its locals with the converter's temp marker.
/// </summary>
[TestClass]
public class ValueAssignFieldNameTests
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
        global using uint8 = System.Byte;
        """;

    [TestMethod]
    public void AssignFromCompilesOverFieldsNamedLikeItsLocals()
    {
        const string source =
            """
            namespace go;

            [GoPackage("atest")]
            public static partial class atest_package
            {
                [GoType] partial struct segment
                {
                    internal array<uint8> from = new(2);
                    internal array<uint8> own = new(2);
                    internal array<uint8> source = new(2);
                    internal array<uint8> target = new(2);
                }

                [GoValueClone("from", "own", "source", "target")] internal partial struct segment {}
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

        SyntaxTree? generated = output.SyntaxTrees.FirstOrDefault(tree => tree.FilePath.EndsWith("segment.g.cs", StringComparison.Ordinal));

        Assert.IsNotNull(generated, "the generator emitted no part for segment");
        StringAssert.Contains(generated!.ToString(), "AssignFrom(in segment source)", "no AssignFrom was generated:\n" + generated);

        string[] errors = output.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Location.SourceTree == generated)
            .Select(diagnostic => diagnostic.ToString())
            .ToArray();

        Assert.AreEqual(0, errors.Length, "the generated part does not compile:\n" + string.Join("\n", errors) + "\n\n" + generated);
    }
}
