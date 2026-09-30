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

    [TestMethod]
    public void AContendedRuntimeLockAddsOneMutexSampleAtRuntimeUnlock()
    {
        nint previousFraction = SetMutexProfileFraction(1);
        int previousStacks = GoSetRuntimeContentionStacks(1);
        try
        {
            int64 before = TotalAtFirstNamed("runtime.unlock");

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

            Assert.AreEqual(before + 1, TotalAtFirstNamed("runtime.unlock"),
                "one contended runtime lock must add exactly one mutex-profile sample whose stack starts at runtime.unlock");
        }
        finally
        {
            GoSetRuntimeContentionStacks(previousStacks);
            SetMutexProfileFraction(previousFraction);
        }
    }
}
