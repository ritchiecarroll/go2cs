#nullable enable

using System;
using System.IO;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// The builtin <c>print</c>/<c>println</c> given a float or complex: gc's runtime printer writes
/// <c>strconv.AppendFloat(v, 'g', -1, bitSize)</c> and <c>AppendComplex</c> (runtime/print.go's printfloat and
/// printcomplex). golib wrote .NET's ToString, so a large float printed <c>1234567</c> and a complex
/// <c>&lt;1; 2&gt;</c>, and a named basic type printed its String(). Every expected text is go1.26.10's own println
/// output (linux/amd64, measured 2026-10-24).
/// </summary>
[TestClass]
public class PrintFloatArgumentTests
{
    // As PrintNilArgumentTests: MSTest runs this assembly serially, so swapping the global stderr writer is safe.
    private static string CaptureStdErr(Action body)
    {
        TextWriter saved = Console.Error;
        StringWriter captured = new() { NewLine = "\n" };

        try
        {
            Console.SetError(captured);
            body();
        }
        finally
        {
            Console.SetError(saved);
        }

        return captured.ToString();
    }

    private static void AssertPrintln(string expected, object value, string name) =>
        Assert.AreEqual(expected + "\n", CaptureStdErr(() => builtin.println(value)), name);

    [TestMethod]
    public void AFloatPrintsInTheShortestGForm()
    {
        AssertPrintln("1.5", 1.5, "1.5");
        AssertPrintln("0.25", 0.25f, "float32(0.25)");
        AssertPrintln("1e+21", 1e21, "1e21");
        AssertPrintln("1e-07", 1e-7, "1e-7");
        AssertPrintln("0", 0.0, "0.0");
        AssertPrintln("-0", -0.0, "negative zero");
        AssertPrintln("NaN", double.NaN, "NaN");
        AssertPrintln("+Inf", double.PositiveInfinity, "+Inf");
        AssertPrintln("-Inf", double.NegativeInfinity, "-Inf");
        AssertPrintln("0.33333334", (float)(1.0 / 3.0), "float32(1.0 / 3.0)");
        AssertPrintln("1.234567e+06", 1234567.0, "1234567.0");
    }

    [TestMethod]
    public void AComplexPrintsParenthesizedWithASignedImaginaryPart()
    {
        AssertPrintln("(1+2i)", new Complex(1, 2), "complex(1, 2)");
        AssertPrintln("(0.5-1i)", new complex64(0.5f, -1f), "complex64(complex(0.5, -1))");
    }

#pragma warning disable CS8981 // Go's own lower-case names
    // Stands in for the converted `main` package: the shape go2cs-gen emits for `type T <basic>`.
    private static class main_package
    {
        [GoType("num:float64")] public readonly struct myFloat(double value) { private readonly double m_value = value; public static implicit operator myFloat(double value) => new(value); }
        [GoType("bool")] public readonly struct myBool(bool value) { private readonly bool m_value = value; public static implicit operator myBool(bool value) => new(value); }
        [GoType("@string")] public readonly struct myStr(@string value) { private readonly @string m_value = value; public static implicit operator myStr(@string value) => new(value); }
        [GoType("num:complex128")] public readonly struct myComplex(Complex value) { private readonly Complex m_value = value; public static implicit operator myComplex(Complex value) => new(value); }
    }
#pragma warning restore CS8981

    [TestMethod]
    public void ANamedBasicTypePrintsItsUnderlyingValueNeverItsStringMethod()
    {
        // println(time.Second): the runtime printer never calls String(), so Go prints the int64, not "1s".
        AssertPrintln("1000000000", time_package.ΔSecond, "time.Second");
        AssertPrintln("1.234567e+06", (main_package.myFloat)1234567.0, "myFloat(1234567)");
        AssertPrintln("true", (main_package.myBool)true, "myBool(true)");
        AssertPrintln("x", (main_package.myStr)(@string)"x", "myStr(\"x\")");
        AssertPrintln("(1+2i)", (main_package.myComplex)new Complex(1, 2), "myComplex(complex(1, 2))");
    }

    [TestMethod]
    public void AnUntypedConstantPrintsAsItsDefaultType()
    {
        // const big = 1e21; const c = 1 + 2i; println(big, math.Pi, c, math.MaxFloat64)
        // The converter casts an untyped float to float64 at the call and passes the untyped complex as itself.
        UntypedFloat big = 1e21, pi = 3.14159265358979323846264338327950288419716939937510582097494459, maxFloat64 = 1.7976931348623157e+308;
        UntypedComplex c = new Complex(1, 2);

        Assert.AreEqual("1e+21 3.141592653589793 (1+2i) 1.7976931348623157e+308\n",
            CaptureStdErr(() => builtin.println((double)big, (double)pi, c, (double)maxFloat64)), "converted call");
        AssertPrintln("1e+21", big, "an untyped float passed as itself");
    }

    [TestMethod]
    public void PrintAndPrintlnSeparateAsBefore()
    {
        Assert.AreEqual("1.5 (1+2i) 0.25\n", CaptureStdErr(() => builtin.println(1.5, new Complex(1, 2), 0.25f)), "println separates with one space");
        Assert.AreEqual("1.52.5\n", CaptureStdErr(() => builtin.print(1.5, 2.5, (@string)"\n")), "print does not separate");
    }
}
