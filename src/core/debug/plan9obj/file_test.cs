// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using reflect = reflect_package;
using testing = testing_package;
using static go.debug.plan9obj_package;

partial class plan9obj_internal_test_package {

internal partial struct fileTest {
    internal @string @file;
    internal global::go.debug.plan9obj_package.FileHeader hdr;
    internal slice<ж<global::go.debug.plan9obj_package.SectionHeader>> sections;
}

internal static ж<slice<fileTest>> ᏑfileTests = new StandardBox<slice<fileTest>>(new fileTest[]{
    new(
        "testdata/386-plan9-exec"u8,
        new FileHeader(Magic386, 0x324, 0x14, 4, 0x1000, 32),
        new ж<global::go.debug.plan9obj_package.SectionHeader>[]{
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("text"u8, 0x4c5f, 0x20)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("data"u8, 0x94c, 0x4c7f)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("syms"u8, 0x2c2b, 0x55cb)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("spsz"u8, 0x0, 0x81f6)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("pcsz"u8, 0xf7a, 0x81f6))
        }.slice()
    ),
    new(
        "testdata/amd64-plan9-exec"u8,
        new FileHeader(MagicAMD64, 0x618, 0x13, 8, 0x200000, 40),
        new ж<global::go.debug.plan9obj_package.SectionHeader>[]{
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("text"u8, 0x4213, 0x28)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("data"u8, 0xa80, 0x423b)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("syms"u8, 0x2c8c, 0x4cbb)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("spsz"u8, 0x0, 0x7947)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("pcsz"u8, 0xca0, 0x7947))
        }.slice()
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
            var have = sh.of(global::go.debug.plan9obj_package.ΔSection.ᏑSectionHeader);
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
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fileGoˢ = "file.go"u8;

public static void TestOpenFailure(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string filename = fileGoˢ; // not a Plan 9 a.out file
    var (_, err) = Open(filename); // don't crash
    if (err == default!) {
        Ꮡt.Errorf("open %s: succeeded unexpectedly"u8, filename);
    }
}

} // end plan9obj_internal_test_package
