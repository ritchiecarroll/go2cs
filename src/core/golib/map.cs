// map.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using go.golib;

namespace go;

public interface IMap
{
    nint Length { get; }

    /// <summary>
    /// Gets a flag indicating whether the map is nil — no backing store; an empty but
    /// allocated map is NOT nil (Go's only legal map comparison is against nil).
    /// </summary>
    bool IsNil { get; }

    /// <summary>
    /// Gets the map's NIL-KEY entry: <c>(false, null)</c> when the key type cannot be nil or no
    /// nil-key entry is present, otherwise <c>(true, value)</c> with the value boxed.
    /// </summary>
    /// <remarks>
    /// Go's <c>range</c> visits a nil key like any other, but the nil entry cannot live in the
    /// backing <see cref="Dictionary{TKey, TValue}"/> (which rejects a null key), so it occupies a
    /// dedicated slot. This member is how a NON-generic consumer — the reflection bridge's
    /// <c>DeepEqual</c>, which walks the backing <see cref="IDictionary"/> directly — observes the
    /// entry that walk cannot see.
    /// </remarks>
    (bool present, object? value) NilKeyEntry { get; }

    /// <summary>
    /// Returns a shallow clone of this map with an INDEPENDENT backing store — the runtime
    /// intrinsic behind <c>maps.Clone</c> (Go's <c>runtime.mapclone</c>). Keys and values are
    /// copied by ordinary assignment; mutating the clone does not affect the original. A nil
    /// map clones to a nil map.
    /// </summary>
    IMap CloneMap();

    /// <summary>
    /// Removes every entry, the way Go's <c>clear(m)</c> and <c>reflect.Value.Clear</c> do. A nil
    /// map clears to a nil map without panicking, matching Go — <c>clear</c> on a nil map is a no-op.
    /// </summary>
    /// <remarks>
    /// Declared on the NON-generic interface because the callers that need it hold a map whose key
    /// and value types are not known statically: <c>reflect.Value.Clear</c> reaches an arbitrary
    /// map through a boxed <c>object</c>, and the generic <c>IDictionary{TKey, TValue}.Clear</c> is
    /// unreachable from there without reflection. <c>map{TKey, TValue}</c> already declares a
    /// matching public <c>Clear()</c>, so it satisfies this with no change of its own.
    /// </remarks>
    void Clear();
}

public interface IMap<TKey, TValue> : IMap, IDictionary<TKey, TValue> where TKey : notnull
{
    (TValue, bool) this[TKey key, bool _] { get; }

    /// <summary>
    /// Reads an element whose Go zero value carries run-time SHAPE its C# type does not, taking
    /// that zero from the CALL SITE. See <see cref="map{TKey, TValue}.this[TKey, Func{TValue}]"/>,
    /// which is where the reasoning lives; this is the surface a map-cored TYPE PARAMETER reaches.
    /// </summary>
    TValue this[TKey key, Func<TValue> zero] => TryGetValue(key, out TValue? value) ? value : zero();

    /// <summary>
    /// Comma-ok form of the shaped-zero read, for a map-cored type parameter. See
    /// <see cref="map{TKey, TValue}.this[TKey, Func{TValue}, bool]"/>.
    /// </summary>
    (TValue, bool) this[TKey key, Func<TValue> zero, bool _] => TryGetValue(key, out TValue? value) ? (value!, true) : (zero(), false);

    /// <summary>
    /// Default <see cref="IMap.CloneMap"/> for any <see cref="IMap{TKey, TValue}"/> — covers both
    /// the concrete <see cref="map{TKey, TValue}"/> and the generated named-map wrappers (which
    /// wrap a shared <see cref="map{TKey, TValue}"/>). Builds a fresh <see cref="map{TKey, TValue}"/>
    /// populated from this map's entries, so the clone's backing store is independent of the
    /// original (Go's shallow clone: keys/values set by ordinary assignment). A nil map clones to nil.
    /// </summary>
    IMap IMap.CloneMap()
    {
        return IsNil ? default(map<TKey, TValue>) : new map<TKey, TValue>(this);
    }

    /// <summary>
    /// Default <see cref="IMap.Clear"/> for any <see cref="IMap{TKey, TValue}"/> — Go's
    /// <c>clear(m)</c>, forwarded to the storage this map already exposes as an
    /// <see cref="ICollection{T}"/> of entries, which both the concrete
    /// <see cref="map{TKey, TValue}"/> and the generated named-map wrappers implement.
    /// </summary>
    /// <remarks>
    /// A DEFAULT rather than a plain interface member, for the same reason
    /// <see cref="IMap.CloneMap"/> and <see cref="IMap.NilKeyEntry"/> are: every named Go map type
    /// in the corpus becomes a GENERATED wrapper implementing this interface
    /// (go2cs-gen's InheritedTypeTemplate), and none of them declares a Clear of its own. Requiring
    /// one is a breaking change that does not surface at compile time in this repository — the
    /// wrappers live in already-built package assemblies, so they fail to LOAD instead, and the
    /// symptom is a ReflectionTypeLoadException from the extension-method scan that reads like an
    /// unrelated bridge fault. Measured exactly that way: 19 reflect tests went pass -&gt;
    /// infrastructure-error on a merge, all of them inside GetGoMethodSetCandidates.
    ///
    /// Being an EXPLICIT implementation also keeps <c>Clear()</c> unambiguous through an
    /// <c>IMap{TKey, TValue}</c> reference: it is not a candidate there, so the inherited
    /// <c>ICollection</c> member wins on its own and <c>builtin.clear</c> still binds.
    /// </remarks>
    void IMap.Clear()
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)this).Clear();
    }

    /// <summary>
    /// Default <see cref="IMap.NilKeyEntry"/> for any <see cref="IMap{TKey, TValue}"/> — asks the
    /// comma-ok indexer about the nil key, which both the concrete map and the generated named-map
    /// wrappers already answer. A value-type key can never BE nil, and that test is a JIT-time
    /// constant, so those instantiations fold to a constant <c>(false, null)</c>.
    /// </summary>
    (bool present, object? value) IMap.NilKeyEntry
    {
        get
        {
            if (typeof(TKey).IsValueType)
                return (false, null);

            (TValue value, bool present) = this[default!, true];

            return (present, value);
        }
    }
}

public readonly struct map<TKey, TValue> : IMap<TKey, TValue>, ISupportMake<map<TKey, TValue>> where TKey : notnull
{
    /// <summary>
    /// The backing store — a <see cref="Dictionary{TKey, TValue}"/> that additionally carries Go's
    /// NIL-KEY slot. Go's map accepts a nil key whenever the key type can be nil (<c>map[any]V</c>,
    /// <c>map[error]V</c>, <c>map[*T]V</c>, a named-interface key…), while
    /// <see cref="Dictionary{TKey, TValue}"/> rejects one with an
    /// <see cref="System.ArgumentNullException"/> before the comparer is ever consulted — so the nil
    /// entry gets a slot of its own instead of a bucket.
    /// </summary>
    /// <remarks>
    /// The slot belongs to the STORE, not to this struct. A Go map is a reference type: every copy
    /// of a <c>map&lt;TKey, TValue&gt;</c> value must observe the same nil entry, and a field on the
    /// struct would make a write through one copy invisible through another. DERIVING from
    /// Dictionary (rather than wrapping it) also keeps the struct exactly one reference wide — no
    /// extra allocation, no widened value — and leaves every existing Dictionary interop path (the
    /// implicit conversions, <c>Keys</c>/<c>Values</c>, the <see cref="ICollection{T}"/> casts, the
    /// reflection bridge's backing-field probe) binding exactly as before.
    /// </remarks>
    private sealed class NilKeyDictionary : Dictionary<TKey, TValue>
    {
        public NilKeyDictionary() : base(s_comparer)
        {
        }

        public NilKeyDictionary(int capacity) : base(capacity, s_comparer)
        {
        }

        public NilKeyDictionary(IDictionary<TKey, TValue> source) : base(source, s_comparer)
        {
        }

        public NilKeyDictionary(IEnumerable<KeyValuePair<TKey, TValue>> source) : base(source, s_comparer)
        {
        }

        /// <summary>Gets or sets a flag indicating whether an entry is stored under the nil key.</summary>
        public bool HasNilKey;

        /// <summary>Gets or sets the value stored under the nil key (meaningful only when <see cref="HasNilKey"/>).</summary>
        public TValue NilKeyValue = default!;

        /// <summary>
        /// Counts the overwrites that REPLACED a stored key (a signed zero; see setReplacingKey). A
        /// range over a store where this is non-zero re-reads the stored key of a zero-bearing entry,
        /// since a key it snapshotted can have been replaced underneath it.
        /// </summary>
        public int KeyEpoch;
    }

    // Go compares interface KEYS by (dynamic type, dynamic value) — the same relation `==` uses — but
    // a converted interface value is presented through whichever generated adapter its current static
    // interface calls for, so Dictionary's default comparer (wrapper identity) makes the SAME Go key
    // unfindable after a type assertion. Null for every other key type, which keeps
    // EqualityComparer<TKey>.Default's fast path. See GoEqualityComparer.
    private static readonly IEqualityComparer<TKey>? s_comparer = GoEqualityComparer.ForKeys<TKey>();

    // Go's map hashes an INTERFACE key's dynamic value before it touches the table, so an
    // unhashable one (a slice, map or func) panics on every operation -- including on an empty or nil
    // map, where Dictionary never hashes. A per-instantiation constant: no other key type pays.
    private static readonly bool s_hashMayPanic = typeof(TKey).IsInterface || typeof(TKey) == typeof(object);

    // Go REPLACES the stored key on an overwrite when == admits distinguishable keys (NeedKeyUpdate),
    // and for a Go program the observable case is a signed zero: `m[+0] = v; m[-0] = v` keeps -0.
    // Dictionary keeps the FIRST key and has no way to replace one. A per-instantiation constant.
    private static readonly bool s_keyMayNeedUpdate = GoEqualityComparer.MayHoldSignedZero(typeof(TKey));

    // A Go map KEY is a value, so storing one copies it. golib's array<T> (and any value type carrying
    // one -- IGoValueClone) is a struct over a SHARED T[] backing, so a stored key that is the caller's
    // array ALIASES it and moves with the caller's later writes: runtime's TestBigItems mutates key[37]
    // between inserts of `m[key] = key` and every stored key followed the buffer ("missing key"). The
    // predicate is array<T>.s_elementNeedsDeepCopy's, the one definition of "a Go copy must re-copy this":
    // an array kind that is not a slice (a slice key cannot exist in Go), or a value-clone struct. An
    // INTERFACE key decides per value (keyNeedsClone). A per-instantiation constant otherwise.
    private static readonly bool s_keyNeedsClone =
        (typeof(IArray).IsAssignableFrom(typeof(TKey)) && !typeof(ISlice).IsAssignableFrom(typeof(TKey))) ||
        typeof(IGoValueClone).IsAssignableFrom(typeof(TKey));

    private static readonly bool s_keyMayHoldClonable = typeof(TKey).IsInterface || typeof(TKey) == typeof(object);

    private readonly NilKeyDictionary m_map;

    // Each of these charges ONE object: the store itself. The bucket and entry arrays Dictionary
    // allocates behind its own constructor are BCL-internal and uncharged by policy — Go's own
    // make(map) allocates an hmap plus its buckets, so the two runtimes differ here by exactly the
    // structures neither exposes. See AllocationCounter's coverage statement.
    public map()
    {
        m_map = new NilKeyDictionary();
        AllocationCounter.Count();
    }

    // A size hint is ADVICE, never an error, in Go: runtime.makemap clamps `hint < 0` to 0
    // (map_swiss.go:72) and makemap64 clamps a hint that does not fit an int to 0 (:33), so
    // `make(map[int]int, n)` with a negative or oversized runtime n yields an empty map. Dictionary's
    // capacity argument throws on a negative value instead, which turned internal/runtime/maps'
    // TestTableGroupCount/makemap*/n=-1 cases into a host exception where Go passes.
    public map(nint size)
    {
        m_map = new NilKeyDictionary(size < 0 || size > int.MaxValue ? 0 : (int)size);
        AllocationCounter.Count();
    }

    public map(IEnumerable<KeyValuePair<TKey, TValue>> map)
    {
        // A value-type key can never BE nil — a JIT-time constant — so no source can carry a nil-key
        // entry and Dictionary's own bulk copy applies exactly as it did before nil keys existed.
        if (typeof(TKey).IsValueType)
        {
            m_map = new NilKeyDictionary(map);
            AllocationCounter.Count();
            return;
        }

        // A concrete golib map copies through its store directly, carrying the nil-key entry over as
        // the slot it is (no Dictionary constructor would accept it as a pair).
        if (map is map<TKey, TValue> source)
        {
            m_map = source.m_map is null ? new NilKeyDictionary() : new NilKeyDictionary((IDictionary<TKey, TValue>)source.m_map);
            AllocationCounter.Count();

            if (source.m_map is { HasNilKey: true })
            {
                m_map.NilKeyValue = source.m_map.NilKeyValue;
                m_map.HasNilKey = true;
            }

            return;
        }

        // Any other source — a generated named-map wrapper, a plain Dictionary — is copied entry by
        // entry through the indexer, so a null key ROUTES to the slot rather than throwing.
        m_map = new NilKeyDictionary();
        AllocationCounter.Count();

        foreach (KeyValuePair<TKey, TValue> entry in map)
            this[entry.Key] = entry.Value;
    }

    // Reports whether key is Go's NIL key. `typeof(TKey).IsValueType` is a JIT-time constant, so for
    // a value-type key — the overwhelmingly common shape, and the one PerfMap measures — this test
    // and every nil-slot branch it guards are folded away entirely: those instantiations compile to
    // exactly the code they had before nil keys were supported. Only a reference-typed key (`any`,
    // an interface, `*T`, a func/map/slice-typed key) pays a null check.
    private static bool isNilKey(TKey key)
    {
        return !typeof(TKey).IsValueType && (object?)key is null;
    }

    /// <inheritdoc />
    public int Count
    {
        get
        {
            if (m_map is null)
                return 0;

            // The nil entry counts toward len(m) like any other key.
            return !typeof(TKey).IsValueType && m_map.HasNilKey ? m_map.Count + 1 : m_map.Count;
        }
    }

    public TValue this[TKey key]
    {
        // Reading from a nil map yields the zero value in Go, so route through the
        // null-safe TryGetValue rather than dereferencing m_map directly.
        get => TryGetValue(key, out TValue? value) ? value : GoZero<TValue>();
        set
        {
            // Writing to a nil map panics in Go ("assignment to entry in nil map").
            if (m_map is null)
                throw RuntimeErrorPanic.PlainError("assignment to entry in nil map");

            if (isNilKey(key))
                setNilKey(value);
            else if (s_keyMayNeedUpdate && keyCarriesZero(key))
                setReplacingKey(key, value);
            else if (s_keyNeedsClone || s_keyMayHoldClonable && keyNeedsClone(key))
                setCopyingKey(key, value);
            else
                m_map[key] = value;
        }
    }

    public (TValue, bool) this[TKey key, bool _]
    {
        // Comma-ok read of a nil (or absent) key yields (zero, false).
        get => TryGetValue(key, out TValue? value) ? (value!, true) : (GoZero<TValue>(), false);
    }

    /// <summary>
    /// Reads an element whose Go ZERO VALUE carries run-time SHAPE its C# type does not, taking
    /// that zero from the CALL SITE — Go's <c>m[k]</c> where the element is a fixed-size array.
    /// </summary>
    /// <param name="key">Key to read.</param>
    /// <param name="zero">
    /// Factory for the element type's Go zero value, invoked ONLY on a miss. The converter emits a
    /// non-capturing lambda, so the delegate is cached and a HIT costs nothing beyond the read.
    /// </param>
    /// <remarks>
    /// <para>
    /// A Go map read of an absent key — including every read of a NIL map — yields the element
    /// type's zero value, and for a fixed-size array that zero is <c>[N]T</c> with N zeroed
    /// elements. <c>default(array&lt;T&gt;)</c> is a LENGTH-ZERO array instead (the Go length lives
    /// only in the instance — see <see cref="IGoZeroShaped"/>), so the first index into a missed
    /// entry panicked <c>index out of range [0] with length 0</c> where Go reads a zero. The live
    /// witness is <c>html</c>'s <c>unescapeEntity</c>, whose <c>entity2 map[string][2]rune</c> miss
    /// is the NORMAL path for any text whose <c>&amp;…</c> run is not a two-rune entity.
    /// </para>
    /// <para>
    /// The shape cannot come from the map: it is a property of the Go map TYPE, and neither
    /// <c>map&lt;TKey, TValue&gt;</c> nor a <c>default</c> (nil) one carries it — inferring it from
    /// an existing entry would answer only for a POPULATED map and guess for an empty or nil one.
    /// The READ SITE always knows it statically, and it is the same zero-value ladder every
    /// declaration site already uses (the converter's <c>zeroValueInitializer</c>); this overload is
    /// the seat that ladder had nowhere to sit for a map read.
    /// </para>
    /// </remarks>
    public TValue this[TKey key, Func<TValue> zero] => TryGetValue(key, out TValue? value) ? value : zero();

    /// <summary>
    /// Comma-ok form of the shaped-zero read — Go's <c>v, ok := m[k]</c> where the element type's
    /// zero value carries shape. See <see cref="this[TKey, Func{TValue}]"/>.
    /// </summary>
    /// <param name="key">Key to read.</param>
    /// <param name="zero">Factory for the element's Go zero value, invoked ONLY on a miss.</param>
    /// <param name="_">Overload discriminator (the emitted <c>ꟷ</c>).</param>
    public (TValue, bool) this[TKey key, Func<TValue> zero, bool _] => TryGetValue(key, out TValue? value) ? (value!, true) : (zero(), false);

    /// <inheritdoc />
    public void Add(TKey key, TValue value)
    {
        // Adding to a nil map panics in Go, the same as an index assignment.
        if (m_map is null)
            throw RuntimeErrorPanic.PlainError("assignment to entry in nil map");

        if (isNilKey(key))
            addNilKey(value);
        else
            m_map.Add(storableKey(key), value);
    }

    // Set writes a key with Go's OVERWRITE semantics (unlike Add, which throws on a duplicate
    // key). It exists so a nested map assignment `m[k1][k2] = v` can emit as
    // `m[k1].Set(k2, v)`: `m[k1]` returns a map VALUE (readonly struct), and a C# indexer
    // SETTER cannot run on that rvalue (CS1612) — but a METHOD can, and it mutates the shared
    // backing dictionary, so the write is visible through the original (internal/dag).
    public void Set(TKey key, TValue value)
    {
        if (m_map is null)
            throw RuntimeErrorPanic.PlainError("assignment to entry in nil map");

        if (isNilKey(key))
            setNilKey(value);
        else if (s_keyMayNeedUpdate && keyCarriesZero(key))
            setReplacingKey(key, value);
        else if (s_keyNeedsClone || s_keyMayHoldClonable && keyNeedsClone(key))
            setCopyingKey(key, value);
        else
            m_map[key] = value;
    }

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        if (isNilKey(key))
            return removeNilKey();

        // delete() on a nil map is a no-op in Go (no panic) -- but the key is hashed first, so an
        // unhashable one still panics there.
        if (s_hashMayPanic && m_map is not { Count: > 0 })
            GoEqualityComparer.CheckHashable(key);

        return m_map?.Remove(key) ?? false;
    }

    public void Clear()
    {
        // clear() on a nil map is a no-op in Go.
        if (m_map is null)
            return;

        m_map.Clear();

        if (!typeof(TKey).IsValueType)
        {
            m_map.HasNilKey = false;
            m_map.NilKeyValue = default!;
        }
    }

    /// <inheritdoc />
    public bool TryGetValue(TKey key, out TValue value)
    {
        if (isNilKey(key))
            return tryGetNilKey(out value);

        // A populated store hashes the key itself; an empty or nil one does not, and Go still does.
        if (s_hashMayPanic && m_map is not { Count: > 0 })
            GoEqualityComparer.CheckHashable(key);

        if (m_map is not null)
            return m_map.TryGetValue(key, out value!);

        value = default!;
        return false;
    }

    /// <inheritdoc />
    public bool ContainsKey(TKey key)
    {
        if (isNilKey(key))
            return m_map is { HasNilKey: true };

        if (s_hashMayPanic && m_map is not { Count: > 0 })
            GoEqualityComparer.CheckHashable(key);

        // A nil map contains no keys.
        return m_map?.ContainsKey(key) ?? false;
    }

    #region [ Key replacement ]

    // Whether a key may carry a zero whose sign an overwrite must keep. The typeof tests are JIT-time
    // constants, so a raw float or complex key reads its own value without boxing; any other
    // signed-zero-capable key (an interface, a struct or array holding a float, a defined float type)
    // takes the general test, which is conservative for the composite shapes.
    private static bool keyCarriesZero(TKey key)
    {
        // A reference-type key runs SHARED generic code, where each typeof(TKey) test below is a
        // run-time lookup rather than a constant; send it straight to the general test.
        if (!typeof(TKey).IsValueType)
            return GoEqualityComparer.KeyMayCarryZero(key);

        if (typeof(TKey) == typeof(double))
            return (double)(object)key! == 0;

        if (typeof(TKey) == typeof(float))
            return (float)(object)key! == 0;

        if (typeof(TKey) == typeof(System.Numerics.Complex))
            return ((System.Numerics.Complex)(object)key!).Real == 0 || ((System.Numerics.Complex)(object)key!).Imaginary == 0;

        return GoEqualityComparer.KeyMayCarryZero(key);
    }

    // Go's key update. Dictionary cannot replace a stored key in place, so an existing entry is
    // removed and re-added under the new key. Remove puts the freed entry on Dictionary's free list
    // and the very next Add takes it, so the entry keeps its position and the enumeration order
    // holds -- a BCL implementation fact, guarded loudly by GolibTests (MapKeyReplacementTests).
    // An absent key (a NaN, which equals nothing, is always absent) takes the ordinary store.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void setReplacingKey(TKey key, TValue value)
    {
        if (m_map.Remove(key))
        {
            m_map.Add(storableKey(key), value);
            m_map.KeyEpoch++;
        }
        else
        {
            m_map.Add(storableKey(key), value);
        }
    }

    // A NEW key is stored as a COPY (see s_keyNeedsClone); an existing one keeps its stored key, which is
    // what Go does on an overwrite (only NeedKeyUpdate's signed zero replaces it -- setReplacingKey). The
    // extra lookup is paid only by a clone-needing key.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void setCopyingKey(TKey key, TValue value)
    {
        if (m_map.ContainsKey(key))
            m_map[key] = value;
        else
            m_map.Add(storableKey(key), value);
    }

    // The key as the store must hold it: a copy when it carries shared backing, else itself.
    private static TKey storableKey(TKey key) =>
        s_keyNeedsClone || s_keyMayHoldClonable && keyNeedsClone(key) ? (TKey)((ICloneable)key!).Clone() : key;

    // An INTERFACE key's dynamic value decides (the same predicate as s_keyNeedsClone, per value).
    private static bool keyNeedsClone(TKey key) =>
        key is IArray and not ISlice || key is IGoValueClone;

    // The key a range must produce for a snapshotted entry when a key was replaced during the range:
    // the stored one, found by the store's own comparer. Paid only after a replacement happened.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static TKey storedKey(NilKeyDictionary store, TKey snapshotKey)
    {
        foreach (TKey key in store.Keys)
        {
            if (store.Comparer.Equals(key, snapshotKey))
                return key;
        }

        return snapshotKey;
    }

    #endregion

    #region [ Nil-key slot ]

    // The nil-key operations live out of line so the members above stay small enough for the JIT to
    // inline: a value-type key never reaches them (the guard folds to false and the call site is
    // dropped), and a reference-type key only pays the call on the nil-key path itself.

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void setNilKey(TValue value)
    {
        m_map.NilKeyValue = value;
        m_map.HasNilKey = true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void addNilKey(TValue value)
    {
        // Matches Dictionary<TKey, TValue>.Add's duplicate-key contract (the emitted converted code
        // always writes through the indexer or Set, both of which OVERWRITE like Go).
        if (m_map.HasNilKey)
            throw new System.ArgumentException("An item with the same key has already been added.", nameof(value));

        m_map.NilKeyValue = value;
        m_map.HasNilKey = true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool removeNilKey()
    {
        if (m_map is not { HasNilKey: true })
            return false;

        m_map.HasNilKey = false;
        m_map.NilKeyValue = default!;
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool tryGetNilKey(out TValue value)
    {
        if (m_map is { HasNilKey: true })
        {
            value = m_map.NilKeyValue;
            return true;
        }

        value = default!;
        return false;
    }

    // Go's RANGE-OVER-MAP contract, which is NOT Dictionary's enumeration contract.
    //
    // The Go spec permits the body of a range to mutate the map it is ranging over:
    //
    //     "If a map entry that has not yet been reached is removed during iteration, the
    //      corresponding iteration value will not be produced. If a map entry is created during
    //      iteration, that entry may be produced during the iteration or may be skipped."
    //
    // Dictionary<TKey, TValue>'s enumerator permits neither reading: a structural ADD bumps its
    // version and the very next MoveNext throws InvalidOperationException ("Collection was
    // modified"). (An overwrite of an existing key, and a Remove, are both version-free since
    // .NET Core 3.0 — only the insert bites, which is what made this so easy to miss.) Handing
    // that enumerator to converted code turned a legal Go loop into a runtime fault.
    //
    // It is not a corner case. net/http's h2 server hits it in promoteUndeclaredTrailers, which
    // ranges the handler's header map and writes each promoted "Trailer:Foo" entry back under
    // "Foo" — a NEW key. The exception escaped the handler goroutine, the Phase-4 test host's
    // containment policy absorbed it, the h2 stream was consequently never completed with its
    // trailers and END_STREAM, and the client blocked in http2pipe.Read forever. That is the
    // deterministic hang of TestServerUndeclaredTrailers/h2, and it is guarded from the Go side
    // by tests/Behavioral/MapMutateDuringRange.
    //
    // So range walks a SNAPSHOT OF THE ENTRIES and re-reads each value at the moment it is
    // visited:
    //
    //   * an entry removed before it is reached fails the lookup and is not produced — exactly
    //     the clause Go guarantees;
    //   * an entry created during the range is absent from the snapshot and so is never produced
    //     — the "or may be skipped" half of the clause Go leaves free;
    //   * a value overwritten during the range is produced at its CURRENT value, which is what
    //     Go's own range reads out of the bucket when it arrives there;
    //   * every pre-existing entry is still produced exactly once, so a body that inserts cannot
    //     be re-entered for a key it has already handled.
    //
    // …with ONE key shape where the visit-time lookup is the wrong instrument, and it is a real
    // Go shape rather than a curiosity: a NaN key is equal to NOTHING, itself included, so
    // `m[NaN] = v` twice stores TWO entries and neither can ever be read back OR deleted (see
    // GoEqualityComparer, which gives the float representations Go's `==` rather than the BCL's
    // NaN-finds-itself rule). For such a key the lookup ALWAYS misses, so re-reading on arrival
    // silently dropped every NaN entry from every range — which is a worse defect than the one
    // this method exists to fix, because it is silent. encoding/json read it out immediately:
    // mapEncoder sizes `sv = make([]reflectWithString, v.Len())` and fills it by index from
    // MapRange, so a range that yields fewer entries than len() leaves ZERO reflect.Values in
    // the tail and panics in stringEncoder's v.Type() (TestMarshalTextFloatMap).
    //
    // A miss is therefore disambiguated with the store's OWN comparer: if the key is not even
    // equal to itself, no lookup can ever match it and no delete can ever remove it, so the
    // SNAPSHOTTED entry is produced. Using the dictionary's comparer means "unretrievable" is
    // settled by exactly the relation whose failure is being interpreted, rather than by a
    // hardcoded list of float types — a custom comparer gets the same treatment for free. The
    // one way such an entry does disappear is `clear`, which empties the store outright, so a
    // now-empty store suppresses it.
    //
    // The cost is one KeyValuePair[] per non-empty range where there was none, and the
    // self-equality test only ever runs on the miss path. That is deliberate: this is the
    // construct's SEMANTICS, and go2cs converts behavior first. If a range ever measures hot
    // enough to care, the snapshot is the one thing to pool here — the shape above does not
    // change.
    //
    // THE ORDER IS RANDOMIZED, as Go's is: each range starts the snapshot walk at a random entry and
    // wraps, the way Go's iterator starts at a random group and slot (runtime's TestMapIterOrder and
    // TestMapSparseIterOrder assert two ranges can differ). Only the START moves, so everything above
    // holds whatever it is. The walk is two passes, start..count then 0..start, so no entry pays a
    // wrap test, and the start comes from GoCheapRand (Go's cheaprand, thread-static): no allocation,
    // and a map of 0 or 1 entries draws nothing.
    //
    // The nil-key entry goes first. Go's range order over a map is unspecified (and deliberately
    // randomized), so the position is free.
    private static IEnumerator<KeyValuePair<TKey, TValue>> enumerateStore(NilKeyDictionary store)
    {
        // The snapshot is taken BEFORE anything is produced — including before the nil-key entry —
        // so "created during the range is never produced" holds UNIFORMLY. Taking it after that
        // first yield would leave an entry the body inserted while handling the nil key inside the
        // snapshot and therefore produced, while every later insert was skipped. Both readings are
        // legal Go ("may be produced… or may be skipped"), but only one of them is a rule.
        int count = store.Count;
        KeyValuePair<TKey, TValue>[] entries = count == 0 ? [] : new KeyValuePair<TKey, TValue>[count];

        if (count > 0)
            ((ICollection<KeyValuePair<TKey, TValue>>)store).CopyTo(entries, 0);

        // Go's range visits the nil key like any other key.
        if (!typeof(TKey).IsValueType && store.HasNilKey)
            yield return new KeyValuePair<TKey, TValue>(default!, store.NilKeyValue);

        int start = count > 1 ? (int)GoCheapRand.Next((uint)count) : 0;

        for (int pass = 0, from = start, to = count; pass < 2; pass++, from = 0, to = start)
        for (int index = from; index < to; index++)
        {
            KeyValuePair<TKey, TValue> entry = entries[index];

            // Re-read rather than carrying the value from the snapshot: the body may have
            // overwritten it, and Go reads the bucket on arrival.
            if (store.TryGetValue(entry.Key, out TValue? value))
            {
                // A key REPLACED during the range (a signed zero overwritten) is produced as the
                // stored key, as Go reads the bucket's key on arrival; the snapshot holds the old one.
                // Keyed on "this map has EVER replaced a key", not on a snapshot of the count: a
                // snapshot is one more field on every range's iterator (measured +8 B per range),
                // while this costs nothing on a map that never saw a signed zero overwritten.
                TKey key = s_keyMayNeedUpdate && store.KeyEpoch != 0 && keyCarriesZero(entry.Key)
                    ? storedKey(store, entry.Key)
                    : entry.Key;

                yield return new KeyValuePair<TKey, TValue>(key, value);
                continue;
            }

            // The lookup missed: the entry was either deleted before it was reached — Go says
            // not to produce it — or its key is one NO lookup can match. Only the second kind is
            // still in the map, and only `clear` can have taken it out.
            if (store.Count > 0 && !store.Comparer.Equals(entry.Key, entry.Key))
                yield return entry;
        }
    }

    #endregion

    public bool Equals(map<TKey, TValue> other)
    {
        // Go maps are reference types: `m == nil` is true only for the nil map, and two
        // map values are equal only when they share the same backing store. Comparing by
        // reference identity captures both (and is null-safe — two nil maps share a null
        // backing store and so compare equal, while a nil map differs from an empty one).
        return ReferenceEquals(m_map, other.m_map);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is map<TKey, TValue> other && Equals(other);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        // Enumerates through this map (not the raw dictionary) so a nil-key entry appears, and
        // renders a nil key or value as Go's fmt does.
        return $"map[{(m_map is null ? "<nil>" : string.Join(" ", this.Select(static kvp => $"{render(kvp.Key)}:{render(kvp.Value)}").Take(20)))}{(Count > 20 ? " ..." : "")}]";

        static string render<T>(T value)
        {
            return value?.ToString() ?? "<nil>";
        }
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Reference-based hash, consistent with the identity Equals above; a nil map hashes to 0.
        return m_map?.GetHashCode() ?? 0;
    }

    #region [ Operators ]

    // Enable implicit conversions between map<TKey, TValue> and IDictionary<T>
    public static implicit operator map<TKey, TValue>(Dictionary<TKey, TValue> value)
    {
        return new map<TKey, TValue>(value);
    }

    public static implicit operator Dictionary<TKey, TValue>(map<TKey, TValue> value)
    {
        return value.m_map;
    }

    // map<TKey, TValue> to map<TKey, TValue> comparisons
    public static bool operator ==(map<TKey, TValue> a, map<TKey, TValue> b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(map<TKey, TValue> a, map<TKey, TValue> b)
    {
        return !(a == b);
    }

    // map<T> to IMap comparisons
    public static bool operator ==(IMap a, map<TKey, TValue> b)
    {
        return b.Equals(a);
    }

    public static bool operator !=(IMap a, map<TKey, TValue> b)
    {
        return !(a == b);
    }

    public static bool operator ==(map<TKey, TValue> a, IMap b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(map<TKey, TValue> a, IMap b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Gets a flag indicating whether the map is nil — no backing store.
    /// </summary>
    public bool IsNil => m_map is null;

    // map<T> to nil comparisons
    public static bool operator ==(map<TKey, TValue> map, NilType _)
    {
        // A map is nil only when it has no backing store — an empty but allocated map is not nil.
        return map.m_map is null;
    }

    public static bool operator !=(map<TKey, TValue> map, NilType nil)
    {
        return !(map == nil);
    }

    public static bool operator ==(NilType nil, map<TKey, TValue> map)
    {
        return map == nil;
    }

    public static bool operator !=(NilType nil, map<TKey, TValue> map)
    {
        return map != nil;
    }

    public static implicit operator map<TKey, TValue>(NilType _)
    {
        return default;
    }

    #endregion

    #region [ Interface Implementations ]

    nint IMap.Length => Count;

    (bool present, object? value) IMap.NilKeyEntry
    {
        // Reads the slot directly rather than through the IMap<TKey, TValue> default (which asks the
        // comma-ok indexer) — same answer, no interface dispatch.
        get
        {
            if (typeof(TKey).IsValueType || m_map is not { HasNilKey: true })
                return (false, null);

            return (true, m_map.NilKeyValue);
        }
    }

    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        get => this[key];
        set => this[key] = value;
    }

    // Keys/Values MATERIALIZE when a nil-key entry is present — Dictionary's own live views cannot
    // represent it — and hand back those views unchanged otherwise. A nil map has neither.
    ICollection<TValue> IDictionary<TKey, TValue>.Values =>
        m_map is null ? System.Array.Empty<TValue>() :
        !typeof(TKey).IsValueType && m_map.HasNilKey ? this.Select(static kvp => kvp.Value).ToList() :
        m_map.Values;

    ICollection<TKey> IDictionary<TKey, TValue>.Keys =>
        m_map is null ? System.Array.Empty<TKey>() :
        !typeof(TKey).IsValueType && m_map.HasNilKey ? this.Select(static kvp => kvp.Key).ToList() :
        m_map.Keys;

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        // ICollection's contract: remove only when the stored value matches too.
        return ((ICollection<KeyValuePair<TKey, TValue>>)this).Contains(item) && Remove(item.Key);
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Clear()
    {
        Clear();
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        return TryGetValue(item.Key, out TValue? value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        foreach (KeyValuePair<TKey, TValue> entry in this)
            array[arrayIndex++] = entry;
    }

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        // Ranging over a nil map performs zero iterations in Go.
        if (m_map is null)
            return Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator();

        // Go's range-over-map contract — including the nil-key entry, and including a body that
        // mutates the map it is ranging over. See enumerateStore.
        return enumerateStore(m_map);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        // Yields KeyValuePair<TKey, TValue> entries, matching what Dictionary's own non-generic
        // enumerator produces (the reflection bridge's MapIter reads .Key/.Value off Current).
        return ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator();
    }

    #endregion

    /// <inheritdoc />
    public static map<TKey, TValue> Make(nint p1 = 0, nint p2 = -1)
    {
        return new map<TKey, TValue>(p1);
    }
}
