// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains tests for sizes.
namespace go.go;

using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using types = global::go.go.types_package;
using testenv = global::go.@internal.testenv_package;
using testing = testing_package;
using global::go.@internal;
using global::go.go;
using static global::go.go.types_internal_test_package;

partial class types_test_package {

// findStructType typechecks src and returns the first struct type encountered.
internal static ж<types.Struct> findStructType(ж<testing.T> t, @string src) {
    return findStructTypeConfig(t, src, Ꮡ(new types.Config()));
}

internal static ж<types.Struct> findStructTypeConfig(ж<testing.T> t, @string src, ж<types.Config> conf) {
    var types_ = new map<ast.Expr, types.TypeAndValue>();
    mustTypecheck(src, nil, Ꮡ(new typesꓸInfo(Types: types_)));
    foreach (var (_, tv) in types_) {
        {
            var (ts, ok) = tv.Type._<ж<types.Struct>>(ᐧ); if (ok) {
                return ts;
            }
        }
    }
    t.Fatalf("failed to find a struct type in src:\n%s\n", src);
    return default;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ37 => ref srcᶜ37ᵇ;
private static readonly @string srcᶜ37ᵇ = """

package main

type S struct {
    i int
    b bool
    s string
    n int
}

"""u8.Raw();

// go.dev/issue/16316
public static void TestMultipleSizeUse(ж<testing.T> t) {
    @string src = srcᶜ37;
    var ts = findStructType(t, src);
    ref var sizes = ref heap<types.StdSizes>(new types.StdSizes(WordSize: 4, MaxAlign: 4), out var Ꮡsizes);
    {
        var got = Ꮡsizes.Sizeof(new types.StructжΔType(ts)); if (got != 20) {
            t.Errorf("Sizeof(%v) with WordSize 4 = %d want 20", ts.OrTypedNil(), got);
        }
    }
    sizes = new types.StdSizes(WordSize: 8, MaxAlign: 8);
    {
        var got = Ꮡsizes.Sizeof(new types.StructжΔType(ts)); if (got != 40) {
            t.Errorf("Sizeof(%v) with WordSize 8 = %d want 40", ts.OrTypedNil(), got);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ38 => ref srcᶜ38ᵇ;
private static readonly @string srcᶜ38ᵇ = """

package main

var s struct {
	x *int
	y []byte
}

"""u8.Raw();

// go.dev/issue/16464
public static void TestAlignofNaclSlice(ж<testing.T> t) {
    var fieldsᴷ = new stackAppend<ж<types.Var>>();

    @string src = srcᶜ38;
    var ts = findStructType(t, src);
    var sizes = Ꮡ(new types.StdSizes(WordSize: 4, MaxAlign: 8));
    slice<ж<types.Var>> fields = default;
    // Make a copy manually :(
    for (nint i = 0; i < ts.NumFields(); i++) {
        fields = append(fieldsᴷ, fields, ts.Field(i));
    }
    var offsets = sizes.Offsetsof(fields);
    if (offsets[0] != 0 || offsets[1] != 4) {
        t.Errorf("OffsetsOf(%v) = %v want %v", ts.OrTypedNil(), offsets, slice<nint>([0, 4]));
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ39 => ref srcᶜ39ᵇ;
private static readonly @string srcᶜ39ᵇ = """

package a

import "unsafe"

const _ = unsafe.Offsetof(struct{ x int64 }{}.x)

"""u8.Raw();

public static void TestIssue16902(ж<testing.T> t) {
    @string src = srcᶜ39;
    ref var info = ref heap<typesꓸInfo>(new typesꓸInfo(Types: new map<ast.Expr, types.TypeAndValue>()), out var Ꮡinfo);
    ref var conf = ref heap<types.Config>(new types.Config(
        // TODO(adonovan): use same FileSet as mustTypecheck.
        Importer: defaultImporter(token.NewFileSet()),
        Sizes: new types.StdSizesжSizes(Ꮡ(new types.StdSizes(WordSize: 8, MaxAlign: 8)))
    ), out var Ꮡconf);
    mustTypecheck(src, Ꮡconf, Ꮡinfo);
    foreach (var (_, tv) in info.Types) {
        _ = conf.Sizes.Sizeof(tv.Type);
        _ = conf.Sizes.Alignof(tv.Type);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ40 => ref srcᶜ40ᵇ;
private static readonly @string srcᶜ40ᵇ = """

package main

import "sync/atomic"

var s struct {
	x int32
	y atomic.Int64
	z int64
}

"""u8.Raw();

// go.dev/issue/53884.
public static void TestAtomicAlign(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(t)); // The Go command is needed for the importer to determine the locations of stdlib .a files.

    @string src = srcᶜ40;

    var want = slice<int64>([0, 8, 16]);
    foreach (var (_, arch) in slice<@string>(["386", "amd64"])) {
        var wantʗ1 = want;
        t.Run(arch, (ж<testing.T> tΔ1) => {
            ref var conf = ref heap<types.Config>(new types.Config(
                // TODO(adonovan): use same FileSet as findStructTypeConfig.
                Importer: defaultImporter(token.NewFileSet()),
                Sizes: types.SizesFor("gc", arch)
            ), out var Ꮡconf);
            var ts = findStructTypeConfig(tΔ1, src, Ꮡconf);
            slice<ж<types.Var>> fields = default;
            // Make a copy manually :(
            for (nint i = 0; i < ts.NumFields(); i++) {
                fields = append(fields, ts.Field(i));
            }

            var offsets = conf.Sizes.Offsetsof(fields);
            if (offsets[0] != wantʗ1[0] || offsets[1] != wantʗ1[1] || offsets[2] != wantʗ1[2]) {
                tΔ1.Errorf("OffsetsOf(%v) = %v want %v", ts.OrTypedNil(), offsets, wantʗ1);
            }
        });
    }
}

partial struct gcSizeTest {
    internal @string name;
    internal @string src;
}

internal static slice<gcSizeTest> gcSizesTests = slice<gcSizeTest>([
    new(
        "issue60431",
        """

package main

import "unsafe"

// The foo struct size is expected to be rounded up to 16 bytes.
type foo struct {
	a int64
	b bool
}

func main() {
	assert(unsafe.Sizeof(foo{}) == 16)
}
"""u8.Raw()
    ),
    new(
        "issue60734",
        """

package main

import (
	"unsafe"
)

// The Data struct size is expected to be rounded up to 16 bytes.
type Data struct {
	Value  uint32   // 4 bytes
	Label  [10]byte // 10 bytes
	Active bool     // 1 byte
	// padded with 1 byte to make it align
}

func main() {
	assert(unsafe.Sizeof(Data{}) == 16)
}

"""u8.Raw()
    )
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string amd64ˢ => ref amd64ˢᵇ;
private static readonly @string amd64ˢᵇ = "amd64"u8;

public static void TestGCSizes(ж<testing.T> t) {
    types.DefPredeclaredTestFuncs();
    foreach (var (_, vᴛ1) in gcSizesTests) {
        ref var tc = ref heap(new gcSizeTest(), out var Ꮡtc);
        tc = vᴛ1;

        var tcʗ1 = tc;
        t.Run(tc.name, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            ref var conf = ref heap<types.Config>(new types.Config(
                // TODO(adonovan): use same FileSet as mustTypecheck.
                Importer: defaultImporter(token.NewFileSet()),
                Sizes: types.SizesFor("gc", amd64ˢ)
            ), out var Ꮡconf);
            mustTypecheck(tcʗ1.src, Ꮡconf, nil);
        });
    }
}

} // end types_test_package
