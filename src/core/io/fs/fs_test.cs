// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.io;

using static go.io.fs_package;
using testing = testing_package;

partial class fs_test_package {

partial struct isValidPathTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal bool ok;
}
internal static slice<isValidPathTestsᴛ1> isValidPathTests = slice<isValidPathTestsᴛ1>([
    new(".", true),
    new("x", true),
    new("x/y", true),

    new("", false),
    new("..", false),
    new("/", false),
    new("x/", false),
    new("/x", false),
    new("x/y/", false),
    new("/x/y", false),
    new("./", false),
    new("./x", false),
    new("x/.", false),
    new("x/./y", false),
    new("../", false),
    new("../x", false),
    new("x/..", false),
    new("x/../y", false),
    new("x//y", false),
    new(@"x\", true),
    new(@"x\y", true),
    new(@"x:y", true),
    new(@"\x", true)
]);

public static void TestValidPath(ж<testing.T> t) {
    foreach (var (_, tt) in isValidPathTests) {
        var ok = ValidPath(tt.name);
        if (ok != tt.ok) {
            t.Errorf("ValidPath(%q) = %v, want %v", tt.name, ok, tt.ok);
        }
    }
}

} // end fs_test_package
