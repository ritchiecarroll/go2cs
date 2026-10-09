// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static path_package;
using testing = testing_package;

partial class path_test_package {

partial struct MatchTest {
    internal @string pattern, s;
    internal bool match;
    internal error err;
}

internal static slice<MatchTest> matchTests = new MatchTest[]{
    new("abc"u8, "abc"u8, true, default!),
    new("*"u8, "abc"u8, true, default!),
    new("*c"u8, "abc"u8, true, default!),
    new("a*"u8, "a"u8, true, default!),
    new("a*"u8, "abc"u8, true, default!),
    new("a*"u8, "ab/c"u8, false, default!),
    new("a*/b"u8, "abc/b"u8, true, default!),
    new("a*/b"u8, "a/c/b"u8, false, default!),
    new("a*b*c*d*e*/f"u8, "axbxcxdxe/f"u8, true, default!),
    new("a*b*c*d*e*/f"u8, "axbxcxdxexxx/f"u8, true, default!),
    new("a*b*c*d*e*/f"u8, "axbxcxdxe/xxx/f"u8, false, default!),
    new("a*b*c*d*e*/f"u8, "axbxcxdxexxx/fff"u8, false, default!),
    new("a*b?c*x"u8, "abxbbxdbxebxczzx"u8, true, default!),
    new("a*b?c*x"u8, "abxbbxdbxebxczzy"u8, false, default!),
    new("ab[c]"u8, "abc"u8, true, default!),
    new("ab[b-d]"u8, "abc"u8, true, default!),
    new("ab[e-g]"u8, "abc"u8, false, default!),
    new("ab[^c]"u8, "abc"u8, false, default!),
    new("ab[^b-d]"u8, "abc"u8, false, default!),
    new("ab[^e-g]"u8, "abc"u8, true, default!),
    new("a\\*b"u8, "a*b"u8, true, default!),
    new("a\\*b"u8, "ab"u8, false, default!),
    new("a?b"u8, "a☺b"u8, true, default!),
    new("a[^a]b"u8, "a☺b"u8, true, default!),
    new("a???b"u8, "a☺b"u8, false, default!),
    new("a[^a][^a][^a]b"u8, "a☺b"u8, false, default!),
    new("[a-ζ]*"u8, "α"u8, true, default!),
    new("*[a-ζ]"u8, "A"u8, false, default!),
    new("a?b"u8, "a/b"u8, false, default!),
    new("a*b"u8, "a/b"u8, false, default!),
    new("[\\]a]"u8, "]"u8, true, default!),
    new("[\\-]"u8, "-"u8, true, default!),
    new("[x\\-]"u8, "x"u8, true, default!),
    new("[x\\-]"u8, "-"u8, true, default!),
    new("[x\\-]"u8, "z"u8, false, default!),
    new("[\\-x]"u8, "x"u8, true, default!),
    new("[\\-x]"u8, "-"u8, true, default!),
    new("[\\-x]"u8, "a"u8, false, default!),
    new("[]a]"u8, "]"u8, false, ErrBadPattern),
    new("[-]"u8, "-"u8, false, ErrBadPattern),
    new("[x-]"u8, "x"u8, false, ErrBadPattern),
    new("[x-]"u8, "-"u8, false, ErrBadPattern),
    new("[x-]"u8, "z"u8, false, ErrBadPattern),
    new("[-x]"u8, "x"u8, false, ErrBadPattern),
    new("[-x]"u8, "-"u8, false, ErrBadPattern),
    new("[-x]"u8, "a"u8, false, ErrBadPattern),
    new("\\"u8, "a"u8, false, ErrBadPattern),
    new("[a-b-c]"u8, "a"u8, false, ErrBadPattern),
    new("["u8, "a"u8, false, ErrBadPattern),
    new("[^"u8, "a"u8, false, ErrBadPattern),
    new("[^bc"u8, "a"u8, false, ErrBadPattern),
    new("a["u8, "a"u8, false, ErrBadPattern),
    new("a["u8, "ab"u8, false, ErrBadPattern),
    new("a["u8, "x"u8, false, ErrBadPattern),
    new("a/b["u8, "x"u8, false, ErrBadPattern),
    new("*x"u8, "xxx"u8, true, default!)
}.slice();

public static void TestMatch(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in matchTests) {
        var (ok, err) = Match(tt.pattern, tt.s);
        if (ok != tt.match || !AreEqual(err, tt.err)) {
            Ꮡt.Errorf("Match(%#q, %#q) = %v, %v want %v, %v"u8, tt.pattern, tt.s, ok, err, tt.match, tt.err);
        }
    }
}

} // end path_test_package
