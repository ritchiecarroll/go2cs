namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Row /*[3]nint*/;

partial struct holder {
    internal array<nint> arr = new(3);
}

internal static ж<array<nint>> leaked;
internal static ж<Row> leakedRow;

internal static array<nint> leakLocal() {
    ref var l = ref heap<array<nint>>(array<nint>([1, 2, 3]), out var Ꮡl);
    leaked = Ꮡl;
    return l.Clone();
}

internal static Row leakRow() {
    ref var r = ref heap<Row>(new Row(new nint[]{10, 20, 30}.array()), out var Ꮡr);
    leakedRow = Ꮡr;
    return r.Clone();
}

internal static array<nint> get(this holder h) {
    h = h.ΔClone();

    return h.arr.Clone();
}

internal static void modDirect(/*[3]*/ array<nint> a) {
    a = a.Clone();

    a[0] = 99;
}

internal static void modNamed(Row r) {
    r = r.Clone();

    r[0] = 99;
}

internal static void modDeep(/*[2][3]*/ array<array<nint>> m) {
    m = m.Clone();

    m[0][0] = 99;
}

internal static void mut(this Row r) {
    r = r.Clone();

    r[0] = 77;
}

internal static void rangeValues() {
    var m = new array<nint>[]{array<nint>([1, 2, 3]), array<nint>([4, 5, 6])}.array();

    foreach (var (_, vᴛ1) in range(m.ΔRangeSnapshot())) {
        var row = vᴛ1.Clone();

        row[0] = 99;
    }

    fmt.Println("rangeValues:", m);

    var s = GoReflect.WithElemDims(new array<nint>[]{array<nint>([7, 8, 9])}.slice(), 3);

    foreach (var (_, vᴛ2) in range(s)) {
        var row = vᴛ2.Clone();

        row[1] = 99;
    }

    fmt.Println("rangeSlice:", s);

    var deep = new array<array<nint>>[]{new array<nint>[]{array<nint>([1, 2, 3]), array<nint>([4, 5, 6])}.array(), new array<nint>[]{array<nint>([7, 8, 9]), array<nint>([10, 11, 12])}.array()}.array();

    foreach (var (_, vᴛ3) in range(deep.ΔRangeSnapshot())) {
        var plane = vᴛ3.Clone();

        plane[0][0] = 99;
    }

    fmt.Println("rangeDeep:", deep);
}

internal static void rangeNamed() {
    var rows = GoReflect.WithElemDims(new Row[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.slice(), 3);

    foreach (var (_, vᴛ1) in range(rows)) {
        var r = vᴛ1.Clone();

        r[0] = 99;
    }

    fmt.Println("rangeNamed:", rows[0][0], rows[1][0]);
}

internal static void rangeHeapBoxed() {
    var m = new array<nint>[]{array<nint>([1, 2, 3]), array<nint>([4, 5, 6])}.array();

    foreach (var (_, vᴛ1) in range(m.ΔRangeSnapshot())) {
        ref var row = ref heap(new array<nint>(3), out var Ꮡrow);
        row = vᴛ1.Clone();

        var p = Ꮡrow;
        p.Value[0] = 88;
    }

    fmt.Println("rangeHeapBoxed:", m);
}

internal static void rangeAssignExisting() {
    var m = new array<nint>[]{array<nint>([1, 2, 3]), array<nint>([4, 5, 6])}.array();
    array<nint> row = new(3);
    nint i = default;

    foreach (var (iᴛ1, vᴛ1) in range(m.ΔRangeSnapshot())) {
        i = iᴛ1;
        row = vᴛ1.Clone();

        row[2] = 99;
    }

    fmt.Println("rangeAssignExisting:", m, row, i);
}

internal static void mapKeyRange() {
    var mk = new map<array<nint>, @string>{[array<nint>([1, 2])] = "v"};

    foreach (var (kᴛ1, _) in mk) {
        var k = kᴛ1.Clone();

        k[0] = 99;
    }

    fmt.Println("mapKeyRange:", mk[array<nint>([1, 2])]);
}

internal static void compositeElements() {
    var a = array<nint>([1, 2, 3]);
    var b = array<nint>([4, 5, 6]);

    var m = new array<nint>[]{a.Clone(), b.Clone()}.array();
    m[0][0] = 99;
    fmt.Println("compositeArray:", a, m[0]);

    var s = GoReflect.WithElemDims(new array<nint>[]{a.Clone(), b.Clone()}.slice(), 3);
    s[1][0] = 99;
    fmt.Println("compositeSlice:", b, s[1]);
}

internal static void compositeStructFields() {
    var a = array<nint>([1, 2, 3]);

    var s1 = new holder(arr: a.Clone());
    s1.arr[0] = 99;
    fmt.Println("structKeyed:", a, s1.arr);

    var s2 = new holder(a.Clone());
    a[1] = 88;
    fmt.Println("structPositional:", a, s2.arr);
}

internal static void compositeMapValueAndKey() {
    var a = array<nint>([1, 2, 3]);
    var mv = new map<@string, array<nint>>{["x"] = a.Clone()};
    a[0] = 99;
    fmt.Println("mapValue:", mv["x", () => new array<nint>(3)]);

    var k = array<nint>([1, 2]);
    var mk = new map<array<nint>, @string>{[k.Clone()] = "kv"};
    k[0] = 99;
    fmt.Println("mapKeyLiteral:", mk[array<nint>([1, 2])]);
}

internal static void compositeSparseAndAny() {
    var a = array<nint>([1, 2, 3]);
    var sp = new array<array<nint>>(4, () => new(3)){[2] = a.Clone()};
    a[1] = 88;
    fmt.Println("sparse:", sp[2]);

    var b = array<nint>([7, 8, 9]);
    var lst = slice<any>([b.Clone()]);
    b[0] = 99;
    var got = lst[0]._<array<nint>>();
    fmt.Println("anyBoxed:", got);
}

internal static void returnCopies() {
    var r = leakLocal();
    (leaked.Value)[0] = 99;
    fmt.Println("returnLeak:", r, leaked.Value);

    var h = new holder(arr: array<nint>([5, 6, 7]));
    var g = h.get();
    g[0] = 99;
    fmt.Println("returnField:", h.arr, g);

    var nr = leakRow();
    (leakedRow.Value)[1] = 99;
    fmt.Println("returnNamed:", nr[0], nr[1], nr[2], (leakedRow.Value)[1]);
}

internal static void paramCopies() {
    var a = array<nint>([1, 2, 3]);
    modDirect(a);
    fmt.Println("paramDirect:", a);

    var nr = new Row(new nint[]{4, 5, 6}.array());
    modNamed(nr);
    fmt.Println("paramNamed:", nr[0]);

    var m = new array<nint>[]{array<nint>([1, 2, 3]), array<nint>([4, 5, 6])}.array();
    modDeep(m);
    fmt.Println("paramDeep:", m);

    nr.mut();
    fmt.Println("recvValue:", nr[0]);
}

internal static void funcLitParam() {
    var a = array<nint>([1, 2, 3]);
    void fl([GoArrayDims(3)] array<nint> x) {
        x = x.Clone();
        x[0] = 99;
    }
    fl(a);
    fmt.Println("funcLitParam:", a);
}

internal static void namedAssignCopies() {
    var nr = new Row(new nint[]{1, 2, 3}.array());
    var d = nr.Clone();
    d[0] = 99;
    fmt.Println("namedAssign:", nr[0], d[0]);

    Row e = nr.Clone();
    e[1] = 88;
    fmt.Println("namedVarDecl:", nr[1], e[1]);

    any x = nr.Clone();
    nr[2] = 77;
    var got = x._<Row>();
    fmt.Println("namedAnyBoxed:", got[2], nr[2]);

    var a = array<nint>([5, 6, 7]);
    any y = a.Clone();
    a[0] = 99;
    var gotA = y._<array<nint>>();
    fmt.Println("directAnyBoxed:", gotA[0], a[0]);
}

internal static void channelSend() {
    var a = array<nint>([1, 2, 3]);
    var ch = new channel<array<nint>>(1, ChanCargo.Of(null, new nint[] { 3 }));
    ch.ᐸꟷ(a.Clone());
    a[0] = 99;
    var got = ᐸꟷ(ch);
    fmt.Println("channelSend:", got, a);
}

internal static void appendElement() {
    var sᴷ = new stackAppend<array<nint>>();

    var a = array<nint>([1, 2, 3]);
    slice<array<nint>> s = default;
    s = append(sᴷ, s, a.Clone());
    a[0] = 99;
    fmt.Println("appendElement:", s[0], a);
}

internal static void main() {
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
