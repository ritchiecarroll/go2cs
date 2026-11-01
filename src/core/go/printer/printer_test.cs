// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using bytes = bytes_package;
using errors = errors_package;
using flag = flag_package;
using fmt = fmt_package;
using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using diff = global::go.@internal.diff_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using fs = global::go.io.fs_package;
using global::go.@internal;
using global::go.go;
using path;
using static global::go.go.printer_package;

partial class printer_internal_test_package {

internal static ref readonly @string dataDir => ref dataDirᵇ;
private static readonly @string dataDirᵇ = "testdata"u8;
internal static UntypedInt tabwidth => 8;

internal static ж<bool> update = flag.Bool("update", false, "update golden files");

internal static ж<token.FileSet> fset = token.NewFileSet();

internal partial struct checkMode /*num:nuint*/;

internal static checkMode export => /* 1 << iota */ 1;
internal static checkMode rawFormat => 2;
internal static checkMode normNumber => 4;
internal static checkMode idempotent => 8;
internal static checkMode allowTypeParams => 16;

// format parses src, prints the corresponding AST, verifies the resulting
// src is syntactically correct, and returns the resulting src or an error
// if any.
internal static (slice<byte>, error) format(slice<byte> src, checkMode mode) {
    // parse src
    var (f, err) = parser.ParseFile(fset, ""u8, src, parser.ParseComments);
    if (err != nil) {
        return (default, fmt.Errorf("parse: %s\n%s"u8, err, src));
    }

    // filter exports if necessary
    if ((checkMode)(mode & export) != 0) {
        ast.FileExports(f);   // ignore result
        f.Comments = default; // don't print comments that are not in AST
    }

    // determine printer configuration
    ref var cfg = ref heap<global::go.go.printer_package.Config>(new Config(Tabwidth: tabwidth), out var Ꮡcfg);
    if ((checkMode)(mode & rawFormat) != 0) {
        cfg.Mode |= (global::go.go.printer_package.Mode)(RawFormat);
    }
    if ((checkMode)(mode & normNumber) != 0) {
        cfg.Mode |= (global::go.go.printer_package.Mode)(normalizeNumbers);
    }

    // print AST
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var errΔ1 = Ꮡcfg.Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f.OrTypedNil()); if (errΔ1 != nil) {
            return (default, fmt.Errorf("print: %s"u8, errΔ1));
        }
    }

    // make sure formatted output is syntactically correct
    var res = buf.Bytes();
    {
        var (_, errΔ2) = parser.ParseFile(fset, ""u8, res, parser.ParseComments); if (errΔ2 != nil) {
            return (default, fmt.Errorf("re-parse: %s\n%s"u8, errΔ2, buf.Bytes()));
        }
    }

    return (res, default);
}

// lineAt returns the line in text starting at offset offs.
internal static slice<byte> lineAt(slice<byte> text, nint offs) {
    nint i = offs;
    while (i < len(text) && text[i] != '\n') {
        i++;
    }
    return text.slice(offs, i);
}

// checkEqual compares a and b.
internal static error checkEqual(@string aname, @string bname, slice<byte> a, slice<byte> b) {
    if (bytes.Equal(a, b)) {
        return default;
    }
    return errors.New((@string)diff.Diff(aname, a, bname, b));
}

internal static void runcheck(ж<testing.T> t, @string source, @string golden, checkMode mode) {
    var (src, err) = os.ReadFile(source);
    if (err != nil) {
        t.Error(err);
        return;
    }

    (var res, err) = format(src, mode);
    if (err != nil) {
        t.Error(err);
        return;
    }

    // update golden files if necessary
    if (update.Value) {
        {
            var errΔ1 = os.WriteFile(golden, res, 420); if (errΔ1 != nil) {
                t.Error(errΔ1);
            }
        }
        return;
    }

    // get golden
    (var gld, err) = os.ReadFile(golden);
    if (err != nil) {
        t.Error(err);
        return;
    }

    // formatted source and golden must be the same
    {
        var errΔ2 = checkEqual(fmt.Sprintf("format(%v)"u8, source), golden, res, gld); if (errΔ2 != nil) {
            t.Error(errΔ2);
            return;
        }
    }

    if ((checkMode)(mode & idempotent) != 0) {
        // formatting golden must be idempotent
        // (This is very difficult to achieve in general and for now
        // it is only checked for files explicitly marked as such.)
        (res, err) = format(gld, mode);
        if (err != nil) {
            t.Error(err);
            return;
        }
        {
            var errΔ3 = checkEqual(golden, fmt.Sprintf("format(%s)"u8, golden), gld, res); if (errΔ3 != nil) {
                t.Errorf("golden is not idempotent: %s", errΔ3);
            }
        }
    }
}

internal static partial void check(ж<testing.T> t, @string source, @string golden, checkMode mode) {
    // run the test
    var cc = new channel<nint>(1);
    var ccʗ1 = cc;
    goǃ(() => {
        runcheck(t, source, golden, mode);
        ccʗ1.ᐸꟷ(0);
    });

    // wait with timeout
    var selᴛ1 = time.After((time.Duration)(10000000000L));
    var selᴛ2 = cc;
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out _): { // plenty of a safety margin, even for very slow machines
        // test running past time out
        t.Errorf("%s: running too slowly", source);
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out _): {
        // test finished within allotted time margin
        break;
    }}
}

internal partial struct entry {
    internal @string source, golden;
    internal checkMode mode;
}

// Use go test -update to create/update the respective golden files.
internal static slice<entry> data = slice<entry>([
    new("empty.input", "empty.golden", idempotent),
    new("comments.input", "comments.golden", 0),
    new("comments.input", "comments.x", export),
    new("comments2.input", "comments2.golden", idempotent),
    new("alignment.input", "alignment.golden", idempotent),
    new("linebreaks.input", "linebreaks.golden", idempotent),
    new("expressions.input", "expressions.golden", idempotent),
    new("expressions.input", "expressions.raw", (checkMode)(rawFormat | idempotent)),
    new("declarations.input", "declarations.golden", 0),
    new("statements.input", "statements.golden", 0),
    new("slow.input", "slow.golden", idempotent),
    new("complit.input", "complit.x", export),
    new("go2numbers.input", "go2numbers.golden", idempotent),
    new("go2numbers.input", "go2numbers.norm", (checkMode)(normNumber | idempotent)),
    new("generics.input", "generics.golden", (checkMode)(idempotent | allowTypeParams)),
    new("gobuild1.input", "gobuild1.golden", idempotent),
    new("gobuild2.input", "gobuild2.golden", idempotent),
    new("gobuild3.input", "gobuild3.golden", idempotent),
    new("gobuild4.input", "gobuild4.golden", idempotent),
    new("gobuild5.input", "gobuild5.golden", idempotent),
    new("gobuild6.input", "gobuild6.golden", idempotent),
    new("gobuild7.input", "gobuild7.golden", idempotent)
]);

public static void TestFiles(ж<testing.T> t) {
    t.Parallel();
    foreach (var (_, e) in data) {
        @string source = filepath.Join(dataDir, e.source);
        @string golden = filepath.Join(dataDir, e.golden);
        checkMode mode = e.mode;
        t.Run(e.source, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            check(tΔ1, source, golden, mode);
            // TODO(gri) check that golden is idempotent
            //check(t, golden, golden, e.mode)
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ => ref srcᶜᵇ;
private static readonly @string srcᶜᵇ = """
// comment 1
	// comment 2
	// comment 3
	package main
	
"""u8.Raw();

// TestLineComments, using a simple test case, checks that consecutive line
// comments are properly terminated with a newline even if the AST position
// information is incorrect.
public static void TestLineComments(ж<testing.T> t) {
    @string src = srcᶜ;

    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, ""u8, src, parser.ParseComments);
    if (err != nil) {
        throw panic(err); // error in test
    }

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    fset = token.NewFileSet(); // use the wrong file set
    Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f.OrTypedNil());

    nint nlines = 0;
    foreach (var (_, ch) in buf.Bytes()) {
        if (ch == '\n') {
            nlines++;
        }
    }

    const nint expected = 3;
    if (nlines < expected) {
        t.Errorf("got %d, expected %d\n", nlines, (nint)(expected));
        t.Errorf("result:\n%s", buf.Bytes());
    }
}

// Verify that the printer can be invoked during initialization.
[GoInit] internal static void init() {
    @string name = "foobar";
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, Ꮡ(new ast.Ident(Name: name))); if (err != nil) {
            throw panic(err); // error in test
        }
    }
    // in debug mode, the result contains additional information;
    // ignore it
    {
        @string s = Ꮡbuf.String(); if (!debug && s != name) {
            throw panic("got " + s + ", want " + name);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedIllegalProgramˢ = (@string)"expected illegal program"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ1 => ref srcᶜ1ᵇ;
private static readonly @string srcᶜ1ᵇ = "package p\n("u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string resᶜ => ref resᶜᵇ;
private static readonly @string resᶜᵇ = "package p\nBadDecl\n"u8;

// Verify that the printer doesn't crash if the AST contains BadXXX nodes.
public static void TestBadNodes(ж<testing.T> t) {
    @string src = srcᶜ1;
    @string res = resᶜ;
    var (f, err) = parser.ParseFile(fset, ""u8, src, parser.ParseComments);
    if (err == nil) {
        t.Error(expectedIllegalProgramˢ); // error in test
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f.OrTypedNil());
    if (Ꮡbuf.String() != res) {
        t.Errorf("got %q, expected %q", Ꮡbuf.String(), res);
    }
}

// testComment verifies that f can be parsed again after printing it
// with its first comment set to comment at any possible source offset.
internal static void testComment(ж<testing.T> t, ж<ast.File> Ꮡf, nint srclen, ж<ast.Comment> Ꮡcomment) {
    ref var f = ref Ꮡf.DerefOrNull();
    ref var comment = ref Ꮡcomment.DerefOrNull();

    f.Comments[0].List[0] = Ꮡcomment;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint offs = 0; offs <= srclen; offs++) {
        buf.Reset();
        // Printing f should result in a correct program no
        // matter what the (incorrect) comment position is.
        {
            var err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, Ꮡf.OrTypedNil()); if (err != nil) {
                t.Error(err);
            }
        }
        {
            var (_, err) = parser.ParseFile(fset, ""u8, buf.Bytes(), 0); if (err != nil) {
                t.Fatalf("incorrect program for pos = %d:\n%s", comment.Slash, Ꮡbuf.String());
            }
        }
        // Position information is just an offset.
        // Move comment one byte down in the source.
        comment.Slash++;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedOffset1ˢ = (@string)"expected offset 1"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ2 => ref srcᶜ2ᵇ;
private static readonly @string srcᶜ2ᵇ = """

// first comment - text and position changed by test
package p
import "fmt"
const pi = 3.14 // rough circle
var (
	x, y, z int = 1, 2, 3
	u, v float64
)
func fibo(n int) {
	if n < 2 {
		return n /* seed values */
	}
	return fibo(n-1) + fibo(n-2)
}

"""u8.Raw();

// Verify that the printer produces a correct program
// even if the position information of comments introducing newlines
// is incorrect.
public static void TestBadComments(ж<testing.T> t) {
    t.Parallel();
    @string src = srcᶜ2;

    var (f, err) = parser.ParseFile(fset, ""u8, src, parser.ParseComments);
    if (err != nil) {
        t.Error(err); // error in test
    }

    var comment = f.Comments[0].List[0];
    ref var pos = ref heap<tokenꓸPos>(comment.Pos(), out var Ꮡpos);
    if (fset.PositionFor(pos, false /* absolute position */).Offset != 1) {
        t.Error(expectedOffset1ˢ); // error in test
    }

    testComment(t, f, len(src), Ꮡ(new ast.Comment(Slash: pos, Text: "//-style comment")));
    testComment(t, f, len(src), Ꮡ(new ast.Comment(Slash: pos, Text: "/*-style comment */")));
    testComment(t, f, len(src), Ꮡ(new ast.Comment(Slash: pos, Text: "/*-style \n comment */")));
    testComment(t, f, len(src), Ꮡ(new ast.Comment(Slash: pos, Text: "/*-style comment \n\n\n */")));
}

internal partial struct visitor /*chan ж<ast.Ident>*/;

internal static ast.Visitor /*w*/ Visit(this visitor v, ast.Node n) {
    {
        var (ident, ok) = n._<ж<ast.Ident>>(ᐧ); if (ok) {
            v.ᐸꟷ(ident);
        }
    }
    return v;
}

// idents is an iterator that returns all idents in f via the result channel.
internal static partial /*<-*/channel<ж<ast.Ident>> idents(ж<ast.File> f) {
    var v = new visitor(0);
    var vʗ1 = v;
    goǃ(() => {
        ast.Walk(vʗ1, new ast.FileжNode(f));
        close<ж<ast.Ident>>(vʗ1);
    });
    return v;
}

// identCount returns the number of identifiers found in f.
internal static nint identCount(ж<ast.File> f) {
    nint n = 0;
    foreach (var _ᴛ1 in idents(f)) {
        n++;
    }
    return n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string srcˢ => ref srcˢᵇ;
private static readonly @string srcˢᵇ = "src"u8;
internal static readonly object gotNoIdentsˢ = (@string)"got no idents"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ3 => ref srcᶜ3ᵇ;
private static readonly @string srcᶜ3ᵇ = """

package p
import ( "go/printer"; "math" )
const pi = 3.14; var x = 0
type t struct{ x, y, z int; u, v, w float32 }
func (t *t) foo(a, b, c int) int {
	return a*t.x + b*t.y +
		// two extra lines here
		// ...
		c*t.z
}

"""u8.Raw();

// Verify that the SourcePos mode emits correct //line directives
// by testing that position information for matching identifiers
// is maintained.
public static void TestSourcePos(ж<testing.T> t) {
    @string src = srcᶜ3;

    // parse original
    var (f1, err) = parser.ParseFile(fset, srcˢ, src, parser.ParseComments);
    if (err != nil) {
        t.Fatal(err);
    }

    // pretty-print original
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    err = (Ꮡ(new Config(Mode: (global::go.go.printer_package.Mode)(UseSpaces | SourcePos), Tabwidth: 8))).Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f1.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }

    // parse pretty printed original
    // (//line directives must be interpreted even w/o parser.ParseComments set)
    (var f2, err) = parser.ParseFile(fset, ""u8, buf.Bytes(), 0);
    if (err != nil) {
        t.Fatalf("%s\n%s", err, buf.Bytes());
    }

    // At this point the position information of identifiers in f2 should
    // match the position information of corresponding identifiers in f1.

    // number of identifiers must be > 0 (test should run) and must match
    nint n1 = identCount(f1);
    nint n2 = identCount(f2);
    if (n1 == 0) {
        t.Fatal(gotNoIdentsˢ);
    }
    if (n2 != n1) {
        t.Errorf("got %d idents; want %d", n2, n1);
    }

    // verify that all identifiers have correct line information
    var i2range = idents(f2);
    foreach (var i1 in idents(f1)) {
        var i2 = ᐸꟷ(i2range);

        if (i2.Name != i1.Name) {
            t.Errorf("got ident %s; want %s", i2.Name, i1.Name);
        }

        // here we care about the relative (line-directive adjusted) positions
        nint l1 = fset.Position(i1.Pos()).Line;
        nint l2 = fset.Position(i2.Pos()).Line;
        if (l2 != l1) {
            t.Errorf("got line %d; want %d for %s", l2, l1, i1.Name);
        }
    }

    if (t.Failed()) {
        t.Logf("\n%s", buf.Bytes());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string srcGoˢ => ref srcGoˢᵇ;
private static readonly @string srcGoˢᵇ = "src.go"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string origᶜ => ref origᶜᵇ;
private static readonly @string origᶜᵇ = """

package p   // line 2
func f() {} // line 3

var x, y, z int


func g() { // line 8
}

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = """
//line src.go:2
package p

//line src.go:3
func f() {}

var x, y, z int

//line src.go:8
func g() {
}

"""u8.Raw();

// Verify that the SourcePos mode doesn't emit unnecessary //line directives
// before empty lines.
public static void TestIssue5945(ж<testing.T> t) {
    @string orig = origᶜ;

    @string want = wantᶜ;

    // parse original
    var (f1, err) = parser.ParseFile(fset, srcGoˢ, orig, 0);
    if (err != nil) {
        t.Fatal(err);
    }

    // pretty-print original
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    err = (Ꮡ(new Config(Mode: (global::go.go.printer_package.Mode)(UseSpaces | SourcePos), Tabwidth: 8))).Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f1.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }
    @string got = Ꮡbuf.String();

    // compare original with desired output
    if (got != want) {
        t.Errorf("got:\n%s\nwant:\n%s\n", got, want);
    }
}

internal static slice<@string> decls = slice<@string>([
    @"import ""fmt""",
    "const pi = 3.1415\nconst e = 2.71828\n\nvar x = pi",
    "func sum(x, y int) int\t{ return x + y }"
]);

public static void TestDeclLists(ж<testing.T> t) {
    foreach (var (_, src) in decls) {
        var (@file, err) = parser.ParseFile(fset, ""u8, "package p;" + src, parser.ParseComments);
        if (err != nil) {
            throw panic(err); // error in test
        }

        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, @file.Decls); // only print declarations
        if (err != nil) {
            throw panic(err); // error in test
        }

        @string @out = Ꮡbuf.String();
        if (@out != src) {
            t.Errorf("\ngot : %q\nwant: %q\n", @out, src);
        }
    }
}

internal static slice<@string> stmts = slice<@string>([
    "i := 0",
    "select {}\nvar a, b = 1, 2\nreturn a + b",
    "go f()\ndefer func() {}()"
]);

public static void TestStmtLists(ж<testing.T> t) {
    foreach (var (_, src) in stmts) {
        var (@file, err) = parser.ParseFile(fset, ""u8, "package p; func _() {" + src + "}", parser.ParseComments);
        if (err != nil) {
            throw panic(err); // error in test
        }

        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, @file.Decls[0]._<ж<ast.FuncDecl>>().Body.List); // only print statements
        if (err != nil) {
            throw panic(err); // error in test
        }

        @string @out = Ꮡbuf.String();
        if (@out != src) {
            t.Errorf("\ngot : %q\nwant: %q\n", @out, src);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string filenameᶜ1 => ref filenameᶜ1ᵇ;
private static readonly @string filenameᶜ1ᵇ = "printer.go"u8;

public static void TestBaseIndent(ж<testing.T> t) {
    t.Parallel();
    // The testfile must not contain multi-line raw strings since those
    // are not indented (because their values must not change) and make
    // this test fail.
    @string filename = filenameᶜ1;
    var (src, err) = os.ReadFile(filename);
    if (err != nil) {
        throw panic(err); // error in test
    }

    (var @file, err) = parser.ParseFile(fset, filename, src, 0);
    if (err != nil) {
        throw panic(err); // error in test
    }

    for (nint indentᴛ1 = 0; indentᴛ1 < 4; indentᴛ1++) {
        ref var indent = ref heap<nint>(out var Ꮡindent);
        indent = indentᴛ1;
        var fileʗ1 = @file;
        var indentʗ1 = indent;
        t.Run(fmt.Sprint(indent), (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            (Ꮡ(new Config(Tabwidth: tabwidth, Indent: indentʗ1))).Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, fileʗ1.OrTypedNil());
            // all code must be indented by at least 'indent' tabs
            var lines = bytes.Split(buf.Bytes(), slice<byte>([(byte)'\n']));
            foreach (var (i, line) in lines) {
                if (len(line) == 0) {
                    continue; // empty lines don't have indentation
                }
                nint n = 0;
                foreach (var (j, b) in line) {
                    if (b != '\t') {
                        // end of indentation
                        n = j;
                        break;
                    }
                }
                if (n < indentʗ1) {
                    tΔ1.Errorf("line %d: got only %d tabs; want at least %d: %q", i, n, indentʗ1, line);
                }
            }
        });
        indentᴛ1 = indent;
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ1 => ref wantᶜ1ᵇ;
private static readonly @string wantᶜ1ᵇ = """
package p

func f()

"""u8.Raw();

// TestFuncType tests that an ast.FuncType with a nil Params field
// can be printed (per go/ast specification). Test case for issue 3870.
public static void TestFuncType(ж<testing.T> t) {
    var src = Ꮡ(new ast.File(
        Name: Ꮡ(new ast.Ident(Name: "p")),
        Decls: slice<ast.Decl>([new ast.FuncDeclжDecl(Ꮡ(new ast.FuncDecl(
            Name: Ꮡ(new ast.Ident(Name: "f")),
            Type: Ꮡ(new ast.FuncType())
        )))
        ])
    ));

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, src.OrTypedNil()); if (err != nil) {
            t.Fatal(err);
        }
    }
    @string got = Ꮡbuf.String();

    @string want = wantᶜ1;

    if (got != want) {
        t.Fatalf("got:\n%s\nwant:\n%s\n", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string chanIntNilˢ => ref chanIntNilˢᵇ;
private static readonly @string chanIntNilˢᵇ = @"<-(<-chan int)(nil)"u8;

// TestChanType tests that the tree for <-(<-chan int), without
// ParenExpr, is correctly formatted with parens.
// Test case for issue #63362.
public static void TestChanType(ж<testing.T> t) {
    var expr = Ꮡ(new ast.UnaryExpr(
        Op: token.ARROW,
        X: new ast.CallExprжExpr(Ꮡ(new ast.CallExpr(
            Fun: new ast.ChanTypeжExpr(Ꮡ(new ast.ChanType(
                Dir: ast.RECV,
                Value: new ast.IdentжExpr(Ꮡ(new ast.Ident(Name: "int")))
            ))),
            Args: slice<ast.Expr>([new ast.IdentжExpr(Ꮡ(new ast.Ident(Name: "nil")))])
        )))
    ));
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, expr.OrTypedNil()); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        @string got = Ꮡbuf.String();
        @string want = chanIntNilˢ; if (got != want) {
            t.Fatalf("got:\n%s\nwant:\n%s\n", got, want);
        }
    }
}

internal partial struct limitWriter {
    internal nint remaining;
    internal nint errCount;
}

internal static (nint n, error err) Write(this ref limitWriter l, slice<byte> buf) {
    nint n = default;
    error err = default;

    n = len(buf);
    if (n >= l.remaining) {
        n = l.remaining;
        err = io.EOF;
        l.errCount++;
    }
    l.remaining -= n;
    return (n, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object writesContinuedAfterˢ = (@string)"Writes continued after first error returned"u8;
internal static readonly object expectedErrWhenErrCount0ˢ = (@string)"Expected err when errCount != 0"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string filenameᶜ2 => ref filenameᶜ2ᵇ;
private static readonly @string filenameᶜ2ᵇ = "printer.go"u8;

// Test whether the printer stops writing after the first error
public static void TestWriteErrors(ж<testing.T> t) {
    t.Parallel();
    @string filename = filenameᶜ2;
    var (src, err) = os.ReadFile(filename);
    if (err != nil) {
        throw panic(err); // error in test
    }
    (var @file, err) = parser.ParseFile(fset, filename, src, 0);
    if (err != nil) {
        throw panic(err); // error in test
    }
    for (nint iᴛ1 = 0; iᴛ1 < 20; iᴛ1++) {
        ref var i = ref heap<nint>(out var Ꮡi);
        i = iᴛ1;
        var lw = Ꮡ(new limitWriter(remaining: i));
        var errΔ1 = (Ꮡ(new Config(Mode: RawFormat))).Fprint(new printer_internal_test_package.limitWriterжWriter(lw), fset, @file.OrTypedNil());
        if (lw.errCount > 1) {
            t.Fatal(writesContinuedAfterˢ);
        }
        // We expect errCount be 1 iff err is set
        if ((lw.errCount != 0) != (errΔ1 != nil)) {
            t.Fatal(expectedErrWhenErrCount0ˢ);
        }
        iᴛ1 = i;
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ4 => ref srcᶜ4ᵇ;
private static readonly @string srcᶜ4ᵇ = """

package p
func _() {}

"""u8.Raw();

// TestX is a skeleton test that can be filled in for debugging one-off cases.
// Do not remove.
public static void TestX(ж<testing.T> t) {
    @string src = srcᶜ4;
    var (_, err) = format(slice<byte>(src), 0);
    if (err != nil) {
        t.Error(err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string inputGoˢ => ref inputGoˢᵇ;
private static readonly @string inputGoˢᵇ = "input.go"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string inputᶜ => ref inputᶜᵇ;
private static readonly @string inputᶜᵇ = """
package main

func foo() {
	// comment inside func
}

// leading comment
type bar int // comment2


"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string fooᶜ => ref fooᶜᵇ;
private static readonly @string fooᶜᵇ = """
func foo() {
	// comment inside func
}
"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string barᶜ => ref barᶜᵇ;
private static readonly @string barᶜᵇ = """
// leading comment
type bar int	// comment2

"""u8.Raw();

public static void TestCommentedNode(ж<testing.T> t) {
    @string input = inputᶜ;

    @string foo = fooᶜ;

    @string bar = barᶜ;

    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, inputGoˢ, input, parser.ParseComments);
    if (err != nil) {
        t.Fatal(err);
    }

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);

    err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, Ꮡ(new CommentedNode(Node: f.Decls[0], Comments: f.Comments)));
    if (err != nil) {
        t.Fatal(err);
    }

    if (Ꮡbuf.String() != foo) {
        t.Errorf("got %q, want %q", Ꮡbuf.String(), foo);
    }

    buf.Reset();

    err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, Ꮡ(new CommentedNode(Node: f.Decls[1], Comments: f.Comments)));
    if (err != nil) {
        t.Fatal(err);
    }

    if (Ꮡbuf.String() != bar) {
        t.Errorf("got %q, want %q", Ꮡbuf.String(), bar);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ5 => ref srcᶜ5ᵇ;
private static readonly @string srcᶜ5ᵇ = "package p\t/*\r/1\r*\r/2*\r\r\r\r/3*\r\r+\r\r/4*/\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ2 => ref wantᶜ2ᵇ;
private static readonly @string wantᶜ2ᵇ = "package p\t/*/1*\r/2*\r/3*+/4*/\n"u8;

public static void TestIssue11151(ж<testing.T> t) {
    @string src = srcᶜ5;
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, ""u8, src, parser.ParseComments);
    if (err != nil) {
        t.Fatal(err);
    }

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f.OrTypedNil());
    @string got = Ꮡbuf.String();
    @string want = wantᶜ2; // \r following opening /* should be stripped
    if (got != want) {
        t.Errorf("\ngot : %q\nwant: %q", got, want);
    }

    // the resulting program must be valid
    (_, err) = parser.ParseFile(fset, ""u8, got, 0);
    if (err != nil) {
        t.Errorf("%v\norig: %q\ngot : %q", err, src, got);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ6 => ref srcᶜ6ᵇ;
private static readonly @string srcᶜ6ᵇ = "package p; var ( a float64; b int )"u8;

// If a declaration has multiple specifications, a parenthesized
// declaration must be printed even if Lparen is token.NoPos.
public static void TestParenthesizedDecl(ж<testing.T> t) {
    // a package with multiple specs in a single declaration
    @string src = srcᶜ6;
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, ""u8, src, 0);
    if (err != nil) {
        t.Fatal(err);
    }

    // print the original package
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }
    @string original = Ꮡbuf.String();

    // now remove parentheses from the declaration
    for (nint i = 0; i != len(f.Decls); i++) {
        f.Decls[i]._<ж<ast.GenDecl>>().Lparen = token.NoPos;
    }
    buf.Reset();
    err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }
    @string noparen = Ꮡbuf.String();

    if (noparen != original) {
        t.Errorf("got %q, want %q", noparen, original);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packageFooFuncFReturnˢ => ref packageFooFuncFReturnˢᵇ;
private static readonly @string packageFooFuncFReturnˢᵇ = """
package foo

func f() {
        return Composite{
                call(),
        }
}
"""u8.Raw();
internal static ref readonly @string returnCallˢ => ref returnCallˢᵇ;
private static readonly @string returnCallˢᵇ = "return call()"u8;

// Verify that we don't print a newline between "return" and its results, as
// that would incorrectly cause a naked return.
public static void TestIssue32854(ж<testing.T> t) {
    @string src = packageFooFuncFReturnˢ;
    var fset = token.NewFileSet();
    var (@file, err) = parser.ParseFile(fset, ""u8, src, 0);
    if (err != nil) {
        throw panic(err);
    }

    // Replace the result with call(), which is on the next line.
    var fd = @file.Decls[0]._<ж<ast.FuncDecl>>();
    var ret = fd.Body.List[0]._<ж<ast.ReturnStmt>>();
    ret.Results[0] = ret.Results[0]._<ж<ast.CompositeLit>>().Elts[0];

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var errΔ1 = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, ret.OrTypedNil()); if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
    }
    @string want = returnCallˢ;
    {
        @string got = Ꮡbuf.String(); if (got != want) {
            t.Fatalf("got %q, want %q", got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooBarBarGoˢ => ref fooBarBarGoˢᵇ;
private static readonly @string fooBarBarGoˢᵇ = "foo\nbar/bar.go"u8;
internal static ref readonly @string packageBarˢ => ref packageBarˢᵇ;
private static readonly @string packageBarˢᵇ = @"package bar"u8;

public static void TestSourcePosNewline(ж<testing.T> t) {
    // We don't provide a syntax for escaping or unescaping characters in line
    // directives (see https://go.dev/issue/24183#issuecomment-372449628).
    // As a result, we cannot write a line directive with the correct path for a
    // filename containing newlines. We should return an error rather than
    // silently dropping or mangling it.

    @string fname = fooBarBarGoˢ;
    @string src = packageBarˢ;
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, fname, src, (parser.Mode)((parser.Mode)(parser.ParseComments | parser.AllErrors) | parser.SkipObjectResolution));
    if (err != nil) {
        t.Fatal(err);
    }

    var cfg = Ꮡ(new Config(
        Mode: SourcePos, // emit line comments
        Tabwidth: 8
    ));
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var errΔ1 = cfg.Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f.OrTypedNil()); if (errΔ1 == nil) {
            t.Errorf("Fprint did not error for source file path containing newline");
        }
    }
    if (buf.Len() != 0) {
        t.Errorf("unexpected Fprint output:\n%s", buf.Bytes());
    }
}

// TestEmptyDecl tests that empty decls for const, var, import are printed with
// valid syntax e.g "var ()" instead of just "var", which is invalid and cannot
// be parsed.
public static void TestEmptyDecl(ж<testing.T> t) {
    // issue 63566
    foreach (var (_, vᴛ1) in slice<token.Token>([token.IMPORT, token.CONST, token.TYPE, token.VAR])) {
        ref var tok = ref heap(new token.Token(), out var Ꮡtok);
        tok = vᴛ1;

        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), token.NewFileSet(), Ꮡ(new ast.GenDecl(Tok: tok)));
        @string got = Ꮡbuf.String();
        @string want = tok.String() + " ()"u8;
        if (got != want) {
            t.Errorf("got %q, want %q", got, want);
        }
    }
}

} // end printer_internal_test_package
