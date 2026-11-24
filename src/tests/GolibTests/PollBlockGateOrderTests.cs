// PollBlockGateOrderTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

#nullable enable

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

// The poller's wait loop (internal/poll's pollBlock, one per flavour) parked with Goroutine.Park INSIDE lock (desc.Gate).
// Monitor.Wait takes the gate back before it returns, so the park scope's Dispose -- which waits for a P under M:N
// (GoScheduler.ExitBlocked) -- ran while the goroutine held the gate. A carrier that holds a P and then takes the same
// gate (SetReadDeadline: net/http's abortPendingRead does exactly this) waits for the gate forever, and the woken reader
// waits for that carrier's P forever. darwin mn-platform run 45358621509 hung 2 of 3 closure runs this way (P5 FINDING
// 20261124T101146Z: 9 and 12 readers in PTokens.Wait under the gate, 5 and 6 carriers on the gate).
//
// The interleaving is forced in a CHILD (GO2CS_MN is read once per process): one P, the monitor's P injection off (the
// hosted hang had used up its injection budget on the blocked carriers), a reader parked in Read on a pipe, and a setter
// that sets a past read deadline (the wake), holds its P while the reader wakes and takes the gate back, then sets the
// deadline again. With the park disposed under the gate the second set never returns; with it disposed after the gate
// is released, both finish and the read reports the deadline.
[TestClass]
public class PollBlockGateOrderTests
{
    private const string ChildVariable = "GO2CS_TEST_POLLBLOCK_CHILD";

    // The CHILD. Run on its own (no variable) it does nothing.
    [TestMethod]
    public void ChildReaderWokenUnderTheGateWhileItsPIsHeld()
    {
        if (Environment.GetEnvironmentVariable(ChildVariable) != "1")
            return;

        Assert.IsTrue(GoScheduler.Enabled, "the child runs with GO2CS_MN=1");

        (ж<os_package.File> r, ж<os_package.File> w, error err) = os_package.Pipe();
        Assert.IsNull(err, "os.Pipe");

        using ManualResetEventSlim reading = new(), readerDone = new(), setterDone = new();
        error? readErr = null;

        Action reader = () =>
        {
            reading.Set();
            (_, readErr) = r.Read(new slice<byte>(1));
            readerDone.Set();
        };

        builtin.goǃtwin(reader, () => { reader(); return default; });

        Assert.IsTrue(reading.Wait(TimeSpan.FromSeconds(30)), "the reader goroutine started");

        // The reader is about to park in pollBlock; give it time to get there (this thread is no carrier and holds no P).
        Thread.Sleep(500);

        Action setter = () =>
        {
            time_package.Time past = time_package.Now().Add(-time_package.Second);

            r.SetReadDeadline(past);    // the wake: the reader takes the gate back as soon as this returns
            Thread.Sleep(500);          // hold the only P: the woken reader now waits for it
            r.SetReadDeadline(past);    // takes the gate again
            setterDone.Set();
        };

        builtin.goǃtwin(setter, () => { setter(); return default; });

        bool setterFinished = setterDone.Wait(TimeSpan.FromSeconds(20));
        bool readerFinished = readerDone.Wait(TimeSpan.FromSeconds(20));

        if (!setterFinished || !readerFinished)
        {
            Console.WriteLine($"POLLBLOCK DEADLOCK setter={(setterFinished ? "done" : "stuck")} reader={(readerFinished ? "done" : "stuck")}");
            Assert.Fail($"deadlock: setter {(setterFinished ? "returned" : "never returned")}, reader {(readerFinished ? "returned" : "never returned")} -- the park was disposed under desc.Gate");
        }

        // os.File.Read wraps the poller's error in a PathError, as Go does.
        Assert.IsTrue(errors_package.Is(readErr!, os_package.ErrDeadlineExceeded), $"the read reports the deadline: {readErr}");

        w.Close();
        r.Close();
    }

    [TestMethod]
    public void AReaderWokenInThePollerDoesNotWaitForAPUnderTheGate()
    {
        string filter = $"FullyQualifiedName={typeof(PollBlockGateOrderTests).FullName}.{nameof(ChildReaderWokenUnderTheGateWhileItsPIsHeld)}";

        ProcessStartInfo start = new("dotnet", ["test", typeof(PollBlockGateOrderTests).Assembly.Location, "--filter", filter])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        start.Environment[ChildVariable] = "1";
        start.Environment["GO2CS_MN"] = "1";
        start.Environment["GO2CS_MN_PROCS"] = "1";
        start.Environment["GO2CS_MN_PREEMPT_MS"] = "0";

        using Process process = Process.Start(start)!;
        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(180_000))
        {
            process.Kill(entireProcessTree: true);
            Assert.Fail("the pollBlock child did not finish in 180 s");
        }

        string child = stdout.Result + stderr.Result;

        Assert.IsFalse(child.Contains("POLLBLOCK DEADLOCK", StringComparison.Ordinal), $"the reader and the setter deadlocked:\n{child}");
        Assert.AreEqual(0, process.ExitCode, $"the child failed:\n{child}");
        Assert.IsTrue(child.Contains("Passed!", StringComparison.Ordinal) || child.Contains("Passed:", StringComparison.Ordinal), $"the child test ran and passed:\n{child}");
    }
}
