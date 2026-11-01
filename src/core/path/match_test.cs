// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using static path_package;
using testing = testing_package;

partial class path_test_package {

partial struct MatchTest {
    internal @string pattern, s;
    internal bool match;
    internal error err;
}

internal static slice<MatchTest> matchTests = slice<MatchTest>([
    new("abc", "abc", true, default),
    new("*", "abc", true, default),
    new("*c", "abc", true, default),
    new("a*", "a", true, default),
    new("a*", "abc", true, default),
    new("a*", "ab/c", false, default),
    new("a*/b", "abc/b", true, default),
    new("a*/b", "a/c/b", false, default),
    new("a*b*c*d*e*/f", "axbxcxdxe/f", true, default),
    new("a*b*c*d*e*/f", "axbxcxdxexxx/f", true, default),
    new("a*b*c*d*e*/f", "axbxcxdxe/xxx/f", false, default),
    new("a*b*c*d*e*/f", "axbxcxdxexxx/fff", false, default),
    new("a*b?c*x", "abxbbxdbxebxczzx", true, default),
    new("a*b?c*x", "abxbbxdbxebxczzy", false, default),
    new("ab[c]", "abc", true, default),
    new("ab[b-d]", "abc", true, default),
    new("ab[e-g]", "abc", false, default),
    new("ab[^c]", "abc", false, default),
    new("ab[^b-d]", "abc", false, default),
    new("ab[^e-g]", "abc", true, default),
    new("a\\*b", "a*b", true, default),
    new("a\\*b", "ab", false, default),
    new("a?b", "a☺b", true, default),
    new("a[^a]b", "a☺b", true, default),
    new("a???b", "a☺b", false, default),
    new("a[^a][^a][^a]b", "a☺b", false, default),
    new("[a-ζ]*", "α", true, default),
    new("*[a-ζ]", "A", false, default),
    new("a?b", "a/b", false, default),
    new("a*b", "a/b", false, default),
    new("[\\]a]", "]", true, default),
    new("[\\-]", "-", true, default),
    new("[x\\-]", "x", true, default),
    new("[x\\-]", "-", true, default),
    new("[x\\-]", "z", false, default),
    new("[\\-x]", "x", true, default),
    new("[\\-x]", "-", true, default),
    new("[\\-x]", "a", false, default),
    new("[]a]", "]", false, ErrBadPattern),
    new("[-]", "-", false, ErrBadPattern),
    new("[x-]", "x", false, ErrBadPattern),
    new("[x-]", "-", false, ErrBadPattern),
    new("[x-]", "z", false, ErrBadPattern),
    new("[-x]", "x", false, ErrBadPattern),
    new("[-x]", "-", false, ErrBadPattern),
    new("[-x]", "a", false, ErrBadPattern),
    new("\\", "a", false, ErrBadPattern),
    new("[a-b-c]", "a", false, ErrBadPattern),
    new("[", "a", false, ErrBadPattern),
    new("[^", "a", false, ErrBadPattern),
    new("[^bc", "a", false, ErrBadPattern),
    new("a[", "a", false, ErrBadPattern),
    new("a[", "ab", false, ErrBadPattern),
    new("a[", "x", false, ErrBadPattern),
    new("a/b[", "x", false, ErrBadPattern),
    new("*x", "xxx", true, default)
]);

public static void TestMatch(ж<testing.T> t) {
    foreach (var (_, tt) in matchTests) {
        var (ok, err) = Match(tt.pattern, tt.s);
        if (ok != tt.match || !AreEqual(err, tt.err)) {
            t.Errorf("Match(%#q, %#q) = %v, %v want %v, %v", tt.pattern, tt.s, ok, err, tt.match, tt.err);
        }
    }
}

public static void BenchmarkMatch(ж<testing.B> b) {
    foreach (var (_, vᴛ1) in matchTests) {
        ref var tt = ref heap(new MatchTest(), out var Ꮡtt);
        tt = vᴛ1;

        @string name = fmt.Sprintf("%q %q"u8, tt.pattern, tt.s);
        var ttʗ1 = tt;
        b.Run(name, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            foreach (var _ᴛ1 in range((~bΔ1).N)) {
                (bSink, errSink) = Match(ttʗ1.pattern, ttʗ1.s);
            }
        });
    }
}

internal static bool bSink;
internal static error errSink;

} // end path_test_package
