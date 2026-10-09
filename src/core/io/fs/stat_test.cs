// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using fmt = fmt_package;
using static go.io.fs_package;
using testing = testing_package;
using fs = go.io.fs_package;
using go.io;

partial class fs_test_package {

partial struct statOnly {
    /*embed*/ public go.io.fs_package.StatFS StatFS;
}

internal static (fs.File, error) Open(this statOnly _, @string name) {
    return (default!, ErrNotExist);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nilˢ = "<nil>"u8;
private static readonly @string statOnlyˢ = "statOnly"u8;

public static void TestStat(ж<testing.T> Ꮡt) {
    void check(@string desc, fs.FileInfo infoΔ1, error errΔ1) {
        Ꮡt.Helper();
        if (errΔ1 != default! || infoΔ1 == default! || infoΔ1.Mode() != 302) {
            @string infoStr = nilˢ;
            if (infoΔ1 != default!) {
                infoStr = fmt.Sprintf("FileInfo(Mode: %#o)"u8, infoΔ1.Mode());
            }
            Ꮡt.Fatalf("Stat(%s) = %v, %v, want Mode:0456, nil"u8, desc, infoStr, errΔ1);
        }
    }
    // Test that Stat uses the method when present.
    var (info, err) = Stat(new statOnly(new fstest_MapFSᴠStatFS(testFsys)), helloTxtˢ);
    check(statOnlyˢ, info, err);
    // Test that Stat uses Open when the method is not present.
    (info, err) = Stat(new openOnly(testFsys), helloTxtˢ);
    check(openOnlyˢ, info, err);
}

} // end fs_test_package
