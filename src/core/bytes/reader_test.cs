// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static bytes_package;
using fmt = fmt_package;
using Δio = io_package;
using Δsync = sync_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using bytes = bytes_package;
using static go.bytes_internal_test_package;

partial class bytes_test_package {

internal partial struct TestReader_tests /*dyn*/ {
    internal int64 off;
    internal nint seek;
    internal nint n;
    internal @string want;
    internal int64 wantpos;
    internal error readerr;
    internal @string seekerr;
}

public static void TestReader(ж<testing.T> t) {
    var r = NewReader(slice<byte>("0123456789"u8));
    var tests = slice<TestReader_tests>([
        new(seek: Δio.SeekStart, off: 0, n: 20, want: "0123456789"),
        new(seek: Δio.SeekStart, off: 1, n: 1, want: "1"),
        new(seek: Δio.SeekCurrent, off: 1, wantpos: 3, n: 2, want: "34"),
        new(seek: Δio.SeekStart, off: -1, seekerr: "bytes.Reader.Seek: negative position"),
        new(seek: Δio.SeekStart, off: 8589934592L, wantpos: 8589934592L, readerr: Δio.EOF),
        new(seek: Δio.SeekCurrent, off: 1, wantpos: 8589934593L, readerr: Δio.EOF),
        new(seek: Δio.SeekStart, n: 5, want: "01234"),
        new(seek: Δio.SeekCurrent, n: 5, want: "56789"),
        new(seek: Δio.SeekEnd, off: -1, n: 1, wantpos: 9, want: "9")
    ]);

    foreach (var (i, tt) in tests) {
        var (pos, err) = r.Seek(tt.off, tt.seek);
        if (err == nil && tt.seekerr != ""u8) {
            t.Errorf("%d. want seek error %q", i, tt.seekerr);
            continue;
        }
        if (err != nil && err.Error() != tt.seekerr) {
            t.Errorf("%d. seek error = %q; want %q", i, err.Error(), tt.seekerr);
            continue;
        }
        if (tt.wantpos != 0 && tt.wantpos != pos) {
            t.Errorf("%d. pos = %d, want %d", i, pos, tt.wantpos);
        }
        var buf = new slice<byte>(tt.n);
        (var n, err) = r.Read(buf);
        if (!AreEqual(err, tt.readerr)) {
            t.Errorf("%d. read = %v; want %v", i, err, tt.readerr);
            continue;
        }
        @string got = (@string)(buf.slice(0, n));
        if (got != tt.want) {
            t.Errorf("%d. got %q; want %q", i, got, tt.want);
        }
    }
}

public static void TestReadAfterBigSeek(ж<testing.T> t) {
    var r = NewReader(slice<byte>("0123456789"u8));
    {
        var (_, err) = r.Seek(2147483653L, Δio.SeekStart); if (err != nil) {
            t.Fatal(err);
        }
    }
    {
        var (n, err) = r.Read(new slice<byte>(10)); if (n != 0 || !AreEqual(err, Δio.EOF)) {
            t.Errorf("Read = %d, %v; want 0, EOF", n, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object bytesReaderReadAtˢ = (@string)"bytes.Reader.ReadAt: negative offset"u8;

internal partial struct TestReaderAt_tests /*dyn*/ {
    internal int64 off;
    internal nint n;
    internal @string want;
    internal any wanterr;
}

public static void TestReaderAt(ж<testing.T> t) {
    var r = NewReader(slice<byte>("0123456789"u8));
    var tests = slice<TestReaderAt_tests>([
        new(0, 10, "0123456789", default),
        new(1, 10, "123456789", Δio.EOF),
        new(1, 9, "123456789", default),
        new(11, 10, "", Δio.EOF),
        new(0, 0, "", default),
        new(-1, 0, "", bytesReaderReadAtˢ)
    ]);
    foreach (var (i, tt) in tests) {
        var b = new slice<byte>(tt.n);
        var (rn, err) = r.ReadAt(b, tt.off);
        @string got = (@string)(b.slice(0, rn));
        if (got != tt.want) {
            t.Errorf("%d. got %q; want %q", i, got, tt.want);
        }
        if (fmt.Sprintf("%v"u8, err) != fmt.Sprintf("%v"u8, tt.wanterr)) {
            t.Errorf("%d. got error = %v; want %v", i, err, tt.wanterr);
        }
    }
}

public static partial void TestReaderAtConcurrent(ж<testing.T> t) {
    // Test for the race detector, to verify ReadAt doesn't mutate
    // any state.
    var r = NewReader(slice<byte>("0123456789"u8));
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 5; i++) {
        Ꮡwg.Add(1);
        var rʗ1 = r;
        goǃ((nint iΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                ref var buf = ref heap(new array<byte>(1), out var Ꮡbuf);
                rʗ1.ReadAt(buf[..], (int64)iΔ1);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, i, GoStack.Bounded);
    }
    Ꮡwg.Wait();
}

public static partial void TestEmptyReaderConcurrent(ж<testing.T> t) {
    // Test for the race detector, to verify a Read that doesn't yield any bytes
    // is okay to use from multiple goroutines. This was our historic behavior.
    // See golang.org/issue/7856
    var r = NewReader(slice<byte>([]));
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 5; i++) {
        Ꮡwg.Add(2);
        var rʗ1 = r;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                ref var buf = ref heap(new array<byte>(1), out var Ꮡbuf);
                rʗ1.Read(buf[..]);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, GoStack.Bounded);
        var rʗ2 = r;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                rʗ2.Read(default);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, GoStack.Bounded);
    }
    Ꮡwg.Wait();
}

public static void TestReaderWriteTo(ж<testing.T> t) {
    for (nint i = 0; i < 30; i += 3) {
        nint l = default;
        if (i > 0) {
            l = len(testString) / i;
        }
        @string s = testString.slice(0, l);
        var r = NewReader(testBytes.slice(0, l));
        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        var (n, err) = r.WriteTo(new bytes_test_package.bytes_BufferжWriter(Ꮡb));
        {
            var expect = (int64)len(s); if (n != expect) {
                t.Errorf("got %v; want %v", n, expect);
            }
        }
        if (err != nil) {
            t.Errorf("for length %d: got error = %v; want nil", l, err);
        }
        if (Ꮡb.String() != s) {
            t.Errorf("got string %q; want %q", Ꮡb.String(), s);
        }
        if (r.Len() != 0) {
            t.Errorf("reader contains %v bytes; want 0", r.Len());
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string dataᶜ => ref dataᶜᵇ;
private static readonly @string dataᶜᵇ = "hello world"u8;

public static void TestReaderLen(ж<testing.T> t) {
    @string data = dataᶜ;
    var r = NewReader(slice<byte>(data));
    {
        nint got = r.Len();
        nint want = 11; if (got != want) {
            t.Errorf("r.Len(): got %d, want %d", got, want);
        }
    }
    {
        var (n, err) = r.Read(new slice<byte>(10)); if (err != nil || n != 10) {
            t.Errorf("Read failed: read %d %v", n, err);
        }
    }
    {
        nint got = r.Len();
        nint want = 1; if (got != want) {
            t.Errorf("r.Len(): got %d, want %d", got, want);
        }
    }
    {
        var (n, err) = r.Read(new slice<byte>(1)); if (err != nil || n != 1) {
            t.Errorf("Read failed: read %d %v; want 1, nil", n, err);
        }
    }
    {
        nint got = r.Len();
        nint want = 0; if (got != want) {
            t.Errorf("r.Len(): got %d, want %d", got, want);
        }
    }
}

partial struct UnreadRuneErrorTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal Action<ж<bytes.Reader>> f;
}
public static readonly ж<slice<UnreadRuneErrorTestsᴛ1>> ᏑUnreadRuneErrorTests = new StaticBox<slice<UnreadRuneErrorTestsᴛ1>>(slice<UnreadRuneErrorTestsᴛ1>([
    new("Read", (ж<bytes.Reader> r) => {
        r.Read(slice<byte>([0]));
    }),
    new("ReadByte", (ж<bytes.Reader> r) => {
        r.ReadByte();
    }),
    new("UnreadRune", (ж<bytes.Reader> r) => {
        r.UnreadRune();
    }),
    new("Seek", (ж<bytes.Reader> r) => {
        r.Seek(0, Δio.SeekCurrent);
    }),
    new("WriteTo", (ж<bytes.Reader> r) => {
        r.WriteTo(new bytes_test_package.bytes_BufferжWriter(Ꮡ(new Buffer())));
    })
]));
public static ref slice<UnreadRuneErrorTestsᴛ1> UnreadRuneErrorTests => ref ((StaticBox<slice<UnreadRuneErrorTestsᴛ1>>)ᏑUnreadRuneErrorTests).ValueSlot;

public static void TestUnreadRuneError(ж<testing.T> t) {
    foreach (var (_, tt) in UnreadRuneErrorTests) {
        var reader = NewReader(slice<byte>("0123456789"u8));
        {
            var (_, _, errΔ1) = reader.ReadRune(); if (errΔ1 != nil) {
                // should not happen
                t.Fatal(errΔ1);
            }
        }
        tt.f(reader);
        var err = reader.UnreadRune();
        if (err == nil) {
            t.Errorf("Unreading after %s: expected error", tt.name);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadByteExpectedErrorˢ = (@string)"UnreadByte: expected error, got nil"u8;

public static void TestReaderDoubleUnreadRune(ж<testing.T> t) {
    var buf = NewBuffer(slice<byte>("groucho"u8));
    {
        var (_, _, err) = buf.ReadRune(); if (err != nil) {
            // should not happen
            t.Fatal(err);
        }
    }
    {
        var err = buf.UnreadByte(); if (err != nil) {
            // should not happen
            t.Fatal(err);
        }
    }
    {
        var err = buf.UnreadByte(); if (err == nil) {
            t.Fatal(unreadByteExpectedErrorˢ);
        }
    }
}

internal partial struct TestReaderCopyNothing_nErr /*dyn*/ {
    internal int64 n;
    internal error err;
}

internal partial struct TestReaderCopyNothing_justReader /*dyn*/ {
    /*embed*/ public io_package.Reader Reader;
}

internal partial struct TestReaderCopyNothing_justWriter /*dyn*/ {
    /*embed*/ public io_package.Writer Writer;
}

// verify that copying from an empty reader always has the same results,
// regardless of the presence of a WriteTo method.
public static void TestReaderCopyNothing(ж<testing.T> t) {
    var discard = new TestReaderCopyNothing_justWriter(Δio.Discard); // hide ReadFrom

    TestReaderCopyNothing_nErr with = default;
    TestReaderCopyNothing_nErr withOut = default;
    (with.n, with.err) = Δio.Copy(discard, new bytes_test_package.bytes_ReaderжReader(NewReader(default)));
    (withOut.n, withOut.err) = Δio.Copy(discard, new TestReaderCopyNothing_justReader(new bytes_test_package.bytes_ReaderжReader(NewReader(default))));
    if (with != withOut) {
        t.Errorf("behavior differs: with = %#v; without: %#v", with, withOut);
    }
}

// tests that Len is affected by reads, but Size is not.
public static void TestReaderLenSize(ж<testing.T> t) {
    var r = NewReader(slice<byte>("abc"u8));
    Δio.CopyN(Δio.Discard, new bytes_test_package.bytes_ReaderжReader(r), 1);
    if (r.Len() != 2) {
        t.Errorf("Len = %d; want 2", r.Len());
    }
    if (r.Size() != 3) {
        t.Errorf("Size = %d; want 3", r.Size());
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = "abcdef"u8;

public static void TestReaderReset(ж<testing.T> t) {
    var r = NewReader(slice<byte>("世界"u8));
    {
        var (_, _, errΔ1) = r.ReadRune(); if (errΔ1 != nil) {
            t.Errorf("ReadRune: unexpected error: %v", errΔ1);
        }
    }

    @string want = wantᶜ;
    r.Reset(slice<byte>(want));
    {
        var errΔ2 = r.UnreadRune(); if (errΔ2 == nil) {
            t.Errorf("UnreadRune: expected error, got nil");
        }
    }
    var (buf, err) = Δio.ReadAll(new bytes_test_package.bytes_ReaderжReader(r));
    if (err != nil) {
        t.Errorf("ReadAll: unexpected error: %v", err);
    }
    {
        @string got = (@string)buf; if (got != want) {
            t.Errorf("ReadAll: got %q, want %q", got, want);
        }
    }
}

public static void TestReaderZero(ж<testing.T> t) {
    {
        nint l = (Ꮡ(new Reader())).Len(); if (l != 0) {
            t.Errorf("Len: got %d, want 0", l);
        }
    }

    {
        var (n, err) = (Ꮡ(new Reader())).Read(default); if (n != 0 || !AreEqual(err, Δio.EOF)) {
            t.Errorf("Read: got %d, %v; want 0, io.EOF", n, err);
        }
    }

    {
        var (n, err) = (Ꮡ(new Reader())).ReadAt(default, 11); if (n != 0 || !AreEqual(err, Δio.EOF)) {
            t.Errorf("ReadAt: got %d, %v; want 0, io.EOF", n, err);
        }
    }

    {
        var (b, err) = (Ꮡ(new Reader())).ReadByte(); if (b != 0 || !AreEqual(err, Δio.EOF)) {
            t.Errorf("ReadByte: got %d, %v; want 0, io.EOF", b, err);
        }
    }

    {
        var (ch, size, err) = (Ꮡ(new Reader())).ReadRune(); if (ch != 0 || size != 0 || !AreEqual(err, Δio.EOF)) {
            t.Errorf("ReadRune: got %d, %d, %v; want 0, 0, io.EOF", ch, size, err);
        }
    }

    {
        var (offset, err) = (Ꮡ(new Reader())).Seek(11, Δio.SeekStart); if (offset != 11 || err != nil) {
            t.Errorf("Seek: got %d, %v; want 11, nil", offset, err);
        }
    }

    {
        var s = (Ꮡ(new Reader())).Size(); if (s != 0) {
            t.Errorf("Size: got %d, want 0", s);
        }
    }

    if ((Ꮡ(new Reader())).UnreadByte() == nil) {
        t.Errorf("UnreadByte: got nil, want error");
    }

    if ((Ꮡ(new Reader())).UnreadRune() == nil) {
        t.Errorf("UnreadRune: got nil, want error");
    }

    {
        var (n, err) = (Ꮡ(new Reader())).WriteTo(Δio.Discard); if (n != 0 || err != nil) {
            t.Errorf("WriteTo: got %d, %v; want 0, nil", n, err);
        }
    }
}

} // end bytes_test_package
