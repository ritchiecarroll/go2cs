// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using bytes = bytes_package;
using testenv = @internal.testenv_package;
using os = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using testing = testing_package;
using @internal;
using fs = go.io.fs_package;
using go.os;
using path;
using static go.math.big_package;

partial class big_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;
internal static ref readonly @string xGoˢ => ref xGoˢᵇ;
private static readonly @string xGoˢᵇ = "x.go"u8;
internal static ref readonly @string xExeˢ => ref xExeˢᵇ;
private static readonly @string xExeˢᵇ = "x.exe"u8;
internal static ref readonly @string toolˢ => ref toolˢᵇ;
private static readonly @string toolˢᵇ = "tool"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = "runtime.main"u8;

// Tests that the linker is able to remove references to Float, Rat,
// and Int if unused (notably, not used by init).
public static void TestLinkerGC(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingInShortModeˢ);
    }
    t.Parallel();
    @string tmp = t.TempDir();
    @string goBin = testenv.GoToolPath(new big_test_package.testing_TжTB(t));
    @string goFile = filepath.Join(tmp, xGoˢ);
    var @file = slice<byte>("""
package main
import _ "math/big"
func main() {}

"""u8.Raw());
    var errΔ1 = os.WriteFile(goFile, @file, 420); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    var cmd = exec.Command(goBin, buildˢ, "-o", xExeˢ, xGoˢ);
    cmd.Dir = tmp;
    var (@out, errΔ2) = cmd.CombinedOutput(); if (errΔ2 != nil) {
        t.Fatalf("compile: %v, %s", errΔ2, @out);
    }

    cmd = exec.Command(goBin, toolˢ, "nm", xExeˢ);
    cmd.Dir = tmp;
    var (nm, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatalf("nm: %v, %s", err, nm);
    }
    @string want = wantᶜ;
    if (!bytes_package.Contains(nm, slice<byte>(want))) {
        // Test the test.
        t.Errorf("expected symbol %q not found", want);
    }
    var bad = slice<@string>([
        "math/big.(*Float)",
        "math/big.(*Rat)",
        "math/big.(*Int)"
    ]);
    foreach (var (_, sym) in range(bad)) {
        if (bytes_package.Contains(nm, slice<byte>(sym))) {
            t.Errorf("unexpected symbol %q found", sym);
        }
    }
    if (t.Failed()) {
        t.Logf("Got: %s", nm);
    }
}

} // end big_internal_test_package
