using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using Δruntime = go.runtime_package;

namespace GolibTests;

// The Go compiler prints a runtime.hex through printhex ("0x" and the digits) and a runtime.quoted
// through printquoted (print.go: "The compiler knows that a print of a value of this type ..."). golib
// cannot name those types, so they implement IRuntimePrintable (runtime/print_impl.cs) and print calls
// them. Before that, print(hex(v)) wrote a decimal and print(quoted(s)) the raw string: TestPrintQuoted
// failed all nine subtests and the runtime's hexdump symbol lines read "+0" for "+0x0".
[TestClass]
public class PrintRuntimeTypesTests
{
    [ClassInitialize]
    public static void LoadTheRuntimesPrinter(TestContext _) =>
        System.Runtime.CompilerServices.RuntimeHelpers.RunModuleConstructor(typeof(Δruntime).Module.ModuleHandle);

    private static string Printed(System.Action body) =>
        Encoding.Latin1.GetString(Δruntime.GoCaptureWritebuf(body));

    // The nine cases of Go 1.26's runtime/print_quoted_test.go, with its expected texts.
    [TestMethod]
    public void QuotedPrintsTheEscapedFormGosPrintquotedWrites()
    {
        (string input, string expected)[] cases =
        [
            ("baz", "\"baz\""),
            ("foobar", "\"foobar\""),
            ("baz\n", "\"baz\\n\""),
            ("b\u001bit", "\"b\\x1bit\""),
            ("b\0ar", "\"b\\x00ar\""),
            ("\u1234\u03a3", "\"\\u1234\\u03a3\""),
            ("fizz\tle", "\"fizz\\tle\""),
            ("\U00045678boop", "\"\\U00045678boop\""),
            ("fiz\\zl\re", "\"fiz\\\\zl\\re\""),
        ];

        foreach ((string input, string expected) in cases)
        {
            string got = Printed(() => builtin.print(Δruntime.GoQuotedPrintArg(input)));

            Assert.AreEqual(expected, got, $"print(quoted({input.Length} chars starting {(int)input[0]:x})) wrote the wrong text");
        }
    }

    [TestMethod]
    public void HexPrintsZeroXAndTheDigits()
    {
        Assert.AreEqual("0x0", Printed(() => builtin.print(Δruntime.GoHexPrintArg(0))));
        Assert.AreEqual("0xff", Printed(() => builtin.print(Δruntime.GoHexPrintArg(255))));
        Assert.AreEqual("0xffffffffffffffff", Printed(() => builtin.print(Δruntime.GoHexPrintArg(ulong.MaxValue))));
    }

    // The statement the hexdump symbol line is: strings, a hex, a string, with no separator in print and
    // a space in println, and the other arguments still print as they did.
    [TestMethod]
    public void TheyMixWithOtherArgumentsInOnePrintStatement()
    {
        Assert.AreEqual("<f+0x10>\n", Printed(() => builtin.print((@string)"<", (@string)"f", (@string)"+", Δruntime.GoHexPrintArg(16), (@string)">\n")));
        Assert.AreEqual("7 0xa \"x\" true\n", Printed(() => builtin.println(7, Δruntime.GoHexPrintArg(10), Δruntime.GoQuotedPrintArg("x"), true)));
    }
}
