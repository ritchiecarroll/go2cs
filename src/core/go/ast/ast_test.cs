// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using testing = testing_package;
using static global::go.go.ast_package;

partial class ast_internal_test_package {


partial struct commentsᴛ1 /*dyn*/ {
    internal slice<@string> list;
    internal @string text;
}
internal static slice<commentsᴛ1> comments = new commentsᴛ1[]{
    new(new @string[]{"//"u8}.slice(), ""u8),
    new(new @string[]{"//   "u8}.slice(), ""u8),
    new(new @string[]{"//"u8, "//"u8, "//   "u8}.slice(), ""u8),
    new(new @string[]{"// foo   "u8}.slice(), "foo\n"u8),
    new(new @string[]{"//"u8, "//"u8, "// foo"u8}.slice(), "foo\n"u8),
    new(new @string[]{"// foo  bar  "u8}.slice(), "foo  bar\n"u8),
    new(new @string[]{"// foo"u8, "// bar"u8}.slice(), "foo\nbar\n"u8),
    new(new @string[]{"// foo"u8, "//"u8, "//"u8, "//"u8, "// bar"u8}.slice(), "foo\n\nbar\n"u8),
    new(new @string[]{"// foo"u8, "/* bar */"u8}.slice(), "foo\n bar\n"u8),
    new(new @string[]{"//"u8, "//"u8, "//"u8, "// foo"u8, "//"u8, "//"u8, "//"u8}.slice(), "foo\n"u8),
    new(new @string[]{"/**/"u8}.slice(), ""u8),
    new(new @string[]{"/*   */"u8}.slice(), ""u8),
    new(new @string[]{"/**/"u8, "/**/"u8, "/*   */"u8}.slice(), ""u8),
    new(new @string[]{"/* Foo   */"u8}.slice(), " Foo\n"u8),
    new(new @string[]{"/* Foo  Bar  */"u8}.slice(), " Foo  Bar\n"u8),
    new(new @string[]{"/* Foo*/"u8, "/* Bar*/"u8}.slice(), " Foo\n Bar\n"u8),
    new(new @string[]{"/* Foo*/"u8, "/**/"u8, "/**/"u8, "/**/"u8, "// Bar"u8}.slice(), " Foo\n\nBar\n"u8),
    new(new @string[]{"/* Foo*/"u8, "/*\n*/"u8, "//"u8, "/*\n*/"u8, "// Bar"u8}.slice(), " Foo\n\nBar\n"u8),
    new(new @string[]{"/* Foo*/"u8, "// Bar"u8}.slice(), " Foo\nBar\n"u8),
    new(new @string[]{"/* Foo\n Bar*/"u8}.slice(), " Foo\n Bar\n"u8),
    new(new @string[]{"// foo"u8, "//go:noinline"u8, "// bar"u8, "//:baz"u8}.slice(), "foo\nbar\n:baz\n"u8),
    new(new @string[]{"// foo"u8, "//lint123:ignore"u8, "// bar"u8}.slice(), "foo\nbar\n"u8)
}.slice();

public static void TestCommentText(ж<testing.T> Ꮡt) {
    foreach (var (i, c) in comments) {
        var list = new slice<ж<global::go.go.ast_package.Comment>>(len(c.list));
        foreach (var (iΔ1, vᴛ1) in c.list) {
            ref var s = ref heap(new @string(), out var Ꮡs);
            s = vᴛ1;

            list[iΔ1] = Ꮡ(new Comment(Text: s));
        }
        @string text = (Ꮡ(new CommentGroup(list))).Text();
        if (text != c.text) {
            Ꮡt.Errorf("case %d: got %q; expected %q"u8, i, text, c.text);
        }
    }
}


partial struct isDirectiveTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal bool ok;
}
internal static slice<isDirectiveTestsᴛ1> isDirectiveTests = new isDirectiveTestsᴛ1[]{
    new("abc"u8, false),
    new("go:inline"u8, true),
    new("Go:inline"u8, false),
    new("go:Inline"u8, false),
    new(":inline"u8, false),
    new("lint:ignore"u8, true),
    new("lint:1234"u8, true),
    new("1234:lint"u8, true),
    new("go: inline"u8, false),
    new("go:"u8, false),
    new("go:*"u8, false),
    new("go:x*"u8, true),
    new("export foo"u8, true),
    new("extern foo"u8, true),
    new("expert foo"u8, false)
}.slice();

public static void TestIsDirective(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in isDirectiveTests) {
        {
            var ok = isDirective(tt.@in); if (ok != tt.ok) {
                Ꮡt.Errorf("isDirective(%q) = %v, want %v"u8, tt.@in, ok, tt.ok);
            }
        }
    }
}

} // end ast_internal_test_package
