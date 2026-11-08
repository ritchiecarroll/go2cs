// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using bytes = bytes_package;
using flate = compress.flate_package;
using testenv = go.@internal.testenv_package;
using os = os_package;
using runtime = runtime_package;
using sync = sync_package;
using testing = testing_package;
using compress;
using exec = go.os.exec_package;
using go.@internal;
using go.os;
using io = io_package;
using static go.crypto.@internal.sysrand_package;

partial class sysrand_internal_test_package {

public static void TestRead(ж<testing.T> t) {
    // 40MiB, more than the documented maximum of 32Mi-1 on Linux 32-bit.
    var b = new slice<byte>((40 << 20));
    Read(b);

    if (testing.Short()) {
        b = b.slice(len(b) - 100_000);
    }

    ref var z = ref heap(new bytes.Buffer(), out var Ꮡz);
    var (f, _) = flate.NewWriter(new sysrand_internal_test_package.bytes_BufferжWriter(Ꮡz), 5);
    f.Write(b);
    f.Close();
    if (z.Len() < len(b) * 99 / 100) {
        t.Fatalf("Compressed %d -> %d", len(b), z.Len());
    }
}

public static void TestReadByteValues(ж<testing.T> t) {
    var b = new slice<byte>(1);
    var v = new map<byte, bool>();
    for (;;) {
        Read(b);
        v[b[0]] = true;
        if (len(v) == 256) {
            break;
        }
    }
}

public static void TestReadEmpty(ж<testing.T> t) {
    Read(new slice<byte>(0));
    Read(default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;

public static partial void TestConcurrentRead(ж<testing.T> t) {
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
                    Read(b);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goGetrandomDisabledˢ => ref goGetrandomDisabledˢᵇ;
private static readonly @string goGetrandomDisabledˢᵇ = "GO_GETRANDOM_DISABLED"u8;
internal static readonly object devUrandomFallbackUsedˢ = (@string)"/dev/urandom fallback used unexpectedly"u8;
internal static readonly object noteIfThisTestFailsItMayˢ = (@string)"note: if this test fails, it may be because the system does not have getrandom(2)"u8;
internal static readonly object devUrandomFallbackNotˢ = (@string)"/dev/urandom fallback not used as expected"u8;

// TestNoUrandomFallback ensures the urandom fallback is not reached in
// normal operations.
public static void TestNoUrandomFallback(ж<testing.T> t) {
    var expectFallback = false;
    if (runtime.GOOS == "aix"u8) {
        // AIX always uses the urandom fallback.
        expectFallback = true;
    }
    if (os.Getenv(goGetrandomDisabledˢ) == "1"u8) {
        // We are testing the urandom fallback intentionally.
        expectFallback = true;
    }
    Read(new slice<byte>(1));
    if (urandomFile != nil && !expectFallback) {
        t.Error(devUrandomFallbackUsedˢ);
        t.Log(noteIfThisTestFailsItMayˢ);
    }
    if (urandomFile == nil && expectFallback) {
        t.Error(devUrandomFallbackNotˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestInShortModeˢ = (@string)"skipping test in short mode"u8;
internal static ref readonly @string goTestReadErrorˢ => ref goTestReadErrorˢᵇ;
private static readonly @string goTestReadErrorˢᵇ = "GO_TEST_READ_ERROR"u8;
internal static readonly object readDidNotCrashˢ = (@string)"Read did not crash"u8;
internal static ref readonly @string testRunTestReadErrorˢ => ref testRunTestReadErrorˢᵇ;
private static readonly @string testRunTestReadErrorˢᵇ = "-test.run=^TestReadError$"u8;
internal static ref readonly @string testVˢ => ref testVˢᵇ;
private static readonly @string testVˢᵇ = "-test.v"u8;
internal static readonly object subprocessSucceededˢ = (@string)"subprocess succeeded unexpectedly"u8;
internal static ref readonly @string fatalErrorCryptoRandˢ => ref fatalErrorCryptoRandˢᵇ;
private static readonly @string fatalErrorCryptoRandˢᵇ = "fatal error: crypto/rand: failed to read random data"u8;

public static void TestReadError(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingTestInShortModeˢ);
    }

    // We run this test in a subprocess because it's expected to crash.
    if (os.Getenv(goTestReadErrorˢ) == "1"u8) {
        testingOnlyFailRead = true;
        Read(new slice<byte>(32));
        t.Error(readDidNotCrashˢ);
        return;
    }

    var cmd = testenv.Command(new sysrand_internal_test_package.testing_TжTB(t), testenv.Executable(new sysrand_internal_test_package.testing_TжTB(t)), testRunTestReadErrorˢ, testVˢ);
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

} // end sysrand_internal_test_package
