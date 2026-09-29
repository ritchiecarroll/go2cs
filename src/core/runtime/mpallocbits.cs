// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using sys = @internal.runtime.sys_package;
using @internal.runtime;

partial class runtime_package {

[GoType("[8]uint64")] /* [pallocChunkPages / 64]uint64 */
partial struct pageBits;

// get returns the value of the i'th bit in the bitmap.
[GoRecv] internal static nuint get(this ref pageBits b, nuint i) {
    return (nuint)((uint64)(((b.Value[i / 64] >> (int)((i % 64)))) & 1));
}

// block64 returns the 64-bit aligned block of bits containing the i'th bit.
[GoRecv] internal static uint64 block64(this ref pageBits b, nuint i) {
    return b.Value[i / 64];
}

// set sets bit i of pageBits.
[GoRecv] internal static void set(this ref pageBits b, nuint i) {
    b.Value[i / 64] |= (uint64)(((uint64)1 << (int)((i % 64))));
}

// setRange sets bits in the range [i, i+n).
[GoRecv] internal static void setRange(this ref pageBits b, nuint i, nuint n) {
    _ = b.Value[i / 64];
    if (n == 1) {
        // Fast path for the n == 1 case.
        b.set(i);
        return;
    }
    // Set bits [i, j].
    nuint j = i + n - 1;
    if (i / 64 == j / 64) {
        b.Value[i / 64] |= (uint64)((((((uint64)1).Lsh(n)) - 1) << (int)((i % 64))));
        return;
    }
    _ = b.Value[j / 64];
    // Set leading bits.
    b.Value[i / 64] |= (uint64)((~(uint64)0 << (int)((i % 64))));
    for (nuint k = i / 64 + 1; k < j / 64; k++) {
        b.Value[k] = ~(uint64)0;
    }
    // Set trailing bits.
    b.Value[j / 64] |= (uint64)((((uint64)1).Lsh((j % 64 + 1))) - 1);
}

// setAll sets all the bits of b.
[GoRecv] internal static void setAll(this ref pageBits b) {
    foreach (var (i, _) in b.Value) {
        b.Value[i] = ~(uint64)0;
    }
}

// setBlock64 sets the 64-bit aligned block of bits containing the i'th bit that
// are set in v.
[GoRecv] internal static void setBlock64(this ref pageBits b, nuint i, uint64 v) {
    b.Value[i / 64] |= (uint64)(v);
}

// clear clears bit i of pageBits.
[GoRecv] internal static void clear(this ref pageBits b, nuint i) {
    b.Value[i / 64] &= unchecked((uint64)~(uint64)(((uint64)1 << (int)((i % 64)))));
}

// clearRange clears bits in the range [i, i+n).
[GoRecv] internal static void clearRange(this ref pageBits b, nuint i, nuint n) {
    _ = b.Value[i / 64];
    if (n == 1) {
        // Fast path for the n == 1 case.
        b.clear(i);
        return;
    }
    // Clear bits [i, j].
    nuint j = i + n - 1;
    if (i / 64 == j / 64) {
        b.Value[i / 64] &= unchecked((uint64)~(uint64)((((((uint64)1).Lsh(n)) - 1) << (int)((i % 64)))));
        return;
    }
    _ = b.Value[j / 64];
    // Clear leading bits.
    b.Value[i / 64] &= unchecked((uint64)~(uint64)((~(uint64)0 << (int)((i % 64)))));
    builtin.clear(b.Value.slice((nint)(i / 64 + 1), (nint)(j / 64)));
    // Clear trailing bits.
    b.Value[j / 64] &= unchecked((uint64)~(uint64)((((uint64)1).Lsh((j % 64 + 1))) - 1));
}

// clearAll frees all the bits of b.
[GoRecv] internal static void clearAll(this ref pageBits b) {
    builtin.clear(b.Value[..]);
}

// clearBlock64 clears the 64-bit aligned block of bits containing the i'th bit that
// are set in v.
[GoRecv] internal static void clearBlock64(this ref pageBits b, nuint i, uint64 v) {
    b.Value[i / 64] &= unchecked((uint64)~(uint64)(v));
}

// popcntRange counts the number of set bits in the
// range [i, i+n).
[GoRecv] internal static nuint /*s*/ popcntRange(this ref pageBits b, nuint i, nuint n) {
    nuint s = default!;

    if (n == 1) {
        return (nuint)((uint64)(((b.Value[i / 64] >> (int)((i % 64)))) & 1));
    }
    _ = b.Value[i / 64];
    nuint j = i + n - 1;
    if (i / 64 == j / 64) {
        return (nuint)sys.OnesCount64((uint64)(((b.Value[i / 64] >> (int)((i % 64)))) & ((((uint64)1).Lsh(n)) - 1)));
    }
    _ = b.Value[j / 64];
    s += (nuint)sys.OnesCount64((b.Value[i / 64] >> (int)((i % 64))));
    for (nuint k = i / 64 + 1; k < j / 64; k++) {
        s += (nuint)sys.OnesCount64(b.Value[k]);
    }
    s += (nuint)sys.OnesCount64((uint64)(b.Value[j / 64] & ((((uint64)1).Lsh((j % 64 + 1))) - 1)));
    return s;
}

[GoType("pageBits")] partial struct pallocBits;

// summarize returns a packed summary of the bitmap in pallocBits.
internal static pallocSum summarize(this ж<pallocBits> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nuint start = default!;
    nuint most = default!;
    nuint cur = default!;
    nuint notSetYet = /* ^uint(0) */ unchecked((nuint)18446744073709551615); // sentinel for start value
    start = notSetYet;
    for (nint i = 0; i < 8; i++) {
        var x = b.Value[i];
        if (x == 0) {
            cur += 64;
            continue;
        }
        nuint t = (nuint)sys.TrailingZeros64(x);
        nuint l = (nuint)sys.LeadingZeros64(x);
        // Finish any region spanning the uint64s
        cur += t;
        if (start == notSetYet) {
            start = cur;
        }
        most = builtin.max(most, cur);
        // Final region that might span to next uint64
        cur = l;
    }
    if (start == notSetYet) {
        // Made it all the way through without finding a single 1 bit.
        const nuint n = /* uint(64 * len(b)) */ 512;
        return packPallocSum(n, n, n);
    }
    most = builtin.max(most, cur);
    if (most >= 64 - 2) {
        // There is no way an internal run of zeros could beat max.
        return packPallocSum(start, most, cur);
    }
    // Now look inside each uint64 for runs of zeros.
    // All uint64s must be nonzero, or we would have aborted above.
outer:
    for (nint i = 0; i < 8; i++) {
        var x = b.Value[i];
        // Look inside this uint64. We have a pattern like
        // 000000 1xxxxx1 000000
        // We need to look inside the 1xxxxx1 for any contiguous
        // region of zeros.
        // We already know the trailing zeros are no larger than max. Remove them.
        x >>= (int)((nint)(sys.TrailingZeros64(x) & 63));
        if ((uint64)(x & (x + 1)) == 0) {
            // no more zeros (except at the top).
            continue;
        }
        // Strategy: shrink all runs of zeros by max. If any runs of zero
        // remain, then we've identified a larger maximum zero run.
        nuint Δp = most; // number of zeros we still need to shrink by.
        nuint k = (nuint)1; // current minimum length of runs of ones in x.
        while (ᐧ) {
            // Shrink all runs of zeros by p places (except the top zeros).
            while (Δp > 0) {
                if (Δp <= k) {
                    // Shift p ones down into the top of each run of zeros.
                    x |= (uint64)((x >> (int)(((nuint)(Δp & 63)))));
                    if ((uint64)(x & (x + 1)) == 0) {
                        // no more zeros (except at the top).
                        goto continue_outer;
                    }
                    break;
                }
                // Shift k ones down into the top of each run of zeros.
                x |= (uint64)((x >> (int)(((nuint)(k & 63)))));
                if ((uint64)(x & (x + 1)) == 0) {
                    // no more zeros (except at the top).
                    goto continue_outer;
                }
                Δp -= k;
                // We've just doubled the minimum length of 1-runs.
                // This allows us to shift farther in the next iteration.
                k *= 2;
            }
            // The length of the lowest-order zero run is an increment to our maximum.
            nuint j = (nuint)sys.TrailingZeros64(~x); // count contiguous trailing ones
            x >>= (int)((nuint)(j & 63)); // remove trailing ones
            j = (nuint)sys.TrailingZeros64(x); // count contiguous trailing zeros
            x >>= (int)((nuint)(j & 63)); // remove zeros
            most += j; // we have a new maximum!
            if ((uint64)(x & (x + 1)) == 0) {
                // no more zeros (except at the top).
                goto continue_outer;
            }
            Δp = j; // remove j more zeros from each zero run.
        }
continue_outer:;
    }
break_outer:;
    return packPallocSum(start, most, cur);
}

// find searches for npages contiguous free pages in pallocBits and returns
// the index where that run starts, as well as the index of the first free page
// it found in the search. searchIdx represents the first known free page and
// where to begin the next search from.
//
// If find fails to find any free space, it returns an index of ^uint(0) and
// the new searchIdx should be ignored.
//
// Note that if npages == 1, the two returned values will always be identical.
internal static (nuint, nuint) find(this ж<pallocBits> Ꮡb, uintptr npages, nuint searchIdx) {
    if (npages == 1){
        nuint addr = Ꮡb.find1(searchIdx);
        return (addr, addr);
    } else 
    if (npages <= 64) {
        return Ꮡb.findSmallN(npages, searchIdx);
    }
    return Ꮡb.findLargeN(npages, searchIdx);
}

// find1 is a helper for find which searches for a single free page
// in the pallocBits and returns the index.
//
// See find for an explanation of the searchIdx parameter.
internal static nuint find1(this ж<pallocBits> Ꮡb, nuint searchIdx) {
    ref var b = ref Ꮡb.DerefOrNull();

    _ = b.Value[0]; // lift nil check out of loop
    for (nuint i = searchIdx / 64; i < (nuint)8; i++) {
        var x = b.Value[i];
        if (~x == 0) {
            continue;
        }
        return i * 64 + (nuint)sys.TrailingZeros64(~x);
    }
    return ~(nuint)0;
}

// findSmallN is a helper for find which searches for npages contiguous free pages
// in this pallocBits and returns the index where that run of contiguous pages
// starts as well as the index of the first free page it finds in its search.
//
// See find for an explanation of the searchIdx parameter.
//
// Returns a ^uint(0) index on failure and the new searchIdx should be ignored.
//
// findSmallN assumes npages <= 64, where any such contiguous run of pages
// crosses at most one aligned 64-bit boundary in the bits.
internal static (nuint, nuint) findSmallN(this ж<pallocBits> Ꮡb, uintptr npages, nuint searchIdx) {
    ref var b = ref Ꮡb.DerefOrNull();

    nuint end = (nuint)0;
    nuint newSearchIdx = ~(nuint)0;
    for (nuint i = searchIdx / 64; i < (nuint)8; i++) {
        var bi = b.Value[i];
        if (~bi == 0) {
            end = 0;
            continue;
        }
        // First see if we can pack our allocation in the trailing
        // zeros plus the end of the last 64 bits.
        if (newSearchIdx == ~(nuint)0) {
            // The new searchIdx is going to be at these 64 bits after any
            // 1s we file, so count trailing 1s.
            newSearchIdx = i * 64 + (nuint)sys.TrailingZeros64(~bi);
        }
        nuint start = (nuint)sys.TrailingZeros64(bi);
        if (end + start >= (nuint)npages) {
            return (i * 64 - end, newSearchIdx);
        }
        // Next, check the interior of the 64-bit chunk.
        nuint j = findBitRange64(~bi, (nuint)npages);
        if (j < 64) {
            return (i * 64 + j, newSearchIdx);
        }
        end = (nuint)sys.LeadingZeros64(bi);
    }
    return (~(nuint)0, newSearchIdx);
}

// findLargeN is a helper for find which searches for npages contiguous free pages
// in this pallocBits and returns the index where that run starts, as well as the
// index of the first free page it found it its search.
//
// See alloc for an explanation of the searchIdx parameter.
//
// Returns a ^uint(0) index on failure and the new searchIdx should be ignored.
//
// findLargeN assumes npages > 64, where any such run of free pages
// crosses at least one aligned 64-bit boundary in the bits.
internal static (nuint, nuint) findLargeN(this ж<pallocBits> Ꮡb, uintptr npages, nuint searchIdx) {
    ref var b = ref Ꮡb.DerefOrNull();

    nuint start = ~(nuint)0;
    nuint size = (nuint)0;
    nuint newSearchIdx = ~(nuint)0;
    for (nuint i = searchIdx / 64; i < (nuint)8; i++) {
        var x = b.Value[i];
        if (x == ~(uint64)0) {
            size = 0;
            continue;
        }
        if (newSearchIdx == ~(nuint)0) {
            // The new searchIdx is going to be at these 64 bits after any
            // 1s we file, so count trailing 1s.
            newSearchIdx = i * 64 + (nuint)sys.TrailingZeros64(~x);
        }
        if (size == 0) {
            size = (nuint)sys.LeadingZeros64(x);
            start = i * 64 + 64 - size;
            continue;
        }
        nuint s = (nuint)sys.TrailingZeros64(x);
        if (s + size >= (nuint)npages) {
            return (start, newSearchIdx);
        }
        if (s < 64) {
            size = (nuint)sys.LeadingZeros64(x);
            start = i * 64 + 64 - size;
            continue;
        }
        size += 64;
    }
    if (size < (nuint)npages) {
        return (~(nuint)0, newSearchIdx);
    }
    return (start, newSearchIdx);
}

// allocRange allocates the range [i, i+n).
internal static void allocRange(this ж<pallocBits> Ꮡb, nuint i, nuint n) {
    (Ꮡb.Reinterpret<pallocBits, pageBits>()).setRange(i, n);
}

// allocAll allocates all the bits of b.
internal static void allocAll(this ж<pallocBits> Ꮡb) {
    (Ꮡb.Reinterpret<pallocBits, pageBits>()).setAll();
}

// free1 frees a single page in the pallocBits at i.
internal static void free1(this ж<pallocBits> Ꮡb, nuint i) {
    (Ꮡb.Reinterpret<pallocBits, pageBits>()).clear(i);
}

// free frees the range [i, i+n) of pages in the pallocBits.
internal static void free(this ж<pallocBits> Ꮡb, nuint i, nuint n) {
    (Ꮡb.Reinterpret<pallocBits, pageBits>()).clearRange(i, n);
}

// freeAll frees all the bits of b.
internal static void freeAll(this ж<pallocBits> Ꮡb) {
    (Ꮡb.Reinterpret<pallocBits, pageBits>()).clearAll();
}

// pages64 returns a 64-bit bitmap representing a block of 64 pages aligned
// to 64 pages. The returned block of pages is the one containing the i'th
// page in this pallocBits. Each bit represents whether the page is in-use.
internal static uint64 pages64(this ж<pallocBits> Ꮡb, nuint i) {
    return (Ꮡb.Reinterpret<pallocBits, pageBits>()).block64(i);
}

// allocPages64 allocates a 64-bit block of 64 pages aligned to 64 pages according
// to the bits set in alloc. The block set is the one containing the i'th page.
internal static void allocPages64(this ж<pallocBits> Ꮡb, nuint i, uint64 alloc) {
    (Ꮡb.Reinterpret<pallocBits, pageBits>()).setBlock64(i, alloc);
}

// findBitRange64 returns the bit index of the first set of
// n consecutive 1 bits. If no consecutive set of 1 bits of
// size n may be found in c, then it returns an integer >= 64.
// n must be > 0.
internal static nuint findBitRange64(uint64 c, nuint n) {
    // This implementation is based on shrinking the length of
    // runs of contiguous 1 bits. We remove the top n-1 1 bits
    // from each run of 1s, then look for the first remaining 1 bit.
    nuint Δp = n - 1; // number of 1s we want to remove.
    nuint k = (nuint)1; // current minimum width of runs of 0 in c.
    while (Δp > 0) {
        if (Δp <= k) {
            // Shift p 0s down into the top of each run of 1s.
            c &= (uint64)((c >> (int)(((nuint)(Δp & 63)))));
            break;
        }
        // Shift k 0s down into the top of each run of 1s.
        c &= (uint64)((c >> (int)(((nuint)(k & 63)))));
        if (c == 0) {
            return 64;
        }
        Δp -= k;
        // We've just doubled the minimum length of 0-runs.
        // This allows us to shift farther in the next iteration.
        k *= 2;
    }
    // Find first remaining 1.
    // Since we shrunk from the top down, the first 1 is in
    // its correct original position.
    return (nuint)sys.TrailingZeros64(c);
}

// pallocData encapsulates pallocBits and a bitmap for
// whether or not a given page is scavenged in a single
// structure. It's effectively a pallocBits with
// additional functionality.
//
// Update the comment on (*pageAlloc).chunks should this
// structure change.
[GoType] partial struct pallocData {
    internal partial ref pallocBits pallocBits { get; }
    internal pageBits scavenged;
}

// allocRange sets bits [i, i+n) in the bitmap to 1 and
// updates the scavenged bits appropriately.
internal static void allocRange(this ж<pallocData> Ꮡm, nuint i, nuint n) {
    ref var m = ref Ꮡm.DerefOrNull();

    // Clear the scavenged bits when we alloc the range.
    Ꮡm.of(pallocData.ᏑpallocBits).allocRange(i, n);
    m.scavenged.clearRange(i, n);
}

// allocAll sets every bit in the bitmap to 1 and updates
// the scavenged bits appropriately.
internal static void allocAll(this ж<pallocData> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    // Clear the scavenged bits when we alloc the range.
    Ꮡm.of(pallocData.ᏑpallocBits).allocAll();
    m.scavenged.clearAll();
}

} // end runtime_package
