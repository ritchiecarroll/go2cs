// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using testenv = global::go.@internal.testenv_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using exec = global::go.os.exec_package;
using global::go.@internal;
using global::go.os;
using static global::go.go.build_package;

partial class build_internal_test_package {

// Prefixes for packages that can be vendored into the go repo.
// The prefixes are component-wise; for example, "golang.org/x"
// matches "golang.org/x/build" but not "golang.org/xyz".
//
// DO NOT ADD TO THIS LIST TO FIX BUILDS.
// Vendoring a new package requires prior discussion.
internal static slice<@string> allowedPackagePrefixes = slice<@string>([
    "golang.org/x",
    "github.com/google/pprof",
    "github.com/ianlancetaylor/demangle",
    "rsc.io/markdown"
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stdˢ => ref stdˢᵇ;
private static readonly @string stdˢᵇ = "std"u8;
internal static ref readonly @string cmdˢ => ref cmdˢᵇ;
private static readonly @string cmdˢᵇ = "cmd"u8;
internal static ref readonly @string vendorˢ3 => ref vendorˢ3ᵇ;
private static readonly @string vendorˢ3ᵇ = "/vendor/"u8;

// Verify that the vendor directories contain only packages matching the list above.
public static partial void TestVendorPackages(ж<testing.T> t) {
    var (_, thisFile, _, _) = runtime.Caller(0);
    @string goBin = testenv.GoToolPath(new build_internal_test_package.testing_TжTB(t));
    var listCmd = testenv.Command(new build_internal_test_package.testing_TжTB(t), goBin, listˢ, stdˢ, cmdˢ);
    var (@out, err) = listCmd.Output();
    if (err != nil) {
        t.Fatal(err);
    }
    foreach (var (_, fullPkg) in strings.Split((@string)@out, "\n")) {
        var (pkg, found) = strings.CutPrefix(fullPkg, vendorˢ2);
        if (!found) {
            (_, pkg, found) = strings.Cut(fullPkg, vendorˢ3);
            if (!found) {
                continue;
            }
        }
        if (!isAllowed(pkg)) {
            t.Errorf("""

		Package %q should not be vendored into this repo.
		After getting approval from the Go team, add it to allowedPackagePrefixes
		in %s.
"""u8.Raw(),
                pkg, thisFile);
        }
    }
}

internal static bool isAllowed(@string pkg) {
    foreach (var (_, pre) in allowedPackagePrefixes) {
        if (pkg == pre || strings.HasPrefix(pkg, pre + "/"u8)) {
            return true;
        }
    }
    return false;
}

internal partial struct TestIsAllowed_type /*dyn*/ {
    internal @string @in;
    internal bool want;
}

public static void TestIsAllowed(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestIsAllowed_type>([
        new("evil.com/bad", false),
        new("golang.org/x/build", true),
        new("rsc.io/markdown", true),
        new("rsc.io/markdowntonabbey", false),
        new("rsc.io/markdown/sub", true)
    ])) {
        var got = isAllowed(test.@in);
        if (got != test.want) {
            t.Errorf("%q: got %t, want %t", test.@in, got, test.want);
        }
    }
}

} // end build_internal_test_package
