// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testing = testing_package;
using static math_package;
using static go.math_internal_test_package;

partial class math_test_package {

public static void TestMaxUint(ж<testing.T> t) {
    {
        nuint v = (nuint)MaxUint; if (v + 1 != 0) {
            t.Errorf("MaxUint should wrap around to zero: %d", v + 1);
        }
    }
    {
        var v = (uint8)MaxUint8; if ((uint8)(v + 1) != 0) {
            t.Errorf("MaxUint8 should wrap around to zero: %d", (uint8)(v + 1));
        }
    }
    {
        var v = (uint16)MaxUint16; if ((uint16)(v + 1) != 0) {
            t.Errorf("MaxUint16 should wrap around to zero: %d", (uint16)(v + 1));
        }
    }
    {
        var v = (uint32)MaxUint32; if (v + 1 != 0) {
            t.Errorf("MaxUint32 should wrap around to zero: %d", v + 1);
        }
    }
    {
        var v = (uint64)MaxUint64; if (v + 1 != 0) {
            t.Errorf("MaxUint64 should wrap around to zero: %d", v + 1);
        }
    }
}

public static void TestMaxInt(ж<testing.T> t) {
    {
        nint v = (nint)MaxInt; if (v + 1 != MinInt) {
            t.Errorf("MaxInt should wrap around to MinInt: %d", v + 1);
        }
    }
    {
        var v = (int8)MaxInt8; if ((int8)(v + 1) != MinInt8) {
            t.Errorf("MaxInt8 should wrap around to MinInt8: %d", (int8)(v + 1));
        }
    }
    {
        var v = (int16)MaxInt16; if ((int16)(v + 1) != MinInt16) {
            t.Errorf("MaxInt16 should wrap around to MinInt16: %d", (int16)(v + 1));
        }
    }
    {
        var v = (int32)MaxInt32; if (v + 1 != MinInt32) {
            t.Errorf("MaxInt32 should wrap around to MinInt32: %d", v + 1);
        }
    }
    {
        var v = (int64)MaxInt64; if (v + 1 != MinInt64) {
            t.Errorf("MaxInt64 should wrap around to MinInt64: %d", v + 1);
        }
    }
}

} // end math_test_package
