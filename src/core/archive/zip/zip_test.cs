// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Tests that involve both reading and writing.
namespace go.archive;

using bytes = bytes_package;
using cmp = cmp_package;
using errors = errors_package;
using fmt = fmt_package;
using hash = hash_package;
using testenv = @internal.testenv_package;
using io = io_package;
using runtime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using @internal;
using fs = go.io.fs_package;
using go.io;
using static go.archive.zip_package;

partial class zip_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;

public static void TestOver65kFiles(ж<testing.T> Ꮡt) {
    if (testing.Short() && testenv.Builder() == ""u8) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    var buf = @new<strings.Builder>();
    var w = NewWriter(new zip_test_package.strings_BuilderжWriter(buf));
    const nint nFiles = /* (1 << 16) + 42 */ 65578;
    for (nint i = 0; i < nFiles; i++) {
        var (_, errΔ1) = w.CreateHeader(Ꮡ(new FileHeader(
            Name: fmt.Sprintf("%d.dat"u8, i),
            Method: Store
        )));
        // Deflate is too slow when it is compiled with -race flag
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("creating file %d: %v"u8, i, errΔ1);
        }
    }
    {
        var errΔ2 = w.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatalf("Writer.Close: %v"u8, errΔ2);
        }
    }
    @string s = buf.String();
    var (zr, err) = NewReader(new zip_test_package.strings_ReaderжReaderAt(strings.NewReader(s)), (int64)len(s));
    if (err != default!) {
        Ꮡt.Fatalf("NewReader: %v"u8, err);
    }
    {
        nint got = len((~zr).File); if (got != nFiles) {
            Ꮡt.Fatalf("File contains %d files, want %d"u8, got, (nint)(nFiles));
        }
    }
    for (nint i = 0; i < nFiles; i++) {
        @string want = fmt.Sprintf("%d.dat"u8, i);
        if ((~(~zr).File[i]).Name != want) {
            Ꮡt.Fatalf("File(%d) = %q, want %q"u8, i, (~(~zr).File[i]).Name, want);
        }
    }
}

public static void TestModTime(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    time.Time testTime = time.Date(2009, time.November, 10, 23, 45, 58, 0, time.ΔUTC);
    var fh = @new<global::go.archive.zip_package.FileHeader>();
    fh.SetModTime(testTime);
    var outTime = fh.ModTime();
    if (!outTime.Equal(testTime)) {
        Ꮡt.Errorf("times don't match: got %s, want %s"u8, outTime, testTime);
    }
}

internal static void testHeaderRoundTrip(ж<global::go.archive.zip_package.FileHeader> Ꮡfh, uint32 wantUncompressedSize, uint64 wantUncompressedSize64, ж<testing.T> Ꮡt) {
    ref var fh = ref Ꮡfh.DerefOrNull();
    ref var t = ref Ꮡt.DerefOrNull();

    var fi = Ꮡfh.FileInfo();
    var (fh2, err) = FileInfoHeader(fi);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        @string got = fh2.Value.Name;
        @string want = fh.Name; if (got != want) {
            Ꮡt.Errorf("Name: got %s, want %s\n"u8, got, want);
        }
    }
    {
        var (got, want) = (fh2.Value.UncompressedSize, wantUncompressedSize); if (got != want) {
            Ꮡt.Errorf("UncompressedSize: got %d, want %d\n"u8, got, want);
        }
    }
    {
        var (got, want) = (fh2.Value.UncompressedSize64, wantUncompressedSize64); if (got != want) {
            Ꮡt.Errorf("UncompressedSize64: got %d, want %d\n"u8, got, want);
        }
    }
    {
        var (got, want) = (fh2.Value.ModifiedTime, fh.ModifiedTime); if (got != want) {
            Ꮡt.Errorf("ModifiedTime: got %d, want %d\n"u8, got, want);
        }
    }
    {
        var (got, want) = (fh2.Value.ModifiedDate, fh.ModifiedDate); if (got != want) {
            Ꮡt.Errorf("ModifiedDate: got %d, want %d\n"u8, got, want);
        }
    }
    {
        var (sysfh, ok) = fi.Sys()._<ж<global::go.archive.zip_package.FileHeader>>(ᐧ); if (!ok && sysfh != Ꮡfh) {
            Ꮡt.Errorf("Sys didn't return original *FileHeader"u8);
        }
    }
}

public static void TestFileHeaderRoundTrip(ж<testing.T> Ꮡt) {
    var fh = Ꮡ(new FileHeader(
        Name: "foo.txt"u8,
        UncompressedSize: 987654321,
        ModifiedTime: 1234,
        ModifiedDate: 5678
    ));
    testHeaderRoundTrip(fh, (~fh).UncompressedSize, (uint64)(~fh).UncompressedSize, Ꮡt);
}

public static void TestFileHeaderRoundTrip64(ж<testing.T> Ꮡt) {
    var fh = Ꮡ(new FileHeader(
        Name: "foo.txt"u8,
        UncompressedSize64: 9876543210UL,
        ModifiedTime: 1234,
        ModifiedDate: 5678
    ));
    testHeaderRoundTrip(fh, uint32max, (~fh).UncompressedSize64, Ꮡt);
}

public static void TestFileHeaderRoundTripModified(ж<testing.T> Ꮡt) {
    var fh = Ꮡ(new FileHeader(
        Name: "foo.txt"u8,
        UncompressedSize: 987654321,
        Modified: time.Now().Local(),
        ModifiedTime: 1234,
        ModifiedDate: 5678
    ));
    var fi = fh.FileInfo();
    var (fh2, err) = FileInfoHeader(fi);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var (got, want) = (fh2.Value.Modified, (~fh).Modified.UTC()); if (got != want) {
            Ꮡt.Errorf("Modified: got %s, want %s\n"u8, got, want);
        }
    }
    {
        var (got, want) = (fi.ModTime(), (~fh).Modified.UTC()); if (got != want) {
            Ꮡt.Errorf("Modified: got %s, want %s\n"u8, got, want);
        }
    }
}

public static void TestFileHeaderRoundTripWithoutModified(ж<testing.T> Ꮡt) {
    var fh = Ꮡ(new FileHeader(
        Name: "foo.txt"u8,
        UncompressedSize: 987654321,
        ModifiedTime: 1234,
        ModifiedDate: 5678
    ));
    var fi = fh.FileInfo();
    var (fh2, err) = FileInfoHeader(fi);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    {
        var (got, want) = (fh2.ModTime(), fh.ModTime()); if (got != want) {
            Ꮡt.Errorf("Modified: got %s, want %s\n"u8, got, want);
        }
    }
    {
        var (got, want) = (fi.ModTime(), fh.ModTime()); if (got != want) {
            Ꮡt.Errorf("Modified: got %s, want %s\n"u8, got, want);
        }
    }
}

[GoType] internal partial struct repeatedByte {
    internal int64 off;
    internal byte b;
    internal int64 n;
}

// rleBuffer is a run-length-encoded byte buffer.
// It's an io.Writer (like a bytes.Buffer) and also an io.ReaderAt,
// allowing random-access reads.
[GoType] internal partial struct rleBuffer {
    internal slice<repeatedByte> buf;
}

[GoRecv] internal static int64 Size(this ref rleBuffer r) {
    if (len(r.buf) == 0) {
        return 0;
    }
    var last = Ꮡ(r.buf, len(r.buf) - 1);
    return (~last).off + (~last).n;
}

[GoRecv] internal static (nint n, error err) Write(this ref rleBuffer r, slice<byte> p) {
    ж<repeatedByte> rp = default!;
    if (len(r.buf) > 0) {
        rp = Ꮡ(r.buf, len(r.buf) - 1);
        // Fast path, if p is entirely the same byte repeated.
        {
            var lastByte = rp.Value.b; if (len(p) > 0 && p[0] == lastByte) {
                if (bytes.Count(p, new byte[]{lastByte}.slice()) == len(p)) {
                    rp.Value.n += (int64)len(p);
                    return (len(p), default!);
                }
            }
        }
    }
    foreach (var (_, b) in p) {
        if (rp == nil || (~rp).b != b){
            r.buf = append(r.buf, new repeatedByte(r.Size(), b, 1));
            rp = Ꮡ(r.buf, len(r.buf) - 1);
        } else {
            rp.Value.n++;
        }
    }
    return (len(p), default!);
}

internal static void memset(slice<byte> a, byte b) {
    if (len(a) == 0) {
        return;
    }
    // Double, until we reach power of 2 >= len(a), same as bytes.Repeat,
    // but without allocation.
    a[0] = b;
    for ((nint i, nint l) = (1, len(a)); i < l; i *= 2) {
        copy(a.slice(i), a.slice(0, i));
    }
}

[GoRecv] internal static (nint n, error err) ReadAt(this ref rleBuffer r, slice<byte> p, int64 off) {
    nint n = default!;
    error err = default!;

    if (len(p) == 0) {
        return (n, err);
    }
    var (skipParts, _) = slices.BinarySearchFunc(r.buf, off, (repeatedByte rb, int64 offΔ1) => cmp.Compare(rb.off + rb.n, offΔ1));
    var parts = r.buf.slice(skipParts);
    if (len(parts) > 0) {
        var skipBytes = off - parts[0].off;
        foreach (var (_, part) in parts) {
            nint repeat = (nint)min(part.n - skipBytes, (int64)(len(p) - n));
            memset(p.slice(n, n + repeat), part.b);
            n += repeat;
            if (n == len(p)) {
                return (n, err);
            }
            skipBytes = 0;
        }
    }
    if (n != len(p)) {
        err = io.ErrUnexpectedEOF;
    }
    return (n, err);
}

// Just testing the rleBuffer used in the Zip64 test above. Not used by the zip code.
public static void TestRLEBuffer(ж<testing.T> Ꮡt) {
    var b = @new<rleBuffer>();
    slice<byte> all = default!;
    var writes = new @string[]{"abcdeee"u8, "eeeeeee"u8, "eeeefghaaiii"u8}.slice();
    foreach (var (_, w) in writes) {
        b.Write(slice<byte>(w));
        all = append(all, w.ꓸꓸꓸ);
    }
    if (len((~b).buf) != 10) {
        Ꮡt.Fatalf("len(b.buf) = %d; want 10"u8, len((~b).buf));
    }
    for (nint i = 0; i < len(all); i++) {
        for (nint j = 0; j < len(all) - i; j++) {
            var buf = new slice<byte>(j);
            var (n, err) = b.ReadAt(buf, (int64)i);
            if (err != default! || n != len(buf)) {
                Ꮡt.Errorf("ReadAt(%d, %d) = %d, %v; want %d, nil"u8, i, j, n, err, len(buf));
            }
            if (!bytes.Equal(buf, all.slice(i, i + j))) {
                Ꮡt.Errorf("ReadAt(%d, %d) = %q; want %q"u8, i, j, buf, all.slice(i, i + j));
            }
        }
    }
}

// fakeHash32 is a dummy Hash32 that always returns 0.
[GoType] internal partial struct fakeHash32 {
    [GoEmbedded] public hash_package.Hash32 Hash32;
}

internal static (nint, error) Write(this fakeHash32 _, slice<byte> p) {
    return (len(p), default!);
}

internal static uint32 Sum32(this fakeHash32 _) {
    return 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object slowTestSkippingˢ = (@string)"slow test; skipping"u8;

public static void TestZip64(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(slowTestSkippingˢ);
    }
    Ꮡt.Parallel();
    const int64 size = /* 1 << 32 */ 4294967296; // before the "END\n" part
    var buf = testZip64(new zip_test_package.testing_TжTB(Ꮡt), size);
    testZip64DirectoryRecordLength(buf, Ꮡt);
}

public static void TestZip64EdgeCase(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(slowTestSkippingˢ);
    }
    Ꮡt.Parallel();
    // Test a zip file with uncompressed size 0xFFFFFFFF.
    // That's the magic marker for a 64-bit file, so even though
    // it fits in a 32-bit field we must use the 64-bit field.
    // Go 1.5 and earlier got this wrong,
    // writing an invalid zip file.
    const int64 size = /* 1<<32 - 1 - int64(len("END\n")) */ 4294967291; // before the "END\n" part
    var buf = testZip64(new zip_test_package.testing_TжTB(Ꮡt), size);
    testZip64DirectoryRecordLength(buf, Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string uint32max2NoZip64ˢ = "uint32max-2_NoZip64"u8;
internal static readonly object unexpectedZip64ˢ = (@string)"unexpected zip64"u8;
internal static readonly @string uint32max1Zip64ˢ = "uint32max-1_Zip64"u8;
internal static readonly object expectedZip64ˢ = (@string)"expected zip64"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string filenameᶜ = "huge.txt"u8;

// Tests that we generate a zip64 file if the directory at offset
// 0xFFFFFFFF, but not before.
public static void TestZip64DirectoryOffset(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    Ꮡt.Parallel();
    @string filename = filenameᶜ;
    Action<ж<global::go.archive.zip_package.Writer>> gen(uint64 wantOff) => (ж<global::go.archive.zip_package.Writer> w) => {
            w.Value.testHookCloseSizeOffset = (uint64 sizeΔ1, uint64 off) => {
                if (off != wantOff) {
                    Ꮡt.Errorf("central directory offset = %d (%x); want %d"u8, off, off, wantOff);
                }
            };
            var (f, err) = w.CreateHeader(Ꮡ(new FileHeader(
                Name: filename,
                Method: Store
            )));
            if (err != default!) {
                Ꮡt.Fatal(err);
            }
            f._<ж<global::go.archive.zip_package.fileWriter>>().Value.crc32 = new fakeHash32(nil);
            var size = wantOff - (uint64)fileHeaderLen - (uint64)len(filename) - (uint64)dataDescriptorLen;
            {
                var (_, errΔ1) = io.CopyN(f, new zeros(nil), (int64)size); if (errΔ1 != default!) {
                    Ꮡt.Fatal(errΔ1);
                }
            }
            {
                var errΔ2 = w.Close(); if (errΔ2 != default!) {
                    Ꮡt.Fatal(errΔ2);
                }
            }
        };
    var genʗ1 = gen;
    Ꮡt.Run(uint32max2NoZip64ˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Parallel();
        if (generatesZip64(tΔ1, genʗ1(0xfffffffeU))) {
            tΔ1.Error(unexpectedZip64ˢ);
        }
    });
    var genʗ2 = gen;
    Ꮡt.Run(uint32max1Zip64ˢ, (ж<testing.T> tΔ2) => {
        tΔ2.Parallel();
        if (!generatesZip64(tΔ2, genʗ2(0xffffffffU))) {
            tΔ2.Error(expectedZip64ˢ);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string uint16max1NoZip64ˢ = "uint16max-1_NoZip64"u8;
internal static readonly @string uint16maxZip64ˢ = "uint16max_Zip64"u8;

// At 16k records, we need to generate a zip64 file.
public static void TestZip64ManyRecords(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    Ꮡt.Parallel();
    Action<ж<global::go.archive.zip_package.Writer>> gen(nint numRec) => (ж<global::go.archive.zip_package.Writer> w) => {
            for (nint i = 0; i < numRec; i++) {
                var (_, err) = w.CreateHeader(Ꮡ(new FileHeader(
                    Name: "a.txt"u8,
                    Method: Store
                )));
                if (err != default!) {
                    Ꮡt.Fatal(err);
                }
            }
            {
                var err = w.Close(); if (err != default!) {
                    Ꮡt.Fatal(err);
                }
            }
        };
    // 16k-1 records shouldn't make a zip64:
    var genʗ1 = gen;
    Ꮡt.Run(uint16max1NoZip64ˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Parallel();
        if (generatesZip64(tΔ1, genʗ1(0xfffe))) {
            tΔ1.Error(unexpectedZip64ˢ);
        }
    });
    // 16k records should make a zip64:
    var genʗ2 = gen;
    Ꮡt.Run(uint16maxZip64ˢ, (ж<testing.T> tΔ2) => {
        tΔ2.Parallel();
        if (!generatesZip64(tΔ2, genʗ2(0xffff))) {
            tΔ2.Error(expectedZip64ˢ);
        }
    });
}

// suffixSaver is an io.Writer & io.ReaderAt that remembers the last 0
// to 'keep' bytes of data written to it. Call Suffix to get the
// suffix bytes.
[GoType] internal partial struct suffixSaver {
    internal nint keep;
    internal slice<byte> buf;
    internal nint start;
    internal int64 size;
}

[GoRecv] internal static int64 Size(this ref suffixSaver ss) {
    return ss.size;
}

internal static error errDiscardedBytes = errors.New("ReadAt of discarded bytes"u8);

[GoRecv] internal static (nint n, error err) ReadAt(this ref suffixSaver ss, slice<byte> p, int64 off) {
    nint n = default!;
    error err = default!;

    var back = ss.size - off;
    if (back > (int64)ss.keep) {
        return (0, errDiscardedBytes);
    }
    var suf = ss.Suffix();
    n = copy(p, suf.slice(len(suf) - (nint)back));
    if (n != len(p)) {
        err = io.EOF;
    }
    return (n, err);
}

[GoRecv] internal static slice<byte> Suffix(this ref suffixSaver ss) {
    if (len(ss.buf) < ss.keep) {
        return ss.buf;
    }
    var buf = new slice<byte>(ss.keep);
    nint n = copy(buf, ss.buf.slice(ss.start));
    copy(buf.slice(n), ss.buf[..]);
    return buf;
}

[GoRecv] internal static (nint n, error err) Write(this ref suffixSaver ss, slice<byte> p) {
    nint n = default!;
    error err = default!;

    n = len(p);
    ss.size += (int64)len(p);
    if (len(ss.buf) < ss.keep) {
        nint space = ss.keep - len(ss.buf);
        nint add = len(p);
        if (add > space) {
            add = space;
        }
        ss.buf = appendꓸꓸꓸ(ss.buf, p.slice(0, add));
        p = p.slice(add);
    }
    while (len(p) > 0) {
        nint nΔ1 = copy(ss.buf.slice(ss.start), p);
        p = p.slice(nΔ1);
        ss.start += nΔ1;
        if (ss.start == ss.keep) {
            ss.start = 0;
        }
    }
    return (n, err);
}

// generatesZip64 reports whether f wrote a zip64 file.
// f is also responsible for closing w.
internal static bool generatesZip64(ж<testing.T> Ꮡt, Action<ж<global::go.archive.zip_package.Writer>> f) {
    var ss = Ꮡ(new suffixSaver(keep: (10 << (int)(20))));
    var w = NewWriter(new zip_internal_test_package.suffixSaverжWriter(ss));
    f(w);
    return suffixIsZip64(Ꮡt, new zip_internal_test_package.suffixSaverжsizedReaderAt(ss));
}

[GoType] internal partial interface sizedReaderAt :
    io.ReaderAt
{
    int64 Size();
}

internal static bool suffixIsZip64(ж<testing.T> Ꮡt, sizedReaderAt zip) {
    var d = new slice<byte>(1024);
    {
        var (_, errΔ1) = zip.ReadAt(d, zip.Size() - (int64)len(d)); if (errΔ1 != default!) {
            Ꮡt.Fatalf("ReadAt: %v"u8, errΔ1);
        }
    }
    nint sigOff = findSignatureInBlock(d);
    if (sigOff == -1) {
        Ꮡt.Errorf("failed to find signature in block"u8);
        return false;
    }
    var (dirOff, err) = findDirectory64End(zip, zip.Size() - (int64)len(d) + (int64)sigOff);
    if (err != default!) {
        Ꮡt.Fatalf("findDirectory64End: %v"u8, err);
    }
    if (dirOff == -1) {
        return false;
    }
    d = new slice<byte>(directory64EndLen);
    {
        var (_, errΔ2) = zip.ReadAt(d, dirOff); if (errΔ2 != default!) {
            Ꮡt.Fatalf("ReadAt(off=%d): %v"u8, dirOff, errΔ2);
        }
    }
    var b = ((global::go.archive.zip_package.readBuf)d);
    {
        var sig = b.uint32(); if (sig != directory64EndSignature) {
            return false;
        }
    }
    var size = b.uint64();
    if (size != (uint64)(directory64EndLen - 12)) {
        Ꮡt.Errorf("expected length of %d, got %d"u8, (nint)(directory64EndLen - 12), size);
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object tooSlowOnWasmˢ = (@string)"too slow on wasm"u8;
internal static readonly @string uint32max1NoZip64ˢ = "uint32max-1_NoZip64"u8;
internal static readonly @string uint32maxHasZip64ˢ = "uint32max_HasZip64"u8;

// Zip64 is required if the total size of the records is uint32max.
public static void TestZip64LargeDirectory(ж<testing.T> Ꮡt) {
    if (runtime.GOARCH == "wasm"u8) {
        Ꮡt.Skip(tooSlowOnWasmˢ);
    }
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    Ꮡt.Parallel();
    // gen returns a func that writes a zip with a wantLen bytes
    // of central directory.
    Action<ж<global::go.archive.zip_package.Writer>> gen(int64 wantLen) => (ж<global::go.archive.zip_package.Writer> w) => {
            w.Value.testHookCloseSizeOffset = (uint64 size, uint64 off) => {
                if (size != (uint64)wantLen) {
                    Ꮡt.Errorf("Close central directory size = %d; want %d"u8, size, wantLen);
                }
            };
            ref var uint16string = ref heap<@string>(out var Ꮡuint16string);
            uint16string = strings.Repeat("."u8, uint16max);
            var remain = wantLen;
            while (remain > 0) {
                nint commentLen = (nint)((nint)uint16max - (nint)directoryHeaderLen) - 1;
                nint thisRecLen = (nint)((nint)directoryHeaderLen + (nint)uint16max) + commentLen;
                if ((int64)thisRecLen > remain) {
                    nint remove = thisRecLen - (nint)remain;
                    commentLen -= remove;
                    thisRecLen -= remove;
                }
                remain -= (int64)thisRecLen;
                var (f, err) = w.CreateHeader(Ꮡ(new FileHeader(
                    Name: uint16string,
                    Comment: uint16string.slice(0, commentLen)
                )));
                if (err != default!) {
                    Ꮡt.Fatalf("CreateHeader: %v"u8, err);
                }
                f._<ж<global::go.archive.zip_package.fileWriter>>().Value.crc32 = new fakeHash32(nil);
            }
            {
                var err = w.Close(); if (err != default!) {
                    Ꮡt.Fatalf("Close: %v"u8, err);
                }
            }
        };
    var genʗ1 = gen;
    Ꮡt.Run(uint32max1NoZip64ˢ, (ж<testing.T> tΔ1) => {
        tΔ1.Parallel();
        if (generatesZip64(tΔ1, genʗ1(4294967294L))) {
            tΔ1.Error(unexpectedZip64ˢ);
        }
    });
    var genʗ2 = gen;
    Ꮡt.Run(uint32maxHasZip64ˢ, (ж<testing.T> tΔ2) => {
        tΔ2.Parallel();
        if (!generatesZip64(tΔ2, genʗ2(uint32max))) {
            tΔ2.Error(expectedZip64ˢ);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object writeChunkˢ = (@string)"write chunk:"u8;
internal static readonly object writeEndˢ = (@string)"write end:"u8;
internal static readonly object readerˢ = (@string)"reader:"u8;
internal static readonly object openingˢ = (@string)"opening:"u8;
internal static readonly object readˢ = (@string)"read:"u8;
internal static readonly object readEndˢ = (@string)"read end:"u8;
internal static readonly object closingˢ = (@string)"closing:"u8;

internal static ж<rleBuffer> testZip64(testing.TB t, int64 size) {
    UntypedInt chunkSize = 1024;
    nint chunks = (nint)(size / (int64)chunkSize);
    // write size bytes plus "END\n" to a zip file
    var buf = @new<rleBuffer>();
    var w = NewWriter(new zip_internal_test_package.rleBufferжWriter(buf));
    var (f, err) = w.CreateHeader(Ꮡ(new FileHeader(
        Name: "huge.txt"u8,
        Method: Store
    )));
    if (err != default!) {
        t.Fatal(err);
    }
    f._<ж<global::go.archive.zip_package.fileWriter>>().Value.crc32 = new fakeHash32(nil);
    var chunk = new slice<byte>(chunkSize);
    foreach (var (i, _) in chunk) {
        chunk[i] = (rune)'.';
    }
    for (nint i = 0; i < chunks; i++) {
        var (_, errΔ1) = f.Write(chunk);
        if (errΔ1 != default!) {
            t.Fatal(writeChunkˢ, errΔ1);
        }
    }
    {
        nint frag = (nint)(size % (int64)chunkSize); if (frag > 0) {
            var (_, errΔ2) = f.Write(chunk.slice(0, frag));
            if (errΔ2 != default!) {
                t.Fatal(writeChunkˢ, errΔ2);
            }
        }
    }
    var end = slice<byte>("END\n"u8);
    (_, err) = f.Write(end);
    if (err != default!) {
        t.Fatal(writeEndˢ, err);
    }
    {
        var errΔ3 = w.Close(); if (errΔ3 != default!) {
            t.Fatal(errΔ3);
        }
    }
    // read back zip file and check that we get to the end of it
    (var r, err) = NewReader(new zip_internal_test_package.rleBufferжReaderAt(buf), buf.Size());
    if (err != default!) {
        t.Fatal(readerˢ, err);
    }
    var f0 = (~r).File[0];
    (var rc, err) = f0.Open();
    if (err != default!) {
        t.Fatal(openingˢ, err);
    }
    rc._<ж<global::go.archive.zip_package.checksumReader>>().Value.hash = new fakeHash32(nil);
    for (nint i = 0; i < chunks; i++) {
        var (_, errΔ4) = io.ReadFull(rc, chunk);
        if (errΔ4 != default!) {
            t.Fatal(readˢ, errΔ4);
        }
    }
    {
        nint frag = (nint)(size % (int64)chunkSize); if (frag > 0) {
            var (_, errΔ5) = io.ReadFull(rc, chunk.slice(0, frag));
            if (errΔ5 != default!) {
                t.Fatal(readˢ, errΔ5);
            }
        }
    }
    (var gotEnd, err) = io.ReadAll(rc);
    if (err != default!) {
        t.Fatal(readEndˢ, err);
    }
    if (!bytes.Equal(gotEnd, end)) {
        t.Errorf("End of zip64 archive %q, want %q"u8, gotEnd, end);
    }
    err = rc.Close();
    if (err != default!) {
        t.Fatal(closingˢ, err);
    }
    if (size + (int64)len("END\n") >= 4294967295L) {
        {
            var (got, want) = (f0.Value.UncompressedSize, (uint32)uint32max); if (got != want) {
                t.Errorf("UncompressedSize %#x, want %#x"u8, got, want);
            }
        }
    }
    {
        var (got, want) = (f0.Value.UncompressedSize64, (uint64)size + (uint64)len(end)); if (got != want) {
            t.Errorf("UncompressedSize64 %#x, want %#x"u8, got, want);
        }
    }
    return buf;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notAZip64ˢ = (@string)"not a zip64"u8;

// Issue 9857
internal static void testZip64DirectoryRecordLength(ж<rleBuffer> Ꮡbuf, ж<testing.T> Ꮡt) {
    if (!suffixIsZip64(Ꮡt, new zip_internal_test_package.rleBufferжsizedReaderAt(Ꮡbuf))) {
        Ꮡt.Fatal(notAZip64ˢ);
    }
}

internal static void testValidHeader(ж<global::go.archive.zip_package.FileHeader> Ꮡh, ж<testing.T> Ꮡt) {
    ref var h = ref Ꮡh.DerefOrNull();

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var z = NewWriter(new zip_test_package.bytes_BufferжWriter(Ꮡbuf));
    var (f, err) = z.CreateHeader(Ꮡh);
    if (err != default!) {
        Ꮡt.Fatalf("error creating header: %v"u8, err);
    }
    {
        var (_, errΔ1) = f.Write(slice<byte>("hi"u8)); if (errΔ1 != default!) {
            Ꮡt.Fatalf("error writing content: %v"u8, errΔ1);
        }
    }
    {
        var errΔ2 = z.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatalf("error closing zip writer: %v"u8, errΔ2);
        }
    }
    var b = buf.Bytes();
    (var zf, err) = NewReader(new zip_test_package.bytes_ReaderжReaderAt(bytes.NewReader(b)), (int64)len(b));
    if (err != default!) {
        Ꮡt.Fatalf("got %v, expected nil"u8, err);
    }
    var zh = (~zf).File[0].Value.FileHeader;
    if (zh.Name != h.Name || zh.Method != h.Method || zh.UncompressedSize64 != (uint64)len("hi")) {
        Ꮡt.Fatalf("got %q/%d/%d expected %q/%d/%d"u8, zh.Name, zh.Method, zh.UncompressedSize64, h.Name, h.Method, len("hi"));
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string timeFormatᶜ = "20060102T150405.000.txt"u8;

// Issue 4302.
public static void TestHeaderInvalidTagAndSize(ж<testing.T> Ꮡt) {
    @string timeFormat = timeFormatᶜ;
    var ts = time.Now();
    @string filename = ts.Format(timeFormat);
    ref var h = ref heap<global::go.archive.zip_package.FileHeader>(out var Ꮡh);
    h = new FileHeader(
        Name: filename,
        Method: Deflate,
        Extra: slice<byte>(ts.Format(time.RFC3339Nano))
    );
    // missing tag and len, but Extra is best-effort parsing
    h.SetModTime(ts);
    testValidHeader(Ꮡh, Ꮡt);
}

public static void TestHeaderTooShort(ж<testing.T> Ꮡt) {
    ref var h = ref heap<global::go.archive.zip_package.FileHeader>(out var Ꮡh);
    h = new FileHeader(
        Name: "foo.txt"u8,
        Method: Deflate,
        Extra: new byte[]{zip64ExtraID}.slice()
    );
    // missing size and second half of tag, but Extra is best-effort parsing
    testValidHeader(Ꮡh, Ꮡt);
}

[GoType("dyn")] internal partial struct TestHeaderTooLongErr_type {
    internal @string name;
    internal slice<byte> extra;
    internal error wanterr;
}

public static void TestHeaderTooLongErr(ж<testing.T> Ꮡt) {
    slice<TestHeaderTooLongErr_type> headerTests = new TestHeaderTooLongErr_type[]{
        new(
            name: strings.Repeat("x"u8, (1 << (int)(16))),
            extra: new byte[]{}.slice(),
            wanterr: errLongName
        ),
        new(
            name: "long_extra"u8,
            extra: bytes.Repeat(new byte[]{0xff}.slice(), (1 << (int)(16))),
            wanterr: errLongExtra
        )
    }.slice();
    // write a zip file
    var buf = @new<bytes.Buffer>();
    var w = NewWriter(new zip_test_package.bytes_BufferжWriter(buf));
    foreach (var (_, test) in headerTests) {
        var h = Ꮡ(new FileHeader(
            Name: test.name,
            Extra: test.extra
        ));
        var (_, err) = w.CreateHeader(h);
        if (!AreEqual(err, test.wanterr)) {
            Ꮡt.Errorf("error=%v, want %v"u8, err, test.wanterr);
        }
    }
    {
        var err = w.Close(); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

public static void TestHeaderIgnoredSize(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var h = ref heap<global::go.archive.zip_package.FileHeader>(out var Ꮡh);
    h = new FileHeader(
        Name: "foo.txt"u8,
        Method: Deflate,
        Extra: new byte[]{(byte)((byte)zip64ExtraID & 0xFF), (byte)(((byte)zip64ExtraID).Rsh((uint64)(8))), 24, 0, 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8, 1, 2, 3, 4, 5, 6, 7, 8}.slice()
    );
    // bad size but shouldn't be consulted
    testValidHeader(Ꮡh, Ꮡt);
}

// Issue 4393. It is valid to have an extra data header
// which contains no body.
public static void TestZeroLengthHeader(ж<testing.T> Ꮡt) {
    ref var h = ref heap<global::go.archive.zip_package.FileHeader>(out var Ꮡh);
    h = new FileHeader(
        Name: "extadata.txt"u8,
        Method: Deflate,
        Extra: new byte[]{
            85, 84, 5, 0, 3, 154, 144, 195, 77, // tag 21589 size 5

            85, 120, 0, 0
        }.slice()
    );
    // tag 30805 size 0
    testValidHeader(Ꮡh, Ꮡt);
}

// Just benchmarking how fast the Zip64 test above is. Not related to
// our zip performance, since the test above disabled CRC32 and flate.
public static void BenchmarkZip64Test(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        testZip64(new zip_test_package.testing_BжTB(Ꮡb), ((int64)1 << (int)(26)));
    }
}

public static void BenchmarkZip64TestSizes(ж<testing.B> Ꮡb) {
    foreach (var (_, size) in new int64[]{((int64)1 << (int)(12)), ((int64)1 << (int)(20)), ((int64)1 << (int)(26))}.slice()) {
        Ꮡb.Run(fmt.Sprint(size), (ж<testing.B> bΔ1) => {
            bΔ1.RunParallel((ж<testing.PB> pb) => {
                while (pb.Next()) {
                    testZip64(new zip_test_package.testing_BжTB(bΔ1), size);
                }
            });
        });
    }
}

public static void TestSuffixSaver(ж<testing.T> Ꮡt) {
    UntypedInt keep = 10;
    var ss = Ꮡ(new suffixSaver(keep: keep));
    ss.Write(slice<byte>("abc"u8));
    {
        @string got = ((@string)ss.Suffix()); if (got != "abc"u8) {
            Ꮡt.Errorf("got = %q; want abc"u8, got);
        }
    }
    ss.Write(slice<byte>("defghijklmno"u8));
    {
        @string got = ((@string)ss.Suffix()); if (got != "fghijklmno"u8) {
            Ꮡt.Errorf("got = %q; want fghijklmno"u8, got);
        }
    }
    {
        var (got, want) = (ss.Size(), (int64)(len("abc") + len("defghijklmno"))); if (got != want) {
            Ꮡt.Errorf("Size = %d; want %d"u8, got, want);
        }
    }
    var buf = new slice<byte>((nint)(ss.Size()));
    for (var off = (int64)0; off < ss.Size(); off++) {
        for (nint size = 1; size <= (nint)(ss.Size() - off); size++) {
            var readBuf = buf.slice(0, size);
            var (n, err) = ss.ReadAt(readBuf, off);
            if (off < ss.Size() - (int64)keep) {
                if (!AreEqual(err, errDiscardedBytes)) {
                    Ꮡt.Errorf("off %d, size %d = %v, %v (%q); want errDiscardedBytes"u8, off, size, n, err, readBuf.slice(0, n));
                }
                continue;
            }
            @string want = "abcdefghijklmno"u8.slice((nint)(off), (nint)(off + (int64)size));
            @string got = ((@string)(readBuf.slice(0, n)));
            if (err != default! || got != want) {
                Ꮡt.Errorf("off %d, size %d = %v, %v (%q); want %q"u8, off, size, n, err, got, want);
            }
        }
    }
}

[GoType] internal partial struct zeros {
}

internal static (nint, error) Read(this zeros _, slice<byte> p) {
    clear(p);
    return (len(p), default!);
}

} // end zip_internal_test_package
