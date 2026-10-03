// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static bytes_package;
using fmt = fmt_package;
using Δio = io_package;
using Δsync = sync_package;
using testing = testing_package;
using bytes = bytes_package;
using static go.bytes_internal_test_package;

partial class bytes_test_package {

[GoType("dyn")] internal partial struct TestReader_tests {
    internal int64 off;
    internal nint seek;
    internal nint n;
    internal @string want;
    internal int64 wantpos;
    internal error readerr;
    internal @string seekerr;
}

public static void TestReader(ж<testing.T> Ꮡt) {
    var r = NewReader(slice<byte>("0123456789"u8));
    var tests = new TestReader_tests[]{
        new(seek: Δio.SeekStart, off: 0, n: 20, want: "0123456789"u8),
        new(seek: Δio.SeekStart, off: 1, n: 1, want: "1"u8),
        new(seek: Δio.SeekCurrent, off: 1, wantpos: 3, n: 2, want: "34"u8),
        new(seek: Δio.SeekStart, off: -1, seekerr: "bytes.Reader.Seek: negative position"u8),
        new(seek: Δio.SeekStart, off: 8589934592L, wantpos: 8589934592L, readerr: Δio.EOF),
        new(seek: Δio.SeekCurrent, off: 1, wantpos: 8589934593L, readerr: Δio.EOF),
        new(seek: Δio.SeekStart, n: 5, want: "01234"u8),
        new(seek: Δio.SeekCurrent, n: 5, want: "56789"u8),
        new(seek: Δio.SeekEnd, off: -1, n: 1, wantpos: 9, want: "9"u8)
    }.slice();
    foreach (var (i, tt) in tests) {
        var (pos, err) = r.Seek(tt.off, tt.seek);
        if (err == default! && tt.seekerr != ""u8) {
            Ꮡt.Errorf("%d. want seek error %q"u8, i, tt.seekerr);
            continue;
        }
        if (err != default! && err.Error() != tt.seekerr) {
            Ꮡt.Errorf("%d. seek error = %q; want %q"u8, i, err.Error(), tt.seekerr);
            continue;
        }
        if (tt.wantpos != 0 && tt.wantpos != pos) {
            Ꮡt.Errorf("%d. pos = %d, want %d"u8, i, pos, tt.wantpos);
        }
        var buf = new slice<byte>(tt.n);
        (var n, err) = r.Read(buf);
        if (!AreEqual(err, tt.readerr)) {
            Ꮡt.Errorf("%d. read = %v; want %v"u8, i, err, tt.readerr);
            continue;
        }
        @string got = ((@string)(buf[..(int)(n)]));
        if (got != tt.want) {
            Ꮡt.Errorf("%d. got %q; want %q"u8, i, got, tt.want);
        }
    }
}

public static void TestReadAfterBigSeek(ж<testing.T> Ꮡt) {
    var r = NewReader(slice<byte>("0123456789"u8));
    {
        var (_, err) = r.Seek(2147483653L, Δio.SeekStart); if (err != default!) {
            Ꮡt.Fatal(err);
        }
    }
    {
        var (n, err) = r.Read(new slice<byte>(10)); if (n != 0 || !AreEqual(err, Δio.EOF)) {
            Ꮡt.Errorf("Read = %d, %v; want 0, EOF"u8, n, err);
        }
    }
}

[GoType("dyn")] internal partial struct TestReaderAt_tests {
    internal int64 off;
    internal nint n;
    internal @string want;
    internal any wanterr;
}

public static void TestReaderAt(ж<testing.T> Ꮡt) {
    var r = NewReader(slice<byte>("0123456789"u8));
    var tests = new TestReaderAt_tests[]{
        new(0, 10, "0123456789"u8, default!),
        new(1, 10, "123456789"u8, Δio.EOF),
        new(1, 9, "123456789"u8, default!),
        new(11, 10, ""u8, Δio.EOF),
        new(0, 0, ""u8, default!),
        new(-1, 0, ""u8, (@string)"bytes.Reader.ReadAt: negative offset"u8)
    }.slice();
    foreach (var (i, tt) in tests) {
        var b = new slice<byte>(tt.n);
        var (rn, err) = r.ReadAt(b, tt.off);
        @string got = ((@string)(b[..(int)(rn)]));
        if (got != tt.want) {
            Ꮡt.Errorf("%d. got %q; want %q"u8, i, got, tt.want);
        }
        if (fmt.Sprintf("%v"u8, err) != fmt.Sprintf("%v"u8, tt.wanterr)) {
            Ꮡt.Errorf("%d. got error = %v; want %v"u8, i, err, tt.wanterr);
        }
    }
}

public static void TestReaderAtConcurrent(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

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
        }, i);
    }
    Ꮡwg.Wait();
}

public static void TestEmptyReaderConcurrent(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test for the race detector, to verify a Read that doesn't yield any bytes
    // is okay to use from multiple goroutines. This was our historic behavior.
    // See golang.org/issue/7856
    var r = NewReader(new byte[]{}.slice());
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
        });
        var rʗ2 = r;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                rʗ2.Read(default!);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

public static void TestReaderWriteTo(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 30; i += 3) {
        nint l = default!;
        if (i > 0) {
            l = len(testString) / i;
        }
        @string s = testString[..(int)(l)];
        var r = NewReader(testBytes[..(int)(l)]);
        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        var (n, err) = r.WriteTo(new bytes_test_package.bytes_BufferжWriter(Ꮡb));
        {
            var expect = (int64)len(s); if (n != expect) {
                Ꮡt.Errorf("got %v; want %v"u8, n, expect);
            }
        }
        if (err != default!) {
            Ꮡt.Errorf("for length %d: got error = %v; want nil"u8, l, err);
        }
        if (Ꮡb.String() != s) {
            Ꮡt.Errorf("got string %q; want %q"u8, Ꮡb.String(), s);
        }
        if (r.Len() != 0) {
            Ꮡt.Errorf("reader contains %v bytes; want 0"u8, r.Len());
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string dataᶜ = "hello world"u8;

public static void TestReaderLen(ж<testing.T> Ꮡt) {
    @string data = dataᶜ;
    var r = NewReader(slice<byte>(data));
    {
        nint got = r.Len();
        nint want = 11; if (got != want) {
            Ꮡt.Errorf("r.Len(): got %d, want %d"u8, got, want);
        }
    }
    {
        var (n, err) = r.Read(new slice<byte>(10)); if (err != default! || n != 10) {
            Ꮡt.Errorf("Read failed: read %d %v"u8, n, err);
        }
    }
    {
        nint got = r.Len();
        nint want = 1; if (got != want) {
            Ꮡt.Errorf("r.Len(): got %d, want %d"u8, got, want);
        }
    }
    {
        var (n, err) = r.Read(new slice<byte>(1)); if (err != default! || n != 1) {
            Ꮡt.Errorf("Read failed: read %d %v; want 1, nil"u8, n, err);
        }
    }
    {
        nint got = r.Len();
        nint want = 0; if (got != want) {
            Ꮡt.Errorf("r.Len(): got %d, want %d"u8, got, want);
        }
    }
}


[GoType("dyn")] partial struct UnreadRuneErrorTestsᴛ1 {
    internal @string name;
    internal Action<ж<bytes.Reader>> f;
}
public static slice<UnreadRuneErrorTestsᴛ1> UnreadRuneErrorTests = new UnreadRuneErrorTestsᴛ1[]{
    new("Read"u8, (ж<bytes.Reader> r) => {
        r.Read(new byte[]{0}.slice());
    }),
    new("ReadByte"u8, (ж<bytes.Reader> r) => {
        r.ReadByte();
    }),
    new("UnreadRune"u8, (ж<bytes.Reader> r) => {
        r.UnreadRune();
    }),
    new("Seek"u8, (ж<bytes.Reader> r) => {
        r.Seek(0, Δio.SeekCurrent);
    }),
    new("WriteTo"u8, (ж<bytes.Reader> r) => {
        r.WriteTo(new bytes_test_package.bytes_BufferжWriter(Ꮡ(new Buffer(nil))));
    })
}.slice();

public static void TestUnreadRuneError(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in UnreadRuneErrorTests) {
        var reader = NewReader(slice<byte>("0123456789"u8));
        {
            var (_, _, errΔ1) = reader.ReadRune(); if (errΔ1 != default!) {
                // should not happen
                Ꮡt.Fatal(errΔ1);
            }
        }
        tt.f(reader);
        var err = reader.UnreadRune();
        if (err == default!) {
            Ꮡt.Errorf("Unreading after %s: expected error"u8, tt.name);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unreadByteExpectedErrorˢ = (@string)"UnreadByte: expected error, got nil"u8;

public static void TestReaderDoubleUnreadRune(ж<testing.T> Ꮡt) {
    var buf = NewBuffer(slice<byte>("groucho"u8));
    {
        var (_, _, err) = buf.ReadRune(); if (err != default!) {
            // should not happen
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = buf.UnreadByte(); if (err != default!) {
            // should not happen
            Ꮡt.Fatal(err);
        }
    }
    {
        var err = buf.UnreadByte(); if (err == default!) {
            Ꮡt.Fatal(unreadByteExpectedErrorˢ);
        }
    }
}

[GoType("dyn")] internal partial struct TestReaderCopyNothing_nErr {
    internal int64 n;
    internal error err;
}

[GoType("dyn")] internal partial struct TestReaderCopyNothing_justReader {
    [GoEmbedded] public io_package.Reader Reader;
}

[GoType("dyn")] internal partial struct TestReaderCopyNothing_justWriter {
    [GoEmbedded] public io_package.Writer Writer;
}

// verify that copying from an empty reader always has the same results,
// regardless of the presence of a WriteTo method.
public static void TestReaderCopyNothing(ж<testing.T> Ꮡt) {
    var discard = new TestReaderCopyNothing_justWriter(Δio.Discard); // hide ReadFrom
    TestReaderCopyNothing_nErr with = default!;
    TestReaderCopyNothing_nErr withOut = default!;
    (with.n, with.err) = Δio.Copy(discard, new bytes_test_package.bytes_ReaderжReader(NewReader(default!)));
    (withOut.n, withOut.err) = Δio.Copy(discard, new TestReaderCopyNothing_justReader(new bytes_test_package.bytes_ReaderжReader(NewReader(default!))));
    if (with != withOut) {
        Ꮡt.Errorf("behavior differs: with = %#v; without: %#v"u8, with, withOut);
    }
}

// tests that Len is affected by reads, but Size is not.
public static void TestReaderLenSize(ж<testing.T> Ꮡt) {
    var r = NewReader(slice<byte>("abc"u8));
    Δio.CopyN(Δio.Discard, new bytes_test_package.bytes_ReaderжReader(r), 1);
    if (r.Len() != 2) {
        Ꮡt.Errorf("Len = %d; want 2"u8, r.Len());
    }
    if (r.Size() != 3) {
        Ꮡt.Errorf("Size = %d; want 3"u8, r.Size());
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = "abcdef"u8;

public static void TestReaderReset(ж<testing.T> Ꮡt) {
    var r = NewReader(slice<byte>("世界"u8));
    {
        var (_, _, errΔ1) = r.ReadRune(); if (errΔ1 != default!) {
            Ꮡt.Errorf("ReadRune: unexpected error: %v"u8, errΔ1);
        }
    }
    @string want = wantᶜ;
    r.Reset(slice<byte>(want));
    {
        var errΔ2 = r.UnreadRune(); if (errΔ2 == default!) {
            Ꮡt.Errorf("UnreadRune: expected error, got nil"u8);
        }
    }
    var (buf, err) = Δio.ReadAll(new bytes_test_package.bytes_ReaderжReader(r));
    if (err != default!) {
        Ꮡt.Errorf("ReadAll: unexpected error: %v"u8, err);
    }
    {
        @string got = ((@string)buf); if (got != want) {
            Ꮡt.Errorf("ReadAll: got %q, want %q"u8, got, want);
        }
    }
}

public static void TestReaderZero(ж<testing.T> Ꮡt) {
    {
        nint l = (Ꮡ(new Reader(nil))).Len(); if (l != 0) {
            Ꮡt.Errorf("Len: got %d, want 0"u8, l);
        }
    }
    {
        var (n, err) = (Ꮡ(new Reader(nil))).Read(default!); if (n != 0 || !AreEqual(err, Δio.EOF)) {
            Ꮡt.Errorf("Read: got %d, %v; want 0, io.EOF"u8, n, err);
        }
    }
    {
        var (n, err) = (Ꮡ(new Reader(nil))).ReadAt(default!, 11); if (n != 0 || !AreEqual(err, Δio.EOF)) {
            Ꮡt.Errorf("ReadAt: got %d, %v; want 0, io.EOF"u8, n, err);
        }
    }
    {
        var (b, err) = (Ꮡ(new Reader(nil))).ReadByte(); if (b != 0 || !AreEqual(err, Δio.EOF)) {
            Ꮡt.Errorf("ReadByte: got %d, %v; want 0, io.EOF"u8, b, err);
        }
    }
    {
        var (ch, size, err) = (Ꮡ(new Reader(nil))).ReadRune(); if (ch != 0 || size != 0 || !AreEqual(err, Δio.EOF)) {
            Ꮡt.Errorf("ReadRune: got %d, %d, %v; want 0, 0, io.EOF"u8, ch, size, err);
        }
    }
    {
        var (offset, err) = (Ꮡ(new Reader(nil))).Seek(11, Δio.SeekStart); if (offset != 11 || err != default!) {
            Ꮡt.Errorf("Seek: got %d, %v; want 11, nil"u8, offset, err);
        }
    }
    {
        var s = (Ꮡ(new Reader(nil))).Size(); if (s != 0) {
            Ꮡt.Errorf("Size: got %d, want 0"u8, s);
        }
    }
    if ((Ꮡ(new Reader(nil))).UnreadByte() == default!) {
        Ꮡt.Errorf("UnreadByte: got nil, want error"u8);
    }
    if ((Ꮡ(new Reader(nil))).UnreadRune() == default!) {
        Ꮡt.Errorf("UnreadRune: got nil, want error"u8);
    }
    {
        var (n, err) = (Ꮡ(new Reader(nil))).WriteTo(Δio.Discard); if (n != 0 || err != default!) {
            Ꮡt.Errorf("WriteTo: got %d, %v; want 0, nil"u8, n, err);
        }
    }
}

} // end bytes_test_package
