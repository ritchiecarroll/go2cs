// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using static go.debug.dwarf_package;
using binary = encoding.binary_package;
using filepath = path.filepath_package;
using reflect = reflect_package;
using testing = testing_package;
using dwarf = go.debug.dwarf_package;
using encoding;
using go.debug;
using path;
using static go.debug.dwarf_internal_test_package;

partial class dwarf_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataSplitElfˢ = "testdata/split.elf"u8;

public static void TestSplit(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // debug/dwarf doesn't (currently) support split DWARF, but
    // the attributes that pointed to the split DWARF used to
    // cause loading the DWARF data to fail entirely (issue
    // #12592). Test that we can at least read the DWARF data.
    var d = elfData(Ꮡt, testdataSplitElfˢ);
    var r = d.Reader();
    var (e, err) = r.Next();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if ((~e).Tag != TagCompileUnit) {
        Ꮡt.Fatalf("bad tag: have %s, want %s"u8, (~e).Tag, TagCompileUnit);
    }
    // Check that we were able to parse the unknown section offset
    // field, even if we can't figure out its DWARF class.
    dwarf.Attr AttrGNUAddrBase = 0x2133;
    var f = e.AttrField(AttrGNUAddrBase);
    {
        var (_, ok) = (~f).Val._<int64>(ᐧ); if (!ok) {
            Ꮡt.Fatalf("bad attribute value type: have %T, want int64"u8, (~f).Val);
        }
    }
    if ((~f).Class != ClassUnknown) {
        Ꮡt.Fatalf("bad class: have %s, want %s"u8, (~f).Class, ClassUnknown);
    }
}

// wantRange maps from a PC to the ranges of the compilation unit
// containing that PC.
[GoType] partial struct wantRange {
    internal uint64 pc;
    [GoArrayDims(2)]
    internal slice<array<uint64>> ranges;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataLineGccElfˢ = "testdata/line-gcc.elf"u8;
internal static readonly @string testdataLineGccDwarf5Elfˢ = "testdata/line-gcc-dwarf5.elf"u8;
internal static readonly @string testdataLineClangDwarf5ˢ = "testdata/line-clang-dwarf5.elf"u8;
internal static readonly @string testdataLineGccZstdElfˢ = "testdata/line-gcc-zstd.elf"u8;

public static void TestReaderSeek(ж<testing.T> Ꮡt) {
    var want = new wantRange[]{
        new(0x40059d, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x40059d, 0x400601}.array()}.slice(), 2)),
        new(0x400600, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x40059d, 0x400601}.array()}.slice(), 2)),
        new(0x400601, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x400601, 0x400611}.array()}.slice(), 2)),
        new(0x4005f0, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x40059d, 0x400601}.array()}.slice(), 2)), // loop test

        new(0x10, default!),
        new(0x400611, default!)
    }.slice();
    testRanges(Ꮡt, testdataLineGccElfˢ, want);
    want = new wantRange[]{
        new(0x401122, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401122, 0x401166}.array()}.slice(), 2)),
        new(0x401165, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401122, 0x401166}.array()}.slice(), 2)),
        new(0x401166, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401166, 0x401179}.array()}.slice(), 2))
    }.slice();
    testRanges(Ꮡt, testdataLineGccDwarf5Elfˢ, want);
    want = new wantRange[]{
        new(0x401130, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401130, 0x40117e}.array()}.slice(), 2)),
        new(0x40117d, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401130, 0x40117e}.array()}.slice(), 2)),
        new(0x40117e, default!)
    }.slice();
    testRanges(Ꮡt, testdataLineClangDwarf5ˢ, want);
    want = new wantRange[]{
        new(0x401126, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401126, 0x40116a}.array()}.slice(), 2)),
        new(0x40116a, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x40116a, 0x401180}.array()}.slice(), 2))
    }.slice();
    testRanges(Ꮡt, testdataLineGccZstdElfˢ, want);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataRangesElfˢ = "testdata/ranges.elf"u8;

public static void TestRangesSection(ж<testing.T> Ꮡt) {
    var want = new wantRange[]{
        new(0x400500, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x400500, 0x400549}.array(), new uint64[]{0x400400, 0x400408}.array()}.slice(), 2)),
        new(0x400400, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x400500, 0x400549}.array(), new uint64[]{0x400400, 0x400408}.array()}.slice(), 2)),
        new(0x400548, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x400500, 0x400549}.array(), new uint64[]{0x400400, 0x400408}.array()}.slice(), 2)),
        new(0x400407, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x400500, 0x400549}.array(), new uint64[]{0x400400, 0x400408}.array()}.slice(), 2)),
        new(0x400408, default!),
        new(0x400449, default!),
        new(0x4003ff, default!)
    }.slice();
    testRanges(Ꮡt, testdataRangesElfˢ, want);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataRnglistxElfˢ = "testdata/rnglistx.elf"u8;

public static void TestRangesRnglistx(ж<testing.T> Ꮡt) {
    var want = new wantRange[]{
        new(0x401000, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401020, 0x40102c}.array(), new uint64[]{0x401000, 0x40101d}.array()}.slice(), 2)),
        new(0x40101c, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401020, 0x40102c}.array(), new uint64[]{0x401000, 0x40101d}.array()}.slice(), 2)),
        new(0x40101d, default!),
        new(0x40101f, default!),
        new(0x401020, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401020, 0x40102c}.array(), new uint64[]{0x401000, 0x40101d}.array()}.slice(), 2)),
        new(0x40102b, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401020, 0x40102c}.array(), new uint64[]{0x401000, 0x40101d}.array()}.slice(), 2)),
        new(0x40102c, default!)
    }.slice();
    testRanges(Ꮡt, testdataRnglistxElfˢ, want);
}

internal static void testRanges(ж<testing.T> Ꮡt, @string name, slice<wantRange> want) {
    var d = elfData(Ꮡt, name);
    var r = d.Reader();
    foreach (var (_, w) in want) {
        var (entry, err) = r.SeekPC(w.pc);
        if (err != default!) {
            if (w.ranges != default!) {
                Ꮡt.Errorf("%s: missing Entry for %#x"u8, name, w.pc);
            }
            if (!AreEqual(err, ErrUnknownPC)) {
                Ꮡt.Errorf("%s: expected ErrUnknownPC for %#x, got %v"u8, name, w.pc, err);
            }
            continue;
        }
        (var ranges, err) = d.Ranges(entry);
        if (err != default!) {
            Ꮡt.Errorf("%s: %v"u8, name, err);
            continue;
        }
        if (!reflect.DeepEqual(ranges, w.ranges)) {
            Ꮡt.Errorf("%s: for %#x got %x, expected %x"u8, name, w.pc, ranges, w.ranges);
        }
    }
}

[GoType("dyn")] internal partial struct TestReaderRanges_subprograms {
    internal @string name;
    [GoArrayDims(2)]
    internal slice<array<uint64>> ranges;
}

[GoLocalName("subprograms")] [GoType("[]TestReaderRanges_subprograms")] internal partial struct TestReaderRanges_subprogramsᴛ1;

[GoType("dyn")] internal partial struct TestReaderRanges_tests {
    internal @string filename;
    internal TestReaderRanges_subprogramsᴛ1 subprograms;
}

public static void TestReaderRanges(ж<testing.T> Ꮡt) {
    var tests = new TestReaderRanges_tests[]{
        new(
            "testdata/line-gcc.elf"u8,
            new TestReaderRanges_subprogramsᴛ1(new TestReaderRanges_subprograms[]{
                new("f1"u8, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x40059d, 0x4005e7}.array()}.slice(), 2)),
                new("main"u8, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x4005e7, 0x400601}.array()}.slice(), 2)),
                new("f2"u8, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x400601, 0x400611}.array()}.slice(), 2))
            }.slice())
        ),
        new(
            "testdata/line-gcc-dwarf5.elf"u8,
            new TestReaderRanges_subprogramsᴛ1(new TestReaderRanges_subprograms[]{
                new("main"u8, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401147, 0x401166}.array()}.slice(), 2)),
                new("f1"u8, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401122, 0x401147}.array()}.slice(), 2)),
                new("f2"u8, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401166, 0x401179}.array()}.slice(), 2))
            }.slice())
        ),
        new(
            "testdata/line-clang-dwarf5.elf"u8,
            new TestReaderRanges_subprogramsᴛ1(new TestReaderRanges_subprograms[]{
                new("main"u8, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401130, 0x401144}.array()}.slice(), 2)),
                new("f1"u8, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401150, 0x40117e}.array()}.slice(), 2)),
                new("f2"u8, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401180, 0x401197}.array()}.slice(), 2))
            }.slice())
        ),
        new(
            "testdata/line-gcc-zstd.elf"u8,
            new TestReaderRanges_subprogramsᴛ1(new TestReaderRanges_subprograms[]{
                new("f2"u8, default!),
                new("main"u8, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x40114b, 0x40116a}.array()}.slice(), 2)),
                new("f1"u8, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x401126, 0x40114b}.array()}.slice(), 2)),
                new("f2"u8, GoReflect.WithElemDims(new array<uint64>[]{new uint64[]{0x40116a, 0x401180}.array()}.slice(), 2))
            }.slice())
        )
    }.slice();
    foreach (var (_, test) in tests) {
        var d = elfData(Ꮡt, test.filename);
        var subprograms = test.subprograms;
        var r = d.Reader();
        nint i = 0;
        for (var (entry, err) = r.Next(); entry != nil && err == default!; (entry, err) = r.Next()) {
            if ((~entry).Tag != TagSubprogram) {
                continue;
            }
            if (i > len(subprograms)) {
                Ꮡt.Fatalf("%s: too many subprograms (expected at most %d)"u8, test.filename, i);
            }
            {
                @string got = entry.Val(AttrName)._<@string>(); if (got != subprograms[i].name) {
                    Ꮡt.Errorf("%s: subprogram %d name is %s, expected %s"u8, test.filename, i, got, subprograms[i].name);
                }
            }
            var (ranges, errΔ1) = d.Ranges(entry);
            if (errΔ1 != default!) {
                Ꮡt.Errorf("%s: subprogram %d: %v"u8, test.filename, i, errΔ1);
                continue;
            }
            if (!reflect.DeepEqual(ranges, subprograms[i].ranges)) {
                Ꮡt.Errorf("%s: subprogram %d ranges are %x, expected %x"u8, test.filename, i, ranges, subprograms[i].ranges);
            }
            i++;
        }
        if (i < len(subprograms)) {
            Ꮡt.Errorf("%s: saw only %d subprograms, expected %d"u8, test.filename, i, len(subprograms));
        }
    }
}

[GoType("dyn")] internal partial struct Test64Bit_tests {
    internal @string name;
    internal slice<byte> info;
    internal nint addrSize;
    internal binary.ByteOrder byteOrder;
}

public static void Test64Bit(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // I don't know how to generate a 64-bit DWARF debug
    // compilation unit except by using XCOFF, so this is
    // hand-written.
    var tests = new Test64Bit_tests[]{
        new(
            "32-bit little"u8,
            new byte[]{0x30, 0, 0, 0, // comp unit length

                4, 0, // DWARF version 4

                0, 0, 0, 0, // abbrev offset

                8, // address size

                0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0
            }.slice(),
            8, binary.LittleEndian
        ),
        new(
            "64-bit little"u8,
            new byte[]{0xff, 0xff, 0xff, 0xff, // 64-bit DWARF

                0x30, 0, 0, 0, 0, 0, 0, 0, // comp unit length

                4, 0, // DWARF version 4

                0, 0, 0, 0, 0, 0, 0, 0, // abbrev offset

                8, // address size

                0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0
            }.slice(),
            8, binary.LittleEndian
        ),
        new(
            "64-bit big"u8,
            new byte[]{0xff, 0xff, 0xff, 0xff, // 64-bit DWARF

                0, 0, 0, 0, 0, 0, 0, 0x30, // comp unit length

                0, 4, // DWARF version 4

                0, 0, 0, 0, 0, 0, 0, 0, // abbrev offset

                8, // address size

                0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0
            }.slice(),
            8, binary.BigEndian
        )
    }.slice();
    foreach (var (_, test) in tests) {
        var (data, err) = New(default!, default!, default!, test.info, default!, default!, default!, default!);
        if (err != default!) {
            Ꮡt.Errorf("%s: %v"u8, test.name, err);
        }
        var r = data.Reader();
        if (r.AddressSize() != test.addrSize) {
            Ꮡt.Errorf("%s: got address size %d, want %d"u8, test.name, r.AddressSize(), test.addrSize);
        }
        if (!AreEqual(r.ByteOrder(), test.byteOrder)) {
            Ꮡt.Errorf("%s: got byte order %s, want %s"u8, test.name, r.ByteOrder(), test.byteOrder);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataˢ = "testdata"u8;
internal static readonly @string elfˢ = "*.elf"u8;
internal static readonly object setOfCUsDifferˢ = (@string)"set of CUs differ"u8;

public static void TestUnitIteration(ж<testing.T> Ꮡt) {
    // Iterate over all ELF test files we have and ensure that
    // we get the same set of compilation units skipping (method 0)
    // and not skipping (method 1) CU children.
    var (files, err) = filepath.Glob(filepath.Join(testdataˢ, elfˢ));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (_, @file) in files) {
        Ꮡt.Run(@file, (ж<testing.T> tΔ1) => {
            var d = elfData(tΔ1, @file);
            array<slice<any>> units = new(2);
            foreach (var (method, _) in units) {
                for (var r = d.Reader(); ᐧ ; ) {
                    var (ent, errΔ1) = r.Next();
                    if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                    if (ent == nil) {
                        break;
                    }
                    if ((~ent).Tag == TagCompileUnit) {
                        units[method] = append(units[method], ent.Val(AttrName));
                    }
                    if (method == 0) {
                        if ((~ent).Tag != TagCompileUnit) {
                            tΔ1.Fatalf("found unexpected tag %v on top level"u8, (~ent).Tag);
                        }
                        r.SkipChildren();
                    }
                }
            }
            tΔ1.Logf("skipping CUs:     %v"u8, (any)(units[0]));
            tΔ1.Logf("not-skipping CUs: %v"u8, (any)(units[1]));
            if (!reflect.DeepEqual(units[0], units[1])) {
                tΔ1.Fatal(setOfCUsDifferˢ);
            }
        });
    }
}

public static void TestIssue51758(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var abbrev = new byte[]{0x21, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x5c,
        0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c,
        0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33,
        0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37,
        0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37,
        0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c,
        0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c,
        0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33,
        0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37,
        0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37,
        0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c,
        0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c,
        0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33,
        0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37,
        0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37,
        0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c,
        0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c,
        0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x22, 0x5c,
        0x6e, 0x20, 0x20, 0x20, 0x20, 0x69, 0x6e, 0x66, 0x6f, 0x3a, 0x20,
        0x5c, 0x22, 0x5c, 0x5c, 0x30, 0x30, 0x35, 0x5c, 0x5c, 0x30, 0x30,
        0x30, 0x5c, 0x5c, 0x30, 0x30, 0x30, 0x5c, 0x5c, 0x30, 0x30, 0x30,
        0x5c, 0x5c, 0x30, 0x30, 0x34, 0x5c, 0x5c, 0x30, 0x30, 0x30, 0x5c,
        0x5c, 0x30, 0x30, 0x30, 0x2d, 0x5c, 0x5c, 0x30, 0x30, 0x30, 0x5c,
        0x22, 0x5c, 0x6e, 0x20, 0x20, 0x7d, 0x5c, 0x6e, 0x7d, 0x5c, 0x6e,
        0x22, 0x0a, 0x20, 0x20, 0x20, 0x20, 0x66, 0x72, 0x61, 0x6d, 0x65,
        0x3a, 0x20, 0x22, 0x21, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37,
        0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33,
        0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c,
        0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37,
        0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37,
        0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33,
        0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c,
        0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37,
        0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37,
        0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33,
        0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c,
        0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37,
        0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37,
        0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33,
        0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c,
        0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37,
        0x5c, 0x33, 0x37, 0x37, 0x22, 0x0a, 0x20, 0x20, 0x20, 0x20, 0x69,
        0x6e, 0x66, 0x6f, 0x3a, 0x20, 0x22, 0x5c, 0x30, 0x30, 0x35, 0x5c,
        0x30, 0x30, 0x30, 0x5c, 0x30, 0x30, 0x30, 0x5c, 0x30, 0x30, 0x30,
        0x5c, 0x30, 0x30, 0x34, 0x5c, 0x30, 0x30, 0x30, 0x5c, 0x30, 0x30,
        0x30, 0x2d, 0x5c, 0x30, 0x30, 0x30, 0x22, 0x0a, 0x20, 0x20, 0x7d,
        0x0a, 0x7d, 0x0a, 0x6c, 0x69, 0x73, 0x74, 0x20, 0x7b, 0x0a, 0x7d,
        0x0a, 0x6c, 0x69, 0x73, 0x74, 0x20, 0x7b, 0x0a, 0x7d, 0x0a, 0x6c,
        0x69, 0x73, 0x74, 0x20, 0x7b, 0x0a, 0x20, 0x20, 0x4e, 0x65, 0x77,
        0x20, 0x7b, 0x0a, 0x20, 0x20, 0x20, 0x20, 0x61, 0x62, 0x62, 0x72,
        0x65, 0x76, 0x3a, 0x20, 0x22, 0x5c, 0x30, 0x30, 0x35, 0x5c, 0x30,
        0x30, 0x30, 0x5c, 0x30, 0x30, 0x30, 0x5c, 0x30, 0x30, 0x30, 0x5c,
        0x30, 0x30, 0x34, 0x5c, 0x30, 0x30, 0x30, 0x5c, 0x30, 0x30, 0x30,
        0x2d, 0x5c, 0x30, 0x30, 0x30, 0x6c, 0x69, 0x73, 0x74, 0x20, 0x7b,
        0x5c, 0x6e, 0x20, 0x20, 0x4e, 0x65, 0x77, 0x20, 0x7b, 0x5c, 0x6e,
        0x20, 0x20, 0x20, 0x20, 0x61, 0x62, 0x62, 0x72, 0x65, 0x76, 0x3a,
        0x20, 0x5c, 0x22, 0x21, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c,
        0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33,
        0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37,
        0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37,
        0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c,
        0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c,
        0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33,
        0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37,
        0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37,
        0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c,
        0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c,
        0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33,
        0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37,
        0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37,
        0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c,
        0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c,
        0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33,
        0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37,
        0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37,
        0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x5c, 0x33, 0x37, 0x37, 0x5c,
        0x5c, 0x33, 0x37, 0x37, 0x5c, 0x22, 0x5c, 0x6e, 0x20, 0x20, 0x20,
        0x20, 0x69, 0x6e, 0x66, 0x6f, 0x3a, 0x20, 0x5c, 0x22, 0x5c, 0x5c,
        0x30, 0x30, 0x35, 0x5c, 0x5c, 0x30, 0x30, 0x30, 0x5c, 0x5c, 0x30,
        0x30, 0x30, 0x5c, 0x5c, 0x30, 0x30, 0x30, 0x5c, 0x5c, 0x30, 0x30,
        0x34, 0x5c, 0x5c, 0x30, 0x30, 0x30, 0x5c, 0x5c, 0x30, 0x30, 0x30,
        0x2d, 0x5c, 0x5c, 0x30, 0x30, 0x30, 0x5c, 0x22, 0x5c, 0x6e, 0x20,
        0x20, 0x7d, 0x5c, 0x6e, 0x7d, 0x5c, 0x6e, 0x22, 0x0a, 0x20, 0x20,
        0x20, 0x20, 0x66, 0x72, 0x61, 0x6d, 0x65, 0x3a, 0x20, 0x22, 0x21,
        0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37,
        0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33,
        0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c,
        0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37,
        0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37,
        0x37, 0x5c, 0x33, 0x37, 0x37, 0x5c, 0x33, 0x37, 0x37, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff}.slice();
    var aranges = new byte[]{0x2c}.slice();
    var frame = new byte[]{}.slice();
    var info = new byte[]{0x5, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x2d, 0x0, 0x5,
        0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x2d, 0x0}.slice();
    // The input above is malformed; the goal here it just to make sure
    // that we don't get a panic or other bad behavior while trying to
    // construct a dwarf.Data object from the input.  For good measure,
    // test to make sure we can handle the case where the input is
    // truncated as well.
    for (nint i = 0; i <= len(info); i++) {
        var truncated = info.slice(0, i);
        var (dw, err) = New(abbrev, aranges, frame, truncated, default!, default!, default!, default!);
        if (err == default!){
            Ꮡt.Errorf("expected error"u8);
        } else {
            if (dw != nil) {
                Ꮡt.Errorf("got non-nil dw, wanted nil"u8);
            }
        }
    }
}

public static void TestIssue52045(ж<testing.T> Ꮡt) {
    slice<byte> abbrev = default!;
    slice<byte> aranges = default!;
    slice<byte> frame = default!;
    slice<byte> line = default!;
    slice<byte> pubnames = default!;
    slice<byte> ranges = default!;
    slice<byte> str = default!;
    var info = new byte[]{0x7, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0}.slice();
    // A hand-crafted input corresponding to a minimal-size
    // .debug_info (header only, no DIEs) and an empty abbrev table.
    var (data0, _) = New(abbrev, aranges, frame, info, line, pubnames, ranges, str);
    var reader0 = data0.Reader();
    var (entry0, _) = reader0.SeekPC(0x0);
    // main goal is to make sure we can get here without crashing
    if (entry0 != nil) {
        Ꮡt.Errorf("got non-nil entry0, wanted nil"u8);
    }
}

} // end dwarf_test_package
