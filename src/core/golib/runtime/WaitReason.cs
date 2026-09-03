// WaitReason.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go.golib;

/// <summary>
/// Why a goroutine is parked — Go's <c>waitReason</c>, carrying Go's own strings.
/// </summary>
/// <remarks>
/// <para>
/// The STRINGS are the contract, not the names: a Go traceback header reads
/// <c>goroutine 7 [chan receive]:</c>, and Go's own tests grep it — <c>runtime/pprof</c>'s
/// <c>awaitBlockedGoroutine</c> builds a regex around exactly this word. So every member's text
/// below is copied VERBATIM from <c>$GOROOT/src/runtime/runtime2.go</c>'s
/// <c>waitReasonStrings</c> table (go1.23.12), and the member names mirror Go's own constants
/// minus their <c>waitReason</c> prefix.
/// </para>
/// <para>
/// <b>This is not all 38 of Go's reasons, deliberately.</b> A reason nothing can ever set is a
/// value a dump can never print and a guard can never exercise — speculative machinery whose only
/// effect would be to make the enum look more complete than the runtime is. The members here are
/// exactly the reasons a go2cs park site actually sets (see
/// <c>docs/phase4/DESIGN-cooperative-scheduler.md</c> §6). The rest fall into three groups, none of
/// which has a managed referent: the GC and scavenger reasons (the CLR owns collection), the
/// scheduler-internal ones (<c>preempted</c>, <c>stopping the world</c>, <c>GC worker (idle)</c> —
/// there is no P, no run queue and no sysmon), and the tracer's (<c>trace reader (blocked)</c> and
/// friends, whose <c>runtime/trace</c> engine is converted-dead). A future arc that mints one of
/// those adds its member with its Go string at that time.
/// </para>
/// </remarks>
public enum WaitReason
{
    /// <summary>
    /// Not parked — Go's <c>waitReasonZero</c>, whose string is empty.
    /// </summary>
    /// <remarks>
    /// This is the encoding of "running" as well as the enum's default, which is what lets ONE
    /// field carry both the state and the reason: a goroutine is parked exactly when its reason is
    /// not <see cref="Zero"/>, so the two facts cannot disagree.
    /// </remarks>
    Zero = 0,

    /// <summary>"IO wait" — blocked in the poller (Go's <c>netpollblock</c>).</summary>
    IOWait,

    /// <summary>"chan receive (nil chan)" — a receive on a nil channel, which blocks forever.</summary>
    ChanReceiveNilChan,

    /// <summary>"chan send (nil chan)" — a send on a nil channel, which blocks forever.</summary>
    ChanSendNilChan,

    /// <summary>"select" — blocked in a select with no ready case.</summary>
    Select,

    /// <summary>"select (no cases)" — <c>select{}</c>, or a select whose every case is nil.</summary>
    SelectNoCases,

    /// <summary>"chan receive" — blocked receiving from an empty channel.</summary>
    ChanReceive,

    /// <summary>"chan send" — blocked sending to a full or unreceived channel.</summary>
    ChanSend,

    /// <summary>
    /// "semacquire" — blocked on a runtime semaphore that is not a mutex: Go's
    /// <c>sync_runtime_Semacquire</c> (<c>sync.WaitGroup.Wait</c>) and
    /// <c>poll_runtime_Semacquire</c> (<c>internal/poll</c>'s fdMutex).
    /// </summary>
    Semacquire,

    /// <summary>"sleep" — inside <c>time.Sleep</c>.</summary>
    Sleep,

    /// <summary>"sync.Cond.Wait" — on a condition variable's notify list.</summary>
    SyncCondWait,

    /// <summary>"sync.Mutex.Lock" — contending for a mutex.</summary>
    SyncMutexLock,

    /// <summary>"sync.RWMutex.RLock" — a reader waiting out a writer.</summary>
    SyncRWMutexRLock,

    /// <summary>"sync.RWMutex.Lock" — a writer waiting out the readers.</summary>
    SyncRWMutexLock
}

/// <summary>
/// The text a <see cref="WaitReason"/> prints in a goroutine traceback header.
/// </summary>
public static class WaitReasons
{
    /// <summary>
    /// Returns Go's own string for <paramref name="reason"/> — the word that appears between the
    /// brackets of a <c>goroutine N [...]:</c> header.
    /// </summary>
    /// <remarks>
    /// An out-of-range value answers Go's own <c>waitReason.String</c> fallback,
    /// <c>"unknown wait reason"</c>, rather than a .NET enum name: a traceback is observable output,
    /// so even the impossible case renders in Go's vocabulary.
    /// </remarks>
    public static string Text(WaitReason reason) => reason switch
    {
        WaitReason.Zero => "",
        WaitReason.IOWait => "IO wait",
        WaitReason.ChanReceiveNilChan => "chan receive (nil chan)",
        WaitReason.ChanSendNilChan => "chan send (nil chan)",
        WaitReason.Select => "select",
        WaitReason.SelectNoCases => "select (no cases)",
        WaitReason.ChanReceive => "chan receive",
        WaitReason.ChanSend => "chan send",
        WaitReason.Semacquire => "semacquire",
        WaitReason.Sleep => "sleep",
        WaitReason.SyncCondWait => "sync.Cond.Wait",
        WaitReason.SyncMutexLock => "sync.Mutex.Lock",
        WaitReason.SyncRWMutexRLock => "sync.RWMutex.RLock",
        WaitReason.SyncRWMutexLock => "sync.RWMutex.Lock",
        _ => "unknown wait reason"
    };

    /// <summary>
    /// Enumerates every reason this runtime can set, in declaration order.
    /// </summary>
    /// <remarks>
    /// <see cref="WaitReason.Zero"/> is excluded: it is the not-parked encoding, and its string is
    /// empty, so it is never a header word.
    /// </remarks>
    public static WaitReason[] Parked()
    {
        WaitReason[] all = Enum.GetValues<WaitReason>();
        WaitReason[] parked = new WaitReason[all.Length - 1];
        int next = 0;

        foreach (WaitReason reason in all)
        {
            if (reason != WaitReason.Zero)
                parked[next++] = reason;
        }

        return parked;
    }
}
