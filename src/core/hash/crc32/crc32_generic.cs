// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains CRC32 algorithms that are not specific to any architecture
// and don't use hardware acceleration.
//
// The simple (and slow) CRC32 implementation only uses a 256*4 bytes table.
//
// The slicing-by-8 algorithm is a faster implementation that uses a bigger
// table (8*256*4 bytes).
namespace go.hash;

using byteorder = @internal.byteorder_package;
using @internal;

partial class crc32_package {

// simpleMakeTable allocates and constructs a Table for the specified
// polynomial. The table is suitable for use with the simple algorithm
// (simpleUpdate).
internal static ж<Table> simpleMakeTable(uint32 poly) {
    var t = @new<Table>();
    simplePopulateTable(poly, ref (t).DerefOrNull());
    return t;
}

// simplePopulateTable constructs a Table for the specified polynomial, suitable
// for use with simpleUpdate.
internal static void simplePopulateTable(uint32 poly, ref Table t) {
    for (nint i = 0; i < 256; i++) {
        var crc = (uint32)i;
        for (nint j = 0; j < 8; j++) {
            if ((crc & 1) == 1){
                crc = (crc >> 1) ^ poly;
            } else {
                crc >>= (int)(1);
            }
        }
        t[i] = crc;
    }
}

// simpleUpdate uses the simple algorithm to update the CRC, given a table that
// was previously computed using simpleMakeTable.
internal static uint32 simpleUpdate(uint32 crc, ref Table tab, slice<byte> p) {
    crc = ~crc;
    foreach (var (_, v) in p) {
        crc = tab[(byte)((byte)crc ^ v)] ^ (crc >> 8);
    }
    return ~crc;
}

// Use slicing-by-8 when payload >= this value.
internal static UntypedInt slicing8Cutoff => 16;

// slicing8Table is array of 8 Tables, used by the slicing-by-8 algorithm.
partial struct slicing8Table /*[8]Table*/;

// slicingMakeTable constructs a slicing8Table for the specified polynomial. The
// table is suitable for use with the slicing-by-8 algorithm (slicingUpdate).
internal static ж<slicing8Table> slicingMakeTable(uint32 poly) {
    var t = @new<slicing8Table>();
    simplePopulateTable(poly, ref (t.at<Table>(0)).DerefOrNull());
    for (nint i = 0; i < 256; i++) {
        var crc = t.Value[0][i];
        for (nint j = 1; j < 8; j++) {
            crc = t.Value[0][(crc & 0xFF)] ^ (crc >> 8);
            t.Value[j][i] = crc;
        }
    }
    return t;
}

// slicingUpdate uses the slicing-by-8 algorithm to update the CRC, given a
// table that was previously computed using slicingMakeTable.
internal static uint32 slicingUpdate(uint32 crc, ж<slicing8Table> Ꮡtab, slice<byte> p) {
    ref var tab = ref Ꮡtab.DerefOrNull();

    if (len(p) >= slicing8Cutoff) {
        crc = ~crc;
        while (len(p) > 8) {
            crc ^= (uint32)(byteorder.LEUint32(p));
            crc = tab[0][p[7]] ^ tab[1][p[6]] ^ tab[2][p[5]] ^ tab[3][p[4]] ^ tab[4][(crc >> 24)] ^ tab[5][((crc >> 16) & 0xFF)] ^ tab[6][((crc >> 8) & 0xFF)] ^ tab[7][(crc & 0xFF)];
            p = p[8..];
        }
        crc = ~crc;
    }
    if (len(p) == 0) {
        return crc;
    }
    return simpleUpdate(crc, ref nonnil(ref tab)[0], p);
}

} // end crc32_package
