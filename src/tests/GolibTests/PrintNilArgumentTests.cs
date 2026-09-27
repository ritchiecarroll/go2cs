using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// The builtin <c>print</c>/<c>println</c> given a nil interface: gc's runtime printer writes its two
/// zero words, <c>(0x0,0x0)</c>. golib's formatter called <c>ToString()</c> on the argument, so a nil
/// threw a NullReferenceException out of the println itself — which golib reports as a Go nil
/// dereference. runtime's checkPanicNil reaches exactly that on its failure path
/// (<c>println(e, want)</c> with <c>want</c> nil), and the crash replaced the test's own diagnosis.
/// </summary>
[TestClass]
public class PrintNilArgumentTests
{
    // MSTest runs this assembly serially (no [Parallelize]), which is what makes swapping the global
    // stderr writer safe; it is restored whatever the arm does.
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

    [TestMethod]
    public void PrintlnOfANilInterfacePrintsTheTwoZeroWords()
    {
        Assert.AreEqual("(0x0,0x0) 1\n", CaptureStdErr(() => builtin.println(null!, 1)));
    }

    [TestMethod]
    public void PrintOfANilInterfacePrintsTheTwoZeroWords()
    {
        Assert.AreEqual("(0x0,0x0)", CaptureStdErr(() => builtin.print((object)null!)));
    }

    [TestMethod]
    public void NonNilArgumentsKeepTheirRendering()
    {
        Assert.AreEqual("true x 7\n", CaptureStdErr(() => builtin.println(true, (@string)"x", 7)));
    }
}
