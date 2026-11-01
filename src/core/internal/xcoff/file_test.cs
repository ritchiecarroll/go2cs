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

internal static readonly ж<slice<fileTest>> ᏑfileTests = new StaticBox<slice<fileTest>>(slice<fileTest>([
    new(
        "testdata/gcc-ppc32-aix-dwarf2-exec",
        new FileHeader(U802TOCMAGIC),
        slice<ж<global::go.@internal.xcoff_package.SectionHeader>>([
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".text", 0x10000290, 0x00000bbd, STYP_TEXT, 0x7ae6, 0x36)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".data", 0x20000e4d, 0x00000437, STYP_DATA, 0x7d02, 0x2b)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".bss", 0x20001284, 0x0000021c, STYP_BSS, 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".loader", 0x00000000, 0x000004b3, STYP_LOADER, 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwline", 0x00000000, 0x000000df, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWLINE), 0x7eb0, 0x7)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwinfo", 0x00000000, 0x00000314, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWINFO), 0x7ef6, 0xa)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwabrev", 0x00000000, 0x000000d6, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWABREV), 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwarnge", 0x00000000, 0x00000020, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWARNGE), 0x7f5a, 0x2)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwloc", 0x00000000, 0x00000074, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWLOC), 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".debug", 0x00000000, 0x00005e4f, STYP_DEBUG, 0, 0))
        ]),
        slice<@string>(["libc.a/shr.o"])
    ),
    new(
        "testdata/gcc-ppc64-aix-dwarf2-exec",
        new FileHeader(U64_TOCMAGIC),
        slice<ж<global::go.@internal.xcoff_package.SectionHeader>>([
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".text", 0x10000480, 0x00000afd, STYP_TEXT, 0x8322, 0x34)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".data", 0x20000f7d, 0x000002f3, STYP_DATA, 0x85fa, 0x25)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".bss", 0x20001270, 0x00000428, STYP_BSS, 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".loader", 0x00000000, 0x00000535, STYP_LOADER, 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwline", 0x00000000, 0x000000b4, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWLINE), 0x8800, 0x4)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwinfo", 0x00000000, 0x0000036a, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWINFO), 0x8838, 0x7)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwabrev", 0x00000000, 0x000000b5, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWABREV), 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwarnge", 0x00000000, 0x00000040, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWARNGE), 0x889a, 0x2)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".dwloc", 0x00000000, 0x00000062, (uint32)((uint32)STYP_DWARF | (uint32)SSUBTYP_DWLOC), 0, 0)),
            Ꮡ(new global::go.@internal.xcoff_package.SectionHeader(".debug", 0x00000000, 0x00006605, STYP_DEBUG, 0, 0))
        ]),
        slice<@string>(["libc.a/shr_64.o"])
    )
]));
internal static ref slice<fileTest> fileTests => ref ((StaticBox<slice<fileTest>>)ᏑfileTests).ValueSlot;

public static void TestOpen(ж<testing.T> t) {
    foreach (var i in range(fileTests)) {
        var tt = Ꮡ(fileTests, i);

        var (f, err) = Open(tt.@file);
        if (err != nil) {
            t.Error(err);
            continue;
        }
        if (!reflect.DeepEqual(f.FileHeader, tt.hdr)) {
            t.Errorf("open %s:\n\thave %#v\n\twant %#v\n", tt.@file, f.FileHeader, tt.hdr);
            continue;
        }

        foreach (var (iΔ1, sh) in f.Sections) {
            if (iΔ1 >= len(tt.sections)) {
                break;
            }
            var have = sh.ᏑSectionHeader;
            var want = tt.sections[iΔ1];
            if (!reflect.DeepEqual(have.OrTypedNil(), want.OrTypedNil())) {
                t.Errorf("open %s, section %d:\n\thave %#v\n\twant %#v\n", tt.@file, iΔ1, have.OrTypedNil(), want.OrTypedNil());
            }
        }
        nint tn = len(tt.sections);
        nint fn = len(f.Sections);
        if (tn != fn) {
            t.Errorf("open %s: len(Sections) = %d, want %d", tt.@file, fn, tn);
        }
        var tl = tt.needed;
        (var fl, err) = f.ImportedLibraries();
        if (err != nil) {
            t.Error(err);
        }
        if (!slices.Equal<slice<@string>, @string>(tl, fl)) {
            t.Errorf("open %s: loader import = %v, want %v", tt.@file, tl, fl);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fileGoˢ => ref fileGoˢᵇ;
private static readonly @string fileGoˢᵇ = "file.go"u8;

public static void TestOpenFailure(ж<testing.T> t) {
    @string filename = fileGoˢ;    // not an XCOFF object file
    var (_, err) = Open(filename); // don't crash
    if (err == nil) {
        t.Errorf("open %s: succeeded unexpectedly", filename);
    }
}

} // end xcoff_internal_test_package
