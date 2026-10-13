// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using pem = encoding.pem_package;
using fmt = fmt_package;
using runtime = runtime_package;
using testing = testing_package;
using time = time_package;
using encoding;
using go.sync;
using static go.crypto.tls_package;

partial class tls_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToDecodeˢ = (@string)"Failed to decode certificate"u8;
internal static readonly object newCertReturnedAUniqueˢ = (@string)"newCert returned a unique reference for a duplicate certificate"u8;
internal static readonly object cacheDoesNotContainˢ = (@string)"cache does not contain expected entry"u8;
internal static readonly object timedOutWaitingForˢ = (@string)"timed out waiting for expected ref count"u8;
internal static readonly object cacheDoesNotContainˢ2 = (@string)"cache does not contain expected key"u8;

public static void TestCertCache(ж<testing.T> Ꮡt) {
    ref var cc = ref heap<global::go.crypto.tls_package.certCache>(out var Ꮡcc);
    cc = new certCache(nil);
    var (p, _) = pem.Decode(slice<byte>(rsaCertPEM));
    if (p == nil) {
        Ꮡt.Fatal(failedToDecodeˢ);
    }

    var (certA, err) = Ꮡcc.newCert((~p).Bytes);
    if (err != default!) {
        Ꮡt.Fatalf("newCert failed: %s"u8, err);
    }
    (var certB, err) = Ꮡcc.newCert((~p).Bytes);
    if (err != default!) {
        Ꮡt.Fatalf("newCert failed: %s"u8, err);
    }
    if ((~certA).cert != (~certB).cert) {
        Ꮡt.Fatal(newCertReturnedAUniqueˢ);
    }

    {
        var (entry, ok) = Ꮡcc.of(global::go.crypto.tls_package.certCache.ᏑMap).Load(((@string)(~p).Bytes)); if (!ok){
            Ꮡt.Fatal(cacheDoesNotContainˢ);
        } else {
            {
                var refs = entry._<ж<global::go.crypto.tls_package.cacheEntry>>().of(global::go.crypto.tls_package.cacheEntry.Ꮡrefs).Load(); if (refs != 2) {
                    Ꮡt.Fatalf("unexpected number of references: got %d, want 2"u8, refs);
                }
            }
        }
    }

    void timeoutRefCheck(ж<testing.T> tΔ1, @string key, int64 count) {
        tΔ1.Helper();
        var c = time_package.After((time.Duration)(4000000000L));
        while (ᐧ) {
            var selᴛ1 = c;
            switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out _): {
                tΔ1.Fatal(timedOutWaitingForˢ);
                break;
            }
            default: {
                var (e, ok) = Ꮡcc.of(global::go.crypto.tls_package.certCache.ᏑMap).Load(key);
                if (!ok && count != 0){
                    tΔ1.Fatal(cacheDoesNotContainˢ2);
                } else 
                if (count == 0 && !ok) {
                    return;
                }

                if (e._<ж<global::go.crypto.tls_package.cacheEntry>>().of(global::go.crypto.tls_package.cacheEntry.Ꮡrefs).Load() == count) {
                    return;
                }
                break;
            }}
        }
    }

    // Keep certA alive until at least now, so that we can
    // purposefully nil it and force the finalizer to be
    // called.
    runtime.KeepAlive(certA.OrTypedNil());
    certA = default!;
    runtime.GC();

    timeoutRefCheck(Ꮡt, ((@string)(~p).Bytes), 1);

    // Keep certB alive until at least now, so that we can
    // purposefully nil it and force the finalizer to be
    // called.
    runtime.KeepAlive(certB.OrTypedNil());
    certB = default!;
    runtime.GC();

    timeoutRefCheck(Ꮡt, ((@string)(~p).Bytes), 0);
}

public static void BenchmarkCertCache(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (p, _) = pem.Decode(slice<byte>(rsaCertPEM));
    if (p == nil) {
        Ꮡb.Fatal(failedToDecodeˢ);
    }

    ref var cc = ref heap<global::go.crypto.tls_package.certCache>(out var Ꮡcc);
    cc = new certCache(nil);
    b.ReportAllocs();
    b.ResetTimer();
    // We expect that calling newCert additional times after
    // the initial call should not cause additional allocations.
    for (nint extraᴛ1 = 0; extraᴛ1 < 4; extraᴛ1++) {
        var extra = extraᴛ1;
        var pʗ1 = p;
        Ꮡb.Run(fmt.Sprint(extra), (ж<testing.B> bΔ1) => {
            var actives = new slice<ж<global::go.crypto.tls_package.activeCert>>(extra + 1);
            bΔ1.ResetTimer();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                error err = default!;
                (actives[0], err) = Ꮡcc.newCert((~pʗ1).Bytes);
                if (err != default!) {
                    bΔ1.Fatal(err);
                }
                for (nint j = 0; j < extra; j++) {
                    (actives[j + 1], err) = Ꮡcc.newCert((~pʗ1).Bytes);
                    if (err != default!) {
                        bΔ1.Fatal(err);
                    }
                }
                for (nint j = 0; j < extra + 1; j++) {
                    actives[j] = default!;
                }
                runtime.GC();
            }
        });
    }
}

} // end tls_internal_test_package
