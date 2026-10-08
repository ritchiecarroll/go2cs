// ChanDirDefinitionCommentTests.cs - Gbtc
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Guards face-lift row 3 (docs/PLAN-marker-comment-parity.md §11, owner ruling 2026-10-07): a DIRECTIONAL
/// defined channel type carries its direction in its definition comment, spelled as Go spells it,
/// <c>partial struct IntChanRecv /*&lt;-chan nint*/;</c>, instead of <c>[GoType("chan nint")]</c> beside
/// <c>[GoChanDir(GoChanDir.Recv)]</c>. The generator reads the comment, builds the Channel wrapper from
/// <c>chan nint</c> exactly as before, and re-emits BOTH attributes on its generated part, so reflection's
/// <c>ChanDir()</c> (golib reads <see cref="golib::go.GoChanDirAttribute"/> off the type) answers as before.
/// </summary>
[TestClass]
public class ChanDirDefinitionCommentTests
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

    private const string TemplateUsings =
        """
        global using static go.builtin;
        global using System;
        global using System.Numerics;
        global using any = System.Object;
        global using int64 = System.Int64;
        """;

    private static (string[] generated, Diagnostic[] errors, Compilation output) Run(string source)
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

        return (generated, errors, output);
    }

    [TestMethod]
    [DataRow("<-chan nint", "Recv", "IntChanRecv")]
    [DataRow("chan<- nint", "Send", "IntChanSend")]
    public void ADirectionalCommentGeneratesWhatTheTwoAttributesDid(string definition, string direction, string name)
    {
        string reemitted = $"[GoType(\"chan nint\")] [GoChanDir(GoChanDir.{direction})] ";
        string[] fromAttributes = Run(Package($"    [GoType(\"chan nint\")] [GoChanDir(GoChanDir.{direction})] partial struct {name};")).generated;
        (string[] fromComment, Diagnostic[] errors, Compilation output) = Run(Package($"    partial struct {name} /*{definition}*/;"));

        Assert.AreEqual(1, fromAttributes.Length, $"the attribute form generates one part for {name}");
        Assert.AreEqual(1, fromComment.Length, $"the comment form generates one part for {name}");
        StringAssert.Contains(fromComment[0], reemitted, "the generated part re-emits the channel definition and its direction");
        Assert.AreEqual(fromAttributes[0], fromComment[0].Replace(reemitted, ""),
            "moving the direction into the comment changes the generated output by the re-emitted attributes only");
        Assert.AreEqual(0, errors.Length, "the comment form compiles: " + string.Join("; ", errors.Take(3)));

        // Reflection's half: the TYPE carries the direction golib reads back.
        using MemoryStream image = new();
        Assert.IsTrue(output.Emit(image).Success, "the comment form emits an assembly");
        Type type = Assembly.Load(image.ToArray()).GetType($"go.ctest_package+{name}", throwOnError: true)!;
        golib::go.GoChanDirAttribute? chanDir = type.GetCustomAttribute<golib::go.GoChanDirAttribute>();

        Assert.IsNotNull(chanDir, $"{name} carries no GoChanDirAttribute, so reflect would read it as bidirectional");
        CollectionAssert.AreEqual(new[] { Enum.Parse<golib::go.GoChanDir>(direction) }, chanDir.DirChain, $"{name}'s direction chain");
    }

    [TestMethod]
    public void ABidirectionalCommentStampsNoDirection()
    {
        string[] generated = Run(Package("    partial struct Pipe /*chan nint*/;")).generated;

        Assert.AreEqual(1, generated.Length);
        StringAssert.Contains(generated[0], "[GoType(\"chan nint\")] ");
        Assert.IsFalse(generated[0].Contains("GoChanDir"), "a bidirectional channel stamps no direction, as before");
    }
}
