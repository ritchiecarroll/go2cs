// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using Δboring = go.crypto.@internal.boring_package;
using impl = go.crypto.@internal.impl_package;
using goos = go.@internal.goos_package;
using testenv = go.@internal.testenv_package;
using testing = testing_package;
using go.@internal;
using go.crypto.@internal;

partial class cryptotest_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object knownIssueSeeGolangOrgˢ = (@string)"known issue, see golang.org/issue/69592"u8;
private static readonly object knownIssueSeeGolangOrgˢ2 = (@string)"known issue, see golang.org/issue/69593"u8;
private static readonly object builderDoesnTSupportCpuˢ = (@string)"builder doesn't support CPU features needed to test this implementation"u8;
private static readonly object implementationNotˢ = (@string)"implementation not supported"u8;
private static readonly @string baseˢ = "Base"u8;

// TestAllImplementations runs the provided test function with each available
// implementation of the package registered with crypto/internal/impl. If there
// are no alternative implementations for pkg, f is invoked directly once.
public static void TestAllImplementations(ж<testing.T> Ꮡt, @string pkg, Action<ж<testing.T>> f) {
    // BoringCrypto bypasses the multiple Go implementations.
    if (Δboring.Enabled) {
        f(Ꮡt);
        return;
    }

    var impls = impl.List(pkg);
    if (len(impls) == 0) {
        f(Ꮡt);
        return;
    }

    Ꮡt.Cleanup(() => {
        impl.Reset(pkg);
    });

    foreach (var (_, name) in impls) {
        {
            var available = impl.Select(pkg, name); if (available){
                Ꮡt.Run(name, f);
            } else {
                Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
                    // Report an error if we're on Linux CI (assumed to be the most
                    // consistent) and the builder can't test this implementation.
                    if (testenv.Builder() != ""u8 && goos.GOOS == "linux"){
                        if (name == "SHA-NI"u8) {
                            tΔ1.Skip(knownIssueSeeGolangOrgˢ);
                        }
                        if (name == "Armv8.2"u8) {
                            tΔ1.Skip(knownIssueSeeGolangOrgˢ2);
                        }
                        tΔ1.Error(builderDoesnTSupportCpuˢ);
                    } else {
                        tΔ1.Skip(implementationNotˢ);
                    }
                });
            }
        }
    }

    // Test the generic implementation.
    impl.Select(pkg, ""u8);
    Ꮡt.Run(baseˢ, f);
}

} // end cryptotest_package
