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
using compress;
using hash;
using hash = hash_package;
using static go.archive.tar_package;
using ꓸꓸꓸstring = Span<@string>;

partial class tar_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string longˢ = "long"u8;
internal static readonly @string bz2ˢ = ".bz2"u8;

[GoType("dyn")] internal partial struct TestReader_vectors {
    internal @string @file;   // Test input file
    internal slice<ж<global::go.archive.tar_package.Header>> headers; // Expected output headers
    internal slice<@string> chksums; // CRC32 checksum of files, leave as nil if not checked
    internal error err;     // Expected error to occur
}

public static void TestReader(ж<testing.T> Ꮡt) {
    var vectors = new TestReader_vectors[]{new(
        @file: "testdata/gnu.tar"u8,
        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small.txt"u8,
            Mode: 416,
            Uid: 73025,
            Gid: 5000,
            Size: 5,
            ModTime: time.Unix(1244428340, 0),
            Typeflag: (rune)'0',
            Uname: "dsymonds"u8,
            Gname: "eng"u8,
            Format: FormatGNU)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small2.txt"u8,
            Mode: 416,
            Uid: 73025,
            Gid: 5000,
            Size: 11,
            ModTime: time.Unix(1244436044, 0),
            Typeflag: (rune)'0',
            Uname: "dsymonds"u8,
            Gname: "eng"u8,
            Format: FormatGNU))
        }.slice(),
        chksums: new @string[]{
            "6cbd88fc"u8,
            "ddac04b3"u8
        }.slice()
    ), new(
        @file: "testdata/sparse-formats.tar"u8,
        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse-gnu"u8,
            Mode: 420,
            Uid: 1000,
            Gid: 1000,
            Size: 200,
            ModTime: time.Unix(1392395740, 0),
            Typeflag: 0x53,
            Linkname: ""u8,
            Uname: "david"u8,
            Gname: "david"u8,
            Devmajor: 0,
            Devminor: 0,
            Format: FormatGNU)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse-posix-0.0"u8,
            Mode: 420,
            Uid: 1000,
            Gid: 1000,
            Size: 200,
            ModTime: time.Unix(1392342187, 0),
            Typeflag: 0x30,
            Linkname: ""u8,
            Uname: "david"u8,
            Gname: "david"u8,
            Devmajor: 0,
            Devminor: 0,
            PAXRecords: new map<@string, @string>{
                ["GNU.sparse.size"u8] = "200"u8,
                ["GNU.sparse.numblocks"u8] = "95"u8,
                ["GNU.sparse.map"u8] = "1,1,3,1,5,1,7,1,9,1,11,1,13,1,15,1,17,1,19,1,21,1,23,1,25,1,27,1,29,1,31,1,33,1,35,1,37,1,39,1,41,1,43,1,45,1,47,1,49,1,51,1,53,1,55,1,57,1,59,1,61,1,63,1,65,1,67,1,69,1,71,1,73,1,75,1,77,1,79,1,81,1,83,1,85,1,87,1,89,1,91,1,93,1,95,1,97,1,99,1,101,1,103,1,105,1,107,1,109,1,111,1,113,1,115,1,117,1,119,1,121,1,123,1,125,1,127,1,129,1,131,1,133,1,135,1,137,1,139,1,141,1,143,1,145,1,147,1,149,1,151,1,153,1,155,1,157,1,159,1,161,1,163,1,165,1,167,1,169,1,171,1,173,1,175,1,177,1,179,1,181,1,183,1,185,1,187,1,189,1"u8
            },
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse-posix-0.1"u8,
            Mode: 420,
            Uid: 1000,
            Gid: 1000,
            Size: 200,
            ModTime: time.Unix(1392340456, 0),
            Typeflag: 0x30,
            Linkname: ""u8,
            Uname: "david"u8,
            Gname: "david"u8,
            Devmajor: 0,
            Devminor: 0,
            PAXRecords: new map<@string, @string>{
                ["GNU.sparse.size"u8] = "200"u8,
                ["GNU.sparse.numblocks"u8] = "95"u8,
                ["GNU.sparse.map"u8] = "1,1,3,1,5,1,7,1,9,1,11,1,13,1,15,1,17,1,19,1,21,1,23,1,25,1,27,1,29,1,31,1,33,1,35,1,37,1,39,1,41,1,43,1,45,1,47,1,49,1,51,1,53,1,55,1,57,1,59,1,61,1,63,1,65,1,67,1,69,1,71,1,73,1,75,1,77,1,79,1,81,1,83,1,85,1,87,1,89,1,91,1,93,1,95,1,97,1,99,1,101,1,103,1,105,1,107,1,109,1,111,1,113,1,115,1,117,1,119,1,121,1,123,1,125,1,127,1,129,1,131,1,133,1,135,1,137,1,139,1,141,1,143,1,145,1,147,1,149,1,151,1,153,1,155,1,157,1,159,1,161,1,163,1,165,1,167,1,169,1,171,1,173,1,175,1,177,1,179,1,181,1,183,1,185,1,187,1,189,1"u8,
                ["GNU.sparse.name"u8] = "sparse-posix-0.1"u8
            },
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse-posix-1.0"u8,
            Mode: 420,
            Uid: 1000,
            Gid: 1000,
            Size: 200,
            ModTime: time.Unix(1392337404, 0),
            Typeflag: 0x30,
            Linkname: ""u8,
            Uname: "david"u8,
            Gname: "david"u8,
            Devmajor: 0,
            Devminor: 0,
            PAXRecords: new map<@string, @string>{
                ["GNU.sparse.major"u8] = "1"u8,
                ["GNU.sparse.minor"u8] = "0"u8,
                ["GNU.sparse.realsize"u8] = "200"u8,
                ["GNU.sparse.name"u8] = "sparse-posix-1.0"u8
            },
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "end"u8,
            Mode: 420,
            Uid: 1000,
            Gid: 1000,
            Size: 4,
            ModTime: time.Unix(1392398319, 0),
            Typeflag: 0x30,
            Linkname: ""u8,
            Uname: "david"u8,
            Gname: "david"u8,
            Devmajor: 0,
            Devminor: 0,
            Format: FormatGNU))
        }.slice(),
        chksums: new @string[]{
            "5375e1d2"u8,
            "5375e1d2"u8,
            "5375e1d2"u8,
            "5375e1d2"u8,
            "8eb179ba"u8
        }.slice()
    ), new(
        @file: "testdata/star.tar"u8,
        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small.txt"u8,
            Mode: 416,
            Uid: 73025,
            Gid: 5000,
            Size: 5,
            ModTime: time.Unix(1244592783, 0),
            Typeflag: (rune)'0',
            Uname: "dsymonds"u8,
            Gname: "eng"u8,
            AccessTime: time.Unix(1244592783, 0),
            ChangeTime: time.Unix(1244592783, 0))), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small2.txt"u8,
            Mode: 416,
            Uid: 73025,
            Gid: 5000,
            Size: 11,
            ModTime: time.Unix(1244592783, 0),
            Typeflag: (rune)'0',
            Uname: "dsymonds"u8,
            Gname: "eng"u8,
            AccessTime: time.Unix(1244592783, 0),
            ChangeTime: time.Unix(1244592783, 0)))
        }.slice()
    ), new(
        @file: "testdata/v7.tar"u8,
        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small.txt"u8,
            Mode: 292,
            Uid: 73025,
            Gid: 5000,
            Size: 5,
            ModTime: time.Unix(1244593104, 0),
            Typeflag: (rune)'0')), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small2.txt"u8,
            Mode: 292,
            Uid: 73025,
            Gid: 5000,
            Size: 11,
            ModTime: time.Unix(1244593104, 0),
            Typeflag: (rune)'0'))
        }.slice()
    ), new(
        @file: "testdata/pax.tar"u8,
        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "a/123456789101112131415161718192021222324252627282930313233343536373839404142434445464748495051525354555657585960616263646566676869707172737475767778798081828384858687888990919293949596979899100"u8,
            Mode: 436,
            Uid: 1000,
            Gid: 1000,
            Uname: "shane"u8,
            Gname: "shane"u8,
            Size: 7,
            ModTime: time.Unix(1350244992, 23960108),
            ChangeTime: time.Unix(1350244992, 23960108),
            AccessTime: time.Unix(1350244992, 23960108),
            Typeflag: TypeReg,
            PAXRecords: new map<@string, @string>{
                ["path"u8] = "a/123456789101112131415161718192021222324252627282930313233343536373839404142434445464748495051525354555657585960616263646566676869707172737475767778798081828384858687888990919293949596979899100"u8,
                ["mtime"u8] = "1350244992.023960108"u8,
                ["atime"u8] = "1350244992.023960108"u8,
                ["ctime"u8] = "1350244992.023960108"u8
            },
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "a/b"u8,
            Mode: 511,
            Uid: 1000,
            Gid: 1000,
            Uname: "shane"u8,
            Gname: "shane"u8,
            Size: 0,
            ModTime: time.Unix(1350266320, 910238425),
            ChangeTime: time.Unix(1350266320, 910238425),
            AccessTime: time.Unix(1350266320, 910238425),
            Typeflag: TypeSymlink,
            Linkname: "123456789101112131415161718192021222324252627282930313233343536373839404142434445464748495051525354555657585960616263646566676869707172737475767778798081828384858687888990919293949596979899100"u8,
            PAXRecords: new map<@string, @string>{
                ["linkpath"u8] = "123456789101112131415161718192021222324252627282930313233343536373839404142434445464748495051525354555657585960616263646566676869707172737475767778798081828384858687888990919293949596979899100"u8,
                ["mtime"u8] = "1350266320.910238425"u8,
                ["atime"u8] = "1350266320.910238425"u8,
                ["ctime"u8] = "1350266320.910238425"u8
            },
            Format: FormatPAX))
        }.slice()
    ), new(
        @file: "testdata/pax-bad-hdr-file.tar"u8,
        err: ErrHeader
    ), new(
        @file: "testdata/pax-bad-hdr-large.tar.bz2"u8,
        err: ErrFieldTooLong
    ), new(
        @file: "testdata/pax-bad-mtime-file.tar"u8,
        err: ErrHeader
    ), new(
        @file: "testdata/pax-pos-size-file.tar"u8,
        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "foo"u8,
            Mode: 416,
            Uid: 319973,
            Gid: 5000,
            Size: 999,
            ModTime: time.Unix(1442282516, 0),
            Typeflag: (rune)'0',
            Uname: "joetsai"u8,
            Gname: "eng"u8,
            PAXRecords: new map<@string, @string>{
                ["size"u8] = "000000000000000000000999"u8
            },
            Format: FormatPAX))
        }.slice(),
        chksums: new @string[]{
            "5fd7e86a"u8
        }.slice()
    ), new(
        @file: "testdata/pax-records.tar"u8,
        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeReg,
            Name: "file"u8,
            Uname: strings.Repeat(longˢ, 10),
            ModTime: time.Unix(0, 0),
            PAXRecords: new map<@string, @string>{
                ["GOLANG.pkg"u8] = "tar"u8,
                ["comment"u8] = "Hello, 世界"u8,
                ["uname"u8] = strings.Repeat(longˢ, 10)
            },
            Format: FormatPAX))
        }.slice()
    ), new(
        @file: "testdata/pax-global-records.tar"u8,
        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeXGlobalHeader,
            Name: "global1"u8,
            PAXRecords: new map<@string, @string>{["path"u8] = "global1"u8, ["mtime"u8] = "1500000000.0"u8},
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeReg,
            Name: "file1"u8,
            ModTime: time.Unix(0, 0),
            Format: FormatUSTAR)), Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeReg,
            Name: "file2"u8,
            PAXRecords: new map<@string, @string>{["path"u8] = "file2"u8},
            ModTime: time.Unix(0, 0),
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeXGlobalHeader,
            Name: "GlobalHead.0.0"u8,
            PAXRecords: new map<@string, @string>{["path"u8] = ""u8},
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeReg,
            Name: "file3"u8,
            ModTime: time.Unix(0, 0),
            Format: FormatUSTAR)), Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeReg,
            Name: "file4"u8,
            ModTime: time.Unix(1400000000, 0),
            PAXRecords: new map<@string, @string>{["mtime"u8] = "1400000000"u8},
            Format: FormatPAX))
        }.slice()
    ), new(
        @file: "testdata/nil-uid.tar"u8, // golang.org/issue/5290

        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "P1050238.JPG.log"u8,
            Mode: 436,
            Uid: 0,
            Gid: 0,
            Size: 14,
            ModTime: time.Unix(1365454838, 0),
            Typeflag: TypeReg,
            Linkname: ""u8,
            Uname: "eyefi"u8,
            Gname: "eyefi"u8,
            Devmajor: 0,
            Devminor: 0,
            Format: FormatGNU))
        }.slice()
    ), new(
        @file: "testdata/xattrs.tar"u8,
        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small.txt"u8,
            Mode: 420,
            Uid: 1000,
            Gid: 10,
            Size: 5,
            ModTime: time.Unix(1386065770, 448252320),
            Typeflag: (rune)'0',
            Uname: "alex"u8,
            Gname: "wheel"u8,
            AccessTime: time.Unix(1389782991, 419875220),
            ChangeTime: time.Unix(1389782956, 794414986),
            Xattrs: new map<@string, @string>{
                ["user.key"u8] = "value"u8,
                ["user.key2"u8] = "value2"u8, // Interestingly, selinux encodes the terminating null inside the xattr

                ["security.selinux"u8] = "unconfined_u:object_r:default_t:s0\x00"u8
            },
            PAXRecords: new map<@string, @string>{
                ["mtime"u8] = "1386065770.44825232"u8,
                ["atime"u8] = "1389782991.41987522"u8,
                ["ctime"u8] = "1389782956.794414986"u8,
                ["SCHILY.xattr.user.key"u8] = "value"u8,
                ["SCHILY.xattr.user.key2"u8] = "value2"u8,
                ["SCHILY.xattr.security.selinux"u8] = "unconfined_u:object_r:default_t:s0\x00"u8
            },
            Format: FormatPAX)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "small2.txt"u8,
            Mode: 420,
            Uid: 1000,
            Gid: 10,
            Size: 11,
            ModTime: time.Unix(1386065770, 449252304),
            Typeflag: (rune)'0',
            Uname: "alex"u8,
            Gname: "wheel"u8,
            AccessTime: time.Unix(1389782991, 419875220),
            ChangeTime: time.Unix(1386065770, 449252304),
            Xattrs: new map<@string, @string>{
                ["security.selinux"u8] = "unconfined_u:object_r:default_t:s0\x00"u8
            },
            PAXRecords: new map<@string, @string>{
                ["mtime"u8] = "1386065770.449252304"u8,
                ["atime"u8] = "1389782991.41987522"u8,
                ["ctime"u8] = "1386065770.449252304"u8,
                ["SCHILY.xattr.security.selinux"u8] = "unconfined_u:object_r:default_t:s0\x00"u8
            },
            Format: FormatPAX))
        }.slice()
    ), new(
        @file: "testdata/gnu-multi-hdrs.tar"u8, // Matches the behavior of GNU, BSD, and STAR tar utilities.

        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "GNU2/GNU2/long-path-name"u8,
            Linkname: "GNU4/GNU4/long-linkpath-name"u8,
            ModTime: time.Unix(0, 0),
            Typeflag: (rune)'2',
            Format: FormatGNU))
        }.slice()
    ), new(
        @file: "testdata/gnu-incremental.tar"u8, // GNU tar file with atime and ctime fields set.
 // Created with the GNU tar v1.27.1.
 //	tar --incremental -S -cvf gnu-incremental.tar test2

        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "test2/"u8,
            Mode: 16877,
            Uid: 1000,
            Gid: 1000,
            Size: 14,
            ModTime: time.Unix(1441973427, 0),
            Typeflag: (rune)'D',
            Uname: "rawr"u8,
            Gname: "dsnet"u8,
            AccessTime: time.Unix(1441974501, 0),
            ChangeTime: time.Unix(1441973436, 0),
            Format: FormatGNU)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "test2/foo"u8,
            Mode: 33188,
            Uid: 1000,
            Gid: 1000,
            Size: 64,
            ModTime: time.Unix(1441973363, 0),
            Typeflag: (rune)'0',
            Uname: "rawr"u8,
            Gname: "dsnet"u8,
            AccessTime: time.Unix(1441974501, 0),
            ChangeTime: time.Unix(1441973436, 0),
            Format: FormatGNU)), Ꮡ(new global::go.archive.tar_package.Header(
            Name: "test2/sparse"u8,
            Mode: 33188,
            Uid: 1000,
            Gid: 1000,
            Size: 536870912,
            ModTime: time.Unix(1441973427, 0),
            Typeflag: (rune)'S',
            Uname: "rawr"u8,
            Gname: "dsnet"u8,
            AccessTime: time.Unix(1441991948, 0),
            ChangeTime: time.Unix(1441973436, 0),
            Format: FormatGNU))
        }.slice()
    ), new(
        @file: "testdata/pax-multi-hdrs.tar"u8, // Matches the behavior of GNU and BSD tar utilities.

        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "bar"u8,
            Linkname: "PAX4/PAX4/long-linkpath-name"u8,
            ModTime: time.Unix(0, 0),
            Typeflag: (rune)'2',
            PAXRecords: new map<@string, @string>{
                ["linkpath"u8] = "PAX4/PAX4/long-linkpath-name"u8
            },
            Format: FormatPAX))
        }.slice()
    ), new(
        @file: "testdata/gnu-long-nul.tar"u8, // Both BSD and GNU tar truncate long names at first NUL even
 // if there is data following that NUL character.
 // This is reasonable as GNU long names are C-strings.

        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "0123456789"u8,
            Mode: 420,
            Uid: 1000,
            Gid: 1000,
            ModTime: time.Unix(1486082191, 0),
            Typeflag: (rune)'0',
            Uname: "rawr"u8,
            Gname: "dsnet"u8,
            Format: FormatGNU))
        }.slice()
    ), new(
        @file: "testdata/gnu-utf8.tar"u8, // This archive was generated by Writer but is readable by both
 // GNU and BSD tar utilities.
 // The archive generated by GNU is nearly byte-for-byte identical
 // to the Go version except the Go version sets a negative Devminor
 // just to force the GNU format.

        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹"u8,
            Mode: 420,
            Uid: 1000, Gid: 1000,
            ModTime: time.Unix(0, 0),
            Typeflag: (rune)'0',
            Uname: "☺"u8,
            Gname: "⚹"u8,
            Format: FormatGNU))
        }.slice()
    ), new(
        @file: "testdata/gnu-not-utf8.tar"u8, // This archive was generated by Writer but is readable by both
 // GNU and BSD tar utilities.
 // The archive generated by GNU is nearly byte-for-byte identical
 // to the Go version except the Go version sets a negative Devminor
 // just to force the GNU format.

        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: ((@string)(new byte[]{0x68, 0x69, 0x80, 0x81, 0x82, 0x83, 0x62, 0x79, 0x65})),
            Mode: 420,
            Uid: 1000,
            Gid: 1000,
            ModTime: time.Unix(0, 0),
            Typeflag: (rune)'0',
            Uname: "rawr"u8,
            Gname: "dsnet"u8,
            Format: FormatGNU))
        }.slice()
    ), new(
        @file: "testdata/pax-nul-xattrs.tar"u8, // BSD tar v3.1.2 and GNU tar v1.27.1 both rejects PAX records
 // with NULs in the key.

        err: ErrHeader
    ), new(
        @file: "testdata/pax-nul-path.tar"u8, // BSD tar v3.1.2 rejects a PAX path with NUL in the value, while
 // GNU tar v1.27.1 simply truncates at first NUL.
 // We emulate the behavior of BSD since it is strange doing NUL
 // truncations since PAX records are length-prefix strings instead
 // of NUL-terminated C-strings.

        err: ErrHeader
    ), new(
        @file: "testdata/neg-size.tar"u8,
        err: ErrHeader
    ), new(
        @file: "testdata/issue10968.tar"u8,
        err: ErrHeader
    ), new(
        @file: "testdata/issue11169.tar"u8,
        err: ErrHeader
    ), new(
        @file: "testdata/issue12435.tar"u8,
        err: ErrHeader
    ), new(
        @file: "testdata/invalid-go17.tar"u8, // Ensure that we can read back the original Header as written with
 // a buggy pre-Go1.8 tar.Writer.

        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa/foo"u8,
            Uid: 2097152,
            ModTime: time.Unix(0, 0),
            Typeflag: (rune)'0'))
        }.slice()
    ), new(
        @file: "testdata/ustar-file-devs.tar"u8, // USTAR archive with a regular entry with non-zero device numbers.

        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "file"u8,
            Mode: 420,
            Typeflag: (rune)'0',
            ModTime: time.Unix(0, 0),
            Devmajor: 1,
            Devminor: 1,
            Format: FormatUSTAR))
        }.slice()
    ), new(
        @file: "testdata/gnu-nil-sparse-data.tar"u8, // Generated by Go, works on BSD tar v3.1.2 and GNU tar v.1.27.1.

        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse.db"u8,
            Typeflag: TypeGNUSparse,
            Size: 1000,
            ModTime: time.Unix(0, 0),
            Format: FormatGNU))
        }.slice()
    ), new(
        @file: "testdata/gnu-nil-sparse-hole.tar"u8, // Generated by Go, works on BSD tar v3.1.2 and GNU tar v.1.27.1.

        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse.db"u8,
            Typeflag: TypeGNUSparse,
            Size: 1000,
            ModTime: time.Unix(0, 0),
            Format: FormatGNU))
        }.slice()
    ), new(
        @file: "testdata/pax-nil-sparse-data.tar"u8, // Generated by Go, works on BSD tar v3.1.2 and GNU tar v.1.27.1.

        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse.db"u8,
            Typeflag: TypeReg,
            Size: 1000,
            ModTime: time.Unix(0, 0),
            PAXRecords: new map<@string, @string>{
                ["size"u8] = "1512"u8,
                ["GNU.sparse.major"u8] = "1"u8,
                ["GNU.sparse.minor"u8] = "0"u8,
                ["GNU.sparse.realsize"u8] = "1000"u8,
                ["GNU.sparse.name"u8] = "sparse.db"u8
            },
            Format: FormatPAX))
        }.slice()
    ), new(
        @file: "testdata/pax-nil-sparse-hole.tar"u8, // Generated by Go, works on BSD tar v3.1.2 and GNU tar v.1.27.1.

        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Name: "sparse.db"u8,
            Typeflag: TypeReg,
            Size: 1000,
            ModTime: time.Unix(0, 0),
            PAXRecords: new map<@string, @string>{
                ["size"u8] = "512"u8,
                ["GNU.sparse.major"u8] = "1"u8,
                ["GNU.sparse.minor"u8] = "0"u8,
                ["GNU.sparse.realsize"u8] = "1000"u8,
                ["GNU.sparse.name"u8] = "sparse.db"u8
            },
            Format: FormatPAX))
        }.slice()
    ), new(
        @file: "testdata/trailing-slash.tar"u8,
        headers: new ж<global::go.archive.tar_package.Header>[]{Ꮡ(new global::go.archive.tar_package.Header(
            Typeflag: TypeDir,
            Name: strings.Repeat("123456789/"u8, 30),
            ModTime: time.Unix(0, 0),
            PAXRecords: new map<@string, @string>{
                ["path"u8] = strings.Repeat("123456789/"u8, 30)
            },
            Format: FormatPAX))
        }.slice()
    ), new(
        @file: "testdata/gnu-sparse-many-zeros.tar.bz2"u8, // Small compressed file that uncompresses to
 // a file with a very large GNU 1.0 sparse map.

        err: errSparseTooLong
    )
    }.slice();
    foreach (var (_, vᴛ1) in vectors) {
        ref var v = ref heap(new TestReader_vectors(), out var Ꮡv);
        v = vᴛ1;

        var vʗ1 = v;
        Ꮡt.Run(path.Base(v.@file), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var (f, err) = os.Open(vʗ1.@file);
                if (err != default!) {
                    tΔ1.Fatalf("unexpected error: %v"u8, err);
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
                io.Reader fr = new tar_test_package.os_FileжReader(f);
                if (strings.HasSuffix(vʗ1.@file, bz2ˢ)) {
                    fr = bzip2.NewReader(fr);
                }
                // Capture all headers and checksums.
                ж<global::go.archive.tar_package.Reader> tr = NewReader(fr);
                
                slice<ж<global::go.archive.tar_package.Header>> hdrs = default!;
                
                slice<@string> chksums = default!;
                
                slice<byte> rdbuf = new slice<byte>(8);
                while (ᐧ) {
                    ж<global::go.archive.tar_package.Header> hdr = default!;
                    (hdr, err) = tr.Next();
                    if (err != default!) {
                        if (AreEqual(err, io.EOF)) {
                            err = default!; // Expected error
                        }
                        break;
                    }
                    hdrs = append(hdrs, hdr);
                    if (vʗ1.chksums == default!) {
                        continue;
                    }
                    var h = crc32.NewIEEE();
                    (_, err) = io.CopyBuffer(new tar_test_package.hash_Hash32ᴠWriter(h), new global::go.archive.tar_package.ReaderжReader(tr), rdbuf); // Effectively an incremental read
                    if (err != default!) {
                        break;
                    }
                    chksums = append(chksums, fmt.Sprintf("%x"u8, h.Sum(default!)));
                }
                foreach (var (i, hdr) in hdrs) {
                    if (i >= len(vʗ1.headers)) {
                        tΔ1.Fatalf("entry %d: unexpected header:\ngot %+v"u8, i, hdr.Value);
                    }
                    if (!reflect.DeepEqual(hdr.Value, vʗ1.headers[i].Value)) {
                        tΔ1.Fatalf("entry %d: incorrect header:\ngot  %+v\nwant %+v"u8, i, hdr.Value, vʗ1.headers[i].Value);
                    }
                }
                if (len(hdrs) != len(vʗ1.headers)) {
                    tΔ1.Fatalf("got %d headers, want %d headers"u8, len(hdrs), len(vʗ1.headers));
                }
                foreach (var (i, sum) in chksums) {
                    if (i >= len(vʗ1.chksums)) {
                        tΔ1.Fatalf("entry %d: unexpected sum: got %s"u8, i, sum);
                    }
                    if (sum != vʗ1.chksums[i]) {
                        tΔ1.Fatalf("entry %d: incorrect checksum: got %s, want %s"u8, i, sum, vʗ1.chksums[i]);
                    }
                }
                if (!AreEqual(err, vʗ1.err)) {
                    tΔ1.Fatalf("unexpected error: got %v, want %v"u8, err, vʗ1.err);
                }
                f.Close();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

[GoType("dyn")] [GoLocalName("testCase")] internal partial struct TestPartialRead_testCase {
    internal nint cnt;   // Number of bytes to read
    internal @string output; // Expected value of string read
}

[GoType("dyn")] internal partial struct TestPartialRead_vectors {
    internal @string @file;
    internal slice<TestPartialRead_testCase> cases;
}

public static void TestPartialRead(ж<testing.T> Ꮡt) {
    var vectors = new TestPartialRead_vectors[]{new(
        @file: "testdata/gnu.tar"u8,
        cases: new TestPartialRead_testCase[]{
            new(4, "Kilt"u8),
            new(6, "Google"u8)
        }.slice()
    ), new(
        @file: "testdata/sparse-formats.tar"u8,
        cases: new TestPartialRead_testCase[]{
            new(2, "\x00G"u8),
            new(4, "\x00G\x00o"u8),
            new(6, "\x00G\x00o\x00G"u8),
            new(8, "\x00G\x00o\x00G\x00o"u8),
            new(4, "end\n"u8)
        }.slice()
    )
    }.slice();
    foreach (var (_, vᴛ1) in vectors) {
        ref var v = ref heap(new TestPartialRead_vectors(), out var Ꮡv);
        v = vᴛ1;

        var vʗ1 = v;
        Ꮡt.Run(path.Base(v.@file), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                var (f, err) = os.Open(vʗ1.@file);
                if (err != default!) {
                    tΔ1.Fatalf("Open() error: %v"u8, err);
                }
                var fʗ1 = f;
                defer(() => fʗ1.Close(), ref ᒐ);
                var tr = NewReader(new tar_test_package.os_FileжReader(f));
                foreach (var (i, tc) in vʗ1.cases) {
                    var (hdr, errΔ1) = tr.Next();
                    if (errΔ1 != default! || hdr == nil) {
                        tΔ1.Fatalf("entry %d, Next(): got %v, want %v"u8, i, errΔ1, (any)(default!));
                    }
                    var buf = new slice<byte>(tc.cnt);
                    {
                        var (_, errΔ2) = io.ReadFull(new global::go.archive.tar_package.ReaderжReader(tr), buf); if (errΔ2 != default!) {
                            tΔ1.Fatalf("entry %d, ReadFull(): got %v, want %v"u8, i, errΔ2, (any)(default!));
                        }
                    }
                    if (((sstring)buf) != tc.output) {
                        tΔ1.Fatalf("entry %d, ReadFull(): got %q, want %q"u8, i, ((@string)buf), tc.output);
                    }
                }
                {
                    var (_, errΔ3) = tr.Next(); if (!AreEqual(errΔ3, io.EOF)) {
                        tΔ1.Fatalf("Next(): got %v, want EOF"u8, errΔ3);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataGnuTarˢ = "testdata/gnu.tar"u8;

public static void TestUninitializedRead(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(testdataGnuTarˢ);
        if (err != default!) {
            Ꮡt.Fatalf("Unexpected error: %v"u8, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        var tr = NewReader(new tar_test_package.os_FileжReader(f));
        (_, err) = tr.Read(new byte[]{}.slice());
        if (err == default! || !AreEqual(err, io.EOF)) {
            Ꮡt.Errorf("Unexpected error: %v, wanted %v"u8, err, io.EOF);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] internal partial struct reader {
    [GoEmbedded] public io_package.Reader Reader;
}

[GoType] internal partial struct readSeeker {
    [GoEmbedded] public io_package.ReadSeeker ReadSeeker;
}

[GoType] internal partial struct readBadSeeker {
    [GoEmbedded] public io_package.ReadSeeker ReadSeeker;
}

// Go method set entry for the promoted 'ReadSeeker.Read()' - provided ONLY by the embedded
// interface field in *readBadSeeker's method set; see the pointer-only satisfaction record.
internal static (nint, error) Read(this readBadSeeker recvᴛ, slice<byte> p) => recvᴛ.ReadSeeker.Read(p);

[GoRecv] internal static (int64, error) Seek(this ref readBadSeeker rbs, int64 _Δp1, nint _Δp2) {
    return (0, fmt.Errorf("illegal seek"u8));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string garbageˢ = "garbage "u8;
internal static readonly @string ioReaderˢ = "io.Reader"u8;
internal static readonly @string autoˢ = "auto"u8;
internal static readonly @string manualˢ = "manual"u8;
internal static readonly @string ioReadSeekerˢ = "io.ReadSeeker"u8;
internal static readonly @string readBadSeekerˢ = "ReadBadSeeker"u8;

[GoType("dyn")] internal partial struct TestReadTruncation_vectors {
    internal @string input; // Input stream
    internal nint cnt;   // Expected number of headers read
    internal error err;  // Expected error outcome
}

// TestReadTruncation test the ending condition on various truncated files and
// that truncated files are still detected even if the underlying io.Reader
// satisfies io.Seeker.
public static void TestReadTruncation(ж<testing.T> Ꮡt) {
    slice<@string> ss = default!;
    foreach (var (_, p) in new @string[]{
        "testdata/gnu.tar"u8,
        "testdata/ustar-file-reg.tar"u8,
        "testdata/pax-path-hdr.tar"u8,
        "testdata/sparse-formats.tar"u8
    }.slice()) {
        var (buf, err) = os.ReadFile(p);
        if (err != default!) {
            Ꮡt.Fatalf("unexpected error: %v"u8, err);
        }
        ss = append(ss, ((@string)buf));
    }
    @string data1 = ss[0];
    @string data2 = ss[1];
    @string pax = ss[2];
    @string sparse = ss[3];
    data2 += strings.Repeat("\x00"u8, 10 * 512);
    @string trash = strings.Repeat(garbageˢ, 64); // Exactly 512 bytes
    var vectors = new TestReadTruncation_vectors[]{
        new(""u8, 0, io.EOF), // Empty file is a "valid" tar file

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
    }.slice();
    foreach (var (i, v) in vectors) {
        for (nint j = 0; j < 6; j++) {
            ж<global::go.archive.tar_package.Reader> tr = default!;
            @string s1 = default!;
            @string s2 = default!;
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

            nint cnt = default!;
            error err = default!;
            while (ᐧ) {
                {
                    (_, err) = tr.Next(); if (err != default!) {
                        break;
                    }
                }
                cnt++;
                if (s2 == "manual"u8) {
                    {
                        (_, err) = tr.writeTo(io.Discard); if (err != default!) {
                            break;
                        }
                    }
                }
            }
            if (!AreEqual(err, v.err)) {
                Ꮡt.Errorf("test %d, NewReader(%s) with %s discard: got %v, want %v"u8,
                    i, s1, s2, err, v.err);
            }
            if (cnt != v.cnt) {
                Ꮡt.Errorf("test %d, NewReader(%s) with %s discard: got %d headers, want %d headers"u8,
                    i, s1, s2, cnt, v.cnt);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataHdrOnlyTarˢ = "testdata/hdr-only.tar"u8;

// TestReadHeaderOnly tests that Reader does not attempt to read special
// header-only files.
public static void TestReadHeaderOnly(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (f, err) = os.Open(testdataHdrOnlyTarˢ);
        if (err != default!) {
            Ꮡt.Fatalf("unexpected error: %v"u8, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        slice<ж<global::go.archive.tar_package.Header>> hdrs = default!;
        var tr = NewReader(new tar_test_package.os_FileжReader(f));
        while (ᐧ) {
            var (hdr, errΔ1) = tr.Next();
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            if (errΔ1 != default!) {
                Ꮡt.Errorf("Next(): got %v, want %v"u8, errΔ1, (any)(default!));
                continue;
            }
            hdrs = append(hdrs, hdr);
            // If a special flag, we should read nothing.
            var (cnt, _) = io.ReadFull(new global::go.archive.tar_package.ReaderжReader(tr), new byte[]{0}.slice());
            if (cnt > 0 && (~hdr).Typeflag != TypeReg) {
                Ꮡt.Errorf("ReadFull(...): got %d bytes, want 0 bytes"u8, cnt);
            }
        }
        // File is crafted with 16 entries. The later 8 are identical to the first
        // 8 except that the size is set.
        if (len(hdrs) != 16) {
            Ꮡt.Fatalf("len(hdrs): got %d, want %d"u8, len(hdrs), (nint)(16));
        }
        for (nint i = 0; i < 8; i++) {
            var (hdr1, hdr2) = (hdrs[i + 0], hdrs[i + 8]);
            (hdr1.Value.Size, hdr2.Value.Size) = (0, 0);
            if (!reflect.DeepEqual(hdr1.Value, hdr2.Value)) {
                Ꮡt.Errorf("incorrect header:\ngot  %+v\nwant %+v"u8, hdr1.Value, hdr2.Value);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestMergePAX_vectors {
    internal map<@string, @string> @in;
    internal ж<global::go.archive.tar_package.Header> want;
    internal bool ok;
}

public static void TestMergePAX(ж<testing.T> Ꮡt) {
    var vectors = new TestMergePAX_vectors[]{new(
        @in: new map<@string, @string>{
            ["path"u8] = "a/b/c"u8,
            ["uid"u8] = "1000"u8,
            ["mtime"u8] = "1350244992.023960108"u8
        },
        want: Ꮡ(new Header(
            Name: "a/b/c"u8,
            Uid: 1000,
            ModTime: time.Unix(1350244992, 23960108),
            PAXRecords: new map<@string, @string>{
                ["path"u8] = "a/b/c"u8,
                ["uid"u8] = "1000"u8,
                ["mtime"u8] = "1350244992.023960108"u8
            }
        )),
        ok: true
    ), new(
        @in: new map<@string, @string>{
            ["gid"u8] = "gtgergergersagersgers"u8
        },
        ok: false
    ), new(
        @in: new map<@string, @string>{
            ["missing"u8] = "missing"u8,
            ["SCHILY.xattr.key"u8] = "value"u8
        },
        want: Ꮡ(new Header(
            Xattrs: new map<@string, @string>{["key"u8] = "value"u8},
            PAXRecords: new map<@string, @string>{
                ["missing"u8] = "missing"u8,
                ["SCHILY.xattr.key"u8] = "value"u8
            }
        )),
        ok: true
    )
    }.slice();
    foreach (var (i, v) in vectors) {
        var got = @new<global::go.archive.tar_package.Header>();
        var err = mergePAX(ref (got).DerefOrNull(), v.@in);
        if (v.ok && !reflect.DeepEqual(got.Value, v.want.Value)) {
            Ꮡt.Errorf("test %d, mergePAX(...):\ngot  %+v\nwant %+v"u8, i, got.Value, v.want.Value);
        }
        {
            var ok = err == default!; if (ok != v.ok) {
                Ꮡt.Errorf("test %d, mergePAX(...): got %v, want %v"u8, i, ok, v.ok);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestParsePAX_vectors {
    internal @string @in;
    internal map<@string, @string> want;
    internal bool ok;
}

public static void TestParsePAX(ж<testing.T> Ꮡt) {
    var vectors = new TestParsePAX_vectors[]{
        new(""u8, default!, true),
        new("6 k=1\n"u8, new map<@string, @string>{["k"u8] = "1"u8}, true),
        new("10 a=name\n"u8, new map<@string, @string>{["a"u8] = "name"u8}, true),
        new("9 a=name\n"u8, new map<@string, @string>{["a"u8] = "name"u8}, true),
        new("30 mtime=1350244992.023960108\n"u8, new map<@string, @string>{["mtime"u8] = "1350244992.023960108"u8}, true),
        new("3 somelongkey=\n"u8, default!, false),
        new("50 tooshort=\n"u8, default!, false),
        new("13 key1=haha\n13 key2=nana\n13 key3=kaka\n"u8,
            new map<@string, @string>{["key1"u8] = "haha"u8, ["key2"u8] = "nana"u8, ["key3"u8] = "kaka"u8}, true),
        new("13 key1=val1\n13 key2=val2\n8 key1=\n"u8,
            new map<@string, @string>{["key1"u8] = ""u8, ["key2"u8] = "val2"u8}, true),
        new("22 GNU.sparse.size=10\n26 GNU.sparse.numblocks=2\n"u8 + "23 GNU.sparse.offset=1\n25 GNU.sparse.numbytes=2\n"u8 + "23 GNU.sparse.offset=3\n25 GNU.sparse.numbytes=4\n"u8,
            new map<@string, @string>{[paxGNUSparseSize] = "10"u8, [paxGNUSparseNumBlocks] = "2"u8, [paxGNUSparseMap] = "1,2,3,4"u8}, true),
        new("22 GNU.sparse.size=10\n26 GNU.sparse.numblocks=1\n"u8 + "25 GNU.sparse.numbytes=2\n23 GNU.sparse.offset=1\n"u8,
            default!, false),
        new("22 GNU.sparse.size=10\n26 GNU.sparse.numblocks=1\n"u8 + "25 GNU.sparse.offset=1,2\n25 GNU.sparse.numbytes=2\n"u8,
            default!, false)
    }.slice();
    foreach (var (i, v) in vectors) {
        var r = strings.NewReader(v.@in);
        var (got, err) = parsePAX(new tar_test_package.strings_ReaderжReader(r));
        if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(got, v.want) && !(len(got) == 0 && len(v.want) == 0)) {
            Ꮡt.Errorf("test %d, parsePAX():\ngot  %v\nwant %v"u8, i, got, v.want);
        }
        {
            var ok = err == default!; if (ok != v.ok) {
                Ꮡt.Errorf("test %d, parsePAX(): got %v, want %v"u8, i, ok, v.ok);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fewaˢ = "fewa"u8;

[GoType("dyn")] internal partial struct TestReadOldGNUSparseMap_vectors {
    internal slice<byte> input;
    internal global::go.archive.tar_package.sparseDatas wantMap;
    internal int64 wantSize;
    internal error wantErr;
}

public static void TestReadOldGNUSparseMap(ж<testing.T> Ꮡt) {
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
        slice<byte> @out = default!;
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
            global::go.archive.tar_package.block blkΔ1 = default!;
            sps = populateSparseMapʗ1(blkΔ1.toSparse(), sps);
            @out = appendꓸꓸꓸ(@out, blkΔ1[..]);
        }
        return @out;
    }
    slice<@string> /*out*/ makeSparseStrings(slice<global::go.archive.tar_package.sparseEntry> sp) {
        slice<@string> @out = default!;
        global::go.archive.tar_package.formatter f = default!;
        foreach (var (_, s) in sp) {
            array<byte> b = new(24);
            f.formatNumeric(b[..12], s.Offset);
            f.formatNumeric(b[12..], s.Length);
            @out = append(@out, ((@string)(b[..])));
        }
        return @out;
    }
    var vectors = new TestReadOldGNUSparseMap_vectors[]{new(
        input: makeInput(FormatUnknown, ""u8),
        wantErr: ErrHeader
    ), new(
        input: makeInput(FormatGNU, "1234"u8, fewaˢ),
        wantSize: 668,
        wantErr: ErrHeader
    ), new(
        input: makeInput(FormatGNU, "0031"u8),
        wantSize: 25
    ), new(
        input: makeInput(FormatGNU, "80"u8),
        wantErr: ErrHeader
    ), new(
        input: makeInput(FormatGNU, "1234"u8,
            makeSparseStrings(new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 0), new(1, 1)}.slice())).ꓸꓸꓸ),
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 0), new(1, 1)}.slice()),
        wantSize: 668
    ), new(
        input: makeInput(FormatGNU, "1234"u8,
            appendꓸꓸꓸ(makeSparseStrings(new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 0), new(1, 1)}.slice())), new @string[]{""u8, "blah"u8}.slice()).ꓸꓸꓸ),
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
                new @string[]{""u8, ""u8}.slice()),
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
    )
    }.slice();
    foreach (var (i, vᴛ1) in vectors) {
        var v = vᴛ1;

        ref var blk = ref heap(new global::go.archive.tar_package.block(), out var Ꮡblk);
        ref var hdr = ref heap(new global::go.archive.tar_package.Header(), out var Ꮡhdr);
        v.input = v.input[(int)(copy(blk[..], v.input))..];
        var tr = new Reader(r: new tar_test_package.bytes_ReaderжReader(bytes.NewReader(v.input)));
        var (got, err) = tr.readOldGNUSparseMap(Ꮡhdr, Ꮡblk);
        if (!slices.Equal<global::go.archive.tar_package.sparseDatas, global::go.archive.tar_package.sparseEntry>(got, v.wantMap)) {
            Ꮡt.Errorf("test %d, readOldGNUSparseMap(): got %v, want %v"u8, i, got, v.wantMap);
        }
        if (!AreEqual(err, v.wantErr)) {
            Ꮡt.Errorf("test %d, readOldGNUSparseMap() = %v, want %v"u8, i, err, v.wantErr);
        }
        if (hdr.Size != v.wantSize) {
            Ꮡt.Errorf("test %d, Header.Size = %d, want %d"u8, i, hdr.Size, v.wantSize);
        }
    }
}

[GoType("dyn")] internal partial struct TestReadGNUSparsePAXHeaders_vectors {
    internal @string inputData;
    internal map<@string, @string> inputHdrs;
    internal global::go.archive.tar_package.sparseDatas wantMap;
    internal int64 wantSize;
    internal @string wantName;
    internal error wantErr;
}

public static void TestReadGNUSparsePAXHeaders(ж<testing.T> Ꮡt) {
    @string padInput(@string s) => s + ((sstring)(zeroBlock[..(int)(blockPadding((int64)len(s)))]));
    var vectors = new TestReadGNUSparsePAXHeaders_vectors[]{new(
        inputHdrs: default!,
        wantErr: default!
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseNumBlocks] = strconv.FormatInt(math.MaxInt64, 10),
            [paxGNUSparseMap] = "0,1,2,3"u8
        },
        wantErr: ErrHeader
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseNumBlocks] = "4\x00"u8,
            [paxGNUSparseMap] = "0,1,2,3"u8
        },
        wantErr: ErrHeader
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseNumBlocks] = "4"u8,
            [paxGNUSparseMap] = "0,1,2,3"u8
        },
        wantErr: ErrHeader
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseNumBlocks] = "2"u8,
            [paxGNUSparseMap] = "0,1,2,3"u8
        },
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 1), new(2, 3)}.slice())
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseNumBlocks] = "2"u8,
            [paxGNUSparseMap] = "0, 1,2,3"u8
        },
        wantErr: ErrHeader
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseNumBlocks] = "2"u8,
            [paxGNUSparseMap] = "0,1,02,3"u8,
            [paxGNUSparseRealSize] = "4321"u8
        },
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 1), new(2, 3)}.slice()),
        wantSize: 4321
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseNumBlocks] = "2"u8,
            [paxGNUSparseMap] = "0,one1,2,3"u8
        },
        wantErr: ErrHeader
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseMajor] = "0"u8,
            [paxGNUSparseMinor] = "0"u8,
            [paxGNUSparseNumBlocks] = "2"u8,
            [paxGNUSparseMap] = "0,1,2,3"u8,
            [paxGNUSparseSize] = "1234"u8,
            [paxGNUSparseRealSize] = "4321"u8,
            [paxGNUSparseName] = "realname"u8
        },
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 1), new(2, 3)}.slice()),
        wantSize: 1234,
        wantName: "realname"u8
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseMajor] = "0"u8,
            [paxGNUSparseMinor] = "0"u8,
            [paxGNUSparseNumBlocks] = "1"u8,
            [paxGNUSparseMap] = "10737418240,512"u8,
            [paxGNUSparseSize] = "10737418240"u8,
            [paxGNUSparseName] = "realname"u8
        },
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(10737418240L, 512)}.slice()),
        wantSize: 10737418240L,
        wantName: "realname"u8
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseMajor] = "0"u8,
            [paxGNUSparseMinor] = "0"u8,
            [paxGNUSparseNumBlocks] = "0"u8,
            [paxGNUSparseMap] = ""u8
        },
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{}.slice())
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseMajor] = "0"u8,
            [paxGNUSparseMinor] = "1"u8,
            [paxGNUSparseNumBlocks] = "4"u8,
            [paxGNUSparseMap] = "0,5,10,5,20,5,30,5"u8
        },
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 5), new(10, 5), new(20, 5), new(30, 5)}.slice())
    ), new(
        inputHdrs: new map<@string, @string>{
            [paxGNUSparseMajor] = "1"u8,
            [paxGNUSparseMinor] = "0"u8,
            [paxGNUSparseNumBlocks] = "4"u8,
            [paxGNUSparseMap] = "0,5,10,5,20,5,30,5"u8
        },
        wantErr: io.ErrUnexpectedEOF
    ), new(
        inputData: padInput("0\n"u8),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1"u8, [paxGNUSparseMinor] = "0"u8},
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{}.slice())
    ), new(
        inputData: padInput("0\n"u8)[..(int)(blockSize - 1)] + "#",
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1"u8, [paxGNUSparseMinor] = "0"u8},
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{}.slice())
    ), new(
        inputData: padInput("0"u8),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1"u8, [paxGNUSparseMinor] = "0"u8},
        wantErr: io.ErrUnexpectedEOF
    ), new(
        inputData: padInput("ab\n"u8),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1"u8, [paxGNUSparseMinor] = "0"u8},
        wantErr: ErrHeader
    ), new(
        inputData: padInput("1\n2\n3\n"u8),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1"u8, [paxGNUSparseMinor] = "0"u8},
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice())
    ), new(
        inputData: padInput("1\n2\n"u8),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1"u8, [paxGNUSparseMinor] = "0"u8},
        wantErr: io.ErrUnexpectedEOF
    ), new(
        inputData: padInput("1\n2\n\n"u8),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1"u8, [paxGNUSparseMinor] = "0"u8},
        wantErr: ErrHeader
    ), new(
        inputData: ((@string)(zeroBlock[..])) + padInput("0\n"u8),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1"u8, [paxGNUSparseMinor] = "0"u8},
        wantErr: ErrHeader
    ), new(
        inputData: strings.Repeat("0"u8, blockSize) + padInput("1\n5\n1\n"u8),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1"u8, [paxGNUSparseMinor] = "0"u8},
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(5, 1)}.slice())
    ), new(
        inputData: padInput(fmt.Sprintf("%d\n"u8, (int64)math.MaxInt64)),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1"u8, [paxGNUSparseMinor] = "0"u8},
        wantErr: ErrHeader
    ), new(
        inputData: padInput(strings.Repeat("0"u8, 300) + "1\n"u8 + strings.Repeat("0"u8, 1000) + "5\n"u8 + strings.Repeat("0"u8, 800) + "2\n"u8),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1"u8, [paxGNUSparseMinor] = "0"u8},
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(5, 2)}.slice())
    ), new(
        inputData: padInput("2\n10737418240\n512\n21474836480\n512\n"u8),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1"u8, [paxGNUSparseMinor] = "0"u8},
        wantMap: new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(10737418240L, 512), new(21474836480L, 512)}.slice())
    ), new(
        inputData: padInput("100\n"u8 + ((Func<@string>)(() => {
            slice<@string> ss = default!;
            for (nint i = 0; i < 100; i++) {
                ss = append(ss, fmt.Sprintf("%d\n%d\n"u8, ((int64)i << (int)(30)), (nint)(512)));
            }
            return strings.Join(ss, ""u8);
        }))()),
        inputHdrs: new map<@string, @string>{[paxGNUSparseMajor] = "1"u8, [paxGNUSparseMinor] = "0"u8},
        wantMap: ((Func<global::go.archive.tar_package.sparseDatas>)(() => {
            global::go.archive.tar_package.sparseDatas spd = default!;
            for (nint i = 0; i < 100; i++) {
                spd = append(spd, new sparseEntry(((int64)i << (int)(30)), 512));
            }
            return spd;
        }))()
    )
    }.slice();
    foreach (var (i, v) in vectors) {
        ref var hdr = ref heap(new global::go.archive.tar_package.Header(), out var Ꮡhdr);
        hdr.PAXRecords = v.inputHdrs;
        var r = strings.NewReader(v.inputData + "#"u8); // Add canary byte
        var tr = new Reader(curr: new global::go.archive.tar_package.regFileReaderжfileReader(Ꮡ(new regFileReader(new tar_test_package.strings_ReaderжReader(r), (int64)r.Len()))));
        var (got, err) = tr.readGNUSparsePAXHeaders(Ꮡhdr);
        if (!slices.Equal<global::go.archive.tar_package.sparseDatas, global::go.archive.tar_package.sparseEntry>(got, v.wantMap)) {
            Ꮡt.Errorf("test %d, readGNUSparsePAXHeaders(): got %v, want %v"u8, i, got, v.wantMap);
        }
        if (!AreEqual(err, v.wantErr)) {
            Ꮡt.Errorf("test %d, readGNUSparsePAXHeaders() = %v, want %v"u8, i, err, v.wantErr);
        }
        if (hdr.Size != v.wantSize) {
            Ꮡt.Errorf("test %d, Header.Size = %d, want %d"u8, i, hdr.Size, v.wantSize);
        }
        if (hdr.Name != v.wantName) {
            Ꮡt.Errorf("test %d, Header.Name = %s, want %s"u8, i, hdr.Name, v.wantName);
        }
        if (v.wantErr == default! && r.Len() == 0) {
            Ꮡt.Errorf("test %d, canary byte unexpectedly consumed"u8, i);
        }
    }
}

// testNonEmptyReader wraps an io.Reader and ensures that
// Read is never called with an empty buffer.
[GoType] internal partial struct testNonEmptyReader {
    [GoEmbedded] public io_package.Reader Reader;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedEmptyReadCallˢ = "unexpected empty Read call"u8;

internal static (nint, error) Read(this testNonEmptyReader r, slice<byte> b) {
    if (len(b) == 0) {
        return (0, errors.New(unexpectedEmptyReadCallˢ));
    }
    return r.Reader.Read(b);
}

[GoType("dyn")] [GoLocalName("testRead")] internal partial struct TestFileReader_testRead {
// Read(cnt) == (wantStr, wantErr)
    internal nint cnt;
    internal @string wantStr;
    internal error wantErr;
}

[GoType("dyn")] [GoLocalName("testWriteTo")] internal partial struct TestFileReader_testWriteTo {
// WriteTo(testFile{ops}) == (wantCnt, wantErr)
    internal fileOps ops;
    internal int64 wantCnt;
    internal error wantErr;
}

[GoType("dyn")] [GoLocalName("testRemaining")] internal partial struct TestFileReader_testRemaining {
// logicalRemaining() == wantLCnt, physicalRemaining() == wantPCnt
    internal int64 wantLCnt;
    internal int64 wantPCnt;
}

[GoType("dyn")] [GoLocalName("makeReg")] internal partial struct TestFileReader_makeReg {
    internal @string str;
    internal int64 size;
}

[GoType("dyn")] [GoLocalName("makeSparse")] internal partial struct TestFileReader_makeSparse {
    internal TestFileReader_makeReg makeReg;
    internal global::go.archive.tar_package.sparseDatas spd;
    internal int64 size;
}

[GoType("dyn")] internal partial struct TestFileReader_vectors {
    internal TestFileReader_fileMaker maker;
    internal slice<TestFileReader_testFnc> tests;
}

public static void TestFileReader(ж<testing.T> Ꮡt) {
    var vectors = new TestFileReader_vectors[]{new(
        maker: new TestFileReader_makeReg(""u8, 0),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRemaining(0, 0),
            new TestFileReader_testRead(0, ""u8, io.EOF),
            new TestFileReader_testRead(1, ""u8, io.EOF),
            new TestFileReader_testWriteTo(default!, 0, default!),
            new TestFileReader_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileReader_makeReg(""u8, 1),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRemaining(1, 1),
            new TestFileReader_testRead(5, ""u8, io.ErrUnexpectedEOF),
            new TestFileReader_testWriteTo(default!, 0, io.ErrUnexpectedEOF),
            new TestFileReader_testRemaining(1, 1)
        }.slice()
    ), new(
        maker: new TestFileReader_makeReg("hello"u8, 5),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRemaining(5, 5),
            new TestFileReader_testRead(5, "hello"u8, io.EOF),
            new TestFileReader_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileReader_makeReg("hello, world"u8, 50),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRemaining(50, 50),
            new TestFileReader_testRead(7, "hello, "u8, default!),
            new TestFileReader_testRemaining(43, 43),
            new TestFileReader_testRead(5, "world"u8, default!),
            new TestFileReader_testRemaining(38, 38),
            new TestFileReader_testWriteTo(default!, 0, io.ErrUnexpectedEOF),
            new TestFileReader_testRead(1, ""u8, io.ErrUnexpectedEOF),
            new TestFileReader_testRemaining(38, 38)
        }.slice()
    ), new(
        maker: new TestFileReader_makeReg("hello, world"u8, 5),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRemaining(5, 5),
            new TestFileReader_testRead(0, ""u8, default!),
            new TestFileReader_testRead(4, "hell"u8, default!),
            new TestFileReader_testRemaining(1, 1),
            new TestFileReader_testWriteTo(new fileOps(new any[]{(@string)"o"u8}.slice()), 1, default!),
            new TestFileReader_testRemaining(0, 0),
            new TestFileReader_testWriteTo(default!, 0, default!),
            new TestFileReader_testRead(0, ""u8, io.EOF)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde"u8, 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 3)}.slice()), 8),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRemaining(8, 5),
            new TestFileReader_testRead(3, "ab\x00"u8, default!),
            new TestFileReader_testRead(10, ((@string)(new byte[]{0x00, 0x00, 0x63, 0x64, 0x65})), io.EOF),
            new TestFileReader_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde"u8, 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 3)}.slice()), 8),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRemaining(8, 5),
            new TestFileReader_testWriteTo(new fileOps(new any[]{(@string)"ab"u8, (int64)3, (@string)"cde"u8}.slice()), 8, default!),
            new TestFileReader_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde"u8, 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 3)}.slice()), 10),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRemaining(10, 5),
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x61, 0x62, 0x00, 0x00, 0x00, 0x63, 0x64, 0x65, 0x00, 0x00})), io.EOF),
            new TestFileReader_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abc"u8, 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 3)}.slice()), 10),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRemaining(10, 5),
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x61, 0x62, 0x00, 0x00, 0x00, 0x63})), io.ErrUnexpectedEOF),
            new TestFileReader_testRemaining(4, 2)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde"u8, 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 2)}.slice()), 8),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRemaining(8, 5),
            new TestFileReader_testRead(8, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x64, 0x65})), io.EOF),
            new TestFileReader_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde"u8, 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 0), new(6, 0), new(6, 2)}.slice()), 8),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRemaining(8, 5),
            new TestFileReader_testRead(8, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x64, 0x65})), io.EOF),
            new TestFileReader_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde"u8, 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 0), new(6, 0), new(6, 2)}.slice()), 8),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRemaining(8, 5),
            new TestFileReader_testWriteTo(new fileOps(new any[]{(int64)1, (@string)"abc"u8, (int64)2, (@string)"de"u8}.slice()), 8, default!),
            new TestFileReader_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde"u8, 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 2)}.slice()), 10),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x64, 0x65, 0x00, 0x00})), io.EOF)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde"u8, 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 2)}.slice()), 10),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testWriteTo(new fileOps(new any[]{(int64)1, (@string)"abc"u8, (int64)2, (@string)"de"u8, (int64)1, (@string)"\x00"u8}.slice()), 10, default!)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde"u8, 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 2), new(8, 0), new(8, 0), new(8, 0), new(8, 0)}.slice()), 10),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x64, 0x65, 0x00, 0x00})), io.EOF)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg(""u8, 0), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{}.slice()), 2),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRead(100, "\x00\x00"u8, io.EOF)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg(""u8, 8), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRead(100, "\x00"u8, io.ErrUnexpectedEOF)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("ab"u8, 2), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62})), errMissData)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("ab"u8, 8), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62})), io.ErrUnexpectedEOF)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abc"u8, 3), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00})), errMissData)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abc"u8, 8), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00})), io.ErrUnexpectedEOF)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde"u8, 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x64, 0x65})), errMissData)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde"u8, 5), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testWriteTo(new fileOps(new any[]{(int64)1, (@string)"abc"u8, (int64)2, (@string)"de"u8}.slice()), 8, errMissData)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcde"u8, 8), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x64, 0x65})), io.ErrUnexpectedEOF)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcdefghEXTRA"u8, 13), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRemaining(15, 13),
            new TestFileReader_testRead(100, ((@string)(new byte[]{0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x64, 0x65, 0x66, 0x67, 0x68, 0x00, 0x00, 0x00, 0x00})), errUnrefData),
            new TestFileReader_testWriteTo(default!, 0, errUnrefData),
            new TestFileReader_testRemaining(0, 5)
        }.slice()
    ), new(
        maker: new TestFileReader_makeSparse(new TestFileReader_makeReg("abcdefghEXTRA"u8, 13), new sparseDatas(new global::go.archive.tar_package.sparseEntry[]{new(1, 3), new(6, 5)}.slice()), 15),
        tests: new TestFileReader_testFnc[]{
            new TestFileReader_testRemaining(15, 13),
            new TestFileReader_testWriteTo(new fileOps(new any[]{(int64)1, (@string)"abc"u8, (int64)2, (@string)"defgh"u8, (int64)4}.slice()), 15, errUnrefData),
            new TestFileReader_testRead(100, ""u8, errUnrefData),
            new TestFileReader_testRemaining(0, 5)
        }.slice()
    )
    }.slice();
    foreach (var (i, v) in vectors) {
        global::go.archive.tar_package.fileReader fr = default!;
        switch (v.maker.type()) {
        case TestFileReader_makeReg maker: {
            ref var r = ref heap<testNonEmptyReader>(out var Ꮡr);
            r = new testNonEmptyReader(new tar_test_package.strings_ReaderжReader(strings.NewReader(maker.str)));
            fr = new global::go.archive.tar_package.regFileReaderжfileReader(Ꮡ(new regFileReader(r, maker.size)));
            break;
        }
        case TestFileReader_makeSparse maker: {
            if (!validateSparseEntries(maker.spd, maker.size)) {
                Ꮡt.Fatalf("invalid sparse map: %v"u8, maker.spd);
            }
            var sph = invertSparseEntries(maker.spd, maker.size);
            ref var r = ref heap<testNonEmptyReader>(out var Ꮡr);
            r = new testNonEmptyReader(new tar_test_package.strings_ReaderжReader(strings.NewReader(maker.makeReg.str)));
            fr = new global::go.archive.tar_package.regFileReaderжfileReader(Ꮡ(new regFileReader(r, maker.makeReg.size)));
            fr = new global::go.archive.tar_package.sparseFileReaderжfileReader(Ꮡ(new sparseFileReader(fr, sph, 0)));
            break;
        }
        default: {
            var maker = v.maker;
            Ꮡt.Fatalf("test %d, unknown make operation: %T"u8, i, maker);
            break;
        }}
        foreach (var (j, tf) in v.tests) {
            switch (tf.type()) {
            case TestFileReader_testRead tfΔ1: {
                var b = new slice<byte>(tfΔ1.cnt);
                var (n, err) = fr.Read(b);
                {
                    @string got = ((@string)(b[..(int)(n)])); if (got != tfΔ1.wantStr || !AreEqual(err, tfΔ1.wantErr)) {
                        Ꮡt.Errorf("test %d.%d, Read(%d):\ngot  (%q, %v)\nwant (%q, %v)"u8, i, j, tfΔ1.cnt, got, err, tfΔ1.wantStr, tfΔ1.wantErr);
                    }
                }
                break;
            }
            case TestFileReader_testWriteTo tfΔ1: {
                var f = Ꮡ(new testFile(ops: tfΔ1.ops));
                var (got, err) = fr.WriteTo(new tar_internal_test_package.testFileжWriter(f));
                {
                    var (_, ok) = err._<testError>(ᐧ); if (ok){
                        Ꮡt.Errorf("test %d.%d, WriteTo(): %v"u8, i, j, err);
                    } else 
                    if (got != tfΔ1.wantCnt || !AreEqual(err, tfΔ1.wantErr)) {
                        Ꮡt.Errorf("test %d.%d, WriteTo() = (%d, %v), want (%d, %v)"u8, i, j, got, err, tfΔ1.wantCnt, tfΔ1.wantErr);
                    }
                }
                if (len((~f).ops) > 0) {
                    Ꮡt.Errorf("test %d.%d, expected %d more operations"u8, i, j, len((~f).ops));
                }
                break;
            }
            case TestFileReader_testRemaining tfΔ1: {
                {
                    var got = fr.logicalRemaining(); if (got != tfΔ1.wantLCnt) {
                        Ꮡt.Errorf("test %d.%d, logicalRemaining() = %d, want %d"u8, i, j, got, tfΔ1.wantLCnt);
                    }
                }
                {
                    var got = fr.physicalRemaining(); if (got != tfΔ1.wantPCnt) {
                        Ꮡt.Errorf("test %d.%d, physicalRemaining() = %d, want %d"u8, i, j, got, tfΔ1.wantPCnt);
                    }
                }
                break;
            }
            default: {
                var tfΔ1 = tf;
                Ꮡt.Fatalf("test %d.%d, unknown test operation: %T"u8, i, j, tfΔ1);
                break;
            }}
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string godebugˢ = "GODEBUG"u8;
internal static readonly @string tarinsecurepath0ˢ = "tarinsecurepath=0"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string securePathᶜ = "secure"u8;

public static void TestInsecurePaths(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, tarinsecurepath0ˢ);
    foreach (var (_, vᴛ1) in new @string[]{
        "../foo"u8,
        "/foo"u8,
        "a/b/../../../c"u8
    }.slice()) {
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
            Ꮡt.Errorf("tr.Next for file %q: got err %v, want ErrInsecurePath"u8, path, err);
            continue;
        }
        if ((~h).Name != path) {
            Ꮡt.Errorf("tr.Next for file %q: got name %q, want %q"u8, path, (~h).Name, path);
        }
        // Error should not be sticky.
        (h, err) = tr.Next();
        if (err != default!) {
            Ꮡt.Errorf("tr.Next for file %q: got err %v, want nil"u8, securePath, err);
        }
        if ((~h).Name != securePath) {
            Ꮡt.Errorf("tr.Next for file %q: got name %q, want %q"u8, securePath, (~h).Name, securePath);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tarinsecurepath1ˢ = "tarinsecurepath=1"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string nameᶜ = "/foo"u8;

public static void TestDisableInsecurePathCheck(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, tarinsecurepath1ˢ);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    @string name = nameᶜ;
    tw.WriteHeader(Ꮡ(new Header(
        Name: name
    )));
    tw.Close();
    var tr = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuf));
    var (h, err) = tr.Next();
    if (err != default!) {
        Ꮡt.Fatalf("tr.Next with tarinsecurepath=1: got err %v, want nil"u8, err);
    }
    if ((~h).Name != name) {
        Ꮡt.Fatalf("tr.Next with tarinsecurepath=1: got name %q, want %q"u8, (~h).Name, name);
    }
}

} // end tar_internal_test_package
