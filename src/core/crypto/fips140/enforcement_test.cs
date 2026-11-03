// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using des = go.crypto.des_package;
using fips140 = go.crypto.fips140_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using testenv = go.@internal.testenv_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using cipher = go.crypto.cipher_package;
using exec = os.exec_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using os;
using static go.crypto.fips140_internal_test_package;

partial class fips140_test_package {

internal static void expectAllowed(ж<testing.T> t, @string why, bool expected) {
    t.Helper();
    var result = isAllowed();
    if (result != expected) {
        t.Fatalf("%v: expected: %v, got: %v", why, expected, result);
    }
}

internal static bool isAllowed() {
    var (_, err) = des.NewCipher(new slice<byte>(8));
    return err == nil;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testRunˢ => ref testRunˢᵇ;
private static readonly @string testRunˢᵇ = "-test.run=^TestWithoutEnforcement$"u8;
internal static ref readonly @string testVˢ => ref testVˢᵇ;
private static readonly @string testVˢᵇ = "-test.v"u8;
internal static ref readonly @string disabledˢ => ref disabledˢᵇ;
private static readonly @string disabledˢᵇ = "Disabled"u8;
internal static ref readonly @string beforeEnforcementˢ => ref beforeEnforcementˢᵇ;
private static readonly @string beforeEnforcementˢᵇ = "before enforcement disabled"u8;
internal static ref readonly @string insideWithoutEnforcementˢ => ref insideWithoutEnforcementˢᵇ;
private static readonly @string insideWithoutEnforcementˢᵇ = "inside WithoutEnforcement"u8;
internal static ref readonly @string afterWithoutEnforcementˢ => ref afterWithoutEnforcementˢᵇ;
private static readonly @string afterWithoutEnforcementˢᵇ = "after WithoutEnforcement"u8;
internal static ref readonly @string nestedˢ => ref nestedˢᵇ;
private static readonly @string nestedˢᵇ = "Nested"u8;
internal static ref readonly @string beforeEnforcementBypassˢ => ref beforeEnforcementBypassˢᵇ;
private static readonly @string beforeEnforcementBypassˢᵇ = "before enforcement bypass"u8;
internal static ref readonly @string insideNestedˢ => ref insideNestedˢᵇ;
private static readonly @string insideNestedˢᵇ = "inside nested WithoutEnforcement"u8;
internal static ref readonly @string afterEnforcementBypassˢ => ref afterEnforcementBypassˢᵇ;
private static readonly @string afterEnforcementBypassˢᵇ = "after enforcement bypass"u8;
internal static ref readonly @string goroutineInheritˢ => ref goroutineInheritˢᵇ;
private static readonly @string goroutineInheritˢᵇ = "GoroutineInherit"u8;
internal static readonly object goroutineDidnTInheritˢ = (@string)"goroutine didn't inherit enforcement bypass"u8;
internal static readonly object goroutineInheritedBypassˢ = (@string)"goroutine inherited bypass after WithoutEnforcement return"u8;

public static void TestWithoutEnforcement(ж<testing.T> t) {
    cryptotest.MustSupportFIPS140(t);
    if (!fips140.Enforced()) {
        var cmd = testenv.Command(new fips140_test_package.testing_TжTB(t), testenv.Executable(new fips140_test_package.testing_TжTB(t)), testRunˢ, testVˢ);
        cmd.Env = append(cmd.Environ(), (@string)"GODEBUG=fips140=only");
        var (@out, err) = cmd.CombinedOutput();
        t.Logf("running with GODEBUG=fips140=only:\n%s", @out);
        if (err != nil) {
            t.Errorf("fips140=only subprocess failed: %v", err);
        }
        return;
    }

    t.Run(disabledˢ, (ж<testing.T> tΔ1) => {
        expectAllowed(tΔ1, beforeEnforcementˢ, false);
        fips140.WithoutEnforcement(() => {
            expectAllowed(tΔ1, insideWithoutEnforcementˢ, true);
        });
        // make sure that bypass doesn't live on after returning
        expectAllowed(tΔ1, afterWithoutEnforcementˢ, false);
    });

    t.Run(nestedˢ, (ж<testing.T> tΔ2) => {
        expectAllowed(tΔ2, beforeEnforcementBypassˢ, false);
        fips140.WithoutEnforcement(() => {
            fips140.WithoutEnforcement(() => {
                expectAllowed(tΔ2, insideNestedˢ, true);
            });
            expectAllowed(tΔ2, insideNestedˢ, true);
        });
        expectAllowed(tΔ2, afterEnforcementBypassˢ, false);
    });

    t.Run(goroutineInheritˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ3) => {
        var ch = new channel<bool>(2);
        expectAllowed(tΔ3, beforeEnforcementBypassˢ, false);
        var chʗ1 = ch;
        fips140.WithoutEnforcement([MethodImpl(MethodImplOptions.NoInlining)] () => {
            var chʗ2 = chʗ1;
            goǃ(() => {
                chʗ2.ᐸꟷ(isAllowed());
            });
        });
        var allowed = ᐸꟷ(ch);
        if (!allowed) {
            tΔ3.Fatal(goroutineDidnTInheritˢ);
        }
        var chʗ3 = ch;
        goǃ(() => {
            chʗ3.ᐸꟷ(isAllowed());
        });
        allowed = ᐸꟷ(ch);
        if (allowed) {
            tΔ3.Fatal(goroutineInheritedBypassˢ);
        }
    });
}

} // end fips140_test_package
