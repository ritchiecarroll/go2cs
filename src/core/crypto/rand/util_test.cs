// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using rand = go.crypto.rand_package;
using fmt = fmt_package;
using io = io_package;
using big = math.big_package;
using mathrand = math.rand_package;
using testing = testing_package;
using time = time_package;
using go.crypto;
using math;
using static go.crypto.rand_internal_test_package;

partial class rand_test_package {

// https://golang.org/issue/6849.
public static void TestPrimeSmall(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (nint n = 2; n < 10; n++) {
        var (p, err) = rand.Prime(rand.Reader, n);
        if (err != default!) {
            Ꮡt.Fatalf("Can't generate %d-bit prime: %v"u8, n, err);
        }
        if (p.BitLen() != n) {
            Ꮡt.Fatalf("%v is not %d-bit"u8, p.OrTypedNil(), n);
        }
        if (!p.ProbablyPrime(32)) {
            Ꮡt.Fatalf("%v is not prime"u8, p.OrTypedNil());
        }
    }
}

// Test that passing bits < 2 causes Prime to return nil, error
public static void TestPrimeBitsLt2(ж<testing.T> Ꮡt) {
    {
        var (p, err) = rand.Prime(rand.Reader, 1); if (p != nil || err == default!) {
            Ꮡt.Errorf("Prime should return nil, error when called with bits < 2"u8);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object primeAlwaysGeneratedTheˢ = (@string)"Prime always generated the same prime given the same input"u8;

public static void TestPrimeNondeterministic(ж<testing.T> Ꮡt) {
    var r = mathrand.New(mathrand.NewSource(42));
    var (p0, err) = rand.Prime(new rand_test_package.rand_RandжReader(r), 32);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    for (nint i = 0; i < 128; i++) {
        r.Seed(42);
        var (p, errΔ1) = rand.Prime(new rand_test_package.rand_RandжReader(r), 32);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        if (p.Cmp(p0) != 0) {
            return;
        }
    }
    Ꮡt.Error(primeAlwaysGeneratedTheˢ);
}

public static void TestInt(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // start at 128 so the case of (max.BitLen() % 8) == 0 is covered
    for (nint n = 128; n < 140; n++) {
        var b = @new<bigꓸInt>().SetInt64((int64)n);
        {
            var (i, err) = rand.Int(rand.Reader, b); if (err != default!) {
                Ꮡt.Fatalf("Can't generate random value: %v, %v"u8, i.OrTypedNil(), err);
            }
        }
    }
}

partial struct countingReader {
    internal io.Reader r;
    internal nint n;
}

internal static (nint n, error err) Read(this ref countingReader r, slice<byte> p) {
    nint n = default!;
    error err = default!;

    (n, err) = r.r.Read(p);
    r.n += n;
    return (n, err);
}

// Test that Int reads only the necessary number of bytes from the reader for
// max at each bit length
public static void TestIntReads(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (nint iᴛ1 = 0; iᴛ1 < 32; iᴛ1++) {
        var i = iᴛ1;
        var max = (int64)(((int64)1).Lsh((uint64)i));
        Ꮡt.Run(fmt.Sprintf("max=%d"u8, max), (ж<testing.T> tΔ1) => {
            var reader = Ꮡ(new countingReader(r: rand.Reader));
            var (_, err) = rand.Int(new rand_test_package.countingReaderжReader(reader), big.NewInt(max));
            if (err != default!) {
                tΔ1.Fatalf("Can't generate random value: %d, %v"u8, max, err);
            }
            nint expected = (i + 7) / 8;
            if ((~reader).n != expected) {
                tΔ1.Errorf("Int(reader, %d) should read %d bytes, but it read: %d"u8, max, expected, (~reader).n);
            }
        });
    }
}

// Test that Int does not mask out valid return values
public static void TestIntMask(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (nint maxᴛ1 = 1; maxᴛ1 <= 256; maxᴛ1++) {
        var max = maxᴛ1;
        Ꮡt.Run(fmt.Sprintf("max=%d"u8, max), (ж<testing.T> tΔ1) => {
            for (nint i = 0; i < max; i++) {
                if (testing.Short() && i == 0) {
                    i = max - 1;
                }
                ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
                b.WriteByte((byte)i);
                var (n, err) = rand.Int(new rand_test_package.bytes_BufferжReader(Ꮡb), big.NewInt((int64)max));
                if (err != default!) {
                    tΔ1.Fatalf("Can't generate random value: %d, %v"u8, max, err);
                }
                if (n.Int64() != (int64)i) {
                    tΔ1.Errorf("Int(reader, %d) should have returned value of %d, but it returned: %v"u8, max, i, n.OrTypedNil());
                }
            }
        });
    }
}

internal static void testIntPanics(ж<testing.T> Ꮡt, ж<bigꓸInt> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var err = recover(); if (err == default!) {
                    Ꮡt.Errorf("Int should panic when called with max <= 0: %v"u8, Ꮡb.OrTypedNil());
                }
            }
        }, ref ᒐ);
        rand.Int(rand.Reader, Ꮡb);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Test that passing a new big.Int as max causes Int to panic
public static void TestIntEmptyMaxPanics(ж<testing.T> Ꮡt) {
    var b = @new<bigꓸInt>();
    testIntPanics(Ꮡt, b);
}

// Test that passing a negative value as max causes Int to panic
public static void TestIntNegativeMaxPanics(ж<testing.T> Ꮡt) {
    var b = @new<bigꓸInt>().SetInt64((int64)(-1));
    testIntPanics(Ꮡt, b);
}

public static void BenchmarkPrime(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = mathrand.New(mathrand.NewSource(time.Now().UnixNano()));
    for (nint i = 0; i < b.N; i++) {
        rand.Prime(new rand_test_package.rand_RandжReader(r), 1024);
    }
}

} // end rand_test_package
