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
    ["t_ptr_volatile_int"u8] = "*volatile int"u8,
    ["t_ptr_const_char"u8] = "*const char"u8,
    ["t_long"u8] = "long int"u8,
    ["t_ushort"u8] = "short unsigned int"u8,
    ["t_func_int_of_float_double"u8] = "func(float, double) int"u8,
    ["t_ptr_func_int_of_float_double"u8] = "*func(float, double) int"u8,
    ["t_ptr_func_int_of_float_complex"u8] = "*func(complex float) int"u8,
    ["t_ptr_func_int_of_double_complex"u8] = "*func(complex double) int"u8,
    ["t_ptr_func_int_of_long_double_complex"u8] = "*func(complex long double) int"u8,
    ["t_func_ptr_int_of_char_schar_uchar"u8] = "func(char, signed char, unsigned char) *int"u8,
    ["t_func_void_of_char"u8] = "func(char) void"u8,
    ["t_func_void_of_void"u8] = "func() void"u8,
    ["t_func_void_of_ptr_char_dots"u8] = "func(*char, ...) void"u8,
    ["t_my_struct"u8] = "struct my_struct {vi volatile int@0; x char@4 : 1@7; y int@4 : 4@27; z [0]int@8; array [40]long long int@8; zz [0]int@328}"u8,
    ["t_my_struct1"u8] = "struct my_struct1 {zz [1]int@0}"u8,
    ["t_my_union"u8] = "union my_union {vi volatile int@0; x char@0 : 1@7; y int@0 : 4@28; array [40]long long int@0}"u8,
    ["t_my_enum"u8] = "enum my_enum {e1=1; e2=2; e3=-5; e4=1000000000000000}"u8,
    ["t_my_list"u8] = "struct list {val short int@0; next *t_my_list@8}"u8,
    ["t_my_tree"u8] = "struct tree {left *struct tree@0; right *struct tree@8; val long long unsigned int@16}"u8
};

// As Apple converts gcc to a clang-based front end
// they keep breaking the DWARF output. This map lists the
// conversion from real answer to Apple answer.
internal static map<@string, @string> machoBug = new map<@string, @string>{
    ["func(*char, ...) void"u8] = "func(*char) void"u8,
    ["enum my_enum {e1=1; e2=2; e3=-5; e4=1000000000000000}"u8] = "enum my_enum {e1=1; e2=2; e3=-5; e4=-1530494976}"u8
};

internal static ж<dwarf.Data> elfData(ж<testing.T> Ꮡt, @string name) {
    var (f, err) = elf.Open(name);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }

    (var d, err) = f.DWARF();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return d;
}

internal static ж<dwarf.Data> machoData(ж<testing.T> Ꮡt, @string name) {
    var (f, err) = macho.Open(name);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }

    (var d, err) = f.DWARF();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return d;
}

internal static ж<dwarf.Data> peData(ж<testing.T> Ꮡt, @string name) {
    var (f, err) = pe.Open(name);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }

    (var d, err) = f.DWARF();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return d;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTypedefElfˢ = "testdata/typedef.elf"u8;
internal static readonly @string elfˢ2 = "elf"u8;

public static void TestTypedefsELF(ж<testing.T> Ꮡt) {
    testTypedefs(Ꮡt, elfData(Ꮡt, testdataTypedefElfˢ), elfˢ2, typedefTests);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTypedefMachoˢ = "testdata/typedef.macho"u8;
internal static readonly @string machoˢ = "macho"u8;

public static void TestTypedefsMachO(ж<testing.T> Ꮡt) {
    testTypedefs(Ꮡt, machoData(Ꮡt, testdataTypedefMachoˢ), machoˢ, typedefTests);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTypedefElf4ˢ = "testdata/typedef.elf4"u8;

public static void TestTypedefsELFDwarf4(ж<testing.T> Ꮡt) {
    testTypedefs(Ꮡt, elfData(Ꮡt, testdataTypedefElf4ˢ), elfˢ2, typedefTests);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rNextˢ = (@string)"r.Next:"u8;
internal static readonly object dTypeˢ = (@string)"d.Type:"u8;

internal static void testTypedefs(ж<testing.T> Ꮡt, ж<dwarf.Data> Ꮡd, @string kind, map<@string, @string> testcases) {
    var r = Ꮡd.Reader();
    var seen = new map<@string, bool>();
    while (ᐧ) {
        var (e, err) = r.Next();
        if (err != default!) {
            Ꮡt.Fatal(rNextˢ, err);
        }
        if (e == nil) {
            break;
        }
        if ((~e).Tag == TagTypedef) {
            var (typ, errΔ1) = Ꮡd.Type((~e).Offset);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(dTypeˢ, errΔ1);
            }
            var t1 = typ._<ж<dwarf.TypedefType>>();
            @string typstr = default!;
            {
                var (ts, ok) = (~t1).Type._<ж<dwarf.StructType>>(ᐧ); if (ok){
                    typstr = ts.Defn();
                } else {
                    typstr = (~t1).Type.String();
                }
            }

            {
                var (want, ok) = testcases[(~t1).Name, ꟷ]; if (ok) {
                    if (seen[(~t1).Name]) {
                        Ꮡt.Errorf("multiple definitions for %s"u8, (~t1).Name);
                    }
                    seen[(~t1).Name] = true;
                    if (typstr != want && (kind != "macho"u8 || typstr != machoBug[want])) {
                        Ꮡt.Errorf("%s:\n\thave %s\n\twant %s"u8, (~t1).Name, typstr, want);
                    }
                }
            }
        }
        if ((~e).Tag != TagCompileUnit) {
            r.SkipChildren();
        }
    }

    foreach (var (k, _) in testcases) {
        if (!seen[k]) {
            Ꮡt.Errorf("missing %s"u8, k);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataCycleElfˢ = "testdata/cycle.elf"u8;

public static void TestTypedefCycle(ж<testing.T> Ꮡt) {
    // See issue #13039: reading a typedef cycle starting from a
    // different place than the size needed to be computed from
    // used to crash.
    //
    // cycle.elf built with GCC 4.8.4:
    //    gcc -g -c -o cycle.elf cycle.c
    var d = elfData(Ꮡt, testdataCycleElfˢ);
    var r = d.Reader();
    var offsets = new dwarf.Offset[]{}.slice();
    while (ᐧ) {
        var (e, err) = r.Next();
        if (err != default!) {
            Ꮡt.Fatal(rNextˢ, err);
        }
        if (e == nil) {
            break;
        }
        var exprᴛ1 = (~e).Tag;
        if (exprᴛ1 == TagBaseType || exprᴛ1 == TagTypedef || exprᴛ1 == TagPointerType || exprᴛ1 == TagStructType) {
            offsets = append(offsets, (~e).Offset);
        }
    }

    // Parse each type with a fresh type cache.
    foreach (var (_, offset) in offsets) {
        var dΔ1 = elfData(Ꮡt, testdataCycleElfˢ);
        var (_, err) = dΔ1.Type(offset);
        if (err != default!) {
            Ꮡt.Fatalf("d.Type(0x%x): %s"u8, offset, err);
        }
    }
}

internal static slice<@string> unsupportedTypeTests = new @string[]{
    // varname:typename:string:size
    "culprit::(unsupported type ReferenceType):8"u8,
    "pdm::(unsupported type PtrToMemberType):-1"u8
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataCppunsuptypesElfˢ = "testdata/cppunsuptypes.elf"u8;

public static void TestUnsupportedTypes(ж<testing.T> Ꮡt) {
    // Issue 29601:
    // When reading DWARF from C++ load modules, we can encounter
    // oddball type DIEs. These will be returned as "UnsupportedType"
    // objects; check to make sure this works properly.
    var d = elfData(Ꮡt, testdataCppunsuptypesElfˢ);
    var r = d.Reader();
    var seen = new map<@string, bool>();
    while (ᐧ) {
        var (e, err) = r.Next();
        if (err != default!) {
            Ꮡt.Fatal(rNextˢ, err);
        }
        if (e == nil) {
            break;
        }
        if ((~e).Tag == TagVariable) {
            var (vname, _) = e.Val(AttrName)._<@string>(ᐧ);
            var tAttr = e.Val(AttrType);
            var (typOff, ok) = tAttr._<Offset>(ᐧ);
            if (!ok) {
                Ꮡt.Errorf("variable at offset %v has no type"u8, (~e).Offset);
                continue;
            }
            var (typ, errΔ1) = d.Type(typOff);
            if (errΔ1 != default!) {
                Ꮡt.Errorf("err in type decode: %v\n"u8, errΔ1);
                continue;
            }
            var (unsup, isok) = typ._<ж<dwarf.UnsupportedType>>(ᐧ);
            if (!isok) {
                continue;
            }
            @string tag = vname + ":"u8 + (~unsup).Name + ":"u8 + unsup.String() + ":"u8 + strconv.FormatInt(unsup.of(dwarf.UnsupportedType.ᏑCommonType).Size(), 10);
            seen[tag] = true;
        }
    }
    var dumpseen = false;
    foreach (var (_, v) in unsupportedTypeTests) {
        if (!seen[v]) {
            Ꮡt.Errorf("missing %s"u8, v);
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
    ["x"u8] = "S:1 DBO:32"u8,
    ["y"u8] = "S:4 DBO:33"u8
};

internal static map<@string, @string> expectedBitOffsets2 = new map<@string, @string>{
    ["x"u8] = "S:1 BO:7"u8,
    ["y"u8] = "S:4 BO:27"u8
};

public static void TestBitOffsetsELF(ж<testing.T> Ꮡt) {
    @string f = testdataTypedefElfˢ;
    testBitOffsets(Ꮡt, elfData(Ꮡt, f), f, expectedBitOffsets2);
}

public static void TestBitOffsetsMachO(ж<testing.T> Ꮡt) {
    @string f = testdataTypedefMachoˢ;
    testBitOffsets(Ꮡt, machoData(Ꮡt, f), f, expectedBitOffsets2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTypedefMacho4ˢ = "testdata/typedef.macho4"u8;

public static void TestBitOffsetsMachO4(ж<testing.T> Ꮡt) {
    @string f = testdataTypedefMacho4ˢ;
    testBitOffsets(Ꮡt, machoData(Ꮡt, f), f, expectedBitOffsets1);
}

public static void TestBitOffsetsELFDwarf4(ж<testing.T> Ꮡt) {
    @string f = testdataTypedefElf4ˢ;
    testBitOffsets(Ꮡt, elfData(Ꮡt, f), f, expectedBitOffsets1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTypedefElf5ˢ = "testdata/typedef.elf5"u8;

public static void TestBitOffsetsELFDwarf5(ж<testing.T> Ꮡt) {
    @string f = testdataTypedefElf5ˢ;
    testBitOffsets(Ꮡt, elfData(Ꮡt, f), f, expectedBitOffsets1);
}

internal static void testBitOffsets(ж<testing.T> Ꮡt, ж<dwarf.Data> Ꮡd, @string tag, map<@string, @string> expectedBitOffsets) {
    ref var d = ref Ꮡd.DerefOrNull();

    var r = Ꮡd.Reader();
    while (ᐧ) {
        var (e, err) = r.Next();
        if (err != default!) {
            Ꮡt.Fatal(rNextˢ, err);
        }
        if (e == nil) {
            break;
        }

        if ((~e).Tag == TagStructType) {
            var (typ, errΔ1) = Ꮡd.Type((~e).Offset);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(dTypeˢ, errΔ1);
            }

            var t1 = typ._<ж<dwarf.StructType>>();

            @string bitInfoDump(ж<dwarf.StructField> f) {
                @string res = fmt.Sprintf("S:%d"u8, (~f).BitSize);
                if ((~f).BitOffset != 0) {
                    res += fmt.Sprintf(" BO:%d"u8, (~f).BitOffset);
                }
                if ((~f).DataBitOffset != 0) {
                    res += fmt.Sprintf(" DBO:%d"u8, (~f).DataBitOffset);
                }
                return res;
            }

            foreach (var (_, field) in (~t1).Field) {
                // We're only testing for bitfields
                if ((~field).BitSize == 0) {
                    continue;
                }
                @string got = bitInfoDump(field);
                @string want = expectedBitOffsets[(~field).Name];
                if (got != want) {
                    Ꮡt.Errorf("%s: field %s in %s: got info %q want %q"u8, tag, (~field).Name, (~t1).StructName, got, want);
                }
            }
        }
        if ((~e).Tag != TagCompileUnit) {
            r.SkipChildren();
        }
    }
}

internal static map<@string, @string> bitfieldTests = new map<@string, @string>{
    ["t_another_struct"u8] = "struct another_struct {quix short unsigned int@0; xyz [0]int@4; x unsigned int@4 : 1@31; array [40]long long int@8}"u8
};

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataBitfieldsElf4ˢ = "testdata/bitfields.elf4"u8;

// TestBitFieldZeroArrayIssue50685 checks to make sure that the DWARF
// type reading code doesn't get confused by the presence of a
// specifically-sized bitfield member immediately following a field
// whose type is a zero-length array. Prior to the fix for issue
// 50685, we would get this type for the case in testdata/bitfields.c:
//
// another_struct {quix short unsigned int@0; xyz [-1]int@4; x unsigned int@4 : 1@31; array [40]long long int@8}
//
// Note the "-1" for the xyz field, which should be zero.
public static void TestBitFieldZeroArrayIssue50685(ж<testing.T> Ꮡt) {
    @string f = testdataBitfieldsElf4ˢ;
    testTypedefs(Ꮡt, elfData(Ꮡt, f), elfˢ2, bitfieldTests);
}

} // end dwarf_test_package
