// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using Δio = io_package;
using strings = strings_package;
using Δsync = sync_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.strings_internal_test_package;

partial class strings_test_package {

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
    var r = strings.NewReader("0123456789"u8);
    var tests = new TestReader_tests[]{
        new(seek: Δio.SeekStart, off: 0, n: 20, want: "0123456789"u8),
        new(seek: Δio.SeekStart, off: 1, n: 1, want: "1"u8),
        new(seek: Δio.SeekCurrent, off: 1, wantpos: 3, n: 2, want: "34"u8),
        new(seek: Δio.SeekStart, off: -1, seekerr: "strings.Reader.Seek: negative position"u8),
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
        @string got = ((@string)(buf.slice(0, n)));
        if (got != tt.want) {
            Ꮡt.Errorf("%d. got %q; want %q"u8, i, got, tt.want);
        }
    }
}

public static void TestReadAfterBigSeek(ж<testing.T> Ꮡt) {
    var r = strings.NewReader("0123456789"u8);
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
    var r = strings.NewReader("0123456789"u8);
    var tests = new TestReaderAt_tests[]{
        new(0, 10, "0123456789"u8, default!),
        new(1, 10, "123456789"u8, Δio.EOF),
        new(1, 9, "123456789"u8, default!),
        new(11, 10, ""u8, Δio.EOF),
        new(0, 0, ""u8, default!),
        new(-1, 0, ""u8, (@string)"strings.Reader.ReadAt: negative offset"u8)
    }.slice();
    foreach (var (i, tt) in tests) {
        var b = new slice<byte>(tt.n);
        var (rn, err) = r.ReadAt(b, tt.off);
        @string got = ((@string)(b.slice(0, rn)));
        if (got != tt.want) {
            Ꮡt.Errorf("%d. got %q; want %q"u8, i, got, tt.want);
        }
        if (fmt.Sprintf("%v"u8, err) != fmt.Sprintf("%v"u8, tt.wanterr)) {
            Ꮡt.Errorf("%d. got error = %v; want %v"u8, i, err, tt.wanterr);
        }
    }
}

public static partial void TestReaderAtConcurrent(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test for the race detector, to verify ReadAt doesn't mutate
    // any state.
    var r = strings.NewReader("0123456789"u8);
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

public static partial void TestEmptyReaderConcurrent(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test for the race detector, to verify a Read that doesn't yield any bytes
    // is okay to use from multiple goroutines. This was our historic behavior.
    // See golang.org/issue/7856
    var r = strings.NewReader(""u8);
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

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string strᶜ = "0123456789"u8;

public static void TestWriteTo(ж<testing.T> Ꮡt) {
    @string str = strᶜ;
    for (nint i = 0; i <= len(str); i++) {
        @string s = str.slice(i);
        var r = strings.NewReader(s);
        ref var b = ref heap(new bytes.Buffer(), out var Ꮡb);
        var (n, err) = r.WriteTo(new strings_test_package.bytes_BufferжWriter(Ꮡb));
        {
            var expect = (int64)len(s); if (n != expect) {
                Ꮡt.Errorf("got %v; want %v"u8, n, expect);
            }
        }
        if (err != default!) {
            Ꮡt.Errorf("for length %d: got error = %v; want nil"u8, len(s), err);
        }
        if (Ꮡb.String() != s) {
            Ꮡt.Errorf("got string %q; want %q"u8, Ꮡb.String(), s);
        }
        if (r.Len() != 0) {
            Ꮡt.Errorf("reader contains %v bytes; want 0"u8, r.Len());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string abcˢ = "abc"u8;

// tests that Len is affected by reads, but Size is not.
public static void TestReaderLenSize(ж<testing.T> Ꮡt) {
    var r = strings.NewReader(abcˢ);
    Δio.CopyN(Δio.Discard, new strings_test_package.strings_ReaderжReader(r), 1);
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
    var r = strings.NewReader("世界"u8);
    {
        var (_, _, errΔ1) = r.ReadRune(); if (errΔ1 != default!) {
            Ꮡt.Errorf("ReadRune: unexpected error: %v"u8, errΔ1);
        }
    }
    @string want = wantᶜ;
    r.Reset(want);
    {
        var errΔ2 = r.UnreadRune(); if (errΔ2 == default!) {
            Ꮡt.Errorf("UnreadRune: expected error, got nil"u8);
        }
    }
    var (buf, err) = Δio.ReadAll(new strings_test_package.strings_ReaderжReader(r));
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
        nint l = (Ꮡ(new strings.Reader(nil))).Len(); if (l != 0) {
            Ꮡt.Errorf("Len: got %d, want 0"u8, l);
        }
    }
    {
        var (n, err) = (Ꮡ(new strings.Reader(nil))).Read(default!); if (n != 0 || !AreEqual(err, Δio.EOF)) {
            Ꮡt.Errorf("Read: got %d, %v; want 0, io.EOF"u8, n, err);
        }
    }
    {
        var (n, err) = (Ꮡ(new strings.Reader(nil))).ReadAt(default!, 11); if (n != 0 || !AreEqual(err, Δio.EOF)) {
            Ꮡt.Errorf("ReadAt: got %d, %v; want 0, io.EOF"u8, n, err);
        }
    }
    {
        var (b, err) = (Ꮡ(new strings.Reader(nil))).ReadByte(); if (b != 0 || !AreEqual(err, Δio.EOF)) {
            Ꮡt.Errorf("ReadByte: got %d, %v; want 0, io.EOF"u8, b, err);
        }
    }
    {
        var (ch, size, err) = (Ꮡ(new strings.Reader(nil))).ReadRune(); if (ch != 0 || size != 0 || !AreEqual(err, Δio.EOF)) {
            Ꮡt.Errorf("ReadRune: got %d, %d, %v; want 0, 0, io.EOF"u8, ch, size, err);
        }
    }
    {
        var (offset, err) = (Ꮡ(new strings.Reader(nil))).Seek(11, Δio.SeekStart); if (offset != 11 || err != default!) {
            Ꮡt.Errorf("Seek: got %d, %v; want 11, nil"u8, offset, err);
        }
    }
    {
        var s = (Ꮡ(new strings.Reader(nil))).Size(); if (s != 0) {
            Ꮡt.Errorf("Size: got %d, want 0"u8, s);
        }
    }
    if ((Ꮡ(new strings.Reader(nil))).UnreadByte() == default!) {
        Ꮡt.Errorf("UnreadByte: got nil, want error"u8);
    }
    if ((Ꮡ(new strings.Reader(nil))).UnreadRune() == default!) {
        Ꮡt.Errorf("UnreadRune: got nil, want error"u8);
    }
    {
        var (n, err) = (Ꮡ(new strings.Reader(nil))).WriteTo(Δio.Discard); if (n != 0 || err != default!) {
            Ꮡt.Errorf("WriteTo: got %d, %v; want 0, nil"u8, n, err);
        }
    }
}

} // end strings_test_package
