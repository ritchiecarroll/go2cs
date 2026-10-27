// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using isync = @internal.sync_package;
using @unsafe = unsafe_package;
using @internal;
using Δsync = @internal.sync_package;

partial class unique_package {

internal static readonly ж<uintptr> Ꮡzero = new StaticBox<uintptr>(default(uintptr));
internal static ref uintptr zero => ref ((StaticBox<uintptr>)Ꮡzero).ValueSlot;

// Handle is a globally unique identity for some value of type T.
//
// Two handles compare equal exactly if the two values used to create the handles
// would have also compared equal. The comparison of two handles is trivial and
// typically much more efficient than comparing the values used to create them.
partial struct Handle<T> {
    internal ж<T> value;
}

// Value returns a shallow copy of the T value that produced the Handle.
// Value is safe for concurrent use by multiple goroutines.
public static T Value<T>(this Handle<T> h) {
    return h.value.ValueSlot;
}

// Make returns a globally unique handle for a value of type T. Handles
// are equal if and only if the values used to produce them are equal.
// Make is safe for concurrent use by multiple goroutines.
public static Handle<T> Make<T>(T value) {
    // Find the map for type T.
    var typ = abi.TypeFor<T>();
    if (typ.Size() == 0) {
        return new Handle<T>(Ꮡzero.Reinterpret<uintptr, T>());
    }
    var (ma, ok) = ᏑuniqueMaps.Load(typ);
    if (!ok) {
        var mΔ1 = Ꮡ(new uniqueMap<T>(canonMap: newCanonMap<T>(), cloneSeq: makeCloneSeq(typ)));
        (ma, _) = ᏑuniqueMaps.LoadOrStore(typ, mΔ1.OrTypedNil());
    }
    var m = ma._<ж<uniqueMap<T>>>();

    // Find the value in the map.
    var ptr = m.Load(value);
    if (ptr == nil) {
        // Insert a new value into the map.
        ptr = m.LoadOrStore(clone(value, m.of(uniqueMap<T>.ᏑcloneSeq)));
    }
    return new Handle<T>(ptr);
}

// uniqueMaps is an index of type-specific concurrent maps used for unique.Make.
//
// The two-level map might seem odd at first since the HashTrieMap could have "any"
// as its key type, but the issue is escape analysis. We do not want to force lookups
// to escape the argument, and using a type-specific map allows us to avoid that where
// possible (for example, for strings and plain-ol'-data structs). We also get the
// benefit of not cramming every different type into a single map, but that's certainly
// not enough to outweigh the cost of two map lookups. What is worth it though, is saving
// on those allocations.
internal static readonly ж<isync.HashTrieMap<ж<abi.Type>, any>> ᏑuniqueMaps = new StaticBox<isync.HashTrieMap<ж<abi.Type>, any>>(default(isync.HashTrieMap<ж<abi.Type>, any>));
internal static ref isync.HashTrieMap<ж<abi.Type>, any> uniqueMaps => ref ((StaticBox<isync.HashTrieMap<ж<abi.Type>, any>>)ᏑuniqueMaps).ValueSlot;                                            // any is always a *uniqueMap[T].

partial struct uniqueMap<T> {
    /*embed*/ internal ж<canonMap<T>> canonMap;
    /*embed*/ internal cloneSeq cloneSeq;
}

} // end unique_package
