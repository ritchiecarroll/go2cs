// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using build = global::go.go.build_package;
using testenv = @internal.testenv_package;
using Δlog = log_package;
using Δos = os_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using exec = global::go.os.exec_package;
using global::go.go;
using global::go.os;

partial class crypto_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string listˢ => ref listˢᵇ;
private static readonly @string listˢᵇ = "list"u8;
private static ref readonly @string cryptoˢ => ref cryptoˢᵇ;
private static readonly @string cryptoˢᵇ = "crypto/..."u8;
private static ref readonly @string mathBigˢ => ref mathBigˢᵇ;
private static readonly @string mathBigˢᵇ = "math/big"u8;
private static ref readonly @string toolˢ => ref toolˢᵇ;
private static readonly @string toolˢᵇ = "tool"u8;
private static ref readonly @string distˢ => ref distˢᵇ;
private static readonly @string distˢᵇ = "dist"u8;
private static ref readonly @string boringˢ2 => ref boringˢ2ᵇ;
private static readonly @string boringˢ2ᵇ = "/boring"u8;

// TestPureGoTag checks that when built with the purego build tag, crypto
// packages don't require any assembly. This is used by alternative compilers
// such as TinyGo. See also the "crypto/...:purego" test in cmd/dist, which
// ensures the packages build correctly.
public static void TestPureGoTag(ж<testing.T> t) {
    var cmd = testenv.Command(new testing_TжTB(t), testenv.GoToolPath(new testing_TжTB(t)), listˢ, "-e", cryptoˢ, mathBigˢ);
    cmd = testenv.CleanCmdEnv(cmd);
    cmd.Env = append(cmd.Environ(), (@string)"GOOS=linux", (@string)"GOFIPS140=off");
    cmd.Stderr = new Δos.FileжWriter(Δos.Stderr);
    var (@out, err) = cmd.Output();
    if (err != nil) {
        Δlog.Fatalf("loading package list: %v\n%s", err, @out);
    }
    var pkgs = strings.Split(strings.TrimSpace((@string)@out), "\n");

    cmd = testenv.Command(new testing_TжTB(t), testenv.GoToolPath(new testing_TжTB(t)), toolˢ, distˢ, listˢ);
    cmd.Stderr = new Δos.FileжWriter(Δos.Stderr);
    (@out, err) = testenv.CleanCmdEnv(cmd).Output();
    if (err != nil) {
        Δlog.Fatalf("loading architecture list: %v\n%s", err, @out);
    }
    var allGOARCH = new map<@string, bool>();
    foreach (var (_, pair) in range(strings.Split(strings.TrimSpace((@string)@out), "\n"))) {
        @string GOARCH = strings.Split(pair, "/")[1];
        allGOARCH[GOARCH] = true;
    }

    foreach (var (_, pkgName) in range(pkgs)) {
        if (strings.Contains(pkgName, boringˢ2)) {
            continue;
        }

        foreach (var (GOARCH, _) in allGOARCH) {
            ref var context = ref heap<build.Context>(new build.Context(
                GOOS: "linux", // darwin has custom assembly
                GOARCH: GOARCH,
                GOROOT: testenv.GOROOT(new testing_TжTB(t)),
                Compiler: build.Default.Compiler,
                BuildTags: ["purego", "math_big_pure_go"]
            ), out var Ꮡcontext);

            var (pkg, errΔ1) = Ꮡcontext.Import(pkgName, ""u8, 0);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
            if (len(pkg.SFiles) == 0) {
                continue;
            }
            t.Errorf("package %s has purego assembly files on %s: %v", pkgName, GOARCH, pkg.SFiles);
        }
    }
}

} // end crypto_test_package
