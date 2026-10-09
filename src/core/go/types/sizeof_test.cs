// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using reflect = reflect_package;
using testing = testing_package;
using static global::go.go.types_package;

partial class types_internal_test_package {

internal partial struct TestSizeof_type /*dyn*/ {
    internal any val;     // type as a value
    internal uintptr _32bit; // size on 32bit platforms
    internal uintptr _64bit; // size on 64bit platforms
}

// Signal size changes of important structures.
public static void TestSizeof(ж<testing.T> Ꮡt) {
    const bool _64bit = /* ^uint(0)>>32 != 0 */ true;
// Types
// Objects
// Misc
    slice<TestSizeof_type> tests = new TestSizeof_type[]{
        new(new Basic(nil), 16, 32),
        new(new Array(nil), 16, 24),
        new(new Slice(nil), 8, 16),
        new(new Struct(nil), 24, 48),
        new(new Pointer(nil), 8, 16),
        new(new Tuple(nil), 12, 24),
        new(new ΔSignature(nil), 28, 56),
        new(new Union(nil), 12, 24),
        new(new Interface(nil), 40, 80),
        new(new Map(nil), 16, 32),
        new(new Chan(nil), 12, 24),
        new(new Named(nil), 60, 112),
        new(new TypeParam(nil), 28, 48),
        new(new Δterm(nil), 12, 24),
        new(new PkgName(nil), 44, 80),
        new(new Const(nil), 48, 88),
        new(new TypeName(nil), 40, 72),
        new(new Var(nil), 48, 88),
        new(new Func(nil), 48, 88),
        new(new Label(nil), 44, 80),
        new(new Builtin(nil), 44, 80),
        new(new Nil(nil), 40, 72),
        new(new ΔScope(nil), 44, 88),
        new(new Package(nil), 44, 88),
        new(new _TypeSet(nil), 28, 56)
    }.slice();
    foreach (var (_, test) in tests) {
        var got = reflect.TypeOf(test.val).Size();
        var want = test._32bit;
        if (_64bit) {
            want = test._64bit;
        }
        if (got != want) {
            Ꮡt.Errorf("unsafe.Sizeof(%T) = %d, want %d"u8, test.val, got, want);
        }
    }
}

} // end types_internal_test_package
