// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using bits = global::go.math.bits_package;
using global::go.math;

partial class fuzz_package {

// ResetCoverage sets all of the counters for each edge of the instrumented
// source code to 0.
public static void ResetCoverage() {
    var cov = coverage();
    builtin.clear(cov);
}

// SnapshotCoverage copies the current counter values into coverageSnapshot,
// preserving them for later inspection. SnapshotCoverage also rounds each
// counter down to the nearest power of two. This lets the coordinator store
// multiple values for each counter by OR'ing them together.
public static void SnapshotCoverage() {
    var cov = coverage();
    foreach (var (i, b) in cov) {
        coverageSnapshot[i] = pow2Table[b];
    }
}

// diffCoverage returns a set of bits set in snapshot but not in base.
// If there are no new bits set, diffCoverage returns nil.
internal static slice<byte> diffCoverage(slice<byte> @base, slice<byte> snapshot) {
    if (len(@base) != len(snapshot)) {
        throw panic(fmt.Sprintf("the number of coverage bits changed: before=%d, after=%d"u8, len(@base), len(snapshot)));
    }
    var found = false;
    foreach (var i in range(snapshot)) {
        if ((byte)(snapshot[i] & ~@base[i]) != 0) {
            found = true;
            break;
        }
    }
    if (!found) {
        return default!;
    }
    var diff = new slice<byte>(len(snapshot));
    foreach (var i in range(diff)) {
        diff[i] = (byte)(snapshot[i] & ~@base[i]);
    }
    return diff;
}

// countNewCoverageBits returns the number of bits set in snapshot that are not
// set in base.
internal static nint countNewCoverageBits(slice<byte> @base, slice<byte> snapshot) {
    nint n = 0;
    foreach (var i in range(snapshot)) {
        n += bits.OnesCount8((uint8)((byte)(snapshot[i] & ~@base[i])));
    }
    return n;
}

// isCoverageSubset returns true if all the base coverage bits are set in
// snapshot.
internal static bool isCoverageSubset(slice<byte> @base, slice<byte> snapshot) {
    foreach (var (i, v) in @base) {
        if ((byte)(v & snapshot[i]) != v) {
            return false;
        }
    }
    return true;
}

// hasCoverageBit returns true if snapshot has at least one bit set that is
// also set in base.
internal static bool hasCoverageBit(slice<byte> @base, slice<byte> snapshot) {
    foreach (var i in range(snapshot)) {
        if ((byte)(snapshot[i] & @base[i]) != 0) {
            return true;
        }
    }
    return false;
}

internal static nint countBits(slice<byte> cov) {
    nint n = 0;
    foreach (var (_, c) in cov) {
        n += bits.OnesCount8(c);
    }
    return n;
}

internal static bool coverageEnabled;
internal static void initᴛcoverageEnabled() { coverageEnabled = len(coverage()) > 0; }
internal static slice<byte> coverageSnapshot;
internal static void initᴛcoverageSnapshot() { coverageSnapshot = new slice<byte>(len(coverage())); }

// _counters and _ecounters mark the start and end, respectively, of where
// the 8-bit coverage counters reside in memory. They're known to cmd/link,
// which specially assigns their addresses for this purpose.
internal static readonly ж<array<byte>> Ꮡ_counters = new StaticBox<array<byte>>(new array<byte>(0));
internal static ref array<byte> _counters => ref ((StaticBox<array<byte>>)Ꮡ_counters).ValueSlot;
internal static readonly ж<array<byte>> Ꮡ_ecounters = new StaticBox<array<byte>>(new array<byte>(0));
internal static ref array<byte> _ecounters => ref ((StaticBox<array<byte>>)Ꮡ_ecounters).ValueSlot;

// lookup table for faster power of two rounding
internal static array<byte> pow2Table = new(256);

[GoInit] internal static void init() {
    foreach (var i in range(pow2Table)) {
        var b = (byte)i;
        b |= (byte)((b >> 1));
        b |= (byte)((b >> 2));
        b |= (byte)((b >> 4));
        b -= (byte)((b >> 1));
        pow2Table[i] = b;
    }
}

} // end fuzz_package
