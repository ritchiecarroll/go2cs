// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using strings = strings_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingTestLongSleepsˢ = (@string)"skipping test: long sleeps"u8;
internal static readonly @string windowsUpdateGOMAXPROCSˢ = "WindowsUpdateGOMAXPROCS"u8;
internal static readonly @string skipˢ = "SKIP"u8;

public static void TestGOMAXPROCSUpdate(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingTestLongSleepsˢ);
    }

    @string got = runTestProg(Ꮡt, testprogˢ, windowsUpdateGOMAXPROCSˢ);
    if (strings.Contains(got, skipˢ)) {
        Ꮡt.Skip(got);
    }
    if (!strings.Contains(got, "OK"u8)) {
        Ꮡt.Fatalf("output got %q want OK"u8, got);
    }
}

public static void TestCgroupGOMAXPROCSDontUpdate(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingTestLongSleepsˢ);
    }

    // Two ways to disable updates: explicit GOMAXPROCS or GODEBUG for
    // update feature.
    foreach (var (_, v) in new @string[]{"GOMAXPROCS=4"u8, "GODEBUG=updatemaxprocs=0"u8}.slice()) {
        Ꮡt.Run(v, (ж<testing.T> tΔ1) => {
            @string got = runTestProg(tΔ1, testprogˢ, "WindowsDontUpdateGOMAXPROCS"u8, v);
            if (strings.Contains(got, skipˢ)) {
                tΔ1.Skip(got);
            }
            if (!strings.Contains(got, "OK"u8)) {
                tΔ1.Fatalf("output got %q want OK"u8, got);
            }
        });
    }
}

} // end runtime_test_package
