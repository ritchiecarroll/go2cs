namespace go;

using fmt = fmt_package;
using Δmath = math_package;
using reflect = reflect_package;

partial class main_package {

internal static float64 negZero = Δmath.Copysign(0D, -1D);

internal static @string sign(float64 f) {
    if (Δmath.Signbit(f)) {
        return "-0"u8;
    }
    return "+0"u8;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object float64KeyAfter0Then0ˢ = (@string)"float64 key after +0 then -0:"u8;
private static readonly object lenˢ = (@string)"len"u8;
private static readonly object float64KeyAfter0Then0ˢ2 = (@string)"float64 key after -0 then +0:"u8;
private static readonly object interfaceKeyAfter0Then0ˢ = (@string)"interface key after +0 then -0:"u8;
private static readonly object valueˢ = (@string)"value"u8;
private static readonly object complex128KeyAfterˢ = (@string)"complex128 key after overwrite:"u8;
private static readonly object structFloat64KeyAfterˢ = (@string)"struct{float64} key after overwrite:"u8;
private static readonly object intKeyOverwriteˢ = (@string)"int key overwrite:"u8;
private static readonly object definedFloat64KeyAfterˢ = (@string)"defined float64 key after overwrite:"u8;
private static readonly object definedFloat32KeyAfterˢ = (@string)"defined float32 key after overwrite:"u8;
private static readonly object definedComplex128Keyˢ = (@string)"defined complex128 key after overwrite:"u8;

internal partial struct keyUpdate_pt /*dyn*/ {
    internal float64 x, y;
}

internal static void keyUpdate() {
    var m = new map<float64, bool>{};
    m[0D] = true;
    m[negZero] = true;
    foreach (var (k, _) in m) {
        fmt.Println(float64KeyAfter0Then0ˢ, sign(k), lenˢ, len(m));
    }
    var m2 = new map<float64, bool>{};
    m2[negZero] = true;
    m2[0D] = true;
    foreach (var (k, _) in m2) {
        fmt.Println(float64KeyAfter0Then0ˢ2, sign(k), lenˢ, len(m2));
    }
    var mi = new map<any, nint>{};
    mi[0.0D] = 1;
    mi[negZero] = 2;
    foreach (var (k, v) in mi) {
        fmt.Println(interfaceKeyAfter0Then0ˢ, sign(k._<float64>()), valueˢ, v, lenˢ, len(mi));
    }
    var mc = new map<complex128, nint>{};
    mc[complex(0D, 0D)] = 1;
    mc[complex(negZero, negZero)] = 2;
    foreach (var (k, v) in mc) {
        fmt.Println(complex128KeyAfterˢ, sign(real(k)), sign(imag(k)), valueˢ, v);
    }
    var ms = new map<keyUpdate_pt, nint>{};
    ms[new keyUpdate_pt(0D, 1D)] = 1;
    ms[new keyUpdate_pt(negZero, 1D)] = 2;
    foreach (var (k, v) in ms) {
        fmt.Println(structFloat64KeyAfterˢ, sign(k.x), valueˢ, v);
    }
    var mn = new map<nint, nint>{[7] = 1};
    mn[7] = 2;
    fmt.Println(intKeyOverwriteˢ, mn[7], len(mn));
    var mf = new map<F64, bool>{};
    mf[0D] = true;
    mf[((F64)negZero)] = true;
    foreach (var (k, _) in mf) {
        fmt.Println(definedFloat64KeyAfterˢ, sign((float64)k), lenˢ, len(mf));
    }
    var mf32 = new map<F32, bool>{};
    mf32[0F] = true;
    mf32[((F32)(float32)negZero)] = true;
    foreach (var (k, _) in mf32) {
        fmt.Println(definedFloat32KeyAfterˢ, sign((float64)(float32)k), lenˢ, len(mf32));
    }
    var mc2 = new map<C128, bool>{};
    mc2[((C128)complex(0D, 0D))] = true;
    mc2[((C128)complex(negZero, 0D))] = true;
    foreach (var (k, _) in mc2) {
        fmt.Println(definedComplex128Keyˢ, sign(real(k)), lenˢ, len(mc2));
    }
}

partial struct F64 /*num:float64*/;

partial struct F32 /*num:float32*/;

partial struct C128 /*num:complex128*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object float64NaNTwiceThen0ˢ = (@string)"float64 NaN twice then ±0:"u8;
private static readonly object interfaceNaNTwiceˢ = (@string)"interface NaN twice:"u8;
private static readonly object definedFloat64NaNTwiceˢ = (@string)"defined float64 NaN twice:"u8;

internal static void nanKeys() {
    var nan = Δmath.NaN();
    var m = new map<float64, nint>{};
    m[nan] = 1;
    m[nan] = 2;
    m[0D] = 3;
    m[negZero] = 4;
    fmt.Println(float64NaNTwiceThen0ˢ, len(m));
    var mi = new map<any, nint>{};
    mi[nan] = 1;
    mi[nan] = 2;
    fmt.Println(interfaceNaNTwiceˢ, len(mi));
    var mf = new map<F64, nint>{};
    mf[((F64)nan)] = 1;
    mf[((F64)nan)] = 2;
    fmt.Println(definedFloat64NaNTwiceˢ, len(mf));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object rangeUnderOverwriteˢ = (@string)"range under overwrite: inconsistent"u8;
private static readonly object bothOrdersSeenˢ = (@string)"both orders seen"u8;
private static readonly object oneˢ = (@string)"one"u8;
private static readonly object interfaceRangeUnderˢ = (@string)"interface range under overwrite: inconsistent"u8;

internal static void rangeUnderOverwrite() {
    nint bad = 0;
    nint after = 0;
    nint before = 0;
    for (nint trial = 0; trial < 400; trial++) {
        var m = new map<float64, nint>{};
        if (trial % 2 == 0){
            (m[0D], m[1D]) = (1, 2);
        } else {
            (m[1D], m[0D]) = (2, 1);
        }
        var overwritten = false;
        foreach (var (k, v) in m) {
            if (k == 0D) {
                @string want = "+0"u8;
                if (overwritten){
                    want = "-0"u8;
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
    fmt.Println(rangeUnderOverwriteˢ, bad, bothOrdersSeenˢ, after > 0 && before > 0);
    (bad, after, before) = (0, 0, 0);
    for (nint trial = 0; trial < 400; trial++) {
        var m = new map<any, nint>{};
        if (trial % 2 == 0){
            (m[0.0D], m[oneˢ]) = (1, 2);
        } else {
            (m[oneˢ], m[0.0D]) = (2, 1);
        }
        var overwritten = false;
        foreach (var (k, v) in m) {
            {
                var (f, ok) = k._<float64>(ᐧ); if (ok) {
                    @string want = "+0"u8;
                    if (overwritten){
                        want = "-0"u8;
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
    fmt.Println(interfaceRangeUnderˢ, bad, bothOrdersSeenˢ, after > 0 && before > 0);
}

internal static void @try(@string label, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println(label + ": panic:", r);
                }
            }
        }, ref ᒐ);
        f();
        fmt.Println(label + ": no panic");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string emptyMapLookupIntˢ = "empty map lookup []int"u8;
private static readonly @string emptyMapCommaOkMapKeyˢ = "empty map comma-ok map key"u8;
private static readonly @string emptyMapDeleteFuncKeyˢ = "empty map delete func key"u8;
private static readonly @string emptyMapAssignIntˢ = "empty map assign []int"u8;
private static readonly @string fullMapLookupIntˢ = "full map lookup []int"u8;
private static readonly @string fullMapAssignIntˢ = "full map assign []int"u8;
private static readonly @string nilMapLookupIntˢ = "nil map lookup []int"u8;
private static readonly @string nilMapDeleteIntˢ = "nil map delete []int"u8;
private static readonly @string hashableKeyOnEmptyMapˢ = "hashable key on empty map"u8;
private static readonly object fullMapLenAfterˢ = (@string)"full map len after:"u8;
private static readonly object emptyMapLenAfterˢ = (@string)"empty map len after:"u8;

internal static void hashPanic() {
    slice<nint> Δslice = default!;
    var empty = new map<any, bool>{};
    var full = new map<any, bool>{[(nint)(1)] = true, [(@string)"a"u8] = true};
    map<any, bool> nilMap = default!;
    var emptyʗ1 = empty;
    var sliceʗ1 = Δslice;
    @try(emptyMapLookupIntˢ, () => {
        _ = emptyʗ1[sliceʗ1];
    });
    var emptyʗ2 = empty;
    @try(emptyMapCommaOkMapKeyˢ, () => {
        (_, _) = emptyʗ2[new map<@string, nint>{}, ꟷ];
    });
    var emptyʗ3 = empty;
    @try(emptyMapDeleteFuncKeyˢ, () => {
        delete(emptyʗ3, () => {
        });
    });
    var emptyʗ4 = empty;
    var sliceʗ2 = Δslice;
    @try(emptyMapAssignIntˢ, () => {
        emptyʗ4[sliceʗ2] = true;
    });
    var fullʗ1 = full;
    var sliceʗ3 = Δslice;
    @try(fullMapLookupIntˢ, () => {
        _ = fullʗ1[sliceʗ3];
    });
    var fullʗ2 = full;
    @try(fullMapAssignIntˢ, () => {
        fullʗ2[new @string[]{"x"u8}.slice()] = true;
    });
    var nilMapʗ1 = nilMap;
    var sliceʗ4 = Δslice;
    @try(nilMapLookupIntˢ, () => {
        _ = nilMapʗ1[sliceʗ4];
    });
    var nilMapʗ2 = nilMap;
    var sliceʗ5 = Δslice;
    @try(nilMapDeleteIntˢ, () => {
        delete(nilMapʗ2, sliceʗ5);
    });
    var emptyʗ5 = empty;
    @try(hashableKeyOnEmptyMapˢ, () => {
        _ = emptyʗ5[new nint[]{1, 2}.array()];
    });
    fmt.Println(fullMapLenAfterˢ, len(full), emptyMapLenAfterˢ, len(empty));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string reflectMapIndexIntOnˢ = "reflect MapIndex []int on empty MapOf(any)"u8;
private static readonly @string reflectSetMapIndexIntˢ = "reflect SetMapIndex []int"u8;
private static readonly @string reflectMapIndexIntOnNilˢ = "reflect MapIndex []int on nil map"u8;
private static readonly @string reflectDeleteIntOnNilMapˢ = "reflect delete []int on nil map"u8;
private static readonly object reflectFloat64KeyAfter0ˢ = (@string)"reflect float64 key after +0 then -0:"u8;

internal static void reflectSide() {
    ref var m = ref heap<reflectꓸValue>(out var Ꮡm);
    m = reflect.MakeMap(reflect.MapOf(reflect.TypeFor<any>(), reflect.TypeFor<bool>()));
    slice<nint> Δslice = default!;
    var mʗ1 = m;
    var sliceʗ1 = Δslice;
    @try(reflectMapIndexIntOnˢ, () => {
        mʗ1.MapIndex(reflect.ValueOf(sliceʗ1));
    });
    var mʗ2 = m;
    var sliceʗ2 = Δslice;
    @try(reflectSetMapIndexIntˢ, () => {
        mʗ2.SetMapIndex(reflect.ValueOf(sliceʗ2), reflect.ValueOf(true));
    });
    ref var nilm = ref heap<reflectꓸValue>(out var Ꮡnilm);
    nilm = reflect.Zero(reflect.TypeFor<map<any, bool>>());
    var nilmʗ1 = nilm;
    var sliceʗ3 = Δslice;
    @try(reflectMapIndexIntOnNilˢ, () => {
        nilmʗ1.MapIndex(reflect.ValueOf(sliceʗ3));
    });
    var nilmʗ2 = nilm;
    var sliceʗ4 = Δslice;
    @try(reflectDeleteIntOnNilMapˢ, () => {
        nilmʗ2.SetMapIndex(reflect.ValueOf(sliceʗ4), new reflectꓸValue(nil));
    });
    var fm = reflect.MakeMap(reflect.MapOf(reflect.TypeFor<float64>(), reflect.TypeFor<bool>()));
    fm.SetMapIndex(reflect.ValueOf(0.0D), reflect.ValueOf(true));
    fm.SetMapIndex(reflect.ValueOf(negZero), reflect.ValueOf(true));
    var iter = fm.MapRange();
    while (iter.Next()) {
        fmt.Println(reflectFloat64KeyAfter0ˢ, sign(iter.Key().Float()), lenˢ, fm.Len());
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
