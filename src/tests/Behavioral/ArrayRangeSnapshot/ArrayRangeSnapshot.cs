namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[4]nint")] partial struct Row;

[GoType("[]nint")] partial struct Digits;

[GoType] partial struct holder {
    internal array<nint> arr = new(4);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object arrayValueˢ = (@string)"arrayValue"u8;
private static readonly object arrayValueAfterˢ = (@string)"arrayValue after:"u8;

internal static void arrayValue() {
    var a = new nint[]{1, 2, 3, 4}.array();
    foreach (var (i, v) in a.ΔRangeSnapshot()) {
        if (i == 0) {
            (a[1], a[2], a[3]) = (91, 92, 93);
        }
        fmt.Println(arrayValueˢ, i, v);
    }
    fmt.Println(arrayValueAfterˢ, a);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object namedArrayValueˢ = (@string)"namedArrayValue"u8;
private static readonly object namedArrayValueAfterˢ = (@string)"namedArrayValue after:"u8;

internal static void namedArrayValue() {
    var r = new Row(new nint[]{1, 2, 3, 4}.array());
    foreach (var (i, v) in r.ΔRangeSnapshot()) {
        if (i == 0) {
            (r[1], r[2], r[3]) = (91, 92, 93);
        }
        fmt.Println(namedArrayValueˢ, i, v);
    }
    fmt.Println(namedArrayValueAfterˢ, r);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object arrayFieldˢ = (@string)"arrayField"u8;
private static readonly object arrayFieldAfterˢ = (@string)"arrayField after:"u8;

internal static void arrayField() {
    var h = new holder(arr: new nint[]{1, 2, 3, 4}.array());
    foreach (var (i, v) in h.arr.ΔRangeSnapshot()) {
        if (i == 0) {
            (h.arr[1], h.arr[2], h.arr[3]) = (91, 92, 93);
        }
        fmt.Println(arrayFieldˢ, i, v);
    }
    fmt.Println(arrayFieldAfterˢ, h.arr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object arrayOfArraysˢ = (@string)"arrayOfArrays"u8;
private static readonly object arrayOfArraysAfterˢ = (@string)"arrayOfArrays after:"u8;

internal static void arrayOfArrays() {
    var m = new array<nint>[]{new nint[]{1, 2}.array(), new nint[]{3, 4}.array(), new nint[]{5, 6}.array()}.array();
    foreach (var (i, vᴛ1) in m.ΔRangeSnapshot()) {
        var row = vᴛ1.Clone();

        if (i == 0) {
            (m[1][0], m[2][0]) = (91, 92);
        }
        fmt.Println(arrayOfArraysˢ, i, row);
    }
    fmt.Println(arrayOfArraysAfterˢ, m);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object pointerArrayˢ = (@string)"pointerArray"u8;
private static readonly object pointerArrayAfterˢ = (@string)"pointerArray after:"u8;

internal static void pointerArray() {
    ref var b = ref heap<array<nint>>(out var Ꮡb);
    b = new nint[]{1, 2, 3, 4}.array();
    var p = Ꮡb;
    foreach (var (i, v) in p.Value) {
        if (i == 0) {
            (b[1], b[2], b[3]) = (91, 92, 93);
        }
        fmt.Println(pointerArrayˢ, i, v);
    }
    fmt.Println(pointerArrayAfterˢ, b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sliceValueˢ = (@string)"sliceValue"u8;
private static readonly object sliceValueAfterˢ = (@string)"sliceValue after:"u8;

internal static void sliceValue() {
    var s = new nint[]{1, 2, 3, 4}.slice();
    foreach (var (i, v) in s) {
        if (i == 0) {
            (s[1], s[2], s[3]) = (91, 92, 93);
        }
        fmt.Println(sliceValueˢ, i, v);
    }
    fmt.Println(sliceValueAfterˢ, s);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object indexOnlyˢ = (@string)"indexOnly"u8;

internal static void indexOnly() {
    var a = new nint[]{1, 2, 3, 4}.array();
    foreach (var (i, _) in a) {
        if (i == 0) {
            (a[1], a[2], a[3]) = (91, 92, 93);
        }
        fmt.Println(indexOnlyˢ, i, a[i]);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object assignVarsˢ = (@string)"assignVars"u8;

internal static void assignVars() {
    var a = new nint[]{1, 2, 3, 4}.array();
    nint i = default!;
    nint v = default!;
    foreach (var (iᴛ1, vᴛ1) in a.ΔRangeSnapshot()) {
        i = iᴛ1;
        v = vᴛ1;

        if (i == 0) {
            (a[1], a[2], a[3]) = (91, 92, 93);
        }
        fmt.Println(assignVarsˢ, i, v);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object mutableRangeVarˢ = (@string)"mutableRangeVar"u8;

internal static void mutableRangeVar() {
    var a = new nint[]{1, 2, 3, 4}.array();
    foreach (var (i, vᴛ1) in a.ΔRangeSnapshot()) {
        var v = vᴛ1;

        if (i == 0) {
            (a[1], a[2], a[3]) = (91, 92, 93);
        }
        v *= 10;
        fmt.Println(mutableRangeVarˢ, i, v);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object aliasedElementˢ = (@string)"aliasedElement"u8;
private static readonly object aliasedElementAfterˢ = (@string)"aliasedElement after:"u8;

internal static void aliasedElement() {
    ref var a = ref heap<array<nint>>(out var Ꮡa);
    a = new nint[]{1, 2, 3, 4}.array();
    var q = Ꮡa.at<nint>(2);
    foreach (var (i, v) in a.ΔRangeSnapshot()) {
        if (i == 0) {
            q.Value = 90;
        }
        fmt.Println(aliasedElementˢ, i, v);
    }
    fmt.Println(aliasedElementAfterˢ, a);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object arrayOfNamedSlicesˢ = (@string)"arrayOfNamedSlices"u8;
private static readonly object arrayOfNamedSlicesAfterˢ = (@string)"arrayOfNamedSlices after:"u8;

internal static void arrayOfNamedSlices() {
    var rows = new Digits[]{new nint[]{1, 2}.slice(), new nint[]{3, 4}.slice(), new nint[]{5, 6}.slice()}.array();
    foreach (var (i, r) in rows.ΔRangeSnapshot()) {
        if (i == 0) {
            rows[1] = new Digits(new nint[]{91, 92}.slice());
            rows[2][0] = 93;
        }
        fmt.Println(arrayOfNamedSlicesˢ, i, r);
    }
    fmt.Println(arrayOfNamedSlicesAfterˢ, rows);
}

internal static void Main() {
    arrayValue();
    namedArrayValue();
    arrayField();
    arrayOfArrays();
    pointerArray();
    sliceValue();
    indexOnly();
    assignVars();
    mutableRangeVar();
    aliasedElement();
    arrayOfNamedSlices();
}

} // end main_package
