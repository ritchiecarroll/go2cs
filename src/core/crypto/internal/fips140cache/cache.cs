// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fips140cache provides a weak map that associates the lifetime of
// values with the lifetime of keys.
//
// It can be used to associate a precomputed value (such as an internal/fips140
// PrivateKey value, which in FIPS 140-3 mode may have required an expensive
// pairwise consistency test) with a type that doesn't have private fields (such
// as an ed25519.PrivateKey), or that can't be safely modified because it may be
// concurrently copied (such as an ecdsa.PrivateKey).
namespace go.crypto.@internal;

using runtime = runtime_package;
using sync = sync_package;
using weak = weak_package;

partial class fips140cache_package {

partial struct Cache<K, V> {
    internal sync.Map m;
}

// Get returns the result of new, for an associated key k.
//
// If Get was called with k before and didn't return an error, Get may return
// the same value it returned from the previous call if check returns true on
// it. If check returns false, Get will call new again and return the result.
//
// The cache is evicted some time after k becomes unreachable.
public static (ж<V>, error) Get<K, V>(this ж<Cache<K, V>> Ꮡc, ж<K> Ꮡk, Func<(ж<V>, error)> @new, Func<ж<V>, bool> check) {
    var p = weak.Make<K>(Ꮡk);
    {
        var (cached, ok) = Ꮡc.of(Cache<K, V>.Ꮡm).Load(p); if (ok) {
            var vΔ1 = cached._<ж<V>>();
            if (check(vΔ1)) {
                return (vΔ1, default!);
            }
        }
    }
    var (v, err) = @new();
    if (err != default!) {
        return (default!, err);
    }
    {
        var (_, present) = Ꮡc.of(Cache<K, V>.Ꮡm).Swap(p, v.OrTypedNil()); if (!present) {
            runtime.AddCleanup<K, weak.Pointer<K>>(Ꮡk, Ꮡc.evict, p);
        }
    }
    return (v, default!);
}

internal static void evict<K, V>(this ж<Cache<K, V>> Ꮡc, weak.Pointer<K> p) {
    Ꮡc.of(Cache<K, V>.Ꮡm).Delete(p);
}

} // end fips140cache_package
