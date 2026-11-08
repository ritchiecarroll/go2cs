// NamedWrapperValueMemberTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// A Go type NAMED <c>Value</c> (<c>type Value []byte</c>, go-json-experiment/json's <c>jsontext.Value</c>)
/// would get a generated underlying-value property also named <c>Value</c>, and C# forbids a member
/// that shares its type's name (CS0542). Such a wrapper declares the property as
/// <see cref="Symbols.NamedWrapperValue"/> instead, and every other wrapper is unchanged.
/// </summary>
/// <remarks>
/// <para>
/// The arms compile the generated parts against the real golib, so "it builds" is the compiler's
/// verdict, not a reading of the text. The control arm generates the same definition under another
/// name and requires the two texts to be identical once the type's name is normalized, so the clash
/// changes the property's name and nothing else.
/// </para>
/// <para>
/// The implicit-conversion arm covers the other generator that names the property: a record between
/// two named numerics reads <c>src.&lt;property&gt;</c> on its source.
/// </para>
/// </remarks>
[TestClass]
public class NamedWrapperValueMemberTests
{
    private const string Path = @"C:\go2cs\src\core\ctest\ctest.cs";

    private static string Package(string declarations, string records = "") =>
        $$"""
        {{records}}

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

    private static (string[] generated, Diagnostic[] errors) Run(string source, params ISourceGenerator[] generators)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("ctest",
            [CSharpSyntaxTree.ParseText(source, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\ctest\obj\ctest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(generators)
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string[] generated = output.SyntaxTrees.Except(compilation.SyntaxTrees)
            .Where(tree => !tree.FilePath.EndsWith(".GoTypeRegistry.g.cs", StringComparison.Ordinal))
            .Select(tree => tree.ToString()).ToArray();
        Diagnostic[] errors = output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

        return (generated, errors);
    }

    private static string Describe(Diagnostic[] errors) => string.Join("; ", errors.Select(error => $"{error.Id} {error.GetMessage()}"));

    /// <summary>
    /// Every wrapper class that declares the property (slice, byte slice, map, array, numeric, chan): the
    /// type named Value compiles, declares <see cref="Symbols.NamedWrapperValue"/>, and its text is the
    /// control's text with the property renamed and nothing else changed.
    /// </summary>
    [TestMethod]
    [DataRow("[]byte")]
    [DataRow("[]nint")]
    [DataRow("map[@string, nint]")]
    [DataRow("[3]nint")]
    [DataRow("num:nint")]
    [DataRow("chan nint")]
    public void AWrapperNamedValueRenamesOnlyItsValueProperty(string definition)
    {
        (string[] clash, Diagnostic[] clashErrors) = Run(Package($"    partial struct Value /*{definition}*/;"), new TypeGenerator());
        (string[] control, Diagnostic[] controlErrors) = Run(Package($"    partial struct Zalue /*{definition}*/;"), new TypeGenerator());

        Assert.AreEqual(0, clashErrors.Length, $"the wrapper named Value must compile: {Describe(clashErrors)}");
        Assert.AreEqual(0, controlErrors.Length, $"control: {Describe(controlErrors)}");

        string clashText = string.Join("\n", clash);
        string controlText = string.Join("\n", control);

        StringAssert.Contains(clashText, Symbols.NamedWrapperValue, "the clashing wrapper declares the door");
        Assert.IsFalse(controlText.Contains(Symbols.NamedWrapperValue), "a non-clashing wrapper keeps the plain Value");

        // Give the control the clash's type name, and the clash its plain property back: the two texts then
        // agree exactly, so the clash changed the property's name and nothing else.
        string controlAsValue = Regex.Replace(controlText, @"\bZalue\b", "Value");
        string clashWithPlainProperty = clashText.Replace(Symbols.NamedWrapperValue, "Value");

        Assert.AreEqual(controlAsValue, clashWithPlainProperty, "the clash must change the property's name and nothing else");
    }

    /// <summary>
    /// The implicit-conversion record between two named numerics reads <c>src.&lt;property&gt;</c> on its
    /// source; with the source named Value it reads the door, and the pair compiles.
    /// </summary>
    [TestMethod]
    public void AnImplicitConversionFromAWrapperNamedValueReadsTheDoor()
    {
        string source = Package(
            "    partial struct Value /*num:nint*/;\n    partial struct Other /*num:nint*/;",
            "[assembly: go.GoImplicitConv<go.ctest_package.Value, go.ctest_package.Other>(ValueType = \"nint\")]");

        (string[] generated, Diagnostic[] errors) = Run(source, new TypeGenerator(), new ImplicitConvGenerator());

        Assert.AreEqual(0, errors.Length, $"the conversion must compile: {Describe(errors)}");
        StringAssert.Contains(string.Join("\n", generated), $"src.{Symbols.NamedWrapperValue}");
    }
}
