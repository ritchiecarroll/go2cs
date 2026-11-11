using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

// golib's one timer heap (GoTimer), which internal/poll's deadlines ride (G's approval, 2026-10-19): deadline
// order, a reset moves a timer in place, a stop releases it, a firing is close to its due time, the service
// thread is idle under a far-future timer, and a callback that blocks is reported by name.
[TestClass]
[DoNotParallelize]
public class GoTimerTests
{
    private static long now() => MonotonicClock.Nanoseconds();

    private const long Millisecond = 1_000_000;

    [TestMethod]
    public void TimersFireInDeadlineOrder()
    {
        ConcurrentQueue<int> order = new();
        using CountdownEvent done = new(3);
        long start = now();

        foreach (int ms in new[] { 30, 10, 20 })
            new GoTimer(state => { order.Enqueue((int)state!); done.Signal(); }, ms).Reset(start + ms * Millisecond);

        Assert.IsTrue(done.Wait(5_000), "three timers did not all fire");
        CollectionAssert.AreEqual(new[] { 10, 20, 30 }, order.ToArray());
    }

    [TestMethod]
    public void AResetMovesTheArmedTimerAndItFiresOnce()
    {
        int fired = 0;
        using ManualResetEventSlim done = new();
        int before = GoTimer.Count;

        GoTimer timer = new(_ => { Interlocked.Increment(ref fired); done.Set(); }, null);

        Assert.IsFalse(timer.Reset(now() + 3_600_000 * Millisecond), "a new timer reported itself armed");
        Assert.IsTrue(timer.Reset(now() + 5 * Millisecond), "a reset of an armed timer reported it unarmed");

        Assert.IsTrue(done.Wait(5_000), "the reset timer did not fire");
        Thread.Sleep(50);

        Assert.AreEqual(1, Volatile.Read(ref fired));
        Assert.AreEqual(before, GoTimer.Count, "a fired timer stayed in the heap");
    }

    // G's condition 1: net/http resets a connection's deadlines on every request, so an armed timer must be in
    // the heap once however many resets it has seen.
    [TestMethod]
    public void AHundredThousandResetsLeaveOneEntry()
    {
        int before = GoTimer.Count;
        GoTimer timer = new(_ => { }, null);
        long far = now() + 3_600_000 * Millisecond;

        for (int i = 0; i < 100_000; i++)
            timer.Reset(far + i % 1_000);

        Assert.AreEqual(before + 1, GoTimer.Count);

        Assert.IsTrue(timer.Stop());
        Assert.AreEqual(before, GoTimer.Count);
    }

    // G's condition 1, second half: a stopped timer holds nothing in the queue, so a closed descriptor's state
    // is collectable. The control is a timer left armed: nothing but the heap names it, and the heap keeps its state
    // alive.
    [TestMethod]
    public void StopReleasesTheCallbackAndItsState()
    {
        (WeakReference armedState, WeakReference armedTimer) = arm(stop: false);
        (WeakReference stoppedState, _) = arm(stop: true);

        collect();

        Assert.IsTrue(armedState.IsAlive, "the control: an armed timer's state must stay reachable from the heap");
        Assert.IsFalse(stoppedState.IsAlive, "a stopped timer's state is still reachable");

        Assert.IsTrue(((GoTimer)armedTimer.Target!).Stop());
    }

    // Only the returned weak references name the timer and its state once this returns.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference state, WeakReference timer) arm(bool stop)
    {
        object state = new();
        GoTimer timer = new(_ => { }, state);
        timer.Reset(now() + 3_600_000 * Millisecond);

        if (stop)
            Assert.IsTrue(timer.Stop());

        return (new WeakReference(state), new WeakReference(timer));
    }

    private static void collect()
    {
        for (int i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    [TestMethod]
    public void AStoppedTimerDoesNotFire()
    {
        int fired = 0;
        GoTimer timer = new(_ => Interlocked.Increment(ref fired), null);
        timer.Reset(now() + 20 * Millisecond);

        Assert.IsTrue(timer.Stop());
        Assert.IsFalse(timer.Stop(), "a second stop found the timer armed");

        Thread.Sleep(80);
        Assert.AreEqual(0, Volatile.Read(ref fired));
    }

    [TestMethod]
    public void ATimeAlreadyPastFiresAtOnce()
    {
        using ManualResetEventSlim done = new();
        new GoTimer(_ => done.Set(), null).Reset(now() - Millisecond);

        Assert.IsTrue(done.Wait(1_000));
    }

    // netpoll's deadlineFired re-arms its own timer from inside the callback; the callback runs with no queue
    // lock held, so that must not deadlock.
    [TestMethod]
    public void ACallbackRearmsItsOwnTimer()
    {
        int fired = 0;
        using ManualResetEventSlim done = new();
        GoTimer? timer = null;

        timer = new GoTimer(_ =>
        {
            if (Interlocked.Increment(ref fired) == 1)
                timer!.Reset(now() + 2 * Millisecond);
            else
                done.Set();
        }, null);

        timer.Reset(now() + 2 * Millisecond);

        Assert.IsTrue(done.Wait(5_000));
        Assert.AreEqual(2, Volatile.Read(ref fired));
    }

    // The reason the queue exists: a 5 ms deadline fired on the windows tick read 10-13 ms late. The median of
    // twenty firings stays under the probe's bar for the I/O rows.
    //
    // On macOS the bar is RELATIVE to the host (G, 2026-11-10): each firing is paired with the test's own kernel
    // wait over the same 5 ms, taken on this thread while the timer is armed, and the median of the per-pair
    // difference stays under the bar. The hosted osx-arm64 runner serves a 5 ms wait anywhere from on time to about
    // 2.6 ms late, for Go's own runtime as well, so the absolute bar read the host there and failed 3 of 13 full
    // runs. The pairs do not: in one full run the host's wait read 0.069 to 2.602 ms late and every GoTimer firing
    // stood within 0.103 ms of its pair. A timer that loses the short wait is later than its pair by the coarse
    // wait's whole error, which is what the bar is for. Where the host's wait gives no reading (linux, windows, or
    // a wait unanswered) the absolute bar holds.
    //
    // What this test can see, measured with GoTimer forced back to the coarse wait (2026-11-11): on osx-arm64 the
    // median stood 13.3 and 15.2 ms beyond the host's wait and the test failed, 2 runs of 2. On osx-x64 it PASSED,
    // 2 runs of 2: the coarse wait is late in only 6 or 7 of twenty firings there, so the median stays under the
    // bar, relative or absolute. This test is the osx-arm64 arm for a lost short wait; on osx-x64 the arm that saw
    // it was TimersFireInDeadlineOrder, in 1 run of 2.
    [TestMethod]
    public void AFiringIsCloseToItsDueTime()
    {
        List<double> late = new();
        List<double> hostLate = new();

        for (int i = 0; i < 20; i++)
        {
            long firedAt = 0;
            using ManualResetEventSlim done = new();
            long due = now() + 5 * Millisecond;

            new GoTimer(_ => { firedAt = now(); done.Set(); }, null).Reset(due);

            long hostStart = now();

            if (kernelWait(5 * Millisecond))
                hostLate.Add((now() - hostStart - 5 * Millisecond) / 1e6);

            Assert.IsTrue(done.Wait(5_000));
            late.Add((firedAt - due) / 1e6);
        }

        double? beyondHostMedian = null;

        if (hostLate.Count == late.Count)
        {
            List<double> beyondHost = late.Select((value, i) => value - hostLate[i]).ToList();

            Console.WriteLine($"pairs host/GoTimer, ms: {string.Join(" ", late.Select((value, i) => $"{hostLate[i]:F3}/{value:F3}"))}");

            hostLate.Sort();
            beyondHost.Sort();
            beyondHostMedian = beyondHost[beyondHost.Count / 2];

            Console.WriteLine($"host 5 ms kernel wait lateness, ms: min {hostLate[0]:F3} median {hostLate[hostLate.Count / 2]:F3} max {hostLate[^1]:F3}");
            Console.WriteLine($"GoTimer beyond the host's wait, per pair, ms: min {beyondHost[0]:F3} median {beyondHostMedian:F3} max {beyondHost[^1]:F3}");
        }
        else if (OperatingSystem.IsMacOS())
        {
            Console.WriteLine($"host 5 ms kernel wait: {hostLate.Count} of {late.Count} waits answered, no pairs: the absolute bar holds");
        }

        late.Sort();
        double median = late[late.Count / 2];

        Console.WriteLine($"GoTimer 5 ms lateness, ms: min {late[0]:F3} median {median:F3} max {late[^1]:F3}");
        Assert.IsTrue(late[0] >= 0, $"a timer fired {-late[0]:F3} ms EARLY");

        if (beyondHostMedian is double beyond)
            Assert.IsTrue(beyond < 1.5, $"median lateness beyond the host's own wait {beyond:F3} ms (absolute median {median:F3} ms)");
        else
            Assert.IsTrue(median < 1.5, $"median lateness {median:F3} ms");
    }

    // The host's own short wait on macOS: a one-shot kqueue EVFILT_TIMER with NOTE_CRITICAL (sys/event.h), made and
    // closed per wait. It is the test's own, not GoCriticalTimer's, so a change to golib's wait cannot move both
    // sides of a pair. False where there is no reading to take: not macOS, kqueue refused, or the wait cut short.
    private static bool kernelWait(long nanoseconds)
    {
        if (!OperatingSystem.IsMacOS())
            return false;

        int kq = kqueue();

        if (kq < 0)
            return false;

        try
        {
            Kevent change = new()
            {
                Ident = 1,
                Filter = EvfiltTimer,
                Flags = EvAdd | EvOneshot,
                Fflags = NoteUseconds | NoteCritical,
                Data = (nint)(nanoseconds / 1000)
            };

            return kevent(kq, ref change, 1, out Kevent fired, 1, 0) == 1 && (fired.Flags & EvError) == 0;
        }
        finally
        {
            close(kq);
        }
    }

    // struct kevent (sys/event.h), 64-bit: uintptr_t ident; int16_t filter; uint16_t flags; uint32_t fflags;
    // intptr_t data; void *udata.
    [StructLayout(LayoutKind.Sequential)]
    private struct Kevent
    {
        public nuint Ident;
        public short Filter;
        public ushort Flags;
        public uint Fflags;
        public nint Data;
        public nint Udata;
    }

    private const short EvfiltTimer = -7;
    private const ushort EvAdd = 0x0001;
    private const ushort EvOneshot = 0x0010;
    private const ushort EvError = 0x4000;
    private const uint NoteUseconds = 0x00000002;
    private const uint NoteCritical = 0x00000020;

    [DllImport("libc")]
    private static extern int kqueue();

    [DllImport("libc")]
    private static extern int kevent(int kq, ref Kevent changelist, int nchanges, out Kevent eventlist, int nevents, nint timeout);

    [DllImport("libc")]
    private static extern int close(int fd);

    // G's condition 3: with one far-future timer armed the service thread is blocked, not spinning. A thread spinning
    // for the head timer would burn about a whole second of CPU over the second.
    [TestMethod]
    public void TheServiceThreadIsIdleUnderAFarFutureTimer()
    {
        ProcessThread service = serviceThread();
        GoTimer timer = new(_ => { }, null);
        timer.Reset(now() + 3_600_000 * Millisecond);
        Thread.Sleep(50);

        double cpuMilliseconds = cpuOver(service, () => Thread.Sleep(1_000));
        timer.Stop();

        Console.WriteLine($"timer service thread CPU over 1 s with one far-future GoTimer armed: {cpuMilliseconds:F1} ms");
        Assert.IsTrue(cpuMilliseconds < 20, $"{cpuMilliseconds:F1} ms of service-thread CPU in 1 s idle");
    }

    // G's review: the idle arm cannot see what the final sub-millisecond wait costs, so 500 timers 2 ms apart (a second
    // of dense firings) bound it. A service thread that spun out each timer's last millisecond would burn about half a
    // second here.
    [TestMethod]
    public void DenseTimersCostTheServiceThreadLittleCpu()
    {
        const int Timers = 500;
        ProcessThread service = serviceThread();
        using CountdownEvent done = new(Timers);

        double cpuMilliseconds = cpuOver(service, () =>
        {
            long start = now() + 10 * Millisecond;

            for (int i = 1; i <= Timers; i++)
                new GoTimer(_ => done.Signal(), null).Reset(start + i * 2 * Millisecond);

            Assert.IsTrue(done.Wait(10_000), "the dense timers did not all fire");
        });

        Console.WriteLine($"timer service thread CPU over 500 timers 2 ms apart (1 s): {cpuMilliseconds:F1} ms");
        Assert.IsTrue(cpuMilliseconds < 100, $"{cpuMilliseconds:F1} ms of service-thread CPU for a second of dense timers");
    }

    // The service thread, found by the OS thread id a callback reads on it (callbacks run inline there).
    private static ProcessThread serviceThread()
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux())
            Assert.Inconclusive("reads a thread's CPU time by its OS thread id, which this platform's ProcessThread does not carry");

        int id = 0;
        using ManualResetEventSlim done = new();
        new GoTimer(_ => { id = osThreadId(); done.Set(); }, null).Reset(now());
        Assert.IsTrue(done.Wait(5_000));

        using Process self = Process.GetCurrentProcess();
        return self.Threads.Cast<ProcessThread>().Single(thread => thread.Id == id);
    }

    private static double cpuOver(ProcessThread thread, Action window)
    {
        TimeSpan before = thread.TotalProcessorTime;
        window();
        return (thread.TotalProcessorTime - before).TotalMilliseconds;
    }

    private static int osThreadId() => OperatingSystem.IsWindows() ? (int)GetCurrentThreadId() : gettid();

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("libc")]
    private static extern int gettid();

    // G's condition 2: callbacks run inline and must not block; one that does is reported by name.
    [TestMethod]
    public void ABlockingCallbackIsReportedByName()
    {
        (long before, _) = GoTimer.SlowCallbacks;

        // The control: a callback inside the bound leaves the counter where it was.
        using (ManualResetEventSlim fast = new())
        {
            new GoTimer(state => ((ManualResetEventSlim)state!).Set(), fast).Reset(now());
            Assert.IsTrue(fast.Wait(5_000));
            Thread.Sleep(50);
            Assert.AreEqual(before, GoTimer.SlowCallbacks.count, "a fast callback was reported as blocking");
        }

        using ManualResetEventSlim done = new();

        new GoTimer(blockingCallback, done).Reset(now());

        Assert.IsTrue(done.Wait(5_000));
        Thread.Sleep(50);

        (long after, string? last) = GoTimer.SlowCallbacks;
        Assert.AreEqual(before + 1, after);
        Assert.AreEqual($"{typeof(GoTimerTests).FullName}.{nameof(blockingCallback)}", last);
    }

    private static void blockingCallback(object? state)
    {
        Thread.Sleep(GoTimer.SlowCallbackMilliseconds * 4);
        ((ManualResetEventSlim)state!).Set();
    }
}
