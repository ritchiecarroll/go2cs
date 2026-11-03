// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using static go.debug.dwarf_package;
using elf = go.debug.elf_package;
using macho = go.debug.macho_package;
using pe = go.debug.pe_package;
using fmt = fmt_package;
using strconv = strconv_package;
using testing = testing_package;
using dwarf = go.debug.dwarf_package;
using go.debug;
using static go.debug.dwarf_internal_test_package;

partial class dwarf_test_package {

internal static map<@string, @string> typedefTests = new map<@string, @string>{
    ["t_ptr_volatile_int"] = "*volatile int",
    ["t_ptr_const_char"] = "*const char",
    ["t_long"] = "long int",
    ["t_ushort"] = "short unsigned int",
    ["t_func_int_of_float_double"] = "func(float, double) int",
    ["t_ptr_func_int_of_float_double"] = "*func(float, double) int",
    ["t_ptr_func_int_of_float_complex"] = "*func(complex float) int",
    ["t_ptr_func_int_of_double_complex"] = "*func(complex double) int",
    ["t_ptr_func_int_of_long_double_complex"] = "*func(complex long double) int",
    ["t_func_ptr_int_of_char_schar_uchar"] = "func(char, signed char, unsigned char) *int",
    ["t_func_void_of_char"] = "func(char) void",
    ["t_func_void_of_void"] = "func() void",
    ["t_func_void_of_ptr_char_dots"] = "func(*char, ...) void",
    ["t_my_struct"] = "struct my_struct {vi volatile int@0; x char@4 : 1@7; y int@4 : 4@27; z [0]int@8; array [40]long long int@8; zz [0]int@328}",
    ["t_my_struct1"] = "struct my_struct1 {zz [1]int@0}",
    ["t_my_union"] = "union my_union {vi volatile int@0; x char@0 : 1@7; y int@0 : 4@28; array [40]long long int@0}",
    ["t_my_enum"] = "enum my_enum {e1=1; e2=2; e3=-5; e4=1000000000000000}",
    ["t_my_list"] = "struct list {val short int@0; next *t_my_list@8}",
    ["t_my_tree"] = "struct tree {left *struct tree@0; right *struct tree@8; val long long unsigned int@16}"
};

// As Apple converts gcc to a clang-based front end
// they keep breaking the DWARF output. This map lists the
// conversion from real answer to Apple answer.
internal static map<@string, @string> machoBug = new map<@string, @string>{
    ["func(*char, ...) void"] = "func(*char) void",
    ["enum my_enum {e1=1; e2=2; e3=-5; e4=1000000000000000}"] = "enum my_enum {e1=1; e2=2; e3=-5; e4=-1530494976}"
};

internal static ж<dwarf.Data> elfData(ж<testing.T> t, @string name) {
    var (f, err) = elf.Open(name);
    if (err != nil) {
        t.Fatal(err);
    }

    (var d, err) = f.DWARF();
    if (err != nil) {
        t.Fatal(err);
    }
    return d;
}

internal static ж<dwarf.Data> machoData(ж<testing.T> t, @string name) {
    var (f, err) = macho.Open(name);
    if (err != nil) {
        t.Fatal(err);
    }

    (var d, err) = f.DWARF();
    if (err != nil) {
        t.Fatal(err);
    }
    return d;
}

internal static ж<dwarf.Data> peData(ж<testing.T> t, @string name) {
    var (f, err) = pe.Open(name);
    if (err != nil) {
        t.Fatal(err);
    }

    (var d, err) = f.DWARF();
    if (err != nil) {
        t.Fatal(err);
    }
    return d;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataTypedefElfˢ => ref testdataTypedefElfˢᵇ;
private static readonly @string testdataTypedefElfˢᵇ = "testdata/typedef.elf"u8;
internal static ref readonly @string elfˢ2 => ref elfˢ2ᵇ;
private static readonly @string elfˢ2ᵇ = "elf"u8;

public static void TestTypedefsELF(ж<testing.T> t) {
    testTypedefs(t, elfData(t, testdataTypedefElfˢ), elfˢ2, typedefTests);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataTypedefMachoˢ => ref testdataTypedefMachoˢᵇ;
private static readonly @string testdataTypedefMachoˢᵇ = "testdata/typedef.macho"u8;
internal static ref readonly @string machoˢ => ref machoˢᵇ;
private static readonly @string machoˢᵇ = "macho"u8;

public static void TestTypedefsMachO(ж<testing.T> t) {
    testTypedefs(t, machoData(t, testdataTypedefMachoˢ), machoˢ, typedefTests);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataTypedefElf4ˢ => ref testdataTypedefElf4ˢᵇ;
private static readonly @string testdataTypedefElf4ˢᵇ = "testdata/typedef.elf4"u8;

public static void TestTypedefsELFDwarf4(ж<testing.T> t) {
    testTypedefs(t, elfData(t, testdataTypedefElf4ˢ), elfˢ2, typedefTests);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rNextˢ = (@string)"r.Next:"u8;
internal static readonly object dTypeˢ = (@string)"d.Type:"u8;

internal static void testTypedefs(ж<testing.T> t, ж<dwarf.Data> d, @string kind, map<@string, @string> testcases) {
    var r = d.Reader();
    var seen = new map<@string, bool>();
    for (;;) {
        var (e, err) = r.Next();
        if (err != nil) {
            t.Fatal(rNextˢ, err);
        }
        if (e == nil) {
            break;
        }
        if (e.Tag == TagTypedef) {
            var (typ, errΔ1) = d.Type(e.Offset);
            if (errΔ1 != nil) {
                t.Fatal(dTypeˢ, errΔ1);
            }
            var t1 = typ._<ж<dwarf.TypedefType>>();
            @string typstr = default;
            {
                var (ts, ok) = t1.Type._<ж<dwarf.StructType>>(ᐧ); if (ok) {
                    typstr = ts.Defn();
                } else {
                    typstr = t1.Type.String();
                }
            }

            {
                var (want, ok) = testcases[t1.Name, ꟷ]; if (ok) {
                    if (seen[t1.Name]) {
                        t.Errorf("multiple definitions for %s", t1.Name);
                    }
                    seen[t1.Name] = true;
                    if (typstr != want && (kind != "macho"u8 || typstr != machoBug[want])) {
                        t.Errorf("%s:\n\thave %s\n\twant %s", t1.Name, typstr, want);
                    }
                }
            }
        }
        if (e.Tag != TagCompileUnit) {
            r.SkipChildren();
        }
    }

    foreach (var (k, _) in testcases) {
        if (!seen[k]) {
            t.Errorf("missing %s", k);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataCycleElfˢ => ref testdataCycleElfˢᵇ;
private static readonly @string testdataCycleElfˢᵇ = "testdata/cycle.elf"u8;

public static void TestTypedefCycle(ж<testing.T> t) {
    var offsetsᴷ = new stackAppend<dwarf.Offset>(moves: true);

    // See issue #13039: reading a typedef cycle starting from a
    // different place than the size needed to be computed from
    // used to crash.
    //
    // cycle.elf built with GCC 4.8.4:
    //    gcc -g -c -o cycle.elf cycle.c
    var d = elfData(t, testdataCycleElfˢ);
    var r = d.Reader();
    var offsets = slice<dwarf.Offset>([]);
    for (;;) {
        var (e, err) = r.Next();
        if (err != nil) {
            t.Fatal(rNextˢ, err);
        }
        if (e == nil) {
            break;
        }
        switch (e.Tag) {
        case var ᴛ when ᴛ == TagBaseType || ᴛ == TagTypedef || ᴛ == TagPointerType || ᴛ == TagStructType: {
            offsets = append(offsetsᴷ, offsets, e.Offset);
            break;
        }}
    }

    // Parse each type with a fresh type cache.
    offsets = move2heap(offsetsᴷ, offsets);
    foreach (var (_, offset) in offsets) {
        var dΔ1 = elfData(t, testdataCycleElfˢ);
        var (_, err) = dΔ1.Type(offset);
        if (err != nil) {
            t.Fatalf("d.Type(0x%x): %s", offset, err);
        }
    }
}

internal static slice<@string> unsupportedTypeTests = slice<@string>([
    // varname:typename:string:size
    "culprit::(unsupported type ReferenceType):8",
    "pdm::(unsupported type PtrToMemberType):-1"
]);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataCppunsuptypesElfˢ => ref testdataCppunsuptypesElfˢᵇ;
private static readonly @string testdataCppunsuptypesElfˢᵇ = "testdata/cppunsuptypes.elf"u8;

public static void TestUnsupportedTypes(ж<testing.T> t) {
    // Issue 29601:
    // When reading DWARF from C++ load modules, we can encounter
    // oddball type DIEs. These will be returned as "UnsupportedType"
    // objects; check to make sure this works properly.
    var d = elfData(t, testdataCppunsuptypesElfˢ);
    var r = d.Reader();
    var seen = new map<@string, bool>();
    for (;;) {
        var (e, err) = r.Next();
        if (err != nil) {
            t.Fatal(rNextˢ, err);
        }
        if (e == nil) {
            break;
        }
        if (e.Tag == TagVariable) {
            var (vname, _) = e.Val(AttrName)._<@string>(ᐧ);
            var tAttr = e.Val(AttrType);
            var (typOff, ok) = tAttr._<Offset>(ᐧ);
            if (!ok) {
                t.Errorf("variable at offset %v has no type", e.Offset);
                continue;
            }
            var (typ, errΔ1) = d.Type(typOff);
            if (errΔ1 != nil) {
                t.Errorf("err in type decode: %v\n", errΔ1);
                continue;
            }
            var (unsup, isok) = typ._<ж<dwarf.UnsupportedType>>(ᐧ);
            if (!isok) {
                continue;
            }
            @string tag = vname + ":"u8 + unsup.Name + ":"u8 + unsup.String() + ":"u8 + strconv.FormatInt(unsup.of(dwarf.UnsupportedType.ᏑCommonType).Size(), 10);
            seen[tag] = true;
        }
    }
    var dumpseen = false;
    foreach (var (_, v) in unsupportedTypeTests) {
        if (!seen[v]) {
            t.Errorf("missing %s", v);
            dumpseen = true;
        }
    }
    if (dumpseen) {
        foreach (var (k, _) in seen) {
            fmt.Printf("seen: %s\n"u8, k);
        }
    }
}

internal static map<@string, @string> expectedBitOffsets1 = new map<@string, @string>{
    ["x"] = "S:1 DBO:32",
    ["y"] = "S:4 DBO:33"
};

internal static map<@string, @string> expectedBitOffsets2 = new map<@string, @string>{
    ["x"] = "S:1 BO:7",
    ["y"] = "S:4 BO:27"
};

public static void TestBitOffsetsELF(ж<testing.T> t) {
    @string f = testdataTypedefElfˢ;
    testBitOffsets(t, elfData(t, f), f, expectedBitOffsets2);
}

public static void TestBitOffsetsMachO(ж<testing.T> t) {
    @string f = testdataTypedefMachoˢ;
    testBitOffsets(t, machoData(t, f), f, expectedBitOffsets2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataTypedefMacho4ˢ => ref testdataTypedefMacho4ˢᵇ;
private static readonly @string testdataTypedefMacho4ˢᵇ = "testdata/typedef.macho4"u8;

public static void TestBitOffsetsMachO4(ж<testing.T> t) {
    @string f = testdataTypedefMacho4ˢ;
    testBitOffsets(t, machoData(t, f), f, expectedBitOffsets1);
}

public static void TestBitOffsetsELFDwarf4(ж<testing.T> t) {
    @string f = testdataTypedefElf4ˢ;
    testBitOffsets(t, elfData(t, f), f, expectedBitOffsets1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataTypedefElf5ˢ => ref testdataTypedefElf5ˢᵇ;
private static readonly @string testdataTypedefElf5ˢᵇ = "testdata/typedef.elf5"u8;

public static void TestBitOffsetsELFDwarf5(ж<testing.T> t) {
    @string f = testdataTypedefElf5ˢ;
    testBitOffsets(t, elfData(t, f), f, expectedBitOffsets1);
}

internal static void testBitOffsets(ж<testing.T> t, ж<dwarf.Data> d, @string tag, map<@string, @string> expectedBitOffsets) {
    var r = d.Reader();
    for (;;) {
        var (e, err) = r.Next();
        if (err != nil) {
            t.Fatal(rNextˢ, err);
        }
        if (e == nil) {
            break;
        }

        if (e.Tag == TagStructType) {
            var (typ, errΔ1) = d.Type(e.Offset);
            if (errΔ1 != nil) {
                t.Fatal(dTypeˢ, errΔ1);
            }

            var t1 = typ._<ж<dwarf.StructType>>();

            @string bitInfoDump(ж<dwarf.StructField> f) {
                @string res = fmt.Sprintf("S:%d"u8, f.BitSize);
                if (f.BitOffset != 0) {
                    res += fmt.Sprintf(" BO:%d"u8, f.BitOffset);
                }
                if (f.DataBitOffset != 0) {
                    res += fmt.Sprintf(" DBO:%d"u8, f.DataBitOffset);
                }
                return res;
            }

            foreach (var (_, field) in t1.Field) {
                // We're only testing for bitfields
                if (field.BitSize == 0) {
                    continue;
                }
                @string got = bitInfoDump(field);
                @string want = expectedBitOffsets[field.Name];
                if (got != want) {
                    t.Errorf("%s: field %s in %s: got info %q want %q", tag, field.Name, t1.StructName, got, want);
                }
            }
        }
        if (e.Tag != TagCompileUnit) {
            r.SkipChildren();
        }
    }
}

internal static map<@string, @string> bitfieldTests = new map<@string, @string>{
    ["t_another_struct"] = "struct another_struct {quix short unsigned int@0; xyz [0]int@4; x unsigned int@4 : 1@31; array [40]long long int@8}"
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataBitfieldsElf4ˢ => ref testdataBitfieldsElf4ˢᵇ;
private static readonly @string testdataBitfieldsElf4ˢᵇ = "testdata/bitfields.elf4"u8;

// TestBitFieldZeroArrayIssue50685 checks to make sure that the DWARF
// type reading code doesn't get confused by the presence of a
// specifically-sized bitfield member immediately following a field
// whose type is a zero-length array. Prior to the fix for issue
// 50685, we would get this type for the case in testdata/bitfields.c:
//
// another_struct {quix short unsigned int@0; xyz [-1]int@4; x unsigned int@4 : 1@31; array [40]long long int@8}
//
// Note the "-1" for the xyz field, which should be zero.
public static void TestBitFieldZeroArrayIssue50685(ж<testing.T> t) {
    @string f = testdataBitfieldsElf4ˢ;
    testTypedefs(t, elfData(t, f), elfˢ2, bitfieldTests);
}

} // end dwarf_test_package
