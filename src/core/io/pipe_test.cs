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
using static go.io_internal_test_package;
using Δio = io_package;

partial class io_test_package {

internal static void checkWrite(ж<testing.T> Ꮡt, Δio.Writer w, slice<byte> data, channel<nint> c) {
    var (n, err) = w.Write(data);
    if (err != default!) {
        Ꮡt.Errorf("write: %v"u8, err);
    }
    if (n != len(data)) {
        Ꮡt.Errorf("short write: %d != %d"u8, n, len(data));
    }
    c.ᐸꟷ(0);
}

// Test a single read/write pair.
public static void TestPipe1(ж<testing.T> Ꮡt) {
    var c = new channel<nint>(0);
    var (r, w) = Pipe();
    slice<byte> buf = new slice<byte>(64);
    goǃ(checkWrite, Ꮡt, new Δio.PipeWriterжWriter(w), slice<byte>("hello, world"u8), c);
    var (n, err) = r.Read(buf);
    if (err != default!){
        Ꮡt.Errorf("read: %v"u8, err);
    } else 
    if (n != 12 || ((sstring)(buf[0..12])) != "hello, world"u8) {
        Ꮡt.Errorf("bad read: got %q"u8, buf[0..(int)(n)]);
    }
    ᐸꟷ(c);
    r.Close();
    w.Close();
}

internal static void reader(ж<testing.T> Ꮡt, Δio.Reader r, channel<nint> c) {
    slice<byte> buf = new slice<byte>(64);
    while (ᐧ) {
        var (n, err) = r.Read(buf);
        if (AreEqual(err, EOF)) {
            c.ᐸꟷ(0);
            break;
        }
        if (err != default!) {
            Ꮡt.Errorf("read: %v"u8, err);
        }
        c.ᐸꟷ(n);
    }
}

// Test a sequence of read/write pairs.
public static void TestPipe2(ж<testing.T> Ꮡt) {
    var c = new channel<nint>(0);
    var (r, w) = Pipe();
    goǃ(reader, Ꮡt, new Δio.PipeReaderжReader(r), c);
    slice<byte> buf = new slice<byte>(64);
    for (nint i = 0; i < 5; i++) {
        var p = buf[0..(int)(5 + i * 10)];
        var (n, err) = w.Write(p);
        if (n != len(p)) {
            Ꮡt.Errorf("wrote %d, got %d"u8, len(p), n);
        }
        if (err != default!) {
            Ꮡt.Errorf("write: %v"u8, err);
        }
        nint nnΔ1 = ᐸꟷ(c);
        if (nnΔ1 != n) {
            Ꮡt.Errorf("wrote %d, read got %d"u8, n, nnΔ1);
        }
    }
    w.Close();
    nint nn = ᐸꟷ(c);
    if (nn != 0) {
        Ꮡt.Errorf("final read got %d"u8, nn);
    }
}

[GoType] partial struct pipeReturn {
    internal nint n;
    internal error err;
}

// Test a large write that requires multiple reads to satisfy.
internal static void writer(Δio.WriteCloser w, slice<byte> buf, channel<pipeReturn> c) {
    var (n, err) = w.Write(buf);
    w.Close();
    c.ᐸꟷ(new pipeReturn(n, err));
}

public static void TestPipe3(ж<testing.T> Ꮡt) {
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
        var (nn, err) = r.Read(rdat[(int)(tot)..(int)(tot + n)]);
        if (err != default! && !AreEqual(err, EOF)) {
            Ꮡt.Fatalf("read: %v"u8, err);
        }
        // only final two reads should be short - 1 byte, then 0
        nint expect = n;
        if (n == 128){
            expect = 1;
        } else 
        if (n == 256) {
            expect = 0;
            if (!AreEqual(err, EOF)) {
                Ꮡt.Fatalf("read at end: %v"u8, err);
            }
        }
        if (nn != expect) {
            Ꮡt.Fatalf("read %d, expected %d, got %d"u8, n, expect, nn);
        }
        tot += nn;
    }
    var pr = ᐸꟷ(c);
    if (pr.n != 128 || pr.err != default!) {
        Ꮡt.Fatalf("write 128: %d, %v"u8, pr.n, pr.err);
    }
    if (tot != 128) {
        Ꮡt.Fatalf("total read %d != 128"u8, tot);
    }
    for (nint i = 0; i < 128; i++) {
        if (rdat[i] != (byte)i) {
            Ꮡt.Fatalf("rdat[%d] = %d"u8, i, rdat[i]);
        }
    }
}

// Test read after/before writer close.
[GoType] partial interface closer :
    Δio.Closer
{
    error CloseWithError(error _);
}

[GoType] partial struct pipeTest {
    internal bool async;
    internal error err;
    internal bool closeWithError;
}

internal static @string String(this pipeTest p) {
    return fmt.Sprintf("async=%v err=%v closeWithError=%v"u8, p.async, p.err, p.closeWithError);
}

internal static slice<pipeTest> pipeTests = new pipeTest[]{
    new(true, default!, false),
    new(true, default!, true),
    new(true, ErrShortWrite, true),
    new(false, default!, false),
    new(false, default!, true),
    new(false, ErrShortWrite, true)
}.slice();

internal static void delayClose(ж<testing.T> Ꮡt, closer cl, channel<nint> ch, pipeTest tt) {
    time.Sleep(1 * time.Millisecond);
    error err = default!;
    if (tt.closeWithError){
        err = cl.CloseWithError(tt.err);
    } else {
        err = cl.Close();
    }
    if (err != default!) {
        Ꮡt.Errorf("delayClose: %v"u8, err);
    }
    ch.ᐸꟷ(0);
}

public static void TestPipeReadClose(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in pipeTests) {
        ref var tt = ref heap(new pipeTest(), out var Ꮡtt);
        tt = vᴛ1;

        var c = new channel<nint>(1);
        var (r, w) = Pipe();
        if (tt.async){
            goǃ(delayClose, Ꮡt, new io_test_package.io_PipeWriterжcloser(w), c, tt);
        } else {
            delayClose(Ꮡt, new io_test_package.io_PipeWriterжcloser(w), c, tt);
        }
        slice<byte> buf = new slice<byte>(64);
        var (n, err) = r.Read(buf);
        ᐸꟷ(c);
        var want = tt.err;
        if (want == default!) {
            want = EOF;
        }
        if (!AreEqual(err, want)) {
            Ꮡt.Errorf("read from closed pipe: %v want %v"u8, err, want);
        }
        if (n != 0) {
            Ꮡt.Errorf("read on closed pipe returned %d"u8, n);
        }
        {
            err = r.Close(); if (err != default!) {
                Ꮡt.Errorf("r.Close: %v"u8, err);
            }
        }
    }
}

// Test close on Read side during Read.
public static void TestPipeReadClose2(ж<testing.T> Ꮡt) {
    var c = new channel<nint>(1);
    var (r, _) = Pipe();
    goǃ(delayClose, Ꮡt, new io_test_package.io_PipeReaderжcloser(r), c, new pipeTest(nil));
    var (n, err) = r.Read(new slice<byte>(64));
    ᐸꟷ(c);
    if (n != 0 || !AreEqual(err, ErrClosedPipe)) {
        Ꮡt.Errorf("read from closed pipe: %v, %v want %v, %v"u8, n, err, (nint)(0), ErrClosedPipe);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloWorldˢ3 = "hello, world"u8;

// Test write after/before reader close.
public static void TestPipeWriteClose(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in pipeTests) {
        ref var tt = ref heap(new pipeTest(), out var Ꮡtt);
        tt = vᴛ1;

        var c = new channel<nint>(1);
        var (r, w) = Pipe();
        if (tt.async){
            goǃ(delayClose, Ꮡt, new io_test_package.io_PipeReaderжcloser(r), c, tt);
        } else {
            delayClose(Ꮡt, new io_test_package.io_PipeReaderжcloser(r), c, tt);
        }
        var (n, err) = WriteString(new Δio.PipeWriterжWriter(w), helloWorldˢ3);
        ᐸꟷ(c);
        var expect = tt.err;
        if (expect == default!) {
            expect = ErrClosedPipe;
        }
        if (!AreEqual(err, expect)) {
            Ꮡt.Errorf("write on closed pipe: %v want %v"u8, err, expect);
        }
        if (n != 0) {
            Ꮡt.Errorf("write on closed pipe returned %d"u8, n);
        }
        {
            err = w.Close(); if (err != default!) {
                Ꮡt.Errorf("w.Close: %v"u8, err);
            }
        }
    }
}

// Test close on Write side during Write.
public static void TestPipeWriteClose2(ж<testing.T> Ꮡt) {
    var c = new channel<nint>(1);
    var (_, w) = Pipe();
    goǃ(delayClose, Ꮡt, new io_test_package.io_PipeWriterжcloser(w), c, new pipeTest(nil));
    var (n, err) = w.Write(new slice<byte>(64));
    ᐸꟷ(c);
    if (n != 0 || !AreEqual(err, ErrClosedPipe)) {
        Ꮡt.Errorf("write to closed pipe: %v, %v want %v, %v"u8, n, err, (nint)(0), ErrClosedPipe);
    }
}

public static void TestWriteEmpty(ж<testing.T> Ꮡt) {
    var (r, w) = Pipe();
    var wʗ1 = w;
    goǃ(() => {
        wʗ1.Write(new byte[]{}.slice());
        wʗ1.Close();
    });
    array<byte> b = new(2);
    ReadFull(new Δio.PipeReaderжReader(r), b[0..2]);
    r.Close();
}

public static void TestWriteNil(ж<testing.T> Ꮡt) {
    var (r, w) = Pipe();
    var wʗ1 = w;
    goǃ(() => {
        wʗ1.Write(default!);
        wʗ1.Close();
    });
    array<byte> b = new(2);
    ReadFull(new Δio.PipeReaderжReader(r), b[0..2]);
    r.Close();
}

public static void TestWriteAfterWriterClose(ж<testing.T> Ꮡt) {
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
            if (errΔ1 != default!) {
                Ꮡt.Errorf("got error: %q; expected none"u8, errΔ1);
            }
            wʗ1.Close();
            (_, ᏑwriteErr.ValueSlot) = wʗ1.Write(slice<byte>("world"u8));
            doneʗ1.ᐸꟷ(true);
        });
        var buf = new slice<byte>(100);
        @string result = default!;
        var (n, err) = ReadFull(new Δio.PipeReaderжReader(r), buf);
        if (err != default! && !AreEqual(err, ErrUnexpectedEOF)) {
            Ꮡt.Fatalf("got: %q; want: %q"u8, err, ErrUnexpectedEOF);
        }
        result = ((@string)(buf[0..(int)(n)]));
        ᐸꟷ(done);
        if (result != "hello"u8) {
            Ꮡt.Errorf("got: %q; want: %q"u8, result, helloˢ);
        }
        if (!AreEqual(writeErr, ErrClosedPipe)) {
            Ꮡt.Errorf("got: %q; want: %q"u8, writeErr, ErrClosedPipe);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("dyn")] internal partial struct TestPipeCloseError_testError1 {
    [GoEmbedded] internal error error;
}

[GoType("dyn")] internal partial struct TestPipeCloseError_testError2 {
    [GoEmbedded] internal error error;
}

public static void TestPipeCloseError(ж<testing.T> Ꮡt) {
    var (r, w) = Pipe();
    r.CloseWithError(new TestPipeCloseError_testError1(nil));
    {
        var (_, err) = w.Write(default!); if (!AreEqual(err, (new TestPipeCloseError_testError1(nil)))) {
            Ꮡt.Errorf("Write error: got %T, want testError1"u8, err);
        }
    }
    r.CloseWithError(new TestPipeCloseError_testError2(nil));
    {
        var (_, err) = w.Write(default!); if (!AreEqual(err, (new TestPipeCloseError_testError1(nil)))) {
            Ꮡt.Errorf("Write error: got %T, want testError1"u8, err);
        }
    }
    (r, w) = Pipe();
    w.CloseWithError(new TestPipeCloseError_testError1(nil));
    {
        var (_, err) = r.Read(default!); if (!AreEqual(err, (new TestPipeCloseError_testError1(nil)))) {
            Ꮡt.Errorf("Read error: got %T, want testError1"u8, err);
        }
    }
    w.CloseWithError(new TestPipeCloseError_testError2(nil));
    {
        var (_, err) = r.Read(default!); if (!AreEqual(err, (new TestPipeCloseError_testError1(nil)))) {
            Ꮡt.Errorf("Read error: got %T, want testError1"u8, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string readˢ = "Read"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string inputᶜ = "0123456789abcdef"u8;

public static void TestPipeConcurrent(ж<testing.T> Ꮡt) {
    @string input = inputᶜ;
    UntypedInt count = 8;
    UntypedInt readSize = 2;
    Ꮡt.Run(writeˢ, (ж<testing.T> tΔ1) => {
        var (r, w) = Pipe();
        for (nint i = 0; i < count; i++) {
            var wʗ1 = w;
            goǃ(() => {
                time.Sleep(time.Millisecond); // Increase probability of race
                {
                    var (n, err) = wʗ1.Write(slice<byte>(input)); if (n != len(input) || err != default!) {
                        tΔ1.Errorf("Write() = (%d, %v); want (%d, nil)"u8, n, err, len(input));
                    }
                }
            });
        }
        var buf = new slice<byte>((nint)count * len(input));
        for (nint i = 0; i < len(buf); i += readSize) {
            {
                var (n, err) = r.Read(buf[(int)(i)..(int)(i + (nint)readSize)]); if (n != readSize || err != default!) {
                    tΔ1.Errorf("Read() = (%d, %v); want (%d, nil)"u8, n, err, (nint)(readSize));
                }
            }
        }
        // Since each Write is fully gated, if multiple Read calls were needed,
        // the contents of Write should still appear together in the output.
        @string got = ((@string)buf);
        @string want = strings.Repeat(input, count);
        if (got != want) {
            tΔ1.Errorf("got: %q; want: %q"u8, got, want);
        }
    });
    Ꮡt.Run(readˢ, (ж<testing.T> tΔ2) => {
        var (r, w) = Pipe();
        var c = new channel<slice<byte>>((nint)((nint)count * len(input)) / readSize);
        for (nint i = 0; i < cap(c); i++) {
            var cʗ1 = c;
            var rʗ1 = r;
            goǃ(() => {
                time.Sleep(time.Millisecond); // Increase probability of race
                var buf = new slice<byte>(readSize);
                {
                    var (n, err) = rʗ1.Read(buf); if (n != readSize || err != default!) {
                        tΔ2.Errorf("Read() = (%d, %v); want (%d, nil)"u8, n, err, (nint)(readSize));
                    }
                }
                cʗ1.ᐸꟷ(buf);
            });
        }
        for (nint i = 0; i < count; i++) {
            {
                var (n, err) = w.Write(slice<byte>(input)); if (n != len(input) || err != default!) {
                    tΔ2.Errorf("Write() = (%d, %v); want (%d, nil)"u8, n, err, len(input));
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
            tΔ2.Errorf("got: %q; want: %q"u8, got, want);
        }
    });
}

internal static slice<byte> sortBytesInGroups(slice<byte> b, nint n) {
    slice<slice<byte>> groups = default!;
    while (len(b) > 0) {
        groups = append(groups, b[..(int)(n)]);
        b = b[(int)(n)..];
    }
    slices.SortFunc<slice<slice<byte>>, slice<byte>>(groups, bytes.Compare);
    return bytes.Join(groups, default!);
}

internal static ж<Δio.PipeReader> rSink;
internal static ж<Δio.PipeWriter> wSink;

public static void TestPipeAllocations(ж<testing.T> Ꮡt) {
    var numAllocs = testing.AllocsPerRun(10, () => {
        (rSink, wSink) = Pipe();
    });
    // go.dev/cl/473535 claimed Pipe() should only do 2 allocations,
    // plus the 2 escaping to heap for simulating real world usages.
    nint expectedAllocs = 4;
    if ((nint)numAllocs > expectedAllocs) {
        Ꮡt.Fatalf("too many allocations for io.Pipe() call: %f"u8, numAllocs);
    }
}

} // end io_test_package
