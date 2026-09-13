using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using Δruntime = go.runtime_package;

namespace GolibTests;

/// <summary>
/// The acceptance row for <c>runtime.AddCleanup</c> — the Go 1.24 API whose auto conversion was a
/// SILENT NO-OP (COORD ruling <c>c58b4c01d</c>, finding C1 <c>f9f41e8d8</c> §3).
/// </summary>
/// <remarks>
/// <para>
/// WHAT WENT WRONG IN THE AUTO, and why a row exists for an API nothing calls yet.
/// <c>mcleanup.go</c>'s <c>AddCleanup</c> calls <c>createfing()</c>, which in the converted corpus
/// started the CONVERTED <c>runfinq</c> — the body <c>mfinal.cs</c>'s own header declares dead. Taken
/// as a plain auto, <c>runtime.AddCleanup</c> would compile, return a <c>Cleanup</c>, and never run
/// it: no throw, no diagnostic, nothing in the system able to say why. "Compiling is not
/// correctness" has no sharper instance, so the hand-own that replaces it is not believed without a
/// row that would have gone red on the auto.
/// </para>
/// <para>
/// ⚠ WHAT THESE ARMS CANNOT SEE, STATED HERE RATHER THAN IMPLIED BY THEIR PASSING. Arm 1 proves a
/// cleanup body reaches the live runner, but it CANNOT prove that <c>AddCleanup</c> is what started
/// that runner: MSTest runs one process, and any <c>SetFinalizer</c> anywhere in this assembly —
/// <c>FinalizerDispatchTests</c> runs several — has already called
/// <c>GoFinalizerQueue.EnsureRunner()</c> by then. So on the OLD <c>createfing</c> these arms would
/// still pass whenever a finalizer test ran first, and go red only in a process that uses cleanups
/// alone. The rewire's guard is therefore arm 1 PLUS the fact that <c>createfing</c> has exactly one
/// body; closing the gap properly needs a single-test process or a source-level guard, and is
/// offered as one rather than quietly assumed away.
/// </para>
/// <para>
/// ISOLATION follows <c>FinalizerDispatchTests</c>: every arm mints and drops its referent on a
/// DEDICATED THREAD that is joined before anything is measured, so no caller frame can root the box
/// and no two arms can contaminate one another through a shared stack.
/// </para>
/// </remarks>
[TestClass]
public sealed class CleanupDispatchTests
{
    private const int JoinWaitMs = 60_000;

    // How long an arm waits for a queued cleanup body to run. Cleanups are handed to the fing
    // analogue rather than invoked inline, so "collected" and "cleanup ran" are two events and the
    // second is asynchronous — polling for it is the difference between a real reading and a race
    // that passes on a fast host.
    private const int RanWaitMs = 30_000;

    // ------------------------------------------------------------------------------------------
    // Shared mint/drop machinery. Nothing here may leak the referent into a caller's frame.
    // ------------------------------------------------------------------------------------------

    // Built in its own method so its display class captures ONLY the observation cell — never the
    // box. Go's own AddCleanup contract makes the same demand of a caller: a cleanup that closes
    // over ptr keeps ptr alive and can never run.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Action<string> RecordingCleanup(List<string> seen) =>
        arg => { lock (seen) seen.Add(arg); };

    // Mints `new(*int)` in the converted shape, attaches `count` cleanups each carrying its own
    // argument, and returns only a WeakReference plus the handles. The box is never returned.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference MintAttachDrop(Action<string> cleanup, int count, bool stopThem, List<Δruntime.Cleanup> handles)
    {
        ж<ж<nint>> garbage = @new<ж<nint>>();

        for (int i = 0; i < count; i++)
            handles.Add(Δruntime.AddCleanup(garbage, cleanup, $"arg{i}"));

        if (stopThem)
        {
            // Go: "To guarantee that Stop removes the cleanup function, the caller must ensure that
            // the pointer that was passed to AddCleanup is reachable across the call to Stop." It is
            // — `garbage` is live on this frame until the line below.
            foreach (Δruntime.Cleanup handle in handles)
                handle.Stop();
        }

        WeakReference weak = new(garbage, trackResurrection: false);
        garbage = default!;
        return weak;
    }

    private static WeakReference MintOnDedicatedThread(Action<string> cleanup, int count, bool stopThem, List<Δruntime.Cleanup> handles)
    {
        WeakReference? weak = null;
        Thread minter = new(() => weak = MintAttachDrop(cleanup, count, stopThem, handles))
        {
            IsBackground = true,
            Name = "cleanup-minter"
        };
        minter.Start();
        Assert.IsTrue(minter.Join(JoinWaitMs), "the minting thread did not finish");
        Assert.IsNotNull(weak, "the minting thread produced no WeakReference — the arm measured nothing");
        return weak!;
    }

    // Collects, then polls for the queued bodies. Returns what actually ran.
    private static List<string> CollectAndDrain(List<string> seen, int expected)
    {
        Δruntime.GC();
        Δruntime.GC();

        // Poll rather than sleep-once: a fixed sleep either wastes the budget or reads a race.
        for (int waited = 0; waited < RanWaitMs; waited += 25)
        {
            lock (seen)
            {
                if (seen.Count >= expected)
                    break;
            }

            Thread.Sleep(25);
        }

        lock (seen)
            return new List<string>(seen);
    }

    // ------------------------------------------------------------------------------------------
    // ARM 1 — the row that would have gone RED on the auto conversion.
    // ------------------------------------------------------------------------------------------

    [TestMethod]
    public void Arm1_CleanupRunsWithItsArgumentAfterTheObjectDies()
    {
        List<string> seen = new();
        List<Δruntime.Cleanup> handles = new();
        WeakReference weak = MintOnDedicatedThread(RecordingCleanup(seen), count: 1, stopThem: false, handles);

        List<string> ran = CollectAndDrain(seen, expected: 1);

        Console.WriteLine($"[cleanup:arm1] IsAlive={weak.IsAlive} ran=[{string.Join(",", ran)}]");

        Assert.IsFalse(weak.IsAlive,
            "ARM 1: the object is STILL ROOTED after two runtime.GC() calls — an AddCleanup registration " +
            "is holding the very object whose death is supposed to trigger it, so the cleanup can never run.");
        CollectionAssert.AreEqual(new[] { "arg0" }, ran,
            "ARM 1: the object was collected but its cleanup never ran with its argument — this is the " +
            "silent no-op the auto conversion would have shipped (AddCleanup -> createfing -> the dead runfinq).");
    }

    // ------------------------------------------------------------------------------------------
    // ARM 2 — the differential control WITHOUT which arm 1 proves nothing.
    // ------------------------------------------------------------------------------------------

    [TestMethod]
    public void Arm2_StopPreventsTheCleanupFromRunning()
    {
        List<string> seen = new();
        List<Δruntime.Cleanup> handles = new();
        WeakReference weak = MintOnDedicatedThread(RecordingCleanup(seen), count: 1, stopThem: true, handles);

        // Give a cancelled cleanup every chance to run before concluding it did not.
        Δruntime.GC();
        Δruntime.GC();
        Thread.Sleep(250);
        List<string> ran = CollectAndDrain(seen, expected: 1);

        Console.WriteLine($"[cleanup:arm2] IsAlive={weak.IsAlive} ran=[{string.Join(",", ran)}]");

        Assert.IsFalse(weak.IsAlive, "ARM 2: a STOPPED cleanup is still rooting the object.");
        Assert.AreEqual(0, ran.Count,
            "ARM 2: Cleanup.Stop() did not cancel the cleanup — so arm 1's green says only that SOMETHING " +
            "runs cleanups, not that this mechanism is under control.");
    }

    // ------------------------------------------------------------------------------------------
    // ARM 3 — Go's "multiple cleanups may be attached to the same pointer".
    // ------------------------------------------------------------------------------------------

    [TestMethod]
    public void Arm3_EveryCleanupOnOneObjectRuns()
    {
        List<string> seen = new();
        List<Δruntime.Cleanup> handles = new();
        WeakReference weak = MintOnDedicatedThread(RecordingCleanup(seen), count: 3, stopThem: false, handles);

        List<string> ran = CollectAndDrain(seen, expected: 3);
        ran.Sort(StringComparer.Ordinal);

        Console.WriteLine($"[cleanup:arm3] IsAlive={weak.IsAlive} ran=[{string.Join(",", ran)}]");

        Assert.IsFalse(weak.IsAlive, "ARM 3: three cleanups on one object left it rooted.");
        CollectionAssert.AreEqual(new[] { "arg0", "arg1", "arg2" }, ran,
            "ARM 3: not every cleanup attached to the object ran. Go specifies no ORDER among them — this " +
            "arm sorts for exactly that reason — but it does specify that multiple may be attached, and a " +
            "one-value-per-key registry silently keeps only the last.");
    }

    // ------------------------------------------------------------------------------------------
    // ARM 4 — the guard Go itself provides, and the one place we are deliberately WIDER than Go.
    // ------------------------------------------------------------------------------------------

    [TestMethod]
    public void Arm4_AddCleanupPanicsWhenArgIsThePointer()
    {
        ж<ж<nint>> garbage = @new<ж<nint>>();

        PanicException panic = Assert.ThrowsException<PanicException>(
            () => Δruntime.AddCleanup(garbage, (ж<ж<nint>> _) => { }, garbage),
            "ARM 4: AddCleanup accepted arg == ptr. Go panics here because such a cleanup can NEVER run — " +
            "arg keeps the object alive — and accepting it is how a caller gets a cleanup that silently " +
            "never fires, which is the same class of defect as the auto conversion this row exists for.");

        StringAssert.Contains(panic.ToString(), "ptr is equal to arg",
            "ARM 4: it panicked, but not with Go's message — a reader hitting this must be able to find " +
            "Go's own documentation of the rule from the text they see.");

        Δruntime.KeepAlive(garbage);
    }

    // ------------------------------------------------------------------------------------------
    // ARM 5 — a nil pointer is refused, as Go refuses it.
    // ------------------------------------------------------------------------------------------

    [TestMethod]
    public void Arm5_AddCleanupPanicsOnANilPointer()
    {
        PanicException panic = Assert.ThrowsException<PanicException>(
            () => Δruntime.AddCleanup(default(ж<ж<nint>>)!, (string _) => { }, "unused"),
            "ARM 5: AddCleanup accepted a nil ptr.");

        StringAssert.Contains(panic.ToString(), "ptr is nil",
            "ARM 5: refused, but not with Go's message.");
    }
}
