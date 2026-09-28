// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.archive;

using bytes = bytes_package;
using binary = encoding.binary_package;
using hex = encoding.hex_package;
using fmt = fmt_package;
using obscuretestdata = @internal.obscuretestdata_package;
using io = io_package;
using fs = go.io.fs_package;
using os = os_package;
using filepath = go.path.filepath_package;
using regexp = regexp_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using fstest = go.testing.fstest_package;
using time = time_package;
using @internal;
using encoding;
using go.io;
using go.path;
using go.testing;
using static go.archive.zip_package;

partial class zip_internal_test_package {

[GoType] public partial struct ZipTest {
    public @string Name;
    public Func<(io.ReaderAt r, int64 size)> Source;  // if non-nil, used instead of testdata/<Name> file
    public @string Comment;
    public slice<ZipTestFile> File;
    public bool Obscured;  // needed for Apple notarization (golang.org/issue/34986)
    public error Error; // the error that Opening this file should return
}

[GoType] public partial struct ZipTestFile {
    public @string Name;
    public fs.FileMode Mode;
    public bool NonUTF8;
    public time.Time ModTime;
    public time.Time Modified;
    // Information describing expected zip file content.
    // First, reading the entire content should produce the error ContentErr.
    // Second, if ContentErr==nil, the content should match Content.
    // If content is large, an alternative to setting Content is to set File,
    // which names a file in the testdata/ directory containing the
    // uncompressed expected content.
    // If content is very large, an alternative to setting Content or File
    // is to set Size, which will then be checked against the header-reported size
    // but will bypass the decompressing of the actual data.
    // This last option is used for testing very large (multi-GB) compressed files.
    public error ContentErr;
    public slice<byte> Content;
    public @string File;
    public uint64 Size;
}

// created in windows XP file manager.
// created by Zip 3.0 under Linux
// created by Go, before we wrote the "optional" data
// descriptor signatures (which are required by macOS).
// Use obscured file to avoid Apple’s notarization service
// rejecting the toolchain due to an inability to unzip this archive.
// See golang.org/issue/34986
// created by Go, after we wrote the "optional" data
// descriptor signatures (which are required by macOS)
// Tests that we verify (and accept valid) crc32s on files
// with crc32s in their file header (not in data descriptors)
// Tests that we verify (and reject invalid) crc32s on files
// with crc32s in their file header (not in data descriptors)
// Another zip64 file with different Extras fields. (golang.org/issue/7069)
// Largest possible non-zip64 file, with no zip64 header.
// Name is valid UTF-8, but format does not have UTF-8 flag set.
// We don't do UTF-8 detection for multi-byte runes due to
// false-positives with other encodings (e.g., Shift-JIS).
// Format says encoding is not UTF-8, so we trust it.
// Name is valid UTF-8, but format does not have UTF-8 set.
// Issue 66869: Don't skip over an EOCDR with a truncated comment.
// The test file sneakily hides a second EOCDR before the first one;
// previously we would extract one file ("file") from this archive,
// while most other tools would reject the file or extract a different one ("FILE").
internal static slice<ZipTest> tests;
internal static void initᴛtests() { tests = new ZipTest[]{
    new(
        Name: "test.zip"u8,
        Comment: "This is a zipfile comment."u8,
        File: new ZipTestFile[]{
            new(
                Name: "test.txt"u8,
                Content: slice<byte>("This is a test text file.\n"u8),
                Modified: time.Date(2010, 9, 5, 12, 12, 1, 0, timeZone((time.Duration)(36000000000000L))),
                Mode: 420
            ),
            new(
                Name: "gophercolor16x16.png"u8,
                File: "gophercolor16x16.png"u8,
                Modified: time.Date(2010, 9, 5, 15, 52, 58, 0, timeZone((time.Duration)(36000000000000L))),
                Mode: 420
            )
        }.slice()
    ),
    new(
        Name: "test-trailing-junk.zip"u8,
        Comment: "This is a zipfile comment."u8,
        File: new ZipTestFile[]{
            new(
                Name: "test.txt"u8,
                Content: slice<byte>("This is a test text file.\n"u8),
                Modified: time.Date(2010, 9, 5, 12, 12, 1, 0, timeZone((time.Duration)(36000000000000L))),
                Mode: 420
            ),
            new(
                Name: "gophercolor16x16.png"u8,
                File: "gophercolor16x16.png"u8,
                Modified: time.Date(2010, 9, 5, 15, 52, 58, 0, timeZone((time.Duration)(36000000000000L))),
                Mode: 420
            )
        }.slice()
    ),
    new(
        Name: "test-prefix.zip"u8,
        Comment: "This is a zipfile comment."u8,
        File: new ZipTestFile[]{
            new(
                Name: "test.txt"u8,
                Content: slice<byte>("This is a test text file.\n"u8),
                Modified: time.Date(2010, 9, 5, 12, 12, 1, 0, timeZone((time.Duration)(36000000000000L))),
                Mode: 420
            ),
            new(
                Name: "gophercolor16x16.png"u8,
                File: "gophercolor16x16.png"u8,
                Modified: time.Date(2010, 9, 5, 15, 52, 58, 0, timeZone((time.Duration)(36000000000000L))),
                Mode: 420
            )
        }.slice()
    ),
    new(
        Name: "test-baddirsz.zip"u8,
        Comment: "This is a zipfile comment."u8,
        File: new ZipTestFile[]{
            new(
                Name: "test.txt"u8,
                Content: slice<byte>("This is a test text file.\n"u8),
                Modified: time.Date(2010, 9, 5, 12, 12, 1, 0, timeZone((time.Duration)(36000000000000L))),
                Mode: 420
            ),
            new(
                Name: "gophercolor16x16.png"u8,
                File: "gophercolor16x16.png"u8,
                Modified: time.Date(2010, 9, 5, 15, 52, 58, 0, timeZone((time.Duration)(36000000000000L))),
                Mode: 420
            )
        }.slice()
    ),
    new(
        Name: "test-badbase.zip"u8,
        Comment: "This is a zipfile comment."u8,
        File: new ZipTestFile[]{
            new(
                Name: "test.txt"u8,
                Content: slice<byte>("This is a test text file.\n"u8),
                Modified: time.Date(2010, 9, 5, 12, 12, 1, 0, timeZone((time.Duration)(36000000000000L))),
                Mode: 420
            ),
            new(
                Name: "gophercolor16x16.png"u8,
                File: "gophercolor16x16.png"u8,
                Modified: time.Date(2010, 9, 5, 15, 52, 58, 0, timeZone((time.Duration)(36000000000000L))),
                Mode: 420
            )
        }.slice()
    ),
    new(
        Name: "r.zip"u8,
        Source: returnRecursiveZip,
        File: new ZipTestFile[]{
            new(
                Name: "r/r.zip"u8,
                Content: rZipBytes(),
                Modified: time.Date(2010, 3, 4, 0, 24, 16, 0, time.ΔUTC),
                Mode: 438
            )
        }.slice()
    ),
    new(
        Name: "symlink.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "symlink"u8,
                Content: slice<byte>("../target"u8),
                Modified: time.Date(2012, 2, 3, 19, 56, 48, 0, timeZone((time.Duration)(-7200000000000L))),
                Mode: (fs.FileMode)(511 | fs.ModeSymlink)
            )
        }.slice()
    ),
    new(
        Name: "readme.zip"u8
    ),
    new(
        Name: "readme.notzip"u8,
        Error: ErrFormat
    ),
    new(
        Name: "dd.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "filename"u8,
                Content: slice<byte>("This is a test textfile.\n"u8),
                Modified: time.Date(2011, 2, 2, 13, 6, 20, 0, time.ΔUTC),
                Mode: 438
            )
        }.slice()
    ),
    new(
        Name: "winxp.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "hello"u8,
                Content: slice<byte>("world \r\n"u8),
                Modified: time.Date(2011, 12, 8, 10, 4, 24, 0, time.ΔUTC),
                Mode: 438
            ),
            new(
                Name: "dir/bar"u8,
                Content: slice<byte>("foo \r\n"u8),
                Modified: time.Date(2011, 12, 8, 10, 4, 50, 0, time.ΔUTC),
                Mode: 438
            ),
            new(
                Name: "dir/empty/"u8,
                Content: new byte[]{}.slice(),
                Modified: time.Date(2011, 12, 8, 10, 8, 6, 0, time.ΔUTC),
                Mode: (fs.FileMode)(fs.ModeDir | 511)
            ),
            new(
                Name: "readonly"u8,
                Content: slice<byte>("important \r\n"u8),
                Modified: time.Date(2011, 12, 8, 10, 6, 8, 0, time.ΔUTC),
                Mode: 292
            )
        }.slice()
    ),
    new(
        Name: "unix.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "hello"u8,
                Content: slice<byte>("world \r\n"u8),
                Modified: time.Date(2011, 12, 8, 10, 4, 24, 0, timeZone(0)),
                Mode: 438
            ),
            new(
                Name: "dir/bar"u8,
                Content: slice<byte>("foo \r\n"u8),
                Modified: time.Date(2011, 12, 8, 10, 4, 50, 0, timeZone(0)),
                Mode: 438
            ),
            new(
                Name: "dir/empty/"u8,
                Content: new byte[]{}.slice(),
                Modified: time.Date(2011, 12, 8, 10, 8, 6, 0, timeZone(0)),
                Mode: (fs.FileMode)(fs.ModeDir | 511)
            ),
            new(
                Name: "readonly"u8,
                Content: slice<byte>("important \r\n"u8),
                Modified: time.Date(2011, 12, 8, 10, 6, 8, 0, timeZone(0)),
                Mode: 292
            )
        }.slice()
    ),
    new(
        Name: "go-no-datadesc-sig.zip.base64"u8,
        Obscured: true,
        File: new ZipTestFile[]{
            new(
                Name: "foo.txt"u8,
                Content: slice<byte>("foo\n"u8),
                Modified: time.Date(2012, 3, 8, 16, 59, 10, 0, timeZone((time.Duration)(-28800000000000L))),
                Mode: 420
            ),
            new(
                Name: "bar.txt"u8,
                Content: slice<byte>("bar\n"u8),
                Modified: time.Date(2012, 3, 8, 16, 59, 12, 0, timeZone((time.Duration)(-28800000000000L))),
                Mode: 420
            )
        }.slice()
    ),
    new(
        Name: "go-with-datadesc-sig.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "foo.txt"u8,
                Content: slice<byte>("foo\n"u8),
                Modified: time.Date(1979, 11, 30, 0, 0, 0, 0, time.ΔUTC),
                Mode: 438
            ),
            new(
                Name: "bar.txt"u8,
                Content: slice<byte>("bar\n"u8),
                Modified: time.Date(1979, 11, 30, 0, 0, 0, 0, time.ΔUTC),
                Mode: 438
            )
        }.slice()
    ),
    new(
        Name: "Bad-CRC32-in-data-descriptor"u8,
        Source: returnCorruptCRC32Zip,
        File: new ZipTestFile[]{
            new(
                Name: "foo.txt"u8,
                Content: slice<byte>("foo\n"u8),
                Modified: time.Date(1979, 11, 30, 0, 0, 0, 0, time.ΔUTC),
                Mode: 438,
                ContentErr: ErrChecksum
            ),
            new(
                Name: "bar.txt"u8,
                Content: slice<byte>("bar\n"u8),
                Modified: time.Date(1979, 11, 30, 0, 0, 0, 0, time.ΔUTC),
                Mode: 438
            )
        }.slice()
    ),
    new(
        Name: "crc32-not-streamed.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "foo.txt"u8,
                Content: slice<byte>("foo\n"u8),
                Modified: time.Date(2012, 3, 8, 16, 59, 10, 0, timeZone((time.Duration)(-28800000000000L))),
                Mode: 420
            ),
            new(
                Name: "bar.txt"u8,
                Content: slice<byte>("bar\n"u8),
                Modified: time.Date(2012, 3, 8, 16, 59, 12, 0, timeZone((time.Duration)(-28800000000000L))),
                Mode: 420
            )
        }.slice()
    ),
    new(
        Name: "crc32-not-streamed.zip"u8,
        Source: returnCorruptNotStreamedZip,
        File: new ZipTestFile[]{
            new(
                Name: "foo.txt"u8,
                Content: slice<byte>("foo\n"u8),
                Modified: time.Date(2012, 3, 8, 16, 59, 10, 0, timeZone((time.Duration)(-28800000000000L))),
                Mode: 420,
                ContentErr: ErrChecksum
            ),
            new(
                Name: "bar.txt"u8,
                Content: slice<byte>("bar\n"u8),
                Modified: time.Date(2012, 3, 8, 16, 59, 12, 0, timeZone((time.Duration)(-28800000000000L))),
                Mode: 420
            )
        }.slice()
    ),
    new(
        Name: "zip64.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "README"u8,
                Content: slice<byte>("This small file is in ZIP64 format.\n"u8),
                Modified: time.Date(2012, 8, 10, 14, 33, 32, 0, time.ΔUTC),
                Mode: 420
            )
        }.slice()
    ),
    new(
        Name: "zip64-2.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "README"u8,
                Content: slice<byte>("This small file is in ZIP64 format.\n"u8),
                Modified: time.Date(2012, 8, 10, 14, 33, 32, 0, timeZone((time.Duration)(-14400000000000L))),
                Mode: 420
            )
        }.slice()
    ),
    new(
        Name: "big.zip"u8,
        Source: returnBigZipBytes,
        File: new ZipTestFile[]{
            new(
                Name: "big.file"u8,
                Content: default!,
                Size: (uint64)(4294967296L - 1),
                Modified: time.Date(1979, 11, 30, 0, 0, 0, 0, time.ΔUTC),
                Mode: 438
            )
        }.slice()
    ),
    new(
        Name: "utf8-7zip.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "世界"u8,
                Content: new byte[]{}.slice(),
                Mode: 438,
                Modified: time.Date(2017, 11, 6, 13, 9, 27, 867862500, timeZone((time.Duration)(-28800000000000L)))
            )
        }.slice()
    ),
    new(
        Name: "utf8-infozip.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "世界"u8,
                Content: new byte[]{}.slice(),
                Mode: 420,
                NonUTF8: true,
                Modified: time.Date(2017, 11, 6, 13, 9, 27, 0, timeZone((time.Duration)(-28800000000000L)))
            )
        }.slice()
    ),
    new(
        Name: "utf8-osx.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "世界"u8,
                Content: new byte[]{}.slice(),
                Mode: 420,
                NonUTF8: true,
                Modified: time.Date(2017, 11, 6, 13, 9, 27, 0, timeZone((time.Duration)(-28800000000000L)))
            )
        }.slice()
    ),
    new(
        Name: "utf8-winrar.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "世界"u8,
                Content: new byte[]{}.slice(),
                Mode: 438,
                Modified: time.Date(2017, 11, 6, 13, 9, 27, 867862500, timeZone((time.Duration)(-28800000000000L)))
            )
        }.slice()
    ),
    new(
        Name: "utf8-winzip.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "世界"u8,
                Content: new byte[]{}.slice(),
                Mode: 438,
                Modified: time.Date(2017, 11, 6, 13, 9, 27, 867000000, timeZone((time.Duration)(-28800000000000L)))
            )
        }.slice()
    ),
    new(
        Name: "time-7zip.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "test.txt"u8,
                Content: new byte[]{}.slice(),
                Size: (uint64)(4294967296L - 1),
                Modified: time.Date(2017, 10, 31, 21, 11, 57, 244817900, timeZone((time.Duration)(-25200000000000L))),
                Mode: 438
            )
        }.slice()
    ),
    new(
        Name: "time-infozip.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "test.txt"u8,
                Content: new byte[]{}.slice(),
                Size: (uint64)(4294967296L - 1),
                Modified: time.Date(2017, 10, 31, 21, 11, 57, 0, timeZone((time.Duration)(-25200000000000L))),
                Mode: 420
            )
        }.slice()
    ),
    new(
        Name: "time-osx.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "test.txt"u8,
                Content: new byte[]{}.slice(),
                Size: (uint64)(4294967296L - 1),
                Modified: time.Date(2017, 10, 31, 21, 11, 57, 0, timeZone((time.Duration)(-25200000000000L))),
                Mode: 420
            )
        }.slice()
    ),
    new(
        Name: "time-win7.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "test.txt"u8,
                Content: new byte[]{}.slice(),
                Size: (uint64)(4294967296L - 1),
                Modified: time.Date(2017, 10, 31, 21, 11, 58, 0, time.ΔUTC),
                Mode: 438
            )
        }.slice()
    ),
    new(
        Name: "time-winrar.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "test.txt"u8,
                Content: new byte[]{}.slice(),
                Size: (uint64)(4294967296L - 1),
                Modified: time.Date(2017, 10, 31, 21, 11, 57, 244817900, timeZone((time.Duration)(-25200000000000L))),
                Mode: 438
            )
        }.slice()
    ),
    new(
        Name: "time-winzip.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "test.txt"u8,
                Content: new byte[]{}.slice(),
                Size: (uint64)(4294967296L - 1),
                Modified: time.Date(2017, 10, 31, 21, 11, 57, 244000000, timeZone((time.Duration)(-25200000000000L))),
                Mode: 438
            )
        }.slice()
    ),
    new(
        Name: "time-go.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "test.txt"u8,
                Content: new byte[]{}.slice(),
                Size: (uint64)(4294967296L - 1),
                Modified: time.Date(2017, 10, 31, 21, 11, 57, 0, timeZone((time.Duration)(-25200000000000L))),
                Mode: 438
            )
        }.slice()
    ),
    new(
        Name: "time-22738.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "file"u8,
                Content: new byte[]{}.slice(),
                Mode: 438,
                Modified: time.Date(1999, 12, 31, 19, 0, 0, 0, timeZone((time.Duration)(-18000000000000L))),
                ModTime: time.Date(1999, 12, 31, 19, 0, 0, 0, time.ΔUTC)
            )
        }.slice()
    ),
    new(
        Name: "dupdir.zip"u8,
        File: new ZipTestFile[]{
            new(
                Name: "a/"u8,
                Content: new byte[]{}.slice(),
                Mode: (fs.FileMode)(fs.ModeDir | 438),
                Modified: time.Date(2021, 12, 29, 0, 0, 0, 0, timeZone(0))
            ),
            new(
                Name: "a/b"u8,
                Content: new byte[]{}.slice(),
                Mode: 438,
                Modified: time.Date(2021, 12, 29, 0, 0, 0, 0, timeZone(0))
            ),
            new(
                Name: "a/b/"u8,
                Content: new byte[]{}.slice(),
                Mode: (fs.FileMode)(fs.ModeDir | 438),
                Modified: time.Date(2021, 12, 29, 0, 0, 0, 0, timeZone(0))
            ),
            new(
                Name: "a/b/c"u8,
                Content: new byte[]{}.slice(),
                Mode: 438,
                Modified: time.Date(2021, 12, 29, 0, 0, 0, 0, timeZone(0))
            )
        }.slice()
    ),
    new(
        Name: "comment-truncated.zip"u8,
        Error: ErrFormat
    )
}.slice(); }

public static void TestReader(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in tests) {
        ref var zt = ref heap(new ZipTest(), out var Ꮡzt);
        zt = vᴛ1;

        var ztʗ1 = zt;
        Ꮡt.Run(zt.Name, (ж<testing.T> tΔ1) => {
            readTestZip(tΔ1, ztʗ1);
        });
    }
}

internal static void readTestZip(ж<testing.T> Ꮡt, ZipTest zt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        ж<global::go.archive.zip_package.Reader> z = default!;
        error err = default!;
        slice<byte> raw = default!;
        if (zt.Source != default!){
            var (rat, size) = zt.Source();
            (z, err) = NewReader(rat, size);
            raw = new slice<byte>((nint)(size));
            {
                var (_, errΔ1) = rat.ReadAt(raw, 0); if (errΔ1 != default!) {
                    Ꮡt.Errorf("ReadAt error=%v"u8, errΔ1);
                    return;
                }
            }
        } else {
            @string path = filepath.Join(testdataˢ, zt.Name);
            if (zt.Obscured) {
                var (tf, errΔ2) = obscuretestdata.DecodeToTempFile(path);
                if (errΔ2 != default!) {
                    Ꮡt.Errorf("obscuretestdata.DecodeToTempFile(%s): %v"u8, path, errΔ2);
                    return;
                }
                defer(os.Remove, tf, ref ᒐ);
                path = tf;
            }
            ж<global::go.archive.zip_package.ReadCloser> rc = default!;
            (rc, err) = OpenReader(path);
            if (err == default!) {
                var rcʗ1 = rc;
                defer(() => rcʗ1.Close(), ref ᒐ);
                z = rc.of(global::go.archive.zip_package.ReadCloser.ᏑReader);
            }
            error err2 = default!;
            (raw, err2) = os.ReadFile(path);
            if (err2 != default!) {
                Ꮡt.Errorf("ReadFile(%s) error=%v"u8, path, err2);
                return;
            }
        }
        if (!AreEqual(err, zt.Error)) {
            Ꮡt.Errorf("error=%v, want %v"u8, err, zt.Error);
            return;
        }
        // bail if file is not zip
        if (AreEqual(err, ErrFormat)) {
            return;
        }
        // bail here if no Files expected to be tested
        // (there may actually be files in the zip, but we don't care)
        if (zt.File == default!) {
            return;
        }
        if ((~z).Comment != zt.Comment) {
            Ꮡt.Errorf("comment=%q, want %q"u8, (~z).Comment, zt.Comment);
        }
        if (len((~z).File) != len(zt.File)) {
            Ꮡt.Fatalf("file count=%d, want %d"u8, len((~z).File), len(zt.File));
        }
        // test read of each file
        foreach (var (i, ft) in zt.File) {
            readTestFile(Ꮡt, zt, ft, (~z).File[i], raw);
        }
        if (Ꮡt.Failed()) {
            return;
        }
        // test simultaneous reads
        nint n = 0;
        var done = new channel<bool>(0);
        for (nint i = 0; i < 5; i++) {
            foreach (var (j, vᴛ1) in zt.File) {
                ref var ft = ref heap(new ZipTestFile(), out var Ꮡft);
                ft = vᴛ1;

                var doneʗ1 = done;
                var rawʗ1 = raw;
                var zʗ1 = z;
                var ztʗ1 = zt;
                goǃ((nint jΔ1, ZipTestFile ftΔ1) => {
                    readTestFile(Ꮡt, ztʗ1, ftΔ1, (~zʗ1).File[jΔ1], rawʗ1);
                    doneʗ1.ᐸꟷ(true);
                }, j, ft);
                n++;
            }
        }
        for (; n > 0; n--) {
            ᐸꟷ(done);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static bool equalTimeAndZone(time.Time t1, time.Time t2) {
    var (name1, offset1) = t1.Zone();
    var (name2, offset2) = t2.Zone();
    return t1.Equal(t2) && name1 == name2 && offset1 == offset2;
}

internal static void readTestFile(ж<testing.T> Ꮡt, ZipTest zt, ZipTestFile ft, ж<global::go.archive.zip_package.File> Ꮡf, slice<byte> raw) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var f = ref Ꮡf.DerefOrNull();

    if (f.Name != ft.Name) {
        Ꮡt.Errorf("name=%q, want %q"u8, f.Name, ft.Name);
    }
    if (!ft.Modified.IsZero() && !equalTimeAndZone(f.Modified, ft.Modified)) {
        Ꮡt.Errorf("%s: Modified=%s, want %s"u8, f.Name, f.Modified, ft.Modified);
    }
    if (!ft.ModTime.IsZero() && !equalTimeAndZone(Ꮡf.of(global::go.archive.zip_package.File.ᏑFileHeader).ModTime(), ft.ModTime)) {
        Ꮡt.Errorf("%s: ModTime=%s, want %s"u8, f.Name, Ꮡf.of(global::go.archive.zip_package.File.ᏑFileHeader).ModTime(), ft.ModTime);
    }
    testFileMode(Ꮡt, Ꮡf, ft.Mode);
    var size = (uint64)f.UncompressedSize;
    if (size == uint32max){
        size = f.UncompressedSize64;
    } else 
    if (size != f.UncompressedSize64) {
        Ꮡt.Errorf("%v: UncompressedSize=%#x does not match UncompressedSize64=%#x"u8, f.Name, size, f.UncompressedSize64);
    }
    // Check that OpenRaw returns the correct byte segment
    var (rw, err) = f.OpenRaw();
    if (err != default!) {
        Ꮡt.Errorf("%v: OpenRaw error=%v"u8, f.Name, err);
        return;
    }
    (var start, err) = f.DataOffset();
    if (err != default!) {
        Ꮡt.Errorf("%v: DataOffset error=%v"u8, f.Name, err);
        return;
    }
    (var got, err) = io.ReadAll(rw);
    if (err != default!) {
        Ꮡt.Errorf("%v: OpenRaw ReadAll error=%v"u8, f.Name, err);
        return;
    }
    var end = (uint64)start + f.CompressedSize64;
    var want = raw[(int)(start)..(int)(end)];
    if (!bytes.Equal(got, want)) {
        Ꮡt.Logf("got %q"u8, got);
        Ꮡt.Logf("want %q"u8, want);
        Ꮡt.Errorf("%v: OpenRaw returned unexpected bytes"u8, f.Name);
        return;
    }
    (var r, err) = Ꮡf.Open();
    if (err != default!) {
        Ꮡt.Errorf("%v"u8, err);
        return;
    }
    // For very large files, just check that the size is correct.
    // The content is expected to be all zeros.
    // Don't bother uncompressing: too big.
    if (ft.Content == default! && ft.File == ""u8 && ft.Size > 0) {
        if (size != ft.Size) {
            Ꮡt.Errorf("%v: uncompressed size %#x, want %#x"u8, ft.Name, size, ft.Size);
        }
        r.Close();
        return;
    }
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    (_, err) = io.Copy(new zip_test_package.bytes_BufferжWriter(Ꮡb), r);
    if (!AreEqual(err, ft.ContentErr)) {
        Ꮡt.Errorf("copying contents: %v (want %v)"u8, err, ft.ContentErr);
    }
    if (err != default!) {
        return;
    }
    r.Close();
    {
        var g = (uint64)b.Len(); if (g != size) {
            Ꮡt.Errorf("%v: read %v bytes but f.UncompressedSize == %v"u8, f.Name, g, size);
        }
    }
    slice<byte> c = default!;
    if (ft.Content != default!){
        c = ft.Content;
    } else 
    {
        (c, err) = os.ReadFile("testdata/"u8 + ft.File); if (err != default!) {
            Ꮡt.Error(err);
            return;
        }
    }
    if (b.Len() != len(c)) {
        Ꮡt.Errorf("%s: len=%d, want %d"u8, f.Name, b.Len(), len(c));
        return;
    }
    foreach (var (i, bΔ1) in b.Bytes()) {
        if (bΔ1 != c[i]) {
            Ꮡt.Errorf("%s: content[%d]=%q want %q"u8, f.Name, i, bΔ1, c[i]);
            return;
        }
    }
}

internal static void testFileMode(ж<testing.T> Ꮡt, ж<global::go.archive.zip_package.File> Ꮡf, fs.FileMode want) {
    ref var f = ref Ꮡf.DerefOrNull();

    var mode = Ꮡf.of(global::go.archive.zip_package.File.ᏑFileHeader).Mode();
    if (want == 0){
        Ꮡt.Errorf("%s mode: got %v, want none"u8, f.Name, mode);
    } else 
    if (mode != want) {
        Ꮡt.Errorf("%s mode: want %v, got %v"u8, f.Name, want, mode);
    }
}

public static void TestInvalidFiles(ж<testing.T> Ꮡt) {
    UntypedInt size = /* 1024 * 70 */ 71680; // 70kb
    var b = new slice<byte>(size);
    // zeroes
    var (_, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(b)), size);
    if (!AreEqual(err, ErrFormat)) {
        Ꮡt.Errorf("zeroes: error=%v, want %v"u8, err, ErrFormat);
    }
    // repeated directoryEndSignatures
    var sig = new slice<byte>(4);
    binary.LittleEndian.PutUint32(sig, directoryEndSignature);
    for (nint i = 0; i < (nint)(size - 4); i += 4) {
        copy(b[(int)(i)..(int)(i + 4)], sig);
    }
    (_, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(b)), size);
    if (!AreEqual(err, ErrFormat)) {
        Ꮡt.Errorf("sigs: error=%v, want %v"u8, err, ErrFormat);
    }
    // negative size
    (_, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(slice<byte>("foobar"u8))), -1);
    if (err == default!) {
        Ꮡt.Errorf("archive/zip.NewReader: expected error when negative size is passed"u8);
    }
}

internal static (io.ReaderAt r, int64 size) messWith(@string fileName, Action<slice<byte>> corrupter) {
    var (data, err) = os.ReadFile(filepath.Join("testdata"u8, fileName));
    if (err != default!) {
        throw panic("Error reading " + fileName + ": " + err.Error());
    }
    corrupter(data);
    return (new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)), (int64)len(data));
}

internal static (io.ReaderAt r, int64 size) returnCorruptCRC32Zip() {
    return messWith("go-with-datadesc-sig.zip"u8, (slice<byte> b) => {
        // Corrupt one of the CRC32s in the data descriptor:
        b[0x2d]++;
    });
}

internal static (io.ReaderAt r, int64 size) returnCorruptNotStreamedZip() {
    return messWith("crc32-not-streamed.zip"u8, (slice<byte> b) => {
        // Corrupt foo.txt's final crc32 byte, in both
        // the file header and TOC. (0x7e -> 0x7f)
        b[0x11]++;
        b[0x9d]++;
    });
}

// TODO(bradfitz): add a new test that only corrupts
// one of these values, and verify that that's also an
// error. Currently, the reader code doesn't verify the
// fileheader and TOC's crc32 match if they're both
// non-zero and only the second line above, the TOC,
// is what matters.

// rZipBytes returns the bytes of a recursive zip file, without
// putting it on disk and triggering certain virus scanners.
internal static slice<byte> rZipBytes() {
    @string s = """

0000000 50 4b 03 04 14 00 00 00 08 00 08 03 64 3c f9 f4
0000010 89 64 48 01 00 00 b8 01 00 00 07 00 00 00 72 2f
0000020 72 2e 7a 69 70 00 25 00 da ff 50 4b 03 04 14 00
0000030 00 00 08 00 08 03 64 3c f9 f4 89 64 48 01 00 00
0000040 b8 01 00 00 07 00 00 00 72 2f 72 2e 7a 69 70 00
0000050 2f 00 d0 ff 00 25 00 da ff 50 4b 03 04 14 00 00
0000060 00 08 00 08 03 64 3c f9 f4 89 64 48 01 00 00 b8
0000070 01 00 00 07 00 00 00 72 2f 72 2e 7a 69 70 00 2f
0000080 00 d0 ff c2 54 8e 57 39 00 05 00 fa ff c2 54 8e
0000090 57 39 00 05 00 fa ff 00 05 00 fa ff 00 14 00 eb
00000a0 ff c2 54 8e 57 39 00 05 00 fa ff 00 05 00 fa ff
00000b0 00 14 00 eb ff 42 88 21 c4 00 00 14 00 eb ff 42
00000c0 88 21 c4 00 00 14 00 eb ff 42 88 21 c4 00 00 14
00000d0 00 eb ff 42 88 21 c4 00 00 14 00 eb ff 42 88 21
00000e0 c4 00 00 00 00 ff ff 00 00 00 ff ff 00 34 00 cb
00000f0 ff 42 88 21 c4 00 00 00 00 ff ff 00 00 00 ff ff
0000100 00 34 00 cb ff 42 e8 21 5e 0f 00 00 00 ff ff 0a
0000110 f0 66 64 12 61 c0 15 dc e8 a0 48 bf 48 af 2a b3
0000120 20 c0 9b 95 0d c4 67 04 42 53 06 06 06 40 00 06
0000130 00 f9 ff 6d 01 00 00 00 00 42 e8 21 5e 0f 00 00
0000140 00 ff ff 0a f0 66 64 12 61 c0 15 dc e8 a0 48 bf
0000150 48 af 2a b3 20 c0 9b 95 0d c4 67 04 42 53 06 06
0000160 06 40 00 06 00 f9 ff 6d 01 00 00 00 00 50 4b 01
0000170 02 14 00 14 00 00 00 08 00 08 03 64 3c f9 f4 89
0000180 64 48 01 00 00 b8 01 00 00 07 00 00 00 00 00 00
0000190 00 00 00 00 00 00 00 00 00 00 00 72 2f 72 2e 7a
00001a0 69 70 50 4b 05 06 00 00 00 00 01 00 01 00 35 00
00001b0 00 00 6d 01 00 00 00 00
"""u8;
    s = regexp.MustCompile(@"[0-9a-f]{7}"u8).ReplaceAllString(s, ""u8);
    s = regexp.MustCompile(@"\s+"u8).ReplaceAllString(s, ""u8);
    var (b, err) = hex.DecodeString(s);
    if (err != default!) {
        throw panic(err);
    }
    return b;
}

internal static (io.ReaderAt r, int64 size) returnRecursiveZip() {
    var b = rZipBytes();
    return (new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(b)), (int64)len(b));
}

// biggestZipBytes returns the bytes of a zip file biggest.zip
// that contains a zip file bigger.zip that contains a zip file
// big.zip that contains big.file, which contains 2³²-1 zeros.
// The big.zip file is interesting because it has no zip64 header,
// much like the innermost zip files in the well-known 42.zip.
//
// biggest.zip was generated by changing isZip64 to use > uint32max
// instead of >= uint32max and then running this program:
//
//	package main
//
//	import (
//		"archive/zip"
//		"bytes"
//		"io"
//		"log"
//		"os"
//	)
//
//	type zeros struct{}
//
//	func (zeros) Read(b []byte) (int, error) {
//		clear(b)
//		return len(b), nil
//	}
//
//	func main() {
//		bigZip := makeZip("big.file", io.LimitReader(zeros{}, 1<<32-1))
//		if err := os.WriteFile("/tmp/big.zip", bigZip, 0666); err != nil {
//			log.Fatal(err)
//		}
//
//		biggerZip := makeZip("big.zip", bytes.NewReader(bigZip))
//		if err := os.WriteFile("/tmp/bigger.zip", biggerZip, 0666); err != nil {
//			log.Fatal(err)
//		}
//
//		biggestZip := makeZip("bigger.zip", bytes.NewReader(biggerZip))
//		if err := os.WriteFile("/tmp/biggest.zip", biggestZip, 0666); err != nil {
//			log.Fatal(err)
//		}
//	}
//
//	func makeZip(name string, r io.Reader) []byte {
//		var buf bytes.Buffer
//		w := zip.NewWriter(&buf)
//		wf, err := w.Create(name)
//		if err != nil {
//			log.Fatal(err)
//		}
//		if _, err = io.Copy(wf, r); err != nil {
//			log.Fatal(err)
//		}
//		if err := w.Close(); err != nil {
//			log.Fatal(err)
//		}
//		return buf.Bytes()
//	}
//
// The 4 GB of zeros compresses to 4 MB, which compresses to 20 kB,
// which compresses to 1252 bytes (in the hex dump below).
//
// It's here in hex for the same reason as rZipBytes above: to avoid
// problems with on-disk virus scanners or other zip processors.
internal static slice<byte> biggestZipBytes() {
    @string s = """

0000000 50 4b 03 04 14 00 08 00 08 00 00 00 00 00 00 00
0000010 00 00 00 00 00 00 00 00 00 00 0a 00 00 00 62 69
0000020 67 67 65 72 2e 7a 69 70 ec dc 6b 4c 53 67 18 07
0000030 f0 16 c5 ca 65 2e cb b8 94 20 61 1f 44 33 c7 cd
0000040 c0 86 4a b5 c0 62 8a 61 05 c6 cd 91 b2 54 8c 1b
0000050 63 8b 03 9c 1b 95 52 5a e3 a0 19 6c b2 05 59 44
0000060 64 9d 73 83 71 11 46 61 14 b9 1d 14 09 4a c3 60
0000070 2e 4c 6e a5 60 45 02 62 81 95 b6 94 9e 9e 77 e7
0000080 d0 43 b6 f8 71 df 96 3c e7 a4 69 ce bf cf e9 79
0000090 ce ef 79 3f bf f1 31 db b6 bb 31 76 92 e7 f3 07
00000a0 8b fc 9c ca cc 08 cc cb cc 5e d2 1c 88 d9 7e bb
00000b0 4f bb 3a 3f 75 f1 5d 7f 8f c2 68 67 77 8f 25 ff
00000c0 84 e2 93 2d ef a4 95 3d 71 4e 2c b9 b0 87 c3 be
00000d0 3d f8 a7 60 24 61 c5 ef ae 9e c8 6c 6d 4e 69 c8
00000e0 67 65 34 f8 37 76 2d 76 5c 54 f3 95 65 49 c7 0f
00000f0 18 71 4b 7e 5b 6a d1 79 47 61 41 b0 4e 2a 74 45
0000100 43 58 12 b2 5a a5 c6 7d 68 55 88 d4 98 75 18 6d
0000110 08 d1 1f 8f 5a 9e 96 ee 45 cf a4 84 4e 4b e8 50
0000120 a7 13 d9 06 de 52 81 97 36 b2 d7 b8 fc 2b 5f 55
0000130 23 1f 32 59 cf 30 27 fb e2 8a b9 de 45 dd 63 9c
0000140 4b b5 8b 96 4c 7a 62 62 cc a1 a7 cf fa f1 fe dd
0000150 54 62 11 bf 36 78 b3 c7 b1 b5 f2 61 4d 4e dd 66
0000160 32 2e e6 70 34 5f f4 c9 e6 6c 43 6f da 6b c6 c3
0000170 09 2c ce 09 57 7f d2 7e b4 23 ba 7c 1b 99 bc 22
0000180 3e f1 de 91 2f e3 9c 1b 82 cc c2 84 39 aa e6 de
0000190 b4 69 fc cc cb 72 a6 61 45 f0 d3 1d 26 19 7c 8d
00001a0 29 c8 66 02 be 77 6a f9 3d 34 79 17 19 c8 96 24
00001b0 a3 ac e4 dd 3b 1a 8e c6 fe 96 38 6b bf 67 5a 23
00001c0 f4 16 f4 e6 8a b4 fc c2 cd bf 95 66 1d bb 35 aa
00001d0 92 7d 66 d8 08 8d a5 1f 54 2a af 09 cf 61 ff d2
00001e0 85 9d 8f b6 d7 88 07 4a 86 03 db 64 f3 d9 92 73
00001f0 df ec a7 fc 23 4c 8d 83 79 63 2a d9 fd 8d b3 c8
0000200 8f 7e d4 19 85 e6 8d 1c 76 f0 8b 58 32 fd 9a d6
0000210 85 e2 48 ad c3 d5 60 6f 7e 22 dd ef 09 49 7c 7f
0000220 3a 45 c3 71 b7 df f3 4c 63 fb b5 d9 31 5f 6e d6
0000230 24 1d a4 4a fe 32 a7 5c 16 48 5c 3e 08 6b 8a d3
0000240 25 1d a2 12 a5 59 24 ea 20 5f 52 6d ad 94 db 6b
0000250 94 b9 5d eb 4b a7 5c 44 bb 1e f2 3c 6b cf 52 c9
0000260 e9 e5 ba 06 b9 c4 e5 0a d0 00 0d d0 00 0d d0 00
0000270 0d d0 00 0d d0 00 0d d0 00 0d d0 00 0d d0 00 0d
0000280 d0 00 0d d0 00 0d d0 00 0d d0 00 0d d0 00 0d d0
0000290 00 0d d0 00 0d d0 00 0d d0 00 0d d0 00 0d d0 00
00002a0 0d d0 00 cd ff 9e 46 86 fa a7 7d 3a 43 d7 8e 10
00002b0 52 e9 be e6 6e cf eb 9e 85 4d 65 ce cc 30 c1 44
00002c0 c0 4e af bc 9c 6c 4b a0 d7 54 ff 1d d5 5c 89 fb
00002d0 b5 34 7e c4 c2 9e f5 a0 f6 5b 7e 6e ca 73 c7 ef
00002e0 5d be de f9 e8 81 eb a5 0a a5 63 54 2c d7 1c d1
00002f0 89 17 85 f8 16 94 f2 8a b2 a3 f5 b6 6d df 75 cd
0000300 90 dd 64 bd 5d 55 4e f2 55 19 1b b7 cc ef 1b ea
0000310 2e 05 9c f4 aa 1e a8 cd a6 82 c7 59 0f 5e 9d e0
0000320 bb fc 6c d6 99 23 eb 36 ad c6 c5 e1 d8 e1 e2 3e
0000330 d9 90 5a f7 91 5d 6f bc 33 6d 98 47 d2 7c 2e 2f
0000340 99 a4 25 72 85 49 2c be 0b 5b af 8f e5 6e 81 a6
0000350 a3 5a 6f 39 53 3a ab 7a 8b 1e 26 f7 46 6c 7d 26
0000360 53 b3 22 31 94 d3 83 f2 18 4d f5 92 33 27 53 97
0000370 0f d3 e6 55 9c a6 c5 31 87 6f d3 f3 ae 39 6f 56
0000380 10 7b ab 7e d0 b4 ca f2 b8 05 be 3f 0e 6e 5a 75
0000390 ab 0c f5 37 0e ba 8e 75 71 7a aa ed 7a dd 6a 63
00003a0 be 9b a0 97 27 6a 6f e7 d3 8b c4 7c ec d3 91 56
00003b0 d9 ac 5e bf 16 42 2f 00 1f 93 a2 23 87 bd e2 59
00003c0 a0 de 1a 66 c8 62 eb 55 8f 91 17 b4 61 42 7a 50
00003d0 40 03 34 40 03 34 40 03 34 40 03 34 40 03 34 40
00003e0 03 34 40 03 34 40 03 34 40 03 34 40 03 34 40 03
00003f0 34 40 03 34 40 03 34 ff 85 86 90 8b ea 67 90 0d
0000400 e1 42 1b d2 61 d6 79 ec fd 3e 44 28 a4 51 6c 5c
0000410 fc d2 72 ca ba 82 18 46 16 61 cd 93 a9 0f d1 24
0000420 17 99 e2 2c 71 16 84 0c c8 7a 13 0f 9a 5e c5 f0
0000430 79 64 e2 12 4d c8 82 a1 81 19 2d aa 44 6d 87 54
0000440 84 71 c1 f6 d4 ca 25 8c 77 b9 08 c7 c8 5e 10 8a
0000450 8f 61 ed 8c ba 30 1f 79 9a c7 60 34 2b b9 8c f8
0000460 18 a6 83 1b e3 9f ad 79 fe fd 1b 8b f1 fc 41 6f
0000470 d4 13 1f e3 b8 83 ba 64 92 e7 eb e4 77 05 8f ba
0000480 fa 3b 00 00 ff ff 50 4b 07 08 a6 18 b1 91 5e 04
0000490 00 00 e4 47 00 00 50 4b 01 02 14 00 14 00 08 00
00004a0 08 00 00 00 00 00 a6 18 b1 91 5e 04 00 00 e4 47
00004b0 00 00 0a 00 00 00 00 00 00 00 00 00 00 00 00 00
00004c0 00 00 00 00 62 69 67 67 65 72 2e 7a 69 70 50 4b
00004d0 05 06 00 00 00 00 01 00 01 00 38 00 00 00 96 04
00004e0 00 00 00 00
"""u8;
    s = regexp.MustCompile(@"[0-9a-f]{7}"u8).ReplaceAllString(s, ""u8);
    s = regexp.MustCompile(@"\s+"u8).ReplaceAllString(s, ""u8);
    var (b, err) = hex.DecodeString(s);
    if (err != default!) {
        throw panic(err);
    }
    return b;
}

internal static (io.ReaderAt r, int64 size) returnBigZipBytes() {
    var b = biggestZipBytes();
    for (nint i = 0; i < 2; i++) {
        var (rΔ1, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(b)), (int64)len(b));
        if (err != default!) {
            throw panic(err);
        }
        (var f, err) = (~rΔ1).File[0].Open();
        if (err != default!) {
            throw panic(err);
        }
        (b, err) = io.ReadAll(f);
        if (err != default!) {
            throw panic(err);
        }
    }
    return (new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(b)), (int64)len(b));
}

public static void TestIssue8186(ж<testing.T> Ꮡt) {
    // Directory headers & data found in the TOC of a JAR file.
    var dirEnts = new @string[]{
        ((@string)(new byte[]{0x50, 0x4b, 0x01, 0x02, 0x0a, 0x00, 0x0a, 0x00, 0x00, 0x08, 0x00, 0x00, 0x34, 0x9d, 0x33, 0x3f, 0xaa, 0x1b, 0x06, 0xf0, 0x81, 0x02, 0x00, 0x00, 0x81, 0x02, 0x00, 0x00, 0x2d, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x72, 0x65, 0x73, 0x2f, 0x64, 0x72, 0x61, 0x77, 0x61, 0x62, 0x6c, 0x65, 0x2d, 0x78, 0x68, 0x64, 0x70, 0x69, 0x2d, 0x76, 0x34, 0x2f, 0x69, 0x63, 0x5f, 0x61, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x62, 0x61, 0x72, 0x5f, 0x61, 0x63, 0x63, 0x65, 0x70, 0x74, 0x2e, 0x70, 0x6e, 0x67, 0xfe, 0xca, 0x00, 0x00, 0x00})),
        ((@string)(new byte[]{0x50, 0x4b, 0x01, 0x02, 0x0a, 0x00, 0x0a, 0x00, 0x00, 0x08, 0x00, 0x00, 0x34, 0x9d, 0x33, 0x3f, 0x90, 0x4b, 0x89, 0xc7, 0x74, 0x0a, 0x00, 0x00, 0x74, 0x0a, 0x00, 0x00, 0x0e, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xd1, 0x02, 0x00, 0x00, 0x72, 0x65, 0x73, 0x6f, 0x75, 0x72, 0x63, 0x65, 0x73, 0x2e, 0x61, 0x72, 0x73, 0x63, 0x00, 0x00, 0x00})),
        ((@string)(new byte[]{0x50, 0x4b, 0x01, 0x02, 0x14, 0x00, 0x14, 0x00, 0x08, 0x08, 0x08, 0x00, 0x34, 0x9d, 0x33, 0x3f, 0xff, 0x24, 0x18, 0xed, 0x33, 0x03, 0x00, 0x00, 0xb4, 0x08, 0x00, 0x00, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x74, 0x0d, 0x00, 0x00, 0x41, 0x6e, 0x64, 0x72, 0x6f, 0x69, 0x64, 0x4d, 0x61, 0x6e, 0x69, 0x66, 0x65, 0x73, 0x74, 0x2e, 0x78, 0x6d, 0x6c})),
        ((@string)(new byte[]{0x50, 0x4b, 0x01, 0x02, 0x14, 0x00, 0x14, 0x00, 0x08, 0x08, 0x08, 0x00, 0x34, 0x9d, 0x33, 0x3f, 0x14, 0xc5, 0x4b, 0xab, 0x19, 0x32, 0x02, 0x00, 0xc8, 0xcd, 0x04, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xe8, 0x10, 0x00, 0x00, 0x63, 0x6c, 0x61, 0x73, 0x73, 0x65, 0x73, 0x2e, 0x64, 0x65, 0x78})),
        ((@string)(new byte[]{0x50, 0x4b, 0x01, 0x02, 0x14, 0x00, 0x14, 0x00, 0x08, 0x08, 0x08, 0x00, 0x34, 0x9d, 0x33, 0x3f, 0x45, 0x96, 0x0a, 0x44, 0xac, 0x01, 0x00, 0x00, 0x50, 0x03, 0x00, 0x00, 0x26, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3a, 0x43, 0x02, 0x00, 0x72, 0x65, 0x73, 0x2f, 0x6c, 0x61, 0x79, 0x6f, 0x75, 0x74, 0x2f, 0x61, 0x63, 0x74, 0x69, 0x6f, 0x6e, 0x62, 0x61, 0x72, 0x5f, 0x73, 0x65, 0x74, 0x5f, 0x77, 0x61, 0x6c, 0x6c, 0x70, 0x61, 0x70, 0x65, 0x72, 0x2e, 0x78, 0x6d, 0x6c})),
        ((@string)(new byte[]{0x50, 0x4b, 0x01, 0x02, 0x14, 0x00, 0x14, 0x00, 0x08, 0x08, 0x08, 0x00, 0x34, 0x9d, 0x33, 0x3f, 0xc4, 0xbb, 0x14, 0xe3, 0xd8, 0x01, 0x00, 0x00, 0xd8, 0x03, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3a, 0x45, 0x02, 0x00, 0x72, 0x65, 0x73, 0x2f, 0x6c, 0x61, 0x79, 0x6f, 0x75, 0x74, 0x2f, 0x77, 0x61, 0x6c, 0x6c, 0x70, 0x61, 0x70, 0x65, 0x72, 0x5f, 0x63, 0x72, 0x6f, 0x70, 0x70, 0x65, 0x72, 0x2e, 0x78, 0x6d, 0x6c})),
        ((@string)(new byte[]{0x50, 0x4b, 0x01, 0x02, 0x14, 0x00, 0x14, 0x00, 0x08, 0x08, 0x08, 0x00, 0x34, 0x9d, 0x33, 0x3f, 0x7d, 0xc1, 0x15, 0x9e, 0x5a, 0x01, 0x00, 0x00, 0x21, 0x02, 0x00, 0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x60, 0x47, 0x02, 0x00, 0x4d, 0x45, 0x54, 0x41, 0x2d, 0x49, 0x4e, 0x46, 0x2f, 0x4d, 0x41, 0x4e, 0x49, 0x46, 0x45, 0x53, 0x54, 0x2e, 0x4d, 0x46})),
        ((@string)(new byte[]{0x50, 0x4b, 0x01, 0x02, 0x14, 0x00, 0x14, 0x00, 0x08, 0x08, 0x08, 0x00, 0x34, 0x9d, 0x33, 0x3f, 0xe6, 0x98, 0xd0, 0xac, 0x6f, 0x01, 0x00, 0x00, 0x84, 0x02, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xfc, 0x48, 0x02, 0x00, 0x4d, 0x45, 0x54, 0x41, 0x2d, 0x49, 0x4e, 0x46, 0x2f, 0x43, 0x45, 0x52, 0x54, 0x2e, 0x53, 0x46})),
        ((@string)(new byte[]{0x50, 0x4b, 0x01, 0x02, 0x14, 0x00, 0x14, 0x00, 0x08, 0x08, 0x08, 0x00, 0x34, 0x9d, 0x33, 0x3f, 0xbf, 0x50, 0x96, 0x62, 0x86, 0x04, 0x00, 0x00, 0xb2, 0x06, 0x00, 0x00, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xa9, 0x4a, 0x02, 0x00, 0x4d, 0x45, 0x54, 0x41, 0x2d, 0x49, 0x4e, 0x46, 0x2f, 0x43, 0x45, 0x52, 0x54, 0x2e, 0x52, 0x53, 0x41}))
    }.slice();
    foreach (var (i, s) in dirEnts) {
        ref var f = ref heap(new global::go.archive.zip_package.File(), out var Ꮡf);
        var err = readDirectoryHeader(ref f, new zip_test_package.strings_ReaderжReader(strings.NewReader(s)));
        if (err != default!) {
            Ꮡt.Errorf("error reading #%d: %v"u8, i, err);
        }
    }
}

// Verify we return ErrUnexpectedEOF when length is short.
public static void TestIssue10957(ж<testing.T> Ꮡt) {
    var data = slice<byte>(((@string)(new byte[]{0x50, 0x4b, 0x03, 0x04, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x50, 0x4b, 0x01, 0x02, 0x30, 0x30, 0x30, 0x30, 0x30})) + "0000000000000000000\x00" + ((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x50, 0x4b, 0x01})) + ((@string)(new byte[]{0x02, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x30, 0x30, 0x30, 0x30, 0x30, 0x0b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x50, 0x4b, 0x01, 0x02, 0x30, 0x30})) + "00000000000000000000" + ((@string)(new byte[]{0x30, 0x30, 0x0b, 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x50, 0x4b, 0x01, 0x02, 0x30, 0x30, 0x30, 0x30, 0x3c})) + ((@string)(new byte[]{0x30, 0x00, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x0b, 0x00, 0x0b})) + ((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x00, 0x00, 0x00, 0x00, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x50, 0x4b, 0x01, 0x02, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + "0000000000000000\v\x00\x00\x00" + ((@string)(new byte[]{0x00, 0x00, 0x30, 0x30, 0x50, 0x4b, 0x05, 0x06, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x05, 0x00, 0xfd, 0x00, 0x00, 0x00})) + "\v\x00\x00\x00\x00\x00");
    var (z, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)), (int64)len(data));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (i, f) in (~z).File) {
        var (r, errΔ1) = f.Open();
        if (errΔ1 != default!) {
            continue;
        }
        if ((~f).UncompressedSize64 < 1000000) {
            var (n, errΔ2) = io.Copy(io.Discard, r);
            if (i == 3 && !AreEqual(errΔ2, io.ErrUnexpectedEOF)) {
                Ꮡt.Errorf("File[3] error = %v; want io.ErrUnexpectedEOF"u8, errΔ2);
            }
            if (errΔ2 == default! && (uint64)n != (~f).UncompressedSize64) {
                Ꮡt.Errorf("file %d: bad size: copied=%d; want=%d"u8, i, n, (~f).UncompressedSize64);
            }
        }
        r.Close();
    }
}

// Verify that this particular malformed zip file is rejected.
public static void TestIssue10956(ж<testing.T> Ꮡt) {
    var data = slice<byte>("PK\x06\x06PK\x06\a0000\x00\x00\x00\x00\x00\x00\x00\x00" + ((@string)(new byte[]{0x30, 0x30, 0x30, 0x30, 0x50, 0x4b, 0x05, 0x06, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x30, 0x30, 0x30, 0x30, 0x0b, 0x00, 0x30, 0x30, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x30})));
    var (r, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)), (int64)len(data));
    if (err == default!) {
        Ꮡt.Errorf("got nil error, want ErrFormat"u8);
    }
    if (r != nil) {
        Ꮡt.Errorf("got non-nil Reader, want nil"u8);
    }
}

// Verify we return ErrUnexpectedEOF when reading truncated data descriptor.
public static void TestIssue11146(ж<testing.T> Ꮡt) {
    var data = slice<byte>(((@string)(new byte[]{0x50, 0x4b, 0x03, 0x04, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x01, 0x00, 0x00, 0x00, 0x30, 0x01, 0x00, 0x00, 0xff, 0xff, 0x30, 0x30, 0x30, 0x30})) + "0000000000000000PK\x01\x02" + ((@string)(new byte[]{0x30, 0x30, 0x30, 0x30, 0x08, 0x30, 0x08, 0x00, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30})) + ((@string)(new byte[]{0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x30, 0x30, 0x30, 0x50, 0x4b, 0x05, 0x06, 0x00, 0x00})) + ((@string)(new byte[]{0x00, 0x00, 0x30, 0x30, 0x01, 0x00, 0x26, 0x00, 0x00, 0x00, 0x38, 0x00, 0x00, 0x00, 0x00, 0x00})));
    var (z, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)), (int64)len(data));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var r, err) = (~z).File[0].Open();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (_, err) = io.ReadAll(r);
    if (!AreEqual(err, io.ErrUnexpectedEOF)) {
        Ꮡt.Errorf("File[0] error = %v; want io.ErrUnexpectedEOF"u8, err);
    }
    r.Close();
}

// Verify we do not treat non-zip64 archives as zip64
public static void TestIssue12449(ж<testing.T> Ꮡt) {
    var data = new byte[]{
        0x50, 0x4b, 0x03, 0x04, 0x14, 0x00, 0x08, 0x00,
        0x00, 0x00, 0x6b, 0xb4, 0xba, 0x46, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x03, 0x00, 0x18, 0x00, 0xca, 0x64,
        0x55, 0x75, 0x78, 0x0b, 0x00, 0x50, 0x4b, 0x05,
        0x06, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01,
        0x00, 0x49, 0x00, 0x00, 0x00, 0x44, 0x00, 0x00,
        0x00, 0x31, 0x31, 0x31, 0x32, 0x32, 0x32, 0x0a,
        0x50, 0x4b, 0x07, 0x08, 0x1d, 0x88, 0x77, 0xb0,
        0x07, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00,
        0x50, 0x4b, 0x01, 0x02, 0x14, 0x03, 0x14, 0x00,
        0x08, 0x00, 0x00, 0x00, 0x6b, 0xb4, 0xba, 0x46,
        0x1d, 0x88, 0x77, 0xb0, 0x07, 0x00, 0x00, 0x00,
        0x07, 0x00, 0x00, 0x00, 0x03, 0x00, 0x18, 0x00,
        0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xa0, 0x81, 0x00, 0x00, 0x00, 0x00, 0xca, 0x64,
        0x55, 0x75, 0x78, 0x0b, 0x00, 0x50, 0x4b, 0x05,
        0x06, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01,
        0x00, 0x49, 0x00, 0x00, 0x00, 0x44, 0x00, 0x00,
        0x00, 0x97, 0x2b, 0x49, 0x23, 0x05, 0xc5, 0x0b,
        0xa7, 0xd1, 0x52, 0xa2, 0x9c, 0x50, 0x4b, 0x06,
        0x07, 0xc8, 0x19, 0xc1, 0xaf, 0x94, 0x9c, 0x61,
        0x44, 0xbe, 0x94, 0x19, 0x42, 0x58, 0x12, 0xc6,
        0x5b, 0x50, 0x4b, 0x05, 0x06, 0x00, 0x00, 0x00,
        0x00, 0x01, 0x00, 0x01, 0x00, 0x69, 0x00, 0x00,
        0x00, 0x50, 0x00, 0x00, 0x00, 0x00, 0x00
    }.slice();
    // Read in the archive.
    var (_, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)), (int64)len(data));
    if (err != default!) {
        Ꮡt.Errorf("Error reading the archive: %v"u8, err);
    }
}

[GoType("dyn")] internal partial struct TestFS_type {
    internal @string @file;
    internal slice<@string> want;
}

public static void TestFS(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFS_type[]{
        new(
            "testdata/unix.zip"u8,
            new @string[]{"hello"u8, "dir/bar"u8, "readonly"u8}.slice()
        ),
        new(
            "testdata/subdir.zip"u8,
            new @string[]{"a/b/c"u8}.slice()
        )
    }.slice()) {
        ref var testΔ1 = ref heap<TestFS_type>(out var ᏑtestΔ1);
        testΔ1 = test;
        var testʗ1 = testΔ1;
        Ꮡt.Run(testΔ1.@file, (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                tΔ1.Parallel();
                var (z, err) = OpenReader(testʗ1.@file);
                if (err != default!) {
                    tΔ1.Fatal(err);
                }
                var zʗ1 = z;
                defer(() => zʗ1.Close(), ref ᒐ);
                {
                    var errΔ1 = fstest.TestFS(new zip_test_package.zip_ReadCloserжFS(z), testʗ1.want.ꓸꓸꓸ); if (errΔ1 != default!) {
                        tΔ1.Error(errΔ1);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object succeededButWantErrorˢ = (@string)"succeeded but want error"u8;
internal static readonly object unexpectedErrorˢ = (@string)"unexpected error"u8;

[GoType("dyn")] internal partial struct TestFSWalk_type {
    internal @string @file;
    internal slice<@string> want;
    internal bool wantErr;
}

public static void TestFSWalk(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFSWalk_type[]{
        new(
            @file: "testdata/unix.zip"u8,
            want: new @string[]{"."u8, "dir"u8, "dir/bar"u8, "dir/empty"u8, "hello"u8, "readonly"u8}.slice()
        ),
        new(
            @file: "testdata/subdir.zip"u8,
            want: new @string[]{"."u8, "a"u8, "a/b"u8, "a/b/c"u8}.slice()
        ),
        new(
            @file: "testdata/dupdir.zip"u8,
            wantErr: true
        )
    }.slice()) {
        ref var testΔ1 = ref heap<TestFSWalk_type>(out var ᏑtestΔ1);
        testΔ1 = test;
        var testʗ1 = testΔ1;
        Ꮡt.Run(testΔ1.@file, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            var (z, err) = OpenReader(testʗ1.@file);
            if (err != default!) {
                tΔ1.Fatal(err);
            }
            ref var files = ref heap<slice<@string>>(out var Ꮡfiles);
            var sawErr = false;
            var testʗ2 = testʗ1;
            err = fs.WalkDir(new zip_test_package.zip_ReadCloserжFS(z), "."u8, (@string path, fs.DirEntry d, error errΔ1) => {
                if (errΔ1 != default!) {
                    if (!testʗ2.wantErr) {
                        tΔ1.Errorf("%s: %v"u8, path, errΔ1);
                    }
                    sawErr = true;
                    return default!;
                }
                Ꮡfiles.ValueSlot = append(Ꮡfiles.ValueSlot, path);
                return default!;
            });
            if (err != default!) {
                tΔ1.Errorf("fs.WalkDir error: %v"u8, err);
            }
            if (testʗ1.wantErr && !sawErr){
                tΔ1.Error(succeededButWantErrorˢ);
            } else 
            if (!testʗ1.wantErr && sawErr) {
                tΔ1.Error(unexpectedErrorˢ);
            }
            if (testʗ1.want != default! && !slices.Equal<slice<@string>, @string>(Ꮡfiles.ValueSlot, testʗ1.want)) {
                tΔ1.Errorf("got %v want %v"u8, Ꮡfiles.ValueSlot, testʗ1.want);
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataSubdirZipˢ = "testdata/subdir.zip"u8;

[GoType("dyn")] internal partial struct TestFSModTime_type {
    internal @string name;
    internal time.Time want;
}

public static void TestFSModTime(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var (z, err) = OpenReader(testdataSubdirZipˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var zʗ1 = z;
        defer(() => zʗ1.Close(), ref ᒐ);
        foreach (var (_, test) in new TestFSModTime_type[]{
            new(
                "a"u8,
                time.Date(2021, 4, 19, 12, 29, 56, 0, timeZone((time.Duration)(-25200000000000L))).UTC()
            ),
            new(
                "a/b/c"u8,
                time.Date(2021, 4, 19, 12, 29, 59, 0, timeZone((time.Duration)(-25200000000000L))).UTC()
            )
        }.slice()) {
            var (fi, errΔ1) = fs.Stat(new zip_test_package.zip_ReadCloserжFS(z), test.name);
            if (errΔ1 != default!) {
                Ꮡt.Errorf("%s: %v"u8, test.name, errΔ1);
                continue;
            }
            {
                var got = fi.ModTime(); if (!got.Equal(test.want)) {
                    Ꮡt.Errorf("%s: got modtime %v, want %v"u8, test.name, got, test.want);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string godebugˢ = "GODEBUG"u8;
internal static readonly @string zipinsecurepath0ˢ = "zipinsecurepath=0"u8;
internal static readonly @string testTxtˢ = "test.txt"u8;

public static void TestCVE202127919(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, zipinsecurepath0ˢ);
    // Archive containing only the file "../test.txt"
    var data = new byte[]{
        0x50, 0x4b, 0x03, 0x04, 0x14, 0x00, 0x08, 0x00,
        0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x2e, 0x2e,
        0x2f, 0x74, 0x65, 0x73, 0x74, 0x2e, 0x74, 0x78,
        0x74, 0x0a, 0xc9, 0xc8, 0x2c, 0x56, 0xc8, 0x2c,
        0x56, 0x48, 0x54, 0x28, 0x49, 0x2d, 0x2e, 0x51,
        0x28, 0x49, 0xad, 0x28, 0x51, 0x48, 0xcb, 0xcc,
        0x49, 0xd5, 0xe3, 0x02, 0x04, 0x00, 0x00, 0xff,
        0xff, 0x50, 0x4b, 0x07, 0x08, 0xc0, 0xd7, 0xed,
        0xc3, 0x20, 0x00, 0x00, 0x00, 0x1a, 0x00, 0x00,
        0x00, 0x50, 0x4b, 0x01, 0x02, 0x14, 0x00, 0x14,
        0x00, 0x08, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00,
        0x00, 0xc0, 0xd7, 0xed, 0xc3, 0x20, 0x00, 0x00,
        0x00, 0x1a, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2e,
        0x2e, 0x2f, 0x74, 0x65, 0x73, 0x74, 0x2e, 0x74,
        0x78, 0x74, 0x50, 0x4b, 0x05, 0x06, 0x00, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x39, 0x00,
        0x00, 0x00, 0x59, 0x00, 0x00, 0x00, 0x00, 0x00
    }.slice();
    var (r, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)), (int64)len(data));
    if (!AreEqual(err, ErrInsecurePath)) {
        Ꮡt.Fatalf("Error reading the archive: %v"u8, err);
    }
    (_, err) = r.Open(testTxtˢ);
    if (err != default!) {
        Ꮡt.Errorf("Error reading file: %v"u8, err);
    }
    if (len((~r).File) != 1) {
        Ꮡt.Fatalf("No entries in the file list"u8);
    }
    if ((~(~r).File[0]).Name != "../test.txt"u8) {
        Ꮡt.Errorf("Unexpected entry name: %s"u8, (~(~r).File[0]).Name);
    }
    {
        var (_, errΔ1) = (~r).File[0].Open(); if (errΔ1 != default!) {
            Ꮡt.Errorf("Error opening file: %v"u8, errΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testZipˢ = "test.zip"u8;

public static void TestOpenReaderInsecurePath(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Setenv(godebugˢ, zipinsecurepath0ˢ);
        // Archive containing only the file "../test.txt"
        var data = new byte[]{
            0x50, 0x4b, 0x03, 0x04, 0x14, 0x00, 0x08, 0x00,
            0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x0b, 0x00, 0x00, 0x00, 0x2e, 0x2e,
            0x2f, 0x74, 0x65, 0x73, 0x74, 0x2e, 0x74, 0x78,
            0x74, 0x0a, 0xc9, 0xc8, 0x2c, 0x56, 0xc8, 0x2c,
            0x56, 0x48, 0x54, 0x28, 0x49, 0x2d, 0x2e, 0x51,
            0x28, 0x49, 0xad, 0x28, 0x51, 0x48, 0xcb, 0xcc,
            0x49, 0xd5, 0xe3, 0x02, 0x04, 0x00, 0x00, 0xff,
            0xff, 0x50, 0x4b, 0x07, 0x08, 0xc0, 0xd7, 0xed,
            0xc3, 0x20, 0x00, 0x00, 0x00, 0x1a, 0x00, 0x00,
            0x00, 0x50, 0x4b, 0x01, 0x02, 0x14, 0x00, 0x14,
            0x00, 0x08, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00,
            0x00, 0xc0, 0xd7, 0xed, 0xc3, 0x20, 0x00, 0x00,
            0x00, 0x1a, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2e,
            0x2e, 0x2f, 0x74, 0x65, 0x73, 0x74, 0x2e, 0x74,
            0x78, 0x74, 0x50, 0x4b, 0x05, 0x06, 0x00, 0x00,
            0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x39, 0x00,
            0x00, 0x00, 0x59, 0x00, 0x00, 0x00, 0x00, 0x00
        }.slice();
        // Read in the archive with the OpenReader interface
        @string name = filepath.Join(Ꮡt.TempDir(), testZipˢ);
        var err = os.WriteFile(name, data, 420);
        if (err != default!) {
            Ꮡt.Fatalf("Unable to write out the bugos zip entry"u8);
        }
        (var r, err) = OpenReader(name);
        if (r != nil) {
            var rʗ1 = r;
            defer(() => rʗ1.Close(), ref ᒐ);
        }
        if (!AreEqual(err, ErrInsecurePath)) {
            Ꮡt.Fatalf("Error reading the archive, we expected ErrInsecurePath but got: %v"u8, err);
        }
        (_, err) = r.of(global::go.archive.zip_package.ReadCloser.ᏑReader).Open(testTxtˢ);
        if (err != default!) {
            Ꮡt.Errorf("Error reading file: %v"u8, err);
        }
        if (len((~r).File) != 1) {
            Ꮡt.Fatalf("No entries in the file list"u8);
        }
        if ((~(~r).File[0]).Name != "../test.txt"u8) {
            Ꮡt.Errorf("Unexpected entry name: %s"u8, (~(~r).File[0]).Name);
        }
        {
            var (_, errΔ1) = (~r).File[0].Open(); if (errΔ1 != default!) {
                Ꮡt.Errorf("Error opening file: %v"u8, errΔ1);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestCVE202133196(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Archive that indicates it has 1 << 128 -1 files,
    // this would previously cause a panic due to attempting
    // to allocate a slice with 1 << 128 -1 elements.
    var data = new byte[]{
        0x50, 0x4b, 0x03, 0x04, 0x14, 0x00, 0x08, 0x08,
        0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, 0x02,
        0x03, 0x62, 0x61, 0x65, 0x03, 0x04, 0x00, 0x00,
        0xff, 0xff, 0x50, 0x4b, 0x07, 0x08, 0xbe, 0x20,
        0x5c, 0x6c, 0x09, 0x00, 0x00, 0x00, 0x03, 0x00,
        0x00, 0x00, 0x50, 0x4b, 0x01, 0x02, 0x14, 0x00,
        0x14, 0x00, 0x08, 0x08, 0x08, 0x00, 0x00, 0x00,
        0x00, 0x00, 0xbe, 0x20, 0x5c, 0x6c, 0x09, 0x00,
        0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x03, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x01, 0x02, 0x03, 0x50, 0x4b, 0x06, 0x06, 0x2c,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2d,
        0x00, 0x2d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0x31, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x3a, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x50, 0x4b, 0x06, 0x07, 0x00,
        0x00, 0x00, 0x00, 0x6b, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x50,
        0x4b, 0x05, 0x06, 0x00, 0x00, 0x00, 0x00, 0xff,
        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        0xff, 0xff, 0xff, 0x00, 0x00
    }.slice();
    var (_, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)), (int64)len(data));
    if (!AreEqual(err, ErrFormat)) {
        Ꮡt.Fatalf("unexpected error, got: %v, want: %v"u8, err, ErrFormat);
    }
    // Also check that an archive containing a handful of empty
    // files doesn't cause an issue
    var b = bytes.NewBuffer(default!);
    var w = NewWriter(new zip_test_package.bytes_BufferжWriter(b));
    for (nint i = 0; i < 5; i++) {
        var (_, errΔ1) = w.Create(""u8);
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("Writer.Create failed: %s"u8, errΔ1);
        }
    }
    {
        var errΔ2 = w.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatalf("Writer.Close failed: %s"u8, errΔ2);
        }
    }
    (var r, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(b.Bytes())), (int64)b.Len());
    if (err != default!) {
        Ꮡt.Fatalf("NewReader failed: %s"u8, err);
    }
    if (len((~r).File) != 5) {
        Ꮡt.Errorf("Archive has unexpected number of files, got %d, want 5"u8, len((~r).File));
    }
}

public static void TestCVE202139293(ж<testing.T> Ꮡt) {
    // directory size is so large, that the check in Reader.init
    // overflows when subtracting from the archive size, causing
    // the pre-allocation check to be bypassed.
    var data = new byte[]{
        0x50, 0x4b, 0x06, 0x06, 0x05, 0x06, 0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x50, 0x4b,
        0x06, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
        0x00, 0x00, 0x50, 0x4b, 0x05, 0x06, 0x00, 0x1a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x50, 0x4b,
        0x06, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
        0x00, 0x00, 0x00, 0x50, 0x4b, 0x05, 0x06, 0x00, 0x31, 0x00, 0x00, 0x00, 0x00, 0xff, 0xff,
        0xff, 0x50, 0xfe, 0x00, 0xff, 0x00, 0x3a, 0x00, 0x00, 0x00, 0xff
    }.slice();
    var (_, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)), (int64)len(data));
    if (!AreEqual(err, ErrFormat)) {
        Ꮡt.Fatalf("unexpected error, got: %v, want: %v"u8, err, ErrFormat);
    }
}

public static void TestCVE202141772(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, zipinsecurepath0ˢ);
    // Archive contains a file whose name is exclusively made up of '/', '\'
    // characters, or "../", "..\" paths, which would previously cause a panic.
    //
    //  Length   Method    Size  Cmpr    Date    Time   CRC-32   Name
    // --------  ------  ------- ---- ---------- ----- --------  ----
    //        0  Stored        0   0% 08-05-2021 18:32 00000000  /
    //        0  Stored        0   0% 09-14-2021 12:59 00000000  //
    //        0  Stored        0   0% 09-14-2021 12:59 00000000  \
    //       11  Stored       11   0% 09-14-2021 13:04 0d4a1185  /test.txt
    // --------          -------  ---                            -------
    //       11               11   0%                            4 files
    var data = new byte[]{
        0x50, 0x4b, 0x03, 0x04, 0x0a, 0x00, 0x00, 0x08,
        0x00, 0x00, 0x06, 0x94, 0x05, 0x53, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x2f, 0x50,
        0x4b, 0x03, 0x04, 0x0a, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x78, 0x67, 0x2e, 0x53, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x02, 0x00, 0x00, 0x00, 0x2f, 0x2f, 0x50,
        0x4b, 0x03, 0x04, 0x0a, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x78, 0x67, 0x2e, 0x53, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x01, 0x00, 0x00, 0x00, 0x5c, 0x50, 0x4b,
        0x03, 0x04, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x91, 0x68, 0x2e, 0x53, 0x85, 0x11, 0x4a, 0x0d,
        0x0b, 0x00, 0x00, 0x00, 0x0b, 0x00, 0x00, 0x00,
        0x09, 0x00, 0x00, 0x00, 0x2f, 0x74, 0x65, 0x73,
        0x74, 0x2e, 0x74, 0x78, 0x74, 0x68, 0x65, 0x6c,
        0x6c, 0x6f, 0x20, 0x77, 0x6f, 0x72, 0x6c, 0x64,
        0x50, 0x4b, 0x01, 0x02, 0x14, 0x03, 0x0a, 0x00,
        0x00, 0x08, 0x00, 0x00, 0x06, 0x94, 0x05, 0x53,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00,
        0xed, 0x41, 0x00, 0x00, 0x00, 0x00, 0x2f, 0x50,
        0x4b, 0x01, 0x02, 0x3f, 0x00, 0x0a, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x78, 0x67, 0x2e, 0x53, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x02, 0x00, 0x24, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00,
        0x00, 0x1f, 0x00, 0x00, 0x00, 0x2f, 0x2f, 0x0a,
        0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
        0x00, 0x18, 0x00, 0x93, 0x98, 0x25, 0x57, 0x25,
        0xa9, 0xd7, 0x01, 0x93, 0x98, 0x25, 0x57, 0x25,
        0xa9, 0xd7, 0x01, 0x93, 0x98, 0x25, 0x57, 0x25,
        0xa9, 0xd7, 0x01, 0x50, 0x4b, 0x01, 0x02, 0x3f,
        0x00, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x78,
        0x67, 0x2e, 0x53, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01,
        0x00, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x20, 0x00, 0x00, 0x00, 0x3f, 0x00, 0x00,
        0x00, 0x5c, 0x0a, 0x00, 0x20, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x93, 0x98,
        0x25, 0x57, 0x25, 0xa9, 0xd7, 0x01, 0x93, 0x98,
        0x25, 0x57, 0x25, 0xa9, 0xd7, 0x01, 0x93, 0x98,
        0x25, 0x57, 0x25, 0xa9, 0xd7, 0x01, 0x50, 0x4b,
        0x01, 0x02, 0x3f, 0x00, 0x0a, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x91, 0x68, 0x2e, 0x53, 0x85, 0x11,
        0x4a, 0x0d, 0x0b, 0x00, 0x00, 0x00, 0x0b, 0x00,
        0x00, 0x00, 0x09, 0x00, 0x24, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00,
        0x5e, 0x00, 0x00, 0x00, 0x2f, 0x74, 0x65, 0x73,
        0x74, 0x2e, 0x74, 0x78, 0x74, 0x0a, 0x00, 0x20,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x18,
        0x00, 0xa9, 0x80, 0x51, 0x01, 0x26, 0xa9, 0xd7,
        0x01, 0x31, 0xd1, 0x57, 0x01, 0x26, 0xa9, 0xd7,
        0x01, 0xdf, 0x48, 0x85, 0xf9, 0x25, 0xa9, 0xd7,
        0x01, 0x50, 0x4b, 0x05, 0x06, 0x00, 0x00, 0x00,
        0x00, 0x04, 0x00, 0x04, 0x00, 0x31, 0x01, 0x00,
        0x00, 0x90, 0x00, 0x00, 0x00, 0x00, 0x00
    }.slice();
    var (r, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)), (int64)len(data));
    if (!AreEqual(err, ErrInsecurePath)) {
        Ꮡt.Fatalf("Error reading the archive: %v"u8, err);
    }
    var entryNames = new @string[]{@"/"u8, @"//"u8, @"\"u8, @"/test.txt"u8}.slice();
    slice<@string> names = default!;
    foreach (var (_, f) in (~r).File) {
        names = append(names, (~f).Name);
        {
            var (_, errΔ1) = f.Open(); if (errΔ1 != default!) {
                Ꮡt.Errorf("Error opening %q: %v"u8, (~f).Name, errΔ1);
            }
        }
        {
            var (_, errΔ2) = r.Open((~f).Name); if (errΔ2 == default!) {
                Ꮡt.Errorf("Opening %q with fs.FS API succeeded"u8, (~f).Name);
            }
        }
    }
    if (!slices.Equal<slice<@string>, @string>(names, entryNames)) {
        Ꮡt.Errorf("Unexpected file entries: %q"u8, names);
    }
    {
        var (_, errΔ3) = r.Open(""u8); if (errΔ3 == default!) {
            Ꮡt.Errorf("Opening %q with fs.FS API succeeded"u8, (@string)""u8);
        }
    }
    {
        var (_, errΔ4) = r.Open(testTxtˢ); if (errΔ4 != default!) {
            Ꮡt.Errorf("Error opening %q with fs.FS API: %v"u8, testTxtˢ, errΔ4);
        }
    }
    (var dirEntries, err) = fs.ReadDir(new zip_test_package.zip_ReaderжFS(r), "."u8);
    if (err != default!) {
        Ꮡt.Fatalf("Error reading the root directory: %v"u8, err);
    }
    if (len(dirEntries) != 1 || dirEntries[0].Name() != "test.txt"u8) {
        Ꮡt.Errorf("Unexpected directory entries"u8);
        foreach (var (_, dirEntry) in dirEntries) {
            var (_, errΔ5) = r.Open(dirEntry.Name());
            Ꮡt.Logf("%q (Open error: %v)"u8, dirEntry.Name(), errΔ5);
        }
        Ꮡt.FailNow();
    }
    (var info, err) = dirEntries[0].Info();
    if (err != default!) {
        Ꮡt.Fatalf("Error reading info entry: %v"u8, err);
    }
    {
        @string name = info.Name(); if (name != "test.txt"u8) {
            Ꮡt.Errorf("Inconsistent name in info entry: %v"u8, name);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataReadmeZipˢ = "testdata/readme.zip"u8;

public static void TestUnderSize(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (z, err) = OpenReader(testdataReadmeZipˢ);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var zʗ1 = z;
        defer(() => zʗ1.Close(), ref ᒐ);
        foreach (var (_, f) in (~z).File) {
            f.Value.UncompressedSize64 = 1;
        }
        foreach (var (_, f) in (~z).File) {
            var fʗ1 = f;
            Ꮡt.Run((~f).Name, (ж<testing.T> tΔ1) => {
                GoFrame ᒐ = default;
                try {
                    var (rd, errΔ1) = fʗ1.Open();
                    if (errΔ1 != default!) {
                        tΔ1.Fatal(errΔ1);
                    }
                    var rdʗ1 = rd;
                    defer(() => rdʗ1.Close(), ref ᒐ);
                    (_, errΔ1) = io.Copy(io.Discard, rd);
                    if (!AreEqual(errΔ1, ErrFormat)) {
                        tΔ1.Fatalf("Error mismatch\n\tGot:  %v\n\tWant: %v"u8, errΔ1, ErrFormat);
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIssue54801(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        foreach (var (_, input) in new @string[]{"testdata/readme.zip"u8, "testdata/dd.zip"u8}.slice()) {
            var (z, err) = OpenReader(input);
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            var zʗ1 = z;
            defer(() => zʗ1.Close(), ref ᒐ);
            foreach (var (_, f) in (~z).File) {
                // Make file a directory
                f.Value.Name += "/"u8;
                var fʗ1 = f;
                Ꮡt.Run((~f).Name, (ж<testing.T> tΔ1) => {
                    GoFrame ᒐ = default;
                    try {
                        tΔ1.Logf("CompressedSize64: %d, Flags: %#x"u8, (~fʗ1).CompressedSize64, (~fʗ1).Flags);
                        var (rd, errΔ1) = fʗ1.Open();
                        if (errΔ1 != default!) {
                            tΔ1.Fatal(errΔ1);
                        }
                        var rdʗ1 = rd;
                        defer(() => rdʗ1.Close(), ref ᒐ);
                        var (n, got) = io.Copy(io.Discard, rd);
                        if (n != 0 || !AreEqual(got, ErrFormat)) {
                            tΔ1.Fatalf("Error mismatch, got: %d, %v, want: %v"u8, n, got, ErrFormat);
                        }
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestInsecurePaths(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, zipinsecurepath0ˢ);
    foreach (var (_, path) in new @string[]{
        "../foo"u8,
        "/foo"u8,
        "a/b/../../../c"u8,
        @"a\b"u8
    }.slice()) {
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        var zw = NewWriter(new zip_test_package.bytes_BufferжWriter(Ꮡbuf));
        var (_, err) = zw.Create(path);
        if (err != default!) {
            Ꮡt.Errorf("zw.Create(%q) = %v"u8, path, err);
            continue;
        }
        zw.Close();
        (var zr, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(buf.Bytes())), (int64)buf.Len());
        if (!AreEqual(err, ErrInsecurePath)) {
            Ꮡt.Errorf("NewReader for archive with file %q: got err %v, want ErrInsecurePath"u8, path, err);
            continue;
        }
        slice<@string> gotPaths = default!;
        foreach (var (_, f) in (~zr).File) {
            gotPaths = append(gotPaths, (~f).Name);
        }
        if (!slices.Equal<slice<@string>, @string>(gotPaths, new @string[]{path}.slice())) {
            Ꮡt.Errorf("NewReader for archive with file %q: got files %q"u8, path, gotPaths);
            continue;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string zipinsecurepath1ˢ = "zipinsecurepath=1"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string nameᶜ = "/foo"u8;

public static void TestDisableInsecurePathCheck(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, zipinsecurepath1ˢ);
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var zw = NewWriter(new zip_test_package.bytes_BufferжWriter(Ꮡbuf));
    @string name = nameᶜ;
    var (_, err) = zw.Create(name);
    if (err != default!) {
        Ꮡt.Fatalf("zw.Create(%q) = %v"u8, name, err);
    }
    zw.Close();
    (var zr, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(buf.Bytes())), (int64)buf.Len());
    if (err != default!) {
        Ꮡt.Fatalf("NewReader with zipinsecurepath=1: got err %v, want nil"u8, err);
    }
    slice<@string> gotPaths = default!;
    foreach (var (_, f) in (~zr).File) {
        gotPaths = append(gotPaths, (~f).Name);
    }
    {
        var want = new @string[]{name}.slice(); if (!slices.Equal<slice<@string>, @string>(gotPaths, want)) {
            Ꮡt.Errorf("NewReader with zipinsecurepath=1: got files %q, want %q"u8, gotPaths, want);
        }
    }
}

public static void TestCompressedDirectory(ж<testing.T> Ꮡt) {
    // Empty Java JAR, with a compressed directory with uncompressed size 0
    // which should not fail.
    //
    // Length   Method    Size  Cmpr    Date    Time   CRC-32   Name
    // --------  ------  ------- ---- ---------- ----- --------  ----
    //        0  Defl:N        2   0% 12-01-2022 16:50 00000000  META-INF/
    //       60  Defl:N       59   2% 12-01-2022 16:50 af937e93  META-INF/MANIFEST.MF
    // --------          -------  ---                            -------
    //       60               61  -2%                            2 files
    var data = new byte[]{
        0x50, 0x4b, 0x03, 0x04, 0x14, 0x00, 0x08, 0x08,
        0x08, 0x00, 0x49, 0x86, 0x81, 0x55, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x09, 0x00, 0x04, 0x00, 0x4d, 0x45,
        0x54, 0x41, 0x2d, 0x49, 0x4e, 0x46, 0x2f, 0xfe,
        0xca, 0x00, 0x00, 0x03, 0x00, 0x50, 0x4b, 0x07,
        0x08, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x50, 0x4b, 0x03,
        0x04, 0x14, 0x00, 0x08, 0x08, 0x08, 0x00, 0x49,
        0x86, 0x81, 0x55, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14,
        0x00, 0x00, 0x00, 0x4d, 0x45, 0x54, 0x41, 0x2d,
        0x49, 0x4e, 0x46, 0x2f, 0x4d, 0x41, 0x4e, 0x49,
        0x46, 0x45, 0x53, 0x54, 0x2e, 0x4d, 0x46, 0xf3,
        0x4d, 0xcc, 0xcb, 0x4c, 0x4b, 0x2d, 0x2e, 0xd1,
        0x0d, 0x4b, 0x2d, 0x2a, 0xce, 0xcc, 0xcf, 0xb3,
        0x52, 0x30, 0xd4, 0x33, 0xe0, 0xe5, 0x72, 0x2e,
        0x4a, 0x4d, 0x2c, 0x49, 0x4d, 0xd1, 0x75, 0xaa,
        0x04, 0x0a, 0x00, 0x45, 0xf4, 0x0c, 0x8d, 0x15,
        0x34, 0xdc, 0xf3, 0xf3, 0xd3, 0x73, 0x52, 0x15,
        0x3c, 0xf3, 0x92, 0xf5, 0x34, 0x79, 0xb9, 0x78,
        0xb9, 0x00, 0x50, 0x4b, 0x07, 0x08, 0x93, 0x7e,
        0x93, 0xaf, 0x3b, 0x00, 0x00, 0x00, 0x3c, 0x00,
        0x00, 0x00, 0x50, 0x4b, 0x01, 0x02, 0x14, 0x00,
        0x14, 0x00, 0x08, 0x08, 0x08, 0x00, 0x49, 0x86,
        0x81, 0x55, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x00,
        0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x4d, 0x45, 0x54, 0x41, 0x2d, 0x49, 0x4e, 0x46,
        0x2f, 0xfe, 0xca, 0x00, 0x00, 0x50, 0x4b, 0x01,
        0x02, 0x14, 0x00, 0x14, 0x00, 0x08, 0x08, 0x08,
        0x00, 0x49, 0x86, 0x81, 0x55, 0x93, 0x7e, 0x93,
        0xaf, 0x3b, 0x00, 0x00, 0x00, 0x3c, 0x00, 0x00,
        0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3d,
        0x00, 0x00, 0x00, 0x4d, 0x45, 0x54, 0x41, 0x2d,
        0x49, 0x4e, 0x46, 0x2f, 0x4d, 0x41, 0x4e, 0x49,
        0x46, 0x45, 0x53, 0x54, 0x2e, 0x4d, 0x46, 0x50,
        0x4b, 0x05, 0x06, 0x00, 0x00, 0x00, 0x00, 0x02,
        0x00, 0x02, 0x00, 0x7d, 0x00, 0x00, 0x00, 0xba,
        0x00, 0x00, 0x00, 0x00, 0x00
    }.slice();
    var (r, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)), (int64)len(data));
    if (err != default!) {
        Ꮡt.Fatalf("unexpected error: %v"u8, err);
    }
    foreach (var (_, f) in (~r).File) {
        var (rΔ1, errΔ1) = f.Open();
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("unexpected error: %v"u8, errΔ1);
        }
        {
            var (_, errΔ2) = io.Copy(io.Discard, rΔ1); if (errΔ2 != default!) {
                Ꮡt.Fatalf("unexpected error: %v"u8, errΔ2);
            }
        }
    }
}

public static void TestBaseOffsetPlusOverflow(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // directoryOffset > maxInt64 && size-directoryOffset < 0
        var data = new byte[]{
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0xff, 0xff, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x50, 0x4b, 0x06, 0x06, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x20, 0xff, 0xff, 0x20, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x20, 0x08, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x80, 0x50, 0x4b, 0x06, 0x07, 0x00,
            0x00, 0x00, 0x00, 0x6b, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x50,
            0x4b, 0x05, 0x06, 0x20, 0x20, 0x20, 0x20, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0x20, 0x00
        }.slice();
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    Ꮡt.Fatalf("NewReader panicked: %s"u8, r);
                }
            }
        }, ref ᒐ);
        // Previously, this would trigger a panic as we attempt to read from
        // an io.SectionReader which would access a slice at a negative offset
        // as the section reader offset & size were < 0.
        NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)), (int64)len(data) + 1875);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string doesNotExistˢ = "does-not-exist"u8;

public static void BenchmarkReaderOneDeepDir(ж<testing.B> Ꮡb) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var zw = NewWriter(new zip_test_package.bytes_BufferжWriter(Ꮡbuf));
    foreach (var i in range(4000)) {
        ref var name = ref heap<@string>(out var Ꮡname);
        name = strings.Repeat("a/"u8, i) + "data"u8;
        zw.CreateHeader(Ꮡ(new FileHeader(
            Name: name,
            Method: Store
        )));
    }
    {
        var err = zw.Close(); if (err != default!) {
            Ꮡb.Fatal(err);
        }
    }
    var data = buf.Bytes();
    while (Ꮡb.Loop()) {
        var (zr, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)), (int64)len(data));
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        zr.Open(doesNotExistˢ);
    }
}

public static void BenchmarkReaderManyDeepDirs(ж<testing.B> Ꮡb) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var zw = NewWriter(new zip_test_package.bytes_BufferжWriter(Ꮡbuf));
    foreach (var i in range(2850)) {
        ref var name = ref heap<@string>(out var Ꮡname);
        name = fmt.Sprintf("%x"u8, i);
        name = strings.Repeat("/"u8 + name, i + 1)[1..];
        zw.CreateHeader(Ꮡ(new FileHeader(
            Name: name,
            Method: Store
        )));
    }
    {
        var err = zw.Close(); if (err != default!) {
            Ꮡb.Fatal(err);
        }
    }
    var data = buf.Bytes();
    while (Ꮡb.Loop()) {
        var (zr, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)), (int64)len(data));
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        zr.Open(doesNotExistˢ);
    }
}

public static void BenchmarkReaderManyShallowFiles(ж<testing.B> Ꮡb) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var zw = NewWriter(new zip_test_package.bytes_BufferжWriter(Ꮡbuf));
    foreach (var i in range(310000)) {
        ref var name = ref heap<@string>(out var Ꮡname);
        name = fmt.Sprintf("%v"u8, i);
        zw.CreateHeader(Ꮡ(new FileHeader(
            Name: name,
            Method: Store
        )));
    }
    {
        var err = zw.Close(); if (err != default!) {
            Ꮡb.Fatal(err);
        }
    }
    var data = buf.Bytes();
    while (Ꮡb.Loop()) {
        var (zr, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(data)), (int64)len(data));
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        zr.Open(doesNotExistˢ);
    }
}

} // end zip_internal_test_package
