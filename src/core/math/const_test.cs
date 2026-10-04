// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testing = testing_package;
using static math_package;
using static go.math_internal_test_package;

partial class math_test_package {

public static void TestMaxUint(ж<testing.T> Ꮡt) {
    {
        nuint v = (nuint)MaxUint; if (v + 1 != 0) {
            Ꮡt.Errorf("MaxUint should wrap around to zero: %d"u8, v + 1);
        }
    }
    {
        var v = (uint8)MaxUint8; if ((uint8)(v + 1) != 0) {
            Ꮡt.Errorf("MaxUint8 should wrap around to zero: %d"u8, (uint8)(v + 1));
        }
    }
    {
        var v = (uint16)MaxUint16; if ((uint16)(v + 1) != 0) {
            Ꮡt.Errorf("MaxUint16 should wrap around to zero: %d"u8, (uint16)(v + 1));
        }
    }
    {
        var v = (uint32)MaxUint32; if (v + 1 != 0) {
            Ꮡt.Errorf("MaxUint32 should wrap around to zero: %d"u8, v + 1);
        }
    }
    {
        var v = (uint64)MaxUint64; if (v + 1 != 0) {
            Ꮡt.Errorf("MaxUint64 should wrap around to zero: %d"u8, v + 1);
        }
    }
}

public static void TestMaxInt(ж<testing.T> Ꮡt) {
    {
        nint v = (nint)MaxInt; if (v + 1 != MinInt) {
            Ꮡt.Errorf("MaxInt should wrap around to MinInt: %d"u8, v + 1);
        }
    }
    {
        var v = (int8)MaxInt8; if ((int8)(v + 1) != MinInt8) {
            Ꮡt.Errorf("MaxInt8 should wrap around to MinInt8: %d"u8, (int8)(v + 1));
        }
    }
    {
        var v = (int16)MaxInt16; if ((int16)(v + 1) != MinInt16) {
            Ꮡt.Errorf("MaxInt16 should wrap around to MinInt16: %d"u8, (int16)(v + 1));
        }
    }
    {
        var v = (int32)MaxInt32; if (v + 1 != MinInt32) {
            Ꮡt.Errorf("MaxInt32 should wrap around to MinInt32: %d"u8, v + 1);
        }
    }
    {
        var v = (int64)MaxInt64; if (v + 1 != MinInt64) {
            Ꮡt.Errorf("MaxInt64 should wrap around to MinInt64: %d"u8, v + 1);
        }
    }
}

} // end math_test_package
