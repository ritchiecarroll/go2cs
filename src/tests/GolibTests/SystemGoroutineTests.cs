using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;

namespace GolibTests;

// The runtime's own goroutines are SYSTEM goroutines: omitted from runtime.Stack(all) and not counted
// by runtime.NumGoroutine -- Go's isSystemGoroutine (runtime/traceback.go) and gcount's sched.ngsys.
//
// Measured on the net/http row (2026-09-04): once (B) named creators, the one goroutine its leak check
// kept was `created by runtime.unique_runtime_registerUniqueMapCleanup` -- Go's unique map-cleanup
// goroutine, started inside the runtime precisely "so it's counted as a system goroutine" (mgc.go).
// Go's Stack(all) never prints it (tracebackothers skips system goroutines below GOTRACEBACK=system)
// and NumGoroutine never counts it; this runtime did both, because nothing classified a goroutine
// until its creator was recorded. The classification is the creator's package (runtime), decided at
// registration; a system counter beside the registry's total gives UserCount, which is what
// NumGoroutine and gcount now return.
[TestClass]
public class SystemGoroutineTests
{
    private static string CaptureStack(bool all)
    {
        byte[] storage = new byte[256 * 1024];
        slice<byte> buf = new(storage);
        nint written = runtime_package.Stack(buf, all);

        return Encoding.UTF8.GetString(storage, 0, (int)written);
    }

    // A real function of the converted runtime package -- the creator every runtime-started goroutine
    // carries. Any public static will do: the predicate reads the PACKAGE, never the name.
    private static MethodBase RuntimeMethod() =>
        typeof(runtime_package).GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => !m.IsGenericMethodDefinition);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AUserFunction() { }

    // THE PREDICATE. Go's rule is the start function's package: `runtime.` is system, anything else is
    // user, and no creator at all (the main goroutine, a host-entered thread) is user.
    [TestMethod]
    public void ThePredicateIsTheCreatorsPackage()
    {
        Assert.IsTrue(Goroutine.IsSystemCreator(RuntimeMethod()), "a runtime-package creator must classify as system");
        Assert.IsFalse(Goroutine.IsSystemCreator(typeof(SystemGoroutineTests).GetMethod(nameof(AUserFunction), BindingFlags.NonPublic | BindingFlags.Static)),
            "a creator outside the runtime package must classify as user");
        Assert.IsFalse(Goroutine.IsSystemCreator(null), "no creator is a user goroutine, as runtime.main is in Go");
    }

    // THE GUARD. A goroutine registered with a runtime creator is absent from Stack(all) and from
    // UserCount while the registry's own Count still sees it; a user goroutine parked the same way
    // renders, with its created-by line. Positive control (measured at the cut): the predicate
    // neutered to false -> the system block renders and UserCount moves -> RED.
    [TestMethod]
    public void ASystemGoroutineIsOmittedFromStackAllAndNotCounted() => AssertSystemGoroutineOmitted(afterBaseline: null);

    // An earlier test's goroutine retiring inside this one's window (StragglerStaging), staged.
    [TestMethod]
    public void ASystemGoroutineIsNotCountedWhileAnEarlierGoroutineRetires()
    {
        for (int i = 0; i < StragglerStaging.Iterations; i++)
            AssertSystemGoroutineOmitted(StragglerStaging.Stage());
    }

    private static void AssertSystemGoroutineOmitted(Action? afterBaseline)
    {
        channel<int> park = new(0);

        // Exact, and by identity (StragglerStaging): the ONE goroutine minted is the guard, and the
        // system count rises by exactly one for it. A user goroutine retiring meanwhile moves Count and
        // UserCount together and leaves the system count alone, so neither assertion can be falsified
        // by an earlier test's straggler; the old Count/UserCount deltas both could.
        HashSet<long> live = StragglerStaging.LiveIds();
        int systemBefore = SystemCount();
        afterBaseline?.Invoke();

        Goroutine.StartForGuard(() => park.Receive(), RuntimeMethod());

        Goroutine? guard = null;

        for (int i = 0; i < 400 && guard is not { State: GoroutineState.Parked }; i++)
        {
            guard = StragglerStaging.Minted(live).FirstOrDefault(g => g.IsSystem);

            if (guard is not { State: GoroutineState.Parked })
                System.Threading.Thread.Sleep(5);
        }

        try
        {
            List<Goroutine> minted = StragglerStaging.Minted(live);
            Assert.AreEqual(1, minted.Count, "exactly one goroutine must be minted, the system goroutine");
            Assert.IsTrue(minted[0].IsSystem, "the registry must hold the guard as a system goroutine");
            Assert.AreEqual(systemBefore + 1, SystemCount(), "a system goroutine must be counted as system, not user (runtime.NumGoroutine)");

            string dump = CaptureStack(all: true);

            Console.WriteLine(dump);

            Assert.IsFalse(dump.Contains("created by runtime.", StringComparison.Ordinal),
                "a system goroutine was rendered by runtime.Stack(all); Go's tracebackothers omits it below GOTRACEBACK=system:\n" + dump);
            Assert.IsTrue(Goroutine.Snapshot().Any(g => g.IsSystem), "the system goroutine must exist in the registry snapshot even though the traceback omits it");
        }
        finally
        {
            park.Close();

            // Leave no straggler of our own: the guard retires before this returns.
            if (guard is not null)
                System.Threading.SpinWait.SpinUntil(() => Goroutine.FromId(guard.Id) is null, 30000);
        }
    }

    // Count - UserCount, the registry's system count, read so a user goroutine retiring between the two
    // reads cannot skew it: Count is re-read after UserCount and the pair retried until it held still.
    private static int SystemCount()
    {
        while (true)
        {
            int count = Goroutine.Count;
            int user = Goroutine.UserCount;

            if (Goroutine.Count == count)
                return count - user;
        }
    }

    // The symmetric half: NumGoroutine's own body reports UserCount with Go's floor. Asserted as a
    // RELATION at a moment the system goroutine is registered -- Count above UserCount, NumGoroutine
    // equal to the floored UserCount -- rather than as a before/after difference, which a goroutine
    // from a sibling test exiting in the same window can absorb (measured: the neutered predicate's
    // +1 was cancelled by exactly such an exit and the arm read green against a broken predicate).
    [TestMethod]
    public void NumGoroutineReportsUserGoroutinesWithGosFloor()
    {
        channel<int> park = new(0);
        HashSet<long> live = StragglerStaging.LiveIds();

        Goroutine.StartForGuard(() => park.Receive(), RuntimeMethod());

        for (int i = 0; i < 400 && !Goroutine.Snapshot().Any(g => g.IsSystem && g.State == GoroutineState.Parked); i++)
            System.Threading.Thread.Sleep(5);

        try
        {
            Assert.IsTrue(Goroutine.Count > Goroutine.UserCount, "a registered system goroutine must be in Count and not in UserCount");
            Assert.AreEqual(Math.Max(1, Goroutine.UserCount), (int)runtime_package.NumGoroutine(), "NumGoroutine must report UserCount with Go's floor");
        }
        finally
        {
            park.Close();

            // Leave no straggler of our own: a SYSTEM goroutine retiring inside a later test's window
            // would move the system count that ASystemGoroutineIsOmittedFromStackAllAndNotCounted asserts.
            System.Threading.SpinWait.SpinUntil(() => !StragglerStaging.Minted(live).Any(g => g.IsSystem), 30000);
        }
    }
}
