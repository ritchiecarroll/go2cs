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
internal static readonly @string xGoˢ = "x.go"u8;
internal static readonly @string buildˢ = "build"u8;
internal static readonly @string xExeˢ = "x.exe"u8;
internal static readonly @string toolˢ = "tool"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = "runtime.main"u8;

// Tests that the linker is able to remove references to Float, Rat,
// and Int if unused (notably, not used by init).
public static void TestLinkerGC(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    Ꮡt.Parallel();
    @string tmp = Ꮡt.TempDir();
    @string goBin = testenv.GoToolPath(new big_test_package.testing_TжTB(Ꮡt));
    @string goFile = filepath.Join(tmp, xGoˢ);
    var @file = slice<byte>("""
package main
import _ "math/big"
func main() {}

"""u8);
    {
        var errΔ1 = os.WriteFile(goFile, @file, 420); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    var cmd = exec.Command(goBin, buildˢ, "-o", xExeˢ, xGoˢ);
    cmd.Value.Dir = tmp;
    {
        var (@out, errΔ2) = cmd.CombinedOutput(); if (errΔ2 != default!) {
            Ꮡt.Fatalf("compile: %v, %s"u8, errΔ2, @out);
        }
    }
    cmd = exec.Command(goBin, toolˢ, "nm", xExeˢ);
    cmd.Value.Dir = tmp;
    var (nm, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("nm: %v, %s"u8, err, nm);
    }
    @string want = wantᶜ;
    if (!bytes_package.Contains(nm, slice<byte>(want))) {
        // Test the test.
        Ꮡt.Errorf("expected symbol %q not found"u8, want);
    }
    var bad = new @string[]{
        "math/big.(*Float)"u8,
        "math/big.(*Rat)"u8,
        "math/big.(*Int)"u8
    }.slice();
    foreach (var (_, sym) in bad) {
        if (bytes_package.Contains(nm, slice<byte>(sym))) {
            Ꮡt.Errorf("unexpected symbol %q found"u8, sym);
        }
    }
    if (Ꮡt.Failed()) {
        Ꮡt.Logf("Got: %s"u8, nm);
    }
}

} // end big_internal_test_package
