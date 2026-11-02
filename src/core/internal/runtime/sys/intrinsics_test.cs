// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using sys = go.@internal.runtime.sys_package;
using testing = testing_package;
using go.@internal.runtime;

partial class sys_test_package {

public static void TestTrailingZeros64(ж<testing.T> t) {
    for (nint i = 0; i <= 64; i++) {
        var x = ((uint64)5).Lsh((nuint)i);
        {
            nint got = sys.TrailingZeros64(x); if (got != i) {
                t.Errorf("TrailingZeros64(%d)=%d, want %d", x, got, i);
            }
        }
    }
}

public static void TestTrailingZeros32(ж<testing.T> t) {
    for (nint i = 0; i <= 32; i++) {
        var x = ((uint32)5).Lsh((nuint)i);
        {
            nint got = sys.TrailingZeros32(x); if (got != i) {
                t.Errorf("TrailingZeros32(%d)=%d, want %d", x, got, i);
            }
        }
    }
}

public static void TestBswap64(ж<testing.T> t) {
    var x = (uint64)0x1122334455667788UL;
    var y = sys.Bswap64(x);
    if (y != 0x8877665544332211UL) {
        t.Errorf("Bswap(%x)=%x, want 0x8877665544332211", x, y);
    }
}

public static void TestBswap32(ж<testing.T> t) {
    var x = (uint32)0x11223344;
    var y = sys.Bswap32(x);
    if (y != 0x44332211) {
        t.Errorf("Bswap(%x)=%x, want 0x44332211", x, y);
    }
}

} // end sys_test_package
