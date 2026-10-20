// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using build = global::go.go.build_package;
using testenv = @internal.testenv_package;
using Δlog = log_package;
using Δos = os_package;
using exec = global::go.os.exec_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using global::go.go;
using global::go.os;

partial class crypto_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string listˢ = "list"u8;
private static readonly @string cryptoˢ = "crypto/..."u8;
private static readonly @string mathBigˢ = "math/big"u8;
private static readonly @string toolˢ = "tool"u8;
private static readonly @string distˢ = "dist"u8;
private static readonly @string boringˢ = "/boring"u8;

// TestPureGoTag checks that when built with the purego build tag, crypto
// packages don't require any assembly. This is used by alternative compilers
// such as TinyGo. See also the "crypto/...:purego" test in cmd/dist, which
// ensures the packages build correctly.
public static void TestPureGoTag(ж<testing.T> Ꮡt) {
    var cmd = exec.Command(testenv.GoToolPath(new testing_TжTB(Ꮡt)), listˢ, "-e", cryptoˢ, mathBigˢ);
    cmd.Value.Env = append((~cmd).Env, "GOOS=linux"u8);
    cmd.Value.Stderr = new Δos.FileжWriter(Δos.Stderr);
    var (@out, err) = cmd.Output();
    if (err != default!) {
        Δlog.Fatalf("loading package list: %v\n%s"u8, err, @out);
    }
    var pkgs = strings.Split(strings.TrimSpace((@string)@out), "\n"u8);

    cmd = exec.Command(testenv.GoToolPath(new testing_TжTB(Ꮡt)), toolˢ, distˢ, listˢ);
    cmd.Value.Stderr = new Δos.FileжWriter(Δos.Stderr);
    (@out, err) = cmd.Output();
    if (err != default!) {
        Δlog.Fatalf("loading architecture list: %v\n%s"u8, err, @out);
    }
    var allGOARCH = new map<@string, bool>();
    foreach (var (_, pair) in strings.Split(strings.TrimSpace((@string)@out), "\n"u8)) {
        @string GOARCH = strings.Split(pair, "/"u8)[1];
        allGOARCH[GOARCH] = true;
    }

    foreach (var (_, pkgName) in pkgs) {
        if (strings.Contains(pkgName, boringˢ)) {
            continue;
        }

        foreach (var (GOARCH, _) in allGOARCH) {
            ref var context = ref heap<build.Context>(out var Ꮡcontext);
            context = new build.Context(
                GOOS: "linux"u8, // darwin has custom assembly
                GOARCH: GOARCH,
                GOROOT: testenv.GOROOT(new testing_TжTB(Ꮡt)),
                Compiler: build.Default.Compiler,
                BuildTags: new @string[]{"purego"u8, "math_big_pure_go"u8}.slice()
            );

            var (pkg, errΔ1) = Ꮡcontext.Import(pkgName, ""u8, 0);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            if (len((~pkg).SFiles) == 0) {
                continue;
            }
            Ꮡt.Errorf("package %s has purego assembly files on %s: %v"u8, pkgName, GOARCH, (~pkg).SFiles);
        }
    }
}

} // end crypto_test_package
