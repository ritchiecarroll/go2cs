// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using Δio = io_package;
using Δmath = math_package;
using os = os_package;
using reflect = reflect_package;
using strconv = strconv_package;
using Δsync = sync_package;
using utf8 = unicode.utf8_package;
using unicode;
using ꓸꓸꓸany = Span<any>;

partial class fmt_package {

// ScanState represents the scanner state passed to custom scanners.
// Scanners may do rune-at-a-time scanning or ask the ScanState
// to discover the next space-delimited token.
partial interface ScanState :
    Δio.Reader,
    Δio.RuneScanner
{
    // SkipSpace skips space in the input. Newlines are treated appropriately
    // for the operation being performed; see the package documentation
    // for more information.
    void SkipSpace();
    // Token skips space in the input if skipSpace is true, then returns the
    // run of Unicode code points c satisfying f(c).  If f is nil,
    // !unicode.IsSpace(c) is used; that is, the token will hold non-space
    // characters. Newlines are treated appropriately for the operation being
    // performed; see the package documentation for more information.
    // The returned slice points to shared data that may be overwritten
    // by the next call to Token, a call to a Scan function using the ScanState
    // as input, or when the calling Scan method returns.
    (slice<byte> token, error err) Token(bool skipSpace, Func<rune, bool> f);
    // Width returns the value of the width option and whether it has been set.
    // The unit is Unicode code points.
    (nint wid, bool ok) Width();
}

// Scanner is implemented by any value that has a Scan method, which scans
// the input for the representation of a value and stores the result in the
// receiver, which must be a pointer to be useful. The Scan method is called
// for any argument to [Scan], [Scanf], or [Scanln] that implements it.
partial interface Scanner {
    error Scan(ScanState state, rune verb);
}

// Scan scans text read from standard input, storing successive
// space-separated values into successive arguments. Newlines count
// as space. It returns the number of items successfully scanned.
// If that is less than the number of arguments, err will report why.
public static (nint n, error err) Scan(params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    return Fscan(new os_FileжReader(os.Stdin), a.ꓸꓸꓸ);
}

// Scanln is similar to [Scan], but stops scanning at a newline and
// after the final item there must be a newline or EOF.
public static (nint n, error err) Scanln(params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    return Fscanln(new os_FileжReader(os.Stdin), a.ꓸꓸꓸ);
}

// Scanf scans text read from standard input, storing successive
// space-separated values into successive arguments as determined by
// the format. It returns the number of items successfully scanned.
// If that is less than the number of arguments, err will report why.
// Newlines in the input must match newlines in the format.
// The one exception: the verb %c always scans the next rune in the
// input, even if it is a space (or tab etc.) or newline.
public static (nint n, error err) Scanf(@string format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    return Fscanf(new os_FileжReader(os.Stdin), format, a.ꓸꓸꓸ);
}

partial struct stringReader /*@string*/;

internal static (nint n, error err) Read(this ref stringReader r, slice<byte> b) {
    nint n = default!;
    error err = default!;

    n = copy(b, r);
    r = (r).slice(n);
    if (n == 0) {
        err = Δio.EOF;
    }
    return (n, err);
}

// Sscan scans the argument string, storing successive space-separated
// values into successive arguments. Newlines count as space. It
// returns the number of items successfully scanned. If that is less
// than the number of arguments, err will report why.
public static (nint n, error err) Sscan(@string strʗp, params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    ref var str = ref heap(strʗp, out var Ꮡstr);
    return Fscan(new stringReaderжReader(Ꮡstr.Reinterpret<@string, stringReader>()), a.ꓸꓸꓸ);
}

// Sscanln is similar to [Sscan], but stops scanning at a newline and
// after the final item there must be a newline or EOF.
public static (nint n, error err) Sscanln(@string strʗp, params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    ref var str = ref heap(strʗp, out var Ꮡstr);
    return Fscanln(new stringReaderжReader(Ꮡstr.Reinterpret<@string, stringReader>()), a.ꓸꓸꓸ);
}

// Sscanf scans the argument string, storing successive space-separated
// values into successive arguments as determined by the format. It
// returns the number of items successfully parsed.
// Newlines in the input must match newlines in the format.
public static (nint n, error err) Sscanf(@string strʗp, sstring format, params ꓸꓸꓸany aʗp) {
    var a = aʗp.sslice();

    ref var str = ref heap(strʗp, out var Ꮡstr);
    return Fscanf(new stringReaderжReader(Ꮡstr.Reinterpret<@string, stringReader>()), format, a.ꓸꓸꓸ);
}

// Fscan scans text read from r, storing successive space-separated
// values into successive arguments. Newlines count as space. It
// returns the number of items successfully scanned. If that is less
// than the number of arguments, err will report why.
public static (nint n, error err) Fscan(Δio.Reader r, params ꓸꓸꓸany aʗp) {
    nint n = default!;
    error err = default!;
    var a = aʗp.slice();

    var (s, old) = newScanState(r, true, false);
    (n, err) = s.doScan(a);
    s.free(old);
    return (n, err);
}

// Fscanln is similar to [Fscan], but stops scanning at a newline and
// after the final item there must be a newline or EOF.
public static (nint n, error err) Fscanln(Δio.Reader r, params ꓸꓸꓸany aʗp) {
    nint n = default!;
    error err = default!;
    var a = aʗp.slice();

    var (s, old) = newScanState(r, false, true);
    (n, err) = s.doScan(a);
    s.free(old);
    return (n, err);
}

// Fscanf scans text read from r, storing successive space-separated
// values into successive arguments as determined by the format. It
// returns the number of items successfully parsed.
// Newlines in the input must match newlines in the format.
public static (nint n, error err) Fscanf(Δio.Reader r, sstring format, params ꓸꓸꓸany aʗp) {
    nint n = default!;
    error err = default!;
    var a = aʗp.slice();

    var (s, old) = newScanState(r, false, false);
    (n, err) = s.doScanf(format, a);
    s.free(old);
    return (n, err);
}

// scanError represents an error generated by the scanning software.
// It's used as a unique signature to identify such errors when recovering.
partial struct scanError {
    internal error err;
}

internal static UntypedInt eof => -1;

// ss is the internal implementation of ScanState.
partial struct ss {
    internal Δio.RuneScanner rs; // where to read input
    internal buffer buf;         // token accumulator
    internal nint count;           // runes consumed so far.
    internal bool atEOF;           // already read EOF
    internal partial ref ssave ssave { get; }
}

// ssave holds the parts of ss that need to be
// saved and restored on recursive scans.
partial struct ssave {
    internal bool validSave; // is or was a part of an actual ss.
    internal bool nlIsEnd; // whether newline terminates scan
    internal bool nlIsSpace; // whether newline counts as white space
    internal nint argLimit; // max value of ss.count for this arg; argLimit <= limit
    internal nint limit; // max value of ss.count.
    internal nint maxWid; // width of this arg.
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string scanStateSReadShouldNotˢ = "ScanState's Read should not be called. Use ReadRune"u8;

// The Read method is only in ScanState so that ScanState
// satisfies io.Reader. It will never be called when used as
// intended, so there is no need to make it actually work.
internal static (nint n, error err) Read(this ref ss s, slice<byte> buf) {
    return (0, errors.New(scanStateSReadShouldNotˢ));
}

internal static (rune r, nint size, error err) ReadRune(this ref ss s) {
    rune r = default!;
    nint size = default!;
    error err = default!;

    if (s.atEOF || s.count >= s.argLimit) {
        err = Δio.EOF;
        return (r, size, err);
    }
    (r, size, err) = s.rs.ReadRune();
    if (err == default!){
        s.count++;
        if (s.nlIsEnd && r == (rune)'\n') {
            s.atEOF = true;
        }
    } else 
    if (AreEqual(err, Δio.EOF)) {
        s.atEOF = true;
    }
    return (r, size, err);
}

internal static (nint wid, bool ok) Width(this ref ss s) {
    if (s.maxWid == hugeWid) {
        return (0, false);
    }
    return (s.maxWid, true);
}

// The public method returns an error; this private one panics.
// If getRune reaches EOF, the return value is EOF (-1).
internal static rune /*r*/ getRune(this ref ss s) {
    rune r = default!;

    (r, _, var err) = s.ReadRune();
    if (err != default!) {
        if (AreEqual(err, Δio.EOF)) {
            return eof;
        }
        s.error(err);
    }
    return r;
}

// mustReadRune turns io.EOF into a panic(io.ErrUnexpectedEOF).
// It is called in cases such as string scanning where an EOF is a
// syntax error.
internal static rune /*r*/ mustReadRune(this ref ss s) {
    rune r = default!;

    r = s.getRune();
    if (r == eof) {
        s.error(Δio.ErrUnexpectedEOF);
    }
    return r;
}

internal static error UnreadRune(this ref ss s) {
    s.rs.UnreadRune();
    s.atEOF = false;
    s.count--;
    return default!;
}

internal static void error(this ref ss s, error err) {
    throw panic(new scanError(err));
}

internal static void errorString(this ref ss s, @string err) {
    throw panic(new scanError(errors.New(err)));
}

internal static (slice<byte> tok, error err) Token(this ж<ss> Ꮡs, bool skipSpace, Func<rune, bool> f) {
    slice<byte> tok = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        defer(() => {
            {
                var e = recover(); if (e != default!) {
                    {
                        ref var se = ref heap<scanError>(out var Ꮡse);
                        (se, var ok) = e._<scanError>(ᐧ); if (ok){
                            err = se.err;
                        } else {
                            throw panic(e);
                        }
                    }
                }
            }
        }, ref ᒐ);
        if (f == default!) {
            f = notSpace;
        }
        s.buf = s.buf[..0];
        tok = s.token(skipSpace, f);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (tok, err);
}

// space is a copy of the unicode.White_Space ranges,
// to avoid depending on package unicode.
internal static slice<array<uint16>> space = GoReflect.WithElemDims(new array<uint16>[]{
    new uint16[]{0x0009, 0x000d}.array(),
    new uint16[]{0x0020, 0x0020}.array(),
    new uint16[]{0x0085, 0x0085}.array(),
    new uint16[]{0x00a0, 0x00a0}.array(),
    new uint16[]{0x1680, 0x1680}.array(),
    new uint16[]{0x2000, 0x200a}.array(),
    new uint16[]{0x2028, 0x2029}.array(),
    new uint16[]{0x202f, 0x202f}.array(),
    new uint16[]{0x205f, 0x205f}.array(),
    new uint16[]{0x3000, 0x3000}.array()
}.slice(), 2);

internal static bool isSpace(rune r) {
    if (r >= (rune)(1 << (int)(16))) {
        return false;
    }
    var rx = (uint16)r;
    foreach (var (_, vᴛ1) in space) {
        var rng = vᴛ1.Clone();

        if (rx < rng[0]) {
            return false;
        }
        if (rx <= rng[1]) {
            return true;
        }
    }
    return false;
}

// notSpace is the default scanning function used in Token.
internal static bool notSpace(rune r) {
    return !isSpace(r);
}

// readRune is a structure to enable reading UTF-8 encoded code points
// from an io.Reader. It is used if the Reader given to the scanner does
// not already implement io.RuneScanner.
partial struct readRune {
    internal Δio.Reader reader;
    internal array<byte> buf = new(utf8.UTFMax); // used only inside ReadRune
    internal nint pending;              // number of bytes in pendBuf; only >0 for bad UTF-8
    internal array<byte> pendBuf = new(utf8.UTFMax); // bytes left over
    internal rune peekRune;              // if >=0 next rune; when <0 is ^(previous Rune)
}

// readByte returns the next byte from the input, which may be
// left over from a previous read if the UTF-8 was ill-formed.
internal static (byte b, error err) readByte(this ref readRune r) {
    byte b = default!;
    error err = default!;

    if (r.pending > 0) {
        b = r.pendBuf[0];
        copy(r.pendBuf[0..], r.pendBuf[1..]);
        r.pending--;
        return (b, err);
    }
    (var n, err) = Δio.ReadFull(r.reader, r.pendBuf[..1]);
    if (n != 1) {
        return (0, err);
    }
    return (r.pendBuf[0], err);
}

// ReadRune returns the next UTF-8 encoded code point from the
// io.Reader inside r.
internal static (rune rr, nint size, error err) ReadRune(this ref readRune r) {
    rune rr = default!;
    nint size = default!;
    error err = default!;

    if (r.peekRune >= 0) {
        rr = r.peekRune;
        r.peekRune = ~r.peekRune;
        size = utf8.RuneLen(rr);
        return (rr, size, err);
    }
    (r.buf[0], err) = r.readByte();
    if (err != default!) {
        return (rr, size, err);
    }
    if (r.buf[0] < utf8.RuneSelf) {
        // fast check for common ASCII case
        rr = (rune)r.buf[0];
        size = 1; // Known to be 1.
        // Flip the bits of the rune so it's available to UnreadRune.
        r.peekRune = ~rr;
        return (rr, size, err);
    }
    nint n = default!;
    for (n = 1; !utf8.FullRune(r.buf.slice(0, n)); n++) {
        (r.buf[n], err) = r.readByte();
        if (err != default!) {
            if (AreEqual(err, Δio.EOF)) {
                err = default!;
                break;
            }
            return (rr, size, err);
        }
    }
    (rr, size) = utf8.DecodeRune(r.buf.slice(0, n));
    if (size < n) {
        // an error, save the bytes for the next read
        copy(r.pendBuf.slice(r.pending), r.buf.slice(size, n));
        r.pending += n - size;
    }
    // Flip the bits of the rune so it's available to UnreadRune.
    r.peekRune = ~rr;
    return (rr, size, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fmtScanningCalledˢ = "fmt: scanning called UnreadRune with no rune available"u8;

internal static error UnreadRune(this ref readRune r) {
    if (r.peekRune >= 0) {
        return errors.New(fmtScanningCalledˢ);
    }
    // Reverse bit flip of previously read rune to obtain valid >=0 state.
    r.peekRune = ~r.peekRune;
    return default!;
}

internal static ж<Δsync.Pool> ᏑssFree = new StandardBox<Δsync.Pool>(new Δsync.Pool(
    New: () => @new<ss>()
));
internal static ref Δsync.Pool ssFree => ref ᏑssFree.Value;

// newScanState allocates a new ss struct or grab a cached one.
internal static (ж<ss> s, ssave old) newScanState(Δio.Reader r, bool nlIsSpace, bool nlIsEnd) {
    ж<ss> s = default!;
    ssave old = default!;

    s = ᏑssFree.Get()._<ж<ss>>();
    {
        var (rs, ok) = r._<Δio.RuneScanner>(ᐧ); if (ok){
            s.Value.rs = rs;
        } else {
            s.Value.rs = new readRuneжRuneScanner(Ꮡ(new readRune(reader: r, peekRune: -1)));
        }
    }
    s.Value.nlIsSpace = nlIsSpace;
    s.Value.nlIsEnd = nlIsEnd;
    s.Value.atEOF = false;
    s.Value.limit = hugeWid;
    s.Value.argLimit = hugeWid;
    s.Value.maxWid = hugeWid;
    s.Value.validSave = true;
    s.Value.count = 0;
    return (s, old);
}

// free saves used ss structs in ssFree; avoid an allocation per invocation.
internal static void free(this ж<ss> Ꮡs, ssave old) {
    ref var s = ref Ꮡs.DerefOrNull();

    // If it was used recursively, just restore the old state.
    if (old.validSave) {
        s.ssave = old;
        return;
    }
    // Don't hold on to ss structs with large buffers.
    if (cap(s.buf) > 1024) {
        return;
    }
    s.buf = s.buf[..0];
    s.rs = default!;
    ᏑssFree.Put(Ꮡs.OrTypedNil());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unexpectedNewlineˢ = "unexpected newline"u8;

// SkipSpace provides Scan methods the ability to skip space and newline
// characters in keeping with the current scanning mode set by format strings
// and [Scan]/[Scanln].
internal static void SkipSpace(this ref ss s) {
    while (ᐧ) {
        var r = s.getRune();
        if (r == eof) {
            return;
        }
        if (r == (rune)'\r' && s.peek("\n"u8)) {
            continue;
        }
        if (r == (rune)'\n') {
            if (s.nlIsSpace) {
                continue;
            }
            s.errorString(unexpectedNewlineˢ);
            return;
        }
        if (!isSpace(r)) {
            s.UnreadRune();
            break;
        }
    }
}

// token returns the next space-delimited string from the input. It
// skips white space. For Scanln, it stops at newlines. For Scan,
// newlines are treated as spaces.
internal static slice<byte> token(this ref ss s, bool skipSpace, Func<rune, bool> f) {
    if (skipSpace) {
        s.SkipSpace();
    }
    // read until white space or newline
    while (ᐧ) {
        var r = s.getRune();
        if (r == eof) {
            break;
        }
        if (!f(r)) {
            s.UnreadRune();
            break;
        }
        s.buf.writeRune(r);
    }
    return s.buf;
}

internal static error errComplex = errors.New("syntax error scanning complex number"u8);

internal static error errBool = errors.New("syntax error scanning boolean"u8);

internal static nint indexRune(sstring s, rune r) {
    foreach (var (i, c) in s) {
        if (c == r) {
            return i;
        }
    }
    return -1;
}

// consume reads the next rune in the input and reports whether it is in the ok string.
// If accept is true, it puts the character into the input token.
internal static bool consume(this ref ss s, sstring ok, bool accept) {
    var r = s.getRune();
    if (r == eof) {
        return false;
    }
    if (indexRune(ok, r) >= 0) {
        if (accept) {
            s.buf.writeRune(r);
        }
        return true;
    }
    if (r != eof && accept) {
        s.UnreadRune();
    }
    return false;
}

// peek reports whether the next character is in the ok string, without consuming it.
internal static bool peek(this ref ss s, sstring ok) {
    var r = s.getRune();
    if (r != eof) {
        s.UnreadRune();
    }
    return indexRune(ok, r) >= 0;
}

internal static void notEOF(this ref ss s) {
    // Guarantee there is data to be read.
    {
        var r = s.getRune(); if (r == eof) {
            throw panic(Δio.EOF);
        }
    }
    s.UnreadRune();
}

// accept checks the next rune in the input. If it's a byte (sic) in the string, it puts it in the
// buffer and returns true. Otherwise it return false.
internal static bool accept(this ref ss s, sstring ok) {
    return s.consume(ok, true);
}

// okVerb verifies that the verb is present in the list, setting s.err appropriately if not.
internal static bool okVerb(this ref ss s, rune verb, sstring okVerbs, sstring typ) {
    foreach (var (_, v) in okVerbs) {
        if (v == verb) {
            return true;
        }
    }
    s.errorString("bad verb '%"u8 + ((@string)verb) + "' for "u8 + typ);
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string booleanˢ = "boolean"u8;

// scanBool returns the value of the boolean represented by the next token.
internal static bool scanBool(this ref ss s, rune verb) {
    s.SkipSpace();
    s.notEOF();
    if (!s.okVerb(verb, "tv"u8, booleanˢ)) {
        return false;
    }
    // Syntax-checking a boolean is annoying. We're not fastidious about case.
    switch (s.getRune()) {
    case (rune)'0': {
        return false;
    }
    case (rune)'1': {
        return true;
    }
    case (rune)'t' or (rune)'T': {
        if (s.accept("rR"u8) && (!s.accept("uU"u8) || !s.accept("eE"u8))) {
            s.error(errBool);
        }
        return true;
    }
    case (rune)'f' or (rune)'F': {
        if (s.accept("aA"u8) && (!s.accept("lL"u8) || !s.accept("sS"u8) || !s.accept("eE"u8))) {
            s.error(errBool);
        }
        return false;
    }}

    return false;
}

// Numerical elements
internal static readonly @string binaryDigits = "01"u8;

internal static readonly @string octalDigits = "01234567"u8;

internal static readonly @string decimalDigits = "0123456789"u8;

internal static readonly @string hexadecimalDigits = "0123456789aAbBcCdDeEfF"u8;

internal static readonly @string sign = "+-"u8;

internal static readonly @string period = "."u8;

internal static readonly @string exponent = "eEpP"u8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bdoUxXvˢ = "bdoUxXv"u8;
internal static readonly @string integerˢ = "integer"u8;

// getBase returns the numeric base represented by the verb and its digit string.
internal static (nint @base, @string digits) getBase(this ref ss s, rune verb) {
    nint @base = default!;
    @string digits = default!;

    s.okVerb(verb, bdoUxXvˢ, integerˢ); // sets s.err
    @base = 10;
    digits = decimalDigits;
    switch (verb) {
    case (rune)'b': {
        @base = 2;
        digits = binaryDigits;
        break;
    }
    case (rune)'o': {
        @base = 8;
        digits = octalDigits;
        break;
    }
    case (rune)'x' or (rune)'X' or (rune)'U': {
        @base = 16;
        digits = hexadecimalDigits;
        break;
    }}

    return (@base, digits);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string expectedIntegerˢ = "expected integer"u8;

// scanNumber returns the numerical string with specified digits starting here.
internal static @string scanNumber(this ref ss s, sstring digits, bool haveDigits) {
    if (!haveDigits) {
        s.notEOF();
        if (!s.accept(digits)) {
            s.errorString(expectedIntegerˢ);
        }
    }
    while (s.accept(digits)) {
    }
    return ((@string)(slice<byte>)s.buf);
}

// scanRune returns the next rune value in the input.
internal static int64 scanRune(this ref ss s, nint bitSize) {
    s.notEOF();
    var r = s.getRune();
    nuint n = (nuint)bitSize;
    var x = (((int64)r).Lsh((64 - n))).Rsh((64 - n));
    if (x != (int64)r) {
        s.errorString("overflow on character value "u8 + ((@string)r));
    }
    return (int64)r;
}

// scanBasePrefix reports whether the integer begins with a base prefix
// and returns the base, digit string, and whether a zero was found.
// It is called only if the verb is %v.
internal static (nint @base, @string digits, bool zeroFound) scanBasePrefix(this ref ss s) {
    if (!s.peek("0"u8)) {
        return (0, decimalDigits + "_", false);
    }
    s.accept("0"u8);
    // Special cases for 0, 0b, 0o, 0x.
    switch (ᐧ) {
    case {} when s.peek("bB"u8): {
        s.consume("bB"u8, true);
        return (0, binaryDigits + "_", true);
    }
    case {} when s.peek("oO"u8): {
        s.consume("oO"u8, true);
        return (0, octalDigits + "_", true);
    }
    case {} when s.peek("xX"u8): {
        s.consume("xX"u8, true);
        return (0, hexadecimalDigits + "_", true);
    }
    default: {
        return (0, octalDigits + "_", true);
    }}

}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badUnicodeFormatˢ = "bad unicode format "u8;

// scanInt returns the value of the integer represented by the next
// token, checking for overflow. Any error is stored in s.err.
internal static int64 scanInt(this ref ss s, rune verb, nint bitSize) {
    if (verb == (rune)'c') {
        return s.scanRune(bitSize);
    }
    s.SkipSpace();
    s.notEOF();
    var (@base, digits) = s.getBase(verb);
    var haveDigits = false;
    if (verb == (rune)'U'){
        if (!s.consume("U"u8, false) || !s.consume("+"u8, false)) {
            s.errorString(badUnicodeFormatˢ);
        }
    } else {
        s.accept(sign); // If there's a sign, it will be left in the token buffer.
        if (verb == (rune)'v') {
            (@base, digits, haveDigits) = s.scanBasePrefix();
        }
    }
    @string tok = s.scanNumber(digits, haveDigits);
    var (i, err) = strconv.ParseInt(tok, @base, 64);
    if (err != default!) {
        s.error(err);
    }
    nuint n = (nuint)bitSize;
    var x = (i.Lsh((64 - n))).Rsh((64 - n));
    if (x != i) {
        s.errorString("integer overflow on token "u8 + tok);
    }
    return i;
}

// scanUint returns the value of the unsigned integer represented
// by the next token, checking for overflow. Any error is stored in s.err.
internal static uint64 scanUint(this ref ss s, rune verb, nint bitSize) {
    if (verb == (rune)'c') {
        return (uint64)s.scanRune(bitSize);
    }
    s.SkipSpace();
    s.notEOF();
    var (@base, digits) = s.getBase(verb);
    var haveDigits = false;
    if (verb == (rune)'U'){
        if (!s.consume("U"u8, false) || !s.consume("+"u8, false)) {
            s.errorString(badUnicodeFormatˢ);
        }
    } else 
    if (verb == (rune)'v') {
        (@base, digits, haveDigits) = s.scanBasePrefix();
    }
    @string tok = s.scanNumber(digits, haveDigits);
    var (i, err) = strconv.ParseUint(tok, @base, 64);
    if (err != default!) {
        s.error(err);
    }
    nuint n = (nuint)bitSize;
    var x = (i.Lsh((64 - n))).Rsh((64 - n));
    if (x != i) {
        s.errorString("unsigned integer overflow on token "u8 + tok);
    }
    return i;
}

// floatToken returns the floating-point number starting here, no longer than swid
// if the width is specified. It's not rigorous about syntax because it doesn't check that
// we have at least some digits, but Atof will do that.
internal static @string floatToken(this ref ss s) {
    s.buf = s.buf[..0];
    // NaN?
    if (s.accept("nN"u8) && s.accept("aA"u8) && s.accept("nN"u8)) {
        return ((@string)(slice<byte>)s.buf);
    }
    // leading sign?
    s.accept(sign);
    // Inf?
    if (s.accept("iI"u8) && s.accept("nN"u8) && s.accept("fF"u8)) {
        return ((@string)(slice<byte>)s.buf);
    }
    @string digits = decimalDigits + "_";
    @string exp = exponent;
    if (s.accept("0"u8) && s.accept("xX"u8)) {
        digits = hexadecimalDigits + "_";
        exp = "pP"u8;
    }
    // digits?
    while (s.accept(digits)) {
    }
    // decimal point?
    if (s.accept(period)) {
        // fraction?
        while (s.accept(digits)) {
        }
    }
    // exponent?
    if (s.accept(exp)) {
        // leading sign?
        s.accept(sign);
        // digits?
        while (s.accept(decimalDigits + "_")) {
        }
    }
    return ((@string)(slice<byte>)s.buf);
}

// complexTokens returns the real and imaginary parts of the complex number starting here.
// The number might be parenthesized and has the format (N+Ni) where N is a floating-point
// number and there are no spaces within.
internal static (@string real, @string imag) complexTokens(this ref ss s) {
    @string real = default!;
    @string imag = default!;

    // TODO: accept N and Ni independently?
    var parens = s.accept("("u8);
    real = s.floatToken();
    s.buf = s.buf[..0];
    // Must now have a sign.
    if (!s.accept("+-"u8)) {
        s.error(errComplex);
    }
    // Sign is now in buffer
    @string imagSign = ((@string)(slice<byte>)s.buf);
    imag = s.floatToken();
    if (!s.accept("i"u8)) {
        s.error(errComplex);
    }
    if (parens && !s.accept(")"u8)) {
        s.error(errComplex);
    }
    return (real, imagSign + imag);
}

internal static bool hasX(sstring s) {
    for (nint i = 0; i < len(s); i++) {
        if (s[i] == (rune)'x' || s[i] == (rune)'X') {
            return true;
        }
    }
    return false;
}

// convertFloat converts the string to a float64value.
internal static float64 convertFloat(this ref ss s, @string str, nint n) {
    // strconv.ParseFloat will handle "+0x1.fp+2",
    // but we have to implement our non-standard
    // decimal+binary exponent mix (1.2p4) ourselves.
    {
        nint p = indexRune(str, (rune)'p'); if (p >= 0 && !hasX(str)) {
            // Atof doesn't handle power-of-2 exponents,
            // but they're easy to evaluate.
            var (fΔ1, errΔ1) = strconv.ParseFloat(str.slice(0, p), n);
            if (errΔ1 != default!) {
                // Put full string into error.
                {
                    var (e, ok) = errΔ1._<ж<strconv.NumError>>(ᐧ); if (ok) {
                        e.Value.Num = str;
                    }
                }
                s.error(errΔ1);
            }
            (var m, errΔ1) = strconv.Atoi(str.slice(p + 1));
            if (errΔ1 != default!) {
                // Put full string into error.
                {
                    var (e, ok) = errΔ1._<ж<strconv.NumError>>(ᐧ); if (ok) {
                        e.Value.Num = str;
                    }
                }
                s.error(errΔ1);
            }
            return Δmath.Ldexp(fΔ1, m);
        }
    }
    var (f, err) = strconv.ParseFloat(str, n);
    if (err != default!) {
        s.error(err);
    }
    return f;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string complexˢ = "complex"u8;

// scanComplex converts the next token to a complex128 value.
// The atof argument is a type-specific reader for the underlying type.
// If we're reading complex64, atof will parse float32s and convert them
// to float64's to avoid reproducing this code for each complex type.
internal static complex128 scanComplex(this ref ss s, rune verb, nint n) {
    if (!s.okVerb(verb, floatVerbs, complexˢ)) {
        return 0D;
    }
    s.SkipSpace();
    s.notEOF();
    var (sreal, simag) = s.complexTokens();
    var real = s.convertFloat(sreal, n / 2);
    var imag = s.convertFloat(simag, n / 2);
    return complex(real, imag);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string svqxXˢ = "svqxX"u8;
internal static readonly @string stringˢ2 = "string"u8;

// convertString returns the string represented by the next input characters.
// The format of the input is determined by the verb.
internal static @string /*str*/ convertString(this ref ss s, rune verb) {
    @string str = default!;

    if (!s.okVerb(verb, svqxXˢ, stringˢ2)) {
        return ""u8;
    }
    s.SkipSpace();
    s.notEOF();
    switch (verb) {
    case (rune)'q': {
        str = s.quotedString();
        break;
    }
    case (rune)'x' or (rune)'X': {
        str = s.hexString();
        break;
    }
    default: {
        str = ((@string)s.token(true, notSpace)); // %s and %v just return the next word
        break;
    }}

    return str;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string expectedQuotedStringˢ = "expected quoted string"u8;

// quotedString returns the double- or back-quoted string represented by the next input characters.
internal static @string quotedString(this ref ss s) {
    s.notEOF();
    var quote = s.getRune();
    switch (quote) {
    case (rune)'`': {
        while (ᐧ) {
            // Back-quoted: Anything goes until EOF or back quote.
            var r = s.mustReadRune();
            if (r == quote) {
                break;
            }
            s.buf.writeRune(r);
        }
        return ((@string)(slice<byte>)s.buf);
    }
    case (rune)'"': {
        s.buf.writeByte((rune)'"');
        while (ᐧ) {
            // Double-quoted: Include the quotes and let strconv.Unquote do the backslash escapes.
            var r = s.mustReadRune();
            s.buf.writeRune(r);
            if (r == (rune)'\\'){
                // In a legal backslash escape, no matter how long, only the character
                // immediately after the escape can itself be a backslash or quote.
                // Thus we only need to protect the first character after the backslash.
                s.buf.writeRune(s.mustReadRune());
            } else 
            if (r == (rune)'"') {
                break;
            }
        }
        var (result, err) = strconv.Unquote(((@string)(slice<byte>)s.buf));
        if (err != default!) {
            s.error(err);
        }
        return result;
    }
    default: {
        s.errorString(expectedQuotedStringˢ);
        break;
    }}

    return ""u8;
}

// hexDigit returns the value of the hexadecimal digit.
internal static (nint, bool) hexDigit(rune d) {
    nint digit = (nint)d;
    switch (digit) {
    case (rune)'0' or (rune)'1' or (rune)'2' or (rune)'3' or (rune)'4' or (rune)'5' or (rune)'6' or (rune)'7' or (rune)'8' or (rune)'9': {
        return (digit - (rune)'0', true);
    }
    case (rune)'a' or (rune)'b' or (rune)'c' or (rune)'d' or (rune)'e' or (rune)'f': {
        return (10 + digit - (rune)'a', true);
    }
    case (rune)'A' or (rune)'B' or (rune)'C' or (rune)'D' or (rune)'E' or (rune)'F': {
        return (10 + digit - (rune)'A', true);
    }}

    return (-1, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string illegalHexDigitˢ = "illegal hex digit"u8;

// hexByte returns the next hex-encoded (two-character) byte from the input.
// It returns ok==false if the next bytes in the input do not encode a hex byte.
// If the first byte is hex and the second is not, processing stops.
internal static (byte b, bool ok) hexByte(this ref ss s) {
    byte b = default!;
    bool ok = default!;

    var rune1 = s.getRune();
    if (rune1 == eof) {
        return (b, ok);
    }
    (var value1, ok) = hexDigit(rune1);
    if (!ok) {
        s.UnreadRune();
        return (b, ok);
    }
    (var value2, ok) = hexDigit(s.mustReadRune());
    if (!ok) {
        s.errorString(illegalHexDigitˢ);
        return (b, ok);
    }
    return ((byte)((nint)((value1 << (int)(4)) | value2)), true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noHexDataForXStringˢ = "no hex data for %x string"u8;

// hexString returns the space-delimited hexpair-encoded string.
internal static @string hexString(this ref ss s) {
    s.notEOF();
    while (ᐧ) {
        var (b, ok) = s.hexByte();
        if (!ok) {
            break;
        }
        s.buf.writeByte(b);
    }
    if (len(s.buf) == 0) {
        s.errorString(noHexDataForXStringˢ);
        return ""u8;
    }
    return ((@string)(slice<byte>)s.buf);
}

internal static readonly @string floatVerbs = "beEfFgGv"u8;
internal static UntypedInt hugeWid => /* 1 << 30 */ 1073741824;
internal static UntypedInt intBits => /* 32 << (^uint(0) >> 63) */ 64;
internal static UntypedInt uintptrBits => /* 32 << (^uintptr(0) >> 63) */ 64;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingLiteralˢ = "missing literal %"u8;

// scanPercent scans a literal percent character.
internal static void scanPercent(this ref ss s) {
    s.SkipSpace();
    s.notEOF();
    if (!s.accept("%"u8)) {
        s.errorString(missingLiteralˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string float32ˢ = "float32"u8;
internal static readonly @string float64ˢ = "float64"u8;

// scanOne scans a single value, deriving the scanner from the type of the argument.
internal static void scanOne(this ж<ss> Ꮡs, rune verb, any arg) {
    ref var s = ref Ꮡs.DerefOrNull();

    s.buf = s.buf[..0];
    error err = default!;
    // If the parameter has its own Scan method, use that.
    {
        var (v, ok) = arg._<Scanner>(ᐧ); if (ok) {
            err = v.Scan(new ssжScanState(Ꮡs), verb);
            if (err != default!) {
                if (AreEqual(err, Δio.EOF)) {
                    err = Δio.ErrUnexpectedEOF;
                }
                s.error(err);
            }
            return;
        }
    }
    switch (arg.type()) {
    case ж<bool> v: {
        v.Value = s.scanBool(verb);
        break;
    }
    case ж<complex64> v: {
        v.Value = (complex64)s.scanComplex(verb, 64);
        break;
    }
    case ж<complex128> v: {
        v.Value = s.scanComplex(verb, 128);
        break;
    }
    case ж<nint> v: {
        v.Value = (nint)s.scanInt(verb, intBits);
        break;
    }
    case ж<int8> v: {
        v.Value = (int8)s.scanInt(verb, 8);
        break;
    }
    case ж<int16> v: {
        v.Value = (int16)s.scanInt(verb, 16);
        break;
    }
    case ж<int32> v: {
        v.Value = (int32)s.scanInt(verb, 32);
        break;
    }
    case ж<int64> v: {
        v.Value = s.scanInt(verb, 64);
        break;
    }
    case ж<nuint> v: {
        v.Value = (nuint)s.scanUint(verb, intBits);
        break;
    }
    case ж<uint8> v: {
        v.Value = (uint8)s.scanUint(verb, 8);
        break;
    }
    case ж<uint16> v: {
        v.Value = (uint16)s.scanUint(verb, 16);
        break;
    }
    case ж<uint32> v: {
        v.Value = (uint32)s.scanUint(verb, 32);
        break;
    }
    case ж<uint64> v: {
        v.Value = s.scanUint(verb, 64);
        break;
    }
    case ж<uintptr> v: {
        v.Value = (uintptr)s.scanUint(verb, uintptrBits);
        break;
    }
    case ж<float32> v: {
        if (s.okVerb(verb, // Floats are tricky because you want to scan in the precision of the result, not
 // scan in high precision and convert, in order to preserve the correct error condition.
 floatVerbs, float32ˢ)) {
            s.SkipSpace();
            s.notEOF();
            v.Value = (float32)s.convertFloat(s.floatToken(), 32);
        }
        break;
    }
    case ж<float64> v: {
        if (s.okVerb(verb, floatVerbs, float64ˢ)) {
            s.SkipSpace();
            s.notEOF();
            v.Value = s.convertFloat(s.floatToken(), 64);
        }
        break;
    }
    case ж<@string> v: {
        v.Value = s.convertString(verb);
        break;
    }
    case ж<slice<byte>> v: {
        v.ValueSlot = slice<byte>(s.convertString(verb));
        break;
    }
    default: {
        var v = arg;
        var val = reflect.ValueOf(v);
        var ptr = val;
        if (ptr.Kind() != reflect.ΔPointer) {
            // We scan to string and convert so we get a copy of the data.
            // If we scanned to bytes, the slice would point at the buffer.
            s.errorString("type not a pointer: "u8 + val.Type().String());
            return;
        }
        {
            var vΔ1 = ptr.Elem();
            var exprᴛ1 = vΔ1.Kind();
            if (exprᴛ1 == reflect.ΔBool) {
                vΔ1.SetBool(s.scanBool(verb));
            }
            else if (exprᴛ1 == reflect.ΔInt || exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64) {
                vΔ1.SetInt(s.scanInt(verb, vΔ1.Type().Bits()));
            }
            else if (exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.Uintptr) {
                vΔ1.SetUint(s.scanUint(verb, vΔ1.Type().Bits()));
            }
            else if (exprᴛ1 == reflect.ΔString) {
                vΔ1.SetString(s.convertString(verb));
            }
            else if (exprᴛ1 == reflect.ΔSlice) {
                var typ = vΔ1.Type();
                if (typ.Elem().Kind() != reflect.Uint8) {
                    // For now, can only handle (renamed) []byte.
                    s.errorString("can't scan type: "u8 + val.Type().String());
                }
                @string str = s.convertString(verb);
                vΔ1.Set(reflect.MakeSlice(typ, len(str), len(str)));
                for (nint i = 0; i < len(str); i++) {
                    vΔ1.Index(i).SetUint((uint64)str[i]);
                }
            }
            else if (exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64) {
                s.SkipSpace();
                s.notEOF();
                vΔ1.SetFloat(s.convertFloat(s.floatToken(), vΔ1.Type().Bits()));
            }
            else if (exprᴛ1 == reflect.Complex64 || exprᴛ1 == reflect.Complex128) {
                vΔ1.SetComplex(s.scanComplex(verb, vΔ1.Type().Bits()));
            }
            else { /* default: */
                s.errorString("can't scan type: "u8 + val.Type().String());
            }
        }

        break;
    }}
}

// errorHandler turns local panics into error returns.
internal static void errorHandler(ref error errp) {
    GoFrame ᒐ = default;
    try {
        {
            var e = recover(); if (e != default!) {
                {
                    var (se, ok) = e._<scanError>(ᐧ); if (ok){
                        // catch local error
                        errp = se.err;
                    } else 
                    {
                        var (eof, okΔ1) = e._<error>(ᐧ); if (okΔ1 && AreEqual(eof, Δio.EOF)){
                            // out of input
                            errp = eof;
                        } else {
                            throw panic(e);
                        }
                    }
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string expectedNewlineˢ = "expected newline"u8;

// doScan does the real work for scanning without a format string.
internal static (nint numProcessed, error err) doScan(this ж<ss> Ꮡs, slice<any> a) {
    nint numProcessed = default!;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        ref var err = ref Ꮡerr.ValueSlot;
        defer(ᴛ1 => errorHandler(ref ᴛ1.DerefOrNull()), Ꮡerr, ref ᒐ);
        foreach (var (_, arg) in a) {
            Ꮡs.scanOne((rune)'v', arg);
            numProcessed++;
        }
        // Check for newline (or EOF) if required (Scanln etc.).
        if (s.nlIsEnd) {
            while (ᐧ) {
                var r = s.getRune();
                if (r == (rune)'\n' || r == eof) {
                    break;
                }
                if (!isSpace(r)) {
                    s.errorString(expectedNewlineˢ);
                    break;
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (numProcessed, Ꮡerr.ValueSlot);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string newlineInFormatDoesNotˢ = "newline in format does not match input"u8;
internal static readonly @string expectedSpaceInInputToˢ = "expected space in input to match format"u8;
internal static readonly @string newlineInInputDoesNotˢ = "newline in input does not match format"u8;
internal static readonly @string missingVerbAtEndOfFormatˢ = "missing verb: % at end of format string"u8;

// advance determines whether the next characters in the input match
// those of the format. It returns the number of bytes (sic) consumed
// in the format. All runs of space characters in either input or
// format behave as a single space. Newlines are special, though:
// newlines in the format must match those in the input and vice versa.
// This routine also handles the %% case. If the return value is zero,
// either format starts with a % (with no following %) or the input
// is empty. If it is negative, the input did not match the string.
internal static nint /*i*/ advance(this ref ss s, sstring format) {
    nint i = default!;

    while (i < len(format)) {
        var (fmtc, w) = utf8.DecodeRuneInString(format.slice(i));
        // Space processing.
        // In the rest of this comment "space" means spaces other than newline.
        // Newline in the format matches input of zero or more spaces and then newline or end-of-input.
        // Spaces in the format before the newline are collapsed into the newline.
        // Spaces in the format after the newline match zero or more spaces after the corresponding input newline.
        // Other spaces in the format match input of one or more spaces or end-of-input.
        if (isSpace(fmtc)) {
            nint newlines = 0;
            var trailingSpace = false;
            while (isSpace(fmtc) && i < len(format)) {
                if (fmtc == (rune)'\n'){
                    newlines++;
                    trailingSpace = false;
                } else {
                    trailingSpace = true;
                }
                i += w;
                (fmtc, w) = utf8.DecodeRuneInString(format.slice(i));
            }
            for (nint j = 0; j < newlines; j++) {
                var inputcΔ1 = s.getRune();
                while (isSpace(inputcΔ1) && inputcΔ1 != (rune)'\n') {
                    inputcΔ1 = s.getRune();
                }
                if (inputcΔ1 != (rune)'\n' && inputcΔ1 != eof) {
                    s.errorString(newlineInFormatDoesNotˢ);
                }
            }
            if (trailingSpace) {
                var inputcΔ2 = s.getRune();
                if (newlines == 0) {
                    // If the trailing space stood alone (did not follow a newline),
                    // it must find at least one space to consume.
                    if (!isSpace(inputcΔ2) && inputcΔ2 != eof) {
                        s.errorString(expectedSpaceInInputToˢ);
                    }
                    if (inputcΔ2 == (rune)'\n') {
                        s.errorString(newlineInInputDoesNotˢ);
                    }
                }
                while (isSpace(inputcΔ2) && inputcΔ2 != (rune)'\n') {
                    inputcΔ2 = s.getRune();
                }
                if (inputcΔ2 != eof) {
                    s.UnreadRune();
                }
            }
            continue;
        }
        // Verbs.
        if (fmtc == (rune)'%') {
            // % at end of string is an error.
            if (i + w == len(format)) {
                s.errorString(missingVerbAtEndOfFormatˢ);
            }
            // %% acts like a real percent
            var (nextc, _) = utf8.DecodeRuneInString(format.slice(i + w)); // will not match % if string is empty
            if (nextc != (rune)'%') {
                return i;
            }
            i += w; // skip the first %
        }
        // Literals.
        var inputc = s.mustReadRune();
        if (fmtc != inputc) {
            s.UnreadRune();
            return -1;
        }
        i += w;
    }
    return i;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string inputDoesNotMatchFormatˢ = "input does not match format"u8;
internal static readonly @string tooManyOperandsˢ = "too many operands"u8;

// doScanf does the real work when scanning with a format string.
// At the moment, it handles only pointers to basic types.
internal static (nint numProcessed, error err) doScanf(this ж<ss> Ꮡs, sstring format, slice<any> a) {
    nint numProcessed = default!;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var s = ref Ꮡs.DerefOrNull();

        ref var err = ref Ꮡerr.ValueSlot;
        defer(ᴛ1 => errorHandler(ref ᴛ1.DerefOrNull()), Ꮡerr, ref ᒐ);
        nint end = len(format) - 1;
        // We process one item per non-trivial format
        for (nint i = 0; i <= end; ) {
            nint w = s.advance(format.slice(i));
            if (w > 0) {
                i += w;
                continue;
            }
            // Either we failed to advance, we have a percent character, or we ran out of input.
            if (format[i] != (rune)'%') {
                // Can't advance format. Why not?
                if (w < 0) {
                    s.errorString(inputDoesNotMatchFormatˢ);
                }
                // Otherwise at EOF; "too many operands" error handled below
                break;
            }
            i++; // % is one byte
            // do we have 20 (width)?
            bool widPresent = default!;
            (s.maxWid, widPresent, i) = parsenum(format, i, end);
            if (!widPresent) {
                s.maxWid = hugeWid;
            }
            (var c, w) = utf8.DecodeRuneInString(format.slice(i));
            i += w;
            if (c != (rune)'c') {
                s.SkipSpace();
            }
            if (c == (rune)'%') {
                s.scanPercent();
                continue; // Do not consume an argument.
            }
            s.argLimit = s.limit;
            {
                nint f = s.count + s.maxWid; if (f < s.argLimit) {
                    s.argLimit = f;
                }
            }
            if (numProcessed >= len(a)) {
                // out of operands
                s.errorString("too few operands for format '%" + format.slice(i - w) + "'");
                break;
            }
            var arg = a[numProcessed];
            Ꮡs.scanOne(c, arg);
            numProcessed++;
            s.argLimit = s.limit;
        }
        if (numProcessed < len(a)) {
            s.errorString(tooManyOperandsˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (numProcessed, Ꮡerr.ValueSlot);
}

} // end fmt_package
