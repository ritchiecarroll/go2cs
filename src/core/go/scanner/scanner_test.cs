// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using token = global::go.go.token_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
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

// Special tokens
// issue 11151
// Identifiers and basic type literals
// was bug (issue 4000)
// was bug (issue 4000)
// Operators and delimiters
// Keywords
internal static slice<elt> tokens = new elt[]{
    new(token.COMMENT, "/* a comment */"u8, special),
    new(token.COMMENT, "// a comment \n"u8, special),
    new(token.COMMENT, "/*\r*/"u8, special),
    new(token.COMMENT, "/**\r/*/"u8, special),
    new(token.COMMENT, "/**\r\r/*/"u8, special),
    new(token.COMMENT, "//\r\n"u8, special),
    new(token.IDENT, "foobar"u8, literal),
    new(token.IDENT, "a۰۱۸"u8, literal),
    new(token.IDENT, "foo६४"u8, literal),
    new(token.IDENT, "bar９８７６"u8, literal),
    new(token.IDENT, "ŝ"u8, literal),
    new(token.IDENT, "ŝfoo"u8, literal),
    new(token.INT, "0"u8, literal),
    new(token.INT, "1"u8, literal),
    new(token.INT, "123456789012345678890"u8, literal),
    new(token.INT, "01234567"u8, literal),
    new(token.INT, "0xcafebabe"u8, literal),
    new(token.FLOAT, "0."u8, literal),
    new(token.FLOAT, ".0"u8, literal),
    new(token.FLOAT, "3.14159265"u8, literal),
    new(token.FLOAT, "1e0"u8, literal),
    new(token.FLOAT, "1e+100"u8, literal),
    new(token.FLOAT, "1e-100"u8, literal),
    new(token.FLOAT, "2.71828e-1000"u8, literal),
    new(token.IMAG, "0i"u8, literal),
    new(token.IMAG, "1i"u8, literal),
    new(token.IMAG, "012345678901234567889i"u8, literal),
    new(token.IMAG, "123456789012345678890i"u8, literal),
    new(token.IMAG, "0.i"u8, literal),
    new(token.IMAG, ".0i"u8, literal),
    new(token.IMAG, "3.14159265i"u8, literal),
    new(token.IMAG, "1e0i"u8, literal),
    new(token.IMAG, "1e+100i"u8, literal),
    new(token.IMAG, "1e-100i"u8, literal),
    new(token.IMAG, "2.71828e-1000i"u8, literal),
    new(token.CHAR, "'a'"u8, literal),
    new(token.CHAR, "'\\000'"u8, literal),
    new(token.CHAR, "'\\xFF'"u8, literal),
    new(token.CHAR, "'\\uff16'"u8, literal),
    new(token.CHAR, "'\\U0000ff16'"u8, literal),
    new(token.STRING, "`foobar`"u8, literal),
    new(token.STRING, "`"u8 + """
foo
	                        bar
"""u8 + "`"u8,
        literal
    ),
    new(token.STRING, "`\r`"u8, literal),
    new(token.STRING, "`foo\r\nbar`"u8, literal),
    new(token.ADD, "+"u8, @operator),
    new(token.SUB, "-"u8, @operator),
    new(token.MUL, "*"u8, @operator),
    new(token.QUO, "/"u8, @operator),
    new(token.REM, "%"u8, @operator),
    new(token.AND, "&"u8, @operator),
    new(token.OR, "|"u8, @operator),
    new(token.XOR, "^"u8, @operator),
    new(token.SHL, "<<"u8, @operator),
    new(token.SHR, ">>"u8, @operator),
    new(token.AND_NOT, "&^"u8, @operator),
    new(token.ADD_ASSIGN, "+="u8, @operator),
    new(token.SUB_ASSIGN, "-="u8, @operator),
    new(token.MUL_ASSIGN, "*="u8, @operator),
    new(token.QUO_ASSIGN, "/="u8, @operator),
    new(token.REM_ASSIGN, "%="u8, @operator),
    new(token.AND_ASSIGN, "&="u8, @operator),
    new(token.OR_ASSIGN, "|="u8, @operator),
    new(token.XOR_ASSIGN, "^="u8, @operator),
    new(token.SHL_ASSIGN, "<<="u8, @operator),
    new(token.SHR_ASSIGN, ">>="u8, @operator),
    new(token.AND_NOT_ASSIGN, "&^="u8, @operator),
    new(token.LAND, "&&"u8, @operator),
    new(token.LOR, "||"u8, @operator),
    new(token.ARROW, "<-"u8, @operator),
    new(token.INC, "++"u8, @operator),
    new(token.DEC, "--"u8, @operator),
    new(token.EQL, "=="u8, @operator),
    new(token.LSS, "<"u8, @operator),
    new(token.GTR, ">"u8, @operator),
    new(token.ASSIGN, "="u8, @operator),
    new(token.NOT, "!"u8, @operator),
    new(token.NEQ, "!="u8, @operator),
    new(token.LEQ, "<="u8, @operator),
    new(token.GEQ, ">="u8, @operator),
    new(token.DEFINE, ":="u8, @operator),
    new(token.ELLIPSIS, "..."u8, @operator),
    new(token.LPAREN, "("u8, @operator),
    new(token.LBRACK, "["u8, @operator),
    new(token.LBRACE, "{"u8, @operator),
    new(token.COMMA, ","u8, @operator),
    new(token.PERIOD, "."u8, @operator),
    new(token.RPAREN, ")"u8, @operator),
    new(token.RBRACK, "]"u8, @operator),
    new(token.RBRACE, "}"u8, @operator),
    new(token.SEMICOLON, ";"u8, @operator),
    new(token.COLON, ":"u8, @operator),
    new(token.TILDE, "~"u8, @operator),
    new(token.BREAK, "break"u8, keyword),
    new(token.CASE, "case"u8, keyword),
    new(token.CHAN, "chan"u8, keyword),
    new(token.CONST, "const"u8, keyword),
    new(token.CONTINUE, "continue"u8, keyword),
    new(token.DEFAULT, "default"u8, keyword),
    new(token.DEFER, "defer"u8, keyword),
    new(token.ELSE, "else"u8, keyword),
    new(token.FALLTHROUGH, "fallthrough"u8, keyword),
    new(token.FOR, "for"u8, keyword),
    new(token.FUNC, "func"u8, keyword),
    new(token.GO, "go"u8, keyword),
    new(token.GOTO, "goto"u8, keyword),
    new(token.IF, "if"u8, keyword),
    new(token.IMPORT, "import"u8, keyword),
    new(token.INTERFACE, "interface"u8, keyword),
    new(token.MAP, "map"u8, keyword),
    new(token.PACKAGE, "package"u8, keyword),
    new(token.RANGE, "range"u8, keyword),
    new(token.RETURN, "return"u8, keyword),
    new(token.SELECT, "select"u8, keyword),
    new(token.STRUCT, "struct"u8, keyword),
    new(token.SWITCH, "switch"u8, keyword),
    new(token.TYPE, "type"u8, keyword),
    new(token.VAR, "var"u8, keyword)
}.slice();

internal static readonly @string whitespace = "  \t  \n\n\n"u8; // to separate tokens

internal static slice<byte> source = ((Func<slice<byte>>)(() => {
    slice<byte> src = default!;
    foreach (var (_, t) in tokens) {
        src = append(src, t.lit.ꓸꓸꓸ);
        src = append(src, whitespace.ꓸꓸꓸ);
    }
    return src;
}))();

internal static nint newlineCount(@string s) {
    nint n = 0;
    for (nint i = 0; i < len(s); i++) {
        if (s[i] == (rune)'\n') {
            n++;
        }
    }
    return n;
}

internal static void checkPos(ж<testing.T> Ꮡt, @string lit, tokenꓸPos p, tokenꓸPosition expected) {
    ref var t = ref Ꮡt.DerefOrNull();

    var pos = fset.Position(p);
    // Check cleaned filenames so that we don't have to worry about
    // different os.PathSeparator values.
    if (pos.Filename != expected.Filename && filepath.Clean(pos.Filename) != filepath.Clean(expected.Filename)) {
        Ꮡt.Errorf("bad filename for %q: got %s, expected %s"u8, lit, pos.Filename, expected.Filename);
    }
    if (pos.Offset != expected.Offset) {
        Ꮡt.Errorf("bad position for %q: got %d, expected %d"u8, lit, pos.Offset, expected.Offset);
    }
    if (pos.Line != expected.Line) {
        Ꮡt.Errorf("bad line for %q: got %d, expected %d"u8, lit, pos.Line, expected.Line);
    }
    if (pos.Column != expected.Column) {
        Ꮡt.Errorf("bad column for %q: got %d, expected %d"u8, lit, pos.Column, expected.Column);
    }
}

// Verify that calling Scan() provides the correct results.
public static void TestScan(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    nint whitespace_linecount = newlineCount(whitespace);
    // error handler
    var eh = (tokenꓸPosition _, @string msg) => {
        Ꮡt.Errorf("error handler called (msg = %s)"u8, msg);
    };
    // verify scan
    global::go.go.scanner_package.Scanner s = default!;
    s.Init(fset.AddFile(""u8, fset.Base(), len(source)), source, new Action<tokenꓸPosition, @string>(eh), (global::go.go.scanner_package.Mode)(ScanComments | dontInsertSemis));
    // set up expected position
    var epos = new tokenꓸPosition(
        Filename: ""u8,
        Offset: 0,
        Line: 1,
        Column: 1
    );
    nint index = 0;
    while (ᐧ) {
        var (pos, tok, lit) = s.Scan();
        // check position
        if (tok == token.EOF) {
            // correction for EOF
            epos.Line = newlineCount(((@string)source));
            epos.Column = 2;
        }
        checkPos(Ꮡt, lit, pos, epos);
        // check token
        var e = new elt(token.EOF, ""u8, special);
        if (index < len(tokens)) {
            e = tokens[index];
            index++;
        }
        if (tok != e.tok) {
            Ꮡt.Errorf("bad token for %q: got %s, expected %s"u8, lit, tok, e.tok);
        }
        // check token class
        if (tokenclass(tok) != e.@class) {
            Ꮡt.Errorf("bad class for %q: got %d, expected %d"u8, lit, tokenclass(tok), e.@class);
        }
        // check literal
        @string elit = ""u8;
        var exprᴛ1 = e.tok;
        if (exprᴛ1 == token.COMMENT) {
            elit = ((@string)stripCR(slice<byte>(e.lit), // no CRs in comments
 e.lit[1] == (rune)'*'));
            if (elit[1] == (rune)'/') {
                //-style comment literal doesn't contain newline
                elit = elit.slice(0, len(elit) - 1);
            }
        }
        else if (exprᴛ1 == token.IDENT) {
            elit = e.lit;
        }
        else if (exprᴛ1 == token.SEMICOLON) {
            elit = ";"u8;
        }
        else { /* default: */
            if (e.tok.IsLiteral()){
                // no CRs in raw string literals
                elit = e.lit;
                if (elit[0] == (rune)'`') {
                    elit = ((@string)stripCR(slice<byte>(elit), false));
                }
            } else 
            if (e.tok.IsKeyword()) {
                elit = e.lit;
            }
        }

        if (lit != elit) {
            Ꮡt.Errorf("bad literal for %q: got %q, expected %q"u8, lit, lit, elit);
        }
        if (tok == token.EOF) {
            break;
        }
        // update position
        epos.Offset += len(e.lit) + len(whitespace);
        epos.Line += newlineCount(e.lit) + whitespace_linecount;
    }
    if (s.ErrorCount != 0) {
        Ꮡt.Errorf("found %d errors"u8, s.ErrorCount);
    }
}

internal partial struct TestStripCR_type /*dyn*/ {
    internal @string have, want;
}

public static void TestStripCR(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestStripCR_type[]{
        new("//\n"u8, "//\n"u8),
        new("//\r\n"u8, "//\n"u8),
        new("//\r\r\r\n"u8, "//\n"u8),
        new("//\r*\r/\r\n"u8, "//*/\n"u8),
        new("/**/"u8, "/**/"u8),
        new("/*\r/*/"u8, "/*/*/"u8),
        new("/*\r*/"u8, "/**/"u8),
        new("/**\r/*/"u8, "/**\r/*/"u8),
        new("/*\r/\r*\r/*/"u8, "/*/*\r/*/"u8),
        new("/*\r\r\r\r*/"u8, "/**/"u8)
    }.slice()) {
        @string got = ((@string)stripCR(slice<byte>(test.have), len(test.have) >= 2 && test.have[1] == (rune)'*'));
        if (got != test.want) {
            Ꮡt.Errorf("stripCR(%q) = %q; want %q"u8, test.have, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string commentˢ = "COMMENT "u8;
internal static readonly @string commentˢ2 = " COMMENT"u8;
internal static readonly @string commentˢ3 = "COMMENT"u8;
internal static readonly @string testSemisˢ = "TestSemis"u8;

internal static void checkSemi(ж<testing.T> Ꮡt, @string input, @string want, global::go.go.scanner_package.Mode mode) {
    if ((global::go.go.scanner_package.Mode)(mode & ScanComments) == 0) {
        want = strings.ReplaceAll(want, commentˢ, ""u8);
        want = strings.ReplaceAll(want, commentˢ2, ""u8); // if at end
        want = strings.ReplaceAll(want, commentˢ3, ""u8); // if sole token
    }
    var @file = fset.AddFile(testSemisˢ, fset.Base(), len(input));
    global::go.go.scanner_package.Scanner scan = default!;
    scan.Init(@file, slice<byte>(input), default!, mode);
    slice<@string> tokens = default!;
    while (ᐧ) {
        var (pos, tok, lit) = scan.Scan();
        if (tok == token.EOF) {
            break;
        }
        if (tok == token.SEMICOLON && lit != ";"u8) {
            // Artificial semicolon:
            // assert that position is EOF or that of a newline.
            nint off = @file.Offset(pos);
            if (off != len(input) && input[off] != (rune)'\n') {
                Ꮡt.Errorf("scanning <<%s>>, got SEMICOLON at offset %d, want newline or EOF"u8, input, off);
            }
        }
        lit = tok.String(); // "\n" => ";"
        tokens = append(tokens, lit);
    }
    {
        @string got = strings.Join(tokens, " "u8); if (got != want) {
            Ꮡt.Errorf("scanning <<%s>>, got [%s], want [%s]"u8, input, got, want);
        }
    }
}

// first BOM is ignored

partial struct semicolonTestsᴛ1 /*dyn*/ {
    internal @string input, want;
}
internal static array<semicolonTestsᴛ1> semicolonTests = new semicolonTestsᴛ1[]{
    new(""u8, ""u8),
    new("\ufeff;"u8, ";"u8),
    new(";"u8, ";"u8),
    new("foo\n"u8, "IDENT ;"u8),
    new("123\n"u8, "INT ;"u8),
    new("1.2\n"u8, "FLOAT ;"u8),
    new("'x'\n"u8, "CHAR ;"u8),
    new(@"""x"""u8 + "\n"u8, "STRING ;"u8),
    new("`x`\n"u8, "STRING ;"u8),
    new("+\n"u8, "+"u8),
    new("-\n"u8, "-"u8),
    new("*\n"u8, "*"u8),
    new("/\n"u8, "/"u8),
    new("%\n"u8, "%"u8),
    new("&\n"u8, "&"u8),
    new("|\n"u8, "|"u8),
    new("^\n"u8, "^"u8),
    new("<<\n"u8, "<<"u8),
    new(">>\n"u8, ">>"u8),
    new("&^\n"u8, "&^"u8),
    new("+=\n"u8, "+="u8),
    new("-=\n"u8, "-="u8),
    new("*=\n"u8, "*="u8),
    new("/=\n"u8, "/="u8),
    new("%=\n"u8, "%="u8),
    new("&=\n"u8, "&="u8),
    new("|=\n"u8, "|="u8),
    new("^=\n"u8, "^="u8),
    new("<<=\n"u8, "<<="u8),
    new(">>=\n"u8, ">>="u8),
    new("&^=\n"u8, "&^="u8),
    new("&&\n"u8, "&&"u8),
    new("||\n"u8, "||"u8),
    new("<-\n"u8, "<-"u8),
    new("++\n"u8, "++ ;"u8),
    new("--\n"u8, "-- ;"u8),
    new("==\n"u8, "=="u8),
    new("<\n"u8, "<"u8),
    new(">\n"u8, ">"u8),
    new("=\n"u8, "="u8),
    new("!\n"u8, "!"u8),
    new("!=\n"u8, "!="u8),
    new("<=\n"u8, "<="u8),
    new(">=\n"u8, ">="u8),
    new(":=\n"u8, ":="u8),
    new("...\n"u8, "..."u8),
    new("(\n"u8, "("u8),
    new("[\n"u8, "["u8),
    new("{\n"u8, "{"u8),
    new(",\n"u8, ","u8),
    new(".\n"u8, "."u8),
    new(")\n"u8, ") ;"u8),
    new("]\n"u8, "] ;"u8),
    new("}\n"u8, "} ;"u8),
    new(";\n"u8, ";"u8),
    new(":\n"u8, ":"u8),
    new("break\n"u8, "break ;"u8),
    new("case\n"u8, "case"u8),
    new("chan\n"u8, "chan"u8),
    new("const\n"u8, "const"u8),
    new("continue\n"u8, "continue ;"u8),
    new("default\n"u8, "default"u8),
    new("defer\n"u8, "defer"u8),
    new("else\n"u8, "else"u8),
    new("fallthrough\n"u8, "fallthrough ;"u8),
    new("for\n"u8, "for"u8),
    new("func\n"u8, "func"u8),
    new("go\n"u8, "go"u8),
    new("goto\n"u8, "goto"u8),
    new("if\n"u8, "if"u8),
    new("import\n"u8, "import"u8),
    new("interface\n"u8, "interface"u8),
    new("map\n"u8, "map"u8),
    new("package\n"u8, "package"u8),
    new("range\n"u8, "range"u8),
    new("return\n"u8, "return ;"u8),
    new("select\n"u8, "select"u8),
    new("struct\n"u8, "struct"u8),
    new("switch\n"u8, "switch"u8),
    new("type\n"u8, "type"u8),
    new("var\n"u8, "var"u8),
    new("foo//comment\n"u8, "IDENT COMMENT ;"u8),
    new("foo//comment"u8, "IDENT COMMENT ;"u8),
    new("foo/*comment*/\n"u8, "IDENT COMMENT ;"u8),
    new("foo/*\n*/"u8, "IDENT COMMENT ;"u8),
    new("foo/*comment*/    \n"u8, "IDENT COMMENT ;"u8),
    new("foo/*\n*/    "u8, "IDENT COMMENT ;"u8),
    new("foo    // comment\n"u8, "IDENT COMMENT ;"u8),
    new("foo    // comment"u8, "IDENT COMMENT ;"u8),
    new("foo    /*comment*/\n"u8, "IDENT COMMENT ;"u8),
    new("foo    /*\n*/"u8, "IDENT COMMENT ;"u8),
    new("foo    /*  */ /* \n */ bar/**/\n"u8, "IDENT COMMENT COMMENT ; IDENT COMMENT ;"u8),
    new("foo    /*0*/ /*1*/ /*2*/\n"u8, "IDENT COMMENT COMMENT COMMENT ;"u8),
    new("foo    /*comment*/    \n"u8, "IDENT COMMENT ;"u8),
    new("foo    /*0*/ /*1*/ /*2*/    \n"u8, "IDENT COMMENT COMMENT COMMENT ;"u8),
    new("foo	/**/ /*-------------*/       /*----\n*/bar       /*  \n*/baa\n"u8, "IDENT COMMENT COMMENT COMMENT ; IDENT COMMENT ; IDENT ;"u8),
    new("foo    /* an EOF terminates a line */"u8, "IDENT COMMENT ;"u8),
    new("foo    /* an EOF terminates a line */ /*"u8, "IDENT COMMENT COMMENT ;"u8),
    new("foo    /* an EOF terminates a line */ //"u8, "IDENT COMMENT COMMENT ;"u8),
    new("package main\n\nfunc main() {\n\tif {\n\t\treturn /* */ }\n}\n"u8, "package IDENT ; func IDENT ( ) { if { return COMMENT } ; } ;"u8),
    new("package main"u8, "package IDENT ;"u8)
}.array();

public static void TestSemicolons(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in semicolonTests.ΔRangeSnapshot()) {
        @string input = test.input;
        @string want = test.want;
        checkSemi(Ꮡt, input, want, 0);
        checkSemi(Ꮡt, input, want, ScanComments);
        // if the input ended in newlines, the input must tokenize the
        // same with or without those newlines
        for (nint i = len(input) - 1; i >= 0 && input[i] == (rune)'\n'; i--) {
            checkSemi(Ꮡt, input.slice(0, i), want, 0);
            checkSemi(Ꮡt, input.slice(0, i), want, ScanComments);
        }
    }
}

internal partial struct segment {
    internal @string srcline; // a line of source text
    internal @string filename; // filename for current token; error message for invalid line directives
    internal nint line, column;   // line and column for current token; error position for invalid line directives
}

// exactly one token per line since the test consumes one token per segment
// bad line comment, ignored
// bad line comment, ignored (use existing, prior filename)
// bad line comment, ignored (use existing, prior filename)
// tests for new line directive syntax
// missing filename means empty filename
// missing filename means current filename
// missing filename means empty filename
// missing filename means current filename
// line-directive relative column
// absolute column since on new line
internal static slice<segment> segments = new segment[]{
    new("  line1"u8, "TestLineDirectives"u8, 1, 3),
    new("\nline2"u8, "TestLineDirectives"u8, 2, 1),
    new("\nline3  //line File1.go:100"u8, "TestLineDirectives"u8, 3, 1),
    new("\nline4"u8, "TestLineDirectives"u8, 4, 1),
    new("\n//line File1.go:100\n  line100"u8, "File1.go"u8, 100, 0),
    new("\n//line  \t :42\n  line1"u8, " \t "u8, 42, 0),
    new("\n//line File2.go:200\n  line200"u8, "File2.go"u8, 200, 0),
    new("\n//line foo\t:42\n  line42"u8, "foo\t"u8, 42, 0),
    new("\n //line foo:42\n  line43"u8, "foo\t"u8, 44, 0),
    new("\n//line foo 42\n  line44"u8, "foo\t"u8, 46, 0),
    new("\n//line /bar:42\n  line45"u8, "/bar"u8, 42, 0),
    new("\n//line ./foo:42\n  line46"u8, "foo"u8, 42, 0),
    new("\n//line a/b/c/File1.go:100\n  line100"u8, "a/b/c/File1.go"u8, 100, 0),
    new("\n//line c:\\bar:42\n  line200"u8, "c:\\bar"u8, 42, 0),
    new("\n//line c:\\dir\\File1.go:100\n  line201"u8, "c:\\dir\\File1.go"u8, 100, 0),
    new("\n//line :100\na1"u8, ""u8, 100, 0),
    new("\n//line bar:100\nb1"u8, "bar"u8, 100, 0),
    new("\n//line :100:10\nc1"u8, "bar"u8, 100, 10),
    new("\n//line foo:100:10\nd1"u8, "foo"u8, 100, 10),
    new("\n/*line :100*/a2"u8, ""u8, 100, 0),
    new("\n/*line bar:100*/b2"u8, "bar"u8, 100, 0),
    new("\n/*line :100:10*/c2"u8, "bar"u8, 100, 10),
    new("\n/*line foo:100:10*/d2"u8, "foo"u8, 100, 10),
    new("\n/*line foo:100:10*/    e2"u8, "foo"u8, 100, 14),
    new("\n/*line foo:100:10*/\n\nf2"u8, "foo"u8, 102, 1)
}.slice();

// exactly one token per line since the test consumes one token per segment
internal static slice<segment> dirsegments = new segment[]{
    new("  line1"u8, "TestLineDir/TestLineDirectives"u8, 1, 3),
    new("\n//line File1.go:100\n  line100"u8, "TestLineDir/File1.go"u8, 100, 0)
}.slice();

internal static slice<segment> dirUnixSegments = new segment[]{
    new("\n//line /bar:42\n  line42"u8, "/bar"u8, 42, 0)
}.slice();

internal static slice<segment> dirWindowsSegments = new segment[]{
    new("\n//line c:\\bar:42\n  line42"u8, "c:\\bar"u8, 42, 0)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testLineDirectivesˢ = "TestLineDirectives"u8;
internal static readonly @string testLineDirˢ = "TestLineDir/TestLineDirectives"u8;

// Verify that line directives are interpreted correctly.
public static void TestLineDirectives(ж<testing.T> Ꮡt) {
    testSegments(Ꮡt, segments, testLineDirectivesˢ);
    testSegments(Ꮡt, dirsegments, testLineDirˢ);
    if (runtime.GOOS == "windows"u8){
        testSegments(Ꮡt, dirWindowsSegments, testLineDirˢ);
    } else {
        testSegments(Ꮡt, dirUnixSegments, testLineDirˢ);
    }
}

internal static void testSegments(ж<testing.T> Ꮡt, slice<segment> segments, @string filename) {
    @string src = default!;
    foreach (var (_, e) in segments) {
        src += e.srcline;
    }
    // verify scan
    global::go.go.scanner_package.Scanner S = default!;
    var @file = fset.AddFile(filename, fset.Base(), len(src));
    S.Init(@file, slice<byte>(src), (tokenꓸPosition pos, @string msg) => {
        Ꮡt.Error(new ΔError(pos, msg));
    }, dontInsertSemis);
    foreach (var (_, s) in segments) {
        var (p, _, lit) = S.Scan();
        var pos = @file.Position(p);
        checkPos(Ꮡt, lit, p, new tokenꓸPosition(
            Filename: s.filename,
            Offset: pos.Offset,
            Line: s.line,
            Column: s.column
        ));
    }
    if (S.ErrorCount != 0) {
        Ꮡt.Errorf("got %d errors"u8, S.ErrorCount);
    }
}

// foo is considered part of the filename
// The filename is used for the error message in these test cases.
// The first line directive is valid and used to control the expected error line.
internal static slice<segment> invalidSegments = new segment[]{
    new("\n//line :1:1\n//line foo:42 extra text\ndummy"u8, "invalid line number: 42 extra text"u8, 1, 12),
    new("\n//line :2:1\n//line foobar:\ndummy"u8, "invalid line number: "u8, 2, 15),
    new("\n//line :5:1\n//line :0\ndummy"u8, "invalid line number: 0"u8, 5, 9),
    new("\n//line :10:1\n//line :1:0\ndummy"u8, "invalid column number: 0"u8, 10, 11),
    new("\n//line :1:1\n//line :foo:0\ndummy"u8, "invalid line number: 0"u8, 1, 13)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dirˢ = "dir"u8;

// Verify that invalid line directives get the correct error message.
public static void TestInvalidLineDirectives(ж<testing.T> Ꮡt) {
    // make source
    @string src = default!;
    foreach (var (_, e) in invalidSegments) {
        src += e.srcline;
    }
    // verify scan
    global::go.go.scanner_package.Scanner S = default!;
    ref var s = ref heap(new segment(), out var Ꮡs);               // current segment
    var @file = fset.AddFile(filepath.Join(dirˢ, "TestInvalidLineDirectives"), fset.Base(), len(src));
    S.Init(@file, slice<byte>(src), (tokenꓸPosition pos, @string msg) => {
        if (msg != Ꮡs.Value.filename) {
            Ꮡt.Errorf("got error %q; want %q"u8, msg, Ꮡs.Value.filename);
        }
        if (pos.Line != Ꮡs.Value.line || pos.Column != Ꮡs.Value.column) {
            Ꮡt.Errorf("got position %d:%d; want %d:%d"u8, pos.Line, pos.Column, Ꮡs.Value.line, Ꮡs.Value.column);
        }
    }, dontInsertSemis);
    foreach (var (_, vᴛ1) in invalidSegments) {
        s = vᴛ1;

        S.Scan();
    }
    if (S.ErrorCount != len(invalidSegments)) {
        Ꮡt.Errorf("got %d errors; want %d"u8, S.ErrorCount, len(invalidSegments));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ifTrueˢ = "if true { }"u8;
internal static readonly @string src1ˢ = "src1"u8;
internal static readonly @string goTrueˢ = "go true { ]"u8;
internal static readonly @string src2ˢ = "src2"u8;

// Verify that initializing the same scanner more than once works correctly.
public static void TestInit(ж<testing.T> Ꮡt) {
    global::go.go.scanner_package.Scanner s = default!;
    // 1st init
    @string src1 = ifTrueˢ;
    var f1 = fset.AddFile(src1ˢ, fset.Base(), len(src1));
    s.Init(f1, slice<byte>(src1), default!, dontInsertSemis);
    if (f1.Size() != len(src1)) {
        Ꮡt.Errorf("bad file size: got %d, expected %d"u8, f1.Size(), len(src1));
    }
    s.Scan(); // if
    s.Scan(); // true
    var (_, tok, _) = s.Scan(); // {
    if (tok != token.LBRACE) {
        Ꮡt.Errorf("bad token: got %s, expected %s"u8, tok, token.LBRACE);
    }
    // 2nd init
    @string src2 = goTrueˢ;
    var f2 = fset.AddFile(src2ˢ, fset.Base(), len(src2));
    s.Init(f2, slice<byte>(src2), default!, dontInsertSemis);
    if (f2.Size() != len(src2)) {
        Ꮡt.Errorf("bad file size: got %d, expected %d"u8, f2.Size(), len(src2));
    }
    (_, tok, _) = s.Scan(); // go
    if (tok != token.GO) {
        Ꮡt.Errorf("bad token: got %s, expected %s"u8, tok, token.GO);
    }
    if (s.ErrorCount != 0) {
        Ꮡt.Errorf("found %d errors"u8, s.ErrorCount);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string file1ˢ = "File1"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ = "@\n@ @\n//line File2:20\n@\n//line File2:1\n@ @\n//line File1:1\n@ @ @";

public static void TestStdErrorHandler(ж<testing.T> Ꮡt) {
// illegal character, cause an error
// two errors on the same line
// different file, but same line
// same file, decreasing line number
    @string src = srcᶜ; // original file, line 1 again
    ref var list = ref heap<global::go.go.scanner_package.ErrorList>(out var Ꮡlist);
    var eh = (tokenꓸPosition pos, @string msg) => {
        Ꮡlist.ValueSlot.Add(pos, msg);
    };
    global::go.go.scanner_package.Scanner s = default!;
    s.Init(fset.AddFile(file1ˢ, fset.Base(), len(src)), slice<byte>(src), new Action<tokenꓸPosition, @string>(eh), dontInsertSemis);
    while (ᐧ) {
        {
            var (_, tok, _) = s.Scan(); if (tok == token.EOF) {
                break;
            }
        }
    }
    if (len(list) != s.ErrorCount) {
        Ꮡt.Errorf("found %d errors, expected %d"u8, len(list), s.ErrorCount);
    }
    if (len(list) != 9) {
        Ꮡt.Errorf("found %d raw errors, expected 9"u8, len(list));
        PrintError(new os.FileжWriter(os.Stderr), list);
    }
    list.Sort();
    if (len(list) != 9) {
        Ꮡt.Errorf("found %d sorted errors, expected 9"u8, len(list));
        PrintError(new os.FileжWriter(os.Stderr), list);
    }
    Ꮡlist.RemoveMultiples();
    if (len(list) != 4) {
        Ꮡt.Errorf("found %d one-per-line errors, expected 4"u8, len(list));
        PrintError(new os.FileжWriter(os.Stderr), list);
    }
}

internal partial struct errorCollector {
    internal nint cnt;           // number of errors encountered
    internal @string msg;        // last error message encountered
    internal tokenꓸPosition pos; // last error position encountered
}

internal static void checkError(ж<testing.T> Ꮡt, @string src, token.Token tok, nint pos, @string lit, @string err) {
    global::go.go.scanner_package.Scanner s = default!;
    ref var h = ref heap(new errorCollector(), out var Ꮡh);
    var eh = (tokenꓸPosition posΔ1, @string msg) => {
        Ꮡh.Value.cnt++;
        Ꮡh.Value.msg = msg;
        Ꮡh.Value.pos = posΔ1;
    };
    s.Init(fset.AddFile(""u8, fset.Base(), len(src)), slice<byte>(src), new Action<tokenꓸPosition, @string>(eh), (global::go.go.scanner_package.Mode)(ScanComments | dontInsertSemis));
    var (_, tok0, lit0) = s.Scan();
    if (tok0 != tok) {
        Ꮡt.Errorf("%q: got %s, expected %s"u8, src, tok0, tok);
    }
    if (tok0 != token.ILLEGAL && lit0 != lit) {
        Ꮡt.Errorf("%q: got literal %q, expected %q"u8, src, lit0, lit);
    }
    nint cnt = 0;
    if (err != ""u8) {
        cnt = 1;
    }
    if (h.cnt != cnt) {
        Ꮡt.Errorf("%q: got cnt %d, expected %d"u8, src, h.cnt, cnt);
    }
    if (h.msg != err) {
        Ꮡt.Errorf("%q: got msg %q, expected %q"u8, src, h.msg, err);
    }
    if (h.pos.Offset != pos) {
        Ꮡt.Errorf("%q: got offset %d, expected %d"u8, src, h.pos.Offset, pos);
    }
}

// two periods, not invalid token (issue #28112)
// issue 17621
// only first BOM is ignored
// only first BOM is ignored
// only first BOM is ignored
// only first BOM is ignored

partial struct errorsᴛ1 /*dyn*/ {
    internal @string src;
    internal token.Token tok;
    internal nint pos;
    internal @string lit;
    internal @string err;
}
internal static slice<errorsᴛ1> errors = new errorsᴛ1[]{
    new("\a"u8, token.ILLEGAL, 0, ""u8, "illegal character U+0007"u8),
    new(@"#"u8, token.ILLEGAL, 0, ""u8, "illegal character U+0023 '#'"u8),
    new(@"…"u8, token.ILLEGAL, 0, ""u8, "illegal character U+2026 '…'"u8),
    new(".."u8, token.PERIOD, 0, ""u8, ""u8),
    new(@"' '"u8, token.CHAR, 0, @"' '"u8, ""u8),
    new(@"''"u8, token.CHAR, 0, @"''"u8, "illegal rune literal"u8),
    new(@"'12'"u8, token.CHAR, 0, @"'12'"u8, "illegal rune literal"u8),
    new(@"'123'"u8, token.CHAR, 0, @"'123'"u8, "illegal rune literal"u8),
    new(@"'\0'"u8, token.CHAR, 3, @"'\0'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\07'"u8, token.CHAR, 4, @"'\07'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\8'"u8, token.CHAR, 2, @"'\8'"u8, "unknown escape sequence"u8),
    new(@"'\08'"u8, token.CHAR, 3, @"'\08'"u8, "illegal character U+0038 '8' in escape sequence"u8),
    new(@"'\x'"u8, token.CHAR, 3, @"'\x'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\x0'"u8, token.CHAR, 4, @"'\x0'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\x0g'"u8, token.CHAR, 4, @"'\x0g'"u8, "illegal character U+0067 'g' in escape sequence"u8),
    new(@"'\u'"u8, token.CHAR, 3, @"'\u'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\u0'"u8, token.CHAR, 4, @"'\u0'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\u00'"u8, token.CHAR, 5, @"'\u00'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\u000'"u8, token.CHAR, 6, @"'\u000'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\u000"u8, token.CHAR, 6, @"'\u000"u8, "escape sequence not terminated"u8),
    new(@"'\u0000'"u8, token.CHAR, 0, @"'\u0000'"u8, ""u8),
    new(@"'\U'"u8, token.CHAR, 3, @"'\U'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\U0'"u8, token.CHAR, 4, @"'\U0'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\U00'"u8, token.CHAR, 5, @"'\U00'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\U000'"u8, token.CHAR, 6, @"'\U000'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\U0000'"u8, token.CHAR, 7, @"'\U0000'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\U00000'"u8, token.CHAR, 8, @"'\U00000'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\U000000'"u8, token.CHAR, 9, @"'\U000000'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\U0000000'"u8, token.CHAR, 10, @"'\U0000000'"u8, "illegal character U+0027 ''' in escape sequence"u8),
    new(@"'\U0000000"u8, token.CHAR, 10, @"'\U0000000"u8, "escape sequence not terminated"u8),
    new(@"'\U00000000'"u8, token.CHAR, 0, @"'\U00000000'"u8, ""u8),
    new(@"'\Uffffffff'"u8, token.CHAR, 2, @"'\Uffffffff'"u8, "escape sequence is invalid Unicode code point"u8),
    new(@"'"u8, token.CHAR, 0, @"'"u8, "rune literal not terminated"u8),
    new(@"'\"u8, token.CHAR, 2, @"'\"u8, "escape sequence not terminated"u8),
    new("'\n"u8, token.CHAR, 0, "'"u8, "rune literal not terminated"u8),
    new("'\n   "u8, token.CHAR, 0, "'"u8, "rune literal not terminated"u8),
    new(@""""""u8, token.STRING, 0, @""""""u8, ""u8),
    new(@"""abc"u8, token.STRING, 0, @"""abc"u8, "string literal not terminated"u8),
    new("\"abc\n"u8, token.STRING, 0, @"""abc"u8, "string literal not terminated"u8),
    new("\"abc\n   "u8, token.STRING, 0, @"""abc"u8, "string literal not terminated"u8),
    new("``"u8, token.STRING, 0, "``"u8, ""u8),
    new("`"u8, token.STRING, 0, "`"u8, "raw string literal not terminated"u8),
    new("/**/"u8, token.COMMENT, 0, "/**/"u8, ""u8),
    new("/*"u8, token.COMMENT, 0, "/*"u8, "comment not terminated"u8),
    new("077"u8, token.INT, 0, "077"u8, ""u8),
    new("078."u8, token.FLOAT, 0, "078."u8, ""u8),
    new("07801234567."u8, token.FLOAT, 0, "07801234567."u8, ""u8),
    new("078e0"u8, token.FLOAT, 0, "078e0"u8, ""u8),
    new("0E"u8, token.FLOAT, 2, "0E"u8, "exponent has no digits"u8),
    new("078"u8, token.INT, 2, "078"u8, "invalid digit '8' in octal literal"u8),
    new("07090000008"u8, token.INT, 3, "07090000008"u8, "invalid digit '9' in octal literal"u8),
    new("0x"u8, token.INT, 2, "0x"u8, "hexadecimal literal has no digits"u8),
    new(((@string)(new byte[]{0x22, 0x61, 0x62, 0x63, 0x00, 0x64, 0x65, 0x66, 0x22})), token.STRING, 4, ((@string)(new byte[]{0x22, 0x61, 0x62, 0x63, 0x00, 0x64, 0x65, 0x66, 0x22})), "illegal character NUL"u8),
    new(((@string)(new byte[]{0x22, 0x61, 0x62, 0x63, 0x80, 0x64, 0x65, 0x66, 0x22})), token.STRING, 4, ((@string)(new byte[]{0x22, 0x61, 0x62, 0x63, 0x80, 0x64, 0x65, 0x66, 0x22})), "illegal UTF-8 encoding"u8),
    new("\ufeff\ufeff"u8, token.ILLEGAL, 3, "\ufeff\ufeff"u8, "illegal byte order mark"u8),
    new("//\ufeff"u8, token.COMMENT, 2, "//\ufeff"u8, "illegal byte order mark"u8),
    new("'\ufeff"u8 + @"'"u8, token.CHAR, 1, "'\ufeff"u8 + @"'"u8, "illegal byte order mark"u8),
    new(@""""u8 + "abc\ufeffdef"u8 + @""""u8, token.STRING, 4, @""""u8 + "abc\ufeffdef"u8 + @""""u8, "illegal byte order mark"u8),
    new(((@string)(new byte[]{0x61, 0x62, 0x63, 0x00, 0x64, 0x65, 0x66})), token.IDENT, 3, "abc"u8, "illegal character NUL"u8),
    new("abc\x00"u8, token.IDENT, 3, "abc"u8, "illegal character NUL"u8),
    new("“abc”"u8, token.ILLEGAL, 0, "abc"u8, @"curly quotation mark '“' (use neutral '""')"u8)
}.slice();

public static void TestScanErrors(ж<testing.T> Ꮡt) {
    foreach (var (_, e) in errors) {
        checkError(Ꮡt, e.src, e.tok, e.pos, e.lit, e.err);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ1 = """

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
	
"""u8;

// Verify that no comments show up as literal values when skipping comments.
public static void TestIssue10213(ж<testing.T> Ꮡt) {
    @string src = srcᶜ1;
    global::go.go.scanner_package.Scanner s = default!;
    s.Init(fset.AddFile(""u8, fset.Base(), len(src)), slice<byte>(src), default!, 0);
    while (ᐧ) {
        var (pos, tok, lit) = s.Scan();
        nint @class = tokenclass(tok);
        if (lit != ""u8 && @class != keyword && @class != literal && tok != token.SEMICOLON) {
            Ꮡt.Errorf("%s: tok = %s, lit = %q"u8, fset.Position(pos), tok, lit);
        }
        if (tok <= token.EOF) {
            break;
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ2 = "... .. 0.. .."u8;

public static void TestIssue28112(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string src = srcᶜ2; // make sure to have stand-alone ".." immediately before EOF to test EOF behavior
    var tokens = new token.Token[]{token.ELLIPSIS, token.PERIOD, token.PERIOD, token.FLOAT, token.PERIOD, token.PERIOD, token.PERIOD, token.EOF}.slice();
    global::go.go.scanner_package.Scanner s = default!;
    s.Init(fset.AddFile(""u8, fset.Base(), len(src)), slice<byte>(src), default!, 0);
    foreach (var (_, want) in tokens) {
        var (pos, got, lit) = s.Scan();
        if (got != want) {
            Ꮡt.Errorf("%s: got %s, want %s"u8, fset.Position(pos), got, want);
        }
        // literals expect to have a (non-empty) literal string and we don't care about other tokens for this test
        if (tokenclass(got) == literal && lit == ""u8) {
            Ꮡt.Errorf("%s: for %s got empty literal string"u8, fset.Position(pos), got);
        }
    }
}

public static void BenchmarkScan(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    var fset = token.NewFileSet();
    var @file = fset.AddFile(""u8, fset.Base(), len(source));
    global::go.go.scanner_package.Scanner s = default!;
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        s.Init(@file, source, default!, ScanComments);
        while (ᐧ) {
            var (_, tok, _) = s.Scan();
            if (tok == token.EOF) {
                break;
            }
        }
    }
}

public static void BenchmarkScanFiles(ж<testing.B> Ꮡb) {
    // Scan a few arbitrary large files, and one small one, to provide some
    // variety in benchmarks.
    foreach (var (_, p) in new @string[]{
        "go/types/expr.go"u8,
        "go/parser/parser.go"u8,
        "net/http/server.go"u8,
        "go/scanner/errors.go"u8
    }.slice()) {
        Ꮡb.Run(p, (ж<testing.B> bΔ1) => {
            bΔ1.StopTimer();
            @string filename = filepath.Join(".."u8, "..", filepath.FromSlash(p));
            var (src, err) = os.ReadFile(filename);
            if (err != default!) {
                bΔ1.Fatal(err);
            }
            var fset = token.NewFileSet();
            var @file = fset.AddFile(filename, fset.Base(), len(src));
            bΔ1.SetBytes((int64)len(src));
            global::go.go.scanner_package.Scanner s = default!;
            bΔ1.StartTimer();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                s.Init(@file, src, default!, ScanComments);
                while (ᐧ) {
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

public static void TestNumbers(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestNumbers_type[]{ // binaries

        new(token.INT, "0b0"u8, "0b0"u8, ""u8),
        new(token.INT, "0b1010"u8, "0b1010"u8, ""u8),
        new(token.INT, "0B1110"u8, "0B1110"u8, ""u8),
        new(token.INT, "0b"u8, "0b"u8, "binary literal has no digits"u8),
        new(token.INT, "0b0190"u8, "0b0190"u8, "invalid digit '9' in binary literal"u8),
        new(token.INT, "0b01a0"u8, "0b01 a0"u8, ""u8), // only accept 0-9

        new(token.FLOAT, "0b."u8, "0b."u8, "invalid radix point in binary literal"u8),
        new(token.FLOAT, "0b.1"u8, "0b.1"u8, "invalid radix point in binary literal"u8),
        new(token.FLOAT, "0b1.0"u8, "0b1.0"u8, "invalid radix point in binary literal"u8),
        new(token.FLOAT, "0b1e10"u8, "0b1e10"u8, "'e' exponent requires decimal mantissa"u8),
        new(token.FLOAT, "0b1P-1"u8, "0b1P-1"u8, "'P' exponent requires hexadecimal mantissa"u8),
        new(token.IMAG, "0b10i"u8, "0b10i"u8, ""u8),
        new(token.IMAG, "0b10.0i"u8, "0b10.0i"u8, "invalid radix point in binary literal"u8), // octals

        new(token.INT, "0o0"u8, "0o0"u8, ""u8),
        new(token.INT, "0o1234"u8, "0o1234"u8, ""u8),
        new(token.INT, "0O1234"u8, "0O1234"u8, ""u8),
        new(token.INT, "0o"u8, "0o"u8, "octal literal has no digits"u8),
        new(token.INT, "0o8123"u8, "0o8123"u8, "invalid digit '8' in octal literal"u8),
        new(token.INT, "0o1293"u8, "0o1293"u8, "invalid digit '9' in octal literal"u8),
        new(token.INT, "0o12a3"u8, "0o12 a3"u8, ""u8), // only accept 0-9

        new(token.FLOAT, "0o."u8, "0o."u8, "invalid radix point in octal literal"u8),
        new(token.FLOAT, "0o.2"u8, "0o.2"u8, "invalid radix point in octal literal"u8),
        new(token.FLOAT, "0o1.2"u8, "0o1.2"u8, "invalid radix point in octal literal"u8),
        new(token.FLOAT, "0o1E+2"u8, "0o1E+2"u8, "'E' exponent requires decimal mantissa"u8),
        new(token.FLOAT, "0o1p10"u8, "0o1p10"u8, "'p' exponent requires hexadecimal mantissa"u8),
        new(token.IMAG, "0o10i"u8, "0o10i"u8, ""u8),
        new(token.IMAG, "0o10e0i"u8, "0o10e0i"u8, "'e' exponent requires decimal mantissa"u8), // 0-octals

        new(token.INT, "0"u8, "0"u8, ""u8),
        new(token.INT, "0123"u8, "0123"u8, ""u8),
        new(token.INT, "08123"u8, "08123"u8, "invalid digit '8' in octal literal"u8),
        new(token.INT, "01293"u8, "01293"u8, "invalid digit '9' in octal literal"u8),
        new(token.INT, "0F."u8, "0 F ."u8, ""u8), // only accept 0-9

        new(token.INT, "0123F."u8, "0123 F ."u8, ""u8),
        new(token.INT, "0123456x"u8, "0123456 x"u8, ""u8), // decimals

        new(token.INT, "1"u8, "1"u8, ""u8),
        new(token.INT, "1234"u8, "1234"u8, ""u8),
        new(token.INT, "1f"u8, "1 f"u8, ""u8), // only accept 0-9

        new(token.IMAG, "0i"u8, "0i"u8, ""u8),
        new(token.IMAG, "0678i"u8, "0678i"u8, ""u8), // decimal floats

        new(token.FLOAT, "0."u8, "0."u8, ""u8),
        new(token.FLOAT, "123."u8, "123."u8, ""u8),
        new(token.FLOAT, "0123."u8, "0123."u8, ""u8),
        new(token.FLOAT, ".0"u8, ".0"u8, ""u8),
        new(token.FLOAT, ".123"u8, ".123"u8, ""u8),
        new(token.FLOAT, ".0123"u8, ".0123"u8, ""u8),
        new(token.FLOAT, "0.0"u8, "0.0"u8, ""u8),
        new(token.FLOAT, "123.123"u8, "123.123"u8, ""u8),
        new(token.FLOAT, "0123.0123"u8, "0123.0123"u8, ""u8),
        new(token.FLOAT, "0e0"u8, "0e0"u8, ""u8),
        new(token.FLOAT, "123e+0"u8, "123e+0"u8, ""u8),
        new(token.FLOAT, "0123E-1"u8, "0123E-1"u8, ""u8),
        new(token.FLOAT, "0.e+1"u8, "0.e+1"u8, ""u8),
        new(token.FLOAT, "123.E-10"u8, "123.E-10"u8, ""u8),
        new(token.FLOAT, "0123.e123"u8, "0123.e123"u8, ""u8),
        new(token.FLOAT, ".0e-1"u8, ".0e-1"u8, ""u8),
        new(token.FLOAT, ".123E+10"u8, ".123E+10"u8, ""u8),
        new(token.FLOAT, ".0123E123"u8, ".0123E123"u8, ""u8),
        new(token.FLOAT, "0.0e1"u8, "0.0e1"u8, ""u8),
        new(token.FLOAT, "123.123E-10"u8, "123.123E-10"u8, ""u8),
        new(token.FLOAT, "0123.0123e+456"u8, "0123.0123e+456"u8, ""u8),
        new(token.FLOAT, "0e"u8, "0e"u8, "exponent has no digits"u8),
        new(token.FLOAT, "0E+"u8, "0E+"u8, "exponent has no digits"u8),
        new(token.FLOAT, "1e+f"u8, "1e+ f"u8, "exponent has no digits"u8),
        new(token.FLOAT, "0p0"u8, "0p0"u8, "'p' exponent requires hexadecimal mantissa"u8),
        new(token.FLOAT, "1.0P-1"u8, "1.0P-1"u8, "'P' exponent requires hexadecimal mantissa"u8),
        new(token.IMAG, "0.i"u8, "0.i"u8, ""u8),
        new(token.IMAG, ".123i"u8, ".123i"u8, ""u8),
        new(token.IMAG, "123.123i"u8, "123.123i"u8, ""u8),
        new(token.IMAG, "123e+0i"u8, "123e+0i"u8, ""u8),
        new(token.IMAG, "123.E-10i"u8, "123.E-10i"u8, ""u8),
        new(token.IMAG, ".123E+10i"u8, ".123E+10i"u8, ""u8), // hexadecimals

        new(token.INT, "0x0"u8, "0x0"u8, ""u8),
        new(token.INT, "0x1234"u8, "0x1234"u8, ""u8),
        new(token.INT, "0xcafef00d"u8, "0xcafef00d"u8, ""u8),
        new(token.INT, "0XCAFEF00D"u8, "0XCAFEF00D"u8, ""u8),
        new(token.INT, "0x"u8, "0x"u8, "hexadecimal literal has no digits"u8),
        new(token.INT, "0x1g"u8, "0x1 g"u8, ""u8),
        new(token.IMAG, "0xf00i"u8, "0xf00i"u8, ""u8), // hexadecimal floats

        new(token.FLOAT, "0x0p0"u8, "0x0p0"u8, ""u8),
        new(token.FLOAT, "0x12efp-123"u8, "0x12efp-123"u8, ""u8),
        new(token.FLOAT, "0xABCD.p+0"u8, "0xABCD.p+0"u8, ""u8),
        new(token.FLOAT, "0x.0189P-0"u8, "0x.0189P-0"u8, ""u8),
        new(token.FLOAT, "0x1.ffffp+1023"u8, "0x1.ffffp+1023"u8, ""u8),
        new(token.FLOAT, "0x."u8, "0x."u8, "hexadecimal literal has no digits"u8),
        new(token.FLOAT, "0x0."u8, "0x0."u8, "hexadecimal mantissa requires a 'p' exponent"u8),
        new(token.FLOAT, "0x.0"u8, "0x.0"u8, "hexadecimal mantissa requires a 'p' exponent"u8),
        new(token.FLOAT, "0x1.1"u8, "0x1.1"u8, "hexadecimal mantissa requires a 'p' exponent"u8),
        new(token.FLOAT, "0x1.1e0"u8, "0x1.1e0"u8, "hexadecimal mantissa requires a 'p' exponent"u8),
        new(token.FLOAT, "0x1.2gp1a"u8, "0x1.2 gp1a"u8, "hexadecimal mantissa requires a 'p' exponent"u8),
        new(token.FLOAT, "0x0p"u8, "0x0p"u8, "exponent has no digits"u8),
        new(token.FLOAT, "0xeP-"u8, "0xeP-"u8, "exponent has no digits"u8),
        new(token.FLOAT, "0x1234PAB"u8, "0x1234P AB"u8, "exponent has no digits"u8),
        new(token.FLOAT, "0x1.2p1a"u8, "0x1.2p1 a"u8, ""u8),
        new(token.IMAG, "0xf00.bap+12i"u8, "0xf00.bap+12i"u8, ""u8), // separators

        new(token.INT, "0b_1000_0001"u8, "0b_1000_0001"u8, ""u8),
        new(token.INT, "0o_600"u8, "0o_600"u8, ""u8),
        new(token.INT, "0_466"u8, "0_466"u8, ""u8),
        new(token.INT, "1_000"u8, "1_000"u8, ""u8),
        new(token.FLOAT, "1_000.000_1"u8, "1_000.000_1"u8, ""u8),
        new(token.IMAG, "10e+1_2_3i"u8, "10e+1_2_3i"u8, ""u8),
        new(token.INT, "0x_f00d"u8, "0x_f00d"u8, ""u8),
        new(token.FLOAT, "0x_f00d.0p1_2"u8, "0x_f00d.0p1_2"u8, ""u8),
        new(token.INT, "0b__1000"u8, "0b__1000"u8, "'_' must separate successive digits"u8),
        new(token.INT, "0o60___0"u8, "0o60___0"u8, "'_' must separate successive digits"u8),
        new(token.INT, "0466_"u8, "0466_"u8, "'_' must separate successive digits"u8),
        new(token.FLOAT, "1_."u8, "1_."u8, "'_' must separate successive digits"u8),
        new(token.FLOAT, "0._1"u8, "0._1"u8, "'_' must separate successive digits"u8),
        new(token.FLOAT, "2.7_e0"u8, "2.7_e0"u8, "'_' must separate successive digits"u8),
        new(token.IMAG, "10e+12_i"u8, "10e+12_i"u8, "'_' must separate successive digits"u8),
        new(token.INT, "0x___0"u8, "0x___0"u8, "'_' must separate successive digits"u8),
        new(token.FLOAT, "0x1.0_p0"u8, "0x1.0_p0"u8, "'_' must separate successive digits"u8)
    }.slice()) {
        global::go.go.scanner_package.Scanner s = default!;
        @string err = default!;
        s.Init(fset.AddFile(""u8, fset.Base(), len(test.src)), slice<byte>(test.src), (tokenꓸPosition _Δp0, @string msg) => {
            if (err == ""u8) {
                err = msg;
            }
        }, 0);
        foreach (var (i, want) in strings.Split(test.tokens, " "u8)) {
            err = ""u8;
            var (_, tokΔ1, lit) = s.Scan();
            // compute lit where for tokens where lit is not defined
            var exprᴛ1 = tokΔ1;
            if (exprᴛ1 == token.PERIOD) {
                lit = "."u8;
            }
            else if (exprᴛ1 == token.ADD) {
                lit = "+"u8;
            }
            else if (exprᴛ1 == token.SUB) {
                lit = "-"u8;
            }

            if (i == 0) {
                if (tokΔ1 != test.tok) {
                    Ꮡt.Errorf("%q: got token %s; want %s"u8, test.src, tokΔ1, test.tok);
                }
                if (err != test.err) {
                    Ꮡt.Errorf("%q: got error %q; want %q"u8, test.src, err, test.err);
                }
            }
            if (lit != want) {
                Ꮡt.Errorf("%q: got literal %q (%s); want %s"u8, test.src, lit, tokΔ1, want);
            }
        }
        // make sure we read all
        var (_, tok, _) = s.Scan();
        if (tok == token.SEMICOLON) {
            (_, tok, _) = s.Scan();
        }
        if (tok != token.EOF) {
            Ꮡt.Errorf("%q: got %s; want EOF"u8, test.src, tok);
        }
    }
}

} // end scanner_internal_test_package
