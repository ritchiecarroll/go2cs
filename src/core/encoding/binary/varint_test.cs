// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using io = io_package;
using math = math_package;
using testing = testing_package;
using static go.encoding.binary_package;

partial class binary_internal_test_package {

internal static void testConstant(ж<testing.T> t, nuint w, nint max) {
    var buf = new slice<byte>(MaxVarintLen64);
    nint n = PutUvarint(buf, ((uint64)1).Lsh(w) - 1);
    if (n != max) {
        t.Errorf("MaxVarintLen%d = %d; want %d", w, max, n);
    }
}

public static void TestConstants(ж<testing.T> t) {
    testConstant(t, 16, MaxVarintLen16);
    testConstant(t, 32, MaxVarintLen32);
    testConstant(t, 64, MaxVarintLen64);
}

internal static void testVarint(ж<testing.T> t, int64 x) {
    var buf = new slice<byte>(MaxVarintLen64);
    nint n = PutVarint(buf, x);
    var (y, m) = Varint(buf.slice(0, n));
    if (x != y) {
        t.Errorf("Varint(%d): got %d", x, y);
    }
    if (n != m) {
        t.Errorf("Varint(%d): got n = %d; want %d", x, m, n);
    }

    var buf2 = slice<byte>("prefix"u8);
    buf2 = AppendVarint(buf2, x);
    if (((@string)buf2) != "prefix"u8 + ((sstring)(buf.slice(0, n)))) {
        t.Errorf("AppendVarint(%d): got %q, want %q", x, buf2, "prefix" + ((sstring)(buf.slice(0, n))));
    }

    (y, var err) = ReadVarint(new binary_test_package.bytes_ReaderжByteReader(bytes.NewReader(buf)));
    if (err != nil) {
        t.Errorf("ReadVarint(%d): %s", x, err);
    }
    if (x != y) {
        t.Errorf("ReadVarint(%d): got %d", x, y);
    }
}

internal static void testUvarint(ж<testing.T> t, uint64 x) {
    var buf = new slice<byte>(MaxVarintLen64);
    nint n = PutUvarint(buf, x);
    var (y, m) = Uvarint(buf.slice(0, n));
    if (x != y) {
        t.Errorf("Uvarint(%d): got %d", x, y);
    }
    if (n != m) {
        t.Errorf("Uvarint(%d): got n = %d; want %d", x, m, n);
    }

    var buf2 = slice<byte>("prefix"u8);
    buf2 = AppendUvarint(buf2, x);
    if (((@string)buf2) != "prefix"u8 + ((sstring)(buf.slice(0, n)))) {
        t.Errorf("AppendUvarint(%d): got %q, want %q", x, buf2, "prefix" + ((sstring)(buf.slice(0, n))));
    }

    (y, var err) = ReadUvarint(new binary_test_package.bytes_ReaderжByteReader(bytes.NewReader(buf)));
    if (err != nil) {
        t.Errorf("ReadUvarint(%d): %s", x, err);
    }
    if (x != y) {
        t.Errorf("ReadUvarint(%d): got %d", x, y);
    }
}

internal static slice<int64> tests = slice<int64>([
    -9223372036854775808L,
    -9223372036854775807L,
    -1,
    0,
    1,
    2,
    10,
    20,
    63,
    64,
    65,
    127,
    128,
    129,
    255,
    256,
    257,
    9223372036854775807L
]);

public static void TestVarint(ж<testing.T> t) {
    foreach (var (_, x) in tests) {
        testVarint(t, x);
        testVarint(t, -x);
    }
    for (var x = (int64)0x7; x != 0; x <<= 1) {
        testVarint(t, x);
        testVarint(t, -x);
    }
}

public static void TestUvarint(ж<testing.T> t) {
    foreach (var (_, x) in tests) {
        testUvarint(t, (uint64)x);
    }
    for (var x = (uint64)0x7; x != 0; x <<= 1) {
        testUvarint(t, x);
    }
}

public static void TestBufferTooSmall(ж<testing.T> t) {
    var buf = slice<byte>([0x80, 0x80, 0x80, 0x80]);
    for (nint i = 0; i <= len(buf); i++) {
        var bufΔ1 = buf.slice(0, i);
        var (x, n) = Uvarint(bufΔ1);
        if (x != 0 || n != 0) {
            t.Errorf("Uvarint(%v): got x = %d, n = %d", bufΔ1, x, n);
        }

        (x, var err) = ReadUvarint(new binary_test_package.bytes_ReaderжByteReader(bytes.NewReader(bufΔ1)));
        var wantErr = io.EOF;
        if (i > 0) {
            wantErr = io.ErrUnexpectedEOF;
        }
        if (x != 0 || !AreEqual(err, wantErr)) {
            t.Errorf("ReadUvarint(%v): got x = %d, err = %s", bufΔ1, x, err);
        }
    }
}

internal partial struct TestBufferTooBigWithOverflow_tests /*dyn*/ {
    internal slice<byte> @in;
    internal @string name;
    internal nint wantN;
    internal uint64 wantValue;
}

// Ensure that we catch overflows of bytes going past MaxVarintLen64.
// See issue https://golang.org/issues/41185
public static void TestBufferTooBigWithOverflow(ж<testing.T> t) {
    var tests = slice<TestBufferTooBigWithOverflow_tests>([
        new(
            name: "invalid: 1000 bytes",
            @in: func(slice<byte> () => {
                var b = new slice<byte>(1000);
                foreach (var i in range(b)) {
                    b[i] = 0xff;
                }
                b[999] = 0;
                return b;
            })(),
            wantN: -11,
            wantValue: 0
        ),
        new(
            name: "valid: math.MaxUint64-40",
            @in: slice<byte>([0xd7, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x01]),
            wantValue: math.MaxUint64 - 40,
            wantN: 10
        ),
        new(
            name: "invalid: with more than MaxVarintLen64 bytes",
            @in: slice<byte>([0xd7, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x01]),
            wantN: -11,
            wantValue: 0
        ),
        new(
            name: "invalid: 10th byte",
            @in: slice<byte>([0xd7, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x7f]),
            wantN: -10,
            wantValue: 0
        )
    ]);

    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestBufferTooBigWithOverflow_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            var (value, n) = Uvarint(ttʗ1.@in);
            {
                nint g = n;
                nint w = ttʗ1.wantN; if (g != w) {
                    tΔ1.Errorf("bytes returned=%d, want=%d", g, w);
                }
            }
            {
                var (g, w) = (value, ttʗ1.wantValue); if (g != w) {
                    tΔ1.Errorf("value=%d, want=%d", g, w);
                }
            }
        });
    }
}

internal static void testOverflow(ж<testing.T> t, slice<byte> buf, uint64 x0, nint n0, error err0) {
    var (x, n) = Uvarint(buf);
    if (x != 0 || n != n0) {
        t.Errorf("Uvarint(% X): got x = %d, n = %d; want 0, %d", buf, x, n, n0);
    }

    var r = bytes.NewReader(buf);
    nint len = r.Len();
    (x, var err) = ReadUvarint(new binary_test_package.bytes_ReaderжByteReader(r));
    if (x != x0 || !AreEqual(err, err0)) {
        t.Errorf("ReadUvarint(%v): got x = %d, err = %s; want %d, %s", buf, x, err, x0, err0);
    }
    {
        nint read = len - r.Len(); if (read > MaxVarintLen64) {
            t.Errorf("ReadUvarint(%v): read more than MaxVarintLen64 bytes, got %d", buf, read);
        }
    }
}

public static void TestOverflow(ж<testing.T> t) {
    testOverflow(t, slice<byte>([0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x2]), 0, -10, errOverflow);
    testOverflow(t, slice<byte>([0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x1, 0, 0]), 0, -11, errOverflow);
    testOverflow(t, slice<byte>([0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF]), 18446744073709551615UL, -11, errOverflow); // 11 bytes, should overflow
}

public static void TestNonCanonicalZero(ж<testing.T> t) {
    var buf = slice<byte>([0x80, 0x80, 0x80, 0]);
    var (x, n) = Uvarint(buf);
    if (x != 0 || n != 4) {
        t.Errorf("Uvarint(%v): got x = %d, n = %d; want 0, 4", buf, x, n);
    }
}

public static void BenchmarkPutUvarint32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(MaxVarintLen32);
    b.SetBytes(4);
    for (nint i = 0; i < b.N; i++) {
        for (nuint j = (nuint)0; j < MaxVarintLen32; j++) {
            PutUvarint(buf, ((uint64)1).Lsh((j * 7)));
        }
    }
}

public static void BenchmarkPutUvarint64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(MaxVarintLen64);
    b.SetBytes(8);
    for (nint i = 0; i < b.N; i++) {
        for (nuint j = (nuint)0; j < MaxVarintLen64; j++) {
            PutUvarint(buf, ((uint64)1).Lsh((j * 7)));
        }
    }
}

} // end binary_internal_test_package
