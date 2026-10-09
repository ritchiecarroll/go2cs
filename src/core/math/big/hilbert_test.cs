// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// A little test program and benchmark for rational arithmetics.
// Computes a Hilbert matrix, its inverse, multiplies them
// and verifies that the product is the identity matrix.
namespace go.math;

using fmt = fmt_package;
using testing = testing_package;
using static go.math.big_package;

partial class big_internal_test_package {

internal partial struct matrix {
    internal nint n, m;
    internal slice<ж<global::go.math.big_package.ΔRat>> a;
}

internal static ж<global::go.math.big_package.ΔRat> at(this ref matrix a, nint i, nint j) {
    if (!(0 <= i && i < a.n && 0 <= j && j < a.m)) {
        throw panic("index out of range");
    }
    return a.a[i * a.m + j];
}

internal static void set(this ref matrix a, nint i, nint j, ж<global::go.math.big_package.ΔRat> Ꮡx) {
    if (!(0 <= i && i < a.n && 0 <= j && j < a.m)) {
        throw panic("index out of range");
    }
    a.a[i * a.m + j] = Ꮡx;
}

internal static ж<matrix> newMatrix(nint n, nint m) {
    if (!(0 <= n && 0 <= m)) {
        throw panic("illegal matrix");
    }
    var a = @new<matrix>();
    a.Value.n = n;
    a.Value.m = m;
    a.Value.a = new slice<ж<global::go.math.big_package.ΔRat>>(n * m);
    return a;
}

internal static ж<matrix> newUnit(nint n) {
    var a = newMatrix(n, n);
    for (nint i = 0; i < n; i++) {
        for (nint j = 0; j < n; j++) {
            var x = NewRat(0, 1);
            if (i == j) {
                x.SetInt64(1);
            }
            a.set(i, j, x);
        }
    }
    return a;
}

internal static ж<matrix> newHilbert(nint n) {
    var a = newMatrix(n, n);
    for (nint i = 0; i < n; i++) {
        for (nint j = 0; j < n; j++) {
            a.set(i, j, NewRat(1, (int64)(i + j + 1)));
        }
    }
    return a;
}

internal static ж<matrix> newInverseHilbert(nint n) {
    var a = newMatrix(n, n);
    for (nint i = 0; i < n; i++) {
        for (nint j = 0; j < n; j++) {
            var x1 = @new<global::go.math.big_package.ΔRat>().SetInt64((int64)(i + j + 1));
            var x2 = @new<global::go.math.big_package.ΔRat>().SetInt(@new<global::go.math.big_package.ΔInt>().Binomial((int64)(n + i), (int64)(n - j - 1)));
            var x3 = @new<global::go.math.big_package.ΔRat>().SetInt(@new<global::go.math.big_package.ΔInt>().Binomial((int64)(n + j), (int64)(n - i - 1)));
            var x4 = @new<global::go.math.big_package.ΔRat>().SetInt(@new<global::go.math.big_package.ΔInt>().Binomial((int64)(i + j), (int64)i));
            x1.Mul(x1, x2);
            x1.Mul(x1, x3);
            x1.Mul(x1, x4);
            x1.Mul(x1, x4);
            if ((nint)((i + j) & 1) != 0) {
                x1.Neg(x1);
            }
            a.set(i, j, x1);
        }
    }
    return a;
}

internal static ж<matrix> mul(this ref matrix a, ж<matrix> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (a.m != b.n) {
        throw panic("illegal matrix multiply");
    }
    var c = newMatrix(a.n, b.m);
    for (nint i = 0; i < (~c).n; i++) {
        for (nint j = 0; j < (~c).m; j++) {
            var x = NewRat(0, 1);
            for (nint k = 0; k < a.m; k++) {
                x.Add(x, @new<global::go.math.big_package.ΔRat>().Mul(a.at(i, k), b.at(k, j)));
            }
            c.set(i, j, x);
        }
    }
    return c;
}

internal static bool eql(this ref matrix a, ж<matrix> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (a.n != b.n || a.m != b.m) {
        return false;
    }
    for (nint i = 0; i < a.n; i++) {
        for (nint j = 0; j < a.m; j++) {
            if (a.at(i, j).Cmp(b.at(i, j)) != 0) {
                return false;
            }
        }
    }
    return true;
}

internal static @string String(this ref matrix a) {
    @string s = ""u8;
    for (nint i = 0; i < a.n; i++) {
        for (nint j = 0; j < a.m; j++) {
            s += fmt.Sprintf("\t%s"u8, a.at(i, j).OrTypedNil());
        }
        s += "\n"u8;
    }
    return s;
}

internal static void doHilbert(ж<testing.T> Ꮡt, nint n) {
    var a = newHilbert(n);
    var b = newInverseHilbert(n);
    var I = newUnit(n);
    var ab = a.mul(b);
    if (!ab.eql(I)) {
        if (Ꮡt == nil) {
            throw panic("Hilbert failed");
        }
        Ꮡt.Errorf("a   = %s\n"u8, a.OrTypedNil());
        Ꮡt.Errorf("b   = %s\n"u8, b.OrTypedNil());
        Ꮡt.Errorf("a*b = %s\n"u8, ab.OrTypedNil());
        Ꮡt.Errorf("I   = %s\n"u8, I.OrTypedNil());
    }
}

public static void TestHilbert(ж<testing.T> Ꮡt) {
    doHilbert(Ꮡt, 10);
}

public static void BenchmarkHilbert(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        doHilbert(nil, 10);
    }
}

} // end big_internal_test_package
