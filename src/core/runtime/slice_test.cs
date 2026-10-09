// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

public static UntypedInt N => 20;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mallocmoveˢ = "mallocmove"u8;
internal static readonly @string makecopyˢ = "makecopy"u8;
internal static readonly @string nilappendˢ = "nilappend"u8;

public static void BenchmarkMakeSliceCopy(ж<testing.B> Ꮡb) {
    UntypedInt length = 32;
    slice<byte> bytes = new slice<byte>(8 * length);
    slice<nint> ints = new slice<nint>(length);
    slice<ж<byte>> ptrs = new slice<ж<byte>>(length);
    var bytesʗ1 = bytes;
    var intsʗ1 = ints;
    var ptrsʗ1 = ptrs;
    Ꮡb.Run(mallocmoveˢ, (ж<testing.B> bΔ1) => {
        var bytesʗ2 = bytesʗ1;
        bΔ1.Run(byteˢ, (ж<testing.B> bΔ2) => {
            slice<byte> x = default!;
            for (nint i = 0; i < (~bΔ2).N; i++) {
                x = new slice<byte>(len(bytesʗ2));
                copy(x, bytesʗ2);
            }
        });
        var intsʗ2 = intsʗ1;
        bΔ1.Run(intˢ, (ж<testing.B> bΔ3) => {
            slice<nint> x = default!;
            for (nint i = 0; i < (~bΔ3).N; i++) {
                x = new slice<nint>(len(intsʗ2));
                copy(x, intsʗ2);
            }
        });
        var ptrsʗ2 = ptrsʗ1;
        bΔ1.Run(ptrˢ, (ж<testing.B> bΔ4) => {
            slice<ж<byte>> x = default!;
            for (nint i = 0; i < (~bΔ4).N; i++) {
                x = new slice<ж<byte>>(len(ptrsʗ2));
                copy(x, ptrsʗ2);
            }
        });
    });
    var bytesʗ3 = bytes;
    var intsʗ3 = ints;
    var ptrsʗ3 = ptrs;
    Ꮡb.Run(makecopyˢ, (ж<testing.B> bΔ5) => {
        var bytesʗ4 = bytesʗ3;
        bΔ5.Run(byteˢ, (ж<testing.B> bΔ6) => {
            slice<byte> x = default!;
            for (nint i = 0; i < (~bΔ6).N; i++) {
                x = new slice<byte>(8 * length);
                copy(x, bytesʗ4);
            }
        });
        var intsʗ4 = intsʗ3;
        bΔ5.Run(intˢ, (ж<testing.B> bΔ7) => {
            slice<nint> x = default!;
            for (nint i = 0; i < (~bΔ7).N; i++) {
                x = new slice<nint>(length);
                copy(x, intsʗ4);
            }
        });
        var ptrsʗ4 = ptrsʗ3;
        bΔ5.Run(ptrˢ, (ж<testing.B> bΔ8) => {
            slice<ж<byte>> x = default!;
            for (nint i = 0; i < (~bΔ8).N; i++) {
                x = new slice<ж<byte>>(length);
                copy(x, ptrsʗ4);
            }
        });
    });
    var bytesʗ5 = bytes;
    var intsʗ5 = ints;
    var ptrsʗ5 = ptrs;
    Ꮡb.Run(nilappendˢ, (ж<testing.B> bΔ9) => {
        var bytesʗ6 = bytesʗ5;
        bΔ9.Run(byteˢ, (ж<testing.B> bΔ10) => {
            slice<byte> x = default!;
            for (nint i = 0; i < (~bΔ10).N; i++) {
                x = appendꓸꓸꓸ(slice<byte>(default!), bytesʗ6);
                _ = x;
            }
        });
        var intsʗ6 = intsʗ5;
        bΔ9.Run(intˢ, (ж<testing.B> bΔ11) => {
            slice<nint> x = default!;
            for (nint i = 0; i < (~bΔ11).N; i++) {
                x = appendꓸꓸꓸ(slice<nint>(default!), intsʗ6);
                _ = x;
            }
        });
        var ptrsʗ6 = ptrsʗ5;
        bΔ9.Run(ptrˢ, (ж<testing.B> bΔ12) => {
            slice<ж<byte>> x = default!;
            for (nint i = 0; i < (~bΔ12).N; i++) {
                x = appendꓸꓸꓸ(slice<ж<byte>>(default!), ptrsʗ6);
                _ = x;
            }
        });
    });
}

partial struct struct24 {
    internal int64 a, b, c;
}

partial struct struct32 {
    internal int64 a, b, c, d;
}

partial struct struct40 {
    internal int64 a, b, c, d, e;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string int16ˢ = "Int16"u8;

public static void BenchmarkMakeSlice(ж<testing.B> Ꮡb) {
    UntypedInt length = 2;
    Ꮡb.Run(byteˢ, (ж<testing.B> bΔ1) => {
        slice<byte> x = default!;
        for (nint i = 0; i < (~bΔ1).N; i++) {
            x = new slice<byte>(length, 2 * length);
            _ = x;
        }
    });
    Ꮡb.Run(int16ˢ, (ж<testing.B> bΔ2) => {
        slice<int16> x = default!;
        for (nint i = 0; i < (~bΔ2).N; i++) {
            x = new slice<int16>(length, 2 * length);
            _ = x;
        }
    });
    Ꮡb.Run(intˢ, (ж<testing.B> bΔ3) => {
        slice<nint> x = default!;
        for (nint i = 0; i < (~bΔ3).N; i++) {
            x = new slice<nint>(length, 2 * length);
            _ = x;
        }
    });
    Ꮡb.Run(ptrˢ, (ж<testing.B> bΔ4) => {
        slice<ж<byte>> x = default!;
        for (nint i = 0; i < (~bΔ4).N; i++) {
            x = new slice<ж<byte>>(length, 2 * length);
            _ = x;
        }
    });
    Ꮡb.Run(structˢ3, (ж<testing.B> bΔ5) => {
        bΔ5.Run("24"u8, (ж<testing.B> bΔ6) => {
            slice<struct24> x = default!;
            for (nint i = 0; i < (~bΔ6).N; i++) {
                x = new slice<struct24>(length, 2 * length);
                _ = x;
            }
        });
        bΔ5.Run("32"u8, (ж<testing.B> bΔ7) => {
            slice<struct32> x = default!;
            for (nint i = 0; i < (~bΔ7).N; i++) {
                x = new slice<struct32>(length, 2 * length);
                _ = x;
            }
        });
        bΔ5.Run("40"u8, (ж<testing.B> bΔ8) => {
            slice<struct40> x = default!;
            for (nint i = 0; i < (~bΔ8).N; i++) {
                x = new slice<struct40>(length, 2 * length);
                _ = x;
            }
        });
    });
}

public static void BenchmarkGrowSlice(ж<testing.B> Ꮡb) {
    Ꮡb.Run(byteˢ, (ж<testing.B> bΔ1) => {
        var x = new slice<byte>(9);
        for (nint i = 0; i < (~bΔ1).N; i++) {
            _ = appendꓸꓸꓸ(slice<byte>(default!), x);
        }
    });
    Ꮡb.Run(int16ˢ, (ж<testing.B> bΔ2) => {
        var x = new slice<int16>(9);
        for (nint i = 0; i < (~bΔ2).N; i++) {
            _ = appendꓸꓸꓸ(slice<int16>(default!), x);
        }
    });
    Ꮡb.Run(intˢ, (ж<testing.B> bΔ3) => {
        var x = new slice<nint>(9);
        for (nint i = 0; i < (~bΔ3).N; i++) {
            _ = appendꓸꓸꓸ(slice<nint>(default!), x);
        }
    });
    Ꮡb.Run(ptrˢ, (ж<testing.B> bΔ4) => {
        var x = new slice<ж<byte>>(9);
        for (nint i = 0; i < (~bΔ4).N; i++) {
            _ = appendꓸꓸꓸ(slice<ж<byte>>(default!), x);
        }
    });
    Ꮡb.Run(structˢ3, (ж<testing.B> bΔ5) => {
        bΔ5.Run("24"u8, (ж<testing.B> bΔ6) => {
            var x = new slice<struct24>(9);
            for (nint i = 0; i < (~bΔ6).N; i++) {
                _ = appendꓸꓸꓸ(slice<struct24>(default!), x);
            }
        });
        bΔ5.Run("32"u8, (ж<testing.B> bΔ7) => {
            var x = new slice<struct32>(9);
            for (nint i = 0; i < (~bΔ7).N; i++) {
                _ = appendꓸꓸꓸ(slice<struct32>(default!), x);
            }
        });
        bΔ5.Run("40"u8, (ж<testing.B> bΔ8) => {
            var x = new slice<struct40>(9);
            for (nint i = 0; i < (~bΔ8).N; i++) {
                _ = appendꓸꓸꓸ(slice<struct40>(default!), x);
            }
        });
    });
}

public static slice<nint> SinkIntSlice;
public static slice<ж<nint>> SinkIntPointerSlice;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string intSliceˢ = "IntSlice"u8;
internal static readonly @string pointerSliceˢ = "PointerSlice"u8;
internal static readonly @string noGrowˢ = "NoGrow"u8;

public static void BenchmarkExtendSlice(ж<testing.B> Ꮡb) {
    nint length = 4; // Use a variable to prevent stack allocation of slices.
    Ꮡb.Run(intSliceˢ, (ж<testing.B> bΔ1) => {
        var s = new slice<nint>(0, length);
        for (nint i = 0; i < (~bΔ1).N; i++) {
            s = appendꓸꓸꓸ(s.slice(0, 0, length / 2), makeꓸꓸꓸ<nint>(length));
        }
        SinkIntSlice = s;
    });
    Ꮡb.Run(pointerSliceˢ, (ж<testing.B> bΔ2) => {
        var s = new slice<ж<nint>>(0, length);
        for (nint i = 0; i < (~bΔ2).N; i++) {
            s = appendꓸꓸꓸ(s.slice(0, 0, length / 2), makeꓸꓸꓸ<ж<nint>>(length));
        }
        SinkIntPointerSlice = s;
    });
    Ꮡb.Run(noGrowˢ, (ж<testing.B> bΔ3) => {
        var s = new slice<nint>(0, length);
        for (nint i = 0; i < (~bΔ3).N; i++) {
            s = appendꓸꓸꓸ(s.slice(0, 0, length), makeꓸꓸꓸ<nint>(length));
        }
        SinkIntSlice = s;
    });
}

public static void BenchmarkAppend(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var x = new slice<nint>(0, N);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        x = x[0..0];
        for (nint j = 0; j < N; j++) {
            x = append(x, j);
        }
    }
}

public static void BenchmarkAppendGrowByte(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        slice<byte> x = default!;
        for (nint j = 0; j < (1 << (int)(20)); j++) {
            x = append(x, (byte)j);
        }
    }
}

public static void BenchmarkAppendGrowString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string s = default!;
    for (nint i = 0; i < b.N; i++) {
        slice<@string> x = default!;
        for (nint j = 0; j < (1 << (int)(20)); j++) {
            x = append(x, s);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bytesˢ = (@string)"Bytes"u8;

public static void BenchmarkAppendSlice(ж<testing.B> Ꮡb) {
    foreach (var (_, length) in new nint[]{1, 4, 7, 8, 15, 16, 32}.slice()) {
        Ꮡb.Run(fmt.Sprint(length, bytesˢ), (ж<testing.B> bΔ1) => {
            var x = new slice<byte>(0, N);
            var y = new slice<byte>(length);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                x = x[0..0];
                x = appendꓸꓸꓸ(x, y);
            }
        });
    }
}

internal static slice<byte> blackhole;

public static void BenchmarkAppendSliceLarge(ж<testing.B> Ꮡb) {
    foreach (var (_, length) in new nint[]{(1 << (int)(10)), (4 << (int)(10)), (16 << (int)(10)), (64 << (int)(10)), (256 << (int)(10)), (1024 << (int)(10))}.slice()) {
        var y = new slice<byte>(length);
        var yʗ1 = y;
        Ꮡb.Run(fmt.Sprint(length, bytesˢ), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                blackhole = default!;
                blackhole = appendꓸꓸꓸ(blackhole, yʗ1);
            }
        });
    }
}

public static void BenchmarkAppendStr(ж<testing.B> Ꮡb) {
    foreach (var (_, str) in new @string[]{
        "1"u8,
        "1234"u8,
        "12345678"u8,
        "1234567890123456"u8,
        "12345678901234567890123456789012"u8
    }.slice()) {
        Ꮡb.Run(fmt.Sprint(len(str), bytesˢ), (ж<testing.B> bΔ1) => {
            var x = new slice<byte>(0, N);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                x = x[0..0];
                x = append(x, str.ꓸꓸꓸ);
            }
        });
    }
}

public static void BenchmarkAppendSpecialCase(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var x = new slice<nint>(0, N);
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        x = x[0..0];
        for (nint j = 0; j < N; j++) {
            if (len(x) < cap(x)){
                x = x.slice(0, len(x) + 1);
                x[len(x) - 1] = j;
            } else {
                x = append(x, j);
            }
        }
    }
}

internal static slice<nint> x;

internal static nint f() {
    x[..1][0] = 3;
    return 2;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object appendFailedˢ = (@string)"append failed: "u8;

public static void TestSideEffectOrder(ж<testing.T> Ꮡt) {
    x = new slice<nint>(0, 10);
    x = append(x, (nint)(1), f());
    if (x[0] != 1 || x[1] != 2) {
        Ꮡt.Error(appendFailedˢ, x[0], x[1]);
    }
}

public static void TestAppendOverlap(ж<testing.T> Ꮡt) {
    var x = slice<byte>("1234"u8);
    x = appendꓸꓸꓸ(x[1..], x); // p > q in runtime·appendslice.
    @string got = ((@string)x);
    @string want = "2341234"u8;
    if (got != want) {
        Ꮡt.Errorf("overlap failed: got %q want %q"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object stringˢ = (@string)"String"u8;

public static void BenchmarkCopy(ж<testing.B> Ꮡb) {
    foreach (var (_, l) in new nint[]{1, 2, 4, 8, 12, 16, 32, 128, 1024}.slice()) {
        var buf = new slice<byte>(4096);
        var bufʗ1 = buf;
        Ꮡb.Run(fmt.Sprint(l, byteˢ), (ж<testing.B> bΔ1) => {
            var s = new slice<byte>(l);
            nint n = default!;
            for (nint i = 0; i < (~bΔ1).N; i++) {
                n = copy(bufʗ1, s);
            }
            bΔ1.SetBytes((int64)n);
        });
        var bufʗ2 = buf;
        Ꮡb.Run(fmt.Sprint(l, stringˢ), (ж<testing.B> bΔ2) => {
            @string s = ((@string)new slice<byte>(l));
            nint n = default!;
            for (nint i = 0; i < (~bΔ2).N; i++) {
                n = copy(bufʗ2, s);
            }
            bΔ2.SetBytes((int64)n);
        });
    }
}

internal static slice<byte> sByte;
internal static slice<uintptr> s1Ptr;
internal static slice<array<uintptr>> s2Ptr;
internal static slice<array<uintptr>> s3Ptr;
internal static slice<array<uintptr>> s4Ptr;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string growˢ = "Grow"u8;

// BenchmarkAppendInPlace tests the performance of append
// when the result is being written back to the same slice.
// In order for the in-place optimization to occur,
// the slice must be referred to by address;
// using a global is an easy way to trigger that.
// We test the "grow" and "no grow" paths separately,
// but not the "normal" (occasionally grow) path,
// because it is a blend of the other two.
// We use small numbers and small sizes in an attempt
// to avoid benchmarking memory allocation and copying.
// We use scalars instead of pointers in an attempt
// to avoid benchmarking the write barriers.
// We benchmark four common sizes (byte, pointer, string/interface, slice),
// and one larger size.
public static void BenchmarkAppendInPlace(ж<testing.B> Ꮡb) {
    Ꮡb.Run(noGrowˢ, (ж<testing.B> bΔ1) => {
        const nint C = 128;
        bΔ1.Run(byteˢ, (ж<testing.B> bΔ2) => {
            for (nint i = 0; i < (~bΔ2).N; i++) {
                sByte = new slice<byte>(C);
                for (nint j = 0; j < C; j++) {
                    sByte = append(sByte, (byte)(0x77));
                }
            }
        });
        bΔ1.Run("1Ptr"u8, (ж<testing.B> bΔ3) => {
            for (nint i = 0; i < (~bΔ3).N; i++) {
                s1Ptr = new slice<uintptr>(C);
                for (nint j = 0; j < C; j++) {
                    s1Ptr = append(s1Ptr, (uintptr)(0x77));
                }
            }
        });
        bΔ1.Run("2Ptr"u8, (ж<testing.B> bΔ4) => {
            for (nint i = 0; i < (~bΔ4).N; i++) {
                s2Ptr = GoReflect.WithElemDims(new slice<array<uintptr>>(C, () => new(2)), 2);
                for (nint j = 0; j < C; j++) {
                    s2Ptr = append(s2Ptr, new uintptr[]{0x77, 0x88}.array());
                }
            }
        });
        bΔ1.Run("3Ptr"u8, (ж<testing.B> bΔ5) => {
            for (nint i = 0; i < (~bΔ5).N; i++) {
                s3Ptr = GoReflect.WithElemDims(new slice<array<uintptr>>(C, () => new(3)), 3);
                for (nint j = 0; j < C; j++) {
                    s3Ptr = append(s3Ptr, new uintptr[]{0x77, 0x88, 0x99}.array());
                }
            }
        });
        bΔ1.Run("4Ptr"u8, (ж<testing.B> bΔ6) => {
            for (nint i = 0; i < (~bΔ6).N; i++) {
                s4Ptr = GoReflect.WithElemDims(new slice<array<uintptr>>(C, () => new(4)), 4);
                for (nint j = 0; j < C; j++) {
                    s4Ptr = append(s4Ptr, new uintptr[]{0x77, 0x88, 0x99, 0xAA}.array());
                }
            }
        });
    });
    Ꮡb.Run(growˢ, (ж<testing.B> bΔ7) => {
        const nint C = 5;
        bΔ7.Run(byteˢ, (ж<testing.B> bΔ8) => {
            for (nint i = 0; i < (~bΔ8).N; i++) {
                sByte = new slice<byte>(0);
                for (nint j = 0; j < C; j++) {
                    sByte = append(sByte, (byte)(0x77));
                    sByte = sByte.slice(0, cap(sByte));
                }
            }
        });
        bΔ7.Run("1Ptr"u8, (ж<testing.B> bΔ9) => {
            for (nint i = 0; i < (~bΔ9).N; i++) {
                s1Ptr = new slice<uintptr>(0);
                for (nint j = 0; j < C; j++) {
                    s1Ptr = append(s1Ptr, (uintptr)(0x77));
                    s1Ptr = s1Ptr.slice(0, cap(s1Ptr));
                }
            }
        });
        bΔ7.Run("2Ptr"u8, (ж<testing.B> bΔ10) => {
            for (nint i = 0; i < (~bΔ10).N; i++) {
                s2Ptr = GoReflect.WithElemDims(new slice<array<uintptr>>(0, () => new(2)), 2);
                for (nint j = 0; j < C; j++) {
                    s2Ptr = append(s2Ptr, new uintptr[]{0x77, 0x88}.array());
                    s2Ptr = GoReflect.WithElemDims(s2Ptr.slice(0, cap(s2Ptr)), 2);
                }
            }
        });
        bΔ7.Run("3Ptr"u8, (ж<testing.B> bΔ11) => {
            for (nint i = 0; i < (~bΔ11).N; i++) {
                s3Ptr = GoReflect.WithElemDims(new slice<array<uintptr>>(0, () => new(3)), 3);
                for (nint j = 0; j < C; j++) {
                    s3Ptr = append(s3Ptr, new uintptr[]{0x77, 0x88, 0x99}.array());
                    s3Ptr = GoReflect.WithElemDims(s3Ptr.slice(0, cap(s3Ptr)), 3);
                }
            }
        });
        bΔ7.Run("4Ptr"u8, (ж<testing.B> bΔ12) => {
            for (nint i = 0; i < (~bΔ12).N; i++) {
                s4Ptr = GoReflect.WithElemDims(new slice<array<uintptr>>(0, () => new(4)), 4);
                for (nint j = 0; j < C; j++) {
                    s4Ptr = append(s4Ptr, new uintptr[]{0x77, 0x88, 0x99, 0xAA}.array());
                    s4Ptr = GoReflect.WithElemDims(s4Ptr.slice(0, cap(s4Ptr)), 4);
                }
            }
        });
    });
}

} // end runtime_test_package
