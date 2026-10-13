// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using reflect = reflect_package;
using regexp = regexp_package;
using strings = strings_package;
using testing = testing_package;
using static global::go.go.types_package;
using global::go.go;
using static global::go.go.types_internal_test_package;
using types = global::go.go.types_package;
using ꓸꓸꓸжastꓸFile = Span<ж<global::go.go.ast_package.File>>;

partial class types_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packageLibVarXIntˢ = "package lib; var X int"u8;
internal static readonly @string libPkgname5XVar1PiConst8ˢ = """

/*lib=pkgname:5*/ /*X=var:1*/ /*Pi=const:8*/ /*T=typename:9*/ /*Y=var:10*/ /*F=func:12*/
package main

import "lib"
import . "lib"

const Pi = 3.1415
type T struct{}
var Y, _ = lib.X, X

func F[T *U, U any](param1, param2 int) /*param1=undef*/ (res1 /*res1=undef*/, res2 int) /*param1=var:12*/ /*res1=var:12*/ /*U=typename:12*/ {
	const pi, e = 3.1415, /*pi=undef*/ 2.71828 /*pi=const:13*/ /*e=const:13*/
	type /*t=undef*/ t /*t=typename:14*/ *t
	print(Y) /*Y=var:10*/
	x, Y := Y, /*x=undef*/ /*Y=var:10*/ Pi /*x=var:16*/ /*Y=var:16*/ ; _ = x; _ = Y
	var F = /*F=func:12*/ F[*int, int] /*F=var:17*/ ; _ = F

	var a []int
	for i, x := range a /*i=undef*/ /*x=var:16*/ { _ = i; _ = x }

	var i interface{}
	switch y := i.(type) { /*y=undef*/
	case /*y=undef*/ int /*y=undef*/ : /*y=var:23*/ ;
	case float32, /*y=undef*/ float64 /*y=undef*/ : /*y=var:23*/ ;
	default /*y=undef*/ : /*y=var:23*/
		println(y)
	}
	/*y=undef*/

        switch int := i.(type) {
        case /*int=typename:0*/ int /*int=typename:0*/ : /*int=var:31*/
        	println(int)
        default /*int=typename:0*/ : /*int=var:31*/ ;
        }

	_ = param1
	_ = res1
	return
}
/*main=undef*/

"""u8.Raw();
internal static readonly @string undefˢ = "undef"u8;
internal static readonly @string typesˢ2 = "*types."u8;

// TestScopeLookupParent ensures that (*Scope).LookupParent returns
// the correct result at various positions with the source.
public static void TestScopeLookupParent(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var fset = token.NewFileSet();
    var imports = new testImporter(0);
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(Importer: imports);
    ref var info = ref heap(new typesꓸInfo(), out var Ꮡinfo);
    var fsetʗ1 = fset;
    var importsʗ1 = imports;
    void makePkg(@string path, params ꓸꓸꓸжastꓸFile filesʗp) {
        var files = filesʗp.slice();
        error err = default!;
        (importsʗ1[path], err) = Ꮡconf.Check(path, fsetʗ1, files, Ꮡinfo);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }

    makePkg(libˢ, mustParse(fset, packageLibVarXIntˢ));
    // Each /*name=kind:line*/ comment makes the test look up the
    // name at that point and checks that it resolves to a decl of
    // the specified kind and line number.  "undef" means undefined.
    // Note that type switch case clauses with an empty body (but for
    // comments) need the ";" to ensure that the recorded scope extends
    // past the comments.
    @string mainSrc = libPkgname5XVar1PiConst8ˢ;

    info.Uses = new map<ж<ast.Ident>, types.Object>();
    var f = mustParse(fset, mainSrc);
    makePkg(mainˢ, f);
    var mainScope = imports[mainˢ].Scope();
    var rx = regexp.MustCompile(@"^/\*(\w*)=([\w:]*)\*/$"u8);
    foreach (var (_, group) in (~f).Comments) {
        foreach (var (_, comment) in (~group).List) {
            // Parse the assertion in the comment.
            var m = rx.FindStringSubmatch((~comment).Text);
            if (m == default!) {
                Ꮡt.Errorf("%s: bad comment: %s"u8,
                    fset.Position(comment.Pos()), (~comment).Text);
                continue;
            }
            @string name = m[1];
            @string want = m[2];

            // Look up the name in the innermost enclosing scope.
            var inner = mainScope.Innermost(comment.Pos());
            if (inner == nil) {
                Ꮡt.Errorf("%s: at %s: can't find innermost scope"u8,
                    fset.Position(comment.Pos()), (~comment).Text);
                continue;
            }
            @string got = undefˢ;
            {
                var (_, obj) = inner.LookupParent(name, comment.Pos()); if (obj != default!) {
                    @string kind = strings.ToLower(strings.TrimPrefix(reflect.TypeOf(obj).String(), typesˢ2));
                    got = fmt.Sprintf("%s:%d"u8, kind, fset.Position(obj.Pos()).Line);
                }
            }
            if (got != want) {
                Ꮡt.Errorf("%s: at %s: %s resolved to %s, want %s"u8,
                    fset.Position(comment.Pos()), (~comment).Text, name, got, want);
            }
        }
    }

    // Check that for each referring identifier,
    // a lookup of its name on the innermost
    // enclosing scope returns the correct object.

    foreach (var (id, wantObj) in info.Uses) {
        var inner = mainScope.Innermost(id.Pos());
        if (inner == nil) {
            Ꮡt.Errorf("%s: can't find innermost scope enclosing %q"u8,
                fset.Position(id.Pos()), (~id).Name);
            continue;
        }

        // Exclude selectors and qualified identifiers---lexical
        // refs only.  (Ideally, we'd see if the AST parent is a
        // SelectorExpr, but that requires PathEnclosingInterval
        // from golang.org/x/tools/go/ast/astutil.)
        if ((~id).Name == "X"u8) {
            continue;
        }

        var (_, gotObj) = inner.LookupParent((~id).Name, id.Pos());
        if (!AreEqual(gotObj, wantObj)) {
            // Print the scope tree of mainScope in case of error.
            ref var printScopeTree = ref heap<Action<@string, ж<typesꓸScope>>>(out var ᏑprintScopeTree);
            printScopeTree = (@string indent, ж<typesꓸScope> s) => {
                Ꮡt.Logf("%sscope %s %v-%v = %v"u8,
                    indent,
                    types_internal_test_package.ScopeComment(s),
                    s.Pos(),
                    s.End(),
                    s.Names());
                foreach (var i in range(s.NumChildren())) {
                    ᏑprintScopeTree.ValueSlot(indent + "  "u8, s.Child(i));
                }
            };
            printScopeTree(""u8, mainScope);

            Ꮡt.Errorf("%s: Scope(%s).LookupParent(%s@%v) got %v, want %v [scopePos=%v]"u8,
                fset.Position(id.Pos()),
                types_internal_test_package.ScopeComment(inner),
                (~id).Name,
                id.Pos(),
                gotObj,
                wantObj,
                types_internal_test_package.ObjectScopePos(wantObj));
            continue;
        }
    }
}

} // end types_test_package
