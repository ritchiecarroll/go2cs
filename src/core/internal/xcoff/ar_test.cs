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

internal static ж<slice<archiveTest>> ᏑarchTest = new StandardBox<slice<archiveTest>>(default(slice<archiveTest>));
internal static ref slice<archiveTest> archTest => ref ᏑarchTest.ValueSlot;
internal static void initᴛarchTest() { archTest = new archiveTest[]{
    new(
        "testdata/bigar-ppc64"u8,
        new ArchiveHeader(AIAMAGBIG),
        new ж<global::go.@internal.xcoff_package.MemberHeader>[]{
            Ꮡ(new global::go.@internal.xcoff_package.MemberHeader("printbye.o"u8, 836)),
            Ꮡ(new global::go.@internal.xcoff_package.MemberHeader("printhello.o"u8, 860))
        }.slice(),
        new global::go.@internal.xcoff_package.FileHeader[]{
            new(U64_TOCMAGIC),
            new(U64_TOCMAGIC)
        }.slice()
    ),
    new(
        "testdata/bigar-empty"u8,
        new ArchiveHeader(AIAMAGBIG),
        new ж<global::go.@internal.xcoff_package.MemberHeader>[]{}.slice(),
        new global::go.@internal.xcoff_package.FileHeader[]{}.slice()
    )
}.slice(); }

public static void TestOpenArchive(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in archTest) {
        var tt = Ꮡ(archTest, i);
        var (arch, err) = OpenArchive((~tt).@file);
        if (err != default!) {
            Ꮡt.Error(err);
            continue;
        }
        if (!reflect.DeepEqual((~arch).ArchiveHeader, (~tt).hdr)) {
            Ꮡt.Errorf("open archive %s:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, (~arch).ArchiveHeader, (~tt).hdr);
            continue;
        }
        foreach (var (iΔ1, mem) in (~arch).Members) {
            if (iΔ1 >= len((~tt).members)) {
                break;
            }
            var have = mem.of(global::go.@internal.xcoff_package.Member.ᏑMemberHeader);
            var want = (~tt).members[iΔ1];
            if (!reflect.DeepEqual(have.OrTypedNil(), want.OrTypedNil())) {
                Ꮡt.Errorf("open %s, member %d:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, iΔ1, have.OrTypedNil(), want.OrTypedNil());
            }
            var (f, errΔ1) = arch.GetFile((~mem).Name);
            if (errΔ1 != default!) {
                Ꮡt.Error(errΔ1);
                continue;
            }
            if (!reflect.DeepEqual((~f).FileHeader, (~tt).membersFileHeader[iΔ1])) {
                Ꮡt.Errorf("open %s, member file header %d:\n\thave %#v\n\twant %#v\n"u8, (~tt).@file, iΔ1, (~f).FileHeader, (~tt).membersFileHeader[iΔ1]);
            }
        }
        nint tn = len((~tt).members);
        nint an = len((~arch).Members);
        if (tn != an) {
            Ꮡt.Errorf("open %s: len(Members) = %d, want %d"u8, (~tt).@file, an, tn);
        }
    }
}

} // end xcoff_internal_test_package
