// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Address range data structure.
//
// This file contains an implementation of a data structure which
// manages ordered address ranges.
namespace go;

using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// addrRange represents a region of address space.
//
// An addrRange must never span a gap in the address space.
[GoType] partial struct addrRange {
    // base and limit together represent the region of address space
    // [base, limit). That is, base is inclusive, limit is exclusive.
    // These are address over an offset view of the address space on
    // platforms with a segmented address space, that is, on platforms
    // where arenaBaseOffset != 0.
    internal offAddr @base, limit;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addrRangeBaseAndLimitAreˢ = "addr range base and limit are not in the same memory segment"u8;

// makeAddrRange creates a new address range from two virtual addresses.
//
// Throws if the base and limit are not in the same memory segment.
internal static addrRange makeAddrRange(uintptr @base, uintptr limit) {
    var r = new addrRange(new offAddr(@base), new offAddr(limit));
    if ((@base - (uintptr)arenaBaseOffset >= @base) != (limit - (uintptr)arenaBaseOffset >= limit)) {
        @throw(addrRangeBaseAndLimitAreˢ);
    }
    return r;
}

// size returns the size of the range represented in bytes.
internal static uintptr size(this addrRange a) {
    if (!a.@base.lessThan(a.limit)) {
        return 0;
    }
    // Subtraction is safe because limit and base must be in the same
    // segment of the address space.
    return a.limit.diff(a.@base);
}

// contains returns whether or not the range contains a given address.
internal static bool contains(this addrRange a, uintptr addr) {
    return a.@base.lessEqual(new offAddr(addr)) && (new offAddr(addr)).lessThan(a.limit);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badPruneˢ = "bad prune"u8;

// subtract takes the addrRange toPrune and cuts out any overlap with
// from, then returns the new range. subtract assumes that a and b
// either don't overlap at all, only overlap on one side, or are equal.
// If b is strictly contained in a, thus forcing a split, it will throw.
internal static addrRange subtract(this addrRange a, addrRange b) {
    if (b.@base.lessEqual(a.@base) && a.limit.lessEqual(b.limit)){
        return new addrRange(nil);
    } else 
    if (a.@base.lessThan(b.@base) && b.limit.lessThan(a.limit)){
        @throw(badPruneˢ);
    } else 
    if (b.limit.lessThan(a.limit) && a.@base.lessThan(b.limit)){
        a.@base = b.limit;
    } else 
    if (a.@base.lessThan(b.@base) && b.@base.lessThan(a.limit)) {
        a.limit = b.@base;
    }
    return a;
}

// takeFromFront takes len bytes from the front of the address range, aligning
// the base to align first. On success, returns the aligned start of the region
// taken and true.
[GoRecv] internal static (uintptr, bool) takeFromFront(this ref addrRange a, uintptr len, uint8 align) {
    var @base = alignUp(a.@base.addr(), (uintptr)align) + len;
    if (@base > a.limit.addr()) {
        return (0, false);
    }
    a.@base = new offAddr(@base);
    return (@base - len, true);
}

// takeFromBack takes len bytes from the end of the address range, aligning
// the limit to align after subtracting len. On success, returns the aligned
// start of the region taken and true.
[GoRecv] internal static (uintptr, bool) takeFromBack(this ref addrRange a, uintptr len, uint8 align) {
    var limit = alignDown(a.limit.addr() - len, (uintptr)align);
    if (a.@base.addr() > limit) {
        return (0, false);
    }
    a.limit = new offAddr(limit);
    return (limit, true);
}

// removeGreaterEqual removes all addresses in a greater than or equal
// to addr and returns the new range.
internal static addrRange removeGreaterEqual(this addrRange a, uintptr addr) {
    if ((new offAddr(addr)).lessEqual(a.@base)) {
        return new addrRange(nil);
    }
    if (a.limit.lessEqual(new offAddr(addr))) {
        return a;
    }
    return makeAddrRange(a.@base.addr(), addr);
}

internal static offAddr minOffAddr = new offAddr(arenaBaseOffset);
internal static offAddr maxOffAddr = new offAddr(unchecked((nuint)(140737488355327UL)));

// offAddr represents an address in a contiguous view
// of the address space on systems where the address space is
// segmented. On other systems, it's just a normal address.
[GoType] partial struct offAddr {
    // a is just the virtual address, but should never be used
    // directly. Call addr() to get this value instead.
    internal uintptr a;
}

// add adds a uintptr offset to the offAddr.
internal static offAddr add(this offAddr l, uintptr bytes) {
    return new offAddr(a: l.a + bytes);
}

// sub subtracts a uintptr offset from the offAddr.
internal static offAddr sub(this offAddr l, uintptr bytes) {
    return new offAddr(a: l.a - bytes);
}

// diff returns the amount of bytes in between the
// two offAddrs.
internal static uintptr diff(this offAddr l1, offAddr l2) {
    return l1.a - l2.a;
}

// lessThan returns true if l1 is less than l2 in the offset
// address space.
internal static bool lessThan(this offAddr l1, offAddr l2) {
    return (l1.a - (uintptr)arenaBaseOffset) < (l2.a - (uintptr)arenaBaseOffset);
}

// lessEqual returns true if l1 is less than or equal to l2 in
// the offset address space.
internal static bool lessEqual(this offAddr l1, offAddr l2) {
    return (l1.a - (uintptr)arenaBaseOffset) <= (l2.a - (uintptr)arenaBaseOffset);
}

// equal returns true if the two offAddr values are equal.
internal static bool equal(this offAddr l1, offAddr l2) {
    // No need to compare in the offset space, it
    // means the same thing.
    return l1 == l2;
}

// addr returns the virtual address for this offset address.
internal static uintptr addr(this offAddr l) {
    return l.a;
}

// atomicOffAddr is like offAddr, but operations on it are atomic.
// It also contains operations to be able to store marked addresses
// to ensure that they're not overridden until they've been seen.
[GoType] partial struct atomicOffAddr {
    // a contains the offset address, unlike offAddr.
    internal atomic.Int64 a;
}

// Clear attempts to store minOffAddr in atomicOffAddr. It may fail
// if a marked value is placed in the box in the meanwhile.
internal static void Clear(this ж<atomicOffAddr> Ꮡb) {
    while (ᐧ) {
        var old = Ꮡb.of(atomicOffAddr.Ꮡa).Load();
        if (old < 0) {
            return;
        }
        if (Ꮡb.of(atomicOffAddr.Ꮡa).CompareAndSwap(old, (int64)(minOffAddr.addr() - (uintptr)arenaBaseOffset))) {
            return;
        }
    }
}

// StoreMin stores addr if it's less than the current value in the
// offset address space if the current value is not marked.
internal static void StoreMin(this ж<atomicOffAddr> Ꮡb, uintptr addr) {
    var @new = (int64)(addr - (uintptr)arenaBaseOffset);
    while (ᐧ) {
        var old = Ꮡb.of(atomicOffAddr.Ꮡa).Load();
        if (old < @new) {
            return;
        }
        if (Ꮡb.of(atomicOffAddr.Ꮡa).CompareAndSwap(old, @new)) {
            return;
        }
    }
}

// StoreUnmark attempts to unmark the value in atomicOffAddr and
// replace it with newAddr. markedAddr must be a marked address
// returned by Load. This function will not store newAddr if the
// box no longer contains markedAddr.
internal static void StoreUnmark(this ж<atomicOffAddr> Ꮡb, uintptr markedAddr, uintptr newAddr) {
    Ꮡb.of(atomicOffAddr.Ꮡa).CompareAndSwap(-(int64)(markedAddr - (uintptr)arenaBaseOffset), (int64)(newAddr - (uintptr)arenaBaseOffset));
}

// StoreMarked stores addr but first converted to the offset address
// space and then negated.
internal static void StoreMarked(this ж<atomicOffAddr> Ꮡb, uintptr addr) {
    Ꮡb.of(atomicOffAddr.Ꮡa).Store(-(int64)(addr - (uintptr)arenaBaseOffset));
}

// Load returns the address in the box as a virtual address. It also
// returns if the value was marked or not.
internal static (uintptr, bool) Load(this ж<atomicOffAddr> Ꮡb) {
    var v = Ꮡb.of(atomicOffAddr.Ꮡa).Load();
    var wasMarked = false;
    if (v < 0) {
        wasMarked = true;
        v = -v;
    }
    return ((uintptr)v + (uintptr)arenaBaseOffset, wasMarked);
}

// addrRanges is a data structure holding a collection of ranges of
// address space.
//
// The ranges are coalesced eagerly to reduce the
// number ranges it holds.
//
// The slice backing store for this field is persistentalloc'd
// and thus there is no way to free it.
//
// addrRanges is not thread-safe.
[GoType] partial struct addrRanges {
    // ranges is a slice of ranges sorted by base.
    internal slice<addrRange> ranges;
    // totalBytes is the total amount of address space in bytes counted by
    // this addrRanges.
    internal uintptr totalBytes;
    // sysStat is the stat to track allocations by this type
    internal ж<sysMemStat> sysStat;
}

// go2cs generated this placeholder — func init is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// findSucc returns the first index in a such that addr is
// less than the base of the addrRange at that index.
[GoRecv] internal static nint findSucc(this ref addrRanges a, uintptr addr) {
    var @base = new offAddr(addr);
    // Narrow down the search space via a binary search
    // for large addrRanges until we have at most iterMax
    // candidates left.
    const nint iterMax = 8;
    nint bot = 0;
    nint top = len(a.ranges);
    while (top - bot > iterMax) {
        nint i = (nint)(((nuint)(bot + top) >> (int)(1)));
        if (a.ranges[i].contains(@base.addr())) {
            // a.ranges[i] contains base, so
            // its successor is the next index.
            return i + 1;
        }
        if (@base.lessThan(a.ranges[i].@base)){
            // In this case i might actually be
            // the successor, but we can't be sure
            // until we check the ones before it.
            top = i;
        } else {
            // In this case we know base is
            // greater than or equal to a.ranges[i].limit-1,
            // so i is definitely not the successor.
            // We already checked i, so pick the next
            // one.
            bot = i + 1;
        }
    }
    // There are top-bot candidates left, so
    // iterate over them and find the first that
    // base is strictly less than.
    for (nint i = bot; i < top; i++) {
        if (@base.lessThan(a.ranges[i].@base)) {
            return i;
        }
    }
    return top;
}

// findAddrGreaterEqual returns the smallest address represented by a
// that is >= addr. Thus, if the address is represented by a,
// then it returns addr. The second return value indicates whether
// such an address exists for addr in a. That is, if addr is larger than
// any address known to a, the second return value will be false.
[GoRecv] internal static (uintptr, bool) findAddrGreaterEqual(this ref addrRanges a, uintptr addr) {
    nint i = a.findSucc(addr);
    if (i == 0) {
        return (a.ranges[0].@base.addr(), true);
    }
    if (a.ranges[i - 1].contains(addr)) {
        return (addr, true);
    }
    if (i < len(a.ranges)) {
        return (a.ranges[i].@base.addr(), true);
    }
    return (0, false);
}

// contains returns true if a covers the address addr.
[GoRecv] internal static bool contains(this ref addrRanges a, uintptr addr) {
    nint i = a.findSucc(addr);
    if (i == 0) {
        return false;
    }
    return a.ranges[i - 1].contains(addr);
}

// go2cs generated this placeholder — func add is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// removeLast removes and returns the highest-addressed contiguous range
// of a, or the last nBytes of that range, whichever is smaller. If a is
// empty, it returns an empty range.
[GoRecv] internal static addrRange removeLast(this ref addrRanges a, uintptr nBytes) {
    if (len(a.ranges) == 0) {
        return new addrRange(nil);
    }
    var r = a.ranges[len(a.ranges) - 1];
    var size = r.size();
    if (size > nBytes) {
        var newEnd = r.limit.sub(nBytes);
        a.ranges[len(a.ranges) - 1].limit = newEnd;
        a.totalBytes -= nBytes;
        return new addrRange(newEnd, r.limit);
    }
    a.ranges = a.ranges[..(int)(len(a.ranges) - 1)];
    a.totalBytes -= size;
    return r;
}

// removeGreaterEqual removes the ranges of a which are above addr, and additionally
// splits any range containing addr.
[GoRecv] internal static void removeGreaterEqual(this ref addrRanges a, uintptr addr) {
    nint pivot = a.findSucc(addr);
    if (pivot == 0) {
        // addr is before all ranges in a.
        a.totalBytes = 0;
        a.ranges = a.ranges[..0];
        return;
    }
    var removed = (uintptr)0;
    foreach (var (_, r) in a.ranges[(int)(pivot)..]) {
        removed += r.size();
    }
    {
        var r = a.ranges[pivot - 1]; if (r.contains(addr)) {
            removed += r.size();
            r = r.removeGreaterEqual(addr);
            if (r.size() == 0){
                pivot--;
            } else {
                removed -= r.size();
                a.ranges[pivot - 1] = r;
            }
        }
    }
    a.ranges = a.ranges[..(int)(pivot)];
    a.totalBytes -= removed;
}

// go2cs generated this placeholder — func cloneInto is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

} // end runtime_package
