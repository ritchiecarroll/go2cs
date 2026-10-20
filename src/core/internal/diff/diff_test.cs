// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using txtar = go.@internal.txtar_package;
using filepath = path.filepath_package;
using testing = testing_package;
using go.@internal;
using path;
using static go.@internal.diff_package;

partial class diff_internal_test_package {

internal static slice<byte> clean(slice<byte> text) {
    text = bytes.ReplaceAll(text, slice<byte>("$\n"u8), slice<byte>("\n"u8));
    text = bytes.TrimSuffix(text, slice<byte>("^D\n"u8));
    return text;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTxtˢ = "testdata/*.txt"u8;
internal static readonly @string haveˢ = "have"u8;
internal static readonly @string wantˢ = "want"u8;

public static void Test(ж<testing.T> Ꮡt) {
    var (files, _) = filepath.Glob(testdataTxtˢ);
    if (len(files) == 0) {
        Ꮡt.Fatalf("no testdata"u8);
    }

    foreach (var (_, @file) in files) {
        Ꮡt.Run(filepath.Base(@file), (ж<testing.T> tΔ1) => {
            var (a, err) = txtar.ParseFile(@file);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            if (len((~a).Files) != 3 || (~a).Files[2].Name != "diff"u8) {
                tΔ1.Fatalf("%s: want three files, third named \"diff\""u8, @file);
            }
            var diffs = Diff((~a).Files[0].Name, clean((~a).Files[0].Data), (~a).Files[1].Name, clean((~a).Files[1].Data));
            var want = clean((~a).Files[2].Data);
            if (!bytes.Equal(diffs, want)) {
                tΔ1.Fatalf("%s: have:\n%s\nwant:\n%s\n%s"u8, @file,
                    diffs, want, Diff(haveˢ, diffs, wantˢ, want));
            }
        });
    }
}

} // end diff_internal_test_package
