// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using math = math_package;

partial class flate_package {

// This encoding algorithm, which prioritizes speed over output size, is
// based on Snappy's LZ77-style encoder: github.com/golang/snappy
internal static UntypedInt tableBits => 14; // Bits used in the table.
internal static UntypedInt tableSize => /* 1 << tableBits */ 16384; // Size of the table.
internal static UntypedInt tableMask => /* tableSize - 1 */ 16383; // Mask for table indices. Redundant, but can eliminate bounds checks.
internal static UntypedInt tableShift => /* 32 - tableBits */ 18; // Right-shift to get the tableBits most significant bits of a uint32.
internal static UntypedInt bufferReset => /* math.MaxInt32 - maxStoreBlockSize*2 */ 2147352577;

internal static uint32 load32(slice<byte> b, int32 i) {
    b = b.slice(i, i + 4, len(b)); // Help the compiler eliminate bounds checks on the next line.
    return (uint32)((uint32)((uint32)((uint32)b[0] | ((uint32)b[1] << (int)(8))) | ((uint32)b[2] << (int)(16))) | ((uint32)b[3] << (int)(24)));
}

internal static uint64 load64(slice<byte> b, int32 i) {
    b = b.slice(i, i + 8, len(b)); // Help the compiler eliminate bounds checks on the next line.
    return (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)b[0] | ((uint64)b[1] << (int)(8))) | ((uint64)b[2] << (int)(16))) | ((uint64)b[3] << (int)(24))) | ((uint64)b[4] << (int)(32))) | ((uint64)b[5] << (int)(40))) | ((uint64)b[6] << (int)(48))) | ((uint64)b[7] << (int)(56)));
}

internal static uint32 hash(uint32 u) {
    return ((u * 0x1e35a7bd) >> (int)(tableShift));
}

// These constants are defined by the Snappy implementation so that its
// assembly implementation can fast-path some 16-bytes-at-a-time copies. They
// aren't necessary in the pure Go implementation, as we don't use those same
// optimizations, but using the same thresholds doesn't really hurt.
internal static UntypedInt inputMargin => /* 16 - 1 */ 15;

internal static UntypedInt minNonLiteralBlockSize => /* 1 + 1 + inputMargin */ 17;

partial struct tableEntry {
    internal uint32 val; // Value at destination
    internal int32 offset;
}

// deflateFast maintains the table for matches,
// and the previous byte block for cross block matching.
partial struct deflateFast {
    internal array<tableEntry> table = new(tableSize);
    internal slice<byte> prev; // Previous block, zero length if unknown.
    internal int32 cur;  // Current match offset.
}

internal static ж<deflateFast> newDeflateFast() {
    return Ꮡ(new deflateFast(cur: maxStoreBlockSize, prev: new slice<byte>(0, maxStoreBlockSize)));
}

// encode encodes a block given in src and appends tokens
// to dst and returns the result.
internal static slice<token> encode(this ref deflateFast e, slice<token> dst, slice<byte> src) {
    // Ensure that e.cur doesn't wrap.
    if (e.cur >= bufferReset) {
        e.shiftOffsets();
    }
    // This check isn't in the Snappy implementation, but there, the caller
    // instead of the callee handles this case.
    if (len(src) < minNonLiteralBlockSize) {
        e.cur += maxStoreBlockSize;
        e.prev = e.prev[..0];
        return emitLiteral(dst, src);
    }
    // sLimit is when to stop looking for offset/length copies. The inputMargin
    // lets us use a fast path for emitLiteral in the main loop, while we are
    // looking for copies.
    var sLimit = (int32)(len(src) - (nint)inputMargin);
    // nextEmit is where in src the next emitLiteral should start from.
    var nextEmit = (int32)0;
    var s = (int32)0;
    var cv = load32(src, s);
    var nextHash = hash(cv);
    while (ᐧ) {
        // Copied from the C++ snappy implementation:
        //
        // Heuristic match skipping: If 32 bytes are scanned with no matches
        // found, start looking only at every other byte. If 32 more bytes are
        // scanned (or skipped), look at every third byte, etc.. When a match
        // is found, immediately go back to looking at every byte. This is a
        // small loss (~5% performance, ~0.1% density) for compressible data
        // due to more bookkeeping, but for non-compressible data (such as
        // JPEG) it's a huge win since the compressor quickly "realizes" the
        // data is incompressible and doesn't bother looking for matches
        // everywhere.
        //
        // The "skip" variable keeps track of how many bytes there are since
        // the last match; dividing it by 32 (ie. right-shifting by five) gives
        // the number of bytes to move ahead for each iteration.
        var skip = (int32)32;
        var nextS = s;
        tableEntry candidate = default!;
        while (ᐧ) {
            s = nextS;
            var bytesBetweenHashLookups = (skip >> (int)(5));
            nextS = s + bytesBetweenHashLookups;
            skip += bytesBetweenHashLookups;
            if (nextS > sLimit) {
                goto emitRemainder;
            }
            candidate = e.table[(uint32)(nextHash & (uint32)tableMask)];
            var now = load32(src, nextS);
            e.table[(uint32)(nextHash & (uint32)tableMask)] = new tableEntry(offset: s + e.cur, val: cv);
            nextHash = hash(now);
            var offset = s - (candidate.offset - e.cur);
            if (offset > maxMatchOffset || cv != candidate.val) {
                // Out of range or not matched.
                cv = now;
                continue;
            }
            break;
        }
        // A 4-byte match has been found. We'll later see if more than 4 bytes
        // match. But, prior to the match, src[nextEmit:s] are unmatched. Emit
        // them as literal bytes.
        dst = emitLiteral(dst, src.slice(nextEmit, s));
        // Call emitCopy, and then see if another emitCopy could be our next
        // move. Repeat until we find no match for the input immediately after
        // what was consumed by the last emitCopy call.
        //
        // If we exit this loop normally then we need to call emitLiteral next,
        // though we don't yet know how big the literal will be. We handle that
        // by proceeding to the next iteration of the main loop. We also can
        // exit this loop via goto if we get close to exhausting the input.
        while (ᐧ) {
            // Invariant: we have a 4-byte match at s, and no need to emit any
            // literal bytes prior to s.
            // Extend the 4-byte match as long as possible.
            //
            s += 4;
            var t = candidate.offset - e.cur + 4;
            var l = e.matchLen(s, t, src);
            // matchToken is flate's equivalent of Snappy's emitCopy. (length,offset)
            dst = append(dst, matchToken((uint32)(l + 4 - (int32)baseMatchLength), (uint32)(s - t - (int32)baseMatchOffset)));
            s += l;
            nextEmit = s;
            if (s >= sLimit) {
                goto emitRemainder;
            }
            // We could immediately start working at s now, but to improve
            // compression we first update the hash table at s-1 and at s. If
            // another emitCopy is not our next move, also calculate nextHash
            // at s+1. At least on GOARCH=amd64, these three hash calculations
            // are faster as one load64 call (with some shifts) instead of
            // three load32 calls.
            var x = load64(src, s - 1);
            var prevHash = hash((uint32)x);
            e.table[(uint32)(prevHash & (uint32)tableMask)] = new tableEntry(offset: e.cur + s - 1, val: (uint32)x);
            x >>= (int)(8);
            var currHash = hash((uint32)x);
            candidate = e.table[(uint32)(currHash & (uint32)tableMask)];
            e.table[(uint32)(currHash & (uint32)tableMask)] = new tableEntry(offset: e.cur + s, val: (uint32)x);
            var offset = s - (candidate.offset - e.cur);
            if (offset > maxMatchOffset || (uint32)x != candidate.val) {
                cv = (uint32)((x >> (int)(8)));
                nextHash = hash(cv);
                s++;
                break;
            }
        }
    }
emitRemainder:
    if ((nint)nextEmit < len(src)) {
        dst = emitLiteral(dst, src.slice(nextEmit));
    }
    e.cur += (int32)len(src);
    e.prev = e.prev.slice(0, len(src));
    copy(e.prev, src);
    return dst;
}

internal static slice<token> emitLiteral(slice<token> dst, slice<byte> lit) {
    foreach (var (_, v) in lit) {
        dst = append(dst, literalToken((uint32)v));
    }
    return dst;
}

// matchLen returns the match length between src[s:] and src[t:].
// t can be negative to indicate the match is starting in e.prev.
// We assume that src[s-4:s] and src[t-4:t] already match.
internal static int32 matchLen(this ref deflateFast e, int32 s, int32 t, slice<byte> src) {
    nint s1 = (nint)s + (nint)maxMatchLength - 4;
    if (s1 > len(src)) {
        s1 = len(src);
    }
    // If we are inside the current block
    if (t >= 0) {
        var bΔ1 = src.slice(t);
        var aΔ1 = src.slice(s, s1);
        bΔ1 = bΔ1.slice(0, len(aΔ1));
        // Extend the match to be as long as possible.
        foreach (var (i, _) in aΔ1) {
            if (aΔ1[i] != bΔ1[i]) {
                return (int32)i;
            }
        }
        return (int32)len(aΔ1);
    }
    // We found a match in the previous block.
    var tp = (int32)len(e.prev) + t;
    if (tp < 0) {
        return 0;
    }
    // Extend the match to be as long as possible.
    var a = src.slice(s, s1);
    var b = e.prev.slice(tp);
    if (len(b) > len(a)) {
        b = b.slice(0, len(a));
    }
    a = a.slice(0, len(b));
    foreach (var (i, _) in b) {
        if (a[i] != b[i]) {
            return (int32)i;
        }
    }
    // If we reached our limit, we matched everything we are
    // allowed to in the previous block and we return.
    var n = (int32)len(b);
    if ((nint)(s + n) == s1) {
        return n;
    }
    // Continue looking for more matches in the current block.
    a = src.slice(s + n, s1);
    b = src.slice(0, len(a));
    foreach (var (i, _) in a) {
        if (a[i] != b[i]) {
            return (int32)i + n;
        }
    }
    return (int32)len(a) + n;
}

// Reset resets the encoding history.
// This ensures that no matches are made to the previous block.
internal static void reset(this ref deflateFast e) {
    e.prev = e.prev[..0];
    // Bump the offset, so all matches will fail distance check.
    // Nothing should be >= e.cur in the table.
    e.cur += maxMatchOffset;
    // Protect against e.cur wraparound.
    if (e.cur >= bufferReset) {
        e.shiftOffsets();
    }
}

// shiftOffsets will shift down all match offset.
// This is only called in rare situations to prevent integer overflow.
//
// See https://golang.org/issue/18636 and https://github.com/golang/go/issues/34121.
internal static void shiftOffsets(this ref deflateFast e) {
    if (len(e.prev) == 0) {
        // We have no history; just clear the table.
        clear(e.table[..]);
        e.cur = maxMatchOffset + 1;
        return;
    }
    // Shift down everything in the table that isn't already too far away.
    foreach (var (i, _) in e.table[..]) {
        var v = e.table[i].offset - e.cur + (int32)maxMatchOffset + 1;
        if (v < 0) {
            // We want to reset e.cur to maxMatchOffset + 1, so we need to shift
            // all table entries down by (e.cur - (maxMatchOffset + 1)).
            // Because we ignore matches > maxMatchOffset, we can cap
            // any negative offsets at 0.
            v = 0;
        }
        e.table[i].offset = v;
    }
    e.cur = maxMatchOffset + 1;
}

} // end flate_package
