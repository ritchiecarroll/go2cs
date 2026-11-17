// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Hand-finished conversion (tick.go — Ticker construction and control).
//
// Go allocates a Ticker inside the RUNTIME and reaches it by reinterpreting the *Timer that
// runtime.newTimer returned — "Ticker and Timer have the same layout, so that newTimer can handle
// both":
//
//     t := (*Ticker)(unsafe.Pointer(newTimer(when(d), int64(d), sendTime, c, syncTimer(c))))
//     ...
//     stopTimer((*Timer)(unsafe.Pointer(t)))
//
// The literal conversion of those three reinterprets COMPILES but cannot work in a managed runtime.
// Each is a `(ж<U>)(uintptr)(unsafe.Pointer(Ꮡbox))` round-trip, and golib's managed-box
// `ж<T> → uintptr` operator returns `fixed (void* ptr = &value.Value) (uintptr)ptr` — an address
// whose pin ends with that statement. The derived box holds only the bare address and NO reference
// to the source box, so the ticker's storage is (a) not kept alive at all — nothing else references
// the ж<Timer> newTimer produced, so it is collected at the next GC — and (b) not stable even if it
// were, since a compacting GC may move it. Every read of `t.C` or `t.initTicker` afterwards reads
// whatever now occupies that address. Worse, `Ticker` holds a `channel<Time>`, i.e. a MANAGED
// reference, so the storage cannot be pinned either (`GCHandle.Alloc(…, Pinned)` rejects a type
// containing references) — the raw-address model has no sound form here. See
// docs/phase4/FINDING-managed-box-uintptr-lifetime.md, which characterizes this hazard corpus-wide;
// NewTicker is its worst case, because the derived pointer is RETAINED for the ticker's lifetime
// rather than consumed immediately.
//
// This file therefore constructs the Ticker DIRECTLY and addresses the runtime timer by the box's
// own identity, which is the project's standing ruling for managed-referent pointers ("hold the
// ж<T>/object directly, never a nuint round-trip"). Behavior is otherwise unchanged: armNewTimer /
// stopRuntimeTimer / resetRuntimeTimer (time_impl.cs) are the very entry points sleep.cs's
// newTimer / stopTimer / resetTimer call, and Go's own runtime.newTimer serves Timer and Ticker
// through one implementation for the same reason.
//
// MODELED since 2026-08-03: the `syncTimer(c)` argument. It marks the channel a SYNCHRONOUS timer
// channel (Go 1.23, #37196), which the managed timer model now implements; the call below is made
// for its nil/non-nil ANSWER, which is the mode selector, not for its pointer value. See
// SYNCHRONOUS TIMER CHANNELS in time_impl.cs for the property and the two mechanisms.
//
// REVERTIBLE: if the general managed-reinterpret capability lands (option 2 of the FINDING — a
// converter-emitted `Ꮡt.Reinterpret<U>()` aliasing the same managed slot), delete this file and let
// the converter's tick.cs.auto take over unchanged; nothing here is a semantic choice of its own.
[module: go.GoManualConversion]

namespace go;

using static time_package;

partial class time_package {

// Note: The runtime knows the layout of struct Ticker, since newTimer allocates it.
// Note also that Ticker and Timer have the same layout, so that newTimer can handle both.
// The initTimer and initTicker fields are named differently so that
// users cannot convert between the two without unsafe.

// A Ticker holds a channel that delivers “ticks” of a clock
// at intervals.
[GoType] public partial struct Ticker {
    public /*<-*/channel<Time> C; // The channel on which the ticks are delivered.
    internal bool initTicker;
}

// NewTicker returns a new [Ticker] containing a channel that will send
// the current time on the channel after each tick. The period of the
// ticks is specified by the duration argument. The ticker will adjust
// the time interval or drop ticks to make up for slow receivers.
// The duration d must be greater than zero; if not, NewTicker will
// panic.
//
// Before Go 1.23, the garbage collector did not recover
// tickers that had not yet expired or been stopped, so code often
// immediately deferred t.Stop after calling NewTicker, to make
// the ticker recoverable when it was no longer needed.
// As of Go 1.23, the garbage collector can recover unreferenced
// tickers, even if they haven't been stopped.
// The Stop method is no longer necessary to help the garbage collector.
// (Code may of course still want to call Stop to stop the ticker for other reasons.)
public static ж<Ticker> NewTicker(Duration d) {
    if (d <= 0) {
        throw panic("non-positive interval for NewTicker");
    }
    // Give the channel a 1-element time buffer.
    // If the client falls behind while reading, we drop ticks
    // on the floor until the client catches up.
    var c = new channel<Time>(1);
    // Go: t := (*Ticker)(unsafe.Pointer(newTimer(when(d), int64(d), sendTime, c, syncTimer(c))))
    var t = new StandardBox<Ticker>(new Ticker{ initTicker = true });
    // Go's `syncTimer(c)` argument, modeled by its ANSWER rather than by its pointer: the pointer
    // is unusable here for the same reason the reinterprets above are, and its nil-ness is only a
    // re-encoding of the setting syncTimerChanEnabled reads directly. newTimer resolves it the same
    // way, so the two constructors cannot disagree about the model. (Not calling syncTimer also
    // avoids its own reinterpret; the IncNonDefault counter it would bump is inert here — the
    // hand-owned godebug has no metrics consumer.)
    armNewTimer(t, when(d), (int64)d, sendTime, c, syncTimerChanEnabled() ? c : default);
    t.Value.C = c;
    return t;
}

} partial class timeᴛTicker {

// Stop turns off a ticker. After Stop, no more ticks will be sent.
// Stop does not close the channel, to prevent a concurrent goroutine
// reading from the channel from seeing an erroneous "tick".
public static void Stop(this ж<Ticker> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if (!t.initTicker) {
        // This is misuse, and the same for time.Timer would panic,
        // but this didn't always panic, and we keep it not panicking
        // to avoid breaking old programs. See issue 21874.
        return;
    }
    // Go: stopTimer((*Timer)(unsafe.Pointer(t)))
    stopRuntimeTimer(Ꮡt);
}

// Reset stops a ticker and resets its period to the specified duration.
// The next tick will arrive after the new period elapses. The duration d
// must be greater than zero; if not, Reset will panic.
public static void Reset(this ж<Ticker> Ꮡt, Duration d) {
    ref var t = ref Ꮡt.Value;

    if (d <= 0) {
        throw panic("non-positive interval for Ticker.Reset");
    }
    if (!t.initTicker) {
        throw panic("time: Reset called on uninitialized Ticker");
    }
    // Go: resetTimer((*Timer)(unsafe.Pointer(t)), when(d), int64(d))
    resetRuntimeTimer(Ꮡt, when(d), (int64)d);
}

} partial class time_package {

// Tick is a convenience wrapper for [NewTicker] providing access to the ticking
// channel only. Unlike NewTicker, Tick will return nil if d <= 0.
//
// Before Go 1.23, this documentation warned that the underlying
// [Ticker] would never be recovered by the garbage collector, and that
// if efficiency was a concern, code should use NewTicker instead and
// call [Ticker.Stop] when the ticker is no longer needed.
// As of Go 1.23, the garbage collector can recover unreferenced
// tickers, even if they haven't been stopped.
// The Stop method is no longer necessary to help the garbage collector.
// There is no longer any reason to prefer NewTicker when Tick will do.
public static /*<-*/channel<Time> Tick(Duration d) {
    if (d <= 0) {
        return default!;
    }
    return (~NewTicker(d)).C;
}

} // end time_package
