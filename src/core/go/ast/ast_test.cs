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
internal static slice<commentsᴛ1> comments = [
    new(["//"], ""),
    new(["//   "], ""),
    new(["//", "//", "//   "], ""),
    new(["// foo   "], "foo\n"),
    new(["//", "//", "// foo"], "foo\n"),
    new(["// foo  bar  "], "foo  bar\n"),
    new(["// foo", "// bar"], "foo\nbar\n"),
    new(["// foo", "//", "//", "//", "// bar"], "foo\n\nbar\n"),
    new(["// foo", "/* bar */"], "foo\n bar\n"),
    new(["//", "//", "//", "// foo", "//", "//", "//"], "foo\n"),

    new(["/**/"], ""),
    new(["/*   */"], ""),
    new(["/**/", "/**/", "/*   */"], ""),
    new(["/* Foo   */"], " Foo\n"),
    new(["/* Foo  Bar  */"], " Foo  Bar\n"),
    new(["/* Foo*/", "/* Bar*/"], " Foo\n Bar\n"),
    new(["/* Foo*/", "/**/", "/**/", "/**/", "// Bar"], " Foo\n\nBar\n"),
    new(["/* Foo*/", "/*\n*/", "//", "/*\n*/", "// Bar"], " Foo\n\nBar\n"),
    new(["/* Foo*/", "// Bar"], " Foo\nBar\n"),
    new(["/* Foo\n Bar*/"], " Foo\n Bar\n"),

    new(["// foo", "//go:noinline", "// bar", "//:baz"], "foo\nbar\n:baz\n"),
    new(["// foo", "//lint123:ignore", "// bar"], "foo\nbar\n")
];

public static void TestCommentText(ж<testing.T> t) {
    foreach (var (i, c) in range(comments)) {
        var list = new slice<ж<global::go.go.ast_package.Comment>>(len(c.list));
        foreach (var (iΔ1, vᴛ1) in range(c.list)) {
            ref var s = ref heap(new @string(), out var Ꮡs);
            s = vᴛ1;

            list[iΔ1] = Ꮡ(new Comment(Text: s));
        }

        @string text = (Ꮡ(new CommentGroup(list))).Text();
        if (text != c.text) {
            t.Errorf("case %d: got %q; expected %q", i, text, c.text);
        }
    }
}

partial struct isDirectiveTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal bool ok;
}
internal static slice<isDirectiveTestsᴛ1> isDirectiveTests = [
    new("abc", false),
    new("go:inline", true),
    new("Go:inline", false),
    new("go:Inline", false),
    new(":inline", false),
    new("lint:ignore", true),
    new("lint:1234", true),
    new("1234:lint", true),
    new("go: inline", false),
    new("go:", false),
    new("go:*", false),
    new("go:x*", true),
    new("export foo", true),
    new("extern foo", true),
    new("expert foo", false)
];

public static void TestIsDirective(ж<testing.T> t) {
    foreach (var (_, tt) in range(isDirectiveTests)) {
        var ok = isDirective(tt.@in); if (ok != tt.ok) {
            t.Errorf("isDirective(%q) = %v, want %v", tt.@in, ok, tt.ok);
        }
    }
}

} // end ast_internal_test_package
