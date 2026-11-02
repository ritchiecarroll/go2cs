// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using testing = testing_package;
using static global::go.go.doc_package;

partial class doc_internal_test_package {

partial struct testsᴛ1 /*dyn*/ {
    internal @string txt;
    internal nint fsl;
    internal @string syn;
}
internal static slice<testsᴛ1> tests = slice<testsᴛ1>([
    new("", 0, ""),
    new("foo", 3, "foo"),
    new("foo.", 4, "foo."),
    new("foo.bar", 7, "foo.bar"),
    new("  foo.  ", 6, "foo."),
    new("  foo\t  bar.\n", 12, "foo bar."),
    new("  foo\t  bar.\n", 12, "foo bar."),
    new("a  b\n\nc\r\rd\t\t", 12, "a b"),
    new("a  b\n\nc\r\rd\t\t  . BLA", 15, "a b"),
    new("Package poems by T.S.Eliot. To rhyme...", 27, "Package poems by T.S.Eliot."),
    new("Package poems by T. S. Eliot. To rhyme...", 29, "Package poems by T. S. Eliot."),
    new("foo implements the foo ABI. The foo ABI is...", 27, "foo implements the foo ABI."),
    new("Package\nfoo. ..", 12, "Package foo."),
    new("P . Q.", 3, "P ."),
    new("P. Q.   ", 8, "P. Q."),
    new("Package Καλημέρα κόσμε.", 36, "Package Καλημέρα κόσμε."),
    new("Package こんにちは 世界\n", 31, "Package こんにちは 世界"),
    new("Package こんにちは。世界", 26, "Package こんにちは。"),
    new("Package 안녕．世界", 17, "Package 안녕．"),
    new("Package foo does bar.", 21, "Package foo does bar."),
    new("Copyright 2012 Google, Inc. Package foo does bar.", 27, ""),
    new("All Rights reserved. Package foo does bar.", 20, ""),
    new("All rights reserved. Package foo does bar.", 20, ""),
    new("Authors: foo@bar.com. Package foo does bar.", 21, ""),
    new("typically invoked as ``go tool asm'',", 37, "typically invoked as “go tool asm”,")
]);

public static void TestSynopsis(ж<testing.T> t) {
    foreach (var (_, e) in tests) {
        @string fs = firstSentence(e.txt);
        if (fs != e.txt.slice(0, e.fsl)) {
            t.Errorf("firstSentence(%q) = %q, want %q", e.txt, fs, e.txt.slice(0, e.fsl));
        }
        @string syn = Synopsis(e.txt);
        if (syn != e.syn) {
            t.Errorf("Synopsis(%q) = %q, want %q", e.txt, syn, e.syn);
        }
    }
}

} // end doc_internal_test_package
