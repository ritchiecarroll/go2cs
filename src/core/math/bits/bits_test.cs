// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using entry = go.math.bits_test_package.entryᴛ1;

namespace go.math;

using static go.math.bits_package;
using runtime = runtime_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using static go.math.bits_internal_test_package;

partial class bits_test_package {

public static void TestUintSize(ж<testing.T> Ꮡt) {
    nuint x = default!;
    {
        var want = /* unsafe.Sizeof(x) */ (uintptr)8 * 8; if (UintSize != want) {
            Ꮡt.Fatalf("UintSize = %d; want %d"u8, (nint)(UintSize), want);
        }
    }
}

public static void TestLeadingZeros(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 256; i++) {
        nint nlz = tab[i].nlz;
        for (nint k = 0; k < 64 - 8; k++) {
            var x = ((uint64)i).Lsh((nuint)k);
            if (x <= (1 << (int)(8)) - 1) {
                nint got = LeadingZeros8((uint8)x);
                nint want = nlz - k + (8 - 8);
                if (x == 0) {
                    want = 8;
                }
                if (got != want) {
                    Ꮡt.Fatalf("LeadingZeros8(%#02x) == %d; want %d"u8, x, got, want);
                }
            }
            if (x <= (1 << (int)(16)) - 1) {
                nint got = LeadingZeros16((uint16)x);
                nint want = nlz - k + (16 - 8);
                if (x == 0) {
                    want = 16;
                }
                if (got != want) {
                    Ꮡt.Fatalf("LeadingZeros16(%#04x) == %d; want %d"u8, x, got, want);
                }
            }
            if (x <= (uint64)(4294967296L - 1)) {
                nint got = LeadingZeros32((uint32)x);
                nint want = nlz - k + (32 - 8);
                if (x == 0) {
                    want = 32;
                }
                if (got != want) {
                    Ꮡt.Fatalf("LeadingZeros32(%#08x) == %d; want %d"u8, x, got, want);
                }
                if (UintSize == 32) {
                    got = LeadingZeros((nuint)x);
                    if (got != want) {
                        Ꮡt.Fatalf("LeadingZeros(%#08x) == %d; want %d"u8, x, got, want);
                    }
                }
            }
            if (x <= 18446744073709551615UL) {
                nint got = LeadingZeros64((uint64)x);
                nint want = nlz - k + (64 - 8);
                if (x == 0) {
                    want = 64;
                }
                if (got != want) {
                    Ꮡt.Fatalf("LeadingZeros64(%#016x) == %d; want %d"u8, x, got, want);
                }
                if (UintSize == 64) {
                    got = LeadingZeros((nuint)x);
                    if (got != want) {
                        Ꮡt.Fatalf("LeadingZeros(%#016x) == %d; want %d"u8, x, got, want);
                    }
                }
            }
        }
    }
}

// Exported (global) variable serving as input for some
// of the benchmarks to ensure side-effect free calls
// are not optimized away.
public static uint64 Input = bits_internal_test_package.DeBruijn64;

// Exported (global) variable to store function results
// during benchmarking to ensure side-effect free calls
// are not optimized away.
public static nint Output;

public static void BenchmarkLeadingZeros(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += LeadingZeros(((nuint)Input >> (int)(((nuint)i % (nuint)UintSize))));
    }
    Output = s;
}

public static void BenchmarkLeadingZeros8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += LeadingZeros8((uint8)(((uint8)Input >> (int)(((nuint)i % 8)))));
    }
    Output = s;
}

public static void BenchmarkLeadingZeros16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += LeadingZeros16((uint16)(((uint16)Input >> (int)(((nuint)i % 16)))));
    }
    Output = s;
}

public static void BenchmarkLeadingZeros32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += LeadingZeros32(((uint32)Input >> (int)(((nuint)i % 32))));
    }
    Output = s;
}

public static void BenchmarkLeadingZeros64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += LeadingZeros64(((uint64)Input >> (int)(((nuint)i % 64))));
    }
    Output = s;
}

public static void TestTrailingZeros(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 256; i++) {
        nint ntz = tab[i].ntz;
        for (nint k = 0; k < 64 - 8; k++) {
            var x = ((uint64)i).Lsh((nuint)k);
            nint want = ntz + k;
            if (x <= (1 << (int)(8)) - 1) {
                nint got = TrailingZeros8((uint8)x);
                if (x == 0) {
                    want = 8;
                }
                if (got != want) {
                    Ꮡt.Fatalf("TrailingZeros8(%#02x) == %d; want %d"u8, x, got, want);
                }
            }
            if (x <= (1 << (int)(16)) - 1) {
                nint got = TrailingZeros16((uint16)x);
                if (x == 0) {
                    want = 16;
                }
                if (got != want) {
                    Ꮡt.Fatalf("TrailingZeros16(%#04x) == %d; want %d"u8, x, got, want);
                }
            }
            if (x <= (uint64)(4294967296L - 1)) {
                nint got = TrailingZeros32((uint32)x);
                if (x == 0) {
                    want = 32;
                }
                if (got != want) {
                    Ꮡt.Fatalf("TrailingZeros32(%#08x) == %d; want %d"u8, x, got, want);
                }
                if (UintSize == 32) {
                    got = TrailingZeros((nuint)x);
                    if (got != want) {
                        Ꮡt.Fatalf("TrailingZeros(%#08x) == %d; want %d"u8, x, got, want);
                    }
                }
            }
            if (x <= 18446744073709551615UL) {
                nint got = TrailingZeros64((uint64)x);
                if (x == 0) {
                    want = 64;
                }
                if (got != want) {
                    Ꮡt.Fatalf("TrailingZeros64(%#016x) == %d; want %d"u8, x, got, want);
                }
                if (UintSize == 64) {
                    got = TrailingZeros((nuint)x);
                    if (got != want) {
                        Ꮡt.Fatalf("TrailingZeros(%#016x) == %d; want %d"u8, x, got, want);
                    }
                }
            }
        }
    }
}

public static void BenchmarkTrailingZeros(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += TrailingZeros(((nuint)Input << (int)(((nuint)i % (nuint)UintSize))));
    }
    Output = s;
}

public static void BenchmarkTrailingZeros8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += TrailingZeros8((uint8)((uint8)Input << (int)(((nuint)i % 8))));
    }
    Output = s;
}

public static void BenchmarkTrailingZeros16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += TrailingZeros16((uint16)((uint16)Input << (int)(((nuint)i % 16))));
    }
    Output = s;
}

public static void BenchmarkTrailingZeros32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += TrailingZeros32(((uint32)Input << (int)(((nuint)i % 32))));
    }
    Output = s;
}

public static void BenchmarkTrailingZeros64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += TrailingZeros64(((uint64)Input << (int)(((nuint)i % 64))));
    }
    Output = s;
}

public static void TestOnesCount(ж<testing.T> Ꮡt) {
    uint64 x = default!;
    for (nint i = 0; i <= 64; i++) {
        testOnesCount(Ꮡt, x, i);
        x = (uint64)((x << (int)(1)) | 1);
    }
    for (nint i = 64; i >= 0; i--) {
        testOnesCount(Ꮡt, x, i);
        x = (x << (int)(1));
    }
    for (nint i = 0; i < 256; i++) {
        for (nint k = 0; k < 64 - 8; k++) {
            testOnesCount(Ꮡt, ((uint64)i).Lsh((nuint)k), tab[i].pop);
        }
    }
}

internal static void testOnesCount(ж<testing.T> Ꮡt, uint64 x, nint want) {
    if (x <= (1 << (int)(8)) - 1) {
        nint got = OnesCount8((uint8)x);
        if (got != want) {
            Ꮡt.Fatalf("OnesCount8(%#02x) == %d; want %d"u8, (uint8)x, got, want);
        }
    }
    if (x <= (1 << (int)(16)) - 1) {
        nint got = OnesCount16((uint16)x);
        if (got != want) {
            Ꮡt.Fatalf("OnesCount16(%#04x) == %d; want %d"u8, (uint16)x, got, want);
        }
    }
    if (x <= (uint64)(4294967296L - 1)) {
        nint got = OnesCount32((uint32)x);
        if (got != want) {
            Ꮡt.Fatalf("OnesCount32(%#08x) == %d; want %d"u8, (uint32)x, got, want);
        }
        if (UintSize == 32) {
            got = OnesCount((nuint)x);
            if (got != want) {
                Ꮡt.Fatalf("OnesCount(%#08x) == %d; want %d"u8, (uint32)x, got, want);
            }
        }
    }
    if (x <= 18446744073709551615UL) {
        nint got = OnesCount64((uint64)x);
        if (got != want) {
            Ꮡt.Fatalf("OnesCount64(%#016x) == %d; want %d"u8, x, got, want);
        }
        if (UintSize == 64) {
            got = OnesCount((nuint)x);
            if (got != want) {
                Ꮡt.Fatalf("OnesCount(%#016x) == %d; want %d"u8, x, got, want);
            }
        }
    }
}

public static void BenchmarkOnesCount(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += OnesCount((nuint)Input);
    }
    Output = s;
}

public static void BenchmarkOnesCount8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += OnesCount8((uint8)Input);
    }
    Output = s;
}

public static void BenchmarkOnesCount16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += OnesCount16((uint16)Input);
    }
    Output = s;
}

public static void BenchmarkOnesCount32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += OnesCount32((uint32)Input);
    }
    Output = s;
}

public static void BenchmarkOnesCount64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += OnesCount64((uint64)Input);
    }
    Output = s;
}

public static void TestRotateLeft(ж<testing.T> Ꮡt) {
    uint64 m = bits_internal_test_package.DeBruijn64;
    for (nuint k = (nuint)0; k < 128; k++) {
        var x8 = (uint8)m;
        var got8 = RotateLeft8(x8, (nint)k);
        var want8 = (uint8)((uint8)(x8 << (int)(((nuint)(k & 0x7)))) | x8.Rsh((8 - (nuint)(k & 0x7))));
        if (got8 != want8) {
            Ꮡt.Fatalf("RotateLeft8(%#02x, %d) == %#02x; want %#02x"u8, x8, k, got8, want8);
        }
        got8 = RotateLeft8(want8, -(nint)k);
        if (got8 != x8) {
            Ꮡt.Fatalf("RotateLeft8(%#02x, -%d) == %#02x; want %#02x"u8, want8, k, got8, x8);
        }
        var x16 = (uint16)m;
        var got16 = RotateLeft16(x16, (nint)k);
        var want16 = (uint16)((uint16)(x16 << (int)(((nuint)(k & 0xf)))) | x16.Rsh((16 - (nuint)(k & 0xf))));
        if (got16 != want16) {
            Ꮡt.Fatalf("RotateLeft16(%#04x, %d) == %#04x; want %#04x"u8, x16, k, got16, want16);
        }
        got16 = RotateLeft16(want16, -(nint)k);
        if (got16 != x16) {
            Ꮡt.Fatalf("RotateLeft16(%#04x, -%d) == %#04x; want %#04x"u8, want16, k, got16, x16);
        }
        var x32 = (uint32)m;
        var got32 = RotateLeft32(x32, (nint)k);
        var want32 = (uint32)((x32 << (int)(((nuint)(k & 0x1f)))) | x32.Rsh((32 - (nuint)(k & 0x1f))));
        if (got32 != want32) {
            Ꮡt.Fatalf("RotateLeft32(%#08x, %d) == %#08x; want %#08x"u8, x32, k, got32, want32);
        }
        got32 = RotateLeft32(want32, -(nint)k);
        if (got32 != x32) {
            Ꮡt.Fatalf("RotateLeft32(%#08x, -%d) == %#08x; want %#08x"u8, want32, k, got32, x32);
        }
        if (UintSize == 32) {
            nuint x = (nuint)m;
            nuint got = RotateLeft(x, (nint)k);
            nuint want = (nuint)((x << (int)(((nuint)(k & 0x1f)))) | x.Rsh((32 - (nuint)(k & 0x1f))));
            if (got != want) {
                Ꮡt.Fatalf("RotateLeft(%#08x, %d) == %#08x; want %#08x"u8, x, k, got, want);
            }
            got = RotateLeft(want, -(nint)k);
            if (got != x) {
                Ꮡt.Fatalf("RotateLeft(%#08x, -%d) == %#08x; want %#08x"u8, want, k, got, x);
            }
        }
        var x64 = (uint64)m;
        var got64 = RotateLeft64(x64, (nint)k);
        var want64 = (uint64)((x64 << (int)(((nuint)(k & 0x3f)))) | x64.Rsh((64 - (nuint)(k & 0x3f))));
        if (got64 != want64) {
            Ꮡt.Fatalf("RotateLeft64(%#016x, %d) == %#016x; want %#016x"u8, x64, k, got64, want64);
        }
        got64 = RotateLeft64(want64, -(nint)k);
        if (got64 != x64) {
            Ꮡt.Fatalf("RotateLeft64(%#016x, -%d) == %#016x; want %#016x"u8, want64, k, got64, x64);
        }
        if (UintSize == 64) {
            nuint x = (nuint)m;
            nuint got = RotateLeft(x, (nint)k);
            nuint want = (nuint)((x << (int)(((nuint)(k & 0x3f)))) | x.Rsh((64 - (nuint)(k & 0x3f))));
            if (got != want) {
                Ꮡt.Fatalf("RotateLeft(%#016x, %d) == %#016x; want %#016x"u8, x, k, got, want);
            }
            got = RotateLeft(want, -(nint)k);
            if (got != x) {
                Ꮡt.Fatalf("RotateLeft(%#08x, -%d) == %#08x; want %#08x"u8, want, k, got, x);
            }
        }
    }
}

public static void BenchmarkRotateLeft(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nuint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += RotateLeft((nuint)Input, i);
    }
    Output = (nint)s;
}

public static void BenchmarkRotateLeft8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint8 s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += RotateLeft8((uint8)Input, i);
    }
    Output = (nint)s;
}

public static void BenchmarkRotateLeft16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint16 s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += RotateLeft16((uint16)Input, i);
    }
    Output = (nint)s;
}

public static void BenchmarkRotateLeft32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint32 s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += RotateLeft32((uint32)Input, i);
    }
    Output = (nint)s;
}

public static void BenchmarkRotateLeft64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint64 s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += RotateLeft64((uint64)Input, i);
    }
    Output = (nint)s;
}

internal partial struct TestReverse_type /*dyn*/ {
    internal uint64 x, r;
}

public static void TestReverse(ж<testing.T> Ꮡt) {
    // test each bit
    for (nuint i = (nuint)0; i < 64; i++) {
        testReverse(Ꮡt, ((uint64)1).Lsh(i), ((uint64)1).Lsh((63 - i)));
    }
    // test a few patterns
    foreach (var (_, test) in new TestReverse_type[]{
        new(0, 0),
        new(0x1, ((uint64)0x8 << (int)(60))),
        new(0x2, ((uint64)0x4 << (int)(60))),
        new(0x3, ((uint64)0xc << (int)(60))),
        new(0x4, ((uint64)0x2 << (int)(60))),
        new(0x5, ((uint64)0xa << (int)(60))),
        new(0x6, ((uint64)0x6 << (int)(60))),
        new(0x7, ((uint64)0xe << (int)(60))),
        new(0x8, ((uint64)0x1 << (int)(60))),
        new(0x9, ((uint64)0x9 << (int)(60))),
        new(0xa, ((uint64)0x5 << (int)(60))),
        new(0xb, ((uint64)0xd << (int)(60))),
        new(0xc, ((uint64)0x3 << (int)(60))),
        new(0xd, ((uint64)0xb << (int)(60))),
        new(0xe, ((uint64)0x7 << (int)(60))),
        new(0xf, ((uint64)0xf << (int)(60))),
        new(0x5686487, 0xe12616a000000000UL),
        new(0x0123456789abcdefUL, 0xf7b3d591e6a2c480UL)
    }.slice()) {
        testReverse(Ꮡt, test.x, test.r);
        testReverse(Ꮡt, test.r, test.x);
    }
}

internal static void testReverse(ж<testing.T> Ꮡt, uint64 x64, uint64 want64) {
    var x8 = (uint8)x64;
    var got8 = Reverse8(x8);
    var want8 = (uint8)((want64 >> (int)((64 - 8))));
    if (got8 != want8) {
        Ꮡt.Fatalf("Reverse8(%#02x) == %#02x; want %#02x"u8, x8, got8, want8);
    }
    var x16 = (uint16)x64;
    var got16 = Reverse16(x16);
    var want16 = (uint16)((want64 >> (int)((64 - 16))));
    if (got16 != want16) {
        Ꮡt.Fatalf("Reverse16(%#04x) == %#04x; want %#04x"u8, x16, got16, want16);
    }
    var x32 = (uint32)x64;
    var got32 = Reverse32(x32);
    var want32 = (uint32)((want64 >> (int)((64 - 32))));
    if (got32 != want32) {
        Ꮡt.Fatalf("Reverse32(%#08x) == %#08x; want %#08x"u8, x32, got32, want32);
    }
    if (UintSize == 32) {
        nuint x = (nuint)x32;
        nuint got = Reverse(x);
        nuint want = (nuint)want32;
        if (got != want) {
            Ꮡt.Fatalf("Reverse(%#08x) == %#08x; want %#08x"u8, x, got, want);
        }
    }
    var got64 = Reverse64(x64);
    if (got64 != want64) {
        Ꮡt.Fatalf("Reverse64(%#016x) == %#016x; want %#016x"u8, x64, got64, want64);
    }
    if (UintSize == 64) {
        nuint x = (nuint)x64;
        nuint got = Reverse(x);
        nuint want = (nuint)want64;
        if (got != want) {
            Ꮡt.Fatalf("Reverse(%#08x) == %#016x; want %#016x"u8, x, got, want);
        }
    }
}

public static void BenchmarkReverse(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nuint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += Reverse((nuint)i);
    }
    Output = (nint)s;
}

public static void BenchmarkReverse8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint8 s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += Reverse8((uint8)i);
    }
    Output = (nint)s;
}

public static void BenchmarkReverse16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint16 s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += Reverse16((uint16)i);
    }
    Output = (nint)s;
}

public static void BenchmarkReverse32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint32 s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += Reverse32((uint32)i);
    }
    Output = (nint)s;
}

public static void BenchmarkReverse64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint64 s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += Reverse64((uint64)i);
    }
    Output = (nint)s;
}

internal partial struct TestReverseBytes_type /*dyn*/ {
    internal uint64 x, r;
}

public static void TestReverseBytes(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestReverseBytes_type[]{
        new(0, 0),
        new(0x01, ((uint64)0x01 << (int)(56))),
        new(0x0123, ((uint64)0x2301 << (int)(48))),
        new(0x012345, ((uint64)0x452301 << (int)(40))),
        new(0x01234567, ((uint64)0x67452301 << (int)(32))),
        new(0x0123456789UL, ((uint64)unchecked((nint)0x8967452301L) << (int)(24))),
        new(0x0123456789abUL, ((uint64)unchecked((nint)0xab8967452301L) << (int)(16))),
        new(0x0123456789abcdUL, ((uint64)unchecked((nint)0xcdab8967452301L) << (int)(8))),
        new(0x0123456789abcdefUL, ((uint64)(nuint)0xefcdab8967452301UL << (int)(0)))
    }.slice()) {
        testReverseBytes(Ꮡt, test.x, test.r);
        testReverseBytes(Ꮡt, test.r, test.x);
    }
}

internal static void testReverseBytes(ж<testing.T> Ꮡt, uint64 x64, uint64 want64) {
    var x16 = (uint16)x64;
    var got16 = ReverseBytes16(x16);
    var want16 = (uint16)((want64 >> (int)((64 - 16))));
    if (got16 != want16) {
        Ꮡt.Fatalf("ReverseBytes16(%#04x) == %#04x; want %#04x"u8, x16, got16, want16);
    }
    var x32 = (uint32)x64;
    var got32 = ReverseBytes32(x32);
    var want32 = (uint32)((want64 >> (int)((64 - 32))));
    if (got32 != want32) {
        Ꮡt.Fatalf("ReverseBytes32(%#08x) == %#08x; want %#08x"u8, x32, got32, want32);
    }
    if (UintSize == 32) {
        nuint x = (nuint)x32;
        nuint got = ReverseBytes(x);
        nuint want = (nuint)want32;
        if (got != want) {
            Ꮡt.Fatalf("ReverseBytes(%#08x) == %#08x; want %#08x"u8, x, got, want);
        }
    }
    var got64 = ReverseBytes64(x64);
    if (got64 != want64) {
        Ꮡt.Fatalf("ReverseBytes64(%#016x) == %#016x; want %#016x"u8, x64, got64, want64);
    }
    if (UintSize == 64) {
        nuint x = (nuint)x64;
        nuint got = ReverseBytes(x);
        nuint want = (nuint)want64;
        if (got != want) {
            Ꮡt.Fatalf("ReverseBytes(%#016x) == %#016x; want %#016x"u8, x, got, want);
        }
    }
}

public static void BenchmarkReverseBytes(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nuint s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += ReverseBytes((nuint)i);
    }
    Output = (nint)s;
}

public static void BenchmarkReverseBytes16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint16 s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += ReverseBytes16((uint16)i);
    }
    Output = (nint)s;
}

public static void BenchmarkReverseBytes32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint32 s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += ReverseBytes32((uint32)i);
    }
    Output = (nint)s;
}

public static void BenchmarkReverseBytes64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint64 s = default!;
    for (nint i = 0; i < b.N; i++) {
        s += ReverseBytes64((uint64)i);
    }
    Output = (nint)s;
}

public static void TestLen(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 256; i++) {
        nint len = 8 - tab[i].nlz;
        for (nint k = 0; k < 64 - 8; k++) {
            var x = ((uint64)i).Lsh((nuint)k);
            nint want = 0;
            if (x != 0) {
                want = len + k;
            }
            if (x <= (1 << (int)(8)) - 1) {
                nint got = Len8((uint8)x);
                if (got != want) {
                    Ꮡt.Fatalf("Len8(%#02x) == %d; want %d"u8, x, got, want);
                }
            }
            if (x <= (1 << (int)(16)) - 1) {
                nint got = Len16((uint16)x);
                if (got != want) {
                    Ꮡt.Fatalf("Len16(%#04x) == %d; want %d"u8, x, got, want);
                }
            }
            if (x <= (uint64)(4294967296L - 1)) {
                nint got = Len32((uint32)x);
                if (got != want) {
                    Ꮡt.Fatalf("Len32(%#08x) == %d; want %d"u8, x, got, want);
                }
                if (UintSize == 32) {
                    nint gotΔ1 = Len((nuint)x);
                    if (gotΔ1 != want) {
                        Ꮡt.Fatalf("Len(%#08x) == %d; want %d"u8, x, gotΔ1, want);
                    }
                }
            }
            if (x <= 18446744073709551615UL) {
                nint got = Len64((uint64)x);
                if (got != want) {
                    Ꮡt.Fatalf("Len64(%#016x) == %d; want %d"u8, x, got, want);
                }
                if (UintSize == 64) {
                    nint gotΔ1 = Len((nuint)x);
                    if (gotΔ1 != want) {
                        Ꮡt.Fatalf("Len(%#016x) == %d; want %d"u8, x, gotΔ1, want);
                    }
                }
            }
        }
    }
}

internal static UntypedInt _M => /* 1<<UintSize - 1 */ 18446744073709551615;
internal static UntypedInt _M32 => /* 1<<32 - 1 */ 4294967295;
internal static UntypedInt _M64 => /* 1<<64 - 1 */ 18446744073709551615;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addˢ = "Add"u8;
internal static readonly @string addSymmetricˢ = "Add symmetric"u8;
internal static readonly @string subˢ = "Sub"u8;
internal static readonly @string subSymmetricˢ = "Sub symmetric"u8;
internal static readonly @string addIntrinsicˢ = "Add intrinsic"u8;
internal static readonly @string addIntrinsicSymmetricˢ = "Add intrinsic symmetric"u8;
internal static readonly @string subIntrinsicˢ = "Sub intrinsic"u8;
internal static readonly @string subIntrinsicSymmetricˢ = "Sub intrinsic symmetric"u8;

internal partial struct TestAddSubUint_type /*dyn*/ {
    internal nuint x, y, c, z, cout;
}

public static void TestAddSubUint(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    void test(@string msg, Func<nuint, nuint, nuint, (nuint, nuint)> f, nuint x, nuint y, nuint c, nuint z, nuint cout) {
        var (z1, cout1) = f(x, y, c);
        if (z1 != z || cout1 != cout) {
            Ꮡt.Errorf("%s: got z:cout = %#x:%#x; want %#x:%#x"u8, msg, z1, cout1, z, cout);
        }
    }
    foreach (var (_, a) in new TestAddSubUint_type[]{
        new(0, 0, 0, 0, 0),
        new(0, 1, 0, 1, 0),
        new(0, 0, 1, 1, 0),
        new(0, 1, 1, 2, 0),
        new(12345, 67890, 0, 80235, 0),
        new(12345, 67890, 1, 80236, 0),
        new(_M, 1, 0, 0, 1),
        new(_M, 0, 1, 0, 1),
        new(_M, 1, 1, 1, 1),
        new(_M, _M, 0, _M - 1, 1),
        new(_M, _M, 1, _M, 1)
    }.slice()) {
        test(addˢ, Add, a.x, a.y, a.c, a.z, a.cout);
        test(addSymmetricˢ, Add, a.y, a.x, a.c, a.z, a.cout);
        test(subˢ, Sub, a.z, a.x, a.c, a.y, a.cout);
        test(subSymmetricˢ, Sub, a.z, a.y, a.c, a.x, a.cout);
        // The above code can't test intrinsic implementation, because the passed function is not called directly.
        // The following code uses a closure to test the intrinsic version in case the function is intrinsified.
        test(addIntrinsicˢ, (nuint x, nuint y, nuint c) => Add(x, y, c), a.x, a.y, a.c, a.z, a.cout);
        test(addIntrinsicSymmetricˢ, (nuint x, nuint y, nuint c) => Add(x, y, c), a.y, a.x, a.c, a.z, a.cout);
        test(subIntrinsicˢ, (nuint x, nuint y, nuint c) => Sub(x, y, c), a.z, a.x, a.c, a.y, a.cout);
        test(subIntrinsicSymmetricˢ, (nuint x, nuint y, nuint c) => Sub(x, y, c), a.z, a.y, a.c, a.x, a.cout);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string add32ˢ = "Add32"u8;
internal static readonly @string add32Symmetricˢ = "Add32 symmetric"u8;
internal static readonly @string sub32ˢ = "Sub32"u8;
internal static readonly @string sub32Symmetricˢ = "Sub32 symmetric"u8;

internal partial struct TestAddSubUint32_type /*dyn*/ {
    internal uint32 x, y, c, z, cout;
}

public static void TestAddSubUint32(ж<testing.T> Ꮡt) {
    void test(@string msg, Func<uint32, uint32, uint32, (uint32, uint32)> f, uint32 x, uint32 y, uint32 c, uint32 z, uint32 cout) {
        var (z1, cout1) = f(x, y, c);
        if (z1 != z || cout1 != cout) {
            Ꮡt.Errorf("%s: got z:cout = %#x:%#x; want %#x:%#x"u8, msg, z1, cout1, z, cout);
        }
    }
    foreach (var (_, a) in new TestAddSubUint32_type[]{
        new(0, 0, 0, 0, 0),
        new(0, 1, 0, 1, 0),
        new(0, 0, 1, 1, 0),
        new(0, 1, 1, 2, 0),
        new(12345, 67890, 0, 80235, 0),
        new(12345, 67890, 1, 80236, 0),
        new(_M32, 1, 0, 0, 1),
        new(_M32, 0, 1, 0, 1),
        new(_M32, 1, 1, 1, 1),
        new(_M32, _M32, 0, _M32 - 1, 1),
        new(_M32, _M32, 1, _M32, 1)
    }.slice()) {
        test(add32ˢ, Add32, a.x, a.y, a.c, a.z, a.cout);
        test(add32Symmetricˢ, Add32, a.y, a.x, a.c, a.z, a.cout);
        test(sub32ˢ, Sub32, a.z, a.x, a.c, a.y, a.cout);
        test(sub32Symmetricˢ, Sub32, a.z, a.y, a.c, a.x, a.cout);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string add64ˢ = "Add64"u8;
internal static readonly @string add64Symmetricˢ = "Add64 symmetric"u8;
internal static readonly @string sub64ˢ = "Sub64"u8;
internal static readonly @string sub64Symmetricˢ = "Sub64 symmetric"u8;
internal static readonly @string add64Intrinsicˢ = "Add64 intrinsic"u8;
internal static readonly @string add64IntrinsicSymmetricˢ = "Add64 intrinsic symmetric"u8;
internal static readonly @string sub64Intrinsicˢ = "Sub64 intrinsic"u8;
internal static readonly @string sub64IntrinsicSymmetricˢ = "Sub64 intrinsic symmetric"u8;

internal partial struct TestAddSubUint64_type /*dyn*/ {
    internal uint64 x, y, c, z, cout;
}

public static void TestAddSubUint64(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    void test(@string msg, Func<uint64, uint64, uint64, (uint64, uint64)> f, uint64 x, uint64 y, uint64 c, uint64 z, uint64 cout) {
        var (z1, cout1) = f(x, y, c);
        if (z1 != z || cout1 != cout) {
            Ꮡt.Errorf("%s: got z:cout = %#x:%#x; want %#x:%#x"u8, msg, z1, cout1, z, cout);
        }
    }
    foreach (var (_, a) in new TestAddSubUint64_type[]{
        new(0, 0, 0, 0, 0),
        new(0, 1, 0, 1, 0),
        new(0, 0, 1, 1, 0),
        new(0, 1, 1, 2, 0),
        new(12345, 67890, 0, 80235, 0),
        new(12345, 67890, 1, 80236, 0),
        new(_M64, 1, 0, 0, 1),
        new(_M64, 0, 1, 0, 1),
        new(_M64, 1, 1, 1, 1),
        new(_M64, _M64, 0, _M64 - 1, 1),
        new(_M64, _M64, 1, _M64, 1)
    }.slice()) {
        test(add64ˢ, Add64, a.x, a.y, a.c, a.z, a.cout);
        test(add64Symmetricˢ, Add64, a.y, a.x, a.c, a.z, a.cout);
        test(sub64ˢ, Sub64, a.z, a.x, a.c, a.y, a.cout);
        test(sub64Symmetricˢ, Sub64, a.z, a.y, a.c, a.x, a.cout);
        // The above code can't test intrinsic implementation, because the passed function is not called directly.
        // The following code uses a closure to test the intrinsic version in case the function is intrinsified.
        test(add64Intrinsicˢ, (uint64 x, uint64 y, uint64 c) => Add64(x, y, c), a.x, a.y, a.c, a.z, a.cout);
        test(add64IntrinsicSymmetricˢ, (uint64 x, uint64 y, uint64 c) => Add64(x, y, c), a.y, a.x, a.c, a.z, a.cout);
        test(sub64Intrinsicˢ, (uint64 x, uint64 y, uint64 c) => Sub64(x, y, c), a.z, a.x, a.c, a.y, a.cout);
        test(sub64IntrinsicSymmetricˢ, (uint64 x, uint64 y, uint64 c) => Sub64(x, y, c), a.z, a.y, a.c, a.x, a.cout);
    }
}

public static void TestAdd64OverflowPanic(ж<testing.T> Ꮡt) {
    // Test that 64-bit overflow panics fire correctly.
    // These are designed to improve coverage of compiler intrinsics.
    var tests = new Func<uint64, uint64, uint64>[]{
        (uint64 a, uint64 b) => {
            var (x, c) = Add64(a, b, 0);
            if (c > 0) {
                throw panic("overflow");
            }
            return x;
        },
        (uint64 a, uint64 b) => {
            var (x, c) = Add64(a, b, 0);
            if (c != 0) {
                throw panic("overflow");
            }
            return x;
        },
        (uint64 a, uint64 b) => {
            var (x, c) = Add64(a, b, 0);
            if (c == 1) {
                throw panic("overflow");
            }
            return x;
        },
        (uint64 a, uint64 b) => {
            var (x, c) = Add64(a, b, 0);
            if (c != 1) {
                return x;
            }
            throw panic("overflow");
        },
        (uint64 a, uint64 b) => {
            var (x, c) = Add64(a, b, 0);
            if (c == 0) {
                return x;
            }
            throw panic("overflow");
        }
    }.slice();
    foreach (var (_, test) in tests) {
        void shouldPanic(Action f) {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    {
                        var err = recover(); if (err == default!) {
                            Ꮡt.Fatalf("expected panic"u8);
                        }
                    }
                }, ref ᒐ);
                f();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }
        // overflow
        var testʗ1 = test;
        shouldPanic(() => {
            testʗ1(_M64, 1);
        });
        var testʗ2 = test;
        shouldPanic(() => {
            testʗ2(1, _M64);
        });
        var testʗ3 = test;
        shouldPanic(() => {
            testʗ3(_M64, _M64);
        });
        // no overflow
        test(_M64, 0);
        test(0, 0);
        test(1, 1);
    }
}

public static void TestSub64OverflowPanic(ж<testing.T> Ꮡt) {
    // Test that 64-bit overflow panics fire correctly.
    // These are designed to improve coverage of compiler intrinsics.
    var tests = new Func<uint64, uint64, uint64>[]{
        (uint64 a, uint64 b) => {
            var (x, c) = Sub64(a, b, 0);
            if (c > 0) {
                throw panic("overflow");
            }
            return x;
        },
        (uint64 a, uint64 b) => {
            var (x, c) = Sub64(a, b, 0);
            if (c != 0) {
                throw panic("overflow");
            }
            return x;
        },
        (uint64 a, uint64 b) => {
            var (x, c) = Sub64(a, b, 0);
            if (c == 1) {
                throw panic("overflow");
            }
            return x;
        },
        (uint64 a, uint64 b) => {
            var (x, c) = Sub64(a, b, 0);
            if (c != 1) {
                return x;
            }
            throw panic("overflow");
        },
        (uint64 a, uint64 b) => {
            var (x, c) = Sub64(a, b, 0);
            if (c == 0) {
                return x;
            }
            throw panic("overflow");
        }
    }.slice();
    foreach (var (_, test) in tests) {
        void shouldPanic(Action f) {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    {
                        var err = recover(); if (err == default!) {
                            Ꮡt.Fatalf("expected panic"u8);
                        }
                    }
                }, ref ᒐ);
                f();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }
        // overflow
        var testʗ1 = test;
        shouldPanic(() => {
            testʗ1(0, 1);
        });
        var testʗ2 = test;
        shouldPanic(() => {
            testʗ2(1, _M64);
        });
        var testʗ3 = test;
        shouldPanic(() => {
            testʗ3(_M64 - 1, _M64);
        });
        // no overflow
        test(_M64, 0);
        test(0, 0);
        test(1, 1);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mulˢ = "Mul"u8;
internal static readonly @string mulSymmetricˢ = "Mul symmetric"u8;
internal static readonly @string divˢ = "Div"u8;
internal static readonly @string divSymmetricˢ = "Div symmetric"u8;
internal static readonly @string mulIntrinsicˢ = "Mul intrinsic"u8;
internal static readonly @string mulIntrinsicSymmetricˢ = "Mul intrinsic symmetric"u8;
internal static readonly @string divIntrinsicˢ = "Div intrinsic"u8;
internal static readonly @string divIntrinsicSymmetricˢ = "Div intrinsic symmetric"u8;

internal partial struct TestMulDiv_type /*dyn*/ {
    internal nuint x, y;
    internal nuint hi, lo, r;
}

public static void TestMulDiv(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    void testMul(@string msg, Func<nuint, nuint, (nuint, nuint)> f, nuint x, nuint y, nuint hi, nuint lo) {
        var (hi1, lo1) = f(x, y);
        if (hi1 != hi || lo1 != lo) {
            Ꮡt.Errorf("%s: got hi:lo = %#x:%#x; want %#x:%#x"u8, msg, hi1, lo1, hi, lo);
        }
    }
    void testDiv(@string msg, Func<nuint, nuint, nuint, (nuint, nuint)> f, nuint hi, nuint lo, nuint y, nuint q, nuint r) {
        var (q1, r1) = f(hi, lo, y);
        if (q1 != q || r1 != r) {
            Ꮡt.Errorf("%s: got q:r = %#x:%#x; want %#x:%#x"u8, msg, q1, r1, q, r);
        }
    }
    foreach (var (_, a) in new TestMulDiv_type[]{
        new(((nuint)1 << (int)((UintSize - 1))), 2, 1, 0, 1),
        new(_M, _M, _M - 1, 1, 42)
    }.slice()) {
        testMul(mulˢ, Mul, a.x, a.y, a.hi, a.lo);
        testMul(mulSymmetricˢ, Mul, a.y, a.x, a.hi, a.lo);
        testDiv(divˢ, Div, a.hi, a.lo + a.r, a.y, a.x, a.r);
        testDiv(divSymmetricˢ, Div, a.hi, a.lo + a.r, a.x, a.y, a.r);
        // The above code can't test intrinsic implementation, because the passed function is not called directly.
        // The following code uses a closure to test the intrinsic version in case the function is intrinsified.
        testMul(mulIntrinsicˢ, (nuint x, nuint y) => Mul(x, y), a.x, a.y, a.hi, a.lo);
        testMul(mulIntrinsicSymmetricˢ, (nuint x, nuint y) => Mul(x, y), a.y, a.x, a.hi, a.lo);
        testDiv(divIntrinsicˢ, (nuint hi, nuint lo, nuint y) => Div(hi, lo, y), a.hi, a.lo + a.r, a.y, a.x, a.r);
        testDiv(divIntrinsicSymmetricˢ, (nuint hi, nuint lo, nuint y) => Div(hi, lo, y), a.hi, a.lo + a.r, a.x, a.y, a.r);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mul32ˢ = "Mul32"u8;
internal static readonly @string mul32Symmetricˢ = "Mul32 symmetric"u8;
internal static readonly @string div32ˢ = "Div32"u8;
internal static readonly @string div32Symmetricˢ = "Div32 symmetric"u8;

internal partial struct TestMulDiv32_type /*dyn*/ {
    internal uint32 x, y;
    internal uint32 hi, lo, r;
}

public static void TestMulDiv32(ж<testing.T> Ꮡt) {
    void testMul(@string msg, Func<uint32, uint32, (uint32, uint32)> f, uint32 x, uint32 y, uint32 hi, uint32 lo) {
        var (hi1, lo1) = f(x, y);
        if (hi1 != hi || lo1 != lo) {
            Ꮡt.Errorf("%s: got hi:lo = %#x:%#x; want %#x:%#x"u8, msg, hi1, lo1, hi, lo);
        }
    }
    void testDiv(@string msg, Func<uint32, uint32, uint32, (uint32, uint32)> f, uint32 hi, uint32 lo, uint32 y, uint32 q, uint32 r) {
        var (q1, r1) = f(hi, lo, y);
        if (q1 != q || r1 != r) {
            Ꮡt.Errorf("%s: got q:r = %#x:%#x; want %#x:%#x"u8, msg, q1, r1, q, r);
        }
    }
    foreach (var (_, a) in new TestMulDiv32_type[]{
        new(((uint32)1 << (int)(31)), 2, 1, 0, 1),
        new(0xc47dfa8cU, 50911, 0x98a4, 0x998587f4U, 13),
        new(_M32, _M32, _M32 - 1, 1, 42)
    }.slice()) {
        testMul(mul32ˢ, Mul32, a.x, a.y, a.hi, a.lo);
        testMul(mul32Symmetricˢ, Mul32, a.y, a.x, a.hi, a.lo);
        testDiv(div32ˢ, Div32, a.hi, a.lo + a.r, a.y, a.x, a.r);
        testDiv(div32Symmetricˢ, Div32, a.hi, a.lo + a.r, a.x, a.y, a.r);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mul64ˢ = "Mul64"u8;
internal static readonly @string mul64Symmetricˢ = "Mul64 symmetric"u8;
internal static readonly @string div64ˢ = "Div64"u8;
internal static readonly @string div64Symmetricˢ = "Div64 symmetric"u8;
internal static readonly @string mul64Intrinsicˢ = "Mul64 intrinsic"u8;
internal static readonly @string mul64IntrinsicSymmetricˢ = "Mul64 intrinsic symmetric"u8;
internal static readonly @string div64Intrinsicˢ = "Div64 intrinsic"u8;
internal static readonly @string div64IntrinsicSymmetricˢ = "Div64 intrinsic symmetric"u8;

internal partial struct TestMulDiv64_type /*dyn*/ {
    internal uint64 x, y;
    internal uint64 hi, lo, r;
}

public static void TestMulDiv64(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    void testMul(@string msg, Func<uint64, uint64, (uint64, uint64)> f, uint64 x, uint64 y, uint64 hi, uint64 lo) {
        var (hi1, lo1) = f(x, y);
        if (hi1 != hi || lo1 != lo) {
            Ꮡt.Errorf("%s: got hi:lo = %#x:%#x; want %#x:%#x"u8, msg, hi1, lo1, hi, lo);
        }
    }
    void testDiv(@string msg, Func<uint64, uint64, uint64, (uint64, uint64)> f, uint64 hi, uint64 lo, uint64 y, uint64 q, uint64 r) {
        var (q1, r1) = f(hi, lo, y);
        if (q1 != q || r1 != r) {
            Ꮡt.Errorf("%s: got q:r = %#x:%#x; want %#x:%#x"u8, msg, q1, r1, q, r);
        }
    }
    foreach (var (_, a) in new TestMulDiv64_type[]{
        new(((uint64)1 << (int)(63)), 2, 1, 0, 1),
        new(0x3626229738a3b9UL, 0xd8988a9f1cc4a61UL, 0x2dd0712657fe8UL, 0x9dd6a3364c358319UL, 13),
        new(_M64, _M64, _M64 - 1, 1, 42)
    }.slice()) {
        testMul(mul64ˢ, Mul64, a.x, a.y, a.hi, a.lo);
        testMul(mul64Symmetricˢ, Mul64, a.y, a.x, a.hi, a.lo);
        testDiv(div64ˢ, Div64, a.hi, a.lo + a.r, a.y, a.x, a.r);
        testDiv(div64Symmetricˢ, Div64, a.hi, a.lo + a.r, a.x, a.y, a.r);
        // The above code can't test intrinsic implementation, because the passed function is not called directly.
        // The following code uses a closure to test the intrinsic version in case the function is intrinsified.
        testMul(mul64Intrinsicˢ, (uint64 x, uint64 y) => Mul64(x, y), a.x, a.y, a.hi, a.lo);
        testMul(mul64IntrinsicSymmetricˢ, (uint64 x, uint64 y) => Mul64(x, y), a.y, a.x, a.hi, a.lo);
        testDiv(div64Intrinsicˢ, (uint64 hi, uint64 lo, uint64 y) => Div64(hi, lo, y), a.hi, a.lo + a.r, a.y, a.x, a.r);
        testDiv(div64IntrinsicSymmetricˢ, (uint64 hi, uint64 lo, uint64 y) => Div64(hi, lo, y), a.hi, a.lo + a.r, a.x, a.y, a.r);
    }
}

internal static readonly @string divZeroError = "runtime error: integer divide by zero"u8;
internal static readonly @string overflowError = "runtime error: integer overflow"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object divShouldHavePanickedˢ = (@string)"Div should have panicked when y<=hi"u8;

public static void TestDivPanicOverflow(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Expect a panic
        defer(() => {
            {
                var err = recover(); if (err == default!){
                    Ꮡt.Error(divShouldHavePanickedˢ);
                } else 
                {
                    var (e, ok) = err._<runtimeꓸError>(ᐧ); if (!ok || e.Error() != overflowError) {
                        Ꮡt.Errorf("Div expected panic: %q, got: %q "u8, overflowError, e.Error());
                    }
                }
            }
        }, ref ᒐ);
        var (q, r) = Div(1, 0, 1);
        Ꮡt.Errorf("undefined q, r = %v, %v calculated when Div should have panicked"u8, q, r);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object div32ShouldHavePanickedˢ = (@string)"Div32 should have panicked when y<=hi"u8;

public static void TestDiv32PanicOverflow(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Expect a panic
        defer(() => {
            {
                var err = recover(); if (err == default!){
                    Ꮡt.Error(div32ShouldHavePanickedˢ);
                } else 
                {
                    var (e, ok) = err._<runtimeꓸError>(ᐧ); if (!ok || e.Error() != overflowError) {
                        Ꮡt.Errorf("Div32 expected panic: %q, got: %q "u8, overflowError, e.Error());
                    }
                }
            }
        }, ref ᒐ);
        var (q, r) = Div32(1, 0, 1);
        Ꮡt.Errorf("undefined q, r = %v, %v calculated when Div32 should have panicked"u8, q, r);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object div64ShouldHavePanickedˢ = (@string)"Div64 should have panicked when y<=hi"u8;

public static void TestDiv64PanicOverflow(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Expect a panic
        defer(() => {
            {
                var err = recover(); if (err == default!){
                    Ꮡt.Error(div64ShouldHavePanickedˢ);
                } else 
                {
                    var (e, ok) = err._<runtimeꓸError>(ᐧ); if (!ok || e.Error() != overflowError) {
                        Ꮡt.Errorf("Div64 expected panic: %q, got: %q "u8, overflowError, e.Error());
                    }
                }
            }
        }, ref ᒐ);
        var (q, r) = Div64(1, 0, 1);
        Ꮡt.Errorf("undefined q, r = %v, %v calculated when Div64 should have panicked"u8, q, r);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object divShouldHavePanickedˢ2 = (@string)"Div should have panicked when y==0"u8;

public static void TestDivPanicZero(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Expect a panic
        defer(() => {
            {
                var err = recover(); if (err == default!){
                    Ꮡt.Error(divShouldHavePanickedˢ2);
                } else 
                {
                    var (e, ok) = err._<runtimeꓸError>(ᐧ); if (!ok || e.Error() != divZeroError) {
                        Ꮡt.Errorf("Div expected panic: %q, got: %q "u8, divZeroError, e.Error());
                    }
                }
            }
        }, ref ᒐ);
        var (q, r) = Div(1, 1, 0);
        Ꮡt.Errorf("undefined q, r = %v, %v calculated when Div should have panicked"u8, q, r);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object div32ShouldHavePanickedˢ2 = (@string)"Div32 should have panicked when y==0"u8;

public static void TestDiv32PanicZero(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Expect a panic
        defer(() => {
            {
                var err = recover(); if (err == default!){
                    Ꮡt.Error(div32ShouldHavePanickedˢ2);
                } else 
                {
                    var (e, ok) = err._<runtimeꓸError>(ᐧ); if (!ok || e.Error() != divZeroError) {
                        Ꮡt.Errorf("Div32 expected panic: %q, got: %q "u8, divZeroError, e.Error());
                    }
                }
            }
        }, ref ᒐ);
        var (q, r) = Div32(1, 1, 0);
        Ꮡt.Errorf("undefined q, r = %v, %v calculated when Div32 should have panicked"u8, q, r);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object div64ShouldHavePanickedˢ2 = (@string)"Div64 should have panicked when y==0"u8;

public static void TestDiv64PanicZero(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Expect a panic
        defer(() => {
            {
                var err = recover(); if (err == default!){
                    Ꮡt.Error(div64ShouldHavePanickedˢ2);
                } else 
                {
                    var (e, ok) = err._<runtimeꓸError>(ᐧ); if (!ok || e.Error() != divZeroError) {
                        Ꮡt.Errorf("Div64 expected panic: %q, got: %q "u8, divZeroError, e.Error());
                    }
                }
            }
        }, ref ᒐ);
        var (q, r) = Div64(1, 1, 0);
        Ꮡt.Errorf("undefined q, r = %v, %v calculated when Div64 should have panicked"u8, q, r);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestRem32(ж<testing.T> Ꮡt) {
    // Sanity check: for non-overflowing dividends, the result is the
    // same as the rem returned by Div32
    var (hi, lo, y) = ((uint32)510510, (uint32)9699690, (uint32)(510510 + 1)); // ensure hi < y
    for (nint i = 0; i < 1000; i++) {
        var r = Rem32(hi, lo, y);
        var (_, r2) = Div32(hi, lo, y);
        if (r != r2) {
            Ꮡt.Errorf("Rem32(%v, %v, %v) returned %v, but Div32 returned rem %v"u8, hi, lo, y, r, r2);
        }
        y += 13;
    }
}

public static void TestRem32Overflow(ж<testing.T> Ꮡt) {
    // To trigger a quotient overflow, we need y <= hi
    var (hi, lo, y) = ((uint32)510510, (uint32)9699690, (uint32)7);
    for (nint i = 0; i < 1000; i++) {
        var r = Rem32(hi, lo, y);
        var (_, r2) = Div64(0, (uint64)(((uint64)hi << (int)(32)) | (uint64)lo), (uint64)y);
        if (r != (uint32)r2) {
            Ꮡt.Errorf("Rem32(%v, %v, %v) returned %v, but Div64 returned rem %v"u8, hi, lo, y, r, r2);
        }
        y += 13;
    }
}

public static void TestRem64(ж<testing.T> Ꮡt) {
    // Sanity check: for non-overflowing dividends, the result is the
    // same as the rem returned by Div64
    var (hi, lo, y) = ((uint64)510510, (uint64)9699690, (uint64)(510510 + 1)); // ensure hi < y
    for (nint i = 0; i < 1000; i++) {
        var r = Rem64(hi, lo, y);
        var (_, r2) = Div64(hi, lo, y);
        if (r != r2) {
            Ꮡt.Errorf("Rem64(%v, %v, %v) returned %v, but Div64 returned rem %v"u8, hi, lo, y, r, r2);
        }
        y += 13;
    }
}

internal partial struct TestRem64Overflow_Rem64Tests /*dyn*/ {
    internal uint64 hi, lo, y;
    internal uint64 rem;
}

public static void TestRem64Overflow(ж<testing.T> Ꮡt) {
    var Rem64Tests = new TestRem64Overflow_Rem64Tests[]{ // Testcases computed using Python 3, as:
 //   >>> hi = 42; lo = 1119; y = 42
 //   >>> ((hi<<64)+lo) % y

        new(42, 1119, 42, 27),
        new(42, 1119, 38, 9),
        new(42, 1119, 26, 23),
        new(469, 0, 467, 271),
        new(469, 0, 113, 58),
        new(111111, 111111, 1171, 803),
        new(3968194946088682615UL, 3192705705065114702UL, 1000037, 56067)
    }.slice();
    foreach (var (_, rt) in Rem64Tests) {
        if (rt.hi < rt.y) {
            Ꮡt.Fatalf("Rem64(%v, %v, %v) is not a test with quo overflow"u8, rt.hi, rt.lo, rt.y);
        }
        var rem = Rem64(rt.hi, rt.lo, rt.y);
        if (rem != rt.rem) {
            Ꮡt.Errorf("Rem64(%v, %v, %v) returned %v, wanted %v"u8,
                rt.hi, rt.lo, rt.y, rem, rt.rem);
        }
    }
}

public static void BenchmarkAdd(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nuint z = default!;
    nuint c = default!;
    for (nint i = 0; i < b.N; i++) {
        (z, c) = Add((nuint)Input, (nuint)i, c);
    }
    Output = (nint)(z + c);
}

public static void BenchmarkAdd32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint32 z = default!;
    uint32 c = default!;
    for (nint i = 0; i < b.N; i++) {
        (z, c) = Add32((uint32)Input, (uint32)i, c);
    }
    Output = (nint)(z + c);
}

public static void BenchmarkAdd64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint64 z = default!;
    uint64 c = default!;
    for (nint i = 0; i < b.N; i++) {
        (z, c) = Add64((uint64)Input, (uint64)i, c);
    }
    Output = (nint)(z + c);
}

public static void BenchmarkAdd64multiple(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint64 z0 = (uint64)Input;
    uint64 z1 = (uint64)Input;
    uint64 z2 = (uint64)Input;
    uint64 z3 = (uint64)Input;
    for (nint i = 0; i < b.N; i++) {
        uint64 c = default!;
        (z0, c) = Add64(z0, (uint64)i, c);
        (z1, c) = Add64(z1, (uint64)i, c);
        (z2, c) = Add64(z2, (uint64)i, c);
        (z3, _) = Add64(z3, (uint64)i, c);
    }
    Output = (nint)(z0 + z1 + z2 + z3);
}

public static void BenchmarkSub(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nuint z = default!;
    nuint c = default!;
    for (nint i = 0; i < b.N; i++) {
        (z, c) = Sub((nuint)Input, (nuint)i, c);
    }
    Output = (nint)(z + c);
}

public static void BenchmarkSub32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint32 z = default!;
    uint32 c = default!;
    for (nint i = 0; i < b.N; i++) {
        (z, c) = Sub32((uint32)Input, (uint32)i, c);
    }
    Output = (nint)(z + c);
}

public static void BenchmarkSub64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint64 z = default!;
    uint64 c = default!;
    for (nint i = 0; i < b.N; i++) {
        (z, c) = Sub64((uint64)Input, (uint64)i, c);
    }
    Output = (nint)(z + c);
}

public static void BenchmarkSub64multiple(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint64 z0 = (uint64)Input;
    uint64 z1 = (uint64)Input;
    uint64 z2 = (uint64)Input;
    uint64 z3 = (uint64)Input;
    for (nint i = 0; i < b.N; i++) {
        uint64 c = default!;
        (z0, c) = Sub64(z0, (uint64)i, c);
        (z1, c) = Sub64(z1, (uint64)i, c);
        (z2, c) = Sub64(z2, (uint64)i, c);
        (z3, _) = Sub64(z3, (uint64)i, c);
    }
    Output = (nint)(z0 + z1 + z2 + z3);
}

public static void BenchmarkMul(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nuint hi = default!;
    nuint lo = default!;
    for (nint i = 0; i < b.N; i++) {
        (hi, lo) = Mul((nuint)Input, (nuint)i);
    }
    Output = (nint)(hi + lo);
}

public static void BenchmarkMul32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint32 hi = default!;
    uint32 lo = default!;
    for (nint i = 0; i < b.N; i++) {
        (hi, lo) = Mul32((uint32)Input, (uint32)i);
    }
    Output = (nint)(hi + lo);
}

public static void BenchmarkMul64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint64 hi = default!;
    uint64 lo = default!;
    for (nint i = 0; i < b.N; i++) {
        (hi, lo) = Mul64((uint64)Input, (uint64)i);
    }
    Output = (nint)(hi + lo);
}

public static void BenchmarkDiv(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nuint q = default!;
    nuint r = default!;
    for (nint i = 0; i < b.N; i++) {
        (q, r) = Div(1, (nuint)i, (nuint)Input);
    }
    Output = (nint)(q + r);
}

public static void BenchmarkDiv32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint32 q = default!;
    uint32 r = default!;
    for (nint i = 0; i < b.N; i++) {
        (q, r) = Div32(1, (uint32)i, (uint32)Input);
    }
    Output = (nint)(q + r);
}

public static void BenchmarkDiv64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    uint64 q = default!;
    uint64 r = default!;
    for (nint i = 0; i < b.N; i++) {
        (q, r) = Div64(1, (uint64)i, (uint64)Input);
    }
    Output = (nint)(q + r);
}

// ----------------------------------------------------------------------------
// Testing support
partial struct entryᴛ1 /*dyn*/ {
    internal nint nlz, ntz, pop;
}

// tab contains results for all uint8 values
internal static array<entry> tab = new(256);

[GoInit] internal static void init() {
    tab[0] = new entry(8, 8, 0);
    for (nint i = 1; i < len(tab); i++) {
        // nlz
        nint x = i; // x != 0
        nint n = 0;
        while ((nint)(x & 0x80) == 0) {
            n++;
            x <<= (int)(1);
        }
        tab[i].nlz = n;
        // ntz
        x = i; // x != 0
        n = 0;
        while ((nint)(x & 1) == 0) {
            n++;
            x >>= (int)(1);
        }
        tab[i].ntz = n;
        // pop
        x = i; // x != 0
        n = 0;
        while (x != 0) {
            n += (nint)((nint)(x & 1));
            x >>= (int)(1);
        }
        tab[i].pop = n;
    }
}

} // end bits_test_package
