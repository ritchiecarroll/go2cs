// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using fmt = fmt_package;
using tracev2 = @internal.trace.tracev2_package;
using io = io_package;
using runtime = runtime_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using @internal.trace;
using System.Runtime.CompilerServices;
using go.sync;

partial class trace_package {

internal static ж<traceMultiplexer> Ꮡtracing = new StaticBox<traceMultiplexer>(new traceMultiplexer(nil));
internal static ref traceMultiplexer tracing => ref Ꮡtracing.Value;

partial struct traceMultiplexer {
    /*embed*/ public sync_package.Mutex Mutex;
    internal atomic.Bool enabled;
    internal nint subscribers;
    internal sync.Mutex subscribersMu;
    internal io.Writer traceStartWriter;
    internal ж<recorder> flightRecorder;
}

internal static error subscribeFlightRecorder(this ж<traceMultiplexer> Ꮡt, ж<recorder> Ꮡr) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        Ꮡt.of(traceMultiplexer.ᏑMutex).Lock();
        ᒐd1 = true;

        t.subscribersMu.Lock();
        if (t.flightRecorder != nil) {
            t.subscribersMu.Unlock();
            return fmt.Errorf("flight recorder already enabled"u8);
        }
        t.flightRecorder = Ꮡr;
        t.subscribersMu.Unlock();

        {
            var err = Ꮡt.addedSubscriber(); if (err != default!) {
                t.subscribersMu.Lock();
                t.flightRecorder = default!;
                t.subscribersMu.Unlock();
                return err;
            }
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.of(traceMultiplexer.ᏑMutex).Unlock(); ᒐ.Run(); }
}

internal static error unsubscribeFlightRecorder(this ж<traceMultiplexer> Ꮡt) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        Ꮡt.of(traceMultiplexer.ᏑMutex).Lock();
        ᒐd1 = true;

        Ꮡt.removingSubscriber();

        t.subscribersMu.Lock();
        if (t.flightRecorder == nil) {
            t.subscribersMu.Unlock();
            return fmt.Errorf("attempt to unsubscribe missing flight recorder"u8);
        }
        t.flightRecorder = default!;
        t.subscribersMu.Unlock();

        t.removedSubscriber();
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.of(traceMultiplexer.ᏑMutex).Unlock(); ᒐ.Run(); }
}

internal static error subscribeTraceStartWriter(this ж<traceMultiplexer> Ꮡt, io.Writer w) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        Ꮡt.of(traceMultiplexer.ᏑMutex).Lock();
        ᒐd1 = true;

        t.subscribersMu.Lock();
        if (t.traceStartWriter != default!) {
            t.subscribersMu.Unlock();
            return fmt.Errorf("execution tracer already enabled"u8);
        }
        t.traceStartWriter = w;
        t.subscribersMu.Unlock();

        {
            var err = Ꮡt.addedSubscriber(); if (err != default!) {
                t.subscribersMu.Lock();
                t.traceStartWriter = default!;
                t.subscribersMu.Unlock();
                return err;
            }
        }
        return default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡt.of(traceMultiplexer.ᏑMutex).Unlock(); ᒐ.Run(); }
}

internal static void unsubscribeTraceStartWriter(this ж<traceMultiplexer> Ꮡt) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        Ꮡt.of(traceMultiplexer.ᏑMutex).Lock();
        ᒐd1 = true;

        Ꮡt.removingSubscriber();

        t.subscribersMu.Lock();
        if (t.traceStartWriter == default!) {
            t.subscribersMu.Unlock();
            return;
        }
        t.traceStartWriter = default!;
        t.subscribersMu.Unlock();

        t.removedSubscriber();
        return;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡt.of(traceMultiplexer.ᏑMutex).Unlock(); ᒐ.Run(); }
}

internal static error addedSubscriber(this ж<traceMultiplexer> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (Ꮡt.of(traceMultiplexer.Ꮡenabled).Load()){
        // This is necessary for the trace reader goroutine to pick up on the new subscriber.
        runtime_traceAdvance(false);
    } else {
        {
            var err = Ꮡt.startLocked(); if (err != default!) {
                return err;
            }
        }
    }
    t.subscribers++;
    return default!;
}

internal static void removingSubscriber(this ж<traceMultiplexer> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.subscribers == 0) {
        return;
    }
    t.subscribers--;
    if (t.subscribers == 0){
        runtime.StopTrace();
        Ꮡt.of(traceMultiplexer.Ꮡenabled).Store(false);
    } else {
        // This is necessary to avoid missing trace data when the system is under high load.
        runtime_traceAdvance(false);
    }
}

internal static void removedSubscriber(this ref traceMultiplexer t) {
    if (t.subscribers > 0) {
        // This is necessary for the trace reader goroutine to pick up on the new subscriber.
        runtime_traceAdvance(false);
    }
}

internal static partial error startLocked(this ж<traceMultiplexer> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    {
        var err = runtime.StartTrace(); if (err != default!) {
            return err;
        }
    }

    // Grab the trace reader goroutine's subscribers.
    //
    // We only update our subscribers if we see an end-of-generation
    // signal from the runtime after this, so any new subscriptions
    // or unsubscriptions must call traceAdvance to ensure the reader
    // goroutine sees an end-of-generation signal.
    t.subscribersMu.Lock();
    ref var flightRecorder = ref heap<ж<recorder>>(out var ᏑflightRecorder);
    flightRecorder = t.flightRecorder;
    ref var traceStartWriter = ref heap<io.Writer>(out var ᏑtraceStartWriter);
    traceStartWriter = t.traceStartWriter;
    t.subscribersMu.Unlock();

    goǃ(() => {
        var header = runtime.ReadTrace();
        if (ᏑtraceStartWriter.ValueSlot != default!) {
            ᏑtraceStartWriter.ValueSlot.Write(header);
        }
        if (ᏑflightRecorder.ValueSlot != nil) {
            ᏑflightRecorder.ValueSlot.Write(header);
        }

        while (ᐧ) {
            var data = runtime.ReadTrace();
            if (data == default!) {
                break;
            }
            if (ᏑtraceStartWriter.ValueSlot != default!) {
                ᏑtraceStartWriter.ValueSlot.Write(data);
            }
            if (ᏑflightRecorder.ValueSlot != nil) {
                ᏑflightRecorder.ValueSlot.Write(data);
            }
            if (len(data) == 1 && ((tracev2.EventType)data[0]) == tracev2.EvEndOfGeneration) {
                if (ᏑflightRecorder.ValueSlot != nil) {
                    ᏑflightRecorder.ValueSlot.endGeneration();
                }

                // Pick up any changes.
                Ꮡt.Value.subscribersMu.Lock();
                var frIsNew = ᏑflightRecorder.ValueSlot != Ꮡt.Value.flightRecorder && Ꮡt.Value.flightRecorder != nil;
                var trIsNew = !AreEqual(ᏑtraceStartWriter.ValueSlot, Ꮡt.Value.traceStartWriter) && Ꮡt.Value.traceStartWriter != default!;
                ᏑflightRecorder.ValueSlot = Ꮡt.Value.flightRecorder;
                ᏑtraceStartWriter.ValueSlot = Ꮡt.Value.traceStartWriter;
                Ꮡt.Value.subscribersMu.Unlock();

                if (trIsNew) {
                    ᏑtraceStartWriter.ValueSlot.Write(header);
                }
                if (frIsNew) {
                    ᏑflightRecorder.ValueSlot.Write(header);
                }
            }
        }
    });
    Ꮡt.of(traceMultiplexer.Ꮡenabled).Store(true);
    return default!;
}

//go:linkname runtime_readTrace
internal static partial slice<byte> /*buf*/ runtime_readTrace();

} // end trace_package
