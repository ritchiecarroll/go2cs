global using aliasArr = go.array<ulong>;

namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct holder {
    internal int32 n;
    internal array<uint64> x = new(8);
}

partial struct outerHolder {
    internal int32 k;
    internal holder h;
}

partial struct named /*[8]uint64*/;

internal static readonly ж<array<uint64>> Ꮡglobal = new StaticBox<array<uint64>>(new array<uint64>(8));
internal static ref array<uint64> global => ref ((StaticBox<array<uint64>>)Ꮡglobal).ValueSlot;

internal static array<uint64> mk() { return array<uint64>(8, [11, 12]); }

internal static (array<uint64>, error) pair() { return (array<uint64>(8, [17, 18]), default); }

internal static void variable() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var b = array<uint64>([1, 2, 3, 4, 5, 6, 7, 8]);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    a.Assign(b);
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("variable:", a[0], f.ElementRef(2));
}

internal static void literal() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    a.Assign(array<uint64>(8, [9, 9, 9]));
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("literal :", a[0], a[1], f.ElementRef(2));
}

internal static void call() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    a.Assign(mk());
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("call    :", a[0], f.ElementRef(2));
}

internal static void tuple() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    nint n = default;
    (var aᴛ1, n) = (array<uint64>(8, [21, 22]), 1);
    a.Assign(aᴛ1);
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("tuple   :", a[0], f.ElementRef(2), n);
}

internal static void swap() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var b = array<uint64>(8, [31, 32]);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    (var aᴛ1, var bᴛ1) = (b.Clone(), a.Clone());
    a.Assign(aᴛ1);
    b.Assign(bᴛ1);
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("swap    :", a[0], f.ElementRef(2), b[0]);
}

internal static void field() {
    ref var h = ref heap(new holder(), out var Ꮡh);
    var f = @unsafe.ArrayPointer<uint32>.Of(Ꮡh.Ꮡx, 16);
    h.x.Assign(array<uint64>(8, [41, 42]));
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("field   :", h.x[0], f.ElementRef(2));
}

internal static void structWhole() {
    ref var h = ref heap(new holder(), out var Ꮡh);
    var f = @unsafe.ArrayPointer<uint32>.Of(Ꮡh.Ꮡx, 16);
    h.AssignFrom(new holder(n: 1, x: array<uint64>(8, [5, 6])));
    f.ElementRef(0) = 0xBBBB;
    fmt.Println("struct  :", h.x[0], f.ElementRef(2), h.n);
}

internal static void element() {
    ref var m = ref heap(new array<array<uint64>>(2, () => new(8)), out var Ꮡm);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡm.at<array<uint64>>(1)), 16);
    m[1].Assign(array<uint64>(8, [51, 52]));
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("element :", m[1][0], f.ElementRef(2));
}

internal static void outer() {
    ref var m = ref heap(new array<array<uint64>>(2, () => new(8)), out var Ꮡm);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡm.at<array<uint64>>(1)), 16);
    m.Assign(new array<uint64>[]{new uint64[]{}.array(8), array<uint64>(8, [61, 62])}.array());
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("outer   :", m[1][0], f.ElementRef(2));
}

internal static void throughPointer() {
    var p = Ꮡ(new array<uint64>(8));
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(p), 16);
    p.Assign(array<uint64>(8, [7, 8]));
    f.ElementRef(0) = 0xCCCC;
    fmt.Println("pointer :", p.Value[0], f.ElementRef(2));
}

internal static void glob() {
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡglobal), 16);
    global.Assign(array<uint64>(8, [3, 4]));
    f.ElementRef(0) = 0xDDDD;
    fmt.Println("global  :", global[0], f.ElementRef(2));
}

internal static void sliceElement() {
    var hs = new slice<holder>(2, () => new());
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡ(hs, 1).Ꮡx), 16);
    hs[1].AssignFrom(new holder(n: 2, x: array<uint64>(8, [71, 72])));
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("slice el:", hs[1].x[0], f.ElementRef(2));
}

internal static void forClause() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var b = array<uint64>(8, [101, 102]);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    nint n = default;
    for ((var aᴛ1, n) = (b.Clone(), 0), a.Assign(aᴛ1); n < 1; n++) {
        f.ElementRef(0) = 0xAAAA;
    }
    fmt.Println("for cl  :", a[0], f.ElementRef(2), n);
}

internal static void forPost() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var b = array<uint64>(8, [171, 172]);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    nint n = 0;
    for (; n < 2; ) {
        if (n == 0) {
            goto continueᴛ1;
        }
continueᴛ1:;
        array<uint64> aᴛ1;
        (aᴛ1, n) = (b.Clone(), n + 1); a.Assign(aᴛ1);
    }
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("for post:", a[0], f.ElementRef(2), n);
}

internal static void forField() {
    ref var h = ref heap(new holder(), out var Ꮡh);
    var f = @unsafe.ArrayPointer<uint32>.Of(Ꮡh.Ꮡx, 16);
    nint n = default;
    ref var hᴛ1 = ref h.x;
    for ((var hᴛ2, n) = (array<uint64>(8, [181, 182]), 0), hᴛ1.Assign(hᴛ2); n < 1; n++) {
        f.ElementRef(0) = 0xAAAA;
    }
    fmt.Println("for fld :", h.x[0], f.ElementRef(2), n);
}

internal static void ifInit() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var b = array<uint64>(8, [111, 112]);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    nint n = default;
    (var aᴛ1, n) = (b.Clone(), 1);
    a.Assign(aᴛ1); if (n > 0) {
        f.ElementRef(0) = 0xAAAA;
    }
    fmt.Println("if init :", a[0], f.ElementRef(2), n);
}

internal static void switchInit() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var b = array<uint64>(8, [121, 122]);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    nint n = default;
    (var aᴛ1, n) = (b.Clone(), 1);
    a.Assign(aᴛ1);
    switch (n) {
    case 1: {
        f.ElementRef(0) = 0xAAAA;
        break;
    }}

    fmt.Println("sw init :", a[0], f.ElementRef(2), n);
}

internal static void multiValue() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    error err = default;
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    (var aᴛ1, err) = pair();
    a.Assign(aᴛ1);
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("multi   :", a[0], f.ElementRef(2), err == nil);
}

internal static void rangeAssign() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var arrs = GoReflect.WithElemDims(new array<uint64>[]{array<uint64>(8, [131, 132])}.slice(), 8);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    foreach (var (_, vᴛ1) in range(arrs)) {
        a.Assign(vᴛ1);

        f.ElementRef(0) = 0xAAAA;
    }
    fmt.Println("range   :", a[0], f.ElementRef(2));
}

internal static void nested() {
    ref var o = ref heap(new outerHolder(), out var Ꮡo);
    var f = @unsafe.ArrayPointer<uint32>.Of(Ꮡo.Ꮡh.Ꮡx, 16);
    o.AssignFrom(new outerHolder(k: 1, h: new holder(n: 2, x: array<uint64>(8, [141, 142]))));
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("nested  :", o.h.x[0], f.ElementRef(2), o.k, o.h.n);
}

internal static void pointerStruct() {
    var ph = @new<holder>();
    var f = @unsafe.ArrayPointer<uint32>.Of(ph.Ꮡx, 16);
    ph.Value.AssignFrom(new holder(n: 3, x: array<uint64>(8, [151, 152])));
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("ptr str :", ph.x[0], f.ElementRef(2), ph.n);
}

internal static void namedArray() {
    ref var a = ref heap(new named(), out var Ꮡa);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    a.Assign(new named(new uint64[]{161, 162}.array(8)));
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("named   :", a[0], f.ElementRef(2));
}

internal static void selectRecv() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    var ch = new channel<array<uint64>>(1, ChanCargo.Of(null, new nint[] { 8 }));
    ch.ᐸꟷ(array<uint64>(8, [211, 212]));
    var chᴛ1 = ch;
    switch (select(ᐸꟷ(chᴛ1, ꓸꓸꓸ))) {
    case 0 when chᴛ1.ꟷᐳ(out var aᴛ1): {
        a.Assign(aᴛ1);
        break;
    }}
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("select  :", a[0], f.ElementRef(2));
}

internal static void recvOk() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    bool ok = default;
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    var ch = new channel<array<uint64>>(1, ChanCargo.Of(null, new nint[] { 8 }));
    ch.ᐸꟷ(array<uint64>(8, [221, 222]));
    (var aᴛ1, ok) = ᐸꟷ(ch, ꟷ);
    a.Assign(aᴛ1);
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("recv ok :", a[0], f.ElementRef(2), ok);
}

internal static array<uint64> /*a*/ namedResultIndex() {
    heap(new array<uint64>(8), out var Ꮡa);
    GoFrame ᒐ = default;
    try {
        ref var a = ref Ꮡa.Value;

        var p = Ꮡa;
        defer(() => {
            Ꮡa.Value[0] = 0xBBBB;
        }, ref ᒐ);
        p.Value[1] = 202;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return Ꮡa.Value;
}

internal static void forSingle() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var b = array<uint64>(8, [201, 202]);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    nint n = 0;
    for (a.Assign(b); n < 1; n++) {
        f.ElementRef(0) = 0xAAAA;
    }
    fmt.Println("for one :", a[0], f.ElementRef(2), n);
}

internal static void forSinglePost() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var b = array<uint64>(8, [211, 212]);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    nint n = 0;
    for (; n < 2; a.Assign(b)) {
        n++;
    }
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("for 1pst:", a[0], f.ElementRef(2), n);
}

internal static void forLabelled() {
    ref var a = ref heap(new array<uint64>(8), out var Ꮡa);
    var b = array<uint64>(8, [221, 222]);
    var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
    nint n = 0;
outer:
    for (; n < 2; ) {
        for (nint k = 0; k < 2; k++) {
            if (k == 0) {
                goto continue_outer;
            }
        }
continue_outer:;
        array<uint64> aᴛ1;
        (aᴛ1, n) = (b.Clone(), n + 1); a.Assign(aᴛ1);
    }
break_outer:;
    f.ElementRef(0) = 0xAAAA;
    fmt.Println("for lbl :", a[0], f.ElementRef(2), n);
}

internal static holder /*h*/ namedStructResultIndex() {
    heap<holder>(out var Ꮡh);
    GoFrame ᒐ = default;
    try {
        ref var h = ref Ꮡh.Value;

        var p = Ꮡh;
        defer(() => {
            Ꮡh.Value.x[0] = 0xDDDD;
        }, ref ᒐ);
        p.x[1] = 404;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return Ꮡh.Value;
}

internal static aliasArr /*a*/ aliasedResultIndex() {
    heap(new array<uint64>(8), out var Ꮡa);
    GoFrame ᒐ = default;
    try {
        ref var a = ref Ꮡa.Value;

        var p = Ꮡa;
        defer(() => {
            Ꮡa.Value[0] = 0xCCCC;
        }, ref ᒐ);
        p.Value[1] = 303;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return Ꮡa.Value;
}

internal static void main() {
    variable();
    literal();
    call();
    tuple();
    swap();
    field();
    structWhole();
    element();
    outer();
    throughPointer();
    glob();
    sliceElement();
    forClause();
    forPost();
    forField();
    ifInit();
    switchInit();
    multiValue();
    rangeAssign();
    nested();
    pointerStruct();
    namedArray();
    selectRecv();
    recvOk();
    var r = namedResultIndex();
    fmt.Println("res idx :", r[0], r[1], len(r));
    forSingle();
    forSinglePost();
    forLabelled();
    var h = namedStructResultIndex();
    fmt.Println("res sidx:", h.x[0], h.x[1], len(h.x));
    var ra = aliasedResultIndex();
    fmt.Println("res aidx:", ra[0], ra[1], len(ra));
}

} // end main_package
