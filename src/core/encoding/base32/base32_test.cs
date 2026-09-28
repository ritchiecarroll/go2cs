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
using static go.encoding.base32_package;
using ꓸꓸꓸany = Span<any>;

partial class base32_internal_test_package {

[GoType] internal partial struct testpair {
    internal @string decoded, encoded;
}

// RFC 4648 examples
// Wikipedia examples, converted to base32
internal static slice<testpair> pairs = new testpair[]{
    new(""u8, ""u8),
    new("f"u8, "MY======"u8),
    new("fo"u8, "MZXQ===="u8),
    new("foo"u8, "MZXW6==="u8),
    new("foob"u8, "MZXW6YQ="u8),
    new("fooba"u8, "MZXW6YTB"u8),
    new("foobar"u8, "MZXW6YTBOI======"u8),
    new("sure."u8, "ON2XEZJO"u8),
    new("sure"u8, "ON2XEZI="u8),
    new("sur"u8, "ON2XE==="u8),
    new("su"u8, "ON2Q===="u8),
    new("leasure."u8, "NRSWC43VOJSS4==="u8),
    new("easure."u8, "MVQXG5LSMUXA===="u8),
    new("asure."u8, "MFZXK4TFFY======"u8),
    new("sure."u8, "ON2XEZJO"u8)
}.slice();

internal static testpair bigtest = new testpair(
    "Twas brillig, and the slithy toves"u8,
    "KR3WC4ZAMJZGS3DMNFTSYIDBNZSCA5DIMUQHG3DJORUHSIDUN53GK4Y="u8
);

internal static bool testEqual(ж<testing.T> Ꮡt, @string msg, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    Ꮡt.Helper();
    if (!AreEqual(args[len(args) - 2], args[len(args) - 1])) {
        Ꮡt.Errorf(msg, args.ꓸꓸꓸ);
        return false;
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string encodeQQWantQˢ = "Encode(%q) = %q, want %q"u8;
internal static readonly @string appendEncodeLeadQQWantQˢ = @"AppendEncode(""lead"", %q) = %q, want %q"u8;

public static void TestEncode(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        @string got = StdEncoding.EncodeToString(slice<byte>(p.decoded));
        testEqual(Ꮡt, encodeQQWantQˢ, p.decoded, got, p.encoded);
        var dst = StdEncoding.AppendEncode(slice<byte>("lead"u8), slice<byte>(p.decoded));
        testEqual(Ꮡt, appendEncodeLeadQQWantQˢ, p.decoded, ((@string)dst), "lead" + p.encoded);
    }
}

public static void TestEncoder(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        var bb = Ꮡ(new strings.Builder(nil));
        var encoder = NewEncoder(StdEncoding, new base32_test_package.strings_BuilderжWriter(bb));
        encoder.Write(slice<byte>(p.decoded));
        encoder.Close();
        testEqual(Ꮡt, encodeQQWantQˢ, p.decoded, bb.String(), p.encoded);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string writeQGaveErrorVWantVˢ = "Write(%q) gave error %v, want %v"u8;
internal static readonly @string writeQGaveLengthVWantVˢ = "Write(%q) gave length %v, want %v"u8;
internal static readonly @string closeGaveErrorVWantVˢ = "Close gave error %v, want %v"u8;
internal static readonly @string encodingDOfQQWantQˢ = "Encoding/%d of %q = %q, want %q"u8;

public static void TestEncoderBuffering(ж<testing.T> Ꮡt) {
    var input = slice<byte>(bigtest.decoded);
    for (nint bs = 1; bs <= 12; bs++) {
        var bb = Ꮡ(new strings.Builder(nil));
        var encoder = NewEncoder(StdEncoding, new base32_test_package.strings_BuilderжWriter(bb));
        for (nint pos = 0; pos < len(input); pos += bs) {
            nint end = pos + bs;
            if (end > len(input)) {
                end = len(input);
            }
            var (n, errΔ1) = encoder.Write(input[(int)(pos)..(int)(end)]);
            testEqual(Ꮡt, writeQGaveErrorVWantVˢ, input[(int)(pos)..(int)(end)], errΔ1, ((error)default!));
            testEqual(Ꮡt, writeQGaveLengthVWantVˢ, input[(int)(pos)..(int)(end)], n, end - pos);
        }
        var err = encoder.Close();
        testEqual(Ꮡt, closeGaveErrorVWantVˢ, err, ((error)default!));
        testEqual(Ꮡt, encodingDOfQQWantQˢ, bs, bigtest.decoded, bb.String(), bigtest.encoded);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string decodingDOfQQWantQˢ = "Decoding/%d of %q = %q, want %q\n"u8;

public static void TestDecoderBufferingWithPadding(ж<testing.T> Ꮡt) {
    for (nint bs = 0; bs <= 12; bs++) {
        foreach (var (_, s) in pairs) {
            var decoder = NewDecoder(StdEncoding, new base32_test_package.strings_ReaderжReader(strings.NewReader(s.encoded)));
            var buf = new slice<byte>(len(s.decoded) + bs);
            nint n = default!;
            error err = default!;
            (n, err) = decoder.Read(buf);
            if (err != default! && !AreEqual(err, io.EOF)) {
                Ꮡt.Errorf("Read from %q at pos %d = %d, unexpected error %v"u8, s.encoded, len(s.decoded), n, err);
            }
            testEqual(Ꮡt, decodingDOfQQWantQˢ, bs, s.encoded, ((@string)(buf[..(int)(n)])), s.decoded);
        }
    }
}

public static void TestDecoderBufferingWithoutPadding(ж<testing.T> Ꮡt) {
    for (nint bs = 0; bs <= 12; bs++) {
        foreach (var (_, s) in pairs) {
            @string encoded = strings.TrimRight(s.encoded, "="u8);
            var decoder = NewDecoder((~StdEncoding).WithPadding(NoPadding), new base32_test_package.strings_ReaderжReader(strings.NewReader(encoded)));
            var buf = new slice<byte>(len(s.decoded) + bs);
            nint n = default!;
            error err = default!;
            (n, err) = decoder.Read(buf);
            if (err != default! && !AreEqual(err, io.EOF)) {
                Ꮡt.Errorf("Read from %q at pos %d = %d, unexpected error %v"u8, encoded, len(s.decoded), n, err);
            }
            testEqual(Ꮡt, decodingDOfQQWantQˢ, bs, encoded, ((@string)(buf[..(int)(n)])), s.decoded);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string decodeQErrorVWantVˢ = "Decode(%q) = error %v, want %v"u8;
internal static readonly @string decodeQLengthVWantVˢ = "Decode(%q) = length %v, want %v"u8;
internal static readonly @string decodeQEndVWantVˢ = "Decode(%q) = end %v, want %v"u8;
internal static readonly @string decodeQQWantQˢ = "Decode(%q) = %q, want %q"u8;
internal static readonly @string decodeStringQErrorVWantVˢ = "DecodeString(%q) = error %v, want %v"u8;
internal static readonly @string decodeStringQQWantQˢ = "DecodeString(%q) = %q, want %q"u8;
internal static readonly @string appendDecodeQErrorVWantVˢ = "AppendDecode(%q) = error %v, want %v"u8;
internal static readonly @string appendDecodeLeadQQWantQˢ = @"AppendDecode(""lead"", %q) = %q, want %q"u8;
internal static readonly @string appendDecodeQQWantQˢ = @"AppendDecode("""", %q) = %q, want %q"u8;

public static void TestDecode(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        var dbuf = new slice<byte>(StdEncoding.DecodedLen(len(p.encoded)));
        var (count, end, err) = StdEncoding.decode(dbuf, slice<byte>(p.encoded));
        testEqual(Ꮡt, decodeQErrorVWantVˢ, p.encoded, err, ((error)default!));
        testEqual(Ꮡt, decodeQLengthVWantVˢ, p.encoded, count, len(p.decoded));
        if (len(p.encoded) > 0) {
            testEqual(Ꮡt, decodeQEndVWantVˢ, p.encoded, end, (p.encoded[len(p.encoded) - 1] == (rune)'='));
        }
        testEqual(Ꮡt, decodeQQWantQˢ, p.encoded, ((@string)(dbuf[0..(int)(count)])), p.decoded);
        (dbuf, err) = StdEncoding.DecodeString(p.encoded);
        testEqual(Ꮡt, decodeStringQErrorVWantVˢ, p.encoded, err, ((error)default!));
        testEqual(Ꮡt, decodeStringQQWantQˢ, p.encoded, ((@string)dbuf), p.decoded);
        (var dst, err) = StdEncoding.AppendDecode(slice<byte>("lead"u8), slice<byte>(p.encoded));
        testEqual(Ꮡt, appendDecodeQErrorVWantVˢ, p.encoded, err, ((error)default!));
        testEqual(Ꮡt, appendDecodeLeadQQWantQˢ, p.encoded, ((@string)dst), "lead" + p.decoded);
        (var dst2, err) = StdEncoding.AppendDecode(dst.slice(-1, 0, len(p.decoded)), slice<byte>(p.encoded));
        testEqual(Ꮡt, appendDecodeQErrorVWantVˢ, p.encoded, err, ((error)default!));
        testEqual(Ꮡt, appendDecodeQQWantQˢ, p.encoded, ((@string)dst2), p.decoded);
        if (len(dst) > 0 && len(dst2) > 0 && Ꮡ(dst, 0) != Ꮡ(dst2, 0)) {
            Ꮡt.Errorf("unexpected capacity growth: got %d, want %d"u8, cap(dst2), cap(dst));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readFailedˢ = (@string)"Read failed"u8;
internal static readonly @string readFromQLengthVWantVˢ = "Read from %q = length %v, want %v"u8;
internal static readonly @string decodingOfQQWantQˢ = "Decoding of %q = %q, want %q"u8;
internal static readonly @string readFromQVWantVˢ = "Read from %q = %v, want %v"u8;

public static void TestDecoder(ж<testing.T> Ꮡt) {
    foreach (var (_, p) in pairs) {
        var decoder = NewDecoder(StdEncoding, new base32_test_package.strings_ReaderжReader(strings.NewReader(p.encoded)));
        var dbuf = new slice<byte>(StdEncoding.DecodedLen(len(p.encoded)));
        var (count, err) = decoder.Read(dbuf);
        if (err != default! && !AreEqual(err, io.EOF)) {
            Ꮡt.Fatal(readFailedˢ, err);
        }
        testEqual(Ꮡt, readFromQLengthVWantVˢ, p.encoded, count, len(p.decoded));
        testEqual(Ꮡt, decodingOfQQWantQˢ, p.encoded, ((@string)(dbuf[0..(int)(count)])), p.decoded);
        if (!AreEqual(err, io.EOF)) {
            (_, err) = decoder.Read(dbuf);
        }
        testEqual(Ꮡt, readFromQVWantVˢ, p.encoded, err, io.EOF);
    }
}

[GoType] internal partial struct badReader {
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
[GoRecv] internal static (nint, error) Read(this ref badReader b, slice<byte> p) {
    nint lim = len(p);
    if (b.limit != 0 && b.limit < lim) {
        lim = b.limit;
    }
    if (len(b.data) < lim) {
        lim = len(b.data);
    }
    foreach (var (i, _) in p[..(int)(lim)]) {
        p[i] = b.data[i];
    }
    b.data = b.data[(int)(lim)..];
    var err = io.EOF;
    if (b.called < len(b.errs)) {
        err = b.errs[b.called];
    }
    b.called++;
    return (lim, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badReaderErrorˢ = "bad reader error"u8;
internal static readonly @string decodingOfQErrVExpectedVˢ = "Decoding of %q err = %v, expected %v"u8;

[GoType("dyn")] internal partial struct TestIssue20044_testCases {
    internal badReader r;
    internal @string res;
    internal error err;
    internal nint dbuflen;
}

// TestIssue20044 tests that decoder.Read behaves correctly when the caller
// supplied reader returns an error.
public static void TestIssue20044(ж<testing.T> Ꮡt) {
    var badErr = errors.New(badReaderErrorˢ);
    var testCases = new TestIssue20044_testCases[]{ // Check valid input data accompanied by an error is processed and the error is propagated.

        new(r: new badReader(data: slice<byte>("MY======"u8), errs: new error[]{badErr}.slice()),
            res: "f"u8, err: badErr), // Check a read error accompanied by input data consisting of newlines only is propagated.

        new(r: new badReader(data: slice<byte>("\n\n\n\n\n\n\n\n"u8), errs: new error[]{badErr, default!}.slice()),
            res: ""u8, err: badErr), // Reader will be called twice.  The first time it will return 8 newline characters.  The
 // second time valid base32 encoded data and an error.  The data should be decoded
 // correctly and the error should be propagated.

        new(r: new badReader(data: slice<byte>("\n\n\n\n\n\n\n\nMY======"u8), errs: new error[]{default!, badErr}.slice()),
            res: "f"u8, err: badErr, dbuflen: 8), // Reader returns invalid input data (too short) and an error.  Verify the reader
 // error is returned.

        new(r: new badReader(data: slice<byte>("MY====="u8), errs: new error[]{badErr}.slice()),
            res: ""u8, err: badErr), // Reader returns invalid input data (too short) but no error.  Verify io.ErrUnexpectedEOF
 // is returned.

        new(r: new badReader(data: slice<byte>("MY====="u8), errs: new error[]{default!}.slice()),
            res: ""u8, err: io.ErrUnexpectedEOF), // Reader returns invalid input data and an error.  Verify the reader and not the
 // decoder error is returned.

        new(r: new badReader(data: slice<byte>("Ma======"u8), errs: new error[]{badErr}.slice()),
            res: ""u8, err: badErr), // Reader returns valid data and io.EOF.  Check data is decoded and io.EOF is propagated.

        new(r: new badReader(data: slice<byte>("MZXW6YTB"u8), errs: new error[]{io.EOF}.slice()),
            res: "fooba"u8, err: io.EOF), // Check errors are properly reported when decoder.Read is called multiple times.
 // decoder.Read will be called 8 times, badReader.Read will be called twice, returning
 // valid data both times but an error on the second call.

        new(r: new badReader(data: slice<byte>("NRSWC43VOJSS4==="u8), errs: new error[]{default!, badErr}.slice()),
            res: "leasure."u8, err: badErr, dbuflen: 1), // Check io.EOF is properly reported when decoder.Read is called multiple times.
 // decoder.Read will be called 8 times, badReader.Read will be called twice, returning
 // valid data both times but io.EOF on the second call.

        new(r: new badReader(data: slice<byte>("NRSWC43VOJSS4==="u8), errs: new error[]{default!, io.EOF}.slice()),
            res: "leasure."u8, err: io.EOF, dbuflen: 1), // The following two test cases check that errors are propagated correctly when more than
 // 8 bytes are read at a time.

        new(r: new badReader(data: slice<byte>("NRSWC43VOJSS4==="u8), errs: new error[]{io.EOF}.slice()),
            res: "leasure."u8, err: io.EOF, dbuflen: 11),
        new(r: new badReader(data: slice<byte>("NRSWC43VOJSS4==="u8), errs: new error[]{badErr}.slice()),
            res: "leasure."u8, err: badErr, dbuflen: 11), // Check that errors are correctly propagated when the reader returns valid bytes in
 // groups that are not divisible by 8.  The first read will return 11 bytes and no
 // error.  The second will return 7 and an error.  The data should be decoded correctly
 // and the error should be propagated.

        new(r: new badReader(data: slice<byte>("NRSWC43VOJSS4==="u8), errs: new error[]{default!, badErr}.slice(), limit: 11),
            res: "leasure."u8, err: badErr)
    }.slice();
    foreach (var (_, vᴛ1) in testCases) {
        ref var tc = ref heap(new TestIssue20044_testCases(), out var Ꮡtc);
        tc = vᴛ1;

        var input = tc.r.data;
        var decoder = NewDecoder(StdEncoding, new base32_internal_test_package.badReaderжReader(Ꮡtc.of(TestIssue20044_testCases.Ꮡr)));
        nint dbuflen = default!;
        if (tc.dbuflen > 0){
            dbuflen = tc.dbuflen;
        } else {
            dbuflen = StdEncoding.DecodedLen(len(input));
        }
        var dbuf = new slice<byte>(dbuflen);
        error err = default!;
        slice<byte> res = default!;
        while (err == default!) {
            nint n = default!;
            (n, err) = decoder.Read(dbuf);
            if (n > 0) {
                res = appendꓸꓸꓸ(res, dbuf[..(int)(n)]);
            }
        }
        testEqual(Ꮡt, decodingOfQQWantQˢ, ((@string)input), ((@string)res), tc.res);
        testEqual(Ꮡt, decodingOfQErrVExpectedVˢ, ((@string)input), err, tc.err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mzxw6YTbˢ = "MZXW6YTb"u8;
internal static readonly @string readAfterEofNDExpectedDˢ = "Read after EOF, n = %d, expected %d"u8;

// TestDecoderError verifies decode errors are propagated when there are no read
// errors.
public static void TestDecoderError(ж<testing.T> Ꮡt) {
    foreach (var (_, readErr) in new error[]{io.EOF, default!}.slice()) {
        @string input = mzxw6YTbˢ;
        var dbuf = new slice<byte>(StdEncoding.DecodedLen(len(input)));
        ref var br = ref heap<badReader>(out var Ꮡbr);
        br = new badReader(data: slice<byte>(input), errs: new error[]{readErr}.slice());
        var decoder = NewDecoder(StdEncoding, new base32_internal_test_package.badReaderжReader(Ꮡbr));
        var (n, err) = decoder.Read(dbuf);
        testEqual(Ꮡt, readAfterEofNDExpectedDˢ, n, (nint)(0));
        {
            var (_, ok) = err._<CorruptInputError>(ᐧ); if (!ok) {
                Ꮡt.Errorf("Corrupt input error expected.  Found %T"u8, err);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mzxw6ytbˢ = "MZXW6YTB"u8;
internal static readonly @string readAfterEofErrVExpectedˢ = "Read after EOF, err = %v, expected %v"u8;

// TestReaderEOF ensures decoder.Read behaves correctly when input data is
// exhausted.
public static void TestReaderEOF(ж<testing.T> Ꮡt) {
    foreach (var (_, readErr) in new error[]{io.EOF, default!}.slice()) {
        @string input = mzxw6ytbˢ;
        ref var br = ref heap<badReader>(out var Ꮡbr);
        br = new badReader(data: slice<byte>(input), errs: new error[]{default!, readErr}.slice());
        var decoder = NewDecoder(StdEncoding, new base32_internal_test_package.badReaderжReader(Ꮡbr));
        var dbuf = new slice<byte>(StdEncoding.DecodedLen(len(input)));
        var (n, err) = decoder.Read(dbuf);
        testEqual(Ꮡt, decodingOfQErrVExpectedVˢ, input, err, ((error)default!));
        (n, err) = decoder.Read(dbuf);
        testEqual(Ꮡt, readAfterEofNDExpectedDˢ, n, (nint)(0));
        testEqual(Ꮡt, readAfterEofErrVExpectedˢ, err, io.EOF);
        (n, err) = decoder.Read(dbuf);
        testEqual(Ꮡt, readAfterEofNDExpectedDˢ, n, (nint)(0));
        testEqual(Ꮡt, readAfterEofErrVExpectedˢ, err, io.EOF);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string decodingDOfQQWantQˢ2 = "Decoding/%d of %q = %q, want %q"u8;

public static void TestDecoderBuffering(ж<testing.T> Ꮡt) {
    for (nint bs = 1; bs <= 12; bs++) {
        var decoder = NewDecoder(StdEncoding, new base32_test_package.strings_ReaderжReader(strings.NewReader(bigtest.encoded)));
        var buf = new slice<byte>(len(bigtest.decoded) + 12);
        nint total = default!;
        nint n = default!;
        error err = default!;
        for (total = 0; total < len(bigtest.decoded) && err == default!; ) {
            (n, err) = decoder.Read(buf[(int)(total)..(int)(total + bs)]);
            total += n;
        }
        if (err != default! && !AreEqual(err, io.EOF)) {
            Ꮡt.Errorf("Read from %q at pos %d = %d, unexpected error %v"u8, bigtest.encoded, total, n, err);
        }
        testEqual(Ꮡt, decodingDOfQQWantQˢ2, bs, bigtest.encoded, ((@string)(buf[0..(int)(total)])), bigtest.decoded);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decoderWronglyDetectedˢ = (@string)"Decoder wrongly detected corruption in"u8;
internal static readonly @string corruptionInQAtOffsetVˢ = "Corruption in %q at offset %v, want %v"u8;
internal static readonly object decoderFailedToDetectˢ = (@string)"Decoder failed to detect corruption in"u8;

[GoType("dyn")] internal partial struct TestDecodeCorrupt_testCases {
    internal @string input;
    internal nint offset; // -1 means no corruption.
}

public static void TestDecodeCorrupt(ж<testing.T> Ꮡt) {
    var testCases = new TestDecodeCorrupt_testCases[]{
        new(""u8, -1),
        new("!!!!"u8, 0),
        new("x==="u8, 0),
        new("AA=A===="u8, 2),
        new("AAA=AAAA"u8, 3),
        new("MMMMMMMMM"u8, 8),
        new("MMMMMM"u8, 0),
        new("A="u8, 1),
        new("AA="u8, 3),
        new("AA=="u8, 4),
        new("AA==="u8, 5),
        new("AAAA="u8, 5),
        new("AAAA=="u8, 6),
        new("AAAAA="u8, 6),
        new("AAAAA=="u8, 7),
        new("A======="u8, 1),
        new("AA======"u8, -1),
        new("AAA====="u8, 3),
        new("AAAA===="u8, -1),
        new("AAAAA==="u8, -1),
        new("AAAAAA=="u8, 6),
        new("AAAAAAA="u8, -1),
        new("AAAAAAAA"u8, -1)
    }.slice();
    foreach (var (_, tc) in testCases) {
        var dbuf = new slice<byte>(StdEncoding.DecodedLen(len(tc.input)));
        var (_, err) = StdEncoding.Decode(dbuf, slice<byte>(tc.input));
        if (tc.offset == -1) {
            if (err != default!) {
                Ꮡt.Error(decoderWronglyDetectedˢ, tc.input);
            }
            continue;
        }
        switch (err.type()) {
        case CorruptInputError errΔ1: {
            testEqual(Ꮡt, corruptionInQAtOffsetVˢ, tc.input, (nint)(int64)errΔ1, tc.offset);
            break;
        }
        default: {
            var errΔ1 = err;
            Ꮡt.Error(decoderFailedToDetectˢ, tc);
            break;
        }}
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string alphaᶜ = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"u8;

public static void TestBig(ж<testing.T> Ꮡt) {
    nint n = 3 * 1000 + 1;
    var raw = new slice<byte>(n);
    @string alpha = alphaᶜ;
    for (nint i = 0; i < n; i++) {
        raw[i] = alpha[i % len(alpha)];
    }
    var encoded = @new<bytes.Buffer>();
    var w = NewEncoder(StdEncoding, new base32_test_package.bytes_BufferжWriter(encoded));
    var (nn, err) = w.Write(raw);
    if (nn != n || err != default!) {
        Ꮡt.Fatalf("Encoder.Write(raw) = %d, %v want %d, nil"u8, nn, err, n);
    }
    err = w.Close();
    if (err != default!) {
        Ꮡt.Fatalf("Encoder.Close() = %v want nil"u8, err);
    }
    (var decoded, err) = io.ReadAll(NewDecoder(StdEncoding, new base32_test_package.bytes_BufferжReader(encoded)));
    if (err != default!) {
        Ꮡt.Fatalf("io.ReadAll(NewDecoder(...)): %v"u8, err);
    }
    if (!bytes.Equal(raw, decoded)) {
        nint i = default!;
        for (i = 0; i < len(decoded) && i < len(raw); i++) {
            if (decoded[i] != raw[i]) {
                break;
            }
        }
        Ꮡt.Errorf("Decode(Encode(%d-byte string)) failed at offset %d"u8, n, i);
    }
}

internal static void testStringEncoding(ж<testing.T> Ꮡt, @string expected, slice<@string> examples) {
    foreach (var (_, e) in examples) {
        var (buf, err) = StdEncoding.DecodeString(e);
        if (err != default!) {
            Ꮡt.Errorf("Decode(%q) failed: %v"u8, e, err);
            continue;
        }
        {
            @string s = ((@string)buf); if (s != expected) {
                Ꮡt.Errorf("Decode(%q) = %q, want %q"u8, e, s, expected);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sureˢ = "sure"u8;
internal static readonly @string foobarˢ = "foobar"u8;

public static void TestNewLineCharacters(ж<testing.T> Ꮡt) {
    // Each of these should decode to the string "sure", without errors.
    var examples = new @string[]{
        "ON2XEZI="u8,
        "ON2XEZI=\r"u8,
        "ON2XEZI=\n"u8,
        "ON2XEZI=\r\n"u8,
        "ON2XEZ\r\nI="u8,
        "ON2X\rEZ\nI="u8,
        "ON2X\nEZ\rI="u8,
        "ON2XEZ\nI="u8,
        "ON2XEZI\n="u8
    }.slice();
    testStringEncoding(Ꮡt, sureˢ, examples);
    // Each of these should decode to the string "foobar", without errors.
    examples = new @string[]{
        "MZXW6YTBOI======"u8,
        "MZXW6YTBOI=\r\n====="u8
    }.slice();
    testStringEncoding(Ꮡt, foobarˢ, examples);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object decodedResultsNotEqualˢ = (@string)"Decoded results not equal"u8;

public static void TestDecoderIssue4779(ж<testing.T> Ꮡt) {
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
"""u8;
    @string encodedShort = strings.ReplaceAll(encoded, "\n"u8, ""u8);
    var dec = NewDecoder(StdEncoding, new base32_test_package.strings_ReaderжReader(strings.NewReader(encoded)));
    var (res1, err) = io.ReadAll(dec);
    if (err != default!) {
        Ꮡt.Errorf("ReadAll failed: %v"u8, err);
    }
    dec = NewDecoder(StdEncoding, new base32_test_package.strings_ReaderжReader(strings.NewReader(encodedShort)));
    slice<byte> res2 = default!;
    (res2, err) = io.ReadAll(dec);
    if (err != default!) {
        Ꮡt.Errorf("ReadAll failed: %v"u8, err);
    }
    if (!bytes.Equal(res1, res2)) {
        Ꮡt.Error(decodedResultsNotEqualˢ);
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

public static void TestWithCustomPadding(ж<testing.T> Ꮡt) {
    foreach (var (_, testcase) in pairs) {
        @string defaultPadding = StdEncoding.EncodeToString(slice<byte>(testcase.decoded));
        @string customPadding = (~StdEncoding).WithPadding((rune)'@').EncodeToString(slice<byte>(testcase.decoded));
        @string expected = strings.ReplaceAll(defaultPadding, "="u8, "@"u8);
        if (expected != customPadding) {
            Ꮡt.Errorf("Expected custom %s, got %s"u8, expected, customPadding);
        }
        if (testcase.encoded != defaultPadding) {
            Ꮡt.Errorf("Expected %s, got %s"u8, testcase.encoded, defaultPadding);
        }
    }
}

public static void TestWithoutPadding(ж<testing.T> Ꮡt) {
    foreach (var (_, testcase) in pairs) {
        @string defaultPadding = StdEncoding.EncodeToString(slice<byte>(testcase.decoded));
        @string customPadding = (~StdEncoding).WithPadding(NoPadding).EncodeToString(slice<byte>(testcase.decoded));
        @string expected = strings.TrimRight(defaultPadding, "="u8);
        if (expected != customPadding) {
            Ꮡt.Errorf("Expected custom %s, got %s"u8, expected, customPadding);
        }
        if (testcase.encoded != defaultPadding) {
            Ꮡt.Errorf("Expected %s, got %s"u8, testcase.encoded, defaultPadding);
        }
    }
}

public static void TestDecodeWithPadding(ж<testing.T> Ꮡt) {
    var encodings = new ж<global::go.encoding.base32_package.Encoding>[]{
        StdEncoding,
        (~StdEncoding).WithPadding((rune)'-'),
        (~StdEncoding).WithPadding(NoPadding)
    }.slice();
    foreach (var (i, enc) in encodings) {
        foreach (var (_, pair) in pairs) {
            @string input = pair.decoded;
            @string encoded = enc.EncodeToString(slice<byte>(input));
            var (decoded, err) = enc.DecodeString(encoded);
            if (err != default!) {
                Ꮡt.Errorf("DecodeString Error for encoding %d (%q): %v"u8, i, input, err);
            }
            if (input != ((sstring)decoded)) {
                Ꮡt.Errorf("Unexpected result for encoding %d: got %q; want %q"u8, i, decoded, input);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorˢ = (@string)"expected error"u8;

public static void TestDecodeWithWrongPadding(ж<testing.T> Ꮡt) {
    @string encoded = StdEncoding.EncodeToString(slice<byte>("foobar"u8));
    var (_, err) = (~StdEncoding).WithPadding((rune)'-').DecodeString(encoded);
    if (err == default!) {
        Ꮡt.Error(expectedErrorˢ);
    }
    (_, err) = (~StdEncoding).WithPadding(NoPadding).DecodeString(encoded);
    if (err == default!) {
        Ꮡt.Error(expectedErrorˢ);
    }
}

[GoType("dyn")] internal partial struct TestBufferedDecodingSameError_testcases {
    internal @string prefix;
    internal slice<slice<@string>> chunkCombinations;
    internal error expected;
}

public static void TestBufferedDecodingSameError(ж<testing.T> Ꮡt) {
    var testcases = new TestBufferedDecodingSameError_testcases[]{ // NBSWY3DPO5XXE3DE == helloworld
 // Test with "ZZ" as extra input

        new("helloworld"u8, new slice<@string>[]{
            new @string[]{"NBSW"u8, "Y3DP"u8, "O5XX"u8, "E3DE"u8, "ZZ"u8}.slice(),
            new @string[]{"NBSWY3DPO5XXE3DE"u8, "ZZ"u8}.slice(),
            new @string[]{"NBSWY3DPO5XXE3DEZZ"u8}.slice(),
            new @string[]{"NBS"u8, "WY3"u8, "DPO"u8, "5XX"u8, "E3D"u8, "EZZ"u8}.slice(),
            new @string[]{"NBSWY3DPO5XXE3"u8, "DEZZ"u8}.slice()
        }.slice(), io.ErrUnexpectedEOF), // Test with "ZZY" as extra input

        new("helloworld"u8, new slice<@string>[]{
            new @string[]{"NBSW"u8, "Y3DP"u8, "O5XX"u8, "E3DE"u8, "ZZY"u8}.slice(),
            new @string[]{"NBSWY3DPO5XXE3DE"u8, "ZZY"u8}.slice(),
            new @string[]{"NBSWY3DPO5XXE3DEZZY"u8}.slice(),
            new @string[]{"NBS"u8, "WY3"u8, "DPO"u8, "5XX"u8, "E3D"u8, "EZZY"u8}.slice(),
            new @string[]{"NBSWY3DPO5XXE3"u8, "DEZZY"u8}.slice()
        }.slice(), io.ErrUnexpectedEOF), // Normal case, this is valid input

        new("helloworld"u8, new slice<@string>[]{
            new @string[]{"NBSW"u8, "Y3DP"u8, "O5XX"u8, "E3DE"u8}.slice(),
            new @string[]{"NBSWY3DPO5XXE3DE"u8}.slice(),
            new @string[]{"NBS"u8, "WY3"u8, "DPO"u8, "5XX"u8, "E3D"u8, "E"u8}.slice(),
            new @string[]{"NBSWY3DPO5XXE3"u8, "DE"u8}.slice()
        }.slice(), default!), // MZXW6YTB = fooba

        new("fooba"u8, new slice<@string>[]{
            new @string[]{"MZXW6YTBZZ"u8}.slice(),
            new @string[]{"MZXW6YTBZ"u8, "Z"u8}.slice(),
            new @string[]{"MZXW6YTB"u8, "ZZ"u8}.slice(),
            new @string[]{"MZXW6YT"u8, "BZZ"u8}.slice(),
            new @string[]{"MZXW6Y"u8, "TBZZ"u8}.slice(),
            new @string[]{"MZXW6Y"u8, "TB"u8, "ZZ"u8}.slice(),
            new @string[]{"MZXW6"u8, "YTBZZ"u8}.slice(),
            new @string[]{"MZXW6"u8, "YTB"u8, "ZZ"u8}.slice(),
            new @string[]{"MZXW6"u8, "YT"u8, "BZZ"u8}.slice()
        }.slice(), io.ErrUnexpectedEOF), // Normal case, this is valid input

        new("fooba"u8, new slice<@string>[]{
            new @string[]{"MZXW6YTB"u8}.slice(),
            new @string[]{"MZXW6YT"u8, "B"u8}.slice(),
            new @string[]{"MZXW6Y"u8, "TB"u8}.slice(),
            new @string[]{"MZXW6"u8, "YTB"u8}.slice(),
            new @string[]{"MZXW6"u8, "YT"u8, "B"u8}.slice(),
            new @string[]{"MZXW"u8, "6YTB"u8}.slice(),
            new @string[]{"MZXW"u8, "6Y"u8, "TB"u8}.slice()
        }.slice(), default!)
    }.slice();
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
                Ꮡt.Errorf("Expected %v, got %v; case %s %+v"u8, testcase.expected, err, testcase.prefix, chunks);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestBufferedDecodingPadding_testcases {
    internal slice<@string> chunks;
    internal @string expectedError;
}

public static void TestBufferedDecodingPadding(ж<testing.T> Ꮡt) {
    var testcases = new TestBufferedDecodingPadding_testcases[]{
        new(new @string[]{
            "I4======"u8,
            "=="u8
        }.slice(), "unexpected EOF"u8),
        new(new @string[]{
            "I4======N4======"u8
        }.slice(), "illegal base32 data at input byte 2"u8),
        new(new @string[]{
            "I4======"u8,
            "N4======"u8
        }.slice(), "illegal base32 data at input byte 0"u8),
        new(new @string[]{
            "I4======"u8,
            "========"u8
        }.slice(), "illegal base32 data at input byte 0"u8),
        new(new @string[]{
            "I4I4I4I4"u8,
            "I4======"u8,
            "I4======"u8
        }.slice(), "illegal base32 data at input byte 0"u8)
    }.slice();
    foreach (var (_, testcase) in testcases) {
        ref var testcaseΔ1 = ref heap<TestBufferedDecodingPadding_testcases>(out var ᏑtestcaseΔ1);
        testcaseΔ1 = testcase;
        var (pr, pw) = io.Pipe();
        var pwʗ1 = pw;
        var testcaseʗ1 = testcaseΔ1;
        goǃ(() => {
            foreach (var (_, chunk) in testcaseʗ1.chunks) {
                (_, _) = pwʗ1.Write(slice<byte>(chunk));
            }
            _ = pwʗ1.Close();
        });
        var decoder = NewDecoder(StdEncoding, new io.PipeReaderжReader(pr));
        var (_, err) = io.ReadAll(decoder);
        if (err == default! && len(testcaseΔ1.expectedError) != 0){
            Ꮡt.Errorf("case %q: got nil error, want %v"u8, testcaseΔ1.chunks, testcaseΔ1.expectedError);
        } else 
        if (err.Error() != testcaseΔ1.expectedError) {
            Ꮡt.Errorf("case %q: got %v, want %v"u8, testcaseΔ1.chunks, err, testcaseΔ1.expectedError);
        }
    }
}

[GoType("dyn")] [GoLocalName("test")] internal partial struct TestEncodedLen_test {
    internal ж<global::go.encoding.base32_package.Encoding> enc;
    internal nint n;
    internal int64 want;
}

public static void TestEncodedLen(ж<testing.T> Ꮡt) {
    ж<global::go.encoding.base32_package.Encoding> rawStdEncoding = (~StdEncoding).WithPadding(NoPadding);
    var tests = new TestEncodedLen_test[]{
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
    }.slice();
    // check overflow
    var exprᴛ1 = strconv.IntSize;
    if (exprᴛ1 == 32) {
        tests = append(tests, new TestEncodedLen_test(rawStdEncoding, unchecked((nint)(1152921504606846976L)), 429496730));
        tests = append(tests, new TestEncodedLen_test(rawStdEncoding, unchecked((nint)(5764607523034234879L)), math.MaxInt));
    }
    else if (exprᴛ1 == 64) {
        tests = append(tests, new TestEncodedLen_test(rawStdEncoding, unchecked((nint)(1152921504606846976L)), 1844674407370955162L));
        tests = append(tests, new TestEncodedLen_test(rawStdEncoding, unchecked((nint)(5764607523034234879L)), math.MaxInt));
    }

    foreach (var (_, tt) in tests) {
        {
            nint got = tt.enc.EncodedLen(tt.n); if ((int64)got != tt.want) {
                Ꮡt.Errorf("EncodedLen(%d): got %d, want %d"u8, tt.n, got, tt.want);
            }
        }
    }
}

[GoType("dyn")] [GoLocalName("test")] internal partial struct TestDecodedLen_test {
    internal ж<global::go.encoding.base32_package.Encoding> enc;
    internal nint n;
    internal int64 want;
}

public static void TestDecodedLen(ж<testing.T> Ꮡt) {
    ж<global::go.encoding.base32_package.Encoding> rawStdEncoding = (~StdEncoding).WithPadding(NoPadding);
    var tests = new TestDecodedLen_test[]{
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
    }.slice();
    // check overflow
    var exprᴛ1 = strconv.IntSize;
    if (exprᴛ1 == 32) {
        tests = append(tests, new TestDecodedLen_test(rawStdEncoding, unchecked((nint)(1844674407370955162L)), 268435456));
        tests = append(tests, new TestDecodedLen_test(rawStdEncoding, math.MaxInt, 1342177279));
    }
    else if (exprᴛ1 == 64) {
        tests = append(tests, new TestDecodedLen_test(rawStdEncoding, unchecked((nint)(1844674407370955162L)), 1152921504606846976L));
        tests = append(tests, new TestDecodedLen_test(rawStdEncoding, math.MaxInt, 5764607523034234879L));
    }

    foreach (var (_, tt) in tests) {
        {
            nint got = tt.enc.DecodedLen(tt.n); if ((int64)got != tt.want) {
                Ꮡt.Errorf("DecodedLen(%d): got %d, want %d"u8, tt.n, got, tt.want);
            }
        }
    }
}

public static void TestWithoutPaddingClose(ж<testing.T> Ꮡt) {
    var encodings = new ж<global::go.encoding.base32_package.Encoding>[]{
        StdEncoding,
        (~StdEncoding).WithPadding(NoPadding)
    }.slice();
    foreach (var (_, encoding) in encodings) {
        foreach (var (_, testpair) in pairs) {
            ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
            var encoder = NewEncoder(encoding, new base32_test_package.strings_BuilderжWriter(Ꮡbuf));
            encoder.Write(slice<byte>(testpair.decoded));
            encoder.Close();
            @string expected = testpair.encoded;
            if ((~encoding).padChar == NoPadding) {
                expected = strings.ReplaceAll(expected, "="u8, ""u8);
            }
            @string res = buf.String();
            if (res != expected) {
                Ꮡt.Errorf("Expected %s got %s; padChar=%d"u8, expected, res, (~encoding).padChar);
            }
        }
    }
}

public static void TestDecodeReadAll(ж<testing.T> Ꮡt) {
    var encodings = new ж<global::go.encoding.base32_package.Encoding>[]{
        StdEncoding,
        (~StdEncoding).WithPadding(NoPadding)
    }.slice();
    foreach (var (_, pair) in pairs) {
        foreach (var (encIndex, encoding) in encodings) {
            @string encoded = pair.encoded;
            if ((~encoding).padChar == NoPadding) {
                encoded = strings.ReplaceAll(encoded, "="u8, ""u8);
            }
            var (decReader, err) = io.ReadAll(NewDecoder(encoding, new base32_test_package.strings_ReaderжReader(strings.NewReader(encoded))));
            if (err != default!) {
                Ꮡt.Errorf("NewDecoder error: %v"u8, err);
            }
            if (pair.decoded != ((sstring)decReader)) {
                Ꮡt.Errorf("Expected %s got %s; Encoding %d"u8, pair.decoded, decReader, encIndex);
            }
        }
    }
}

public static void TestDecodeSmallBuffer(ж<testing.T> Ꮡt) {
    var encodings = new ж<global::go.encoding.base32_package.Encoding>[]{
        StdEncoding,
        (~StdEncoding).WithPadding(NoPadding)
    }.slice();
    for (nint bufferSize = 1; bufferSize < 200; bufferSize++) {
        foreach (var (_, pair) in pairs) {
            foreach (var (encIndex, encoding) in encodings) {
                @string encoded = pair.encoded;
                if ((~encoding).padChar == NoPadding) {
                    encoded = strings.ReplaceAll(encoded, "="u8, ""u8);
                }
                var decoder = NewDecoder(encoding, new base32_test_package.strings_ReaderжReader(strings.NewReader(encoded)));
                slice<byte> allRead = default!;
                while (ᐧ) {
                    var buf = new slice<byte>(bufferSize);
                    var (n, err) = decoder.Read(buf);
                    allRead = appendꓸꓸꓸ(allRead, buf[0..(int)(n)]);
                    if (AreEqual(err, io.EOF)) {
                        break;
                    }
                    if (err != default!) {
                        Ꮡt.Error(err);
                    }
                }
                if (pair.decoded != ((sstring)allRead)) {
                    Ꮡt.Errorf("Expected %s got %s; Encoding %d; bufferSize %d"u8, pair.decoded, allRead, encIndex, bufferSize);
                }
            }
        }
    }
}

} // end base32_internal_test_package
