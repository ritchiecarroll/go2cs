// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.compress;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using rand = go.math.rand_package;
using runtime = runtime_package;
using testing = testing_package;
using go.math;
using static go.compress.flate_package;

partial class flate_internal_test_package {

public static void BenchmarkEncode(ж<testing.B> b) {
    doBench(b, (ж<testing.B> bΔ1, slice<byte> buf0, nint level, nint n) => {
        bΔ1.StopTimer();
        bΔ1.SetBytes((int64)n);

        var buf1 = new slice<byte>(n);
        for (nint i = 0; i < n; i += len(buf0)) {
            if (len(buf0) > n - i) {
                buf0 = buf0.slice(0, n - i);
            }
            copy(buf1.slice(i), buf0);
        }
        buf0 = default;
        var (w, err) = NewWriter(io.Discard, level);
        if (err != nil) {
            bΔ1.Fatal(err);
        }
        runtime.GC();
        bΔ1.StartTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            w.Reset(io.Discard);
            w.Write(buf1);
            w.Close();
        }
    });
}

// errorWriter is a writer that fails after N writes.
internal partial struct errorWriter {
    public nint N;
}

internal static (nint, error) Write(this ref errorWriter e, slice<byte> b) {
    if (e.N <= 0) {
        return (0, io.ErrClosedPipe);
    }
    e.N--;
    return (len(b), default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object levelˢ = (@string)"Level"u8;
internal static readonly object expected0LengthWriteGotˢ = (@string)"Expected 0 length write, got"u8;
internal static readonly object expectedAnErrorˢ = (@string)"Expected an error"u8;
internal static readonly object expectedAnErrorOnFlushˢ = (@string)"Expected an error on flush"u8;
internal static readonly object expectedAnErrorOnCloseˢ = (@string)"Expected an error on close"u8;
internal static readonly object gotUnexpectedErrorAfterˢ = (@string)"Got unexpected error after reset:"u8;
internal static readonly object got0LengthWriteExpected0ˢ = (@string)"Got 0 length write, expected > 0"u8;

internal partial struct TestWriteError_src /*dyn*/ {
    /*embed*/ public io_package.Reader Reader;
}

// Test if errors from the underlying writer is passed upwards.
public static void TestWriteError(ж<testing.T> t) {
    t.Parallel();
    var buf = @new<bytes.Buffer>();
    nint n = 65536;
    if (!testing.Short()) {
        n *= 4;
    }
    for (nint i = 0; i < n; i++) {
        fmt.Fprintf(new flate_test_package.bytes_BufferжWriter(buf), "asdasfasf%d%dfghfgujyut%dyutyu\n"u8, i, i, i);
    }
    var @in = buf.Bytes();
    // We create our own buffer to control number of writes.
    var copyBuffer = new slice<byte>(128);
    for (nint l = 0; l < 10; l++) {
        for (nint failᴛ1 = 1; failᴛ1 <= 256; failᴛ1 *= 2) {
            ref var fail = ref heap<nint>(out var Ꮡfail);
            fail = failᴛ1;
            // Fail after 'fail' writes
            var ew = Ꮡ(new errorWriter(N: fail));
            var (w, err) = NewWriter(new flate_internal_test_package.errorWriterжWriter(ew), l);
            if (err != nil) {
                t.Fatalf("NewWriter: level %d: %v", l, err);
            }
            (var nΔ1, err) = io.CopyBuffer(new flate_test_package.flate_WriterжWriter(w), new TestWriteError_src(new flate_test_package.bytes_BufferжReader(bytes.NewBuffer(@in))), copyBuffer);
            if (err == nil) {
                t.Fatalf("Level %d: Expected an error, writer was %#v", l, ew.OrTypedNil());
            }
            (var n2, err) = w.Write([1, 2, 2, 3, 4, 5]);
            if (n2 != 0) {
                t.Fatal(levelˢ, l, expected0LengthWriteGotˢ, nΔ1);
            }
            if (err == nil) {
                t.Fatal(levelˢ, l, expectedAnErrorˢ);
            }
            err = w.Flush();
            if (err == nil) {
                t.Fatal(levelˢ, l, expectedAnErrorOnFlushˢ);
            }
            err = w.Close();
            if (err == nil) {
                t.Fatal(levelˢ, l, expectedAnErrorOnCloseˢ);
            }

            w.Reset(io.Discard);
            (n2, err) = w.Write([1, 2, 3, 4, 5, 6]);
            if (err != nil) {
                t.Fatal(levelˢ, l, gotUnexpectedErrorAfterˢ, err);
            }
            if (n2 == 0) {
                t.Fatal(levelˢ, l, got0LengthWriteExpected0ˢ);
            }
            if (testing.Short()) {
                return;
            }
            failᴛ1 = fail;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string lm2ˢ => ref lm2ˢᵇ;
private static readonly @string lm2ˢᵇ = "LM2"u8;

// Test if two runs produce identical results
// even when writing different sizes to the Writer.
public static void TestDeterministic(ж<testing.T> t) {
    t.Parallel();
    for (nint iᴛ1 = 0; iᴛ1 <= 9; iᴛ1++) {
        var i = iᴛ1;
        t.Run(fmt.Sprint("L", i), (ж<testing.T> tΔ1) => {
            testDeterministic(i, tΔ1);
        });
    }
    t.Run(lm2ˢ, (ж<testing.T> tΔ2) => {
        testDeterministic(-2, tΔ2);
    });
}

internal static void testDeterministic(nint i, ж<testing.T> t) {
    t.Parallel();
    // Test so much we cross a good number of block boundaries.
    nint length = maxStoreBlockSize * 30 + 500;
    if (testing.Short()) {
        length /= 10;
    }

    // Create a random, but compressible stream.
    var rng = rand.New(rand.NewSource(1));
    var t1 = new slice<byte>(length);
    foreach (var iΔ1 in range(len(t1))) {
        t1[iΔ1] = (byte)(rng.Int63() & 7);
    }

    // Do our first encode.
    ref var b1 = ref heap(new bytes.Buffer(), out var Ꮡb1);
    var br = bytes.NewBuffer(t1);
    var (w, err) = NewWriter(new flate_test_package.bytes_BufferжWriter(Ꮡb1), i);
    if (err != nil) {
        t.Fatal(err);
    }
    // Use a very small prime sized buffer.
    var cbuf = new slice<byte>(787);
    (_, err) = io.CopyBuffer(new flate_test_package.flate_WriterжWriter(w), new TestWriteError_src(new flate_test_package.bytes_BufferжReader(br)), cbuf);
    if (err != nil) {
        t.Fatal(err);
    }
    w.Close();

    // We choose a different buffer size,
    // bigger than a maximum block, and also a prime.
    ref var b2 = ref heap(new bytes.Buffer(), out var Ꮡb2);
    cbuf = new slice<byte>(81761);
    var br2 = bytes.NewBuffer(t1);
    (var w2, err) = NewWriter(new flate_test_package.bytes_BufferжWriter(Ꮡb2), i);
    if (err != nil) {
        t.Fatal(err);
    }
    (_, err) = io.CopyBuffer(new flate_test_package.flate_WriterжWriter(w2), new TestWriteError_src(new flate_test_package.bytes_BufferжReader(br2)), cbuf);
    if (err != nil) {
        t.Fatal(err);
    }
    w2.Close();

    var b1b = b1.Bytes();
    var b2b = b2.Bytes();

    if (!bytes.Equal(b1b, b2b)) {
        t.Errorf("level %d did not produce deterministic result, result mismatch, len(a) = %d, len(b) = %d", i, len(b1b), len(b2b));
    }
}

// TestDeflateFast_Reset will test that encoding is consistent
// across a warparound of the table offset.
// See https://github.com/golang/go/issues/34121
public static void TestDeflateFast_Reset(ж<testing.T> t) {
    var buf = @new<bytes.Buffer>();
    nint n = 65536;

    for (nint i = 0; i < n; i++) {
        fmt.Fprintf(new flate_test_package.bytes_BufferжWriter(buf), "asdfasdfasdfasdf%d%dfghfgujyut%dyutyu\n"u8, i, i, i);
    }
    // This is specific to level 1.
    const nint level = 1;
    var @in = buf.Bytes();
    nint offset = 1;
    if (testing.Short()) {
        offset = 256;
    }

    // We do an encode with a clean buffer to compare.
    ref var want = ref heap(new bytes.Buffer(), out var Ꮡwant);
    var (w, err) = NewWriter(new flate_test_package.bytes_BufferжWriter(Ꮡwant), level);
    if (err != nil) {
        t.Fatalf("NewWriter: level %d: %v", (nint)(level), err);
    }

    // Output written 3 times.
    w.Write(@in);
    w.Write(@in);
    w.Write(@in);
    w.Close();

    for (; offset <= 256; offset *= 2) {
        var (wΔ1, errΔ1) = NewWriter(io.Discard, level);
        if (errΔ1 != nil) {
            t.Fatalf("NewWriter: level %d: %v", (nint)(level), errΔ1);
        }

        // Reset until we are right before the wraparound.
        // Each reset adds maxMatchOffset to the offset.
        for (nint i = 0; i < ((nint)bufferReset - len(@in) - offset - (nint)maxMatchOffset) / (nint)maxMatchOffset; i++) {
            // skip ahead to where we are close to wrap around...
            wΔ1.Ꮡd.reset(default);
        }
        ref var got = ref heap(new bytes.Buffer(), out var Ꮡgot);
        wΔ1.Reset(new flate_test_package.bytes_BufferжWriter(Ꮡgot));

        // Write 3 times, close.
        for (nint i = 0; i < 3; i++) {
            (_, errΔ1) = wΔ1.Write(@in);
            if (errΔ1 != nil) {
                t.Fatal(errΔ1);
            }
        }
        errΔ1 = wΔ1.Close();
        if (errΔ1 != nil) {
            t.Fatal(errΔ1);
        }
        if (!bytes.Equal(got.Bytes(), want.Bytes())) {
            t.Fatalf("output did not match at wraparound, len(want)  = %d, len(got) = %d", want.Len(), got.Len());
        }
    }
}

} // end flate_internal_test_package
