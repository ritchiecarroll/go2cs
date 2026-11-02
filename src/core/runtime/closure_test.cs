// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using testing = testing_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

internal static nint s;

public static void BenchmarkCallClosure(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        s += func(nint (nint ii) => {
            return 2 * ii;
        })(i);
    }
}

public static void BenchmarkCallClosure1(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        nint j = i;
        s += func(nint (nint ii) => {
            return 2 * ii + j;
        })(i);
    }
}

internal static ж<nint> ss;

public static void BenchmarkCallClosure2(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ref var j = ref heap<nint>(i, out var Ꮡj);
        s += func(nint () => {
            ss = Ꮡj;
            return 2;
        })();
    }
}

internal static ж<nint> addr1(nint xʗp) {
    ref var x = ref heap(xʗp, out var Ꮡx);

    return func(ж<nint> () => {
        return Ꮡx;
    })();
}

public static void BenchmarkCallClosure3(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ss = addr1(i);
    }
}

internal static (nint x, ж<nint> p) addr2() {
    ref var x = ref heap(new nint(), out var Ꮡx);

    return (0, func(ж<nint> () => {
        return Ꮡx;
    })());
}

public static void BenchmarkCallClosure4(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        (_, ss) = addr2();
    }
}

} // end runtime_test_package
