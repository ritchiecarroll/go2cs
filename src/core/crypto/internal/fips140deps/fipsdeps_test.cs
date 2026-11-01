// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140deps;

using testenv = go.@internal.testenv_package;
using strings = strings_package;
using testing = testing_package;
using exec = os.exec_package;
using go.@internal;
using os;
using static go.crypto.@internal.fips140deps.fipsdeps_package;

partial class fipsdeps_internal_test_package {

// AllowedInternalPackages are internal packages that can be imported from the
// FIPS module. The API of these packages ends up locked for the lifetime of the
// validated module, which can be years.
//
// DO NOT add new packages here just to make the tests pass.
public static readonly ж<map<@string, bool>> ᏑAllowedInternalPackages = new StaticBox<map<@string, bool>>(new map<@string, bool>{
    // entropy.Depleted/Seed is the entropy source, and sysrand.Read
    // is the actual (but uncredited!) random bytes source.
    ["crypto/internal/entropy"] = true,
    ["crypto/internal/entropy/v1.0.0"] = true,
    ["crypto/internal/sysrand"] = true,

    // impl.Register is how the packages expose their alternative
    // implementations to tests outside the module.
    ["crypto/internal/impl"] = true,

    // randutil.MaybeReadByte is used in non-FIPS mode by GenerateKey functions.
    ["crypto/internal/randutil"] = true,

    // constanttime are the constant-time intrinsics.
    ["crypto/internal/constanttime"] = true
});
public static ref map<@string, bool> AllowedInternalPackages => ref ((StaticBox<map<@string, bool>>)ᏑAllowedInternalPackages).ValueSlot;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string listˢ => ref listˢᵇ;
private static readonly @string listˢᵇ = "list"u8;
internal static ref readonly @string pathImportPathRangeˢ => ref pathImportPathRangeˢᵇ;
private static readonly @string pathImportPathRangeˢᵇ = """
{{$path := .ImportPath -}}
{{range .Imports -}}
{{$path}} {{.}}
{{end -}}
{{range .TestImports -}}
{{$path}} {{.}}
{{end -}}
{{range .XTestImports -}}
{{$path}} {{.}}
{{end -}}
"""u8.Raw();
internal static ref readonly @string cryptoInternalFips140ˢ => ref cryptoInternalFips140ˢᵇ;
private static readonly @string cryptoInternalFips140ˢᵇ = "crypto/internal/fips140/..."u8;
internal static ref readonly @string cryptoInternalFips140ˢ2 => ref cryptoInternalFips140ˢ2ᵇ;
private static readonly @string cryptoInternalFips140ˢ2ᵇ = "crypto/internal/fips140/"u8;
internal static ref readonly @string cryptoInternalFips140ˢ3 => ref cryptoInternalFips140ˢ3ᵇ;
private static readonly @string cryptoInternalFips140ˢ3ᵇ = "crypto/internal/fips140"u8;
internal static ref readonly @string cryptoInternalˢ => ref cryptoInternalˢᵇ;
private static readonly @string cryptoInternalˢᵇ = "crypto/internal/fips140deps/"u8;
internal static ref readonly @string internalˢ => ref internalˢᵇ;
private static readonly @string internalˢᵇ = "internal"u8;

public static void TestImports(ж<testing.T> t) {
    var cmd = testenv.Command(new fipsdeps_internal_test_package.testing_TжTB(t), testenv.GoToolPath(new fipsdeps_internal_test_package.testing_TжTB(t)), listˢ, "-f", pathImportPathRangeˢ, cryptoInternalFips140ˢ);
    var (bout, err) = testenv.CleanCmdEnv(cmd).CombinedOutput();
    if (err != nil) {
        t.Fatalf("go list: %v\n%s", err, bout);
    }
    @string @out = (@string)bout;

    // In a snapshot, all the paths are crypto/internal/fips140/v1.2.3/...
    // Determine the version number and remove it for the test.
    var (_, v, _) = strings.Cut(@out, cryptoInternalFips140ˢ2);
    (v, _, _) = strings.Cut(v, "/");
    (v, _, _) = strings.Cut(v, " ");
    if (strings.HasPrefix(v, "v") && strings.Count(v, ".") == 2) {
        @out = strings.ReplaceAll(@out, "crypto/internal/fips140/"u8 + v, cryptoInternalFips140ˢ3);
    }

    var allPackages = new map<@string, bool>();

    // importCheck is the set of packages that import crypto/internal/fips140/check.
    var importCheck = new map<@string, bool>();

    foreach (var (_, line) in strings.Split(@out, "\n")) {
        if (line == ""u8) {
            continue;
        }
        var (pkg, importedPkg, _) = strings.Cut(line, " ");

        allPackages[pkg] = true;

        if (importedPkg == "crypto/internal/fips140/check"u8) {
            importCheck[pkg] = true;
        }

        // Ensure we don't import any unexpected internal package from the FIPS
        // module, since we can't change the module source after it starts
        // validation. This locks in the API of otherwise internal packages.
        if (importedPkg == "crypto/internal/fips140"u8 || strings.HasPrefix(importedPkg, cryptoInternalFips140ˢ2) || strings.HasPrefix(importedPkg, cryptoInternalˢ)) {
            continue;
        }
        if (AllowedInternalPackages[importedPkg]) {
            continue;
        }
        if (strings.Contains(importedPkg, internalˢ)) {
            t.Errorf("unexpected import of internal package: %s -> %s", pkg, importedPkg);
        }
    }

    // Ensure that all packages except check and check's dependencies import check.
    foreach (var (pkg, _) in allPackages) {
        var exprᴛ1 = pkg;
        switch (ᐧ) {
        case {} when exprᴛ1 == "crypto/internal/fips140/check"u8: {
            break;
        }
        case {} when exprᴛ1 == "crypto/internal/fips140"u8: {
            break;
        }
        case {} when exprᴛ1 == "crypto/internal/fips140/alias"u8: {
            break;
        }
        case {} when exprᴛ1 == "crypto/internal/fips140/subtle"u8: {
            break;
        }
        case {} when exprᴛ1 == "crypto/internal/fips140/hmac"u8: {
            break;
        }
        case {} when exprᴛ1 == "crypto/internal/fips140/sha3"u8: {
            break;
        }
        case {} when exprᴛ1 == "crypto/internal/fips140/sha256"u8: {
            break;
        }
        case {} when exprᴛ1 == "crypto/internal/fips140/sha512"u8: {
            break;
        }
        default: {
            if (!importCheck[pkg]) {
                t.Errorf("package %s does not import crypto/internal/fips140/check", pkg);
            }
            break;
        }}
    }
}

} // end fipsdeps_internal_test_package
