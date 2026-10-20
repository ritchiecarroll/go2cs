// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using testing = testing_package;

partial class errors_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string abcˢ = "abc"u8;
private static readonly @string xyzˢ = "xyz"u8;
private static readonly @string jklˢ = "jkl"u8;

public static void TestNewEqual(ж<testing.T> Ꮡt) {
    // Different allocations should not be equal.
    if (AreEqual(errors.New(abcˢ), errors.New(abcˢ))) {
        Ꮡt.Errorf(@"New(""abc"") == New(""abc"")"u8);
    }
    if (AreEqual(errors.New(abcˢ), errors.New(xyzˢ))) {
        Ꮡt.Errorf(@"New(""abc"") == New(""xyz"")"u8);
    }

    // Same allocation should be equal to itself (not crash).
    var err = errors.New(jklˢ);
    if (!AreEqual(err, err)) {
        Ꮡt.Errorf(@"err != err"u8);
    }
}

public static void TestErrorMethod(ж<testing.T> Ꮡt) {
    var err = errors.New(abcˢ);
    if (err.Error() != "abc"u8) {
        Ꮡt.Errorf(@"New(""abc"").Error() = %q, want %q"u8, err.Error(), abcˢ);
    }
}

} // end errors_test_package
