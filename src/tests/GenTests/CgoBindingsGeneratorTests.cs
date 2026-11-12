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

    // A converted program that converts between C numeric types, as the converter writes it: `C.ushort(d)` with d a
    // C.DWORD, and a Go type `count` to and from C.DWORD. The files are the converter's own output for that program,
    // comments removed. Its conversion records name C types, which live in the pseudo-package class.
    private const string ConversionsPackageInfo =
        """
        using go;
        using static go.example.com.main_package;
        using C = go.example.com.mainꓸC_package;

        [assembly: GoImplicitConv<C.@ushort, C.DWORD>(Inverted = false, ValueType = "uint32")]
        [assembly: GoImplicitConv<count, C.DWORD>(Inverted = true, ValueType = "uint32")]

        namespace go.example.com;

        [GoPackage("main", ImportPath = "main")]
        public static partial class main_package
        {
            internal partial struct count {}
        }

        [GoPackage("main", ImportPath = "main")]
        partial class mainꓸC_package
        {
            [GoLocalName("_Ctype_int")] internal partial struct @int {}
            [GoLocalName("_Ctype_ulong")] internal partial struct @ulong {}
            [GoLocalName("_Ctype_ushort")] internal partial struct @ushort {}
            [GoLocalName("_Ctype_DWORD")] internal partial struct DWORD {}
        }
        """;

    private const string ConversionsTypes =
        """
        global using Cꓸsize_t = go.example.com.mainꓸC_package.@ulong;

        namespace go.example.com;

        using C = mainꓸC_package;

        partial class main_package {
        }

        partial class mainꓸC_package {

        partial struct @int /*num:int32*/;
        partial struct @ulong /*num:uint64*/;
        partial struct @ushort /*num:uint16*/;
        internal partial struct DWORD /*num:uint32*/;

        }
        """;

    private const string ConversionsProgram =
        """
        namespace go.example.com;

        using C = mainꓸC_package;

        partial class main_package {

        partial struct count /*num:uint32*/;

        internal static void main() {
            var n = (C.@int)7;
            var size = (Cꓸsize_t)(uint64)n;
            C.DWORD d = 70000;
            var narrow = (C.@ushort)(uint16)d;
            var c = (count)(uint32)d;
            var back = (C.DWORD)(uint32)c;
            println((uint64)size, (uint16)narrow, (uint32)c, (uint32)back);
        }

        }
        """;

    /// <summary>
    /// A conversion between two C numeric types, and between a C type and a Go one, compiles. The implicit-conversion
    /// generator hosted each operator in a part of the source type declared inside the PACKAGE class, where a C type is
    /// not: it declared a second, empty <c>main_package.@ushort</c>, and the build failed on CS1061 (first read by
    /// building a converted program for cgo Phase 2's exit gate, docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.22). A C
    /// type named from the package class by its bare name does not resolve there either.
    /// </summary>
    [TestMethod]
    public void AConversionBetweenCTypesCompiles()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        SyntaxTree[] trees =
        [
            CSharpSyntaxTree.ParseText(ConversionsPackageInfo, options, path: @"C:\go2cs\src\conversions\package_info.cs"),
            CSharpSyntaxTree.ParseText(ConversionsTypes, options, path: @"C:\go2cs\src\conversions\cgo_gotypes.cs"),
            CSharpSyntaxTree.ParseText(ConversionsProgram, options, path: @"C:\go2cs\src\conversions\main.cs"),
            CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\conversions\obj\conversions.GlobalUsings.g.cs")
        ];

        CSharpCompilation compilation = CSharpCompilation.Create("conversions", trees, References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(Generators(), parseOptions: options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string[] errors = Errors(output);

        Assert.AreEqual(0, errors.Length, $"the converted program has {errors.Length} error(s) with go2cs-gen run over it:\n{string.Join("\n", errors.Take(20))}");

        // The C-to-C operator is a member of the C type itself, in the class that holds it.
        INamedTypeSymbol narrow = output.GetTypeByMetadataName("go.example.com.mainꓸC_package+ushort")!;

        Assert.IsNotNull(narrow, "the pseudo-package class holds C.ushort");
        Assert.IsTrue(narrow.GetMembers(WellKnownMemberNames.ImplicitConversionName).Any(), "C.ushort declares the conversion from C.DWORD");
        Assert.IsNull(output.GetTypeByMetadataName("go.example.com.main_package+ushort"), "no second ushort is declared in the package class");
    }
}
