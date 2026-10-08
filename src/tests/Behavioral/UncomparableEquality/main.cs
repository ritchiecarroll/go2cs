namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct myMap /*map[@string, nint]*/;

partial struct mySlice /*[]nint*/;

// type myFunc is a methodless func type — rendered inline as its base delegate

partial struct withSlice {
    public nint A;
    public slice<nint> B;
}

partial struct withMap {
    public nint A;
    public map<@string, nint> M;
}

partial struct withFunc {
    public Action F;
}

public partial struct inner {
    public slice<byte> S;
}

partial struct outer {
    public inner I;
    public nint N;
}

partial struct point {
    public nint X;
    public nint Y;
}

partial struct withAny {
    public nint A;
    public any V;
}

partial struct sliceErr {
    public slice<nint> S;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string sliceErrˢ = "sliceErr"u8;

internal static @string Error(this sliceErr _) {
    return sliceErrˢ;
}

internal static void check(@string name, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Printf("%-24s PANIC: %v\n"u8, name, r);
                    return;
                }
            }
            fmt.Printf("%-24s no panic\n"u8, name);
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void checkPanicOnly(@string name, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Printf("%-24s PANIC\n"u8, name);
                    return;
                }
            }
            fmt.Printf("%-24s no panic\n"u8, name);
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object uncomparableDynamicTypesˢ = (@string)"== uncomparable dynamic types: Go panics =="u8;
private static readonly @string mapˢ = "map"u8;
private static readonly @string sliceˢ = "slice"u8;
private static readonly @string funcˢ = "func"u8;
private static readonly @string namedMapˢ = "named map"u8;
private static readonly @string namedSliceˢ = "named slice"u8;
private static readonly @string namedFuncˢ = "named func"u8;
private static readonly @string structWSliceˢ = "struct w/ slice"u8;
private static readonly @string structWMapˢ = "struct w/ map"u8;
private static readonly @string structWFuncˢ = "struct w/ func"u8;
private static readonly @string structWStructˢ = "struct w/ struct"u8;
private static readonly object arrayLengthIsPartOfTheˢ = (@string)"== array length is part of the reported type =="u8;
private static readonly @string arrayOfSliceˢ = "array of slice"u8;
private static readonly @string arrayOfMapˢ = "array of map"u8;
private static readonly @string dArrayOfSliceˢ = "2-D array of slice"u8;
private static readonly @string arrayOfStructˢ = "array of struct"u8;
private static readonly object interfaceFieldPanicsˢ = (@string)"== interface FIELD panics naming the inner type, recoverably =="u8;
private static readonly @string structWAnyMapˢ = "struct w/ any=map"u8;
private static readonly @string structWAnySliceˢ = "struct w/ any=slice"u8;
private static readonly object structWAnyIntˢ = (@string)"struct w/ any=int:"u8;
private static readonly object nonEmptyInterfaceˢ = (@string)"== non-empty interface =="u8;
private static readonly @string errorWSliceˢ = "error w/ slice"u8;
private static readonly object nilComparesNeverPanicˢ = (@string)"== nil compares never panic =="u8;
private static readonly @string mapNilˢ = "map == nil"u8;
private static readonly @string sliceNilˢ = "slice == nil"u8;
private static readonly @string funcNilˢ = "func == nil"u8;
private static readonly @string structWSliceNilˢ = "struct w/ slice == nil"u8;
private static readonly @string arrayOfSliceNilˢ = "array of slice == nil"u8;
private static readonly @string nilIfaceMapˢ = "nil iface == map"u8;
private static readonly @string nilIfaceNilIfaceˢ = "nil iface == nil iface"u8;
private static readonly object differingDynamicTypesˢ = (@string)"== differing dynamic types never panic =="u8;
private static readonly @string mapIntˢ = "map == int"u8;
private static readonly @string sliceMapˢ = "slice == map"u8;
private static readonly @string intSliceˢ = "int == slice"u8;
private static readonly object comparableValuesStillˢ = (@string)"== comparable values still compare =="u8;
private static readonly object intˢ = (@string)"int:      "u8;
private static readonly object stringˢ = (@string)"string:   "u8;
private static readonly object boolˢ = (@string)"bool:     "u8;
private static readonly object floatˢ = (@string)"float:    "u8;
private static readonly object structˢ = (@string)"struct:   "u8;
private static readonly object arrayˢ = (@string)"array:    "u8;
private static readonly object pointerˢ = (@string)"pointer:  "u8;
private static readonly object ptrToMapˢ = (@string)"ptr-to-map:"u8;
private static readonly object chanˢ = (@string)"chan:     "u8;
private static readonly object ifaceˢ = (@string)"iface:    "u8;
private static readonly object nestedˢ = (@string)"nested:   "u8;
private static readonly object theVerdictIsStableAcrossˢ = (@string)"== the verdict is stable across repeated comparisons =="u8;
private static readonly @string repeatMapˢ = "repeat map"u8;
private static readonly @string repeatComparableˢ = "repeat comparable"u8;

internal partial struct main_nestedComparable /*dyn*/ {
    public point P;
    public array<nint> A = new(2);
}

internal static void Main() {
    fmt.Println(uncomparableDynamicTypesˢ);
    any m = new map<@string, nint>{["a"u8] = 1};
    var mʗ1 = m;
    check(mapˢ, () => {
        _ = AreEqual(mʗ1, mʗ1);
    });
    any s = new nint[]{1, 2}.slice();
    var sʗ1 = s;
    check(sliceˢ, () => {
        _ = AreEqual(sʗ1, sʗ1);
    });

    any fn = () => {
    };
    var fnʗ1 = fn;
    check(funcˢ, () => {
        _ = AreEqual(fnʗ1, fnʗ1);
    });
    any nm = new myMap(new map<@string, nint>{["a"u8] = 1});
    var nmʗ1 = nm;
    check(namedMapˢ, () => {
        _ = AreEqual(nmʗ1, nmʗ1);
    });
    any nsl = new mySlice(new nint[]{1}.slice());
    var nslʗ1 = nsl;
    check(namedSliceˢ, () => {
        _ = AreEqual(nslʗ1, nslʗ1);
    });

    any nf = new Func<nint, @string>((nint _Δp0) => ""u8);
    var nfʗ1 = nf;
    checkPanicOnly(namedFuncˢ, () => {
        _ = AreEqual(nfʗ1, nfʗ1);
    });
    any ws = new withSlice(1, new nint[]{2}.slice());
    var wsʗ1 = ws;
    check(structWSliceˢ, () => {
        _ = AreEqual(wsʗ1, wsʗ1);
    });
    any wm = new withMap(1, new map<@string, nint>{});
    var wmʗ1 = wm;
    check(structWMapˢ, () => {
        _ = AreEqual(wmʗ1, wmʗ1);
    });

    any wf = new withFunc(() => {
    });
    var wfʗ1 = wf;
    check(structWFuncˢ, () => {
        _ = AreEqual(wfʗ1, wfʗ1);
    });
    any nested = new outer(new inner(slice<byte>("x"u8)), 1);
    var nestedʗ1 = nested;
    check(structWStructˢ, () => {
        _ = AreEqual(nestedʗ1, nestedʗ1);
    });
    fmt.Println();
    fmt.Println(arrayLengthIsPartOfTheˢ);
    any aos = new slice<nint>[]{new nint[]{1}.slice()}.array();
    var aosʗ1 = aos;
    check(arrayOfSliceˢ, () => {
        _ = AreEqual(aosʗ1, aosʗ1);
    });
    any aom = new map<@string, nint>[]{}.array(2);
    var aomʗ1 = aom;
    check(arrayOfMapˢ, () => {
        _ = AreEqual(aomʗ1, aomʗ1);
    });
    any a2d = new array<slice<nint>>[]{}.array(2, () => new(3));
    var a2dʗ1 = a2d;
    check(dArrayOfSliceˢ, () => {
        _ = AreEqual(a2dʗ1, a2dʗ1);
    });
    any aostruct = new withSlice[]{}.array(3);
    var aostructʗ1 = aostruct;
    check(arrayOfStructˢ, () => {
        _ = AreEqual(aostructʗ1, aostructʗ1);
    });
    fmt.Println();
    fmt.Println(interfaceFieldPanicsˢ);
    any wa = new withAny(1, new map<@string, nint>{});
    var waʗ1 = wa;
    check(structWAnyMapˢ, () => {
        _ = AreEqual(waʗ1, waʗ1);
    });
    any wa2 = new withAny(1, new nint[]{1}.slice());
    var wa2ʗ1 = wa2;
    check(structWAnySliceˢ, () => {
        _ = AreEqual(wa2ʗ1, wa2ʗ1);
    });
    any wc1 = new withAny(1, (nint)(5));
    any wc2 = new withAny(1, (nint)(5));
    any wc3 = new withAny(1, (nint)(6));
    fmt.Println(structWAnyIntˢ, AreEqual(wc1, wc2), AreEqual(wc1, wc3));
    fmt.Println();
    fmt.Println(nonEmptyInterfaceˢ);
    error e = new sliceErr(new nint[]{1}.slice());
    var eʗ1 = e;
    check(errorWSliceˢ, () => {
        _ = AreEqual(eʗ1, eʗ1);
    });
    fmt.Println();
    fmt.Println(nilComparesNeverPanicˢ);
    var mʗ2 = m;
    check(mapNilˢ, () => {
        _ = mʗ2 == default!;
    });
    var sʗ2 = s;
    check(sliceNilˢ, () => {
        _ = sʗ2 == default!;
    });
    var fnʗ2 = fn;
    check(funcNilˢ, () => {
        _ = fnʗ2 == default!;
    });
    var wsʗ2 = ws;
    check(structWSliceNilˢ, () => {
        _ = wsʗ2 == default!;
    });
    var aosʗ2 = aos;
    check(arrayOfSliceNilˢ, () => {
        _ = aosʗ2 == default!;
    });
    any nilIface = default!;
    var mʗ3 = m;
    var nilIfaceʗ1 = nilIface;
    check(nilIfaceMapˢ, () => {
        _ = AreEqual(nilIfaceʗ1, mʗ3);
    });
    var nilIfaceʗ2 = nilIface;
    check(nilIfaceNilIfaceˢ, () => {
        _ = AreEqual(nilIfaceʗ2, nilIfaceʗ2);
    });
    fmt.Println();
    fmt.Println(differingDynamicTypesˢ);
    any i = (nint)(5);
    var iʗ1 = i;
    var mʗ4 = m;
    check(mapIntˢ, () => {
        _ = AreEqual(mʗ4, iʗ1);
    });
    var mʗ5 = m;
    var sʗ3 = s;
    check(sliceMapˢ, () => {
        _ = AreEqual(sʗ3, mʗ5);
    });
    var iʗ2 = i;
    var sʗ4 = s;
    check(intSliceˢ, () => {
        _ = AreEqual(iʗ2, sʗ4);
    });
    fmt.Println();
    fmt.Println(comparableValuesStillˢ);
    any i1 = (nint)(5);
    any i2 = (nint)(5);
    any i3 = (nint)(6);
    fmt.Println(intˢ, AreEqual(i1, i2), AreEqual(i1, i3));
    any s1 = (@string)"hi"u8;
    any s2 = (@string)"hi"u8;
    any s3 = (@string)"ho"u8;
    fmt.Println(stringˢ, AreEqual(s1, s2), AreEqual(s1, s3));
    any b1 = true;
    any b2 = false;
    fmt.Println(boolˢ, AreEqual(b1, b1), AreEqual(b1, b2));
    any f1 = 1.5D;
    any f2 = 2.5D;
    fmt.Println(floatˢ, AreEqual(f1, f1), AreEqual(f1, f2));
    any p1 = new point(1, 2);
    any p2 = new point(1, 2);
    any p3 = new point(1, 3);
    fmt.Println(structˢ, AreEqual(p1, p2), AreEqual(p1, p3));
    any arr1 = new nint[]{1, 2}.array();
    any arr2 = new nint[]{1, 2}.array();
    any arr3 = new nint[]{1, 3}.array();
    fmt.Println(arrayˢ, AreEqual(arr1, arr2), AreEqual(arr1, arr3));
    ref var x = ref heap<nint>(out var Ꮡx);
    x = 1;
    ref var y = ref heap<nint>(out var Ꮡy);
    y = 2;
    any ptr1 = Ꮡx;
    any ptr2 = Ꮡx;
    any ptr3 = Ꮡy;
    fmt.Println(pointerˢ, AreEqual(ptr1, ptr2), AreEqual(ptr1, ptr3));
    ref var pm = ref heap<map<@string, nint>>(out var Ꮡpm);
    pm = new map<@string, nint>{};
    any pmap1 = Ꮡpm;
    any pmap2 = Ꮡpm;
    fmt.Println(ptrToMapˢ, AreEqual(pmap1, pmap2));
    var (ch1, ch2) = (new channel<nint>(0), new channel<nint>(0));
    any c1 = ch1;
    any c2 = ch1;
    any c3 = ch2;
    fmt.Println(chanˢ, AreEqual(c1, c2), AreEqual(c1, c3));
    any e1 = ((any)(nint)(7));
    any e2 = ((any)(nint)(7));
    fmt.Println(ifaceˢ, AreEqual(e1, e2));
    any nc1 = new main_nestedComparable(new point(1, 2), new nint[]{3, 4}.array());
    any nc2 = new main_nestedComparable(new point(1, 2), new nint[]{3, 4}.array());
    fmt.Println(nestedˢ, AreEqual(nc1, nc2));
    fmt.Println();
    fmt.Println(theVerdictIsStableAcrossˢ);
    for (nint iΔ1 = 0; iΔ1 < 2; iΔ1++) {
        var mʗ6 = m;
        check(repeatMapˢ, () => {
            _ = AreEqual(mʗ6, mʗ6);
        });
        var i1ʗ1 = i1;
        var i2ʗ1 = i2;
        check(repeatComparableˢ, () => {
            _ = AreEqual(i1ʗ1, i2ʗ1);
        });
    }
}

} // end main_package
