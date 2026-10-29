// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Trace string management.
namespace go;

using tracev2 = @internal.trace.tracev2_package;
using @internal.trace;
using @unsafe = unsafe_package;

partial class runtime_package {

// Trace strings.

// traceStringTable is map of string -> unique ID that also manages
// writing strings out into the trace.
//
// ID 0 is reserved for the empty string.
partial struct traceStringTable {
    // lock protects buf.
    internal mutex @lock;
    internal ж<traceBuf> buf; // string batches to write out to the trace.
    // tab is a mapping of string -> unique ID.
    internal traceMap tab;
}

// put adds a string to the table, emits it, and returns a unique ID for it.
internal static uint64 put(this ж<traceStringTable> Ꮡt, uintptr gen, @string sʗp) {
    ref var s = ref heap(sʗp, out var Ꮡs);

    // Put the string in the table.
    var ss = stringStructOf(Ꮡs);
    var (id, added) = Ꮡt.of(traceStringTable.Ꮡtab).put((~ss).str, (uintptr)(~ss).len);
    if (added) {
        // Write the string to the buffer.
        systemstack(() => {
            Ꮡt.writeString(gen, id, Ꮡs.Value);
        });
    }
    return id;
}

// emit emits a string and creates an ID for it, but doesn't add it to the table. Returns the ID.
internal static uint64 emit(this ж<traceStringTable> Ꮡt, uintptr gen, @string s) {
    if (len(s) == 0) {
        return 0; // Empty strings are implicitly assigned ID 0 already.
    }
    // Grab an ID and write the string to the buffer.
    var id = Ꮡt.of(traceStringTable.Ꮡtab).stealID();
    systemstack(() => {
        Ꮡt.writeString(gen, id, s);
    });
    return id;
}

// writeString writes the string to t.buf.
//
// Must run on the systemstack because it acquires t.lock.
//
//go:systemstack
internal static void writeString(this ж<traceStringTable> Ꮡt, uintptr gen, uint64 id, @string s) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Truncate the string if necessary.
    if (len(s) > tracev2.MaxEventTrailerDataSize) {
        s = s[..(int)(tracev2.MaxEventTrailerDataSize)];
    }

    @lock(Ꮡt.of(traceStringTable.Ꮡlock));
    var w = unsafeTraceWriter(gen, t.buf);

    // Ensure we have a place to write to.
    bool flushed = default;
    (w, flushed) = w.ensure((nint)(2 + 2 * traceBytesPerNumber) + len(s) /* tracev2.EvStrings + tracev2.EvString + ID + len + string data */);
    if (flushed) {
        // Annotate the batch as containing strings.
        w.@byte((byte)tracev2.EvStrings);
    }

    // Write out the string.
    w.@byte((byte)tracev2.EvString);
    w.varint(id);
    w.varint((uint64)len(s));
    w.stringData(s);

    // Store back buf in case it was updated during ensure.
    t.buf = w.traceBuf;
    unlock(Ꮡt.of(traceStringTable.Ꮡlock));
}

// reset clears the string table and flushes any buffers it has.
//
// Must be called only once the caller is certain nothing else will be
// added to this table.
internal static void reset(this ж<traceStringTable> Ꮡt, uintptr gen) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (t.buf != nil) {
        systemstack(() => {
            @lock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
            traceBufFlush(Ꮡt.Value.buf, gen);
            unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
        });
        t.buf = default;
    }

    // Reset the table.
    Ꮡt.of(traceStringTable.Ꮡtab).reset();
}

} // end runtime_package
