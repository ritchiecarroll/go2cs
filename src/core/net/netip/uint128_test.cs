// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net;

using testing = testing_package;

partial class netip_package {

internal partial struct TestUint128AddSub_tests /*dyn*/ {
    internal uint128 @in;
    internal nint op; // +1 or -1 to add vs subtract
    internal uint128 want;
}

public static void TestUint128AddSub(ж<testing.T> t) {
    const nint add1 = 1;
    const nint sub1 = -1;
    var tests = slice<TestUint128AddSub_tests>([
        new(new uint128(0, 0), add1, new uint128(0, 1)),
        new(new uint128(0, 1), add1, new uint128(0, 2)),
        new(new uint128(1, 0), add1, new uint128(1, 1)),
        new(new uint128(0, ~(uint64)0), add1, new uint128(1, 0)),
        new(new uint128(~(uint64)0, ~(uint64)0), add1, new uint128(0, 0)),

        new(new uint128(0, 0), sub1, new uint128(~(uint64)0, ~(uint64)0)),
        new(new uint128(0, 1), sub1, new uint128(0, 0)),
        new(new uint128(0, 2), sub1, new uint128(0, 1)),
        new(new uint128(1, 0), sub1, new uint128(0, ~(uint64)0)),
        new(new uint128(1, 1), sub1, new uint128(1, 0))
    ]);
    foreach (var (_, tt) in range(tests)) {
        uint128 got = default;
        switch (tt.op) {
        case var ᴛ when ᴛ == add1: {
            got = tt.@in.addOne();
            break;
        }
        case var ᴛ when ᴛ == sub1: {
            got = tt.@in.subOne();
            break;
        }
        default: {
            throw panic("bogus op");
            break;
        }}
        if (got != tt.want) {
            t.Errorf("%v add %d = %v; want %v", tt.@in, tt.op, got, tt.want);
        }
    }
}

internal partial struct TestBitsSetFrom_tests /*dyn*/ {
    internal uint8 bit;
    internal uint128 want;
}

public static void TestBitsSetFrom(ж<testing.T> t) {
    var tests = slice<TestBitsSetFrom_tests>([
        new(0, new uint128(~(uint64)0, ~(uint64)0)),
        new(1, new uint128(~(uint64)0 >> 1, ~(uint64)0)),
        new(63, new uint128(1, ~(uint64)0)),
        new(64, new uint128(0, ~(uint64)0)),
        new(65, new uint128(0, ~(uint64)0 >> 1)),
        new(127, new uint128(0, 1)),
        new(128, new uint128(0, 0))
    ]);
    foreach (var (_, tt) in range(tests)) {
        uint128 zero = default;
        var got = zero.bitsSetFrom(tt.bit);
        if (got != tt.want) {
            t.Errorf("0.bitsSetFrom(%d) = %064b want %064b", tt.bit, got, tt.want);
        }
    }
}

internal partial struct TestBitsClearedFrom_tests /*dyn*/ {
    internal uint8 bit;
    internal uint128 want;
}

public static void TestBitsClearedFrom(ж<testing.T> t) {
    var tests = slice<TestBitsClearedFrom_tests>([
        new(0, new uint128(0, 0)),
        new(1, new uint128((uint64)1 << 63, 0)),
        new(63, new uint128((uint64)(~(uint64)0 & ~(uint64)1), 0)),
        new(64, new uint128(~(uint64)0, 0)),
        new(65, new uint128(~(uint64)0, (uint64)1 << 63)),
        new(127, new uint128(~(uint64)0, (uint64)(~(uint64)0 & ~(uint64)1))),
        new(128, new uint128(~(uint64)0, ~(uint64)0))
    ]);
    foreach (var (_, tt) in range(tests)) {
        var ones = new uint128(~(uint64)0, ~(uint64)0);
        var got = ones.bitsClearedFrom(tt.bit);
        if (got != tt.want) {
            t.Errorf("ones.bitsClearedFrom(%d) = %064b want %064b", tt.bit, got, tt.want);
        }
    }
}

} // end netip_package
