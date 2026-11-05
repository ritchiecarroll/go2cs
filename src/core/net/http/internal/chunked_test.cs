// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.net.http;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using strings = strings_package;
using testing = testing_package;
using iotest = go.testing.iotest_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.testing;
using static go.net.http.internal_package;

partial class internal_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string hello17Worldˢ => ref hello17Worldˢᵇ;
private static readonly @string hello17Worldˢᵇ = "7\r\nhello, \r\n17\r\nworld! 0123456789abcdef\r\n0\r\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string chunk1ᶜ => ref chunk1ᶜᵇ;
private static readonly @string chunk1ᶜᵇ = "hello, "u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string chunk2ᶜ => ref chunk2ᶜᵇ;
private static readonly @string chunk2ᶜᵇ = "world! 0123456789abcdef"u8;

public static void TestChunk(ж<testing.T> t) {
    ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);

    var w = NewChunkedWriter(new internal_internal_test_package.bytes_BufferжWriter(Ꮡb));
    @string chunk1 = chunk1ᶜ;
    @string chunk2 = chunk2ᶜ;
    w.Write(slice<byte>(chunk1));
    w.Write(slice<byte>(chunk2));
    w.Close();

    {
        @string g = Ꮡb.String();
        @string e = hello17Worldˢ; if (g != e) {
            t.Fatalf("chunk writer wrote %q; want %q", g, e);
        }
    }

    var r = NewChunkedReader(new internal_internal_test_package.bytes_BufferжReader(Ꮡb));
    var (data, err) = io.ReadAll(r);
    if (err != nil) {
        t.Logf(@"data: ""%s""", data);
        t.Fatalf("ReadAll from reader: %v", err);
    }
    {
        @string g = (@string)data;
        @string e = chunk1 + chunk2; if (g != e) {
            t.Errorf("chunk reader read %q; want %q", g, e);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object foobarˢ = (@string)"foobar"u8;
internal static ref readonly @string foo0ˢ => ref foo0ˢᵇ;
private static readonly @string foo0ˢᵇ = "3\r\nfoo\r\n0\r\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string fillBufChunkᶜ => ref fillBufChunkᶜᵇ;
private static readonly @string fillBufChunkᶜᵇ = "0123456789a"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string shortChunkᶜ => ref shortChunkᶜᵇ;
private static readonly @string shortChunkᶜᵇ = "foo"u8;

public static void TestChunkReadMultiple(ж<testing.T> t) {
    // Bunch of small chunks, all read together.
    {
        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        var w = NewChunkedWriter(new internal_internal_test_package.bytes_BufferжWriter(Ꮡb));
        w.Write(slice<byte>("foo"u8));
        w.Write(slice<byte>("bar"u8));
        w.Close();

        var r = NewChunkedReader(new internal_internal_test_package.bytes_BufferжReader(Ꮡb));
        var buf = new slice<byte>(10);
        var (n, err) = r.Read(buf);
        if (n != 6 || !AreEqual(err, io.EOF)) {
            t.Errorf("Read = %d, %v; want 6, EOF", n, err);
        }
        buf = buf.slice(0, n);
        if (((sstring)buf) != "foobar"u8) {
            t.Errorf("Read = %q; want %q", buf, foobarˢ);
        }
    }

    // One big chunk followed by a little chunk, but the small bufio.Reader size
    // should prevent the second chunk header from being read.
    {
        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        var w = NewChunkedWriter(new internal_internal_test_package.bytes_BufferжWriter(Ꮡb));
        // fillBufChunk is 11 bytes + 3 bytes header + 2 bytes footer = 16 bytes,
        // the same as the bufio ReaderSize below (the minimum), so even
        // though we're going to try to Read with a buffer larger enough to also
        // receive "foo", the second chunk header won't be read yet.
        @string fillBufChunk = fillBufChunkᶜ;
        @string shortChunk = shortChunkᶜ;
        w.Write(slice<byte>(fillBufChunk));
        w.Write(slice<byte>(shortChunk));
        w.Close();

        var r = NewChunkedReader(new internal_internal_test_package.bufio_ReaderжReader(bufio.NewReaderSize(new internal_internal_test_package.bytes_BufferжReader(Ꮡb), 16)));
        var buf = new slice<byte>(len(fillBufChunk) + len(shortChunk));
        var (n, err) = r.Read(buf);
        if (n != len(fillBufChunk) || err != nil) {
            t.Errorf("Read = %d, %v; want %d, nil", n, err, len(fillBufChunk));
        }
        buf = buf.slice(0, n);
        if (((sstring)buf) != fillBufChunk) {
            t.Errorf("Read = %q; want %q", buf, fillBufChunk);
        }

        (n, err) = r.Read(buf);
        if (n != len(shortChunk) || !AreEqual(err, io.EOF)) {
            t.Errorf("Read = %d, %v; want %d, EOF", n, err, len(shortChunk));
        }
    }

    // And test that we see an EOF chunk, even though our buffer is already full:
    {
        var r = NewChunkedReader(new internal_internal_test_package.bufio_ReaderжReader(bufio.NewReader(new internal_internal_test_package.strings_ReaderжReader(strings.NewReader(foo0ˢ)))));
        var buf = new slice<byte>(3);
        var (n, err) = r.Read(buf);
        if (n != 3 || !AreEqual(err, io.EOF)) {
            t.Errorf("Read = %d, %v; want 3, EOF", n, err);
        }
        if (((sstring)buf) != "foo"u8) {
            t.Errorf("buf = %q; want foo", buf);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;

public static void TestChunkReaderAllocs(ж<testing.T> t) {
    if (testing.Short()) {
        t.Skip(skippingInShortModeˢ);
    }
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var w = NewChunkedWriter(new internal_internal_test_package.bytes_BufferжWriter(Ꮡbuf));
    var (a, b, c) = (slice<byte>("aaaaaa"u8), slice<byte>("bbbbbbbbbbbb"u8), slice<byte>("cccccccccccccccccccccccc"u8));
    w.Write(a);
    w.Write(b);
    w.Write(c);
    w.Close();

    var readBuf = new slice<byte>(len(a) + len(b) + len(c) + 1);
    var byter = bytes.NewReader(buf.Bytes());
    var bufr = bufio.NewReader(new internal_internal_test_package.bytes_ReaderжReader(byter));
    var bufrʗ1 = bufr;
    var byterʗ1 = byter;
    var readBufʗ1 = readBuf;
    var mallocs = testing.AllocsPerRun(100, () => {
        byterʗ1.Seek(0, io.SeekStart);
        bufrʗ1.Reset(new internal_internal_test_package.bytes_ReaderжReader(byterʗ1));
        var r = NewChunkedReader(new internal_internal_test_package.bufio_ReaderжReader(bufrʗ1));
        var (n, err) = io.ReadFull(r, readBufʗ1);
        if (n != len(readBufʗ1) - 1) {
            t.Fatalf("read %d bytes; want %d", n, len(readBufʗ1) - 1);
        }
        if (!AreEqual(err, io.ErrUnexpectedEOF)) {
            t.Fatalf("read error = %v; want ErrUnexpectedEOF", err);
        }
    });
    if (mallocs > 1.5D) {
        t.Errorf("mallocs = %v; want 1", mallocs);
    }
}

internal partial struct TestParseHexUint_testCase /*dyn*/ {
    internal @string @in;
    internal uint64 want;
    internal @string wantErr;
}

public static void TestParseHexUint(ж<testing.T> t) {
    var tests = slice<TestParseHexUint_testCase>([
        new("x", 0, "invalid byte in chunk length"),
        new("0000000000000000", 0, ""),
        new("0000000000000001", 1, ""),
        new("ffffffffffffffff", 18446744073709551615UL, ""),
        new("000000000000bogus", 0, "invalid byte in chunk length"),
        new("00000000000000000", 0, "http chunk length too large"), // could accept if we wanted
        new("10000000000000000", 0, "http chunk length too large"),
        new("00000000000000001", 0, "http chunk length too large"), // could accept if we wanted
        new("", 0, "empty hex number for chunk length")
    ]);
    for (var i = (uint64)0; i <= 1234; i++) {
        tests = append(tests, new TestParseHexUint_testCase(@in: fmt.Sprintf("%x"u8, i), want: i));
    }
    foreach (var (_, tt) in tests) {
        var (got, err) = parseHexUint(slice<byte>(tt.@in));
        if (tt.wantErr != ""u8) {
            if (!strings.Contains(fmt.Sprint(err), tt.wantErr)) {
                t.Errorf("parseHexUint(%q) = %v, %v; want error %q", tt.@in, got, err, tt.wantErr);
            }
        } else {
            if (err != nil || got != tt.want) {
                t.Errorf("parseHexUint(%q) = %v, %v; want %v", tt.@in, got, err, tt.want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloWorldˢ => ref helloWorldˢᵇ;
private static readonly @string helloWorldˢᵇ = "hello, world! 0123456789abcdef"u8;

public static void TestChunkReadingIgnoresExtensions(ж<testing.T> t) {
    @string @in = "7;ext=\"some quoted string\"\r\n"u8 + // token=quoted string
        "hello, \r\n"u8 + "17;someext\r\n"u8 + // token without value
        "world! 0123456789abcdef\r\n"u8 + "0;someextension=sometoken\r\n"u8; // token=token
    var (data, err) = io.ReadAll(NewChunkedReader(new internal_internal_test_package.strings_ReaderжReader(strings.NewReader(@in))));
    if (err != nil) {
        t.Fatalf("ReadAll = %q, %v", data, err);
    }
    {
        @string g = (@string)data;
        @string e = helloWorldˢ; if (g != e) {
            t.Errorf("read %q; want %q", g, e);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string malformedˢ => ref malformedˢᵇ;
private static readonly @string malformedˢᵇ = "malformed"u8;

// Issue 17355: ChunkedReader shouldn't block waiting for more data
// if it can return something.
public static partial void TestChunkReadPartial(ж<testing.T> t) {
    var (pr, pw) = io.Pipe();
    var pwʗ1 = pw;
    goǃ(() => {
        pwʗ1.Write(slice<byte>("7\r\n1234567"u8));
    });
    var cr = NewChunkedReader(new io.PipeReaderжReader(pr));
    var readBuf = new slice<byte>(7);
    var (n, err) = cr.Read(readBuf);
    if (err != nil) {
        t.Fatal(err);
    }
    @string want = "1234567";
    if (n != 7 || ((sstring)readBuf) != want) {
        t.Fatalf("Read: %v %q; want %d, %q", n, readBuf.slice(0, n), len(want), want);
    }
    var pwʗ2 = pw;
    goǃ(() => {
        pwʗ2.Write(slice<byte>("xx"u8));
    });
    (_, err) = cr.Read(readBuf);
    {
        @string got = fmt.Sprint(err); if (!strings.Contains(got, malformedˢ)) {
            t.Fatalf("second read = %v; want malformed error", err);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string validᶜ => ref validᶜᵇ;
private static readonly @string validᶜᵇ = "4\r\nabcd\r\n5\r\nabc\r\n\r\n0\r\n";

// Issue 48861: ChunkedReader should report incomplete chunks
public static void TestIncompleteChunk(ж<testing.T> t) {
    @string valid = validᶜ;

    for (nint i = 0; i < len(valid); i++) {
        @string incomplete = valid.slice(0, i);
        var rΔ1 = NewChunkedReader(new internal_internal_test_package.strings_ReaderжReader(strings.NewReader(incomplete)));
        {
            var (_, err) = io.ReadAll(rΔ1); if (!AreEqual(err, io.ErrUnexpectedEOF)) {
                t.Errorf("expected io.ErrUnexpectedEOF for %q, got %v", incomplete, err);
            }
        }
    }

    var r = NewChunkedReader(new internal_internal_test_package.strings_ReaderжReader(strings.NewReader(valid)));
    {
        var (_, err) = io.ReadAll(r); if (err != nil) {
            t.Errorf("unexpected error for %q: %v", valid, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string abcdˢ => ref abcdˢᵇ;
private static readonly @string abcdˢᵇ = "4\r\nabcd"u8;

public static void TestChunkEndReadError(ж<testing.T> t) {
    var readErr = fmt.Errorf("chunk end read error"u8);

    var r = NewChunkedReader(io.MultiReader(new internal_internal_test_package.strings_ReaderжReader(strings.NewReader(abcdˢ)), iotest.ErrReader(readErr)));
    {
        var (_, err) = io.ReadAll(r); if (!AreEqual(err, readErr)) {
            t.Errorf("expected %v, got %v", readErr, err);
        }
    }
}

public static void TestChunkReaderTooMuchOverhead(ж<testing.T> t) {
    // If the sender is sending 100x as many chunk header bytes as chunk data,
    // we should reject the stream at some point.
    var chunk = slice<byte>("1;"u8);
    for (nint i = 0; i < 100; i++) {
        chunk = append(chunk, (byte)'a'); // chunk extension
    }
    chunk = append(chunk, ((@string)"\r\nX\r\n").ꓸꓸꓸ);
    const nint bodylen = /* 1 << 20 */ 1048576;
    var chunkʗ1 = chunk;
    var r = NewChunkedReader(new internal_internal_test_package.funcReaderжReader(Ꮡ(new funcReader(f: (nint i) => {
        if (i < bodylen) {
            return (chunkʗ1, default);
        }
        return (slice<byte>("0\r\n"u8), default);
    }
    ))));
    var (_, err) = io.ReadAll(r);
    if (err == nil) {
        t.Fatalf("successfully read body with excessive overhead; want error");
    }
}

public static void TestChunkReaderByteAtATime(ж<testing.T> t) {
    // Sending one byte per chunk should not trip the excess-overhead detection.
    const nint bodylen = /* 1 << 20 */ 1048576;
    var r = NewChunkedReader(new internal_internal_test_package.funcReaderжReader(Ꮡ(new funcReader(f: (nint i) => {
        if (i < bodylen) {
            return (slice<byte>("1\r\nX\r\n"u8), default);
        }
        return (slice<byte>("0\r\n"u8), default);
    }
    ))));
    var (got, err) = io.ReadAll(r);
    if (err != nil) {
        t.Errorf("unexpected error: %v", err);
    }
    if (len(got) != bodylen) {
        t.Errorf("read %v bytes, want %v", len(got), (nint)(bodylen));
    }
}

internal partial struct TestChunkInvalidInputs_type /*dyn*/ {
    internal @string name;
    internal @string b;
}

public static void TestChunkInvalidInputs(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in slice<TestChunkInvalidInputs_type>([new(
        name: "bare LF in chunk size",
        b: "1\na\r\n0\r\n"
    ), new(
        name: "extra LF in chunk size",
        b: "1\r\r\na\r\n0\r\n"
    ), new(
        name: "bare LF in chunk data",
        b: "1\r\na\n0\r\n"
    ), new(
        name: "bare LF in chunk extension",
        b: "1;\na\r\n0\r\n"
    )
    ])) {
        ref var test = ref heap(new TestChunkInvalidInputs_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            var r = NewChunkedReader(new internal_internal_test_package.strings_ReaderжReader(strings.NewReader(testʗ1.b)));
            var (got, err) = io.ReadAll(r);
            if (err == nil) {
                tΔ1.Fatalf("unexpectedly parsed invalid chunked data:\n%q", got);
            }
        });
    }
}

internal partial struct funcReader {
    internal Func<nint, (slice<byte>, error)> f;
    internal nint i;
    internal slice<byte> b;
    internal error err;
}

internal static (nint n, error err) Read(this ref funcReader r, slice<byte> p) {
    nint n = default;

    if (len(r.b) == 0 && r.err == nil) {
        (r.b, r.err) = r.f(r.i);
        r.i++;
    }
    n = copy(p, r.b);
    r.b = r.b.slice(n);
    if (len(r.b) > 0) {
        return (n, default);
    }
    return (n, r.err);
}

} // end internal_internal_test_package
