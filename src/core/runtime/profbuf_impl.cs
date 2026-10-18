// profbuf_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// profBuf.write, hand-owned for its tag store (manualConversionFuncs["runtime"], goosAny; declared once,
// in profbuf.go). The labels piece of section 11.6, docs/phase4/DESIGN-managed-profiling.md.
//
// Go stores a record's labels pointer without a write barrier, because it runs in the SIGPROF handler:
//
//     *(*uintptr)(unsafe.Pointer(&b.tags[wt])) = uintptr(*tagPtr)
//
// The conversion reinterprets a reference slot of b.tags as an integer, which cannot hold a managed
// reference, so the first labelled CPU sample threw, and the sampler seam (cpusampler_impl.cs) dropped
// that sample and every one after it. Here the slot takes the reference itself. There is no signal
// handler and no barrier to avoid: the CLR's own store keeps the labels reachable from b.tags, which is
// the property Go's comment argues for by hand. Every other line is Go's, at go1.26.9: the commit loop
// carries Go 1.26's wakeup threshold (the reader is woken at half full, not on every write).
//
// Hand-owned (no profbuf_impl.go exists, so a reconvert never regenerates this file).
[module: go.GoManualConversion]

namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// write writes an entry to the profiling buffer b.
// The entry begins with a fixed hdr, which must have
// length b.hdrsize, followed by a variable-sized stack
// and a single tag pointer *tagPtr (or nil if tagPtr is nil).
internal static void write(this ж<profBuf> Ꮡb, ж<@unsafe.Pointer> ᏑtagPtr, int64 now, slice<uint64> hdr, slice<uintptr> stk) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡb == nil) {
        return;
    }
    if (len(hdr) > (nint)b.hdrsize) {
        @throw("misuse of profBuf.write"u8);
    }
    {
        var hasOverflow = Ꮡb.hasOverflow(); if (hasOverflow && Ꮡb.canWriteTwoRecords(1, len(stk))){
            // Room for both an overflow record and the one being written.
            // Write the overflow record if the reader hasn't gotten to it yet.
            // Only racing against reader, not other writers.
            var (count, time) = Ꮡb.takeOverflow();
            if (count > 0) {
                array<uintptr> stkΔ1 = new(1);
                stkΔ1[0] = (uintptr)count;
                Ꮡb.write(nil, (int64)time, default!, stkΔ1[..]);
            }
        } else
        if (hasOverflow || !Ꮡb.canWriteRecord(len(stk))) {
            // Pending overflow without room to write overflow and new records
            // or no overflow but also no room for new record.
            Ꮡb.incrementOverflow(now);
            Ꮡb.wakeupExtra();
            return;
        }
    }
    // There's room: write the record.
    var br = Ꮡb.of(profBuf.Ꮡr).load();
    var bw = Ꮡb.of(profBuf.Ꮡw).load();
    // Profiling tag: the labels reference itself (Go's store is a barrier-free uintptr write; see the
    // header). Go arranges that it always overwrites a nil; read clears each slot it consumes.
    nint wt = (nint)(bw.tagCount() % (uint32)len(b.tags));
    if (ᏑtagPtr != nil) {
        b.tags[wt] = ᏑtagPtr.Value;
    }
    // Main record.
    // It has to fit in a contiguous section of the slice, so if it doesn't fit at the end,
    // leave a rewind marker (0) and start over at the beginning of the slice.
    nint wd = (nint)(bw.dataCount() % (uint32)len(b.data));
    nint nd = countSub(br.dataCount(), bw.dataCount()) + len(b.data);
    nint skip = 0;
    if (wd + 2 + (nint)b.hdrsize + len(stk) > len(b.data)) {
        b.data[wd] = 0;
        skip = len(b.data) - wd;
        nd -= skip;
        wd = 0;
    }
    var data = b.data[(int)(wd)..];
    data[0] = (uint64)(2 + b.hdrsize + (uintptr)len(stk)); // length
    data[1] = (uint64)now; // time stamp
    // header, zero-padded
    nint i = copy(data[2..(int)(2 + b.hdrsize)], hdr);
    builtin.clear(data[(int)(2 + i)..(int)(2 + b.hdrsize)]);
    foreach (var (iΔ1, pc) in stk) {
        data[(nint)(2 + b.hdrsize + (uintptr)iΔ1)] = (uint64)pc;
    }
    while (ᐧ) {
        // Commit write.
        // Racing with reader setting flag bits in b.w, to avoid lost wakeups.
        var old = Ꮡb.of(profBuf.Ꮡw).load();
        var @new = old.addCountsAndClearFlags(skip + 2 + len(stk) + (nint)b.hdrsize, 1);
        // We re-load b.r here to reduce the likelihood of early wakeups
        // if the reader already consumed some data between the last
        // time we read b.r and now. This isn't strictly necessary.
        nint unread = countSub(@new.dataCount(), Ꮡb.of(profBuf.Ꮡr).load().dataCount());
        if (unread < 0) {
            // The new count overflowed and wrapped around.
            unread += len(b.data);
        }
        nint wakeupThreshold = len(b.data) / 2;
        if (unread < wakeupThreshold) {
            // Carry over the sleeping flag since we're not planning
            // to wake the reader yet
            @new |= (profIndex)(old & profReaderSleeping);
        }
        if (!Ꮡb.of(profBuf.Ꮡw).cas(old, @new)) {
            continue;
        }
        // If we've hit our high watermark for data in the buffer,
        // and there is a reader, wake it up.
        if (unread >= wakeupThreshold && (profIndex)(old & profReaderSleeping) != 0) {
            // NB: if we reach this point, then the sleeping bit is
            // cleared in the new b.w value
            notewakeup(Ꮡb.of(profBuf.Ꮡwait));
        }
        break;
    }
}

} // end runtime_package
