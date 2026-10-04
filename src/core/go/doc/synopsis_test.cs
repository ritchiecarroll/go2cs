// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using testing = testing_package;
using static global::go.go.doc_package;

partial class doc_internal_test_package {


[GoType("dyn")] partial struct testsᴛ1 {
    internal @string txt;
    internal nint fsl;
    internal @string syn;
}
internal static slice<testsᴛ1> tests = new testsᴛ1[]{
    new(""u8, 0, ""u8),
    new("foo"u8, 3, "foo"u8),
    new("foo."u8, 4, "foo."u8),
    new("foo.bar"u8, 7, "foo.bar"u8),
    new("  foo.  "u8, 6, "foo."u8),
    new("  foo\t  bar.\n"u8, 12, "foo bar."u8),
    new("  foo\t  bar.\n"u8, 12, "foo bar."u8),
    new("a  b\n\nc\r\rd\t\t"u8, 12, "a b"u8),
    new("a  b\n\nc\r\rd\t\t  . BLA"u8, 15, "a b"u8),
    new("Package poems by T.S.Eliot. To rhyme..."u8, 27, "Package poems by T.S.Eliot."u8),
    new("Package poems by T. S. Eliot. To rhyme..."u8, 29, "Package poems by T. S. Eliot."u8),
    new("foo implements the foo ABI. The foo ABI is..."u8, 27, "foo implements the foo ABI."u8),
    new("Package\nfoo. .."u8, 12, "Package foo."u8),
    new("P . Q."u8, 3, "P ."u8),
    new("P. Q.   "u8, 8, "P. Q."u8),
    new("Package Καλημέρα κόσμε."u8, 36, "Package Καλημέρα κόσμε."u8),
    new("Package こんにちは 世界\n"u8, 31, "Package こんにちは 世界"u8),
    new("Package こんにちは。世界"u8, 26, "Package こんにちは。"u8),
    new("Package 안녕．世界"u8, 17, "Package 안녕．"u8),
    new("Package foo does bar."u8, 21, "Package foo does bar."u8),
    new("Copyright 2012 Google, Inc. Package foo does bar."u8, 27, ""u8),
    new("All Rights reserved. Package foo does bar."u8, 20, ""u8),
    new("All rights reserved. Package foo does bar."u8, 20, ""u8),
    new("Authors: foo@bar.com. Package foo does bar."u8, 21, ""u8),
    new("typically invoked as ``go tool asm'',"u8, 37, "typically invoked as “go tool asm”,"u8)
}.slice();

public static void TestSynopsis(ж<testing.T> Ꮡt) {
    foreach (var (_, e) in tests) {
        @string fs = firstSentence(e.txt);
        if (fs != e.txt.slice(0, e.fsl)) {
            Ꮡt.Errorf("firstSentence(%q) = %q, want %q"u8, e.txt, fs, e.txt.slice(0, e.fsl));
        }
        @string syn = Synopsis(e.txt);
        if (syn != e.syn) {
            Ꮡt.Errorf("Synopsis(%q) = %q, want %q"u8, e.txt, syn, e.syn);
        }
    }
}

} // end doc_internal_test_package
