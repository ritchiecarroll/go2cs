// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using flate = compress.flate_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using errors = errors_package;
using testenv = go.@internal.testenv_package;
using io = io_package;
using os = os_package;
using sync = sync_package;
using testing = testing_package;
using compress;
using exec = go.os.exec_package;
using go.@internal;
using go.crypto.@internal;
using go.os;
using static go.crypto.rand_package;

partial class rand_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string readˢ => ref readˢᵇ;
private static readonly @string readˢᵇ = "Read"u8;
internal static ref readonly @string readerReadˢ => ref readerReadˢᵇ;
private static readonly @string readerReadˢᵇ = "Reader.Read"u8;

// These tests are mostly duplicates of the tests in crypto/internal/sysrand,
// and testing both the Reader and Read is pretty redundant when one calls the
// other, but better safe than sorry.
internal static void testReadAndReader(ж<testing.T> t, Action<ж<testing.T>, Func<slice<byte>, (nint, error)>> f) {
    t.Run(readˢ, (ж<testing.T> tΔ1) => {
        f(tΔ1, go.crypto.rand_package.Read);
    });
    t.Run(readerReadˢ, (ж<testing.T> tΔ2) => {
        f(tΔ2, Reader.Read);
    });
}

public static void TestRead(ж<testing.T> t) {
    testReadAndReader(t, testRead);
}

internal static void testRead(ж<testing.T> t, Func<slice<byte>, (nint, error)> Read) {
    nint n = 4000000;
    if (testing.Short()) {
        n = 100000;
    }
    var b = new slice<byte>(n);
    (n, var err) = Read(b);
    if (n != len(b) || err != nil) {
        t.Fatalf("Read(buf) = %d, %s", n, err);
    }

    ref var z = ref heap(new bytes.Buffer(), out var Ꮡz);
    var (f, _) = flate.NewWriter(new rand_test_package.bytes_BufferжWriter(Ꮡz), 5);
    f.Write(b);
    f.Close();
    if (z.Len() < len(b) * 99 / 100) {
        t.Fatalf("Compressed %d -> %d", len(b), z.Len());
    }
}

public static void TestReadByteValues(ж<testing.T> t) {
    testReadAndReader(t, testReadByteValues);
}

internal static void testReadByteValues(ж<testing.T> t, Func<slice<byte>, (nint, error)> Read) {
    var b = new slice<byte>(1);
    var v = new map<byte, bool>();
    for (;;) {
        var (n, err) = Read(b);
        if (n != 1 || err != nil) {
            t.Fatalf("Read(b) = %d, %v", n, err);
        }
        v[b[0]] = true;
        if (len(v) == 256) {
            break;
        }
    }
}

public static void TestLargeRead(ж<testing.T> t) {
    testReadAndReader(t, testLargeRead);
}

internal static void testLargeRead(ж<testing.T> t, Func<slice<byte>, (nint, error)> Read) {
    // 40MiB, more than the documented maximum of 32Mi-1 on Linux 32-bit.
    var b = new slice<byte>((40 << 20));
    var (n, err) = Read(b); if (err != nil) {
        t.Fatal(err);
    } else if (n != len(b)) {
        t.Fatalf("Read(b) = %d, want %d", n, len(b));
    }
}

public static void TestReadEmpty(ж<testing.T> t) {
    testReadAndReader(t, testReadEmpty);
}

internal static void testReadEmpty(ж<testing.T> t, Func<slice<byte>, (nint, error)> Read) {
    var (n, err) = Read(new slice<byte>(0));
    if (n != 0 || err != nil) {
        t.Fatalf("Read(make([]byte, 0)) = %d, %v", n, err);
    }
    (n, err) = Read(default);
    if (n != 0 || err != nil) {
        t.Fatalf("Read(nil) = %d, %v", n, err);
    }
}

internal delegate (nint, error) readerFunc(slice<byte> _);

internal static (nint, error) Read(this readerFunc f, slice<byte> b) {
    return f(b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readDidNotUseReaderˢ = (@string)"Read did not use Reader"u8;

public static void TestReadUsesReader(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        bool called = default;
        defer((io.Reader r) => {
            Reader = r;
        }, Reader, ref ᒐ);
        Reader = new rand_internal_test_package.readerFuncᴠReader(new readerFunc((slice<byte> b) => {
            called = true;
            return (len(b), default);
        }));
        var (n, err) = go.crypto.rand_package.Read(new slice<byte>(32));
        if (n != 32 || err != nil) {
            t.Fatalf("Read(make([]byte, 32)) = %d, %v", n, err);
        }
        if (!called) {
            t.Error(readDidNotUseReaderˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestConcurrentRead(ж<testing.T> t) {
    testReadAndReader(t, testConcurrentRead);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;

internal static partial void testConcurrentRead(ж<testing.T> t, Func<slice<byte>, (nint, error)> Read) {
    if (testing.Short()) {
        t.Skip(skippingInShortModeˢ);
    }
    const nint N = 100;
    const nint M = 1000;
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(N);
    for (nint i = 0; i < N; i++) {
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                for (nint iΔ1 = 0; iΔ1 < M; iΔ1++) {
                    var b = new slice<byte>(32);
                    var (n, err) = Read(b);
                    if (n != 32 || err != nil) {
                        t.Errorf("Read = %d, %v", n, err);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

internal static byte sink;

public static void TestAllocations(ж<testing.T> t) {
    cryptotest.SkipTestAllocations(t);
    nint n = (nint)testing.AllocsPerRun(10, () => {
        var buf = new slice<byte>(32);
        go.crypto.rand_package.Read(buf);
        sink ^= (byte)(buf[0]);
    });
    if (n > 0) {
        t.Errorf("allocs = %d, want 0", n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestInShortModeˢ = (@string)"skipping test in short mode"u8;
internal static ref readonly @string goTestReadErrorˢ => ref goTestReadErrorˢᵇ;
private static readonly @string goTestReadErrorˢᵇ = "GO_TEST_READ_ERROR"u8;
internal static ref readonly @string errorˢ => ref errorˢᵇ;
private static readonly @string errorˢᵇ = "error"u8;
internal static readonly object readDidNotCrashˢ = (@string)"Read did not crash"u8;
internal static ref readonly @string testRunTestReadErrorˢ => ref testRunTestReadErrorˢᵇ;
private static readonly @string testRunTestReadErrorˢᵇ = "-test.run=^TestReadError$"u8;
internal static ref readonly @string testVˢ => ref testVˢᵇ;
private static readonly @string testVˢᵇ = "-test.v"u8;
internal static readonly object subprocessSucceededˢ = (@string)"subprocess succeeded unexpectedly"u8;
internal static ref readonly @string fatalErrorCryptoRandˢ => ref fatalErrorCryptoRandˢᵇ;
private static readonly @string fatalErrorCryptoRandˢᵇ = "fatal error: crypto/rand: failed to read random data"u8;

public static void TestReadError(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            t.Skip(skippingTestInShortModeˢ);
        }

        // We run this test in a subprocess because it's expected to crash.
        if (os.Getenv(goTestReadErrorˢ) == "1"u8) {
            defer((io.Reader r) => {
                Reader = r;
            }, Reader, ref ᒐ);
            Reader = new rand_internal_test_package.readerFuncᴠReader(new readerFunc((slice<byte> _) => (0, errors.New(errorˢ))));
            go.crypto.rand_package.Read(new slice<byte>(32));
            t.Error(readDidNotCrashˢ);
            return;
        }

        var cmd = testenv.Command(new rand_test_package.testing_TжTB(t), testenv.Executable(new rand_test_package.testing_TжTB(t)), testRunTestReadErrorˢ, testVˢ);
        cmd.Env = append(os.Environ(), (@string)"GO_TEST_READ_ERROR=1");
        var (@out, err) = cmd.CombinedOutput();
        if (err == nil) {
            t.Error(subprocessSucceededˢ);
        }
        @string exp = fatalErrorCryptoRandˢ;
        if (!bytes.Contains(@out, slice<byte>(exp))) {
            t.Errorf("subprocess output does not contain %q: %s", exp, @out);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkRead(ж<testing.B> b) {
    b.Run("4", (ж<testing.B> bΔ1) => {
        benchmarkRead(bΔ1, 4);
    });
    b.Run("32", (ж<testing.B> bΔ2) => {
        benchmarkRead(bΔ2, 32);
    });
    b.Run("4K", (ж<testing.B> bΔ3) => {
        benchmarkRead(bΔ3, 4 << 10);
    });
}

internal static void benchmarkRead(ж<testing.B> Ꮡb, nint size) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes((int64)size);
    var buf = new slice<byte>(size);
    for (nint i = 0; i < b.N; i++) {
        var (_, err) = go.crypto.rand_package.Read(buf); if (err != nil) {
            Ꮡb.Fatal(err);
        }
    }
}

} // end rand_internal_test_package
