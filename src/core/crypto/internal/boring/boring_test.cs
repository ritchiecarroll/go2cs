// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Most functionality in this package is tested by replacing existing code
// and inheriting that code's tests.
namespace go.crypto.@internal;

using testing = testing_package;
using static go.crypto.@internal.boring_package;

partial class boring_internal_test_package {

// Test that func init does not panic.
public static void TestInit(ж<testing.T> t) { }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedUnreachableToˢ = (@string)"expected Unreachable to panic"u8;

// Test that Unreachable panics.
public static void TestUnreachable(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            if (Enabled) {
                var err = recover(); if (err == nil) {
                    t.Fatal(expectedUnreachableToˢ);
                }
            } else {
                var err = recover(); if (err != nil) {
                    t.Fatalf("expected Unreachable to be a no-op");
                }
            }
        }, ref ᒐ);
        Unreachable();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Test that UnreachableExceptTests does not panic (this is a test).
public static void TestUnreachableExceptTests(ж<testing.T> t) {
    UnreachableExceptTests();
}

} // end boring_internal_test_package
