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
internal static ref readonly @string testdataPkgdocˢ => ref testdataPkgdocˢᵇ;
private static readonly @string testdataPkgdocˢᵇ = "testdata/pkgdoc"u8;
internal static ref readonly @string pkgdocˢ => ref pkgdocˢᵇ;
private static readonly @string pkgdocˢᵇ = "pkgdoc"u8;
internal static readonly object missingPackagePkgdocˢ = (@string)"missing package pkgdoc"u8;
internal static ref readonly @string tAndUAreTypesAndTMIsAˢ => ref tAndUAreTypesAndTMIsAˢᵇ;
private static readonly @string tAndUAreTypesAndTMIsAˢᵇ = "[T] and [U] are types, and [T.M] is a method, but [V] is a broken link. [rand.Int] and [crand.Reader] are things. [G.X] is a field, [G.M1] and [G.M2] are generic methods. [I.F] is an interface method and [I.V] is a broken link.\n"u8;
internal static ref readonly @string pTAndUAreITypesIAndTMIsAˢ => ref pTAndUAreITypesIAndTMIsAˢᵇ;
private static readonly @string pTAndUAreITypesIAndTMIsAˢᵇ = "<p>[T] and [U] are <i>types</i>, and [T.M] is a method, but [V] is a broken link. [rand.Int] and [crand.Reader] are things. [G.X] is a field, [G.M1] and [G.M2] are generic methods. [I.F] is an interface method and [I.V] is a broken link.\n"u8;
internal static ref readonly @string tTAndUUAreTypesAndTMTMIsˢ => ref tTAndUUAreTypesAndTMTMIsˢᵇ;
private static readonly @string tTAndUUAreTypesAndTMTMIsˢᵇ = "[T](#T) and [U](#U) are types, and [T.M](#T.M) is a method, but \\[V] is a broken link. [rand.Int](/math/rand#Int) and [crand.Reader](/crypto/rand#Reader) are things. [G.X](#G.X) is a field, [G.M1](#G.M1) and [G.M2](#G.M2) are generic methods. [I.F](#I.F) is an interface method and \\[I.V] is a broken link.\n"u8;
internal static ref readonly @string tAndUAreTypesAndTMIsAˢ2 => ref tAndUAreTypesAndTMIsAˢ2ᵇ;
private static readonly @string tAndUAreTypesAndTMIsAˢ2ᵇ = "T and U are types, and T.M is a method, but [V] is a broken link. rand.Int and\ncrand.Reader are things. G.X is a field, G.M1 and G.M2 are generic methods.\nI.F is an interface method and [I.V] is a broken link.\n"u8;
internal static ref readonly @string tAndUAreTypesAndTMIsAˢ3 => ref tAndUAreTypesAndTMIsAˢ3ᵇ;
private static readonly @string tAndUAreTypesAndTMIsAˢ3ᵇ = "[T] and [U] are types, and [T.M] is a method, but [V] is a broken link.\n[rand.Int] and [crand.Reader] are things. [G.X] is a field, [G.M1] and [G.M2]\nare generic methods. [I.F] is an interface method and [I.V] is a broken link.\n"u8;
internal static ref readonly @string tAndUAreTypesAndTMIsAˢ4 => ref tAndUAreTypesAndTMIsAˢ4ᵇ;
private static readonly @string tAndUAreTypesAndTMIsAˢ4ᵇ = "T and U are types, and T.M is a method, but [V] is a broken link."u8;
internal static ref readonly @string tAndUAreTypesAndTMIsAˢ5 => ref tAndUAreTypesAndTMIsAˢ5ᵇ;
private static readonly @string tAndUAreTypesAndTMIsAˢ5ᵇ = "[T] and [U] are types, and [T.M] is a method, but [V] is a broken link."u8;
internal static ref readonly @string pkgHtmlˢ => ref pkgHtmlˢᵇ;
private static readonly @string pkgHtmlˢᵇ = "pkg.HTML"u8;
internal static ref readonly @string wantˢ => ref wantˢᵇ;
private static readonly @string wantˢᵇ = "want"u8;
internal static ref readonly @string pkgMarkdownˢ => ref pkgMarkdownˢᵇ;
private static readonly @string pkgMarkdownˢᵇ = "pkg.Markdown"u8;
internal static ref readonly @string pkgTextˢ => ref pkgTextˢᵇ;
private static readonly @string pkgTextˢᵇ = "pkg.Text"u8;
internal static ref readonly @string pkgSynopsisˢ => ref pkgSynopsisˢᵇ;
private static readonly @string pkgSynopsisˢᵇ = "pkg.Synopsis"u8;
internal static ref readonly @string toHTMLˢ => ref toHTMLˢᵇ;
private static readonly @string toHTMLˢᵇ = "ToHTML"u8;
internal static ref readonly @string toTextˢ => ref toTextˢᵇ;
private static readonly @string toTextˢᵇ = "ToText"u8;
internal static ref readonly @string synopsisˢ => ref synopsisˢᵇ;
private static readonly @string synopsisˢᵇ = "Synopsis"u8;

public static void TestComment(ж<testing.T> t) {
    var fset = token.NewFileSet();
    var (pkgs, err) = parser.ParseDir(fset, testdataPkgdocˢ, default, parser.ParseComments);
    if (err != nil) {
        t.Fatal(err);
    }
    if (pkgs[pkgdocˢ] == nil) {
        t.Fatal(missingPackagePkgdocˢ);
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
            t.Errorf("%s", diff.Diff(pkgHtmlˢ, b, wantˢ, slice<byte>(wantHTML)));
        }
    }
    {
        var b = pkg.Markdown(input); if (((sstring)b) != wantMarkdown) {
            t.Errorf("%s", diff.Diff(pkgMarkdownˢ, b, wantˢ, slice<byte>(wantMarkdown)));
        }
    }
    {
        var b = pkg.Text(input); if (((sstring)b) != wantText) {
            t.Errorf("%s", diff.Diff(pkgTextˢ, b, wantˢ, slice<byte>(wantText)));
        }
    }
    {
        @string b = pkg.Synopsis(input); if (b != wantSynopsis) {
            t.Errorf("%s", diff.Diff(pkgSynopsisˢ, slice<byte>(b), wantˢ, slice<byte>(wantText)));
        }
    }

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);

    buf.Reset();
    ToHTML(new doc_test_package.bytes_BufferжWriter(Ꮡbuf), input, new map<@string, @string>{["types"] = ""});
    {
        var b = buf.Bytes(); if (((sstring)b) != wantOldHTML) {
            t.Errorf("%s", diff.Diff(toHTMLˢ, b, wantˢ, slice<byte>(wantOldHTML)));
        }
    }

    buf.Reset();
    ToText(new doc_test_package.bytes_BufferжWriter(Ꮡbuf), input, ""u8, "\t", 80);
    {
        var b = buf.Bytes(); if (((sstring)b) != wantOldText) {
            t.Errorf("%s", diff.Diff(toTextˢ, b, wantˢ, slice<byte>(wantOldText)));
        }
    }

    {
        @string b = Synopsis(input); if (b != wantOldSynopsis) {
            t.Errorf("%s", diff.Diff(synopsisˢ, slice<byte>(b), wantˢ, slice<byte>(wantOldText)));
        }
    }
}

} // end doc_internal_test_package
