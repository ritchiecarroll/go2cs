// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

partial interface I1 {
    void Method1();
}

partial interface I2 {
    void Method1();
    void Method2();
}

partial struct TS /*num:uint16*/;

partial struct TM /*num:uintptr*/;

partial struct TL /*[2]uintptr*/;

public static void Method1(this TS _) {
}

public static void Method2(this TS _) {
}

public static void Method1(this TM _) {
}

public static void Method2(this TM _) {
}

public static void Method1(this TL _) {
}

public static void Method2(this TL _) {
}

partial struct T8 /*num:uint8*/;

partial struct T16 /*num:uint16*/;

partial struct T32 /*num:uint32*/;

partial struct T64 /*num:uint64*/;

partial struct Tstr /*@string*/;

partial struct Tslice /*[]byte*/;

public static void Method1(this T8 _) {
}

public static void Method1(this T16 _) {
}

public static void Method1(this T32 _) {
}

public static void Method1(this T64 _) {
}

public static void Method1(this Tstr _) {
}

public static void Method1(this Tslice _) {
}

internal static any e;
internal static any e_;
internal static I1 i1;
internal static I2 i2;
internal static TS ts;
internal static TM tm;
internal static TL tl;
internal static bool ok;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingOnNonGcCompilerˢ = (@string)"skipping on non-gc compiler"u8;

// Issue 9370
public static void TestCmpIfaceConcreteAlloc(ж<testing.T> Ꮡt) {
    if (Δruntime.Compiler != "gc") {
        Ꮡt.Skip(skippingOnNonGcCompilerˢ);
    }
    var n = testing.AllocsPerRun(1, () => {
        _ = AreEqual(e, ts);
        _ = AreEqual(i1, ts);
        _ = AreEqual(e, (nint)(1));
    });
    if (n > 0D) {
        Ꮡt.Fatalf("iface cmp allocs=%v; want 0"u8, n);
    }
}

public static void BenchmarkEqEfaceConcrete(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        _ = AreEqual(e, ts);
    }
}

public static void BenchmarkEqIfaceConcrete(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        _ = AreEqual(i1, ts);
    }
}

public static void BenchmarkNeEfaceConcrete(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        _ = !AreEqual(e, ts);
    }
}

public static void BenchmarkNeIfaceConcrete(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        _ = !AreEqual(i1, ts);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string boolˢ = "bool"u8;
internal static readonly @string uint8ˢ = "uint8"u8;

public static void BenchmarkConvT2EByteSized(ж<testing.B> Ꮡb) {
    Ꮡb.Run(boolˢ, (ж<testing.B> bΔ1) => {
        for (nint i = 0; i < (~bΔ1).N; i++) {
            e = yes;
        }
    });
    Ꮡb.Run(uint8ˢ, (ж<testing.B> bΔ2) => {
        for (nint i = 0; i < (~bΔ2).N; i++) {
            e = eight8;
        }
    });
}

public static void BenchmarkConvT2ESmall(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        e = ts;
    }
}

public static void BenchmarkConvT2EUintptr(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        e = tm;
    }
}

public static void BenchmarkConvT2ELarge(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        e = tl.Clone();
    }
}

public static void BenchmarkConvT2ISmall(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        i1 = ts;
    }
}

public static void BenchmarkConvT2IUintptr(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        i1 = tm;
    }
}

public static void BenchmarkConvT2ILarge(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        i1 = tl.Clone();
    }
}

public static void BenchmarkConvI2E(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    i2 = tm;
    for (nint i = 0; i < b.N; i++) {
        e = i2;
    }
}

public static void BenchmarkConvI2I(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    i2 = tm;
    for (nint i = 0; i < b.N; i++) {
        i1 = new runtime_test_package.I2ᴠI1(i2);
    }
}

public static void BenchmarkAssertE2T(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    e = tm;
    for (nint i = 0; i < b.N; i++) {
        tm = e._<TM>();
    }
}

public static void BenchmarkAssertE2TLarge(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    e = tl.Clone();
    for (nint i = 0; i < b.N; i++) {
        tl = e._<TL>();
    }
}

public static void BenchmarkAssertE2I(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    e = tm;
    for (nint i = 0; i < b.N; i++) {
        i1 = e._<I1>();
    }
}

public static void BenchmarkAssertI2T(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    i1 = tm;
    for (nint i = 0; i < b.N; i++) {
        tm = i1._<TM>();
    }
}

public static void BenchmarkAssertI2I(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    i1 = tm;
    for (nint i = 0; i < b.N; i++) {
        i2 = i1._<I2>();
    }
}

public static void BenchmarkAssertI2E(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    i1 = tm;
    for (nint i = 0; i < b.N; i++) {
        e = i1._<any>();
    }
}

public static void BenchmarkAssertE2E(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    e = tm;
    for (nint i = 0; i < b.N; i++) {
        e_ = e;
    }
}

public static void BenchmarkAssertE2T2(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    e = tm;
    for (nint i = 0; i < b.N; i++) {
        (tm, ok) = e._<TM>(ᐧ);
    }
}

public static void BenchmarkAssertE2T2Blank(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    e = tm;
    for (nint i = 0; i < b.N; i++) {
        (_, ok) = e._<TM>(ᐧ);
    }
}

public static void BenchmarkAssertI2E2(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    i1 = tm;
    for (nint i = 0; i < b.N; i++) {
        (e, ok) = i1._<any>(ᐧ);
    }
}

public static void BenchmarkAssertI2E2Blank(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    i1 = tm;
    for (nint i = 0; i < b.N; i++) {
        (_, ok) = i1._<any>(ᐧ);
    }
}

public static void BenchmarkAssertE2E2(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    e = tm;
    for (nint i = 0; i < b.N; i++) {
        (e_, ok) = e._<any>(ᐧ);
    }
}

public static void BenchmarkAssertE2E2Blank(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    e = tm;
    for (nint i = 0; i < b.N; i++) {
        (_, ok) = e._<any>(ᐧ);
    }
}

public static void TestNonEscapingConvT2E(ж<testing.T> Ꮡt) {
    var m = new map<any, bool>();
    m[(nint)(42)] = true;
    if (!m[(nint)(42)]) {
        Ꮡt.Fatalf("42 is not present in the map"u8);
    }
    if (m[(nint)(0)]) {
        Ꮡt.Fatalf("0 is present in the map"u8);
    }
    var mʗ1 = m;
    var n = testing.AllocsPerRun(1000, () => {
        if (mʗ1[(nint)(0)]) {
            Ꮡt.Fatalf("0 is present in the map"u8);
        }
    });
    if (n != 0D) {
        Ꮡt.Fatalf("want 0 allocs, got %v"u8, n);
    }
}

public static void TestNonEscapingConvT2I(ж<testing.T> Ꮡt) {
    var m = new map<I1, bool>();
    m[((TM)42)] = true;
    if (!m[((TM)42)]) {
        Ꮡt.Fatalf("42 is not present in the map"u8);
    }
    if (m[((TM)0)]) {
        Ꮡt.Fatalf("0 is present in the map"u8);
    }
    var mʗ1 = m;
    var n = testing.AllocsPerRun(1000, () => {
        if (mʗ1[((TM)0)]) {
            Ꮡt.Fatalf("0 is present in the map"u8);
        }
    });
    if (n != 0D) {
        Ꮡt.Fatalf("want 0 allocs, got %v"u8, n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object changeˢ = (@string)"change"u8;

internal partial struct TestZeroConvT2x_tests /*dyn*/ {
    internal @string name;
    internal Action fn;
}

public static void TestZeroConvT2x(ж<testing.T> Ꮡt) {
    var tests = new TestZeroConvT2x_tests[]{
        new(name: "E8"u8, fn: () => {
            e = eight8; // any byte-sized value does not allocate
        }),
        new(name: "E16"u8, fn: () => {
            e = zero16; // zero values do not allocate
        }),
        new(name: "E32"u8, fn: () => {
            e = zero32;
        }),
        new(name: "E64"u8, fn: () => {
            e = zero64;
        }),
        new(name: "Estr"u8, fn: () => {
            e = zerostr;
        }),
        new(name: "Eslice"u8, fn: () => {
            e = zeroslice;
        }),
        new(name: "Econstflt"u8, fn: () => {
            e = 99.0D; // constants do not allocate
        }),
        new(name: "Econststr"u8, fn: () => {
            e = changeˢ;
        }),
        new(name: "I8"u8, fn: () => {
            i1 = eight8I;
        }),
        new(name: "I16"u8, fn: () => {
            i1 = zero16I;
        }),
        new(name: "I32"u8, fn: () => {
            i1 = zero32I;
        }),
        new(name: "I64"u8, fn: () => {
            i1 = zero64I;
        }),
        new(name: "Istr"u8, fn: () => {
            i1 = zerostrI;
        }),
        new(name: "Islice"u8, fn: () => {
            i1 = zerosliceI;
        })
    }.slice();
    foreach (var (_, vᴛ1) in tests) {
        ref var test = ref heap(new TestZeroConvT2x_tests(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        Ꮡt.Run(test.name, (ж<testing.T> tΔ1) => {
            var n = testing.AllocsPerRun(1000, testʗ1.fn);
            if (n != 0D) {
                tΔ1.Errorf("want zero allocs, got %v"u8, n);
            }
        });
    }
}

internal static uint8 eight8 = 8;
internal static T8 eight8I = 8;
internal static bool yes = true;
internal static uint16 zero16 = 0;
internal static T16 zero16I = 0;
internal static uint16 one16 = 1;
internal static uint16 thousand16 = 1000;
internal static uint32 zero32 = 0;
internal static T32 zero32I = 0;
internal static uint32 one32 = 1;
internal static uint32 thousand32 = 1000;
internal static uint64 zero64 = 0;
internal static T64 zero64I = 0;
internal static uint64 one64 = 1;
internal static uint64 thousand64 = 1000;
internal static @string zerostr = ""u8;
internal static Tstr zerostrI = ""u8;
internal static @string nzstr = "abc"u8;
internal static slice<byte> zeroslice = default!;
internal static Tslice zerosliceI = default!;
internal static slice<byte> nzslice = slice<byte>("abc"u8);
internal static array<byte> zerobig = new(512);
internal static array<byte> nzbig = new array<byte>(512){[511] = 1};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zeroˢ = "zero"u8;
internal static readonly @string strˢ = "str"u8;
internal static readonly @string sliceˢ = "slice"u8;
internal static readonly @string bigˢ = "big"u8;
internal static readonly @string nonzeroˢ = "nonzero"u8;
internal static readonly @string smallintˢ = "smallint"u8;
internal static readonly @string largeintˢ = "largeint"u8;

public static void BenchmarkConvT2Ezero(ж<testing.B> Ꮡb) {
    Ꮡb.Run(zeroˢ, (ж<testing.B> bΔ1) => {
        bΔ1.Run("16"u8, (ж<testing.B> bΔ2) => {
            for (nint i = 0; i < (~bΔ2).N; i++) {
                e = zero16;
            }
        });
        bΔ1.Run("32"u8, (ж<testing.B> bΔ3) => {
            for (nint i = 0; i < (~bΔ3).N; i++) {
                e = zero32;
            }
        });
        bΔ1.Run("64"u8, (ж<testing.B> bΔ4) => {
            for (nint i = 0; i < (~bΔ4).N; i++) {
                e = zero64;
            }
        });
        bΔ1.Run(strˢ, (ж<testing.B> bΔ5) => {
            for (nint i = 0; i < (~bΔ5).N; i++) {
                e = zerostr;
            }
        });
        bΔ1.Run(sliceˢ, (ж<testing.B> bΔ6) => {
            for (nint i = 0; i < (~bΔ6).N; i++) {
                e = zeroslice;
            }
        });
        bΔ1.Run(bigˢ, (ж<testing.B> bΔ7) => {
            for (nint i = 0; i < (~bΔ7).N; i++) {
                e = zerobig.Clone();
            }
        });
    });
    Ꮡb.Run(nonzeroˢ, (ж<testing.B> bΔ8) => {
        bΔ8.Run(strˢ, (ж<testing.B> bΔ9) => {
            for (nint i = 0; i < (~bΔ9).N; i++) {
                e = nzstr;
            }
        });
        bΔ8.Run(sliceˢ, (ж<testing.B> bΔ10) => {
            for (nint i = 0; i < (~bΔ10).N; i++) {
                e = nzslice;
            }
        });
        bΔ8.Run(bigˢ, (ж<testing.B> bΔ11) => {
            for (nint i = 0; i < (~bΔ11).N; i++) {
                e = nzbig.Clone();
            }
        });
    });
    Ꮡb.Run(smallintˢ, (ж<testing.B> bΔ12) => {
        bΔ12.Run("16"u8, (ж<testing.B> bΔ13) => {
            for (nint i = 0; i < (~bΔ13).N; i++) {
                e = one16;
            }
        });
        bΔ12.Run("32"u8, (ж<testing.B> bΔ14) => {
            for (nint i = 0; i < (~bΔ14).N; i++) {
                e = one32;
            }
        });
        bΔ12.Run("64"u8, (ж<testing.B> bΔ15) => {
            for (nint i = 0; i < (~bΔ15).N; i++) {
                e = one64;
            }
        });
    });
    Ꮡb.Run(largeintˢ, (ж<testing.B> bΔ16) => {
        bΔ16.Run("16"u8, (ж<testing.B> bΔ17) => {
            for (nint i = 0; i < (~bΔ17).N; i++) {
                e = thousand16;
            }
        });
        bΔ16.Run("32"u8, (ж<testing.B> bΔ18) => {
            for (nint i = 0; i < (~bΔ18).N; i++) {
                e = thousand32;
            }
        });
        bΔ16.Run("64"u8, (ж<testing.B> bΔ19) => {
            for (nint i = 0; i < (~bΔ19).N; i++) {
                e = thousand64;
            }
        });
    });
}

} // end runtime_test_package
