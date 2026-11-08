// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using testing = testing_package;

partial class errors_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string abcˢ => ref abcˢᵇ;
private static readonly @string abcˢᵇ = "abc"u8;
private static ref readonly @string xyzˢ => ref xyzˢᵇ;
private static readonly @string xyzˢᵇ = "xyz"u8;
private static ref readonly @string jklˢ => ref jklˢᵇ;
private static readonly @string jklˢᵇ = "jkl"u8;

public static void TestNewEqual(ж<testing.T> t) {
    // Different allocations should not be equal.
    if (AreEqual(errors.New(abcˢ), errors.New(abcˢ))) {
        t.Errorf(@"New(""abc"") == New(""abc"")");
    }
    if (AreEqual(errors.New(abcˢ), errors.New(xyzˢ))) {
        t.Errorf(@"New(""abc"") == New(""xyz"")");
    }

    // Same allocation should be equal to itself (not crash).
    var err = errors.New(jklˢ);
    if (!AreEqual(err, err)) {
        t.Errorf(@"err != err");
    }
}

public static partial void TestErrorMethod(ж<testing.T> t) {
    var err = errors.New(abcˢ);
    if (err.Error() != "abc"u8) {
        t.Errorf(@"New(""abc"").Error() = %q, want %q", err.Error(), abcˢ);
    }
}

} // end errors_test_package
