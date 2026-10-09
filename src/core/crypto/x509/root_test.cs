// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using testing = testing_package;
using static go.crypto.x509_package;

partial class x509_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object multipleCallsToˢ = (@string)"Multiple calls to SetFallbackRoots should panic"u8;

public static void TestFallbackPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            if (recover() == default!) {
                Ꮡt.Fatal(multipleCallsToˢ);
            }
        }, ref ᒐ);
        SetFallbackRoots(nil);
        SetFallbackRoots(nil);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string godebugˢ = "GODEBUG"u8;
internal static readonly @string x509usefallbackroots1ˢ = "x509usefallbackroots=1"u8;
internal static readonly @string x509usefallbackroots0ˢ = "x509usefallbackroots=0"u8;
internal static readonly object systemRootsWasNotSetToˢ = (@string)"systemRoots was not set to fallback pool"u8;
internal static readonly object systemRootsWasSetToˢ = (@string)"systemRoots was set to fallback pool when it shouldn't have been"u8;

internal partial struct TestFallback_tests /*dyn*/ {
    internal @string name;
    internal ж<global::go.crypto.x509_package.CertPool> systemRoots;
    internal bool systemPool;
    internal slice<ж<global::go.crypto.x509_package.Certificate>> poolContent;
    internal bool forceFallback;
    internal bool returnsFallback;
}

public static void TestFallback(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // call systemRootsPool so that the sync.Once is triggered, and we can
        // manipulate systemRoots without worrying about our working being overwritten
        systemRootsPool();
        if (systemRoots != nil) {
            ref var originalSystemRoots = ref heap<global::go.crypto.x509_package.CertPool>(out var ᏑoriginalSystemRoots);
            originalSystemRoots = systemRoots.Value;
            defer(() => {
                systemRoots = ᏑoriginalSystemRoots;
            }, ref ᒐ);
        }
        var tests = new TestFallback_tests[]{
            new(
                name: "nil systemRoots"u8,
                returnsFallback: true
            ),
            new(
                name: "empty systemRoots"u8,
                systemRoots: NewCertPool(),
                returnsFallback: true
            ),
            new(
                name: "empty systemRoots system pool"u8,
                systemRoots: NewCertPool(),
                systemPool: true
            ),
            new(
                name: "filled systemRoots system pool"u8,
                systemRoots: NewCertPool(),
                poolContent: new ж<global::go.crypto.x509_package.Certificate>[]{Ꮡ(new global::go.crypto.x509_package.Certificate())}.slice(),
                systemPool: true
            ),
            new(
                name: "filled systemRoots"u8,
                systemRoots: NewCertPool(),
                poolContent: new ж<global::go.crypto.x509_package.Certificate>[]{Ꮡ(new global::go.crypto.x509_package.Certificate())}.slice()
            ),
            new(
                name: "filled systemRoots, force fallback"u8,
                systemRoots: NewCertPool(),
                poolContent: new ж<global::go.crypto.x509_package.Certificate>[]{Ꮡ(new global::go.crypto.x509_package.Certificate())}.slice(),
                forceFallback: true,
                returnsFallback: true
            ),
            new(
                name: "filled systemRoot system pool, force fallback"u8,
                systemRoots: NewCertPool(),
                poolContent: new ж<global::go.crypto.x509_package.Certificate>[]{Ꮡ(new global::go.crypto.x509_package.Certificate())}.slice(),
                systemPool: true,
                forceFallback: true,
                returnsFallback: true
            )
        }.slice();
        foreach (var (_, vᴛ1) in tests) {
            ref var tc = ref heap(new TestFallback_tests(), out var Ꮡtc);
            tc = vᴛ1;

            var tcʗ1 = tc;
            Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
                fallbacksSet = false;
                systemRoots = tcʗ1.systemRoots;
                if (systemRoots != nil) {
                    systemRoots.Value.systemPool = tcʗ1.systemPool;
                }
                foreach (var (_, c) in tcʗ1.poolContent) {
                    systemRoots.AddCert(c);
                }
                if (tcʗ1.forceFallback){
                    tΔ1.Setenv(godebugˢ, x509usefallbackroots1ˢ);
                } else {
                    tΔ1.Setenv(godebugˢ, x509usefallbackroots0ˢ);
                }
                var fallbackPool = NewCertPool();
                SetFallbackRoots(fallbackPool);
                var systemPoolIsFallback = systemRoots == fallbackPool;
                if (tcʗ1.returnsFallback && !systemPoolIsFallback){
                    tΔ1.Error(systemRootsWasNotSetToˢ);
                } else 
                if (!tcʗ1.returnsFallback && systemPoolIsFallback) {
                    tΔ1.Error(systemRootsWasSetToˢ);
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end x509_internal_test_package
