// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using synctest = @internal.synctest_package;
using Δruntime = runtime_package;
using testing = testing_package;
using @internal;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testsynctestˢ = "testsynctest"u8;

public static void TestSynctest(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testsynctestˢ, ""u8);
    @string want = successˢ;
    if (output != want) {
        Ꮡt.Fatalf("output:\n%s\n\nwanted:\n%s"u8, output, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object mismatchRuntimeˢ = (@string)"mismatch: runtime.BubbleAssoc? != synctest.*"u8;

// TestSynctestAssocConsts verifies that constants defined
// in both runtime and internal/synctest match.
public static void TestSynctestAssocConsts(ж<testing.T> Ꮡt) {
    if (runtime_internal_test_package.BubbleAssocUnbubbled != synctest.Unbubbled || runtime_internal_test_package.BubbleAssocCurrentBubble != synctest.CurrentBubble || runtime_internal_test_package.BubbleAssocOtherBubble != synctest.OtherBubble) {
        Ꮡt.Fatal(mismatchRuntimeˢ);
    }
}

} // end runtime_test_package
