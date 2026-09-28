// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math.rand;

using static global::go.math.rand.rand_package;
using testing = testing_package;
using global::go.math.rand;
using rand = global::go.math.rand.rand_package;
using static global::go.math.rand.rand_internal_test_package;

partial class rand_test_package {

public static void BenchmarkPCG_DXSM(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    rand.PCG p = default!;
    uint64 t = default!;
    for (nint n = b.N; n > 0; n--) {
        t += p.Uint64();
    }
    Sink = t;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = ((@string)(new byte[]{0x70, 0x63, 0x67, 0x3a, 0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde, 0xf0, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54, 0x32, 0x10}));

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantAppendᶜ = ((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x70, 0x63, 0x67, 0x3a, 0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde, 0xf0, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54, 0x32, 0x10}));

public static void TestPCGMarshal(ж<testing.T> Ꮡt) {
    rand.PCG p = default!;
    const uint64 seed1 = 0x123456789abcdef0;
    const uint64 seed2 = 0xfedcba9876543210;
    @string want = wantᶜ;
    @string wantAppend = wantAppendᶜ;
    p.Seed(seed1, seed2);
    var (data, err) = p.MarshalBinary();
    if (((sstring)data) != want || err != default!) {
        Ꮡt.Errorf("MarshalBinary() = %q, %v, want %q, nil"u8, data, err, want);
    }
    var dataAppend = new slice<byte>(4, 32);
    (dataAppend, err) = p.AppendBinary(dataAppend);
    if (((sstring)dataAppend) != wantAppend || err != default!) {
        Ꮡt.Errorf("AppendBinary() = %q, %v, want %q, nil"u8, dataAppend, err, wantAppend);
    }
    var q = new PCG(nil);
    {
        var errΔ1 = q.UnmarshalBinary(slice<byte>(want)); if (errΔ1 != default!) {
            Ꮡt.Fatalf("UnmarshalBinary(): %v"u8, errΔ1);
        }
    }
    if (q != p) {
        Ꮡt.Fatalf("after round trip, q = %#x, but p = %#x"u8, q, p);
    }
    var qu = q.Uint64();
    var pu = p.Uint64();
    if (qu != pu) {
        Ꮡt.Errorf("after round trip, q.Uint64() = %#x, but p.Uint64() = %#x"u8, qu, pu);
    }
}

public static void TestPCG(ж<testing.T> Ꮡt) {
    var p = NewPCG(1, 2);
    var want = new uint64[]{
        0xc4f5a58656eef510UL,
        0x9dcec3ad077dec6cUL,
        0xc8d04605312f8088UL,
        0xcbedc0dcb63ac19aUL,
        0x3bf98798cae97950UL,
        0xa8c6d7f8d485abcUL,
        0x7ffa3780429cd279UL,
        0x730ad2626b1c2f8eUL,
        0x21ff2330f4a0ad99UL,
        0x2f0901a1947094b0UL,
        0xa9735a3cfbe36cefUL,
        0x71ddb0a01a12c84aUL,
        0xf0e53e77a78453bbUL,
        0x1f173e9663be1e9dUL,
        0x657651da3ac4115eUL,
        0xc8987376b65a157bUL,
        0xbb17008f5fca28e7UL,
        0x8232bd645f29ed22UL,
        0x12be8f07ad14c539UL,
        0x54908a48e8e4736eUL
    }.slice();
    foreach (var (i, x) in want) {
        {
            var u = p.Uint64(); if (u != x) {
                Ꮡt.Errorf("PCG #%d = %#x, want %#x"u8, i, u, x);
            }
        }
    }
}

} // end rand_test_package
