// NamedNumericBasicConversionTests.cs - Gbtc
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
/// Seat #26 (N2): Go converts a named numeric to any numeric type in one step, `int(k)` for a
/// `type Kind uint8`. The generated wrapper declares one explicit operator per basic numeric type, so the
/// converter writes `(nint)k` where it wrote `(nint)(uint8)k`. Read from the compiler's own verdict against
/// the real golib, and from the values the compiled casts return: each equals the underlying's C#
/// conversion, wrap-around and truncation included. The operators leave an implicit conversion and a cast
/// to the underlying binding as before, and a complex wrapper takes none.
/// </summary>
[TestClass]
public class NamedNumericBasicConversionTests
{
    private const string Path = @"C:\go2cs\src\core\ntest\ntest.cs";

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

    // The wrappers, and one cast per conversion the converter now writes in a single step. Each cast is
    // CS0030 without the operators: a narrowing, a sign change, a float to an integer, or golib's uintptr.
    private const string Declarations =
        """
            partial struct Kind /*num:uint8*/;
            partial struct Offset /*num:int64*/;
            partial struct Celsius /*num:float64*/;
            partial struct Addr /*num:uintptr*/;
            partial struct Phase /*num:complex128*/;

            public static int8 KindToInt8(uint8 v) => (int8)(Kind)v;
            public static uintptr KindToUintptr(uint8 v) => (uintptr)(Kind)v;
            public static uint64 OffsetToUint64(int64 v) => (uint64)(Offset)v;
            public static nint OffsetToNint(int64 v) => (nint)(Offset)v;
            public static int32 OffsetToInt32(int64 v) => (int32)(Offset)v;
            public static float32 OffsetToFloat32(int64 v) => (float32)(Offset)v;
            public static nint CelsiusToNint(float64 v) => (nint)(Celsius)v;
            public static int64 CelsiusToInt64(float64 v) => (int64)(Celsius)v;
            public static uint8 CelsiusToUint8(float64 v) => (uint8)(Celsius)v;
            public static nint AddrToNint(nuint v) => (nint)(Addr)v;
            public static uint64 AddrToUint64(nuint v) => (uint64)(Addr)v;
            public static float64 AddrToFloat64(nuint v) => (float64)(Addr)v;
            public static int32 AddrToInt32(nuint v) => (int32)(Addr)v;

            public static nint KindToNint(uint8 v) => (nint)(Kind)v;
            public static float64 KindToFloat64(uint8 v) => (float64)(Kind)v;
            public static int64 KindToInt64(uint8 v) => (int64)(Kind)v;
        """;

    // Bindings that held before the operators and must hold after: an implicit conversion to the
    // underlying and its C# widenings, a cast to the underlying, and uintptr's nuint bridge.
    private const string Unchanged =
        """
            public static uint8 KindImplicit(Kind k) { uint8 b = k; return b; }
            public static nint KindImplicitWiden(Kind k) { nint n = k; return n; }
            public static uint8 KindToUnderlying(Kind k) => (uint8)k;
            public static nuint AddrToNuint(Addr a) => (nuint)a;
            public static nuint AddrImplicitNuint(Addr a) { nuint n = a; return n; }
        """;

    private static string Package(string declarations) =>
        $$"""
        namespace go;

        [GoPackage("ntest")]
        public static partial class ntest_package
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

    private static (Compilation output, string generated, Diagnostic[] errors) Run(string source)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("ntest",
            [CSharpSyntaxTree.ParseText(source, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\ntest\obj\ntest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string generated = string.Join(Environment.NewLine, output.SyntaxTrees.Except(compilation.SyntaxTrees).Select(tree => tree.ToString()));
        Diagnostic[] errors = output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

        return (output, generated, errors);
    }

    [TestMethod]
    public void ANamedNumericConvertsToEveryBasicNumericInOneCast()
    {
        (_, _, Diagnostic[] errors) = Run(Package(Declarations + Unchanged));

        Assert.AreEqual(0, errors.Length, "every one-step cast compiles: " + string.Join("; ", errors.Take(5)));
    }

    [TestMethod]
    public void EachOneStepCastReturnsTheUnderlyingConversion()
    {
        (Compilation output, _, Diagnostic[] errors) = Run(Package(Declarations + Unchanged));
        Assert.AreEqual(0, errors.Length, string.Join("; ", errors.Take(5)));

        using MemoryStream image = new();
        Assert.IsTrue(output.Emit(image).Success, "the compilation emits");

        Type package = Assembly.Load(image.ToArray()).GetType("go.ntest_package")!;

        object Call(string method, object argument) => package.GetMethod(method)!.Invoke(null, [argument])!;

        Assert.AreEqual(unchecked((sbyte)200), Call("KindToInt8", (byte)200), "uint8 200 wraps to int8 -56");
        Assert.AreEqual((nuint)200, ((golib::go.uintptr)Call("KindToUintptr", (byte)200)).Value);
        Assert.AreEqual(ulong.MaxValue, Call("OffsetToUint64", -1L), "int64 -1 is uint64 max");
        Assert.AreEqual((nint)(-5), Call("OffsetToNint", -5L));
        Assert.AreEqual(unchecked((int)0x1_0000_0007L), Call("OffsetToInt32", 0x1_0000_0007L), "int64 truncates to its low 32 bits");
        Assert.AreEqual((float)123456789L, Call("OffsetToFloat32", 123456789L));
        Assert.AreEqual((nint)3, Call("CelsiusToNint", 3.9), "a float truncates toward zero");
        Assert.AreEqual(-3L, Call("CelsiusToInt64", -3.9));
        Assert.AreEqual((byte)7, Call("CelsiusToUint8", 7.5));
        Assert.AreEqual((nint)(-1), Call("AddrToNint", nuint.MaxValue));
        Assert.AreEqual(42UL, Call("AddrToUint64", (nuint)42));
        Assert.AreEqual(42.0, Call("AddrToFloat64", (nuint)42));
        Assert.AreEqual(unchecked((int)0x1_0000_0002UL), Call("AddrToInt32", (nuint)0x1_0000_0002UL));
        Assert.AreEqual((nint)250, Call("KindToNint", (byte)250));
        Assert.AreEqual(250.0, Call("KindToFloat64", (byte)250));
        Assert.AreEqual(250L, Call("KindToInt64", (byte)250));
    }

    [TestMethod]
    public void TheUnderlyingAndComplexTakeNoBasicOperator()
    {
        (_, string generated, _) = Run(Package(Declarations));

        Assert.IsFalse(generated.Contains("explicit operator uint8(Kind value)"), "the underlying keeps its implicit operator only");
        Assert.IsFalse(generated.Contains("explicit operator nuint(Addr value)"), "uintptr's nuint bridge is the conversion to nuint");
        Assert.IsFalse(generated.Contains("explicit operator uintptr(Addr value)"), "the underlying uintptr keeps its implicit operator only");
        Assert.IsFalse(generated.Contains("explicit operator") && generated.Split('\n').Any(line => line.Contains("explicit operator") && line.Contains("(Phase value)")),
            "a complex wrapper takes no basic numeric operator");
        Assert.IsTrue(generated.Contains("public static explicit operator nint(Kind value) => (nint)value.m_value;"), "a basic target reads the underlying's own cast");
    }
}
