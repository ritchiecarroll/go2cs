// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using TestWriter_testFnc = object;
global using TestFileWriter_testFnc = object;
global using TestFileWriter_fileMaker = object;

namespace go.archive;

using bytes = bytes_package;
using hex = encoding.hex_package;
using errors = errors_package;
using obscuretestdata = @internal.obscuretestdata_package;
using io = io_package;
using fs = go.io.fs_package;
using maps = maps_package;
using os = os_package;
using path = path_package;
using slices = slices_package;
using sort = sort_package;
using strings = strings_package;
using testing = testing_package;
using fstest = go.testing.fstest_package;
using iotest = go.testing.iotest_package;
using time = time_package;
using @internal;
using encoding;
using go.io;
using go.testing;
using static go.archive.tar_package;

partial class tar_internal_test_package {

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string uniqueAᶜ => ref uniqueAᶜᵇ;
private static readonly @string uniqueAᶜᵇ = "-  "u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string uniqueBᶜ => ref uniqueBᶜᵇ;
private static readonly @string uniqueBᶜᵇ = "+  "u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string identityᶜ => ref identityᶜᵇ;
private static readonly @string identityᶜᵇ = "   "u8;

internal static @string bytediff(slice<byte> a, slice<byte> b) {
    var ssᴷ = new stackAppend<@string>();

    @string uniqueA = uniqueAᶜ;
    @string uniqueB = uniqueBᶜ;
    @string identity = identityᶜ;
    slice<@string> ss = default;
    var sa = strings.Split(strings.TrimSpace(hex.Dump(a)), "\n");
    var sb = strings.Split(strings.TrimSpace(hex.Dump(b)), "\n");
    while (len(sa) > 0 && len(sb) > 0) {
        if (sa[0] == sb[0]) {
            ss = append(ssᴷ, ss, identity + sa[0]);
        } else {
            ss = append(ss, uniqueA + sa[0]);
            ss = append(ss, uniqueB + sb[0]);
        }
        (sa, sb) = (sa[1..], sb[1..]);
    }
    while (len(sa) > 0) {
        ss = append(ss, uniqueA + sa[0]);
        sa = sa[1..];
    }
    while (len(sb) > 0) {
        ss = append(ss, uniqueB + sb[0]);
        sb = sb[1..];
    }
    return strings.Join(ss, "\n");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string longnameˢ => ref longnameˢᵇ;
private static readonly @string longnameˢᵇ = "longname/"u8;

internal partial struct TestWriter_testHeader /*dyn*/ {
// WriteHeader(hdr) == wantErr
    internal global::go.archive.tar_package.Header hdr;
    internal error wantErr;
}

internal partial struct TestWriter_testWrite /*dyn*/ {
// Write(str) == (wantCnt, wantErr)
    internal @string str;
    internal nint wantCnt;
    internal error wantErr;
}

internal partial struct TestWriter_testReadFrom /*dyn*/ {
// ReadFrom(testFile{ops}) == (wantCnt, wantErr)
    internal fileOps ops;
    internal int64 wantCnt;
    internal error wantErr;
}

internal partial struct TestWriter_testClose /*dyn*/ {
// Close() == wantErr
    internal error wantErr;
}

internal partial struct TestWriter_vectors /*dyn*/ {
    internal @string @file; // Optional filename of expected output
    internal bool obscured; // Whether file is obscured
    internal slice<TestWriter_testFnc> tests;
}

public static void TestWriter(ж<testing.T> t) {
    var vectors = slice<TestWriter_vectors>([new(
        // The writer test file was produced with this command:
        // tar (GNU tar) 1.26
        //   ln -s small.txt link.txt
        //   tar -b 1 --format=ustar -c -f writer.tar small.txt small2.txt link.txt
        @file: "testdata/writer.tar",
        tests: slice<TestWriter_testFnc>([
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "small.txt",
                Size: 5,
                Mode: 416,
                Uid: 73025,
                Gid: 5000,
                Uname: "dsymonds",
                Gname: "eng",
                ModTime: time.Unix(1246508266, 0)
            ), default),
            new TestWriter_testWrite("Kilts", 5, default),

            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "small2.txt",
                Size: 11,
                Mode: 416,
                Uid: 73025,
                Uname: "dsymonds",
                Gname: "eng",
                Gid: 5000,
                ModTime: time.Unix(1245217492, 0)
            ), default),
            new TestWriter_testWrite("Google.com\n", 11, default),

            new TestWriter_testHeader(new Header(
                Typeflag: TypeSymlink,
                Name: "link.txt",
                Linkname: "small.txt",
                Mode: 511,
                Uid: 1000,
                Gid: 1000,
                Uname: "strings",
                Gname: "strings",
                ModTime: time.Unix(1314603082, 0)
            ), default),
            new TestWriter_testWrite("", 0, default),

            new TestWriter_testClose((error)(default))
        ])
    ), new(
        // The truncated test file was produced using these commands:
        //   dd if=/dev/zero bs=1048576 count=16384 > /tmp/16gig.txt
        //   tar -b 1 -c -f- /tmp/16gig.txt | dd bs=512 count=8 > writer-big.tar
        @file: "testdata/writer-big.tar",
        tests: slice<TestWriter_testFnc>([
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "tmp/16gig.txt",
                Size: 17179869184L,
                Mode: 416,
                Uid: 73025,
                Gid: 5000,
                Uname: "dsymonds",
                Gname: "eng",
                ModTime: time.Unix(1254699560, 0),
                Format: FormatGNU
            ), default)
        ])
    ), new(
        // This truncated file was produced using this library.
        // It was verified to work with GNU tar 1.27.1 and BSD tar 3.1.2.
        //  dd if=/dev/zero bs=1G count=16 >> writer-big-long.tar
        //  gnutar -xvf writer-big-long.tar
        //  bsdtar -xvf writer-big-long.tar
        //
        // This file is in PAX format.
        @file: "testdata/writer-big-long.tar.base64",
        obscured: true,
        tests: slice<TestWriter_testFnc>([
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: strings.Repeat(longnameˢ, 15) + "16gig.txt"u8,
                Size: 17179869184L,
                Mode: 420,
                Uid: 1000,
                Gid: 1000,
                Uname: "guillaume",
                Gname: "guillaume",
                ModTime: time.Unix(1399583047, 0)
            ), default)
        ])
    ), new(
        // This file was produced using GNU tar v1.17.
        //	gnutar -b 4 --format=ustar (longname/)*15 + file.txt
        @file: "testdata/ustar.tar",
        tests: slice<TestWriter_testFnc>([
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: strings.Repeat(longnameˢ, 15) + "file.txt"u8,
                Size: 6,
                Mode: 420,
                Uid: 501,
                Gid: 20,
                Uname: "shane",
                Gname: "staff",
                ModTime: time.Unix(1360135598, 0)
            ), default),
            new TestWriter_testWrite("hello\n", 6, default),
            new TestWriter_testClose((error)(default))
        ])
    ), new(
        // This file was produced using GNU tar v1.26:
        //	echo "Slartibartfast" > file.txt
        //	ln file.txt hard.txt
        //	tar -b 1 --format=ustar -c -f hardlink.tar file.txt hard.txt
        @file: "testdata/hardlink.tar",
        tests: slice<TestWriter_testFnc>([
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "file.txt",
                Size: 15,
                Mode: 420,
                Uid: 1000,
                Gid: 100,
                Uname: "vbatts",
                Gname: "users",
                ModTime: time.Unix(1425484303, 0)
            ), default),
            new TestWriter_testWrite("Slartibartfast\n", 15, default),

            new TestWriter_testHeader(new Header(
                Typeflag: TypeLink,
                Name: "hard.txt",
                Linkname: "file.txt",
                Mode: 420,
                Uid: 1000,
                Gid: 100,
                Uname: "vbatts",
                Gname: "users",
                ModTime: time.Unix(1425484303, 0)
            ), default),
            new TestWriter_testWrite("", 0, default),

            new TestWriter_testClose((error)(default))
        ])
    ), new(
        tests: [
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "bad-null.txt",
                Xattrs: new map<@string, @string>{["null\x00null\x00"] = "fizzbuzz"}
            ), new headerError(new @string[]{}.slice()))
        ]
    ), new(
        tests: [
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "null\x00.txt"
            ), new headerError(new @string[]{}.slice()))
        ]
    ), new(
        @file: "testdata/pax-records.tar",
        tests: slice<TestWriter_testFnc>([
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "file",
                Uname: strings.Repeat(longˢ, 10),
                PAXRecords: new map<@string, @string>{
                    ["path"] = "FILE",          // Should be ignored
                    ["GNU.sparse.map"] = "0,0", // Should be ignored
                    ["comment"] = "Hello, 世界",
                    ["GOLANG.pkg"] = "tar"
                }
            ), default),
            new TestWriter_testClose((error)(default))
        ])
    ), new(
        // Craft a theoretically valid PAX archive with global headers.
        // The GNU and BSD tar tools do not parse these the same way.
        //
        // BSD tar v3.1.2 parses and ignores all global headers;
        // the behavior is verified by researching the source code.
        //
        //	$ bsdtar -tvf pax-global-records.tar
        //	----------  0 0      0           0 Dec 31  1969 file1
        //	----------  0 0      0           0 Dec 31  1969 file2
        //	----------  0 0      0           0 Dec 31  1969 file3
        //	----------  0 0      0           0 May 13  2014 file4
        //
        // GNU tar v1.27.1 applies global headers to subsequent records,
        // but does not do the following properly:
        //	* It does not treat an empty record as deletion.
        //	* It does not use subsequent global headers to update previous ones.
        //
        //	$ gnutar -tvf pax-global-records.tar
        //	---------- 0/0               0 2017-07-13 19:40 global1
        //	---------- 0/0               0 2017-07-13 19:40 file2
        //	gnutar: Substituting `.' for empty member name
        //	---------- 0/0               0 1969-12-31 16:00
        //	gnutar: Substituting `.' for empty member name
        //	---------- 0/0               0 2014-05-13 09:53
        //
        // According to the PAX specification, this should have been the result:
        //	---------- 0/0               0 2017-07-13 19:40 global1
        //	---------- 0/0               0 2017-07-13 19:40 file2
        //	---------- 0/0               0 2017-07-13 19:40 file3
        //	---------- 0/0               0 2014-05-13 09:53 file4
        @file: "testdata/pax-global-records.tar",
        tests: slice<TestWriter_testFnc>([
            new TestWriter_testHeader(new Header(
                Typeflag: TypeXGlobalHeader,
                PAXRecords: new map<@string, @string>{["path"] = "global1", ["mtime"] = "1500000000.0"}
            ), default),
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg, Name: "file1"
            ), default),
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "file2",
                PAXRecords: new map<@string, @string>{["path"] = "file2"}
            ), default),
            new TestWriter_testHeader(new Header(
                Typeflag: TypeXGlobalHeader,
                PAXRecords: new map<@string, @string>{["path"] = ""} // Should delete "path", but keep "mtime"
            ), default),
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg, Name: "file3"
            ), default),
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "file4",
                ModTime: time.Unix(1400000000, 0),
                PAXRecords: new map<@string, @string>{["mtime"] = "1400000000"}
            ), default),
            new TestWriter_testClose((error)(default))
        ])
    ), new(
        @file: "testdata/gnu-utf8.tar",
        tests: slice<TestWriter_testFnc>([
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹",
                Mode: 420,
                Uid: 1000, Gid: 1000,
                Uname: "☺",
                Gname: "⚹",
                ModTime: time.Unix(0, 0),
                Format: FormatGNU
            ), default),
            new TestWriter_testClose((error)(default))
        ])
    ), new(
        @file: "testdata/gnu-not-utf8.tar",
        tests: slice<TestWriter_testFnc>([
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: ((@string)(new byte[]{0x68, 0x69, 0x80, 0x81, 0x82, 0x83, 0x62, 0x79, 0x65})),
                Mode: 420,
                Uid: 1000,
                Gid: 1000,
                Uname: "rawr",
                Gname: "dsnet",
                ModTime: time.Unix(0, 0),
                Format: FormatGNU
            ), default),
            new TestWriter_testClose((error)(default))
        ]) // TODO(dsnet): Re-enable this test when adding sparse support.
 // See https://golang.org/issue/22735
 /*
			}, {
				file: "testdata/gnu-nil-sparse-data.tar",
				tests: []testFnc{
					testHeader{Header{
						Typeflag:    TypeGNUSparse,
						Name:        "sparse.db",
						Size:        1000,
						SparseHoles: []sparseEntry{{Offset: 1000, Length: 0}},
					}, nil},
					testWrite{strings.Repeat("0123456789", 100), 1000, nil},
					testClose{},
				},
			}, {
				file: "testdata/gnu-nil-sparse-hole.tar",
				tests: []testFnc{
					testHeader{Header{
						Typeflag:    TypeGNUSparse,
						Name:        "sparse.db",
						Size:        1000,
						SparseHoles: []sparseEntry{{Offset: 0, Length: 1000}},
					}, nil},
					testWrite{strings.Repeat("\x00", 1000), 1000, nil},
					testClose{},
				},
			}, {
				file: "testdata/pax-nil-sparse-data.tar",
				tests: []testFnc{
					testHeader{Header{
						Typeflag:    TypeReg,
						Name:        "sparse.db",
						Size:        1000,
						SparseHoles: []sparseEntry{{Offset: 1000, Length: 0}},
					}, nil},
					testWrite{strings.Repeat("0123456789", 100), 1000, nil},
					testClose{},
				},
			}, {
				file: "testdata/pax-nil-sparse-hole.tar",
				tests: []testFnc{
					testHeader{Header{
						Typeflag:    TypeReg,
						Name:        "sparse.db",
						Size:        1000,
						SparseHoles: []sparseEntry{{Offset: 0, Length: 1000}},
					}, nil},
					testWrite{strings.Repeat("\x00", 1000), 1000, nil},
					testClose{},
				},
			}, {
				file:     "testdata/gnu-sparse-big.tar.base64",
				obscured: true,
				tests: []testFnc{
					testHeader{Header{
						Typeflag: TypeGNUSparse,
						Name:     "gnu-sparse",
						Size:     6e10,
						SparseHoles: []sparseEntry{
							{Offset: 0e10, Length: 1e10 - 100},
							{Offset: 1e10, Length: 1e10 - 100},
							{Offset: 2e10, Length: 1e10 - 100},
							{Offset: 3e10, Length: 1e10 - 100},
							{Offset: 4e10, Length: 1e10 - 100},
							{Offset: 5e10, Length: 1e10 - 100},
						},
					}, nil},
					testReadFrom{fileOps{
						int64(1e10 - blockSize),
						strings.Repeat("\x00", blockSize-100) + strings.Repeat("0123456789", 10),
						int64(1e10 - blockSize),
						strings.Repeat("\x00", blockSize-100) + strings.Repeat("0123456789", 10),
						int64(1e10 - blockSize),
						strings.Repeat("\x00", blockSize-100) + strings.Repeat("0123456789", 10),
						int64(1e10 - blockSize),
						strings.Repeat("\x00", blockSize-100) + strings.Repeat("0123456789", 10),
						int64(1e10 - blockSize),
						strings.Repeat("\x00", blockSize-100) + strings.Repeat("0123456789", 10),
						int64(1e10 - blockSize),
						strings.Repeat("\x00", blockSize-100) + strings.Repeat("0123456789", 10),
					}, 6e10, nil},
					testClose{nil},
				},
			}, {
				file:     "testdata/pax-sparse-big.tar.base64",
				obscured: true,
				tests: []testFnc{
					testHeader{Header{
						Typeflag: TypeReg,
						Name:     "pax-sparse",
						Size:     6e10,
						SparseHoles: []sparseEntry{
							{Offset: 0e10, Length: 1e10 - 100},
							{Offset: 1e10, Length: 1e10 - 100},
							{Offset: 2e10, Length: 1e10 - 100},
							{Offset: 3e10, Length: 1e10 - 100},
							{Offset: 4e10, Length: 1e10 - 100},
							{Offset: 5e10, Length: 1e10 - 100},
						},
					}, nil},
					testReadFrom{fileOps{
						int64(1e10 - blockSize),
						strings.Repeat("\x00", blockSize-100) + strings.Repeat("0123456789", 10),
						int64(1e10 - blockSize),
						strings.Repeat("\x00", blockSize-100) + strings.Repeat("0123456789", 10),
						int64(1e10 - blockSize),
						strings.Repeat("\x00", blockSize-100) + strings.Repeat("0123456789", 10),
						int64(1e10 - blockSize),
						strings.Repeat("\x00", blockSize-100) + strings.Repeat("0123456789", 10),
						int64(1e10 - blockSize),
						strings.Repeat("\x00", blockSize-100) + strings.Repeat("0123456789", 10),
						int64(1e10 - blockSize),
						strings.Repeat("\x00", blockSize-100) + strings.Repeat("0123456789", 10),
					}, 6e10, nil},
					testClose{nil},
				},
		*/

    ), new(
        @file: "testdata/trailing-slash.tar",
        tests: slice<TestWriter_testFnc>([
            new TestWriter_testHeader(new Header(Name: strings.Repeat("123456789/", 30)), default),
            new TestWriter_testClose((error)(default))
        ])
    ), new(
        // Automatically promote zero value of Typeflag depending on the name.
        @file: "testdata/file-and-dir.tar",
        tests: [
            new TestWriter_testHeader(new Header(Name: "small.txt", Size: 5), default),
            new TestWriter_testWrite("Kilts", 5, default),
            new TestWriter_testHeader(new Header(Name: "dir/"), default),
            new TestWriter_testClose((error)(default))
        ]
    )
    ]);

    bool equalError(error x, error y) {
        var (_, ok1) = x._<headerError>(ᐧ);
        var (_, ok2) = y._<headerError>(ᐧ);
        if (ok1 || ok2) {
            return ok1 && ok2;
        }
        return AreEqual(x, y);
    }
    foreach (var (_, vᴛ1) in range(vectors)) {
        ref var v = ref heap(new TestWriter_vectors(), out var Ꮡv);
        v = vᴛ1;

        var equalErrorʗ1 = equalError;
        var vʗ1 = v;
        t.Run(strings.TrimSuffix(path.Base(v.@file), base64ˢ), (ж<testing.T> tΔ1) => {
            const int64 maxSize = /* 10 << 10 */ 10240; // 10KiB
            var buf = @new<bytes.Buffer>();
            var tw = NewWriter(iotest.TruncateWriter(new tar_test_package.bytes_BufferжWriter(buf), maxSize));

            foreach (var (i, tf) in range(vʗ1.tests)) {
                switch (tf.type()) {
                case TestWriter_testHeader tfΔ1ᴛ1: {
                    ref var tfΔ1 = ref heap(tfΔ1ᴛ1, out var ᏑtfΔ1);
                    var err = tw.WriteHeader(ᏑtfΔ1.Ꮡhdr);
                    if (!equalErrorʗ1(err, tfΔ1.wantErr)) {
                        tΔ1.Fatalf("test %d, WriteHeader() = %v, want %v", i, err, tfΔ1.wantErr);
                    }
                    break;
                }
                case TestWriter_testWrite tfΔ1: {
                    var (got, err) = tw.Write(slice<byte>(tfΔ1.str));
                    if (got != tfΔ1.wantCnt || !equalErrorʗ1(err, tfΔ1.wantErr)) {
                        tΔ1.Fatalf("test %d, Write() = (%d, %v), want (%d, %v)", i, got, err, tfΔ1.wantCnt, tfΔ1.wantErr);
                    }
                    break;
                }
                case TestWriter_testReadFrom tfΔ1: {
                    var f = Ꮡ(new testFile(ops: tfΔ1.ops));
                    var (got, err) = tw.readFrom(new tar_internal_test_package.testFileжReader(f));
                    var (_, ok) = err._<testError>(ᐧ); if (ok) {
                        tΔ1.Errorf("test %d, ReadFrom(): %v", i, err);
                    } else if (got != tfΔ1.wantCnt || !equalErrorʗ1(err, tfΔ1.wantErr)) {
                        tΔ1.Errorf("test %d, ReadFrom() = (%d, %v), want (%d, %v)", i, got, err, tfΔ1.wantCnt, tfΔ1.wantErr);
                    }
                    if (len(f.ops) > 0) {
                        tΔ1.Errorf("test %d, expected %d more operations", i, len(f.ops));
                    }
                    break;
                }
                case TestWriter_testClose tfΔ1: {
                    var err = tw.Close();
                    if (!equalErrorʗ1(err, tfΔ1.wantErr)) {
                        tΔ1.Fatalf("test %d, Close() = %v, want %v", i, err, tfΔ1.wantErr);
                    }
                    break;
                }
                default: {
                    var tfΔ1 = tf;
                    tΔ1.Fatalf("test %d, unknown test operation: %T", i, tfΔ1);
                    break;
                }}
            }

            if (vʗ1.@file != ""u8) {
                @string pathΔ1 = vʗ1.@file;
                if (vʗ1.obscured) {
                    var (tf, errΔ1) = obscuretestdata.DecodeToTempFile(pathΔ1);
                    if (errΔ1 != nil) {
                        tΔ1.Fatalf("obscuretestdata.DecodeToTempFile(%s): %v", pathΔ1, errΔ1);
                    }
                    pathΔ1 = tf;
                }

                var (want, err) = os.ReadFile(pathΔ1);
                if (err != nil) {
                    tΔ1.Fatalf("ReadFile() = %v, want nil", err);
                }
                var got = buf.Bytes();
                if (!bytes.Equal(want, got)) {
                    tΔ1.Fatalf("incorrect result: (-got +want)\n%v", bytediff(got, want));
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedAtLeastOnePaxˢ = (@string)"Expected at least one PAX header to be written."u8;
internal static readonly object couldnTRecoverLongFileˢ = (@string)"Couldn't recover long file name"u8;

public static void TestPax(ж<testing.T> t) {
    // Create an archive with a large name
    var (fileinfo, err) = os.Stat(testdataSmallTxtˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    (var hdr, err) = FileInfoHeader(fileinfo, ""u8);
    if (err != nil) {
        t.Fatalf("os.Stat: %v", err);
    }
    // Force a PAX long name to be written
    @string longName = strings.Repeat("ab", 100);
    @string contents = strings.Repeat(" ", (nint)hdr.Size);
    hdr.Name = longName;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var writer = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    var errΔ1 = writer.WriteHeader(hdr); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    (_, err) = writer.Write(slice<byte>(contents)); if (err != nil) {
        t.Fatal(err);
    }
    var errΔ2 = writer.Close(); if (errΔ2 != nil) {
        t.Fatal(errΔ2);
    }
    // Simple test to make sure PAX extensions are in effect
    if (!bytes.Contains(buf.Bytes(), slice<byte>("PaxHeaders.0"u8))) {
        t.Fatal(expectedAtLeastOnePaxˢ);
    }
    // Test that we can get a long name back out of the archive.
    var reader = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuf));
    (hdr, err) = reader.Next();
    if (err != nil) {
        t.Fatal(err);
    }
    if (hdr.Name != longName) {
        t.Fatal(couldnTRecoverLongFileˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object couldnTRecoverLongLinkˢ = (@string)"Couldn't recover long link name"u8;

public static void TestPaxSymlink(ж<testing.T> t) {
    // Create an archive with a large linkname
    var (fileinfo, err) = os.Stat(testdataSmallTxtˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    (var hdr, err) = FileInfoHeader(fileinfo, ""u8);
    if (err != nil) {
        t.Fatalf("os.Stat:1 %v", err);
    }
    hdr.Typeflag = TypeSymlink;
    // Force a PAX long linkname to be written
    @string longLinkname = strings.Repeat("1234567890/1234567890", 10);
    hdr.Linkname = longLinkname;

    hdr.Size = 0;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var writer = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    var errΔ1 = writer.WriteHeader(hdr); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    var errΔ2 = writer.Close(); if (errΔ2 != nil) {
        t.Fatal(errΔ2);
    }
    // Simple test to make sure PAX extensions are in effect
    if (!bytes.Contains(buf.Bytes(), slice<byte>("PaxHeaders.0"u8))) {
        t.Fatal(expectedAtLeastOnePaxˢ);
    }
    // Test that we can get a long name back out of the archive.
    var reader = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuf));
    (hdr, err) = reader.Next();
    if (err != nil) {
        t.Fatal(err);
    }
    if (hdr.Linkname != longLinkname) {
        t.Fatal(couldnTRecoverLongLinkˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object couldnTRecoverUnicodeˢ = (@string)"Couldn't recover unicode name"u8;
internal static readonly object couldnTRecoverUnicodeˢ2 = (@string)"Couldn't recover unicode group"u8;
internal static readonly object couldnTRecoverUnicodeˢ3 = (@string)"Couldn't recover unicode user"u8;

public static void TestPaxNonAscii(ж<testing.T> t) {
    // Create an archive with non ascii. These should trigger a pax header
    // because pax headers have a defined utf-8 encoding.
    var (fileinfo, err) = os.Stat(testdataSmallTxtˢ);
    if (err != nil) {
        t.Fatal(err);
    }

    (var hdr, err) = FileInfoHeader(fileinfo, ""u8);
    if (err != nil) {
        t.Fatalf("os.Stat:1 %v", err);
    }

    // some sample data
    @string chineseFilename = "文件名";
    @string chineseGroupname = "組";
    @string chineseUsername = "用戶名";

    hdr.Name = chineseFilename;
    hdr.Gname = chineseGroupname;
    hdr.Uname = chineseUsername;

    @string contents = strings.Repeat(" ", (nint)hdr.Size);

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var writer = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    var errΔ1 = writer.WriteHeader(hdr); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    (_, err) = writer.Write(slice<byte>(contents)); if (err != nil) {
        t.Fatal(err);
    }
    var errΔ2 = writer.Close(); if (errΔ2 != nil) {
        t.Fatal(errΔ2);
    }
    // Simple test to make sure PAX extensions are in effect
    if (!bytes.Contains(buf.Bytes(), slice<byte>("PaxHeaders.0"u8))) {
        t.Fatal(expectedAtLeastOnePaxˢ);
    }
    // Test that we can get a long name back out of the archive.
    var reader = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuf));
    (hdr, err) = reader.Next();
    if (err != nil) {
        t.Fatal(err);
    }
    if (hdr.Name != chineseFilename) {
        t.Fatal(couldnTRecoverUnicodeˢ);
    }
    if (hdr.Gname != chineseGroupname) {
        t.Fatal(couldnTRecoverUnicodeˢ2);
    }
    if (hdr.Uname != chineseUsername) {
        t.Fatal(couldnTRecoverUnicodeˢ3);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string kiltsˢ => ref kiltsˢᵇ;
private static readonly @string kiltsˢᵇ = "Kilts"u8;

public static void TestPaxXattrs(ж<testing.T> t) {
    var xattrs = new map<@string, @string>{
        ["user.key"] = "value"
    };

    // Create an archive with an xattr
    var (fileinfo, err) = os.Stat(testdataSmallTxtˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    (var hdr, err) = FileInfoHeader(fileinfo, ""u8);
    if (err != nil) {
        t.Fatalf("os.Stat: %v", err);
    }
    @string contents = kiltsˢ;
    hdr.Xattrs = xattrs;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var writer = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    var errΔ1 = writer.WriteHeader(hdr); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    (_, err) = writer.Write(slice<byte>(contents)); if (err != nil) {
        t.Fatal(err);
    }
    var errΔ2 = writer.Close(); if (errΔ2 != nil) {
        t.Fatal(errΔ2);
    }
    // Test that we can get the xattrs back out of the archive.
    var reader = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuf));
    (hdr, err) = reader.Next();
    if (err != nil) {
        t.Fatal(err);
    }
    if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(hdr.Xattrs, xattrs)) {
        t.Fatalf("xattrs did not survive round trip: got %+v, want %+v",
            hdr.Xattrs, xattrs);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object paxHeadersAreNotSortedˢ = (@string)"PAX headers are not sorted"u8;

public static void TestPaxHeadersSorted(ж<testing.T> t) {
    var (fileinfo, err) = os.Stat(testdataSmallTxtˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    (var hdr, err) = FileInfoHeader(fileinfo, ""u8);
    if (err != nil) {
        t.Fatalf("os.Stat: %v", err);
    }
    @string contents = strings.Repeat(" ", (nint)hdr.Size);

    hdr.Xattrs = new map<@string, @string>{
        ["foo"] = "foo",
        ["bar"] = "bar",
        ["baz"] = "baz",
        ["qux"] = "qux"
    };

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var writer = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    var errΔ1 = writer.WriteHeader(hdr); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    (_, err) = writer.Write(slice<byte>(contents)); if (err != nil) {
        t.Fatal(err);
    }
    var errΔ2 = writer.Close(); if (errΔ2 != nil) {
        t.Fatal(errΔ2);
    }
    // Simple test to make sure PAX extensions are in effect
    if (!bytes.Contains(buf.Bytes(), slice<byte>("PaxHeaders.0"u8))) {
        t.Fatal(expectedAtLeastOnePaxˢ);
    }

    // xattr bar should always appear before others
    var indices = slice<nint>([
        bytes.Index(buf.Bytes(), slice<byte>("bar=bar"u8)),
        bytes.Index(buf.Bytes(), slice<byte>("baz=baz"u8)),
        bytes.Index(buf.Bytes(), slice<byte>("foo=foo"u8)),
        bytes.Index(buf.Bytes(), slice<byte>("qux=qux"u8))
    ]);
    if (!slices.IsSorted<slice<nint>, nint>(indices)) {
        t.Fatal(paxHeadersAreNotSortedˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object couldnTRecoverLongNameˢ = (@string)"Couldn't recover long name"u8;

public static void TestUSTARLongName(ж<testing.T> t) {
    // Create an archive with a path that failed to split with USTAR extension in previous versions.
    var (fileinfo, err) = os.Stat(testdataSmallTxtˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    (var hdr, err) = FileInfoHeader(fileinfo, ""u8);
    if (err != nil) {
        t.Fatalf("os.Stat:1 %v", err);
    }
    hdr.Typeflag = TypeDir;
    // Force a PAX long name to be written. The name was taken from a practical example
    // that fails and replaced ever char through numbers to anonymize the sample.
    @string longName = "/0000_0000000/00000-000000000/0000_0000000/00000-0000000000000/0000_0000000/00000-0000000-00000000/0000_0000000/00000000/0000_0000000/000/0000_0000000/00000000v00/0000_0000000/000000/0000_0000000/0000000/0000_0000000/00000y-00/0000/0000/00000000/0x000000/";
    hdr.Name = longName;

    hdr.Size = 0;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var writer = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    var errΔ1 = writer.WriteHeader(hdr); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    var errΔ2 = writer.Close(); if (errΔ2 != nil) {
        t.Fatal(errΔ2);
    }
    // Test that we can get a long name back out of the archive.
    var reader = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuf));
    (hdr, err) = reader.Next();
    if (err != nil && !AreEqual(err, ErrInsecurePath)) {
        t.Fatal(err);
    }
    if (hdr.Name != longName) {
        t.Fatal(couldnTRecoverLongNameˢ);
    }
}

public static void TestValidTypeflagWithPAXHeader(ж<testing.T> t) {
    ref var buffer = ref heap(new bytes.Buffer(), out var Ꮡbuffer);
    var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuffer));

    ref var fileName = ref heap<@string>(strings.Repeat("ab", 100), out var ᏑfileName);

    var hdr = Ꮡ(new Header(
        Name: fileName,
        Size: 4,
        Typeflag: 0
    ));
    {
        var err = tw.WriteHeader(hdr); if (err != nil) {
            t.Fatalf("Failed to write header: %s", err);
        }
    }
    {
        var (_, err) = tw.Write(slice<byte>("fooo"u8)); if (err != nil) {
            t.Fatalf("Failed to write the file's data: %s", err);
        }
    }
    tw.Close();

    var tr = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuffer));

    for (;;) {
        var (header, err) = tr.Next();
        if (AreEqual(err, io.EOF)) {
            break;
        }
        if (err != nil) {
            t.Fatalf("Failed to read header: %s", err);
        }
        if (header.Typeflag != TypeReg) {
            t.Fatalf("Typeflag should've been %d, found %d", (int32)(TypeReg), header.Typeflag);
        }
    }
}

// failOnceWriter fails exactly once and then always reports success.
internal partial struct failOnceWriter /*bool*/;

internal static (nint, error) Write(this ref failOnceWriter w, slice<byte> b) {
    if (((failOnceWriter)(!(bool)w))) {
        return (0, io.ErrShortWrite);
    }
    w = true;
    return (len(b), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string headerOnlyˢ => ref headerOnlyˢᵇ;
private static readonly @string headerOnlyˢᵇ = "HeaderOnly"u8;
internal static ref readonly @string negativeSizeˢ => ref negativeSizeˢᵇ;
private static readonly @string negativeSizeˢᵇ = "NegativeSize"u8;
internal static ref readonly @string beforeHeaderˢ => ref beforeHeaderˢᵇ;
private static readonly @string beforeHeaderˢᵇ = "BeforeHeader"u8;
internal static ref readonly @string afterCloseˢ => ref afterCloseˢᵇ;
private static readonly @string afterCloseˢᵇ = "AfterClose"u8;
internal static ref readonly @string prematureFlushˢ => ref prematureFlushˢᵇ;
private static readonly @string prematureFlushˢᵇ = "PrematureFlush"u8;
internal static ref readonly @string prematureCloseˢ => ref prematureCloseˢᵇ;
private static readonly @string prematureCloseˢᵇ = "PrematureClose"u8;
internal static ref readonly @string persistenceˢ => ref persistenceˢᵇ;
private static readonly @string persistenceˢᵇ = "Persistence"u8;

public static void TestWriterErrors(ж<testing.T> t) {
    t.Run(headerOnlyˢ, (ж<testing.T> tΔ1) => {
        var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(@new<bytes.Buffer>()));
        var hdr = Ꮡ(new Header(Name: "dir/", Typeflag: TypeDir));
        {
            var err = tw.WriteHeader(hdr); if (err != nil) {
                tΔ1.Fatalf("WriteHeader() = %v, want nil", err);
            }
        }
        {
            var (_, err) = tw.Write([0x00]); if (!AreEqual(err, ErrWriteTooLong)) {
                tΔ1.Fatalf("Write() = %v, want %v", err, ErrWriteTooLong);
            }
        }
    });

    t.Run(negativeSizeˢ, (ж<testing.T> tΔ2) => {
        var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(@new<bytes.Buffer>()));
        var hdr = Ꮡ(new Header(Name: "small.txt", Size: -1));
        var err = tw.WriteHeader(hdr); if (err == nil) {
            tΔ2.Fatalf("WriteHeader() = nil, want non-nil error");
        }
    });

    t.Run(beforeHeaderˢ, (ж<testing.T> tΔ3) => {
        var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(@new<bytes.Buffer>()));
        var (_, err) = tw.Write(slice<byte>("Kilts"u8)); if (!AreEqual(err, ErrWriteTooLong)) {
            tΔ3.Fatalf("Write() = %v, want %v", err, ErrWriteTooLong);
        }
    });

    t.Run(afterCloseˢ, (ж<testing.T> tΔ4) => {
        var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(@new<bytes.Buffer>()));
        var hdr = Ꮡ(new Header(Name: "small.txt"));
        {
            var err = tw.WriteHeader(hdr); if (err != nil) {
                tΔ4.Fatalf("WriteHeader() = %v, want nil", err);
            }
        }
        {
            var err = tw.Close(); if (err != nil) {
                tΔ4.Fatalf("Close() = %v, want nil", err);
            }
        }
        {
            var (_, err) = tw.Write(slice<byte>("Kilts"u8)); if (!AreEqual(err, ErrWriteAfterClose)) {
                tΔ4.Fatalf("Write() = %v, want %v", err, ErrWriteAfterClose);
            }
        }
        {
            var err = tw.Flush(); if (!AreEqual(err, ErrWriteAfterClose)) {
                tΔ4.Fatalf("Flush() = %v, want %v", err, ErrWriteAfterClose);
            }
        }
        {
            var err = tw.Close(); if (err != nil) {
                tΔ4.Fatalf("Close() = %v, want nil", err);
            }
        }
    });

    t.Run(prematureFlushˢ, (ж<testing.T> tΔ5) => {
        var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(@new<bytes.Buffer>()));
        var hdr = Ꮡ(new Header(Name: "small.txt", Size: 5));
        {
            var err = tw.WriteHeader(hdr); if (err != nil) {
                tΔ5.Fatalf("WriteHeader() = %v, want nil", err);
            }
        }
        {
            var err = tw.Flush(); if (err == nil) {
                tΔ5.Fatalf("Flush() = %v, want non-nil error", err);
            }
        }
    });

    t.Run(prematureCloseˢ, (ж<testing.T> tΔ6) => {
        var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(@new<bytes.Buffer>()));
        var hdr = Ꮡ(new Header(Name: "small.txt", Size: 5));
        {
            var err = tw.WriteHeader(hdr); if (err != nil) {
                tΔ6.Fatalf("WriteHeader() = %v, want nil", err);
            }
        }
        {
            var err = tw.Close(); if (err == nil) {
                tΔ6.Fatalf("Close() = %v, want non-nil error", err);
            }
        }
    });

    t.Run(persistenceˢ, (ж<testing.T> tΔ7) => {
        var tw = NewWriter(new tar_internal_test_package.failOnceWriterжWriter(@new<failOnceWriter>()));
        {
            var err = tw.WriteHeader(Ꮡ(new Header())); if (!AreEqual(err, io.ErrShortWrite)) {
                tΔ7.Fatalf("WriteHeader() = %v, want %v", err, io.ErrShortWrite);
            }
        }
        {
            var err = tw.WriteHeader(Ꮡ(new Header(Name: "small.txt"))); if (err == nil) {
                tΔ7.Errorf("WriteHeader() = got %v, want non-nil error", err);
            }
        }
        {
            var (_, err) = tw.Write(default); if (err == nil) {
                tΔ7.Errorf("Write() = %v, want non-nil error", err);
            }
        }
        {
            var err = tw.Flush(); if (err == nil) {
                tΔ7.Errorf("Flush() = %v, want non-nil error", err);
            }
        }
        {
            var err = tw.Close(); if (err == nil) {
                tΔ7.Errorf("Close() = %v, want non-nil error", err);
            }
        }
    });
}

internal partial struct TestSplitUSTARPath_vectors /*dyn*/ {
    internal @string input;  // Input path
    internal @string prefix; // Expected output prefix
    internal @string suffix; // Expected output suffix
    internal bool ok;        // Split success?
}

public static void TestSplitUSTARPath(ж<testing.T> t) {
    var sr = strings.Repeat;

    var vectors = slice<TestSplitUSTARPath_vectors>([
        new("", "", "", false),
        new("abc", "", "", false),
        new("用戶名", "", "", false),
        new(sr("a", nameSize), "", "", false),
        new(sr("a", nameSize) + "/"u8, "", "", false),
        new(sr("a", nameSize) + "/a"u8, sr("a", nameSize), "a", true),
        new(sr("a", prefixSize) + "/"u8, "", "", false),
        new(sr("a", prefixSize) + "/a"u8, sr("a", prefixSize), "a", true),
        new(sr("a", nameSize + 1), "", "", false),
        new(sr("/", nameSize + 1), sr("/", nameSize - 1), "/", true),
        new(sr("a", prefixSize) + "/"u8 + sr("b", nameSize),
            sr("a", prefixSize), sr("b", nameSize), true),
        new(sr("a", prefixSize) + "//"u8 + sr("b", nameSize), "", "", false),
        new(sr("a/", nameSize), sr("a/", 77) + "a"u8, sr("a/", 22), true)
    ]);

    foreach (var (_, v) in range(vectors)) {
        var (prefix, suffix, ok) = splitUSTARPath(v.input);
        if (prefix != v.prefix || suffix != v.suffix || ok != v.ok) {
            t.Errorf("splitUSTARPath(%q):\ngot  (%q, %q, %v)\nwant (%q, %q, %v)",
                v.input, prefix, suffix, ok, v.prefix, v.suffix, v.ok);
        }
    }
}

// TestIssue12594 tests that the Writer does not attempt to populate the prefix
// field when encoding a header in the GNU format. The prefix field is valid
// in USTAR and PAX, but not GNU.
public static void TestIssue12594(ж<testing.T> t) {
    var names = slice<@string>([
        "0/1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16/17/18/19/20/21/22/23/24/25/26/27/28/29/30/file.txt",
        "0/1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16/17/18/19/20/21/22/23/24/25/26/27/28/29/30/31/32/33/file.txt",
        "0/1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16/17/18/19/20/21/22/23/24/25/26/27/28/29/30/31/32/333/file.txt",
        "0/1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16/17/18/19/20/21/22/23/24/25/26/27/28/29/30/31/32/33/34/35/36/37/38/39/40/file.txt",
        "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000/file.txt",
        "/home/support/.openoffice.org/3/user/uno_packages/cache/registry/com.sun.star.comp.deployment.executable.PackageRegistryBackend"
    ]);

    foreach (var (i, vᴛ1) in range(names)) {
        ref var name = ref heap(new @string(), out var Ꮡname);
        name = vᴛ1;

        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);

        var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡb));
        var errΔ1 = tw.WriteHeader(Ꮡ(new Header(
            Name: name,
            Uid: (1 << 25) // Prevent USTAR format
        ))); if (errΔ1 != nil) {
            t.Errorf("test %d, unexpected WriteHeader error: %v", i, errΔ1);
        }
        var errΔ2 = tw.Close(); if (errΔ2 != nil) {
            t.Errorf("test %d, unexpected Close error: %v", i, errΔ2);
        }

        // The prefix field should never appear in the GNU format.
        ref var blk = ref heap(new global::go.archive.tar_package.block(), out var Ꮡblk);
        copy(blk[..], b.Bytes());
        @string prefix = (@string)Ꮡblk.toUSTAR().prefix();
        (prefix, _, _) = strings.Cut(prefix, "\x00"); // Truncate at the NUL terminator
        if (Ꮡblk.getFormat() == FormatGNU && len(prefix) > 0 && strings.HasPrefix(name, prefix)) {
            t.Errorf("test %d, found prefix in GNU format: %s", i, prefix);
        }

        var tr = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡb));
        var (hdr, err) = tr.Next();
        if (err != nil && !AreEqual(err, ErrInsecurePath)) {
            t.Errorf("test %d, unexpected Next error: %v", i, err);
        }
        if (hdr.Name != name) {
            t.Errorf("test %d, hdr.Name = %s, want %s", i, hdr.Name, name);
        }
    }
}

internal partial struct TestWriteLongHeader_type /*dyn*/ {
    internal @string name;
    internal ж<global::go.archive.tar_package.Header> h;
}

public static void TestWriteLongHeader(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestWriteLongHeader_type>([new(
        name: "name too long",
        h: Ꮡ(new Header(Name: strings.Repeat("a", maxSpecialFileSize)))
    ), new(
        name: "linkname too long",
        h: Ꮡ(new Header(Linkname: strings.Repeat("a", maxSpecialFileSize)))
    ), new(
        name: "uname too long",
        h: Ꮡ(new Header(Uname: strings.Repeat("a", maxSpecialFileSize)))
    ), new(
        name: "gname too long",
        h: Ꮡ(new Header(Gname: strings.Repeat("a", maxSpecialFileSize)))
    ), new(
        name: "PAX header too long",
        h: Ꮡ(new Header(PAXRecords: new map<@string, @string>{["GOLANG.x"] = strings.Repeat("a", maxSpecialFileSize)}))
    )
    ]))) {
        var w = NewWriter(io.Discard);
        var err = w.WriteHeader(test.h); if (!AreEqual(err, ErrFieldTooLong)) {
            t.Errorf("%v: w.WriteHeader() = %v, want ErrFieldTooLong", test.name, err);
        }
    }
}

// testNonEmptyWriter wraps an io.Writer and ensures that
// Write is never called with an empty buffer.
internal partial struct testNonEmptyWriter {
    /*embed*/ public io_package.Writer Writer;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unexpectedEmptyWriteCallˢ => ref unexpectedEmptyWriteCallˢᵇ;
private static readonly @string unexpectedEmptyWriteCallˢᵇ = "unexpected empty Write call"u8;

internal static (nint, error) Write(this testNonEmptyWriter w, slice<byte> b) {
    if (len(b) == 0) {
        return (0, errors.New(unexpectedEmptyWriteCallˢ));
    }
    return w.Writer.Write(b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cdeXˢ = (@string)"cdeX"u8;

internal partial struct TestFileWriter_testWrite /*dyn*/ {
// Write(str) == (wantCnt, wantErr)
    internal @string str;
    internal nint wantCnt;
    internal error wantErr;
}

internal partial struct TestFileWriter_testReadFrom /*dyn*/ {
// ReadFrom(testFile{ops}) == (wantCnt, wantErr)
    internal fileOps ops;
    internal int64 wantCnt;
    internal error wantErr;
}

internal partial struct TestFileWriter_testRemaining /*dyn*/ {
// logicalRemaining() == wantLCnt, physicalRemaining() == wantPCnt
    internal int64 wantLCnt;
    internal int64 wantPCnt;
}

internal partial struct TestFileWriter_makeReg /*dyn*/ {
    internal int64 size;
    internal @string wantStr;
}

internal partial struct TestFileWriter_makeSparse /*dyn*/ {
    internal TestFileWriter_makeReg makeReg;
    internal global::go.archive.tar_package.sparseHoles sph;
    internal int64 size;
}

internal partial struct TestFileWriter_vectors /*dyn*/ {
    internal TestFileWriter_fileMaker maker;
    internal slice<TestFileWriter_testFnc> tests;
}

public static void TestFileWriter(ж<testing.T> t) {
    var vectors = slice<TestFileWriter_vectors>([new(
        maker: new TestFileWriter_makeReg(0, ""),
        tests: [
            new TestFileWriter_testRemaining(0, 0),
            new TestFileWriter_testWrite("", 0, default),
            new TestFileWriter_testWrite("a", 0, ErrWriteTooLong),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)""u8}.slice()), 0, default),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"a"}.slice()), 0, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeReg(1, "a"),
        tests: [
            new TestFileWriter_testRemaining(1, 1),
            new TestFileWriter_testWrite("", 0, default),
            new TestFileWriter_testWrite("a", 1, default),
            new TestFileWriter_testWrite("bcde", 0, ErrWriteTooLong),
            new TestFileWriter_testWrite("", 0, default),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)""u8}.slice()), 0, default),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"a"}.slice()), 0, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeReg(5, "hello"),
        tests: [
            new TestFileWriter_testRemaining(5, 5),
            new TestFileWriter_testWrite("hello", 5, default),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeReg(5, "\x00\x00\x00\x00\x00"),
        tests: [
            new TestFileWriter_testRemaining(5, 5),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"\x00\x00\x00\x00\x00"}.slice()), 5, default),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeReg(5, "\x00\x00\x00\x00\x00"),
        tests: [
            new TestFileWriter_testRemaining(5, 5),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x65, 0x78, 0x74, 0x72, 0x61}))}.slice()), 5, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeReg(5, "abc\x00\x00"),
        tests: [
            new TestFileWriter_testRemaining(5, 5),
            new TestFileWriter_testWrite("abc", 3, default),
            new TestFileWriter_testRemaining(2, 2),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"\x00\x00"}.slice()), 2, default),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeReg(5, ((@string)(new byte[]{0x00, 0x00, 0x61, 0x62, 0x63}))),
        tests: [
            new TestFileWriter_testRemaining(5, 5),
            new TestFileWriter_testWrite("\x00\x00", 2, default),
            new TestFileWriter_testRemaining(3, 3),
            new TestFileWriter_testWrite("abc", 3, default),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"z"}.slice()), 0, ErrWriteTooLong),
            new TestFileWriter_testWrite("z", 0, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(5, "abcde"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: [
            new TestFileWriter_testRemaining(8, 5),
            new TestFileWriter_testWrite(((@string)(new byte[]{0x61, 0x62, 0x00, 0x00, 0x00, 0x63, 0x64, 0x65})), 8, default),
            new TestFileWriter_testWrite("a", 0, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(5, "abcde"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: [
            new TestFileWriter_testWrite(((@string)(new byte[]{0x61, 0x62, 0x00, 0x00, 0x00, 0x63, 0x64, 0x65, 0x7a})), 8, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(5, "abcde"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: [
            new TestFileWriter_testWrite("ab\x00", 3, default),
            new TestFileWriter_testRemaining(5, 3),
            new TestFileWriter_testWrite(((@string)(new byte[]{0x00, 0x00, 0x63, 0x64, 0x65})), 5, default),
            new TestFileWriter_testWrite("a", 0, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(5, "abcde"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: [
            new TestFileWriter_testWrite("ab", 2, default),
            new TestFileWriter_testRemaining(6, 3),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(int64)3, cdeˢ}.slice()), 6, default),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(5, "abcde"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: [
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"ab", (int64)3, cdeˢ}.slice()), 8, default),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(5, "abcde"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: [
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"ab", (int64)3, cdeXˢ}.slice()), 8, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(4, "abcd"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: [
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"ab", (int64)3, (@string)"cd"}.slice()), 7, io.ErrUnexpectedEOF),
            new TestFileWriter_testRemaining(1, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(4, "abcd"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: [
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"ab", (int64)3, cdeˢ}.slice()), 7, errMissData),
            new TestFileWriter_testRemaining(1, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(6, "abcde"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: [
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"ab", (int64)3, cdeˢ}.slice()), 8, errUnrefData),
            new TestFileWriter_testRemaining(0, 1)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(4, "abcd"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: [
            new TestFileWriter_testWrite("ab", 2, default),
            new TestFileWriter_testRemaining(6, 2),
            new TestFileWriter_testWrite("\x00\x00\x00", 3, default),
            new TestFileWriter_testRemaining(3, 2),
            new TestFileWriter_testWrite("cde", 2, errMissData),
            new TestFileWriter_testRemaining(1, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(6, "abcde"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: [
            new TestFileWriter_testWrite("ab", 2, default),
            new TestFileWriter_testRemaining(6, 4),
            new TestFileWriter_testWrite("\x00\x00\x00", 3, default),
            new TestFileWriter_testRemaining(3, 4),
            new TestFileWriter_testWrite("cde", 3, errUnrefData),
            new TestFileWriter_testRemaining(0, 1)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(3, "abc"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: [
            new TestFileWriter_testRemaining(7, 3),
            new TestFileWriter_testWrite(((@string)(new byte[]{0x00, 0x00, 0x61, 0x62, 0x63, 0x00, 0x00})), 7, default),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(3, "abc"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: [
            new TestFileWriter_testRemaining(7, 3),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(int64)2, abcˢ, (int64)1, (@string)"\x00"}.slice()), 7, default),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(3, ""), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: [
            new TestFileWriter_testWrite("abcdefg", 0, errWriteHole)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(3, "abc"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: [
            new TestFileWriter_testWrite(((@string)(new byte[]{0x00, 0x00, 0x61, 0x62, 0x63, 0x64, 0x65})), 5, errWriteHole)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(3, "abc"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: [
            new TestFileWriter_testWrite(((@string)(new byte[]{0x00, 0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x7a})), 7, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(3, "abc"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: [
            new TestFileWriter_testWrite("\x00\x00", 2, default),
            new TestFileWriter_testRemaining(5, 3),
            new TestFileWriter_testWrite("abc", 3, default),
            new TestFileWriter_testRemaining(2, 0),
            new TestFileWriter_testWrite("\x00\x00", 2, default),
            new TestFileWriter_testRemaining(0, 0)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(2, "ab"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: [
            new TestFileWriter_testWrite("\x00\x00", 2, default),
            new TestFileWriter_testWrite("abc", 2, errMissData),
            new TestFileWriter_testWrite("\x00\x00", 0, errMissData)
        ]
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(4, "abc"), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: [
            new TestFileWriter_testWrite("\x00\x00", 2, default),
            new TestFileWriter_testWrite("abc", 3, default),
            new TestFileWriter_testWrite("\x00\x00", 2, errUnrefData)
        ]
    )
    ]);

    foreach (var (i, v) in range(vectors)) {
        @string wantStr = default;
        var bb = @new<strings.Builder>();
        ref var w = ref heap<testNonEmptyWriter>(new testNonEmptyWriter(new tar_test_package.strings_BuilderжWriter(bb)), out var Ꮡw);
        global::go.archive.tar_package.fileWriter fw = default;
        switch (v.maker.type()) {
        case TestFileWriter_makeReg maker: {
            fw = new global::go.archive.tar_package.regFileWriterжfileWriter(Ꮡ(new regFileWriter(w, maker.size)));
            wantStr = maker.wantStr;
            break;
        }
        case TestFileWriter_makeSparse maker: {
            if (!validateSparseEntries(maker.sph, maker.size)) {
                t.Fatalf("invalid sparse map: %v", maker.sph);
            }
            var spd = invertSparseEntries(maker.sph, maker.size);
            fw = new global::go.archive.tar_package.regFileWriterжfileWriter(Ꮡ(new regFileWriter(w, maker.makeReg.size)));
            fw = new global::go.archive.tar_package.sparseFileWriterжfileWriter(Ꮡ(new sparseFileWriter(fw, spd, 0)));
            wantStr = maker.makeReg.wantStr;
            break;
        }
        default: {
            var maker = v.maker;
            t.Fatalf("test %d, unknown make operation: %T", i, maker);
            break;
        }}

        foreach (var (j, tf) in range(v.tests)) {
            switch (tf.type()) {
            case TestFileWriter_testWrite tfΔ1: {
                var (got, err) = fw.Write(slice<byte>(tfΔ1.str));
                if (got != tfΔ1.wantCnt || !AreEqual(err, tfΔ1.wantErr)) {
                    t.Errorf("test %d.%d, Write(%s):\ngot  (%d, %v)\nwant (%d, %v)", i, j, tfΔ1.str, got, err, tfΔ1.wantCnt, tfΔ1.wantErr);
                }
                break;
            }
            case TestFileWriter_testReadFrom tfΔ1: {
                var f = Ꮡ(new testFile(ops: tfΔ1.ops));
                var (got, err) = fw.ReadFrom(new tar_internal_test_package.testFileжReader(f));
                var (_, ok) = err._<testError>(ᐧ); if (ok) {
                    t.Errorf("test %d.%d, ReadFrom(): %v", i, j, err);
                } else if (got != tfΔ1.wantCnt || !AreEqual(err, tfΔ1.wantErr)) {
                    t.Errorf("test %d.%d, ReadFrom() = (%d, %v), want (%d, %v)", i, j, got, err, tfΔ1.wantCnt, tfΔ1.wantErr);
                }
                if (len(f.ops) > 0) {
                    t.Errorf("test %d.%d, expected %d more operations", i, j, len(f.ops));
                }
                break;
            }
            case TestFileWriter_testRemaining tfΔ1: {
                {
                    var got = fw.logicalRemaining(); if (got != tfΔ1.wantLCnt) {
                        t.Errorf("test %d.%d, logicalRemaining() = %d, want %d", i, j, got, tfΔ1.wantLCnt);
                    }
                }
                {
                    var got = fw.physicalRemaining(); if (got != tfΔ1.wantPCnt) {
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

        {
            @string got = bb.String(); if (got != wantStr) {
                t.Fatalf("test %d, String() = %q, want %q", i, got, wantStr);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string subfolderˢ => ref subfolderˢᵇ;
private static readonly @string subfolderˢᵇ = "subfolder"u8;

public static void TestWriterAddFS(ж<testing.T> t) {
    var namesᴷ = new stackAppend<@string>();

    var fsys = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
        ["emptyfolder"] = Ꮡ(new fstest.MapFile(Mode: (fs.FileMode)(493 | os.ModeDir))),
        ["file.go"] = Ꮡ(new fstest.MapFile(Data: slice<byte>("hello"u8))),
        ["subfolder/another.go"] = Ꮡ(new fstest.MapFile(Data: slice<byte>("world"u8))),
        ["symlink.go"] = Ꮡ(new fstest.MapFile(Mode: (fs.FileMode)(511 | os.ModeSymlink), Data: slice<byte>("file.go"u8)))
        // Notably missing here is the "subfolder" directory. This makes sure even
        // if we don't have a subfolder directory listed.
    });
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    {
        var err = tw.AddFS(new tar_test_package.fstest_MapFSᴠFS(fsys)); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var err = tw.Close(); if (err != nil) {
            t.Fatal(err);
        }
    }

    // Add subfolder into fsys to match what we'll read from the tar.
    fsys[subfolderˢ] = Ꮡ(new fstest.MapFile(Mode: (fs.FileMode)(365 | os.ModeDir)));

    // Test that we can get the files back from the archive
    var tr = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuf));

    var names = new slice<@string>(0, len(fsys));
    foreach (var (name, _) in fsys) {
        names = append(namesᴷ, names, name);
    }
    sort.Strings(names);

    nint entriesLeft = len(fsys);
    foreach (var (_, name) in range(names)) {
        entriesLeft--;

        var (entryInfo, err) = fsys.Lstat(name);
        if (err != nil) {
            t.Fatalf("getting entry info error: %v", err);
        }
        (var hdr, err) = tr.Next();
        if (AreEqual(err, io.EOF)) {
            break; // End of archive
        }
        if (err != nil) {
            t.Fatal(err);
        }

        @string tmpName = name;
        if (entryInfo.IsDir()) {
            tmpName += "/"u8;
        }
        if (hdr.Name != tmpName) {
            t.Errorf("test fs has filename %v; archive header has %v",
                name, hdr.Name);
        }

        if (entryInfo.Mode() != hdr.FileInfo().Mode()) {
            t.Errorf("%s: test fs has mode %v; archive header has %v",
                name, entryInfo.Mode(), hdr.FileInfo().Mode());
        }

        switch (entryInfo.Mode().Type()) {
        case var ᴛ when ᴛ == fs.ModeDir: {
            // No additional checks necessary.
            break;
        }
        case var ᴛ when ᴛ == fs.ModeSymlink: {
            @string origtarget = (@string)fsys[name].Data;
            if (hdr.Linkname != origtarget) {
                t.Fatalf("test fs has link content %s; archive header %v", origtarget, hdr.Linkname);
            }
            break;
        }
        default: {
            var (data, errΔ2) = io.ReadAll(new global::go.archive.tar_package.ReaderжReader(tr));
            if (errΔ2 != nil) {
                t.Fatal(errΔ2);
            }
            var origdata = fsys[name].Data;
            if (((sstring)data) != ((sstring)origdata)) {
                t.Fatalf("test fs has file content %v; archive header has %v", origdata, data);
            }
            break;
        }}
    }
    if (entriesLeft > 0) {
        t.Fatalf("not all entries are in the archive");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorGotNilˢ = (@string)"expected error, got nil"u8;

public static void TestWriterAddFSNonRegularFiles(ж<testing.T> t) {
    var fsys = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
        ["device"] = Ꮡ(new fstest.MapFile(Data: slice<byte>("hello"u8), Mode: (fs.FileMode)(493 | fs.ModeDevice))),
        ["symlink"] = Ꮡ(new fstest.MapFile(Data: slice<byte>("world"u8), Mode: (fs.FileMode)(493 | fs.ModeSymlink)))
    });
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    var err = tw.AddFS(new tar_test_package.fstest_MapFSᴠFS(fsys)); if (err == nil) {
        t.Fatal(expectedErrorGotNilˢ);
    }
}

} // end tar_internal_test_package
