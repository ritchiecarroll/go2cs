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
    r13.Value.r = r;
    return r13;
}

internal static (nint, error) Read(this ref rot13Reader r13, slice<byte> p) {
    var (n, err) = r13.r.Read(p);
    for (nint i = 0; i < n; i++) {
        var c = (byte)(p[i] | 0x20); // lowercase byte
        if ((rune)'a' <= c && c <= (rune)'m'){
            p[i] += 13;
        } else 
        if ((rune)'n' <= c && c <= (rune)'z') {
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
    while (ᐧ) {
        var (c, err) = buf.ReadByte();
        if (AreEqual(err, Δio.EOF)) {
            break;
        }
        if (err == default!){
            b[nb] = c;
            nb++;
        } else 
        if (!AreEqual(err, iotest.ErrTimeout)) {
            throw panic("Data: " + err.Error());
        }
    }
    return ((@string)(b.slice(0, nb)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloWorldˢ = "hello world"u8;

public static void TestReaderSimple(ж<Δtesting.T> Ꮡt) {
    @string data = helloWorldˢ;
    var b = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(data)));
    {
        @string s = readBytes(b); if (s != "hello world"u8) {
            Ꮡt.Errorf("simple hello world test failed: got %q"u8, s);
        }
    }
    b = NewReader(new bufio_test_package.rot13ReaderжReader(newRot13Reader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(data)))));
    {
        @string s = readBytes(b); if (s != "uryyb jbeyq"u8) {
            Ꮡt.Errorf("rot13 hello world test failed: got %q"u8, s);
        }
    }
}

partial struct readMaker {
    internal @string name;
    internal Func<Δio.Reader, Δio.Reader> fn;
}

internal static slice<readMaker> readMakers = new readMaker[]{
    new("full"u8, (Δio.Reader r) => r),
    new("byte"u8, iotest.OneByteReader),
    new("half"u8, iotest.HalfReader),
    new("data+err"u8, iotest.DataErrReader),
    new("timeout"u8, iotest.TimeoutReader)
}.slice();

// Call ReadString (which ends up calling everything else)
// to accumulate the text of a file.
internal static @string readLines(ж<bufio.Reader> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string s = ""u8;
    while (ᐧ) {
        var (s1, err) = b.ReadString((rune)'\n');
        if (AreEqual(err, Δio.EOF)) {
            break;
        }
        if (err != default! && !AreEqual(err, iotest.ErrTimeout)) {
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
    while (ᐧ) {
        var (n, err) = buf.Read(b.slice(nb, nb + m));
        nb += n;
        if (AreEqual(err, Δio.EOF)) {
            break;
        }
    }
    return ((@string)(b.slice(0, nb)));
}

partial struct bufReader {
    internal @string name;
    internal Func<ж<bufio.Reader>, @string> fn;
}

internal static slice<bufReader> bufreaders = new bufReader[]{
    new("1"u8, (ж<bufio.Reader> b) => reads(b, 1)),
    new("2"u8, (ж<bufio.Reader> b) => reads(b, 2)),
    new("3"u8, (ж<bufio.Reader> b) => reads(b, 3)),
    new("4"u8, (ж<bufio.Reader> b) => reads(b, 4)),
    new("5"u8, (ж<bufio.Reader> b) => reads(b, 5)),
    new("7"u8, (ж<bufio.Reader> b) => reads(b, 7)),
    new("bytes"u8, readBytes),
    new("lines"u8, readLines)
}.slice();

internal static UntypedInt minReadBufferSize => 16;

internal static slice<nint> bufsizes = new nint[]{
    0, minReadBufferSize, 23, 32, 46, 64, 93, 128, 1024, 4096
}.slice();

public static void TestReader(ж<Δtesting.T> Ꮡt) {
    array<@string> texts = new(31);
    @string str = ""u8;
    @string all = ""u8;
    for (nint i = 0; i < len(texts) - 1; i++) {
        texts[i] = str + "\n"u8;
        all += texts[i];
        str += ((@string)(rune)(i % 26 + (rune)'a'));
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
                        Ꮡt.Errorf("reader=%s fn=%s bufsize=%d want=%q got=%q"u8,
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
    return (0, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorExpectedˢ = (@string)"error expected"u8;
internal static readonly object unexpectedErrorˢ = (@string)"unexpected error:"u8;
internal static readonly object testTimedOutEndlessLoopˢ = (@string)"test timed out (endless loop in ReadByte?)"u8;

public static partial void TestZeroReader(ж<Δtesting.T> Ꮡt) {
    zeroReader z = default!;
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
        if (err == default!){
            Ꮡt.Error(errorExpectedˢ);
        } else 
        if (!AreEqual(err, Δio.ErrNoProgress)) {
            Ꮡt.Error(unexpectedErrorˢ, err);
        }
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out _): {
        Ꮡt.Error(testTimedOutEndlessLoopˢ);
        break;
    }}
}

// A StringReader delivers its data one string segment at a time via Read.
partial struct StringReader {
    internal slice<@string> data;
    internal nint step;
}

public static (nint n, error err) Read(this ref StringReader r, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (r.step < len(r.data)){
        @string s = r.data[r.step];
        n = copy(p, s);
        r.step++;
    } else {
        err = Δio.EOF;
    }
    return (n, err);
}

internal static void readRuneSegments(ж<Δtesting.T> Ꮡt, slice<@string> segments) {
    @string got = ""u8;
    @string want = strings.Join(segments, ""u8);
    var r = NewReader(new bufio_test_package.StringReaderжReader(Ꮡ(new StringReader(data: segments))));
    while (ᐧ) {
        var (rΔ1, _, err) = r.ReadRune();
        if (err != default!) {
            if (!AreEqual(err, Δio.EOF)) {
                return;
            }
            break;
        }
        got += ((@string)rΔ1);
    }
    if (got != want) {
        Ꮡt.Errorf("segments=%v got=%s want=%s"u8, segments, got, want);
    }
}

internal static slice<slice<@string>> segmentList = new slice<@string>[]{
    new @string[]{}.slice(),
    new @string[]{""u8}.slice(),
    new @string[]{"日"u8, "本語"u8}.slice(),
    new @string[]{"\u65e5"u8, "\u672c"u8, "\u8a9e"u8}.slice(),
    new @string[]{"\U000065e5"u8, "\U0000672c"u8, "\U00008a9e"u8}.slice(),
    new @string[]{((@string)(new byte[]{0xe6})), ((@string)(new byte[]{0x97, 0xa5, 0xe6})), ((@string)(new byte[]{0x9c, 0xac, 0xe8, 0xaa, 0x9e}))}.slice(),
    new @string[]{"Hello"u8, ", "u8, "World"u8, "!"u8}.slice(),
    new @string[]{"Hello"u8, ", "u8, ""u8, "World"u8, "!"u8}.slice()
}.slice();

public static void TestReadRune(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, s) in segmentList) {
        readRuneSegments(Ꮡt, s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedErrorOnˢ = (@string)"unexpected error on ReadRune:"u8;
internal static readonly object unexpectedErrorOnˢ2 = (@string)"unexpected error on UnreadRune:"u8;
internal static readonly object unexpectedErrorReadingˢ = (@string)"unexpected error reading after unreading:"u8;

public static void TestUnreadRune(ж<Δtesting.T> Ꮡt) {
    var segments = new @string[]{"Hello, world:"u8, "日本語"u8}.slice();
    var r = NewReader(new bufio_test_package.StringReaderжReader(Ꮡ(new StringReader(data: segments))));
    @string got = ""u8;
    @string want = strings.Join(segments, ""u8);
    // Normal execution.
    while (ᐧ) {
        var (r1, _, err) = r.ReadRune();
        if (err != default!) {
            if (!AreEqual(err, Δio.EOF)) {
                Ꮡt.Error(unexpectedErrorOnˢ, err);
            }
            break;
        }
        got += ((@string)r1);
        // Put it back and read it again.
        {
            err = r.UnreadRune(); if (err != default!) {
                Ꮡt.Fatal(unexpectedErrorOnˢ2, err);
            }
        }
        (var r2, _, err) = r.ReadRune();
        if (err != default!) {
            Ꮡt.Fatal(unexpectedErrorReadingˢ, err);
        }
        if (r1 != r2) {
            Ꮡt.Fatalf("incorrect rune after unread: got %c, want %c"u8, r1, r2);
        }
    }
    if (got != want) {
        Ꮡt.Errorf("got %q, want %q"u8, got, want);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exampleˢ = "example"u8;
internal static readonly object unreadRuneDidnTFailAfterˢ = (@string)"UnreadRune didn't fail after Peek"u8;

public static void TestNoUnreadRuneAfterPeek(ж<Δtesting.T> Ꮡt) {
    var br = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(exampleˢ)));
    br.ReadRune();
    br.Peek(1);
    {
        var err = br.UnreadRune(); if (err == default!) {
            Ꮡt.Error(unreadRuneDidnTFailAfterˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadByteDidnTFailAfterˢ = (@string)"UnreadByte didn't fail after Peek"u8;

public static void TestNoUnreadByteAfterPeek(ж<Δtesting.T> Ꮡt) {
    var br = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(exampleˢ)));
    br.ReadByte();
    br.Peek(1);
    {
        var err = br.UnreadByte(); if (err == default!) {
            Ꮡt.Error(unreadByteDidnTFailAfterˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadRuneDidnTFailAfterˢ2 = (@string)"UnreadRune didn't fail after Discard"u8;

public static void TestNoUnreadRuneAfterDiscard(ж<Δtesting.T> Ꮡt) {
    var br = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(exampleˢ)));
    br.ReadRune();
    br.Discard(1);
    {
        var err = br.UnreadRune(); if (err == default!) {
            Ꮡt.Error(unreadRuneDidnTFailAfterˢ2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadByteDidnTFailAfterˢ2 = (@string)"UnreadByte didn't fail after Discard"u8;

public static void TestNoUnreadByteAfterDiscard(ж<Δtesting.T> Ꮡt) {
    var br = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(exampleˢ)));
    br.ReadByte();
    br.Discard(1);
    {
        var err = br.UnreadByte(); if (err == default!) {
            Ꮡt.Error(unreadByteDidnTFailAfterˢ2);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadRuneDidnTFailAfterˢ3 = (@string)"UnreadRune didn't fail after WriteTo"u8;

public static void TestNoUnreadRuneAfterWriteTo(ж<Δtesting.T> Ꮡt) {
    var br = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(exampleˢ)));
    br.WriteTo(Δio.Discard);
    {
        var err = br.UnreadRune(); if (err == default!) {
            Ꮡt.Error(unreadRuneDidnTFailAfterˢ3);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadByteDidnTFailAfterˢ3 = (@string)"UnreadByte didn't fail after WriteTo"u8;

public static void TestNoUnreadByteAfterWriteTo(ж<Δtesting.T> Ꮡt) {
    var br = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(exampleˢ)));
    br.WriteTo(Δio.Discard);
    {
        var err = br.UnreadByte(); if (err == default!) {
            Ꮡt.Error(unreadByteDidnTFailAfterˢ3);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unexpectedErrorOnˢ3 = (@string)"unexpected error on ReadByte:"u8;
internal static readonly object unexpectedErrorOnˢ4 = (@string)"unexpected error on UnreadByte:"u8;

public static void TestUnreadByte(ж<Δtesting.T> Ꮡt) {
    var segments = new @string[]{"Hello, "u8, "world"u8}.slice();
    var r = NewReader(new bufio_test_package.StringReaderжReader(Ꮡ(new StringReader(data: segments))));
    @string got = ""u8;
    @string want = strings.Join(segments, ""u8);
    // Normal execution.
    while (ᐧ) {
        var (b1, err) = r.ReadByte();
        if (err != default!) {
            if (!AreEqual(err, Δio.EOF)) {
                Ꮡt.Error(unexpectedErrorOnˢ3, err);
            }
            break;
        }
        got += ((@string)b1);
        // Put it back and read it again.
        {
            err = r.UnreadByte(); if (err != default!) {
                Ꮡt.Fatal(unexpectedErrorOnˢ4, err);
            }
        }
        (var b2, err) = r.ReadByte();
        if (err != default!) {
            Ꮡt.Fatal(unexpectedErrorReadingˢ, err);
        }
        if (b1 != b2) {
            Ꮡt.Fatalf("incorrect byte after unread: got %q, want %q"u8, b1, b2);
        }
    }
    if (got != want) {
        Ꮡt.Errorf("got %q, want %q"u8, got, want);
    }
}

public static void TestUnreadByteMultiple(ж<Δtesting.T> Ꮡt) {
    var segments = new @string[]{"Hello, "u8, "world"u8}.slice();
    @string data = strings.Join(segments, ""u8);
    for (nint n = 0; n <= len(data); n++) {
        var r = NewReader(new bufio_test_package.StringReaderжReader(Ꮡ(new StringReader(data: segments))));
        // Read n bytes.
        for (nint i = 0; i < n; i++) {
            var (b, err) = r.ReadByte();
            if (err != default!) {
                Ꮡt.Fatalf("n = %d: unexpected error on ReadByte: %v"u8, n, err);
            }
            if (b != data[i]) {
                Ꮡt.Fatalf("n = %d: incorrect byte returned from ReadByte: got %q, want %q"u8, n, b, data[i]);
            }
        }
        // Unread one byte if there is one.
        if (n > 0) {
            {
                var err = r.UnreadByte(); if (err != default!) {
                    Ꮡt.Errorf("n = %d: unexpected error on UnreadByte: %v"u8, n, err);
                }
            }
        }
        // Test that we cannot unread any further.
        {
            var err = r.UnreadByte(); if (err == default!) {
                Ꮡt.Errorf("n = %d: expected error on UnreadByte"u8, n);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcdefgˢ = "abcdefg"u8;
internal static readonly @string abcdˢ = "abcd"u8;
internal static readonly @string efgˢ = "efg"u8;

public static void TestUnreadByteOthers(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

// ReadLine doesn't fit the data/pattern easily
// so we leave it out. It should be covered via
// the ReadSlice test since ReadLine simply calls
// ReadSlice, and it's that function that handles
// the last byte.
    // A list of readers to use in conjunction with UnreadByte.
    slice<Func<ж<bufio.Reader>, byte, (slice<byte>, error)>> readers = new Func<ж<bufio.Reader>, byte, (slice<byte>, error)>[]{
        ((Func<ж<bufio.Reader>, byte, (slice<byte>, error)>)(bufio.ReadBytes)),
        ((Func<ж<bufio.Reader>, byte, (slice<byte>, error)>)(bufio.ReadSlice)),
        (ж<bufio.Reader> r, byte delim) => {
            var (data, err) = r.ReadString(delim);
            return (slice<byte>(data), err);
        }
    }.slice();
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
            if (errΔ1 != default!) {
                Ꮡt.Fatalf("#%d: unexpected error reading to %c: %v"u8, rno, delim, errΔ1);
            }
            {
                @string got = ((@string)data); if (got != want) {
                    Ꮡt.Fatalf("#%d: got %q, want %q"u8, rno, got, want);
                }
            }
        }
        // Read the data with occasional UnreadByte calls.
        for (nint i = 0; i < n; i++) {
            readTo((rune)'d', abcdˢ);
            for (nint j = 0; j < 3; j++) {
                {
                    var errΔ2 = r.UnreadByte(); if (errΔ2 != default!) {
                        Ꮡt.Fatalf("#%d: unexpected error on UnreadByte: %v"u8, rno, errΔ2);
                    }
                }
                readTo((rune)'d', "d"u8);
            }
            readTo((rune)'g', efgˢ);
        }
        // All data should have been read.
        var (_, err) = r.ReadByte();
        if (!AreEqual(err, Δio.EOF)) {
            Ꮡt.Errorf("#%d: got error %v; want EOF"u8, rno, err);
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
public static void TestUnreadRuneError(ж<Δtesting.T> Ꮡt) {
    var buf = new slice<byte>(3); // All runes in this test are 3 bytes long
    var r = NewReader(new bufio_test_package.StringReaderжReader(Ꮡ(new StringReader(data: new @string[]{"日本語日本語日本語"u8}.slice()))));
    if (r.UnreadRune() == default!) {
        Ꮡt.Error(expectedErrorOnˢ);
    }
    var (_, _, err) = r.ReadRune();
    if (err != default!) {
        Ꮡt.Error(unexpectedErrorOnˢ5, err);
    }
    {
        err = r.UnreadRune(); if (err != default!) {
            Ꮡt.Error(unexpectedErrorOnˢ6, err);
        }
    }
    if (r.UnreadRune() == default!) {
        Ꮡt.Error(expectedErrorAfterˢ);
    }
    // Test error after Read.
    (_, _, err) = r.ReadRune(); // reset state
    if (err != default!) {
        Ꮡt.Error(unexpectedErrorOnˢ7, err);
    }
    (_, err) = r.Read(buf);
    if (err != default!) {
        Ꮡt.Error(unexpectedErrorOnRead2ˢ, err);
    }
    if (r.UnreadRune() == default!) {
        Ꮡt.Error(expectedErrorAfterRead2ˢ);
    }
    // Test error after ReadByte.
    (_, _, err) = r.ReadRune(); // reset state
    if (err != default!) {
        Ꮡt.Error(unexpectedErrorOnˢ7, err);
    }
    foreach ((_, _) in buf) {
        (_, err) = r.ReadByte();
        if (err != default!) {
            Ꮡt.Error(unexpectedErrorOnˢ8, err);
        }
    }
    if (r.UnreadRune() == default!) {
        Ꮡt.Error(expectedErrorAfterˢ2);
    }
    // Test error after UnreadByte.
    (_, _, err) = r.ReadRune(); // reset state
    if (err != default!) {
        Ꮡt.Error(unexpectedErrorOnˢ9, err);
    }
    (_, err) = r.ReadByte();
    if (err != default!) {
        Ꮡt.Error(unexpectedErrorOnˢ10, err);
    }
    err = r.UnreadByte();
    if (err != default!) {
        Ꮡt.Error(unexpectedErrorOnˢ11, err);
    }
    if (r.UnreadRune() == default!) {
        Ꮡt.Error(expectedErrorAfterˢ3);
    }
    // Test error after ReadSlice.
    (_, _, err) = r.ReadRune(); // reset state
    if (err != default!) {
        Ꮡt.Error(unexpectedErrorOnˢ12, err);
    }
    (_, err) = r.ReadSlice(0);
    if (!AreEqual(err, Δio.EOF)) {
        Ꮡt.Error(unexpectedErrorOnˢ13, err);
    }
    if (r.UnreadRune() == default!) {
        Ꮡt.Error(expectedErrorAfterˢ4);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorAtEofˢ = (@string)"expected error at EOF"u8;
internal static readonly object expectedEofGotˢ = (@string)"expected EOF; got"u8;

public static void TestUnreadRuneAtEOF(ж<Δtesting.T> Ꮡt) {
    // UnreadRune/ReadRune should error at EOF (was a bug; used to panic)
    var r = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader("x"u8)));
    r.ReadRune();
    r.ReadRune();
    r.UnreadRune();
    var (_, _, err) = r.ReadRune();
    if (err == default!){
        Ꮡt.Error(expectedErrorAtEofˢ);
    } else 
    if (!AreEqual(err, Δio.EOF)) {
        Ꮡt.Error(expectedEofGotˢ, err);
    }
}

public static void TestReadWriteRune(ж<Δtesting.T> Ꮡt) {
    const rune NRune = 1000;
    var byteBuf = @new<bytes.Buffer>();
    var w = NewWriter(new bufio_test_package.bytes_BufferжWriter(byteBuf));
    // Write the runes out using WriteRune
    var buf = new slice<byte>(utf8.UTFMax);
    for (var rΔ1 = (rune)0; rΔ1 < NRune; rΔ1++) {
        nint size = utf8.EncodeRune(buf, rΔ1);
        var (nbytes, err) = w.WriteRune(rΔ1);
        if (err != default!) {
            Ꮡt.Fatalf("WriteRune(0x%x) error: %s"u8, rΔ1, err);
        }
        if (nbytes != size) {
            Ꮡt.Fatalf("WriteRune(0x%x) expected %d, got %d"u8, rΔ1, size, nbytes);
        }
    }
    w.Flush();
    var r = NewReader(new bufio_test_package.bytes_BufferжReader(byteBuf));
    // Read them back with ReadRune
    for (var r1 = (rune)0; r1 < NRune; r1++) {
        nint size = utf8.EncodeRune(buf, r1);
        var (nr, nbytes, err) = r.ReadRune();
        if (nr != r1 || nbytes != size || err != default!) {
            Ꮡt.Fatalf("ReadRune(0x%x) got 0x%x,%d not 0x%x,%d (err=%s)"u8, r1, nr, nbytes, r1, size, err);
        }
    }
}

public static void TestWriteInvalidRune(ж<Δtesting.T> Ꮡt) {
    // Invalid runes, including negative ones, should be written as the
    // replacement character.
    foreach (var (_, r) in new rune[]{-1, utf8.MaxRune + 1}.slice()) {
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        var w = NewWriter(new bufio_test_package.strings_BuilderжWriter(Ꮡbuf));
        w.WriteRune(r);
        w.Flush();
        {
            @string s = buf.String(); if (s != "\uFFFD"u8) {
                Ꮡt.Errorf("WriteRune(%d) wrote %q, not replacement character"u8, r, s);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testAllocatesMoreWithˢ = (@string)"test allocates more with -asan; see #70079"u8;
internal static readonly @string fooFoo424242424242424242ˢ = "       foo       foo        42        42        42        42        42        42        42        42       4.2       4.2       4.2       4.2\n"u8;

public static void TestReadStringAllocs(ж<Δtesting.T> Ꮡt) {
    if (asan.Enabled) {
        Ꮡt.Skip(testAllocatesMoreWithˢ);
    }
    var r = strings.NewReader(fooFoo424242424242424242ˢ);
    var buf = NewReader(new bufio_test_package.strings_ReaderжReader(r));
    var bufʗ1 = buf;
    var rʗ1 = r;
    var allocs = Δtesting.AllocsPerRun(100, () => {
        rʗ1.Seek(0, Δio.SeekStart);
        bufʗ1.Reset(new bufio_test_package.strings_ReaderжReader(rʗ1));
        var (_, err) = bufʗ1.ReadString((rune)'\n');
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
    });
    if (allocs != 1D) {
        Ꮡt.Errorf("Unexpected number of allocations, got %f, want 1"u8, allocs);
    }
}

public static void TestWriter(ж<Δtesting.T> Ꮡt) {
    array<byte> data = new(8192);
    for (nint i = 0; i < len(data); i++) {
        data[i] = (byte)((rune)' ' + i % ((rune)'~' - (rune)' '));
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
            if (e1 != default! || n != nwrite) {
                Ꮡt.Errorf("%s: buf.Write %d = %d, %v"u8, context, nwrite, n, e1);
                continue;
            }
            {
                var e = buf.Flush(); if (e != default!) {
                    Ꮡt.Errorf("%s: buf.Flush = %v"u8, context, e);
                }
            }
            var written = w.Bytes();
            if (len(written) != nwrite) {
                Ꮡt.Errorf("%s: %d bytes written"u8, context, len(written));
            }
            for (nint l = 0; l < len(written); l++) {
                if (written[l] != data[l]) {
                    Ꮡt.Errorf("wrong bytes written"u8);
                    Ꮡt.Errorf("want=%q"u8, data.slice(0, len(written)));
                    Ꮡt.Errorf("have=%q"u8, written);
                }
            }
        }
    }
}

public static void TestWriterAppend(ж<Δtesting.T> Ꮡt) {
    var got = @new<bytes.Buffer>();
    slice<byte> want = default!;
    var rn = rand.New(rand.NewSource(0));
    var w = NewWriterSize(new bufio_test_package.bytes_BufferжWriter(got), 64);
    for (nint i = 0; i < 100; i++) {
        // Obtain a buffer to append to.
        var b = w.AvailableBuffer();
        if (w.Available() != cap(b)) {
            Ꮡt.Fatalf("Available() = %v, want %v"u8, w.Available(), cap(b));
        }
        // While not recommended, it is valid to append to a shifted buffer.
        // This forces Write to copy the input.
        if (rn.Intn(8) == 0 && cap(b) > 0) {
            b = b.slice(1, 1, cap(b));
        }
        // Append a random integer of varying width.
        var n = (int64)rn.Intn(((nint)1).Lsh((int64)(rn.Intn(30))));
        want = append(strconv.AppendInt(want, n, 10), (byte)((rune)' '));
        b = append(strconv.AppendInt(b, n, 10), (byte)((rune)' '));
        w.Write(b);
    }
    w.Flush();
    if (!bytes.Equal(got.Bytes(), want)) {
        Ꮡt.Errorf("output mismatch:\ngot  %s\nwant %s"u8, got.Bytes(), want);
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

internal static slice<errorWriterTest> errorWriterTests = new errorWriterTest[]{
    new(0, 1, default!, Δio.ErrShortWrite),
    new(1, 2, default!, Δio.ErrShortWrite),
    new(1, 1, default!, default!),
    new(0, 1, Δio.ErrClosedPipe, Δio.ErrClosedPipe),
    new(1, 2, Δio.ErrClosedPipe, Δio.ErrClosedPipe),
    new(1, 1, Δio.ErrClosedPipe, Δio.ErrClosedPipe)
}.slice();

public static void TestWriteErrors(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, w) in errorWriterTests) {
        var buf = NewWriter(w);
        var (_, e) = buf.Write(slice<byte>("hello world"u8));
        if (e != default!) {
            Ꮡt.Errorf("Write hello to %v: %v"u8, w, e);
            continue;
        }
        // Two flushes, to verify the error is sticky.
        for (nint i = 0; i < 2; i++) {
            e = buf.Flush();
            if (!AreEqual(e, w.expect)) {
                Ꮡt.Errorf("Flush %d/2 %v: got %v, wanted %v"u8, i + 1, w, e, w.expect);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object newReaderSizeDidNotˢ = (@string)"NewReaderSize did not detect underlying Reader"u8;
internal static readonly object newReaderSizeDidNotˢ2 = (@string)"NewReaderSize did not enlarge buffer"u8;

public static void TestNewReaderSizeIdempotent(ж<Δtesting.T> Ꮡt) {
    UntypedInt BufSize = 1000;
    var b = NewReaderSize(new bufio_test_package.strings_ReaderжReader(strings.NewReader(helloWorldˢ)), BufSize);
    // Does it recognize itself?
    var b1 = NewReaderSize(new bufio_test_package.bufio_ReaderжReader(b), BufSize);
    if (b1 != b) {
        Ꮡt.Error(newReaderSizeDidNotˢ);
    }
    // Does it wrap if existing buffer is too small?
    var b2 = NewReaderSize(new bufio_test_package.bufio_ReaderжReader(b), 2 * BufSize);
    if (b2 == b) {
        Ꮡt.Error(newReaderSizeDidNotˢ2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object newWriterSizeDidNotˢ = (@string)"NewWriterSize did not detect underlying Writer"u8;
internal static readonly object newWriterSizeDidNotˢ2 = (@string)"NewWriterSize did not enlarge buffer"u8;

public static void TestNewWriterSizeIdempotent(ж<Δtesting.T> Ꮡt) {
    UntypedInt BufSize = 1000;
    var b = NewWriterSize(new bufio_test_package.bytes_BufferжWriter(@new<bytes.Buffer>()), BufSize);
    // Does it recognize itself?
    var b1 = NewWriterSize(new bufio_test_package.bufio_WriterжWriter(b), BufSize);
    if (b1 != b) {
        Ꮡt.Error(newWriterSizeDidNotˢ);
    }
    // Does it wrap if existing buffer is too small?
    var b2 = NewWriterSize(new bufio_test_package.bufio_WriterжWriter(b), 2 * BufSize);
    if (b2 == b) {
        Ꮡt.Error(newWriterSizeDidNotˢ2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object writeStringˢ = (@string)"WriteString"u8;

public static void TestWriteString(ж<Δtesting.T> Ꮡt) {
    const nint BufSize = 8;
    var buf = @new<strings.Builder>();
    var b = NewWriterSize(new bufio_test_package.strings_BuilderжWriter(buf), BufSize);
    b.WriteString("0"u8); // easy
    b.WriteString("123456"u8); // still easy
    b.WriteString("7890"u8); // easy after flush
    b.WriteString("abcdefghijklmnopqrstuvwxy"u8); // hard
    b.WriteString("z"u8);
    {
        var err = b.Flush(); if (err != default!) {
            Ꮡt.Error(writeStringˢ, err);
        }
    }
    @string s = "01234567890abcdefghijklmnopqrstuvwxyz"u8;
    if (buf.String() != s) {
        Ꮡt.Errorf("WriteString wants %q gets %q"u8, s, buf.String());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcˢ = "abc"u8;
internal static readonly @string abc12345ˢ = "abc12345"u8;

public static void TestWriteStringStringWriter(ж<Δtesting.T> Ꮡt) {
    const nint BufSize = 8;
    {
        var tw = Ꮡ(new teststringwriter(nil));
        var b = NewWriterSize(new bufio_test_package.teststringwriterжWriter(tw), BufSize);
        b.WriteString("1234"u8);
        tw.check(Ꮡt, ""u8, ""u8);
        b.WriteString("56789012"u8); // longer than BufSize
        tw.check(Ꮡt, "12345678"u8, ""u8); // but not enough (after filling the partially-filled buffer)
        b.Flush();
        tw.check(Ꮡt, "123456789012"u8, ""u8);
    }
    {
        var tw = Ꮡ(new teststringwriter(nil));
        var b = NewWriterSize(new bufio_test_package.teststringwriterжWriter(tw), BufSize);
        b.WriteString("123456789"u8); // long string, empty buffer:
        tw.check(Ꮡt, ""u8, "123456789"u8); // use WriteString
    }
    {
        var tw = Ꮡ(new teststringwriter(nil));
        var b = NewWriterSize(new bufio_test_package.teststringwriterжWriter(tw), BufSize);
        b.WriteString(abcˢ);
        tw.check(Ꮡt, ""u8, ""u8);
        b.WriteString("123456789012345"u8); // long string, non-empty buffer
        tw.check(Ꮡt, abc12345ˢ, "6789012345"u8); // use Write and then WriteString since the remaining part is still longer than BufSize
    }
    {
        var tw = Ꮡ(new teststringwriter(nil));
        var b = NewWriterSize(new bufio_test_package.teststringwriterжWriter(tw), BufSize);
        b.Write(slice<byte>("abc"u8)); // same as above, but use Write instead of WriteString
        tw.check(Ꮡt, ""u8, ""u8);
        b.WriteString("123456789012345"u8);
        tw.check(Ꮡt, abc12345ˢ, "6789012345"u8); // same as above
    }
}

partial struct teststringwriter {
    internal @string write;
    internal @string writeString;
}

internal static (nint, error) Write(this ref teststringwriter w, slice<byte> b) {
    w.write += ((@string)b);
    return (len(b), default!);
}

internal static (nint, error) WriteString(this ref teststringwriter w, @string s) {
    w.writeString += s;
    return (len(s), default!);
}

internal static void check(this ref teststringwriter w, ж<Δtesting.T> Ꮡt, @string write, @string writeString) {
    Ꮡt.Helper();
    if (w.write != write) {
        Ꮡt.Errorf("write: expected %q, got %q"u8, write, w.write);
    }
    if (w.writeString != writeString) {
        Ꮡt.Errorf("writeString: expected %q, got %q"u8, writeString, w.writeString);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string longStringᶜ = "And now, hello, world! It is the time for all good men to come to the aid of their party"u8;

public static void TestBufferFull(ж<Δtesting.T> Ꮡt) {
    @string longString = longStringᶜ;
    var buf = NewReaderSize(new bufio_test_package.strings_ReaderжReader(strings.NewReader(longString)), minReadBufferSize);
    var (line, err) = buf.ReadSlice((rune)'!');
    if (((sstring)line) != "And now, hello, "u8 || !AreEqual(err, ErrBufferFull)) {
        Ꮡt.Errorf("first ReadSlice(,) = %q, %v"u8, line, err);
    }
    (line, err) = buf.ReadSlice((rune)'!');
    if (((sstring)line) != "world!"u8 || err != default!) {
        Ꮡt.Errorf("second ReadSlice(,) = %q, %v"u8, line, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcdefghijklmnopˢ = "abcdefghijklmnop"u8;
internal static readonly @string defˢ = "def"u8;
internal static readonly object ghijˢ = (@string)"ghij"u8;
internal static readonly object ghijklmnopˢ = (@string)"ghijklmnop"u8;

public static void TestPeek(ж<Δtesting.T> Ꮡt) {
    var p = new slice<byte>(10);
    // string is 16 (minReadBufferSize) long.
    var buf = NewReaderSize(new bufio_test_package.strings_ReaderжReader(strings.NewReader(abcdefghijklmnopˢ)), minReadBufferSize);
    {
        var (s, err) = buf.Peek(1); if (((sstring)s) != "a"u8 || err != default!) {
            Ꮡt.Fatalf("want %q got %q, err=%v"u8, (@string)"a"u8, ((@string)s), err);
        }
    }
    {
        var (s, err) = buf.Peek(4); if (((sstring)s) != "abcd"u8 || err != default!) {
            Ꮡt.Fatalf("want %q got %q, err=%v"u8, abcdˢ, ((@string)s), err);
        }
    }
    {
        var (_, err) = buf.Peek(-1); if (!AreEqual(err, ErrNegativeCount)) {
            Ꮡt.Fatalf("want ErrNegativeCount got %v"u8, err);
        }
    }
    {
        var (s, err) = buf.Peek(32); if (((sstring)s) != "abcdefghijklmnop"u8 || !AreEqual(err, ErrBufferFull)) {
            Ꮡt.Fatalf("want %q, ErrBufFull got %q, err=%v"u8, abcdefghijklmnopˢ, ((@string)s), err);
        }
    }
    {
        var (_, err) = buf.Read(p[0..3]); if (((sstring)(p[0..3])) != "abc"u8 || err != default!) {
            Ꮡt.Fatalf("want %q got %q, err=%v"u8, abcˢ, ((@string)(p[0..3])), err);
        }
    }
    {
        var (s, err) = buf.Peek(1); if (((sstring)s) != "d"u8 || err != default!) {
            Ꮡt.Fatalf("want %q got %q, err=%v"u8, (@string)"d"u8, ((@string)s), err);
        }
    }
    {
        var (s, err) = buf.Peek(2); if (((sstring)s) != "de"u8 || err != default!) {
            Ꮡt.Fatalf("want %q got %q, err=%v"u8, (@string)"de"u8, ((@string)s), err);
        }
    }
    {
        var (_, err) = buf.Read(p[0..3]); if (((sstring)(p[0..3])) != "def"u8 || err != default!) {
            Ꮡt.Fatalf("want %q got %q, err=%v"u8, defˢ, ((@string)(p[0..3])), err);
        }
    }
    {
        var (s, err) = buf.Peek(4); if (((sstring)s) != "ghij"u8 || err != default!) {
            Ꮡt.Fatalf("want %q got %q, err=%v"u8, ghijˢ, ((@string)s), err);
        }
    }
    {
        var (_, err) = buf.Read(p[0..]); if (((sstring)(p[0..])) != "ghijklmnop"u8 || err != default!) {
            Ꮡt.Fatalf("want %q got %q, err=%v"u8, ghijklmnopˢ, ((@string)(p[0..(int)(minReadBufferSize)])), err);
        }
    }
    {
        var (s, err) = buf.Peek(0); if (((sstring)s) != ""u8 || err != default!) {
            Ꮡt.Fatalf("want %q got %q, err=%v"u8, (@string)""u8, ((@string)s), err);
        }
    }
    {
        var (_, err) = buf.Peek(1); if (!AreEqual(err, Δio.EOF)) {
            Ꮡt.Fatalf("want EOF got %v"u8, err);
        }
    }
    // Test for issue 3022, not exposing a reader's error on a successful Peek.
    buf = NewReaderSize(((dataAndEOFReader)(@string)abcdˢ), 32);
    {
        var (s, err) = buf.Peek(2); if (((sstring)s) != "ab"u8 || err != default!) {
            Ꮡt.Errorf(@"Peek(2) on ""abcd"", EOF = %q, %v; want ""ab"", nil"u8, ((@string)s), err);
        }
    }
    {
        var (s, err) = buf.Peek(4); if (((sstring)s) != "abcd"u8 || err != default!) {
            Ꮡt.Errorf(@"Peek(4) on ""abcd"", EOF = %q, %v; want ""abcd"", nil"u8, ((@string)s), err);
        }
    }
    {
        var (n, err) = buf.Read(p[0..5]); if (((sstring)(p.slice(0, n))) != "abcd"u8 || err != default!) {
            Ꮡt.Fatalf("Read after peek = %q, %v; want abcd, EOF"u8, p.slice(0, n), err);
        }
    }
    {
        var (n, err) = buf.Read(p[0..1]); if (((sstring)(p.slice(0, n))) != ""u8 || !AreEqual(err, Δio.EOF)) {
            Ꮡt.Fatalf(@"second Read after peek = %q, %v; want """", EOF"u8, p.slice(0, n), err);
        }
    }
}

partial struct dataAndEOFReader /*@string*/;

internal static (nint, error) Read(this dataAndEOFReader r, slice<byte> p) {
    return (copy(p, r), Δio.EOF);
}

public static void TestPeekThenUnreadRune(ж<Δtesting.T> Ꮡt) {
    // This sequence used to cause a crash.
    var r = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader("x"u8)));
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
    nint n = default!;
    error err = default!;

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

internal static void testReadLine(ж<Δtesting.T> Ꮡt, slice<byte> input) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (nint stride = 1; stride < 2; stride++) {
        nint done = 0;
        ref var reader = ref heap<testReader>(out var Ꮡreader);
        reader = new testReader(input, stride);
        var l = NewReaderSize(new bufio_test_package.testReaderжReader(Ꮡreader), len(input) + 1);
        while (ᐧ) {
            var (line, isPrefix, err) = l.ReadLine();
            if (len(line) > 0 && err != default!) {
                Ꮡt.Errorf("ReadLine returned both data and error: %s"u8, err);
            }
            if (isPrefix) {
                Ꮡt.Errorf("ReadLine returned prefix"u8);
            }
            if (err != default!) {
                if (!AreEqual(err, Δio.EOF)) {
                    Ꮡt.Fatalf("Got unknown error: %s"u8, err);
                }
                break;
            }
            {
                var want = testOutput.slice(done, done + len(line)); if (!bytes.Equal(want, line)) {
                    Ꮡt.Errorf("Bad line at stride %d: want: %x got: %x"u8, stride, want, line);
                }
            }
            done += len(line);
        }
        if (done != len(testOutput)) {
            Ꮡt.Errorf("ReadLine didn't return everything: got: %d, want: %d (stride: %d)"u8, done, len(testOutput), stride);
        }
    }
}

public static void TestReadLine(ж<Δtesting.T> Ꮡt) {
    testReadLine(Ꮡt, testInput);
    testReadLine(Ꮡt, testInputrn);
}

public static void TestLineTooLong(ж<Δtesting.T> Ꮡt) {
    var data = new slice<byte>(0);
    for (nint i = 0; i < (nint)(minReadBufferSize * 5 / 2); i++) {
        data = append(data, (byte)((rune)'0' + (byte)(i % 10)));
    }
    var buf = bytes.NewReader(data);
    var l = NewReaderSize(new bufio_test_package.bytes_ReaderжReader(buf), minReadBufferSize);
    var (line, isPrefix, err) = l.ReadLine();
    if (!isPrefix || !bytes.Equal(line, data[..(int)(minReadBufferSize)]) || err != default!) {
        Ꮡt.Errorf("bad result for first line: got %q want %q %v"u8, line, data[..(int)(minReadBufferSize)], err);
    }
    data = data.slice(len(line));
    (line, isPrefix, err) = l.ReadLine();
    if (!isPrefix || !bytes.Equal(line, data[..(int)(minReadBufferSize)]) || err != default!) {
        Ꮡt.Errorf("bad result for second line: got %q want %q %v"u8, line, data[..(int)(minReadBufferSize)], err);
    }
    data = data.slice(len(line));
    (line, isPrefix, err) = l.ReadLine();
    if (isPrefix || !bytes.Equal(line, data[..(int)(minReadBufferSize / 2)]) || err != default!) {
        Ꮡt.Errorf("bad result for third line: got %q want %q %v"u8, line, data[..(int)(minReadBufferSize / 2)], err);
    }
    (line, isPrefix, err) = l.ReadLine();
    if (isPrefix || err == default!) {
        Ꮡt.Errorf("expected no more lines: %x %s"u8, line, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string thisIsLine1ˢ = "this is line1"u8;
internal static readonly @string thisIsLine2ThisIsLine3ˢ = "this is line2\nthis is line 3\n"u8;

public static void TestReadAfterLines(ж<Δtesting.T> Ꮡt) {
    @string line1 = thisIsLine1ˢ;
    @string restData = thisIsLine2ThisIsLine3ˢ;
    var inbuf = bytes.NewReader(slice<byte>(line1 + "\n" + restData));
    var outbuf = @new<strings.Builder>();
    nint maxLineLength = len(line1) + len(restData) / 2;
    var l = NewReaderSize(new bufio_test_package.bytes_ReaderжReader(inbuf), maxLineLength);
    var (line, isPrefix, err) = l.ReadLine();
    if (isPrefix || err != default! || ((sstring)line) != line1) {
        Ꮡt.Errorf("bad result for first line: isPrefix=%v err=%v line=%q"u8, isPrefix, err, ((@string)line));
    }
    (var n, err) = Δio.Copy(new bufio_test_package.strings_BuilderжWriter(outbuf), new bufio_test_package.bufio_ReaderжReader(l));
    if ((nint)n != len(restData) || err != default!) {
        Ꮡt.Errorf("bad result for Read: n=%d err=%v"u8, n, err);
    }
    if (outbuf.String() != restData) {
        Ꮡt.Errorf("bad result for Read: got %q; expected %q"u8, outbuf.String(), restData);
    }
}

public static void TestReadEmptyBuffer(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var l = NewReaderSize(new bufio_test_package.bytes_BufferжReader(@new<bytes.Buffer>()), minReadBufferSize);
    var (line, isPrefix, err) = l.ReadLine();
    if (!AreEqual(err, Δio.EOF)) {
        Ꮡt.Errorf("expected EOF from ReadLine, got '%s' %t %s"u8, line, isPrefix, err);
    }
}

public static void TestLinesAfterRead(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var l = NewReaderSize(new bufio_test_package.bytes_ReaderжReader(bytes.NewReader(slice<byte>("foo"u8))), minReadBufferSize);
    var (_, err) = Δio.ReadAll(new bufio_test_package.bufio_ReaderжReader(l));
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    (var line, var isPrefix, err) = l.ReadLine();
    if (!AreEqual(err, Δio.EOF)) {
        Ꮡt.Errorf("expected EOF from ReadLine, got '%s' %t %s"u8, line, isPrefix, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string line1ˢ = "line 1\n"u8;

public static void TestReadLineNonNilLineOrError(ж<Δtesting.T> Ꮡt) {
    var r = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(line1ˢ)));
    for (nint i = 0; i < 2; i++) {
        var (l, _, err) = r.ReadLine();
        if (l != default! && err != default!) {
            Ꮡt.Fatalf("on line %d/2; ReadLine=%#v, %v; want non-nil line or Error, but not both"u8,
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
internal static slice<readLineNewlinesTestsᴛ1> readLineNewlinesTests = new readLineNewlinesTestsᴛ1[]{
    new("012345678901234\r\n012345678901234\r\n"u8, new readLineResult[]{
        new(slice<byte>("012345678901234"u8), true, default!),
        new(default!, false, default!),
        new(slice<byte>("012345678901234"u8), true, default!),
        new(default!, false, default!),
        new(default!, false, Δio.EOF)
    }.slice()),
    new("0123456789012345\r012345678901234\r"u8, new readLineResult[]{
        new(slice<byte>("0123456789012345"u8), true, default!),
        new(slice<byte>("\r012345678901234"u8), true, default!),
        new(slice<byte>("\r"u8), false, default!),
        new(default!, false, Δio.EOF)
    }.slice())
}.slice();

public static void TestReadLineNewlines(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, e) in readLineNewlinesTests) {
        testReadLineNewlines(Ꮡt, e.input, e.expect);
    }
}

internal static void testReadLineNewlines(ж<Δtesting.T> Ꮡt, @string input, slice<readLineResult> expect) {
    var b = NewReaderSize(new bufio_test_package.strings_ReaderжReader(strings.NewReader(input)), minReadBufferSize);
    foreach (var (i, e) in expect) {
        var (line, isPrefix, err) = b.ReadLine();
        if (!bytes.Equal(line, e.line)) {
            Ꮡt.Errorf("%q call %d, line == %q, want %q"u8, input, i, line, e.line);
            return;
        }
        if (isPrefix != e.isPrefix) {
            Ꮡt.Errorf("%q call %d, isPrefix == %v, want %v"u8, input, i, isPrefix, e.isPrefix);
            return;
        }
        if (!AreEqual(err, e.err)) {
            Ꮡt.Errorf("%q call %d, err == %v, want %v"u8, input, i, err, e.err);
            return;
        }
    }
}

internal static slice<byte> createTestInput(nint n) {
    var input = new slice<byte>(n);
    foreach (var (i, _) in input) {
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

public static void TestReaderWriteTo(ж<Δtesting.T> Ꮡt) {
    var input = createTestInput(8192);
    var r = NewReader(new onlyReader(new bufio_test_package.bytes_ReaderжReader(bytes.NewReader(input))));
    var w = @new<bytes.Buffer>();
    {
        var (n, err) = r.WriteTo(new bufio_test_package.bytes_BufferжWriter(w)); if (err != default! || n != (int64)len(input)) {
            Ꮡt.Fatalf("r.WriteTo(w) = %d, %v, want %d, nil"u8, n, err, len(input));
        }
    }
    foreach (var (i, val) in w.Bytes()) {
        if (val != input[i]) {
            Ꮡt.Errorf("after write: out[%d] = %#x, want %#x"u8, i, val, input[i]);
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

internal static slice<errorWriterToTest> errorWriterToTests = new errorWriterToTest[]{
    new(1, 0, default!, Δio.ErrClosedPipe, Δio.ErrClosedPipe),
    new(0, 1, Δio.ErrClosedPipe, default!, Δio.ErrClosedPipe),
    new(0, 0, Δio.ErrUnexpectedEOF, Δio.ErrClosedPipe, Δio.ErrClosedPipe),
    new(0, 1, Δio.EOF, default!, default!)
}.slice();

public static void TestReaderWriteToErrors(ж<Δtesting.T> Ꮡt) {
    foreach (var (i, rw) in errorWriterToTests) {
        var r = NewReader(rw);
        {
            var (_, err) = r.WriteTo(rw); if (!AreEqual(err, rw.expected)) {
                Ꮡt.Errorf("r.WriteTo(errorWriterToTests[%d]) = _, %v, want _,%v"u8, i, err, rw.expected);
            }
        }
    }
}

public static void TestWriterReadFrom(ж<Δtesting.T> Ꮡt) {
    var ws = new Func<Δio.Writer, Δio.Writer>[]{
        (Δio.Writer w) => new onlyWriter(w),
        (Δio.Writer w) => w
    }.slice();
    var rs = new Func<Δio.Reader, Δio.Reader>[]{
        iotest.DataErrReader,
        (Δio.Reader r) => r
    }.slice();
    foreach (var (ri, rfunc) in rs) {
        foreach (var (wi, wfunc) in ws) {
            var input = createTestInput(8192);
            var b = @new<strings.Builder>();
            var w = NewWriter(wfunc(new bufio_test_package.strings_BuilderжWriter(b)));
            var r = rfunc(new bufio_test_package.bytes_ReaderжReader(bytes.NewReader(input)));
            {
                var (n, err) = w.ReadFrom(r); if (err != default! || n != (int64)len(input)) {
                    Ꮡt.Errorf("ws[%d],rs[%d]: w.ReadFrom(r) = %d, %v, want %d, nil"u8, wi, ri, n, err, len(input));
                    continue;
                }
            }
            {
                var err = w.Flush(); if (err != default!) {
                    Ꮡt.Errorf("Flush returned %v"u8, err);
                    continue;
                }
            }
            {
                @string got = b.String();
                @string want = ((@string)input); if (got != want) {
                    Ꮡt.Errorf("ws[%d], rs[%d]:\ngot  %q\nwant %q\n"u8, wi, ri, got, want);
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

internal static slice<errorReaderFromTest> errorReaderFromTests = new errorReaderFromTest[]{
    new(0, 1, Δio.EOF, default!, default!),
    new(1, 1, Δio.EOF, default!, default!),
    new(0, 1, Δio.ErrClosedPipe, default!, Δio.ErrClosedPipe),
    new(0, 0, Δio.ErrClosedPipe, Δio.ErrShortWrite, Δio.ErrClosedPipe),
    new(1, 0, default!, Δio.ErrShortWrite, Δio.ErrShortWrite)
}.slice();

public static void TestWriterReadFromErrors(ж<Δtesting.T> Ꮡt) {
    foreach (var (i, rw) in errorReaderFromTests) {
        var w = NewWriter(rw);
        {
            var (_, err) = w.ReadFrom(rw); if (!AreEqual(err, rw.expected)) {
                Ꮡt.Errorf("w.ReadFrom(errorReaderFromTests[%d]) = _, %v, want _,%v"u8, i, err, rw.expected);
            }
        }
    }
}

// TestWriterReadFromCounts tests that using io.Copy to copy into a
// bufio.Writer does not prematurely flush the buffer. For example, when
// buffering writes to a network socket, excessive network writes should be
// avoided.
public static void TestWriterReadFromCounts(ж<Δtesting.T> Ꮡt) {
    ref var w0 = ref heap(new writeCountingDiscard(), out var Ꮡw0);
    var b0 = NewWriterSize(new bufio_test_package.writeCountingDiscardжWriter(Ꮡw0), 1234);
    b0.WriteString(strings.Repeat("x"u8, 1000));
    if (w0 != 0) {
        Ꮡt.Fatalf("write 1000 'x's: got %d writes, want 0"u8, w0);
    }
    b0.WriteString(strings.Repeat("x"u8, 200));
    if (w0 != 0) {
        Ꮡt.Fatalf("write 1200 'x's: got %d writes, want 0"u8, w0);
    }
    Δio.Copy(new bufio_test_package.bufio_WriterжWriter(b0), new onlyReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(strings.Repeat("x"u8, 30)))));
    if (w0 != 0) {
        Ꮡt.Fatalf("write 1230 'x's: got %d writes, want 0"u8, w0);
    }
    Δio.Copy(new bufio_test_package.bufio_WriterжWriter(b0), new onlyReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(strings.Repeat("x"u8, 9)))));
    if (w0 != 1) {
        Ꮡt.Fatalf("write 1239 'x's: got %d writes, want 1"u8, w0);
    }
    ref var w1 = ref heap(new writeCountingDiscard(), out var Ꮡw1);
    var b1 = NewWriterSize(new bufio_test_package.writeCountingDiscardжWriter(Ꮡw1), 1234);
    b1.WriteString(strings.Repeat("x"u8, 1200));
    b1.Flush();
    if (w1 != 1) {
        Ꮡt.Fatalf("flush 1200 'x's: got %d writes, want 1"u8, w1);
    }
    b1.WriteString(strings.Repeat("x"u8, 89));
    if (w1 != 1) {
        Ꮡt.Fatalf("write 1200 + 89 'x's: got %d writes, want 1"u8, w1);
    }
    Δio.Copy(new bufio_test_package.bufio_WriterжWriter(b1), new onlyReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(strings.Repeat("x"u8, 700)))));
    if (w1 != 1) {
        Ꮡt.Fatalf("write 1200 + 789 'x's: got %d writes, want 1"u8, w1);
    }
    Δio.Copy(new bufio_test_package.bufio_WriterжWriter(b1), new onlyReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(strings.Repeat("x"u8, 600)))));
    if (w1 != 2) {
        Ꮡt.Fatalf("write 1200 + 1389 'x's: got %d writes, want 2"u8, w1);
    }
    b1.Flush();
    if (w1 != 3) {
        Ꮡt.Fatalf("flush 1200 + 1389 'x's: got %d writes, want 3"u8, w1);
    }
}

partial struct writeCountingDiscard /*num:nint*/;

internal static (nint, error) Write(this ref writeCountingDiscard w, slice<byte> p) {
    w++;
    return (len(p), default!);
}

partial struct negativeReader /*num:nint*/;

internal static (nint, error) Read(this ref negativeReader r, slice<byte> _) {
    return (-1, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readDidNotPanicˢ = (@string)"read did not panic"u8;
internal static readonly @string readerReturnedNegativeˢ = "reader returned negative count from Read"u8;

public static void TestNegativeRead(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // should panic with a description pointing at the reader, not at itself.
        // (should NOT panic with slice index error, for example.)
        var b = NewReader(new bufio_test_package.negativeReaderжReader(@new<negativeReader>()));
        defer(() => {
            var switchᴛ1 = recover();
            switch (switchᴛ1.type()) {
            case null: {
                Ꮡt.Fatal(readDidNotPanicˢ);
                break;
            }
            case {} Δerr when Δerr._<error>(out var err): {
                if (!strings.Contains(err.Error(), readerReturnedNegativeˢ)) {
                    Ꮡt.Fatalf("wrong panic: %v"u8, err);
                }
                break;
            }
            default: {
                var err = switchᴛ1;
                Ꮡt.Fatalf("unexpected panic value: %T(%v)"u8, err, err);
                break;
            }}
        }, ref ᒐ);
        b.Read(new slice<byte>(100));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static error errFake = errors.New("fake error"u8);

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
    return (len(p), default!);
}

public static void TestReaderClearError(ж<Δtesting.T> Ꮡt) {
    var r = Ꮡ(new errorThenGoodReader(nil));
    var b = NewReader(new bufio_test_package.errorThenGoodReaderжReader(r));
    var buf = new slice<byte>(1);
    {
        var (_, err) = b.Read(default!); if (err != default!) {
            Ꮡt.Fatalf("1st nil Read = %v; want nil"u8, err);
        }
    }
    {
        var (_, err) = b.Read(buf); if (!AreEqual(err, errFake)) {
            Ꮡt.Fatalf("1st Read = %v; want errFake"u8, err);
        }
    }
    {
        var (_, err) = b.Read(default!); if (err != default!) {
            Ꮡt.Fatalf("2nd nil Read = %v; want nil"u8, err);
        }
    }
    {
        var (_, err) = b.Read(buf); if (err != default!) {
            Ꮡt.Fatalf("3rd Read with buffer = %v; want nil"u8, err);
        }
    }
    if ((~r).nread != 2) {
        Ꮡt.Errorf("num reads = %d; want 2"u8, (~r).nread);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcdefˢ = "abcdef"u8;

// Test for golang.org/issue/5947
public static void TestWriterReadFromWhileFull(ж<Δtesting.T> Ꮡt) {
    var buf = @new<bytes.Buffer>();
    var w = NewWriterSize(new bufio_test_package.bytes_BufferжWriter(buf), 10);
    // Fill buffer exactly.
    var (n, err) = w.Write(slice<byte>("0123456789"u8));
    if (n != 10 || err != default!) {
        Ꮡt.Fatalf("Write returned (%v, %v), want (10, nil)"u8, n, err);
    }
    // Use ReadFrom to read in some data.
    (var n2, err) = w.ReadFrom(new bufio_test_package.strings_ReaderжReader(strings.NewReader(abcdefˢ)));
    if (n2 != 6 || err != default!) {
        Ꮡt.Fatalf("ReadFrom returned (%v, %v), want (6, nil)"u8, n2, err);
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
    return (0, default!);
}

// Test for golang.org/issue/7611
public static void TestWriterReadFromUntilEOF(ж<Δtesting.T> Ꮡt) {
    var buf = @new<bytes.Buffer>();
    var w = NewWriterSize(new bufio_test_package.bytes_BufferжWriter(buf), 5);
    // Partially fill buffer
    var (n, err) = w.Write(slice<byte>("0123"u8));
    if (n != 4 || err != default!) {
        Ꮡt.Fatalf("Write returned (%v, %v), want (4, nil)"u8, n, err);
    }
    // Use ReadFrom to read in some data.
    var r = Ꮡ(new emptyThenNonEmptyReader(r: new bufio_test_package.strings_ReaderжReader(strings.NewReader(abcdˢ)), n: 3));
    (var n2, err) = w.ReadFrom(new bufio_test_package.emptyThenNonEmptyReaderжReader(r));
    if (n2 != 4 || err != default!) {
        Ꮡt.Fatalf("ReadFrom returned (%v, %v), want (4, nil)"u8, n2, err);
    }
    w.Flush();
    {
        @string got = buf.String();
        @string want = "0123abcd"u8; if (got != want) {
            Ꮡt.Fatalf("buf.Bytes() returned %q, want %q"u8, got, want);
        }
    }
}

public static void TestWriterReadFromErrNoProgress(ж<Δtesting.T> Ꮡt) {
    var buf = @new<bytes.Buffer>();
    var w = NewWriterSize(new bufio_test_package.bytes_BufferжWriter(buf), 5);
    // Partially fill buffer
    var (n, err) = w.Write(slice<byte>("0123"u8));
    if (n != 4 || err != default!) {
        Ꮡt.Fatalf("Write returned (%v, %v), want (4, nil)"u8, n, err);
    }
    // Use ReadFrom to read in some data.
    var r = Ꮡ(new emptyThenNonEmptyReader(r: new bufio_test_package.strings_ReaderжReader(strings.NewReader(abcdˢ)), n: 100));
    (var n2, err) = w.ReadFrom(new bufio_test_package.emptyThenNonEmptyReaderжReader(r));
    if (n2 != 0 || !AreEqual(err, Δio.ErrNoProgress)) {
        Ꮡt.Fatalf("buf.Bytes() returned (%v, %v), want (0, io.ErrNoProgress)"u8, n2, err);
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
    return (len(p), default!);
}

internal static (int64, error) ReadFrom(this ref readFromWriter w, Δio.Reader r) {
    var (b, err) = Δio.ReadAll(r);
    w.buf = appendꓸꓸꓸ(w.buf, b);
    w.readFromBytes += len(b);
    return ((int64)len(b), err);
}

// Test that calling (*Writer).ReadFrom with a partially-filled buffer
// fills the buffer before switching over to ReadFrom.
public static void TestWriterReadFromWithBufferedData(ж<Δtesting.T> Ꮡt) {
    const nint bufsize = 16;
    var input = createTestInput(64);
    var rfw = Ꮡ(new readFromWriter(nil));
    var w = NewWriterSize(new bufio_test_package.readFromWriterжWriter(rfw), bufsize);
    const nint writeSize = 8;
    {
        var (nΔ1, errΔ1) = w.Write(input[..(int)(writeSize)]); if (nΔ1 != writeSize || errΔ1 != default!) {
            Ꮡt.Errorf("w.Write(%v bytes) = %v, %v; want %v, nil"u8, (nint)(writeSize), nΔ1, errΔ1, (nint)(writeSize));
        }
    }
    var (n, err) = w.ReadFrom(new bufio_test_package.bytes_ReaderжReader(bytes.NewReader(input[(int)(writeSize)..])));
    {
        nint wantn = len(input[(int)(writeSize)..]); if ((nint)n != wantn || err != default!) {
            Ꮡt.Errorf("io.Copy(w, %v bytes) = %v, %v; want %v, nil"u8, wantn, n, err, wantn);
        }
    }
    {
        var errΔ2 = w.Flush(); if (errΔ2 != default!) {
            Ꮡt.Errorf("w.Flush() = %v, want nil"u8, errΔ2);
        }
    }
    {
        nint got = rfw.Value.writeBytes;
        nint want = bufsize; if (got != want) {
            Ꮡt.Errorf("wrote %v bytes with Write, want %v"u8, got, want);
        }
    }
    {
        nint got = rfw.Value.readFromBytes;
        nint want = len(input) - bufsize; if (got != want) {
            Ꮡt.Errorf("wrote %v bytes with ReadFrom, want %v"u8, got, want);
        }
    }
}

public static void TestReadZero(ж<Δtesting.T> Ꮡt) {
    foreach (var (_, size) in new nint[]{100, 2}.slice()) {
        Ꮡt.Run(fmt.Sprintf("bufsize=%d"u8, size), (ж<Δtesting.T> tΔ1) => {
            var r = Δio.MultiReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(abcˢ)), new bufio_test_package.emptyThenNonEmptyReaderжReader(Ꮡ(new emptyThenNonEmptyReader(r: new bufio_test_package.strings_ReaderжReader(strings.NewReader(defˢ)), n: 1))));
            var br = NewReaderSize(r, size);
            var brʗ1 = br;
            void want(@string s, error wantErr) {
                var p = new slice<byte>(50);
                var (n, err) = brʗ1.Read(p);
                if (!AreEqual(err, wantErr) || n != len(s) || ((sstring)(p.slice(0, n))) != s) {
                    tΔ1.Fatalf("read(%d) = %q, %v, want %q, %v"u8, len(p), ((@string)(p.slice(0, n))), err, s, wantErr);
                }
                tΔ1.Logf("read(%d) = %q, %v"u8, len(p), ((@string)(p.slice(0, n))), err);
            }
            want(abcˢ, default!);
            want(""u8, default!);
            want(defˢ, default!);
            want(""u8, Δio.EOF);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooFooˢ = "foo foo"u8;
internal static readonly @string barBarˢ = "bar bar"u8;
internal static readonly @string recurˢ = "recur"u8;
internal static readonly @string recur2ˢ = "recur2"u8;

public static void TestReaderReset(ж<Δtesting.T> Ꮡt) {
    void checkAll(ж<bufio.Reader> rΔ1, @string want) {
        Ꮡt.Helper();
        var (all, err) = Δio.ReadAll(new bufio_test_package.bufio_ReaderжReader(rΔ1));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (((sstring)all) != want) {
            Ꮡt.Errorf("ReadAll returned %q, want %q"u8, all, want);
        }
    }
    var r = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(fooFooˢ)));
    var buf = new slice<byte>(3);
    r.Read(buf);
    if (((sstring)buf) != "foo"u8) {
        Ꮡt.Errorf("buf = %q; want foo"u8, buf);
    }
    r.Reset(new bufio_test_package.strings_ReaderжReader(strings.NewReader(barBarˢ)));
    checkAll(r, barBarˢ);
    r.Value = new Reader(nil); // zero out the Reader
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
internal static readonly @string fooˢ = "foo"u8;
internal static readonly @string barˢ = "bar"u8;

public static void TestWriterReset(ж<Δtesting.T> Ꮡt) {
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
        Ꮡt.Errorf("buf1 = %q; want empty"u8, buf1.String());
    }
    if (buf2.String() != "bar"u8) {
        Ꮡt.Errorf("buf2 = %q; want bar"u8, buf2.String());
    }
    w.Value = new Writer(nil); // zero out the Writer
    w.Reset(new bufio_test_package.strings_BuilderжWriter(Ꮡbuf3)); // and not flushed
    w.WriteString(barˢ);
    w.Flush();
    if (buf1.String() != ""u8) {
        Ꮡt.Errorf("buf1 = %q; want empty"u8, buf1.String());
    }
    if (buf3.String() != "bar"u8) {
        Ꮡt.Errorf("buf3 = %q; want bar"u8, buf3.String());
    }
    // Wrap a writer and then Reset to that writer.
    w.Reset(new bufio_test_package.strings_BuilderжWriter(Ꮡbuf4));
    var w2 = NewWriter(new bufio_test_package.bufio_WriterжWriter(w));
    w2.WriteString(recurˢ);
    w2.Flush();
    if (buf4.String() != "recur"u8) {
        Ꮡt.Errorf("buf4 = %q, want %q"u8, buf4.String(), recurˢ);
    }
    w.Reset(new bufio_test_package.strings_BuilderжWriter(Ꮡbuf5));
    w2.Reset(new bufio_test_package.bufio_WriterжWriter(w));
    w2.WriteString(recur2ˢ);
    w2.Flush();
    if (buf5.String() != "recur2"u8) {
        Ꮡt.Errorf("buf5 = %q, want %q"u8, buf5.String(), recur2ˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string thenErrorˢ = "5-then-error"u8;

internal partial struct TestReaderDiscard_tests /*dyn*/ {
    internal @string name;
    internal Δio.Reader r;
    internal nint bufSize; // 0 means 16
    internal nint peekSize;
    internal nint n; // input to Discard
    internal nint want;  // from Discard
    internal error wantErr; // from Discard
    internal nint wantBuffered;
}

public static void TestReaderDiscard(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestReaderDiscard_tests[]{
        new(
            name: "normal case"u8,
            r: new bufio_test_package.strings_ReaderжReader(strings.NewReader("abcdefghijklmnopqrstuvwxyz"u8)),
            peekSize: 16,
            n: 6,
            want: 6,
            wantBuffered: 10
        ),
        new(
            name: "discard causing read"u8,
            r: new bufio_test_package.strings_ReaderжReader(strings.NewReader("abcdefghijklmnopqrstuvwxyz"u8)),
            n: 6,
            want: 6,
            wantBuffered: 10
        ),
        new(
            name: "discard all without peek"u8,
            r: new bufio_test_package.strings_ReaderжReader(strings.NewReader("abcdefghijklmnopqrstuvwxyz"u8)),
            n: 26,
            want: 26,
            wantBuffered: 0
        ),
        new(
            name: "discard more than end"u8,
            r: new bufio_test_package.strings_ReaderжReader(strings.NewReader("abcdefghijklmnopqrstuvwxyz"u8)),
            n: 27,
            want: 26,
            wantErr: Δio.EOF,
            wantBuffered: 0
        ), // Any error from filling shouldn't show up until we
 // get past the valid bytes. Here we return 5 valid bytes at the same time
 // as an error, but test that we don't see the error from Discard.

        new(
            name: "fill error, discard less"u8,
            r: newScriptedReader((slice<byte> p) => {
                if (len(p) < 5) {
                    throw panic("unexpected small read");
                }
                return (5, errors.New(thenErrorˢ));
            }),
            n: 4,
            want: 4,
            wantErr: default!,
            wantBuffered: 1
        ),
        new(
            name: "fill error, discard equal"u8,
            r: newScriptedReader((slice<byte> p) => {
                if (len(p) < 5) {
                    throw panic("unexpected small read");
                }
                return (5, errors.New(thenErrorˢ));
            }),
            n: 5,
            want: 5,
            wantErr: default!,
            wantBuffered: 0
        ),
        new(
            name: "fill error, discard more"u8,
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
        ), // Discard of 0 shouldn't cause a read:

        new(
            name: "discard zero"u8,
            r: newScriptedReader(), // will panic on Read

            n: 0,
            want: 0,
            wantErr: default!,
            wantBuffered: 0
        ),
        new(
            name: "discard negative"u8,
            r: newScriptedReader(), // will panic on Read

            n: -1,
            want: 0,
            wantErr: ErrNegativeCount,
            wantBuffered: 0
        )
    }.slice();
    foreach (var (_, tt) in tests) {
        var br = NewReaderSize(tt.r, tt.bufSize);
        if (tt.peekSize > 0) {
            var (peekBuf, errΔ1) = br.Peek(tt.peekSize);
            if (errΔ1 != default!) {
                Ꮡt.Errorf("%s: Peek(%d): %v"u8, tt.name, tt.peekSize, errΔ1);
                continue;
            }
            if (len(peekBuf) != tt.peekSize) {
                Ꮡt.Errorf("%s: len(Peek(%d)) = %v; want %v"u8, tt.name, tt.peekSize, len(peekBuf), tt.peekSize);
                continue;
            }
        }
        var (discarded, err) = br.Discard(tt.n);
        {
            @string ge = fmt.Sprint(err);
            @string we = fmt.Sprint(tt.wantErr); if (discarded != tt.want || ge != we) {
                Ꮡt.Errorf("%s: Discard(%d) = (%v, %v); want (%v, %v)"u8, tt.name, tt.n, discarded, ge, tt.want, we);
                continue;
            }
        }
        {
            nint bn = br.Buffered(); if (bn != tt.wantBuffered) {
                Ꮡt.Errorf("%s: after Discard, Buffered = %d; want %d"u8, tt.name, bn, tt.wantBuffered);
            }
        }
    }
}

public static void TestReaderSize(ж<Δtesting.T> Ꮡt) {
    {
        nint got = NewReader(default!).Size();
        nint want = bufio_internal_test_package.DefaultBufSize; if (got != want) {
            Ꮡt.Errorf("NewReader's Reader.Size = %d; want %d"u8, got, want);
        }
    }
    {
        nint got = NewReaderSize(default!, 1234).Size();
        nint want = 1234; if (got != want) {
            Ꮡt.Errorf("NewReaderSize's Reader.Size = %d; want %d"u8, got, want);
        }
    }
}

public static void TestWriterSize(ж<Δtesting.T> Ꮡt) {
    {
        nint got = NewWriter(default!).Size();
        nint want = bufio_internal_test_package.DefaultBufSize; if (got != want) {
            Ꮡt.Errorf("NewWriter's Writer.Size = %d; want %d"u8, got, want);
        }
    }
    {
        nint got = NewWriterSize(default!, 1234).Size();
        nint want = 1234; if (got != want) {
            Ꮡt.Errorf("NewWriterSize's Writer.Size = %d; want %d"u8, got, want);
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

    ref var sr = ref heap<scriptedReader>(out var Ꮡsr);
    sr = ((scriptedReader)steps);
    return new bufio_test_package.scriptedReaderжReader(Ꮡsr);
}

// eofReader returns the number of bytes read and io.EOF for the read that consumes the last of the content.
partial struct eofReader {
    internal slice<byte> buf;
}

internal static (nint, error) Read(this ref eofReader r, slice<byte> p) {
    nint read = copy(p, r.buf);
    r.buf = r.buf.slice(read);
    var exprᴛ1 = read;
    if (exprᴛ1 == 0 || exprᴛ1 == len(r.buf)) {
        return (read, Δio.EOF);
    }

    // As allowed in the documentation, this will return io.EOF
    // in the same call that consumes the last of the data.
    // https://godoc.org/io#Reader
    return (read, default!);
}

public static void TestPartialReadEOF(ж<Δtesting.T> Ꮡt) {
    var src = new slice<byte>(10);
    var eofR = Ꮡ(new eofReader(buf: src));
    var r = NewReader(new bufio_test_package.eofReaderжReader(eofR));
    // Start by reading 5 of the 10 available bytes.
    var dest = new slice<byte>(5);
    var (read, err) = r.Read(dest);
    if (err != default!) {
        Ꮡt.Fatalf("unexpected error: %v"u8, err);
    }
    {
        nint n = len(dest); if (read != n) {
            Ꮡt.Fatalf("read %d bytes; wanted %d bytes"u8, read, n);
        }
    }
    // The Reader should have buffered all the content from the io.Reader.
    {
        nint n = len((~eofR).buf); if (n != 0) {
            Ꮡt.Fatalf("got %d bytes left in bufio.Reader source; want 0 bytes"u8, n);
        }
    }
    // To prove the point, check that there are still 5 bytes available to read.
    {
        nint n = r.Buffered(); if (n != 5) {
            Ꮡt.Fatalf("got %d bytes buffered in bufio.Reader; want 5 bytes"u8, n);
        }
    }
    // This is the second read of 0 bytes.
    (read, err) = r.Read(new byte[]{}.slice());
    if (err != default!) {
        Ꮡt.Fatalf("unexpected error: %v"u8, err);
    }
    if (read != 0) {
        Ꮡt.Fatalf("read %d bytes; want 0 bytes"u8, read);
    }
}

partial struct writerWithReadFromError {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string writerWithReadFromErrorˢ = "writerWithReadFromError error"u8;

internal static (int64, error) ReadFrom(this writerWithReadFromError w, Δio.Reader r) {
    return (0, errors.New(writerWithReadFromErrorˢ));
}

internal static (nint n, error err) Write(this writerWithReadFromError w, slice<byte> b) {
    return (10, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string test2ˢ = "test2"u8;
internal static readonly object expectedReadFromReturnsˢ = (@string)"expected ReadFrom returns error, got nil"u8;
internal static readonly object expectedWriteReturnsˢ = (@string)"expected Write returns error, got nil"u8;

public static void TestWriterReadFromMustSetUnderlyingError(ж<Δtesting.T> Ꮡt) {
    ж<bufio.Writer> wr = NewWriter(new writerWithReadFromError(nil));
    {
        var (_, err) = wr.ReadFrom(new bufio_test_package.strings_ReaderжReader(strings.NewReader(test2ˢ))); if (err == default!) {
            Ꮡt.Fatal(expectedReadFromReturnsˢ);
        }
    }
    {
        var (_, err) = wr.Write(slice<byte>("123"u8)); if (err == default!) {
            Ꮡt.Fatal(expectedWriteReturnsˢ);
        }
    }
}

partial struct writeErrorOnlyWriter {
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string writeErrorOnlyWriterˢ = "writeErrorOnlyWriter error"u8;

internal static (nint n, error err) Write(this writeErrorOnlyWriter w, slice<byte> p) {
    return (0, errors.New(writeErrorOnlyWriterˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string test1ˢ = "test1"u8;
internal static readonly object expectedFlushErrorGotNilˢ = (@string)"expected flush error, got nil"u8;
internal static readonly object expectedErrorGotNilˢ = (@string)"expected error, got nil"u8;

// Ensure that previous Write errors are immediately returned
// on any ReadFrom. See golang.org/issue/35194.
public static void TestWriterReadFromMustReturnUnderlyingError(ж<Δtesting.T> Ꮡt) {
    ж<bufio.Writer> wr = NewWriter(new writeErrorOnlyWriter(nil));
    @string s = test1ˢ;
    nint wantBuffered = len(s);
    {
        var (_, err) = wr.WriteString(s); if (err != default!) {
            Ꮡt.Fatalf("unexpected error: %v"u8, err);
        }
    }
    {
        var err = wr.Flush(); if (err == default!) {
            Ꮡt.Error(expectedFlushErrorGotNilˢ);
        }
    }
    {
        var (_, err) = wr.ReadFrom(new bufio_test_package.strings_ReaderжReader(strings.NewReader(test2ˢ))); if (err == default!) {
            Ꮡt.Fatal(expectedErrorGotNilˢ);
        }
    }
    {
        nint buffered = wr.Buffered(); if (buffered != wantBuffered) {
            Ꮡt.Fatalf("Buffered = %v; want %v"u8, buffered, wantBuffered);
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
    var srcReader = NewReaderSize(new onlyReader(new bufio_test_package.bytes_ReaderжReader(r)), (1 << (int)(10)));
    {
        var (_, ok) = Δio.Discard._<Δio.ReaderFrom>(ᐧ); if (!ok) {
            Ꮡb.Fatal(ioDiscardDoesnTSupportˢ);
        }
    }
    for (nint i = 0; i < b.N; i++) {
        r.Seek(0, Δio.SeekStart);
        srcReader.Reset(new onlyReader(new bufio_test_package.bytes_ReaderжReader(r)));
        var (n, err) = srcReader.WriteTo(Δio.Discard);
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        if (n != bufSize) {
            Ꮡb.Fatalf("n = %d; want %d"u8, n, (nint)(bufSize));
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
        var (_, err) = buf.ReadString((rune)'\n');
        if (err != default!) {
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
    @string str = strings.Repeat("x"u8, (16 << (int)(10)));
    for (nint i = 0; i < b.N; i++) {
        var br = NewReader(new bufio_test_package.strings_ReaderжReader(strings.NewReader(str)));
        var (n, err) = Δio.Copy(Δio.Discard, new bufio_test_package.bufio_ReaderжReader(br));
        if (err != default!) {
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
    @string str = strings.Repeat("x"u8, (1 << (int)(10)));
    var bs = slice<byte>(str);
    for (nint i = 0; i < b.N; i++) {
        var bw = NewWriter(Δio.Discard);
        bw.Flush();
        bw.WriteByte((rune)'a');
        bw.Flush();
        bw.WriteRune((rune)'B');
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
    @string str = strings.Repeat("x"u8, 50);
    for (nint i = 0; i < b.N; i++) {
        bw.WriteString(str);
        bw.Flush();
    }
}

} // end bufio_test_package
