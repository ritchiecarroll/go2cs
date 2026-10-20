using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

[TestClass]
public class RawStringLiteralTests
{
    // A Go raw string never holds a CR, and the converter emits a multi-line one as """..."""u8.Raw()
    // because every checkout gives the .cs file CRLF line endings. These lock the two halves: Raw()
    // removes every CR, and nothing WITHOUT Raw() does -- an interpreted "\r\n" keeps its CR, so the
    // strip cannot move into the shared span-to-@string conversion.

    [TestMethod]
    public void RawRemovesEveryCarriageReturn()
    {
        @string value = "line one\r\nline two\r\n"u8.Raw();

        Assert.AreEqual((nint)18, len(value));
        Assert.AreEqual("line one\nline two\n", value.ToString());
    }

    [TestMethod]
    public void RawLeavesAnLfOnlyLiteralByteIdentical()
    {
        @string value = "line one\nline two"u8.Raw();

        Assert.AreEqual((nint)17, len(value));
        Assert.AreEqual("line one\nline two", value.ToString());
    }

    [TestMethod]
    public void AnInterpretedCrWithoutRawKeepsItsCr()
    {
        @string value = "keeps\r\nits CR"u8;

        Assert.AreEqual((nint)13, len(value));
        Assert.AreEqual((byte)'\r', value[5]);
    }

    [TestMethod]
    public void RawStringOverloadRemovesEveryCarriageReturn()
    {
        @string value = "head\r\n".Raw() + (@string)"`x`" + "\r\ntail".Raw();

        Assert.AreEqual("head\n`x`\ntail", value.ToString());
    }

    [TestMethod]
    public void RawOfAnEmptyLiteralIsTheEmptyString()
    {
        @string value = ""u8.Raw();

        Assert.AreEqual((nint)0, len(value));
    }
}
