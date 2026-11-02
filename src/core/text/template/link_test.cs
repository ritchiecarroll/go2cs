// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text;

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
using static go.text.template_internal_test_package;

partial class template_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;
internal static ref readonly @string xGoˢ => ref xGoˢᵇ;
private static readonly @string xGoˢᵇ = "x.go"u8;
internal static ref readonly @string buildˢ => ref buildˢᵇ;
private static readonly @string buildˢᵇ = "build"u8;
internal static ref readonly @string xExeˢ => ref xExeˢᵇ;
private static readonly @string xExeˢᵇ = "x.exe"u8;
internal static readonly object binaryContainsCodeThatˢ = (@string)"binary contains code that should be deadcode eliminated"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string progᶜ => ref progᶜᵇ;
private static readonly @string progᶜᵇ = """
package main

import (
	_ "text/template"
)

type T struct{}

func (t *T) Unused() { println("THIS SHOULD BE ELIMINATED") }
func (t *T) Used() {}

var sink *T

func main() {
	var t T
	sink = &t
	t.Used()
}

"""u8.Raw();

// Issue 36021: verify that text/template doesn't prevent the linker from removing
// unused methods.
public static void TestLinkerGC(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingInShortModeˢ);
    }
    testenv.MustHaveGoBuild(new template_test_package.testing_TжTB(t));
    @string prog = progᶜ;
    @string td = t.TempDir();

    {
        var errΔ1 = os.WriteFile(filepath.Join(td, xGoˢ), slice<byte>(prog), 420); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }
    var cmd = exec.Command(testenv.GoToolPath(new template_test_package.testing_TжTB(t)), buildˢ, "-o", xExeˢ, xGoˢ);
    cmd.Dir = td;
    {
        var (@out, errΔ2) = cmd.CombinedOutput(); if (errΔ2 != nil) {
            t.Fatalf("go build: %v, %s", errΔ2, @out);
        }
    }
    var (slurp, err) = os.ReadFile(filepath.Join(td, xExeˢ));
    if (err != nil) {
        t.Fatal(err);
    }
    if (bytes.Contains(slurp, slice<byte>("THIS SHOULD BE ELIMINATED"u8))) {
        t.Error(binaryContainsCodeThatˢ);
    }
}

} // end template_test_package
