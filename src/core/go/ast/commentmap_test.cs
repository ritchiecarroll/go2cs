// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// To avoid a cyclic dependency with go/parser, this file is in a separate package.
namespace go.go;

using fmt = fmt_package;
using static global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using sort = sort_package;
using strings = strings_package;
using testing = testing_package;
using ast = global::go.go.ast_package;
using global::go.go;
using static global::go.go.ast_internal_test_package;

partial class ast_test_package {

internal static readonly @string src = """

// the very first comment

// package p
package p /* the name is p */

// imports
import (
	"bytes"     // bytes
	"fmt"       // fmt
	"go/ast"
	"go/parser"
)

// T
type T struct {
	a, b, c int // associated with a, b, c
	// associated with x, y
	x, y float64    // float values
	z    complex128 // complex value
}
// also associated with T

// x
var x = 0 // x = 0
// also associated with x

// f1
func f1() {
	/* associated with s1 */
	s1()
	// also associated with s1
	
	// associated with s2
	
	// also associated with s2
	s2() // line comment for s2
}
// associated with f1
// also associated with f1

// associated with f2

// f2
func f2() {
}

func f3() {
	i := 1 /* 1 */ + 2 // addition
	_ = i
}

// the very last comment

"""u8;

// res maps a key of the form "line number: node type"
// to the associated comments' text.
internal static map<@string, @string> res = new map<@string, @string>{
    [" 5: *ast.File"u8] = "the very first comment\npackage p\n"u8,
    [" 5: *ast.Ident"u8] = " the name is p\n"u8,
    [" 8: *ast.GenDecl"u8] = "imports\n"u8,
    [" 9: *ast.ImportSpec"u8] = "bytes\n"u8,
    ["10: *ast.ImportSpec"u8] = "fmt\n"u8,
    ["16: *ast.GenDecl"u8] = "T\nalso associated with T\n"u8,
    ["17: *ast.Field"u8] = "associated with a, b, c\n"u8,
    ["19: *ast.Field"u8] = "associated with x, y\nfloat values\n"u8,
    ["20: *ast.Field"u8] = "complex value\n"u8,
    ["25: *ast.GenDecl"u8] = "x\nx = 0\nalso associated with x\n"u8,
    ["29: *ast.FuncDecl"u8] = "f1\nassociated with f1\nalso associated with f1\n"u8,
    ["31: *ast.ExprStmt"u8] = " associated with s1\nalso associated with s1\n"u8,
    ["37: *ast.ExprStmt"u8] = "associated with s2\nalso associated with s2\nline comment for s2\n"u8,
    ["45: *ast.FuncDecl"u8] = "associated with f2\nf2\n"u8,
    ["49: *ast.AssignStmt"u8] = "addition\n"u8,
    ["49: *ast.BasicLit"u8] = " 1\n"u8,
    ["50: *ast.Ident"u8] = "the very last comment\n"u8
};

internal static @string ctext(slice<ж<ast.CommentGroup>> list) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    foreach (var (_, g) in list) {
        Ꮡbuf.WriteString(g.Text());
    }
    return buf.String();
}

public static void TestCommentMap(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, ""u8, src, parser.ParseComments);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var cmap = NewCommentMap(fset, new ast.FileжNode(f), (~f).Comments);
    // very correct association of comments
    foreach (var (n, list) in cmap) {
        @string key = fmt.Sprintf("%2d: %T"u8, fset.Position(n.Pos()).Line, n);
        @string got = ctext(list);
        @string want = res[key];
        if (got != want) {
            Ꮡt.Errorf("%s: got %q; want %q"u8, key, got, want);
        }
    }
    // verify that no comments got lost
    {
        nint n = len(cmap.Comments()); if (n != len((~f).Comments)) {
            Ꮡt.Errorf("got %d comment groups in map; want %d"u8, n, len((~f).Comments));
        }
    }
    // support code to update test:
    // set genMap to true to generate res map
    const bool genMap = false;
    if (genMap) {
        var @out = new slice<@string>(0, len(cmap));
        foreach (var (n, list) in cmap) {
            @out = append(@out, fmt.Sprintf("\t\"%2d: %T\":\t%q,"u8, fset.Position(n.Pos()).Line, n, ctext(list)));
        }
        sort.Strings(@out);
        foreach (var (_, s) in @out) {
            fmt.Println(s);
        }
    }
}

public static void TestFilter(ж<testing.T> Ꮡt) {
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, ""u8, src, parser.ParseComments);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var cmap = NewCommentMap(fset, new ast.FileжNode(f), (~f).Comments);
    // delete variable declaration
    foreach (var (i, decl) in (~f).Decls) {
        {
            var (gen, ok) = decl._<ж<ast.GenDecl>>(ᐧ); if (ok && (~gen).Tok == token.VAR) {
                copy((~f).Decls.slice(i), (~f).Decls.slice(i + 1));
                f.Value.Decls = (~f).Decls.slice(0, len((~f).Decls) - 1);
                break;
            }
        }
    }
    // check if comments are filtered correctly
    var cc = cmap.Filter(new ast.FileжNode(f));
    foreach (var (n, list) in cc) {
        @string key = fmt.Sprintf("%2d: %T"u8, fset.Position(n.Pos()).Line, n);
        @string got = ctext(list);
        @string want = res[key];
        if (key == "25: *ast.GenDecl"u8 || got != want) {
            Ꮡt.Errorf("%s: got %q; want %q"u8, key, got, want);
        }
    }
}

} // end ast_test_package
