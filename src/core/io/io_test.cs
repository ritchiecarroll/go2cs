// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using static io_package;
using os = os_package;
using strings = strings_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using testing = testing_package;
using go.sync;
using static go.io_internal_test_package;
using Δio = io_package;

partial class io_test_package {

// A version of bytes.Buffer without ReadFrom and WriteTo
[GoType] partial struct Buffer {
    public partial ref bytes_package.Buffer ΔBuffer { get; }
    [GoEmbedded] public io_package.ReaderFrom ReaderFrom; // conflicts with and hides bytes.Buffer's ReaderFrom.
    [GoEmbedded] public io_package.WriterTo WriterTo;   // conflicts with and hides bytes.Buffer's WriterTo.
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloWorldˢ = "hello, world."u8;

// Simple tests, primarily to verify the ReadFrom and WriteTo callouts inside Copy, CopyBuffer and CopyN.
public static void TestCopy(ж<testing.T> Ꮡt) {
    var rb = @new<Buffer>();
    var wb = @new<Buffer>();
    rb.of(Buffer.ᏑΔBuffer).WriteString(helloWorldˢ);
    Copy(new io_test_package.BufferжWriter(wb), new io_test_package.BufferжReader(rb));
    if (wb.of(Buffer.ᏑΔBuffer).String() != "hello, world."u8) {
        Ꮡt.Errorf("Copy did not work properly"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloˢ = "hello"u8;

public static void TestCopyNegative(ж<testing.T> Ꮡt) {
    var rb = @new<Buffer>();
    var wb = @new<Buffer>();
    rb.of(Buffer.ᏑΔBuffer).WriteString(helloˢ);
    Copy(new io_test_package.BufferжWriter(wb), new Δio.LimitedReaderжReader(Ꮡ(new LimitedReader(R: new io_test_package.BufferжReader(rb), N: -1))));
    if (wb.of(Buffer.ᏑΔBuffer).String() != ""u8) {
        Ꮡt.Errorf("Copy on LimitedReader with N<0 copied data"u8);
    }
    CopyN(new io_test_package.BufferжWriter(wb), new io_test_package.BufferжReader(rb), -1);
    if (wb.of(Buffer.ᏑΔBuffer).String() != ""u8) {
        Ꮡt.Errorf("CopyN with N<0 copied data"u8);
    }
}

public static void TestCopyBuffer(ж<testing.T> Ꮡt) {
    var rb = @new<Buffer>();
    var wb = @new<Buffer>();
    rb.of(Buffer.ᏑΔBuffer).WriteString(helloWorldˢ);
    CopyBuffer(new io_test_package.BufferжWriter(wb), new io_test_package.BufferжReader(rb), new slice<byte>(1)); // Tiny buffer to keep it honest.
    if (wb.of(Buffer.ᏑΔBuffer).String() != "hello, world."u8) {
        Ꮡt.Errorf("CopyBuffer did not work properly"u8);
    }
}

public static void TestCopyBufferNil(ж<testing.T> Ꮡt) {
    var rb = @new<Buffer>();
    var wb = @new<Buffer>();
    rb.of(Buffer.ᏑΔBuffer).WriteString(helloWorldˢ);
    CopyBuffer(new io_test_package.BufferжWriter(wb), new io_test_package.BufferжReader(rb), default!); // Should allocate a buffer.
    if (wb.of(Buffer.ᏑΔBuffer).String() != "hello, world."u8) {
        Ꮡt.Errorf("CopyBuffer did not work properly"u8);
    }
}

public static void TestCopyReadFrom(ж<testing.T> Ꮡt) {
    var rb = @new<Buffer>();
    var wb = @new<bytes.Buffer>(); // implements ReadFrom.
    rb.of(Buffer.ᏑΔBuffer).WriteString(helloWorldˢ);
    Copy(new io_test_package.bytes_BufferжWriter(wb), new io_test_package.BufferжReader(rb));
    if (wb.String() != "hello, world."u8) {
        Ꮡt.Errorf("Copy did not work properly"u8);
    }
}

public static void TestCopyWriteTo(ж<testing.T> Ꮡt) {
    var rb = @new<bytes.Buffer>(); // implements WriteTo.
    var wb = @new<Buffer>();
    rb.WriteString(helloWorldˢ);
    Copy(new io_test_package.BufferжWriter(wb), new io_test_package.bytes_BufferжReader(rb));
    if (wb.of(Buffer.ᏑΔBuffer).String() != "hello, world."u8) {
        Ꮡt.Errorf("Copy did not work properly"u8);
    }
}

// Version of bytes.Buffer that checks whether WriteTo was called or not
[GoType] partial struct writeToChecker {
    public partial ref bytes_package.Buffer Buffer { get; }
    internal bool writeToCalled;
}

[GoRecv] internal static (int64, error) WriteTo(this ref writeToChecker wt, Δio.Writer w) {
    wt.writeToCalled = true;
    return wt.Buffer.WriteTo(w);
}

// It's preferable to choose WriterTo over ReaderFrom, since a WriterTo can issue one large write,
// while the ReaderFrom must read until EOF, potentially allocating when running out of buffer.
// Make sure that we choose WriterTo when both are implemented.
public static void TestCopyPriority(ж<testing.T> Ꮡt) {
    var rb = @new<writeToChecker>();
    var wb = @new<bytes.Buffer>();
    rb.of(writeToChecker.ᏑBuffer).WriteString(helloWorldˢ);
    Copy(new io_test_package.bytes_BufferжWriter(wb), new io_test_package.writeToCheckerжReader(rb));
    if (wb.String() != "hello, world."u8){
        Ꮡt.Errorf("Copy did not work properly"u8);
    } else 
    if (!(~rb).writeToCalled) {
        Ꮡt.Errorf("WriteTo was not prioritized over ReadFrom"u8);
    }
}

[GoType] partial struct zeroErrReader {
    internal error err;
}

internal static (nint, error) Read(this zeroErrReader r, slice<byte> p) {
    return (copy(p, new byte[]{0}.slice()), r.err);
}

[GoType] partial struct errWriter {
    internal error err;
}

internal static (nint, error) Write(this errWriter w, slice<byte> _) {
    return (0, w.err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string readErrorˢ = "readError"u8;
internal static readonly @string writeErrorˢ = "writeError"u8;

// In case a Read results in an error with non-zero bytes read, and
// the subsequent Write also results in an error, the error from Write
// is returned, as it is the one that prevented progressing further.
public static void TestCopyReadErrWriteErr(ж<testing.T> Ꮡt) {
    var (er, ew) = (errors.New(readErrorˢ), errors.New(writeErrorˢ));
    var (r, w) = (new zeroErrReader(err: er), new errWriter(err: ew));
    var (n, err) = Copy(w, r);
    if (n != 0 || !AreEqual(err, ew)) {
        Ꮡt.Errorf("Copy(zeroErrReader, errWriter) = %d, %v; want 0, writeError"u8, n, err);
    }
}

public static void TestCopyN(ж<testing.T> Ꮡt) {
    var rb = @new<Buffer>();
    var wb = @new<Buffer>();
    rb.of(Buffer.ᏑΔBuffer).WriteString(helloWorldˢ);
    CopyN(new io_test_package.BufferжWriter(wb), new io_test_package.BufferжReader(rb), 5);
    if (wb.of(Buffer.ᏑΔBuffer).String() != "hello"u8) {
        Ꮡt.Errorf("CopyN did not work properly"u8);
    }
}

public static void TestCopyNReadFrom(ж<testing.T> Ꮡt) {
    var rb = @new<Buffer>();
    var wb = @new<bytes.Buffer>(); // implements ReadFrom.
    rb.of(Buffer.ᏑΔBuffer).WriteString(helloˢ);
    CopyN(new io_test_package.bytes_BufferжWriter(wb), new io_test_package.BufferжReader(rb), 5);
    if (wb.String() != "hello"u8) {
        Ꮡt.Errorf("CopyN did not work properly"u8);
    }
}

public static void TestCopyNWriteTo(ж<testing.T> Ꮡt) {
    var rb = @new<bytes.Buffer>(); // implements WriteTo.
    var wb = @new<Buffer>();
    rb.WriteString(helloWorldˢ);
    CopyN(new io_test_package.BufferжWriter(wb), new io_test_package.bytes_BufferжReader(rb), 5);
    if (wb.of(Buffer.ᏑΔBuffer).String() != "hello"u8) {
        Ꮡt.Errorf("CopyN did not work properly"u8);
    }
}

public static void BenchmarkCopyNSmall(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var bs = bytes.Repeat(new byte[]{0}.slice(), 512 + 1);
    var rd = bytes.NewReader(bs);
    var buf = @new<Buffer>();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        CopyN(new io_test_package.BufferжWriter(buf), new io_test_package.bytes_ReaderжReader(rd), 512);
        rd.Reset(bs);
    }
}

public static void BenchmarkCopyNLarge(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var bs = bytes.Repeat(new byte[]{0}.slice(), (32 * 1024) + 1);
    var rd = bytes.NewReader(bs);
    var buf = @new<Buffer>();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        CopyN(new io_test_package.BufferжWriter(buf), new io_test_package.bytes_ReaderжReader(rd), 32 * 1024);
        rd.Reset(bs);
    }
}

[GoType] partial struct noReadFrom {
    internal Δio.Writer w;
}

[GoRecv] internal static (nint n, error err) Write(this ref noReadFrom w, slice<byte> p) {
    return w.w.Write(p);
}

[GoType] partial struct wantedAndErrReader {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string wantedAndErrReaderErrorˢ = "wantedAndErrReader error"u8;

internal static (nint, error) Read(this wantedAndErrReader _, slice<byte> p) {
    return (len(p), errors.New(wantedAndErrReaderErrorˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "foo"u8;

public static void TestCopyNEOF(ж<testing.T> Ꮡt) {
    // Test that EOF behavior is the same regardless of whether
    // argument to CopyN has ReadFrom.
    var b = @new<bytes.Buffer>();
    var (n, err) = CopyN(new io_test_package.noReadFromжWriter(Ꮡ(new noReadFrom(new io_test_package.bytes_BufferжWriter(b)))), new io_test_package.strings_ReaderжReader(strings.NewReader(fooˢ)), 3);
    if (n != 3 || err != default!) {
        Ꮡt.Errorf("CopyN(noReadFrom, foo, 3) = %d, %v; want 3, nil"u8, n, err);
    }
    (n, err) = CopyN(new io_test_package.noReadFromжWriter(Ꮡ(new noReadFrom(new io_test_package.bytes_BufferжWriter(b)))), new io_test_package.strings_ReaderжReader(strings.NewReader(fooˢ)), 4);
    if (n != 3 || !AreEqual(err, EOF)) {
        Ꮡt.Errorf("CopyN(noReadFrom, foo, 4) = %d, %v; want 3, EOF"u8, n, err);
    }
    (n, err) = CopyN(new io_test_package.bytes_BufferжWriter(b), new io_test_package.strings_ReaderжReader(strings.NewReader(fooˢ)), 3); // b has read from
    if (n != 3 || err != default!) {
        Ꮡt.Errorf("CopyN(bytes.Buffer, foo, 3) = %d, %v; want 3, nil"u8, n, err);
    }
    (n, err) = CopyN(new io_test_package.bytes_BufferжWriter(b), new io_test_package.strings_ReaderжReader(strings.NewReader(fooˢ)), 4); // b has read from
    if (n != 3 || !AreEqual(err, EOF)) {
        Ꮡt.Errorf("CopyN(bytes.Buffer, foo, 4) = %d, %v; want 3, EOF"u8, n, err);
    }
    (n, err) = CopyN(new io_test_package.bytes_BufferжWriter(b), new wantedAndErrReader(nil), 5);
    if (n != 5 || err != default!) {
        Ꮡt.Errorf("CopyN(bytes.Buffer, wantedAndErrReader, 5) = %d, %v; want 5, nil"u8, n, err);
    }
    (n, err) = CopyN(new io_test_package.noReadFromжWriter(Ꮡ(new noReadFrom(new io_test_package.bytes_BufferжWriter(b)))), new wantedAndErrReader(nil), 5);
    if (n != 5 || err != default!) {
        Ꮡt.Errorf("CopyN(noReadFrom, wantedAndErrReader, 5) = %d, %v; want 5, nil"u8, n, err);
    }
}

public static void TestReadAtLeast(ж<testing.T> Ꮡt) {
    ref var rb = ref heap(new bytes.Buffer(), out var Ꮡrb);
    testReadAtLeast(Ꮡt, new io_test_package.bytes_BufferжReadWriter(Ꮡrb));
}

// A version of bytes.Buffer that returns n > 0, err on Read
// when the input is exhausted.
[GoType] partial struct dataAndErrorBuffer {
    internal error err;
    public partial ref bytes_package.Buffer Buffer { get; }
}

[GoRecv] internal static (nint n, error err) Read(this ref dataAndErrorBuffer r, slice<byte> p) {
    nint n = default!;
    error err = default!;

    (n, err) = r.Buffer.Read(p);
    if (n > 0 && r.Buffer.Len() == 0 && err == default!) {
        err = r.err;
    }
    return (n, err);
}

public static void TestReadAtLeastWithDataAndEOF(ж<testing.T> Ꮡt) {
    ref var rb = ref heap(new dataAndErrorBuffer(), out var Ꮡrb);
    rb.err = EOF;
    testReadAtLeast(Ꮡt, new io_test_package.dataAndErrorBufferжReadWriter(Ꮡrb));
}

public static void TestReadAtLeastWithDataAndError(ж<testing.T> Ꮡt) {
    ref var rb = ref heap(new dataAndErrorBuffer(), out var Ꮡrb);
    rb.err = fmt.Errorf("fake error"u8);
    testReadAtLeast(Ꮡt, new io_test_package.dataAndErrorBufferжReadWriter(Ꮡrb));
}

internal static void testReadAtLeast(ж<testing.T> Ꮡt, Δio.ReadWriter rb) {
    rb.Write(slice<byte>("0123"u8));
    var buf = new slice<byte>(2);
    var (n, err) = ReadAtLeast(rb, buf, 2);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    if (n != 2) {
        Ꮡt.Errorf("expected to have read 2 bytes, got %v"u8, n);
    }
    (n, err) = ReadAtLeast(rb, buf, 4);
    if (!AreEqual(err, ErrShortBuffer)) {
        Ꮡt.Errorf("expected ErrShortBuffer got %v"u8, err);
    }
    if (n != 0) {
        Ꮡt.Errorf("expected to have read 0 bytes, got %v"u8, n);
    }
    (n, err) = ReadAtLeast(rb, buf, 1);
    if (err != default!) {
        Ꮡt.Error(err);
    }
    if (n != 2) {
        Ꮡt.Errorf("expected to have read 2 bytes, got %v"u8, n);
    }
    (n, err) = ReadAtLeast(rb, buf, 2);
    if (!AreEqual(err, EOF)) {
        Ꮡt.Errorf("expected EOF, got %v"u8, err);
    }
    if (n != 0) {
        Ꮡt.Errorf("expected to have read 0 bytes, got %v"u8, n);
    }
    rb.Write(slice<byte>("4"u8));
    (n, err) = ReadAtLeast(rb, buf, 2);
    var want = ErrUnexpectedEOF;
    {
        var (rbΔ1, ok) = rb._<ж<dataAndErrorBuffer>>(ᐧ); if (ok && !AreEqual((~rbΔ1).err, EOF)) {
            want = rbΔ1.Value.err;
        }
    }
    if (!AreEqual(err, want)) {
        Ꮡt.Errorf("expected %v, got %v"u8, want, err);
    }
    if (n != 1) {
        Ꮡt.Errorf("expected to have read 1 bytes, got %v"u8, n);
    }
}

public static void TestTeeReader(ж<testing.T> Ꮡt) {
    var src = slice<byte>("hello, world"u8);
    var dst = new slice<byte>(len(src));
    var rb = bytes.NewBuffer(src);
    var wb = @new<bytes.Buffer>();
    var r = TeeReader(new io_test_package.bytes_BufferжReader(rb), new io_test_package.bytes_BufferжWriter(wb));
    {
        var (n, err) = ReadFull(r, dst); if (err != default! || n != len(src)) {
            Ꮡt.Fatalf("ReadFull(r, dst) = %d, %v; want %d, nil"u8, n, err, len(src));
        }
    }
    if (!bytes.Equal(dst, src)) {
        Ꮡt.Errorf("bytes read = %q want %q"u8, dst, src);
    }
    if (!bytes.Equal(wb.Bytes(), src)) {
        Ꮡt.Errorf("bytes written = %q want %q"u8, wb.Bytes(), src);
    }
    {
        var (n, err) = r.Read(dst); if (n != 0 || !AreEqual(err, EOF)) {
            Ꮡt.Errorf("r.Read at EOF = %d, %v want 0, EOF"u8, n, err);
        }
    }
    rb = bytes.NewBuffer(src);
    var (pr, pw) = Pipe();
    pr.Close();
    r = TeeReader(new io_test_package.bytes_BufferжReader(rb), new Δio.PipeWriterжWriter(pw));
    {
        var (n, err) = ReadFull(r, dst); if (n != 0 || !AreEqual(err, ErrClosedPipe)) {
            Ꮡt.Errorf("closed tee: ReadFull(r, dst) = %d, %v; want 0, EPIPE"u8, n, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aLongSampleDataˢ = "a long sample data, 1234567890"u8;

[GoType("dyn")] internal partial struct TestSectionReader_ReadAt_tests {
    internal @string data;
    internal nint off;
    internal nint n;
    internal nint bufLen;
    internal nint at;
    internal @string exp;
    internal error err;
}

public static void TestSectionReader_ReadAt(ж<testing.T> Ꮡt) {
    @string dat = aLongSampleDataˢ;
    var tests = new TestSectionReader_ReadAt_tests[]{
        new(data: ""u8, off: 0, n: 10, bufLen: 2, at: 0, exp: ""u8, err: EOF),
        new(data: dat, off: 0, n: len(dat), bufLen: 0, at: 0, exp: ""u8, err: default!),
        new(data: dat, off: len(dat), n: 1, bufLen: 1, at: 0, exp: ""u8, err: EOF),
        new(data: dat, off: 0, n: len(dat) + 2, bufLen: len(dat), at: 0, exp: dat, err: default!),
        new(data: dat, off: 0, n: len(dat), bufLen: len(dat) / 2, at: 0, exp: dat[..(int)(len(dat) / 2)], err: default!),
        new(data: dat, off: 0, n: len(dat), bufLen: len(dat), at: 0, exp: dat, err: default!),
        new(data: dat, off: 0, n: len(dat), bufLen: len(dat) / 2, at: 2, exp: dat[2..(int)(2 + len(dat) / 2)], err: default!),
        new(data: dat, off: 3, n: len(dat), bufLen: len(dat) / 2, at: 2, exp: dat[5..(int)(5 + len(dat) / 2)], err: default!),
        new(data: dat, off: 3, n: len(dat) / 2, bufLen: len(dat) / 2 - 2, at: 2, exp: dat[5..(int)(5 + len(dat) / 2 - 2)], err: default!),
        new(data: dat, off: 3, n: len(dat) / 2, bufLen: len(dat) / 2 + 2, at: 2, exp: dat[5..(int)(5 + len(dat) / 2 - 2)], err: EOF),
        new(data: dat, off: 0, n: 0, bufLen: 0, at: -1, exp: ""u8, err: EOF),
        new(data: dat, off: 0, n: 0, bufLen: 0, at: 1, exp: ""u8, err: EOF)
    }.slice();
    foreach (var (i, tt) in tests) {
        var r = strings.NewReader(tt.data);
        var s = NewSectionReader(new io_test_package.strings_ReaderжReaderAt(r), (int64)tt.off, (int64)tt.n);
        var buf = new slice<byte>(tt.bufLen);
        {
            var (n, err) = s.ReadAt(buf, (int64)tt.at); if (n != len(tt.exp) || ((sstring)(buf[..(int)(n)])) != tt.exp || !AreEqual(err, tt.err)) {
                Ꮡt.Fatalf("%d: ReadAt(%d) = %q, %v; expected %q, %v"u8, i, tt.at, buf[..(int)(n)], err, tt.exp, tt.err);
            }
        }
        {
            var (_r, off, n) = s.Outer(); if (!AreEqual(_r, r) || off != (int64)tt.off || n != (int64)tt.n) {
                Ꮡt.Fatalf("%d: Outer() = %v, %d, %d; expected %v, %d, %d"u8, i, _r, off, n, r.OrTypedNil(), tt.off, tt.n);
            }
        }
    }
}

public static void TestSectionReader_Seek(ж<testing.T> Ꮡt) {
    // Verifies that NewSectionReader's Seeker behaves like bytes.NewReader (which is like strings.NewReader)
    var br = bytes.NewReader(slice<byte>("foo"u8));
    var sr = NewSectionReader(new io_test_package.bytes_ReaderжReaderAt(br), 0, (int64)len("foo"));
    foreach (var (_, whence) in new nint[]{SeekStart, SeekCurrent, SeekEnd}.slice()) {
        for (var offset = (int64)(-3); offset <= 4; offset++) {
            var (brOff, brErr) = br.Seek(offset, whence);
            var (srOff, srErr) = sr.Seek(offset, whence);
            if ((brErr != default!) != (srErr != default!) || brOff != srOff) {
                Ꮡt.Errorf("For whence %d, offset %d: bytes.Reader.Seek = (%v, %v) != SectionReader.Seek = (%v, %v)"u8,
                    whence, offset, brOff, brErr, srErr, srOff);
            }
        }
    }
    // And verify we can just seek past the end and get an EOF
    var (got, err) = sr.Seek(100, SeekStart);
    if (err != default! || got != 100) {
        Ꮡt.Errorf("Seek = %v, %v; want 100, nil"u8, got, err);
    }
    (var n, err) = sr.Read(new slice<byte>(10));
    if (n != 0 || !AreEqual(err, EOF)) {
        Ꮡt.Errorf("Read = %v, %v; want 0, EOF"u8, n, err);
    }
}

[GoType("dyn")] internal partial struct TestSectionReader_Size_tests {
    internal @string data;
    internal int64 want;
}

public static void TestSectionReader_Size(ж<testing.T> Ꮡt) {
    var tests = new TestSectionReader_Size_tests[]{
        new("a long sample data, 1234567890"u8, 30),
        new(""u8, 0)
    }.slice();
    foreach (var (_, tt) in tests) {
        var r = strings.NewReader(tt.data);
        var sr = NewSectionReader(new io_test_package.strings_ReaderжReaderAt(r), 0, (int64)len(tt.data));
        {
            var got = sr.Size(); if (got != tt.want) {
                Ꮡt.Errorf("Size = %v; want %v"u8, got, tt.want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcdefˢ = "abcdef"u8;

public static void TestSectionReader_Max(ж<testing.T> Ꮡt) {
    var r = strings.NewReader(abcdefˢ);
    UntypedInt maxint64 = /* 1<<63 - 1 */ 9223372036854775807;
    var sr = NewSectionReader(new io_test_package.strings_ReaderжReaderAt(r), 3, maxint64);
    var (n, err) = sr.Read(new slice<byte>(3));
    if (n != 3 || err != default!) {
        Ꮡt.Errorf("Read = %v %v, want 3, nil"u8, n, err);
    }
    (n, err) = sr.Read(new slice<byte>(3));
    if (n != 0 || !AreEqual(err, EOF)) {
        Ꮡt.Errorf("Read = %v, %v, want 0, EOF"u8, n, err);
    }
    {
        var (_r, off, nΔ1) = sr.Outer(); if (!AreEqual(_r, r) || off != 3 || nΔ1 != maxint64) {
            Ꮡt.Fatalf("Outer = %v, %d, %d; expected %v, %d, %d"u8, _r, off, nΔ1, r.OrTypedNil(), (nint)(3), (int64)maxint64);
        }
    }
}

// largeWriter returns an invalid count that is larger than the number
// of bytes provided (issue 39978).
[GoType] partial struct largeWriter {
    internal error err;
}

internal static (nint, error) Write(this largeWriter w, slice<byte> p) {
    return (len(p) + 1, w.err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string largeWriterErrorˢ = "largeWriterError"u8;

public static void TestCopyLargeWriter(ж<testing.T> Ꮡt) {
    var want = io_internal_test_package.ErrInvalidWrite;
    var rb = @new<Buffer>();
    var wb = new largeWriter(nil);
    rb.of(Buffer.ᏑΔBuffer).WriteString(helloWorldˢ);
    {
        var (_, err) = Copy(wb, new io_test_package.BufferжReader(rb)); if (!AreEqual(err, want)) {
            Ꮡt.Errorf("Copy error: got %v, want %v"u8, err, want);
        }
    }
    want = errors.New(largeWriterErrorˢ);
    rb = @new<Buffer>();
    wb = new largeWriter(err: want);
    rb.of(Buffer.ᏑΔBuffer).WriteString(helloWorldˢ);
    {
        var (_, err) = Copy(wb, new io_test_package.BufferжReader(rb)); if (!AreEqual(err, want)) {
            Ꮡt.Errorf("Copy error: got %v, want %v"u8, err, want);
        }
    }
}

[GoType("dyn")] internal partial struct TestNopCloserWriterToForwarding_type {
    public @string Name;
    internal Δio.Reader r;
}

[GoType("dyn")] internal partial struct TestNopCloserWriterToForwarding_typeᴛ1 {
    [GoEmbedded] public io_package.Reader Reader;
    [GoEmbedded] public io_package.WriterTo WriterTo;
}

public static void TestNopCloserWriterToForwarding(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, tc) in new TestNopCloserWriterToForwarding_type[]{
        new("not a WriterTo"u8, ((Δio.Reader)default!)),
        new("a WriterTo"u8, new TestNopCloserWriterToForwarding_typeᴛ1())
    }.array()) {
        var nc = NopCloser(tc.r);
        var (_, expected) = tc.r._<WriterTo>(ᐧ);
        var (_, got) = nc._<WriterTo>(ᐧ);
        if (expected != got) {
            Ꮡt.Errorf("NopCloser incorrectly forwards WriterTo for %s, got %t want %t"u8, tc.Name, got, expected);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testOffsetWriterSeekˢ = "TestOffsetWriter_Seek"u8;
internal static readonly @string errWhenceˢ = "errWhence"u8;
internal static readonly @string errOffsetˢ = "errOffset"u8;
internal static readonly @string normalˢ = "normal"u8;

[GoType("dyn")] internal partial struct TestOffsetWriter_Seek_tests {
    internal int64 offset;
    internal nint whence;
    internal int64 returnOff;
}

public static void TestOffsetWriter_Seek(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string tmpfilename = testOffsetWriterSeekˢ;
        var (tmpfile, err) = os.CreateTemp(Ꮡt.TempDir(), tmpfilename);
        if (err != default!) {
            Ꮡt.Fatalf("CreateTemp(%s) failed: %v"u8, tmpfilename, err);
        }
        var tmpfileʗ1 = tmpfile;
        defer(() => tmpfileʗ1.Close(), ref ᒐ);
        var w = NewOffsetWriter(new io_test_package.os_FileжWriterAt(tmpfile), 0);
        // Should throw error errWhence if whence is not valid
        var wʗ1 = w;
        Ꮡt.Run(errWhenceˢ, (ж<testing.T> tΔ1) => {
            foreach (var (_, whence) in new nint[]{-3, -2, -1, 3, 4, 5}.slice()) {
                int64 offset = 0;
                var (gotOff, gotErr) = wʗ1.Seek(offset, whence);
                if (gotOff != 0 || !AreEqual(gotErr, io_internal_test_package.ErrWhence)) {
                    tΔ1.Errorf("For whence %d, offset %d, OffsetWriter.Seek got: (%d, %v), want: (%d, %v)"u8,
                        whence, offset, gotOff, gotErr, (nint)(0), io_internal_test_package.ErrWhence);
                }
            }
        });
        // Should throw error errOffset if offset is negative
        var wʗ2 = w;
        Ꮡt.Run(errOffsetˢ, (ж<testing.T> tΔ2) => {
            foreach (var (_, whence) in new nint[]{SeekStart, SeekCurrent}.slice()) {
                for (var offset = (int64)(-3); offset < 0; offset++) {
                    var (gotOff, gotErr) = wʗ2.Seek(offset, whence);
                    if (gotOff != 0 || !AreEqual(gotErr, io_internal_test_package.ErrOffset)) {
                        tΔ2.Errorf("For whence %d, offset %d, OffsetWriter.Seek got: (%d, %v), want: (%d, %v)"u8,
                            whence, offset, gotOff, gotErr, (nint)(0), io_internal_test_package.ErrOffset);
                    }
                }
            }
        });
        // Normal tests
        var wʗ3 = w;
        Ꮡt.Run(normalˢ, (ж<testing.T> tΔ3) => {
            var tests = new TestOffsetWriter_Seek_tests[]{ // keep in order

                new(whence: SeekStart, offset: 1, returnOff: 1),
                new(whence: SeekStart, offset: 2, returnOff: 2),
                new(whence: SeekStart, offset: 3, returnOff: 3),
                new(whence: SeekCurrent, offset: 1, returnOff: 4),
                new(whence: SeekCurrent, offset: 2, returnOff: 6),
                new(whence: SeekCurrent, offset: 3, returnOff: 9)
            }.slice();
            foreach (var (idx, tt) in tests) {
                var (gotOff, gotErr) = wʗ3.Seek(tt.offset, tt.whence);
                if (gotOff != tt.returnOff || gotErr != default!) {
                    tΔ3.Errorf("%d:: For whence %d, offset %d, OffsetWriter.Seek got: (%d, %v), want: (%d, <nil>)"u8,
                        idx + 1, tt.whence, tt.offset, gotOff, gotErr, tt.returnOff);
                }
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string contentᶜ = "0123456789ABCDEF"u8;

public static void TestOffsetWriter_WriteAt(ж<testing.T> Ꮡt) {
    @string content = contentᶜ;
    var contentSize = (int64)len(content);
    @string tmpdir = Ꮡt.TempDir();
    void work(int64 off, int64 at) {
        GoFrame ᒐ = default;
        try {
            @string position = fmt.Sprintf("off_%d_at_%d"u8, off, at);
            var (tmpfile, err) = os.CreateTemp(tmpdir, position);
            if (err != default!) {
                Ꮡt.Fatalf("CreateTemp(%s) failed: %v"u8, position, err);
            }
            var tmpfileʗ1 = tmpfile;
            defer(() => tmpfileʗ1.Close(), ref ᒐ);
            ref var writeN = ref heap(new int64(), out var ᏑwriteN);
            ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
            // Concurrent writes, one byte at a time
            foreach (var (step, value) in slice<byte>(content)) {
                Ꮡwg.Add(1);
                goǃ((ж<Δsync.WaitGroup> wgΔ1, ж<os.File> tmpfileΔ1, byte valueΔ1, int64 offΔ1, int64 atΔ1, nint stepΔ1) => {
                    GoFrame ᒐ = default;
                    try {
                        defer(wgΔ1.Done, ref ᒐ);
                        var w = NewOffsetWriter(new io_test_package.os_FileжWriterAt(tmpfileΔ1), offΔ1);
                        var (n, e) = w.WriteAt(new byte[]{valueΔ1}.slice(), atΔ1 + (int64)stepΔ1);
                        if (e != default!) {
                            Ꮡt.Errorf("WriteAt failed. off: %d, at: %d, step: %d\n error: %v"u8, offΔ1, atΔ1, stepΔ1, e);
                        }
                        atomic.AddInt64(ᏑwriteN, (int64)n);
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, Ꮡwg, tmpfile, value, off, at, step);
            }
            Ꮡwg.Wait();
            // Read one more byte to reach EOF
            var buf = new slice<byte>((nint)(contentSize + 1));
            (var readN, err) = tmpfile.ReadAt(buf, off + at);
            if (!AreEqual(err, EOF)) {
                Ꮡt.Fatalf("ReadAt failed: %v"u8, err);
            }
            @string readContent = ((@string)(buf[..(int)(contentSize)]));
            if (ᏑwriteN.Value != (int64)readN || ᏑwriteN.Value != contentSize || readContent != content) {
                Ꮡt.Fatalf("%s:: WriteAt(%s, %d) error. \ngot n: %v, content: %s \nexpected n: %v, content: %v"u8,
                    position, content, at, readN, readContent, contentSize, content);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    for (var off = (int64)0; off < 2; off++) {
        for (var at = (int64)0; at < 2; at++) {
            work(off, at);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testOffsetWriterWriteAtˢ = "TestOffsetWriter_WriteAt"u8;

public static void TestWriteAt_PositionPriorToBase(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string tmpdir = Ꮡt.TempDir();
        @string tmpfilename = testOffsetWriterWriteAtˢ;
        var (tmpfile, err) = os.CreateTemp(tmpdir, tmpfilename);
        if (err != default!) {
            Ꮡt.Fatalf("CreateTemp(%s) failed: %v"u8, tmpfilename, err);
        }
        var tmpfileʗ1 = tmpfile;
        defer(() => tmpfileʗ1.Close(), ref ᒐ);
        // start writing position in OffsetWriter
        var offset = (int64)10;
        // position we want to write to the tmpfile
        var at = (int64)(-1);
        var w = NewOffsetWriter(new io_test_package.os_FileжWriterAt(tmpfile), offset);
        var (_, e) = w.WriteAt(slice<byte>("hello"u8), at);
        if (e == default!) {
            Ꮡt.Errorf("error expected to be not nil"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string writeˢ = "Write"u8;
internal static readonly @string copyˢ = "Copy"u8;
internal static readonly @string writeOfCopyWriteToˢ = "Write_Of_Copy_WriteTo"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string contentᶜ1 = "0123456789ABCDEF"u8;

public static void TestOffsetWriter_Write(ж<testing.T> Ꮡt) {
    @string content = contentᶜ1;
    nint contentSize = len(content);
    @string tmpdir = Ꮡt.TempDir();
    (ж<Δio.OffsetWriter>, ж<os.File>) makeOffsetWriter(@string nameΔ1) {
        @string tmpfilename = "TestOffsetWriter_Write_"u8 + nameΔ1;
        var (tmpfile, err) = os.CreateTemp(tmpdir, tmpfilename);
        if (err != default!) {
            Ꮡt.Fatalf("CreateTemp(%s) failed: %v"u8, tmpfilename, err);
        }
        return (NewOffsetWriter(new io_test_package.os_FileжWriterAt(tmpfile), 0), tmpfile);
    }
    void checkContent(@string nameΔ2, ж<os.File> f) {
        // Read one more byte to reach EOF
        var buf = new slice<byte>(contentSize + 1);
        var (readN, err) = f.ReadAt(buf, 0);
        if (!AreEqual(err, EOF)) {
            Ꮡt.Fatalf("ReadAt failed, err: %v"u8, err);
        }
        @string readContent = ((@string)(buf[..(int)(contentSize)]));
        if (readN != contentSize || readContent != content) {
            Ꮡt.Fatalf("%s error. \ngot n: %v, content: %s \nexpected n: %v, content: %v"u8,
                nameΔ2, readN, readContent, contentSize, content);
        }
    }
    @string name = default!;
    name = writeˢ;
    var checkContentʗ1 = checkContent;
    var makeOffsetWriterʗ1 = makeOffsetWriter;
    Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            // Write directly (off: 0, at: 0)
            // Write content to file
            var (w, f) = makeOffsetWriterʗ1(name);
            var fʗ1 = f;
            defer(() => fʗ1.Close(), ref ᒐ);
            foreach (var (_, value) in slice<byte>(content)) {
                var (n, err) = w.Write(new byte[]{value}.slice());
                if (err != default!) {
                    tΔ1.Fatalf("Write failed, n: %d, err: %v"u8, n, err);
                }
            }
            checkContentʗ1(name, f);
            // Copy -> Write
            // Copy file f to file f2
            name = copyˢ;
            var (w2, f2) = makeOffsetWriterʗ1(name);
            var f2ʗ1 = f2;
            defer(() => f2ʗ1.Close(), ref ᒐ);
            Copy(new Δio.OffsetWriterжWriter(w2), new io_test_package.os_FileжReader(f));
            checkContentʗ1(name, f2);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    // Copy -> WriteTo -> Write
    // Note: strings.Reader implements the io.WriterTo interface.
    name = writeOfCopyWriteToˢ;
    var checkContentʗ2 = checkContent;
    var makeOffsetWriterʗ2 = makeOffsetWriter;
    Ꮡt.Run(name, (ж<testing.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            var (w, f) = makeOffsetWriterʗ2(name);
            var fʗ2 = f;
            defer(() => fʗ2.Close(), ref ᒐ);
            Copy(new Δio.OffsetWriterжWriter(w), new io_test_package.strings_ReaderжReader(strings.NewReader(content)));
            checkContentʗ2(name, f);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

} // end io_test_package
