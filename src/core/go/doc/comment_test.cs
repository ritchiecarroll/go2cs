// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using bytes = bytes_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using diff = global::go.@internal.diff_package;
using testing = testing_package;
using ast = global::go.go.ast_package;
using fs = global::go.io.fs_package;
using global::go.@internal;
using global::go.go;
using io = io_package;
using static global::go.go.doc_package;

partial class doc_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataPkgdocˢ = "testdata/pkgdoc"u8;
internal static readonly @string pkgdocˢ = "pkgdoc"u8;
internal static readonly object missingPackagePkgdocˢ = (@string)"missing package pkgdoc"u8;
internal static readonly @string tAndUAreTypesAndTMIsAˢ = "[T] and [U] are types, and [T.M] is a method, but [V] is a broken link. [rand.Int] and [crand.Reader] are things. [G.X] is a field, [G.M1] and [G.M2] are generic methods. [I.F] is an interface method and [I.V] is a broken link.\n"u8;
internal static readonly @string pTAndUAreITypesIAndTMIsAˢ = "<p>[T] and [U] are <i>types</i>, and [T.M] is a method, but [V] is a broken link. [rand.Int] and [crand.Reader] are things. [G.X] is a field, [G.M1] and [G.M2] are generic methods. [I.F] is an interface method and [I.V] is a broken link.\n"u8;
internal static readonly @string tTAndUUAreTypesAndTMTMIsˢ = "[T](#T) and [U](#U) are types, and [T.M](#T.M) is a method, but \\[V] is a broken link. [rand.Int](/math/rand#Int) and [crand.Reader](/crypto/rand#Reader) are things. [G.X](#G.X) is a field, [G.M1](#G.M1) and [G.M2](#G.M2) are generic methods. [I.F](#I.F) is an interface method and \\[I.V] is a broken link.\n"u8;
internal static readonly @string tAndUAreTypesAndTMIsAˢ2 = "T and U are types, and T.M is a method, but [V] is a broken link. rand.Int and\ncrand.Reader are things. G.X is a field, G.M1 and G.M2 are generic methods.\nI.F is an interface method and [I.V] is a broken link.\n"u8;
internal static readonly @string tAndUAreTypesAndTMIsAˢ3 = "[T] and [U] are types, and [T.M] is a method, but [V] is a broken link.\n[rand.Int] and [crand.Reader] are things. [G.X] is a field, [G.M1] and [G.M2]\nare generic methods. [I.F] is an interface method and [I.V] is a broken link.\n"u8;
internal static readonly @string tAndUAreTypesAndTMIsAˢ4 = "T and U are types, and T.M is a method, but [V] is a broken link."u8;
internal static readonly @string tAndUAreTypesAndTMIsAˢ5 = "[T] and [U] are types, and [T.M] is a method, but [V] is a broken link."u8;
internal static readonly @string pkgHtmlˢ = "pkg.HTML"u8;
internal static readonly @string wantˢ = "want"u8;
internal static readonly @string pkgMarkdownˢ = "pkg.Markdown"u8;
internal static readonly @string pkgTextˢ = "pkg.Text"u8;
internal static readonly @string pkgSynopsisˢ = "pkg.Synopsis"u8;
internal static readonly @string toHTMLˢ = "ToHTML"u8;
internal static readonly @string toTextˢ = "ToText"u8;
internal static readonly @string synopsisˢ = "Synopsis"u8;

public static void TestComment(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var fset = token.NewFileSet();
    var (pkgs, err) = parser.ParseDir(fset, testdataPkgdocˢ, default!, parser.ParseComments);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (pkgs[pkgdocˢ] == nil) {
        Ꮡt.Fatal(missingPackagePkgdocˢ);
    }
    var pkg = New(pkgs[pkgdocˢ], testdataPkgdocˢ, 0);

    @string input = tAndUAreTypesAndTMIsAˢ;
    @string wantHTML = @"<p><a href=""#T"">T</a> and <a href=""#U"">U</a> are types, and <a href=""#T.M"">T.M</a> is a method, but [V] is a broken link. <a href=""/math/rand#Int"">rand.Int</a> and <a href=""/crypto/rand#Reader"">crand.Reader</a> are things. <a href=""#G.X"">G.X</a> is a field, <a href=""#G.M1"">G.M1</a> and <a href=""#G.M2"">G.M2</a> are generic methods. <a href=""#I.F"">I.F</a> is an interface method and [I.V] is a broken link."u8 + "\n"u8;
    @string wantOldHTML = pTAndUAreITypesIAndTMIsAˢ;
    @string wantMarkdown = tTAndUUAreTypesAndTMTMIsˢ;
    @string wantText = tAndUAreTypesAndTMIsAˢ2;
    @string wantOldText = tAndUAreTypesAndTMIsAˢ3;
    @string wantSynopsis = tAndUAreTypesAndTMIsAˢ4;
    @string wantOldSynopsis = tAndUAreTypesAndTMIsAˢ5;

    {
        var b = pkg.HTML(input); if (((sstring)b) != wantHTML) {
            Ꮡt.Errorf("%s"u8, diff.Diff(pkgHtmlˢ, b, wantˢ, slice<byte>(wantHTML)));
        }
    }
    {
        var b = pkg.Markdown(input); if (((sstring)b) != wantMarkdown) {
            Ꮡt.Errorf("%s"u8, diff.Diff(pkgMarkdownˢ, b, wantˢ, slice<byte>(wantMarkdown)));
        }
    }
    {
        var b = pkg.Text(input); if (((sstring)b) != wantText) {
            Ꮡt.Errorf("%s"u8, diff.Diff(pkgTextˢ, b, wantˢ, slice<byte>(wantText)));
        }
    }
    {
        @string b = pkg.Synopsis(input); if (b != wantSynopsis) {
            Ꮡt.Errorf("%s"u8, diff.Diff(pkgSynopsisˢ, slice<byte>(b), wantˢ, slice<byte>(wantText)));
        }
    }

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);

    buf.Reset();
    ToHTML(new doc_test_package.bytes_BufferжWriter(Ꮡbuf), input, new map<@string, @string>{["types"u8] = ""u8});
    {
        var b = buf.Bytes(); if (((sstring)b) != wantOldHTML) {
            Ꮡt.Errorf("%s"u8, diff.Diff(toHTMLˢ, b, wantˢ, slice<byte>(wantOldHTML)));
        }
    }

    buf.Reset();
    ToText(new doc_test_package.bytes_BufferжWriter(Ꮡbuf), input, ""u8, "\t"u8, 80);
    {
        var b = buf.Bytes(); if (((sstring)b) != wantOldText) {
            Ꮡt.Errorf("%s"u8, diff.Diff(toTextˢ, b, wantˢ, slice<byte>(wantOldText)));
        }
    }

    {
        @string b = Synopsis(input); if (b != wantOldSynopsis) {
            Ꮡt.Errorf("%s"u8, diff.Diff(synopsisˢ, slice<byte>(b), wantˢ, slice<byte>(wantOldText)));
        }
    }
}

} // end doc_internal_test_package
