// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using reflect = reflect_package;
using slices = slices_package;
using testing = testing_package;
using static go.@internal.xcoff_package;

partial class xcoff_internal_test_package {

internal partial struct fileTest {
    internal @string @file;
    internal global::go.@internal.xcoff_package.FileHeader hdr;
    internal slice<ж<global::go.@internal.xcoff_package.SectionHeader>> sections;
    internal slice<@string> needed;
}

internal static ж<slice<fileTest>> ᏑfileTests = new StandardBox<slice<fileTest>>(new fileTest[]{
    new(
        "testdata/gcc-ppc32-aix-dwarf2-exec"u8,
        new FileHeader(U802TOCMAGIC),
        new ж<global::go.@internal.xcoff_package.SectionHeader>[]{
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".text"u8, 0x10000290, 0x00000bbd, STYP_TEXT, 0x7ae6, 0x36)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".data"u8, 0x20000e4d, 0x00000437, STYP_DATA, 0x7d02, 0x2b)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".bss"u8, 0x20001284, 0x0000021c, STYP_BSS, 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".loader"u8, 0x00000000, 0x000004b3, STYP_LOADER, 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwline"u8, 0x00000000, 0x000000df, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWLINE), 0x7eb0, 0x7)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwinfo"u8, 0x00000000, 0x00000314, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWINFO), 0x7ef6, 0xa)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwabrev"u8, 0x00000000, 0x000000d6, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWABREV), 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwarnge"u8, 0x00000000, 0x00000020, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWARNGE), 0x7f5a, 0x2)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwloc"u8, 0x00000000, 0x00000074, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWLOC), 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".debug"u8, 0x00000000, 0x00005e4f, STYP_DEBUG, 0, 0))
        }.slice(),
        new @string[]{"libc.a/shr.o"u8}.slice()
    ),
    new(
        "testdata/gcc-ppc64-aix-dwarf2-exec"u8,
        new FileHeader(U64_TOCMAGIC),
        new ж<global::go.@internal.xcoff_package.SectionHeader>[]{
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".text"u8, 0x10000480, 0x00000afd, STYP_TEXT, 0x8322, 0x34)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".data"u8, 0x20000f7d, 0x000002f3, STYP_DATA, 0x85fa, 0x25)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".bss"u8, 0x20001270, 0x00000428, STYP_BSS, 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".loader"u8, 0x00000000, 0x00000535, STYP_LOADER, 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwline"u8, 0x00000000, 0x000000b4, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWLINE), 0x8800, 0x4)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwinfo"u8, 0x00000000, 0x0000036a, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWINFO), 0x8838, 0x7)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwabrev"u8, 0x00000000, 0x000000b5, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWABREV), 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwarnge"u8, 0x00000000, 0x00000040, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWARNGE), 0x889a, 0x2)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwloc"u8, 0x00000000, 0x00000062, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWLOC), 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".debug"u8, 0x00000000, 0x00006605, STYP_DEBUG, 0, 0))
        }.slice(),
        new @string[]{"libc.a/shr_64.o"u8}.slice()
    )
}.slice());
internal static ref slice<fileTest> fileTests => ref ᏑfileTests.ValueSlot;

public static void TestOpen(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in fileTests) {
        var tt = Ꮡ(fileTests, i);
        var (f, err) = Open((~tt).@file);
        if (err != default!) {
            Ꮡt.Error(err);
            continue;
        }
        if (!reflect.DeepEqual((~f).FileHeader, (~tt).hdr)) {
            Ꮡt.Errorf("open %s:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, (~f).FileHeader, (~tt).hdr);
            continue;
        }
        foreach (var (iΔ1, sh) in (~f).Sections) {
            if (iΔ1 >= len((~tt).sections)) {
                break;
            }
            var have = sh.of(global::go.@internal.xcoff_package.ΔSection.ᏑSectionHeader);
            var want = (~tt).sections[iΔ1];
            if (!reflect.DeepEqual(have.OrTypedNil(), want.OrTypedNil())) {
                Ꮡt.Errorf("open %s, section %d:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, iΔ1, have.OrTypedNil(), want.OrTypedNil());
            }
        }
        nint tn = len((~tt).sections);
        nint fn = len((~f).Sections);
        if (tn != fn) {
            Ꮡt.Errorf("open %s: len(Sections) = %d, want %d"u8, (~tt).@file, fn, tn);
        }
        var tl = tt.Value.needed;
        (var fl, err) = f.ImportedLibraries();
        if (err != default!) {
            Ꮡt.Error(err);
        }
        if (!slices.Equal<slice<@string>, @string>(tl, fl)) {
            Ꮡt.Errorf("open %s: loader import = %v, want %v"u8, (~tt).@file, tl, fl);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileGoˢ = "file.go"u8;

public static void TestOpenFailure(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string filename = fileGoˢ; // not an XCOFF object file
    var (_, err) = Open(filename); // don't crash
    if (err == default!) {
        Ꮡt.Errorf("open %s: succeeded unexpectedly"u8, filename);
    }
}

} // end xcoff_internal_test_package
