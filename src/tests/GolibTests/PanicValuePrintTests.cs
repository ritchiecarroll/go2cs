#nullable enable

using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// The panic VALUE as Go's crash report prints it (runtime/error.go's printpanicval): a basic value through the
/// runtime printer (floats and complexes in strconv's shortest 'g' form), a named basic type as <c>T(v)</c>
/// (printanycustomtype), and a string, or the Error()/String() text preprintpanics substitutes, with every newline
/// followed by a TAB (printindented). Every expected text is go1.26.10's own `panic(v)` output (linux/amd64,
/// measured 2026-10-24). PanicValueTextTests pins the substitution itself.
/// </summary>
[TestClass]
public class PanicValuePrintTests
{
#pragma warning disable CS8981 // Go's own lower-case names
    // Stands in for the converted `main` package, so GoTypeName qualifies these as main.T.
    private static class main_package
    {
        // The shape go2cs-gen emits for `type T <basic>`: one m_value field and the conversions.
        [GoType("num:int")] public readonly struct myInt(nint value) { private readonly nint m_value = value; public static implicit operator myInt(nint value) => new(value); }
        [GoType("num:uint8")] public readonly struct myU8(byte value) { private readonly byte m_value = value; public static implicit operator myU8(byte value) => new(value); }
        [GoType("num:uintptr")] public readonly struct myUintptr(uintptr value) { private readonly uintptr m_value = value; public static implicit operator myUintptr(uintptr value) => new(value); }
        [GoType("num:float64")] public readonly struct myFloat(double value) { private readonly double m_value = value; public static implicit operator myFloat(double value) => new(value); }
        [GoType("num:float32")] public readonly struct myF32(float value) { private readonly float m_value = value; public static implicit operator myF32(float value) => new(value); }
        [GoType("num:complex128")] public readonly struct myComplex(Complex value) { private readonly Complex m_value = value; public static implicit operator myComplex(Complex value) => new(value); }
        [GoType("bool")] public readonly struct myBool(bool value) { private readonly bool m_value = value; public static implicit operator myBool(bool value) => new(value); }
        [GoType("@string")] public readonly struct myStr(@string value) { private readonly @string m_value = value; public static implicit operator myStr(@string value) => new(value); }
    }
#pragma warning restore CS8981

    private static void AssertText(string expected, object? value, string name) =>
        Assert.AreEqual(expected, PanicException.PanicText(value), name);

    [TestMethod]
    public void BasicValuesPrintAsTheRuntimePrinterDoes()
    {
        AssertText("true", true, "true");
        AssertText("false", false, "false");
        AssertText("-5", (sbyte)-5, "int8");
        AssertText("18446744073709551615", ulong.MaxValue, "uint64");
        AssertText("42", (uintptr)(nuint)42, "uintptr");
        AssertText("-7", (nint)(-7), "int");
        AssertText("120", (int)'x', "rune");
        AssertText("200", (byte)200, "byte");
    }

    [TestMethod]
    public void FloatsPrintInTheShortestGForm()
    {
        AssertText("1.5", 1.5, "f1.5");
        AssertText("0.25", 0.25f, "f32");
        AssertText("1.234567e+06", 1234567.0, "f1234567");
        AssertText("1e+21", 1e21, "f1e21");
        AssertText("1e-07", 1e-7, "f1e-7");
        AssertText("100000", 100000.0, "f100000");
        AssertText("1e+06", 1e6, "f1e6");
        AssertText("-0", -0.0, "fneg0");
        AssertText("NaN", double.NaN, "fnan");
        AssertText("+Inf", double.PositiveInfinity, "finf");
        AssertText("-Inf", double.NegativeInfinity, "fninf");
        AssertText("0.33333334", (float)(1.0 / 3.0), "f32third");
        AssertText("0.3333333333333333", 1.0 / 3.0, "fthird");
        AssertText("1.2345678901234568e+17", 123456789012345678.0, "fbig");
        AssertText("0.0001", 0.0001, "f0.0001");
        AssertText("1e-05", 0.00001, "f0.00001");
    }

    [TestMethod]
    public void ComplexesPrintParenthesizedWithASignedImaginaryPart()
    {
        AssertText("(1+2i)", new Complex(1, 2), "c128");
        AssertText("(0.5-1i)", new complex64(0.5f, -1f), "c64");
        AssertText("(+Inf+NaNi)", new Complex(double.PositiveInfinity, double.NaN), "cinfnan");
        AssertText("(0+0i)", new Complex(0, 0), "c0");
    }

    [TestMethod]
    public void ANamedBasicTypePrintsAsItsTypeAroundItsValue()
    {
        AssertText("main.myInt(7)", (main_package.myInt)(nint)7, "myInt");
        AssertText("main.myU8(9)", (main_package.myU8)(byte)9, "myU8");
        AssertText("main.myUintptr(3)", (main_package.myUintptr)(uintptr)(nuint)3, "myUintptr");
        AssertText("main.myFloat(2.5)", (main_package.myFloat)2.5, "myFloat");
        AssertText("main.myF32(0.1)", (main_package.myF32)0.1f, "myF32");
        AssertText("main.myComplex(1-2i)", (main_package.myComplex)new Complex(1, -2), "myComplex");
        AssertText("main.myBool(true)", (main_package.myBool)true, "myBool");
        AssertText("main.myStr(\"a\n\tb\")", (main_package.myStr)(@string)"a\nb", "myStr");
    }

    [TestMethod]
    public void AStringAndASubstitutedErrorTextAreIndented()
    {
        AssertText("line1\n\tline2\n\t", (@string)"line1\nline2\n", "multiline");
        AssertText("e1\n\te2", errors_package.New("e1\ne2"), "err");
        AssertText("", (@string)"", "empty");
    }
}
