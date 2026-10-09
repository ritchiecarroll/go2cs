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
internal static slice<isValidPathTestsᴛ1> isValidPathTests = new isValidPathTestsᴛ1[]{
    new("."u8, true),
    new("x"u8, true),
    new("x/y"u8, true),
    new(""u8, false),
    new(".."u8, false),
    new("/"u8, false),
    new("x/"u8, false),
    new("/x"u8, false),
    new("x/y/"u8, false),
    new("/x/y"u8, false),
    new("./"u8, false),
    new("./x"u8, false),
    new("x/."u8, false),
    new("x/./y"u8, false),
    new("../"u8, false),
    new("../x"u8, false),
    new("x/.."u8, false),
    new("x/../y"u8, false),
    new("x//y"u8, false),
    new(@"x\"u8, true),
    new(@"x\y"u8, true),
    new(@"x:y"u8, true),
    new(@"\x"u8, true)
}.slice();

public static void TestValidPath(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in isValidPathTests) {
        var ok = ValidPath(tt.name);
        if (ok != tt.ok) {
            Ꮡt.Errorf("ValidPath(%q) = %v, want %v"u8, tt.name, ok, tt.ok);
        }
    }
}

} // end fs_test_package
