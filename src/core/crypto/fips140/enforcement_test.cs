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

internal static void expectAllowed(ж<testing.T> Ꮡt, @string why, bool expected) {
    Ꮡt.Helper();
    var result = isAllowed();
    if (result != expected) {
        Ꮡt.Fatalf("%v: expected: %v, got: %v"u8, why, expected, result);
    }
}

internal static bool isAllowed() {
    var (_, err) = des.NewCipher(new slice<byte>(8));
    return err == default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testRunˢ = "-test.run=^TestWithoutEnforcement$"u8;
internal static readonly @string testVˢ = "-test.v"u8;
internal static readonly @string disabledˢ = "Disabled"u8;
internal static readonly @string beforeEnforcementˢ = "before enforcement disabled"u8;
internal static readonly @string insideWithoutEnforcementˢ = "inside WithoutEnforcement"u8;
internal static readonly @string afterWithoutEnforcementˢ = "after WithoutEnforcement"u8;
internal static readonly @string nestedˢ = "Nested"u8;
internal static readonly @string beforeEnforcementBypassˢ = "before enforcement bypass"u8;
internal static readonly @string insideNestedˢ = "inside nested WithoutEnforcement"u8;
internal static readonly @string afterEnforcementBypassˢ = "after enforcement bypass"u8;
internal static readonly @string goroutineInheritˢ = "GoroutineInherit"u8;
internal static readonly object goroutineDidnTInheritˢ = (@string)"goroutine didn't inherit enforcement bypass"u8;
internal static readonly object goroutineInheritedBypassˢ = (@string)"goroutine inherited bypass after WithoutEnforcement return"u8;

public static void TestWithoutEnforcement(ж<testing.T> Ꮡt) {
    cryptotest.MustSupportFIPS140(Ꮡt);
    if (!fips140.Enforced()) {
        var cmd = testenv.Command(new fips140_test_package.testing_TжTB(Ꮡt), testenv.Executable(new fips140_test_package.testing_TжTB(Ꮡt)), testRunˢ, testVˢ);
        cmd.Value.Env = append(cmd.Environ(), "GODEBUG=fips140=only"u8);
        var (@out, err) = cmd.CombinedOutput();
        Ꮡt.Logf("running with GODEBUG=fips140=only:\n%s"u8, @out);
        if (err != default!) {
            Ꮡt.Errorf("fips140=only subprocess failed: %v"u8, err);
        }
        return;
    }

    Ꮡt.Run(disabledˢ, (ж<testing.T> tΔ1) => {
        expectAllowed(tΔ1, beforeEnforcementˢ, false);
        fips140.WithoutEnforcement(() => {
            expectAllowed(tΔ1, insideWithoutEnforcementˢ, true);
        });
        // make sure that bypass doesn't live on after returning
        expectAllowed(tΔ1, afterWithoutEnforcementˢ, false);
    });

    Ꮡt.Run(nestedˢ, (ж<testing.T> tΔ2) => {
        expectAllowed(tΔ2, beforeEnforcementBypassˢ, false);
        fips140.WithoutEnforcement(() => {
            fips140.WithoutEnforcement(() => {
                expectAllowed(tΔ2, insideNestedˢ, true);
            });
            expectAllowed(tΔ2, insideNestedˢ, true);
        });
        expectAllowed(tΔ2, afterEnforcementBypassˢ, false);
    });

    Ꮡt.Run(goroutineInheritˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ3) => {
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
