// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package maps defines various functions useful with maps of any type.
//
// This package does not have any special handling for non-reflexive keys
// (keys k where k != k), such as floating-point NaNs.
namespace go;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)

partial class maps_package {

// Equal reports whether two maps contain the same key/value pairs.
// Values are compared using ==.
public static bool Equal<M1, M2, K, V>(M1 m1, M2 m2)
    where M1 : /* ~map[K]V */ IMap<K, V>, ISupportMake<M1>, new()
    where M2 : /* ~map[K]V */ IMap<K, V>, ISupportMake<M2>, new()
{
    if (len(m1) != len(m2)) {
        return false;
    }
    foreach (var (k, v1) in m1) {
        {
            var (v2, ok) = m2[k, ꟷ]; if (!ok || !AreEqual(v1, v2)) {
                return false;
            }
        }
    }
    return true;
}

// EqualFunc is like Equal, but compares values using eq.
// Keys are still compared with ==.
public static bool EqualFunc<M1, M2, K, V1, V2>(M1 m1, M2 m2, Func<V1, V2, bool> eq)
    where M1 : /* ~map[K]V1 */ IMap<K, V1>, ISupportMake<M1>, new()
    where M2 : /* ~map[K]V2 */ IMap<K, V2>, ISupportMake<M2>, new()
{
    if (len(m1) != len(m2)) {
        return false;
    }
    foreach (var (k, v1) in m1) {
        {
            var (v2, ok) = m2[k, ꟷ]; if (!ok || !eq(v1, v2)) {
                return false;
            }
        }
    }
    return true;
}

// clone is implemented in the runtime package.
//
//go:linkname clone maps.clone
[global::System.Diagnostics.StackTraceHidden] internal static any clone(any m) {
    return mapclone(m);
}

// Clone returns a copy of m.  This is a shallow clone:
// the new keys and values are set using ordinary assignment.
public static M Clone<M, K, V>(M m)
    where M : /* ~map[K]V */ IMap<K, V>, ISupportMake<M>, new()
{
    // Preserve nil in case it matters.
    if (m.IsNil) {
        return default!;
    }
    return clone(m)._<M>();
}

// Copy copies all key/value pairs in src adding them to dst.
// When a key in src is already present in dst,
// the value in dst will be overwritten by the value associated
// with the key in src.
public static void Copy<M1, M2, K, V>(M1 dst, M2 src)
    where M1 : /* ~map[K]V */ IMap<K, V>, ISupportMake<M1>, new()
    where M2 : /* ~map[K]V */ IMap<K, V>, ISupportMake<M2>, new()
{
    foreach (var (k, v) in src) {
        dst[k] = v;
    }
}

// DeleteFunc deletes any key/value pairs from m for which del returns true.
public static void DeleteFunc<M, K, V>(M m, Func<K, V, bool> del)
    where M : /* ~map[K]V */ IMap<K, V>, ISupportMake<M>, new()
{
    foreach (var (k, v) in m) {
        if (del(k, v)) {
            delete(m, k);
        }
    }
}

} // end maps_package
