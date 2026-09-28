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
internal static ж<types.Struct> findStructType(ж<testing.T> Ꮡt, @string src) {
    return findStructTypeConfig(Ꮡt, src, Ꮡ(new types.Config(nil)));
}

internal static ж<types.Struct> findStructTypeConfig(ж<testing.T> Ꮡt, @string src, ж<types.Config> Ꮡconf) {
    var types_ = new map<ast.Expr, types.TypeAndValue>();
    mustTypecheck(src, nil, Ꮡ(new typesꓸInfo(Types: types_)));
    foreach (var (_, tv) in types_) {
        {
            var (ts, ok) = tv.Type._<ж<types.Struct>>(ᐧ); if (ok) {
                return ts;
            }
        }
    }
    Ꮡt.Fatalf("failed to find a struct type in src:\n%s\n"u8, src);
    return default!;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ36 = """

package main

type S struct {
    i int
    b bool
    s string
    n int
}

"""u8;

// go.dev/issue/16316
public static void TestMultipleSizeUse(ж<testing.T> Ꮡt) {
    @string src = srcᶜ36;
    var ts = findStructType(Ꮡt, src);
    ref var sizes = ref heap<types.StdSizes>(out var Ꮡsizes);
    sizes = new types.StdSizes(WordSize: 4, MaxAlign: 4);
    {
        var got = Ꮡsizes.Sizeof(new types.StructжΔType(ts)); if (got != 20) {
            Ꮡt.Errorf("Sizeof(%v) with WordSize 4 = %d want 20"u8, ts.OrTypedNil(), got);
        }
    }
    sizes = new types.StdSizes(WordSize: 8, MaxAlign: 8);
    {
        var got = Ꮡsizes.Sizeof(new types.StructжΔType(ts)); if (got != 40) {
            Ꮡt.Errorf("Sizeof(%v) with WordSize 8 = %d want 40"u8, ts.OrTypedNil(), got);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ37 = """

package main

var s struct {
	x *int
	y []byte
}

"""u8;

// go.dev/issue/16464
public static void TestAlignofNaclSlice(ж<testing.T> Ꮡt) {
    @string src = srcᶜ37;
    var ts = findStructType(Ꮡt, src);
    var sizes = Ꮡ(new types.StdSizes(WordSize: 4, MaxAlign: 8));
    slice<ж<types.Var>> fields = default!;
    // Make a copy manually :(
    for (nint i = 0; i < ts.NumFields(); i++) {
        fields = append(fields, ts.Field(i));
    }
    var offsets = sizes.Offsetsof(fields);
    if (offsets[0] != 0 || offsets[1] != 4) {
        Ꮡt.Errorf("OffsetsOf(%v) = %v want %v"u8, ts.OrTypedNil(), offsets, new nint[]{0, 4}.slice());
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ38 = """

package a

import "unsafe"

const _ = unsafe.Offsetof(struct{ x int64 }{}.x)

"""u8;

public static void TestIssue16902(ж<testing.T> Ꮡt) {
    @string src = srcᶜ38;
    ref var info = ref heap<typesꓸInfo>(out var Ꮡinfo);
    info = new typesꓸInfo(Types: new map<ast.Expr, types.TypeAndValue>());
    ref var conf = ref heap<types.Config>(out var Ꮡconf);
    conf = new types.Config( // TODO(adonovan): use same FileSet as mustTypecheck.

        Importer: defaultImporter(token.NewFileSet()),
        Sizes: new types.StdSizesжSizes(Ꮡ(new types.StdSizes(WordSize: 8, MaxAlign: 8)))
    );
    mustTypecheck(src, Ꮡconf, Ꮡinfo);
    foreach (var (_, tv) in info.Types) {
        _ = conf.Sizes.Sizeof(tv.Type);
        _ = conf.Sizes.Alignof(tv.Type);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ39 = """

package main

import "sync/atomic"

var s struct {
	x int32
	y atomic.Int64
	z int64
}

"""u8;

// go.dev/issue/53884.
public static void TestAtomicAlign(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new types_test_package.testing_TжTB(Ꮡt)); // The Go command is needed for the importer to determine the locations of stdlib .a files.
    @string src = srcᶜ39;
    var want = new int64[]{0, 8, 16}.slice();
    foreach (var (_, arch) in new @string[]{"386"u8, "amd64"u8}.slice()) {
        var wantʗ1 = want;
        Ꮡt.Run(arch, (ж<testing.T> tΔ1) => {
            ref var conf = ref heap<types.Config>(out var Ꮡconf);
            conf = new types.Config( // TODO(adonovan): use same FileSet as findStructTypeConfig.

                Importer: defaultImporter(token.NewFileSet()),
                Sizes: types.SizesFor("gc"u8, arch)
            );
            var ts = findStructTypeConfig(tΔ1, src, Ꮡconf);
            slice<ж<types.Var>> fields = default!;
            // Make a copy manually :(
            for (nint i = 0; i < ts.NumFields(); i++) {
                fields = append(fields, ts.Field(i));
            }
            var offsets = conf.Sizes.Offsetsof(fields);
            if (offsets[0] != wantʗ1[0] || offsets[1] != wantʗ1[1] || offsets[2] != wantʗ1[2]) {
                tΔ1.Errorf("OffsetsOf(%v) = %v want %v"u8, ts.OrTypedNil(), offsets, wantʗ1);
            }
        });
    }
}

[GoType] partial struct gcSizeTest {
    internal @string name;
    internal @string src;
}

internal static slice<gcSizeTest> gcSizesTests = new gcSizeTest[]{
    new(
        "issue60431"u8,
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
"""u8
    ),
    new(
        "issue60734"u8,
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

"""u8
    )
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string amd64ˢ = "amd64"u8;

public static void TestGCSizes(ж<testing.T> Ꮡt) {
    types.DefPredeclaredTestFuncs();
    foreach (var (_, tc) in gcSizesTests) {
        ref var tcΔ1 = ref heap<gcSizeTest>(out var ᏑtcΔ1);
        tcΔ1 = tc;
        var tcʗ1 = tcΔ1;
        Ꮡt.Run(tcΔ1.name, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            ref var conf = ref heap<types.Config>(out var Ꮡconf);
            conf = new types.Config( // TODO(adonovan): use same FileSet as mustTypecheck.

                Importer: defaultImporter(token.NewFileSet()),
                Sizes: types.SizesFor("gc"u8, amd64ˢ)
            );
            mustTypecheck(tcʗ1.src, Ꮡconf, nil);
        });
    }
}

} // end types_test_package
