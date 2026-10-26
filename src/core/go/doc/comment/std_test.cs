// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.doc;

using diff = @internal.diff_package;
using testenv = @internal.testenv_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using exec = os.exec_package;
using os;
using static global::go.go.doc.comment_package;

partial class comment_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string listˢ = "list"u8;
internal static readonly @string stdˢ = "std"u8;
internal static readonly @string stdPkgsˢ = "stdPkgs"u8;
internal static readonly @string wantˢ = "want"u8;

public static void TestStd(ж<testing.T> Ꮡt) {
    var listᴷ = new stackAppend<@string>();

    var cmd = testenv.Command(new comment_internal_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new comment_internal_test_package.testing_TжTB(Ꮡt)), listˢ, stdˢ);
    cmd.Value.Env = append(cmd.Environ(), "GOEXPERIMENT=none"u8);
    var (@out, err) = cmd.CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("%v\n%s"u8, err, @out);
    }

    slice<@string> list = default!;
    foreach (var (_, pkg) in strings.Fields((@string)@out)) {
        if (!strings.Contains(pkg, "/"u8)) {
            list = append(listᴷ, list, pkg);
        }
    }
    slices.Sort<slice<@string>, @string>(list);

    @string have = strings.Join(stdPkgs, "\n"u8) + "\n"u8;
    @string want = strings.Join(list, "\n"u8) + "\n"u8;
    if (have != want) {
        Ꮡt.Errorf("stdPkgs is out of date: regenerate with 'go generate'\n%s"u8, diff.Diff(stdPkgsˢ, slice<byte>(have), wantˢ, slice<byte>(want)));
    }
}

} // end comment_internal_test_package
