// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using bytes = bytes_package;
using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using global::go.go;
using io = io_package;
using static global::go.go.format_package;

partial class format_internal_test_package {

internal static readonly @string testfile = "format_test.go"u8;

internal static void diff(ж<testing.T> Ꮡt, slice<byte> dst, slice<byte> src) {
    nint line = 1;
    nint offs = 0; // line offset
    for (nint i = 0; i < len(dst) && i < len(src); i++) {
        var d = dst[i];
        var s = src[i];
        if (d != s) {
            Ꮡt.Errorf("dst:%d: %s\n"u8, line, dst[(int)(offs)..(int)(i + 1)]);
            Ꮡt.Errorf("src:%d: %s\n"u8, line, src[(int)(offs)..(int)(i + 1)]);
            return;
        }
        if (s == (rune)'\n') {
            line++;
            offs = i + 1;
        }
    }
    if (len(dst) != len(src)) {
        Ꮡt.Errorf("len(dst) = %d, len(src) = %d\nsrc = %q"u8, len(dst), len(src), src);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object nodeFailedˢ = (@string)"Node failed:"u8;

public static void TestNode(ж<testing.T> Ꮡt) {
    var (src, err) = os.ReadFile(testfile);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var fset = token.NewFileSet();
    (var @file, err) = parser.ParseFile(fset, testfile, src, parser.ParseComments);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        err = Node(new format_test_package.bytes_BufferжWriter(Ꮡbuf), fset, @file.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatal(nodeFailedˢ, err);
        }
    }
    diff(Ꮡt, buf.Bytes(), src);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ = "package p\n\nconst _ = 0000000123i\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string goldenᶜ = "package p\n\nconst _ = 123i\n"u8;

// Node is documented to not modify the AST.
// Test that it is so even when numbers are normalized.
public static void TestNodeNoModify(ж<testing.T> Ꮡt) {
    @string src = srcᶜ;
    @string golden = goldenᶜ;
    var fset = token.NewFileSet();
    var (@file, err) = parser.ParseFile(fset, ""u8, src, parser.ParseComments);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // Capture original address and value of a BasicLit node
    // which will undergo formatting changes during printing.
    var wantLit = (~(~(~@file).Decls[0]._<ж<ast.GenDecl>>()).Specs[0]._<ж<ast.ValueSpec>>()).Values[0]._<ж<ast.BasicLit>>();
    @string wantVal = wantLit.Value.Value;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        err = Node(new format_test_package.bytes_BufferжWriter(Ꮡbuf), fset, @file.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatal(nodeFailedˢ, err);
        }
    }
    diff(Ꮡt, buf.Bytes(), slice<byte>(golden));
    // Check if anything changed after Node returned.
    var gotLit = (~(~(~@file).Decls[0]._<ж<ast.GenDecl>>()).Specs[0]._<ж<ast.ValueSpec>>()).Values[0]._<ж<ast.BasicLit>>();
    @string gotVal = gotLit.Value.Value;
    if (gotLit != wantLit) {
        Ꮡt.Errorf("got *ast.BasicLit address %p, want %p"u8, gotLit.OrTypedNil(), wantLit.OrTypedNil());
    }
    if (gotVal != wantVal) {
        Ꮡt.Errorf("got *ast.BasicLit value %q, want %q"u8, gotVal, wantVal);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object sourceFailedˢ = (@string)"Source failed:"u8;

public static void TestSource(ж<testing.T> Ꮡt) {
    var (src, err) = os.ReadFile(testfile);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var res, err) = Source(src);
    if (err != default!) {
        Ꮡt.Fatal(sourceFailedˢ, err);
    }
    diff(Ꮡt, res, src);
}

// declaration lists
// statement lists
// indentation, leading and trailing space
// no indentation added inside raw strings
// no indentation removed inside raw strings
// comments
// issue #5551
// issue #11276
// issue #11276
// issue #11276
// issue #11276
// whitespace
// issue #11275
// issue #11275
// issue #11275
// issue #11275
// issue #11275
// issue #11275
// issue #11275
// erroneous programs
// build comments
// Test cases that are expected to fail are marked by the prefix "ERROR".
// The formatted result must look the same as the input for successful tests.
internal static slice<@string> tests = new @string[]{
    @"import ""go/format"""u8,
    "var x int"u8,
    "var x int\n\ntype T struct{}"u8,
    "x := 0"u8,
    "f(a, b, c)\nvar x int = f(1, 2, 3)"u8,
    "\tx := 0\n\tgo f()"u8,
    "\tx := 0\n\tgo f()\n\n\n"u8,
    "\n\t\t\n\n\tx := 0\n\tgo f()\n\n\n"u8,
    "\n\t\t\n\n\t\t\tx := 0\n\t\t\tgo f()\n\n\n"u8,
    "\n\t\t\n\n\t\t\tx := 0\n\t\t\tconst s = `\nfoo\n`\n\n\n"u8,
    "\n\t\t\n\n\t\t\tx := 0\n\t\t\tconst s = `\n\t\tfoo\n`\n\n\n"u8,
    "/* Comment */"u8,
    "\t/* Comment */ "u8,
    "\n/* Comment */ "u8,
    "i := 5 /* Comment */"u8,
    "\ta()\n//line :1"u8,
    "\t//xxx\n\ta()\n//line :2"u8,
    "\ta() //line :1\n\tb()\n"u8,
    "x := 0\n//line :1\n//line :2"u8,
    ""u8,
    " "u8,
    "\t"u8,
    "\t\t"u8,
    "\n"u8,
    "\n\n"u8,
    "\t\n"u8,
    "ERROR1 + 2 +"u8,
    "ERRORx :=  0"u8,
    "// copyright\n\n//go:build x\n\npackage p\n"u8,
    "// copyright\n\n//go:build x\n// +build x\n\npackage p\n"u8
}.slice();

public static (@string, error) String(@string s) {
    var (res, err) = Source(slice<byte>(s));
    if (err != default!) {
        return ("", err);
    }
    return (((@string)res), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string errorˢ = "ERROR"u8;

public static void TestPartial(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in tests) {
        var src = vᴛ1;

        if (strings.HasPrefix(src, errorˢ)){
            // test expected to fail
            src = src[5..]; // remove ERROR prefix
            var (res, err) = String(src);
            if (err == default! && res == src) {
                Ꮡt.Errorf("formatting succeeded but was expected to fail:\n%q"u8, src);
            }
        } else {
            // test expected to succeed
            var (res, err) = String(src);
            if (err != default!){
                Ꮡt.Errorf("formatting failed (%s):\n%q"u8, err, src);
            } else 
            if (res != src) {
                Ꮡt.Errorf("formatting incorrect:\nsource: %q\nresult: %q"u8, src, res);
            }
        }
    }
}

} // end format_internal_test_package
