// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using asan = @internal.asan_package;
using Δio = io_package;
using rand = math.rand_package;
using strconv = strconv_package;
using strings = strings_package;
using Δtesting = testing_package;
using iotest = go.testing.iotest_package;
using time = time_package;
using utf8 = go.unicode.utf8_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using bufio = bufio_package;
using go.testing;
using go.unicode;
using math;
using static go.bufio_internal_test_package;

partial class bufio_test_package {

// Reads from a reader and rot13s the result.
partial struct rot13Reader {
    internal Δio.Reader r;
}

internal static ж<rot13Reader> newRot13Reader(Δio.Reader r) {
    var r13 = @new<rot13Reader>();
    r13.r = r;
    return r13;
}

internal static (nint, error) Read(this ref rot13Reader r13, slice<byte> p) {
    var (n, err) = r13.r.Read(p);
    for (nint i = 0; i < n; i++) {
        var c = (byte)(p[i] | 0x20); // lowercase byte
        if ('a' <= c && c <= 'm') {
            p[i] += 13;
        } else if ('n' <= c && c <= 'z') {
            p[i] -= 13;
        }
    }
    return (n, err);
}

// Call ReadByte to accumulate the text of a file
internal static @string readBytes(ж<bufio.Reader> Ꮡbuf) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    array<byte> b = new(1000);
    nint nb = 0;
    for (;;) {
        var (c, err) = buf.ReadByte();
        if (AreEqual(err, Δio.EOF)) {
            break;
        }
        if (err == nil) {
            b[nb] = c;
            nb++;
        } else if (!AreEqual(err, iotest.ErrTimeout)) {
            throw panic("Data: " + err.Error());
        }
    }
    return (@string)(b.slice(0, nb));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloWorldˢ => ref helloWorldˢᵇ;
private static readonly @string helloWorldˢᵇ = "hello world"u8;

public static void TestReaderSimple(ж<Δtesting.T> t) {
    @string data = helloWorldˢ;
    var b = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(data)));
    {
        @string s = readBytes(b); if (s != "hello world"u8) {
            t.Errorf("simple hello world test failed: got %q", s);
        }
    }

    b = NewReader(new bufio_test_package.rot13ReaderжReader(newRot13Reader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(data)))));
    {
        @string s = readBytes(b); if (s != "uryyb jbeyq"u8) {
            t.Errorf("rot13 hello world test failed: got %q", s);
        }
    }
}

partial struct readMaker {
    internal @string name;
    internal Func<Δio.Reader, Δio.Reader> fn;
}

internal static slice<readMaker> readMakers = slice<readMaker>([
    new("full", (Δio.Reader r) => r),
    new("byte", iotest.OneByteReader),
    new("half", iotest.HalfReader),
    new("data+err", iotest.DataErrReader),
    new("timeout", iotest.TimeoutReader)
]);

// Call ReadString (which ends up calling everything else)
// to accumulate the text of a file.
internal static @string readLines(ж<bufio.Reader> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string s = "";
    for (;;) {
        var (s1, err) = b.ReadString((byte)'\n');
        if (AreEqual(err, Δio.EOF)) {
            break;
        }
        if (err != nil && !AreEqual(err, iotest.ErrTimeout)) {
            throw panic("GetLines: " + err.Error());
        }
        s += s1;
    }
    return s;
}

// Call Read to accumulate the text of a file
internal static @string reads(ж<bufio.Reader> Ꮡbuf, nint m) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    array<byte> b = new(1000);
    nint nb = 0;
    for (;;) {
        var (n, err) = buf.Read(b.slice(nb, nb + m));
        nb += n;
        if (AreEqual(err, Δio.EOF)) {
            break;
        }
    }
    return (@string)(b.slice(0, nb));
}

partial struct bufReader {
    internal @string name;
    internal Func<ж<bufio.Reader>, @string> fn;
}

internal static slice<bufReader> bufreaders = slice<bufReader>([
    new("1", (ж<bufio.Reader> b) => reads(b, 1)),
    new("2", (ж<bufio.Reader> b) => reads(b, 2)),
    new("3", (ж<bufio.Reader> b) => reads(b, 3)),
    new("4", (ж<bufio.Reader> b) => reads(b, 4)),
    new("5", (ж<bufio.Reader> b) => reads(b, 5)),
    new("7", (ж<bufio.Reader> b) => reads(b, 7)),
    new("bytes", readBytes),
    new("lines", readLines)
]);

internal static UntypedInt minReadBufferSize => 16;

internal static slice<nint> bufsizes = slice<nint>([
    0, minReadBufferSize, 23, 32, 46, 64, 93, 128, 1024, 4096
]);

public static void TestReader(ж<Δtesting.T> t) {
    array<@string> texts = new(31);
    @string str = ""u8;
    @string all = ""u8;
    for (nint i = 0; i < len(texts) - 1; i++) {
        texts[i] = str + "\n"u8;
        all += texts[i];
        str += ((@string)(rune)(i % 26 + 'a'));
    }
    texts[len(texts) - 1] = all;

    for (nint h = 0; h < len(texts); h++) {
        @string text = texts[h];
        for (nint i = 0; i < len(readMakers); i++) {
            for (nint j = 0; j < len(bufreaders); j++) {
                for (nint k = 0; k < len(bufsizes); k++) {
                    var readmaker = readMakers[i];
                    var bufreader = bufreaders[j];
                    nint bufsize = bufsizes[k];
                    var read = readmaker.fn(new bufio_test_package.strings_ReaderжReader(strings.NewReader(text)));
                    var buf = NewReaderSize(read, bufsize);
                    @string s = bufreader.fn(buf);
                    if (s != text) {
                        t.Errorf("reader=%s fn=%s bufsize=%d want=%q got=%q",
                            readmaker.name, bufreader.name, bufsize, text, s);
                    }
                }
            }
        }
    }
}

partial struct zeroReader {
}

internal static (nint, error) Read(this zeroReader _, slice<byte> p) {
    return (0, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorExpectedˢ = (@string)"error expected"u8;
internal static readonly object unexpectedErrorˢ = (@string)"unexpected error:"u8;
internal static readonly object testTimedOutEndlessLoopˢ = (@string)"test timed out (endless loop in ReadByte?)"u8;

public static partial void TestZeroReader(ж<Δtesting.T> t) {
    zeroReader z = default;
    var r = NewReader(z);

    var c = new channel<error>(0);
    var cʗ1 = c;
    var rʗ1 = r;
    goǃ(() => {
        var (_, err) = rʗ1.ReadByte();
        cʗ1.ᐸꟷ(err);
    });

    var selᴛ1 = c;
    var selᴛ2 = time.After(time.ΔSecond);
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var err): {
        if (err == nil) {
            t.Error(errorExpectedˢ);
        } else if (!AreEqual(err, Δio.ErrNoProgress)) {
            t.Error(unexpectedErrorˢ, err);
        }
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out _): {
        t.Error(testTimedOutEndlessLoopˢ);
        break;
    }}
}

// A StringReader delivers its data one string segment at a time via Read.
partial struct StringReader {
    internal slice<@string> data;
    internal nint step;
}

public static (nint n, error err) Read(this ref StringReader r, slice<byte> p) {
    nint n = default;
    error err = default;

    if (r.step < len(r.data)) {
        @string s = r.data[r.step];
        n = copy(p, s);
        r.step++;
    } else {
        err = Δio.EOF;
    }
    return (n, err);
}

internal static void readRuneSegments(ж<Δtesting.T> t, slice<@string> segments) {
    @string got = ""u8;
    @string want = strings.Join(segments, ""u8);
    var r = NewReader(new bufio_test_package.StringReaderжReader(Ꮡ(new StringReader(data: segments))));
    for (;;) {
        var (rΔ1, _, err) = r.ReadRune();
        if (err != nil) {
            if (!AreEqual(err, Δio.EOF)) {
                return;
            }
            break;
        }
        got += ((@string)rΔ1);
    }
    if (got != want) {
        t.Errorf("segments=%v got=%s want=%s", segments, got, want);
    }
}

internal static slice<slice<@string>> segmentList = slice<slice<@string>>([
    slice<@string>([]),
    slice<@string>([""]),
    slice<@string>(["日", "本語"]),
    slice<@string>(["\u65e5", "\u672c", "\u8a9e"]),
    slice<@string>(["\U000065e5", "\U0000672c", "\U00008a9e"]),
    slice<@string>([((@string)(new byte[]{0xe6})), ((@string)(new byte[]{0x97, 0xa5, 0xe6})), ((@string)(new byte[]{0x9c, 0xac, 0xe8, 0xaa, 0x9e}))]),
    slice<@string>(["Hello", ", ", "World", "!"]),
    slice<@string>(["Hello", ", ", "", "World", "!"])
]);

public static void TestReadRune(ж<Δtesting.T> t) {
    foreach (var (_, s) in segmentList) {
        readRuneSegments(t, s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedErrorOnˢ = (@string)"unexpected error on ReadRune:"u8;
internal static readonly object unexpectedErrorOnˢ2 = (@string)"unexpected error on UnreadRune:"u8;
internal static readonly object unexpectedErrorReadingˢ = (@string)"unexpected error reading after unreading:"u8;

public static void TestUnreadRune(ж<Δtesting.T> t) {
    var segments = slice<@string>(["Hello, world:", "日本語"]);
    var r = NewReader(new bufio_test_package.StringReaderжReader(Ꮡ(new StringReader(data: segments))));
    @string got = ""u8;
    @string want = strings.Join(segments, ""u8);
    // Normal execution.
    for (;;) {
        var (r1, _, err) = r.ReadRune();
        if (err != nil) {
            if (!AreEqual(err, Δio.EOF)) {
                t.Error(unexpectedErrorOnˢ, err);
            }
            break;
        }
        got += ((@string)r1);
        // Put it back and read it again.
        {
            err = r.UnreadRune(); if (err != nil) {
                t.Fatal(unexpectedErrorOnˢ2, err);
            }
        }
        (var r2, _, err) = r.ReadRune();
        if (err != nil) {
            t.Fatal(unexpectedErrorReadingˢ, err);
        }
        if (r1 != r2) {
            t.Fatalf("incorrect rune after unread: got %c, want %c", r1, r2);
        }
    }
    if (got != want) {
        t.Errorf("got %q, want %q", got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string exampleˢ => ref exampleˢᵇ;
private static readonly @string exampleˢᵇ = "example"u8;
internal static readonly object unreadRuneDidnTFailAfterˢ = (@string)"UnreadRune didn't fail after Peek"u8;

public static void TestNoUnreadRuneAfterPeek(ж<Δtesting.T> t) {
    var br = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(exampleˢ)));
    br.ReadRune();
    br.Peek(1);
    {
        var err = br.UnreadRune(); if (err == nil) {
            t.Error(unreadRuneDidnTFailAfterˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadByteDidnTFailAfterˢ = (@string)"UnreadByte didn't fail after Peek"u8;

public static void TestNoUnreadByteAfterPeek(ж<Δtesting.T> t) {
    var br = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(exampleˢ)));
    br.ReadByte();
    br.Peek(1);
    {
        var err = br.UnreadByte(); if (err == nil) {
            t.Error(unreadByteDidnTFailAfterˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadRuneDidnTFailAfterˢ2 = (@string)"UnreadRune didn't fail after Discard"u8;

public static void TestNoUnreadRuneAfterDiscard(ж<Δtesting.T> t) {
    var br = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(exampleˢ)));
    br.ReadRune();
    br.Discard(1);
    {
        var err = br.UnreadRune(); if (err == nil) {
            t.Error(unreadRuneDidnTFailAfterˢ2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadByteDidnTFailAfterˢ2 = (@string)"UnreadByte didn't fail after Discard"u8;

public static void TestNoUnreadByteAfterDiscard(ж<Δtesting.T> t) {
    var br = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(exampleˢ)));
    br.ReadByte();
    br.Discard(1);
    {
        var err = br.UnreadByte(); if (err == nil) {
            t.Error(unreadByteDidnTFailAfterˢ2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadRuneDidnTFailAfterˢ3 = (@string)"UnreadRune didn't fail after WriteTo"u8;

public static void TestNoUnreadRuneAfterWriteTo(ж<Δtesting.T> t) {
    var br = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(exampleˢ)));
    br.WriteTo(Δio.Discard);
    {
        var err = br.UnreadRune(); if (err == nil) {
            t.Error(unreadRuneDidnTFailAfterˢ3);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadByteDidnTFailAfterˢ3 = (@string)"UnreadByte didn't fail after WriteTo"u8;

public static void TestNoUnreadByteAfterWriteTo(ж<Δtesting.T> t) {
    var br = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(exampleˢ)));
    br.WriteTo(Δio.Discard);
    {
        var err = br.UnreadByte(); if (err == nil) {
            t.Error(unreadByteDidnTFailAfterˢ3);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedErrorOnˢ3 = (@string)"unexpected error on ReadByte:"u8;
internal static readonly object unexpectedErrorOnˢ4 = (@string)"unexpected error on UnreadByte:"u8;

public static void TestUnreadByte(ж<Δtesting.T> t) {
    var segments = slice<@string>(["Hello, ", "world"]);
    var r = NewReader(new bufio_test_package.StringReaderжReader(Ꮡ(new StringReader(data: segments))));
    @string got = ""u8;
    @string want = strings.Join(segments, ""u8);
    // Normal execution.
    for (;;) {
        var (b1, err) = r.ReadByte();
        if (err != nil) {
            if (!AreEqual(err, Δio.EOF)) {
                t.Error(unexpectedErrorOnˢ3, err);
            }
            break;
        }
        got += ((@string)b1);
        // Put it back and read it again.
        {
            err = r.UnreadByte(); if (err != nil) {
                t.Fatal(unexpectedErrorOnˢ4, err);
            }
        }
        (var b2, err) = r.ReadByte();
        if (err != nil) {
            t.Fatal(unexpectedErrorReadingˢ, err);
        }
        if (b1 != b2) {
            t.Fatalf("incorrect byte after unread: got %q, want %q", b1, b2);
        }
    }
    if (got != want) {
        t.Errorf("got %q, want %q", got, want);
    }
}

public static void TestUnreadByteMultiple(ж<Δtesting.T> t) {
    var segments = slice<@string>(["Hello, ", "world"]);
    @string data = strings.Join(segments, ""u8);
    for (nint n = 0; n <= len(data); n++) {
        var r = NewReader(new bufio_test_package.StringReaderжReader(Ꮡ(new StringReader(data: segments))));
        // Read n bytes.
        for (nint i = 0; i < n; i++) {
            var (b, err) = r.ReadByte();
            if (err != nil) {
                t.Fatalf("n = %d: unexpected error on ReadByte: %v", n, err);
            }
            if (b != data[i]) {
                t.Fatalf("n = %d: incorrect byte returned from ReadByte: got %q, want %q", n, b, data[i]);
            }
        }
        // Unread one byte if there is one.
        if (n > 0) {
            {
                var err = r.UnreadByte(); if (err != nil) {
                    t.Errorf("n = %d: unexpected error on UnreadByte: %v", n, err);
                }
            }
        }
        // Test that we cannot unread any further.
        {
            var err = r.UnreadByte(); if (err == nil) {
                t.Errorf("n = %d: expected error on UnreadByte", n);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string abcdefgˢ => ref abcdefgˢᵇ;
private static readonly @string abcdefgˢᵇ = "abcdefg"u8;
internal static ref readonly @string abcdˢ => ref abcdˢᵇ;
private static readonly @string abcdˢᵇ = "abcd"u8;
internal static ref readonly @string efgˢ => ref efgˢᵇ;
private static readonly @string efgˢᵇ = "efg"u8;

public static void TestUnreadByteOthers(ж<Δtesting.T> t) {
    // A list of readers to use in conjunction with UnreadByte.
    slice<Func<ж<bufio.Reader>, byte, (slice<byte>, error)>> readers = slice<Func<ж<bufio.Reader>, byte, (slice<byte>, error)>>([
        ((Func<ж<bufio.Reader>, byte, (slice<byte>, error)>)(bufio.ReadBytes)),
        ((Func<ж<bufio.Reader>, byte, (slice<byte>, error)>)(bufio.ReadSlice)),
        (ж<bufio.Reader> r, byte delim) => {
            var (data, err) = r.ReadString(delim);
            return (slice<byte>(data), err);
        }
        // ReadLine doesn't fit the data/pattern easily
        // so we leave it out. It should be covered via
        // the ReadSlice test since ReadLine simply calls
        // ReadSlice, and it's that function that handles
        // the last byte.
    ]);

    // Try all readers with UnreadByte.
    foreach (var (rno, read) in readers) {
        // Some input data that is longer than the minimum reader buffer size.
        const nint n = 10;
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        for (nint i = 0; i < n; i++) {
            buf.WriteString(abcdefgˢ);
        }

        var r = NewReaderSize(new bufio_test_package.bytes_BufferжReader(Ꮡbuf), minReadBufferSize);
        var rʗ1 = r;
        var readʗ1 = read;
        void readTo(byte delim, @string want) {
            var (data, errΔ1) = readʗ1(rʗ1, delim);
            if (errΔ1 != nil) {
                t.Fatalf("#%d: unexpected error reading to %c: %v", rno, delim, errΔ1);
            }
            {
                @string got = (@string)data; if (got != want) {
                    t.Fatalf("#%d: got %q, want %q", rno, got, want);
                }
            }
        }

        // Read the data with occasional UnreadByte calls.
        for (nint i = 0; i < n; i++) {
            readTo((byte)'d', abcdˢ);
            for (nint j = 0; j < 3; j++) {
                {
                    var errΔ2 = r.UnreadByte(); if (errΔ2 != nil) {
                        t.Fatalf("#%d: unexpected error on UnreadByte: %v", rno, errΔ2);
                    }
                }
                readTo((byte)'d', "d");
            }
            readTo((byte)'g', efgˢ);
        }

        // All data should have been read.
        var (_, err) = r.ReadByte();
        if (!AreEqual(err, Δio.EOF)) {
            t.Errorf("#%d: got error %v; want EOF", rno, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorOnˢ = (@string)"expected error on UnreadRune from fresh buffer"u8;
internal static readonly object unexpectedErrorOnˢ5 = (@string)"unexpected error on ReadRune (1):"u8;
internal static readonly object unexpectedErrorOnˢ6 = (@string)"unexpected error on UnreadRune (1):"u8;
internal static readonly object expectedErrorAfterˢ = (@string)"expected error after UnreadRune (1)"u8;
internal static readonly object unexpectedErrorOnˢ7 = (@string)"unexpected error on ReadRune (2):"u8;
internal static readonly object unexpectedErrorOnRead2ˢ = (@string)"unexpected error on Read (2):"u8;
internal static readonly object expectedErrorAfterRead2ˢ = (@string)"expected error after Read (2)"u8;
internal static readonly object unexpectedErrorOnˢ8 = (@string)"unexpected error on ReadByte (2):"u8;
internal static readonly object expectedErrorAfterˢ2 = (@string)"expected error after ReadByte"u8;
internal static readonly object unexpectedErrorOnˢ9 = (@string)"unexpected error on ReadRune (3):"u8;
internal static readonly object unexpectedErrorOnˢ10 = (@string)"unexpected error on ReadByte (3):"u8;
internal static readonly object unexpectedErrorOnˢ11 = (@string)"unexpected error on UnreadByte (3):"u8;
internal static readonly object expectedErrorAfterˢ3 = (@string)"expected error after UnreadByte (3)"u8;
internal static readonly object unexpectedErrorOnˢ12 = (@string)"unexpected error on ReadRune (4):"u8;
internal static readonly object unexpectedErrorOnˢ13 = (@string)"unexpected error on ReadSlice (4):"u8;
internal static readonly object expectedErrorAfterˢ4 = (@string)"expected error after ReadSlice (4)"u8;

// Test that UnreadRune fails if the preceding operation was not a ReadRune.
public static void TestUnreadRuneError(ж<Δtesting.T> t) {
    var buf = new slice<byte>(3); // All runes in this test are 3 bytes long
    var r = NewReader(new bufio_test_package.StringReaderжReader(Ꮡ(new StringReader(data: slice<@string>(["日本語日本語日本語"])))));
    if (r.UnreadRune() == nil) {
        t.Error(expectedErrorOnˢ);
    }
    var (_, _, err) = r.ReadRune();
    if (err != nil) {
        t.Error(unexpectedErrorOnˢ5, err);
    }
    {
        err = r.UnreadRune(); if (err != nil) {
            t.Error(unexpectedErrorOnˢ6, err);
        }
    }
    if (r.UnreadRune() == nil) {
        t.Error(expectedErrorAfterˢ);
    }
    // Test error after Read.
    (_, _, err) = r.ReadRune(); // reset state
    if (err != nil) {
        t.Error(unexpectedErrorOnˢ7, err);
    }
    (_, err) = r.Read(buf);
    if (err != nil) {
        t.Error(unexpectedErrorOnRead2ˢ, err);
    }
    if (r.UnreadRune() == nil) {
        t.Error(expectedErrorAfterRead2ˢ);
    }
    // Test error after ReadByte.
    (_, _, err) = r.ReadRune(); // reset state
    if (err != nil) {
        t.Error(unexpectedErrorOnˢ7, err);
    }
    foreach (var _ᴛ1 in range(buf)) {
        (_, err) = r.ReadByte();
        if (err != nil) {
            t.Error(unexpectedErrorOnˢ8, err);
        }
    }
    if (r.UnreadRune() == nil) {
        t.Error(expectedErrorAfterˢ2);
    }
    // Test error after UnreadByte.
    (_, _, err) = r.ReadRune(); // reset state
    if (err != nil) {
        t.Error(unexpectedErrorOnˢ9, err);
    }
    (_, err) = r.ReadByte();
    if (err != nil) {
        t.Error(unexpectedErrorOnˢ10, err);
    }
    err = r.UnreadByte();
    if (err != nil) {
        t.Error(unexpectedErrorOnˢ11, err);
    }
    if (r.UnreadRune() == nil) {
        t.Error(expectedErrorAfterˢ3);
    }
    // Test error after ReadSlice.
    (_, _, err) = r.ReadRune(); // reset state
    if (err != nil) {
        t.Error(unexpectedErrorOnˢ12, err);
    }
    (_, err) = r.ReadSlice(0);
    if (!AreEqual(err, Δio.EOF)) {
        t.Error(unexpectedErrorOnˢ13, err);
    }
    if (r.UnreadRune() == nil) {
        t.Error(expectedErrorAfterˢ4);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorAtEofˢ = (@string)"expected error at EOF"u8;
internal static readonly object expectedEofGotˢ = (@string)"expected EOF; got"u8;

public static void TestUnreadRuneAtEOF(ж<Δtesting.T> t) {
    // UnreadRune/ReadRune should error at EOF (was a bug; used to panic)
    var r = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader("x")));
    r.ReadRune();
    r.ReadRune();
    r.UnreadRune();
    var (_, _, err) = r.ReadRune();
    if (err == nil) {
        t.Error(expectedErrorAtEofˢ);
    } else if (!AreEqual(err, Δio.EOF)) {
        t.Error(expectedEofGotˢ, err);
    }
}

public static void TestReadWriteRune(ж<Δtesting.T> t) {
    const rune NRune = 1000;
    var byteBuf = @new<bytes.Buffer>();
    var w = NewWriter(new bufio_test_package.bytes_BufferжWriter(byteBuf));
    // Write the runes out using WriteRune
    var buf = new slice<byte>(utf8.UTFMax);
    for (var rΔ1 = (rune)0; rΔ1 < NRune; rΔ1++) {
        nint size = utf8.EncodeRune(buf, rΔ1);
        var (nbytes, err) = w.WriteRune(rΔ1);
        if (err != nil) {
            t.Fatalf("WriteRune(0x%x) error: %s", rΔ1, err);
        }
        if (nbytes != size) {
            t.Fatalf("WriteRune(0x%x) expected %d, got %d", rΔ1, size, nbytes);
        }
    }
    w.Flush();

    var r = NewReader(new bufio_test_package.bytes_BufferжReader(byteBuf));
    // Read them back with ReadRune
    for (var r1 = (rune)0; r1 < NRune; r1++) {
        nint size = utf8.EncodeRune(buf, r1);
        var (nr, nbytes, err) = r.ReadRune();
        if (nr != r1 || nbytes != size || err != nil) {
            t.Fatalf("ReadRune(0x%x) got 0x%x,%d not 0x%x,%d (err=%s)", r1, nr, nbytes, r1, size, err);
        }
    }
}

public static void TestWriteInvalidRune(ж<Δtesting.T> t) {
    // Invalid runes, including negative ones, should be written as the
    // replacement character.
    foreach (var (_, r) in slice<rune>([-1, utf8.MaxRune + 1])) {
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        var w = NewWriter(new bufio_test_package.strings_BuilderжWriter(Ꮡbuf));
        w.WriteRune(r);
        w.Flush();
        {
            @string s = buf.String(); if (s != "\uFFFD"u8) {
                t.Errorf("WriteRune(%d) wrote %q, not replacement character", r, s);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testAllocatesMoreWithˢ = (@string)"test allocates more with -asan; see #70079"u8;
internal static ref readonly @string fooFoo424242424242424242ˢ => ref fooFoo424242424242424242ˢᵇ;
private static readonly @string fooFoo424242424242424242ˢᵇ = "       foo       foo        42        42        42        42        42        42        42        42       4.2       4.2       4.2       4.2\n"u8;

public static void TestReadStringAllocs(ж<Δtesting.T> t) {
    if (asan.Enabled) {
        t.Skip(testAllocatesMoreWithˢ);
    }
    var r = strings.NewReader(fooFoo424242424242424242ˢ);
    var buf = NewReader(new bufio_test_package.strings_ReaderжReader(r));
    var bufʗ1 = buf;
    var rʗ1 = r;
    var allocs = Δtesting.AllocsPerRun(100, () => {
        rʗ1.Seek(0, Δio.SeekStart);
        bufʗ1.Reset(new bufio_test_package.strings_ReaderжReader(rʗ1));

        var (_, err) = bufʗ1.ReadString((byte)'\n');
        if (err != nil) {
            t.Fatal(err);
        }
    });
    if (allocs != 1D) {
        t.Errorf("Unexpected number of allocations, got %f, want 1", allocs);
    }
}

public static void TestWriter(ж<Δtesting.T> t) {
    array<byte> data = new(8192);

    for (nint i = 0; i < len(data); i++) {
        data[i] = (byte)(' ' + i % ((rune)'~' - (rune)' '));
    }
    var w = @new<bytes.Buffer>();
    for (nint i = 0; i < len(bufsizes); i++) {
        for (nint j = 0; j < len(bufsizes); j++) {
            nint nwrite = bufsizes[i];
            nint bs = bufsizes[j];

            // Write nwrite bytes using buffer size bs.
            // Check that the right amount makes it out
            // and that the data is correct.

            w.Reset();
            var buf = NewWriterSize(new bufio_test_package.bytes_BufferжWriter(w), bs);
            @string context = fmt.Sprintf("nwrite=%d bufsize=%d"u8, nwrite, bs);
            var (n, e1) = buf.Write(data.slice(0, nwrite));
            if (e1 != nil || n != nwrite) {
                t.Errorf("%s: buf.Write %d = %d, %v", context, nwrite, n, e1);
                continue;
            }
            {
                var e = buf.Flush(); if (e != nil) {
                    t.Errorf("%s: buf.Flush = %v", context, e);
                }
            }

            var written = w.Bytes();
            if (len(written) != nwrite) {
                t.Errorf("%s: %d bytes written", context, len(written));
            }
            for (nint l = 0; l < len(written); l++) {
                if (written[l] != data[l]) {
                    t.Errorf("wrong bytes written");
                    t.Errorf("want=%q", data.slice(0, len(written)));
                    t.Errorf("have=%q", written);
                }
            }
        }
    }
}

public static void TestWriterAppend(ж<Δtesting.T> t) {
    var got = @new<bytes.Buffer>();
    slice<byte> want = default;
    var rn = rand.New(rand.NewSource(0));
    var w = NewWriterSize(new bufio_test_package.bytes_BufferжWriter(got), 64);
    for (nint i = 0; i < 100; i++) {
        // Obtain a buffer to append to.
        var b = w.AvailableBuffer();
        if (w.Available() != cap(b)) {
            t.Fatalf("Available() = %v, want %v", w.Available(), cap(b));
        }

        // While not recommended, it is valid to append to a shifted buffer.
        // This forces Write to copy the input.
        if (rn.Intn(8) == 0 && cap(b) > 0) {
            b = b.slice(1, 1, cap(b));
        }

        // Append a random integer of varying width.
        var n = (int64)rn.Intn(((nint)1).Lsh((int64)(rn.Intn(30))));
        want = append(strconv.AppendInt(want, n, 10), (byte)' ');
        b = append(strconv.AppendInt(b, n, 10), (byte)' ');
        w.Write(b);
    }
    w.Flush();

    if (!bytes.Equal(got.Bytes(), want)) {
        t.Errorf("output mismatch:\ngot  %s\nwant %s", got.Bytes(), want);
    }
}

// Check that write errors are returned properly.
partial struct errorWriterTest {
    internal nint n, m;
    internal error err;
    internal error expect;
}

internal static (nint, error) Write(this errorWriterTest w, slice<byte> p) {
    return (quo(len(p) * w.n, w.m), w.err);
}

internal static slice<errorWriterTest> errorWriterTests = slice<errorWriterTest>([
    new(0, 1, default, Δio.ErrShortWrite),
    new(1, 2, default, Δio.ErrShortWrite),
    new(1, 1, default, default),
    new(0, 1, Δio.ErrClosedPipe, Δio.ErrClosedPipe),
    new(1, 2, Δio.ErrClosedPipe, Δio.ErrClosedPipe),
    new(1, 1, Δio.ErrClosedPipe, Δio.ErrClosedPipe)
]);

public static void TestWriteErrors(ж<Δtesting.T> t) {
    foreach (var (_, w) in errorWriterTests) {
        var buf = NewWriter(w);
        var (_, e) = buf.Write(slice<byte>("hello world"u8));
        if (e != nil) {
            t.Errorf("Write hello to %v: %v", w, e);
            continue;
        }
        // Two flushes, to verify the error is sticky.
        for (nint i = 0; i < 2; i++) {
            e = buf.Flush();
            if (!AreEqual(e, w.expect)) {
                t.Errorf("Flush %d/2 %v: got %v, wanted %v", i + 1, w, e, w.expect);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object newReaderSizeDidNotˢ = (@string)"NewReaderSize did not detect underlying Reader"u8;
internal static readonly object newReaderSizeDidNotˢ2 = (@string)"NewReaderSize did not enlarge buffer"u8;

public static void TestNewReaderSizeIdempotent(ж<Δtesting.T> t) {
    UntypedInt BufSize = 1000;
    var b = NewReaderSize(new bufio_test_package.strings_ReaderжReader(strings.NewReader(helloWorldˢ)), BufSize);
    // Does it recognize itself?
    var b1 = NewReaderSize(new bufio_test_package.bufio_ReaderжReader(b), BufSize);
    if (b1 != b) {
        t.Error(newReaderSizeDidNotˢ);
    }
    // Does it wrap if existing buffer is too small?
    var b2 = NewReaderSize(new bufio_test_package.bufio_ReaderжReader(b), 2 * BufSize);
    if (b2 == b) {
        t.Error(newReaderSizeDidNotˢ2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object newWriterSizeDidNotˢ = (@string)"NewWriterSize did not detect underlying Writer"u8;
internal static readonly object newWriterSizeDidNotˢ2 = (@string)"NewWriterSize did not enlarge buffer"u8;

public static void TestNewWriterSizeIdempotent(ж<Δtesting.T> t) {
    UntypedInt BufSize = 1000;
    var b = NewWriterSize(new bufio_test_package.bytes_BufferжWriter(@new<bytes.Buffer>()), BufSize);
    // Does it recognize itself?
    var b1 = NewWriterSize(new bufio_test_package.bufio_WriterжWriter(b), BufSize);
    if (b1 != b) {
        t.Error(newWriterSizeDidNotˢ);
    }
    // Does it wrap if existing buffer is too small?
    var b2 = NewWriterSize(new bufio_test_package.bufio_WriterжWriter(b), 2 * BufSize);
    if (b2 == b) {
        t.Error(newWriterSizeDidNotˢ2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object writeStringˢ = (@string)"WriteString"u8;

public static void TestWriteString(ж<Δtesting.T> t) {
    const nint BufSize = 8;
    var buf = @new<strings.Builder>();
    var b = NewWriterSize(new bufio_test_package.strings_BuilderжWriter(buf), BufSize);
    b.WriteString("0");                         // easy
    b.WriteString("123456");                    // still easy
    b.WriteString("7890");                      // easy after flush
    b.WriteString("abcdefghijklmnopqrstuvwxy"); // hard
    b.WriteString("z");
    {
        var err = b.Flush(); if (err != nil) {
            t.Error(writeStringˢ, err);
        }
    }
    @string s = "01234567890abcdefghijklmnopqrstuvwxyz";
    if (buf.String() != s) {
        t.Errorf("WriteString wants %q gets %q", s, buf.String());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string abcˢ => ref abcˢᵇ;
private static readonly @string abcˢᵇ = "abc"u8;
internal static ref readonly @string abc12345ˢ => ref abc12345ˢᵇ;
private static readonly @string abc12345ˢᵇ = "abc12345"u8;

public static void TestWriteStringStringWriter(ж<Δtesting.T> t) {
    const nint BufSize = 8;
    {
        var tw = Ꮡ(new teststringwriter());
        var b = NewWriterSize(new bufio_test_package.teststringwriterжWriter(tw), BufSize);
        b.WriteString("1234");
        tw.check(t, ""u8, ""u8);
        b.WriteString("56789012");     // longer than BufSize
        tw.check(t, "12345678", ""u8); // but not enough (after filling the partially-filled buffer)
        b.Flush();
        tw.check(t, "123456789012", ""u8);
    }
    {
        var tw = Ꮡ(new teststringwriter());
        var b = NewWriterSize(new bufio_test_package.teststringwriterжWriter(tw), BufSize);
        b.WriteString("123456789");     // long string, empty buffer:
        tw.check(t, ""u8, "123456789"); // use WriteString
    }
    {
        var tw = Ꮡ(new teststringwriter());
        var b = NewWriterSize(new bufio_test_package.teststringwriterжWriter(tw), BufSize);
        b.WriteString(abcˢ);
        tw.check(t, ""u8, ""u8);
        b.WriteString("123456789012345");     // long string, non-empty buffer
        tw.check(t, abc12345ˢ, "6789012345"); // use Write and then WriteString since the remaining part is still longer than BufSize
    }
    {
        var tw = Ꮡ(new teststringwriter());
        var b = NewWriterSize(new bufio_test_package.teststringwriterжWriter(tw), BufSize);
        b.Write(slice<byte>("abc"u8)); // same as above, but use Write instead of WriteString
        tw.check(t, ""u8, ""u8);
        b.WriteString("123456789012345");
        tw.check(t, abc12345ˢ, "6789012345"); // same as above
    }
}

partial struct teststringwriter {
    internal @string write;
    internal @string writeString;
}

internal static (nint, error) Write(this ref teststringwriter w, slice<byte> b) {
    w.write += ((@string)b);
    return (len(b), default);
}

internal static (nint, error) WriteString(this ref teststringwriter w, @string s) {
    w.writeString += s;
    return (len(s), default);
}

internal static void check(this ref teststringwriter w, ж<Δtesting.T> t, @string write, @string writeString) {
    t.Helper();
    if (w.write != write) {
        t.Errorf("write: expected %q, got %q", write, w.write);
    }
    if (w.writeString != writeString) {
        t.Errorf("writeString: expected %q, got %q", writeString, w.writeString);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string longStringᶜ => ref longStringᶜᵇ;
private static readonly @string longStringᶜᵇ = "And now, hello, world! It is the time for all good men to come to the aid of their party"u8;

public static void TestBufferFull(ж<Δtesting.T> t) {
    @string longString = longStringᶜ;
    var buf = NewReaderSize(new bufio_test_package.strings_ReaderжReader(strings.NewReader(longString)), minReadBufferSize);
    var (line, err) = buf.ReadSlice((byte)'!');
    if (((sstring)line) != "And now, hello, "u8 || !AreEqual(err, ErrBufferFull)) {
        t.Errorf("first ReadSlice(,) = %q, %v", line, err);
    }
    (line, err) = buf.ReadSlice((byte)'!');
    if (((sstring)line) != "world!"u8 || err != nil) {
        t.Errorf("second ReadSlice(,) = %q, %v", line, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string abcdefghijklmnopˢ => ref abcdefghijklmnopˢᵇ;
private static readonly @string abcdefghijklmnopˢᵇ = "abcdefghijklmnop"u8;
internal static ref readonly @string defˢ => ref defˢᵇ;
private static readonly @string defˢᵇ = "def"u8;
internal static readonly object ghijˢ = (@string)"ghij"u8;
internal static readonly object ghijklmnopˢ = (@string)"ghijklmnop"u8;

public static void TestPeek(ж<Δtesting.T> t) {
    var p = new slice<byte>(10);
    // string is 16 (minReadBufferSize) long.
    var buf = NewReaderSize(new bufio_test_package.strings_ReaderжReader(strings.NewReader(abcdefghijklmnopˢ)), minReadBufferSize);
    {
        var (s, err) = buf.Peek(1); if (((sstring)s) != "a"u8 || err != nil) {
            t.Fatalf("want %q got %q, err=%v", (@string)"a", (@string)s, err);
        }
    }
    {
        var (s, err) = buf.Peek(4); if (((sstring)s) != "abcd"u8 || err != nil) {
            t.Fatalf("want %q got %q, err=%v", abcdˢ, (@string)s, err);
        }
    }
    {
        var (_, err) = buf.Peek(-1); if (!AreEqual(err, ErrNegativeCount)) {
            t.Fatalf("want ErrNegativeCount got %v", err);
        }
    }
    {
        var (s, err) = buf.Peek(32); if (((sstring)s) != "abcdefghijklmnop"u8 || !AreEqual(err, ErrBufferFull)) {
            t.Fatalf("want %q, ErrBufFull got %q, err=%v", abcdefghijklmnopˢ, (@string)s, err);
        }
    }
    {
        var (_, err) = buf.Read(p[0..3]); if (((sstring)(p[0..3])) != "abc"u8 || err != nil) {
            t.Fatalf("want %q got %q, err=%v", abcˢ, (@string)(p[0..3]), err);
        }
    }
    {
        var (s, err) = buf.Peek(1); if (((sstring)s) != "d"u8 || err != nil) {
            t.Fatalf("want %q got %q, err=%v", (@string)"d", (@string)s, err);
        }
    }
    {
        var (s, err) = buf.Peek(2); if (((sstring)s) != "de"u8 || err != nil) {
            t.Fatalf("want %q got %q, err=%v", (@string)"de", (@string)s, err);
        }
    }
    {
        var (_, err) = buf.Read(p[0..3]); if (((sstring)(p[0..3])) != "def"u8 || err != nil) {
            t.Fatalf("want %q got %q, err=%v", defˢ, (@string)(p[0..3]), err);
        }
    }
    {
        var (s, err) = buf.Peek(4); if (((sstring)s) != "ghij"u8 || err != nil) {
            t.Fatalf("want %q got %q, err=%v", ghijˢ, (@string)s, err);
        }
    }
    {
        var (_, err) = buf.Read(p[0..]); if (((sstring)(p[0..])) != "ghijklmnop"u8 || err != nil) {
            t.Fatalf("want %q got %q, err=%v", ghijklmnopˢ, (@string)(p[0..(int)(minReadBufferSize)]), err);
        }
    }
    {
        var (s, err) = buf.Peek(0); if (((sstring)s) != ""u8 || err != nil) {
            t.Fatalf("want %q got %q, err=%v", (@string)""u8, (@string)s, err);
        }
    }
    {
        var (_, err) = buf.Peek(1); if (!AreEqual(err, Δio.EOF)) {
            t.Fatalf("want EOF got %v", err);
        }
    }

    // Test for issue 3022, not exposing a reader's error on a successful Peek.
    buf = NewReaderSize((dataAndEOFReader)(@string)abcdˢ, 32);
    {
        var (s, err) = buf.Peek(2); if (((sstring)s) != "ab"u8 || err != nil) {
            t.Errorf(@"Peek(2) on ""abcd"", EOF = %q, %v; want ""ab"", nil", (@string)s, err);
        }
    }
    {
        var (s, err) = buf.Peek(4); if (((sstring)s) != "abcd"u8 || err != nil) {
            t.Errorf(@"Peek(4) on ""abcd"", EOF = %q, %v; want ""abcd"", nil", (@string)s, err);
        }
    }
    {
        var (n, err) = buf.Read(p[0..5]); if (((sstring)(p.slice(0, n))) != "abcd"u8 || err != nil) {
            t.Fatalf("Read after peek = %q, %v; want abcd, EOF", p.slice(0, n), err);
        }
    }
    {
        var (n, err) = buf.Read(p[0..1]); if (((sstring)(p.slice(0, n))) != ""u8 || !AreEqual(err, Δio.EOF)) {
            t.Fatalf(@"second Read after peek = %q, %v; want """", EOF", p.slice(0, n), err);
        }
    }
}

partial struct dataAndEOFReader /*@string*/;

internal static (nint, error) Read(this dataAndEOFReader r, slice<byte> p) {
    return (copy(p, r), Δio.EOF);
}

public static void TestPeekThenUnreadRune(ж<Δtesting.T> t) {
    // This sequence used to cause a crash.
    var r = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader("x")));
    r.ReadRune();
    r.Peek(1);
    r.UnreadRune();
    r.ReadRune(); // Used to panic here
}

internal static slice<byte> testOutput = slice<byte>("0123456789abcdefghijklmnopqrstuvwxy"u8);

internal static slice<byte> testInput = slice<byte>("012\n345\n678\n9ab\ncde\nfgh\nijk\nlmn\nopq\nrst\nuvw\nxy"u8);

internal static slice<byte> testInputrn = slice<byte>("012\r\n345\r\n678\r\n9ab\r\ncde\r\nfgh\r\nijk\r\nlmn\r\nopq\r\nrst\r\nuvw\r\nxy\r\n\n\r\n"u8);

// TestReader wraps a []byte and returns reads of a specific length.
partial struct testReader {
    internal slice<byte> data;
    internal nint stride;
}

internal static (nint n, error err) Read(this ref testReader t, slice<byte> buf) {
    nint n = default;
    error err = default;

    n = t.stride;
    if (n > len(t.data)) {
        n = len(t.data);
    }
    if (n > len(buf)) {
        n = len(buf);
    }
    copy(buf, t.data);
    t.data = t.data.slice(n);
    if (len(t.data) == 0) {
        err = Δio.EOF;
    }
    return (n, err);
}

internal static void testReadLine(ж<Δtesting.T> t, slice<byte> input) {
    for (nint stride = 1; stride < 2; stride++) {
        nint done = 0;
        ref var reader = ref heap<testReader>(new testReader(input, stride), out var Ꮡreader);
        var l = NewReaderSize(new bufio_test_package.testReaderжReader(Ꮡreader), len(input) + 1);
        for (;;) {
            var (line, isPrefix, err) = l.ReadLine();
            if (len(line) > 0 && err != nil) {
                t.Errorf("ReadLine returned both data and error: %s", err);
            }
            if (isPrefix) {
                t.Errorf("ReadLine returned prefix");
            }
            if (err != nil) {
                if (!AreEqual(err, Δio.EOF)) {
                    t.Fatalf("Got unknown error: %s", err);
                }
                break;
            }
            {
                var want = testOutput.slice(done, done + len(line)); if (!bytes.Equal(want, line)) {
                    t.Errorf("Bad line at stride %d: want: %x got: %x", stride, want, line);
                }
            }
            done += len(line);
        }
        if (done != len(testOutput)) {
            t.Errorf("ReadLine didn't return everything: got: %d, want: %d (stride: %d)", done, len(testOutput), stride);
        }
    }
}

public static void TestReadLine(ж<Δtesting.T> t) {
    testReadLine(t, testInput);
    testReadLine(t, testInputrn);
}

public static void TestLineTooLong(ж<Δtesting.T> t) {
    var data = new slice<byte>(0);
    for (nint i = 0; i < (nint)(minReadBufferSize * 5 / 2); i++) {
        data = append(data, (byte)('0' + (byte)(i % 10)));
    }
    var buf = bytes.NewReader(data);
    var l = NewReaderSize(new bufio_test_package.bytes_ReaderжReader(buf), minReadBufferSize);
    var (line, isPrefix, err) = l.ReadLine();
    if (!isPrefix || !bytes.Equal(line, data[..(int)(minReadBufferSize)]) || err != nil) {
        t.Errorf("bad result for first line: got %q want %q %v", line, data[..(int)(minReadBufferSize)], err);
    }
    data = data.slice(len(line));
    (line, isPrefix, err) = l.ReadLine();
    if (!isPrefix || !bytes.Equal(line, data[..(int)(minReadBufferSize)]) || err != nil) {
        t.Errorf("bad result for second line: got %q want %q %v", line, data[..(int)(minReadBufferSize)], err);
    }
    data = data.slice(len(line));
    (line, isPrefix, err) = l.ReadLine();
    if (isPrefix || !bytes.Equal(line, data[..(int)(minReadBufferSize / 2)]) || err != nil) {
        t.Errorf("bad result for third line: got %q want %q %v", line, data[..(int)(minReadBufferSize / 2)], err);
    }
    (line, isPrefix, err) = l.ReadLine();
    if (isPrefix || err == nil) {
        t.Errorf("expected no more lines: %x %s", line, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string thisIsLine1ˢ => ref thisIsLine1ˢᵇ;
private static readonly @string thisIsLine1ˢᵇ = "this is line1"u8;
internal static ref readonly @string thisIsLine2ThisIsLine3ˢ => ref thisIsLine2ThisIsLine3ˢᵇ;
private static readonly @string thisIsLine2ThisIsLine3ˢᵇ = "this is line2\nthis is line 3\n"u8;

public static void TestReadAfterLines(ж<Δtesting.T> t) {
    @string line1 = thisIsLine1ˢ;
    @string restData = thisIsLine2ThisIsLine3ˢ;
    var inbuf = bytes.NewReader(slice<byte>(line1 + "\n" + restData));
    var outbuf = @new<strings.Builder>();
    nint maxLineLength = len(line1) + len(restData) / 2;
    var l = NewReaderSize(new bufio_test_package.bytes_ReaderжReader(inbuf), maxLineLength);
    var (line, isPrefix, err) = l.ReadLine();
    if (isPrefix || err != nil || ((sstring)line) != line1) {
        t.Errorf("bad result for first line: isPrefix=%v err=%v line=%q", isPrefix, err, (@string)line);
    }
    (var n, err) = Δio.Copy(new bufio_test_package.strings_BuilderжWriter(outbuf), new bufio_test_package.bufio_ReaderжReader(l));
    if ((nint)n != len(restData) || err != nil) {
        t.Errorf("bad result for Read: n=%d err=%v", n, err);
    }
    if (outbuf.String() != restData) {
        t.Errorf("bad result for Read: got %q; expected %q", outbuf.String(), restData);
    }
}

public static void TestReadEmptyBuffer(ж<Δtesting.T> t) {
    var l = NewReaderSize(new bufio_test_package.bytes_BufferжReader(@new<bytes.Buffer>()), minReadBufferSize);
    var (line, isPrefix, err) = l.ReadLine();
    if (!AreEqual(err, Δio.EOF)) {
        t.Errorf("expected EOF from ReadLine, got '%s' %t %s", line, isPrefix, err);
    }
}

public static void TestLinesAfterRead(ж<Δtesting.T> t) {
    var l = NewReaderSize(new bufio_test_package.bytes_ReaderжReader(bytes.NewReader(slice<byte>("foo"u8))), minReadBufferSize);
    var (_, err) = Δio.ReadAll(new bufio_test_package.bufio_ReaderжReader(l));
    if (err != nil) {
        t.Error(err);
        return;
    }

    (var line, var isPrefix, err) = l.ReadLine();
    if (!AreEqual(err, Δio.EOF)) {
        t.Errorf("expected EOF from ReadLine, got '%s' %t %s", line, isPrefix, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string line1ˢ => ref line1ˢᵇ;
private static readonly @string line1ˢᵇ = "line 1\n"u8;

public static void TestReadLineNonNilLineOrError(ж<Δtesting.T> t) {
    var r = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(line1ˢ)));
    for (nint i = 0; i < 2; i++) {
        var (l, _, err) = r.ReadLine();
        if (l != nil && err != nil) {
            t.Fatalf("on line %d/2; ReadLine=%#v, %v; want non-nil line or Error, but not both",
                i + 1, l, err);
        }
    }
}

partial struct readLineResult {
    internal slice<byte> line;
    internal bool isPrefix;
    internal error err;
}

partial struct readLineNewlinesTestsᴛ1 /*dyn*/ {
    internal @string input;
    internal slice<readLineResult> expect;
}
internal static slice<readLineNewlinesTestsᴛ1> readLineNewlinesTests = slice<readLineNewlinesTestsᴛ1>([
    new("012345678901234\r\n012345678901234\r\n", slice<readLineResult>([
        new(slice<byte>("012345678901234"u8), true, default),
        new(default, false, default),
        new(slice<byte>("012345678901234"u8), true, default),
        new(default, false, default),
        new(default, false, Δio.EOF)
    ])),
    new("0123456789012345\r012345678901234\r", slice<readLineResult>([
        new(slice<byte>("0123456789012345"u8), true, default),
        new(slice<byte>("\r012345678901234"u8), true, default),
        new(slice<byte>("\r"u8), false, default),
        new(default, false, Δio.EOF)
    ]))
]);

public static void TestReadLineNewlines(ж<Δtesting.T> t) {
    foreach (var (_, e) in readLineNewlinesTests) {
        testReadLineNewlines(t, e.input, e.expect);
    }
}

internal static void testReadLineNewlines(ж<Δtesting.T> t, @string input, slice<readLineResult> expect) {
    var b = NewReaderSize(new bufio_test_package.strings_ReaderжReader(strings.NewReader(input)), minReadBufferSize);
    foreach (var (i, e) in expect) {
        var (line, isPrefix, err) = b.ReadLine();
        if (!bytes.Equal(line, e.line)) {
            t.Errorf("%q call %d, line == %q, want %q", input, i, line, e.line);
            return;
        }
        if (isPrefix != e.isPrefix) {
            t.Errorf("%q call %d, isPrefix == %v, want %v", input, i, isPrefix, e.isPrefix);
            return;
        }
        if (!AreEqual(err, e.err)) {
            t.Errorf("%q call %d, err == %v, want %v", input, i, err, e.err);
            return;
        }
    }
}

internal static slice<byte> createTestInput(nint n) {
    var input = new slice<byte>(n);
    foreach (var i in range(input)) {
        // 101 and 251 are arbitrary prime numbers.
        // The idea is to create an input sequence
        // which doesn't repeat too frequently.
        input[i] = (byte)(i % 251);
        if (i % 101 == 0) {
            input[i] ^= (byte)((byte)(i / 101));
        }
    }
    return input;
}

public static void TestReaderWriteTo(ж<Δtesting.T> t) {
    var input = createTestInput(8192);
    var r = NewReader(new onlyReader(new bufio_test_package.bytes_ReaderжReader(bytes.NewReader(input))));
    var w = @new<bytes.Buffer>();
    {
        var (n, err) = r.WriteTo(new bufio_test_package.bytes_BufferжWriter(w)); if (err != nil || n != (int64)len(input)) {
            t.Fatalf("r.WriteTo(w) = %d, %v, want %d, nil", n, err, len(input));
        }
    }

    foreach (var (i, val) in w.Bytes()) {
        if (val != input[i]) {
            t.Errorf("after write: out[%d] = %#x, want %#x", i, val, input[i]);
        }
    }
}

partial struct errorWriterToTest {
    internal nint rn, wn;
    internal error rerr, werr;
    internal error expected;
}

internal static (nint, error) Read(this errorWriterToTest r, slice<byte> p) {
    return (len(p) * r.rn, r.rerr);
}

internal static (nint, error) Write(this errorWriterToTest w, slice<byte> p) {
    return (len(p) * w.wn, w.werr);
}

internal static slice<errorWriterToTest> errorWriterToTests = slice<errorWriterToTest>([
    new(1, 0, default, Δio.ErrClosedPipe, Δio.ErrClosedPipe),
    new(0, 1, Δio.ErrClosedPipe, default, Δio.ErrClosedPipe),
    new(0, 0, Δio.ErrUnexpectedEOF, Δio.ErrClosedPipe, Δio.ErrUnexpectedEOF),
    new(0, 1, Δio.EOF, default, default)
]);

public static void TestReaderWriteToErrors(ж<Δtesting.T> t) {
    foreach (var (i, rw) in errorWriterToTests) {
        var r = NewReader(rw);
        {
            var (_, err) = r.WriteTo(rw); if (!AreEqual(err, rw.expected)) {
                t.Errorf("r.WriteTo(errorWriterToTests[%d]) = _, %v, want _,%v", i, err, rw.expected);
            }
        }
    }
}

public static void TestWriterReadFrom(ж<Δtesting.T> t) {
    var ws = slice<Func<Δio.Writer, Δio.Writer>>([
        (Δio.Writer w) => new onlyWriter(w),
        (Δio.Writer w) => w
    ]);

    var rs = slice<Func<Δio.Reader, Δio.Reader>>([
        iotest.DataErrReader,
        (Δio.Reader r) => r
    ]);

    foreach (var (ri, rfunc) in rs) {
        foreach (var (wi, wfunc) in ws) {
            var input = createTestInput(8192);
            var b = @new<strings.Builder>();
            var w = NewWriter(wfunc(new bufio_test_package.strings_BuilderжWriter(b)));
            var r = rfunc(new bufio_test_package.bytes_ReaderжReader(bytes.NewReader(input)));
            {
                var (n, err) = w.ReadFrom(r); if (err != nil || n != (int64)len(input)) {
                    t.Errorf("ws[%d],rs[%d]: w.ReadFrom(r) = %d, %v, want %d, nil", wi, ri, n, err, len(input));
                    continue;
                }
            }
            {
                var err = w.Flush(); if (err != nil) {
                    t.Errorf("Flush returned %v", err);
                    continue;
                }
            }
            {
                @string got = b.String();
                @string want = (@string)input; if (got != want) {
                    t.Errorf("ws[%d], rs[%d]:\ngot  %q\nwant %q\n", wi, ri, got, want);
                }
            }
        }
    }
}

partial struct errorReaderFromTest {
    internal nint rn, wn;
    internal error rerr, werr;
    internal error expected;
}

internal static (nint, error) Read(this errorReaderFromTest r, slice<byte> p) {
    return (len(p) * r.rn, r.rerr);
}

internal static (nint, error) Write(this errorReaderFromTest w, slice<byte> p) {
    return (len(p) * w.wn, w.werr);
}

internal static slice<errorReaderFromTest> errorReaderFromTests = slice<errorReaderFromTest>([
    new(0, 1, Δio.EOF, default, default),
    new(1, 1, Δio.EOF, default, default),
    new(0, 1, Δio.ErrClosedPipe, default, Δio.ErrClosedPipe),
    new(0, 0, Δio.ErrClosedPipe, Δio.ErrShortWrite, Δio.ErrClosedPipe),
    new(1, 0, default, Δio.ErrShortWrite, Δio.ErrShortWrite)
]);

public static void TestWriterReadFromErrors(ж<Δtesting.T> t) {
    foreach (var (i, rw) in errorReaderFromTests) {
        var w = NewWriter(rw);
        {
            var (_, err) = w.ReadFrom(rw); if (!AreEqual(err, rw.expected)) {
                t.Errorf("w.ReadFrom(errorReaderFromTests[%d]) = _, %v, want _,%v", i, err, rw.expected);
            }
        }
    }
}

// TestWriterReadFromCounts tests that using io.Copy to copy into a
// bufio.Writer does not prematurely flush the buffer. For example, when
// buffering writes to a network socket, excessive network writes should be
// avoided.
public static void TestWriterReadFromCounts(ж<Δtesting.T> t) {
    ref var w0 = ref heap(new writeCountingDiscard(), out var Ꮡw0);
    var b0 = NewWriterSize(new bufio_test_package.writeCountingDiscardжWriter(Ꮡw0), 1234);
    b0.WriteString(strings.Repeat("x", 1000));
    if (w0 != 0) {
        t.Fatalf("write 1000 'x's: got %d writes, want 0", w0);
    }
    b0.WriteString(strings.Repeat("x", 200));
    if (w0 != 0) {
        t.Fatalf("write 1200 'x's: got %d writes, want 0", w0);
    }
    Δio.Copy(new bufio_test_package.bufio_WriterжWriter(b0), new onlyReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(strings.Repeat("x", 30)))));
    if (w0 != 0) {
        t.Fatalf("write 1230 'x's: got %d writes, want 0", w0);
    }
    Δio.Copy(new bufio_test_package.bufio_WriterжWriter(b0), new onlyReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(strings.Repeat("x", 9)))));
    if (w0 != 1) {
        t.Fatalf("write 1239 'x's: got %d writes, want 1", w0);
    }

    ref var w1 = ref heap(new writeCountingDiscard(), out var Ꮡw1);
    var b1 = NewWriterSize(new bufio_test_package.writeCountingDiscardжWriter(Ꮡw1), 1234);
    b1.WriteString(strings.Repeat("x", 1200));
    b1.Flush();
    if (w1 != 1) {
        t.Fatalf("flush 1200 'x's: got %d writes, want 1", w1);
    }
    b1.WriteString(strings.Repeat("x", 89));
    if (w1 != 1) {
        t.Fatalf("write 1200 + 89 'x's: got %d writes, want 1", w1);
    }
    Δio.Copy(new bufio_test_package.bufio_WriterжWriter(b1), new onlyReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(strings.Repeat("x", 700)))));
    if (w1 != 1) {
        t.Fatalf("write 1200 + 789 'x's: got %d writes, want 1", w1);
    }
    Δio.Copy(new bufio_test_package.bufio_WriterжWriter(b1), new onlyReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(strings.Repeat("x", 600)))));
    if (w1 != 2) {
        t.Fatalf("write 1200 + 1389 'x's: got %d writes, want 2", w1);
    }
    b1.Flush();
    if (w1 != 3) {
        t.Fatalf("flush 1200 + 1389 'x's: got %d writes, want 3", w1);
    }
}

// A writeCountingDiscard is like io.Discard and counts the number of times
// Write is called on it.
partial struct writeCountingDiscard /*num:nint*/;

internal static (nint, error) Write(this ref writeCountingDiscard w, slice<byte> p) {
    w++;
    return (len(p), default);
}

partial struct negativeReader /*num:nint*/;

internal static (nint, error) Read(this ref negativeReader r, slice<byte> _) {
    return (-1, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readDidNotPanicˢ = (@string)"read did not panic"u8;
internal static ref readonly @string readerReturnedNegativeˢ => ref readerReturnedNegativeˢᵇ;
private static readonly @string readerReturnedNegativeˢᵇ = "reader returned negative count from Read"u8;

public static void TestNegativeRead(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        // should panic with a description pointing at the reader, not at itself.
        // (should NOT panic with slice index error, for example.)
        var b = NewReader(new bufio_test_package.negativeReaderжReader(@new<negativeReader>()));
        defer(() => {
            var switchᴛ1 = recover();
            switch (switchᴛ1.type()) {
            case null: {
                t.Fatal(readDidNotPanicˢ);
                break;
            }
            case {} Δerr when Δerr._<error>(out var err): {
                if (!strings.Contains(err.Error(), readerReturnedNegativeˢ)) {
                    t.Fatalf("wrong panic: %v", err);
                }
                break;
            }
            default: {
                var err = switchᴛ1;
                t.Fatalf("unexpected panic value: %T(%v)", err, err);
                break;
            }}
        }, ref ᒐ);
        b.Read(new slice<byte>(100));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static error errFake = errors.New("fake error");

partial struct errorThenGoodReader {
    internal bool didErr;
    internal nint nread;
}

internal static (nint, error) Read(this ref errorThenGoodReader r, slice<byte> p) {
    r.nread++;
    if (!r.didErr) {
        r.didErr = true;
        return (0, errFake);
    }
    return (len(p), default);
}

public static void TestReaderClearError(ж<Δtesting.T> t) {
    var r = Ꮡ(new errorThenGoodReader());
    var b = NewReader(new bufio_test_package.errorThenGoodReaderжReader(r));
    var buf = new slice<byte>(1);
    {
        var (_, err) = b.Read(default); if (err != nil) {
            t.Fatalf("1st nil Read = %v; want nil", err);
        }
    }
    {
        var (_, err) = b.Read(buf); if (!AreEqual(err, errFake)) {
            t.Fatalf("1st Read = %v; want errFake", err);
        }
    }
    {
        var (_, err) = b.Read(default); if (err != nil) {
            t.Fatalf("2nd nil Read = %v; want nil", err);
        }
    }
    {
        var (_, err) = b.Read(buf); if (err != nil) {
            t.Fatalf("3rd Read with buffer = %v; want nil", err);
        }
    }
    if (r.nread != 2) {
        t.Errorf("num reads = %d; want 2", r.nread);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string abcdefˢ => ref abcdefˢᵇ;
private static readonly @string abcdefˢᵇ = "abcdef"u8;

// Test for golang.org/issue/5947
public static void TestWriterReadFromWhileFull(ж<Δtesting.T> t) {
    var buf = @new<bytes.Buffer>();
    var w = NewWriterSize(new bufio_test_package.bytes_BufferжWriter(buf), 10);

    // Fill buffer exactly.
    var (n, err) = w.Write(slice<byte>("0123456789"u8));
    if (n != 10 || err != nil) {
        t.Fatalf("Write returned (%v, %v), want (10, nil)", n, err);
    }

    // Use ReadFrom to read in some data.
    (var n2, err) = w.ReadFrom(new bufio_test_package.strings_ReaderжReader(strings.NewReader(abcdefˢ)));
    if (n2 != 6 || err != nil) {
        t.Fatalf("ReadFrom returned (%v, %v), want (6, nil)", n2, err);
    }
}

partial struct emptyThenNonEmptyReader {
    internal Δio.Reader r;
    internal nint n;
}

internal static (nint, error) Read(this ref emptyThenNonEmptyReader r, slice<byte> p) {
    if (r.n <= 0) {
        return r.r.Read(p);
    }
    r.n--;
    return (0, default);
}

// Test for golang.org/issue/7611
public static void TestWriterReadFromUntilEOF(ж<Δtesting.T> t) {
    var buf = @new<bytes.Buffer>();
    var w = NewWriterSize(new bufio_test_package.bytes_BufferжWriter(buf), 5);

    // Partially fill buffer
    var (n, err) = w.Write(slice<byte>("0123"u8));
    if (n != 4 || err != nil) {
        t.Fatalf("Write returned (%v, %v), want (4, nil)", n, err);
    }

    // Use ReadFrom to read in some data.
    var r = Ꮡ(new emptyThenNonEmptyReader(r: new bufio_test_package.strings_ReaderжReader(strings.NewReader(abcdˢ)), n: 3));
    (var n2, err) = w.ReadFrom(new bufio_test_package.emptyThenNonEmptyReaderжReader(r));
    if (n2 != 4 || err != nil) {
        t.Fatalf("ReadFrom returned (%v, %v), want (4, nil)", n2, err);
    }
    w.Flush();
    {
        @string got = buf.String();
        @string want = "0123abcd"; if (got != want) {
            t.Fatalf("buf.Bytes() returned %q, want %q", got, want);
        }
    }
}

public static void TestWriterReadFromErrNoProgress(ж<Δtesting.T> t) {
    var buf = @new<bytes.Buffer>();
    var w = NewWriterSize(new bufio_test_package.bytes_BufferжWriter(buf), 5);

    // Partially fill buffer
    var (n, err) = w.Write(slice<byte>("0123"u8));
    if (n != 4 || err != nil) {
        t.Fatalf("Write returned (%v, %v), want (4, nil)", n, err);
    }

    // Use ReadFrom to read in some data.
    var r = Ꮡ(new emptyThenNonEmptyReader(r: new bufio_test_package.strings_ReaderжReader(strings.NewReader(abcdˢ)), n: 100));
    (var n2, err) = w.ReadFrom(new bufio_test_package.emptyThenNonEmptyReaderжReader(r));
    if (n2 != 0 || !AreEqual(err, Δio.ErrNoProgress)) {
        t.Fatalf("buf.Bytes() returned (%v, %v), want (0, io.ErrNoProgress)", n2, err);
    }
}

partial struct readFromWriter {
    internal slice<byte> buf;
    internal nint writeBytes;
    internal nint readFromBytes;
}

internal static (nint, error) Write(this ref readFromWriter w, slice<byte> p) {
    w.buf = appendꓸꓸꓸ(w.buf, p);
    w.writeBytes += len(p);
    return (len(p), default);
}

internal static (int64, error) ReadFrom(this ref readFromWriter w, Δio.Reader r) {
    var (b, err) = Δio.ReadAll(r);
    w.buf = appendꓸꓸꓸ(w.buf, b);
    w.readFromBytes += len(b);
    return ((int64)len(b), err);
}

// Test that calling (*Writer).ReadFrom with a partially-filled buffer
// fills the buffer before switching over to ReadFrom.
public static void TestWriterReadFromWithBufferedData(ж<Δtesting.T> t) {
    const nint bufsize = 16;

    var input = createTestInput(64);
    var rfw = Ꮡ(new readFromWriter());
    var w = NewWriterSize(new bufio_test_package.readFromWriterжWriter(rfw), bufsize);

    const nint writeSize = 8;
    {
        var (nΔ1, errΔ1) = w.Write(input[..(int)(writeSize)]); if (nΔ1 != writeSize || errΔ1 != nil) {
            t.Errorf("w.Write(%v bytes) = %v, %v; want %v, nil", (nint)(writeSize), nΔ1, errΔ1, (nint)(writeSize));
        }
    }
    var (n, err) = w.ReadFrom(new bufio_test_package.bytes_ReaderжReader(bytes.NewReader(input[(int)(writeSize)..])));
    {
        nint wantn = len(input[(int)(writeSize)..]); if ((nint)n != wantn || err != nil) {
            t.Errorf("io.Copy(w, %v bytes) = %v, %v; want %v, nil", wantn, n, err, wantn);
        }
    }
    {
        var errΔ2 = w.Flush(); if (errΔ2 != nil) {
            t.Errorf("w.Flush() = %v, want nil", errΔ2);
        }
    }

    {
        nint got = rfw.writeBytes;
        nint want = bufsize; if (got != want) {
            t.Errorf("wrote %v bytes with Write, want %v", got, want);
        }
    }
    {
        nint got = rfw.readFromBytes;
        nint want = len(input) - bufsize; if (got != want) {
            t.Errorf("wrote %v bytes with ReadFrom, want %v", got, want);
        }
    }
}

public static void TestReadZero(ж<Δtesting.T> t) {
    foreach (var (_, size) in slice<nint>([100, 2])) {
        t.Run(fmt.Sprintf("bufsize=%d"u8, size), (ж<Δtesting.T> tΔ1) => {
            var r = Δio.MultiReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(abcˢ)), new bufio_test_package.emptyThenNonEmptyReaderжReader(Ꮡ(new emptyThenNonEmptyReader(r: new bufio_test_package.strings_ReaderжReader(strings.NewReader(defˢ)), n: 1))));
            var br = NewReaderSize(r, size);
            var brʗ1 = br;
            void want(@string s, error wantErr) {
                var p = new slice<byte>(50);
                var (n, err) = brʗ1.Read(p);
                if (!AreEqual(err, wantErr) || n != len(s) || ((sstring)(p.slice(0, n))) != s) {
                    tΔ1.Fatalf("read(%d) = %q, %v, want %q, %v", len(p), (@string)(p.slice(0, n)), err, s, wantErr);
                }
                tΔ1.Logf("read(%d) = %q, %v", len(p), (@string)(p.slice(0, n)), err);
            }
            want(abcˢ, default);
            want(""u8, default);
            want(defˢ, default);
            want(""u8, Δio.EOF);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooFooˢ => ref fooFooˢᵇ;
private static readonly @string fooFooˢᵇ = "foo foo"u8;
internal static ref readonly @string barBarˢ => ref barBarˢᵇ;
private static readonly @string barBarˢᵇ = "bar bar"u8;
internal static ref readonly @string recurˢ => ref recurˢᵇ;
private static readonly @string recurˢᵇ = "recur"u8;
internal static ref readonly @string recur2ˢ => ref recur2ˢᵇ;
private static readonly @string recur2ˢᵇ = "recur2"u8;

public static partial void TestReaderReset(ж<Δtesting.T> t) {
    void checkAll(ж<bufio.Reader> rΔ1, @string want) {
        t.Helper();
        var (all, err) = Δio.ReadAll(new bufio_test_package.bufio_ReaderжReader(rΔ1));
        if (err != nil) {
            t.Fatal(err);
        }
        if (((sstring)all) != want) {
            t.Errorf("ReadAll returned %q, want %q", all, want);
        }
    }

    var r = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(fooFooˢ)));
    var buf = new slice<byte>(3);
    r.Read(buf);
    if (((sstring)buf) != "foo"u8) {
        t.Errorf("buf = %q; want foo", buf);
    }

    r.Reset(new bufio_test_package.strings_ReaderжReader(strings.NewReader(barBarˢ)));
    checkAll(r, barBarˢ);

    r.Value = new Reader(); // zero out the Reader
    r.Reset(new bufio_test_package.strings_ReaderжReader(strings.NewReader(barBarˢ)));
    checkAll(r, barBarˢ);

    // Wrap a reader and then Reset to that reader.
    r.Reset(new bufio_test_package.strings_ReaderжReader(strings.NewReader(recurˢ)));
    var r2 = NewReader(new bufio_test_package.bufio_ReaderжReader(r));
    checkAll(r2, recurˢ);
    r.Reset(new bufio_test_package.strings_ReaderжReader(strings.NewReader(recur2ˢ)));
    r2.Reset(new bufio_test_package.bufio_ReaderжReader(r));
    checkAll(r2, recur2ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;
internal static ref readonly @string barˢ => ref barˢᵇ;
private static readonly @string barˢᵇ = "bar"u8;

public static void TestWriterReset(ж<Δtesting.T> t) {
    ref var buf1 = ref heap(new strings.Builder(), out var Ꮡbuf1);
    ref var buf2 = ref heap(new strings.Builder(), out var Ꮡbuf2);
    ref var buf3 = ref heap(new strings.Builder(), out var Ꮡbuf3);
    ref var buf4 = ref heap(new strings.Builder(), out var Ꮡbuf4);
    ref var buf5 = ref heap(new strings.Builder(), out var Ꮡbuf5);
    var w = NewWriter(new bufio_test_package.strings_BuilderжWriter(Ꮡbuf1));
    w.WriteString(fooˢ);

    w.Reset(new bufio_test_package.strings_BuilderжWriter(Ꮡbuf2)); // and not flushed
    w.WriteString(barˢ);
    w.Flush();
    if (buf1.String() != ""u8) {
        t.Errorf("buf1 = %q; want empty", buf1.String());
    }
    if (buf2.String() != "bar"u8) {
        t.Errorf("buf2 = %q; want bar", buf2.String());
    }

    w.Value = new Writer();                                        // zero out the Writer
    w.Reset(new bufio_test_package.strings_BuilderжWriter(Ꮡbuf3)); // and not flushed
    w.WriteString(barˢ);
    w.Flush();
    if (buf1.String() != ""u8) {
        t.Errorf("buf1 = %q; want empty", buf1.String());
    }
    if (buf3.String() != "bar"u8) {
        t.Errorf("buf3 = %q; want bar", buf3.String());
    }

    // Wrap a writer and then Reset to that writer.
    w.Reset(new bufio_test_package.strings_BuilderжWriter(Ꮡbuf4));
    var w2 = NewWriter(new bufio_test_package.bufio_WriterжWriter(w));
    w2.WriteString(recurˢ);
    w2.Flush();
    if (buf4.String() != "recur"u8) {
        t.Errorf("buf4 = %q, want %q", buf4.String(), recurˢ);
    }
    w.Reset(new bufio_test_package.strings_BuilderжWriter(Ꮡbuf5));
    w2.Reset(new bufio_test_package.bufio_WriterжWriter(w));
    w2.WriteString(recur2ˢ);
    w2.Flush();
    if (buf5.String() != "recur2"u8) {
        t.Errorf("buf5 = %q, want %q", buf5.String(), recur2ˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string thenErrorˢ => ref thenErrorˢᵇ;
private static readonly @string thenErrorˢᵇ = "5-then-error"u8;

internal partial struct TestReaderDiscard_tests /*dyn*/ {
    internal @string name;
    internal Δio.Reader r;
    internal nint bufSize; // 0 means 16
    internal nint peekSize;
    internal nint n; // input to Discard
    internal nint want;     // from Discard
    internal error wantErr; // from Discard
    internal nint wantBuffered;
}

public static void TestReaderDiscard(ж<Δtesting.T> t) {
    var tests = slice<TestReaderDiscard_tests>([
        new(
            name: "normal case",
            r: new bufio_test_package.strings_ReaderжReader(strings.NewReader("abcdefghijklmnopqrstuvwxyz")),
            peekSize: 16,
            n: 6,
            want: 6,
            wantBuffered: 10
        ),
        new(
            name: "discard causing read",
            r: new bufio_test_package.strings_ReaderжReader(strings.NewReader("abcdefghijklmnopqrstuvwxyz")),
            n: 6,
            want: 6,
            wantBuffered: 10
        ),
        new(
            name: "discard all without peek",
            r: new bufio_test_package.strings_ReaderжReader(strings.NewReader("abcdefghijklmnopqrstuvwxyz")),
            n: 26,
            want: 26,
            wantBuffered: 0
        ),
        new(
            name: "discard more than end",
            r: new bufio_test_package.strings_ReaderжReader(strings.NewReader("abcdefghijklmnopqrstuvwxyz")),
            n: 27,
            want: 26,
            wantErr: Δio.EOF,
            wantBuffered: 0
        ),
        // Any error from filling shouldn't show up until we
        // get past the valid bytes. Here we return 5 valid bytes at the same time
        // as an error, but test that we don't see the error from Discard.
        new(
            name: "fill error, discard less",
            r: newScriptedReader((slice<byte> p) => {
                if (len(p) < 5) {
                    throw panic("unexpected small read");
                }
                return (5, errors.New(thenErrorˢ));
            }),
            n: 4,
            want: 4,
            wantErr: default,
            wantBuffered: 1
        ),
        new(
            name: "fill error, discard equal",
            r: newScriptedReader((slice<byte> p) => {
                if (len(p) < 5) {
                    throw panic("unexpected small read");
                }
                return (5, errors.New(thenErrorˢ));
            }),
            n: 5,
            want: 5,
            wantErr: default,
            wantBuffered: 0
        ),
        new(
            name: "fill error, discard more",
            r: newScriptedReader((slice<byte> p) => {
                if (len(p) < 5) {
                    throw panic("unexpected small read");
                }
                return (5, errors.New(thenErrorˢ));
            }),
            n: 6,
            want: 5,
            wantErr: errors.New(thenErrorˢ),
            wantBuffered: 0
        ),
        // Discard of 0 shouldn't cause a read:
        new(
            name: "discard zero",
            r: newScriptedReader(), // will panic on Read
            n: 0,
            want: 0,
            wantErr: default,
            wantBuffered: 0
        ),
        new(
            name: "discard negative",
            r: newScriptedReader(), // will panic on Read
            n: -1,
            want: 0,
            wantErr: ErrNegativeCount,
            wantBuffered: 0
        )
    ]);
    foreach (var (_, tt) in tests) {
        var br = NewReaderSize(tt.r, tt.bufSize);
        if (tt.peekSize > 0) {
            var (peekBuf, errΔ1) = br.Peek(tt.peekSize);
            if (errΔ1 != nil) {
                t.Errorf("%s: Peek(%d): %v", tt.name, tt.peekSize, errΔ1);
                continue;
            }
            if (len(peekBuf) != tt.peekSize) {
                t.Errorf("%s: len(Peek(%d)) = %v; want %v", tt.name, tt.peekSize, len(peekBuf), tt.peekSize);
                continue;
            }
        }
        var (discarded, err) = br.Discard(tt.n);
        {
            @string ge = fmt.Sprint(err);
            @string we = fmt.Sprint(tt.wantErr); if (discarded != tt.want || ge != we) {
                t.Errorf("%s: Discard(%d) = (%v, %v); want (%v, %v)", tt.name, tt.n, discarded, ge, tt.want, we);
                continue;
            }
        }
        {
            nint bn = br.Buffered(); if (bn != tt.wantBuffered) {
                t.Errorf("%s: after Discard, Buffered = %d; want %d", tt.name, bn, tt.wantBuffered);
            }
        }
    }
}

public static void TestReaderSize(ж<Δtesting.T> t) {
    {
        nint got = NewReader(default).Size();
        nint want = bufio_internal_test_package.DefaultBufSize; if (got != want) {
            t.Errorf("NewReader's Reader.Size = %d; want %d", got, want);
        }
    }
    {
        nint got = NewReaderSize(default, 1234).Size();
        nint want = 1234; if (got != want) {
            t.Errorf("NewReaderSize's Reader.Size = %d; want %d", got, want);
        }
    }
}

public static void TestWriterSize(ж<Δtesting.T> t) {
    {
        nint got = NewWriter(default).Size();
        nint want = bufio_internal_test_package.DefaultBufSize; if (got != want) {
            t.Errorf("NewWriter's Writer.Size = %d; want %d", got, want);
        }
    }
    {
        nint got = NewWriterSize(default, 1234).Size();
        nint want = 1234; if (got != want) {
            t.Errorf("NewWriterSize's Writer.Size = %d; want %d", got, want);
        }
    }
}

// An onlyReader only implements io.Reader, no matter what other methods the underlying implementation may have.
partial struct onlyReader {
    /*embed*/ public io_package.Reader Reader;
}

// An onlyWriter only implements io.Writer, no matter what other methods the underlying implementation may have.
partial struct onlyWriter {
    /*embed*/ public io_package.Writer Writer;
}

// A scriptedReader is an io.Reader that executes its steps sequentially.
partial struct scriptedReader /*[]Func<slice<byte>, (nint n, error err)>*/;

internal static (nint n, error err) Read(this ref scriptedReader sr, slice<byte> p) {
    if (len(sr) == 0) {
        throw panic("too many Read calls on scripted Reader. No steps remain.");
    }
    var step = (sr)[0];
    sr = (sr)[1..];
    return step(p);
}

internal static Δio.Reader newScriptedReader(params Span<Func<slice<byte>, (nint, error)>> stepsʗp) {
    var steps = stepsʗp.slice();

    ref var sr = ref heap<scriptedReader>((scriptedReader)steps, out var Ꮡsr);
    return new bufio_test_package.scriptedReaderжReader(Ꮡsr);
}

// eofReader returns the number of bytes read and io.EOF for the read that consumes the last of the content.
partial struct eofReader {
    internal slice<byte> buf;
}

internal static (nint, error) Read(this ref eofReader r, slice<byte> p) {
    nint read = copy(p, r.buf);
    r.buf = r.buf.slice(read);

    switch (read) {
    case var ᴛ when ᴛ == 0 || ᴛ == len(r.buf): {
        // As allowed in the documentation, this will return io.EOF
        // in the same call that consumes the last of the data.
        // https://godoc.org/io#Reader
        return (read, Δio.EOF);
    }}

    return (read, default);
}

public static void TestPartialReadEOF(ж<Δtesting.T> t) {
    var src = new slice<byte>(10);
    var eofR = Ꮡ(new eofReader(buf: src));
    var r = NewReader(new bufio_test_package.eofReaderжReader(eofR));

    // Start by reading 5 of the 10 available bytes.
    var dest = new slice<byte>(5);
    var (read, err) = r.Read(dest);
    if (err != nil) {
        t.Fatalf("unexpected error: %v", err);
    }
    {
        nint n = len(dest); if (read != n) {
            t.Fatalf("read %d bytes; wanted %d bytes", read, n);
        }
    }

    // The Reader should have buffered all the content from the io.Reader.
    {
        nint n = len(eofR.buf); if (n != 0) {
            t.Fatalf("got %d bytes left in bufio.Reader source; want 0 bytes", n);
        }
    }
    // To prove the point, check that there are still 5 bytes available to read.
    {
        nint n = r.Buffered(); if (n != 5) {
            t.Fatalf("got %d bytes buffered in bufio.Reader; want 5 bytes", n);
        }
    }

    // This is the second read of 0 bytes.
    (read, err) = r.Read(slice<byte>([]));
    if (err != nil) {
        t.Fatalf("unexpected error: %v", err);
    }
    if (read != 0) {
        t.Fatalf("read %d bytes; want 0 bytes", read);
    }
}

partial struct writerWithReadFromError {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string writerWithReadFromErrorˢ => ref writerWithReadFromErrorˢᵇ;
private static readonly @string writerWithReadFromErrorˢᵇ = "writerWithReadFromError error"u8;

internal static (int64, error) ReadFrom(this writerWithReadFromError w, Δio.Reader r) {
    return (0, errors.New(writerWithReadFromErrorˢ));
}

internal static (nint n, error err) Write(this writerWithReadFromError w, slice<byte> b) {
    return (10, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string test2ˢ => ref test2ˢᵇ;
private static readonly @string test2ˢᵇ = "test2"u8;
internal static readonly object expectedReadFromReturnsˢ = (@string)"expected ReadFrom returns error, got nil"u8;
internal static readonly object expectedWriteReturnsˢ = (@string)"expected Write returns error, got nil"u8;

public static void TestWriterReadFromMustSetUnderlyingError(ж<Δtesting.T> t) {
    ж<bufio.Writer> wr = NewWriter(new writerWithReadFromError());
    {
        var (_, err) = wr.ReadFrom(new bufio_test_package.strings_ReaderжReader(strings.NewReader(test2ˢ))); if (err == nil) {
            t.Fatal(expectedReadFromReturnsˢ);
        }
    }
    {
        var (_, err) = wr.Write(slice<byte>("123"u8)); if (err == nil) {
            t.Fatal(expectedWriteReturnsˢ);
        }
    }
}

partial struct writeErrorOnlyWriter {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string writeErrorOnlyWriterˢ => ref writeErrorOnlyWriterˢᵇ;
private static readonly @string writeErrorOnlyWriterˢᵇ = "writeErrorOnlyWriter error"u8;

internal static (nint n, error err) Write(this writeErrorOnlyWriter w, slice<byte> p) {
    return (0, errors.New(writeErrorOnlyWriterˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string test1ˢ => ref test1ˢᵇ;
private static readonly @string test1ˢᵇ = "test1"u8;
internal static readonly object expectedFlushErrorGotNilˢ = (@string)"expected flush error, got nil"u8;
internal static readonly object expectedErrorGotNilˢ = (@string)"expected error, got nil"u8;

// Ensure that previous Write errors are immediately returned
// on any ReadFrom. See golang.org/issue/35194.
public static void TestWriterReadFromMustReturnUnderlyingError(ж<Δtesting.T> t) {
    ж<bufio.Writer> wr = NewWriter(new writeErrorOnlyWriter());
    @string s = test1ˢ;
    nint wantBuffered = len(s);
    {
        var (_, err) = wr.WriteString(s); if (err != nil) {
            t.Fatalf("unexpected error: %v", err);
        }
    }
    {
        var err = wr.Flush(); if (err == nil) {
            t.Error(expectedFlushErrorGotNilˢ);
        }
    }
    {
        var (_, err) = wr.ReadFrom(new bufio_test_package.strings_ReaderжReader(strings.NewReader(test2ˢ))); if (err == nil) {
            t.Fatal(expectedErrorGotNilˢ);
        }
    }
    {
        nint buffered = wr.Buffered(); if (buffered != wantBuffered) {
            t.Fatalf("Buffered = %v; want %v", buffered, wantBuffered);
        }
    }
}

public static void BenchmarkReaderCopyOptimal(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Optimal case is where the underlying reader implements io.WriterTo
    var srcBuf = bytes.NewBuffer(new slice<byte>(8192));
    var src = NewReader(new bufio_test_package.bytes_BufferжReader(srcBuf));
    var dstBuf = @new<bytes.Buffer>();
    var dst = new onlyWriter(new bufio_test_package.bytes_BufferжWriter(dstBuf));
    for (nint i = 0; i < b.N; i++) {
        srcBuf.Reset();
        src.Reset(new bufio_test_package.bytes_BufferжReader(srcBuf));
        dstBuf.Reset();
        Δio.Copy(dst, new bufio_test_package.bufio_ReaderжReader(src));
    }
}

public static void BenchmarkReaderCopyUnoptimal(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Unoptimal case is where the underlying reader doesn't implement io.WriterTo
    var srcBuf = bytes.NewBuffer(new slice<byte>(8192));
    var src = NewReader(new onlyReader(new bufio_test_package.bytes_BufferжReader(srcBuf)));
    var dstBuf = @new<bytes.Buffer>();
    var dst = new onlyWriter(new bufio_test_package.bytes_BufferжWriter(dstBuf));
    for (nint i = 0; i < b.N; i++) {
        srcBuf.Reset();
        src.Reset(new onlyReader(new bufio_test_package.bytes_BufferжReader(srcBuf)));
        dstBuf.Reset();
        Δio.Copy(dst, new bufio_test_package.bufio_ReaderжReader(src));
    }
}

public static void BenchmarkReaderCopyNoWriteTo(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var srcBuf = bytes.NewBuffer(new slice<byte>(8192));
    var srcReader = NewReader(new bufio_test_package.bytes_BufferжReader(srcBuf));
    var src = new onlyReader(new bufio_test_package.bufio_ReaderжReader(srcReader));
    var dstBuf = @new<bytes.Buffer>();
    var dst = new onlyWriter(new bufio_test_package.bytes_BufferжWriter(dstBuf));
    for (nint i = 0; i < b.N; i++) {
        srcBuf.Reset();
        srcReader.Reset(new bufio_test_package.bytes_BufferжReader(srcBuf));
        dstBuf.Reset();
        Δio.Copy(dst, src);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object ioDiscardDoesnTSupportˢ = (@string)"io.Discard doesn't support ReaderFrom"u8;

public static void BenchmarkReaderWriteToOptimal(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    UntypedInt bufSize = /* 16 << 10 */ 16384;
    var buf = new slice<byte>(bufSize);
    var r = bytes.NewReader(buf);
    var srcReader = NewReaderSize(new onlyReader(new bufio_test_package.bytes_ReaderжReader(r)), 1 << 10);
    {
        var (_, ok) = Δio.Discard._<Δio.ReaderFrom>(ᐧ); if (!ok) {
            Ꮡb.Fatal(ioDiscardDoesnTSupportˢ);
        }
    }
    for (nint i = 0; i < b.N; i++) {
        r.Seek(0, Δio.SeekStart);
        srcReader.Reset(new onlyReader(new bufio_test_package.bytes_ReaderжReader(r)));
        var (n, err) = srcReader.WriteTo(Δio.Discard);
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
        if (n != bufSize) {
            Ꮡb.Fatalf("n = %d; want %d", n, (nint)(bufSize));
        }
    }
}

public static void BenchmarkReaderReadString(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = strings.NewReader(fooFoo424242424242424242ˢ);
    var buf = NewReader(new bufio_test_package.strings_ReaderжReader(r));
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        r.Seek(0, Δio.SeekStart);
        buf.Reset(new bufio_test_package.strings_ReaderжReader(r));

        var (_, err) = buf.ReadString((byte)'\n');
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
    }
}

public static void BenchmarkWriterCopyOptimal(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Optimal case is where the underlying writer implements io.ReaderFrom
    var srcBuf = bytes.NewBuffer(new slice<byte>(8192));
    var src = new onlyReader(new bufio_test_package.bytes_BufferжReader(srcBuf));
    var dstBuf = @new<bytes.Buffer>();
    var dst = NewWriter(new bufio_test_package.bytes_BufferжWriter(dstBuf));
    for (nint i = 0; i < b.N; i++) {
        srcBuf.Reset();
        dstBuf.Reset();
        dst.Reset(new bufio_test_package.bytes_BufferжWriter(dstBuf));
        Δio.Copy(new bufio_test_package.bufio_WriterжWriter(dst), src);
    }
}

public static void BenchmarkWriterCopyUnoptimal(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var srcBuf = bytes.NewBuffer(new slice<byte>(8192));
    var src = new onlyReader(new bufio_test_package.bytes_BufferжReader(srcBuf));
    var dstBuf = @new<bytes.Buffer>();
    var dst = NewWriter(new onlyWriter(new bufio_test_package.bytes_BufferжWriter(dstBuf)));
    for (nint i = 0; i < b.N; i++) {
        srcBuf.Reset();
        dstBuf.Reset();
        dst.Reset(new onlyWriter(new bufio_test_package.bytes_BufferжWriter(dstBuf)));
        Δio.Copy(new bufio_test_package.bufio_WriterжWriter(dst), src);
    }
}

public static void BenchmarkWriterCopyNoReadFrom(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var srcBuf = bytes.NewBuffer(new slice<byte>(8192));
    var src = new onlyReader(new bufio_test_package.bytes_BufferжReader(srcBuf));
    var dstBuf = @new<bytes.Buffer>();
    var dstWriter = NewWriter(new bufio_test_package.bytes_BufferжWriter(dstBuf));
    var dst = new onlyWriter(new bufio_test_package.bufio_WriterжWriter(dstWriter));
    for (nint i = 0; i < b.N; i++) {
        srcBuf.Reset();
        dstBuf.Reset();
        dstWriter.Reset(new bufio_test_package.bytes_BufferжWriter(dstBuf));
        Δio.Copy(dst, src);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object wrongLengthˢ = (@string)"wrong length"u8;

public static void BenchmarkReaderEmpty(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    @string str = strings.Repeat("x", 16 << 10);
    for (nint i = 0; i < b.N; i++) {
        var br = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(str)));
        var (n, err) = Δio.Copy(Δio.Discard, new bufio_test_package.bufio_ReaderжReader(br));
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
        if (n != (int64)len(str)) {
            Ꮡb.Fatal(wrongLengthˢ);
        }
    }
}

public static void BenchmarkWriterEmpty(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    @string str = strings.Repeat("x", 1 << 10);
    var bs = slice<byte>(str);
    for (nint i = 0; i < b.N; i++) {
        var bw = NewWriter(Δio.Discard);
        bw.Flush();
        bw.WriteByte((byte)'a');
        bw.Flush();
        bw.WriteRune('B');
        bw.Flush();
        bw.Write(bs);
        bw.Flush();
        bw.WriteString(str);
        bw.Flush();
    }
}

public static void BenchmarkWriterFlush(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var bw = NewWriter(Δio.Discard);
    @string str = strings.Repeat("x", 50);
    for (nint i = 0; i < b.N; i++) {
        bw.WriteString(str);
        bw.Flush();
    }
}

} // end bufio_test_package
