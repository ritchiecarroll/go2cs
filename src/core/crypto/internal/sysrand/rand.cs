// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package rand provides cryptographically secure random bytes from the
// operating system.
namespace go.crypto.@internal;

using os = os_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using time = time_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using go.sync;

partial class sysrand_package {

internal static ж<atomic.Bool> ᏑfirstUse = new StandardBox<atomic.Bool>(default(atomic.Bool));
internal static ref atomic.Bool firstUse => ref ᏑfirstUse.Value;

internal static void warnBlocked() {
    println((@string)"crypto/rand: blocked for 60 seconds waiting to read random data from the kernel"u8);
}

// fatal is [runtime.fatal], pushed via linkname.
//
//go:linkname fatal
[global::System.Diagnostics.StackTraceHidden] internal static void fatal(@string _) {
    go.runtime_package.sysrand_fatal(_);
}

internal static bool testingOnlyFailRead;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testingSimulatedFailureˢ = "testing simulated failure"u8;

// Read fills b with cryptographically secure random bytes from the operating
// system. It always fills b entirely and crashes the program irrecoverably if
// an error is encountered. The operating system APIs are documented to never
// return an error on all but legacy Linux systems.
public static void Read(slice<byte> b) {
    GoFrame ᒐ = default;
    try {
        if (ᏑfirstUse.CompareAndSwap(false, true)) {
            // First use of randomness. Start timer to warn about
            // being blocked on entropy not being available.
            var t = time.AfterFunc(time.ΔMinute, warnBlocked);
            var tʗ1 = t;
            defer(() => tʗ1.Stop(), ref ᒐ);
        }
        {
            var err = read(b); if (err != default! || testingOnlyFailRead) {
                @string errStr = default!;
                if (!testingOnlyFailRead){
                    errStr = err.Error();
                } else {
                    errStr = testingSimulatedFailureˢ;
                }
                fatal("crypto/rand: failed to read random data (see https://go.dev/issue/66821): "u8 + errStr);
                throw panic("unreachable"); // To be sure.
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// The urandom fallback is only used on Linux kernels before 3.17 and on AIX.
internal static ж<sync.Once> ᏑurandomOnce = new StandardBox<sync.Once>(default(sync.Once));
internal static ref sync.Once urandomOnce => ref ᏑurandomOnce.Value;

internal static ж<os.File> urandomFile;

internal static error urandomErr;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string devUrandomˢ = "/dev/urandom"u8;

internal static error urandomRead(slice<byte> b) {
    ᏑurandomOnce.Do(() => {
        (urandomFile, urandomErr) = os.Open(devUrandomˢ);
    });
    if (urandomErr != default!) {
        return urandomErr;
    }
    while (len(b) > 0) {
        var (n, err) = urandomFile.Read(b);
        // Note that we don't ignore EAGAIN because it should not be possible to
        // hit for a blocking read from urandom, although there were
        // unreproducible reports of it at https://go.dev/issue/9205.
        if (err != default!) {
            return err;
        }
        b = b[(int)(n)..];
    }
    return default!;
}

} // end sysrand_package
