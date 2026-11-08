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
internal static ref readonly @string listˢ => ref listˢᵇ;
private static readonly @string listˢᵇ = "list"u8;
internal static ref readonly @string stdˢ => ref stdˢᵇ;
private static readonly @string stdˢᵇ = "std"u8;
internal static ref readonly @string stdPkgsˢ => ref stdPkgsˢᵇ;
private static readonly @string stdPkgsˢᵇ = "stdPkgs"u8;
internal static ref readonly @string wantˢ => ref wantˢᵇ;
private static readonly @string wantˢᵇ = "want"u8;

public static void TestStd(ж<testing.T> t) {
    var listᴷ = new stackAppend<@string>();

    var cmd = testenv.Command(new comment_internal_test_package.testing_TжTB(t), testenv.GoToolPath(new comment_internal_test_package.testing_TжTB(t)), listˢ, stdˢ);
    cmd.Env = append(cmd.Environ(), (@string)"GOEXPERIMENT=none");
    var (@out, err) = cmd.CombinedOutput();
    if (err != nil) {
        t.Fatalf("%v\n%s", err, @out);
    }

    slice<@string> list = default;
    foreach (var (_, pkg) in range(strings.Fields((@string)@out))) {
        if (!strings.Contains(pkg, "/")) {
            list = append(listᴷ, list, pkg);
        }
    }
    slices.Sort<slice<@string>, @string>(list);

    @string have = strings.Join(stdPkgs, "\n") + "\n"u8;
    @string want = strings.Join(list, "\n") + "\n"u8;
    if (have != want) {
        t.Errorf("stdPkgs is out of date: regenerate with 'go generate'\n%s", diff.Diff(stdPkgsˢ, slice<byte>(have), wantˢ, slice<byte>(want)));
    }
}

} // end comment_internal_test_package
