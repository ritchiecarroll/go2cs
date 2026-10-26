// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using pem = encoding.pem_package;
using runtime = runtime_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using encoding;
using static go.crypto.tls_package;
using Δx509 = go.crypto.x509_package;

partial class tls_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToDecodeˢ = (@string)"Failed to decode certificate"u8;
internal static readonly object newCertReturnedAUniqueˢ = (@string)"newCert returned a unique reference for a duplicate certificate"u8;
internal static readonly object cacheDoesNotContainˢ = (@string)"cache does not contain expected entry"u8;
internal static readonly object timedOutWaitingForˢ = (@string)"timed out waiting for expected ref count"u8;

public static partial void TestWeakCertCache(ж<testing.T> Ꮡt) {
    ref var wcc = ref heap<global::go.crypto.tls_package.weakCertCache>(out var Ꮡwcc);
    wcc = new weakCertCache(nil);
    var (p, _) = pem.Decode(slice<byte>(rsaCertPEM));
    if (p == nil) {
        Ꮡt.Fatal(failedToDecodeˢ);
    }

    var (certA, err) = Ꮡwcc.newCert((~p).Bytes);
    if (err != default!) {
        Ꮡt.Fatalf("newCert failed: %s"u8, err);
    }
    (var certB, err) = Ꮡwcc.newCert((~p).Bytes);
    if (err != default!) {
        Ꮡt.Fatalf("newCert failed: %s"u8, err);
    }
    if (certA != certB) {
        Ꮡt.Fatal(newCertReturnedAUniqueˢ);
    }

    {
        var (_, ok) = Ꮡwcc.of(global::go.crypto.tls_package.weakCertCache.ᏑMap).Load((@string)(~p).Bytes); if (!ok) {
            Ꮡt.Fatal(cacheDoesNotContainˢ);
        }
    }

    void timeoutRefCheck(ж<testing.T> tΔ1, @string key, bool present) {
        tΔ1.Helper();
        var timeout = time_package.After((time.Duration)(4000000000L));
        while (ᐧ) {
            var selᴛ1 = timeout;
            switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out _): {
                tΔ1.Fatal(timedOutWaitingForˢ);
                break;
            }
            default: {
                var (_, ok) = Ꮡwcc.of(global::go.crypto.tls_package.weakCertCache.ᏑMap).Load(key);
                if (ok == present) {
                    return;
                }
                break;
            }}
            // Explicitly yield to the scheduler.
            //
            // On single-threaded platforms like js/wasm a busy-loop might
            // never call into the scheduler for the full timeout, meaning
            // that if we arrive here and the cleanup hasn't already run,
            // we'll simply loop until the timeout. Busy-loops put us at the
            // mercy of the Go scheduler, making this test fragile on some
            // platforms.
            runtime.Gosched();
        }
    }

    // Keep certA alive until at least now, so that we can
    // purposefully nil it and force the finalizer to be
    // called.
    runtime.KeepAlive(certA.OrTypedNil());
    certA = default!;
    runtime.GC();

    timeoutRefCheck(Ꮡt, (@string)(~p).Bytes, true);

    // Keep certB alive until at least now, so that we can
    // purposefully nil it and force the finalizer to be
    // called.
    runtime.KeepAlive(certB.OrTypedNil());
    certB = default!;
    runtime.GC();

    timeoutRefCheck(Ꮡt, (@string)(~p).Bytes, false);
}

} // end tls_internal_test_package
