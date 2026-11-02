// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains tests for Eval.
namespace go.go;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using types = global::go.go.types_package;
using godebug = global::go.@internal.godebug_package;
using testenv = global::go.@internal.testenv_package;
using strings = strings_package;
using testing = testing_package;
using static global::go.go.types_package;
using global::go.@internal;
using global::go.go;
using static global::go.go.types_internal_test_package;

partial class types_test_package {

internal static void testEval(ж<testing.T> t, ж<token.FileSet> fset, ж<types.Package> pkg, tokenꓸPos pos, @string expr, typesꓸType typ, @string typStr, @string valStr) {
    var (gotTv, err) = Eval(fset, pkg, pos, expr);
    if (err != nil) {
        t.Errorf("Eval(%q) failed: %s", expr, err);
        return;
    }
    if (gotTv.Type == nil) {
        t.Errorf("Eval(%q) got nil type but no error", expr);
        return;
    }

    // compare types
    if (typ != nil) {
        // we have a type, check identity
        if (!Identical(gotTv.Type, typ)) {
            t.Errorf("Eval(%q) got type %s, want %s", expr, gotTv.Type, typ);
            return;
        }
    } else {
        // we have a string, compare type string
        @string gotStrΔ1 = gotTv.Type.String();
        if (gotStrΔ1 != typStr) {
            t.Errorf("Eval(%q) got type %s, want %s", expr, gotStrΔ1, typStr);
            return;
        }
    }

    // compare values
    @string gotStr = ""u8;
    if (gotTv.Value != nil) {
        gotStr = gotTv.Value.ExactString();
    }
    if (gotStr != valStr) {
        t.Errorf("Eval(%q) got value %s, want %s", expr, gotStr, valStr);
    }
}

public static void TestEvalBasic(ж<testing.T> t) {
    var fset = token.NewFileSet();
    foreach (var (_, typ) in Typ[(int)(Bool)..(int)(ΔString + 1)]) {
        testEval(t, fset, nil, nopos, typ.Name(), new types.BasicжΔType(typ), ""u8, ""u8);
    }
}

public static void TestEvalComposite(ж<testing.T> t) {
    var fset = token.NewFileSet();
    foreach (var (_, test) in independentTestTypes) {
        testEval(t, fset, nil, nopos, test.src, default, test.str, ""u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string trueˢ => ref trueˢᵇ;
private static readonly @string trueˢᵇ = "true"u8;

public static void TestEvalArith(ж<testing.T> t) {
    slice<@string> tests = slice<@string>([
        @"true",
        @"false == false",
        @"12345678 + 87654321 == 99999999",
        @"10 * 20 == 200",
        @"(1<<500)*2 >> 100 == 2<<400",
        @"""foo"" + ""bar"" == ""foobar""",
        @"""abc"" <= ""bcd""",
        @"len([10]struct{}{}) == 2*5"
    ]);
    var fset = token.NewFileSet();
    foreach (var (_, test) in tests) {
        testEval(t, fset, nil, nopos, test, new types.BasicжΔType(Typ[UntypedBool]), ""u8, trueˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string interfaceRReadˢ => ref interfaceRReadˢᵇ;
private static readonly @string interfaceRReadˢᵇ = "interface{R}.Read"u8;

public static void TestEvalPos(ж<testing.T> t) {
    var filesᴷ = new stackAppend<ж<ast.File>>();

    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(t));

    // The contents of /*-style comments are of the form
    //	expr => value, type
    // where value may be the empty string.
    // Each expr is evaluated at the position of the comment
    // and the result is compared with the expected value
    // and type.
    slice<@string> sources = slice<@string>([
        """

		package p
		import "fmt"
		import m "math"
		const c = 3.0
		type T []int
		func f(a int, s string) float64 {
			fmt.Println("calling f")
			_ = m.Pi // use package math
			const d int = c + 1
			var x int
			x = a + len(s)
			return float64(x)
			/* true => true, untyped bool */
			/* fmt.Println => , func(a ...any) (n int, err error) */
			/* c => 3, untyped float */
			/* T => , p.T */
			/* a => , int */
			/* s => , string */
			/* d => 4, int */
			/* x => , int */
			/* d/c => 1, int */
			/* c/2 => 3/2, untyped float */
			/* m.Pi < m.E => false, untyped bool */
		}
		
"""u8.Raw(),
        """

		package p
		/* c => 3, untyped float */
		type T1 /* T1 => , p.T1 */ struct {}
		var v1 /* v1 => , int */ = 42
		func /* f1 => , func(v1 float64) */ f1(v1 float64) {
			/* f1 => , func(v1 float64) */
			/* v1 => , float64 */
			var c /* c => 3, untyped float */ = "foo" /* c => , string */
			{
				var c struct {
					c /* c => , string */ int
				}
				/* c => , struct{c int} */
				_ = c
			}
			_ = func(a, b, c int /* c => , string */) /* c => , int */ {
				/* c => , int */
			}
			_ = c
			type FT /* FT => , p.FT */ interface{}
		}
		
"""u8.Raw(),
        """

		package p
		/* T => , p.T */
		
"""u8.Raw(),
        """

		package p
		import "io"
		type R = io.Reader
		func _() {
			/* interface{R}.Read => , func(_ interface{io.Reader}, p []byte) (n int, err error) */
			_ = func() {
				/* interface{io.Writer}.Write => , func(_ interface{io.Writer}, p []byte) (n int, err error) */
				type io interface {} // must not shadow io in line above
			}
			type R interface {} // must not shadow R in first line of this function body
		}
		
"""u8.Raw()
    ]);

    var fset = token.NewFileSet();
    slice<ж<ast.File>> files = default;
    foreach (var (i, src) in sources) {
        var (@file, errΔ1) = parser.ParseFile(fset, "p", src, parser.ParseComments);
        if (errΔ1 != nil) {
            t.Fatalf("could not parse file %d: %s", i, errΔ1);
        }

        // Materialized aliases give a different (better)
        // result for the final test, so skip it for now.
        // TODO(adonovan): reenable when gotypesalias=1 is the default.
        var exprᴛ1 = gotypesalias.Value();
        switch (ᐧ) {
        case {} when exprᴛ1 == ""u8 || exprᴛ1 == "1"u8: {
            if (strings.Contains(src, interfaceRReadˢ)) {
                continue;
            }
            break;
        }}

        files = append(filesᴷ, files, @file);
    }

    ref var conf = ref heap<types.Config>(new Config(Importer: defaultImporter(fset)), out var Ꮡconf);
    var (pkg, err) = Ꮡconf.Check("p", fset, files, nil);
    if (err != nil) {
        t.Fatal(err);
    }

    foreach (var (_, @file) in files) {
        foreach (var (_, group) in @file.Comments) {
            foreach (var (_, comment) in group.List) {
                @string s = comment.Text;
                if (len(s) >= 4 && s[..2] == "/*" && s.slice(len(s) - 2) == "*/") {
                    var (str, typ) = split(s.slice(2, len(s) - 2), ", ");
                    (str, var val) = split(str, "=>");
                    testEval(t, fset, pkg, comment.Pos(), str, default, typ, val);
                }
            }
        }
    }
}

// gotypesalias controls the use of Alias types.
internal static ж<godebug.Setting> gotypesalias = godebug.New("gotypesalias");

// split splits string s at the first occurrence of s, trimming spaces.
internal static (@string, @string) split(@string s, @string sep) {
    var (before, after, _) = strings.Cut(s, sep);
    return (strings.TrimSpace(before), strings.TrimSpace(after));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string evalˢ => ref evalˢᵇ;
private static readonly @string evalˢᵇ = "eval"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ16 => ref srcᶜ16ᵇ;
private static readonly @string srcᶜ16ᵇ = """

package p

import "fmt"

const c = 3.0
type T []int
type S struct{ X int }

func f(a int, s string) S {
	/* fmt.Println => func fmt.Println(a ...any) (n int, err error) */
	/* fmt.Stringer.String => func (fmt.Stringer).String() string */
	fmt.Println("calling f")

	var fmt struct{ Println int }
	/* fmt => var fmt struct{Println int} */
	/* fmt.Println => field Println int */
	/* f(1, "").X => field X int */
	fmt.Println = 1

	/* append => builtin append */

	/* new(S).X => field X int */

	return S{}
}
"""u8.Raw();

public static void TestCheckExpr(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(t));

    // Each comment has the form /* expr => object */:
    // expr is an identifier or selector expression that is passed
    // to CheckExpr at the position of the comment, and object is
    // the string form of the object it denotes.
    @string src = srcᶜ16;

    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, "p", src, parser.ParseComments);
    if (err != nil) {
        t.Fatal(err);
    }

    ref var conf = ref heap<types.Config>(new Config(Importer: defaultImporter(fset)), out var Ꮡconf);
    (var pkg, err) = Ꮡconf.Check("p", fset, slice<ж<ast.File>>([f]), nil);
    if (err != nil) {
        t.Fatal(err);
    }

    var fsetʗ1 = fset;
    var pkgʗ1 = pkg;
    (types.Object, error) checkExpr(tokenꓸPos pos, @string str) {
        var (expr, errΔ1) = parser.ParseExprFrom(fsetʗ1, evalˢ, str, 0);
        if (errΔ1 != nil) {
            return (default, errΔ1);
        }

        var info = Ꮡ(new typesꓸInfo(
            Uses: new map<ж<ast.Ident>, types.Object>(),
            Selections: new map<ж<ast.SelectorExpr>, ж<types.Selection>>()
        ));
        {
            var errΔ2 = CheckExpr(fsetʗ1, pkgʗ1, pos, expr, info); if (errΔ2 != nil) {
                return (default, fmt.Errorf("CheckExpr(%q) failed: %s"u8, str, errΔ2));
            }
        }
        switch (expr.type()) {
        case ж<ast.Ident> exprΔ1: {
            {
                var (obj, ok) = info.Uses[exprΔ1, ꟷ]; if (ok) {
                    return (obj, default);
                }
            }
            break;
        }
        case ж<ast.SelectorExpr> exprΔ1: {
            {
                var (sel, ok) = info.Selections[exprΔ1, ꟷ]; if (ok) {
                    return (sel.Obj(), default);
                }
            }
            {
                var (obj, ok) = info.Uses[exprΔ1.Sel, ꟷ]; if (ok) {
                    return (obj, default); // qualified identifier
                }
            }
            break;
        }}
        return (default, fmt.Errorf("no object for %s"u8, str));
    }

    foreach (var (_, group) in f.Comments) {
        foreach (var (_, comment) in group.List) {
            @string s = comment.Text;
            if (len(s) >= 4 && strings.HasPrefix(s, "/*") && strings.HasSuffix(s, "*/")) {
                tokenꓸPos pos = comment.Pos();
                var (expr, wantObj) = split(s.slice(2, len(s) - 2), "=>");
                var (obj, errΔ3) = checkExpr(pos, expr);
                if (errΔ3 != nil) {
                    t.Errorf("%s: %s", fset.Position(pos), errΔ3);
                    continue;
                }
                if (obj.String() != wantObj) {
                    t.Errorf("%s: checkExpr(%s) = %s, want %v",
                        fset.Position(pos), expr, obj, wantObj);
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string undefinedˢ => ref undefinedˢᵇ;
private static readonly @string undefinedˢᵇ = "undefined"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ17 => ref srcᶜ17ᵇ;
private static readonly @string srcᶜ17ᵇ = """

package p
func _[A any](A) {}

"""u8.Raw();

public static void TestIssue65898(ж<testing.T> t) {
    @string src = srcᶜ17;

    var fset = token.NewFileSet();
    var f = mustParse(fset, src);

    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    var (pkg, err) = Ꮡconf.Check(pkgName(src), fset, slice<ж<ast.File>>([f]), nil);
    if (err != nil) {
        t.Fatal(err);
    }

    foreach (var (_, d) in f.Decls) {
        {
            var (fun, _) = d._<ж<ast.FuncDecl>>(ᐧ); if (fun != nil) {
                // type parameter A is not found at the start of the function type
                {
                    var errΔ1 = types.CheckExpr(fset, pkg, fun.Type.Pos(), new ast.FuncTypeжExpr(fun.Type), nil); if (errΔ1 == nil || !strings.Contains(errΔ1.Error(), undefinedˢ)) {
                        t.Fatalf("got %s, want undefined error", errΔ1);
                    }
                }
                // type parameter A must be found at the end of the function type
                {
                    var errΔ2 = types.CheckExpr(fset, pkg, fun.Type.End(), new ast.FuncTypeжExpr(fun.Type), nil); if (errΔ2 != nil) {
                        t.Fatal(errΔ2);
                    }
                }
            }
        }
    }
}

} // end types_test_package
