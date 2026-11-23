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
        defer([GoFunc("namedResultIndex.func1")] () => {
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
        defer([GoFunc("namedStructResultIndex.func1")] () => {
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
        defer([GoFunc("aliasedResultIndex.func1")] () => {
            Ꮡa.Value[0] = 0xCCCC;
        }, ref ᒐ);
        p.Value[1] = 303;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return Ꮡa.Value;
}

internal static array<uint64> /*a*/ retSlice() {
    array<uint64> a = new(8);
    GoFrame ᒐ = default;
    try {
        var s = a[..];
        var sʗ1 = s;
        defer([GoFunc("retSlice.func1")] () => {
            sʗ1[0] = 7;
        }, ref ᒐ);
        a.Assign(array<uint64>(8, [1, 2]));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return a;
}

internal static array<uint64> /*a*/ retElemPtr() {
    heap(new array<uint64>(8), out var Ꮡa);
    GoFrame ᒐ = default;
    try {
        ref var a = ref Ꮡa.Value;

        var p = Ꮡa.at<uint64>(1);
        var pʗ1 = p;
        defer([GoFunc("retElemPtr.func1")] () => {
            pʗ1.Value = 9;
        }, ref ᒐ);
        a.Assign(array<uint64>(8, [1, 2]));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return Ꮡa.Value;
}

internal static array<uint64> /*a*/ retWindow() {
    heap(new array<uint64>(8), out var Ꮡa);
    GoFrame ᒐ = default;
    try {
        ref var a = ref Ꮡa.Value;

        var f = @unsafe.Pointer.ToArray<uint32>(@unsafe.Pointer.FromPinnedBox(Ꮡa), 16);
        var fʗ1 = f;
        defer([GoFunc("retWindow.func1")] () => {
            fʗ1.ElementRef(2) = 5;
        }, ref ᒐ);
        a.Assign(array<uint64>(8, [1, 2]));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return Ꮡa.Value;
}

internal static holder /*h*/ retStruct() {
    holder h = new();
    GoFrame ᒐ = default;
    try {
        var s = h.x[..];
        var sʗ1 = s;
        defer([GoFunc("retStruct.func1")] () => {
            sʗ1[0] = 7;
        }, ref ᒐ);
        h.AssignFrom(new holder(n: 3, x: array<uint64>(8, [1, 2])));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return h;
}

internal static named /*a*/ retNamed() {
    named a = default;
    GoFrame ᒐ = default;
    try {
        var s = a[..];
        var sʗ1 = s;
        defer([GoFunc("retNamed.func1")] () => {
            sʗ1[0] = 7;
        }, ref ᒐ);
        a.Assign(new named(new uint64[]{1, 2}.array(8)));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return a;
}

internal static (array<uint64> a, error err) retMulti() {
    array<uint64> a = new(8);
    error err = default;
    GoFrame ᒐ = default;
    try {
        var s = a[..];
        var sʗ1 = s;
        defer([GoFunc("retMulti.func1")] () => {
            sʗ1[0] = 7;
        }, ref ᒐ);
        (var aᴛ1, err) = (mk(), default); a.Assign(aᴛ1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (a, err);
}

internal static (nint n, array<uint64> a) retSecond(bool early) {
    nint n = default;
    array<uint64> a = new(8);
    GoFrame ᒐ = default;
    try {
        var s = a[..];
        var sʗ1 = s;
        defer([GoFunc("retSecond.func1")] () => {
            sʗ1[0] = 7;
        }, ref ᒐ);
        if (early) {
            (n, var aᴛ1) = (5, array<uint64>(8, [3, 4])); a.Assign(aᴛ1); goto ᒐdone;
        }
        (n, var aᴛ2) = (6, array<uint64>(8, [1, 2])); a.Assign(aᴛ2);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, a);
}

internal static array<uint64> /*a*/ retRecover() {
    array<uint64> a = new(8);
    GoFrame ᒐ = default;
    try {
        var s = a[..];
        var sʗ1 = s;
        defer([GoFunc("retRecover.func1")] () => {
            recover();
            sʗ1[0] = 7;
        }, ref ᒐ);
        a.Assign(array<uint64>(8, [1, 2]));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return a;
}

internal static array<uint64> /*a*/ retFromVar() {
    array<uint64> a = new(8);
    GoFrame ᒐ = default;
    try {
        ref var v = ref heap<array<uint64>>(array<uint64>(8, [1, 2]), out var Ꮡv);
        var s = a[..];
        var sʗ1 = s;
        defer([GoFunc("retFromVar.func1")] () => {
            sʗ1[0] = 7;
            Ꮡv.Value[1] = 99;
        }, ref ᒐ);
        a.Assign(v);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return a;
}

[GoLazy] partial class lazyᴛretLit { internal static Func<array<uint64>> retLit = [GoFunc("init.func1")] () => {
    array<uint64> a = new(8);
    GoFrame ᒐ = default;
    try {
        var s = a[..];
        var sʗ1 = s;
        defer([GoFunc("init.func1.1")] () => {
            sʗ1[0] = 7;
        }, ref ᒐ);
        a.Assign(array<uint64>(8, [1, 2])); goto ᒐdone;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return a;
}; }

internal static array<uint64> retLitCaptured() {
    [GoFunc("retLitCaptured.func1")] array<uint64> /*a*/ f() {
        array<uint64> a = new(8);
        GoFrame ᒐ = default;
        try {
            defer([GoFunc("retLitCaptured.func1.1")] () => {
                a[0] += 10;
            }, ref ᒐ);
            var s = a[..];
            var sʗ1 = s;
            defer([GoFunc("retLitCaptured.func1.2")] () => {
                sʗ1[1] = 8;
            }, ref ᒐ);
            a.Assign(array<uint64>(8, [1, 2])); goto ᒐdone;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        ᒐdone: return a;
    }
    return f();
}

[GoLazy] partial class lazyᴛretLitIndex { internal static Func<array<uint64>> retLitIndex = [GoFunc("init.func2")] () => {
    heap(new array<uint64>(8), out var Ꮡa);
    GoFrame ᒐ = default;
    try {
        ref var a = ref Ꮡa.Value;
        var p = Ꮡa;
        defer([GoFunc("init.func2.1")] () => {
            Ꮡa.Value[0] = 0xEEEE;
        }, ref ᒐ);
        p.Value[1] = 505;
        goto ᒐdone;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return Ꮡa.Value;
}; }

[GoLazy] partial class lazyᴛretLitAddr { internal static Func<array<uint64>> retLitAddr = [GoFunc("init.func3")] () => {
    heap(new array<uint64>(8), out var Ꮡa);
    GoFrame ᒐ = default;
    try {
        ref var a = ref Ꮡa.Value;
        var p = Ꮡa;
        var pʗ1 = p;
        defer([GoFunc("init.func3.1")] () => {
            pʗ1.Value[0] = 7;
        }, ref ᒐ);
        Ꮡa.Value.Assign(array<uint64>(8, [1, 2])); goto ᒐdone;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return Ꮡa.Value;
}; }

internal static (array<uint64> a, error err) retForward() {
    array<uint64> a = new(8);
    error err = default;
    GoFrame ᒐ = default;
    try {
        var s = a[..];
        var sʗ1 = s;
        defer([GoFunc("retForward.func1")] () => {
            sʗ1[0] = 7;
        }, ref ᒐ);
        (var aᴛ1, err) = pair(); a.Assign(aᴛ1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (a, err);
}

internal static (array<uint64> a, array<uint64> b) retCrossed() {
    array<uint64> a = new(8);
    array<uint64> b = new(8);
    GoFrame ᒐ = default;
    try {
        var (sa, sb) = (a[..], b[..]);
        var saʗ1 = sa;
        var sbʗ1 = sb;
        defer([GoFunc("retCrossed.func1")] () => {
            saʗ1[0] = 7;
            sbʗ1[0] = 8;
        }, ref ᒐ);
        (a[1], b[1]) = (1, 2);
        (var aᴛ1, var bᴛ1) = (b.Clone(), a.Clone()); a.Assign(aᴛ1); b.Assign(bᴛ1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (a, b);
}

internal static error boom() { throw panic("boom"); }

internal static (array<uint64> a, error err) retPanicAfter() {
    array<uint64> a = new(8);
    error err = default;
    GoFrame ᒐ = default;
    try {
        var s = a[..];
        var sʗ1 = s;
        defer([GoFunc("retPanicAfter.func1")] () => {
            recover();
            sʗ1[1] = 5;
        }, ref ᒐ);
        (var aᴛ1, err) = (array<uint64>(8, [1, 2]), boom()); a.Assign(aᴛ1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (a, err);
}

internal static (named a, error err) retNamedTuple() {
    named a = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        var s = a[..];
        var sʗ1 = s;
        defer([GoFunc("retNamedTuple.func1")] () => {
            sʗ1[0] = 7;
        }, ref ᒐ);
        (var aᴛ1, err) = (array<uint64>(8, [1, 2]), default); a.Assign(aᴛ1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (a, err);
}

internal static void returns() {
    var a = retSlice();
    fmt.Println("ret slice  :", a[0], a[1]);
    a.Assign(retElemPtr());
    fmt.Println("ret elemPtr:", a[0], a[1]);
    a.Assign(retWindow());
    fmt.Println("ret window :", a[0], a[1]);
    var h = retStruct();
    fmt.Println("ret struct :", h.n, h.x[0], h.x[1]);
    var b = retNamed();
    fmt.Println("ret named  :", b[0], b[1]);
    (var aᴛ1, var err) = retMulti();
    a.Assign(aᴛ1);
    fmt.Println("ret multi  :", a[0], a[1], err);
    var (n1, a1) = retSecond(true);
    var (n2, a2) = retSecond(false);
    fmt.Println("ret second :", n1, a1[0], a1[1], n2, a2[0], a2[1]);
    a.Assign(retRecover());
    fmt.Println("ret recover:", a[0], a[1]);
    a.Assign(retFromVar());
    fmt.Println("ret fromVar:", a[0], a[1]);
    a.Assign(retLit());
    fmt.Println("ret lit    :", a[0], a[1]);
    a.Assign(retLitCaptured());
    fmt.Println("ret litCapt:", a[0], a[1]);
    a.Assign(retLitIndex());
    fmt.Println("ret litIdx :", a[0], a[1], len(a));
    a.Assign(retLitAddr());
    fmt.Println("ret litAddr:", a[0], a[1]);
    (var aᴛ2, err) = retForward();
    a.Assign(aᴛ2);
    fmt.Println("ret forward:", a[0], a[1], err);
    var (c1, c2) = retCrossed();
    fmt.Println("ret crossed:", c1[0], c1[1], c2[0], c2[1]);
    (var aᴛ3, err) = retPanicAfter();
    a.Assign(aᴛ3);
    fmt.Println("ret panicAf:", a[0], a[1], err);
    (var bᴛ1, err) = retNamedTuple();
    b.Assign(bᴛ1);
    fmt.Println("ret namedTp:", b[0], b[1], err);
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
    returns();
}

} // end main_package
