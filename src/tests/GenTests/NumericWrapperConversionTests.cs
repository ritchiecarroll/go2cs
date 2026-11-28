// NumericWrapperConversionTests.cs - Gbtc
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
/// go2cs-gen gives every named numeric wrapper (<c>[GoType("num:…")]</c>) golib's <c>IGoNumericWrapper&lt;T&gt;</c>, the
/// members golib's ConvertToType, ConvertToUInt64, ConvertToFloat64 and ConvertToFloat32 reach a named type argument
/// through instead of reflection. These rows compile a wrapper of each kind, run golib's conversions on it and compare
/// with Go's answers (behavioral NamedNumericOperatorConstraint runs the same rows from Go source). A complex kind takes
/// no interface: Go converts no integer or float to it.
/// </summary>
[TestClass]
public class NumericWrapperConversionTests
{
    private const string Package =
        """
        namespace go;

        [GoPackage("ncast")]
        public static partial class ncast_package
        {
            [GoType("num:uint8")] partial struct small;
            [GoType("num:int32")] partial struct offset;
            [GoType("num:uint64")] partial struct stringID;
            [GoType("num:uintptr")] partial struct handle;
            [GoType("num:float64")] partial struct celsius;
            [GoType("num:float32")] partial struct gauge;
            [GoType("num:complex128")] partial struct phase;

            public static string Probe() => string.Join(" ",
                (uint8)ConvertToType<small>(300UL),
                (int32)ConvertToType<offset>((1UL << 32) + 5),
                (uint64)ConvertToType<stringID>(ulong.MaxValue),
                ((uintptr)ConvertToType<handle>(1UL << 40)).Value,
                (uint8)ConvertToType<small>(-1L),
                (int32)ConvertToType<offset>(-7.9),
                (uint8)ConvertToType<small>(200.7),
                (float64)ConvertToType<celsius>(3L),
                (float64)ConvertToType<celsius>(-2L),
                (float64)ConvertToType<celsius>(ulong.MaxValue),
                (float32)ConvertToType<gauge>(ulong.MaxValue),
                (float64)ConvertToType<celsius>(0.25),
                (float32)ConvertToType<gauge>(16777217.0),
                ConvertToUInt64(new offset(-1)),
                ConvertToUInt64(new stringID(ulong.MaxValue)),
                ConvertToUInt64(new handle(new uintptr(7))),
                ConvertToUInt64(new celsius(2.9)),
                ConvertToUInt64(new gauge(7.5f)),
                ConvertToFloat64(new celsius(21.5)),
                ConvertToFloat64(new offset(-3)),
                ConvertToFloat32(new offset(16777217)),
                ConvertToFloat32(new gauge(1.5f)));
        }
        """;

    private const string TemplateUsings =
        """
        global using static go.builtin;
        global using System;
        global using System.Numerics;
        global using int8 = System.SByte;
        global using int16 = System.Int16;
        global using int32 = System.Int32;
        global using int64 = System.Int64;
        global using uint8 = System.Byte;
        global using uint16 = System.UInt16;
        global using uint32 = System.UInt32;
        global using uint64 = System.UInt64;
        global using float32 = System.Single;
        global using float64 = System.Double;
        global using complex128 = System.Numerics.Complex;
        """;

    private static (Compilation Output, Diagnostic[] Errors) Generated()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        IEnumerable<MetadataReference> references = trusted.Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));

        CSharpCompilation compilation = CSharpCompilation.Create("ncast",
            [CSharpSyntaxTree.ParseText(Package, options, path: @"C:\go2cs\src\core\ncast\ncast.cs"),
             CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\ncast\obj\ncast.GlobalUsings.g.cs")],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        return (output, output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray());
    }

    [TestMethod]
    public void EveryRealNumericKindImplementsTheWrapperAndComplexDoesNot()
    {
        (Compilation output, Diagnostic[] errors) = Generated();

        Assert.AreEqual(0, errors.Length, "the package compiles: " + string.Join("; ", errors.Take(5)));

        INamedTypeSymbol package = output.GetTypeByMetadataName("go.ncast_package")!;

        foreach (string name in new[] { "small", "offset", "stringID", "handle", "celsius", "gauge", "phase" })
        {
            INamedTypeSymbol type = package.GetTypeMembers(name).Single();
            bool implements = type.AllInterfaces.Any(contract => contract.ToDisplayString() == $"go.golib.IGoNumericWrapper<go.ncast_package.{name}>");

            Assert.AreEqual(name != "phase", implements, $"{name} implements IGoNumericWrapper: {implements}");
        }
    }

    [TestMethod]
    public void GolibsConversionsGiveGosAnswersThroughTheGeneratedMembers()
    {
        (Compilation output, Diagnostic[] errors) = Generated();

        Assert.AreEqual(0, errors.Length, "the package compiles: " + string.Join("; ", errors.Take(5)));

        using MemoryStream image = new();
        Assert.IsTrue(output.Emit(image).Success, "the package emits an assembly");

        System.Reflection.MethodInfo probe = Assembly.Load(image.ToArray()).GetType("go.ncast_package", throwOnError: true)!.GetMethod("Probe")!;

        // Go: small(300) 44, offset(1<<32+5) 5, stringID(1<<64-1) every bit, handle(1<<40); small(int64(-1)) 255;
        // offset(-7.9) -7, small(200.7) 200; celsius(3), celsius(int64(-2)) -2, celsius(uint64(1<<64-1)) and
        // gauge(uint64(1<<64-1)) by the source's signedness, celsius(0.25); gauge(1<<24+1) rounds once to 1<<24;
        // uint64(offset(-1)) sign-extends, uint64(stringID max), uint64(handle(7)), uint64(celsius(2.9)) 2,
        // uint64(gauge(7.5)) 7; float64(celsius(21.5)), float64(offset(-3)), float32(offset(1<<24+1)) rounds once,
        // float32(gauge(1.5)).
        Assert.AreEqual(
            "44 5 18446744073709551615 1099511627776 255 -7 200 3 -2 1.8446744073709552E+19 1.8446744E+19 0.25 16777216 " +
            "18446744073709551615 18446744073709551615 7 2 7 21.5 -3 16777216 1.5",
            probe.Invoke(null, null));
    }
}
