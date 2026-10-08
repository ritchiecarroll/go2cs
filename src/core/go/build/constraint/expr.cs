// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package constraint implements parsing and evaluation of build constraint lines.
// See https://golang.org/cmd/go/#hdr-Build_constraints for documentation about build constraints themselves.
//
// This package parses both the original “// +build” syntax and the “//go:build” syntax that was added in Go 1.17.
// See https://golang.org/design/draft-gobuild for details about the “//go:build” syntax.
namespace go.go.build;

using errors = errors_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = global::go.unicode.utf8_package;
using global::go.unicode;

partial class constraint_package {

// maxSize is a limit used to control the complexity of expressions, in order
// to prevent stack exhaustion issues due to recursion.
internal static UntypedInt maxSize => 1000;

// An Expr is a build tag constraint expression.
// The underlying concrete type is *[AndExpr], *[OrExpr], *[NotExpr], or *[TagExpr].
partial interface Expr {
    // String returns the string form of the expression,
    // using the boolean syntax used in //go:build lines.
    @string String();
    // Eval reports whether the expression evaluates to true.
    // It calls ok(tag) as needed to find out whether a given build tag
    // is satisfied by the current build configuration.
    bool Eval(Func<@string, bool> ok);
    // The presence of an isExpr method explicitly marks the type as an Expr.
    // Only implementations in this package should be used as Exprs.
    void isExpr();
}

// A TagExpr is an [Expr] for the single tag Tag.
partial struct TagExpr {
    public @string Tag; // for example, “linux” or “cgo”
}

internal static void isExpr(this ref TagExpr x) {
}

public static bool Eval(this ref TagExpr x, Func<@string, bool> ok) {
    return ok(x.Tag);
}

public static @string String(this ref TagExpr x) {
    return x.Tag;
}

internal static Expr tag(@string tag) {
    return new TagExprжExpr(Ꮡ(new TagExpr(tag)));
}

// A NotExpr represents the expression !X (the negation of X).
partial struct NotExpr {
    public Expr X;
}

internal static void isExpr(this ref NotExpr x) {
}

public static bool Eval(this ref NotExpr x, Func<@string, bool> ok) {
    return !x.X.Eval(ok);
}

public static @string String(this ref NotExpr x) {
    @string s = x.X.String();
    switch (x.X.type()) {
    case ж<AndExpr> _:
    case ж<OrExpr> _: {
        s = "("u8 + s + ")"u8;
        break;
    }}

    return "!"u8 + s;
}

internal static Expr not(Expr x) {
    return new NotExprжExpr(Ꮡ(new NotExpr(x)));
}

// An AndExpr represents the expression X && Y.
partial struct AndExpr {
    public Expr X, Y;
}

internal static void isExpr(this ref AndExpr x) {
}

public static bool Eval(this ref AndExpr x, Func<@string, bool> ok) {
    // Note: Eval both, to make sure ok func observes all tags.
    var xok = x.X.Eval(ok);
    var yok = x.Y.Eval(ok);
    return xok && yok;
}

public static @string String(this ref AndExpr x) {
    return andArg(x.X) + " && "u8 + andArg(x.Y);
}

internal static @string andArg(Expr x) {
    @string s = x.String();
    {
        var (_, ok) = x._<ж<OrExpr>>(ᐧ); if (ok) {
            s = "("u8 + s + ")"u8;
        }
    }
    return s;
}

internal static Expr and(Expr x, Expr y) {
    return new AndExprжExpr(Ꮡ(new AndExpr(x, y)));
}

// An OrExpr represents the expression X || Y.
partial struct OrExpr {
    public Expr X, Y;
}

internal static void isExpr(this ref OrExpr x) {
}

public static bool Eval(this ref OrExpr x, Func<@string, bool> ok) {
    // Note: Eval both, to make sure ok func observes all tags.
    var xok = x.X.Eval(ok);
    var yok = x.Y.Eval(ok);
    return xok || yok;
}

public static @string String(this ref OrExpr x) {
    return orArg(x.X) + " || "u8 + orArg(x.Y);
}

internal static @string orArg(Expr x) {
    @string s = x.String();
    {
        var (_, ok) = x._<ж<AndExpr>>(ᐧ); if (ok) {
            s = "("u8 + s + ")"u8;
        }
    }
    return s;
}

internal static Expr or(Expr x, Expr y) {
    return new OrExprжExpr(Ꮡ(new OrExpr(x, y)));
}

// A SyntaxError reports a syntax error in a parsed build expression.
partial struct SyntaxError {
    public nint Offset;   // byte offset in input where error was detected
    public @string Err; // description of error
}

public static @string Error(this ref SyntaxError e) {
    return e.Err;
}

internal static error errNotConstraint = errors.New("not a build constraint"u8);

// Parse parses a single build constraint line of the form “//go:build ...” or “// +build ...”
// and returns the corresponding boolean expression.
public static (Expr, error) Parse(@string line) {
    {
        var (text, ok) = splitGoBuild(line); if (ok) {
            return parseExpr(text);
        }
    }
    {
        var (text, ok) = splitPlusBuild(line); if (ok) {
            return parsePlusBuildExpr(text);
        }
    }
    return (default!, errNotConstraint);
}

// IsGoBuild reports whether the line of text is a “//go:build” constraint.
// It only checks the prefix of the text, not that the expression itself parses.
public static bool IsGoBuild(@string line) {
    var (_, ok) = splitGoBuild(line);
    return ok;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goBuildˢ = "//go:build"u8;

// splitGoBuild splits apart the leading //go:build prefix in line from the build expression itself.
// It returns "", false if the input is not a //go:build line or if the input contains multiple lines.
internal static (@string expr, bool ok) splitGoBuild(@string line) {
    // A single trailing newline is OK; otherwise multiple lines are not.
    if (len(line) > 0 && line[len(line) - 1] == (rune)'\n') {
        line = line.slice(0, len(line) - 1);
    }
    if (strings.Contains(line, "\n"u8)) {
        return ("", false);
    }
    if (!strings.HasPrefix(line, goBuildˢ)) {
        return ("", false);
    }
    line = strings.TrimSpace(line);
    line = line[(int)(len("//go:build"))..];
    // If strings.TrimSpace finds more to trim after removing the //go:build prefix,
    // it means that the prefix was followed by a space, making this a //go:build line
    // (as opposed to a //go:buildsomethingelse line).
    // If line is empty, we had "//go:build" by itself, which also counts.
    @string trim = strings.TrimSpace(line);
    if (len(line) == len(trim) && line != ""u8) {
        return ("", false);
    }
    return (trim, true);
}

// An exprParser holds state for parsing a build expression.
partial struct exprParser {
    internal @string s; // input string
    internal nint i;   // next read location in s
    internal @string tok; // last token read
    internal bool isTag;
    internal nint pos; // position (start) of last token
    internal nint size;
}

// parseExpr parses a boolean build tag expression.
internal static (Expr x, error err) parseExpr(@string text) {
    Expr x = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var e = recover(); if (e != default!) {
                    {
                        var (eΔ1, ok) = e._<ж<SyntaxError>>(ᐧ); if (ok) {
                            err = new SyntaxErrorжerror(eΔ1);
                            return;
                        }
                    }
                    throw panic(e); // unreachable unless parser has a bug
                }
            }
        }, ref ᒐ);
        var p = Ꮡ(new exprParser(s: text));
        x = p.or();
        if ((~p).tok != ""u8) {
            throw panic(Ꮡ(new SyntaxError(Offset: (~p).pos, Err: "unexpected token "u8 + (~p).tok)));
        }
        (x, err) = (x, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (x, err);
}

// or parses a sequence of || expressions.
// On entry, the next input token has not yet been lexed.
// On exit, the next input token has been lexed and is in p.tok.
internal static Expr or(this ж<exprParser> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    var x = Ꮡp.and();
    while (p.tok == "||"u8) {
        x = or(x, Ꮡp.and());
    }
    return x;
}

// and parses a sequence of && expressions.
// On entry, the next input token has not yet been lexed.
// On exit, the next input token has been lexed and is in p.tok.
internal static Expr and(this ж<exprParser> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    var x = Ꮡp.not();
    while (p.tok == "&&"u8) {
        x = and(x, Ꮡp.not());
    }
    return x;
}

// not parses a ! expression.
// On entry, the next input token has not yet been lexed.
// On exit, the next input token has been lexed and is in p.tok.
internal static Expr not(this ж<exprParser> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    p.size++;
    if (p.size > maxSize) {
        throw panic(Ꮡ(new SyntaxError(Offset: p.pos, Err: "build expression too large"u8)));
    }
    p.lex();
    if (p.tok == "!"u8) {
        p.lex();
        if (p.tok == "!"u8) {
            throw panic(Ꮡ(new SyntaxError(Offset: p.pos, Err: "double negation not allowed"u8)));
        }
        return not(Ꮡp.atom());
    }
    return Ꮡp.atom();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingCloseParenˢ = "missing close paren"u8;

// atom parses a tag or a parenthesized expression.
// On entry, the next input token HAS been lexed.
// On exit, the next input token has been lexed and is in p.tok.
internal static Expr atom(this ж<exprParser> Ꮡp) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        // first token already in p.tok
        if (p.tok == "("u8) {
            ref var pos = ref heap<nint>(out var Ꮡpos);
            pos = p.pos;
            defer(() => {
                {
                    var e = recover(); if (e != default!) {
                        {
                            var (eΔ1, ok) = e._<ж<SyntaxError>>(ᐧ); if (ok && (~eΔ1).Err == "unexpected end of expression"u8) {
                                eΔ1.Value.Err = missingCloseParenˢ;
                            }
                        }
                        throw panic(e);
                    }
                }
            }, ref ᒐ);
            var x = Ꮡp.or();
            if (p.tok != ")"u8) {
                throw panic(Ꮡ(new SyntaxError(Offset: pos, Err: "missing close paren"u8)));
            }
            p.lex();
            return x;
        }
        if (!p.isTag) {
            if (p.tok == ""u8) {
                throw panic(Ꮡ(new SyntaxError(Offset: p.pos, Err: "unexpected end of expression"u8)));
            }
            throw panic(Ꮡ(new SyntaxError(Offset: p.pos, Err: "unexpected token "u8 + p.tok)));
        }
        @string tok = p.tok;
        p.lex();
        return tag(tok);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// lex finds and consumes the next token in the input stream.
// On return, p.tok is set to the token text,
// p.isTag reports whether the token was a tag,
// and p.pos records the byte offset of the start of the token in the input stream.
// If lex reaches the end of the input, p.tok is set to the empty string.
// For any other syntax error, lex panics with a SyntaxError.
internal static void lex(this ref exprParser p) {
    p.isTag = false;
    while (p.i < len(p.s) && (p.s[p.i] == (rune)' ' || p.s[p.i] == (rune)'\t')) {
        p.i++;
    }
    if (p.i >= len(p.s)) {
        p.tok = ""u8;
        p.pos = p.i;
        return;
    }
    switch (p.s[p.i]) {
    case (rune)'(' or (rune)')' or (rune)'!': {
        p.pos = p.i;
        p.i++;
        p.tok = p.s.slice(p.pos, p.i);
        return;
    }
    case (rune)'&' or (rune)'|': {
        if (p.i + 1 >= len(p.s) || p.s[p.i + 1] != p.s[p.i]) {
            throw panic(Ꮡ(new SyntaxError(Offset: p.i, Err: "invalid syntax at "u8 + ((@string)(rune)p.s[p.i]))));
        }
        p.pos = p.i;
        p.i += 2;
        p.tok = p.s.slice(p.pos, p.i);
        return;
    }}

    @string tag = p.s.slice(p.i);
    foreach (var (i, c) in tag) {
        if (!unicode.IsLetter(c) && !unicode.IsDigit(c) && c != (rune)'_' && c != (rune)'.') {
            tag = tag.slice(0, i);
            break;
        }
    }
    if (tag == ""u8) {
        var (c, _) = utf8.DecodeRuneInString(p.s.slice(p.i));
        throw panic(Ꮡ(new SyntaxError(Offset: p.i, Err: "invalid syntax at "u8 + ((@string)c))));
    }
    p.pos = p.i;
    p.i += len(tag);
    p.tok = p.s.slice(p.pos, p.i);
    p.isTag = true;
}

// IsPlusBuild reports whether the line of text is a “// +build” constraint.
// It only checks the prefix of the text, not that the expression itself parses.
public static bool IsPlusBuild(@string line) {
    var (_, ok) = splitPlusBuild(line);
    return ok;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string buildˢ = "+build"u8;

// splitPlusBuild splits apart the leading // +build prefix in line from the build expression itself.
// It returns "", false if the input is not a // +build line or if the input contains multiple lines.
internal static (@string expr, bool ok) splitPlusBuild(@string line) {
    // A single trailing newline is OK; otherwise multiple lines are not.
    if (len(line) > 0 && line[len(line) - 1] == (rune)'\n') {
        line = line.slice(0, len(line) - 1);
    }
    if (strings.Contains(line, "\n"u8)) {
        return ("", false);
    }
    if (!strings.HasPrefix(line, "//"u8)) {
        return ("", false);
    }
    line = line[(int)(len("//"))..];
    // Note the space is optional; "//+build" is recognized too.
    line = strings.TrimSpace(line);
    if (!strings.HasPrefix(line, buildˢ)) {
        return ("", false);
    }
    line = line[(int)(len("+build"))..];
    // If strings.TrimSpace finds more to trim after removing the +build prefix,
    // it means that the prefix was followed by a space, making this a +build line
    // (as opposed to a +buildsomethingelse line).
    // If line is empty, we had "// +build" by itself, which also counts.
    @string trim = strings.TrimSpace(line);
    if (len(line) == len(trim) && line != ""u8) {
        return ("", false);
    }
    return (trim, true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ignoreˢ = "ignore"u8;

// parsePlusBuildExpr parses a legacy build tag expression (as used with “// +build”).
internal static (Expr, error) parsePlusBuildExpr(@string text) {
    // Only allow up to 100 AND/OR operators for "old" syntax.
    // This is much less than the limit for "new" syntax,
    // but uses of old syntax were always very simple.
    const nint maxOldSize = 100;
    nint size = 0;
    Expr x = default!;
    foreach (var (_, clause) in strings.Fields(text)) {
        Expr y = default!;
        foreach (var (_, vᴛ1) in strings.Split(clause, ","u8)) {
            var lit = vᴛ1;

            Expr z = default!;
            bool neg = default!;
            if (strings.HasPrefix(lit, "!!"u8) || lit == "!"u8){
                z = tag(ignoreˢ);
            } else {
                if (strings.HasPrefix(lit, "!"u8)) {
                    neg = true;
                    lit = lit[(int)(len("!"))..];
                }
                if (isValidTag(lit)){
                    z = tag(lit);
                } else {
                    z = tag(ignoreˢ);
                }
                if (neg) {
                    z = not(z);
                }
            }
            if (y == default!){
                y = z;
            } else {
                {
                    size++; if (size > maxOldSize) {
                        return (default!, errComplex);
                    }
                }
                y = and(y, z);
            }
        }
        if (x == default!){
            x = y;
        } else {
            {
                size++; if (size > maxOldSize) {
                    return (default!, errComplex);
                }
            }
            x = or(x, y);
        }
    }
    if (x == default!) {
        x = tag(ignoreˢ);
    }
    return (x, default!);
}

// isValidTag reports whether the word is a valid build tag.
// Tags must be letters, digits, underscores or dots.
// Unlike in Go identifiers, all digits are fine (e.g., "386").
internal static bool isValidTag(@string word) {
    if (word == ""u8) {
        return false;
    }
    foreach (var (_, c) in word) {
        if (!unicode.IsLetter(c) && !unicode.IsDigit(c) && c != (rune)'_' && c != (rune)'.') {
            return false;
        }
    }
    return true;
}

internal static error errComplex = errors.New("expression too complex for // +build lines"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string buildˢ2 = "// +build"u8;

// PlusBuildLines returns a sequence of “// +build” lines that evaluate to the build expression x.
// If the expression is too complex to convert directly to “// +build” lines, PlusBuildLines returns an error.
public static (slice<@string>, error) PlusBuildLines(Expr x) {
    // Push all NOTs to the expression leaves, so that //go:build !(x && y) can be treated as !x || !y.
    // This rewrite is both efficient and commonly needed, so it's worth doing.
    // Essentially all other possible rewrites are too expensive and too rarely needed.
    x = pushNot(x, false);
    // Split into AND of ORs of ANDs of literals (tag or NOT tag).
    slice<slice<slice<Expr>>> split = default!;
    foreach (var (_, or) in appendSplitAnd(default!, x)) {
        slice<slice<Expr>> ands = default!;
        foreach (var (_, and) in appendSplitOr(default!, or)) {
            slice<Expr> lits = default!;
            foreach (var (_, lit) in appendSplitAnd(default!, and)) {
                switch (lit.type()) {
                case ж<TagExpr> _:
                case ж<NotExpr> _: {
                    lits = append(lits, lit);
                    break;
                }
                default: {
                    return (default!, errComplex);
                }}

            }
            ands = append(ands, lits);
        }
        split = append(split, ands);
    }
    // If all the ORs have length 1 (no actual OR'ing going on),
    // push the top-level ANDs to the bottom level, so that we get
    // one // +build line instead of many.
    nint maxOr = 0;
    foreach (var (_, or) in split) {
        if (maxOr < len(or)) {
            maxOr = len(or);
        }
    }
    if (maxOr == 1) {
        slice<Expr> lits = default!;
        foreach (var (_, or) in split) {
            lits = appendꓸꓸꓸ(lits, or[0]);
        }
        split = new slice<slice<Expr>>[]{new slice<Expr>[]{lits}.slice()}.slice();
    }
    // Prepare the +build lines.
    slice<@string> lines = default!;
    foreach (var (_, or) in split) {
        @string line = buildˢ2;
        foreach (var (_, and) in or) {
            @string clause = ""u8;
            foreach (var (i, lit) in and) {
                if (i > 0) {
                    clause += ","u8;
                }
                clause += lit.String();
            }
            line += " "u8 + clause;
        }
        lines = append(lines, line);
    }
    return (lines, default!);
}

// pushNot applies DeMorgan's law to push negations down the expression,
// so that only tags are negated in the result.
// (It applies the rewrites !(X && Y) => (!X || !Y) and !(X || Y) => (!X && !Y).)
internal static Expr pushNot(Expr x, bool not) {
    switch (x.type()) {
    default: {
        var xΔ1 = x;
        return xΔ1;
    }
    case ж<NotExpr> xΔ1: {
        {
            var (_, ok) = (~xΔ1).X._<ж<TagExpr>>(ᐧ); if (ok && !not) {
                // unreachable
                return new NotExprжExpr(xΔ1);
            }
        }
        return pushNot((~xΔ1).X, !not);
    }
    case ж<TagExpr> xΔ1: {
        if (not) {
            return new NotExprжExpr(Ꮡ(new NotExpr(X: new TagExprжExpr(xΔ1))));
        }
        return new TagExprжExpr(xΔ1);
    }
    case ж<AndExpr> xΔ1: {
        var x1 = pushNot((~xΔ1).X, not);
        var y1 = pushNot((~xΔ1).Y, not);
        if (not) {
            return or(x1, y1);
        }
        if (AreEqual(x1, (~xΔ1).X) && AreEqual(y1, (~xΔ1).Y)) {
            return new AndExprжExpr(xΔ1);
        }
        return and(x1, y1);
    }
    case ж<OrExpr> xΔ1: {
        var x1 = pushNot((~xΔ1).X, not);
        var y1 = pushNot((~xΔ1).Y, not);
        if (not) {
            return and(x1, y1);
        }
        if (AreEqual(x1, (~xΔ1).X) && AreEqual(y1, (~xΔ1).Y)) {
            return new OrExprжExpr(xΔ1);
        }
        return or(x1, y1);
    }}
}

// appendSplitAnd appends x to list while splitting apart any top-level && expressions.
// For example, appendSplitAnd({W}, X && Y && Z) = {W, X, Y, Z}.
internal static slice<Expr> appendSplitAnd(slice<Expr> list, Expr x) {
    {
        var (xΔ1, ok) = x._<ж<AndExpr>>(ᐧ); if (ok) {
            list = appendSplitAnd(list, (~xΔ1).X);
            list = appendSplitAnd(list, (~xΔ1).Y);
            return list;
        }
    }
    return append(list, x);
}

// appendSplitOr appends x to list while splitting apart any top-level || expressions.
// For example, appendSplitOr({W}, X || Y || Z) = {W, X, Y, Z}.
internal static slice<Expr> appendSplitOr(slice<Expr> list, Expr x) {
    {
        var (xΔ1, ok) = x._<ж<OrExpr>>(ᐧ); if (ok) {
            list = appendSplitOr(list, (~xΔ1).X);
            list = appendSplitOr(list, (~xΔ1).Y);
            return list;
        }
    }
    return append(list, x);
}

} // end constraint_package
