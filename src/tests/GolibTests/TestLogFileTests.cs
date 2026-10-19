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
        string? message = (string?)open.Invoke(null, BindingFlags.DoNotWrapExceptions, null, [path, true], null);

        // Go on windows: fmt.Fprintf(os.Stderr, "testing: %s\n", err) with err os.Create's *PathError.
        Assert.AreEqual($"testing: open {path}: The system cannot find the path specified.\n", message);
    }

    // Divergence (d), G's review of 4dea95c684: Go closes the log only after the FIRST m.Run (m.after runs
    // writeProfiles under m.afterOnce, testing.go:2683-2686). A second m.Run re-opens the file write-only at its
    // end and installs a fresh 4096-byte bufio.Writer that nothing ever flushes or closes (go.dev/issue/23129),
    // so its records reach the file only when that buffer fills, one whole buffer at a time.

    [TestMethod]
    public void ASecondRunNeitherFlushesNorClosesTheLog()
    {
        (byte[] written, _) = RunTwice(run => os_package.Getenv($"GO2CS_TESTLOG_RUN{run}"));

        Assert.IsTrue(Contains(written, "getenv GO2CS_TESTLOG_RUN1\n"u8.ToArray()), "the first run's record is flushed by its close");
        Assert.IsFalse(Contains(written, "getenv GO2CS_TESTLOG_RUN2\n"u8.ToArray()),
            "the second run's record waits in a buffer Go never flushes, so it never reaches the file");
    }

    [TestMethod]
    public void ASecondRunWritesOnlyWholeBuffers()
    {
        // 100 records of 68 bytes ("getenv " + a 60-byte name + "\n") is 6800 bytes. bufio fills its 4096 bytes
        // and writes them out only when the NEXT write needs room: one 4096-byte write, and 2704 bytes stay behind.
        string name = "GO2CS_TESTLOG_" + new string('X', 46);
        Assert.AreEqual(60, name.Length);

        (byte[] written, long afterFirstRun) = RunTwice(run =>
        {
            if (run == 1)
                os_package.Getenv("GO2CS_TESTLOG_RUN1");
            else
                for (int i = 0; i < 100; i++)
                    os_package.Getenv(name);
        });

        Assert.AreEqual(4096L, written.Length - afterFirstRun,
            "the second run's 6800 bytes reach the file as exactly one full buffer");
    }

    // A TestMain that calls m.Run twice, each run running one test that does `body(run)`. Returns the file's
    // bytes after the host returns, and its length between the two runs.
    private static (byte[] written, long afterFirstRun) RunTwice(Action<int> body)
    {
        string dir = Directory.CreateTempSubdirectory("go2cs-testlog-").FullName;
        string log = Path.Combine(dir, "testlog.txt");
        int run = 0;
        long afterFirstRun = -1;

        TestRegistry registry = new("guard", []);
        registry.Add("TestRecord", _ => body(run), "a_test.go", 10);
        registry.SetTestMain(m =>
        {
            run = 1;
            m.Value.Run();
            afterFirstRun = new FileInfo(log).Length;
            run = 2;
            m.Value.Run();
        });

        TextWriter previousOut = Console.Out;

        try
        {
            Console.SetOut(new StringWriter());
            TestHost.Run(registry, [$"-test.testlogfile={log}", "-test.run=^TestRecord$"]);
        }
        finally
        {
            Console.SetOut(previousOut);
        }

        Assert.AreEqual(2, run, "TestMain must reach its second m.Run");
        Assert.IsTrue(afterFirstRun > 0, "the first run's close must leave its records in the file");

        // The second run's file is never closed, so read it shared.
        using FileStream stream = new(log, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using MemoryStream bytes = new();
        stream.CopyTo(bytes);
        return (bytes.ToArray(), afterFirstRun);
    }

    private static bool Contains(byte[] haystack, byte[] needle) =>
        Enumerable.Range(0, haystack.Length - needle.Length + 1).Any(i => haystack.AsSpan(i, needle.Length).SequenceEqual(needle));
}
