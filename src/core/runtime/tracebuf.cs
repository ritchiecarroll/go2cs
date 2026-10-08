// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Trace buffer management.
namespace go;

using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal.runtime;
using ꓸꓸꓸtraceArg = Span<runtime_package.traceArg>;

partial class runtime_package {

// Maximum number of bytes required to encode uint64 in base-128.
internal static UntypedInt traceBytesPerNumber => 10;

// traceWriter is the interface for writing all trace data.
//
// This type is passed around as a value, and all of its methods return
// a new traceWriter. This allows for chaining together calls in a fluent-style
// API. This is partly stylistic, and very slightly for performance, since
// the compiler can destructure this value and pass it between calls as
// just regular arguments. However, this style is not load-bearing, and
// we can change it if it's deemed too error-prone.
partial struct traceWriter {
    internal partial ref traceLocker traceLocker { get; }
    internal traceExperiment exp;
    internal partial ref ж<traceBuf> traceBuf { get; }
}

// writer returns an a traceWriter that writes into the current M's stream.
//
// Once this is called, the caller must guard against stack growth until
// end is called on it. Therefore, it's highly recommended to use this
// API in a "fluent" style, for example tl.writer().event(...).end().
// Better yet, callers just looking to write events should use eventWriter
// when possible, which is a much safer wrapper around this function.
//
// nosplit to allow for safe reentrant tracing from stack growth paths.
//
//go:nosplit
internal static traceWriter writer(this traceLocker tl) {
    if (debugTraceReentrancy) {
        // Checks that the invariants of this function are being upheld.
        var gp = getg();
        if (gp == (~(~gp).m).curg) {
            tl.mp.Value.trace.oldthrowsplit = gp.Value.throwsplit;
            gp.Value.throwsplit = true;
        }
    }
    return new traceWriter(traceLocker: tl, traceBuf: (~tl.mp).trace.buf[tl.gen % 2][traceNoExperiment]);
}

// unsafeTraceWriter produces a traceWriter that doesn't lock the trace.
//
// It should only be used in contexts where either:
// - Another traceLocker is held.
// - trace.gen is prevented from advancing.
//
// This does not have the same stack growth restrictions as traceLocker.writer.
//
// buf may be nil.
internal static traceWriter unsafeTraceWriter(uintptr gen, ж<traceBuf> Ꮡbuf) {
    return new traceWriter(traceLocker: new traceLocker(gen: gen), traceBuf: Ꮡbuf);
}

// event writes out the bytes of an event into the event stream.
//
// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static traceWriter @event(this traceWriter w, traceEv ev, params ꓸꓸꓸtraceArg argsʗp) {
    var args = argsʗp.sslice();

    // N.B. Everything in this call must be nosplit to maintain
    // the stack growth related invariants for writing events.
    // Make sure we have room.
    (w, _) = w.ensure(1 + (len(args) + 1) * (nint)traceBytesPerNumber);
    // Compute the timestamp diff that we'll put in the trace.
    var ts = traceClockNow();
    if (ts <= (~w.traceBuf).lastTime) {
        ts = (~w.traceBuf).lastTime + 1;
    }
    var tsDiff = (uint64)(ts - (~w.traceBuf).lastTime);
    w.traceBuf.Value.lastTime = ts;
    // Write out event.
    w.@byte((byte)ev);
    w.varint(tsDiff);
    foreach (var (_, arg) in args) {
        w.varint((uint64)arg);
    }
    return w;
}

// end writes the buffer back into the m.
//
// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static void end(this traceWriter w) {
    if (w.mp == nil) {
        // Tolerate a nil mp. It makes code that creates traceWriters directly
        // less error-prone.
        return;
    }
    (~w.mp).trace.buf[w.gen % 2][w.exp] = w.traceBuf;
    if (debugTraceReentrancy) {
        // The writer is no longer live, we can drop throwsplit (if it wasn't
        // already set upon entry).
        var gp = getg();
        if (gp == (~(~gp).m).curg) {
            gp.Value.throwsplit = w.mp.Value.trace.oldthrowsplit;
        }
    }
}

// ensure makes sure that at least maxSize bytes are available to write.
//
// Returns whether the buffer was flushed.
//
// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static (traceWriter, bool) ensure(this traceWriter w, nint maxSize) {
    var refill = w.traceBuf == nil || !w.available(maxSize);
    if (refill) {
        w = w.refill();
    }
    return (w, refill);
}

// flush puts w.traceBuf on the queue of full buffers.
//
// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static traceWriter flush(this traceWriter w) {
    systemstack(() => {
        @lock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
        if (w.traceBuf != nil) {
            traceBufFlush(w.traceBuf, w.gen);
        }
        unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
    });
    w.traceBuf = default!;
    return w;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string traceOutOfMemoryˢ = "trace: out of memory"u8;

// refill puts w.traceBuf on the queue of full buffers and refresh's w's buffer.
internal static traceWriter refill(this traceWriter w) {
    systemstack(() => {
        @lock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
        if (w.traceBuf != nil) {
            traceBufFlush(w.traceBuf, w.gen);
        }
        if (Δtrace.empty != nil){
            w.traceBuf = Δtrace.empty;
            Δtrace.empty = w.traceBuf.Value.link;
            unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
        } else {
            unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
            w.traceBuf = (ж<traceBuf>)(uintptr)(sysAlloc(/* unsafe.Sizeof(traceBuf{}) */ (uintptr)65536, Ꮡmemstats.of(mstats.Ꮡother_sys)));
            if (w.traceBuf == nil) {
                @throw(traceOutOfMemoryˢ);
            }
        }
    });
    // Initialize the buffer.
    var ts = traceClockNow();
    if (ts <= (~w.traceBuf).lastTime) {
        ts = (~w.traceBuf).lastTime + 1;
    }
    w.traceBuf.Value.lastTime = ts;
    w.traceBuf.Value.link = default!;
    w.traceBuf.Value.pos = 0;
    // Tolerate a nil mp.
    var mID = ~(uint64)0;
    if (w.mp != nil) {
        mID = (uint64)(~w.mp).procid;
    }
    // Write the buffer's header.
    if (w.exp == traceNoExperiment){
        w.@byte((byte)traceEvEventBatch);
    } else {
        w.@byte((byte)traceEvExperimentalBatch);
        w.@byte((byte)w.exp);
    }
    w.varint((uint64)w.gen);
    w.varint((uint64)mID);
    w.varint((uint64)ts);
    w.traceBuf.Value.lenPos = w.varintReserve();
    return w;
}

// traceBufQueue is a FIFO of traceBufs.
partial struct traceBufQueue {
    internal ж<traceBuf> head, tail;
}

// push queues buf into queue of buffers.
internal static void push(this ref traceBufQueue q, ж<traceBuf> Ꮡbuf) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    buf.link = default!;
    if (q.head == nil){
        q.head = Ꮡbuf;
    } else {
        q.tail.Value.link = Ꮡbuf;
    }
    q.tail = Ꮡbuf;
}

// pop dequeues from the queue of buffers.
internal static ж<traceBuf> pop(this ref traceBufQueue q) {
    var buf = q.head;
    if (buf == nil) {
        return default!;
    }
    q.head = buf.Value.link;
    if (q.head == nil) {
        q.tail = default!;
    }
    buf.Value.link = default!;
    return buf;
}

internal static bool empty(this ref traceBufQueue q) {
    return q.head == nil;
}

// traceBufHeader is per-P tracing buffer.
partial struct traceBufHeader {
    internal ж<traceBuf> link; // in trace.empty/full
    internal traceTime lastTime; // when we wrote the last event
    internal nint pos;      // next write offset in arr
    internal nint lenPos;      // position of batch length value
}

// traceBuf is per-M tracing buffer.
//
// TODO(mknyszek): Rename traceBuf to traceBatch, since they map 1:1 with event batches.
partial struct traceBuf {
    internal sys.NotInHeap _;
    internal partial ref traceBufHeader traceBufHeader { get; }
    internal array<byte> arr = new(((uintptr)64 << (int)(10)) - /* unsafe.Sizeof(traceBufHeader{}) */ (uintptr)32); // underlying buffer for traceBufHeader.buf
}

// byte appends v to buf.
//
// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static void @byte(this ref traceBuf buf, byte v) {
    buf.arr[buf.pos] = v;
    buf.pos++;
}

// varint appends v to buf in little-endian-base-128 encoding.
//
// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static void varint(this ref traceBuf buf, uint64 v) {
    nint pos = buf.pos;
    var arr = buf.arr.slice(pos, pos + (nint)traceBytesPerNumber);
    foreach (var (i, _) in arr) {
        if (v < 0x80) {
            pos += i + 1;
            arr[i] = (byte)v;
            break;
        }
        arr[i] = (byte)(0x80 | (byte)v);
        v >>= (int)(7);
    }
    buf.pos = pos;
}

// varintReserve reserves enough space in buf to hold any varint.
//
// Space reserved this way can be filled in with the varintAt method.
//
// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static nint varintReserve(this ref traceBuf buf) {
    nint Δp = buf.pos;
    buf.pos += traceBytesPerNumber;
    return Δp;
}

// stringData appends s's data directly to buf.
//
// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static void stringData(this ref traceBuf buf, @string s) {
    buf.pos += copy(buf.arr.slice(buf.pos), s);
}

// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static bool available(this ref traceBuf buf, nint size) {
    return len(buf.arr) - buf.pos >= size;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string vCouldNotFitInˢ = "v could not fit in traceBytesPerNumber"u8;

// varintAt writes varint v at byte position pos in buf. This always
// consumes traceBytesPerNumber bytes. This is intended for when the caller
// needs to reserve space for a varint but can't populate it until later.
// Use varintReserve to reserve this space.
//
// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static void varintAt(this ref traceBuf buf, nint pos, uint64 v) {
    for (nint i = 0; i < traceBytesPerNumber; i++) {
        if (i < (nint)(traceBytesPerNumber - 1)){
            buf.arr[pos] = (byte)(0x80 | (byte)v);
        } else {
            buf.arr[pos] = (byte)v;
        }
        v >>= (int)(7);
        pos++;
    }
    if (v != 0) {
        @throw(vCouldNotFitInˢ);
    }
}

// traceBufFlush flushes a trace buffer.
//
// Must run on the system stack because trace.lock must be held.
//
//go:systemstack
internal static void traceBufFlush(ж<traceBuf> Ꮡbuf, uintptr gen) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    assertLockHeld(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
    // Write out the non-header length of the batch in the header.
    //
    // Note: the length of the header is not included to make it easier
    // to calculate this value when deserializing and reserializing the
    // trace. Varints can have additional padding of zero bits that is
    // quite difficult to preserve, and if we include the header we
    // force serializers to do more work. Nothing else actually needs
    // padding.
    buf.varintAt(buf.lenPos, (uint64)(buf.pos - (buf.lenPos + (nint)traceBytesPerNumber)));
    Δtrace.full[gen % 2].push(Ꮡbuf);
    // Notify the scheduler that there's work available and that the trace
    // reader should be scheduled.
    if (!ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑworkAvailable).Load()) {
        ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑworkAvailable).Store(true);
    }
}

} // end runtime_package
