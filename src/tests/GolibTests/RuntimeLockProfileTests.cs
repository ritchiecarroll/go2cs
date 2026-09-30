using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// Guards runtime-internal lock contention in the MUTEX PROFILE (census A3; runtime's
/// TestRuntimeLockMetricsAndProfile/runtime.lock/sample-1). Go's lock2 charges a contended wait to the
/// M's mLockProfile (recordLock) and unlock2 captures the unlocker's stack -- starting at runtime.unlock,
/// three frames above recordUnlock -- into the mutex bucket (recordUnlock/store). The managed lock core
/// fed only the metric half (/sync/mutex/wait/total:seconds). Red while a contended runtime lock adds
/// nothing to the mutex profile.
/// </summary>
[TestClass]
public class RuntimeLockProfileTests
{
    private const int Probe = 1;

    // The count of mutex-profile records whose first NAMED frame is the given function, read the way Go's
    // own tests read them (FuncForPC(pc - 1)); records carry Go's logical-stack sentinel ahead of the frames.
    private static int64 TotalAtFirstNamed(string function)
    {
        var (n, _) = MutexProfile(default);
        var records = new slice<BlockProfileRecord>((int)n + 16, static () => new BlockProfileRecord(nil));
        var (m, ok) = MutexProfile(records);
        Assert.IsTrue(ok, "the profile must fit a buffer sized from its own count");
        int64 count = 0;
        for (int i = 0; i < (int)m; i++)
        {
            foreach (var (_, pc) in records[i].StackRecord.Stack())
            {
                string name = (string)FuncForPC(pc - 1).Name();
                if (name.Length == 0)
                    continue;
                if (name == function)
                    count += records[i].Count;
                break;
            }
        }
        return count;
    }

    // ONE contended acquire of the probe, through Go's own runtime.lock/runtime.unlock: a holder keeps it
    // 50 ms while this thread waits in lock2's slow path. Returns the wait the metric half charged, the
    // WITNESS that the acquire really was contended (so a red cannot be a fixture that measured nothing).
    private static long ContendProbeOnce()
    {
        long waitBefore = GoTotalMutexWaitTimeNanos();
        using var held = new ManualResetEventSlim();
        var holder = new Thread(() =>
        {
            GoRuntimeLockProbeLockGo(Probe);
            held.Set();
            Thread.Sleep(50);
            GoRuntimeLockProbeUnlockGo(Probe);
        });
        holder.Start();
        held.Wait();
        GoRuntimeLockProbeLockGo(Probe);     // contended: the holder keeps it for 50 ms
        GoRuntimeLockProbeUnlockGo(Probe);
        holder.Join();
        return GoTotalMutexWaitTimeNanos() - waitBefore;
    }

    private static void WithRuntimeLockProfiling(int contentionStacks, System.Action body)
    {
        nint previousFraction = SetMutexProfileFraction(1);
        int previousStacks = GoSetRuntimeContentionStacks(contentionStacks);
        try
        {
            body();
        }
        finally
        {
            GoSetRuntimeContentionStacks(previousStacks);
            SetMutexProfileFraction(previousFraction);
        }
    }

    [TestMethod]
    public void AContendedRuntimeLockAddsOneMutexSampleAtRuntimeUnlock()
    {
        WithRuntimeLockProfiling(1, () =>
        {
            int64 before = TotalAtFirstNamed("runtime.unlock");

            long waited = ContendProbeOnce();

            Assert.IsTrue(waited >= 40_000_000, $"the probe acquire was not contended (the wait metric grew {waited} ns, the holder kept it 50 ms): the arm measured nothing");
            Assert.AreEqual(before + 1, TotalAtFirstNamed("runtime.unlock"),
                "one contended runtime lock must add exactly one mutex-profile sample whose stack starts at runtime.unlock");
        });
    }

    // GODEBUG runtimecontentionstacks=0, Go's default: the event is still counted, under the placeholder
    // frame runtime._LostContendedRuntimeLock instead of the unlocker's stack (mprof.go captureStack).
    [TestMethod]
    public void WithContentionStacksOffTheSampleSitsAtTheLostPlaceholder()
    {
        WithRuntimeLockProfiling(0, () =>
        {
            int64 before = TotalAtFirstNamed("runtime._LostContendedRuntimeLock");
            int64 unlockBefore = TotalAtFirstNamed("runtime.unlock");

            long waited = ContendProbeOnce();

            Assert.IsTrue(waited >= 40_000_000, $"the probe acquire was not contended (the wait metric grew {waited} ns): the arm measured nothing");
            Assert.AreEqual(before + 1, TotalAtFirstNamed("runtime._LostContendedRuntimeLock"),
                "with runtimecontentionstacks=0 the contended runtime lock's sample must sit at runtime._LostContendedRuntimeLock");
            Assert.AreEqual(unlockBefore, TotalAtFirstNamed("runtime.unlock"),
                "with runtimecontentionstacks=0 no sample may carry the unlocker's stack");
        });
    }

    // CONTROL for the reader: sync.Mutex contention (class F (i), already modeled) is visible through the
    // same TotalAtFirstNamed, so a red on the arms above is the runtime-lock half missing, not a reader
    // that cannot see mutex-profile records at all. Green at the base.
    [TestMethod]
    public void ControlTheReaderSeesASyncMutexHandoff()
    {
        WithRuntimeLockProfiling(1, () =>
        {
            int64 before = TotalAtFirstNamed("sync.(*Mutex).Unlock");
            var box = new StrongBox<sync_package.Mutex>();
            using var held = new ManualResetEventSlim();
            var holder = new Thread(() =>
            {
                box.Value.Lock();
                held.Set();
                Thread.Sleep(50);
                box.Value.Unlock();
            });
            holder.Start();
            held.Wait();
            box.Value.Lock();
            box.Value.Unlock();
            holder.Join();

            Assert.AreEqual(before + 1, TotalAtFirstNamed("sync.(*Mutex).Unlock"),
                "the reader must see one sync.Mutex handoff at sync.(*Mutex).Unlock");
        });
    }
}
