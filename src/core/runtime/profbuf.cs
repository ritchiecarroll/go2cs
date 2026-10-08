// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// A profBuf is a lock-free buffer for profiling events,
// safe for concurrent use by one reader and one writer.
// The writer may be a signal handler running without a user g.
// The reader is assumed to be a user g.
//
// Each logged event corresponds to a fixed size header, a list of
// uintptrs (typically a stack), and exactly one unsafe.Pointer tag.
// The header and uintptrs are stored in the circular buffer data and the
// tag is stored in a circular buffer tags, running in parallel.
// In the circular buffer data, each event takes 2+hdrsize+len(stk)
// words: the value 2+hdrsize+len(stk), then the time of the event, then
// hdrsize words giving the fixed-size header, and then len(stk) words
// for the stack.
//
// The current effective offsets into the tags and data circular buffers
// for reading and writing are stored in the high 30 and low 32 bits of r and w.
// The bottom bits of the high 32 are additional flag bits in w, unused in r.
// "Effective" offsets means the total number of reads or writes, mod 2^length.
// The offset in the buffer is the effective offset mod the length of the buffer.
// To make wraparound mod 2^length match wraparound mod length of the buffer,
// the length of the buffer must be a power of two.
//
// If the reader catches up to the writer, a flag passed to read controls
// whether the read blocks until more data is available. A read returns a
// pointer to the buffer data itself; the caller is assumed to be done with
// that data at the next read. The read offset rNext tracks the next offset to
// be returned by read. By definition, r ≤ rNext ≤ w (before wraparound),
// and rNext is only used by the reader, so it can be accessed without atomics.
//
// If the writer gets ahead of the reader, so that the buffer fills,
// future writes are discarded and replaced in the output stream by an
// overflow entry, which has size 2+hdrsize+1, time set to the time of
// the first discarded write, a header of all zeroed words, and a "stack"
// containing one word, the number of discarded writes.
//
// Between the time the buffer fills and the buffer becomes empty enough
// to hold more data, the overflow entry is stored as a pending overflow
// entry in the fields overflow and overflowTime. The pending overflow
// entry can be turned into a real record by either the writer or the
// reader. If the writer is called to write a new record and finds that
// the output buffer has room for both the pending overflow entry and the
// new record, the writer emits the pending overflow entry and the new
// record into the buffer. If the reader is called to read data and finds
// that the output buffer is empty but that there is a pending overflow
// entry, the reader will return a synthesized record for the pending
// overflow entry.
//
// Only the writer can create or add to a pending overflow entry, but
// either the reader or the writer can clear the pending overflow entry.
// A pending overflow entry is indicated by the low 32 bits of 'overflow'
// holding the number of discarded writes, and overflowTime holding the
// time of the first discarded write. The high 32 bits of 'overflow'
// increment each time the low 32 bits transition from zero to non-zero
// or vice versa. This sequence number avoids ABA problems in the use of
// compare-and-swap to coordinate between reader and writer.
// The overflowTime is only written when the low 32 bits of overflow are
// zero, that is, only when there is no pending overflow entry, in
// preparation for creating a new one. The reader can therefore fetch and
// clear the entry atomically using
//
//	for {
//		overflow = load(&b.overflow)
//		if uint32(overflow) == 0 {
//			// no pending entry
//			break
//		}
//		time = load(&b.overflowTime)
//		if cas(&b.overflow, overflow, ((overflow>>32)+1)<<32) {
//			// pending entry cleared
//			break
//		}
//	}
//	if uint32(overflow) > 0 {
//		emit entry for uint32(overflow), time
//	}
partial struct profBuf {
    // accessed atomically
    internal profAtomic r, w;
    internal atomic.Uint64 overflow;
    internal atomic.Uint64 overflowTime;
    internal atomic.Uint32 eof;
    // immutable (excluding slice content)
    internal uintptr hdrsize;
    internal slice<uint64> data;
    internal slice<@unsafe.Pointer> tags;
    // owned by reader
    internal profIndex rNext;
    internal slice<uint64> overflowBuf; // for use by reader to return overflow record
    internal note wait;
}

partial struct profAtomic /*num:uint64*/;

partial struct profIndex /*num:uint64*/;

internal static profIndex profReaderSleeping => /* 1 << 32 */ unchecked((profIndex)4294967296);   // reader is sleeping and must be woken up
internal static profIndex profWriteExtra => /* 1 << 33 */ unchecked((profIndex)8589934592);       // overflow or eof waiting

internal static profIndex load(this ж<profAtomic> Ꮡx) {
    return ((profIndex)atomic.Load64(Ꮡx.Reinterpret<profAtomic, uint64>()));
}

internal static void store(this ж<profAtomic> Ꮡx, profIndex @new) {
    atomic.Store64(Ꮡx.Reinterpret<profAtomic, uint64>(), (uint64)@new);
}

internal static bool cas(this ж<profAtomic> Ꮡx, profIndex old, profIndex @new) {
    return atomic.Cas64(Ꮡx.Reinterpret<profAtomic, uint64>(), (uint64)old, (uint64)@new);
}

internal static uint32 dataCount(this profIndex x) {
    return (uint32)(uint64)x;
}

internal static uint32 tagCount(this profIndex x) {
    return (uint32)(uint64)((x >> (int)(34)));
}

// countSub subtracts two counts obtained from profIndex.dataCount or profIndex.tagCount,
// assuming that they are no more than 2^29 apart (guaranteed since they are never more than
// len(data) or len(tags) apart, respectively).
// tagCount wraps at 2^30, while dataCount wraps at 2^32.
// This function works for both.
internal static nint countSub(uint32 x, uint32 y) {
    // x-y is 32-bit signed or 30-bit signed; sign-extend to 32 bits and convert to int.
    return (nint)((((int32)(x - y) << (int)(2)) >> (int)(2)));
}

// addCountsAndClearFlags returns the packed form of "x + (data, tag) - all flags".
internal static profIndex addCountsAndClearFlags(this profIndex x, nint data, nint tag) {
    return ((profIndex)((uint64)(((((uint64)x >> (int)(34)) + (uint64)((((uint32)tag << (int)(2)) >> (int)(2)))) << (int)(34)) | (uint64)((uint32)(uint64)x + (uint32)data))));
}

// hasOverflow reports whether b has any overflow records pending.
internal static bool hasOverflow(this ж<profBuf> Ꮡb) {
    return (uint32)Ꮡb.of(profBuf.Ꮡoverflow).Load() > 0;
}

// takeOverflow consumes the pending overflow records, returning the overflow count
// and the time of the first overflow.
// When called by the reader, it is racing against incrementOverflow.
internal static (uint32 count, uint64 time) takeOverflow(this ж<profBuf> Ꮡb) {
    uint32 count = default!;
    uint64 time = default!;

    var overflow = Ꮡb.of(profBuf.Ꮡoverflow).Load();
    time = Ꮡb.of(profBuf.ᏑoverflowTime).Load();
    while (ᐧ) {
        count = (uint32)overflow;
        if (count == 0) {
            time = 0;
            break;
        }
        // Increment generation, clear overflow count in low bits.
        if (Ꮡb.of(profBuf.Ꮡoverflow).CompareAndSwap(overflow, ((((overflow >> (int)(32))) + 1) << (int)(32)))) {
            break;
        }
        overflow = Ꮡb.of(profBuf.Ꮡoverflow).Load();
        time = Ꮡb.of(profBuf.ᏑoverflowTime).Load();
    }
    return ((uint32)overflow, time);
}

// incrementOverflow records a single overflow at time now.
// It is racing against a possible takeOverflow in the reader.
internal static void incrementOverflow(this ж<profBuf> Ꮡb, int64 now) {
    ref var b = ref Ꮡb.DerefOrNull();

    while (ᐧ) {
        var overflow = Ꮡb.of(profBuf.Ꮡoverflow).Load();
        // Once we see b.overflow reach 0, it's stable: no one else is changing it underfoot.
        // We need to set overflowTime if we're incrementing b.overflow from 0.
        if ((uint32)overflow == 0) {
            // Store overflowTime first so it's always available when overflow != 0.
            Ꮡb.of(profBuf.ᏑoverflowTime).Store((uint64)now);
            Ꮡb.of(profBuf.Ꮡoverflow).Store((((((overflow >> (int)(32))) + 1) << (int)(32))) + 1);
            break;
        }
        // Otherwise we're racing to increment against reader
        // who wants to set b.overflow to 0.
        // Out of paranoia, leave 2³²-1 a sticky overflow value,
        // to avoid wrapping around. Extremely unlikely.
        if ((int32)overflow == -1) {
            break;
        }
        if (Ꮡb.of(profBuf.Ꮡoverflow).CompareAndSwap(overflow, overflow + 1)) {
            break;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string newProfBufBufferTooLargeˢ = "newProfBuf: buffer too large"u8;

// newProfBuf returns a new profiling buffer with room for
// a header of hdrsize words and a buffer of at least bufwords words.
internal static ж<profBuf> newProfBuf(nint hdrsize, nint bufwords, nint tags) {
    {
        nint min = 2 + hdrsize + 1; if (bufwords < min) {
            bufwords = min;
        }
    }
    // Buffer sizes must be power of two, so that we don't have to
    // worry about uint32 wraparound changing the effective position
    // within the buffers. We store 30 bits of count; limiting to 28
    // gives us some room for intermediate calculations.
    if (bufwords >= (1 << (int)(28)) || tags >= (1 << (int)(28))) {
        @throw(newProfBufBufferTooLargeˢ);
    }
    nint i = default!;
    for (i = 1; i < bufwords; i <<= (int)(1)) {
    }
    bufwords = i;
    for (i = 1; i < tags; i <<= (int)(1)) {
    }
    tags = i;
    var b = @new<profBuf>();
    b.Value.hdrsize = (uintptr)hdrsize;
    b.Value.data = new slice<uint64>(bufwords);
    b.Value.tags = new slice<@unsafe.Pointer>(tags);
    b.Value.overflowBuf = new slice<uint64>((nint)(2 + (~b).hdrsize + 1));
    return b;
}

// canWriteRecord reports whether the buffer has room
// for a single contiguous record with a stack of length nstk.
internal static bool canWriteRecord(this ж<profBuf> Ꮡb, nint nstk) {
    ref var b = ref Ꮡb.DerefOrNull();

    var br = Ꮡb.of(profBuf.Ꮡr).load();
    var bw = Ꮡb.of(profBuf.Ꮡw).load();
    // room for tag?
    if (countSub(br.tagCount(), bw.tagCount()) + len(b.tags) < 1) {
        return false;
    }
    // room for data?
    nint nd = countSub(br.dataCount(), bw.dataCount()) + len(b.data);
    nint want = 2 + (nint)b.hdrsize + nstk;
    nint i = (nint)(bw.dataCount() % (uint32)len(b.data));
    if (i + want > len(b.data)) {
        // Can't fit in trailing fragment of slice.
        // Skip over that and start over at beginning of slice.
        nd -= len(b.data) - i;
    }
    return nd >= want;
}

// canWriteTwoRecords reports whether the buffer has room
// for two records with stack lengths nstk1, nstk2, in that order.
// Each record must be contiguous on its own, but the two
// records need not be contiguous (one can be at the end of the buffer
// and the other can wrap around and start at the beginning of the buffer).
internal static bool canWriteTwoRecords(this ж<profBuf> Ꮡb, nint nstk1, nint nstk2) {
    ref var b = ref Ꮡb.DerefOrNull();

    var br = Ꮡb.of(profBuf.Ꮡr).load();
    var bw = Ꮡb.of(profBuf.Ꮡw).load();
    // room for tag?
    if (countSub(br.tagCount(), bw.tagCount()) + len(b.tags) < 2) {
        return false;
    }
    // room for data?
    nint nd = countSub(br.dataCount(), bw.dataCount()) + len(b.data);
    // first record
    nint want = 2 + (nint)b.hdrsize + nstk1;
    nint i = (nint)(bw.dataCount() % (uint32)len(b.data));
    if (i + want > len(b.data)) {
        // Can't fit in trailing fragment of slice.
        // Skip over that and start over at beginning of slice.
        nd -= len(b.data) - i;
        i = 0;
    }
    i += want;
    nd -= want;
    // second record
    want = 2 + (nint)b.hdrsize + nstk2;
    if (i + want > len(b.data)) {
        // Can't fit in trailing fragment of slice.
        // Skip over that and start over at beginning of slice.
        nd -= len(b.data) - i;
        i = 0;
    }
    return nd >= want;
}

// go2cs generated this placeholder — func write is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeProfBufAlreadyˢ = "runtime: profBuf already closed"u8;

// close signals that there will be no more writes on the buffer.
// Once all the data has been read from the buffer, reads will return eof=true.
internal static void close(this ж<profBuf> Ꮡb) {
    if (Ꮡb.of(profBuf.Ꮡeof).Load() > 0) {
        @throw(runtimeProfBufAlreadyˢ);
    }
    Ꮡb.of(profBuf.Ꮡeof).Store(1);
    Ꮡb.wakeupExtra();
}

// wakeupExtra must be called after setting one of the "extra"
// atomic fields b.overflow or b.eof.
// It records the change in b.w and wakes up the reader if needed.
internal static void wakeupExtra(this ж<profBuf> Ꮡb) {
    while (ᐧ) {
        var old = Ꮡb.of(profBuf.Ꮡw).load();
        var @new = (profIndex)(old | profWriteExtra);
        if (!Ꮡb.of(profBuf.Ꮡw).cas(old, @new)) {
            continue;
        }
        if ((profIndex)(old & profReaderSleeping) != 0) {
            notewakeup(Ꮡb.of(profBuf.Ꮡwait));
        }
        break;
    }
}

public partial struct profBufReadMode /*num:nint*/;

internal static profBufReadMode profBufBlocking => /* iota */ 0;
internal static profBufReadMode profBufNonBlocking => 1;

internal static array<@unsafe.Pointer> overflowTag = new(1);           // always nil

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeMalformedProfBufˢ = "runtime: malformed profBuf buffer - tag and data out of sync"u8;
internal static readonly @string runtimeMalformedProfBufˢ2 = "runtime: malformed profBuf buffer - invalid size"u8;

internal static (slice<uint64> data, slice<@unsafe.Pointer> tags, bool eof) read(this ж<profBuf> Ꮡb, profBufReadMode mode) {
    slice<uint64> data = default!;
    slice<@unsafe.Pointer> tags = default!;

    ref var b = ref Ꮡb.DerefOrNull();
    if (Ꮡb == nil) {
        return (default!, default!, true);
    }
    var br = b.rNext;
    // Commit previous read, returning that part of the ring to the writer.
    // First clear tags that have now been read, both to avoid holding
    // up the memory they point at for longer than necessary
    // and so that b.write can assume it is always overwriting
    // nil tag entries (see comment in b.write).
    var rPrev = Ꮡb.of(profBuf.Ꮡr).load();
    if (rPrev != br) {
        nint ntagΔ1 = countSub(br.tagCount(), rPrev.tagCount());
        nint tiΔ1 = (nint)(rPrev.tagCount() % (uint32)len(b.tags));
        for (nint i = 0; i < ntagΔ1; i++) {
            b.tags[tiΔ1] = default!;
            {
                tiΔ1++; if (tiΔ1 == len(b.tags)) {
                    tiΔ1 = 0;
                }
            }
        }
        Ꮡb.of(profBuf.Ꮡr).store(br);
    }
Read:
    var bw = Ꮡb.of(profBuf.Ꮡw).load();
    nint numData = countSub(bw.dataCount(), br.dataCount());
    if (numData == 0) {
        if (Ꮡb.hasOverflow()) {
            // No data to read, but there is overflow to report.
            // Racing with writer flushing b.overflow into a real record.
            var (count, time) = Ꮡb.takeOverflow();
            if (count == 0) {
                // Lost the race, go around again.
                goto Read;
            }
            // Won the race, report overflow.
            var dst = b.overflowBuf;
            dst[0] = (uint64)(2 + b.hdrsize + 1);
            dst[1] = time;
            builtin.clear(dst.slice(2, (nint)(2 + b.hdrsize)));
            dst[2 + b.hdrsize] = (uint64)count;
            return (dst.slice(0, (nint)(2 + b.hdrsize + 1)), overflowTag[..1], false);
        }
        if (Ꮡb.of(profBuf.Ꮡeof).Load() > 0) {
            // No data, no overflow, EOF set: done.
            return (default!, default!, true);
        }
        if ((profIndex)(bw & profWriteExtra) != 0) {
            // Writer claims to have published extra information (overflow or eof).
            // Attempt to clear notification and then check again.
            // If we fail to clear the notification it means b.w changed,
            // so we still need to check again.
            Ꮡb.of(profBuf.Ꮡw).cas(bw, (profIndex)(bw & ~profWriteExtra));
            goto Read;
        }
        // Nothing to read right now.
        // Return or sleep according to mode.
        if (mode == profBufNonBlocking) {
            // Necessary on Darwin, notetsleepg below does not work in signal handler, root cause of #61768.
            return (default!, default!, false);
        }
        if (!Ꮡb.of(profBuf.Ꮡw).cas(bw, (profIndex)(bw | profReaderSleeping))) {
            goto Read;
        }
        // Committed to sleeping.
        notetsleepg(Ꮡb.of(profBuf.Ꮡwait), -1);
        noteclear(Ꮡb.of(profBuf.Ꮡwait));
        goto Read;
    }
    data = b.data.slice((nint)(br.dataCount() % (uint32)len(b.data)));
    if (len(data) > numData){
        data = data.slice(0, numData);
    } else {
        numData -= len(data); // available in case of wraparound
    }
    nint skip = 0;
    if (data[0] == 0) {
        // Wraparound record. Go back to the beginning of the ring.
        skip = len(data);
        data = b.data;
        if (len(data) > numData) {
            data = data.slice(0, numData);
        }
    }
    nint ntag = countSub(bw.tagCount(), br.tagCount());
    if (ntag == 0) {
        @throw(runtimeMalformedProfBufˢ);
    }
    tags = b.tags.slice((nint)(br.tagCount() % (uint32)len(b.tags)));
    if (len(tags) > ntag) {
        tags = tags.slice(0, ntag);
    }
    // Count out whole data records until either data or tags is done.
    // They are always in sync in the buffer, but due to an end-of-slice
    // wraparound we might need to stop early and return the rest
    // in the next call.
    nint di = 0;
    nint ti = 0;
    while (di < len(data) && data[di] != 0 && ti < len(tags)) {
        if ((uintptr)di + (uintptr)data[di] > (uintptr)len(data)) {
            @throw(runtimeMalformedProfBufˢ2);
        }
        di += (nint)data[di];
        ti++;
    }
    // Remember how much we returned, to commit read on next call.
    b.rNext = br.addCountsAndClearFlags(skip + di, ti);
    if (raceenabled) {
        // Match racereleasemerge in runtime_setProfLabel,
        // so that the setting of the labels in runtime_setProfLabel
        // is treated as happening before any use of the labels
        // by our caller. The synchronization on labelSync itself is a fiction
        // for the race detector. The actual synchronization is handled
        // by the fact that the signal handler only reads from the current
        // goroutine and uses atomics to write the updated queue indices,
        // and then the read-out from the signal handler buffer uses
        // atomics to read those queue indices.
        raceacquire(@unsafe.Pointer.FromBox(ᏑlabelSync));
    }
    return (data.slice(0, di), tags.slice(0, ti), false);
}

} // end runtime_package
