// park_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The runtime's gopark / ready / injectglist, over golib's park seam (S1b of
// docs/phase4/DESIGN-gopark-goready-synctest.md).
//
// WHY. The converted gopark ends in mcall(park_m): switch to g0 and hand this g to the scheduler.
// The managed runtime has neither a g0 nor a scheduler -- every goroutine is its own thread -- so
// mcall stays a refusing stub ("no managed answer at this layer"), and before this file every
// converted park reached it. The runtime row died there (2026-09-22, three mcall roots, all ONE event:
// TestScavenger's harness goroutine in scavengerState.park -> goparkunlock -> gopark -> mcall).
//
// WHAT. Park the calling goroutine through golib's seam, in gopark's own order: Goroutine.Park (the g
// moves _Grunning -> _Gwaiting through the runtime's ParkTransition), THEN unlockf (Go's commit:
// releasing the lock that lets a waker find this g), THEN wait on the goroutine's own gate. ready
// resolves a g to its goroutine by goid through golib's live registry (Goroutine.FromId), readies it
// on the waker's side (Goroutine.Ready: _Gwaiting -> _Grunnable, where Go adds the mutex wait time)
// and releases its gate. injectglist is Go's batch form of the same, and it is the one the first
// reached caller uses (scavengerState.wake), which is why a gopark/goready-only pair would have missed
// it. goready and goparkunlock stay CONVERTED: goready is systemstack(ready) and systemstack is fn();
// goparkunlock is gopark(parkunlock_c).
//
// WHAT IS NOT MODELLED, and does not need to be: the run queue, `next`, P hand-off, wakep, traceskip
// and the trace events. The target is its own thread, so readying it IS running it. mcall stays a stub:
// nothing here calls it, and a future caller reaching it still refuses by name.
//
// THE MAPPING OWNS NOTHING. The runtime owns each g (one per goroutine thread, never reused); golib
// owns each goroutine (live from start to exit, ids never reused); the link is g.goid == the
// goroutine's id, read each time. A stale g names an id golib no longer holds, and that is Go's own
// "bad g->status in ready".

using System;
using go.golib;
using @unsafe = go.unsafe_package;

// Hand-owned (no park_impl.go exists, so a reconvert never regenerates this file). The declarations it
// replaces are registered in the converter's manualConversionFuncs, which is what turns the generated
// bodies into placeholders.
[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    // golib's WaitReason for a runtime waitReason: the inverse of stubs_impl.cs's mapWaitReason, so the
    // two cannot drift. A reason golib does not carry is refused by name rather than parked under a
    // wrong word (a traceback would print it) -- golib adds a member when a park first needs one.
    private static WaitReason golibWaitReasonOf(waitReason reason)
    {
        foreach (WaitReason candidate in WaitReasons.Parked())
        {
            if (mapWaitReason(candidate) == reason)
                return candidate;
        }

        throw new PanicException($"runtime: gopark with wait reason \"{reason.String()}\", which golib's WaitReason does not carry -- add the member with Go's string (golib runtime/WaitReason.cs) and its mapWaitReason arm");
    }

    // gopark puts the current goroutine into a waiting state and calls unlockf; if unlockf returns
    // false, the goroutine is resumed (Go's park_m). Otherwise it waits until a ready of its g.
    internal static void gopark(Func<ж<g>, @unsafe.Pointer, bool> unlockf, @unsafe.Pointer @lock, waitReason reason, traceBlockReason traceReason, nint traceskip)
    {
        if (Goroutine.Current is null)
            throw new PanicException("runtime: gopark on a thread with no goroutine identity -- nothing could ever ready it");

        WaitReason parkReason = golibWaitReasonOf(reason);
        ж<g> gp = getg();

        using (Goroutine.Park(parkReason))
        {
            // The runtime's OWN reason, over the one ParkTransition mapped from golib's: the same word
            // for every reason golib carries, and exactly Go's for the rest.
            gp.Value.waitreason = reason;

            // Go's park_m: the commit may refuse, and then the goroutine resumes without parking
            // (the scope's dispose returns the g to _Grunning).
            if (unlockf is not null && !unlockf(gp, @lock))
                return;

            Goroutine.WaitOnParkGate();
        }
    }

    // ready marks gp ready to run (Go: _Gwaiting -> _Grunnable, then onto a run queue). Here: resolve
    // the goroutine, ready it on this -- the waker's -- side, release it.
    internal static void ready(ж<g> Ꮡgp, nint traceskip, bool next)
    {
        if (goroutineOf(Ꮡgp) is not { } target)
        {
            println("runtime: ready of goroutine ", Ꮡgp.Value.goid, ", which has exited or was never a goroutine");
            @throw("bad g->status in ready"u8);
            return;
        }

        Goroutine.Ready(target);
        target.ReleaseParkGate();
    }

    // The goroutine a g belongs to: g.goid is its id, read through golib's live registry. Null for an
    // exited goroutine and for a g minted on a thread that was never one (goid 0; golib's ids start at 1).
    private static Goroutine? goroutineOf(ж<g> gp) => Goroutine.FromId(unchecked((long)gp.Value.goid));

    // injectglist adds each runnable G on the list to some run queue and clears glist. Here: ready
    // each, in list order.
    internal static void injectglist(ж<gList> Ꮡglist)
    {
        ref gList glist = ref Ꮡglist.Value;

        while (!glist.empty())
            ready(glist.pop(), 0, false);
    }

    // ---- the guard's view (RuntimeGoparkPairTests): the pair, driven from a golib goroutine ----

    /// <summary>
    /// A golib goroutine goparks under GC scavenge wait with an unlockf that records its call; the
    /// caller waits for it to reach _Gwaiting, reads the status and reason, then readies it through
    /// injectglist (the scavenger's own wake path) and reports how it came back.
    /// </summary>
    // The parker's failure, reported as text rather than escaping its goroutine: an unhandled exception
    // on a golib goroutine ends the process (Go's semantics), and the guard would then read only "the
    // test host crashed" -- measured, this seat's first red, which named nothing.
    public static (bool unlockfRan, uint statusParked, string reasonParked, bool resumed, string? parkerFailure) GoGoparkInjectglistProbe(int timeoutMs)
    {
        using System.Threading.ManualResetEventSlim published = new(false);
        using System.Threading.ManualResetEventSlim resumedEvent = new(false);
        ж<g>? parked = null;
        bool unlockfRan = false;
        string? parkerFailure = null;

        Goroutine.Start(() =>
        {
            try
            {
                parked = getg();
                published.Set();

                gopark((_, _) => { unlockfRan = true; return true; }, default, waitReasonGCScavengeWait, traceBlockSystemGoroutine, 0);
            }
            catch (Exception ex)
            {
                parkerFailure = $"{ex.GetType().Name}: {ex.Message}";
            }

            resumedEvent.Set();
        });

        if (!published.Wait(timeoutMs))
            throw new TimeoutException("the parker never started");

        long deadline = Environment.TickCount64 + timeoutMs;

        while (readgstatus(parked!) != _Gwaiting)
        {
            if (parkerFailure is not null)
                return (unlockfRan, (uint)readgstatus(parked!), "", false, parkerFailure);

            if (Environment.TickCount64 > deadline)
                throw new TimeoutException("the parker never reached _Gwaiting");

            System.Threading.Thread.Sleep(1);
        }

        uint status = (uint)readgstatus(parked!);
        string reasonText = parked!.Value.waitreason.String().ToString();

        gList list = default;
        list.push(parked!);
        injectglist(Ꮡ(list));

        return (unlockfRan, status, reasonText, resumedEvent.Wait(timeoutMs), parkerFailure);
    }

    /// <summary>
    /// A golib goroutine goparks with an unlockf that REFUSES the commit: it must come back without
    /// waiting, as Go's park_m resumes it, and read _Grunning again.
    /// </summary>
    public static (bool returned, uint statusAfter, string? parkerFailure) GoGoparkRefusedCommitProbe(int timeoutMs)
    {
        using System.Threading.ManualResetEventSlim done = new(false);
        uint statusAfter = 0;
        string? parkerFailure = null;

        Goroutine.Start(() =>
        {
            try
            {
                gopark((_, _) => false, default, waitReasonGCScavengeWait, traceBlockSystemGoroutine, 0);
                statusAfter = (uint)readgstatus(getg());
            }
            catch (Exception ex)
            {
                parkerFailure = $"{ex.GetType().Name}: {ex.Message}";
            }

            done.Set();
        });

        return (done.Wait(timeoutMs), statusAfter, parkerFailure);
    }

    /// <summary>gopark from a thread with no goroutine identity: the refusal's text.</summary>
    /// <remarks>
    /// On a FRESH plain thread, never the caller's: golib's module initializer registers the MAIN
    /// goroutine on whichever thread first loads golib, so a test's own thread can carry an identity,
    /// and a gopark there really parks it (this probe's first cut hung the test host exactly that way).
    /// </remarks>
    public static string GoGoparkOffGoroutineRefusal()
    {
        string result = "(the thread did not report)";

        System.Threading.Thread thread = new(() =>
        {
            if (Goroutine.Current is not null)
            {
                result = "(the fresh thread has a goroutine identity -- the probe measures nothing)";
                return;
            }

            try
            {
                gopark((_, _) => true, default, waitReasonGCScavengeWait, traceBlockSystemGoroutine, 0);
                result = "(no refusal)";
            }
            catch (Exception ex)
            {
                result = $"{ex.GetType().Name}: {ex.Message}";
            }
        });

        thread.Start();

        return thread.Join(30000) ? result : "(the thread never returned: gopark parked it)";
    }

    /// <summary>
    /// The resolution ready's refusal rests on: a g no goroutine owns (goid 0) resolves to NO goroutine,
    /// and a goroutine's own g resolves to it. The refusal itself is Go's fatal throw ("bad g->status in
    /// ready"), which ends the process as Go's does, so it cannot be exercised in-process; this is the
    /// half that decides whether it fires.
    /// </summary>
    public static (bool strangerResolvesToNone, bool ownResolvesToSelf) GoGoroutineOfProbe(int timeoutMs)
    {
        bool strangerResolvesToNone = goroutineOf(Ꮡ(new g())) is null;
        bool ownResolvesToSelf = false;

        using System.Threading.ManualResetEventSlim done = new(false);

        Goroutine.Start(() =>
        {
            ownResolvesToSelf = ReferenceEquals(goroutineOf(getg()), Goroutine.Current);
            done.Set();
        });

        if (!done.Wait(timeoutMs))
            throw new TimeoutException("the probe goroutine never ran");

        return (strangerResolvesToNone, ownResolvesToSelf);
    }

    public static uint GoStatusRunning => (uint)_Grunning;
}
