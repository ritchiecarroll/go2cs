// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.archive;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using io = io_package;
using fs = go.io.fs_package;
using maps = maps_package;
using math = math_package;
using os = os_package;
using path = path_package;
using filepath = go.path.filepath_package;
using reflect = reflect_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using @internal;
using go.io;
using go.path;
using static go.archive.tar_package;

partial class tar_internal_test_package {

internal partial struct testError {
    /*embed*/ internal error error;
}

internal partial struct fileOps /*[]any*/; // []T where T is (string | int64)

// testFile is an io.ReadWriteSeeker where the IO operations performed
// on it must match the list of operations in ops.
internal partial struct testFile {
    internal fileOps ops;
    internal int64 pos;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unexpectedReadOperationˢ => ref unexpectedReadOperationˢᵇ;
private static readonly @string unexpectedReadOperationˢᵇ = "unexpected Read operation"u8;

internal static (nint, error) Read(this ref testFile f, slice<byte> b) {
    if (len(b) == 0) {
        return (0, default);
    }
    if (len(f.ops) == 0) {
        return (0, io.EOF);
    }
    var (s, ok) = f.ops[0]._<@string>(ᐧ);
    if (!ok) {
        return (0, errors.New(unexpectedReadOperationˢ));
    }

    nint n = copy(b, s);
    if (len(s) > n) {
        f.ops[0] = s.slice(n);
    } else {
        f.ops = f.ops[1..];
    }
    f.pos += (int64)len(b);
    return (n, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unexpectedWriteOperationˢ => ref unexpectedWriteOperationˢᵇ;
private static readonly @string unexpectedWriteOperationˢᵇ = "unexpected Write operation"u8;

internal static (nint, error) Write(this ref testFile f, slice<byte> b) {
    if (len(b) == 0) {
        return (0, default);
    }
    if (len(f.ops) == 0) {
        return (0, errors.New(unexpectedWriteOperationˢ));
    }
    var (s, ok) = f.ops[0]._<@string>(ᐧ);
    if (!ok) {
        return (0, errors.New(unexpectedWriteOperationˢ));
    }

    if (!strings.HasPrefix(s, (@string)b)) {
        return (0, new testError(fmt.Errorf("got Write(%q), want Write(%q)"u8, b, s)));
    }
    if (len(s) > len(b)) {
        f.ops[0] = s.slice(len(b));
    } else {
        f.ops = f.ops[1..];
    }
    f.pos += (int64)len(b);
    return (len(b), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unexpectedSeekOperationˢ => ref unexpectedSeekOperationˢᵇ;
private static readonly @string unexpectedSeekOperationˢᵇ = "unexpected Seek operation"u8;

internal static (int64, error) Seek(this ref testFile f, int64 pos, nint whence) {
    if (pos == 0 && whence == io.SeekCurrent) {
        return (f.pos, default);
    }
    if (len(f.ops) == 0) {
        return (0, errors.New(unexpectedSeekOperationˢ));
    }
    var (s, ok) = f.ops[0]._<int64>(ᐧ);
    if (!ok) {
        return (0, errors.New(unexpectedSeekOperationˢ));
    }

    if (s != pos || whence != io.SeekCurrent) {
        return (0, new testError(fmt.Errorf("got Seek(%d, %d), want Seek(%d, %d)"u8, pos, whence, s, (nint)(io.SeekCurrent))));
    }
    f.pos += s;
    f.ops = f.ops[1..];
    return (f.pos, default);
}

internal partial struct TestSparseEntries_vectors /*dyn*/ {
    internal slice<global::go.archive.tar_package.sparseEntry> @in;
    internal int64 size;
    internal bool wantValid;                                                 // Result of validateSparseEntries
    internal slice<global::go.archive.tar_package.sparseEntry> wantAligned;  // Result of alignSparseEntries
    internal slice<global::go.archive.tar_package.sparseEntry> wantInverted; // Result of invertSparseEntries
}

public static void TestSparseEntries(ж<testing.T> t) {
    var vectors = slice<TestSparseEntries_vectors>([new(
        @in: [], size: 0,
        wantValid: true,
        wantInverted: [new(0, 0)]
    ), new(
        @in: [], size: 5000,
        wantValid: true,
        wantInverted: [new(0, 5000)]
    ), new(
        @in: [new(0, 5000)], size: 5000,
        wantValid: true,
        wantAligned: [new(0, 5000)],
        wantInverted: [new(5000, 0)]
    ), new(
        @in: [new(1000, 4000)], size: 5000,
        wantValid: true,
        wantAligned: [new(1024, 3976)],
        wantInverted: [new(0, 1000), new(5000, 0)]
    ), new(
        @in: [new(0, 3000)], size: 5000,
        wantValid: true,
        wantAligned: [new(0, 2560)],
        wantInverted: [new(3000, 2000)]
    ), new(
        @in: [new(3000, 2000)], size: 5000,
        wantValid: true,
        wantAligned: [new(3072, 1928)],
        wantInverted: [new(0, 3000), new(5000, 0)]
    ), new(
        @in: [new(2000, 2000)], size: 5000,
        wantValid: true,
        wantAligned: [new(2048, 1536)],
        wantInverted: [new(0, 2000), new(4000, 1000)]
    ), new(
        @in: [new(0, 2000), new(8000, 2000)], size: 10000,
        wantValid: true,
        wantAligned: [new(0, 1536), new(8192, 1808)],
        wantInverted: [new(2000, 6000), new(10000, 0)]
    ), new(
        @in: [new(0, 2000), new(2000, 2000), new(4000, 0), new(4000, 3000), new(7000, 1000), new(8000, 0), new(8000, 2000)], size: 10000,
        wantValid: true,
        wantAligned: [new(0, 1536), new(2048, 1536), new(4096, 2560), new(7168, 512), new(8192, 1808)],
        wantInverted: [new(10000, 0)]
    ), new(
        @in: [new(0, 0), new(1000, 0), new(2000, 0), new(3000, 0), new(4000, 0), new(5000, 0)], size: 5000,
        wantValid: true,
        wantInverted: [new(0, 5000)]
    ), new(
        @in: [new(1, 0)], size: 0,
        wantValid: false
    ), new(
        @in: [new(-1, 0)], size: 100,
        wantValid: false
    ), new(
        @in: [new(0, -1)], size: 100,
        wantValid: false
    ), new(
        @in: [new(0, 0)], size: -100,
        wantValid: false
    ), new(
        @in: [new(math.MaxInt64, 3), new(6, -5)], size: 35,
        wantValid: false
    ), new(
        @in: [new(1, 3), new(6, -5)], size: 35,
        wantValid: false
    ), new(
        @in: [new(math.MaxInt64, math.MaxInt64)], size: math.MaxInt64,
        wantValid: false
    ), new(
        @in: [new(3, 3)], size: 5,
        wantValid: false
    ), new(
        @in: [new(2, 0), new(1, 0), new(0, 0)], size: 3,
        wantValid: false
    ), new(
        @in: [new(1, 3), new(2, 2)], size: 10,
        wantValid: false
    )
    ]);

    foreach (var (i, v) in range(vectors)) {
        var gotValid = validateSparseEntries(v.@in, v.size);
        if (gotValid != v.wantValid) {
            t.Errorf("test %d, validateSparseEntries() = %v, want %v", i, gotValid, v.wantValid);
        }
        if (!v.wantValid) {
            continue;
        }
        var gotAligned = alignSparseEntries(appendꓸꓸꓸ(slice<global::go.archive.tar_package.sparseEntry>([]), v.@in), v.size);
        if (!slices.Equal<slice<global::go.archive.tar_package.sparseEntry>, global::go.archive.tar_package.sparseEntry>(gotAligned, v.wantAligned)) {
            t.Errorf("test %d, alignSparseEntries():\ngot  %v\nwant %v", i, gotAligned, v.wantAligned);
        }
        var gotInverted = invertSparseEntries(appendꓸꓸꓸ(slice<global::go.archive.tar_package.sparseEntry>([]), v.@in), v.size);
        if (!slices.Equal<slice<global::go.archive.tar_package.sparseEntry>, global::go.archive.tar_package.sparseEntry>(gotInverted, v.wantInverted)) {
            t.Errorf("test %d, inverseSparseEntries():\ngot  %v\nwant %v", i, gotInverted, v.wantInverted);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataSmallTxtˢ => ref testdataSmallTxtˢᵇ;
private static readonly @string testdataSmallTxtˢᵇ = "testdata/small.txt"u8;
internal static ref readonly @string smallTxtˢ => ref smallTxtˢᵇ;
private static readonly @string smallTxtˢᵇ = "small.txt"u8;

public static void TestFileInfoHeader(ж<testing.T> t) {
    var (fi, err) = os.Stat(testdataSmallTxtˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    (var h, err) = FileInfoHeader(fi, ""u8);
    if (err != nil) {
        t.Fatalf("FileInfoHeader: %v", err);
    }
    {
        @string g = h.Name;
        @string e = smallTxtˢ; if (g != e) {
            t.Errorf("Name = %q; want %q", g, e);
        }
    }
    {
        var (g, e) = (h.Mode, (int64)fi.Mode().Perm()); if (g != e) {
            t.Errorf("Mode = %#o; want %#o", g, e);
        }
    }
    {
        var (g, e) = (h.Size, (int64)5); if (g != e) {
            t.Errorf("Size = %v; want %v", g, e);
        }
    }
    {
        var (g, e) = (h.ModTime, fi.ModTime()); if (!g.Equal(e)) {
            t.Errorf("ModTime = %v; want %v", g, e);
        }
    }
    // FileInfoHeader should error when passing nil FileInfo
    var (_, errΔ1) = FileInfoHeader(default, ""u8); if (errΔ1 == nil) {
        t.Fatalf("Expected error when passing nil to FileInfoHeader");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "testdata"u8;
internal static ref readonly @string testdataˢ2 => ref testdataˢ2ᵇ;
private static readonly @string testdataˢ2ᵇ = "testdata/"u8;

public static void TestFileInfoHeaderDir(ж<testing.T> t) {
    var (fi, err) = os.Stat(testdataˢ);
    if (err != nil) {
        t.Fatal(err);
    }
    (var h, err) = FileInfoHeader(fi, ""u8);
    if (err != nil) {
        t.Fatalf("FileInfoHeader: %v", err);
    }
    {
        @string g = h.Name;
        @string e = testdataˢ2; if (g != e) {
            t.Errorf("Name = %q; want %q", g, e);
        }
    }
    // Ignoring c_ISGID for golang.org/issue/4867
    {
        var (g, e) = (h.Mode & ~(int64)c_ISGID, (int64)fi.Mode().Perm()); if (g != e) {
            t.Errorf("Mode = %#o; want %#o", g, e);
        }
    }
    {
        var (g, e) = (h.Size, (int64)0); if (g != e) {
            t.Errorf("Size = %v; want %v", g, e);
        }
    }
    {
        var (g, e) = (h.ModTime, fi.ModTime()); if (!g.Equal(e)) {
            t.Errorf("ModTime = %v; want %v", g, e);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string linkˢ => ref linkˢᵇ;
private static readonly @string linkˢᵇ = "link"u8;

public static void TestFileInfoHeaderSymlink(ж<testing.T> t) {
    testenv.MustHaveSymlink(new tar_test_package.testing_TжTB(t));

    @string tmpdir = t.TempDir();

    @string link = filepath.Join(tmpdir, linkˢ);
    @string target = tmpdir;
    var errΔ1 = os.Symlink(target, link); if (errΔ1 != nil) {
        t.Fatal(errΔ1);
    }
    var (fi, err) = os.Lstat(link);
    if (err != nil) {
        t.Fatal(err);
    }

    (var h, err) = FileInfoHeader(fi, target);
    if (err != nil) {
        t.Fatal(err);
    }
    {
        @string g = h.Name;
        @string e = fi.Name(); if (g != e) {
            t.Errorf("Name = %q; want %q", g, e);
        }
    }
    {
        @string g = h.Linkname;
        @string e = target; if (g != e) {
            t.Errorf("Linkname = %q; want %q", g, e);
        }
    }
    {
        var (g, e) = (h.Typeflag, (byte)TypeSymlink); if (g != e) {
            t.Errorf("Typeflag = %v; want %v", g, e);
        }
    }
}

public static void TestRoundTrip(ж<testing.T> t) {
    var data = slice<byte>("some file contents"u8);

    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
    var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡb));
    var hdr = Ꮡ(new Header(
        Name: "file.txt",
        Uid: (1 << 21), // Too big for 8 octal digits
        Size: (int64)len(data),
        ModTime: time.Now().Round(time.ΔSecond),
        PAXRecords: new map<@string, @string>{["uid"] = "2097152"},
        Format: FormatPAX,
        Typeflag: TypeReg
    ));
    var errΔ1 = tw.WriteHeader(hdr); if (errΔ1 != nil) {
        t.Fatalf("tw.WriteHeader: %v", errΔ1);
    }
    var (_, errΔ2) = tw.Write(data); if (errΔ2 != nil) {
        t.Fatalf("tw.Write: %v", errΔ2);
    }
    var errΔ3 = tw.Close(); if (errΔ3 != nil) {
        t.Fatalf("tw.Close: %v", errΔ3);
    }

    // Read it back.
    var tr = NewReader(new tar_test_package.bytes_BufferжReader(Ꮡb));
    var (rHdr, err) = tr.Next();
    if (err != nil) {
        t.Fatalf("tr.Next: %v", err);
    }
    if (!reflect.DeepEqual(rHdr.OrTypedNil(), hdr.OrTypedNil())) {
        t.Errorf("Header mismatch.\n got %+v\nwant %+v", rHdr.OrTypedNil(), hdr.OrTypedNil());
    }
    (var rData, err) = io.ReadAll(new global::go.archive.tar_package.ReaderжReader(tr));
    if (err != nil) {
        t.Fatalf("Read: %v", err);
    }
    if (!bytes.Equal(rData, data)) {
        t.Errorf("Data mismatch.\n got %q\nwant %q", rData, data);
    }
}

internal partial struct headerRoundTripTest {
    internal ж<global::go.archive.tar_package.Header> h;
    internal fs.FileMode fm;
}

public static void TestHeaderRoundTrip(ж<testing.T> t) {
    var vectors = slice<headerRoundTripTest>([new(
        // regular file.
        h: Ꮡ(new Header(
            Name: "test.txt",
            Mode: 420,
            Size: 12,
            ModTime: time.Unix(1360600916, 0),
            Typeflag: TypeReg
        )),
        fm: 420
    ), new(
        // symbolic link.
        h: Ꮡ(new Header(
            Name: "link.txt",
            Mode: 511,
            Size: 0,
            ModTime: time.Unix(1360600852, 0),
            Typeflag: TypeSymlink
        )),
        fm: (fs.FileMode)(511 | fs.ModeSymlink)
    ), new(
        // character device node.
        h: Ꮡ(new Header(
            Name: "dev/null",
            Mode: 438,
            Size: 0,
            ModTime: time.Unix(1360578951, 0),
            Typeflag: TypeChar
        )),
        fm: (fs.FileMode)((fs.FileMode)(438 | fs.ModeDevice) | fs.ModeCharDevice)
    ), new(
        // block device node.
        h: Ꮡ(new Header(
            Name: "dev/sda",
            Mode: 432,
            Size: 0,
            ModTime: time.Unix(1360578954, 0),
            Typeflag: TypeBlock
        )),
        fm: (fs.FileMode)(432 | fs.ModeDevice)
    ), new(
        // directory.
        h: Ꮡ(new Header(
            Name: "dir/",
            Mode: 493,
            Size: 0,
            ModTime: time.Unix(1360601116, 0),
            Typeflag: TypeDir
        )),
        fm: (fs.FileMode)(493 | fs.ModeDir)
    ), new(
        // fifo node.
        h: Ꮡ(new Header(
            Name: "dev/initctl",
            Mode: 384,
            Size: 0,
            ModTime: time.Unix(1360578949, 0),
            Typeflag: TypeFifo
        )),
        fm: (fs.FileMode)(384 | fs.ModeNamedPipe)
    ), new(
        // setuid.
        h: Ꮡ(new Header(
            Name: "bin/su",
            Mode: (int64)(493 | (int64)c_ISUID),
            Size: 23232,
            ModTime: time.Unix(1355405093, 0),
            Typeflag: TypeReg
        )),
        fm: (fs.FileMode)(493 | fs.ModeSetuid)
    ), new(
        // setguid.
        h: Ꮡ(new Header(
            Name: "group.txt",
            Mode: (int64)(488 | (int64)c_ISGID),
            Size: 0,
            ModTime: time.Unix(1360602346, 0),
            Typeflag: TypeReg
        )),
        fm: (fs.FileMode)(488 | fs.ModeSetgid)
    ), new(
        // sticky.
        h: Ꮡ(new Header(
            Name: "sticky.txt",
            Mode: (int64)(384 | (int64)c_ISVTX),
            Size: 7,
            ModTime: time.Unix(1360602540, 0),
            Typeflag: TypeReg
        )),
        fm: (fs.FileMode)(384 | fs.ModeSticky)
    ), new(
        // hard link.
        h: Ꮡ(new Header(
            Name: "hard.txt",
            Mode: 420,
            Size: 0,
            Linkname: "file.txt",
            ModTime: time.Unix(1360600916, 0),
            Typeflag: TypeLink
        )),
        fm: 420
    ), new(
        // More information.
        h: Ꮡ(new Header(
            Name: "info.txt",
            Mode: 384,
            Size: 0,
            Uid: 1000,
            Gid: 1000,
            ModTime: time.Unix(1360602540, 0),
            Uname: "slartibartfast",
            Gname: "users",
            Typeflag: TypeReg
        )),
        fm: 384
    )
    ]);

    foreach (var (i, v) in range(vectors)) {
        var fi = v.h.FileInfo();
        var (h2, err) = FileInfoHeader(fi, ""u8);
        if (err != nil) {
            t.Error(err);
            continue;
        }
        if (strings.Contains(fi.Name(), "/")) {
            t.Errorf("FileInfo of %q contains slash: %q", v.h.Name, fi.Name());
        }
        @string name = path.Base(v.h.Name);
        if (fi.IsDir()) {
            name += "/"u8;
        }
        {
            @string got = h2.Name;
            @string want = name; if (got != want) {
                t.Errorf("i=%d: Name: got %v, want %v", i, got, want);
            }
        }
        {
            var (got, want) = (h2.Size, v.h.Size); if (got != want) {
                t.Errorf("i=%d: Size: got %v, want %v", i, got, want);
            }
        }
        {
            nint got = h2.Uid;
            nint want = v.h.Uid; if (got != want) {
                t.Errorf("i=%d: Uid: got %d, want %d", i, got, want);
            }
        }
        {
            nint got = h2.Gid;
            nint want = v.h.Gid; if (got != want) {
                t.Errorf("i=%d: Gid: got %d, want %d", i, got, want);
            }
        }
        {
            @string got = h2.Uname;
            @string want = v.h.Uname; if (got != want) {
                t.Errorf("i=%d: Uname: got %q, want %q", i, got, want);
            }
        }
        {
            @string got = h2.Gname;
            @string want = v.h.Gname; if (got != want) {
                t.Errorf("i=%d: Gname: got %q, want %q", i, got, want);
            }
        }
        {
            @string got = h2.Linkname;
            @string want = v.h.Linkname; if (got != want) {
                t.Errorf("i=%d: Linkname: got %v, want %v", i, got, want);
            }
        }
        {
            var (got, want) = (h2.Typeflag, v.h.Typeflag); if (got != want) {
                t.Logf("%#v %#v", v.h.OrTypedNil(), fi.Sys());
                t.Errorf("i=%d: Typeflag: got %q, want %q", i, got, want);
            }
        }
        {
            var (got, want) = (h2.Mode, v.h.Mode); if (got != want) {
                t.Errorf("i=%d: Mode: got %o, want %o", i, got, want);
            }
        }
        {
            var (got, want) = (fi.Mode(), v.fm); if (got != want) {
                t.Errorf("i=%d: fi.Mode: got %o, want %o", i, got, want);
            }
        }
        {
            var (got, want) = (h2.AccessTime, v.h.AccessTime); if (got != want) {
                t.Errorf("i=%d: AccessTime: got %v, want %v", i, got, want);
            }
        }
        {
            var (got, want) = (h2.ChangeTime, v.h.ChangeTime); if (got != want) {
                t.Errorf("i=%d: ChangeTime: got %v, want %v", i, got, want);
            }
        }
        {
            var (got, want) = (h2.ModTime, v.h.ModTime); if (got != want) {
                t.Errorf("i=%d: ModTime: got %v, want %v", i, got, want);
            }
        }
        var (sysh, ok) = fi.Sys()._<ж<global::go.archive.tar_package.Header>>(ᐧ); if (!ok || sysh != v.h) {
            t.Errorf("i=%d: Sys didn't return original *Header", i);
        }
    }
}

internal partial struct TestHeaderAllowedFormats_vectors /*dyn*/ {
    internal ж<global::go.archive.tar_package.Header> header; // Input header
    internal map<@string, @string> paxHdrs;                   // Expected PAX headers that may be needed
    internal global::go.archive.tar_package.Format formats;   // Expected formats that can encode the header
}

public static void TestHeaderAllowedFormats(ж<testing.T> t) {
    var vectors = slice<TestHeaderAllowedFormats_vectors>([new(
        header: Ꮡ(new Header()),
        formats: (global::go.archive.tar_package.Format)((global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX) | FormatGNU)
    ), new(
        header: Ꮡ(new Header(Size: 8589934591L)),
        formats: (global::go.archive.tar_package.Format)((global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX) | FormatGNU)
    ), new(
        header: Ꮡ(new Header(Size: 8589934591L, Format: FormatUSTAR)),
        formats: FormatUSTAR
    ), new(
        header: Ꮡ(new Header(Size: 8589934591L, Format: FormatPAX)),
        formats: (global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX)
    ), new(
        header: Ꮡ(new Header(Size: 8589934591L, Format: FormatGNU)),
        formats: FormatGNU
    ), new(
        header: Ꮡ(new Header(Size: 8589934592L)),
        paxHdrs: new map<@string, @string>{[paxSize] = "8589934592"},
        formats: (global::go.archive.tar_package.Format)(FormatPAX | FormatGNU)
    ), new(
        header: Ꮡ(new Header(Size: 8589934592L, Format: FormatPAX)),
        paxHdrs: new map<@string, @string>{[paxSize] = "8589934592"},
        formats: FormatPAX
    ), new(
        header: Ꮡ(new Header(Size: 8589934592L, Format: FormatGNU)),
        paxHdrs: new map<@string, @string>{[paxSize] = "8589934592"},
        formats: FormatGNU
    ), new(
        header: Ꮡ(new Header(Mode: 2097151)),
        formats: (global::go.archive.tar_package.Format)((global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX) | FormatGNU)
    ), new(
        header: Ꮡ(new Header(Mode: 2097151 + 1)),
        formats: FormatGNU
    ), new(
        header: Ꮡ(new Header(Devmajor: -123)),
        formats: FormatGNU
    ), new(
        header: Ꮡ(new Header(Devmajor: 72057594037927935L)),
        formats: FormatGNU
    ), new(
        header: Ꮡ(new Header(Devmajor: 72057594037927936L)),
        formats: FormatUnknown
    ), new(
        header: Ꮡ(new Header(Devmajor: -72057594037927936L)),
        formats: FormatGNU
    ), new(
        header: Ꮡ(new Header(Devmajor: -72057594037927937L)),
        formats: FormatUnknown
    ), new(
        header: Ꮡ(new Header(Name: "用戶名", Devmajor: -72057594037927936L)),
        formats: FormatGNU
    ), new(
        header: Ꮡ(new Header(Size: math.MaxInt64)),
        paxHdrs: new map<@string, @string>{[paxSize] = "9223372036854775807"},
        formats: (global::go.archive.tar_package.Format)(FormatPAX | FormatGNU)
    ), new(
        header: Ꮡ(new Header(Size: math.MinInt64)),
        paxHdrs: new map<@string, @string>{[paxSize] = "-9223372036854775808"},
        formats: FormatUnknown
    ), new(
        header: Ꮡ(new Header(Uname: "0123456789abcdef0123456789abcdef")),
        formats: (global::go.archive.tar_package.Format)((global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX) | FormatGNU)
    ), new(
        header: Ꮡ(new Header(Uname: "0123456789abcdef0123456789abcdefx")),
        paxHdrs: new map<@string, @string>{[paxUname] = "0123456789abcdef0123456789abcdefx"},
        formats: FormatPAX
    ), new(
        header: Ꮡ(new Header(Name: "foobar")),
        formats: (global::go.archive.tar_package.Format)((global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX) | FormatGNU)
    ), new(
        header: Ꮡ(new Header(Name: strings.Repeat("a", nameSize))),
        formats: (global::go.archive.tar_package.Format)((global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX) | FormatGNU)
    ), new(
        header: Ꮡ(new Header(Name: strings.Repeat("a", nameSize + 1))),
        paxHdrs: new map<@string, @string>{[paxPath] = strings.Repeat("a", nameSize + 1)},
        formats: (global::go.archive.tar_package.Format)(FormatPAX | FormatGNU)
    ), new(
        header: Ꮡ(new Header(Linkname: "用戶名")),
        paxHdrs: new map<@string, @string>{[paxLinkpath] = "用戶名"},
        formats: (global::go.archive.tar_package.Format)(FormatPAX | FormatGNU)
    ), new(
        header: Ꮡ(new Header(Linkname: strings.Repeat("用戶名\x00", nameSize))),
        paxHdrs: new map<@string, @string>{[paxLinkpath] = strings.Repeat("用戶名\x00", nameSize)},
        formats: FormatUnknown
    ), new(
        header: Ꮡ(new Header(Linkname: "\x00hello")),
        paxHdrs: new map<@string, @string>{[paxLinkpath] = "\x00hello"},
        formats: FormatUnknown
    ), new(
        header: Ꮡ(new Header(Uid: 2097151)),
        formats: (global::go.archive.tar_package.Format)((global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX) | FormatGNU)
    ), new(
        header: Ꮡ(new Header(Uid: 2097151 + 1)),
        paxHdrs: new map<@string, @string>{[paxUid] = "2097152"},
        formats: (global::go.archive.tar_package.Format)(FormatPAX | FormatGNU)
    ), new(
        header: Ꮡ(new Header(Xattrs: default)),
        formats: (global::go.archive.tar_package.Format)((global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX) | FormatGNU)
    ), new(
        header: Ꮡ(new Header(Xattrs: new map<@string, @string>{["foo"] = "bar"})),
        paxHdrs: new map<@string, @string>{[paxSchilyXattr + "foo"] = "bar"},
        formats: FormatPAX
    ), new(
        header: Ꮡ(new Header(Xattrs: new map<@string, @string>{["foo"] = "bar"}, Format: FormatGNU)),
        paxHdrs: new map<@string, @string>{[paxSchilyXattr + "foo"] = "bar"},
        formats: FormatUnknown
    ), new(
        header: Ꮡ(new Header(Xattrs: new map<@string, @string>{["用戶名"] = "\x00hello"})),
        paxHdrs: new map<@string, @string>{[paxSchilyXattr + "用戶名"] = "\x00hello"},
        formats: FormatPAX
    ), new(
        header: Ꮡ(new Header(Xattrs: new map<@string, @string>{["foo=bar"] = "baz"})),
        formats: FormatUnknown
    ), new(
        header: Ꮡ(new Header(Xattrs: new map<@string, @string>{["foo"] = ""})),
        paxHdrs: new map<@string, @string>{[paxSchilyXattr + "foo"] = ""},
        formats: FormatPAX
    ), new(
        header: Ꮡ(new Header(ModTime: time.Unix(0, 0))),
        formats: (global::go.archive.tar_package.Format)((global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX) | FormatGNU)
    ), new(
        header: Ꮡ(new Header(ModTime: time.Unix(8589934591L, 0))),
        formats: (global::go.archive.tar_package.Format)((global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX) | FormatGNU)
    ), new(
        header: Ꮡ(new Header(ModTime: time.Unix(8589934592L, 0))),
        paxHdrs: new map<@string, @string>{[paxMtime] = "8589934592"},
        formats: (global::go.archive.tar_package.Format)(FormatPAX | FormatGNU)
    ), new(
        header: Ꮡ(new Header(ModTime: time.Unix(math.MaxInt64, 0))),
        paxHdrs: new map<@string, @string>{[paxMtime] = "9223372036854775807"},
        formats: (global::go.archive.tar_package.Format)(FormatPAX | FormatGNU)
    ), new(
        header: Ꮡ(new Header(ModTime: time.Unix(math.MaxInt64, 0), Format: FormatUSTAR)),
        paxHdrs: new map<@string, @string>{[paxMtime] = "9223372036854775807"},
        formats: FormatUnknown
    ), new(
        header: Ꮡ(new Header(ModTime: time.Unix(-1, 0))),
        paxHdrs: new map<@string, @string>{[paxMtime] = "-1"},
        formats: (global::go.archive.tar_package.Format)(FormatPAX | FormatGNU)
    ), new(
        header: Ꮡ(new Header(ModTime: time.Unix(1, 500))),
        paxHdrs: new map<@string, @string>{[paxMtime] = "1.0000005"},
        formats: (global::go.archive.tar_package.Format)((global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX) | FormatGNU)
    ), new(
        header: Ꮡ(new Header(ModTime: time.Unix(1, 0))),
        formats: (global::go.archive.tar_package.Format)((global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX) | FormatGNU)
    ), new(
        header: Ꮡ(new Header(ModTime: time.Unix(1, 0), Format: FormatPAX)),
        formats: (global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX)
    ), new(
        header: Ꮡ(new Header(ModTime: time.Unix(1, 500), Format: FormatUSTAR)),
        paxHdrs: new map<@string, @string>{[paxMtime] = "1.0000005"},
        formats: FormatUSTAR
    ), new(
        header: Ꮡ(new Header(ModTime: time.Unix(1, 500), Format: FormatPAX)),
        paxHdrs: new map<@string, @string>{[paxMtime] = "1.0000005"},
        formats: FormatPAX
    ), new(
        header: Ꮡ(new Header(ModTime: time.Unix(1, 500), Format: FormatGNU)),
        paxHdrs: new map<@string, @string>{[paxMtime] = "1.0000005"},
        formats: FormatGNU
    ), new(
        header: Ꮡ(new Header(ModTime: time.Unix(-1, 500))),
        paxHdrs: new map<@string, @string>{[paxMtime] = "-0.9999995"},
        formats: (global::go.archive.tar_package.Format)(FormatPAX | FormatGNU)
    ), new(
        header: Ꮡ(new Header(ModTime: time.Unix(-1, 500), Format: FormatGNU)),
        paxHdrs: new map<@string, @string>{[paxMtime] = "-0.9999995"},
        formats: FormatGNU
    ), new(
        header: Ꮡ(new Header(AccessTime: time.Unix(0, 0))),
        paxHdrs: new map<@string, @string>{[paxAtime] = "0"},
        formats: (global::go.archive.tar_package.Format)(FormatPAX | FormatGNU)
    ), new(
        header: Ꮡ(new Header(AccessTime: time.Unix(0, 0), Format: FormatUSTAR)),
        paxHdrs: new map<@string, @string>{[paxAtime] = "0"},
        formats: FormatUnknown
    ), new(
        header: Ꮡ(new Header(AccessTime: time.Unix(0, 0), Format: FormatPAX)),
        paxHdrs: new map<@string, @string>{[paxAtime] = "0"},
        formats: FormatPAX
    ), new(
        header: Ꮡ(new Header(AccessTime: time.Unix(0, 0), Format: FormatGNU)),
        paxHdrs: new map<@string, @string>{[paxAtime] = "0"},
        formats: FormatGNU
    ), new(
        header: Ꮡ(new Header(AccessTime: time.Unix(-123, 0))),
        paxHdrs: new map<@string, @string>{[paxAtime] = "-123"},
        formats: (global::go.archive.tar_package.Format)(FormatPAX | FormatGNU)
    ), new(
        header: Ꮡ(new Header(AccessTime: time.Unix(-123, 0), Format: FormatPAX)),
        paxHdrs: new map<@string, @string>{[paxAtime] = "-123"},
        formats: FormatPAX
    ), new(
        header: Ꮡ(new Header(ChangeTime: time.Unix(123, 456))),
        paxHdrs: new map<@string, @string>{[paxCtime] = "123.000000456"},
        formats: (global::go.archive.tar_package.Format)(FormatPAX | FormatGNU)
    ), new(
        header: Ꮡ(new Header(ChangeTime: time.Unix(123, 456), Format: FormatUSTAR)),
        paxHdrs: new map<@string, @string>{[paxCtime] = "123.000000456"},
        formats: FormatUnknown
    ), new(
        header: Ꮡ(new Header(ChangeTime: time.Unix(123, 456), Format: FormatGNU)),
        paxHdrs: new map<@string, @string>{[paxCtime] = "123.000000456"},
        formats: FormatGNU
    ), new(
        header: Ꮡ(new Header(ChangeTime: time.Unix(123, 456), Format: FormatPAX)),
        paxHdrs: new map<@string, @string>{[paxCtime] = "123.000000456"},
        formats: FormatPAX
    ), new(
        header: Ꮡ(new Header(Name: "foo/", Typeflag: TypeDir)),
        formats: (global::go.archive.tar_package.Format)((global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX) | FormatGNU)
    ), new(
        header: Ꮡ(new Header(Name: "foo/", Typeflag: TypeReg)),
        formats: FormatUnknown
    ), new(
        header: Ꮡ(new Header(Name: "foo/", Typeflag: TypeSymlink)),
        formats: (global::go.archive.tar_package.Format)((global::go.archive.tar_package.Format)(FormatUSTAR | FormatPAX) | FormatGNU)
    )
    ]);

    foreach (var (i, v) in range(vectors)) {
        var (formats, paxHdrs, err) = v.header.allowedFormats();
        if (formats != v.formats) {
            t.Errorf("test %d, allowedFormats(): got %v, want %v", i, formats, v.formats);
        }
        if ((global::go.archive.tar_package.Format)(formats & FormatPAX) > 0 && !maps.Equal<map<@string, @string>, map<@string, @string>, @string, @string>(paxHdrs, v.paxHdrs) && !(len(paxHdrs) == 0 && len(v.paxHdrs) == 0)) {
            t.Errorf("test %d, allowedFormats():\ngot  %v\nwant %s", i, paxHdrs, v.paxHdrs);
        }
        if ((formats != FormatUnknown) && (err != nil)) {
            t.Errorf("test %d, unexpected error: %v", i, err);
        }
        if ((formats == FormatUnknown) && (err == nil)) {
            t.Errorf("test %d, got nil-error, want non-nil error", i);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string writerˢ => ref writerˢᵇ;
private static readonly @string writerˢᵇ = "Writer"u8;
internal static ref readonly @string readerˢ => ref readerˢᵇ;
private static readonly @string readerˢᵇ = "Reader"u8;

internal partial struct Benchmark_file /*dyn*/ {
    internal ж<global::go.archive.tar_package.Header> hdr;
    internal slice<byte> body;
}

internal partial struct Benchmark_vectors /*dyn*/ {
    internal @string label;
    internal slice<Benchmark_file> files;
}

public static void Benchmark(ж<testing.B> b) {
    var vectors = slice<Benchmark_vectors>([new(
        "USTAR",
        [new(
            Ꮡ(new Header(Name: "bar", Mode: 416, Size: (int64)3)),
            slice<byte>("foo"u8)
        ), new(
            Ꮡ(new Header(Name: "world", Mode: 416, Size: (int64)5)),
            slice<byte>("hello"u8)
        )
        ]
    ), new(
        "GNU",
        [new(
            Ꮡ(new Header(Name: "bar", Mode: 416, Size: (int64)3, Devmajor: -1)),
            slice<byte>("foo"u8)
        ), new(
            Ꮡ(new Header(Name: "world", Mode: 416, Size: (int64)5, Devmajor: -1)),
            slice<byte>("hello"u8)
        )
        ]
    ), new(
        "PAX",
        [new(
            Ꮡ(new Header(Name: "bar", Mode: 416, Size: (int64)3, Xattrs: new map<@string, @string>{["foo"] = "bar"})),
            slice<byte>("foo"u8)
        ), new(
            Ꮡ(new Header(Name: "world", Mode: 416, Size: (int64)5, Xattrs: new map<@string, @string>{["foo"] = "bar"})),
            slice<byte>("hello"u8)
        )
        ]
    )
    ]);

    var vectorsʗ1 = vectors;
    b.Run(writerˢ, (ж<testing.B> bΔ1) => {
        foreach (var (_, vᴛ1) in range(vectorsʗ1)) {
            ref var v = ref heap(new Benchmark_vectors(), out var Ꮡv);
            v = vᴛ1;

            var vʗ1 = v;
            bΔ1.Run(v.label, (ж<testing.B> bΔ2) => {
                bΔ2.ReportAllocs();
                for (nint i = 0; i < (~bΔ2).N; i++) {
                    // Writing to io.Discard because we want to
                    // test purely the writer code and not bring in disk performance into this.
                    var tw = NewWriter(io.Discard);
                    foreach (var (_, @file) in range(vʗ1.files)) {
                        {
                            var err = tw.WriteHeader(@file.hdr); if (err != nil) {
                                bΔ2.Errorf("unexpected WriteHeader error: %v", err);
                            }
                        }
                        {
                            var (_, err) = tw.Write(@file.body); if (err != nil) {
                                bΔ2.Errorf("unexpected Write error: %v", err);
                            }
                        }
                    }
                    {
                        var err = tw.Close(); if (err != nil) {
                            bΔ2.Errorf("unexpected Close error: %v", err);
                        }
                    }
                }
            });
        }
    });

    var vectorsʗ2 = vectors;
    b.Run(readerˢ, (ж<testing.B> bΔ3) => {
        foreach (var (_, v) in range(vectorsʗ2)) {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            ref var r = ref heap(new bytes.Reader(), out var Ꮡr);

            // Write the archive to a byte buffer.
            var tw = NewWriter(new tar_test_package.bytes_BufferжWriter(Ꮡbuf));
            foreach (var (_, @file) in range(v.files)) {
                tw.WriteHeader(@file.hdr);
                tw.Write(@file.body);
            }
            tw.Close();
            bΔ3.Run(v.label, (ж<testing.B> bΔ4) => {
                bΔ4.ReportAllocs();
                // Read from the byte buffer.
                for (nint i = 0; i < (~bΔ4).N; i++) {
                    Ꮡr.Value.Reset(Ꮡbuf.Value.Bytes());
                    var tr = NewReader(new tar_test_package.bytes_ReaderжReader(Ꮡr));
                    {
                        var (_, err) = tr.Next(); if (err != nil) {
                            bΔ4.Errorf("unexpected Next error: %v", err);
                        }
                    }
                    {
                        var (_, err) = io.Copy(io.Discard, new global::go.archive.tar_package.ReaderжReader(tr)); if (err != nil) {
                            bΔ4.Errorf("unexpected Copy error : %v", err);
                        }
                    }
                }
            });
        }
    });
}

internal static fileInfoNames _ᴛ1ʗ = new fileInfoNames();

internal partial struct fileInfoNames {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string tmpˢ => ref tmpˢᵇ;
private static readonly @string tmpˢᵇ = "tmp"u8;

internal static @string Name(this ref fileInfoNames f) {
    return tmpˢ;
}

internal static int64 Size(this ref fileInfoNames f) {
    return 0;
}

internal static fs.FileMode Mode(this ref fileInfoNames f) {
    return 511;
}

internal static time.Time ModTime(this ref fileInfoNames f) {
    return new time.Time();
}

internal static bool IsDir(this ref fileInfoNames f) {
    return false;
}

internal static any Sys(this ref fileInfoNames f) {
    return default;
}

internal static (@string, error) Uname(this ref fileInfoNames f) {
    return (unameˢ, default);
}

internal static (@string, error) Gname(this ref fileInfoNames f) {
    return (gnameˢ, default);
}

public static void TestFileInfoHeaderUseFileInfoNames(ж<testing.T> t) {
    var info = Ꮡ(new fileInfoNames());
    var (header, err) = FileInfoHeader(new tar_internal_test_package.fileInfoNamesжFileInfo(info), ""u8);
    if (err != nil) {
        t.Fatal(err);
    }
    if (header.Uname != "Uname"u8) {
        t.Fatalf("header.Uname: got %s, want %s", header.Uname, unameˢ);
    }
    if (header.Gname != "Gname"u8) {
        t.Fatalf("header.Gname: got %s, want %s", header.Gname, gnameˢ);
    }
}

} // end tar_internal_test_package
