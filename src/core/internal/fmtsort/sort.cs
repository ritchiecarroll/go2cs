// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fmtsort provides a general stable ordering mechanism
// for maps, on behalf of the fmt and text/template packages.
// It is not guaranteed to be efficient and works only for types
// that are valid map keys.
namespace go.@internal;

using cmp = cmp_package;
using reflect = reflect_package;
using slices = slices_package;

partial class fmtsort_package {

partial struct SortedMap /*[]KeyValue*/;

// Note: Throughout this package we avoid calling reflect.Value.Interface as
// it is not always legal to do so and it's easier to avoid the issue than to face it.

// KeyValue holds a single key and value pair found in a map.
partial struct KeyValue {
    public reflectꓸValue Key, Value;
}

// Sort accepts a map and returns a SortedMap that has the same keys and
// values but in a stable sorted order according to the keys, modulo issues
// raised by unorderable key values such as NaNs.
//
// The ordering rules are more general than with Go's < operator:
//
//   - when applicable, nil compares low
//   - ints, floats, and strings order by <
//   - NaN compares less than non-NaN floats
//   - bool compares false before true
//   - complex compares real, then imag
//   - pointers compare by machine address
//   - channel values compare by machine address
//   - structs compare each field in turn
//   - arrays compare each element in turn.
//     Otherwise identical arrays compare by length.
//   - interface values compare first by reflect.Type describing the concrete type
//     and then by concrete value as described in the previous rules.
public static SortedMap Sort(reflectꓸValue mapValue) {
    if (mapValue.Type().Kind() != reflect.Map) {
        return default!;
    }
    // Note: this code is arranged to not panic even in the presence
    // of a concurrent map update. The runtime is responsible for
    // yelling loudly if that happens. See issue 33275.
    nint n = mapValue.Len();
    var sorted = new SortedMap(new slice<KeyValue>(0, () => new(), n));
    var iter = mapValue.MapRange();
    while (iter.Next()) {
        sorted = append(sorted, new KeyValue(iter.Key(), iter.Value()));
    }
    slices.SortStableFunc(sorted, (KeyValue a, KeyValue b) => compare(a.Key, b.Key));
    return sorted;
}

// compare compares two values of the same type. It returns -1, 0, 1
// according to whether a > b (1), a == b (0), or a < b (-1).
// If the types differ, it returns -1.
// See the comment on Sort for the comparison rules.
internal static nint compare(reflectꓸValue aVal, reflectꓸValue bVal) {
    var (aType, bType) = (aVal.Type(), bVal.Type());
    if (!AreEqual(aType, bType)) {
        return -1; // No good answer possible, but don't return 0: they're not equal.
    }
    var exprᴛ1 = aVal.Kind();
    if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
        return cmp.Compare(aVal.Int(), bVal.Int());
    }
    if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) {
        return cmp.Compare(aVal.Uint(), bVal.Uint());
    }
    if (exprᴛ1 == reflect.ΔString) {
        return cmp.Compare(aVal.String(), bVal.String());
    }
    if (exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64) {
        return cmp.Compare(aVal.Float(), bVal.Float());
    }
    if (exprᴛ1 == reflect.Complex64 || exprᴛ1 == reflect.Complex128) {
        var (a, b) = (aVal.Complex(), bVal.Complex());
        {
            nint c = cmp.Compare(real(a), real(b)); if (c != 0) {
                return c;
            }
        }
        return cmp.Compare(imag(a), imag(b));
    }
    if (exprᴛ1 == reflect.ΔBool) {
        var (a, b) = (aVal.Bool(), bVal.Bool());
        switch (ᐧ) {
        case {} when a == b: {
            return 0;
        }
        case {} when a: {
            return 1;
        }
        default: {
            return -1;
        }}

    }
    else if (exprᴛ1 == reflect.ΔPointer || exprᴛ1 == reflect.ΔUnsafePointer) {
        return cmp.Compare(aVal.Pointer(), bVal.Pointer());
    }
    else if (exprᴛ1 == reflect.Chan) {
        {
            var (c, ok) = nilCompare(aVal, bVal); if (ok) {
                return c;
            }
        }
        return cmp.Compare(aVal.Pointer(), bVal.Pointer());
    }
    else if (exprᴛ1 == reflect.Struct) {
        for (nint i = 0; i < aVal.NumField(); i++) {
            {
                nint c = compare(aVal.Field(i), bVal.Field(i)); if (c != 0) {
                    return c;
                }
            }
        }
        return 0;
    }
    else if (exprᴛ1 == reflect.Array) {
        for (nint i = 0; i < aVal.Len(); i++) {
            {
                nint c = compare(aVal.Index(i), bVal.Index(i)); if (c != 0) {
                    return c;
                }
            }
        }
        return 0;
    }
    else if (exprᴛ1 == reflect.ΔInterface) {
        {
            var (cΔ1, ok) = nilCompare(aVal, bVal); if (ok) {
                return cΔ1;
            }
        }
        nint c = compare(reflect.ValueOf(aVal.Elem().Type()), reflect.ValueOf(bVal.Elem().Type()));
        if (c != 0) {
            return c;
        }
        return compare(aVal.Elem(), bVal.Elem());
    }
    else { /* default: */
        throw panic("bad type in compare: " + aType.String());
    }

}

// Certain types cannot appear as keys (maps, funcs, slices), but be explicit.

// nilCompare checks whether either value is nil. If not, the boolean is false.
// If either value is nil, the boolean is true and the integer is the comparison
// value. The comparison is defined to be 0 if both are nil, otherwise the one
// nil value compares low. Both arguments must represent a chan, func,
// interface, map, pointer, or slice.
internal static (nint, bool) nilCompare(reflectꓸValue aVal, reflectꓸValue bVal) {
    if (aVal.IsNil()) {
        if (bVal.IsNil()) {
            return (0, true);
        }
        return (-1, true);
    }
    if (bVal.IsNil()) {
        return (1, true);
    }
    return (0, false);
}

} // end fmtsort_package
