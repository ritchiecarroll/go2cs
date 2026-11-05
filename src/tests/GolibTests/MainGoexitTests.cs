// MainGoexitTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

// runtime.Goexit from the MAIN goroutine (docs/DESIGN-goexit-main.md, COORD's D1 rulings). Main has no goroutine root,
// and an exception nothing catches runs no finally, so every deferring frame catches a from-main Goexit through
// GoFrame.IsPanic's marker, runs its deferred calls with recover() reading nil, and raises a fresh from-main Goexit
// at its end. A goroutine's Goexit keeps failing the filter. The process-level arms (the park, the two fatal errors,
// os.Exit and a panic after it, iter.Pull) are the behavioral GoexitFromMain.
//
// Each frame test runs on a thread of its own: GoexitException marks its thread as exiting (GoexitException.Started),
// which must not leak into the next test on a shared thread.
[TestClass]
public class MainGoexitTests
{
    private static Exception? OnFreshThread(Action body)
    {
        Exception? escaped = null;

        Thread thread = new(() =>
        {
            try
            {
                body();
            }
            catch (Exception ex)
            {
                escaped = ex;
            }
        });

        thread.Start();
        thread.Join();
        return escaped;
    }

    // func f() { defer deferred(); runtime.Goexit() } on main, in the emitted shape.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void deferringFrame(Action deferred, Action body)
    {
        GoFrame ᒐ = default;
        try {
            defer(deferred, ref ᒐ);
            body();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    private static void mainGoexit() => throw new GoexitException(fromMain: true);

    [TestMethod]
    public void AFrameRunsItsDeferredCallsAndRaisesMainsGoexitAgain()
    {
        bool ran = false;
        object? recovered = "not called";

        Exception? escaped = OnFreshThread(() => deferringFrame(() =>
        {
            ran = true;
            recovered = recover();
        }, mainGoexit));

        Assert.IsTrue(ran, "the frame's deferred call must run");
        Assert.IsNull(recovered, "recover() reads nil while a Goexit runs the deferred calls");
        Assert.IsInstanceOfType(escaped, typeof(GoexitException), "the Goexit must leave the frame, not end in its catch");
        Assert.IsTrue(((GoexitException)escaped!).FromMain, "it leaves the frame still marked as main's");
    }

    // Go runs the deferred calls of every frame on main's stack, innermost first (the design's arm (e)).
    [TestMethod]
    public void EveryFrameOnTheStackRunsItsDeferredCallsInnermostFirst()
    {
        List<string> order = [];

        Exception? escaped = OnFreshThread(() =>
            deferringFrame(() => order.Add("outer"), () =>
                deferringFrame(() => order.Add("inner"), mainGoexit)));

        CollectionAssert.AreEqual(new[] { "inner", "outer" }, order);
        Assert.IsTrue(escaped is GoexitException { FromMain: true }, $"escaped: {escaped}");
    }

    // The marker answers only for main: a goroutine's Goexit still fails the filter and unwinds to its root, exactly
    // as before (GoexitDefers is the behavioral control).
    [TestMethod]
    public void AGoroutinesGoexitStillFailsTheFilter()
    {
        Assert.IsFalse(GoFrame.IsPanic(new GoexitException(), out _), "a goroutine's Goexit is not caught by a deferring frame");
        Assert.IsTrue(GoFrame.IsPanic(new GoexitException(fromMain: true), out PanicException? marker), "main's is");
        Assert.IsNull(marker!.State, "the marker carries no value a recover() could return");
    }

    // A deferred call that panics while main's Goexit runs: recovered, the Goexit goes on (Go's
    // TestRecoveredPanicAfterGoexit); unrecovered, the panic wins.
    [TestMethod]
    public void APanicInADeferredCallDecidesWhetherTheGoexitGoesOn()
    {
        object? recovered = null;

        Exception? escaped = OnFreshThread(() =>
        {
            GoFrame ᒐ = default;
            try {
                defer(() => recovered = recover(), ref ᒐ);
                defer(() => throw panic("in a deferred call"), ref ᒐ);
                mainGoexit();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });

        Assert.AreEqual("in a deferred call", recovered?.ToString(), "the earlier-registered deferred call recovers the panic");
        Assert.IsTrue(escaped is GoexitException { FromMain: true }, $"a recovered panic lets main's Goexit go on; escaped: {escaped}");

        escaped = OnFreshThread(() => deferringFrame(() => throw panic("unrecovered"), mainGoexit));

        Assert.IsInstanceOfType(escaped, typeof(PanicException), "an unrecovered panic in a deferred call replaces the Goexit");
    }

    // The park's two entry points see main's Goexit through what wrapped it: the test host's Task (an
    // AggregateException) and reflection.
    [TestMethod]
    public void MainGoexitIsSeenThroughItsWrappers()
    {
        GoexitException main = new(fromMain: true);

        Assert.IsTrue(MainGoexit.Is(main));
        Assert.IsTrue(MainGoexit.Is(new AggregateException(main)));
        Assert.IsTrue(MainGoexit.Is(new TargetInvocationException(new AggregateException(main))));
        Assert.IsFalse(MainGoexit.Is(new GoexitException()), "a goroutine's Goexit is not main's");
        Assert.IsFalse(MainGoexit.Is(new InvalidOperationException()));
        Assert.IsFalse(MainGoexit.Is(null));
    }

    // The dead-program check with main ended by Goexit: main counts as blocked for good, so the goroutines left decide
    // alone (the design's step 5). The rows beside ForeverWaitDeadlockDecisionTests' table.
    [TestMethod]
    public void AnExitedMainLeavesTheDecisionToTheGoroutinesLeft()
    {
        static bool Decide(params (WaitReason Reason, bool Readied, bool IsMain)[] users) =>
            Goroutine.IsProvableDeadlock(users, mainExited: true);

        Assert.IsTrue(Decide((WaitReason.SelectNoCases, false, false)), "the one goroutine left in select {}");
        Assert.IsTrue(Decide((WaitReason.ChanReceiveNilChan, false, false), (WaitReason.ChanSendNilChan, false, false)), "every goroutine left in a forever wait");
        Assert.IsFalse(Decide((WaitReason.SelectNoCases, false, false), (WaitReason.Zero, false, false)), "a goroutine left that can still run");
        Assert.IsFalse(Decide((WaitReason.SelectNoCases, true, false)), "a goroutine already readied");
        Assert.IsFalse(Decide((WaitReason.ChanReceive, false, false)), "the named residual: an ordinary channel wait blocks instead of reporting");
        Assert.IsFalse(Goroutine.IsProvableDeadlock([(WaitReason.SelectNoCases, false, false)]), "control: with main not exited, no main among them is no deadlock");
    }
}
