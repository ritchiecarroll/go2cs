// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements tests for various issues.
namespace go.go;

using fmt = fmt_package;
using ast = global::go.go.ast_package;
using parser = global::go.go.parser_package;
using token = global::go.go.token_package;
using testenv = global::go.@internal.testenv_package;
using regexp = regexp_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using static global::go.go.types_package;
using global::go.@internal;
using global::go.go;
using io = io_package;
using static global::go.go.types_internal_test_package;
using types = global::go.go.types_package;

partial class types_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packagePTypeSStructTˢ => ref packagePTypeSStructTˢᵇ;
private static readonly @string packagePTypeSStructTˢᵇ = @"package p; type S struct{T}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ1 => ref wantᶜ1ᵇ;
private static readonly @string wantᶜ1ᵇ = "undefined: T"u8;

public static void TestIssue5770(ж<testing.T> t) {
    var (_, err) = typecheck(packagePTypeSStructTˢ, nil, nil);
    @string want = wantᶜ1;
    if (err == nil || !strings.Contains(err.Error(), want)) {
        t.Errorf("got: %v; want: %s", err, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packagePVarSUintUint88ˢ => ref packagePVarSUintUint88ˢᵇ;
private static readonly @string packagePVarSUintUint88ˢᵇ = """

package p
var (
	s uint
	_ = uint8(8)
	_ = uint16(16) << s
	_ = uint32(32 << s)
	_ = uint64(64 << s + s)
	_ = (interface{})("foo")
	_ = (interface{})(nil)
)
"""u8.Raw();

public static void TestIssue5849(ж<testing.T> t) {
    @string src = packagePVarSUintUint88ˢ;
    var types = new map<ast.Expr, types.TypeAndValue>();
    mustTypecheck(src, nil, Ꮡ(new typesꓸInfo(Types: types)));

    foreach (var (x, tv) in types) {
        typesꓸType want = default;
        switch (x.type()) {
        case ж<ast.BasicLit> xΔ1: {
            var exprᴛ1 = (~xΔ1).Value;
            switch (ᐧ) {
            case {} when exprᴛ1 == @"8"u8: {
                want = new types.BasicжΔType(Typ[Uint8]);
                break;
            }
            case {} when exprᴛ1 == @"16"u8: {
                want = new types.BasicжΔType(Typ[Uint16]);
                break;
            }
            case {} when exprᴛ1 == @"32"u8: {
                want = new types.BasicжΔType(Typ[Uint32]);
                break;
            }
            case {} when exprᴛ1 == @"64"u8: {
                want = new types.BasicжΔType(Typ[Uint]); // because of "+ s", s is of type uint
                break;
            }
            case {} when exprᴛ1 == @"""foo"""u8: {
                want = new types.BasicжΔType(Typ[ΔString]);
                break;
            }}
            break;
        }
        case ж<ast.Ident> xΔ1: {
            if (xΔ1.Name == "nil"u8) {
                want = new types.BasicжΔType(Typ[UntypedNil]);
            }
            break;
        }}
        if (want != nil && !Identical(tv.Type, want)) {
            t.Errorf("got %s; want %s", tv.Type, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packagePFuncFIntDeferFGoˢ => ref packagePFuncFIntDeferFGoˢᵇ;
private static readonly @string packagePFuncFIntDeferFGoˢᵇ = """

package p
func f() int {
	defer f()
	go f()
	return 0
}

"""u8.Raw();

public static void TestIssue6413(ж<testing.T> t) {
    @string src = packagePFuncFIntDeferFGoˢ;
    var types = new map<ast.Expr, types.TypeAndValue>();
    mustTypecheck(src, nil, Ꮡ(new typesꓸInfo(Types: types)));

    var want = Typ[Int];
    nint n = 0;
    foreach (var (x, tv) in types) {
        var (_, ok) = x._<ж<ast.CallExpr>>(ᐧ); if (ok) {
            if (!AreEqual(tv.Type, want)) {
                t.Errorf("%s: got %s; want %s", fset.Position(x.Pos()), tv.Type, want.OrTypedNil());
            }
            n++;
        }
    }

    if (n != 2) {
        t.Errorf("got %d CallExprs; want 2", n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packagePFuncTMResBoolˢ => ref packagePFuncTMResBoolˢᵇ;
private static readonly @string packagePFuncTMResBoolˢᵇ = """

package p
func (T) m() (res bool) { return }
type T struct{} // receiver type after method declaration

"""u8.Raw();

public static void TestIssue7245(ж<testing.T> t) {
    @string src = packagePFuncTMResBoolˢ;
    var f = mustParse(fset, src);

    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    var defs = new map<ж<ast.Ident>, types.Object>();
    var (_, err) = Ꮡconf.Check(f.Name.Name, fset, [f], Ꮡ(new typesꓸInfo(Defs: defs)));
    if (err != nil) {
        t.Fatal(err);
    }

    var m = f.Decls[0]._<ж<ast.FuncDecl>>();
    var res1 = defs[m.Name]._<ж<types.Func>>().Signature().Results().At(0);
    var res2 = defs[m.Type.Results.List[0].Names[0]]._<ж<types.Var>>();

    if (res1 != res2) {
        t.Errorf("got %s (%p) != %s (%p)", res1.OrTypedNil(), res2.OrTypedNil(), res1.OrTypedNil(), res2.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cannotAssignToWˢ => ref cannotAssignToWˢᵇ;
private static readonly @string cannotAssignToWˢᵇ = "cannot assign to w"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ20 => ref srcᶜ20ᵇ;
private static readonly @string srcᶜ20ᵇ = """

package p
func _() {
	const w = 1        // defs w
        x, y := 2, 3       // defs x, y
        w, x, z := 4, 5, 6 // uses w, x, defs z; error: cannot assign to w
        _, _, _ = x, y, z  // uses x, y, z
}

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ2 => ref wantᶜ2ᵇ;
private static readonly @string wantᶜ2ᵇ = """
L3 defs func p._()
L4 defs const w untyped int
L5 defs var x int
L5 defs var y int
L6 defs var z int
L6 uses const w untyped int
L6 uses var x int
L7 uses var x int
L7 uses var y int
L7 uses var z int
"""u8.Raw();

// This tests that uses of existing vars on the LHS of an assignment
// are Uses, not Defs; and also that the (illegal) use of a non-var on
// the LHS of an assignment is a Use nonetheless.
public static void TestIssue7827(ж<testing.T> t) {
    var factsᴷ = new stackAppend<@string>();

    @string src = srcᶜ20;
    // We need a specific fileset in this test below for positions.
    // Cannot use typecheck helper.
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);

    @string want = wantᶜ2;

    // don't abort at the first error
    ref var conf = ref heap<types.Config>(new Config(Error: (error errΔ1) => {
        t.Log(errΔ1);
    }), out var Ꮡconf);
    var defs = new map<ж<ast.Ident>, types.Object>();
    var uses = new map<ж<ast.Ident>, types.Object>();
    var (_, err) = Ꮡconf.Check(f.Name.Name, fset, [f], Ꮡ(new typesꓸInfo(Defs: defs, Uses: uses)));
    @string s = err.Error(); if (!strings.HasSuffix(s, cannotAssignToWˢ)) {
        t.Errorf("Check: unexpected error: %s", s);
    }

    slice<@string> facts = default;
    foreach (var (id, obj) in defs) {
        if (obj != nil) {
            @string fact = fmt.Sprintf("L%d defs %s"u8, fset.Position(id.Pos()).Line, obj);
            facts = append(factsᴷ, facts, fact);
        }
    }
    foreach (var (id, obj) in uses) {
        @string fact = fmt.Sprintf("L%d uses %s"u8, fset.Position(id.Pos()).Line, obj);
        facts = append(facts, fact);
    }
    slices.Sort<slice<@string>, @string>(facts);

    @string got = strings.Join(facts, "\n");
    if (got != want) {
        t.Errorf("Unexpected defs/uses\ngot:\n%s\nwant:\n%s", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string src0ˢ => ref src0ˢᵇ;
private static readonly @string src0ˢᵇ = "src0"u8;
internal static ref readonly @string src1ˢ => ref src1ˢᵇ;
private static readonly @string src1ˢᵇ = "src1"u8;
internal static ref readonly @string src2ˢ => ref src2ˢᵇ;
private static readonly @string src2ˢᵇ = "src2"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string src0ᶜ => ref src0ᶜᵇ;
private static readonly @string src0ᶜᵇ = """

package main

import "go/types"

func main() {
	var info types.Info
	for _, obj := range info.Uses {
		_ = obj.Pkg()
	}
}

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string src1ᶜ1 => ref src1ᶜ1ᵇ;
private static readonly @string src1ᶜ1ᵇ = """

package main

import (
	"go/types"
	_ "go/importer"
)

func main() {
	var info types.Info
	for _, obj := range info.Uses {
		_ = obj.Pkg()
	}
}

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string src2ᶜ1 => ref src2ᶜ1ᵇ;
private static readonly @string src2ᶜ1ᵇ = """

package main

import (
	_ "go/importer"
	"go/types"
)

func main() {
	var info types.Info
	for _, obj := range info.Uses {
		_ = obj.Pkg()
	}
}

"""u8.Raw();

// This tests that the package associated with the types.Object.Pkg method
// is the type's package independent of the order in which the imports are
// listed in the sources src1, src2 below.
// The actual issue is in go/internal/gcimporter which has a corresponding
// test; we leave this test here to verify correct behavior at the go/types
// level.
public static void TestIssue13898(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(t));

    @string src0 = src0ᶜ;
    // like src0, but also imports go/importer
    @string src1 = src1ᶜ1;
    // like src1 but with different import order
    // (used to fail with this issue)
    @string src2 = src2ᶜ1;
    void f(@string test, @string src) {
        var info = Ꮡ(new typesꓸInfo(Uses: new map<ж<ast.Ident>, types.Object>()));
        mustTypecheck(src, nil, info);

        ж<types.Package> pkg = default;
        nint count = 0;
        foreach (var (id, obj) in info.Uses) {
            if (id.Name == "Pkg"u8) {
                pkg = obj.Pkg();
                count++;
            }
        }
        if (count != 1) {
            t.Fatalf("%s: got %d entries named Pkg; want 1", test, count);
        }
        if (pkg.Name() != "types"u8) {
            t.Fatalf("%s: got %v; want package types", test, pkg.OrTypedNil());
        }
    }

    f(src0ˢ, src0);
    f(src1ˢ, src1);
    f(src2ˢ, src2);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ21 => ref srcᶜ21ᵇ;
private static readonly @string srcᶜ21ᵇ = @"package p; func f() { var a, b, c, d, e int }"u8;

public static void TestIssue22525(ж<testing.T> t) {
    @string src = srcᶜ21;

    @string got = "\n";
    ref var conf = ref heap<types.Config>(new Config(Error: (error err) => {
        got += err.Error() + "\n"u8;
    }), out var Ꮡconf);
    typecheck(src, Ꮡconf, nil); // do not crash
    @string want = "\n"u8 + "p:1:27: declared and not used: a\n"u8 + "p:1:30: declared and not used: b\n"u8 + "p:1:33: declared and not used: c\n"u8 + "p:1:36: declared and not used: d\n"u8 + "p:1:39: declared and not used: e\n"u8;
    if (got != want) {
        t.Errorf("got: %swant: %s", got, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string prefixᶜ2 => ref prefixᶜ2ᵇ;
private static readonly @string prefixᶜ2ᵇ = @"package p; import ""unsafe""; type P *struct{}; type I interface{}; type T "u8;

public static void TestIssue25627(ж<testing.T> t) {
    @string prefix = prefixᶜ2;
    // The src strings (without prefix) are constructed such that the number of semicolons
    // plus one corresponds to the number of fields expected in the respective struct.
    foreach (var (_, src) in range(slice<@string>([
        @"struct { x Missing }",
        @"struct { Missing }",
        @"struct { *Missing }",
        @"struct { unsafe.Pointer }",
        @"struct { P }",
        @"struct { *I }",
        @"struct { a int; b Missing; *Missing }"
    ]))) {
        var f = mustParse(fset, prefix + src);

        ref var cfg = ref heap<types.Config>(new Config(Importer: defaultImporter(fset), Error: (error errΔ1) => {
        }), out var Ꮡcfg);
        var info = Ꮡ(new typesꓸInfo(Types: new map<ast.Expr, types.TypeAndValue>()));
        var (_, err) = Ꮡcfg.Check(f.Name.Name, fset, [f], info);
        if (err != nil) {
            var (_, ok) = err._<typesꓸError>(ᐧ); if (!ok) {
                t.Fatal(err);
            }
        }

        var infoʗ1 = info;
        ast.Inspect(new ast.FileжNode(f), (ast.Node n) => {
            var (spec, _) = n._<ж<ast.TypeSpec>>(ᐧ); if (spec != nil) {
                var (tv, ok) = infoʗ1.Types[spec.Type, ꟷ]; if (ok && spec.Name.Name == "T"u8) {
                    nint want = strings.Count(src, ";") + 1;
                    nint got = tv.Type._<ж<types.Struct>>().NumFields(); if (got != want) {
                        t.Errorf("%s: got %d fields; want %d", src, got, want);
                    }
                }
            }
            return true;
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object objectXNotFoundˢ = (@string)"object X not found"u8;

public static void TestIssue28005(ж<testing.T> t) {
    // method names must match defining interface name for this test
    // (see last comment in this function)
    var sources = new @string[]{
        "package p; type A interface{ A() }",
        "package p; type B interface{ B() }",
        "package p; type X interface{ A; B }"
    }.array();

    // compute original file ASTs
    array<ж<ast.File>> orig = new(3); /* len(sources) */
    foreach (var (i, src) in range(sources.ΔRangeSnapshot())) {
        orig[i] = mustParse(fset, src);
    }

    // run the test for all order permutations of the incoming files
    foreach (var (_, vᴛ1) in range(GoReflect.WithElemDims(new array<nint>[]{
        new nint[]{0, 1, 2}.array(),
        new nint[]{0, 2, 1}.array(),
        new nint[]{1, 0, 2}.array(),
        new nint[]{1, 2, 0}.array(),
        new nint[]{2, 0, 1}.array(),
        new nint[]{2, 1, 0}.array()
    }.slice(), 3))) {
        var perm = vᴛ1.Clone();

        // create file order permutation
        var files = new slice<ж<ast.File>>(len(sources));
        foreach (var i in range(len(perm))) {
            files[i] = orig[perm[i]];
        }

        // type-check package with given file order permutation
        ref var conf = ref heap(new types.Config(), out var Ꮡconf);
        var info = Ꮡ(new typesꓸInfo(Defs: new map<ж<ast.Ident>, types.Object>()));
        var (_, err) = Ꮡconf.Check(""u8, fset, files, info);
        if (err != nil) {
            t.Fatal(err);
        }

        // look for interface object X
        types.Object obj = default;
        foreach (var (name, def) in info.Defs) {
            if (name.Name == "X"u8) {
                obj = def;
                break;
            }
        }
        if (obj == nil) {
            t.Fatal(objectXNotFoundˢ);
        }
        var iface = obj.Type().Underlying()._<ж<types.Interface>>(); // object X must be an interface

        // Each iface method m is embedded; and m's receiver base type name
        // must match the method's name per the choice in the source file.
        for (nint i = 0; i < iface.NumMethods(); i++) {
            var m = iface.Method(i);
            @string recvName = m.Signature().Recv().Type()._<ж<types.Named>>().Obj().Name();
            if (recvName != m.Name()) {
                t.Errorf("perm %v: got recv %s; want %s", perm, recvName, m.Name());
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string errorˢ3 => ref errorˢ3ᵇ;
private static readonly @string errorˢ3ᵇ = "error"u8;
internal static ref readonly @string errorˢ4 => ref errorˢ4ᵇ;
private static readonly @string errorˢ4ᵇ = "Error"u8;

public static void TestIssue28282(ж<testing.T> t) {
    // create type interface { error }
    var et = Universe.Lookup(errorˢ3).Type();
    var it = NewInterfaceType(default, [et]);
    it.Complete();
    // verify that after completing the interface, the embedded method remains unchanged
    var want = et.Underlying()._<ж<types.Interface>>().Method(0);
    var got = it.Method(0);
    if (got != want) {
        t.Fatalf("%s.Method(0): got %q (%p); want %q (%p)", it.OrTypedNil(), got.OrTypedNil(), got.OrTypedNil(), want.OrTypedNil(), want.OrTypedNil());
    }
    // verify that lookup finds the same method in both interfaces (redundant check)
    var (obj, _, _) = LookupFieldOrMethod(et, false, nil, errorˢ4);
    if (!AreEqual(obj, want)) {
        t.Fatalf("%s.Lookup: got %q (%p); want %q (%p)", et, obj, obj, want.OrTypedNil(), want.OrTypedNil());
    }
    (obj, _, _) = LookupFieldOrMethod(new types.InterfaceжΔType(it), false, nil, errorˢ4);
    if (!AreEqual(obj, want)) {
        t.Fatalf("%s.Lookup: got %q (%p); want %q (%p)", it.OrTypedNil(), obj, obj, want.OrTypedNil(), want.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string packagePTypeAInterfaceMˢ => ref packagePTypeAInterfaceMˢᵇ;
private static readonly @string packagePTypeAInterfaceMˢᵇ = @"package p; type A interface { M() }"u8;
internal static ref readonly @string packagePVarBInterfaceAˢ => ref packagePVarBInterfaceAˢᵇ;
private static readonly @string packagePVarBInterfaceAˢᵇ = @"package p; var B interface { A }"u8;

public static void TestIssue29029(ж<testing.T> t) {
    var f1 = mustParse(fset, packagePTypeAInterfaceMˢ);
    var f2 = mustParse(fset, packagePVarBInterfaceAˢ);

    // printInfo prints the *Func definitions recorded in info, one *Func per line.
    @string printInfo(ж<typesꓸInfo> infoΔ1) {
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        foreach (var (_, obj) in infoΔ1.Defs) {
            var (fn, ok) = obj._<ж<types.Func>>(ᐧ); if (ok) {
                fmt.Fprintln(new types_test_package.strings_BuilderжWriter(Ꮡbuf), fn.OrTypedNil());
            }
        }
        return buf.String();
    }

    // The *Func (method) definitions for package p must be the same
    // independent on whether f1 and f2 are type-checked together, or
    // incrementally.

    // type-check together
    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    var info = Ꮡ(new typesꓸInfo(Defs: new map<ж<ast.Ident>, types.Object>()));
    var check = NewChecker(Ꮡconf, fset, NewPackage(""u8, "p"), info);
    {
        var err = check.Files([f1, f2]); if (err != nil) {
            t.Fatal(err);
        }
    }
    @string want = printInfo(info);

    // type-check incrementally
    info = Ꮡ(new typesꓸInfo(Defs: new map<ж<ast.Ident>, types.Object>()));
    check = NewChecker(Ꮡconf, fset, NewPackage(""u8, "p"), info);
    {
        var err = check.Files([f1]); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = check.Files([f2]); if (err != nil) {
            t.Fatal(err);
        }
    }
    @string got = printInfo(info);

    if (got != want) {
        t.Errorf("\ngot : %swant: %s", got, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string asrcᶜ => ref asrcᶜᵇ;
private static readonly @string asrcᶜᵇ = @"package a; type I interface{ M() }; type T struct { F interface { I } }"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string bsrcᶜ => ref bsrcᶜᵇ;
private static readonly @string bsrcᶜᵇ = @"package b; import ""a""; type T struct { F interface { a.I } }; var _ = a.T(T{})"u8;

public static void TestIssue34151(ж<testing.T> t) {
    @string asrc = asrcᶜ;
    @string bsrc = bsrcᶜ;

    var a = mustTypecheck(asrc, nil, nil);

    ref var conf = ref heap<types.Config>(new Config(Importer: new importHelper(pkg: a)), out var Ꮡconf);
    mustTypecheck(bsrc, Ꮡconf, nil);
}

partial struct importHelper {
    internal ж<types.Package> pkg;
    internal types.Importer fallback;
}

internal static (ж<types.Package>, error) Import(this importHelper h, @string path) {
    if (path == h.pkg.Path()) {
        return (h.pkg, default);
    }
    if (h.fallback == nil) {
        return (default, fmt.Errorf("got package path %q; want %q"u8, path, h.pkg.Path()));
    }
    return h.fallback.Import(path);
}

// TestIssue34921 verifies that we don't update an imported type's underlying
// type when resolving an underlying type. Specifically, when determining the
// underlying type of b.T (which is the underlying type of a.T, which is int)
// we must not set the underlying type of a.T again since that would lead to
// a race condition if package b is imported elsewhere, in a package that is
// concurrently type-checked.
public static void TestIssue34921(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover(); if (r != nil) {
                t.Error(r);
            }
        }, ref ᒐ);

        slice<@string> sources = slice<@string>([
            @"package a; type T int",
            @"package b; import ""a""; type T a.T"
        ]);

        ж<types.Package> pkg = default;
        foreach (var (_, src) in range(sources)) {
            ref var conf = ref heap<types.Config>(new Config(Importer: new importHelper(pkg: pkg)), out var Ꮡconf);
            pkg = mustTypecheck(src, Ꮡconf, nil); // pkg imported by the next package in this test
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIssue43088(ж<testing.T> t) {
    // type T1 struct {
    //         _ T2
    // }
    //
    // type T2 struct {
    //         _ struct {
    //                 _ T2
    //         }
    // }
    var n1 = NewTypeName(nopos, nil, "T1", default);
    var T1 = NewNamed(n1, default, default);
    var n2 = NewTypeName(nopos, nil, "T2", default);
    var T2 = NewNamed(n2, default, default);
    var s1 = NewStruct(slice<ж<types.Var>>([NewField(nopos, nil, "_", new types.NamedжΔType(T2), false)]), default);
    T1.SetUnderlying(new types.StructжΔType(s1));
    var s2 = NewStruct(slice<ж<types.Var>>([NewField(nopos, nil, "_", new types.NamedжΔType(T2), false)]), default);
    var s3 = NewStruct(slice<ж<types.Var>>([NewField(nopos, nil, "_", new types.StructжΔType(s2), false)]), default);
    T2.SetUnderlying(new types.StructжΔType(s3));

    // These calls must terminate (no endless recursion).
    Comparable(new types.NamedжΔType(T1));
    Comparable(new types.NamedжΔType(T2));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string pointerˢ => ref pointerˢᵇ;
private static readonly @string pointerˢᵇ = "Pointer"u8;
internal static ref readonly @string unsafePointerˢ => ref unsafePointerˢᵇ;
private static readonly @string unsafePointerˢᵇ = "unsafe.Pointer"u8;
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static ref readonly @string fooPointerˢ => ref fooPointerˢᵇ;
private static readonly @string fooPointerˢᵇ = "foo.Pointer"u8;

public static void TestIssue44515(ж<testing.T> t) {
    var typ = Unsafe.Scope().Lookup(pointerˢ).Type();

    @string got = TypeString(typ, default);
    @string want = unsafePointerˢ;
    if (got != want) {
        t.Errorf("got %q; want %q", got, want);
    }

    var qf = @string (ж<types.Package> pkg) => {
        if (pkg == Unsafe) {
            return fooˢ;
        }
        return ""u8;
    };
    got = TypeString(typ, new Func<ж<types.Package>, @string>(qf));
    want = fooPointerˢ;
    if (got != want) {
        t.Errorf("got %q; want %q", got, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string asrcᶜ1 => ref asrcᶜ1ᵇ;
private static readonly @string asrcᶜ1ᵇ = @"package a; import ""text/template""; func F(template.Template) {}; func G(int) {}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string bsrcᶜ1 => ref bsrcᶜ1ᵇ;
private static readonly @string bsrcᶜ1ᵇ = """

package b

import (
	"a"
	"html/template"
)

func _() {
	// Packages should be fully qualified when there is ambiguity within the
	// error string itself.
	a.F(template /* ERRORx "cannot use.*html/template.* as .*text/template" */ .Template{})
}

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string csrcᶜ => ref csrcᶜᵇ;
private static readonly @string csrcᶜᵇ = """

package c

import (
	"a"
	"fmt"
	"html/template"
)

// go.dev/issue/46905: make sure template is not the first package qualified.
var _ fmt.Stringer = 1 // ERRORx "cannot use 1.*as fmt\\.Stringer"

// Packages should be fully qualified when there is ambiguity in reachable
// packages. In this case both a (and for that matter html/template) import
// text/template.
func _() { a.G(template /* ERRORx "cannot use .*html/template.*Template" */ .Template{}) }

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string tsrcᶜ => ref tsrcᶜᵇ;
private static readonly @string tsrcᶜᵇ = """

package template

import "text/template"

type T int

// Verify that the current package name also causes disambiguation.
var _ T = template /* ERRORx "cannot use.*text/template.* as T value" */.Template{}

"""u8.Raw();

public static void TestIssue43124(ж<testing.T> t) {
    // TODO(rFindley) move this to testdata by enhancing support for importing.

    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(t)); // The go command is needed for the importer to determine the locations of stdlib .a files.

    // All involved packages have the same name (template). Error messages should
    // disambiguate between text/template and html/template by printing the full
    // path.
    @string asrc = asrcᶜ1;
    @string bsrc = bsrcᶜ1;
    @string csrc = csrcᶜ;

    @string tsrc = tsrcᶜ;

    var a = mustTypecheck(asrc, nil, nil);
    ref var imp = ref heap<importHelper>(new importHelper(
        pkg: a,
        // TODO(adonovan): use same FileSet as mustTypecheck.
        fallback: defaultImporter(token.NewFileSet())
    ), out var Ꮡimp);

    var impʗ1 = imp;
    var withImporter = (ж<types.Config> cfg) => {
        cfg.Importer = impʗ1;
    };

    testFiles(t, ["b.go"], [slice<byte>(bsrc)], false, withImporter);
    testFiles(t, ["c.go"], [slice<byte>(csrc)], false, withImporter);
    testFiles(t, ["t.go"], [slice<byte>(tsrc)], false, withImporter);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string comparableˢ => ref comparableˢᵇ;
private static readonly @string comparableˢᵇ = "comparable"u8;
internal static readonly object anyIsNotAComparableTypeˢ = (@string)"any is not a comparable type"u8;
internal static readonly object comparableIsNotAˢ = (@string)"comparable is not a comparable type"u8;
internal static readonly object anyImplementsComparableˢ = (@string)"any implements comparable"u8;
internal static readonly object comparableDoesNotˢ = (@string)"comparable does not implement any"u8;
internal static readonly object anyAssignableToˢ = (@string)"any assignable to comparable"u8;
internal static readonly object comparableNotAssignableˢ = (@string)"comparable not assignable to any"u8;

public static void TestIssue50646(ж<testing.T> t) {
    var anyType = Universe.Lookup(anyˢ).Type().Underlying();
    var comparableType = Universe.Lookup(comparableˢ).Type();

    if (!Comparable(anyType)) {
        t.Error(anyIsNotAComparableTypeˢ);
    }
    if (!Comparable(comparableType)) {
        t.Error(comparableIsNotAˢ);
    }

    if (Implements(anyType, comparableType.Underlying()._<ж<types.Interface>>())) {
        t.Error(anyImplementsComparableˢ);
    }
    if (!Implements(comparableType, anyType._<ж<types.Interface>>())) {
        t.Error(comparableDoesNotˢ);
    }

    if (AssignableTo(anyType, comparableType)) {
        t.Error(anyAssignableToˢ);
    }
    if (!AssignableTo(comparableType, anyType)) {
        t.Error(comparableNotAssignableˢ);
    }
}

public static void TestIssue55030(ж<testing.T> t) {
    // makeSig makes the signature func(typ...)
    // If valid is not set, making that signature is expected to panic.
    void makeSig(typesꓸType typ, bool valid) {
        GoFrame ᒐ = default;
        try {
            if (!valid) {
                defer(() => {
                    var r = recover();
                    if (r == nil) {
                        throw panic("NewSignatureType panic expected");
                    }
                    var (_, ok) = r._<@string>(ᐧ); if (!ok) {
                        throw panic("NewSignatureType string panic expected");
                    }
                }, ref ᒐ);
            }
            var par = NewParam(nopos, nil, ""u8, typ);
            var @params = NewTuple(par);
            NewSignatureType(nil, default, default, @params, nil, true);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    // makeSig must not panic for the following (example) types:
    // []int
    makeSig(new types.SliceжΔType(NewSlice(new types.BasicжΔType(Typ[Int]))), true);

    // string
    makeSig(new types.BasicжΔType(Typ[ΔString]), true);

    // P where P's common underlying type is string
    {
        var P = NewTypeName(nopos, nil, "P", default); // [P string]
        makeSig(new types.TypeParamжΔType(NewTypeParam(P, new types.InterfaceжΔType(NewInterfaceType(default, [new types.BasicжΔType(Typ[ΔString])])))), true);
    }

    // P where P's common underlying type is an (unnamed) slice
    {
        var P = NewTypeName(nopos, nil, "P", default); // [P []int]
        makeSig(new types.TypeParamжΔType(NewTypeParam(P, new types.InterfaceжΔType(NewInterfaceType(default, slice<typesꓸType>([new types.SliceжΔType(NewSlice(new types.BasicжΔType(Typ[Int])))]))))), true);
    }

    // P where P's type set contains strings and []byte
    {
        var t1 = NewTerm(true, new types.BasicжΔType(Typ[ΔString]));                                // ~string
        var t2 = NewTerm(false, new types.SliceжΔType(NewSlice(new types.BasicжΔType(Typ[Byte])))); // []byte
        var u = NewUnion([t1, t2]);                                                                 // ~string | []byte
        var P = NewTypeName(nopos, nil, "P", default);                                              // [P ~string | []byte]
        makeSig(new types.TypeParamжΔType(NewTypeParam(P, new types.InterfaceжΔType(NewInterfaceType(default, [new types.UnionжΔType(u)])))), true);
    }

    // makeSig must panic for the following (example) types:
    // int
    makeSig(new types.BasicжΔType(Typ[Int]), false);

    // P where P's type set doesn't have any specific types
    {
        var P = NewTypeName(nopos, nil, "P", default); // [P any]
        makeSig(new types.TypeParamжΔType(NewTypeParam(P, new types.InterfaceжΔType(NewInterfaceType(default, slice<typesꓸType>([Universe.Lookup(anyˢ).Type()]))))), false);
    }

    // P where P's type set doesn't have any slice or string types
    {
        var P = NewTypeName(nopos, nil, "P", default); // [P any]
        makeSig(new types.TypeParamжΔType(NewTypeParam(P, new types.InterfaceжΔType(NewInterfaceType(default, [new types.BasicжΔType(Typ[Int])])))), false);
    }
}

internal partial struct TestIssue51093_type /*dyn*/ {
    internal @string typ;
    internal @string val;
}

public static void TestIssue51093(ж<testing.T> t) {
    // Each test stands for a conversion of the form P(val)
    // where P is a type parameter with typ as constraint.
    // The test ensures that P(val) has the correct type P
    // and is not a constant.
    slice<TestIssue51093_type> tests = slice<TestIssue51093_type>([
        new("bool", "false"),
        new("int", "-1"),
        new("uint", "1.0"),
        new("rune", "'a'"),
        new("float64", "3.5"),
        new("complex64", "1.25"),
        new("string", "\"foo\""),

        // some more complex constraints
        new("~byte", "1"),
        new("~int | ~float64 | complex128", "1"),
        new("~uint64 | ~rune", "'X'")
    ]);

    foreach (var (_, test) in range(tests)) {
        @string src = fmt.Sprintf("package p; func _[P %s]() { _ = P(%s) }"u8, test.typ, test.val);
        var types = new map<ast.Expr, types.TypeAndValue>();
        mustTypecheck(src, nil, Ꮡ(new typesꓸInfo(Types: types)));

        nint n = default;
        foreach (var (x, tv) in types) {
            var (xΔ1, _) = x._<ж<ast.CallExpr>>(ᐧ); if (xΔ1 != nil) {
                // there must be exactly one CallExpr which is the P(val) conversion
                n++;
                var (tpar, _) = tv.Type._<ж<types.TypeParam>>(ᐧ);
                if (tpar == nil) {
                    t.Fatalf("%s: got type %s, want type parameter", ExprString(new ast.CallExprжExpr(xΔ1)), tv.Type);
                }
                @string name = tpar.Obj().Name(); if (name != "P"u8) {
                    t.Fatalf("%s: got type parameter name %s, want P", ExprString(new ast.CallExprжExpr(xΔ1)), name);
                }
                // P(val) must not be constant
                if (tv.Value != nil) {
                    t.Errorf("%s: got constant value %s (%s), want no constant", ExprString(new ast.CallExprжExpr(xΔ1)), tv.Value, tv.Value.String());
                }
            }
        }

        if (n != 1) {
            t.Fatalf("%s: got %d CallExpr nodes; want 1", src, (nint)1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedFailureButItDidˢ = (@string)"Expected failure, but it did not"u8;

internal partial struct TestIssue54258_tests /*dyn*/ {
    internal @string main, b, want;
}

public static void TestIssue54258(ж<testing.T> t) {
    var tests = slice<TestIssue54258_tests>([
        new( //---------------------------------------------------------------
            """
package main
import "b"
type I0 interface {
	M0(w struct{ f string })
}
var _ I0 = b.S{}

"""u8.Raw(),
            """
package b
type S struct{}
func (S) M0(struct{ f string }) {}

"""u8.Raw(),
            """
6:12: cannot use b[.]S{} [(]value of struct type b[.]S[)] as I0 value in variable declaration: b[.]S does not implement I0 [(]wrong type for method M0[)]
.*have M0[(]struct{f string /[*] package b [*]/ }[)]
.*want M0[(]struct{f string /[*] package main [*]/ }[)]
"""u8.Raw()),

        new( //---------------------------------------------------------------
            """
package main
import "b"
type I1 interface {
	M1(struct{ string })
}
var _ I1 = b.S{}

"""u8.Raw(),
            """
package b
type S struct{}
func (S) M1(struct{ string }) {}

"""u8.Raw(),
            """
6:12: cannot use b[.]S{} [(]value of struct type b[.]S[)] as I1 value in variable declaration: b[.]S does not implement I1 [(]wrong type for method M1[)]
.*have M1[(]struct{string /[*] package b [*]/ }[)]
.*want M1[(]struct{string /[*] package main [*]/ }[)]
"""u8.Raw()),

        new( //---------------------------------------------------------------
            """
package main
import "b"
type I2 interface {
	M2(y struct{ f struct{ f string } })
}
var _ I2 = b.S{}

"""u8.Raw(),
            """
package b
type S struct{}
func (S) M2(struct{ f struct{ f string } }) {}

"""u8.Raw(),
            """
6:12: cannot use b[.]S{} [(]value of struct type b[.]S[)] as I2 value in variable declaration: b[.]S does not implement I2 [(]wrong type for method M2[)]
.*have M2[(]struct{f struct{f string} /[*] package b [*]/ }[)]
.*want M2[(]struct{f struct{f string} /[*] package main [*]/ }[)]
"""u8.Raw()),

        new( //---------------------------------------------------------------
            """
package main
import "b"
type I3 interface {
	M3(z struct{ F struct{ f string } })
}
var _ I3 = b.S{}

"""u8.Raw(),
            """
package b
type S struct{}
func (S) M3(struct{ F struct{ f string } }) {}

"""u8.Raw(),
            """
6:12: cannot use b[.]S{} [(]value of struct type b[.]S[)] as I3 value in variable declaration: b[.]S does not implement I3 [(]wrong type for method M3[)]
.*have M3[(]struct{F struct{f string /[*] package b [*]/ }}[)]
.*want M3[(]struct{F struct{f string /[*] package main [*]/ }}[)]
"""u8.Raw()),

        new( //---------------------------------------------------------------
            """
package main
import "b"
type I4 interface {
	M4(_ struct { *string })
}
var _ I4 = b.S{}

"""u8.Raw(),
            """
package b
type S struct{}
func (S) M4(struct { *string }) {}

"""u8.Raw(),
            """
6:12: cannot use b[.]S{} [(]value of struct type b[.]S[)] as I4 value in variable declaration: b[.]S does not implement I4 [(]wrong type for method M4[)]
.*have M4[(]struct{[*]string /[*] package b [*]/ }[)]
.*want M4[(]struct{[*]string /[*] package main [*]/ }[)]
"""u8.Raw()),

        new( //---------------------------------------------------------------
            """
package main
import "b"
type t struct{ A int }
type I5 interface {
	M5(_ struct {b.S;t})
}
var _ I5 = b.S{}

"""u8.Raw(),
            """
package b
type S struct{}
type t struct{ A int }
func (S) M5(struct {S;t}) {}

"""u8.Raw(),
            """
7:12: cannot use b[.]S{} [(]value of struct type b[.]S[)] as I5 value in variable declaration: b[.]S does not implement I5 [(]wrong type for method M5[)]
.*have M5[(]struct{b[.]S; b[.]t}[)]
.*want M5[(]struct{b[.]S; t}[)]
"""u8.Raw())
    ]);

    var fset = token.NewFileSet();
    var fsetʗ1 = fset;
    void test(@string main, @string b, @string want) {
        var re = regexp.MustCompile(want);
        var bpkg = mustTypecheck(b, nil, nil);
        var mast = mustParse(fsetʗ1, main);
        ref var conf = ref heap<types.Config>(new Config(Importer: new importHelper(pkg: bpkg)), out var Ꮡconf);
        var (_, err) = Ꮡconf.Check(mast.Name.Name, fsetʗ1, [mast], nil);
        if (err == nil) {
            t.Error(expectedFailureButItDidˢ);
        } else {
            @string got = err.Error(); if (!re.MatchString(got)) {
                t.Errorf("Wanted match for\n\t%s\n but got\n\t%s", want, got);
            } else if (testing.Verbose()) {
                t.Logf("Saw expected\n\t%s", err.Error());
            }
        }
    }
    foreach (var (_, tΔ1) in range(tests)) {
        test(tΔ1.main, tΔ1.b, tΔ1.want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string go115UsesCgoˢ => ref go115UsesCgoˢᵇ;
private static readonly @string go115UsesCgoˢᵇ = "go115UsesCgo"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ22 => ref srcᶜ22ᵇ;
private static readonly @string srcᶜ22ᵇ = """
// -gotypesalias=1

package p

/*
struct layout {};
*/
import "C"

type Layout = C.struct_layout

func (*Layout /* ERROR "cannot define new methods on non-local type Layout" */) Binding() {}

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string cgoTypesᶜ => ref cgoTypesᶜᵇ;
private static readonly @string cgoTypesᶜᵇ = """

// Code generated by cmd/cgo; DO NOT EDIT.

package p

import "unsafe"

import "syscall"

import _cgopackage "runtime/cgo"

type _ _cgopackage.Incomplete
var _ syscall.Errno
func _Cgo_ptr(ptr unsafe.Pointer) unsafe.Pointer { return ptr }

//go:linkname _Cgo_always_false runtime.cgoAlwaysFalse
var _Cgo_always_false bool
//go:linkname _Cgo_use runtime.cgoUse
func _Cgo_use(interface{})
//go:linkname _Cgo_keepalive runtime.cgoKeepAlive
//go:noescape
func _Cgo_keepalive(interface{})
//go:linkname _Cgo_no_callback runtime.cgoNoCallback
func _Cgo_no_callback(bool)
type _Ctype_struct_layout struct {
}

type _Ctype_void [0]byte

//go:linkname _cgo_runtime_cgocall runtime.cgocall
func _cgo_runtime_cgocall(unsafe.Pointer, uintptr) int32

//go:linkname _cgoCheckPointer runtime.cgoCheckPointer
//go:noescape
func _cgoCheckPointer(interface{}, interface{})

//go:linkname _cgoCheckResult runtime.cgoCheckResult
//go:noescape
func _cgoCheckResult(interface{})

"""u8.Raw();

public static void TestIssue59944(ж<testing.T> t) {
    testenv.MustHaveCGO(new types_test_package.testing_TжTB(t));

    // Methods declared on aliases of cgo types are not permitted.
    @string src = srcᶜ22;

    // code generated by cmd/cgo for the above source.
    @string cgoTypes = cgoTypesᶜ;
    testFiles(t, ["p.go", "_cgo_gotypes.go"], [slice<byte>(src), slice<byte>(cgoTypes)], false, (ж<types.Config> cfg) => {
        boolFieldAddr(cfg, go115UsesCgoˢ).Value = true;
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedSyntaxErrorˢ = (@string)"expected syntax error"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ23 => ref srcᶜ23ᵇ;
private static readonly @string srcᶜ23ᵇ = """

package p

func A(func(any), ...any) {}
func B[T any](T)          {}

func _() {
	A(B, nil // syntax error: missing ',' before newline in argument list
}

"""u8.Raw();

public static void TestIssue61931(ж<testing.T> t) {
    @string src = srcᶜ23;
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, pkgName(src), src, 0);
    if (err == nil) {
        t.Fatal(expectedSyntaxErrorˢ);
    }

    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    Ꮡconf.Check(f.Name.Name, fset, [f], nil); // must not panic
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ24 => ref srcᶜ24ᵇ;
private static readonly @string srcᶜ24ᵇ = """

package p

func f[T any]() {}
func _()        { f() }

"""u8.Raw();

public static void TestIssue61938(ж<testing.T> t) {
    @string src = srcᶜ24;
    // no error handler provided (this issue)
    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    typecheck(src, Ꮡconf, nil); // must not panic

    // with error handler (sanity check)
    conf.Error = (error _) => {
    };
    typecheck(src, Ꮡconf, nil); // must not panic
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object variableVNotFoundˢ = (@string)"variable v not found"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ25 => ref srcᶜ25ᵇ;
private static readonly @string srcᶜ25ᵇ = """

package p

func _() {
        use(f[*string])
}

func use(func()) {}

func f[I *T, T any]() {
        var v T
        _ = v
}
"""u8.Raw();

public static void TestIssue63260(ж<testing.T> t) {
    @string src = srcᶜ25;

    ref var info = ref heap<typesꓸInfo>(new typesꓸInfo(
        Defs: new map<ж<ast.Ident>, types.Object>()
    ), out var Ꮡinfo);
    var pkg = mustTypecheck(src, nil, Ꮡinfo);

    // get type parameter T in signature of f
    var T = pkg.Scope().Lookup("f").Type()._<ж<typesꓸSignature>>().TypeParams().At(1);
    if (T.Obj().Name() != "T"u8) {
        t.Fatalf("got type parameter %s, want T", T.OrTypedNil());
    }

    // get type of variable v in body of f
    types.Object v = default;
    foreach (var (name, obj) in info.Defs) {
        if (name.Name == "v"u8) {
            v = obj;
            break;
        }
    }
    if (v == nil) {
        t.Fatal(variableVNotFoundˢ);
    }

    // type of v and T must be pointer-identical
    if (!AreEqual(v.Type(), T)) {
        t.Fatalf("types of v and T are not pointer-identical: %p != %p", v.Type()._<ж<types.TypeParam>>().OrTypedNil(), T.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object objectSNotFoundˢ = (@string)"object S not found"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ26 => ref srcᶜ26ᵇ;
private static readonly @string srcᶜ26ᵇ = """

package p

type A = []int
type S struct{ A }

"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ3 => ref wantᶜ3ᵇ;
private static readonly @string wantᶜ3ᵇ = "type p.S struct{p.A}"u8;

public static void TestIssue44410(ж<testing.T> t) {
    @string src = srcᶜ26;

    t.Setenv(godebugˢ, gotypesalias1ˢ);
    var pkg = mustTypecheck(src, nil, nil);

    var S = pkg.Scope().Lookup("S");
    if (S == nil) {
        t.Fatal(objectSNotFoundˢ);
    }

    @string got = S.String();
    @string want = wantᶜ3;
    if (got != want) {
        t.Fatalf("got %q; want %q", got, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string asrcᶜ2 => ref asrcᶜ2ᵇ;
private static readonly @string asrcᶜ2ᵇ = @"package a; type S struct{}; func (S) m() {}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string bsrcᶜ2 => ref bsrcᶜ2ᵇ;
private static readonly @string bsrcᶜ2ᵇ = @"package b; type S struct{}; func (S) M() {}"u8;

internal partial struct TestIssue59831_tests /*dyn*/ {
    internal ж<types.Package> imported;
    internal @string src, err;
}

public static void TestIssue59831(ж<testing.T> t) {
    // Package a exports a type S with an unexported method m;
    // the tests check the error messages when m is not found.
    @string asrc = asrcᶜ2;
    var apkg = mustTypecheck(asrc, nil, nil);

    // Package b exports a type S with an exported method m;
    // the tests check the error messages when M is not found.
    @string bsrc = bsrcᶜ2;
    var bpkg = mustTypecheck(bsrc, nil, nil);

    var tests = slice<TestIssue59831_tests>([
        // tests importing a (or nothing)
        new(apkg, @"package a1; import ""a""; var _ interface { M() } = a.S{}",
            "a.S does not implement interface{M()} (missing method M) have m() want M()"),

        new(apkg, @"package a2; import ""a""; var _ interface { m() } = a.S{}",
            "a.S does not implement interface{m()} (unexported method m)"), // test for issue

        new(nil, @"package a3; type S struct{}; func (S) m(); var _ interface { M() } = S{}",
            "S does not implement interface{M()} (missing method M) have m() want M()"),

        new(nil, @"package a4; type S struct{}; func (S) m(); var _ interface { m() } = S{}",
            ""), // no error expected

        new(nil, @"package a5; type S struct{}; func (S) m(); var _ interface { n() } = S{}",
            "S does not implement interface{n()} (missing method n)"),

        // tests importing b (or nothing)
        new(bpkg, @"package b1; import ""b""; var _ interface { m() } = b.S{}",
            "b.S does not implement interface{m()} (missing method m) have M() want m()"),

        new(bpkg, @"package b2; import ""b""; var _ interface { M() } = b.S{}",
            ""), // no error expected

        new(nil, @"package b3; type S struct{}; func (S) M(); var _ interface { M() } = S{}",
            ""), // no error expected

        new(nil, @"package b4; type S struct{}; func (S) M(); var _ interface { m() } = S{}",
            "S does not implement interface{m()} (missing method m) have M() want m()"),

        new(nil, @"package b5; type S struct{}; func (S) M(); var _ interface { n() } = S{}",
            "S does not implement interface{n()} (missing method n)")
    ]);

    foreach (var (_, test) in range(tests)) {
        // typecheck test source
        ref var conf = ref heap<types.Config>(new Config(Importer: new importHelper(pkg: test.imported)), out var Ꮡconf);
        var (pkg, err) = typecheck(test.src, Ꮡconf, nil);
        if (err == nil) {
            if (test.err != ""u8) {
                t.Errorf("package %s: got no error, want %q", pkg.Name(), test.err);
            }
            continue;
        }
        if (test.err == ""u8) {
            t.Errorf("package %s: got %q, want not error", pkg.Name(), err.Error());
        }

        // flatten reported error message
        @string errmsg = strings.ReplaceAll(err.Error(), "\n", " ");
        errmsg = strings.ReplaceAll(errmsg, "\t", ""u8);

        // verify error message
        if (!strings.Contains(errmsg, test.err)) {
            t.Errorf("package %s: got %q, want %q", pkg.Name(), errmsg, test.err);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ27 => ref srcᶜ27ᵇ;
private static readonly @string srcᶜ27ᵇ = """

//go:build go1.18
package p

func f[S ~[]E, E any](S) {}

func _() {
	f([]string{})
}

"""u8.Raw();

public static void TestIssue64759(ж<testing.T> t) {
    @string src = srcᶜ27;
    // Per the go:build directive, the source must typecheck
    // even though the (module) Go version is set to go1.17.
    ref var conf = ref heap<types.Config>(new Config(GoVersion: "go1.17"), out var Ꮡconf);
    mustTypecheck(src, Ꮡconf, nil);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ28 => ref srcᶜ28ᵇ;
private static readonly @string srcᶜ28ᵇ = """

package p

func f(x int) {
	for i, j := range x {
		_, _ = i, j
	}
	var a, b int
	for a, b = range x {
		_, _ = a, b
	}
}

"""u8.Raw();

public static void TestIssue68334(ж<testing.T> t) {
    @string src = srcᶜ28;

    @string got = ""u8;
    ref var conf = ref heap<types.Config>(new Config(
        GoVersion: "go1.21", // #68334 requires GoVersion <= 1.21
        Error: (error err) => {
            got += err.Error() + "\n"u8;
        }                    // #68334 requires Error != nil
    ), out var Ꮡconf);
    typecheck(src, Ꮡconf, nil); // do not crash

    @string want = "p:5:20: cannot range over x (variable of type int): requires go1.22 or later\n"u8 + "p:9:19: cannot range over x (variable of type int): requires go1.22 or later\n"u8;
    if (got != want) {
        t.Errorf("got: %s want: %s", got, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ29 => ref srcᶜ29ᵇ;
private static readonly @string srcᶜ29ᵇ = """

package p

type (
	S struct{}
	A = S
	T A
)
"""u8.Raw();

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ4 => ref wantᶜ4ᵇ;
private static readonly @string wantᶜ4ᵇ = "type p.T struct{}"u8;

public static void TestIssue68877(ж<testing.T> t) {
    @string src = srcᶜ29;

    t.Setenv(godebugˢ, gotypesalias1ˢ);
    var pkg = mustTypecheck(src, nil, nil);
    var T = pkg.Scope().Lookup("T")._<ж<types.TypeName>>();
    @string got = T.String(); // this must not panic (was issue)
    @string want = wantᶜ4;
    if (got != want) {
        t.Errorf("got %s, want %s", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noTypeFoundForXˢ = (@string)"no type found for {x}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ30 => ref srcᶜ30ᵇ;
private static readonly @string srcᶜ30ᵇ = """

package p

var _ = T{{x}}

"""u8.Raw();

public static void TestIssue69092(ж<testing.T> t) {
    @string src = srcᶜ30;

    var fset = token.NewFileSet();
    var @file = mustParse(fset, src);
    ref var conf = ref heap<types.Config>(new Config(Error: (error err) => {
    }), out var Ꮡconf); // ignore errors
    ref var info = ref heap<typesꓸInfo>(new typesꓸInfo(Types: new map<ast.Expr, types.TypeAndValue>()), out var Ꮡinfo);
    Ꮡconf.Check("p", fset, [@file], Ꮡinfo);

    // look for {x} expression
    var outer = @file.Decls[0]._<ж<ast.GenDecl>>().Specs[0]._<ж<ast.ValueSpec>>().Values[0]._<ж<ast.CompositeLit>>(); // T{{x}}
    var inner = outer.Elts[0];                                                                                        // {x}

    // type of {x} must have been recorded
    var (tv, ok) = info.Types[inner, ꟷ];
    if (!ok) {
        t.Fatal(noTypeFoundForXˢ);
    }
    if (!AreEqual(tv.Type, Typ[Invalid])) {
        t.Fatalf("unexpected type for {x}: %s", tv.Type);
    }
}

} // end types_test_package
