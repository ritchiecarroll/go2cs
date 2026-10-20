// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using syscall = syscall_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gorootˢ = "GOROOT"u8;

public static void TestFixedGOROOT(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Restore both the real GOROOT environment variable, and runtime's copies:
        {
            var (orig, ok) = syscall.Getenv(gorootˢ); if (ok){
                defer(syscall.Setenv, gorootˢ, orig, ref ᒐ);
            } else {
                defer(syscall.Unsetenv, gorootˢ, ref ᒐ);
            }
        }
        var envs = runtime_internal_test_package.Envs();
        var oldenvs = appendꓸꓸꓸ(new @string[]{}.slice(), envs);
        defer(runtime_internal_test_package.SetEnvs, oldenvs, ref ᒐ);

        // attempt to reuse existing envs backing array.
        @string want = Δruntime.GOROOT();
        runtime_internal_test_package.SetEnvs(append(envs[..0], "GOROOT="u8 + want));

        {
            @string got = Δruntime.GOROOT(); if (got != want) {
                Ꮡt.Errorf(@"initial runtime.GOROOT()=%q, want %q"u8, got, want);
            }
        }
        {
            var err = syscall.Setenv(gorootˢ, "/os"u8); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        {
            @string got = Δruntime.GOROOT(); if (got != want) {
                Ꮡt.Errorf(@"after setenv runtime.GOROOT()=%q, want %q"u8, got, want);
            }
        }
        {
            var err = syscall.Unsetenv(gorootˢ); if (err != default!) {
                Ꮡt.Fatal(err);
            }
        }
        {
            @string got = Δruntime.GOROOT(); if (got != want) {
                Ꮡt.Errorf(@"after unsetenv runtime.GOROOT()=%q, want %q"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end runtime_test_package
