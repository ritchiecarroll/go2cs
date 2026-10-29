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

internal static @string Error(this sliceErr _) {
    return "sliceErr";
}

internal static void check(@string name, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
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
                var r = recover(); if (r != nil) {
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

internal partial struct main_nestedComparable /*dyn*/ {
    public point P;
    public array<nint> A = new(2);
}

internal static void Main() {
    fmt.Println("== uncomparable dynamic types: Go panics ==");

    any m = new map<@string, nint>{["a"] = 1};
    var mʗ1 = m;
    check("map", () => {
        _ = AreEqual(mʗ1, mʗ1);
    });

    any s = new nint[]{1, 2}.slice();
    var sʗ1 = s;
    check("slice", () => {
        _ = AreEqual(sʗ1, sʗ1);
    });

    any fn = () => {
    };
    var fnʗ1 = fn;
    check("func", () => {
        _ = AreEqual(fnʗ1, fnʗ1);
    });

    any nm = new myMap(new map<@string, nint>{["a"] = 1});
    var nmʗ1 = nm;
    check("named map", () => {
        _ = AreEqual(nmʗ1, nmʗ1);
    });

    any nsl = new mySlice(new nint[]{1}.slice());
    var nslʗ1 = nsl;
    check("named slice", () => {
        _ = AreEqual(nslʗ1, nslʗ1);
    });

    any nf = new Func<nint, @string>((nint _Δp0) => "");
    var nfʗ1 = nf;
    checkPanicOnly("named func", () => {
        _ = AreEqual(nfʗ1, nfʗ1);
    });

    any ws = new withSlice(1, new nint[]{2}.slice());
    var wsʗ1 = ws;
    check("struct w/ slice", () => {
        _ = AreEqual(wsʗ1, wsʗ1);
    });

    any wm = new withMap(1, new map<@string, nint>{});
    var wmʗ1 = wm;
    check("struct w/ map", () => {
        _ = AreEqual(wmʗ1, wmʗ1);
    });

    any wf = new withFunc(() => {
    });
    var wfʗ1 = wf;
    check("struct w/ func", () => {
        _ = AreEqual(wfʗ1, wfʗ1);
    });

    any nested = new outer(new inner(slice<byte>("x"u8)), 1);
    var nestedʗ1 = nested;
    check("struct w/ struct", () => {
        _ = AreEqual(nestedʗ1, nestedʗ1);
    });

    fmt.Println();
    fmt.Println("== array length is part of the reported type ==");

    any aos = new slice<nint>[]{new nint[]{1}.slice()}.array();
    var aosʗ1 = aos;
    check("array of slice", () => {
        _ = AreEqual(aosʗ1, aosʗ1);
    });

    any aom = new map<@string, nint>[]{}.array(2);
    var aomʗ1 = aom;
    check("array of map", () => {
        _ = AreEqual(aomʗ1, aomʗ1);
    });

    any a2d = new array<slice<nint>>[]{}.array(2, () => new(3));
    var a2dʗ1 = a2d;
    check("2-D array of slice", () => {
        _ = AreEqual(a2dʗ1, a2dʗ1);
    });

    any aostruct = new withSlice[]{}.array(3);
    var aostructʗ1 = aostruct;
    check("array of struct", () => {
        _ = AreEqual(aostructʗ1, aostructʗ1);
    });

    fmt.Println();
    fmt.Println("== interface FIELD panics naming the inner type, recoverably ==");

    any wa = new withAny(1, new map<@string, nint>{});
    var waʗ1 = wa;
    check("struct w/ any=map", () => {
        _ = AreEqual(waʗ1, waʗ1);
    });

    any wa2 = new withAny(1, new nint[]{1}.slice());
    var wa2ʗ1 = wa2;
    check("struct w/ any=slice", () => {
        _ = AreEqual(wa2ʗ1, wa2ʗ1);
    });

    any wc1 = new withAny(1, (nint)5);
    any wc2 = new withAny(1, (nint)5);
    any wc3 = new withAny(1, (nint)6);
    fmt.Println("struct w/ any=int:", AreEqual(wc1, wc2), AreEqual(wc1, wc3));

    fmt.Println();
    fmt.Println("== non-empty interface ==");

    error e = new sliceErr(new nint[]{1}.slice());
    var eʗ1 = e;
    check("error w/ slice", () => {
        _ = AreEqual(eʗ1, eʗ1);
    });

    fmt.Println();
    fmt.Println("== nil compares never panic ==");

    var mʗ2 = m;
    check("map == nil", () => {
        _ = mʗ2 == nil;
    });
    var sʗ2 = s;
    check("slice == nil", () => {
        _ = sʗ2 == nil;
    });
    var fnʗ2 = fn;
    check("func == nil", () => {
        _ = fnʗ2 == nil;
    });
    var wsʗ2 = ws;
    check("struct w/ slice == nil", () => {
        _ = wsʗ2 == nil;
    });
    var aosʗ2 = aos;
    check("array of slice == nil", () => {
        _ = aosʗ2 == nil;
    });

    any nilIface = default;
    var mʗ3 = m;
    var nilIfaceʗ1 = nilIface;
    check("nil iface == map", () => {
        _ = AreEqual(nilIfaceʗ1, mʗ3);
    });
    var nilIfaceʗ2 = nilIface;
    check("nil iface == nil iface", () => {
        _ = AreEqual(nilIfaceʗ2, nilIfaceʗ2);
    });

    fmt.Println();
    fmt.Println("== differing dynamic types never panic ==");

    any i = (nint)5;
    var iʗ1 = i;
    var mʗ4 = m;
    check("map == int", () => {
        _ = AreEqual(mʗ4, iʗ1);
    });
    var mʗ5 = m;
    var sʗ3 = s;
    check("slice == map", () => {
        _ = AreEqual(sʗ3, mʗ5);
    });
    var iʗ2 = i;
    var sʗ4 = s;
    check("int == slice", () => {
        _ = AreEqual(iʗ2, sʗ4);
    });

    fmt.Println();
    fmt.Println("== comparable values still compare ==");

    any i1 = (nint)5;
    any i2 = (nint)5;
    any i3 = (nint)6;
    fmt.Println("int:      ", AreEqual(i1, i2), AreEqual(i1, i3));

    any s1 = (@string)"hi"u8;
    any s2 = (@string)"hi"u8;
    any s3 = (@string)"ho"u8;
    fmt.Println("string:   ", AreEqual(s1, s2), AreEqual(s1, s3));

    any b1 = true;
    any b2 = false;
    fmt.Println("bool:     ", AreEqual(b1, b1), AreEqual(b1, b2));

    any f1 = 1.5D;
    any f2 = 2.5D;
    fmt.Println("float:    ", AreEqual(f1, f1), AreEqual(f1, f2));

    any p1 = new point(1, 2);
    any p2 = new point(1, 2);
    any p3 = new point(1, 3);
    fmt.Println("struct:   ", AreEqual(p1, p2), AreEqual(p1, p3));

    any arr1 = new nint[]{1, 2}.array();
    any arr2 = new nint[]{1, 2}.array();
    any arr3 = new nint[]{1, 3}.array();
    fmt.Println("array:    ", AreEqual(arr1, arr2), AreEqual(arr1, arr3));

    ref var x = ref heap<nint>(out var Ꮡx);
    x = 1;
    ref var y = ref heap<nint>(out var Ꮡy);
    y = 2;
    any ptr1 = Ꮡx;
    any ptr2 = Ꮡx;
    any ptr3 = Ꮡy;
    fmt.Println("pointer:  ", AreEqual(ptr1, ptr2), AreEqual(ptr1, ptr3));

    ref var pm = ref heap<map<@string, nint>>(new map<@string, nint>{}, out var Ꮡpm);
    any pmap1 = Ꮡpm;
    any pmap2 = Ꮡpm;
    fmt.Println("ptr-to-map:", AreEqual(pmap1, pmap2));

    var (ch1, ch2) = (new channel<nint>(0), new channel<nint>(0));
    any c1 = ch1;
    any c2 = ch1;
    any c3 = ch2;
    fmt.Println("chan:     ", AreEqual(c1, c2), AreEqual(c1, c3));

    any e1 = ((any)(nint)7);
    any e2 = ((any)(nint)7);
    fmt.Println("iface:    ", AreEqual(e1, e2));
    any nc1 = new main_nestedComparable(new point(1, 2), new nint[]{3, 4}.array());
    any nc2 = new main_nestedComparable(new point(1, 2), new nint[]{3, 4}.array());
    fmt.Println("nested:   ", AreEqual(nc1, nc2));

    fmt.Println();
    fmt.Println("== the verdict is stable across repeated comparisons ==");

    for (nint iΔ1 = 0; iΔ1 < 2; iΔ1++) {
        var mʗ6 = m;
        check("repeat map", () => {
            _ = AreEqual(mʗ6, mʗ6);
        });
        var i1ʗ1 = i1;
        var i2ʗ1 = i2;
        check("repeat comparable", () => {
            _ = AreEqual(i1ʗ1, i2ʗ1);
        });
    }
}

} // end main_package
