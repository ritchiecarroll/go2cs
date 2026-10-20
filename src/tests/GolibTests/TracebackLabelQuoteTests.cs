using System;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// The goroutine-header label quoting (runtime's managed appendQuoted, Go's printquoted, go1.26.9
/// print.go:212-246) decodes a label the way Go's <c>for _, r := range s</c> does: an invalid byte is ONE
/// U+FFFD and consumes exactly one byte, where .NET's Rune.DecodeFromUtf8 consumes a maximal invalid
/// subpart. G's probes against Go: E2 82 41 prints two U+FFFD and an A; F0 9F 98 prints three U+FFFD.
/// </summary>
[TestClass]
public class TracebackLabelQuoteTests
{
    private static string Quoted(params byte[] bytes)
    {
        MethodInfo method = typeof(runtime_package).GetMethod("appendQuoted", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("runtime's appendQuoted is not where the guard expects it");

        StringBuilder trace = new();
        method.Invoke(null, BindingFlags.DoNotWrapExceptions, null, [trace, new @string(bytes)], null);
        return trace.ToString();
    }

    private const string FFFD = "\\ufffd";

    [TestMethod]
    public void AnUnfinishedThreeByteSequenceIsOneReplacementPerByte() =>
        Assert.AreEqual($"\"{FFFD}{FFFD}A\"", Quoted(0xE2, 0x82, 0x41));

    [TestMethod]
    public void AnUnfinishedFourByteSequenceIsOneReplacementPerByte() =>
        Assert.AreEqual($"\"{FFFD}{FFFD}{FFFD}\"", Quoted(0xF0, 0x9F, 0x98));

    [TestMethod]
    public void ControlValidRunesQuoteAsGoDoes()
    {
        // b ESC i t, a Greek capital sigma, and a supplementary rune: \x, \u and \U, lower-case hex.
        Assert.AreEqual("\"b\\x1bit\"", Quoted(0x62, 0x1B, 0x69, 0x74));
        Assert.AreEqual("\"\\u03a3\"", Quoted(0xCE, 0xA3));
        Assert.AreEqual("\"\\U00045678\"", Quoted(0xF1, 0x85, 0x99, 0xB8));
    }
}
