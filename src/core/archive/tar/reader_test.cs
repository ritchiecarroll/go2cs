// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using TestFileReader_testFnc = object;
global using TestFileReader_fileMaker = object;

namespace go.archive;

using bytes = bytes_package;
using bzip2 = compress.bzip2_package;
using errors = errors_package;
using fmt = fmt_package;
using crc32 = hash.crc32_package;
using obscuretestdata = @internal.obscuretestdata_package;
using io = io_package;
using maps = maps_package;
using math = math_package;
using os = os_package;
using path = path_package;
using reflect = reflect_package;
using slices = slices_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using @internal;
using compress;
using hash;
using hash = hash_package;
using static go.archive.tar_package;
using ꓸꓸꓸstring = Span<@string>;

partial class tar_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string longˢ => ref longˢᵇ;
private static readonly @string longˢᵇ = "long"u8;
internal static ref readonly @string base64ˢ => ref base64ˢᵇ;
private static readonly @string base64ˢᵇ = ".base64"u8;
internal static ref readonly @string bz2ˢ => ref bz2ˢᵇ;
private static readonly @string bz2ˢᵇ = ".bz2"u8;
internal static ref readonly @string bz2Base64ˢ => ref bz2Base64ˢᵇ;
private static readonly @string bz2Base64ˢᵇ = ".bz2.base64"u8;

internal partial struct TestReader_vectors /*dyn*/ {
    internal @string @file;                                           // Test input file
    internal bool obscured;                                           // Obscured with obscuretestdata package
    internal slice<ж<global::go.archive.tar_package.Header>> headers; // Expected output headers
    internal slice<@string> chksums;                                  // CRC32 checksum of files, leave as nil if not checked
    internal error err;                                               // Expected error to occur
}

public static void TestReader(ж<testing.T> t) {
    var vectors = slice<TestReader_vectors>([new(
        @file: "testdata/gnu.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small.txt",
            Mode: 416,
            Uid: 73025,
            Gid: 5000,
            Size: 5,
            ModTime: time.Unix(1244428340, 0),
            Typeflag: (rune)'0',
            Uname: "dsymonds",
            Gname: "eng",
            Format: FormatGNU)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small2.txt",
            Mode: 416,
            Uid: 73025,
            Gid: 5000,
            Size: 11,
            ModTime: time.Unix(1244436044, 0),
            Typeflag: (rune)'0',
            Uname: "dsymonds",
            Gname: "eng",
            Format: FormatGNU))
        ]),
        chksums: slice<@string>([
            "6cbd88fc",
            "ddac04b3"
        ])
    ), new(
        @file: "testdata/sparse-formats.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse-gnu",
            Mode: 420,
            Uid: 1000,
            Gid: 1000,
            Size: 200,
            ModTime: time.Unix(1392395740, 0),
            Typeflag: 0x53,
            Linkname: "",
            Uname: "david",
            Gname: "david",
            Devmajor: 0,
            Devminor: 0,
            Format: FormatGNU)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse-posix-0.0",
            Mode: 420,
            Uid: 1000,
            Gid: 1000,
            Size: 200,
            ModTime: time.Unix(1392342187, 0),
            Typeflag: 0x30,
            Linkname: "",
            Uname: "david",
            Gname: "david",
            Devmajor: 0,
            Devminor: 0,
            PAXRecords: new map<@string, @string>{
                ["GNU.sparse.size"] = "200",
                ["GNU.sparse.numblocks"] = "95",
                ["GNU.sparse.map"] = "1,1,3,1,5,1,7,1,9,1,11,1,13,1,15,1,17,1,19,1,21,1,23,1,25,1,27,1,29,1,31,1,33,1,35,1,37,1,39,1,41,1,43,1,45,1,47,1,49,1,51,1,53,1,55,1,57,1,59,1,61,1,63,1,65,1,67,1,69,1,71,1,73,1,75,1,77,1,79,1,81,1,83,1,85,1,87,1,89,1,91,1,93,1,95,1,97,1,99,1,101,1,103,1,105,1,107,1,109,1,111,1,113,1,115,1,117,1,119,1,121,1,123,1,125,1,127,1,129,1,131,1,133,1,135,1,137,1,139,1,141,1,143,1,145,1,147,1,149,1,151,1,153,1,155,1,157,1,159,1,161,1,163,1,165,1,167,1,169,1,171,1,173,1,175,1,177,1,179,1,181,1,183,1,185,1,187,1,189,1"
            },
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse-posix-0.1",
            Mode: 420,
            Uid: 1000,
            Gid: 1000,
            Size: 200,
            ModTime: time.Unix(1392340456, 0),
            Typeflag: 0x30,
            Linkname: "",
            Uname: "david",
            Gname: "david",
            Devmajor: 0,
            Devminor: 0,
            PAXRecords: new map<@string, @string>{
                ["GNU.sparse.size"] = "200",
                ["GNU.sparse.numblocks"] = "95",
                ["GNU.sparse.map"] = "1,1,3,1,5,1,7,1,9,1,11,1,13,1,15,1,17,1,19,1,21,1,23,1,25,1,27,1,29,1,31,1,33,1,35,1,37,1,39,1,41,1,43,1,45,1,47,1,49,1,51,1,53,1,55,1,57,1,59,1,61,1,63,1,65,1,67,1,69,1,71,1,73,1,75,1,77,1,79,1,81,1,83,1,85,1,87,1,89,1,91,1,93,1,95,1,97,1,99,1,101,1,103,1,105,1,107,1,109,1,111,1,113,1,115,1,117,1,119,1,121,1,123,1,125,1,127,1,129,1,131,1,133,1,135,1,137,1,139,1,141,1,143,1,145,1,147,1,149,1,151,1,153,1,155,1,157,1,159,1,161,1,163,1,165,1,167,1,169,1,171,1,173,1,175,1,177,1,179,1,181,1,183,1,185,1,187,1,189,1",
                ["GNU.sparse.name"] = "sparse-posix-0.1"
            },
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse-posix-1.0",
            Mode: 420,
            Uid: 1000,
            Gid: 1000,
            Size: 200,
            ModTime: time.Unix(1392337404, 0),
            Typeflag: 0x30,
            Linkname: "",
            Uname: "david",
            Gname: "david",
            Devmajor: 0,
            Devminor: 0,
            PAXRecords: new map<@string, @string>{
                ["GNU.sparse.major"] = "1",
                ["GNU.sparse.minor"] = "0",
                ["GNU.sparse.realsize"] = "200",
                ["GNU.sparse.name"] = "sparse-posix-1.0"
            },
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "end",
            Mode: 420,
            Uid: 1000,
            Gid: 1000,
            Size: 4,
            ModTime: time.Unix(1392398319, 0),
            Typeflag: 0x30,
            Linkname: "",
            Uname: "david",
            Gname: "david",
            Devmajor: 0,
            Devminor: 0,
            Format: FormatGNU))
        ]),
        chksums: slice<@string>([
            "5375e1d2",
            "5375e1d2",
            "5375e1d2",
            "5375e1d2",
            "8eb179ba"
        ])
    ), new(
        @file: "testdata/star.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small.txt",
            Mode: 416,
            Uid: 73025,
            Gid: 5000,
            Size: 5,
            ModTime: time.Unix(1244592783, 0),
            Typeflag: (rune)'0',
            Uname: "dsymonds",
            Gname: "eng",
            AccessTime: time.Unix(1244592783, 0),
            ChangeTime: time.Unix(1244592783, 0))), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small2.txt",
            Mode: 416,
            Uid: 73025,
            Gid: 5000,
            Size: 11,
            ModTime: time.Unix(1244592783, 0),
            Typeflag: (rune)'0',
            Uname: "dsymonds",
            Gname: "eng",
            AccessTime: time.Unix(1244592783, 0),
            ChangeTime: time.Unix(1244592783, 0)))
        ])
    ), new(
        @file: "testdata/v7.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small.txt",
            Mode: 292,
            Uid: 73025,
            Gid: 5000,
            Size: 5,
            ModTime: time.Unix(1244593104, 0),
            Typeflag: (rune)'0')), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small2.txt",
            Mode: 292,
            Uid: 73025,
            Gid: 5000,
            Size: 11,
            ModTime: time.Unix(1244593104, 0),
            Typeflag: (rune)'0'))
        ])
    ), new(
        @file: "testdata/pax.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "a/123456789101112131415161718192021222324252627282930313233343536373839404142434445464748495051525354555657585960616263646566676869707172737475767778798081828384858687888990919293949596979899100",
            Mode: 436,
            Uid: 1000,
            Gid: 1000,
            Uname: "shane",
            Gname: "shane",
            Size: 7,
            ModTime: time.Unix(1350244992, 23960108),
            ChangeTime: time.Unix(1350244992, 23960108),
            AccessTime: time.Unix(1350244992, 23960108),
            Typeflag: TypeReg,
            PAXRecords: new map<@string, @string>{
                ["path"] = "a/123456789101112131415161718192021222324252627282930313233343536373839404142434445464748495051525354555657585960616263646566676869707172737475767778798081828384858687888990919293949596979899100",
                ["mtime"] = "1350244992.023960108",
                ["atime"] = "1350244992.023960108",
                ["ctime"] = "1350244992.023960108"
            },
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "a/b",
            Mode: 511,
            Uid: 1000,
            Gid: 1000,
            Uname: "shane",
            Gname: "shane",
            Size: 0,
            ModTime: time.Unix(1350266320, 910238425),
            ChangeTime: time.Unix(1350266320, 910238425),
            AccessTime: time.Unix(1350266320, 910238425),
            Typeflag: TypeSymlink,
            Linkname: "123456789101112131415161718192021222324252627282930313233343536373839404142434445464748495051525354555657585960616263646566676869707172737475767778798081828384858687888990919293949596979899100",
            PAXRecords: new map<@string, @string>{
                ["linkpath"] = "123456789101112131415161718192021222324252627282930313233343536373839404142434445464748495051525354555657585960616263646566676869707172737475767778798081828384858687888990919293949596979899100",
                ["mtime"] = "1350266320.910238425",
                ["atime"] = "1350266320.910238425",
                ["ctime"] = "1350266320.910238425"
            },
            Format: FormatPAX))
        ])
    ), new(
        @file: "testdata/pax-bad-hdr-file.tar",
        err: ErrHeader
    ), new(
        @file: "testdata/pax-bad-hdr-large.tar.bz2",
        err: ErrFieldTooLong
    ), new(
        @file: "testdata/pax-bad-mtime-file.tar",
        err: ErrHeader
    ), new(
        @file: "testdata/pax-pos-size-file.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "foo",
            Mode: 416,
            Uid: 319973,
            Gid: 5000,
            Size: 999,
            ModTime: time.Unix(1442282516, 0),
            Typeflag: (rune)'0',
            Uname: "joetsai",
            Gname: "eng",
            PAXRecords: new map<@string, @string>{
                ["size"] = "000000000000000000000999"
            },
            Format: FormatPAX))
        ]),
        chksums: slice<@string>([
            "5fd7e86a"
        ])
    ), new(
        @file: "testdata/pax-records.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeReg,
            Name: "file",
            Uname: strings.Repeat(longˢ, 10),
            ModTime: time.Unix(0, 0),
            PAXRecords: new map<@string, @string>{
                ["GOLANG.pkg"] = "tar",
                ["comment"] = "Hello, 世界",
                ["uname"] = strings.Repeat(longˢ, 10)
            },
            Format: FormatPAX))
        ])
    ), new(
        @file: "testdata/pax-global-records.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeXGlobalHeader,
            Name: "global1",
            PAXRecords: new map<@string, @string>{["path"] = "global1", ["mtime"] = "1500000000.0"},
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeReg,
            Name: "file1",
            ModTime: time.Unix(0, 0),
            Format: FormatUSTAR)), Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeReg,
            Name: "file2",
            PAXRecords: new map<@string, @string>{["path"] = "file2"},
            ModTime: time.Unix(0, 0),
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeXGlobalHeader,
            Name: "GlobalHead.0.0",
            PAXRecords: new map<@string, @string>{["path"] = ""},
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeReg,
            Name: "file3",
            ModTime: time.Unix(0, 0),
            Format: FormatUSTAR)), Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeReg,
            Name: "file4",
            ModTime: time.Unix(1400000000, 0),
            PAXRecords: new map<@string, @string>{["mtime"] = "1400000000"},
            Format: FormatPAX))
        ])
    ), new(
        @file: "testdata/nil-uid.tar", // golang.org/issue/5290
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "P1050238.JPG.log",
            Mode: 436,
            Uid: 0,
            Gid: 0,
            Size: 14,
            ModTime: time.Unix(1365454838, 0),
            Typeflag: TypeReg,
            Linkname: "",
            Uname: "eyefi",
            Gname: "eyefi",
            Devmajor: 0,
            Devminor: 0,
            Format: FormatGNU))
        ])
    ), new(
        @file: "testdata/xattrs.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small.txt",
            Mode: 420,
            Uid: 1000,
            Gid: 10,
            Size: 5,
            ModTime: time.Unix(1386065770, 448252320),
            Typeflag: (rune)'0',
            Uname: "alex",
            Gname: "wheel",
            AccessTime: time.Unix(1389782991, 419875220),
            ChangeTime: time.Unix(1389782956, 794414986),
            Xattrs: new map<@string, @string>{
                ["user.key"] = "value",
                ["user.key2"] = "value2",
                // Interestingly, selinux encodes the terminating null inside the xattr
                ["security.selinux"] = "unconfined_u:object_r:default_t:s0\x00"
            },
            PAXRecords: new map<@string, @string>{
                ["mtime"] = "1386065770.44825232",
                ["atime"] = "1389782991.41987522",
                ["ctime"] = "1389782956.794414986",
                ["SCHILY.xattr.user.key"] = "value",
                ["SCHILY.xattr.user.key2"] = "value2",
                ["SCHILY.xattr.security.selinux"] = "unconfined_u:object_r:default_t:s0\x00"
            },
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small2.txt",
            Mode: 420,
            Uid: 1000,
            Gid: 10,
            Size: 11,
            ModTime: time.Unix(1386065770, 449252304),
            Typeflag: (rune)'0',
            Uname: "alex",
            Gname: "wheel",
            AccessTime: time.Unix(1389782991, 419875220),
            ChangeTime: time.Unix(1386065770, 449252304),
            Xattrs: new map<@string, @string>{
                ["security.selinux"] = "unconfined_u:object_r:default_t:s0\x00"
            },
            PAXRecords: new map<@string, @string>{
                ["mtime"] = "1386065770.449252304",
                ["atime"] = "1389782991.41987522",
                ["ctime"] = "1386065770.449252304",
                ["SCHILY.xattr.security.selinux"] = "unconfined_u:object_r:default_t:s0\x00"
            },
            Format: FormatPAX))
        ])
    ), new(
        // Matches the behavior of GNU, BSD, and STAR tar utilities.
        @file: "testdata/gnu-multi-hdrs.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "GNU2/GNU2/long-path-name",
            Linkname: "GNU4/GNU4/long-linkpath-name",
            ModTime: time.Unix(0, 0),
            Typeflag: (rune)'2',
            Format: FormatGNU))
        ])
    ), new(
        // GNU tar file with atime and ctime fields set.
        // Created with the GNU tar v1.27.1.
        //	tar --incremental -S -cvf gnu-incremental.tar test2
        @file: "testdata/gnu-incremental.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "test2/",
            Mode: 16877,
            Uid: 1000,
            Gid: 1000,
            Size: 14,
            ModTime: time.Unix(1441973427, 0),
            Typeflag: (rune)'D',
            Uname: "rawr",
            Gname: "dsnet",
            AccessTime: time.Unix(1441974501, 0),
            ChangeTime: time.Unix(1441973436, 0),
            Format: FormatGNU)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "test2/foo",
            Mode: 33188,
            Uid: 1000,
            Gid: 1000,
            Size: 64,
            ModTime: time.Unix(1441973363, 0),
            Typeflag: (rune)'0',
            Uname: "rawr",
            Gname: "dsnet",
            AccessTime: time.Unix(1441974501, 0),
            ChangeTime: time.Unix(1441973436, 0),
            Format: FormatGNU)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "test2/sparse",
            Mode: 33188,
            Uid: 1000,
            Gid: 1000,
            Size: 536870912,
            ModTime: time.Unix(1441973427, 0),
            Typeflag: (rune)'S',
            Uname: "rawr",
            Gname: "dsnet",
            AccessTime: time.Unix(1441991948, 0),
            ChangeTime: time.Unix(1441973436, 0),
            Format: FormatGNU))
        ])
    ), new(
        // Matches the behavior of GNU and BSD tar utilities.
        @file: "testdata/pax-multi-hdrs.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "bar",
            Linkname: "PAX4/PAX4/long-linkpath-name",
            ModTime: time.Unix(0, 0),
            Typeflag: (rune)'2',
            PAXRecords: new map<@string, @string>{
                ["linkpath"] = "PAX4/PAX4/long-linkpath-name"
            },
            Format: FormatPAX))
        ])
    ), new(
        // Both BSD and GNU tar truncate long names at first NUL even
        // if there is data following that NUL character.
        // This is reasonable as GNU long names are C-strings.
        @file: "testdata/gnu-long-nul.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "0123456789",
            Mode: 420,
            Uid: 1000,
            Gid: 1000,
            ModTime: time.Unix(1486082191, 0),
            Typeflag: (rune)'0',
            Uname: "rawr",
            Gname: "dsnet",
            Format: FormatGNU))
        ])
    ), new(
        // This archive was generated by Writer but is readable by both
        // GNU and BSD tar utilities.
        // The archive generated by GNU is nearly byte-for-byte identical
        // to the Go version except the Go version sets a negative Devminor
        // just to force the GNU format.
        @file: "testdata/gnu-utf8.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹",
            Mode: 420,
            Uid: 1000, Gid: 1000,
            ModTime: time.Unix(0, 0),
            Typeflag: (rune)'0',
            Uname: "☺",
            Gname: "⚹",
            Format: FormatGNU))
        ])
    ), new(
        // This archive was generated by Writer but is readable by both
        // GNU and BSD tar utilities.
        // The archive generated by GNU is nearly byte-for-byte identical
        // to the Go version except the Go version sets a negative Devminor
        // just to force the GNU format.
        @file: "testdata/gnu-not-utf8.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: ((@string)(new byte[]{0x68, 0x69, 0x80, 0x81, 0x82, 0x83, 0x62, 0x79, 0x65})),
            Mode: 420,
            Uid: 1000,
            Gid: 1000,
            ModTime: time.Unix(0, 0),
            Typeflag: (rune)'0',
            Uname: "rawr",
            Gname: "dsnet",
            Format: FormatGNU))
        ])
    ), new(
        // BSD tar v3.1.2 and GNU tar v1.27.1 both rejects PAX records
        // with NULs in the key.
        @file: "testdata/pax-nul-xattrs.tar",
        err: ErrHeader
    ), new(
        // BSD tar v3.1.2 rejects a PAX path with NUL in the value, while
        // GNU tar v1.27.1 simply truncates at first NUL.
        // We emulate the behavior of BSD since it is strange doing NUL
        // truncations since PAX records are length-prefix strings instead
        // of NUL-terminated C-strings.
        @file: "testdata/pax-nul-path.tar",
        err: ErrHeader
    ), new(
        @file: "testdata/neg-size.tar.base64",
        obscured: true,
        err: ErrHeader
    ), new(
        @file: "testdata/issue10968.tar",
        err: ErrHeader
    ), new(
        @file: "testdata/issue11169.tar",
        err: ErrHeader
    ), new(
        @file: "testdata/issue12435.tar",
        err: ErrHeader
    ), new(
        // Ensure that we can read back the original Header as written with
        // a buggy pre-Go1.8 tar.Writer.
        @file: "testdata/invalid-go17.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa/foo",
            Uid: 2097152,
            ModTime: time.Unix(0, 0),
            Typeflag: (rune)'0'))
        ])
    ), new(
        // USTAR archive with a regular entry with non-zero device numbers.
        @file: "testdata/ustar-file-devs.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "file",
            Mode: 420,
            Typeflag: (rune)'0',
            ModTime: time.Unix(0, 0),
            Devmajor: 1,
            Devminor: 1,
            Format: FormatUSTAR))
        ])
    ), new(
        // Generated by Go, works on BSD tar v3.1.2 and GNU tar v.1.27.1.
        @file: "testdata/gnu-nil-sparse-data.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse.db",
            Typeflag: TypeGNUSparse,
            Size: 1000,
            ModTime: time.Unix(0, 0),
            Format: FormatGNU))
        ])
    ), new(
        // Generated by Go, works on BSD tar v3.1.2 and GNU tar v.1.27.1.
        @file: "testdata/gnu-nil-sparse-hole.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse.db",
            Typeflag: TypeGNUSparse,
            Size: 1000,
            ModTime: time.Unix(0, 0),
            Format: FormatGNU))
        ])
    ), new(
        // Generated by Go, works on BSD tar v3.1.2 and GNU tar v.1.27.1.
        @file: "testdata/pax-nil-sparse-data.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse.db",
            Typeflag: TypeReg,
            Size: 1000,
            ModTime: time.Unix(0, 0),
            PAXRecords: new map<@string, @string>{
                ["size"] = "1512",
                ["GNU.sparse.major"] = "1",
                ["GNU.sparse.minor"] = "0",
                ["GNU.sparse.realsize"] = "1000",
                ["GNU.sparse.name"] = "sparse.db"
            },
            Format: FormatPAX))
        ])
    ), new(
        // Generated by Go, works on BSD tar v3.1.2 and GNU tar v.1.27.1.
        @file: "testdata/pax-nil-sparse-hole.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse.db",
            Typeflag: TypeReg,
            Size: 1000,
            ModTime: time.Unix(0, 0),
            PAXRecords: new map<@string, @string>{
                ["size"] = "512",
                ["GNU.sparse.major"] = "1",
                ["GNU.sparse.minor"] = "0",
                ["GNU.sparse.realsize"] = "1000",
                ["GNU.sparse.name"] = "sparse.db"
            },
            Format: FormatPAX))
        ])
    ), new(
        @file: "testdata/trailing-slash.tar",
        headers: slice<ж<global::go.archive.tar_package.Header>>([Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeDir,
            Name: strings.Repeat("123456789/", 30),
            ModTime: time.Unix(0, 0),
            PAXRecords: new map<@string, @string>{
                ["path"] = strings.Repeat("123456789/", 30)
            },
            Format: FormatPAX))
        ])
    ), new(
        // Small compressed file that uncompresses to
        // a file with a very large GNU 1.0 sparse map.
        @file: "testdata/gnu-sparse-many-zeros.tar.bz2",
        err: errSparseTooLong
    )
    ]);

    foreach (var (_, vᴛ1) in vectors) {
        ref var v = ref heap(new TestReader_vectors(), out var Ꮡv);
        v = vᴛ1;

        var vʗ1 = v;
        t.Run(strings.TrimSuffix(path.Base(v.@file), base64ˢ), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var hdrsᴷ = new stackAppend<ж<global::go.archive.tar_package.Header>>(moves: true);
                var chksumsᴷ = new stackAppend<@string>(moves: true);
                @string pathΔ1 = vʗ1.@file;
                if (vʗ1.obscured) {
                    var (tf, errΔ1) = obscuretestdata.DecodeToTempFile(pathΔ1);
                    if (errΔ1 != nil) {
                        tΔ1.Fatalf("obscuredtestdata.DecodeToTempFile(%s): %v", pathΔ1, errΔ1);
                    }
                    pathΔ1 = tf;
                }

                var (f, err) = os.Open(pathΔ1);
                if (err != nil) {
                    tΔ1.Fatalf("unexpected error: %v", err);
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);

                io.Reader fr = new tar_test_package.os_FileжReader(f);
                if (strings.HasSuffix(vʗ1.@file, bz2ˢ) || strings.HasSuffix(vʗ1.@file, bz2Base64ˢ)) {
                    fr = bzip2.NewReader(fr);
                }

                // Capture all headers and checksums.
                ж<global::go.archive.tar_package.Reader> tr = NewReader(fr);
                slice<ж<global::go.archive.tar_package.Header>> hdrs = default;
                slice<@string> chksums = default;
                slice<byte> rdbuf = new slice<byte>(8);
                for (;;) {
                    ж<global::go.archive.tar_package.Header> hdr = default;
                    (hdr, err) = tr.Next();
                    if (err != nil) {
                        if (AreEqual(err, io.EOF)) {
                            err = default; // Expected error
                        }
                        break;
                    }
                    hdrs = append(hdrsᴷ, hdrs, hdr);

                    if (vʗ1.chksums == nil) {
                        continue;
                    }
                    var h = crc32.NewIEEE();
                    (_, err) = io.CopyBuffer(new tar_test_package.hash_Hash32ᴠWriter(h), new global::go.archive.tar_package.ReaderжReader(tr), rdbuf); // Effectively an incremental read
                    if (err != nil) {
                        break;
                    }
                    chksums = append(chksumsᴷ, chksums, fmt.Sprintf("%x"u8, h.Sum(default)));
                }

                hdrs = move2heap(hdrsᴷ, hdrs);
                foreach (var (i, hdr) in hdrs) {
                    if (i >= len(vʗ1.headers)) {
                        tΔ1.Fatalf("entry %d: unexpected header:\ngot %+v", i, hdr.Value);
                    }
                    if (!reflect.DeepEqual(hdr.Value, vʗ1.headers[i].Value)) {
                        tΔ1.Fatalf("entry %d: incorrect header:\ngot  %+v\nwant %+v", i, hdr.Value, vʗ1.headers[i].Value);
                    }
                }
                if (len(hdrs) != len(vʗ1.headers)) {
                    tΔ1.Fatalf("got %d headers, want %d headers", len(hdrs), len(vʗ1.headers));
                }

                chksums = move2heap(chksumsᴷ, chksums);
                foreach (var (i, sum) in chksums) {
                    if (i >= len(vʗ1.chksums)) {
                        tΔ1.Fatalf("entry %d: unexpected sum: got %s", i, sum);
                    }
                    if (sum != vʗ1.chksums[i]) {
                        tΔ1.Fatalf("entry %d: incorrect checksum: got %s, want %s", i, sum, vʗ1.chksums[i]);
                    }
                }

                if (!AreEqual(err, vʗ1.err)) {
                    tΔ1.Fatalf("unexpected error: got %v, want %v", err, vʗ1.err);
                }
                f.Close();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

internal partial struct TestPartialRead_testCase /*dyn*/ {
    internal nint cnt;       // Number of bytes to read
    internal @string output; // Expected value of string read
}

internal partial struct TestPartialRead_vectors /*dyn*/ {
    internal @string @file;
    internal slice<TestPartialRead_testCase> cases;
}

public static void TestPartialRead(ж<testing.T> t) {
    var vectors = slice<TestPartialRead_vectors>([new(
        @file: "testdata/gnu.tar",
        cases: slice<TestPartialRead_testCase>([
            new(4, "Kilt"),
            new(6, "Google")
        ])
    ), new(
        @file: "testdata/sparse-formats.tar",
        cases: slice<TestPartialRead_testCase>([
            new(2, "\x00G"),
            new(4, "\x00G\x00o"),
            new(6, "\x00G\x00o\x00G"),
            new(8, "\x00G\x00o\x00G\x00o"),
            new(4, "end\n")
        ])
    )
    ]);

    foreach (var (_, vᴛ1) in vectors) {
        ref var v = ref heap(new TestPartialRead_vectors(), out var Ꮡv);
        v = vᴛ1;

        var vʗ1 = v;
        t.Run(path.Base(v.@file), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var (f, err) = os.Open(vʗ1.@file);
                if (err != nil) {
                    tΔ1.Fatalf("Open() error: %v", err);
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);

                var tr = NewReader(new tar_test_package.os_FileжReader(f));
                foreach (var (i, tc) in vʗ1.cases) {
                    var (hdr, errΔ1) = tr.Next();
                    if (errΔ1 != nil || hdr == nil) {
                        tΔ1.Fatalf("entry %d, Next(): got %v, want %v", i, errΔ1, (any)(default));
                    }
                    var buf = new slice<byte>(tc.cnt);
                    {
                        var (_, errΔ2) = io.ReadFull(new global::go.archive.tar_package.ReaderжReader(tr), buf); if (errΔ2 != nil) {
                            tΔ1.Fatalf("entry %d, ReadFull(): got %v, want %v", i, errΔ2, (any)(default));
                        }
                    }
                    if (((sstring)buf) != tc.output) {
                        tΔ1.Fatalf("entry %d, ReadFull(): got %q, want %q", i, (@string)buf, tc.output);
                    }
                }

                {
                    var (_, errΔ3) = tr.Next(); if (!AreEqual(errΔ3, io.EOF)) {
                        tΔ1.Fatalf("Next(): got %v, want EOF", errΔ3);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataGnuTarˢ => ref testdataGnuTarˢᵇ;
private static readonly @string testdataGnuTarˢᵇ = "testdata/gnu.tar"u8;

public static void TestUninitializedRead(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(testdataGnuTarˢ);
        if (err != nil) {
            t.Fatalf("Unexpected error: %v", err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        var tr = NewReader(new tar_test_package.os_FileжReader(f));
        (_, err) = tr.Read(slice<byte>([]));
        if (err == nil || !AreEqual(err, io.EOF)) {
            t.Errorf("Unexpected error: %v, wanted %v", err, io.EOF);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct reader {
    /*embed*/ public io_package.Reader Reader;
}

internal partial struct readSeeker {
    /*embed*/ public io_package.ReadSeeker ReadSeeker;
}

internal partial struct readBadSeeker {
    /*embed*/ public io_package.ReadSeeker ReadSeeker;
}

// Go method set entry for the promoted 'ReadSeeker.Read()' - provided ONLY by the embedded
// interface field in *readBadSeeker's method set; see the pointer-only satisfaction record.
internal static (nint, error) Read(this readBadSeeker recvᴛ, slice<byte> p) => recvᴛ.ReadSeeker.Read(p);

internal static (int64, error) Seek(this ref readBadSeeker rbs, int64 _Δp1, nint _Δp2) {
    return (0, fmt.Errorf("illegal seek"u8));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string garbageˢ => ref garbageˢᵇ;
private static readonly @string garbageˢᵇ = "garbage "u8;
internal static ref readonly @string ioReaderˢ => ref ioReaderˢᵇ;
private static readonly @string ioReaderˢᵇ = "io.Reader"u8;
internal static ref readonly @string autoˢ => ref autoˢᵇ;
private static readonly @string autoˢᵇ = "auto"u8;
internal static ref readonly @string manualˢ => ref manualˢᵇ;
private static readonly @string manualˢᵇ = "manual"u8;
internal static ref readonly @string ioReadSeekerˢ => ref ioReadSeekerˢᵇ;
private static readonly @string ioReadSeekerˢᵇ = "io.ReadSeeker"u8;
internal static ref readonly @string readBadSeekerˢ => ref readBadSeekerˢᵇ;
private static readonly @string readBadSeekerˢᵇ = "ReadBadSeeker"u8;

internal partial struct TestReadTruncation_vectors /*dyn*/ {
    internal @string input; // Input stream
    internal nint cnt;      // Expected number of headers read
    internal error err;     // Expected error outcome
}

// TestReadTruncation tests the ending condition on various truncated files and
// that truncated files are still detected even if the underlying io.Reader
// satisfies io.Seeker.
public static void TestReadTruncation(ж<testing.T> t) {
    var ssᴷ = new stackAppend<@string>();

    slice<@string> ss = default;
    foreach (var (_, p) in slice<@string>([
        "testdata/gnu.tar",
        "testdata/ustar-file-reg.tar",
        "testdata/pax-path-hdr.tar",
        "testdata/sparse-formats.tar"
    ])) {
        var (buf, err) = os.ReadFile(p);
        if (err != nil) {
            t.Fatalf("unexpected error: %v", err);
        }
        ss = append(ssᴷ, ss, (@string)buf);
    }

    @string data1 = ss[0];
    @string data2 = ss[1];
    @string pax = ss[2];
    @string sparse = ss[3];
    data2 += strings.Repeat("\x00", 10 * 512);
    @string trash = strings.Repeat(garbageˢ, 64); // Exactly 512 bytes

    var vectors = slice<TestReadTruncation_vectors>([
        new("", 0, io.EOF), // Empty file is a "valid" tar file
        new(data1[..511], 0, io.ErrUnexpectedEOF),
        new(data1[..512], 1, io.ErrUnexpectedEOF),
        new(data1[..1024], 1, io.EOF),
        new(data1[..1536], 2, io.ErrUnexpectedEOF),
        new(data1[..2048], 2, io.EOF),
        new(data1, 2, io.EOF),
        new(data1[..2048] + data2[..1536], 3, io.EOF),
        new(data2[..511], 0, io.ErrUnexpectedEOF),
        new(data2[..512], 1, io.ErrUnexpectedEOF),
        new(data2[..1195], 1, io.ErrUnexpectedEOF),
        new(data2[..1196], 1, io.EOF), // Exact end of data and start of padding
        new(data2[..1200], 1, io.EOF),
        new(data2[..1535], 1, io.EOF),
        new(data2[..1536], 1, io.EOF), // Exact end of padding
        new(data2[..1536] + trash[..1], 1, io.ErrUnexpectedEOF),
        new(data2[..1536] + trash[..511], 1, io.ErrUnexpectedEOF),
        new(data2[..1536] + trash, 1, ErrHeader),
        new(data2[..2048], 1, io.EOF), // Exactly 1 empty block
        new(data2[..2048] + trash[..1], 1, io.ErrUnexpectedEOF),
        new(data2[..2048] + trash[..511], 1, io.ErrUnexpectedEOF),
        new(data2[..2048] + trash, 1, ErrHeader),
        new(data2[..2560], 1, io.EOF), // Exactly 2 empty blocks (normal end-of-stream)
        new(data2[..2560] + trash[..1], 1, io.EOF),
        new(data2[..2560] + trash[..511], 1, io.EOF),
        new(data2[..2560] + trash, 1, io.EOF),
        new(data2[..3072], 1, io.EOF),
        new(pax, 0, io.EOF), // PAX header without data is a "valid" tar file
        new(pax + trash[..1], 0, io.ErrUnexpectedEOF),
        new(pax + trash[..511], 0, io.ErrUnexpectedEOF),
        new(sparse[..511], 0, io.ErrUnexpectedEOF),
        new(sparse[..512], 0, io.ErrUnexpectedEOF),
        new(sparse[..3584], 1, io.EOF),
        new(sparse[..9200], 1, io.EOF), // Terminate in padding of sparse header
        new(sparse[..9216], 1, io.EOF),
        new(sparse[..9728], 2, io.ErrUnexpectedEOF),
        new(sparse[..10240], 2, io.EOF),
        new(sparse[..11264], 2, io.ErrUnexpectedEOF),
        new(sparse, 5, io.EOF),
        new(sparse + trash, 5, io.EOF)
    ]);

    foreach (var (i, v) in vectors) {
        for (nint j = 0; j < 6; j++) {
            ж<global::go.archive.tar_package.Reader> tr = default;
            @string s1 = default;
            @string s2 = default;

            switch (j) {
            case 0: {
                tr = NewReader(new tar_internal_test_package.readerжReader(Ꮡ(new reader(new tar_test_package.strings_ReaderжReader(strings.NewReader(v.input))))));
                (s1, s2) = (ioReaderˢ, autoˢ);
                break;
            }
            case 1: {
                tr = NewReader(new tar_internal_test_package.readerжReader(Ꮡ(new reader(new tar_test_package.strings_ReaderжReader(strings.NewReader(v.input))))));
                (s1, s2) = (ioReaderˢ, manualˢ);
                break;
            }
            case 2: {
                tr = NewReader(new tar_internal_test_package.readSeekerжReader(Ꮡ(new readSeeker(new tar_test_package.strings_ReaderжReadSeeker(strings.NewReader(v.input))))));
                (s1, s2) = (ioReadSeekerˢ, autoˢ);
                break;
            }
            case 3: {
                tr = NewReader(new tar_internal_test_package.readSeekerжReader(Ꮡ(new readSeeker(new tar_test_package.strings_ReaderжReadSeeker(strings.NewReader(v.input))))));
                (s1, s2) = (ioReadSeekerˢ, manualˢ);
                break;
            }
            case 4: {
                tr = NewReader(new tar_internal_test_package.readBadSeekerжReader(Ꮡ(new readBadSeeker(new tar_test_package.strings_ReaderжReadSeeker(strings.NewReader(v.input))))));
                (s1, s2) = (readBadSeekerˢ, autoˢ);
                break;
            }
            case 5: {
                tr = NewReader(new tar_internal_test_package.readBadSeekerжReader(Ꮡ(new readBadSeeker(new tar_test_package.strings_ReaderжReadSeeker(strings.NewReader(v.input))))));
                (s1, s2) = (readBadSeekerˢ, manualˢ);
                break;
            }}

            nint cnt = default;
            error err = default;
            for (;;) {
                {
                    (_, err) = tr.Next(); if (err != nil) {
                        break;
                    }
                }
                cnt++;
                if (s2 == "manual"u8) {
                    {
                        (_, err) = tr.writeTo(io.Discard); if (err != nil) {
                            break;
                        }
                    }
                }
            }
            if (!AreEqual(err, v.err)) {
                t.Errorf("test %d, NewReader(%s) with %s discard: got %v, want %v",
                    i, s1, s2, err, v.err);
            }
            if (cnt != v.cnt) {
                t.Errorf("test %d, NewReader(%s) with %s discard: got %d headers, want %d headers",
                    i, s1, s2, cnt, v.cnt);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataHdrOnlyTarˢ => ref testdataHdrOnlyTarˢᵇ;
private static readonly @string testdataHdrOnlyTarˢᵇ = "testdata/hdr-only.tar"u8;

// TestReadHeaderOnly tests that Reader does not attempt to read special
// header-only files.
public static void TestReadHeaderOnly(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var hdrsᴷ = new stackAppend<ж<global::go.archive.tar_package.Header>>();

        var (f, err) = os.Open(testdataHdrOnlyTarˢ);
        if (err != nil) {
            t.Fatalf("unexpected error: %v", err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        slice<ж<global::go.archive.tar_package.Header>> hdrs = default;
        var tr = NewReader(new tar_test_package.os_FileжReader(f));
        for (;;) {
            var (hdr, errΔ1) = tr.Next();
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            if (errΔ1 != nil) {
                t.Errorf("Next(): got %v, want %v", errΔ1, (any)(default));
                continue;
            }
            hdrs = append(hdrsᴷ, hdrs, hdr);

            // If a special flag, we should read nothing.
            var (cnt, _) = io.ReadFull(new global::go.archive.tar_package.ReaderжReader(tr), slice<byte>([0]));
            if (cnt > 0 && hdr.Typeflag != TypeReg) {
                t.Errorf("ReadFull(...): got %d bytes, want 0 bytes", cnt);
            }
        }

        // File is crafted with 16 entries. The later 8 are identical to the first
        // 8 except that the size is set.
        if (len(hdrs) != 16) {
            t.Fatalf("len(hdrs): got %d, want %d", len(hdrs), (nint)16);
        }
        for (nint i = 0; i < 8; i++) {
            var (hdr1, hdr2) = (hdrs[i + 0], hdrs[i + 8]);
            (hdr1.Size, hdr2.Size) = (0, 0);
            if (!reflect.DeepEqual(hdr1.Value, hdr2.Value)) {
                t.Errorf("incorrect header:\ngot  %+v\nwant %+v", hdr1.Value, hdr2.Value);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestMergePAX_vectors /*dyn*/ {
    internal map<@string, @string> @in;
    internal ж<global::go.archive.tar_package.Header> want;
    internal bool ok;
}

public static void TestMergePAX(ж<testing.T> t) {
    var vectors = slice<TestMergePAX_vectors>([new(
        @in: new map<@string, @string>{
            ["path"] = "a/b/c",
            ["uid"] = "1000",
            ["mtime"] = "1350244992.023960108"
        },
        want: Ꮡ(new Header(
            Name: "a/b/c",
            Uid: 1000,
            ModTime: time.Unix(1350244992, 23960108),
            PAXRecords: new map<@string, @string>{
                ["path"] = "a/b/c",
                ["uid"] = "1000",
                ["mtime"] = "1350244992.023960108"
            }
        )),
        ok: true
    ), new(
        @in: new map<@string, @string>{
            ["gid"] = "gtgergergersagersgers"
        },
        ok: false
    ), new(
        @in: new map<@string, @string>{
            ["missing"] = "missing",
            ["SCHILY.xattr.key"] = "value"
        },
        want: Ꮡ(new Header(
            Xattrs: new map<@string, @string>{["key"] = "value"},
            PAXRecords: new map<@string, @string>{
                ["missing"] = "missing",
                ["SCHILY.xattr.key"] = "value"
            }
        )),
        ok: true
    )
    ]);

    foreach (var (i, v) in vectors) {
        var got = @new<global::go.archive.tar_package.Header>();
        var err = mergePAX(ref (got).DerefOrNull(), v.@in);
        if (v.ok && !reflect.DeepEqual(got.Value, v.want.Value)) {
            t.Errorf("test %d, mergePAX(...):\ngot  %+v\nwant %+v", i, got.Value, v.want.Value);
        }
        {
            var ok = err == nil; if (ok != v.ok) {
                t.Errorf("test %d, mergePAX(...): got %v, want %v", i, ok, v.ok);
            }
        }
    }
}

internal partial struct TestParsePAX_vectors /*dyn*/ {
    internal @string @in;
    internal map<@string, @string> want;
    internal bool ok;
}

public static void TestParsePAX(ж<testing.T> t) {
    var vectors = slice<TestParsePAX_vectors>([
        new("", default, true),
        new("6 k=1\n", new map<@string, @string>{["k"] = "1"}, true),
        new("10 a=name\n", new map<@string, @string>{["a"] = "name"}, true),
        new("9 a=name\n", new map<@string, @string>{["a"] = "name"}, true),
        new("30 mtime=1350244992.023960108\n", new map<@string, @string>{["mtime"] = "1350244992.023960108"}, true),
        new("3 somelongkey=\n", default, false),
        new("50 tooshort=\n", default, false),
        new("13 key1=haha\n13 key2=nana\n13 key3=kaka\n",
            new map<@string, @string>{["key1"] = "haha", ["key2"] = "nana", ["key3"] = "kaka"}, true),
        new("13 key1=val1\n13 key2=val2\n8 key1=\n",
            new map<@string, @string>{["key1"] = "", ["key2"] = "val2"}, true),
        new("22 GNU.sparse.size=10\n26 GNU.sparse.numblocks=2\n"u8 + "23 GNU.sparse.offset=1\n25 GNU.sparse.numbytes=2\n"u8 + "23 GNU.sparse.offset=3\n25 GNU.sparse.numbytes=4\n"u8,
            new map<@string, @string>{[paxGNUSparseSize] = "10", [paxGNUSparseNumBlocks] = "2", [paxGNUSparseMap] = "1,2,3,4"}, true),
        new("22 GNU.sparse.size=10\n26 GNU.sparse.numblocks=1\n"u8 + "25 GNU.sparse.numbytes=2\n23 GNU.sparse.offset=1\n"u8,
            default, false),
        new("22 GNU.sparse.size=10\n26 GNU.sparse.numblocks=1\n"u8 + "25 GNU.sparse.offset=1,2\n25 GNU.sparse.numbytes=2\n"u8,
            default, false)
    ]);

    foreach (var (i, v) in vectors) {
        var r = strings.NewReader(v.@in);
        var (got, err) = parsePAX(new tar_test_package.strings_ReaderжReader(r));
        if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(got, v.want) && !(len(got) == 0 && len(v.want) == 0)) {
            t.Errorf("test %d, parsePAX():\ngot  %v\nwant %v", i, got, v.want);
        }
        {
            var ok = err == nil; if (ok != v.ok) {
                t.Errorf("test %d, parsePAX(): got %v, want %v", i, ok, v.ok);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fewaˢ => ref fewaˢᵇ;
private static readonly @string fewaˢᵇ = "fewa"u8;

internal partial struct TestReadOldGNUSparseMap_vectors /*dyn*/ {
    internal slice<byte> input;
    internal global::go.archive.tar_package.sparseDatas wantMap;
    internal int64 wantSize;
    internal error wantErr;
}

public static void TestReadOldGNUSparseMap(ж<testing.T> t) {
    slice<@string> populateSparseMap(global::go.archive.tar_package.sparseArray sa, slice<@string> sps) {
        for (nint i = 0; len(sps) > 0 && i < sa.maxEntries(); i++) {
            copy(sa.entry(i), sps[0]);
            sps = sps[1..];
        }
        if (len(sps) > 0) {
            copy(sa.isExtended(), ((@string)(new byte[]{0x80})));
        }
        return sps;
    }

    var populateSparseMapʗ1 = populateSparseMap;
    slice<byte> /*out*/ makeInput(global::go.archive.tar_package.Format format, @string size, params ꓸꓸꓸstring spsʗp) {
        slice<byte> @out = default;
        var sps = spsʗp.slice();
        // Write the initial GNU header.
        ref var blk = ref heap(new global::go.archive.tar_package.block(), out var Ꮡblk);
        var gnu = Ꮡblk.toGNU();
        var sparse = gnu.sparse();
        copy(gnu.realSize(), size);
        sps = populateSparseMapʗ1(sparse, sps);
        if (format != FormatUnknown) {
            Ꮡblk.setFormat(format);
        }
        @out = appendꓸꓸꓸ(@out, blk[..]);

        // Write extended sparse blocks.
        while (len(sps) > 0) {
            global::go.archive.tar_package.block blkΔ1 = default;
            sps = populateSparseMapʗ1(blkΔ1.toSparse(), sps);
            @out = appendꓸꓸꓸ(@out, blkΔ1[..]);
        }
        return @out;
    }

    slice<@string> /*out*/ makeSparseStrings(slice<global::go.archive.tar_package.sparseEntry> sp) {
        slice<@string> @out = default;
        global::go.archive.tar_package.formatter f = default;
        foreach (var (_, s) in sp) {
            array<byte> b = new(24);
            f.formatNumeric(b[..12], s.Offset);
            f.formatNumeric(b[12..], s.Length);
            @out = append(@out, (@string)(b[..]));
        }
        return @out;
    }

    var vectors = slice<TestReadOldGNUSparseMap_vectors>([new(
        input: makeInput(FormatUnknown, ""u8),
        wantErr: ErrHeader
    ), new(
        input: makeInput(FormatGNU, "1234", fewaˢ),
        wantSize: 668,
        wantErr: ErrHeader
    ), new(
        input: makeInput(FormatGNU, "0031"),
        wantSize: 25
    ), new(
        input: makeInput(FormatGNU, "80"),
        wantErr: ErrHeader
    ), new(
        input: makeInput(FormatGNU, "1234"u8,
            makeSparseStrings(new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 0), new(1, 1)}.slice())).ꓸꓸꓸ),
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 0), new(1, 1)}.slice()),
        wantSize: 668
    ), new(
        input: makeInput(FormatGNU, "1234"u8,
            appendꓸꓸꓸ(makeSparseStrings(new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 0), new(1, 1)}.slice())), slice<@string>(["", "blah"])).ꓸꓸꓸ),
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 0), new(1, 1)}.slice()),
        wantSize: 668
    ), new(
        input: makeInput(FormatGNU, "3333"u8,
            makeSparseStrings(new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 1), new(2, 1), new(4, 1), new(6, 1)}.slice())).ꓸꓸꓸ),
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 1), new(2, 1), new(4, 1), new(6, 1)}.slice()),
        wantSize: 1755
    ), new(
        input: makeInput(FormatGNU, ""u8,
            appendꓸꓸꓸ(appendꓸꓸꓸ(
                makeSparseStrings(new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 1), new(2, 1)}.slice())),
                slice<@string>(["", ""])),
                makeSparseStrings(new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(4, 1), new(6, 1)}.slice()))).ꓸꓸꓸ),
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 1), new(2, 1), new(4, 1), new(6, 1)}.slice())
    ), new(
        input: makeInput(FormatGNU, ""u8,
            makeSparseStrings(new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 1), new(2, 1), new(4, 1), new(6, 1), new(8, 1), new(10, 1)}.slice())).ꓸꓸꓸ)[..(int)(blockSize)],
        wantErr: io.ErrUnexpectedEOF
    ), new(
        input: makeInput(FormatGNU, ""u8,
            makeSparseStrings(new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 1), new(2, 1), new(4, 1), new(6, 1), new(8, 1), new(10, 1)}.slice())).ꓸꓸꓸ)[..(int)(3 * blockSize / 2)],
        wantErr: io.ErrUnexpectedEOF
    ), new(
        input: makeInput(FormatGNU, ""u8,
            makeSparseStrings(new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 1), new(2, 1), new(4, 1), new(6, 1), new(8, 1), new(10, 1)}.slice())).ꓸꓸꓸ),
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 1), new(2, 1), new(4, 1), new(6, 1), new(8, 1), new(10, 1)}.slice())
    ), new(
        input: makeInput(FormatGNU, ""u8,
            makeSparseStrings(new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(10737418240L, 512), new(21474836480L, 512)}.slice())).ꓸꓸꓸ),
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(10737418240L, 512), new(21474836480L, 512)}.slice())
    ), new(
        input: makeInput(FormatGNU, ""u8,
            makeSparseStrings(func(global::go.archive.tar_package.sparseDatas () => {
                global::go.archive.tar_package.sparseDatas datas = default;
                // This is more than enough entries to exceed our limit.
                foreach (var i in range<int64>((int64)((int64)1 << 20))) {
                    datas = append(datas, new sparseEntry(i * 2, (i * 2) + 1));
                }
                return datas;
            })()).ꓸꓸꓸ),
        wantErr: errSparseTooLong
    )
    ]);

    foreach (var (i, vᴛ1) in vectors) {
        var v = vᴛ1;

        ref var blk = ref heap(new global::go.archive.tar_package.block(), out var Ꮡblk);
        ref var hdr = ref heap(new global::go.archive.tar_package.Header(), out var Ꮡhdr);
        v.input = v.input.slice(copy(blk[..], v.input));
        var tr = new Reader(r: new tar_test_package.bytes_ReaderжReader(bytes.NewReader(v.input)));
        var (got, err) = tr.readOldGNUSparseMap(Ꮡhdr, Ꮡblk);
        if (!slices.Equal<global::go.archive.tar_package.sparseDatas, global::go.archive.tar_package.sparseEntry>(got, v.wantMap)) {
            t.Errorf("test %d, readOldGNUSparseMap(): got %v, want %v", i, got, v.wantMap);
        }
        if (!AreEqual(err, v.wantErr)) {
            t.Errorf("test %d, readOldGNUSparseMap() = %v, want %v", i, err, v.wantErr);
        }
        if (hdr.Size != v.wantSize) {
            t.Errorf("test %d, Header.Size = %d, want %d", i, hdr.Size, v.wantSize);
        }
    }
}

internal partial struct TestReadGNUSparsePAXHeaders_vectors /*dyn*/ {
    internal @string inputData;
    internal map<@string, @string> inputHdrs;
    internal global::go.archive.tar_package.sparseDatas wantMap;
    internal int64 wantSize;
    internal @string wantName;
    internal error wantErr;
}

public static void TestReadGNUSparsePAXHeaders(ж<testing.T> t) {
    @string padInput(@string s) => s + ((sstring)(zeroBlock.slice(0, (nint)(blockPadding((int64)len(s))))));

    var vectors = slice<TestReadGNUSparsePAXHeaders_vectors>([new(
        inputHdrs: default,
        wantErr: default
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseNumBlocks] = strconv.FormatInt(math.MaxInt64, 10),
            [paxGNUSparseMap] = "0,1,2,3"
        },
        wantErr: ErrHeader
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseNumBlocks] = "4\x00",
            [paxGNUSparseMap] = "0,1,2,3"
        },
        wantErr: ErrHeader
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseNumBlocks] = "4",
            [paxGNUSparseMap] = "0,1,2,3"
        },
        wantErr: ErrHeader
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseNumBlocks] = "2",
            [paxGNUSparseMap] = "0,1,2,3"
        },
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 1), new(2, 3)}.slice())
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseNumBlocks] = "2",
            [paxGNUSparseMap] = "0, 1,2,3"
        },
        wantErr: ErrHeader
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseNumBlocks] = "2",
            [paxGNUSparseMap] = "0,1,02,3",
            [paxGNUSparseRealSize] = "4321"
        },
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 1), new(2, 3)}.slice()),
        wantSize: 4321
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseNumBlocks] = "2",
            [paxGNUSparseMap] = "0,one1,2,3"
        },
        wantErr: ErrHeader
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseMajor] = "0",
            [paxGNUSparseMinor] = "0",
            [paxGNUSparseNumBlocks] = "2",
            [paxGNUSparseMap] = "0,1,2,3",
            [paxGNUSparseSize] = "1234",
            [paxGNUSparseRealSize] = "4321",
            [paxGNUSparseName] = "realname"
        },
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 1), new(2, 3)}.slice()),
        wantSize: 1234,
        wantName: "realname"
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseMajor] = "0",
            [paxGNUSparseMinor] = "0",
            [paxGNUSparseNumBlocks] = "1",
            [paxGNUSparseMap] = "10737418240,512",
            [paxGNUSparseSize] = "10737418240",
            [paxGNUSparseName] = "realname"
        },
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(10737418240L, 512)}.slice()),
        wantSize: 10737418240L,
        wantName: "realname"
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseMajor] = "0",
            [paxGNUSparseMinor] = "0",
            [paxGNUSparseNumBlocks] = "0",
            [paxGNUSparseMap] = ""
        },
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{}.slice())
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseMajor] = "0",
            [paxGNUSparseMinor] = "1",
            [paxGNUSparseNumBlocks] = "4",
            [paxGNUSparseMap] = "0,5,10,5,20,5,30,5"
        },
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 5), new(10, 5), new(20, 5), new(30, 5)}.slice())
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseMajor] = "1",
            [paxGNUSparseMinor] = "0",
            [paxGNUSparseNumBlocks] = "4",
            [paxGNUSparseMap] = "0,5,10,5,20,5,30,5"
        },
        wantErr: io.ErrUnexpectedEOF
    ), new(
        inputData: padInput("0\n"),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1", [paxGNUSparseMinor] = "0"},
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{}.slice())
    ), new(
        inputData: padInput("0\n")[..(int)(blockSize - 1)] + "#",
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1", [paxGNUSparseMinor] = "0"},
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{}.slice())
    ), new(
        inputData: padInput("0"),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1", [paxGNUSparseMinor] = "0"},
        wantErr: io.ErrUnexpectedEOF
    ), new(
        inputData: padInput("ab\n"),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1", [paxGNUSparseMinor] = "0"},
        wantErr: ErrHeader
    ), new(
        inputData: padInput("1\n2\n3\n"),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1", [paxGNUSparseMinor] = "0"},
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice())
    ), new(
        inputData: padInput("1\n2\n"),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1", [paxGNUSparseMinor] = "0"},
        wantErr: io.ErrUnexpectedEOF
    ), new(
        inputData: padInput("1\n2\n\n"),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1", [paxGNUSparseMinor] = "0"},
        wantErr: ErrHeader
    ), new(
        inputData: ((@string)(zeroBlock[..])) + padInput("0\n"),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1", [paxGNUSparseMinor] = "0"},
        wantErr: ErrHeader
    ), new(
        inputData: strings.Repeat("0", blockSize) + padInput("1\n5\n1\n"),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1", [paxGNUSparseMinor] = "0"},
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(5, 1)}.slice())
    ), new(
        inputData: padInput(fmt.Sprintf("%d\n"u8, (int64)math.MaxInt64)),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1", [paxGNUSparseMinor] = "0"},
        wantErr: ErrHeader
    ), new(
        inputData: padInput(strings.Repeat("0", 300) + "1\n"u8 + strings.Repeat("0", 1000) + "5\n"u8 + strings.Repeat("0", 800) + "2\n"u8),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1", [paxGNUSparseMinor] = "0"},
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(5, 2)}.slice())
    ), new(
        inputData: padInput("2\n10737418240\n512\n21474836480\n512\n"),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1", [paxGNUSparseMinor] = "0"},
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(10737418240L, 512), new(21474836480L, 512)}.slice())
    ), new(
        inputData: padInput("100\n"u8 + func(@string () => {
            slice<@string> ss = default;
            for (nint i = 0; i < 100; i++) {
                ss = append(ss, fmt.Sprintf("%d\n%d\n"u8, (int64)i << 30, (nint)512));
            }
            return strings.Join(ss, ""u8);
        })()),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1", [paxGNUSparseMinor] = "0"},
        wantMap: func(global::go.archive.tar_package.sparseDatas () => {
            global::go.archive.tar_package.sparseDatas spd = default;
            for (nint i = 0; i < 100; i++) {
                spd = append(spd, new sparseEntry((int64)i << 30, 512));
            }
            return spd;
        })()
    )
    ]);

    foreach (var (i, v) in vectors) {
        ref var hdr = ref heap(new global::go.archive.tar_package.Header(), out var Ꮡhdr);
        hdr.PAXRecords = v.inputHdrs;
        var r = strings.NewReader(v.inputData + "#"u8); // Add canary byte
        var tr = new Reader(curr: new global::go.archive.tar_package.regFileReaderжfileReader(Ꮡ(new regFileReader(new tar_test_package.strings_ReaderжReader(r), (int64)r.Len()))));
        var (got, err) = tr.readGNUSparsePAXHeaders(Ꮡhdr);
        if (!slices.Equal<global::go.archive.tar_package.sparseDatas, global::go.archive.tar_package.sparseEntry>(got, v.wantMap)) {
            t.Errorf("test %d, readGNUSparsePAXHeaders(): got %v, want %v", i, got, v.wantMap);
        }
        if (!AreEqual(err, v.wantErr)) {
            t.Errorf("test %d, readGNUSparsePAXHeaders() = %v, want %v", i, err, v.wantErr);
        }
        if (hdr.Size != v.wantSize) {
            t.Errorf("test %d, Header.Size = %d, want %d", i, hdr.Size, v.wantSize);
        }
        if (hdr.Name != v.wantName) {
            t.Errorf("test %d, Header.Name = %s, want %s", i, hdr.Name, v.wantName);
        }
        if (v.wantErr == nil && r.Len() == 0) {
            t.Errorf("test %d, canary byte unexpectedly consumed", i);
        }
    }
}

// testNonEmptyReader wraps an io.Reader and ensures that
// Read is never called with an empty buffer.
internal partial struct testNonEmptyReader {
    /*embed*/ public io_package.Reader Reader;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unexpectedEmptyReadCallˢ => ref unexpectedEmptyReadCallˢᵇ;
private static readonly @string unexpectedEmptyReadCallˢᵇ = "unexpected empty Read call"u8;

internal static (nint, error) Read(this testNonEmptyReader r, slice<byte> b) {
    if (len(b) == 0) {
        return (0, errors.New(unexpectedEmptyReadCallˢ));
    }
    return r.Reader.Read(b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cdeˢ = (@string)"cde"u8;
internal static readonly object abcˢ = (@string)"abc"u8;
internal static readonly object defghˢ = (@string)"defgh"u8;

internal partial struct TestFileReader_testRead /*dyn*/ {
// Read(cnt) == (wantStr, wantErr)
    internal nint cnt;
    internal @string wantStr;
    internal error wantErr;
}

internal partial struct TestFileReader_testWriteTo /*dyn*/ {
// WriteTo(testFile{ops}) == (wantCnt, wantErr)
    internal fileOps ops;
    internal int64 wantCnt;
    internal error wantErr;
}

internal partial struct TestFileReader_testRemaining /*dyn*/ {
// logicalRemaining() == wantLCnt, physicalRemaining() == wantPCnt
    internal int64 wantLCnt;
    internal int64 wantPCnt;
}

internal partial struct TestFileReader_makeReg /*dyn*/ {
    internal @string str;
    internal int64 size;
}

internal partial struct TestFileReader_makeSparse /*dyn*/ {
    internal TestFileReader_makeReg makeReg;
    internal global::go.archive.tar_package.sparseDatas spd;
    internal int64 size;
}

internal partial struct TestFileReader_vectors /*dyn*/ {
    internal TestFileReader_fileMaker maker;
    internal slice<TestFileReader_testFnc> tests;
}

public static void TestFileReader(ж<testing.T> t) {
    var vectors = slice<TestFileReader_vectors>([new(
        maker: new TestFileReader_makeReg("", 0),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRemaining(0, 0),
            new TestFileReader_testRead(0, "", io.EOF),
            new TestFileReader_testRead(1, "", io.EOF),
            new TestFileReader_testWriteTo(default, 0, default),
            new TestFileReader_testRemaining(0, 0)
        ])
    ), new(
        maker: new TestFileReader_makeReg("", 1),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRemaining(1, 1),
            new TestFileReader_testRead(5, "", io.ErrUnexpectedEOF),
            new TestFileReader_testWriteTo(default, 0, io.ErrUnexpectedEOF),
            new TestFileReader_testRemaining(1, 1)
        ])
    ), new(
        maker: new TestFileReader_makeReg("hello", 5),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRemaining(5, 5),
            new TestFileReader_testRead(5, "hello", io.EOF),
            new TestFileReader_testRemaining(0, 0)
        ])
    ), new(
        maker: new TestFileReader_makeReg("hello, world", 50),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRemaining(50, 50),
            new TestFileReader_testRead(7, "hello, ", default),
            new TestFileReader_testRemaining(43, 43),
            new TestFileReader_testRead(5, "world", default),
            new TestFileReader_testRemaining(38, 38),
            new TestFileReader_testWriteTo(default, 0, io.ErrUnexpectedEOF),
            new TestFileReader_testRead(1, "", io.ErrUnexpectedEOF),
            new TestFileReader_testRemaining(38, 38)
        ])
    ), new(
        maker: new TestFileReader_makeReg("hello, world", 5),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRemaining(5, 5),
            new TestFileReader_testRead(0, "", default),
            new TestFileReader_testRead(4, "hell", default),
            new TestFileReader_testRemaining(1, 1),
            new TestFileReader_testWriteTo(new fileOps(new any[]{(@string)"o"}.slice()), 1, default),
            new TestFileReader_testRemaining(0, 0),
            new TestFileReader_testWriteTo(default, 0, default),
            new TestFileReader_testRead(0, "", io.EOF)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde", 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 3)}.slice()), 8),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRemaining(8, 5),
            new TestFileReader_testRead(3, "ab\x00", default),
            new TestFileReader_testRead(10, ((@string)(new byte[]{0x00, 0x00, 0x63, 0x64, 0x65})), io.EOF),
            new TestFileReader_testRemaining(0, 0)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde", 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 3)}.slice()), 8),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRemaining(8, 5),
            new TestFileReader_testWriteTo(new fileOps(new any[]{(@string)"ab", (int64)3, cdeˢ}.slice()), 8, default),
            new TestFileReader_testRemaining(0, 0)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde", 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 3)}.slice()), 10),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRemaining(10, 5),
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x61, 0x62, 0x00, 0x00, 0x00, 0x63, 0x64, 0x65, 0x00, 0x00})), io.EOF),
            new TestFileReader_testRemaining(0, 0)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abc", 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 3)}.slice()), 10),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRemaining(10, 5),
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x61, 0x62, 0x00, 0x00, 0x00, 0x63})), io.ErrUnexpectedEOF),
            new TestFileReader_testRemaining(4, 2)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde", 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 2)}.slice()), 8),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRemaining(8, 5),
            new TestFileReader_testRead(8, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x64, 0x65})), io.EOF),
            new TestFileReader_testRemaining(0, 0)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde", 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 0), new(6, 0), new(6, 2)}.slice()), 8),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRemaining(8, 5),
            new TestFileReader_testRead(8, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x64, 0x65})), io.EOF),
            new TestFileReader_testRemaining(0, 0)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde", 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 0), new(6, 0), new(6, 2)}.slice()), 8),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRemaining(8, 5),
            new TestFileReader_testWriteTo(new fileOps(new any[]{(int64)1, abcˢ, (int64)2, (@string)"de"}.slice()), 8, default),
            new TestFileReader_testRemaining(0, 0)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde", 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 2)}.slice()), 10),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x64, 0x65, 0x00, 0x00})), io.EOF)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde", 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 2)}.slice()), 10),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testWriteTo(new fileOps(new any[]{(int64)1, abcˢ, (int64)2, (@string)"de", (int64)1, (@string)"\x00"}.slice()), 10, default)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde", 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 2), new(8, 0), new(8, 0), new(8, 0), new(8, 0)}.slice()), 10),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x64, 0x65, 0x00, 0x00})), io.EOF)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("", 0), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{}.slice()), 2),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRead(100, "\x00\x00", io.EOF)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("", 8), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRead(100, "\x00", io.ErrUnexpectedEOF)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("ab", 2), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62})), errMissData)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("ab", 8), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62})), io.ErrUnexpectedEOF)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abc", 3), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00})), errMissData)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abc", 8), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00})), io.ErrUnexpectedEOF)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde", 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x64, 0x65})), errMissData)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde", 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testWriteTo(new fileOps(new any[]{(int64)1, abcˢ, (int64)2, (@string)"de"}.slice()), 8, errMissData)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde", 8), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x64, 0x65})), io.ErrUnexpectedEOF)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcdefghEXTRA", 13), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRemaining(15, 13),
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x64, 0x65, 0x66, 0x67, 0x68, 0x00, 0x00, 0x00, 0x00})), errUnrefData),
            new TestFileReader_testWriteTo(default, 0, errUnrefData),
            new TestFileReader_testRemaining(0, 5)
        ])
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcdefghEXTRA", 13), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: slice<TestFileReader_testFnc>([
            new TestFileReader_testRemaining(15, 13),
            new TestFileReader_testWriteTo(new fileOps(new any[]{(int64)1, abcˢ, (int64)2, defghˢ, (int64)4}.slice()), 15, errUnrefData),
            new TestFileReader_testRead(100, "", errUnrefData),
            new TestFileReader_testRemaining(0, 5)
        ])
    )
    ]);

    foreach (var (i, v) in vectors) {
        global::go.archive.tar_package.fileReader fr = default;
        switch (v.maker.type()) {
        case TestFileReader_makeReg maker: {
            ref var r = ref heap<testNonEmptyReader>(new testNonEmptyReader(new tar_test_package.strings_ReaderжReader(strings.NewReader(maker.str))), out var Ꮡr);
            fr = new global::go.archive.tar_package.regFileReaderжfileReader(Ꮡ(new regFileReader(r, maker.size)));
            break;
        }
        case TestFileReader_makeSparse maker: {
            if (!validateSparseEntries(maker.spd, maker.size)) {
                t.Fatalf("invalid sparse map: %v", maker.spd);
            }
            var sph = invertSparseEntries(maker.spd, maker.size);
            ref var r = ref heap<testNonEmptyReader>(new testNonEmptyReader(new tar_test_package.strings_ReaderжReader(strings.NewReader(maker.makeReg.str))), out var Ꮡr);
            fr = new global::go.archive.tar_package.regFileReaderжfileReader(Ꮡ(new regFileReader(r, maker.makeReg.size)));
            fr = new global::go.archive.tar_package.sparseFileReaderжfileReader(Ꮡ(new sparseFileReader(fr, sph, 0)));
            break;
        }
        default: {
            var maker = v.maker;
            t.Fatalf("test %d, unknown make operation: %T", i, maker);
            break;
        }}

        foreach (var (j, tf) in v.tests) {
            switch (tf.type()) {
            case TestFileReader_testRead tfΔ1: {
                var b = new slice<byte>(tfΔ1.cnt);
                var (n, err) = fr.Read(b);
                {
                    @string got = (@string)(b.slice(0, n)); if (got != tfΔ1.wantStr || !AreEqual(err, tfΔ1.wantErr)) {
                        t.Errorf("test %d.%d, Read(%d):\ngot  (%q, %v)\nwant (%q, %v)", i, j, tfΔ1.cnt, got, err, tfΔ1.wantStr, tfΔ1.wantErr);
                    }
                }
                break;
            }
            case TestFileReader_testWriteTo tfΔ1: {
                var f = Ꮡ(new testFile(ops: tfΔ1.ops));
                var (got, err) = fr.WriteTo(new tar_internal_test_package.testFileжWriter(f));
                {
                    var (_, ok) = err._<testError>(ᐧ); if (ok) {
                        t.Errorf("test %d.%d, WriteTo(): %v", i, j, err);
                    } else if (got != tfΔ1.wantCnt || !AreEqual(err, tfΔ1.wantErr)) {
                        t.Errorf("test %d.%d, WriteTo() = (%d, %v), want (%d, %v)", i, j, got, err, tfΔ1.wantCnt, tfΔ1.wantErr);
                    }
                }
                if (len(f.ops) > 0) {
                    t.Errorf("test %d.%d, expected %d more operations", i, j, len(f.ops));
                }
                break;
            }
            case TestFileReader_testRemaining tfΔ1: {
                {
                    var got = fr.logicalRemaining(); if (got != tfΔ1.wantLCnt) {
                        t.Errorf("test %d.%d, logicalRemaining() = %d, want %d", i, j, got, tfΔ1.wantLCnt);
                    }
                }
                {
                    var got = fr.physicalRemaining(); if (got != tfΔ1.wantPCnt) {
                        t.Errorf("test %d.%d, physicalRemaining() = %d, want %d", i, j, got, tfΔ1.wantPCnt);
                    }
                }
                break;
            }
            default: {
                var tfΔ1 = tf;
                t.Fatalf("test %d.%d, unknown test operation: %T", i, j, tfΔ1);
                break;
            }}
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string godebugˢ => ref godebugˢᵇ;
private static readonly @string godebugˢᵇ = "GODEBUG"u8;
internal static ref readonly @string tarinsecurepath0ˢ => ref tarinsecurepath0ˢᵇ;
private static readonly @string tarinsecurepath0ˢᵇ = "tarinsecurepath=0"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string securePathᶜ => ref securePathᶜᵇ;
private static readonly @string securePathᶜᵇ = "secure"u8;

public static void TestInsecurePaths(ж<testing.T> t) {
    t.Setenv(godebugˢ, tarinsecurepath0ˢ);
    foreach (var (_, vᴛ1) in slice<@string>([
        "../foo",
        "/foo",
        "a/b/../../../c"
    ])) {
        ref var path = ref heap(new @string(), out var Ꮡpath);
        path = vᴛ1;

        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
        tw.WriteHeader(Ꮡ(new Header(
            Name: path
        )));
        @string securePath = securePathᶜ;
        tw.WriteHeader(Ꮡ(new Header(
            Name: securePath
        )));
        tw.Close();

        var tr = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuf));
        var (h, err) = tr.Next();
        if (!AreEqual(err, ErrInsecurePath)) {
            t.Errorf("tr.Next for file %q: got err %v, want ErrInsecurePath", path, err);
            continue;
        }
        if (h.Name != path) {
            t.Errorf("tr.Next for file %q: got name %q, want %q", path, h.Name, path);
        }
        // Error should not be sticky.
        (h, err) = tr.Next();
        if (err != nil) {
            t.Errorf("tr.Next for file %q: got err %v, want nil", securePath, err);
        }
        if (h.Name != securePath) {
            t.Errorf("tr.Next for file %q: got name %q, want %q", securePath, h.Name, securePath);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tarinsecurepath1ˢ => ref tarinsecurepath1ˢᵇ;
private static readonly @string tarinsecurepath1ˢᵇ = "tarinsecurepath=1"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string nameᶜ => ref nameᶜᵇ;
private static readonly @string nameᶜᵇ = "/foo"u8;

public static void TestDisableInsecurePathCheck(ж<testing.T> t) {
    t.Setenv(godebugˢ, tarinsecurepath1ˢ);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    @string name = nameᶜ;
    tw.WriteHeader(Ꮡ(new Header(
        Name: name
    )));
    tw.Close();
    var tr = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuf));
    var (h, err) = tr.Next();
    if (err != nil) {
        t.Fatalf("tr.Next with tarinsecurepath=1: got err %v, want nil", err);
    }
    if (h.Name != name) {
        t.Fatalf("tr.Next with tarinsecurepath=1: got name %q, want %q", h.Name, name);
    }
}

} // end tar_internal_test_package
