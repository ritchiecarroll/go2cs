// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.archive;

using bytes = bytes_package;
using flate = compress.flate_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using crc32 = go.hash.crc32_package;
using io = io_package;
using fs = go.io.fs_package;
using rand = math.rand_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using fstest = go.testing.fstest_package;
using time = time_package;
using compress;
using encoding;
using go.hash;
using go.io;
using go.testing;
using math;
using static go.archive.zip_package;

partial class zip_internal_test_package {

// TODO(adg): a more sophisticated test suite
public partial struct WriteTest {
    public @string Name;
    public slice<byte> Data;
    public uint16 Method;
    public fs.FileMode Mode;
}

// large data set in the test
internal static slice<WriteTest> writeTests = new WriteTest[]{
    new(
        Name: "foo"u8,
        Data: slice<byte>("Rabbits, guinea pigs, gophers, marsupial rats, and quolls."u8),
        Method: Store,
        Mode: 438
    ),
    new(
        Name: "bar"u8,
        Data: default!,
        Method: Deflate,
        Mode: 420
    ),
    new(
        Name: "setuid"u8,
        Data: slice<byte>("setuid file"u8),
        Method: Deflate,
        Mode: (fs.FileMode)(493 | fs.ModeSetuid)
    ),
    new(
        Name: "setgid"u8,
        Data: slice<byte>("setgid file"u8),
        Method: Deflate,
        Mode: (fs.FileMode)(493 | fs.ModeSetgid)
    ),
    new(
        Name: "symlink"u8,
        Data: slice<byte>("../link/target"u8),
        Method: Deflate,
        Mode: (fs.FileMode)(493 | fs.ModeSymlink)
    ),
    new(
        Name: "device"u8,
        Data: slice<byte>("device file"u8),
        Method: Deflate,
        Mode: (fs.FileMode)(493 | fs.ModeDevice)
    ),
    new(
        Name: "chardevice"u8,
        Data: slice<byte>("char device file"u8),
        Method: Deflate,
        Mode: (fs.FileMode)((fs.FileMode)(493 | fs.ModeDevice) | fs.ModeCharDevice)
    )
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object randReadFailedˢ = (@string)"rand.Read failed:"u8;

public static void TestWriter(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var largeData = new slice<byte>((1 << (int)(17)));
        {
            var (_, errΔ1) = rand.Read(largeData); if (errΔ1 != default!) {
                Ꮡt.Fatal(randReadFailedˢ, errΔ1);
            }
        }
        writeTests[1].Data = largeData;
        defer(() => {
            writeTests[1].Data = default!;
        }, ref ᒐ);
        // write a zip file
        var buf = @new<bytes.Buffer>();
        var w = NewWriter(new zip_test_package.bytes_BufferжWriter(buf));
        foreach (var (_, vᴛ1) in writeTests) {
            ref var wt = ref heap(new WriteTest(), out var Ꮡwt);
            wt = vᴛ1;

            testCreate(Ꮡt, w, Ꮡwt);
        }
        {
            var errΔ2 = w.Close(); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        // read it back
        var (r, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(buf.Bytes())), (int64)buf.Len());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        foreach (var (i, vᴛ2) in writeTests) {
            ref var wt = ref heap(new WriteTest(), out var Ꮡwt);
            wt = vᴛ2;

            testReadFile(Ꮡt, (~r).File[i], Ꮡwt);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestWriterComment_tests /*dyn*/ {
    internal @string comment;
    internal bool ok;
}

// TestWriterComment is test for EOCD comment read/write.
public static void TestWriterComment(ж<testing.T> Ꮡt) {
    var tests = new TestWriterComment_tests[]{
        new("hi, hello"u8, true),
        new("hi, こんにちわ"u8, true),
        new(strings.Repeat("a"u8, uint16max), true),
        new(strings.Repeat("a"u8, uint16max + 1), false)
    }.slice();
    foreach (var (_, test) in tests) {
        // write a zip file
        var buf = @new<bytes.Buffer>();
        var w = NewWriter(new zip_test_package.bytes_BufferжWriter(buf));
        {
            var errΔ1 = w.SetComment(test.comment); if (errΔ1 != default!){
                if (test.ok) {
                    Ꮡt.Fatalf("SetComment: unexpected error %v"u8, errΔ1);
                }
                continue;
            } else {
                if (!test.ok) {
                    Ꮡt.Fatalf("SetComment: unexpected success, want error"u8);
                }
            }
        }
        {
            var errΔ2 = w.Close(); if (test.ok == (errΔ2 != default!)) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        if ((~w).closed != test.ok) {
            Ꮡt.Fatalf("Writer.closed: got %v, want %v"u8, (~w).closed, test.ok);
        }
        // skip read test in failure cases
        if (!test.ok) {
            continue;
        }
        // read it back
        var (r, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(buf.Bytes())), (int64)buf.Len());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if ((~r).Comment != test.comment) {
            Ꮡt.Fatalf("Reader.Comment: got %v, want %v"u8, (~r).Comment, test.comment);
        }
    }
}

internal partial struct TestWriterUTF8_utf8Tests /*dyn*/ {
    internal @string name;
    internal @string comment;
    internal bool nonUTF8;
    internal uint16 flags;
}

public static void TestWriterUTF8(ж<testing.T> Ꮡt) {
    var utf8Tests = new TestWriterUTF8_utf8Tests[]{
        new(
            name: "hi, hello"u8,
            comment: "in the world"u8,
            flags: 0x8
        ),
        new(
            name: "hi, こんにちわ"u8,
            comment: "in the world"u8,
            flags: 0x808
        ),
        new(
            name: "hi, こんにちわ"u8,
            comment: "in the world"u8,
            nonUTF8: true,
            flags: 0x8
        ),
        new(
            name: "hi, hello"u8,
            comment: "in the 世界"u8,
            flags: 0x808
        ),
        new(
            name: "hi, こんにちわ"u8,
            comment: "in the 世界"u8,
            flags: 0x808
        ),
        new(
            name: "the replacement rune is �"u8,
            comment: "the replacement rune is �"u8,
            flags: 0x808
        ),
        new(
            name: ((@string)(new byte[]{0x93, 0xfa, 0x96, 0x7b, 0x8c, 0xea, 0x2e, 0x74, 0x78, 0x74})), // Name is Japanese encoded in Shift JIS.

            comment: "in the 世界"u8,
            flags: 0x008 // UTF-8 must not be set

        )
    }.slice();
    // write a zip file
    var buf = @new<bytes.Buffer>();
    var w = NewWriter(new zip_test_package.bytes_BufferжWriter(buf));
    foreach (var (_, test) in utf8Tests) {
        var h = Ꮡ(new FileHeader(
            Name: test.name,
            Comment: test.comment,
            NonUTF8: test.nonUTF8,
            Method: Deflate
        ));
        var (wΔ1, errΔ1) = w.CreateHeader(h);
        if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
        wΔ1.Write(new byte[]{}.slice());
    }
    {
        var errΔ2 = w.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
    }
    // read it back
    var (r, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(buf.Bytes())), (int64)buf.Len());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (i, test) in utf8Tests) {
        var flags = (~r).File[i].Value.Flags;
        if (flags != test.flags) {
            Ꮡt.Errorf("CreateHeader(name=%q comment=%q nonUTF8=%v): flags=%#x, want %#x"u8, test.name, test.comment, test.nonUTF8, flags, test.flags);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTimeGoZipˢ = "testdata/time-go.zip"u8;
internal static readonly object contentsOfTimeGoZipˢ = (@string)"contents of time-go.zip differ"u8;

public static void TestWriterTime(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var h = Ꮡ(new FileHeader(
        Name: "test.txt"u8,
        Modified: time.Date(2017, 10, 31, 21, 11, 57, 0, timeZone((time.Duration)(-25200000000000L)))
    ));
    var w = NewWriter(new zip_test_package.bytes_BufferжWriter(Ꮡbuf));
    {
        var (_, errΔ1) = w.CreateHeader(h); if (errΔ1 != default!) {
            Ꮡt.Fatalf("unexpected CreateHeader error: %v"u8, errΔ1);
        }
    }
    {
        var errΔ2 = w.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatalf("unexpected Close error: %v"u8, errΔ2);
        }
    }
    var (want, err) = os.ReadFile(testdataTimeGoZipˢ);
    if (err != default!) {
        Ꮡt.Fatalf("unexpected ReadFile error: %v"u8, err);
    }
    {
        var got = buf.Bytes(); if (!bytes.Equal(got, want)) {
            fmt.Printf("%x\n%x\n"u8, got, want);
            Ꮡt.Error(contentsOfTimeGoZipˢ);
        }
    }
}

public static void TestWriterOffset(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var largeData = new slice<byte>((1 << (int)(17)));
        {
            var (_, errΔ1) = rand.Read(largeData); if (errΔ1 != default!) {
                Ꮡt.Fatal(randReadFailedˢ, errΔ1);
            }
        }
        writeTests[1].Data = largeData;
        defer(() => {
            writeTests[1].Data = default!;
        }, ref ᒐ);
        // write a zip file
        var buf = @new<bytes.Buffer>();
        var existingData = new byte[]{1, 2, 3, 1, 2, 3, 1, 2, 3}.slice();
        var (n, _) = buf.Write(existingData);
        var w = NewWriter(new zip_test_package.bytes_BufferжWriter(buf));
        w.SetOffset((int64)n);
        foreach (var (_, vᴛ1) in writeTests) {
            ref var wt = ref heap(new WriteTest(), out var Ꮡwt);
            wt = vᴛ1;

            testCreate(Ꮡt, w, Ꮡwt);
        }
        {
            var errΔ2 = w.Close(); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
        // read it back
        var (r, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(buf.Bytes())), (int64)buf.Len());
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        foreach (var (i, vᴛ2) in writeTests) {
            ref var wt = ref heap(new WriteTest(), out var Ꮡwt);
            wt = vᴛ2;

            testReadFile(Ꮡt, (~r).File[i], Ꮡwt);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;
internal static readonly object noBytesWrittenAfterFlushˢ = (@string)"No bytes written after Flush"u8;

internal partial struct TestWriterFlush_w /*dyn*/ {
    /*embed*/ public io_package.Writer Writer;
}

public static void TestWriterFlush(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var w = NewWriter(new TestWriterFlush_w(new zip_test_package.bytes_BufferжWriter(Ꮡbuf)));
    var (_, err) = w.Create(fooˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (buf.Len() > 0) {
        Ꮡt.Fatalf("Unexpected %d bytes already in buffer"u8, buf.Len());
    }
    {
        var errΔ1 = w.Flush(); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    if (buf.Len() == 0) {
        Ꮡt.Fatal(noBytesWrittenAfterFlushˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string dirˢ = "dir/"u8;
internal static readonly object writeHelloToDirectoryGotˢ = (@string)@"Write(""hello"") to directory: got nil error, want non-nil"u8;

public static void TestWriterDir(ж<testing.T> Ꮡt) {
    var w = NewWriter(io.Discard);
    var (dw, err) = w.Create(dirˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var (_, errΔ1) = dw.Write(default!); if (errΔ1 != default!) {
            Ꮡt.Errorf("Write(nil) to directory: got %v, want nil"u8, errΔ1);
        }
    }
    {
        var (_, errΔ2) = dw.Write(slice<byte>("hello"u8)); if (errΔ2 == default!) {
            Ꮡt.Error(writeHelloToDirectoryGotˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object fileHeaderNotFoundˢ = (@string)"file header not found"u8;
internal static readonly object thereShouldBeNoDataˢ = (@string)"there should be no data descriptor"u8;

public static void TestWriterDirAttributes(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var w = NewWriter(new zip_test_package.bytes_BufferжWriter(Ꮡbuf));
    {
        var (_, err) = w.CreateHeader(Ꮡ(new FileHeader(
            Name: "dir/"u8,
            Method: Deflate,
            CompressedSize64: 1234,
            UncompressedSize64: 5678
        ))); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = w.Close(); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    var b = buf.Bytes();
    array<byte> sig = new(4);
    binary.LittleEndian.PutUint32(sig[..], (uint32)fileHeaderSignature);
    nint idx = bytes.Index(b, sig[..]);
    if (idx == -1) {
        Ꮡt.Fatal(fileHeaderNotFoundˢ);
    }
    b = b.slice(idx);
    if (!bytes.Equal(b[6..10], new byte[]{0, 0, 0, 0}.slice())) {
        // FileHeader.Flags: 0, FileHeader.Method: 0
        Ꮡt.Errorf("unexpected method and flags: %v"u8, b[6..10]);
    }
    if (!bytes.Equal(b[14..26], new slice<byte>(12))) {
        // FileHeader.{CRC32,CompressSize,UncompressedSize} all zero.
        Ꮡt.Errorf("unexpected crc, compress and uncompressed size to be 0 was: %v"u8, b[14..26]);
    }
    binary.LittleEndian.PutUint32(sig[..], (uint32)dataDescriptorSignature);
    if (bytes.Contains(b, sig[..])) {
        Ꮡt.Error(thereShouldBeNoDataˢ);
    }
}

public static void TestWriterCopy(ж<testing.T> Ꮡt) {
    // make a zip file
    var buf = @new<bytes.Buffer>();
    var w = NewWriter(new zip_test_package.bytes_BufferжWriter(buf));
    foreach (var (_, vᴛ1) in writeTests) {
        ref var wt = ref heap(new WriteTest(), out var Ꮡwt);
        wt = vᴛ1;

        testCreate(Ꮡt, w, Ꮡwt);
    }
    {
        var errΔ1 = w.Close(); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    // read it back
    var (src, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(buf.Bytes())), (int64)buf.Len());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (i, vᴛ2) in writeTests) {
        ref var wt = ref heap(new WriteTest(), out var Ꮡwt);
        wt = vᴛ2;

        testReadFile(Ꮡt, (~src).File[i], Ꮡwt);
    }
    // make a new zip file copying the old compressed data.
    var buf2 = @new<bytes.Buffer>();
    var dst = NewWriter(new zip_test_package.bytes_BufferжWriter(buf2));
    foreach (var (_, f) in (~src).File) {
        {
            var errΔ2 = dst.Copy(f); if (errΔ2 != default!) {
                Ꮡt.Fatal(errΔ2);
            }
        }
    }
    {
        var errΔ3 = dst.Close(); if (errΔ3 != default!) {
            Ꮡt.Fatal(errΔ3);
        }
    }
    // read the new one back
    (var r, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(buf2.Bytes())), (int64)buf2.Len());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (i, vᴛ3) in writeTests) {
        ref var wt = ref heap(new WriteTest(), out var Ꮡwt);
        wt = vᴛ3;

        testReadFile(Ꮡt, (~r).File[i], Ꮡwt);
    }
}

internal partial struct TestWriterCreateRaw_files /*dyn*/ {
    internal @string name;
    internal slice<byte> content;
    internal uint16 method;
    internal uint16 flags;
    internal uint32 crc32;
    internal uint64 uncompressedSize;
    internal uint64 compressedSize;
}

public static void TestWriterCreateRaw(ж<testing.T> Ꮡt) {
    var files = new TestWriterCreateRaw_files[]{
        new(
            name: "small store w desc"u8,
            content: slice<byte>("gophers"u8),
            method: Store,
            flags: 0x8
        ),
        new(
            name: "small deflate wo desc"u8,
            content: bytes.Repeat(slice<byte>("abcdefg"u8), 2048),
            method: Deflate
        )
    }.slice();
    // write a zip file
    var archive = @new<bytes.Buffer>();
    var w = NewWriter(new zip_test_package.bytes_BufferжWriter(archive));
    foreach (var (i, _) in files) {
        var f = Ꮡ(files, i);
        f.Value.crc32 = crc32.ChecksumIEEE((~f).content);
        var size = (uint64)len((~f).content);
        f.Value.uncompressedSize = size;
        f.Value.compressedSize = size;
        slice<byte> compressedContent = default!;
        if ((~f).method == Deflate) {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            var (wΔ1, errΔ1) = flate.NewWriter(new zip_test_package.bytes_BufferжWriter(Ꮡbuf), flate.BestSpeed);
            if (errΔ1 != default!) {
                Ꮡt.Fatalf("flate.NewWriter err = %v"u8, errΔ1);
            }
            (_, errΔ1) = wΔ1.Write((~f).content);
            if (errΔ1 != default!) {
                Ꮡt.Fatalf("flate Write err = %v"u8, errΔ1);
            }
            errΔ1 = wΔ1.Close();
            if (errΔ1 != default!) {
                Ꮡt.Fatalf("flate Writer.Close err = %v"u8, errΔ1);
            }
            compressedContent = buf.Bytes();
            f.Value.compressedSize = (uint64)len(compressedContent);
        }
        var h = Ꮡ(new FileHeader(
            Name: (~f).name,
            Method: (~f).method,
            Flags: (~f).flags,
            CRC32: (~f).crc32,
            CompressedSize64: (~f).compressedSize,
            UncompressedSize64: (~f).uncompressedSize
        ));
        var (wΔ2, errΔ2) = w.CreateRaw(h);
        if (errΔ2 != default!) {
            Ꮡt.Fatal(errΔ2);
        }
        if (compressedContent != default!){
            (_, errΔ2) = wΔ2.Write(compressedContent);
        } else {
            (_, errΔ2) = wΔ2.Write((~f).content);
        }
        if (errΔ2 != default!) {
            Ꮡt.Fatalf("%s Write got %v; want nil"u8, (~f).name, errΔ2);
        }
    }
    {
        var errΔ3 = w.Close(); if (errΔ3 != default!) {
            Ꮡt.Fatal(errΔ3);
        }
    }
    // read it back
    var (r, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(archive.Bytes())), (int64)archive.Len());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (i, want) in files) {
        var got = (~r).File[i];
        if ((~got).Name != want.name) {
            Ꮡt.Errorf("got Name %s; want %s"u8, (~got).Name, want.name);
        }
        if ((~got).Method != want.method) {
            Ꮡt.Errorf("%s: got Method %#x; want %#x"u8, want.name, (~got).Method, want.method);
        }
        if ((~got).Flags != want.flags) {
            Ꮡt.Errorf("%s: got Flags %#x; want %#x"u8, want.name, (~got).Flags, want.flags);
        }
        if ((~got).CRC32 != want.crc32) {
            Ꮡt.Errorf("%s: got CRC32 %#x; want %#x"u8, want.name, (~got).CRC32, want.crc32);
        }
        if ((~got).CompressedSize64 != want.compressedSize) {
            Ꮡt.Errorf("%s: got CompressedSize64 %d; want %d"u8, want.name, (~got).CompressedSize64, want.compressedSize);
        }
        if ((~got).UncompressedSize64 != want.uncompressedSize) {
            Ꮡt.Errorf("%s: got UncompressedSize64 %d; want %d"u8, want.name, (~got).UncompressedSize64, want.uncompressedSize);
        }
        var (rΔ1, errΔ4) = got.Open();
        if (errΔ4 != default!) {
            Ꮡt.Errorf("%s: Open err = %v"u8, (~got).Name, errΔ4);
            continue;
        }
        (var buf, errΔ4) = io.ReadAll(rΔ1);
        if (errΔ4 != default!) {
            Ꮡt.Errorf("%s: ReadAll err = %v"u8, (~got).Name, errΔ4);
            continue;
        }
        if (!bytes.Equal(buf, want.content)) {
            Ꮡt.Errorf("%v: ReadAll returned unexpected bytes"u8, (~got).Name);
        }
    }
}

internal static void testCreate(ж<testing.T> Ꮡt, ж<global::go.archive.zip_package.Writer> Ꮡw, ж<WriteTest> Ꮡwt) {
    ref var w = ref Ꮡw.DerefOrNull();
    ref var wt = ref Ꮡwt.DerefOrNull();

    var header = Ꮡ(new FileHeader(
        Name: wt.Name,
        Method: wt.Method
    ));
    if (wt.Mode != 0) {
        header.SetMode(wt.Mode);
    }
    var (f, err) = w.CreateHeader(header);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (_, err) = f.Write(wt.Data);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

internal static void testReadFile(ж<testing.T> Ꮡt, ж<global::go.archive.zip_package.File> Ꮡf, ж<WriteTest> Ꮡwt) {
    ref var f = ref Ꮡf.DerefOrNull();
    ref var wt = ref Ꮡwt.DerefOrNull();

    if (f.Name != wt.Name) {
        Ꮡt.Fatalf("File name: got %q, want %q"u8, f.Name, wt.Name);
    }
    testFileMode(Ꮡt, Ꮡf, wt.Mode);
    var (rc, err) = Ꮡf.Open();
    if (err != default!) {
        Ꮡt.Fatalf("opening %s: %v"u8, f.Name, err);
    }
    (var b, err) = io.ReadAll(rc);
    if (err != default!) {
        Ꮡt.Fatalf("reading %s: %v"u8, f.Name, err);
    }
    err = rc.Close();
    if (err != default!) {
        Ꮡt.Fatalf("closing %s: %v"u8, f.Name, err);
    }
    if (!bytes.Equal(b, wt.Data)) {
        Ꮡt.Errorf("File contents %q, want %q"u8, b, wt.Data);
    }
}

public static void BenchmarkCompressedZipGarbage(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var bigBuf = bytes.Repeat(slice<byte>("a"u8), (1 << (int)(20)));
    var bigBufʗ1 = bigBuf;
    void runOnce(ж<bytes.Buffer> buf) {
        buf.Reset();
        var zw = NewWriter(new zip_test_package.bytes_BufferжWriter(buf));
        for (nint j = 0; j < 3; j++) {
            var (w, _) = zw.CreateHeader(Ꮡ(new FileHeader(
                Name: "foo"u8,
                Method: Deflate
            )));
            w.Write(bigBufʗ1);
        }
        zw.Close();
    }
    b.ReportAllocs();
    // Run once and then reset the timer.
    // This effectively discards the very large initial flate setup cost,
    // as well as the initialization of bigBuf.
    runOnce(Ꮡ(new bytes.Buffer(nil)));
    b.ResetTimer();
    var runOnceʗ1 = runOnce;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        while (pb.Next()) {
            runOnceʗ1(Ꮡbuf);
        }
    });
}

internal static fs.FS writeTestsToFS(slice<WriteTest> tests) {
    var fsys = new fstest.MapFS(new map<@string, ж<fstest.MapFile>>{});
    foreach (var (_, wt) in tests) {
        fsys[wt.Name] = Ꮡ(new fstest.MapFile(
            Data: wt.Data,
            Mode: wt.Mode
        ));
    }
    return fsys;
}

public static void TestWriterAddFS(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var buf = @new<bytes.Buffer>();
    var w = NewWriter(new zip_test_package.bytes_BufferжWriter(buf));
    var tests = new WriteTest[]{
        new(Name: "emptyfolder"u8, Mode: (fs.FileMode)(493 | os.ModeDir)),
        new(Name: "file.go"u8, Data: slice<byte>("hello"u8), Mode: 420),
        new(Name: "subfolder/another.go"u8, Data: slice<byte>("world"u8), Mode: 420)
    }.slice();
    // Notably missing here is the "subfolder" directory. This makes sure even
    // if we don't have a subfolder directory listed.
    var err = w.AddFS(writeTestsToFS(tests));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var errΔ1 = w.Close(); if (errΔ1 != default!) {
            Ꮡt.Fatal(errΔ1);
        }
    }
    // Add subfolder into fsys to match what we'll read from the zip.
    tests = append(tests.slice(0, 2, 2), new WriteTest(Name: "subfolder"u8, Mode: (fs.FileMode)(365 | os.ModeDir)), tests[2]);
    // read it back
    (var r, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(buf.Bytes())), (int64)buf.Len());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    foreach (var (i, vᴛ1) in tests) {
        ref var wt = ref heap(new WriteTest(), out var Ꮡwt);
        wt = vᴛ1;

        if (wt.Mode.IsDir()) {
            wt.Name += "/"u8;
        }
        testReadFile(Ꮡt, (~r).File[i], Ꮡwt);
    }
}

public static void TestIssue61875(ж<testing.T> Ꮡt) {
    var buf = @new<bytes.Buffer>();
    var w = NewWriter(new zip_test_package.bytes_BufferжWriter(buf));
    var tests = new WriteTest[]{
        new(
            Name: "symlink"u8,
            Data: slice<byte>("../link/target"u8),
            Method: Deflate,
            Mode: (fs.FileMode)(493 | fs.ModeSymlink)
        ),
        new(
            Name: "device"u8,
            Data: slice<byte>(""u8),
            Method: Deflate,
            Mode: (fs.FileMode)(493 | fs.ModeDevice)
        )
    }.slice();
    var err = w.AddFS(writeTestsToFS(tests));
    if (err == default!) {
        Ꮡt.Errorf("expected error, got nil"u8);
    }
}

} // end zip_internal_test_package
