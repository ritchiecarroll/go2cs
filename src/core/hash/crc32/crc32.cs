// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package crc32 implements the 32-bit cyclic redundancy check, or CRC-32,
// checksum. See https://en.wikipedia.org/wiki/Cyclic_redundancy_check for
// information.
//
// Polynomials are represented in LSB-first form also known as reversed representation.
//
// See https://en.wikipedia.org/wiki/Mathematics_of_cyclic_redundancy_checks#Reversed_representations_and_reciprocal_polynomials
// for information.
namespace go.hash;

using errors = errors_package;
using hash = hash_package;
using byteorder = @internal.byteorder_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using @internal;
using go.sync;

partial class crc32_package {

// The size of a CRC-32 checksum in bytes.
public static UntypedInt ΔSize => 4;

// Predefined polynomials.
public static UntypedInt IEEE => 0xedb88320;

public static UntypedInt Castagnoli => 0x82f63b78;

public static UntypedInt Koopman => 0xeb31d82e;

partial struct Table /*[256]uint32*/;

// This file makes use of functions implemented in architecture-specific files.
// The interface that they implement is as follows:
//
//    // archAvailableIEEE reports whether an architecture-specific CRC32-IEEE
//    // algorithm is available.
//    archAvailableIEEE() bool
//
//    // archInitIEEE initializes the architecture-specific CRC3-IEEE algorithm.
//    // It can only be called if archAvailableIEEE() returns true.
//    archInitIEEE()
//
//    // archUpdateIEEE updates the given CRC32-IEEE. It can only be called if
//    // archInitIEEE() was previously called.
//    archUpdateIEEE(crc uint32, p []byte) uint32
//
//    // archAvailableCastagnoli reports whether an architecture-specific
//    // CRC32-C algorithm is available.
//    archAvailableCastagnoli() bool
//
//    // archInitCastagnoli initializes the architecture-specific CRC32-C
//    // algorithm. It can only be called if archAvailableCastagnoli() returns
//    // true.
//    archInitCastagnoli()
//
//    // archUpdateCastagnoli updates the given CRC32-C. It can only be called
//    // if archInitCastagnoli() was previously called.
//    archUpdateCastagnoli(crc uint32, p []byte) uint32

// castagnoliTable points to a lazily initialized Table for the Castagnoli
// polynomial. MakeTable will always return this value when asked to make a
// Castagnoli table so we can compare against it to find when the caller is
// using this polynomial.
internal static ж<Table> castagnoliTable;

internal static ж<slicing8Table> castagnoliTable8;

internal static Func<uint32, slice<byte>, uint32> updateCastagnoli;

internal static ж<atomic.Bool> ᏑhaveCastagnoli = new StandardBox<atomic.Bool>(default(atomic.Bool));
internal static ref atomic.Bool haveCastagnoli => ref ᏑhaveCastagnoli.Value;

// Initialize the slicing-by-8 table.
internal static Action castagnoliInitOnce;
internal static void initᴛcastagnoliInitOnce() { castagnoliInitOnce = sync.OnceFunc(() => {
    castagnoliTable = simpleMakeTable(Castagnoli);
    if (archAvailableCastagnoli()){
        archInitCastagnoli();
        updateCastagnoli = archUpdateCastagnoli;
    } else {
        castagnoliTable8 = slicingMakeTable(Castagnoli);
        updateCastagnoli = (uint32 crc, slice<byte> p) => slicingUpdate(crc, castagnoliTable8, p);
    }
    ᏑhaveCastagnoli.Store(true);
}); }

// IEEETable is the table for the [IEEE] polynomial.
public static ж<Table> IEEETable = simpleMakeTable(IEEE);

// ieeeTable8 is the slicing8Table for IEEE
internal static ж<slicing8Table> ieeeTable8;

internal static Func<uint32, slice<byte>, uint32> updateIEEE;

// Initialize the slicing-by-8 table.
internal static Action ieeeInitOnce;
internal static void initᴛieeeInitOnce() { ieeeInitOnce = sync.OnceFunc(() => {
    if (archAvailableIEEE()){
        archInitIEEE();
        updateIEEE = archUpdateIEEE;
    } else {
        ieeeTable8 = slicingMakeTable(IEEE);
        updateIEEE = (uint32 crc, slice<byte> p) => slicingUpdate(crc, ieeeTable8, p);
    }
}); }

// MakeTable returns a [Table] constructed from the specified polynomial.
// The contents of this [Table] must not be modified.
public static ж<Table> MakeTable(uint32 poly) {
    var exprᴛ1 = poly;
    if (exprᴛ1 == IEEE) {
        ieeeInitOnce();
        return IEEETable;
    }
    if (exprᴛ1 == Castagnoli) {
        castagnoliInitOnce();
        return castagnoliTable;
    }
    { /* default: */
        return simpleMakeTable(poly);
    }

}

// digest represents the partial evaluation of a checksum.
partial struct digest {
    internal uint32 crc;
    internal ж<Table> tab;
}

// New creates a new [hash.Hash32] computing the CRC-32 checksum using the
// polynomial represented by the [Table]. Its Sum method will lay the
// value out in big-endian byte order. The returned Hash32 also
// implements [encoding.BinaryMarshaler] and [encoding.BinaryUnmarshaler] to
// marshal and unmarshal the internal state of the hash.
public static hash.Hash32 New(ж<Table> Ꮡtab) {
    if (Ꮡtab == IEEETable) {
        ieeeInitOnce();
    }
    return new digestжHash32(Ꮡ(new digest(0, Ꮡtab)));
}

// NewIEEE creates a new [hash.Hash32] computing the CRC-32 checksum using
// the [IEEE] polynomial. Its Sum method will lay the value out in
// big-endian byte order. The returned Hash32 also implements
// [encoding.BinaryMarshaler] and [encoding.BinaryUnmarshaler] to marshal
// and unmarshal the internal state of the hash.
public static hash.Hash32 NewIEEE() {
    return New(IEEETable);
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

internal static readonly @string magic = "crc\x01"u8;
internal const nint marshaledSize = /* len(magic) + 4 + 4 */ 12;

internal static (slice<byte>, error) AppendBinary(this ref digest d, slice<byte> b) {
    b = append(b, magic.ꓸꓸꓸ);
    b = byteorder.BEAppendUint32(b, tableSum(d.tab));
    b = byteorder.BEAppendUint32(b, d.crc);
    return (b, default!);
}

internal static (slice<byte>, error) MarshalBinary(this ref digest d) {
    return d.AppendBinary(new slice<byte>(0, marshaledSize));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hashCrc32InvalidHashˢ = "hash/crc32: invalid hash state identifier"u8;
internal static readonly @string hashCrc32InvalidHashˢ2 = "hash/crc32: invalid hash state size"u8;
internal static readonly @string hashCrc32TablesDoNotˢ = "hash/crc32: tables do not match"u8;

internal static error UnmarshalBinary(this ref digest d, slice<byte> b) {
    if (len(b) < len(magic) || ((sstring)(b[..(int)(len(magic))])) != magic) {
        return errors.New(hashCrc32InvalidHashˢ);
    }
    if (len(b) != marshaledSize) {
        return errors.New(hashCrc32InvalidHashˢ2);
    }
    if (tableSum(d.tab) != byteorder.BEUint32(b[4..])) {
        return errors.New(hashCrc32TablesDoNotˢ);
    }
    d.crc = byteorder.BEUint32(b[8..]);
    return default!;
}

internal static uint32 update(uint32 crc, ж<Table> Ꮡtab, slice<byte> p, bool checkInitIEEE) {
    switch (ᐧ) {
    case {} when ᏑhaveCastagnoli.Load() && Ꮡtab == castagnoliTable: {
        return updateCastagnoli(crc, p);
    }
    case {} when Ꮡtab == IEEETable: {
        if (checkInitIEEE) {
            ieeeInitOnce();
        }
        return updateIEEE(crc, p);
    }
    default: {
        return simpleUpdate(crc, ref (Ꮡtab).DerefOrNull(), p);
    }}

}

// Update returns the result of adding the bytes in p to the crc.
public static uint32 Update(uint32 crc, ж<Table> Ꮡtab, slice<byte> p) {
    // Unfortunately, because IEEETable is exported, IEEE may be used without a
    // call to MakeTable. We have to make sure it gets initialized in that case.
    return update(crc, Ꮡtab, p, true);
}

internal static (nint n, error err) Write(this ref digest d, slice<byte> p) {
    // We only create digest objects through New() which takes care of
    // initialization in this case.
    d.crc = update(d.crc, d.tab, p, false);
    return (len(p), default!);
}

internal static uint32 Sum32(this ref digest d) {
    return d.crc;
}

internal static slice<byte> Sum(this ref digest d, slice<byte> @in) {
    var s = d.Sum32();
    return append(@in, (byte)((s >> (int)(24))), (byte)((s >> (int)(16))), (byte)((s >> (int)(8))), (byte)s);
}

// Checksum returns the CRC-32 checksum of data
// using the polynomial represented by the [Table].
public static uint32 Checksum(slice<byte> data, ж<Table> Ꮡtab) {
    return Update(0, Ꮡtab, data);
}

// ChecksumIEEE returns the CRC-32 checksum of data
// using the [IEEE] polynomial.
public static uint32 ChecksumIEEE(slice<byte> data) {
    ieeeInitOnce();
    return updateIEEE(0, data);
}

// tableSum returns the IEEE checksum of table t.
internal static uint32 tableSum(ж<Table> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    array<byte> a = new(1024);
    var b = a[..0];
    if (Ꮡt != nil) {
        foreach (var (_, x) in t) {
            b = byteorder.BEAppendUint32(b, x);
        }
    }
    return ChecksumIEEE(b);
}

} // end crc32_package
