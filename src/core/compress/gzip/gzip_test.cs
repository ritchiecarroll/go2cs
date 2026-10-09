// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using bufio = bufio_package;
using bytes = bytes_package;
using io = io_package;
using reflect = reflect_package;
using testing = testing_package;
using time = time_package;
using static go.compress.gzip_package;

partial class gzip_internal_test_package {

// TestEmpty tests that an empty payload still forms a valid GZIP stream.
public static void TestEmpty(ж<testing.T> Ꮡt) {
    var buf = @new<bytes.Buffer>();
    {
        var errΔ1 = NewWriter(new gzip_test_package.bytes_BufferжWriter(buf)).Close(); if (errΔ1 != default!) {
            Ꮡt.Fatalf("Writer.Close: %v"u8, errΔ1);
        }
    }
    var (r, err) = NewReader(new gzip_test_package.bytes_BufferжReader(buf));
    if (err != default!) {
        Ꮡt.Fatalf("NewReader: %v"u8, err);
    }
    {
        var want = (new Header(OS: 255)); if (!reflect.DeepEqual((~r).Header, want)) {
            Ꮡt.Errorf("Header mismatch:\ngot  %#v\nwant %#v"u8, (~r).Header, want);
        }
    }
    (var b, err) = io.ReadAll(new gzip_test_package.gzip_ReaderжReader(r));
    if (err != default!) {
        Ꮡt.Fatalf("ReadAll: %v"u8, err);
    }
    if (len(b) != 0) {
        Ꮡt.Fatalf("got %d bytes, want 0"u8, len(b));
    }
    {
        var errΔ2 = r.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatalf("Reader.Close: %v"u8, errΔ2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string commentˢ = "comment"u8;
internal static readonly @string nameˢ = "name"u8;
internal static readonly object payloadˢ = (@string)"payload"u8;
internal static readonly object extraˢ = (@string)"extra"u8;

// TestRoundTrip tests that gzipping and then gunzipping is the identity
// function.
public static void TestRoundTrip(ж<testing.T> Ꮡt) {
    var buf = @new<bytes.Buffer>();
    var w = NewWriter(new gzip_test_package.bytes_BufferжWriter(buf));
    w.Value.Comment = commentˢ;
    w.Value.Extra = slice<byte>("extra"u8);
    w.Value.ModTime = time.Unix(100000000, 0);
    w.Value.Name = nameˢ;
    {
        var (_, errΔ1) = w.Write(slice<byte>("payload"u8)); if (errΔ1 != default!) {
            Ꮡt.Fatalf("Write: %v"u8, errΔ1);
        }
    }
    {
        var errΔ2 = w.Close(); if (errΔ2 != default!) {
            Ꮡt.Fatalf("Writer.Close: %v"u8, errΔ2);
        }
    }
    var (r, err) = NewReader(new gzip_test_package.bytes_BufferжReader(buf));
    if (err != default!) {
        Ꮡt.Fatalf("NewReader: %v"u8, err);
    }
    (var b, err) = io.ReadAll(new gzip_test_package.gzip_ReaderжReader(r));
    if (err != default!) {
        Ꮡt.Fatalf("ReadAll: %v"u8, err);
    }
    if (((sstring)b) != "payload"u8) {
        Ꮡt.Fatalf("payload is %q, want %q"u8, ((@string)b), payloadˢ);
    }
    if ((~r).Comment != "comment"u8) {
        Ꮡt.Fatalf("comment is %q, want %q"u8, (~r).Comment, commentˢ);
    }
    if (((sstring)(~r).Extra) != "extra"u8) {
        Ꮡt.Fatalf("extra is %q, want %q"u8, (~r).Extra, extraˢ);
    }
    if ((~r).ModTime.Unix() != 100000000) {
        Ꮡt.Fatalf("mtime is %d, want %d"u8, (~r).ModTime.Unix(), (uint32)100000000);
    }
    if ((~r).Name != "name"u8) {
        Ꮡt.Fatalf("name is %q, want %q"u8, (~r).Name, nameˢ);
    }
    {
        var errΔ3 = r.Close(); if (errΔ3 != default!) {
            Ꮡt.Fatalf("Reader.Close: %v"u8, errΔ3);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string uErungˢ = "Äußerung"u8;

// TestLatin1 tests the internal functions for converting to and from Latin-1.
public static void TestLatin1(ж<testing.T> Ꮡt) {
    var latin1 = new byte[]{0xc4, (rune)'u', 0xdf, (rune)'e', (rune)'r', (rune)'u', (rune)'n', (rune)'g', 0}.slice();
    @string utf8 = uErungˢ;
    var z = new Reader(r: new gzip_test_package.bufio_ReaderжReader(bufio.NewReader(new gzip_test_package.bytes_ReaderжReader(bytes.NewReader(latin1)))));
    var (s, err) = z.readString();
    if (err != default!) {
        Ꮡt.Fatalf("readString: %v"u8, err);
    }
    if (s != utf8) {
        Ꮡt.Fatalf("read latin-1: got %q, want %q"u8, s, utf8);
    }
    var buf = bytes.NewBuffer(new slice<byte>(0, len(latin1)));
    var c = new Writer(w: new gzip_test_package.bytes_BufferжWriter(buf));
    {
        err = c.writeString(utf8); if (err != default!) {
            Ꮡt.Fatalf("writeString: %v"u8, err);
        }
    }
    s = buf.String();
    if (s != ((sstring)latin1)) {
        Ꮡt.Fatalf("write utf-8: got %q, want %q"u8, s, ((@string)latin1));
    }
}

internal partial struct TestLatin1RoundTrip_testCases /*dyn*/ {
    internal @string name;
    internal bool ok;
}

// TestLatin1RoundTrip tests that metadata that is representable in Latin-1
// survives a round trip.
public static void TestLatin1RoundTrip(ж<testing.T> Ꮡt) {
    var testCases = new TestLatin1RoundTrip_testCases[]{
        new(""u8, true),
        new("ASCII is OK"u8, true),
        new("unless it contains a NUL\x00"u8, false),
        new("no matter where \x00 occurs"u8, false),
        new("\x00\x00\x00"u8, false),
        new("Látin-1 also passes (U+00E1)"u8, true),
        new("but LĀtin Extended-A (U+0100) does not"u8, false),
        new("neither does 日本語"u8, false),
        new(((@string)(new byte[]{0x69, 0x6e, 0x76, 0x61, 0x6c, 0x69, 0x64, 0x20, 0x55, 0x54, 0x46, 0x2d, 0x38, 0x20, 0x61, 0x6c, 0x73, 0x6f, 0x20, 0xff, 0x61, 0x69, 0x6c, 0x73})), false),
        new("\x00 as does Látin-1 with NUL"u8, false)
    }.slice();
    foreach (var (_, tc) in testCases) {
        var buf = @new<bytes.Buffer>();
        var w = NewWriter(new gzip_test_package.bytes_BufferжWriter(buf));
        w.Value.Name = tc.name;
        var err = w.Close();
        if ((err == default!) != tc.ok) {
            Ꮡt.Errorf("Writer.Close: name = %q, err = %v"u8, tc.name, err);
            continue;
        }
        if (!tc.ok) {
            continue;
        }
        (var r, err) = NewReader(new gzip_test_package.bytes_BufferжReader(buf));
        if (err != default!) {
            Ꮡt.Errorf("NewReader: %v"u8, err);
            continue;
        }
        (_, err) = io.ReadAll(new gzip_test_package.gzip_ReaderжReader(r));
        if (err != default!) {
            Ꮡt.Errorf("ReadAll: %v"u8, err);
            continue;
        }
        if ((~r).Name != tc.name) {
            Ꮡt.Errorf("name is %q, want %q"u8, (~r).Name, tc.name);
            continue;
        }
        {
            var errΔ1 = r.Close(); if (errΔ1 != default!) {
                Ꮡt.Errorf("Reader.Close: %v"u8, errΔ1);
                continue;
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noDataAfterFirstFlushˢ = (@string)"no data after first flush"u8;
internal static readonly object flushDidnTFlushAnyDataˢ = (@string)"Flush didn't flush any data"u8;

public static void TestWriterFlush(ж<testing.T> Ꮡt) {
    var buf = @new<bytes.Buffer>();
    var w = NewWriter(new gzip_test_package.bytes_BufferжWriter(buf));
    w.Value.Comment = commentˢ;
    w.Value.Extra = slice<byte>("extra"u8);
    w.Value.ModTime = time.Unix(100000000, 0);
    w.Value.Name = nameˢ;
    nint n0 = buf.Len();
    if (n0 != 0) {
        Ꮡt.Fatalf("buffer size = %d before writes; want 0"u8, n0);
    }
    {
        var err = w.Flush(); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    nint n1 = buf.Len();
    if (n1 == 0) {
        Ꮡt.Fatal(noDataAfterFirstFlushˢ);
    }
    w.Write(slice<byte>("x"u8));
    nint n2 = buf.Len();
    if (n1 != n2) {
        Ꮡt.Fatalf("after writing a single byte, size changed from %d to %d; want no change"u8, n1, n2);
    }
    {
        var err = w.Flush(); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    nint n3 = buf.Len();
    if (n2 == n3) {
        Ꮡt.Fatal(flushDidnTFlushAnyDataˢ);
    }
    {
        var err = w.Close(); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object helloWorldˢ = (@string)"hello world"u8;

// Multiple gzip files concatenated form a valid gzip file.
public static void TestConcat(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var w = NewWriter(new gzip_test_package.bytes_BufferжWriter(Ꮡbuf));
    w.Write(slice<byte>("hello "u8));
    w.Close();
    w = NewWriter(new gzip_test_package.bytes_BufferжWriter(Ꮡbuf));
    w.Write(slice<byte>("world\n"u8));
    w.Close();
    var (r, err) = NewReader(new gzip_test_package.bytes_BufferжReader(Ꮡbuf));
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var data, err) = io.ReadAll(new gzip_test_package.gzip_ReaderжReader(r));
    if (((sstring)data) != "hello world\n"u8 || err != default!) {
        Ꮡt.Fatalf("ReadAll = %q, %v, want %q, nil"u8, data, err, helloWorldˢ);
    }
}

public static void TestWriterReset(ж<testing.T> Ꮡt) {
    var buf = @new<bytes.Buffer>();
    var buf2 = @new<bytes.Buffer>();
    var z = NewWriter(new gzip_test_package.bytes_BufferжWriter(buf));
    var msg = slice<byte>("hello world"u8);
    z.Write(msg);
    z.Close();
    z.Reset(new gzip_test_package.bytes_BufferжWriter(buf2));
    z.Write(msg);
    z.Close();
    if (buf.String() != buf2.String()) {
        Ꮡt.Errorf("buf2 %q != original buf of %q"u8, buf2.String(), buf.String());
    }
}

internal partial struct limitedWriter {
    public nint N;
}

internal static (nint n, error err) Write(this ref limitedWriter l, slice<byte> p) {
    {
        nint nΔ1 = l.N; if (nΔ1 < len(p)) {
            l.N = 0;
            return (nΔ1, io.ErrShortWrite);
        }
    }
    l.N -= len(p);
    return (len(p), default!);
}

// Write should never return more bytes than the input slice.
public static void TestLimitedWrite(ж<testing.T> Ꮡt) {
    var msg = slice<byte>("a"u8);
    for (nint limᴛ1 = 2; limᴛ1 < 20; limᴛ1++) {
        ref var lim = ref heap<nint>(out var Ꮡlim);
        lim = limᴛ1;
        var z = NewWriter(new gzip_internal_test_package.limitedWriterжWriter(Ꮡ(new limitedWriter(lim))));
        {
            var (n, _) = z.Write(msg); if (n > len(msg)) {
                Ꮡt.Errorf("Write() = %d, want %d or less"u8, n, len(msg));
            }
        }
        z.Reset(new gzip_internal_test_package.limitedWriterжWriter(Ꮡ(new limitedWriter(lim))));
        z.Value.Header = new Header(
            Comment: "comment"u8,
            Extra: slice<byte>("extra"u8),
            ModTime: time.Now(),
            Name: "name"u8,
            OS: 1
        );
        {
            var (n, _) = z.Write(msg); if (n > len(msg)) {
                Ꮡt.Errorf("Write() = %d, want %d or less"u8, n, len(msg));
            }
        }
        limᴛ1 = lim;
    }
}

} // end gzip_internal_test_package
