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
using encoding;
using go.io;
using go.testing;
using static go.archive.tar_package;

partial class tar_internal_test_package {

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string uniqueAᶜ = "-  "u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string uniqueBᶜ = "+  "u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string identityᶜ = "   "u8;

internal static @string bytediff(slice<byte> a, slice<byte> b) {
    @string uniqueA = uniqueAᶜ;
    @string uniqueB = uniqueBᶜ;
    @string identity = identityᶜ;
    slice<@string> ss = default!;
    var sa = strings.Split(strings.TrimSpace(hex.Dump(a)), "\n"u8);
    var sb = strings.Split(strings.TrimSpace(hex.Dump(b)), "\n"u8);
    while (len(sa) > 0 && len(sb) > 0) {
        if (sa[0] == sb[0]){
            ss = append(ss, identity + sa[0]);
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
    return strings.Join(ss, "\n"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string longnameˢ = "longname/"u8;

[GoType("dyn")] [GoLocalName("testHeader")] internal partial struct TestWriter_testHeader {
// WriteHeader(hdr) == wantErr
    internal global::go.archive.tar_package.Header hdr;
    internal error wantErr;
}

[GoType("dyn")] [GoLocalName("testWrite")] internal partial struct TestWriter_testWrite {
// Write(str) == (wantCnt, wantErr)
    internal @string str;
    internal nint wantCnt;
    internal error wantErr;
}

[GoType("dyn")] [GoLocalName("testReadFrom")] internal partial struct TestWriter_testReadFrom {
// ReadFrom(testFile{ops}) == (wantCnt, wantErr)
    internal fileOps ops;
    internal int64 wantCnt;
    internal error wantErr;
}

[GoType("dyn")] [GoLocalName("testClose")] internal partial struct TestWriter_testClose {
// Close() == wantErr
    internal error wantErr;
}

[GoType("dyn")] internal partial struct TestWriter_vectors {
    internal @string @file; // Optional filename of expected output
    internal slice<TestWriter_testFnc> tests;
}

public static void TestWriter(ж<testing.T> Ꮡt) {
    var vectors = new TestWriter_vectors[]{new(
        @file: "testdata/writer.tar"u8, // The writer test file was produced with this command:
 // tar (GNU tar) 1.26
 //   ln -s small.txt link.txt
 //   tar -b 1 --format=ustar -c -f writer.tar small.txt small2.txt link.txt

        tests: new TestWriter_testFnc[]{
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "small.txt"u8,
                Size: 5,
                Mode: 416,
                Uid: 73025,
                Gid: 5000,
                Uname: "dsymonds"u8,
                Gname: "eng"u8,
                ModTime: time.Unix(1246508266, 0)
            ), default!),
            new TestWriter_testWrite("Kilts"u8, 5, default!),
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "small2.txt"u8,
                Size: 11,
                Mode: 416,
                Uid: 73025,
                Uname: "dsymonds"u8,
                Gname: "eng"u8,
                Gid: 5000,
                ModTime: time.Unix(1245217492, 0)
            ), default!),
            new TestWriter_testWrite("Google.com\n"u8, 11, default!),
            new TestWriter_testHeader(new Header(
                Typeflag: TypeSymlink,
                Name: "link.txt"u8,
                Linkname: "small.txt"u8,
                Mode: 511,
                Uid: 1000,
                Gid: 1000,
                Uname: "strings"u8,
                Gname: "strings"u8,
                ModTime: time.Unix(1314603082, 0)
            ), default!),
            new TestWriter_testWrite(""u8, 0, default!),
            new TestWriter_testClose((error)(default!))
        }.slice()
    ), new(
        @file: "testdata/writer-big.tar"u8, // The truncated test file was produced using these commands:
 //   dd if=/dev/zero bs=1048576 count=16384 > /tmp/16gig.txt
 //   tar -b 1 -c -f- /tmp/16gig.txt | dd bs=512 count=8 > writer-big.tar

        tests: new TestWriter_testFnc[]{
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "tmp/16gig.txt"u8,
                Size: 17179869184L,
                Mode: 416,
                Uid: 73025,
                Gid: 5000,
                Uname: "dsymonds"u8,
                Gname: "eng"u8,
                ModTime: time.Unix(1254699560, 0),
                Format: FormatGNU
            ), default!)
        }.slice()
    ), new(
        @file: "testdata/writer-big-long.tar"u8, // This truncated file was produced using this library.
 // It was verified to work with GNU tar 1.27.1 and BSD tar 3.1.2.
 //  dd if=/dev/zero bs=1G count=16 >> writer-big-long.tar
 //  gnutar -xvf writer-big-long.tar
 //  bsdtar -xvf writer-big-long.tar
 //
 // This file is in PAX format.

        tests: new TestWriter_testFnc[]{
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: strings.Repeat(longnameˢ, 15) + "16gig.txt"u8,
                Size: 17179869184L,
                Mode: 420,
                Uid: 1000,
                Gid: 1000,
                Uname: "guillaume"u8,
                Gname: "guillaume"u8,
                ModTime: time.Unix(1399583047, 0)
            ), default!)
        }.slice()
    ), new(
        @file: "testdata/ustar.tar"u8, // This file was produced using GNU tar v1.17.
 //	gnutar -b 4 --format=ustar (longname/)*15 + file.txt

        tests: new TestWriter_testFnc[]{
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: strings.Repeat(longnameˢ, 15) + "file.txt"u8,
                Size: 6,
                Mode: 420,
                Uid: 501,
                Gid: 20,
                Uname: "shane"u8,
                Gname: "staff"u8,
                ModTime: time.Unix(1360135598, 0)
            ), default!),
            new TestWriter_testWrite("hello\n"u8, 6, default!),
            new TestWriter_testClose((error)(default!))
        }.slice()
    ), new(
        @file: "testdata/hardlink.tar"u8, // This file was produced using GNU tar v1.26:
 //	echo "Slartibartfast" > file.txt
 //	ln file.txt hard.txt
 //	tar -b 1 --format=ustar -c -f hardlink.tar file.txt hard.txt

        tests: new TestWriter_testFnc[]{
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "file.txt"u8,
                Size: 15,
                Mode: 420,
                Uid: 1000,
                Gid: 100,
                Uname: "vbatts"u8,
                Gname: "users"u8,
                ModTime: time.Unix(1425484303, 0)
            ), default!),
            new TestWriter_testWrite("Slartibartfast\n"u8, 15, default!),
            new TestWriter_testHeader(new Header(
                Typeflag: TypeLink,
                Name: "hard.txt"u8,
                Linkname: "file.txt"u8,
                Mode: 420,
                Uid: 1000,
                Gid: 100,
                Uname: "vbatts"u8,
                Gname: "users"u8,
                ModTime: time.Unix(1425484303, 0)
            ), default!),
            new TestWriter_testWrite(""u8, 0, default!),
            new TestWriter_testClose((error)(default!))
        }.slice()
    ), new(
        tests: new TestWriter_testFnc[]{
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "bad-null.txt"u8,
                Xattrs: new map<@string, @string>{["null\x00null\x00"u8] = "fizzbuzz"u8}
            ), new headerError(new @string[]{}.slice()))
        }.slice()
    ), new(
        tests: new TestWriter_testFnc[]{
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "null\x00.txt"u8
            ), new headerError(new @string[]{}.slice()))
        }.slice()
    ), new(
        @file: "testdata/pax-records.tar"u8,
        tests: new TestWriter_testFnc[]{
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "file"u8,
                Uname: strings.Repeat(longˢ, 10),
                PAXRecords: new map<@string, @string>{
                    ["path"u8] = "FILE"u8, // Should be ignored

                    ["GNU.sparse.map"u8] = "0,0"u8, // Should be ignored

                    ["comment"u8] = "Hello, 世界"u8,
                    ["GOLANG.pkg"u8] = "tar"u8
                }
            ), default!),
            new TestWriter_testClose((error)(default!))
        }.slice()
    ), new(
        @file: "testdata/pax-global-records.tar"u8, // Craft a theoretically valid PAX archive with global headers.
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

        tests: new TestWriter_testFnc[]{
            new TestWriter_testHeader(new Header(
                Typeflag: TypeXGlobalHeader,
                PAXRecords: new map<@string, @string>{["path"u8] = "global1"u8, ["mtime"u8] = "1500000000.0"u8}
            ), default!),
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg, Name: "file1"u8
            ), default!),
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "file2"u8,
                PAXRecords: new map<@string, @string>{["path"u8] = "file2"u8}
            ), default!),
            new TestWriter_testHeader(new Header(
                Typeflag: TypeXGlobalHeader,
                PAXRecords: new map<@string, @string>{["path"u8] = ""u8}
            ), // Should delete "path", but keep "mtime"
 default!),
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg, Name: "file3"u8
            ), default!),
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "file4"u8,
                ModTime: time.Unix(1400000000, 0),
                PAXRecords: new map<@string, @string>{["mtime"u8] = "1400000000"u8}
            ), default!),
            new TestWriter_testClose((error)(default!))
        }.slice()
    ), new(
        @file: "testdata/gnu-utf8.tar"u8,
        tests: new TestWriter_testFnc[]{
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: "☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹☺☻☹"u8,
                Mode: 420,
                Uid: 1000, Gid: 1000,
                Uname: "☺"u8,
                Gname: "⚹"u8,
                ModTime: time.Unix(0, 0),
                Format: FormatGNU
            ), default!),
            new TestWriter_testClose((error)(default!))
        }.slice()
    ), new(
        @file: "testdata/gnu-not-utf8.tar"u8,
        tests: new TestWriter_testFnc[]{
            new TestWriter_testHeader(new Header(
                Typeflag: TypeReg,
                Name: ((@string)(new byte[]{0x68, 0x69, 0x80, 0x81, 0x82, 0x83, 0x62, 0x79, 0x65})),
                Mode: 420,
                Uid: 1000,
                Gid: 1000,
                Uname: "rawr"u8,
                Gname: "dsnet"u8,
                ModTime: time.Unix(0, 0),
                Format: FormatGNU
            ), default!),
            new TestWriter_testClose((error)(default!))
        }.slice() // TODO(dsnet): Re-enable this test when adding sparse support.
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
				file: "testdata/gnu-sparse-big.tar",
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
				file: "testdata/pax-sparse-big.tar",
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
        @file: "testdata/trailing-slash.tar"u8,
        tests: new TestWriter_testFnc[]{
            new TestWriter_testHeader(new Header(Name: strings.Repeat("123456789/"u8, 30)), default!),
            new TestWriter_testClose((error)(default!))
        }.slice()
    ), new(
        @file: "testdata/file-and-dir.tar"u8, // Automatically promote zero value of Typeflag depending on the name.

        tests: new TestWriter_testFnc[]{
            new TestWriter_testHeader(new Header(Name: "small.txt"u8, Size: 5), default!),
            new TestWriter_testWrite("Kilts"u8, 5, default!),
            new TestWriter_testHeader(new Header(Name: "dir/"u8), default!),
            new TestWriter_testClose((error)(default!))
        }.slice()
    )
    }.slice();
    bool equalError(error x, error y) {
        var (_, ok1) = x._<headerError>(ᐧ);
        var (_, ok2) = y._<headerError>(ᐧ);
        if (ok1 || ok2) {
            return ok1 && ok2;
        }
        return AreEqual(x, y);
    }
    foreach (var (_, vᴛ1) in vectors) {
        ref var v = ref heap(new TestWriter_vectors(), out var Ꮡv);
        v = vᴛ1;

        var equalErrorʗ1 = equalError;
        var vʗ1 = v;
        Ꮡt.Run(path.Base(v.@file), (ж<testing.T> tΔ1) => {
            const int64 maxSize = /* 10 << 10 */ 10240; // 10KiB
            var buf = @new<bytes.Buffer>();
            var tw = NewWriter(iotest.TruncateWriter(new tar_test_package.bytes_BufferжWriter(buf), maxSize));
            foreach (var (i, tf) in vʗ1.tests) {
                switch (tf.type()) {
                case TestWriter_testHeader tfΔ1ᴛ1: {
                    ref var tfΔ1 = ref heap(tfΔ1ᴛ1, out var ᏑtfΔ1);
                    var err = tw.WriteHeader(ᏑtfΔ1.of(TestWriter_testHeader.Ꮡhdr));
                    if (!equalErrorʗ1(err, tfΔ1.wantErr)) {
                        tΔ1.Fatalf("test %d, WriteHeader() = %v, want %v"u8, i, err, tfΔ1.wantErr);
                    }
                    break;
                }
                case TestWriter_testWrite tfΔ1: {
                    var (got, err) = tw.Write(slice<byte>(tfΔ1.str));
                    if (got != tfΔ1.wantCnt || !equalErrorʗ1(err, tfΔ1.wantErr)) {
                        tΔ1.Fatalf("test %d, Write() = (%d, %v), want (%d, %v)"u8, i, got, err, tfΔ1.wantCnt, tfΔ1.wantErr);
                    }
                    break;
                }
                case TestWriter_testReadFrom tfΔ1: {
                    var f = Ꮡ(new testFile(ops: tfΔ1.ops));
                    var (got, err) = tw.readFrom(new tar_internal_test_package.testFileжReader(f));
                    {
                        var (_, ok) = err._<testError>(ᐧ); if (ok){
                            tΔ1.Errorf("test %d, ReadFrom(): %v"u8, i, err);
                        } else 
                        if (got != tfΔ1.wantCnt || !equalErrorʗ1(err, tfΔ1.wantErr)) {
                            tΔ1.Errorf("test %d, ReadFrom() = (%d, %v), want (%d, %v)"u8, i, got, err, tfΔ1.wantCnt, tfΔ1.wantErr);
                        }
                    }
                    if (len((~f).ops) > 0) {
                        tΔ1.Errorf("test %d, expected %d more operations"u8, i, len((~f).ops));
                    }
                    break;
                }
                case TestWriter_testClose tfΔ1: {
                    var err = tw.Close();
                    if (!equalErrorʗ1(err, tfΔ1.wantErr)) {
                        tΔ1.Fatalf("test %d, Close() = %v, want %v"u8, i, err, tfΔ1.wantErr);
                    }
                    break;
                }
                default: {
                    var tfΔ1 = tf;
                    tΔ1.Fatalf("test %d, unknown test operation: %T"u8, i, tfΔ1);
                    break;
                }}
            }
            if (vʗ1.@file != ""u8) {
                var (want, err) = os.ReadFile(vʗ1.@file);
                if (err != default!) {
                    tΔ1.Fatalf("ReadFile() = %v, want nil"u8, err);
                }
                var got = buf.Bytes();
                if (!bytes.Equal(want, got)) {
                    tΔ1.Fatalf("incorrect result: (-got +want)\n%v"u8, bytediff(got, want));
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedAtLeastOnePaxˢ = (@string)"Expected at least one PAX header to be written."u8;
internal static readonly object couldnTRecoverLongFileˢ = (@string)"Couldn't recover long file name"u8;

public static void TestPax(ж<testing.T> Ꮡt) {
    // Create an archive with a large name
    var (fileinfo, err) = os.Stat(testdataSmallTxtˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var hdr, err) = FileInfoHeader(fileinfo, ""u8);
    if (err != default!) {
        Ꮡt.Fatalf("os.Stat: %v"u8, err);
    }
    // Force a PAX long name to be written
    @string longName = strings.Repeat("ab"u8, 100);
    @string contents = strings.Repeat(" "u8, (nint)(~hdr).Size);
    hdr.Value.Name = longName;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var writer = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    {
        var errΔ1 = writer.WriteHeader(hdr); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    {
        (_, err) = writer.Write(slice<byte>(contents)); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var errΔ2 = writer.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    // Simple test to make sure PAX extensions are in effect
    if (!bytes.Contains(buf.Bytes(), slice<byte>("PaxHeaders.0"u8))) {
        Ꮡt.Fatal(expectedAtLeastOnePaxˢ);
    }
    // Test that we can get a long name back out of the archive.
    var reader = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuf));
    (hdr, err) = reader.Next();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if ((~hdr).Name != longName) {
        Ꮡt.Fatal(couldnTRecoverLongFileˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object couldnTRecoverLongLinkˢ = (@string)"Couldn't recover long link name"u8;

public static void TestPaxSymlink(ж<testing.T> Ꮡt) {
    // Create an archive with a large linkname
    var (fileinfo, err) = os.Stat(testdataSmallTxtˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var hdr, err) = FileInfoHeader(fileinfo, ""u8);
    if (err != default!) {
        Ꮡt.Fatalf("os.Stat:1 %v"u8, err);
    }
    hdr.Value.Typeflag = TypeSymlink;
    // Force a PAX long linkname to be written
    @string longLinkname = strings.Repeat("1234567890/1234567890"u8, 10);
    hdr.Value.Linkname = longLinkname;
    hdr.Value.Size = 0;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var writer = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    {
        var errΔ1 = writer.WriteHeader(hdr); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    {
        var errΔ2 = writer.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    // Simple test to make sure PAX extensions are in effect
    if (!bytes.Contains(buf.Bytes(), slice<byte>("PaxHeaders.0"u8))) {
        Ꮡt.Fatal(expectedAtLeastOnePaxˢ);
    }
    // Test that we can get a long name back out of the archive.
    var reader = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuf));
    (hdr, err) = reader.Next();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if ((~hdr).Linkname != longLinkname) {
        Ꮡt.Fatal(couldnTRecoverLongLinkˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object couldnTRecoverUnicodeˢ = (@string)"Couldn't recover unicode name"u8;
internal static readonly object couldnTRecoverUnicodeˢ2 = (@string)"Couldn't recover unicode group"u8;
internal static readonly object couldnTRecoverUnicodeˢ3 = (@string)"Couldn't recover unicode user"u8;

public static void TestPaxNonAscii(ж<testing.T> Ꮡt) {
    // Create an archive with non ascii. These should trigger a pax header
    // because pax headers have a defined utf-8 encoding.
    var (fileinfo, err) = os.Stat(testdataSmallTxtˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var hdr, err) = FileInfoHeader(fileinfo, ""u8);
    if (err != default!) {
        Ꮡt.Fatalf("os.Stat:1 %v"u8, err);
    }
    // some sample data
    @string chineseFilename = "文件名"u8;
    @string chineseGroupname = "組"u8;
    @string chineseUsername = "用戶名"u8;
    hdr.Value.Name = chineseFilename;
    hdr.Value.Gname = chineseGroupname;
    hdr.Value.Uname = chineseUsername;
    @string contents = strings.Repeat(" "u8, (nint)(~hdr).Size);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var writer = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    {
        var errΔ1 = writer.WriteHeader(hdr); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    {
        (_, err) = writer.Write(slice<byte>(contents)); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var errΔ2 = writer.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    // Simple test to make sure PAX extensions are in effect
    if (!bytes.Contains(buf.Bytes(), slice<byte>("PaxHeaders.0"u8))) {
        Ꮡt.Fatal(expectedAtLeastOnePaxˢ);
    }
    // Test that we can get a long name back out of the archive.
    var reader = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuf));
    (hdr, err) = reader.Next();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if ((~hdr).Name != chineseFilename) {
        Ꮡt.Fatal(couldnTRecoverUnicodeˢ);
    }
    if ((~hdr).Gname != chineseGroupname) {
        Ꮡt.Fatal(couldnTRecoverUnicodeˢ2);
    }
    if ((~hdr).Uname != chineseUsername) {
        Ꮡt.Fatal(couldnTRecoverUnicodeˢ3);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string kiltsˢ = "Kilts"u8;

public static void TestPaxXattrs(ж<testing.T> Ꮡt) {
    var xattrs = new map<@string, @string>{
        ["user.key"u8] = "value"u8
    };
    // Create an archive with an xattr
    var (fileinfo, err) = os.Stat(testdataSmallTxtˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var hdr, err) = FileInfoHeader(fileinfo, ""u8);
    if (err != default!) {
        Ꮡt.Fatalf("os.Stat: %v"u8, err);
    }
    @string contents = kiltsˢ;
    hdr.Value.Xattrs = xattrs;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var writer = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    {
        var errΔ1 = writer.WriteHeader(hdr); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    {
        (_, err) = writer.Write(slice<byte>(contents)); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var errΔ2 = writer.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    // Test that we can get the xattrs back out of the archive.
    var reader = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuf));
    (hdr, err) = reader.Next();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>((~hdr).Xattrs, xattrs)) {
        Ꮡt.Fatalf("xattrs did not survive round trip: got %+v, want %+v"u8,
            (~hdr).Xattrs, xattrs);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object paxHeadersAreNotSortedˢ = (@string)"PAX headers are not sorted"u8;

public static void TestPaxHeadersSorted(ж<testing.T> Ꮡt) {
    var (fileinfo, err) = os.Stat(testdataSmallTxtˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var hdr, err) = FileInfoHeader(fileinfo, ""u8);
    if (err != default!) {
        Ꮡt.Fatalf("os.Stat: %v"u8, err);
    }
    @string contents = strings.Repeat(" "u8, (nint)(~hdr).Size);
    hdr.Value.Xattrs = new map<@string, @string>{
        ["foo"u8] = "foo"u8,
        ["bar"u8] = "bar"u8,
        ["baz"u8] = "baz"u8,
        ["qux"u8] = "qux"u8
    };
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var writer = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    {
        var errΔ1 = writer.WriteHeader(hdr); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    {
        (_, err) = writer.Write(slice<byte>(contents)); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var errΔ2 = writer.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    // Simple test to make sure PAX extensions are in effect
    if (!bytes.Contains(buf.Bytes(), slice<byte>("PaxHeaders.0"u8))) {
        Ꮡt.Fatal(expectedAtLeastOnePaxˢ);
    }
    // xattr bar should always appear before others
    var indices = new nint[]{
        bytes.Index(buf.Bytes(), slice<byte>("bar=bar"u8)),
        bytes.Index(buf.Bytes(), slice<byte>("baz=baz"u8)),
        bytes.Index(buf.Bytes(), slice<byte>("foo=foo"u8)),
        bytes.Index(buf.Bytes(), slice<byte>("qux=qux"u8))
    }.slice();
    if (!slices.IsSorted<slice<nint>, nint>(indices)) {
        Ꮡt.Fatal(paxHeadersAreNotSortedˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object couldnTRecoverLongNameˢ = (@string)"Couldn't recover long name"u8;

public static void TestUSTARLongName(ж<testing.T> Ꮡt) {
    // Create an archive with a path that failed to split with USTAR extension in previous versions.
    var (fileinfo, err) = os.Stat(testdataSmallTxtˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var hdr, err) = FileInfoHeader(fileinfo, ""u8);
    if (err != default!) {
        Ꮡt.Fatalf("os.Stat:1 %v"u8, err);
    }
    hdr.Value.Typeflag = TypeDir;
    // Force a PAX long name to be written. The name was taken from a practical example
    // that fails and replaced ever char through numbers to anonymize the sample.
    @string longName = "/0000_0000000/00000-000000000/0000_0000000/00000-0000000000000/0000_0000000/00000-0000000-00000000/0000_0000000/00000000/0000_0000000/000/0000_0000000/00000000v00/0000_0000000/000000/0000_0000000/0000000/0000_0000000/00000y-00/0000/0000/00000000/0x000000/"u8;
    hdr.Value.Name = longName;
    hdr.Value.Size = 0;
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var writer = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    {
        var errΔ1 = writer.WriteHeader(hdr); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    {
        var errΔ2 = writer.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    // Test that we can get a long name back out of the archive.
    var reader = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuf));
    (hdr, err) = reader.Next();
    if (err != default! && !AreEqual(err, ErrInsecurePath)) {
        Ꮡt.Fatal(err);
    }
    if ((~hdr).Name != longName) {
        Ꮡt.Fatal(couldnTRecoverLongNameˢ);
    }
}

public static void TestValidTypeflagWithPAXHeader(ж<testing.T> Ꮡt) {
    ref var buffer = ref heap(new bytes.Buffer(), out var Ꮡbuffer);
    var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuffer));
    ref var fileName = ref heap<@string>(out var ᏑfileName);
    fileName = strings.Repeat("ab"u8, 100);
    var hdr = Ꮡ(new Header(
        Name: fileName,
        Size: 4,
        Typeflag: 0
    ));
    {
        var err = tw.WriteHeader(hdr); if (err != default!) {
            Ꮡt.Fatalf("Failed to write header: %s"u8, err);
        }
    }
    {
        var (_, err) = tw.Write(slice<byte>("fooo"u8)); if (err != default!) {
            Ꮡt.Fatalf("Failed to write the file's data: %s"u8, err);
        }
    }
    tw.Close();
    var tr = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuffer));
    while (ᐧ) {
        var (header, err) = tr.Next();
        if (AreEqual(err, io.EOF)) {
            break;
        }
        if (err != default!) {
            Ꮡt.Fatalf("Failed to read header: %s"u8, err);
        }
        if ((~header).Typeflag != TypeReg) {
            Ꮡt.Fatalf("Typeflag should've been %d, found %d"u8, (int32)(TypeReg), (~header).Typeflag);
        }
    }
}

[GoType("bool")] internal partial struct failOnceWriter;

[GoRecv] internal static (nint, error) Write(this ref failOnceWriter w, slice<byte> b) {
    if (((failOnceWriter)(!(bool)w))) {
        return (0, io.ErrShortWrite);
    }
    w = true;
    return (len(b), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string headerOnlyˢ = "HeaderOnly"u8;
internal static readonly @string negativeSizeˢ = "NegativeSize"u8;
internal static readonly @string beforeHeaderˢ = "BeforeHeader"u8;
internal static readonly @string afterCloseˢ = "AfterClose"u8;
internal static readonly @string prematureFlushˢ = "PrematureFlush"u8;
internal static readonly @string prematureCloseˢ = "PrematureClose"u8;
internal static readonly @string persistenceˢ = "Persistence"u8;

public static void TestWriterErrors(ж<testing.T> Ꮡt) {
    Ꮡt.Run(headerOnlyˢ, (ж<testing.T> tΔ1) => {
        var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(@new<bytes.Buffer>()));
        var hdr = Ꮡ(new Header(Name: "dir/"u8, Typeflag: TypeDir));
        {
            var err = tw.WriteHeader(hdr); if (err != default!) {
                tΔ1.Fatalf("WriteHeader() = %v, want nil"u8, err);
            }
        }
        {
            var (_, err) = tw.Write(new byte[]{0x00}.slice()); if (!AreEqual(err, ErrWriteTooLong)) {
                tΔ1.Fatalf("Write() = %v, want %v"u8, err, ErrWriteTooLong);
            }
        }
    });
    Ꮡt.Run(negativeSizeˢ, (ж<testing.T> tΔ2) => {
        var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(@new<bytes.Buffer>()));
        var hdr = Ꮡ(new Header(Name: "small.txt"u8, Size: -1));
        {
            var err = tw.WriteHeader(hdr); if (err == default!) {
                tΔ2.Fatalf("WriteHeader() = nil, want non-nil error"u8);
            }
        }
    });
    Ꮡt.Run(beforeHeaderˢ, (ж<testing.T> tΔ3) => {
        var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(@new<bytes.Buffer>()));
        {
            var (_, err) = tw.Write(slice<byte>("Kilts"u8)); if (!AreEqual(err, ErrWriteTooLong)) {
                tΔ3.Fatalf("Write() = %v, want %v"u8, err, ErrWriteTooLong);
            }
        }
    });
    Ꮡt.Run(afterCloseˢ, (ж<testing.T> tΔ4) => {
        var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(@new<bytes.Buffer>()));
        var hdr = Ꮡ(new Header(Name: "small.txt"u8));
        {
            var err = tw.WriteHeader(hdr); if (err != default!) {
                tΔ4.Fatalf("WriteHeader() = %v, want nil"u8, err);
            }
        }
        {
            var err = tw.Close(); if (err != default!) {
                tΔ4.Fatalf("Close() = %v, want nil"u8, err);
            }
        }
        {
            var (_, err) = tw.Write(slice<byte>("Kilts"u8)); if (!AreEqual(err, ErrWriteAfterClose)) {
                tΔ4.Fatalf("Write() = %v, want %v"u8, err, ErrWriteAfterClose);
            }
        }
        {
            var err = tw.Flush(); if (!AreEqual(err, ErrWriteAfterClose)) {
                tΔ4.Fatalf("Flush() = %v, want %v"u8, err, ErrWriteAfterClose);
            }
        }
        {
            var err = tw.Close(); if (err != default!) {
                tΔ4.Fatalf("Close() = %v, want nil"u8, err);
            }
        }
    });
    Ꮡt.Run(prematureFlushˢ, (ж<testing.T> tΔ5) => {
        var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(@new<bytes.Buffer>()));
        var hdr = Ꮡ(new Header(Name: "small.txt"u8, Size: 5));
        {
            var err = tw.WriteHeader(hdr); if (err != default!) {
                tΔ5.Fatalf("WriteHeader() = %v, want nil"u8, err);
            }
        }
        {
            var err = tw.Flush(); if (err == default!) {
                tΔ5.Fatalf("Flush() = %v, want non-nil error"u8, err);
            }
        }
    });
    Ꮡt.Run(prematureCloseˢ, (ж<testing.T> tΔ6) => {
        var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(@new<bytes.Buffer>()));
        var hdr = Ꮡ(new Header(Name: "small.txt"u8, Size: 5));
        {
            var err = tw.WriteHeader(hdr); if (err != default!) {
                tΔ6.Fatalf("WriteHeader() = %v, want nil"u8, err);
            }
        }
        {
            var err = tw.Close(); if (err == default!) {
                tΔ6.Fatalf("Close() = %v, want non-nil error"u8, err);
            }
        }
    });
    Ꮡt.Run(persistenceˢ, (ж<testing.T> tΔ7) => {
        var tw = NewWriter(new tar_internal_test_package.failOnceWriterжWriter(@new<failOnceWriter>()));
        {
            var err = tw.WriteHeader(Ꮡ(new Header(nil))); if (!AreEqual(err, io.ErrShortWrite)) {
                tΔ7.Fatalf("WriteHeader() = %v, want %v"u8, err, io.ErrShortWrite);
            }
        }
        {
            var err = tw.WriteHeader(Ꮡ(new Header(Name: "small.txt"u8))); if (err == default!) {
                tΔ7.Errorf("WriteHeader() = got %v, want non-nil error"u8, err);
            }
        }
        {
            var (_, err) = tw.Write(default!); if (err == default!) {
                tΔ7.Errorf("Write() = %v, want non-nil error"u8, err);
            }
        }
        {
            var err = tw.Flush(); if (err == default!) {
                tΔ7.Errorf("Flush() = %v, want non-nil error"u8, err);
            }
        }
        {
            var err = tw.Close(); if (err == default!) {
                tΔ7.Errorf("Close() = %v, want non-nil error"u8, err);
            }
        }
    });
}

[GoType("dyn")] internal partial struct TestSplitUSTARPath_vectors {
    internal @string input; // Input path
    internal @string prefix; // Expected output prefix
    internal @string suffix; // Expected output suffix
    internal bool ok;   // Split success?
}

public static void TestSplitUSTARPath(ж<testing.T> Ꮡt) {
    var sr = strings.Repeat;
    var vectors = new TestSplitUSTARPath_vectors[]{
        new(""u8, ""u8, ""u8, false),
        new("abc"u8, ""u8, ""u8, false),
        new("用戶名"u8, ""u8, ""u8, false),
        new(sr("a"u8, nameSize), ""u8, ""u8, false),
        new(sr("a"u8, nameSize) + "/"u8, ""u8, ""u8, false),
        new(sr("a"u8, nameSize) + "/a"u8, sr("a"u8, nameSize), "a"u8, true),
        new(sr("a"u8, prefixSize) + "/"u8, ""u8, ""u8, false),
        new(sr("a"u8, prefixSize) + "/a"u8, sr("a"u8, prefixSize), "a"u8, true),
        new(sr("a"u8, nameSize + 1), ""u8, ""u8, false),
        new(sr("/"u8, nameSize + 1), sr("/"u8, nameSize - 1), "/"u8, true),
        new(sr("a"u8, prefixSize) + "/"u8 + sr("b"u8, nameSize),
            sr("a"u8, prefixSize), sr("b"u8, nameSize), true),
        new(sr("a"u8, prefixSize) + "//"u8 + sr("b"u8, nameSize), ""u8, ""u8, false),
        new(sr("a/"u8, nameSize), sr("a/"u8, 77) + "a"u8, sr("a/"u8, 22), true)
    }.slice();
    foreach (var (_, v) in vectors) {
        var (prefix, suffix, ok) = splitUSTARPath(v.input);
        if (prefix != v.prefix || suffix != v.suffix || ok != v.ok) {
            Ꮡt.Errorf("splitUSTARPath(%q):\ngot  (%q, %q, %v)\nwant (%q, %q, %v)"u8,
                v.input, prefix, suffix, ok, v.prefix, v.suffix, v.ok);
        }
    }
}

// TestIssue12594 tests that the Writer does not attempt to populate the prefix
// field when encoding a header in the GNU format. The prefix field is valid
// in USTAR and PAX, but not GNU.
public static void TestIssue12594(ж<testing.T> Ꮡt) {
    var names = new @string[]{
        "0/1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16/17/18/19/20/21/22/23/24/25/26/27/28/29/30/file.txt"u8,
        "0/1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16/17/18/19/20/21/22/23/24/25/26/27/28/29/30/31/32/33/file.txt"u8,
        "0/1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16/17/18/19/20/21/22/23/24/25/26/27/28/29/30/31/32/333/file.txt"u8,
        "0/1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16/17/18/19/20/21/22/23/24/25/26/27/28/29/30/31/32/33/34/35/36/37/38/39/40/file.txt"u8,
        "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000/file.txt"u8,
        "/home/support/.openoffice.org/3/user/uno_packages/cache/registry/com.sun.star.comp.deployment.executable.PackageRegistryBackend"u8
    }.slice();
    foreach (var (i, vᴛ1) in names) {
        ref var name = ref heap(new @string(), out var Ꮡname);
        name = vᴛ1;

        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡb));
        {
            var errΔ1 = tw.WriteHeader(Ꮡ(new Header(
                Name: name,
                Uid: (1 << (int)(25))
            ))); if (errΔ1 != default!) {
                // Prevent USTAR format
                Ꮡt.Errorf("test %d, unexpected WriteHeader error: %v"u8, i, errΔ1);
            }
        }
        {
            var errΔ2 = tw.Close(); if (errΔ2 != default!) {
                Ꮡt.Errorf("test %d, unexpected Close error: %v"u8, i, errΔ2);
            }
        }
        // The prefix field should never appear in the GNU format.
        ref var blk = ref heap(new global::go.archive.tar_package.block(), out var Ꮡblk);
        copy(blk[..], b.Bytes());
        @string prefix = ((@string)Ꮡblk.toUSTAR().prefix());
        (prefix, _, _) = strings.Cut(prefix, "\x00"u8); // Truncate at the NUL terminator
        if (Ꮡblk.getFormat() == FormatGNU && len(prefix) > 0 && strings.HasPrefix(name, prefix)) {
            Ꮡt.Errorf("test %d, found prefix in GNU format: %s"u8, i, prefix);
        }
        var tr = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡb));
        var (hdr, err) = tr.Next();
        if (err != default! && !AreEqual(err, ErrInsecurePath)) {
            Ꮡt.Errorf("test %d, unexpected Next error: %v"u8, i, err);
        }
        if ((~hdr).Name != name) {
            Ꮡt.Errorf("test %d, hdr.Name = %s, want %s"u8, i, (~hdr).Name, name);
        }
    }
}

[GoType("dyn")] internal partial struct TestWriteLongHeader_type {
    internal @string name;
    internal ж<global::go.archive.tar_package.Header> h;
}

public static void TestWriteLongHeader(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestWriteLongHeader_type[]{new(
        name: "name too long"u8,
        h: Ꮡ(new Header(Name: strings.Repeat("a"u8, maxSpecialFileSize)))
    ), new(
        name: "linkname too long"u8,
        h: Ꮡ(new Header(Linkname: strings.Repeat("a"u8, maxSpecialFileSize)))
    ), new(
        name: "uname too long"u8,
        h: Ꮡ(new Header(Uname: strings.Repeat("a"u8, maxSpecialFileSize)))
    ), new(
        name: "gname too long"u8,
        h: Ꮡ(new Header(Gname: strings.Repeat("a"u8, maxSpecialFileSize)))
    ), new(
        name: "PAX header too long"u8,
        h: Ꮡ(new Header(PAXRecords: new map<@string, @string>{["GOLANG.x"u8] = strings.Repeat("a"u8, maxSpecialFileSize)}))
    )
    }.slice()) {
        var w = NewWriter(io.Discard);
        {
            var err = w.WriteHeader(test.h); if (!AreEqual(err, ErrFieldTooLong)) {
                Ꮡt.Errorf("%v: w.WriteHeader() = %v, want ErrFieldTooLong"u8, test.name, err);
            }
        }
    }
}

// testNonEmptyWriter wraps an io.Writer and ensures that
// Write is never called with an empty buffer.
[GoType] internal partial struct testNonEmptyWriter {
    [GoEmbedded] public io_package.Writer Writer;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedEmptyWriteCallˢ = "unexpected empty Write call"u8;

internal static (nint, error) Write(this testNonEmptyWriter w, slice<byte> b) {
    if (len(b) == 0) {
        return (0, errors.New(unexpectedEmptyWriteCallˢ));
    }
    return w.Writer.Write(b);
}

[GoType("dyn")] [GoLocalName("testWrite")] internal partial struct TestFileWriter_testWrite {
// Write(str) == (wantCnt, wantErr)
    internal @string str;
    internal nint wantCnt;
    internal error wantErr;
}

[GoType("dyn")] [GoLocalName("testReadFrom")] internal partial struct TestFileWriter_testReadFrom {
// ReadFrom(testFile{ops}) == (wantCnt, wantErr)
    internal fileOps ops;
    internal int64 wantCnt;
    internal error wantErr;
}

[GoType("dyn")] [GoLocalName("testRemaining")] internal partial struct TestFileWriter_testRemaining {
// logicalRemaining() == wantLCnt, physicalRemaining() == wantPCnt
    internal int64 wantLCnt;
    internal int64 wantPCnt;
}

[GoType("dyn")] [GoLocalName("makeReg")] internal partial struct TestFileWriter_makeReg {
    internal int64 size;
    internal @string wantStr;
}

[GoType("dyn")] [GoLocalName("makeSparse")] internal partial struct TestFileWriter_makeSparse {
    internal TestFileWriter_makeReg makeReg;
    internal global::go.archive.tar_package.sparseHoles sph;
    internal int64 size;
}

[GoType("dyn")] internal partial struct TestFileWriter_vectors {
    internal TestFileWriter_fileMaker maker;
    internal slice<TestFileWriter_testFnc> tests;
}

public static void TestFileWriter(ж<testing.T> Ꮡt) {
    var vectors = new TestFileWriter_vectors[]{new(
        maker: new TestFileWriter_makeReg(0, ""u8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testRemaining(0, 0),
            new TestFileWriter_testWrite(""u8, 0, default!),
            new TestFileWriter_testWrite("a"u8, 0, ErrWriteTooLong),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)""u8}.slice()), 0, default!),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"a"u8}.slice()), 0, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeReg(1, "a"u8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testRemaining(1, 1),
            new TestFileWriter_testWrite(""u8, 0, default!),
            new TestFileWriter_testWrite("a"u8, 1, default!),
            new TestFileWriter_testWrite("bcde"u8, 0, ErrWriteTooLong),
            new TestFileWriter_testWrite(""u8, 0, default!),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)""u8}.slice()), 0, default!),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"a"u8}.slice()), 0, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeReg(5, "hello"u8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testRemaining(5, 5),
            new TestFileWriter_testWrite("hello"u8, 5, default!),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeReg(5, "\x00\x00\x00\x00\x00"u8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testRemaining(5, 5),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"\x00\x00\x00\x00\x00"u8}.slice()), 5, default!),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeReg(5, "\x00\x00\x00\x00\x00"u8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testRemaining(5, 5),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x65, 0x78, 0x74, 0x72, 0x61}))}.slice()), 5, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeReg(5, "abc\x00\x00"u8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testRemaining(5, 5),
            new TestFileWriter_testWrite("abc"u8, 3, default!),
            new TestFileWriter_testRemaining(2, 2),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"\x00\x00"u8}.slice()), 2, default!),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeReg(5, ((@string)(new byte[]{0x00, 0x00, 0x61, 0x62, 0x63}))),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testRemaining(5, 5),
            new TestFileWriter_testWrite("\x00\x00"u8, 2, default!),
            new TestFileWriter_testRemaining(3, 3),
            new TestFileWriter_testWrite("abc"u8, 3, default!),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"z"u8}.slice()), 0, ErrWriteTooLong),
            new TestFileWriter_testWrite("z"u8, 0, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(5, "abcde"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testRemaining(8, 5),
            new TestFileWriter_testWrite(((@string)(new byte[]{0x61, 0x62, 0x00, 0x00, 0x00, 0x63, 0x64, 0x65})), 8, default!),
            new TestFileWriter_testWrite("a"u8, 0, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(5, "abcde"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testWrite(((@string)(new byte[]{0x61, 0x62, 0x00, 0x00, 0x00, 0x63, 0x64, 0x65, 0x7a})), 8, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(5, "abcde"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testWrite("ab\x00"u8, 3, default!),
            new TestFileWriter_testRemaining(5, 3),
            new TestFileWriter_testWrite(((@string)(new byte[]{0x00, 0x00, 0x63, 0x64, 0x65})), 5, default!),
            new TestFileWriter_testWrite("a"u8, 0, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(5, "abcde"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testWrite("ab"u8, 2, default!),
            new TestFileWriter_testRemaining(6, 3),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(int64)3, (@string)"cde"u8}.slice()), 6, default!),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(5, "abcde"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"ab"u8, (int64)3, (@string)"cde"u8}.slice()), 8, default!),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(5, "abcde"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"ab"u8, (int64)3, (@string)"cdeX"u8}.slice()), 8, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(4, "abcd"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"ab"u8, (int64)3, (@string)"cd"u8}.slice()), 7, io.ErrUnexpectedEOF),
            new TestFileWriter_testRemaining(1, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(4, "abcd"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"ab"u8, (int64)3, (@string)"cde"u8}.slice()), 7, errMissData),
            new TestFileWriter_testRemaining(1, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(6, "abcde"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(@string)"ab"u8, (int64)3, (@string)"cde"u8}.slice()), 8, errUnrefData),
            new TestFileWriter_testRemaining(0, 1)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(4, "abcd"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testWrite("ab"u8, 2, default!),
            new TestFileWriter_testRemaining(6, 2),
            new TestFileWriter_testWrite("\x00\x00\x00"u8, 3, default!),
            new TestFileWriter_testRemaining(3, 2),
            new TestFileWriter_testWrite("cde"u8, 2, errMissData),
            new TestFileWriter_testRemaining(1, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(6, "abcde"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(2, 3)}.slice()), 8),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testWrite("ab"u8, 2, default!),
            new TestFileWriter_testRemaining(6, 4),
            new TestFileWriter_testWrite("\x00\x00\x00"u8, 3, default!),
            new TestFileWriter_testRemaining(3, 4),
            new TestFileWriter_testWrite("cde"u8, 3, errUnrefData),
            new TestFileWriter_testRemaining(0, 1)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(3, "abc"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testRemaining(7, 3),
            new TestFileWriter_testWrite(((@string)(new byte[]{0x00, 0x00, 0x61, 0x62, 0x63, 0x00, 0x00})), 7, default!),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(3, "abc"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testRemaining(7, 3),
            new TestFileWriter_testReadFrom(new fileOps(new any[]{(int64)2, (@string)"abc"u8, (int64)1, (@string)"\x00"u8}.slice()), 7, default!),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(3, ""u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testWrite("abcdefg"u8, 0, errWriteHole)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(3, "abc"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testWrite(((@string)(new byte[]{0x00, 0x00, 0x61, 0x62, 0x63, 0x64, 0x65})), 5, errWriteHole)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(3, "abc"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testWrite(((@string)(new byte[]{0x00, 0x00, 0x61, 0x62, 0x63, 0x00, 0x00, 0x7a})), 7, ErrWriteTooLong),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(3, "abc"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testWrite("\x00\x00"u8, 2, default!),
            new TestFileWriter_testRemaining(5, 3),
            new TestFileWriter_testWrite("abc"u8, 3, default!),
            new TestFileWriter_testRemaining(2, 0),
            new TestFileWriter_testWrite("\x00\x00"u8, 2, default!),
            new TestFileWriter_testRemaining(0, 0)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(2, "ab"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testWrite("\x00\x00"u8, 2, default!),
            new TestFileWriter_testWrite("abc"u8, 2, errMissData),
            new TestFileWriter_testWrite("\x00\x00"u8, 0, errMissData)
        }.slice()
    ), new(
        maker: new TestFileWriter_makeSparse(new TestFileWriter_makeReg(4, "abc"u8), new sparseHoles(new global::go.archive.tar_package.sparseEntry[]{new(0, 2), new(5, 2)}.slice()), 7),
        tests: new TestFileWriter_testFnc[]{
            new TestFileWriter_testWrite("\x00\x00"u8, 2, default!),
            new TestFileWriter_testWrite("abc"u8, 3, default!),
            new TestFileWriter_testWrite("\x00\x00"u8, 2, errUnrefData)
        }.slice()
    )
    }.slice();
    foreach (var (i, v) in vectors) {
        @string wantStr = default!;
        var bb = @new<strings.Builder>();
        ref var w = ref heap<testNonEmptyWriter>(out var Ꮡw);
        w = new testNonEmptyWriter(new tar_test_package.strings_BuilderжWriter(bb));
        global::go.archive.tar_package.fileWriter fw = default!;
        switch (v.maker.type()) {
        case TestFileWriter_makeReg maker: {
            fw = new global::go.archive.tar_package.regFileWriterжfileWriter(Ꮡ(new regFileWriter(w, maker.size)));
            wantStr = maker.wantStr;
            break;
        }
        case TestFileWriter_makeSparse maker: {
            if (!validateSparseEntries(maker.sph, maker.size)) {
                Ꮡt.Fatalf("invalid sparse map: %v"u8, maker.sph);
            }
            var spd = invertSparseEntries(maker.sph, maker.size);
            fw = new global::go.archive.tar_package.regFileWriterжfileWriter(Ꮡ(new regFileWriter(w, maker.makeReg.size)));
            fw = new global::go.archive.tar_package.sparseFileWriterжfileWriter(Ꮡ(new sparseFileWriter(fw, spd, 0)));
            wantStr = maker.makeReg.wantStr;
            break;
        }
        default: {
            var maker = v.maker;
            Ꮡt.Fatalf("test %d, unknown make operation: %T"u8, i, maker);
            break;
        }}
        foreach (var (j, tf) in v.tests) {
            switch (tf.type()) {
            case TestFileWriter_testWrite tfΔ1: {
                var (got, err) = fw.Write(slice<byte>(tfΔ1.str));
                if (got != tfΔ1.wantCnt || !AreEqual(err, tfΔ1.wantErr)) {
                    Ꮡt.Errorf("test %d.%d, Write(%s):\ngot  (%d, %v)\nwant (%d, %v)"u8, i, j, tfΔ1.str, got, err, tfΔ1.wantCnt, tfΔ1.wantErr);
                }
                break;
            }
            case TestFileWriter_testReadFrom tfΔ1: {
                var f = Ꮡ(new testFile(ops: tfΔ1.ops));
                var (got, err) = fw.ReadFrom(new tar_internal_test_package.testFileжReader(f));
                {
                    var (_, ok) = err._<testError>(ᐧ); if (ok){
                        Ꮡt.Errorf("test %d.%d, ReadFrom(): %v"u8, i, j, err);
                    } else 
                    if (got != tfΔ1.wantCnt || !AreEqual(err, tfΔ1.wantErr)) {
                        Ꮡt.Errorf("test %d.%d, ReadFrom() = (%d, %v), want (%d, %v)"u8, i, j, got, err, tfΔ1.wantCnt, tfΔ1.wantErr);
                    }
                }
                if (len((~f).ops) > 0) {
                    Ꮡt.Errorf("test %d.%d, expected %d more operations"u8, i, j, len((~f).ops));
                }
                break;
            }
            case TestFileWriter_testRemaining tfΔ1: {
                {
                    var got = fw.logicalRemaining(); if (got != tfΔ1.wantLCnt) {
                        Ꮡt.Errorf("test %d.%d, logicalRemaining() = %d, want %d"u8, i, j, got, tfΔ1.wantLCnt);
                    }
                }
                {
                    var got = fw.physicalRemaining(); if (got != tfΔ1.wantPCnt) {
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
        {
            @string got = bb.String(); if (got != wantStr) {
                Ꮡt.Fatalf("test %d, String() = %q, want %q"u8, i, got, wantStr);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string subfolderˢ = "subfolder"u8;

public static void TestWriterAddFS(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var fsys = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
        ["emptyfolder"u8] = Ꮡ(new fstest.MapFile(Mode: (fs.FileMode)(493 | os.ModeDir))),
        ["file.go"u8] = Ꮡ(new fstest.MapFile(Data: slice<byte>("hello"u8))),
        ["subfolder/another.go"u8] = Ꮡ(new fstest.MapFile(Data: slice<byte>("world"u8)))
    });
    // Notably missing here is the "subfolder" directory. This makes sure even
    // if we don't have a subfolder directory listed.
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    {
        var err = tw.AddFS(fsys); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = tw.Close(); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    // Add subfolder into fsys to match what we'll read from the tar.
    fsys[subfolderˢ] = Ꮡ(new fstest.MapFile(Mode: (fs.FileMode)(365 | os.ModeDir)));
    // Test that we can get the files back from the archive
    var tr = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡbuf));
    var names = new slice<@string>(0, len(fsys));
    foreach (var (name, _) in fsys) {
        names = append(names, name);
    }
    sort.Strings(names);
    nint entriesLeft = len(fsys);
    foreach (var (_, name) in names) {
        entriesLeft--;
        var (entryInfo, err) = fsys.Stat(name);
        if (err != default!) {
            Ꮡt.Fatalf("getting entry info error: %v"u8, err);
        }
        (var hdr, err) = tr.Next();
        if (AreEqual(err, io.EOF)) {
            break; // End of archive
        }
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        @string tmpName = name;
        if (entryInfo.IsDir()) {
            tmpName += "/"u8;
        }
        if ((~hdr).Name != tmpName) {
            Ꮡt.Errorf("test fs has filename %v; archive header has %v"u8,
                name, (~hdr).Name);
        }
        if (entryInfo.Mode() != hdr.FileInfo().Mode()) {
            Ꮡt.Errorf("%s: test fs has mode %v; archive header has %v"u8,
                name, entryInfo.Mode(), hdr.FileInfo().Mode());
        }
        if (entryInfo.IsDir()) {
            continue;
        }
        (var data, err) = io.ReadAll(new global::go.archive.tar_package.ReaderжReader(tr));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var origdata = fsys[name].Value.Data;
        if (((sstring)data) != ((sstring)origdata)) {
            Ꮡt.Fatalf("test fs has file content %v; archive header has %v"u8,
                data, origdata);
        }
    }
    if (entriesLeft > 0) {
        Ꮡt.Fatalf("not all entries are in the archive"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorGotNilˢ = (@string)"expected error, got nil"u8;

public static void TestWriterAddFSNonRegularFiles(ж<testing.T> Ꮡt) {
    var fsys = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{
        ["device"u8] = Ꮡ(new fstest.MapFile(Data: slice<byte>("hello"u8), Mode: (fs.FileMode)(493 | fs.ModeDevice))),
        ["symlink"u8] = Ꮡ(new fstest.MapFile(Data: slice<byte>("world"u8), Mode: (fs.FileMode)(493 | fs.ModeSymlink)))
    });
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
    {
        var err = tw.AddFS(fsys); if (err == default!) {
            Ꮡt.Fatal(expectedErrorGotNilˢ);
        }
    }
}

} // end tar_internal_test_package
