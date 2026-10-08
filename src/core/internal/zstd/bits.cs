// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bits = math.bits_package;
using math;

partial class zstd_package {

partial struct block /*[]byte*/;

// bitReader reads a bit stream going forward.
partial struct bitReader {
    internal ж<Reader> r; // for error reporting
    internal block data;   // the bits to read
    internal uint32 off;  // current offset into data
    internal uint32 bits;  // bits ready to be returned
    internal uint32 cnt;  // number of valid bits in the bits field
}

// makeBitReader makes a bit reader starting at off.
internal static bitReader makeBitReader(this ж<Reader> Ꮡr, block data, nint off) {
    return new bitReader(
        r: Ꮡr,
        data: data,
        off: (uint32)off
    );
}

// moreBits is called to read more bits.
// This ensures that at least 16 bits are available.
internal static error moreBits(this ref bitReader br) {
    while (br.cnt < 16) {
        if (br.off >= (uint32)builtin.len(br.data)) {
            return br.r.makeEOFError((nint)br.off);
        }
        var c = br.data[br.off];
        br.off++;
        br.bits |= (uint32)(((uint32)c).Lsh((uint64)(br.cnt)));
        br.cnt += 8;
    }
    return default!;
}

// val is called to fetch a value of b bits.
internal static uint32 val(this ref bitReader br, uint8 b) {
    var r = (uint32)(br.bits & ((((uint32)1).Lsh((uint64)(b))) - 1));
    br.bits.RshAssign((uint64)(b));
    br.cnt -= (uint32)b;
    return r;
}

// backup steps back to the last byte we used.
internal static void backup(this ref bitReader br) {
    while (br.cnt >= 8) {
        br.off--;
        br.cnt -= 8;
    }
}

// makeError returns an error at the current offset wrapping a string.
internal static error makeError(this ref bitReader br, @string msg) {
    return br.r.makeError((nint)br.off, msg);
}

// reverseBitReader reads a bit stream in reverse.
partial struct reverseBitReader {
    internal ж<Reader> r; // for error reporting
    internal block data;   // the bits to read
    internal uint32 off;  // current offset into data
    internal uint32 start;  // start in data; we read backward to start
    internal uint32 bits;  // bits ready to be returned
    internal uint32 cnt;  // number of valid bits in bits field
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zeroByteAtReverseBitˢ = "zero byte at reverse bit stream start"u8;

// makeReverseBitReader makes a reverseBitReader reading backward
// from off to start. The bitstream starts with a 1 bit in the last
// byte, at off.
internal static (reverseBitReader, error) makeReverseBitReader(this ж<Reader> Ꮡr, block data, nint off, nint start) {
    ref var r = ref Ꮡr.DerefOrNull();

    var streamStart = data[off];
    if (streamStart == 0) {
        return (new reverseBitReader(nil), r.makeError(off, zeroByteAtReverseBitˢ));
    }
    var rbr = new reverseBitReader(
        r: Ꮡr,
        data: data,
        off: (uint32)off,
        start: (uint32)start,
        bits: (uint32)streamStart,
        cnt: (uint32)(7 - bits.LeadingZeros8(streamStart))
    );
    return (rbr, default!);
}

// val is called to fetch a value of b bits.
internal static (uint32, error) val(this ref reverseBitReader rbr, uint8 b) {
    if (!rbr.fetch(b)) {
        return (0, rbr.r.makeEOFError((nint)rbr.off));
    }
    rbr.cnt -= (uint32)b;
    var v = (uint32)((rbr.bits.Rsh((uint64)(rbr.cnt))) & ((((uint32)1).Lsh((uint64)(b))) - 1));
    return (v, default!);
}

// fetch is called to ensure that at least b bits are available.
// It reports false if this can't be done,
// in which case only rbr.cnt bits are available.
internal static bool fetch(this ref reverseBitReader rbr, uint8 b) {
    while (rbr.cnt < (uint32)b) {
        if (rbr.off <= rbr.start) {
            return false;
        }
        rbr.off--;
        var c = rbr.data[rbr.off];
        rbr.bits <<= (int)(8);
        rbr.bits |= (uint32)((uint32)c);
        rbr.cnt += 8;
    }
    return true;
}

// makeError returns an error at the current offset wrapping a string.
internal static error makeError(this ref reverseBitReader rbr, @string msg) {
    return rbr.r.makeError((nint)rbr.off, msg);
}

} // end zstd_package
