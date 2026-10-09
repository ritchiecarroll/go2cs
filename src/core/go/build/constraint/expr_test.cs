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
internal static slice<exprStringTestsᴛ1> exprStringTests = new exprStringTestsᴛ1[]{
    new(
        x: tag("abc"u8),
        @out: "abc"u8
    ),
    new(
        x: not(tag("abc"u8)),
        @out: "!abc"u8
    ),
    new(
        x: not(and(tag("abc"u8), tag("def"u8))),
        @out: "!(abc && def)"u8
    ),
    new(
        x: and(tag("abc"u8), or(tag("def"u8), tag("ghi"u8))),
        @out: "abc && (def || ghi)"u8
    ),
    new(
        x: or(and(tag("abc"u8), tag("def"u8)), tag("ghi"u8)),
        @out: "(abc && def) || ghi"u8
    )
}.slice();

public static void TestExprString(ж<testing.T> Ꮡt) {
    foreach (var (i, vᴛ1) in exprStringTests) {
        ref var tt = ref heap(new exprStringTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            @string s = ttʗ1.x.String();
            if (s != ttʗ1.@out) {
                tΔ1.Errorf("String() mismatch:\nhave %s\nwant %s"u8, s, ttʗ1.@out);
            }
        });
    }
}


partial struct lexTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string @out;
}
internal static slice<lexTestsᴛ1> lexTests = new lexTestsᴛ1[]{
    new(""u8, ""u8),
    new("x"u8, "x"u8),
    new("x.y"u8, "x.y"u8),
    new("x_y"u8, "x_y"u8),
    new("αx"u8, "αx"u8),
    new("αx²"u8, "αx err: invalid syntax at ²"u8),
    new("go1.2"u8, "go1.2"u8),
    new("x y"u8, "x y"u8),
    new("x!y"u8, "x ! y"u8),
    new("&&||!()xy yx "u8, "&& || ! ( ) xy yx"u8),
    new("x~"u8, "x err: invalid syntax at ~"u8),
    new("x ~"u8, "x err: invalid syntax at ~"u8),
    new("x &"u8, "x err: invalid syntax at &"u8),
    new("x &y"u8, "x err: invalid syntax at &"u8)
}.slice();

public static void TestLex(ж<testing.T> Ꮡt) {
    foreach (var (i, vᴛ1) in lexTests) {
        ref var tt = ref heap(new lexTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            var p = Ꮡ(new exprParser(s: ttʗ1.@in));
            @string @out = ""u8;
            while (ᐧ) {
                var (tok, err) = lexHelp(p);
                if (tok == ""u8 && err == default!) {
                    break;
                }
                if (@out != ""u8) {
                    @out += " "u8;
                }
                if (err != default!) {
                    @out += "err: "u8 + err.Error();
                    break;
                }
                @out += tok;
            }
            if (@out != ttʗ1.@out) {
                tΔ1.Errorf("lex(%q):\nhave %s\nwant %s"u8, ttʗ1.@in, @out, ttʗ1.@out);
            }
        });
    }
}

internal static (@string tok, error err) lexHelp(ж<global::go.go.build.constraint_package.exprParser> Ꮡp) {
    @string tok = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        defer(() => {
            {
                var e = recover(); if (e != default!) {
                    {
                        var (eΔ1, ok) = e._<ж<global::go.go.build.constraint_package.SyntaxError>>(ᐧ); if (ok) {
                            err = new global::go.go.build.constraint_package.SyntaxErrorжerror(eΔ1);
                            return;
                        }
                    }
                    throw panic(e);
                }
            }
        }, ref ᒐ);
        p.lex();
        (tok, err) = (p.tok, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (tok, err);
}


partial struct parseExprTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal global::go.go.build.constraint_package.Expr x;
}
internal static slice<parseExprTestsᴛ1> parseExprTests = new parseExprTestsᴛ1[]{
    new("x"u8, tag("x"u8)),
    new("x&&y"u8, and(tag("x"u8), tag("y"u8))),
    new("x||y"u8, or(tag("x"u8), tag("y"u8))),
    new("(x)"u8, tag("x"u8)),
    new("x||y&&z"u8, or(tag("x"u8), and(tag("y"u8), tag("z"u8)))),
    new("x&&y||z"u8, or(and(tag("x"u8), tag("y"u8)), tag("z"u8))),
    new("x&&(y||z)"u8, and(tag("x"u8), or(tag("y"u8), tag("z"u8)))),
    new("(x||y)&&z"u8, and(or(tag("x"u8), tag("y"u8)), tag("z"u8))),
    new("!(x&&y)"u8, not(and(tag("x"u8), tag("y"u8))))
}.slice();

public static void TestParseExpr(ж<testing.T> Ꮡt) {
    foreach (var (i, vᴛ1) in parseExprTests) {
        ref var tt = ref heap(new parseExprTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            var (x, err) = parseExpr(ttʗ1.@in);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            if (x.String() != ttʗ1.x.String()) {
                tΔ1.Errorf("parseExpr(%q):\nhave %s\nwant %s"u8, ttʗ1.@in, x, ttʗ1.x);
            }
        });
    }
}


partial struct parseExprErrorTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal error err;
}
internal static slice<parseExprErrorTestsᴛ1> parseExprErrorTests = new parseExprErrorTestsᴛ1[]{
    new("x && "u8, new global::go.go.build.constraint_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(Offset: 5, Err: "unexpected end of expression"u8)))),
    new("x && ("u8, new global::go.go.build.constraint_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(Offset: 6, Err: "missing close paren"u8)))),
    new("x && ||"u8, new global::go.go.build.constraint_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(Offset: 5, Err: "unexpected token ||"u8)))),
    new("x && !"u8, new global::go.go.build.constraint_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(Offset: 6, Err: "unexpected end of expression"u8)))),
    new("x && !!"u8, new global::go.go.build.constraint_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(Offset: 6, Err: "double negation not allowed"u8)))),
    new("x !"u8, new global::go.go.build.constraint_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(Offset: 2, Err: "unexpected token !"u8)))),
    new("x && (y"u8, new global::go.go.build.constraint_package.SyntaxErrorжerror(Ꮡ(new SyntaxError(Offset: 5, Err: "missing close paren"u8))))
}.slice();

public static void TestParseError(ж<testing.T> Ꮡt) {
    foreach (var (i, vᴛ1) in parseExprErrorTests) {
        ref var tt = ref heap(new parseExprErrorTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            var (x, err) = parseExpr(ttʗ1.@in);
            if (err == default!) {
                tΔ1.Fatalf("parseExpr(%q) = %v, want error"u8, ttʗ1.@in, x);
            }
            if (!reflect.DeepEqual(err, ttʗ1.err)) {
                tΔ1.Fatalf("parseExpr(%q): wrong error:\nhave %#v\nwant %#v"u8, ttʗ1.@in, err, ttʗ1.err);
            }
        });
    }
}


partial struct exprEvalTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal bool ok;
    internal @string tags;
}
internal static slice<exprEvalTestsᴛ1> exprEvalTests = new exprEvalTestsᴛ1[]{
    new("x"u8, false, "x"u8),
    new("x && y"u8, false, "x y"u8),
    new("x || y"u8, false, "x y"u8),
    new("!x && yes"u8, true, "x yes"u8),
    new("yes || y"u8, true, "y yes"u8)
}.slice();

public static void TestExprEval(ж<testing.T> Ꮡt) {
    foreach (var (i, vᴛ1) in exprEvalTests) {
        ref var tt = ref heap(new exprEvalTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            var (x, err) = parseExpr(ttʗ1.@in);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            var tags = new map<@string, bool>();
            var wantTags = new map<@string, bool>();
            foreach (var (_, tag) in strings.Fields(ttʗ1.tags)) {
                wantTags[tag] = true;
            }
            var tagsʗ1 = tags;
            var hasTag = (@string tag) => {
                tagsʗ1[tag] = true;
                return tag == "yes"u8;
            };
            var ok = x.Eval(hasTag);
            if (ok != ttʗ1.ok || !maps.Equal<map<@string, bool>, map<@string, bool>, @string, bool>(tags, wantTags)) {
                tΔ1.Errorf("Eval(%#q):\nhave ok=%v, tags=%v\nwant ok=%v, tags=%v"u8,
                    ttʗ1.@in, ok, tags, ttʗ1.ok, wantTags);
            }
        });
    }
}

internal static slice<parseExprTestsᴛ1> parsePlusBuildExprTests = new parseExprTestsᴛ1[]{
    new("x"u8, tag("x"u8)),
    new("x,y"u8, and(tag("x"u8), tag("y"u8))),
    new("x y"u8, or(tag("x"u8), tag("y"u8))),
    new("x y,z"u8, or(tag("x"u8), and(tag("y"u8), tag("z"u8)))),
    new("x,y z"u8, or(and(tag("x"u8), tag("y"u8)), tag("z"u8))),
    new("x,!y !z"u8, or(and(tag("x"u8), not(tag("y"u8))), not(tag("z"u8)))),
    new("!! x"u8, or(tag("ignore"u8), tag("x"u8))),
    new("!!x"u8, tag("ignore"u8)),
    new("!x"u8, not(tag("x"u8))),
    new("!"u8, tag("ignore"u8)),
    new(""u8, tag("ignore"u8))
}.slice();

public static void TestParsePlusBuildExpr(ж<testing.T> Ꮡt) {
    foreach (var (i, vᴛ1) in parsePlusBuildExprTests) {
        ref var tt = ref heap(new parseExprTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            var (x, _) = parsePlusBuildExpr(ttʗ1.@in);
            if (x.String() != ttʗ1.x.String()) {
                tΔ1.Errorf("parsePlusBuildExpr(%q):\nhave %v\nwant %v"u8, ttʗ1.@in, x, ttʗ1.x);
            }
        });
    }
}


partial struct constraintTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal global::go.go.build.constraint_package.Expr x;
    internal @string err;
}
internal static slice<constraintTestsᴛ1> constraintTests = new constraintTestsᴛ1[]{
    new("//+build !"u8, tag("ignore"u8), ""u8),
    new("//+build"u8, tag("ignore"u8), ""u8),
    new("//+build x y"u8, or(tag("x"u8), tag("y"u8)), ""u8),
    new("// +build x y \n"u8, or(tag("x"u8), tag("y"u8)), ""u8),
    new("// +build x y \n "u8, default!, "not a build constraint"u8),
    new("// +build x y \nmore"u8, default!, "not a build constraint"u8),
    new(" //+build x y"u8, default!, "not a build constraint"u8),
    new("//go:build x && y"u8, and(tag("x"u8), tag("y"u8)), ""u8),
    new("//go:build x && y\n"u8, and(tag("x"u8), tag("y"u8)), ""u8),
    new("//go:build x && y\n "u8, default!, "not a build constraint"u8),
    new("//go:build x && y\nmore"u8, default!, "not a build constraint"u8),
    new(" //go:build x && y"u8, default!, "not a build constraint"u8),
    new("//go:build\n"u8, default!, "unexpected end of expression"u8)
}.slice();

public static void TestParse(ж<testing.T> Ꮡt) {
    foreach (var (i, vᴛ1) in constraintTests) {
        ref var tt = ref heap(new constraintTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            var (x, err) = Parse(ttʗ1.@in);
            if (err != default!) {
                if (ttʗ1.err == ""u8){
                    tΔ1.Errorf("Constraint(%q): unexpected error: %v"u8, ttʗ1.@in, err);
                } else 
                if (!strings.Contains(err.Error(), ttʗ1.err)) {
                    tΔ1.Errorf("Constraint(%q): error %v, want %v"u8, ttʗ1.@in, err, ttʗ1.err);
                }
                return;
            }
            if (ttʗ1.err != ""u8) {
                tΔ1.Errorf("Constraint(%q) = %v, want error %v"u8, ttʗ1.@in, x, ttʗ1.err);
                return;
            }
            if (x.String() != ttʗ1.x.String()) {
                tΔ1.Errorf("Constraint(%q):\nhave %v\nwant %v"u8, ttʗ1.@in, x, ttʗ1.x);
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
internal static void initᴛplusBuildLinesTests() { plusBuildLinesTests = new plusBuildLinesTestsᴛ1[]{
    new("x"u8, new @string[]{"x"u8}.slice(), default!),
    new("x && !y"u8, new @string[]{"x,!y"u8}.slice(), default!),
    new("x || y"u8, new @string[]{"x y"u8}.slice(), default!),
    new("x && (y || z)"u8, new @string[]{"x"u8, "y z"u8}.slice(), default!),
    new("!(x && y)"u8, new @string[]{"!x !y"u8}.slice(), default!),
    new("x || (y && z)"u8, new @string[]{"x y,z"u8}.slice(), default!),
    new("w && (x || (y && z))"u8, new @string[]{"w"u8, "x y,z"u8}.slice(), default!),
    new("v || (w && (x || (y && z)))"u8, default!, errComplex)
}.slice(); }

public static void TestPlusBuildLines(ж<testing.T> Ꮡt) {
    foreach (var (i, vᴛ1) in plusBuildLinesTests) {
        ref var tt = ref heap(new plusBuildLinesTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(fmt.Sprint(i), (ж<testing.T> tΔ1) => {
            var (x, err) = parseExpr(ttʗ1.@in);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            (var lines, err) = PlusBuildLines(x);
            if (err != default!) {
                if (ttʗ1.err == default!){
                    tΔ1.Errorf("PlusBuildLines(%q): unexpected error: %v"u8, ttʗ1.@in, err);
                } else 
                if (!AreEqual(ttʗ1.err, err)) {
                    tΔ1.Errorf("PlusBuildLines(%q): error %v, want %v"u8, ttʗ1.@in, err, ttʗ1.err);
                }
                return;
            }
            if (ttʗ1.err != default!) {
                tΔ1.Errorf("PlusBuildLines(%q) = %v, want error %v"u8, ttʗ1.@in, lines, ttʗ1.err);
                return;
            }
            slice<@string> want = default!;
            foreach (var (_, line) in ttʗ1.@out) {
                want = append(want, "// +build "u8 + line);
            }
            if (!slices.Equal<slice<@string>, @string>(lines, want)) {
                tΔ1.Errorf("PlusBuildLines(%q):\nhave %q\nwant %q"u8, ttʗ1.@in, lines, want);
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

public static void TestSizeLimits(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestSizeLimits_type[]{
        new(
            name: "go:build or limit"u8,
            expr: "//go:build "u8 + strings.Repeat("a || "u8, maxSize + 2)
        ),
        new(
            name: "go:build and limit"u8,
            expr: "//go:build "u8 + strings.Repeat("a && "u8, maxSize + 2)
        ),
        new(
            name: "go:build and depth limit"u8,
            expr: "//go:build "u8 + strings.Repeat("(a &&"u8, maxSize + 2)
        ),
        new(
            name: "go:build or depth limit"u8,
            expr: "//go:build "u8 + strings.Repeat("(a ||"u8, maxSize + 2)
        )
    }.slice()) {
        ref var tc = ref heap(new TestSizeLimits_type(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var (_, err) = Parse(tcʗ1.expr);
            if (err == default!){
                tΔ1.Error(expressionDidNotTriggerˢ);
            } else 
            {
                var (syntaxErr, ok) = err._<ж<global::go.go.build.constraint_package.SyntaxError>>(ᐧ); if (!ok || (~syntaxErr).Err != "build expression too large"u8) {
                    if (!ok){
                        tΔ1.Errorf("unexpected error: %v"u8, err);
                    } else {
                        tΔ1.Errorf("unexpected syntax error: %s"u8, (~syntaxErr).Err);
                    }
                }
            }
        });
    }
}

internal partial struct TestPlusSizeLimits_type /*dyn*/ {
    internal @string name;
    internal @string expr;
}

public static void TestPlusSizeLimits(ж<testing.T> Ꮡt) {
    nint maxOldSize = 100;
    foreach (var (_, vᴛ1) in new TestPlusSizeLimits_type[]{
        new(
            name: "+build or limit"u8,
            expr: "// +build "u8 + strings.Repeat("a "u8, maxOldSize + 2)
        ),
        new(
            name: "+build and limit"u8,
            expr: "// +build "u8 + strings.Repeat("a,"u8, maxOldSize + 2)
        )
    }.slice()) {
        ref var tc = ref heap(new TestPlusSizeLimits_type(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        Ꮡt.Run(tc.name, (ж<testing.T> tΔ1) => {
            var (_, err) = Parse(tcʗ1.expr);
            if (err == default!){
                tΔ1.Error(expressionDidNotTriggerˢ);
            } else 
            if (!AreEqual(err, errComplex)) {
                tΔ1.Errorf("unexpected error: got %q, want %q"u8, err, errComplex);
            }
        });
    }
}

} // end constraint_internal_test_package
