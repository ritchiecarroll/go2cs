// GoTypeDefinitionCommentTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Guards C (plan 5.3): a defined type's underlying-type definition moves from the <c>[GoType("…")]</c>
/// argument into a comment right after the type's name, <c>partial struct Duration /*num:int64*/;</c>.
/// The generator reads the comment exactly as it read the argument, and its generated part re-emits
/// <c>[GoType("num:int64")]</c> so <c>GoTypeAttribute.Definition</c> reads as before. A comment anywhere
/// else is not a definition. And a DELETED comment fails the build: the defined number type falls back
/// to a plain struct and loses its arithmetic, read here from the compiler's own verdict against the
/// real golib.
/// </summary>
[TestClass]
public class GoTypeDefinitionCommentTests
{
    private const string Path = @"C:\go2cs\src\core\ctest\ctest.cs";

    private static string Package(string declarations) =>
        $$"""
        namespace go;

        [GoPackage("ctest")]
        public static partial class ctest_package
        {
        {{declarations}}
        }
        """;

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    // The global usings a converted project gets from the converter's csproj template (<Using> items),
    // which the generated code relies on (int64, the builtin statics, ...).
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
        global using float32 = System.Single;
        global using float64 = System.Double;
        global using complex128 = System.Numerics.Complex;
        global using rune = System.Int32;
        global using GoBigConst = System.Numerics.BigInteger;
        global using GoTagAttribute = System.ComponentModel.DescriptionAttribute;
        global using GoInitAttribute = System.Runtime.CompilerServices.ModuleInitializerAttribute;
        """;

    private static (string[] generated, Diagnostic[] errors) Run(string source)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("ctest",
            [CSharpSyntaxTree.ParseText(source, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\ctest\obj\ctest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string[] generated = output.SyntaxTrees.Except(compilation.SyntaxTrees).Select(tree => tree.ToString()).ToArray();
        Diagnostic[] errors = output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

        return (generated, errors);
    }

    [TestMethod]
    [DataRow("num:int64", "struct", "Duration")]
    [DataRow("[]byte", "struct", "Bytes")]
    [DataRow("map[@string]nint", "struct", "Counts")]
    [DataRow("dyn", "struct", "Lifted")]
    [DataRow("ж<nint>", "class", "IntPtr")]
    public void ACommentDefinitionGeneratesWhatTheArgumentDid(string definition, string kind, string name)
    {
        string[] fromAttribute = Run(Package($"    [GoType(\"{definition}\")] partial {kind} {name};")).generated;
        string[] fromComment = Run(Package($"    partial {kind} {name} /*{definition}*/;")).generated;

        Assert.AreEqual(1, fromAttribute.Length, $"the attribute form generates one part for {name}");
        Assert.AreEqual(1, fromComment.Length, $"the comment form generates one part for {name}");
        StringAssert.Contains(fromComment[0], $"[GoType(\"{definition}\")] ", "the generated part re-emits the definition as the attribute's argument");
        Assert.AreEqual(fromAttribute[0], fromComment[0].Replace($"[GoType(\"{definition}\")] ", ""),
            "moving the definition into the comment changes the generated output by the re-emitted attribute only");
    }

    [TestMethod]
    public void OnlyACommentRightAfterTheNameIsADefinition()
    {
        string[] elsewhere = Run(Package("""
                /*num:int64*/ partial struct Before;
                partial struct After; /*num:int64*/
                // num:int64
                partial struct LineComment;
            """)).generated;

        Assert.IsFalse(elsewhere.Any(text => text.Contains("[GoType(\"num:int64\")]")), "a comment before the name, after the semicolon or on its own line is not a definition");
        Assert.IsFalse(Run(Package("    partial class NoDefinition;")).generated.Any(), "a class without a definition comment is not a Go type");
    }

    [TestMethod]
    public void ADeletedDefinitionCommentFailsTheBuild()
    {
        const string use = "\n    public static Duration Twice(Duration d) => d + d;";

        Diagnostic[] withComment = Run(Package("    partial struct Duration /*num:int64*/;" + use)).errors;
        Diagnostic[] withoutComment = Run(Package("    partial struct Duration;" + use)).errors;

        Assert.AreEqual(0, withComment.Length, "with its definition comment the defined number type compiles: " + string.Join("; ", withComment.Take(3)));
        Assert.IsTrue(withoutComment.Any(error => error.Id == "CS0019"),
            "with the comment deleted the type is a plain struct with no arithmetic, so the build fails (CS0019): " + string.Join("; ", withoutComment.Take(3)));
    }
}
