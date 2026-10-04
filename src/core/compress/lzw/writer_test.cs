// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using bytes = bytes_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using io = io_package;
using math = math_package;
using os = os_package;
using runtime = runtime_package;
using testing = testing_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.compress.lzw_package;

partial class lzw_internal_test_package {

internal static slice<@string> filenames = new @string[]{
    "../testdata/gettysburg.txt"u8,
    "../testdata/e.txt"u8,
    "../testdata/pi.txt"u8
}.slice();

// testFile tests that compressing and then decompressing the given file with
// the given options yields equivalent bytes to the original file.
[MethodImpl(MethodImplOptions.NoInlining)] internal static void testFile(ж<testing.T> Ꮡt, @string fn, global::go.compress.lzw_package.Order order, nint litWidth) {
    GoFrame ᒐ = default;
    try {
        // Read the file, as golden output.
        var (golden, err) = os.Open(fn);
        if (err != default!) {
            Ꮡt.Errorf("%s (order=%d litWidth=%d): %v"u8, fn, order, litWidth, err);
            return;
        }
        var goldenʗ1 = golden;
        defer(() => goldenʗ1.Close(), ref ᒐ);
        // Read the file again, and push it through a pipe that compresses at the write end, and decompresses at the read end.
        (var raw, err) = os.Open(fn);
        if (err != default!) {
            Ꮡt.Errorf("%s (order=%d litWidth=%d): %v"u8, fn, order, litWidth, err);
            return;
        }
        var (piper, pipew) = io.Pipe();
        var piperʗ1 = piper;
        defer(() => piperʗ1.Close(), ref ᒐ);
        var pipewʗ1 = pipew;
        var rawʗ1 = raw;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var rawʗ2 = rawʗ1;
                defer(() => rawʗ2.Close(), ref ᒐ);
                var pipewʗ2 = pipewʗ1;
                defer(() => pipewʗ2.Close(), ref ᒐ);
                var lzww = NewWriter(new io.PipeWriterжWriter(pipewʗ1), order, litWidth);
                var lzwwʗ1 = lzww;
                defer(() => lzwwʗ1.Close(), ref ᒐ);
                ref var b = ref heap(new array<byte>(4096), out var Ꮡb);
                while (ᐧ) {
                    var (n, err0Δ1) = rawʗ1.Read(b[..]);
                    if (err0Δ1 != default! && !AreEqual(err0Δ1, io.EOF)) {
                        Ꮡt.Errorf("%s (order=%d litWidth=%d): %v"u8, fn, order, litWidth, err0Δ1);
                        return;
                    }
                    var (_, err1Δ1) = lzww.Write(b.slice(0, n));
                    if (err1Δ1 != default!) {
                        Ꮡt.Errorf("%s (order=%d litWidth=%d): %v"u8, fn, order, litWidth, err1Δ1);
                        return;
                    }
                    if (AreEqual(err0Δ1, io.EOF)) {
                        break;
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        var lzwr = NewReader(new io.PipeReaderжReader(piper), order, litWidth);
        var lzwrʗ1 = lzwr;
        defer(() => lzwrʗ1.Close(), ref ᒐ);
        // Compare the two.
        var (b0, err0) = io.ReadAll(new lzw_internal_test_package.os_FileжReader(golden));
        var (b1, err1) = io.ReadAll(lzwr);
        if (err0 != default!) {
            Ꮡt.Errorf("%s (order=%d litWidth=%d): %v"u8, fn, order, litWidth, err0);
            return;
        }
        if (err1 != default!) {
            Ꮡt.Errorf("%s (order=%d litWidth=%d): %v"u8, fn, order, litWidth, err1);
            return;
        }
        if (len(b1) != len(b0)) {
            Ꮡt.Errorf("%s (order=%d litWidth=%d): length mismatch %d != %d"u8, fn, order, litWidth, len(b1), len(b0));
            return;
        }
        for (nint i = 0; i < len(b0); i++) {
            if (b1[i] != b0[i]) {
                Ꮡt.Errorf("%s (order=%d litWidth=%d): mismatch at %d, 0x%02x != 0x%02x\n"u8, fn, order, litWidth, i, b1[i], b0[i]);
                return;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestWriter(ж<testing.T> Ꮡt) {
    foreach (var (_, filename) in filenames) {
        foreach (var (_, order) in new global::go.compress.lzw_package.Order[]{LSB, MSB}.array()) {
            // The test data "2.71828 etcetera" is ASCII text requiring at least 6 bits.
            for (nint litWidth = 6; litWidth <= 8; litWidth++) {
                if (filename == "../testdata/gettysburg.txt"u8 && litWidth == 6) {
                    continue;
                }
                testFile(Ꮡt, filename, order, litWidth);
            }
        }
        if (testing.Short() && testenv.Builder() == ""u8) {
            break;
        }
    }
}

public static void TestWriterReset(ж<testing.T> Ꮡt) {
    foreach (var (_, order) in new global::go.compress.lzw_package.Order[]{LSB, MSB}.array()) {
        Ꮡt.Run(fmt.Sprintf("Order %d"u8, order), (ж<testing.T> tΔ1) => {
            for (nint litWidthᴛ1 = 6; litWidthᴛ1 <= 8; litWidthᴛ1++) {
                var litWidth = litWidthᴛ1;
                tΔ1.Run(fmt.Sprintf("LitWidth %d"u8, litWidth), (ж<testing.T> tΔ2) => {
                    slice<byte> data = default!;
                    if (litWidth == 6){
                        data = new byte[]{1, 2, 3}.slice();
                    } else {
                        data = slice<byte>(@"lorem ipsum dolor sit amet"u8);
                    }
                    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
                    var w = NewWriter(new lzw_internal_test_package.bytes_BufferжWriter(Ꮡbuf), order, litWidth);
                    {
                        var (_, err) = w.Write(data); if (err != default!) {
                            tΔ2.Errorf("write: %v: %v"u8, ((@string)data), err);
                        }
                    }
                    {
                        var err = w.Close(); if (err != default!) {
                            tΔ2.Errorf("close: %v"u8, err);
                        }
                    }
                    var b1 = buf.Bytes();
                    buf.Reset();
                    w._<ж<global::go.compress.lzw_package.Writer>>().Reset(new lzw_internal_test_package.bytes_BufferжWriter(Ꮡbuf), order, litWidth);
                    {
                        var (_, err) = w.Write(data); if (err != default!) {
                            tΔ2.Errorf("write: %v: %v"u8, ((@string)data), err);
                        }
                    }
                    {
                        var err = w.Close(); if (err != default!) {
                            tΔ2.Errorf("close: %v"u8, err);
                        }
                    }
                    var b2 = buf.Bytes();
                    if (!bytes.Equal(b1, b2)) {
                        tΔ2.Errorf("bytes written were not same"u8);
                    }
                });
            }
        });
    }
}

public static void TestWriterReturnValues(ж<testing.T> Ꮡt) {
    var w = NewWriter(io.Discard, LSB, 8);
    var (n, err) = w.Write(slice<byte>("asdf"u8));
    if (n != 4 || err != default!) {
        Ꮡt.Errorf("got %d, %v, want 4, nil"u8, n, err);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object writeAByte12GotNilErrorˢ = (@string)"write a byte >= 1<<2: got nil error, want non-nil"u8;

public static void TestSmallLitWidth(ж<testing.T> Ꮡt) {
    var w = NewWriter(io.Discard, LSB, 2);
    {
        var (_, err) = w.Write(new byte[]{0x03}.slice()); if (err != default!) {
            Ꮡt.Fatalf("write a byte < 1<<2: %v"u8, err);
        }
    }
    {
        var (_, err) = w.Write(new byte[]{0x04}.slice()); if (err == default!) {
            Ꮡt.Fatal(writeAByte12GotNilErrorˢ);
        }
    }
}

public static void TestStartsWithClearCode(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // A literal width of 7 bits means that the code width starts at 8 bits,
    // which makes it easier to visually inspect the output (provided that the
    // output is short so codes don't get longer). Each byte is a code:
    //  - ASCII bytes are literal codes,
    //  - 0x80 is the clear code,
    //  - 0x81 is the end code.
    //  - 0x82 and above are copy codes (unused in this test case).
    foreach (var (_, empty) in new bool[]{false, true}.slice()) {
        ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
        var w = NewWriter(new lzw_internal_test_package.bytes_BufferжWriter(Ꮡbuf), LSB, 7);
        if (!empty) {
            w.Write(slice<byte>("Hi"u8));
        }
        w.Close();
        @string got = Ꮡbuf.String();
        @string want = ((@string)(new byte[]{0x80, 0x81}));
        if (!empty) {
            want = ((@string)(new byte[]{0x80, 0x48, 0x69, 0x81}));
        }
        if (got != want) {
            Ꮡt.Errorf("empty=%t: got %q, want %q"u8, empty, got, want);
        }
    }
}

public static void BenchmarkEncoder(ж<testing.B> Ꮡb) {
    var (buf, err) = os.ReadFile(testdataETxtˢ);
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    if (len(buf) == 0) {
        Ꮡb.Fatalf("test file has no data"u8);
    }
    for (nint e = 4; e <= 6; e++) {
        nint n = (nint)math.Pow10(e);
        var buf0 = buf;
        var buf1 = new slice<byte>(n);
        for (nint i = 0; i < n; i += len(buf0)) {
            if (len(buf0) > n - i) {
                buf0 = buf0.slice(0, n - i);
            }
            copy(buf1.slice(i), buf0);
        }
        buf0 = default!;
        runtime.GC();
        var buf1ʗ1 = buf1;
        Ꮡb.Run(fmt.Sprint((@string)"1e"u8, e), (ж<testing.B> bΔ1) => {
            bΔ1.SetBytes((int64)n);
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var w = NewWriter(io.Discard, LSB, 8);
                w.Write(buf1ʗ1);
                w.Close();
            }
        });
        var buf1ʗ2 = buf1;
        Ꮡb.Run(fmt.Sprint(reuseˢ, e), (ж<testing.B> bΔ2) => {
            bΔ2.SetBytes((int64)n);
            var w = NewWriter(io.Discard, LSB, 8);
            for (nint i = 0; i < (~bΔ2).N; i++) {
                w.Write(buf1ʗ2);
                w.Close();
                w._<ж<global::go.compress.lzw_package.Writer>>().Reset(io.Discard, LSB, 8);
            }
        });
    }
}

} // end lzw_internal_test_package
