// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static bytes_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δio = io_package;
using rand = go.math.rand_package;
using strconv = strconv_package;
using testing = testing_package;
using utf8 = go.unicode.utf8_package;
using @internal;
using bytes = bytes_package;
using go.math;
using go.unicode;
using static go.bytes_internal_test_package;

partial class bytes_test_package {

public static UntypedInt N => 10000; // make this bigger for a larger (and slower) test

internal static @string testString; // test data for write tests

internal static slice<byte> testBytes; // test data; same as testString but as a slice.

[GoType] partial struct negativeReader {
}

[GoRecv] internal static (nint, error) Read(this ref negativeReader r, slice<byte> _) {
    return (-1, default!);
}

[GoInit] internal static void init() {
    testBytes = new slice<byte>(N);
    for (nint i = 0; i < N; i++) {
        testBytes[i] = (byte)((rune)'a' + (byte)(i % 26));
    }
    testString = ((@string)testBytes);
}

// Verify that contents of buf match the string s.
internal static void check(ж<testing.T> Ꮡt, @string testname, ж<bytes.Buffer> Ꮡbuf, @string s) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    var bytes = buf.Bytes();
    @string str = Ꮡbuf.String();
    if (buf.Len() != len(bytes)) {
        Ꮡt.Errorf("%s: buf.Len() == %d, len(buf.Bytes()) == %d"u8, testname, buf.Len(), len(bytes));
    }
    if (buf.Len() != len(str)) {
        Ꮡt.Errorf("%s: buf.Len() == %d, len(buf.String()) == %d"u8, testname, buf.Len(), len(str));
    }
    if (buf.Len() != len(s)) {
        Ꮡt.Errorf("%s: buf.Len() == %d, len(s) == %d"u8, testname, buf.Len(), len(s));
    }
    if (((sstring)bytes) != s) {
        Ꮡt.Errorf("%s: string(buf.Bytes()) == %q, s == %q"u8, testname, ((@string)bytes), s);
    }
}

// Fill buf through n writes of string fus.
// The initial contents of buf corresponds to the string s;
// the result is the final contents of buf returned as a string.
internal static @string fillString(ж<testing.T> Ꮡt, @string testname, ж<bytes.Buffer> Ꮡbuf, @string s, nint n, @string fus) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    check(Ꮡt, testname + " (fill 1)"u8, Ꮡbuf, s);
    for (; n > 0; n--) {
        var (m, err) = buf.WriteString(fus);
        if (m != len(fus)) {
            Ꮡt.Errorf(testname + " (fill 2): m == %d, expected %d"u8, m, len(fus));
        }
        if (err != default!) {
            Ꮡt.Errorf(testname + " (fill 3): err should always be nil, found err == %s"u8, err);
        }
        s += fus;
        check(Ꮡt, testname + " (fill 4)"u8, Ꮡbuf, s);
    }
    return s;
}

// Fill buf through n writes of byte slice fub.
// The initial contents of buf corresponds to the string s;
// the result is the final contents of buf returned as a string.
internal static @string fillBytes(ж<testing.T> Ꮡt, @string testname, ж<bytes.Buffer> Ꮡbuf, @string s, nint n, slice<byte> fub) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    check(Ꮡt, testname + " (fill 1)"u8, Ꮡbuf, s);
    for (; n > 0; n--) {
        var (m, err) = buf.Write(fub);
        if (m != len(fub)) {
            Ꮡt.Errorf(testname + " (fill 2): m == %d, expected %d"u8, m, len(fub));
        }
        if (err != default!) {
            Ꮡt.Errorf(testname + " (fill 3): err should always be nil, found err == %s"u8, err);
        }
        s += ((@string)fub);
        check(Ꮡt, testname + " (fill 4)"u8, Ꮡbuf, s);
    }
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string newBufferˢ = "NewBuffer"u8;

public static void TestNewBuffer(ж<testing.T> Ꮡt) {
    var buf = NewBuffer(testBytes);
    check(Ꮡt, newBufferˢ, buf, testString);
}

internal static ж<bytes.Buffer> Ꮡbuf = new StandardBox<bytes.Buffer>(default(bytes.Buffer));
internal static ref bytes.Buffer buf => ref Ꮡbuf.Value;

// Calling NewBuffer and immediately shallow copying the Buffer struct
// should not result in any allocations.
// This can be used to reset the underlying []byte of an existing Buffer.
public static void TestNewBufferShallow(ж<testing.T> Ꮡt) {
    testenv.SkipIfOptimizationOff(new bytes_test_package.testing_TжTB(Ꮡt));
    var n = testing.AllocsPerRun(1000, () => {
        buf = NewBuffer(testBytes).Value;
    });
    if (n > 0D) {
        Ꮡt.Errorf("allocations occurred while shallow copying"u8);
    }
    check(Ꮡt, newBufferˢ, Ꮡbuf, testString);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string newBufferStringˢ = "NewBufferString"u8;

public static void TestNewBufferString(ж<testing.T> Ꮡt) {
    var buf = NewBufferString(testString);
    check(Ꮡt, newBufferStringˢ, buf, testString);
}

// Empty buf through repeated reads into fub.
// The initial contents of buf corresponds to the string s.
internal static void empty(ж<testing.T> Ꮡt, @string testname, ж<bytes.Buffer> Ꮡbuf, @string s, slice<byte> fub) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    check(Ꮡt, testname + " (empty 1)"u8, Ꮡbuf, s);
    while (ᐧ) {
        var (n, err) = buf.Read(fub);
        if (n == 0) {
            break;
        }
        if (err != default!) {
            Ꮡt.Errorf(testname + " (empty 2): err should always be nil, found err == %s"u8, err);
        }
        s = s.slice(n);
        check(Ꮡt, testname + " (empty 3)"u8, Ꮡbuf, s);
    }
    check(Ꮡt, testname + " (empty 4)"u8, Ꮡbuf, ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testBasicOperations1ˢ = "TestBasicOperations (1)"u8;
internal static readonly @string testBasicOperations2ˢ = "TestBasicOperations (2)"u8;
internal static readonly @string testBasicOperations3ˢ = "TestBasicOperations (3)"u8;
internal static readonly @string testBasicOperations4ˢ = "TestBasicOperations (4)"u8;
internal static readonly @string testBasicOperations5ˢ = "TestBasicOperations (5)"u8;
internal static readonly @string testBasicOperations6ˢ = "TestBasicOperations (6)"u8;
internal static readonly @string testBasicOperations7ˢ = "TestBasicOperations (7)"u8;
internal static readonly @string testBasicOperations8ˢ = "TestBasicOperations (8)"u8;
internal static readonly @string testBasicOperations9ˢ = "TestBasicOperations (9)"u8;
internal static readonly @string testBasicOperations10ˢ = "TestBasicOperations (10)"u8;

public static void TestBasicOperations(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 0; i < 5; i++) {
        check(Ꮡt, testBasicOperations1ˢ, Ꮡbuf, ""u8);
        buf.Reset();
        check(Ꮡt, testBasicOperations2ˢ, Ꮡbuf, ""u8);
        buf.Truncate(0);
        check(Ꮡt, testBasicOperations3ˢ, Ꮡbuf, ""u8);
        var (n, err) = buf.Write(testBytes[0..1]);
        {
            nint want = 1; if (err != default! || n != want) {
                Ꮡt.Errorf("Write: got (%d, %v), want (%d, %v)"u8, n, err, want, (any)(default!));
            }
        }
        check(Ꮡt, testBasicOperations4ˢ, Ꮡbuf, "a"u8);
        buf.WriteByte(testString[1]);
        check(Ꮡt, testBasicOperations5ˢ, Ꮡbuf, "ab"u8);
        (n, err) = buf.Write(testBytes[2..26]);
        {
            nint want = 24; if (err != default! || n != want) {
                Ꮡt.Errorf("Write: got (%d, %v), want (%d, %v)"u8, n, err, want, (any)(default!));
            }
        }
        check(Ꮡt, testBasicOperations6ˢ, Ꮡbuf, testString[0..26]);
        buf.Truncate(26);
        check(Ꮡt, testBasicOperations7ˢ, Ꮡbuf, testString[0..26]);
        buf.Truncate(20);
        check(Ꮡt, testBasicOperations8ˢ, Ꮡbuf, testString[0..20]);
        empty(Ꮡt, testBasicOperations9ˢ, Ꮡbuf, testString[0..20], new slice<byte>(5));
        empty(Ꮡt, testBasicOperations10ˢ, Ꮡbuf, ""u8, new slice<byte>(100));
        buf.WriteByte(testString[1]);
        (var c, err) = buf.ReadByte();
        {
            var want = testString[1]; if (err != default! || c != want) {
                Ꮡt.Errorf("ReadByte: got (%q, %v), want (%q, %v)"u8, c, err, want, (any)(default!));
            }
        }
        (c, err) = buf.ReadByte();
        if (!AreEqual(err, Δio.EOF)) {
            Ꮡt.Errorf("ReadByte: got (%q, %v), want (%q, %v)"u8, c, err, (byte)0, Δio.EOF);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testLargeWrites1ˢ = "TestLargeWrites (1)"u8;
internal static readonly @string testLargeStringWrites2ˢ = "TestLargeStringWrites (2)"u8;
internal static readonly @string testLargeStringWrites3ˢ = "TestLargeStringWrites (3)"u8;

public static void TestLargeStringWrites(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    nint limit = 30;
    if (testing.Short()) {
        limit = 9;
    }
    for (nint i = 3; i < limit; i += 3) {
        @string s = fillString(Ꮡt, testLargeWrites1ˢ, Ꮡbuf, ""u8, 5, testString);
        empty(Ꮡt, testLargeStringWrites2ˢ, Ꮡbuf, s, new slice<byte>(len(testString) / i));
    }
    check(Ꮡt, testLargeStringWrites3ˢ, Ꮡbuf, ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testLargeByteWrites2ˢ = "TestLargeByteWrites (2)"u8;
internal static readonly @string testLargeByteWrites3ˢ = "TestLargeByteWrites (3)"u8;

public static void TestLargeByteWrites(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    nint limit = 30;
    if (testing.Short()) {
        limit = 9;
    }
    for (nint i = 3; i < limit; i += 3) {
        @string s = fillBytes(Ꮡt, testLargeWrites1ˢ, Ꮡbuf, ""u8, 5, testBytes);
        empty(Ꮡt, testLargeByteWrites2ˢ, Ꮡbuf, s, new slice<byte>(len(testString) / i));
    }
    check(Ꮡt, testLargeByteWrites3ˢ, Ꮡbuf, ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testLargeReads1ˢ = "TestLargeReads (1)"u8;
internal static readonly @string testLargeReads2ˢ = "TestLargeReads (2)"u8;
internal static readonly @string testLargeStringReads3ˢ = "TestLargeStringReads (3)"u8;

public static void TestLargeStringReads(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 3; i < 30; i += 3) {
        @string s = fillString(Ꮡt, testLargeReads1ˢ, Ꮡbuf, ""u8, 5, testString.slice(0, len(testString) / i));
        empty(Ꮡt, testLargeReads2ˢ, Ꮡbuf, s, new slice<byte>(len(testString)));
    }
    check(Ꮡt, testLargeStringReads3ˢ, Ꮡbuf, ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testLargeByteReads3ˢ = "TestLargeByteReads (3)"u8;

public static void TestLargeByteReads(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 3; i < 30; i += 3) {
        @string s = fillBytes(Ꮡt, testLargeReads1ˢ, Ꮡbuf, ""u8, 5, testBytes.slice(0, len(testBytes) / i));
        empty(Ꮡt, testLargeReads2ˢ, Ꮡbuf, s, new slice<byte>(len(testString)));
    }
    check(Ꮡt, testLargeByteReads3ˢ, Ꮡbuf, ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testMixedReadsAndWrites1ˢ = "TestMixedReadsAndWrites (1)"u8;
internal static readonly @string testMixedReadsAndWrites2ˢ = "TestMixedReadsAndWrites (2)"u8;

public static void TestMixedReadsAndWrites(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    @string s = ""u8;
    for (nint i = 0; i < 50; i++) {
        nint wlen = rand.Intn(len(testString));
        if (i % 2 == 0){
            s = fillString(Ꮡt, testMixedReadsAndWrites1ˢ, Ꮡbuf, s, 1, testString.slice(0, wlen));
        } else {
            s = fillBytes(Ꮡt, testMixedReadsAndWrites1ˢ, Ꮡbuf, s, 1, testBytes.slice(0, wlen));
        }
        nint rlen = rand.Intn(len(testString));
        var fub = new slice<byte>(rlen);
        var (n, _) = buf.Read(fub);
        s = s.slice(n);
    }
    empty(Ꮡt, testMixedReadsAndWrites2ˢ, Ꮡbuf, s, new slice<byte>(buf.Len()));
}

public static void TestCapWithPreallocatedSlice(ж<testing.T> Ꮡt) {
    var buf = NewBuffer(new slice<byte>(10));
    nint n = buf.Cap();
    if (n != 10) {
        Ꮡt.Errorf("expected 10, got %d"u8, n);
    }
}

public static void TestCapWithSliceAndWrittenData(ж<testing.T> Ꮡt) {
    var buf = NewBuffer(new slice<byte>(0, 10));
    buf.Write(slice<byte>("test"u8));
    nint n = buf.Cap();
    if (n != 10) {
        Ꮡt.Errorf("expected 10, got %d"u8, n);
    }
}

public static void TestNil(ж<testing.T> Ꮡt) {
    ж<bytes.Buffer> b = default!;
    if (b.String() != "<nil>"u8) {
        Ꮡt.Errorf("expected <nil>; got %q"u8, b.String());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testReadFrom1ˢ = "TestReadFrom (1)"u8;
internal static readonly @string testReadFrom2ˢ = "TestReadFrom (2)"u8;

public static void TestReadFrom(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 3; i < 30; i += 3) {
        @string s = fillBytes(Ꮡt, testReadFrom1ˢ, Ꮡbuf, ""u8, 5, testBytes.slice(0, len(testBytes) / i));
        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        b.ReadFrom(new bytes_test_package.bytes_BufferжReader(Ꮡbuf));
        empty(Ꮡt, testReadFrom2ˢ, Ꮡb, s, new slice<byte>(len(testString)));
    }
}

[GoType] partial struct panicReader {
    internal bool panic;
}

internal static (nint, error) Read(this panicReader r, slice<byte> p) {
    if (r.panic) {
        throw panic("oops");
    }
    return (0, Δio.EOF);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testReadFromPanicReader1ˢ = "TestReadFromPanicReader (1)"u8;
internal static readonly @string testReadFromPanicReader2ˢ = "TestReadFromPanicReader (2)"u8;

// Make sure that an empty Buffer remains empty when
// it is "grown" before a Read that panics
public static void TestReadFromPanicReader(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // First verify non-panic behaviour
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        var (i, err) = buf.ReadFrom(new panicReader(nil));
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (i != 0) {
            Ꮡt.Fatalf("unexpected return from bytes.ReadFrom (1): got: %d, want %d"u8, i, (nint)(0));
        }
        check(Ꮡt, testReadFromPanicReader1ˢ, Ꮡbuf, ""u8);
        // Confirm that when Reader panics, the empty buffer remains empty
        ref var buf2 = ref heap(new bytes.Buffer(), out var Ꮡbuf2);
        defer(() => {
            recover();
            check(Ꮡt, testReadFromPanicReader2ˢ, Ꮡbuf2, ""u8);
        }, ref ᒐ);
        buf2.ReadFrom(new panicReader(panic: true));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bytesBufferReadFromDidnTˢ = (@string)"bytes.Buffer.ReadFrom didn't panic"u8;
internal static readonly @string bytesBufferReaderˢ = "bytes.Buffer: reader returned negative count from Read"u8;

public static void TestReadFromNegativeReader(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        bytes.Buffer b = default!;
        defer(() => {
            var switchᴛ1 = recover();
            switch (switchᴛ1.type()) {
            case null: {
                Ꮡt.Fatal(bytesBufferReadFromDidnTˢ);
                break;
            }
            case {} Δerr when Δerr._<error>(out var err): {
                @string wantError = bytesBufferReaderˢ;
                if (err.Error() != wantError) {
                    // this is the error string of errNegativeRead
                    Ꮡt.Fatalf("recovered panic: got %v, want %v"u8, err.Error(), wantError);
                }
                break;
            }
            default: {
                var err = switchᴛ1;
                Ꮡt.Fatalf("unexpected panic value: %#v"u8, err);
                break;
            }}
        }, ref ᒐ);
        b.ReadFrom(new bytes_test_package.negativeReaderжReader(@new<negativeReader>()));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testWriteTo1ˢ = "TestWriteTo (1)"u8;
internal static readonly @string testWriteTo2ˢ = "TestWriteTo (2)"u8;

public static void TestWriteTo(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 3; i < 30; i += 3) {
        @string s = fillBytes(Ꮡt, testWriteTo1ˢ, Ꮡbuf, ""u8, 5, testBytes.slice(0, len(testBytes) / i));
        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        buf.WriteTo(new bytes_test_package.bytes_BufferжWriter(Ꮡb));
        empty(Ꮡt, testWriteTo2ˢ, Ꮡb, s, new slice<byte>(len(testString)));
    }
}

public static void TestWriteAppend(ж<testing.T> Ꮡt) {
    ref var got = ref heap(new bytes.Buffer(), out var Ꮡgot);
    slice<byte> want = default!;
    for (nint i = 0; i < 1000; i++) {
        var b = got.AvailableBuffer();
        b = strconv.AppendInt(b, (int64)i, 10);
        want = strconv.AppendInt(want, (int64)i, 10);
        got.Write(b);
    }
    if (!Equal(got.Bytes(), want)) {
        Ꮡt.Fatalf("Bytes() = %q, want %q"u8, got, want);
    }
    // With a sufficiently sized buffer, there should be no allocations.
    var n = testing.AllocsPerRun(100, () => {
        Ꮡgot.Value.Reset();
        for (nint i = 0; i < 1000; i++) {
            var b = Ꮡgot.Value.AvailableBuffer();
            b = strconv.AppendInt(b, (int64)i, 10);
            Ꮡgot.Value.Write(b);
        }
    });
    if (n > 0D) {
        Ꮡt.Errorf("allocations occurred while appending"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadRuneAtEofGotNoˢ = (@string)"UnreadRune at EOF: got no error"u8;
internal static readonly object readRuneAtEofGotNoErrorˢ = (@string)"ReadRune at EOF: got no error"u8;
internal static readonly object unreadRuneAfterReadRuneˢ = (@string)"UnreadRune after ReadRune at EOF: got no error"u8;

public static void TestRuneIO(ж<testing.T> Ꮡt) {
    UntypedInt NRune = 1000;
    // Built a test slice while we write the data
    var b = new slice<byte>(utf8.UTFMax * NRune);
    bytes.Buffer buf = default!;
    nint n = 0;
    for (var r = (rune)0; r < NRune; r++) {
        nint size = utf8.EncodeRune(b.slice(n), r);
        var (nbytes, err) = buf.WriteRune(r);
        if (err != default!) {
            Ꮡt.Fatalf("WriteRune(%U) error: %s"u8, r, err);
        }
        if (nbytes != size) {
            Ꮡt.Fatalf("WriteRune(%U) expected %d, got %d"u8, r, size, nbytes);
        }
        n += size;
    }
    b = b.slice(0, n);
    // Check the resulting bytes
    if (!Equal(buf.Bytes(), b)) {
        Ꮡt.Fatalf("incorrect result from WriteRune: %q not %q"u8, buf.Bytes(), b);
    }
    var p = new slice<byte>(utf8.UTFMax);
    // Read it back with ReadRune
    for (var r = (rune)0; r < NRune; r++) {
        nint size = utf8.EncodeRune(p, r);
        var (nr, nbytes, err) = buf.ReadRune();
        if (nr != r || nbytes != size || err != default!) {
            Ꮡt.Fatalf("ReadRune(%U) got %U,%d not %U,%d (err=%s)"u8, r, nr, nbytes, r, size, err);
        }
    }
    // Check that UnreadRune works
    buf.Reset();
    // check at EOF
    {
        var err = buf.UnreadRune(); if (err == default!) {
            Ꮡt.Fatal(unreadRuneAtEofGotNoˢ);
        }
    }
    {
        var (_, _, err) = buf.ReadRune(); if (err == default!) {
            Ꮡt.Fatal(readRuneAtEofGotNoErrorˢ);
        }
    }
    {
        var err = buf.UnreadRune(); if (err == default!) {
            Ꮡt.Fatal(unreadRuneAfterReadRuneˢ);
        }
    }
    // check not at EOF
    buf.Write(b);
    for (var r = (rune)0; r < NRune; r++) {
        var (r1, size, _) = buf.ReadRune();
        {
            var errΔ1 = buf.UnreadRune(); if (errΔ1 != default!) {
                Ꮡt.Fatalf("UnreadRune(%U) got error %q"u8, r, errΔ1);
            }
        }
        var (r2, nbytes, err) = buf.ReadRune();
        if (r1 != r2 || r1 != r || nbytes != size || err != default!) {
            Ꮡt.Fatalf("ReadRune(%U) after UnreadRune got %U,%d not %U,%d (err=%s)"u8, r, r2, nbytes, r, size, err);
        }
    }
}

public static void TestWriteInvalidRune(ж<testing.T> Ꮡt) {
    // Invalid runes, including negative ones, should be written as
    // utf8.RuneError.
    foreach (var (_, r) in new rune[]{-1, utf8.MaxRune + 1}.slice()) {
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        buf.WriteRune(r);
        check(Ꮡt, fmt.Sprintf("TestWriteInvalidRune (%d)"u8, r), Ꮡbuf, "\uFFFD"u8);
    }
}

public static void TestNext(ж<testing.T> Ꮡt) {
    var b = new byte[]{0, 1, 2, 3, 4}.slice();
    var tmp = new slice<byte>(5);
    for (nint i = 0; i <= 5; i++) {
        for (nint j = i; j <= 5; j++) {
            for (nint k = 0; k <= 6; k++) {
                // 0 <= i <= j <= 5; 0 <= k <= 6
                // Check that if we start with a buffer
                // of length j at offset i and ask for
                // Next(k), we get the right bytes.
                var buf = NewBuffer(b.slice(0, j));
                var (n, _) = buf.Read(tmp.slice(0, i));
                if (n != i) {
                    Ꮡt.Fatalf("Read %d returned %d"u8, i, n);
                }
                var bb = buf.Next(k);
                nint want = k;
                if (want > j - i) {
                    want = j - i;
                }
                if (len(bb) != want) {
                    Ꮡt.Fatalf("in %d,%d: len(Next(%d)) == %d"u8, i, j, k, len(bb));
                }
                foreach (var (l, v) in bb) {
                    if (v != (byte)(l + i)) {
                        Ꮡt.Fatalf("in %d,%d: Next(%d)[%d] = %d, want %d"u8, i, j, k, l, v, l + i);
                    }
                }
            }
        }
    }
}


[GoType("dyn")] partial struct readBytesTestsᴛ1 {
    internal @string buffer;
    internal byte delim;
    internal slice<@string> expected;
    internal error err;
}
internal static slice<readBytesTestsᴛ1> readBytesTests = new readBytesTestsᴛ1[]{
    new(""u8, 0, new @string[]{""u8}.slice(), Δio.EOF),
    new("a\x00"u8, 0, new @string[]{"a\x00"u8}.slice(), default!),
    new("abbbaaaba"u8, (rune)'b', new @string[]{"ab"u8, "b"u8, "b"u8, "aaab"u8}.slice(), default!),
    new("hello\x01world"u8, 1, new @string[]{"hello\x01"u8}.slice(), default!),
    new("foo\nbar"u8, 0, new @string[]{"foo\nbar"u8}.slice(), Δio.EOF),
    new("alpha\nbeta\ngamma\n"u8, (rune)'\n', new @string[]{"alpha\n"u8, "beta\n"u8, "gamma\n"u8}.slice(), default!),
    new("alpha\nbeta\ngamma"u8, (rune)'\n', new @string[]{"alpha\n"u8, "beta\n"u8, "gamma"u8}.slice(), Δio.EOF)
}.slice();

public static void TestReadBytes(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in readBytesTests) {
        var buf = NewBufferString(test.buffer);
        error err = default!;
        foreach (var (_, expected) in test.expected) {
            slice<byte> bytes = default!;
            (bytes, err) = buf.ReadBytes(test.delim);
            if (((sstring)bytes) != expected) {
                Ꮡt.Errorf("expected %q, got %q"u8, expected, bytes);
            }
            if (err != default!) {
                break;
            }
        }
        if (!AreEqual(err, test.err)) {
            Ꮡt.Errorf("expected error %v, got %v"u8, test.err, err);
        }
    }
}

public static void TestReadString(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in readBytesTests) {
        var buf = NewBufferString(test.buffer);
        error err = default!;
        foreach (var (_, expected) in test.expected) {
            @string s = default!;
            (s, err) = buf.ReadString(test.delim);
            if (s != expected) {
                Ꮡt.Errorf("expected %q, got %q"u8, expected, s);
            }
            if (err != default!) {
                break;
            }
        }
        if (!AreEqual(err, test.err)) {
            Ꮡt.Errorf("expected error %v, got %v"u8, test.err, err);
        }
    }
}

public static void BenchmarkReadString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    UntypedInt n = /* 32 << 10 */ 32768;
    var data = new slice<byte>(n);
    data[n - 1] = (rune)'x';
    b.SetBytes((int64)n);
    for (nint i = 0; i < b.N; i++) {
        var buf = NewBuffer(data);
        var (_, err) = buf.ReadString((rune)'x');
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
    }
}

public static void TestGrow(ж<testing.T> Ꮡt) {
    var x = new byte[]{(rune)'x'}.slice();
    var y = new byte[]{(rune)'y'}.slice();
    var tmp = new slice<byte>(72);
    foreach (var (_, growLen) in new nint[]{0, 100, 1000, 10000, 100000}.slice()) {
        foreach (var (_, startLen) in new nint[]{0, 100, 1000, 10000, 100000}.slice()) {
            var xBytes = Repeat(x, startLen);
            var buf = NewBuffer(xBytes);
            // If we read, this affects buf.off, which is good to test.
            var (readBytes, _) = buf.Read(tmp);
            var yBytes = Repeat(y, growLen);
            var bufʗ1 = buf;
            var yBytesʗ1 = yBytes;
            var allocs = testing.AllocsPerRun(100, () => {
                bufʗ1.Grow(growLen);
                bufʗ1.Write(yBytesʗ1);
            });
            // Check no allocation occurs in write, as long as we're single-threaded.
            if (allocs != 0D) {
                Ꮡt.Errorf("allocation occurred during write"u8);
            }
            // Check that buffer has correct data.
            if (!Equal(buf.Bytes().slice(0, startLen - readBytes), xBytes.slice(readBytes))) {
                Ꮡt.Errorf("bad initial data at %d %d"u8, startLen, growLen);
            }
            if (!Equal(buf.Bytes().slice(startLen - readBytes, startLen - readBytes + growLen), yBytes)) {
                Ꮡt.Errorf("bad written data at %d %d"u8, startLen, growLen);
            }
        }
    }
}

public static void TestGrowOverflow(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var err = recover(); if (!AreEqual(err, ErrTooLarge)) {
                    Ꮡt.Errorf("after too-large Grow, recover() = %v; want %v"u8, err, ErrTooLarge);
                }
            }
        }, ref ᒐ);
        var buf = NewBuffer(new slice<byte>(1));
        nint maxInt = /* int(^uint(0) >> 1) */ unchecked((nint)9223372036854775807);
        buf.Grow(maxInt);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Was a bug: used to give EOF reading empty slice at EOF.
public static void TestReadEmptyAtEOF(ж<testing.T> Ꮡt) {
    var b = @new<bytes.Buffer>();
    var Δslice = new slice<byte>(0);
    var (n, err) = b.Read(Δslice);
    if (err != default!) {
        Ꮡt.Errorf("read error: %v"u8, err);
    }
    if (n != 0) {
        Ꮡt.Errorf("wrong count; got %d want 0"u8, n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadByteAtEofGotNoˢ = (@string)"UnreadByte at EOF: got no error"u8;
internal static readonly object readByteAtEofGotNoErrorˢ = (@string)"ReadByte at EOF: got no error"u8;
internal static readonly object unreadByteAfterReadByteˢ = (@string)"UnreadByte after ReadByte at EOF: got no error"u8;
internal static readonly object unreadByteAfterReadNilˢ = (@string)"UnreadByte after Read(nil): got no error"u8;

public static void TestUnreadByte(ж<testing.T> Ꮡt) {
    var b = @new<bytes.Buffer>();
    // check at EOF
    {
        var errΔ1 = b.UnreadByte(); if (errΔ1 == default!) {
            Ꮡt.Fatal(unreadByteAtEofGotNoˢ);
        }
    }
    {
        var (_, errΔ2) = b.ReadByte(); if (errΔ2 == default!) {
            Ꮡt.Fatal(readByteAtEofGotNoErrorˢ);
        }
    }
    {
        var errΔ3 = b.UnreadByte(); if (errΔ3 == default!) {
            Ꮡt.Fatal(unreadByteAfterReadByteˢ);
        }
    }
    // check not at EOF
    b.WriteString("abcdefghijklmnopqrstuvwxyz"u8);
    // after unsuccessful read
    {
        var (n, errΔ4) = b.Read(default!); if (n != 0 || errΔ4 != default!) {
            Ꮡt.Fatalf("Read(nil) = %d,%v; want 0,nil"u8, n, errΔ4);
        }
    }
    {
        var errΔ5 = b.UnreadByte(); if (errΔ5 == default!) {
            Ꮡt.Fatal(unreadByteAfterReadNilˢ);
        }
    }
    // after successful read
    {
        var (_, errΔ6) = b.ReadBytes((rune)'m'); if (errΔ6 != default!) {
            Ꮡt.Fatalf("ReadBytes: %v"u8, errΔ6);
        }
    }
    {
        var errΔ7 = b.UnreadByte(); if (errΔ7 != default!) {
            Ꮡt.Fatalf("UnreadByte: %v"u8, errΔ7);
        }
    }
    var (c, err) = b.ReadByte();
    if (err != default!) {
        Ꮡt.Fatalf("ReadByte: %v"u8, err);
    }
    if (c != (rune)'m') {
        Ꮡt.Errorf("ReadByte = %q; want %q"u8, c, (rune)'m');
    }
}

// Tests that we occasionally compact. Issue 5154.
public static void TestBufferGrowth(ж<testing.T> Ꮡt) {
    bytes.Buffer b = default!;
    var buf = new slice<byte>(1024);
    b.Write(buf[0..1]);
    nint cap0 = default!;
    for (nint i = 0; i < (5 << (int)(10)); i++) {
        b.Write(buf);
        b.Read(buf);
        if (i == 0) {
            cap0 = b.Cap();
        }
    }
    nint cap1 = b.Cap();
    // (*Buffer).grow allows for 2x capacity slop before sliding,
    // so set our error threshold at 3x.
    if (cap1 > cap0 * 3) {
        Ꮡt.Errorf("buffer cap = %d; too big (grew from %d)"u8, cap1, cap0);
    }
}

public static void BenchmarkWriteByte(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    UntypedInt n = /* 4 << 10 */ 4096;
    b.SetBytes(n);
    var buf = NewBuffer(new slice<byte>(n));
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        for (nint iΔ1 = 0; iΔ1 < n; iΔ1++) {
            buf.WriteByte((rune)'x');
        }
    }
}

public static void BenchmarkWriteRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    UntypedInt n = /* 4 << 10 */ 4096;
    const rune r = /* '☺' */ 9786;
    b.SetBytes((int64)((nint)n * utf8.RuneLen(r)));
    var buf = NewBuffer(new slice<byte>(n * utf8.UTFMax));
    for (nint i = 0; i < b.N; i++) {
        buf.Reset();
        for (nint iΔ1 = 0; iΔ1 < n; iΔ1++) {
            buf.WriteRune(r);
        }
    }
}

// From Issue 5154.
public static void BenchmarkBufferNotEmptyWriteRead(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(1024);
    for (nint i = 0; i < b.N; i++) {
        bytes.Buffer bΔ1 = default!;
        bΔ1.Write(buf[0..1]);
        for (nint iΔ1 = 0; iΔ1 < (5 << (int)(10)); iΔ1++) {
            bΔ1.Write(buf);
            bΔ1.Read(buf);
        }
    }
}

// Check that we don't compact too often. From Issue 5154.
public static void BenchmarkBufferFullSmallReads(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(1024);
    for (nint i = 0; i < b.N; i++) {
        bytes.Buffer bΔ1 = default!;
        bΔ1.Write(buf);
        while (bΔ1.Len() + 20 < bΔ1.Cap()) {
            bΔ1.Write(buf[..10]);
        }
        for (nint iΔ1 = 0; iΔ1 < (5 << (int)(10)); iΔ1++) {
            bΔ1.Read(buf[..1]);
            bΔ1.Write(buf[..1]);
        }
    }
}

public static void BenchmarkBufferWriteBlock(ж<testing.B> Ꮡb) {
    var block = new slice<byte>(1024);
    foreach (var (_, n) in new nint[]{(1 << (int)(12)), (1 << (int)(16)), (1 << (int)(20))}.slice()) {
        var blockʗ1 = block;
        Ꮡb.Run(fmt.Sprintf("N%d"u8, n), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                bytes.Buffer bb = default!;
                while (bb.Len() < n) {
                    bb.Write(blockʗ1);
                }
            }
        });
    }
}

public static void BenchmarkBufferAppendNoCopy(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    bytes.Buffer bb = default!;
    bb.Grow((16 << (int)(20)));
    b.SetBytes((int64)bb.Available());
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        bb.Reset();
        var bΔ1 = bb.AvailableBuffer();
        bΔ1 = bΔ1.slice(0, cap(bΔ1)); // use max capacity to simulate a large append operation
        bb.Write(bΔ1); // should be nearly infinitely fast
    }
}

} // end bytes_test_package
