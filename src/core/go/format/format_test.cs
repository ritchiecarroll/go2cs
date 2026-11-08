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

internal static ref readonly @string testfile => ref testfileᵇ;
private static readonly @string testfileᵇ = "format_test.go"u8;

internal static void diff(ж<testing.T> t, slice<byte> dst, slice<byte> src) {
    nint line = 1;
    nint offs = 0; // line offset
    for (nint i = 0; i < len(dst) && i < len(src); i++) {
        var d = dst[i];
        var s = src[i];
        if (d != s) {
            t.Errorf("dst:%d: %s\n", line, dst.slice(offs, i + 1));
            t.Errorf("src:%d: %s\n", line, src.slice(offs, i + 1));
            return;
        }
        if (s == '\n') {
            line++;
            offs = i + 1;
        }
    }
    if (len(dst) != len(src)) {
        t.Errorf("len(dst) = %d, len(src) = %d\nsrc = %q", len(dst), len(src), src);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object nodeFailedˢ = (@string)"Node failed:"u8;

public static void TestNode(ж<testing.T> t) {
    var (src, err) = os.ReadFile(testfile);
    if (err != nil) {
        t.Fatal(err);
    }

    var fset = token.NewFileSet();
    (var @file, err) = parser.ParseFile(fset, testfile, src, parser.ParseComments);
    if (err != nil) {
        t.Fatal(err);
    }

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);

    err = Node(new format_test_package.bytes_BufferжWriter(Ꮡbuf), fset, @file.OrTypedNil()); if (err != nil) {
        t.Fatal(nodeFailedˢ, err);
    }

    diff(t, buf.Bytes(), src);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ => ref srcᶜᵇ;
private static readonly @string srcᶜᵇ = "package p\n\nconst _ = 0000000123i\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string goldenᶜ => ref goldenᶜᵇ;
private static readonly @string goldenᶜᵇ = "package p\n\nconst _ = 123i\n"u8;

// Node is documented to not modify the AST.
// Test that it is so even when numbers are normalized.
public static void TestNodeNoModify(ж<testing.T> t) {
    @string src = srcᶜ;
    @string golden = goldenᶜ;

    var fset = token.NewFileSet();
    var (@file, err) = parser.ParseFile(fset, ""u8, src, parser.ParseComments);
    if (err != nil) {
        t.Fatal(err);
    }

    // Capture original address and value of a BasicLit node
    // which will undergo formatting changes during printing.
    var wantLit = @file.Decls[0]._<ж<ast.GenDecl>>().Specs[0]._<ж<ast.ValueSpec>>().Values[0]._<ж<ast.BasicLit>>();
    @string wantVal = wantLit.Value.Value;

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    err = Node(new format_test_package.bytes_BufferжWriter(Ꮡbuf), fset, @file.OrTypedNil()); if (err != nil) {
        t.Fatal(nodeFailedˢ, err);
    }
    diff(t, buf.Bytes(), slice<byte>(golden));

    // Check if anything changed after Node returned.
    var gotLit = @file.Decls[0]._<ж<ast.GenDecl>>().Specs[0]._<ж<ast.ValueSpec>>().Values[0]._<ж<ast.BasicLit>>();
    @string gotVal = gotLit.Value.Value;

    if (gotLit != wantLit) {
        t.Errorf("got *ast.BasicLit address %p, want %p", gotLit.OrTypedNil(), wantLit.OrTypedNil());
    }
    if (gotVal != wantVal) {
        t.Errorf("got *ast.BasicLit value %q, want %q", gotVal, wantVal);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object sourceFailedˢ = (@string)"Source failed:"u8;

public static void TestSource(ж<testing.T> t) {
    var (src, err) = os.ReadFile(testfile);
    if (err != nil) {
        t.Fatal(err);
    }

    (var res, err) = Source(src);
    if (err != nil) {
        t.Fatal(sourceFailedˢ, err);
    }

    diff(t, res, src);
}

// Test cases that are expected to fail are marked by the prefix "ERROR".
// The formatted result must look the same as the input for successful tests.
internal static slice<@string> tests = [
    // declaration lists
    @"import ""go/format""",
    "var x int",
    "var x int\n\ntype T struct{}",

    // statement lists
    "x := 0",
    "f(a, b, c)\nvar x int = f(1, 2, 3)",

    // indentation, leading and trailing space
    "\tx := 0\n\tgo f()",
    "\tx := 0\n\tgo f()\n\n\n",
    "\n\t\t\n\n\tx := 0\n\tgo f()\n\n\n",
    "\n\t\t\n\n\t\t\tx := 0\n\t\t\tgo f()\n\n\n",
    "\n\t\t\n\n\t\t\tx := 0\n\t\t\tconst s = `\nfoo\n`\n\n\n",     // no indentation added inside raw strings
    "\n\t\t\n\n\t\t\tx := 0\n\t\t\tconst s = `\n\t\tfoo\n`\n\n\n", // no indentation removed inside raw strings

    // comments
    "/* Comment */",
    "\t/* Comment */ ",
    "\n/* Comment */ ",
    "i := 5 /* Comment */",         // issue #5551
    "\ta()\n//line :1",             // issue #11276
    "\t//xxx\n\ta()\n//line :2",    // issue #11276
    "\ta() //line :1\n\tb()\n",     // issue #11276
    "x := 0\n//line :1\n//line :2", // issue #11276

    // whitespace
    "",     // issue #11275
    " ",    // issue #11275
    "\t",   // issue #11275
    "\t\t", // issue #11275
    "\n",   // issue #11275
    "\n\n", // issue #11275
    "\t\n", // issue #11275

    // erroneous programs
    "ERROR1 + 2 +",
    "ERRORx :=  0",

    // build comments
    "// copyright\n\n//go:build x\n\npackage p\n",
    "// copyright\n\n//go:build x\n// +build x\n\npackage p\n"
];

public static (@string, error) String(@string s) {
    var (res, err) = Source(slice<byte>(s));
    if (err != nil) {
        return ("", err);
    }
    return ((@string)res, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string errorˢ => ref errorˢᵇ;
private static readonly @string errorˢᵇ = "ERROR"u8;

public static void TestPartial(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(tests)) {
        var src = vᴛ1;

        if (strings.HasPrefix(src, errorˢ)) {
            // test expected to fail
            src = src[5..]; // remove ERROR prefix
            var (res, err) = String(src);
            if (err == nil && res == src) {
                t.Errorf("formatting succeeded but was expected to fail:\n%q", src);
            }
        } else {
            // test expected to succeed
            var (res, err) = String(src);
            if (err != nil) {
                t.Errorf("formatting failed (%s):\n%q", err, src);
            } else if (res != src) {
                t.Errorf("formatting incorrect:\nsource: %q\nresult: %q", src, res);
            }
        }
    }
}

} // end format_internal_test_package
