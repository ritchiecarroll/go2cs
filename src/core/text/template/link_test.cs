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
internal static readonly @string xGoˢ = "x.go"u8;
internal static readonly @string buildˢ = "build"u8;
internal static readonly @string xExeˢ = "x.exe"u8;
internal static readonly object binaryContainsCodeThatˢ = (@string)"binary contains code that should be deadcode eliminated"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string progᶜ = """
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

"""u8;

// Issue 36021: verify that text/template doesn't prevent the linker from removing
// unused methods.
public static void TestLinkerGC(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    testenv.MustHaveGoBuild(new template_test_package.testing_TжTB(Ꮡt));
    @string prog = progᶜ;
    @string td = Ꮡt.TempDir();
    {
        var errΔ1 = os.WriteFile(filepath.Join(td, xGoˢ), slice<byte>(prog), 420); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    var cmd = exec.Command(testenv.GoToolPath(new template_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", xExeˢ, xGoˢ);
    cmd.Value.Dir = td;
    {
        var (@out, errΔ2) = cmd.CombinedOutput(); if (errΔ2 != default!) {
            Ꮡt.Fatalf("go build: %v, %s"u8, errΔ2, @out);
        }
    }
    var (slurp, err) = os.ReadFile(filepath.Join(td, xExeˢ));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (bytes.Contains(slurp, slice<byte>("THIS SHOULD BE ELIMINATED"u8))) {
        Ꮡt.Error(binaryContainsCodeThatˢ);
    }
}

} // end template_test_package
