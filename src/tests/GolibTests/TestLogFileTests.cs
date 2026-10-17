using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.testing_runtime;

namespace GolibTests;

/// <summary>
/// The host's <c>-test.testlogfile</c> (TestLogBridge) does what Go's testdeps does, observably (G's review
/// of 6f50ffb840): a name os reports is written as its RAW bytes, invalid UTF-8 included; and an open failure
/// says Go's <c>*PathError</c> text, the message Go prints before <c>os.Exit(2)</c>.
/// </summary>
/// <remarks>
/// The exit itself is not asserted here: <c>os.Exit</c> ends the process, so the guard reads the DECISION
/// (TestLogBridge.Open's message) that M.Run's exit acts on. The decision is reached by reflection so this
/// file compiles against the bridge before the decision existed.
/// </remarks>
[TestClass]
public class TestLogFileTests
{
    [TestMethod]
    public void ANameIsLoggedAsItsRawBytes()
    {
        string dir = Directory.CreateTempSubdirectory("go2cs-testlog-").FullName;
        string log = Path.Combine(dir, "testlog.txt");

        // An environment variable NAME that is not valid UTF-8: Go's os.Getenv reports it to the testlog.
        byte[] name = [(byte)'G', (byte)'O', (byte)'2', (byte)'C', (byte)'S', 0xFF, (byte)'K'];

        TestRegistry registry = new("guard", []);
        registry.Add("TestGetenv", _ => { os_package.Getenv(new @string(name)); }, "a_test.go", 10);

        TextWriter previousOut = Console.Out;
        int exitCode;

        try
        {
            Console.SetOut(new StringWriter());
            exitCode = TestHost.Run(registry, [$"-test.testlogfile={log}", "-test.run=^TestGetenv$"]);
        }
        finally
        {
            Console.SetOut(previousOut);
        }

        Assert.AreEqual(0, exitCode);
        byte[] written = File.ReadAllBytes(log);
        byte[] want = [.. "getenv "u8.ToArray(), .. name, (byte)'\n'];

        Assert.IsTrue(Contains(written, want),
            $"the log must hold `getenv ` + the name's own bytes; it holds {BitConverter.ToString(written)}");
    }

    [TestMethod]
    public void AnOpenFailureSaysGosPathErrorText()
    {
        MethodInfo open = typeof(TestHost).Assembly.GetType("go.testing_runtime.TestLogBridge")
            ?.GetMethod("Open", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new AssertFailedException("TestLogBridge.Open, the open decision M.Run exits on, does not exist");

        string path = Path.Combine(Path.GetTempPath(), "go2cs-no-such-dir-" + Guid.NewGuid().ToString("N"), "testlog.txt");
        string? message = (string?)open.Invoke(null, BindingFlags.DoNotWrapExceptions, null, [path], null);

        // Go on windows: fmt.Fprintf(os.Stderr, "testing: %s\n", err) with err os.Create's *PathError.
        Assert.AreEqual($"testing: open {path}: The system cannot find the path specified.\n", message);
    }

    private static bool Contains(byte[] haystack, byte[] needle) =>
        Enumerable.Range(0, haystack.Length - needle.Length + 1).Any(i => haystack.AsSpan(i, needle.Length).SequenceEqual(needle));
}
