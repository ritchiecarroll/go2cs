// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.build;

using fmt = fmt_package;
using maps = maps_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using static global::go.go.build.constraint_package;

partial class constraint_internal_test_package {

partial struct exprStringTestsᴛ1 /*dyn*/ {
    internal global::go.go.build.constraint_package.Expr x;
    internal @string @out;
}
internal static slice<exprStringTestsᴛ1> exprStringTests = slice<exprStringTestsᴛ1>([
    new(
        x: tag("abc"),
        @out: "abc"
    ),
    new(
        x: not(tag("abc")),
        @out: "!abc"
    ),
    new(
        x: not(and(tag("abc"), tag("def"))),
        @out: "!(abc && def)"
    ),
    new(
        x: and(tag("abc"), or(tag("def"), tag("ghi"))),
        @out: "abc && (def || ghi)"
    ),
    new(
        x: or(and(tag("abc"), tag("def")), tag("ghi")),
        @out: "(abc && def) || ghi"
    )
]);

public static void TestExprString(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in range(exprStringTests)) {
        ref var tt = ref heap(new exprStringTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            @string s = ttʗ1.x.String();
            if (s != ttʗ1.@out) {
                tΔ1.Errorf("String() mismatch:\nhave %s\nwant %s", s, ttʗ1.@out);
            }
        });
    }
}

partial struct lexTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string @out;
}
internal static slice<lexTestsᴛ1> lexTests = [
    new("", ""),
    new("x", "x"),
    new("x.y", "x.y"),
    new("x_y", "x_y"),
    new("αx", "αx"),
    new("αx²", "αx err: invalid syntax at ²"),
    new("go1.2", "go1.2"),
    new("x y", "x y"),
    new("x!y", "x ! y"),
    new("&&||!()xy yx ", "&& || ! ( ) xy yx"),
    new("x~", "x err: invalid syntax at ~"),
    new("x ~", "x err: invalid syntax at ~"),
    new("x &", "x err: invalid syntax at &"),
    new("x &y", "x err: invalid syntax at &")
];

public static void TestLex(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in range(lexTests)) {
        ref var tt = ref heap(new lexTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            var p = Ꮡ(new exprParser(s: ttʗ1.@in));
            @string @out = ""u8;
            for (;;) {
                var (tok, err) = lexHelp(p);
                if (tok == ""u8 && err == nil) {
                    break;
                }
                if (@out != ""u8) {
                    @out += " "u8;
                }
                if (err != nil) {
                    @out += "err: "u8 + err.Error();
                    break;
                }
                @out += tok;
            }
            if (@out != ttʗ1.@out) {
                tΔ1.Errorf("lex(%q):\nhave %s\nwant %s", ttʗ1.@in, @out, ttʗ1.@out);
            }
        });
    }
}

internal static (@string tok, error err) lexHelp(ж<global::go.go.build.constraint_package.exprParser> Ꮡp) {
    @string tok = default;
    error err = default;
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        defer(() => {
            var e = recover(); if (e != nil) {
                var (eΔ1, ok) = e._<ж<global::go.go.build.constraint_package.SyntaxError>>(ᐧ); if (ok) {
                    err = new global::go.go.build.constraint_package.SyntaxErrorжerror(eΔ1);
                    return;
                }
                throw panic(e);
            }
        }, ref ᒐ);

        p.lex();
        (tok, err) = (p.tok, default);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (tok, err);
}

partial struct parseExprTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal global::go.go.build.constraint_package.Expr x;
}
internal static slice<parseExprTestsᴛ1> parseExprTests = slice<parseExprTestsᴛ1>([
    new("x", tag("x")),
    new("x&&y", and(tag("x"), tag("y"))),
    new("x||y", or(tag("x"), tag("y"))),
    new("(x)", tag("x")),
    new("x||y&&z", or(tag("x"), and(tag("y"), tag("z")))),
    new("x&&y||z", or(and(tag("x"), tag("y")), tag("z"))),
    new("x&&(y||z)", and(tag("x"), or(tag("y"), tag("z")))),
    new("(x||y)&&z", and(or(tag("x"), tag("y")), tag("z"))),
    new("!(x&&y)", not(and(tag("x"), tag("y"))))
]);

public static void TestParseExpr(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in range(parseExprTests)) {
        ref var tt = ref heap(new parseExprTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            var (x, err) = parseExpr(ttʗ1.@in);
            if (err != nil) {
                tΔ1.Fatal(err);
            }
            if (x.String() != ttʗ1.x.String()) {
                tΔ1.Errorf("parseExpr(%q):\nhave %s\nwant %s", ttʗ1.@in, x, ttʗ1.x);
            }
        });
    }
}

partial struct parseExprErrorTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal error err;
}
internal static slice<parseExprErrorTestsᴛ1> parseExprErrorTests = [
    new("x && ", new global::go.go.build.constraint_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(Offset: 5, Err: "unexpected end of expression")))),
    new("x && (", new global::go.go.build.constraint_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(Offset: 6, Err: "missing close paren")))),
    new("x && ||", new global::go.go.build.constraint_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(Offset: 5, Err: "unexpected token ||")))),
    new("x && !", new global::go.go.build.constraint_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(Offset: 6, Err: "unexpected end of expression")))),
    new("x && !!", new global::go.go.build.constraint_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(Offset: 6, Err: "double negation not allowed")))),
    new("x !", new global::go.go.build.constraint_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(Offset: 2, Err: "unexpected token !")))),
    new("x && (y", new global::go.go.build.constraint_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(Offset: 5, Err: "missing close paren"))))
];

public static void TestParseError(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in range(parseExprErrorTests)) {
        ref var tt = ref heap(new parseExprErrorTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            var (x, err) = parseExpr(ttʗ1.@in);
            if (err == nil) {
                tΔ1.Fatalf("parseExpr(%q) = %v, want error", ttʗ1.@in, x);
            }
            if (!reflect.DeepEqual(err, ttʗ1.err)) {
                tΔ1.Fatalf("parseExpr(%q): wrong error:\nhave %#v\nwant %#v", ttʗ1.@in, err, ttʗ1.err);
            }
        });
    }
}

partial struct exprEvalTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal bool ok;
    internal @string tags;
}
internal static slice<exprEvalTestsᴛ1> exprEvalTests = [
    new("x", false, "x"),
    new("x && y", false, "x y"),
    new("x || y", false, "x y"),
    new("!x && yes", true, "x yes"),
    new("yes || y", true, "y yes")
];

public static void TestExprEval(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in range(exprEvalTests)) {
        ref var tt = ref heap(new exprEvalTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            var (x, err) = parseExpr(ttʗ1.@in);
            if (err != nil) {
                tΔ1.Fatal(err);
            }
            var tags = new map<@string, bool>();
            var wantTags = new map<@string, bool>();
            foreach (var (_, tag) in range(strings.Fields(ttʗ1.tags))) {
                wantTags[tag] = true;
            }
            var tagsʗ1 = tags;
            var hasTag = (@string tag) => {
                tagsʗ1[tag] = true;
                return tag == "yes"u8;
            };
            var ok = x.Eval(hasTag);
            if (ok != ttʗ1.ok || !maps.Equal<map<@string, bool>, map<@string, bool>, @string, bool>(tags, wantTags)) {
                tΔ1.Errorf("Eval(%#q):\nhave ok=%v, tags=%v\nwant ok=%v, tags=%v",
                    ttʗ1.@in, ok, tags, ttʗ1.ok, wantTags);
            }
        });
    }
}

internal static slice<parseExprTestsᴛ1> parsePlusBuildExprTests = slice<parseExprTestsᴛ1>([
    new("x", tag("x")),
    new("x,y", and(tag("x"), tag("y"))),
    new("x y", or(tag("x"), tag("y"))),
    new("x y,z", or(tag("x"), and(tag("y"), tag("z")))),
    new("x,y z", or(and(tag("x"), tag("y")), tag("z"))),
    new("x,!y !z", or(and(tag("x"), not(tag("y"))), not(tag("z")))),
    new("!! x", or(tag("ignore"), tag("x"))),
    new("!!x", tag("ignore")),
    new("!x", not(tag("x"))),
    new("!", tag("ignore")),
    new("", tag("ignore"))
]);

public static void TestParsePlusBuildExpr(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in range(parsePlusBuildExprTests)) {
        ref var tt = ref heap(new parseExprTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            var (x, _) = parsePlusBuildExpr(ttʗ1.@in);
            if (x.String() != ttʗ1.x.String()) {
                tΔ1.Errorf("parsePlusBuildExpr(%q):\nhave %v\nwant %v", ttʗ1.@in, x, ttʗ1.x);
            }
        });
    }
}

partial struct constraintTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal global::go.go.build.constraint_package.Expr x;
    internal @string err;
}
internal static slice<constraintTestsᴛ1> constraintTests = slice<constraintTestsᴛ1>([
    new("//+build !", tag("ignore"), ""),
    new("//+build", tag("ignore"), ""),
    new("//+build x y", or(tag("x"), tag("y")), ""),
    new("// +build x y \n", or(tag("x"), tag("y")), ""),
    new("// +build x y \n ", default, "not a build constraint"),
    new("// +build x y \nmore", default, "not a build constraint"),
    new(" //+build x y", default, "not a build constraint"),

    new("//go:build x && y", and(tag("x"), tag("y")), ""),
    new("//go:build x && y\n", and(tag("x"), tag("y")), ""),
    new("//go:build x && y\n ", default, "not a build constraint"),
    new("//go:build x && y\nmore", default, "not a build constraint"),
    new(" //go:build x && y", default, "not a build constraint"),
    new("//go:build\n", default, "unexpected end of expression")
]);

public static void TestParse(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in range(constraintTests)) {
        ref var tt = ref heap(new constraintTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            var (x, err) = Parse(ttʗ1.@in);
            if (err != nil) {
                if (ttʗ1.err == ""u8) {
                    tΔ1.Errorf("Constraint(%q): unexpected error: %v", ttʗ1.@in, err);
                } else if (!strings.Contains(err.Error(), ttʗ1.err)) {
                    tΔ1.Errorf("Constraint(%q): error %v, want %v", ttʗ1.@in, err, ttʗ1.err);
                }
                return;
            }
            if (ttʗ1.err != ""u8) {
                tΔ1.Errorf("Constraint(%q) = %v, want error %v", ttʗ1.@in, x, ttʗ1.err);
                return;
            }
            if (x.String() != ttʗ1.x.String()) {
                tΔ1.Errorf("Constraint(%q):\nhave %v\nwant %v", ttʗ1.@in, x, ttʗ1.x);
            }
        });
    }
}

partial struct plusBuildLinesTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal slice<@string> @out;
    internal error err;
}
internal static slice<plusBuildLinesTestsᴛ1> plusBuildLinesTests;
internal static void initᴛplusBuildLinesTests() { plusBuildLinesTests = [
    new("x", ["x"], default),
    new("x && !y", ["x,!y"], default),
    new("x || y", ["x y"], default),
    new("x && (y || z)", ["x", "y z"], default),
    new("!(x && y)", ["!x !y"], default),
    new("x || (y && z)", ["x y,z"], default),
    new("w && (x || (y && z))", ["w", "x y,z"], default),
    new("v || (w && (x || (y && z)))", default, errComplex)
]; }

public static void TestPlusBuildLines(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in range(plusBuildLinesTests)) {
        ref var tt = ref heap(new plusBuildLinesTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            var (x, err) = parseExpr(ttʗ1.@in);
            if (err != nil) {
                tΔ1.Fatal(err);
            }
            (var lines, err) = PlusBuildLines(x);
            if (err != nil) {
                if (ttʗ1.err == nil) {
                    tΔ1.Errorf("PlusBuildLines(%q): unexpected error: %v", ttʗ1.@in, err);
                } else if (!AreEqual(ttʗ1.err, err)) {
                    tΔ1.Errorf("PlusBuildLines(%q): error %v, want %v", ttʗ1.@in, err, ttʗ1.err);
                }
                return;
            }
            if (ttʗ1.err != nil) {
                tΔ1.Errorf("PlusBuildLines(%q) = %v, want error %v", ttʗ1.@in, lines, ttʗ1.err);
                return;
            }
            slice<@string> want = default;
            foreach (var (_, line) in range(ttʗ1.@out)) {
                want = append(want, "// +build "u8 + line);
            }
            if (!slices.Equal<slice<@string>, @string>(lines, want)) {
                tΔ1.Errorf("PlusBuildLines(%q):\nhave %q\nwant %q", ttʗ1.@in, lines, want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expressionDidNotTriggerˢ = (@string)"expression did not trigger limit"u8;

internal partial struct TestSizeLimits_type /*dyn*/ {
    internal @string name;
    internal @string expr;
}

public static void TestSizeLimits(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(slice<TestSizeLimits_type>([
        new(
            name: "go:build or limit",
            expr: "//go:build "u8 + strings.Repeat("a || ", maxSize + 2)
        ),
        new(
            name: "go:build and limit",
            expr: "//go:build "u8 + strings.Repeat("a && ", maxSize + 2)
        ),
        new(
            name: "go:build and depth limit",
            expr: "//go:build "u8 + strings.Repeat("(a &&", maxSize + 2)
        ),
        new(
            name: "go:build or depth limit",
            expr: "//go:build "u8 + strings.Repeat("(a ||", maxSize + 2)
        )
    ]))) {
        ref var tc = ref heap(new TestSizeLimits_type(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            var (_, err) = Parse(tcʗ1.expr);
            if (err == nil) {
                tΔ1.Error(expressionDidNotTriggerˢ);
            } else if (err._<ж<global::go.go.build.constraint_package.SyntaxError>>(ᐧ) is var (syntaxErr, ok) && (!ok || syntaxErr.Err != "build expression too large"u8)) {
                if (!ok) {
                    tΔ1.Errorf("unexpected error: %v", err);
                } else {
                    tΔ1.Errorf("unexpected syntax error: %s", syntaxErr.Err);
                }
            }
        });
    }
}

internal partial struct TestPlusSizeLimits_type /*dyn*/ {
    internal @string name;
    internal @string expr;
}

public static void TestPlusSizeLimits(ж<testing.T> t) {
    nint maxOldSize = 100;
    foreach (var (_, vᴛ1) in range(slice<TestPlusSizeLimits_type>([
        new(
            name: "+build or limit",
            expr: "// +build "u8 + strings.Repeat("a ", maxOldSize + 2)
        ),
        new(
            name: "+build and limit",
            expr: "// +build "u8 + strings.Repeat("a,", maxOldSize + 2)
        )
    ]))) {
        ref var tc = ref heap(new TestPlusSizeLimits_type(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            var (_, err) = Parse(tcʗ1.expr);
            if (err == nil) {
                tΔ1.Error(expressionDidNotTriggerˢ);
            } else if (!AreEqual(err, errComplex)) {
                tΔ1.Errorf("unexpected error: got %q, want %q", err, errComplex);
            }
        });
    }
}

} // end constraint_internal_test_package
