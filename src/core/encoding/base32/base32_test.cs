// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.encoding;

using bytes = bytes_package;
using errors = errors_package;
using io = io_package;
using math = math_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.encoding.base32_package;
using ꓸꓸꓸany = Span<any>;

partial class base32_internal_test_package {

internal partial struct testpair {
    internal @string decoded, encoded;
}

internal static slice<testpair> pairs = slice<testpair>([
    // RFC 4648 examples
    new("", ""),
    new("f", "MY======"),
    new("fo", "MZXQ===="),
    new("foo", "MZXW6==="),
    new("foob", "MZXW6YQ="),
    new("fooba", "MZXW6YTB"),
    new("foobar", "MZXW6YTBOI======"),

    // Wikipedia examples, converted to base32
    new("sure.", "ON2XEZJO"),
    new("sure", "ON2XEZI="),
    new("sur", "ON2XE==="),
    new("su", "ON2Q===="),
    new("leasure.", "NRSWC43VOJSS4==="),
    new("easure.", "MVQXG5LSMUXA===="),
    new("asure.", "MFZXK4TFFY======"),
    new("sure.", "ON2XEZJO")
]);

internal static testpair bigtest = new testpair(
    "Twas brillig, and the slithy toves",
    "KR3WC4ZAMJZGS3DMNFTSYIDBNZSCA5DIMUQHG3DJORUHSIDUN53GK4Y="
);

internal static bool testEqual(ж<testing.T> t, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    t.Helper();
    if (!AreEqual(args[len(args) - 2], args[len(args) - 1])) {
        t.Errorf(msg, args.ꓸꓸꓸ);
        return false;
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string encodeQQWantQˢ => ref encodeQQWantQˢᵇ;
private static readonly @string encodeQQWantQˢᵇ = "Encode(%q) = %q, want %q"u8;
internal static ref readonly @string appendEncodeLeadQQWantQˢ => ref appendEncodeLeadQQWantQˢᵇ;
private static readonly @string appendEncodeLeadQQWantQˢᵇ = @"AppendEncode(""lead"", %q) = %q, want %q"u8;

public static void TestEncode(ж<testing.T> t) {
    foreach (var (_, p) in pairs) {
        @string got = StdEncoding.EncodeToString(slice<byte>(p.decoded));
        testEqual(t, encodeQQWantQˢ, p.decoded, got, p.encoded);
        var dst = StdEncoding.AppendEncode(slice<byte>("lead"u8), slice<byte>(p.decoded));
        testEqual(t, appendEncodeLeadQQWantQˢ, p.decoded, (@string)dst, "lead" + p.encoded);
    }
}

public static void TestEncoder(ж<testing.T> t) {
    foreach (var (_, p) in pairs) {
        var bb = Ꮡ(new strings.Builder());
        var encoder = NewEncoder(StdEncoding, new base32_test_package.strings_BuilderжWriter(bb));
        encoder.Write(slice<byte>(p.decoded));
        encoder.Close();
        testEqual(t, encodeQQWantQˢ, p.decoded, bb.String(), p.encoded);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string writeQGaveErrorVWantVˢ => ref writeQGaveErrorVWantVˢᵇ;
private static readonly @string writeQGaveErrorVWantVˢᵇ = "Write(%q) gave error %v, want %v"u8;
internal static ref readonly @string writeQGaveLengthVWantVˢ => ref writeQGaveLengthVWantVˢᵇ;
private static readonly @string writeQGaveLengthVWantVˢᵇ = "Write(%q) gave length %v, want %v"u8;
internal static ref readonly @string closeGaveErrorVWantVˢ => ref closeGaveErrorVWantVˢᵇ;
private static readonly @string closeGaveErrorVWantVˢᵇ = "Close gave error %v, want %v"u8;
internal static ref readonly @string encodingDOfQQWantQˢ => ref encodingDOfQQWantQˢᵇ;
private static readonly @string encodingDOfQQWantQˢᵇ = "Encoding/%d of %q = %q, want %q"u8;

public static void TestEncoderBuffering(ж<testing.T> t) {
    var input = slice<byte>(bigtest.decoded);
    for (nint bs = 1; bs <= 12; bs++) {
        var bb = Ꮡ(new strings.Builder());
        var encoder = NewEncoder(StdEncoding, new base32_test_package.strings_BuilderжWriter(bb));
        for (nint pos = 0; pos < len(input); pos += bs) {
            nint end = pos + bs;
            if (end > len(input)) {
                end = len(input);
            }
            var (n, errΔ1) = encoder.Write(input.slice(pos, end));
            testEqual(t, writeQGaveErrorVWantVˢ, input.slice(pos, end), errΔ1, (error)default);
            testEqual(t, writeQGaveLengthVWantVˢ, input.slice(pos, end), n, end - pos);
        }
        var err = encoder.Close();
        testEqual(t, closeGaveErrorVWantVˢ, err, (error)default);
        testEqual(t, encodingDOfQQWantQˢ, bs, bigtest.decoded, bb.String(), bigtest.encoded);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string decodingDOfQQWantQˢ => ref decodingDOfQQWantQˢᵇ;
private static readonly @string decodingDOfQQWantQˢᵇ = "Decoding/%d of %q = %q, want %q\n"u8;

public static void TestDecoderBufferingWithPadding(ж<testing.T> t) {
    for (nint bs = 0; bs <= 12; bs++) {
        foreach (var (_, s) in pairs) {
            var decoder = NewDecoder(StdEncoding, new base32_test_package.strings_ReaderжReader(strings.NewReader(s.encoded)));
            var buf = new slice<byte>(len(s.decoded) + bs);

            nint n = default;
            error err = default;
            (n, err) = decoder.Read(buf);

            if (err != nil && !AreEqual(err, io.EOF)) {
                t.Errorf("Read from %q at pos %d = %d, unexpected error %v", s.encoded, len(s.decoded), n, err);
            }
            testEqual(t, decodingDOfQQWantQˢ, bs, s.encoded, (@string)(buf.slice(0, n)), s.decoded);
        }
    }
}

public static void TestDecoderBufferingWithoutPadding(ж<testing.T> t) {
    for (nint bs = 0; bs <= 12; bs++) {
        foreach (var (_, s) in pairs) {
            @string encoded = strings.TrimRight(s.encoded, "=");
            var decoder = NewDecoder((~StdEncoding).WithPadding(NoPadding), new base32_test_package.strings_ReaderжReader(strings.NewReader(encoded)));
            var buf = new slice<byte>(len(s.decoded) + bs);

            nint n = default;
            error err = default;
            (n, err) = decoder.Read(buf);

            if (err != nil && !AreEqual(err, io.EOF)) {
                t.Errorf("Read from %q at pos %d = %d, unexpected error %v", encoded, len(s.decoded), n, err);
            }
            testEqual(t, decodingDOfQQWantQˢ, bs, encoded, (@string)(buf.slice(0, n)), s.decoded);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string decodeQErrorVWantVˢ => ref decodeQErrorVWantVˢᵇ;
private static readonly @string decodeQErrorVWantVˢᵇ = "Decode(%q) = error %v, want %v"u8;
internal static ref readonly @string decodeQLengthVWantVˢ => ref decodeQLengthVWantVˢᵇ;
private static readonly @string decodeQLengthVWantVˢᵇ = "Decode(%q) = length %v, want %v"u8;
internal static ref readonly @string decodeQEndVWantVˢ => ref decodeQEndVWantVˢᵇ;
private static readonly @string decodeQEndVWantVˢᵇ = "Decode(%q) = end %v, want %v"u8;
internal static ref readonly @string decodeQQWantQˢ => ref decodeQQWantQˢᵇ;
private static readonly @string decodeQQWantQˢᵇ = "Decode(%q) = %q, want %q"u8;
internal static ref readonly @string decodeStringQErrorVWantVˢ => ref decodeStringQErrorVWantVˢᵇ;
private static readonly @string decodeStringQErrorVWantVˢᵇ = "DecodeString(%q) = error %v, want %v"u8;
internal static ref readonly @string decodeStringQQWantQˢ => ref decodeStringQQWantQˢᵇ;
private static readonly @string decodeStringQQWantQˢᵇ = "DecodeString(%q) = %q, want %q"u8;
internal static ref readonly @string appendDecodeQErrorVWantVˢ => ref appendDecodeQErrorVWantVˢᵇ;
private static readonly @string appendDecodeQErrorVWantVˢᵇ = "AppendDecode(%q) = error %v, want %v"u8;
internal static ref readonly @string appendDecodeLeadQQWantQˢ => ref appendDecodeLeadQQWantQˢᵇ;
private static readonly @string appendDecodeLeadQQWantQˢᵇ = @"AppendDecode(""lead"", %q) = %q, want %q"u8;
internal static ref readonly @string appendDecodeQQWantQˢ => ref appendDecodeQQWantQˢᵇ;
private static readonly @string appendDecodeQQWantQˢᵇ = @"AppendDecode("""", %q) = %q, want %q"u8;

public static void TestDecode(ж<testing.T> t) {
    foreach (var (_, p) in pairs) {
        var dbuf = new slice<byte>(StdEncoding.DecodedLen(len(p.encoded)));
        var (count, end, err) = StdEncoding.decode(dbuf, slice<byte>(p.encoded));
        testEqual(t, decodeQErrorVWantVˢ, p.encoded, err, (error)default);
        testEqual(t, decodeQLengthVWantVˢ, p.encoded, count, len(p.decoded));
        if (len(p.encoded) > 0) {
            testEqual(t, decodeQEndVWantVˢ, p.encoded, end, (p.encoded[len(p.encoded) - 1] == '='));
        }
        testEqual(t, decodeQQWantQˢ, p.encoded, (@string)(dbuf.slice(0, count)), p.decoded);

        (dbuf, err) = StdEncoding.DecodeString(p.encoded);
        testEqual(t, decodeStringQErrorVWantVˢ, p.encoded, err, (error)default);
        testEqual(t, decodeStringQQWantQˢ, p.encoded, (@string)dbuf, p.decoded);

        (var dst, err) = StdEncoding.AppendDecode(slice<byte>("lead"u8), slice<byte>(p.encoded));
        testEqual(t, appendDecodeQErrorVWantVˢ, p.encoded, err, (error)default);
        testEqual(t, appendDecodeLeadQQWantQˢ, p.encoded, (@string)dst, "lead" + p.decoded);

        (var dst2, err) = StdEncoding.AppendDecode(dst.slice(0, 0, len(p.decoded)), slice<byte>(p.encoded));
        testEqual(t, appendDecodeQErrorVWantVˢ, p.encoded, err, (error)default);
        testEqual(t, appendDecodeQQWantQˢ, p.encoded, (@string)dst2, p.decoded);
        if (len(dst) > 0 && len(dst2) > 0 && Ꮡelem(dst, 0) != Ꮡelem(dst2, 0)) {
            t.Errorf("unexpected capacity growth: got %d, want %d", cap(dst2), cap(dst));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readFailedˢ = (@string)"Read failed"u8;
internal static ref readonly @string readFromQLengthVWantVˢ => ref readFromQLengthVWantVˢᵇ;
private static readonly @string readFromQLengthVWantVˢᵇ = "Read from %q = length %v, want %v"u8;
internal static ref readonly @string decodingOfQQWantQˢ => ref decodingOfQQWantQˢᵇ;
private static readonly @string decodingOfQQWantQˢᵇ = "Decoding of %q = %q, want %q"u8;
internal static ref readonly @string readFromQVWantVˢ => ref readFromQVWantVˢᵇ;
private static readonly @string readFromQVWantVˢᵇ = "Read from %q = %v, want %v"u8;

public static void TestDecoder(ж<testing.T> t) {
    foreach (var (_, p) in pairs) {
        var decoder = NewDecoder(StdEncoding, new base32_test_package.strings_ReaderжReader(strings.NewReader(p.encoded)));
        var dbuf = new slice<byte>(StdEncoding.DecodedLen(len(p.encoded)));
        var (count, err) = decoder.Read(dbuf);
        if (err != nil && !AreEqual(err, io.EOF)) {
            t.Fatal(readFailedˢ, err);
        }
        testEqual(t, readFromQLengthVWantVˢ, p.encoded, count, len(p.decoded));
        testEqual(t, decodingOfQQWantQˢ, p.encoded, (@string)(dbuf.slice(0, count)), p.decoded);
        if (!AreEqual(err, io.EOF)) {
            (_, err) = decoder.Read(dbuf);
        }
        testEqual(t, readFromQVWantVˢ, p.encoded, err, io.EOF);
    }
}

internal partial struct badReader {
    internal slice<byte> data;
    internal slice<error> errs;
    internal nint called;
    internal nint limit;
}

// Populates p with data, returns a count of the bytes written and an
// error.  The error returned is taken from badReader.errs, with each
// invocation of Read returning the next error in this slice, or io.EOF,
// if all errors from the slice have already been returned.  The
// number of bytes returned is determined by the size of the input buffer
// the test passes to decoder.Read and will be a multiple of 8, unless
// badReader.limit is non zero.
internal static (nint, error) Read(this ref badReader b, slice<byte> p) {
    nint lim = len(p);
    if (b.limit != 0 && b.limit < lim) {
        lim = b.limit;
    }
    if (len(b.data) < lim) {
        lim = len(b.data);
    }
    foreach (var i in range(p.slice(0, lim))) {
        p[i] = b.data[i];
    }
    b.data = b.data.slice(lim);
    var err = io.EOF;
    if (b.called < len(b.errs)) {
        err = b.errs[b.called];
    }
    b.called++;
    return (lim, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string badReaderErrorˢ => ref badReaderErrorˢᵇ;
private static readonly @string badReaderErrorˢᵇ = "bad reader error"u8;
internal static ref readonly @string decodingOfQErrVExpectedVˢ => ref decodingOfQErrVExpectedVˢᵇ;
private static readonly @string decodingOfQErrVExpectedVˢᵇ = "Decoding of %q err = %v, expected %v"u8;

internal partial struct TestIssue20044_testCases /*dyn*/ {
    internal badReader r;
    internal @string res;
    internal error err;
    internal nint dbuflen;
}

// TestIssue20044 tests that decoder.Read behaves correctly when the caller
// supplied reader returns an error.
public static void TestIssue20044(ж<testing.T> t) {
    var badErr = errors.New(badReaderErrorˢ);
    var testCases = slice<TestIssue20044_testCases>([
        // Check valid input data accompanied by an error is processed and the error is propagated.
        new(r: new badReader(data: slice<byte>("MY======"u8), errs: slice<error>([badErr])),
            res: "f", err: badErr),
        // Check a read error accompanied by input data consisting of newlines only is propagated.
        new(r: new badReader(data: slice<byte>("\n\n\n\n\n\n\n\n"u8), errs: slice<error>([badErr, default])),
            res: "", err: badErr),
        // Reader will be called twice.  The first time it will return 8 newline characters.  The
        // second time valid base32 encoded data and an error.  The data should be decoded
        // correctly and the error should be propagated.
        new(r: new badReader(data: slice<byte>("\n\n\n\n\n\n\n\nMY======"u8), errs: slice<error>([default, badErr])),
            res: "f", err: badErr, dbuflen: 8),
        // Reader returns invalid input data (too short) and an error.  Verify the reader
        // error is returned.
        new(r: new badReader(data: slice<byte>("MY====="u8), errs: slice<error>([badErr])),
            res: "", err: badErr),
        // Reader returns invalid input data (too short) but no error.  Verify io.ErrUnexpectedEOF
        // is returned.
        new(r: new badReader(data: slice<byte>("MY====="u8), errs: slice<error>([default])),
            res: "", err: io.ErrUnexpectedEOF),
        // Reader returns invalid input data and an error.  Verify the reader and not the
        // decoder error is returned.
        new(r: new badReader(data: slice<byte>("Ma======"u8), errs: slice<error>([badErr])),
            res: "", err: badErr),
        // Reader returns valid data and io.EOF.  Check data is decoded and io.EOF is propagated.
        new(r: new badReader(data: slice<byte>("MZXW6YTB"u8), errs: slice<error>([io.EOF])),
            res: "fooba", err: io.EOF),
        // Check errors are properly reported when decoder.Read is called multiple times.
        // decoder.Read will be called 8 times, badReader.Read will be called twice, returning
        // valid data both times but an error on the second call.
        new(r: new badReader(data: slice<byte>("NRSWC43VOJSS4==="u8), errs: slice<error>([default, badErr])),
            res: "leasure.", err: badErr, dbuflen: 1),
        // Check io.EOF is properly reported when decoder.Read is called multiple times.
        // decoder.Read will be called 8 times, badReader.Read will be called twice, returning
        // valid data both times but io.EOF on the second call.
        new(r: new badReader(data: slice<byte>("NRSWC43VOJSS4==="u8), errs: slice<error>([default, io.EOF])),
            res: "leasure.", err: io.EOF, dbuflen: 1),
        // The following two test cases check that errors are propagated correctly when more than
        // 8 bytes are read at a time.
        new(r: new badReader(data: slice<byte>("NRSWC43VOJSS4==="u8), errs: slice<error>([io.EOF])),
            res: "leasure.", err: io.EOF, dbuflen: 11),
        new(r: new badReader(data: slice<byte>("NRSWC43VOJSS4==="u8), errs: slice<error>([badErr])),
            res: "leasure.", err: badErr, dbuflen: 11),
        // Check that errors are correctly propagated when the reader returns valid bytes in
        // groups that are not divisible by 8.  The first read will return 11 bytes and no
        // error.  The second will return 7 and an error.  The data should be decoded correctly
        // and the error should be propagated.
        new(r: new badReader(data: slice<byte>("NRSWC43VOJSS4==="u8), errs: slice<error>([default, badErr]), limit: 11),
            res: "leasure.", err: badErr)
    ]);

    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestIssue20044_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var input = tc.r.data;
        var decoder = NewDecoder(StdEncoding, new base32_internal_test_package.badReaderжReader(Ꮡtc.of(TestIssue20044_testCases.Ꮡr)));
        nint dbuflen = default;
        if (tc.dbuflen > 0) {
            dbuflen = tc.dbuflen;
        } else {
            dbuflen = StdEncoding.DecodedLen(len(input));
        }
        var dbuf = new slice<byte>(dbuflen);
        error err = default;
        slice<byte> res = default;
        while (err == nil) {
            nint n = default;
            (n, err) = decoder.Read(dbuf);
            if (n > 0) {
                res = appendꓸꓸꓸ(res, dbuf.slice(0, n));
            }
        }

        testEqual(t, decodingOfQQWantQˢ, (@string)input, (@string)res, tc.res);
        testEqual(t, decodingOfQErrVExpectedVˢ, (@string)input, err, tc.err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mzxw6YTbˢ => ref mzxw6YTbˢᵇ;
private static readonly @string mzxw6YTbˢᵇ = "MZXW6YTb"u8;
internal static ref readonly @string readAfterEofNDExpectedDˢ => ref readAfterEofNDExpectedDˢᵇ;
private static readonly @string readAfterEofNDExpectedDˢᵇ = "Read after EOF, n = %d, expected %d"u8;

// TestDecoderError verifies decode errors are propagated when there are no read
// errors.
public static void TestDecoderError(ж<testing.T> t) {
    foreach (var (_, readErr) in slice<error>([io.EOF, default])) {
        @string input = mzxw6YTbˢ;
        var dbuf = new slice<byte>(StdEncoding.DecodedLen(len(input)));
        ref var br = ref heap<badReader>(new badReader(data: slice<byte>(input), errs: slice<error>([readErr])), out var Ꮡbr);
        var decoder = NewDecoder(StdEncoding, new base32_internal_test_package.badReaderжReader(Ꮡbr));
        var (n, err) = decoder.Read(dbuf);
        testEqual(t, readAfterEofNDExpectedDˢ, n, (nint)0);
        {
            var (_, ok) = err._<CorruptInputError>(ᐧ); if (!ok) {
                t.Errorf("Corrupt input error expected.  Found %T", err);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string mzxw6ytbˢ => ref mzxw6ytbˢᵇ;
private static readonly @string mzxw6ytbˢᵇ = "MZXW6YTB"u8;
internal static ref readonly @string readAfterEofErrVExpectedˢ => ref readAfterEofErrVExpectedˢᵇ;
private static readonly @string readAfterEofErrVExpectedˢᵇ = "Read after EOF, err = %v, expected %v"u8;

// TestReaderEOF ensures decoder.Read behaves correctly when input data is
// exhausted.
public static void TestReaderEOF(ж<testing.T> t) {
    foreach (var (_, readErr) in slice<error>([io.EOF, default])) {
        @string input = mzxw6ytbˢ;
        ref var br = ref heap<badReader>(new badReader(data: slice<byte>(input), errs: slice<error>([default, readErr])), out var Ꮡbr);
        var decoder = NewDecoder(StdEncoding, new base32_internal_test_package.badReaderжReader(Ꮡbr));
        var dbuf = new slice<byte>(StdEncoding.DecodedLen(len(input)));
        var (n, err) = decoder.Read(dbuf);
        testEqual(t, decodingOfQErrVExpectedVˢ, input, err, (error)default);
        (n, err) = decoder.Read(dbuf);
        testEqual(t, readAfterEofNDExpectedDˢ, n, (nint)0);
        testEqual(t, readAfterEofErrVExpectedˢ, err, io.EOF);
        (n, err) = decoder.Read(dbuf);
        testEqual(t, readAfterEofNDExpectedDˢ, n, (nint)0);
        testEqual(t, readAfterEofErrVExpectedˢ, err, io.EOF);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string decodingDOfQQWantQˢ2 => ref decodingDOfQQWantQˢ2ᵇ;
private static readonly @string decodingDOfQQWantQˢ2ᵇ = "Decoding/%d of %q = %q, want %q"u8;

public static void TestDecoderBuffering(ж<testing.T> t) {
    for (nint bs = 1; bs <= 12; bs++) {
        var decoder = NewDecoder(StdEncoding, new base32_test_package.strings_ReaderжReader(strings.NewReader(bigtest.encoded)));
        var buf = new slice<byte>(len(bigtest.decoded) + 12);
        nint total = default;
        nint n = default;
        error err = default;
        for (total = 0; total < len(bigtest.decoded) && err == nil; ) {
            (n, err) = decoder.Read(buf.slice(total, total + bs));
            total += n;
        }
        if (err != nil && !AreEqual(err, io.EOF)) {
            t.Errorf("Read from %q at pos %d = %d, unexpected error %v", bigtest.encoded, total, n, err);
        }
        testEqual(t, decodingDOfQQWantQˢ2, bs, bigtest.encoded, (@string)(buf.slice(0, total)), bigtest.decoded);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decoderWronglyDetectedˢ = (@string)"Decoder wrongly detected corruption in"u8;
internal static ref readonly @string corruptionInQAtOffsetVˢ => ref corruptionInQAtOffsetVˢᵇ;
private static readonly @string corruptionInQAtOffsetVˢᵇ = "Corruption in %q at offset %v, want %v"u8;
internal static readonly object decoderFailedToDetectˢ = (@string)"Decoder failed to detect corruption in"u8;

internal partial struct TestDecodeCorrupt_testCases /*dyn*/ {
    internal @string input;
    internal nint offset; // -1 means no corruption.
}

public static void TestDecodeCorrupt(ж<testing.T> t) {
    var testCases = slice<TestDecodeCorrupt_testCases>([
        new("", -1),
        new("!!!!", 0),
        new("x===", 0),
        new("AA=A====", 2),
        new("AAA=AAAA", 3),
        new("MMMMMMMMM", 8),
        new("MMMMMM", 0),
        new("A=", 1),
        new("AA=", 3),
        new("AA==", 4),
        new("AA===", 5),
        new("AAAA=", 5),
        new("AAAA==", 6),
        new("AAAAA=", 6),
        new("AAAAA==", 7),
        new("A=======", 1),
        new("AA======", -1),
        new("AAA=====", 3),
        new("AAAA====", -1),
        new("AAAAA===", -1),
        new("AAAAAA==", 6),
        new("AAAAAAA=", -1),
        new("AAAAAAAA", -1)
    ]);
    foreach (var (_, tc) in testCases) {
        var dbuf = new slice<byte>(StdEncoding.DecodedLen(len(tc.input)));
        var (_, err) = StdEncoding.Decode(dbuf, slice<byte>(tc.input));
        if (tc.offset == -1) {
            if (err != nil) {
                t.Error(decoderWronglyDetectedˢ, tc.input);
            }
            continue;
        }
        switch (err.type()) {
        case CorruptInputError errΔ1: {
            testEqual(t, corruptionInQAtOffsetVˢ, tc.input, (nint)errΔ1, tc.offset);
            break;
        }
        default: {
            var errΔ1 = err;
            t.Error(decoderFailedToDetectˢ, tc);
            break;
        }}
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string alphaᶜ => ref alphaᶜᵇ;
private static readonly @string alphaᶜᵇ = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"u8;

public static void TestBig(ж<testing.T> t) {
    nint n = 3 * 1000 + 1;
    var raw = new slice<byte>(n);
    @string alpha = alphaᶜ;
    for (nint i = 0; i < n; i++) {
        raw[i] = alpha[i % len(alpha)];
    }
    var encoded = @new<bytes.Buffer>();
    var w = NewEncoder(StdEncoding, new base32_test_package.bytes_BufferжWriter(encoded));
    var (nn, err) = w.Write(raw);
    if (nn != n || err != nil) {
        t.Fatalf("Encoder.Write(raw) = %d, %v want %d, nil", nn, err, n);
    }
    err = w.Close();
    if (err != nil) {
        t.Fatalf("Encoder.Close() = %v want nil", err);
    }
    (var decoded, err) = io.ReadAll(NewDecoder(StdEncoding, new base32_test_package.bytes_BufferжReader(encoded)));
    if (err != nil) {
        t.Fatalf("io.ReadAll(NewDecoder(...)): %v", err);
    }

    if (!bytes.Equal(raw, decoded)) {
        nint i = default;
        for (i = 0; i < len(decoded) && i < len(raw); i++) {
            if (decoded[i] != raw[i]) {
                break;
            }
        }
        t.Errorf("Decode(Encode(%d-byte string)) failed at offset %d", n, i);
    }
}

internal static void testStringEncoding(ж<testing.T> t, @string expected, slice<@string> examples) {
    foreach (var (_, e) in examples) {
        var (buf, err) = StdEncoding.DecodeString(e);
        if (err != nil) {
            t.Errorf("Decode(%q) failed: %v", e, err);
            continue;
        }
        {
            @string s = (@string)buf; if (s != expected) {
                t.Errorf("Decode(%q) = %q, want %q", e, s, expected);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string sureˢ => ref sureˢᵇ;
private static readonly @string sureˢᵇ = "sure"u8;
internal static ref readonly @string foobarˢ => ref foobarˢᵇ;
private static readonly @string foobarˢᵇ = "foobar"u8;

public static void TestNewLineCharacters(ж<testing.T> t) {
    // Each of these should decode to the string "sure", without errors.
    var examples = slice<@string>([
        "ON2XEZI=",
        "ON2XEZI=\r",
        "ON2XEZI=\n",
        "ON2XEZI=\r\n",
        "ON2XEZ\r\nI=",
        "ON2X\rEZ\nI=",
        "ON2X\nEZ\rI=",
        "ON2XEZ\nI=",
        "ON2XEZI\n="
    ]);
    testStringEncoding(t, sureˢ, examples);

    // Each of these should decode to the string "foobar", without errors.
    examples = slice<@string>([
        "MZXW6YTBOI======",
        "MZXW6YTBOI=\r\n====="
    ]);
    testStringEncoding(t, foobarˢ, examples);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decodedResultsNotEqualˢ = (@string)"Decoded results not equal"u8;

public static void TestDecoderIssue4779(ж<testing.T> t) {
    @string encoded = """
JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWCAY3PNZZWKY3UMV2HK4
RAMFSGS4DJONUWG2LOM4QGK3DJOQWCA43FMQQGI3YKMVUXK43NN5SCA5DFNVYG64RANFXGG2LENFSH
K3TUEB2XIIDMMFRG64TFEBSXIIDEN5WG64TFEBWWCZ3OMEQGC3DJOF2WCLRAKV2CAZLONFWQUYLEEB
WWS3TJNUQHMZLONFQW2LBAOF2WS4ZANZXXG5DSOVSCAZLYMVZGG2LUMF2GS33OEB2WY3DBNVRW6IDM
MFRG64TJOMQG42LTNEQHK5AKMFWGS4LVNFYCAZLYEBSWCIDDN5WW233EN4QGG33OONSXC5LBOQXCAR
DVNFZSAYLVORSSA2LSOVZGKIDEN5WG64RANFXAU4TFOBZGK2DFNZSGK4TJOQQGS3RAOZXWY5LQORQX
IZJAOZSWY2LUEBSXG43FEBRWS3DMOVWSAZDPNRXXEZJAMV2SAZTVM5UWC5BANZ2WY3DBBJYGC4TJMF
2HK4ROEBCXQY3FOB2GK5LSEBZWS3TUEBXWGY3BMVRWC5BAMN2XA2LEMF2GC5BANZXW4IDQOJXWSZDF
NZ2CYIDTOVXHIIDJNYFGG5LMOBQSA4LVNEQG6ZTGNFRWSYJAMRSXGZLSOVXHIIDNN5WGY2LUEBQW42
LNEBUWIIDFON2CA3DBMJXXE5LNFY==
====
"""u8.Raw();
    @string encodedShort = strings.ReplaceAll(encoded, "\n", ""u8);

    var dec = NewDecoder(StdEncoding, new base32_test_package.strings_ReaderжReader(strings.NewReader(encoded)));
    var (res1, err) = io.ReadAll(dec);
    if (err != nil) {
        t.Errorf("ReadAll failed: %v", err);
    }

    dec = NewDecoder(StdEncoding, new base32_test_package.strings_ReaderжReader(strings.NewReader(encodedShort)));
    slice<byte> res2 = default;
    (res2, err) = io.ReadAll(dec);
    if (err != nil) {
        t.Errorf("ReadAll failed: %v", err);
    }

    if (!bytes.Equal(res1, res2)) {
        t.Error(decodedResultsNotEqualˢ);
    }
}

public static void BenchmarkEncode(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var data = new slice<byte>(8192);
    var buf = new slice<byte>(StdEncoding.EncodedLen(len(data)));
    b.SetBytes((int64)len(data));
    for (nint i = 0; i < b.N; i++) {
        StdEncoding.Encode(buf, data);
    }
}

public static void BenchmarkEncodeToString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var data = new slice<byte>(8192);
    b.SetBytes((int64)len(data));
    for (nint i = 0; i < b.N; i++) {
        StdEncoding.EncodeToString(data);
    }
}

public static void BenchmarkDecode(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var data = new slice<byte>(StdEncoding.EncodedLen(8192));
    StdEncoding.Encode(data, new slice<byte>(8192));
    var buf = new slice<byte>(8192);
    b.SetBytes((int64)len(data));
    for (nint i = 0; i < b.N; i++) {
        StdEncoding.Decode(buf, data);
    }
}

public static void BenchmarkDecodeString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string data = StdEncoding.EncodeToString(new slice<byte>(8192));
    b.SetBytes((int64)len(data));
    for (nint i = 0; i < b.N; i++) {
        StdEncoding.DecodeString(data);
    }
}

public static void TestWithCustomPadding(ж<testing.T> t) {
    foreach (var (_, testcase) in pairs) {
        @string defaultPadding = StdEncoding.EncodeToString(slice<byte>(testcase.decoded));
        @string customPadding = (~StdEncoding).WithPadding('@').EncodeToString(slice<byte>(testcase.decoded));
        @string expected = strings.ReplaceAll(defaultPadding, "=", "@");

        if (expected != customPadding) {
            t.Errorf("Expected custom %s, got %s", expected, customPadding);
        }
        if (testcase.encoded != defaultPadding) {
            t.Errorf("Expected %s, got %s", testcase.encoded, defaultPadding);
        }
    }
}

public static void TestWithoutPadding(ж<testing.T> t) {
    foreach (var (_, testcase) in pairs) {
        @string defaultPadding = StdEncoding.EncodeToString(slice<byte>(testcase.decoded));
        @string customPadding = (~StdEncoding).WithPadding(NoPadding).EncodeToString(slice<byte>(testcase.decoded));
        @string expected = strings.TrimRight(defaultPadding, "=");

        if (expected != customPadding) {
            t.Errorf("Expected custom %s, got %s", expected, customPadding);
        }
        if (testcase.encoded != defaultPadding) {
            t.Errorf("Expected %s, got %s", testcase.encoded, defaultPadding);
        }
    }
}

public static void TestDecodeWithPadding(ж<testing.T> t) {
    var encodings = slice<ж<global::go.encoding.base32_package.Encoding>>([
        StdEncoding,
        (~StdEncoding).WithPadding('-'),
        (~StdEncoding).WithPadding(NoPadding)
    ]);

    foreach (var (i, enc) in encodings) {
        foreach (var (_, pair) in pairs) {
            @string input = pair.decoded;
            @string encoded = enc.EncodeToString(slice<byte>(input));

            var (decoded, err) = enc.DecodeString(encoded);
            if (err != nil) {
                t.Errorf("DecodeString Error for encoding %d (%q): %v", i, input, err);
            }

            if (input != ((sstring)decoded)) {
                t.Errorf("Unexpected result for encoding %d: got %q; want %q", i, decoded, input);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorˢ = (@string)"expected error"u8;

public static void TestDecodeWithWrongPadding(ж<testing.T> t) {
    @string encoded = StdEncoding.EncodeToString(slice<byte>("foobar"u8));

    var (_, err) = (~StdEncoding).WithPadding('-').DecodeString(encoded);
    if (err == nil) {
        t.Error(expectedErrorˢ);
    }

    (_, err) = (~StdEncoding).WithPadding(NoPadding).DecodeString(encoded);
    if (err == nil) {
        t.Error(expectedErrorˢ);
    }
}

internal partial struct TestBufferedDecodingSameError_testcases /*dyn*/ {
    internal @string prefix;
    internal slice<slice<@string>> chunkCombinations;
    internal error expected;
}

public static partial void TestBufferedDecodingSameError(ж<testing.T> t) {
    var testcases = slice<TestBufferedDecodingSameError_testcases>([
        // NBSWY3DPO5XXE3DE == helloworld
        // Test with "ZZ" as extra input
        new("helloworld", slice<slice<@string>>([
            slice<@string>(["NBSW", "Y3DP", "O5XX", "E3DE", "ZZ"]),
            slice<@string>(["NBSWY3DPO5XXE3DE", "ZZ"]),
            slice<@string>(["NBSWY3DPO5XXE3DEZZ"]),
            slice<@string>(["NBS", "WY3", "DPO", "5XX", "E3D", "EZZ"]),
            slice<@string>(["NBSWY3DPO5XXE3", "DEZZ"])
        ]), io.ErrUnexpectedEOF),

        // Test with "ZZY" as extra input
        new("helloworld", slice<slice<@string>>([
            slice<@string>(["NBSW", "Y3DP", "O5XX", "E3DE", "ZZY"]),
            slice<@string>(["NBSWY3DPO5XXE3DE", "ZZY"]),
            slice<@string>(["NBSWY3DPO5XXE3DEZZY"]),
            slice<@string>(["NBS", "WY3", "DPO", "5XX", "E3D", "EZZY"]),
            slice<@string>(["NBSWY3DPO5XXE3", "DEZZY"])
        ]), io.ErrUnexpectedEOF),

        // Normal case, this is valid input
        new("helloworld", slice<slice<@string>>([
            slice<@string>(["NBSW", "Y3DP", "O5XX", "E3DE"]),
            slice<@string>(["NBSWY3DPO5XXE3DE"]),
            slice<@string>(["NBS", "WY3", "DPO", "5XX", "E3D", "E"]),
            slice<@string>(["NBSWY3DPO5XXE3", "DE"])
        ]), default),

        // MZXW6YTB = fooba
        new("fooba", slice<slice<@string>>([
            slice<@string>(["MZXW6YTBZZ"]),
            slice<@string>(["MZXW6YTBZ", "Z"]),
            slice<@string>(["MZXW6YTB", "ZZ"]),
            slice<@string>(["MZXW6YT", "BZZ"]),
            slice<@string>(["MZXW6Y", "TBZZ"]),
            slice<@string>(["MZXW6Y", "TB", "ZZ"]),
            slice<@string>(["MZXW6", "YTBZZ"]),
            slice<@string>(["MZXW6", "YTB", "ZZ"]),
            slice<@string>(["MZXW6", "YT", "BZZ"])
        ]), io.ErrUnexpectedEOF),

        // Normal case, this is valid input
        new("fooba", slice<slice<@string>>([
            slice<@string>(["MZXW6YTB"]),
            slice<@string>(["MZXW6YT", "B"]),
            slice<@string>(["MZXW6Y", "TB"]),
            slice<@string>(["MZXW6", "YTB"]),
            slice<@string>(["MZXW6", "YT", "B"]),
            slice<@string>(["MZXW", "6YTB"]),
            slice<@string>(["MZXW", "6Y", "TB"])
        ]), default)
    ]);

    foreach (var (_, testcase) in testcases) {
        foreach (var (_, chunks) in testcase.chunkCombinations) {
            var (pr, pw) = io.Pipe();

            // Write the encoded chunks into the pipe
            var chunksʗ1 = chunks;
            var pwʗ1 = pw;
            goǃ(() => {
                foreach (var (_, chunk) in chunksʗ1) {
                    pwʗ1.Write(slice<byte>(chunk));
                }
                pwʗ1.Close();
            });

            var decoder = NewDecoder(StdEncoding, new io.PipeReaderжReader(pr));
            var (_, err) = io.ReadAll(decoder);

            if (!AreEqual(err, testcase.expected)) {
                t.Errorf("Expected %v, got %v; case %s %+v", testcase.expected, err, testcase.prefix, chunks);
            }
        }
    }
}

internal partial struct TestBufferedDecodingPadding_testcases /*dyn*/ {
    internal slice<@string> chunks;
    internal @string expectedError;
}

public static partial void TestBufferedDecodingPadding(ж<testing.T> t) {
    var testcases = slice<TestBufferedDecodingPadding_testcases>([
        new(slice<@string>([
            "I4======",
            "=="
        ]), "unexpected EOF"),

        new(slice<@string>([
            "I4======N4======"
        ]), "illegal base32 data at input byte 2"),

        new(slice<@string>([
            "I4======",
            "N4======"
        ]), "illegal base32 data at input byte 0"),

        new(slice<@string>([
            "I4======",
            "========"
        ]), "illegal base32 data at input byte 0"),

        new(slice<@string>([
            "I4I4I4I4",
            "I4======",
            "I4======"
        ]), "illegal base32 data at input byte 0")
    ]);

    foreach (var (_, vᴛ1) in testcases) {
        ref var testcase = ref heap(new TestBufferedDecodingPadding_testcases(), out var Ꮡtestcase);
        testcase = vᴛ1;

        var (pr, pw) = io.Pipe();
        var pwʗ1 = pw;
        var testcaseʗ1 = testcase;
        goǃ(() => {
            foreach (var (_, chunk) in testcaseʗ1.chunks) {
                (_, _) = pwʗ1.Write(slice<byte>(chunk));
            }
            _ = pwʗ1.Close();
        });

        var decoder = NewDecoder(StdEncoding, new io.PipeReaderжReader(pr));
        var (_, err) = io.ReadAll(decoder);

        if (err == nil && len(testcase.expectedError) != 0) {
            t.Errorf("case %q: got nil error, want %v", testcase.chunks, testcase.expectedError);
        } else if (err.Error() != testcase.expectedError) {
            t.Errorf("case %q: got %v, want %v", testcase.chunks, err, testcase.expectedError);
        }
    }
}

internal partial struct TestEncodedLen_test /*dyn*/ {
    internal ж<global::go.encoding.base32_package.Encoding> enc;
    internal nint n;
    internal int64 want;
}

public static void TestEncodedLen(ж<testing.T> t) {
    var testsᴷ = new stackAppend<TestEncodedLen_test>();

    ж<global::go.encoding.base32_package.Encoding> rawStdEncoding = (~StdEncoding).WithPadding(NoPadding);
    var tests = slice<TestEncodedLen_test>([
        new(StdEncoding, 0, 0),
        new(StdEncoding, 1, 8),
        new(StdEncoding, 2, 8),
        new(StdEncoding, 3, 8),
        new(StdEncoding, 4, 8),
        new(StdEncoding, 5, 8),
        new(StdEncoding, 6, 16),
        new(StdEncoding, 10, 16),
        new(StdEncoding, 11, 24),
        new(rawStdEncoding, 0, 0),
        new(rawStdEncoding, 1, 2),
        new(rawStdEncoding, 2, 4),
        new(rawStdEncoding, 3, 5),
        new(rawStdEncoding, 4, 7),
        new(rawStdEncoding, 5, 8),
        new(rawStdEncoding, 6, 10),
        new(rawStdEncoding, 7, 12),
        new(rawStdEncoding, 10, 16),
        new(rawStdEncoding, 11, 18)
    ]);
    // check overflow
    switch (strconv.IntSize) {
    case var ᴛ when ᴛ == 32: {
        tests = append(tests, new TestEncodedLen_test(rawStdEncoding, unchecked((nint)(1152921504606846976L)), 429496730));
        tests = append(tests, new TestEncodedLen_test(rawStdEncoding, unchecked((nint)(5764607523034234879L)), math.MaxInt));
        break;
    }
    case var ᴛ when ᴛ == 64: {
        tests = append(testsᴷ, tests, new TestEncodedLen_test(rawStdEncoding, unchecked((nint)(1152921504606846976L)), 1844674407370955162L));
        tests = append(tests, new TestEncodedLen_test(rawStdEncoding, unchecked((nint)(5764607523034234879L)), math.MaxInt));
        break;
    }}
    foreach (var (_, tt) in tests) {
        {
            nint got = tt.enc.EncodedLen(tt.n); if ((int64)got != tt.want) {
                t.Errorf("EncodedLen(%d): got %d, want %d", tt.n, got, tt.want);
            }
        }
    }
}

internal partial struct TestDecodedLen_test /*dyn*/ {
    internal ж<global::go.encoding.base32_package.Encoding> enc;
    internal nint n;
    internal int64 want;
}

public static void TestDecodedLen(ж<testing.T> t) {
    var testsᴷ = new stackAppend<TestDecodedLen_test>();

    ж<global::go.encoding.base32_package.Encoding> rawStdEncoding = (~StdEncoding).WithPadding(NoPadding);
    var tests = slice<TestDecodedLen_test>([
        new(StdEncoding, 0, 0),
        new(StdEncoding, 8, 5),
        new(StdEncoding, 16, 10),
        new(StdEncoding, 24, 15),
        new(rawStdEncoding, 0, 0),
        new(rawStdEncoding, 2, 1),
        new(rawStdEncoding, 4, 2),
        new(rawStdEncoding, 5, 3),
        new(rawStdEncoding, 7, 4),
        new(rawStdEncoding, 8, 5),
        new(rawStdEncoding, 10, 6),
        new(rawStdEncoding, 12, 7),
        new(rawStdEncoding, 16, 10),
        new(rawStdEncoding, 18, 11)
    ]);
    // check overflow
    switch (strconv.IntSize) {
    case var ᴛ when ᴛ == 32: {
        tests = append(tests, new TestDecodedLen_test(rawStdEncoding, unchecked((nint)(1844674407370955162L)), 268435456));
        tests = append(tests, new TestDecodedLen_test(rawStdEncoding, math.MaxInt, 1342177279));
        break;
    }
    case var ᴛ when ᴛ == 64: {
        tests = append(testsᴷ, tests, new TestDecodedLen_test(rawStdEncoding, unchecked((nint)(1844674407370955162L)), 1152921504606846976L));
        tests = append(tests, new TestDecodedLen_test(rawStdEncoding, math.MaxInt, 5764607523034234879L));
        break;
    }}
    foreach (var (_, tt) in tests) {
        {
            nint got = tt.enc.DecodedLen(tt.n); if ((int64)got != tt.want) {
                t.Errorf("DecodedLen(%d): got %d, want %d", tt.n, got, tt.want);
            }
        }
    }
}

public static void TestWithoutPaddingClose(ж<testing.T> t) {
    var encodings = slice<ж<global::go.encoding.base32_package.Encoding>>([
        StdEncoding,
        (~StdEncoding).WithPadding(NoPadding)
    ]);

    foreach (var (_, encoding) in encodings) {
        foreach (var (_, testpair) in pairs) {
            ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
            var encoder = NewEncoder(encoding, new base32_test_package.strings_BuilderжWriter(Ꮡbuf));
            encoder.Write(slice<byte>(testpair.decoded));
            encoder.Close();

            @string expected = testpair.encoded;
            if (encoding.padChar == NoPadding) {
                expected = strings.ReplaceAll(expected, "=", ""u8);
            }

            @string res = buf.String();

            if (res != expected) {
                t.Errorf("Expected %s got %s; padChar=%d", expected, res, encoding.padChar);
            }
        }
    }
}

public static void TestDecodeReadAll(ж<testing.T> t) {
    var encodings = slice<ж<global::go.encoding.base32_package.Encoding>>([
        StdEncoding,
        (~StdEncoding).WithPadding(NoPadding)
    ]);

    foreach (var (_, pair) in pairs) {
        foreach (var (encIndex, encoding) in encodings) {
            @string encoded = pair.encoded;
            if (encoding.padChar == NoPadding) {
                encoded = strings.ReplaceAll(encoded, "=", ""u8);
            }

            var (decReader, err) = io.ReadAll(NewDecoder(encoding, new base32_test_package.strings_ReaderжReader(strings.NewReader(encoded))));
            if (err != nil) {
                t.Errorf("NewDecoder error: %v", err);
            }

            if (pair.decoded != ((sstring)decReader)) {
                t.Errorf("Expected %s got %s; Encoding %d", pair.decoded, decReader, encIndex);
            }
        }
    }
}

public static void TestDecodeSmallBuffer(ж<testing.T> t) {
    var encodings = slice<ж<global::go.encoding.base32_package.Encoding>>([
        StdEncoding,
        (~StdEncoding).WithPadding(NoPadding)
    ]);

    for (nint bufferSize = 1; bufferSize < 200; bufferSize++) {
        foreach (var (_, pair) in pairs) {
            foreach (var (encIndex, encoding) in encodings) {
                @string encoded = pair.encoded;
                if (encoding.padChar == NoPadding) {
                    encoded = strings.ReplaceAll(encoded, "=", ""u8);
                }

                var decoder = NewDecoder(encoding, new base32_test_package.strings_ReaderжReader(strings.NewReader(encoded)));

                slice<byte> allRead = default;

                for (;;) {
                    var buf = new slice<byte>(bufferSize);
                    var (n, err) = decoder.Read(buf);
                    allRead = appendꓸꓸꓸ(allRead, buf.slice(0, n));
                    if (AreEqual(err, io.EOF)) {
                        break;
                    }
                    if (err != nil) {
                        t.Error(err);
                    }
                }

                if (pair.decoded != ((sstring)allRead)) {
                    t.Errorf("Expected %s got %s; Encoding %d; bufferSize %d", pair.decoded, allRead, encIndex, bufferSize);
                }
            }
        }
    }
}

} // end base32_internal_test_package
