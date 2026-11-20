// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using isync = @internal.sync_package;
using @internal;
using Δsync = @internal.sync_package;
using static sync_package;

partial class sync_package {

// Map is like a Go map[any]any but is safe for concurrent use
// by multiple goroutines without additional locking or coordination.
// Loads, stores, and deletes run in amortized constant time.
//
// The Map type is specialized. Most code should use a plain Go map instead,
// with separate locking or coordination, for better type safety and to make it
// easier to maintain other invariants along with the map content.
//
// The Map type is optimized for two common use cases: (1) when the entry for a given
// key is only ever written once but read many times, as in caches that only grow,
// or (2) when multiple goroutines read, write, and overwrite entries for disjoint
// sets of keys. In these two cases, use of a Map may significantly reduce lock
// contention compared to a Go map paired with a separate [Mutex] or [RWMutex].
//
// The zero Map is empty and ready for use. A Map must not be copied after first use.
//
// In the terminology of [the Go memory model], Map arranges that a write operation
// “synchronizes before” any read operation that observes the effect of the write, where
// read and write operations are defined as follows.
// [Map.Load], [Map.LoadAndDelete], [Map.LoadOrStore], [Map.Swap], [Map.CompareAndSwap],
// and [Map.CompareAndDelete] are read operations;
// [Map.Delete], [Map.LoadAndDelete], [Map.Store], and [Map.Swap] are write operations;
// [Map.LoadOrStore] is a write operation when it returns loaded set to false;
// [Map.CompareAndSwap] is a write operation when it returns swapped set to true;
// and [Map.CompareAndDelete] is a write operation when it returns deleted set to true.
//
// [the Go memory model]: https://go.dev/ref/mem
partial struct Map {
    internal noCopy _;
    internal isync.HashTrieMap<any, any> m;
}

} partial class sync_package {

// Load returns the value stored in the map for a key, or nil if no
// value is present.
// The ok result indicates whether value was found in the map.
internal static (any value, bool ok) Loadᴍ(ж<Map> Ꮡm, any key) {
    return Ꮡm.Ꮡm.Load(key);
}

// Store sets the value for a key.
internal static void Storeᴍ(ж<Map> Ꮡm, any key, any value) {
    Ꮡm.Ꮡm.Store(key, value);
}

// Clear deletes all the entries, resulting in an empty Map.
internal static void Clearᴍ(ж<Map> Ꮡm) {
    Ꮡm.Ꮡm.Clear();
}

// LoadOrStore returns the existing value for the key if present.
// Otherwise, it stores and returns the given value.
// The loaded result is true if the value was loaded, false if stored.
internal static (any actual, bool loaded) LoadOrStoreᴍ(ж<Map> Ꮡm, any key, any value) {
    return Ꮡm.Ꮡm.LoadOrStore(key, value);
}

// LoadAndDelete deletes the value for a key, returning the previous value if any.
// The loaded result reports whether the key was present.
internal static (any value, bool loaded) LoadAndDeleteᴍ(ж<Map> Ꮡm, any key) {
    return Ꮡm.Ꮡm.LoadAndDelete(key);
}

// Delete deletes the value for a key.
// If the key is not in the map, Delete does nothing.
internal static void Deleteᴍ(ж<Map> Ꮡm, any key) {
    Ꮡm.Ꮡm.Delete(key);
}

// Swap swaps the value for a key and returns the previous value if any.
// The loaded result reports whether the key was present.
internal static (any previous, bool loaded) Swapᴍ(ж<Map> Ꮡm, any key, any value) {
    return Ꮡm.Ꮡm.Swap(key, value);
}

// CompareAndSwap swaps the old and new values for key
// if the value stored in the map is equal to old.
// The old value must be of a comparable type.
internal static bool /*swapped*/ CompareAndSwapᴍ(ж<Map> Ꮡm, any key, any old, any @new) {
    return Ꮡm.Ꮡm.CompareAndSwap(key, old, @new);
}

// CompareAndDelete deletes the entry for key if its value is equal to old.
// The old value must be of a comparable type.
//
// If there is no current value for key in the map, CompareAndDelete
// returns false (even if the old value is the nil interface value).
internal static bool /*deleted*/ CompareAndDeleteᴍ(ж<Map> Ꮡm, any key, any old) {
    return Ꮡm.Ꮡm.CompareAndDelete(key, old);
}

// Range calls f sequentially for each key and value present in the map.
// If f returns false, range stops the iteration.
//
// Range does not necessarily correspond to any consistent snapshot of the Map's
// contents: no key will be visited more than once, but if the value for any key
// is stored or deleted concurrently (including by f), Range may reflect any
// mapping for that key from any point during the Range call. Range does not
// block other methods on the receiver; even f itself may call any method on m.
//
// Range may be O(N) with the number of elements in the map even if f returns
// false after a constant number of calls.
internal static void Rangeᴍ(ж<Map> Ꮡm, Func<any, any, bool> f) {
    Ꮡm.Ꮡm.Range(f);
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class syncᴛMap {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (any value, bool ok) Load(this ж<Map> Ꮡm, any key) => sync_package.Loadᴍ(Ꮡm, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Store(this ж<Map> Ꮡm, any key, any value) => sync_package.Storeᴍ(Ꮡm, key, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Clear(this ж<Map> Ꮡm) => sync_package.Clearᴍ(Ꮡm);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (any actual, bool loaded) LoadOrStore(this ж<Map> Ꮡm, any key, any value) => sync_package.LoadOrStoreᴍ(Ꮡm, key, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (any value, bool loaded) LoadAndDelete(this ж<Map> Ꮡm, any key) => sync_package.LoadAndDeleteᴍ(Ꮡm, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Delete(this ж<Map> Ꮡm, any key) => sync_package.Deleteᴍ(Ꮡm, key);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (any previous, bool loaded) Swap(this ж<Map> Ꮡm, any key, any value) => sync_package.Swapᴍ(Ꮡm, key, value);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool /*swapped*/ CompareAndSwap(this ж<Map> Ꮡm, any key, any old, any @new) => sync_package.CompareAndSwapᴍ(Ꮡm, key, old, @new);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool /*deleted*/ CompareAndDelete(this ж<Map> Ꮡm, any key, any old) => sync_package.CompareAndDeleteᴍ(Ꮡm, key, old);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Range(this ж<Map> Ꮡm, Func<any, any, bool> f) => sync_package.Rangeᴍ(Ꮡm, f);
}
// </GlyphForwarders>
