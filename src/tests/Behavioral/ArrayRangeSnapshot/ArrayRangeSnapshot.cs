namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Row /*[4]nint*/;

partial struct Digits /*[]nint*/;

partial struct holder {
    internal array<nint> arr = new(4);
}

internal static void arrayValue() {
    var a = new nint[]{1, 2, 3, 4}.array();

    foreach (var (i, v) in a.ΔRangeSnapshot()) {
        if (i == 0) {
            (a[1], a[2], a[3]) = (91, 92, 93);
        }

        fmt.Println("arrayValue", i, v);
    }

    fmt.Println("arrayValue after:", a);
}

internal static void namedArrayValue() {
    var r = new Row(new nint[]{1, 2, 3, 4}.array());

    foreach (var (i, v) in r.ΔRangeSnapshot()) {
        if (i == 0) {
            (r[1], r[2], r[3]) = (91, 92, 93);
        }

        fmt.Println("namedArrayValue", i, v);
    }

    fmt.Println("namedArrayValue after:", r);
}

internal static void arrayField() {
    var h = new holder(arr: new nint[]{1, 2, 3, 4}.array());

    foreach (var (i, v) in h.arr.ΔRangeSnapshot()) {
        if (i == 0) {
            (h.arr[1], h.arr[2], h.arr[3]) = (91, 92, 93);
        }

        fmt.Println("arrayField", i, v);
    }

    fmt.Println("arrayField after:", h.arr);
}

internal static void arrayOfArrays() {
    var m = new array<nint>[]{new nint[]{1, 2}.array(), new nint[]{3, 4}.array(), new nint[]{5, 6}.array()}.array();

    foreach (var (i, vᴛ1) in m.ΔRangeSnapshot()) {
        var row = vᴛ1.Clone();

        if (i == 0) {
            (m[1][0], m[2][0]) = (91, 92);
        }

        fmt.Println("arrayOfArrays", i, row);
    }

    fmt.Println("arrayOfArrays after:", m);
}

internal static void pointerArray() {
    ref var b = ref heap<array<nint>>(out var Ꮡb);
    b = new nint[]{1, 2, 3, 4}.array();
    var p = Ꮡb;

    foreach (var (i, v) in p.Value) {
        if (i == 0) {
            (b[1], b[2], b[3]) = (91, 92, 93);
        }

        fmt.Println("pointerArray", i, v);
    }

    fmt.Println("pointerArray after:", b);
}

internal static void sliceValue() {
    var s = new nint[]{1, 2, 3, 4}.slice();

    foreach (var (i, v) in s) {
        if (i == 0) {
            (s[1], s[2], s[3]) = (91, 92, 93);
        }

        fmt.Println("sliceValue", i, v);
    }

    fmt.Println("sliceValue after:", s);
}

internal static void indexOnly() {
    var a = new nint[]{1, 2, 3, 4}.array();

    foreach (var (i, _) in a) {
        if (i == 0) {
            (a[1], a[2], a[3]) = (91, 92, 93);
        }

        fmt.Println("indexOnly", i, a[i]);
    }
}

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

        fmt.Println("assignVars", i, v);
    }
}

internal static void mutableRangeVar() {
    var a = new nint[]{1, 2, 3, 4}.array();

    foreach (var (i, vᴛ1) in a.ΔRangeSnapshot()) {
        var v = vᴛ1;

        if (i == 0) {
            (a[1], a[2], a[3]) = (91, 92, 93);
        }

        v *= 10;

        fmt.Println("mutableRangeVar", i, v);
    }
}

internal static void aliasedElement() {
    ref var a = ref heap<array<nint>>(out var Ꮡa);
    a = new nint[]{1, 2, 3, 4}.array();
    var q = Ꮡa.at<nint>(2);

    foreach (var (i, v) in a.ΔRangeSnapshot()) {
        if (i == 0) {
            q.Value = 90;
        }

        fmt.Println("aliasedElement", i, v);
    }

    fmt.Println("aliasedElement after:", a);
}

internal static void arrayOfNamedSlices() {
    var rows = new Digits[]{new nint[]{1, 2}.slice(), new nint[]{3, 4}.slice(), new nint[]{5, 6}.slice()}.array();

    foreach (var (i, r) in rows.ΔRangeSnapshot()) {
        if (i == 0) {
            rows[1] = new Digits(new nint[]{91, 92}.slice());
            rows[2][0] = 93;
        }

        fmt.Println("arrayOfNamedSlices", i, r);
    }

    fmt.Println("arrayOfNamedSlices after:", rows);
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
