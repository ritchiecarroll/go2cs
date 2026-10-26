using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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
/// no Go toolchain resolves (GOROOT, then PATH) or the one that does is not the pinned release; the reason
/// is named either way, and the arm never runs an ambient go of another release.
/// </remarks>
[TestClass]
public class ExecutionTracerParserTests
{
    // The go the oracle runs: GOROOT's, else the first on PATH, and only if it IS the pinned release.
    // The parser under test is internal/trace's, and a go of another release parses a different flag set
    // (go1.22's -d is an int, so `-d=parsed` prints usage), so an ambient toolchain that differs is a
    // wrong oracle and the arm must not run it. `Skip` names why no go was returned.
    internal static (string? Go, string? Skip) ResolveOracle(string? goRoot, string? path, string? pinnedRelease, Func<string, string?> versionOf)
    {
        string exe = OperatingSystem.IsWindows() ? "go.exe" : "go";
        string? found = null;

        if (goRoot is { Length: > 0 } && File.Exists(Path.Combine(goRoot, "bin", exe)))
            found = Path.Combine(goRoot, "bin", exe);

        foreach (string dir in found is null ? (path ?? "").Split(Path.PathSeparator) : [])
        {
            if (dir.Length > 0 && File.Exists(Path.Combine(dir, exe)))
            {
                found = Path.Combine(dir, exe);
                break;
            }
        }

        if (found is null)
            return (null, "no Go toolchain resolves (GOROOT, PATH): the oracle is unavailable here");

        if (pinnedRelease is not { Length: > 0 })
            return (null, $"the pinned Go release cannot be read from version.props, so the go at {found} cannot be verified as the oracle");

        if (versionOf(found) is not { } version)
            return (null, $"`go version` gave no answer for the go at {found}, so it cannot be verified as the pinned release go{pinnedRelease}");

        // Go prints go1.25 for the x.y.0 release, not go1.25.0.
        if (version != $"go{pinnedRelease}" && !(pinnedRelease.EndsWith(".0") && version == $"go{pinnedRelease[..^2]}"))
        {
            return (null, $"the go at {found} is {version}, but the oracle is the pinned toolchain's parser " +
                $"(go{pinnedRelease}); set GOROOT to a go{pinnedRelease} toolchain to run this arm");
        }

        return (found, null);
    }

    // The corpus's pinned Go release: GoStdLibVersion in the nearest version.props above the test binary.
    private static string? PinnedRelease()
    {
        for (DirectoryInfo? dir = new(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            string props = Path.Combine(dir.FullName, "version.props");

            if (File.Exists(props) &&
                System.Text.RegularExpressions.Regex.Match(File.ReadAllText(props), @"<GoStdLibVersion>\s*([^<\s]+)\s*</GoStdLibVersion>") is { Success: true } match)
                return match.Groups[1].Value;
        }

        return null;
    }

    // What `go version` reports for one go binary, with its own toolchain (GOTOOLCHAIN=local): "go1.24.13".
    private static string? VersionOf(string go)
    {
        try
        {
            ProcessStartInfo start = new(go, ["version"])
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            start.Environment["GOTOOLCHAIN"] = "local";

            using Process process = Process.Start(start)!;
            Task<string> stdout = process.StandardOutput.ReadToEndAsync();
            process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(30_000) || process.ExitCode != 0)
                return null;

            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(stdout.Result, @"^go version (go\S+)");
            return match.Success ? match.Groups[1].Value : null;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return null;
        }
    }

    internal static (string? Go, string? Skip) Oracle() =>
        ResolveOracle(Environment.GetEnvironmentVariable("GOROOT"), Environment.GetEnvironmentVariable("PATH"), PinnedRelease(), VersionOf);

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
        (string? resolved, string? skip) = Oracle();

        if (resolved is not { } go)
        {
            Assert.Inconclusive(skip);
            return;
        }

        byte[] trace = Scenario();

        // The red arm first: the same stream with its sync batches (and the frequency each carries)
        // withheld must be REFUSED.
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

    // A managed program's trace over THREE generations, the shape a flight recorder holds: a goroutine
    // parked through two advances and woken in the third, a goroutine RUNNING through two advances
    // without an event of its own that logs in the third, and the stop's own end of generation.
    private static byte[] GenerationsScenario()
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
            Goroutine? sleeper = null;
            using SemaphoreSlim gate = new(0, 1);
            using ManualResetEventSlim parked = new();
            using ManualResetEventSlim spinning = new();
            using CountdownEvent done = new(2);
            int release = 0;

            Goroutine.Start(() =>
            {
                sleeper = Goroutine.Current;

                using (Goroutine.Park(WaitReason.ChanReceive))
                {
                    parked.Set();
                    gate.Wait();
                }

                done.Signal();
            });

            Goroutine.Start(() =>
            {
                spinning.Set();
                SpinWait.SpinUntil(() => Volatile.Read(ref release) != 0);
                ExecutionTracer.OnUserLog(0, "message"u8, "hello"u8);
                done.Signal();
            });

            Assert.IsTrue(parked.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(spinning.Wait(TimeSpan.FromSeconds(10)));

            ExecutionTracer.Advance(false);
            ExecutionTracer.Advance(false);

            Volatile.Write(ref release, 1);
            Goroutine.Ready(sleeper);
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
    public void GosOwnParserAcceptsAMultiGenerationManagedTrace()
    {
        (string? resolved, string? skip) = Oracle();

        if (resolved is not { } go)
        {
            Assert.Inconclusive(skip);
            return;
        }

        byte[] trace = GenerationsScenario();
        List<(int Start, int End, byte First, ulong Gen)> items = Items(trace);
        Assert.IsTrue(items.Count(i => i.First == EvEndOfGeneration) >= 3, "two advances and the stop end three generations");

        // RED ARM 1: the last generation's end signal withheld. Go 1.26 refuses a generation that ends at EOF.
        byte[] truncated = trace[..items[^1].Start];
        Assert.AreEqual(EvEndOfGeneration, items[^1].First, "the stream must end with its end-of-generation signal");
        (int truncCode, _, string truncErr) = Parse(go, truncated);
        Assert.AreEqual(1, truncCode, "the oracle must be able to fail: a generation with no end signal is truncated");
        StringAssert.Contains(truncErr, "incomplete generation found; trace likely truncated");

        // RED ARM 2: the first generation's clock snapshot withheld. Go 1.25+ requires one per generation.
        (int clockCode, _, string clockErr) = Parse(go, WithoutFirstClockSnapshot(trace, items));
        Assert.AreEqual(1, clockCode, "the oracle must be able to fail: a generation with no clock snapshot is invalid");
        StringAssert.Contains(clockErr, "no clock snapshot event found");

        // GREEN: the whole trace.
        (int code, string output, string err) = Parse(go, trace);
        Assert.AreEqual(0, code, $"Go's parser refused the managed trace: {err}");
        StringAssert.Contains(output, "hello", "the user log crosses into the trace");

        // GREEN: a snapshot that starts at a LATER generation, as a flight recorder's ring does. Every
        // goroutine alive in a generation is stated in it, so the parser's first generation is enough.
        int secondStart = items.First(i => i.Gen == 2).Start;
        byte[] fromSecond = [.. trace[..16], .. trace[secondStart..]];
        (int ringCode, string ringOut, string ringErr) = Parse(go, fromSecond);
        Assert.AreEqual(0, ringCode, $"Go's parser refused a trace that starts at generation 2: {ringErr}");
        StringAssert.Contains(ringOut, "hello");

        int thirdStart = items.First(i => i.Gen == 3).Start;
        byte[] fromThird = [.. trace[..16], .. trace[thirdStart..]];
        (int lastCode, _, string lastErr) = Parse(go, fromThird);
        Assert.AreEqual(0, lastCode, $"Go's parser refused a trace that starts at generation 3: {lastErr}");
    }

    private const byte EvEndOfGeneration = 52;
    private const byte EvSync = 50;
    private const byte EvClockSnapshot = 51;

    // Every item of a Go 1.26 stream after its header: (start, end, first payload byte, generation),
    // with an in-band end-of-generation signal as a one-byte item of generation 0.
    private static List<(int Start, int End, byte First, ulong Gen)> Items(byte[] trace)
    {
        List<(int, int, byte, ulong)> items = [];
        int at = 16;

        while (at < trace.Length)
        {
            int start = at;

            if (trace[at] == EvEndOfGeneration)
            {
                items.Add((start, ++at, EvEndOfGeneration, 0));
                continue;
            }

            at++;
            ulong gen = UVarint(trace, ref at);
            UVarint(trace, ref at);
            UVarint(trace, ref at);
            int length = (int)UVarint(trace, ref at);
            byte first = trace[at];
            at += length;
            items.Add((start, at, first, gen));
        }

        return items;
    }

    // The stream with generation 1's sync batch rebuilt to hold its frequency event alone.
    private static byte[] WithoutFirstClockSnapshot(byte[] trace, List<(int Start, int End, byte First, ulong Gen)> items)
    {
        (int start, int end, _, ulong gen) = items.First(i => i.First == EvSync && i.Gen == 1);
        int at = start + 1;
        UVarint(trace, ref at);
        UVarint(trace, ref at);
        ulong ts = UVarint(trace, ref at);
        UVarint(trace, ref at);

        // The payload: EvSync, EvFrequency, freq, EvClockSnapshot, ... -- keep through the frequency.
        int payload = at;
        int cut = payload + 2;
        UVarint(trace, ref cut);
        Assert.AreEqual(EvClockSnapshot, trace[cut], "the sync batch's clock snapshot follows its frequency");

        MemoryStream rebuilt = new();
        rebuilt.Write(trace, 0, start);
        rebuilt.WriteByte(1);
        WriteUVarint(rebuilt, gen);
        WriteUVarint(rebuilt, ulong.MaxValue);
        WriteUVarint(rebuilt, ts);
        WriteUVarint(rebuilt, (ulong)(cut - payload));
        rebuilt.Write(trace, payload, cut - payload);
        rebuilt.Write(trace, end, trace.Length - end);
        return rebuilt.ToArray();
    }

    private static void WriteUVarint(Stream stream, ulong value)
    {
        while (value >= 0x80)
        {
            stream.WriteByte((byte)(value | 0x80));
            value >>= 7;
        }

        stream.WriteByte((byte)value);
    }

    // Rebuilds the stream without its sync batches (payload byte EvSync, each carrying the generation's
    // one frequency event): header, then every other item, end-of-generation signals included.
    private static byte[] WithoutFrequencyBatch(byte[] trace)
    {
        MemoryStream kept = new();
        kept.Write(trace, 0, 16);

        foreach ((int start, int end, byte first, ulong _) in Items(trace))
        {
            if (first != EvSync)
                kept.Write(trace, start, end - start);
        }

        return kept.ToArray();
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
