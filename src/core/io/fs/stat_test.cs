// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using fmt = fmt_package;
using static go.io.fs_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using fs = go.io.fs_package;
using go.io;

partial class fs_test_package {

partial struct statOnly {
    /*embed*/ public go.io.fs_package.StatFS StatFS;
}

internal static (fs.File, error) Open(this statOnly _, @string name) {
    return (default, ErrNotExist);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string nilˢ => ref nilˢᵇ;
private static readonly @string nilˢᵇ = "<nil>"u8;
private static ref readonly @string statOnlyˢ => ref statOnlyˢᵇ;
private static readonly @string statOnlyˢᵇ = "statOnly"u8;

public static partial void TestStat(ж<testing.T> t) {
    void check(@string desc, fs.FileInfo infoΔ1, error errΔ1) {
        t.Helper();
        if (errΔ1 != nil || infoΔ1 == nil || infoΔ1.Mode() != 302) {
            @string infoStr = nilˢ;
            if (infoΔ1 != nil) {
                infoStr = fmt.Sprintf("FileInfo(Mode: %#o)"u8, infoΔ1.Mode());
            }
            t.Fatalf("Stat(%s) = %v, %v, want Mode:0456, nil", desc, infoStr, errΔ1);
        }
    }

    // Test that Stat uses the method when present.
    var (info, err) = Stat(new statOnly(new fstest_MapFSᴠStatFS(testFsys)), helloTxtˢ);
    check(statOnlyˢ, info, err);

    // Test that Stat uses Open when the method is not present.
    (info, err) = Stat(new openOnly(new fstest_MapFSᴠFS(testFsys)), helloTxtˢ);
    check(openOnlyˢ, info, err);
}

} // end fs_test_package
