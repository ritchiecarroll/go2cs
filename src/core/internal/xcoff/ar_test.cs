// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using reflect = reflect_package;
using testing = testing_package;
using static go.@internal.xcoff_package;

partial class xcoff_internal_test_package {

internal partial struct archiveTest {
    internal @string @file;
    internal global::go.@internal.xcoff_package.ArchiveHeader hdr;
    internal slice<ж<global::go.@internal.xcoff_package.MemberHeader>> members;
    internal slice<global::go.@internal.xcoff_package.FileHeader> membersFileHeader;
}

internal static readonly ж<slice<archiveTest>> ᏑarchTest = new StaticBox<slice<archiveTest>>(default(slice<archiveTest>));
internal static ref slice<archiveTest> archTest => ref ((StaticBox<slice<archiveTest>>)ᏑarchTest).ValueSlot;
internal static void initᴛarchTest() { archTest = [
    new(
        "testdata/bigar-ppc64",
        new ArchiveHeader(AIAMAGBIG),
        [
            Ꮡ(new global::go.@internal.xcoff_package.MemberHeader("printbye.o", 836)),
            Ꮡ(new global::go.@internal.xcoff_package.MemberHeader("printhello.o", 860))
        ],
        [
            new(U64_TOCMAGIC),
            new(U64_TOCMAGIC)
        ]
    ),
    new(
        "testdata/bigar-empty",
        new ArchiveHeader(AIAMAGBIG),
        [],
        []
    )
]; }

public static void TestOpenArchive(ж<testing.T> t) {
    foreach (var i in range(len(archTest))) {
        var tt = Ꮡ(archTest, i);
        var (arch, err) = OpenArchive(tt.@file);
        if (err != nil) {
            t.Error(err);
            continue;
        }
        if (!reflect.DeepEqual(arch.ArchiveHeader, tt.hdr)) {
            t.Errorf("open archive %s:\n\thave %#v\n\twant %#v\n", tt.@file, arch.ArchiveHeader, tt.hdr);
            continue;
        }

        foreach (var (iΔ1, mem) in range(arch.Members)) {
            if (iΔ1 >= len(tt.members)) {
                break;
            }
            var have = mem.ᏑMemberHeader;
            var want = tt.members[iΔ1];
            if (!reflect.DeepEqual(have.OrTypedNil(), want.OrTypedNil())) {
                t.Errorf("open %s, member %d:\n\thave %#v\n\twant %#v\n", tt.@file, iΔ1, have.OrTypedNil(), want.OrTypedNil());
            }

            var (f, errΔ1) = arch.GetFile(mem.Name);
            if (errΔ1 != nil) {
                t.Error(errΔ1);
                continue;
            }
            if (!reflect.DeepEqual(f.FileHeader, tt.membersFileHeader[iΔ1])) {
                t.Errorf("open %s, member file header %d:\n\thave %#v\n\twant %#v\n", tt.@file, iΔ1, f.FileHeader, tt.membersFileHeader[iΔ1]);
            }
        }
        nint tn = len(tt.members);
        nint an = len(arch.Members);
        if (tn != an) {
            t.Errorf("open %s: len(Members) = %d, want %d", tt.@file, an, tn);
        }
    }
}

} // end xcoff_internal_test_package
