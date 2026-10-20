// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using abi = go.@internal.abi_package;
using @unsafe = unsafe_package;
using go.@internal;
using static go.@internal.sync_package;

partial class sync_internal_test_package {

// NewBadHashTrieMap creates a new HashTrieMap for the provided key and value
// but with an intentionally bad hash function.
public static ж<global::go.@internal.sync_package.HashTrieMap<K, V>> NewBadHashTrieMap<K, V>() {
    // Stub out the good hash function with a terrible one.
    // Everything should still work as expected.
    ref var m = ref heap(new global::go.@internal.sync_package.HashTrieMap<K, V>(), out var Ꮡm);
    Ꮡm.init();
    m.keyHash = uintptr (@unsafe.Pointer _Δp0, uintptr _Δp1) => (uintptr)(0);
    return Ꮡm;
}

// NewTruncHashTrieMap creates a new HashTrieMap for the provided key and value
// but with an intentionally bad hash function.
public static ж<global::go.@internal.sync_package.HashTrieMap<K, V>> NewTruncHashTrieMap<K, V>() {
    // Stub out the good hash function with a terrible one.
    // Everything should still work as expected.
    ref var m = ref heap(new global::go.@internal.sync_package.HashTrieMap<K, V>(), out var Ꮡm);
    map<@string, nint> mx = default!;
    var mapType = abi.TypeOf(mx).MapType();
    var hasher = mapType.Value.Hasher;
    var hasherʗ1 = hasher;
    m.keyHash = (@unsafe.Pointer p, uintptr n) => (uintptr)(hasherʗ1(p, n) & (uintptr)(((uintptr)1 << 4) - 1));
    return Ꮡm;
}

} // end sync_internal_test_package
