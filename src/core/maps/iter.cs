// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using iter = iter_package;

partial class maps_package {

// All returns an iterator over key-value pairs from m.
// The iteration order is not specified and is not guaranteed
// to be the same from one call to the next.
public static iter.Seq2<K, V> All<Map, K, V>(Map m)
    where Map : /* ~map[K]V */ IMap<K, V>, ISupportMake<Map>, new()
{
    return [GoFunc("All[...].func1")] (Func<K, V, bool> yield) => {
        foreach (var (k, v) in m) {
            if (!yield(k, v)) {
                return;
            }
        }
    };
}

// Keys returns an iterator over keys in m.
// The iteration order is not specified and is not guaranteed
// to be the same from one call to the next.
public static iter.Seq<K> Keys<Map, K, V>(Map m)
    where Map : /* ~map[K]V */ IMap<K, V>, ISupportMake<Map>, new()
{
    return [GoFunc("Keys[...].func1")] (Func<K, bool> yield) => {
        foreach (var (k, _) in m) {
            if (!yield(k)) {
                return;
            }
        }
    };
}

// Values returns an iterator over values in m.
// The iteration order is not specified and is not guaranteed
// to be the same from one call to the next.
public static iter.Seq<V> Values<Map, K, V>(Map m)
    where Map : /* ~map[K]V */ IMap<K, V>, ISupportMake<Map>, new()
{
    return [GoFunc("Values[...].func1")] (Func<V, bool> yield) => {
        foreach (var (_, v) in m) {
            if (!yield(v)) {
                return;
            }
        }
    };
}

// Insert adds the key-value pairs from seq to m.
// If a key in seq already exists in m, its value will be overwritten.
public static void Insert<Map, K, V>(Map m, iter.Seq2<K, V> seq)
    where Map : /* ~map[K]V */ IMap<K, V>, ISupportMake<Map>, new()
{
    foreach (var (k, v) in range<K, V>(seq.Invoke)) {
        m[k] = v;
    }
}

// Collect collects key-value pairs from seq into a new map
// and returns it.
public static map<K, V> Collect<K, V>(iter.Seq2<K, V> seq) {
    var m = new map<K, V>();
    Insert(m, seq);
    return m;
}

} // end maps_package
