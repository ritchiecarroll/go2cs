// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using rand = math.rand.rand_package;
using testing = testing_package;
using time = time_package;
using go.crypto.@internal.fips140deps;
using math.rand;
using static go.crypto.@internal.fips140.subtle_package;

partial class subtle_internal_test_package {

public static void TestConstantTimeLessOrEqBytes(ж<testing.T> Ꮡt) {
    var seed = new slice<byte>(32);
    byteorder.BEPutUint64(seed, (uint64)time.Now().UnixNano());
    var r = rand.NewChaCha8(new array<byte>(seed, 32));
    foreach (var l in range(20)) {
        var a = new slice<byte>(l);
        var b = new slice<byte>(l);
        var empty = new slice<byte>(l);
        r.Read(a);
        r.Read(b);
        nint exp = 0;
        if (bytes.Compare(a, b) <= 0) {
            exp = 1;
        }
        {
            nint got = ConstantTimeLessOrEqBytes(a, b); if (got != exp) {
                Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want %d"u8, a, b, got, exp);
            }
        }
        exp = 0;
        if (bytes.Compare(b, a) <= 0) {
            exp = 1;
        }
        {
            nint got = ConstantTimeLessOrEqBytes(b, a); if (got != exp) {
                Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want %d"u8, b, a, got, exp);
            }
        }
        {
            nint got = ConstantTimeLessOrEqBytes(empty, a); if (got != 1) {
                Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want 1"u8, empty, a, got);
            }
        }
        {
            nint got = ConstantTimeLessOrEqBytes(empty, b); if (got != 1) {
                Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want 1"u8, empty, b, got);
            }
        }
        {
            nint got = ConstantTimeLessOrEqBytes(a, a); if (got != 1) {
                Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want 1"u8, a, a, got);
            }
        }
        {
            nint got = ConstantTimeLessOrEqBytes(b, b); if (got != 1) {
                Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want 1"u8, b, b, got);
            }
        }
        {
            nint got = ConstantTimeLessOrEqBytes(empty, empty); if (got != 1) {
                Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want 1"u8, empty, empty, got);
            }
        }
        if (l == 0) {
            continue;
        }
        var max = new slice<byte>(l);
        foreach (var i in range(max)) {
            max[i] = 0xff;
        }
        {
            nint got = ConstantTimeLessOrEqBytes(a, max); if (got != 1) {
                Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want 1"u8, a, max, got);
            }
        }
        {
            nint got = ConstantTimeLessOrEqBytes(b, max); if (got != 1) {
                Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want 1"u8, b, max, got);
            }
        }
        {
            nint got = ConstantTimeLessOrEqBytes(empty, max); if (got != 1) {
                Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want 1"u8, empty, max, got);
            }
        }
        {
            nint got = ConstantTimeLessOrEqBytes(max, max); if (got != 1) {
                Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want 1"u8, max, max, got);
            }
        }
        var aPlusOne = new slice<byte>(l);
        copy(aPlusOne, a);
        for (nint i = l - 1; i >= 0; i--) {
            if (aPlusOne[i] == 0xff) {
                aPlusOne[i] = 0;
                continue;
            }
            aPlusOne[i]++;
            {
                nint got = ConstantTimeLessOrEqBytes(a, aPlusOne); if (got != 1) {
                    Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want 1"u8, a, aPlusOne, got);
                }
            }
            {
                nint got = ConstantTimeLessOrEqBytes(aPlusOne, a); if (got != 0) {
                    Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want 0"u8, aPlusOne, a, got);
                }
            }
            break;
        }
        var shorter = new slice<byte>(l - 1);
        copy(shorter, a);
        {
            nint got = ConstantTimeLessOrEqBytes(a, shorter); if (got != 0) {
                Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want 0"u8, a, shorter, got);
            }
        }
        {
            nint got = ConstantTimeLessOrEqBytes(shorter, a); if (got != 0) {
                Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want 0"u8, shorter, a, got);
            }
        }
        {
            nint got = ConstantTimeLessOrEqBytes(b, shorter); if (got != 0) {
                Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want 0"u8, b, shorter, got);
            }
        }
        {
            nint got = ConstantTimeLessOrEqBytes(shorter, b); if (got != 0) {
                Ꮡt.Errorf("ConstantTimeLessOrEqBytes(%x, %x) = %d, want 0"u8, shorter, b, got);
            }
        }
    }
}

} // end subtle_internal_test_package
