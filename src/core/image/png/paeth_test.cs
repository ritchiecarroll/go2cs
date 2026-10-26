// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.image;

using bytes = bytes_package;
using rand = math.rand_package;
using testing = testing_package;
using math;
using static go.image.png_package;

partial class png_internal_test_package {

internal static nint slowAbs(nint x) {
    if (x < 0) {
        return -x;
    }
    return x;
}

// slowPaeth is a slow but simple implementation of the Paeth function.
// It is a straight port of the sample code in the PNG spec, section 9.4.
internal static uint8 slowPaeth(uint8 a, uint8 b, uint8 c) {
    nint p = (nint)a + (nint)b - (nint)c;
    nint pa = slowAbs(p - (nint)a);
    nint pb = slowAbs(p - (nint)b);
    nint pc = slowAbs(p - (nint)c);
    if (pa <= pb && pa <= pc){
        return a;
    } else 
    if (pb <= pc) {
        return b;
    }
    return c;
}

// slowFilterPaeth is a slow but simple implementation of func filterPaeth.
internal static void slowFilterPaeth(slice<byte> cdat, slice<byte> pdat, nint bytesPerPixel) {
    for (nint i = 0; i < bytesPerPixel; i++) {
        cdat[i] += paeth(0, pdat[i], 0);
    }
    for (nint i = bytesPerPixel; i < len(cdat); i++) {
        cdat[i] += paeth(cdat[i - bytesPerPixel], pdat[i], pdat[i - bytesPerPixel]);
    }
}

public static void TestPaeth(ж<testing.T> Ꮡt) {
    for (nint a = 0; a < 256; a += 15) {
        for (nint b = 0; b < 256; b += 15) {
            for (nint c = 0; c < 256; c += 15) {
                var got = paeth((uint8)a, (uint8)b, (uint8)c);
                var want = slowPaeth((uint8)a, (uint8)b, (uint8)c);
                if (got != want) {
                    Ꮡt.Errorf("a, b, c = %d, %d, %d: got %d, want %d"u8, a, b, c, got, want);
                }
            }
        }
    }
}

public static void BenchmarkPaeth(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        paeth((uint8)(i >> 16), (uint8)(i >> 8), (uint8)i);
    }
}

public static void TestPaethDecode(ж<testing.T> Ꮡt) {
    var pdat0 = new slice<byte>(32);
    var pdat1 = new slice<byte>(32);
    var pdat2 = new slice<byte>(32);
    var cdat0 = new slice<byte>(32);
    var cdat1 = new slice<byte>(32);
    var cdat2 = new slice<byte>(32);
    var r = rand.New(rand.NewSource(1));
    for (nint bytesPerPixel = 1; bytesPerPixel <= 8; bytesPerPixel++) {
        for (nint i = 0; i < 100; i++) {
            foreach (var j in range(pdat0)) {
                pdat0[j] = (uint8)r.Uint32();
                cdat0[j] = (uint8)r.Uint32();
            }
            copy(pdat1, pdat0);
            copy(pdat2, pdat0);
            copy(cdat1, cdat0);
            copy(cdat2, cdat0);
            filterPaeth(cdat1, pdat1, bytesPerPixel);
            slowFilterPaeth(cdat2, pdat2, bytesPerPixel);
            if (!bytes.Equal(cdat1, cdat2)) {
                Ꮡt.Errorf("bytesPerPixel: %d\npdat0: % x\ncdat0: % x\ngot:   % x\nwant:  % x"u8, bytesPerPixel, pdat0, cdat0, cdat1, cdat2);
                break;
            }
        }
    }
}

} // end png_internal_test_package
