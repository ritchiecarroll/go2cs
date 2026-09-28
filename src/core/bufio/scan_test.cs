// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using Δio = io_package;
using strings = strings_package;
using Δtesting = testing_package;
using Δunicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using bufio = bufio_package;
using go.unicode;
using static go.bufio_internal_test_package;

partial class bufio_test_package {

internal static UntypedInt smallMaxTokenSize => 256; // Much smaller for more efficient testing.

// Test white space table matches the Unicode definition.
public static void TestSpace(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    for (var r = (rune)0; r <= utf8.MaxRune; r++) {
        if (bufio_internal_test_package.IsSpace(r) != Δunicode.IsSpace(r)) {
            Ꮡt.Fatalf("white space property disagrees: %#U should be %t"u8, r, Δunicode.IsSpace(r));
        }
    }
}

// UTF-8 error
// correctly encoded RuneError
internal static slice<@string> scanTests = new @string[]{
    ""u8,
    "a"u8,
    "¼"u8,
    "☹"u8,
    ((@string)(new byte[]{0x81})),
    "\uFFFD"u8,
    "abcdefgh"u8,
    "abc def\n\t\tgh    "u8,
    ((@string)(new byte[]{0x61, 0x62, 0x63, 0xc2, 0xbc, 0xe2, 0x98, 0xb9, 0x81, 0xef, 0xbf, 0xbd, 0xe6, 0x97, 0xa5, 0xe6, 0x9c, 0xac, 0xe8, 0xaa, 0x9e, 0x82, 0x61, 0x62, 0x63}))
}.slice();

public static void TestScanByte(ж<Δtesting.T> Ꮡt) {
    foreach (var (n, test) in scanTests) {
        var buf = strings.NewReader(test);
        var s = NewScanner(new bufio_test_package.strings_ReaderжReader(buf));
        s.Split(new Func<slice<byte>, bool, (nint, slice<byte>, error)>(ScanBytes));
        nint i = default!;
        for (i = 0; s.Scan(); i++) {
            {
                var b = s.Bytes(); if (len(b) != 1 || b[0] != test[i]) {
                    Ꮡt.Errorf("#%d: %d: expected %q got %q"u8, n, i, test, b);
                }
            }
        }
        if (i != len(test)) {
            Ꮡt.Errorf("#%d: termination expected at %d; got %d"u8, n, len(test), i);
        }
        var err = s.Err();
        if (err != default!) {
            Ꮡt.Errorf("#%d: %v"u8, n, err);
        }
    }
}

// Test that the rune splitter returns same sequence of runes (not bytes) as for range string.
public static void TestScanRune(ж<Δtesting.T> Ꮡt) {
    foreach (var (n, test) in scanTests) {
        var buf = strings.NewReader(test);
        var s = NewScanner(new bufio_test_package.strings_ReaderжReader(buf));
        s.Split(new Func<slice<byte>, bool, (nint, slice<byte>, error)>(ScanRunes));
        nint i = default!;
        nint runeCount = default!;
        rune expect = default!;
        // Use a string range loop to validate the sequence of runes.
        foreach (var (iᴛ1, rᴛ1) in test) {
            i = iᴛ1;
            expect = rᴛ1;

            if (!s.Scan()) {
                break;
            }
            runeCount++;
            var (got, _) = utf8.DecodeRune(s.Bytes());
            if (got != expect) {
                Ꮡt.Errorf("#%d: %d: expected %q got %q"u8, n, i, expect, got);
            }
        }
        if (s.Scan()) {
            Ꮡt.Errorf("#%d: scan ran too long, got %q"u8, n, s.Text());
        }
        nint testRuneCount = utf8.RuneCountInString(test);
        if (runeCount != testRuneCount) {
            Ꮡt.Errorf("#%d: termination expected at %d; got %d"u8, n, testRuneCount, runeCount);
        }
        var err = s.Err();
        if (err != default!) {
            Ꮡt.Errorf("#%d: %v"u8, n, err);
        }
    }
}

internal static slice<@string> wordScanTests = new @string[]{
    ""u8,
    " "u8,
    "\n"u8,
    "a"u8,
    " a "u8,
    "abc def"u8,
    " abc def "u8,
    " abc\tdef\nghi\rjkl\fmno\vpqr\u0085stu\u00a0\n"u8
}.slice();

// Test that the word splitter returns the same data as strings.Fields.
public static void TestScanWords(ж<Δtesting.T> Ꮡt) {
    foreach (var (n, test) in wordScanTests) {
        var buf = strings.NewReader(test);
        var s = NewScanner(new bufio_test_package.strings_ReaderжReader(buf));
        s.Split(new Func<slice<byte>, bool, (nint, slice<byte>, error)>(ScanWords));
        var words = strings.Fields(test);
        nint wordCount = default!;
        for (wordCount = 0; wordCount < len(words); wordCount++) {
            if (!s.Scan()) {
                break;
            }
            @string got = s.Text();
            if (got != words[wordCount]) {
                Ꮡt.Errorf("#%d: %d: expected %q got %q"u8, n, wordCount, words[wordCount], got);
            }
        }
        if (s.Scan()) {
            Ꮡt.Errorf("#%d: scan ran too long, got %q"u8, n, s.Text());
        }
        if (wordCount != len(words)) {
            Ꮡt.Errorf("#%d: termination expected at %d; got %d"u8, n, len(words), wordCount);
        }
        var err = s.Err();
        if (err != default!) {
            Ꮡt.Errorf("#%d: %v"u8, n, err);
        }
    }
}

// slowReader is a reader that returns only a few bytes at a time, to test the incremental
// reads in Scanner.Scan.
[GoType] partial struct slowReader {
    internal nint max;
    internal Δio.Reader buf;
}

[GoRecv] internal static (nint n, error err) Read(this ref slowReader sr, slice<byte> p) {
    if (len(p) > sr.max) {
        p = p[0..(int)(sr.max)];
    }
    return sr.buf.Read(p);
}

// genLine writes to buf a predictable but non-trivial line of text of length
// n, including the terminal newline and an occasional carriage return.
// If addNewline is false, the \r and \n are not emitted.
internal static void genLine(ж<bytes.Buffer> Ꮡbuf, nint lineNum, nint n, bool addNewline) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    buf.Reset();
    var doCR = lineNum % 5 == 0;
    if (doCR) {
        n--;
    }
    for (nint i = 0; i < n - 1; i++) {
        // Stop early for \n.
        var c = (byte)((rune)'a' + (byte)(lineNum + i));
        if (c == (rune)'\n' || c == (rune)'\r') {
            // Don't confuse us.
            c = (rune)'N';
        }
        buf.WriteByte(c);
    }
    if (addNewline) {
        if (doCR) {
            buf.WriteByte((rune)'\r');
        }
        buf.WriteByte((rune)'\n');
    }
}

// Test the line splitter, including some carriage returns but no long lines.
public static void TestScanLongLines(ж<Δtesting.T> Ꮡt) {
    // Build a buffer of lots of line lengths up to but not exceeding smallMaxTokenSize.
    var tmp = @new<bytes.Buffer>();
    var buf = @new<bytes.Buffer>();
    nint lineNum = 0;
    nint j = 0;
    for (nint i = 0; i < (nint)(2 * smallMaxTokenSize); i++) {
        genLine(tmp, lineNum, j, true);
        if (j < smallMaxTokenSize){
            j++;
        } else {
            j--;
        }
        buf.Write(tmp.Bytes());
        lineNum++;
    }
    var s = NewScanner(new bufio_test_package.slowReaderжReader(Ꮡ(new slowReader(1, new bufio_test_package.bytes_BufferжReader(buf)))));
    s.Split(new Func<slice<byte>, bool, (nint, slice<byte>, error)>(ScanLines));
    s.MaxTokenSize(smallMaxTokenSize);
    j = 0;
    for (nint lineNumΔ1 = 0; s.Scan(); lineNumΔ1++) {
        genLine(tmp, lineNumΔ1, j, false);
        if (j < smallMaxTokenSize){
            j++;
        } else {
            j--;
        }
        @string line = tmp.String(); // We use the string-valued token here, for variety.
        if (s.Text() != line) {
            Ꮡt.Errorf("%d: bad line: %d %d\n%.100q\n%.100q\n"u8, lineNumΔ1, len(s.Bytes()), len(line), s.Text(), line);
        }
    }
    var err = s.Err();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

// Test that the line splitter errors out on a long line.
public static void TestScanLineTooLong(ж<Δtesting.T> Ꮡt) {
    UntypedInt smallMaxTokenSize = 256; // Much smaller for more efficient testing.
    // Build a buffer of lots of line lengths up to but not exceeding smallMaxTokenSize.
    var tmp = @new<bytes.Buffer>();
    var buf = @new<bytes.Buffer>();
    nint lineNum = 0;
    nint j = 0;
    for (nint i = 0; i < (nint)(2 * smallMaxTokenSize); i++) {
        genLine(tmp, lineNum, j, true);
        j++;
        buf.Write(tmp.Bytes());
        lineNum++;
    }
    var s = NewScanner(new bufio_test_package.slowReaderжReader(Ꮡ(new slowReader(3, new bufio_test_package.bytes_BufferжReader(buf)))));
    s.Split(new Func<slice<byte>, bool, (nint, slice<byte>, error)>(ScanLines));
    s.MaxTokenSize(smallMaxTokenSize);
    j = 0;
    for (nint lineNumΔ1 = 0; s.Scan(); lineNumΔ1++) {
        genLine(tmp, lineNumΔ1, j, false);
        if (j < smallMaxTokenSize){
            j++;
        } else {
            j--;
        }
        var line = tmp.Bytes();
        if (!bytes.Equal(s.Bytes(), line)) {
            Ꮡt.Errorf("%d: bad line: %d %d\n%.100q\n%.100q\n"u8, lineNumΔ1, len(s.Bytes()), len(line), s.Bytes(), line);
        }
    }
    var err = s.Err();
    if (!AreEqual(err, ErrTooLong)) {
        Ꮡt.Fatalf("expected ErrTooLong; got %s"u8, err);
    }
}

// Test that the line splitter handles a final line without a newline.
internal static void testNoNewline(@string text, slice<@string> lines, ж<Δtesting.T> Ꮡt) {
    var buf = strings.NewReader(text);
    var s = NewScanner(new bufio_test_package.slowReaderжReader(Ꮡ(new slowReader(7, new bufio_test_package.strings_ReaderжReader(buf)))));
    s.Split(new Func<slice<byte>, bool, (nint, slice<byte>, error)>(ScanLines));
    for (nint lineNum = 0; s.Scan(); lineNum++) {
        @string line = lines[lineNum];
        if (s.Text() != line) {
            Ꮡt.Errorf("%d: bad line: %d %d\n%.100q\n%.100q\n"u8, lineNum, len(s.Bytes()), len(line), s.Bytes(), line);
        }
    }
    var err = s.Err();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string textᶜ = "abcdefghijklmn\nopqrstuvwxyz"u8;

// Test that the line splitter handles a final line without a newline.
public static void TestScanLineNoNewline(ж<Δtesting.T> Ꮡt) {
    @string text = textᶜ;
    var lines = new @string[]{
        "abcdefghijklmn"u8,
        "opqrstuvwxyz"u8
    }.slice();
    testNoNewline(text, lines, Ꮡt);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string textᶜ1 = "abcdefghijklmn\nopqrstuvwxyz\r"u8;

// Test that the line splitter handles a final line with a carriage return but no newline.
public static void TestScanLineReturnButNoNewline(ж<Δtesting.T> Ꮡt) {
    @string text = textᶜ1;
    var lines = new @string[]{
        "abcdefghijklmn"u8,
        "opqrstuvwxyz"u8
    }.slice();
    testNoNewline(text, lines, Ꮡt);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string textᶜ2 = "abcdefghijklmn\nopqrstuvwxyz\n\n"u8;

// Test that the line splitter handles a final empty line.
public static void TestScanLineEmptyFinalLine(ж<Δtesting.T> Ꮡt) {
    @string text = textᶜ2;
    var lines = new @string[]{
        "abcdefghijklmn"u8,
        "opqrstuvwxyz"u8,
        ""u8
    }.slice();
    testNoNewline(text, lines, Ꮡt);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string textᶜ3 = "abcdefghijklmn\nopqrstuvwxyz\n\r"u8;

// Test that the line splitter handles a final empty line with a carriage return but no newline.
public static void TestScanLineEmptyFinalLineWithCR(ж<Δtesting.T> Ꮡt) {
    @string text = textᶜ3;
    var lines = new @string[]{
        "abcdefghijklmn"u8,
        "opqrstuvwxyz"u8,
        ""u8
    }.slice();
    testNoNewline(text, lines, Ꮡt);
}

internal static error testError = errors.New("testError"u8);

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string textᶜ4 = "abcdefghijklmnopqrstuvwxyz"u8;

// Test the correct error is returned when the split function errors out.
public static void TestSplitError(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Create a split function that delivers a little data, then a predictable error.
    nint numSplits = 0;
    const nint okCount = 7;
    var errorSplit = (nint advance, slice<byte> token, error err) (slice<byte> data, bool atEOF) => {
        if (atEOF) {
            throw panic("didn't get enough data");
        }
        if (numSplits >= okCount) {
            return (0, default!, testError);
        }
        numSplits++;
        return (1, data[0..1], default!);
    };
    // Read the data.
    @string text = textᶜ4;
    var buf = strings.NewReader(text);
    var s = NewScanner(new bufio_test_package.slowReaderжReader(Ꮡ(new slowReader(1, new bufio_test_package.strings_ReaderжReader(buf)))));
    s.Split(new Func<slice<byte>, bool, (nint, slice<byte>, error)>(errorSplit));
    nint i = default!;
    for (i = 0; s.Scan(); i++) {
        if (len(s.Bytes()) != 1 || text[i] != s.Bytes()[0]) {
            Ꮡt.Errorf("#%d: expected %q got %q"u8, i, text[i], s.Bytes()[0]);
        }
    }
    // Check correct termination location and error.
    if (i != okCount) {
        Ꮡt.Errorf("unexpected termination; expected %d tokens got %d"u8, (nint)(okCount), i);
    }
    var err = s.Err();
    if (!AreEqual(err, testError)) {
        Ꮡt.Fatalf("expected %q got %v"u8, testError, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notTestingEofˢ = (@string)"not testing EOF"u8;
internal static readonly object wrongErrorˢ = (@string)"wrong error:"u8;

// Test that an EOF is overridden by a user-generated scan error.
public static void TestErrAtEOF(ж<Δtesting.T> Ꮡt) {
    var s = NewScanner(new bufio_test_package.strings_ReaderжReader(strings.NewReader("1 2 33"u8)));
    // This splitter will fail on last entry, after s.err==EOF.
    var sʗ1 = s;
    var split = (slice<byte> data, bool atEOF) => {
        nint advance = default!;
        slice<byte> token = default!;
        error err = default!;
        (advance, token, err) = ScanWords(data, atEOF);
        if (len(token) > 1) {
            if (!AreEqual(sʗ1.ErrOrEOF(), Δio.EOF)) {
                Ꮡt.Fatal(notTestingEofˢ);
            }
            err = testError;
        }
        return (advance, token, err);
    };
    s.Split(new Func<slice<byte>, bool, (nint, slice<byte>, error)>(split));
    while (s.Scan()) {
    }
    if (!AreEqual(s.Err(), testError)) {
        Ꮡt.Fatal(wrongErrorˢ, s.Err());
    }
}

// Test for issue 5268.
[GoType] partial struct alwaysError {
}

internal static (nint, error) Read(this alwaysError _, slice<byte> p) {
    return (0, Δio.ErrUnexpectedEOF);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readShouldFailˢ = (@string)"read should fail"u8;

public static void TestNonEOFWithEmptyRead(ж<Δtesting.T> Ꮡt) {
    var scanner = NewScanner(new alwaysError(nil));
    while (scanner.Scan()) {
        Ꮡt.Fatal(readShouldFailˢ);
    }
    var err = scanner.Err();
    if (!AreEqual(err, Δio.ErrUnexpectedEOF)) {
        Ꮡt.Errorf("unexpected error: %v"u8, err);
    }
}

// Test that Scan finishes if we have endless empty reads.
[GoType] partial struct endlessZeros {
}

internal static (nint, error) Read(this endlessZeros _, slice<byte> p) {
    return (0, default!);
}

public static void TestBadReader(ж<Δtesting.T> Ꮡt) {
    var scanner = NewScanner(new endlessZeros(nil));
    while (scanner.Scan()) {
        Ꮡt.Fatal(readShouldFailˢ);
    }
    var err = scanner.Err();
    if (!AreEqual(err, Δio.ErrNoProgress)) {
        Ꮡt.Errorf("unexpected error: %v"u8, err);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wordᶜ = "ipsum"u8;

public static void TestScanWordsExcessiveWhiteSpace(ж<Δtesting.T> Ꮡt) {
    @string word = wordᶜ;
    @string s = strings.Repeat(" "u8, 4 * smallMaxTokenSize) + word;
    var scanner = NewScanner(new bufio_test_package.strings_ReaderжReader(strings.NewReader(s)));
    scanner.MaxTokenSize(smallMaxTokenSize);
    scanner.Split(new Func<slice<byte>, bool, (nint, slice<byte>, error)>(ScanWords));
    if (!scanner.Scan()) {
        Ꮡt.Fatalf("scan failed: %v"u8, scanner.Err());
    }
    {
        @string token = scanner.Text(); if (token != word) {
            Ꮡt.Fatalf("unexpected token: %v"u8, token);
        }
    }
}

// Test that empty tokens, including at end of line or end of file, are found by the scanner.
// Issue 8672: Could miss final empty token.
internal static (nint advance, slice<byte> token, error err) commaSplit(slice<byte> data, bool atEOF) {
    for (nint i = 0; i < len(data); i++) {
        if (data[i] == (rune)',') {
            return (i + 1, data[..(int)(i)], default!);
        }
    }
    return (0, data, ErrFinalToken);
}

internal static void testEmptyTokens(ж<Δtesting.T> Ꮡt, @string text, slice<@string> values) {
    var s = NewScanner(new bufio_test_package.strings_ReaderжReader(strings.NewReader(text)));
    s.Split(new Func<slice<byte>, bool, (nint, slice<byte>, error)>(commaSplit));
    nint i = default!;
    for (i = 0; s.Scan(); i++) {
        if (i >= len(values)) {
            Ꮡt.Fatalf("got %d fields, expected %d"u8, i + 1, len(values));
        }
        if (s.Text() != values[i]) {
            Ꮡt.Errorf("%d: expected %q got %q"u8, i, values[i], s.Text());
        }
    }
    if (i != len(values)) {
        Ꮡt.Fatalf("got %d fields, expected %d"u8, i, len(values));
    }
    {
        var err = s.Err(); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
}

public static void TestEmptyTokens(ж<Δtesting.T> Ꮡt) {
    testEmptyTokens(Ꮡt, "1,2,3,"u8, new @string[]{"1"u8, "2"u8, "3"u8, ""u8}.slice());
}

public static void TestWithNoEmptyTokens(ж<Δtesting.T> Ꮡt) {
    testEmptyTokens(Ꮡt, "1,2,3"u8, new @string[]{"1"u8, "2"u8, "3"u8}.slice());
}

internal static (nint advance, slice<byte> token, error err) loopAtEOFSplit(slice<byte> data, bool atEOF) {
    if (len(data) > 0) {
        return (1, data[..1], default!);
    }
    return (0, data, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object shouldHavePanickedˢ = (@string)"should have panicked"u8;
internal static readonly @string emptyTokensˢ = "empty tokens"u8;
internal static readonly object loopingˢ = (@string)"looping"u8;
internal static readonly object afterScanˢ = (@string)"after scan:"u8;

public static void TestDontLoopForever(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var s = NewScanner(new bufio_test_package.strings_ReaderжReader(strings.NewReader(abcˢ)));
        s.Split(new Func<slice<byte>, bool, (nint, slice<byte>, error)>(loopAtEOFSplit));
        // Expect a panic
        defer(() => {
            var err = recover();
            if (err == default!) {
                Ꮡt.Fatal(shouldHavePanickedˢ);
            }
            {
                var (msg, ok) = err._<@string>(ᐧ); if (!ok || !strings.Contains(msg, emptyTokensˢ)) {
                    throw panic(err);
                }
            }
        }, ref ᒐ);
        for (nint count = 0; s.Scan(); count++) {
            if (count > 1000) {
                Ꮡt.Fatal(loopingˢ);
            }
        }
        if (s.Err() != default!) {
            Ꮡt.Fatal(afterScanˢ, s.Err());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestBlankLines(ж<Δtesting.T> Ꮡt) {
    var s = NewScanner(new bufio_test_package.strings_ReaderжReader(strings.NewReader(strings.Repeat("\n"u8, 1000))));
    for (nint count = 0; s.Scan(); count++) {
        if (count > 2000) {
            Ꮡt.Fatal(loopingˢ);
        }
    }
    if (s.Err() != default!) {
        Ꮡt.Fatal(afterScanˢ, s.Err());
    }
}

[GoType("num:nint")] partial struct countdown;

[GoRecv] internal static (nint advance, slice<byte> token, error err) split(this ref countdown c, slice<byte> data, bool atEOF) {
    if (c > 0) {
        c--;
        return (1, data[..1], default!);
    }
    return (0, default!, default!);
}

// Check that the looping-at-EOF check doesn't trigger for merely empty tokens.
public static void TestEmptyLinesOK(ж<Δtesting.T> Ꮡt) {
    ref var c = ref heap<countdown>(out var Ꮡc);
    c = ((countdown)10000);
    var s = NewScanner(new bufio_test_package.strings_ReaderжReader(strings.NewReader(strings.Repeat("\n"u8, 10000))));
    s.Split(new Func<slice<byte>, bool, (nint, slice<byte>, error)>(Ꮡc.split));
    while (s.Scan()) {
    }
    if (s.Err() != default!) {
        Ꮡt.Fatal(afterScanˢ, s.Err());
    }
    if (c != 0) {
        Ꮡt.Fatalf("stopped with %d left to process"u8, c);
    }
}

// Make sure we can read a huge token if a big enough buffer is provided.
public static void TestHugeBuffer(ж<Δtesting.T> Ꮡt) {
    @string text = strings.Repeat("x"u8, 2 * MaxScanTokenSize);
    var s = NewScanner(new bufio_test_package.strings_ReaderжReader(strings.NewReader(text + "\n"u8)));
    s.Buffer(new slice<byte>(100), 3 * MaxScanTokenSize);
    while (s.Scan()) {
        @string token = s.Text();
        if (token != text) {
            Ꮡt.Errorf("scan got incorrect token of length %d"u8, len(token));
        }
    }
    if (s.Err() != default!) {
        Ꮡt.Fatal(afterScanˢ, s.Err());
    }
}

[GoType("num:nint")] partial struct negativeEOFReader;

[GoRecv] internal static (nint, error) Read(this ref negativeEOFReader r, slice<byte> p) {
    if (r > 0) {
        nint c = (nint)(r);
        if (c > len(p)) {
            c = len(p);
        }
        for (nint i = 0; i < c; i++) {
            p[i] = (rune)'a';
        }
        p[c - 1] = (rune)'\n';
        r -= ((negativeEOFReader)c);
        return (c, default!);
    }
    return (-1, Δio.EOF);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readTooManyLinesˢ = (@string)"read too many lines"u8;

// Test that the scanner doesn't panic and returns ErrBadReadCount
// on a reader that returns a negative count of bytes read (issue 38053).
public static void TestNegativeEOFReader(ж<Δtesting.T> Ꮡt) {
    ref var r = ref heap<negativeEOFReader>(out var Ꮡr);
    r = ((negativeEOFReader)10);
    var scanner = NewScanner(new bufio_test_package.negativeEOFReaderжReader(Ꮡr));
    nint c = 0;
    while (scanner.Scan()) {
        c++;
        if (c > 1) {
            Ꮡt.Error(readTooManyLinesˢ);
            break;
        }
    }
    {
        var (got, want) = (scanner.Err(), ErrBadReadCount); if (!AreEqual(got, want)) {
            Ꮡt.Errorf("scanner.Err: got %v, want %v"u8, got, want);
        }
    }
}

// largeReader returns an invalid count that is larger than the number
// of bytes requested.
[GoType] partial struct largeReader {
}

internal static (nint, error) Read(this largeReader _, slice<byte> p) {
    return (len(p) + 1, default!);
}

// Test that the scanner doesn't panic and returns ErrBadReadCount
// on a reader that returns an impossibly large count of bytes read (issue 38053).
public static void TestLargeReader(ж<Δtesting.T> Ꮡt) {
    var scanner = NewScanner(new largeReader(nil));
    while (scanner.Scan()) {
    }
    {
        var (got, want) = (scanner.Err(), ErrBadReadCount); if (!AreEqual(got, want)) {
            Ꮡt.Errorf("scanner.Err: got %v, want %v"u8, got, want);
        }
    }
}

} // end bufio_test_package
