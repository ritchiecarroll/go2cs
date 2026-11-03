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
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using bytes = bytes_package;
using go.math;
using go.unicode;
using static go.bytes_internal_test_package;

partial class bytes_test_package {

public static UntypedInt N => 10000;   // make this bigger for a larger (and slower) test

internal static @string testString;    // test data for write tests

internal static slice<byte> testBytes; // test data; same as testString but as a slice.

partial struct negativeReader {
}

internal static (nint, error) Read(this ref negativeReader r, slice<byte> _) {
    return (-1, default);
}

[GoInit] internal static void init() {
    testBytes = new slice<byte>(N);
    for (nint i = 0; i < N; i++) {
        testBytes[i] = (byte)('a' + (byte)(i % 26));
    }
    testString = (@string)testBytes;
}

// Verify that contents of buf match the string s.
internal static void check(ж<testing.T> t, @string testname, ж<bytes.Buffer> Ꮡbuf, @string s) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    var bytes = buf.Bytes();
    @string str = Ꮡbuf.String();
    if (buf.Len() != len(bytes)) {
        t.Errorf("%s: buf.Len() == %d, len(buf.Bytes()) == %d", testname, buf.Len(), len(bytes));
    }

    if (buf.Len() != len(str)) {
        t.Errorf("%s: buf.Len() == %d, len(buf.String()) == %d", testname, buf.Len(), len(str));
    }

    if (buf.Len() != len(s)) {
        t.Errorf("%s: buf.Len() == %d, len(s) == %d", testname, buf.Len(), len(s));
    }

    if (((sstring)bytes) != s) {
        t.Errorf("%s: string(buf.Bytes()) == %q, s == %q", testname, (@string)bytes, s);
    }
}

// Fill buf through n writes of string fus.
// The initial contents of buf corresponds to the string s;
// the result is the final contents of buf returned as a string.
internal static @string fillString(ж<testing.T> t, @string testname, ж<bytes.Buffer> Ꮡbuf, @string s, nint n, @string fus) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    check(t, testname + " (fill 1)"u8, Ꮡbuf, s);
    for (; n > 0; n--) {
        var (m, err) = buf.WriteString(fus);
        if (m != len(fus)) {
            t.Errorf(testname + " (fill 2): m == %d, expected %d"u8, m, len(fus));
        }
        if (err != nil) {
            t.Errorf(testname + " (fill 3): err should always be nil, found err == %s"u8, err);
        }
        s += fus;
        check(t, testname + " (fill 4)"u8, Ꮡbuf, s);
    }
    return s;
}

// Fill buf through n writes of byte slice fub.
// The initial contents of buf corresponds to the string s;
// the result is the final contents of buf returned as a string.
internal static @string fillBytes(ж<testing.T> t, @string testname, ж<bytes.Buffer> Ꮡbuf, @string s, nint n, slice<byte> fub) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    check(t, testname + " (fill 1)"u8, Ꮡbuf, s);
    for (; n > 0; n--) {
        var (m, err) = buf.Write(fub);
        if (m != len(fub)) {
            t.Errorf(testname + " (fill 2): m == %d, expected %d"u8, m, len(fub));
        }
        if (err != nil) {
            t.Errorf(testname + " (fill 3): err should always be nil, found err == %s"u8, err);
        }
        s += ((@string)fub);
        check(t, testname + " (fill 4)"u8, Ꮡbuf, s);
    }
    return s;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string newBufferˢ => ref newBufferˢᵇ;
private static readonly @string newBufferˢᵇ = "NewBuffer"u8;

public static void TestNewBuffer(ж<testing.T> t) {
    var buf = NewBuffer(testBytes);
    check(t, newBufferˢ, buf, testString);
}

internal static readonly ж<bytes.Buffer> Ꮡbuf = new StaticBox<bytes.Buffer>(default(bytes.Buffer));
internal static ref bytes.Buffer buf => ref ((StaticBox<bytes.Buffer>)Ꮡbuf).ValueSlot;

// Calling NewBuffer and immediately shallow copying the Buffer struct
// should not result in any allocations.
// This can be used to reset the underlying []byte of an existing Buffer.
public static void TestNewBufferShallow(ж<testing.T> t) {
    testenv.SkipIfOptimizationOff(new bytes_test_package.testing_TжTB(t));
    var n = testing.AllocsPerRun(1000, () => {
        buf = NewBuffer(testBytes).Value;
    });
    if (n > 0D) {
        t.Errorf("allocations occurred while shallow copying");
    }
    check(t, newBufferˢ, Ꮡbuf, testString);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string newBufferStringˢ => ref newBufferStringˢᵇ;
private static readonly @string newBufferStringˢᵇ = "NewBufferString"u8;

public static void TestNewBufferString(ж<testing.T> t) {
    var buf = NewBufferString(testString);
    check(t, newBufferStringˢ, buf, testString);
}

// Empty buf through repeated reads into fub.
// The initial contents of buf corresponds to the string s.
internal static void empty(ж<testing.T> t, @string testname, ж<bytes.Buffer> Ꮡbuf, @string s, slice<byte> fub) {
    ref var buf = ref Ꮡbuf.DerefOrNull();

    check(t, testname + " (empty 1)"u8, Ꮡbuf, s);

    for (;;) {
        var (n, err) = buf.Read(fub);
        if (n == 0) {
            break;
        }
        if (err != nil) {
            t.Errorf(testname + " (empty 2): err should always be nil, found err == %s"u8, err);
        }
        s = s.slice(n);
        check(t, testname + " (empty 3)"u8, Ꮡbuf, s);
    }

    check(t, testname + " (empty 4)"u8, Ꮡbuf, ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testBasicOperations1ˢ => ref testBasicOperations1ˢᵇ;
private static readonly @string testBasicOperations1ˢᵇ = "TestBasicOperations (1)"u8;
internal static ref readonly @string testBasicOperations2ˢ => ref testBasicOperations2ˢᵇ;
private static readonly @string testBasicOperations2ˢᵇ = "TestBasicOperations (2)"u8;
internal static ref readonly @string testBasicOperations3ˢ => ref testBasicOperations3ˢᵇ;
private static readonly @string testBasicOperations3ˢᵇ = "TestBasicOperations (3)"u8;
internal static ref readonly @string testBasicOperations4ˢ => ref testBasicOperations4ˢᵇ;
private static readonly @string testBasicOperations4ˢᵇ = "TestBasicOperations (4)"u8;
internal static ref readonly @string testBasicOperations5ˢ => ref testBasicOperations5ˢᵇ;
private static readonly @string testBasicOperations5ˢᵇ = "TestBasicOperations (5)"u8;
internal static ref readonly @string testBasicOperations6ˢ => ref testBasicOperations6ˢᵇ;
private static readonly @string testBasicOperations6ˢᵇ = "TestBasicOperations (6)"u8;
internal static ref readonly @string testBasicOperations7ˢ => ref testBasicOperations7ˢᵇ;
private static readonly @string testBasicOperations7ˢᵇ = "TestBasicOperations (7)"u8;
internal static ref readonly @string testBasicOperations8ˢ => ref testBasicOperations8ˢᵇ;
private static readonly @string testBasicOperations8ˢᵇ = "TestBasicOperations (8)"u8;
internal static ref readonly @string testBasicOperations9ˢ => ref testBasicOperations9ˢᵇ;
private static readonly @string testBasicOperations9ˢᵇ = "TestBasicOperations (9)"u8;
internal static ref readonly @string testBasicOperations10ˢ => ref testBasicOperations10ˢᵇ;
private static readonly @string testBasicOperations10ˢᵇ = "TestBasicOperations (10)"u8;

public static void TestBasicOperations(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);

    for (nint i = 0; i < 5; i++) {
        check(t, testBasicOperations1ˢ, Ꮡbuf, ""u8);

        buf.Reset();
        check(t, testBasicOperations2ˢ, Ꮡbuf, ""u8);

        buf.Truncate(0);
        check(t, testBasicOperations3ˢ, Ꮡbuf, ""u8);

        var (n, err) = buf.Write(testBytes[0..1]);
        {
            nint want = 1; if (err != nil || n != want) {
                t.Errorf("Write: got (%d, %v), want (%d, %v)", n, err, want, (any)(default));
            }
        }
        check(t, testBasicOperations4ˢ, Ꮡbuf, "a");

        buf.WriteByte(testString[1]);
        check(t, testBasicOperations5ˢ, Ꮡbuf, "ab");

        (n, err) = buf.Write(testBytes[2..26]);
        {
            nint want = 24; if (err != nil || n != want) {
                t.Errorf("Write: got (%d, %v), want (%d, %v)", n, err, want, (any)(default));
            }
        }
        check(t, testBasicOperations6ˢ, Ꮡbuf, testString[0..26]);

        buf.Truncate(26);
        check(t, testBasicOperations7ˢ, Ꮡbuf, testString[0..26]);

        buf.Truncate(20);
        check(t, testBasicOperations8ˢ, Ꮡbuf, testString[0..20]);

        empty(t, testBasicOperations9ˢ, Ꮡbuf, testString[0..20], new slice<byte>(5));
        empty(t, testBasicOperations10ˢ, Ꮡbuf, ""u8, new slice<byte>(100));

        buf.WriteByte(testString[1]);
        (var c, err) = buf.ReadByte();
        {
            var want = testString[1]; if (err != nil || c != want) {
                t.Errorf("ReadByte: got (%q, %v), want (%q, %v)", c, err, want, (any)(default));
            }
        }
        (c, err) = buf.ReadByte();
        if (!AreEqual(err, Δio.EOF)) {
            t.Errorf("ReadByte: got (%q, %v), want (%q, %v)", c, err, (byte)0, Δio.EOF);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testLargeWrites1ˢ => ref testLargeWrites1ˢᵇ;
private static readonly @string testLargeWrites1ˢᵇ = "TestLargeWrites (1)"u8;
internal static ref readonly @string testLargeStringWrites2ˢ => ref testLargeStringWrites2ˢᵇ;
private static readonly @string testLargeStringWrites2ˢᵇ = "TestLargeStringWrites (2)"u8;
internal static ref readonly @string testLargeStringWrites3ˢ => ref testLargeStringWrites3ˢᵇ;
private static readonly @string testLargeStringWrites3ˢᵇ = "TestLargeStringWrites (3)"u8;

public static void TestLargeStringWrites(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    nint limit = 30;
    if (testing.Short()) {
        limit = 9;
    }
    for (nint i = 3; i < limit; i += 3) {
        @string s = fillString(t, testLargeWrites1ˢ, Ꮡbuf, ""u8, 5, testString);
        empty(t, testLargeStringWrites2ˢ, Ꮡbuf, s, new slice<byte>(len(testString) / i));
    }
    check(t, testLargeStringWrites3ˢ, Ꮡbuf, ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testLargeByteWrites2ˢ => ref testLargeByteWrites2ˢᵇ;
private static readonly @string testLargeByteWrites2ˢᵇ = "TestLargeByteWrites (2)"u8;
internal static ref readonly @string testLargeByteWrites3ˢ => ref testLargeByteWrites3ˢᵇ;
private static readonly @string testLargeByteWrites3ˢᵇ = "TestLargeByteWrites (3)"u8;

public static void TestLargeByteWrites(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    nint limit = 30;
    if (testing.Short()) {
        limit = 9;
    }
    for (nint i = 3; i < limit; i += 3) {
        @string s = fillBytes(t, testLargeWrites1ˢ, Ꮡbuf, ""u8, 5, testBytes);
        empty(t, testLargeByteWrites2ˢ, Ꮡbuf, s, new slice<byte>(len(testString) / i));
    }
    check(t, testLargeByteWrites3ˢ, Ꮡbuf, ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testLargeReads1ˢ => ref testLargeReads1ˢᵇ;
private static readonly @string testLargeReads1ˢᵇ = "TestLargeReads (1)"u8;
internal static ref readonly @string testLargeReads2ˢ => ref testLargeReads2ˢᵇ;
private static readonly @string testLargeReads2ˢᵇ = "TestLargeReads (2)"u8;
internal static ref readonly @string testLargeStringReads3ˢ => ref testLargeStringReads3ˢᵇ;
private static readonly @string testLargeStringReads3ˢᵇ = "TestLargeStringReads (3)"u8;

public static void TestLargeStringReads(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 3; i < 30; i += 3) {
        @string s = fillString(t, testLargeReads1ˢ, Ꮡbuf, ""u8, 5, testString.slice(0, len(testString) / i));
        empty(t, testLargeReads2ˢ, Ꮡbuf, s, new slice<byte>(len(testString)));
    }
    check(t, testLargeStringReads3ˢ, Ꮡbuf, ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testLargeByteReads3ˢ => ref testLargeByteReads3ˢᵇ;
private static readonly @string testLargeByteReads3ˢᵇ = "TestLargeByteReads (3)"u8;

public static void TestLargeByteReads(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 3; i < 30; i += 3) {
        @string s = fillBytes(t, testLargeReads1ˢ, Ꮡbuf, ""u8, 5, testBytes.slice(0, len(testBytes) / i));
        empty(t, testLargeReads2ˢ, Ꮡbuf, s, new slice<byte>(len(testString)));
    }
    check(t, testLargeByteReads3ˢ, Ꮡbuf, ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testMixedReadsAndWrites1ˢ => ref testMixedReadsAndWrites1ˢᵇ;
private static readonly @string testMixedReadsAndWrites1ˢᵇ = "TestMixedReadsAndWrites (1)"u8;
internal static ref readonly @string testMixedReadsAndWrites2ˢ => ref testMixedReadsAndWrites2ˢᵇ;
private static readonly @string testMixedReadsAndWrites2ˢᵇ = "TestMixedReadsAndWrites (2)"u8;

public static void TestMixedReadsAndWrites(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    @string s = ""u8;
    for (nint i = 0; i < 50; i++) {
        nint wlen = rand.Intn(len(testString));
        if (i % 2 == 0) {
            s = fillString(t, testMixedReadsAndWrites1ˢ, Ꮡbuf, s, 1, testString.slice(0, wlen));
        } else {
            s = fillBytes(t, testMixedReadsAndWrites1ˢ, Ꮡbuf, s, 1, testBytes.slice(0, wlen));
        }

        nint rlen = rand.Intn(len(testString));
        var fub = new slice<byte>(rlen);
        var (n, _) = buf.Read(fub);
        s = s.slice(n);
    }
    empty(t, testMixedReadsAndWrites2ˢ, Ꮡbuf, s, new slice<byte>(buf.Len()));
}

public static void TestCapWithPreallocatedSlice(ж<testing.T> t) {
    var buf = NewBuffer(new slice<byte>(10));
    nint n = buf.Cap();
    if (n != 10) {
        t.Errorf("expected 10, got %d", n);
    }
}

public static void TestCapWithSliceAndWrittenData(ж<testing.T> t) {
    var buf = NewBuffer(new slice<byte>(0, 10));
    buf.Write(slice<byte>("test"u8));
    nint n = buf.Cap();
    if (n != 10) {
        t.Errorf("expected 10, got %d", n);
    }
}

public static partial void TestNil(ж<testing.T> t) {
    ж<bytes.Buffer> b = default;
    if (b.String() != "<nil>"u8) {
        t.Errorf("expected <nil>; got %q", b.String());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testReadFrom1ˢ => ref testReadFrom1ˢᵇ;
private static readonly @string testReadFrom1ˢᵇ = "TestReadFrom (1)"u8;
internal static ref readonly @string testReadFrom2ˢ => ref testReadFrom2ˢᵇ;
private static readonly @string testReadFrom2ˢᵇ = "TestReadFrom (2)"u8;

public static void TestReadFrom(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 3; i < 30; i += 3) {
        @string s = fillBytes(t, testReadFrom1ˢ, Ꮡbuf, ""u8, 5, testBytes.slice(0, len(testBytes) / i));
        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        b.ReadFrom(new bytes_test_package.bytes_BufferжReader(Ꮡbuf));
        empty(t, testReadFrom2ˢ, Ꮡb, s, new slice<byte>(len(testString)));
    }
}

partial struct panicReader {
    internal bool panic;
}

internal static (nint, error) Read(this panicReader r, slice<byte> p) {
    if (r.panic) {
        throw panic("oops");
    }
    return (0, Δio.EOF);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testReadFromPanicReader1ˢ => ref testReadFromPanicReader1ˢᵇ;
private static readonly @string testReadFromPanicReader1ˢᵇ = "TestReadFromPanicReader (1)"u8;
internal static ref readonly @string testReadFromPanicReader2ˢ => ref testReadFromPanicReader2ˢᵇ;
private static readonly @string testReadFromPanicReader2ˢᵇ = "TestReadFromPanicReader (2)"u8;

// Make sure that an empty Buffer remains empty when
// it is "grown" before a Read that panics
public static void TestReadFromPanicReader(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // First verify non-panic behaviour
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        var (i, err) = buf.ReadFrom(new panicReader());
        if (err != nil) {
            t.Fatal(err);
        }
        if (i != 0) {
            t.Fatalf("unexpected return from bytes.ReadFrom (1): got: %d, want %d", i, (nint)0);
        }
        check(t, testReadFromPanicReader1ˢ, Ꮡbuf, ""u8);

        // Confirm that when Reader panics, the empty buffer remains empty
        ref var buf2 = ref heap(new bytes.Buffer(), out var Ꮡbuf2);
        defer(() => {
            recover();
            check(t, testReadFromPanicReader2ˢ, Ꮡbuf2, ""u8);
        }, ref ᒐ);
        buf2.ReadFrom(new panicReader(panic: true));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bytesBufferReadFromDidnTˢ = (@string)"bytes.Buffer.ReadFrom didn't panic"u8;
internal static ref readonly @string bytesBufferReaderˢ => ref bytesBufferReaderˢᵇ;
private static readonly @string bytesBufferReaderˢᵇ = "bytes.Buffer: reader returned negative count from Read"u8;

public static void TestReadFromNegativeReader(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        bytes.Buffer b = default;
        defer(() => {
            var switchᴛ1 = recover();
            switch (switchᴛ1.type()) {
            case null: {
                t.Fatal(bytesBufferReadFromDidnTˢ);
                break;
            }
            case {} Δerr when Δerr._<error>(out var err): {
                // this is the error string of errNegativeRead
                @string wantError = bytesBufferReaderˢ;
                if (err.Error() != wantError) {
                    t.Fatalf("recovered panic: got %v, want %v", err.Error(), wantError);
                }
                break;
            }
            default: {
                var err = switchᴛ1;
                t.Fatalf("unexpected panic value: %#v", err);
                break;
            }}
        }, ref ᒐ);

        b.ReadFrom(new bytes_test_package.negativeReaderжReader(@new<negativeReader>()));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testWriteTo1ˢ => ref testWriteTo1ˢᵇ;
private static readonly @string testWriteTo1ˢᵇ = "TestWriteTo (1)"u8;
internal static ref readonly @string testWriteTo2ˢ => ref testWriteTo2ˢᵇ;
private static readonly @string testWriteTo2ˢᵇ = "TestWriteTo (2)"u8;

public static void TestWriteTo(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    for (nint i = 3; i < 30; i += 3) {
        @string s = fillBytes(t, testWriteTo1ˢ, Ꮡbuf, ""u8, 5, testBytes.slice(0, len(testBytes) / i));
        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        buf.WriteTo(new bytes_test_package.bytes_BufferжWriter(Ꮡb));
        empty(t, testWriteTo2ˢ, Ꮡb, s, new slice<byte>(len(testString)));
    }
}

public static void TestWriteAppend(ж<testing.T> t) {
    ref var got = ref heap(new bytes.Buffer(), out var Ꮡgot);
    slice<byte> want = default;
    for (nint i = 0; i < 1000; i++) {
        var b = got.AvailableBuffer();
        b = strconv.AppendInt(b, (int64)i, 10);
        want = strconv.AppendInt(want, (int64)i, 10);
        got.Write(b);
    }
    if (!Equal(got.Bytes(), want)) {
        t.Fatalf("Bytes() = %q, want %q", Ꮡgot, want);
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
        t.Errorf("allocations occurred while appending");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadRuneAtEofGotNoˢ = (@string)"UnreadRune at EOF: got no error"u8;
internal static readonly object readRuneAtEofGotNoErrorˢ = (@string)"ReadRune at EOF: got no error"u8;
internal static readonly object unreadRuneAfterReadRuneˢ = (@string)"UnreadRune after ReadRune at EOF: got no error"u8;

public static void TestRuneIO(ж<testing.T> t) {
    UntypedInt NRune = 1000;
    // Built a test slice while we write the data
    var b = new slice<byte>(utf8.UTFMax * NRune);
    bytes.Buffer buf = default;
    nint n = 0;
    for (var r = (rune)0; r < NRune; r++) {
        nint size = utf8.EncodeRune(b.slice(n), r);
        var (nbytes, err) = buf.WriteRune(r);
        if (err != nil) {
            t.Fatalf("WriteRune(%U) error: %s", r, err);
        }
        if (nbytes != size) {
            t.Fatalf("WriteRune(%U) expected %d, got %d", r, size, nbytes);
        }
        n += size;
    }
    b = b.slice(0, n);

    // Check the resulting bytes
    if (!Equal(buf.Bytes(), b)) {
        t.Fatalf("incorrect result from WriteRune: %q not %q", buf.Bytes(), b);
    }

    var p = new slice<byte>(utf8.UTFMax);
    // Read it back with ReadRune
    for (var r = (rune)0; r < NRune; r++) {
        nint size = utf8.EncodeRune(p, r);
        var (nr, nbytes, err) = buf.ReadRune();
        if (nr != r || nbytes != size || err != nil) {
            t.Fatalf("ReadRune(%U) got %U,%d not %U,%d (err=%s)", r, nr, nbytes, r, size, err);
        }
    }

    // Check that UnreadRune works
    buf.Reset();

    // check at EOF
    {
        var err = buf.UnreadRune(); if (err == nil) {
            t.Fatal(unreadRuneAtEofGotNoˢ);
        }
    }
    {
        var (_, _, err) = buf.ReadRune(); if (err == nil) {
            t.Fatal(readRuneAtEofGotNoErrorˢ);
        }
    }
    {
        var err = buf.UnreadRune(); if (err == nil) {
            t.Fatal(unreadRuneAfterReadRuneˢ);
        }
    }

    // check not at EOF
    buf.Write(b);
    for (var r = (rune)0; r < NRune; r++) {
        var (r1, size, _) = buf.ReadRune();
        {
            var errΔ1 = buf.UnreadRune(); if (errΔ1 != nil) {
                t.Fatalf("UnreadRune(%U) got error %q", r, errΔ1);
            }
        }
        var (r2, nbytes, err) = buf.ReadRune();
        if (r1 != r2 || r1 != r || nbytes != size || err != nil) {
            t.Fatalf("ReadRune(%U) after UnreadRune got %U,%d not %U,%d (err=%s)", r, r2, nbytes, r, size, err);
        }
    }
}

public static void TestWriteInvalidRune(ж<testing.T> t) {
    // Invalid runes, including negative ones, should be written as
    // utf8.RuneError.
    foreach (var (_, r) in slice<rune>([-1, utf8.MaxRune + 1])) {
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        buf.WriteRune(r);
        check(t, fmt.Sprintf("TestWriteInvalidRune (%d)"u8, r), Ꮡbuf, "\uFFFD");
    }
}

public static void TestNext(ж<testing.T> t) {
    var b = slice<byte>([0, 1, 2, 3, 4]);
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
                    t.Fatalf("Read %d returned %d", i, n);
                }
                var bb = buf.Next(k);
                nint want = k;
                if (want > j - i) {
                    want = j - i;
                }
                if (len(bb) != want) {
                    t.Fatalf("in %d,%d: len(Next(%d)) == %d", i, j, k, len(bb));
                }
                foreach (var (l, v) in bb) {
                    if (v != (byte)(l + i)) {
                        t.Fatalf("in %d,%d: Next(%d)[%d] = %d, want %d", i, j, k, l, v, l + i);
                    }
                }
            }
        }
    }
}

partial struct readBytesTestsᴛ1 /*dyn*/ {
    internal @string buffer;
    internal byte delim;
    internal slice<@string> expected;
    internal error err;
}
internal static slice<readBytesTestsᴛ1> readBytesTests = slice<readBytesTestsᴛ1>([
    new("", 0, slice<@string>([""]), Δio.EOF),
    new("a\x00", 0, slice<@string>(["a\x00"]), default),
    new("abbbaaaba", (byte)'b', slice<@string>(["ab", "b", "b", "aaab"]), default),
    new("hello\x01world", 1, slice<@string>(["hello\x01"]), default),
    new("foo\nbar", 0, slice<@string>(["foo\nbar"]), Δio.EOF),
    new("alpha\nbeta\ngamma\n", (byte)'\n', slice<@string>(["alpha\n", "beta\n", "gamma\n"]), default),
    new("alpha\nbeta\ngamma", (byte)'\n', slice<@string>(["alpha\n", "beta\n", "gamma"]), Δio.EOF)
]);

public static void TestReadBytes(ж<testing.T> t) {
    foreach (var (_, test) in readBytesTests) {
        var buf = NewBufferString(test.buffer);
        error err = default;
        foreach (var (_, expected) in test.expected) {
            slice<byte> bytes = default;
            (bytes, err) = buf.ReadBytes(test.delim);
            if (((sstring)bytes) != expected) {
                t.Errorf("expected %q, got %q", expected, bytes);
            }
            if (err != nil) {
                break;
            }
        }
        if (!AreEqual(err, test.err)) {
            t.Errorf("expected error %v, got %v", test.err, err);
        }
    }
}

public static void TestReadString(ж<testing.T> t) {
    foreach (var (_, test) in readBytesTests) {
        var buf = NewBufferString(test.buffer);
        error err = default;
        foreach (var (_, expected) in test.expected) {
            @string s = default;
            (s, err) = buf.ReadString(test.delim);
            if (s != expected) {
                t.Errorf("expected %q, got %q", expected, s);
            }
            if (err != nil) {
                break;
            }
        }
        if (!AreEqual(err, test.err)) {
            t.Errorf("expected error %v, got %v", test.err, err);
        }
    }
}

partial struct peekTestsᴛ1 /*dyn*/ {
    internal @string buffer;
    internal nint skip;
    internal nint n;
    internal @string expected;
    internal error err;
}
internal static slice<peekTestsᴛ1> peekTests = slice<peekTestsᴛ1>([
    new("", 0, 0, "", default),
    new("aaa", 0, 3, "aaa", default),
    new("foobar", 0, 2, "fo", default),
    new("a", 0, 2, "a", Δio.EOF),
    new("helloworld", 4, 3, "owo", default),
    new("helloworld", 5, 5, "world", default),
    new("helloworld", 5, 6, "world", Δio.EOF),
    new("helloworld", 10, 1, "", Δio.EOF)
]);

public static void TestPeek(ж<testing.T> t) {
    foreach (var (_, test) in peekTests) {
        var buf = NewBufferString(test.buffer);
        buf.Next(test.skip);
        var (bytes, err) = buf.Peek(test.n);
        if (((sstring)bytes) != test.expected) {
            t.Errorf("expected %q, got %q", test.expected, bytes);
        }
        if (!AreEqual(err, test.err)) {
            t.Errorf("expected error %v, got %v", test.err, err);
        }
        if (buf.Len() != len(test.buffer) - test.skip) {
            t.Errorf("bad length after peek: %d, want %d", buf.Len(), len(test.buffer) - test.skip);
        }
    }
}

public static void BenchmarkReadString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    UntypedInt n = /* 32 << 10 */ 32768;

    var data = new slice<byte>(n);
    data[n - 1] = (byte)'x';
    b.SetBytes((int64)n);
    for (nint i = 0; i < b.N; i++) {
        var buf = NewBuffer(data);
        var (_, err) = buf.ReadString((byte)'x');
        if (err != nil) {
            Ꮡb.Fatal(err);
        }
    }
}

public static void TestGrow(ж<testing.T> t) {
    var x = slice<byte>([(byte)'x']);
    var y = slice<byte>([(byte)'y']);
    var tmp = new slice<byte>(72);
    foreach (var (_, growLen) in slice<nint>([0, 100, 1000, 10000, 100000])) {
        foreach (var (_, startLen) in slice<nint>([0, 100, 1000, 10000, 100000])) {
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
                t.Errorf("allocation occurred during write");
            }
            // Check that buffer has correct data.
            if (!Equal(buf.Bytes().slice(0, startLen - readBytes), xBytes.slice(readBytes))) {
                t.Errorf("bad initial data at %d %d", startLen, growLen);
            }
            if (!Equal(buf.Bytes().slice(startLen - readBytes, startLen - readBytes + growLen), yBytes)) {
                t.Errorf("bad written data at %d %d", startLen, growLen);
            }
        }
    }
}

public static void TestGrowOverflow(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var err = recover(); if (!AreEqual(err, ErrTooLarge)) {
                    t.Errorf("after too-large Grow, recover() = %v; want %v", err, ErrTooLarge);
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
public static void TestReadEmptyAtEOF(ж<testing.T> t) {
    var b = @new<bytes.Buffer>();
    var Δslice = new slice<byte>(0);
    var (n, err) = b.Read(Δslice);
    if (err != nil) {
        t.Errorf("read error: %v", err);
    }
    if (n != 0) {
        t.Errorf("wrong count; got %d want 0", n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadByteAtEofGotNoˢ = (@string)"UnreadByte at EOF: got no error"u8;
internal static readonly object readByteAtEofGotNoErrorˢ = (@string)"ReadByte at EOF: got no error"u8;
internal static readonly object unreadByteAfterReadByteˢ = (@string)"UnreadByte after ReadByte at EOF: got no error"u8;
internal static readonly object unreadByteAfterReadNilˢ = (@string)"UnreadByte after Read(nil): got no error"u8;

public static void TestUnreadByte(ж<testing.T> t) {
    var b = @new<bytes.Buffer>();

    // check at EOF
    {
        var errΔ1 = b.UnreadByte(); if (errΔ1 == nil) {
            t.Fatal(unreadByteAtEofGotNoˢ);
        }
    }
    {
        var (_, errΔ2) = b.ReadByte(); if (errΔ2 == nil) {
            t.Fatal(readByteAtEofGotNoErrorˢ);
        }
    }
    {
        var errΔ3 = b.UnreadByte(); if (errΔ3 == nil) {
            t.Fatal(unreadByteAfterReadByteˢ);
        }
    }

    // check not at EOF
    b.WriteString("abcdefghijklmnopqrstuvwxyz");

    // after unsuccessful read
    {
        var (n, errΔ4) = b.Read(default); if (n != 0 || errΔ4 != nil) {
            t.Fatalf("Read(nil) = %d,%v; want 0,nil", n, errΔ4);
        }
    }
    {
        var errΔ5 = b.UnreadByte(); if (errΔ5 == nil) {
            t.Fatal(unreadByteAfterReadNilˢ);
        }
    }

    // after successful read
    {
        var (_, errΔ6) = b.ReadBytes((byte)'m'); if (errΔ6 != nil) {
            t.Fatalf("ReadBytes: %v", errΔ6);
        }
    }
    {
        var errΔ7 = b.UnreadByte(); if (errΔ7 != nil) {
            t.Fatalf("UnreadByte: %v", errΔ7);
        }
    }
    var (c, err) = b.ReadByte();
    if (err != nil) {
        t.Fatalf("ReadByte: %v", err);
    }
    if (c != 'm') {
        t.Errorf("ReadByte = %q; want %q", c, (rune)'m');
    }
}

// Tests that we occasionally compact. Issue 5154.
public static void TestBufferGrowth(ж<testing.T> t) {
    bytes.Buffer b = default;
    var buf = new slice<byte>(1024);
    b.Write(buf[0..1]);
    nint cap0 = default;
    for (nint i = 0; i < 5 << 10; i++) {
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
        t.Errorf("buffer cap = %d; too big (grew from %d)", cap1, cap0);
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
            buf.WriteByte((byte)'x');
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
        bytes.Buffer bΔ1 = default;
        bΔ1.Write(buf[0..1]);
        for (nint iΔ1 = 0; iΔ1 < 5 << 10; iΔ1++) {
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
        bytes.Buffer bΔ1 = default;
        bΔ1.Write(buf);
        while (bΔ1.Len() + 20 < bΔ1.Cap()) {
            bΔ1.Write(buf[..10]);
        }
        for (nint iΔ1 = 0; iΔ1 < 5 << 10; iΔ1++) {
            bΔ1.Read(buf[..1]);
            bΔ1.Write(buf[..1]);
        }
    }
}

public static void BenchmarkBufferWriteBlock(ж<testing.B> b) {
    var block = new slice<byte>(1024);
    foreach (var (_, n) in slice<nint>([1 << 12, 1 << 16, 1 << 20])) {
        var blockʗ1 = block;
        b.Run(fmt.Sprintf("N%d"u8, n), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                bytes.Buffer bb = default;
                while (bb.Len() < n) {
                    bb.Write(blockʗ1);
                }
            }
        });
    }
}

public static void BenchmarkBufferAppendNoCopy(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    bytes.Buffer bb = default;
    bb.Grow(16 << 20);
    b.SetBytes((int64)bb.Available());
    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        bb.Reset();
        var bΔ1 = bb.AvailableBuffer();
        bΔ1 = bΔ1.slice(0, cap(bΔ1)); // use max capacity to simulate a large append operation
        bb.Write(bΔ1);                // should be nearly infinitely fast
    }
}

} // end bytes_test_package
