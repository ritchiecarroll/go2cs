// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// CPU profile -> trace
namespace go;

using @internal.runtime;
using @unsafe = unsafe_package;
using atomic = @internal.runtime.atomic_package;

partial class runtime_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string traceInitReadCPUCalledˢ = "traceInitReadCPU called with trace enabled"u8;

// traceInitReadCPU initializes CPU profile -> tracer state for tracing.
//
// Returns a profBuf for reading from.
internal static void traceInitReadCPU() {
    if (traceEnabled()) {
        @throw(traceInitReadCPUCalledˢ);
    }
    // Create new profBuf for CPU samples that will be emitted as events.
    // Format: after the timestamp, header is [pp.id, gp.goid, mp.procid].
    Δtrace.cpuLogRead[0] = newProfBuf(3, profBufWordCount, profBufTagCount);
    Δtrace.cpuLogRead[1] = newProfBuf(3, profBufWordCount, profBufTagCount);
    // We must not acquire trace.signalLock outside of a signal handler: a
    // profiling signal may arrive at any time and try to acquire it, leading to
    // deadlock. Because we can't use that lock to protect updates to
    // trace.cpuLogWrite (only use of the structure it references), reads and
    // writes of the pointer must be atomic. (And although this field is never
    // the sole pointer to the profBuf value, it's best to allow a write barrier
    // here.)
    ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑcpuLogWrite, 0).Store(Δtrace.cpuLogRead[0]);
    ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑcpuLogWrite, 1).Store(Δtrace.cpuLogRead[1]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string traceStartReadCPUCalledˢ = "traceStartReadCPU called with trace disabled"u8;

// traceStartReadCPU creates a goroutine to start reading CPU profile
// data into an active trace.
//
// traceAdvanceSema must be held.
internal static void traceStartReadCPU() {
    if (!traceEnabled()) {
        @throw(traceStartReadCPUCalledˢ);
    }
    // Spin up the logger goroutine.
    Δtrace.cpuSleep = newWakeableSleep();
    var done = new channel<EmptyStruct>(1);
    var doneʗ1 = done;
    goǃ(() => {
        while (traceEnabled()) {
            // Sleep here because traceReadCPU is non-blocking. This mirrors
            // how the runtime/pprof package obtains CPU profile data.
            //
            // We can't do a blocking read here because Darwin can't do a
            // wakeup from a signal handler, so all CPU profiling is just
            // non-blocking. See #61768 for more details.
            //
            // Like the runtime/pprof package, even if that bug didn't exist
            // we would still want to do a goroutine-level sleep in between
            // reads to avoid frequent wakeups.
            Δtrace.cpuSleep.sleep(100_000_000);
            ref var tl = ref heap<traceLocker>(out var Ꮡtl);
            tl = traceAcquire();
            if (!tl.ok()) {
                // Tracing disabled.
                break;
            }
            var keepGoing = traceReadCPU(tl.gen);
            traceRelease(tl);
            if (!keepGoing) {
                break;
            }
        }
        doneʗ1.ᐸꟷ(new EmptyStruct());
    });
    Δtrace.cpuLogDone = done.WithDirection(GoChanDir.Recv);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string traceStopReadCPUCalledˢ = "traceStopReadCPU called with trace enabled"u8;

// traceStopReadCPU blocks until the trace CPU reading goroutine exits.
//
// traceAdvanceSema must be held, and tracing must be disabled.
internal static void traceStopReadCPU() {
    if (traceEnabled()) {
        @throw(traceStopReadCPUCalledˢ);
    }
    // Once we close the profbuf, we'll be in one of two situations:
    // - The logger goroutine has already exited because it observed
    //   that the trace is disabled.
    // - The logger goroutine is asleep.
    //
    // Wake the goroutine so it can observe that their the buffer is
    // closed an exit.
    ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑcpuLogWrite, 0).Store(nil);
    ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑcpuLogWrite, 1).Store(nil);
    Δtrace.cpuLogRead[0].close();
    Δtrace.cpuLogRead[1].close();
    Δtrace.cpuSleep.wake();
    // Wait until the logger goroutine exits.
    ᐸꟷ(Δtrace.cpuLogDone);
    // Clear state for the next trace.
    Δtrace.cpuLogDone = default!;
    Δtrace.cpuLogRead[0] = default!;
    Δtrace.cpuLogRead[1] = default!;
    Δtrace.cpuSleep.close();
}

// traceReadCPU attempts to read from the provided profBuf[gen%2] and write
// into the trace. Returns true if there might be more to read or false
// if the profBuf is closed or the caller should otherwise stop reading.
//
// The caller is responsible for ensuring that gen does not change. Either
// the caller must be in a traceAcquire/traceRelease block, or must be calling
// with traceAdvanceSema held.
//
// No more than one goroutine may be in traceReadCPU for the same
// profBuf at a time.
//
// Must not run on the system stack because profBuf.read performs race
// operations.
internal static bool traceReadCPU(uintptr gen) {
    array<uintptr> pcBuf = new(128); /* traceStackSize */
    var (data, tags, eof) = Δtrace.cpuLogRead[(nint)(gen % 2)].read(profBufNonBlocking);
    while (len(data) > 0) {
        if (len(data) < 4 || data[0] > (uint64)len(data)) {
            break; // truncated profile
        }
        if (data[0] < 4 || tags != default! && len(tags) < 1) {
            break; // malformed profile
        }
        if (len(tags) < 1) {
            break; // mismatched profile records and tags
        }
        // Deserialize the data in the profile buffer.
        var recordLen = data[0];
        var timestamp = data[1];
        var ppid = (data[2] >> (int)(1));
        {
            var hasP = ((uint64)(data[2] & 0b1)) != 0; if (!hasP) {
                ppid = ~(uint64)0;
            }
        }
        var goid = data[3];
        var mpid = data[4];
        var stk = data[5..(int)(recordLen)];
        // Overflow records always have their headers contain
        // all zeroes.
        var isOverflowRecord = len(stk) == 1 && data[2] == 0 && data[3] == 0 && data[4] == 0;
        // Move the data iterator forward.
        data = data[(int)(recordLen)..];
        // No support here for reporting goroutine tags at the moment; if
        // that information is to be part of the execution trace, we'd
        // probably want to see when the tags are applied and when they
        // change, instead of only seeing them when we get a CPU sample.
        tags = tags[1..];
        if (isOverflowRecord) {
            // Looks like an overflow record from the profBuf. Not much to
            // do here, we only want to report full records.
            continue;
        }
        // Construct the stack for insertion to the stack table.
        nint nstk = 1;
        pcBuf[0] = logicalStackSentinel;
        for (; nstk < len(pcBuf) && nstk - 1 < len(stk); nstk++) {
            pcBuf[nstk] = (uintptr)stk[nstk - 1];
        }
        // Write out a trace event.
        var w = unsafeTraceWriter(gen, Δtrace.cpuBuf[(nint)(gen % 2)]);
        // Ensure we have a place to write to.
        bool flushed = default!;
        (w, flushed) = w.ensure(2 + 5 * traceBytesPerNumber);
        /* traceEvCPUSamples + traceEvCPUSample + timestamp + g + m + p + stack ID */
        if (flushed) {
            // Annotate the batch as containing strings.
            w.@byte((byte)traceEvCPUSamples);
        }
        // Add the stack to the table.
        var stackID = ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstackTab, (nint)(gen % 2)).put(pcBuf[..(int)(nstk)]);
        // Write out the CPU sample.
        w.@byte((byte)traceEvCPUSample);
        w.varint(timestamp);
        w.varint(mpid);
        w.varint(ppid);
        w.varint(goid);
        w.varint(stackID);
        Δtrace.cpuBuf[(nint)(gen % 2)] = w.traceBuf;
    }
    return !eof;
}

// traceCPUFlush flushes trace.cpuBuf[gen%2]. The caller must be certain that gen
// has completed and that there are no more writers to it.
internal static void traceCPUFlush(uintptr gen) {
    // Flush any remaining trace buffers containing CPU samples.
    {
        var buf = Δtrace.cpuBuf[(nint)(gen % 2)]; if (buf != nil) {
            var bufʗ1 = buf;
            systemstack(() => {
                @lock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
                traceBufFlush(bufʗ1, gen);
                unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
                Δtrace.cpuBuf[(nint)(gen % 2)] = default!;
            });
        }
    }
}

// traceCPUSample writes a CPU profile sample stack to the execution tracer's
// profiling buffer. It is called from a signal handler, so is limited in what
// it can do. mp must be the thread that is currently stopped in a signal.
internal static void traceCPUSample(ж<g> Ꮡgp, ж<m> Ꮡmp, ж<Δp> Ꮡpp, slice<uintptr> stk) {
    ref var gp = ref Ꮡgp.DerefOrNull();
    ref var mp = ref Ꮡmp.DerefOrNull();
    ref var pp = ref Ꮡpp.DerefOrNull();

    if (!traceEnabled()) {
        // Tracing is usually turned off; don't spend time acquiring the signal
        // lock unless it's active.
        return;
    }
    if (Ꮡmp == nil) {
        // Drop samples that don't have an identifiable thread. We can't render
        // this in any useful way anyway.
        return;
    }
    // We're going to conditionally write to one of two buffers based on the
    // generation. To make sure we write to the correct one, we need to make
    // sure this thread's trace seqlock is held. If it already is, then we're
    // in the tracer and we can just take advantage of that. If it isn't, then
    // we need to acquire it and read the generation.
    var locked = false;
    if (Ꮡmp.of(m.Ꮡtrace).of(mTraceState.Ꮡseqlock).Load() % 2 == 0) {
        Ꮡmp.of(m.Ꮡtrace).of(mTraceState.Ꮡseqlock).Add(1);
        locked = true;
    }
    var gen = ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡgen).Load();
    if (gen == 0) {
        // Tracing is disabled, as it turns out. Release the seqlock if necessary
        // and exit.
        if (locked) {
            Ꮡmp.of(m.Ꮡtrace).of(mTraceState.Ꮡseqlock).Add(1);
        }
        return;
    }
    var now = traceClockNow();
    // The "header" here is the ID of the M that was running the profiled code,
    // followed by the IDs of the P and goroutine. (For normal CPU profiling, it's
    // usually the number of samples with the given stack.) Near syscalls, pp
    // may be nil. Reporting goid of 0 is fine for either g0 or a nil gp.
    array<uint64> hdr = new(3);
    if (Ꮡpp != nil){
        // Overflow records in profBuf have all header values set to zero. Make
        // sure that real headers have at least one bit set.
        hdr[0] = (uint64)(((uint64)pp.id << (int)(1)) | 0b1);
    } else {
        hdr[0] = 0b10;
    }
    if (Ꮡgp != nil) {
        hdr[1] = gp.goid;
    }
    hdr[2] = (uint64)mp.procid;
    // Allow only one writer at a time
    while (!ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑsignalLock).CompareAndSwap(0, 1)) {
        // TODO: Is it safe to osyield here? https://go.dev/issue/52672
        osyield();
    }
    {
        var log = ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑcpuLogWrite, (nint)(gen % 2)).Load(); if (log != nil) {
            // Note: we don't pass a tag pointer here (how should profiling tags
            // interact with the execution tracer?), but if we did we'd need to be
            // careful about write barriers. See the long comment in profBuf.write.
            log.write(nil, (int64)(uint64)now, hdr[..], stk);
        }
    }
    ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑsignalLock).Store(0);
    // Release the seqlock if we acquired it earlier.
    if (locked) {
        Ꮡmp.of(m.Ꮡtrace).of(mTraceState.Ꮡseqlock).Add(1);
    }
}

} // end runtime_package
