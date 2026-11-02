// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using reflect = reflect_package;
using testing = testing_package;
using static global::go.go.types_package;

partial class types_internal_test_package {

internal partial struct TestSizeof_type /*dyn*/ {
    internal any val;        // type as a value
    internal uintptr _32bit; // size on 32bit platforms
    internal uintptr _64bit; // size on 64bit platforms
}

// Signal size changes of important structures.
public static void TestSizeof(ж<testing.T> t) {
    const bool _64bit = /* ^uint(0)>>32 != 0 */ true;

    slice<TestSizeof_type> tests = slice<TestSizeof_type>([
        // Types
        new(new Basic(), 16, 32),
        new(new Array(), 16, 24),
        new(new Slice(), 8, 16),
        new(new Struct(), 24, 48),
        new(new Pointer(), 8, 16),
        new(new Tuple(), 12, 24),
        new(new ΔSignature(), 28, 56),
        new(new Union(), 12, 24),
        new(new Interface(), 40, 80),
        new(new Map(), 16, 32),
        new(new Chan(), 12, 24),
        new(new Named(), 68, 128),
        new(new TypeParam(), 28, 48),
        new(new Δterm(), 12, 24),

        // Objects
        new(new PkgName(), 40, 80),
        new(new Const(), 44, 88),
        new(new TypeName(), 36, 72),
        new(new Var(), 44, 88),
        new(new Func(), 44, 88),
        new(new Label(), 40, 80),
        new(new Builtin(), 40, 80),
        new(new Nil(), 36, 72),

        // Misc
        new(new ΔScope(), 44, 88),
        new(new Package(), 44, 88),
        new(new _TypeSet(), 28, 56)
    ]);
    foreach (var (_, test) in tests) {
        var got = reflect.TypeOf(test.val).Size();
        var want = test._32bit;
        if (_64bit) {
            want = test._64bit;
        }
        if (got != want) {
            t.Errorf("unsafe.Sizeof(%T) = %d, want %d", test.val, got, want);
        }
    }
}

} // end types_internal_test_package
