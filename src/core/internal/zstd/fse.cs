// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bits = math.bits_package;
using math;

partial class zstd_package {

// fseEntry is one entry in an FSE table.
[GoType] partial struct fseEntry {
    internal uint8 sym;  // value that this entry records
    internal uint8 bits;  // number of bits to read to determine next state
    internal uint16 @base; // add those bits to this state to get the next state
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fseAccuracyLogTooLargeˢ = "FSE accuracy log too large"u8;
internal static readonly @string fseSymbolIndexOverflowˢ = "FSE symbol index overflow"u8;
internal static readonly @string fseSymOverflowˢ = "FSE sym overflow"u8;
internal static readonly @string tooManySymbolsInFseTableˢ = "too many symbols in FSE table"u8;

// readFSE reads an FSE table from data starting at off.
// maxSym is the maximum symbol value.
// maxBits is the maximum number of bits permitted for symbols in the table.
// The FSE is written into table, which must be at least 1<<maxBits in size.
// This returns the number of bits in the FSE table and the new offset.
// RFC 4.1.1.
internal static (nint tableBits, nint roff, error err) readFSE(this ж<Reader> Ꮡr, block data, nint off, nint maxSym, nint maxBits, slice<fseEntry> table) {
    ref var r = ref Ꮡr.DerefOrNull();

    var br = Ꮡr.makeBitReader(data, off);
    {
        var errΔ1 = br.moreBits(); if (errΔ1 != default!) {
            return (0, 0, errΔ1);
        }
    }
    nint accuracyLog = (nint)br.val(4) + 5;
    if (accuracyLog > maxBits) {
        return (0, 0, br.makeError(fseAccuracyLogTooLargeˢ));
    }
    // The number of remaining probabilities, plus 1.
    // This determines the number of bits to be read for the next value.
    nint remaining = (((nint)1).Lsh((int64)(accuracyLog))) + 1;
    // The current difference between small and large values,
    // which depends on the number of remaining values.
    // Small values use 1 less bit.
    nint threshold = ((nint)1).Lsh((int64)(accuracyLog));
    // The number of bits needed to compute threshold.
    nint bitsNeeded = accuracyLog + 1;
    // The next character value.
    nint sym = 0;
    // Whether the last count was 0.
    var prev0 = false;
    array<int16> norm = new(256);
    while (remaining > 1 && sym <= maxSym) {
        {
            var errΔ2 = br.moreBits(); if (errΔ2 != default!) {
                return (0, 0, errΔ2);
            }
        }
        if (prev0) {
            // Previous count was 0, so there is a 2-bit
            // repeat flag. If the 2-bit flag is 0b11,
            // it adds 3 and then there is another repeat flag.
            nint zsym = sym;
            while (((uint32)(br.bits & 0xfff)) == 0xfff) {
                zsym += 3 * 6;
                br.bits >>= (int)(12);
                br.cnt -= 12;
                {
                    var errΔ3 = br.moreBits(); if (errΔ3 != default!) {
                        return (0, 0, errΔ3);
                    }
                }
            }
            while (((uint32)(br.bits & 3)) == 3) {
                zsym += 3;
                br.bits >>= (int)(2);
                br.cnt -= 2;
                {
                    var errΔ4 = br.moreBits(); if (errΔ4 != default!) {
                        return (0, 0, errΔ4);
                    }
                }
            }
            // We have at least 14 bits here,
            // no need to call moreBits
            zsym += (nint)br.val(2);
            if (zsym > maxSym) {
                return (0, 0, br.makeError(fseSymbolIndexOverflowˢ));
            }
            for (; sym < zsym; sym++) {
                norm[(uint8)sym] = 0;
            }
            prev0 = false;
            continue;
        }
        nint max = (2 * threshold - 1) - remaining;
        nint count = default!;
        if ((nint)((uint32)(br.bits & (uint32)(threshold - 1))) < max){
            // A small value.
            count = (nint)((uint32)(br.bits & (uint32)(threshold - 1)));
            br.bits.RshAssign((int64)(bitsNeeded - 1));
            br.cnt -= (uint32)(bitsNeeded - 1);
        } else {
            // A large value.
            count = (nint)((uint32)(br.bits & (uint32)(2 * threshold - 1)));
            if (count >= threshold) {
                count -= max;
            }
            br.bits.RshAssign((int64)(bitsNeeded));
            br.cnt -= (uint32)bitsNeeded;
        }
        count--;
        if (count >= 0){
            remaining -= count;
        } else {
            remaining--;
        }
        if (sym >= 256) {
            return (0, 0, br.makeError(fseSymOverflowˢ));
        }
        norm[(uint8)sym] = (int16)count;
        sym++;
        prev0 = count == 0;
        while (remaining < threshold) {
            bitsNeeded--;
            threshold >>= (int)(1);
        }
    }
    if (remaining != 1) {
        return (0, 0, br.makeError(tooManySymbolsInFseTableˢ));
    }
    for (; sym <= maxSym; sym++) {
        norm[(uint8)sym] = 0;
    }
    br.backup();
    {
        var errΔ5 = r.buildFSE(off, norm.slice(0, maxSym + 1), table, accuracyLog); if (errΔ5 != default!) {
            return (0, 0, errΔ5);
        }
    }
    return (accuracyLog, (nint)br.off, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fseCountErrorˢ = "FSE count error"u8;
internal static readonly @string fseStateErrorˢ = "FSE state error"u8;

// buildFSE builds an FSE decoding table from a list of probabilities.
// The probabilities are in norm. next is scratch space. The number of bits
// in the table is tableBits.
[GoRecv] internal static error buildFSE(this ref Reader r, nint off, slice<int16> norm, slice<fseEntry> table, nint tableBits) {
    nint tableSize = ((nint)1).Lsh((int64)(tableBits));
    nint highThreshold = tableSize - 1;
    array<uint16> next = new(256);
    foreach (var (i, n) in norm) {
        if (n >= 0){
            next[(uint8)i] = (uint16)n;
        } else {
            table[highThreshold].sym = (uint8)i;
            highThreshold--;
            next[(uint8)i] = 1;
        }
    }
    nint pos = 0;
    nint step = ((tableSize >> (int)(1))) + ((tableSize >> (int)(3))) + 3;
    nint mask = tableSize - 1;
    foreach (var (i, n) in norm) {
        for (nint j = 0; j < (nint)n; j++) {
            table[pos].sym = (uint8)i;
            pos = (nint)((pos + step) & mask);
            while (pos > highThreshold) {
                pos = (nint)((pos + step) & mask);
            }
        }
    }
    if (pos != 0) {
        return r.makeError(off, fseCountErrorˢ);
    }
    for (nint i = 0; i < tableSize; i++) {
        var sym = table[i].sym;
        var nextState = next[sym];
        next[sym]++;
        if (nextState == 0) {
            return r.makeError(off, fseStateErrorˢ);
        }
        nint highBit = 15 - bits.LeadingZeros16(nextState);
        nint bitsΔ1 = tableBits - highBit;
        table[i].bits = (uint8)bitsΔ1;
        table[i].@base = (uint16)((nextState.Lsh((int64)(bitsΔ1))) - (uint16)tableSize);
    }
    return default!;
}

// fseBaselineEntry is an entry in an FSE baseline table.
// We use these for literal/match/length values.
// Those require mapping the symbol to a baseline value,
// and then reading zero or more bits and adding the value to the baseline.
// Rather than looking these up in separate tables,
// we convert the FSE table to an FSE baseline table.
[GoType] partial struct fseBaselineEntry {
    internal uint32 baseline; // baseline for value that this entry represents
    internal uint8 basebits;  // number of bits to read to add to baseline
    internal uint8 bits;  // number of bits to read to determine next state
    internal uint16 @base; // add the bits to this base to get the next state
}

// Given a literal length code, we need to read a number of bits and
// add that to a baseline. For states 0 to 15 the baseline is the
// state and the number of bits is zero. RFC 3.1.1.3.2.1.1.
internal static UntypedInt literalLengthOffset => 16;

internal static slice<uint32> literalLengthBase = new uint32[]{
    (uint32)(16 | ((1 << (int)(24)))),
    (uint32)(18 | ((1 << (int)(24)))),
    (uint32)(20 | ((1 << (int)(24)))),
    (uint32)(22 | ((1 << (int)(24)))),
    (uint32)(24 | ((2 << (int)(24)))),
    (uint32)(28 | ((2 << (int)(24)))),
    (uint32)(32 | ((3 << (int)(24)))),
    (uint32)(40 | ((3 << (int)(24)))),
    (uint32)(48 | ((4 << (int)(24)))),
    (uint32)(64 | ((6 << (int)(24)))),
    (uint32)(128 | ((7 << (int)(24)))),
    (uint32)(256 | ((8 << (int)(24)))),
    (uint32)(512 | ((9 << (int)(24)))),
    (uint32)(1024 | ((10 << (int)(24)))),
    (uint32)(2048 | ((11 << (int)(24)))),
    (uint32)(4096 | ((12 << (int)(24)))),
    (uint32)(8192 | ((13 << (int)(24)))),
    (uint32)(16384 | ((14 << (int)(24)))),
    (uint32)(32768 | ((15 << (int)(24)))),
    (uint32)(65536 | ((16 << (int)(24))))
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fseBaselineSymbolˢ = "FSE baseline symbol overflow"u8;

// makeLiteralBaselineFSE converts the literal length fseTable to baselineTable.
[GoRecv] internal static error makeLiteralBaselineFSE(this ref Reader r, nint off, slice<fseEntry> fseTable, slice<fseBaselineEntry> baselineTable) {
    foreach (var (i, e) in fseTable) {
        var be = new fseBaselineEntry(
            bits: e.bits,
            @base: e.@base
        );
        if (e.sym < literalLengthOffset){
            be.baseline = (uint32)e.sym;
            be.basebits = 0;
        } else {
            if (e.sym > 35) {
                return r.makeError(off, fseBaselineSymbolˢ);
            }
            var idx = (uint8)(e.sym - (uint8)literalLengthOffset);
            var basebits = literalLengthBase[idx];
            be.baseline = (uint32)(basebits & 0xffffff);
            be.basebits = (uint8)((basebits >> (int)(24)));
        }
        baselineTable[i] = be;
    }
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fseOffsetSymbolOverflowˢ = "FSE offset symbol overflow"u8;

// makeOffsetBaselineFSE converts the offset length fseTable to baselineTable.
[GoRecv] internal static error makeOffsetBaselineFSE(this ref Reader r, nint off, slice<fseEntry> fseTable, slice<fseBaselineEntry> baselineTable) {
    foreach (var (i, e) in fseTable) {
        var be = new fseBaselineEntry(
            bits: e.bits,
            @base: e.@base
        );
        if (e.sym > 31) {
            return r.makeError(off, fseOffsetSymbolOverflowˢ);
        }
        // The simple way to write this is
        //     be.baseline = 1 << e.sym
        //     be.basebits = e.sym
        // That would give us an offset value that corresponds to
        // the one described in the RFC. However, for offsets > 3
        // we have to subtract 3. And for offset values 1, 2, 3
        // we use a repeated offset.
        //
        // The baseline is always a power of 2, and is never 0,
        // so for those low values we will see one entry that is
        // baseline 1, basebits 0, and one entry that is baseline 2,
        // basebits 1. All other entries will have baseline >= 4
        // basebits >= 2.
        //
        // So we can check for RFC offset <= 3 by checking for
        // basebits <= 1. That means that we can subtract 3 here
        // and not worry about doing it in the hot loop.
        be.baseline = ((uint32)1).Lsh((uint64)(e.sym));
        if (e.sym >= 2) {
            be.baseline -= 3;
        }
        be.basebits = e.sym;
        baselineTable[i] = be;
    }
    return default!;
}

// Given a match length code, we need to read a number of bits and add
// that to a baseline. For states 0 to 31 the baseline is state+3 and
// the number of bits is zero. RFC 3.1.1.3.2.1.1.
internal static UntypedInt matchLengthOffset => 32;

internal static slice<uint32> matchLengthBase = new uint32[]{
    (uint32)(35 | ((1 << (int)(24)))),
    (uint32)(37 | ((1 << (int)(24)))),
    (uint32)(39 | ((1 << (int)(24)))),
    (uint32)(41 | ((1 << (int)(24)))),
    (uint32)(43 | ((2 << (int)(24)))),
    (uint32)(47 | ((2 << (int)(24)))),
    (uint32)(51 | ((3 << (int)(24)))),
    (uint32)(59 | ((3 << (int)(24)))),
    (uint32)(67 | ((4 << (int)(24)))),
    (uint32)(83 | ((4 << (int)(24)))),
    (uint32)(99 | ((5 << (int)(24)))),
    (uint32)(131 | ((7 << (int)(24)))),
    (uint32)(259 | ((8 << (int)(24)))),
    (uint32)(515 | ((9 << (int)(24)))),
    (uint32)(1027 | ((10 << (int)(24)))),
    (uint32)(2051 | ((11 << (int)(24)))),
    (uint32)(4099 | ((12 << (int)(24)))),
    (uint32)(8195 | ((13 << (int)(24)))),
    (uint32)(16387 | ((14 << (int)(24)))),
    (uint32)(32771 | ((15 << (int)(24)))),
    (uint32)(65539 | ((16 << (int)(24))))
}.slice();

// makeMatchBaselineFSE converts the match length fseTable to baselineTable.
[GoRecv] internal static error makeMatchBaselineFSE(this ref Reader r, nint off, slice<fseEntry> fseTable, slice<fseBaselineEntry> baselineTable) {
    foreach (var (i, e) in fseTable) {
        var be = new fseBaselineEntry(
            bits: e.bits,
            @base: e.@base
        );
        if (e.sym < matchLengthOffset){
            be.baseline = (uint32)e.sym + 3;
            be.basebits = 0;
        } else {
            if (e.sym > 52) {
                return r.makeError(off, fseBaselineSymbolˢ);
            }
            var idx = (uint8)(e.sym - (uint8)matchLengthOffset);
            var basebits = matchLengthBase[idx];
            be.baseline = (uint32)(basebits & 0xffffff);
            be.basebits = (uint8)((basebits >> (int)(24)));
        }
        baselineTable[i] = be;
    }
    return default!;
}

// predefinedLiteralTable is the predefined table to use for literal lengths.
// Generated from table in RFC 3.1.1.3.2.2.1.
// Checked by TestPredefinedTables.
internal static array<fseBaselineEntry> predefinedLiteralTable = new fseBaselineEntry[]{
    new(0, 0, 4, 0), new(0, 0, 4, 16), new(1, 0, 5, 32),
    new(3, 0, 5, 0), new(4, 0, 5, 0), new(6, 0, 5, 0),
    new(7, 0, 5, 0), new(9, 0, 5, 0), new(10, 0, 5, 0),
    new(12, 0, 5, 0), new(14, 0, 6, 0), new(16, 1, 5, 0),
    new(20, 1, 5, 0), new(22, 1, 5, 0), new(28, 2, 5, 0),
    new(32, 3, 5, 0), new(48, 4, 5, 0), new(64, 6, 5, 32),
    new(128, 7, 5, 0), new(256, 8, 6, 0), new(1024, 10, 6, 0),
    new(4096, 12, 6, 0), new(0, 0, 4, 32), new(1, 0, 4, 0),
    new(2, 0, 5, 0), new(4, 0, 5, 32), new(5, 0, 5, 0),
    new(7, 0, 5, 32), new(8, 0, 5, 0), new(10, 0, 5, 32),
    new(11, 0, 5, 0), new(13, 0, 6, 0), new(16, 1, 5, 32),
    new(18, 1, 5, 0), new(22, 1, 5, 32), new(24, 2, 5, 0),
    new(32, 3, 5, 32), new(40, 3, 5, 0), new(64, 6, 4, 0),
    new(64, 6, 4, 16), new(128, 7, 5, 32), new(512, 9, 6, 0),
    new(2048, 11, 6, 0), new(0, 0, 4, 48), new(1, 0, 4, 16),
    new(2, 0, 5, 32), new(3, 0, 5, 32), new(5, 0, 5, 32),
    new(6, 0, 5, 32), new(8, 0, 5, 32), new(9, 0, 5, 32),
    new(11, 0, 5, 32), new(12, 0, 5, 32), new(15, 0, 6, 0),
    new(18, 1, 5, 32), new(20, 1, 5, 32), new(24, 2, 5, 32),
    new(28, 2, 5, 32), new(40, 3, 5, 32), new(48, 4, 5, 32),
    new(65536, 16, 6, 0), new(32768, 15, 6, 0), new(16384, 14, 6, 0),
    new(8192, 13, 6, 0)
}.array();

// predefinedOffsetTable is the predefined table to use for offsets.
// Generated from table in RFC 3.1.1.3.2.2.3.
// Checked by TestPredefinedTables.
internal static array<fseBaselineEntry> predefinedOffsetTable = new fseBaselineEntry[]{
    new(1, 0, 5, 0), new(61, 6, 4, 0), new(509, 9, 5, 0),
    new(32765, 15, 5, 0), new(2097149, 21, 5, 0), new(5, 3, 5, 0),
    new(125, 7, 4, 0), new(4093, 12, 5, 0), new(262141, 18, 5, 0),
    new(8388605, 23, 5, 0), new(29, 5, 5, 0), new(253, 8, 4, 0),
    new(16381, 14, 5, 0), new(1048573, 20, 5, 0), new(1, 2, 5, 0),
    new(125, 7, 4, 16), new(2045, 11, 5, 0), new(131069, 17, 5, 0),
    new(4194301, 22, 5, 0), new(13, 4, 5, 0), new(253, 8, 4, 16),
    new(8189, 13, 5, 0), new(524285, 19, 5, 0), new(2, 1, 5, 0),
    new(61, 6, 4, 16), new(1021, 10, 5, 0), new(65533, 16, 5, 0),
    new(268435453, 28, 5, 0), new(134217725, 27, 5, 0), new(67108861, 26, 5, 0),
    new(33554429, 25, 5, 0), new(16777213, 24, 5, 0)
}.array();

// predefinedMatchTable is the predefined table to use for match lengths.
// Generated from table in RFC 3.1.1.3.2.2.2.
// Checked by TestPredefinedTables.
internal static array<fseBaselineEntry> predefinedMatchTable = new fseBaselineEntry[]{
    new(3, 0, 6, 0), new(4, 0, 4, 0), new(5, 0, 5, 32),
    new(6, 0, 5, 0), new(8, 0, 5, 0), new(9, 0, 5, 0),
    new(11, 0, 5, 0), new(13, 0, 6, 0), new(16, 0, 6, 0),
    new(19, 0, 6, 0), new(22, 0, 6, 0), new(25, 0, 6, 0),
    new(28, 0, 6, 0), new(31, 0, 6, 0), new(34, 0, 6, 0),
    new(37, 1, 6, 0), new(41, 1, 6, 0), new(47, 2, 6, 0),
    new(59, 3, 6, 0), new(83, 4, 6, 0), new(131, 7, 6, 0),
    new(515, 9, 6, 0), new(4, 0, 4, 16), new(5, 0, 4, 0),
    new(6, 0, 5, 32), new(7, 0, 5, 0), new(9, 0, 5, 32),
    new(10, 0, 5, 0), new(12, 0, 6, 0), new(15, 0, 6, 0),
    new(18, 0, 6, 0), new(21, 0, 6, 0), new(24, 0, 6, 0),
    new(27, 0, 6, 0), new(30, 0, 6, 0), new(33, 0, 6, 0),
    new(35, 1, 6, 0), new(39, 1, 6, 0), new(43, 2, 6, 0),
    new(51, 3, 6, 0), new(67, 4, 6, 0), new(99, 5, 6, 0),
    new(259, 8, 6, 0), new(4, 0, 4, 32), new(4, 0, 4, 48),
    new(5, 0, 4, 16), new(7, 0, 5, 32), new(8, 0, 5, 32),
    new(10, 0, 5, 32), new(11, 0, 5, 32), new(14, 0, 6, 0),
    new(17, 0, 6, 0), new(20, 0, 6, 0), new(23, 0, 6, 0),
    new(26, 0, 6, 0), new(29, 0, 6, 0), new(32, 0, 6, 0),
    new(65539, 16, 6, 0), new(32771, 15, 6, 0), new(16387, 14, 6, 0),
    new(8195, 13, 6, 0), new(4099, 12, 6, 0), new(2051, 11, 6, 0),
    new(1027, 10, 6, 0)
}.array();

} // end zstd_package
