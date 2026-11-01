// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using token = global::go.go.token_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using global::go.go;
using io = io_package;
using path;
using static global::go.go.scanner_package;

partial class scanner_internal_test_package {

internal static ж<token.FileSet> fset = token.NewFileSet();

/* class */
internal static UntypedInt special => iota;
internal static UntypedInt literal => 1;
internal static UntypedInt @operator => 2;
internal static UntypedInt keyword => 3;

internal static nint tokenclass(token.Token tok) {
    switch (ᐧ) {
    case {} when tok.IsLiteral(): {
        return literal;
    }
    case {} when tok.IsOperator(): {
        return @operator;
    }
    case {} when tok.IsKeyword(): {
        return keyword;
    }}
    return special;
}

internal partial struct elt {
    internal token.Token tok;
    internal @string lit;
    internal nint @class;
}

internal static slice<elt> tokens = slice<elt>([
    // Special tokens
    new(token.COMMENT, "/* a comment */", special),
    new(token.COMMENT, "// a comment \n", special),
    new(token.COMMENT, "/*\r*/", special),
    new(token.COMMENT, "/**\r/*/", special), // issue 11151
    new(token.COMMENT, "/**\r\r/*/", special),
    new(token.COMMENT, "//\r\n", special),

    // Identifiers and basic type literals
    new(token.IDENT, "foobar", literal),
    new(token.IDENT, "a۰۱۸", literal),
    new(token.IDENT, "foo६४", literal),
    new(token.IDENT, "bar９８７６", literal),
    new(token.IDENT, "ŝ", literal),    // was bug (issue 4000)
    new(token.IDENT, "ŝfoo", literal), // was bug (issue 4000)
    new(token.INT, "0", literal),
    new(token.INT, "1", literal),
    new(token.INT, "123456789012345678890", literal),
    new(token.INT, "01234567", literal),
    new(token.INT, "0xcafebabe", literal),
    new(token.FLOAT, "0.", literal),
    new(token.FLOAT, ".0", literal),
    new(token.FLOAT, "3.14159265", literal),
    new(token.FLOAT, "1e0", literal),
    new(token.FLOAT, "1e+100", literal),
    new(token.FLOAT, "1e-100", literal),
    new(token.FLOAT, "2.71828e-1000", literal),
    new(token.IMAG, "0i", literal),
    new(token.IMAG, "1i", literal),
    new(token.IMAG, "012345678901234567889i", literal),
    new(token.IMAG, "123456789012345678890i", literal),
    new(token.IMAG, "0.i", literal),
    new(token.IMAG, ".0i", literal),
    new(token.IMAG, "3.14159265i", literal),
    new(token.IMAG, "1e0i", literal),
    new(token.IMAG, "1e+100i", literal),
    new(token.IMAG, "1e-100i", literal),
    new(token.IMAG, "2.71828e-1000i", literal),
    new(token.CHAR, "'a'", literal),
    new(token.CHAR, "'\\000'", literal),
    new(token.CHAR, "'\\xFF'", literal),
    new(token.CHAR, "'\\uff16'", literal),
    new(token.CHAR, "'\\U0000ff16'", literal),
    new(token.STRING, "`foobar`", literal),
    new(token.STRING, "`"u8 + """
foo
	                        bar
"""u8.Raw() + "`"u8,
        literal
    ),
    new(token.STRING, "`\r`", literal),
    new(token.STRING, "`foo\r\nbar`", literal),

    // Operators and delimiters
    new(token.ADD, "+", @operator),
    new(token.SUB, "-", @operator),
    new(token.MUL, "*", @operator),
    new(token.QUO, "/", @operator),
    new(token.REM, "%", @operator),

    new(token.AND, "&", @operator),
    new(token.OR, "|", @operator),
    new(token.XOR, "^", @operator),
    new(token.SHL, "<<", @operator),
    new(token.SHR, ">>", @operator),
    new(token.AND_NOT, "&^", @operator),

    new(token.ADD_ASSIGN, "+=", @operator),
    new(token.SUB_ASSIGN, "-=", @operator),
    new(token.MUL_ASSIGN, "*=", @operator),
    new(token.QUO_ASSIGN, "/=", @operator),
    new(token.REM_ASSIGN, "%=", @operator),

    new(token.AND_ASSIGN, "&=", @operator),
    new(token.OR_ASSIGN, "|=", @operator),
    new(token.XOR_ASSIGN, "^=", @operator),
    new(token.SHL_ASSIGN, "<<=", @operator),
    new(token.SHR_ASSIGN, ">>=", @operator),
    new(token.AND_NOT_ASSIGN, "&^=", @operator),

    new(token.LAND, "&&", @operator),
    new(token.LOR, "||", @operator),
    new(token.ARROW, "<-", @operator),
    new(token.INC, "++", @operator),
    new(token.DEC, "--", @operator),

    new(token.EQL, "==", @operator),
    new(token.LSS, "<", @operator),
    new(token.GTR, ">", @operator),
    new(token.ASSIGN, "=", @operator),
    new(token.NOT, "!", @operator),

    new(token.NEQ, "!=", @operator),
    new(token.LEQ, "<=", @operator),
    new(token.GEQ, ">=", @operator),
    new(token.DEFINE, ":=", @operator),
    new(token.ELLIPSIS, "...", @operator),

    new(token.LPAREN, "(", @operator),
    new(token.LBRACK, "[", @operator),
    new(token.LBRACE, "{", @operator),
    new(token.COMMA, ",", @operator),
    new(token.PERIOD, ".", @operator),

    new(token.RPAREN, ")", @operator),
    new(token.RBRACK, "]", @operator),
    new(token.RBRACE, "}", @operator),
    new(token.SEMICOLON, ";", @operator),
    new(token.COLON, ":", @operator),
    new(token.TILDE, "~", @operator),

    // Keywords
    new(token.BREAK, "break", keyword),
    new(token.CASE, "case", keyword),
    new(token.CHAN, "chan", keyword),
    new(token.CONST, "const", keyword),
    new(token.CONTINUE, "continue", keyword),

    new(token.DEFAULT, "default", keyword),
    new(token.DEFER, "defer", keyword),
    new(token.ELSE, "else", keyword),
    new(token.FALLTHROUGH, "fallthrough", keyword),
    new(token.FOR, "for", keyword),

    new(token.FUNC, "func", keyword),
    new(token.GO, "go", keyword),
    new(token.GOTO, "goto", keyword),
    new(token.IF, "if", keyword),
    new(token.IMPORT, "import", keyword),

    new(token.INTERFACE, "interface", keyword),
    new(token.MAP, "map", keyword),
    new(token.PACKAGE, "package", keyword),
    new(token.RANGE, "range", keyword),
    new(token.RETURN, "return", keyword),

    new(token.SELECT, "select", keyword),
    new(token.STRUCT, "struct", keyword),
    new(token.SWITCH, "switch", keyword),
    new(token.TYPE, "type", keyword),
    new(token.VAR, "var", keyword)
]);

internal static ref readonly @string whitespace => ref whitespaceᵇ;
private static readonly @string whitespaceᵇ = "  \t  \n\n\n"u8; // to separate tokens

internal static slice<byte> source = func(slice<byte> () => {
    slice<byte> src = default;
    foreach (var (_, t) in tokens) {
        src = append(src, t.lit.ꓸꓸꓸ);
        src = append(src, whitespace.ꓸꓸꓸ);
    }
    return src;
})();

internal static nint newlineCount(@string s) {
    nint n = 0;
    for (nint i = 0; i < len(s); i++) {
        if (s[i] == '\n') {
            n++;
        }
    }
    return n;
}

internal static void checkPos(ж<testing.T> t, @string lit, tokenꓸPos p, tokenꓸPosition expected) {
    var pos = fset.Position(p);
    // Check cleaned filenames so that we don't have to worry about
    // different os.PathSeparator values.
    if (pos.Filename != expected.Filename && filepath.Clean(pos.Filename) != filepath.Clean(expected.Filename)) {
        t.Errorf("bad filename for %q: got %s, expected %s", lit, pos.Filename, expected.Filename);
    }
    if (pos.Offset != expected.Offset) {
        t.Errorf("bad position for %q: got %d, expected %d", lit, pos.Offset, expected.Offset);
    }
    if (pos.Line != expected.Line) {
        t.Errorf("bad line for %q: got %d, expected %d", lit, pos.Line, expected.Line);
    }
    if (pos.Column != expected.Column) {
        t.Errorf("bad column for %q: got %d, expected %d", lit, pos.Column, expected.Column);
    }
}

// Verify that calling Scan() provides the correct results.
public static void TestScan(ж<testing.T> t) {
    nint whitespace_linecount = newlineCount(whitespace);

    // error handler
    var eh = (tokenꓸPosition _, @string msg) => {
        t.Errorf("error handler called (msg = %s)", msg);
    };

    // verify scan
    global::go.go.scanner_package.Scanner s = default;
    s.Init(fset.AddFile(""u8, fset.Base(), len(source)), source, new Action<tokenꓸPosition, @string>(eh), (global::go.go.scanner_package.Mode)(ScanComments | dontInsertSemis));

    // set up expected position
    var epos = new tokenꓸPosition(
        Filename: "",
        Offset: 0,
        Line: 1,
        Column: 1
    );

    nint index = 0;
    for (;;) {
        var (pos, tok, lit) = s.Scan();

        // check position
        if (tok == token.EOF) {
            // correction for EOF
            epos.Line = newlineCount((@string)source);
            epos.Column = 2;
        }
        checkPos(t, lit, pos, epos);

        // check token
        var e = new elt(token.EOF, "", special);
        if (index < len(tokens)) {
            e = tokens[index];
            index++;
        }
        if (tok != e.tok) {
            t.Errorf("bad token for %q: got %s, expected %s", lit, tok, e.tok);
        }

        // check token class
        if (tokenclass(tok) != e.@class) {
            t.Errorf("bad class for %q: got %d, expected %d", lit, tokenclass(tok), e.@class);
        }

        // check literal
        @string elit = ""u8;
        switch (e.tok) {
        case var ᴛ when ᴛ == token.COMMENT: {
            // no CRs in comments
            elit = (@string)stripCR(slice<byte>(e.lit), e.lit[1] == '*');
            //-style comment literal doesn't contain newline
            if (elit[1] == '/') {
                elit = elit.slice(0, len(elit) - 1);
            }
            break;
        }
        case var ᴛ when ᴛ == token.IDENT: {
            elit = e.lit;
            break;
        }
        case var ᴛ when ᴛ == token.SEMICOLON: {
            elit = ";";
            break;
        }
        default: {
            if (e.tok.IsLiteral()) {
                // no CRs in raw string literals
                elit = e.lit;
                if (elit[0] == '`') {
                    elit = (@string)stripCR(slice<byte>(elit), false);
                }
            } else if (e.tok.IsKeyword()) {
                elit = e.lit;
            }
            break;
        }}
        if (lit != elit) {
            t.Errorf("bad literal for %q: got %q, expected %q", lit, lit, elit);
        }

        if (tok == token.EOF) {
            break;
        }

        // update position
        epos.Offset += len(e.lit) + len(whitespace);
        epos.Line += newlineCount(e.lit) + whitespace_linecount;
    }

    if (s.ErrorCount != 0) {
        t.Errorf("found %d errors", s.ErrorCount);
    }
}

internal partial struct TestStripCR_type /*dyn*/ {
    internal @string have, want;
}

public static void TestStripCR(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestStripCR_type>([
        new("//\n", "//\n"),
        new("//\r\n", "//\n"),
        new("//\r\r\r\n", "//\n"),
        new("//\r*\r/\r\n", "//*/\n"),
        new("/**/", "/**/"),
        new("/*\r/*/", "/*/*/"),
        new("/*\r*/", "/**/"),
        new("/**\r/*/", "/**\r/*/"),
        new("/*\r/\r*\r/*/", "/*/*\r/*/"),
        new("/*\r\r\r\r*/", "/**/")
    ])) {
        @string got = (@string)stripCR(slice<byte>(test.have), len(test.have) >= 2 && test.have[1] == '*');
        if (got != test.want) {
            t.Errorf("stripCR(%q) = %q; want %q", test.have, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string commentˢ => ref commentˢᵇ;
private static readonly @string commentˢᵇ = "COMMENT "u8;
internal static ref readonly @string commentˢ2 => ref commentˢ2ᵇ;
private static readonly @string commentˢ2ᵇ = " COMMENT"u8;
internal static ref readonly @string commentˢ3 => ref commentˢ3ᵇ;
private static readonly @string commentˢ3ᵇ = "COMMENT"u8;
internal static ref readonly @string testSemisˢ => ref testSemisˢᵇ;
private static readonly @string testSemisˢᵇ = "TestSemis"u8;

internal static void checkSemi(ж<testing.T> t, @string input, @string want, global::go.go.scanner_package.Mode mode) {
    var tokensᴷ = new stackAppend<@string>();

    if ((global::go.go.scanner_package.Mode)(mode & ScanComments) == 0) {
        want = strings.ReplaceAll(want, commentˢ, ""u8);
        want = strings.ReplaceAll(want, commentˢ2, ""u8); // if at end
        want = strings.ReplaceAll(want, commentˢ3, ""u8); // if sole token
    }

    var @file = fset.AddFile(testSemisˢ, fset.Base(), len(input));
    global::go.go.scanner_package.Scanner scan = default;
    scan.Init(@file, slice<byte>(input), default, mode);
    slice<@string> tokens = default;
    for (;;) {
        var (pos, tok, lit) = scan.Scan();
        if (tok == token.EOF) {
            break;
        }
        if (tok == token.SEMICOLON && lit != ";"u8) {
            // Artificial semicolon:
            // assert that position is EOF or that of a newline.
            nint off = @file.Offset(pos);
            if (off != len(input) && input[off] != '\n') {
                t.Errorf("scanning <<%s>>, got SEMICOLON at offset %d, want newline or EOF", input, off);
            }
        }
        lit = tok.String(); // "\n" => ";"
        tokens = append(tokensᴷ, tokens, lit);
    }
    {
        @string got = strings.Join(tokens, " "); if (got != want) {
            t.Errorf("scanning <<%s>>, got [%s], want [%s]", input, got, want);
        }
    }
}

partial struct semicolonTestsᴛ1 /*dyn*/ {
    internal @string input, want;
}
internal static array<semicolonTestsᴛ1> semicolonTests = new semicolonTestsᴛ1[]{
    new("", ""),
    new("\ufeff;", ";"), // first BOM is ignored
    new(";", ";"),
    new("foo\n", "IDENT ;"),
    new("123\n", "INT ;"),
    new("1.2\n", "FLOAT ;"),
    new("'x'\n", "CHAR ;"),
    new(@"""x"""u8 + "\n"u8, "STRING ;"),
    new("`x`\n", "STRING ;"),

    new("+\n", "+"),
    new("-\n", "-"),
    new("*\n", "*"),
    new("/\n", "/"),
    new("%\n", "%"),

    new("&\n", "&"),
    new("|\n", "|"),
    new("^\n", "^"),
    new("<<\n", "<<"),
    new(">>\n", ">>"),
    new("&^\n", "&^"),

    new("+=\n", "+="),
    new("-=\n", "-="),
    new("*=\n", "*="),
    new("/=\n", "/="),
    new("%=\n", "%="),

    new("&=\n", "&="),
    new("|=\n", "|="),
    new("^=\n", "^="),
    new("<<=\n", "<<="),
    new(">>=\n", ">>="),
    new("&^=\n", "&^="),

    new("&&\n", "&&"),
    new("||\n", "||"),
    new("<-\n", "<-"),
    new("++\n", "++ ;"),
    new("--\n", "-- ;"),

    new("==\n", "=="),
    new("<\n", "<"),
    new(">\n", ">"),
    new("=\n", "="),
    new("!\n", "!"),

    new("!=\n", "!="),
    new("<=\n", "<="),
    new(">=\n", ">="),
    new(":=\n", ":="),
    new("...\n", "..."),

    new("(\n", "("),
    new("[\n", "["),
    new("{\n", "{"),
    new(",\n", ","),
    new(".\n", "."),

    new(")\n", ") ;"),
    new("]\n", "] ;"),
    new("}\n", "} ;"),
    new(";\n", ";"),
    new(":\n", ":"),

    new("break\n", "break ;"),
    new("case\n", "case"),
    new("chan\n", "chan"),
    new("const\n", "const"),
    new("continue\n", "continue ;"),

    new("default\n", "default"),
    new("defer\n", "defer"),
    new("else\n", "else"),
    new("fallthrough\n", "fallthrough ;"),
    new("for\n", "for"),

    new("func\n", "func"),
    new("go\n", "go"),
    new("goto\n", "goto"),
    new("if\n", "if"),
    new("import\n", "import"),

    new("interface\n", "interface"),
    new("map\n", "map"),
    new("package\n", "package"),
    new("range\n", "range"),
    new("return\n", "return ;"),

    new("select\n", "select"),
    new("struct\n", "struct"),
    new("switch\n", "switch"),
    new("type\n", "type"),
    new("var\n", "var"),

    new("foo//comment\n", "IDENT COMMENT ;"),
    new("foo//comment", "IDENT COMMENT ;"),
    new("foo/*comment*/\n", "IDENT COMMENT ;"),
    new("foo/*\n*/", "IDENT COMMENT ;"),
    new("foo/*comment*/    \n", "IDENT COMMENT ;"),
    new("foo/*\n*/    ", "IDENT COMMENT ;"),

    new("foo    // comment\n", "IDENT COMMENT ;"),
    new("foo    // comment", "IDENT COMMENT ;"),
    new("foo    /*comment*/\n", "IDENT COMMENT ;"),
    new("foo    /*\n*/", "IDENT COMMENT ;"),
    new("foo    /*  */ /* \n */ bar/**/\n", "IDENT COMMENT COMMENT ; IDENT COMMENT ;"),
    new("foo    /*0*/ /*1*/ /*2*/\n", "IDENT COMMENT COMMENT COMMENT ;"),

    new("foo    /*comment*/    \n", "IDENT COMMENT ;"),
    new("foo    /*0*/ /*1*/ /*2*/    \n", "IDENT COMMENT COMMENT COMMENT ;"),
    new("foo	/**/ /*-------------*/       /*----\n*/bar       /*  \n*/baa\n", "IDENT COMMENT COMMENT COMMENT ; IDENT COMMENT ; IDENT ;"),
    new("foo    /* an EOF terminates a line */", "IDENT COMMENT ;"),
    new("foo    /* an EOF terminates a line */ /*", "IDENT COMMENT COMMENT ;"),
    new("foo    /* an EOF terminates a line */ //", "IDENT COMMENT COMMENT ;"),

    new("package main\n\nfunc main() {\n\tif {\n\t\treturn /* */ }\n}\n", "package IDENT ; func IDENT ( ) { if { return COMMENT } ; } ;"),
    new("package main", "package IDENT ;")
}.array();

public static void TestSemicolons(ж<testing.T> t) {
    foreach (var (_, test) in semicolonTests.ΔRangeSnapshot()) {
        @string input = test.input;
        @string want = test.want;
        checkSemi(t, input, want, 0);
        checkSemi(t, input, want, ScanComments);

        // if the input ended in newlines, the input must tokenize the
        // same with or without those newlines
        for (nint i = len(input) - 1; i >= 0 && input[i] == '\n'; i--) {
            checkSemi(t, input.slice(0, i), want, 0);
            checkSemi(t, input.slice(0, i), want, ScanComments);
        }
    }
}

internal partial struct segment {
    internal @string srcline;   // a line of source text
    internal @string filename;  // filename for current token; error message for invalid line directives
    internal nint line, column; // line and column for current token; error position for invalid line directives
}

internal static slice<segment> segments = slice<segment>([
    // exactly one token per line since the test consumes one token per segment
    new("  line1", "TestLineDirectives", 1, 3),
    new("\nline2", "TestLineDirectives", 2, 1),
    new("\nline3  //line File1.go:100", "TestLineDirectives", 3, 1), // bad line comment, ignored
    new("\nline4", "TestLineDirectives", 4, 1),
    new("\n//line File1.go:100\n  line100", "File1.go", 100, 0),
    new("\n//line  \t :42\n  line1", " \t ", 42, 0),
    new("\n//line File2.go:200\n  line200", "File2.go", 200, 0),
    new("\n//line foo\t:42\n  line42", "foo\t", 42, 0),
    new("\n //line foo:42\n  line43", "foo\t", 44, 0), // bad line comment, ignored (use existing, prior filename)
    new("\n//line foo 42\n  line44", "foo\t", 46, 0),  // bad line comment, ignored (use existing, prior filename)
    new("\n//line /bar:42\n  line45", "/bar", 42, 0),
    new("\n//line ./foo:42\n  line46", "foo", 42, 0),
    new("\n//line a/b/c/File1.go:100\n  line100", "a/b/c/File1.go", 100, 0),
    new("\n//line c:\\bar:42\n  line200", "c:\\bar", 42, 0),
    new("\n//line c:\\dir\\File1.go:100\n  line201", "c:\\dir\\File1.go", 100, 0),

    // tests for new line directive syntax
    new("\n//line :100\na1", "", 100, 0), // missing filename means empty filename
    new("\n//line bar:100\nb1", "bar", 100, 0),
    new("\n//line :100:10\nc1", "bar", 100, 10), // missing filename means current filename
    new("\n//line foo:100:10\nd1", "foo", 100, 10),

    new("\n/*line :100*/a2", "", 100, 0), // missing filename means empty filename
    new("\n/*line bar:100*/b2", "bar", 100, 0),
    new("\n/*line :100:10*/c2", "bar", 100, 10), // missing filename means current filename
    new("\n/*line foo:100:10*/d2", "foo", 100, 10),
    new("\n/*line foo:100:10*/    e2", "foo", 100, 14), // line-directive relative column
    new("\n/*line foo:100:10*/\n\nf2", "foo", 102, 1)   // absolute column since on new line
]);

internal static slice<segment> dirsegments = slice<segment>([
    // exactly one token per line since the test consumes one token per segment
    new("  line1", "TestLineDir/TestLineDirectives", 1, 3),
    new("\n//line File1.go:100\n  line100", "TestLineDir/File1.go", 100, 0)
]);

internal static slice<segment> dirUnixSegments = slice<segment>([
    new("\n//line /bar:42\n  line42", "/bar", 42, 0)
]);

internal static slice<segment> dirWindowsSegments = slice<segment>([
    new("\n//line c:\\bar:42\n  line42", "c:\\bar", 42, 0)
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testLineDirectivesˢ => ref testLineDirectivesˢᵇ;
private static readonly @string testLineDirectivesˢᵇ = "TestLineDirectives"u8;
internal static ref readonly @string testLineDirˢ => ref testLineDirˢᵇ;
private static readonly @string testLineDirˢᵇ = "TestLineDir/TestLineDirectives"u8;

// Verify that line directives are interpreted correctly.
public static void TestLineDirectives(ж<testing.T> t) {
    testSegments(t, segments, testLineDirectivesˢ);
    testSegments(t, dirsegments, testLineDirˢ);
    if (runtime.GOOS == "windows"u8) {
        testSegments(t, dirWindowsSegments, testLineDirˢ);
    } else {
        testSegments(t, dirUnixSegments, testLineDirˢ);
    }
}

internal static void testSegments(ж<testing.T> t, slice<segment> segments, @string filename) {
    @string src = default;
    foreach (var (_, e) in segments) {
        src += e.srcline;
    }

    // verify scan
    global::go.go.scanner_package.Scanner S = default;
    var @file = fset.AddFile(filename, fset.Base(), len(src));
    S.Init(@file, slice<byte>(src), (tokenꓸPosition pos, @string msg) => {
        t.Error(new ΔError(pos, msg));
    }, dontInsertSemis);
    foreach (var (_, s) in segments) {
        var (p, _, lit) = S.Scan();
        var pos = @file.Position(p);
        checkPos(t, lit, p, new tokenꓸPosition(
            Filename: s.filename,
            Offset: pos.Offset,
            Line: s.line,
            Column: s.column
        ));
    }

    if (S.ErrorCount != 0) {
        t.Errorf("got %d errors", S.ErrorCount);
    }
}

// The filename is used for the error message in these test cases.
// The first line directive is valid and used to control the expected error line.
internal static slice<segment> invalidSegments = slice<segment>([
    new("\n//line :1:1\n//line foo:42 extra text\ndummy", "invalid line number: 42 extra text", 1, 12),
    new("\n//line :2:1\n//line foobar:\ndummy", "invalid line number: ", 2, 15),
    new("\n//line :5:1\n//line :0\ndummy", "invalid line number: 0", 5, 9),
    new("\n//line :10:1\n//line :1:0\ndummy", "invalid column number: 0", 10, 11),
    new("\n//line :1:1\n//line :foo:0\ndummy", "invalid line number: 0", 1, 13) // foo is considered part of the filename
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string dirˢ => ref dirˢᵇ;
private static readonly @string dirˢᵇ = "dir"u8;

// Verify that invalid line directives get the correct error message.
public static void TestInvalidLineDirectives(ж<testing.T> t) {
    // make source
    @string src = default;
    foreach (var (_, e) in invalidSegments) {
        src += e.srcline;
    }

    // verify scan
    global::go.go.scanner_package.Scanner S = default;
    ref var s = ref heap(new segment(), out var Ꮡs); // current segment
    var @file = fset.AddFile(filepath.Join(dirˢ, "TestInvalidLineDirectives"), fset.Base(), len(src));
    S.Init(@file, slice<byte>(src), (tokenꓸPosition pos, @string msg) => {
        if (msg != Ꮡs.Value.filename) {
            t.Errorf("got error %q; want %q", msg, Ꮡs.Value.filename);
        }
        if (pos.Line != Ꮡs.Value.line || pos.Column != Ꮡs.Value.column) {
            t.Errorf("got position %d:%d; want %d:%d", pos.Line, pos.Column, Ꮡs.Value.line, Ꮡs.Value.column);
        }
    }, dontInsertSemis);
    foreach (var (_, vᴛ1) in invalidSegments) {
        s = vᴛ1;

        S.Scan();
    }

    if (S.ErrorCount != len(invalidSegments)) {
        t.Errorf("got %d errors; want %d", S.ErrorCount, len(invalidSegments));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ifTrueˢ => ref ifTrueˢᵇ;
private static readonly @string ifTrueˢᵇ = "if true { }"u8;
internal static ref readonly @string src1ˢ => ref src1ˢᵇ;
private static readonly @string src1ˢᵇ = "src1"u8;
internal static ref readonly @string goTrueˢ => ref goTrueˢᵇ;
private static readonly @string goTrueˢᵇ = "go true { ]"u8;
internal static ref readonly @string src2ˢ => ref src2ˢᵇ;
private static readonly @string src2ˢᵇ = "src2"u8;

// Verify that initializing the same scanner more than once works correctly.
public static void TestInit(ж<testing.T> t) {
    global::go.go.scanner_package.Scanner s = default;

    // 1st init
    @string src1 = ifTrueˢ;
    var f1 = fset.AddFile(src1ˢ, fset.Base(), len(src1));
    s.Init(f1, slice<byte>(src1), default, dontInsertSemis);
    if (f1.Size() != len(src1)) {
        t.Errorf("bad file size: got %d, expected %d", f1.Size(), len(src1));
    }
    s.Scan();                   // if
    s.Scan();                   // true
    var (_, tok, _) = s.Scan(); // {
    if (tok != token.LBRACE) {
        t.Errorf("bad token: got %s, expected %s", tok, token.LBRACE);
    }

    // 2nd init
    @string src2 = goTrueˢ;
    var f2 = fset.AddFile(src2ˢ, fset.Base(), len(src2));
    s.Init(f2, slice<byte>(src2), default, dontInsertSemis);
    if (f2.Size() != len(src2)) {
        t.Errorf("bad file size: got %d, expected %d", f2.Size(), len(src2));
    }
    (_, tok, _) = s.Scan(); // go
    if (tok != token.GO) {
        t.Errorf("bad token: got %s, expected %s", tok, token.GO);
    }

    if (s.ErrorCount != 0) {
        t.Errorf("found %d errors", s.ErrorCount);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string file1ˢ => ref file1ˢᵇ;
private static readonly @string file1ˢᵇ = "File1"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ => ref srcᶜᵇ;
private static readonly @string srcᶜᵇ = "@\n@ @\n//line File2:20\n@\n//line File2:1\n@ @\n//line File1:1\n@ @ @";

public static void TestStdErrorHandler(ж<testing.T> t) {
// illegal character, cause an error
// two errors on the same line
// different file, but same line
// same file, decreasing line number
    @string src = srcᶜ; // original file, line 1 again

    ref var list = ref heap<global::go.go.scanner_package.ErrorList>(out var Ꮡlist);
    var eh = (tokenꓸPosition pos, @string msg) => {
        Ꮡlist.ValueSlot.Add(pos, msg);
    };

    global::go.go.scanner_package.Scanner s = default;
    s.Init(fset.AddFile(file1ˢ, fset.Base(), len(src)), slice<byte>(src), new Action<tokenꓸPosition, @string>(eh), dontInsertSemis);
    for (;;) {
        {
            var (_, tok, _) = s.Scan(); if (tok == token.EOF) {
                break;
            }
        }
    }

    if (len(list) != s.ErrorCount) {
        t.Errorf("found %d errors, expected %d", len(list), s.ErrorCount);
    }

    if (len(list) != 9) {
        t.Errorf("found %d raw errors, expected 9", len(list));
        PrintError(new os.FileжWriter(os.Stderr), list);
    }

    list.Sort();
    if (len(list) != 9) {
        t.Errorf("found %d sorted errors, expected 9", len(list));
        PrintError(new os.FileжWriter(os.Stderr), list);
    }

    Ꮡlist.RemoveMultiples();
    if (len(list) != 4) {
        t.Errorf("found %d one-per-line errors, expected 4", len(list));
        PrintError(new os.FileжWriter(os.Stderr), list);
    }
}

internal partial struct errorCollector {
    internal nint cnt;           // number of errors encountered
    internal @string msg;        // last error message encountered
    internal tokenꓸPosition pos; // last error position encountered
}

internal static void checkError(ж<testing.T> t, @string src, token.Token tok, nint pos, @string lit, @string err) {
    global::go.go.scanner_package.Scanner s = default;
    ref var h = ref heap(new errorCollector(), out var Ꮡh);
    var eh = (tokenꓸPosition posΔ1, @string msg) => {
        Ꮡh.Value.cnt++;
        Ꮡh.Value.msg = msg;
        Ꮡh.Value.pos = posΔ1;
    };
    s.Init(fset.AddFile(""u8, fset.Base(), len(src)), slice<byte>(src), new Action<tokenꓸPosition, @string>(eh), (global::go.go.scanner_package.Mode)(ScanComments | dontInsertSemis));
    var (_, tok0, lit0) = s.Scan();
    if (tok0 != tok) {
        t.Errorf("%q: got %s, expected %s", src, tok0, tok);
    }
    if (tok0 != token.ILLEGAL && lit0 != lit) {
        t.Errorf("%q: got literal %q, expected %q", src, lit0, lit);
    }
    nint cnt = 0;
    if (err != ""u8) {
        cnt = 1;
    }
    if (h.cnt != cnt) {
        t.Errorf("%q: got cnt %d, expected %d", src, h.cnt, cnt);
    }
    if (h.msg != err) {
        t.Errorf("%q: got msg %q, expected %q", src, h.msg, err);
    }
    if (h.pos.Offset != pos) {
        t.Errorf("%q: got offset %d, expected %d", src, h.pos.Offset, pos);
    }
}

partial struct errorsᴛ1 /*dyn*/ {
    internal @string src;
    internal token.Token tok;
    internal nint pos;
    internal @string lit;
    internal @string err;
}
internal static slice<errorsᴛ1> errors = slice<errorsᴛ1>([
    new("\a", token.ILLEGAL, 0, "", "illegal character U+0007"),
    new(@"#", token.ILLEGAL, 0, "", "illegal character U+0023 '#'"),
    new(@"…", token.ILLEGAL, 0, "", "illegal character U+2026 '…'"),
    new("..", token.PERIOD, 0, "", ""), // two periods, not invalid token (issue #28112)
    new(@"' '", token.CHAR, 0, @"' '", ""),
    new(@"''", token.CHAR, 0, @"''", "illegal rune literal"),
    new(@"'12'", token.CHAR, 0, @"'12'", "illegal rune literal"),
    new(@"'123'", token.CHAR, 0, @"'123'", "illegal rune literal"),
    new(@"'\0'", token.CHAR, 3, @"'\0'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\07'", token.CHAR, 4, @"'\07'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\8'", token.CHAR, 2, @"'\8'", "unknown escape sequence"),
    new(@"'\08'", token.CHAR, 3, @"'\08'", "illegal character U+0038 '8' in escape sequence"),
    new(@"'\x'", token.CHAR, 3, @"'\x'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\x0'", token.CHAR, 4, @"'\x0'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\x0g'", token.CHAR, 4, @"'\x0g'", "illegal character U+0067 'g' in escape sequence"),
    new(@"'\u'", token.CHAR, 3, @"'\u'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\u0'", token.CHAR, 4, @"'\u0'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\u00'", token.CHAR, 5, @"'\u00'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\u000'", token.CHAR, 6, @"'\u000'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\u000", token.CHAR, 6, @"'\u000", "escape sequence not terminated"),
    new(@"'\u0000'", token.CHAR, 0, @"'\u0000'", ""),
    new(@"'\U'", token.CHAR, 3, @"'\U'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\U0'", token.CHAR, 4, @"'\U0'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\U00'", token.CHAR, 5, @"'\U00'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\U000'", token.CHAR, 6, @"'\U000'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\U0000'", token.CHAR, 7, @"'\U0000'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\U00000'", token.CHAR, 8, @"'\U00000'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\U000000'", token.CHAR, 9, @"'\U000000'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\U0000000'", token.CHAR, 10, @"'\U0000000'", "illegal character U+0027 ''' in escape sequence"),
    new(@"'\U0000000", token.CHAR, 10, @"'\U0000000", "escape sequence not terminated"),
    new(@"'\U00000000'", token.CHAR, 0, @"'\U00000000'", ""),
    new(@"'\Uffffffff'", token.CHAR, 2, @"'\Uffffffff'", "escape sequence is invalid Unicode code point"),
    new(@"'", token.CHAR, 0, @"'", "rune literal not terminated"),
    new(@"'\", token.CHAR, 2, @"'\", "escape sequence not terminated"),
    new("'\n", token.CHAR, 0, "'", "rune literal not terminated"),
    new("'\n   ", token.CHAR, 0, "'", "rune literal not terminated"),
    new(@"""""", token.STRING, 0, @"""""", ""),
    new(@"""abc", token.STRING, 0, @"""abc", "string literal not terminated"),
    new("\"abc\n", token.STRING, 0, @"""abc", "string literal not terminated"),
    new("\"abc\n   ", token.STRING, 0, @"""abc", "string literal not terminated"),
    new("``", token.STRING, 0, "``", ""),
    new("`", token.STRING, 0, "`", "raw string literal not terminated"),
    new("/**/", token.COMMENT, 0, "/**/", ""),
    new("/*", token.COMMENT, 0, "/*", "comment not terminated"),
    new("077", token.INT, 0, "077", ""),
    new("078.", token.FLOAT, 0, "078.", ""),
    new("07801234567.", token.FLOAT, 0, "07801234567.", ""),
    new("078e0", token.FLOAT, 0, "078e0", ""),
    new("0E", token.FLOAT, 2, "0E", "exponent has no digits"), // issue 17621
    new("078", token.INT, 2, "078", "invalid digit '8' in octal literal"),
    new("07090000008", token.INT, 3, "07090000008", "invalid digit '9' in octal literal"),
    new("0x", token.INT, 2, "0x", "hexadecimal literal has no digits"),
    new(((@string)(new byte[]{0x22, 0x61, 0x62, 0x63, 0x00, 0x64, 0x65, 0x66, 0x22})), token.STRING, 4, ((@string)(new byte[]{0x22, 0x61, 0x62, 0x63, 0x00, 0x64, 0x65, 0x66, 0x22})), "illegal character NUL"),
    new(((@string)(new byte[]{0x22, 0x61, 0x62, 0x63, 0x80, 0x64, 0x65, 0x66, 0x22})), token.STRING, 4, ((@string)(new byte[]{0x22, 0x61, 0x62, 0x63, 0x80, 0x64, 0x65, 0x66, 0x22})), "illegal UTF-8 encoding"),
    new("\ufeff\ufeff", token.ILLEGAL, 3, "\ufeff\ufeff", "illegal byte order mark"),                                            // only first BOM is ignored
    new("//\ufeff", token.COMMENT, 2, "//\ufeff", "illegal byte order mark"),                                                    // only first BOM is ignored
    new("'\ufeff"u8 + @"'"u8, token.CHAR, 1, "'\ufeff"u8 + @"'"u8, "illegal byte order mark"),                                   // only first BOM is ignored
    new(@""""u8 + "abc\ufeffdef"u8 + @""""u8, token.STRING, 4, @""""u8 + "abc\ufeffdef"u8 + @""""u8, "illegal byte order mark"), // only first BOM is ignored
    new(((@string)(new byte[]{0x61, 0x62, 0x63, 0x00, 0x64, 0x65, 0x66})), token.IDENT, 3, "abc", "illegal character NUL"),
    new("abc\x00", token.IDENT, 3, "abc", "illegal character NUL"),
    new("“abc”", token.ILLEGAL, 0, "abc", @"curly quotation mark '“' (use neutral '""')")
]);

public static void TestScanErrors(ж<testing.T> t) {
    foreach (var (_, e) in errors) {
        checkError(t, e.src, e.tok, e.pos, e.lit, e.err);
    }
}

public static void TestUTF16(ж<testing.T> t) {
    // This test doesn't fit within TestScanErrors because
    // the latter assumes that there was only one error.
    foreach (var (_, src) in slice<@string>([
        ((@string)(new byte[]{0xfe, 0xff, 0x00, 0x70, 0x00, 0x61, 0x00, 0x63, 0x00, 0x6b, 0x00, 0x61, 0x00, 0x67, 0x00, 0x65, 0x00, 0x20, 0x00, 0x70})), // BOM + "package p" encoded as UTF-16 BE
        ((@string)(new byte[]{0xff, 0xfe, 0x70, 0x00, 0x61, 0x00, 0x63, 0x00, 0x6b, 0x00, 0x61, 0x00, 0x67, 0x00, 0x65, 0x00, 0x20, 0x00, 0x70, 0x00}))  // BOM + "package p" encoded as UTF-16 LE
    ])) {
        ref var got = ref heap<slice<@string>>(out var Ꮡgot);
        var eh = (tokenꓸPosition posn, @string msg) => {
            Ꮡgot.ValueSlot = append(Ꮡgot.ValueSlot, fmt.Sprintf("#%d: %s"u8, posn.Offset, msg));
        };
        global::go.go.scanner_package.Scanner sc = default;
        sc.Init(fset.AddFile(""u8, fset.Base(), len(src)), slice<byte>(src), new Action<tokenꓸPosition, @string>(eh), 0);
        sc.Scan();

        // We expect two errors:
        // one from the decoder, one from the scanner.
        var want = slice<@string>([
            "#0: illegal UTF-8 encoding (got UTF-16)",
            "#0: illegal character U+FFFD '�'"
        ]);
        if (!slices.Equal<slice<@string>, @string>(got, want)) {
            t.Errorf("Scan(%q) returned errors %q, want %q", src, got, want);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ1 => ref srcᶜ1ᵇ;
private static readonly @string srcᶜ1ᵇ = """

		var (
			A = 1 // foo
		)

		var (
			B = 2
			// foo
		)

		var C = 3 // foo

		var D = 4
		// foo

		func anycode() {
		// foo
		}
	
"""u8.Raw();

// Verify that no comments show up as literal values when skipping comments.
public static void TestIssue10213(ж<testing.T> t) {
    @string src = srcᶜ1;
    global::go.go.scanner_package.Scanner s = default;
    s.Init(fset.AddFile(""u8, fset.Base(), len(src)), slice<byte>(src), default, 0);
    for (;;) {
        var (pos, tok, lit) = s.Scan();
        nint @class = tokenclass(tok);
        if (lit != ""u8 && @class != keyword && @class != literal && tok != token.SEMICOLON) {
            t.Errorf("%s: tok = %s, lit = %q", fset.Position(pos), tok, lit);
        }
        if (tok <= token.EOF) {
            break;
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ2 => ref srcᶜ2ᵇ;
private static readonly @string srcᶜ2ᵇ = "... .. 0.. .."u8;

public static void TestIssue28112(ж<testing.T> t) {
    @string src = srcᶜ2; // make sure to have stand-alone ".." immediately before EOF to test EOF behavior
    var tokens = slice<token.Token>([token.ELLIPSIS, token.PERIOD, token.PERIOD, token.FLOAT, token.PERIOD, token.PERIOD, token.PERIOD, token.EOF]);
    global::go.go.scanner_package.Scanner s = default;
    s.Init(fset.AddFile(""u8, fset.Base(), len(src)), slice<byte>(src), default, 0);
    foreach (var (_, want) in tokens) {
        var (pos, got, lit) = s.Scan();
        if (got != want) {
            t.Errorf("%s: got %s, want %s", fset.Position(pos), got, want);
        }
        // literals expect to have a (non-empty) literal string and we don't care about other tokens for this test
        if (tokenclass(got) == literal && lit == ""u8) {
            t.Errorf("%s: for %s got empty literal string", fset.Position(pos), got);
        }
    }
}

public static void BenchmarkScan(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var fset = token.NewFileSet();
    var @file = fset.AddFile(""u8, fset.Base(), len(source));
    global::go.go.scanner_package.Scanner s = default;
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        s.Init(@file, source, default, ScanComments);
        for (;;) {
            var (_, tok, _) = s.Scan();
            if (tok == token.EOF) {
                break;
            }
        }
    }
}

public static void BenchmarkScanFiles(ж<testing.B> b) {
    // Scan a few arbitrary large files, and one small one, to provide some
    // variety in benchmarks.
    foreach (var (_, p) in slice<@string>([
        "go/types/expr.go",
        "go/parser/parser.go",
        "net/http/server.go",
        "go/scanner/errors.go"
    ])) {
        b.Run(p, (ж<testing.B> bΔ1) => {
            bΔ1.StopTimer();
            @string filename = filepath.Join("..", "..", filepath.FromSlash(p));
            var (src, err) = os.ReadFile(filename);
            if (err != nil) {
                bΔ1.Fatal(err);
            }
            var fset = token.NewFileSet();
            var @file = fset.AddFile(filename, fset.Base(), len(src));
            bΔ1.SetBytes((int64)len(src));
            global::go.go.scanner_package.Scanner s = default;
            bΔ1.StartTimer();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                s.Init(@file, src, default, ScanComments);
                for (;;) {
                    var (_, tok, _) = s.Scan();
                    if (tok == token.EOF) {
                        break;
                    }
                }
            }
        });
    }
}

internal partial struct TestNumbers_type /*dyn*/ {
    internal token.Token tok;
    internal @string src, tokens, err;
}

public static void TestNumbers(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestNumbers_type>([
        // binaries
        new(token.INT, "0b0", "0b0", ""),
        new(token.INT, "0b1010", "0b1010", ""),
        new(token.INT, "0B1110", "0B1110", ""),

        new(token.INT, "0b", "0b", "binary literal has no digits"),
        new(token.INT, "0b0190", "0b0190", "invalid digit '9' in binary literal"),
        new(token.INT, "0b01a0", "0b01 a0", ""), // only accept 0-9

        new(token.FLOAT, "0b.", "0b.", "invalid radix point in binary literal"),
        new(token.FLOAT, "0b.1", "0b.1", "invalid radix point in binary literal"),
        new(token.FLOAT, "0b1.0", "0b1.0", "invalid radix point in binary literal"),
        new(token.FLOAT, "0b1e10", "0b1e10", "'e' exponent requires decimal mantissa"),
        new(token.FLOAT, "0b1P-1", "0b1P-1", "'P' exponent requires hexadecimal mantissa"),

        new(token.IMAG, "0b10i", "0b10i", ""),
        new(token.IMAG, "0b10.0i", "0b10.0i", "invalid radix point in binary literal"),

        // octals
        new(token.INT, "0o0", "0o0", ""),
        new(token.INT, "0o1234", "0o1234", ""),
        new(token.INT, "0O1234", "0O1234", ""),

        new(token.INT, "0o", "0o", "octal literal has no digits"),
        new(token.INT, "0o8123", "0o8123", "invalid digit '8' in octal literal"),
        new(token.INT, "0o1293", "0o1293", "invalid digit '9' in octal literal"),
        new(token.INT, "0o12a3", "0o12 a3", ""), // only accept 0-9

        new(token.FLOAT, "0o.", "0o.", "invalid radix point in octal literal"),
        new(token.FLOAT, "0o.2", "0o.2", "invalid radix point in octal literal"),
        new(token.FLOAT, "0o1.2", "0o1.2", "invalid radix point in octal literal"),
        new(token.FLOAT, "0o1E+2", "0o1E+2", "'E' exponent requires decimal mantissa"),
        new(token.FLOAT, "0o1p10", "0o1p10", "'p' exponent requires hexadecimal mantissa"),

        new(token.IMAG, "0o10i", "0o10i", ""),
        new(token.IMAG, "0o10e0i", "0o10e0i", "'e' exponent requires decimal mantissa"),

        // 0-octals
        new(token.INT, "0", "0", ""),
        new(token.INT, "0123", "0123", ""),

        new(token.INT, "08123", "08123", "invalid digit '8' in octal literal"),
        new(token.INT, "01293", "01293", "invalid digit '9' in octal literal"),
        new(token.INT, "0F.", "0 F .", ""), // only accept 0-9
        new(token.INT, "0123F.", "0123 F .", ""),
        new(token.INT, "0123456x", "0123456 x", ""),

        // decimals
        new(token.INT, "1", "1", ""),
        new(token.INT, "1234", "1234", ""),

        new(token.INT, "1f", "1 f", ""), // only accept 0-9

        new(token.IMAG, "0i", "0i", ""),
        new(token.IMAG, "0678i", "0678i", ""),

        // decimal floats
        new(token.FLOAT, "0.", "0.", ""),
        new(token.FLOAT, "123.", "123.", ""),
        new(token.FLOAT, "0123.", "0123.", ""),

        new(token.FLOAT, ".0", ".0", ""),
        new(token.FLOAT, ".123", ".123", ""),
        new(token.FLOAT, ".0123", ".0123", ""),

        new(token.FLOAT, "0.0", "0.0", ""),
        new(token.FLOAT, "123.123", "123.123", ""),
        new(token.FLOAT, "0123.0123", "0123.0123", ""),

        new(token.FLOAT, "0e0", "0e0", ""),
        new(token.FLOAT, "123e+0", "123e+0", ""),
        new(token.FLOAT, "0123E-1", "0123E-1", ""),

        new(token.FLOAT, "0.e+1", "0.e+1", ""),
        new(token.FLOAT, "123.E-10", "123.E-10", ""),
        new(token.FLOAT, "0123.e123", "0123.e123", ""),

        new(token.FLOAT, ".0e-1", ".0e-1", ""),
        new(token.FLOAT, ".123E+10", ".123E+10", ""),
        new(token.FLOAT, ".0123E123", ".0123E123", ""),

        new(token.FLOAT, "0.0e1", "0.0e1", ""),
        new(token.FLOAT, "123.123E-10", "123.123E-10", ""),
        new(token.FLOAT, "0123.0123e+456", "0123.0123e+456", ""),

        new(token.FLOAT, "0e", "0e", "exponent has no digits"),
        new(token.FLOAT, "0E+", "0E+", "exponent has no digits"),
        new(token.FLOAT, "1e+f", "1e+ f", "exponent has no digits"),
        new(token.FLOAT, "0p0", "0p0", "'p' exponent requires hexadecimal mantissa"),
        new(token.FLOAT, "1.0P-1", "1.0P-1", "'P' exponent requires hexadecimal mantissa"),

        new(token.IMAG, "0.i", "0.i", ""),
        new(token.IMAG, ".123i", ".123i", ""),
        new(token.IMAG, "123.123i", "123.123i", ""),
        new(token.IMAG, "123e+0i", "123e+0i", ""),
        new(token.IMAG, "123.E-10i", "123.E-10i", ""),
        new(token.IMAG, ".123E+10i", ".123E+10i", ""),

        // hexadecimals
        new(token.INT, "0x0", "0x0", ""),
        new(token.INT, "0x1234", "0x1234", ""),
        new(token.INT, "0xcafef00d", "0xcafef00d", ""),
        new(token.INT, "0XCAFEF00D", "0XCAFEF00D", ""),

        new(token.INT, "0x", "0x", "hexadecimal literal has no digits"),
        new(token.INT, "0x1g", "0x1 g", ""),

        new(token.IMAG, "0xf00i", "0xf00i", ""),

        // hexadecimal floats
        new(token.FLOAT, "0x0p0", "0x0p0", ""),
        new(token.FLOAT, "0x12efp-123", "0x12efp-123", ""),
        new(token.FLOAT, "0xABCD.p+0", "0xABCD.p+0", ""),
        new(token.FLOAT, "0x.0189P-0", "0x.0189P-0", ""),
        new(token.FLOAT, "0x1.ffffp+1023", "0x1.ffffp+1023", ""),

        new(token.FLOAT, "0x.", "0x.", "hexadecimal literal has no digits"),
        new(token.FLOAT, "0x0.", "0x0.", "hexadecimal mantissa requires a 'p' exponent"),
        new(token.FLOAT, "0x.0", "0x.0", "hexadecimal mantissa requires a 'p' exponent"),
        new(token.FLOAT, "0x1.1", "0x1.1", "hexadecimal mantissa requires a 'p' exponent"),
        new(token.FLOAT, "0x1.1e0", "0x1.1e0", "hexadecimal mantissa requires a 'p' exponent"),
        new(token.FLOAT, "0x1.2gp1a", "0x1.2 gp1a", "hexadecimal mantissa requires a 'p' exponent"),
        new(token.FLOAT, "0x0p", "0x0p", "exponent has no digits"),
        new(token.FLOAT, "0xeP-", "0xeP-", "exponent has no digits"),
        new(token.FLOAT, "0x1234PAB", "0x1234P AB", "exponent has no digits"),
        new(token.FLOAT, "0x1.2p1a", "0x1.2p1 a", ""),

        new(token.IMAG, "0xf00.bap+12i", "0xf00.bap+12i", ""),

        // separators
        new(token.INT, "0b_1000_0001", "0b_1000_0001", ""),
        new(token.INT, "0o_600", "0o_600", ""),
        new(token.INT, "0_466", "0_466", ""),
        new(token.INT, "1_000", "1_000", ""),
        new(token.FLOAT, "1_000.000_1", "1_000.000_1", ""),
        new(token.IMAG, "10e+1_2_3i", "10e+1_2_3i", ""),
        new(token.INT, "0x_f00d", "0x_f00d", ""),
        new(token.FLOAT, "0x_f00d.0p1_2", "0x_f00d.0p1_2", ""),

        new(token.INT, "0b__1000", "0b__1000", "'_' must separate successive digits"),
        new(token.INT, "0o60___0", "0o60___0", "'_' must separate successive digits"),
        new(token.INT, "0466_", "0466_", "'_' must separate successive digits"),
        new(token.FLOAT, "1_.", "1_.", "'_' must separate successive digits"),
        new(token.FLOAT, "0._1", "0._1", "'_' must separate successive digits"),
        new(token.FLOAT, "2.7_e0", "2.7_e0", "'_' must separate successive digits"),
        new(token.IMAG, "10e+12_i", "10e+12_i", "'_' must separate successive digits"),
        new(token.INT, "0x___0", "0x___0", "'_' must separate successive digits"),
        new(token.FLOAT, "0x1.0_p0", "0x1.0_p0", "'_' must separate successive digits")
    ])) {
        global::go.go.scanner_package.Scanner s = default;
        @string err = default;
        s.Init(fset.AddFile(""u8, fset.Base(), len(test.src)), slice<byte>(test.src), (tokenꓸPosition _Δp0, @string msg) => {
            if (err == ""u8) {
                err = msg;
            }
        }, 0);
        foreach (var (i, want) in strings.Split(test.tokens, " ")) {
            err = ""u8;
            var (_, tokΔ1, lit) = s.Scan();

            // compute lit where for tokens where lit is not defined
            switch (tokΔ1) {
            case var ᴛ when ᴛ == token.PERIOD: {
                lit = ".";
                break;
            }
            case var ᴛ when ᴛ == token.ADD: {
                lit = "+";
                break;
            }
            case var ᴛ when ᴛ == token.SUB: {
                lit = "-";
                break;
            }}

            if (i == 0) {
                if (tokΔ1 != test.tok) {
                    t.Errorf("%q: got token %s; want %s", test.src, tokΔ1, test.tok);
                }
                if (err != test.err) {
                    t.Errorf("%q: got error %q; want %q", test.src, err, test.err);
                }
            }

            if (lit != want) {
                t.Errorf("%q: got literal %q (%s); want %s", test.src, lit, tokΔ1, want);
            }
        }

        // make sure we read all
        var (_, tok, _) = s.Scan();
        if (tok == token.SEMICOLON) {
            (_, tok, _) = s.Scan();
        }
        if (tok != token.EOF) {
            t.Errorf("%q: got %s; want EOF", test.src, tok);
        }
    }
}

} // end scanner_internal_test_package
