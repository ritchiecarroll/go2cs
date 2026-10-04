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

// entropy.Depleted is the external passive entropy source, and sysrand.Read
// is the actual (but uncredited!) random bytes source.
// impl.Register is how the packages expose their alternative
// implementations to tests outside the module.
// randutil.MaybeReadByte is used in non-FIPS mode by GenerateKey functions.
// AllowedInternalPackages are internal packages that can be imported from the
// FIPS module. The API of these packages ends up locked for the lifetime of the
// validated module, which can be years.
//
// DO NOT add new packages here just to make the tests pass.
public static map<@string, bool> AllowedInternalPackages = new map<@string, bool>{
    ["crypto/internal/entropy"u8] = true,
    ["crypto/internal/sysrand"u8] = true,
    ["crypto/internal/impl"u8] = true,
    ["crypto/internal/randutil"u8] = true
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string listˢ = "list"u8;
internal static readonly @string pathImportPathRangeˢ = """
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
"""u8;
internal static readonly @string cryptoInternalFips140ˢ = "crypto/internal/fips140/..."u8;
internal static readonly @string cryptoInternalFips140ˢ2 = "crypto/internal/fips140/"u8;
internal static readonly @string cryptoInternalFips140ˢ3 = "crypto/internal/fips140"u8;
internal static readonly @string cryptoInternalˢ = "crypto/internal/fips140deps/"u8;
internal static readonly @string internalˢ = "internal"u8;

public static void TestImports(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var cmd = testenv.Command(new fipsdeps_internal_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new fipsdeps_internal_test_package.testing_TжTB(Ꮡt)), listˢ, "-f", pathImportPathRangeˢ, cryptoInternalFips140ˢ);
    var (bout, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("go list: %v\n%s"u8, err, bout);
    }
    @string @out = ((@string)bout);
    // In a snapshot, all the paths are crypto/internal/fips140/v1.2.3/...
    // Determine the version number and remove it for the test.
    var (_, v, _) = strings.Cut(@out, cryptoInternalFips140ˢ2);
    (v, _, _) = strings.Cut(v, "/"u8);
    (v, _, _) = strings.Cut(v, " "u8);
    if (strings.HasPrefix(v, "v"u8) && strings.Count(v, "."u8) == 2) {
        @out = strings.ReplaceAll(@out, "crypto/internal/fips140/"u8 + v, cryptoInternalFips140ˢ3);
    }
    var allPackages = new map<@string, bool>();
    // importCheck is the set of packages that import crypto/internal/fips140/check.
    var importCheck = new map<@string, bool>();
    foreach (var (_, line) in strings.Split(@out, "\n"u8)) {
        if (line == ""u8) {
            continue;
        }
        var (pkg, importedPkg, _) = strings.Cut(line, " "u8);
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
            Ꮡt.Errorf("unexpected import of internal package: %s -> %s"u8, pkg, importedPkg);
        }
    }
    // Ensure that all packages except check and check's dependencies import check.
    foreach (var (pkg, _) in allPackages) {
        var exprᴛ1 = pkg;
        if (exprᴛ1 == "crypto/internal/fips140/check"u8) {
        }
        else if (exprᴛ1 == "crypto/internal/fips140"u8) {
        }
        else if (exprᴛ1 == "crypto/internal/fips140/alias"u8) {
        }
        else if (exprᴛ1 == "crypto/internal/fips140/subtle"u8) {
        }
        else if (exprᴛ1 == "crypto/internal/fips140/hmac"u8) {
        }
        else if (exprᴛ1 == "crypto/internal/fips140/sha3"u8) {
        }
        else if (exprᴛ1 == "crypto/internal/fips140/sha256"u8) {
        }
        else if (exprᴛ1 == "crypto/internal/fips140/sha512"u8) {
        }
        else { /* default: */
            if (!importCheck[pkg]) {
                Ꮡt.Errorf("package %s does not import crypto/internal/fips140/check"u8, pkg);
            }
        }

    }
}

} // end fipsdeps_internal_test_package
