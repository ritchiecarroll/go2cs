using System;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using Δruntime = go.runtime_package;

namespace GolibTests;

/// <summary>
/// Go's printquoted has ONE implementation here: the converted runtime/print.cs body, which
/// print(quoted(s)) reaches through runtime/print_impl.cs. The managed traceback cannot call it, because
/// it assembles a goroutine header in a StringBuilder rather than writing to the print stream, so
/// runtime/managed_impl.cs keeps a hand-written twin, appendQuoted, for the header's pprof labels. This
/// test binds the twin to the converted printer: for every escape arm of go1.26.9 print.go:212-246, its
/// boundaries, and the invalid UTF-8 sequences G probed against Go, both must produce the same bytes.
/// </summary>
[TestClass]
public class PrintQuotedParityTests
{
    [ClassInitialize]
    public static void LoadTheRuntimesPrinter(TestContext _) =>
        System.Runtime.CompilerServices.RuntimeHelpers.RunModuleConstructor(typeof(Δruntime).Module.ModuleHandle);

    // The traceback's twin, as TracebackLabelQuoteTests reads it.
    private static string AppendQuoted(byte[] bytes)
    {
        MethodInfo method = typeof(Δruntime).GetMethod("appendQuoted", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("runtime's appendQuoted is not where the parity test expects it");

        StringBuilder trace = new();
        method.Invoke(null, BindingFlags.DoNotWrapExceptions, null, [trace, new @string(bytes)], null);
        return trace.ToString();
    }

    // The converted printquoted, through print(quoted(s)); every byte it writes is ASCII.
    private static string DumpPrintQuoted(byte[] bytes) =>
        Encoding.Latin1.GetString(Δruntime.GoCaptureWritebuf(() => builtin.print(Δruntime.GoQuotedPrintArg(new @string(bytes)))));

    private static void AssertSameBytes(string name, params byte[] bytes)
    {
        string printed = DumpPrintQuoted(bytes);

        Assert.AreEqual(printed, AppendQuoted(bytes), $"{name}: appendQuoted differs from the converted printquoted ({Convert.ToHexString(bytes)})");
    }

    [TestMethod]
    public void TheNamedEscapesMatch()
    {
        AssertSameBytes("newline", (byte)'a', (byte)'\n', (byte)'b');
        AssertSameBytes("carriage return", (byte)'\r');
        AssertSameBytes("tab", (byte)'x', (byte)'\t', (byte)'y');
        AssertSameBytes("backslash", (byte)'\\');
        AssertSameBytes("double quote", (byte)'"', (byte)'q', (byte)'"');
        AssertSameBytes("empty");
    }

    [TestMethod]
    public void PrintableAsciiAndItsBoundariesMatch()
    {
        AssertSameBytes("space, the first printable", (byte)' ');
        AssertSameBytes("tilde, the last printable", (byte)'~');
        AssertSameBytes("letters, digits and punctuation", "Az09!#$%&'()*+,-./:;<=>?@[]^_`{|}"u8.ToArray());
    }

    [TestMethod]
    public void TheHexEscapeArmsAndTheirBoundariesMatch()
    {
        AssertSameBytes("NUL, the first \\x", 0x00);
        AssertSameBytes("ESC", 0x1B);
        AssertSameBytes("unit separator, the last below space", 0x1F);
        AssertSameBytes("DEL, the first \\u", 0x7F);
        AssertSameBytes("U+0080", 0xC2, 0x80);
        AssertSameBytes("U+03A3", 0xCE, 0xA3);
        AssertSameBytes("U+FFFF, the last \\u", 0xEF, 0xBF, 0xBF);
        AssertSameBytes("U+10000, the first \\U", 0xF0, 0x90, 0x80, 0x80);
        AssertSameBytes("U+45678", 0xF1, 0x85, 0x99, 0xB8);
        AssertSameBytes("U+10FFFF, the last rune", 0xF4, 0x8F, 0xBF, 0xBF);
    }

    [TestMethod]
    public void InvalidUtf8MatchesOneReplacementPerByte()
    {
        AssertSameBytes("E2 82 41, an unfinished three-byte sequence", 0xE2, 0x82, 0x41);
        AssertSameBytes("F0 9F 98, an unfinished four-byte sequence", 0xF0, 0x9F, 0x98);
        AssertSameBytes("a lone continuation byte", 0x80);
        AssertSameBytes("an encoded surrogate", 0xED, 0xA0, 0x80);
        AssertSameBytes("an overlong slash", 0xC0, 0xAF);
        AssertSameBytes("a byte past U+10FFFF", 0xF5, 0x80, 0x80, 0x80);
    }
}
