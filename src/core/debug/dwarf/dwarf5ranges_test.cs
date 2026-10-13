// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using binary = encoding.binary_package;
using os = os_package;
using reflect = reflect_package;
using testing = testing_package;
using encoding;
using static go.debug.dwarf_package;

partial class dwarf_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataDebugRnglistsˢ = "testdata/debug_rnglists"u8;
internal static readonly @string debugRnglistsˢ = ".debug_rnglists"u8;

public static void TestDwarf5Ranges(ж<testing.T> Ꮡt) {
    var (rngLists, err) = os.ReadFile(testdataDebugRnglistsˢ);
    if (err != default!) {
        Ꮡt.Fatalf("could not read test data: %v"u8, err);
    }

    var d = Ꮡ(new Data(nil));
    d.Value.order = binary.LittleEndian;
    {
        var errΔ1 = d.AddSection(debugRnglistsˢ, rngLists); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    var u = Ꮡ(new unit(
        asize: 8,
        vers: 5,
        is64: true
    ));
    (var ret, err) = d.dwarf5Ranges(u, nil, 0x5fbd, 0xc, GoReflect.WithElemDims(new array<uint64>[]{}.slice(), 2));
    if (err != default!) {
        Ꮡt.Fatalf("could not read rnglist: %v"u8, err);
    }
    Ꮡt.Logf("%#v"u8, ret);

    var tgt = GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x0000000000006712, 0x000000000000679f}.array(), new uint64[]{0x00000000000067af}.array(2), new uint64[]{0x00000000000067b3}.array(2)}.slice(), 2);

    if (reflect.DeepEqual(ret, tgt)) {
        Ꮡt.Errorf("expected %#v got %#x"u8, tgt, ret);
    }
}

} // end dwarf_internal_test_package
