// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using errors = errors_package;
using static go.io.fs_package;
using testing = testing_package;
using fs = go.io.fs_package;
using go.io;

partial class fs_test_package {

partial struct subOnly {
    /*embed*/ public go.io.fs_package.SubFS SubFS;
}

internal static (fs.File, error) Open(this subOnly _, @string name) { return (default, ErrNotExist); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string goodbyeTxtˢ => ref goodbyeTxtˢᵇ;
private static readonly @string goodbyeTxtˢᵇ = "goodbye.txt"u8;
private static readonly object goodbyeWorldˢ = (@string)"goodbye, world"u8;
private static ref readonly @string subˢ2 => ref subˢ2ᵇ;
private static readonly @string subˢ2ᵇ = "sub"u8;
private static ref readonly @string subOnlyˢ => ref subOnlyˢᵇ;
private static readonly @string subOnlyˢᵇ = "subOnly"u8;
private static ref readonly @string nonexistˢ => ref nonexistˢᵇ;
private static readonly @string nonexistˢᵇ = "nonexist"u8;
private static readonly object openNonexistSucceededˢ = (@string)"Open(nonexist): succeeded"u8;

public static void TestSub(ж<testing.T> t) {
    void check(@string desc, fs.FS subΔ1, error errΔ1) {
        t.Helper();
        if (errΔ1 != nil) {
            t.Errorf("Sub(sub): %v", errΔ1);
            return;
        }
        (var data, errΔ1) = ReadFile(subΔ1, goodbyeTxtˢ);
        if (((sstring)data) != "goodbye, world"u8 || errΔ1 != nil) {
            t.Errorf(@"ReadFile(%s, ""goodbye.txt"" = %q, %v, want %q, nil", desc, (@string)data, errΔ1, goodbyeWorldˢ);
        }

        (var dirs, errΔ1) = ReadDir(subΔ1, ".");
        if (errΔ1 != nil || len(dirs) != 1 || dirs[0].Name() != "goodbye.txt"u8) {
            slice<@string> names = default;
            foreach (var (_, d) in range(dirs)) {
                names = append(names, d.Name());
            }
            t.Errorf(@"ReadDir(%s, ""."") = %v, %v, want %v, nil", desc, names, errΔ1, slice<@string>(["goodbye.txt"]));
        }
    }

    // Test that Sub uses the method when present.
    var (sub, err) = Sub(new subOnly(new fstest_MapFSᴠSubFS(testFsys)), subˢ2);
    check(subOnlyˢ, sub, err);

    // Test that Sub uses Open when the method is not present.
    (sub, err) = Sub(new openOnly(new fstest_MapFSᴠFS(testFsys)), subˢ2);
    check(openOnlyˢ, sub, err);

    (_, err) = sub.Open(nonexistˢ);
    if (err == nil) {
        t.Fatal(openNonexistSucceededˢ);
    }
    var (pe, ok) = err._<ж<fs.PathError>>(ᐧ);
    if (!ok) {
        t.Fatalf("Open(nonexist): error is %T, want *PathError", err);
    }
    if (pe.Path != "nonexist"u8) {
        t.Fatalf("Open(nonexist): err.Path = %q, want %q", pe.Path, nonexistˢ);
    }

    (_, err) = sub.Open("./");
    if (!errors.Is(err, ErrInvalid)) {
        t.Fatalf("Open(./): error is %v, want %v", err, ErrInvalid);
    }
}

} // end fs_test_package
