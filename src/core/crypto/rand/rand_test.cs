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
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using compress;
using exec = go.os.exec_package;
using go.@internal;
using go.crypto.@internal;
using go.os;
using static go.crypto.rand_package;

partial class rand_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string readˢ = "Read"u8;
internal static readonly @string readerReadˢ = "Reader.Read"u8;

// These tests are mostly duplicates of the tests in crypto/internal/sysrand,
// and testing both the Reader and Read is pretty redundant when one calls the
// other, but better safe than sorry.
internal static void testReadAndReader(ж<testing.T> Ꮡt, Action<ж<testing.T>, Func<slice<byte>, (nint, error)>> f) {
    Ꮡt.Run(readˢ, (ж<testing.T> tΔ1) => {
        f(tΔ1, go.crypto.rand_package.Read);
    });
    Ꮡt.Run(readerReadˢ, (ж<testing.T> tΔ2) => {
        f(tΔ2, Reader.Read);
    });
}

public static void TestRead(ж<testing.T> Ꮡt) {
    testReadAndReader(Ꮡt, testRead);
}

internal static void testRead(ж<testing.T> Ꮡt, Func<slice<byte>, (nint, error)> Read) {
    nint n = 4000000;
    if (testing.Short()) {
        n = 100000;
    }
    var b = new slice<byte>(n);
    (n, var err) = Read(b);
    if (n != len(b) || err != default!) {
        Ꮡt.Fatalf("Read(buf) = %d, %s"u8, n, err);
    }
    ref var z = ref heap(new bytes.Buffer(), out var Ꮡz);
    var (f, _) = flate.NewWriter(new rand_test_package.bytes_BufferжWriter(Ꮡz), 5);
    f.Write(b);
    f.Close();
    if (z.Len() < len(b) * 99 / 100) {
        Ꮡt.Fatalf("Compressed %d -> %d"u8, len(b), z.Len());
    }
}

public static void TestReadByteValues(ж<testing.T> Ꮡt) {
    testReadAndReader(Ꮡt, testReadByteValues);
}

internal static void testReadByteValues(ж<testing.T> Ꮡt, Func<slice<byte>, (nint, error)> Read) {
    var b = new slice<byte>(1);
    var v = new map<byte, bool>();
    while (ᐧ) {
        var (n, err) = Read(b);
        if (n != 1 || err != default!) {
            Ꮡt.Fatalf("Read(b) = %d, %v"u8, n, err);
        }
        v[b[0]] = true;
        if (len(v) == 256) {
            break;
        }
    }
}

public static void TestLargeRead(ж<testing.T> Ꮡt) {
    testReadAndReader(Ꮡt, testLargeRead);
}

internal static void testLargeRead(ж<testing.T> Ꮡt, Func<slice<byte>, (nint, error)> Read) {
    // 40MiB, more than the documented maximum of 32Mi-1 on Linux 32-bit.
    var b = new slice<byte>((40 << (int)(20)));
    {
        var (n, err) = Read(b); if (err != default!){
            Ꮡt.Fatal(err);
        } else 
        if (n != len(b)) {
            Ꮡt.Fatalf("Read(b) = %d, want %d"u8, n, len(b));
        }
    }
}

public static void TestReadEmpty(ж<testing.T> Ꮡt) {
    testReadAndReader(Ꮡt, testReadEmpty);
}

internal static void testReadEmpty(ж<testing.T> Ꮡt, Func<slice<byte>, (nint, error)> Read) {
    var (n, err) = Read(new slice<byte>(0));
    if (n != 0 || err != default!) {
        Ꮡt.Fatalf("Read(make([]byte, 0)) = %d, %v"u8, n, err);
    }
    (n, err) = Read(default!);
    if (n != 0 || err != default!) {
        Ꮡt.Fatalf("Read(nil) = %d, %v"u8, n, err);
    }
}

internal delegate (nint, error) readerFunc(slice<byte> _);

internal static (nint, error) Read(this readerFunc f, slice<byte> b) {
    return f(b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readDidNotUseReaderˢ = (@string)"Read did not use Reader"u8;

public static void TestReadUsesReader(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        bool called = default!;
        defer((io.Reader r) => {
            Reader = r;
        }, Reader, ref ᒐ);
        Reader = new rand_internal_test_package.readerFuncᴠReader(new readerFunc((slice<byte> b) => {
            called = true;
            return (len(b), default!);
        }));
        var (n, err) = go.crypto.rand_package.Read(new slice<byte>(32));
        if (n != 32 || err != default!) {
            Ꮡt.Fatalf("Read(make([]byte, 32)) = %d, %v"u8, n, err);
        }
        if (!called) {
            Ꮡt.Error(readDidNotUseReaderˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestConcurrentRead(ж<testing.T> Ꮡt) {
    testReadAndReader(Ꮡt, testConcurrentRead);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;

internal static partial void testConcurrentRead(ж<testing.T> Ꮡt, Func<slice<byte>, (nint, error)> Read) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
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
                    if (n != 32 || err != default!) {
                        Ꮡt.Errorf("Read = %d, %v"u8, n, err);
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

public static void TestAllocations(ж<testing.T> Ꮡt) {
    cryptotest.SkipTestAllocations(Ꮡt);
    nint n = (nint)testing.AllocsPerRun(10, () => {
        var buf = new slice<byte>(32);
        go.crypto.rand_package.Read(buf);
        sink ^= (byte)(buf[0]);
    });
    if (n > 0) {
        Ꮡt.Errorf("allocs = %d, want 0"u8, n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestInShortModeˢ = (@string)"skipping test in short mode"u8;
internal static readonly @string goTestReadErrorˢ = "GO_TEST_READ_ERROR"u8;
internal static readonly @string errorˢ = "error"u8;
internal static readonly object readDidNotCrashˢ = (@string)"Read did not crash"u8;
internal static readonly @string testRunTestReadErrorˢ = "-test.run=TestReadError"u8;
internal static readonly object subprocessSucceededˢ = (@string)"subprocess succeeded unexpectedly"u8;
internal static readonly @string fatalErrorCryptoRandˢ = "fatal error: crypto/rand: failed to read random data"u8;

public static void TestReadError(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(skippingTestInShortModeˢ);
        }
        testenv.MustHaveExec(new rand_test_package.testing_TжTB(Ꮡt));
        // We run this test in a subprocess because it's expected to crash.
        if (os.Getenv(goTestReadErrorˢ) == "1"u8) {
            defer((io.Reader r) => {
                Reader = r;
            }, Reader, ref ᒐ);
            Reader = new rand_internal_test_package.readerFuncᴠReader(new readerFunc((slice<byte> _) => (0, errors.New(errorˢ))));
            go.crypto.rand_package.Read(new slice<byte>(32));
            Ꮡt.Error(readDidNotCrashˢ);
            return;
        }
        var cmd = testenv.Command(new rand_test_package.testing_TжTB(Ꮡt), os.Args[0], testRunTestReadErrorˢ);
        cmd.Value.Env = append(os.Environ(), "GO_TEST_READ_ERROR=1"u8);
        var (@out, err) = cmd.CombinedOutput();
        if (err == default!) {
            Ꮡt.Error(subprocessSucceededˢ);
        }
        @string exp = fatalErrorCryptoRandˢ;
        if (!bytes.Contains(@out, slice<byte>(exp))) {
            Ꮡt.Errorf("subprocess output does not contain %q: %s"u8, exp, @out);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkRead(ж<testing.B> Ꮡb) {
    Ꮡb.Run("4"u8, (ж<testing.B> bΔ1) => {
        benchmarkRead(bΔ1, 4);
    });
    Ꮡb.Run("32"u8, (ж<testing.B> bΔ2) => {
        benchmarkRead(bΔ2, 32);
    });
    Ꮡb.Run("4K"u8, (ж<testing.B> bΔ3) => {
        benchmarkRead(bΔ3, (4 << (int)(10)));
    });
}

internal static void benchmarkRead(ж<testing.B> Ꮡb, nint size) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes((int64)size);
    var buf = new slice<byte>(size);
    for (nint i = 0; i < b.N; i++) {
        {
            var (_, err) = go.crypto.rand_package.Read(buf); if (err != default!) {
                Ꮡb.Fatal(err);
            }
        }
    }
}

} // end rand_internal_test_package
