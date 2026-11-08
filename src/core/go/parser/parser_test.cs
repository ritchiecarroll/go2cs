// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using fs = global::go.io.fs_package;
using reflect = reflect_package;
using strings = strings_package;
using testing = testing_package;
using global::go.go;
using global::go.io;
using io = io_package;
using static global::go.go.parser_package;

partial class parser_internal_test_package {

internal static slice<@string> validFiles = [
    "parser.go",
    "parser_test.go",
    "error_test.go",
    "short_test.go"
];

public static void TestParse(ж<testing.T> t) {
    foreach (var (_, filename) in range(validFiles)) {
        var (_, err) = ParseFile(token.NewFileSet(), filename, default, DeclarationErrors);
        if (err != nil) {
            t.Fatalf("ParseFile(%s): %v", filename, err);
        }
    }
}

internal static bool nameFilter(@string filename) {
    var exprᴛ1 = filename;
    switch (ᐧ) {
    case {} when exprᴛ1 == "parser.go"u8 || exprᴛ1 == "interface.go"u8 || exprᴛ1 == "parser_test.go"u8: {
        return true;
    }
    case {} when exprᴛ1 == "parser.go.orig"u8: {
        return true; // permit but should be ignored by ParseDir
    }}
    return false;
}

internal static bool dirFilter(fs.FileInfo f) { return nameFilter(f.Name()); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packagePVarSSSSSSSSSSSSˢ => ref packagePVarSSSSSSSSSSSSˢᵇ;
private static readonly @string packagePVarSSSSSSSSSSSSˢᵇ = "package p\nvar _=s[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]"u8;

public static void TestParseFile(ж<testing.T> t) {
    @string src = packagePVarSSSSSSSSSSSSˢ;
    var (_, err) = ParseFile(token.NewFileSet(), ""u8, src, 0);
    if (err == nil) {
        t.Errorf("ParseFile(%s) succeeded unexpectedly", src);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sSSSSSSSSSSSˢ => ref sSSSSSSSSSSSˢᵇ;
private static readonly @string sSSSSSSSSSSSˢᵇ = "s[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]+\ns[::]"u8;

public static void TestParseExprFrom(ж<testing.T> t) {
    @string src = sSSSSSSSSSSSˢ;
    var (_, err) = ParseExprFrom(token.NewFileSet(), ""u8, src, 0);
    if (err == nil) {
        t.Errorf("ParseExprFrom(%s) succeeded unexpectedly", src);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string parserˢ => ref parserˢᵇ;
private static readonly @string parserˢᵇ = "parser"u8;

public static void TestParseDir(ж<testing.T> t) {
    @string path = ".";
    var (pkgs, err) = ParseDir(token.NewFileSet(), path, dirFilter, 0);
    if (err != nil) {
        t.Fatalf("ParseDir(%s): %v", path, err);
    }
    {
        nint n = len(pkgs); if (n != 1) {
            t.Errorf("got %d packages; want 1", n);
        }
    }
    var pkg = pkgs[parserˢ];
    if (pkg == nil) {
        t.Errorf(@"package ""parser"" not found");
        return;
    }
    {
        nint n = len(pkg.Files); if (n != 3) {
            t.Errorf("got %d package files; want 3", n);
        }
    }
    foreach (var (filename, _) in pkg.Files) {
        if (!nameFilter(filename)) {
            t.Errorf("unexpected package file: %s", filename);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataIssue42951ˢ => ref testdataIssue42951ˢᵇ;
private static readonly @string testdataIssue42951ˢᵇ = "./testdata/issue42951"u8;

public static void TestIssue42951(ж<testing.T> t) {
    @string path = testdataIssue42951ˢ;
    var (_, err) = ParseDir(token.NewFileSet(), path, default, 0);
    if (err != nil) {
        t.Errorf("ParseDir(%s): %v", path, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string structXIntˢ => ref structXIntˢᵇ;
private static readonly @string structXIntˢᵇ = "struct{x *int}"u8;
internal static ref readonly @string aIXˢ => ref aIXˢᵇ;
private static readonly @string aIXˢᵇ = "a[i] := x"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string validExprᶜ => ref validExprᶜᵇ;
private static readonly @string validExprᶜᵇ = "a + b"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string anythingᶜ => ref anythingᶜᵇ;
private static readonly @string anythingᶜᵇ = "dh3*#D)#_"u8;

public static void TestParseExpr(ж<testing.T> t) {
    // just kicking the tires:
    // a valid arithmetic expression
    @string src = "a + b";
    var (x, err) = ParseExpr(src);
    if (err != nil) {
        t.Errorf("ParseExpr(%q): %v", src, err);
    }
    // sanity check
    if (!x._<ж<ast.BinaryExpr>>(out _)) {
        t.Errorf("ParseExpr(%q): got %T, want *ast.BinaryExpr", src, x);
    }

    // a valid type expression
    src = structXIntˢ;
    (x, err) = ParseExpr(src);
    if (err != nil) {
        t.Errorf("ParseExpr(%q): %v", src, err);
    }
    // sanity check
    if (!x._<ж<ast.StructType>>(out _)) {
        t.Errorf("ParseExpr(%q): got %T, want *ast.StructType", src, x);
    }

    // an invalid expression
    src = "a + *";
    (x, err) = ParseExpr(src);
    if (err == nil) {
        t.Errorf("ParseExpr(%q): got no error", src);
    }
    if (x == nil) {
        t.Errorf("ParseExpr(%q): got no (partial) result", src);
    }
    if (!x._<ж<ast.BinaryExpr>>(out _)) {
        t.Errorf("ParseExpr(%q): got %T, want *ast.BinaryExpr", src, x);
    }

    // a valid expression followed by extra tokens is invalid
    src = aIXˢ;
    var (_, errΔ1) = ParseExpr(src); if (errΔ1 == nil) {
        t.Errorf("ParseExpr(%q): got no error", src);
    }

    // a semicolon is not permitted unless automatically inserted
    src = "a + b\n";
    var (_, errΔ2) = ParseExpr(src); if (errΔ2 != nil) {
        t.Errorf("ParseExpr(%q): got error %s", src, errΔ2);
    }
    src = "a + b;";
    var (_, errΔ3) = ParseExpr(src); if (errΔ3 == nil) {
        t.Errorf("ParseExpr(%q): got no error", src);
    }

    // various other stuff following a valid expression
    @string validExpr = validExprᶜ;
    @string anything = anythingᶜ;
    foreach (var (_, c) in (@string)"!)]};,") {
        @string srcΔ1 = validExpr + ((@string)c) + anything;
        var (_, errΔ4) = ParseExpr(srcΔ1); if (errΔ4 == nil) {
            t.Errorf("ParseExpr(%q): got no error", srcΔ1);
        }
    }

    // ParseExpr must not crash
    foreach (var (_, srcΔ2) in range(valids)) {
        ParseExpr(srcΔ2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object packagePFuncFXYZXYZˢ = (@string)@"package p; func f() { x, y, z := x, y, z }"u8;

public static void TestColonEqualsScope(ж<testing.T> t) {
    var (f, err) = ParseFile(token.NewFileSet(), ""u8, packagePFuncFXYZXYZˢ, 0);
    if (err != nil) {
        t.Fatal(err);
    }

    // RHS refers to undefined globals; LHS does not.
    var @as = f.Decls[0]._<ж<ast.FuncDecl>>().Body.List[0]._<ж<ast.AssignStmt>>();
    foreach (var (_, v) in range(@as.Rhs)) {
        var id = v._<ж<ast.Ident>>();
        if (id.Obj != nil) {
            t.Errorf("rhs %s has Obj, should not", id.Name);
        }
    }
    foreach (var (_, v) in range(@as.Lhs)) {
        var id = v._<ж<ast.Ident>>();
        if (id.Obj == nil) {
            t.Errorf("lhs %s does not have Obj, should", id.Name);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object packagePFuncFVarXYZXYZˢ = (@string)@"package p; func f() { var x, y, z = x, y, z }"u8;

public static void TestVarScope(ж<testing.T> t) {
    var (f, err) = ParseFile(token.NewFileSet(), ""u8, packagePFuncFVarXYZXYZˢ, 0);
    if (err != nil) {
        t.Fatal(err);
    }

    // RHS refers to undefined globals; LHS does not.
    var @as = f.Decls[0]._<ж<ast.FuncDecl>>().Body.List[0]._<ж<ast.DeclStmt>>().Decl._<ж<ast.GenDecl>>().Specs[0]._<ж<ast.ValueSpec>>();
    foreach (var (_, v) in range(@as.Values)) {
        var id = v._<ж<ast.Ident>>();
        if (id.Obj != nil) {
            t.Errorf("rhs %s has Obj, should not", id.Name);
        }
    }
    foreach (var (_, id) in range(@as.Names)) {
        if (id.Obj == nil) {
            t.Errorf("lhs %s does not have Obj, should", id.Name);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ => ref srcᶜᵇ;
private static readonly @string srcᶜᵇ = """

package p
import fmt "fmt"
const pi = 3.14
type T struct{}
var x int
func f() { L: }

"""u8.Raw();

public static void TestObjects(ж<testing.T> t) {
    @string src = srcᶜ;

    var (f, err) = ParseFile(token.NewFileSet(), ""u8, src, 0);
    if (err != nil) {
        t.Fatal(err);
    }

    var objects = new map<@string, ast.ObjKind>{
        ["p"] = ast.Bad,   // not in a scope
        ["fmt"] = ast.Bad, // not resolved yet
        ["pi"] = ast.Con,
        ["T"] = ast.Typ,
        ["x"] = ast.Var,
        ["int"] = ast.Bad, // not resolved yet
        ["f"] = ast.Fun,
        ["L"] = ast.Lbl
    };

    var objectsʗ1 = objects;
    ast.Inspect(new ast.FileжNode(f), (ast.Node n) => {
        var (ident, ok) = n._<ж<ast.Ident>>(ᐧ); if (ok) {
            var obj = ident.Obj;
            if (obj == nil) {
                if (objectsʗ1[ident.Name] != ast.Bad) {
                    t.Errorf("no object for %s", ident.Name);
                }
                return true;
            }
            if (obj.Name != ident.Name) {
                t.Errorf("names don't match: obj.Name = %s, ident.Name = %s", obj.Name, ident.Name);
            }
            ast.ObjKind kind = objectsʗ1[ident.Name];
            if (obj.Kind != kind) {
                t.Errorf("%s: obj.Kind = %s; want %s", ident.Name, obj.Kind, kind);
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

"""u8.Raw();

public static void TestUnresolved(ж<testing.T> t) {
    var (f, err) = ParseFile(token.NewFileSet(), ""u8, packagePFuncF1aIntFuncˢ, 0);
    if (err != nil) {
        t.Fatal(err);
    }

    @string want = "int "u8 + // f1a
        "byte int float "u8 + // f2a
        "int float "u8 + // f3a
        "complex "u8 + // f4a
        "complex "u8 + // f5a
        //
        "int "u8 + // f1b
        "byte int float "u8 + // f2b
        "int float "u8 + // f3b
        "complex "u8 + // f4b
        "complex "u8 + // f5b
        //
        "int "u8 + // s1a
        "byte int "u8 + // s2a
        "int float "u8 + // s3a
        //
        "int "u8 + // s1a
        "byte int float "u8 + // s2a
        "float "u8; // s3a

    // collect unresolved identifiers
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    foreach (var (_, u) in range(f.Unresolved)) {
        Ꮡbuf.WriteString(u.Name);
        Ꮡbuf.WriteByte((byte)' ');
    }
    @string got = buf.String();

    if (got != want) {
        t.Errorf("\ngot:  %s\nwant: %s", got, want);
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

"""u8.Raw();

public static void TestCommentGroups(ж<testing.T> t) {
    var (f, err) = ParseFile(token.NewFileSet(), ""u8, packageP1a1b1c1d2a2bˢ, ParseComments);
    if (err != nil) {
        t.Fatal(err);
    }
    var expected = slice<slice<@string>>([
        ["/* 1a */", "/* 1b */", "/* 1c */", "// 1d"],
        ["/* 2a\n*/", "// 2b"],
        ["/* 3a */", "// 3b", "/* 3c */"],
        ["// Example from go.dev/issue/3139"],
        ["// before & after each rune"],
        ["// Output:", "// 3", "// 5"]
    ]);
    if (len(f.Comments) != len(expected)) {
        t.Fatalf("got %d comment groups; expected %d", len(f.Comments), len(expected));
    }
    foreach (var (i, exp) in range(expected)) {
        var got = f.Comments[i].List;
        if (len(got) != len(exp)) {
            t.Errorf("got %d comments in group %d; expected %d", len(got), i, len(exp));
            continue;
        }
        foreach (var (j, expΔ1) in range(exp)) {
            @string gotΔ1 = got[j].Text;
            if (gotΔ1 != expΔ1) {
                t.Errorf("got %q in group %d; expected %q", gotΔ1, i, expΔ1);
            }
        }
    }
}

internal static ж<ast.Field> getField(ж<ast.File> Ꮡfile, @string fieldname) {
    ref var @file = ref Ꮡfile.DerefOrNull();

    var parts = strings.Split(fieldname, ".");
    foreach (var (_, d) in range(@file.Decls)) {
        var (dΔ1, ok) = d._<ж<ast.GenDecl>>(ᐧ); if (ok && dΔ1.Tok == token.TYPE) {
            foreach (var (_, s) in range(dΔ1.Specs)) {
                var (sΔ1, okΔ1) = s._<ж<ast.TypeSpec>>(ᐧ); if (okΔ1 && sΔ1.Name.Name == parts[0]) {
                    var (sΔ2, okΔ2) = sΔ1.Type._<ж<ast.StructType>>(ᐧ); if (okΔ2) {
                        foreach (var (_, f) in range(sΔ2.Fields.List)) {
                            foreach (var (_, name) in range(f.Names)) {
                                if (name.Name == parts[1]) {
                                    return f;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    return default;
}

// Don't use ast.CommentGroup.Text() - we want to see exact comment text.
internal static @string commentText(ж<ast.CommentGroup> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    if (Ꮡc != nil) {
        foreach (var (_, cΔ1) in range(c.List)) {
            Ꮡbuf.WriteString(cΔ1.Text);
        }
    }
    return buf.String();
}

internal static void checkFieldComments(ж<testing.T> t, ж<ast.File> @file, @string fieldname, @string lead, @string line) {
    var f = getField(@file, fieldname);
    if (f == nil) {
        t.Fatalf("field not found: %s", fieldname);
    }
    {
        @string got = commentText(f.Doc); if (got != lead) {
            t.Errorf("got lead comment %q; expected %q", got, lead);
        }
    }
    {
        @string got = commentText(f.Comment); if (got != line) {
            t.Errorf("got line comment %q; expected %q", got, line);
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

"""u8.Raw();
internal static ref readonly @string tF1ˢ => ref tF1ˢᵇ;
private static readonly @string tF1ˢᵇ = "T.F1"u8;
internal static ref readonly @string f1LeadCommentˢ => ref f1LeadCommentˢᵇ;
private static readonly @string f1LeadCommentˢᵇ = "/* F1 lead comment *///"u8;
internal static ref readonly @string f1LineCommentˢ => ref f1LineCommentˢᵇ;
private static readonly @string f1LineCommentˢᵇ = "/* F1 */// line comment"u8;
internal static ref readonly @string tF2ˢ => ref tF2ˢᵇ;
private static readonly @string tF2ˢᵇ = "T.F2"u8;
internal static ref readonly @string f2LeadCommentˢ => ref f2LeadCommentˢᵇ;
private static readonly @string f2LeadCommentˢᵇ = "// F2 lead// comment"u8;
internal static ref readonly @string f2LineCommentˢ => ref f2LineCommentˢᵇ;
private static readonly @string f2LineCommentˢᵇ = "// F2 line comment"u8;
internal static ref readonly @string tF3ˢ => ref tF3ˢᵇ;
private static readonly @string tF3ˢᵇ = "T.f3"u8;
internal static ref readonly @string f3LeadCommentˢ => ref f3LeadCommentˢᵇ;
private static readonly @string f3LeadCommentˢᵇ = "// f3 lead comment"u8;
internal static ref readonly @string f3LineCommentˢ => ref f3LineCommentˢᵇ;
private static readonly @string f3LineCommentˢᵇ = "// f3 line comment"u8;
internal static ref readonly @string tF4ˢ => ref tF4ˢᵇ;
private static readonly @string tF4ˢᵇ = "T.f4"u8;
internal static ref readonly @string tF5ˢ => ref tF5ˢᵇ;
private static readonly @string tF5ˢᵇ = "T.f5"u8;
internal static ref readonly @string f5LineCommentˢ => ref f5LineCommentˢᵇ;
private static readonly @string f5LineCommentˢᵇ = "// f5 line comment"u8;
internal static ref readonly @string tF6ˢ => ref tF6ˢᵇ;
private static readonly @string tF6ˢᵇ = "T.f6"u8;
internal static ref readonly @string f6LineCommentˢ => ref f6LineCommentˢᵇ;
private static readonly @string f6LineCommentˢᵇ = "/* f6 line comment */"u8;
internal static ref readonly @string tF7ˢ => ref tF7ˢᵇ;
private static readonly @string tF7ˢᵇ = "T.f7"u8;
internal static ref readonly @string f7aF7bF7cˢ => ref f7aF7bF7cˢᵇ;
private static readonly @string f7aF7bF7cˢᵇ = "/*f7a*//*f7b*///f7c"u8;
internal static readonly object notExpectedToFindTF3ˢ = (@string)"not expected to find T.f3"u8;

public static void TestLeadAndLineComments(ж<testing.T> t) {
    var (f, err) = ParseFile(token.NewFileSet(), ""u8, packagePTypeTStructF1ˢ, ParseComments);
    if (err != nil) {
        t.Fatal(err);
    }
    checkFieldComments(t, f, tF1ˢ, f1LeadCommentˢ, f1LineCommentˢ);
    checkFieldComments(t, f, tF2ˢ, f2LeadCommentˢ, f2LineCommentˢ);
    checkFieldComments(t, f, tF3ˢ, f3LeadCommentˢ, f3LineCommentˢ);
    checkFieldComments(t, f, tF4ˢ, ""u8, ""u8);
    checkFieldComments(t, f, tF5ˢ, ""u8, f5LineCommentˢ);
    checkFieldComments(t, f, tF6ˢ, ""u8, f6LineCommentˢ);
    checkFieldComments(t, f, tF7ˢ, ""u8, f7aF7bF7cˢ);

    ast.FileExports(f);
    checkFieldComments(t, f, tF1ˢ, f1LeadCommentˢ, f1LineCommentˢ);
    checkFieldComments(t, f, tF2ˢ, f2LeadCommentˢ, f2LineCommentˢ);
    if (getField(f, tF3ˢ) != nil) {
        t.Error(notExpectedToFindTF3ˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string wantˢ => ref wantˢᵇ;
private static readonly @string wantˢᵇ = "want ';'"u8;
internal static ref readonly @string butIsImplicitˢ => ref butIsImplicitˢᵇ;
private static readonly @string butIsImplicitˢᵇ = "but ';' is implicit"u8;

// TestIssue9979 verifies that empty statements are contained within their enclosing blocks.
public static void TestIssue9979(ж<testing.T> t) {
    foreach (var (_, src) in range(slice<@string>([
        "package p; func f() {;}",
        "package p; func f() {L:}",
        "package p; func f() {L:;}",
        "package p; func f() {L:\n}",
        "package p; func f() {L:\n;}",
        "package p; func f() { ; }",
        "package p; func f() { L: }",
        "package p; func f() { L: ; }",
        "package p; func f() { L: \n}",
        "package p; func f() { L: \n; }"
    ]))) {
        var fset = token.NewFileSet();
        var (f, err) = ParseFile(fset, ""u8, src, 0);
        if (err != nil) {
            t.Fatal(err);
        }

        tokenꓸPos pos = default;
        tokenꓸPos end = default;
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
                // check containment
                if (s.Pos() < pos || s.End() > end) {
                    t.Errorf("%s: %T[%d, %d] not inside [%d, %d]", src, s.OrTypedNil(), s.Pos(), s.End(), pos, end);
                }
                // check semicolon
                nint offs = fsetʗ1.Position(s.Pos()).Offset;
                var ch = src[offs]; if (ch != ';' != s.Implicit) {
                    @string want = wantˢ;
                    if (s.Implicit) {
                        want = butIsImplicitˢ;
                    }
                    t.Errorf("%s: found %q at offset %d; %s", src, ch, offs, want);
                }
                break;
            }}
            return true;
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fileGoˢ => ref fileGoˢᵇ;
private static readonly @string fileGoˢᵇ = "file.go"u8;
internal static ref readonly @string fileGo11ˢ => ref fileGo11ˢᵇ;
private static readonly @string fileGo11ˢᵇ = "file.go:1:1"u8;
internal static ref readonly @string fileGo1019ˢ => ref fileGo1019ˢᵇ;
private static readonly @string fileGo1019ˢᵇ = "file.go:10:19"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ1 => ref srcᶜ1ᵇ;
private static readonly @string srcᶜ1ᵇ = """
// Copyright

//+build tag

// Package p doc comment.
package p

var lastDecl int

/* end of file */

"""u8.Raw();

public static void TestFileStartEndPos(ж<testing.T> t) {
    @string src = srcᶜ1;
    var fset = token.NewFileSet();
    var (f, err) = ParseFile(fset, fileGoˢ, src, 0);
    if (err != nil) {
        t.Fatal(err);
    }

    // File{Start,End} spans the entire file, not just the declarations.
    {
        @string got = fset.Position(f.FileStart).String();
        @string want = fileGo11ˢ; if (got != want) {
            t.Errorf("for File.FileStart, got %s, want %s", got, want);
        }
    }
    // The end position is the newline at the end of the /* end of file */ line.
    {
        @string got = fset.Position(f.FileEnd).String();
        @string want = fileGo1019ˢ; if (got != want) {
            t.Errorf("for File.FileEnd, got %s, want %s", got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object foundNoAstSelectorExprˢ = (@string)"found no *ast.SelectorExpr"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantErrᶜ => ref wantErrᶜᵇ;
private static readonly @string wantErrᶜᵇ = "expected selector or type assertion"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantSelᶜ => ref wantSelᶜᵇ;
private static readonly @string wantSelᶜᵇ = "&{fmt _}"u8;

// TestIncompleteSelection ensures that an incomplete selector
// expression is parsed as a (blank) *ast.SelectorExpr, not a
// *ast.BadExpr.
public static void TestIncompleteSelection(ж<testing.T> t) {
    foreach (var (_, src) in range(slice<@string>([
        "package p; var _ = fmt.",            // at EOF
        "package p; var _ = fmt.\ntype X int" // not at EOF
    ]))) {
        var fset = token.NewFileSet();
        var (f, err) = ParseFile(fset, ""u8, src, 0);
        if (err == nil) {
            t.Errorf("ParseFile(%s) succeeded unexpectedly", src);
            continue;
        }

        @string wantErr = wantErrᶜ;
        if (!strings.Contains(err.Error(), wantErr)) {
            t.Errorf("ParseFile returned wrong error %q, want %q", err, wantErr);
        }

        ref var sel = ref heap<ж<ast.SelectorExpr>>(out var Ꮡsel);
        ast.Inspect(new ast.FileжNode(f), (ast.Node n) => {
            var (nΔ1, ok) = n._<ж<ast.SelectorExpr>>(ᐧ); if (ok) {
                Ꮡsel.ValueSlot = nΔ1;
            }
            return true;
        });
        if (sel == nil) {
            t.Error(foundNoAstSelectorExprˢ);
            continue;
        }
        @string wantSel = wantSelᶜ;
        if (fmt.Sprint(sel.OrTypedNil()) != wantSel) {
            t.Errorf("found selector %s, want %s", sel.OrTypedNil(), wantSel);
            continue;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object commentˢ = (@string)"// comment"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ2 => ref srcᶜ2ᵇ;
private static readonly @string srcᶜ2ᵇ = """
package main
type x int // comment

"""u8.Raw();

public static void TestLastLineComment(ж<testing.T> t) {
    @string src = srcᶜ2;
    var fset = token.NewFileSet();
    var (f, err) = ParseFile(fset, ""u8, src, ParseComments);
    if (err != nil) {
        t.Fatal(err);
    }
    @string comment = f.Decls[0]._<ж<ast.GenDecl>>().Specs[0]._<ж<ast.TypeSpec>>().Comment.List[0].Text;
    if (comment != "// comment"u8) {
        t.Errorf("got %q, want %q", comment, commentˢ);
    }
}

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
internal static slice<parseDepthTestsᴛ1> parseDepthTests = [
    // The format expands the part inside « » many times.
    // A second set of brackets nested inside the first stops the repetition,
    // so that for example «(«1»)» expands to (((...((((1))))...))).
    new(name: "array", format: "package main; var x «[1]»int"),
    new(name: "slice", format: "package main; var x «[]»int"),
    new(name: "struct", format: "package main; var x «struct { X «int» }»", scope: true),
    new(name: "pointer", format: "package main; var x «*»int"),
    new(name: "func", format: "package main; var x «func()»int", scope: true),
    new(name: "chan", format: "package main; var x «chan »int"),
    new(name: "chan2", format: "package main; var x «<-chan »int"),
    new(name: "interface", format: "package main; var x «interface { M() «int» }»", scope: true, scopeMultiplier: 2), // Scopes: InterfaceType, FuncType
    new(name: "map", format: "package main; var x «map[int]»int"),
    new(name: "slicelit", format: "package main; var x = []any{«[]any{«»}»}", parseMultiplier: 3),      // Parser nodes: UnaryExpr, CompositeLit
    new(name: "arraylit", format: "package main; var x = «[1]any{«nil»}»", parseMultiplier: 3),         // Parser nodes: UnaryExpr, CompositeLit
    new(name: "structlit", format: "package main; var x = «struct{x any}{«nil»}»", parseMultiplier: 3), // Parser nodes: UnaryExpr, CompositeLit
    new(name: "maplit", format: "package main; var x = «map[int]any{1:«nil»}»", parseMultiplier: 3),    // Parser nodes: CompositeLit, KeyValueExpr
    new(name: "element", format: "package main; var x = struct{x any}{x: «{«»}»}"),
    new(name: "dot", format: "package main; var x = «x.»x"),
    new(name: "index", format: "package main; var x = x«[1]»"),
    new(name: "slice", format: "package main; var x = x«[1:2]»"),
    new(name: "slice3", format: "package main; var x = x«[1:2:3]»"),
    new(name: "dottype", format: "package main; var x = x«.(any)»"),
    new(name: "callseq", format: "package main; var x = x«()»"),
    new(name: "methseq", format: "package main; var x = x«.m()»", parseMultiplier: 2), // Parser nodes: SelectorExpr, CallExpr
    new(name: "binary", format: "package main; var x = «1+»1"),
    new(name: "binaryparen", format: "package main; var x = «1+(«1»)»", parseMultiplier: 2), // Parser nodes: BinaryExpr, ParenExpr
    new(name: "unary", format: "package main; var x = «^»1"),
    new(name: "addr", format: "package main; var x = «& »x"),
    new(name: "star", format: "package main; var x = «*»x"),
    new(name: "recv", format: "package main; var x = «<-»x"),
    new(name: "call", format: "package main; var x = «f(«1»)»", parseMultiplier: 2),    // Parser nodes: Ident, CallExpr
    new(name: "conv", format: "package main; var x = «(*T)(«1»)»", parseMultiplier: 2), // Parser nodes: ParenExpr, CallExpr
    new(name: "label", format: "package main; func main() { «Label:» }"),
    new(name: "if", format: "package main; func main() { «if true { «» }»}", parseMultiplier: 2, scope: true, scopeMultiplier: 2), // Parser nodes: IfStmt, BlockStmt. Scopes: IfStmt, BlockStmt
    new(name: "ifelse", format: "package main; func main() { «if true {} else » {} }", scope: true),
    new(name: "switch", format: "package main; func main() { «switch { default: «» }»}", scope: true, scopeMultiplier: 2),               // Scopes: TypeSwitchStmt, CaseClause
    new(name: "typeswitch", format: "package main; func main() { «switch x.(type) { default: «» }» }", scope: true, scopeMultiplier: 2), // Scopes: TypeSwitchStmt, CaseClause
    new(name: "for0", format: "package main; func main() { «for { «» }» }", scope: true, scopeMultiplier: 2),                            // Scopes: ForStmt, BlockStmt
    new(name: "for1", format: "package main; func main() { «for x { «» }» }", scope: true, scopeMultiplier: 2),                          // Scopes: ForStmt, BlockStmt
    new(name: "for3", format: "package main; func main() { «for f(); g(); h() { «» }» }", scope: true, scopeMultiplier: 2),              // Scopes: ForStmt, BlockStmt
    new(name: "forrange0", format: "package main; func main() { «for range x { «» }» }", scope: true, scopeMultiplier: 2),               // Scopes: RangeStmt, BlockStmt
    new(name: "forrange1", format: "package main; func main() { «for x = range z { «» }» }", scope: true, scopeMultiplier: 2),           // Scopes: RangeStmt, BlockStmt
    new(name: "forrange2", format: "package main; func main() { «for x, y = range z { «» }» }", scope: true, scopeMultiplier: 2),        // Scopes: RangeStmt, BlockStmt
    new(name: "go", format: "package main; func main() { «go func() { «» }()» }", parseMultiplier: 2, scope: true),                      // Parser nodes: GoStmt, FuncLit
    new(name: "defer", format: "package main; func main() { «defer func() { «» }()» }", parseMultiplier: 2, scope: true),                // Parser nodes: DeferStmt, FuncLit
    new(name: "select", format: "package main; func main() { «select { default: «» }» }", scope: true),
    new(name: "block", format: "package main; func main() { «{«»}» }", scope: true)
];

// split splits pre«mid»post into pre, mid, post.
// If the string does not have that form, split returns x, "", "".
internal static (@string pre, @string mid, @string post) split(@string x) {
    nint start = strings.Index(x, "«");
    nint end = strings.LastIndex(x, "»");
    if (start < 0 || end < 0) {
        return (x, "", "");
    }
    return (x.slice(0, start), x.slice(start + len("«"), end), x.slice(end + len("»")));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testRequiresSignificantˢ = (@string)"test requires significant memory"u8;

public static void TestParseDepthLimit(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(testRequiresSignificantˢ);
    }
    foreach (var (_, vᴛ1) in range(parseDepthTests)) {
        ref var tt = ref heap(new parseDepthTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        foreach (var (_, size) in range(slice<@string>(["small", "big"]))) {
            var ttʗ1 = tt;
            t.Run(tt.name + "/"u8 + size, (ж<testing.T> tΔ1) => {
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
                if (strings.Contains(mid, "«")) {
                    var (left, @base, right) = split(mid);
                    mid = strings.Repeat(left, n) + @base + strings.Repeat(right, n);
                } else {
                    mid = strings.Repeat(mid, n);
                }
                @string input = pre + mid + post;

                var fset = token.NewFileSet();
                var (_, err) = ParseFile(fset, ""u8, input, (global::go.go.parser_package.Mode)(ParseComments | SkipObjectResolution));
                if (size == "small"u8) {
                    if (err != nil) {
                        tΔ1.Errorf("ParseFile(...): %v (want success)", err);
                    }
                } else {
                    @string expected = exceededMaxNestingDepthˢ;
                    if (err == nil || !strings.HasSuffix(err.Error(), expected)) {
                        tΔ1.Errorf("ParseFile(...) = _, %v, want %q", err, expected);
                    }
                }
            });
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string exceededMaxScopeDepthˢ => ref exceededMaxScopeDepthˢᵇ;
private static readonly @string exceededMaxScopeDepthˢᵇ = "exceeded max scope depth during object resolution"u8;

public static void TestScopeDepthLimit(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(parseDepthTests)) {
        ref var tt = ref heap(new parseDepthTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        if (!tt.scope) {
            continue;
        }
        foreach (var (_, size) in range(slice<@string>(["small", "big"]))) {
            var ttʗ1 = tt;
            t.Run(tt.name + "/"u8 + size, (ж<testing.T> tΔ1) => {
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
                if (strings.Contains(mid, "«")) {
                    var (left, @base, right) = split(mid);
                    mid = strings.Repeat(left, n) + @base + strings.Repeat(right, n);
                } else {
                    mid = strings.Repeat(mid, n);
                }
                @string input = pre + mid + post;

                var fset = token.NewFileSet();
                var (_, err) = ParseFile(fset, ""u8, input, DeclarationErrors);
                if (size == "small"u8) {
                    if (err != nil) {
                        tΔ1.Errorf("ParseFile(...): %v (want success)", err);
                    }
                } else {
                    @string expected = exceededMaxScopeDepthˢ;
                    if (err == nil || !strings.HasSuffix(err.Error(), expected)) {
                        tΔ1.Errorf("ParseFile(...) = _, %v, want %q", err, expected);
                    }
                }
            });
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string rangeˢ => ref rangeˢᵇ;
private static readonly @string rangeˢᵇ = "range"u8;

// proposal go.dev/issue/50429
public static void TestRangePos(ж<testing.T> t) {
    var testcases = slice<@string>([
        "package p; func _() { for range x {} }",
        "package p; func _() { for i = range x {} }",
        "package p; func _() { for i := range x {} }",
        "package p; func _() { for k, v = range x {} }",
        "package p; func _() { for k, v := range x {} }"
    ]);

    foreach (var (_, src) in range(testcases)) {
        var fset = token.NewFileSet();
        var (f, err) = ParseFile(fset, src, src, 0);
        if (err != nil) {
            t.Fatal(err);
        }

        var fsetʗ1 = fset;
        ast.Inspect(new ast.FileжNode(f), (ast.Node x) => {
            switch (x.type()) {
            case ж<ast.RangeStmt> s: {
                var pos = fsetʗ1.Position(s.Range);
                if (pos.Offset != strings.Index(src, rangeˢ)) {
                    t.Errorf("%s: got offset %v, want %v", src, pos.Offset, strings.Index(src, rangeˢ));
                }
                break;
            }}
            return true;
        });
    }
}

// TestIssue59180 tests that line number overflow doesn't cause an infinite loop.
public static void TestIssue59180(ж<testing.T> t) {
    var testcases = slice<@string>([
        "package p\n//line :9223372036854775806\n\n//",
        "package p\n//line :1:9223372036854775806\n\n//",
        "package p\n//line file:9223372036854775806\n\n//"
    ]);

    foreach (var (_, src) in range(testcases)) {
        var (_, err) = ParseFile(token.NewFileSet(), ""u8, src, ParseComments);
        if (err == nil) {
            t.Errorf("ParseFile(%s) succeeded unexpectedly", src);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataGoversionˢ => ref testdataGoversionˢᵇ;
private static readonly @string testdataGoversionˢᵇ = "./testdata/goversion"u8;

public static void TestGoVersion(ж<testing.T> t) {
    var fset = token.NewFileSet();
    var (pkgs, err) = ParseDir(fset, testdataGoversionˢ, default, 0);
    if (err != nil) {
        t.Fatal(err);
    }

    foreach (var (_, p) in pkgs) {
        @string want = strings.ReplaceAll(p.Name, "_", ".");
        if (want == "none"u8) {
            want = ""u8;
        }
        foreach (var (_, f) in p.Files) {
            if (f.GoVersion != want) {
                t.Errorf("%s: GoVersion = %q, want %q", fset.Position(f.Pos()), f.GoVersion, want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packagePFuncFVarXStructˢ => ref packagePFuncFVarXStructˢᵇ;
private static readonly @string packagePFuncFVarXStructˢᵇ = @"package p; func f() { var x struct"u8;

public static void TestIssue57490(ж<testing.T> t) {
    @string src = packagePFuncFVarXStructˢ; // program not correctly terminated
    var fset = token.NewFileSet();
    var (@file, err) = ParseFile(fset, ""u8, src, 0);
    if (err == nil) {
        t.Fatalf("syntax error expected, but no error reported");
    }

    // Because of the syntax error, the end position of the function declaration
    // is past the end of the file's position range.
    tokenꓸPos funcEnd = @file.Decls[0].End();

    // Offset(funcEnd) must not panic (to test panic, set debug=true in token package)
    // (panic: offset 35 out of bounds [0, 34] (position 36 out of bounds [1, 35]))
    var tokFile = fset.File(@file.Pos());
    nint offset = tokFile.Offset(funcEnd);
    if (offset != tokFile.Size()) {
        t.Fatalf("offset = %d, want %d", offset, tokFile.Size());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testGoˢ => ref testGoˢᵇ;
private static readonly @string testGoˢᵇ = "test.go"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ3 => ref srcᶜ3ᵇ;
private static readonly @string srcᶜ3ᵇ = "package p; type X[A (B),] struct{}"u8;

public static void TestParseTypeParamsAsParenExpr(ж<testing.T> t) {
    @string src = srcᶜ3;

    var fset = token.NewFileSet();
    var (f, err) = ParseFile(fset, testGoˢ, src, (global::go.go.parser_package.Mode)(ParseComments | SkipObjectResolution));
    if (err != nil) {
        t.Fatal(err);
    }

    var typeParam = f.Decls[0]._<ж<ast.GenDecl>>().Specs[0]._<ж<ast.TypeSpec>>().TypeParams.List[0].Type;
    var (_, ok) = typeParam._<ж<ast.ParenExpr>>(ᐧ);
    if (!ok) {
        t.Fatalf("typeParam is a %T; want: *ast.ParenExpr", typeParam);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aGoˢ => ref aGoˢᵇ;
private static readonly @string aGoˢᵇ = "a.go"u8;

internal partial struct TestEmptyFileHasValidStartEnd_type /*dyn*/ {
    internal @string src;
    internal @string want; // "Pos() FileStart FileEnd"
}

// TestEmptyFileHasValidStartEnd is a regression test for #70162.
public static void TestEmptyFileHasValidStartEnd(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestEmptyFileHasValidStartEnd_type>([
        new(src: "", want: "0 1 1"),
        new(src: "package ", want: "0 1 9"),
        new(src: "package p", want: "1 1 10"),
        new(src: "type T int", want: "0 1 11")
    ]))) {
        var fset = token.NewFileSet();
        var (f, _) = ParseFile(fset, aGoˢ, test.src, 0);
        @string got = fmt.Sprintf("%d %d %d"u8, f.Pos(), f.FileStart, f.FileEnd);
        if (got != test.want) {
            t.Fatalf("src = %q: got %s, want %s", test.src, got, test.want);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ4 => ref srcᶜ4ᵇ;
private static readonly @string srcᶜ4ᵇ = """
package main
func test() {
//line a:15:1
	//
}

"""u8.Raw();

public static void TestCommentGroupWithLineDirective(ж<testing.T> t) {
    @string src = srcᶜ4;
    var fset = token.NewFileSet();
    var (f, err) = ParseFile(fset, testGoˢ, src, (global::go.go.parser_package.Mode)(ParseComments | SkipObjectResolution));
    if (err != nil) {
        t.Fatal(err);
    }

    var wantCommentGroups = slice<ж<ast.CommentGroup>>([
        Ꮡ(new ast.CommentGroup(
            List: slice<ж<ast.Comment>>([
                Ꮡ(new ast.Comment(
                    Slash: ((tokenꓸPos)28),
                    Text: "//line a:15:1")),
                Ꮡ(new ast.Comment(
                    Slash: ((tokenꓸPos)43),
                    Text: "//"))
            ])))
    ]);

    if (!reflect.DeepEqual(f.Comments, wantCommentGroups)) {
        ref var got = ref heap(new strings.Builder(), out var Ꮡgot);
        ref var want = ref heap(new strings.Builder(), out var Ꮡwant);
        ast.Fprint(new parser_test_package.strings_BuilderжWriter(Ꮡgot), fset, f.Comments, default);
        ast.Fprint(new parser_test_package.strings_BuilderжWriter(Ꮡwant), fset, wantCommentGroups, default);
        t.Fatalf("unexpected f.Comments got:\n%v\nwant:\n%v", got.String(), want.String());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object missingLineCommentˢ = (@string)"missing line comment"u8;
internal static readonly object missingDocCommentˢ = (@string)"missing doc comment"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ5 => ref srcᶜ5ᵇ;
private static readonly @string srcᶜ5ᵇ = """
package test

var _ int; /* line comment */
// Doc comment
func _() {}

var _ int; /* line comment */
// Some comment

func _() {}

"""u8.Raw();

// TestBothLineAndLeadComment makes sure that we populate the
// p.lineComment field even though there is a comment after the
// line comment.
public static void TestBothLineAndLeadComment(ж<testing.T> t) {
    @string src = srcᶜ5;

    var fset = token.NewFileSet();
    var (f, _) = ParseFile(fset, ""u8, src, (global::go.go.parser_package.Mode)(ParseComments | SkipObjectResolution));

    var lineComment = f.Decls[0]._<ж<ast.GenDecl>>().Specs[0]._<ж<ast.ValueSpec>>().Comment;
    var docComment = f.Decls[1]._<ж<ast.FuncDecl>>().Doc;

    if (lineComment == nil) {
        t.Fatal(missingLineCommentˢ);
    }
    if (docComment == nil) {
        t.Fatal(missingDocCommentˢ);
    }

    if (lineComment.List[0].Text != "/* line comment */"u8) {
        t.Errorf(@"unexpected line comment got = %q; want ""/* line comment */""", lineComment.List[0].Text);
    }
    if (docComment.List[0].Text != "// Doc comment"u8) {
        t.Errorf(@"unexpected line comment got = %q; want ""// Doc comment""", docComment.List[0].Text);
    }

    var lineComment2 = f.Decls[2]._<ж<ast.GenDecl>>().Specs[0]._<ж<ast.ValueSpec>>().Comment;
    if (lineComment2 == nil) {
        t.Fatal(missingLineCommentˢ);
    }
    if (lineComment.List[0].Text != "/* line comment */"u8) {
        t.Errorf(@"unexpected line comment got = %q; want ""/* line comment */""", lineComment.List[0].Text);
    }

    var docComment2 = f.Decls[3]._<ж<ast.FuncDecl>>().Doc;
    if (docComment2 != nil) {
        t.Errorf("unexpected doc comment %v", docComment2.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string abcˢ => ref abcˢᵇ;
private static readonly @string abcˢᵇ = "`abc\n`"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string stringlitᶜ => ref stringlitᶜᵇ;
private static readonly @string stringlitᶜᵇ = "`abc\r\n`"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ6 => ref srcᶜ6ᵇ;
private static readonly @string srcᶜ6ᵇ = "package p\n\nimport `abc\r\n` ;\n\ntype _ struct{ x int `abc\r\n` }\n\nconst _ = `abc\r\n` ;\n";

// Tests of BasicLit.End() method, which in go1.26 started precisely
// recording the Value token's end position instead of heuristically
// computing it, which is inaccurate for strings containing "\r".
public static void TestBasicLit_End(ж<testing.T> t) {
    // lit is a raw string literal containing [a b c \r \n],
    // denoting "abc\n", because the scanner normalizes \r\n to \n.
    @string stringlit = stringlitᶜ;

    // The semicolons exercise the case in which the next token
    // (a SEMICOLON implied by a \n) isn't immediate but follows
    // some horizontal space.
    @string src = srcᶜ6;

    var fset = token.NewFileSet();
    var (f, _) = ParseFile(fset, ""u8, src, (global::go.go.parser_package.Mode)(ParseComments | SkipObjectResolution));
    var tokFile = fset.File(f.Pos());

    nint count = 0;
    var fsetʗ1 = fset;
    var tokFileʗ1 = tokFile;
    ast.Inspect(new ast.FileжNode(f), (ast.Node n) => {
        var (lit, ok) = n._<ж<ast.BasicLit>>(ᐧ); if (ok) {
            count++;
            nint start = tokFileʗ1.Offset(lit.Pos());
            nint end = tokFileʗ1.Offset(lit.End());

            // Check BasicLit.Value.
            @string want = abcˢ; if ((~lit).Value != want) {
                t.Errorf("%s: BasicLit.Value = %q, want %q", fsetʗ1.Position(lit.Pos()), (~lit).Value, want);
            }

            // Check source extent.
            @string got = src.slice(start, end); if (got != stringlit) {
                t.Errorf("%s: src[BasicLit.Pos:End] = %q, want %q", fsetʗ1.Position(lit.Pos()), got, stringlit);
            }
        }
        return true;
    });
    if (count != 3) {
        t.Errorf("found %d BasicLit, want 3", count);
    }
}

} // end parser_internal_test_package
