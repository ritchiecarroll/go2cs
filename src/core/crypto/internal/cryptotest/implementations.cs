// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal;

using Δboring = go.crypto.@internal.boring_package;
using impl = go.crypto.@internal.impl_package;
using goarch = go.@internal.goarch_package;
using goos = go.@internal.goos_package;
using testenv = go.@internal.testenv_package;
using strings = strings_package;
using testing = testing_package;
using go.@internal;
using go.crypto.@internal;

partial class cryptotest_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object builderDoesnTSupportCpuˢ = (@string)"builder doesn't support CPU features needed to test this implementation"u8;
private static readonly object implementationNotˢ = (@string)"implementation not supported"u8;
private static readonly @string baseˢ = "Base"u8;

// TestAllImplementations runs the provided test function with each available
// implementation of the package registered with crypto/internal/impl. If there
// are no alternative implementations for pkg, f is invoked directly once.
public static void TestAllImplementations(ж<testing.T> t, @string pkg, Action<ж<testing.T>> f) {
    // BoringCrypto bypasses the multiple Go implementations.
    if (Δboring.Enabled) {
        f(t);
        return;
    }

    var impls = impl.List(pkg);
    if (len(impls) == 0) {
        f(t);
        return;
    }

    t.Cleanup(() => {
        impl.Reset(pkg);
    });

    foreach (var (_, name) in impls) {
        {
            var available = impl.Select(pkg, name); if (available) {
                t.Run(name, f);
            } else {
                t.Run(name, (ж<testing.T> tΔ1) => {
                    // Report an error if we're on the most capable builder for the
                    // architecture and the builder can't test this implementation.
                    if (flagshipBuilder()) {
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
    t.Run(baseˢ, f);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string avx512ˢ = "_avx512"u8;

internal static bool flagshipBuilder() {
    @string builder = testenv.Builder();
    if (builder == ""u8) {
        return false;
    }
    var exprᴛ1 = goarch.GOARCH;
    switch (ᐧ) {
    case {} when exprᴛ1 == "amd64"u8: {
        return strings.Contains(builder, avx512ˢ);
    }
    case {} when exprᴛ1 == "arm64"u8: {
        // Apple M chips support everything we use.
        return goos.GOOS == "darwin";
    }
    default: {
        // Presumably the Linux builders are the most capable.
        return goos.GOOS == "linux";
    }}
}

} // end cryptotest_package
