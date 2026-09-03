namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[3]nint")] partial struct Row;

[GoType] partial struct holder {
    internal array<nint> arr = new(3);
}

internal static ж<array<nint>> leaked;

internal static ж<Row> leakedRow;

internal static array<nint> leakLocal() {
    ref var l = ref heap<array<nint>>(out var Ꮡl);
    l = new nint[]{1, 2, 3}.array();
    leaked = Ꮡl;
    return l.Clone();
}

internal static Row leakRow() {
    ref var r = ref heap<Row>(out var Ꮡr);
    r = new Row(new nint[]{10, 20, 30}.array());
    leakedRow = Ꮡr;
    return r.Clone();
}

internal static array<nint> get(this holder h) {
    h = h.ΔClone();

    return h.arr.Clone();
}

internal static void modDirect([GoArrayDims(3)] array<nint> a) {
    a = a.Clone();

    a[0] = 99;
}

internal static void modNamed(Row r) {
    r = r.Clone();

    r[0] = 99;
}

internal static void modDeep([GoArrayDims(2, 3)] array<array<nint>> m) {
    m = m.Clone();

    m[0][0] = 99;
}

internal static void mut(this Row r) {
    r = r.Clone();

    r[0] = 77;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object rangeValuesˢ = (@string)"rangeValues:"u8;
private static readonly object rangeSliceˢ = (@string)"rangeSlice:"u8;
private static readonly object rangeDeepˢ = (@string)"rangeDeep:"u8;

internal static void rangeValues() {
    var m = new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array();
    foreach (var (_, vᴛ1) in m.ΔRangeSnapshot()) {
        var row = vᴛ1.Clone();

        row[0] = 99;
    }
    fmt.Println(rangeValuesˢ, m);
    var s = new array<nint>[]{new nint[]{7, 8, 9}.array()}.slice();
    foreach (var (_, vᴛ2) in s) {
        var row = vᴛ2.Clone();

        row[1] = 99;
    }
    fmt.Println(rangeSliceˢ, s);
    var deep = new array<array<nint>>[]{new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array(), new array<nint>[]{new nint[]{7, 8, 9}.array(), new nint[]{10, 11, 12}.array()}.array()}.array();
    foreach (var (_, vᴛ3) in deep.ΔRangeSnapshot()) {
        var plane = vᴛ3.Clone();

        plane[0][0] = 99;
    }
    fmt.Println(rangeDeepˢ, deep);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object rangeNamedˢ = (@string)"rangeNamed:"u8;

internal static void rangeNamed() {
    var rows = new Row[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.slice();
    foreach (var (_, vᴛ1) in rows) {
        var r = vᴛ1.Clone();

        r[0] = 99;
    }
    fmt.Println(rangeNamedˢ, rows[0][0], rows[1][0]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object rangeHeapBoxedˢ = (@string)"rangeHeapBoxed:"u8;

internal static void rangeHeapBoxed() {
    var m = new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array();
    foreach (var (_, vᴛ1) in m.ΔRangeSnapshot()) {
        ref var row = ref heap(new array<nint>(3), out var Ꮡrow);
        row = vᴛ1.Clone();

        var p = Ꮡrow;
        p.Value[0] = 88;
    }
    fmt.Println(rangeHeapBoxedˢ, m);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object rangeAssignExistingˢ = (@string)"rangeAssignExisting:"u8;

internal static void rangeAssignExisting() {
    var m = new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array();
    array<nint> row = new(3);
    nint i = default!;
    foreach (var (iᴛ1, vᴛ1) in m.ΔRangeSnapshot()) {
        i = iᴛ1;
        row = vᴛ1.Clone();

        row[2] = 99;
    }
    fmt.Println(rangeAssignExistingˢ, m, row, i);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object mapKeyRangeˢ = (@string)"mapKeyRange:"u8;

internal static void mapKeyRange() {
    var mk = new map<array<nint>, @string>{[new nint[]{1, 2}.array()] = "v"u8};
    foreach (var (kᴛ1, _) in mk) {
        var k = kᴛ1.Clone();

        k[0] = 99;
    }
    fmt.Println(mapKeyRangeˢ, mk[new nint[]{1, 2}.array()]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object compositeArrayˢ = (@string)"compositeArray:"u8;
private static readonly object compositeSliceˢ = (@string)"compositeSlice:"u8;

internal static void compositeElements() {
    var a = new nint[]{1, 2, 3}.array();
    var b = new nint[]{4, 5, 6}.array();
    var m = new array<nint>[]{a.Clone(), b.Clone()}.array();
    m[0][0] = 99;
    fmt.Println(compositeArrayˢ, a, m[0]);
    var s = new array<nint>[]{a.Clone(), b.Clone()}.slice();
    s[1][0] = 99;
    fmt.Println(compositeSliceˢ, b, s[1]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object structKeyedˢ = (@string)"structKeyed:"u8;
private static readonly object structPositionalˢ = (@string)"structPositional:"u8;

internal static void compositeStructFields() {
    var a = new nint[]{1, 2, 3}.array();
    var s1 = new holder(arr: a.Clone());
    s1.arr[0] = 99;
    fmt.Println(structKeyedˢ, a, s1.arr);
    var s2 = new holder(a.Clone());
    a[1] = 88;
    fmt.Println(structPositionalˢ, a, s2.arr);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object mapValueˢ = (@string)"mapValue:"u8;
private static readonly object mapKeyLiteralˢ = (@string)"mapKeyLiteral:"u8;

internal static void compositeMapValueAndKey() {
    var a = new nint[]{1, 2, 3}.array();
    var mv = new map<@string, array<nint>>{["x"u8] = a.Clone()};
    a[0] = 99;
    fmt.Println(mapValueˢ, mv["x"u8, () => new array<nint>(3)]);
    var k = new nint[]{1, 2}.array();
    var mk = new map<array<nint>, @string>{[k.Clone()] = "kv"u8};
    k[0] = 99;
    fmt.Println(mapKeyLiteralˢ, mk[new nint[]{1, 2}.array()]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sparseˢ = (@string)"sparse:"u8;
private static readonly object anyBoxedˢ = (@string)"anyBoxed:"u8;

internal static void compositeSparseAndAny() {
    var a = new nint[]{1, 2, 3}.array();
    var sp = new array<array<nint>>(4, () => new(3)){[2] = a.Clone()};
    a[1] = 88;
    fmt.Println(sparseˢ, sp[2]);
    var b = new nint[]{7, 8, 9}.array();
    var lst = new any[]{b.Clone()}.slice();
    b[0] = 99;
    var got = lst[0]._<array<nint>>();
    fmt.Println(anyBoxedˢ, got);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object returnLeakˢ = (@string)"returnLeak:"u8;
private static readonly object returnFieldˢ = (@string)"returnField:"u8;
private static readonly object returnNamedˢ = (@string)"returnNamed:"u8;

internal static void returnCopies() {
    var r = leakLocal();
    (leaked.Value)[0] = 99;
    fmt.Println(returnLeakˢ, r, leaked.Value);
    var h = new holder(arr: new nint[]{5, 6, 7}.array());
    var g = h.get();
    g[0] = 99;
    fmt.Println(returnFieldˢ, h.arr, g);
    var nr = leakRow();
    (leakedRow.Value)[1] = 99;
    fmt.Println(returnNamedˢ, nr[0], nr[1], nr[2], (leakedRow.Value)[1]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object paramDirectˢ = (@string)"paramDirect:"u8;
private static readonly object paramNamedˢ = (@string)"paramNamed:"u8;
private static readonly object paramDeepˢ = (@string)"paramDeep:"u8;
private static readonly object recvValueˢ = (@string)"recvValue:"u8;

internal static void paramCopies() {
    var a = new nint[]{1, 2, 3}.array();
    modDirect(a);
    fmt.Println(paramDirectˢ, a);
    var nr = new Row(new nint[]{4, 5, 6}.array());
    modNamed(nr);
    fmt.Println(paramNamedˢ, nr[0]);
    var m = new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array();
    modDeep(m);
    fmt.Println(paramDeepˢ, m);
    nr.mut();
    fmt.Println(recvValueˢ, nr[0]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object funcLitParamˢ = (@string)"funcLitParam:"u8;

internal static void funcLitParam() {
    var a = new nint[]{1, 2, 3}.array();
    void fl([GoArrayDims(3)] array<nint> x) {
        x = x.Clone();
        x[0] = 99;
    }
    fl(a);
    fmt.Println(funcLitParamˢ, a);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object namedAssignˢ = (@string)"namedAssign:"u8;
private static readonly object namedVarDeclˢ = (@string)"namedVarDecl:"u8;
private static readonly object namedAnyBoxedˢ = (@string)"namedAnyBoxed:"u8;
private static readonly object directAnyBoxedˢ = (@string)"directAnyBoxed:"u8;

internal static void namedAssignCopies() {
    var nr = new Row(new nint[]{1, 2, 3}.array());
    var d = nr.Clone();
    d[0] = 99;
    fmt.Println(namedAssignˢ, nr[0], d[0]);
    Row e = nr.Clone();
    e[1] = 88;
    fmt.Println(namedVarDeclˢ, nr[1], e[1]);
    any x = nr.Clone();
    nr[2] = 77;
    var got = x._<Row>();
    fmt.Println(namedAnyBoxedˢ, got[2], nr[2]);
    var a = new nint[]{5, 6, 7}.array();
    any y = a.Clone();
    a[0] = 99;
    var gotA = y._<array<nint>>();
    fmt.Println(directAnyBoxedˢ, gotA[0], a[0]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object channelSendˢ = (@string)"channelSend:"u8;

internal static void channelSend() {
    var a = new nint[]{1, 2, 3}.array();
    var ch = new channel<array<nint>>(1);
    ch.ᐸꟷ(a.Clone());
    a[0] = 99;
    var got = ᐸꟷ(ch);
    fmt.Println(channelSendˢ, got, a);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object appendElementˢ = (@string)"appendElement:"u8;

internal static void appendElement() {
    var a = new nint[]{1, 2, 3}.array();
    slice<array<nint>> s = default!;
    s = append(s, a.Clone());
    a[0] = 99;
    fmt.Println(appendElementˢ, s[0], a);
}

internal static void Main() {
    rangeValues();
    rangeNamed();
    rangeHeapBoxed();
    rangeAssignExisting();
    mapKeyRange();
    compositeElements();
    compositeStructFields();
    compositeMapValueAndKey();
    compositeSparseAndAny();
    returnCopies();
    paramCopies();
    funcLitParam();
    namedAssignCopies();
    channelSend();
    appendElement();
}

} // end main_package
