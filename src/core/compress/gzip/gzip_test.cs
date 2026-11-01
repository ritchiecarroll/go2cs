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
public static void TestEmpty(ж<testing.T> t) {
    var buf = @new<bytes.Buffer>();

    {
        var errΔ1 = NewWriter(new gzip_test_package.bytes_BufferжWriter(buf)).Close(); if (errΔ1 != nil) {
            t.Fatalf("Writer.Close: %v", errΔ1);
        }
    }

    var (r, err) = NewReader(new gzip_test_package.bytes_BufferжReader(buf));
    if (err != nil) {
        t.Fatalf("NewReader: %v", err);
    }
    {
        var want = (new Header(OS: 255)); if (!reflect.DeepEqual(r.Header, want)) {
            t.Errorf("Header mismatch:\ngot  %#v\nwant %#v", r.Header, want);
        }
    }
    (var b, err) = io.ReadAll(new gzip_test_package.gzip_ReaderжReader(r));
    if (err != nil) {
        t.Fatalf("ReadAll: %v", err);
    }
    if (len(b) != 0) {
        t.Fatalf("got %d bytes, want 0", len(b));
    }
    {
        var errΔ2 = r.Close(); if (errΔ2 != nil) {
            t.Fatalf("Reader.Close: %v", errΔ2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string commentˢ => ref commentˢᵇ;
private static readonly @string commentˢᵇ = "comment"u8;
internal static ref readonly @string nameˢ => ref nameˢᵇ;
private static readonly @string nameˢᵇ = "name"u8;
internal static readonly object payloadˢ = (@string)"payload"u8;
internal static readonly object extraˢ = (@string)"extra"u8;

// TestRoundTrip tests that gzipping and then gunzipping is the identity
// function.
public static void TestRoundTrip(ж<testing.T> t) {
    var buf = @new<bytes.Buffer>();

    var w = NewWriter(new gzip_test_package.bytes_BufferжWriter(buf));
    w.Comment = commentˢ;
    w.Extra = slice<byte>("extra"u8);
    w.ModTime = time.Unix(100000000, 0);
    w.Name = nameˢ;
    {
        var (_, errΔ1) = w.Write(slice<byte>("payload"u8)); if (errΔ1 != nil) {
            t.Fatalf("Write: %v", errΔ1);
        }
    }
    {
        var errΔ2 = w.Close(); if (errΔ2 != nil) {
            t.Fatalf("Writer.Close: %v", errΔ2);
        }
    }

    var (r, err) = NewReader(new gzip_test_package.bytes_BufferжReader(buf));
    if (err != nil) {
        t.Fatalf("NewReader: %v", err);
    }
    (var b, err) = io.ReadAll(new gzip_test_package.gzip_ReaderжReader(r));
    if (err != nil) {
        t.Fatalf("ReadAll: %v", err);
    }
    if (((sstring)b) != "payload"u8) {
        t.Fatalf("payload is %q, want %q", (@string)b, payloadˢ);
    }
    if (r.Comment != "comment"u8) {
        t.Fatalf("comment is %q, want %q", r.Comment, commentˢ);
    }
    if (((sstring)r.Extra) != "extra"u8) {
        t.Fatalf("extra is %q, want %q", r.Extra, extraˢ);
    }
    if (r.ModTime.Unix() != 100000000) {
        t.Fatalf("mtime is %d, want %d", r.ModTime.Unix(), (uint32)100000000);
    }
    if (r.Name != "name"u8) {
        t.Fatalf("name is %q, want %q", r.Name, nameˢ);
    }
    {
        var errΔ3 = r.Close(); if (errΔ3 != nil) {
            t.Fatalf("Reader.Close: %v", errΔ3);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string uErungˢ => ref uErungˢᵇ;
private static readonly @string uErungˢᵇ = "Äußerung"u8;

// TestLatin1 tests the internal functions for converting to and from Latin-1.
public static void TestLatin1(ж<testing.T> t) {
    var latin1 = slice<byte>([0xc4, (byte)'u', 0xdf, (byte)'e', (byte)'r', (byte)'u', (byte)'n', (byte)'g', 0]);
    @string utf8 = uErungˢ;
    var z = new Reader(r: new gzip_test_package.bufio_ReaderжReader(bufio.NewReader(new gzip_test_package.bytes_ReaderжReader(bytes.NewReader(latin1)))));
    var (s, err) = z.readString();
    if (err != nil) {
        t.Fatalf("readString: %v", err);
    }
    if (s != utf8) {
        t.Fatalf("read latin-1: got %q, want %q", s, utf8);
    }

    var buf = bytes.NewBuffer(new slice<byte>(0, len(latin1)));
    var c = new Writer(w: new gzip_test_package.bytes_BufferжWriter(buf));
    {
        err = c.writeString(utf8); if (err != nil) {
            t.Fatalf("writeString: %v", err);
        }
    }
    s = buf.String();
    if (s != ((sstring)latin1)) {
        t.Fatalf("write utf-8: got %q, want %q", s, (@string)latin1);
    }
}

internal partial struct TestLatin1RoundTrip_testCases /*dyn*/ {
    internal @string name;
    internal bool ok;
}

// TestLatin1RoundTrip tests that metadata that is representable in Latin-1
// survives a round trip.
public static void TestLatin1RoundTrip(ж<testing.T> t) {
    var testCases = slice<TestLatin1RoundTrip_testCases>([
        new("", true),
        new("ASCII is OK", true),
        new("unless it contains a NUL\x00", false),
        new("no matter where \x00 occurs", false),
        new("\x00\x00\x00", false),
        new("Látin-1 also passes (U+00E1)", true),
        new("but LĀtin Extended-A (U+0100) does not", false),
        new("neither does 日本語", false),
        new(((@string)(new byte[]{0x69, 0x6e, 0x76, 0x61, 0x6c, 0x69, 0x64, 0x20, 0x55, 0x54, 0x46, 0x2d, 0x38, 0x20, 0x61, 0x6c, 0x73, 0x6f, 0x20, 0xff, 0x61, 0x69, 0x6c, 0x73})), false),
        new("\x00 as does Látin-1 with NUL", false)
    ]);
    foreach (var (_, tc) in testCases) {
        var buf = @new<bytes.Buffer>();

        var w = NewWriter(new gzip_test_package.bytes_BufferжWriter(buf));
        w.Name = tc.name;
        var err = w.Close();
        if ((err == nil) != tc.ok) {
            t.Errorf("Writer.Close: name = %q, err = %v", tc.name, err);
            continue;
        }
        if (!tc.ok) {
            continue;
        }

        (var r, err) = NewReader(new gzip_test_package.bytes_BufferжReader(buf));
        if (err != nil) {
            t.Errorf("NewReader: %v", err);
            continue;
        }
        (_, err) = io.ReadAll(new gzip_test_package.gzip_ReaderжReader(r));
        if (err != nil) {
            t.Errorf("ReadAll: %v", err);
            continue;
        }
        if (r.Name != tc.name) {
            t.Errorf("name is %q, want %q", r.Name, tc.name);
            continue;
        }
        {
            var errΔ1 = r.Close(); if (errΔ1 != nil) {
                t.Errorf("Reader.Close: %v", errΔ1);
                continue;
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noDataAfterFirstFlushˢ = (@string)"no data after first flush"u8;
internal static readonly object flushDidnTFlushAnyDataˢ = (@string)"Flush didn't flush any data"u8;

public static void TestWriterFlush(ж<testing.T> t) {
    var buf = @new<bytes.Buffer>();

    var w = NewWriter(new gzip_test_package.bytes_BufferжWriter(buf));
    w.Comment = commentˢ;
    w.Extra = slice<byte>("extra"u8);
    w.ModTime = time.Unix(100000000, 0);
    w.Name = nameˢ;

    nint n0 = buf.Len();
    if (n0 != 0) {
        t.Fatalf("buffer size = %d before writes; want 0", n0);
    }

    {
        var err = w.Flush(); if (err != nil) {
            t.Fatal(err);
        }
    }

    nint n1 = buf.Len();
    if (n1 == 0) {
        t.Fatal(noDataAfterFirstFlushˢ);
    }

    w.Write(slice<byte>("x"u8));

    nint n2 = buf.Len();
    if (n1 != n2) {
        t.Fatalf("after writing a single byte, size changed from %d to %d; want no change", n1, n2);
    }

    {
        var err = w.Flush(); if (err != nil) {
            t.Fatal(err);
        }
    }

    nint n3 = buf.Len();
    if (n2 == n3) {
        t.Fatal(flushDidnTFlushAnyDataˢ);
    }

    {
        var err = w.Close(); if (err != nil) {
            t.Fatal(err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object helloWorldˢ = (@string)"hello world"u8;

// Multiple gzip files concatenated form a valid gzip file.
public static void TestConcat(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var w = NewWriter(new gzip_test_package.bytes_BufferжWriter(Ꮡbuf));
    w.Write(slice<byte>("hello "u8));
    w.Close();
    w = NewWriter(new gzip_test_package.bytes_BufferжWriter(Ꮡbuf));
    w.Write(slice<byte>("world\n"u8));
    w.Close();

    var (r, err) = NewReader(new gzip_test_package.bytes_BufferжReader(Ꮡbuf));
    if (err != nil) {
        t.Fatal(err);
    }
    (var data, err) = io.ReadAll(new gzip_test_package.gzip_ReaderжReader(r));
    if (((sstring)data) != "hello world\n"u8 || err != nil) {
        t.Fatalf("ReadAll = %q, %v, want %q, nil", data, err, helloWorldˢ);
    }
}

public static void TestWriterReset(ж<testing.T> t) {
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
        t.Errorf("buf2 %q != original buf of %q", buf2.String(), buf.String());
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
    return (len(p), default);
}

// Write should never return more bytes than the input slice.
public static void TestLimitedWrite(ж<testing.T> t) {
    var msg = slice<byte>("a"u8);

    for (nint limᴛ1 = 2; limᴛ1 < 20; limᴛ1++) {
        ref var lim = ref heap<nint>(out var Ꮡlim);
        lim = limᴛ1;
        var z = NewWriter(new gzip_internal_test_package.limitedWriterжWriter(Ꮡ(new limitedWriter(lim))));
        {
            var (n, _) = z.Write(msg); if (n > len(msg)) {
                t.Errorf("Write() = %d, want %d or less", n, len(msg));
            }
        }

        z.Reset(new gzip_internal_test_package.limitedWriterжWriter(Ꮡ(new limitedWriter(lim))));
        z.Header = new Header(
            Comment: "comment",
            Extra: slice<byte>("extra"u8),
            ModTime: time.Now(),
            Name: "name",
            OS: 1
        );
        {
            var (n, _) = z.Write(msg); if (n > len(msg)) {
                t.Errorf("Write() = %d, want %d or less", n, len(msg));
            }
        }
        limᴛ1 = lim;
    }
}

} // end gzip_internal_test_package
