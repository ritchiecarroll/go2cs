namespace go;

using fmt = fmt_package;
using Δmath = math_package;
using reflect = reflect_package;
using runtime = runtime_package;

partial class main_package {

internal static float64 negZero = Δmath.Copysign(0D, -1D);

internal static @string sign(float64 f) {
    if (Δmath.Signbit(f)) {
        return "-0";
    }
    return "+0";
}

internal partial struct keyUpdate_pt /*dyn*/ {
    internal float64 x, y;
}

internal static void keyUpdate() {
    var m = new map<float64, bool>{};
    m[0D] = true;
    m[negZero] = true;
    foreach (var (k, _) in m) {
        fmt.Println("float64 key after +0 then -0:", sign(k), "len", len(m));
    }

    var m2 = new map<float64, bool>{};
    m2[negZero] = true;
    m2[0D] = true;
    foreach (var (k, _) in m2) {
        fmt.Println("float64 key after -0 then +0:", sign(k), "len", len(m2));
    }

    var mi = new map<any, nint>{};
    mi[0.0D] = 1;
    mi[negZero] = 2;
    foreach (var (k, v) in mi) {
        fmt.Println("interface key after +0 then -0:", sign(k._<float64>()), "value", v, "len", len(mi));
    }

    var mc = new map<complex128, nint>{};
    mc[complex(0D, 0D)] = 1;
    mc[complex(negZero, negZero)] = 2;
    foreach (var (k, v) in mc) {
        fmt.Println("complex128 key after overwrite:", sign(real(k)), sign(imag(k)), "value", v);
    }
    var ms = new map<keyUpdate_pt, nint>{};
    ms[new keyUpdate_pt(0D, 1D)] = 1;
    ms[new keyUpdate_pt(negZero, 1D)] = 2;
    foreach (var (k, v) in ms) {
        fmt.Println("struct{float64} key after overwrite:", sign(k.x), "value", v);
    }

    var mn = new map<nint, nint>{[7] = 1};
    mn[7] = 2;
    fmt.Println("int key overwrite:", mn[7], len(mn));

    var mf = new map<F64, bool>{};
    mf[0D] = true;
    mf[((F64)negZero)] = true;
    foreach (var (k, _) in mf) {
        fmt.Println("defined float64 key after overwrite:", sign((float64)k), "len", len(mf));
    }
    var mf32 = new map<F32, bool>{};
    mf32[0F] = true;
    mf32[((F32)(float32)negZero)] = true;
    foreach (var (k, _) in mf32) {
        fmt.Println("defined float32 key after overwrite:", sign((float64)k), "len", len(mf32));
    }
    var mc2 = new map<C128, bool>{};
    mc2[((C128)complex(0D, 0D))] = true;
    mc2[((C128)complex(negZero, 0D))] = true;
    foreach (var (k, _) in mc2) {
        fmt.Println("defined complex128 key after overwrite:", sign(real(k)), "len", len(mc2));
    }
}

partial struct F64 /*num:float64*/;

partial struct F32 /*num:float32*/;

partial struct C128 /*num:complex128*/;

internal static void nanKeys() {
    var nan = Δmath.NaN();
    var m = new map<float64, nint>{};
    m[nan] = 1;
    m[nan] = 2;
    m[0D] = 3;
    m[negZero] = 4;
    fmt.Println("float64 NaN twice then ±0:", len(m));

    var mi = new map<any, nint>{};
    mi[nan] = 1;
    mi[nan] = 2;
    fmt.Println("interface NaN twice:", len(mi));

    var mf = new map<F64, nint>{};
    mf[((F64)nan)] = 1;
    mf[((F64)nan)] = 2;
    fmt.Println("defined float64 NaN twice:", len(mf));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object oneˢ = (@string)"one"u8;

internal static void rangeUnderOverwrite() {
    nint bad = 0;
    nint after = 0;
    nint before = 0;
    for (nint trial = 0; trial < 400; trial++) {
        var m = new map<float64, nint>{};
        if (trial % 2 == 0) {
            (m[0D], m[1D]) = (1, 2);
        } else {
            (m[1D], m[0D]) = (2, 1);
        }
        var overwritten = false;
        foreach (var (k, v) in m) {
            if (k == 0D) {
                @string want = "+0";
                if (overwritten) {
                    want = "-0";
                    after++;
                    if (v != 3) {
                        bad++;
                    }
                } else {
                    before++;
                }
                if (sign(k) != want) {
                    bad++;
                }
            }
            if (!overwritten) {
                m[negZero] = 3;
                overwritten = true;
            }
        }
    }
    fmt.Println("range under overwrite: inconsistent", bad, "both orders seen", after > 0 && before > 0);

    (bad, after, before) = (0, 0, 0);
    for (nint trial = 0; trial < 400; trial++) {
        var m = new map<any, nint>{};
        if (trial % 2 == 0) {
            (m[0.0D], m[oneˢ]) = (1, 2);
        } else {
            (m[oneˢ], m[0.0D]) = (2, 1);
        }
        var overwritten = false;
        foreach (var (k, v) in m) {
            {
                var (f, ok) = k._<float64>(ᐧ); if (ok) {
                    @string want = "+0";
                    if (overwritten) {
                        want = "-0";
                        after++;
                        if (v != 3) {
                            bad++;
                        }
                    } else {
                        before++;
                    }
                    if (sign(f) != want) {
                        bad++;
                    }
                }
            }
            if (!overwritten) {
                m[negZero] = 3;
                overwritten = true;
            }
        }
    }
    fmt.Println("interface range under overwrite: inconsistent", bad, "both orders seen", after > 0 && before > 0);
}

internal static void @try(@string label, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
                    var (_, isRuntimeError) = r._<runtimeꓸError>(ᐧ);
                    fmt.Println(label + ": panic:", r, "runtime.Error:", isRuntimeError);
                }
            }
        }, ref ᒐ);
        f();
        fmt.Println(label + ": no panic");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void hashPanic() {
    slice<nint> Δslice = default;
    var empty = new map<any, bool>{};
    var full = new map<any, bool>{[(nint)1] = true, [(@string)"a"u8] = true};
    map<any, bool> nilMap = default;

    var emptyʗ1 = empty;
    var sliceʗ1 = Δslice;
    @try("empty map lookup []int", () => {
        _ = emptyʗ1[sliceʗ1];
    });
    var emptyʗ2 = empty;
    @try("empty map comma-ok map key", () => {
        (_, _) = emptyʗ2[new map<@string, nint>{}, ꟷ];
    });
    var emptyʗ3 = empty;
    @try("empty map delete func key", () => {
        delete(emptyʗ3, () => {
        });
    });
    var emptyʗ4 = empty;
    var sliceʗ2 = Δslice;
    @try("empty map assign []int", () => {
        emptyʗ4[sliceʗ2] = true;
    });
    var fullʗ1 = full;
    var sliceʗ3 = Δslice;
    @try("full map lookup []int", () => {
        _ = fullʗ1[sliceʗ3];
    });
    var fullʗ2 = full;
    @try("full map assign []int", () => {
        fullʗ2[new @string[]{"x"}.slice()] = true;
    });
    var nilMapʗ1 = nilMap;
    var sliceʗ4 = Δslice;
    @try("nil map lookup []int", () => {
        _ = nilMapʗ1[sliceʗ4];
    });
    var nilMapʗ2 = nilMap;
    var sliceʗ5 = Δslice;
    @try("nil map delete []int", () => {
        delete(nilMapʗ2, sliceʗ5);
    });
    var emptyʗ5 = empty;
    @try("hashable key on empty map", () => {
        _ = emptyʗ5[new nint[]{1, 2}.array()];
    });
    fmt.Println("full map len after:", len(full), "empty map len after:", len(empty));
}

internal static void reflectSide() {
    ref var m = ref heap<reflectꓸValue>(reflect.MakeMap(reflect.MapOf(reflect.TypeFor<any>(), reflect.TypeFor<bool>())), out var Ꮡm);
    slice<nint> Δslice = default;
    var mʗ1 = m;
    var sliceʗ1 = Δslice;
    @try("reflect MapIndex []int on empty MapOf(any)", () => {
        mʗ1.MapIndex(reflect.ValueOf(sliceʗ1));
    });
    var mʗ2 = m;
    var sliceʗ2 = Δslice;
    @try("reflect SetMapIndex []int", () => {
        mʗ2.SetMapIndex(reflect.ValueOf(sliceʗ2), reflect.ValueOf(true));
    });
    ref var nilm = ref heap<reflectꓸValue>(reflect.Zero(reflect.TypeFor<map<any, bool>>()), out var Ꮡnilm);
    var nilmʗ1 = nilm;
    var sliceʗ3 = Δslice;
    @try("reflect MapIndex []int on nil map", () => {
        nilmʗ1.MapIndex(reflect.ValueOf(sliceʗ3));
    });
    var nilmʗ2 = nilm;
    var sliceʗ4 = Δslice;
    @try("reflect delete []int on nil map", () => {
        nilmʗ2.SetMapIndex(reflect.ValueOf(sliceʗ4), new reflectꓸValue());
    });

    var fm = reflect.MakeMap(reflect.MapOf(reflect.TypeFor<float64>(), reflect.TypeFor<bool>()));
    fm.SetMapIndex(reflect.ValueOf(0.0D), reflect.ValueOf(true));
    fm.SetMapIndex(reflect.ValueOf(negZero), reflect.ValueOf(true));
    var iter = fm.MapRange();
    while (iter.Next()) {
        fmt.Println("reflect float64 key after +0 then -0:", sign(iter.Key().Float()), "len", fm.Len());
    }
}

internal static void Main() {
    keyUpdate();
    nanKeys();
    rangeUnderOverwrite();
    hashPanic();
    reflectSide();
}

} // end main_package
