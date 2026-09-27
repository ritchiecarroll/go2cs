using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

/// <summary>
/// Bar B of the managed execution tracer (Q28, C-3): Go's OWN trace parser accepts a managed program's
/// trace. The oracle is the pinned toolchain's <c>go tool trace -d=parsed</c> -- internal/trace's full
/// reader, exit 0 at EOF and exit 1 with the parser's error on stderr (cmd/trace/main.go logAndDie).
/// </summary>
/// <remarks>
/// Red first: a stream with its frequency batch withheld is refused by the same invocation ("no frequency
/// event found"), so the arm is shown able to fail before its green is believed. Inconclusive only where
/// no Go toolchain resolves (GOROOT, then PATH); every fleet box has one.
/// </remarks>
[TestClass]
public class ExecutionTracerParserTests
{
    private static string? GoTool()
    {
        string exe = OperatingSystem.IsWindows() ? "go.exe" : "go";

        if (Environment.GetEnvironmentVariable("GOROOT") is { Length: > 0 } root && File.Exists(Path.Combine(root, "bin", exe)))
            return Path.Combine(root, "bin", exe);

        foreach (string dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
        {
            if (dir.Length > 0 && File.Exists(Path.Combine(dir, exe)))
                return Path.Combine(dir, exe);
        }

        return null;
    }

    // Runs the toolchain's parser on `trace`: (exit code, stdout, stderr).
    private static (int Code, string Out, string Err) Parse(string go, byte[] trace)
    {
        string file = Path.Combine(Path.GetTempPath(), $"go2cs-trace-{Guid.NewGuid():N}.trace");
        File.WriteAllBytes(file, trace);

        try
        {
            ProcessStartInfo start = new(go, ["tool", "trace", "-d=parsed", file])
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            start.Environment["GOTOOLCHAIN"] = "local";

            using Process process = Process.Start(start)!;
            Task<string> stdout = process.StandardOutput.ReadToEndAsync();
            Task<string> stderr = process.StandardError.ReadToEndAsync();
            Assert.IsTrue(process.WaitForExit(120_000), "go tool trace did not finish");
            return (process.ExitCode, stdout.Result, stderr.Result);
        }
        finally
        {
            File.Delete(file);
        }
    }

    // A managed program's trace: a goroutine created, parked on a channel receive, woken, and exited,
    // beside a second goroutine that sleeps (a self-wake with no waker) and a nested create.
    private static byte[] Scenario()
    {
        MemoryStream bytes = new();
        Thread reader = new(() =>
        {
            while (ExecutionTracer.Read() is { } chunk)
                bytes.Write(chunk);
        });

        Assert.IsTrue(ExecutionTracer.Start());
        reader.Start();

        try
        {
            Goroutine? receiver = null;
            using SemaphoreSlim gate = new(0, 1);
            using ManualResetEventSlim parked = new();
            using CountdownEvent done = new(3);

            Goroutine.Start(() =>
            {
                receiver = Goroutine.Current;

                using (Goroutine.Park(WaitReason.ChanReceive))
                {
                    parked.Set();
                    gate.Wait();
                }

                // A goroutine creating a goroutine: GoCreate from a running G.
                Goroutine.Start(() => done.Signal());
                done.Signal();
            });

            Goroutine.Start(() =>
            {
                using (Goroutine.Park(WaitReason.Sleep))
                    Thread.Sleep(20);

                done.Signal();
            });

            Assert.IsTrue(parked.Wait(TimeSpan.FromSeconds(10)));
            Goroutine.Ready(receiver);
            gate.Release();
            Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(10)));
            Thread.Sleep(100);
        }
        finally
        {
            ExecutionTracer.Stop();
            reader.Join();
        }

        return bytes.ToArray();
    }

    [TestMethod]
    public void GosOwnParserAcceptsAManagedProgramsTrace()
    {
        if (GoTool() is not { } go)
        {
            Assert.Inconclusive("no Go toolchain resolves (GOROOT, PATH): the oracle is unavailable here");
            return;
        }

        byte[] trace = Scenario();

        // The red arm first: the same stream with its frequency batch withheld must be REFUSED.
        byte[] withoutFrequency = WithoutFrequencyBatch(trace);
        (int badCode, _, string badErr) = Parse(go, withoutFrequency);
        Assert.AreEqual(1, badCode, "the oracle must be able to fail: a stream with no frequency batch is invalid");
        StringAssert.Contains(badErr, "no frequency event found");

        (int code, string output, string err) = Parse(go, trace);
        Assert.AreEqual(0, code, $"Go's parser refused the managed trace: {err}");

        StringAssert.Contains(output, "Reason=\"chan receive\"", "the block reason crosses in Go's vocabulary");
        StringAssert.Contains(output, "Running->Waiting");
        StringAssert.Contains(output, "Waiting->Runnable");
        StringAssert.Contains(output, "NotExist->Runnable", "a create");
        StringAssert.Contains(output, "Running->NotExist", "a destroy");
    }

    // Rebuilds the stream without its frequency batch (payload byte 8): header, then every other batch.
    private static byte[] WithoutFrequencyBatch(byte[] trace)
    {
        MemoryStream kept = new();
        kept.Write(trace, 0, 16);

        foreach ((int start, int end, byte first) in Batches(trace))
        {
            if (first != 8)
                kept.Write(trace, start, end - start);
        }

        return kept.ToArray();
    }

    private static System.Collections.Generic.IEnumerable<(int Start, int End, byte First)> Batches(byte[] trace)
    {
        int at = 16;

        while (at < trace.Length)
        {
            int start = at++;

            for (int field = 0; field < 3; field++)
                UVarint(trace, ref at);

            int length = (int)UVarint(trace, ref at);
            byte first = trace[at];
            at += length;
            yield return (start, at, first);
        }
    }

    private static ulong UVarint(byte[] data, ref int at)
    {
        ulong value = 0;

        for (int shift = 0; ; shift += 7)
        {
            byte b = data[at++];
            value |= (ulong)(b & 0x7F) << shift;

            if (b < 0x80)
                return value;
        }
    }
}
