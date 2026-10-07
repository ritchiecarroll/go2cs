// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Deep equality test via reflection
using go;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

// Hand-finished conversion (the reflection bridge — Phase 4, DeepEqual). Go's deepValueEqual keys its
// cycle-detection `visited` map on the values' internal data words (v.ptr / v.pointer()) — raw eface
// addresses that the managed bridge never populates, so the auto form NREs converting the null
// unsafe.Pointer slot (first operational hits: strings/bytes TestSplit/TestSplitAfter). The managed
// form recurses over the SAME boxed values the bridge Value carries (value_impl.cs) and keys cycle
// detection on managed reference identity instead: a pointer is its ж<T> box, a map is its backing
// Dictionary, and a slice is its backing array + window offset (Go keys on &s[0] — base plus offset).
// DeepEqual is displaced too since 2026-09-07 — not for a semantic but for the dead `map<visit, bool>`
// its auto body minted on every call and this file never reads (88 B and one golib object, flat). Both
// go through the manualConversionFuncs registry (go2cs/manualTypeOperations.go); this module marker
// also makes go2cs skip re-converting this file.
// See docs/phase4/DESIGN-reflection-bridge.md.

[module: GoManualConversion]

namespace go;

partial class reflect_package {

// DeepEqual reports whether x and y are “deeply equal,” defined as follows.
// Two values of identical type are deeply equal if one of the following cases applies.
// Values of distinct types are never deeply equal.
//
// Array values are deeply equal when their corresponding elements are deeply equal.
//
// Struct values are deeply equal if their corresponding fields,
// both exported and unexported, are deeply equal.
//
// Func values are deeply equal if both are nil; otherwise they are not deeply equal.
//
// Interface values are deeply equal if they hold deeply equal concrete values.
//
// Map values are deeply equal when all of the following are true:
// they are both nil or both non-nil, they have the same length,
// and either they are the same map object or their corresponding keys
// (matched using Go equality) map to deeply equal values.
//
// Pointer values are deeply equal if they are equal using Go's == operator
// or if they point to deeply equal values.
//
// Slice values are deeply equal when all of the following are true:
// they are both nil or both non-nil, they have the same length,
// and either they point to the same initial entry of the same underlying array
// (that is, &x[0] == &y[0]) or their corresponding elements (up to length) are deeply equal.
// Note that a non-nil empty slice and a nil slice (for example, []byte{} and []byte(nil))
// are not deeply equal.
//
// Other values - numbers, bools, strings, and channels - are deeply equal
// if they are equal using Go's == operator.
//
// In general DeepEqual is a recursive relaxation of Go's == operator.
// However, this idea is impossible to implement without some inconsistency.
// Specifically, it is possible for a value to be unequal to itself,
// either because it is of func type (uncomparable in general)
// or because it is a floating-point NaN value (not equal to itself in floating-point comparison),
// or because it is an array, struct, or interface containing
// such a value.
// On the other hand, pointer values are always equal to themselves,
// even if they point at or contain such problematic values,
// because they compare equal using Go's == operator, and that
// is a sufficient condition to be deeply equal, regardless of content.
// DeepEqual has been defined so that the same short-cut applies
// to slices and maps: if x and y are the same slice or the same map,
// they are deeply equal regardless of content.
//
// As DeepEqual traverses the data values it may find a cycle. The
// second and subsequent times that DeepEqual compares two pointer
// values that have been compared before, it treats the values as
// equal rather than examining the values to which they point.
// This ensures that DeepEqual terminates.
//
// HAND-OWNED, and the reason is an ALLOCATION rather than a semantic. Go's body ends
// `deepValueEqual(v1, v2, make(map[visit]bool))`, and the hand-owned deepValueEqual below never
// reads that map — it carries a HashSet<visitPair> instead, because Go keys cycle detection on the
// values’ internal data words (unsafe.Pointer) and the managed bridge has none. The auto conversion
// therefore minted a golib map nothing could read, on EVERY DeepEqual call in the corpus: measured
// at exactly 88 B and exactly ONE golib object, flat across every shape.
//
// That one object is load-bearing. reflect’s TestDeepEqualAllocs rows are `deferred` with a ratified
// FLOOR of 2 boxes at the `any` seam, and the floor counts OBJECTS: dropping the map takes a scalar
// comparison from 3 to 2, which is the floor itself. Judged in BYTES it is 0.7% of the worst row and
// not worth a displacement; judged against the floor it is the entire remaining gap for that family.
//
// Go’s doc comment above is carried HERE deliberately: a displaced declaration’s attached doc group
// is dropped by design (displacedCommentDrain_test.go states this is a placeholder ruling, not a
// drain defect), and DeepEqual’s 52 lines are the contract for a public API. Losing them from the
// corpus is not acceptable for a derivative work, so the hand-own carries them.
//
// Everything else is Go’s control flow verbatim: the nil pair short-circuits, the types must match,
// and the walk is the same one the auto body called.
public static bool DeepEqual(any x, any y) {
    if (x == default! || y == default!) {
        return AreEqual(x, y);
    }
    var v1 = ValueOf(x);
    var v2 = ValueOf(y);
    if (!AreEqual(v1.Type(), v2.Type())) {
        return false;
    }
    return deepValueEqualBoxed(v1, v2, new HashSet<visitPair>());
}

// Tests for deep equality using reflected types. Mirrors Go's deepValueEqual over the bridge's boxed
// managed values. The map argument is Go's address-keyed visited map — unusable in the managed model
// (no data words); the managed recursion carries a reference-identity set instead, with the same
// semantics: all checks in progress are assumed true when re-encountered, and entries persist for the
// whole DeepEqual call.
//
// Retained although DeepEqual above no longer routes through it: it is the displaced form of a Go
// function that still exists, and reflectlite's mirror and any future in-package caller reach it.
internal static bool deepValueEqual(ΔValue v1, ΔValue v2, map<visit, bool> visited) {
    return deepValueEqualBoxed(v1, v2, new HashSet<visitPair>());
}

private static bool deepValueEqualBoxed(ΔValue v1, ΔValue v2, HashSet<visitPair> visited) {
    if (!v1.IsValid() || !v2.IsValid()) {
        return v1.IsValid() == v2.IsValid();
    }
    if (!AreEqual(v1.Type(), v2.Type())) {
        return false;
    }
    // The LIVE value, never the raw `boxed` field: an ADDRESSABLE Value — a slice element, an array
    // element, a struct field — carries its value behind `addrBox` (the ж<T> it aliases) and leaves
    // `boxed` null, so every raw read below saw null on BOTH sides and the identity short-circuits
    // fired. `DeepEqual([][]byte{[]byte("ab")}, [][]byte{[]byte("ac")})` was TRUE: each element's
    // backing read as null, matched "same initial entry of the same underlying array", and the
    // elementwise walk never ran (`live` IS `boxed` whenever the Value is not addressable, so this
    // changes nothing else).
    object? live1 = v1.live, live2 = v2.live;
    // Go's hard()/visited step: only pointer, map, and slice values can head a reference cycle in the
    // managed model (a bridge Value never has Kind Interface — the boxed value is always concrete).
    // Go also keys the visit on the Type; managed identity roots are per-variable objects with a fixed
    // type, so the (root1, root2) pair alone cannot collide across types.
    ΔKind kind = v1.Kind();
    if (kind == ΔPointer || kind == Map || kind == ΔSlice) {
        (object? root1, nint off1) = identityRoot(live1);
        (object? root2, nint off2) = identityRoot(live2);
        if (root1 is not null && root2 is not null && !visited.Add(new visitPair(root1, off1, root2, off2))) {
            // Already seen further up the recursion — the comparison algorithm assumes checks in
            // progress are true when it reencounters them (this is what makes DeepEqual terminate).
            return true;
        }
    }
    if (kind == Array) {
        // The BYTE-ARRAY fast path, the Array arm's counterpart to the []byte one the Slice arm
        // below already has. Until this existed the Array arm had NO fast path at all, which is why
        // [6]byte cost 32 golib objects for six bytes while []byte cost 4: TryByteSliceView is gated
        // on KindOf(ct) == Slice and never accepts an array<byte>, so every array walked elementwise
        // through Index(), paying an ElemRefBox and an element boxing per element per side.
        //
        // array<T> implements IArray<T>, which declares ToSpan(), so a boxed array<byte> compares as
        // a span with no Index(), no box and no boxing. Both sides are the SAME Go type by the
        // AreEqual check above, so one view test settles both -- the same soundness argument the
        // []byte arm makes.
        //
        // ⚠ RESTRICTED TO BYTE, and that is load-bearing rather than caution. Byte equality IS Go's
        // ==, but a span compare over float/double dispatches to Double.Equals, which reports
        // [NaN] == [NaN] TRUE where Go's == says false -- measured on both sides. Widening this to
        // floats would invert a real answer, and NONE of TestDeepEqualAllocs' 39 subtests could
        // catch it, because deepEqualPerfTests uses 1.414 and never NaN. A float path needs an
        // elementwise == loop, which is still allocation-free; it is not written here.
        //
        // Measured, [][6]uint8: 52 golib objects / 9,216.55 B/op -> 9 / 2,152.04. The residue is the
        // OUTER one-element slice walk, which still pays two Index() calls; a slice of scalars is
        // untouched by this arm, which reaches Array kind only. Guarded by the differ-first,
        // differ-last, nested and non-byte rows of the arm-4 correctness arbiter.
        if (live1 is IArray<byte> byteArr1 && live2 is IArray<byte> byteArr2) {
            return byteArr1.ToSpan().SequenceEqual(byteArr2.ToSpan());
        }
        for (nint i = 0; i < v1.Len(); i++) {
            if (!deepValueEqualBoxed(v1.Index(i), v2.Index(i), visited)) {
                return false;
            }
        }
        return true;
    }
    if (kind == ΔSlice) {
        (object? data1, nint low1) = sliceData(live1);
        (object? data2, nint low2) = sliceData(live2);
        if (data1 is null != data2 is null) {
            // A nil slice (null backing — the golib `default`) and a non-nil empty slice are not
            // deeply equal, per the DeepEqual doc.
            return false;
        }
        if (v1.Len() != v2.Len()) {
            return false;
        }
        if (v1.Len() == 0) {
            return true;
        }
        if (ReferenceEquals(data1, data2) && low1 == low2) {
            // Same initial entry of the same underlying array (&x[0] == &y[0]).
            return true;
        }
        if (GoReflect.TryByteSliceView(live1, out slice<byte> b1) &&
            GoReflect.TryByteSliceView(live2, out slice<byte> b2)) {
            // Special case for []byte, which is common (Go routes this through bytealg.Equal). Go
            // reaches it by the element KIND, never by the slice's or the element's NAME — a raw
            // []byte, a defined slice type over byte (xml.CharData, net.IP) and a slice over a
            // defined byte element all qualify — and that is exactly the set TryByteSliceView
            // aliases, so all three take one route here as they already do in Value.Bytes. The
            // `is slice<byte>` test this replaces covered only the first, silently sending the
            // other two around the elementwise Value.Index walk (both sides are the SAME Go type
            // by the AreEqual check above, so one view test settling both is sound).
            return b1.ToSpan().SequenceEqual(b2.ToSpan());
        }
        // The []byte arm above generalised to every element kind whose BITWISE equality IS Go's `==`.
        // Same argument, same soundness: both sides are the SAME Go type by the AreEqual check, the
        // nil/length/same-backing cases are already settled, so all that remains is content -- and for
        // these kinds a span compare answers it without materialising a Value per element.
        //
        // ⚠ The admitted set is DELIBERATELY NARROW and float/complex are NOT in it. SequenceEqual
        // dispatches to IEquatable<T>.Equals, and Double.Equals(NaN, NaN) is TRUE where Go's `==` is
        // false -- measured on both sides. A []float64{NaN} pair would compare EQUAL here and NOT equal
        // in Go, and NONE of TestDeepEqualAllocs' 39 subtests could catch it because deepEqualPerfTests
        // uses 1.414. Those kinds keep the elementwise walk. Guarded by arm5's NaN rows, which go RED
        // when float32/float64 are added to the admitted set (measured: exactly 3 rows invert).
        if (tryBitwiseSliceEqual(live1, live2, elementTypeOf(v1), out bool bitwiseEqual)) {
            return bitwiseEqual;
        }
        for (nint i = 0; i < v1.Len(); i++) {
            if (!deepValueEqualBoxed(v1.Index(i), v2.Index(i), visited)) {
                return false;
            }
        }
        return true;
    }
    if (kind == ΔInterface) {
        if (v1.IsNil() || v2.IsNil()) {
            return v1.IsNil() == v2.IsNil();
        }
        return deepValueEqualBoxed(v1.Elem(), v2.Elem(), visited);
    }
    if (kind == ΔPointer) {
        if (live1 is not null && ReferenceEquals(live1, live2)) {
            // Same ж<T> box — Go's same-address short-circuit (one box per variable).
            return true;
        }
        // Elem maps a nil box to the invalid Value, so two distinct nil pointers compare equal
        // through the invalid==invalid rule, and nil-vs-non-nil compares false — matching Go.
        return deepValueEqualBoxed(v1.Elem(), v2.Elem(), visited);
    }
    if (kind == Struct) {
        for ((nint i, nint n) = (0, v1.NumField()); i < n; i++) {
            if (!deepValueEqualBoxed(v1.Field(i), v2.Field(i), visited)) {
                return false;
            }
        }
        return true;
    }
    if (kind == Map) {
        if (v1.IsNil() != v2.IsNil()) {
            return false;
        }
        if (v1.Len() != v2.Len()) {
            return false;
        }
        IDictionary? m1 = mapBacking(live1);
        IDictionary? m2 = mapBacking(live2);
        if (ReferenceEquals(m1, m2)) {
            // The same map object (or both nil) — deeply equal regardless of content.
            return true;
        }
        if (m1 is null || m2 is null) {
            return m1 is null == m2 is null;
        }
        // Every element Value is typed by the map's DECLARED element type, exactly as MapIndex and
        // MapIter.Value type theirs — never by the stored object's dynamic type. A slot-derived Value
        // is Go's rule for the whole bridge, and a map entry is a slot: an element read through
        // makeReflectValue instead reports the INVALID zero Value whenever the entry physically holds
        // C# null, so a nil map element compared EQUAL to a missing key and UNEQUAL to the canonical
        // typed nil the write path stores. The two spellings of one nil then separated every map with
        // a nil element that had been WRITTEN through reflect on one side and declared as a literal on
        // the other — `map[string]*Small{"20": nil}` decoded by encoding/json vs the same literal
        // (encoding/json's TestUnmarshal `All` fixture, rows #56–#63, is exactly that pair).
        System.Type? elemType1 = elementTypeOf(v1);
        System.Type? elemType2 = elementTypeOf(v2);
        // Go's range visits a NIL key like any other, but the backing Dictionary cannot HOLD one —
        // golib keeps that entry in a dedicated slot, invisible to the walk below (and its presence
        // alone does not show up in the Len comparison above, which one extra ordinary key hides).
        (bool nilPresent1, object? nilValue1) = live1 is IMap nilMap1 ? nilMap1.NilKeyEntry : (false, null);
        (bool nilPresent2, object? nilValue2) = live2 is IMap nilMap2 ? nilMap2.NilKeyEntry : (false, null);
        if (nilPresent1 != nilPresent2) {
            return false;
        }
        if (nilPresent1 && !deepValueEqualBoxed(mapElemValue(nilValue1, elemType1, v1.flag),
                                                mapElemValue(nilValue2, elemType2, v2.flag), visited)) {
            return false;
        }
        foreach (DictionaryEntry entry in m1) {
            if (!m2.Contains(entry.Key)) {
                // Go: MapIndex yields the invalid Value for a missing key → not equal.
                return false;
            }
            // Two stored nil elements now recurse as two VALID nil Values of the element type, and
            // agree through the kind's own nil rule (pointer: both boxes nil; interface: IsNil ==
            // IsNil), rather than through the invalid==invalid rule that only held when BOTH sides
            // happened to spell nil the same way.
            if (!deepValueEqualBoxed(mapElemValue(entry.Value, elemType1, v1.flag),
                                     mapElemValue(m2[entry.Key], elemType2, v2.flag), visited)) {
                return false;
            }
        }
        return true;
    }
    if (kind == Func) {
        // Go: "Func values are deeply equal if both are nil; otherwise they are not deeply equal."
        // This must be asked of the values, NOT inferred from the invalid==invalid rule above: that
        // rule only fires for a nil func boxed as `any` (the null object). A nil func reached as a
        // STRUCT FIELD — or as a slice/array element, or a map value — is typed by its static func
        // type and is therefore a VALID nil Value (see Value.Field), so an unconditional false
        // declared two nil func fields unequal. compress/flate's TestWriterReset nils out the
        // compressor's fill/step/bulkHasher precisely so DeepEqual can compare the rest, and every
        // one of its ten levels failed on that.
        return v1.IsNil() && v2.IsNil();
    }
    if (kind == ΔInt || kind == Int8 || kind == Int16 || kind == Int32 || kind == Int64) {
        return v1.Int() == v2.Int();
    }
    if (kind == ΔUint || kind == Uint8 || kind == Uint16 || kind == Uint32 || kind == Uint64 || kind == Uintptr) {
        return v1.Uint() == v2.Uint();
    }
    if (kind == ΔString) {
        return v1.String() == v2.String();
    }
    if (kind == ΔBool) {
        return v1.Bool() == v2.Bool();
    }
    if (kind == Float32 || kind == Float64) {
        // C# double == carries IEEE semantics: a NaN is not equal to itself, exactly like Go.
        return v1.Float() == v2.Float();
    }
    if (kind == Complex64 || kind == Complex128) {
        return v1.Complex() == v2.Complex();
    }
    { /* default: */
        // Can't do better than this: normal equality suffices.
        return AreEqual(valueInterface(v1, false), valueInterface(v2, false));
    }
}

// elementTypeOf returns a map Value's DECLARED element type, or null when the bridge has no managed
// type for it (a synthetic descriptor with no sysType) — in which case the walk falls back to the
// dynamic typing it had before, which is right for every entry that physically holds a value.
private static System.Type? elementTypeOf(ΔValue v) {
    return v.typ_ == nil ? null : GoReflect.ElementType(v.typ_.Value.sysType);
}

// The element kinds whose BITWISE equality is exactly Go's `==`, so a span compare answers DeepEqual
// for a slice of them. Integers and bool only.
//
// ⚠ float32/float64 are ABSENT ON PURPOSE and the omission is load-bearing: Go compares floats with
// `==`, under which NaN != NaN, while Double.Equals(NaN, NaN) -- which SequenceEqual dispatches to --
// is TRUE. Admitting them inverts a real answer on a shape the alloc suite cannot see (arm5 control B:
// exactly 3 NaN rows red). complex64/128 are absent for the same reason one level in, and a DEFINED
// ELEMENT type (`type MyInt int`) is absent because it arrives as its own wrapper System.Type and falls
// through to the elementwise walk -- correct, merely unaccelerated. A defined SLICE type over a plain
// element DOES take this path, through the shared-backing view ctor, exactly as the []byte arm does.
// Aliases are golib's: int8 = SByte, uint8 = Byte, int/uint = nint/nuint (golib.csproj).
private static bool isGoBitwiseEqualElement(System.Type t) =>
    t == typeof(bool) ||
    t == typeof(sbyte) || t == typeof(byte) ||
    t == typeof(short) || t == typeof(ushort) ||
    t == typeof(int) || t == typeof(uint) ||
    t == typeof(long) || t == typeof(ulong) ||
    t == typeof(nint) || t == typeof(nuint) ||
    // golib's uintptr is a STRUCT, not a BCL primitive, so it needs naming separately -- and it is
    // admitted on a READ rather than an assumption: `Equals(uintptr other) => Value == other.Value`
    // (uintptr.cs:65) is an integer comparison of the nuint backing, and its float operators are
    // explicit CONVERSIONS, not equality, so none of the NaN asymmetry that bars float32/float64
    // reaches it. It is the only golib scalar struct, so no sibling is left inconsistent.
    t == typeof(uintptr);

// One cached delegate per element type; a null memoises a REFUSAL so an inadmissible kind is rejected
// without re-deciding. The factory is STATIC -- the key IS the state -- so a cache hit allocates no
// display-class closure, which is the cost the descriptor caches were carrying until train 33.
private static readonly ConcurrentDictionary<System.Type, Func<object, object, bool>?> s_bitwiseSliceEq = new();

private static bool tryBitwiseSliceEqual(object? a, object? b, System.Type? elemType, out bool equal) {
    equal = false;
    if (a is null || b is null || elemType is null) {
        return false;
    }
    Func<object, object, bool>? cmp = s_bitwiseSliceEq.GetOrAdd(elemType, static et =>
        isGoBitwiseEqualElement(et)
            ? typeof(reflect_package).GetMethod(nameof(bitwiseSliceEqualOf), BindingFlags.NonPublic | BindingFlags.Static)!
                  .MakeGenericMethod(et).CreateDelegate<Func<object, object, bool>>()
            : null);
    if (cmp is null) {
        return false;
    }
    equal = cmp(a, b);
    return true;
}

// A raw slice<E> unboxes; a DEFINED slice type over E reaches its window through the same shared
// backing view ctor GoReflect.byteSliceViewOf uses, so both shapes alias rather than copy.
private static bool bitwiseSliceEqualOf<E>(object a, object b) where E : IEquatable<E> {
    slice<E> x = a is slice<E> rawA ? rawA : new slice<E>((ISlice<E>)a);
    slice<E> y = b is slice<E> rawB ? rawB : new slice<E>((ISlice<E>)b);
    return x.ToSpan().SequenceEqual(y.ToSpan());
}

// mapElemValue builds the Value for one map entry, typed by the map's declared element type — the
// slot rule MapIndex and MapIter.Value already follow, so a lookup, a range and a DeepEqual over one
// map all describe its elements identically.
private static ΔValue mapElemValue(object? boxed, System.Type? elemType, flag inheritRO) {
    return elemType is null ? makeReflectValue(boxed) : makeTypedValue(boxed, elemType, null, inheritRO);
}

// A visited entry: the identity roots of two values under in-progress comparison, compared by managed
// reference identity plus the slice window offset (Go keys on the data addresses; a pointer's root is
// its ж<T> box, a map's its backing Dictionary, a slice's its backing array + Low).
private readonly struct visitPair(object a1, nint off1, object a2, nint off2) : IEquatable<visitPair> {
    private readonly object m_a1 = a1;
    private readonly nint m_off1 = off1;
    private readonly object m_a2 = a2;
    private readonly nint m_off2 = off2;

    public bool Equals(visitPair other) {
        return ReferenceEquals(m_a1, other.m_a1) && ReferenceEquals(m_a2, other.m_a2) &&
               m_off1 == other.m_off1 && m_off2 == other.m_off2;
    }

    public override bool Equals(object? obj) {
        return obj is visitPair other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(RuntimeHelpers.GetHashCode(m_a1), m_off1, RuntimeHelpers.GetHashCode(m_a2), m_off2);
    }
}

// identityRoot returns the managed object that stands for a value's Go data address, for cycle
// detection: a pointer's ж<T> box, a map's backing Dictionary, a slice's backing array + Low. A nil
// value (null box, nil pointer, null backing) has no root — Go never puts nil in the visited map.
private static (object? root, nint offset) identityRoot(object? boxed) {
    switch (boxed) {
        case null:
            return (null, 0);
        case ISlice:
            return sliceData(boxed);
        case IMap:
            return (mapBacking(boxed), 0);
        // The STRUCTURAL nil-pointer question, asked through the interface every pointer box
        // implements — not the reflected `IsNull` property this used to read, which reports a real
        // address whose pointee is nil (`&i` with a nil `i`) as nil and would drop it from cycle
        // detection. See ж<T>.IsNilPointer.
        case INilPointer { IsNilPointer: true }:
            return (null, 0);
        default:
            return (boxed, 0);
    }
}

// Per-closed-generic-type accessors for the REAL backing store of a boxed golib container. slice<T>'s
// public Source materializes a detached copy, so identity (and nil-ness — a nil slice is the golib
// `default`, null m_array) must come from the actual m_array/m_low fields; map<K,V> likewise only
// exposes its Dictionary internally. Field reads are cached per type.
private static readonly ConcurrentDictionary<System.Type, (FieldInfo? array, FieldInfo? low, FieldInfo? nested)> s_sliceFields = new();
private static readonly ConcurrentDictionary<System.Type, FieldInfo?> s_mapField = new();

// sliceData returns a boxed slice's backing array and window offset — (null, 0) for the nil slice.
//
// A generated NAMED-slice wrapper (`type S []E`) holds a slice<E> STRUCT, not the m_array/m_low pair
// this probe reads, so — exactly as for the named-MAP wrapper mapBacking handles below, and with the
// same signature — the probe takes a second step through such a field. Without it BOTH sides of a
// named-slice comparison resolved to null and the "same initial entry of the same underlying array"
// short-circuit above matched them, so two named slices of equal length were reported deeply equal
// REGARDLESS of their contents; a nil named slice compared equal to an empty one (both backings read
// null, so the nil/empty rule never fired); and identityRoot was blind the same way, so a named-slice
// cycle was never detected either. encoding/xml's TestCopyTokenCharData/TestCopyTokenComment are
// exactly that shape: CopyToken really does clone its buffer, yet mutating the ORIGINAL still
// compared equal to the clone — which the test reports as "uses same buffer", pointing at a copy that
// was never the defect. The second step is taken only for a slice-KINDED type, so a struct that
// merely HAS a slice field can never be mistaken for one, and the recursion terminates because the
// nested value is a strictly smaller struct — slice<E> carries the m_array/m_low pair itself.
private static (object? data, nint low) sliceData(object? boxed) {
    if (boxed is null) {
        return (null, 0);
    }
    (FieldInfo? array, FieldInfo? low, FieldInfo? nested) = s_sliceFields.GetOrAdd(boxed.GetType(), static t => {
        FieldInfo? array = t.GetField("m_array", BindingFlags.Instance | BindingFlags.NonPublic);
        if (array is not null) {
            return (array, t.GetField("m_low", BindingFlags.Instance | BindingFlags.NonPublic), null);
        }
        if (typeof(ISlice).IsAssignableFrom(t)) {
            foreach (FieldInfo f in t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {
                if (typeof(ISlice).IsAssignableFrom(f.FieldType)) {
                    return (null, null, f);
                }
            }
        }
        return (null, null, null);
    });
    if (array is null) {
        return nested is null ? (null, 0) : sliceData(nested.GetValue(boxed));
    }
    object? data = array.GetValue(boxed);
    return data is null ? (null, 0) : (data, low is null ? 0 : (nint)low.GetValue(boxed)!);
}

// mapBacking returns a boxed map's backing Dictionary — null for the nil map (no backing store).
//
// A generated NAMED-map wrapper (`type M map[K]V`) holds a map<K,V> STRUCT, and map<K,V> implements
// only the GENERIC dictionary surface (IMap<K,V> : IDictionary<K,V>) — nothing assignable to the
// non-generic IDictionary this walk needs. The probe therefore takes a second step through such a
// field: without it BOTH sides of a named-map comparison resolved to null, the ReferenceEquals(m1, m2)
// short-circuit above matched them as "the same map object", and two named maps of equal length were
// reported deeply equal REGARDLESS of their contents (identityRoot was blind the same way, so a
// named-map cycle was never detected either). The recursion terminates because the nested value is a
// strictly smaller struct — map<K,V>'s own backing store IS an IDictionary.
//
// A SELF-CONTAINING named map (`type M map[int]M`) is the one wrapper that holds its map<K,V> in a
// StrongBox instead (go2cs-gen's InheritedTypeTemplate.HoldsMapInHolder: .NET cannot load the wrapper
// with the map inline), so the probe also steps through a holder of an IMap — the step
// GoReflect.TryUnwrapWrapperValue already takes. Without it both sides resolved to null again and any
// two such maps compared deeply equal (SelfContainingMapHolder: `DeepEqual(M{1: nil}, M{2: nil})`).
private static IDictionary? mapBacking(object? boxed) {
    if (boxed is null) {
        return null;
    }
    FieldInfo? field = s_mapField.GetOrAdd(boxed.GetType(), static t => {
        FieldInfo? nested = null;
        foreach (FieldInfo f in t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)) {
            if (typeof(IDictionary).IsAssignableFrom(f.FieldType)) {
                return f;
            }
            if (nested is null && (typeof(IMap).IsAssignableFrom(f.FieldType) || isMapHolder(f.FieldType))) {
                nested = f;
            }
        }
        return nested;
    });
    object? value = field?.GetValue(boxed);
    if (value is IStrongBox holder) {
        value = holder.Value;
    }
    return value as IDictionary ?? (value is IMap ? mapBacking(value) : null);
}

private static bool isMapHolder(Type fieldType) {
    return fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(StrongBox<>) &&
           typeof(IMap).IsAssignableFrom(fieldType.GetGenericArguments()[0]);
}

} // end reflect_package
