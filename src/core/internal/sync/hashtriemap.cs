// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted hashtriemap.go output). Go's
// HashTrieMap is a lock-free hash-trie seeded entirely from one runtime descriptor read: initSlow
// takes `abi.TypeOf(m).MapType().Hasher` — a raw function pointer into the hashing machinery the
// compiler emits for `map[K]V` — together with `Elem.Equal`, its matching bit-compare thunk. Both
// take unsafe.Pointers and mean "hash / compare the bytes AT this address".
//
// The managed reflection bridge cannot honor that contract and must not pretend to. A managed
// address names no value: two boxes holding equal strings sit at different addresses, and a pointee
// containing references moves across a GC — so an address-derived hash would stop `unique.Make(x)`
// agreeing with itself, the precise inverse of the package's purpose. Populating `Hasher` with
// something plausible-but-fake is barred by the standing rule that a descriptor field whose read
// cannot be honored must stay EMPTY: it would turn a loud construction failure into a map that is
// silently wrong. So the literal conversion compiles and can never run — the descriptor read threw
// inside the package initializer of every `unique` consumer, taking net/netip down with it.
//
// The remedy is the sync.Mutex / sync.WaitGroup precedent (mutex.cs): runtime-coupled machinery gets
// a managed-native rewrite that keeps the Go API and its concurrency contract while dropping the
// mechanism. Semantics over mechanism — nothing below is a trie. The store is a
// ConcurrentDictionary, whose guarantees line up member for member (see each method), and Go's
// keyHash/valEqual pair becomes EqualityComparer<K>/<V>.Default, which for every key shape the
// converted corpus interns IS Go's `==`:
//
//   - ж<T> — unique's own `map[*abi.Type]any` — implements IEquatable<ж<T>> as pointer IDENTITY with
//     a matching identity hash, and abi.TypeFor<T>() interns one descriptor box per System.Type, so
//     one Go type always presents one key.
//   - A [GoType] struct — net/netip's `addrDetail{isV6 bool; zoneV6 string}`, the shape unique
//     actually interns — carries a generated field-wise Equals over `==` plus a HashCode.Combine of
//     the same fields, which is Go's struct `==` exactly. It does not implement IEquatable<T>, so
//     EqualityComparer<T>.Default routes through the object override; that lands on the same
//     comparison, at the cost of a box per lookup.
//   - @string compares and hashes by CONTENT, as Go's string `==` does.
//
// ⚠ If a lookup here ever dies with EntryPointNotFoundException at IEquatable<T>.Equals, the key is
// not the type it claims to be and the defect is upstream, not in this file. The known instance:
// abi.TypeFor<T>() for an INTERFACE T returns the descriptor's `Equal` DELEGATE — Type.Elem()'s
// PtrType reinterpretation lands on the wrong member under the managed layout — and shared generics
// let that object into a ConcurrentDictionary<ж<abi.Type>, …> uncast-checked. Deliberately NOT
// defended against: tolerating a type-unsafe key is the same fake-but-plausible move the empty-Hasher
// rule forbids. The loud failure is correct; the fix belongs in internal/abi.
//
// ── Go 1.24.13 surface ────────────────────────────────────────────────────────────────────────────
// The package MOVED at 1.24 (internal/concurrent → internal/sync) and the type gained seven methods,
// lost its constructor, and widened V:
//
//   - `NewHashTrieMap` is GONE. The zero HashTrieMap is usable and `init`/`initSlow` seed it on first
//     touch, which is what `storeOf` below already did — so the constructor's departure removes a
//     member here rather than changing a design.
//   - V widened: 1.23's `HashTrieMap[K, V comparable]` became `HashTrieMap[K comparable, V any]`.
//     `valEqual` is therefore NIL for a non-comparable V, and CompareAndSwap / CompareAndDelete panic
//     UP FRONT on that — before the key is ever looked up (see mustBeStaticallyComparable). The
//     older, dynamic panic for an interface V holding a non-comparable dynamic type SURVIVES at 1.24
//     (it fires inside `valEqual`, i.e. after the key is found): BOTH panics exist, static then
//     dynamic, and both are mirrored below in that order.
//   - `keyEqual` is gone as a field; 1.24's `entry.lookup` uses K's own `==`. EqualityComparer<K>.Default
//     already was exactly that, so nothing moves.
//   - Seven methods ADDED to the managed surface: Clear, CompareAndSwap, Delete, LoadAndDelete,
//     Range, Store, Swap. All eleven public methods, and init/initSlow, take the auto's plain
//     `ж<HashTrieMap<K, V>>` receiver — the binding shape every call site already passes
//     (`Ꮡm.of(Map.Ꮡm)`, `Ꮡht.of(uniqueMap<T>.ᏑHashTrieMap)`).
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using go.golib;
using @unsafe = go.unsafe_package;

// Hand-owned native replacement of the converted hashtriemap.go output — the converter skips
// regenerating a file that carries this marker, so a -stdlib reconvert preserves it (see
// containsManualConversionMarker). hashtriemap.go is this package's ONLY Go file, so marking it makes
// the whole package hand-owned: the driver `continue`s on unmarkedFileCount == 0 and stops re-emitting
// internal.sync.csproj, package_info.cs and README.md as well (the position internal/godebug is
// in). A hashtriemap.cs.auto review sibling IS produced, since 2026-08-19: the fully-hand-owned
// branch used to panic here, and the cause was neither this file's genericity nor its reduced set of
// whole-package pre-passes — emitAutoConversionSiblings hand-rolled a copy of newFileVisitor that had
// drifted, leaving `blankImportInits` nil for visitFile to dereference. It builds through the real
// constructor now. The marker's protection is unaffected and is proven in both directions by a seeded
// reconvert.
[module: go.GoManualConversion]

namespace go.@internal;

using static sync_package;

partial class sync_package {

// HashTrieMap is an implementation of a concurrent hash-trie. The implementation
// is designed around frequent loads, but offers decent performance for stores
// and deletes as well, especially if the map is larger. Its primary use-case is
// the unique package, but can be used elsewhere as well.
//
// The zero HashTrieMap is empty and ready to use.
// It must not be copied after first use.
[GoType] public partial struct HashTrieMap<K, V>
{
    // The whole of the map's state, held behind a REFERENCE exactly as Go holds it behind the
    // `root *indirect[K, V]` pointer: a by-value copy of a HashTrieMap shares one map. Null until
    // the first touch, which is 1.24's zero-value contract — `init`/`initSlow` and every accessor
    // seed it through storeOf.
    internal mapStore<K, V> store;

    // THE HASH HOOK. Go carries `keyHash hashFunc` as a plain field and the package's own test
    // WRITES it: TestHashTrieMapBadHash — one of the suite's two top-level tests, and the parent of
    // nine of its eighteen subtests — replaces it on a freshly built map with
    // `func(unsafe.Pointer, uintptr) uintptr { return 0 }` under the comment "Stub out the good
    // hash function with a terrible one. Everything should still work as expected." The assertion is
    // therefore not about the hash at all: it is that the map's CONTRACT survives total collision.
    //
    // That contract is honored here for real, not simulated. Installing a hook rebuilds the store
    // behind an IEqualityComparer<K> whose GetHashCode returns whatever the hook returns, leaving
    // Equals as it was — which is exactly the split Go's test makes (only keyHash is replaced;
    // valEqual stays the map's own). Every key then lands in one bucket and correctness rests
    // entirely on equality and on the store's own concurrency guarantees, which is the state Go's
    // badly-hashed trie is in and the state the nine subtests measure.
    //
    // ⚠ Only ONE of the hook's two inputs can be honored, and the other is passed as the NIL pointer
    // rather than as a plausible substitute. `seed` is genuine — a real per-map salt (see
    // mapStore.seed) — but the unsafe.Pointer argument means "the key's bytes are AT this address",
    // and a managed address names no value; that is this whole file's founding premise. A hook that
    // ignores the address — the only kind that can be honored, and the kind Go's test writes — gets
    // its exact answer. A hook that dereferences it faults loudly, which is the correct outcome under
    // the same rule that keeps the descriptor's Hasher empty: a loud failure beats a silent lie.
    //
    // Note what this does NOT claim. Because the address is unavailable, every honorable hook is a
    // function of the seed alone, i.e. constant across keys — so honoring a hook and forcing total
    // collision are the same act here. Nothing is lost by that: no hook this implementation could
    // accept was ever able to distinguish two keys.
    internal Func<@unsafe.Pointer, uintptr, uintptr> keyHash
    {
        get => storeOf(ref this).hashHook!;
        set => installKeyHash(ref this, value);
    }

    // The map's hash salt. Go draws it from runtime_rand at first touch and feeds it to keyHash as
    // the second argument; here it serves the same one purpose and nothing else, since the store's
    // own hashing goes through EqualityComparer<K>.Default and takes no salt of ours.
    internal uintptr seed => storeOf(ref this).seed;
}

// The map's backing store. Named as a type of its own so that the [GoType]-generated members of
// HashTrieMap (its field-wise constructor, its field-reference accessor) never restate
// ConcurrentDictionary<K, valueCell<V>>: the converter renders Go's `comparable` constraint as
// `new()`, which carries no C# `notnull` signal, and generated code cannot carry the suppression that
// vacuous mismatch would need (a Go map key is never a null reference).
//
// ⚠ THE VALUE IS A CELL, NOT A V. Go's Swap replaces a node's value unconditionally under the node
// lock. The obvious managed spelling — `TryUpdate(key, new, previous)` in a retry loop — INVENTS a
// value comparison Go does not make, because TryUpdate compares the old value through
// EqualityComparer<V>.Default; for a V whose equality is not reflexive (a `slice<T>` that does not
// compare equal to itself) that loop never terminates. Storing a valueCell<V> makes every mutation a
// reference CAS on the cell — precisely Go's node-pointer store — because valueCell overrides
// nothing, so EqualityComparer<valueCell<V>>.Default IS reference equality, which is reflexive for
// every V. One allocation per store; Go allocates an `entry` per store too, so it is faithful, not
// wasteful. The same holder carries the nil key (below), so there is one cell type, not two.
#pragma warning disable CS8714
internal sealed class mapStore<K, V> : ConcurrentDictionary<K, valueCell<V>>
{
    // THE NIL KEY. Go admits a nil interface as a map key: `HashTrieMap[K comparable, V any]`
    // instantiated at an interface K hashes and compares nil exactly like any other value, and
    // `unique.Make[testEface](nil)` — a row of Go's own unique suite — depends on it.
    // ConcurrentDictionary refuses it outright, and refuses it EARLY: every accessor runs its own
    // `if (key is null) ThrowKeyNullException()` before the comparer is ever consulted, so no
    // IEqualityComparer<K> can rescue the key. Substituting a sentinel is not available either — a
    // sentinel has to BE a K, and no object implements an arbitrary named Go interface.
    //
    // So the nil key gets one dedicated slot beside the dictionary, holding the same valueCell<V> the
    // dictionary holds. A null cell means ABSENT and a non-null one carries the value, which puts
    // presence in the reference itself: Interlocked over that one field gives the nil key the same
    // publish/retract/exchange atomicity TryAdd/TryRemove/TryUpdate give every other key, with no
    // lock and no second dictionary. Inert for a value-type K, where null never arrives and the JIT
    // drops the branch.
    internal valueCell<V>? nilKey;

    // The salt Go's initSlow draws from runtime_rand — genuine here too, and per store. Its only
    // consumer is an installed hash hook's second argument (see HashTrieMap.keyHash): the dictionary
    // itself hashes through its comparer and never sees it.
    internal readonly uintptr seed;

    // The installed whitebox hash hook, or null in the normal case, where the store hashes through
    // EqualityComparer<K>.Default. Held so HashTrieMap.keyHash can read back what was written, and
    // so a store rebuilt for any other reason — Clear, or installKeyHash — can carry the hook across.
    internal readonly Func<@unsafe.Pointer, uintptr, uintptr>? hashHook;

    internal mapStore() : this(newSeed(), null) { }

    // A hooked store is built through hookedHash; an unhooked one passes null and gets
    // ConcurrentDictionary's default comparer, which keeps the fast path — the one unique and
    // net/netip actually run on — free of the seam entirely.
    internal mapStore(uintptr seed, Func<@unsafe.Pointer, uintptr, uintptr>? hook)
        : base(hook is null ? null : new hookedHash<K>(hook, seed))
    {
        this.seed = seed;
        this.hashHook = hook;
    }
}

// The comparer an installed hash hook is honored through. Equality is untouched — Go's test replaces
// keyHash alone — so membership still answers exactly as it did; only bucket placement moves, to
// wherever the hook says.
internal sealed class hookedHash<K> : IEqualityComparer<K>
{
    private readonly Func<@unsafe.Pointer, uintptr, uintptr> hook;
    private readonly uintptr seed;

    internal hookedHash(Func<@unsafe.Pointer, uintptr, uintptr> hook, uintptr seed)
    {
        this.hook = hook;
        this.seed = seed;
    }

    public bool Equals(K? x, K? y)
    {
        return EqualityComparer<K>.Default.Equals(x, y);
    }

    // The hook's answer IS the hash code. The width narrows — Go's uintptr is 64-bit here and a
    // .NET hash code is 32 — which loses nothing a hash cares about: a narrowing of the hash
    // function is still a hash function, and the collision behavior the test forces survives it
    // exactly.
    public int GetHashCode(K obj)
    {
        return unchecked((int)(uint)hook(noAddress, seed).Value);
    }
}

// The value holder — Go's `*entry[K, V]`, reduced to the one property the managed store needs from
// it: a REFERENCE that can be exchanged atomically and compared by identity. It serves the
// dictionary and the nil key alike (see mapStore). A plain V could not serve either role: V may be a
// value type, so for the nil key "absent" would be indistinguishable from a stored zero; and for the
// dictionary a V comparison is the non-reflexive-equality trap mapStore's header records.
internal sealed class valueCell<V>
{
    internal readonly V value;

    internal valueCell(V value)
    {
        this.value = value;
    }
}
#pragma warning restore CS8714

} partial class syncᴛHashTrieMap {

// init seeds the zero HashTrieMap on first touch. Go's split is a cheap flag read that falls through
// to a locked publish; here the same split lives in storeOf — a Volatile.Read fast path over an
// Interlocked.CompareExchange publish — so init reads and initSlow publishes, and storeOf stays the
// one place a store is created. Every accessor below calls storeOf directly, exactly as every Go
// method calls ht.init().
internal static void init<K, V>(this ж<HashTrieMap<K, V>> Ꮡht)
{
    ref HashTrieMap<K, V> ht = ref Ꮡht.Value;

    if (Volatile.Read(ref ht.store) is null)
    {
        Ꮡht.initSlow();
    }
}

internal static void initSlow<K, V>(this ж<HashTrieMap<K, V>> Ꮡht)
{
    storeOf(ref Ꮡht.Value);
}

// Load returns the value stored in the map for a key, or nil if no
// value is present.
// The ok result indicates whether value was found in the map.
public static (V value, bool ok) Load<K, V>(this ж<HashTrieMap<K, V>> Ꮡht, K key)
{
    mapStore<K, V> store = storeOf(ref Ꮡht.Value);
    // The nil key lives beside the dictionary, never in it (see mapStore.nilKey).
    if (key is null) {
        valueCell<V>? cell = Volatile.Read(ref store.nilKey);
        return cell is null ? (zeroValue<V>(), false) : (cell.value, true);
    }
    if (store.TryGetValue(key, out valueCell<V>? found)) {
        return (found!.value, true);
    }
    return (zeroValue<V>(), false);
}

// LoadOrStore returns the existing value for the key if present.
// Otherwise, it stores and returns the given value.
// The loaded result is true if the value was loaded, false if stored.
//
// Exactly one caller of a racing set observes loaded == false, which is what unique.Make relies on
// to keep one canonical value per key: TryAdd fails for every loser, and the retry then finds the
// winner's value. (GetOrAdd would be a single call but cannot report WHICH outcome occurred.)
public static (V result, bool loaded) LoadOrStore<K, V>(this ж<HashTrieMap<K, V>> Ꮡht, K key, V value)
{
    mapStore<K, V> store = storeOf(ref Ꮡht.Value);
    // The nil key's one-shot publish is a single CAS, and it reports the same winner/loser split the
    // TryAdd loop below does: the thread whose exchange observed no cell stored, everyone else
    // loaded (see mapStore.nilKey).
    if (key is null) {
        valueCell<V> candidate = new(value);
        valueCell<V>? existingCell = Interlocked.CompareExchange(ref store.nilKey, candidate, null);
        return existingCell is null ? (value, false) : (existingCell.value, true);
    }
    valueCell<V> fresh = new(value);
    while (ᐧ) {
        if (store.TryGetValue(key, out valueCell<V>? existing)) {
            return (existing!.value, true);
        }
        if (store.TryAdd(key, fresh)) {
            return (value, false);
        }
    }
}

// Store sets the value for a key.
//
// Upstream really does name the second parameter `old` — Store is one line, `_, _ = ht.Swap(key, old)`,
// and the name came along from Swap's signature. It is positional at every call site; kept verbatim
// so the hand-own and the .auto review sibling read the same.
public static void Store<K, V>(this ж<HashTrieMap<K, V>> Ꮡht, K key, V old)
{
    _ = Ꮡht.Swap(key, old);
}

// Swap swaps the value for a key and returns the previous value if any.
// The loaded result reports whether the key was present.
//
// Go replaces the node's value unconditionally under the node lock, making NO comparison of values.
// Every mutation here is therefore a reference CAS on the cell, never a V comparison — see the
// valueCell note on mapStore for why TryUpdate over a bare V does not terminate for a non-reflexive V.
public static (V previous, bool loaded) Swap<K, V>(this ж<HashTrieMap<K, V>> Ꮡht, K key, V @new)
{
    mapStore<K, V> store = storeOf(ref Ꮡht.Value);
    valueCell<V> replacement = new(@new);
    // Unconditional publish, so the nil key's slot takes a plain exchange rather than a compare (see
    // mapStore.nilKey): whatever cell was there is the previous value, and null means absent.
    if (key is null) {
        valueCell<V>? prior = Interlocked.Exchange(ref store.nilKey, replacement);
        return prior is null ? (zeroValue<V>(), false) : (prior.value, true);
    }
    while (ᐧ) {
        if (store.TryGetValue(key, out valueCell<V>? prior)) {
            // TryUpdate's comparison is against the CELL, i.e. reference identity, which is Go's
            // "is this still the node I read?" and nothing more. A lost race means the cell moved;
            // re-read and try again, exactly as Go re-takes the node lock and re-checks the slot.
            if (store.TryUpdate(key, replacement, prior!)) {
                return (prior!.value, true);
            }
            continue;
        }
        if (store.TryAdd(key, replacement)) {
            return (zeroValue<V>(), false);
        }
    }
}

// CompareAndSwap swaps the old and new values for key
// if the value stored in the map is equal to old.
// The value type must be of a comparable type, otherwise CompareAndSwap will panic.
public static bool /*swapped*/ CompareAndSwap<K, V>(this ж<HashTrieMap<K, V>> Ꮡht, K key, V old, V @new)
{
    mapStore<K, V> store = storeOf(ref Ꮡht.Value);
    // 1.24's STATIC panic: Go reads `valEqual == nil` straight after init, before the key is hashed
    // or looked up (see mustBeStaticallyComparable). The dynamic panic below is the second of the two.
    mustBeStaticallyComparable<V>("CompareAndSwap");
    valueCell<V> replacement = new(@new);
    // The nil key's conditional replace, spelled against its own slot: the CAS succeeds only if the
    // cell we compared is still the published one, so a racing Swap cannot have its value replaced
    // out from under it (see mapStore.nilKey).
    if (key is null) {
        valueCell<V>? prior = Volatile.Read(ref store.nilKey);
        if (prior is null) {
            return false;
        }
        mustBeComparable(old);
        return EqualityComparer<V>.Default.Equals(prior.value, old) &&
               ReferenceEquals(Interlocked.CompareExchange(ref store.nilKey, replacement, prior), prior);
    }
    while (ᐧ) {
        // Go reaches its value comparison only once the key is found, and only then can that
        // comparison panic — mirror both the order and the panic (see mustBeComparable).
        if (!store.TryGetValue(key, out valueCell<V>? prior)) {
            return false;
        }
        mustBeComparable(old);
        if (!EqualityComparer<V>.Default.Equals(prior!.value, old)) {
            return false;
        }
        // Identity CAS again: the values compared equal, so the only question left is whether the
        // cell is still the one we compared. Go answers the same question by re-reading the slot
        // under the node lock, and reports false only when the node is gone — a cell that MOVED is
        // a retry there, not a false.
        if (store.TryUpdate(key, replacement, prior!)) {
            return true;
        }
    }
}

// LoadAndDelete deletes the value for a key, returning the previous value if any.
// The loaded result reports whether the key was present.
public static (V value, bool loaded) LoadAndDelete<K, V>(this ж<HashTrieMap<K, V>> Ꮡht, K key)
{
    mapStore<K, V> store = storeOf(ref Ꮡht.Value);
    // An unconditional retraction, so the nil key's slot takes a plain exchange to null: the cell it
    // returns is the value, and null means the key was absent (see mapStore.nilKey).
    if (key is null) {
        valueCell<V>? prior = Interlocked.Exchange(ref store.nilKey, null);
        return prior is null ? (zeroValue<V>(), false) : (prior.value, true);
    }
    // TryRemove's atomic remove-and-report IS Go's "delete the entry and return what was there":
    // exactly one racing caller observes loaded == true, which is the guarantee unique's cleanup
    // pass and sync.Map's LoadAndDelete both rest on.
    if (store.TryRemove(key, out valueCell<V>? removed)) {
        return (removed!.value, true);
    }
    return (zeroValue<V>(), false);
}

// Delete deletes the value for a key.
public static void Delete<K, V>(this ж<HashTrieMap<K, V>> Ꮡht, K key)
{
    _ = Ꮡht.LoadAndDelete(key);
}

// CompareAndDelete deletes the entry for key if its value is equal to old.
// The value type must be comparable, otherwise this CompareAndDelete will panic.
//
// If there is no current value for key in the map, CompareAndDelete returns false
// (even if the old value is the nil interface value).
public static bool /*deleted*/ CompareAndDelete<K, V>(this ж<HashTrieMap<K, V>> Ꮡht, K key, V old)
{
    mapStore<K, V> store = storeOf(ref Ꮡht.Value);
    // 1.24's STATIC panic, before the lookup — the same pair of panics CompareAndSwap carries.
    mustBeStaticallyComparable<V>("CompareAndDelete");
    // The nil key's retraction is the same compare-and-remove, spelled against its own slot: the CAS
    // succeeds only if the cell we compared is still the published one, so a racing LoadOrStore
    // cannot have its value deleted out from under it (see mapStore.nilKey). Key-found-first and the
    // panic order below hold here too.
    if (key is null) {
        valueCell<V>? cell = Volatile.Read(ref store.nilKey);
        if (cell is null) {
            return false;
        }
        mustBeComparable(old);
        return EqualityComparer<V>.Default.Equals(cell.value, old) &&
               ReferenceEquals(Interlocked.CompareExchange(ref store.nilKey, null, cell), cell);
    }
    // Go reaches its value comparison only once the key is found, and only then can that comparison
    // panic — mirror both the order and the panic (see mustBeComparable).
    if (!store.TryGetValue(key, out valueCell<V>? current)) {
        return false;
    }
    mustBeComparable(old);
    // Atomic compare-and-remove: the pair overload removes only if the stored CELL is still the one
    // just compared, so the V comparison above is made exactly once and by us — never a second time
    // by EqualityComparer<V> inside the dictionary.
    return EqualityComparer<V>.Default.Equals(current!.value, old) &&
           store.TryRemove(new KeyValuePair<K, valueCell<V>>(key, current!));
}

// All returns an iter.Seq2 that produces all key-value pairs in the map.
// The enumeration does not represent any consistent snapshot of the map,
// but is guaranteed to visit each unique key-value pair only once. It is
// safe to operate on the tree during iteration. No particular enumeration
// order is guaranteed.
//
// ConcurrentDictionary's enumerator satisfies all three: it is weakly consistent (never throws on
// concurrent mutation, so unique's cleanup pass can CompareAndDelete while it walks), it yields each
// live key once, and it promises no order.
public static Action<Func<K, V, bool>> All<K, V>(this ж<HashTrieMap<K, V>> Ꮡht)
{
    mapStore<K, V> store = storeOf(ref Ꮡht.Value);
    return (Func<K, V, bool> yield) => {
        // The nil key is not in the dictionary, so the enumeration below cannot reach it — yield it
        // first, under the key it actually has (see mapStore.nilKey). Reading the cell once keeps
        // the walk weakly consistent in the same way the dictionary's own enumerator is: a
        // concurrently published nil entry may or may not be seen, and one deleted mid-walk is
        // yielded at most once. unique's cleanup pass relies on visiting it at all — without this,
        // a dead weak pointer under the nil key could never be reclaimed.
        valueCell<V>? cell = Volatile.Read(ref store.nilKey);
        if (cell is not null && !yield(default!, cell.value)) {
            return;
        }
        foreach (KeyValuePair<K, valueCell<V>> pair in store) {
            if (!yield(pair.Key, pair.Value.value)) {
                return;
            }
        }
    };
}

// Range calls f sequentially for each key and value present in the map.
// If f returns false, range stops the iteration.
//
// This exists for compatibility with sync.Map; All should be preferred.
// It provides the same guarantees as sync.Map, and All.
//
// Go states that relationship and then spells both bodies as one call to `ht.iter`. Here All IS the
// iteration, so Range runs it — the two cannot drift.
public static void Range<K, V>(this ж<HashTrieMap<K, V>> Ꮡht, Func<K, V, bool> yield)
{
    Ꮡht.All()(yield);
}

// Clear deletes all the entries, resulting in an empty HashTrieMap.
//
// Go drops the ROOT on the floor and keeps keyHash and seed. The managed match is a FRESH mapStore
// carrying the current seed and hook — not ConcurrentDictionary.Clear(), which would keep the store
// (and so keep any entry a racing writer is mid-insert on) and would leave the nil key's slot
// untouched. Publishing a new store retracts both in one reference write, which is exactly the
// atomicity of Go's single root Store.
public static void Clear<K, V>(this ж<HashTrieMap<K, V>> Ꮡht)
{
    ref HashTrieMap<K, V> ht = ref Ꮡht.Value;
    mapStore<K, V> current = storeOf(ref ht);

    Volatile.Write(ref ht.store, new mapStore<K, V>(current.seed, current.hashHook));
}

} partial class sync_package {

// storeOf returns the map's backing store, creating it once on first use (race-safe). This IS 1.24's
// init/initSlow: the zero HashTrieMap is documented empty and ready to use, the flag read is the
// Volatile.Read below and the locked publish is the CompareExchange, and a loser of that race adopts
// the winner's store rather than its own. Same idiom, and same reasoning, as sync.Mutex's gateOf.
internal static mapStore<K, V> storeOf<K, V>(ref HashTrieMap<K, V> ht)
{
    mapStore<K, V>? store = Volatile.Read(ref ht.store);

    if (store is not null) {
        return store;
    }

    mapStore<K, V> created = new();

    return Interlocked.CompareExchange(ref ht.store, created, null) ?? created;
}

// The zero V, spelled once. `@new<V>().ValueSlot` is the corpus idiom for Go's `*new(V)`, but it
// cannot be written inside Swap or CompareAndSwap, whose Go-named `@new` parameter takes that
// identifier; naming it here keeps ONE spelling of the zero in every method rather than two.
internal static V zeroValue<V>()
{
    return @new<V>().ValueSlot;
}

// The "no address" the hash hook is handed in place of Go's `unsafe.Pointer(&key)` — the nil
// pointer, stated once. Go passes the address of the key's bytes; there is no such address here, and
// this file's whole argument is that inventing one would be worse than admitting there is none (see
// HashTrieMap.keyHash).
private static readonly @unsafe.Pointer noAddress = new(nil);

// A real per-store salt. The hook's second argument is the one half of its contract that CAN be
// honored, so it is honored with a genuine random value rather than a constant.
private static uintptr newSeed()
{
    return new uintptr(unchecked((nuint)System.Random.Shared.NextInt64()));
}

// Installing a hook REBUILDS the store behind it. ConcurrentDictionary fixes its comparer at
// construction and caches each entry's hash code in its nodes, so swapping the hash of a live
// dictionary in place would strand every entry already in it. Go's plain field write has no such
// constraint — its next Load simply hashes differently — so the entries are carried across here to
// reach the same end state. TestHashTrieMapBadHash writes the hook on an empty map, where the copy
// moves nothing; a populated map is handled anyway rather than left as a trap.
//
// The publish is a single reference write, so a concurrent reader sees either the whole old store or
// the whole new one. It is NOT atomic with respect to the copy — an entry stored by another thread
// mid-rebuild can be lost — and Go's field write is no better: both expect the hook to be installed
// before the map is shared, which is what the test does.
private static void installKeyHash<K, V>(ref HashTrieMap<K, V> ht, Func<@unsafe.Pointer, uintptr, uintptr> hook)
{
    mapStore<K, V> current = storeOf(ref ht);
    mapStore<K, V> replacement = new(current.seed, hook);

    replacement.nilKey = Volatile.Read(ref current.nilKey);

    foreach (KeyValuePair<K, valueCell<V>> pair in current) {
        replacement.TryAdd(pair.Key, pair.Value);
    }

    Volatile.Write(ref ht.store, replacement);
}

// 1.24's FIRST panic, and the new one: V widened to `any`, so `mapType.Elem.Equal` — the thunk Go
// calls valEqual — is NIL for a V that is not comparable at all, and CompareAndSwap / CompareAndDelete
// refuse UP FRONT, before the key is hashed or looked up. GoReflect.IsComparable is the managed
// reading of Go's rule (slices, maps, funcs and anything transitively containing one are not
// comparable; everything else, `any` included, is) and it is the same definition abi.Type.Equal is
// populated from, so this answers exactly what Go's nil check answers. Resolved once per
// instantiation, not per call — this sits in front of every CompareAndSwap on the hot path.
internal static void mustBeStaticallyComparable<V>(string caller)
{
    if (staticallyComparable<V>.applies) {
        return;
    }
    throw panic((@string)$"called {caller} when value is not of comparable type");
}

// Whether V is comparable at all, resolved once per instantiation. A static readonly field on a
// generic type is initialized once per closed generic, which is exactly "per instantiation".
private static class staticallyComparable<V> {
    internal static readonly bool applies = GoReflect.IsComparable(typeof(V));
}

// 1.24's SECOND panic, and the older one. Go compares the two values with V's own `==`, and for an
// INTERFACE V that comparison panics when the dynamic type is not comparable — the static check
// above admits `any`, which moves the question to run time. Mirror the panic rather than letting
// EqualityComparer<V>.Default answer a question Go refuses to answer. Inert for every other V: a
// non-interface V that got past the static check was comparable at the Go type check too.
internal static void mustBeComparable<V>(V value) {
    if (!dynamicallyComparable<V>.applies || value is null) {
        return;
    }
    Type dynamicType = value.GetType();
    if (!GoReflect.IsComparable(dynamicType)) {
        throw RuntimeErrorPanic.ComparingUncomparableType(GoReflect.GoTypeName(dynamicType)); // Go's runtime error, raised from runtime.efaceeq
    }
}

// Whether V's comparability is a run-time question at all, resolved once per instantiation. Only an
// interface-typed V is: `any` converts to object, a named Go interface to a C# interface.
private static class dynamicallyComparable<V> {
    internal static readonly bool applies = typeof(V).IsInterface || typeof(V) == typeof(object);
}

} // end sync_package
