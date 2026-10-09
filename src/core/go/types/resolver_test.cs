// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using testenv = global::go.@internal.testenv_package;
using slices = slices_package;
using testing = testing_package;
using static global::go.go.types_package;
using global::go.@internal;
using global::go.go;
using static global::go.go.types_internal_test_package;
using types = global::go.go.types_package;

partial class types_test_package {

partial struct resolveTestImporter {
    internal ж<token.FileSet> fset;
    internal types.ImporterFrom importer;
    internal map<@string, bool> imported;
}

internal static (ж<types.Package>, error) Import(this ref resolveTestImporter imp, @string _) {
    throw panic("should not be called");
}

internal static (ж<types.Package>, error) ImportFrom(this ref resolveTestImporter imp, @string path, @string srcDir, types.ImportMode mode) {
    if (mode != 0) {
        throw panic("mode must be 0");
    }
    if (imp.importer == default!) {
        imp.importer = defaultImporter(fset)._<ImporterFrom>();
        imp.imported = new map<@string, bool>();
    }
    var (pkg, err) = imp.importer.ImportFrom(path, srcDir, mode);
    if (err != default!) {
        return (default!, err);
    }
    imp.imported[path] = true;
    return (pkg, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testResolveIdentsˢ = "testResolveIdents"u8;
internal static readonly @string mutexStringerErrorˢ = "[Mutex Stringer error]"u8;

public static void TestResolveIdents(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt));
    var sources = new @string[]{
        """

		package p
		import "fmt"
		import "math"
		const pi = math.Pi
		func sin(x float64) float64 {
			return math.Sin(x)
		}
		var Println = fmt.Println
		
"""u8,
        """

		package p
		import "fmt"
		type errorStringer struct { fmt.Stringer; error }
		func f() string {
			_ = "foo"
			return fmt.Sprintf("%d", g())
		}
		func g() (x int) { return }
		
"""u8,
        """

		package p
		import . "go/parser"
		import "sync"
		func h() Mode { return ImportsOnly }
		var _, x int = 1, 2
		func init() {}
		type T struct{ *sync.Mutex; a, b, c int}
		type I interface{ m() }
		var _ = T{a: 1, b: 2, c: 3}
		func (_ T) m() {}
		func (T) _() {}
		var i I
		var _ = i.m
		func _(s []int) { for i, x := range s { _, _ = i, x } }
		func _(x interface{}) {
			switch x := x.(type) {
			case int:
				_ = x
			}
			switch {} // implicit 'true' tag
		}
		
"""u8,
        """

		package p
		type S struct{}
		func (T) _() {}
		func (T) _() {}
		
"""u8,
        """

		package p
		func _() {
		L0:
		L1:
			goto L0
			for {
				goto L1
			}
			if true {
				goto L2
			}
		L2:
		}
		
"""u8
    }.slice();
    var pkgnames = new @string[]{
        "fmt"u8,
        "math"u8
    }.slice();
    // parse package files
    var fset = token.NewFileSet();
    slice<ж<ast.File>> files = default!;
    foreach (var (_, src) in sources) {
        files = append(files, mustParse(fset, src));
    }
    // resolve and type-check package AST
    var importer = Ꮡ(new resolveTestImporter(fset: fset));
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(Importer: new types_test_package.resolveTestImporterжImporter(importer));
    var uses = new map<ж<ast.Ident>, types.Object>();
    var defs = new map<ж<ast.Ident>, types.Object>();
    var (_, err) = Ꮡconf.Check(testResolveIdentsˢ, fset, files, Ꮡ(new typesꓸInfo(Defs: defs, Uses: uses)));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    // check that all packages were imported
    foreach (var (_, name) in pkgnames) {
        if (!(~importer).imported[name]) {
            Ꮡt.Errorf("package %s not imported"u8, name);
        }
    }
    // check that qualified identifiers are resolved
    foreach (var (_, f) in files) {
        var fsetʗ1 = fset;
        var usesʗ1 = uses;
        ast.Inspect(new ast.FileжNode(f), (ast.Node n) => {
            {
                var (s, ok) = n._<ж<ast.SelectorExpr>>(ᐧ); if (ok) {
                    {
                        var (x, okΔ1) = (~s).X._<ж<ast.Ident>>(ᐧ); if (okΔ1) {
                            var obj = usesʗ1[x];
                            if (obj == default!) {
                                Ꮡt.Errorf("%s: unresolved qualified identifier %s"u8, fsetʗ1.Position(x.Pos()), (~x).Name);
                                return false;
                            }
                            {
                                var (_, okΔ2) = obj._<ж<types.PkgName>>(ᐧ); if (okΔ2 && usesʗ1[(~s).Sel] == default!) {
                                    Ꮡt.Errorf("%s: unresolved selector %s"u8, fsetʗ1.Position((~s).Sel.Pos()), (~(~s).Sel).Name);
                                    return false;
                                }
                            }
                            return false;
                        }
                    }
                    return true;
                }
            }
            return true;
        });
    }
    foreach (var (id, obj) in uses) {
        if (obj == default!) {
            Ꮡt.Errorf("%s: Uses[%s] == nil"u8, fset.Position(id.Pos()), (~id).Name);
        }
    }
    // check that each identifier in the source is found in uses or defs or both
    ref var both = ref heap<slice<@string>>(out var Ꮡboth);
    foreach (var (_, f) in files) {
        var defsʗ1 = defs;
        var fsetʗ2 = fset;
        var usesʗ2 = uses;
        ast.Inspect(new ast.FileжNode(f), (ast.Node n) => {
            {
                var (x, ok) = n._<ж<ast.Ident>>(ᐧ); if (ok) {
                    nint objects = default!;
                    {
                        var (_, found) = usesʗ2[x, ꟷ]; if (found) {
                            objects |= (nint)(1);
                            delete(usesʗ2, x);
                        }
                    }
                    {
                        var (_, found) = defsʗ1[x, ꟷ]; if (found) {
                            objects |= (nint)(2);
                            delete(defsʗ1, x);
                        }
                    }
                    if (objects == 0){
                        Ꮡt.Errorf("%s: unresolved identifier %s"u8, fsetʗ2.Position(x.Pos()), (~x).Name);
                    } else 
                    if (objects == 3) {
                        Ꮡboth.ValueSlot = append(Ꮡboth.ValueSlot, (~x).Name);
                    }
                    return false;
                }
            }
            return true;
        });
    }
    // check the expected set of idents that are simultaneously uses and defs
    slices.Sort<slice<@string>, @string>(both);
    {
        @string got = fmt.Sprint(both);
        @string want = mutexStringerErrorˢ; if (got != want) {
            Ꮡt.Errorf("simultaneous uses/defs = %s, want %s"u8, got, want);
        }
    }
    // any left-over identifiers didn't exist in the source
    foreach (var (x, _) in uses) {
        Ꮡt.Errorf("%s: identifier %s not present in source"u8, fset.Position(x.Pos()), (~x).Name);
    }
    foreach (var (x, _) in defs) {
        Ꮡt.Errorf("%s: identifier %s not present in source"u8, fset.Position(x.Pos()), (~x).Name);
    }
}

// TODO(gri) add tests to check ImplicitObj callbacks

} // end types_test_package
