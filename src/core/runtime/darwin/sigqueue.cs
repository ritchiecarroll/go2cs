// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements runtime support for signal handling.
//
// Most synchronization primitives are not available from
// the signal handler (it cannot block, allocate memory, or use locks)
// so the handler communicates with a processing goroutine
// via struct sig, below.
//
// sigsend is called by the signal handler to queue a new signal.
// signal_recv is called by the Go program to receive a newly queued signal.
//
// Synchronization between sigsend and signal_recv is based on the sig.state
// variable. It can be in three states:
// * sigReceiving means that signal_recv is blocked on sig.Note and there are
//   no new pending signals.
// * sigSending means that sig.mask *may* contain new pending signals,
//   signal_recv can't be blocked in this state.
// * sigIdle means that there are no new pending signals and signal_recv is not
//   blocked.
//
// Transitions between states are done atomically with CAS.
//
// When signal_recv is unblocked, it resets sig.Note and rechecks sig.mask.
// If several sigsends and signal_recv execute concurrently, it can lead to
// unnecessary rechecks of sig.mask, but it cannot lead to missed signals
// nor deadlocks.
//go:build !plan9
namespace go;

using atomic = @internal.runtime.atomic_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname
using @internal.runtime;

partial class runtime_package {

// sig handles communication between the signal handler and os/signal.
// Other than the inuse and recv fields, the fields are accessed atomically.
//
// The wanted and ignored fields are only written by one goroutine at
// a time; access is controlled by the handlers Mutex in os/signal.
// The fields are only read by that one goroutine and by the signal handler.
// We access them atomically to minimize the race between setting them
// in the goroutine calling os/signal and the signal handler,
// which may be running in a different thread. That race is unavoidable,
// as there is no connection between handling a signal and receiving one,
// but atomic instructions should minimize it.

[GoType("dyn")] partial struct sigᴛ1 {
    internal note note;
    internal array<uint32> mask = new(1);
    internal array<uint32> wanted = new(1);
    internal array<uint32> ignored = new(1);
    internal array<uint32> recv = new(1);
    internal atomic.Uint32 state;
    internal atomic.Uint32 delivering;
    internal bool inuse;
}
internal static ж<sigᴛ1> Ꮡsig = new StandardBox<sigᴛ1>(new sigᴛ1());
internal static ref sigᴛ1 sig => ref Ꮡsig.Value;

internal static UntypedInt sigIdle => iota;
internal static UntypedInt sigReceiving => 1;
internal static UntypedInt sigSending => 2;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sigsendInconsistentStateˢ = "sigsend: inconsistent state"u8;

// sigsend delivers a signal from sighandler to the internal signal delivery queue.
// It reports whether the signal was sent. If not, the caller typically crashes the program.
// It runs from the signal handler, so it's limited in what it can do.
internal static bool sigsend(uint32 s) {
    var bit = ((uint32)1).Lsh((nuint)((uint32)(s & 31)));
    if (s >= (uint32)(32 * len(sig.wanted))) {
        return false;
    }
    Ꮡsig.of(sigᴛ1.Ꮡdelivering).Add(1);
    // We are running in the signal handler; defer is not available.
    {
        var w = atomic.Load(Ꮡsig.at(sigᴛ1.Ꮡwanted, (ulong)(s / 32))); if ((uint32)(w & bit) == 0) {
            Ꮡsig.of(sigᴛ1.Ꮡdelivering).Add(-1);
            return false;
        }
    }
    // Add signal to outgoing queue.
    while (ᐧ) {
        var mask = sig.mask[s / 32];
        if ((uint32)(mask & bit) != 0) {
            Ꮡsig.of(sigᴛ1.Ꮡdelivering).Add(-1);
            return true; // signal already in queue
        }
        if (atomic.Cas(Ꮡsig.at(sigᴛ1.Ꮡmask, (ulong)(s / 32)), mask, (uint32)(mask | bit))) {
            break;
        }
    }
    // Notify receiver that queue has new bit.
Send:
    while (ᐧ) {
        var exprᴛ1 = Ꮡsig.of(sigᴛ1.Ꮡstate).Load();
        if (exprᴛ1 == sigIdle) {
            if (Ꮡsig.of(sigᴛ1.Ꮡstate).CompareAndSwap(sigIdle, sigSending)) {
                goto break_Send;
            }
        }
        else if (exprᴛ1 == sigSending) {
            goto break_Send;
        }
        else if (exprᴛ1 == sigReceiving) {
            if (Ꮡsig.of(sigᴛ1.Ꮡstate).CompareAndSwap(sigReceiving, // notification already pending
 sigIdle)) {
                if (GOOS == "darwin"u8 || GOOS == "ios"u8) {
                    sigNoteWakeup(Ꮡsig.of(sigᴛ1.Ꮡnote));
                    goto break_Send;
                }
                notewakeup(Ꮡsig.of(sigᴛ1.Ꮡnote));
                goto break_Send;
            }
        }
        else { /* default: */
            @throw(sigsendInconsistentStateˢ);
        }

continue_Send:;
    }
break_Send:;
    Ꮡsig.of(sigᴛ1.Ꮡdelivering).Add(-1);
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string signalRecvInconsistentˢ = "signal_recv: inconsistent state"u8;

// Called to receive the next queued signal.
// Must only be called from a single goroutine at a time.
//
//go:linkname signal_recv os/signal.signal_recv
public static uint32 signal_recv() {
    while (ᐧ) {
        // Serve any signals from local copy.
        for (var i = (uint32)0; i < _NSIG; i++) {
            if ((uint32)(sig.recv[i / 32] & (((uint32)1 << (int)(((uint32)(i & 31)))))) != 0) {
                sig.recv[i / 32] &= unchecked((uint32)~(uint32)(((uint32)1 << (int)(((uint32)(i & 31))))));
                return i;
            }
        }
        // Wait for updates to be available from signal sender.
Receive:
        while (ᐧ) {
            var exprᴛ1 = Ꮡsig.of(sigᴛ1.Ꮡstate).Load();
            if (exprᴛ1 == sigIdle) {
                if (Ꮡsig.of(sigᴛ1.Ꮡstate).CompareAndSwap(sigIdle, sigReceiving)) {
                    if (GOOS == "darwin"u8 || GOOS == "ios"u8) {
                        sigNoteSleep(Ꮡsig.of(sigᴛ1.Ꮡnote));
                        goto break_Receive;
                    }
                    notetsleepg(Ꮡsig.of(sigᴛ1.Ꮡnote), -1);
                    noteclear(Ꮡsig.of(sigᴛ1.Ꮡnote));
                    goto break_Receive;
                }
            }
            else if (exprᴛ1 == sigSending) {
                if (Ꮡsig.of(sigᴛ1.Ꮡstate).CompareAndSwap(sigSending, sigIdle)) {
                    goto break_Receive;
                }
            }
            else { /* default: */
                @throw(signalRecvInconsistentˢ);
            }

continue_Receive:;
        }
break_Receive:;
        // Incorporate updates from sender into local copy.
        foreach (var (i, _) in sig.mask) {
            sig.recv[i] = atomic.Xchg(Ꮡsig.at(sigᴛ1.Ꮡmask, i), 0);
        }
    }
}

// signalWaitUntilIdle waits until the signal delivery mechanism is idle.
// This is used to ensure that we do not drop a signal notification due
// to a race between disabling a signal and receiving a signal.
// This assumes that signal delivery has already been disabled for
// the signal(s) in question, and here we are just waiting to make sure
// that all the signals have been delivered to the user channels
// by the os/signal package.
//
//go:linkname signalWaitUntilIdle os/signal.signalWaitUntilIdle
public static void signalWaitUntilIdle() {
    // Although the signals we care about have been removed from
    // sig.wanted, it is possible that another thread has received
    // a signal, has read from sig.wanted, is now updating sig.mask,
    // and has not yet woken up the processor thread. We need to wait
    // until all current signal deliveries have completed.
    while (Ꮡsig.of(sigᴛ1.Ꮡdelivering).Load() != 0) {
        Gosched();
    }
    // Although WaitUntilIdle seems like the right name for this
    // function, the state we are looking for is sigReceiving, not
    // sigIdle.  The sigIdle state is really more like sigProcessing.
    while (Ꮡsig.of(sigᴛ1.Ꮡstate).Load() != sigReceiving) {
        Gosched();
    }
}

// Must only be called from a single goroutine at a time.
//
//go:linkname signal_enable os/signal.signal_enable
public static void signal_enable(uint32 s) {
    if (!sig.inuse) {
        // This is the first call to signal_enable. Initialize.
        sig.inuse = true; // enable reception of signals; cannot disable
        if (GOOS == "darwin"u8 || GOOS == "ios"u8){
            sigNoteSetup(Ꮡsig.of(sigᴛ1.Ꮡnote));
        } else {
            noteclear(Ꮡsig.of(sigᴛ1.Ꮡnote));
        }
    }
    if (s >= (uint32)(len(sig.wanted) * 32)) {
        return;
    }
    var w = sig.wanted[s / 32];
    w |= (uint32)(((uint32)1 << (int)(((uint32)(s & 31)))));
    atomic.Store(Ꮡsig.at(sigᴛ1.Ꮡwanted, (ulong)(s / 32)), w);
    var i = sig.ignored[s / 32];
    i &= unchecked((uint32)~(uint32)(((uint32)1 << (int)(((uint32)(s & 31))))));
    atomic.Store(Ꮡsig.at(sigᴛ1.Ꮡignored, (ulong)(s / 32)), i);
    sigenable(s);
}

// Must only be called from a single goroutine at a time.
//
//go:linkname signal_disable os/signal.signal_disable
public static void signal_disable(uint32 s) {
    if (s >= (uint32)(len(sig.wanted) * 32)) {
        return;
    }
    sigdisable(s);
    var w = sig.wanted[s / 32];
    w &= unchecked((uint32)~(uint32)(((uint32)1 << (int)(((uint32)(s & 31))))));
    atomic.Store(Ꮡsig.at(sigᴛ1.Ꮡwanted, (ulong)(s / 32)), w);
}

// Must only be called from a single goroutine at a time.
//
//go:linkname signal_ignore os/signal.signal_ignore
public static void signal_ignore(uint32 s) {
    if (s >= (uint32)(len(sig.wanted) * 32)) {
        return;
    }
    sigignore(s);
    var w = sig.wanted[s / 32];
    w &= unchecked((uint32)~(uint32)(((uint32)1 << (int)(((uint32)(s & 31))))));
    atomic.Store(Ꮡsig.at(sigᴛ1.Ꮡwanted, (ulong)(s / 32)), w);
    var i = sig.ignored[s / 32];
    i |= (uint32)(((uint32)1 << (int)(((uint32)(s & 31)))));
    atomic.Store(Ꮡsig.at(sigᴛ1.Ꮡignored, (ulong)(s / 32)), i);
}

// sigInitIgnored marks the signal as already ignored. This is called at
// program start by initsig. In a shared library initsig is called by
// libpreinit, so the runtime may not be initialized yet.
//
//go:nosplit
internal static void sigInitIgnored(uint32 s) {
    var i = sig.ignored[s / 32];
    i |= (uint32)(((uint32)1 << (int)(((uint32)(s & 31)))));
    atomic.Store(Ꮡsig.at(sigᴛ1.Ꮡignored, (ulong)(s / 32)), i);
}

// Checked by signal handlers.
//
//go:linkname signal_ignored os/signal.signal_ignored
public static bool signal_ignored(uint32 s) {
    var i = atomic.Load(Ꮡsig.at(sigᴛ1.Ꮡignored, (ulong)(s / 32)));
    return (uint32)(i & (((uint32)1 << (int)(((uint32)(s & 31)))))) != 0;
}

} // end runtime_package
