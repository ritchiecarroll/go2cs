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
internal static ref readonly @string windowsUpdateGOMAXPROCSˢ => ref windowsUpdateGOMAXPROCSˢᵇ;
private static readonly @string windowsUpdateGOMAXPROCSˢᵇ = "WindowsUpdateGOMAXPROCS"u8;
internal static ref readonly @string skipˢ => ref skipˢᵇ;
private static readonly @string skipˢᵇ = "SKIP"u8;

public static void TestGOMAXPROCSUpdate(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingTestLongSleepsˢ);
    }

    @string got = runTestProg(t, testprogˢ, windowsUpdateGOMAXPROCSˢ);
    if (strings.Contains(got, skipˢ)) {
        t.Skip(got);
    }
    if (!strings.Contains(got, "OK")) {
        t.Fatalf("output got %q want OK", got);
    }
}

public static void TestCgroupGOMAXPROCSDontUpdate(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingTestLongSleepsˢ);
    }

    // Two ways to disable updates: explicit GOMAXPROCS or GODEBUG for
    // update feature.
    foreach (var (_, v) in range(slice<@string>(["GOMAXPROCS=4", "GODEBUG=updatemaxprocs=0"]))) {
        t.Run(v, (ж<testing.T> tΔ1) => {
            @string got = runTestProg(tΔ1, testprogˢ, "WindowsDontUpdateGOMAXPROCS", v);
            if (strings.Contains(got, skipˢ)) {
                tΔ1.Skip(got);
            }
            if (!strings.Contains(got, "OK")) {
                tΔ1.Fatalf("output got %q want OK", got);
            }
        });
    }
}

} // end runtime_test_package
