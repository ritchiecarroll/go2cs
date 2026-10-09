// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static bytes_package;
using fmt = fmt_package;
using testing = testing_package;
using static go.bytes_internal_test_package;

partial class bytes_test_package {

// test runtime·memeq's chunked implementation
// nil tests

partial struct compareTestsᴛ1 /*dyn*/ {
    internal slice<byte> a, b;
    internal nint i;
}
internal static slice<compareTestsᴛ1> compareTests = new compareTestsᴛ1[]{
    new(slice<byte>(""u8), slice<byte>(""u8), 0),
    new(slice<byte>("a"u8), slice<byte>(""u8), 1),
    new(slice<byte>(""u8), slice<byte>("a"u8), -1),
    new(slice<byte>("abc"u8), slice<byte>("abc"u8), 0),
    new(slice<byte>("abd"u8), slice<byte>("abc"u8), 1),
    new(slice<byte>("abc"u8), slice<byte>("abd"u8), -1),
    new(slice<byte>("ab"u8), slice<byte>("abc"u8), -1),
    new(slice<byte>("abc"u8), slice<byte>("ab"u8), 1),
    new(slice<byte>("x"u8), slice<byte>("ab"u8), 1),
    new(slice<byte>("ab"u8), slice<byte>("x"u8), -1),
    new(slice<byte>("x"u8), slice<byte>("a"u8), 1),
    new(slice<byte>("b"u8), slice<byte>("x"u8), -1),
    new(slice<byte>("abcdefgh"u8), slice<byte>("abcdefgh"u8), 0),
    new(slice<byte>("abcdefghi"u8), slice<byte>("abcdefghi"u8), 0),
    new(slice<byte>("abcdefghi"u8), slice<byte>("abcdefghj"u8), -1),
    new(slice<byte>("abcdefghj"u8), slice<byte>("abcdefghi"u8), 1),
    new(default!, default!, 0),
    new(slice<byte>(""u8), default!, 0),
    new(default!, slice<byte>(""u8), 0),
    new(slice<byte>("a"u8), default!, 1),
    new(default!, slice<byte>("a"u8), -1)
}.slice();

public static void TestCompare(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in compareTests) {
        nint numShifts = 16;
        var buffer = new slice<byte>(len(tt.b) + numShifts);
        // vary the input alignment of tt.b
        for (nint offset = 0; offset <= numShifts; offset++) {
            var shiftedB = buffer.slice(offset, len(tt.b) + offset);
            copy(shiftedB, tt.b);
            nint cmp = Compare(tt.a, shiftedB);
            if (cmp != tt.i) {
                Ꮡt.Errorf(@"Compare(%q, %q), offset %d = %v; want %v"u8, tt.a, tt.b, offset, cmp, tt.i);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bB1Failedˢ = (@string)"b > b[:1] failed"u8;

public static void TestCompareIdenticalSlice(ж<testing.T> Ꮡt) {
    slice<byte> b = slice<byte>("Hello Gophers!"u8);
    if (Compare(b, b) != 0) {
        Ꮡt.Error((@string)"b != b"u8);
    }
    if (Compare(b, b[..1]) != 1) {
        Ꮡt.Error(bB1Failedˢ);
    }
}

public static void TestCompareBytes(ж<testing.T> Ꮡt) {
    var lengths = new slice<nint>(0); // lengths to test in ascending order
    for (nint i = 0; i <= 128; i++) {
        lengths = append(lengths, i);
    }
    lengths = append(lengths, (nint)(256), (nint)(512), (nint)(1024), (nint)(1333), (nint)(4095), (nint)(4096), (nint)(4097));
    if (!testing.Short()) {
        lengths = append(lengths, (nint)(65535), (nint)(65536), (nint)(65537), (nint)(99999));
    }
    nint n = lengths[len(lengths) - 1];
    var a = new slice<byte>(n + 1);
    var b = new slice<byte>(n + 1);
    foreach (var (_, lenΔ1) in lengths) {
        // randomish but deterministic data. No 0 or 255.
        for (nint i = 0; i < lenΔ1; i++) {
            a[i] = (byte)(1 + 31 * i % 254);
            b[i] = (byte)(1 + 31 * i % 254);
        }
        // data past the end is different
        for (nint i = lenΔ1; i <= n; i++) {
            a[i] = 8;
            b[i] = 9;
        }
        nint cmp = Compare(a.slice(0, lenΔ1), b.slice(0, lenΔ1));
        if (cmp != 0) {
            Ꮡt.Errorf(@"CompareIdentical(%d) = %d"u8, lenΔ1, cmp);
        }
        if (lenΔ1 > 0) {
            cmp = Compare(a.slice(0, lenΔ1 - 1), b.slice(0, lenΔ1));
            if (cmp != -1) {
                Ꮡt.Errorf(@"CompareAshorter(%d) = %d"u8, lenΔ1, cmp);
            }
            cmp = Compare(a.slice(0, lenΔ1), b.slice(0, lenΔ1 - 1));
            if (cmp != 1) {
                Ꮡt.Errorf(@"CompareBshorter(%d) = %d"u8, lenΔ1, cmp);
            }
        }
        for (nint k = 0; k < lenΔ1; k++) {
            b[k] = (byte)(a[k] - 1);
            cmp = Compare(a.slice(0, lenΔ1), b.slice(0, lenΔ1));
            if (cmp != 1) {
                Ꮡt.Errorf(@"CompareAbigger(%d,%d) = %d"u8, lenΔ1, k, cmp);
            }
            b[k] = (byte)(a[k] + 1);
            cmp = Compare(a.slice(0, lenΔ1), b.slice(0, lenΔ1));
            if (cmp != -1) {
                Ꮡt.Errorf(@"CompareBbigger(%d,%d) = %d"u8, lenΔ1, k, cmp);
            }
            b[k] = a[k];
        }
    }
}

public static void TestEndianBaseCompare(ж<testing.T> Ꮡt) {
    // This test compares byte slices that are almost identical, except one
    // difference that for some j, a[j]>b[j] and a[j+1]<b[j+1]. If the implementation
    // compares large chunks with wrong endianness, it gets wrong result.
    // no vector register is larger than 512 bytes for now
    const nint maxLength = 512;
    var a = new slice<byte>(maxLength);
    var b = new slice<byte>(maxLength);
    // randomish but deterministic data. No 0 or 255.
    for (nint i = 0; i < maxLength; i++) {
        a[i] = (byte)(1 + 31 * i % 254);
        b[i] = (byte)(1 + 31 * i % 254);
    }
    for (nint i = 2; i <= maxLength; i <<= (int)(1)) {
        for (nint j = 0; j < i - 1; j++) {
            a[j] = (byte)(b[j] - 1);
            a[j + 1] = (byte)(b[j + 1] + 1);
            nint cmp = Compare(a.slice(0, i), b.slice(0, i));
            if (cmp != -1) {
                Ꮡt.Errorf(@"CompareBbigger(%d,%d) = %d"u8, i, j, cmp);
            }
            a[j] = (byte)(b[j] + 1);
            a[j + 1] = (byte)(b[j + 1] - 1);
            cmp = Compare(a.slice(0, i), b.slice(0, i));
            if (cmp != 1) {
                Ꮡt.Errorf(@"CompareAbigger(%d,%d) = %d"u8, i, j, cmp);
            }
            a[j] = b[j];
            a[j + 1] = b[j + 1];
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object b1B2ˢ = (@string)"b1 != b2"u8;

public static void BenchmarkCompareBytesEqual(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var b1 = slice<byte>("Hello Gophers!"u8);
    var b2 = slice<byte>("Hello Gophers!"u8);
    for (nint i = 0; i < b.N; i++) {
        if (Compare(b1, b2) != 0) {
            Ꮡb.Fatal(b1B2ˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object b1B2Failedˢ = (@string)"b1 > b2 failed"u8;

public static void BenchmarkCompareBytesToNil(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var b1 = slice<byte>("Hello Gophers!"u8);
    slice<byte> b2 = default!;
    for (nint i = 0; i < b.N; i++) {
        if (Compare(b1, b2) != 1) {
            Ꮡb.Fatal(b1B2Failedˢ);
        }
    }
}

public static void BenchmarkCompareBytesEmpty(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var b1 = slice<byte>(""u8);
    var b2 = b1;
    for (nint i = 0; i < b.N; i++) {
        if (Compare(b1, b2) != 0) {
            Ꮡb.Fatal(b1B2ˢ);
        }
    }
}

public static void BenchmarkCompareBytesIdentical(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var b1 = slice<byte>("Hello Gophers!"u8);
    var b2 = b1;
    for (nint i = 0; i < b.N; i++) {
        if (Compare(b1, b2) != 0) {
            Ꮡb.Fatal(b1B2ˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object b1B2Failedˢ2 = (@string)"b1 < b2 failed"u8;

public static void BenchmarkCompareBytesSameLength(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var b1 = slice<byte>("Hello Gophers!"u8);
    var b2 = slice<byte>("Hello, Gophers"u8);
    for (nint i = 0; i < b.N; i++) {
        if (Compare(b1, b2) != -1) {
            Ꮡb.Fatal(b1B2Failedˢ2);
        }
    }
}

public static void BenchmarkCompareBytesDifferentLength(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var b1 = slice<byte>("Hello Gophers!"u8);
    var b2 = slice<byte>("Hello, Gophers!"u8);
    for (nint i = 0; i < b.N; i++) {
        if (Compare(b1, b2) != -1) {
            Ꮡb.Fatal(b1B2Failedˢ2);
        }
    }
}

internal static void benchmarkCompareBytesBigUnaligned(ж<testing.B> Ꮡb, nint offset) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var b1 = new slice<byte>(0, (1 << (int)(20)));
    while (len(b1) < (1 << (int)(20))) {
        b1 = append(b1, ((@string)"Hello Gophers!"u8).ꓸꓸꓸ);
    }
    var b2 = appendꓸꓸꓸ(slice<byte>("12345678"u8).slice(0, offset), b1);
    b.StartTimer();
    for (nint j = 0; j < b.N; j++) {
        if (Compare(b1, b2.slice(offset)) != 0) {
            Ꮡb.Fatal(b1B2ˢ);
        }
    }
    b.SetBytes((int64)len(b1));
}

public static void BenchmarkCompareBytesBigUnaligned(ж<testing.B> Ꮡb) {
    for (nint iᴛ1 = 1; iᴛ1 < 8; iᴛ1++) {
        var i = iᴛ1;
        Ꮡb.Run(fmt.Sprintf("offset=%d"u8, i), (ж<testing.B> bΔ1) => {
            benchmarkCompareBytesBigUnaligned(bΔ1, i);
        });
    }
}

internal static void benchmarkCompareBytesBigBothUnaligned(ж<testing.B> Ꮡb, nint offset) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var pattern = slice<byte>("Hello Gophers!"u8);
    var b1 = new slice<byte>(0, (1 << (int)(20)) + len(pattern));
    while (len(b1) < (1 << (int)(20))) {
        b1 = appendꓸꓸꓸ(b1, pattern);
    }
    var b2 = new slice<byte>(len(b1));
    copy(b2, b1);
    b.StartTimer();
    for (nint j = 0; j < b.N; j++) {
        if (Compare(b1.slice(offset), b2.slice(offset)) != 0) {
            Ꮡb.Fatal(b1B2ˢ);
        }
    }
    b.SetBytes((int64)len(b1.slice(offset)));
}

public static void BenchmarkCompareBytesBigBothUnaligned(ж<testing.B> Ꮡb) {
    for (nint iᴛ1 = 0; iᴛ1 < 8; iᴛ1++) {
        var i = iᴛ1;
        Ꮡb.Run(fmt.Sprintf("offset=%d"u8, i), (ж<testing.B> bΔ1) => {
            benchmarkCompareBytesBigBothUnaligned(bΔ1, i);
        });
    }
}

public static void BenchmarkCompareBytesBig(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var b1 = new slice<byte>(0, (1 << (int)(20)));
    while (len(b1) < (1 << (int)(20))) {
        b1 = append(b1, ((@string)"Hello Gophers!"u8).ꓸꓸꓸ);
    }
    var b2 = appendꓸꓸꓸ(new byte[]{}.slice(), b1);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        if (Compare(b1, b2) != 0) {
            Ꮡb.Fatal(b1B2ˢ);
        }
    }
    b.SetBytes((int64)len(b1));
}

public static void BenchmarkCompareBytesBigIdentical(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var b1 = new slice<byte>(0, (1 << (int)(20)));
    while (len(b1) < (1 << (int)(20))) {
        b1 = append(b1, ((@string)"Hello Gophers!"u8).ꓸꓸꓸ);
    }
    var b2 = b1;
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        if (Compare(b1, b2) != 0) {
            Ꮡb.Fatal(b1B2ˢ);
        }
    }
    b.SetBytes((int64)len(b1));
}

} // end bytes_test_package
