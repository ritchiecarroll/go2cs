// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using testenv = @internal.testenv_package;
using runtime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using exec = global::go.os.exec_package;
using global::go.os;
using static global::go.go.build_package;

partial class build_internal_test_package {

// Prefixes for packages that can be vendored into the go repo.
// The prefixes are component-wise; for example, "golang.org/x"
// matches "golang.org/x/build" but not "golang.org/xyz".
//
// DO NOT ADD TO THIS LIST TO FIX BUILDS.
// Vendoring a new package requires prior discussion.
internal static slice<@string> allowedPackagePrefixes = new @string[]{
    "golang.org/x"u8,
    "github.com/google/pprof"u8,
    "github.com/ianlancetaylor/demangle"u8,
    "rsc.io/markdown"u8
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stdˢ = "std"u8;
internal static readonly @string cmdˢ = "cmd"u8;
internal static readonly @string vendorˢ3 = "/vendor/"u8;

// Verify that the vendor directories contain only packages matching the list above.
public static partial void TestVendorPackages(ж<testing.T> Ꮡt) {
    var (_, thisFile, _, _) = runtime.Caller(0);
    @string goBin = testenv.GoToolPath(new build_internal_test_package.testing_TжTB(Ꮡt));
    var listCmd = testenv.Command(new build_internal_test_package.testing_TжTB(Ꮡt), goBin, listˢ, stdˢ, cmdˢ);
    var (@out, err) = listCmd.Output();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (_, fullPkg) in strings.Split(((@string)@out), "\n"u8)) {
        var (pkg, found) = strings.CutPrefix(fullPkg, vendorˢ2);
        if (!found) {
            (_, pkg, found) = strings.Cut(fullPkg, vendorˢ3);
            if (!found) {
                continue;
            }
        }
        if (!isAllowed(pkg)) {
            Ꮡt.Errorf("""

		Package %q should not be vendored into this repo.
		After getting approval from the Go team, add it to allowedPackagePrefixes
		in %s.
"""u8,
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

[GoType("dyn")] internal partial struct TestIsAllowed_type {
    internal @string @in;
    internal bool want;
}

public static void TestIsAllowed(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in new TestIsAllowed_type[]{
        new("evil.com/bad"u8, false),
        new("golang.org/x/build"u8, true),
        new("rsc.io/markdown"u8, true),
        new("rsc.io/markdowntonabbey"u8, false),
        new("rsc.io/markdown/sub"u8, true)
    }.slice()) {
        var got = isAllowed(test.@in);
        if (got != test.want) {
            Ꮡt.Errorf("%q: got %t, want %t"u8, test.@in, got, test.want);
        }
    }
}

} // end build_internal_test_package
