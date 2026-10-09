// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using reflect = reflect_package;
using Δruntime = runtime_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

internal partial struct TestSizeof_type /*dyn*/ {
    internal any val;     // type as a value
    internal uintptr _32bit; // size on 32bit platforms
    internal uintptr _64bit; // size on 64bit platforms
}

// Assert that the size of important structures do not change unexpectedly.
public static void TestSizeof(ж<testing.T> Ꮡt) {
    const bool _64bit = /* unsafe.Sizeof(uintptr(0)) == 8 */ true;
// g, but exported for testing
// sudog, but exported for testing
    slice<TestSizeof_type> tests = new TestSizeof_type[]{
        new(new G(), 280, 440),
        new(new Sudog(), 56, 88)
    }.slice();
    foreach (var (_, tt) in tests) {
        var want = tt._32bit;
        if (_64bit) {
            want = tt._64bit;
        }
        var got = reflect.TypeOf(tt.val).Size();
        if (want != got) {
            Ꮡt.Errorf("unsafe.Sizeof(%T) = %d, want %d"u8, tt.val, got, want);
        }
    }
}

} // end runtime_test_package
