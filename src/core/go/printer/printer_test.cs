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
using diff = @internal.diff_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using testing = testing_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using fs = global::go.io.fs_package;
using global::go.go;
using path;
using static global::go.go.printer_package;

partial class printer_internal_test_package {

internal static readonly @string dataDir = "testdata"u8;
internal static UntypedInt tabwidth => 8;

internal static ж<bool> update = flag.Bool("update"u8, false, "update golden files"u8);

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
    if (err != default!) {
        return (default!, fmt.Errorf("parse: %s\n%s"u8, err, src));
    }
    // filter exports if necessary
    if ((checkMode)(mode & export) != 0) {
        ast.FileExports(f); // ignore result
        f.Value.Comments = default!; // don't print comments that are not in AST
    }
    // determine printer configuration
    ref var cfg = ref heap<global::go.go.printer_package.Config>(out var Ꮡcfg);
    cfg = new Config(Tabwidth: tabwidth);
    if ((checkMode)(mode & rawFormat) != 0) {
        cfg.Mode |= (global::go.go.printer_package.Mode)(RawFormat);
    }
    if ((checkMode)(mode & normNumber) != 0) {
        cfg.Mode |= (global::go.go.printer_package.Mode)(normalizeNumbers);
    }
    // print AST
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var errΔ1 = Ꮡcfg.Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f.OrTypedNil()); if (errΔ1 != default!) {
            return (default!, fmt.Errorf("print: %s"u8, errΔ1));
        }
    }
    // make sure formatted output is syntactically correct
    var res = buf.Bytes();
    {
        var (_, errΔ2) = parser.ParseFile(fset, ""u8, res, parser.ParseComments); if (errΔ2 != default!) {
            return (default!, fmt.Errorf("re-parse: %s\n%s"u8, errΔ2, buf.Bytes()));
        }
    }
    return (res, default!);
}

// lineAt returns the line in text starting at offset offs.
internal static slice<byte> lineAt(slice<byte> text, nint offs) {
    nint i = offs;
    while (i < len(text) && text[i] != (rune)'\n') {
        i++;
    }
    return text.slice(offs, i);
}

// checkEqual compares a and b.
internal static error checkEqual(@string aname, @string bname, slice<byte> a, slice<byte> b) {
    if (bytes.Equal(a, b)) {
        return default!;
    }
    return errors.New(((@string)diff.Diff(aname, a, bname, b)));
}

internal static void runcheck(ж<testing.T> Ꮡt, @string source, @string golden, checkMode mode) {
    var (src, err) = os.ReadFile(source);
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    (var res, err) = format(src, mode);
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    // update golden files if necessary
    if (update.Value) {
        {
            var errΔ1 = os.WriteFile(golden, res, 420); if (errΔ1 != default!) {
                Ꮡt.Error(errΔ1);
            }
        }
        return;
    }
    // get golden
    (var gld, err) = os.ReadFile(golden);
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    // formatted source and golden must be the same
    {
        var errΔ2 = checkEqual(fmt.Sprintf("format(%v)"u8, source), golden, res, gld); if (errΔ2 != default!) {
            Ꮡt.Error(errΔ2);
            return;
        }
    }
    if ((checkMode)(mode & idempotent) != 0) {
        // formatting golden must be idempotent
        // (This is very difficult to achieve in general and for now
        // it is only checked for files explicitly marked as such.)
        (res, err) = format(gld, mode);
        if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
        {
            var errΔ3 = checkEqual(golden, fmt.Sprintf("format(%s)"u8, golden), gld, res); if (errΔ3 != default!) {
                Ꮡt.Errorf("golden is not idempotent: %s"u8, errΔ3);
            }
        }
    }
}

internal static partial void check(ж<testing.T> Ꮡt, @string source, @string golden, checkMode mode) {
    // run the test
    var cc = new channel<nint>(1);
    var ccʗ1 = cc;
    goǃ(() => {
        runcheck(Ꮡt, source, golden, mode);
        ccʗ1.ᐸꟷ(0);
    });
    // wait with timeout
    var selᴛ1 = time.After((time.Duration)(10000000000L));
    var selᴛ2 = cc;
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out _): {
        Ꮡt.Errorf("%s: running too slowly"u8, // plenty of a safety margin, even for very slow machines
 // test running past time out
 source);
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out _): {
        break;
    }}
}

// test finished within allotted time margin
internal partial struct entry {
    internal @string source, golden;
    internal checkMode mode;
}

// Use go test -update to create/update the respective golden files.
internal static slice<entry> data = new entry[]{
    new("empty.input"u8, "empty.golden"u8, idempotent),
    new("comments.input"u8, "comments.golden"u8, 0),
    new("comments.input"u8, "comments.x"u8, export),
    new("comments2.input"u8, "comments2.golden"u8, idempotent),
    new("alignment.input"u8, "alignment.golden"u8, idempotent),
    new("linebreaks.input"u8, "linebreaks.golden"u8, idempotent),
    new("expressions.input"u8, "expressions.golden"u8, idempotent),
    new("expressions.input"u8, "expressions.raw"u8, (checkMode)(rawFormat | idempotent)),
    new("declarations.input"u8, "declarations.golden"u8, 0),
    new("statements.input"u8, "statements.golden"u8, 0),
    new("slow.input"u8, "slow.golden"u8, idempotent),
    new("complit.input"u8, "complit.x"u8, export),
    new("go2numbers.input"u8, "go2numbers.golden"u8, idempotent),
    new("go2numbers.input"u8, "go2numbers.norm"u8, (checkMode)(normNumber | idempotent)),
    new("generics.input"u8, "generics.golden"u8, (checkMode)(idempotent | allowTypeParams)),
    new("gobuild1.input"u8, "gobuild1.golden"u8, idempotent),
    new("gobuild2.input"u8, "gobuild2.golden"u8, idempotent),
    new("gobuild3.input"u8, "gobuild3.golden"u8, idempotent),
    new("gobuild4.input"u8, "gobuild4.golden"u8, idempotent),
    new("gobuild5.input"u8, "gobuild5.golden"u8, idempotent),
    new("gobuild6.input"u8, "gobuild6.golden"u8, idempotent),
    new("gobuild7.input"u8, "gobuild7.golden"u8, idempotent)
}.slice();

public static void TestFiles(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, e) in data) {
        @string source = filepath.Join(dataDir, e.source);
        @string golden = filepath.Join(dataDir, e.golden);
        checkMode mode = e.mode;
        Ꮡt.Run(e.source, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            check(tΔ1, source, golden, mode);
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ = """
// comment 1
	// comment 2
	// comment 3
	package main
	
"""u8;

// TODO(gri) check that golden is idempotent
//check(t, golden, golden, e.mode)

// TestLineComments, using a simple test case, checks that consecutive line
// comments are properly terminated with a newline even if the AST position
// information is incorrect.
public static void TestLineComments(ж<testing.T> Ꮡt) {
    @string src = srcᶜ;
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, ""u8, src, parser.ParseComments);
    if (err != default!) {
        throw panic(err); // error in test
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    fset = token.NewFileSet(); // use the wrong file set
    Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f.OrTypedNil());
    nint nlines = 0;
    foreach (var (_, ch) in buf.Bytes()) {
        if (ch == (rune)'\n') {
            nlines++;
        }
    }
    const nint expected = 3;
    if (nlines < expected) {
        Ꮡt.Errorf("got %d, expected %d\n"u8, nlines, (nint)(expected));
        Ꮡt.Errorf("result:\n%s"u8, buf.Bytes());
    }
}

// Verify that the printer can be invoked during initialization.
[GoInit] internal static void init() {
    @string name = "foobar"u8;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, Ꮡ(new ast.Ident(Name: name))); if (err != default!) {
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
internal static readonly @string srcᶜ1 = "package p\n("u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string resᶜ = "package p\nBadDecl\n"u8;

// Verify that the printer doesn't crash if the AST contains BadXXX nodes.
public static void TestBadNodes(ж<testing.T> Ꮡt) {
    @string src = srcᶜ1;
    @string res = resᶜ;
    var (f, err) = parser.ParseFile(fset, ""u8, src, parser.ParseComments);
    if (err == default!) {
        Ꮡt.Error(expectedIllegalProgramˢ); // error in test
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f.OrTypedNil());
    if (Ꮡbuf.String() != res) {
        Ꮡt.Errorf("got %q, expected %q"u8, Ꮡbuf.String(), res);
    }
}

// testComment verifies that f can be parsed again after printing it
// with its first comment set to comment at any possible source offset.
internal static void testComment(ж<testing.T> Ꮡt, ж<ast.File> Ꮡf, nint srclen, ж<ast.Comment> Ꮡcomment) {
    ref var f = ref Ꮡf.DerefOrNull();
    ref var comment = ref Ꮡcomment.DerefOrNull();

    f.Comments[0].Value.List[0] = Ꮡcomment;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint offs = 0; offs <= srclen; offs++) {
        buf.Reset();
        // Printing f should result in a correct program no
        // matter what the (incorrect) comment position is.
        {
            var err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, Ꮡf.OrTypedNil()); if (err != default!) {
                Ꮡt.Error(err);
            }
        }
        {
            var (_, err) = parser.ParseFile(fset, ""u8, buf.Bytes(), 0); if (err != default!) {
                Ꮡt.Fatalf("incorrect program for pos = %d:\n%s"u8, comment.Slash, Ꮡbuf.String());
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
internal static readonly @string srcᶜ2 = """

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

"""u8;

// Verify that the printer produces a correct program
// even if the position information of comments introducing newlines
// is incorrect.
public static void TestBadComments(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    @string src = srcᶜ2;
    var (f, err) = parser.ParseFile(fset, ""u8, src, parser.ParseComments);
    if (err != default!) {
        Ꮡt.Error(err); // error in test
    }
    var comment = (~(~f).Comments[0]).List[0];
    ref var pos = ref heap<tokenꓸPos>(out var Ꮡpos);
    pos = comment.Pos();
    if (fset.PositionFor(pos, false).Offset != 1) {
        /* absolute position */
        Ꮡt.Error(expectedOffset1ˢ); // error in test
    }
    testComment(Ꮡt, f, len(src), Ꮡ(new ast.Comment(Slash: pos, Text: "//-style comment"u8)));
    testComment(Ꮡt, f, len(src), Ꮡ(new ast.Comment(Slash: pos, Text: "/*-style comment */"u8)));
    testComment(Ꮡt, f, len(src), Ꮡ(new ast.Comment(Slash: pos, Text: "/*-style \n comment */"u8)));
    testComment(Ꮡt, f, len(src), Ꮡ(new ast.Comment(Slash: pos, Text: "/*-style comment \n\n\n */"u8)));
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
internal static partial /*<-*/channel<ж<ast.Ident>> idents(ж<ast.File> Ꮡf) {
    var v = new visitor(0);
    var vʗ1 = v;
    goǃ(() => {
        ast.Walk(vʗ1, new ast.FileжNode(Ꮡf));
        close<ж<ast.Ident>>(vʗ1);
    });
    return v;
}

// identCount returns the number of identifiers found in f.
internal static nint identCount(ж<ast.File> Ꮡf) {
    nint n = 0;
    foreach (var _ᴛ1 in idents(Ꮡf)) {
        n++;
    }
    return n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string srcˢ = "src"u8;
internal static readonly object gotNoIdentsˢ = (@string)"got no idents"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ3 = """

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

"""u8;

// Verify that the SourcePos mode emits correct //line directives
// by testing that position information for matching identifiers
// is maintained.
public static void TestSourcePos(ж<testing.T> Ꮡt) {
    @string src = srcᶜ3;
    // parse original
    var (f1, err) = parser.ParseFile(fset, srcˢ, src, parser.ParseComments);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // pretty-print original
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    err = (Ꮡ(new Config(Mode: (global::go.go.printer_package.Mode)(UseSpaces | SourcePos), Tabwidth: 8))).Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f1.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // parse pretty printed original
    // (//line directives must be interpreted even w/o parser.ParseComments set)
    (var f2, err) = parser.ParseFile(fset, ""u8, buf.Bytes(), 0);
    if (err != default!) {
        Ꮡt.Fatalf("%s\n%s"u8, err, buf.Bytes());
    }
    // At this point the position information of identifiers in f2 should
    // match the position information of corresponding identifiers in f1.
    // number of identifiers must be > 0 (test should run) and must match
    nint n1 = identCount(f1);
    nint n2 = identCount(f2);
    if (n1 == 0) {
        Ꮡt.Fatal(gotNoIdentsˢ);
    }
    if (n2 != n1) {
        Ꮡt.Errorf("got %d idents; want %d"u8, n2, n1);
    }
    // verify that all identifiers have correct line information
    var i2range = idents(f2);
    foreach (var i1 in idents(f1)) {
        var i2 = ᐸꟷ(i2range);
        if ((~i2).Name != (~i1).Name) {
            Ꮡt.Errorf("got ident %s; want %s"u8, (~i2).Name, (~i1).Name);
        }
        // here we care about the relative (line-directive adjusted) positions
        nint l1 = fset.Position(i1.Pos()).Line;
        nint l2 = fset.Position(i2.Pos()).Line;
        if (l2 != l1) {
            Ꮡt.Errorf("got line %d; want %d for %s"u8, l2, l1, (~i1).Name);
        }
    }
    if (Ꮡt.Failed()) {
        Ꮡt.Logf("\n%s"u8, buf.Bytes());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string srcGoˢ = "src.go"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string origᶜ = """

package p   // line 2
func f() {} // line 3

var x, y, z int


func g() { // line 8
}

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = """
//line src.go:2
package p

//line src.go:3
func f() {}

var x, y, z int

//line src.go:8
func g() {
}

"""u8;

// Verify that the SourcePos mode doesn't emit unnecessary //line directives
// before empty lines.
public static void TestIssue5945(ж<testing.T> Ꮡt) {
    @string orig = origᶜ;
    @string want = wantᶜ;
    // parse original
    var (f1, err) = parser.ParseFile(fset, srcGoˢ, orig, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // pretty-print original
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    err = (Ꮡ(new Config(Mode: (global::go.go.printer_package.Mode)(UseSpaces | SourcePos), Tabwidth: 8))).Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f1.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string got = Ꮡbuf.String();
    // compare original with desired output
    if (got != want) {
        Ꮡt.Errorf("got:\n%s\nwant:\n%s\n"u8, got, want);
    }
}

internal static slice<@string> decls = new @string[]{
    @"import ""fmt"""u8,
    "const pi = 3.1415\nconst e = 2.71828\n\nvar x = pi"u8,
    "func sum(x, y int) int\t{ return x + y }"u8
}.slice();

public static void TestDeclLists(ж<testing.T> Ꮡt) {
    foreach (var (_, src) in decls) {
        var (@file, err) = parser.ParseFile(fset, ""u8, "package p;" + src, parser.ParseComments);
        if (err != default!) {
            throw panic(err); // error in test
        }
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, (~@file).Decls); // only print declarations
        if (err != default!) {
            throw panic(err); // error in test
        }
        @string @out = Ꮡbuf.String();
        if (@out != src) {
            Ꮡt.Errorf("\ngot : %q\nwant: %q\n"u8, @out, src);
        }
    }
}

internal static slice<@string> stmts = new @string[]{
    "i := 0"u8,
    "select {}\nvar a, b = 1, 2\nreturn a + b"u8,
    "go f()\ndefer func() {}()"u8
}.slice();

public static void TestStmtLists(ж<testing.T> Ꮡt) {
    foreach (var (_, src) in stmts) {
        var (@file, err) = parser.ParseFile(fset, ""u8, "package p; func _() {" + src + "}", parser.ParseComments);
        if (err != default!) {
            throw panic(err); // error in test
        }
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, (~(~(~@file).Decls[0]._<ж<ast.FuncDecl>>()).Body).List); // only print statements
        if (err != default!) {
            throw panic(err); // error in test
        }
        @string @out = Ꮡbuf.String();
        if (@out != src) {
            Ꮡt.Errorf("\ngot : %q\nwant: %q\n"u8, @out, src);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string filenameᶜ1 = "printer.go"u8;

public static void TestBaseIndent(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Parallel();
    // The testfile must not contain multi-line raw strings since those
    // are not indented (because their values must not change) and make
    // this test fail.
    @string filename = filenameᶜ1;
    var (src, err) = os.ReadFile(filename);
    if (err != default!) {
        throw panic(err); // error in test
    }
    (var @file, err) = parser.ParseFile(fset, filename, src, 0);
    if (err != default!) {
        throw panic(err); // error in test
    }
    for (nint indent = 0; indent < 4; indent++) {
        ref var indentΔ1 = ref heap<nint>(out var ᏑindentΔ1);
        indentΔ1 = indent;
        var fileʗ1 = @file;
        var indentʗ1 = indentΔ1;
        Ꮡt.Run(fmt.Sprint(indentΔ1), (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            (Ꮡ(new Config(Tabwidth: tabwidth, Indent: indentʗ1))).Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, fileʗ1.OrTypedNil());
            // all code must be indented by at least 'indent' tabs
            var lines = bytes.Split(buf.Bytes(), new byte[]{(rune)'\n'}.slice());
            foreach (var (i, line) in lines) {
                if (len(line) == 0) {
                    continue; // empty lines don't have indentation
                }
                nint n = 0;
                foreach (var (j, b) in line) {
                    if (b != (rune)'\t') {
                        // end of indentation
                        n = j;
                        break;
                    }
                }
                if (n < indentʗ1) {
                    tΔ1.Errorf("line %d: got only %d tabs; want at least %d: %q"u8, i, n, indentʗ1, line);
                }
            }
        });
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ1 = """
package p

func f()

"""u8;

// TestFuncType tests that an ast.FuncType with a nil Params field
// can be printed (per go/ast specification). Test case for issue 3870.
public static void TestFuncType(ж<testing.T> Ꮡt) {
    var src = Ꮡ(new ast.File(
        Name: Ꮡ(new ast.Ident(Name: "p"u8)),
        Decls: new ast.Decl[]{new ast.FuncDeclжDecl(Ꮡ(new ast.FuncDecl(
            Name: Ꮡ(new ast.Ident(Name: "f"u8)),
            Type: Ꮡ(new ast.FuncType(nil))
        )))
        }.slice()
    ));
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, src.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    @string got = Ꮡbuf.String();
    @string want = wantᶜ1;
    if (got != want) {
        Ꮡt.Fatalf("got:\n%s\nwant:\n%s\n"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string chanIntNilˢ = @"<-(<-chan int)(nil)"u8;

// TestChanType tests that the tree for <-(<-chan int), without
// ParenExpr, is correctly formatted with parens.
// Test case for issue #63362.
public static void TestChanType(ж<testing.T> Ꮡt) {
    var expr = Ꮡ(new ast.UnaryExpr(
        Op: token.ARROW,
        X: new ast.CallExprжExpr(Ꮡ(new ast.CallExpr(
            Fun: new ast.ChanTypeжExpr(Ꮡ(new ast.ChanType(
                Dir: ast.RECV,
                Value: new ast.IdentжExpr(Ꮡ(new ast.Ident(Name: "int"u8)))
            ))),
            Args: new ast.Expr[]{new ast.IdentжExpr(Ꮡ(new ast.Ident(Name: "nil"u8)))}.slice()
        )))
    ));
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, expr.OrTypedNil()); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        @string got = Ꮡbuf.String();
        @string want = chanIntNilˢ; if (got != want) {
            Ꮡt.Fatalf("got:\n%s\nwant:\n%s\n"u8, got, want);
        }
    }
}

internal partial struct limitWriter {
    internal nint remaining;
    internal nint errCount;
}

internal static (nint n, error err) Write(this ref limitWriter l, slice<byte> buf) {
    nint n = default!;
    error err = default!;

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
internal static readonly @string filenameᶜ2 = "printer.go"u8;

// Test whether the printer stops writing after the first error
public static void TestWriteErrors(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    @string filename = filenameᶜ2;
    var (src, err) = os.ReadFile(filename);
    if (err != default!) {
        throw panic(err); // error in test
    }
    (var @file, err) = parser.ParseFile(fset, filename, src, 0);
    if (err != default!) {
        throw panic(err); // error in test
    }
    for (nint iᴛ1 = 0; iᴛ1 < 20; iᴛ1++) {
        ref var i = ref heap<nint>(out var Ꮡi);
        i = iᴛ1;
        var lw = Ꮡ(new limitWriter(remaining: i));
        var errΔ1 = (Ꮡ(new Config(Mode: RawFormat))).Fprint(new printer_internal_test_package.limitWriterжWriter(lw), fset, @file.OrTypedNil());
        if ((~lw).errCount > 1) {
            Ꮡt.Fatal(writesContinuedAfterˢ);
        }
        // We expect errCount be 1 iff err is set
        if (((~lw).errCount != 0) != (errΔ1 != default!)) {
            Ꮡt.Fatal(expectedErrWhenErrCount0ˢ);
        }
        iᴛ1 = i;
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ4 = """

package p
func _() {}

"""u8;

// TestX is a skeleton test that can be filled in for debugging one-off cases.
// Do not remove.
public static void TestX(ж<testing.T> Ꮡt) {
    @string src = srcᶜ4;
    var (_, err) = format(slice<byte>(src), 0);
    if (err != default!) {
        Ꮡt.Error(err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string inputGoˢ = "input.go"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string inputᶜ = """
package main

func foo() {
	// comment inside func
}

// leading comment
type bar int // comment2


"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string fooᶜ = """
func foo() {
	// comment inside func
}
"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string barᶜ = """
// leading comment
type bar int	// comment2

"""u8;

public static void TestCommentedNode(ж<testing.T> Ꮡt) {
    @string input = inputᶜ;
    @string foo = fooᶜ;
    @string bar = barᶜ;
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, inputGoˢ, input, parser.ParseComments);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, Ꮡ(new CommentedNode(Node: (~f).Decls[0], Comments: (~f).Comments)));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (Ꮡbuf.String() != foo) {
        Ꮡt.Errorf("got %q, want %q"u8, Ꮡbuf.String(), foo);
    }
    buf.Reset();
    err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, Ꮡ(new CommentedNode(Node: (~f).Decls[1], Comments: (~f).Comments)));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (Ꮡbuf.String() != bar) {
        Ꮡt.Errorf("got %q, want %q"u8, Ꮡbuf.String(), bar);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ5 = "package p\t/*\r/1\r*\r/2*\r\r\r\r/3*\r\r+\r\r/4*/\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ2 = "package p\t/*/1*\r/2*\r/3*+/4*/\n"u8;

public static void TestIssue11151(ж<testing.T> Ꮡt) {
    @string src = srcᶜ5;
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, ""u8, src, parser.ParseComments);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f.OrTypedNil());
    @string got = Ꮡbuf.String();
    @string want = wantᶜ2;                    // \r following opening /* should be stripped
    if (got != want) {
        Ꮡt.Errorf("\ngot : %q\nwant: %q"u8, got, want);
    }
    // the resulting program must be valid
    (_, err) = parser.ParseFile(fset, ""u8, got, 0);
    if (err != default!) {
        Ꮡt.Errorf("%v\norig: %q\ngot : %q"u8, err, src, got);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ6 = "package p; var ( a float64; b int )"u8;

// If a declaration has multiple specifications, a parenthesized
// declaration must be printed even if Lparen is token.NoPos.
public static void TestParenthesizedDecl(ж<testing.T> Ꮡt) {
    // a package with multiple specs in a single declaration
    @string src = srcᶜ6;
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, ""u8, src, 0);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // print the original package
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string original = Ꮡbuf.String();
    // now remove parentheses from the declaration
    for (nint i = 0; i != len((~f).Decls); i++) {
        (~f).Decls[i]._<ж<ast.GenDecl>>().Value.Lparen = token.NoPos;
    }
    buf.Reset();
    err = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    @string noparen = Ꮡbuf.String();
    if (noparen != original) {
        Ꮡt.Errorf("got %q, want %q"u8, noparen, original);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packageFooFuncFReturnˢ = """
package foo

func f() {
        return Composite{
                call(),
        }
}
"""u8;
internal static readonly @string returnCallˢ = "return call()"u8;

// Verify that we don't print a newline between "return" and its results, as
// that would incorrectly cause a naked return.
public static void TestIssue32854(ж<testing.T> Ꮡt) {
    @string src = packageFooFuncFReturnˢ;
    var fset = token.NewFileSet();
    var (@file, err) = parser.ParseFile(fset, ""u8, src, 0);
    if (err != default!) {
        throw panic(err);
    }
    // Replace the result with call(), which is on the next line.
    var fd = (~@file).Decls[0]._<ж<ast.FuncDecl>>();
    var ret = (~(~fd).Body).List[0]._<ж<ast.ReturnStmt>>();
    ret.Value.Results[0] = (~(~ret).Results[0]._<ж<ast.CompositeLit>>()).Elts[0];
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var errΔ1 = Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, ret.OrTypedNil()); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    @string want = returnCallˢ;
    {
        @string got = Ꮡbuf.String(); if (got != want) {
            Ꮡt.Fatalf("got %q, want %q"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooBarBarGoˢ = "foo\nbar/bar.go"u8;
internal static readonly @string packageBarˢ = @"package bar"u8;

public static void TestSourcePosNewline(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // We don't provide a syntax for escaping or unescaping characters in line
    // directives (see https://go.dev/issue/24183#issuecomment-372449628).
    // As a result, we cannot write a line directive with the correct path for a
    // filename containing newlines. We should return an error rather than
    // silently dropping or mangling it.
    @string fname = fooBarBarGoˢ;
    @string src = packageBarˢ;
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, fname, src, (parser.Mode)((parser.Mode)(parser.ParseComments | parser.AllErrors) | parser.SkipObjectResolution));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var cfg = Ꮡ(new Config(
        Mode: SourcePos, // emit line comments

        Tabwidth: 8
    ));
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    {
        var errΔ1 = cfg.Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), fset, f.OrTypedNil()); if (errΔ1 == default!) {
            Ꮡt.Errorf("Fprint did not error for source file path containing newline"u8);
        }
    }
    if (buf.Len() != 0) {
        Ꮡt.Errorf("unexpected Fprint output:\n%s"u8, buf.Bytes());
    }
}

// TestEmptyDecl tests that empty decls for const, var, import are printed with
// valid syntax e.g "var ()" instead of just "var", which is invalid and cannot
// be parsed.
public static void TestEmptyDecl(ж<testing.T> Ꮡt) {
    // issue 63566
    foreach (var (_, vᴛ1) in new token.Token[]{token.IMPORT, token.CONST, token.TYPE, token.VAR}.slice()) {
        ref var tok = ref heap(new token.Token(), out var Ꮡtok);
        tok = vᴛ1;

        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        Fprint(new printer_test_package.bytes_BufferжWriter(Ꮡbuf), token.NewFileSet(), Ꮡ(new ast.GenDecl(Tok: tok)));
        @string got = Ꮡbuf.String();
        @string want = tok.String() + " ()"u8;
        if (got != want) {
            Ꮡt.Errorf("got %q, want %q"u8, got, want);
        }
    }
}

} // end printer_internal_test_package
