// Copyright 2026 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δmath = math_package;
using Δruntime = runtime_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

public static void FuzzPrintFloat64(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    f.Add((float64)(Δmath.SmallestNonzeroFloat64));
    f.Add((float64)(Δmath.MaxFloat64));
    f.Add(-1.7976931348623157e+308D); // requires 24 digits

    Ꮡf.Fuzz((ж<testing.T> t, float64 v) => {
        @string s = runtime_internal_test_package.DumpPrint(v);
        if (len(s) > runtime_internal_test_package.Float64Bytes) {
            t.Errorf("print(%f) got %s (len %d) want len <= %d"u8, v, s, len(s), runtime_internal_test_package.Float64Bytes);
        }
    });
}

public static void FuzzPrintFloat32(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    f.Add((float32)Δmath.SmallestNonzeroFloat32);
    f.Add((float32)Δmath.MaxFloat32);
    f.Add((float32)(-1.06338233e+37F)); // requires 15 digits

    Ꮡf.Fuzz((ж<testing.T> t, float32 v) => {
        @string s = runtime_internal_test_package.DumpPrint(v);
        if (len(s) > runtime_internal_test_package.Float32Bytes) {
            t.Errorf("print(%f) got %s (len %d) want len <= %d"u8, v, s, len(s), runtime_internal_test_package.Float32Bytes);
        }
    });
}

public static void FuzzPrintComplex128(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    f.Add((float64)(Δmath.SmallestNonzeroFloat64), (float64)(Δmath.SmallestNonzeroFloat64));
    f.Add((float64)(Δmath.MaxFloat64), (float64)(Δmath.MaxFloat64));
    f.Add(-1.7976931348623157e+308D, -1.7976931348623157e+308D); // requires 51 digits

    Ꮡf.Fuzz((ж<testing.T> t, float64 r, float64 i) => {
        var v = complex(r, i);
        @string s = runtime_internal_test_package.DumpPrint(v);
        if (len(s) > runtime_internal_test_package.Complex128Bytes) {
            t.Errorf("print(%f) got %s (len %d) want len <= %d"u8, v, s, len(s), runtime_internal_test_package.Complex128Bytes);
        }
    });
}

public static void FuzzPrintComplex64(ж<testing.F> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    f.Add((float32)Δmath.SmallestNonzeroFloat32, (float32)Δmath.SmallestNonzeroFloat32);
    f.Add((float32)Δmath.MaxFloat32, (float32)Δmath.MaxFloat32);
    f.Add((float32)(-1.06338233e+37F), (float32)(-1.06338233e+37F)); // requires 33 digits

    Ꮡf.Fuzz((ж<testing.T> t, float32 r, float32 i) => {
        var v = complex(r, i);
        @string s = runtime_internal_test_package.DumpPrint(v);
        if (len(s) > runtime_internal_test_package.Complex64Bytes) {
            t.Errorf("print(%f) got %s (len %d) want len <= %d"u8, v, s, len(s), runtime_internal_test_package.Complex64Bytes);
        }
    });
}

} // end runtime_test_package
