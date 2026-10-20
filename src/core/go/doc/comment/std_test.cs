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
    var (@out, err) = testenv.Command(new comment_internal_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new comment_internal_test_package.testing_TжTB(Ꮡt)), listˢ, stdˢ).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("%v\n%s"u8, err, @out);
    }

    slice<@string> list = default!;
    foreach (var (_, pkg) in strings.Fields((@string)@out)) {
        if (!strings.Contains(pkg, "/"u8)) {
            list = append(list, pkg);
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
