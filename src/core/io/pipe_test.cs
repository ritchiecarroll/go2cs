// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using fmt = fmt_package;
using static io_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.io_internal_test_package;
using Δio = io_package;

partial class io_test_package {

internal static void checkWrite(ж<testing.T> t, Δio.Writer w, slice<byte> data, channel<nint> c) {
    var (n, err) = w.Write(data);
    if (err != nil) {
        t.Errorf("write: %v", err);
    }
    if (n != len(data)) {
        t.Errorf("short write: %d != %d", n, len(data));
    }
    c.ᐸꟷ(0);
}

// Test a single read/write pair.
public static partial void TestPipe1(ж<testing.T> t) {
    var c = new channel<nint>(0);
    var (r, w) = Pipe();
    slice<byte> buf = new slice<byte>(64);
    goǃ(checkWrite, t, new Δio.PipeWriterжWriter(w), slice<byte>("hello, world"u8), c);
    var (n, err) = r.Read(buf);
    if (err != nil) {
        t.Errorf("read: %v", err);
    } else if (n != 12 || ((sstring)(buf[0..12])) != "hello, world"u8) {
        t.Errorf("bad read: got %q", buf.slice(0, n));
    }
    ᐸꟷ(c);
    r.Close();
    w.Close();
}

internal static void reader(ж<testing.T> t, Δio.Reader r, channel<nint> c) {
    slice<byte> buf = new slice<byte>(64);
    for (;;) {
        var (n, err) = r.Read(buf);
        if (AreEqual(err, EOF)) {
            c.ᐸꟷ(0);
            break;
        }
        if (err != nil) {
            t.Errorf("read: %v", err);
        }
        c.ᐸꟷ(n);
    }
}

// Test a sequence of read/write pairs.
public static partial void TestPipe2(ж<testing.T> t) {
    var c = new channel<nint>(0);
    var (r, w) = Pipe();
    goǃ(reader, t, new Δio.PipeReaderжReader(r), c);
    slice<byte> buf = new slice<byte>(64);
    for (nint i = 0; i < 5; i++) {
        var p = buf.slice(0, 5 + i * 10);
        var (n, err) = w.Write(p);
        if (n != len(p)) {
            t.Errorf("wrote %d, got %d", len(p), n);
        }
        if (err != nil) {
            t.Errorf("write: %v", err);
        }
        nint nnΔ1 = ᐸꟷ(c);
        if (nnΔ1 != n) {
            t.Errorf("wrote %d, read got %d", n, nnΔ1);
        }
    }
    w.Close();
    nint nn = ᐸꟷ(c);
    if (nn != 0) {
        t.Errorf("final read got %d", nn);
    }
}

partial struct pipeReturn {
    internal nint n;
    internal error err;
}

// Test a large write that requires multiple reads to satisfy.
internal static void writer(Δio.WriteCloser w, slice<byte> buf, channel<pipeReturn> c) {
    var (n, err) = w.Write(buf);
    w.Close();
    c.ᐸꟷ(new pipeReturn(n, err));
}

public static partial void TestPipe3(ж<testing.T> t) {
    var c = new channel<pipeReturn>(0);
    var (r, w) = Pipe();
    slice<byte> wdat = new slice<byte>(128);
    for (nint i = 0; i < len(wdat); i++) {
        wdat[i] = (byte)i;
    }
    goǃ(writer, new Δio.PipeWriterжWriteCloser(w), wdat, c);
    slice<byte> rdat = new slice<byte>(1024);
    nint tot = 0;
    for (nint n = 1; n <= 256; n *= 2) {
        var (nn, err) = r.Read(rdat.slice(tot, tot + n));
        if (err != nil && !AreEqual(err, EOF)) {
            t.Fatalf("read: %v", err);
        }

        // only final two reads should be short - 1 byte, then 0
        nint expect = n;
        if (n == 128) {
            expect = 1;
        } else if (n == 256) {
            expect = 0;
            if (!AreEqual(err, EOF)) {
                t.Fatalf("read at end: %v", err);
            }
        }
        if (nn != expect) {
            t.Fatalf("read %d, expected %d, got %d", n, expect, nn);
        }
        tot += nn;
    }
    var pr = ᐸꟷ(c);
    if (pr.n != 128 || pr.err != nil) {
        t.Fatalf("write 128: %d, %v", pr.n, pr.err);
    }
    if (tot != 128) {
        t.Fatalf("total read %d != 128", tot);
    }
    for (nint i = 0; i < 128; i++) {
        if (rdat[i] != (byte)i) {
            t.Fatalf("rdat[%d] = %d", i, rdat[i]);
        }
    }
}

// Test read after/before writer close.
partial interface closer :
    Δio.Closer
{
    error CloseWithError(error _);
}

partial struct pipeTest {
    internal bool async;
    internal error err;
    internal bool closeWithError;
}

internal static @string String(this pipeTest p) {
    return fmt.Sprintf("async=%v err=%v closeWithError=%v"u8, p.async, p.err, p.closeWithError);
}

internal static slice<pipeTest> pipeTests = slice<pipeTest>([
    new(true, default, false),
    new(true, default, true),
    new(true, ErrShortWrite, true),
    new(false, default, false),
    new(false, default, true),
    new(false, ErrShortWrite, true)
]);

internal static void delayClose(ж<testing.T> t, closer cl, channel<nint> ch, pipeTest tt) {
    time.Sleep(1 * time.Millisecond);
    error err = default;
    if (tt.closeWithError) {
        err = cl.CloseWithError(tt.err);
    } else {
        err = cl.Close();
    }
    if (err != nil) {
        t.Errorf("delayClose: %v", err);
    }
    ch.ᐸꟷ(0);
}

public static partial void TestPipeReadClose(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in pipeTests) {
        ref var tt = ref heap(new pipeTest(), out var Ꮡtt);
        tt = vᴛ1;

        var c = new channel<nint>(1);
        var (r, w) = Pipe();
        if (tt.async) {
            goǃ(delayClose, t, new io_test_package.io_PipeWriterжcloser(w), c, tt);
        } else {
            delayClose(t, new io_test_package.io_PipeWriterжcloser(w), c, tt);
        }
        slice<byte> buf = new slice<byte>(64);
        var (n, err) = r.Read(buf);
        ᐸꟷ(c);
        var want = tt.err;
        if (want == nil) {
            want = EOF;
        }
        if (!AreEqual(err, want)) {
            t.Errorf("read from closed pipe: %v want %v", err, want);
        }
        if (n != 0) {
            t.Errorf("read on closed pipe returned %d", n);
        }
        {
            err = r.Close(); if (err != nil) {
                t.Errorf("r.Close: %v", err);
            }
        }
    }
}

// Test close on Read side during Read.
public static partial void TestPipeReadClose2(ж<testing.T> t) {
    var c = new channel<nint>(1);
    var (r, _) = Pipe();
    goǃ(delayClose, t, new io_test_package.io_PipeReaderжcloser(r), c, new pipeTest());
    var (n, err) = r.Read(new slice<byte>(64));
    ᐸꟷ(c);
    if (n != 0 || !AreEqual(err, ErrClosedPipe)) {
        t.Errorf("read from closed pipe: %v, %v want %v, %v", n, err, (nint)0, ErrClosedPipe);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloWorldˢ3 => ref helloWorldˢ3ᵇ;
private static readonly @string helloWorldˢ3ᵇ = "hello, world"u8;

// Test write after/before reader close.
public static partial void TestPipeWriteClose(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in pipeTests) {
        ref var tt = ref heap(new pipeTest(), out var Ꮡtt);
        tt = vᴛ1;

        var c = new channel<nint>(1);
        var (r, w) = Pipe();
        if (tt.async) {
            goǃ(delayClose, t, new io_test_package.io_PipeReaderжcloser(r), c, tt);
        } else {
            delayClose(t, new io_test_package.io_PipeReaderжcloser(r), c, tt);
        }
        var (n, err) = WriteString(new Δio.PipeWriterжWriter(w), helloWorldˢ3);
        ᐸꟷ(c);
        var expect = tt.err;
        if (expect == nil) {
            expect = ErrClosedPipe;
        }
        if (!AreEqual(err, expect)) {
            t.Errorf("write on closed pipe: %v want %v", err, expect);
        }
        if (n != 0) {
            t.Errorf("write on closed pipe returned %d", n);
        }
        {
            err = w.Close(); if (err != nil) {
                t.Errorf("w.Close: %v", err);
            }
        }
    }
}

// Test close on Write side during Write.
public static partial void TestPipeWriteClose2(ж<testing.T> t) {
    var c = new channel<nint>(1);
    var (_, w) = Pipe();
    goǃ(delayClose, t, new io_test_package.io_PipeWriterжcloser(w), c, new pipeTest());
    var (n, err) = w.Write(new slice<byte>(64));
    ᐸꟷ(c);
    if (n != 0 || !AreEqual(err, ErrClosedPipe)) {
        t.Errorf("write to closed pipe: %v, %v want %v, %v", n, err, (nint)0, ErrClosedPipe);
    }
}

public static partial void TestWriteEmpty(ж<testing.T> t) {
    var (r, w) = Pipe();
    var wʗ1 = w;
    goǃ(() => {
        wʗ1.Write(slice<byte>([]));
        wʗ1.Close();
    });
    array<byte> b = new(2);
    ReadFull(new Δio.PipeReaderжReader(r), b[0..2]);
    r.Close();
}

public static partial void TestWriteNil(ж<testing.T> t) {
    var (r, w) = Pipe();
    var wʗ1 = w;
    goǃ(() => {
        wʗ1.Write(default);
        wʗ1.Close();
    });
    array<byte> b = new(2);
    ReadFull(new Δio.PipeReaderжReader(r), b[0..2]);
    r.Close();
}

public static partial void TestWriteAfterWriterClose(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var (r, w) = Pipe();
        var rʗ1 = r;
        defer(() => rʗ1.Close(), ref ᒐ);
        var done = new channel<bool>(0);
        ref var writeErr = ref heap<error>(out var ᏑwriteErr);
        var doneʗ1 = done;
        var wʗ1 = w;
        goǃ(() => {
            var (_, errΔ1) = wʗ1.Write(slice<byte>("hello"u8));
            if (errΔ1 != nil) {
                t.Errorf("got error: %q; expected none", errΔ1);
            }
            wʗ1.Close();
            (_, ᏑwriteErr.ValueSlot) = wʗ1.Write(slice<byte>("world"u8));
            doneʗ1.ᐸꟷ(true);
        });

        var buf = new slice<byte>(100);
        @string result = default;
        var (n, err) = ReadFull(new Δio.PipeReaderжReader(r), buf);
        if (err != nil && !AreEqual(err, ErrUnexpectedEOF)) {
            t.Fatalf("got: %q; want: %q", err, ErrUnexpectedEOF);
        }
        result = (@string)(buf.slice(0, n));
        ᐸꟷ(done);

        if (result != "hello"u8) {
            t.Errorf("got: %q; want: %q", result, helloˢ);
        }
        if (!AreEqual(writeErr, ErrClosedPipe)) {
            t.Errorf("got: %q; want: %q", writeErr, ErrClosedPipe);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestPipeCloseError_testError1 /*dyn*/ {
    /*embed*/ internal error error;
}

internal partial struct TestPipeCloseError_testError2 /*dyn*/ {
    /*embed*/ internal error error;
}

public static void TestPipeCloseError(ж<testing.T> t) {
    var (r, w) = Pipe();
    r.CloseWithError(new TestPipeCloseError_testError1());
    {
        var (_, err) = w.Write(default); if (!AreEqual(err, (new TestPipeCloseError_testError1()))) {
            t.Errorf("Write error: got %T, want testError1", err);
        }
    }
    r.CloseWithError(new TestPipeCloseError_testError2());
    {
        var (_, err) = w.Write(default); if (!AreEqual(err, (new TestPipeCloseError_testError1()))) {
            t.Errorf("Write error: got %T, want testError1", err);
        }
    }

    (r, w) = Pipe();
    w.CloseWithError(new TestPipeCloseError_testError1());
    {
        var (_, err) = r.Read(default); if (!AreEqual(err, (new TestPipeCloseError_testError1()))) {
            t.Errorf("Read error: got %T, want testError1", err);
        }
    }
    w.CloseWithError(new TestPipeCloseError_testError2());
    {
        var (_, err) = r.Read(default); if (!AreEqual(err, (new TestPipeCloseError_testError1()))) {
            t.Errorf("Read error: got %T, want testError1", err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string readˢ => ref readˢᵇ;
private static readonly @string readˢᵇ = "Read"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string inputᶜ => ref inputᶜᵇ;
private static readonly @string inputᶜᵇ = "0123456789abcdef"u8;

public static void TestPipeConcurrent(ж<testing.T> t) {
    @string input = inputᶜ;
    UntypedInt count = 8;
    UntypedInt readSize = 2;

    t.Run(writeˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
        var (r, w) = Pipe();

        for (nint i = 0; i < count; i++) {
            var wʗ1 = w;
            goǃ(() => {
                time.Sleep(time.Millisecond); // Increase probability of race
                {
                    var (n, err) = wʗ1.Write(slice<byte>(input)); if (n != len(input) || err != nil) {
                        tΔ1.Errorf("Write() = (%d, %v); want (%d, nil)", n, err, len(input));
                    }
                }
            });
        }

        var buf = new slice<byte>((nint)count * len(input));
        for (nint i = 0; i < len(buf); i += readSize) {
            {
                var (n, err) = r.Read(buf.slice(i, i + (nint)readSize)); if (n != readSize || err != nil) {
                    tΔ1.Errorf("Read() = (%d, %v); want (%d, nil)", n, err, (nint)(readSize));
                }
            }
        }

        // Since each Write is fully gated, if multiple Read calls were needed,
        // the contents of Write should still appear together in the output.
        @string got = (@string)buf;
        @string want = strings.Repeat(input, count);
        if (got != want) {
            tΔ1.Errorf("got: %q; want: %q", got, want);
        }
    });

    t.Run(readˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ2) => {
        var (r, w) = Pipe();

        var c = new channel<slice<byte>>((nint)((nint)count * len(input)) / readSize);
        for (nint i = 0; i < cap(c); i++) {
            var cʗ1 = c;
            var rʗ1 = r;
            goǃ(() => {
                time.Sleep(time.Millisecond); // Increase probability of race
                var buf = new slice<byte>(readSize);
                {
                    var (n, err) = rʗ1.Read(buf); if (n != readSize || err != nil) {
                        tΔ2.Errorf("Read() = (%d, %v); want (%d, nil)", n, err, (nint)(readSize));
                    }
                }
                cʗ1.ᐸꟷ(buf);
            });
        }

        for (nint i = 0; i < count; i++) {
            {
                var (n, err) = w.Write(slice<byte>(input)); if (n != len(input) || err != nil) {
                    tΔ2.Errorf("Write() = (%d, %v); want (%d, nil)", n, err, len(input));
                }
            }
        }

        // Since each read is independent, the only guarantee about the output
        // is that it is a permutation of the input in readSized groups.
        var got = new slice<byte>(0, (nint)count * len(input));
        for (nint i = 0; i < cap(c); i++) {
            got = appendꓸꓸꓸ(got, (ᐸꟷ(c)));
        }
        got = sortBytesInGroups(got, readSize);
        var want = bytes.Repeat(slice<byte>(input), count);
        want = sortBytesInGroups(want, readSize);
        if (((sstring)got) != ((sstring)want)) {
            tΔ2.Errorf("got: %q; want: %q", got, want);
        }
    });
}

internal static slice<byte> sortBytesInGroups(slice<byte> b, nint n) {
    var groupsᴷ = new stackAppend<slice<byte>>();

    slice<slice<byte>> groups = default;
    while (len(b) > 0) {
        groups = append(groupsᴷ, groups, b.slice(0, n));
        b = b.slice(n);
    }
    slices.SortFunc<slice<slice<byte>>, slice<byte>>(groups, bytes.Compare);
    return bytes.Join(groups, default);
}

internal static ж<Δio.PipeReader> rSink;
internal static ж<Δio.PipeWriter> wSink;

public static void TestPipeAllocations(ж<testing.T> t) {
    var numAllocs = testing.AllocsPerRun(10, () => {
        (rSink, wSink) = Pipe();
    });

    // go.dev/cl/473535 claimed Pipe() should only do 2 allocations,
    // plus the 2 escaping to heap for simulating real world usages.
    nint expectedAllocs = 4;
    if ((nint)numAllocs > expectedAllocs) {
        t.Fatalf("too many allocations for io.Pipe() call: %f", numAllocs);
    }
}

} // end io_test_package
