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

internal static readonly ж<slice<fileTest>> ᏑfileTests = new StaticBox<slice<fileTest>>(slice<fileTest>([
    new(
        "testdata/386-plan9-exec",
        new FileHeader(Magic386, 0x324, 0x14, 4, 0x1000, 32),
        slice<ж<global::go.debug.plan9obj_package.SectionHeader>>([
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("text", 0x4c5f, 0x20)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("data", 0x94c, 0x4c7f)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("syms", 0x2c2b, 0x55cb)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("spsz", 0x0, 0x81f6)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("pcsz", 0xf7a, 0x81f6))
        ])
    ),
    new(
        "testdata/amd64-plan9-exec",
        new FileHeader(MagicAMD64, 0x618, 0x13, 8, 0x200000, 40),
        slice<ж<global::go.debug.plan9obj_package.SectionHeader>>([
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("text", 0x4213, 0x28)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("data", 0xa80, 0x423b)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("syms", 0x2c8c, 0x4cbb)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("spsz", 0x0, 0x7947)),
            Ꮡ(new global::go.debug.plan9obj_package.SectionHeader("pcsz", 0xca0, 0x7947))
        ])
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
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fileGoˢ => ref fileGoˢᵇ;
private static readonly @string fileGoˢᵇ = "file.go"u8;

public static void TestOpenFailure(ж<testing.T> t) {
    @string filename = fileGoˢ;    // not a Plan 9 a.out file
    var (_, err) = Open(filename); // don't crash
    if (err == nil) {
        t.Errorf("open %s: succeeded unexpectedly", filename);
    }
}

} // end plan9obj_internal_test_package
