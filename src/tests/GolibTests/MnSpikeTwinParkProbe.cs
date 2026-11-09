using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MnSched;
using static go.builtin;

namespace GolibTests;

// G2, the M:N spike's macOS gate (docs/phase4/DESIGN-mn-scheduler.md §8 item 5): 10^5 goroutines parked at once on
// the hosted osx-arm64 runner, where kern.num_taskthreads caps a process near 2,000 threads. SPIKE BRANCH ONLY: it
// compiles the P2 prototype's sources (src/tools/mnspike/mnsched) into GolibTests and never reaches master.
//
//   O0 arm: today's goroutines, one OS thread each, parked on an event. On macOS the `go` statement throws once the
//           cap is reached (C1's run 42497561630: 2,027 on osx-arm64, 3,051 on osx-x64), so this arm is the RED and
//           names the count it reached. Elsewhere 10^5 threads cost minutes and gigabytes: Inconclusive.
//   TWIN arm: the same 10^5 goroutines as async twins on the prototype scheduler: a parked goroutine is a stored
//           continuation, so it holds no thread. GREEN when all park on about GOMAXPROCS carriers.
// Both arms print one READ line with what they measured.
[TestClass]
public class MnSpikeTwinParkProbe
{
    private const int Count = 100_000;

    private static string TaskThreadCap()
    {
        if (!OperatingSystem.IsMacOS()) return "n/a";
        try
        {
            using Process p = Process.Start(new ProcessStartInfo("/usr/sbin/sysctl", "-n kern.num_taskthreads") { RedirectStandardOutput = true })!;
            string value = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExit();
            return value;
        }
        catch (Exception ex) { return $"unread ({ex.GetType().Name})"; }
    }

    [TestMethod]
    public void O0ThreadPerGoroutineParks100k()
    {
        if (!OperatingSystem.IsMacOS())
        {
            Assert.Inconclusive("O0's 10^5 parked threads are the macOS red; elsewhere they cost minutes and gigabytes: NOT MEASURED on this host");
            return;
        }

        using ManualResetEventSlim release = new(false);
        using CountdownEvent exited = new(1);
        int started = 0;
        Exception? stopped = null;
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < Count; i++)
        {
            try
            {
                exited.AddCount();
                goǃ(() => { release.Wait(); exited.Signal(); });
                started++;
            }
            catch (Exception ex)
            {
                exited.Signal();            // the goroutine that failed to start never signals
                stopped = ex;
                break;
            }
        }

        double seconds = sw.Elapsed.TotalSeconds;
        release.Set();
        exited.Signal();
        bool joined = exited.Wait(TimeSpan.FromSeconds(60));

        Console.WriteLine($"READ G2-O0 rid={System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier} kern.num_taskthreads={TaskThreadCap()} started={started}/{Count} in {seconds:F2}s stopped-by={stopped?.GetType().Name ?? "none"} joined={joined}");
        Assert.AreEqual(Count, started, $"O0 (a thread per goroutine) started {started} of {Count} before the `go` statement threw {stopped?.GetType().Name}: the macOS cap (kern.num_taskthreads={TaskThreadCap()})");
    }

    // COORD's ask for the record: the macOS thread budget as a function of CONCURRENT UNTWINNED BLOCKS. Each
    // synchronous park on a carrier (an untwinned function, and likewise a blocking native call) holds its OS thread,
    // and the handoff starts a spare carrier. This arm blocks goroutines synchronously, one per goroutine, until the OS
    // refuses a thread or the arm's maximum is reached, and reads how many blocks the process held. On macOS the
    // answer is the budget a program can plan against; elsewhere it is bounded by the arm's maximum. A READ, not a gate.
    [TestMethod]
    public void UntwinnedBlocksBeforeTheThreadCap()
    {
        int max = OperatingSystem.IsMacOS() ? 4000 : 1500;
        var s = new Scheduler(new Options { Injection = false });
        var gate = new Chan<int>(s);
        var finished = new WaitGroup(s);
        finished.Add(max);
        int blocking = 0;
        Process self = Process.GetCurrentProcess();
        self.Refresh();
        int threadsAtStart = self.Threads.Count;

        for (int i = 0; i < max; i++)
            s.Go(() => { Interlocked.Increment(ref blocking); gate.Recv(); finished.Done(); return default; });

        // Wait until the count of blocked goroutines stops growing (or every one is blocked).
        int last = -1;
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < TimeSpan.FromSeconds(120))
        {
            Thread.Sleep(500);
            int now = Volatile.Read(ref blocking);
            if (now == max || now == last) break;
            last = now;
        }

        int held = Volatile.Read(ref blocking);
        self.Refresh();
        int threadsAtMax = self.Threads.Count;
        int failures = s.CarrierStartFailures;

        gate.Close();
        bool allFinished = finished.Wait(TimeSpan.FromSeconds(120));
        s.Stop();

        Console.WriteLine($"READ G2-BUDGET rid={System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier} kern.num_taskthreads={TaskThreadCap()} procs={s.Options.Procs} threads-at-start={threadsAtStart} concurrent-untwinned-blocks={held}/{max} threads-at-max={threadsAtMax} carrier-start-failures={failures} all-finished={allFinished}");
        Assert.IsTrue(held > 0, "no goroutine blocked: the arm measured nothing");
    }

    [TestMethod]
    public void TwinParks100kOnThePrototype()
    {
        var s = new Scheduler(new Options());
        var release = new Chan<ValueTuple>(s);
        var parked = new WaitGroup(s);
        var finished = new WaitGroup(s);
        parked.Add(Count);
        finished.Add(Count);
        var sw = Stopwatch.StartNew();

        for (int i = 0; i < Count; i++)
            s.Go(async () => { parked.Done(); await release.RecvAsync(); finished.Done(); });

        bool allParked = parked.Wait(TimeSpan.FromSeconds(120));
        double seconds = sw.Elapsed.TotalSeconds;
        Process self = Process.GetCurrentProcess();
        self.Refresh();
        int threads = self.Threads.Count;
        long privateMb = self.PrivateMemorySize64 / (1024 * 1024);
        int carriers = s.CarrierCount;
        long live = s.Live;

        release.Close();
        bool allFinished = finished.Wait(TimeSpan.FromSeconds(120));
        s.Stop();

        Console.WriteLine($"READ G2-TWIN rid={System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier} kern.num_taskthreads={TaskThreadCap()} parked={(allParked ? Count : -1)}/{Count} in {seconds:F2}s live={live} carriers={carriers} procs={s.Options.Procs} process-threads={threads} private={privateMb}MB finished={allFinished}");
        Assert.IsTrue(allParked, $"only some of {Count} twinned goroutines parked within 120 s");
        Assert.IsTrue(allFinished, "the released goroutines did not all finish");
        Assert.IsTrue(carriers <= s.Options.Procs * 4, $"{carriers} carriers for {Count} parked goroutines: parking held threads");
    }
}
