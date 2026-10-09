// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using fs = global::go.io.fs_package;
using strings = strings_package;
using testing = testing_package;
using global::go.go;
using global::go.io;
using static global::go.go.parser_package;

partial class parser_internal_test_package {

internal static slice<@string> validFiles = new @string[]{
    "parser.go"u8,
    "parser_test.go"u8,
    "error_test.go"u8,
    "short_test.go"u8
}.slice();

public static void TestParse(ж<testing.T> Ꮡt) {
    foreach (var (_, filename) in validFiles) {
        var (_, err) = ParseFile(token.NewFileSet(), filename, default!, DeclarationErrors);
        if (err != default!) {
            Ꮡt.Fatalf("ParseFile(%s): %v"u8, filename, err);
        }
    }
}

internal static bool nameFilter(@string filename) {
    var exprᴛ1 = filename;
    if (exprᴛ1 == "parser.go"u8 || exprᴛ1 == "interface.go"u8 || exprᴛ1 == "parser_test.go"u8) {
        return true;
    }
    if (exprᴛ1 == "parser.go.orig"u8) {
        return true; // permit but should be ignored by ParseDir
    }

    return false;
}

internal static bool dirFilter(fs.FileInfo f) {
    return nameFilter(f.Name());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packagePVarSSSSSSSSSSSSˢ = "package p\nvar _=s[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]"u8;

public static void TestParseFile(ж<testing.T> Ꮡt) {
    @string src = packagePVarSSSSSSSSSSSSˢ;
    var (_, err) = ParseFile(token.NewFileSet(), ""u8, src, 0);
    if (err == default!) {
        Ꮡt.Errorf("ParseFile(%s) succeeded unexpectedly"u8, src);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sSSSSSSSSSSSˢ = "s[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]"u8;

public static void TestParseExprFrom(ж<testing.T> Ꮡt) {
    @string src = sSSSSSSSSSSSˢ;
    var (_, err) = ParseExprFrom(token.NewFileSet(), ""u8, src, 0);
    if (err == default!) {
        Ꮡt.Errorf("ParseExprFrom(%s) succeeded unexpectedly"u8, src);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string parserˢ = "parser"u8;

public static void TestParseDir(ж<testing.T> Ꮡt) {
    @string path = "."u8;
    var (pkgs, err) = ParseDir(token.NewFileSet(), path, dirFilter, 0);
    if (err != default!) {
        Ꮡt.Fatalf("ParseDir(%s): %v"u8, path, err);
    }
    {
        nint n = len(pkgs); if (n != 1) {
            Ꮡt.Errorf("got %d packages; want 1"u8, n);
        }
    }
    var pkg = pkgs[parserˢ];
    if (pkg == nil) {
        Ꮡt.Errorf(@"package ""parser"" not found"u8);
        return;
    }
    {
        nint n = len((~pkg).Files); if (n != 3) {
            Ꮡt.Errorf("got %d package files; want 3"u8, n);
        }
    }
    foreach (var (filename, _) in (~pkg).Files) {
        if (!nameFilter(filename)) {
            Ꮡt.Errorf("unexpected package file: %s"u8, filename);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataIssue42951ˢ = "./testdata/issue42951"u8;

public static void TestIssue42951(ж<testing.T> Ꮡt) {
    @string path = testdataIssue42951ˢ;
    var (_, err) = ParseDir(token.NewFileSet(), path, default!, 0);
    if (err != default!) {
        Ꮡt.Errorf("ParseDir(%s): %v"u8, path, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string structXIntˢ = "struct{x *int}"u8;
internal static readonly @string aIXˢ = "a[i] := x"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string validExprᶜ = "a + b"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string anythingᶜ = "dh3*#D)#_"u8;

public static void TestParseExpr(ж<testing.T> Ꮡt) {
    // just kicking the tires:
    // a valid arithmetic expression
    @string src = "a + b"u8;
    var (x, err) = ParseExpr(src);
    if (err != default!) {
        Ꮡt.Errorf("ParseExpr(%q): %v"u8, src, err);
    }
    // sanity check
    {
        var (_, ok) = x._<ж<ast.BinaryExpr>>(ᐧ); if (!ok) {
            Ꮡt.Errorf("ParseExpr(%q): got %T, want *ast.BinaryExpr"u8, src, x);
        }
    }
    // a valid type expression
    src = structXIntˢ;
    (x, err) = ParseExpr(src);
    if (err != default!) {
        Ꮡt.Errorf("ParseExpr(%q): %v"u8, src, err);
    }
    // sanity check
    {
        var (_, ok) = x._<ж<ast.StructType>>(ᐧ); if (!ok) {
            Ꮡt.Errorf("ParseExpr(%q): got %T, want *ast.StructType"u8, src, x);
        }
    }
    // an invalid expression
    src = "a + *"u8;
    (x, err) = ParseExpr(src);
    if (err == default!) {
        Ꮡt.Errorf("ParseExpr(%q): got no error"u8, src);
    }
    if (x == default!) {
        Ꮡt.Errorf("ParseExpr(%q): got no (partial) result"u8, src);
    }
    {
        var (_, ok) = x._<ж<ast.BinaryExpr>>(ᐧ); if (!ok) {
            Ꮡt.Errorf("ParseExpr(%q): got %T, want *ast.BinaryExpr"u8, src, x);
        }
    }
    // a valid expression followed by extra tokens is invalid
    src = aIXˢ;
    {
        var (_, errΔ1) = ParseExpr(src); if (errΔ1 == default!) {
            Ꮡt.Errorf("ParseExpr(%q): got no error"u8, src);
        }
    }
    // a semicolon is not permitted unless automatically inserted
    src = "a + b\n"u8;
    {
        var (_, errΔ2) = ParseExpr(src); if (errΔ2 != default!) {
            Ꮡt.Errorf("ParseExpr(%q): got error %s"u8, src, errΔ2);
        }
    }
    src = "a + b;"u8;
    {
        var (_, errΔ3) = ParseExpr(src); if (errΔ3 == default!) {
            Ꮡt.Errorf("ParseExpr(%q): got no error"u8, src);
        }
    }
    // various other stuff following a valid expression
    @string validExpr = validExprᶜ;
    @string anything = anythingᶜ;
    foreach (var (_, c) in (@string)"!)]};,"u8) {
        @string srcΔ1 = validExpr + ((@string)c) + anything;
        {
            var (_, errΔ4) = ParseExpr(srcΔ1); if (errΔ4 == default!) {
                Ꮡt.Errorf("ParseExpr(%q): got no error"u8, srcΔ1);
            }
        }
    }
    // ParseExpr must not crash
    foreach (var (_, srcΔ2) in valids) {
        ParseExpr(srcΔ2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object packagePFuncFXYZXYZˢ = (@string)@"package p; func f() { x, y, z := x, y, z }"u8;

public static void TestColonEqualsScope(ж<testing.T> Ꮡt) {
    var (f, err) = ParseFile(token.NewFileSet(), ""u8, packagePFuncFXYZXYZˢ, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // RHS refers to undefined globals; LHS does not.
    var @as = (~(~(~f).Decls[0]._<ж<ast.FuncDecl>>()).Body).List[0]._<ж<ast.AssignStmt>>();
    foreach (var (_, v) in (~@as).Rhs) {
        var id = v._<ж<ast.Ident>>();
        if ((~id).Obj != nil) {
            Ꮡt.Errorf("rhs %s has Obj, should not"u8, (~id).Name);
        }
    }
    foreach (var (_, v) in (~@as).Lhs) {
        var id = v._<ж<ast.Ident>>();
        if ((~id).Obj == nil) {
            Ꮡt.Errorf("lhs %s does not have Obj, should"u8, (~id).Name);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object packagePFuncFVarXYZXYZˢ = (@string)@"package p; func f() { var x, y, z = x, y, z }"u8;

public static void TestVarScope(ж<testing.T> Ꮡt) {
    var (f, err) = ParseFile(token.NewFileSet(), ""u8, packagePFuncFVarXYZXYZˢ, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // RHS refers to undefined globals; LHS does not.
    var @as = (~(~(~(~(~f).Decls[0]._<ж<ast.FuncDecl>>()).Body).List[0]._<ж<ast.DeclStmt>>()).Decl._<ж<ast.GenDecl>>()).Specs[0]._<ж<ast.ValueSpec>>();
    foreach (var (_, v) in (~@as).Values) {
        var id = v._<ж<ast.Ident>>();
        if ((~id).Obj != nil) {
            Ꮡt.Errorf("rhs %s has Obj, should not"u8, (~id).Name);
        }
    }
    foreach (var (_, id) in (~@as).Names) {
        if ((~id).Obj == nil) {
            Ꮡt.Errorf("lhs %s does not have Obj, should"u8, (~id).Name);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ = """

package p
import fmt "fmt"
const pi = 3.14
type T struct{}
var x int
func f() { L: }

"""u8;

public static void TestObjects(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string src = srcᶜ;
    var (f, err) = ParseFile(token.NewFileSet(), ""u8, src, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var objects = new map<@string, ast.ObjKind>{
        ["p"u8] = ast.Bad, // not in a scope

        ["fmt"u8] = ast.Bad, // not resolved yet

        ["pi"u8] = ast.Con,
        ["T"u8] = ast.Typ,
        ["x"u8] = ast.Var,
        ["int"u8] = ast.Bad, // not resolved yet

        ["f"u8] = ast.Fun,
        ["L"u8] = ast.Lbl
    };
    var objectsʗ1 = objects;
    ast.Inspect(new ast.FileжNode(f), (ast.Node n) => {
        {
            var (ident, ok) = n._<ж<ast.Ident>>(ᐧ); if (ok) {
                var obj = ident.Value.Obj;
                if (obj == nil) {
                    if (objectsʗ1[(~ident).Name] != ast.Bad) {
                        Ꮡt.Errorf("no object for %s"u8, (~ident).Name);
                    }
                    return true;
                }
                if ((~obj).Name != (~ident).Name) {
                    Ꮡt.Errorf("names don't match: obj.Name = %s, ident.Name = %s"u8, (~obj).Name, (~ident).Name);
                }
                ast.ObjKind kind = objectsʗ1[(~ident).Name];
                if ((~obj).Kind != kind) {
                    Ꮡt.Errorf("%s: obj.Kind = %s; want %s"u8, (~ident).Name, (~obj).Kind, kind);
                }
            }
        }
        return true;
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object packagePFuncF1aIntFuncˢ = (@string)"""

package p
//
func f1a(int)
func f2a(byte, int, float)
func f3a(a, b int, c float)
func f4a(...complex)
func f5a(a s1a, b ...complex)
//
func f1b(*int)
func f2b([]byte, (int), *float)
func f3b(a, b *int, c []float)
func f4b(...*complex)
func f5b(a s1a, b ...[]complex)
//
type s1a struct { int }
type s2a struct { byte; int; s1a }
type s3a struct { a, b int; c float }
//
type s1b struct { *int }
type s2b struct { byte; int; *float }
type s3b struct { a, b *s3b; c []float }

"""u8;

public static void TestUnresolved(ж<testing.T> Ꮡt) {
    var (f, err) = ParseFile(token.NewFileSet(), ""u8, packagePFuncF1aIntFuncˢ, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string want = "int "u8 + "byte int float "u8 + "int float "u8 + "complex "u8 + "complex "u8 + "int "u8 + "byte int float "u8 + "int float "u8 + "complex "u8 + "complex "u8 + "int "u8 + "byte int "u8 + "int float "u8 + "int "u8 + "byte int float "u8 + "float "u8; // s3a
    // f1a
    // f2a
    // f3a
    // f4a
    // f5a
    //
    // f1b
    // f2b
    // f3b
    // f4b
    // f5b
    //
    // s1a
    // s2a
    // s3a
    //
    // s1a
    // s2a
    // collect unresolved identifiers
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    foreach (var (_, u) in (~f).Unresolved) {
        Ꮡbuf.WriteString((~u).Name);
        Ꮡbuf.WriteByte((rune)' ');
    }
    @string got = buf.String();
    if (got != want) {
        Ꮡt.Errorf("\ngot:  %s\nwant: %s"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object packageP1a1b1c1d2a2bˢ = (@string)"""

package p /* 1a */ /* 1b */      /* 1c */ // 1d
/* 2a
*/
// 2b
const pi = 3.1415
/* 3a */ // 3b
/* 3c */ const e = 2.7182

// Example from go.dev/issue/3139
func ExampleCount() {
	fmt.Println(strings.Count("cheese", "e"))
	fmt.Println(strings.Count("five", "")) // before & after each rune
	// Output:
	// 3
	// 5
}

"""u8;

public static void TestCommentGroups(ж<testing.T> Ꮡt) {
    var (f, err) = ParseFile(token.NewFileSet(), ""u8, packageP1a1b1c1d2a2bˢ, ParseComments);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var expected = new slice<@string>[]{
        new @string[]{"/* 1a */"u8, "/* 1b */"u8, "/* 1c */"u8, "// 1d"u8}.slice(),
        new @string[]{"/* 2a\n*/"u8, "// 2b"u8}.slice(),
        new @string[]{"/* 3a */"u8, "// 3b"u8, "/* 3c */"u8}.slice(),
        new @string[]{"// Example from go.dev/issue/3139"u8}.slice(),
        new @string[]{"// before & after each rune"u8}.slice(),
        new @string[]{"// Output:"u8, "// 3"u8, "// 5"u8}.slice()
    }.slice();
    if (len((~f).Comments) != len(expected)) {
        Ꮡt.Fatalf("got %d comment groups; expected %d"u8, len((~f).Comments), len(expected));
    }
    foreach (var (i, exp) in expected) {
        var got = (~f).Comments[i].Value.List;
        if (len(got) != len(exp)) {
            Ꮡt.Errorf("got %d comments in group %d; expected %d"u8, len(got), i, len(exp));
            continue;
        }
        foreach (var (j, expΔ1) in exp) {
            @string gotΔ1 = got[j].Value.Text;
            if (gotΔ1 != expΔ1) {
                Ꮡt.Errorf("got %q in group %d; expected %q"u8, gotΔ1, i, expΔ1);
            }
        }
    }
}

internal static ж<ast.Field> getField(ж<ast.File> Ꮡfile, @string fieldname) {
    ref var @file = ref Ꮡfile.DerefOrNull();

    var parts = strings.Split(fieldname, "."u8);
    foreach (var (_, d) in @file.Decls) {
        {
            var (dΔ1, ok) = d._<ж<ast.GenDecl>>(ᐧ); if (ok && (~dΔ1).Tok == token.TYPE) {
                foreach (var (_, s) in (~dΔ1).Specs) {
                    {
                        var (sΔ1, okΔ1) = s._<ж<ast.TypeSpec>>(ᐧ); if (okΔ1 && (~(~sΔ1).Name).Name == parts[0]) {
                            {
                                var (sΔ2, okΔ2) = (~sΔ1).Type._<ж<ast.StructType>>(ᐧ); if (okΔ2) {
                                    foreach (var (_, f) in (~(~sΔ2).Fields).List) {
                                        foreach (var (_, name) in (~f).Names) {
                                            if ((~name).Name == parts[1]) {
                                                return f;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    return default!;
}

// Don't use ast.CommentGroup.Text() - we want to see exact comment text.
internal static @string commentText(ж<ast.CommentGroup> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    if (Ꮡc != nil) {
        foreach (var (_, cΔ1) in c.List) {
            Ꮡbuf.WriteString((~cΔ1).Text);
        }
    }
    return buf.String();
}

internal static void checkFieldComments(ж<testing.T> Ꮡt, ж<ast.File> Ꮡfile, @string fieldname, @string lead, @string line) {
    var f = getField(Ꮡfile, fieldname);
    if (f == nil) {
        Ꮡt.Fatalf("field not found: %s"u8, fieldname);
    }
    {
        @string got = commentText((~f).Doc); if (got != lead) {
            Ꮡt.Errorf("got lead comment %q; expected %q"u8, got, lead);
        }
    }
    {
        @string got = commentText((~f).Comment); if (got != line) {
            Ꮡt.Errorf("got line comment %q; expected %q"u8, got, line);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object packagePTypeTStructF1ˢ = (@string)"""

package p
type T struct {
	/* F1 lead comment */
	//
	F1 int  /* F1 */ // line comment
	// F2 lead
	// comment
	F2 int  // F2 line comment
	// f3 lead comment
	f3 int  // f3 line comment

	f4 int   /* not a line comment */ ;
        f5 int ; // f5 line comment
	f6 int ; /* f6 line comment */
	f7 int ; /*f7a*/ /*f7b*/ //f7c
}

"""u8;
internal static readonly @string tF1ˢ = "T.F1"u8;
internal static readonly @string f1LeadCommentˢ = "/* F1 lead comment *///"u8;
internal static readonly @string f1LineCommentˢ = "/* F1 */// line comment"u8;
internal static readonly @string tF2ˢ = "T.F2"u8;
internal static readonly @string f2LeadCommentˢ = "// F2 lead// comment"u8;
internal static readonly @string f2LineCommentˢ = "// F2 line comment"u8;
internal static readonly @string tF3ˢ = "T.f3"u8;
internal static readonly @string f3LeadCommentˢ = "// f3 lead comment"u8;
internal static readonly @string f3LineCommentˢ = "// f3 line comment"u8;
internal static readonly @string tF4ˢ = "T.f4"u8;
internal static readonly @string tF5ˢ = "T.f5"u8;
internal static readonly @string f5LineCommentˢ = "// f5 line comment"u8;
internal static readonly @string tF6ˢ = "T.f6"u8;
internal static readonly @string f6LineCommentˢ = "/* f6 line comment */"u8;
internal static readonly @string tF7ˢ = "T.f7"u8;
internal static readonly @string f7aF7bF7cˢ = "/*f7a*//*f7b*///f7c"u8;
internal static readonly object notExpectedToFindTF3ˢ = (@string)"not expected to find T.f3"u8;

public static void TestLeadAndLineComments(ж<testing.T> Ꮡt) {
    var (f, err) = ParseFile(token.NewFileSet(), ""u8, packagePTypeTStructF1ˢ, ParseComments);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    checkFieldComments(Ꮡt, f, tF1ˢ, f1LeadCommentˢ, f1LineCommentˢ);
    checkFieldComments(Ꮡt, f, tF2ˢ, f2LeadCommentˢ, f2LineCommentˢ);
    checkFieldComments(Ꮡt, f, tF3ˢ, f3LeadCommentˢ, f3LineCommentˢ);
    checkFieldComments(Ꮡt, f, tF4ˢ, ""u8, ""u8);
    checkFieldComments(Ꮡt, f, tF5ˢ, ""u8, f5LineCommentˢ);
    checkFieldComments(Ꮡt, f, tF6ˢ, ""u8, f6LineCommentˢ);
    checkFieldComments(Ꮡt, f, tF7ˢ, ""u8, f7aF7bF7cˢ);
    ast.FileExports(f);
    checkFieldComments(Ꮡt, f, tF1ˢ, f1LeadCommentˢ, f1LineCommentˢ);
    checkFieldComments(Ꮡt, f, tF2ˢ, f2LeadCommentˢ, f2LineCommentˢ);
    if (getField(f, tF3ˢ) != nil) {
        Ꮡt.Error(notExpectedToFindTF3ˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string wantˢ = "want ';'"u8;
internal static readonly @string butIsImplicitˢ = "but ';' is implicit"u8;

// TestIssue9979 verifies that empty statements are contained within their enclosing blocks.
public static void TestIssue9979(ж<testing.T> Ꮡt) {
    foreach (var (_, src) in new @string[]{
        "package p; func f() {;}"u8,
        "package p; func f() {L:}"u8,
        "package p; func f() {L:;}"u8,
        "package p; func f() {L:\n}"u8,
        "package p; func f() {L:\n;}"u8,
        "package p; func f() { ; }"u8,
        "package p; func f() { L: }"u8,
        "package p; func f() { L: ; }"u8,
        "package p; func f() { L: \n}"u8,
        "package p; func f() { L: \n; }"u8
    }.slice()) {
        var fset = token.NewFileSet();
        var (f, err) = ParseFile(fset, ""u8, src, 0);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        tokenꓸPos pos = default!;
        tokenꓸPos end = default!;
        var fsetʗ1 = fset;
        ast.Inspect(new ast.FileжNode(f), (ast.Node x) => {
            switch (x.type()) {
            case ж<ast.BlockStmt> s: {
                (pos, end) = (s.Pos() + 1, s.End() - 1); // exclude "{", "}"
                break;
            }
            case ж<ast.LabeledStmt> s: {
                (pos, end) = (s.Pos() + 2, s.End()); // exclude "L:"
                break;
            }
            case ж<ast.EmptyStmt> s: {
                if (s.Pos() < pos || s.End() > end) {
                    // check containment
                    Ꮡt.Errorf("%s: %T[%d, %d] not inside [%d, %d]"u8, src, s.OrTypedNil(), s.Pos(), s.End(), pos, end);
                }
                nint offs = fsetʗ1.Position(s.Pos()).Offset;
                {
                    var ch = src[offs]; if (ch != (rune)';' != (~s).Implicit) {
                        // check semicolon
                        @string want = wantˢ;
                        if ((~s).Implicit) {
                            want = butIsImplicitˢ;
                        }
                        Ꮡt.Errorf("%s: found %q at offset %d; %s"u8, src, ch, offs, want);
                    }
                }
                break;
            }}
            return true;
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileGoˢ = "file.go"u8;
internal static readonly @string fileGo11ˢ = "file.go:1:1"u8;
internal static readonly @string fileGo1019ˢ = "file.go:10:19"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ1 = """
// Copyright

//+build tag

// Package p doc comment.
package p

var lastDecl int

/* end of file */

"""u8;

public static void TestFileStartEndPos(ж<testing.T> Ꮡt) {
    @string src = srcᶜ1;
    var fset = token.NewFileSet();
    var (f, err) = ParseFile(fset, fileGoˢ, src, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // File{Start,End} spans the entire file, not just the declarations.
    {
        @string got = fset.Position((~f).FileStart).String();
        @string want = fileGo11ˢ; if (got != want) {
            Ꮡt.Errorf("for File.FileStart, got %s, want %s"u8, got, want);
        }
    }
    // The end position is the newline at the end of the /* end of file */ line.
    {
        @string got = fset.Position((~f).FileEnd).String();
        @string want = fileGo1019ˢ; if (got != want) {
            Ꮡt.Errorf("for File.FileEnd, got %s, want %s"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object foundNoAstSelectorExprˢ = (@string)"found no *ast.SelectorExpr"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantErrᶜ = "expected selector or type assertion"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantSelᶜ = "&{fmt _}"u8;

// TestIncompleteSelection ensures that an incomplete selector
// expression is parsed as a (blank) *ast.SelectorExpr, not a
// *ast.BadExpr.
public static void TestIncompleteSelection(ж<testing.T> Ꮡt) {
    foreach (var (_, src) in new @string[]{
        "package p; var _ = fmt."u8, // at EOF

        "package p; var _ = fmt.\ntype X int"u8
    }.slice()) {
        // not at EOF
        var fset = token.NewFileSet();
        var (f, err) = ParseFile(fset, ""u8, src, 0);
        if (err == default!) {
            Ꮡt.Errorf("ParseFile(%s) succeeded unexpectedly"u8, src);
            continue;
        }
        @string wantErr = wantErrᶜ;
        if (!strings.Contains(err.Error(), wantErr)) {
            Ꮡt.Errorf("ParseFile returned wrong error %q, want %q"u8, err, wantErr);
        }
        ref var sel = ref heap<ж<ast.SelectorExpr>>(out var Ꮡsel);
        ast.Inspect(new ast.FileжNode(f), (ast.Node n) => {
            {
                var (nΔ1, ok) = n._<ж<ast.SelectorExpr>>(ᐧ); if (ok) {
                    Ꮡsel.ValueSlot = nΔ1;
                }
            }
            return true;
        });
        if (sel == nil) {
            Ꮡt.Error(foundNoAstSelectorExprˢ);
            continue;
        }
        @string wantSel = wantSelᶜ;
        if (fmt.Sprint(sel.OrTypedNil()) != wantSel) {
            Ꮡt.Errorf("found selector %s, want %s"u8, sel.OrTypedNil(), wantSel);
            continue;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object commentˢ = (@string)"// comment"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ2 = """
package main
type x int // comment

"""u8;

public static void TestLastLineComment(ж<testing.T> Ꮡt) {
    @string src = srcᶜ2;
    var fset = token.NewFileSet();
    var (f, err) = ParseFile(fset, ""u8, src, ParseComments);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string comment = (~(~(~(~f).Decls[0]._<ж<ast.GenDecl>>()).Specs[0]._<ж<ast.TypeSpec>>()).Comment).List[0].Value.Text;
    if (comment != "// comment"u8) {
        Ꮡt.Errorf("got %q, want %q"u8, comment, commentˢ);
    }
}

// The format expands the part inside « » many times.
// A second set of brackets nested inside the first stops the repetition,
// so that for example «(«1»)» expands to (((...((((1))))...))).
// Scopes: InterfaceType, FuncType
// Parser nodes: UnaryExpr, CompositeLit
// Parser nodes: UnaryExpr, CompositeLit
// Parser nodes: UnaryExpr, CompositeLit
// Parser nodes: CompositeLit, KeyValueExpr
// Parser nodes: SelectorExpr, CallExpr
// Parser nodes: BinaryExpr, ParenExpr
// Parser nodes: Ident, CallExpr
// Parser nodes: ParenExpr, CallExpr
// Parser nodes: IfStmt, BlockStmt. Scopes: IfStmt, BlockStmt
// Scopes: TypeSwitchStmt, CaseClause
// Scopes: TypeSwitchStmt, CaseClause
// Scopes: ForStmt, BlockStmt
// Scopes: ForStmt, BlockStmt
// Scopes: ForStmt, BlockStmt
// Scopes: RangeStmt, BlockStmt
// Scopes: RangeStmt, BlockStmt
// Scopes: RangeStmt, BlockStmt
// Parser nodes: GoStmt, FuncLit
// Parser nodes: DeferStmt, FuncLit

partial struct parseDepthTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal @string format;
    // parseMultiplier is used when a single statement may result in more than one
    // change in the depth level, for instance "1+(..." produces a BinaryExpr
    // followed by a UnaryExpr, which increments the depth twice. The test
    // case comment explains which nodes are triggering the multiple depth
    // changes.
    internal nint parseMultiplier;
    // scope is true if we should also test the statement for the resolver scope
    // depth limit.
    internal bool scope;
    // scopeMultiplier does the same as parseMultiplier, but for the scope
    // depths.
    internal nint scopeMultiplier;
}
internal static slice<parseDepthTestsᴛ1> parseDepthTests = new parseDepthTestsᴛ1[]{
    new(name: "array"u8, format: "package main; var x «[1]»int"u8),
    new(name: "slice"u8, format: "package main; var x «[]»int"u8),
    new(name: "struct"u8, format: "package main; var x «struct { X «int» }»"u8, scope: true),
    new(name: "pointer"u8, format: "package main; var x «*»int"u8),
    new(name: "func"u8, format: "package main; var x «func()»int"u8, scope: true),
    new(name: "chan"u8, format: "package main; var x «chan »int"u8),
    new(name: "chan2"u8, format: "package main; var x «<-chan »int"u8),
    new(name: "interface"u8, format: "package main; var x «interface { M() «int» }»"u8, scope: true, scopeMultiplier: 2),
    new(name: "map"u8, format: "package main; var x «map[int]»int"u8),
    new(name: "slicelit"u8, format: "package main; var x = []any{«[]any{«»}»}"u8, parseMultiplier: 3),
    new(name: "arraylit"u8, format: "package main; var x = «[1]any{«nil»}»"u8, parseMultiplier: 3),
    new(name: "structlit"u8, format: "package main; var x = «struct{x any}{«nil»}»"u8, parseMultiplier: 3),
    new(name: "maplit"u8, format: "package main; var x = «map[int]any{1:«nil»}»"u8, parseMultiplier: 3),
    new(name: "element"u8, format: "package main; var x = struct{x any}{x: «{«»}»}"u8),
    new(name: "dot"u8, format: "package main; var x = «x.»x"u8),
    new(name: "index"u8, format: "package main; var x = x«[1]»"u8),
    new(name: "slice"u8, format: "package main; var x = x«[1:2]»"u8),
    new(name: "slice3"u8, format: "package main; var x = x«[1:2:3]»"u8),
    new(name: "dottype"u8, format: "package main; var x = x«.(any)»"u8),
    new(name: "callseq"u8, format: "package main; var x = x«()»"u8),
    new(name: "methseq"u8, format: "package main; var x = x«.m()»"u8, parseMultiplier: 2),
    new(name: "binary"u8, format: "package main; var x = «1+»1"u8),
    new(name: "binaryparen"u8, format: "package main; var x = «1+(«1»)»"u8, parseMultiplier: 2),
    new(name: "unary"u8, format: "package main; var x = «^»1"u8),
    new(name: "addr"u8, format: "package main; var x = «& »x"u8),
    new(name: "star"u8, format: "package main; var x = «*»x"u8),
    new(name: "recv"u8, format: "package main; var x = «<-»x"u8),
    new(name: "call"u8, format: "package main; var x = «f(«1»)»"u8, parseMultiplier: 2),
    new(name: "conv"u8, format: "package main; var x = «(*T)(«1»)»"u8, parseMultiplier: 2),
    new(name: "label"u8, format: "package main; func main() { «Label:» }"u8),
    new(name: "if"u8, format: "package main; func main() { «if true { «» }»}"u8, parseMultiplier: 2, scope: true, scopeMultiplier: 2),
    new(name: "ifelse"u8, format: "package main; func main() { «if true {} else » {} }"u8, scope: true),
    new(name: "switch"u8, format: "package main; func main() { «switch { default: «» }»}"u8, scope: true, scopeMultiplier: 2),
    new(name: "typeswitch"u8, format: "package main; func main() { «switch x.(type) { default: «» }» }"u8, scope: true, scopeMultiplier: 2),
    new(name: "for0"u8, format: "package main; func main() { «for { «» }» }"u8, scope: true, scopeMultiplier: 2),
    new(name: "for1"u8, format: "package main; func main() { «for x { «» }» }"u8, scope: true, scopeMultiplier: 2),
    new(name: "for3"u8, format: "package main; func main() { «for f(); g(); h() { «» }» }"u8, scope: true, scopeMultiplier: 2),
    new(name: "forrange0"u8, format: "package main; func main() { «for range x { «» }» }"u8, scope: true, scopeMultiplier: 2),
    new(name: "forrange1"u8, format: "package main; func main() { «for x = range z { «» }» }"u8, scope: true, scopeMultiplier: 2),
    new(name: "forrange2"u8, format: "package main; func main() { «for x, y = range z { «» }» }"u8, scope: true, scopeMultiplier: 2),
    new(name: "go"u8, format: "package main; func main() { «go func() { «» }()» }"u8, parseMultiplier: 2, scope: true),
    new(name: "defer"u8, format: "package main; func main() { «defer func() { «» }()» }"u8, parseMultiplier: 2, scope: true),
    new(name: "select"u8, format: "package main; func main() { «select { default: «» }» }"u8, scope: true)
}.slice();

// split splits pre«mid»post into pre, mid, post.
// If the string does not have that form, split returns x, "", "".
internal static (@string pre, @string mid, @string post) split(@string x) {
    nint start = strings.Index(x, "«"u8);
    nint end = strings.LastIndex(x, "»"u8);
    if (start < 0 || end < 0) {
        return (x, "", "");
    }
    return (x.slice(0, start), x.slice(start + len("«"), end), x.slice(end + len("»")));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testRequiresSignificantˢ = (@string)"test requires significant memory"u8;

public static void TestParseDepthLimit(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(testRequiresSignificantˢ);
    }
    foreach (var (_, vᴛ1) in parseDepthTests) {
        ref var tt = ref heap(new parseDepthTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        foreach (var (_, size) in new @string[]{"small"u8, "big"u8}.slice()) {
            var ttʗ1 = tt;
            Ꮡt.Run(tt.name + "/"u8 + size, (ж<testing.T> tΔ1) => {
                nint n = maxNestLev + 1;
                if (ttʗ1.parseMultiplier > 0) {
                    n = quo(n, ttʗ1.parseMultiplier);
                }
                if (size == "small"u8) {
                    // Decrease the number of statements by 10, in order to check
                    // that we do not fail when under the limit. 10 is used to
                    // provide some wiggle room for cases where the surrounding
                    // scaffolding syntax adds some noise to the depth that changes
                    // on a per testcase basis.
                    n -= 10;
                }
                var (pre, mid, post) = split(ttʗ1.format);
                if (strings.Contains(mid, "«"u8)){
                    var (left, @base, right) = split(mid);
                    mid = strings.Repeat(left, n) + @base + strings.Repeat(right, n);
                } else {
                    mid = strings.Repeat(mid, n);
                }
                @string input = pre + mid + post;
                var fset = token.NewFileSet();
                var (_, err) = ParseFile(fset, ""u8, input, (global::go.go.parser_package.Mode)(ParseComments | SkipObjectResolution));
                if (size == "small"u8){
                    if (err != default!) {
                        tΔ1.Errorf("ParseFile(...): %v (want success)"u8, err);
                    }
                } else {
                    @string expected = exceededMaxNestingDepthˢ;
                    if (err == default! || !strings.HasSuffix(err.Error(), expected)) {
                        tΔ1.Errorf("ParseFile(...) = _, %v, want %q"u8, err, expected);
                    }
                }
            });
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exceededMaxScopeDepthˢ = "exceeded max scope depth during object resolution"u8;

public static void TestScopeDepthLimit(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in parseDepthTests) {
        ref var tt = ref heap(new parseDepthTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        if (!tt.scope) {
            continue;
        }
        foreach (var (_, size) in new @string[]{"small"u8, "big"u8}.slice()) {
            var ttʗ1 = tt;
            Ꮡt.Run(tt.name + "/"u8 + size, (ж<testing.T> tΔ1) => {
                nint n = maxScopeDepth + 1;
                if (ttʗ1.scopeMultiplier > 0) {
                    n = quo(n, ttʗ1.scopeMultiplier);
                }
                if (size == "small"u8) {
                    // Decrease the number of statements by 10, in order to check
                    // that we do not fail when under the limit. 10 is used to
                    // provide some wiggle room for cases where the surrounding
                    // scaffolding syntax adds some noise to the depth that changes
                    // on a per testcase basis.
                    n -= 10;
                }
                var (pre, mid, post) = split(ttʗ1.format);
                if (strings.Contains(mid, "«"u8)){
                    var (left, @base, right) = split(mid);
                    mid = strings.Repeat(left, n) + @base + strings.Repeat(right, n);
                } else {
                    mid = strings.Repeat(mid, n);
                }
                @string input = pre + mid + post;
                var fset = token.NewFileSet();
                var (_, err) = ParseFile(fset, ""u8, input, DeclarationErrors);
                if (size == "small"u8){
                    if (err != default!) {
                        tΔ1.Errorf("ParseFile(...): %v (want success)"u8, err);
                    }
                } else {
                    @string expected = exceededMaxScopeDepthˢ;
                    if (err == default! || !strings.HasSuffix(err.Error(), expected)) {
                        tΔ1.Errorf("ParseFile(...) = _, %v, want %q"u8, err, expected);
                    }
                }
            });
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rangeˢ = "range"u8;

// proposal go.dev/issue/50429
public static void TestRangePos(ж<testing.T> Ꮡt) {
    var testcases = new @string[]{
        "package p; func _() { for range x {} }"u8,
        "package p; func _() { for i = range x {} }"u8,
        "package p; func _() { for i := range x {} }"u8,
        "package p; func _() { for k, v = range x {} }"u8,
        "package p; func _() { for k, v := range x {} }"u8
    }.slice();
    foreach (var (_, src) in testcases) {
        var fset = token.NewFileSet();
        var (f, err) = ParseFile(fset, src, src, 0);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var fsetʗ1 = fset;
        ast.Inspect(new ast.FileжNode(f), (ast.Node x) => {
            switch (x.type()) {
            case ж<ast.RangeStmt> s: {
                var pos = fsetʗ1.Position((~s).Range);
                if (pos.Offset != strings.Index(src, rangeˢ)) {
                    Ꮡt.Errorf("%s: got offset %v, want %v"u8, src, pos.Offset, strings.Index(src, rangeˢ));
                }
                break;
            }}
            return true;
        });
    }
}

// TestIssue59180 tests that line number overflow doesn't cause an infinite loop.
public static void TestIssue59180(ж<testing.T> Ꮡt) {
    var testcases = new @string[]{
        "package p\n//line :9223372036854775806\n\n//"u8,
        "package p\n//line :1:9223372036854775806\n\n//"u8,
        "package p\n//line file:9223372036854775806\n\n//"u8
    }.slice();
    foreach (var (_, src) in testcases) {
        var (_, err) = ParseFile(token.NewFileSet(), ""u8, src, ParseComments);
        if (err == default!) {
            Ꮡt.Errorf("ParseFile(%s) succeeded unexpectedly"u8, src);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataGoversionˢ = "./testdata/goversion"u8;

public static void TestGoVersion(ж<testing.T> Ꮡt) {
    var fset = token.NewFileSet();
    var (pkgs, err) = ParseDir(fset, testdataGoversionˢ, default!, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (_, p) in pkgs) {
        @string want = strings.ReplaceAll((~p).Name, "_"u8, "."u8);
        if (want == "none"u8) {
            want = ""u8;
        }
        foreach (var (_, f) in (~p).Files) {
            if ((~f).GoVersion != want) {
                Ꮡt.Errorf("%s: GoVersion = %q, want %q"u8, fset.Position(f.Pos()), (~f).GoVersion, want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packagePFuncFVarXStructˢ = @"package p; func f() { var x struct"u8;

public static void TestIssue57490(ж<testing.T> Ꮡt) {
    @string src = packagePFuncFVarXStructˢ; // program not correctly terminated
    var fset = token.NewFileSet();
    var (@file, err) = ParseFile(fset, ""u8, src, 0);
    if (err == default!) {
        Ꮡt.Fatalf("syntax error expected, but no error reported"u8);
    }
    // Because of the syntax error, the end position of the function declaration
    // is past the end of the file's position range.
    tokenꓸPos funcEnd = (~@file).Decls[0].End();
    // Offset(funcEnd) must not panic (to test panic, set debug=true in token package)
    // (panic: offset 35 out of bounds [0, 34] (position 36 out of bounds [1, 35]))
    var tokFile = fset.File(@file.Pos());
    nint offset = tokFile.Offset(funcEnd);
    if (offset != tokFile.Size()) {
        Ꮡt.Fatalf("offset = %d, want %d"u8, offset, tokFile.Size());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testGoˢ = "test.go"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ3 = "package p; type X[A (B),] struct{}"u8;

public static void TestParseTypeParamsAsParenExpr(ж<testing.T> Ꮡt) {
    @string src = srcᶜ3;
    var fset = token.NewFileSet();
    var (f, err) = ParseFile(fset, testGoˢ, src, (global::go.go.parser_package.Mode)(ParseComments | SkipObjectResolution));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var typeParam = (~(~(~(~f).Decls[0]._<ж<ast.GenDecl>>()).Specs[0]._<ж<ast.TypeSpec>>()).TypeParams).List[0].Value.Type;
    var (_, ok) = typeParam._<ж<ast.ParenExpr>>(ᐧ);
    if (!ok) {
        Ꮡt.Fatalf("typeParam is a %T; want: *ast.ParenExpr"u8, typeParam);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aGoˢ = "a.go"u8;

internal partial struct TestEmptyFileHasValidStartEnd_type /*dyn*/ {
    internal @string src;
    internal @string want; // "Pos() FileStart FileEnd"
}

// TestEmptyFileHasValidStartEnd is a regression test for #70162.
public static void TestEmptyFileHasValidStartEnd(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestEmptyFileHasValidStartEnd_type[]{
        new(src: ""u8, want: "0 1 1"u8),
        new(src: "package "u8, want: "0 1 9"u8),
        new(src: "package p"u8, want: "1 1 10"u8),
        new(src: "type T int"u8, want: "0 1 11"u8)
    }.slice()) {
        var fset = token.NewFileSet();
        var (f, _) = ParseFile(fset, aGoˢ, test.src, 0);
        @string got = fmt.Sprintf("%d %d %d"u8, f.Pos(), (~f).FileStart, (~f).FileEnd);
        if (got != test.want) {
            Ꮡt.Fatalf("src = %q: got %s, want %s"u8, test.src, got, test.want);
        }
    }
}

} // end parser_internal_test_package
