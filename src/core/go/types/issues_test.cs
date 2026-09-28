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
using constant = global::go.go.constant_package;
using global::go.@internal;
using global::go.go;
using io = io_package;
using static global::go.go.types_internal_test_package;
using types = global::go.go.types_package;

partial class types_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packagePTypeSStructTˢ = @"package p; type S struct{T}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ1 = "undefined: T"u8;

public static void TestIssue5770(ж<testing.T> Ꮡt) {
    var (_, err) = typecheck(packagePTypeSStructTˢ, nil, nil);
    @string want = wantᶜ1;
    if (err == default! || !strings.Contains(err.Error(), want)) {
        Ꮡt.Errorf("got: %v; want: %s"u8, err, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packagePVarSUintUint88ˢ = """

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
"""u8;

public static void TestIssue5849(ж<testing.T> Ꮡt) {
    @string src = packagePVarSUintUint88ˢ;
    var types = new map<ast.Expr, types.TypeAndValue>();
    mustTypecheck(src, nil, Ꮡ(new typesꓸInfo(Types: types)));
    foreach (var (x, tv) in types) {
        typesꓸType want = default!;
        switch (x.type()) {
        case ж<ast.BasicLit> xΔ1: {
            var exprᴛ1 = (~xΔ1).Value;
            if (exprᴛ1 == @"8"u8) {
                want = new types.BasicжΔType(Typ[Uint8]);
            }
            else if (exprᴛ1 == @"16"u8) {
                want = new types.BasicжΔType(Typ[Uint16]);
            }
            else if (exprᴛ1 == @"32"u8) {
                want = new types.BasicжΔType(Typ[Uint32]);
            }
            else if (exprᴛ1 == @"64"u8) {
                want = new types.BasicжΔType(Typ[Uint]); // because of "+ s", s is of type uint
            }
            else if (exprᴛ1 == @"""foo"""u8) {
                want = new types.BasicжΔType(Typ[ΔString]);
            }

            break;
        }
        case ж<ast.Ident> xΔ1: {
            if ((~xΔ1).Name == "nil"u8) {
                want = new types.BasicжΔType(Typ[UntypedNil]);
            }
            break;
        }}
        if (want != default! && !Identical(tv.Type, want)) {
            Ꮡt.Errorf("got %s; want %s"u8, tv.Type, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packagePFuncFIntDeferFGoˢ = """

package p
func f() int {
	defer f()
	go f()
	return 0
}

"""u8;

public static void TestIssue6413(ж<testing.T> Ꮡt) {
    @string src = packagePFuncFIntDeferFGoˢ;
    var types = new map<ast.Expr, types.TypeAndValue>();
    mustTypecheck(src, nil, Ꮡ(new typesꓸInfo(Types: types)));
    var want = Typ[Int];
    nint n = 0;
    foreach (var (x, tv) in types) {
        {
            var (_, ok) = x._<ж<ast.CallExpr>>(ᐧ); if (ok) {
                if (!AreEqual(tv.Type, want)) {
                    Ꮡt.Errorf("%s: got %s; want %s"u8, fset.Position(x.Pos()), tv.Type, want.OrTypedNil());
                }
                n++;
            }
        }
    }
    if (n != 2) {
        Ꮡt.Errorf("got %d CallExprs; want 2"u8, n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packagePFuncTMResBoolˢ = """

package p
func (T) m() (res bool) { return }
type T struct{} // receiver type after method declaration

"""u8;

public static void TestIssue7245(ж<testing.T> Ꮡt) {
    @string src = packagePFuncTMResBoolˢ;
    var f = mustParse(fset, src);
    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    var defs = new map<ж<ast.Ident>, types.Object>();
    var (_, err) = Ꮡconf.Check((~(~f).Name).Name, fset, new ж<ast.File>[]{f}.slice(), Ꮡ(new typesꓸInfo(Defs: defs)));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var m = (~f).Decls[0]._<ж<ast.FuncDecl>>();
    var res1 = defs[(~m).Name]._<ж<types.Func>>().Signature().Results().At(0);
    var res2 = defs[(~(~(~(~m).Type).Results).List[0]).Names[0]]._<ж<types.Var>>();
    if (res1 != res2) {
        Ꮡt.Errorf("got %s (%p) != %s (%p)"u8, res1.OrTypedNil(), res2.OrTypedNil(), res1.OrTypedNil(), res2.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cannotAssignToWˢ = "cannot assign to w"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ19 = """

package p
func _() {
	const w = 1        // defs w
        x, y := 2, 3       // defs x, y
        w, x, z := 4, 5, 6 // uses w, x, defs z; error: cannot assign to w
        _, _, _ = x, y, z  // uses x, y, z
}

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ2 = """
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
"""u8;

// This tests that uses of existing vars on the LHS of an assignment
// are Uses, not Defs; and also that the (illegal) use of a non-var on
// the LHS of an assignment is a Use nonetheless.
public static void TestIssue7827(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string src = srcᶜ19;
    // We need a specific fileset in this test below for positions.
    // Cannot use typecheck helper.
    var fset = token.NewFileSet();
    var f = mustParse(fset, src);
    @string want = wantᶜ2;
    // don't abort at the first error
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(Error: (error errΔ1) => {
        Ꮡt.Log(errΔ1);
    });
    var defs = new map<ж<ast.Ident>, types.Object>();
    var uses = new map<ж<ast.Ident>, types.Object>();
    var (_, err) = Ꮡconf.Check((~(~f).Name).Name, fset, new ж<ast.File>[]{f}.slice(), Ꮡ(new typesꓸInfo(Defs: defs, Uses: uses)));
    {
        @string s = err.Error(); if (!strings.HasSuffix(s, cannotAssignToWˢ)) {
            Ꮡt.Errorf("Check: unexpected error: %s"u8, s);
        }
    }
    slice<@string> facts = default!;
    foreach (var (id, obj) in defs) {
        if (obj != default!) {
            @string fact = fmt.Sprintf("L%d defs %s"u8, fset.Position(id.Pos()).Line, obj);
            facts = append(facts, fact);
        }
    }
    foreach (var (id, obj) in uses) {
        @string fact = fmt.Sprintf("L%d uses %s"u8, fset.Position(id.Pos()).Line, obj);
        facts = append(facts, fact);
    }
    slices.Sort<slice<@string>, @string>(facts);
    @string got = strings.Join(facts, "\n"u8);
    if (got != want) {
        Ꮡt.Errorf("Unexpected defs/uses\ngot:\n%s\nwant:\n%s"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string src0ˢ = "src0"u8;
internal static readonly @string src1ˢ = "src1"u8;
internal static readonly @string src2ˢ = "src2"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string src0ᶜ = """

package main

import "go/types"

func main() {
	var info types.Info
	for _, obj := range info.Uses {
		_ = obj.Pkg()
	}
}

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string src1ᶜ1 = """

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

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string src2ᶜ1 = """

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

"""u8;

// This tests that the package associated with the types.Object.Pkg method
// is the type's package independent of the order in which the imports are
// listed in the sources src1, src2 below.
// The actual issue is in go/internal/gcimporter which has a corresponding
// test; we leave this test here to verify correct behavior at the go/types
// level.
public static void TestIssue13898(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt));
    @string src0 = src0ᶜ;
    // like src0, but also imports go/importer
    @string src1 = src1ᶜ1;
    // like src1 but with different import order
    // (used to fail with this issue)
    @string src2 = src2ᶜ1;
    void f(@string test, @string src) {
        var info = Ꮡ(new typesꓸInfo(Uses: new map<ж<ast.Ident>, types.Object>()));
        mustTypecheck(src, nil, info);
        ж<types.Package> pkg = default!;
        nint count = 0;
        foreach (var (id, obj) in (~info).Uses) {
            if ((~id).Name == "Pkg"u8) {
                pkg = obj.Pkg();
                count++;
            }
        }
        if (count != 1) {
            Ꮡt.Fatalf("%s: got %d entries named Pkg; want 1"u8, test, count);
        }
        if (pkg.Name() != "types"u8) {
            Ꮡt.Fatalf("%s: got %v; want package types"u8, test, pkg.OrTypedNil());
        }
    }
    f(src0ˢ, src0);
    f(src1ˢ, src1);
    f(src2ˢ, src2);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ20 = @"package p; func f() { var a, b, c, d, e int }"u8;

public static void TestIssue22525(ж<testing.T> Ꮡt) {
    @string src = srcᶜ20;
    @string got = "\n"u8;
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(Error: (error err) => {
        got += err.Error() + "\n"u8;
    });
    typecheck(src, Ꮡconf, nil); // do not crash
    @string want = "\n"u8 + "p:1:27: declared and not used: a\n"u8 + "p:1:30: declared and not used: b\n"u8 + "p:1:33: declared and not used: c\n"u8 + "p:1:36: declared and not used: d\n"u8 + "p:1:39: declared and not used: e\n"u8;
    if (got != want) {
        Ꮡt.Errorf("got: %swant: %s"u8, got, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string prefixᶜ2 = @"package p; import ""unsafe""; type P *struct{}; type I interface{}; type T "u8;

public static void TestIssue25627(ж<testing.T> Ꮡt) {
    @string prefix = prefixᶜ2;
    // The src strings (without prefix) are constructed such that the number of semicolons
    // plus one corresponds to the number of fields expected in the respective struct.
    foreach (var (_, src) in new @string[]{
        @"struct { x Missing }"u8,
        @"struct { Missing }"u8,
        @"struct { *Missing }"u8,
        @"struct { unsafe.Pointer }"u8,
        @"struct { P }"u8,
        @"struct { *I }"u8,
        @"struct { a int; b Missing; *Missing }"u8
    }.slice()) {
        var f = mustParse(fset, prefix + src);
        ref var cfg = ref heap<types.Config>(out var Ꮡcfg);
        cfg = new Config(Importer: defaultImporter(fset), Error: (error errΔ1) => {
        });
        var info = Ꮡ(new typesꓸInfo(Types: new map<ast.Expr, types.TypeAndValue>()));
        var (_, err) = Ꮡcfg.Check((~(~f).Name).Name, fset, new ж<ast.File>[]{f}.slice(), info);
        if (err != default!) {
            {
                var (_, ok) = err._<typesꓸError>(ᐧ); if (!ok) {
                    Ꮡt.Fatal(err);
                }
            }
        }
        var infoʗ1 = info;
        ast.Inspect(new ast.FileжNode(f), (ast.Node n) => {
            {
                var (spec, _) = n._<ж<ast.TypeSpec>>(ᐧ); if (spec != nil) {
                    {
                        var (tv, ok) = (~infoʗ1).Types[(~spec).Type, ꟷ]; if (ok && (~(~spec).Name).Name == "T"u8) {
                            nint want = strings.Count(src, ";"u8) + 1;
                            {
                                nint got = tv.Type._<ж<types.Struct>>().NumFields(); if (got != want) {
                                    Ꮡt.Errorf("%s: got %d fields; want %d"u8, src, got, want);
                                }
                            }
                        }
                    }
                }
            }
            return true;
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object objectXNotFoundˢ = (@string)"object X not found"u8;

public static void TestIssue28005(ж<testing.T> Ꮡt) {
    // method names must match defining interface name for this test
    // (see last comment in this function)
    var sources = new @string[]{
        "package p; type A interface{ A() }"u8,
        "package p; type B interface{ B() }"u8,
        "package p; type X interface{ A; B }"u8
    }.array();
    // compute original file ASTs
    array<ж<ast.File>> orig = new(3); /* len(sources) */
    foreach (var (i, src) in sources.ΔRangeSnapshot()) {
        orig[i] = mustParse(fset, src);
    }
    // run the test for all order permutations of the incoming files
    foreach (var (_, vᴛ1) in GoReflect.WithElemDims(new array<nint>[]{
        new nint[]{0, 1, 2}.array(),
        new nint[]{0, 2, 1}.array(),
        new nint[]{1, 0, 2}.array(),
        new nint[]{1, 2, 0}.array(),
        new nint[]{2, 0, 1}.array(),
        new nint[]{2, 1, 0}.array()
    }.slice(), 3)) {
        var perm = vᴛ1.Clone();

        // create file order permutation
        var files = new slice<ж<ast.File>>(len(sources));
        foreach (var (i, _) in perm) {
            files[i] = orig[perm[i]];
        }
        // type-check package with given file order permutation
        ref var conf = ref heap(new types.Config(), out var Ꮡconf);
        var info = Ꮡ(new typesꓸInfo(Defs: new map<ж<ast.Ident>, types.Object>()));
        var (_, err) = Ꮡconf.Check(""u8, fset, files, info);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        // look for interface object X
        types.Object obj = default!;
        foreach (var (name, def) in (~info).Defs) {
            if ((~name).Name == "X"u8) {
                obj = def;
                break;
            }
        }
        if (obj == default!) {
            Ꮡt.Fatal(objectXNotFoundˢ);
        }
        var iface = obj.Type().Underlying()._<ж<types.Interface>>(); // object X must be an interface
        // Each iface method m is embedded; and m's receiver base type name
        // must match the method's name per the choice in the source file.
        for (nint i = 0; i < iface.NumMethods(); i++) {
            var m = iface.Method(i);
            @string recvName = m.Signature().Recv().Type()._<ж<types.Named>>().Obj().Name();
            if (recvName != m.Name()) {
                Ꮡt.Errorf("perm %v: got recv %s; want %s"u8, perm, recvName, m.Name());
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string errorˢ3 = "error"u8;
internal static readonly @string errorˢ4 = "Error"u8;

public static void TestIssue28282(ж<testing.T> Ꮡt) {
    // create type interface { error }
    var et = Universe.Lookup(errorˢ3).Type();
    var it = NewInterfaceType(default!, new typesꓸType[]{et}.slice());
    it.Complete();
    // verify that after completing the interface, the embedded method remains unchanged
    var want = et.Underlying()._<ж<types.Interface>>().Method(0);
    var got = it.Method(0);
    if (got != want) {
        Ꮡt.Fatalf("%s.Method(0): got %q (%p); want %q (%p)"u8, it.OrTypedNil(), got.OrTypedNil(), got.OrTypedNil(), want.OrTypedNil(), want.OrTypedNil());
    }
    // verify that lookup finds the same method in both interfaces (redundant check)
    var (obj, _, _) = LookupFieldOrMethod(et, false, nil, errorˢ4);
    if (!AreEqual(obj, want)) {
        Ꮡt.Fatalf("%s.Lookup: got %q (%p); want %q (%p)"u8, et, obj, obj, want.OrTypedNil(), want.OrTypedNil());
    }
    (obj, _, _) = LookupFieldOrMethod(new types.InterfaceжΔType(it), false, nil, errorˢ4);
    if (!AreEqual(obj, want)) {
        Ꮡt.Fatalf("%s.Lookup: got %q (%p); want %q (%p)"u8, it.OrTypedNil(), obj, obj, want.OrTypedNil(), want.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string packagePTypeAInterfaceMˢ = @"package p; type A interface { M() }"u8;
internal static readonly @string packagePVarBInterfaceAˢ = @"package p; var B interface { A }"u8;

public static void TestIssue29029(ж<testing.T> Ꮡt) {
    var f1 = mustParse(fset, packagePTypeAInterfaceMˢ);
    var f2 = mustParse(fset, packagePVarBInterfaceAˢ);
    // printInfo prints the *Func definitions recorded in info, one *Func per line.
    @string printInfo(ж<typesꓸInfo> infoΔ1) {
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        foreach (var (_, obj) in (~infoΔ1).Defs) {
            {
                var (fn, ok) = obj._<ж<types.Func>>(ᐧ); if (ok) {
                    fmt.Fprintln(new types_test_package.strings_BuilderжWriter(Ꮡbuf), fn.OrTypedNil());
                }
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
    var check = NewChecker(Ꮡconf, fset, NewPackage(""u8, "p"u8), info);
    {
        var err = check.Files(new ж<ast.File>[]{f1, f2}.slice()); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    @string want = printInfo(info);
    // type-check incrementally
    info = Ꮡ(new typesꓸInfo(Defs: new map<ж<ast.Ident>, types.Object>()));
    check = NewChecker(Ꮡconf, fset, NewPackage(""u8, "p"u8), info);
    {
        var err = check.Files(new ж<ast.File>[]{f1}.slice()); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = check.Files(new ж<ast.File>[]{f2}.slice()); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    @string got = printInfo(info);
    if (got != want) {
        Ꮡt.Errorf("\ngot : %swant: %s"u8, got, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string asrcᶜ = @"package a; type I interface{ M() }; type T struct { F interface { I } }"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string bsrcᶜ = @"package b; import ""a""; type T struct { F interface { a.I } }; var _ = a.T(T{})"u8;

public static void TestIssue34151(ж<testing.T> Ꮡt) {
    @string asrc = asrcᶜ;
    @string bsrc = bsrcᶜ;
    var a = mustTypecheck(asrc, nil, nil);
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(Importer: new importHelper(pkg: a));
    mustTypecheck(bsrc, Ꮡconf, nil);
}

[GoType] partial struct importHelper {
    internal ж<types.Package> pkg;
    internal types.Importer fallback;
}

internal static (ж<types.Package>, error) Import(this importHelper h, @string path) {
    if (path == h.pkg.Path()) {
        return (h.pkg, default!);
    }
    if (h.fallback == default!) {
        return (default!, fmt.Errorf("got package path %q; want %q"u8, path, h.pkg.Path()));
    }
    return h.fallback.Import(path);
}

// TestIssue34921 verifies that we don't update an imported type's underlying
// type when resolving an underlying type. Specifically, when determining the
// underlying type of b.T (which is the underlying type of a.T, which is int)
// we must not set the underlying type of a.T again since that would lead to
// a race condition if package b is imported elsewhere, in a package that is
// concurrently type-checked.
public static void TestIssue34921(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    Ꮡt.Error(r);
                }
            }
        }, ref ᒐ);
        slice<@string> sources = new @string[]{
            @"package a; type T int"u8,
            @"package b; import ""a""; type T a.T"u8
        }.slice();
        ж<types.Package> pkg = default!;
        foreach (var (_, src) in sources) {
            ref var conf = ref heap<types.Config>(out var Ꮡconf);
            conf = new Config(Importer: new importHelper(pkg: pkg));
            pkg = mustTypecheck(src, Ꮡconf, nil); // pkg imported by the next package in this test
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIssue43088(ж<testing.T> Ꮡt) {
    // type T1 struct {
    //         _ T2
    // }
    //
    // type T2 struct {
    //         _ struct {
    //                 _ T2
    //         }
    // }
    var n1 = NewTypeName(nopos, nil, "T1"u8, default!);
    var T1 = NewNamed(n1, default!, default!);
    var n2 = NewTypeName(nopos, nil, "T2"u8, default!);
    var T2 = NewNamed(n2, default!, default!);
    var s1 = NewStruct(new ж<types.Var>[]{NewField(nopos, nil, "_"u8, new types.NamedжΔType(T2), false)}.slice(), default!);
    T1.SetUnderlying(new types.StructжΔType(s1));
    var s2 = NewStruct(new ж<types.Var>[]{NewField(nopos, nil, "_"u8, new types.NamedжΔType(T2), false)}.slice(), default!);
    var s3 = NewStruct(new ж<types.Var>[]{NewField(nopos, nil, "_"u8, new types.StructжΔType(s2), false)}.slice(), default!);
    T2.SetUnderlying(new types.StructжΔType(s3));
    // These calls must terminate (no endless recursion).
    Comparable(new types.NamedжΔType(T1));
    Comparable(new types.NamedжΔType(T2));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string pointerˢ = "Pointer"u8;
internal static readonly @string unsafePointerˢ = "unsafe.Pointer"u8;
internal static readonly @string fooˢ = "foo"u8;
internal static readonly @string fooPointerˢ = "foo.Pointer"u8;

public static void TestIssue44515(ж<testing.T> Ꮡt) {
    var typ = Unsafe.Scope().Lookup(pointerˢ).Type();
    @string got = TypeString(typ, default!);
    @string want = unsafePointerˢ;
    if (got != want) {
        Ꮡt.Errorf("got %q; want %q"u8, got, want);
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
        Ꮡt.Errorf("got %q; want %q"u8, got, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string asrcᶜ1 = @"package a; import ""text/template""; func F(template.Template) {}; func G(int) {}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string bsrcᶜ1 = """

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

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string csrcᶜ = """

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

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string tsrcᶜ = """

package template

import "text/template"

type T int

// Verify that the current package name also causes disambiguation.
var _ T = template /* ERRORx "cannot use.*text/template.* as T value" */.Template{}

"""u8;

public static void TestIssue43124(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // TODO(rFindley) move this to testdata by enhancing support for importing.
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt)); // The go command is needed for the importer to determine the locations of stdlib .a files.
    // All involved packages have the same name (template). Error messages should
    // disambiguate between text/template and html/template by printing the full
    // path.
    @string asrc = asrcᶜ1;
    
    @string bsrc = bsrcᶜ1;
    
    @string csrc = csrcᶜ;
    
    @string tsrc = tsrcᶜ;
    var a = mustTypecheck(asrc, nil, nil);
    ref var imp = ref heap<importHelper>(out var Ꮡimp);
    imp = new importHelper(
        pkg: a, // TODO(adonovan): use same FileSet as mustTypecheck.

        fallback: defaultImporter(token.NewFileSet())
    );
    var impʗ1 = imp;
    var withImporter = (ж<types.Config> cfg) => {
        cfg.Value.Importer = impʗ1;
    };
    testFiles(Ꮡt, new @string[]{"b.go"u8}.slice(), new slice<byte>[]{slice<byte>(bsrc)}.slice(), false, withImporter);
    testFiles(Ꮡt, new @string[]{"c.go"u8}.slice(), new slice<byte>[]{slice<byte>(csrc)}.slice(), false, withImporter);
    testFiles(Ꮡt, new @string[]{"t.go"u8}.slice(), new slice<byte>[]{slice<byte>(tsrc)}.slice(), false, withImporter);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string comparableˢ = "comparable"u8;
internal static readonly object anyIsNotAComparableTypeˢ = (@string)"any is not a comparable type"u8;
internal static readonly object comparableIsNotAˢ = (@string)"comparable is not a comparable type"u8;
internal static readonly object anyImplementsComparableˢ = (@string)"any implements comparable"u8;
internal static readonly object comparableDoesNotˢ = (@string)"comparable does not implement any"u8;
internal static readonly object anyAssignableToˢ = (@string)"any assignable to comparable"u8;
internal static readonly object comparableNotAssignableˢ = (@string)"comparable not assignable to any"u8;

public static void TestIssue50646(ж<testing.T> Ꮡt) {
    var anyType = Universe.Lookup(anyˢ).Type().Underlying();
    var comparableType = Universe.Lookup(comparableˢ).Type();
    if (!Comparable(anyType)) {
        Ꮡt.Error(anyIsNotAComparableTypeˢ);
    }
    if (!Comparable(comparableType)) {
        Ꮡt.Error(comparableIsNotAˢ);
    }
    if (Implements(anyType, comparableType.Underlying()._<ж<types.Interface>>())) {
        Ꮡt.Error(anyImplementsComparableˢ);
    }
    if (!Implements(comparableType, anyType._<ж<types.Interface>>())) {
        Ꮡt.Error(comparableDoesNotˢ);
    }
    if (AssignableTo(anyType, comparableType)) {
        Ꮡt.Error(anyAssignableToˢ);
    }
    if (!AssignableTo(comparableType, anyType)) {
        Ꮡt.Error(comparableNotAssignableˢ);
    }
}

public static void TestIssue55030(ж<testing.T> Ꮡt) {
    // makeSig makes the signature func(typ...)
    void makeSig(typesꓸType typ) {
        var par = NewVar(nopos, nil, ""u8, typ);
        var @params = NewTuple(par);
        NewSignatureType(nil, default!, default!, @params, nil, true);
    }
    // makeSig must not panic for the following (example) types:
    // []int
    makeSig(new types.SliceжΔType(NewSlice(new types.BasicжΔType(Typ[Int]))));
    // string
    makeSig(new types.BasicжΔType(Typ[ΔString]));
    // P where P's core type is string
    {
        var P = NewTypeName(nopos, nil, "P"u8, default!); // [P string]
        makeSig(new types.TypeParamжΔType(NewTypeParam(P, new types.InterfaceжΔType(NewInterfaceType(default!, new typesꓸType[]{new types.BasicжΔType(Typ[ΔString])}.slice())))));
    }
    // P where P's core type is an (unnamed) slice
    {
        var P = NewTypeName(nopos, nil, "P"u8, default!); // [P []int]
        makeSig(new types.TypeParamжΔType(NewTypeParam(P, new types.InterfaceжΔType(NewInterfaceType(default!, new typesꓸType[]{new types.SliceжΔType(NewSlice(new types.BasicжΔType(Typ[Int])))}.slice())))));
    }
    // P where P's core type is bytestring (i.e., string or []byte)
    {
        var t1 = NewTerm(true, new types.BasicжΔType(Typ[ΔString])); // ~string
        var t2 = NewTerm(false, new types.SliceжΔType(NewSlice(new types.BasicжΔType(Typ[Byte])))); // []byte
        var u = NewUnion(new ж<typesꓸTerm>[]{t1, t2}.slice()); // ~string | []byte
        var P = NewTypeName(nopos, nil, "P"u8, default!); // [P ~string | []byte]
        makeSig(new types.TypeParamжΔType(NewTypeParam(P, new types.InterfaceжΔType(NewInterfaceType(default!, new typesꓸType[]{new types.UnionжΔType(u)}.slice())))));
    }
}

[GoType("dyn")] internal partial struct TestIssue51093_type {
    internal @string typ;
    internal @string val;
}

public static void TestIssue51093(ж<testing.T> Ꮡt) {
// some more complex constraints
    // Each test stands for a conversion of the form P(val)
    // where P is a type parameter with typ as constraint.
    // The test ensures that P(val) has the correct type P
    // and is not a constant.
    slice<TestIssue51093_type> tests = new TestIssue51093_type[]{
        new("bool"u8, "false"u8),
        new("int"u8, "-1"u8),
        new("uint"u8, "1.0"u8),
        new("rune"u8, "'a'"u8),
        new("float64"u8, "3.5"u8),
        new("complex64"u8, "1.25"u8),
        new("string"u8, "\"foo\""u8),
        new("~byte"u8, "1"u8),
        new("~int | ~float64 | complex128"u8, "1"u8),
        new("~uint64 | ~rune"u8, "'X'"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        @string src = fmt.Sprintf("package p; func _[P %s]() { _ = P(%s) }"u8, test.typ, test.val);
        var types = new map<ast.Expr, types.TypeAndValue>();
        mustTypecheck(src, nil, Ꮡ(new typesꓸInfo(Types: types)));
        nint n = default!;
        foreach (var (x, tv) in types) {
            {
                var (xΔ1, _) = x._<ж<ast.CallExpr>>(ᐧ); if (xΔ1 != nil) {
                    // there must be exactly one CallExpr which is the P(val) conversion
                    n++;
                    var (tpar, _) = tv.Type._<ж<types.TypeParam>>(ᐧ);
                    if (tpar == nil) {
                        Ꮡt.Fatalf("%s: got type %s, want type parameter"u8, ExprString(new ast.CallExprжExpr(xΔ1)), tv.Type);
                    }
                    {
                        @string name = tpar.Obj().Name(); if (name != "P"u8) {
                            Ꮡt.Fatalf("%s: got type parameter name %s, want P"u8, ExprString(new ast.CallExprжExpr(xΔ1)), name);
                        }
                    }
                    // P(val) must not be constant
                    if (tv.Value != default!) {
                        Ꮡt.Errorf("%s: got constant value %s (%s), want no constant"u8, ExprString(new ast.CallExprжExpr(xΔ1)), tv.Value, tv.Value.String());
                    }
                }
            }
        }
        if (n != 1) {
            Ꮡt.Fatalf("%s: got %d CallExpr nodes; want 1"u8, src, (nint)(1));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedFailureButItDidˢ = (@string)"Expected failure, but it did not"u8;

[GoType("dyn")] internal partial struct TestIssue54258_tests {
    internal @string main, b, want;
}

public static void TestIssue54258(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestIssue54258_tests[]{
        new(
            """
package main
import "b"
type I0 interface {
	M0(w struct{ f string })
}
var _ I0 = b.S{}

"""u8, //---------------------------------------------------------------

            """
package b
type S struct{}
func (S) M0(struct{ f string }) {}

"""u8,
            """
6:12: cannot use b[.]S{} [(]value of struct type b[.]S[)] as I0 value in variable declaration: b[.]S does not implement I0 [(]wrong type for method M0[)]
.*have M0[(]struct{f string /[*] package b [*]/ }[)]
.*want M0[(]struct{f string /[*] package main [*]/ }[)]
"""u8),
        new(
            """
package main
import "b"
type I1 interface {
	M1(struct{ string })
}
var _ I1 = b.S{}

"""u8, //---------------------------------------------------------------

            """
package b
type S struct{}
func (S) M1(struct{ string }) {}

"""u8,
            """
6:12: cannot use b[.]S{} [(]value of struct type b[.]S[)] as I1 value in variable declaration: b[.]S does not implement I1 [(]wrong type for method M1[)]
.*have M1[(]struct{string /[*] package b [*]/ }[)]
.*want M1[(]struct{string /[*] package main [*]/ }[)]
"""u8),
        new(
            """
package main
import "b"
type I2 interface {
	M2(y struct{ f struct{ f string } })
}
var _ I2 = b.S{}

"""u8, //---------------------------------------------------------------

            """
package b
type S struct{}
func (S) M2(struct{ f struct{ f string } }) {}

"""u8,
            """
6:12: cannot use b[.]S{} [(]value of struct type b[.]S[)] as I2 value in variable declaration: b[.]S does not implement I2 [(]wrong type for method M2[)]
.*have M2[(]struct{f struct{f string} /[*] package b [*]/ }[)]
.*want M2[(]struct{f struct{f string} /[*] package main [*]/ }[)]
"""u8),
        new(
            """
package main
import "b"
type I3 interface {
	M3(z struct{ F struct{ f string } })
}
var _ I3 = b.S{}

"""u8, //---------------------------------------------------------------

            """
package b
type S struct{}
func (S) M3(struct{ F struct{ f string } }) {}

"""u8,
            """
6:12: cannot use b[.]S{} [(]value of struct type b[.]S[)] as I3 value in variable declaration: b[.]S does not implement I3 [(]wrong type for method M3[)]
.*have M3[(]struct{F struct{f string /[*] package b [*]/ }}[)]
.*want M3[(]struct{F struct{f string /[*] package main [*]/ }}[)]
"""u8),
        new(
            """
package main
import "b"
type I4 interface {
	M4(_ struct { *string })
}
var _ I4 = b.S{}

"""u8, //---------------------------------------------------------------

            """
package b
type S struct{}
func (S) M4(struct { *string }) {}

"""u8,
            """
6:12: cannot use b[.]S{} [(]value of struct type b[.]S[)] as I4 value in variable declaration: b[.]S does not implement I4 [(]wrong type for method M4[)]
.*have M4[(]struct{[*]string /[*] package b [*]/ }[)]
.*want M4[(]struct{[*]string /[*] package main [*]/ }[)]
"""u8),
        new(
            """
package main
import "b"
type t struct{ A int }
type I5 interface {
	M5(_ struct {b.S;t})
}
var _ I5 = b.S{}

"""u8, //---------------------------------------------------------------

            """
package b
type S struct{}
type t struct{ A int }
func (S) M5(struct {S;t}) {}

"""u8,
            """
7:12: cannot use b[.]S{} [(]value of struct type b[.]S[)] as I5 value in variable declaration: b[.]S does not implement I5 [(]wrong type for method M5[)]
.*have M5[(]struct{b[.]S; b[.]t}[)]
.*want M5[(]struct{b[.]S; t}[)]
"""u8)
    }.slice();
    var fset = token.NewFileSet();
    var fsetʗ1 = fset;
    void test(@string main, @string b, @string want) {
        var re = regexp.MustCompile(want);
        var bpkg = mustTypecheck(b, nil, nil);
        var mast = mustParse(fsetʗ1, main);
        ref var conf = ref heap<types.Config>(out var Ꮡconf);
        conf = new Config(Importer: new importHelper(pkg: bpkg));
        var (_, err) = Ꮡconf.Check((~(~mast).Name).Name, fsetʗ1, new ж<ast.File>[]{mast}.slice(), nil);
        if (err == default!){
            Ꮡt.Error(expectedFailureButItDidˢ);
        } else 
        {
            @string got = err.Error(); if (!re.MatchString(got)){
                Ꮡt.Errorf("Wanted match for\n\t%s\n but got\n\t%s"u8, want, got);
            } else 
            if (testing.Verbose()) {
                Ꮡt.Logf("Saw expected\n\t%s"u8, err.Error());
            }
        }
    }
    foreach (var (_, tΔ1) in tests) {
        test(tΔ1.main, tΔ1.b, tΔ1.want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string go115UsesCgoˢ = "go115UsesCgo"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ21 = """
// -gotypesalias=1

package p

/*
struct layout {};
*/
import "C"

type Layout = C.struct_layout

func (*Layout /* ERROR "cannot define new methods on non-local type Layout" */) Binding() {}

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string cgoTypesᶜ = """

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

"""u8;

public static void TestIssue59944(ж<testing.T> Ꮡt) {
    testenv.MustHaveCGO(new types_test_package.testing_TжTB(Ꮡt));
    // Methods declared on aliases of cgo types are not permitted.
    @string src = srcᶜ21;
    // code generated by cmd/cgo for the above source.
    @string cgoTypes = cgoTypesᶜ;
    testFiles(Ꮡt, new @string[]{"p.go"u8, "_cgo_gotypes.go"u8}.slice(), new slice<byte>[]{slice<byte>(src), slice<byte>(cgoTypes)}.slice(), false, (ж<types.Config> cfg) => {
        boolFieldAddr(cfg, go115UsesCgoˢ).Value = true;
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedSyntaxErrorˢ = (@string)"expected syntax error"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ22 = """

package p

func A(func(any), ...any) {}
func B[T any](T)          {}

func _() {
	A(B, nil // syntax error: missing ',' before newline in argument list
}

"""u8;

public static void TestIssue61931(ж<testing.T> Ꮡt) {
    @string src = srcᶜ22;
    var fset = token.NewFileSet();
    var (f, err) = parser.ParseFile(fset, pkgName(src), src, 0);
    if (err == default!) {
        Ꮡt.Fatal(expectedSyntaxErrorˢ);
    }
    ref var conf = ref heap(new types.Config(), out var Ꮡconf);
    Ꮡconf.Check((~(~f).Name).Name, fset, new ж<ast.File>[]{f}.slice(), nil); // must not panic
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ23 = """

package p

func f[T any]() {}
func _()        { f() }

"""u8;

public static void TestIssue61938(ж<testing.T> Ꮡt) {
    @string src = srcᶜ23;
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
internal static readonly @string srcᶜ24 = """

package p

func _() {
        use(f[*string])
}

func use(func()) {}

func f[I *T, T any]() {
        var v T
        _ = v
}
"""u8;

public static void TestIssue63260(ж<testing.T> Ꮡt) {
    @string src = srcᶜ24;
    ref var info = ref heap<typesꓸInfo>(out var Ꮡinfo);
    info = new typesꓸInfo(
        Defs: new map<ж<ast.Ident>, types.Object>()
    );
    var pkg = mustTypecheck(src, nil, Ꮡinfo);
    // get type parameter T in signature of f
    var T = pkg.Scope().Lookup("f"u8).Type()._<ж<typesꓸSignature>>().TypeParams().At(1);
    if (T.Obj().Name() != "T"u8) {
        Ꮡt.Fatalf("got type parameter %s, want T"u8, T.OrTypedNil());
    }
    // get type of variable v in body of f
    types.Object v = default!;
    foreach (var (name, obj) in info.Defs) {
        if ((~name).Name == "v"u8) {
            v = obj;
            break;
        }
    }
    if (v == default!) {
        Ꮡt.Fatal(variableVNotFoundˢ);
    }
    // type of v and T must be pointer-identical
    if (!AreEqual(v.Type(), T)) {
        Ꮡt.Fatalf("types of v and T are not pointer-identical: %p != %p"u8, v.Type()._<ж<types.TypeParam>>().OrTypedNil(), T.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object objectSNotFoundˢ = (@string)"object S not found"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ25 = """

package p

type A = []int
type S struct{ A }

"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ3 = "type p.S struct{p.A}"u8;

public static void TestIssue44410(ж<testing.T> Ꮡt) {
    @string src = srcᶜ25;
    Ꮡt.Setenv(godebugˢ, gotypesalias1ˢ);
    var pkg = mustTypecheck(src, nil, nil);
    var S = pkg.Scope().Lookup("S"u8);
    if (S == default!) {
        Ꮡt.Fatal(objectSNotFoundˢ);
    }
    @string got = S.String();
    @string want = wantᶜ3;
    if (got != want) {
        Ꮡt.Fatalf("got %q; want %q"u8, got, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string asrcᶜ2 = @"package a; type S struct{}; func (S) m() {}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string bsrcᶜ2 = @"package b; type S struct{}; func (S) M() {}"u8;

[GoType("dyn")] internal partial struct TestIssue59831_tests {
    internal ж<types.Package> imported;
    internal @string src, err;
}

public static void TestIssue59831(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Package a exports a type S with an unexported method m;
    // the tests check the error messages when m is not found.
    @string asrc = asrcᶜ2;
    var apkg = mustTypecheck(asrc, nil, nil);
    // Package b exports a type S with an exported method m;
    // the tests check the error messages when M is not found.
    @string bsrc = bsrcᶜ2;
    var bpkg = mustTypecheck(bsrc, nil, nil);
    var tests = new TestIssue59831_tests[]{ // tests importing a (or nothing)

        new(apkg, @"package a1; import ""a""; var _ interface { M() } = a.S{}"u8,
            "a.S does not implement interface{M()} (missing method M) have m() want M()"u8),
        new(apkg, @"package a2; import ""a""; var _ interface { m() } = a.S{}"u8,
            "a.S does not implement interface{m()} (unexported method m)"u8), // test for issue

        new(nil, @"package a3; type S struct{}; func (S) m(); var _ interface { M() } = S{}"u8,
            "S does not implement interface{M()} (missing method M) have m() want M()"u8),
        new(nil, @"package a4; type S struct{}; func (S) m(); var _ interface { m() } = S{}"u8,
            ""u8), // no error expected

        new(nil, @"package a5; type S struct{}; func (S) m(); var _ interface { n() } = S{}"u8,
            "S does not implement interface{n()} (missing method n)"u8), // tests importing b (or nothing)

        new(bpkg, @"package b1; import ""b""; var _ interface { m() } = b.S{}"u8,
            "b.S does not implement interface{m()} (missing method m) have M() want m()"u8),
        new(bpkg, @"package b2; import ""b""; var _ interface { M() } = b.S{}"u8,
            ""u8), // no error expected

        new(nil, @"package b3; type S struct{}; func (S) M(); var _ interface { M() } = S{}"u8,
            ""u8), // no error expected

        new(nil, @"package b4; type S struct{}; func (S) M(); var _ interface { m() } = S{}"u8,
            "S does not implement interface{m()} (missing method m) have M() want m()"u8),
        new(nil, @"package b5; type S struct{}; func (S) M(); var _ interface { n() } = S{}"u8,
            "S does not implement interface{n()} (missing method n)"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        // typecheck test source
        ref var conf = ref heap<types.Config>(out var Ꮡconf);
        conf = new Config(Importer: new importHelper(pkg: test.imported));
        var (pkg, err) = typecheck(test.src, Ꮡconf, nil);
        if (err == default!) {
            if (test.err != ""u8) {
                Ꮡt.Errorf("package %s: got no error, want %q"u8, pkg.Name(), test.err);
            }
            continue;
        }
        if (test.err == ""u8) {
            Ꮡt.Errorf("package %s: got %q, want not error"u8, pkg.Name(), err.Error());
        }
        // flatten reported error message
        @string errmsg = strings.ReplaceAll(err.Error(), "\n"u8, " "u8);
        errmsg = strings.ReplaceAll(errmsg, "\t"u8, ""u8);
        // verify error message
        if (!strings.Contains(errmsg, test.err)) {
            Ꮡt.Errorf("package %s: got %q, want %q"u8, pkg.Name(), errmsg, test.err);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ26 = """

//go:build go1.18
package p

func f[S ~[]E, E any](S) {}

func _() {
	f([]string{})
}

"""u8;

public static void TestIssue64759(ж<testing.T> Ꮡt) {
    @string src = srcᶜ26;
    // Per the go:build directive, the source must typecheck
    // even though the (module) Go version is set to go1.17.
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(GoVersion: "go1.17"u8);
    mustTypecheck(src, Ꮡconf, nil);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ27 = """

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

"""u8;

public static void TestIssue68334(ж<testing.T> Ꮡt) {
    @string src = srcᶜ27;
    @string got = ""u8;
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(
        GoVersion: "go1.21"u8, // #68334 requires GoVersion <= 1.21

        Error: (error err) => {
            got += err.Error() + "\n"u8; // #68334 requires Error != nil
        }
    );
    typecheck(src, Ꮡconf, nil); // do not crash
    @string want = "p:5:20: cannot range over x (variable of type int): requires go1.22 or later\n"u8 + "p:9:19: cannot range over x (variable of type int): requires go1.22 or later\n"u8;
    if (got != want) {
        Ꮡt.Errorf("got: %s want: %s"u8, got, want);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ28 = """

package p

type (
	S struct{}
	A = S
	T A
)
"""u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ4 = "type p.T struct{}"u8;

public static void TestIssue68877(ж<testing.T> Ꮡt) {
    @string src = srcᶜ28;
    Ꮡt.Setenv(godebugˢ, gotypesalias1ˢ);
    var pkg = mustTypecheck(src, nil, nil);
    var T = pkg.Scope().Lookup("T"u8)._<ж<types.TypeName>>();
    @string got = T.String(); // this must not panic (was issue)
    @string want = wantᶜ4;
    if (got != want) {
        Ꮡt.Errorf("got %s, want %s"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noTypeFoundForXˢ = (@string)"no type found for {x}"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ29 = """

package p

var _ = T{{x}}

"""u8;

public static void TestIssue69092(ж<testing.T> Ꮡt) {
    @string src = srcᶜ29;
    var fset = token.NewFileSet();
    var @file = mustParse(fset, src);
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new Config(Error: (error err) => {
    }); // ignore errors
    ref var info = ref heap<typesꓸInfo>(out var Ꮡinfo);
    info = new typesꓸInfo(Types: new map<ast.Expr, types.TypeAndValue>());
    Ꮡconf.Check("p"u8, fset, new ж<ast.File>[]{@file}.slice(), Ꮡinfo);
    // look for {x} expression
    var outer = (~(~(~@file).Decls[0]._<ж<ast.GenDecl>>()).Specs[0]._<ж<ast.ValueSpec>>()).Values[0]._<ж<ast.CompositeLit>>(); // T{{x}}
    var inner = (~outer).Elts[0]; // {x}
    // type of {x} must have been recorded
    var (tv, ok) = info.Types[inner, ꟷ];
    if (!ok) {
        Ꮡt.Fatal(noTypeFoundForXˢ);
    }
    if (!AreEqual(tv.Type, Typ[Invalid])) {
        Ꮡt.Fatalf("unexpected type for {x}: %s"u8, tv.Type);
    }
}

} // end types_test_package
