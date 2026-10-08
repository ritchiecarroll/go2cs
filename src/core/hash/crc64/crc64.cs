// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package crc64 implements the 64-bit cyclic redundancy check, or CRC-64,
// checksum. See https://en.wikipedia.org/wiki/Cyclic_redundancy_check for
// information.
namespace go.hash;

using errors = errors_package;
using hash = hash_package;
using byteorder = @internal.byteorder_package;
using sync = sync_package;
using @internal;

partial class crc64_package {

// The size of a CRC-64 checksum in bytes.
public static UntypedInt ΔSize => 8;

// Predefined polynomials.
public static UntypedInt ISO => 0xD800000000000000;

public static UntypedInt ECMA => 0xC96C5795D7870F42;

partial struct Table /*[256]uint64*/;

internal static ж<ж<array<Table>>> Ꮡslicing8TableISO = new StandardBox<ж<array<Table>>>(default(ж<array<Table>>));
internal static ref ж<array<Table>> slicing8TableISO => ref Ꮡslicing8TableISO.ValueSlot;
internal static ж<ж<array<Table>>> Ꮡslicing8TableECMA = new StandardBox<ж<array<Table>>>(default(ж<array<Table>>));
internal static ref ж<array<Table>> slicing8TableECMA => ref Ꮡslicing8TableECMA.ValueSlot;

internal static Action buildSlicing8TablesOnce = sync.OnceFunc(buildSlicing8Tables);

internal static void buildSlicing8Tables() {
    slicing8TableISO = makeSlicingBy8Table(ref (makeTable(ISO)).DerefOrNull());
    slicing8TableECMA = makeSlicingBy8Table(ref (makeTable(ECMA)).DerefOrNull());
}

// MakeTable returns a [Table] constructed from the specified polynomial.
// The contents of this [Table] must not be modified.
public static ж<Table> MakeTable(uint64 poly) {
    buildSlicing8TablesOnce();
    var exprᴛ1 = poly;
    if (exprᴛ1 == ISO) {
        return slicing8TableISO.at<Table>(0);
    }
    if (exprᴛ1 == ECMA) {
        return slicing8TableECMA.at<Table>(0);
    }
    { /* default: */
        return makeTable(poly);
    }

}

internal static ж<Table> makeTable(uint64 poly) {
    var t = @new<Table>();
    for (nint i = 0; i < 256; i++) {
        var crc = (uint64)i;
        for (nint j = 0; j < 8; j++) {
            if ((uint64)(crc & 1) == 1){
                crc = (uint64)(((crc >> (int)(1))) ^ poly);
            } else {
                crc >>= (int)(1);
            }
        }
        t.Value[i] = crc;
    }
    return t;
}

internal static ж<array<Table>> makeSlicingBy8Table(ref Table t) {
    ref var helperTable = ref heap(new array<Table>(8), out var ᏑhelperTable);
    helperTable[0] = t.Clone();
    for (nint i = 0; i < 256; i++) {
        var crc = t[i];
        for (nint j = 1; j < 8; j++) {
            crc = (uint64)(t[(uint64)(crc & 0xff)] ^ ((crc >> (int)(8))));
            helperTable[j][i] = crc;
        }
    }
    return ᏑhelperTable;
}

// digest represents the partial evaluation of a checksum.
partial struct digest {
    internal uint64 crc;
    internal ж<Table> tab;
}

// New creates a new hash.Hash64 computing the CRC-64 checksum using the
// polynomial represented by the [Table]. Its Sum method will lay the
// value out in big-endian byte order. The returned Hash64 also
// implements [encoding.BinaryMarshaler] and [encoding.BinaryUnmarshaler] to
// marshal and unmarshal the internal state of the hash.
public static hash.Hash64 New(ж<Table> Ꮡtab) {
    return new digestжHash64(Ꮡ(new digest(0, Ꮡtab)));
}

internal static nint Size(this ref digest d) {
    return ΔSize;
}

internal static nint BlockSize(this ref digest d) {
    return 1;
}

internal static void Reset(this ref digest d) {
    d.crc = 0;
}

internal static readonly @string magic = "crc\x02"u8;
internal const nint marshaledSize = /* len(magic) + 8 + 8 */ 20;

internal static (slice<byte>, error) AppendBinary(this ref digest d, slice<byte> b) {
    b = append(b, magic.ꓸꓸꓸ);
    b = byteorder.BEAppendUint64(b, tableSum(d.tab));
    b = byteorder.BEAppendUint64(b, d.crc);
    return (b, default!);
}

internal static (slice<byte>, error) MarshalBinary(this ref digest d) {
    return d.AppendBinary(new slice<byte>(0, marshaledSize));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hashCrc64InvalidHashˢ = "hash/crc64: invalid hash state identifier"u8;
internal static readonly @string hashCrc64InvalidHashˢ2 = "hash/crc64: invalid hash state size"u8;
internal static readonly @string hashCrc64TablesDoNotˢ = "hash/crc64: tables do not match"u8;

internal static error UnmarshalBinary(this ref digest d, slice<byte> b) {
    if (len(b) < len(magic) || ((sstring)(b[..(int)(len(magic))])) != magic) {
        return errors.New(hashCrc64InvalidHashˢ);
    }
    if (len(b) != marshaledSize) {
        return errors.New(hashCrc64InvalidHashˢ2);
    }
    if (tableSum(d.tab) != byteorder.BEUint64(b[4..])) {
        return errors.New(hashCrc64TablesDoNotˢ);
    }
    d.crc = byteorder.BEUint64(b[12..]);
    return default!;
}

internal static uint64 update(uint64 crc, ref Table tab, slice<byte> p) {
    buildSlicing8TablesOnce();
    crc = ~crc;
    // Table comparison is somewhat expensive, so avoid it for small sizes
    while (len(p) >= 64) {
        ж<array<Table>> helperTable = default!;
        if (tab == slicing8TableECMA.Value[0]){
            helperTable = slicing8TableECMA;
        } else 
        if (tab == slicing8TableISO.Value[0]){
            helperTable = slicing8TableISO;
        } else 
        if (len(p) >= 2048){
            // For smaller sizes creating extended table takes too much time
            // According to the tests between various x86 and arm CPUs, 2k is a reasonable
            // threshold for now. This may change in the future.
            helperTable = makeSlicingBy8Table(ref tab);
        } else {
            break;
        }
        // Update using slicing-by-8
        while (len(p) > 8) {
            crc ^= (uint64)(byteorder.LEUint64(p));
            crc = (uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)(helperTable.Value[7][(uint64)(crc & 0xff)] ^ helperTable.Value[6][(uint64)(((crc >> (int)(8))) & 0xff)]) ^ helperTable.Value[5][(uint64)(((crc >> (int)(16))) & 0xff)]) ^ helperTable.Value[4][(uint64)(((crc >> (int)(24))) & 0xff)]) ^ helperTable.Value[3][(uint64)(((crc >> (int)(32))) & 0xff)]) ^ helperTable.Value[2][(uint64)(((crc >> (int)(40))) & 0xff)]) ^ helperTable.Value[1][(uint64)(((crc >> (int)(48))) & 0xff)]) ^ helperTable.Value[0][(crc >> (int)(56))]);
            p = p[8..];
        }
    }
    // For reminders or small sizes
    foreach (var (_, v) in p) {
        crc = (uint64)(tab[(byte)((byte)crc ^ v)] ^ ((crc >> (int)(8))));
    }
    return ~crc;
}

// Update returns the result of adding the bytes in p to the crc.
public static uint64 Update(uint64 crc, ж<Table> Ꮡtab, slice<byte> p) {
    return update(crc, ref (Ꮡtab).DerefOrNull(), p);
}

internal static (nint n, error err) Write(this ref digest d, slice<byte> p) {
    d.crc = update(d.crc, ref (d.tab).DerefOrNull(), p);
    return (len(p), default!);
}

internal static uint64 Sum64(this ref digest d) {
    return d.crc;
}

internal static slice<byte> Sum(this ref digest d, slice<byte> @in) {
    var s = d.Sum64();
    return append(@in, (byte)((s >> (int)(56))), (byte)((s >> (int)(48))), (byte)((s >> (int)(40))), (byte)((s >> (int)(32))), (byte)((s >> (int)(24))), (byte)((s >> (int)(16))), (byte)((s >> (int)(8))), (byte)s);
}

// Checksum returns the CRC-64 checksum of data
// using the polynomial represented by the [Table].
public static uint64 Checksum(slice<byte> data, ж<Table> Ꮡtab) {
    return update(0, ref (Ꮡtab).DerefOrNull(), data);
}

// tableSum returns the ISO checksum of table t.
internal static uint64 tableSum(ж<Table> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    array<byte> a = new(2048);
    var b = a[..0];
    if (Ꮡt != nil) {
        foreach (var (_, x) in t) {
            b = byteorder.BEAppendUint64(b, x);
        }
    }
    return Checksum(b, MakeTable(ISO));
}

} // end crc64_package
