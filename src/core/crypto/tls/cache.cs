// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using Δx509 = go.crypto.x509_package;
using runtime = runtime_package;
using sync = sync_package;
using weak = weak_package;
using go.crypto;

partial class tls_package {

// weakCertCache provides a cache of *x509.Certificates, allowing multiple
// connections to reuse parsed certificates, instead of re-parsing the
// certificate for every connection, which is an expensive operation.
partial struct weakCertCache {
    /*embed*/ public sync_package.Map Map;
}

internal static (ж<Δx509.Certificate>, error) newCert(this ж<weakCertCache> Ꮡwcc, slice<byte> der) {
    {
        var (entry, ok) = Ꮡwcc.of(weakCertCache.ᏑMap).Load((@string)der); if (ok) {
            {
                var v = entry._<weak.Pointer<Δx509.Certificate>>().Value(); if (v != nil) {
                    return (v, default!);
                }
            }
        }
    }

    var (cert, err) = Δx509.ParseCertificate(der);
    if (err != default!) {
        return (default!, err);
    }

    ref var wp = ref heap<weak.Pointer<Δx509.Certificate>>(out var Ꮡwp);
    wp = weak.Make<Δx509.Certificate>(cert);
    {
        var (entry, loaded) = Ꮡwcc.of(weakCertCache.ᏑMap).LoadOrStore((@string)der, wp); if (!loaded){
            var derʗ1 = der;
            var entryʗ1 = entry;
            runtime.AddCleanup(cert, (any _) => {
                Ꮡwcc.of(weakCertCache.ᏑMap).CompareAndDelete((@string)derʗ1, entryʗ1);
            }, (any)((@string)der));
        } else 
        {
            var v = entry._<weak.Pointer<Δx509.Certificate>>().Value(); if (v != nil){
                return (v, default!);
            } else {
                if (Ꮡwcc.of(weakCertCache.ᏑMap).CompareAndSwap((@string)der, entry, wp)) {
                    var derʗ2 = der;
                    var wpʗ1 = wp;
                    runtime.AddCleanup(cert, (any _) => {
                        Ꮡwcc.of(weakCertCache.ᏑMap).CompareAndDelete((@string)derʗ2, wpʗ1);
                    }, (any)((@string)der));
                }
            }
        }
    }
    return (cert, default!);
}

internal static ж<weakCertCache> globalCertCache = @new<weakCertCache>();

} // end tls_package
