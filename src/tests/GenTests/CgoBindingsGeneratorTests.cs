// CgoBindingsGeneratorTests.cs - Gbtc
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
/// The cgo bindings the converter writes compile WITH go2cs-gen as an analyzer, as a converted cgo project compiles them
/// (cgo Phase 2 increment 5a, docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.20). GolibTests compiles the same bindings
/// goldens (GoCgoWrappers.&lt;os&gt;.golden.cs) with no generator at all, so a declaration go2cs-gen misreads is in no gate
/// there: the bindings' mirror structs (<c>_Ctype_struct_tmᴍ</c>) sit directly in the pseudo-package class, a
/// <c>*_package</c> class, and the converted-Go-type rule took each for a Go type and generated a second part of it (CS0260,
/// found only by building a converted package). This runs every go2cs-gen generator over each OS's golden, against the
/// real golib, and holds that the generators add no error to it. The golden's hand-declared C types stand in for the
/// converter's own here; they are given a hand-written companion's file name, as the rule's own exclusion reads it, and
/// their parts of the pseudo-package class carry no accessibility, as none of the converter's parts do (go2cs-gen
/// declares the class public, and GolibTests' own internal would conflict, CS0262).
/// </summary>
[TestClass]
public class CgoBindingsGeneratorTests
{
    // The global usings a converted project gets from the converter's csproj template (<Using> items).
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

    private static DirectoryInfo GolibTests()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            DirectoryInfo candidate = new(Path.Combine(directory.FullName, "src", "tests", "GolibTests"));

            if (candidate.Exists)
                return candidate;
        }

        throw new DirectoryNotFoundException("src/tests/GolibTests not found above " + AppContext.BaseDirectory);
    }

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    // Every generator go2cs-gen ships, as an analyzer reference runs them.
    private static ISourceGenerator[] Generators() =>
        typeof(TypeGenerator).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && type.GetCustomAttribute<GeneratorAttribute>() is not null)
            .Select(type => Activator.CreateInstance(type) switch
            {
                IIncrementalGenerator incremental => incremental.AsSourceGenerator(),
                ISourceGenerator generator => generator,
                _ => throw new InvalidOperationException($"{type.Name} is marked [Generator] and is no generator")
            })
            .ToArray();

    // A hand-declared stand-in for the converter's C types, its pseudo-package part without GolibTests' accessibility.
    private static string StandIn(DirectoryInfo golibTests, string name) =>
        File.ReadAllText(Path.Combine(golibTests.FullName, name))
            .Replace("internal static partial class cgotestꓸC_package", "static partial class cgotestꓸC_package");

    private static string[] Errors(Compilation compilation) =>
        compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Select(diagnostic => $"{diagnostic.Id} {diagnostic.Location.GetLineSpan()}: {diagnostic.GetMessage()}")
            .ToArray();

    [TestMethod]
    [DataRow("linux", "posix")]
    [DataRow("darwin", "posix")]
    [DataRow("windows", "windows")]
    public void TheGeneratorsAddNoErrorToTheBindings(string goos, string longFlavor)
    {
        DirectoryInfo golibTests = GolibTests();
        string golden = File.ReadAllText(Path.Combine(golibTests.FullName, $"GoCgoWrappers.{goos}.golden.cs"));
        CSharpParseOptions options = new(LanguageVersion.Preview);

        Assert.IsTrue(golden.Contains("ᴍ"), $"the {goos} golden declares a mirror, so this arm reaches the rule that misread one");

        // The bindings under a converted file's name, the hand-declared C types under a companion's.
        SyntaxTree[] trees =
        [
            CSharpSyntaxTree.ParseText(golden, options, path: $@"C:\go2cs\src\cgotest\{goos}-amd64\cgotest.cgo.cs"),
            CSharpSyntaxTree.ParseText(StandIn(golibTests, "GoCgoWrapperTypes.cs"), options, path: @"C:\go2cs\src\cgotest\cgotypes_impl.cs"),
            CSharpSyntaxTree.ParseText(StandIn(golibTests, $"GoCgoWrapperTypes.{longFlavor}.cs"), options, path: @"C:\go2cs\src\cgotest\cgotypes_long_impl.cs"),
            CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\cgotest\obj\cgotest.GlobalUsings.g.cs")
        ];

        CSharpCompilation compilation = CSharpCompilation.Create("cgotest", trees, References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(Generators(), parseOptions: options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        // The golden names runtime's and syscall's members, which this compilation does not reference, so it has errors
        // of its own; the generators must add none.
        HashSet<string> before = [.. Errors(compilation)];
        string[] added = Errors(output).Where(error => !before.Contains(error)).ToArray();

        Assert.AreEqual(0, added.Length, $"go2cs-gen adds {added.Length} error(s) to the {goos} bindings:\n{string.Join("\n", added.Take(20))}");
    }
}
